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
using System.Threading;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Item;
using Mangos.Common.Enums.Player;
using Mangos.Common.Enums.Spell;
using Mangos.Common.Enums.Unit;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Player;
using Mangos.World.Server;
using Mangos.World.Spells;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Objects
{
    public class WS_Items
    {

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        private readonly int[] ItemWeaponSkills = new int[] { (int)SKILL_IDs.SKILL_AXES, (int)SKILL_IDs.SKILL_TWO_HANDED_AXES, (int)SKILL_IDs.SKILL_BOWS, (int)SKILL_IDs.SKILL_GUNS, (int)SKILL_IDs.SKILL_MACES, (int)SKILL_IDs.SKILL_TWO_HANDED_MACES, (int)SKILL_IDs.SKILL_POLEARMS, (int)SKILL_IDs.SKILL_SWORDS, (int)SKILL_IDs.SKILL_TWO_HANDED_SWORDS, 0, (int)SKILL_IDs.SKILL_STAVES, 0, 0, 0, 0, (int)SKILL_IDs.SKILL_DAGGERS, (int)SKILL_IDs.SKILL_THROWN, (int)SKILL_IDs.SKILL_SPEARS, (int)SKILL_IDs.SKILL_CROSSBOWS, (int)SKILL_IDs.SKILL_WANDS, (int)SKILL_IDs.SKILL_FISHING };
        private readonly int[] ItemArmorSkills = new int[] { 0, (int)SKILL_IDs.SKILL_CLOTH, (int)SKILL_IDs.SKILL_LEATHER, (int)SKILL_IDs.SKILL_MAIL, (int)SKILL_IDs.SKILL_PLATE_MAIL, 0, (int)SKILL_IDs.SKILL_SHIELD, 0, 0, 0 };


        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        // WARNING: Use only with _WorldServer.ITEMDatabase()
        public class ItemInfo : IDisposable
        {
            private readonly bool _found = false;

            private ItemInfo()
            {
                Damage[0] = new TDamage();
                Damage[1] = new TDamage();
                Damage[2] = new TDamage();
                Damage[3] = new TDamage();
                Damage[4] = new TDamage();
                Spells[0] = new TItemSpellInfo();
                Spells[1] = new TItemSpellInfo();
                Spells[2] = new TItemSpellInfo();
                Spells[3] = new TItemSpellInfo();
                Spells[4] = new TItemSpellInfo();
            }

            public ItemInfo(int itemId) : this()
            {
                Id = itemId;
                WorldServiceLocator._WorldServer.ITEMDatabase.Add(Id, this);

                // DONE: Load Item Data from MySQL
                var mySqlQuery = new DataTable();
                WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT * FROM item_template WHERE entry = {0};", (object)itemId), mySqlQuery);
                if (mySqlQuery.Rows.Count == 0)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "ItemID {0} not found in SQL database! Loading default \"Unknown Item\" info.", itemId);
                    _found = false;
                    return;
                }

                _found = true;
                Model = Conversions.ToInteger(mySqlQuery.Rows[0]["displayid"]);
                Name = Conversions.ToString(mySqlQuery.Rows[0]["name"]);
                Quality = Conversions.ToInteger(mySqlQuery.Rows[0]["quality"]);
                // 0=Grey-Poor 1=White-Common 2=Green-Uncommon 3=Blue-Rare 4=Purple-Epic 5=Orange-Legendary 6=Red-Artifact
                Material = Conversions.ToInteger(mySqlQuery.Rows[0]["Material"]);
                // -1=Consumables 1=Metal 2=Wood 3=Liquid 4=Jewelry 5=Chain 6=Plate 7=Cloth 8=Leather
                Durability = Conversions.ToInteger(mySqlQuery.Rows[0]["MaxDurability"]);
                Sheath = (SHEATHE_TYPE)mySqlQuery.Rows[0]["sheath"];
                Bonding = Conversions.ToInteger(mySqlQuery.Rows[0]["bonding"]);
                BuyCount = Conversions.ToInteger(mySqlQuery.Rows[0]["buycount"]);
                BuyPrice = Conversions.ToInteger(mySqlQuery.Rows[0]["buyprice"]);
                SellPrice = Conversions.ToInteger(mySqlQuery.Rows[0]["sellprice"]);

                // Item's Characteristics
                Id = Conversions.ToInteger(mySqlQuery.Rows[0]["entry"]);
                Flags = Conversions.ToInteger(mySqlQuery.Rows[0]["flags"]);
                ObjectClass = (ITEM_CLASS)mySqlQuery.Rows[0]["class"];
                SubClass = (ITEM_SUBCLASS)mySqlQuery.Rows[0]["subclass"];
                InventoryType = (INVENTORY_TYPES)mySqlQuery.Rows[0]["inventorytype"];
                Level = Conversions.ToInteger(mySqlQuery.Rows[0]["itemlevel"]);
                AvailableClasses = BitConverter.ToUInt32(BitConverter.GetBytes(mySqlQuery.Rows[0]["allowableclass"]), 0);
                AvailableRaces = BitConverter.ToUInt32(BitConverter.GetBytes(mySqlQuery.Rows[0]["allowablerace"]), 0);
                ReqLevel = Conversions.ToInteger(mySqlQuery.Rows[0]["requiredlevel"]);
                ReqSkill = Conversions.ToInteger(mySqlQuery.Rows[0]["RequiredSkill"]);
                ReqSkillRank = Conversions.ToInteger(mySqlQuery.Rows[0]["RequiredSkillRank"]);
                // ReqSkillSubRank = MySQLQuery.Rows(0).Item("RequiredSkillSubRank")
                ReqSpell = Conversions.ToInteger(mySqlQuery.Rows[0]["requiredspell"]);
                ReqFaction = Conversions.ToInteger(mySqlQuery.Rows[0]["RequiredReputationFaction"]);
                ReqFactionLevel = Conversions.ToInteger(mySqlQuery.Rows[0]["RequiredReputationRank"]);
                ReqHonorRank = Conversions.ToInteger(mySqlQuery.Rows[0]["requiredhonorrank"]);
                ReqHonorRank2 = Conversions.ToInteger(mySqlQuery.Rows[0]["RequiredCityRank"]);

                // Special items
                AmmoType = Conversions.ToInteger(mySqlQuery.Rows[0]["ammo_type"]);
                PageText = Conversions.ToInteger(mySqlQuery.Rows[0]["PageText"]);
                Stackable = Conversions.ToInteger(mySqlQuery.Rows[0]["stackable"]);
                Unique = Conversions.ToInteger(mySqlQuery.Rows[0]["maxcount"]);
                Description = Conversions.ToString(mySqlQuery.Rows[0]["description"]);
                Block = Conversions.ToInteger(mySqlQuery.Rows[0]["block"]);
                ItemSet = Conversions.ToInteger(mySqlQuery.Rows[0]["itemset"]);
                PageMaterial = Conversions.ToInteger(mySqlQuery.Rows[0]["PageMaterial"]);
                // The background of the page window: 1=Parchment 2=Stone 3=Marble 4=Silver 5=Bronze
                StartQuest = Conversions.ToInteger(mySqlQuery.Rows[0]["startquest"]);
                ContainerSlots = Conversions.ToInteger(mySqlQuery.Rows[0]["ContainerSlots"]);
                LanguageID = Conversions.ToInteger(mySqlQuery.Rows[0]["LanguageID"]);
                BagFamily = (ITEM_BAG)mySqlQuery.Rows[0]["BagFamily"];
                Delay = Conversions.ToInteger(mySqlQuery.Rows[0]["delay"]);
                Range = Conversions.ToSingle(mySqlQuery.Rows[0]["RangedModRange"]);
                Damage[0].Minimum = Conversions.ToSingle(mySqlQuery.Rows[0]["dmg_min1"]);
                Damage[0].Maximum = Conversions.ToSingle(mySqlQuery.Rows[0]["dmg_max1"]);
                Damage[0].Type = Conversions.ToInteger(mySqlQuery.Rows[0]["dmg_type1"]);
                Damage[1].Minimum = Conversions.ToSingle(mySqlQuery.Rows[0]["dmg_min2"]);
                Damage[1].Maximum = Conversions.ToSingle(mySqlQuery.Rows[0]["dmg_max2"]);
                Damage[1].Type = Conversions.ToInteger(mySqlQuery.Rows[0]["dmg_type2"]);
                Damage[2].Minimum = Conversions.ToSingle(mySqlQuery.Rows[0]["dmg_min3"]);
                Damage[2].Maximum = Conversions.ToSingle(mySqlQuery.Rows[0]["dmg_max3"]);
                Damage[2].Type = Conversions.ToInteger(mySqlQuery.Rows[0]["dmg_type3"]);
                Damage[3].Minimum = Conversions.ToSingle(mySqlQuery.Rows[0]["dmg_min4"]);
                Damage[3].Maximum = Conversions.ToSingle(mySqlQuery.Rows[0]["dmg_max4"]);
                Damage[3].Type = Conversions.ToInteger(mySqlQuery.Rows[0]["dmg_type4"]);
                Damage[4].Minimum = Conversions.ToSingle(mySqlQuery.Rows[0]["dmg_min5"]);
                Damage[4].Maximum = Conversions.ToSingle(mySqlQuery.Rows[0]["dmg_max5"]);
                Damage[4].Type = Conversions.ToInteger(mySqlQuery.Rows[0]["dmg_type5"]);
                (object)Resistances[DamageTypes.DMG_PHYSICAL] = mySqlQuery.Rows[0]["armor"];        // Armor
                (object)Resistances[DamageTypes.DMG_HOLY] = mySqlQuery.Rows[0]["holy_res"];          // Holy
                (object)Resistances[DamageTypes.DMG_FIRE] = mySqlQuery.Rows[0]["fire_res"];          // Fire
                (object)Resistances[DamageTypes.DMG_NATURE] = mySqlQuery.Rows[0]["nature_res"];      // Nature
                (object)Resistances[DamageTypes.DMG_FROST] = mySqlQuery.Rows[0]["frost_res"];        // Frost
                (object)Resistances[DamageTypes.DMG_SHADOW] = mySqlQuery.Rows[0]["shadow_res"];      // Shadow
                (object)Resistances[DamageTypes.DMG_ARCANE] = mySqlQuery.Rows[0]["arcane_res"];      // Arcane
                Spells[0].SpellID = Conversions.ToInteger(mySqlQuery.Rows[0]["spellid_1"]);
                Spells[0].SpellTrigger = (ITEM_SPELLTRIGGER_TYPE)mySqlQuery.Rows[0]["spelltrigger_1"];
                // 0="Use:" 1="Equip:" 2="Chance on Hit:"
                Spells[0].SpellCharges = Conversions.ToInteger(mySqlQuery.Rows[0]["spellcharges_1"]);
                // 0=Doesn't disappear after use -1=Disappears after use
                Spells[0].SpellCooldown = Conversions.ToInteger(mySqlQuery.Rows[0]["spellcooldown_1"]);
                Spells[0].SpellCategory = Conversions.ToInteger(mySqlQuery.Rows[0]["spellcategory_1"]);
                Spells[0].SpellCategoryCooldown = Conversions.ToInteger(mySqlQuery.Rows[0]["spellcategorycooldown_1"]);
                Spells[1].SpellID = Conversions.ToInteger(mySqlQuery.Rows[0]["spellid_2"]);
                Spells[1].SpellTrigger = (ITEM_SPELLTRIGGER_TYPE)mySqlQuery.Rows[0]["spelltrigger_2"];
                // 0="Use:" 1="Equip:" 2="Chance on Hit:"
                Spells[1].SpellCharges = Conversions.ToInteger(mySqlQuery.Rows[0]["spellcharges_2"]);
                // 0=Doesn't disappear after use -1=Disappears after use
                Spells[1].SpellCooldown = Conversions.ToInteger(mySqlQuery.Rows[0]["spellcooldown_2"]);
                Spells[1].SpellCategory = Conversions.ToInteger(mySqlQuery.Rows[0]["spellcategory_2"]);
                Spells[1].SpellCategoryCooldown = Conversions.ToInteger(mySqlQuery.Rows[0]["spellcategorycooldown_2"]);
                Spells[2].SpellID = Conversions.ToInteger(mySqlQuery.Rows[0]["spellid_3"]);
                Spells[2].SpellTrigger = (ITEM_SPELLTRIGGER_TYPE)mySqlQuery.Rows[0]["spelltrigger_3"];
                // 0="Use:" 1="Equip:" 2="Chance on Hit:"
                Spells[2].SpellCharges = Conversions.ToInteger(mySqlQuery.Rows[0]["spellcharges_3"]);
                // 0=Doesn't disappear after use -1=Disappears after use
                Spells[2].SpellCooldown = Conversions.ToInteger(mySqlQuery.Rows[0]["spellcooldown_3"]);
                Spells[2].SpellCategory = Conversions.ToInteger(mySqlQuery.Rows[0]["spellcategory_3"]);
                Spells[2].SpellCategoryCooldown = Conversions.ToInteger(mySqlQuery.Rows[0]["spellcategorycooldown_3"]);
                Spells[3].SpellID = Conversions.ToInteger(mySqlQuery.Rows[0]["spellid_4"]);
                Spells[3].SpellTrigger = (ITEM_SPELLTRIGGER_TYPE)mySqlQuery.Rows[0]["spelltrigger_4"];
                // 0="Use:" 1="Equip:" 2="Chance on Hit:"
                Spells[3].SpellCharges = Conversions.ToInteger(mySqlQuery.Rows[0]["spellcharges_4"]);
                // 0=Doesn't disappear after use -1=Disappears after use
                Spells[3].SpellCooldown = Conversions.ToInteger(mySqlQuery.Rows[0]["spellcooldown_4"]);
                Spells[3].SpellCategory = Conversions.ToInteger(mySqlQuery.Rows[0]["spellcategory_4"]);
                Spells[3].SpellCategoryCooldown = Conversions.ToInteger(mySqlQuery.Rows[0]["spellcategorycooldown_4"]);
                Spells[4].SpellID = Conversions.ToInteger(mySqlQuery.Rows[0]["spellid_5"]);
                Spells[4].SpellTrigger = (ITEM_SPELLTRIGGER_TYPE)mySqlQuery.Rows[0]["spelltrigger_5"];
                // 0="Use:" 1="Equip:" 2="Chance on Hit:"
                Spells[4].SpellCharges = Conversions.ToInteger(mySqlQuery.Rows[0]["spellcharges_5"]);
                // 0=Doesn't disappear after use -1=Disappears after use
                Spells[4].SpellCooldown = Conversions.ToInteger(mySqlQuery.Rows[0]["spellcooldown_5"]);
                Spells[4].SpellCategory = Conversions.ToInteger(mySqlQuery.Rows[0]["spellcategory_5"]);
                Spells[4].SpellCategoryCooldown = Conversions.ToInteger(mySqlQuery.Rows[0]["spellcategorycooldown_5"]);

                // Unknown
                LockID = Conversions.ToInteger(mySqlQuery.Rows[0]["lockid"]);
                // Extra = MySQLQuery.Rows(0).Item("Extra")

                ItemBonusStatType[0] = Conversions.ToInteger(mySqlQuery.Rows[0]["stat_type1"]);
                ItemBonusStatValue[0] = Conversions.ToInteger(mySqlQuery.Rows[0]["stat_value1"]);
                ItemBonusStatType[1] = Conversions.ToInteger(mySqlQuery.Rows[0]["stat_type2"]);
                ItemBonusStatValue[1] = Conversions.ToInteger(mySqlQuery.Rows[0]["stat_value2"]);
                ItemBonusStatType[2] = Conversions.ToInteger(mySqlQuery.Rows[0]["stat_type3"]);
                ItemBonusStatValue[2] = Conversions.ToInteger(mySqlQuery.Rows[0]["stat_value3"]);
                ItemBonusStatType[3] = Conversions.ToInteger(mySqlQuery.Rows[0]["stat_type4"]);
                ItemBonusStatValue[3] = Conversions.ToInteger(mySqlQuery.Rows[0]["stat_value4"]);
                ItemBonusStatType[4] = Conversions.ToInteger(mySqlQuery.Rows[0]["stat_type5"]);
                ItemBonusStatValue[4] = Conversions.ToInteger(mySqlQuery.Rows[0]["stat_value5"]);
                ItemBonusStatType[5] = Conversions.ToInteger(mySqlQuery.Rows[0]["stat_type6"]);
                ItemBonusStatValue[5] = Conversions.ToInteger(mySqlQuery.Rows[0]["stat_value6"]);
                ItemBonusStatType[6] = Conversions.ToInteger(mySqlQuery.Rows[0]["stat_type7"]);
                ItemBonusStatValue[6] = Conversions.ToInteger(mySqlQuery.Rows[0]["stat_value7"]);
                ItemBonusStatType[7] = Conversions.ToInteger(mySqlQuery.Rows[0]["stat_type8"]);
                ItemBonusStatValue[7] = Conversions.ToInteger(mySqlQuery.Rows[0]["stat_value8"]);
                ItemBonusStatType[8] = Conversions.ToInteger(mySqlQuery.Rows[0]["stat_type9"]);
                ItemBonusStatValue[8] = Conversions.ToInteger(mySqlQuery.Rows[0]["stat_value9"]);
                ItemBonusStatType[9] = Conversions.ToInteger(mySqlQuery.Rows[0]["stat_type10"]);
                ItemBonusStatValue[9] = Conversions.ToInteger(mySqlQuery.Rows[0]["stat_value10"]);

                // RandomProp = MySQLQuery.Rows(0).Item("randomprop")
                // RandomSuffix = MySQLQuery.Rows(0).Item("randomsuffix") ' Not sure about this one
                ZoneNameID = Conversions.ToInteger(mySqlQuery.Rows[0]["area"]);
                // MapID = MySQLQuery.Rows(0).Item("mapid")
                // TotemCategory = MySQLQuery.Rows(0).Item("TotemCategory")
                _reqDisenchantSkill = Conversions.ToInteger(mySqlQuery.Rows[0]["DisenchantID"]);
                // ArmorDamageModifier = MySQLQuery.Rows(0).Item("armorDamageModifier")
                // ExistingDuration = MySQLQuery.Rows(0).Item("ExistingDuration")

                // DONE: Internal database fixers
                if (Stackable == 0)
                    Stackable = 1;
            }

            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            private bool _disposedValue; // To detect redundant calls

            // IDisposable
            protected virtual void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                    // TODO: set large fields to null.
                    WorldServiceLocator._WorldServer.ITEMDatabase.Remove(Id);
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
            // Item's visuals
            public readonly int Model = 0;
            public readonly string Name = "Unknown Item";
            public readonly int Quality = 0;
            public readonly int Material = 0;
            public readonly int Durability = 0;
            public readonly SHEATHE_TYPE Sheath = 0;
            public readonly int Bonding = 0;
            public readonly int BuyCount = 0;
            public readonly int BuyPrice = 0;
            public readonly int SellPrice = 0;

            // Item's Characteristics
            public readonly int Id = 0;
            public readonly int Flags = 0;
            public readonly ITEM_CLASS ObjectClass = 0;
            public readonly ITEM_SUBCLASS SubClass = 0;
            public readonly INVENTORY_TYPES InventoryType = 0;
            public readonly int Level = 0;
            public readonly uint AvailableClasses = 0U;
            public readonly uint AvailableRaces = 0U;
            public readonly int ReqLevel = 0;
            public readonly int ReqSkill = 0;
            public readonly int ReqSkillRank = 0;
            public int ReqSkillSubRank;
            public readonly int ReqSpell = 0;
            public readonly int ReqFaction = 0;
            public readonly int ReqFactionLevel = 0;
            public readonly int ReqHonorRank = 0;
            public readonly int ReqHonorRank2 = 0;

            // Special items
            public readonly int AmmoType = 0;
            public readonly int PageText = 0;
            public readonly int Stackable = 1;
            public readonly int Unique = 0;
            public readonly string Description = "";
            public readonly int Block = 0;
            public readonly int ItemSet = 0;
            public readonly int PageMaterial = 0;
            public readonly int StartQuest = 0;
            public readonly int ContainerSlots = 0;
            public readonly int LanguageID = 0;
            public readonly ITEM_BAG BagFamily = 0;

            // Item's bonuses
            public readonly int Delay = 0;
            public readonly float Range = 0f;
            public readonly TDamage[] Damage = new TDamage[5];
            public readonly int[] Resistances = new int[] { 0, 0, 0, 0, 0, 0, 0 };
            public readonly int[] ItemBonusStatType = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            public readonly int[] ItemBonusStatValue = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            // Item's Spells
            public readonly TItemSpellInfo[] Spells = new TItemSpellInfo[5];
            private readonly int _reqDisenchantSkill = -1;
            public float ArmorDamageModifier = 0f;
            public int ExistingDuration = 0;

            // Other
            public int Unk2 = 0;
            public readonly int LockID = 0;
            public int Extra = 0;
            public int Area = 0;
            public readonly int ZoneNameID = 0;
            public int MapID = 0;
            public int TotemCategory = 0;
            public int RandomProp = 0;
            public int RandomSuffix = 0;

            public bool IsContainer
            {
                get
                {
                    if (ContainerSlots > 0)
                        return true;
                    else
                        return false;
                }
            }

            public byte[] GetSlots
            {
                get
                {
                    switch (InventoryType)
                    {
                        case var @case when @case == INVENTORY_TYPES.INVTYPE_HEAD:
                            {
                                return new byte[] { (byte)EquipmentSlots.EQUIPMENT_SLOT_HEAD };
                            }

                        case var case1 when case1 == INVENTORY_TYPES.INVTYPE_NECK:
                            {
                                return new byte[] { (byte)EquipmentSlots.EQUIPMENT_SLOT_NECK };
                            }

                        case var case2 when case2 == INVENTORY_TYPES.INVTYPE_SHOULDERS:
                            {
                                return new byte[] { (byte)EquipmentSlots.EQUIPMENT_SLOT_SHOULDERS };
                            }

                        case var case3 when case3 == INVENTORY_TYPES.INVTYPE_BODY:
                            {
                                return new byte[] { (byte)EquipmentSlots.EQUIPMENT_SLOT_BODY };
                            }

                        case var case4 when case4 == INVENTORY_TYPES.INVTYPE_CHEST:
                            {
                                return new byte[] { (byte)EquipmentSlots.EQUIPMENT_SLOT_CHEST };
                            }

                        case var case5 when case5 == INVENTORY_TYPES.INVTYPE_ROBE:
                            {
                                return new byte[] { (byte)EquipmentSlots.EQUIPMENT_SLOT_CHEST };
                            }

                        case var case6 when case6 == INVENTORY_TYPES.INVTYPE_WAIST:
                            {
                                return new byte[] { (byte)EquipmentSlots.EQUIPMENT_SLOT_WAIST };
                            }

                        case var case7 when case7 == INVENTORY_TYPES.INVTYPE_LEGS:
                            {
                                return new byte[] { (byte)EquipmentSlots.EQUIPMENT_SLOT_LEGS };
                            }

                        case var case8 when case8 == INVENTORY_TYPES.INVTYPE_FEET:
                            {
                                return new byte[] { (byte)EquipmentSlots.EQUIPMENT_SLOT_FEET };
                            }

                        case var case9 when case9 == INVENTORY_TYPES.INVTYPE_WRISTS:
                            {
                                return new byte[] { (byte)EquipmentSlots.EQUIPMENT_SLOT_WRISTS };
                            }

                        case var case10 when case10 == INVENTORY_TYPES.INVTYPE_HANDS:
                            {
                                return new byte[] { (byte)EquipmentSlots.EQUIPMENT_SLOT_HANDS };
                            }

                        case var case11 when case11 == INVENTORY_TYPES.INVTYPE_FINGER:
                            {
                                return new byte[] { (byte)EquipmentSlots.EQUIPMENT_SLOT_FINGER1, (byte)EquipmentSlots.EQUIPMENT_SLOT_FINGER2 };
                            }

                        case var case12 when case12 == INVENTORY_TYPES.INVTYPE_TRINKET:
                            {
                                return new byte[] { (byte)EquipmentSlots.EQUIPMENT_SLOT_TRINKET1, (byte)EquipmentSlots.EQUIPMENT_SLOT_TRINKET2 };
                            }

                        case var case13 when case13 == INVENTORY_TYPES.INVTYPE_CLOAK:
                            {
                                return new byte[] { (byte)EquipmentSlots.EQUIPMENT_SLOT_BACK };
                            }

                        case var case14 when case14 == INVENTORY_TYPES.INVTYPE_WEAPON:
                            {
                                return new byte[] { (byte)EquipmentSlots.EQUIPMENT_SLOT_MAINHAND, (byte)EquipmentSlots.EQUIPMENT_SLOT_OFFHAND };
                            }

                        case var case15 when case15 == INVENTORY_TYPES.INVTYPE_SHIELD:
                            {
                                return new byte[] { (byte)EquipmentSlots.EQUIPMENT_SLOT_OFFHAND };
                            }

                        case var case16 when case16 == INVENTORY_TYPES.INVTYPE_RANGED:
                            {
                                return new byte[] { (byte)EquipmentSlots.EQUIPMENT_SLOT_RANGED };
                            }

                        case var case17 when case17 == INVENTORY_TYPES.INVTYPE_TWOHAND_WEAPON:
                            {
                                return new byte[] { (byte)EquipmentSlots.EQUIPMENT_SLOT_MAINHAND };
                            }

                        case var case18 when case18 == INVENTORY_TYPES.INVTYPE_TABARD:
                            {
                                return new byte[] { (byte)EquipmentSlots.EQUIPMENT_SLOT_TABARD };
                            }

                        case var case19 when case19 == INVENTORY_TYPES.INVTYPE_WEAPONMAINHAND:
                            {
                                return new byte[] { (byte)EquipmentSlots.EQUIPMENT_SLOT_MAINHAND };
                            }

                        case var case20 when case20 == INVENTORY_TYPES.INVTYPE_WEAPONOFFHAND:
                            {
                                return new byte[] { (byte)EquipmentSlots.EQUIPMENT_SLOT_OFFHAND };
                            }

                        case var case21 when case21 == INVENTORY_TYPES.INVTYPE_HOLDABLE:
                            {
                                return new byte[] { (byte)EquipmentSlots.EQUIPMENT_SLOT_OFFHAND };
                            }

                        case var case22 when case22 == INVENTORY_TYPES.INVTYPE_THROWN:
                            {
                                return new byte[] { (byte)EquipmentSlots.EQUIPMENT_SLOT_RANGED };
                            }

                        case var case23 when case23 == INVENTORY_TYPES.INVTYPE_RANGEDRIGHT:
                            {
                                return new byte[] { (byte)EquipmentSlots.EQUIPMENT_SLOT_RANGED };
                            }

                        case var case24 when case24 == INVENTORY_TYPES.INVTYPE_BAG:
                            {
                                return new byte[] { (byte)InventorySlots.INVENTORY_SLOT_BAG_1, (byte)InventorySlots.INVENTORY_SLOT_BAG_2, (byte)InventorySlots.INVENTORY_SLOT_BAG_3, (byte)InventorySlots.INVENTORY_SLOT_BAG_4 };

                            }

                        case var case25 when case25 == INVENTORY_TYPES.INVTYPE_RELIC:
                            {
                                return new byte[] { (byte)EquipmentSlots.EQUIPMENT_SLOT_RANGED };
                            }

                        default:
                            {
                                return new byte[] { };
                            }
                    }
                }
            }

            public int GetReqSkill
            {
                get
                {
                    if (ObjectClass == ITEM_CLASS.ITEM_CLASS_WEAPON)
                        return WorldServiceLocator._WS_Items.ItemWeaponSkills[SubClass];
                    if (ObjectClass == ITEM_CLASS.ITEM_CLASS_ARMOR)
                        return WorldServiceLocator._WS_Items.ItemArmorSkills[SubClass];
                    return 0;
                }
            }

            public short GetReqSpell
            {
                get
                {
                    switch (ObjectClass)
                    {
                        case var @case when @case == ITEM_CLASS.ITEM_CLASS_WEAPON:
                            {
                                switch (SubClass)
                                {
                                    case var case1 when case1 == ITEM_SUBCLASS.ITEM_SUBCLASS_MISC_WEAPON:
                                        {
                                            return 0;
                                        }

                                    case var case2 when case2 == ITEM_SUBCLASS.ITEM_SUBCLASS_AXE:
                                        {
                                            return 196;
                                        }

                                    case var case3 when case3 == ITEM_SUBCLASS.ITEM_SUBCLASS_TWOHAND_AXE:
                                        {
                                            return 197;
                                        }

                                    case var case4 when case4 == ITEM_SUBCLASS.ITEM_SUBCLASS_BOW:
                                        {
                                            return 264;
                                        }

                                    case var case5 when case5 == ITEM_SUBCLASS.ITEM_SUBCLASS_GUN:
                                        {
                                            return 266;
                                        }

                                    case var case6 when case6 == ITEM_SUBCLASS.ITEM_SUBCLASS_MACE:
                                        {
                                            return 198;
                                        }

                                    case var case7 when case7 == ITEM_SUBCLASS.ITEM_SUBCLASS_TWOHAND_MACE:
                                        {
                                            return 199;
                                        }

                                    case var case8 when case8 == ITEM_SUBCLASS.ITEM_SUBCLASS_POLEARM:
                                        {
                                            return 200;
                                        }

                                    case var case9 when case9 == ITEM_SUBCLASS.ITEM_SUBCLASS_SWORD:
                                        {
                                            return 201;
                                        }

                                    case var case10 when case10 == ITEM_SUBCLASS.ITEM_SUBCLASS_TWOHAND_SWORD:
                                        {
                                            return 202;
                                        }

                                    case var case11 when case11 == ITEM_SUBCLASS.ITEM_SUBCLASS_STAFF:
                                        {
                                            return 227;
                                        }

                                    case var case12 when case12 == ITEM_SUBCLASS.ITEM_SUBCLASS_WEAPON_EXOTIC:
                                        {
                                            return 262;
                                        }

                                    case var case13 when case13 == ITEM_SUBCLASS.ITEM_SUBCLASS_WEAPON_EXOTIC2:
                                        {
                                            return 263;
                                        }

                                    case var case14 when case14 == ITEM_SUBCLASS.ITEM_SUBCLASS_FIST_WEAPON:
                                        {
                                            return 15590;
                                        }

                                    case var case15 when case15 == ITEM_SUBCLASS.ITEM_SUBCLASS_DAGGER:
                                        {
                                            return 1180;
                                        }

                                    case var case16 when case16 == ITEM_SUBCLASS.ITEM_SUBCLASS_THROWN:
                                        {
                                            return 2567;
                                        }

                                    case var case17 when case17 == ITEM_SUBCLASS.ITEM_SUBCLASS_SPEAR:
                                        {
                                            return 3386;
                                        }

                                    case var case18 when case18 == ITEM_SUBCLASS.ITEM_SUBCLASS_CROSSBOW:
                                        {
                                            return 5011;
                                        }

                                    case var case19 when case19 == ITEM_SUBCLASS.ITEM_SUBCLASS_WAND:
                                        {
                                            return 5009;
                                        }

                                    case var case20 when case20 == ITEM_SUBCLASS.ITEM_SUBCLASS_FISHING_POLE:
                                        {
                                            return 7738;
                                        }
                                }

                                break;
                            }

                        case var case21 when case21 == ITEM_CLASS.ITEM_CLASS_ARMOR:
                            {
                                switch (SubClass)
                                {
                                    case var case22 when case22 == ITEM_SUBCLASS.ITEM_SUBCLASS_MISC:
                                        {
                                            return 0;
                                        }

                                    case var case23 when case23 == ITEM_SUBCLASS.ITEM_SUBCLASS_CLOTH:
                                        {
                                            return 9078;
                                        }

                                    case var case24 when case24 == ITEM_SUBCLASS.ITEM_SUBCLASS_LEATHER:
                                        {
                                            return 9077;
                                        }

                                    case var case25 when case25 == ITEM_SUBCLASS.ITEM_SUBCLASS_MAIL:
                                        {
                                            return 8737;
                                        }

                                    case var case26 when case26 == ITEM_SUBCLASS.ITEM_SUBCLASS_PLATE:
                                        {
                                            return 750;
                                        }

                                    case var case27 when case27 == ITEM_SUBCLASS.ITEM_SUBCLASS_SHIELD:
                                        {
                                            return 9116;
                                        }

                                    case var case28 when case28 == ITEM_SUBCLASS.ITEM_SUBCLASS_BUCKLER:
                                        {
                                            return 9124;
                                        }

                                    case var case29 when case29 == ITEM_SUBCLASS.ITEM_SUBCLASS_LIBRAM:
                                        {
                                            return 27762;
                                        }

                                    case var case30 when case30 == ITEM_SUBCLASS.ITEM_SUBCLASS_TOTEM:
                                        {
                                            return 27763;
                                        }

                                    case var case31 when case31 == ITEM_SUBCLASS.ITEM_SUBCLASS_IDOL:
                                        {
                                            return 27764;
                                        }
                                }

                                break;
                            }

                        default:
                            {
                                return 0;
                            }
                    }

                    return 0;
                }
            }
        }

        public class TDamage
        {
            public float Minimum = 0f;
            public float Maximum = 0f;
            public int Type = (int)DamageTypes.DMG_PHYSICAL;
        }

        public class TEnchantmentInfo
        {
            public readonly int ID = 0;
            public readonly int Duration = 0;
            public readonly int Charges = 0;

            public TEnchantmentInfo(int ID_, int Duration_ = 0, int Charges_ = 0)
            {
                ID = ID_;
                Duration = Duration_;
                Charges = Charges_;
            }
        }

        public class TItemSpellInfo
        {
            public int SpellID = 0;
            public ITEM_SPELLTRIGGER_TYPE SpellTrigger = 0;
            public int SpellCharges = -1;
            public int SpellCooldown = 0;
            public int SpellCategory = 0;
            public int SpellCategoryCooldown = 0;
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public ItemObject LoadItemByGUID(ulong guid, WS_PlayerData.CharacterObject owner = null, bool equipped = false)
        {
            if (WorldServiceLocator._WorldServer.WORLD_ITEMs.ContainsKey(guid + WorldServiceLocator._Global_Constants.GUID_ITEM))
            {
                return WorldServiceLocator._WorldServer.WORLD_ITEMs(guid + WorldServiceLocator._Global_Constants.GUID_ITEM);
            }

            var tmpItem = new ItemObject(guid, owner, equipped);
            return tmpItem;
        }

        public void SendItemInfo(ref WS_Network.ClientClass client, int itemID)
        {
            var response = new Packets.PacketClass(OPCODES.SMSG_ITEM_QUERY_SINGLE_RESPONSE);
            ItemInfo item;
            if (WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(itemID) == false)
            {
                item = new ItemInfo(itemID);
            }
            else
            {
                item = WorldServiceLocator._WorldServer.ITEMDatabase[itemID];
            }

            response.AddInt32(item.Id);
            response.AddInt32((int)item.ObjectClass);
            if (item.ObjectClass == ITEM_CLASS.ITEM_CLASS_CONSUMABLE)
            {
                response.AddInt32(0);
            }
            else
            {
                response.AddInt32((int)item.SubClass);
            }

            response.AddString(item.Name);
            response.AddInt8(0);     // Item.Name2
            response.AddInt8(0);     // Item.Name3
            response.AddInt8(0);     // Item.Name4
            response.AddInt32(item.Model);
            response.AddInt32(item.Quality);
            response.AddInt32(item.Flags);
            response.AddInt32(item.BuyPrice);
            response.AddInt32(item.SellPrice);
            response.AddInt32((int)item.InventoryType);
            response.AddUInt32(item.AvailableClasses);
            response.AddUInt32(item.AvailableRaces);
            response.AddInt32(item.Level);
            response.AddInt32(item.ReqLevel);
            response.AddInt32(item.ReqSkill);
            response.AddInt32(item.ReqSkillRank);
            response.AddInt32(item.ReqSpell);
            response.AddInt32(item.ReqHonorRank);
            response.AddInt32(item.ReqHonorRank2);
            // RequiredCityRank           [1 - Protector of Stormwind, 2 - Overlord of Orgrimmar, 3 - Thane of Ironforge, 4 - High Sentinel of Darnassus, 5 - Deathlord of the Undercity, 6 - Chieftan of Thunderbluff, 7 - Avenger of Gnomeregan, 8 - Voodoo Boss of Senjin]
            response.AddInt32(item.ReqFaction);          // RequiredReputationFaction
            response.AddInt32(item.ReqFactionLevel);     // RequiredRaputationRank
            response.AddInt32(item.Unique); // Was stackable
            response.AddInt32(item.Stackable);
            response.AddInt32(item.ContainerSlots);
            for (int i = 0; i <= 9; i++)
            {
                response.AddInt32(item.ItemBonusStatType[i]);
                response.AddInt32(item.ItemBonusStatValue[i]);
            }

            for (int i = 0; i <= 4; i++)
            {
                response.AddSingle(item.Damage[i].Minimum);
                response.AddSingle(item.Damage[i].Maximum);
                response.AddInt32(item.Damage[i].Type);
            }

            for (int i = 0; i <= 6; i++)
                response.AddInt32(item.Resistances[i]);
            response.AddInt32(item.Delay);
            response.AddInt32(item.AmmoType);
            response.AddSingle(item.Range);          // itemRangeModifier (Ranged Weapons = 100.0, Fishing Poles = 3.0)
            for (int i = 0; i <= 4; i++)
            {
                if (WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(item.Spells[i].SpellID) == false)
                {
                    response.AddInt32(0);
                    response.AddInt32(0);
                    response.AddInt32(0);
                    response.AddInt32(-1);
                    response.AddInt32(0);
                    response.AddInt32(-1);
                }
                else
                {
                    response.AddInt32(item.Spells[i].SpellID);
                    response.AddInt32((int)item.Spells[i].SpellTrigger);
                    response.AddInt32(item.Spells[i].SpellCharges);
                    if (item.Spells[i].SpellCooldown > 0 || item.Spells[i].SpellCategoryCooldown > 0)
                    {
                        response.AddInt32(item.Spells[i].SpellCooldown);
                        response.AddInt32(item.Spells[i].SpellCategory);
                        response.AddInt32(item.Spells[i].SpellCategoryCooldown);
                    }
                    else
                    {
                        response.AddInt32(WorldServiceLocator._WS_Spells.SPELLs[item.Spells[i].SpellID].SpellCooldown);
                        response.AddInt32(WorldServiceLocator._WS_Spells.SPELLs[item.Spells[i].SpellID].Category);
                        response.AddInt32(WorldServiceLocator._WS_Spells.SPELLs[item.Spells[i].SpellID].CategoryCooldown);
                    }
                }
            }

            response.AddInt32(item.Bonding);
            response.AddString(item.Description);
            response.AddInt32(item.PageText);
            response.AddInt32(item.LanguageID);
            response.AddInt32(item.PageMaterial);
            response.AddInt32(item.StartQuest);
            response.AddInt32(item.LockID);
            response.AddInt32(item.Material);
            response.AddInt32((int)item.Sheath);
            response.AddInt32(item.Extra);
            response.AddInt32(item.Block);
            response.AddInt32(item.ItemSet);
            response.AddInt32(item.Durability);
            response.AddInt32(item.ZoneNameID);
            response.AddInt32(item.MapID); // Added in 1.12.1 client branch
            response.AddInt32((int)item.BagFamily);

            // response.AddInt32(Item.TotemCategory)
            // response.AddInt32(Item.ReqDisenchantSkill)
            // response.AddInt32(Item.ArmorDamageModifier)
            // response.AddInt32(Item.ExistingDuration)

            client.Send(response);
            response.Dispose();
        }

        public void On_CMSG_ITEM_QUERY_SINGLE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 9)
                return;
            packet.GetInt16();
            int itemID = packet.GetInt32();
            SendItemInfo(ref client, itemID);
        }

        public void On_CMSG_ITEM_NAME_QUERY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 9)
                return;
            packet.GetInt16();
            int itemID = packet.GetInt32();
            ItemInfo item;
            if (WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(itemID) == false)
            {
                item = new ItemInfo(itemID);
            }
            else
            {
                item = WorldServiceLocator._WorldServer.ITEMDatabase[itemID];
            }

            var response = new Packets.PacketClass(OPCODES.SMSG_ITEM_NAME_QUERY_RESPONSE);
            response.AddInt32(itemID);
            response.AddString(item.Name);
            response.AddInt32((int)item.InventoryType);
            client.Send(response);
            response.Dispose();
        }

        public void On_CMSG_SWAP_INV_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 7)
                return;
            packet.GetInt16();
            byte srcSlot = packet.GetInt8();
            byte dstSlot = packet.GetInt8();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SWAP_INV_ITEM [srcSlot=0:{2}, dstSlot=0:{3}]", client.IP, client.Port, srcSlot, dstSlot);
            client.Character.ItemSWAP(0, srcSlot, 0, dstSlot);
        }

        public void On_CMSG_AUTOEQUIP_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 7)
                return;
            try
            {
                packet.GetInt16();
                byte srcBag = packet.GetInt8();
                byte srcSlot = packet.GetInt8();
                if (srcBag == 255)
                    srcBag = 0;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AUTOEQUIP_ITEM [srcSlot={3}:{2}]", client.IP, client.Port, srcSlot, srcBag);
                byte errCode = (byte)InventoryChangeFailure.EQUIP_ERR_ITEM_CANT_BE_EQUIPPED;

                // DONE: Check owner of the item
                if (client.Character.ItemGET(srcBag, srcSlot).OwnerGUID != client.Character.GUID)
                {
                    errCode = (byte)InventoryChangeFailure.EQUIP_ERR_DONT_OWN_THAT_ITEM;
                }
                else if (srcBag == 0 && client.Character.Items.ContainsKey(srcSlot))
                {
                    var slots = client.Character.Items[srcSlot].ItemInfo.GetSlots;
                    foreach (byte tmpSlot in slots)
                    {
                        if (!client.Character.Items.ContainsKey(tmpSlot))
                        {
                            client.Character.ItemSWAP(srcBag, srcSlot, 0, tmpSlot);
                            errCode = (byte)InventoryChangeFailure.EQUIP_ERR_OK;
                            break;
                        }
                        else
                        {
                            errCode = (byte)InventoryChangeFailure.EQUIP_ERR_NO_EQUIPMENT_SLOT_AVAILABLE;
                        }
                    }

                    if (errCode == InventoryChangeFailure.EQUIP_ERR_NO_EQUIPMENT_SLOT_AVAILABLE)
                    {
                        foreach (byte tmpSlot in slots)
                        {
                            client.Character.ItemSWAP(srcBag, srcSlot, 0, tmpSlot);
                            errCode = (byte)InventoryChangeFailure.EQUIP_ERR_OK;
                            break;
                        }
                    }
                }
                else if (srcBag > 0)
                {
                    var slots = client.Character.Items[srcBag].Items[srcSlot].ItemInfo.GetSlots;
                    foreach (byte tmpSlot in slots)
                    {
                        if (!client.Character.Items.ContainsKey(tmpSlot))
                        {
                            client.Character.ItemSWAP(srcBag, srcSlot, 0, tmpSlot);
                            errCode = (byte)InventoryChangeFailure.EQUIP_ERR_OK;
                            break;
                        }
                        else
                        {
                            errCode = (byte)InventoryChangeFailure.EQUIP_ERR_NO_EQUIPMENT_SLOT_AVAILABLE;
                        }
                    }

                    if (errCode == InventoryChangeFailure.EQUIP_ERR_NO_EQUIPMENT_SLOT_AVAILABLE)
                    {
                        foreach (byte tmpSlot in slots)
                        {
                            client.Character.ItemSWAP(srcBag, srcSlot, 0, tmpSlot);
                            errCode = (byte)InventoryChangeFailure.EQUIP_ERR_OK;
                            break;
                        }
                    }
                }
                else
                {
                    errCode = (byte)InventoryChangeFailure.EQUIP_ERR_ITEM_NOT_FOUND;
                }

                if (errCode != InventoryChangeFailure.EQUIP_ERR_OK)
                {
                    var response = new Packets.PacketClass(OPCODES.SMSG_INVENTORY_CHANGE_FAILURE);
                    response.AddInt8(errCode);
                    response.AddUInt64(client.Character.ItemGetGUID(srcBag, srcSlot));
                    response.AddUInt64(0UL);
                    response.AddInt8(0);
                    client.Send(response);
                    response.Dispose();
                }
            }
            catch (Exception err)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "[{0}:{1}] Unable to equip item. {2}{3}", client.IP, client.Port, Environment.NewLine, err.ToString());
            }
        }

        public void On_CMSG_AUTOSTORE_BAG_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 8)
                return;
            packet.GetInt16();
            byte srcBag = packet.GetInt8();
            byte srcSlot = packet.GetInt8();
            byte dstBag = packet.GetInt8();
            if (srcBag == 255)
                srcBag = 0;
            if (dstBag == 255)
                dstBag = 0;
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AUTOSTORE_BAG_ITEM [srcSlot={3}:{2}, dstBag={4}]", client.IP, client.Port, srcSlot, srcBag, dstBag);
            var tmp = WorldServiceLocator._WorldServer.WORLD_ITEMs;
            var argItem = tmp[client.Character.ItemGetGUID(srcBag, srcSlot)];
            if (client.Character.ItemADD_AutoBag(ref argItem, dstBag))
            {
                client.Character.ItemREMOVE(srcBag, srcSlot, false, true);
                SendInventoryChangeFailure(ref client.Character, InventoryChangeFailure.EQUIP_ERR_OK, client.Character.ItemGetGUID(srcBag, srcSlot), 0);
            }

            tmp[client.Character.ItemGetGUID(srcBag, srcSlot)] = argItem;
        }

        public void On_CMSG_SWAP_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 9)
                return;
            packet.GetInt16();
            byte dstBag = packet.GetInt8();
            byte dstSlot = packet.GetInt8();
            byte srcBag = packet.GetInt8();
            byte srcSlot = packet.GetInt8();
            if (dstBag == 255)
                dstBag = 0;
            if (srcBag == 255)
                srcBag = 0;
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SWAP_ITEM [srcSlot={4}:{2}, dstSlot={5}:{3}]", client.IP, client.Port, srcSlot, dstSlot, srcBag, dstBag);
            client.Character.ItemSWAP(srcBag, srcSlot, dstBag, dstSlot);
        }

        public void On_CMSG_SPLIT_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 10)
                return;
            packet.GetInt16();
            byte srcBag = packet.GetInt8();
            byte srcSlot = packet.GetInt8();
            byte dstBag = packet.GetInt8();
            byte dstSlot = packet.GetInt8();
            byte count = packet.GetInt8();
            if (dstBag == 255)
                dstBag = 0;
            if (srcBag == 255)
                srcBag = 0;
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SPLIT_ITEM [srcSlot={3}:{2}, dstBag={5}:{4}, count={6}]", client.IP, client.Port, srcSlot, srcBag, dstSlot, dstBag, count);
            if (srcBag == dstBag && srcSlot == dstSlot)
                return;
            if (count > 0)
                client.Character.ItemSPLIT(srcBag, srcSlot, dstBag, dstSlot, count);
        }

        public void On_CMSG_READ_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 7)
                return;
            packet.GetInt16();
            byte srcBag = packet.GetInt8();
            byte srcSlot = packet.GetInt8();
            if (srcBag == 255)
                srcBag = 0;
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_READ_ITEM [srcSlot={3}:{2}]", client.IP, client.Port, srcSlot, srcBag);

            // TODO: If InCombat/Dead
            short opcode = (short)OPCODES.SMSG_READ_ITEM_FAILED;
            ulong guid = 0UL;
            if (srcBag == 0)
            {
                if (client.Character.Items.ContainsKey(srcSlot))
                {
                    opcode = (short)OPCODES.SMSG_READ_ITEM_OK;
                    if (client.Character.Items[srcSlot].ItemInfo.PageText > 0)
                        guid = client.Character.Items[srcSlot].GUID;
                }
            }
            else if (client.Character.Items.ContainsKey(srcBag))
            {
                if (client.Character.Items[srcBag].Items.ContainsKey(srcSlot))
                {
                    opcode = (short)OPCODES.SMSG_READ_ITEM_OK;
                    if (client.Character.Items[srcBag].Items[srcSlot].ItemInfo.PageText > 0)
                        guid = client.Character.Items[srcBag].Items[srcSlot].GUID;
                }
            }

            if (guid != 0m)
            {
                var response = new Packets.PacketClass((OPCODES)opcode);
                response.AddUInt64(guid);
                client.Send(response);
                response.Dispose();
            }
        }

        public void On_CMSG_PAGE_TEXT_QUERY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 17)
                return;
            packet.GetInt16();
            int pageID = packet.GetInt32();
            ulong itemGuid = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_PAGE_TEXT_QUERY [pageID={2}, itemGuid={3:X}]", client.IP, client.Port, pageID, itemGuid);
            var mySqlQuery = new DataTable();
            WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT * FROM page_text WHERE entry = \"{0}\";", (object)pageID), mySqlQuery);
            var response = new Packets.PacketClass(OPCODES.SMSG_PAGE_TEXT_QUERY_RESPONSE);
            response.AddInt32(pageID);
            if (mySqlQuery.Rows.Count != 0)
                response.AddString(Conversions.ToString(mySqlQuery.Rows[0]["text"]));
            else
                response.AddString("Page " + pageID + " not found! Please report this to database devs.");
            if (mySqlQuery.Rows.Count != 0)
                response.AddInt32(Conversions.ToInteger(mySqlQuery.Rows[0]["next_page"]));
            else
                response.AddInt32(0);
            client.Send(response);
            response.Dispose();
        }

        public void On_CMSG_WRAP_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 9)
                return;
            packet.GetInt16();
            byte giftBag = packet.GetInt8();
            byte giftSlot = packet.GetInt8();
            byte itemBag = packet.GetInt8();
            byte itemSlot = packet.GetInt8();
            if (giftBag == 255)
                giftBag = 0;
            if (itemBag == 255)
                itemBag = 0;
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_WRAP_ITEM [{2}:{3} -> {4}{5}]", client.IP, client.Port, giftBag, giftSlot, itemBag, itemSlot);
            var gift = client.Character.ItemGET(giftBag, giftSlot);
            var item = client.Character.ItemGET(itemBag, itemSlot);
            if (gift is null | item is null)
            {
                SendInventoryChangeFailure(ref client.Character, InventoryChangeFailure.EQUIP_ERR_ITEM_NOT_FOUND, 0, 0);
            }

            // if(item==gift)                                          // not possable with pacjket from real client
            // {
            // _player->SendEquipError( EQUIP_ERR_WRAPPED_CANT_BE_WRAPPED, item, NULL );
            // return;
            // }

            // if(item->IsEquipped())
            // {
            // _player->SendEquipError( EQUIP_ERR_EQUIPPED_CANT_BE_WRAPPED, item, NULL );
            // return;
            // }

            // if(item->GetUInt64Value(ITEM_FIELD_GIFTCREATOR)) // HasFlag(ITEM_FIELD_FLAGS, 8);
            // {
            // _player->SendEquipError( EQUIP_ERR_WRAPPED_CANT_BE_WRAPPED, item, NULL );
            // return;
            // }

            // if(item->IsBag())
            // {
            // _player->SendEquipError( EQUIP_ERR_BAGS_CANT_BE_WRAPPED, item, NULL );
            // return;
            // }

            // if(item->IsSoulBound() || item->GetProto()->Class == ITEM_CLASS_QUEST)
            // {
            // _player->SendEquipError( EQUIP_ERR_BOUND_CANT_BE_WRAPPED, item, NULL );
            // return;
            // }

            // if(item->GetMaxStackCount() != 1)
            // {
            // _player->SendEquipError( EQUIP_ERR_STACKABLE_CANT_BE_WRAPPED, item, NULL );
            // return;
            // }

            // //if(item->IsUnique()) // need figure out unique item flags...
            // //{
            // //    _player->SendEquipError( EQUIP_ERR_UNIQUE_CANT_BE_WRAPPED, item, NULL );
            // //    return;
            // //}

            // sDatabase.BeginTransaction();
            // sDatabase.PExecute("INSERT INTO `character_gifts` VALUES ('%u', '%u', '%u', '%u')", GUID_LOPART(item->GetOwnerGUID()), item->GetGUIDLow(), item->GetEntry(), item->GetUInt32Value(ITEM_FIELD_FLAGS));
            // item->SetUInt32Value(OBJECT_FIELD_ENTRY, gift->GetUInt32Value(OBJECT_FIELD_ENTRY));
            // item->SetUInt64Value(ITEM_FIELD_GIFTCREATOR, _player->GetGUID());
            // item->SetUInt32Value(ITEM_FIELD_FLAGS, 8); // wrapped ?
            // item->SetState(ITEM_CHANGED, _player);

            // if(item->GetState()==ITEM_NEW)                          // save new item, to have alway for `character_gifts` record in `item_template`
            // item->SaveToDB();
            // sDatabase.CommitTransaction();

            // uint32 count = 1;
            // _player->DestroyItemCount(gift, count, true);
        }

        public void On_CMSG_DESTROYITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 8)
                return;
            try
            {
                packet.GetInt16();
                byte srcBag = packet.GetInt8();
                byte srcSlot = packet.GetInt8();
                byte count = packet.GetInt8();
                if (srcBag == 255)
                    srcBag = 0;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_DESTROYITEM [srcSlot={3}:{2}  count={4}]", client.IP, client.Port, srcSlot, srcBag, count);
                if (srcBag == 0)
                {
                    if (client.Character.Items.ContainsKey(srcSlot) == false)
                        return;
                    // DONE: Fire quest event to check for if this item is required for quest
                    // NOTE: Not only quest items are needed for quests
                    WorldServiceLocator._WorldServer.ALLQUESTS.OnQuestItemRemove(ref client.Character, client.Character.Items[srcSlot].ItemEntry, count);
                    if (count == 0 | count >= client.Character.Items[srcSlot].StackCount)
                    {
                        if (srcSlot < InventorySlots.INVENTORY_SLOT_BAG_END)
                        {
                            var tmp = client.Character.Items;
                            var argItem = tmp[srcSlot];
                            client.Character.UpdateRemoveItemStats(ref argItem, srcSlot);
                            tmp[srcSlot] = argItem;
                        }

                        client.Character.ItemREMOVE(srcBag, srcSlot, true, true);
                    }
                    else
                    {
                        client.Character.Items[srcSlot].StackCount -= count;
                        client.Character.SendItemUpdate(client.Character.Items[srcSlot]);
                        client.Character.Items[srcSlot].Save();
                    }
                }
                else
                {
                    if (client.Character.Items.ContainsKey(srcBag) == false)
                        return;
                    if (client.Character.Items[srcBag].Items.ContainsKey(srcSlot) == false)
                        return;
                    // DONE: Fire quest event to check for if this item is required for quest
                    // NOTE: Not only quest items are needed for quests
                    WorldServiceLocator._WorldServer.ALLQUESTS.OnQuestItemRemove(ref client.Character, client.Character.Items[srcBag].Items[srcSlot].ItemEntry, count);
                    if (count == 0 | count >= client.Character.Items[srcBag].Items[srcSlot].StackCount)
                    {
                        client.Character.ItemREMOVE(srcBag, srcSlot, true, true);
                    }
                    else
                    {
                        client.Character.Items[srcBag].Items[srcSlot].StackCount -= count;
                        client.Character.SendItemUpdate(client.Character.Items[srcBag].Items[srcSlot]);
                        client.Character.Items[srcBag].Items[srcSlot].Save();
                    }
                }
            }
            catch (Exception e)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Error destroying item.{0}", Environment.NewLine + e.ToString());
            }
        }

        public void On_CMSG_USE_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            try
            {
                if (packet.Data.Length - 1 < 9)
                    return;
                packet.GetInt16();
                byte bag = packet.GetInt8();
                if (bag == 255)
                    bag = 0;
                byte slot = packet.GetInt8();
                byte tmp3 = packet.GetInt8();
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_USE_ITEM [bag={2} slot={3} tmp3={4}]", client.IP, client.Port, bag, slot, tmp3);
                if (client.Character.cUnitFlags & UnitFlags.UNIT_FLAG_TAXI_FLIGHT)
                    return;
                // Don't allow item usage when on a taxi

                ulong itemGuid = client.Character.ItemGetGUID(bag, slot);
                if (WorldServiceLocator._WorldServer.WORLD_ITEMs.ContainsKey(itemGuid) == false)
                {
                    SendInventoryChangeFailure(ref client.Character, InventoryChangeFailure.EQUIP_ERR_ITEM_NOT_FOUND, 0, 0);
                    return;
                }

                var itemInfo = WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid].ItemInfo;

                // DONE: Check if the item can be used in combat
                bool InstantCast = false;
                for (byte i = 0; i <= 4; i++)
                {
                    if (WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(itemInfo.Spells[i].SpellID))
                    {
                        if ((client.Character.cUnitFlags & UnitFlags.UNIT_FLAG_IN_COMBAT) == UnitFlags.UNIT_FLAG_IN_COMBAT)
                        {
                            if (WorldServiceLocator._WS_Spells.SPELLs[itemInfo.Spells[i].SpellID].Attributes & SpellAttributes.SPELL_ATTR_NOT_WHILE_COMBAT)
                            {
                                SendInventoryChangeFailure(ref client.Character, InventoryChangeFailure.EQUIP_ERR_CANT_DO_IN_COMBAT, itemGuid, 0);
                                return;
                            }
                        }
                    }
                }

                if (client.Character.DEAD == true)
                {
                    SendInventoryChangeFailure(ref client.Character, InventoryChangeFailure.EQUIP_ERR_YOU_ARE_DEAD, itemGuid, 0);
                    return;
                }

                if (itemInfo.ObjectClass != ITEM_CLASS.ITEM_CLASS_CONSUMABLE)
                {
                    // DONE: Bind item to player
                    if (WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid].ItemInfo.Bonding == ITEM_BONDING_TYPE.BIND_WHEN_USED && WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid].IsSoulBound == false)
                        WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid].SoulbindItem(ref client);
                }

                // DONE: Read spell targets
                var targets = new WS_Spells.SpellTargets();
                WS_Base.BaseObject argCaster = client.Character;
                targets.ReadTargets(ref packet, ref argCaster);
                for (byte i = 0; i <= 4; i++)
                {
                    if (itemInfo.Spells[i].SpellID > 0 && (itemInfo.Spells[i].SpellTrigger == ITEM_SPELLTRIGGER_TYPE.USE || itemInfo.Spells[i].SpellTrigger == ITEM_SPELLTRIGGER_TYPE.NO_DELAY_USE))
                    {
                        if (WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(itemInfo.Spells[i].SpellID))
                        {
                            // DONE: If there's no more charges
                            if (itemInfo.Spells[i].SpellCharges > 0 && WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid].ChargesLeft == 0)
                            {
                                WorldServiceLocator._WS_Spells.SendCastResult(SpellFailedReason.SPELL_FAILED_NO_CHARGES_REMAIN, ref client, itemInfo.Spells[i].SpellID);
                                return;
                            }

                            WS_Base.BaseObject argCaster1 = client.Character;
                            var tmp = WorldServiceLocator._WorldServer.WORLD_ITEMs;
                            var argItem = tmp[itemGuid];
                            var tmpSpell = new WS_Spells.CastSpellParameters(ref targets, ref argCaster1, itemInfo.Spells[i].SpellID, ref argItem, InstantCast);
                            tmp[itemGuid] = argItem;
                            byte castResult = (byte)SpellFailedReason.SPELL_NO_ERROR;
                            try
                            {
                                castResult = (byte)WorldServiceLocator._WS_Spells.SPELLs[itemInfo.Spells[i].SpellID].CanCast(ref client.Character, targets, true);

                                // Only instant cast send ERR_OK for cast result?
                                if (castResult == SpellFailedReason.SPELL_NO_ERROR)
                                {
                                    // DONE: Enqueue spell casting function
                                    ThreadPool.QueueUserWorkItem(new WaitCallback(tmpSpell.Cast));
                                }
                                else
                                {
                                    WorldServiceLocator._WS_Spells.SendCastResult((SpellFailedReason)castResult, ref client, itemInfo.Spells[(int)i].SpellID);
                                }
                            }
                            catch (Exception e)
                            {
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Error casting spell {0}.{1}", itemInfo.Spells[i].SpellID, Environment.NewLine + e.ToString());
                                WorldServiceLocator._WS_Spells.SendCastResult((SpellFailedReason)castResult, ref client, itemInfo.Spells[(int)i].SpellID);
                            }

                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error while using a item.{0}", Environment.NewLine + ex.ToString());
            }
        }

        public void On_CMSG_OPEN_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 7)
                return;
            packet.GetInt16();
            byte bag = packet.GetInt8();
            if (bag == 255)
                bag = 0;
            byte slot = packet.GetInt8();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_OPEN_ITEM [bag={2} slot={3}]", client.IP, client.Port, bag, slot);
            ulong itemGuid;
            if (bag == 0)
            {
                itemGuid = client.Character.Items[slot].GUID;
            }
            else
            {
                itemGuid = client.Character.Items[bag].Items[slot].GUID;
            }

            if (itemGuid == 0m || WorldServiceLocator._WorldServer.WORLD_ITEMs.ContainsKey(itemGuid) == false)
                return;
            if (WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid].GenerateLoot())
            {
                WorldServiceLocator._WS_Loot.LootTable[itemGuid].SendLoot(ref client);
                return;
            }

            WorldServiceLocator._WS_Loot.SendEmptyLoot(itemGuid, LootType.LOOTTYPE_CORPSE, ref client);
        }

        public void SendInventoryChangeFailure(ref WS_PlayerData.CharacterObject objCharacter, InventoryChangeFailure errorCode, ulong guid1, ulong guid2)
        {
            var packet = new Packets.PacketClass(OPCODES.SMSG_INVENTORY_CHANGE_FAILURE);
            packet.AddInt8((byte)errorCode);
            if (errorCode == InventoryChangeFailure.EQUIP_ERR_YOU_MUST_REACH_LEVEL_N)
            {
                packet.AddInt32(WorldServiceLocator._WorldServer.WORLD_ITEMs[guid1].ItemInfo.ReqLevel);
            }

            packet.AddUInt64(guid1);
            packet.AddUInt64(guid2);
            packet.AddInt8(0);
            objCharacter.client.Send(packet);
            packet.Dispose();
        }

        // Public Sub SendEnchantmentLog(ByRef objCharacter As CharacterObject, ByVal iGUID As ULong, ByVal iEntry As Integer,
        // ByVal iSpellID As Integer)
        // Dim packet As New PacketClass(OPCODES.SMSG_ENCHANTMENTLOG)
        // packet.AddUInt64(iGUID)
        // packet.AddUInt64(objCharacter.GUID)
        // packet.AddInt32(iEntry)
        // packet.AddInt32(iSpellID)
        // packet.AddInt8(0)
        // objCharacter.Client.Send(packet)
        // packet.Dispose()
        // End Sub

        // Public Sub SendEnchantmentTimeUpdate(ByRef objCharacter As CharacterObject, ByVal iGUID As ULong, ByVal iSlot As Integer,
        // ByVal iTime As Integer)
        // Dim packet As New PacketClass(OPCODES.SMSG_ITEM_ENCHANT_TIME_UPDATE)
        // packet.AddUInt64(iGUID)
        // packet.AddInt32(iSlot)
        // packet.AddInt32(iTime)
        // objCharacter.Client.Send(packet)
        // packet.Dispose()
        // End Sub

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
    }
}