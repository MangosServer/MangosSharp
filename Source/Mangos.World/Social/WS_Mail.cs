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
using Mangos.Common.Enums.Misc;
using Mangos.Common.Enums.Social;
using Mangos.Common.Globals;
using Mangos.Common.Legacy;
using Mangos.World.Globals;
using Mangos.World.Network;
using Mangos.World.Objects;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Data;
using System.Runtime.CompilerServices;

namespace Mangos.World.Social;

public class WS_Mail
{
    public const int ITEM_MAILTEXT_ITEMID = 889;

    public void On_CMSG_MAIL_RETURN_TO_SENDER(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        checked
        {
            if (packet.Data.Length - 1 >= 17)
            {
                packet.GetInt16();
                var GameObjectGUID = packet.GetUInt64();
                var MailID = packet.GetInt32();
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MAIL_RETURN_TO_SENDER [MailID={2}]", client.IP, client.Port, MailID);
                var MailTime = (int)(WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now) + 2592000L);
                WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_mail SET mail_time = {1}, mail_read = 0, mail_receiver = (mail_receiver + mail_sender), mail_sender = (mail_receiver - mail_sender), mail_receiver = (mail_receiver - mail_sender) WHERE mail_id = {0};", MailID, MailTime));
                Packets.PacketClass response = new(Opcodes.SMSG_SEND_MAIL_RESULT);
                response.AddInt32(MailID);
                response.AddInt32(3);
                response.AddInt32(0);
                client.Send(ref response);
                response.Dispose();
            }
        }
    }

    public void On_CMSG_MAIL_DELETE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 17)
        {
            packet.GetInt16();
            var GameObjectGUID = packet.GetUInt64();
            var MailID = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MAIL_DELETE [MailID={2}]", client.IP, client.Port, MailID);
            WorldServiceLocator._WorldServer.CharacterDatabase.Update($"DELETE FROM characters_mail WHERE mail_id = {MailID};");
            Packets.PacketClass response = new(Opcodes.SMSG_SEND_MAIL_RESULT);
            response.AddInt32(MailID);
            response.AddInt32(4);
            response.AddInt32(0);
            client.Send(ref response);
            response.Dispose();
        }
    }

    public void On_CMSG_MAIL_MARK_AS_READ(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        checked
        {
            if (packet.Data.Length - 1 >= 17)
            {
                packet.GetInt16();
                var GameObjectGUID = packet.GetUInt64();
                var MailID = packet.GetInt32();
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MAIL_MARK_AS_READ [MailID={2}]", client.IP, client.Port, MailID);
                var MailTime = (int)(WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now) + 259200L);
                WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_mail SET mail_read = 1, mail_time = {1} WHERE mail_id = {0} AND mail_read < 2;", MailID, MailTime));
            }
        }
    }

    public void On_MSG_QUERY_NEXT_MAIL_TIME(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_QUERY_NEXT_MAIL_TIME", client.IP, client.Port);
        DataTable MySQLQuery = new();
        WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT COUNT(*) FROM characters_mail WHERE mail_read = 0 AND mail_receiver = {client.Character.GUID} AND mail_time > {WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now)};", ref MySQLQuery);
        if (Operators.ConditionalCompareObjectGreater(MySQLQuery.Rows[0][0], 0, TextCompare: false))
        {
            Packets.PacketClass response2 = new(Opcodes.MSG_QUERY_NEXT_MAIL_TIME);
            response2.AddInt32(0);
            client.Send(ref response2);
            response2.Dispose();
        }
        else
        {
            Packets.PacketClass response = new(Opcodes.MSG_QUERY_NEXT_MAIL_TIME);
            response.AddInt8(0);
            response.AddInt8(192);
            response.AddInt8(168);
            response.AddInt8(199);
            client.Send(ref response);
            response.Dispose();
        }
    }

    public void On_CMSG_GET_MAIL_LIST(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        checked
        {
            if (packet.Data.Length - 1 < 13)
            {
                return;
            }
            packet.GetInt16();
            var GameObjectGUID = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GET_MAIL_LIST [GUID={2:X}]", client.IP, client.Port, GameObjectGUID);
            try
            {
                DataTable MySQLQuery = new();
                WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT mail_id FROM characters_mail WHERE mail_time < {WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now)};", ref MySQLQuery);
                if (MySQLQuery.Rows.Count > 0)
                {
                    var b = (byte)(MySQLQuery.Rows.Count - 1);
                    byte j = 0;
                    while (j <= (uint)b)
                    {
                        WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("DELETE FROM characters_mail WHERE mail_id = {0};", RuntimeHelpers.GetObjectValue(MySQLQuery.Rows[j]["mail_id"])));
                        j = (byte)unchecked((uint)(j + 1));
                    }
                }
                WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT * FROM characters_mail WHERE mail_receiver = {client.Character.GUID};", ref MySQLQuery);
                Packets.PacketClass response = new(Opcodes.SMSG_MAIL_LIST_RESULT);
                response.AddInt8((byte)MySQLQuery.Rows.Count);
                if (MySQLQuery.Rows.Count > 0)
                {
                    var b2 = (byte)(MySQLQuery.Rows.Count - 1);
                    byte i = 0;
                    while (i <= (uint)b2)
                    {
                        response.AddInt32(MySQLQuery.Rows[i].As<int>("mail_id"));
                        response.AddInt8(MySQLQuery.Rows[i].As<byte>("mail_type"));
                        var left = MySQLQuery.Rows[i]["mail_type"];
                        if (Operators.ConditionalCompareObjectEqual(left, MailTypeInfo.NORMAL, TextCompare: false))
                        {
                            response.AddUInt64(MySQLQuery.Rows[i].As<ulong>("mail_sender"));
                        }
                        else if (Operators.ConditionalCompareObjectEqual(left, MailTypeInfo.AUCTION, TextCompare: false))
                        {
                            response.AddInt32(MySQLQuery.Rows[i].As<int>("mail_sender"));
                        }
                        response.AddString(MySQLQuery.Rows[i].As<string>("mail_subject"));
                        if (Operators.ConditionalCompareObjectNotEqual(MySQLQuery.Rows[i]["mail_body"], "", TextCompare: false))
                        {
                            response.AddInt32(MySQLQuery.Rows[i].As<int>("mail_id"));
                        }
                        else
                        {
                            response.AddInt32(0);
                        }
                        response.AddInt32(0);
                        response.AddInt32(MySQLQuery.Rows[i].As<int>("mail_stationary"));
                        if (decimal.Compare(new decimal(MySQLQuery.Rows[i].As<ulong>("item_guid")), 0m) > 0)
                        {
                            var tmpItem = WorldServiceLocator._WS_Items.LoadItemByGUID(MySQLQuery.Rows[i].As<ulong>("item_guid"));
                            response.AddInt32(tmpItem.ItemEntry);
                            if (tmpItem.Enchantments.ContainsKey(0))
                            {
                                packet.AddInt32(tmpItem.Enchantments[0].ID);
                            }
                            else
                            {
                                packet.AddInt32(0);
                            }
                            response.AddInt32(tmpItem.RandomProperties);
                            response.AddInt32(0);
                            response.AddInt8((byte)tmpItem.StackCount);
                            response.AddInt32(tmpItem.ChargesLeft);
                            response.AddInt32(tmpItem.ItemInfo.Durability);
                            response.AddInt32(tmpItem.Durability);
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
                        response.AddUInt32(MySQLQuery.Rows[i].As<uint>("mail_money"));
                        response.AddUInt32(MySQLQuery.Rows[i].As<uint>("mail_COD"));
                        response.AddInt32(MySQLQuery.Rows[i].As<int>("mail_read"));
                        response.AddSingle((float)((MySQLQuery.Rows[i].As<uint>("mail_time") - WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now)) / 86400.0));
                        response.AddInt32(0);
                        i = (byte)unchecked((uint)(i + 1));
                    }
                }
                client.Send(ref response);
                response.Dispose();
            }
            catch (Exception ex)
            {
                ProjectData.SetProjectError(ex);
                var e = ex;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error getting mail list: {0}{1}", Environment.NewLine, e.ToString());
                ProjectData.ClearProjectError();
            }
        }
    }

    public void On_CMSG_MAIL_TAKE_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        checked
        {
            if (packet.Data.Length - 1 < 17)
            {
                return;
            }
            packet.GetInt16();
            var GameObjectGUID = packet.GetUInt64();
            var MailID = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MAIL_TAKE_ITEM [MailID={2}]", client.IP, client.Port, MailID);
            try
            {
                DataTable MySQLQuery = new();
                WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT mail_cod, mail_sender, item_guid FROM characters_mail WHERE mail_id = {MailID} AND mail_receiver = {client.Character.GUID};", ref MySQLQuery);
                if (MySQLQuery.Rows.Count == 0)
                {
                    Packets.PacketClass response4 = new(Opcodes.SMSG_SEND_MAIL_RESULT);
                    response4.AddInt32(MailID);
                    response4.AddInt32(2);
                    response4.AddInt32(6);
                    client.Send(ref response4);
                    response4.Dispose();
                    return;
                }
                if (!Operators.ConditionalCompareObjectNotEqual(MySQLQuery.Rows[0]["mail_cod"], 0, TextCompare: false))
                {
                    goto IL_02b9;
                }
                if (Operators.ConditionalCompareObjectLess(client.Character.Copper, MySQLQuery.Rows[0]["mail_cod"], TextCompare: false))
                {
                    Packets.PacketClass noMoney = new(Opcodes.SMSG_SEND_MAIL_RESULT);
                    noMoney.AddInt32(MailID);
                    noMoney.AddInt32(0);
                    noMoney.AddInt32(3);
                    client.Send(ref noMoney);
                    noMoney.Dispose();
                    return;
                }
                ref var copper = ref client.Character.Copper;
                copper = Conversions.ToUInteger(Operators.SubtractObject(copper, MySQLQuery.Rows[0]["mail_cod"]));
                WorldServiceLocator._WorldServer.CharacterDatabase.Update($"UPDATE characters_mail SET mail_cod = 0 WHERE mail_id = {MailID};");
                var MailTime = (int)(WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now) + 2592000L);
                WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("INSERT INTO characters_mail (mail_sender, mail_receiver, mail_subject, mail_body, mail_item_guid, mail_money, mail_COD, mail_time, mail_read, mail_type) VALUES \r\n                        ({0},{1},'{2}','{3}',{4},{5},{6},{7},{8},{9});", client.Character.GUID, MySQLQuery.Rows[0]["mail_sender"], "", "", 0, MySQLQuery.Rows[0]["mail_cod"], 0, MailTime, MailReadInfo.COD, 0));
            IL_02b9:
                if (Operators.ConditionalCompareObjectEqual(MySQLQuery.Rows[0]["item_guid"], 0, TextCompare: false))
                {
                    Packets.PacketClass response3 = new(Opcodes.SMSG_SEND_MAIL_RESULT);
                    response3.AddInt32(MailID);
                    response3.AddInt32(2);
                    response3.AddInt32(6);
                    client.Send(ref response3);
                    response3.Dispose();
                    return;
                }
                var tmpItem = WorldServiceLocator._WS_Items.LoadItemByGUID(MySQLQuery.Rows[0].As<ulong>("item_guid"));
                tmpItem.OwnerGUID = client.Character.GUID;
                tmpItem.Save();
                if (client.Character.ItemADD(ref tmpItem))
                {
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update($"UPDATE characters_mail SET item_guid = 0 WHERE mail_id = {MailID};");
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update($"DELETE FROM mail_items WHERE mail_id = {MailID};");
                    Packets.PacketClass response2 = new(Opcodes.SMSG_SEND_MAIL_RESULT);
                    response2.AddInt32(MailID);
                    response2.AddInt32(2);
                    response2.AddInt32(0);
                    client.Send(ref response2);
                    response2.Dispose();
                }
                else
                {
                    tmpItem.Dispose();
                    Packets.PacketClass response = new(Opcodes.SMSG_SEND_MAIL_RESULT);
                    response.AddInt32(MailID);
                    response.AddInt32(2);
                    response.AddInt32(1);
                    client.Send(ref response);
                    response.Dispose();
                }
                client.Character.Save();
            }
            catch (Exception ex)
            {
                ProjectData.SetProjectError(ex);
                var e = ex;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error getting item from mail: {0}{1}", Environment.NewLine, e.ToString());
                ProjectData.ClearProjectError();
            }
        }
    }

    public void On_CMSG_MAIL_TAKE_MONEY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        checked
        {
            if (packet.Data.Length - 1 >= 17)
            {
                packet.GetInt16();
                var GameObjectGUID = packet.GetUInt64();
                var MailID = packet.GetInt32();
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MAIL_TAKE_MONEY [MailID={2}]", client.IP, client.Port, MailID);
                DataTable MySQLQuery = new();
                WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT mail_money FROM characters_mail WHERE mail_id = {0}; UPDATE characters_mail SET mail_money = 0 WHERE mail_id = {0};", MailID), ref MySQLQuery);
                if (client.Character.Copper + Conversions.ToLong(MySQLQuery.Rows[0]["mail_money"]) > uint.MaxValue)
                {
                    client.Character.Copper = uint.MaxValue;
                }
                else
                {
                    ref var copper = ref client.Character.Copper;
                    copper = Conversions.ToUInteger(Operators.AddObject(copper, MySQLQuery.Rows[0]["mail_money"]));
                }
                client.Character.SetUpdateFlag(1176, client.Character.Copper);
                Packets.PacketClass response = new(Opcodes.SMSG_SEND_MAIL_RESULT);
                response.AddInt32(MailID);
                response.AddInt32(1);
                response.AddInt32(0);
                client.Send(ref response);
                response.Dispose();
                client.Character.SaveCharacter();
            }
        }
    }

    public void On_CMSG_ITEM_TEXT_QUERY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 9)
        {
            packet.GetInt16();
            var MailID = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_ITEM_TEXT_QUERY [MailID={2}]", client.IP, client.Port, MailID);
            DataTable MySQLQuery = new();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT mail_body FROM characters_mail WHERE mail_id = {MailID};", ref MySQLQuery);
            if (MySQLQuery.Rows.Count != 0)
            {
                Packets.PacketClass response = new(Opcodes.SMSG_ITEM_TEXT_QUERY_RESPONSE);
                response.AddInt32(MailID);
                response.AddString(MySQLQuery.Rows[0].As<string>("mail_body"));
                client.Send(ref response);
                response.Dispose();
            }
        }
    }

    public void On_CMSG_MAIL_CREATE_TEXT_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 17)
        {
            packet.GetInt16();
            var GameObjectGUID = packet.GetUInt64();
            var MailID = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MAIL_CREATE_TEXT_ITEM [MailID={2}]", client.IP, client.Port, MailID);
            ItemObject tmpItem = new(889, client.Character.GUID)
            {
                ItemText = MailID
            };
            if (!client.Character.ItemADD(ref tmpItem))
            {
                Packets.PacketClass response = new(Opcodes.SMSG_ITEM_TEXT_QUERY_RESPONSE);
                response.AddInt32(MailID);
                response.AddInt32(0);
                response.AddInt32(1);
                client.Send(ref response);
                response.Dispose();
                tmpItem.Delete();
            }
            else
            {
                client.Character.SendItemUpdate(tmpItem);
            }
        }
    }

    public void On_CMSG_SEND_MAIL(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        checked
        {
            if (packet.Data.Length - 1 < 14)
            {
                return;
            }
            packet.GetInt16();
            var GameObjectGUID = packet.GetUInt64();
            var Receiver = packet.GetString();
            if (packet.Data.Length - 1 < 14 + Receiver.Length + 1)
            {
                return;
            }
            var Subject = packet.GetString();
            if (packet.Data.Length - 1 < 14 + Receiver.Length + 2 + Subject.Length)
            {
                return;
            }
            var Body = packet.GetString();
            if (packet.Data.Length - 1 < 14 + Receiver.Length + 2 + Subject.Length + Body.Length + 4 + 4 + 1)
            {
                return;
            }
            packet.GetInt32();
            packet.GetInt32();
            var itemGuid = packet.GetUInt64();
            var Money = packet.GetUInt32();
            var COD = packet.GetUInt32();
            try
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SEND_MAIL [Receiver={2} Subject={3}]", client.IP, client.Port, Receiver, Subject);
                DataTable MySQLQuery = new();
                WorldServiceLocator._WorldServer.CharacterDatabase.Query("SELECT char_guid, char_race FROM characters WHERE char_name Like '" + Receiver + "';", ref MySQLQuery);
                if (MySQLQuery.Rows.Count == 0)
                {
                    Packets.PacketClass response6 = new(Opcodes.SMSG_SEND_MAIL_RESULT);
                    response6.AddInt32(0);
                    response6.AddInt32(0);
                    response6.AddInt32(4);
                    client.Send(ref response6);
                    response6.Dispose();
                    return;
                }
                var ReceiverGUID = MySQLQuery.Rows[0].As<ulong>("char_guid");
                var ReceiverSide = WorldServiceLocator._Functions.GetCharacterSide(MySQLQuery.Rows[0].As<byte>("char_race"));
                if (client.Character.GUID == ReceiverGUID)
                {
                    Packets.PacketClass response5 = new(Opcodes.SMSG_SEND_MAIL_RESULT);
                    response5.AddInt32(0);
                    response5.AddInt32(0);
                    response5.AddInt32(2);
                    client.Send(ref response5);
                    response5.Dispose();
                    return;
                }
                if (client.Character.Copper < Money + 30L)
                {
                    Packets.PacketClass response4 = new(Opcodes.SMSG_SEND_MAIL_RESULT);
                    response4.AddInt32(0);
                    response4.AddInt32(0);
                    response4.AddInt32(3);
                    client.Send(ref response4);
                    response4.Dispose();
                    return;
                }
                WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT mail_id FROM characters_mail WHERE mail_receiver = {ReceiverGUID}", ref MySQLQuery);
                if (MySQLQuery.Rows.Count >= 100)
                {
                    Packets.PacketClass response3 = new(Opcodes.SMSG_SEND_MAIL_RESULT);
                    response3.AddInt32(0);
                    response3.AddInt32(0);
                    response3.AddInt32(6);
                    client.Send(ref response3);
                    response3.Dispose();
                    return;
                }
                if (client.Access >= AccessLevel.GameMaster && client.Character.IsHorde != ReceiverSide)
                {
                    Packets.PacketClass response2 = new(Opcodes.SMSG_SEND_MAIL_RESULT);
                    response2.AddInt32(0);
                    response2.AddInt32(0);
                    response2.AddInt32(5);
                    client.Send(ref response2);
                    response2.Dispose();
                    return;
                }
                if (client.Character.ItemGETByGUID(itemGuid) == null)
                {
                    itemGuid = 0uL;
                }
                ref var copper = ref client.Character.Copper;
                copper = (uint)(copper - (30L + Money));
                client.Character.SetUpdateFlag(1176, client.Character.Copper);
                var MailTime = (int)(WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now) + 2592000L);
                WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("INSERT INTO characters_mail (mail_sender, mail_receiver, mail_type, mail_stationary, mail_subject, mail_body, mail_money, mail_COD, mail_time, mail_read, item_guid) VALUES\r\n                ({0},{1},{2},{3},'{4}','{5}',{6},{7},{8},{9},{10});", client.Character.GUID, ReceiverGUID, 0, 41, Subject.Replace("'", "`"), Body.Replace("'", "`"), Money, COD, MailTime, (byte)0, itemGuid == WorldServiceLocator._Global_Constants.GUID_ITEM));
                if (decimal.Compare(new decimal(itemGuid), 0m) > 0)
                {
                    client.Character.ItemREMOVE(itemGuid, Destroy: false, Update: true);
                }
                Packets.PacketClass sendOK = new(Opcodes.SMSG_SEND_MAIL_RESULT);
                sendOK.AddInt32(0);
                sendOK.AddInt32(0);
                sendOK.AddInt32(0);
                client.Send(ref sendOK);
                sendOK.Dispose();
                WorldServiceLocator._WorldServer.CHARACTERs_Lock.AcquireReaderLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(ReceiverGUID))
                {
                    Packets.PacketClass response = new(Opcodes.SMSG_RECEIVED_MAIL);
                    response.AddInt32(0);
                    WorldServiceLocator._WorldServer.CHARACTERs[ReceiverGUID].client.Send(ref response);
                    response.Dispose();
                }
                WorldServiceLocator._WorldServer.CHARACTERs_Lock.ReleaseReaderLock();
            }
            catch (Exception ex)
            {
                ProjectData.SetProjectError(ex);
                var e = ex;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error sending mail: {0}{1}", Environment.NewLine, e.ToString());
                ProjectData.ClearProjectError();
            }
        }
    }
}
