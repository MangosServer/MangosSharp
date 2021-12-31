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
using System.Collections;
using System.Numerics;
using System.Security.Cryptography;

namespace Mangos.Realm;

public sealed class AuthEngine
{
    public static readonly byte[] CrcSalt;
    private static readonly Random _random;

    private byte[] _a;
    private readonly byte[] _b;
    public byte[] PublicB;
    public byte[] g;
    private readonly byte[] _k;

    // Private PublicK As Byte() = SS_Hash
    public byte[] M2;

    public readonly byte[] N;

    // Private Password As Byte()
    private byte[] _s;

    public readonly byte[] Salt;
    private byte[] _u;

    // Public CrcHash As Byte()
    private byte[] _username;

    public byte[] M1;
    public byte[] SsHash;
    private BigInteger _bna;
    private BigInteger _bNb;
    private BigInteger _bnPublicB;
    private BigInteger _bNg;
    private BigInteger _bNk;
    private BigInteger _bNn;
    private BigInteger _bns;
    private BigInteger _bnu;
    private BigInteger _bNv;
    private BigInteger _bNx;

    static AuthEngine()
    {
        CrcSalt = new byte[16];
        _random = new Random();
        _random.NextBytes(CrcSalt);
    }

    public AuthEngine()
    {
        var buffer1 = new byte[] { 7 };
        g = buffer1;
        N = new byte[] { 137, 75, 100, 94, 137, 225, 83, 91, 189, 173, 91, 139, 41, 6, 80, 83, 8, 1, 177, 142, 191, 191, 94, 143, 171, 60, 130, 135, 42, 62, 155, 183 };
        Salt = new byte[] { 173, 208, 58, 49, 210, 113, 20, 70, 117, 242, 112, 126, 80, 38, 182, 210, 241, 134, 89, 153, 118, 2, 80, 170, 185, 69, 224, 158, 221, 42, 163, 69 };
        var buffer2 = new byte[] { 3 };
        _k = buffer2;
        PublicB = new byte[32];
        _b = new byte[20];
    }

    private void CalculateB()
    {
        // Dim encoding1 As New UTF7Encoding
        _random.NextBytes(_b);
        BigInteger ptr1 = new();
        BigInteger ptr2 = new();
        BigInteger ptr3 = new();
        // Dim ptr4 As IntPtr = BN_new("")
        Array.Reverse(_b);
        _bNb = new BigInteger(_b, isUnsigned: true, isBigEndian: true);
        Array.Reverse(_b);
        ptr1 = BigInteger.ModPow(_bNg, _bNb, _bNn);
        ptr2 = _bNk * _bNv;
        ptr3 = ptr1 + ptr2;
        _bnPublicB = ptr3 % _bNn;
        PublicB = _bnPublicB.ToByteArray(isUnsigned: true, isBigEndian: true);
        Array.Reverse(PublicB);
    }

    private void CalculateK()
    {
        SHA1Managed algorithm1 = new();
        ArrayList list1 = new();
        list1 = Split(_s);
        list1[0] = algorithm1.ComputeHash((byte[])list1[0]);
        list1[1] = algorithm1.ComputeHash((byte[])list1[1]);
        SsHash = Combine((byte[])list1[0], (byte[])list1[1]);
    }

    public void CalculateM2(byte[] m1Loc)
    {
        SHA1Managed algorithm1 = new();
        var buffer1 = new byte[(_a.Length + m1Loc.Length + SsHash.Length)];
        Buffer.BlockCopy(_a, 0, buffer1, 0, _a.Length);
        Buffer.BlockCopy(m1Loc, 0, buffer1, _a.Length, m1Loc.Length);
        Buffer.BlockCopy(SsHash, 0, buffer1, _a.Length + m1Loc.Length, SsHash.Length);
        M2 = algorithm1.ComputeHash(buffer1);
    }

    private void CalculateS()
    {
        BigInteger ptr1 = new();
        BigInteger ptr2 = new();
        // Dim ptr3 As IntPtr = BN_new("")
        // Dim ptr4 As IntPtr = BN_new("")
        _bns = new BigInteger();
        _s = new byte[32];
        ptr1 = BigInteger.ModPow(_bNv, _bnu, _bNn);
        ptr2 = _bna * ptr1;
        _bns = BigInteger.ModPow(ptr2, _bNb, _bNn);
        _s = _bns.ToByteArray(isUnsigned: true, isBigEndian: true);
        Array.Reverse(_s);
        CalculateK();
    }

    public void CalculateU(byte[] a)
    {
        _a = a;
        SHA1Managed algorithm1 = new();
        var buffer1 = new byte[(a.Length + PublicB.Length)];
        Buffer.BlockCopy(a, 0, buffer1, 0, a.Length);
        Buffer.BlockCopy(PublicB, 0, buffer1, a.Length, PublicB.Length);
        _u = algorithm1.ComputeHash(buffer1);
        Array.Reverse(_u);
        _bnu = new BigInteger(_u, isUnsigned: true, isBigEndian: true);
        Array.Reverse(_u);
        Array.Reverse(a);
        _bna = new BigInteger(a, isUnsigned: true, isBigEndian: true);
        Array.Reverse(a);
        CalculateS();
    }

    private void CalculateV()
    {
        _bNv = BigInteger.ModPow(_bNg, _bNx, _bNn);
        CalculateB();
    }

    public void CalculateX(byte[] username, byte[] pwHash)
    {
        _username = username;
        SHA1Managed algorithm1 = new();
        // Dim encoding1 As New UTF7Encoding
        byte[] buffer3;
        buffer3 = new byte[20];
        byte[] buffer5;
        buffer5 = new byte[(Salt.Length + 20)];
        Buffer.BlockCopy(pwHash, 0, buffer5, Salt.Length, 20);
        Buffer.BlockCopy(Salt, 0, buffer5, 0, Salt.Length);
        buffer3 = algorithm1.ComputeHash(buffer5);
        Array.Reverse(buffer3);
        _bNx = new BigInteger(buffer3, isUnsigned: true, isBigEndian: true);
        Array.Reverse(g);
        _bNg = new BigInteger(g, isUnsigned: true, isBigEndian: true);
        Array.Reverse(g);
        Array.Reverse(_k);
        _bNk = new BigInteger(_k, isUnsigned: true, isBigEndian: true);
        Array.Reverse(_k);
        Array.Reverse(N);
        _bNn = new BigInteger(N, isUnsigned: true, isBigEndian: true);
        Array.Reverse(N);
        CalculateV();
    }

    public void CalculateM1()
    {
        SHA1Managed algorithm1 = new();
        byte[] nHash;
        nHash = new byte[20];
        byte[] gHash;
        gHash = new byte[20];
        byte[] ngHash;
        ngHash = new byte[20];
        byte[] userHash;
        userHash = new byte[20];
        nHash = algorithm1.ComputeHash(N);
        gHash = algorithm1.ComputeHash(g);
        userHash = algorithm1.ComputeHash(_username);
        for (var i = 0; i <= 19; i++)
        {
            ngHash[i] = (byte)(nHash[i] ^ gHash[i]);
        }

        var temp = Concat(ngHash, userHash);
        temp = Concat(temp, Salt);
        temp = Concat(temp, _a);
        temp = Concat(temp, PublicB);
        temp = Concat(temp, SsHash);
        M1 = algorithm1.ComputeHash(temp);
    }

    // Public Sub CalculateM1_Full()
    // Dim sha2 As New SHA1CryptoServiceProvider
    // Dim i As Byte = 0

    // 'Calc S1/S2
    // Dim s1 As Byte()
    // s1 = New Byte(16 - 1) {}
    // Dim s2 As Byte()
    // s2 = New Byte(16 - 1) {}
    // Do While (i < 16)
    // s1(i) = _s((i * 2))
    // s2(i) = _s(((i * 2) + 1))
    // i += 1
    // Loop

    // 'Calc SSHash
    // Dim s1Hash As Byte()
    // s1Hash = sha2.ComputeHash(s1)
    // Dim s2Hash As Byte()
    // s2Hash = sha2.ComputeHash(s2)
    // ReDim SsHash(32 - 1)
    // i = 0
    // Do While (i < 16)
    // SsHash((i * 2)) = s1Hash(i)
    // SsHash(((i * 2) + 1)) = s2Hash(i)
    // i += 1
    // Loop

    // 'Calc M1
    // Dim nHash As Byte()
    // nHash = sha2.ComputeHash(N)
    // Dim gHash As Byte()
    // gHash = sha2.ComputeHash(g)
    // Dim userHash As Byte()
    // userHash = sha2.ComputeHash(_Username)

    // Dim ngHash As Byte()
    // ngHash = New Byte(20 - 1) {}
    // i = 0
    // Do While (i < 20)
    // ngHash(i) = (nHash(i) Xor gHash(i))
    // i += 1
    // Loop

    // Dim temp As Byte() = Concat(ngHash, userHash)
    // temp = Concat(temp, Salt)
    // temp = Concat(temp, _a)
    // temp = Concat(temp, PublicB)
    // temp = Concat(temp, SsHash)
    // M1 = sha2.ComputeHash(temp)
    // End Sub

    private byte[] Combine(byte[] Bytes1, byte[] Bytes2)
    {
        if (Bytes1.Length != Bytes2.Length)
        {
            return null;
        }

        var CombineBuffer = new byte[(Bytes1.Length + Bytes2.Length)];
        var Counter = 0;
        for (int i = 0, loopTo = CombineBuffer.Length - 1; i <= loopTo; i += 2)
        {
            CombineBuffer[i] = Bytes1[Counter];
            Counter += 1;
        }

        Counter = 0;
        for (int i = 1, loopTo1 = CombineBuffer.Length - 1; i <= loopTo1; i += 2)
        {
            CombineBuffer[i] = Bytes2[Counter];
            Counter += 1;
        }

        return CombineBuffer;
    }

    public byte[] Concat(byte[] Buffer1, byte[] Buffer2)
    {
        var ConcatBuffer = new byte[(Buffer1.Length + Buffer2.Length)];
        Array.Copy(Buffer1, ConcatBuffer, Buffer1.Length);
        Array.Copy(Buffer2, 0, ConcatBuffer, Buffer1.Length, Buffer2.Length);
        return ConcatBuffer;
    }

    private ArrayList Split(byte[] ByteBuffer)
    {
        var SplitBuffer1 = new byte[(int)((ByteBuffer.Length / 2d) - 1d + 1)];
        var SplitBuffer2 = new byte[(int)((ByteBuffer.Length / 2d) - 1d + 1)];
        ArrayList ReturnList = new();
        var Counter = 0;
        for (int i = 0, loopTo = SplitBuffer1.Length - 1; i <= loopTo; i++)
        {
            SplitBuffer1[i] = ByteBuffer[Counter];
            Counter += 2;
        }

        Counter = 1;
        for (int i = 0, loopTo1 = SplitBuffer2.Length - 1; i <= loopTo1; i++)
        {
            SplitBuffer2[i] = ByteBuffer[Counter];
            Counter += 2;
        }

        ReturnList.Add(SplitBuffer1);
        ReturnList.Add(SplitBuffer2);
        return ReturnList;
    }
}
