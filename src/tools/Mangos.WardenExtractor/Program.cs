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

namespace Mangos.WardenExtractor;

internal static class Program
{
    public static void Main()
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("WardenExtractor by UniX");
        Console.WriteLine("");
        Console.WriteLine("");
    TryAgain:
        ;
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("Menu:");
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine("1: Extract Warden Modules from WDB file");
        Console.WriteLine("2: Converts all .mod files in the directory to .dll");
        Console.WriteLine("3: Converts all .dll files in the directory to .mod");
        Console.WriteLine("4: Converts WDB to the new version format");
        Console.WriteLine("5: Quit this program");
        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        Console.Write("Your choice: ");
        var sInput = Console.ReadLine();
        if (sInput == "1")
        {
            Module_CacheExtract.ExtractCache();
        }
        else if (sInput == "2")
        {
            Module_ModuleToDll.ModulesToDlls();
        }
        else if (sInput == "3")
        {
            Module_ModuleToDll.DllsToModules();
        }
        else if (sInput == "4")
        {
            Module_CacheExtract.ConvertWDB();
        }
        else if (sInput == "5")
        {
            Environment.Exit(0);
        }

        goto TryAgain;
    }

    public static string ToHex(ref byte[] bBytes)
    {
        var tmpStr = "";
        for (int i = 0, loopTo = bBytes.Length - 1; i <= loopTo; i++)
        {
            if (bBytes[i] < 16)
            {
                tmpStr += "0" + Conversion.Hex(bBytes[i]);
            }
            else
            {
                tmpStr += Conversion.Hex(bBytes[i]);
            }
        }

        return tmpStr;
    }

    public static string Reverse(string str)
    {
        var tmpStr = "";
        for (var i = str.Length - 1; i >= 0; i -= 1)
        {
            tmpStr += Conversions.ToString(str[i]);
        }

        return tmpStr;
    }

    public static byte[] Reverse(byte[] bytes)
    {
        if (bytes.Length == 0)
        {
            return Array.Empty<byte>();
        }

        var tmpBytes = new byte[bytes.Length];
        for (var i = bytes.Length - 1; i >= 0; i -= 1)
        {
            tmpBytes[bytes.Length - 1 - i] = bytes[i];
        }

        return tmpBytes;
    }

    public static byte[] ParseKey(string str)
    {
        if (str.Length == 0)
        {
            return Array.Empty<byte>();
        }

        var bBytes = new byte[Conversion.Int((str.Length - 1) / 2) + 1];
        for (int i = 0, loopTo = str.Length - 1; i <= loopTo; i += 2)
        {
            try
            {
                bBytes[Conversion.Int(i / 2)] = i + 1 >= str.Length - 1 ? Conversions.ToByte("&H" + str[i]) : Conversions.ToByte("&H" + str[i] + str[i + 1]);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write("ParseKey has thrown an Exception! The exception is {0}", e);
            }
        }

        return bBytes;
    }

    public class RC4
    {
        // http://www.skullsecurity.org/wiki/index.php/Crypto_and_Hashing

        public static byte[] Init(byte[] @base)
        {
            var val = 0;
            var position = 0;
            byte temp;
            var key = new byte[258];
            for (var i = 0; i <= 256 - 1; i++)
            {
                key[i] = (byte)i;
            }

            key[256] = 0;
            key[257] = 0;
            for (var i = 1; i <= 64; i++)
            {
                val = val + key[(i * 4) - 4] + @base[position % @base.Length];
                val &= 0xFF;
                position += 1;
                temp = key[(i * 4) - 4];
                key[(i * 4) - 4] = key[val & 0xFF];
                key[val & 0xFF] = temp;
                val = val + key[(i * 4) - 3] + @base[position % @base.Length];
                val &= 0xFF;
                position += 1;
                temp = key[(i * 4) - 3];
                key[(i * 4) - 3] = key[val & 0xFF];
                key[val & 0xFF] = temp;
                val = val + key[(i * 4) - 2] + @base[position % @base.Length];
                val &= 0xFF;
                position += 1;
                temp = key[(i * 4) - 2];
                key[(i * 4) - 2] = key[val & 0xFF];
                key[val & 0xFF] = temp;
                val = val + key[(i * 4) - 1] + @base[position % @base.Length];
                val &= 0xFF;
                position += 1;
                temp = key[(i * 4) - 1];
                key[(i * 4) - 1] = key[val & 0xFF];
                key[val & 0xFF] = temp;
            }

            return key;
        }

        public static void Crypt(ref byte[] data, byte[] key)
        {
            byte temp;
            for (int i = 0, loopTo = data.Length - 1; i <= loopTo; i++)
            {
                key[256] = (byte)((Conversions.ToInteger(key[256]) + 1) & 0xFF);
                key[257] = (byte)((Conversions.ToInteger(key[257]) + Conversions.ToInteger(key[key[256]])) & 0xFF);
                temp = key[key[257] & 0xFF];
                key[key[257] & 0xFF] = key[key[256] & 0xFF];
                key[key[256] & 0xFF] = temp;
                data[i] = (byte)(data[i] ^ key[(Conversions.ToInteger(key[key[257]]) + Conversions.ToInteger(key[key[256]])) & 0xFF]);
            }
        }
    }
}
