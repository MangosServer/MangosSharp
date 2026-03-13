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
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mangos.World.Maps.VMap;

/// <summary>
/// Per-map spatial tree for VMap model instances.
/// Loaded from {mapId:D4}.vmtree files produced by the MangosZero extractor.
/// Port of MangosZero's StaticMapTree.
/// </summary>
public class StaticMapTree
{
    private const string VMAP_MAGIC = "VMAP004";

    private uint m_mapId;
    private BIH m_tree;
    private ModelSpawn[] m_spawns;
    private ModelInstance[] m_instances;
    private Dictionary<string, WorldModel> m_modelCache;
    private string m_basePath;
    private HashSet<uint> m_loadedTiles;

    public uint MapId => m_mapId;
    public bool IsLoaded => m_tree != null;

    public StaticMapTree(uint mapId, string basePath)
    {
        m_mapId = mapId;
        m_basePath = basePath;
        m_modelCache = new Dictionary<string, WorldModel>(StringComparer.OrdinalIgnoreCase);
        m_loadedTiles = new HashSet<uint>();
    }

    /// <summary>
    /// Loads the .vmtree file for this map.
    /// This reads the BIH tree structure and model spawn list.
    /// </summary>
    public bool InitMap()
    {
        var filename = Path.Combine(m_basePath, $"{m_mapId:D4}.vmtree");
        if (!File.Exists(filename))
            return false;

        try
        {
            using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new BinaryReader(fs);

            // Read and verify magic
            var magic = Encoding.ASCII.GetString(reader.ReadBytes(8));
            if (!magic.StartsWith(VMAP_MAGIC))
                return false;

            // Read chunk: "NODE"
            var chunkId = Encoding.ASCII.GetString(reader.ReadBytes(4));
            if (chunkId != "NODE")
                return false;

            // Read BIH tree
            m_tree = new BIH();
            if (!m_tree.ReadFromFile(reader))
                return false;

            // Read model spawns: "SIDX"
            if (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                chunkId = Encoding.ASCII.GetString(reader.ReadBytes(4));
                if (chunkId == "SIDX")
                {
                    var spawnCount = reader.ReadUInt32();
                    m_spawns = new ModelSpawn[spawnCount];
                    m_instances = new ModelInstance[spawnCount];

                    for (int i = 0; i < spawnCount; i++)
                    {
                        m_spawns[i] = ModelSpawn.Read(reader);
                    }
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Loads the models referenced by a specific tile.
    /// Called when a map tile is activated.
    /// </summary>
    public void LoadMapTile(byte tileX, byte tileY)
    {
        var tileId = PackTileId(tileX, tileY);
        if (m_loadedTiles.Contains(tileId))
            return;

        m_loadedTiles.Add(tileId);

        var filename = Path.Combine(m_basePath, $"{m_mapId:D4}{tileY:D2}{tileX:D2}.vmtile");
        if (!File.Exists(filename))
            return;

        try
        {
            using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new BinaryReader(fs);

            // Read magic
            var magic = Encoding.ASCII.GetString(reader.ReadBytes(8));
            if (!magic.StartsWith(VMAP_MAGIC))
                return;

            // Read spawn references for this tile
            var refCount = reader.ReadUInt32();
            for (int i = 0; i < refCount; i++)
            {
                var spawnIndex = reader.ReadUInt32();
                if (spawnIndex < m_spawns.Length && m_instances[spawnIndex] == null)
                {
                    var spawn = m_spawns[spawnIndex];
                    var model = LoadModel(spawn.Name);
                    if (model != null)
                    {
                        m_instances[spawnIndex] = new ModelInstance(spawn, model);
                    }
                }
            }
        }
        catch
        {
            // Log but don't fail
        }
    }

    /// <summary>
    /// Unloads models for a specific tile.
    /// </summary>
    public void UnloadMapTile(byte tileX, byte tileY)
    {
        var tileId = PackTileId(tileX, tileY);
        m_loadedTiles.Remove(tileId);
    }

    /// <summary>
    /// Loads a world model (.vmo file) from disk, using a cache.
    /// </summary>
    private WorldModel LoadModel(string modelName)
    {
        if (string.IsNullOrEmpty(modelName))
            return null;

        if (m_modelCache.TryGetValue(modelName, out var cached))
            return cached;

        var modelPath = Path.Combine(m_basePath, modelName);
        if (!File.Exists(modelPath))
        {
            m_modelCache[modelName] = null;
            return null;
        }

        var model = new WorldModel();
        if (model.ReadFile(modelPath))
        {
            m_modelCache[modelName] = model;
            return model;
        }

        m_modelCache[modelName] = null;
        return null;
    }

    /// <summary>
    /// Tests line-of-sight between two points by ray-casting against all loaded model instances.
    /// Returns true if there is a clear line of sight (no obstruction).
    /// </summary>
    public bool IsInLineOfSight(Vector3 pos1, Vector3 pos2)
    {
        if (m_instances == null)
            return true;

        var direction = pos2 - pos1;
        float maxDist = direction.Length;
        if (maxDist < 1e-5f)
            return true;

        direction = direction * (1.0f / maxDist);
        var ray = new Ray(pos1, direction);

        // Test against all loaded model instances
        for (int i = 0; i < m_instances.Length; i++)
        {
            if (m_instances[i] == null) continue;

            float dist = maxDist;
            if (m_instances[i].IntersectRay(ray, ref dist, true))
            {
                if (dist < maxDist)
                    return false; // obstruction found
            }
        }

        return true;
    }

    /// <summary>
    /// Gets the model height at the given position by casting downward rays
    /// against all loaded model instances.
    /// </summary>
    public float GetHeight(Vector3 pos, float maxSearchDist)
    {
        if (m_instances == null)
            return -200000.0f;

        float bestHeight = -200000.0f;
        bool found = false;

        for (int i = 0; i < m_instances.Length; i++)
        {
            if (m_instances[i] == null) continue;

            if (m_instances[i].GetHeight(pos, maxSearchDist, out float height))
            {
                if (!found || height > bestHeight)
                {
                    bestHeight = height;
                    found = true;
                }
            }
        }

        return bestHeight;
    }

    /// <summary>
    /// Gets the hit position along a ray between two points.
    /// Returns true if there is a hit, with the hit position output.
    /// </summary>
    public bool GetObjectHitPos(Vector3 pos1, Vector3 pos2, out Vector3 hitPos, float pModifyDist)
    {
        hitPos = pos2;
        if (m_instances == null)
            return false;

        var direction = pos2 - pos1;
        float maxDist = direction.Length;
        if (maxDist < 1e-5f)
            return false;

        direction = direction * (1.0f / maxDist);
        var ray = new Ray(pos1, direction);

        float closestDist = maxDist;
        bool hit = false;

        for (int i = 0; i < m_instances.Length; i++)
        {
            if (m_instances[i] == null) continue;

            float dist = closestDist;
            if (m_instances[i].IntersectRay(ray, ref dist, false))
            {
                if (dist < closestDist)
                {
                    closestDist = dist;
                    hit = true;
                }
            }
        }

        if (hit)
        {
            // Move the hit position back along the ray by pModifyDist
            float adjustedDist = MathF.Max(0, closestDist + pModifyDist);
            hitPos = pos1 + direction * adjustedDist;
            return true;
        }

        return false;
    }

    private static uint PackTileId(byte x, byte y) => (uint)(x << 8 | y);
}
