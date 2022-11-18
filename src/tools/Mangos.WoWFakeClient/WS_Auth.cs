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
using System;
using System.Security.Cryptography;
using System.Text;

namespace Mangos.WoWFakeClient;

public static class WS_Auth
{
    public static void On_SMSG_PONG(ref Packets.PacketClass Packet)
    {
        var SequenceID = Packet.GetUInt32();
        var Latency = Worldserver.timeGetTime() - Worldserver.PingSent;
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
        var temp = Encoding.ASCII.GetBytes(Realmserver.Account.ToCharArray());
        temp = Realmserver.Concat(temp, BitConverter.GetBytes(0));
        temp = Realmserver.Concat(temp, BitConverter.GetBytes(Worldserver.ClientSeed));
        temp = Realmserver.Concat(temp, BitConverter.GetBytes(Worldserver.ServerSeed));
        temp = Realmserver.Concat(temp, Realmserver.SS_Hash);
        SHA1Managed algorithm1 = new();
        var ShaDigest = algorithm1.ComputeHash(temp);
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
        var ErrorCode = Packet.GetInt8();
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
        var NumChars = Packet.GetInt8();
        if (NumChars > 0)
        {
            for (byte i = 1, loopTo = NumChars; i <= loopTo; i++)
            {
                var GUID = Packet.GetUInt64();
                var Name = Packet.GetString();
                var Race = Packet.GetInt8();
                var Classe = Packet.GetInt8();
                var Gender = Packet.GetInt8();
                var Skin = Packet.GetInt8();
                var Face = Packet.GetInt8();
                var HairStyle = Packet.GetInt8();
                var HairColor = Packet.GetInt8();
                var FacialHair = Packet.GetInt8();
                var Level = Packet.GetInt8();
                var Zone = Packet.GetInt32();
                var Map = Packet.GetInt32();
                var PosX = Packet.GetFloat();
                var PosY = Packet.GetFloat();
                var PosZ = Packet.GetFloat();
                var GuildID = Packet.GetUInt32();
                var PlayerState = Packet.GetUInt32();
                var RestState = Packet.GetInt8();
                var PetInfoID = Packet.GetUInt32();
                var PetLevel = Packet.GetUInt32();
                var PetFamilyID = Packet.GetUInt32();
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
