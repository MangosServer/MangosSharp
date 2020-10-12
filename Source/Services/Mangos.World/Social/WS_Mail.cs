using System;
using System.Data;
using System.Runtime.CompilerServices;
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
		public const int ITEM_MAILTEXT_ITEMID = 889;

		public void On_CMSG_MAIL_RETURN_TO_SENDER(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
		{
			checked
			{
				if (packet.Data.Length - 1 >= 17)
				{
					packet.GetInt16();
					ulong GameObjectGUID = packet.GetUInt64();
					int MailID = packet.GetInt32();
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MAIL_RETURN_TO_SENDER [MailID={2}]", client.IP, client.Port, MailID);
					int MailTime = (int)(unchecked((long)WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now)) + 2592000L);
					WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_mail SET mail_time = {1}, mail_read = 0, mail_receiver = (mail_receiver + mail_sender), mail_sender = (mail_receiver - mail_sender), mail_receiver = (mail_receiver - mail_sender) WHERE mail_id = {0};", MailID, MailTime));
					Packets.PacketClass response = new Packets.PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT);
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
				ulong GameObjectGUID = packet.GetUInt64();
				int MailID = packet.GetInt32();
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MAIL_DELETE [MailID={2}]", client.IP, client.Port, MailID);
				WorldServiceLocator._WorldServer.CharacterDatabase.Update($"DELETE FROM characters_mail WHERE mail_id = {MailID};");
				Packets.PacketClass response = new Packets.PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT);
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
					ulong GameObjectGUID = packet.GetUInt64();
					int MailID = packet.GetInt32();
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MAIL_MARK_AS_READ [MailID={2}]", client.IP, client.Port, MailID);
					int MailTime = (int)(unchecked((long)WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now)) + 259200L);
					WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_mail SET mail_read = 1, mail_time = {1} WHERE mail_id = {0} AND mail_read < 2;", MailID, MailTime));
				}
			}
		}

		public void On_MSG_QUERY_NEXT_MAIL_TIME(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
		{
			WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_QUERY_NEXT_MAIL_TIME", client.IP, client.Port);
			DataTable MySQLQuery = new DataTable();
			WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT COUNT(*) FROM characters_mail WHERE mail_read = 0 AND mail_receiver = {client.Character.GUID} AND mail_time > {WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now)};", ref MySQLQuery);
			if (Operators.ConditionalCompareObjectGreater(MySQLQuery.Rows[0][0], 0, TextCompare: false))
			{
				Packets.PacketClass response2 = new Packets.PacketClass(OPCODES.MSG_QUERY_NEXT_MAIL_TIME);
				response2.AddInt32(0);
				client.Send(ref response2);
				response2.Dispose();
			}
			else
			{
				Packets.PacketClass response = new Packets.PacketClass(OPCODES.MSG_QUERY_NEXT_MAIL_TIME);
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
				ulong GameObjectGUID = packet.GetUInt64();
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GET_MAIL_LIST [GUID={2:X}]", client.IP, client.Port, GameObjectGUID);
				try
				{
					DataTable MySQLQuery = new DataTable();
					WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT mail_id FROM characters_mail WHERE mail_time < {WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now)};", ref MySQLQuery);
					if (MySQLQuery.Rows.Count > 0)
					{
						byte b = (byte)(MySQLQuery.Rows.Count - 1);
						byte j = 0;
						while (unchecked((uint)j <= (uint)b))
						{
							WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("DELETE FROM characters_mail WHERE mail_id = {0};", RuntimeHelpers.GetObjectValue(MySQLQuery.Rows[j]["mail_id"])));
							j = (byte)unchecked((uint)(j + 1));
						}
					}
					WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT * FROM characters_mail WHERE mail_receiver = {client.Character.GUID};", ref MySQLQuery);
					Packets.PacketClass response = new Packets.PacketClass(OPCODES.SMSG_MAIL_LIST_RESULT);
					response.AddInt8((byte)MySQLQuery.Rows.Count);
					if (MySQLQuery.Rows.Count > 0)
					{
						byte b2 = (byte)(MySQLQuery.Rows.Count - 1);
						byte i = 0;
						while (unchecked((uint)i <= (uint)b2))
						{
							response.AddInt32(Conversions.ToInteger(MySQLQuery.Rows[i]["mail_id"]));
							response.AddInt8(Conversions.ToByte(MySQLQuery.Rows[i]["mail_type"]));
							object left = MySQLQuery.Rows[i]["mail_type"];
							if (Operators.ConditionalCompareObjectEqual(left, MailTypeInfo.NORMAL, TextCompare: false))
							{
								response.AddUInt64(Conversions.ToULong(MySQLQuery.Rows[i]["mail_sender"]));
							}
							else if (Operators.ConditionalCompareObjectEqual(left, MailTypeInfo.AUCTION, TextCompare: false))
							{
								response.AddInt32(Conversions.ToInteger(MySQLQuery.Rows[i]["mail_sender"]));
							}
							response.AddString(Conversions.ToString(MySQLQuery.Rows[i]["mail_subject"]));
							if (Operators.ConditionalCompareObjectNotEqual(MySQLQuery.Rows[i]["mail_body"], "", TextCompare: false))
							{
								response.AddInt32(Conversions.ToInteger(MySQLQuery.Rows[i]["mail_id"]));
							}
							else
							{
								response.AddInt32(0);
							}
							response.AddInt32(0);
							response.AddInt32(Conversions.ToInteger(MySQLQuery.Rows[i]["mail_stationary"]));
							if (decimal.Compare(new decimal(Conversions.ToULong(MySQLQuery.Rows[i]["item_guid"])), 0m) > 0)
							{
								ItemObject tmpItem = WorldServiceLocator._WS_Items.LoadItemByGUID(Conversions.ToULong(MySQLQuery.Rows[i]["item_guid"]));
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
							response.AddUInt32(Conversions.ToUInteger(MySQLQuery.Rows[i]["mail_money"]));
							response.AddUInt32(Conversions.ToUInteger(MySQLQuery.Rows[i]["mail_COD"]));
							response.AddInt32(Conversions.ToInteger(MySQLQuery.Rows[i]["mail_read"]));
							response.AddSingle((float)((double)(Conversions.ToUInteger(MySQLQuery.Rows[i]["mail_time"]) - WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now)) / 86400.0));
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
					Exception e = ex;
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
				ulong GameObjectGUID = packet.GetUInt64();
				int MailID = packet.GetInt32();
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MAIL_TAKE_ITEM [MailID={2}]", client.IP, client.Port, MailID);
				try
				{
					DataTable MySQLQuery = new DataTable();
					WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT mail_cod, mail_sender, item_guid FROM characters_mail WHERE mail_id = {MailID} AND mail_receiver = {client.Character.GUID};", ref MySQLQuery);
					if (MySQLQuery.Rows.Count == 0)
					{
						Packets.PacketClass response4 = new Packets.PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT);
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
						Packets.PacketClass noMoney = new Packets.PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT);
						noMoney.AddInt32(MailID);
						noMoney.AddInt32(0);
						noMoney.AddInt32(3);
						client.Send(ref noMoney);
						noMoney.Dispose();
						return;
					}
					ref uint copper = ref client.Character.Copper;
					copper = Conversions.ToUInteger(Operators.SubtractObject(copper, MySQLQuery.Rows[0]["mail_cod"]));
					WorldServiceLocator._WorldServer.CharacterDatabase.Update($"UPDATE characters_mail SET mail_cod = 0 WHERE mail_id = {MailID};");
					int MailTime = (int)(unchecked((long)WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now)) + 2592000L);
					WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("INSERT INTO characters_mail (mail_sender, mail_receiver, mail_subject, mail_body, mail_item_guid, mail_money, mail_COD, mail_time, mail_read, mail_type) VALUES \r\n                        ({0},{1},'{2}','{3}',{4},{5},{6},{7},{8},{9});", client.Character.GUID, MySQLQuery.Rows[0]["mail_sender"], "", "", 0, MySQLQuery.Rows[0]["mail_cod"], 0, MailTime, MailReadInfo.COD, 0));
					goto IL_02b9;
					IL_02b9:
					if (Operators.ConditionalCompareObjectEqual(MySQLQuery.Rows[0]["item_guid"], 0, TextCompare: false))
					{
						Packets.PacketClass response3 = new Packets.PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT);
						response3.AddInt32(MailID);
						response3.AddInt32(2);
						response3.AddInt32(6);
						client.Send(ref response3);
						response3.Dispose();
						return;
					}
					ItemObject tmpItem = WorldServiceLocator._WS_Items.LoadItemByGUID(Conversions.ToULong(MySQLQuery.Rows[0]["item_guid"]));
					tmpItem.OwnerGUID = client.Character.GUID;
					tmpItem.Save();
					if (client.Character.ItemADD(ref tmpItem))
					{
						WorldServiceLocator._WorldServer.CharacterDatabase.Update($"UPDATE characters_mail SET item_guid = 0 WHERE mail_id = {MailID};");
						WorldServiceLocator._WorldServer.CharacterDatabase.Update($"DELETE FROM mail_items WHERE mail_id = {MailID};");
						Packets.PacketClass response2 = new Packets.PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT);
						response2.AddInt32(MailID);
						response2.AddInt32(2);
						response2.AddInt32(0);
						client.Send(ref response2);
						response2.Dispose();
					}
					else
					{
						tmpItem.Dispose();
						Packets.PacketClass response = new Packets.PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT);
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
					Exception e = ex;
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
					ulong GameObjectGUID = packet.GetUInt64();
					int MailID = packet.GetInt32();
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MAIL_TAKE_MONEY [MailID={2}]", client.IP, client.Port, MailID);
					DataTable MySQLQuery = new DataTable();
					WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT mail_money FROM characters_mail WHERE mail_id = {0}; UPDATE characters_mail SET mail_money = 0 WHERE mail_id = {0};", MailID), ref MySQLQuery);
					if (unchecked((long)client.Character.Copper) + Conversions.ToLong(MySQLQuery.Rows[0]["mail_money"]) > uint.MaxValue)
					{
						client.Character.Copper = uint.MaxValue;
					}
					else
					{
						ref uint copper = ref client.Character.Copper;
						copper = Conversions.ToUInteger(Operators.AddObject(copper, MySQLQuery.Rows[0]["mail_money"]));
					}
					client.Character.SetUpdateFlag(1176, client.Character.Copper);
					Packets.PacketClass response = new Packets.PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT);
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
				int MailID = packet.GetInt32();
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_ITEM_TEXT_QUERY [MailID={2}]", client.IP, client.Port, MailID);
				DataTable MySQLQuery = new DataTable();
				WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT mail_body FROM characters_mail WHERE mail_id = {MailID};", ref MySQLQuery);
				if (MySQLQuery.Rows.Count != 0)
				{
					Packets.PacketClass response = new Packets.PacketClass(OPCODES.SMSG_ITEM_TEXT_QUERY_RESPONSE);
					response.AddInt32(MailID);
					response.AddString(Conversions.ToString(MySQLQuery.Rows[0]["mail_body"]));
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
				ulong GameObjectGUID = packet.GetUInt64();
				int MailID = packet.GetInt32();
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MAIL_CREATE_TEXT_ITEM [MailID={2}]", client.IP, client.Port, MailID);
				ItemObject tmpItem = new ItemObject(889, client.Character.GUID)
				{
					ItemText = MailID
				};
				if (!client.Character.ItemADD(ref tmpItem))
				{
					Packets.PacketClass response = new Packets.PacketClass(OPCODES.SMSG_ITEM_TEXT_QUERY_RESPONSE);
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
				ulong GameObjectGUID = packet.GetUInt64();
				string Receiver = packet.GetString();
				if (packet.Data.Length - 1 < 14 + Receiver.Length + 1)
				{
					return;
				}
				string Subject = packet.GetString();
				if (packet.Data.Length - 1 < 14 + Receiver.Length + 2 + Subject.Length)
				{
					return;
				}
				string Body = packet.GetString();
				if (packet.Data.Length - 1 < 14 + Receiver.Length + 2 + Subject.Length + Body.Length + 4 + 4 + 1)
				{
					return;
				}
				packet.GetInt32();
				packet.GetInt32();
				ulong itemGuid = packet.GetUInt64();
				uint Money = packet.GetUInt32();
				uint COD = packet.GetUInt32();
				try
				{
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SEND_MAIL [Receiver={2} Subject={3}]", client.IP, client.Port, Receiver, Subject);
					DataTable MySQLQuery = new DataTable();
					WorldServiceLocator._WorldServer.CharacterDatabase.Query("SELECT char_guid, char_race FROM characters WHERE char_name Like '" + Receiver + "';", ref MySQLQuery);
					if (MySQLQuery.Rows.Count == 0)
					{
						Packets.PacketClass response6 = new Packets.PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT);
						response6.AddInt32(0);
						response6.AddInt32(0);
						response6.AddInt32(4);
						client.Send(ref response6);
						response6.Dispose();
						return;
					}
					ulong ReceiverGUID = Conversions.ToULong(MySQLQuery.Rows[0]["char_guid"]);
					bool ReceiverSide = WorldServiceLocator._Functions.GetCharacterSide(Conversions.ToByte(MySQLQuery.Rows[0]["char_race"]));
					if (client.Character.GUID == ReceiverGUID)
					{
						Packets.PacketClass response5 = new Packets.PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT);
						response5.AddInt32(0);
						response5.AddInt32(0);
						response5.AddInt32(2);
						client.Send(ref response5);
						response5.Dispose();
						return;
					}
					if (client.Character.Copper < unchecked((long)Money) + 30L)
					{
						Packets.PacketClass response4 = new Packets.PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT);
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
						Packets.PacketClass response3 = new Packets.PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT);
						response3.AddInt32(0);
						response3.AddInt32(0);
						response3.AddInt32(6);
						client.Send(ref response3);
						response3.Dispose();
						return;
					}
					if (client.Access >= AccessLevel.GameMaster && client.Character.IsHorde != ReceiverSide)
					{
						Packets.PacketClass response2 = new Packets.PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT);
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
					ref uint copper = ref client.Character.Copper;
					copper = (uint)(unchecked((long)copper) - (30L + unchecked((long)Money)));
					client.Character.SetUpdateFlag(1176, client.Character.Copper);
					int MailTime = (int)(unchecked((long)WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now)) + 2592000L);
					WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("INSERT INTO characters_mail (mail_sender, mail_receiver, mail_type, mail_stationary, mail_subject, mail_body, mail_money, mail_COD, mail_time, mail_read, item_guid) VALUES\r\n                ({0},{1},{2},{3},'{4}','{5}',{6},{7},{8},{9},{10});", client.Character.GUID, ReceiverGUID, 0, 41, Subject.Replace("'", "`"), Body.Replace("'", "`"), Money, COD, MailTime, (byte)0, itemGuid == WorldServiceLocator._Global_Constants.GUID_ITEM));
					if (decimal.Compare(new decimal(itemGuid), 0m) > 0)
					{
						client.Character.ItemREMOVE(itemGuid, Destroy: false, Update: true);
					}
					Packets.PacketClass sendOK = new Packets.PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT);
					sendOK.AddInt32(0);
					sendOK.AddInt32(0);
					sendOK.AddInt32(0);
					client.Send(ref sendOK);
					sendOK.Dispose();
					WorldServiceLocator._WorldServer.CHARACTERs_Lock.AcquireReaderLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
					if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(ReceiverGUID))
					{
						Packets.PacketClass response = new Packets.PacketClass(OPCODES.SMSG_RECEIVED_MAIL);
						response.AddInt32(0);
						WorldServiceLocator._WorldServer.CHARACTERs[ReceiverGUID].client.Send(ref response);
						response.Dispose();
					}
					WorldServiceLocator._WorldServer.CHARACTERs_Lock.ReleaseReaderLock();
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception e = ex;
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error sending mail: {0}{1}", Environment.NewLine, e.ToString());
					ProjectData.ClearProjectError();
				}
			}
		}
	}
}
