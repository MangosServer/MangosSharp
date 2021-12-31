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

namespace Mangos.Cluster.Globals;

public class PacketClass : IDisposable
{
    public byte[] Data;
    public int Offset = 4;

    public int Length => Data[1] + (Data[0] * 256);

    public Opcodes OpCode
    {
        get
        {
            if (Information.UBound(Data) > 2)
            {
                return (Opcodes)(Data[2] + (Data[3] * 256));
            }

            // If it's a dodgy packet, change it to a null packet
            return 0;
        }
    }

    public PacketClass(Opcodes opcode)
    {
        Array.Resize(ref Data, 4);
        Data[0] = 0;
        Data[1] = 2;
        Data[2] = (byte)(Conversions.ToShort(opcode) % 256);
        Data[3] = (byte)(Conversions.ToShort(opcode) / 256);
    }

    public PacketClass(byte[] rawdata)
    {
        Data = rawdata;
    }

    // Public Sub AddBitArray(ByVal buffer As BitArray, ByVal Len As Integer)
    // ReDim Preserve Data(Data.Length - 1 + Len)
    // Data(0) = (Data.Length - 2) \ 256
    // Data(1) = (Data.Length - 2) Mod 256

    // Dim bufferarray(CType((buffer.Length + 8) / 8, Byte)) As Byte

    // buffer.CopyTo(bufferarray, 0)
    // Array.Copy(bufferarray, 0, Data, Data.Length - Len, Len)
    // End Sub

    public void AddInt8(byte buffer)
    {
        Array.Resize(ref Data, Data.Length + 1);
        Data[0] = (byte)((Data.Length - 2) / 256);
        Data[1] = (byte)((Data.Length - 2) % 256);
        Data[^1] = buffer;
    }

    public void AddInt16(short buffer)
    {
        Array.Resize(ref Data, Data.Length + 1 + 1);
        Data[0] = (byte)((Data.Length - 2) / 256);
        Data[1] = (byte)((Data.Length - 2) % 256);
        Data[^2] = (byte)(buffer & 255);
        Data[^1] = (byte)((buffer >> 8) & 255);
    }

    public void AddInt32(int buffer, int position = 0)
    {
        if (position <= 0 || position > Data.Length - 3)
        {
            position = Data.Length;
            Array.Resize(ref Data, Data.Length + 3 + 1);
            Data[0] = (byte)((Data.Length - 2) / 256);
            Data[1] = (byte)((Data.Length - 2) % 256);
        }

        Data[position] = (byte)(buffer & 255);
        Data[position + 1] = (byte)((buffer >> 8) & 255);
        Data[position + 2] = (byte)((buffer >> 16) & 255);
        Data[position + 3] = (byte)((buffer >> 24) & 255);
    }

    public void AddInt64(long buffer)
    {
        Array.Resize(ref Data, Data.Length + 7 + 1);
        Data[0] = (byte)((Data.Length - 2) / 256);
        Data[1] = (byte)((Data.Length - 2) % 256);
        Data[^8] = (byte)(buffer & 255L);
        Data[^7] = (byte)((buffer >> 8) & 255L);
        Data[^6] = (byte)((buffer >> 16) & 255L);
        Data[^5] = (byte)((buffer >> 24) & 255L);
        Data[^4] = (byte)((buffer >> 32) & 255L);
        Data[^3] = (byte)((buffer >> 40) & 255L);
        Data[^2] = (byte)((buffer >> 48) & 255L);
        Data[^1] = (byte)((buffer >> 56) & 255L);
    }

    public void AddString(string buffer)
    {
        if (Information.IsDBNull(buffer) | string.IsNullOrEmpty(buffer))
        {
            AddInt8(0);
        }
        else
        {
            var bytes = Encoding.UTF8.GetBytes(buffer.ToCharArray());
            Array.Resize(ref Data, Data.Length + bytes.Length + 1);
            Data[0] = (byte)((Data.Length - 2) / 256);
            Data[1] = (byte)((Data.Length - 2) % 256);
            for (int i = 0, loopTo = bytes.Length - 1; i <= loopTo; i++)
            {
                Data[Data.Length - 1 - bytes.Length + i] = bytes[i];
            }

            Data[^1] = 0;
        }
    }

    public void AddDouble(double buffer2)
    {
        var buffer1 = BitConverter.GetBytes(buffer2);
        Array.Resize(ref Data, Data.Length + buffer1.Length);
        Buffer.BlockCopy(buffer1, 0, Data, Data.Length - buffer1.Length, buffer1.Length);
        Data[0] = (byte)((Data.Length - 2) / 256);
        Data[1] = (byte)((Data.Length - 2) % 256);
    }

    public void AddSingle(float buffer2)
    {
        var buffer1 = BitConverter.GetBytes(buffer2);
        Array.Resize(ref Data, Data.Length + buffer1.Length);
        Buffer.BlockCopy(buffer1, 0, Data, Data.Length - buffer1.Length, buffer1.Length);
        Data[0] = (byte)((Data.Length - 2) / 256);
        Data[1] = (byte)((Data.Length - 2) % 256);
    }

    public void AddByteArray(byte[] buffer)
    {
        var tmp = Data.Length;
        Array.Resize(ref Data, Data.Length + buffer.Length);
        Array.Copy(buffer, 0, Data, tmp, buffer.Length);
        Data[0] = (byte)((Data.Length - 2) / 256);
        Data[1] = (byte)((Data.Length - 2) % 256);
    }

    public void AddPackGuid(ulong buffer)
    {
        var guid = BitConverter.GetBytes(buffer);
        BitArray flags = new(8);
        var offsetStart = Data.Length;
        var offsetNewSize = offsetStart;
        for (byte i = 0; i <= 7; i++)
        {
            flags[i] = guid[i] != 0;
            if (flags[i])
            {
                offsetNewSize += 1;
            }
        }

        Array.Resize(ref Data, offsetNewSize + 1);
        flags.CopyTo(Data, offsetStart);
        offsetStart += 1;
        for (byte i = 0; i <= 7; i++)
        {
            if (flags[i])
            {
                Data[offsetStart] = guid[i];
                offsetStart += 1;
            }
        }
    }

    // Public Sub AddUInt8(ByVal buffer As Byte)
    // ReDim Preserve Data(Data.Length + 1)
    //
    // Data(Data.Length - 1) = CType(((buffer >> 8) And 255), Byte)
    // End Sub

    public ushort GetUInt8()
    {
        var num1 = (ushort)(Data.Length + 1);
        Offset += 1;
        return num1;
    }

    public void AddUInt16(ushort buffer)
    {
        Array.Resize(ref Data, Data.Length + 1 + 1);
        Data[0] = (byte)((Data.Length - 2) / 256);
        Data[1] = (byte)((Data.Length - 2) % 256);
        Data[^2] = (byte)(buffer & 255);
        Data[^1] = (byte)((buffer >> 8) & 255);
    }

    public void AddUInt32(uint buffer)
    {
        Array.Resize(ref Data, Data.Length + 3 + 1);
        Data[0] = (byte)((Data.Length - 2) / 256);
        Data[1] = (byte)((Data.Length - 2) % 256);
        Data[^4] = (byte)(buffer & 255L);
        Data[^3] = (byte)((buffer >> 8) & 255L);
        Data[^2] = (byte)((buffer >> 16) & 255L);
        Data[^1] = (byte)((buffer >> 24) & 255L);
    }

    public void AddUInt64(ulong buffer)
    {
        Array.Resize(ref Data, Data.Length + 7 + 1);
        Data[0] = (byte)((Data.Length - 2) / 256);
        Data[1] = (byte)((Data.Length - 2) % 256);
        Data[^8] = (byte)((long)buffer & 255L);
        Data[^7] = (byte)((long)(buffer >> 8) & 255L);
        Data[^6] = (byte)((long)(buffer >> 16) & 255L);
        Data[^5] = (byte)((long)(buffer >> 24) & 255L);
        Data[^4] = (byte)((long)(buffer >> 32) & 255L);
        Data[^3] = (byte)((long)(buffer >> 40) & 255L);
        Data[^2] = (byte)((long)(buffer >> 48) & 255L);
        Data[^1] = (byte)((long)(buffer >> 56) & 255L);
    }

    public byte GetInt8()
    {
        Offset += 1;
        return Data[Offset - 1];
    }

    // Public Function GetInt8(ByVal Offset As Integer) As Byte
    // Offset = Offset + 1
    // Return Data(Offset - 1)
    // End Function

    public short GetInt16()
    {
        var num1 = BitConverter.ToInt16(Data, Offset);
        Offset += 2;
        return num1;
    }

    // Public Function GetInt16(ByVal Offset As Integer) As Short
    // Dim num1 As Short = BitConverter.ToInt16(Data, Offset)
    // Offset = (Offset + 2)
    // Return num1
    // End Function

    public int GetInt32()
    {
        var num1 = BitConverter.ToInt32(Data, Offset);
        Offset += 4;
        return num1;
    }

    // Public Function GetInt32(ByVal Offset As Integer) As Integer
    // Dim num1 As Integer = BitConverter.ToInt32(Data, Offset)
    // Offset = (Offset + 4)
    // Return num1
    // End Function

    public long GetInt64()
    {
        var num1 = BitConverter.ToInt64(Data, Offset);
        Offset += 8;
        return num1;
    }

    // Public Function GetInt64(ByVal Offset As Integer) As Long
    // Dim num1 As Long = BitConverter.ToInt64(Data, Offset)
    // Offset = (Offset + 8)
    // Return num1
    // End Function

    public float GetFloat()
    {
        var single1 = BitConverter.ToSingle(Data, Offset);
        Offset += 4;
        return single1;
    }

    // Public Function GetFloat(ByVal Offset_ As Integer) As Single
    // Dim single1 As Single = BitConverter.ToSingle(Data, Offset)
    // Offset = (Offset_ + 4)
    // Return single1
    // End Function

    public double GetDouble()
    {
        var num1 = BitConverter.ToDouble(Data, Offset);
        Offset += 8;
        return num1;
    }

    // Public Function GetDouble(ByVal Offset As Integer) As Double
    // Dim num1 As Double = BitConverter.ToDouble(Data, Offset)
    // Offset = (Offset + 8)
    // Return num1
    // End Function

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

    // Public Function GetString(ByVal Offset As Integer) As String
    // Dim i As Integer = Offset
    // Dim tmpString As String = ""
    // While Data(i) <> 0
    // tmpString = tmpString + Chr(Data(i))
    // i = i + 1
    // Offset = Offset + 1
    // End While
    // Offset = Offset + 1
    // Return tmpString
    // End Function

    public ushort GetUInt16()
    {
        var num1 = BitConverter.ToUInt16(Data, Offset);
        Offset += 2;
        return num1;
    }

    // Public Function GetUInt16(ByVal Offset As Integer) As UShort
    // Dim num1 As UShort = BitConverter.ToUInt16(Data, Offset)
    // Offset = (Offset + 2)
    // Return num1
    // End Function

    public uint GetUInt32()
    {
        var num1 = BitConverter.ToUInt32(Data, Offset);
        Offset += 4;
        return num1;
    }

    // Public Function GetUInt32(ByVal Offset As Integer) As UInteger
    // Dim num1 As UInteger = BitConverter.ToUInt32(Data, Offset)
    // Offset = (Offset + 4)
    // Return num1
    // End Function

    public ulong GetUInt64()
    {
        var num1 = BitConverter.ToUInt64(Data, Offset);
        Offset += 8;
        return num1;
    }

    // Public Function GetUInt64(ByVal Offset As Integer) As ULong
    // Dim num1 As ULong = BitConverter.ToUInt64(Data, Offset)
    // Offset = (Offset + 8)
    // Return num1
    // End Function

    // Public Function GetPackGUID() As ULong
    // Dim flags As Byte = Data(Offset)
    // Dim GUID() As Byte = {0, 0, 0, 0, 0, 0, 0, 0}
    // Offset += 1

    // If (flags And 1) = 1 Then
    // GUID(0) = Data(Offset)
    // Offset += 1
    // End If
    // If (flags And 2) = 2 Then
    // GUID(1) = Data(Offset)
    // Offset += 1
    // End If
    // If (flags And 4) = 4 Then
    // GUID(2) = Data(Offset)
    // Offset += 1
    // End If
    // If (flags And 8) = 8 Then
    // GUID(3) = Data(Offset)
    // Offset += 1
    // End If
    // If (flags And 16) = 16 Then
    // GUID(4) = Data(Offset)
    // Offset += 1
    // End If
    // If (flags And 32) = 32 Then
    // GUID(5) = Data(Offset)
    // Offset += 1
    // End If
    // If (flags And 64) = 64 Then
    // GUID(6) = Data(Offset)
    // Offset += 1
    // End If
    // If (flags And 128) = 128 Then
    // GUID(7) = Data(Offset)
    // Offset += 1
    // End If

    // Return CType(BitConverter.ToUInt64(GUID, 0), ULong)
    // End Function

    // Public Function GetPackGUID(ByVal Offset As Integer) As ULong
    // Dim flags As Byte = Data(Offset)
    // Dim GUID() As Byte = {0, 0, 0, 0, 0, 0, 0, 0}
    // Offset += 1

    // If (flags And 1) = 1 Then
    // GUID(0) = Data(Offset)
    // Offset += 1
    // End If
    // If (flags And 2) = 2 Then
    // GUID(1) = Data(Offset)
    // Offset += 1
    // End If
    // If (flags And 4) = 4 Then
    // GUID(2) = Data(Offset)
    // Offset += 1
    // End If
    // If (flags And 8) = 8 Then
    // GUID(3) = Data(Offset)
    // Offset += 1
    // End If
    // If (flags And 16) = 16 Then
    // GUID(4) = Data(Offset)
    // Offset += 1
    // End If
    // If (flags And 32) = 32 Then
    // GUID(5) = Data(Offset)
    // Offset += 1
    // End If
    // If (flags And 64) = 64 Then
    // GUID(6) = Data(Offset)
    // Offset += 1
    // End If
    // If (flags And 128) = 128 Then
    // GUID(7) = Data(Offset)
    // Offset += 1
    // End If

    // Return CType(BitConverter.ToUInt64(GUID, 0), ULong)
    // End Function

    private bool _disposedValue; // To detect redundant calls

    // IDisposable
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            // TODO: set large fields to null.
        }

        _disposedValue = true;
    }

    // This code added by Visual Basic to correctly implement the disposable pattern.
    public void Dispose()
    {
        // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
