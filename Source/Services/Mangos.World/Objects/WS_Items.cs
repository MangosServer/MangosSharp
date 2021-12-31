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
using Mangos.Common.Enums.Item;
using Mangos.Common.Enums.Spell;
using Mangos.Common.Globals;
using Mangos.Common.Legacy;
using Mangos.World.Globals;
using Mangos.World.Network;
using Mangos.World.Player;
using Mangos.World.Spells;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Mangos.World.Objects;

public class WS_Items
{
    public class ItemInfo : IDisposable
    {
        private readonly bool _found;

        private bool _disposedValue;

        public readonly int Model;

        public readonly string Name;

        public readonly int Quality;

        public readonly int Material;

        public readonly int Durability;

        public readonly SHEATHE_TYPE Sheath;

        public readonly int Bonding;

        public readonly int BuyCount;

        public readonly int BuyPrice;

        public readonly int SellPrice;

        public readonly int Id;

        public readonly int Flags;

        public readonly ITEM_CLASS ObjectClass;

        public readonly ITEM_SUBCLASS SubClass;

        public readonly INVENTORY_TYPES InventoryType;

        public readonly int Level;

        public readonly uint AvailableClasses;

        public readonly uint AvailableRaces;

        public readonly int ReqLevel;

        public readonly int ReqSkill;

        public readonly int ReqSkillRank;

        public int ReqSkillSubRank;

        public readonly int ReqSpell;

        public readonly int ReqFaction;

        public readonly int ReqFactionLevel;

        public readonly int ReqHonorRank;

        public readonly int ReqHonorRank2;

        public readonly int AmmoType;

        public readonly int PageText;

        public readonly int Stackable;

        public readonly int Unique;

        public readonly string Description;

        public readonly int Block;

        public readonly int ItemSet;

        public readonly int PageMaterial;

        public readonly int StartQuest;

        public readonly int ContainerSlots;

        public readonly int LanguageID;

        public readonly ITEM_BAG BagFamily;

        public readonly int Delay;

        public readonly float Range;

        public readonly TDamage[] Damage;

        public readonly int[] Resistances;

        public readonly int[] ItemBonusStatType;

        public readonly int[] ItemBonusStatValue;

        public readonly TItemSpellInfo[] Spells;

        private readonly int _reqDisenchantSkill;

        public float ArmorDamageModifier;

        public int ExistingDuration;

        public int Unk2;

        public readonly int LockID;

        public int Extra;

        public int Area;

        public readonly int ZoneNameID;

        public int MapID;

        public int TotemCategory;

        public int RandomProp;

        public int RandomSuffix;

        public bool IsContainer => ContainerSlots > 0;

        public byte[] GetSlots => InventoryType switch
        {
            INVENTORY_TYPES.INVTYPE_HEAD => new byte[1],
            INVENTORY_TYPES.INVTYPE_NECK => new byte[1]
            {
                    1
            },
            INVENTORY_TYPES.INVTYPE_SHOULDERS => new byte[1]
            {
                    2
            },
            INVENTORY_TYPES.INVTYPE_BODY => new byte[1]
            {
                    3
            },
            INVENTORY_TYPES.INVTYPE_CHEST => new byte[1]
            {
                    4
            },
            INVENTORY_TYPES.INVTYPE_ROBE => new byte[1]
            {
                    4
            },
            INVENTORY_TYPES.INVTYPE_WAIST => new byte[1]
            {
                    5
            },
            INVENTORY_TYPES.INVTYPE_LEGS => new byte[1]
            {
                    6
            },
            INVENTORY_TYPES.INVTYPE_FEET => new byte[1]
            {
                    7
            },
            INVENTORY_TYPES.INVTYPE_WRISTS => new byte[1]
            {
                    8
            },
            INVENTORY_TYPES.INVTYPE_HANDS => new byte[1]
            {
                    9
            },
            INVENTORY_TYPES.INVTYPE_FINGER => new byte[2]
            {
                    10,
                    11
            },
            INVENTORY_TYPES.INVTYPE_TRINKET => new byte[2]
            {
                    12,
                    13
            },
            INVENTORY_TYPES.INVTYPE_CLOAK => new byte[1]
            {
                    14
            },
            INVENTORY_TYPES.INVTYPE_WEAPON => new byte[2]
            {
                    15,
                    16
            },
            INVENTORY_TYPES.INVTYPE_SHIELD => new byte[1]
            {
                    16
            },
            INVENTORY_TYPES.INVTYPE_RANGED => new byte[1]
            {
                    17
            },
            INVENTORY_TYPES.INVTYPE_TWOHAND_WEAPON => new byte[1]
            {
                    15
            },
            INVENTORY_TYPES.INVTYPE_TABARD => new byte[1]
            {
                    18
            },
            INVENTORY_TYPES.INVTYPE_WEAPONMAINHAND => new byte[1]
            {
                    15
            },
            INVENTORY_TYPES.INVTYPE_WEAPONOFFHAND => new byte[1]
            {
                    16
            },
            INVENTORY_TYPES.INVTYPE_HOLDABLE => new byte[1]
            {
                    16
            },
            INVENTORY_TYPES.INVTYPE_THROWN => new byte[1]
            {
                    17
            },
            INVENTORY_TYPES.INVTYPE_RANGEDRIGHT => new byte[1]
            {
                    17
            },
            INVENTORY_TYPES.INVTYPE_BAG => new byte[4]
            {
                    19,
                    20,
                    21,
                    22
            },
            INVENTORY_TYPES.INVTYPE_RELIC => new byte[1]
            {
                    17
            },
            _ => Array.Empty<byte>(),
        };

        public int GetReqSkill
        {
            get
            {
                if (ObjectClass == ITEM_CLASS.ITEM_CLASS_WEAPON)
                {
                    return WorldServiceLocator._WS_Items.ItemWeaponSkills[(uint)SubClass];
                }
                return ObjectClass == ITEM_CLASS.ITEM_CLASS_ARMOR ? WorldServiceLocator._WS_Items.ItemArmorSkills[(uint)SubClass] : 0;
            }
        }

        public short GetReqSpell
        {
            get
            {
                switch (ObjectClass)
                {
                    case ITEM_CLASS.ITEM_CLASS_WEAPON:
                        switch (SubClass)
                        {
                            case ITEM_SUBCLASS.ITEM_SUBCLASS_MISC_WEAPON:
                                return 0;

                            case ITEM_SUBCLASS.ITEM_SUBCLASS_CONSUMABLE:
                                return 196;

                            case ITEM_SUBCLASS.ITEM_SUBCLASS_FOOD:
                                return 197;

                            case ITEM_SUBCLASS.ITEM_SUBCLASS_LIQUID:
                                return 264;

                            case ITEM_SUBCLASS.ITEM_SUBCLASS_POTION:
                                return 266;

                            case ITEM_SUBCLASS.ITEM_SUBCLASS_SCROLL:
                                return 198;

                            case ITEM_SUBCLASS.ITEM_SUBCLASS_BANDAGE:
                                return 199;

                            case ITEM_SUBCLASS.ITEM_SUBCLASS_HEALTHSTONE:
                                return 200;

                            case ITEM_SUBCLASS.ITEM_SUBCLASS_COMBAT_EFFECT:
                                return 201;

                            case ITEM_SUBCLASS.ITEM_SUBCLASS_TWOHAND_SWORD:
                                return 202;

                            case ITEM_SUBCLASS.ITEM_SUBCLASS_STAFF:
                                return 227;

                            case ITEM_SUBCLASS.ITEM_SUBCLASS_WEAPON_EXOTIC:
                                return 262;

                            case ITEM_SUBCLASS.ITEM_SUBCLASS_WEAPON_EXOTIC2:
                                return 263;

                            case ITEM_SUBCLASS.ITEM_SUBCLASS_FIST_WEAPON:
                                return 15590;

                            case ITEM_SUBCLASS.ITEM_SUBCLASS_DAGGER:
                                return 1180;

                            case ITEM_SUBCLASS.ITEM_SUBCLASS_THROWN:
                                return 2567;

                            case ITEM_SUBCLASS.ITEM_SUBCLASS_SPEAR:
                                return 3386;

                            case ITEM_SUBCLASS.ITEM_SUBCLASS_CROSSBOW:
                                return 5011;

                            case ITEM_SUBCLASS.ITEM_SUBCLASS_WAND:
                                return 5009;

                            case ITEM_SUBCLASS.ITEM_SUBCLASS_FISHING_POLE:
                                return 7738;
                        }
                        break;

                    case ITEM_CLASS.ITEM_CLASS_ARMOR:
                        switch (SubClass)
                        {
                            case ITEM_SUBCLASS.ITEM_SUBCLASS_CONSUMABLE:
                                return 0;

                            case ITEM_SUBCLASS.ITEM_SUBCLASS_FOOD:
                                return 9078;

                            case ITEM_SUBCLASS.ITEM_SUBCLASS_LIQUID:
                                return 9077;

                            case ITEM_SUBCLASS.ITEM_SUBCLASS_POTION:
                                return 8737;

                            case ITEM_SUBCLASS.ITEM_SUBCLASS_SCROLL:
                                return 750;

                            case ITEM_SUBCLASS.ITEM_SUBCLASS_HEALTHSTONE:
                                return 9116;

                            case ITEM_SUBCLASS.ITEM_SUBCLASS_BANDAGE:
                                return 9124;

                            case ITEM_SUBCLASS.ITEM_SUBCLASS_COMBAT_EFFECT:
                                return 27762;

                            case ITEM_SUBCLASS.ITEM_SUBCLASS_WEAPON_obsolete:
                                return 27763;

                            case ITEM_SUBCLASS.ITEM_SUBCLASS_TWOHAND_SWORD:
                                return 27764;
                        }
                        break;

                    default:
                        return 0;
                }
                return 0;
            }
        }

        private ItemInfo()
        {
            _found = false;
            Model = 0;
            Name = "Unknown Item";
            Quality = 0;
            Material = 0;
            Durability = 0;
            Sheath = SHEATHE_TYPE.SHEATHETYPE_NONE;
            Bonding = 0;
            BuyCount = 0;
            BuyPrice = 0;
            SellPrice = 0;
            Id = 0;
            Flags = 0;
            ObjectClass = ITEM_CLASS.ITEM_CLASS_CONSUMABLE;
            SubClass = ITEM_SUBCLASS.ITEM_SUBCLASS_CONSUMABLE;
            InventoryType = INVENTORY_TYPES.INVTYPE_NON_EQUIP;
            Level = 0;
            AvailableClasses = 0u;
            AvailableRaces = 0u;
            ReqLevel = 0;
            ReqSkill = 0;
            ReqSkillRank = 0;
            ReqSpell = 0;
            ReqFaction = 0;
            ReqFactionLevel = 0;
            ReqHonorRank = 0;
            ReqHonorRank2 = 0;
            AmmoType = 0;
            PageText = 0;
            Stackable = 1;
            Unique = 0;
            Description = "";
            Block = 0;
            ItemSet = 0;
            PageMaterial = 0;
            StartQuest = 0;
            ContainerSlots = 0;
            LanguageID = 0;
            BagFamily = ITEM_BAG.NONE;
            Delay = 0;
            Range = 0f;
            Damage = new TDamage[5];
            Resistances = new int[7];
            ItemBonusStatType = new int[10];
            ItemBonusStatValue = new int[10];
            Spells = new TItemSpellInfo[5];
            _reqDisenchantSkill = -1;
            ArmorDamageModifier = 0f;
            ExistingDuration = 0;
            Unk2 = 0;
            LockID = 0;
            Extra = 0;
            Area = 0;
            ZoneNameID = 0;
            MapID = 0;
            TotemCategory = 0;
            RandomProp = 0;
            RandomSuffix = 0;
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

        public ItemInfo(int itemId)
            : this()
        {
            Id = itemId;
            WorldServiceLocator._WorldServer.ITEMDatabase.Add(Id, this);
            DataTable mySqlQuery = new();
            WorldServiceLocator._WorldServer.WorldDatabase.Query($"SELECT * FROM item_template WHERE entry = {itemId};", ref mySqlQuery);
            if (mySqlQuery.Rows.Count == 0)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "ItemID {0} not found in SQL database! Loading default \"Unknown Item\" info.", itemId);
                _found = false;
                return;
            }
            _found = true;
            Model = mySqlQuery.Rows[0].As<int>("displayid");
            Name = mySqlQuery.Rows[0].As<string>("name");
            Quality = mySqlQuery.Rows[0].As<int>("quality");
            Material = mySqlQuery.Rows[0].As<int>("Material");
            Durability = mySqlQuery.Rows[0].As<int>("MaxDurability");
            Sheath = (SHEATHE_TYPE)mySqlQuery.Rows[0].As<byte>("sheath");
            Bonding = mySqlQuery.Rows[0].As<int>("bonding");
            BuyCount = mySqlQuery.Rows[0].As<int>("buycount");
            BuyPrice = mySqlQuery.Rows[0].As<int>("buyprice");
            SellPrice = mySqlQuery.Rows[0].As<int>("sellprice");
            Id = mySqlQuery.Rows[0].As<int>("entry");
            Flags = mySqlQuery.Rows[0].As<int>("flags");
            ObjectClass = (ITEM_CLASS)mySqlQuery.Rows[0].As<byte>("class");
            SubClass = (ITEM_SUBCLASS)mySqlQuery.Rows[0].As<byte>("subclass");
            InventoryType = (INVENTORY_TYPES)mySqlQuery.Rows[0].As<byte>("inventorytype");
            Level = mySqlQuery.Rows[0].As<int>("itemlevel");
            var typeFromHandle = typeof(BitConverter);
            DataRow row;
            var obj = new object[1]
            {
                    (row = mySqlQuery.Rows[0])["allowableclass"]
            };
            var array = obj;
            var obj2 = new bool[1]
            {
                    true
            };
            var array2 = obj2;
            var obj3 = NewLateBinding.LateGet(null, typeFromHandle, "GetBytes", obj, null, null, obj2);
            if (array2[0])
            {
                row["allowableclass"] = RuntimeHelpers.GetObjectValue(RuntimeHelpers.GetObjectValue(array[0]));
            }
            AvailableClasses = BitConverter.ToUInt32((byte[])obj3, 0);
            var obj4 = NewLateBinding.LateGet(null, typeof(BitConverter), "GetBytes", array = new object[1]
            {
                    (row = mySqlQuery.Rows[0])["allowablerace"]
            }, null, null, array2 = new bool[1]
            {
                    true
            });
            if (array2[0])
            {
                row["allowablerace"] = RuntimeHelpers.GetObjectValue(RuntimeHelpers.GetObjectValue(array[0]));
            }
            AvailableRaces = BitConverter.ToUInt32((byte[])obj4, 0);
            ReqLevel = mySqlQuery.Rows[0].As<int>("requiredlevel");
            ReqSkill = mySqlQuery.Rows[0].As<int>("RequiredSkill");
            ReqSkillRank = mySqlQuery.Rows[0].As<int>("RequiredSkillRank");
            ReqSpell = mySqlQuery.Rows[0].As<int>("requiredspell");
            ReqFaction = mySqlQuery.Rows[0].As<int>("RequiredReputationFaction");
            ReqFactionLevel = mySqlQuery.Rows[0].As<int>("RequiredReputationRank");
            ReqHonorRank = mySqlQuery.Rows[0].As<int>("requiredhonorrank");
            ReqHonorRank2 = mySqlQuery.Rows[0].As<int>("RequiredCityRank");
            AmmoType = mySqlQuery.Rows[0].As<int>("ammo_type");
            PageText = mySqlQuery.Rows[0].As<int>("PageText");
            Stackable = mySqlQuery.Rows[0].As<int>("stackable");
            Unique = mySqlQuery.Rows[0].As<int>("maxcount");
            Description = mySqlQuery.Rows[0].As<string>("description");
            Block = mySqlQuery.Rows[0].As<int>("block");
            ItemSet = mySqlQuery.Rows[0].As<int>("itemset");
            PageMaterial = mySqlQuery.Rows[0].As<int>("PageMaterial");
            StartQuest = mySqlQuery.Rows[0].As<int>("startquest");
            ContainerSlots = mySqlQuery.Rows[0].As<int>("ContainerSlots");
            LanguageID = mySqlQuery.Rows[0].As<int>("LanguageID");
            BagFamily = (ITEM_BAG)mySqlQuery.Rows[0].As<int>("BagFamily");
            Delay = mySqlQuery.Rows[0].As<int>("delay");
            Range = mySqlQuery.Rows[0].As<float>("RangedModRange");
            Damage[0].Minimum = mySqlQuery.Rows[0].As<float>("dmg_min1");
            Damage[0].Maximum = mySqlQuery.Rows[0].As<float>("dmg_max1");
            Damage[0].Type = mySqlQuery.Rows[0].As<int>("dmg_type1");
            Damage[1].Minimum = mySqlQuery.Rows[0].As<float>("dmg_min2");
            Damage[1].Maximum = mySqlQuery.Rows[0].As<float>("dmg_max2");
            Damage[1].Type = mySqlQuery.Rows[0].As<int>("dmg_type2");
            Damage[2].Minimum = mySqlQuery.Rows[0].As<float>("dmg_min3");
            Damage[2].Maximum = mySqlQuery.Rows[0].As<float>("dmg_max3");
            Damage[2].Type = mySqlQuery.Rows[0].As<int>("dmg_type3");
            Damage[3].Minimum = mySqlQuery.Rows[0].As<float>("dmg_min4");
            Damage[3].Maximum = mySqlQuery.Rows[0].As<float>("dmg_max4");
            Damage[3].Type = mySqlQuery.Rows[0].As<int>("dmg_type4");
            Damage[4].Minimum = mySqlQuery.Rows[0].As<float>("dmg_min5");
            Damage[4].Maximum = mySqlQuery.Rows[0].As<float>("dmg_max5");
            Damage[4].Type = mySqlQuery.Rows[0].As<int>("dmg_type5");
            Resistances[0] = mySqlQuery.Rows[0].As<int>("armor");
            Resistances[1] = mySqlQuery.Rows[0].As<int>("holy_res");
            Resistances[2] = mySqlQuery.Rows[0].As<int>("fire_res");
            Resistances[3] = mySqlQuery.Rows[0].As<int>("nature_res");
            Resistances[4] = mySqlQuery.Rows[0].As<int>("frost_res");
            Resistances[5] = mySqlQuery.Rows[0].As<int>("shadow_res");
            Resistances[6] = mySqlQuery.Rows[0].As<int>("arcane_res");
            Spells[0].SpellID = mySqlQuery.Rows[0].As<int>("spellid_1");
            Spells[0].SpellTrigger = (ITEM_SPELLTRIGGER_TYPE)mySqlQuery.Rows[0].As<byte>("spelltrigger_1");
            Spells[0].SpellCharges = mySqlQuery.Rows[0].As<int>("spellcharges_1");
            Spells[0].SpellCooldown = mySqlQuery.Rows[0].As<int>("spellcooldown_1");
            Spells[0].SpellCategory = mySqlQuery.Rows[0].As<int>("spellcategory_1");
            Spells[0].SpellCategoryCooldown = mySqlQuery.Rows[0].As<int>("spellcategorycooldown_1");
            Spells[1].SpellID = mySqlQuery.Rows[0].As<int>("spellid_2");
            Spells[1].SpellTrigger = (ITEM_SPELLTRIGGER_TYPE)mySqlQuery.Rows[0].As<byte>("spelltrigger_2");
            Spells[1].SpellCharges = mySqlQuery.Rows[0].As<int>("spellcharges_2");
            Spells[1].SpellCooldown = mySqlQuery.Rows[0].As<int>("spellcooldown_2");
            Spells[1].SpellCategory = mySqlQuery.Rows[0].As<int>("spellcategory_2");
            Spells[1].SpellCategoryCooldown = mySqlQuery.Rows[0].As<int>("spellcategorycooldown_2");
            Spells[2].SpellID = mySqlQuery.Rows[0].As<int>("spellid_3");
            Spells[2].SpellTrigger = (ITEM_SPELLTRIGGER_TYPE)mySqlQuery.Rows[0].As<byte>("spelltrigger_3");
            Spells[2].SpellCharges = mySqlQuery.Rows[0].As<int>("spellcharges_3");
            Spells[2].SpellCooldown = mySqlQuery.Rows[0].As<int>("spellcooldown_3");
            Spells[2].SpellCategory = mySqlQuery.Rows[0].As<int>("spellcategory_3");
            Spells[2].SpellCategoryCooldown = mySqlQuery.Rows[0].As<int>("spellcategorycooldown_3");
            Spells[3].SpellID = mySqlQuery.Rows[0].As<int>("spellid_4");
            Spells[3].SpellTrigger = (ITEM_SPELLTRIGGER_TYPE)mySqlQuery.Rows[0].As<byte>("spelltrigger_4");
            Spells[3].SpellCharges = mySqlQuery.Rows[0].As<int>("spellcharges_4");
            Spells[3].SpellCooldown = mySqlQuery.Rows[0].As<int>("spellcooldown_4");
            Spells[3].SpellCategory = mySqlQuery.Rows[0].As<int>("spellcategory_4");
            Spells[3].SpellCategoryCooldown = mySqlQuery.Rows[0].As<int>("spellcategorycooldown_4");
            Spells[4].SpellID = mySqlQuery.Rows[0].As<int>("spellid_5");
            Spells[4].SpellTrigger = (ITEM_SPELLTRIGGER_TYPE)mySqlQuery.Rows[0].As<byte>("spelltrigger_5");
            Spells[4].SpellCharges = mySqlQuery.Rows[0].As<int>("spellcharges_5");
            Spells[4].SpellCooldown = mySqlQuery.Rows[0].As<int>("spellcooldown_5");
            Spells[4].SpellCategory = mySqlQuery.Rows[0].As<int>("spellcategory_5");
            Spells[4].SpellCategoryCooldown = mySqlQuery.Rows[0].As<int>("spellcategorycooldown_5");
            LockID = mySqlQuery.Rows[0].As<int>("lockid");
            ItemBonusStatType[0] = mySqlQuery.Rows[0].As<int>("stat_type1");
            ItemBonusStatValue[0] = mySqlQuery.Rows[0].As<int>("stat_value1");
            ItemBonusStatType[1] = mySqlQuery.Rows[0].As<int>("stat_type2");
            ItemBonusStatValue[1] = mySqlQuery.Rows[0].As<int>("stat_value2");
            ItemBonusStatType[2] = mySqlQuery.Rows[0].As<int>("stat_type3");
            ItemBonusStatValue[2] = mySqlQuery.Rows[0].As<int>("stat_value3");
            ItemBonusStatType[3] = mySqlQuery.Rows[0].As<int>("stat_type4");
            ItemBonusStatValue[3] = mySqlQuery.Rows[0].As<int>("stat_value4");
            ItemBonusStatType[4] = mySqlQuery.Rows[0].As<int>("stat_type5");
            ItemBonusStatValue[4] = mySqlQuery.Rows[0].As<int>("stat_value5");
            ItemBonusStatType[5] = mySqlQuery.Rows[0].As<int>("stat_type6");
            ItemBonusStatValue[5] = mySqlQuery.Rows[0].As<int>("stat_value6");
            ItemBonusStatType[6] = mySqlQuery.Rows[0].As<int>("stat_type7");
            ItemBonusStatValue[6] = mySqlQuery.Rows[0].As<int>("stat_value7");
            ItemBonusStatType[7] = mySqlQuery.Rows[0].As<int>("stat_type8");
            ItemBonusStatValue[7] = mySqlQuery.Rows[0].As<int>("stat_value8");
            ItemBonusStatType[8] = mySqlQuery.Rows[0].As<int>("stat_type9");
            ItemBonusStatValue[8] = mySqlQuery.Rows[0].As<int>("stat_value9");
            ItemBonusStatType[9] = mySqlQuery.Rows[0].As<int>("stat_type10");
            ItemBonusStatValue[9] = mySqlQuery.Rows[0].As<int>("stat_value10");
            ZoneNameID = mySqlQuery.Rows[0].As<int>("area");
            _reqDisenchantSkill = mySqlQuery.Rows[0].As<int>("DisenchantID");
            if (Stackable == 0)
            {
                Stackable = 1;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                WorldServiceLocator._WorldServer.ITEMDatabase.Remove(Id);
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
            Dispose();
        }
    }

    public class TDamage
    {
        public float Minimum;

        public float Maximum;

        public int Type;

        public TDamage()
        {
            Minimum = 0f;
            Maximum = 0f;
            Type = 0;
        }
    }

    public class TEnchantmentInfo
    {
        public readonly int ID;

        public readonly int Duration;

        public readonly int Charges;

        public TEnchantmentInfo(int ID_, int Duration_ = 0, int Charges_ = 0)
        {
            ID = 0;
            Duration = 0;
            Charges = 0;
            ID = ID_;
            Duration = Duration_;
            Charges = Charges_;
        }
    }

    public class TItemSpellInfo
    {
        public int SpellID;

        public ITEM_SPELLTRIGGER_TYPE SpellTrigger;

        public int SpellCharges;

        public int SpellCooldown;

        public int SpellCategory;

        public int SpellCategoryCooldown;

        public TItemSpellInfo()
        {
            SpellID = 0;
            SpellTrigger = ITEM_SPELLTRIGGER_TYPE.USE;
            SpellCharges = -1;
            SpellCooldown = 0;
            SpellCategory = 0;
            SpellCategoryCooldown = 0;
        }
    }

    private readonly int[] ItemWeaponSkills;

    private readonly int[] ItemArmorSkills;

    public WS_Items()
    {
        ItemWeaponSkills = new int[21]
        {
                44,
                172,
                45,
                46,
                54,
                160,
                229,
                43,
                55,
                0,
                136,
                0,
                0,
                0,
                0,
                173,
                176,
                227,
                226,
                228,
                356
        };
        ItemArmorSkills = new int[10]
        {
                0,
                415,
                414,
                413,
                293,
                0,
                433,
                0,
                0,
                0
        };
    }

    public ItemObject LoadItemByGUID(ulong guid, WS_PlayerData.CharacterObject owner = null, bool equipped = false)
    {
        checked
        {
            return WorldServiceLocator._WorldServer.WORLD_ITEMs.ContainsKey(guid + WorldServiceLocator._Global_Constants.GUID_ITEM)
                ? WorldServiceLocator._WorldServer.WORLD_ITEMs[guid + WorldServiceLocator._Global_Constants.GUID_ITEM]
                : new ItemObject(guid, owner, equipped);
        }
    }

    public void SendItemInfo(ref WS_Network.ClientClass client, int itemID)
    {
        Packets.PacketClass response = new(Opcodes.SMSG_ITEM_QUERY_SINGLE_RESPONSE);
        var item = WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(itemID) ? WorldServiceLocator._WorldServer.ITEMDatabase[itemID] : new ItemInfo(itemID);
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
        response.AddInt8(0);
        response.AddInt8(0);
        response.AddInt8(0);
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
        response.AddInt32(item.ReqFaction);
        response.AddInt32(item.ReqFactionLevel);
        response.AddInt32(item.Unique);
        response.AddInt32(item.Stackable);
        response.AddInt32(item.ContainerSlots);
        var l = 0;
        checked
        {
            do
            {
                response.AddInt32(item.ItemBonusStatType[l]);
                response.AddInt32(item.ItemBonusStatValue[l]);
                l++;
            }
            while (l <= 9);
            var k = 0;
            do
            {
                response.AddSingle(item.Damage[k].Minimum);
                response.AddSingle(item.Damage[k].Maximum);
                response.AddInt32(item.Damage[k].Type);
                k++;
            }
            while (k <= 4);
            var j = 0;
            do
            {
                response.AddInt32(item.Resistances[j]);
                j++;
            }
            while (j <= 6);
            response.AddInt32(item.Delay);
            response.AddInt32(item.AmmoType);
            response.AddSingle(item.Range);
            var i = 0;
            do
            {
                if (!WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(item.Spells[i].SpellID))
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
                i++;
            }
            while (i <= 4);
            response.AddInt32(item.Bonding);
            response.AddString(item.Description);
            response.AddInt32(item.PageText);
            response.AddInt32(item.LanguageID);
            response.AddInt32(item.PageMaterial);
            response.AddInt32(item.StartQuest);
            response.AddInt32(item.LockID);
            response.AddInt32(item.Material);
        }
        response.AddInt32((int)item.Sheath);
        response.AddInt32(item.Extra);
        response.AddInt32(item.Block);
        response.AddInt32(item.ItemSet);
        response.AddInt32(item.Durability);
        response.AddInt32(item.ZoneNameID);
        response.AddInt32(item.MapID);
        response.AddInt32((int)item.BagFamily);
        client.Send(ref response);
        response.Dispose();
    }

    public void On_CMSG_ITEM_QUERY_SINGLE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 9)
        {
            packet.GetInt16();
            var itemID = packet.GetInt32();
            SendItemInfo(ref client, itemID);
        }
    }

    public void On_CMSG_ITEM_NAME_QUERY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 9)
        {
            packet.GetInt16();
            var itemID = packet.GetInt32();
            var item = WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(itemID) ? WorldServiceLocator._WorldServer.ITEMDatabase[itemID] : new ItemInfo(itemID);
            Packets.PacketClass response = new(Opcodes.SMSG_ITEM_NAME_QUERY_RESPONSE);
            response.AddInt32(itemID);
            response.AddString(item.Name);
            response.AddInt32((int)item.InventoryType);
            client.Send(ref response);
            response.Dispose();
        }
    }

    public void On_CMSG_SWAP_INV_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 7)
        {
            packet.GetInt16();
            var srcSlot = packet.GetInt8();
            var dstSlot = packet.GetInt8();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SWAP_INV_ITEM [srcSlot=0:{2}, dstSlot=0:{3}]", client.IP, client.Port, srcSlot, dstSlot);
            client.Character.ItemSWAP(0, srcSlot, 0, dstSlot);
        }
    }

    public void On_CMSG_AUTOEQUIP_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) < 7)
        {
            return;
        }
        try
        {
            packet.GetInt16();
            var srcBag = packet.GetInt8();
            var srcSlot = packet.GetInt8();
            if (srcBag == byte.MaxValue)
            {
                srcBag = 0;
            }
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AUTOEQUIP_ITEM [srcSlot={3}:{2}]", client.IP, client.Port, srcSlot, srcBag);
            byte errCode = 20;
            if (client.Character.ItemGET(srcBag, srcSlot).OwnerGUID != client.Character.GUID)
            {
                errCode = 32;
            }
            else if (srcBag == 0 && client.Character.Items.ContainsKey(srcSlot))
            {
                var slots2 = client.Character.Items[srcSlot].ItemInfo.GetSlots;
                var array = slots2;
                foreach (var tmpSlot4 in array)
                {
                    if (!client.Character.Items.ContainsKey(tmpSlot4))
                    {
                        client.Character.ItemSWAP(srcBag, srcSlot, 0, tmpSlot4);
                        errCode = 0;
                        break;
                    }
                    errCode = 9;
                }
                if (errCode == 9)
                {
                    var array2 = slots2;
                    var num = 0;
                    if (num < array2.Length)
                    {
                        var tmpSlot3 = array2[num];
                        client.Character.ItemSWAP(srcBag, srcSlot, 0, tmpSlot3);
                        errCode = 0;
                    }
                }
            }
            else if (srcBag > 0)
            {
                var slots = client.Character.Items[srcBag].Items[srcSlot].ItemInfo.GetSlots;
                var array3 = slots;
                foreach (var tmpSlot2 in array3)
                {
                    if (!client.Character.Items.ContainsKey(tmpSlot2))
                    {
                        client.Character.ItemSWAP(srcBag, srcSlot, 0, tmpSlot2);
                        errCode = 0;
                        break;
                    }
                    errCode = 9;
                }
                if (errCode == 9)
                {
                    var array4 = slots;
                    var num2 = 0;
                    if (num2 < array4.Length)
                    {
                        var tmpSlot = array4[num2];
                        client.Character.ItemSWAP(srcBag, srcSlot, 0, tmpSlot);
                        errCode = 0;
                    }
                }
            }
            else
            {
                errCode = 23;
            }
            if (errCode != 0)
            {
                Packets.PacketClass response = new(Opcodes.SMSG_INVENTORY_CHANGE_FAILURE);
                response.AddInt8(errCode);
                response.AddUInt64(client.Character.ItemGetGUID(srcBag, srcSlot));
                response.AddUInt64(0uL);
                response.AddInt8(0);
                client.Send(ref response);
                response.Dispose();
            }
        }
        catch (Exception ex)
        {
            ProjectData.SetProjectError(ex);
            var err = ex;
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "[{0}:{1}] Unable to equip item. {2}{3}", client.IP, client.Port, Environment.NewLine, err.ToString());
            ProjectData.ClearProjectError();
        }
    }

    public void On_CMSG_AUTOSTORE_BAG_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 8)
        {
            packet.GetInt16();
            var srcBag = packet.GetInt8();
            var srcSlot = packet.GetInt8();
            var dstBag = packet.GetInt8();
            if (srcBag == byte.MaxValue)
            {
                srcBag = 0;
            }
            if (dstBag == byte.MaxValue)
            {
                dstBag = 0;
            }
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AUTOSTORE_BAG_ITEM [srcSlot={3}:{2}, dstBag={4}]", client.IP, client.Port, srcSlot, srcBag, dstBag);
            var character = client.Character;
            Dictionary<ulong, ItemObject> wORLD_ITEMs;
            ulong key;
            var Item = (wORLD_ITEMs = WorldServiceLocator._WorldServer.WORLD_ITEMs)[key = client.Character.ItemGetGUID(srcBag, srcSlot)];
            var num = character.ItemADD_AutoBag(ref Item, dstBag);
            wORLD_ITEMs[key] = Item;
            if (num)
            {
                client.Character.ItemREMOVE(srcBag, srcSlot, Destroy: false, Update: true);
                SendInventoryChangeFailure(ref client.Character, InventoryChangeFailure.EQUIP_ERR_OK, client.Character.ItemGetGUID(srcBag, srcSlot), 0uL);
            }
        }
    }

    public void On_CMSG_SWAP_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 9)
        {
            packet.GetInt16();
            var dstBag = packet.GetInt8();
            var dstSlot = packet.GetInt8();
            var srcBag = packet.GetInt8();
            var srcSlot = packet.GetInt8();
            if (dstBag == byte.MaxValue)
            {
                dstBag = 0;
            }
            if (srcBag == byte.MaxValue)
            {
                srcBag = 0;
            }
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SWAP_ITEM [srcSlot={4}:{2}, dstSlot={5}:{3}]", client.IP, client.Port, srcSlot, dstSlot, srcBag, dstBag);
            client.Character.ItemSWAP(srcBag, srcSlot, dstBag, dstSlot);
        }
    }

    public void On_CMSG_SPLIT_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 10)
        {
            packet.GetInt16();
            var srcBag = packet.GetInt8();
            var srcSlot = packet.GetInt8();
            var dstBag = packet.GetInt8();
            var dstSlot = packet.GetInt8();
            var count = packet.GetInt8();
            if (dstBag == byte.MaxValue)
            {
                dstBag = 0;
            }
            if (srcBag == byte.MaxValue)
            {
                srcBag = 0;
            }
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SPLIT_ITEM [srcSlot={3}:{2}, dstBag={5}:{4}, count={6}]", client.IP, client.Port, srcSlot, srcBag, dstSlot, dstBag, count);
            if ((srcBag != dstBag || srcSlot != dstSlot) && count > 0)
            {
                client.Character.ItemSPLIT(srcBag, srcSlot, dstBag, dstSlot, count);
            }
        }
    }

    public void On_CMSG_READ_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) < 7)
        {
            return;
        }
        packet.GetInt16();
        var srcBag = packet.GetInt8();
        var srcSlot = packet.GetInt8();
        if (srcBag == byte.MaxValue)
        {
            srcBag = 0;
        }
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_READ_ITEM [srcSlot={3}:{2}]", client.IP, client.Port, srcSlot, srcBag);
        short opcode = 175;
        var guid = 0uL;
        if (srcBag == 0)
        {
            if (client.Character.Items.ContainsKey(srcSlot))
            {
                opcode = 174;
                if (client.Character.Items[srcSlot].ItemInfo.PageText > 0)
                {
                    guid = client.Character.Items[srcSlot].GUID;
                }
            }
        }
        else if (client.Character.Items.ContainsKey(srcBag) && client.Character.Items[srcBag].Items.ContainsKey(srcSlot))
        {
            opcode = 174;
            if (client.Character.Items[srcBag].Items[srcSlot].ItemInfo.PageText > 0)
            {
                guid = client.Character.Items[srcBag].Items[srcSlot].GUID;
            }
        }
        if (decimal.Compare(new decimal(guid), 0m) != 0)
        {
            Packets.PacketClass response = new((Opcodes)opcode);
            response.AddUInt64(guid);
            client.Send(ref response);
            response.Dispose();
        }
    }

    public void On_CMSG_PAGE_TEXT_QUERY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 17)
        {
            packet.GetInt16();
            var pageID = packet.GetInt32();
            var itemGuid = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_PAGE_TEXT_QUERY [pageID={2}, itemGuid={3:X}]", client.IP, client.Port, pageID, itemGuid);
            DataTable mySqlQuery = new();
            WorldServiceLocator._WorldServer.WorldDatabase.Query($"SELECT * FROM page_text WHERE entry = \"{pageID}\";", ref mySqlQuery);
            Packets.PacketClass response = new(Opcodes.SMSG_PAGE_TEXT_QUERY_RESPONSE);
            response.AddInt32(pageID);
            if (mySqlQuery.Rows.Count != 0)
            {
                response.AddString(mySqlQuery.Rows[0].As<string>("text"));
            }
            else
            {
                response.AddString("Page " + Conversions.ToString(pageID) + " not found! Please report this to database devs.");
            }
            if (mySqlQuery.Rows.Count != 0)
            {
                response.AddInt32(mySqlQuery.Rows[0].As<int>("next_page"));
            }
            else
            {
                response.AddInt32(0);
            }
            client.Send(ref response);
            response.Dispose();
        }
    }

    public void On_CMSG_WRAP_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 9)
        {
            packet.GetInt16();
            var giftBag = packet.GetInt8();
            var giftSlot = packet.GetInt8();
            var itemBag = packet.GetInt8();
            var itemSlot = packet.GetInt8();
            if (giftBag == byte.MaxValue)
            {
                giftBag = 0;
            }
            if (itemBag == byte.MaxValue)
            {
                itemBag = 0;
            }
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_WRAP_ITEM [{2}:{3} -> {4}{5}]", client.IP, client.Port, giftBag, giftSlot, itemBag, itemSlot);
            var gift = client.Character.ItemGET(giftBag, giftSlot);
            var item = client.Character.ItemGET(itemBag, itemSlot);
            if (gift == null || item == null)
            {
                SendInventoryChangeFailure(ref client.Character, InventoryChangeFailure.EQUIP_ERR_ITEM_NOT_FOUND, 0uL, 0uL);
            }
        }
    }

    public void On_CMSG_DESTROYITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        checked
        {
            if (packet.Data.Length - 1 < 8)
            {
                return;
            }
            try
            {
                packet.GetInt16();
                var srcBag = packet.GetInt8();
                var srcSlot = packet.GetInt8();
                var count = packet.GetInt8();
                if (srcBag == byte.MaxValue)
                {
                    srcBag = 0;
                }
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_DESTROYITEM [srcSlot={3}:{2}  count={4}]", client.IP, client.Port, srcSlot, srcBag, count);
                if (srcBag == 0)
                {
                    if (!client.Character.Items.ContainsKey(srcSlot))
                    {
                        return;
                    }
                    WorldServiceLocator._WorldServer.ALLQUESTS.OnQuestItemRemove(ref client.Character, client.Character.Items[srcSlot].ItemEntry, count);
                    if ((count == 0) | (count >= client.Character.Items[srcSlot].StackCount))
                    {
                        if (srcSlot < 23u)
                        {
                            var character = client.Character;
                            Dictionary<byte, ItemObject> items;
                            byte key;
                            var Item = (items = client.Character.Items)[key = srcSlot];
                            character.UpdateRemoveItemStats(ref Item, srcSlot);
                            items[key] = Item;
                        }
                        client.Character.ItemREMOVE(srcBag, srcSlot, Destroy: true, Update: true);
                    }
                    else
                    {
                        client.Character.Items[srcSlot].StackCount -= count;
                        client.Character.SendItemUpdate(client.Character.Items[srcSlot]);
                        client.Character.Items[srcSlot].Save();
                    }
                    return;
                }
                if (client.Character.Items.ContainsKey(srcBag) && client.Character.Items[srcBag].Items.ContainsKey(srcSlot))
                {
                    WorldServiceLocator._WorldServer.ALLQUESTS.OnQuestItemRemove(ref client.Character, client.Character.Items[srcBag].Items[srcSlot].ItemEntry, count);
                    if ((count == 0) | (count >= client.Character.Items[srcBag].Items[srcSlot].StackCount))
                    {
                        client.Character.ItemREMOVE(srcBag, srcSlot, Destroy: true, Update: true);
                        return;
                    }
                    client.Character.Items[srcBag].Items[srcSlot].StackCount -= count;
                    client.Character.SendItemUpdate(client.Character.Items[srcBag].Items[srcSlot]);
                    client.Character.Items[srcBag].Items[srcSlot].Save();
                }
            }
            catch (Exception ex)
            {
                ProjectData.SetProjectError(ex);
                var e = ex;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Error destroying item.{0}", Environment.NewLine + e);
                ProjectData.ClearProjectError();
            }
        }
    }

    public void On_CMSG_USE_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        try
        {
            if (checked(packet.Data.Length - 1) < 9)
            {
                return;
            }
            packet.GetInt16();
            var bag = packet.GetInt8();
            if (bag == byte.MaxValue)
            {
                bag = 0;
            }
            var slot = packet.GetInt8();
            var tmp3 = packet.GetInt8();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_USE_ITEM [bag={2} slot={3} tmp3={4}]", client.IP, client.Port, bag, slot, tmp3);
            if (((uint)client.Character.cUnitFlags & 0x100000u) != 0)
            {
                return;
            }
            var itemGuid = client.Character.ItemGetGUID(bag, slot);
            if (!WorldServiceLocator._WorldServer.WORLD_ITEMs.ContainsKey(itemGuid))
            {
                SendInventoryChangeFailure(ref client.Character, InventoryChangeFailure.EQUIP_ERR_ITEM_NOT_FOUND, 0uL, 0uL);
                return;
            }
            var itemInfo = WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid].ItemInfo;
            var InstantCast = false;
            byte j = 0;
            do
            {
                if (WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(itemInfo.Spells[j].SpellID) && (client.Character.cUnitFlags & 0x80000) == 524288 && ((uint)WorldServiceLocator._WS_Spells.SPELLs[itemInfo.Spells[j].SpellID].Attributes & 0x10000000u) != 0)
                {
                    SendInventoryChangeFailure(ref client.Character, InventoryChangeFailure.EQUIP_ERR_CANT_DO_IN_COMBAT, itemGuid, 0uL);
                    return;
                }
                checked
                {
                    j = (byte)unchecked((uint)(j + 1));
                }
            }
            while (j <= 4u);
            if (client.Character.DEAD)
            {
                SendInventoryChangeFailure(ref client.Character, InventoryChangeFailure.EQUIP_ERR_YOU_ARE_DEAD, itemGuid, 0uL);
                return;
            }
            if (itemInfo.ObjectClass != 0 && WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid].ItemInfo.Bonding == 3 && !WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid].IsSoulBound)
            {
                WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid].SoulbindItem(client);
            }
            WS_Spells.SpellTargets targets = new();
            var spellTargets = targets;
            ref var character = ref client.Character;
            ref var reference = ref character;
            WS_Base.BaseObject Caster = character;
            spellTargets.ReadTargets(ref packet, ref Caster);
            reference = (WS_PlayerData.CharacterObject)Caster;
            byte i = 0;
            do
            {
                if (itemInfo.Spells[i].SpellID > 0 && (itemInfo.Spells[i].SpellTrigger == ITEM_SPELLTRIGGER_TYPE.USE || itemInfo.Spells[i].SpellTrigger == ITEM_SPELLTRIGGER_TYPE.NO_DELAY_USE) && WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(itemInfo.Spells[i].SpellID))
                {
                    if (itemInfo.Spells[i].SpellCharges > 0 && WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid].ChargesLeft == 0)
                    {
                        WorldServiceLocator._WS_Spells.SendCastResult(SpellFailedReason.SPELL_FAILED_NO_CHARGES_REMAIN, ref client, itemInfo.Spells[i].SpellID);
                        break;
                    }
                    ref var character2 = ref client.Character;
                    reference = ref character2;
                    Caster = character2;
                    var spellID = itemInfo.Spells[i].SpellID;
                    Dictionary<ulong, ItemObject> wORLD_ITEMs;
                    ulong key;
                    var Item = (wORLD_ITEMs = WorldServiceLocator._WorldServer.WORLD_ITEMs)[key = itemGuid];
                    WS_Spells.CastSpellParameters castSpellParameters = new(ref targets, ref Caster, spellID, ref Item, InstantCast);
                    wORLD_ITEMs[key] = Item;
                    reference = (WS_PlayerData.CharacterObject)Caster;
                    var tmpSpell = castSpellParameters;
                    var castResult = byte.MaxValue;
                    try
                    {
                        castResult = (byte)WorldServiceLocator._WS_Spells.SPELLs[itemInfo.Spells[i].SpellID].CanCast(ref client.Character, targets, FirstCheck: true);
                        if (castResult == byte.MaxValue)
                        {
                            ThreadPool.QueueUserWorkItem(tmpSpell.Cast);
                        }
                        else
                        {
                            WorldServiceLocator._WS_Spells.SendCastResult((SpellFailedReason)castResult, ref client, itemInfo.Spells[i].SpellID);
                        }
                    }
                    catch (Exception ex2)
                    {
                        ProjectData.SetProjectError(ex2);
                        var e = ex2;
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Error casting spell {0}.{1}", itemInfo.Spells[i].SpellID, Environment.NewLine + e);
                        WorldServiceLocator._WS_Spells.SendCastResult((SpellFailedReason)castResult, ref client, itemInfo.Spells[i].SpellID);
                        ProjectData.ClearProjectError();
                    }
                    break;
                }
                checked
                {
                    i = (byte)unchecked((uint)(i + 1));
                }
            }
            while (i <= 4u);
        }
        catch (Exception ex3)
        {
            ProjectData.SetProjectError(ex3);
            var ex = ex3;
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error while using a item.{0}", Environment.NewLine + ex);
            ProjectData.ClearProjectError();
        }
    }

    public void On_CMSG_OPEN_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) < 7)
        {
            return;
        }
        packet.GetInt16();
        var bag = packet.GetInt8();
        if (bag == byte.MaxValue)
        {
            bag = 0;
        }
        var slot = packet.GetInt8();
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_OPEN_ITEM [bag={2} slot={3}]", client.IP, client.Port, bag, slot);
        var itemGuid = (bag != 0) ? client.Character.Items[bag].Items[slot].GUID : client.Character.Items[slot].GUID;
        if (decimal.Compare(new decimal(itemGuid), 0m) != 0 && WorldServiceLocator._WorldServer.WORLD_ITEMs.ContainsKey(itemGuid))
        {
            if (WorldServiceLocator._WorldServer.WORLD_ITEMs[itemGuid].GenerateLoot())
            {
                WorldServiceLocator._WS_Loot.LootTable[itemGuid].SendLoot(ref client);
            }
            else
            {
                WorldServiceLocator._WS_Loot.SendEmptyLoot(itemGuid, LootType.LOOTTYPE_CORPSE, ref client);
            }
        }
    }

    public void SendInventoryChangeFailure(ref WS_PlayerData.CharacterObject objCharacter, InventoryChangeFailure errorCode, ulong guid1, ulong guid2)
    {
        Packets.PacketClass packet = new(Opcodes.SMSG_INVENTORY_CHANGE_FAILURE);
        packet.AddInt8((byte)errorCode);
        if (errorCode == InventoryChangeFailure.EQUIP_ERR_YOU_MUST_REACH_LEVEL_N)
        {
            packet.AddInt32(WorldServiceLocator._WorldServer.WORLD_ITEMs[guid1].ItemInfo.ReqLevel);
        }
        packet.AddUInt64(guid1);
        packet.AddUInt64(guid2);
        packet.AddInt8(0);
        objCharacter.client.Send(ref packet);
        packet.Dispose();
    }
}
