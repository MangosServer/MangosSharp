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

using Mangos.World.Maps.VMap;
using System;
using System.Collections.Generic;

namespace Mangos.World.Maps;

/// <summary>
/// Manages VMap (Virtual Map) data for line-of-sight, height queries, and collision detection.
/// Port of MangosZero's VMapManager2.
///
/// VMap data is produced by the MangosZero map extractor and consists of:
/// - .vmtree files: per-map BIH trees with model spawn information
/// - .vmtile files: per-tile references to model spawns
/// - .vmo files: individual model geometry (WMO/M2 collision meshes)
/// </summary>
public class VMapManager
{
    private const float INVALID_HEIGHT = -200000.0f;

    private string m_basePath;
    private Dictionary<uint, StaticMapTree> m_mapTrees;
    private bool m_initialized;

    public VMapManager()
    {
        m_mapTrees = new Dictionary<uint, StaticMapTree>();
    }

    /// <summary>
    /// Initializes the VMap manager with the base directory containing VMap data files.
    /// </summary>
    public void Initialize(string basePath)
    {
        m_basePath = basePath;
        m_initialized = true;
    }

    /// <summary>
    /// Loads the VMap tree for a specific map.
    /// This reads the .vmtree file and prepares the spatial index.
    /// </summary>
    public bool LoadMap(uint mapId)
    {
        if (!m_initialized)
            return false;

        if (m_mapTrees.ContainsKey(mapId))
            return true;

        var tree = new StaticMapTree(mapId, m_basePath);
        if (tree.InitMap())
        {
            m_mapTrees[mapId] = tree;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Unloads the VMap tree for a specific map.
    /// </summary>
    public void UnloadMap(uint mapId)
    {
        m_mapTrees.Remove(mapId);
    }

    /// <summary>
    /// Loads VMap data for a specific map tile.
    /// Should be called when a tile becomes active (player enters the area).
    /// </summary>
    public void LoadMapTile(uint mapId, byte tileX, byte tileY)
    {
        if (m_mapTrees.TryGetValue(mapId, out var tree))
        {
            tree.LoadMapTile(tileX, tileY);
        }
    }

    /// <summary>
    /// Unloads VMap data for a specific map tile.
    /// </summary>
    public void UnloadMapTile(uint mapId, byte tileX, byte tileY)
    {
        if (m_mapTrees.TryGetValue(mapId, out var tree))
        {
            tree.UnloadMapTile(tileX, tileY);
        }
    }

    /// <summary>
    /// Tests line-of-sight between two world positions.
    /// Returns true if there is a clear line of sight (no VMap obstruction).
    /// </summary>
    public bool IsInLineOfSight(uint mapId, float x1, float y1, float z1, float x2, float y2, float z2)
    {
        if (!m_mapTrees.TryGetValue(mapId, out var tree))
            return true; // No VMap data - assume clear

        // Convert from WoW coordinates to VMap internal coordinates
        var pos1 = Vector3.ConvertToVMapCoords(x1, y1, z1);
        var pos2 = Vector3.ConvertToVMapCoords(x2, y2, z2);

        return tree.IsInLineOfSight(pos1, pos2);
    }

    /// <summary>
    /// Gets the VMap model height at the given world position.
    /// Returns INVALID_HEIGHT if no model surface is found.
    /// </summary>
    public float GetHeight(uint mapId, float x, float y, float z)
    {
        if (!m_mapTrees.TryGetValue(mapId, out var tree))
            return INVALID_HEIGHT;

        var pos = Vector3.ConvertToVMapCoords(x, y, z);
        float height = tree.GetHeight(pos, z + 100.0f);

        if (height <= INVALID_HEIGHT + 1.0f)
            return INVALID_HEIGHT;

        return height;
    }

    /// <summary>
    /// Gets the hit position along a ray between two world positions.
    /// Returns true if there is a collision, with the hit position in rx/ry/rz.
    /// </summary>
    public bool GetObjectHitPos(uint mapId, float x1, float y1, float z1, float x2, float y2, float z2,
        ref float rx, ref float ry, ref float rz, float pModifyDist)
    {
        if (!m_mapTrees.TryGetValue(mapId, out var tree))
        {
            rx = x2;
            ry = y2;
            rz = z2;
            return false;
        }

        var pos1 = Vector3.ConvertToVMapCoords(x1, y1, z1);
        var pos2 = Vector3.ConvertToVMapCoords(x2, y2, z2);

        if (tree.GetObjectHitPos(pos1, pos2, out var hitPos, pModifyDist))
        {
            Vector3.ConvertToWoWCoords(hitPos, out rx, out ry, out rz);
            return true;
        }

        rx = x2;
        ry = y2;
        rz = z2;
        return false;
    }

    /// <summary>
    /// Checks if the specified VMap data directory contains valid data for a map.
    /// </summary>
    public static bool ExistMapTree(string basePath, uint mapId)
    {
        var filename = System.IO.Path.Combine(basePath, $"{mapId:D4}.vmtree");
        return System.IO.File.Exists(filename);
    }
}
