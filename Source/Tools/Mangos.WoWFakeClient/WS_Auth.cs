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
using System;
using System.Security.Cryptography;
using System.Text;

namespace Mangos.WoWFakeClient
{
    public static class WS_Auth
    {
        public static void On_SMSG_PONG(ref Packets.PacketClass Packet)
        {
            uint SequenceID = Packet.GetUInt32();
            int Latency = Worldserver.timeGetTime() - Worldserver.PingSent;
            if (SequenceID == Worldserver.CurrentPing && Latency >= 0)
            {
                Worldserver.CurrentLatency = Latency;
            }
        }

        public static void On_SMSG_AUTH_CHALLENGE(ref Packets.PacketClass Packet)
        {
            Console.WriteLine("[{0}][World] Received Auth Challenge.", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"));
            WS_WardenClient.InitWarden();
            Worldserver.ServerSeed = Packet.GetUInt32();
            byte[] temp = Encoding.ASCII.GetBytes(Realmserver.Account.ToCharArray());
            temp = Realmserver.Concat(temp, BitConverter.GetBytes(0));
            temp = Realmserver.Concat(temp, BitConverter.GetBytes(Worldserver.ClientSeed));
            temp = Realmserver.Concat(temp, BitConverter.GetBytes(Worldserver.ServerSeed));
            temp = Realmserver.Concat(temp, Realmserver.SS_Hash);
            SHA1Managed algorithm1 = new();
            byte[] ShaDigest = algorithm1.ComputeHash(temp);
            Worldserver.Decoding = true;
            VBMath.Randomize();
            Worldserver.ClientSeed = (uint)(uint.MaxValue * VBMath.Rnd());
            Packets.PacketClass Response = new(OPCODES.CMSG_AUTH_SESSION);
            Response.AddInt32(Realmserver.Revision);
            Response.AddInt32(0); // SessionID?
            Response.AddString(Realmserver.Account.ToUpper());
            Response.AddUInt32(Worldserver.ClientSeed);
            Response.AddByteArray(ShaDigest);
            Response.AddInt32(0); // Addon size
            Worldserver.Send(Response);
            Response.Dispose();
            Worldserver.Encoding = true;
        }

        public static void On_SMSG_AUTH_RESPONSE(ref Packets.PacketClass Packet)
        {
            Console.WriteLine("[{0}][World] Received Auth Response.", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"));
            byte ErrorCode = Packet.GetInt8();
            switch (ErrorCode)
            {
                case 0xC:
                    {
                        Console.WriteLine("[{0}][World] Auth succeeded.", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"));
                        Packets.PacketClass Response = new(OPCODES.CMSG_CHAR_ENUM);
                        Worldserver.Send(Response);
                        Response.Dispose();
                        break;
                    }

                case 0x15:
                    {
                        Console.WriteLine("[{0}][World] Auth Challenge failed.", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"));
                        Worldserver.Disconnect();
                        break;
                    }

                default:
                    {
                        Console.WriteLine("[{0}][World] Unknown Auth Response error [{1}].", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"), ErrorCode);
                        Worldserver.Disconnect();
                        break;
                    }
            }
        }

        public static void On_SMSG_CHAR_ENUM(ref Packets.PacketClass Packet)
        {
            Console.WriteLine("[{0}][World] Received Character List.", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"));
            byte NumChars = Packet.GetInt8();
            if (NumChars > 0)
            {
                for (byte i = 1, loopTo = NumChars; i <= loopTo; i++)
                {
                    ulong GUID = Packet.GetUInt64();
                    string Name = Packet.GetString();
                    byte Race = Packet.GetInt8();
                    byte Classe = Packet.GetInt8();
                    byte Gender = Packet.GetInt8();
                    byte Skin = Packet.GetInt8();
                    byte Face = Packet.GetInt8();
                    byte HairStyle = Packet.GetInt8();
                    byte HairColor = Packet.GetInt8();
                    byte FacialHair = Packet.GetInt8();
                    byte Level = Packet.GetInt8();
                    int Zone = Packet.GetInt32();
                    int Map = Packet.GetInt32();
                    float PosX = Packet.GetFloat();
                    float PosY = Packet.GetFloat();
                    float PosZ = Packet.GetFloat();
                    uint GuildID = Packet.GetUInt32();
                    uint PlayerState = Packet.GetUInt32();
                    byte RestState = Packet.GetInt8();
                    uint PetInfoID = Packet.GetUInt32();
                    uint PetLevel = Packet.GetUInt32();
                    uint PetFamilyID = Packet.GetUInt32();
                    Console.WriteLine("[{0}][World] Logging in with character [{1}].", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"), Name);
                    Worldserver.CharacterGUID = GUID;
                    Packets.PacketClass Response = new(OPCODES.CMSG_PLAYER_LOGIN);
                    Response.AddUInt64(GUID);
                    Worldserver.Send(Response);
                    Response.Dispose();
                    break;

                    // Skip the equipment
                    Packet.Offset += 20 * 9;
                }
            }
            else
            {
                Console.WriteLine("[{0}][World] No characters found.", Strings.Format(DateAndTime.TimeOfDay, "HH:mm:ss"));
            }
        }
    }
}
