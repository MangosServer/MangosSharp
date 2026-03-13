//
// Copyright (C) 2013-2025 getMaNGOS <https://www.getmangos.eu>
//
// This program is free software. You can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation. either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY. Without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//

using System;
using System.IO;

namespace Mangos.World.Maps.VMap;

/// <summary>
/// Flags for model spawn instances, matching MangosZero's ModelFlags enum.
/// </summary>
[Flags]
public enum ModelFlags : uint
{
    MOD_M2 = 1,
    MOD_WORLDSPAWN = 2,
    MOD_HAS_BOUND = 4,
}

/// <summary>
/// Model spawn information read from .vmtree files.
/// Port of MangosZero's ModelSpawn.
/// </summary>
public class ModelSpawn
{
    public ModelFlags Flags;
    public ushort AdtId;
    public uint UniqueId;
    public Vector3 Position;
    public float Orientation;     // iRot
    public AABox Bound;
    public string Name;

    public static ModelSpawn Read(BinaryReader reader)
    {
        var spawn = new ModelSpawn();
        spawn.Flags = (ModelFlags)reader.ReadUInt32();
        spawn.AdtId = reader.ReadUInt16();
        spawn.UniqueId = reader.ReadUInt32();
        spawn.Position = Vector3.Read(reader);
        spawn.Orientation = reader.ReadSingle();

        if ((spawn.Flags & ModelFlags.MOD_HAS_BOUND) != 0)
        {
            spawn.Bound = AABox.Read(reader);
        }

        // Read name
        var nameLength = reader.ReadInt32();
        if (nameLength > 0 && nameLength < 4096) // sanity check
        {
            var nameBytes = reader.ReadBytes(nameLength);
            spawn.Name = System.Text.Encoding.ASCII.GetString(nameBytes).TrimEnd('\0');
        }
        else
        {
            spawn.Name = "";
        }

        return spawn;
    }
}

/// <summary>
/// A placed instance of a model in the world.
/// Contains the model spawn data and a reference to the loaded WorldModel.
/// Port of MangosZero's ModelInstance.
/// </summary>
public class ModelInstance
{
    private ModelSpawn m_spawn;
    private WorldModel m_model;
    private float m_invScale;

    // Transform matrices for converting world<->model coordinates
    private float m_sinAngle;
    private float m_cosAngle;

    public ModelSpawn Spawn => m_spawn;
    public WorldModel Model => m_model;
    public AABox Bounds => m_spawn.Bound;
    public string ModelName => m_spawn.Name;

    public ModelInstance(ModelSpawn spawn, WorldModel model)
    {
        m_spawn = spawn;
        m_model = model;
        m_invScale = 1.0f;

        // Pre-compute rotation
        m_sinAngle = MathF.Sin(spawn.Orientation * MathF.PI / 180.0f);
        m_cosAngle = MathF.Cos(spawn.Orientation * MathF.PI / 180.0f);
    }

    /// <summary>
    /// Transforms a world-space position into model-local space.
    /// </summary>
    public Vector3 WorldToModel(Vector3 worldPos)
    {
        var offset = worldPos - m_spawn.Position;
        // Apply inverse rotation (Y-axis rotation)
        return new Vector3(
            offset.X * m_cosAngle + offset.Z * m_sinAngle,
            offset.Y,
            -offset.X * m_sinAngle + offset.Z * m_cosAngle
        );
    }

    /// <summary>
    /// Transforms a model-space direction into world space.
    /// </summary>
    public Vector3 ModelToWorldDir(Vector3 modelDir)
    {
        return new Vector3(
            modelDir.X * m_cosAngle - modelDir.Z * m_sinAngle,
            modelDir.Y,
            modelDir.X * m_sinAngle + modelDir.Z * m_cosAngle
        );
    }

    /// <summary>
    /// Tests ray intersection against this model instance in world space.
    /// Transforms the ray to model space, tests, and transforms back.
    /// </summary>
    public bool IntersectRay(Ray worldRay, ref float distance, bool stopAtFirstHit)
    {
        if (m_model == null)
            return false;

        // Check bounds first
        if (!m_spawn.Bound.IntersectRay(worldRay.Origin, worldRay.Direction, out _, out _))
            return false;

        // Transform ray to model space
        var modelOrigin = WorldToModel(worldRay.Origin);
        var modelDir = WorldToModel(worldRay.Origin + worldRay.Direction) - modelOrigin;

        // Normalize
        float dirLen = modelDir.Length;
        if (dirLen < 1e-10f)
            return false;
        modelDir = modelDir * (1.0f / dirLen);

        var modelRay = new Ray(modelOrigin, modelDir);
        float modelDist = distance * dirLen;

        if (m_model.IntersectRay(modelRay, ref modelDist, stopAtFirstHit))
        {
            distance = modelDist / dirLen;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the model height at the given world position by casting a downward ray.
    /// </summary>
    public bool GetHeight(Vector3 worldPos, float maxDist, out float height)
    {
        height = float.NegativeInfinity;
        if (m_model == null)
            return false;

        // Transform position to model space
        var modelPos = WorldToModel(worldPos);
        if (m_model.GetHeight(modelPos, maxDist, out float modelHeight))
        {
            // Model height is in model space Y, add spawn position Y
            height = modelHeight + m_spawn.Position.Y - modelPos.Y + worldPos.Y;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gets the liquid level at the given world position.
    /// </summary>
    public bool GetLiquidLevel(Vector3 worldPos, out float level)
    {
        level = float.NegativeInfinity;
        if (m_model == null)
            return false;

        var modelPos = WorldToModel(worldPos);
        return m_model.GetLiquidLevel(modelPos, out level);
    }
}
