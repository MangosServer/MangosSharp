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

namespace Mangos.World.Maps.MMap;

/// <summary>
/// Result of a pathfinding computation.
/// </summary>
public enum PathType
{
    PathNotFound = 0,
    PathFoundComplete = 1,
    PathFoundPartial = 2,
    PathFoundShortcut = 3,
}

/// <summary>
/// A waypoint in a computed path.
/// </summary>
public struct PathPoint
{
    public float X;
    public float Y;
    public float Z;

    public PathPoint(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}

/// <summary>
/// Encapsulates a pathfinding request and its result.
/// Port of MangosZero's PathInfo concept.
/// </summary>
public class PathInfo
{
    public const int MAX_PATH_LENGTH = 256;
    public const int MAX_POINT_PATH_LENGTH = 74;

    private readonly uint m_mapId;
    private readonly MMapManager m_mmapManager;

    private PathPoint[] m_pathPoints;
    private int m_pathPointCount;
    private PathType m_type;
    private float m_totalLength;

    public PathPoint[] Points => m_pathPoints;
    public int PointCount => m_pathPointCount;
    public PathType Type => m_type;
    public float TotalLength => m_totalLength;

    public PathInfo(uint mapId, MMapManager mmapManager)
    {
        m_mapId = mapId;
        m_mmapManager = mmapManager;
        m_pathPoints = Array.Empty<PathPoint>();
        m_pathPointCount = 0;
        m_type = PathType.PathNotFound;
    }

    /// <summary>
    /// Computes a path from start to end position using the navmesh.
    /// </summary>
    public PathType Calculate(float startX, float startY, float startZ, float endX, float endY, float endZ)
    {
        m_type = PathType.PathNotFound;
        m_pathPointCount = 0;
        m_totalLength = 0;

        if (m_mmapManager == null)
        {
            // No navmesh - return direct path
            return BuildDirectPath(startX, startY, startZ, endX, endY, endZ);
        }

        var navMesh = m_mmapManager.GetNavMesh(m_mapId);
        if (navMesh == null || !navMesh.IsLoaded)
        {
            return BuildDirectPath(startX, startY, startZ, endX, endY, endZ);
        }

        var query = m_mmapManager.GetNavMeshQuery(m_mapId);
        if (query == null)
        {
            return BuildDirectPath(startX, startY, startZ, endX, endY, endZ);
        }

        var filter = new DtQueryFilter();
        var extents = new float[] { 6.0f, 50.0f, 6.0f };
        var startPos = new float[] { startY, startZ, startX }; // Convert to Detour coords
        var endPos = new float[] { endY, endZ, endX };

        // Find nearest polygons
        var status = query.FindNearestPoly(startPos, extents, filter, out var startRef, out _);
        if (DtStatus.Failed(status) || !startRef.IsValid)
        {
            return BuildDirectPath(startX, startY, startZ, endX, endY, endZ);
        }

        status = query.FindNearestPoly(endPos, extents, filter, out var endRef, out _);
        if (DtStatus.Failed(status) || !endRef.IsValid)
        {
            return BuildDirectPath(startX, startY, startZ, endX, endY, endZ);
        }

        // Find polygon path
        var polyPath = new DtPolyRef[MAX_PATH_LENGTH];
        status = query.FindPath(startRef, endRef, startPos, endPos, filter, polyPath, out int polyPathCount, MAX_PATH_LENGTH);
        if (DtStatus.Failed(status) || polyPathCount == 0)
        {
            return BuildDirectPath(startX, startY, startZ, endX, endY, endZ);
        }

        // Convert to straight path (waypoints)
        var straightPath = new float[MAX_POINT_PATH_LENGTH * 3];
        var straightPathFlags = new byte[MAX_POINT_PATH_LENGTH];
        var straightPathRefs = new DtPolyRef[MAX_POINT_PATH_LENGTH];

        status = query.FindStraightPath(startPos, endPos, polyPath, polyPathCount,
            straightPath, straightPathFlags, straightPathRefs,
            out int straightPathCount, MAX_POINT_PATH_LENGTH);

        if (DtStatus.Failed(status) || straightPathCount == 0)
        {
            return BuildDirectPath(startX, startY, startZ, endX, endY, endZ);
        }

        // Convert waypoints from Detour coords to WoW coords
        m_pathPoints = new PathPoint[straightPathCount];
        m_pathPointCount = straightPathCount;
        for (int i = 0; i < straightPathCount; i++)
        {
            int idx = i * 3;
            // Detour uses (Y, Z, X) relative to WoW
            m_pathPoints[i] = new PathPoint(
                straightPath[idx + 2],  // detour Z -> wow X
                straightPath[idx],      // detour X -> wow Y
                straightPath[idx + 1]   // detour Y -> wow Z
            );
        }

        // Calculate total path length
        m_totalLength = 0;
        for (int i = 1; i < m_pathPointCount; i++)
        {
            float dx = m_pathPoints[i].X - m_pathPoints[i - 1].X;
            float dy = m_pathPoints[i].Y - m_pathPoints[i - 1].Y;
            float dz = m_pathPoints[i].Z - m_pathPoints[i - 1].Z;
            m_totalLength += MathF.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        m_type = straightPathCount > 1 ? PathType.PathFoundComplete : PathType.PathFoundShortcut;
        return m_type;
    }

    private PathType BuildDirectPath(float startX, float startY, float startZ, float endX, float endY, float endZ)
    {
        m_pathPoints = new PathPoint[]
        {
            new PathPoint(startX, startY, startZ),
            new PathPoint(endX, endY, endZ)
        };
        m_pathPointCount = 2;

        float dx = endX - startX;
        float dy = endY - startY;
        float dz = endZ - startZ;
        m_totalLength = MathF.Sqrt(dx * dx + dy * dy + dz * dz);

        m_type = PathType.PathFoundShortcut;
        return m_type;
    }
}
