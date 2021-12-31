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

using Mangos.Common.Enums.Global;
using Mangos.World.Globals;
using Mangos.World.Handlers;
using Mangos.World.Player;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Mangos.World.Warden;

public partial class WS_Warden
{
    public class WardenMaiev : IDisposable
    {
        [StructLayout(LayoutKind.Explicit, Size = 8)]
        public struct CLibraryEntry
        {
            [FieldOffset(0)]
            public int dwFileName;

            [FieldOffset(4)]
            public int dwImports;
        }

        [StructLayout(LayoutKind.Explicit, Size = 40)]
        public struct CHeader
        {
            [FieldOffset(0)]
            public int dwModuleSize;

            [FieldOffset(4)]
            public int dwDestructor;

            [FieldOffset(8)]
            public int dwSizeOfCode;

            [FieldOffset(12)]
            public int dwRelocationCount;

            [FieldOffset(16)]
            public int dwProcedureTable;

            [FieldOffset(20)]
            public int dwProcedureCount;

            [FieldOffset(24)]
            public int dwProcedureAdjust;

            [FieldOffset(28)]
            public int dwLibraryTable;

            [FieldOffset(32)]
            public int dwLibraryCount;

            [FieldOffset(36)]
            public int dwChunkCount;
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

        [StructLayout(LayoutKind.Explicit, Size = 28)]
        private struct FuncList
        {
            [FieldOffset(0)]
            public int fpSendPacket;

            [FieldOffset(4)]
            public int fpCheckModule;

            [FieldOffset(8)]
            public int fpLoadModule;

            [FieldOffset(12)]
            public int fpAllocateMemory;

            [FieldOffset(16)]
            public int fpReleaseMemory;

            [FieldOffset(20)]
            public int fpSetRC4Data;

            [FieldOffset(24)]
            public int fpGetRC4Data;
        }

        [StructLayout(LayoutKind.Explicit, Size = 16)]
        private struct WardenFuncList
        {
            [FieldOffset(0)]
            public int fpGenerateRC4Keys;

            [FieldOffset(4)]
            public int fpUnload;

            [FieldOffset(8)]
            public int fpPacketHandler;

            [FieldOffset(12)]
            public int fpTick;
        }

        public class ApiRedirector
        {
        }

        public byte[] WardenModule;

        public string ModuleName;

        public byte[] ModuleKey;

        public byte[] ModuleData;

        public int ModuleSize;

        public byte[] CheckIDs;

        public ScriptedObject Script;

        private readonly Dictionary<string, Delegate> delegateCache;

        private int dwModuleSize;

        private int dwLibraryCount;

        private CHeader Header;

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

        private readonly int m_ModMem;

        private int InitPointer;

        private InitializeModule init;

        private IntPtr myFuncList;

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

        private int m_RC4;

        private byte[] m_PKT;

        private bool _disposedValue;

        public WardenMaiev()
        {
            WardenModule = Array.Empty<byte>();
            ModuleName = "";
            ModuleKey = Array.Empty<byte>();
            ModuleData = Array.Empty<byte>();
            ModuleSize = 0;
            CheckIDs = new byte[8];
            Script = null;
            delegateCache = new Dictionary<string, Delegate>();
            dwModuleSize = 0;
            dwLibraryCount = 0;
            SendPacketD = null;
            CheckModuleD = null;
            ModuleLoadD = null;
            AllocateMemD = null;
            FreeMemoryD = null;
            SetRC4DataD = null;
            GetRC4DataD = null;
            GenerateRC4Keys = null;
            UnloadModule = null;
            PacketHandler = null;
            Tick = null;
            m_Mod = 0;
            m_ModMem = 0;
            InitPointer = 0;
            init = null;
            myFuncList = IntPtr.Zero;
            myFunctionList = default;
            pFuncList = 0;
            ppFuncList = 0;
            myWardenList = default;
            pWardenList = 0;
            m_RC4 = 0;
            m_PKT = Array.Empty<byte>();
        }

        public void InitWarden()
        {
            ModuleName = "5F947B8AB100C93D206D1FC3EA1FE6DD";
            Script = new ScriptedObject($"warden\\{ModuleName}.vb", $"warden\\{ModuleName}.dll", InMemory: false);
            WardenModule = (byte[])Script.InvokeField("Main", "MD5");
            ModuleKey = (byte[])Script.InvokeField("Main", "RC4");
            ModuleSize = Conversions.ToInteger(Script.InvokeField("Main", "Size"));
            CheckIDs[0] = Conversions.ToByte(Script.InvokeField("Main", "MEM_CHECK"));
            CheckIDs[1] = Conversions.ToByte(Script.InvokeField("Main", "PAGE_CHECK_A"));
            CheckIDs[2] = Conversions.ToByte(Script.InvokeField("Main", "MPQ_CHECK"));
            CheckIDs[3] = Conversions.ToByte(Script.InvokeField("Main", "LUA_STR_CHECK"));
            CheckIDs[4] = Conversions.ToByte(Script.InvokeField("Main", "DRIVER_CHECK"));
            CheckIDs[5] = Conversions.ToByte(Script.InvokeField("Main", "TIMING_CHECK"));
            CheckIDs[6] = Conversions.ToByte(Script.InvokeField("Main", "PROC_CHECK"));
            CheckIDs[7] = Conversions.ToByte(Script.InvokeField("Main", "MODULE_CHECK"));
            ModuleData = File.ReadAllBytes("warden\\" + ModuleName + ".bin");
            if (LoadModule(ModuleName, ref ModuleData, ModuleKey))
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.SUCCESS, "[WARDEN] Load of module, success [{0}]", ModuleName);
            }
            else
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "[WARDEN] Failed to load module [{0}]", ModuleName);
            }
        }

        public bool LoadModule(string Name, ref byte[] Data, byte[] Key)
        {
            Key = WS_Handlers_Warden.RC4.Init(Key);
            WS_Handlers_Warden.RC4.Crypt(ref Data, Key);
            var UncompressedLen = BitConverter.ToInt32(Data, 0);
            if (UncompressedLen < 0)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "[WARDEN] Failed to decrypt {0}, incorrect length.", Name);
                return false;
            }
            checked
            {
                var CompressedData = new byte[Data.Length - 264 - 1 + 1];
                Array.Copy(Data, 4, CompressedData, 0, CompressedData.Length);
                var dataPos = 4 + CompressedData.Length;
                var Sign = Conversions.ToString(Strings.Chr(Data[dataPos + 3])) + Conversions.ToString(Strings.Chr(Data[dataPos + 2])) + Conversions.ToString(Strings.Chr(Data[dataPos + 1])) + Conversions.ToString(Strings.Chr(Data[dataPos]));
                if (Operators.CompareString(Sign, "SIGN", TextCompare: false) != 0)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "[WARDEN] Failed to decrypt {0}, sign missing.", Name);
                    return false;
                }
                dataPos += 4;
                var Signature = new byte[256];
                Array.Copy(Data, dataPos, Signature, 0, Signature.Length);
                if (!CheckSignature(Signature, Data, Data.Length - 260))
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "[WARDEN] Signature fail on Warden Module.");
                    return false;
                }
                var DecompressedData = WorldServiceLocator._GlobalZip.DeCompress(CompressedData);
                if (!PrepairModule(ref DecompressedData))
                {
                    return false;
                }
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.SUCCESS, "[WARDEN] Successfully prepaired Warden Module.");
                try
                {
                    if (!InitModule())
                    {
                        return false;
                    }
                }
                catch (Exception ex2)
                {
                    ProjectData.SetProjectError(ex2);
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "[WARDEN] InitModule Failed.");
                    ProjectData.ClearProjectError();
                }
                return true;
            }
        }

        public bool CheckSignature(byte[] Signature, byte[] Data, int DataLen)
        {
            BigInteger power = new(new byte[4]
            {
                    1,
                    0,
                    1,
                    0
            }, isUnsigned: true, isBigEndian: true);
            BigInteger pmod = new(new byte[256]
            {
                    107,
                    206,
                    245,
                    45,
                    42,
                    125,
                    122,
                    103,
                    33,
                    33,
                    132,
                    201,
                    188,
                    37,
                    199,
                    188,
                    223,
                    61,
                    143,
                    217,
                    71,
                    188,
                    69,
                    72,
                    139,
                    34,
                    133,
                    59,
                    197,
                    193,
                    244,
                    245,
                    60,
                    12,
                    73,
                    187,
                    86,
                    224,
                    61,
                    188,
                    162,
                    210,
                    53,
                    193,
                    240,
                    116,
                    46,
                    21,
                    90,
                    6,
                    138,
                    104,
                    1,
                    158,
                    96,
                    23,
                    112,
                    139,
                    189,
                    248,
                    213,
                    249,
                    58,
                    211,
                    37,
                    178,
                    102,
                    146,
                    186,
                    67,
                    138,
                    129,
                    82,
                    15,
                    100,
                    152,
                    255,
                    96,
                    55,
                    175,
                    180,
                    17,
                    140,
                    249,
                    46,
                    197,
                    238,
                    202,
                    180,
                    65,
                    96,
                    60,
                    125,
                    2,
                    175,
                    161,
                    43,
                    155,
                    34,
                    75,
                    59,
                    252,
                    210,
                    93,
                    115,
                    233,
                    41,
                    52,
                    145,
                    133,
                    147,
                    76,
                    190,
                    190,
                    115,
                    169,
                    210,
                    59,
                    39,
                    122,
                    71,
                    118,
                    236,
                    176,
                    40,
                    201,
                    193,
                    218,
                    238,
                    170,
                    179,
                    150,
                    156,
                    30,
                    245,
                    107,
                    246,
                    100,
                    216,
                    148,
                    46,
                    241,
                    247,
                    20,
                    95,
                    160,
                    241,
                    163,
                    185,
                    177,
                    170,
                    88,
                    151,
                    220,
                    9,
                    23,
                    12,
                    4,
                    211,
                    142,
                    2,
                    44,
                    131,
                    138,
                    214,
                    175,
                    124,
                    254,
                    131,
                    51,
                    198,
                    168,
                    195,
                    132,
                    239,
                    41,
                    6,
                    169,
                    183,
                    45,
                    6,
                    11,
                    13,
                    111,
                    112,
                    158,
                    52,
                    166,
                    199,
                    49,
                    190,
                    86,
                    222,
                    221,
                    2,
                    146,
                    248,
                    160,
                    88,
                    11,
                    252,
                    250,
                    186,
                    73,
                    180,
                    72,
                    219,
                    236,
                    37,
                    243,
                    24,
                    143,
                    45,
                    179,
                    192,
                    184,
                    221,
                    188,
                    214,
                    170,
                    166,
                    219,
                    111,
                    125,
                    125,
                    37,
                    166,
                    205,
                    57,
                    109,
                    218,
                    118,
                    12,
                    121,
                    191,
                    72,
                    37,
                    252,
                    45,
                    197,
                    250,
                    83,
                    155,
                    77,
                    96,
                    244,
                    239,
                    199,
                    234,
                    172,
                    161,
                    123,
                    3,
                    244,
                    175,
                    199
            }, isUnsigned: true, isBigEndian: true);
            BigInteger sig = new(Signature, isUnsigned: true, isBigEndian: true);
            var result = BigInteger.ModPow(sig, power, pmod).ToByteArray(isUnsigned: true, isBigEndian: true);
            var properResult = new byte[256];
            checked
            {
                var num = properResult.Length - 1;
                for (var k = 0; k <= num; k++)
                {
                    properResult[k] = 187;
                }
                properResult[255] = 11;
                var tmpKey = "MAIEV.MOD";
                var bKey = new byte[tmpKey.Length - 1 + 1];
                var num2 = tmpKey.Length - 1;
                for (var j = 0; j <= num2; j++)
                {
                    bKey[j] = (byte)Strings.Asc(tmpKey[j]);
                }
                var newData = new byte[DataLen + bKey.Length - 1 + 1];
                Array.Copy(Data, 0, newData, 0, DataLen);
                Array.Copy(bKey, 0, newData, DataLen, bKey.Length);
                SHA1Managed sha1 = new();
                var digest = sha1.ComputeHash(newData);
                Array.Copy(digest, 0, properResult, 0, digest.Length);
                Console.WriteLine("Result:       " + BitConverter.ToString(result));
                Console.WriteLine("ProperResult: " + BitConverter.ToString(properResult));
                var num3 = result.Length - 1;
                for (var i = 0; i <= num3; i++)
                {
                    if (result[i] != properResult[i])
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        private bool PrepairModule(ref byte[] data)
        {
            checked
            {
                try
                {
                    var pModule = WorldServiceLocator._WS_Warden.ByteArrPtr(ref data);
                    var obj = Marshal.PtrToStructure(new IntPtr(pModule), typeof(CHeader));
                    Header = (obj != null) ? ((CHeader)obj) : default;
                    dwModuleSize = Header.dwModuleSize;
                    if (dwModuleSize < int.MaxValue)
                    {
                        m_Mod = WorldServiceLocator._WS_Warden.Malloc(dwModuleSize);
                        if (m_Mod != 0)
                        {
                            Marshal.Copy(data, 0, (IntPtr)m_Mod, 40);
                            var index = 40 + (Header.dwChunkCount * 3 * 4);
                            var dwChunkDest = m_Mod + BitConverter.ToInt32(data, 40);
                            var dwModuleEnd = m_Mod + dwModuleSize;
                            var bCopyChunk = true;
                            while (dwChunkDest < dwModuleEnd)
                            {
                                int dwCurrentChunkSize = BitConverter.ToInt16(data, index);
                                index += 2;
                                if (bCopyChunk)
                                {
                                    Marshal.Copy(data, index, new IntPtr(dwChunkDest), dwCurrentChunkSize);
                                    index += dwCurrentChunkSize;
                                }
                                dwChunkDest += dwCurrentChunkSize;
                                bCopyChunk = !bCopyChunk;
                            }
                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[WARDEN] Update...");
                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[WARDEN] Update: Adjusting references to global variables...");
                            var pbRelocationTable = m_Mod + Header.dwSizeOfCode;
                            var dwRelocationIndex = 0;
                            var dwLastRelocation = 0;
                            while (dwRelocationIndex < Header.dwRelocationCount)
                            {
                                int dwValue = Marshal.ReadByte(new IntPtr(pbRelocationTable));
                                if (dwValue < 0)
                                {
                                    dwValue = (dwValue & 0x7F) << 8;
                                    dwValue = (dwValue + Marshal.ReadByte(new IntPtr(checked(pbRelocationTable + 1)))) << 8;
                                    dwValue = (dwValue + Marshal.ReadByte(new IntPtr(checked(pbRelocationTable + 2)))) << 8;
                                    dwValue += Marshal.ReadByte(new IntPtr(checked(pbRelocationTable + 3)));
                                    pbRelocationTable += 4;
                                    var old2 = Marshal.ReadInt32(new IntPtr(m_Mod + dwValue));
                                    Marshal.WriteInt32(new IntPtr(m_Mod + dwValue), m_Mod + old2);
                                }
                                else
                                {
                                    dwValue = (dwValue << 8) + dwLastRelocation + Marshal.ReadByte(new IntPtr(checked(pbRelocationTable + 1)));
                                    pbRelocationTable += 2;
                                    var old = Marshal.ReadInt32(new IntPtr(m_Mod + dwValue));
                                    Marshal.WriteInt32(new IntPtr(m_Mod + dwValue), m_Mod + old);
                                }
                                dwRelocationIndex++;
                                dwLastRelocation = dwValue;
                            }
                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[WARDEN] Update: Updating API library references...");
                            var dwLibraryIndex = 0;
                            while (dwLibraryIndex < Header.dwLibraryCount)
                            {
                                var obj2 = Marshal.PtrToStructure(new IntPtr(m_Mod + Header.dwLibraryTable + (dwLibraryIndex * 8)), typeof(CLibraryEntry));
                                var pLibraryTable = (obj2 != null) ? ((CLibraryEntry)obj2) : default;
                                var procLib = Marshal.PtrToStringAnsi(new IntPtr(m_Mod + pLibraryTable.dwFileName));
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "    Library: {0}", procLib);
                                var hModule = NativeMethods.LoadLibrary(procLib, "");
                                if (hModule != 0)
                                {
                                    var dwImports = m_Mod + pLibraryTable.dwImports;
                                    var dwCurrent = Marshal.ReadInt32(new IntPtr(dwImports));
                                    while (dwCurrent != 0)
                                    {
                                        var procAddr = 0;
                                        dwCurrent = Marshal.ReadInt32(new IntPtr(dwImports));
                                        if (dwCurrent <= 0)
                                        {
                                            dwCurrent &= 0x7FFFFFFF;
                                            procAddr = (int)(uint)NativeMethods.GetProcAddress((IntPtr)hModule, Convert.ToString(new IntPtr(dwCurrent)), "");
                                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "        Ordinary: 0x{0:X8}", dwCurrent);
                                        }
                                        else
                                        {
                                            var procFunc = Marshal.PtrToStringAnsi(new IntPtr(m_Mod + dwCurrent));
                                            var procRedirector = typeof(ApiRedirector).GetMethod("my" + procFunc);
                                            var procDelegate = typeof(ApiRedirector).GetNestedType("d" + procFunc);
                                            if (procRedirector is null || procDelegate is null)
                                            {
                                                procAddr = (int)(uint)NativeMethods.GetProcAddress((IntPtr)hModule, procFunc, "");
                                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "        Function: {0} @ 0x{1:X8}", procFunc, procAddr);
                                            }
                                            else
                                            {
                                                delegateCache.Add(procFunc, Delegate.CreateDelegate(procDelegate, procRedirector));
                                                procAddr = (int)Marshal.GetFunctionPointerForDelegate(delegateCache[procFunc]);
                                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "        Function: {0} @ MY 0x{1:X8}", procFunc, procAddr);
                                            }
                                            Marshal.WriteInt32(new IntPtr(dwImports), procAddr);
                                        }
                                        dwImports += 4;
                                    }
                                }
                                dwLibraryIndex++;
                                dwLibraryCount++;
                            }
                            for (var dwIndex = 0; dwIndex < Header.dwChunkCount; dwIndex++)
                            {
                                var pdwChunk2 = m_Mod + ((10 + (dwIndex * 3 * 4)) * 4);
                                var dwOldProtect = 0u;
                                var lpAddress = m_Mod + Marshal.ReadInt32(new IntPtr(pdwChunk2));
                                var dwSize = Marshal.ReadInt32(new IntPtr(pdwChunk2 + 4));
                                var flNewProtect = Marshal.ReadInt32(new IntPtr(pdwChunk2 + 8));
                                var lpflOldProtect = (int)dwOldProtect;
                                NativeMethods.VirtualProtect(lpAddress, dwSize, flNewProtect, ref lpflOldProtect, "");
                                dwOldProtect = (uint)lpflOldProtect;
                                if ((unchecked((uint)flNewProtect) & 0xF0u) != 0)
                                {
                                    NativeMethods.FlushInstructionCache(NativeMethods.GetCurrentProcess(""), lpAddress, dwSize, "");
                                }
                            }
                            var bUnload = true;
                            if (Header.dwSizeOfCode < dwModuleSize)
                            {
                                var dwOffset = (Header.dwSizeOfCode + 4095) & -4096;
                                if (dwOffset >= Header.dwSizeOfCode && dwOffset > dwModuleSize)
                                {
                                    NativeMethods.VirtualFree(m_Mod + dwOffset, dwModuleSize - dwOffset, 16384, "");
                                }
                                bUnload = false;
                            }
                            return !bUnload;
                        }
                        return false;
                    }
                    return false;
                }
                catch (Exception ex2)
                {
                    ProjectData.SetProjectError(ex2);
                    var ex = ex2;
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Failed to prepair module.{0}{1}", Environment.NewLine, ex.ToString());
                    var PrepairModule = false;
                    ProjectData.ClearProjectError();
                    return PrepairModule;
                }
            }
        }

        private bool InitModule()
        {
            checked
            {
                var dwProcedureDiff = 1 - Header.dwProcedureAdjust;
                if (dwProcedureDiff > Header.dwProcedureCount)
                {
                    return false;
                }
                var fInit = Marshal.ReadInt32(new IntPtr(m_Mod + Header.dwProcedureTable + (dwProcedureDiff * 4)));
                InitPointer = m_Mod + fInit;
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
                myFuncList = new IntPtr(WorldServiceLocator._WS_Warden.Malloc(28));
                Marshal.StructureToPtr(myFunctionList, myFuncList, fDeleteOld: false);
                pFuncList = myFuncList.ToInt32();
                var wS_Warden = WorldServiceLocator._WS_Warden;
                ref var reference = ref pFuncList;
                object obj = reference;
                var num = wS_Warden.VarPtr(ref obj);
                reference = Conversions.ToInteger(obj);
                ppFuncList = num;
                Console.WriteLine("Initializing module");
                try
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.SUCCESS, "[WARDEN] Successfully Initialized Module.");
                    init = (InitializeModule)Marshal.GetDelegateForFunctionPointer(new IntPtr(InitPointer), typeof(InitializeModule));
                }
                catch (Exception ex2)
                {
                    ProjectData.SetProjectError(ex2);
                    var ex = ex2;
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "[WARDEN] Failed to Initialize Module.");
                    ProjectData.ClearProjectError();
                }
                pWardenList = Marshal.ReadInt32(new IntPtr(m_ModMem));
                var obj2 = Marshal.PtrToStructure(new IntPtr(pWardenList), typeof(WardenFuncList));
                myWardenList = (obj2 != null) ? ((WardenFuncList)obj2) : default;
                Console.WriteLine("Exports:");
                Console.WriteLine("  GenerateRC4Keys: 0x{0:X}", myWardenList.fpGenerateRC4Keys);
                Console.WriteLine("  Unload: 0x{0:X}", myWardenList.fpUnload);
                Console.WriteLine("  PacketHandler: 0x{0:X}", myWardenList.fpPacketHandler);
                Console.WriteLine("  Tick: 0x{0:X}", myWardenList.fpTick);
                GenerateRC4Keys = (GenerateRC4KeysDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(myWardenList.fpGenerateRC4Keys), typeof(GenerateRC4KeysDelegate));
                UnloadModule = (UnloadModuleDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(myWardenList.fpUnload), typeof(UnloadModuleDelegate));
                PacketHandler = (PacketHandlerDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(myWardenList.fpPacketHandler), typeof(PacketHandlerDelegate));
                Tick = (TickDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(myWardenList.fpTick), typeof(TickDelegate));
                return true;
            }
        }

        private void Unload_Module()
        {
            WorldServiceLocator._WS_Warden.Free(m_Mod);
        }

        private void SendPacket(int ptrPacket, int dwSize)
        {
            if (dwSize is >= 1 and <= 5000)
            {
                m_PKT = new byte[checked(dwSize - 1 + 1)];
                Marshal.Copy(new IntPtr(ptrPacket), m_PKT, 0, dwSize);
                Console.WriteLine("Warden.SendPacket() ptrPacket={0}, size={1}", ptrPacket, dwSize);
            }
        }

        private int CheckModule(int ptrMod, int ptrKey)
        {
            Console.WriteLine("Warden.CheckModule() Mod={0}, size={1}", ptrMod, ptrKey);
            return 1;
        }

        private int ModuleLoad(int ptrRC4Key, int pModule, int dwModSize)
        {
            Console.WriteLine("Warden.ModuleLoad() Key={0}, Mod={1}, size={2}", ptrRC4Key, pModule, dwModSize);
            return 1;
        }

        private int AllocateMem(int dwSize)
        {
            Console.WriteLine("Warden.AllocateMem() Size={0}", dwSize);
            return WorldServiceLocator._WS_Warden.Malloc(dwSize);
        }

        private void FreeMemory(int dwMemory)
        {
            Console.WriteLine("Warden.FreeMemory() Memory={0}", dwMemory);
            WorldServiceLocator._WS_Warden.Free(dwMemory);
        }

        private int SetRC4Data(int lpKeys, int dwSize)
        {
            Console.WriteLine("Warden.SetRC4Data() Keys={0}, Size={1}", lpKeys, dwSize);
            return 1;
        }

        private int GetRC4Data(int lpBuffer, ref int dwSize)
        {
            Console.WriteLine("Warden.GetRC4Data() Buffer={0}, Size={1}", lpBuffer, dwSize);
            checked
            {
                var num = dwSize - 1;
                for (var i = 0; i <= num; i++)
                {
                    Marshal.WriteByte(new IntPtr(lpBuffer + i), 0);
                }
                m_RC4 = lpBuffer;
                return 1;
            }
        }

        public void GenerateNewRC4Keys(byte[] K)
        {
            m_RC4 = 0;
            var pK = WorldServiceLocator._WS_Warden.ByteArrPtr(ref K);
            GenerateRC4Keys(m_ModMem, pK, K.Length);
            WorldServiceLocator._WS_Warden.Free(pK);
        }

        public int HandlePacket(byte[] PacketData)
        {
            m_PKT = Array.Empty<byte>();
            var BytesRead = 0;
            var wS_Warden = WorldServiceLocator._WS_Warden;
            object obj = BytesRead;
            var num = wS_Warden.VarPtr(ref obj);
            BytesRead = Conversions.ToInteger(obj);
            BytesRead = num;
            var pPacket = WorldServiceLocator._WS_Warden.ByteArrPtr(ref PacketData);
            PacketHandler(m_ModMem, pPacket, PacketData.Length, BytesRead);
            WorldServiceLocator._WS_Warden.Free(pPacket);
            return Marshal.ReadInt32(new IntPtr(BytesRead));
        }

        public byte[] ReadPacket()
        {
            return m_PKT;
        }

        public void ReadKeys(ref WS_PlayerData.CharacterObject objCharacter)
        {
            var KeyData = new byte[516];
            Marshal.Copy(new IntPtr(checked(m_ModMem + 32)), KeyData, 0, KeyData.Length);
            Buffer.BlockCopy(KeyData, 0, objCharacter.WardenData.KeyOut, 0, 258);
            Buffer.BlockCopy(KeyData, 258, objCharacter.WardenData.KeyIn, 0, 258);
        }

        public void ReadXorByte(ref WS_PlayerData.CharacterObject objCharacter)
        {
            var ClientSeed = new byte[16];
            Marshal.Copy(new IntPtr(checked(m_ModMem + 4)), ClientSeed, 0, ClientSeed.Length);
            objCharacter.WardenData.ClientSeed = ClientSeed;
            objCharacter.WardenData.xorByte = ClientSeed[0];
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                Script.Dispose();
            }
            _disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        void IDisposable.Dispose()
        {
            //ILSpy generated this explicit interface implementation from .override directive in Dispose
            Dispose();
        }
    }
}
