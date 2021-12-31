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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Mangos.Extractor;

public static class Functions
{
    public static int SearchInFile(Stream f, string s, int o = 0)
    {
        f.Seek(0L, SeekOrigin.Begin);
        BinaryReader r = new(f);
        var b1 = r.ReadBytes((int)f.Length);
        var b2 = Encoding.ASCII.GetBytes(s);
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
        var b1 = r.ReadBytes((int)f.Length);
        var b2 = BitConverter.GetBytes(v);
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
        var r = "";
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
        var r = "";
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
        var tmp = sField.Substring(0, 1).ToUpper() + sField[1..].ToLower();
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
        var tmp = "";
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
            AddFlag(ref tmp, "UNK3");
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
        var TBC = 0;
        var alpha = 0;
        FileVersionInfo versInfo = FileVersionInfo.GetVersionInfo("Wow.exe");
        FileStream f = new("wow.exe", FileMode.Open, FileAccess.Read, FileShare.Read, 10000000);
        BinaryReader r1 = new(f);
        StreamReader r2 = new(f);
        FileStream o = new(versInfo.FileMajorPart + "." + versInfo.FileMinorPart + "." + versInfo.FileBuildPart + "." + versInfo.FilePrivatePart + "_Global.UpdateFields.cs", FileMode.Create, FileAccess.Write, FileShare.None, 1024);
        StreamWriter w = new(o);
        var FIELD_NAME_OFFSET = SearchInFile(f, "CORPSE_FIELD_PAD");
        var OBJECT_FIELD_GUID = SearchInFile(f, "OBJECT_FIELD_GUID") + 0x400000;
        var FIELD_TYPE_OFFSET = SearchInFile(f, OBJECT_FIELD_GUID);
#if DEBUG
        MessageBox.Show("FIELD_NAME_OFFSET " + FIELD_NAME_OFFSET + " OBJECT_FIELD_GUID " + OBJECT_FIELD_GUID + " FIELD_TYPE_OFFSET " + FIELD_TYPE_OFFSET);
#endif
        if (FIELD_NAME_OFFSET == -1) // pre 1.5 vanilla support
        {
            FIELD_NAME_OFFSET = SearchInFile(f, "CORPSE_FIELD_FLAGS");
        }
        if (FIELD_NAME_OFFSET == -1) // alpha support
        {
            FIELD_NAME_OFFSET = SearchInFile(f, "CORPSE_FIELD_LEVEL");
            alpha = 1;
        }
        if (FIELD_TYPE_OFFSET == -1) // TBC support
        {
            OBJECT_FIELD_GUID = SearchInFile(f, "OBJECT_FIELD_GUID") + 0x1A00 + 0x400000;
            FIELD_TYPE_OFFSET = SearchInFile(f, OBJECT_FIELD_GUID);
            TBC = 1;
        }
        if (FIELD_NAME_OFFSET == -1 || FIELD_TYPE_OFFSET == -1)
        {
            MessageBox.Show("Wrong offsets! " + FIELD_NAME_OFFSET + "  " + FIELD_TYPE_OFFSET);
        }
        else
        {
            List<string> Names = new();
            var Last = "";
            var Offset = FIELD_NAME_OFFSET;
            f.Seek(Offset, SeekOrigin.Begin);
            while (Last != "OBJECT_FIELD_GUID")
            {
                Last = ReadString(f);
                Names.Add(Last);
            }

            List<TypeEntry> Info = new();
            int Temp;
            var Buffer = new byte[4];
            Offset = 0;
            f.Seek(FIELD_TYPE_OFFSET, SeekOrigin.Begin);
            for (int i = 0, loopTo = Names.Count - 1; i <= loopTo; i++)
            {
                f.Seek(FIELD_TYPE_OFFSET + (i * 5 * 4) + Offset, SeekOrigin.Begin);
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
            w.WriteLine("// {0}", DateAndTime.Now);
            w.WriteLine("// Patch: " + versInfo.FileMajorPart + "." + versInfo.FileMinorPart + "." + versInfo.FileBuildPart);
            w.WriteLine("// Build: " + versInfo.FilePrivatePart);
            w.WriteLine();
            var LastFieldType = "";
            string sName;
            string sField;
            var BasedOn = 0;
            var BasedOnName = "";
            Dictionary<string, int> EndNum = new();
            for (int j = 0, loopTo1 = Info.Count - 1; j <= loopTo1; j++)
            {
                sName = ReadString(f, Info[j].Name - 0x400000);
                if (TBC == 1) // TBC support
                {
                    sName = ReadString(f, Info[j].Name - (0x1A00 + 0x400000));
                }
                if (!string.IsNullOrEmpty(sName))
                {
                    sField = ToField(sName.Substring(0, sName.IndexOf("_")));
                    if (sName == "OBJECT_FIELD_CREATED_BY" && alpha == 0)
                    {
                        sField = "GameObject";
                    }

                    if (sName is "UINT_FIELD_BASESTAT0" or // alpha support
                        "UINT_FIELD_BASESTAT1" or
                        "UINT_FIELD_BASESTAT2" or
                        "UINT_FIELD_BASESTAT3" or
                        "UINT_FIELD_BASESTAT4" or
                        "UINT_FIELD_BYTES_1")
                    {
                        sField = "Unit";
                    }

                    if ((LastFieldType ?? "") != (sField ?? ""))
                    {
                        if (!string.IsNullOrEmpty(LastFieldType))
                        {
                            EndNum.Add(LastFieldType, Info[j - 1].Offset + 1);
                            if (LastFieldType.ToLower() == "object")
                            {
                                w.WriteLine("    {0,-78}", LastFieldType.ToUpper() + "_END = 0x" + Conversion.Hex(Info[j - 1].Offset + Info[j - 1].Size));
                            }
                            else
                            {
                                w.WriteLine("    {0,-78}// 0x{1:X3}", LastFieldType.ToUpper() + "_END = " + BasedOnName + " + 0x" + Conversion.Hex(Info[j - 1].Offset + Info[j - 1].Size), BasedOn + Info[j - 1].Offset + Info[j - 1].Size);
                            }

                            w.WriteLine("}");
                        }

                        w.WriteLine("Public Enum E" + sField + "Fields");
                        w.WriteLine("{");
#if DEBUG
                        MessageBox.Show("sField: " + sField + "\nsName: " + sName);
#endif
                        if (TBC == 1) // TBC support
                        {
                            if (sField.ToLower() == "item")
                            {
                                BasedOn = EndNum["Container"];
                                BasedOnName = "EContainerFields.CONTAINER_END";
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
                        }

                        if (TBC == 0)
                        {
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
                        }

                        LastFieldType = sField;
                    }

                    if (BasedOn > 0)
                    {
                        w.WriteLine("    {0,-78}// 0x{1:X3} - Size: {2} - Type: {3} - Flags: {4}", sName + " = " + BasedOnName + " + 0x" + Conversion.Hex(Info[j].Offset) + ",", BasedOn + Info[j].Offset, Info[j].Size, ToType(Info[j].Type), ToFlags(Info[j].Flags));
                    }
                    else
                    {
                        w.WriteLine("    {0,-78}// 0x{1:X3} - Size: {2} - Type: {3} - Flags: {4}", sName + " = 0x" + Conversion.Hex(Info[j].Offset) + ",", Info[j].Offset, Info[j].Size, ToType(Info[j].Type), ToFlags(Info[j].Flags));
                    }
                }
            }

            if (!string.IsNullOrEmpty(LastFieldType))
            {
                w.WriteLine("    {0,-78}// 0x{1:X3}", LastFieldType.ToUpper() + "_END = " + BasedOnName + " + 0x" + Conversion.Hex(Info[^1].Offset + Info[^1].Size), BasedOn + Info[^1].Offset + Info[^1].Size);
            }

            w.WriteLine("}");
            w.Flush();
        }

        o.Close();
        f.Close();
    }

    public static void ExtractOpcodes()
    {
        FileStream f = new("wow.exe", FileMode.Open, FileAccess.Read, FileShare.Read, 10000000);
        BinaryReader r1 = new(f);
        StreamReader r2 = new(f);
        FileStream o = new("Global.Opcodes.cs", FileMode.Create, FileAccess.Write, FileShare.None, 1024);
        StreamWriter w = new(o);
        MessageBox.Show(ReadString(f, SearchInFile(f, "CMSG_REQUEST_PARTY_MEMBER_STATS")));
        var START = SearchInFile(f, "NUM_MSG_TYPES");
        if (START == -1)
        {
            MessageBox.Show("Wrong offsets!");
        }
        else
        {
            Stack<string> Names = new();
            var Last = "";
            f.Seek(START, SeekOrigin.Begin);
            while (Last != "MSG_NULL_ACTION")
            {
                Last = ReadString(f);
                Names.Push(Last);
            }

            MessageBox.Show(string.Format("{0} opcodes extracted.", Names.Count));
            w.WriteLine("// Auto generated file");
            w.WriteLine("// {0}", DateAndTime.Now);
            w.WriteLine();
            w.WriteLine("Public Enum OPCODES");
            w.WriteLine("{");
            var i = 0;
            while (Names.Count > 0)
            {
                w.WriteLine("    {0,-64}// 0x{1:X3}", Names.Pop() + "=" + i, i);
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
        FileStream f = new("wow.exe", FileMode.Open, FileAccess.Read, FileShare.Read, 10000000);
        BinaryReader r1 = new(f);
        StreamReader r2 = new(f);
        FileStream o = new("Global.SpellFailedReasons.cs", FileMode.Create, FileAccess.Write, FileShare.None, 1024);
        StreamWriter w = new(o);
        var REASON_NAME_OFFSET = SearchInFile(f, "SPELL_FAILED_UNKNOWN");
        if (REASON_NAME_OFFSET == -1)
        {
            MessageBox.Show("Wrong offsets!");
        }
        else
        {
            Stack<string> Names = new();
            var Last = "";
            var Offset = REASON_NAME_OFFSET;
            f.Seek(Offset, SeekOrigin.Begin);
            while (Last.Length == 0 || Last.Substring(0, 13) == "SPELL_FAILED_")
            {
                Last = ReadString(f);
                if (Last.Length > 13 && Last.Substring(0, 13) == "SPELL_FAILED_")
                {
                    Names.Push(Last);
                }
            }

            MessageBox.Show(string.Format("{0} spell failed reasons extracted.", Names.Count));
            w.WriteLine("// Auto generated file");
            w.WriteLine("// {0}", DateAndTime.Now);
            w.WriteLine();
            w.WriteLine("Public Enum SpellFailedReason");
            w.WriteLine("{");
            var i = 0;
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
        FileStream f = new("wow.exe", FileMode.Open, FileAccess.Read, FileShare.Read, 10000000);
        BinaryReader r1 = new(f);
        StreamReader r2 = new(f);
        FileStream o = new("Global.ChatTypes.cs", FileMode.Create, FileAccess.Write, FileShare.None, 1024);
        StreamWriter w = new(o);
        var START = SearchInFile(f, "CHAT_MSG_RAID_WARNING");
        if (START == -1)
        {
            MessageBox.Show("Wrong offsets!");
        }
        else
        {
            Stack<string> Names = new();
            var Last = "";
            var Offset = START;
            f.Seek(Offset, SeekOrigin.Begin);
            while (Last.Length == 0 || Last.Substring(0, 9) == "CHAT_MSG_")
            {
                Last = ReadString(f);
                if (Last.Length > 10 && Last.Substring(0, 9) == "CHAT_MSG_")
                {
                    Names.Push(Last);
                }
            }

            MessageBox.Show(string.Format("{0} chat types extracted.", Names.Count));
            w.WriteLine("// Auto generated file");
            w.WriteLine("// {0}", DateAndTime.Now);
            w.WriteLine();
            w.WriteLine("Public Enum ChatMsg");
            w.WriteLine("{");
            var i = 0;
            while (Names.Count > 0)
            {
                w.WriteLine("    {0,-64}// 0x{1:X3}", Names.Pop() + " = 0x" + Conversion.Hex(i) + ",", i);
                i += 1;
            }

            w.WriteLine("}");
            w.Flush();
        }

        o.Close();
        f.Close();
    }
}
