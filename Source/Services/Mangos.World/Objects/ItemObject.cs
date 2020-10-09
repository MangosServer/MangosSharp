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

// WARNING: Use only with ITEMs()
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Item;
using Mangos.Common.Enums.Player;
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
        public WS_Items.ItemInfo ItemInfo
        {
            get
            {
                return WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry];
            }
        }

        public readonly int ItemEntry;
        public ulong GUID;
        public ulong OwnerGUID;
        public readonly ulong GiftCreatorGUID = 0UL;
        public readonly ulong CreatorGUID;
        public int StackCount = 1;
        public int Durability = 1;
        public int ChargesLeft = 0;
        private int _flags = 0;
        public Dictionary<byte, ItemObject> Items = null;
        public readonly int RandomProperties = 0;
        public int SuffixFactor = 0;
        public readonly Dictionary<byte, WS_Items.TEnchantmentInfo> Enchantments = new Dictionary<byte, WS_Items.TEnchantmentInfo>();
        private WS_Loot.LootObject _loot = null;

        // WARNING: Containers cannot hold itemText value
        public int ItemText = 0;

        [MethodImpl(MethodImplOptions.Synchronized)]
        private ulong GetNewGUID()
        {
            ulong GetNewGUIDRet = default;
            WorldServiceLocator._WorldServer.itemGuidCounter = (ulong)(WorldServiceLocator._WorldServer.itemGuidCounter + 1m);
            GetNewGUIDRet = WorldServiceLocator._WorldServer.itemGuidCounter;
            return GetNewGUIDRet;
        }

        public void FillAllUpdateFlags(ref Packets.UpdateClass update)
        {
            if (ItemInfo.ContainerSlots > 0)
            {
                update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_GUID, GUID);
                update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_TYPE, ObjectType.TYPE_CONTAINER + ObjectType.TYPE_OBJECT);
                update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_ENTRY, ItemEntry);
                update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_SCALE_X, 1.0f);
                update.SetUpdateFlag(EItemFields.ITEM_FIELD_OWNER, OwnerGUID);
                update.SetUpdateFlag(EItemFields.ITEM_FIELD_CONTAINED, OwnerGUID);
                if (CreatorGUID > 0m)
                    update.SetUpdateFlag(EItemFields.ITEM_FIELD_CREATOR, CreatorGUID);
                update.SetUpdateFlag(EItemFields.ITEM_FIELD_GIFTCREATOR, GiftCreatorGUID);
                update.SetUpdateFlag(EItemFields.ITEM_FIELD_STACK_COUNT, StackCount);
                // Update.SetUpdateFlag(EItemFields.ITEM_FIELD_DURATION, 0)
                update.SetUpdateFlag(EItemFields.ITEM_FIELD_FLAGS, _flags);
                // Update.SetUpdateFlag(EItemFields.ITEM_FIELD_ITEM_TEXT_ID, ItemText)

                update.SetUpdateFlag(EContainerFields.CONTAINER_FIELD_NUM_SLOTS, ItemInfo.ContainerSlots);
                // DONE: Here list in bag items
                for (byte i = 0; i <= 35; i++)
                {
                    if (Items.ContainsKey(i))
                    {
                        update.SetUpdateFlag(EContainerFields.CONTAINER_FIELD_SLOT_1 + (int)i * 2, Conversions.ToLong(Items[i].GUID));
                    }
                    else
                    {
                        update.SetUpdateFlag(EContainerFields.CONTAINER_FIELD_SLOT_1 + (int)i * 2, 0);
                    }
                }
            }
            else
            {
                update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_GUID, GUID);
                update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_TYPE, ObjectType.TYPE_ITEM + ObjectType.TYPE_OBJECT);
                update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_ENTRY, ItemEntry);
                update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_SCALE_X, 1.0f);
                update.SetUpdateFlag(EItemFields.ITEM_FIELD_OWNER, OwnerGUID);
                update.SetUpdateFlag(EItemFields.ITEM_FIELD_CONTAINED, OwnerGUID);
                if (CreatorGUID > 0m)
                    update.SetUpdateFlag(EItemFields.ITEM_FIELD_CREATOR, CreatorGUID);
                update.SetUpdateFlag(EItemFields.ITEM_FIELD_GIFTCREATOR, GiftCreatorGUID);
                update.SetUpdateFlag(EItemFields.ITEM_FIELD_STACK_COUNT, StackCount);
                // Update.SetUpdateFlag(EItemFields.ITEM_FIELD_DURATION, 0)
                for (int i = 0; i <= 4; i++)
                {
                    if (ItemInfo.Spells[i].SpellTrigger == ITEM_SPELLTRIGGER_TYPE.USE || ItemInfo.Spells[i].SpellTrigger == ITEM_SPELLTRIGGER_TYPE.NO_DELAY_USE)
                    {
                        update.SetUpdateFlag(EItemFields.ITEM_FIELD_SPELL_CHARGES + i, ChargesLeft);
                    }
                    else
                    {
                        update.SetUpdateFlag(EItemFields.ITEM_FIELD_SPELL_CHARGES + i, -1);
                    }
                }

                update.SetUpdateFlag(EItemFields.ITEM_FIELD_FLAGS, _flags);

                // Update.SetUpdateFlag(EItemFields.ITEM_FIELD_PROPERTY_SEED, 0)
                update.SetUpdateFlag(EItemFields.ITEM_FIELD_RANDOM_PROPERTIES_ID, RandomProperties);
                foreach (KeyValuePair<byte, WS_Items.TEnchantmentInfo> enchant in Enchantments)
                {
                    update.SetUpdateFlag(EItemFields.ITEM_FIELD_ENCHANTMENT + (int)enchant.Key * 3, enchant.Value.ID);
                    update.SetUpdateFlag(EItemFields.ITEM_FIELD_ENCHANTMENT + (int)enchant.Key * 3 + 1, enchant.Value.Duration);
                    update.SetUpdateFlag(EItemFields.ITEM_FIELD_ENCHANTMENT + (int)enchant.Key * 3 + 2, enchant.Value.Charges);
                }

                update.SetUpdateFlag(EItemFields.ITEM_FIELD_ITEM_TEXT_ID, ItemText);
                update.SetUpdateFlag(EItemFields.ITEM_FIELD_DURABILITY, Durability);
                update.SetUpdateFlag(EItemFields.ITEM_FIELD_MAXDURABILITY, WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].Durability);
            }
        }

        public void SendContainedItemsUpdate(ref WS_Network.ClientClass client, int updatetype = ObjectUpdateType.UPDATETYPE_CREATE_OBJECT)
        {
            var packet = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
            packet.AddInt32(Items.Count);      // Operations.Count
            packet.AddInt8(0);
            foreach (KeyValuePair<byte, ItemObject> item in Items)
            {
                var tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_ITEM);
                item.Value.FillAllUpdateFlags(ref tmpUpdate);
                tmpUpdate.AddToPacket(packet, updatetype, item.Value);
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

        public bool IsFree
        {
            get
            {
                if (Items.Count > 0)
                    return false;
                else
                    return true;
            }
        }
        // Public ReadOnly Property IsFull() As Boolean
        // Get
        // If Items.Count = _WorldServer.ITEMDatabase(ItemEntry).ContainerSlots Then Return True Else Return False
        // End Get
        // End Property
        // Public ReadOnly Property IsEquipped() As Boolean
        // Get
        // Dim srcBag As Byte = GetBagSlot
        // Dim srcSlot As Integer = GetSlot
        // If srcBag = 255 AndAlso srcSlot < EQUIPMENT_SLOT_END AndAlso srcSlot >= 0 Then Return True
        // Return False
        // End Get
        // End Property
        public bool IsRanged
        {
            get
            {
                return ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_WEAPON && (ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_BOW || ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_CROSSBOW || ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_GUN);
            }
        }

        public byte GetBagSlot
        {
            get
            {
                if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(OwnerGUID) == false)
                    return 255;
                {
                    var withBlock = WorldServiceLocator._WorldServer.CHARACTERs[OwnerGUID];
                    for (byte i = InventorySlots.INVENTORY_SLOT_BAG_1, loopTo = InventorySlots.INVENTORY_SLOT_BAG_END - 1; i <= loopTo; i++)
                    {
                        if (withBlock.Items.ContainsKey(i))
                        {
                            for (byte j = 0, loopTo1 = (byte)(withBlock.Items[i].ItemInfo.ContainerSlots - 1); j <= loopTo1; j++)
                            {
                                if (withBlock.Items[i].Items.ContainsKey(j))
                                {
                                    if (ReferenceEquals(withBlock.Items[i].Items[j], this))
                                        return i;
                                }
                            }
                        }
                    }
                }

                return 255;
            }
        }

        public int GetSlot
        {
            get
            {
                if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(OwnerGUID) == false)
                    return -1;
                {
                    var withBlock = WorldServiceLocator._WorldServer.CHARACTERs[OwnerGUID];
                    for (byte i = EquipmentSlots.EQUIPMENT_SLOT_START, loopTo = InventoryPackSlots.INVENTORY_SLOT_ITEM_END - 1; i <= loopTo; i++)
                    {
                        if (withBlock.Items.ContainsKey(i))
                        {
                            if (ReferenceEquals(withBlock.Items[i], this))
                                return i;
                        }
                    }

                    for (byte i = KeyRingSlots.KEYRING_SLOT_START, loopTo1 = KeyRingSlots.KEYRING_SLOT_END - 1; i <= loopTo1; i++)
                    {
                        if (withBlock.Items.ContainsKey(i))
                        {
                            if (ReferenceEquals(withBlock.Items[i], this))
                                return i;
                        }
                    }

                    for (byte i = InventorySlots.INVENTORY_SLOT_BAG_1, loopTo2 = InventorySlots.INVENTORY_SLOT_BAG_END - 1; i <= loopTo2; i++)
                    {
                        if (withBlock.Items.ContainsKey(i))
                        {
                            for (byte j = 0, loopTo3 = (byte)(withBlock.Items[i].ItemInfo.ContainerSlots - 1); j <= loopTo3; j++)
                            {
                                if (withBlock.Items[i].Items.ContainsKey(j))
                                {
                                    if (ReferenceEquals(withBlock.Items[i].Items[j], this))
                                        return j;
                                }
                            }
                        }
                    }
                }

                return -1;
            }
        }

        public int GetSkill
        {
            get
            {
                return ItemInfo.GetReqSkill;
            }
        }

        public bool GenerateLoot()
        {
            if (_loot is object)
                return true;

            // DONE: Loot generation
            var mySqlQuery = new DataTable();
            WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT * FROM item_loot WHERE entry = {0};", (object)ItemEntry), mySqlQuery);
            if (mySqlQuery.Rows.Count == 0)
                return false;
            _loot = new WS_Loot.LootObject(GUID, LootType.LOOTTYPE_CORPSE);
            var template = WorldServiceLocator._WS_Loot.LootTemplates_Item.GetLoot(ItemEntry);
            if (template is object)
            {
                template.Process(ref _loot, 0);
            }

            _loot.LootOwner = 0UL;
            return true;
        }

        public ItemObject(ulong guidVal, WS_PlayerData.CharacterObject owner = null, bool equipped = false)
        {
            // DONE: Get from SQLDB
            var mySqlQuery = new DataTable();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT * FROM characters_inventory WHERE item_guid = \"{0}\";", (object)guidVal), mySqlQuery);
            if (mySqlQuery.Rows.Count == 0)
                Information.Err().Raise(1, "ItemObject.New", string.Format("itemGuid {0} not found in SQL database!", guidVal));
            GUID = mySqlQuery.Rows[0]["item_guid"] + WorldServiceLocator._Global_Constants.GUID_ITEM;
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

            // DONE: Intitialize enchantments - Saved as STRING like "Slot1:ID1:Duration:Charges Slot2:ID2:Duration:Charges Slot3:ID3:Duration:Charges"
            var tmp = Strings.Split(Conversions.ToString(mySqlQuery.Rows[0]["item_enchantment"]), " ");
            if (tmp.Length > 0)
            {
                for (int i = 0, loopTo = tmp.Length - 1; i <= loopTo; i++)
                {
                    if (!string.IsNullOrEmpty(Strings.Trim(tmp[i])))
                    {
                        string[] tmp2;
                        tmp2 = Strings.Split(tmp[i], ":");
                        // DONE: Add the enchantment
                        Enchantments.Add(Conversions.ToByte(tmp2[0]), new WS_Items.TEnchantmentInfo(Conversions.ToInteger(tmp2[1]), Conversions.ToInteger(tmp2[2]), Conversions.ToInteger(tmp2[3])));
                        // DONE: Add the bonuses to the character
                        if (equipped)
                            AddEnchantBonus(Conversions.ToByte(tmp2[0]), ref owner);
                    }
                }
            }

            // DONE: Load ItemID in cashe if not loaded
            if (WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(ItemEntry) == false)
            {
                // TODO: This needs to actually do something
                var tmpItem = new WS_Items.ItemInfo(ItemEntry);
            }

            InitializeBag();

            // DONE: Get Items
            mySqlQuery.Clear();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT * FROM characters_inventory WHERE item_bag = {0};", (object)GUID), mySqlQuery);
            foreach (DataRow row in mySqlQuery.Rows)
            {
                if (!Operators.ConditionalCompareObjectEqual(row["item_slot"], WorldServiceLocator._Global_Constants.ITEM_SLOT_NULL, false))
                {
                    var tmpItem = new ItemObject((ulong)Conversions.ToLong(row["item_guid"]));
                    Items[Conversions.ToByte(row["item_slot"])] = tmpItem;
                }
            }

            WorldServiceLocator._WorldServer.WORLD_ITEMs.Add(GUID, this);
        }

        public ItemObject(int itemId, ulong owner)
        {
            // DONE: Load ItemID in cashe if not loaded
            try
            {
                if (WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(itemId) == false)
                {
                    // TODO: This needs to actually do something
                    var tmpItem = new WS_Items.ItemInfo(itemId);
                }

                ItemEntry = itemId;
                OwnerGUID = owner;
                Durability = WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].Durability;
                for (int i = 0; i <= 4; i++)
                {
                    if (WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].Spells[i].SpellTrigger == ITEM_SPELLTRIGGER_TYPE.USE || WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].Spells[i].SpellTrigger == ITEM_SPELLTRIGGER_TYPE.NO_DELAY_USE)
                    {
                        if (WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].Spells[i].SpellCharges != 0)
                        {
                            ChargesLeft = WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].Spells[i].SpellCharges;
                            break;
                        }
                    }
                }

                // DONE: Create new GUID
                GUID = GetNewGUID();
                InitializeBag();
                SaveAsNew();
                WorldServiceLocator._WorldServer.WORLD_ITEMs.Add(GUID, this);
            }
            catch (Exception Ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Duplicate Key Warning ITEMID:{0} OWNERGUID:{1}", itemId, owner);
            }
        }

        private void SaveAsNew()
        {
            // DONE: Save to SQL
            string tmpCmd = "INSERT INTO characters_inventory (item_guid";
            string tmpValues = " VALUES (" + (GUID - WorldServiceLocator._Global_Constants.GUID_ITEM);
            tmpCmd += ", item_owner";
            tmpValues = tmpValues + ", \"" + OwnerGUID + "\"";
            tmpCmd += ", item_creator";
            tmpValues = tmpValues + ", " + CreatorGUID;
            tmpCmd += ", item_giftCreator";
            tmpValues = tmpValues + ", " + GiftCreatorGUID;
            tmpCmd += ", item_stackCount";
            tmpValues = tmpValues + ", " + StackCount;
            tmpCmd += ", item_durability";
            tmpValues = tmpValues + ", " + Durability;
            tmpCmd += ", item_chargesLeft";
            tmpValues = tmpValues + ", " + ChargesLeft;
            tmpCmd += ", item_random_properties";
            tmpValues = tmpValues + ", " + RandomProperties;
            tmpCmd += ", item_id";
            tmpValues = tmpValues + ", " + ItemEntry;
            tmpCmd += ", item_flags";
            tmpValues = tmpValues + ", " + _flags;

            // DONE: Saving enchanments
            var temp = new ArrayList();
            foreach (KeyValuePair<byte, WS_Items.TEnchantmentInfo> enchantment in Enchantments)
                temp.Add(string.Format("{0}:{1}:{2}:{3}", enchantment.Key, enchantment.Value.ID, enchantment.Value.Duration, enchantment.Value.Charges));
            tmpCmd += ", item_enchantment";
            tmpValues = tmpValues + ", '" + Strings.Join(temp.ToArray(), " ") + "'";
            tmpCmd += ", item_textId";
            tmpValues = tmpValues + ", " + ItemText;
            tmpCmd = tmpCmd + ") " + tmpValues + ");";
            WorldServiceLocator._WorldServer.CharacterDatabase.Update(tmpCmd);
        }

        public void Save(bool saveAll = true)
        {
            string tmp = "UPDATE characters_inventory SET";
            tmp = tmp + " item_owner=\"" + OwnerGUID + "\"";
            tmp = tmp + ", item_creator=" + CreatorGUID;
            tmp = tmp + ", item_giftCreator=" + GiftCreatorGUID;
            tmp = tmp + ", item_stackCount=" + StackCount;
            tmp = tmp + ", item_durability=" + Durability;
            tmp = tmp + ", item_chargesLeft=" + ChargesLeft;
            tmp = tmp + ", item_random_properties=" + RandomProperties;
            tmp = tmp + ", item_flags=" + _flags;

            // DONE: Saving enchanments
            var temp = new ArrayList();
            foreach (KeyValuePair<byte, WS_Items.TEnchantmentInfo> enchantment in Enchantments)
                temp.Add(string.Format("{0}:{1}:{2}:{3}", enchantment.Key, enchantment.Value.ID, enchantment.Value.Duration, enchantment.Value.Charges));
            tmp = tmp + ", item_enchantment=\"" + Strings.Join(temp.ToArray(), " ") + "\"";
            tmp = tmp + ", item_textId=" + ItemText;
            tmp = tmp + " WHERE item_guid = \"" + (GUID - WorldServiceLocator._Global_Constants.GUID_ITEM) + "\";";
            WorldServiceLocator._WorldServer.CharacterDatabase.Update(tmp);
            if (WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].IsContainer && saveAll)
            {
                foreach (KeyValuePair<byte, ItemObject> item in Items)
                    item.Value.Save();
            }
        }

        public void Delete()
        {
            // DONE: Check if item is petition
            if (ItemEntry == WorldServiceLocator._Global_Constants.PETITION_GUILD)

                WorldServiceLocator._WorldServer.CharacterDatabase.Update("DELETE FROM petitions WHERE petition_itemGuid = " + (GUID - WorldServiceLocator._Global_Constants.GUID_ITEM) + ";");
            WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("DELETE FROM characters_inventory WHERE item_guid = {0}", GUID - WorldServiceLocator._Global_Constants.GUID_ITEM));
            if (WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].IsContainer)
            {
                foreach (KeyValuePair<byte, ItemObject> item in Items)
                    item.Value.Delete();
            }

            Dispose();
        }

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        private bool _disposedValue; // To detect redundant calls

        // IDisposable
        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                // TODO: set large fields to null.
                WorldServiceLocator._WorldServer.WORLD_ITEMs.Remove(GUID);
                if (WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].IsContainer)
                {
                    foreach (KeyValuePair<byte, ItemObject> item in Items)
                        item.Value.Dispose();
                }

                if (!Information.IsNothing(_loot))
                    _loot.Dispose();
            }

            _disposedValue = true;
        }

        // This code added by Visual Basic to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        public bool IsBroken()
        {
            return Durability == 0 && ItemInfo.Durability > 0;
        }

        public void ModifyDurability(float percent, ref WS_Network.ClientClass client)
        {
            if (WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].Durability > 0)
            {
                Durability = (int)(Durability - Conversion.Fix(WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].Durability * percent));
                if (Durability < 0)
                    Durability = 0;
                if (Durability > WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].Durability)
                    Durability = WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].Durability;
                UpdateDurability(ref client);
            }
        }

        public void ModifyToDurability(float percent, ref WS_Network.ClientClass client)
        {
            if (WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].Durability > 0)
            {
                Durability = (int)Conversion.Fix(WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].Durability * percent);
                if (Durability < 0)
                    Durability = 0;
                if (Durability > WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].Durability)
                    Durability = WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].Durability;
                UpdateDurability(ref client);
            }
        }

        private void UpdateDurability(ref WS_Network.ClientClass client)
        {
            var packet = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
            packet.AddInt32(1);      // Operations.Count
            packet.AddInt8(0);
            var tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_ITEM);
            tmpUpdate.SetUpdateFlag(EItemFields.ITEM_FIELD_DURABILITY, Durability);
            tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, this);
            tmpUpdate.Dispose();
            client.Send(ref packet);
        }

        public uint GetDurabulityCost
        {
            get
            {
                try
                {
                    int lostDurability = WorldServiceLocator._WorldServer.ITEMDatabase[ItemEntry].Durability - Durability;
                    if (lostDurability > DataStores.WS_DBCDatabase.DurabilityCosts_MAX)
                        lostDurability = DataStores.WS_DBCDatabase.DurabilityCosts_MAX;
                    int subClass = 0;
                    if (ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_WEAPON)
                        subClass = ItemInfo.SubClass;
                    else
                        subClass = ItemInfo.SubClass + 21;
                    uint durabilityCost = (uint)(lostDurability * (WorldServiceLocator._WS_DBCDatabase.DurabilityCosts[ItemInfo.Level, subClass] / 40d * 100d));
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Durability cost: {0}", durabilityCost);
                    return durabilityCost;
                }
                catch
                {
                    return 0U;
                }
            }
        }

        public void AddEnchantment(int id, byte slot, int duration = 0, int charges = 0)
        {
            // DONE: Replace if an enchant already is placed in this slot
            if (Enchantments.ContainsKey(slot))
                RemoveEnchantment(slot);
            // DONE: Add the enchantment
            Enchantments.Add(slot, new WS_Items.TEnchantmentInfo(id, duration, charges));
            WS_PlayerData.CharacterObject argobjCharacter = null;
            // DONE: Add the bonuses to the character if it's equipped
            AddEnchantBonus(slot, objCharacter: ref argobjCharacter);
        }

        public void AddEnchantBonus(byte slot, [Optional, DefaultParameterValue(null)] ref WS_PlayerData.CharacterObject objCharacter)
        {
            if (objCharacter is null)
            {
                if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(OwnerGUID) == false)
                    return;
                objCharacter = WorldServiceLocator._WorldServer.CHARACTERs[OwnerGUID];
            }

            if (objCharacter is object && WorldServiceLocator._WS_DBCDatabase.SpellItemEnchantments.ContainsKey(Enchantments[slot].ID))
            {
                for (byte i = 0; i <= 2; i++)
                {
                    if (WorldServiceLocator._WS_DBCDatabase.SpellItemEnchantments[Enchantments[slot].ID].SpellID[i] != 0)
                    {
                        if (WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(WorldServiceLocator._WS_DBCDatabase.SpellItemEnchantments[Enchantments[slot].ID].SpellID[i]))
                        {
                            WS_Spells.SpellInfo spellInfo;
                            spellInfo = WorldServiceLocator._WS_Spells.SPELLs[WorldServiceLocator._WS_DBCDatabase.SpellItemEnchantments[Enchantments[slot].ID].SpellID[i]];
                            for (byte j = 0; j <= 2; j++)
                            {
                                if (spellInfo.SpellEffects[j] is object)
                                {
                                    switch (spellInfo.SpellEffects[j].ID)
                                    {
                                        case var @case when @case == SpellEffects_Names.SPELL_EFFECT_APPLY_AURA:
                                            {
                                                WorldServiceLocator._WS_Spells.AURAs[spellInfo.SpellEffects[(int)j].ApplyAuraIndex].Invoke(ref objCharacter, ref objCharacter, ref spellInfo.SpellEffects[(int)j], spellInfo.ID, 1, AuraAction.AURA_ADD);
                                                break;
                                            }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void RemoveEnchantBonus(byte slot)
        {
            if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(OwnerGUID) && WorldServiceLocator._WS_DBCDatabase.SpellItemEnchantments.ContainsKey(Enchantments[slot].ID))
            {
                for (byte i = 0; i <= 2; i++)
                {
                    if (WorldServiceLocator._WS_DBCDatabase.SpellItemEnchantments[Enchantments[slot].ID].SpellID[i] != 0)
                    {
                        if (WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(WorldServiceLocator._WS_DBCDatabase.SpellItemEnchantments[Enchantments[slot].ID].SpellID[i]))
                        {
                            WS_Spells.SpellInfo spellInfo;
                            spellInfo = WorldServiceLocator._WS_Spells.SPELLs[WorldServiceLocator._WS_DBCDatabase.SpellItemEnchantments[Enchantments[slot].ID].SpellID[i]];
                            for (byte j = 0; j <= 2; j++)
                            {
                                if (spellInfo.SpellEffects[j] is object)
                                {
                                    switch (spellInfo.SpellEffects[j].ID)
                                    {
                                        case var @case when @case == SpellEffects_Names.SPELL_EFFECT_APPLY_AURA:
                                            {
                                                var tmp = WorldServiceLocator._WorldServer.CHARACTERs;
                                                var argTarget = tmp[OwnerGUID];
                                                var tmp1 = WorldServiceLocator._WorldServer.CHARACTERs;
                                                var argCaster = tmp1[OwnerGUID];
                                                WorldServiceLocator._WS_Spells.AURAs[spellInfo.SpellEffects[(int)j].ApplyAuraIndex].Invoke(ref argTarget, ref argCaster, ref spellInfo.SpellEffects[(int)j], spellInfo.ID, 1, AuraAction.AURA_REMOVE);
                                                tmp[OwnerGUID] = argTarget;
                                                tmp1[OwnerGUID] = argCaster;
                                                break;
                                            }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void RemoveEnchantment(byte slot)
        {
            if (Enchantments.ContainsKey(slot) == false)
                return;
            // DONE: Remove the bonuses from the character
            RemoveEnchantBonus(slot);
            // DONE: Remove the enchant
            Enchantments.Remove(slot);
            // DONE: Send the update to the client about it
            if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(OwnerGUID))
            {
                var packet = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                packet.AddInt32(1);      // Operations.Count
                packet.AddInt8(0);
                var tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_ITEM);
                tmpUpdate.SetUpdateFlag(EItemFields.ITEM_FIELD_ENCHANTMENT + (int)slot * 3, 0);
                tmpUpdate.SetUpdateFlag(EItemFields.ITEM_FIELD_ENCHANTMENT + (int)slot * 3 + 1, 0);
                tmpUpdate.SetUpdateFlag(EItemFields.ITEM_FIELD_ENCHANTMENT + (int)slot * 3 + 2, 0);
                tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, this);
                WorldServiceLocator._WorldServer.CHARACTERs[OwnerGUID].client.Send(ref packet);
                packet.Dispose();
                tmpUpdate.Dispose();
            }
        }

        public void SoulbindItem([Optional, DefaultParameterValue(null)] ref WS_Network.ClientClass client)
        {
            if ((_flags & ITEM_FLAGS.ITEM_FLAGS_BINDED) == ITEM_FLAGS.ITEM_FLAGS_BINDED)
                return;

            // DONE: Setting the flag
            _flags = _flags | ITEM_FLAGS.ITEM_FLAGS_BINDED;
            Save();

            // DONE: Sending update to character
            if (client is object)
            {
                var packet = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                packet.AddInt32(1);      // Operations.Count
                packet.AddInt8(0);
                var tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_ITEM);
                tmpUpdate.SetUpdateFlag(EItemFields.ITEM_FIELD_FLAGS, _flags);
                tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, this);
                client.Send(ref packet);
                packet.Dispose();
                tmpUpdate.Dispose();
            }
        }

        public bool IsSoulBound
        {
            get
            {
                return (_flags & ITEM_FLAGS.ITEM_FLAGS_BINDED) == ITEM_FLAGS.ITEM_FLAGS_BINDED;
            }
        }
    }
}