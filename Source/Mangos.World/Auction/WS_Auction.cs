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

using Mangos.Common.Enums.AuctionHouse;
using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using Mangos.Common.Legacy;
using Mangos.World.Globals;
using Mangos.World.Network;
using Mangos.World.Objects;
using Mangos.World.Player;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections;
using System.Data;
using System.Runtime.CompilerServices;

namespace Mangos.World.Auction;

public class WS_Auction
{
    private const IEnumerator Enumerator = default;
    public int AuctionID;

    public int AuctionFee;

    public int AuctionTax;

    public AuctionHouses GetAuctionSide(ulong GUID)
    {
        if (WorldServiceLocator._ConfigurationProvider.GetConfiguration().GlobalAuction)
        {
            return AuctionHouses.AUCTION_UNDEFINED;
        }

        return WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].CreatureInfo.Faction switch
        {
            29 or 68 or 104 => AuctionHouses.AUCTION_HORDE,
            12 or 55 or 79 => AuctionHouses.AUCTION_ALLIANCE,
            _ => AuctionHouses.AUCTION_NEUTRAL,
        };
    }

    public int GetAuctionDeposit(ulong GUID, int Price, int ItemCount, int Time)
    {
        if (ItemCount == 0)
        {
            ItemCount = 1;
        }
        return checked(GetAuctionSide(GUID) switch
        {
            AuctionHouses.AUCTION_NEUTRAL => (int)(0.25f * Price * ItemCount * (Time / 120.0)),
            AuctionHouses.AUCTION_UNDEFINED => 0,
            _ => (int)(0.05f * Price * ItemCount * (Time / 120.0)),
        });
    }

    public void AuctionCreateMail(MailAuctionAction MailAction, AuctionHouses AuctionLocation, ulong ReceiverGUID, int ItemID, ref Packets.PacketClass packet)
    {
        var queryString = "INSERT INTO characters_mail (";
        var valuesString = ") VALUES (";
        var MailID = packet.GetInt32();
        queryString += "mail_sender,";
        var str = valuesString;
        var num = (int)AuctionLocation;
        valuesString = str + (int)AuctionLocation;
        queryString += "mail_receiver,";
        valuesString += ReceiverGUID;
        queryString += "mail_type,";
        valuesString += "2";
        queryString += "mail_stationary,";
        valuesString += "62";
        queryString += "mail_subject,";
        var str2 = valuesString;
        var str3 = ItemID.ToString();
        num = (int)MailAction;
        valuesString = str2 + str3 + ":0:" + (int)AuctionLocation;
        queryString += "mail_body,";
        valuesString ??= "";
        queryString += "mail_money,";
        valuesString ??= "";
        queryString += "mail_COD,";
        valuesString += "0";
        queryString += "mail_time,";
        valuesString += "30";
        queryString += "mail_read,";
        valuesString += "0";
        queryString += "item_guid,";
        valuesString += ");";
        WorldServiceLocator._WorldServer.CharacterDatabase.Update($"{queryString}{valuesString}");
    }

    public void SendShowAuction(ref WS_PlayerData.CharacterObject objCharacter, ulong GUID)
    {
        Packets.PacketClass packet = new(Opcodes.MSG_AUCTION_HELLO);
        new Packets.PacketClass(Opcodes.MSG_AUCTION_HELLO).AddUInt64(GUID);
        new Packets.PacketClass(Opcodes.MSG_AUCTION_HELLO).AddUInt64((ulong)GetAuctionSide(GUID));
        objCharacter.client.Send(ref packet);
        new Packets.PacketClass(Opcodes.MSG_AUCTION_HELLO).Dispose();
    }

    public void AuctionListAddItem(ref Packets.PacketClass packet, ref DataRow row)
    {
        packet.AddUInt32(row.As<uint>("auction_id"));
        var itemId = row.As<uint>("auction_itemId");
        packet.AddUInt32(itemId);
        checked
        {
            packet.AddUInt32(0u);
            packet.AddUInt32((uint)((!WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey((int)itemId)) ? new WS_Items.ItemInfo((int)itemId) : WorldServiceLocator._WorldServer.ITEMDatabase[(int)itemId]).RandomProp);
            packet.AddUInt32((uint)((!WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey((int)itemId)) ? new WS_Items.ItemInfo((int)itemId) : WorldServiceLocator._WorldServer.ITEMDatabase[(int)itemId]).RandomSuffix);
            packet.AddUInt32(row.As<uint>("auction_itemCount"));
            packet.AddInt32(((!WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey((int)itemId)) ? new WS_Items.ItemInfo((int)itemId) : WorldServiceLocator._WorldServer.ITEMDatabase[(int)itemId]).Spells[0].SpellCharges);
            packet.AddUInt64(row.As<ulong>("auction_owner"));
            packet.AddUInt32(row.As<uint>("auction_bid"));
            packet.AddUInt32(Conversions.ToUInteger(Operators.AddObject(Conversion.Fix(Operators.MultiplyObject(row["auction_bid"], 0.1f)), 1)));
            packet.AddUInt32(row.As<uint>("auction_buyout"));
            packet.AddUInt32(Conversions.ToUInteger(Operators.MultiplyObject(row["auction_timeleft"], 1000)));
            packet.AddUInt64(row.As<ulong>("auction_bidder"));
            packet.AddUInt32(row.As<uint>("auction_bid"));
        }
    }

    public void SendAuctionCommandResult(ref WS_Network.ClientClass client, int AuctionID, AuctionAction AuctionAction, AuctionError AuctionError, int BidError)
    {
        Packets.PacketClass response = new(Opcodes.SMSG_AUCTION_COMMAND_RESULT);
        new Packets.PacketClass(Opcodes.SMSG_AUCTION_COMMAND_RESULT).AddInt32(AuctionID);
        new Packets.PacketClass(Opcodes.SMSG_AUCTION_COMMAND_RESULT).AddInt32((int)AuctionAction);
        new Packets.PacketClass(Opcodes.SMSG_AUCTION_COMMAND_RESULT).AddInt32((int)AuctionError);
        new Packets.PacketClass(Opcodes.SMSG_AUCTION_COMMAND_RESULT).AddInt32(BidError);
        client.Send(ref response);
        new Packets.PacketClass(Opcodes.SMSG_AUCTION_COMMAND_RESULT).Dispose();
    }

    public void SendAuctionBidderNotification(ref WS_PlayerData.CharacterObject objCharacter)
    {
        Packets.PacketClass packet = new(Opcodes.SMSG_AUCTION_BIDDER_NOTIFICATION);
        new Packets.PacketClass(Opcodes.SMSG_AUCTION_BIDDER_NOTIFICATION).AddInt32(0);
        new Packets.PacketClass(Opcodes.SMSG_AUCTION_BIDDER_NOTIFICATION).AddInt32(0);
        new Packets.PacketClass(Opcodes.SMSG_AUCTION_BIDDER_NOTIFICATION).AddUInt64(0uL);
        new Packets.PacketClass(Opcodes.SMSG_AUCTION_BIDDER_NOTIFICATION).AddInt32(0);
        new Packets.PacketClass(Opcodes.SMSG_AUCTION_BIDDER_NOTIFICATION).AddInt32(0);
        new Packets.PacketClass(Opcodes.SMSG_AUCTION_BIDDER_NOTIFICATION).AddInt32(0);
        new Packets.PacketClass(Opcodes.SMSG_AUCTION_BIDDER_NOTIFICATION).AddInt32(0);
        objCharacter.client.Send(ref packet);
        new Packets.PacketClass(Opcodes.SMSG_AUCTION_BIDDER_NOTIFICATION).Dispose();
    }

    public void SendAuctionOwnerNotification(ref WS_PlayerData.CharacterObject objCharacter)
    {
        Packets.PacketClass packet = new(Opcodes.SMSG_AUCTION_OWNER_NOTIFICATION);
        new Packets.PacketClass(Opcodes.SMSG_AUCTION_OWNER_NOTIFICATION).AddInt32(0);
        new Packets.PacketClass(Opcodes.SMSG_AUCTION_OWNER_NOTIFICATION).AddInt32(0);
        new Packets.PacketClass(Opcodes.SMSG_AUCTION_OWNER_NOTIFICATION).AddInt32(0);
        new Packets.PacketClass(Opcodes.SMSG_AUCTION_OWNER_NOTIFICATION).AddInt32(0);
        new Packets.PacketClass(Opcodes.SMSG_AUCTION_OWNER_NOTIFICATION).AddInt32(0);
        new Packets.PacketClass(Opcodes.SMSG_AUCTION_OWNER_NOTIFICATION).AddInt32(0);
        new Packets.PacketClass(Opcodes.SMSG_AUCTION_OWNER_NOTIFICATION).AddInt32(0);
        objCharacter.client.Send(ref packet);
        new Packets.PacketClass(Opcodes.SMSG_AUCTION_OWNER_NOTIFICATION).Dispose();
    }

    public void SendAuctionRemovedNotification(ref WS_PlayerData.CharacterObject objCharacter)
    {
        Packets.PacketClass packet = new(Opcodes.SMSG_AUCTION_REMOVED_NOTIFICATION);
        new Packets.PacketClass(Opcodes.SMSG_AUCTION_REMOVED_NOTIFICATION).AddInt32(0);
        new Packets.PacketClass(Opcodes.SMSG_AUCTION_REMOVED_NOTIFICATION).AddInt32(0);
        new Packets.PacketClass(Opcodes.SMSG_AUCTION_REMOVED_NOTIFICATION).AddInt32(0);
        objCharacter.client.Send(ref packet);
        new Packets.PacketClass(Opcodes.SMSG_AUCTION_REMOVED_NOTIFICATION).Dispose();
    }

    public void SendAuctionListOwnerItems(ref WS_Network.ClientClass client)
    {
        Packets.PacketClass response = new(Opcodes.SMSG_AUCTION_OWNER_LIST_RESULT);
        DataTable MySQLQuery = new();
        WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT * FROM auctionhouse WHERE auction_owner = {Conversions.ToString(client.Character.GUID)};", ref MySQLQuery);
        if (MySQLQuery.Rows.Count > 50)
        {
            new Packets.PacketClass(Opcodes.SMSG_AUCTION_OWNER_LIST_RESULT).AddInt32(50);
        }
        else
        {
            new Packets.PacketClass(Opcodes.SMSG_AUCTION_OWNER_LIST_RESULT).AddInt32(MySQLQuery.Rows.Count);
        }
        var count = 0;
        var enumerator = Enumerator;
        try
        {
            enumerator = MySQLQuery.Rows.GetEnumerator();
            while (enumerator.MoveNext())
            {
                DataRow row = (DataRow)enumerator.Current;
                AuctionListAddItem(ref response, ref row);
                count = checked(count + 1);
                if (count == 50)
                {
                    break;
                }
            }
        }
        finally
        {
            if (enumerator is IDisposable)
            {
                (enumerator as IDisposable).Dispose();
            }
        }
        new Packets.PacketClass(Opcodes.SMSG_AUCTION_OWNER_LIST_RESULT).AddInt32(MySQLQuery.Rows.Count);
        client.Send(ref response);
        new Packets.PacketClass(Opcodes.SMSG_AUCTION_OWNER_LIST_RESULT).Dispose();
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_AUCTION_OWNER_LIST_RESULT", client.IP, client.Port);
    }

    public void SendAuctionListBidderItems(ref WS_Network.ClientClass client)
    {
        Packets.PacketClass response = new(Opcodes.SMSG_AUCTION_BIDDER_LIST_RESULT);
        DataTable MySQLQuery = new();
        WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT * FROM auctionhouse WHERE auction_bidder = {Conversions.ToString(client.Character.GUID)};", ref MySQLQuery);
        if (MySQLQuery.Rows.Count > 50)
        {
            response.AddInt32(50);
        }
        else
        {
            response.AddInt32(MySQLQuery.Rows.Count);
        }
        var count = 0;
        var enumerator = Enumerator;
        try
        {
            enumerator = MySQLQuery.Rows.GetEnumerator();
            while (enumerator.MoveNext())
            {
                DataRow row = (DataRow)enumerator.Current;
                AuctionListAddItem(ref response, ref row);
                count = checked(count + 1);
                if (count == 50)
                {
                    break;
                }
            }
        }
        finally
        {
            if (enumerator is IDisposable)
            {
                (enumerator as IDisposable).Dispose();
            }
        }
        response.AddInt32(MySQLQuery.Rows.Count);
        client.Send(ref response);
        response.Dispose();
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_AUCTION_BIDDER_LIST_RESULT", client.IP, client.Port);
    }

    public void On_MSG_AUCTION_HELLO(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 13)
        {
            packet.GetInt16();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_AUCTION_HELLO [GUID={2}]", client.IP, client.Port, packet.GetUInt32());
            SendShowAuction(ref client.Character, packet.GetUInt32());
        }
    }

    public void On_CMSG_AUCTION_SELL_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        checked
        {
            if (packet.Data.Length - 1 < 33)
            {
                return;
            }
            packet.GetInt16();
            var cGUID = packet.GetUInt64();
            var iGUID = packet.GetUInt64();
            var Bid = packet.GetInt32();
            var Buyout = packet.GetInt32();
            var Time = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AUCTION_SELL_ITEM [Bid={2} BuyOut={3} Time={4}]", client.IP, client.Port, Bid, Buyout, Time);
            Time *= 60;
            if (WorldServiceLocator._WorldServer.WORLD_ITEMs[iGUID].ItemInfo.IsContainer && !WorldServiceLocator._WorldServer.WORLD_ITEMs[iGUID].IsFree)
            {
                SendAuctionCommandResult(ref client, 0, AuctionAction.AUCTION_SELL_ITEM, AuctionError.CANNOT_BID_YOUR_AUCTION_ERROR, 0);
                return;
            }
            if (client.Character.Copper < GetAuctionDeposit(cGUID, WorldServiceLocator._WorldServer.WORLD_ITEMs[iGUID].ItemInfo.SellPrice, WorldServiceLocator._WorldServer.WORLD_ITEMs[iGUID].StackCount, Time))
            {
                SendAuctionCommandResult(ref client, 0, AuctionAction.AUCTION_SELL_ITEM, AuctionError.AUCTION_NOT_ENOUGHT_MONEY, 0);
                return;
            }
            ref var copper = ref client.Character.Copper;
            copper = (uint)(copper - GetAuctionDeposit(cGUID, WorldServiceLocator._WorldServer.WORLD_ITEMs[iGUID].ItemInfo.SellPrice, WorldServiceLocator._WorldServer.WORLD_ITEMs[iGUID].StackCount, Time));
            client.Character.ItemREMOVE(iGUID, Destroy: false, Update: true);
            WorldServiceLocator._WorldServer.CharacterDatabase.Update($@"INSERT INTO auctionhouse (auction_bid, auction_buyout, auction_timeleft, auction_bidder, auction_owner, auction_itemId, auction_itemGuid, auction_itemCount) VALUES 
            ({Bid},{Buyout},{Time},{0},{client.Character.GUID},{WorldServiceLocator._WorldServer.WORLD_ITEMs[iGUID].ItemEntry},{iGUID - WorldServiceLocator._Global_Constants.GUID_ITEM},{WorldServiceLocator._WorldServer.WORLD_ITEMs[iGUID].StackCount});");
            DataTable MySQLQuery = new();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT auction_id FROM auctionhouse WHERE auction_itemGuid = {Conversions.ToString(iGUID - WorldServiceLocator._Global_Constants.GUID_ITEM)};", ref MySQLQuery);
            if (MySQLQuery.Rows.Count != 0)
            {
                SendAuctionCommandResult(ref client, MySQLQuery.Rows[0].As<int>("auction_id"), AuctionAction.AUCTION_SELL_ITEM, AuctionError.AUCTION_OK, 0);
            }
        }
    }

    public void On_CMSG_AUCTION_REMOVE_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        packet.GetInt16();
        var GUID = packet.GetUInt64();
        checked
        {
            var MailTime = (int)(WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now) + 2592000L);
            var AuctionID = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AUCTION_REMOVE_ITEM [GUID={2} AuctionID={3}]", client.IP, client.Port, GUID, AuctionID);
            DataTable MySQLQuery = new();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT * FROM auctionhouse WHERE auction_id = {Conversions.ToString(AuctionID)};", ref MySQLQuery);
            if (MySQLQuery.Rows.Count != 0)
            {
                WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format(@"INSERT INTO characters_mail (mail_sender, mail_receiver, mail_type, mail_stationary, mail_subject, mail_body, mail_money, mail_COD, mail_time, mail_read, item_guid) VALUES
            ({0},{1},{2},{3},'{4}','{5}',{6},{7},{8},{9},{10});", AuctionID, MySQLQuery.Rows[0]["auction_owner"], 2, 62, Operators.ConcatenateObject(MySQLQuery.Rows[0]["auction_itemId"], ":0:4"), "", 0, 0, MailTime, 0, MySQLQuery.Rows[0]["auction_itemGuid"]));
                DataTable MailQuery = new();
                WorldServiceLocator._WorldServer.CharacterDatabase.Query(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("SELECT mail_id FROM characters_mail WHERE mail_receiver = ", MySQLQuery.Rows[0]["auction_owner"]), ";")), ref MailQuery);
                var MailID = Conversions.ToInteger(MailQuery.Rows[0]["mail_id"]);
                WorldServiceLocator._WorldServer.CharacterDatabase.Update($"INSERT INTO mail_items (mail_id, item_guid) VALUES ({MailID}, {RuntimeHelpers.GetObjectValue(MySQLQuery.Rows[0]["auction_itemGuid"])});");
                if (Operators.ConditionalCompareObjectNotEqual(MySQLQuery.Rows[0]["auction_bidder"], 0, TextCompare: false))
                {
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format(@"INSERT INTO characters_mail (mail_sender, mail_receiver, mail_type, mail_stationary, mail_subject, mail_body, mail_money, mail_COD, mail_time, mail_read, item_guid) VALUES
            ({0},{1},{2},{3},'{4}','{5}',{6},{7},{8},{9},{10});", AuctionID, MySQLQuery.Rows[0]["auction_bidder"], 2, 62, Operators.ConcatenateObject(MySQLQuery.Rows[0]["auction_itemId"], ":0:4"), "", MySQLQuery.Rows[0]["auction_bid"], 0, MailTime, 0, MySQLQuery.Rows[0]["auction_itemGuid"]));
                }
                WorldServiceLocator._WorldServer.CharacterDatabase.Update($"DELETE FROM auctionhouse WHERE auction_id = {Conversions.ToString(AuctionID)};");
                SendAuctionCommandResult(ref client, AuctionID, AuctionAction.AUCTION_CANCEL, AuctionError.AUCTION_OK, 0);
            }
        }
    }

    public void On_CMSG_AUCTION_PLACE_BID(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        packet.GetInt16();
        var cGUID = packet.GetUInt64();
        checked
        {
            var MailTime = (int)(WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now) + 2592000L);
            var AuctionID = packet.GetInt32();
            var Bid = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AUCTION_PLACE_BID [AuctionID={2} Bid={3}]", client.IP, client.Port, AuctionID, Bid);
            if (client.Character.Copper < Bid)
            {
                return;
            }
            DataTable MySQLQuery = new();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT * FROM auctionhouse WHERE auction_id = {Conversions.ToString(AuctionID)};", ref MySQLQuery);
            if (MySQLQuery.Rows.Count != 0 && !Operators.ConditionalCompareObjectLess(Bid, MySQLQuery.Rows[0]["auction_bid"], TextCompare: false))
            {
                if (Operators.ConditionalCompareObjectNotEqual(MySQLQuery.Rows[0]["auction_bidder"], 0, TextCompare: false))
                {
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format(@"INSERT INTO characters_mail (mail_sender, mail_receiver, mail_type, mail_stationary, mail_subject, mail_body, mail_money, mail_COD, mail_time, mail_read) VALUES
                ({0},{1},{2},{3},'{4}','{5}',{6},{7},{8},{9});", AuctionID, MySQLQuery.Rows[0]["auction_bidder"], 2, 62, Operators.ConcatenateObject(MySQLQuery.Rows[0]["auction_itemId"], ":0:0"), "", MySQLQuery.Rows[0]["auction_bid"], 0, MailTime, 0));
                }
                if (Operators.ConditionalCompareObjectEqual(Bid, MySQLQuery.Rows[0]["auction_buyout"], TextCompare: false))
                {
                    var buffer = BitConverter.GetBytes((long)client.Character.GUID);
                    Array.Reverse(buffer);
                    var bodyText = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(BitConverter.ToString(buffer).Replace("-", "") + ":" + Conversions.ToString(Bid) + ":", MySQLQuery.Rows[0]["auction_buyout"]), ":0:0"));
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format(@"INSERT INTO characters_mail (mail_sender, mail_receiver, mail_type, mail_stationary, mail_subject, mail_body, mail_money, mail_COD, mail_time, mail_read) VALUES
                ({0},{1},{2},{3},'{4}','{5}',{6},{7},{8},{9});", AuctionID, MySQLQuery.Rows[0]["auction_owner"], 2, 62, Operators.ConcatenateObject(MySQLQuery.Rows[0]["auction_itemId"], ":0:2"), bodyText, MySQLQuery.Rows[0]["auction_bid"], 0, MailTime, 0));
                    buffer = BitConverter.GetBytes(Conversions.ToLong(MySQLQuery.Rows[0]["auction_owner"]));
                    Array.Reverse(buffer);
                    bodyText = Conversions.ToString(Operators.ConcatenateObject(BitConverter.ToString(buffer).Replace("-", "") + ":" + Conversions.ToString(Bid) + ":", MySQLQuery.Rows[0]["auction_buyout"]));
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format(@"INSERT INTO characters_mail (mail_sender, mail_receiver, mail_type, mail_stationary, mail_subject, mail_body, mail_money, mail_COD, mail_time, mail_read, item_guid) VALUES 
                ({0},{1},{2},{3},'{4}','{5}',{6},{7},{8},{9},{10});", AuctionID, client.Character.GUID, 2, 62, Operators.ConcatenateObject(MySQLQuery.Rows[0]["auction_itemId"], ":0:1"), bodyText, 0, 0, MailTime, 0, MySQLQuery.Rows[0]["auction_itemGuid"]));
                    DataTable MailQuery = new();
                    WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT mail_id FROM characters_mail WHERE mail_receiver = {Conversions.ToString(client.Character.GUID)};", ref MailQuery);
                    var MailID = Conversions.ToInteger(MailQuery.Rows[0]["mail_id"]);
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update($"INSERT INTO mail_items (mail_id, item_guid) VALUES ({MailID},{RuntimeHelpers.GetObjectValue(MySQLQuery.Rows[0]["auction_itemGuid"])});");
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update($"DELETE FROM auctionhouse WHERE auction_id = {Conversions.ToString(AuctionID)};");
                }
                else
                {
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update($"UPDATE auctionhouse SET auction_bidder = {client.Character.GUID}, auction_bid = {Bid} WHERE auction_id = {AuctionID};");
                }
                ref var copper = ref client.Character.Copper;
                copper = (uint)(copper - Bid);
                client.Character.SetUpdateFlag(1176, client.Character.Copper);
                client.Character.SendCharacterUpdate(toNear: false);
                SendAuctionCommandResult(ref client, MySQLQuery.Rows[0].As<int>(field: "auction_id"), AuctionAction.AUCTION_PLACE_BID, AuctionError.AUCTION_OK, 0);
            }
        }
    }

    public void On_CMSG_AUCTION_LIST_ITEMS(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        checked
        {
            if (packet.Data.Length - 1 < 18)
            {
                return;
            }
            packet.GetInt16();
            var GUID = packet.GetUInt64();
            var Unk1 = packet.GetInt32();
            var Name = packet.GetString();
            if (packet.Data.Length - 1 < 18 + Name.Length + 1 + 1 + 4 + 4 + 4 + 4 + 1)
            {
                return;
            }
            var LevelMIN = packet.GetInt8();
            var LevelMAX = packet.GetInt8();
            var itemSlot = packet.GetInt32();
            var itemClass = packet.GetInt32();
            var itemSubClass = packet.GetInt32();
            var itemQuality = packet.GetInt32();
            int mustBeUsable = packet.GetInt8();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AUCTION_LIST_ITEMS [{2} ({3}-{4})]", client.IP, client.Port, Name, LevelMIN, LevelMAX);
            Packets.PacketClass response = new(Opcodes.SMSG_AUCTION_LIST_RESULT);
            var QueryString = $"SELECT auctionhouse.* FROM {WorldServiceLocator._WorldServer.CharacterDatabase.SQLDBName}.auctionhouse, {WorldServiceLocator._WorldServer.WorldDatabase.SQLDBName}.item_template WHERE item_template.entry = auctionhouse.auction_itemId";
            if (Operators.CompareString(Name, "", TextCompare: false) != 0)
            {
                QueryString = $"{QueryString} AND item_template.name LIKE '%{Name}%'";
            }
            if (LevelMIN != 0)
            {
                QueryString = $"{QueryString} AND item_template.itemlevel > {Conversions.ToString(LevelMIN - 1)}";
            }
            if (LevelMAX != 0)
            {
                QueryString = $"{QueryString} AND item_template.itemlevel < {Conversions.ToString(LevelMAX + 1)}";
            }
            if (itemSlot != -1)
            {
                QueryString = $"{QueryString} AND item_template.inventoryType = {Conversions.ToString(itemSlot)}";
            }
            if (itemClass != -1)
            {
                QueryString = $"{QueryString} AND item_template.class = {Conversions.ToString(itemClass)}";
            }
            if (itemSubClass != -1)
            {
                QueryString = $"{QueryString} AND item_template.subclass = {Conversions.ToString(itemSubClass)}";
            }
            if (itemQuality != -1)
            {
                QueryString = $"{QueryString} AND item_template.quality = {Conversions.ToString(itemQuality)}";
            }
            DataTable MySQLQuery = new();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query(QueryString + ";", ref MySQLQuery);
            if (MySQLQuery.Rows.Count > 32)
            {
                response.AddInt32(32);
            }
            else
            {
                response.AddInt32(MySQLQuery.Rows.Count);
            }
            var count = 0;
            var enumerator = Enumerator;
            try
            {
                enumerator = MySQLQuery.Rows.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    DataRow row = (DataRow)enumerator.Current;
                    AuctionListAddItem(ref response, ref row);
                    count++;
                    if (count == 32)
                    {
                        break;
                    }
                }
            }
            finally
            {
                if (enumerator is IDisposable)
                {
                    (enumerator as IDisposable).Dispose();
                }
            }
            response.AddInt32(MySQLQuery.Rows.Count);
            client.Send(ref response);
            response.Dispose();
        }
    }

    public void On_CMSG_AUCTION_LIST_OWNER_ITEMS(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 13)
        {
            packet.GetInt16();
            var GUID = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AUCTION_LIST_OWNER_ITEMS [GUID={2:X}]", client.IP, client.Port, GUID);
            SendAuctionListOwnerItems(ref client);
        }
    }

    public void On_CMSG_AUCTION_LIST_BIDDER_ITEMS(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 21)
        {
            packet.GetInt16();
            var GUID = packet.GetUInt64();
            var Unk = packet.GetInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AUCTION_LIST_BIDDER_ITEMS [GUID={2:X} UNK={3}]", client.IP, client.Port, GUID, Unk);
            SendAuctionListBidderItems(ref client);
        }
    }
}
