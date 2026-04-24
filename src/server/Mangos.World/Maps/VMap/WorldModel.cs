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

using Mangos.World.Maps;
using System;
using System.IO;
using System.Text;

namespace Mangos.World.Maps.VMap;

/// <summary>
/// A group within a world model (corresponds to a WMO group).
/// Contains mesh data (vertices + triangles) and an optional BIH for ray-casting.
/// Port of MangosZero's GroupModel.
/// </summary>
public class GroupModel
{
    private uint m_mogpFlags;
    private uint m_groupWMOID;
    private AABox m_bound;
    private Vector3[] m_vertices;
    private int[] m_triangleIndices;
    private BIH m_meshTree;

    // Liquid data
    private uint m_liquidType;
    private WmoLiquid m_liquid;

    public AABox Bounds => m_bound;
    public uint MogpFlags => m_mogpFlags;
    public uint GroupWMOID => m_groupWMOID;

    public bool ReadFromFile(BinaryReader reader)
    {
        try
        {
            m_mogpFlags = reader.ReadUInt32();
            m_groupWMOID = reader.ReadUInt32();
            m_bound = AABox.Read(reader);
            m_liquidType = reader.ReadUInt32();

            // Read mesh triangle BIH
            m_meshTree = new BIH();
            if (!m_meshTree.ReadFromFile(reader))
                return false;

            // Read vertices
            var vertexCount = reader.ReadUInt32();
            m_vertices = new Vector3[vertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                m_vertices[i] = Vector3.Read(reader);
            }

            // Read triangle indices
            var triangleCount = reader.ReadUInt32();
            m_triangleIndices = new int[triangleCount * 3];
            for (int i = 0; i < triangleCount * 3; i++)
            {
                m_triangleIndices[i] = reader.ReadInt32();
            }

            // Read liquid data if present
            if (reader.ReadUInt32() != 0) // hasLiquid
            {
                m_liquid = new WmoLiquid();
                if (!m_liquid.ReadFromFile(reader))
                    return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Tests ray intersection against the mesh triangles in this group.
    /// Returns true if any triangle is hit, with distance set to the nearest hit.
    /// </summary>
    public bool IntersectRay(Ray ray, ref float distance, bool stopAtFirstHit)
    {
        if (m_vertices == null || m_triangleIndices == null)
            return false;

        if (!m_bound.IntersectRay(ray.Origin, ray.Direction, out _, out _))
            return false;

        bool hit = false;
        float closestDist = distance;

        // Test each triangle directly (for models without BIH)
        if (m_meshTree == null || m_meshTree.IsEmpty)
        {
            int triCount = m_triangleIndices.Length / 3;
            for (int i = 0; i < triCount; i++)
            {
                int idx0 = m_triangleIndices[i * 3];
                int idx1 = m_triangleIndices[i * 3 + 1];
                int idx2 = m_triangleIndices[i * 3 + 2];

                if (idx0 >= m_vertices.Length || idx1 >= m_vertices.Length || idx2 >= m_vertices.Length)
                    continue;

                if (IntersectTriangle(ray, m_vertices[idx0], m_vertices[idx1], m_vertices[idx2], out float t))
                {
                    if (t >= 0 && t < closestDist)
                    {
                        closestDist = t;
                        hit = true;
                        if (stopAtFirstHit)
                        {
                            distance = closestDist;
                            return true;
                        }
                    }
                }
            }
        }
        else
        {
            // Use BIH tree for acceleration
            m_meshTree.IntersectRay(ray, (uint triIndex, Ray r, float maxDist) =>
            {
                if (triIndex * 3 + 2 >= (uint)m_triangleIndices.Length)
                    return false;

                int idx0 = m_triangleIndices[triIndex * 3];
                int idx1 = m_triangleIndices[triIndex * 3 + 1];
                int idx2 = m_triangleIndices[triIndex * 3 + 2];

                if (idx0 >= m_vertices.Length || idx1 >= m_vertices.Length || idx2 >= m_vertices.Length)
                    return false;

                if (IntersectTriangle(r, m_vertices[idx0], m_vertices[idx1], m_vertices[idx2], out float t))
                {
                    if (t >= 0 && t < closestDist)
                    {
                        closestDist = t;
                        hit = true;
                        return true;
                    }
                }
                return false;
            }, closestDist);
        }

        if (hit)
        {
            distance = closestDist;
        }
        return hit;
    }

    /// <summary>
    /// Gets the liquid level at the given coordinates within this group model.
    /// Returns negative infinity if no liquid data exists.
    /// </summary>
    public bool GetLiquidLevel(Vector3 pos, out float level)
    {
        level = float.NegativeInfinity;
        if (m_liquid == null)
            return false;
        return m_liquid.GetLiquidHeight(pos, out level);
    }

    /// <summary>
    /// Moller-Trumbore ray-triangle intersection test.
    /// </summary>
    private static bool IntersectTriangle(Ray ray, Vector3 v0, Vector3 v1, Vector3 v2, out float t)
    {
        t = 0;
        const float EPSILON = 1e-6f;

        var edge1 = v1 - v0;
        var edge2 = v2 - v0;
        var pvec = Vector3.Cross(ray.Direction, edge2);
        float det = Vector3.Dot(edge1, pvec);

        if (det > -EPSILON && det < EPSILON)
            return false;

        float invDet = 1.0f / det;
        var tvec = ray.Origin - v0;
        float u = Vector3.Dot(tvec, pvec) * invDet;

        if (u < 0f || u > 1f)
            return false;

        var qvec = Vector3.Cross(tvec, edge1);
        float v = Vector3.Dot(ray.Direction, qvec) * invDet;

        if (v < 0f || u + v > 1f)
            return false;

        t = Vector3.Dot(edge2, qvec) * invDet;
        return t > EPSILON;
    }
}

/// <summary>
/// Liquid data within a WMO group.
/// Port of MangosZero's WmoLiquid.
/// </summary>
public class WmoLiquid
{
    private uint m_tilesX;
    private uint m_tilesY;
    private Vector3 m_corner;
    private uint m_type;
    private float[] m_height;
    private byte[] m_flags;

    public bool ReadFromFile(BinaryReader reader)
    {
        try
        {
            m_tilesX = reader.ReadUInt32();
            m_tilesY = reader.ReadUInt32();
            m_corner = Vector3.Read(reader);
            m_type = reader.ReadUInt32();

            var heightCount = (m_tilesX + 1) * (m_tilesY + 1);
            if (heightCount > 0)
            {
                m_height = new float[heightCount];
                for (int i = 0; i < heightCount; i++)
                {
                    m_height[i] = reader.ReadSingle();
                }
            }

            var flagCount = m_tilesX * m_tilesY;
            if (flagCount > 0)
            {
                m_flags = reader.ReadBytes((int)flagCount);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool GetLiquidHeight(Vector3 pos, out float level)
    {
        level = float.NegativeInfinity;
        if (m_height == null || m_tilesX == 0 || m_tilesY == 0)
            return false;

        // Calculate cell position
        float cx = (pos.X - m_corner.X) / MapFileFormats.GRID_PART;
        float cy = (pos.Y - m_corner.Y) / MapFileFormats.GRID_PART;

        int ix = (int)cx;
        int iy = (int)cy;

        if (ix < 0 || ix >= m_tilesX || iy < 0 || iy >= m_tilesY)
            return false;

        // Check if this tile has liquid
        if (m_flags != null)
        {
            int flagIdx = ix * (int)m_tilesY + iy;
            if (flagIdx < m_flags.Length && m_flags[flagIdx] == 0x0F)
                return false; // no liquid in this cell
        }

        // Bilinear interpolation of height
        float fx = cx - ix;
        float fy = cy - iy;
        int stride = (int)(m_tilesY + 1);

        float h00 = m_height[ix * stride + iy];
        float h10 = m_height[(ix + 1) * stride + iy];
        float h01 = m_height[ix * stride + iy + 1];
        float h11 = m_height[(ix + 1) * stride + iy + 1];

        level = h00 * (1 - fx) * (1 - fy) + h10 * fx * (1 - fy) + h01 * (1 - fx) * fy + h11 * fx * fy;
        return true;
    }
}

/// <summary>
/// A world model containing groups of geometry.
/// Loaded from .vmo files generated by the MangosZero extractor.
/// Port of MangosZero's WorldModel.
/// </summary>
public class WorldModel
{
    private const string VMAP_MAGIC = "VMAP004";

    private uint m_rootWmoID;
    private GroupModel[] m_groups;
    private BIH m_groupTree;

    public GroupModel[] Groups => m_groups;

    public bool ReadFile(string filename)
    {
        if (!File.Exists(filename))
            return false;

        try
        {
            using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new BinaryReader(fs);

            // Read magic
            var magic = Encoding.ASCII.GetString(reader.ReadBytes(8));
            if (!magic.StartsWith(VMAP_MAGIC))
                return false;

            // Read chunk: "WMOD"
            var chunkId = Encoding.ASCII.GetString(reader.ReadBytes(4));
            var chunkSize = reader.ReadUInt32();
            if (chunkId != "WMOD")
                return false;

            m_rootWmoID = reader.ReadUInt32();

            // Read chunk: "GMOD" - group models
            chunkId = Encoding.ASCII.GetString(reader.ReadBytes(4));
            chunkSize = reader.ReadUInt32();
            if (chunkId != "GMOD")
                return false;

            var groupCount = reader.ReadUInt32();
            m_groups = new GroupModel[groupCount];
            for (int i = 0; i < groupCount; i++)
            {
                m_groups[i] = new GroupModel();
                if (!m_groups[i].ReadFromFile(reader))
                    return false;
            }

            // Read chunk: "GBIH" - group BIH tree
            if (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                chunkId = Encoding.ASCII.GetString(reader.ReadBytes(4));
                chunkSize = reader.ReadUInt32();
                if (chunkId == "GBIH")
                {
                    m_groupTree = new BIH();
                    if (!m_groupTree.ReadFromFile(reader))
                        return false;
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
    /// Tests ray intersection against all groups in this model.
    /// Returns true if any triangle is hit, with distance set to the nearest hit.
    /// </summary>
    public bool IntersectRay(Ray ray, ref float distance, bool stopAtFirstHit)
    {
        if (m_groups == null)
            return false;

        bool hit = false;
        float closestDist = distance;

        for (int i = 0; i < m_groups.Length; i++)
        {
            if (m_groups[i].IntersectRay(ray, ref closestDist, stopAtFirstHit))
            {
                hit = true;
                if (stopAtFirstHit)
                {
                    distance = closestDist;
                    return true;
                }
            }
        }

        if (hit)
            distance = closestDist;

        return hit;
    }

    /// <summary>
    /// Gets the height by casting a vertical ray downward through the model.
    /// </summary>
    public bool GetHeight(Vector3 position, float maxDist, out float height)
    {
        height = float.NegativeInfinity;

        var ray = new Ray(position, new Vector3(0, -1, 0)); // cast downward
        float dist = maxDist;
        if (IntersectRay(ray, ref dist, false))
        {
            height = position.Y - dist;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the liquid level at the given position within this model.
    /// </summary>
    public bool GetLiquidLevel(Vector3 pos, out float level)
    {
        level = float.NegativeInfinity;
        if (m_groups == null)
            return false;

        for (int i = 0; i < m_groups.Length; i++)
        {
            if (m_groups[i].GetLiquidLevel(pos, out level))
                return true;
        }
        return false;
    }
}
