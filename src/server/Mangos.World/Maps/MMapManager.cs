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

using Mangos.World.Maps.MMap;
using System;
using System.Collections.Generic;
using System.IO;

namespace Mangos.World.Maps;

/// <summary>
/// Manages MMap (Movement Map) data for server-side pathfinding.
/// Port of MangosZero's MMapManager.
///
/// MMap data is produced by the MangosZero map extractor and consists of:
/// - .mmap files: per-map navmesh parameters (origin, tile size, etc.)
/// - .mmtile files: per-tile Detour navmesh tile data
///
/// The MMap system uses Recast/Detour for navigation mesh generation and pathfinding.
/// Tiles are loaded on-demand as players enter areas.
/// </summary>
public class MMapManager
{
    private string m_basePath;
    private bool m_initialized;
    private Dictionary<uint, MMapData> m_maps;

    private class MMapData
    {
        public DtNavMesh NavMesh;
        public DtNavMeshQuery NavMeshQuery;
        public HashSet<uint> LoadedTiles;
    }

    public MMapManager()
    {
        m_maps = new Dictionary<uint, MMapData>();
    }

    /// <summary>
    /// Initializes the MMap manager with the base directory containing MMap data files.
    /// </summary>
    public void Initialize(string basePath)
    {
        m_basePath = basePath;
        m_initialized = true;
    }

    /// <summary>
    /// Loads the navmesh parameters for a specific map from its .mmap file.
    /// </summary>
    public bool LoadMap(uint mapId)
    {
        if (!m_initialized)
            return false;

        if (m_maps.ContainsKey(mapId))
            return true;

        var filename = Path.Combine(m_basePath, $"{mapId:D4}.mmap");
        if (!File.Exists(filename))
            return false;

        try
        {
            using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new BinaryReader(fs);

            // Read MMap header
            var header = MapFileFormats.MmapTileHeader.Read(reader);
            if (!header.IsValid)
                return false;

            // Read navmesh parameters
            // The .mmap file contains dtNavMeshParams: origin, tileWidth, tileHeight, maxTiles, maxPolys
            var origin = new float[3];
            origin[0] = reader.ReadSingle();
            origin[1] = reader.ReadSingle();
            origin[2] = reader.ReadSingle();
            float tileWidth = reader.ReadSingle();
            float tileHeight = reader.ReadSingle();
            int maxTiles = reader.ReadInt32();
            int maxPolys = reader.ReadInt32();

            var navMesh = new DtNavMesh();
            if (!navMesh.Init(origin, tileWidth, tileHeight, maxTiles, maxPolys))
                return false;

            var query = new DtNavMeshQuery();
            if (!query.Init(navMesh))
                return false;

            m_maps[mapId] = new MMapData
            {
                NavMesh = navMesh,
                NavMeshQuery = query,
                LoadedTiles = new HashSet<uint>()
            };

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Loads a navmesh tile for a specific map.
    /// </summary>
    public bool LoadMapTile(uint mapId, byte tileX, byte tileY)
    {
        if (!m_maps.TryGetValue(mapId, out var data))
        {
            // Try to load the map first
            if (!LoadMap(mapId))
                return false;
            data = m_maps[mapId];
        }

        var tileId = PackTileId(tileX, tileY);
        if (data.LoadedTiles.Contains(tileId))
            return true;

        var filename = Path.Combine(m_basePath, $"{mapId:D4}{tileY:D2}{tileX:D2}.mmtile");
        if (!File.Exists(filename))
            return false;

        try
        {
            using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new BinaryReader(fs);

            // Read MmapTileHeader
            var header = MapFileFormats.MmapTileHeader.Read(reader);
            if (!header.IsValid)
                return false;

            // Read the raw Detour tile data
            var tileData = reader.ReadBytes((int)header.Size);
            if (tileData.Length != header.Size)
                return false;

            if (data.NavMesh.AddTile(tileData, tileX, tileY))
            {
                data.LoadedTiles.Add(tileId);
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Unloads a navmesh tile.
    /// </summary>
    public void UnloadMapTile(uint mapId, byte tileX, byte tileY)
    {
        if (m_maps.TryGetValue(mapId, out var data))
        {
            var tileId = PackTileId(tileX, tileY);
            if (data.LoadedTiles.Remove(tileId))
            {
                data.NavMesh.RemoveTile(tileX, tileY);
            }
        }
    }

    /// <summary>
    /// Gets the navmesh for a specific map.
    /// </summary>
    public DtNavMesh GetNavMesh(uint mapId)
    {
        return m_maps.TryGetValue(mapId, out var data) ? data.NavMesh : null;
    }

    /// <summary>
    /// Gets the navmesh query object for a specific map.
    /// </summary>
    public DtNavMeshQuery GetNavMeshQuery(uint mapId)
    {
        return m_maps.TryGetValue(mapId, out var data) ? data.NavMeshQuery : null;
    }

    /// <summary>
    /// Computes a path between two world positions on a specific map.
    /// Returns an array of waypoints.
    /// </summary>
    public PathInfo FindPath(uint mapId, float startX, float startY, float startZ, float endX, float endY, float endZ)
    {
        var pathInfo = new PathInfo(mapId, this);
        pathInfo.Calculate(startX, startY, startZ, endX, endY, endZ);
        return pathInfo;
    }

    /// <summary>
    /// Checks if MMap data exists for a specific map.
    /// </summary>
    public static bool ExistMMap(string basePath, uint mapId)
    {
        var filename = Path.Combine(basePath, $"{mapId:D4}.mmap");
        return File.Exists(filename);
    }

    private static uint PackTileId(byte x, byte y) => (uint)(x << 8 | y);
}
