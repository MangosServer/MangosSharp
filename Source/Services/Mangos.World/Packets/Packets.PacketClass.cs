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

using Mangos.Common.Globals;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections;
using System.Text;

namespace Mangos.World.Globals;

public partial class Packets
{
    public class PacketClass : IDisposable
    {
        public byte[] Data;

        public int Offset;

        private bool _disposedValue;

        public int Length
        {
            get
            {
                checked
                {
                    return Data[1] + (Data[0] * 256);
                }
            }
        }

        public Opcodes OpCode => Information.UBound(Data) > 2 ? (Opcodes)checked(Data[2] + (Data[3] * 256)) : Opcodes.MSG_NULL_ACTION;

        public PacketClass(Opcodes opcode)
        {
            Offset = 4;
            Data = new byte[4];
            Data[0] = 0;
            Data[1] = 0;
            checked
            {
                Data[2] = (byte)(checked((short)opcode) % 256);
                Data[3] = (byte)(checked((short)opcode) / 256);
            }
        }

        public PacketClass(ref byte[] rawdata)
        {
            Offset = 4;
            Data = rawdata;
            rawdata.CopyTo(Data, 0);
        }

        public void CompressUpdatePacket()
        {
            if (OpCode == Opcodes.SMSG_UPDATE_OBJECT && Data.Length >= 200)
            {
                var uncompressedSize = Data.Length;
                var compressedBuffer = WorldServiceLocator._GlobalZip.Compress(Data, 4, checked(Data.Length - 4));
                if (compressedBuffer.Length != 0)
                {
                    Data = new byte[4];
                    Data[0] = 0;
                    Data[1] = 0;
                    Data[2] = 246;
                    Data[3] = 1;
                    AddInt32(uncompressedSize);
                    AddByteArray(compressedBuffer);
                    UpdateLength();
                }
            }
        }

        public void AddBitArray(BitArray buffer, int arraryLen)
        {
            ref var data = ref Data;
            checked
            {
                data = (byte[])Utils.CopyArray(data, new byte[Data.Length - 1 + arraryLen + 1]);
                var bufferarray = new byte[checked((byte)Math.Round((buffer.Length + 8) / 8.0)) + 1];
                buffer.CopyTo(bufferarray, 0);
                Array.Copy(bufferarray, 0, Data, Data.Length - arraryLen, arraryLen);
            }
        }

        public void AddInt8(byte buffer, int position = 0)
        {
            if (position <= 0 || position >= Data.Length)
            {
                position = Data.Length;
                ref var data = ref Data;
                data = (byte[])Utils.CopyArray(data, new byte[checked(Data.Length + 1)]);
            }
            Data[position] = buffer;
        }

        public void AddInt16(short buffer, int position = 0)
        {
            checked
            {
                if (position <= 0 || position >= Data.Length)
                {
                    position = Data.Length;
                    ref var data = ref Data;
                    data = (byte[])Utils.CopyArray(data, new byte[Data.Length + 1 + 1]);
                }
                Data[position] = (byte)(buffer & 0xFF);
                Data[position + 1] = (byte)(unchecked((short)(buffer >> 8)) & 0xFF);
            }
        }

        public void AddInt32(int buffer, int position = 0)
        {
            checked
            {
                if (position <= 0 || position > Data.Length - 3)
                {
                    position = Data.Length;
                    ref var data = ref Data;
                    data = (byte[])Utils.CopyArray(data, new byte[Data.Length + 3 + 1]);
                }
                Data[position] = (byte)(buffer & 0xFF);
                Data[position + 1] = (byte)((buffer >> 8) & 0xFF);
                Data[position + 2] = (byte)((buffer >> 16) & 0xFF);
                Data[position + 3] = (byte)((buffer >> 24) & 0xFF);
            }
        }

        public void AddInt64(long buffer, int position = 0)
        {
            checked
            {
                if (position <= 0 || position > Data.Length - 7)
                {
                    position = Data.Length;
                    ref var data = ref Data;
                    data = (byte[])Utils.CopyArray(data, new byte[Data.Length + 7 + 1]);
                }
                Data[position] = (byte)(buffer & 0xFF);
                Data[position + 1] = (byte)((buffer >> 8) & 0xFF);
                Data[position + 2] = (byte)((buffer >> 16) & 0xFF);
                Data[position + 3] = (byte)((buffer >> 24) & 0xFF);
                Data[position + 4] = (byte)((buffer >> 32) & 0xFF);
                Data[position + 5] = (byte)((buffer >> 40) & 0xFF);
                Data[position + 6] = (byte)((buffer >> 48) & 0xFF);
                Data[position + 7] = (byte)((buffer >> 56) & 0xFF);
            }
        }

        public void AddString(string buffer)
        {
            if (Information.IsDBNull(buffer) | (Operators.CompareString(buffer, "", TextCompare: false) == 0))
            {
                AddInt8(0);
                return;
            }
            var bytes = Encoding.UTF8.GetBytes(buffer.ToCharArray());
            ref var data = ref Data;
            checked
            {
                data = (byte[])Utils.CopyArray(data, new byte[Data.Length + bytes.Length + 1]);
                var num = bytes.Length - 1;
                for (var i = 0; i <= num; i++)
                {
                    Data[Data.Length - 1 - bytes.Length + i] = bytes[i];
                }
                Data[^1] = 0;
            }
        }

        public void AddString2(string buffer)
        {
            if (Information.IsDBNull(buffer) | (Operators.CompareString(buffer, "", TextCompare: false) == 0))
            {
                AddInt8(0);
                return;
            }
            var bytes = Encoding.UTF8.GetBytes(buffer.ToCharArray());
            ref var data = ref Data;
            checked
            {
                data = (byte[])Utils.CopyArray(data, new byte[Data.Length + bytes.Length + 1]);
                Data[Data.Length - 1 - bytes.Length] = (byte)bytes.Length;
                var num = bytes.Length - 1;
                for (var i = 0; i <= num; i++)
                {
                    Data[Data.Length - bytes.Length + i] = bytes[i];
                }
            }
        }

        public void AddSingle(float buffer2)
        {
            var buffer3 = BitConverter.GetBytes(buffer2);
            ref var data = ref Data;
            checked
            {
                data = (byte[])Utils.CopyArray(data, new byte[Data.Length + buffer3.Length - 1 + 1]);
                Buffer.BlockCopy(buffer3, 0, Data, Data.Length - buffer3.Length, buffer3.Length);
            }
        }

        public void AddByteArray(byte[] buffer)
        {
            var tmp = Data.Length;
            ref var data = ref Data;
            data = (byte[])Utils.CopyArray(data, new byte[checked(Data.Length + buffer.Length - 1 + 1)]);
            Array.Copy(buffer, 0, Data, tmp, buffer.Length);
        }

        public void AddPackGUID(ulong buffer)
        {
            var guid = BitConverter.GetBytes(buffer);
            BitArray flags = new(8);
            var offsetStart = Data.Length;
            var offsetNewSize = offsetStart;
            byte j = 0;
            checked
            {
                do
                {
                    flags[j] = guid[j] != 0;
                    if (flags[j])
                    {
                        offsetNewSize++;
                    }
                    j = (byte)unchecked((uint)(j + 1));
                }
                while (j <= 7u);
                ref var data = ref Data;
                data = (byte[])Utils.CopyArray(data, new byte[offsetNewSize + 1]);
                flags.CopyTo(Data, offsetStart);
                offsetStart++;
                byte i = 0;
                do
                {
                    if (flags[i])
                    {
                        Data[offsetStart] = guid[i];
                        offsetStart++;
                    }
                    i = (byte)unchecked((uint)(i + 1));
                }
                while (i <= 7u);
            }
        }

        public void AddUInt16(ushort buffer)
        {
            ref var data = ref Data;
            checked
            {
                data = (byte[])Utils.CopyArray(data, new byte[Data.Length + 1 + 1]);
                Data[^2] = (byte)(buffer & 0xFF);
                Data[^1] = (byte)(unchecked((ushort)((uint)buffer >> 8)) & 0xFF);
            }
        }

        public void AddUInt32(uint buffer)
        {
            ref var data = ref Data;
            checked
            {
                data = (byte[])Utils.CopyArray(data, new byte[Data.Length + 3 + 1]);
                Data[^4] = (byte)(buffer & 0xFFL);
                Data[^3] = (byte)((buffer >> 8) & 0xFFL);
                Data[^2] = (byte)((buffer >> 16) & 0xFFL);
                Data[^1] = (byte)((buffer >> 24) & 0xFFL);
            }
        }

        public void AddUInt64(ulong buffer, int position = 0)
        {
            var dBuffer = BitConverter.GetBytes(buffer);
            var valueConverted = BitConverter.ToInt64(dBuffer, 0);
            AddInt64(valueConverted, position);
        }

        public void UpdateLength()
        {
            checked
            {
                if (!((Data[0] != 0) || (Data[1] != 0)))
                {
                    Data[0] = (byte)(checked(Data.Length - 2) / 256);
                    Data[1] = (byte)(checked(Data.Length - 2) % 256);
                }
            }
        }

        public byte GetInt8()
        {
            checked
            {
                Offset++;
                return Data[Offset - 1];
            }
        }

        public short GetInt16()
        {
            var num1 = BitConverter.ToInt16(Data, Offset);
            checked
            {
                Offset += 2;
                return num1;
            }
        }

        public int GetInt32()
        {
            var num1 = BitConverter.ToInt32(Data, Offset);
            checked
            {
                Offset += 4;
                return num1;
            }
        }

        public long GetInt64()
        {
            var num1 = BitConverter.ToInt64(Data, Offset);
            checked
            {
                Offset += 8;
                return num1;
            }
        }

        public float GetFloat()
        {
            var single1 = BitConverter.ToSingle(Data, Offset);
            checked
            {
                Offset += 4;
                return single1;
            }
        }

        public string GetString()
        {
            var start = Offset;
            var i = 0;
            checked
            {
                while (Data[start + i] != 0)
                {
                    i++;
                    Offset++;
                }
                Offset++;
                return WorldServiceLocator._Functions.EscapeString(Encoding.UTF8.GetString(Data, start, i));
            }
        }

        public string GetString2()
        {
            int thisLength = Data[Offset];
            checked
            {
                var start = Offset + 1;
                Offset += thisLength + 1;
                return WorldServiceLocator._Functions.EscapeString(Encoding.UTF8.GetString(Data, start, thisLength));
            }
        }

        public ushort GetUInt16()
        {
            var num1 = BitConverter.ToUInt16(Data, Offset);
            checked
            {
                Offset += 2;
                return num1;
            }
        }

        public uint GetUInt32()
        {
            var num1 = BitConverter.ToUInt32(Data, Offset);
            checked
            {
                Offset += 4;
                return num1;
            }
        }

        public ulong GetUInt64()
        {
            var num1 = BitConverter.ToUInt64(Data, Offset);
            checked
            {
                Offset += 8;
                return num1;
            }
        }

        public ulong GetPackGuid()
        {
            var flags = Data[Offset];
            var guid = new byte[8];
            checked
            {
                Offset++;
                if ((flags & 1) == 1)
                {
                    guid[0] = Data[Offset];
                    Offset++;
                }
                if ((flags & 2) == 2)
                {
                    guid[1] = Data[Offset];
                    Offset++;
                }
                if ((flags & 4) == 4)
                {
                    guid[2] = Data[Offset];
                    Offset++;
                }
                if ((flags & 8) == 8)
                {
                    guid[3] = Data[Offset];
                    Offset++;
                }
                if ((flags & 0x10) == 16)
                {
                    guid[4] = Data[Offset];
                    Offset++;
                }
                if ((flags & 0x20) == 32)
                {
                    guid[5] = Data[Offset];
                    Offset++;
                }
                if ((flags & 0x40) == 64)
                {
                    guid[6] = Data[Offset];
                    Offset++;
                }
                if ((flags & 0x80) == 128)
                {
                    guid[7] = Data[Offset];
                    Offset++;
                }
                return BitConverter.ToUInt64(guid, 0);
            }
        }

        public byte[] GetByteArray()
        {
            var lengthLoc = checked(Data.Length - Offset);
            return lengthLoc <= 0 ? Array.Empty<byte>() : GetByteArray(lengthLoc);
        }

        private byte[] GetByteArray(int lengthLoc)
        {
            checked
            {
                if (Offset + lengthLoc > Data.Length)
                {
                    lengthLoc = Data.Length - Offset;
                }
                if (lengthLoc <= 0)
                {
                    return Array.Empty<byte>();
                }
                var tmpBytes = new byte[lengthLoc - 1 + 1];
                Array.Copy(Data, Offset, tmpBytes, 0, tmpBytes.Length);
                Offset += tmpBytes.Length;
                return tmpBytes;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
            }
            _disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        void IDisposable.Dispose()
        {
            //ILSpy generated this explicit interface implementation from .override directive in Dispose
            Dispose();
        }
    }
}
