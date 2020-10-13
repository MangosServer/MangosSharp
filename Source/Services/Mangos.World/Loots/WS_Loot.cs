//
//  Copyright (C) 2013-2020 getMaNGOS <https:\\getmangos.eu>
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading;
using Mangos.Common.Enums.GameObject;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Group;
using Mangos.Common.Enums.Spell;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Objects;
using Mangos.World.Player;
using Mangos.World.Server;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Loots
{
	public class WS_Loot
	{
		public class TLock
		{
			public byte[] KeyType;

			public int[] Keys;

			public short RequiredMiningSkill;

			public short RequiredLockingSkill;

			public TLock(byte[] KeyType_, int[] Keys_, short ReqMining, short ReqLock)
			{
				KeyType = new byte[5];
				Keys = new int[5];
				byte i = 0;
				do
				{
					KeyType[i] = KeyType_[i];
					Keys[i] = Keys_[i];
					checked
					{
						i = (byte)unchecked((uint)(i + 1));
					}
				}
				while ((uint)i <= 4u);
				RequiredMiningSkill = ReqMining;
				RequiredLockingSkill = ReqLock;
			}
		}

		public class LootItem : IDisposable
		{
			public int ItemID;

			public byte ItemCount;

			private bool _disposedValue;

			public int ItemModel
			{
				get
				{
					if (!WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(ItemID))
					{
						WS_Items.ItemInfo tmpItem = new WS_Items.ItemInfo(ItemID);
						try
						{
							WorldServiceLocator._WorldServer.ITEMDatabase.Remove(ItemID);
						}
						catch (Exception ex2)
						{
							ProjectData.SetProjectError(ex2);
							Exception ex = ex2;
							ProjectData.ClearProjectError();
						}
						WorldServiceLocator._WorldServer.ITEMDatabase.Add(ItemID, tmpItem);
					}
					return WorldServiceLocator._WorldServer.ITEMDatabase[ItemID].Model;
				}
			}

			public LootItem(ref LootStoreItem Item)
			{
				ItemID = 0;
				ItemCount = 0;
				ItemID = Item.ItemID;
				checked
				{
					ItemCount = (byte)WorldServiceLocator._WorldServer.Rnd.Next(Item.MinCountOrRef, unchecked((int)Item.MaxCount) + 1);
				}
			}

			protected virtual void Dispose(bool disposing)
			{
				if (!_disposedValue)
				{
				}
				_disposedValue = true;
			}

			public void Dispose()
			{
				Dispose(disposing: true);
				GC.SuppressFinalize(this);
			}

			void IDisposable.Dispose()
			{
				//ILSpy generated this explicit interface implementation from .override directive in Dispose
				this.Dispose();
			}
		}

		public class LootStoreItem
		{
			public int ItemID;

			public float Chance;

			public byte Group;

			public int MinCountOrRef;

			public byte MaxCount;

			public ConditionType LootCondition;

			public int ConditionValue1;

			public int ConditionValue2;

			public bool NeedQuest;

			public LootStoreItem(int Item, float Chance, byte Group, int MinCountOrRef, byte MaxCount, ConditionType LootCondition, int ConditionValue1, int ConditionValue2, bool NeedQuest)
			{
				ItemID = 0;
				this.Chance = 0f;
				this.Group = 0;
				this.MinCountOrRef = 0;
				this.MaxCount = 0;
				this.LootCondition = ConditionType.CONDITION_NONE;
				this.ConditionValue1 = 0;
				this.ConditionValue2 = 0;
				this.NeedQuest = false;
				ItemID = Item;
				this.Chance = Chance;
				this.Group = Group;
				this.MinCountOrRef = MinCountOrRef;
				this.MaxCount = MaxCount;
				this.LootCondition = LootCondition;
				this.ConditionValue1 = ConditionValue1;
				this.ConditionValue2 = ConditionValue2;
				this.NeedQuest = NeedQuest;
			}

			public bool Roll()
			{
				if (Chance >= 100f)
				{
					return true;
				}
				return WorldServiceLocator._Functions.RollChance(Chance);
			}
		}

		public class LootGroup
		{
			public List<LootStoreItem> ExplicitlyChanced;

			public List<LootStoreItem> EqualChanced;

			public LootGroup()
			{
				ExplicitlyChanced = new List<LootStoreItem>();
				EqualChanced = new List<LootStoreItem>();
			}

			public void AddItem(ref LootStoreItem Item)
			{
				if (Item.Chance != 0f)
				{
					ExplicitlyChanced.Add(Item);
				}
				else
				{
					EqualChanced.Add(Item);
				}
			}

			public LootStoreItem Roll()
			{
				checked
				{
					if (ExplicitlyChanced.Count > 0)
					{
						float rollChance = (float)(WorldServiceLocator._WorldServer.Rnd.NextDouble() * 100.0);
						int num = ExplicitlyChanced.Count - 1;
						for (int i = 0; i <= num; i++)
						{
							if (ExplicitlyChanced[i].Chance >= 100f)
							{
								return ExplicitlyChanced[i];
							}
							rollChance -= ExplicitlyChanced[i].Chance;
							if (rollChance <= 0f)
							{
								return ExplicitlyChanced[i];
							}
						}
					}
					if (EqualChanced.Count > 0)
					{
						return EqualChanced[WorldServiceLocator._WorldServer.Rnd.Next(0, EqualChanced.Count)];
					}
					return null;
				}
			}

			public void Process(ref LootObject Loot)
			{
				LootStoreItem Item = Roll();
				if (Item != null)
				{
					Loot.Items.Add(new LootItem(ref Item));
				}
			}
		}

		public class LootObject : IDisposable
		{
			public ulong GUID;

			public List<LootItem> Items;

			public int Money;

			public LootType LootType;

			public ulong LootOwner;

			public Dictionary<int, GroupLootInfo> GroupLootInfo;

			private bool _disposedValue;

			public bool IsEmpty
			{
				get
				{
					if (Money != 0)
					{
						return false;
					}
					checked
					{
						int num = Items.Count - 1;
						for (int i = 0; i <= num; i++)
						{
							if (Items[i] != null)
							{
								return false;
							}
						}
						return true;
					}
				}
			}

			public LootObject(ulong GUID_, LootType LootType_)
			{
				GUID = 0uL;
				Items = new List<LootItem>();
				Money = 0;
				LootType = LootType.LOOTTYPE_CORPSE;
				LootOwner = 0uL;
				GroupLootInfo = new Dictionary<int, GroupLootInfo>(0);
				WorldServiceLocator._WS_Loot.LootTable[GUID_] = this;
				LootType = LootType_;
				GUID = GUID_;
			}

			public void SendLoot(ref WS_Network.ClientClass client)
			{
				if (Items.Count == 0)
				{
					WorldServiceLocator._WS_Loot.SendEmptyLoot(GUID, LootType, ref client);
					return;
				}
				if (decimal.Compare(new decimal(LootOwner), 0m) != 0 && client.Character.GUID != LootOwner)
				{
					Packets.PacketClass notMy = new Packets.PacketClass(OPCODES.SMSG_INVENTORY_CHANGE_FAILURE);
					notMy.AddInt8(58);
					notMy.AddUInt64(0uL);
					notMy.AddUInt64(0uL);
					notMy.AddInt8(0);
					client.Send(ref notMy);
					notMy.Dispose();
					return;
				}
				Packets.PacketClass response = new Packets.PacketClass(OPCODES.SMSG_LOOT_RESPONSE);
				response.AddUInt64(GUID);
				response.AddInt8((byte)LootType);
				response.AddInt32(Money);
				byte b2;
				byte i;
				checked
				{
					response.AddInt8((byte)Items.Count);
					byte b = (byte)(Items.Count - 1);
					byte j = 0;
					while (unchecked((uint)j <= (uint)b))
					{
						if (Items[j] == null)
						{
							response.AddInt8(j);
							response.AddInt32(0);
							response.AddInt32(0);
							response.AddInt32(0);
							response.AddUInt64(0uL);
							response.AddInt8(0);
						}
						else
						{
							response.AddInt8(j);
							response.AddInt32(Items[j].ItemID);
							response.AddInt32(Items[j].ItemCount);
							response.AddInt32(Items[j].ItemModel);
							response.AddUInt64(0uL);
							if (client.Character.IsInGroup && client.Character.Group.LootMethod == GroupLootMethod.LOOT_MASTER && client.Character.Group.LocalLootMaster != null && client.Character.Group.LocalLootMaster != client.Character)
							{
								response.AddInt8(2);
							}
							else
							{
								response.AddInt8(0);
							}
						}
						j = (byte)unchecked((uint)(j + 1));
					}
					client.Send(ref response);
					response.Dispose();
					client.Character.lootGUID = GUID;
					if (!client.Character.IsInGroup || !((client.Character.Group.LootMethod == GroupLootMethod.LOOT_NEED_BEFORE_GREED) | (client.Character.Group.LootMethod == GroupLootMethod.LOOT_GROUP)))
					{
						return;
					}
					b2 = (byte)(Items.Count - 1);
					i = 0;
				}
				while ((uint)i <= (uint)b2)
				{
					if (Items[i] != null && WorldServiceLocator._WorldServer.ITEMDatabase[Items[i].ItemID].Quality >= (int)client.Character.Group.LootThreshold)
					{
						GroupLootInfo[i] = new GroupLootInfo();
						GroupLootInfo[i].LootObject = this;
						GroupLootInfo[i].LootSlot = i;
						GroupLootInfo[i].Item = Items[i];
						WorldServiceLocator._WS_Loot.StartRoll(GUID, i, ref client.Character);
						break;
					}
					checked
					{
						i = (byte)unchecked((uint)(i + 1));
					}
				}
			}

			public void GetLoot(ref WS_Network.ClientClass client, byte Slot)
			{
				try
				{
					if (Items[Slot] == null)
					{
						Packets.PacketClass response4 = new Packets.PacketClass(OPCODES.SMSG_INVENTORY_CHANGE_FAILURE);
						response4.AddInt8(49);
						response4.AddUInt64(0uL);
						response4.AddUInt64(0uL);
						response4.AddInt8(0);
						client.Send(ref response4);
						response4.Dispose();
						return;
					}
					if (GroupLootInfo.ContainsKey(Slot))
					{
						Packets.PacketClass response3 = new Packets.PacketClass(OPCODES.SMSG_INVENTORY_CHANGE_FAILURE);
						response3.AddInt8(58);
						response3.AddUInt64(0uL);
						response3.AddUInt64(0uL);
						response3.AddInt8(0);
						client.Send(ref response3);
						response3.Dispose();
						return;
					}
					ItemObject itemObject = new ItemObject(Items[Slot].ItemID, client.Character.GUID);
					itemObject.StackCount = Items[Slot].ItemCount;
					ItemObject tmpItem = itemObject;
					if (client.Character.ItemADD(ref tmpItem))
					{
						if (tmpItem.ItemInfo.Bonding == 1)
						{
							ItemObject itemObject2 = tmpItem;
							WS_Network.ClientClass client2 = null;
							itemObject2.SoulbindItem(client2);
						}
						Packets.PacketClass response2 = new Packets.PacketClass(OPCODES.SMSG_LOOT_REMOVED);
						response2.AddInt8(Slot);
						client.Send(ref response2);
						response2.Dispose();
						client.Character.LogLootItem(tmpItem, Items[Slot].ItemCount, Recieved: false, Created: false);
						Items[Slot].Dispose();
						Items[Slot] = null;
						if (LootType == LootType.LOOTTYPE_FISHING && IsEmpty)
						{
							SendRelease(ref client);
							Dispose();
						}
					}
					else
					{
						tmpItem.Delete();
						Packets.PacketClass response = new Packets.PacketClass(OPCODES.SMSG_INVENTORY_CHANGE_FAILURE);
						response.AddInt8(50);
						response.AddUInt64(0uL);
						response.AddUInt64(0uL);
						response.AddInt8(0);
						client.Send(ref response);
						response.Dispose();
					}
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception e = ex;
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Error getting loot.{0}", Environment.NewLine + e.ToString());
					ProjectData.ClearProjectError();
				}
			}

			public void SendRelease(ref WS_Network.ClientClass client)
			{
				Packets.PacketClass responseRelease = new Packets.PacketClass(OPCODES.SMSG_LOOT_RELEASE_RESPONSE);
				responseRelease.AddUInt64(GUID);
				responseRelease.AddInt8(1);
				client.Send(ref responseRelease);
				responseRelease.Dispose();
			}

			protected virtual void Dispose(bool disposing)
			{
				if (!_disposedValue)
				{
					WorldServiceLocator._WS_Loot.LootTable.Remove(GUID);
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Loot destroyed.");
				}
				_disposedValue = true;
			}

			public void Dispose()
			{
				Dispose(disposing: true);
				GC.SuppressFinalize(this);
			}

			void IDisposable.Dispose()
			{
				//ILSpy generated this explicit interface implementation from .override directive in Dispose
				this.Dispose();
			}
		}

		public class LootTemplate
		{
			public List<LootStoreItem> Items;

			public Dictionary<byte, LootGroup> Groups;

			public LootTemplate()
			{
				Items = new List<LootStoreItem>();
				Groups = new Dictionary<byte, LootGroup>();
			}

			public void AddItem(ref LootStoreItem Item)
			{
				if (Item.Group > 0 && Item.MinCountOrRef > 0)
				{
					if (!Groups.ContainsKey(Item.Group))
					{
						Groups.Add(Item.Group, new LootGroup());
					}
					Groups[Item.Group].AddItem(ref Item);
				}
				else
				{
					Items.Add(Item);
				}
			}

			public void Process(ref LootObject Loot, byte GroupID)
			{
				if (GroupID > 0)
				{
					if (Groups.ContainsKey(GroupID))
					{
						Groups[GroupID].Process(ref Loot);
					}
					return;
				}
				checked
				{
					int num = Items.Count - 1;
					for (int i = 0; i <= num; i++)
					{
						if (!Items[i].Roll())
						{
							continue;
						}
						if (Items[i].MinCountOrRef < 0)
						{
							LootTemplate Referenced = WorldServiceLocator._WS_Loot.LootTemplates_Reference.GetLoot(-Items[i].MinCountOrRef);
							if (Referenced != null)
							{
								int maxCount = Items[i].MaxCount;
								for (int j = 1; j <= maxCount; j++)
								{
									Referenced.Process(ref Loot, Items[i].Group);
								}
							}
						}
						else
						{
							List<LootItem> items = Loot.Items;
							List<LootStoreItem> items2;
							int index;
							LootStoreItem Item = (items2 = Items)[index = i];
							LootItem item = new LootItem(ref Item);
							items2[index] = Item;
							items.Add(item);
						}
					}
					foreach (KeyValuePair<byte, LootGroup> group in Groups)
					{
						group.Value.Process(ref Loot);
					}
				}
			}
		}

		public class LootStore
		{
			private readonly string Name;

			private readonly Dictionary<int, LootTemplate> Templates;

			public LootStore(string Name)
			{
				Templates = new Dictionary<int, LootTemplate>();
				this.Name = Name;
			}

			public LootTemplate GetLoot(int Entry)
			{
				if (Templates.ContainsKey(Entry))
				{
					return Templates[Entry];
				}
				return CreateTemplate(Entry);
			}

			private LootTemplate CreateTemplate(int Entry)
			{
				LootTemplate newTemplate = new LootTemplate();
				Templates.Add(Entry, newTemplate);
				DataTable MysqlQuery = new DataTable();
				WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT {0}.*,conditions.type,conditions.value1, conditions.value2 FROM {0} LEFT JOIN conditions ON {0}.`condition_id`=conditions.`condition_entry` WHERE entry = {1};", Name, Entry), ref MysqlQuery);
				if (MysqlQuery.Rows.Count == 0)
				{
					Templates[Entry] = null;
					return null;
				}
				IEnumerator enumerator = default(IEnumerator);
				try
				{
					enumerator = MysqlQuery.Rows.GetEnumerator();
					while (enumerator.MoveNext())
					{
						DataRow LootRow = (DataRow)enumerator.Current;
						int Item = Conversions.ToInteger(LootRow["item"]);
						float ChanceOrQuestChance = Conversions.ToSingle(LootRow["ChanceOrQuestChance"]);
						byte GroupID = Conversions.ToByte(LootRow["groupid"]);
						int MinCountOrRef = Conversions.ToInteger(LootRow["mincountOrRef"]);
						byte MaxCount = Conversions.ToByte(LootRow["maxcount"]);
						ConditionType LootCondition = ConditionType.CONDITION_NONE;
						if (!Information.IsDBNull(RuntimeHelpers.GetObjectValue(LootRow["type"])))
						{
							LootCondition = (ConditionType)Conversions.ToInteger(LootRow["type"]);
						}
						int ConditionValue1 = 0;
						if (!Information.IsDBNull(RuntimeHelpers.GetObjectValue(LootRow["value1"])))
						{
							ConditionValue1 = Conversions.ToInteger(LootRow["value1"]);
						}
						int ConditionValue2 = 0;
						if (!Information.IsDBNull(RuntimeHelpers.GetObjectValue(LootRow["value2"])))
						{
							ConditionValue2 = Conversions.ToInteger(LootRow["value2"]);
						}
						LootStoreItem newItem = new LootStoreItem(Item, Math.Abs(ChanceOrQuestChance), GroupID, MinCountOrRef, MaxCount, LootCondition, ConditionValue1, ConditionValue2, ChanceOrQuestChance < 0f);
						newTemplate.AddItem(ref newItem);
					}
				}
				finally
				{
					if (enumerator is IDisposable)
					{
						(enumerator as IDisposable).Dispose();
					}
				}
				return newTemplate;
			}
		}

		public class GroupLootInfo
		{
			public LootObject LootObject;

			public byte LootSlot;

			public LootItem Item;

			public List<WS_PlayerData.CharacterObject> Rolls;

			public Dictionary<WS_PlayerData.CharacterObject, int> Looters;

			public Timer RollTimeoutTimer;

			public GroupLootInfo()
			{
				Rolls = new List<WS_PlayerData.CharacterObject>();
				Looters = new Dictionary<WS_PlayerData.CharacterObject, int>(5);
				RollTimeoutTimer = null;
			}

			public void Check()
			{
				if (Looters.Count != Rolls.Count)
				{
					return;
				}
				byte maxRollType = 0;
				foreach (KeyValuePair<WS_PlayerData.CharacterObject, int> looter2 in Looters)
				{
					if (looter2.Value == 1)
					{
						maxRollType = 1;
					}
					if (looter2.Value == 2 && maxRollType != 1)
					{
						maxRollType = 2;
					}
				}
				if (maxRollType == 0)
				{
					LootObject.GroupLootInfo.Remove(LootSlot);
					Packets.PacketClass response2 = new Packets.PacketClass(OPCODES.SMSG_LOOT_ALL_PASSED);
					response2.AddUInt64(LootObject.GUID);
					response2.AddInt32(LootSlot);
					response2.AddInt32(Item.ItemID);
					response2.AddInt32(0);
					response2.AddInt32(0);
					Broadcast(ref response2);
					response2.Dispose();
					return;
				}
				int maxRoll = -1;
				WS_PlayerData.CharacterObject looterCharacter = null;
				checked
				{
					foreach (KeyValuePair<WS_PlayerData.CharacterObject, int> looter in Looters)
					{
						if (looter.Value == maxRollType)
						{
							byte rollValue = (byte)WorldServiceLocator._WorldServer.Rnd.Next(0, 100);
							if (rollValue > maxRoll)
							{
								maxRoll = rollValue;
								looterCharacter = looter.Key;
							}
							Packets.PacketClass response = new Packets.PacketClass(OPCODES.SMSG_LOOT_ROLL);
							response.AddUInt64(LootObject.GUID);
							response.AddInt32(LootSlot);
							response.AddUInt64(looter.Key.GUID);
							response.AddInt32(Item.ItemID);
							response.AddInt32(0);
							response.AddInt32(0);
							response.AddInt8(rollValue);
							response.AddInt8((byte)looter.Value);
							Broadcast(ref response);
							response.Dispose();
						}
					}
					ItemObject itemObject = new ItemObject(Item.ItemID, looterCharacter.GUID);
					itemObject.StackCount = Item.ItemCount;
					ItemObject tmpItem = itemObject;
					Packets.PacketClass wonItem = new Packets.PacketClass(OPCODES.SMSG_LOOT_ROLL_WON);
					wonItem.AddUInt64(LootObject.GUID);
					wonItem.AddInt32(LootSlot);
					wonItem.AddInt32(Item.ItemID);
					wonItem.AddInt32(0);
					wonItem.AddInt32(0);
					wonItem.AddUInt64(looterCharacter.GUID);
					wonItem.AddInt8((byte)maxRoll);
					wonItem.AddInt8(maxRollType);
					Broadcast(ref wonItem);
					if (looterCharacter.ItemADD(ref tmpItem))
					{
						looterCharacter.LogLootItem(tmpItem, Item.ItemCount, Recieved: false, Created: false);
						LootObject.GroupLootInfo.Remove(LootSlot);
						LootObject.Items[LootSlot] = null;
					}
					else
					{
						tmpItem.Delete();
						LootObject.GroupLootInfo.Remove(LootSlot);
					}
				}
			}

			public void Broadcast(ref Packets.PacketClass packet)
			{
				foreach (WS_PlayerData.CharacterObject objCharacter in Rolls)
				{
					objCharacter.client.SendMultiplyPackets(ref packet);
				}
			}

			public void EndRoll(object state)
			{
				foreach (WS_PlayerData.CharacterObject objCharacter in Rolls)
				{
					if (!Looters.ContainsKey(objCharacter))
					{
						Looters[objCharacter] = 0;
						Packets.PacketClass response = new Packets.PacketClass(OPCODES.SMSG_LOOT_ROLL);
						response.AddUInt64(LootObject.GUID);
						response.AddInt32(LootSlot);
						response.AddUInt64(objCharacter.GUID);
						response.AddInt32(Item.ItemID);
						response.AddInt32(0);
						response.AddInt32(0);
						response.AddInt8(249);
						response.AddInt8(0);
						Broadcast(ref response);
					}
				}
				RollTimeoutTimer.Dispose();
				RollTimeoutTimer = null;
				Check();
			}
		}

		public LootStore LootTemplates_Creature;

		public LootStore LootTemplates_Disenchant;

		public LootStore LootTemplates_Fishing;

		public LootStore LootTemplates_Gameobject;

		public LootStore LootTemplates_Item;

		public LootStore LootTemplates_Pickpocketing;

		public LootStore LootTemplates_QuestMail;

		public LootStore LootTemplates_Reference;

		public LootStore LootTemplates_Skinning;

		public Dictionary<ulong, LootObject> LootTable;

		public Dictionary<int, TLock> Locks;

		public WS_Loot()
		{
			LootTable = new Dictionary<ulong, LootObject>();
			Locks = new Dictionary<int, TLock>();
		}

		public void On_CMSG_AUTOSTORE_LOOT_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
		{
			if (checked(packet.Data.Length - 1) < 6)
			{
				return;
			}
			try
			{
				packet.GetInt16();
				byte slot = packet.GetInt8();
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AUTOSTORE_LOOT_ITEM [slot={2}]", client.IP, client.Port, slot);
				if (LootTable.ContainsKey(client.Character.lootGUID))
				{
					LootTable[client.Character.lootGUID].GetLoot(ref client, slot);
					return;
				}
				Packets.PacketClass response = new Packets.PacketClass(OPCODES.SMSG_INVENTORY_CHANGE_FAILURE);
				response.AddInt8(49);
				response.AddUInt64(0uL);
				response.AddUInt64(0uL);
				response.AddInt8(0);
				client.Send(ref response);
				response.Dispose();
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception e = ex;
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Error looting item.{0}", Environment.NewLine + e.ToString());
				ProjectData.ClearProjectError();
			}
		}

		public void On_CMSG_LOOT_MONEY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
		{
			WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_LOOT_MONEY", client.IP, client.Port);
			if (!LootTable.ContainsKey(client.Character.lootGUID))
			{
				return;
			}
			checked
			{
				if (client.Character.IsInGroup)
				{
					List<WS_Base.BaseUnit> members = WorldServiceLocator._WS_Spells.GetPartyMembersAroundMe(ref client.Character, 100f);
					int copper2 = unchecked(LootTable[client.Character.lootGUID].Money / members.Count) + 1;
					LootTable[client.Character.lootGUID].Money = 0;
					Packets.PacketClass sharePcket = new Packets.PacketClass(OPCODES.SMSG_LOOT_MONEY_NOTIFY);
					sharePcket.AddInt32(copper2);
					foreach (WS_PlayerData.CharacterObject character in members)
					{
						character.client.SendMultiplyPackets(ref sharePcket);
						ref uint copper3 = ref character.Copper;
						copper3 = (uint)(unchecked((long)copper3) + unchecked((long)copper2));
						character.SetUpdateFlag(1176, character.Copper);
						character.SaveCharacter();
					}
					client.SendMultiplyPackets(ref sharePcket);
					ref uint copper4 = ref client.Character.Copper;
					copper4 = (uint)(unchecked((long)copper4) + unchecked((long)copper2));
					sharePcket.Dispose();
				}
				else
				{
					int copper = LootTable[client.Character.lootGUID].Money;
					ref uint copper5 = ref client.Character.Copper;
					copper5 = (uint)(unchecked((long)copper5) + unchecked((long)copper));
					LootTable[client.Character.lootGUID].Money = 0;
					Packets.PacketClass lootPacket = new Packets.PacketClass(OPCODES.SMSG_LOOT_MONEY_NOTIFY);
					lootPacket.AddInt32(copper);
					client.Send(ref lootPacket);
					lootPacket.Dispose();
				}
				client.Character.SetUpdateFlag(1176, client.Character.Copper);
				client.Character.SendCharacterUpdate(toNear: false);
				client.Character.SaveCharacter();
				Packets.PacketClass response2 = new Packets.PacketClass(OPCODES.SMSG_LOOT_CLEAR_MONEY);
				client.SendMultiplyPackets(ref response2);
				response2.Dispose();
			}
		}

		public void On_CMSG_LOOT(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
		{
			if (checked(packet.Data.Length - 1) >= 13)
			{
				packet.GetInt16();
				ulong GUID = packet.GetUInt64();
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_LOOT [GUID={2:X}]", client.IP, client.Port, GUID);
				client.Character.cUnitFlags = client.Character.cUnitFlags | 0x400;
				client.Character.SetUpdateFlag(46, client.Character.cUnitFlags);
				client.Character.SendCharacterUpdate();
				if (LootTable.ContainsKey(GUID))
				{
					LootTable[GUID].SendLoot(ref client);
				}
				else
				{
					SendEmptyLoot(GUID, LootType.LOOTTYPE_CORPSE, ref client);
				}
			}
		}

		public void On_CMSG_LOOT_RELEASE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
		{
			if (checked(packet.Data.Length - 1) < 13)
			{
				return;
			}
			packet.GetInt16();
			ulong GUID = packet.GetUInt64();
			WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_LOOT_RELEASE [lootGUID={2:X}]", client.IP, client.Port, GUID);
			if (client.Character.spellCasted[1] != null)
			{
				client.Character.spellCasted[1].State = SpellCastState.SPELL_STATE_IDLE;
			}
			client.Character.cUnitFlags = client.Character.cUnitFlags & -1025;
			client.Character.SetUpdateFlag(46, client.Character.cUnitFlags);
			client.Character.SendCharacterUpdate();
			if (LootTable.ContainsKey(GUID))
			{
				LootTable[GUID].SendRelease(ref client);
				LootTable[GUID].LootOwner = 0uL;
				LootType LootType = LootTable[GUID].LootType;
				if (LootTable[GUID].IsEmpty)
				{
					LootTable[GUID].Dispose();
					if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(GUID))
					{
						switch (LootType)
						{
						case LootType.LOOTTYPE_CORPSE:
						{
							if (WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].CreatureInfo.SkinLootID > 0)
							{
								WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cUnitFlags = WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cUnitFlags | 0x4000000;
							}
							WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cDynamicFlags = 0;
							Packets.PacketClass response3 = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
							response3.AddInt32(1);
							response3.AddInt8(0);
							Packets.UpdateClass UpdateData4 = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
							UpdateData4.SetUpdateFlag(143, WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cDynamicFlags);
							UpdateData4.SetUpdateFlag(46, WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cUnitFlags);
							Dictionary<ulong, WS_Creatures.CreatureObject> wORLD_CREATUREs;
							ulong key;
							WS_Creatures.CreatureObject updateObject = (wORLD_CREATUREs = WorldServiceLocator._WorldServer.WORLD_CREATUREs)[key = GUID];
							UpdateData4.AddToPacket(ref response3, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
							wORLD_CREATUREs[key] = updateObject;
							WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].SendToNearPlayers(ref response3, 0uL);
							response3.Dispose();
							UpdateData4.Dispose();
							break;
						}
						case LootType.LOOTTYPE_SKINNING:
							WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].Despawn();
							break;
						}
					}
					else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsGameObject(GUID) && WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(GUID))
					{
						if (WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].IsConsumeable)
						{
							WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].State = GameObjectLootState.LOOT_LOOTED;
							WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].Despawn();
						}
						else
						{
							WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].State = GameObjectLootState.DOOR_CLOSED;
						}
					}
					else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsItem(GUID))
					{
						client.Character.ItemREMOVE(GUID, Destroy: true, Update: true);
					}
				}
				else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(GUID))
				{
					switch (LootType)
					{
					case LootType.LOOTTYPE_CORPSE:
					{
						if (!WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(GUID))
						{
							LootTable[GUID].Dispose();
							break;
						}
						WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cDynamicFlags = 1;
						Packets.PacketClass response4 = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
						response4.AddInt32(1);
						response4.AddInt8(0);
						Packets.UpdateClass UpdateData3 = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
						UpdateData3.SetUpdateFlag(143, WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cDynamicFlags);
						Dictionary<ulong, WS_Creatures.CreatureObject> wORLD_CREATUREs;
						ulong key;
						WS_Creatures.CreatureObject updateObject = (wORLD_CREATUREs = WorldServiceLocator._WorldServer.WORLD_CREATUREs)[key = GUID];
						UpdateData3.AddToPacket(ref response4, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
						wORLD_CREATUREs[key] = updateObject;
						WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].SendToNearPlayers(ref response4, 0uL);
						response4.Dispose();
						UpdateData3.Dispose();
						break;
					}
					case LootType.LOOTTYPE_SKINNING:
						WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].Despawn();
						break;
					}
				}
				else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsGameObject(GUID))
				{
					if (!WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(GUID) || LootTable[GUID].LootType == LootType.LOOTTYPE_FISHING)
					{
						LootTable[GUID].Dispose();
					}
					else
					{
						WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].State = GameObjectLootState.DOOR_CLOSED;
						Packets.PacketClass response2 = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
						response2.AddInt32(1);
						response2.AddInt8(0);
						Packets.UpdateClass UpdateData2 = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
						UpdateData2.SetUpdateFlag(14, 0, (byte)WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].State);
						Dictionary<ulong, WS_GameObjects.GameObjectObject> wORLD_GAMEOBJECTs;
						ulong key;
						WS_GameObjects.GameObjectObject updateObject2 = (wORLD_GAMEOBJECTs = WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs)[key = GUID];
						UpdateData2.AddToPacket(ref response2, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject2);
						wORLD_GAMEOBJECTs[key] = updateObject2;
						WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].SendToNearPlayers(ref response2, 0uL);
						response2.Dispose();
						UpdateData2.Dispose();
					}
				}
				else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsItem(GUID))
				{
					LootTable[GUID].Dispose();
					client.Character.ItemREMOVE(GUID, Destroy: true, Update: true);
				}
				else
				{
					LootTable[GUID].Dispose();
				}
			}
			else
			{
				Packets.PacketClass responseRelease = new Packets.PacketClass(OPCODES.SMSG_LOOT_RELEASE_RESPONSE);
				responseRelease.AddUInt64(GUID);
				responseRelease.AddInt8(1);
				client.Send(ref responseRelease);
				responseRelease.Dispose();
				if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(GUID))
				{
					if (WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].CreatureInfo.SkinLootID > 0)
					{
						WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cUnitFlags = WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cUnitFlags | 0x4000000;
					}
					WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cDynamicFlags = 0;
					Packets.PacketClass response = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
					response.AddInt32(1);
					response.AddInt8(0);
					Packets.UpdateClass UpdateData = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
					UpdateData.SetUpdateFlag(143, WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cDynamicFlags);
					UpdateData.SetUpdateFlag(46, WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].cUnitFlags);
					Dictionary<ulong, WS_Creatures.CreatureObject> wORLD_CREATUREs;
					ulong key;
					WS_Creatures.CreatureObject updateObject = (wORLD_CREATUREs = WorldServiceLocator._WorldServer.WORLD_CREATUREs)[key = GUID];
					UpdateData.AddToPacket(ref response, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
					wORLD_CREATUREs[key] = updateObject;
					WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].SendToNearPlayers(ref response, 0uL);
					response.Dispose();
					UpdateData.Dispose();
				}
			}
			client.Character.lootGUID = 0uL;
		}

		public void SendEmptyLoot(ulong GUID, LootType LootType, ref WS_Network.ClientClass client)
		{
			Packets.PacketClass response = new Packets.PacketClass(OPCODES.SMSG_LOOT_RESPONSE);
			response.AddUInt64(GUID);
			response.AddInt8((byte)LootType);
			response.AddInt32(0);
			response.AddInt8(0);
			client.Send(ref response);
			response.Dispose();
			WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "[{0}:{1}] Empty loot for GUID [{2:X}].", client.IP, client.Port, GUID);
		}

		public void StartRoll(ulong LootGUID, byte Slot, ref WS_PlayerData.CharacterObject Character)
		{
			List<WS_PlayerData.CharacterObject> rollCharacters = new List<WS_PlayerData.CharacterObject>
			{
				Character
			};
			foreach (ulong GUID in Character.Group.LocalMembers)
			{
				if (Character.playersNear.Contains(GUID))
				{
					rollCharacters.Add(WorldServiceLocator._WorldServer.CHARACTERs[GUID]);
				}
			}
			Packets.PacketClass startRoll = new Packets.PacketClass(OPCODES.SMSG_LOOT_START_ROLL);
			startRoll.AddUInt64(LootGUID);
			startRoll.AddInt32(Slot);
			startRoll.AddInt32(LootTable[LootGUID].GroupLootInfo[Slot].Item.ItemID);
			startRoll.AddInt32(0);
			startRoll.AddInt32(0);
			startRoll.AddInt32(60000);
			foreach (WS_PlayerData.CharacterObject objCharacter in rollCharacters)
			{
				objCharacter.client.SendMultiplyPackets(ref startRoll);
			}
			startRoll.Dispose();
			LootTable[LootGUID].GroupLootInfo[Slot].Rolls = rollCharacters;
			LootTable[LootGUID].GroupLootInfo[Slot].RollTimeoutTimer = new Timer(new TimerCallback(LootTable[LootGUID].GroupLootInfo[Slot].EndRoll), 0, 60000, -1);
		}

		public void On_CMSG_LOOT_ROLL(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
		{
			checked
			{
				if (packet.Data.Length - 1 >= 18)
				{
					packet.GetInt16();
					ulong GUID = packet.GetUInt64();
					byte Slot = (byte)packet.GetInt32();
					byte rollType = packet.GetInt8();
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_LOOT_ROLL [loot={2} roll={3}]", client.IP, client.Port, GUID, rollType);
					Packets.PacketClass response = new Packets.PacketClass(OPCODES.SMSG_LOOT_ROLL);
					response.AddUInt64(GUID);
					response.AddInt32(Slot);
					response.AddUInt64(client.Character.GUID);
					response.AddInt32(LootTable[GUID].GroupLootInfo[Slot].Item.ItemID);
					response.AddInt32(0);
					response.AddInt32(0);
					switch (rollType)
					{
					case 0:
						response.AddInt8(249);
						response.AddInt8(0);
						break;
					case 1:
						response.AddInt8(0);
						response.AddInt8(0);
						break;
					case 2:
						response.AddInt8(249);
						response.AddInt8(2);
						break;
					}
					LootTable[GUID].GroupLootInfo[Slot].Broadcast(ref response);
					response.Dispose();
					LootTable[GUID].GroupLootInfo[Slot].Looters[client.Character] = rollType;
					LootTable[GUID].GroupLootInfo[Slot].Check();
				}
			}
		}
	}
}
