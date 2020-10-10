// 
// Copyright (C) 2013-2020 getMaNGOS <https://getmangos.eu>
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
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Warden;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Handlers;
using Mangos.World.Player;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Warden
{
    public class WS_Warden
    {

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public WardenMaiev Maiev = new WardenMaiev();

        public class WardenMaiev : IDisposable
        {
            public byte[] WardenModule = Array.Empty<byte>();
            public string ModuleName = "";
            public byte[] ModuleKey = Array.Empty<byte>();
            public byte[] ModuleData = Array.Empty<byte>();
            public int ModuleSize = 0;
            public byte[] CheckIDs = new byte[8];
            public ScriptedObject Script = null;

            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            public void InitWarden()
            {
                // ModuleName = "ED4272452F70779CC12079C155812E0A"
                ModuleName = "5F947B8AB100C93D206D1FC3EA1FE6DD";
                Script = new ScriptedObject(string.Format(@"warden\{0}.vb", ModuleName), string.Format(@"warden\{0}.dll", ModuleName), false);
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
                ModuleData = File.ReadAllBytes(@"warden\" + ModuleName + ".bin");
                if (LoadModule(ModuleName, ref ModuleData, ModuleKey))
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.SUCCESS, "[WARDEN] Load of module, success [{0}]", ModuleName);
                }
                else
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "[WARDEN] Failed to load module [{0}]", ModuleName);
                }
            }
            /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            public bool LoadModule(string Name, ref byte[] Data, byte[] Key)
            {
                Key = WS_Handlers_Warden.RC4.Init(Key);
                WS_Handlers_Warden.RC4.Crypt(ref Data, Key);
                int UncompressedLen = BitConverter.ToInt32(Data, 0);
                if (UncompressedLen < 0)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "[WARDEN] Failed to decrypt {0}, incorrect length.", Name);
                    return false;
                }

                var CompressedData = new byte[(Data.Length - 0x108)];
                Array.Copy(Data, 4, CompressedData, 0, CompressedData.Length);
                int dataPos = 4 + CompressedData.Length;
                string Sign = Conversions.ToString((char)Data[dataPos + 3]) + (char)Data[dataPos + 2] + (char)Data[dataPos + 1] + (char)Data[dataPos];
                if (Sign != "SIGN")
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "[WARDEN] Failed to decrypt {0}, sign missing.", Name);
                    return false;
                }

                dataPos += 4;
                var Signature = new byte[256];
                Array.Copy(Data, dataPos, Signature, 0, Signature.Length);

                // Check signature
                if (CheckSignature(Signature, Data, Data.Length - 0x104) == false)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "[WARDEN] Signature fail on Warden Module.");
                    return false;
                }

                byte[] DecompressedData = WorldServiceLocator._GlobalZip.DeCompress(CompressedData);
                if (!PrepairModule(ref DecompressedData))
                    return false;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.SUCCESS, "[WARDEN] Successfully prepaired Warden Module.");
                try
                {
                    if (!InitModule())
                        return false;
                }
                catch (Exception)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "[WARDEN] InitModule Failed.");
                }

                return true;
            }

            public bool CheckSignature(byte[] Signature, byte[] Data, int DataLen)
            {
                var power = new BigInteger(new byte[] { 0x1, 0x0, 0x1, 0x0 }, isUnsigned: true, isBigEndian: true);
                var pmod = new BigInteger(new byte[] { 0x6B, 0xCE, 0xF5, 0x2D, 0x2A, 0x7D, 0x7A, 0x67, 0x21, 0x21, 0x84, 0xC9, 0xBC, 0x25, 0xC7, 0xBC, 0xDF, 0x3D, 0x8F, 0xD9, 0x47, 0xBC, 0x45, 0x48, 0x8B, 0x22, 0x85, 0x3B, 0xC5, 0xC1, 0xF4, 0xF5, 0x3C, 0xC, 0x49, 0xBB, 0x56, 0xE0, 0x3D, 0xBC, 0xA2, 0xD2, 0x35, 0xC1, 0xF0, 0x74, 0x2E, 0x15, 0x5A, 0x6, 0x8A, 0x68, 0x1, 0x9E, 0x60, 0x17, 0x70, 0x8B, 0xBD, 0xF8, 0xD5, 0xF9, 0x3A, 0xD3, 0x25, 0xB2, 0x66, 0x92, 0xBA, 0x43, 0x8A, 0x81, 0x52, 0xF, 0x64, 0x98, 0xFF, 0x60, 0x37, 0xAF, 0xB4, 0x11, 0x8C, 0xF9, 0x2E, 0xC5, 0xEE, 0xCA, 0xB4, 0x41, 0x60, 0x3C, 0x7D, 0x2, 0xAF, 0xA1, 0x2B, 0x9B, 0x22, 0x4B, 0x3B, 0xFC, 0xD2, 0x5D, 0x73, 0xE9, 0x29, 0x34, 0x91, 0x85, 0x93, 0x4C, 0xBE, 0xBE, 0x73, 0xA9, 0xD2, 0x3B, 0x27, 0x7A, 0x47, 0x76, 0xEC, 0xB0, 0x28, 0xC9, 0xC1, 0xDA, 0xEE, 0xAA, 0xB3, 0x96, 0x9C, 0x1E, 0xF5, 0x6B, 0xF6, 0x64, 0xD8, 0x94, 0x2E, 0xF1, 0xF7, 0x14, 0x5F, 0xA0, 0xF1, 0xA3, 0xB9, 0xB1, 0xAA, 0x58, 0x97, 0xDC, 0x9, 0x17, 0xC, 0x4, 0xD3, 0x8E, 0x2, 0x2C, 0x83, 0x8A, 0xD6, 0xAF, 0x7C, 0xFE, 0x83, 0x33, 0xC6, 0xA8, 0xC3, 0x84, 0xEF, 0x29, 0x6, 0xA9, 0xB7, 0x2D, 0x6, 0xB, 0xD, 0x6F, 0x70, 0x9E, 0x34, 0xA6, 0xC7, 0x31, 0xBE, 0x56, 0xDE, 0xDD, 0x2, 0x92, 0xF8, 0xA0, 0x58, 0xB, 0xFC, 0xFA, 0xBA, 0x49, 0xB4, 0x48, 0xDB, 0xEC, 0x25, 0xF3, 0x18, 0x8F, 0x2D, 0xB3, 0xC0, 0xB8, 0xDD, 0xBC, 0xD6, 0xAA, 0xA6, 0xDB, 0x6F, 0x7D, 0x7D, 0x25, 0xA6, 0xCD, 0x39, 0x6D, 0xDA, 0x76, 0xC, 0x79, 0xBF, 0x48, 0x25, 0xFC, 0x2D, 0xC5, 0xFA, 0x53, 0x9B, 0x4D, 0x60, 0xF4, 0xEF, 0xC7, 0xEA, 0xAC, 0xA1, 0x7B, 0x3, 0xF4, 0xAF, 0xC7 }, isUnsigned: true, isBigEndian: true);
                var sig = new BigInteger(Signature, isUnsigned: true, isBigEndian: true);
                var res = BigInteger.ModPow(sig, power, pmod);
                var result = res.ToByteArray(isUnsigned: true, isBigEndian: true);
                byte[] digest;
                var properResult = new byte[256];
                for (int i = 0, loopTo = properResult.Length - 1; i <= loopTo; i++)
                    properResult[i] = 0xBB;
                properResult[0x100 - 1] = 0xB;
                string tmpKey = "MAIEV.MOD";
                var bKey = new byte[tmpKey.Length];
                for (int i = 0, loopTo1 = tmpKey.Length - 1; i <= loopTo1; i++)
                    bKey[i] = (byte)Strings.Asc(tmpKey[i]);
                var newData = new byte[(DataLen + bKey.Length)];
                Array.Copy(Data, 0, newData, 0, DataLen);
                Array.Copy(bKey, 0, newData, DataLen, bKey.Length);
                var sha1 = new SHA1Managed();
                digest = sha1.ComputeHash(newData);
                Array.Copy(digest, 0, properResult, 0, digest.Length);
                Console.WriteLine("Result:       " + BitConverter.ToString(result));
                Console.WriteLine("ProperResult: " + BitConverter.ToString(properResult));
                for (int i = 0, loopTo2 = result.Length - 1; i <= loopTo2; i++)
                {
                    if (result[i] != properResult[i])
                        return false;
                }

                return true;
            }
            /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            [StructLayout(LayoutKind.Explicit, Size = 0x8)]
            public struct CLibraryEntry
            {
                [FieldOffset(0x0)]
                public int dwFileName;
                [FieldOffset(0x4)]
                public int dwImports;
            }

            [StructLayout(LayoutKind.Explicit, Size = 0x28)]
            public struct CHeader
            {
                [FieldOffset(0x0)]
                public int dwModuleSize;
                [FieldOffset(0x4)]
                public int dwDestructor;
                [FieldOffset(0x8)]
                public int dwSizeOfCode;
                [FieldOffset(0xC)]
                public int dwRelocationCount;
                [FieldOffset(0x10)]
                public int dwProcedureTable;
                [FieldOffset(0x14)]
                public int dwProcedureCount;
                [FieldOffset(0x18)]
                public int dwProcedureAdjust;
                [FieldOffset(0x1C)]
                public int dwLibraryTable;
                [FieldOffset(0x20)]
                public int dwLibraryCount;
                [FieldOffset(0x24)]
                public int dwChunkCount;
            }

            private readonly Dictionary<string, Delegate> delegateCache = new Dictionary<string, Delegate>();
            private int dwModuleSize = 0;
            private int dwLibraryCount = 0;
            private CHeader Header;

            private bool PrepairModule(ref byte[] data)
            {
                try
                {
                    int pModule = WorldServiceLocator._WS_Warden.ByteArrPtr(ref data);
                    Header = (CHeader)Marshal.PtrToStructure(new IntPtr(pModule), typeof(CHeader));
                    dwModuleSize = Header.dwModuleSize;
                    if (dwModuleSize < 0x7FFFFFFF)
                    {
                        m_Mod = WorldServiceLocator._WS_Warden.Malloc(dwModuleSize);
                        if (Conversions.ToBoolean(m_Mod))
                        {
                            Marshal.Copy(data, 0, (IntPtr)m_Mod, 0x28);
                            int index = 0x28 + Header.dwChunkCount * 3 * 4;
                            int dwChunkDest = m_Mod + BitConverter.ToInt32(data, 0x28);
                            int dwModuleEnd = m_Mod + dwModuleSize;
                            bool bCopyChunk = true;
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
                            int pbRelocationTable = m_Mod + Header.dwSizeOfCode;
                            int dwRelocationIndex = 0;
                            int dwLastRelocation = 0;
                            while (dwRelocationIndex < Header.dwRelocationCount)
                            {
                                int dwValue = Marshal.ReadByte(new IntPtr(pbRelocationTable));
                                if (dwValue < 0)
                                {
                                    dwValue = (dwValue & 0x7F) << 8;
                                    dwValue = dwValue + Marshal.ReadByte(new IntPtr(pbRelocationTable + 1)) << 8;
                                    dwValue = dwValue + Marshal.ReadByte(new IntPtr(pbRelocationTable + 2)) << 8;
                                    dwValue += Marshal.ReadByte(new IntPtr(pbRelocationTable + 3));
                                    pbRelocationTable += 4;
                                    int old = Marshal.ReadInt32(new IntPtr(m_Mod + dwValue));
                                    Marshal.WriteInt32(new IntPtr(m_Mod + dwValue), m_Mod + old);
                                }
                                else
                                {
                                    dwValue = (dwValue << 8) + dwLastRelocation + Marshal.ReadByte(new IntPtr(pbRelocationTable + 1));
                                    pbRelocationTable += 2;
                                    int old = Marshal.ReadInt32(new IntPtr(m_Mod + dwValue));
                                    Marshal.WriteInt32(new IntPtr(m_Mod + dwValue), m_Mod + old);
                                }

                                dwRelocationIndex += 1;
                                dwLastRelocation = dwValue;
                            }

                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[WARDEN] Update: Updating API library references...");
                            int dwLibraryIndex = 0;
                            while (dwLibraryIndex < Header.dwLibraryCount)
                            {
                                CLibraryEntry pLibraryTable = (CLibraryEntry)Marshal.PtrToStructure(new IntPtr(m_Mod + Header.dwLibraryTable + dwLibraryIndex * 8), typeof(CLibraryEntry));
                                string procLib = Marshal.PtrToStringAnsi(new IntPtr(m_Mod + pLibraryTable.dwFileName));
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "    Library: {0}", procLib);
                                int hModule = NativeMethods.LoadLibrary(procLib, "");
                                if (Conversions.ToBoolean(hModule))
                                {
                                    int dwImports = m_Mod + pLibraryTable.dwImports;
                                    int dwCurrent = Marshal.ReadInt32(new IntPtr(dwImports));
                                    while (dwCurrent != 0)
                                    {
                                        int procAddr = 0;
                                        dwCurrent = Marshal.ReadInt32(new IntPtr(dwImports));
                                        if (dwCurrent <= 0)
                                        {
                                            dwCurrent = dwCurrent & 0x7FFFFFFF;
                                            procAddr = (int)NativeMethods.GetProcAddress((IntPtr)hModule, Convert.ToString(new IntPtr(dwCurrent)), "");
                                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "        Ordinary: 0x{0:X8}", dwCurrent);
                                        }
                                        else
                                        {
                                            string procFunc = Marshal.PtrToStringAnsi(new IntPtr(m_Mod + dwCurrent));
                                            var procRedirector = typeof(ApiRedirector).GetMethod("my" + procFunc);
                                            var procDelegate = typeof(ApiRedirector).GetNestedType("d" + procFunc);
                                            if (procRedirector is null || procDelegate is null)
                                            {
                                                procAddr = (int)NativeMethods.GetProcAddress((IntPtr)hModule, procFunc, "");
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

                                dwLibraryIndex += 1;
                                dwLibraryCount += 1;
                            }

                            int dwIndex = 0;
                            while (dwIndex < Header.dwChunkCount)
                            {
                                int pdwChunk2 = m_Mod + (10 + dwIndex * 3 * 4) * 4;
                                uint dwOldProtect = 0U;
                                int lpAddress = m_Mod + Marshal.ReadInt32(new IntPtr(pdwChunk2));
                                int dwSize = Marshal.ReadInt32(new IntPtr(pdwChunk2 + 4));
                                int flNewProtect = Marshal.ReadInt32(new IntPtr(pdwChunk2 + 8));
                                int arglpflOldProtect = (int)dwOldProtect;
                                NativeMethods.VirtualProtect(lpAddress, dwSize, flNewProtect, ref arglpflOldProtect, "");
                                if (Conversions.ToBoolean(flNewProtect & 0xF0))
                                {
                                    NativeMethods.FlushInstructionCache(NativeMethods.GetCurrentProcess(""), lpAddress, dwSize, "");
                                }

                                dwIndex += 1;
                            }

                            bool bUnload = true;
                            if (Header.dwSizeOfCode < dwModuleSize)
                            {
                                int dwOffset = Header.dwSizeOfCode + 0xFFF & 0xFFFFF000;
                                if (dwOffset >= Header.dwSizeOfCode && dwOffset > dwModuleSize)
                                {
                                    NativeMethods.VirtualFree(m_Mod + dwOffset, dwModuleSize - dwOffset, 0x4000, "");
                                }

                                bUnload = false;
                            }

                            if (bUnload)
                            {
                                // TODO: Unload?
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Failed to prepair module.{0}{1}", Environment.NewLine, ex.ToString());
                    return false;
                }
            }

            /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
            /* TODO ERROR: Skipped RegionDirectiveTrivia */
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

            private SendPacketDelegate SendPacketD = null;
            private CheckModuleDelegate CheckModuleD = null;
            private ModuleLoadDelegate ModuleLoadD = null;
            private AllocateMemDelegate AllocateMemD = null;
            private FreeMemoryDelegate FreeMemoryD = null;
            private SetRC4DataDelegate SetRC4DataD = null;
            private GetRC4DataDelegate GetRC4DataD = null;
            private GenerateRC4KeysDelegate GenerateRC4Keys = null;
            private UnloadModuleDelegate UnloadModule = null;
            private PacketHandlerDelegate PacketHandler = null;
            private TickDelegate Tick = null;
            private int m_Mod = 0;
            private readonly int m_ModMem = 0;
            private int InitPointer = 0;
            private InitializeModule init = null;
            private IntPtr myFuncList = IntPtr.Zero;
            private FuncList myFunctionList = default;
            private int pFuncList = 0;
            private int ppFuncList = 0;
            private WardenFuncList myWardenList = default;
            private int pWardenList = 0;
            private GCHandle gchSendPacket;
            private GCHandle gchCheckModule;
            private GCHandle gchModuleLoad;
            private GCHandle gchAllocateMem;
            private GCHandle gchReleaseMem;
            private GCHandle gchSetRC4Data;
            private GCHandle gchGetRC4Data;

            private bool InitModule()
            {
                int dwProcedureDiff = 1 - Header.dwProcedureAdjust;
                if (dwProcedureDiff > Header.dwProcedureCount)
                    return false;
                int fInit = Marshal.ReadInt32(new IntPtr(m_Mod + Header.dwProcedureTable + dwProcedureDiff * 4));
                InitPointer = m_Mod + fInit;
                Console.WriteLine("Initialize Function is mapped at 0x{0:X}", InitPointer);
                SendPacketD = SendPacket;
                CheckModuleD = CheckModule;
                ModuleLoadD = ModuleLoad;
                AllocateMemD = AllocateMem;
                FreeMemoryD = FreeMemory;
                SetRC4DataD = SetRC4Data;
                GetRC4DataD = GetRC4Data;
                myFunctionList = new FuncList()
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
                myFuncList = new IntPtr(WorldServiceLocator._WS_Warden.Malloc(0x1C));
                Marshal.StructureToPtr(myFunctionList, myFuncList, false);
                pFuncList = myFuncList.ToInt32();
                int localVarPtr() { object argobj = pFuncList; var ret = WorldServiceLocator._WS_Warden.VarPtr(ref argobj); return ret; }

                ppFuncList = localVarPtr();
                Console.WriteLine("Initializing module");
                try
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.SUCCESS, "[WARDEN] Successfully Initialized Module.");
                    init = (InitializeModule)Marshal.GetDelegateForFunctionPointer(new IntPtr(InitPointer), typeof(InitializeModule));
                }
                catch (Exception)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "[WARDEN] Failed to Initialize Module.");
                }

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
                return true;
            }

            /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            private void Unload_Module()
            {
                // TODO!!
                WorldServiceLocator._WS_Warden.Free(m_Mod);
            }
            /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
            /* TODO ERROR: Skipped RegionDirectiveTrivia */
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

            private int m_RC4 = 0;
            private byte[] m_PKT = Array.Empty<byte>();

            private void SendPacket(int ptrPacket, int dwSize)
            {
                if (dwSize < 1)
                    return;
                if (dwSize > 5000)
                    return;
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
                // GetRC4Data = 1 'got the keys already
                // GetRC4Data = 0 'generate new keys

                for (int i = 0, loopTo = dwSize - 1; i <= loopTo; i++) // Clear the keys
                    Marshal.WriteByte(new IntPtr(lpBuffer + i), 0);
                m_RC4 = lpBuffer;
                return 1;
            }

            public void GenerateNewRC4Keys(byte[] K)
            {
                m_RC4 = 0;
                int pK = WorldServiceLocator._WS_Warden.ByteArrPtr(ref K);
                GenerateRC4Keys(m_ModMem, pK, K.Length);
                WorldServiceLocator._WS_Warden.Free(pK);
            }

            public int HandlePacket(byte[] PacketData)
            {
                m_PKT = new byte[] { };
                int BytesRead = 0;
                int localVarPtr() { object argobj = BytesRead; var ret = WorldServiceLocator._WS_Warden.VarPtr(ref argobj); return ret; }

                BytesRead = localVarPtr();
                int pPacket = WorldServiceLocator._WS_Warden.ByteArrPtr(ref PacketData);
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
                Marshal.Copy(new IntPtr(m_ModMem + 32), KeyData, 0, KeyData.Length);
                Buffer.BlockCopy(KeyData, 0, objCharacter.WardenData.KeyOut, 0, 258);
                Buffer.BlockCopy(KeyData, 258, objCharacter.WardenData.KeyIn, 0, 258);
            }

            public void ReadXorByte(ref WS_PlayerData.CharacterObject objCharacter)
            {
                var ClientSeed = new byte[16];
                Marshal.Copy(new IntPtr(m_ModMem + 4), ClientSeed, 0, ClientSeed.Length);
                objCharacter.WardenData.ClientSeed = ClientSeed;
                objCharacter.WardenData.xorByte = ClientSeed[0];
            }
            /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            public class ApiRedirector
            {
            }
            /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            private bool _disposedValue; // To detect redundant calls

            // IDisposable
            protected virtual void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                    // TODO: set large fields to null.
                    Script.Dispose();
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
            /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public class WardenScan
        {
            private readonly WS_PlayerData.CharacterObject Character = null;
            private readonly List<string> UsedStrings = new List<string>();
            private readonly List<CheatCheck> Checks = new List<CheatCheck>();

            public WardenScan(ref WS_PlayerData.CharacterObject objCharacter)
            {
                Character = objCharacter;
            }

            public void Do_MEM_CHECK(string ScanModule, int Offset, byte Length)
            {
                var newCheck = new CheatCheck(CheckTypes.MEM_CHECK)
                {
                    Str = ScanModule,
                    Addr = Offset,
                    Length = Length
                };
                if (!string.IsNullOrEmpty(ScanModule))
                    UsedStrings.Add(ScanModule);
                Checks.Add(newCheck);
            }

            public void Do_PAGE_CHECK_A_B(int Seed, byte[] Hash, int Offset, byte Length)
            {
                var newCheck = new CheatCheck(CheckTypes.PAGE_CHECK_A_B)
                {
                    Seed = Seed,
                    Hash = Hash,
                    Addr = Offset,
                    Length = Length
                };
                Checks.Add(newCheck);
            }

            public void Do_MPQ_CHECK(string File)
            {
                var newCheck = new CheatCheck(CheckTypes.MPQ_CHECK) { Str = File };
                UsedStrings.Add(File);
                Checks.Add(newCheck);
            }

            public void Do_LUA_STR_CHECK(string str)
            {
                var newCheck = new CheatCheck(CheckTypes.LUA_STR_CHECK) { Str = str };
                UsedStrings.Add(str);
                Checks.Add(newCheck);
            }

            public void Do_DRIVER_CHECK(int Seed, byte[] Hash, string Driver)
            {
                var newCheck = new CheatCheck(CheckTypes.DRIVER_CHECK)
                {
                    Seed = Seed,
                    Hash = Hash,
                    Str = Driver
                };
                UsedStrings.Add(Driver);
                Checks.Add(newCheck);
            }

            public void Do_TIMING_CHECK()
            {
                var newCheck = new CheatCheck(CheckTypes.TIMING_CHECK);
                Checks.Add(newCheck);
            }

            public void Do_PROC_CHECK(int Seed, byte[] Hash, string ScanModule, string ProcName, int Offset, byte Length)
            {
                var newCheck = new CheatCheck(CheckTypes.PROC_CHECK)
                {
                    Seed = Seed,
                    Hash = Hash,
                    Str = ScanModule,
                    Str2 = ProcName,
                    Addr = Offset,
                    Length = Length
                };
                UsedStrings.Add(ScanModule);
                UsedStrings.Add(ProcName);
                Checks.Add(newCheck);
            }

            public void Do_MODULE_CHECK(int Seed, byte[] Hash)
            {
                var newCheck = new CheatCheck(CheckTypes.MODULE_CHECK)
                {
                    Seed = Seed,
                    Hash = Hash
                };
                Checks.Add(newCheck);
            }

            public Packets.PacketClass GetPacket()
            {
                var packet = new Packets.PacketClass(OPCODES.SMSG_WARDEN_DATA);
                packet.AddInt8(MaievOpcode.MAIEV_MODULE_RUN);
                foreach (string tmpStr in UsedStrings)
                    packet.AddString2(tmpStr);
                packet.AddInt8(0);
                byte i = 0;
                foreach (CheatCheck Check in Checks)
                {
                    byte xorCheck = WorldServiceLocator._WS_Warden.Maiev.CheckIDs[Check.Type] ^ Character.WardenData.xorByte;
                    var checkData = Check.ToData(xorCheck, ref i);
                    packet.AddByteArray(checkData);
                }

                packet.AddInt8(Character.WardenData.xorByte);
                return packet;
            }

            public void Reset()
            {
                Checks.Clear();
                UsedStrings.Clear();
            }

            public void HandleResponse(ref Packets.PacketClass p)
            {
                // TODO: Now do the check if we have a cheater or not :)

                foreach (CheatCheck Check in Checks)
                {
                    switch (Check.Type)
                    {
                        case var @case when @case == CheckTypes.MEM_CHECK: // MEM_CHECK: uint8 result, uint8[] bytes
                            {
                                byte result = p.GetInt8();
                                var bytes = p.GetByteArray(); // (Check.Length)
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[WARDEN] [{0}] Result={1} Bytes=0x{2}", Check.Type, result, BitConverter.ToString(bytes).Replace("-", ""));
                                break;
                            }

                        case var case1 when case1 == CheckTypes.PAGE_CHECK_A_B: // PAGE_CHECK_A_B: uint8 result
                            {
                                byte result = p.GetInt8();
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[WARDEN] [{0}] Result={1}", Check.Type, result);
                                break;
                            }

                        case var case2 when case2 == CheckTypes.MPQ_CHECK: // MPQ_CHECK: uint8 result, uint8[20] sha1
                            {
                                byte result = p.GetInt8();
                                var hash = p.GetByteArray(); // (20)
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[WARDEN] [{0}] Result={1} Hash=0x{2}", Check.Type, result, BitConverter.ToString(hash).Replace("-", ""));
                                break;
                            }

                        case var case3 when case3 == CheckTypes.LUA_STR_CHECK: // LUA_STR_CHECK: uint8 unk, uint8 len, char[len] data
                            {
                                byte unk = p.GetInt8();
                                string data = p.GetString2();
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[WARDEN] [{0}] Result={1} Data={2}", Check.Type, unk, data);
                                break;
                            }

                        case var case4 when case4 == CheckTypes.DRIVER_CHECK: // DRIVER_CHECK: uint8 result
                            {
                                byte result = p.GetInt8();
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[WARDEN] [{0}] Result={1}", Check.Type, result);
                                break;
                            }

                        case var case5 when case5 == CheckTypes.TIMING_CHECK: // TIMING_CHECK: uint8 result, uint32 time
                            {
                                byte result = p.GetInt8();
                                int time = p.GetInt32();
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[WARDEN] [{0}] Result={1} Time={2}", Check.Type, result, time);
                                break;
                            }

                        case var case6 when case6 == CheckTypes.PROC_CHECK: // PROC_CHECK: uint8 result
                            {
                                byte result = p.GetInt8();
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[WARDEN] [{0}] Result={1}", Check.Type, result);
                                break;
                            }

                        case var case7 when case7 == CheckTypes.MODULE_CHECK:
                            {
                                // What is the structure for this result?
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[WARDEN] [{0}]", Check.Type);
                                break;
                            }
                    }
                }

                Reset();
            }
        }

        public class CheatCheck
        {
            public CheckTypes Type;
            public string Str = "";
            public string Str2 = "";
            public int Addr = 0;
            public byte[] Hash = Array.Empty<byte>();
            public int Seed = 0;
            public byte Length = 0;

            public CheatCheck(CheckTypes Type_)
            {
                Type = Type_;
            }

            public byte[] ToData(byte XorCheck, ref byte index)
            {
                var ms = new MemoryStream();
                var bw = new BinaryWriter(ms);
                bw.Write(XorCheck);
                switch (Type)
                {
                    case var @case when @case == CheckTypes.MEM_CHECK: // byte strIndex + uint Offset + byte Len
                        {
                            if (string.IsNullOrEmpty(Str))
                            {
                                bw.Write((byte)0);
                            }
                            else
                            {
                                bw.Write(index);
                                index = (byte)(index + 1);
                            }

                            bw.Write(Addr);
                            bw.Write(Length);
                            break;
                        }

                    case var case1 when case1 == CheckTypes.PAGE_CHECK_A_B: // uint Seed + byte[20] SHA1 + uint Addr + byte Len
                        {
                            bw.Write(Seed);
                            bw.Write(Hash, 0, Hash.Length);
                            bw.Write(Addr);
                            bw.Write(Length);
                            break;
                        }

                    case var case2 when case2 == CheckTypes.MPQ_CHECK: // byte strIndex
                        {
                            bw.Write(index);
                            index = (byte)(index + 1);
                            break;
                        }

                    case var case3 when case3 == CheckTypes.LUA_STR_CHECK: // byte strIndex
                        {
                            bw.Write(index);
                            index = (byte)(index + 1);
                            break;
                        }

                    case var case4 when case4 == CheckTypes.DRIVER_CHECK: // uint Seed + byte[20] SHA1 + byte strIndex
                        {
                            bw.Write(Seed);
                            bw.Write(Hash, 0, Hash.Length);
                            bw.Write(index);
                            index = (byte)(index + 1);
                            break;
                        }

                    case var case5 when case5 == CheckTypes.TIMING_CHECK: // empty
                        {
                            break;
                        }

                    case var case6 when case6 == CheckTypes.PROC_CHECK: // uint Seed + byte[20] SHA1 + byte strIndex1 + byte strIndex2 + uint Offset + byte Len
                        {
                            bw.Write(Seed);
                            bw.Write(Hash, 0, Hash.Length);
                            bw.Write(index);
                            index = (byte)(index + 1);
                            bw.Write(index);
                            index = (byte)(index + 1);
                            bw.Write(Addr);
                            bw.Write(Length);
                            break;
                        }

                    case var case7 when case7 == CheckTypes.MODULE_CHECK: // uint Seed + byte[20] SHA1
                        {
                            bw.Write(Seed);
                            bw.Write(Hash, 0, Hash.Length);
                            break;
                        }
                }

                var tmpData = ms.ToArray();
                ms.Close();
                // ms.Dispose()
                return tmpData;
            }
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        private int VarPtr(ref object obj)
        {
            var gc = GCHandle.Alloc(obj, GCHandleType.Pinned);
            return gc.AddrOfPinnedObject().ToInt32();
        }

        private int ByteArrPtr(ref byte[] arr)
        {
            int pData = Malloc(arr.Length);
            Marshal.Copy(arr, 0, new IntPtr(pData), arr.Length);
            return pData;
        }

        private int Malloc(int length)
        {
            int tmpHandle = Marshal.AllocHGlobal(length + 4).ToInt32();
            int lockedHandle = NativeMethods.GlobalLock(tmpHandle, "") + 4;
            Marshal.WriteInt32(new IntPtr(lockedHandle - 4), tmpHandle);
            return lockedHandle;
        }

        private void Free(int ptr)
        {
            int tmpHandle = Marshal.ReadInt32(new IntPtr(ptr - 4));
            NativeMethods.GlobalUnlock(tmpHandle, "");
            Marshal.FreeHGlobal(new IntPtr(tmpHandle));
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        public void SendWardenPacket(ref WS_PlayerData.CharacterObject objCharacter, ref Packets.PacketClass Packet)
        {
            // START Warden Encryption
            var b = new byte[(Packet.Data.Length - 4)];
            Buffer.BlockCopy(Packet.Data, 4, b, 0, b.Length);
            WS_Handlers_Warden.RC4.Crypt(ref b, objCharacter.WardenData.KeyIn);
            Buffer.BlockCopy(b, 0, Packet.Data, 4, b.Length);
            // END

            objCharacter.client.Send(ref Packet);
        }
    }
}