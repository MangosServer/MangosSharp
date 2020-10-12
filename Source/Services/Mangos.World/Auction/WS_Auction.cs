using System;
using System.Collections;
using System.Data;
using System.Runtime.CompilerServices;
using Mangos.Common.Enums.AuctionHouse;
using Mangos.Common.Enums.Global;
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
		public int AuctionID;

		public int AuctionFee;

		public int AuctionTax;

		public AuctionHouses GetAuctionSide(ulong GUID)
		{
			if (WorldServiceLocator._ConfigurationProvider.GetConfiguration().GlobalAuction)
			{
				return AuctionHouses.AUCTION_UNDEFINED;
			}
			switch (WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].CreatureInfo.Faction)
			{
			case 29:
			case 68:
			case 104:
				return AuctionHouses.AUCTION_HORDE;
			case 12:
			case 55:
			case 79:
				return AuctionHouses.AUCTION_ALLIANCE;
			default:
				return AuctionHouses.AUCTION_NEUTRAL;
			}
		}

		public int GetAuctionDeposit(ulong GUID, int Price, int ItemCount, int Time)
		{
			if (ItemCount == 0)
			{
				ItemCount = 1;
			}
			return checked(GetAuctionSide(GUID) switch
			{
				AuctionHouses.AUCTION_NEUTRAL => (int)((double)(0.25f * (float)Price * (float)ItemCount) * ((double)Time / 120.0)), 
				AuctionHouses.AUCTION_UNDEFINED => 0, 
				_ => (int)((double)(0.05f * (float)Price * (float)ItemCount) * ((double)Time / 120.0)), 
			});
		}

		public void AuctionCreateMail(MailAuctionAction MailAction, AuctionHouses AuctionLocation, ulong ReceiverGUID, int ItemID, ref Packets.PacketClass packet)
		{
			string queryString = "INSERT INTO characters_mail (";
			string valuesString = ") VALUES (";
			int MailID = packet.GetInt32();
			queryString += "mail_sender,";
			string str = valuesString;
			int num = (int)AuctionLocation;
			valuesString = str + num;
			queryString += "mail_receiver,";
			valuesString += ReceiverGUID;
			queryString += "mail_type,";
			valuesString += "2";
			queryString += "mail_stationary,";
			valuesString += "62";
			queryString += "mail_subject,";
			string str2 = valuesString;
			string str3 = ItemID.ToString();
			num = (int)MailAction;
			valuesString = str2 + str3 + ":0:" + num;
			queryString += "mail_body,";
			valuesString = valuesString ?? "";
			queryString += "mail_money,";
			valuesString = valuesString ?? "";
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

		public void SendShowAuction(ref WS_PlayerData.CharacterObject objCharacter, ulong GUID)
		{
			Packets.PacketClass packet = new Packets.PacketClass(OPCODES.MSG_AUCTION_HELLO);
			packet.AddUInt64(GUID);
			packet.AddInt32((int)GetAuctionSide(GUID));
			objCharacter.client.Send(ref packet);
			packet.Dispose();
		}

		public void AuctionListAddItem(ref Packets.PacketClass packet, ref DataRow Row)
		{
			packet.AddUInt32(Conversions.ToUInteger(Row["auction_id"]));
			uint itemId = Conversions.ToUInteger(Row["auction_itemId"]);
			packet.AddUInt32(itemId);
			checked
			{
				WS_Items.ItemInfo item = ((!WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey((int)itemId)) ? new WS_Items.ItemInfo((int)itemId) : WorldServiceLocator._WorldServer.ITEMDatabase[(int)itemId]);
				packet.AddUInt32(0u);
				packet.AddUInt32((uint)item.RandomProp);
				packet.AddUInt32((uint)item.RandomSuffix);
				packet.AddUInt32(Conversions.ToUInteger(Row["auction_itemCount"]));
				packet.AddInt32(item.Spells[0].SpellCharges);
				packet.AddUInt64(Conversions.ToULong(Row["auction_owner"]));
				packet.AddUInt32(Conversions.ToUInteger(Row["auction_bid"]));
				packet.AddUInt32(Conversions.ToUInteger(Operators.AddObject(Conversion.Fix(Operators.MultiplyObject(Row["auction_bid"], 0.1f)), 1)));
				packet.AddUInt32(Conversions.ToUInteger(Row["auction_buyout"]));
				packet.AddUInt32(Conversions.ToUInteger(Operators.MultiplyObject(Row["auction_timeleft"], 1000)));
				packet.AddUInt64(Conversions.ToULong(Row["auction_bidder"]));
				packet.AddUInt32(Conversions.ToUInteger(Row["auction_bid"]));
			}
		}

		public void SendAuctionCommandResult(ref WS_Network.ClientClass client, int AuctionID, AuctionAction AuctionAction, AuctionError AuctionError, int BidError)
		{
			Packets.PacketClass response = new Packets.PacketClass(OPCODES.SMSG_AUCTION_COMMAND_RESULT);
			response.AddInt32(AuctionID);
			response.AddInt32((int)AuctionAction);
			response.AddInt32((int)AuctionError);
			response.AddInt32(BidError);
			client.Send(ref response);
			response.Dispose();
		}

		public void SendAuctionBidderNotification(ref WS_PlayerData.CharacterObject objCharacter)
		{
			Packets.PacketClass packet = new Packets.PacketClass(OPCODES.SMSG_AUCTION_BIDDER_NOTIFICATION);
			packet.AddInt32(0);
			packet.AddInt32(0);
			packet.AddUInt64(0uL);
			packet.AddInt32(0);
			packet.AddInt32(0);
			packet.AddInt32(0);
			packet.AddInt32(0);
			objCharacter.client.Send(ref packet);
			packet.Dispose();
		}

		public void SendAuctionOwnerNotification(ref WS_PlayerData.CharacterObject objCharacter)
		{
			Packets.PacketClass packet = new Packets.PacketClass(OPCODES.SMSG_AUCTION_OWNER_NOTIFICATION);
			packet.AddInt32(0);
			packet.AddInt32(0);
			packet.AddInt32(0);
			packet.AddInt32(0);
			packet.AddInt32(0);
			packet.AddInt32(0);
			packet.AddInt32(0);
			objCharacter.client.Send(ref packet);
			packet.Dispose();
		}

		public void SendAuctionRemovedNotification(ref WS_PlayerData.CharacterObject objCharacter)
		{
			Packets.PacketClass packet = new Packets.PacketClass(OPCODES.SMSG_AUCTION_REMOVED_NOTIFICATION);
			packet.AddInt32(0);
			packet.AddInt32(0);
			packet.AddInt32(0);
			objCharacter.client.Send(ref packet);
			packet.Dispose();
		}

		public void SendAuctionListOwnerItems(ref WS_Network.ClientClass client)
		{
			Packets.PacketClass response = new Packets.PacketClass(OPCODES.SMSG_AUCTION_OWNER_LIST_RESULT);
			DataTable MySQLQuery = new DataTable();
			WorldServiceLocator._WorldServer.CharacterDatabase.Query("SELECT * FROM auctionhouse WHERE auction_owner = " + Conversions.ToString(client.Character.GUID) + ";", ref MySQLQuery);
			if (MySQLQuery.Rows.Count > 50)
			{
				response.AddInt32(50);
			}
			else
			{
				response.AddInt32(MySQLQuery.Rows.Count);
			}
			int count = 0;
			IEnumerator enumerator = default(IEnumerator);
			try
			{
				enumerator = MySQLQuery.Rows.GetEnumerator();
				while (enumerator.MoveNext())
				{
					DataRow Row = (DataRow)enumerator.Current;
					AuctionListAddItem(ref response, ref Row);
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
			WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_AUCTION_OWNER_LIST_RESULT", client.IP, client.Port);
		}

		public void SendAuctionListBidderItems(ref WS_Network.ClientClass client)
		{
			Packets.PacketClass response = new Packets.PacketClass(OPCODES.SMSG_AUCTION_BIDDER_LIST_RESULT);
			DataTable MySQLQuery = new DataTable();
			WorldServiceLocator._WorldServer.CharacterDatabase.Query("SELECT * FROM auctionhouse WHERE auction_bidder = " + Conversions.ToString(client.Character.GUID) + ";", ref MySQLQuery);
			if (MySQLQuery.Rows.Count > 50)
			{
				response.AddInt32(50);
			}
			else
			{
				response.AddInt32(MySQLQuery.Rows.Count);
			}
			int count = 0;
			IEnumerator enumerator = default(IEnumerator);
			try
			{
				enumerator = MySQLQuery.Rows.GetEnumerator();
				while (enumerator.MoveNext())
				{
					DataRow Row = (DataRow)enumerator.Current;
					AuctionListAddItem(ref response, ref Row);
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
				ulong guid = packet.GetUInt64();
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_AUCTION_HELLO [GUID={2}]", client.IP, client.Port, guid);
				SendShowAuction(ref client.Character, guid);
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
				ulong cGUID = packet.GetUInt64();
				ulong iGUID = packet.GetUInt64();
				int Bid = packet.GetInt32();
				int Buyout = packet.GetInt32();
				int Time = packet.GetInt32();
				int Deposit = GetAuctionDeposit(cGUID, WorldServiceLocator._WorldServer.WORLD_ITEMs[iGUID].ItemInfo.SellPrice, WorldServiceLocator._WorldServer.WORLD_ITEMs[iGUID].StackCount, Time);
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AUCTION_SELL_ITEM [Bid={2} BuyOut={3} Time={4}]", client.IP, client.Port, Bid, Buyout, Time);
				Time *= 60;
				if (WorldServiceLocator._WorldServer.WORLD_ITEMs[iGUID].ItemInfo.IsContainer && !WorldServiceLocator._WorldServer.WORLD_ITEMs[iGUID].IsFree)
				{
					SendAuctionCommandResult(ref client, 0, AuctionAction.AUCTION_SELL_ITEM, AuctionError.CANNOT_BID_YOUR_AUCTION_ERROR, 0);
					return;
				}
				if (client.Character.Copper < Deposit)
				{
					SendAuctionCommandResult(ref client, 0, AuctionAction.AUCTION_SELL_ITEM, AuctionError.AUCTION_NOT_ENOUGHT_MONEY, 0);
					return;
				}
				ref uint copper = ref client.Character.Copper;
				copper = (uint)(unchecked((long)copper) - unchecked((long)Deposit));
				client.Character.ItemREMOVE(iGUID, Destroy: false, Update: true);
				WorldServiceLocator._WorldServer.CharacterDatabase.Update($"INSERT INTO auctionhouse (auction_bid, auction_buyout, auction_timeleft, auction_bidder, auction_owner, auction_itemId, auction_itemGuid, auction_itemCount) VALUES \r\n            ({Bid},{Buyout},{Time},{0},{client.Character.GUID},{WorldServiceLocator._WorldServer.WORLD_ITEMs[iGUID].ItemEntry},{iGUID - WorldServiceLocator._Global_Constants.GUID_ITEM},{WorldServiceLocator._WorldServer.WORLD_ITEMs[iGUID].StackCount});");
				DataTable MySQLQuery = new DataTable();
				WorldServiceLocator._WorldServer.CharacterDatabase.Query("SELECT auction_id FROM auctionhouse WHERE auction_itemGuid = " + Conversions.ToString(iGUID - WorldServiceLocator._Global_Constants.GUID_ITEM) + ";", ref MySQLQuery);
				if (MySQLQuery.Rows.Count != 0)
				{
					SendAuctionCommandResult(ref client, Conversions.ToInteger(MySQLQuery.Rows[0]["auction_id"]), AuctionAction.AUCTION_SELL_ITEM, AuctionError.AUCTION_OK, 0);
				}
			}
		}

		public void On_CMSG_AUCTION_REMOVE_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
		{
			packet.GetInt16();
			ulong GUID = packet.GetUInt64();
			int AuctionID = packet.GetInt32();
			checked
			{
				int MailTime = (int)(unchecked((long)WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now)) + 2592000L);
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AUCTION_REMOVE_ITEM [GUID={2} AuctionID={3}]", client.IP, client.Port, GUID, AuctionID);
				DataTable MySQLQuery = new DataTable();
				WorldServiceLocator._WorldServer.CharacterDatabase.Query("SELECT * FROM auctionhouse WHERE auction_id = " + Conversions.ToString(AuctionID) + ";", ref MySQLQuery);
				if (MySQLQuery.Rows.Count != 0)
				{
					WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("INSERT INTO characters_mail (mail_sender, mail_receiver, mail_type, mail_stationary, mail_subject, mail_body, mail_money, mail_COD, mail_time, mail_read, item_guid) VALUES\r\n            ({0},{1},{2},{3},'{4}','{5}',{6},{7},{8},{9},{10});", AuctionID, MySQLQuery.Rows[0]["auction_owner"], 2, 62, Operators.ConcatenateObject(MySQLQuery.Rows[0]["auction_itemId"], ":0:4"), "", 0, 0, MailTime, 0, MySQLQuery.Rows[0]["auction_itemGuid"]));
					DataTable MailQuery = new DataTable();
					WorldServiceLocator._WorldServer.CharacterDatabase.Query(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("SELECT mail_id FROM characters_mail WHERE mail_receiver = ", MySQLQuery.Rows[0]["auction_owner"]), ";")), ref MailQuery);
					int MailID = Conversions.ToInteger(MailQuery.Rows[0]["mail_id"]);
					WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("INSERT INTO mail_items (mail_id, item_guid) VALUES ({0}, {1});", MailID, RuntimeHelpers.GetObjectValue(MySQLQuery.Rows[0]["auction_itemGuid"])));
					if (Operators.ConditionalCompareObjectNotEqual(MySQLQuery.Rows[0]["auction_bidder"], 0, TextCompare: false))
					{
						WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("INSERT INTO characters_mail (mail_sender, mail_receiver, mail_type, mail_stationary, mail_subject, mail_body, mail_money, mail_COD, mail_time, mail_read, item_guid) VALUES\r\n            ({0},{1},{2},{3},'{4}','{5}',{6},{7},{8},{9},{10});", AuctionID, MySQLQuery.Rows[0]["auction_bidder"], 2, 62, Operators.ConcatenateObject(MySQLQuery.Rows[0]["auction_itemId"], ":0:4"), "", MySQLQuery.Rows[0]["auction_bid"], 0, MailTime, 0, MySQLQuery.Rows[0]["auction_itemGuid"]));
					}
					WorldServiceLocator._WorldServer.CharacterDatabase.Update("DELETE FROM auctionhouse WHERE auction_id = " + Conversions.ToString(AuctionID) + ";");
					SendAuctionCommandResult(ref client, AuctionID, AuctionAction.AUCTION_CANCEL, AuctionError.AUCTION_OK, 0);
				}
			}
		}

		public void On_CMSG_AUCTION_PLACE_BID(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
		{
			packet.GetInt16();
			ulong cGUID = packet.GetUInt64();
			int AuctionID = packet.GetInt32();
			int Bid = packet.GetInt32();
			checked
			{
				int MailTime = (int)(unchecked((long)WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now)) + 2592000L);
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AUCTION_PLACE_BID [AuctionID={2} Bid={3}]", client.IP, client.Port, AuctionID, Bid);
				if (client.Character.Copper < Bid)
				{
					return;
				}
				DataTable MySQLQuery = new DataTable();
				WorldServiceLocator._WorldServer.CharacterDatabase.Query("SELECT * FROM auctionhouse WHERE auction_id = " + Conversions.ToString(AuctionID) + ";", ref MySQLQuery);
				if (MySQLQuery.Rows.Count != 0 && !Operators.ConditionalCompareObjectLess(Bid, MySQLQuery.Rows[0]["auction_bid"], TextCompare: false))
				{
					if (Operators.ConditionalCompareObjectNotEqual(MySQLQuery.Rows[0]["auction_bidder"], 0, TextCompare: false))
					{
						WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("INSERT INTO characters_mail (mail_sender, mail_receiver, mail_type, mail_stationary, mail_subject, mail_body, mail_money, mail_COD, mail_time, mail_read) VALUES\r\n                ({0},{1},{2},{3},'{4}','{5}',{6},{7},{8},{9});", AuctionID, MySQLQuery.Rows[0]["auction_bidder"], 2, 62, Operators.ConcatenateObject(MySQLQuery.Rows[0]["auction_itemId"], ":0:0"), "", MySQLQuery.Rows[0]["auction_bid"], 0, MailTime, 0));
					}
					if (Operators.ConditionalCompareObjectEqual(Bid, MySQLQuery.Rows[0]["auction_buyout"], TextCompare: false))
					{
						byte[] buffer = BitConverter.GetBytes((long)client.Character.GUID);
						Array.Reverse(buffer);
						string bodyText = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(BitConverter.ToString(buffer).Replace("-", "") + ":" + Conversions.ToString(Bid) + ":", MySQLQuery.Rows[0]["auction_buyout"]), ":0:0"));
						WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("INSERT INTO characters_mail (mail_sender, mail_receiver, mail_type, mail_stationary, mail_subject, mail_body, mail_money, mail_COD, mail_time, mail_read) VALUES\r\n                ({0},{1},{2},{3},'{4}','{5}',{6},{7},{8},{9});", AuctionID, MySQLQuery.Rows[0]["auction_owner"], 2, 62, Operators.ConcatenateObject(MySQLQuery.Rows[0]["auction_itemId"], ":0:2"), bodyText, MySQLQuery.Rows[0]["auction_bid"], 0, MailTime, 0));
						buffer = BitConverter.GetBytes(Conversions.ToLong(MySQLQuery.Rows[0]["auction_owner"]));
						Array.Reverse(buffer);
						bodyText = Conversions.ToString(Operators.ConcatenateObject(BitConverter.ToString(buffer).Replace("-", "") + ":" + Conversions.ToString(Bid) + ":", MySQLQuery.Rows[0]["auction_buyout"]));
						WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("INSERT INTO characters_mail (mail_sender, mail_receiver, mail_type, mail_stationary, mail_subject, mail_body, mail_money, mail_COD, mail_time, mail_read, item_guid) VALUES \r\n                ({0},{1},{2},{3},'{4}','{5}',{6},{7},{8},{9},{10});", AuctionID, client.Character.GUID, 2, 62, Operators.ConcatenateObject(MySQLQuery.Rows[0]["auction_itemId"], ":0:1"), bodyText, 0, 0, MailTime, 0, MySQLQuery.Rows[0]["auction_itemGuid"]));
						DataTable MailQuery = new DataTable();
						WorldServiceLocator._WorldServer.CharacterDatabase.Query("SELECT mail_id FROM characters_mail WHERE mail_receiver = " + Conversions.ToString(client.Character.GUID) + ";", ref MailQuery);
						int MailID = Conversions.ToInteger(MailQuery.Rows[0]["mail_id"]);
						WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("INSERT INTO mail_items (mail_id, item_guid) VALUES ({0},{1});", MailID, RuntimeHelpers.GetObjectValue(MySQLQuery.Rows[0]["auction_itemGuid"])));
						WorldServiceLocator._WorldServer.CharacterDatabase.Update("DELETE FROM auctionhouse WHERE auction_id = " + Conversions.ToString(AuctionID) + ";");
					}
					else
					{
						WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE auctionhouse SET auction_bidder = {1}, auction_bid = {2} WHERE auction_id = {0};", AuctionID, client.Character.GUID, Bid));
					}
					ref uint copper = ref client.Character.Copper;
					copper = (uint)(unchecked((long)copper) - unchecked((long)Bid));
					client.Character.SetUpdateFlag(1176, client.Character.Copper);
					client.Character.SendCharacterUpdate(toNear: false);
					SendAuctionCommandResult(ref client, Conversions.ToInteger(MySQLQuery.Rows[0]["auction_id"]), AuctionAction.AUCTION_PLACE_BID, AuctionError.AUCTION_OK, 0);
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
				ulong GUID = packet.GetUInt64();
				int Unk1 = packet.GetInt32();
				string Name = packet.GetString();
				if (packet.Data.Length - 1 < 18 + Name.Length + 1 + 1 + 4 + 4 + 4 + 4 + 1)
				{
					return;
				}
				byte LevelMIN = packet.GetInt8();
				byte LevelMAX = packet.GetInt8();
				int itemSlot = packet.GetInt32();
				int itemClass = packet.GetInt32();
				int itemSubClass = packet.GetInt32();
				int itemQuality = packet.GetInt32();
				int mustBeUsable = packet.GetInt8();
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AUCTION_LIST_ITEMS [{2} ({3}-{4})]", client.IP, client.Port, Name, LevelMIN, LevelMAX);
				Packets.PacketClass response = new Packets.PacketClass(OPCODES.SMSG_AUCTION_LIST_RESULT);
				string QueryString = "SELECT auctionhouse.* FROM " + WorldServiceLocator._WorldServer.CharacterDatabase.SQLDBName + ".auctionhouse, " + WorldServiceLocator._WorldServer.WorldDatabase.SQLDBName + ".item_template WHERE item_template.entry = auctionhouse.auction_itemId";
				if (Operators.CompareString(Name, "", TextCompare: false) != 0)
				{
					QueryString = QueryString + " AND item_template.name LIKE '%" + Name + "%'";
				}
				if (LevelMIN != 0)
				{
					QueryString = QueryString + " AND item_template.itemlevel > " + Conversions.ToString(unchecked((int)LevelMIN) - 1);
				}
				if (LevelMAX != 0)
				{
					QueryString = QueryString + " AND item_template.itemlevel < " + Conversions.ToString(unchecked((int)LevelMAX) + 1);
				}
				if (itemSlot != -1)
				{
					QueryString = QueryString + " AND item_template.inventoryType = " + Conversions.ToString(itemSlot);
				}
				if (itemClass != -1)
				{
					QueryString = QueryString + " AND item_template.class = " + Conversions.ToString(itemClass);
				}
				if (itemSubClass != -1)
				{
					QueryString = QueryString + " AND item_template.subclass = " + Conversions.ToString(itemSubClass);
				}
				if (itemQuality != -1)
				{
					QueryString = QueryString + " AND item_template.quality = " + Conversions.ToString(itemQuality);
				}
				DataTable MySQLQuery = new DataTable();
				WorldServiceLocator._WorldServer.CharacterDatabase.Query(QueryString + ";", ref MySQLQuery);
				if (MySQLQuery.Rows.Count > 32)
				{
					response.AddInt32(32);
				}
				else
				{
					response.AddInt32(MySQLQuery.Rows.Count);
				}
				int count = 0;
				IEnumerator enumerator = default(IEnumerator);
				try
				{
					enumerator = MySQLQuery.Rows.GetEnumerator();
					while (enumerator.MoveNext())
					{
						DataRow Row = (DataRow)enumerator.Current;
						AuctionListAddItem(ref response, ref Row);
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
				ulong GUID = packet.GetUInt64();
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AUCTION_LIST_OWNER_ITEMS [GUID={2:X}]", client.IP, client.Port, GUID);
				SendAuctionListOwnerItems(ref client);
			}
		}

		public void On_CMSG_AUCTION_LIST_BIDDER_ITEMS(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
		{
			if (checked(packet.Data.Length - 1) >= 21)
			{
				packet.GetInt16();
				ulong GUID = packet.GetUInt64();
				long Unk = packet.GetInt64();
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AUCTION_LIST_BIDDER_ITEMS [GUID={2:X} UNK={3}]", client.IP, client.Port, GUID, Unk);
				SendAuctionListBidderItems(ref client);
			}
		}
	}
}
