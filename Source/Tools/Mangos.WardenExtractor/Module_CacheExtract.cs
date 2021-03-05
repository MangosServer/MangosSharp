//
//  Copyright (C) 2013-2021 getMaNGOS <https://getmangos.eu>
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

using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.IO;

namespace Mangos.WardenExtractor
{
    public static class Module_CacheExtract
    {
        public static void ExtractCache()
        {
            Console.Write("Name of WDB: ");
            string sWDB = Console.ReadLine();
            if (File.Exists(sWDB) == false)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("The file [{0}] did not exist.", sWDB);
                return;
            }

            FileStream fs = new(sWDB, FileMode.Open, FileAccess.Read, FileShare.Read);
            BinaryReader br = new(fs);
            string Header = Conversions.ToString(br.ReadChars(4));
            uint Version = br.ReadUInt32();
            string Lang = Program.Reverse(Conversions.ToString(br.ReadChars(4)));
            byte[] Unk = br.ReadBytes(8);
            Console.ForegroundColor = ConsoleColor.White;
            Directory.CreateDirectory("Modules");
            while (fs.Position + 20L <= fs.Length)
            {
                byte[] argbBytes = br.ReadBytes(16);
                string ModName = Program.ToHex(ref argbBytes);
                int DataLen = br.ReadInt32();
                if (DataLen == 0)
                {
                    continue;
                }

                int ModLen = br.ReadInt32();
                byte[] ModData = new byte[ModLen];
                br.Read(ModData, 0, ModLen);
                FileStream fs2 = new(@"Modules\" + ModName + ".mod", FileMode.Create, FileAccess.Write, FileShare.None);
                fs2.Write(ModData, 0, ModLen);
                fs2.Close();
                fs2.Dispose();
                Console.WriteLine("Module: {0} [{1} bytes]", ModName, ModLen);
            }

            fs.Close();
            fs.Dispose();
        }

        public static void ConvertWDB()
        {
            Console.Write("Name of WDB: ");
            string sWDB = Console.ReadLine();
            if (File.Exists(sWDB) == false)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("The file [{0}] did not exist.", sWDB);
                return;
            }

            FileStream fs = new(sWDB, FileMode.Open, FileAccess.Read, FileShare.Read);
            BinaryReader br = new(fs);
            MemoryStream ms = new();
            BinaryWriter bw = new(ms);
            string Header = Conversions.ToString(br.ReadChars(4));
            uint Version = br.ReadUInt32();
            string Lang = Program.Reverse(Conversions.ToString(br.ReadChars(4)));
            int Unk1 = br.ReadInt32();
            int Unk2 = br.ReadInt32();
            bw.Write((byte)Strings.Asc(Header[0]));
            bw.Write((byte)Strings.Asc(Header[1]));
            bw.Write((byte)Strings.Asc(Header[2]));
            bw.Write((byte)Strings.Asc(Header[3]));
            bw.Write(Version);
            bw.Write((byte)Strings.Asc(Lang[3]));
            bw.Write((byte)Strings.Asc(Lang[2]));
            bw.Write((byte)Strings.Asc(Lang[1]));
            bw.Write((byte)Strings.Asc(Lang[0]));
            bw.Write(Unk1);
            bw.Write(Unk2);
            Console.WriteLine("Unk1: {0}{1}Unk2: {2}", Unk1, Constants.vbCrLf, Unk2);
            Console.ForegroundColor = ConsoleColor.White;
            bw.Write(1); // Count of modules?
            while (fs.Position + 20L <= fs.Length)
            {
                byte[] byteName = br.ReadBytes(16);
                string ModName = Program.ToHex(ref byteName);
                int DataLen = br.ReadInt32();
                bw.Write(byteName, 0, byteName.Length);
                bw.Write(DataLen);
                if (DataLen == 0)
                {
                    continue;
                }

                int ModLen = br.ReadInt32();
                bw.Write(ModLen);
                byte[] ModData = new byte[ModLen];
                br.Read(ModData, 0, ModLen);
                bw.Write(ModData, 0, ModLen);
                Console.WriteLine("Module: {0} [{1} bytes]", ModName, ModLen);
            }

            fs.Close();
            fs.Dispose();
            FileStream fs2 = new(sWDB.Replace(Path.GetExtension(sWDB), "") + ".new.wdb", FileMode.Create, FileAccess.Write, FileShare.Read);
            byte[] newFile = ms.ToArray();
            fs2.Write(newFile, 0, newFile.Length);
            fs2.Close();
            fs2.Dispose();
            ms.Close();
            ms.Dispose();
        }
    }
}