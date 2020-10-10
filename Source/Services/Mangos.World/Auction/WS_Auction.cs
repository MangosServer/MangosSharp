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
using Mangos.Common.Enums.AuctionHouse;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Misc;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Objects;
using Mangos.World.Player;
using Mangos.World.Server;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Auction
{
    public class WS_Auction
    {

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public int AuctionID;
        public int AuctionFee;
        public int AuctionTax;

        public AuctionHouses GetAuctionSide(ulong GUID)
        {
            if (WorldServiceLocator._WorldServer.Config.GlobalAuction)
                return AuctionHouses.AUCTION_UNDEFINED;
            switch (WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].CreatureInfo.Faction)
            {
                case 29:
                case 68:
                case 104:
                    {
                        return AuctionHouses.AUCTION_HORDE;
                    }

                case 12:
                case 55:
                case 79:
                    {
                        return AuctionHouses.AUCTION_ALLIANCE;
                    }

                default:
                    {
                        return AuctionHouses.AUCTION_NEUTRAL;
                    }
            }
        }

        public int GetAuctionDeposit(ulong GUID, int Price, int ItemCount, int Time)
        {
            if (ItemCount == 0)
                ItemCount = 1;
            switch (GetAuctionSide(GUID))
            {
                case var @case when @case == AuctionHouses.AUCTION_NEUTRAL:
                case var case1 when case1 == AuctionHouses.AUCTION_BLACKWATER:
                    {
                        return (int)Conversion.Fix(0.25f * Price * ItemCount * (Time / 120d));
                    }

                case var case2 when case2 == AuctionHouses.AUCTION_UNDEFINED:
                    {
                        return 0;
                    }

                default:
                    {
                        return (int)Conversion.Fix(0.05f * Price * ItemCount * (Time / 120d));
                    }
            }
        }

        public void AuctionCreateMail(MailAuctionAction MailAction, AuctionHouses AuctionLocation, ulong ReceiverGUID, int ItemID, ref Packets.PacketClass packet)
        {
            string queryString = "INSERT INTO characters_mail (";
            string valuesString = ") VALUES (";
            int MailID = packet.GetInt32();
            queryString += "mail_sender,";
            valuesString += Microsoft.VisualBasic.CompilerServices.Conversions.ToInteger(AuctionLocation).ToString();
            queryString += "mail_receiver,";
            valuesString += ReceiverGUID.ToString();
            queryString += "mail_type,";
            valuesString += "2";
            queryString += "mail_stationary,";
            valuesString += "62";
            queryString += "mail_subject,";
            valuesString += ItemID + ":0:" + MailAction;
            queryString += "mail_body,";
            valuesString += "";
            queryString += "mail_money,";
            valuesString += "";
            queryString += "mail_COD,";
            valuesString += "0";
            queryString += "mail_time,";
            valuesString += "30";
            queryString += "mail_read,";
            valuesString += "0";
            queryString += "item_guid,";
            valuesString += ");";
            WorldServiceLocator._WorldServer.CharacterDatabase.Update(queryString + valuesString);
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public void SendShowAuction(ref WS_PlayerData.CharacterObject objCharacter, ulong GUID)
        {
            var packet = new Packets.PacketClass(OPCODES.MSG_AUCTION_HELLO);
            packet.AddUInt64(GUID);
            packet.AddInt32((int)GetAuctionSide(GUID));          // AuctionID (on this is based the fees shown in client side)
            objCharacter.client.Send(packet);
            packet.Dispose();
        }

        public void AuctionListAddItem(ref Packets.PacketClass packet, DataRow Row)
        {
            packet.AddUInt32(Conversions.ToUInteger(Row["auction_id"]));
            uint itemId = Conversions.ToUInteger(Row["auction_itemId"]);
            packet.AddUInt32(itemId);
            WS_Items.ItemInfo item;
            if (WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey((int)itemId))
            {
                item = WorldServiceLocator._WorldServer.ITEMDatabase[(int)itemId];
            }
            else
            {
                item = new WS_Items.ItemInfo((int)itemId);
            }

            packet.AddUInt32(0U);                                        // PERM_ENCHANMENT_SLOT (Not sure if we have to do anything here)
            packet.AddUInt32((uint)item.RandomProp);                          // Item Random Property ID
            packet.AddUInt32((uint)item.RandomSuffix);                        // SuffixFactor
            packet.AddUInt32(Conversions.ToUInteger(Row["auction_itemCount"]));            // Item Count
            packet.AddInt32(item.Spells[0].SpellCharges);               // Item Spell Charges
            packet.AddUInt64(Conversions.ToULong(Row["auction_owner"]));                // Bid Owner
            packet.AddUInt32(Conversions.ToUInteger(Row["auction_bid"]));                  // Bid Price
            packet.AddUInt32(Conversions.ToUInteger(Operators.AddObject(Conversion.Fix(Operators.MultiplyObject(Row["auction_bid"], 0.1f)), 1)));  // Bid Step
            packet.AddUInt32(Conversions.ToUInteger(Row["auction_buyout"]));               // Bid Buyout
            packet.AddUInt32(Conversions.ToUInteger(Operators.MultiplyObject(Row["auction_timeleft"], 1000)));      // Bid Timeleft (in ms)
            packet.AddUInt64(Conversions.ToULong(Row["auction_bidder"]));               // Bidder GUID
            packet.AddUInt32(Conversions.ToUInteger(Row["auction_bid"]));                  // Bidder Current Bid
        }

        public void SendAuctionCommandResult(ref WS_Network.ClientClass client, int AuctionID, AuctionAction AuctionAction, AuctionError AuctionError, int BidError)
        {
            var response = new Packets.PacketClass(OPCODES.SMSG_AUCTION_COMMAND_RESULT);
            response.AddInt32(AuctionID);
            response.AddInt32((int)AuctionAction);
            response.AddInt32((int)AuctionError);
            // If AuctionError <> AuctionError.AUCTION_OK AndAlso AuctionAction <> AuctionAction.AUCTION_SELL_ITEM Then
            response.AddInt32(BidError);
            client.Send(response);
            response.Dispose();
        }

        public void SendAuctionBidderNotification(ref WS_PlayerData.CharacterObject objCharacter)
        {
            // Displays: "Outbid on <Item>."

            var packet = new Packets.PacketClass(OPCODES.SMSG_AUCTION_BIDDER_NOTIFICATION);
            packet.AddInt32(0);          // Location
            packet.AddInt32(0);          // AutionID
            packet.AddUInt64(0UL);         // BidderGUID
            packet.AddInt32(0);          // BidSum
            packet.AddInt32(0);          // Diff
            packet.AddInt32(0);          // ItemID
            packet.AddInt32(0);          // RandomProperyID
            objCharacter.client.Send(packet);
            packet.Dispose();
        }

        public void SendAuctionOwnerNotification(ref WS_PlayerData.CharacterObject objCharacter)
        {
            // Displays: "Your auction of <Item> sold."

            var packet = new Packets.PacketClass(OPCODES.SMSG_AUCTION_OWNER_NOTIFICATION);
            packet.AddInt32(0);          // AutionID
            packet.AddInt32(0);          // Bid
            packet.AddInt32(0);
            packet.AddInt32(0);
            packet.AddInt32(0);
            packet.AddInt32(0);          // ItemID
            packet.AddInt32(0);          // RandomProperyID
            objCharacter.client.Send(packet);
            packet.Dispose();
        }

        public void SendAuctionRemovedNotification(ref WS_PlayerData.CharacterObject objCharacter)
        {
            // Displays: "Auction of <Item> canceled by the seller."

            var packet = new Packets.PacketClass(OPCODES.SMSG_AUCTION_REMOVED_NOTIFICATION);
            packet.AddInt32(0);          // AutionID
            packet.AddInt32(0);          // ItemID
            packet.AddInt32(0);          // RandomProperyID
            objCharacter.client.Send(packet);
            packet.Dispose();
        }

        public void SendAuctionListOwnerItems(ref WS_Network.ClientClass client)
        {
            var response = new Packets.PacketClass(OPCODES.SMSG_AUCTION_OWNER_LIST_RESULT);
            var MySQLQuery = new DataTable();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query("SELECT * FROM auctionhouse WHERE auction_owner = " + client.Character.GUID + ";", ref MySQLQuery);
            if (MySQLQuery.Rows.Count > 50)
            {
                response.AddInt32(50);                               // Count
            }
            else
            {
                response.AddInt32(MySQLQuery.Rows.Count);
            }            // Count

            int count = 0;
            foreach (DataRow Row in MySQLQuery.Rows)
            {
                AuctionListAddItem(ref response, Row);
                count += 1;
                if (count == 50)
                    break;
            }

            response.AddInt32(MySQLQuery.Rows.Count);            // AllCount
            client.Send(response);
            response.Dispose();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_AUCTION_OWNER_LIST_RESULT", client.IP, client.Port);
        }

        public void SendAuctionListBidderItems(ref WS_Network.ClientClass client)
        {
            var response = new Packets.PacketClass(OPCODES.SMSG_AUCTION_BIDDER_LIST_RESULT);
            var MySQLQuery = new DataTable();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query("SELECT * FROM auctionhouse WHERE auction_bidder = " + client.Character.GUID + ";", ref MySQLQuery);
            if (MySQLQuery.Rows.Count > 50)
            {
                response.AddInt32(50);                               // Count
            }
            else
            {
                response.AddInt32(MySQLQuery.Rows.Count);
            }            // Count

            int count = 0;
            foreach (DataRow Row in MySQLQuery.Rows)
            {
                AuctionListAddItem(ref response, Row);
                count += 1;
                if (count == 50)
                    break;
            }

            response.AddInt32(MySQLQuery.Rows.Count);            // AllCount
            client.Send(response);
            response.Dispose();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_AUCTION_BIDDER_LIST_RESULT", client.IP, client.Port);
        }


        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public void On_MSG_AUCTION_HELLO(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 13)
                return;
            packet.GetInt16();
            ulong guid = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_AUCTION_HELLO [GUID={2}]", client.IP, client.Port, guid);
            SendShowAuction(ref client.Character, guid);
        }

        public void On_CMSG_AUCTION_SELL_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 33)
                return;
            packet.GetInt16();
            ulong cGUID = packet.GetUInt64();
            ulong iGUID = packet.GetUInt64();
            int Bid = packet.GetInt32();
            int Buyout = packet.GetInt32();
            int Time = packet.GetInt32();

            // DONE: Calculate deposit with time in hours
            int Deposit = GetAuctionDeposit(cGUID, WorldServiceLocator._WorldServer.WORLD_ITEMs[iGUID].ItemInfo.SellPrice, WorldServiceLocator._WorldServer.WORLD_ITEMs[iGUID].StackCount, Time);
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AUCTION_SELL_ITEM [Bid={2} BuyOut={3} Time={4}]", client.IP, client.Port, Bid, Buyout, Time);

            // DONE: Convert time in seconds left
            Time *= 60;

            // DONE: Check if item is bag with items
            if (WorldServiceLocator._WorldServer.WORLD_ITEMs[iGUID].ItemInfo.IsContainer && !WorldServiceLocator._WorldServer.WORLD_ITEMs[iGUID].IsFree)
            {
                SendAuctionCommandResult(ref client, 0, AuctionAction.AUCTION_SELL_ITEM, AuctionError.CANNOT_BID_YOUR_AUCTION_ERROR, 0);
                return;
            }
            // DONE: Check deposit
            if (client.Character.Copper < Deposit)
            {
                SendAuctionCommandResult(ref client, 0, AuctionAction.AUCTION_SELL_ITEM, AuctionError.AUCTION_NOT_ENOUGHT_MONEY, 0);
                return;
            }

            // DONE: Get 5% deposit per 2h in auction (http://www.wowwiki.com/Formulas:Auction_House)
            client.Character.Copper = (uint)(client.Character.Copper - Deposit);

            // DONE: Remove item from inventory
            client.Character.ItemREMOVE(iGUID, false, true);

            // DONE: Add auction entry into table
            WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format(@"INSERT INTO auctionhouse (auction_bid, auction_buyout, auction_timeleft, auction_bidder, auction_owner, auction_itemId, auction_itemGuid, auction_itemCount) VALUES 
            ({0},{1},{2},{3},{4},{5},{6},{7});", Bid, Buyout, Time, 0, client.Character.GUID, WorldServiceLocator._WorldServer.WORLD_ITEMs[iGUID].ItemEntry, iGUID - WorldServiceLocator._Global_Constants.GUID_ITEM, WorldServiceLocator._WorldServer.WORLD_ITEMs[iGUID].StackCount));

            // DONE: Send result packet
            var MySQLQuery = new DataTable();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query("SELECT auction_id FROM auctionhouse WHERE auction_itemGuid = " + (iGUID - WorldServiceLocator._Global_Constants.GUID_ITEM) + ";", ref MySQLQuery);
            if (MySQLQuery.Rows.Count == 0)
                return;
            SendAuctionCommandResult(ref client, (int)MySQLQuery.Rows[0]["auction_id"], AuctionAction.AUCTION_SELL_ITEM, AuctionError.AUCTION_OK, 0);

            // NOTE: Not needed, client would request it
            // SendAuctionListOwnerItems(Client)
        }

        public void On_CMSG_AUCTION_REMOVE_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            packet.GetInt16();
            ulong GUID = packet.GetUInt64();
            int AuctionID = packet.GetInt32();
            int MailTime = (int)WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now) + (int)TimeConstant.DAY * 30;
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AUCTION_REMOVE_ITEM [GUID={2} AuctionID={3}]", client.IP, client.Port, GUID, AuctionID);
            var MySQLQuery = new DataTable();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query("SELECT * FROM auctionhouse WHERE auction_id = " + AuctionID + ";", ref MySQLQuery);
            if (MySQLQuery.Rows.Count == 0)
                return;

            // DONE: Return item to owner
            // TODO: Call the correct auction house location
            WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format(@"INSERT INTO characters_mail (mail_sender, mail_receiver, mail_type, mail_stationary, mail_subject, mail_body, mail_money, mail_COD, mail_time, mail_read, item_guid) VALUES
            ({0},{1},{2},{3},'{4}','{5}',{6},{7},{8},{9},{10});", AuctionID, MySQLQuery.Rows[0]["auction_owner"], 2, 62, Operators.ConcatenateObject(MySQLQuery.Rows[0]["auction_itemId"], ":0:4"), "", 0, 0, MailTime, 0, MySQLQuery.Rows[0]["auction_itemGuid"]));
            var MailQuery = new DataTable();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query(Operators.ConcatenateObject(Operators.ConcatenateObject("SELECT mail_id FROM characters_mail WHERE mail_receiver = ", MySQLQuery.Rows[0]["auction_owner"]), ";").ToString(), ref MailQuery);
            int MailID = Conversions.ToInteger(MailQuery.Rows[0]["mail_id"]);
            WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("INSERT INTO mail_items (mail_id, item_guid) VALUES ({0}, {1});", MailID, MySQLQuery.Rows[0]["auction_itemGuid"]));

            // DONE: Return money to bidder
            // TODO: Call the correct auction house location
            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(MySQLQuery.Rows[0]["auction_bidder"], 0, false)))
                WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format(@"INSERT INTO characters_mail (mail_sender, mail_receiver, mail_type, mail_stationary, mail_subject, mail_body, mail_money, mail_COD, mail_time, mail_read, item_guid) VALUES
            ({0},{1},{2},{3},'{4}','{5}',{6},{7},{8},{9},{10});", AuctionID, MySQLQuery.Rows[0]["auction_bidder"], 2, 62, Operators.ConcatenateObject(MySQLQuery.Rows[0]["auction_itemId"], ":0:4"), "", MySQLQuery.Rows[0]["auction_bid"], 0, MailTime, 0, MySQLQuery.Rows[0]["auction_itemGuid"]));

            // DONE: Remove from auction table
            WorldServiceLocator._WorldServer.CharacterDatabase.Update("DELETE FROM auctionhouse WHERE auction_id = " + AuctionID + ";");
            SendAuctionCommandResult(ref client, AuctionID, AuctionAction.AUCTION_CANCEL, AuctionError.AUCTION_OK, 0);
            // SendNotify(client) 'Notifies the client that they have mail

            // NOTE: Not needed, client would request it
            // SendAuctionListOwnerItems(Client)
        }

        public void On_CMSG_AUCTION_PLACE_BID(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            packet.GetInt16();
            ulong cGUID = packet.GetUInt64();
            int AuctionID = packet.GetInt32();
            int Bid = packet.GetInt32();
            int MailTime = (int)WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now) + (int)TimeConstant.DAY * 30;
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AUCTION_PLACE_BID [AuctionID={2} Bid={3}]", client.IP, client.Port, AuctionID, Bid);
            if (client.Character.Copper < Bid)
                return;
            var MySQLQuery = new DataTable();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query("SELECT * FROM auctionhouse WHERE auction_id = " + AuctionID + ";", MySQLQuery);
            if (MySQLQuery.Rows.Count == 0)
                return;
            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectLess(Bid, MySQLQuery.Rows[0]["auction_bid"], false)))
                return;
            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(MySQLQuery.Rows[0]["auction_bidder"], 0, false)))
            {
                // DONE: Send outbid mail
                // TODO: Call the correct auction house location
                WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format(@"INSERT INTO characters_mail (mail_sender, mail_receiver, mail_type, mail_stationary, mail_subject, mail_body, mail_money, mail_COD, mail_time, mail_read) VALUES
                ({0},{1},{2},{3},'{4}','{5}',{6},{7},{8},{9});", AuctionID, MySQLQuery.Rows[0]["auction_bidder"], 2, 62, Operators.ConcatenateObject(MySQLQuery.Rows[0]["auction_itemId"], ":0:0"), "", MySQLQuery.Rows[0]["auction_bid"], 0, MailTime, 0));
            }

            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(Bid, MySQLQuery.Rows[0]["auction_buyout"], false)))
            {
                // Do buyout
                string bodyText;
                byte[] buffer;

                // DONE: Send auction succ to owner (PurchasedBy:SalePrice:BuyoutPrice:Deposit:AuctionHouseCut)
                // TODO: Call the correct auction house location
                buffer = BitConverter.GetBytes(Conversions.ToLong(client.Character.GUID));
                Array.Reverse(buffer);
                bodyText = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(BitConverter.ToString(buffer).Replace("-", "") + ":" + Bid + ":", MySQLQuery.Rows[0]["auction_buyout"]), ":0:0"));
                WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format(@"INSERT INTO characters_mail (mail_sender, mail_receiver, mail_type, mail_stationary, mail_subject, mail_body, mail_money, mail_COD, mail_time, mail_read) VALUES
                ({0},{1},{2},{3},'{4}','{5}',{6},{7},{8},{9});", AuctionID, MySQLQuery.Rows[0]["auction_owner"], 2, 62, Operators.ConcatenateObject(MySQLQuery.Rows[0]["auction_itemId"], ":0:2"), bodyText, MySQLQuery.Rows[0]["auction_bid"], 0, MailTime, 0));

                // DONE: Send auction won to bidder with item (SoldBy:SalePrice:BuyoutPrice)
                // TODO: Call the correct auction house location
                buffer = BitConverter.GetBytes(Conversions.ToLong(MySQLQuery.Rows[0]["auction_owner"]));
                Array.Reverse(buffer);
                bodyText = Conversions.ToString(Operators.ConcatenateObject(BitConverter.ToString(buffer).Replace("-", "") + ":" + Bid + ":", MySQLQuery.Rows[0]["auction_buyout"]));
                WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format(@"INSERT INTO characters_mail (mail_sender, mail_receiver, mail_type, mail_stationary, mail_subject, mail_body, mail_money, mail_COD, mail_time, mail_read, item_guid) VALUES 
                ({0},{1},{2},{3},'{4}','{5}',{6},{7},{8},{9},{10});", AuctionID, client.Character.GUID, 2, 62, Operators.ConcatenateObject(MySQLQuery.Rows[0]["auction_itemId"], ":0:1"), bodyText, 0, 0, MailTime, 0, MySQLQuery.Rows[0]["auction_itemGuid"]));
                var MailQuery = new DataTable();
                WorldServiceLocator._WorldServer.CharacterDatabase.Query("SELECT mail_id FROM characters_mail WHERE mail_receiver = " + client.Character.GUID + ";", MailQuery);
                int MailID = Conversions.ToInteger(MailQuery.Rows[0]["mail_id"]);
                WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("INSERT INTO mail_items (mail_id, item_guid) VALUES ({0},{1});", MailID, MySQLQuery.Rows[0]["auction_itemGuid"]));

                // DONE: Remove auction
                WorldServiceLocator._WorldServer.CharacterDatabase.Update("DELETE FROM auctionhouse WHERE auction_id = " + AuctionID + ";");
            }
            // SendNotify(client) 'Notifies the Client that they have mail
            else
            {
                // Do bid
                // NOTE: Here is using external timer or web page script to count what time is left and to do the actual buy

                // DONE: Set bidder in auction table, update bid value
                WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE auctionhouse SET auction_bidder = {1}, auction_bid = {2} WHERE auction_id = {0};", AuctionID, client.Character.GUID, Bid));
            }

            client.Character.Copper = (uint)(client.Character.Copper - Bid);
            client.Character.SetUpdateFlag((int)EPlayerFields.PLAYER_FIELD_COINAGE, client.Character.Copper);
            client.Character.SendCharacterUpdate(false);

            // DONE: Send result packet
            SendAuctionCommandResult(ref client, (int)MySQLQuery.Rows[0]["auction_id"], AuctionAction.AUCTION_PLACE_BID, AuctionError.AUCTION_OK, 0);

            // NOTE: Not needed, client would request it
            // SendAuctionListBidderItems(Client)
        }

        public void On_CMSG_AUCTION_LIST_ITEMS(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 18)
                return;
            packet.GetInt16();
            ulong GUID = packet.GetUInt64();
            int Unk1 = packet.GetInt32();           // Always 0... may be page?
            string Name = packet.GetString();
            if (packet.Data.Length - 1 < 18 + Name.Length + 1 + 1 + 4 + 4 + 4 + 4 + 1)
                return;
            byte LevelMIN = packet.GetInt8();           // 0 if not used
            byte LevelMAX = packet.GetInt8();           // 0 if not used
            int itemSlot = packet.GetInt32();       // &H FF FF FF FF
            int itemClass = packet.GetInt32();      // &H FF FF FF FF
            int itemSubClass = packet.GetInt32();   // &H FF FF FF FF
            int itemQuality = packet.GetInt32();    // &H FF FF FF FF
            int mustBeUsable = packet.GetInt8();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AUCTION_LIST_ITEMS [{2} ({3}-{4})]", client.IP, client.Port, Name, LevelMIN, LevelMAX);
            var response = new Packets.PacketClass(OPCODES.SMSG_AUCTION_LIST_RESULT);
            string QueryString = "SELECT auctionhouse.* FROM " + WorldServiceLocator._WorldServer.CharacterDatabase.SQLDBName + ".auctionhouse, " + WorldServiceLocator._WorldServer.WorldDatabase.SQLDBName + ".item_template WHERE item_template.entry = auctionhouse.auction_itemId";
            if (!string.IsNullOrEmpty(Name))
                QueryString += " AND item_template.name LIKE '%" + Name + "%'";
            if (LevelMIN != 0)
                QueryString += " AND item_template.itemlevel > " + (LevelMIN - 1);
            if (LevelMAX != 0)
                QueryString += " AND item_template.itemlevel < " + (LevelMAX + 1);
            if (itemSlot != -1)
                QueryString += " AND item_template.inventoryType = " + itemSlot;
            if (itemClass != -1)
                QueryString += " AND item_template.class = " + itemClass;
            if (itemSubClass != -1)
                QueryString += " AND item_template.subclass = " + itemSubClass;
            if (itemQuality != -1)
                QueryString += " AND item_template.quality = " + itemQuality;
            var MySQLQuery = new DataTable();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query(QueryString + ";", MySQLQuery);
            if (MySQLQuery.Rows.Count > 32)
            {
                response.AddInt32(32);                               // Count
            }
            else
            {
                response.AddInt32(MySQLQuery.Rows.Count);
            }            // Count

            int count = 0;
            foreach (DataRow Row in MySQLQuery.Rows)
            {
                AuctionListAddItem(ref response, ref Row);
                count += 1;
                if (count == 32)
                    break;
            }

            response.AddInt32(MySQLQuery.Rows.Count);            // AllCount
            client.Send(response);
            response.Dispose();
        }

        public void On_CMSG_AUCTION_LIST_OWNER_ITEMS(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 13)
                return;
            packet.GetInt16();
            ulong GUID = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AUCTION_LIST_OWNER_ITEMS [GUID={2:X}]", client.IP, client.Port, GUID);
            SendAuctionListOwnerItems(ref client);
        }

        public void On_CMSG_AUCTION_LIST_BIDDER_ITEMS(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 21)
                return;
            packet.GetInt16();
            ulong GUID = packet.GetUInt64();
            long Unk = packet.GetInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AUCTION_LIST_BIDDER_ITEMS [GUID={2:X} UNK={3}]", client.IP, client.Port, GUID, Unk);
            SendAuctionListBidderItems(ref client);
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
    }
}