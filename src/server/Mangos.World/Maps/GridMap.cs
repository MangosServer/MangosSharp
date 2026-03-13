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

namespace Mangos.World.Maps;

/// <summary>
/// C# port of MangosZero's GridMap class.
/// Reads MangosZero-format .map files (binary with MAPS/AREA/MHGT/MLIQ sections)
/// and provides height, area, liquid, and terrain queries with proper triangle interpolation.
/// </summary>
public class GridMap : IDisposable
{
    // Grid constants matching MangosZero's GridDefines.h
    private const int MAP_RESOLUTION = 128;
    private const float SIZE_OF_GRIDS = 533.33333f;
    private const float INVALID_HEIGHT_VALUE = -200000.0f;

    // Hole lookup tables matching MangosZero's static arrays
    private static readonly ushort[] HoleTabH = { 0x1111, 0x2222, 0x4444, 0x8888 };
    private static readonly ushort[] HoleTabV = { 0x000F, 0x00F0, 0x0F00, 0xF000 };

    // Liquid type constants
    public const byte MAP_LIQUID_TYPE_NO_WATER = 0x00;
    public const byte MAP_LIQUID_TYPE_MAGMA = 0x01;
    public const byte MAP_LIQUID_TYPE_OCEAN = 0x02;
    public const byte MAP_LIQUID_TYPE_SLIME = 0x04;
    public const byte MAP_LIQUID_TYPE_WATER = 0x08;
    public const byte MAP_ALL_LIQUIDS = MAP_LIQUID_TYPE_WATER | MAP_LIQUID_TYPE_MAGMA | MAP_LIQUID_TYPE_OCEAN | MAP_LIQUID_TYPE_SLIME;
    public const byte MAP_LIQUID_TYPE_DARK_WATER = 0x10;

    // Liquid status enum values
    public const uint LIQUID_MAP_NO_WATER = 0x00000000;
    public const uint LIQUID_MAP_ABOVE_WATER = 0x00000001;
    public const uint LIQUID_MAP_WATER_WALK = 0x00000002;
    public const uint LIQUID_MAP_IN_WATER = 0x00000004;
    public const uint LIQUID_MAP_UNDER_WATER = 0x00000008;

    // Height storage type
    private enum HeightDataType
    {
        Flat,
        Float,
        UInt16,
        UInt8
    }

    // Hole data
    private readonly ushort[,] m_holes = new ushort[16, 16];

    // Area data
    private ushort m_gridArea;
    private ushort[] m_areaMap;

    // Height data
    private float m_gridHeight = INVALID_HEIGHT_VALUE;
    private float m_gridIntHeightMultiplier;
    private HeightDataType m_heightType = HeightDataType.Flat;
    private float[] m_V9_float;
    private float[] m_V8_float;
    private ushort[] m_V9_uint16;
    private ushort[] m_V8_uint16;
    private byte[] m_V9_uint8;
    private byte[] m_V8_uint8;

    // Liquid data
    private ushort m_liquidType;
    private byte m_liquidOffX;
    private byte m_liquidOffY;
    private byte m_liquidWidth;
    private byte m_liquidHeight;
    private float m_liquidLevel = INVALID_HEIGHT_VALUE;
    private ushort[] m_liquidEntry;
    private byte[] m_liquidFlags;
    private float[] m_liquidMap;

    private bool m_loaded;
    private bool m_disposed;

    public bool IsLoaded => m_loaded;

    public bool LoadData(string filename)
    {
        UnloadData();

        if (!File.Exists(filename))
        {
            return true; // Not an error if file doesn't exist (matches C++ behavior)
        }

        try
        {
            using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new BinaryReader(fs);

            var header = MapFileFormats.MapFileHeader.Read(reader);
            if (!header.IsValid)
            {
                return false;
            }

            // Load area data
            if (header.AreaMapOffset != 0 && !LoadAreaData(reader, header.AreaMapOffset))
            {
                return false;
            }

            // Load holes data
            if (header.HolesOffset != 0 && !LoadHolesData(reader, header.HolesOffset))
            {
                return false;
            }

            // Load height data
            if (header.HeightMapOffset != 0 && !LoadHeightData(reader, header.HeightMapOffset))
            {
                return false;
            }

            // Load liquid data
            if (header.LiquidMapOffset != 0 && !LoadLiquidData(reader, header.LiquidMapOffset))
            {
                return false;
            }

            m_loaded = true;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void UnloadData()
    {
        m_areaMap = null;
        m_V9_float = null;
        m_V8_float = null;
        m_V9_uint16 = null;
        m_V8_uint16 = null;
        m_V9_uint8 = null;
        m_V8_uint8 = null;
        m_liquidEntry = null;
        m_liquidFlags = null;
        m_liquidMap = null;
        m_heightType = HeightDataType.Flat;
        m_gridHeight = INVALID_HEIGHT_VALUE;
        m_loaded = false;
    }

    private bool LoadAreaData(BinaryReader reader, uint offset)
    {
        reader.BaseStream.Seek(offset, SeekOrigin.Begin);
        var header = MapFileFormats.MapAreaHeader.Read(reader);
        if (header.FourCC != MapFileFormats.MAP_AREA_MAGIC)
        {
            return false;
        }

        m_gridArea = header.GridArea;
        if (!header.HasNoArea)
        {
            m_areaMap = new ushort[16 * 16];
            for (int i = 0; i < 16 * 16; i++)
            {
                m_areaMap[i] = reader.ReadUInt16();
            }
        }

        return true;
    }

    private bool LoadHolesData(BinaryReader reader, uint offset)
    {
        reader.BaseStream.Seek(offset, SeekOrigin.Begin);
        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                m_holes[i, j] = reader.ReadUInt16();
            }
        }
        return true;
    }

    private bool LoadHeightData(BinaryReader reader, uint offset)
    {
        reader.BaseStream.Seek(offset, SeekOrigin.Begin);
        var header = MapFileFormats.MapHeightHeader.Read(reader);
        if (header.FourCC != MapFileFormats.MAP_HEIGHT_MAGIC)
        {
            return false;
        }

        m_gridHeight = header.GridHeight;

        if (!header.HasNoHeight)
        {
            if (header.IsInt16)
            {
                m_V9_uint16 = new ushort[129 * 129];
                m_V8_uint16 = new ushort[128 * 128];
                for (int i = 0; i < 129 * 129; i++)
                {
                    m_V9_uint16[i] = reader.ReadUInt16();
                }
                for (int i = 0; i < 128 * 128; i++)
                {
                    m_V8_uint16[i] = reader.ReadUInt16();
                }
                m_gridIntHeightMultiplier = (header.GridMaxHeight - header.GridHeight) / 65535;
                m_heightType = HeightDataType.UInt16;
            }
            else if (header.IsInt8)
            {
                m_V9_uint8 = new byte[129 * 129];
                m_V8_uint8 = new byte[128 * 128];
                m_V9_uint8 = reader.ReadBytes(129 * 129);
                m_V8_uint8 = reader.ReadBytes(128 * 128);
                m_gridIntHeightMultiplier = (header.GridMaxHeight - header.GridHeight) / 255;
                m_heightType = HeightDataType.UInt8;
            }
            else
            {
                m_V9_float = new float[129 * 129];
                m_V8_float = new float[128 * 128];
                for (int i = 0; i < 129 * 129; i++)
                {
                    m_V9_float[i] = reader.ReadSingle();
                }
                for (int i = 0; i < 128 * 128; i++)
                {
                    m_V8_float[i] = reader.ReadSingle();
                }
                m_heightType = HeightDataType.Float;
            }
        }
        else
        {
            m_heightType = HeightDataType.Flat;
        }

        return true;
    }

    private bool LoadLiquidData(BinaryReader reader, uint offset)
    {
        reader.BaseStream.Seek(offset, SeekOrigin.Begin);
        var header = MapFileFormats.MapLiquidHeader.Read(reader);
        if (header.FourCC != MapFileFormats.MAP_LIQUID_MAGIC)
        {
            return false;
        }

        m_liquidType = header.LiquidType;
        m_liquidOffX = header.OffsetX;
        m_liquidOffY = header.OffsetY;
        m_liquidWidth = header.Width;
        m_liquidHeight = header.Height;
        m_liquidLevel = header.LiquidLevel;

        if (!header.HasNoType)
        {
            m_liquidEntry = new ushort[16 * 16];
            for (int i = 0; i < 16 * 16; i++)
            {
                m_liquidEntry[i] = reader.ReadUInt16();
            }

            m_liquidFlags = new byte[16 * 16];
            m_liquidFlags = reader.ReadBytes(16 * 16);
        }

        if (!header.HasNoHeight)
        {
            int count = m_liquidWidth * m_liquidHeight;
            m_liquidMap = new float[count];
            for (int i = 0; i < count; i++)
            {
                m_liquidMap[i] = reader.ReadSingle();
            }
        }

        return true;
    }

    /// <summary>
    /// Gets the area ID at the given world coordinates.
    /// Matches MangosZero GridMap::getArea.
    /// </summary>
    public ushort GetArea(float x, float y)
    {
        if (m_areaMap == null)
        {
            return m_gridArea;
        }

        x = 16 * (32 - x / SIZE_OF_GRIDS);
        y = 16 * (32 - y / SIZE_OF_GRIDS);
        int lx = (int)x & 15;
        int ly = (int)y & 15;
        return m_areaMap[lx * 16 + ly];
    }

    /// <summary>
    /// Gets the height at the given world coordinates using triangle interpolation.
    /// Dispatches to the appropriate method based on height data storage type.
    /// </summary>
    public float GetHeight(float x, float y)
    {
        return m_heightType switch
        {
            HeightDataType.Float => GetHeightFromFloat(x, y),
            HeightDataType.UInt16 => GetHeightFromUint16(x, y),
            HeightDataType.UInt8 => GetHeightFromUint8(x, y),
            _ => m_gridHeight
        };
    }

    private bool IsHole(int row, int col)
    {
        int cellRow = row / 8;
        int cellCol = col / 8;
        int holeRow = row % 8 / 2;
        int holeCol = (col - (cellCol * 8)) / 2;

        ushort hole = m_holes[cellRow, cellCol];
        return (hole & HoleTabH[holeCol] & HoleTabV[holeRow]) != 0;
    }

    /// <summary>
    /// Height interpolation from float-stored V9/V8 data.
    /// Uses 4-triangle decomposition of each grid cell with bilinear interpolation.
    /// Matches MangosZero GridMap::getHeightFromFloat exactly.
    /// </summary>
    private float GetHeightFromFloat(float x, float y)
    {
        if (m_V8_float == null || m_V9_float == null)
        {
            return INVALID_HEIGHT_VALUE;
        }

        x = MAP_RESOLUTION * (32 - x / SIZE_OF_GRIDS);
        y = MAP_RESOLUTION * (32 - y / SIZE_OF_GRIDS);

        int xInt = (int)x;
        int yInt = (int)y;
        x -= xInt;
        y -= yInt;
        xInt &= (MAP_RESOLUTION - 1);
        yInt &= (MAP_RESOLUTION - 1);

        if (IsHole(xInt, yInt))
        {
            return INVALID_HEIGHT_VALUE;
        }

        // Height stored as: h5 - its V8 grid, h1-h4 - its V9 grid
        // +--------------> X
        // | h1-------h2     h1 = V9[x,   y  ]
        // | | \  1  / |     h2 = V9[x+1, y  ]
        // | |  \   /  |     h3 = V9[x,   y+1]
        // | | 2  h5 3 |     h4 = V9[x+1, y+1]
        // | |  /   \  |     h5 = V8[x,   y  ]
        // | | /  4  \ |
        // | h3-------h4
        // V Y
        // Solve h = a*x + b*y + c for the appropriate triangle

        float a, b, c;
        if (x + y < 1)
        {
            if (x > y)
            {
                // Triangle 1 (h1, h2, h5)
                float h1 = m_V9_float[xInt * 129 + yInt];
                float h2 = m_V9_float[(xInt + 1) * 129 + yInt];
                float h5 = 2 * m_V8_float[xInt * 128 + yInt];
                a = h2 - h1;
                b = h5 - h1 - h2;
                c = h1;
            }
            else
            {
                // Triangle 2 (h1, h3, h5)
                float h1 = m_V9_float[xInt * 129 + yInt];
                float h3 = m_V9_float[xInt * 129 + yInt + 1];
                float h5 = 2 * m_V8_float[xInt * 128 + yInt];
                a = h5 - h1 - h3;
                b = h3 - h1;
                c = h1;
            }
        }
        else
        {
            if (x > y)
            {
                // Triangle 3 (h2, h4, h5)
                float h2 = m_V9_float[(xInt + 1) * 129 + yInt];
                float h4 = m_V9_float[(xInt + 1) * 129 + yInt + 1];
                float h5 = 2 * m_V8_float[xInt * 128 + yInt];
                a = h2 + h4 - h5;
                b = h4 - h2;
                c = h5 - h4;
            }
            else
            {
                // Triangle 4 (h3, h4, h5)
                float h3 = m_V9_float[xInt * 129 + yInt + 1];
                float h4 = m_V9_float[(xInt + 1) * 129 + yInt + 1];
                float h5 = 2 * m_V8_float[xInt * 128 + yInt];
                a = h4 - h3;
                b = h3 + h4 - h5;
                c = h5 - h4;
            }
        }

        return a * x + b * y + c;
    }

    /// <summary>
    /// Height interpolation from uint16-compressed V9/V8 data.
    /// Same triangle logic as float variant, with decompression.
    /// Matches MangosZero GridMap::getHeightFromUint16.
    /// </summary>
    private float GetHeightFromUint16(float x, float y)
    {
        if (m_V8_uint16 == null || m_V9_uint16 == null)
        {
            return m_gridHeight;
        }

        x = MAP_RESOLUTION * (32 - x / SIZE_OF_GRIDS);
        y = MAP_RESOLUTION * (32 - y / SIZE_OF_GRIDS);

        int xInt = (int)x;
        int yInt = (int)y;
        x -= xInt;
        y -= yInt;
        xInt &= (MAP_RESOLUTION - 1);
        yInt &= (MAP_RESOLUTION - 1);

        // V9_h1_ptr = &m_uint16_V9[x_int * 128 + x_int + y_int]
        // which is m_uint16_V9[x_int * 129 + y_int]
        int v9Base = xInt * 129 + yInt;

        int a, b, c;
        if (x + y < 1)
        {
            if (x > y)
            {
                // Triangle 1
                int h1 = m_V9_uint16[v9Base];
                int h2 = m_V9_uint16[v9Base + 129];
                int h5 = 2 * m_V8_uint16[xInt * 128 + yInt];
                a = h2 - h1;
                b = h5 - h1 - h2;
                c = h1;
            }
            else
            {
                // Triangle 2
                int h1 = m_V9_uint16[v9Base];
                int h3 = m_V9_uint16[v9Base + 1];
                int h5 = 2 * m_V8_uint16[xInt * 128 + yInt];
                a = h5 - h1 - h3;
                b = h3 - h1;
                c = h1;
            }
        }
        else
        {
            if (x > y)
            {
                // Triangle 3
                int h2 = m_V9_uint16[v9Base + 129];
                int h4 = m_V9_uint16[v9Base + 130];
                int h5 = 2 * m_V8_uint16[xInt * 128 + yInt];
                a = h2 + h4 - h5;
                b = h4 - h2;
                c = h5 - h4;
            }
            else
            {
                // Triangle 4
                int h3 = m_V9_uint16[v9Base + 1];
                int h4 = m_V9_uint16[v9Base + 130];
                int h5 = 2 * m_V8_uint16[xInt * 128 + yInt];
                a = h4 - h3;
                b = h3 + h4 - h5;
                c = h5 - h4;
            }
        }

        return (float)((a * x) + (b * y) + c) * m_gridIntHeightMultiplier + m_gridHeight;
    }

    /// <summary>
    /// Height interpolation from uint8-compressed V9/V8 data.
    /// Same triangle logic as float variant, with decompression.
    /// Matches MangosZero GridMap::getHeightFromUint8.
    /// </summary>
    private float GetHeightFromUint8(float x, float y)
    {
        if (m_V8_uint8 == null || m_V9_uint8 == null)
        {
            return m_gridHeight;
        }

        x = MAP_RESOLUTION * (32 - x / SIZE_OF_GRIDS);
        y = MAP_RESOLUTION * (32 - y / SIZE_OF_GRIDS);

        int xInt = (int)x;
        int yInt = (int)y;
        x -= xInt;
        y -= yInt;
        xInt &= (MAP_RESOLUTION - 1);
        yInt &= (MAP_RESOLUTION - 1);

        int v9Base = xInt * 129 + yInt;

        int a, b, c;
        if (x + y < 1)
        {
            if (x > y)
            {
                // Triangle 1
                int h1 = m_V9_uint8[v9Base];
                int h2 = m_V9_uint8[v9Base + 129];
                int h5 = 2 * m_V8_uint8[xInt * 128 + yInt];
                a = h2 - h1;
                b = h5 - h1 - h2;
                c = h1;
            }
            else
            {
                // Triangle 2
                int h1 = m_V9_uint8[v9Base];
                int h3 = m_V9_uint8[v9Base + 1];
                int h5 = 2 * m_V8_uint8[xInt * 128 + yInt];
                a = h5 - h1 - h3;
                b = h3 - h1;
                c = h1;
            }
        }
        else
        {
            if (x > y)
            {
                // Triangle 3
                int h2 = m_V9_uint8[v9Base + 129];
                int h4 = m_V9_uint8[v9Base + 130];
                int h5 = 2 * m_V8_uint8[xInt * 128 + yInt];
                a = h2 + h4 - h5;
                b = h4 - h2;
                c = h5 - h4;
            }
            else
            {
                // Triangle 4
                int h3 = m_V9_uint8[v9Base + 1];
                int h4 = m_V9_uint8[v9Base + 130];
                int h5 = 2 * m_V8_uint8[xInt * 128 + yInt];
                a = h4 - h3;
                b = h3 + h4 - h5;
                c = h5 - h4;
            }
        }

        return (float)((a * x) + (b * y) + c) * m_gridIntHeightMultiplier + m_gridHeight;
    }

    /// <summary>
    /// Gets the liquid surface level at the given world coordinates.
    /// Matches MangosZero GridMap::getLiquidLevel.
    /// </summary>
    public float GetLiquidLevel(float x, float y)
    {
        if (m_liquidMap == null)
        {
            return m_liquidLevel;
        }

        x = MAP_RESOLUTION * (32 - x / SIZE_OF_GRIDS);
        y = MAP_RESOLUTION * (32 - y / SIZE_OF_GRIDS);

        int cxInt = ((int)x & (MAP_RESOLUTION - 1)) - m_liquidOffY;
        int cyInt = ((int)y & (MAP_RESOLUTION - 1)) - m_liquidOffX;

        if (cxInt < 0 || cxInt >= m_liquidHeight)
        {
            return INVALID_HEIGHT_VALUE;
        }
        if (cyInt < 0 || cyInt >= m_liquidWidth)
        {
            return INVALID_HEIGHT_VALUE;
        }

        return m_liquidMap[cxInt * m_liquidWidth + cyInt];
    }

    /// <summary>
    /// Gets the terrain type (liquid flags) at the given world coordinates.
    /// Matches MangosZero GridMap::getTerrainType.
    /// </summary>
    public byte GetTerrainType(float x, float y)
    {
        if (m_liquidFlags == null)
        {
            return (byte)m_liquidType;
        }

        x = 16 * (32 - x / SIZE_OF_GRIDS);
        y = 16 * (32 - y / SIZE_OF_GRIDS);
        int lx = (int)x & 15;
        int ly = (int)y & 15;
        return m_liquidFlags[lx * 16 + ly];
    }

    /// <summary>
    /// Liquid status data returned by GetLiquidStatus.
    /// </summary>
    public struct LiquidData
    {
        public uint TypeFlags;
        public uint Entry;
        public float Level;
        public float DepthLevel;
    }

    /// <summary>
    /// Gets the liquid status at the given world coordinates.
    /// Matches MangosZero GridMap::getLiquidStatus.
    /// </summary>
    public uint GetLiquidStatus(float x, float y, float z, byte reqLiquidType, out LiquidData data)
    {
        data = default;

        // Check water type (if no water return)
        if (m_liquidFlags == null && m_liquidType == 0)
        {
            return LIQUID_MAP_NO_WATER;
        }

        // Get cell
        float cx = MAP_RESOLUTION * (32 - x / SIZE_OF_GRIDS);
        float cy = MAP_RESOLUTION * (32 - y / SIZE_OF_GRIDS);

        int xInt = (int)cx & (MAP_RESOLUTION - 1);
        int yInt = (int)cy & (MAP_RESOLUTION - 1);

        // Check water type in cell
        int idx = (xInt >> 3) * 16 + (yInt >> 3);
        byte type = m_liquidFlags != null ? m_liquidFlags[idx] : (byte)(1 << m_liquidType);
        uint entry = 0;

        if (m_liquidEntry != null)
        {
            entry = m_liquidEntry[idx];
            // Simplified: in full MangosZero this does DBC lookups for liquid type overrides.
            // For compatibility we pass through the entry and type as-is from the map data.
        }

        if (type == 0)
        {
            return LIQUID_MAP_NO_WATER;
        }

        // Check req liquid type mask
        if (reqLiquidType != 0 && (reqLiquidType & type) == 0)
        {
            return LIQUID_MAP_NO_WATER;
        }

        // Check water level
        int lxInt = xInt - m_liquidOffY;
        if (lxInt < 0 || lxInt >= m_liquidHeight)
        {
            return LIQUID_MAP_NO_WATER;
        }

        int lyInt = yInt - m_liquidOffX;
        if (lyInt < 0 || lyInt >= m_liquidWidth)
        {
            return LIQUID_MAP_NO_WATER;
        }

        // Get water level
        float liquidLevel = m_liquidMap != null
            ? m_liquidMap[lxInt * m_liquidWidth + lyInt]
            : m_liquidLevel;

        // Get ground level (sub 0.2 for fix some errors)
        float groundLevel = GetHeight(x, y);

        // Check water level and ground level
        if (liquidLevel < groundLevel || z < groundLevel - 2)
        {
            return LIQUID_MAP_NO_WATER;
        }

        // Store data
        data.Entry = entry;
        data.TypeFlags = type;
        data.Level = liquidLevel;
        data.DepthLevel = groundLevel;

        // For speed check as int values
        int delta = (int)((liquidLevel - z) * 10);

        if (delta > 20) return LIQUID_MAP_UNDER_WATER;
        if (delta > 0) return LIQUID_MAP_IN_WATER;
        if (delta > -1) return LIQUID_MAP_WATER_WALK;
        return LIQUID_MAP_ABOVE_WATER;
    }

    /// <summary>
    /// Checks if a MangosZero-format .map file exists and has valid headers.
    /// Matches MangosZero GridMap::ExistMap.
    /// </summary>
    public static bool ExistMap(string mapsDir, uint mapId, int gx, int gy)
    {
        string filename = Path.Combine(mapsDir, $"{mapId:D3}{gx:D2}{gy:D2}.map");

        if (!File.Exists(filename))
        {
            return false;
        }

        try
        {
            using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new BinaryReader(fs);
            if (fs.Length < 44) // Size of GridMapFileHeader
            {
                return false;
            }
            var header = MapFileFormats.MapFileHeader.Read(reader);
            return header.IsValid;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        if (!m_disposed)
        {
            UnloadData();
            m_disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
