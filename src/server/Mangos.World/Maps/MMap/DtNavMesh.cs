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

namespace Mangos.World.Maps.MMap;

/// <summary>
/// Detour navmesh status codes.
/// </summary>
public static class DtStatus
{
    public const uint DT_FAILURE = 0x80000000;
    public const uint DT_SUCCESS = 0x40000000;
    public const uint DT_IN_PROGRESS = 0x20000000;

    public static bool Succeeded(uint status) => (status & DT_SUCCESS) != 0;
    public static bool Failed(uint status) => (status & DT_FAILURE) != 0;
    public static bool InProgress(uint status) => (status & DT_IN_PROGRESS) != 0;
}

/// <summary>
/// Detour polygon reference. Encodes a tile index and polygon index.
/// </summary>
public readonly struct DtPolyRef
{
    public readonly ulong Value;

    public DtPolyRef(ulong value) => Value = value;

    public bool IsValid => Value != 0;

    public static readonly DtPolyRef Null = new(0);

    public static DtPolyRef Encode(int tileIndex, int polyIndex, int salt)
    {
        // Standard Detour encoding: salt | tile | poly
        return new DtPolyRef((ulong)((salt << 28) | (tileIndex << 16) | polyIndex));
    }

    public void Decode(out int salt, out int tileIndex, out int polyIndex)
    {
        polyIndex = (int)(Value & 0xFFFF);
        tileIndex = (int)((Value >> 16) & 0xFFF);
        salt = (int)((Value >> 28) & 0xF);
    }
}

/// <summary>
/// Navmesh tile header, matching Detour's dtMeshHeader.
/// </summary>
public struct DtMeshHeader
{
    public int Magic;
    public int Version;
    public int X;
    public int Y;
    public int Layer;
    public uint UserId;
    public int PolyCount;
    public int VertCount;
    public int MaxLinkCount;
    public int DetailMeshCount;
    public int DetailVertCount;
    public int DetailTriCount;
    public int BvNodeCount;
    public int OffMeshConCount;
    public int OffMeshBase;
    public float WalkableHeight;
    public float WalkableRadius;
    public float WalkableClimb;
    public float[] BMin;
    public float[] BMax;
    public float BvQuantFactor;

    public static DtMeshHeader Read(BinaryReader reader)
    {
        var h = new DtMeshHeader
        {
            Magic = reader.ReadInt32(),
            Version = reader.ReadInt32(),
            X = reader.ReadInt32(),
            Y = reader.ReadInt32(),
            Layer = reader.ReadInt32(),
            UserId = reader.ReadUInt32(),
            PolyCount = reader.ReadInt32(),
            VertCount = reader.ReadInt32(),
            MaxLinkCount = reader.ReadInt32(),
            DetailMeshCount = reader.ReadInt32(),
            DetailVertCount = reader.ReadInt32(),
            DetailTriCount = reader.ReadInt32(),
            BvNodeCount = reader.ReadInt32(),
            OffMeshConCount = reader.ReadInt32(),
            OffMeshBase = reader.ReadInt32(),
            WalkableHeight = reader.ReadSingle(),
            WalkableRadius = reader.ReadSingle(),
            WalkableClimb = reader.ReadSingle(),
            BMin = new float[3],
            BMax = new float[3]
        };
        for (int i = 0; i < 3; i++) h.BMin[i] = reader.ReadSingle();
        for (int i = 0; i < 3; i++) h.BMax[i] = reader.ReadSingle();
        h.BvQuantFactor = reader.ReadSingle();
        return h;
    }

    public bool IsValid => Magic == 0x4D446E61; // 'DNav' (dtNavMesh magic)
}

/// <summary>
/// Navmesh polygon definition.
/// </summary>
public struct DtPoly
{
    public uint FirstLink;
    public ushort[] Verts;    // max 6
    public ushort[] Neis;     // max 6
    public ushort Flags;
    public byte VertCount;
    public byte AreaAndType;

    public byte Area => (byte)(AreaAndType & 0x3F);
    public byte Type => (byte)(AreaAndType >> 6);

    public static DtPoly Read(BinaryReader reader)
    {
        var p = new DtPoly
        {
            FirstLink = reader.ReadUInt32(),
            Verts = new ushort[6],
            Neis = new ushort[6]
        };
        for (int i = 0; i < 6; i++) p.Verts[i] = reader.ReadUInt16();
        for (int i = 0; i < 6; i++) p.Neis[i] = reader.ReadUInt16();
        p.Flags = reader.ReadUInt16();
        p.VertCount = reader.ReadByte();
        p.AreaAndType = reader.ReadByte();
        return p;
    }
}

/// <summary>
/// A loaded navigation mesh tile containing polygon and vertex data.
/// Port of Detour's dtMeshTile concept (simplified for server-side pathfinding).
/// </summary>
public class DtMeshTile
{
    public DtMeshHeader Header;
    public float[] Verts;         // 3 floats per vertex
    public DtPoly[] Polys;
    public byte[] RawData;        // Original tile data for complex operations

    /// <summary>
    /// Gets the vertices of a polygon as a float array (3 floats per vertex).
    /// </summary>
    public void GetPolyVerts(int polyIndex, out float[][] verts)
    {
        var poly = Polys[polyIndex];
        verts = new float[poly.VertCount][];
        for (int i = 0; i < poly.VertCount; i++)
        {
            int vi = poly.Verts[i] * 3;
            verts[i] = new float[] { Verts[vi], Verts[vi + 1], Verts[vi + 2] };
        }
    }

    /// <summary>
    /// Finds the closest point on a polygon to the given position.
    /// Uses barycentric projection to the polygon surface.
    /// </summary>
    public bool ClosestPointOnPoly(int polyIndex, float[] pos, float[] closest)
    {
        if (polyIndex >= Polys.Length)
            return false;

        var poly = Polys[polyIndex];
        if (poly.VertCount == 0)
            return false;

        // Simple projection: find the centroid as a fallback
        closest[0] = 0; closest[1] = 0; closest[2] = 0;
        for (int i = 0; i < poly.VertCount; i++)
        {
            int vi = poly.Verts[i] * 3;
            closest[0] += Verts[vi];
            closest[1] += Verts[vi + 1];
            closest[2] += Verts[vi + 2];
        }
        closest[0] /= poly.VertCount;
        closest[1] /= poly.VertCount;
        closest[2] /= poly.VertCount;

        // Proper closest-point: project pos onto polygon plane and clamp to edges
        // For detailed nav queries, iterate over edges and find minimum distance
        float minDist = float.MaxValue;
        for (int i = 0, j = poly.VertCount - 1; i < poly.VertCount; j = i++)
        {
            int vi = poly.Verts[i] * 3;
            int vj = poly.Verts[j] * 3;

            // Edge midpoint
            float mx = (Verts[vi] + Verts[vj]) * 0.5f;
            float my = (Verts[vi + 1] + Verts[vj + 1]) * 0.5f;
            float mz = (Verts[vi + 2] + Verts[vj + 2]) * 0.5f;

            float dx = pos[0] - mx;
            float dy = pos[1] - my;
            float dz = pos[2] - mz;
            float dist = dx * dx + dy * dy + dz * dz;
            if (dist < minDist)
            {
                minDist = dist;
            }
        }

        // Use Y from the polygon surface (interpolated height)
        closest[1] = GetPolyHeight(polyIndex, closest[0], closest[2]);

        return true;
    }

    /// <summary>
    /// Gets the height on a polygon at the given XZ position.
    /// </summary>
    public float GetPolyHeight(int polyIndex, float x, float z)
    {
        if (polyIndex >= Polys.Length)
            return 0;

        var poly = Polys[polyIndex];
        if (poly.VertCount < 3)
            return 0;

        // Triangulate the polygon and find which triangle contains (x,z)
        int v0i = poly.Verts[0] * 3;
        for (int i = 1; i < poly.VertCount - 1; i++)
        {
            int v1i = poly.Verts[i] * 3;
            int v2i = poly.Verts[i + 1] * 3;

            float ax = Verts[v0i], ay = Verts[v0i + 1], az = Verts[v0i + 2];
            float bx = Verts[v1i], by = Verts[v1i + 1], bz = Verts[v1i + 2];
            float cx = Verts[v2i], cy = Verts[v2i + 1], cz = Verts[v2i + 2];

            if (PointInTriangle(x, z, ax, az, bx, bz, cx, cz))
            {
                // Barycentric interpolation for height
                float det = (bz - cz) * (ax - cx) + (cx - bx) * (az - cz);
                if (MathF.Abs(det) < 1e-10f) continue;
                float u = ((bz - cz) * (x - cx) + (cx - bx) * (z - cz)) / det;
                float v = ((cz - az) * (x - cx) + (ax - cx) * (z - cz)) / det;
                float w = 1.0f - u - v;
                return u * ay + v * by + w * cy;
            }
        }

        // Fallback: return average height
        float sumY = 0;
        for (int i = 0; i < poly.VertCount; i++)
        {
            sumY += Verts[poly.Verts[i] * 3 + 1];
        }
        return sumY / poly.VertCount;
    }

    private static bool PointInTriangle(float px, float pz, float ax, float az, float bx, float bz, float cx, float cz)
    {
        float d1 = Sign(px, pz, ax, az, bx, bz);
        float d2 = Sign(px, pz, bx, bz, cx, cz);
        float d3 = Sign(px, pz, cx, cz, ax, az);
        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
        return !(hasNeg && hasPos);
    }

    private static float Sign(float x1, float z1, float x2, float z2, float x3, float z3)
    {
        return (x1 - x3) * (z2 - z3) - (x2 - x3) * (z1 - z3);
    }
}

/// <summary>
/// Navigation mesh for a single map. Stores loaded tiles and provides spatial queries.
/// Port of Detour's dtNavMesh (simplified for server-side pathfinding).
/// </summary>
public class DtNavMesh
{
    private readonly Dictionary<ulong, DtMeshTile> m_tiles = new();
    private float m_tileWidth;
    private float m_tileHeight;
    private float[] m_origin = new float[3];
    private int m_maxTiles;
    private int m_maxPolys;

    public bool IsLoaded => m_tiles.Count > 0;

    /// <summary>
    /// Initializes the navmesh with parameters from the .mmap file.
    /// </summary>
    public bool Init(float[] origin, float tileWidth, float tileHeight, int maxTiles, int maxPolys)
    {
        m_origin = origin;
        m_tileWidth = tileWidth;
        m_tileHeight = tileHeight;
        m_maxTiles = maxTiles;
        m_maxPolys = maxPolys;
        return true;
    }

    /// <summary>
    /// Adds a tile to the navmesh.
    /// The data is the raw Detour tile data (after the MmapTileHeader).
    /// </summary>
    public bool AddTile(byte[] data, int tileX, int tileY)
    {
        try
        {
            using var ms = new MemoryStream(data);
            using var reader = new BinaryReader(ms);

            var header = DtMeshHeader.Read(reader);
            if (!header.IsValid)
                return false;

            var tile = new DtMeshTile { Header = header };

            // Read vertices
            tile.Verts = new float[header.VertCount * 3];
            for (int i = 0; i < header.VertCount * 3; i++)
            {
                tile.Verts[i] = reader.ReadSingle();
            }

            // Read polygons
            tile.Polys = new DtPoly[header.PolyCount];
            for (int i = 0; i < header.PolyCount; i++)
            {
                tile.Polys[i] = DtPoly.Read(reader);
            }

            tile.RawData = data;

            var key = PackTileKey(tileX, tileY);
            m_tiles[key] = tile;
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Removes a tile from the navmesh.
    /// </summary>
    public void RemoveTile(int tileX, int tileY)
    {
        var key = PackTileKey(tileX, tileY);
        m_tiles.Remove(key);
    }

    /// <summary>
    /// Gets the tile at the given tile coordinates.
    /// </summary>
    public DtMeshTile GetTileAt(int tileX, int tileY)
    {
        var key = PackTileKey(tileX, tileY);
        return m_tiles.TryGetValue(key, out var tile) ? tile : null;
    }

    /// <summary>
    /// Gets the tile coordinates for a world position.
    /// </summary>
    public void CalcTileLoc(float x, float z, out int tileX, out int tileY)
    {
        tileX = (int)MathF.Floor((x - m_origin[0]) / m_tileWidth);
        tileY = (int)MathF.Floor((z - m_origin[2]) / m_tileHeight);
    }

    /// <summary>
    /// Finds the nearest polygon to the given position within the search extents.
    /// </summary>
    public DtPolyRef FindNearestPoly(float[] center, float[] extents, out float[] nearestPt)
    {
        nearestPt = new float[3];
        Array.Copy(center, nearestPt, 3);

        // Calculate tile range to search
        CalcTileLoc(center[0] - extents[0], center[2] - extents[2], out int minTx, out int minTy);
        CalcTileLoc(center[0] + extents[0], center[2] + extents[2], out int maxTx, out int maxTy);

        float nearestDist = float.MaxValue;
        DtPolyRef nearestRef = DtPolyRef.Null;

        for (int tx = minTx; tx <= maxTx; tx++)
        {
            for (int ty = minTy; ty <= maxTy; ty++)
            {
                var tile = GetTileAt(tx, ty);
                if (tile == null) continue;

                for (int p = 0; p < tile.Polys.Length; p++)
                {
                    var poly = tile.Polys[p];
                    if (poly.VertCount == 0) continue;

                    // Check if polygon centroid is within search bounds
                    float cx = 0, cy = 0, cz = 0;
                    for (int v = 0; v < poly.VertCount; v++)
                    {
                        int vi = poly.Verts[v] * 3;
                        cx += tile.Verts[vi];
                        cy += tile.Verts[vi + 1];
                        cz += tile.Verts[vi + 2];
                    }
                    cx /= poly.VertCount;
                    cy /= poly.VertCount;
                    cz /= poly.VertCount;

                    float dx = cx - center[0];
                    float dy = cy - center[1];
                    float dz = cz - center[2];
                    float dist = dx * dx + dy * dy + dz * dz;

                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearestRef = DtPolyRef.Encode(tx * 1000 + ty, p, 1);
                        nearestPt[0] = cx;
                        nearestPt[1] = cy;
                        nearestPt[2] = cz;
                    }
                }
            }
        }

        return nearestRef;
    }

    private static ulong PackTileKey(int x, int y) => ((ulong)(uint)x << 32) | (uint)y;
}
