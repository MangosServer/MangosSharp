//
// Copyright (C) 2013-2022 getMaNGOS <https://getmangos.eu>
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
    private byte[] data;

    [Description("Header information: File type.")]
    public string Type { get; private set; }

    [Description("Header information: Rows contained in the file.")]
    public int Rows { get; private set; }

    [Description("Header information: Columns for each row.")]
    public int Columns { get; private set; }

    [Description("Header information: Bytes ocupied by each row.")]
    public int RowLength { get; private set; }

    [Description("Header information: Strings data block length.")]
    public int StringBlockLength { get; private set; }

    public async Task LoadFromFileAsync(string path)
    {
        data = await File.ReadAllBytesAsync(path);

        Type = Encoding.ASCII.GetString(data, 0, 4);
        Rows = BitConverter.ToInt32(new ReadOnlySpan<byte>(data, 4, 4));
        Columns = BitConverter.ToInt32(new ReadOnlySpan<byte>(data, 8, 4));
        RowLength = BitConverter.ToInt32(new ReadOnlySpan<byte>(data, 12, 4));
        StringBlockLength = BitConverter.ToInt32(new ReadOnlySpan<byte>(data, 16, 4));
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
        var offset = GetRowOffset(row) + ReadInt(row, column);
        var length = Array.IndexOf<byte>(data, 0, offset) - offset;
        return BitConverter.ToString(data, offset, length);
    }

    private int GetOffset(int row, int column)
    {
        return column >= Columns
            ? throw new ApplicationException("DBC: Column index outside file definition.")
            : GetRowOffset(row) + (column * 4);
    }

    private int GetRowOffset(int row)
    {
        return row >= Rows ? throw new ApplicationException("DBC: Row index outside file definition.") : 20 + (row * RowLength);
    }
}
