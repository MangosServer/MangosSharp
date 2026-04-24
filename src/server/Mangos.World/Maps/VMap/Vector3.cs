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
/// 3D vector struct for VMap calculations.
/// Matches the G3D::Vector3 usage in MangosZero's VMap system.
/// </summary>
public struct Vector3
{
    public float X;
    public float Y;
    public float Z;

    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static Vector3 Read(BinaryReader reader)
    {
        return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
    }

    public float Length => MathF.Sqrt(X * X + Y * Y + Z * Z);

    public float LengthSquared => X * X + Y * Y + Z * Z;

    public Vector3 Normalized
    {
        get
        {
            var len = Length;
            if (len < 1e-10f) return new Vector3(0, 0, 0);
            return new Vector3(X / len, Y / len, Z / len);
        }
    }

    public static Vector3 operator +(Vector3 a, Vector3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vector3 operator -(Vector3 a, Vector3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vector3 operator *(Vector3 a, float s) => new(a.X * s, a.Y * s, a.Z * s);
    public static Vector3 operator *(float s, Vector3 a) => new(a.X * s, a.Y * s, a.Z * s);

    public static float Dot(Vector3 a, Vector3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

    public static Vector3 Cross(Vector3 a, Vector3 b) => new(
        a.Y * b.Z - a.Z * b.Y,
        a.Z * b.X - a.X * b.Z,
        a.X * b.Y - a.Y * b.X
    );

    public static Vector3 Min(Vector3 a, Vector3 b) => new(
        MathF.Min(a.X, b.X),
        MathF.Min(a.Y, b.Y),
        MathF.Min(a.Z, b.Z)
    );

    public static Vector3 Max(Vector3 a, Vector3 b) => new(
        MathF.Max(a.X, b.X),
        MathF.Max(a.Y, b.Y),
        MathF.Max(a.Z, b.Z)
    );

    /// <summary>
    /// Converts from WoW coordinate system to VMap internal coordinate system.
    /// MangosZero uses: internal_x = y, internal_y = z, internal_z = x
    /// </summary>
    public static Vector3 ConvertToVMapCoords(float x, float y, float z)
    {
        return new Vector3(y, z, x);
    }

    /// <summary>
    /// Converts from VMap internal coordinate system back to WoW coordinates.
    /// </summary>
    public static void ConvertToWoWCoords(Vector3 vmapPos, out float x, out float y, out float z)
    {
        x = vmapPos.Z;
        y = vmapPos.X;
        z = vmapPos.Y;
    }

    public float this[int index]
    {
        get => index switch { 0 => X, 1 => Y, 2 => Z, _ => throw new IndexOutOfRangeException() };
        set { switch (index) { case 0: X = value; break; case 1: Y = value; break; case 2: Z = value; break; default: throw new IndexOutOfRangeException(); } }
    }

    public override string ToString() => $"({X:F3}, {Y:F3}, {Z:F3})";
}

/// <summary>
/// Axis-aligned bounding box.
/// </summary>
public struct AABox
{
    public Vector3 Lo;
    public Vector3 Hi;

    public AABox(Vector3 lo, Vector3 hi)
    {
        Lo = lo;
        Hi = hi;
    }

    public static AABox Read(BinaryReader reader)
    {
        return new AABox(Vector3.Read(reader), Vector3.Read(reader));
    }

    public Vector3 Center => new((Lo.X + Hi.X) * 0.5f, (Lo.Y + Hi.Y) * 0.5f, (Lo.Z + Hi.Z) * 0.5f);

    public void Merge(Vector3 point)
    {
        Lo = Vector3.Min(Lo, point);
        Hi = Vector3.Max(Hi, point);
    }

    public void Merge(AABox other)
    {
        Lo = Vector3.Min(Lo, other.Lo);
        Hi = Vector3.Max(Hi, other.Hi);
    }

    /// <summary>
    /// Tests ray-AABB intersection using the slab method.
    /// Returns true if the ray intersects, with tMin/tMax set to the intersection interval.
    /// </summary>
    public bool IntersectRay(Vector3 origin, Vector3 direction, out float tMin, out float tMax)
    {
        tMin = float.NegativeInfinity;
        tMax = float.PositiveInfinity;

        for (int i = 0; i < 3; i++)
        {
            float d = direction[i];
            float o = origin[i];
            float lo = Lo[i];
            float hi = Hi[i];

            if (MathF.Abs(d) < 1e-10f)
            {
                if (o < lo || o > hi) return false;
            }
            else
            {
                float invD = 1.0f / d;
                float t1 = (lo - o) * invD;
                float t2 = (hi - o) * invD;
                if (t1 > t2) (t1, t2) = (t2, t1);
                if (t1 > tMin) tMin = t1;
                if (t2 < tMax) tMax = t2;
                if (tMin > tMax) return false;
            }
        }

        return tMax >= 0;
    }
}

/// <summary>
/// Ray definition for intersection testing.
/// </summary>
public struct Ray
{
    public Vector3 Origin;
    public Vector3 Direction;

    public Ray(Vector3 origin, Vector3 direction)
    {
        Origin = origin;
        Direction = direction;
    }

    public Vector3 GetPoint(float t) => Origin + Direction * t;
}
