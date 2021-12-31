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
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Mangos.WoWFakeClient;

public static class WS_WardenClient
{

    public enum MaievResponse : byte
    {
        MAIEV_RESPONSE_FAILED_OR_MISSING = 0x0,          // The module was either currupt or not in the cache request transfer
        MAIEV_RESPONSE_SUCCESS = 0x1,                    // The module was in the cache and loaded successfully
        MAIEV_RESPONSE_RESULT = 0x2,
        MAIEV_RESPONSE_HASH = 0x4
    }

    public enum MaievOpcode : byte
    {
        MAIEV_MODULE_INFORMATION = 0,
        MAIEV_MODULE_TRANSFER = 1,
        MAIEV_MODULE_RUN = 2,
        MAIEV_MODULE_UNK = 3,
        MAIEV_MODULE_SEED = 5
    }

    public static void InitWarden()
    {
        MaievData m = new(Realmserver.SS_Hash);
        var seedOut = m.GetBytes(16);
        var seedIn = m.GetBytes(16);
        Maiev.KeyOut = RC4.Init(seedOut);
        Maiev.KeyIn = RC4.Init(seedIn);
    }

    public class MaievData
    {
        public int index;
        public byte[] source1;
        public byte[] source2;
        public byte[] data = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        public MaievData(byte[] seed)
        {
            // Initialization
            SHA1Managed sha1 = new();
            source1 = sha1.ComputeHash(seed, 0, 20);
            source2 = sha1.ComputeHash(seed, 20, 20);
            Update();
        }

        public void Update()
        {
            var buffer1 = new byte[60];
            SHA1Managed sha1 = new();
            Buffer.BlockCopy(source1, 0, buffer1, 0, 20);
            Buffer.BlockCopy(data, 0, buffer1, 20, 20);
            Buffer.BlockCopy(source2, 0, buffer1, 40, 20);
            data = sha1.ComputeHash(buffer1);
        }

        private byte GetByte()
        {
            var r = data[index];
            index += 1;
            if (index >= 0x14)
            {
                Update();
                index = 0;
            }

            return r;
        }

        public byte[] GetBytes(int count)
        {
            var b = new byte[count];
            for (int i = 0, loopTo = count - 1; i <= loopTo; i++)
            {
                b[i] = GetByte();
            }

            return b;
        }
    }

    public static int ModuleLength;

    public static void On_SMSG_WARDEN_DATA(ref Packets.PacketClass Packet)
    {
        // START Warden Decryption
        var b = new byte[(Packet.Data.Length - 4)];
        Buffer.BlockCopy(Packet.Data, 4, b, 0, b.Length);
        RC4.Crypt(ref b, Maiev.KeyIn);
        Buffer.BlockCopy(b, 0, Packet.Data, 4, b.Length);
        // END

        var WardenData = new byte[(Packet.Data.Length - 4)];
        Buffer.BlockCopy(Packet.Data, 4, WardenData, 0, WardenData.Length);
        MaievOpcode Opcode = (MaievOpcode)Packet.GetInt8();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("SMSG_WARDEN_DATA [{0}]", Opcode);
        Console.ForegroundColor = ConsoleColor.White;
        switch (Opcode)
        {
            case MaievOpcode.MAIEV_MODULE_INFORMATION:
                {
                    var Name = Packet.GetByteArray(16);
                    var Key = Packet.GetByteArray(16);
                    var Size = Packet.GetUInt32();
                    Maiev.ModuleName = BitConverter.ToString(Name).Replace("-", "");
                    Maiev.ModuleKey = Key;
                    ModuleLength = (int)Size;
                    Maiev.ModuleData = Array.Empty<byte>();
                    if (File.Exists(@"modules\" + Maiev.ModuleName + ".mod") == false)
                    {
                        Console.WriteLine("[{0}][WARDEN] Module is missing.", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"));
                        Packets.PacketClass response = new(OPCODES.CMSG_WARDEN_DATA);
                        response.AddInt8((byte)MaievResponse.MAIEV_RESPONSE_FAILED_OR_MISSING);
                        SendWardenPacket(ref response);
                        response.Dispose();
                    }
                    else
                    {
                        Console.WriteLine("[{0}][WARDEN] Module is initiated.", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"));
                        Maiev.ModuleData = File.ReadAllBytes(@"modules\" + Maiev.ModuleName + ".mod");
                        if (Maiev.LoadModule(Maiev.ModuleName, ref Maiev.ModuleData, Maiev.ModuleKey))
                        {
                            Console.WriteLine("[{0}][WARDEN] Successfully loaded the module.", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"));
                            Packets.PacketClass response = new(OPCODES.CMSG_WARDEN_DATA);
                            response.AddInt8((byte)MaievResponse.MAIEV_RESPONSE_SUCCESS);
                            SendWardenPacket(ref response);
                            response.Dispose();
                        }
                        else
                        {
                            Packets.PacketClass response = new(OPCODES.CMSG_WARDEN_DATA);
                            response.AddInt8((byte)MaievResponse.MAIEV_RESPONSE_FAILED_OR_MISSING);
                            SendWardenPacket(ref response);
                            response.Dispose();
                        }
                    }

                    break;
                }

            case MaievOpcode.MAIEV_MODULE_TRANSFER:
                {
                    var Size = Packet.GetUInt16();
                    var Data = Packet.GetByteArray(Size);
                    Maiev.ModuleData = Realmserver.Concat(Maiev.ModuleData, Data);
                    ModuleLength -= Size;
                    if (ModuleLength <= 0)
                    {
                        Console.WriteLine("[{0}][WARDEN] Module is fully transfered.", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"));
                        if (Directory.Exists("modules") == false)
                        {
                            Directory.CreateDirectory("modules");
                        }

                        File.WriteAllBytes(@"modules\" + Maiev.ModuleName + ".mod", Maiev.ModuleData);
                        if (Maiev.LoadModule(Maiev.ModuleName, ref Maiev.ModuleData, Maiev.ModuleKey))
                        {
                            Console.WriteLine("[{0}][WARDEN] Successfully loaded the module.", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"));
                            Packets.PacketClass response = new(OPCODES.CMSG_WARDEN_DATA);
                            response.AddInt8((byte)MaievResponse.MAIEV_RESPONSE_SUCCESS);
                            SendWardenPacket(ref response);
                            response.Dispose();
                        }
                    }
                    else
                    {
                        Console.WriteLine("[{0}][WARDEN] Module transfer. Bytes left: {1}", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"), ModuleLength);
                    }

                    break;
                }

            case MaievOpcode.MAIEV_MODULE_RUN:
                {
                    Console.WriteLine("[{0}][WARDEN] Requesting a scan.", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"));

                    // TODO: Encrypt?
                    Maiev.ReadKeys2();
                    RC4.Crypt(ref WardenData, Maiev.ModKeyIn);
                    var HandledBytes = Maiev.HandlePacket(WardenData);
                    if (HandledBytes <= 0)
                    {
                        return;
                    }

                    var thePacket = Maiev.ReadPacket();
                    if (thePacket.Length == 0)
                    {
                        return;
                    }

                    RC4.Crypt(ref WardenData, Maiev.ModKeyOut);

                    // TODO: Decrypt?

                    Packets.DumpPacket(thePacket);
                    Packets.PacketClass response = new(OPCODES.CMSG_WARDEN_DATA);
                    response.AddByteArray(thePacket);
                    SendWardenPacket(ref response);
                    response.Dispose();
                    break;
                }

            case MaievOpcode.MAIEV_MODULE_UNK:
                {
                    // TODO: Encrypt?
                    Maiev.ReadKeys2();
                    RC4.Crypt(ref WardenData, Maiev.ModKeyIn);
                    var HandledBytes = Maiev.HandlePacket(WardenData);
                    if (HandledBytes <= 0)
                    {
                        return;
                    }

                    var thePacket = Maiev.ReadPacket();
                    if (thePacket.Length == 0)
                    {
                        return;
                    }

                    RC4.Crypt(ref WardenData, Maiev.ModKeyOut);
                    // TODO: Decrypt?

                    Packets.DumpPacket(thePacket);
                    Packets.PacketClass response = new(OPCODES.CMSG_WARDEN_DATA);
                    response.AddByteArray(thePacket);
                    SendWardenPacket(ref response);
                    response.Dispose();
                    break;
                }

            case MaievOpcode.MAIEV_MODULE_SEED:
                {
                    Maiev.GenerateNewRC4Keys(Realmserver.SS_Hash);
                    var HandledBytes = Maiev.HandlePacket(WardenData);
                    if (HandledBytes <= 0)
                    {
                        return;
                    }

                    var thePacket = Maiev.ReadPacket();
                    Maiev.ModKeyIn = new byte[258];
                    Maiev.ModKeyOut = new byte[258];
                    Packets.PacketClass response = new(OPCODES.CMSG_WARDEN_DATA);
                    response.AddByteArray(thePacket);
                    SendWardenPacket(ref response);
                    response.Dispose();
                    Maiev.ReadKeys();
                    break;
                }

            default:
                {
                    Console.WriteLine("[{0}][WARDEN] Unhandled Opcode [{1}] 0x{2:X}", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"), Opcode, Conversions.ToInteger(Opcode));
                    break;
                }
        }
    }

    public static void SendWardenPacket(ref Packets.PacketClass Packet)
    {
        // START Warden Encryption
        var b = new byte[(Packet.Data.Length - 6)];
        Buffer.BlockCopy(Packet.Data, 6, b, 0, b.Length);
        RC4.Crypt(ref b, Maiev.KeyOut);
        Buffer.BlockCopy(b, 0, Packet.Data, 6, b.Length);
        // END

        Worldserver.Send(Packet);
    }

    public static WardenMaiev Maiev = new();

    public class WardenMaiev
    {
        public byte[] WardenModule = Array.Empty<byte>();
        public string ModuleName = "";
        public byte[] ModuleKey = Array.Empty<byte>();
        public byte[] ModuleData = Array.Empty<byte>();
        public byte[] KeyIn = Array.Empty<byte>();
        public byte[] KeyOut = Array.Empty<byte>();
        public byte[] ModKeyIn = Array.Empty<byte>();
        public byte[] ModKeyOut = Array.Empty<byte>();

        public bool LoadModule(string Name, ref byte[] Data, byte[] Key)
        {
            Key = RC4.Init(Key);
            RC4.Crypt(ref Data, Key);
            var UncompressedLen = BitConverter.ToInt32(Data, 0);
            if (UncompressedLen < 0)
            {
                Console.WriteLine("[WARDEN] Failed to decrypt {0}, incorrect length.", Name);
                return false;
            }

            var CompressedData = new byte[(Data.Length - 0x108)];
            Array.Copy(Data, 4, CompressedData, 0, CompressedData.Length);
            var dataPos = 4 + CompressedData.Length;
            var Sign = Conversions.ToString((char)Data[dataPos + 3]) + (char)Data[dataPos + 2] + (char)Data[dataPos + 1] + (char)Data[dataPos];
            if (Sign != "SIGN")
            {
                Console.WriteLine("[WARDEN] Failed to decrypt {0}, sign missing.", Name);
                return false;
            }

            dataPos += 4;
            var Signature = new byte[256];
            Array.Copy(Data, dataPos, Signature, 0, Signature.Length);

            // Check signature
            if (CheckSignature(Signature, Data, Data.Length - 0x104) == false)
            {
                Console.WriteLine("[WARDEN] Signature fail on Warden Module.");
                return false;
            }

            var DecompressedData = new ZipService().DeCompress(CompressedData);
            MemoryStream ms = new(DecompressedData);
            BinaryReader br = new(ms);
            ModuleData = PrepairModule(ref br);
            ms.Close();
            ms.Dispose();
            br = null;
            Console.WriteLine("[WARDEN] Successfully prepaired Warden Module.");
            return InitModule(ref ModuleData);
        }

        public bool CheckSignature(byte[] Signature, byte[] Data, int DataLen)
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

        [DllImport("kernel32.dll", EntryPoint = "LoadLibraryA", CharSet = CharSet.Unicode)]
        private static extern int LoadLibrary(string lpLibFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetProcAddress(int hModule, string lpProcName);

        [DllImport("kernel32.dll")]
        private static extern int CloseHandle(int hObject);

        private byte[] PrepairModule(ref BinaryReader br)
        {
            var length = br.ReadInt32();
            m_Mod = malloc(length);
            var bModule = new byte[length];
            MemoryStream ms = new(bModule);
            BinaryWriter bw = new(ms);
            BinaryReader br2 = new(ms);

            // Console.WriteLine("Allocated {0} (0x{1:X}) bytes for new module.", length, length)

            // Copy 40 bytes from the original module to the new one.
            br.BaseStream.Position = 0L;
            var tmpBytes = br.ReadBytes(40);
            bw.Write(tmpBytes, 0, tmpBytes.Length);
            br2.BaseStream.Position = 0x24L;
            var source_location = 0x28 + (br2.ReadInt32() * 12);
            br.BaseStream.Position = 0x28L;
            var destination_location = br.ReadInt32();
            br.BaseStream.Position = 0x0L;
            var limit = br.ReadInt32();
            var skip = false;

            // Console.WriteLine("Copying code sections to module.")
            while (destination_location < limit)
            {
                br.BaseStream.Position = source_location;
                int count = br.ReadInt16(); // (CInt(br.ReadByte()) << 0) Or (CInt(br.ReadByte()) << 8)
                source_location += 2;
                if (!skip)
                {
                    br.BaseStream.Position = source_location;
                    tmpBytes = br.ReadBytes(count);
                    bw.BaseStream.Position = destination_location;
                    bw.Write(tmpBytes, 0, tmpBytes.Length);
                    source_location += count;
                }

                skip = !skip;
                destination_location += count;
            }

            // Console.WriteLine("Adjusting references to global variables...")
            br.BaseStream.Position = 8L;
            source_location = br.ReadInt32();
            destination_location = 0;
            var counter = 0;
            br2.BaseStream.Position = 0xCL;
            var CountTo = br2.ReadInt32();
            while (counter < CountTo)
            {
                br2.BaseStream.Position = source_location;
                var tmpByte1 = br2.ReadByte();
                var tmpByte2 = br2.ReadByte();
                destination_location += tmpByte2 | (tmpByte1 << 8);
                source_location += 2;
                br2.BaseStream.Position = destination_location;
                var address = br2.ReadInt32() + m_Mod;
                bw.BaseStream.Position = destination_location;
                bw.Write(address);
                counter += 1;
            }

            // Console.WriteLine("Updating API library references...")
            br2.BaseStream.Position = 0x20L;
            limit = br2.ReadInt32();
            string library;
            var loopTo = limit - 1;
            for (counter = 0; counter <= loopTo; counter++)
            {
                br2.BaseStream.Position = 0x1CL;
                var proc_start = br2.ReadInt32() + (counter * 8);
                br2.BaseStream.Position = proc_start;
                library = getNTString(ref br2, br2.ReadInt32());
                // Console.WriteLine("  Library: {0}", library)

                var hModule = LoadLibrary(library);
                br2.BaseStream.Position = proc_start + 4;
                var proc_offset = br2.ReadInt32();
                br2.BaseStream.Position = proc_offset;
                var proc = br2.ReadInt32();
                while (proc != 0)
                {
                    if (proc > 0)
                    {
                        var strProc = getNTString(ref br2, proc);
                        var addr = GetProcAddress(hModule, strProc);

                        // Console.WriteLine("    Function: {0} (0x{1:X})", strProc, addr)
                        bw.BaseStream.Position = proc_offset;
                        bw.Write(addr);
                    }

                    proc_offset += 4;
                    br2.BaseStream.Position = proc_offset;
                    proc = br2.ReadInt32();
                }

                CloseHandle(hModule);
            }

            return ms.ToArray();
        }

        private string getNTString(ref BinaryReader br, int pos)
        {
            br.BaseStream.Position = pos;
            var i = 0;
            byte tmpByte;
            var tmpStr = "";
            do
            {
                tmpByte = br.ReadByte();
                if (tmpByte == 0)
                {
                    return tmpStr;
                }

                tmpStr += Conversions.ToString((char)tmpByte);
                i += 1;
            }
            while (true);
        }

        public delegate void SendPacketDelegate(int ptrPacket, int dwSize);

        public delegate int CheckModuleDelegate(int ptrMod, int ptrKey);

        public delegate int ModuleLoadDelegate(int ptrRC4Key, int pModule, int dwModSize);

        public delegate int AllocateMemDelegate(int dwSize);

        public delegate void FreeMemoryDelegate(int dwMemory);

        public delegate int SetRC4DataDelegate(int lpKeys, int dwSize);

        public delegate int GetRC4DataDelegate(int lpBuffer, ref int dwSize);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        public delegate int InitializeModule(int lpPtr2Table);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        public delegate void GenerateRC4KeysDelegate(int ppFncList, int lpData, int dwSize);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        public delegate void UnloadModuleDelegate(int ppFncList);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        public delegate void PacketHandlerDelegate(int ppFncList, int pPacket, int dwSize, int dwBuffer);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        public delegate void TickDelegate(int ppFncList, int dwTick);

        private SendPacketDelegate SendPacketD;
        private CheckModuleDelegate CheckModuleD;
        private ModuleLoadDelegate ModuleLoadD;
        private AllocateMemDelegate AllocateMemD;
        private FreeMemoryDelegate FreeMemoryD;
        private SetRC4DataDelegate SetRC4DataD;
        private GetRC4DataDelegate GetRC4DataD;
        private GenerateRC4KeysDelegate GenerateRC4Keys;
        private UnloadModuleDelegate UnloadModule;
        private PacketHandlerDelegate PacketHandler;
        private TickDelegate Tick;
        private int m_Mod;
        private int m_ModMem;
        private int InitPointer;
        private InitializeModule init;
        private IntPtr myFuncList = IntPtr.Zero;
        private FuncList myFunctionList;
        private int pFuncList;
        private int ppFuncList;
        private WardenFuncList myWardenList;
        private int pWardenList;
        private GCHandle gchSendPacket;
        private GCHandle gchCheckModule;
        private GCHandle gchModuleLoad;
        private GCHandle gchAllocateMem;
        private GCHandle gchReleaseMem;
        private GCHandle gchSetRC4Data;
        private GCHandle gchGetRC4Data;

        private bool InitModule(ref byte[] Data)
        {
            int A;
            int B;
            int C;
            var bCode = new byte[16];
            MemoryStream ms = new(Data);
            BinaryReader br = new(ms);
            Marshal.Copy(Data, 0, new IntPtr(m_Mod), Data.Length);
            br.BaseStream.Position = 0x18L;
            C = br.ReadInt32();
            B = 1 - C;
            br.BaseStream.Position = 0x14L;
            if (B > br.ReadInt32())
            {
                return false;
            }

            br.BaseStream.Position = 0x10L;
            A = br.ReadInt32();
            br.BaseStream.Position = A + (B * 4);
            A = br.ReadInt32() + m_Mod;
            InitPointer = A;
            Console.WriteLine("Initialize Function is mapped at 0x{0:X}", InitPointer);
            SendPacketD = SendPacket;
            CheckModuleD = CheckModule;
            ModuleLoadD = ModuleLoad;
            AllocateMemD = AllocateMem;
            FreeMemoryD = FreeMemory;
            SetRC4DataD = SetRC4Data;
            GetRC4DataD = GetRC4Data;
            myFunctionList = new FuncList
            {
                fpSendPacket = Marshal.GetFunctionPointerForDelegate(SendPacketD).ToInt32(),
                fpCheckModule = Marshal.GetFunctionPointerForDelegate(CheckModuleD).ToInt32(),
                fpLoadModule = Marshal.GetFunctionPointerForDelegate(ModuleLoadD).ToInt32(),
                fpAllocateMemory = Marshal.GetFunctionPointerForDelegate(AllocateMemD).ToInt32(),
                fpReleaseMemory = Marshal.GetFunctionPointerForDelegate(FreeMemoryD).ToInt32(),
                fpSetRC4Data = Marshal.GetFunctionPointerForDelegate(SetRC4DataD).ToInt32(),
                fpGetRC4Data = Marshal.GetFunctionPointerForDelegate(GetRC4DataD).ToInt32()
            };
            Console.WriteLine("Imports: ");
            Console.WriteLine("  SendPacket: 0x{0:X}", myFunctionList.fpSendPacket);
            Console.WriteLine("  CheckModule: 0x{0:X}", myFunctionList.fpCheckModule);
            Console.WriteLine("  LoadModule: 0x{0:X}", myFunctionList.fpLoadModule);
            Console.WriteLine("  AllocateMemory: 0x{0:X}", myFunctionList.fpAllocateMemory);
            Console.WriteLine("  ReleaseMemory: 0x{0:X}", myFunctionList.fpReleaseMemory);
            Console.WriteLine("  SetRC4Data: 0x{0:X}", myFunctionList.fpSetRC4Data);
            Console.WriteLine("  GetRC4Data: 0x{0:X}", myFunctionList.fpGetRC4Data);

            // http://forum.valhallalegends.com/index.php?topic=17758.0
            myFuncList = new IntPtr(malloc(0x1C));
            Marshal.StructureToPtr(myFunctionList, myFuncList, false);
            pFuncList = myFuncList.ToInt32();
            int localVarPtr()
            { object argobj = pFuncList; var ret = VarPtr(ref argobj); return ret; }

            ppFuncList = localVarPtr();
            Console.WriteLine("Initializing module");
            init = (InitializeModule)Marshal.GetDelegateForFunctionPointer(new IntPtr(InitPointer), typeof(InitializeModule));
            m_ModMem = init.Invoke(ppFuncList);
            pWardenList = Marshal.ReadInt32(new IntPtr(m_ModMem));
            myWardenList = (WardenFuncList)Marshal.PtrToStructure(new IntPtr(pWardenList), typeof(WardenFuncList));
            Console.WriteLine("Exports:");
            Console.WriteLine("  GenerateRC4Keys: 0x{0:X}", myWardenList.fpGenerateRC4Keys);
            Console.WriteLine("  Unload: 0x{0:X}", myWardenList.fpUnload);
            Console.WriteLine("  PacketHandler: 0x{0:X}", myWardenList.fpPacketHandler);
            Console.WriteLine("  Tick: 0x{0:X}", myWardenList.fpTick);
            GenerateRC4Keys = (GenerateRC4KeysDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(myWardenList.fpGenerateRC4Keys), typeof(GenerateRC4KeysDelegate));
            UnloadModule = (UnloadModuleDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(myWardenList.fpUnload), typeof(UnloadModuleDelegate));
            PacketHandler = (PacketHandlerDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(myWardenList.fpPacketHandler), typeof(PacketHandlerDelegate));
            Tick = (TickDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(myWardenList.fpTick), typeof(TickDelegate));
            ms.Close();
            ms.Dispose();
            ms = null;
            br = null;
            return true;
        }

        private void Unload_Module()
        {
            free(m_Mod);
        }

        [StructLayout(LayoutKind.Explicit, Size = 0x1C)]
        private struct FuncList
        {
            [FieldOffset(0x0)]
            public int fpSendPacket;

            [FieldOffset(0x4)]
            public int fpCheckModule;

            [FieldOffset(0x8)]
            public int fpLoadModule;

            [FieldOffset(0xC)]
            public int fpAllocateMemory;

            [FieldOffset(0x10)]
            public int fpReleaseMemory;

            [FieldOffset(0x14)]
            public int fpSetRC4Data;

            [FieldOffset(0x18)]
            public int fpGetRC4Data;
        }

        [StructLayout(LayoutKind.Explicit, Size = 0x10)]
        private struct WardenFuncList
        {
            [FieldOffset(0x0)]
            public int fpGenerateRC4Keys;

            [FieldOffset(0x4)]
            public int fpUnload;

            [FieldOffset(0x8)]
            public int fpPacketHandler;

            [FieldOffset(0xC)]
            public int fpTick;
        }

        private int m_RC4;
        private byte[] m_PKT = Array.Empty<byte>();

        private void SendPacket(int ptrPacket, int dwSize)
        {
            if (dwSize < 1)
            {
                return;
            }

            if (dwSize > 5000)
            {
                return;
            }

            m_PKT = new byte[dwSize];
            Marshal.Copy(new IntPtr(ptrPacket), m_PKT, 0, dwSize);
            Console.WriteLine("Warden.SendPacket() ptrPacket={0}, size={1}", ptrPacket, dwSize);
        }

        private int CheckModule(int ptrMod, int ptrKey)
        {
            // CheckModule = 0 '//Need to download
            // CheckModule = 1 '//Don't need to download
            Console.WriteLine("Warden.CheckModule() Mod={0}, size={1}", ptrMod, ptrKey);
            return 1;
        }

        private int ModuleLoad(int ptrRC4Key, int pModule, int dwModSize)
        {
            // ModuleLoad = 0 '//Need to download
            // ModuleLoad = 1 '//Don't need to download
            Console.WriteLine("Warden.ModuleLoad() Key={0}, Mod={1}, size={2}", ptrRC4Key, pModule, dwModSize);
            return 1;
        }

        private int AllocateMem(int dwSize)
        {
            Console.WriteLine("Warden.AllocateMem() Size={0}", dwSize);
            return malloc(dwSize);
        }

        private void FreeMemory(int dwMemory)
        {
            Console.WriteLine("Warden.FreeMemory() Memory={0}", dwMemory);
            free(dwMemory);
        }

        private int SetRC4Data(int lpKeys, int dwSize)
        {
            Console.WriteLine("Warden.SetRC4Data() Keys={0}, Size={1}", lpKeys, dwSize);
            return 1;
        }

        private int GetRC4Data(int lpBuffer, ref int dwSize)
        {
            Console.WriteLine("Warden.GetRC4Data() Buffer={0}, Size={1}", lpBuffer, dwSize);
            // GetRC4Data = 1 'got the keys already
            // GetRC4Data = 0 'generate new keys

            for (int i = 0, loopTo = dwSize - 1; i <= loopTo; i++) // Clear the keys
            {
                Marshal.WriteByte(new IntPtr(lpBuffer + i), 0);
            }

            m_RC4 = lpBuffer;
            return 1;
        }

        public void GenerateNewRC4Keys(byte[] K)
        {
            m_RC4 = 0;
            GenerateRC4Keys(m_ModMem, ByteArrPtr(ref K), K.Length);
        }

        public int HandlePacket(byte[] PacketData)
        {
            m_PKT = Array.Empty<byte>();
            var BytesRead = 0;
            int localVarPtr()
            { object argobj = BytesRead; var ret = VarPtr(ref argobj); return ret; }

            BytesRead = localVarPtr();
            PacketHandler(m_ModMem, ByteArrPtr(ref PacketData), PacketData.Length, BytesRead);
            return Marshal.ReadInt32(new IntPtr(BytesRead));
        }

        public byte[] ReadPacket()
        {
            return m_PKT;
        }

        public void ReadKeys()
        {
            var KeyData = new byte[516];
            Marshal.Copy(new IntPtr(m_ModMem + 32), KeyData, 0, KeyData.Length);
            Buffer.BlockCopy(KeyData, 0, KeyOut, 0, 258);
            Buffer.BlockCopy(KeyData, 258, KeyIn, 0, 258);
        }

        public void ReadKeys2()
        {
            var KeyData = new byte[516];
            Marshal.Copy(new IntPtr(m_ModMem + 32), KeyData, 0, KeyData.Length);
            Buffer.BlockCopy(KeyData, 0, ModKeyOut, 0, 258);
            Buffer.BlockCopy(KeyData, 258, ModKeyIn, 0, 258);
        }
    }

    [DllImport("kernel32.dll")]
    private static extern int GlobalLock(int hMem);

    [DllImport("kernel32.dll")]
    private static extern int GlobalUnlock(int hMem);

    private static int VarPtr(ref object obj)
    {
        GCHandle gc = GCHandle.Alloc(obj, GCHandleType.Pinned);
        return gc.AddrOfPinnedObject().ToInt32();
    }

    private static int ByteArrPtr(ref byte[] arr)
    {
        var pData = malloc(arr.Length);
        Marshal.Copy(arr, 0, new IntPtr(pData), arr.Length);
        return pData;
    }

    private static int malloc(int length)
    {
        var tmpHandle = Marshal.AllocHGlobal(length + 4).ToInt32();
        var lockedHandle = GlobalLock(tmpHandle) + 4;
        Marshal.WriteInt32(new IntPtr(lockedHandle - 4), tmpHandle);
        return lockedHandle;
    }

    private static void free(int ptr)
    {
        var tmpHandle = Marshal.ReadInt32(new IntPtr(ptr - 4));
        GlobalUnlock(tmpHandle);
        Marshal.FreeHGlobal(new IntPtr(tmpHandle));
    }
}
