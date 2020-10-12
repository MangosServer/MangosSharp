using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Item;
using Mangos.Common.Enums.Spell;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Loots;
using Mangos.World.Player;
using Mangos.World.Server;
using Mangos.World.Spells;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Objects
{
	public sealed class ItemObject : IDisposable
	{
		public readonly int ItemEntry;

		public ulong GUID;

		public ulong OwnerGUID;

		public readonly ulong GiftCreatorGUID;

		public readonly ulong CreatorGUID;

		public int StackCount;

		public int Durability;

		public int ChargesLeft;

		private int _flags;

		public Dictionary<byte, ItemObject> Items;

		public readonly int RandomProperties;

		public int SuffixFactor;

		public readonly Dictionary<byte, WS_Items.TEnchantmentInfo> Enchantments;

		private WS_Loot.LootObject _loot;

		public int ItemText;

		private bool _disposedValue;

		public WS_Items.ItemInfo ItemInfo => WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry];

		public bool IsFree
		{
			get
			{
				if (Items.Count > 0)
				{
					return false;
				}
				return true;
			}
		}

		public bool IsRanged => ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_WEAPON && (ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_LIQUID || ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_CROSSBOW || ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_POTION);

		public byte GetBagSlot
		{
			get
			{
				if (!WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(OwnerGUID))
				{
					return byte.MaxValue;
				}
				WS_PlayerData.CharacterObject characterObject = WorldServiceLocator._WorldServer.CHARACTERs[OwnerGUID];
				byte i = 19;
				do
				{
					checked
					{
						if (characterObject.Items.ContainsKey(i))
						{
							byte b = (byte)(characterObject.Items[i].ItemInfo.ContainerSlots - 1);
							byte j = 0;
							while (unchecked((uint)j <= (uint)b))
							{
								if (characterObject.Items[i].Items.ContainsKey(j) && characterObject.Items[i].Items[j] == this)
								{
									return i;
								}
								j = (byte)unchecked((uint)(j + 1));
							}
						}
						i = (byte)unchecked((uint)(i + 1));
					}
				}
				while ((uint)i <= 22u);
				characterObject = null;
				return byte.MaxValue;
			}
		}

		public int GetSlot
		{
			get
			{
				if (!WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(OwnerGUID))
				{
					return -1;
				}
				WS_PlayerData.CharacterObject characterObject = WorldServiceLocator._WorldServer.CHARACTERs[OwnerGUID];
				byte i = 0;
				do
				{
					if (characterObject.Items.ContainsKey(i) && characterObject.Items[i] == this)
					{
						return i;
					}
					checked
					{
						i = (byte)unchecked((uint)(i + 1));
					}
				}
				while ((uint)i <= 38u);
				byte k = 81;
				do
				{
					if (characterObject.Items.ContainsKey(k) && characterObject.Items[k] == this)
					{
						return k;
					}
					checked
					{
						k = (byte)unchecked((uint)(k + 1));
					}
				}
				while ((uint)k <= 112u);
				byte j = 19;
				do
				{
					checked
					{
						if (characterObject.Items.ContainsKey(j))
						{
							byte b = (byte)(characterObject.Items[j].ItemInfo.ContainerSlots - 1);
							byte l = 0;
							while (unchecked((uint)l <= (uint)b))
							{
								if (characterObject.Items[j].Items.ContainsKey(l) && characterObject.Items[j].Items[l] == this)
								{
									return l;
								}
								l = (byte)unchecked((uint)(l + 1));
							}
						}
						j = (byte)unchecked((uint)(j + 1));
					}
				}
				while ((uint)j <= 22u);
				characterObject = null;
				return -1;
			}
		}

		public int GetSkill => ItemInfo.GetReqSkill;

		public uint GetDurabulityCost
		{
			get
			{
				checked
				{
					try
					{
						int lostDurability = WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].Durability - Durability;
						if (lostDurability > 300)
						{
							lostDurability = 300;
						}
						int subClass = 0;
						subClass = ((ItemInfo.ObjectClass != ITEM_CLASS.ITEM_CLASS_WEAPON) ? (unchecked((int)ItemInfo.SubClass) + 21) : unchecked((int)ItemInfo.SubClass));
						uint durabilityCost = (uint)Math.Round((double)lostDurability * ((double)WorldServiceLocator._WS_DBCDatabase.DurabilityCosts[ItemInfo.Level, subClass] / 40.0 * 100.0));
						WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Durability cost: {0}", durabilityCost);
						return durabilityCost;
					}
					catch (Exception projectError)
					{
						ProjectData.SetProjectError(projectError);
						uint GetDurabulityCost = 0u;
						ProjectData.ClearProjectError();
						return GetDurabulityCost;
					}
				}
			}
		}

		public bool IsSoulBound => (_flags & 1) == 1;

		[MethodImpl(MethodImplOptions.Synchronized)]
		private ulong GetNewGUID()
		{
			ref ulong itemGuidCounter = ref WorldServiceLocator._WorldServer.itemGuidCounter;
			itemGuidCounter = Convert.ToUInt64(decimal.Add(new decimal(itemGuidCounter), 1m));
			return WorldServiceLocator._WorldServer.itemGuidCounter;
		}

		public void FillAllUpdateFlags(ref Packets.UpdateClass update)
		{
			checked
			{
				if (ItemInfo.ContainerSlots > 0)
				{
					update.SetUpdateFlag(0, GUID);
					update.SetUpdateFlag(2, 7);
					update.SetUpdateFlag(3, ItemEntry);
					update.SetUpdateFlag(4, 1f);
					update.SetUpdateFlag(6, OwnerGUID);
					update.SetUpdateFlag(8, OwnerGUID);
					if (decimal.Compare(new decimal(CreatorGUID), 0m) > 0)
					{
						update.SetUpdateFlag(10, CreatorGUID);
					}
					update.SetUpdateFlag(12, GiftCreatorGUID);
					update.SetUpdateFlag(14, StackCount);
					update.SetUpdateFlag(21, _flags);
					update.SetUpdateFlag(48, ItemInfo.ContainerSlots);
					byte i = 0;
					do
					{
						if (Items.ContainsKey(i))
						{
							update.SetUpdateFlag(50 + unchecked((int)i) * 2, (long)Items[i].GUID);
						}
						else
						{
							update.SetUpdateFlag(50 + unchecked((int)i) * 2, 0);
						}
						i = (byte)unchecked((uint)(i + 1));
					}
					while (unchecked((uint)i) <= 35u);
					return;
				}
				update.SetUpdateFlag(0, GUID);
				update.SetUpdateFlag(2, 3);
				update.SetUpdateFlag(3, ItemEntry);
				update.SetUpdateFlag(4, 1f);
				update.SetUpdateFlag(6, OwnerGUID);
				update.SetUpdateFlag(8, OwnerGUID);
				if (decimal.Compare(new decimal(CreatorGUID), 0m) > 0)
				{
					update.SetUpdateFlag(10, CreatorGUID);
				}
				update.SetUpdateFlag(12, GiftCreatorGUID);
				update.SetUpdateFlag(14, StackCount);
				int j = 0;
				do
				{
					if (ItemInfo.Spells[j].SpellTrigger == ITEM_SPELLTRIGGER_TYPE.USE || ItemInfo.Spells[j].SpellTrigger == ITEM_SPELLTRIGGER_TYPE.NO_DELAY_USE)
					{
						update.SetUpdateFlag(16 + j, ChargesLeft);
					}
					else
					{
						update.SetUpdateFlag(16 + j, -1);
					}
					j++;
				}
				while (j <= 4);
				update.SetUpdateFlag(21, _flags);
				update.SetUpdateFlag(44, RandomProperties);
				foreach (KeyValuePair<byte, WS_Items.TEnchantmentInfo> enchant in Enchantments)
				{
					update.SetUpdateFlag(22 + unchecked((int)enchant.Key) * 3, enchant.Value.ID);
					update.SetUpdateFlag(22 + unchecked((int)enchant.Key) * 3 + 1, enchant.Value.Duration);
					update.SetUpdateFlag(22 + unchecked((int)enchant.Key) * 3 + 2, enchant.Value.Charges);
				}
				update.SetUpdateFlag(45, ItemText);
				update.SetUpdateFlag(46, Durability);
				update.SetUpdateFlag(47, WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].Durability);
			}
		}

		public void SendContainedItemsUpdate(ref WS_Network.ClientClass client, int updatetype = 2)
		{
			Packets.PacketClass packet = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
			packet.AddInt32(Items.Count);
			packet.AddInt8(0);
			foreach (KeyValuePair<byte, ItemObject> item in Items)
			{
				Packets.UpdateClass tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_ITEM);
				item.Value.FillAllUpdateFlags(ref tmpUpdate);
				Packets.UpdateClass updateClass = tmpUpdate;
				ItemObject updateObject = item.Value;
				updateClass.AddToPacket(ref packet, (ObjectUpdateType)updatetype, ref updateObject);
				tmpUpdate.Dispose();
			}
			client.Send(ref packet);
		}

		private void InitializeBag()
		{
			if (WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].IsContainer)
			{
				Items = new Dictionary<byte, ItemObject>();
			}
			else
			{
				Items = null;
			}
		}

		public bool GenerateLoot()
		{
			if (_loot != null)
			{
				return true;
			}
			DataTable mySqlQuery = new DataTable();
			WorldServiceLocator._WorldServer.WorldDatabase.Query($"SELECT * FROM item_loot WHERE entry = {ItemEntry};", ref mySqlQuery);
			if (mySqlQuery.Rows.Count == 0)
			{
				return false;
			}
			_loot = new WS_Loot.LootObject(GUID, LootType.LOOTTYPE_CORPSE);
			WorldServiceLocator._WS_Loot.LootTemplates_Item.GetLoot(ItemEntry)?.Process(ref _loot, 0);
			_loot.LootOwner = 0uL;
			return true;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
		public ItemObject(ulong guidVal, WS_PlayerData.CharacterObject owner = null, bool equipped = false)
		{
			GiftCreatorGUID = 0uL;
			StackCount = 1;
			Durability = 1;
			ChargesLeft = 0;
			_flags = 0;
			Items = null;
			RandomProperties = 0;
			SuffixFactor = 0;
			Enchantments = new Dictionary<byte, WS_Items.TEnchantmentInfo>();
			_loot = null;
			ItemText = 0;
			DataTable mySqlQuery = new DataTable();
			WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT * FROM characters_inventory WHERE item_guid = \"{guidVal}\";", ref mySqlQuery);
			if (mySqlQuery.Rows.Count == 0)
			{
				Information.Err().Raise(1, "ItemObject.New", $"itemGuid {guidVal} not found in SQL database!");
			}
			GUID = Conversions.ToULong(Operators.AddObject(mySqlQuery.Rows[0]["item_guid"], WorldServiceLocator._Global_Constants.GUID_ITEM));
			CreatorGUID = Conversions.ToULong(mySqlQuery.Rows[0]["item_creator"]);
			OwnerGUID = Conversions.ToULong(mySqlQuery.Rows[0]["item_owner"]);
			GiftCreatorGUID = Conversions.ToULong(mySqlQuery.Rows[0]["item_giftCreator"]);
			StackCount = Conversions.ToInteger(mySqlQuery.Rows[0]["item_stackCount"]);
			Durability = Conversions.ToInteger(mySqlQuery.Rows[0]["item_durability"]);
			ChargesLeft = Conversions.ToInteger(mySqlQuery.Rows[0]["item_chargesLeft"]);
			RandomProperties = Conversions.ToInteger(mySqlQuery.Rows[0]["item_random_properties"]);
			ItemEntry = Conversions.ToInteger(mySqlQuery.Rows[0]["item_id"]);
			_flags = Conversions.ToInteger(mySqlQuery.Rows[0]["item_flags"]);
			ItemText = Conversions.ToInteger(mySqlQuery.Rows[0]["item_textId"]);
			string[] tmp = Strings.Split(Conversions.ToString(mySqlQuery.Rows[0]["item_enchantment"]));
			checked
			{
				if (tmp.Length > 0)
				{
					int num = tmp.Length - 1;
					for (int i = 0; i <= num; i++)
					{
						if (Operators.CompareString(Strings.Trim(tmp[i]), "", TextCompare: false) != 0)
						{
							string[] tmp2 = Strings.Split(tmp[i], ":");
							Enchantments.Add(Conversions.ToByte(tmp2[0]), new WS_Items.TEnchantmentInfo(Conversions.ToInteger(tmp2[1]), Conversions.ToInteger(tmp2[2]), Conversions.ToInteger(tmp2[3])));
							if (equipped)
							{
								AddEnchantBonus(Conversions.ToByte(tmp2[0]), owner);
							}
						}
					}
				}
				if (!WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(ItemEntry))
				{
					WS_Items.ItemInfo tmpItem2 = new WS_Items.ItemInfo(ItemEntry);
				}
				InitializeBag();
				mySqlQuery.Clear();
				WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT * FROM characters_inventory WHERE item_bag = {GUID};", ref mySqlQuery);
				IEnumerator enumerator = default(IEnumerator);
				try
				{
					enumerator = mySqlQuery.Rows.GetEnumerator();
					while (enumerator.MoveNext())
					{
						DataRow row = (DataRow)enumerator.Current;
						if (Operators.ConditionalCompareObjectNotEqual(row["item_slot"], WorldServiceLocator._Global_Constants.ITEM_SLOT_NULL, TextCompare: false))
						{
							ItemObject tmpItem = new ItemObject((ulong)Conversions.ToLong(row["item_guid"]));
							Items[Conversions.ToByte(row["item_slot"])] = tmpItem;
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
				WorldServiceLocator._WorldServer.WORLD_ITEMs.Add(GUID, this);
			}
		}

		public ItemObject(int itemId, ulong owner)
		{
			GiftCreatorGUID = 0uL;
			StackCount = 1;
			Durability = 1;
			ChargesLeft = 0;
			_flags = 0;
			Items = null;
			RandomProperties = 0;
			SuffixFactor = 0;
			Enchantments = new Dictionary<byte, WS_Items.TEnchantmentInfo>();
			_loot = null;
			ItemText = 0;
			try
			{
				if (!WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(itemId))
				{
					WS_Items.ItemInfo tmpItem = new WS_Items.ItemInfo(itemId);
				}
				ItemEntry = itemId;
				OwnerGUID = owner;
				Durability = WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].Durability;
				int i = 0;
				do
				{
					if ((WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].Spells[i].SpellTrigger == ITEM_SPELLTRIGGER_TYPE.USE || WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].Spells[i].SpellTrigger == ITEM_SPELLTRIGGER_TYPE.NO_DELAY_USE) && WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].Spells[i].SpellCharges != 0)
					{
						ChargesLeft = WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].Spells[i].SpellCharges;
						break;
					}
					i = checked(i + 1);
				}
				while (i <= 4);
				GUID = GetNewGUID();
				InitializeBag();
				SaveAsNew();
				WorldServiceLocator._WorldServer.WORLD_ITEMs.Add(GUID, this);
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Exception Ex = ex;
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Duplicate Key Warning ITEMID:{0} OWNERGUID:{1}", itemId, owner);
				ProjectData.ClearProjectError();
			}
		}

		private void SaveAsNew()
		{
			string tmpCmd = "INSERT INTO characters_inventory (item_guid";
			string tmpValues = " VALUES (" + Conversions.ToString(checked(GUID - WorldServiceLocator._Global_Constants.GUID_ITEM));
			tmpCmd += ", item_owner";
			tmpValues = tmpValues + ", \"" + Conversions.ToString(OwnerGUID) + "\"";
			tmpCmd += ", item_creator";
			tmpValues = tmpValues + ", " + Conversions.ToString(CreatorGUID);
			tmpCmd += ", item_giftCreator";
			tmpValues = tmpValues + ", " + Conversions.ToString(GiftCreatorGUID);
			tmpCmd += ", item_stackCount";
			tmpValues = tmpValues + ", " + Conversions.ToString(StackCount);
			tmpCmd += ", item_durability";
			tmpValues = tmpValues + ", " + Conversions.ToString(Durability);
			tmpCmd += ", item_chargesLeft";
			tmpValues = tmpValues + ", " + Conversions.ToString(ChargesLeft);
			tmpCmd += ", item_random_properties";
			tmpValues = tmpValues + ", " + Conversions.ToString(RandomProperties);
			tmpCmd += ", item_id";
			tmpValues = tmpValues + ", " + Conversions.ToString(ItemEntry);
			tmpCmd += ", item_flags";
			tmpValues = tmpValues + ", " + Conversions.ToString(_flags);
			ArrayList temp = new ArrayList();
			foreach (KeyValuePair<byte, WS_Items.TEnchantmentInfo> enchantment in Enchantments)
			{
				temp.Add($"{enchantment.Key}:{enchantment.Value.ID}:{enchantment.Value.Duration}:{enchantment.Value.Charges}");
			}
			tmpCmd += ", item_enchantment";
			tmpValues = tmpValues + ", '" + Strings.Join(temp.ToArray()) + "'";
			tmpCmd += ", item_textId";
			tmpValues = tmpValues + ", " + Conversions.ToString(ItemText);
			tmpCmd = tmpCmd + ") " + tmpValues + ");";
			WorldServiceLocator._WorldServer.CharacterDatabase.Update(tmpCmd);
		}

		public void Save(bool saveAll = true)
		{
			string tmp = "UPDATE characters_inventory SET";
			tmp = tmp + " item_owner=\"" + Conversions.ToString(OwnerGUID) + "\"";
			tmp = tmp + ", item_creator=" + Conversions.ToString(CreatorGUID);
			tmp = tmp + ", item_giftCreator=" + Conversions.ToString(GiftCreatorGUID);
			tmp = tmp + ", item_stackCount=" + Conversions.ToString(StackCount);
			tmp = tmp + ", item_durability=" + Conversions.ToString(Durability);
			tmp = tmp + ", item_chargesLeft=" + Conversions.ToString(ChargesLeft);
			tmp = tmp + ", item_random_properties=" + Conversions.ToString(RandomProperties);
			tmp = tmp + ", item_flags=" + Conversions.ToString(_flags);
			ArrayList temp = new ArrayList();
			foreach (KeyValuePair<byte, WS_Items.TEnchantmentInfo> enchantment in Enchantments)
			{
				temp.Add($"{enchantment.Key}:{enchantment.Value.ID}:{enchantment.Value.Duration}:{enchantment.Value.Charges}");
			}
			tmp = tmp + ", item_enchantment=\"" + Strings.Join(temp.ToArray()) + "\"";
			tmp = tmp + ", item_textId=" + Conversions.ToString(ItemText);
			tmp = tmp + " WHERE item_guid = \"" + Conversions.ToString(checked(GUID - WorldServiceLocator._Global_Constants.GUID_ITEM)) + "\";";
			WorldServiceLocator._WorldServer.CharacterDatabase.Update(tmp);
			if (!WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].IsContainer || !saveAll)
			{
				return;
			}
			foreach (KeyValuePair<byte, ItemObject> item in Items)
			{
				item.Value.Save();
			}
		}

		public void Delete()
		{
			checked
			{
				if (ItemEntry == WorldServiceLocator._Global_Constants.PETITION_GUILD)
				{
					WorldServiceLocator._WorldServer.CharacterDatabase.Update("DELETE FROM petitions WHERE petition_itemGuid = " + Conversions.ToString(GUID - WorldServiceLocator._Global_Constants.GUID_ITEM) + ";");
				}
				WorldServiceLocator._WorldServer.CharacterDatabase.Update($"DELETE FROM characters_inventory WHERE item_guid = {GUID - WorldServiceLocator._Global_Constants.GUID_ITEM}");
				if (WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].IsContainer)
				{
					foreach (KeyValuePair<byte, ItemObject> item in Items)
					{
						item.Value.Delete();
					}
				}
				Dispose();
			}
		}

		private void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				WorldServiceLocator._WorldServer.WORLD_ITEMs.Remove(GUID);
				if (WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].IsContainer)
				{
					foreach (KeyValuePair<byte, ItemObject> item in Items)
					{
						item.Value.Dispose();
					}
				}
				if (!Information.IsNothing(_loot))
				{
					_loot.Dispose();
				}
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

		public bool IsBroken()
		{
			return Durability == 0 && ItemInfo.Durability > 0;
		}

		public void ModifyDurability(float percent, ref WS_Network.ClientClass client)
		{
			if (WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].Durability > 0)
			{
				ref int durability = ref Durability;
				durability = checked((int)Math.Round((float)durability - Conversion.Fix((float)WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].Durability * percent)));
				if (Durability < 0)
				{
					Durability = 0;
				}
				if (Durability > WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].Durability)
				{
					Durability = WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].Durability;
				}
				UpdateDurability(ref client);
			}
		}

		public void ModifyToDurability(float percent, ref WS_Network.ClientClass client)
		{
			if (WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].Durability > 0)
			{
				Durability = checked((int)((float)WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].Durability * percent));
				if (Durability < 0)
				{
					Durability = 0;
				}
				if (Durability > WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].Durability)
				{
					Durability = WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].Durability;
				}
				UpdateDurability(ref client);
			}
		}

		private void UpdateDurability(ref WS_Network.ClientClass client)
		{
			Packets.PacketClass packet = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
			packet.AddInt32(1);
			packet.AddInt8(0);
			Packets.UpdateClass tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_ITEM);
			tmpUpdate.SetUpdateFlag(46, Durability);
			ItemObject updateObject = this;
			tmpUpdate.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
			tmpUpdate.Dispose();
			client.Send(ref packet);
		}

		public void AddEnchantment(int id, byte slot, int duration = 0, int charges = 0)
		{
			if (Enchantments.ContainsKey(slot))
			{
				RemoveEnchantment(slot);
			}
			Enchantments.Add(slot, new WS_Items.TEnchantmentInfo(id, duration, charges));
			WS_PlayerData.CharacterObject objCharacter = null;
			AddEnchantBonus(slot, objCharacter);
		}

		public void AddEnchantBonus(byte slot, WS_PlayerData.CharacterObject objCharacter = null)
		{
			if (objCharacter == null)
			{
				if (!WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(OwnerGUID))
				{
					return;
				}
				objCharacter = WorldServiceLocator._WorldServer.CHARACTERs[OwnerGUID];
			}
			if (objCharacter == null || !WorldServiceLocator._WS_DBCDatabase.SpellItemEnchantments.ContainsKey(Enchantments[slot].ID))
			{
				return;
			}
			byte i = 0;
			do
			{
				checked
				{
					if (WorldServiceLocator._WS_DBCDatabase.SpellItemEnchantments[Enchantments[slot].ID].SpellID[i] != 0 && WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(WorldServiceLocator._WS_DBCDatabase.SpellItemEnchantments[Enchantments[slot].ID].SpellID[i]))
					{
						WS_Spells.SpellInfo spellInfo = WorldServiceLocator._WS_Spells.SPELLs[WorldServiceLocator._WS_DBCDatabase.SpellItemEnchantments[Enchantments[slot].ID].SpellID[i]];
						byte j = 0;
						do
						{
							if (spellInfo.SpellEffects[j] != null)
							{
								SpellEffects_Names iD = spellInfo.SpellEffects[j].ID;
								if (iD == SpellEffects_Names.SPELL_EFFECT_APPLY_AURA)
								{
									WS_Spells.ApplyAuraHandler obj = WorldServiceLocator._WS_Spells.AURAs[spellInfo.SpellEffects[j].ApplyAuraIndex];
									WS_Base.BaseUnit Target = objCharacter;
									WS_Base.BaseObject Caster = objCharacter;
									obj(ref Target, ref Caster, ref spellInfo.SpellEffects[j], spellInfo.ID, 1, AuraAction.AURA_ADD);
									objCharacter = (WS_PlayerData.CharacterObject)Caster;
									objCharacter = (WS_PlayerData.CharacterObject)Target;
								}
							}
							j = (byte)unchecked((uint)(j + 1));
						}
						while (unchecked((uint)j) <= 2u);
					}
					i = (byte)unchecked((uint)(i + 1));
				}
			}
			while ((uint)i <= 2u);
		}

		public void RemoveEnchantBonus(byte slot)
		{
			if (!WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(OwnerGUID) || !WorldServiceLocator._WS_DBCDatabase.SpellItemEnchantments.ContainsKey(Enchantments[slot].ID))
			{
				return;
			}
			byte i = 0;
			do
			{
				checked
				{
					if (WorldServiceLocator._WS_DBCDatabase.SpellItemEnchantments[Enchantments[slot].ID].SpellID[i] != 0 && WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(WorldServiceLocator._WS_DBCDatabase.SpellItemEnchantments[Enchantments[slot].ID].SpellID[i]))
					{
						WS_Spells.SpellInfo spellInfo = WorldServiceLocator._WS_Spells.SPELLs[WorldServiceLocator._WS_DBCDatabase.SpellItemEnchantments[Enchantments[slot].ID].SpellID[i]];
						byte j = 0;
						do
						{
							if (spellInfo.SpellEffects[j] != null)
							{
								SpellEffects_Names iD = spellInfo.SpellEffects[j].ID;
								if (iD == SpellEffects_Names.SPELL_EFFECT_APPLY_AURA)
								{
									WS_Spells.ApplyAuraHandler obj = WorldServiceLocator._WS_Spells.AURAs[spellInfo.SpellEffects[j].ApplyAuraIndex];
									Dictionary<ulong, WS_PlayerData.CharacterObject> cHARACTERs;
									ulong ownerGUID;
									WS_Base.BaseUnit Target = (cHARACTERs = WorldServiceLocator._WorldServer.CHARACTERs)[ownerGUID = OwnerGUID];
									Dictionary<ulong, WS_PlayerData.CharacterObject> cHARACTERs2;
									ulong ownerGUID2;
									WS_Base.BaseObject Caster = (cHARACTERs2 = WorldServiceLocator._WorldServer.CHARACTERs)[ownerGUID2 = OwnerGUID];
									obj(ref Target, ref Caster, ref spellInfo.SpellEffects[j], spellInfo.ID, 1, AuraAction.AURA_REMOVE);
									cHARACTERs2[ownerGUID2] = (WS_PlayerData.CharacterObject)Caster;
									cHARACTERs[ownerGUID] = (WS_PlayerData.CharacterObject)Target;
								}
							}
							j = (byte)unchecked((uint)(j + 1));
						}
						while (unchecked((uint)j) <= 2u);
					}
					i = (byte)unchecked((uint)(i + 1));
				}
			}
			while ((uint)i <= 2u);
		}

		private void RemoveEnchantment(byte slot)
		{
			checked
			{
				if (Enchantments.ContainsKey(slot))
				{
					RemoveEnchantBonus(slot);
					Enchantments.Remove(slot);
					if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(OwnerGUID))
					{
						Packets.PacketClass packet = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
						packet.AddInt32(1);
						packet.AddInt8(0);
						Packets.UpdateClass tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_ITEM);
						tmpUpdate.SetUpdateFlag(22 + unchecked((int)slot) * 3, 0);
						tmpUpdate.SetUpdateFlag(22 + unchecked((int)slot) * 3 + 1, 0);
						tmpUpdate.SetUpdateFlag(22 + unchecked((int)slot) * 3 + 2, 0);
						ItemObject updateObject = this;
						tmpUpdate.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
						WorldServiceLocator._WorldServer.CHARACTERs[OwnerGUID].client.Send(ref packet);
						packet.Dispose();
						tmpUpdate.Dispose();
					}
				}
			}
		}

		public void SoulbindItem(WS_Network.ClientClass client = null)
		{
			if ((_flags & 1) != 1)
			{
				_flags |= 1;
				Save();
				if (client != null)
				{
					Packets.PacketClass packet = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
					packet.AddInt32(1);
					packet.AddInt8(0);
					Packets.UpdateClass tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_ITEM);
					tmpUpdate.SetUpdateFlag(21, _flags);
					ItemObject updateObject = this;
					tmpUpdate.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
					client.Send(ref packet);
					packet.Dispose();
					tmpUpdate.Dispose();
				}
			}
		}
	}
}
