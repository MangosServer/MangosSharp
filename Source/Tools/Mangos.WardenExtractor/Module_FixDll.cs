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
using System.IO;

namespace Mangos.WardenExtractor;

public static class Module_FixDll
{
    public static void FixNormalDll(ref byte[] Data)
    {
        MemoryStream ms = new();
        BinaryWriter bw = new(ms);
        MemoryStream ms2 = new(Data);
        BinaryReader br = new(ms2);
        List<Section> Sections = new();
        var CurrentPosition = 0x400;
        var ImportAddress = br.ReadInt32();
        var ImportUnk = br.ReadInt32();
        var ExportAddress = br.ReadInt32();
        var ExportUnk = br.ReadInt32();
        br.BaseStream.Position = 36L;
        var numSections = br.ReadInt32();
        var BufferPosition = 0x4E;
        var i = 40;
        var j = 0;
        do
        {
            var virtualaddress = br.ReadInt32();
            var len = br.ReadInt32();
            var type = br.ReadInt32();
            string name;
            int characteristics;
            if (type == 2)
            {
                name = ".rdata";
                characteristics = 0x40000040;
            }
            // 40000000 + 40 (can be read, initialized data)
            else if (type == 4)
            {
                name = ".data";
                characteristics = unchecked((int)0xC0000040);
            }
            // 80000000 + 40000000 + 40 (Can be written to, can be read, initialized data)
            else if (type == 32)
            {
                name = ".text";
                characteristics = 0x60000020;
            }
            // 40000000 + 20000000 + 20 (can be read, can be executed as code, contains executable code)
            else
            {
                // Can't?!
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ERROR] Invalid section in dll.");
                return;
            }

            var tmpPos = br.BaseStream.Position;
            Section newSection = new()
            {
                Name = name,
                Address = virtualaddress,
                Characteristics = characteristics,
                Size = len,
                Data = GetSectionData(ref br, BufferPosition, name, virtualaddress, len, characteristics),
                Pointer = CurrentPosition
            };
            if (type == 32)
            {
                newSection.RawSize = 0x6E00;
            }
            else if (type == 4)
            {
                newSection.RawSize = 0x200;
            }
            else
            {
                newSection.RawSize = type == 2 ? 0x800 : len;
            }

            Sections.Add(newSection);
            br.BaseStream.Position = tmpPos;
            BufferPosition += len;
            CurrentPosition += newSection.RawSize;
            j += 1;
            i += 12;
        }
        while (j < numSections);
        ms2.Close();
        ms2.Dispose();
        br = null;

        // TODO: Other headers!
        Section ExportSection = new()
        {
            Name = ".edata",
            Address = ExportAddress,
            Characteristics = 0x40000040,
            // 40000000 + 40 (can be read, initialized data)
            Size = 0x8D,
            Data = Array.Empty<byte>(), // TODO!!
            Pointer = CurrentPosition,
            RawSize = 0x200
        };
        CurrentPosition += ExportSection.RawSize;
        Sections.Add(ExportSection);
        Section ImportSection = new()
        {
            Name = ".idata",
            Address = ImportAddress,
            Characteristics = 0x42000040,
            // 40000000 + 2000000 + 40 (can be read, can be discarded, initialized data)
            Size = 0x3C,
            Data = Array.Empty<byte>(), // TODO!!
            Pointer = CurrentPosition,
            RawSize = 0x200
        };
        CurrentPosition += ImportSection.RawSize;
        Sections.Add(ImportSection);
        Section RelocSection = new()
        {
            Name = ".reloc",
            Address = 0xC000,
            Characteristics = 0x42000040,
            // 40000000 + 2000000 + 40 (can be read, can be discarded, initialized data)
            Size = 0x2F4,
            Data = Array.Empty<byte>(), // TODO!!
            Pointer = CurrentPosition,
            RawSize = 0x400
        };
        CurrentPosition += RelocSection.RawSize;
        Sections.Add(RelocSection);
        var Null = new byte[1025];
        var loopTo = Null.Length - 1;
        for (i = 0; i <= loopTo; i++)
        {
            Null[i] = 0;
        }

        // Start Header
        bw.Write(23117);
        bw.Write(Null, 0, 56);
        bw.Write(64);
        // IMAGE_NT_HEADERS
        bw.Write(17744); // Signature
                         // -IMAGE_FILE_HEADER
        bw.Write((short)0x14C); // Machine
        bw.Write((short)Sections.Count); // Number of sections
        bw.Write(0); // Timestamp
        bw.Write(0); // Pointer to symbol table
        bw.Write(0); // Number of symbols
        bw.Write((short)224); // Optional header size
        bw.Write((short)0x2102); // Characteristics
                                 // -IMAGE_OPTIONAL_HEADER
        bw.Write((short)0x10B); // Magic
        bw.Write((byte)7); // MajorLinkerVersion
        bw.Write((byte)10); // MinorLinkerVersion
        bw.Write(0x6E00); // SizeOfCode
        bw.Write(0); // SizeOfInitializedData
        bw.Write(0); // SizeOfUninitializedData
        bw.Write(0x0); // AddressEntryPoint
        bw.Write(0x1000); // BaseOfCode
        bw.Write(0x8000); // BaseOfData
        bw.Write(0x400000); // ImageBase
        bw.Write(0x1000); // SectionAlignment
        bw.Write(0x200); // FileAlignment
        bw.Write((short)4); // MajorOperatingSystemVersion
        bw.Write((short)0); // MinorOperatingSystemVersion
        bw.Write((short)1); // MajorImageVersion
        bw.Write((short)0); // MinorImageVersion
        bw.Write((short)4); // MajorSubsystemVersion
        bw.Write((short)0); // MinorSubsystemVersion
        bw.Write(0); // Win32VersionValue
        bw.Write(0xD000); // SizeOfImage
        bw.Write(0x400); // SizeOfHeaders
        bw.Write(0); // CheckSum
        bw.Write((short)2); // Subsystem
        bw.Write((short)0); // DllCharacteristics
        bw.Write(0x100000); // SizeOfStackReverse
        bw.Write(0x1000); // SizeOfStackCommit
        bw.Write(0x100000); // SizeOfHeapReverse
        bw.Write(0x1000); // SizeOfHeapCommit
        bw.Write(0); // LoaderFlags
        bw.Write(0x10); // NumberOfRvaAndSizes
                        // --IMAGE_DIRECTORY_ENTRY_EXPORT
        bw.Write(0xA000); // VirtualAddress
        bw.Write(0x8D); // Size
                        // --IMAGE_DIRECTORY_ENTRY_IMPORT
        bw.Write(0xB000); // VirtualAddress
        bw.Write(0x3C); // Size
                        // --IMAGE_DIRECTORY_ENTRY_RESOURCE
        bw.Write(0); // VirtualAddress
        bw.Write(0); // Size
                     // --IMAGE_DIRECTORY_ENTRY_EXCEPTION
        bw.Write(0); // VirtualAddress
        bw.Write(0); // Size
                     // --IMAGE_DIRECTORY_ENTRY_SECURITY
        bw.Write(0); // VirtualAddress
        bw.Write(0); // Size
                     // --IMAGE_DIRECTORY_ENTRY_BASERELOC
        bw.Write(0xC000); // VirtualAddress
        bw.Write(0x2F4); // Size
                         // --IMAGE_DIRECTORY_ENTRY_DEBUG
        bw.Write(0); // VirtualAddress
        bw.Write(0); // Size
                     // --IMAGE_DIRECTORY_ENTRY_COPYRIGHT
        bw.Write(0); // VirtualAddress
        bw.Write(0); // Size
                     // --IMAGE_DIRECTORY_ENTRY_GLOBALPTR
        bw.Write(0); // VirtualAddress
        bw.Write(0); // Size
                     // --IMAGE_DIRECTORY_ENTRY_TLS
        bw.Write(0); // VirtualAddress
        bw.Write(0); // Size
                     // --IMAGE_DIRECTORY_ENTRY_LOAD_CONFIG
        bw.Write(0); // VirtualAddress
        bw.Write(0); // Size
                     // --IMAGE_DIRECTORY_ENTRY_BOUND_IMPORT
        bw.Write(0); // VirtualAddress
        bw.Write(0); // Size
                     // --IMAGE_DIRECTORY_ENTRY_IAT
        bw.Write(0x8000); // VirtualAddress
        bw.Write(0x80); // Size
                        // --IMAGE_DIRECTORY_ENTRY_DELAY_IMPORT
        bw.Write(0); // VirtualAddress
        bw.Write(0); // Size
                     // --IMAGE_DIRECTORY_ENTRY_COM_DESCRIPTION
        bw.Write(0); // VirtualAddress
        bw.Write(0); // Size
                     // --IMAGE_DIRECTORY_ENTRY_RESERVED
        bw.Write(0); // VirtualAddress
        bw.Write(0); // Size

        // IMAGE_SECTION_HEADER
        foreach (var tmpSection in Sections)
        {
            bw.Write(GetName(tmpSection.Name)); // Name (8 bytes, null-padded)
            bw.Write(tmpSection.Size); // VirtualSize
            bw.Write(tmpSection.Address); // VirtualAddress
            bw.Write(tmpSection.RawSize); // SizeOfRawData
            bw.Write(tmpSection.Pointer); // PointerToRawData
            bw.Write(0); // PointerToRelocations
            bw.Write(0); // PointerToLinenumbers
            bw.Write((short)0); // NumberOfRelocations
            bw.Write((short)0); // NumberOfLinenumbers
            bw.Write(tmpSection.Characteristics); // Characteristics
        }
        // End Header

        // Section data
        foreach (var tmpSection in Sections)
        {
            bw.BaseStream.Position = tmpSection.Pointer;
            bw.Write(tmpSection.Data, 0, tmpSection.Data.Length);
        }

        // To make sure the file gets this long
        if (bw.BaseStream.Position != CurrentPosition)
        {
            bw.BaseStream.Position = CurrentPosition - 1;
            bw.Write((byte)0);
        }

        var DllData = ms.ToArray();
        Data = DllData;
        ms.Close();
        ms.Dispose();
    }

    public static void FixWardenDll(ref byte[] Data)
    {
    }

    public static ulong GetName(string Name)
    {
        var bBytes = new byte[8];
        for (var i = 0; i <= 7; i++)
        {
            if (i + 1 > Name.Length)
            {
                break;
            }

            bBytes[i] = (byte)Strings.Asc(Name[i]);
        }

        return BitConverter.ToUInt64(bBytes, 0);
    }

    public static byte[] GetSectionData(ref BinaryReader br, int Position, string Name, int Address, int Length, int characteristics)
    {
        br.BaseStream.Position = Position;
        var tmpBytes = br.ReadBytes(Length);
        if (Conversions.ToBoolean(characteristics & 0x20))
        {
            var InstructionPos = 0;
            do
            {
                try
                {
                    var InstrPos = InstructionPos;
                    var Instr = ParseInstuction(ref tmpBytes, ref InstructionPos);
                    if (Instr is null)
                    {
                        continue;
                    }

                    if (Instr.Opcode == 0xA1)
                    {
                        Instr.DisplacementData[2] = 0x40;
                        Instr.Place(ref tmpBytes, InstrPos);
                    }
                    else if (Instr.Opcode == 0xC6 && Instr.ModRmData == 0x5)
                    {
                        Instr.DisplacementData[2] = 0x40;
                        Instr.Place(ref tmpBytes, InstrPos);
                    }
                    else if (Instr.Opcode == 0x80)
                    {
                        Instr.DisplacementData[2] = 0x40;
                        Instr.Place(ref tmpBytes, InstrPos);
                    }
                    else if (Instr.Opcode == 0xFF && (Instr.ModRmData == 0x15 || Instr.ModRmData == 0x25))
                    {
                        Instr.DisplacementData[2] = 0x40;
                        Instr.Place(ref tmpBytes, InstrPos);
                    }
                    else if (Instr.Opcode == 0x8B && Instr.ModRmData == 0xD)
                    {
                        Instr.DisplacementData[2] = 0x40;
                        Instr.Place(ref tmpBytes, InstrPos);
                    }
                    else if (Instr.Opcode == 0xC7 && Instr.ModRmData == 0x0)
                    {
                        Instr.ImmediateData[2] = 0x40;
                        Instr.Place(ref tmpBytes, InstrPos);
                    }
                    else if (Instr.Opcode == 0xC7 && Instr.ModRmData == 0x5)
                    {
                        Instr.DisplacementData[2] = 0x40;
                        if (Instr.ImmediateData[0] != 0xFF)
                        {
                            var tmpBytes2 = tmpBytes;
                            Array.Resize(ref tmpBytes, tmpBytes.Length + 1 + 1);
                            Array.Copy(tmpBytes2, InstructionPos - 2, tmpBytes, InstructionPos, tmpBytes2.Length - (InstructionPos - 2));
                            Instr.ImmediateData[0] = 0;
                            Instr.ImmediateData[1] = 0;
                            Instr.ImmediateData[2] = 0;
                            Instr.ImmediateData[3] = 0;
                        }

                        Instr.Place(ref tmpBytes, InstrPos);
                    }
                    else if (InstrPos >= 0xB60 - 0x400)
                    {
                        // MsgBox(Hex(Instr.Opcode) & " : " & Instr.ToString)
                    }
                }
                catch
                {
                    break;
                }
            }
            while (InstructionPos < tmpBytes.Length);
        }

        return tmpBytes;
    }

    public class Section
    {
        public string Name;
        public int Address;
        public int Characteristics;
        public int Size;
        public byte[] Data;
        public int Pointer;
        public int RawSize;
    }

    private static Instruction ParseInstuction(ref byte[] Data, ref int Position)
    {
        Instruction newInstruction = new();
        if (Data[Position] == 0)
        {
            Position += 1;
            return null;
        }

        var StartAt = Position;
        for (var i = 0; i <= 3; i++)
        {
            if (!IsPrefix(Data[Position]))
            {
                break;
            }

            newInstruction.Prefix[i] = Data[Position];
            Position += 1;
        }

        newInstruction.InitPrefix();
        var bOpcode = Data[Position];
        Position += 1;
        if (bOpcode == 0xF)
        {
            newInstruction.Opcode = Data[Position];
            newInstruction.Two_Bytes = true;
            Position += 1;
        }
        else
        {
            newInstruction.Opcode = bOpcode;
        }

        newInstruction.ExtendedOpcode = IsExtendedOpcode(ref newInstruction);
        newInstruction.ModrefReq = IsModrefRequired(ref newInstruction);
        newInstruction.DisplacementSize = FindDisplacementDataSize(ref newInstruction);
        newInstruction.ImmediateSize = FindImmediateDataSize(ref newInstruction);
        if (newInstruction.ModrefReq)
        {
            newInstruction.ModRmData = Data[Position];
            if ((newInstruction.ModRmData & MOD_FIELD_MASK) != MOD_FIELD_NO_SIB && (newInstruction.ModRmData & RM_FIELD_MASK) == RM_FIELD_SIB)
            {
                Position += 1;
                newInstruction.SibData = Data[Position];
                newInstruction.SibAccompanies = true;
            }

            newInstruction.DisplacementSize = FindDisplacementDataSize2(ref newInstruction);
            newInstruction.ImmediateSize = FindImmediateDataSize(ref newInstruction);
            Position += 1;
        }

        if (newInstruction.DisplacementSize > 0)
        {
            for (int i = 0, loopTo = newInstruction.DisplacementSize - 1; i <= loopTo; i++)
            {
                newInstruction.DisplacementData[i] = Data[Position + i];
            }

            Position += newInstruction.DisplacementSize;
        }

        if (newInstruction.ImmediateSize > 0)
        {
            for (int i = 0, loopTo1 = newInstruction.ImmediateSize - 1; i <= loopTo1; i++)
            {
                newInstruction.ImmediateData[i] = Data[Position + i];
            }

            Position += newInstruction.ImmediateSize;
        }

        newInstruction.OrigSize = Position - StartAt;
        return newInstruction;
    }

    private static bool IsPrefix(byte bByte)
    {
        switch (bByte)
        {
            case 0xF3:
            case 0xF2:
            case 0xF0:
            case 0x2E:
            case 0x36:
            case 0x3E:
            case 0x26:
            case 0x64:
            case 0x65:
            case 0x66:
            case 0x67:
                {
                    return true;
                }

            default:
                {
                    return false;
                }
        }
    }

    private static int IsExtendedOpcode(ref Instruction Instr)
    {
        if (!Instr.Two_Bytes)
        {
            switch (Instr.Opcode)
            {
                case 0x80:
                case 0x81:
                case 0x82:
                case 0x83:
                    {
                        return 1;
                    }

                case 0xC0:
                case 0xC1:
                case 0xD0:
                case 0xD1:
                case 0xD2:
                case 0xD3:
                    {
                        return 2;
                    }

                case 0xF6:
                case 0xF7:
                    {
                        return 3;
                    }

                case 0xFE:
                    {
                        return 4;
                    }

                case 0xFF:
                    {
                        return 5;
                    }

                default:
                    {
                        return 0;
                    }
            }
        }

        switch (Instr.Opcode)
        {
            case 0x0:
                {
                    return 6;
                }

            case 0x1:
                {
                    return 7;
                }

            case 0xC7:
                {
                    return 9;
                }

            case 0x71:
            case 0x72:
            case 0x73:
                {
                    return 0xA;
                }

            default:
                {
                    return 0;
                }
        }
    }

    private static bool IsModrefRequired(ref Instruction Instr)
    {
        return Instr.Two_Bytes
            ? IsBitSetInTable(Instr.Opcode, TWO_BYTE_OPCODE_MODREF_REQUIREMENT)
            : IsBitSetInTable(Instr.Opcode, ONE_BYTE_OPCODE_MODREF_REQUIREMENT);
    }

    private static int FindDisplacementDataSize(ref Instruction Instr)
    {
        var address_size_is_32 = false;
        if (Instr.EffectiveAddressSize == 32)
        {
            address_size_is_32 = true;
        }

        if (Instr.Opcode is 0x9A or 0xEA)
        {
            Instr.FullDisplacement = true;
            return address_size_is_32 ? 6 : 4;
        }

        if (!Instr.Two_Bytes)
        {
            if (IsBitSetInTable(Instr.Opcode, ONE_BYTE_OPCODE_DISPLACEMENT_SIZE_BYTE))
            {
                return 1;
            }

            if (IsBitSetInTable(Instr.Opcode, ONE_BYTE_OPCODE_DISPLACEMENT_SIZE_VARIABLE))
            {
                return address_size_is_32 ? 4 : 2;
            }
        }
        else
        {
            if (IsBitSetInTable(Instr.Opcode, TWO_BYTE_OPCODE_DISPLACEMENT_SIZE_BYTE))
            {
                return 1;
            }

            if (IsBitSetInTable(Instr.Opcode, TWO_BYTE_OPCODE_DISPLACEMENT_SIZE_VARIABLE))
            {
                return address_size_is_32 ? 4 : 2;
            }
        }

        return 0;
    }

    private static int FindImmediateDataSize(ref Instruction Instr)
    {
        var operand_size_32 = false;
        if (Instr.EffectiveOperandSize == 32)
        {
            operand_size_32 = true;
        }

        if (Instr.Opcode is 0xC2 or 0xCA)
        {
            return 2;
        }

        if (Instr.Opcode == 0xC8)
        {
            return 3;
        }

        if (!Instr.Two_Bytes)
        {
            if (IsBitSetInTable(Instr.Opcode, ONE_BYTE_OPCODE_IMMEDIATE_SIZE_BYTE))
            {
                return 1;
            }

            if (IsBitSetInTable(Instr.Opcode, ONE_BYTE_OPCODE_IMMEDIATE_SIZE_VARIABLE))
            {
                return operand_size_32 ? 4 : 2;
            }
        }
        else
        {
            if (IsBitSetInTable(Instr.Opcode, TWO_BYTE_OPCODE_IMMEDIATE_SIZE_BYTE))
            {
                return 1;
            }

            if (IsBitSetInTable(Instr.Opcode, TWO_BYTE_OPCODE_IMMEDIATE_SIZE_VARIABLE))
            {
                return operand_size_32 ? 4 : 2;
            }
        }

        return 0;
    }

    private static int FindDisplacementDataSize2(ref Instruction Instr)
    {
        var address_size_is_32 = false;
        var DisplacementSize = 0;
        if (Instr.EffectiveOperandSize == 32)
        {
            address_size_is_32 = true;
        }

        switch (Instr.ModRmData & MOD_FIELD_MASK)
        {
            case MOD_FIELD_00:
                {
                    if (address_size_is_32)
                    {
                        if ((Instr.ModRmData & RM_FIELD_MASK) == 0x5)
                        {
                            DisplacementSize = 4;
                        }
                    }
                    else if ((Instr.ModRmData & RM_FIELD_MASK) == 0x6)
                    {
                        DisplacementSize = 2;
                    }

                    break;
                }

            case MOD_FIELD_01:
                {
                    DisplacementSize = 1;
                    break;
                }

            case MOD_FIELD_10:
                {
                    DisplacementSize = address_size_is_32 ? 4 : 2;
                    break;
                }

            case MOD_FIELD_11:
                {
                    DisplacementSize = 0;
                    break;
                }
        }

        return DisplacementSize;
    }

    private static int FindImmediateDataSize2(ref Instruction Instr)
    {
        var operand_size_32 = false;
        var immediate_size = 0;
        if (Instr.EffectiveOperandSize == 32)
        {
            operand_size_32 = true;
        }

        switch (Instr.ExtendedOpcode)
        {
            case 1:
                {
                    immediate_size = Instr.Opcode == 0x81 ? operand_size_32 ? 4 : 2 : 1;

                    break;
                }

            case 2:
                {
                    if (Instr.Opcode is 0xC0 or 0xC1)
                    {
                        immediate_size = 1;
                    }

                    break;
                }

            case 3:
                {
                    if ((Instr.ModRmData & OPCODE_FIELD_MASK) == 0)
                    {
                        if (Instr.Opcode == 0xF6)
                        {
                            immediate_size = 1;
                        }
                        else if (Instr.Opcode == 0xF6)
                        {
                            immediate_size = operand_size_32 ? 4 : 2;
                        }
                        else
                        {
                            return 0;
                        }
                    }

                    break;
                }

            case 8:
                {
                    immediate_size = 1;
                    break;
                }

            case 0xA:
                {
                    immediate_size = 1;
                    break;
                }

            default:
                {
                    immediate_size = 0;
                    break;
                }
        }

        return immediate_size;
    }

    private static bool IsBitSetInTable(byte bOpcode, ushort[] Table)
    {
        var row_index = (byte)(0xF0 & bOpcode);
        row_index = (byte)(row_index >> 4);
        var col_index = (byte)(0xF & bOpcode);
        var databits = Table[row_index];
        uint the_bit;
        switch (col_index)
        {
            case 0x0:
                {
                    the_bit = (uint)(databits & 0x8000);
                    break;
                }

            case 0x1:
                {
                    the_bit = (uint)(databits & 0x4000);
                    break;
                }

            case 0x2:
                {
                    the_bit = (uint)(databits & 0x2000);
                    break;
                }

            case 0x3:
                {
                    the_bit = (uint)(databits & 0x1000);
                    break;
                }

            case 0x4:
                {
                    the_bit = (uint)(databits & 0x800);
                    break;
                }

            case 0x5:
                {
                    the_bit = (uint)(databits & 0x400);
                    break;
                }

            case 0x6:
                {
                    the_bit = (uint)(databits & 0x200);
                    break;
                }

            case 0x7:
                {
                    the_bit = (uint)(databits & 0x100);
                    break;
                }

            case 0x8:
                {
                    the_bit = (uint)(databits & 0x80);
                    break;
                }

            case 0x9:
                {
                    the_bit = (uint)(databits & 0x40);
                    break;
                }

            case 0xA:
                {
                    the_bit = (uint)(databits & 0x20);
                    break;
                }

            case 0xB:
                {
                    the_bit = (uint)(databits & 0x10);
                    break;
                }

            case 0xC:
                {
                    the_bit = (uint)(databits & 0x8);
                    break;
                }

            case 0xD:
                {
                    the_bit = (uint)(databits & 0x4);
                    break;
                }

            case 0xE:
                {
                    the_bit = (uint)(databits & 0x2);
                    break;
                }

            case 0xF:
                {
                    the_bit = (uint)(databits & 0x1);
                    break;
                }

            default:
                {
                    return false;
                }
        }

        return the_bit > 0L;
    }

    public class Instruction
    {
        public byte[] Prefix = { 0, 0, 0, 0 };
        public byte Opcode;
        public bool Two_Bytes;
        public int ExtendedOpcode;
        public bool ModrefReq;
        public byte ModRmData;
        public byte SibData;
        public bool SibAccompanies;
        public bool AddressSizeOverwritten;
        public bool OperandSizeOverwritten;
        public bool FullDisplacement;
        public int DisplacementSize;
        public int ImmediateSize;
        public byte[] DisplacementData = { 0, 0, 0, 0, 0, 0 };
        public byte[] ImmediateData = { 0, 0, 0, 0 };
        public int OrigSize;

        public void InitPrefix()
        {
            for (var i = 0; i <= 3; i++)
            {
                if (IsPrefix(Prefix[i]) == false)
                {
                    break;
                }

                if (Prefix[i] == 0x67)
                {
                    AddressSizeOverwritten = true;
                }
                else if (Prefix[i] == 0x66)
                {
                    OperandSizeOverwritten = true;
                }
            }
        }

        public int EffectiveAddressSize => AddressSizeOverwritten ? 16 : 32;

        public int EffectiveOperandSize => OperandSizeOverwritten ? 16 : 32;

        public byte[] GetBytes()
        {
            MemoryStream newData = new();
            for (var i = 0; i <= 3; i++)
            {
                if (IsPrefix(Prefix[i]) == false)
                {
                    break;
                }

                newData.WriteByte(Prefix[i]);
            }

            if (Two_Bytes)
            {
                newData.WriteByte(0xF);
            }

            newData.WriteByte(Opcode);
            if (ModrefReq)
            {
                newData.WriteByte(ModRmData);
                if ((ModRmData & MOD_FIELD_MASK) != MOD_FIELD_NO_SIB && (ModRmData & RM_FIELD_MASK) == RM_FIELD_SIB)
                {
                    newData.WriteByte(SibData);
                }
            }

            if (DisplacementSize > 0)
            {
                for (int i = 0, loopTo = DisplacementSize - 1; i <= loopTo; i++)
                {
                    newData.WriteByte(DisplacementData[i]);
                }
            }

            if (ImmediateSize > 0)
            {
                for (int i = 0, loopTo1 = ImmediateSize - 1; i <= loopTo1; i++)
                {
                    newData.WriteByte(ImmediateData[i]);
                }
            }

            return newData.ToArray();
        }

        public override string ToString()
        {
            return BitConverter.ToString(GetBytes()).Replace("-", " ");
        }

        public void Place(ref byte[] Data, int Index)
        {
            var tmpBytes = GetBytes();
            if (tmpBytes.Length != OrigSize)
            {
                var tmpBytes2 = Data;
                if (tmpBytes.Length > OrigSize)
                {
                    Array.Resize(ref Data, Data.Length + (tmpBytes.Length - OrigSize));
                }

                Array.Copy(tmpBytes2, Index + OrigSize, Data, Index + tmpBytes.Length, tmpBytes2.Length - (Index + OrigSize));
                if (tmpBytes.Length < OrigSize)
                {
                    Array.Resize(ref Data, Data.Length - (OrigSize - tmpBytes.Length));
                }
            }

            Array.Copy(tmpBytes, 0, Data, Index, tmpBytes.Length);
        }
    }

    public static ushort[] ONE_BYTE_OPCODE_MODREF_REQUIREMENT = { 0xF0F0, 0xF0F0, 0xF0F0, 0xF0F0, 0x0, 0x0, 0x3050, 0x0, 0xFFFF, 0x0, 0x0, 0x0, 0xCF00, 0xF000, 0x0, 0x303 };
    public static ushort[] TWO_BYTE_OPCODE_MODREF_REQUIREMENT = { 0xB000, 0x0, 0xF000, 0xF000, 0xFFFF, 0x0, 0xFFF3, 0x7E03, 0x0, 0xFFFF, 0x1C1D, 0xFF3F, 0xC100, 0x74DD, 0x64DD, 0x74EE };
    public static ushort[] ONE_BYTE_OPCODE_DISPLACEMENT_SIZE_BYTE = { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0xFFFF, 0x0, 0x0, 0xA000, 0x0, 0x0, 0x0, 0xF010, 0x0 };
    public static ushort[] ONE_BYTE_OPCODE_DISPLACEMENT_SIZE_VARIABLE = { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x5000, 0x0, 0x0, 0x0, 0xC0, 0x0 };
    public static ushort[] TWO_BYTE_OPCODE_DISPLACEMENT_SIZE_BYTE = { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };
    public static ushort[] TWO_BYTE_OPCODE_DISPLACEMENT_SIZE_VARIABLE = { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0xFFFF, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };
    public static ushort[] ONE_BYTE_OPCODE_IMMEDIATE_SIZE_BYTE = { 0x808, 0x808, 0x808, 0x808, 0x0, 0x0, 0x30, 0x0, 0x0, 0x0, 0x80, 0xFF00, 0x200, 0x0, 0xF00, 0x0 };
    public static ushort[] ONE_BYTE_OPCODE_IMMEDIATE_SIZE_VARIABLE = { 0x404, 0x404, 0x404, 0x404, 0x0, 0x0, 0xC0, 0x0, 0x0, 0x0, 0x40, 0xFF, 0x100, 0x0, 0x0, 0x0 };
    public static ushort[] TWO_BYTE_OPCODE_IMMEDIATE_SIZE_BYTE = { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x808, 0x0, 0x0, 0x0, 0x0, 0x0 };
    public static ushort[] TWO_BYTE_OPCODE_IMMEDIATE_SIZE_VARIABLE = { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };
    public const byte MOD_FIELD_NO_SIB = 0xC0;
    public const byte MOD_FIELD_MASK = 0xC0;
    public const byte RM_FIELD_SIB = 0x4;
    public const byte RM_FIELD_MASK = 0x7;
    public const byte MOD_FIELD_00 = 0x0;
    public const byte MOD_FIELD_01 = 0x40;
    public const byte MOD_FIELD_10 = 0x80;
    public const byte MOD_FIELD_11 = 0xC0;
    public const byte OPCODE_FIELD_MASK = 0x38;
}
