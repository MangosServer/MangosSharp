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
/// Bounding Interval Hierarchy (BIH) for accelerated ray-casting.
/// C# port of MangosZero's BIH implementation.
///
/// The BIH is a spatial acceleration structure similar to a KD-tree but with
/// overlapping split planes. It partitions objects into a binary tree where
/// each internal node has two bounding planes along one axis.
/// </summary>
public class BIH
{
    // Node encoding matches MangosZero's BIH format:
    // Bits 0-1: axis (0=X, 1=Y, 2=Z) or 3=leaf
    // Bits 2-31: child offset (internal) or primitive count (leaf)
    private const int BIH_AXIS_MASK = 0x3;
    private const int BIH_LEAF = 0x3;

    private uint[] m_tree;
    private AABox m_bounds;
    private uint[] m_objects; // indices into the model instance array

    public AABox Bounds => m_bounds;
    public bool IsEmpty => m_tree == null || m_tree.Length == 0;

    public bool ReadFromFile(BinaryReader reader)
    {
        try
        {
            // Read tree nodes
            var treeSize = reader.ReadUInt32();
            m_tree = new uint[treeSize];
            for (int i = 0; i < treeSize; i++)
            {
                m_tree[i] = reader.ReadUInt32();
            }

            // Read bounds
            m_bounds = AABox.Read(reader);

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Intersects a ray against the BIH tree.
    /// Calls the callback for each potentially intersecting leaf primitive.
    /// Returns true if any intersection was found.
    /// </summary>
    public bool IntersectRay(Ray ray, Func<uint, Ray, float, bool> intersectCallback, float maxDist)
    {
        if (m_tree == null || m_tree.Length == 0)
            return false;

        if (!m_bounds.IntersectRay(ray.Origin, ray.Direction, out float tNear, out float tFar))
            return false;

        tNear = MathF.Max(tNear, 0f);
        tFar = MathF.Min(tFar, maxDist);

        if (tNear > tFar)
            return false;

        return IntersectNode(0, ray, ref tNear, ref tFar, intersectCallback, ref maxDist);
    }

    private bool IntersectNode(int nodeIndex, Ray ray, ref float tNear, ref float tFar,
        Func<uint, Ray, float, bool> callback, ref float maxDist)
    {
        if (nodeIndex < 0 || nodeIndex >= m_tree.Length)
            return false;

        var node = m_tree[nodeIndex];
        var nodeType = node & BIH_AXIS_MASK;

        if (nodeType == BIH_LEAF)
        {
            // Leaf node - test all primitives
            var count = (node >> 2);
            bool hit = false;
            for (uint i = 0; i < count; i++)
            {
                var primIndex = (uint)(nodeIndex + 1 + i);
                if (primIndex < m_tree.Length)
                {
                    var objIndex = m_tree[primIndex];
                    if (callback(objIndex, ray, maxDist))
                    {
                        hit = true;
                    }
                }
            }
            return hit;
        }
        else
        {
            // Internal node - split along axis
            int axis = (int)nodeType;
            var childOffset = (int)(node >> 2);

            if (childOffset + 1 >= m_tree.Length)
                return false;

            // The split planes are stored as floats in the child nodes
            float splitLeft = BitConverter.Int32BitsToSingle((int)m_tree[nodeIndex + 1]);
            float splitRight = BitConverter.Int32BitsToSingle((int)m_tree[nodeIndex + 2]);

            float dirAxis = ray.Direction[axis];
            float originAxis = ray.Origin[axis];

            // Determine near/far children based on ray direction
            int nearChild, farChild;
            float tSplitNear, tSplitFar;

            if (dirAxis >= 0)
            {
                nearChild = childOffset;
                farChild = childOffset + (childOffset < m_tree.Length ? GetNodeSize(childOffset) : 0);
                tSplitNear = (MathF.Abs(dirAxis) > 1e-10f) ? (splitLeft - originAxis) / dirAxis : float.PositiveInfinity;
                tSplitFar = (MathF.Abs(dirAxis) > 1e-10f) ? (splitRight - originAxis) / dirAxis : float.NegativeInfinity;
            }
            else
            {
                nearChild = childOffset + (childOffset < m_tree.Length ? GetNodeSize(childOffset) : 0);
                farChild = childOffset;
                tSplitNear = (MathF.Abs(dirAxis) > 1e-10f) ? (splitRight - originAxis) / dirAxis : float.PositiveInfinity;
                tSplitFar = (MathF.Abs(dirAxis) > 1e-10f) ? (splitLeft - originAxis) / dirAxis : float.NegativeInfinity;
            }

            bool hit = false;

            // Traverse near child
            if (tNear <= tSplitNear && nearChild < m_tree.Length)
            {
                float newFar = MathF.Min(tFar, tSplitNear);
                if (tNear <= newFar)
                {
                    if (IntersectNode(nearChild, ray, ref tNear, ref newFar, callback, ref maxDist))
                        hit = true;
                }
            }

            // Traverse far child
            if (tSplitFar <= tFar && farChild < m_tree.Length)
            {
                float newNear = MathF.Max(tNear, tSplitFar);
                if (newNear <= tFar)
                {
                    if (IntersectNode(farChild, ray, ref newNear, ref tFar, callback, ref maxDist))
                        hit = true;
                }
            }

            return hit;
        }
    }

    private int GetNodeSize(int nodeIndex)
    {
        if (nodeIndex >= m_tree.Length)
            return 1;
        var node = m_tree[nodeIndex];
        var nodeType = node & BIH_AXIS_MASK;
        if (nodeType == BIH_LEAF)
        {
            return 1 + (int)(node >> 2); // leaf header + primitive indices
        }
        return 3; // internal node: header + 2 split planes
    }

    /// <summary>
    /// Simplified intersection for model instances - tests bounds only.
    /// </summary>
    public bool IntersectBounds(Ray ray, float maxDist)
    {
        return m_bounds.IntersectRay(ray.Origin, ray.Direction, out float tMin, out _) && tMin <= maxDist;
    }
}
