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
using System.Text;

namespace Mangos.World.Maps;

/// <summary>
/// MangosZero-compatible map file format definitions.
/// These match the binary layout produced by MangosZero's map extractor tools,
/// allowing maps extracted with MangosZero tools to be used with MangosSharp.
/// </summary>
public static class MapFileFormats
{
    // Grid dimensions
    public const int V9_SIZE = 129;          // Height map vertex grid (corners)
    public const int V9_SIZE_SQ = V9_SIZE * V9_SIZE;
    public const int V8_SIZE = 128;          // Height map cell center grid
    public const int V8_SIZE_SQ = V8_SIZE * V8_SIZE;
    public const float GRID_SIZE = 533.33333f;
    public const float GRID_PART = GRID_SIZE / V8_SIZE;
    public const int AREA_SIZE = 16;         // Area data grid
    public const int LIQUID_SIZE = 128;      // Max liquid grid

    // FourCC magic values (little-endian uint32 representation)
    public const uint MAP_MAGIC = 0x5350414D;         // "MAPS"
    public const uint MAP_VERSION_MAGIC_V9 = 0x302E3976;  // "v9.0" (older builds)
    public const uint MAP_VERSION_MAGIC = 0x352E317A;  // "z1.5" (MangosZero current)
    public const uint MAP_AREA_MAGIC = 0x41455241;     // "AREA"
    public const uint MAP_HEIGHT_MAGIC = 0x5447484D;   // "MHGT"
    public const uint MAP_LIQUID_MAGIC = 0x51494C4D;   // "MLIQ"

    // Area header flags
    public const ushort MAP_AREA_NO_AREA = 0x0001;

    // Height header flags
    public const uint MAP_HEIGHT_NO_HEIGHT = 0x0001;
    public const uint MAP_HEIGHT_AS_INT16 = 0x0002;
    public const uint MAP_HEIGHT_AS_INT8 = 0x0004;

    // Liquid header flags
    public const ushort MAP_LIQUID_NO_TYPE = 0x0001;
    public const ushort MAP_LIQUID_NO_HEIGHT = 0x0002;

    // VMAP magic
    public const string VMAP_MAGIC = "VMAP004";

    // MMAP constants
    public const uint MMAP_MAGIC = 0x4D4D4150;   // "MMAP"
    public const uint MMAP_VERSION = 4;
    public const uint DT_NAVMESH_VERSION = 7;     // Detour navmesh version for MangosZero

    /// <summary>
    /// Main map file header - first 44 bytes of every .map file.
    /// Binary layout matches MangosZero's GridMapFileHeader in GridMap.h.
    /// </summary>
    public struct MapFileHeader
    {
        public uint MapMagic;          // Must be MAP_MAGIC ("MAPS")
        public uint VersionMagic;      // Must be MAP_VERSION_MAGIC ("v9.0")
        public uint BuildMagic;        // Client build number
        public uint AreaMapOffset;     // Byte offset to area data section
        public uint AreaMapSize;       // Size of area data section
        public uint HeightMapOffset;   // Byte offset to height data section
        public uint HeightMapSize;     // Size of height data section
        public uint LiquidMapOffset;   // Byte offset to liquid data section
        public uint LiquidMapSize;     // Size of liquid data section
        public uint HolesOffset;       // Byte offset to holes data section
        public uint HolesSize;         // Size of holes data section

        public static MapFileHeader Read(BinaryReader reader)
        {
            return new MapFileHeader
            {
                MapMagic = reader.ReadUInt32(),
                VersionMagic = reader.ReadUInt32(),
                BuildMagic = reader.ReadUInt32(),
                AreaMapOffset = reader.ReadUInt32(),
                AreaMapSize = reader.ReadUInt32(),
                HeightMapOffset = reader.ReadUInt32(),
                HeightMapSize = reader.ReadUInt32(),
                LiquidMapOffset = reader.ReadUInt32(),
                LiquidMapSize = reader.ReadUInt32(),
                HolesOffset = reader.ReadUInt32(),
                HolesSize = reader.ReadUInt32()
            };
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(MapMagic);
            writer.Write(VersionMagic);
            writer.Write(BuildMagic);
            writer.Write(AreaMapOffset);
            writer.Write(AreaMapSize);
            writer.Write(HeightMapOffset);
            writer.Write(HeightMapSize);
            writer.Write(LiquidMapOffset);
            writer.Write(LiquidMapSize);
            writer.Write(HolesOffset);
            writer.Write(HolesSize);
        }

        public bool IsValid => MapMagic == MAP_MAGIC &&
            (VersionMagic == MAP_VERSION_MAGIC || VersionMagic == MAP_VERSION_MAGIC_V9);
    }

    /// <summary>
    /// Area data section header.
    /// </summary>
    public struct MapAreaHeader
    {
        public uint FourCC;       // Must be MAP_AREA_MAGIC ("AREA")
        public ushort Flags;
        public ushort GridArea;   // Default area ID when MAP_AREA_NO_AREA is set

        public static MapAreaHeader Read(BinaryReader reader)
        {
            return new MapAreaHeader
            {
                FourCC = reader.ReadUInt32(),
                Flags = reader.ReadUInt16(),
                GridArea = reader.ReadUInt16()
            };
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(FourCC);
            writer.Write(Flags);
            writer.Write(GridArea);
        }

        public bool HasNoArea => (Flags & MAP_AREA_NO_AREA) != 0;
    }

    /// <summary>
    /// Height data section header.
    /// </summary>
    public struct MapHeightHeader
    {
        public uint FourCC;          // Must be MAP_HEIGHT_MAGIC ("MHGT")
        public uint Flags;
        public float GridHeight;     // Base/flat height when MAP_HEIGHT_NO_HEIGHT
        public float GridMaxHeight;  // Max height for compression range

        public static MapHeightHeader Read(BinaryReader reader)
        {
            return new MapHeightHeader
            {
                FourCC = reader.ReadUInt32(),
                Flags = reader.ReadUInt32(),
                GridHeight = reader.ReadSingle(),
                GridMaxHeight = reader.ReadSingle()
            };
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(FourCC);
            writer.Write(Flags);
            writer.Write(GridHeight);
            writer.Write(GridMaxHeight);
        }

        public bool HasNoHeight => (Flags & MAP_HEIGHT_NO_HEIGHT) != 0;
        public bool IsInt16 => (Flags & MAP_HEIGHT_AS_INT16) != 0;
        public bool IsInt8 => (Flags & MAP_HEIGHT_AS_INT8) != 0;
    }

    /// <summary>
    /// Liquid data section header.
    /// </summary>
    public struct MapLiquidHeader
    {
        public uint FourCC;        // Must be MAP_LIQUID_MAGIC ("MLIQ")
        public ushort Flags;
        public ushort LiquidType;  // Global liquid type
        public byte OffsetX;       // Start X offset in 128x128 grid
        public byte OffsetY;       // Start Y offset in 128x128 grid
        public byte Width;         // Liquid data width
        public byte Height;        // Liquid data height
        public float LiquidLevel;  // Base liquid level

        public static MapLiquidHeader Read(BinaryReader reader)
        {
            return new MapLiquidHeader
            {
                FourCC = reader.ReadUInt32(),
                Flags = reader.ReadUInt16(),
                LiquidType = reader.ReadUInt16(),
                OffsetX = reader.ReadByte(),
                OffsetY = reader.ReadByte(),
                Width = reader.ReadByte(),
                Height = reader.ReadByte(),
                LiquidLevel = reader.ReadSingle()
            };
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(FourCC);
            writer.Write(Flags);
            writer.Write(LiquidType);
            writer.Write(OffsetX);
            writer.Write(OffsetY);
            writer.Write(Width);
            writer.Write(Height);
            writer.Write(LiquidLevel);
        }

        public bool HasNoType => (Flags & MAP_LIQUID_NO_TYPE) != 0;
        public bool HasNoHeight => (Flags & MAP_LIQUID_NO_HEIGHT) != 0;
    }

    /// <summary>
    /// MMap tile header for navigation mesh tiles (20 bytes).
    /// Binary layout matches MangosZero's MmapTileHeader in MoveMapSharedDefines.h.
    /// </summary>
    public struct MmapTileHeader
    {
        public uint MmapMagic;     // Must be MMAP_MAGIC (0x4D4D4150)
        public uint DtVersion;     // Must be DT_NAVMESH_VERSION (Detour's navmesh version)
        public uint MmapVersion;   // Must be MMAP_VERSION (4)
        public uint Size;          // Size of dtNavMesh tile data following this header
        public uint UsesLiquids;   // Whether this tile includes liquid navigation data

        public static MmapTileHeader Read(BinaryReader reader)
        {
            return new MmapTileHeader
            {
                MmapMagic = reader.ReadUInt32(),
                DtVersion = reader.ReadUInt32(),
                MmapVersion = reader.ReadUInt32(),
                Size = reader.ReadUInt32(),
                UsesLiquids = reader.ReadUInt32()
            };
        }

        public bool IsValid => MmapMagic == MMAP_MAGIC && MmapVersion == MMAP_VERSION;
    }

    /// <summary>
    /// Builds the MangosZero-format filename for a map tile.
    /// MangosZero uses: {mapId:D4}{tileY:D2}{tileX:D2}.map
    /// Note: MangosZero uses Y,X order in the filename (not X,Y).
    /// </summary>
    public static string GetMapFileName(uint mapId, byte tileX, byte tileY)
    {
        return $"{mapId:D4}{tileY:D2}{tileX:D2}.map";
    }

    /// <summary>
    /// Gets the VMAP tree file path for a map.
    /// </summary>
    public static string GetVMapTreeFileName(uint mapId)
    {
        return $"{mapId:D4}.vmtree";
    }

    /// <summary>
    /// Gets the VMAP tile file path for a map tile.
    /// </summary>
    public static string GetVMapTileFileName(uint mapId, byte tileX, byte tileY)
    {
        return $"{mapId:D4}{tileY:D2}{tileX:D2}.vmtile";
    }

    /// <summary>
    /// Gets the MMap parameter file path for a map.
    /// </summary>
    public static string GetMMapFileName(uint mapId)
    {
        return $"{mapId:D4}.mmap";
    }

    /// <summary>
    /// Gets the MMap tile file path for a map tile.
    /// </summary>
    public static string GetMMapTileFileName(uint mapId, byte tileX, byte tileY)
    {
        return $"{mapId:D4}{tileY:D2}{tileX:D2}.mmtile";
    }

    /// <summary>
    /// Detects whether a map file is in MangosZero format by checking the magic number.
    /// </summary>
    public static bool IsMangosZeroFormat(string filePath)
    {
        if (!File.Exists(filePath)) return false;
        try
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new BinaryReader(fs);
            if (fs.Length < 4) return false;
            var magic = reader.ReadUInt32();
            return magic == MAP_MAGIC;
        }
        catch
        {
            return false;
        }
    }
}
