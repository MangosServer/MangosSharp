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
using System.Data;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Misc;
using Mangos.Common.Enums.Social;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Objects;
using Mangos.World.Server;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Social
{
    public class WS_Mail
    {

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public const int ITEM_MAILTEXT_ITEMID = 889;

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public void On_CMSG_MAIL_RETURN_TO_SENDER(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 17)
                return;
            packet.GetInt16();
            ulong GameObjectGUID = packet.GetUInt64();
            int MailID = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MAIL_RETURN_TO_SENDER [MailID={2}]", client.IP, client.Port, MailID);

            // A = 1
            // B = 2
            // A = A + B '3
            // B = A - B '3-2=1
            // A = A - B '3-1=2

            int MailTime = WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now) + TimeConstant.DAY * 30; // Set expiredate to today + 30 days
            WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_mail SET mail_time = {1}, mail_read = 0, mail_receiver = (mail_receiver + mail_sender), mail_sender = (mail_receiver - mail_sender), mail_receiver = (mail_receiver - mail_sender) WHERE mail_id = {0};", MailID, MailTime));
            var response = new Packets.PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT);
            response.AddInt32(MailID);
            response.AddInt32((int)MailResult.MAIL_RETURNED);
            response.AddInt32(0);
            client.Send(response);
            response.Dispose();
        }

        public void On_CMSG_MAIL_DELETE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 17)
                return;
            packet.GetInt16();
            ulong GameObjectGUID = packet.GetUInt64();
            int MailID = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MAIL_DELETE [MailID={2}]", client.IP, client.Port, MailID);
            WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("DELETE FROM characters_mail WHERE mail_id = {0};", MailID));
            var response = new Packets.PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT);
            response.AddInt32(MailID);
            response.AddInt32((int)MailResult.MAIL_DELETED);
            response.AddInt32(0);
            client.Send(response);
            response.Dispose();
        }

        public void On_CMSG_MAIL_MARK_AS_READ(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 17)
                return;
            packet.GetInt16();
            ulong GameObjectGUID = packet.GetUInt64();
            int MailID = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MAIL_MARK_AS_READ [MailID={2}]", client.IP, client.Port, MailID);
            int MailTime = WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now) + TimeConstant.DAY * 3; // Set expiredate to today + 3 days
            WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_mail SET mail_read = 1, mail_time = {1} WHERE mail_id = {0} AND mail_read < 2;", MailID, MailTime));
        }

        public void On_MSG_QUERY_NEXT_MAIL_TIME(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_QUERY_NEXT_MAIL_TIME", client.IP, client.Port);
            var MySQLQuery = new DataTable();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT COUNT(*) FROM characters_mail WHERE mail_read = 0 AND mail_receiver = {0} AND mail_time > {1};", (object)client.Character.GUID, (object)WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now)), MySQLQuery);
            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectGreater(MySQLQuery.Rows[0][0], 0, false)))
            {
                var response = new Packets.PacketClass(OPCODES.MSG_QUERY_NEXT_MAIL_TIME);
                response.AddInt32(0);
                client.Send(response);
                response.Dispose();
            }
            else
            {
                var response = new Packets.PacketClass(OPCODES.MSG_QUERY_NEXT_MAIL_TIME);
                response.AddInt8(0);
                response.AddInt8(0xC0);
                response.AddInt8(0xA8);
                response.AddInt8(0xC7);
                client.Send(response);
                response.Dispose();
            }
        }

        public void On_CMSG_GET_MAIL_LIST(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 13)
                return;
            packet.GetInt16();
            ulong GameObjectGUID = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GET_MAIL_LIST [GUID={2:X}]", client.IP, client.Port, GameObjectGUID);
            try
            {
                // Done: Check for old mails, and delete those that have expired
                var MySQLQuery = new DataTable();
                WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT mail_id FROM characters_mail WHERE mail_time < {0};", (object)WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now)), MySQLQuery);
                if (MySQLQuery.Rows.Count > 0)
                {
                    for (byte i = 0, loopTo = (byte)(MySQLQuery.Rows.Count - 1); i <= loopTo; i++)
                        WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("DELETE FROM characters_mail WHERE mail_id = {0};", MySQLQuery.Rows[i]["mail_id"]));
                }

                WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT * FROM characters_mail WHERE mail_receiver = {0};", (object)client.Character.GUID), MySQLQuery);
                var response = new Packets.PacketClass(OPCODES.SMSG_MAIL_LIST_RESULT);
                response.AddInt8((byte)MySQLQuery.Rows.Count);
                ItemObject tmpItem;
                if (MySQLQuery.Rows.Count > 0)
                {
                    for (byte i = 0, loopTo1 = (byte)(MySQLQuery.Rows.Count - 1); i <= loopTo1; i++)
                    {
                        response.AddInt32(Conversions.ToInteger(MySQLQuery.Rows[i]["mail_id"]));
                        response.AddInt8(Conversions.ToByte(MySQLQuery.Rows[i]["mail_type"]));
                        switch (MySQLQuery.Rows[i]["mail_type"])
                        {
                            case var @case when Operators.ConditionalCompareObjectEqual(@case, MailTypeInfo.NORMAL, false):
                                {
                                    response.AddUInt64(Conversions.ToULong(MySQLQuery.Rows[i]["mail_sender"]));
                                    break;
                                }

                            case var case1 when Operators.ConditionalCompareObjectEqual(case1, MailTypeInfo.AUCTION, false):
                                {
                                    response.AddInt32(Conversions.ToInteger(MySQLQuery.Rows[i]["mail_sender"])); // creature/gameobject entry, auction id
                                    break;
                                }
                        }

                        response.AddString(Conversions.ToString(MySQLQuery.Rows[i]["mail_subject"]));
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(MySQLQuery.Rows[i]["mail_body"], "", false)))
                        {
                            response.AddInt32(Conversions.ToInteger(MySQLQuery.Rows[i]["mail_id"])); // ItemtextID?
                        }
                        else
                        {
                            response.AddInt32(0);
                        }

                        response.AddInt32(0); // 2  = Gift
                        response.AddInt32(Conversions.ToInteger(MySQLQuery.Rows[i]["mail_stationary"])); // 41/62 = Mail Background
                        if (Conversions.ToULong(MySQLQuery.Rows[i]["item_guid"]) > 0m)
                        {
                            tmpItem = WorldServiceLocator._WS_Items.LoadItemByGUID(Conversions.ToULong(MySQLQuery.Rows[i]["item_guid"]));
                            response.AddInt32(tmpItem.ItemEntry);
                            if (tmpItem.Enchantments.ContainsKey((byte)EnchantSlots.ENCHANTMENT_PERM))
                            {
                                packet.AddInt32(tmpItem.Enchantments(EnchantSlots.ENCHANTMENT_PERM).ID);
                            }
                            else
                            {
                                packet.AddInt32(0);
                            }                                      // Permanent enchant

                            response.AddInt32(tmpItem.RandomProperties);                 // Item random property
                            response.AddInt32(0);                                        // Item suffix factor
                            response.AddInt8((byte)tmpItem.StackCount);                        // Item count
                            response.AddInt32(tmpItem.ChargesLeft);                      // Spell Charges
                            response.AddInt32(tmpItem.ItemInfo.Durability);              // Durability Max
                            response.AddInt32(tmpItem.Durability);                       // Durability Min
                        }
                        else
                        {
                            response.AddInt32(0);
                            response.AddInt32(0);
                            response.AddInt32(0);
                            response.AddInt32(0);
                            response.AddInt8(0);
                            response.AddInt32(0);
                            response.AddInt32(0);
                            response.AddInt32(0);
                        }

                        response.AddUInt32(Conversions.ToUInteger(MySQLQuery.Rows[i]["mail_money"]));    // Money on delivery
                        response.AddUInt32(Conversions.ToUInteger(MySQLQuery.Rows[i]["mail_COD"]));      // Money as COD
                        response.AddInt32(Conversions.ToInteger(MySQLQuery.Rows[i]["mail_read"]));
                        response.AddSingle((Conversions.ToUInteger(MySQLQuery.Rows[(int)i]["mail_time"]) - WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now)) / TimeConstant.DAY);
                        response.AddInt32(0); // Mail template ID
                    }
                }

                client.Send(response);
                response.Dispose();
            }
            catch (Exception e)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error getting mail list: {0}{1}", Environment.NewLine, e.ToString());
            }
        }

        public void On_CMSG_MAIL_TAKE_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 17)
                return;
            packet.GetInt16();
            ulong GameObjectGUID = packet.GetUInt64();
            int MailID = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MAIL_TAKE_ITEM [MailID={2}]", client.IP, client.Port, MailID);
            try
            {
                // DONE: Check if it's the receiver that is trying to get the item
                var MySQLQuery = new DataTable();
                WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT mail_cod, mail_sender, item_guid FROM characters_mail WHERE mail_id = {0} AND mail_receiver = {1};", (object)MailID, (object)client.Character.GUID), MySQLQuery);
                if (MySQLQuery.Rows.Count == 0) // The mail didn't exit, wrong owner trying to get someone elses item?
                {
                    var response = new Packets.PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT);
                    response.AddInt32(MailID);
                    response.AddInt32((int)MailResult.MAIL_ITEM_REMOVED);
                    response.AddInt32((int)MailSentError.INTERNAL_ERROR);
                    client.Send(response);
                    response.Dispose();
                    return;
                }

                // DONE: Check for COD
                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(MySQLQuery.Rows[0]["mail_cod"], 0, false)))
                {
                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectLess(client.Character.Copper, MySQLQuery.Rows[0]["mail_cod"], false)))
                    {
                        var noMoney = new Packets.PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT);
                        noMoney.AddInt32(MailID);
                        noMoney.AddInt32((int)MailResult.MAIL_SENT);
                        noMoney.AddInt32((int)MailSentError.NOT_ENOUGHT_MONEY);
                        client.Send(noMoney);
                        noMoney.Dispose();
                        return;
                    }
                    else
                    {
                        // DONE: Pay COD and save
                        client.Character.Copper = Conversions.ToUInteger(client.Character.Copper - MySQLQuery.Rows[0]["mail_cod"]);
                        WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_mail SET mail_cod = 0 WHERE mail_id = {0};", MailID));

                        // DONE: Send COD to sender
                        // TODO: Edit text to be more blizzlike
                        int MailTime = WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now) + TimeConstant.DAY * 30; // Set expiredate to today + 30 days
                        WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format(@"INSERT INTO characters_mail (mail_sender, mail_receiver, mail_subject, mail_body, mail_item_guid, mail_money, mail_COD, mail_time, mail_read, mail_type) VALUES 
                        ({0},{1},'{2}','{3}',{4},{5},{6},{7},{8},{9});", client.Character.GUID, MySQLQuery.Rows[0]["mail_sender"], "", "", 0, MySQLQuery.Rows[0]["mail_cod"], 0, MailTime, MailReadInfo.COD, 0));
                    }
                }

                // DONE: Get Item
                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(MySQLQuery.Rows[0]["item_guid"], 0, false))) // The item doesn't exist?
                {
                    var response = new Packets.PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT);
                    response.AddInt32(MailID);
                    response.AddInt32((int)MailResult.MAIL_ITEM_REMOVED);
                    response.AddInt32((int)MailSentError.INTERNAL_ERROR);
                    client.Send(response);
                    response.Dispose();
                    return;
                }

                var tmpItem = WorldServiceLocator._WS_Items.LoadItemByGUID(Conversions.ToULong(MySQLQuery.Rows[0]["item_guid"]));
                tmpItem.OwnerGUID = client.Character.GUID;
                tmpItem.Save();

                // DONE: Send error message if no slots
                if (client.Character.ItemADD(ref tmpItem))
                {
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_mail SET item_guid = 0 WHERE mail_id = {0};", MailID));
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("DELETE FROM mail_items WHERE mail_id = {0};", MailID));
                    var response = new Packets.PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT);
                    response.AddInt32(MailID);
                    response.AddInt32((int)MailResult.MAIL_ITEM_REMOVED);
                    response.AddInt32((int)MailSentError.NO_ERROR);
                    client.Send(response);
                    response.Dispose();
                }
                else
                {
                    tmpItem.Dispose();
                    var response = new Packets.PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT);
                    response.AddInt32(MailID);
                    response.AddInt32((int)MailResult.MAIL_ITEM_REMOVED);
                    response.AddInt32((int)MailSentError.BAG_FULL);
                    client.Send(response);
                    response.Dispose();
                }

                client.Character.Save();
            }
            catch (Exception e)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error getting item from mail: {0}{1}", Environment.NewLine, e.ToString());
            }
        }

        public void On_CMSG_MAIL_TAKE_MONEY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 17)
                return;
            packet.GetInt16();
            ulong GameObjectGUID = packet.GetUInt64();
            int MailID = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MAIL_TAKE_MONEY [MailID={2}]", client.IP, client.Port, MailID);
            var MySQLQuery = new DataTable();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT mail_money FROM characters_mail WHERE mail_id = {0}; UPDATE characters_mail SET mail_money = 0 WHERE mail_id = {0};", (object)MailID), MySQLQuery);
            if (client.Character.Copper + Conversions.ToLong(MySQLQuery.Rows[0]["mail_money"]) > uint.MaxValue)
            {
                client.Character.Copper = uint.MaxValue;
            }
            else
            {
                client.Character.Copper = Conversions.ToUInteger(client.Character.Copper + MySQLQuery.Rows[0]["mail_money"]);
            }

            client.Character.SetUpdateFlag((int)EPlayerFields.PLAYER_FIELD_COINAGE, client.Character.Copper);
            var response = new Packets.PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT);
            response.AddInt32(MailID);
            response.AddInt32((int)MailResult.MAIL_MONEY_REMOVED);
            response.AddInt32(0);
            client.Send(response);
            response.Dispose();
            client.Character.SaveCharacter();
        }

        public void On_CMSG_ITEM_TEXT_QUERY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 9)
                return;
            packet.GetInt16();
            int MailID = packet.GetInt32();
            // Dim GameObjectGUID as ulong = packet.GetuInt64

            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_ITEM_TEXT_QUERY [MailID={2}]", client.IP, client.Port, MailID);
            var MySQLQuery = new DataTable();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT mail_body FROM characters_mail WHERE mail_id = {0};", (object)MailID), MySQLQuery);
            if (MySQLQuery.Rows.Count == 0)
                return;
            var response = new Packets.PacketClass(OPCODES.SMSG_ITEM_TEXT_QUERY_RESPONSE);
            response.AddInt32(MailID);
            response.AddString(Conversions.ToString(MySQLQuery.Rows[0]["mail_body"]));
            client.Send(response);
            response.Dispose();
        }

        public void On_CMSG_MAIL_CREATE_TEXT_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 17)
                return;
            packet.GetInt16();
            ulong GameObjectGUID = packet.GetUInt64();
            int MailID = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MAIL_CREATE_TEXT_ITEM [MailID={2}]", client.IP, client.Port, MailID);

            // DONE: Create Item with ITEM_FIELD_ITEM_TEXT_ID = MailID
            var tmpItem = new ItemObject(ITEM_MAILTEXT_ITEMID, client.Character.GUID) { ItemText = MailID };
            if (!client.Character.ItemADD(ref tmpItem))
            {
                var response = new Packets.PacketClass(OPCODES.SMSG_ITEM_TEXT_QUERY_RESPONSE);
                response.AddInt32(MailID);
                response.AddInt32(0);
                response.AddInt32(1);
                client.Send(response);
                response.Dispose();
                tmpItem.Delete();
            }
            else
            {
                client.Character.SendItemUpdate(tmpItem);
            }
        }

        public void On_CMSG_SEND_MAIL(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 14)
                return;
            packet.GetInt16();
            ulong GameObjectGUID = packet.GetUInt64();
            string Receiver = packet.GetString();
            if (packet.Data.Length - 1 < 14 + Receiver.Length + 1)
                return;
            string Subject = packet.GetString();
            if (packet.Data.Length - 1 < 14 + Receiver.Length + 2 + Subject.Length)
                return;
            string Body = packet.GetString();
            if (packet.Data.Length - 1 < 14 + Receiver.Length + 2 + Subject.Length + Body.Length + 4 + 4 + 1)
                return;
            packet.GetInt32();
            packet.GetInt32();
            ulong itemGuid = packet.GetUInt64();
            uint Money = packet.GetUInt32();
            uint COD = packet.GetUInt32();
            try
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SEND_MAIL [Receiver={2} Subject={3}]", client.IP, client.Port, Receiver, Subject);
                var MySQLQuery = new DataTable();
                WorldServiceLocator._WorldServer.CharacterDatabase.Query("SELECT char_guid, char_race FROM characters WHERE char_name Like '" + Receiver + "';", MySQLQuery);
                if (MySQLQuery.Rows.Count == 0)
                {
                    var response = new Packets.PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT);
                    response.AddInt32(0);
                    response.AddInt32((int)MailResult.MAIL_SENT);
                    response.AddInt32((int)MailSentError.CHARACTER_NOT_FOUND);
                    client.Send(response);
                    response.Dispose();
                    return;
                }

                ulong ReceiverGUID = Conversions.ToULong(MySQLQuery.Rows[0]["char_guid"]);
                bool ReceiverSide = WorldServiceLocator._Functions.GetCharacterSide(Conversions.ToByte(MySQLQuery.Rows[0]["char_race"]));
                if (client.Character.GUID == ReceiverGUID)
                {
                    var response = new Packets.PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT);
                    response.AddInt32(0);
                    response.AddInt32((int)MailResult.MAIL_SENT);
                    response.AddInt32((int)MailSentError.CANNOT_SEND_TO_SELF);
                    client.Send(response);
                    response.Dispose();
                    return;
                }

                if (client.Character.Copper < Money + 30L)
                {
                    var response = new Packets.PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT);
                    response.AddInt32(0);
                    response.AddInt32((int)MailResult.MAIL_SENT);
                    response.AddInt32((int)MailSentError.NOT_ENOUGHT_MONEY);
                    client.Send(response);
                    response.Dispose();
                    return;
                }

                // Lets check so that the receiver doesn't have a full inbox
                WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT mail_id FROM characters_mail WHERE mail_receiver = {0}", (object)ReceiverGUID), MySQLQuery);
                if (MySQLQuery.Rows.Count >= 100)
                {
                    var response = new Packets.PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT);
                    response.AddInt32(0);
                    response.AddInt32((int)MailResult.MAIL_SENT);
                    response.AddInt32((int)MailSentError.INTERNAL_ERROR);
                    client.Send(response);
                    response.Dispose();
                    return;
                }

                // You can only send mails to characters with your same faction, but GMs can ofc
                if (client.Access >= AccessLevel.GameMaster && client.Character.IsHorde != ReceiverSide)
                {
                    var response = new Packets.PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT);
                    response.AddInt32(0);
                    response.AddInt32((int)MailResult.MAIL_SENT);
                    response.AddInt32((int)MailSentError.NOT_YOUR_ALLIANCE);
                    client.Send(response);
                    response.Dispose();
                    return;
                }

                // Check if the item exists
                if (client.Character.ItemGETByGUID(itemGuid) is null)
                    itemGuid = 0UL;
                client.Character.Copper = (uint)(client.Character.Copper - (30L + Money));
                client.Character.SetUpdateFlag((int)EPlayerFields.PLAYER_FIELD_COINAGE, client.Character.Copper);
                int MailTime = WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now) + TimeConstant.DAY * 30; // Add 30 days to the current date/time
                WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format(@"INSERT INTO characters_mail (mail_sender, mail_receiver, mail_type, mail_stationary, mail_subject, mail_body, mail_money, mail_COD, mail_time, mail_read, item_guid) VALUES
                ({0},{1},{2},{3},'{4}','{5}',{6},{7},{8},{9},{10});", client.Character.GUID, ReceiverGUID, 0, 41, Subject.Replace("'", "`"), Body.Replace("'", "`"), Money, COD, MailTime, Conversions.ToByte(MailReadInfo.Unread), itemGuid == WorldServiceLocator._Global_Constants.GUID_ITEM));
                if (itemGuid > 0m)
                    client.Character.ItemREMOVE(itemGuid, false, true);

                // Tell the client we succeded
                var sendOK = new Packets.PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT);
                sendOK.AddInt32(0);
                sendOK.AddInt32((int)MailResult.MAIL_SENT);
                sendOK.AddInt32((int)MailSentError.NO_ERROR);
                client.Send(sendOK);
                sendOK.Dispose();
                WorldServiceLocator._WorldServer.CHARACTERs_Lock.AcquireReaderLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(ReceiverGUID))
                {
                    var response = new Packets.PacketClass(OPCODES.SMSG_RECEIVED_MAIL);
                    response.AddInt32(0);
                    WorldServiceLocator._WorldServer.CHARACTERs[ReceiverGUID].client.Send(response);
                    response.Dispose();
                }

                WorldServiceLocator._WorldServer.CHARACTERs_Lock.ReleaseReaderLock();
            }
            catch (Exception e)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error sending mail: {0}{1}", Environment.NewLine, e.ToString());
            }
        }

        // Public Sub SendNotify(ByRef client As ClientClass)
        // Dim packet As New PacketClass(OPCODES.SMSG_RECEIVED_MAIL)
        // packet.GetInt32() '(0)
        // client.Send(packet)
        // End Sub

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
    }
}