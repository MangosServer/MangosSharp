//
//  Copyright (C) 2013-2020 getMaNGOS <https:\\getmangos.eu>
//  
//  This program is free software. You can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation. either version 2 of the License, or
//  (at your option) any later version.
//  
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY. Without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//  
//  You should have received a copy of the GNU General Public License
//  along with this program. If not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//

using System;
using Microsoft.VisualBasic;

namespace Mangos.Common.Globals
{
    public class Converter
    {
        public byte ToByte(byte[] d, ref int offset)
        {
            return d[offset + 1];
        }
        // Byte
        public void ToBytes(byte a, byte[] b, ref int t)
        {
            b[t] = a;
            t += 1;
        }
        // Float
        public void ToBytes(double a, byte[] b, ref int t)
        {
            var buffer1 = BitConverter.GetBytes(a);
            Buffer.BlockCopy(buffer1, 0, b, t, buffer1.Length);
            t += buffer1.Length;
        }

        public void ToBytes(float a, byte[] b, ref int t)
        {
            var buffer1 = BitConverter.GetBytes(a);
            Buffer.BlockCopy(buffer1, 0, b, t, buffer1.Length);
            t += buffer1.Length;
        }
        // Int16
        public void ToBytes(short a, byte[] b, ref int t)
        {
            b[t] = (byte)(a & 255);
            t += 1;
            b[t] = (byte)(a >> 8 & 255);
            t += 1;
        }
        // Int32
        public void ToBytes(int a, byte[] b, ref int t)
        {
            b[t] = (byte)(a & 255);
            t += 1;
            b[t] = (byte)(a >> 8 & 255);
            t += 1;
            b[t] = (byte)(a >> 16 & 255);
            t += 1;
            b[t] = (byte)(a >> 24 & 255);
            t += 1;
        }
        // Int64
        public void ToBytes(long a, byte[] b, ref int t)
        {
            b[t] = (byte)(a & 255L);
            t += 1;
            b[t] = (byte)(a >> 8 & 255L);
            t += 1;
            b[t] = (byte)(a >> 16 & 255L);
            t += 1;
            b[t] = (byte)(a >> 24 & 255L);
            t += 1;
            b[t] = (byte)(a >> 32 & 255L);
            t += 1;
            b[t] = (byte)(a >> 40 & 255L);
            t += 1;
            b[t] = (byte)(a >> 48 & 255L);
            t += 1;
            b[t] = (byte)(a >> 56 & 255L);
            t += 1;
        }
        // String
        public void ToBytes(string a, byte[] b, ref int t)
        {
            var chArray1 = a.ToCharArray();
            var chArray2 = chArray1;
            int num1;
            var loopTo = chArray2.Length - 1;
            for (num1 = 0; num1 <= loopTo; num1++)
            {
                b[t] = (byte)Strings.Asc(chArray2[num1]);
                t += 1;
            }
        }

        public double ToDouble(byte[] d, ref int offset)
        {
            double num1 = BitConverter.ToDouble(d, offset);
            offset += 8;
            return num1;
        }

        public float ToFloat(byte[] d, ref int offset)
        {
            float single1 = BitConverter.ToSingle(d, offset);
            offset += 4;
            return single1;
        }

        public short ToInt16(byte[] d, ref int offset)
        {
            short num1 = BitConverter.ToInt16(d, offset);
            offset += 2;
            return num1;
        }

        public ushort ToUInt16(byte[] d, ref int offset)
        {
            ushort num1 = BitConverter.ToUInt16(d, offset);
            offset += 2;
            return num1;
        }

        public int ToInt32(byte[] d, ref int offset)
        {
            int num1 = BitConverter.ToInt32(d, offset);
            offset += 4;
            return num1;
        }

        public uint ToUInt32(byte[] d, ref int offset)
        {
            uint num1 = BitConverter.ToUInt32(d, offset);
            offset += 4;
            return num1;
        }

        public long ToInt64(byte[] d, ref int offset)
        {
            long num1 = BitConverter.ToInt64(d, offset);
            offset += 8;
            return num1;
        }

        public ulong ToUInt64(byte[] d, ref int offset)
        {
            ulong num1 = BitConverter.ToUInt64(d, offset);
            offset += 8;
            return num1;
        }
    }
}