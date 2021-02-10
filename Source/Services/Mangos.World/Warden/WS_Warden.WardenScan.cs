﻿//
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

using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Warden;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Player;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;

namespace Mangos.World.Warden
{
    public partial class WS_Warden
    {
        public class WardenScan
        {
            private readonly WS_PlayerData.CharacterObject Character;

            private readonly List<string> UsedStrings;

            private readonly List<CheatCheck> Checks;

            public WardenScan(ref WS_PlayerData.CharacterObject objCharacter)
            {
                Character = null;
                UsedStrings = new List<string>();
                Checks = new List<CheatCheck>();
                Character = objCharacter;
            }

            public void Do_MEM_CHECK(string ScanModule, int Offset, byte Length)
            {
                CheatCheck newCheck = new CheatCheck(CheckTypes.MEM_CHECK)
                {
                    Str = ScanModule,
                    Addr = Offset,
                    Length = Length
                };
                if (Operators.CompareString(ScanModule, "", TextCompare: false) != 0)
                {
                    UsedStrings.Add(ScanModule);
                }
                Checks.Add(newCheck);
            }

            public void Do_PAGE_CHECK_A_B(int Seed, byte[] Hash, int Offset, byte Length)
            {
                CheatCheck newCheck = new CheatCheck(CheckTypes.PAGE_CHECK_A_B)
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
                CheatCheck newCheck = new CheatCheck(CheckTypes.MPQ_CHECK)
                {
                    Str = File
                };
                UsedStrings.Add(File);
                Checks.Add(newCheck);
            }

            public void Do_LUA_STR_CHECK(string str)
            {
                CheatCheck newCheck = new CheatCheck(CheckTypes.LUA_STR_CHECK)
                {
                    Str = str
                };
                UsedStrings.Add(str);
                Checks.Add(newCheck);
            }

            public void Do_DRIVER_CHECK(int Seed, byte[] Hash, string Driver)
            {
                CheatCheck newCheck = new CheatCheck(CheckTypes.DRIVER_CHECK)
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
                CheatCheck newCheck = new CheatCheck(CheckTypes.TIMING_CHECK);
                Checks.Add(newCheck);
            }

            public void Do_PROC_CHECK(int Seed, byte[] Hash, string ScanModule, string ProcName, int Offset, byte Length)
            {
                CheatCheck newCheck = new CheatCheck(CheckTypes.PROC_CHECK)
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
                CheatCheck newCheck = new CheatCheck(CheckTypes.MODULE_CHECK)
                {
                    Seed = Seed,
                    Hash = Hash
                };
                Checks.Add(newCheck);
            }

            public Packets.PacketClass GetPacket()
            {
                Packets.PacketClass packet = new Packets.PacketClass(Opcodes.SMSG_WARDEN_DATA);
                packet.AddInt8(2);
                foreach (string tmpStr in UsedStrings)
                {
                    packet.AddString2(tmpStr);
                }
                packet.AddInt8(0);
                byte i = 0;
                foreach (CheatCheck Check in Checks)
                {
                    byte xorCheck = (byte)(WorldServiceLocator._WS_Warden.Maiev.CheckIDs[(uint)Check.Type] ^ Character.WardenData.xorByte);
                    byte[] checkData = Check.ToData(xorCheck, ref i);
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
                foreach (CheatCheck Check in Checks)
                {
                    switch (Check.Type)
                    {
                        case CheckTypes.MEM_CHECK:
                            {
                                byte result = p.GetInt8();
                                byte[] bytes = p.GetByteArray();
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[WARDEN] [{0}] Result={1} Bytes=0x{2}", Check.Type, result, BitConverter.ToString(bytes).Replace("-", ""));
                                break;
                            }
                        case CheckTypes.PAGE_CHECK_A_B:
                            {
                                byte result2 = p.GetInt8();
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[WARDEN] [{0}] Result={1}", Check.Type, result2);
                                break;
                            }
                        case CheckTypes.MPQ_CHECK:
                            {
                                byte result3 = p.GetInt8();
                                byte[] hash = p.GetByteArray();
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[WARDEN] [{0}] Result={1} Hash=0x{2}", Check.Type, result3, BitConverter.ToString(hash).Replace("-", ""));
                                break;
                            }
                        case CheckTypes.LUA_STR_CHECK:
                            {
                                byte unk = p.GetInt8();
                                string data = p.GetString2();
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[WARDEN] [{0}] Result={1} Data={2}", Check.Type, unk, data);
                                break;
                            }
                        case CheckTypes.DRIVER_CHECK:
                            {
                                byte result4 = p.GetInt8();
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[WARDEN] [{0}] Result={1}", Check.Type, result4);
                                break;
                            }
                        case CheckTypes.TIMING_CHECK:
                            {
                                byte result5 = p.GetInt8();
                                int time = p.GetInt32();
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[WARDEN] [{0}] Result={1} Time={2}", Check.Type, result5, time);
                                break;
                            }
                        case CheckTypes.PROC_CHECK:
                            {
                                byte result6 = p.GetInt8();
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[WARDEN] [{0}] Result={1}", Check.Type, result6);
                                break;
                            }
                        case CheckTypes.MODULE_CHECK:
                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[WARDEN] [{0}]", Check.Type);
                            break;
                    }
                }
                Reset();
            }
        }
    }
}