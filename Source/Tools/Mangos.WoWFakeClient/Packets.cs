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

using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections;
using System.Text;

namespace Mangos.WoWFakeClient;

public static class Packets
{
    public static void DumpPacket(byte[] data)
    {
        // #If DEBUG Then
        int j;
        var buffer = "";
        try
        {
            buffer += string.Format("DEBUG: Packet Dump - Length={0}{1}", data.Length, Constants.vbCrLf);
            if (data.Length % 16 == 0)
            {
                var loopTo = data.Length - 1;
                for (j = 0; j <= loopTo; j += 16)
                {
                    buffer += "|  " + BitConverter.ToString(data, j, 16).Replace("-", " ");
                    buffer += " |  " + Encoding.ASCII.GetString(data, j, 16).Replace(Constants.vbTab, "?").Replace(Constants.vbBack, "?").Replace(Constants.vbCr, "?").Replace(Constants.vbFormFeed, "?").Replace(Constants.vbLf, "?") + " |" + Constants.vbCrLf;
                }
            }
            else
            {
                var loopTo1 = data.Length - 1 - 16;
                for (j = 0; j <= loopTo1; j += 16)
                {
                    buffer += "|  " + BitConverter.ToString(data, j, 16).Replace("-", " ");
                    buffer += " |  " + Encoding.ASCII.GetString(data, j, 16).Replace(Constants.vbTab, "?").Replace(Constants.vbBack, "?").Replace(Constants.vbCr, "?").Replace(Constants.vbFormFeed, "?").Replace(Constants.vbLf, "?") + " |" + Constants.vbCrLf;
                }

                buffer += "|  " + BitConverter.ToString(data, j, data.Length % 16).Replace("-", " ");
                buffer += new string(' ', (16 - (data.Length % 16)) * 3);
                buffer += " |  " + Encoding.ASCII.GetString(data, j, data.Length % 16).Replace(Constants.vbTab, "?").Replace(Constants.vbBack, "?").Replace(Constants.vbCr, "?").Replace(Constants.vbFormFeed, "?").Replace(Constants.vbLf, "?");
                buffer += new string(' ', 16 - (data.Length % 16));
                buffer += " |" + Constants.vbCrLf;
            }

            Console.WriteLine(buffer);
        }
        // #End If
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error dumping packet: {0}{1}", Constants.vbCrLf, e);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    public class PacketClass : IDisposable
    {
        public byte[] Data;
        public int Offset = 4;
        private readonly bool Realm;

        public int Length => Realm ? Data[1] + (Data[2] * 256) : Data[1] + (Data[0] * 256);

        public int OpCode => Realm ? Data[0] : Data[2] + (Data[3] * 256);

        public PacketClass(OPCODES opcode)
        {
            Array.Resize(ref Data, 6);
            Data[0] = 0;
            Data[1] = 4;
            Data[2] = (byte)(Conversions.ToShort(opcode) % 256);
            Data[3] = (byte)(Conversions.ToShort(opcode) / 256);
            Data[4] = 0;
            Data[5] = 0;
        }

        public PacketClass(byte opcode)
        {
            Array.Resize(ref Data, 1);
            Data[0] = opcode;
            Realm = true;
        }

        public PacketClass(ref byte[] rawdata, bool Realm_ = false)
        {
            Data = rawdata;
            Realm = Realm_;
        }

        public void AddBitArray(BitArray buffer, int Len)
        {
            Array.Resize(ref Data, Data.Length - 1 + Len + 1);
            if (Realm == false)
            {
                Data[0] = (byte)((Data.Length - 2) / 256);
                Data[1] = (byte)((Data.Length - 2) % 256);
            }

            var bufferarray = new byte[(Conversions.ToByte((buffer.Length + 8) / 8d) + 1)];
            buffer.CopyTo(bufferarray, 0);
            Array.Copy(bufferarray, 0, Data, Data.Length - Len, Len);
        }

        public void AddInt8(byte buffer)
        {
            Array.Resize(ref Data, Data.Length + 1);
            if (Realm == false)
            {
                Data[0] = (byte)((Data.Length - 2) / 256);
                Data[1] = (byte)((Data.Length - 2) % 256);
            }

            Data[^1] = buffer;
        }

        public void AddInt16(short buffer)
        {
            Array.Resize(ref Data, Data.Length + 1 + 1);
            if (Realm == false)
            {
                Data[0] = (byte)((Data.Length - 2) / 256);
                Data[1] = (byte)((Data.Length - 2) % 256);
            }

            Data[^2] = Conversions.ToByte(buffer & 255);
            Data[^1] = Conversions.ToByte((buffer >> 8) & 255);
        }

        public void AddInt32(int buffer, int position = 0)
        {
            if (position <= 0 || position > Data.Length - 3)
            {
                position = Data.Length;
                Array.Resize(ref Data, Data.Length + 3 + 1);
                if (Realm == false)
                {
                    Data[0] = (byte)((Data.Length - 2) / 256);
                    Data[1] = (byte)((Data.Length - 2) % 256);
                }
            }

            Data[position] = Conversions.ToByte(buffer & 255);
            Data[position + 1] = Conversions.ToByte((buffer >> 8) & 255);
            Data[position + 2] = Conversions.ToByte((buffer >> 16) & 255);
            Data[position + 3] = Conversions.ToByte((buffer >> 24) & 255);
        }

        public void AddInt64(long buffer)
        {
            Array.Resize(ref Data, Data.Length + 7 + 1);
            if (Realm == false)
            {
                Data[0] = (byte)((Data.Length - 2) / 256);
                Data[1] = (byte)((Data.Length - 2) % 256);
            }

            Data[^8] = Conversions.ToByte(buffer & 255L);
            Data[^7] = Conversions.ToByte((buffer >> 8) & 255L);
            Data[^6] = Conversions.ToByte((buffer >> 16) & 255L);
            Data[^5] = Conversions.ToByte((buffer >> 24) & 255L);
            Data[^4] = Conversions.ToByte((buffer >> 32) & 255L);
            Data[^3] = Conversions.ToByte((buffer >> 40) & 255L);
            Data[^2] = Conversions.ToByte((buffer >> 48) & 255L);
            Data[^1] = Conversions.ToByte((buffer >> 56) & 255L);
        }

        public void AddString(string buffer, bool EndZero = true, bool Reversed = false)
        {
            if (Information.IsDBNull(buffer) | string.IsNullOrEmpty(buffer))
            {
                AddInt8(0);
            }
            else
            {
                var Bytes = Encoding.UTF8.GetBytes(buffer.ToCharArray());
                var Position = Data.Length;
                if (EndZero)
                {
                    Array.Resize(ref Data, Data.Length + Bytes.Length + 1);
                }
                else
                {
                    Array.Resize(ref Data, Data.Length + Bytes.Length);
                }

                if (Realm == false)
                {
                    Data[0] = (byte)((Data.Length - 2) / 256);
                    Data[1] = (byte)((Data.Length - 2) % 256);
                }

                int i;
                if (Reversed)
                {
                    for (i = Bytes.Length - 1; i >= 0; i -= 1)
                    {
                        Data[Position] = Bytes[i];
                        Position += 1;
                    }
                }
                else
                {
                    var loopTo = Bytes.Length - 1;
                    for (i = 0; i <= loopTo; i++)
                    {
                        Data[Position] = Bytes[i];
                        Position += 1;
                    }
                }

                if (EndZero)
                {
                    Data[Position] = 0;
                }
            }
        }

        public void AddDouble(double buffer2)
        {
            var buffer1 = BitConverter.GetBytes(buffer2);
            Array.Resize(ref Data, Data.Length + buffer1.Length);
            Buffer.BlockCopy(buffer1, 0, Data, Data.Length - buffer1.Length, buffer1.Length);
            if (Realm == false)
            {
                Data[0] = (byte)((Data.Length - 2) / 256);
                Data[1] = (byte)((Data.Length - 2) % 256);
            }
        }

        public void AddSingle(float buffer2)
        {
            var buffer1 = BitConverter.GetBytes(buffer2);
            Array.Resize(ref Data, Data.Length + buffer1.Length);
            Buffer.BlockCopy(buffer1, 0, Data, Data.Length - buffer1.Length, buffer1.Length);
            if (Realm == false)
            {
                Data[0] = (byte)((Data.Length - 2) / 256);
                Data[1] = (byte)((Data.Length - 2) % 256);
            }
        }

        public void AddByteArray(byte[] buffer)
        {
            var tmp = Data.Length;
            Array.Resize(ref Data, Data.Length + buffer.Length);
            Array.Copy(buffer, 0, Data, tmp, buffer.Length);
            if (Realm == false)
            {
                Data[0] = (byte)((Data.Length - 2) / 256);
                Data[1] = (byte)((Data.Length - 2) % 256);
            }
        }

        public void AddPackGUID(ulong buffer)
        {
            var GUID = BitConverter.GetBytes(buffer);
            BitArray flags = new(8);
            var offsetStart = Data.Length;
            var offsetNewSize = offsetStart;
            byte i;
            for (i = 0; i <= 7; i++)
            {
                flags[i] = GUID[i] != 0;
                if (flags[i])
                {
                    offsetNewSize += 1;
                }
            }

            Array.Resize(ref Data, offsetNewSize + 1);
            if (Realm == false)
            {
                Data[0] = (byte)((Data.Length - 2) / 256);
                Data[1] = (byte)((Data.Length - 2) % 256);
            }

            flags.CopyTo(Data, offsetStart);
            offsetStart += 1;
            for (i = 0; i <= 7; i++)
            {
                if (flags[i])
                {
                    Data[offsetStart] = GUID[i];
                    offsetStart += 1;
                }
            }
        }

        public void AddUInt16(ushort buffer, int Position = 0)
        {
            if (Position == 0)
            {
                Position = Data.Length;
                Array.Resize(ref Data, Data.Length + 1 + 1);
            }

            if (Realm == false)
            {
                Data[0] = (byte)((Data.Length - 2) / 256);
                Data[1] = (byte)((Data.Length - 2) % 256);
            }

            Data[Position] = Conversions.ToByte(buffer & 255);
            Data[Position + 1] = Conversions.ToByte((buffer >> 8) & 255);
        }

        public void AddUInt32(uint buffer)
        {
            Array.Resize(ref Data, Data.Length + 3 + 1);
            if (Realm == false)
            {
                Data[0] = (byte)((Data.Length - 2) / 256);
                Data[1] = (byte)((Data.Length - 2) % 256);
            }

            Data[^4] = Conversions.ToByte(buffer & 255L);
            Data[^3] = Conversions.ToByte((buffer >> 8) & 255L);
            Data[^2] = Conversions.ToByte((buffer >> 16) & 255L);
            Data[^1] = Conversions.ToByte((buffer >> 24) & 255L);
        }

        public void AddUInt64(ulong buffer)
        {
            Array.Resize(ref Data, Data.Length + 7 + 1);
            if (Realm == false)
            {
                Data[0] = (byte)((Data.Length - 2) / 256);
                Data[1] = (byte)((Data.Length - 2) % 256);
            }

            Data[^8] = Conversions.ToByte((long)buffer & 255L);
            Data[^7] = Conversions.ToByte((long)(buffer >> 8) & 255L);
            Data[^6] = Conversions.ToByte((long)(buffer >> 16) & 255L);
            Data[^5] = Conversions.ToByte((long)(buffer >> 24) & 255L);
            Data[^4] = Conversions.ToByte((long)(buffer >> 32) & 255L);
            Data[^3] = Conversions.ToByte((long)(buffer >> 40) & 255L);
            Data[^2] = Conversions.ToByte((long)(buffer >> 48) & 255L);
            Data[^1] = Conversions.ToByte((long)(buffer >> 56) & 255L);
        }

        public byte GetInt8()
        {
            Offset += 1;
            return Data[Offset - 1];
        }

        public byte GetInt8(int Offset)
        {
            Offset += 1;
            return Data[Offset - 1];
        }

        public short GetInt16()
        {
            var num1 = BitConverter.ToInt16(Data, Offset);
            Offset += 2;
            return num1;
        }

        public short GetInt16(int Offset)
        {
            var num1 = BitConverter.ToInt16(Data, Offset);
            return num1;
        }

        public int GetInt32()
        {
            var num1 = BitConverter.ToInt32(Data, Offset);
            Offset += 4;
            return num1;
        }

        public int GetInt32(int Offset)
        {
            var num1 = BitConverter.ToInt32(Data, Offset);
            return num1;
        }

        public long GetInt64()
        {
            var num1 = BitConverter.ToInt64(Data, Offset);
            Offset += 8;
            return num1;
        }

        public long GetInt64(int Offset)
        {
            var num1 = BitConverter.ToInt64(Data, Offset);
            return num1;
        }

        public float GetFloat()
        {
            var single1 = BitConverter.ToSingle(Data, Offset);
            Offset += 4;
            return single1;
        }

        public float GetFloat(int Offset_)
        {
            var single1 = BitConverter.ToSingle(Data, Offset);
            Offset = Offset_ + 4;
            return single1;
        }

        public double GetDouble()
        {
            var num1 = BitConverter.ToDouble(Data, Offset);
            Offset += 8;
            return num1;
        }

        public double GetDouble(int Offset)
        {
            var num1 = BitConverter.ToDouble(Data, Offset);
            return num1;
        }

        public string GetString()
        {
            var start = Offset;
            var i = 0;
            while (Data[start + i] != 0)
            {
                i += 1;
                Offset += 1;
            }

            Offset += 1;
            return Encoding.UTF8.GetString(Data, start, i);
        }

        public string GetString(int Offset)
        {
            var i = Offset;
            var tmpString = "";
            while (Data[i] != 0)
            {
                tmpString += Conversions.ToString((char)Data[i]);
                i += 1;
                Offset += 1;
            }

            return tmpString;
        }

        public ushort GetUInt16()
        {
            var num1 = BitConverter.ToUInt16(Data, Offset);
            Offset += 2;
            return num1;
        }

        public ushort GetUInt16(int Offset)
        {
            var num1 = BitConverter.ToUInt16(Data, Offset);
            return num1;
        }

        public uint GetUInt32()
        {
            var num1 = BitConverter.ToUInt32(Data, Offset);
            Offset += 4;
            return num1;
        }

        public uint GetUInt32(int Offset)
        {
            var num1 = BitConverter.ToUInt32(Data, Offset);
            return num1;
        }

        public ulong GetUInt64()
        {
            var num1 = BitConverter.ToUInt64(Data, Offset);
            Offset += 8;
            return num1;
        }

        public ulong GetUInt64(int Offset)
        {
            var num1 = BitConverter.ToUInt64(Data, Offset);
            return num1;
        }

        public ulong GetPackGUID()
        {
            var flags = Data[Offset];
            var GUID = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            Offset += 1;
            if ((flags & 1) == 1)
            {
                GUID[0] = Data[Offset];
                Offset += 1;
            }

            if ((flags & 2) == 2)
            {
                GUID[1] = Data[Offset];
                Offset += 1;
            }

            if ((flags & 4) == 4)
            {
                GUID[2] = Data[Offset];
                Offset += 1;
            }

            if ((flags & 8) == 8)
            {
                GUID[3] = Data[Offset];
                Offset += 1;
            }

            if ((flags & 16) == 16)
            {
                GUID[4] = Data[Offset];
                Offset += 1;
            }

            if ((flags & 32) == 32)
            {
                GUID[5] = Data[Offset];
                Offset += 1;
            }

            if ((flags & 64) == 64)
            {
                GUID[6] = Data[Offset];
                Offset += 1;
            }

            if ((flags & 128) == 128)
            {
                GUID[7] = Data[Offset];
                Offset += 1;
            }

            return BitConverter.ToUInt64(GUID, 0);
        }

        public ulong GetPackGUID(int Offset)
        {
            var flags = Data[Offset];
            var GUID = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            Offset += 1;
            if ((flags & 1) == 1)
            {
                GUID[0] = Data[Offset];
                Offset += 1;
            }

            if ((flags & 2) == 2)
            {
                GUID[1] = Data[Offset];
                Offset += 1;
            }

            if ((flags & 4) == 4)
            {
                GUID[2] = Data[Offset];
                Offset += 1;
            }

            if ((flags & 8) == 8)
            {
                GUID[3] = Data[Offset];
                Offset += 1;
            }

            if ((flags & 16) == 16)
            {
                GUID[4] = Data[Offset];
                Offset += 1;
            }

            if ((flags & 32) == 32)
            {
                GUID[5] = Data[Offset];
                Offset += 1;
            }

            if ((flags & 64) == 64)
            {
                GUID[6] = Data[Offset];
                Offset += 1;
            }

            if ((flags & 128) == 128)
            {
                GUID[7] = Data[Offset];
            }

            return BitConverter.ToUInt64(GUID, 0);
        }

        public byte[] GetByteArray(int Length)
        {
            if (Length < 0)
            {
                return Array.Empty<byte>();
            }

            var tmpArray = new byte[Length];
            Array.Copy(Data, Offset, tmpArray, 0, Length);
            Offset += Length;
            return tmpArray;
        }

        public void Dispose()
        {
        }
    }
}
