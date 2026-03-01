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
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Mangos.DataStores;

public class DataStore
{
    private const int HeaderSize = 20;

    private byte[] data = Array.Empty<byte>();

    [Description("Header information: File type.")]
    public string Type { get; private set; } = string.Empty;

    [Description("Header information: Rows contained in the file.")]
    public int Rows { get; private set; }

    [Description("Header information: Columns for each row.")]
    public int Columns { get; private set; }

    [Description("Header information: Bytes occupied by each row.")]
    public int RowLength { get; private set; }

    [Description("Header information: Strings data block length.")]
    public int StringBlockLength { get; private set; }

    public bool IsLoaded => data.Length > 0;

    public async Task LoadFromFileAsync(string path)
    {
        Console.WriteLine($"[DBC] Loading DBC file: {path}");
        if (!File.Exists(path))
        {
            Console.WriteLine($"[DBC] ERROR: DBC file not found: {Path.GetFullPath(path)}");
            throw new FileNotFoundException($"DBC file not found: {path}", path);
        }

        data = await File.ReadAllBytesAsync(path);
        Console.WriteLine($"[DBC] Read {data.Length} bytes from {path}");

        if (data.Length < HeaderSize)
        {
            Console.WriteLine($"[DBC] ERROR: DBC file too small ({data.Length} bytes, minimum {HeaderSize}): {path}");
            throw new InvalidDataException($"DBC file too small ({data.Length} bytes): {path}");
        }

        Type = Encoding.ASCII.GetString(data, 0, 4);
        Rows = BitConverter.ToInt32(new ReadOnlySpan<byte>(data, 4, 4));
        Columns = BitConverter.ToInt32(new ReadOnlySpan<byte>(data, 8, 4));
        RowLength = BitConverter.ToInt32(new ReadOnlySpan<byte>(data, 12, 4));
        StringBlockLength = BitConverter.ToInt32(new ReadOnlySpan<byte>(data, 16, 4));

        Console.WriteLine($"[DBC] Header parsed: Type={Type}, Rows={Rows}, Columns={Columns}, RowLength={RowLength}, StringBlockLength={StringBlockLength}");

        var expectedMinSize = HeaderSize + (Rows * RowLength) + StringBlockLength;
        if (data.Length < expectedMinSize)
        {
            Console.WriteLine($"[DBC] ERROR: DBC file truncated: expected {expectedMinSize} bytes, got {data.Length}: {path}");
            throw new InvalidDataException(
                $"DBC file {path} is truncated: expected at least {expectedMinSize} bytes, got {data.Length}");
        }
        Console.WriteLine($"[DBC] File loaded successfully: {Path.GetFileName(path)} ({Rows} rows, {Columns} columns)");
    }

    public int ReadInt(int row, int column)
    {
        return BitConverter.ToInt32(data, GetOffset(row, column));
    }

    public float ReadFloat(int row, int column)
    {
        return BitConverter.ToSingle(data, GetOffset(row, column));
    }

    public string ReadString(int row, int column)
    {
        var stringBlockStart = HeaderSize + (Rows * RowLength);
        var stringOffset = ReadInt(row, column);
        var offset = stringBlockStart + stringOffset;

        if (offset < 0 || offset >= data.Length)
        {
            return string.Empty;
        }

        var nullIndex = Array.IndexOf<byte>(data, 0, offset);
        var length = (nullIndex >= 0 ? nullIndex : data.Length) - offset;

        return length <= 0 ? string.Empty : Encoding.UTF8.GetString(data, offset, length);
    }

    private int GetOffset(int row, int column)
    {
        if (column < 0 || column >= Columns)
        {
            throw new ArgumentOutOfRangeException(nameof(column),
                $"DBC: Column index {column} outside file definition (0-{Columns - 1}).");
        }
        return GetRowOffset(row) + (column * 4);
    }

    private int GetRowOffset(int row)
    {
        if (row < 0 || row >= Rows)
        {
            throw new ArgumentOutOfRangeException(nameof(row),
                $"DBC: Row index {row} outside file definition (0-{Rows - 1}).");
        }
        return HeaderSize + (row * RowLength);
    }
}
