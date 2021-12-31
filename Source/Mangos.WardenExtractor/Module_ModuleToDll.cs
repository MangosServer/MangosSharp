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

using Mangos.Zip;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;

namespace Mangos.WardenExtractor;

public static class Module_ModuleToDll
{
    public static void ModulesToDlls()
    {
        if (Directory.Exists("Modules") == false)
        {
            Directory.CreateDirectory("Modules");
        }

        var sFiles = Directory.GetFiles("Modules");
        if (sFiles.Length == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("No modules found.");
            return;
        }

        foreach (var sFile in Directory.GetFiles("Modules"))
        {
            Console.WriteLine(sFile);
            byte[] fileData = null;
            try
            {
                FileStream fs = new(sFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (fs.Length == 0L)
                {
                    fileData = Array.Empty<byte>();
                }
                else
                {
                    fileData = new byte[(int)(fs.Length - 1L + 1)];
                    fs.Read(fileData, 0, fileData.Length);
                }

                fs.Close();
                fs.Dispose();
                fs = null;
                if (fileData.Length == 0)
                {
                    continue;
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write("FileStream has thrown an Exception! The exception is {0}", e);
            }

            if (fileData is null)
            {
                continue;
            }

            ModuleToDll(Path.GetFileName(sFile), ref fileData);
        }
    }

    public static void DllsToModules()
    {
        if (Directory.Exists("Dlls") == false)
        {
            Directory.CreateDirectory("Dlls");
        }

        var sFiles = Directory.GetFiles("Dlls");
        if (sFiles.Length == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("No dlls found.");
            return;
        }

        foreach (var sFile in Directory.GetFiles("Dlls"))
        {
            Console.WriteLine(sFile);
            byte[] fileData = null;
            try
            {
                FileStream fs = new(sFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (fs.Length == 0L)
                {
                    fileData = Array.Empty<byte>();
                }
                else
                {
                    fileData = new byte[(int)(fs.Length - 1L + 1)];
                    fs.Read(fileData, 0, fileData.Length);
                }

                fs.Close();
                fs.Dispose();
                fs = null;
                if (fileData.Length == 0)
                {
                    continue;
                }
            }
            catch
            {
            }

            if (fileData is null)
            {
                continue;
            }

            DllToModule(Path.GetFileName(sFile), ref fileData);
        }
    }

    public static void ModuleToDll(string ModName, ref byte[] ModData)
    {
        // Console.WriteLine("Insert RC4 Key for {0}:", ModName)
        // Dim RC4Key As String = Console.ReadLine()

        var Key = Program.ParseKey("ECA9A9D1EAAEFD38CC115062FB92996E"); // ParseKey(RC4Key)
        Key = Program.RC4.Init(Key);
        Program.RC4.Crypt(ref ModData, Key);
        var UncompressedLen = BitConverter.ToInt32(ModData, 0);
        if (UncompressedLen < 0)
        {
            Console.WriteLine("Failed to decrypt {0}, incorrect length.", ModName);
            return;
        }

        var CompressedData = new byte[(ModData.Length - 0x108)];
        Array.Copy(ModData, 4, CompressedData, 0, CompressedData.Length);
        var dataPos = 4 + CompressedData.Length;
        var Sign = Conversions.ToString((char)ModData[dataPos + 3]) + (char)ModData[dataPos + 2] + (char)ModData[dataPos + 1] + (char)ModData[dataPos];
        if (Sign != "SIGN")
        {
            Console.WriteLine("Failed to decrypt {0}, sign missing.", ModName);
            return;
        }

        dataPos += 4;
        var Signature = new byte[256];
        Array.Copy(ModData, dataPos, Signature, 0, Signature.Length);

        // Check signature
        if (CheckSignature(Signature, ModData, ModData.Length - 0x104) == false)
        {
            Console.WriteLine("Signature fail.");
            return;
        }

        var DecompressedData = new ZipService().DeCompress(CompressedData);
        FileStream fs3 = new(@"dlls\" + ModName.Replace(Path.GetExtension(ModName), "") + ".before.dll", FileMode.Create, FileAccess.Write, FileShare.None);
        fs3.Write(DecompressedData, 0, DecompressedData.Length);
        fs3.Close();
        fs3.Dispose();
        Module_FixDll.FixNormalDll(ref DecompressedData);
        FileStream fs2 = new(@"dlls\" + ModName.Replace(Path.GetExtension(ModName), "") + ".after.dll", FileMode.Create, FileAccess.Write, FileShare.None);
        fs2.Write(DecompressedData, 0, DecompressedData.Length);
        fs2.Close();
        fs2.Dispose();
    }

    public static void DllToModule(string DllName, ref byte[] DllData)
    {
        Console.WriteLine("Insert RC4 Key for {0}:", DllName);
        var RC4Key = Console.ReadLine();
        var Key = Program.ParseKey(RC4Key);
        Key = Program.RC4.Init(Key);
        var CompressedData = new ZipService().Compress(DllData, 0, DllData.Length);
        MemoryStream mw = new();
        BinaryWriter bw = new(mw);
        bw.Write(DllData.Length); // Uncompressed buffer
        bw.Write(CompressedData, 0, CompressedData.Length); // Data
        bw.Write((byte)Strings.Asc('N')); // \
        bw.Write((byte)Strings.Asc('G')); // \ Sign
        bw.Write((byte)Strings.Asc('I')); // /
        bw.Write((byte)Strings.Asc('S')); // /
        var tmpData = mw.ToArray();
        var Signature = CreateSignature(tmpData, tmpData.Length - 4);
        bw.Write(Signature, 0, Signature.Length);
        tmpData = mw.ToArray();
        mw.Close();
        mw.Dispose();
        Program.RC4.Crypt(ref tmpData, Key);
        FileStream fs2 = new(@"Modules\" + DllName.Replace(Path.GetExtension(DllName), "") + ".mod", FileMode.Create, FileAccess.Write, FileShare.None);
        fs2.Write(tmpData, 0, tmpData.Length);
        fs2.Close();
        fs2.Dispose();
    }

    public static bool CheckSignature(byte[] Signature, byte[] Data, int DataLen)
    {
        BigInteger power = new(new byte[] { 0x1, 0x0, 0x1, 0x0 }, true);
        BigInteger pmod = new(new byte[] { 0x6B, 0xCE, 0xF5, 0x2D, 0x2A, 0x7D, 0x7A, 0x67, 0x21, 0x21, 0x84, 0xC9, 0xBC, 0x25, 0xC7, 0xBC, 0xDF, 0x3D, 0x8F, 0xD9, 0x47, 0xBC, 0x45, 0x48, 0x8B, 0x22, 0x85, 0x3B, 0xC5, 0xC1, 0xF4, 0xF5, 0x3C, 0xC, 0x49, 0xBB, 0x56, 0xE0, 0x3D, 0xBC, 0xA2, 0xD2, 0x35, 0xC1, 0xF0, 0x74, 0x2E, 0x15, 0x5A, 0x6, 0x8A, 0x68, 0x1, 0x9E, 0x60, 0x17, 0x70, 0x8B, 0xBD, 0xF8, 0xD5, 0xF9, 0x3A, 0xD3, 0x25, 0xB2, 0x66, 0x92, 0xBA, 0x43, 0x8A, 0x81, 0x52, 0xF, 0x64, 0x98, 0xFF, 0x60, 0x37, 0xAF, 0xB4, 0x11, 0x8C, 0xF9, 0x2E, 0xC5, 0xEE, 0xCA, 0xB4, 0x41, 0x60, 0x3C, 0x7D, 0x2, 0xAF, 0xA1, 0x2B, 0x9B, 0x22, 0x4B, 0x3B, 0xFC, 0xD2, 0x5D, 0x73, 0xE9, 0x29, 0x34, 0x91, 0x85, 0x93, 0x4C, 0xBE, 0xBE, 0x73, 0xA9, 0xD2, 0x3B, 0x27, 0x7A, 0x47, 0x76, 0xEC, 0xB0, 0x28, 0xC9, 0xC1, 0xDA, 0xEE, 0xAA, 0xB3, 0x96, 0x9C, 0x1E, 0xF5, 0x6B, 0xF6, 0x64, 0xD8, 0x94, 0x2E, 0xF1, 0xF7, 0x14, 0x5F, 0xA0, 0xF1, 0xA3, 0xB9, 0xB1, 0xAA, 0x58, 0x97, 0xDC, 0x9, 0x17, 0xC, 0x4, 0xD3, 0x8E, 0x2, 0x2C, 0x83, 0x8A, 0xD6, 0xAF, 0x7C, 0xFE, 0x83, 0x33, 0xC6, 0xA8, 0xC3, 0x84, 0xEF, 0x29, 0x6, 0xA9, 0xB7, 0x2D, 0x6, 0xB, 0xD, 0x6F, 0x70, 0x9E, 0x34, 0xA6, 0xC7, 0x31, 0xBE, 0x56, 0xDE, 0xDD, 0x2, 0x92, 0xF8, 0xA0, 0x58, 0xB, 0xFC, 0xFA, 0xBA, 0x49, 0xB4, 0x48, 0xDB, 0xEC, 0x25, 0xF3, 0x18, 0x8F, 0x2D, 0xB3, 0xC0, 0xB8, 0xDD, 0xBC, 0xD6, 0xAA, 0xA6, 0xDB, 0x6F, 0x7D, 0x7D, 0x25, 0xA6, 0xCD, 0x39, 0x6D, 0xDA, 0x76, 0xC, 0x79, 0xBF, 0x48, 0x25, 0xFC, 0x2D, 0xC5, 0xFA, 0x53, 0x9B, 0x4D, 0x60, 0xF4, 0xEF, 0xC7, 0xEA, 0xAC, 0xA1, 0x7B, 0x3, 0xF4, 0xAF, 0xC7 }, true);
        BigInteger sig = new(Signature, true);
        BigInteger res = BigInteger.ModPow(sig, power, pmod);
        var result = res.ToByteArray(true);
        byte[] digest;
        var properResult = new byte[256];
        for (int i = 0, loopTo = properResult.Length - 1; i <= loopTo; i++)
        {
            properResult[i] = 0xBB;
        }

        properResult[0x100 - 1] = 0xB;
        var tmpKey = "MAIEV.MOD";
        var bKey = new byte[tmpKey.Length];
        for (int i = 0, loopTo1 = tmpKey.Length - 1; i <= loopTo1; i++)
        {
            bKey[i] = (byte)Strings.Asc(tmpKey[i]);
        }

        var newData = new byte[(DataLen + bKey.Length)];
        Array.Copy(Data, 0, newData, 0, DataLen);
        Array.Copy(bKey, 0, newData, DataLen, bKey.Length);
        SHA1Managed sha1 = new();
        digest = sha1.ComputeHash(newData);
        Array.Copy(digest, 0, properResult, 0, digest.Length);
        for (int i = 0, loopTo2 = result.Length - 1; i <= loopTo2; i++)
        {
            if (result[i] != properResult[i])
            {
                return false;
            }
        }

        return true;
    }

    public static byte[] CreateSignature(byte[] Data, int DataLen)
    {
        byte[] digest;
        var properResult = new byte[256];
        for (int i = 0, loopTo = properResult.Length - 1; i <= loopTo; i++)
        {
            properResult[i] = 0xBB;
        }

        properResult[0x100 - 1] = 0xB;
        var tmpKey = "MAIEV.MOD";
        var bKey = new byte[tmpKey.Length];
        for (int i = 0, loopTo1 = tmpKey.Length - 1; i <= loopTo1; i++)
        {
            bKey[i] = (byte)Strings.Asc(tmpKey[i]);
        }

        var newData = new byte[(DataLen + bKey.Length)];
        Array.Copy(Data, 0, newData, 0, DataLen);
        Array.Copy(bKey, 0, newData, DataLen, bKey.Length);
        SHA1Managed sha1 = new();
        digest = sha1.ComputeHash(newData);
        Array.Copy(digest, 0, properResult, 0, digest.Length);
        BigInteger power = new(new byte[] { 0x1, 0x3, 0x3, 0x7, 0x0, 0xD, 0xE, 0xA, 0xD, 0xF, 0x0, 0x0, 0xD }, true); // Notice our own little private key (original wow clients won't accept this)
        BigInteger pmod = new(new byte[] { 0x6B, 0xCE, 0xF5, 0x2D, 0x2A, 0x7D, 0x7A, 0x67, 0x21, 0x21, 0x84, 0xC9, 0xBC, 0x25, 0xC7, 0xBC, 0xDF, 0x3D, 0x8F, 0xD9, 0x47, 0xBC, 0x45, 0x48, 0x8B, 0x22, 0x85, 0x3B, 0xC5, 0xC1, 0xF4, 0xF5, 0x3C, 0xC, 0x49, 0xBB, 0x56, 0xE0, 0x3D, 0xBC, 0xA2, 0xD2, 0x35, 0xC1, 0xF0, 0x74, 0x2E, 0x15, 0x5A, 0x6, 0x8A, 0x68, 0x1, 0x9E, 0x60, 0x17, 0x70, 0x8B, 0xBD, 0xF8, 0xD5, 0xF9, 0x3A, 0xD3, 0x25, 0xB2, 0x66, 0x92, 0xBA, 0x43, 0x8A, 0x81, 0x52, 0xF, 0x64, 0x98, 0xFF, 0x60, 0x37, 0xAF, 0xB4, 0x11, 0x8C, 0xF9, 0x2E, 0xC5, 0xEE, 0xCA, 0xB4, 0x41, 0x60, 0x3C, 0x7D, 0x2, 0xAF, 0xA1, 0x2B, 0x9B, 0x22, 0x4B, 0x3B, 0xFC, 0xD2, 0x5D, 0x73, 0xE9, 0x29, 0x34, 0x91, 0x85, 0x93, 0x4C, 0xBE, 0xBE, 0x73, 0xA9, 0xD2, 0x3B, 0x27, 0x7A, 0x47, 0x76, 0xEC, 0xB0, 0x28, 0xC9, 0xC1, 0xDA, 0xEE, 0xAA, 0xB3, 0x96, 0x9C, 0x1E, 0xF5, 0x6B, 0xF6, 0x64, 0xD8, 0x94, 0x2E, 0xF1, 0xF7, 0x14, 0x5F, 0xA0, 0xF1, 0xA3, 0xB9, 0xB1, 0xAA, 0x58, 0x97, 0xDC, 0x9, 0x17, 0xC, 0x4, 0xD3, 0x8E, 0x2, 0x2C, 0x83, 0x8A, 0xD6, 0xAF, 0x7C, 0xFE, 0x83, 0x33, 0xC6, 0xA8, 0xC3, 0x84, 0xEF, 0x29, 0x6, 0xA9, 0xB7, 0x2D, 0x6, 0xB, 0xD, 0x6F, 0x70, 0x9E, 0x34, 0xA6, 0xC7, 0x31, 0xBE, 0x56, 0xDE, 0xDD, 0x2, 0x92, 0xF8, 0xA0, 0x58, 0xB, 0xFC, 0xFA, 0xBA, 0x49, 0xB4, 0x48, 0xDB, 0xEC, 0x25, 0xF3, 0x18, 0x8F, 0x2D, 0xB3, 0xC0, 0xB8, 0xDD, 0xBC, 0xD6, 0xAA, 0xA6, 0xDB, 0x6F, 0x7D, 0x7D, 0x25, 0xA6, 0xCD, 0x39, 0x6D, 0xDA, 0x76, 0xC, 0x79, 0xBF, 0x48, 0x25, 0xFC, 0x2D, 0xC5, 0xFA, 0x53, 0x9B, 0x4D, 0x60, 0xF4, 0xEF, 0xC7, 0xEA, 0xAC, 0xA1, 0x7B, 0x3, 0xF4, 0xAF, 0xC7 }, true);
        BigInteger prop = new(properResult, true);
        BigInteger sig = BigInteger.ModPow(prop, power, pmod);
        var result = sig.ToByteArray(true);
        return result;
    }
}
