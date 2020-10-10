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

using System.Data;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Guild;
using Mangos.Common.Enums.Misc;
using Mangos.Common.Enums.Unit;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Objects;
using Mangos.World.Player;
using Mangos.World.Server;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Social
{
    public class WS_Guilds
    {

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public void SendPetitionActivate(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID)
        {
            if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(cGUID) == false)
                return;
            byte Count = 3;
            if (WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].CreatureInfo.cNpcFlags & NPCFlags.UNIT_NPC_FLAG_VENDOR)
            {
                Count = 1;
            }

            var packet = new Packets.PacketClass(OPCODES.SMSG_PETITION_SHOWLIST);
            packet.AddUInt64(cGUID);
            packet.AddInt8(1);
            if (Count == 1)
            {
                packet.AddInt32(1); // Index
                packet.AddInt32(WorldServiceLocator._Global_Constants.PETITION_GUILD);
                packet.AddInt32(16161); // Charter display ID
                packet.AddInt32(WorldServiceLocator._Global_Constants.PETITION_GUILD_PRICE);
                packet.AddInt32(0); // Unknown
                packet.AddInt32(9); // Required signatures
            }

            objCharacter.client.Send(packet);
            packet.Dispose();
        }

        public void On_CMSG_PETITION_SHOWLIST(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 13)
                return;
            packet.GetInt16();
            ulong GUID = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_PETITION_SHOWLIST [GUID={2:X}]", client.IP, client.Port, GUID);
            SendPetitionActivate(ref client.Character, GUID);
        }

        public void On_CMSG_PETITION_BUY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 26)
                return;
            packet.GetInt16();
            ulong GUID = packet.GetUInt64();
            packet.GetInt64();
            packet.GetInt32();
            string Name = packet.GetString();
            if (packet.Data.Length - 1 < 26 + Name.Length + 5 * 8 + 2 + 1 + 4 + 4)
                return;
            packet.GetInt64();
            packet.GetInt64();
            packet.GetInt64();
            packet.GetInt64();
            packet.GetInt64();
            packet.GetInt16();
            packet.GetInt8();
            int Index = packet.GetInt32();
            packet.GetInt32();
            if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(GUID) == false || (WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].CreatureInfo.cNpcFlags & NPCFlags.UNIT_NPC_FLAG_PETITIONER) == 0)
                return;
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_PETITION_BUY [GuildName={2}]", client.IP, client.Port, Name);
            if (client.Character.GuildID != 0L)
                return;
            int CharterID = WorldServiceLocator._Global_Constants.PETITION_GUILD;
            int CharterPrice = WorldServiceLocator._Global_Constants.PETITION_GUILD_PRICE;
            var q = new DataTable();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT guild_id FROM guilds WHERE guild_name = '{0}'", Name), q);
            if (q.Rows.Count > 0)
            {
                SendGuildResult(ref client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_NAME_EXISTS, Name);
            }

            q.Clear();
            if (WorldServiceLocator._Functions.ValidateGuildName(Name) == false)
            {
                SendGuildResult(ref client, GuildCommand.GUILD_CREATE_S, GuildError.GUILD_NAME_INVALID, Name);
            }

            if (WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(CharterID) == false)
            {
                var response = new Packets.PacketClass(OPCODES.SMSG_BUY_FAILED);
                response.AddUInt64(GUID);
                response.AddInt32(CharterID);
                response.AddInt8((byte)BUY_ERROR.BUY_ERR_CANT_FIND_ITEM);
                client.Send(response);
                response.Dispose();
                return;
            }

            if (client.Character.Copper < CharterPrice)
            {
                var response = new Packets.PacketClass(OPCODES.SMSG_BUY_FAILED);
                response.AddUInt64(GUID);
                response.AddInt32(CharterID);
                response.AddInt8((byte)BUY_ERROR.BUY_ERR_NOT_ENOUGHT_MONEY);
                client.Send(response);
                response.Dispose();
                return;
            }

            client.Character.Copper = (uint)(client.Character.Copper - CharterPrice);
            client.Character.SetUpdateFlag((int)EPlayerFields.PLAYER_FIELD_COINAGE, client.Character.Copper);
            client.Character.SendCharacterUpdate(false);

            // Client.Character.AddItem(PETITION_GUILD)
            var tmpItem = new ItemObject(CharterID, client.Character.GUID) { StackCount = 1 };
            tmpItem.AddEnchantment(tmpItem.GUID - WorldServiceLocator._Global_Constants.GUID_ITEM, 0, 0, 0);
            if (client.Character.ItemADD(ref tmpItem))
            {
                // Save petition into database
                WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("INSERT INTO petitions (petition_id, petition_itemGuid, petition_owner, petition_name, petition_type, petition_signedMembers) VALUES ({0}, {0}, {1}, '{2}', {3}, 0);", tmpItem.GUID - WorldServiceLocator._Global_Constants.GUID_ITEM, client.Character.GUID - WorldServiceLocator._Global_Constants.GUID_PLAYER, Name, 9));
            }
            else
            {
                // No free inventory slot
                tmpItem.Delete();
            }
        }

        public void SendPetitionSignatures(ref WS_PlayerData.CharacterObject objCharacter, ulong iGUID)
        {
            var MySQLQuery = new DataTable();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query("SELECT * FROM petitions WHERE petition_itemGuid = " + (iGUID - WorldServiceLocator._Global_Constants.GUID_ITEM) + ";", MySQLQuery);
            if (MySQLQuery.Rows.Count == 0)
                return;
            var response = new Packets.PacketClass(OPCODES.SMSG_PETITION_SHOW_SIGNATURES);
            response.AddUInt64(iGUID);                                                        // itemGuid
            response.AddUInt64(Conversions.ToULong(MySQLQuery.Rows[0]["petition_owner"]));                    // GuildOwner
            response.AddInt32(Conversions.ToInteger(MySQLQuery.Rows[0]["petition_id"]));                        // PetitionGUID
            response.AddInt8(Conversions.ToByte(MySQLQuery.Rows[0]["petition_signedMembers"]));              // PlayersSigned
            for (byte i = 1, loopTo = Conversions.ToByte(MySQLQuery.Rows[0]["petition_signedMembers"]); i <= loopTo; i++)
            {
                response.AddUInt64(Conversions.ToULong(MySQLQuery.Rows[0]["petition_signedMember" + i]));     // SignedGUID
                response.AddInt32(0);                                                         // Unk
            }

            objCharacter.client.Send(response);
            response.Dispose();
        }

        public void On_CMSG_PETITION_SHOW_SIGNATURES(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 13)
                return;
            packet.GetInt16();
            ulong GUID = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_PETITION_SHOW_SIGNATURES [GUID={2:X}]", client.IP, client.Port, GUID);
            SendPetitionSignatures(ref client.Character, GUID);
        }

        public void On_CMSG_PETITION_QUERY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 17)
                return;
            packet.GetInt16();
            int PetitionGUID = packet.GetInt32();
            ulong itemGuid = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_PETITION_QUERY [pGUID={3} iGUID={2:X}]", client.IP, client.Port, itemGuid, PetitionGUID);
            var MySQLQuery = new DataTable();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query("SELECT * FROM petitions WHERE petition_itemGuid = " + (itemGuid - WorldServiceLocator._Global_Constants.GUID_ITEM) + ";", MySQLQuery);
            if (MySQLQuery.Rows.Count == 0)
                return;
            var response = new Packets.PacketClass(OPCODES.SMSG_PETITION_QUERY_RESPONSE);
            response.AddInt32(Conversions.ToInteger(MySQLQuery.Rows[0]["petition_id"]));               // PetitionGUID
            response.AddUInt64(Conversions.ToULong(MySQLQuery.Rows[0]["petition_owner"]));           // GuildOwner
            response.AddString(Conversions.ToString(MySQLQuery.Rows[0]["petition_name"]));            // GuildName
            response.AddInt8(0);         // Unk1
            if (Conversions.ToByte(MySQLQuery.Rows[0]["petition_type"]) == 9)
            {
                response.AddInt32(9);
                response.AddInt32(9);
                response.AddInt32(0); // bypass client - side limitation, a different value is needed here for each petition
            }
            else
            {
                response.AddInt32(Conversions.ToByte(MySQLQuery.Rows[0]["petition_type"]) - 1);
                response.AddInt32(Conversions.ToByte(MySQLQuery.Rows[0]["petition_type"]) - 1);
                response.AddInt32(Conversions.ToByte(MySQLQuery.Rows[0]["petition_type"]));
            } // bypass client - side limitation, a different value is needed here for each petition
            // 9x int32
            response.AddInt32(0);
            response.AddInt32(0);
            response.AddInt32(0);
            response.AddInt32(0);
            response.AddInt16(0);
            response.AddInt32(0);
            response.AddInt32(0);
            response.AddInt32(0);
            response.AddInt32(0);
            if (Conversions.ToByte(MySQLQuery.Rows[0]["petition_type"]) == 9)
            {
                response.AddInt32(0);
            }
            else
            {
                response.AddInt32(1);
            }

            client.Send(response);
            response.Dispose();
        }

        public void On_MSG_PETITION_RENAME(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 14)
                return;
            packet.GetInt16();
            ulong itemGuid = packet.GetUInt64();
            string NewName = packet.GetString();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_PETITION_RENAME [NewName={3} GUID={2:X}]", client.IP, client.Port, itemGuid, NewName);
            WorldServiceLocator._WorldServer.CharacterDatabase.Update("UPDATE petitions SET petition_name = '" + NewName + "' WHERE petition_itemGuid = " + (itemGuid - WorldServiceLocator._Global_Constants.GUID_ITEM) + ";");

            // DONE: Update client-side name information
            var response = new Packets.PacketClass(OPCODES.MSG_PETITION_RENAME);
            response.AddUInt64(itemGuid);
            response.AddString(NewName);
            response.AddInt32(itemGuid - WorldServiceLocator._Global_Constants.GUID_ITEM);
            client.Send(response);
            response.Dispose();
        }

        public void On_CMSG_OFFER_PETITION(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 21)
                return;
            packet.GetInt16();
            int PetitionType = packet.GetInt32();
            ulong itemGuid = packet.GetUInt64();
            ulong GUID = packet.GetUInt64();
            if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(GUID) == false)
                return;
            // If _WorldServer.CHARACTERs(GUID).IgnoreList.Contains(Client.Character.GUID) Then Exit Sub
            if (WorldServiceLocator._WorldServer.CHARACTERs[GUID].IsHorde != client.Character.IsHorde)
                return;
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_OFFER_PETITION [GUID={2:X} Petition={3}]", client.IP, client.Port, GUID, itemGuid);
            var tmp = WorldServiceLocator._WorldServer.CHARACTERs;
            var argobjCharacter = tmp[GUID];
            SendPetitionSignatures(ref argobjCharacter, itemGuid);
            tmp[GUID] = argobjCharacter;
        }

        public void On_CMSG_PETITION_SIGN(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 14)
                return;
            packet.GetInt16();
            ulong itemGuid = packet.GetUInt64();
            int Unk = packet.GetInt8();

            // TODO: Check if the player already has signed

            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_PETITION_SIGN [GUID={2:X} Unk={3}]", client.IP, client.Port, itemGuid, Unk);
            var MySQLQuery = new DataTable();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query("SELECT petition_signedMembers, petition_owner FROM petitions WHERE petition_itemGuid = " + (itemGuid - WorldServiceLocator._Global_Constants.GUID_ITEM) + ";", MySQLQuery);
            if (MySQLQuery.Rows.Count == 0)
                return;
            WorldServiceLocator._WorldServer.CharacterDatabase.Update(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("UPDATE petitions SET petition_signedMembers = petition_signedMembers + 1, petition_signedMember", Operators.AddObject(MySQLQuery.Rows[0]["petition_signedMembers"], 1)), " = "), client.Character.GUID), " WHERE petition_itemGuid = ")) + (itemGuid - WorldServiceLocator._Global_Constants.GUID_ITEM) + ";");

            // DONE: Send result to both players
            var response = new Packets.PacketClass(OPCODES.SMSG_PETITION_SIGN_RESULTS);
            response.AddUInt64(itemGuid);
            response.AddUInt64(client.Character.GUID);
            response.AddInt32((int)PetitionSignError.PETITIONSIGN_OK);
            client.SendMultiplyPackets(ref response);
            if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(Conversions.ToULong(MySQLQuery.Rows[0]["petition_owner"])))
                WorldServiceLocator._WorldServer.CHARACTERs[Conversions.ToULong(MySQLQuery.Rows[0]["petition_owner"])].client.SendMultiplyPackets(ref response);
            response.Dispose();
        }

        public void On_MSG_PETITION_DECLINE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 13)
                return;
            packet.GetInt16();
            ulong itemGuid = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_PETITION_DECLINE [GUID={2:X}]", client.IP, client.Port, itemGuid);

            // DONE: Get petition owner
            var q = new DataTable();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query("SELECT petition_owner FROM petitions WHERE petition_itemGuid = " + (itemGuid - WorldServiceLocator._Global_Constants.GUID_ITEM) + " LIMIT 1;", q);

            // DONE: Send message to player
            var response = new Packets.PacketClass(OPCODES.MSG_PETITION_DECLINE);
            response.AddUInt64(client.Character.GUID);
            if (q.Rows.Count > 0 && WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(Conversions.ToULong(q.Rows[0]["petition_owner"])))
                WorldServiceLocator._WorldServer.CHARACTERs[Conversions.ToULong(q.Rows[0]["petition_owner"])].client.SendMultiplyPackets(ref response);
            response.Dispose();
        }

        public void On_CMSG_TURN_IN_PETITION(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 13)
                return;
            packet.GetInt16();
            ulong itemGuid = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_TURN_IN_PETITION [GUID={2:X}]", client.IP, client.Port, itemGuid);
            client.Character.ItemREMOVE(itemGuid, true, true);
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        // Basic Tabard Framework
        public void SendTabardActivate(ref WS_PlayerData.CharacterObject objCharacter, ulong cGUID)
        {
            var packet = new Packets.PacketClass(OPCODES.MSG_TABARDVENDOR_ACTIVATE);
            packet.AddUInt64(cGUID);
            objCharacter.client.Send(packet);
            packet.Dispose();
        }

        public void On_MSG_TABARDVENDOR_ACTIVATE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 13)
                return;
            packet.GetInt16();
            ulong GUID = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_TABARDVENDOR_ACTIVATE [GUID={2}]", client.IP, client.Port, GUID);
            SendTabardActivate(ref client.Character, GUID);
        }

        public int GetGuildBankTabPrice(byte TabID)
        {
            switch (TabID)
            {
                case 0:
                    {
                        return 100;
                    }

                case 1:
                    {
                        return 250;
                    }

                case 2:
                    {
                        return 500;
                    }

                case 3:
                    {
                        return 1000;
                    }

                case 4:
                    {
                        return 2500;
                    }

                case 5:
                    {
                        return 5000;
                    }

                default:
                    {
                        return 0;
                    }
            }
        }

        public void SendGuildResult(ref WS_Network.ClientClass client, GuildCommand Command, GuildError Result, string Text = "")
        {
            var response = new Packets.PacketClass(OPCODES.SMSG_GUILD_COMMAND_RESULT);
            response.AddInt32((int)Command);
            response.AddString(Text);
            response.AddInt32((int)Result);
            client.Send(response);
            response.Dispose();
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
    }
}