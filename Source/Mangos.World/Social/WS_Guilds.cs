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
using Mangos.Common.Enums.Guild;
using Mangos.Common.Globals;
using Mangos.Common.Legacy;
using Mangos.World.Globals;
using Mangos.World.Network;
using Mangos.World.Objects;
using Mangos.World.Player;
using Microsoft.VisualBasic.CompilerServices;
using System.Collections.Generic;
using System.Data;

namespace Mangos.World.Social;

public class WS_Guilds
{
    public void SendPetitionActivate(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID)
    {
        if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(cGUID))
        {
            byte Count = 3;
            if (((uint)WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].CreatureInfo.cNpcFlags & 4u) != 0)
            {
                Count = 1;
            }
            Packets.PacketClass packet = new(Opcodes.SMSG_PETITION_SHOWLIST);
            packet.AddUInt64(cGUID);
            packet.AddInt8(1);
            if (Count == 1)
            {
                packet.AddInt32(1);
                packet.AddInt32(WorldServiceLocator._Global_Constants.PETITION_GUILD);
                packet.AddInt32(16161);
                packet.AddInt32(WorldServiceLocator._Global_Constants.PETITION_GUILD_PRICE);
                packet.AddInt32(0);
                packet.AddInt32(9);
            }
            objCharacter.client.Send(ref packet);
            packet.Dispose();
        }
    }

    public void On_CMSG_PETITION_SHOWLIST(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 13)
        {
            packet.GetInt16();
            var GUID = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_PETITION_SHOWLIST [GUID={2:X}]", client.IP, client.Port, GUID);
            SendPetitionActivate(ref client.Character, GUID);
        }
    }

    public void On_CMSG_PETITION_BUY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        checked
        {
            if (packet.Data.Length - 1 < 26)
            {
                return;
            }
            packet.GetInt16();
            var GUID = packet.GetUInt64();
            packet.GetInt64();
            packet.GetInt32();
            var Name = packet.GetString();
            if (packet.Data.Length - 1 < 26 + Name.Length + 40 + 2 + 1 + 4 + 4)
            {
                return;
            }
            packet.GetInt64();
            packet.GetInt64();
            packet.GetInt64();
            packet.GetInt64();
            packet.GetInt64();
            packet.GetInt16();
            packet.GetInt8();
            var Index = packet.GetInt32();
            packet.GetInt32();
            if (!WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(GUID) || (WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].CreatureInfo.cNpcFlags & 0x200) == 0)
            {
                return;
            }
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_PETITION_BUY [GuildName={2}]", client.IP, client.Port, Name);
            if ((ulong)client.Character.GuildID != 0)
            {
                return;
            }
            var CharterID = WorldServiceLocator._Global_Constants.PETITION_GUILD;
            var CharterPrice = WorldServiceLocator._Global_Constants.PETITION_GUILD_PRICE;
            DataTable q = new();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT guild_id FROM guilds WHERE guild_name = '{Name}'", ref q);
            if (q.Rows.Count > 0)
            {
                SendGuildResult(ref client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_NAME_EXISTS, Name);
            }
            q.Clear();
            if (!WorldServiceLocator._Functions.ValidateGuildName(Name))
            {
                SendGuildResult(ref client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_NAME_INVALID, Name);
            }
            if (!WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(CharterID))
            {
                Packets.PacketClass response2 = new(Opcodes.SMSG_BUY_FAILED);
                response2.AddUInt64(GUID);
                response2.AddInt32(CharterID);
                response2.AddInt8(0);
                client.Send(ref response2);
                response2.Dispose();
                return;
            }
            if (client.Character.Copper < CharterPrice)
            {
                Packets.PacketClass response = new(Opcodes.SMSG_BUY_FAILED);
                response.AddUInt64(GUID);
                response.AddInt32(CharterID);
                response.AddInt8(2);
                client.Send(ref response);
                response.Dispose();
                return;
            }
            ref var copper = ref client.Character.Copper;
            copper = (uint)(copper - CharterPrice);
            client.Character.SetUpdateFlag(1176, client.Character.Copper);
            client.Character.SendCharacterUpdate(toNear: false);
            ItemObject tmpItem = new(CharterID, client.Character.GUID)
            {
                StackCount = 1
            };
            tmpItem.AddEnchantment((int)(tmpItem.GUID - WorldServiceLocator._Global_Constants.GUID_ITEM), 0);
            if (client.Character.ItemADD(ref tmpItem))
            {
                WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("INSERT INTO petitions (petition_id, petition_itemGuid, petition_owner, petition_name, petition_type, petition_signedMembers) VALUES ({0}, {0}, {1}, '{2}', {3}, 0);", tmpItem.GUID - WorldServiceLocator._Global_Constants.GUID_ITEM, client.Character.GUID - WorldServiceLocator._Global_Constants.GUID_PLAYER, Name, 9));
            }
            else
            {
                tmpItem.Delete();
            }
        }
    }

    public void SendPetitionSignatures(ref WS_PlayerData.CharacterObject objCharacter, ulong iGUID)
    {
        DataTable MySQLQuery = new();
        checked
        {
            WorldServiceLocator._WorldServer.CharacterDatabase.Query("SELECT * FROM petitions WHERE petition_itemGuid = " + Conversions.ToString(iGUID - WorldServiceLocator._Global_Constants.GUID_ITEM) + ";", ref MySQLQuery);
            if (MySQLQuery.Rows.Count != 0)
            {
                Packets.PacketClass response = new(Opcodes.SMSG_PETITION_SHOW_SIGNATURES);
                response.AddUInt64(iGUID);
                response.AddUInt64(MySQLQuery.Rows[0].As<ulong>("petition_owner"));
                response.AddInt32(MySQLQuery.Rows[0].As<int>("petition_id"));
                response.AddInt8(MySQLQuery.Rows[0].As<byte>("petition_signedMembers"));
                var b = MySQLQuery.Rows[0].As<byte>("petition_signedMembers");
                byte i = 1;
                while (i <= (uint)b)
                {
                    response.AddUInt64(MySQLQuery.Rows[0].As<ulong>("petition_signedMember" + Conversions.ToString(i)));
                    response.AddInt32(0);
                    i = (byte)unchecked((uint)(i + 1));
                }
                objCharacter.client.Send(ref response);
                response.Dispose();
            }
        }
    }

    public void On_CMSG_PETITION_SHOW_SIGNATURES(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 13)
        {
            packet.GetInt16();
            var GUID = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_PETITION_SHOW_SIGNATURES [GUID={2:X}]", client.IP, client.Port, GUID);
            SendPetitionSignatures(ref client.Character, GUID);
        }
    }

    public void On_CMSG_PETITION_QUERY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        checked
        {
            if (packet.Data.Length - 1 < 17)
            {
                return;
            }
            packet.GetInt16();
            var PetitionGUID = packet.GetInt32();
            var itemGuid = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_PETITION_QUERY [pGUID={3} iGUID={2:X}]", client.IP, client.Port, itemGuid, PetitionGUID);
            DataTable MySQLQuery = new();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query("SELECT * FROM petitions WHERE petition_itemGuid = " + Conversions.ToString(itemGuid - WorldServiceLocator._Global_Constants.GUID_ITEM) + ";", ref MySQLQuery);
            if (MySQLQuery.Rows.Count != 0)
            {
                Packets.PacketClass response = new(Opcodes.SMSG_PETITION_QUERY_RESPONSE);
                response.AddInt32(MySQLQuery.Rows[0].As<int>("petition_id"));
                response.AddUInt64(MySQLQuery.Rows[0].As<ulong>("petition_owner"));
                response.AddString(MySQLQuery.Rows[0].As<string>("petition_name"));
                response.AddInt8(0);
                if (MySQLQuery.Rows[0].As<byte>("petition_type") == 9)
                {
                    response.AddInt32(9);
                    response.AddInt32(9);
                    response.AddInt32(0);
                }
                else
                {
                    response.AddInt32(MySQLQuery.Rows[0].As<byte>("petition_type") - 1);
                    response.AddInt32(MySQLQuery.Rows[0].As<byte>("petition_type") - 1);
                    response.AddInt32(MySQLQuery.Rows[0].As<byte>("petition_type"));
                }
                response.AddInt32(0);
                response.AddInt32(0);
                response.AddInt32(0);
                response.AddInt32(0);
                response.AddInt16(0);
                response.AddInt32(0);
                response.AddInt32(0);
                response.AddInt32(0);
                response.AddInt32(0);
                if (MySQLQuery.Rows[0].As<byte>("petition_type") == 9)
                {
                    response.AddInt32(0);
                }
                else
                {
                    response.AddInt32(1);
                }
                client.Send(ref response);
                response.Dispose();
            }
        }
    }

    public void On_MSG_PETITION_RENAME(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        checked
        {
            if (packet.Data.Length - 1 >= 14)
            {
                packet.GetInt16();
                var itemGuid = packet.GetUInt64();
                var NewName = packet.GetString();
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_PETITION_RENAME [NewName={3} GUID={2:X}]", client.IP, client.Port, itemGuid, NewName);
                WorldServiceLocator._WorldServer.CharacterDatabase.Update("UPDATE petitions SET petition_name = '" + NewName + "' WHERE petition_itemGuid = " + Conversions.ToString(itemGuid - WorldServiceLocator._Global_Constants.GUID_ITEM) + ";");
                Packets.PacketClass response = new(Opcodes.MSG_PETITION_RENAME);
                response.AddUInt64(itemGuid);
                response.AddString(NewName);
                response.AddInt32((int)(itemGuid - WorldServiceLocator._Global_Constants.GUID_ITEM));
                client.Send(ref response);
                response.Dispose();
            }
        }
    }

    public void On_CMSG_OFFER_PETITION(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 21)
        {
            packet.GetInt16();
            var PetitionType = packet.GetInt32();
            var itemGuid = packet.GetUInt64();
            var GUID = packet.GetUInt64();
            if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(GUID) && WorldServiceLocator._WorldServer.CHARACTERs[GUID].IsHorde == client.Character.IsHorde)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_OFFER_PETITION [GUID={2:X} Petition={3}]", client.IP, client.Port, GUID, itemGuid);
                Dictionary<ulong, WS_PlayerData.CharacterObject> cHARACTERs;
                ulong key;
                var objCharacter = (cHARACTERs = WorldServiceLocator._WorldServer.CHARACTERs)[key = GUID];
                SendPetitionSignatures(ref objCharacter, itemGuid);
                cHARACTERs[key] = objCharacter;
            }
        }
    }

    public void On_CMSG_PETITION_SIGN(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        checked
        {
            if (packet.Data.Length - 1 < 14)
            {
                return;
            }
            packet.GetInt16();
            var itemGuid = packet.GetUInt64();
            int Unk = packet.GetInt8();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_PETITION_SIGN [GUID={2:X} Unk={3}]", client.IP, client.Port, itemGuid, Unk);
            DataTable MySQLQuery = new();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query("SELECT petition_signedMembers, petition_owner FROM petitions WHERE petition_itemGuid = " + Conversions.ToString(itemGuid - WorldServiceLocator._Global_Constants.GUID_ITEM) + ";", ref MySQLQuery);
            if (MySQLQuery.Rows.Count != 0)
            {
                WorldServiceLocator._WorldServer.CharacterDatabase.Update(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("UPDATE petitions SET petition_signedMembers = petition_signedMembers + 1, petition_signedMember", Operators.AddObject(MySQLQuery.Rows[0]["petition_signedMembers"], 1)), " = "), client.Character.GUID), " WHERE petition_itemGuid = "), itemGuid - WorldServiceLocator._Global_Constants.GUID_ITEM), ";")));
                Packets.PacketClass response = new(Opcodes.SMSG_PETITION_SIGN_RESULTS);
                response.AddUInt64(itemGuid);
                response.AddUInt64(client.Character.GUID);
                response.AddInt32(0);
                client.SendMultiplyPackets(ref response);
                if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(MySQLQuery.Rows[0].As<ulong>("petition_owner")))
                {
                    WorldServiceLocator._WorldServer.CHARACTERs[MySQLQuery.Rows[0].As<ulong>("petition_owner")].client.SendMultiplyPackets(ref response);
                }
                response.Dispose();
            }
        }
    }

    public void On_MSG_PETITION_DECLINE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        checked
        {
            if (packet.Data.Length - 1 >= 13)
            {
                packet.GetInt16();
                var itemGuid = packet.GetUInt64();
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_PETITION_DECLINE [GUID={2:X}]", client.IP, client.Port, itemGuid);
                DataTable q = new();
                WorldServiceLocator._WorldServer.CharacterDatabase.Query("SELECT petition_owner FROM petitions WHERE petition_itemGuid = " + Conversions.ToString(itemGuid - WorldServiceLocator._Global_Constants.GUID_ITEM) + " LIMIT 1;", ref q);
                Packets.PacketClass response = new(Opcodes.MSG_PETITION_DECLINE);
                response.AddUInt64(client.Character.GUID);
                if (q.Rows.Count > 0 && WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(q.Rows[0].As<ulong>("petition_owner")))
                {
                    WorldServiceLocator._WorldServer.CHARACTERs[q.Rows[0].As<ulong>("petition_owner")].client.SendMultiplyPackets(ref response);
                }
                response.Dispose();
            }
        }
    }

    public void On_CMSG_TURN_IN_PETITION(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 13)
        {
            packet.GetInt16();
            var itemGuid = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_TURN_IN_PETITION [GUID={2:X}]", client.IP, client.Port, itemGuid);
            client.Character.ItemREMOVE(itemGuid, Destroy: true, Update: true);
        }
    }

    public void SendTabardActivate(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID)
    {
        Packets.PacketClass packet = new(Opcodes.MSG_TABARDVENDOR_ACTIVATE);
        packet.AddUInt64(cGUID);
        objCharacter.client.Send(ref packet);
        packet.Dispose();
    }

    public void On_MSG_TABARDVENDOR_ACTIVATE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 13)
        {
            packet.GetInt16();
            var GUID = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_TABARDVENDOR_ACTIVATE [GUID={2}]", client.IP, client.Port, GUID);
            SendTabardActivate(ref client.Character, GUID);
        }
    }

    public int GetGuildBankTabPrice(byte TabID)
    {
        return TabID switch
        {
            0 => 100,
            1 => 250,
            2 => 500,
            3 => 1000,
            4 => 2500,
            5 => 5000,
            _ => 0,
        };
    }

    public void SendGuildResult(ref WS_Network.ClientClass client, GuildCommand Command, GuildError Result, string Text = "")
    {
        Packets.PacketClass response = new(Opcodes.SMSG_GUILD_COMMAND_RESULT);
        response.AddInt32((int)Command);
        response.AddString(Text);
        response.AddInt32((int)Result);
        client.Send(ref response);
        response.Dispose();
    }
}
