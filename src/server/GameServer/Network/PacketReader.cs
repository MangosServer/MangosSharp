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

using System.Buffers.Binary;
using System.Text;

namespace GameServer.Network;

internal sealed class PacketReader
{
    private Memory<byte> data;

    public int Remaining => data.Length;

    public PacketReader(Memory<byte> data)
    {
        this.data = data;
    }

    public byte UInt8()
    {
        var value = data.Span[0];
        data = data.Slice(1);
        return value;
    }

    public ushort UInt16()
    {
        var value = BinaryPrimitives.ReadUInt16LittleEndian(data.Span);
        data = data.Slice(sizeof(ushort));
        return value;
    }

    public uint UInt32()
    {
        var value = BinaryPrimitives.ReadUInt32LittleEndian(data.Span);
        data = data.Slice(sizeof(uint));
        return value;
    }

    public ulong UInt64()
    {
        var value = BinaryPrimitives.ReadUInt64LittleEndian(data.Span);
        data = data.Slice(sizeof(ulong));
        return value;
    }

    public short Int16()
    {
        var value = BinaryPrimitives.ReadInt16LittleEndian(data.Span);
        data = data.Slice(sizeof(short));
        return value;
    }

    public int Int32()
    {
        var value = BinaryPrimitives.ReadInt32LittleEndian(data.Span);
        data = data.Slice(sizeof(int));
        return value;
    }

    public long Int64()
    {
        var value = BinaryPrimitives.ReadInt64LittleEndian(data.Span);
        data = data.Slice(sizeof(long));
        return value;
    }

    public float Float()
    {
        var value = BinaryPrimitives.ReadSingleLittleEndian(data.Span);
        data = data.Slice(sizeof(float));
        return value;
    }

    public string CString()
    {
        var span = data.Span;
        var length = span.IndexOf((byte)0);
        if (length < 0) length = span.Length;

        var value = Encoding.UTF8.GetString(span.Slice(0, length));
        data = data.Slice(Math.Min(length + 1, data.Length));
        return value;
    }

    public ReadOnlyMemory<byte> Bytes(int count)
    {
        var value = data.Slice(0, count);
        data = data.Slice(count);
        return value;
    }

    public void Skip(int count)
    {
        data = data.Slice(count);
    }
}
