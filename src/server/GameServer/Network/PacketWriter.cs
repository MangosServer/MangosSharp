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

internal sealed class PacketWriter
{
    private readonly Memory<byte> buffer;
    private int offset = 4;

    public int Length => offset;

    public PacketWriter(Memory<byte> buffer, Opcodes opcode)
    {
        this.buffer = buffer;
        var span = buffer.Slice(2).Span;
        BinaryPrimitives.WriteUInt16LittleEndian(span, (ushort)opcode);
    }

    public Memory<byte> ToPacket()
    {
        var span = buffer.Span;
        BinaryPrimitives.WriteUInt16BigEndian(span, (ushort)(offset - 2));
        return buffer.Slice(0, offset);
    }

    public void UInt8(byte value)
    {
        buffer.Span[offset] = value;
        offset += 1;
    }

    public void UInt16(ushort value)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(offset).Span, value);
        offset += sizeof(ushort);
    }

    public void UInt32(uint value)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(buffer.Slice(offset).Span, value);
        offset += sizeof(uint);
    }

    public void UInt64(ulong value)
    {
        BinaryPrimitives.WriteUInt64LittleEndian(buffer.Slice(offset).Span, value);
        offset += sizeof(ulong);
    }

    public void Int16(short value)
    {
        BinaryPrimitives.WriteInt16LittleEndian(buffer.Slice(offset).Span, value);
        offset += sizeof(short);
    }

    public void Int32(int value)
    {
        BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(offset).Span, value);
        offset += sizeof(int);
    }

    public void Int64(long value)
    {
        BinaryPrimitives.WriteInt64LittleEndian(buffer.Slice(offset).Span, value);
        offset += sizeof(long);
    }

    public void Float(float value)
    {
        BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(offset).Span, value);
        offset += sizeof(float);
    }

    public void CString(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        bytes.CopyTo(buffer.Slice(offset));
        offset += bytes.Length;
        buffer.Span[offset] = 0;
        offset += 1;
    }

    public void Bytes(ReadOnlySpan<byte> value)
    {
        value.CopyTo(buffer.Slice(offset).Span);
        offset += value.Length;
    }
}
