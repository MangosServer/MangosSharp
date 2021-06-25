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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Mangos.Extractor
{
    public static class Functions
    {
        public static int SearchInFile(Stream f, string s, int o = 0)
        {
            f.Seek(0L, SeekOrigin.Begin);
            BinaryReader r = new(f);
            byte[] b1 = r.ReadBytes((int)f.Length);
            byte[] b2 = Encoding.ASCII.GetBytes(s);
            for (int i = o, loopTo = b1.Length - 1; i <= loopTo; i++)
            {
                for (int j = 0, loopTo1 = b2.Length - 1; j <= loopTo1; j++)
                {
                    if (b1[i + j] != b2[j])
                    {
                        break;
                    }

                    if (j == b2.Length - 1)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        public static int SearchInFile(Stream f, int v)
        {
            f.Seek(0L, SeekOrigin.Begin);
            BinaryReader r = new(f);
            byte[] b1 = r.ReadBytes((int)f.Length);
            byte[] b2 = BitConverter.GetBytes(v);
            // Array.Reverse(b2)

            for (int i = 0, loopTo = b1.Length - 1; i <= loopTo; i++)
            {
                if (i + 3 >= b1.Length)
                {
                    break;
                }

                if (b1[i] == b2[0] && b1[i + 1] == b2[1] && b1[i + 2] == b2[2] && b1[i + 3] == b2[3])
                {
                    return i;
                }
            }

            return -1;
        }

        public static string ReadString(FileStream f)
        {
            string r = "";
            byte t;

            // Read if there are zeros
            t = (byte)f.ReadByte();
            while (t == 0)
            {
                t = (byte)f.ReadByte();
            }

            // Read string
            while (t != 0)
            {
                r += Conversions.ToString((char)t);
                t = (byte)f.ReadByte();
            }

            return r;
        }

        public static string ReadString(FileStream f, long pos)
        {
            string r = "";
            byte t;
            if (pos == -1)
            {
                return "*Nothing*";
            }

            f.Seek(pos, SeekOrigin.Begin);
            try
            {
                // Read if there are zeros
                t = (byte)f.ReadByte();
                while (t == 0)
                {
                    t = (byte)f.ReadByte();
                }

                // Read string
                while (t != 0)
                {
                    r += Conversions.ToString((char)t);
                    t = (byte)f.ReadByte();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("ReadString has thrown an Exception! The string is {0}", e.Message);
            }

            return r;
        }

        public static string ToField(string sField)
        {
            // Make the first letter in upper case and the rest in lower case
            string tmp = sField.Substring(0, 1).ToUpper() + sField[1..].ToLower();
            // Replace lowercase object with Object (used in f.ex Gameobject -> GameObject)
            if (tmp.IndexOf("object", StringComparison.OrdinalIgnoreCase) > 0)
            {
                tmp = tmp.Length > tmp.IndexOf("object", StringComparison.OrdinalIgnoreCase) + 6
                    ? tmp.Substring(0, tmp.IndexOf("object")) + "Object" + tmp[(tmp.IndexOf("object") + 6)..]
                    : tmp.Substring(0, tmp.IndexOf("object")) + "Object";
            }

            return tmp;
        }

        public static string ToType(int iType)
        {
            // Get the typename
            switch (iType)
            {
                case 1:
                    {
                        return "INT";
                    }

                case 2:
                    {
                        return "TWO_SHORT";
                    }

                case 3:
                    {
                        return "FLOAT";
                    }

                case 4:
                    {
                        return "GUID";
                    }

                case 5:
                    {
                        return "BYTES";
                    }

                default:
                    {
                        return "UNK (" + iType + ")";
                    }
            }
        }

        private static void AddFlag(ref string sFlags, string sFlag)
        {
            if (!string.IsNullOrEmpty(sFlags))
            {
                sFlags += " + ";
            }

            sFlags += sFlag;
        }

        public static string ToFlags(int iFlags)
        {
            string tmp = "";
            if (iFlags == 0)
            {
                tmp = "NONE";
            }

            if (Conversions.ToBoolean(iFlags & 1))
            {
                AddFlag(ref tmp, "PUBLIC");
            }

            if (Conversions.ToBoolean(iFlags & 2))
            {
                AddFlag(ref tmp, "PRIVATE");
            }

            if (Conversions.ToBoolean(iFlags & 4))
            {
                AddFlag(ref tmp, "OWNER_ONLY");
            }

            if (Conversions.ToBoolean(iFlags & 8))
            {
                AddFlag(ref tmp, "UNK1");
            }

            if (Conversions.ToBoolean(iFlags & 16))
            {
                AddFlag(ref tmp, "UNK2");
            }

            if (Conversions.ToBoolean(iFlags & 32))
            {
                AddFlag(ref tmp, "SPECIAL_INFO");
            }

            if (Conversions.ToBoolean(iFlags & 64))
            {
                AddFlag(ref tmp, "GROUP_ONLY");
            }

            if (Conversions.ToBoolean(iFlags & 128))
            {
                AddFlag(ref tmp, "UNK5");
            }

            if (Conversions.ToBoolean(iFlags & 256))
            {
                AddFlag(ref tmp, "DYNAMIC");
            }

            return tmp;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TypeEntry
        {
            public int Name;
            public int Offset;
            public int Size;
            public int Type;
            public int Flags;
        }

        public enum Types
        {
            NULL,
            Int32,
            Chars,
            Float,
            GUID,
            Bytes,
            NULL2
        }

        public static void ExtractUpdateFields()
        {
            FileVersionInfo versInfo = FileVersionInfo.GetVersionInfo("Wow.exe");
            FileStream f = new("wow.exe", FileMode.Open, FileAccess.Read, FileShare.Read, 10000000);
            FileStream o = new("Global.UpdateFields.h", FileMode.Create, FileAccess.Write, FileShare.None, 1024);
            StreamWriter w = new(o);

            // this is right after the data for size, type and flags of update fields begins
            int OBJECT_FIELD_GUID_PointerAddress = SearchInFile(f, "\u0000\u0000\u0000\u0000\u0002\u0000\u0000\u0000\u0004\u0000\u0000\u0000\u0001\u0000\u0000\u0000");
            if (OBJECT_FIELD_GUID_PointerAddress == -1)
            {
                MessageBox.Show("Cannot find where data for update field types begins!");
                return;
            }

            // read the uint32 before that address
            OBJECT_FIELD_GUID_PointerAddress -= 4;
            f.Seek(OBJECT_FIELD_GUID_PointerAddress, SeekOrigin.Begin);
            byte[] Buffer = new byte[4];
            f.Read(Buffer, 0, 4);
            uint OBJECT_FIELD_GUID_Pointer = BitConverter.ToUInt32(Buffer, 0);

            // find the address of the first update field name
            int OBJECT_FIELD_GUID_NameAddress = SearchInFile(f, "OBJECT_FIELD_GUID");
            if (OBJECT_FIELD_GUID_NameAddress == -1)
            {
                MessageBox.Show("Cannot find OBJECT_FIELD_GUID string!");
                return;
            }
            // substract the address of the name to get the offset
            uint AddressOffset = OBJECT_FIELD_GUID_Pointer - (uint)OBJECT_FIELD_GUID_NameAddress;

            int FieldTypesBegin = OBJECT_FIELD_GUID_PointerAddress;
            int FieldNamesBegin = SearchInFile(f, "AREATRIGGER_FINAL_POS");
            if (FieldNamesBegin == -1)
            {
                FieldNamesBegin = SearchInFile(f, "CORPSE_FIELD_PAD");
            }
            if (FieldNamesBegin == -1)
            {
                FieldNamesBegin = SearchInFile(f, "CORPSE_FIELD_FLAGS");
            }
            if (FieldNamesBegin == -1)
            {
                FieldNamesBegin = SearchInFile(f, "CORPSE_FIELD_LEVEL");
            }
            if (FieldNamesBegin == -1)
            {
                MessageBox.Show("Cannot find last update field name!");
            }
            else
            {
                List<string> Names = new();
                string Last = "";
                int Offset = FieldNamesBegin;
                f.Seek(Offset, SeekOrigin.Begin);
                while (Last != "OBJECT_FIELD_GUID")
                {
                    Last = ReadString(f);
                    Names.Add(Last);
                }

                List<TypeEntry> Info = new();
                int Temp;
                Offset = 0;
                f.Seek(FieldTypesBegin, SeekOrigin.Begin);
                for (int i = 0, loopTo = Names.Count - 1; i <= loopTo; i++)
                {
                    f.Seek(FieldTypesBegin + (i * 5 * 4) + Offset, SeekOrigin.Begin);
                    f.Read(Buffer, 0, 4);
                    Temp = BitConverter.ToInt32(Buffer, 0);
                    if (Temp < 0xFFFF)
                    {
                        i -= 1;
                        Offset += 4;
                        continue;
                    }

                    TypeEntry tmp = new()
                    {
                        Name = Temp
                    };
                    f.Read(Buffer, 0, 4);
                    Temp = BitConverter.ToInt32(Buffer, 0);
                    tmp.Offset = Temp;
                    f.Read(Buffer, 0, 4);
                    Temp = BitConverter.ToInt32(Buffer, 0);
                    tmp.Size = Temp;
                    f.Read(Buffer, 0, 4);
                    Temp = BitConverter.ToInt32(Buffer, 0);
                    tmp.Type = Temp;
                    f.Read(Buffer, 0, 4);
                    Temp = BitConverter.ToInt32(Buffer, 0);
                    tmp.Flags = Temp;
                    Info.Add(tmp);
                }

                MessageBox.Show(string.Format("{0} fields extracted.", Names.Count));
                w.WriteLine("// Auto generated file");
                w.WriteLine("// Patch: " + versInfo.FileMajorPart + "." + versInfo.FileMinorPart + "." + versInfo.FileBuildPart);
                w.WriteLine("// Build: " + versInfo.FilePrivatePart);
                w.WriteLine();
                string LastFieldType = "";
                string sName;
                string sField;
                int BasedOn = 0;
                string BasedOnName = "";
                Dictionary<string, int> EndNum = new();

                // on first iteration just add the enum names
                for (int j = 0, loopTo1 = Info.Count - 1; j <= loopTo1; j++)
                {
                    long nextAddress = Info[j].Name - AddressOffset;
                    if (nextAddress < 0 || nextAddress > f.Length)
                    {
                        MessageBox.Show("Wrong address for update field name! " + nextAddress);
                        continue;
                    }
                    sName = ReadString(f, nextAddress);
                    if (!string.IsNullOrEmpty(sName))
                    {
                        sField = ToField(sName.Substring(0, sName.IndexOf("_")));
                        if (sName is "UINT_FIELD_BASESTAT0" or
                            "UINT_FIELD_BASESTAT1" or
                            "UINT_FIELD_BASESTAT2" or
                            "UINT_FIELD_BASESTAT3" or
                            "UINT_FIELD_BASESTAT4" or
                            "UINT_FIELD_BYTES_1")
                        {
                            sField = "Unit";
						}
                        if (sName == "OBJECT_FIELD_CREATED_BY")
                            sField = "GameObject";
                        if ((LastFieldType ?? "") != (sField ?? ""))
                        {
                            if (!string.IsNullOrEmpty(LastFieldType))
                            {
                                EndNum.Add(LastFieldType, Info[j - 1].Offset + 1);
                            }
                            LastFieldType = sField;
                        }
                    }
                }

                sField = "";
                LastFieldType = "";

                for (int j = 0, loopTo1 = Info.Count - 1; j <= loopTo1; j++)
                {
                    long nextAddress = Info[j].Name - AddressOffset;
                    if (nextAddress < 0 || nextAddress > f.Length)
                    {
                        MessageBox.Show("Wrong address for update field name! " + nextAddress);
                        w.WriteLine("// An error occurred while reading field at this spot");
                        continue;
                    }
                    sName = ReadString(f, nextAddress);
                    if (!string.IsNullOrEmpty(sName))
                    {
                        sField = ToField(sName.Substring(0, sName.IndexOf("_")));
                        if (sName is "UINT_FIELD_BASESTAT0" or
                            "UINT_FIELD_BASESTAT1" or
                            "UINT_FIELD_BASESTAT2" or
                            "UINT_FIELD_BASESTAT3" or
                            "UINT_FIELD_BASESTAT4" or
                            "UINT_FIELD_BYTES_1")
                        {
                            sField = "Unit";
                        }
                        if (sName == "OBJECT_FIELD_CREATED_BY")
                            sField = "GameObject";
                        if ((LastFieldType ?? "") != (sField ?? ""))
                        {
                            if (!string.IsNullOrEmpty(LastFieldType))
                            {;
                                if (LastFieldType.ToLower() == "object")
                                {
                                    w.WriteLine("    {0,-48} = {1,-20}", LastFieldType.ToUpper() + "_END", "0x" + Conversion.Hex(Info[j - 1].Offset + Info[j - 1].Size));
                                }
                                else
                                {
                                    w.WriteLine("    {0,-48} = {1,-20}// 0x{2:X3}", LastFieldType.ToUpper() + "_END", BasedOnName.Substring(BasedOnName.IndexOf('.') + 1) + " + 0x" + Conversion.Hex(Info[j - 1].Offset + Info[j - 1].Size), BasedOn + Info[j - 1].Offset + Info[j - 1].Size);
                                }

                                w.WriteLine("};");
                            }

                            w.WriteLine("enum E" + sField + "Fields");
                            w.WriteLine("{");

                            if (sField.ToLower() == "container")
                            {
                                BasedOn = EndNum["Item"];
                                BasedOnName = "EItemFields.ITEM_END";
                            }
                            else if (sField.ToLower() == "player")
                            {
                                BasedOn = EndNum["Unit"];
                                BasedOnName = "EUnitFields.UNIT_END";
                            }
                            else if (sField.ToLower() != "object")
                            {
                                BasedOn = EndNum["Object"];
                                BasedOnName = "EObjectFields.OBJECT_END";
                            }

                            LastFieldType = sField;
                        }

                        if (BasedOn > 0)
                        {
                            w.WriteLine("    {0,-48} = {1,-20}// 0x{2:X3} - Size: {3} - Type: {4} - Flags: {5}", sName, BasedOnName.Substring(BasedOnName.IndexOf('.') + 1) + " + 0x" + Conversion.Hex(Info[j].Offset) + ",", BasedOn + Info[j].Offset, Info[j].Size, ToType(Info[j].Type), ToFlags(Info[j].Flags));
                        }
                        else
                        {
                            w.WriteLine("    {0,-48} = {1,-20}// 0x{2:X3} - Size: {3} - Type: {4} - Flags: {5}", sName, "0x" + Conversion.Hex(Info[j].Offset) + ",", Info[j].Offset, Info[j].Size, ToType(Info[j].Type), ToFlags(Info[j].Flags));
                        }
                    }
                }

                if (!string.IsNullOrEmpty(LastFieldType))
				{
                    w.WriteLine("    {0,-48} = {1,-20}// 0x{2:X3}", LastFieldType.ToUpper() + "_END", BasedOnName.Substring(BasedOnName.IndexOf('.') + 1) + " + 0x" + Conversion.Hex(Info[^1].Offset + Info[^1].Size), BasedOn + Info[^1].Offset + Info[^1].Size);
				}

                w.WriteLine("};");
                w.Flush();
            }

            o.Close();
            f.Close();
        }

        public static void ExtractOpcodes()
        {
            FileVersionInfo versInfo = FileVersionInfo.GetVersionInfo("Wow.exe");
            FileStream f = new("wow.exe", FileMode.Open, FileAccess.Read, FileShare.Read, 10000000);
            FileStream o = new("Global.Opcodes.cs", FileMode.Create, FileAccess.Write, FileShare.None, 1024);
            StreamWriter w = new(o);

            int START = SearchInFile(f, "NUM_MSG_TYPES");
            int END = SearchInFile(f, "CMSG_BOOTME");
            if (START == -1 || END == -1)
            {
                MessageBox.Show("Client does not contain opcodes enum!");
            }
            else
            {
                Stack<string> Names = new();
                string Last = "";
                f.Seek(START, SeekOrigin.Begin);
                while (Last != "MSG_NULL_ACTION")
                {
                    Last = ReadString(f);
                    Names.Push(Last);
                }

                MessageBox.Show(string.Format("{0} opcodes extracted.", Names.Count));
                w.WriteLine("// Auto generated file");
                w.WriteLine("// Patch: " + versInfo.FileMajorPart + "." + versInfo.FileMinorPart + "." + versInfo.FileBuildPart);
                w.WriteLine("// Build: " + versInfo.FilePrivatePart);
                w.WriteLine();
                w.WriteLine("Public Enum OPCODES");
                w.WriteLine("{");
                int i = 0;
                while (Names.Count > 0)
                {
                    w.WriteLine("    {0,-64}// 0x{1:X3}", Names.Pop() + "=" + i + ",", i);
                    i += 1;
                }

                w.WriteLine("}");
                w.Flush();
            }

            o.Close();
            f.Close();
        }

        public static void ExtractSpellFailedReason()
        {
            FileVersionInfo versInfo = FileVersionInfo.GetVersionInfo("Wow.exe");
            FileStream f = new("wow.exe", FileMode.Open, FileAccess.Read, FileShare.Read, 10000000);
            FileStream o = new("Global.SpellFailedReasons.cs", FileMode.Create, FileAccess.Write, FileShare.None, 1024);
            StreamWriter w = new(o);

            int LastCastReasonAddress = SearchInFile(f, "SPELL_FAILED_UNKNOWN");
            if (LastCastReasonAddress == -1)
            {
                MessageBox.Show("Client does not contain cast result enum!");
            }
            else
            {
                Stack<string> Names = new();
                string Last = "";
                int Offset = LastCastReasonAddress;
                f.Seek(Offset, SeekOrigin.Begin);
                while (Last.Length == 0 || (Last.Length > 13 && Last.Substring(0, 13) == "SPELL_FAILED_"))
                {
                    Last = ReadString(f);
                    if (Last.Length > 13 && Last.Substring(0, 13) == "SPELL_FAILED_")
					{
                        Names.Push(Last);
                    }
                }

                MessageBox.Show(string.Format("{0} spell failed reasons extracted.", Names.Count));
                w.WriteLine("// Auto generated file");
                w.WriteLine("// Patch: " + versInfo.FileMajorPart + "." + versInfo.FileMinorPart + "." + versInfo.FileBuildPart);
                w.WriteLine("// Build: " + versInfo.FilePrivatePart);
                w.WriteLine();
                w.WriteLine("Public Enum SpellFailedReason");
                w.WriteLine("{");
                int i = 0;
                while (Names.Count > 0)
                {
                    w.WriteLine("    {0,-64}// 0x{1:X3}", Names.Pop() + " = 0x" + Conversion.Hex(i) + ",", i);
                    i += 1;
                }

                w.WriteLine("    {0,-64}// 0x{1:X3}", "SPELL_NO_ERROR = 0x" + Conversion.Hex(255), 255);
                w.WriteLine("}");
                w.Flush();
            }

            o.Close();
            f.Close();
        }

        public static void ExtractChatTypes()
        {
            FileVersionInfo versInfo = FileVersionInfo.GetVersionInfo("Wow.exe");
            FileStream f = new("wow.exe", FileMode.Open, FileAccess.Read, FileShare.Read, 10000000);
            FileStream o = new("Global.ChatTypes.cs", FileMode.Create, FileAccess.Write, FileShare.None, 1024);
            StreamWriter w = new(o);

            // this string is present before the chat msg enum in all versions
            int START = SearchInFile(f, "LANGUAGE_LIST_CHANGED");
            if (START == -1)
            {
                MessageBox.Show("Cannot determine where chat msg enum starts!");
                return;
            }
            START += ("LANGUAGE_LIST_CHANGED").Length;

            // skin any null bytes after it
            f.Seek(START, SeekOrigin.Begin);
            do
            {
                int chr = f.ReadByte();
                if (chr == -1)
                {
                    MessageBox.Show("Cannot determine where chat msg enum starts!");
                    return;
                }
                if (chr != 0)
                    break;
                START++;
            } while (true);

            Stack<string> Names = new();
            string Last = "";
            int Offset = START;
            f.Seek(Offset, SeekOrigin.Begin);
            while (Last.Length == 0 || Last.Substring(0, 9) == "CHAT_MSG_" || Last.Substring(0, 9) == "RAID_BOSS")
            {
                Last = ReadString(f);
                if (Last.Length > 10 && Last.Substring(0, 9) == "CHAT_MSG_" || Last.Substring(0, 9) == "RAID_BOSS")
				{
                    Names.Push(Last);
				}
            }

            MessageBox.Show(string.Format("{0} chat types extracted.", Names.Count));
            w.WriteLine("// Auto generated file");
            w.WriteLine("// Patch: " + versInfo.FileMajorPart + "." + versInfo.FileMinorPart + "." + versInfo.FileBuildPart);
            w.WriteLine("// Build: " + versInfo.FilePrivatePart);
            w.WriteLine();
            w.WriteLine("Public Enum ChatMsg");
            w.WriteLine("{");
            int i = 0;
            while (Names.Count > 0)
            {
                w.WriteLine("    {0,-64}// 0x{1:X3}", Names.Pop() + " = 0x" + Conversion.Hex(i) + ",", i);
                i += 1;
            }

            w.WriteLine("}");
            w.Flush();

            o.Close();
            f.Close();
        }
    }
}