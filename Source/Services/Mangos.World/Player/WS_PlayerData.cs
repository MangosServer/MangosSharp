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

using Mangos.Common.Enums.Chat;
using Mangos.Common.Enums.Faction;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Group;
using Mangos.Common.Enums.Item;
using Mangos.Common.Enums.Misc;
using Mangos.Common.Enums.Player;
using Mangos.Common.Enums.Spell;
using Mangos.Common.Globals;
using Mangos.Common.Legacy;
using Mangos.World.AI;
using Mangos.World.Globals;
using Mangos.World.Handlers;
using Mangos.World.Maps;
using Mangos.World.Network;
using Mangos.World.Objects;
using Mangos.World.Quests;
using Mangos.World.Social;
using Mangos.World.Spells;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace Mangos.World.Player;

public class WS_PlayerData
{
    public class CharacterObject : WS_Base.BaseUnit, IDisposable
    {
        public WS_Network.ClientClass client;

        public AccessLevel Access;

        public Timer LogoutTimer;

        public bool FullyLoggedIn;

        public uint LoginMap;

        public ulong LoginTransport;

        public ulong TargetGUID;

        public int Model_Native;

        public PlayerFlags cPlayerFlags;

        public int cPlayerBytes;

        public int cPlayerBytes2;

        public int cPlayerBytes3;

        public int cPlayerFieldBytes;

        public int cPlayerFieldBytes2;

        public WS_PlayerHelper.TStatBar Rage;

        public WS_PlayerHelper.TStatBar Energy;

        public WS_PlayerHelper.TStat Strength;

        public WS_PlayerHelper.TStat Agility;

        public WS_PlayerHelper.TStat Stamina;

        public WS_PlayerHelper.TStat Intellect;

        public WS_PlayerHelper.TStat Spirit;

        public short Faction;

        public WS_Combat.TAttackTimer attackState;

        public WS_Base.BaseObject attackSelection;

        public SHEATHE_SLOT attackSheathState;

        public bool Disarmed;

        public int MenuNumber;

        public WS_Spells.CastSpellParameters[] spellCasted;

        public byte spellCastManaRegeneration;

        public bool spellCanDualWeild;

        public WS_PlayerHelper.TDamageBonus healing;

        public WS_PlayerHelper.TDamageBonus[] spellDamage;

        public int spellCriticalRating;

        public bool combatCanDualWield;

        public int combatBlock;

        public int combatBlockValue;

        public int combatParry;

        public int combatCrit;

        public int combatDodge;

        public WS_Items.TDamage Damage;

        public WS_Items.TDamage RangedDamage;

        public WS_Items.TDamage OffHandDamage;

        public short[] AttackTimeBase;

        public float[] AttackTimeMods;

        public float ManaRegenerationModifier;

        public float LifeRegenerationModifier;

        public int ManaRegenBonus;

        public float ManaRegenPercent;

        public int ManaRegen;

        public int ManaRegenInterrupt;

        public int LifeRegenBonus;

        public int RageRegenBonus;

        public LANGUAGES Spell_Language;

        public WS_Pets.PetObject Pet;

        public int HonorPoints;

        public int StandingLastWeek;

        public int HonorKillsLifeTime;

        public int DishonorKillsLifeTime;

        public int HonorPointsLastWeek;

        public int HonorPointsThisWeek;

        public int HonorPointsYesterday;

        public int HonorKillsLastWeek;

        public int HonorKillsThisWeek;

        public short HonorKillsYesterday;

        public short HonorKillsToday;

        public short DishonorKillsToday;

        public uint Copper;

        public string Name;

        public Dictionary<byte, WS_PlayerHelper.TActionButton> ActionButtons;

        public BitArray TaxiZones;

        public Queue<int> TaxiNodes;

        public uint[] ZonesExplored;

        public float WalkSpeed;

        public float RunSpeed;

        public float RunBackSpeed;

        public float SwimSpeed;

        public float SwimBackSpeed;

        public float TurnRate;

        public int charMovementFlags;

        public int ZoneID;

        public int AreaID;

        public float bindpoint_positionX;

        public float bindpoint_positionY;

        public float bindpoint_positionZ;

        public int bindpoint_map_id;

        public int bindpoint_zone_id;

        public bool DEAD;

        public bool exploreCheckQueued_;

        public bool outsideMapID_;

        public int antiHackSpeedChanged_;

        public WS_PlayerHelper.TDrowningTimer underWaterTimer;

        public bool underWaterBreathing;

        public ulong lootGUID;

        public WS_PlayerHelper.TRepopTimer repopTimer;

        public WS_Handlers_Trade.TTradeInfo tradeInfo;

        public ulong corpseGUID;

        public int corpseMapID;

        public CorpseType corpseCorpseType;

        public float corpsePositionX;

        public float corpsePositionY;

        public float corpsePositionZ;

        public ulong resurrectGUID;

        public int resurrectMapID;

        public float resurrectPositionX;

        public float resurrectPositionY;

        public float resurrectPositionZ;

        public int resurrectHealth;

        public int resurrectMana;

        public ReaderWriterLock guidsForRemoving_Lock;

        public List<ulong> guidsForRemoving;

        public List<ulong> creaturesNear;

        public List<ulong> playersNear;

        public List<ulong> gameObjectsNear;

        public List<ulong> dynamicObjectsNear;

        public List<ulong> corpseObjectsNear;

        public List<ulong> inCombatWith;

        public int lastPvpAction;

        public byte[] TutorialFlags;

        private readonly BitArray UpdateMask;

        private readonly Hashtable UpdateData;

        public byte TalentPoints;

        public int AmmoID;

        public float AmmoDPS;

        public float AmmoMod;

        public int AutoShotSpell;

        public WS_Creatures.CreatureObject NonCombatPet;

        public ulong[] TotemSlot;

        public Dictionary<int, WS_PlayerHelper.TSkill> Skills;

        public Dictionary<int, short> SkillsPositions;

        public Dictionary<int, WS_Spells.CharacterSpell> Spells;

        public WS_Base.BaseUnit MindControl;

        public int RestBonus;

        public int XP;

        public Dictionary<byte, ItemObject> Items;

        public int[] BuyBackTimeStamp;

        public byte WatchedFactionIndex;

        public WS_PlayerHelper.TReputation[] Reputation;

        private bool _disposedValue;

        public WS_Group.Group Group;

        public uint GroupUpdateFlag;

        public uint GuildID;

        public byte GuildRank;

        public int GuildInvited;

        public int GuildInvitedBy;

        public ulong DuelArbiter;

        public CharacterObject DuelPartner;

        public byte DuelOutOfBounds;

        public ArrayList TalkMenuTypes;

        public WS_QuestsBase[] TalkQuests;

        public List<int> QuestsCompleted;

        public WS_QuestInfo TalkCurrentQuest;

        public WS_Handlers_Warden.WardenData WardenData;

        public byte HairColor
        {
            get => checked((byte)((cPlayerBytes & -16777216) >> 24));
            set => cPlayerBytes = (cPlayerBytes & 0xFFFFFF) | (value << 24);
        }

        public byte HairStyle
        {
            get => checked((byte)((cPlayerBytes & 0xFF0000) >> 16));
            set => cPlayerBytes = (cPlayerBytes & -16711681) | (value << 16);
        }

        public byte Face
        {
            get => checked((byte)((cPlayerBytes & 0xFF00) >> 8));
            set => cPlayerBytes = (cPlayerBytes & -65281) | (value << 8);
        }

        public byte Skin
        {
            get => checked((byte)((cPlayerBytes & 0xFF) >> 0));
            set => cPlayerBytes = (cPlayerBytes & -256) | (value << 0);
        }

        public XPSTATE RestState
        {
            get => (XPSTATE)checked((byte)((cPlayerBytes2 & -16777216) >> 24));
            set => cPlayerBytes2 = (cPlayerBytes2 & 0xFFFFFF) | (int)((uint)value << 24);
        }

        public byte Items_AvailableBankSlots
        {
            get => checked((byte)((cPlayerBytes2 & 0xFF0000) >> 16));
            set => cPlayerBytes2 = (cPlayerBytes2 & -16711681) | (value << 16);
        }

        public byte FacialHair
        {
            get => checked((byte)((cPlayerBytes2 & 0xFF) >> 0));
            set => cPlayerBytes2 = (cPlayerBytes2 & -256) | (value << 0);
        }

        public override Genders Gender
        {
            get => (Genders)checked((byte)((cBytes0 & 0xFF0000) >> 16));
            set
            {
                cBytes0 = (cBytes0 & -16711681) | (int)((uint)value << 16);
                cPlayerBytes3 = (cPlayerBytes3 & -256) | (int)((uint)value << 0);
            }
        }

        public PlayerHonorRank HonorRank
        {
            get => (PlayerHonorRank)checked((byte)((cPlayerBytes3 & -16777216) >> 24));
            set => cPlayerBytes3 = (cPlayerBytes3 & 0xFFFFFF) | (int)((uint)value << 24);
        }

        public PlayerHonorRank HonorHighestRank
        {
            get => (PlayerHonorRank)checked((byte)((cPlayerFieldBytes & -16777216) >> 24));
            set => cPlayerFieldBytes = (cPlayerFieldBytes & 0xFFFFFF) | (int)((uint)value << 24);
        }

        public byte HonorBar
        {
            get => checked((byte)((cPlayerFieldBytes2 & 0xFF) >> 0));
            set => cPlayerFieldBytes2 = (cPlayerFieldBytes2 & -256) | (value << 0);
        }

        public WS_Base.BaseUnit GetTarget
        {
            get
            {
                if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(TargetGUID))
                {
                    return WorldServiceLocator._WorldServer.WORLD_CREATUREs[TargetGUID];
                }
                if (WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(TargetGUID))
                {
                    return WorldServiceLocator._WorldServer.CHARACTERs[TargetGUID];
                }
                return WorldServiceLocator._CommonGlobalFunctions.GuidIsPet(TargetGUID)
                    ? WorldServiceLocator._WorldServer.WORLD_CREATUREs[TargetGUID]
                    : null;
            }
        }

        public bool CanShootRanged => AmmoID > 0 && Items.ContainsKey(17) && Items[17].IsRanged && !Items[17].IsBroken() && ItemCOUNT(AmmoID) > 0;

        public float GetRageConversion => (float)((0.0091107836 * Level * Level) + (3.225598133 * Level) + 4.2652911);

        public float GetHitFactor(bool MainHand = true, bool Critical = false)
        {
            var HitFactor = 1.75f;
            if (MainHand)
            {
                HitFactor *= 2f;
            }
            if (Critical)
            {
                HitFactor *= 2f;
            }
            return HitFactor;
        }

        public float GetCriticalWithSpells => Classe switch
        {
            Classes.CLASS_DRUID => (float)Conversion.Fix((Intellect.Base / 80.0) + 1.8500000238418579),
            Classes.CLASS_MAGE => (float)Conversion.Fix((Intellect.Base / 80.0) + 0.9100000262260437),
            Classes.CLASS_PRIEST => (float)Conversion.Fix((Intellect.Base / 80.0) + 1.2400000095367432),
            Classes.CLASS_WARLOCK => (float)Conversion.Fix((Intellect.Base / 82.0) + 1.7009999752044678),
            Classes.CLASS_PALADIN => (float)Conversion.Fix((Intellect.Base / 80.0) + 3.3359999656677246),
            Classes.CLASS_SHAMAN => (float)Conversion.Fix((Intellect.Base / 80.0) + 2.2000000476837158),
            Classes.CLASS_HUNTER => (float)Conversion.Fix((Intellect.Base / 80.0) + 3.5999999046325684),
            _ => 0f,
        };

        public int BaseUnarmedDamage => checked((int)Math.Round((AttackPower + AttackPowerMods) * 0.071428571428571425));

        public int BaseRangedDamage => checked((int)Math.Round((AttackPowerRanged + AttackPowerModsRanged) * 0.071428571428571425));

        public int AttackPower
        {
            get
            {
                checked
                {
                    switch (Classe)
                    {
                        case Classes.CLASS_WARRIOR:
                        case Classes.CLASS_PALADIN:
                            return (Level * 3) + (Strength.Base * 3) - 20;

                        case Classes.CLASS_SHAMAN:
                            return (Level * 2) + (Strength.Base * 2) - 20;

                        case Classes.CLASS_PRIEST:
                        case Classes.CLASS_MAGE:
                        case Classes.CLASS_WARLOCK:
                            return Strength.Base - 10;

                        case Classes.CLASS_HUNTER:
                        case Classes.CLASS_ROGUE:
                            return (Level * 2) + Strength.Base + Agility.Base - 20;

                        case Classes.CLASS_DRUID:
                            if (ShapeshiftForm == ShapeshiftForm.FORM_CAT)
                            {
                                return (Level * 2) + (Strength.Base * 2) + Agility.Base - 20;
                            }
                            if ((ShapeshiftForm == ShapeshiftForm.FORM_BEAR) | (ShapeshiftForm == ShapeshiftForm.FORM_DIREBEAR))
                            {
                                return (Level * 3) + (Strength.Base * 2) - 20;
                            }
                            if (ShapeshiftForm == ShapeshiftForm.FORM_MOONKIN)
                            {
                                return (int)Math.Round((Level * 1.5) + Agility.Base + (Strength.Base * 2) - 20.0);
                            }
                            return (Strength.Base * 2) - 20;

                        default:
                            return 0;
                    }
                }
            }
        }

        public int AttackPowerRanged
        {
            get
            {
                checked
                {
                    switch (Classe)
                    {
                        case Classes.CLASS_WARRIOR:
                        case Classes.CLASS_ROGUE:
                            return Level + Agility.Base - 10;

                        case Classes.CLASS_HUNTER:
                            return (Level * 2) + Agility.Base - 10;

                        case Classes.CLASS_PALADIN:
                        case Classes.CLASS_PRIEST:
                        case Classes.CLASS_SHAMAN:
                        case Classes.CLASS_MAGE:
                        case Classes.CLASS_WARLOCK:
                            return Agility.Base - 10;

                        case Classes.CLASS_DRUID:
                            if ((ShapeshiftForm == ShapeshiftForm.FORM_CAT) | (ShapeshiftForm == ShapeshiftForm.FORM_BEAR) | (ShapeshiftForm == ShapeshiftForm.FORM_DIREBEAR) | (ShapeshiftForm == ShapeshiftForm.FORM_MOONKIN))
                            {
                                return 0;
                            }
                            return Agility.Base - 10;

                        default:
                            return 0;
                    }
                }
            }
        }

        public short GetAttackTime(WeaponAttackType weaponAttackType)
        {
            checked
            {
                return (short)(AttackTimeBase[(uint)weaponAttackType] * AttackTimeMods[(uint)weaponAttackType]);
            }
        }

        public uint ClassMask
        {
            get
            {
                checked
                {
                    return (uint)(1 << ((int)Classe - 1));
                }
            }
        }

        public uint RaceMask
        {
            get
            {
                checked
                {
                    return (uint)(1 << ((int)Race - 1));
                }
            }
        }

        public override bool IsDead => DEAD;

        public bool isMoving => (WorldServiceLocator._Global_Constants.movementFlagsMask & charMovementFlags) != 0;

        public bool isTurning => (WorldServiceLocator._Global_Constants.TurningFlagsMask & charMovementFlags) != 0;

        public bool isMovingOrTurning => (WorldServiceLocator._Global_Constants.movementOrTurningFlagsMask & charMovementFlags) != 0;

        public bool isPvP
        {
            get => (cUnitFlags & 0x1000) != 0;
            set
            {
                if (value)
                {
                    cUnitFlags |= 4096;
                }
                else
                {
                    cUnitFlags &= -4097;
                }
            }
        }

        public bool isResting => (cPlayerFlags & PlayerFlags.PLAYER_FLAGS_RESTING) != 0;

        public bool IsInCombat => inCombatWith.Count > 0 || checked(WorldServiceLocator._NativeMethods.timeGetTime("") - lastPvpAction) < WorldServiceLocator._Global_Constants.DEFAULT_PVP_COMBAT_TIME;

        public bool AFK
        {
            get => (cPlayerFlags & PlayerFlags.PLAYER_FLAGS_AFK) != 0;
            set
            {
                if (value)
                {
                    cPlayerFlags |= PlayerFlags.PLAYER_FLAGS_AFK;
                    WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.ClientSetChatFlag(client.Index, 1);
                }
                else
                {
                    cPlayerFlags &= ~PlayerFlags.PLAYER_FLAGS_AFK;
                    WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.ClientSetChatFlag(client.Index, 0);
                }
            }
        }

        public bool DND
        {
            get => (cPlayerFlags & PlayerFlags.PLAYER_FLAGS_DND) != 0;
            set
            {
                if (value)
                {
                    cPlayerFlags |= PlayerFlags.PLAYER_FLAGS_DND;
                    WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.ClientSetChatFlag(client.Index, 2);
                }
                else
                {
                    cPlayerFlags &= ~PlayerFlags.PLAYER_FLAGS_DND;
                    WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.ClientSetChatFlag(client.Index, 0);
                }
            }
        }

        public bool GM
        {
            get => (cPlayerFlags & PlayerFlags.PLAYER_FLAGS_GM) != 0;
            set
            {
                if (value)
                {
                    cPlayerFlags |= PlayerFlags.PLAYER_FLAGS_GM;
                    WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.ClientSetChatFlag(client.Index, 3);
                }
                else
                {
                    cPlayerFlags &= ~PlayerFlags.PLAYER_FLAGS_GM;
                    WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.ClientSetChatFlag(client.Index, 0);
                }
            }
        }

        public bool IsInGroup => Group != null;

        public bool IsInRaid => Group != null && Group.Type == GroupType.RAID;

        public bool IsGroupLeader => Group != null && Group.Leader == GUID;

        public bool IsInGuild => (ulong)GuildID != 0;

        public bool IsInDuel => DuelPartner != null;

        public bool IsHorde => Race switch
        {
            Races.RACE_HUMAN or Races.RACE_DWARF or Races.RACE_NIGHT_ELF or Races.RACE_GNOME => false,
            _ => true,
        };

        public bool IsAlliance => Race switch
        {
            Races.RACE_ORC or Races.RACE_UNDEAD or Races.RACE_TAUREN or Races.RACE_TROLL => false,
            _ => true,
        };

        public int Team => Race switch
        {
            Races.RACE_HUMAN or Races.RACE_DWARF or Races.RACE_NIGHT_ELF or Races.RACE_GNOME => 469,
            _ => 67,
        };

        public short GetStat(byte Type)
        {
            return checked(Type switch
            {
                0 => (short)Strength.Base,
                1 => (short)Agility.Base,
                2 => (short)Stamina.Base,
                3 => (short)Intellect.Base,
                4 => (short)Spirit.Base,
                _ => 0,
            });
        }

        public void UpdateManaRegen()
        {
            if (!FullyLoggedIn)
            {
                return;
            }
            var PowerRegen = (float)(Math.Sqrt(Intellect.Base) * 1.0);
            if (float.IsNaN(PowerRegen))
            {
                PowerRegen = 1f;
            }
            PowerRegen *= ManaRegenPercent;
            var PowerRegenMP5 = (float)(ManaRegenBonus / 5.0);
            var PowerRegenInterrupt = 0;
            checked
            {
                var num = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs - 1;
                for (var i = 0; i <= num; i++)
                {
                    if (ActiveSpells[i] == null)
                    {
                        continue;
                    }
                    byte j = 0;
                    do
                    {
                        if (ActiveSpells[i].Aura_Info[j] != null)
                        {
                            if (ActiveSpells[i].Aura_Info[j].ApplyAuraIndex == 219)
                            {
                                PowerRegenMP5 += GetStat((byte)ActiveSpells[i].Aura_Info[j].MiscValue) * ActiveSpells[i].Aura_Info[j].GetValue(Level, 0) / 500f;
                            }
                            else if (ActiveSpells[i].SpellID == 34074 && ActiveSpells[i].Aura_Info[j].ApplyAuraIndex == 226)
                            {
                                PowerRegenMP5 = (float)(PowerRegenMP5 + ((ActiveSpells[i].Aura_Info[j].GetValue(Level, 0) * Intellect.Base / 500f) + (Level * 35 / 100.0)));
                            }
                            else if (ActiveSpells[i].Aura_Info[j].ApplyAuraIndex == 134)
                            {
                                PowerRegenInterrupt += ActiveSpells[i].Aura_Info[j].GetValue(Level, 0);
                            }
                        }
                        j = (byte)unchecked((uint)(j + 1));
                    }
                    while (j <= 2u);
                }
                if (PowerRegenInterrupt > 100)
                {
                    PowerRegenInterrupt = 100;
                }
                PowerRegenInterrupt = (int)Math.Round(PowerRegenMP5 + (PowerRegen * PowerRegenInterrupt / 100f));
                PowerRegen = (int)Math.Round(PowerRegenMP5 + PowerRegen);
                ManaRegen = (int)Math.Round(PowerRegen);
                ManaRegenInterrupt = PowerRegenInterrupt;
            }
        }

        public void HonorSaveAsNew()
        {
            WorldServiceLocator._WorldServer.CharacterDatabase.Update("INSERT INTO characters_honor (char_guid)  VALUES (" + Conversions.ToString(GUID) + ");");
        }

        public void HonorSave()
        {
            var honor = "UPDATE characters_honor SET";
            honor = honor + ", honor_points =" + Conversions.ToString(HonorPoints);
            honor = honor + ", kills_honor =" + Conversions.ToString(HonorKillsLifeTime);
            honor = honor + ", kills_dishonor =" + Conversions.ToString(DishonorKillsLifeTime);
            honor = honor + ", honor_yesterday =" + Conversions.ToString(HonorPointsYesterday);
            honor = honor + ", honor_thisWeek =" + Conversions.ToString(HonorPointsThisWeek);
            honor = honor + ", kills_thisWeek =" + Conversions.ToString(HonorKillsThisWeek);
            honor = honor + ", kills_today =" + Conversions.ToString((int)HonorKillsToday);
            honor = honor + ", kills_dishonortoday =" + Conversions.ToString((int)DishonorKillsToday);
            honor += $" WHERE char_guid = \"{GUID}\";";
            WorldServiceLocator._WorldServer.CharacterDatabase.Update(honor);
        }

        public void HonorLoad()
        {
            DataTable MySQLQuery = new();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT * FROM characters_honor WHERE char_guid = {GUID};", ref MySQLQuery);
            if (MySQLQuery.Rows.Count == 0)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Unable to get SQLDataBase honor info for character [GUID={0:X}]", GUID);
                return;
            }
            HonorPoints = MySQLQuery.Rows[0].As<int>("honor_points");
            HonorRank = (PlayerHonorRank)MySQLQuery.Rows[0].As<byte>("honor_rank");
            HonorHighestRank = (PlayerHonorRank)MySQLQuery.Rows[0].As<byte>("honor_hightestRank");
            StandingLastWeek = MySQLQuery.Rows[0].As<int>("standing_lastweek");
            HonorKillsLifeTime = MySQLQuery.Rows[0].As<int>("kills_honor");
            DishonorKillsLifeTime = MySQLQuery.Rows[0].As<int>("kills_dishonor");
            HonorPointsLastWeek = MySQLQuery.Rows[0].As<int>("honor_lastWeek");
            HonorPointsThisWeek = MySQLQuery.Rows[0].As<int>("honor_thisWeek");
            HonorPointsYesterday = MySQLQuery.Rows[0].As<int>("honor_yesterday");
            HonorKillsLastWeek = MySQLQuery.Rows[0].As<int>("kills_lastWeek");
            HonorKillsThisWeek = MySQLQuery.Rows[0].As<int>("kills_thisWeek");
            HonorKillsYesterday = MySQLQuery.Rows[0].As<short>("kills_yesterday");
            HonorKillsToday = MySQLQuery.Rows[0].As<short>("kills_today");
            DishonorKillsToday = MySQLQuery.Rows[0].As<short>("kills_dishonortoday");
            MySQLQuery.Dispose();
        }

        public void HonorLog(int honorPoints, ulong victimGUID, int victimRank)
        {
            Packets.PacketClass packet = new(Opcodes.SMSG_PVP_CREDIT);
            try
            {
                packet.AddInt32(honorPoints);
                packet.AddUInt64(victimGUID);
                packet.AddInt32(victimRank);
                client.Send(ref packet);
            }
            finally
            {
                packet.Dispose();
            }
        }

        public override bool IsFriendlyTo(ref WS_Base.BaseUnit Unit)
        {
            if (Unit == this)
            {
                return true;
            }
            if (Unit is CharacterObject characterObject)
            {
                if (characterObject.GM)
                {
                    return true;
                }
                if (DuelPartner != null && DuelPartner == Unit)
                {
                    return false;
                }
                if (characterObject.DuelPartner != null && characterObject.DuelPartner == this)
                {
                    return false;
                }
                if (IsInGroup && characterObject.IsInGroup && Group == characterObject.Group)
                {
                    return true;
                }
                if (WorldServiceLocator._Functions.HaveFlags((int)cPlayerFlags, 128) && WorldServiceLocator._Functions.HaveFlags((int)characterObject.cPlayerFlags, 128))
                {
                    return false;
                }
                return Team == characterObject.Team || !characterObject.isPvP;
            }
            if (Unit is WS_Creatures.CreatureObject creatureObject)
            {
                if (GetReputation(creatureObject.Faction) < ReputationRank.Friendly)
                {
                    return false;
                }
                if (GetReaction(creatureObject.Faction) < TReaction.NEUTRAL)
                {
                    return false;
                }
            }
            return true;
        }

        public override bool IsEnemyTo(ref WS_Base.BaseUnit Unit)
        {
            if (Unit == this)
            {
                return false;
            }
            if (Unit is CharacterObject characterObject)
            {
                if (characterObject.GM)
                {
                    return false;
                }
                if (DuelPartner != null && DuelPartner == Unit)
                {
                    return true;
                }
                if (characterObject.DuelPartner != null && characterObject.DuelPartner == this)
                {
                    return true;
                }
                if (IsInGroup && characterObject.IsInGroup && Group == characterObject.Group)
                {
                    return false;
                }
                if (WorldServiceLocator._Functions.HaveFlags((int)cPlayerFlags, 128) && WorldServiceLocator._Functions.HaveFlags((int)characterObject.cPlayerFlags, 128))
                {
                    return true;
                }
                return Team != characterObject.Team && characterObject.isPvP;
            }
            if (Unit is WS_Creatures.CreatureObject creatureObject)
            {
                if (GetReputation(creatureObject.Faction) < ReputationRank.Neutral)
                {
                    return true;
                }
                if (GetReaction(creatureObject.Faction) < TReaction.NEUTRAL)
                {
                    return true;
                }
            }
            return false;
        }

        public void AddToCombat(WS_Base.BaseUnit Unit)
        {
            if (Unit is CharacterObject)
            {
                lastPvpAction = WorldServiceLocator._NativeMethods.timeGetTime("");
            }
            else
            {
                if (inCombatWith.Contains(Unit.GUID))
                {
                    return;
                }
                inCombatWith.Add(Unit.GUID);
            }
            CheckCombat();
        }

        public void RemoveFromCombat(WS_Base.BaseUnit Unit)
        {
            if (inCombatWith.Contains(Unit.GUID))
            {
                inCombatWith.Remove(Unit.GUID);
                CheckCombat();
            }
        }

        public void CheckCombat()
        {
            if (((uint)cUnitFlags & 0x80000u) != 0)
            {
                if (!IsInCombat)
                {
                    var wS_Combat = WorldServiceLocator._WS_Combat;
                    var objCharacter = this;
                    wS_Combat.SetPlayerOutOfCombat(ref objCharacter);
                }
            }
            else if (IsInCombat)
            {
                var wS_Combat2 = WorldServiceLocator._WS_Combat;
                var objCharacter = this;
                wS_Combat2.SetPlayerInCombat(ref objCharacter);
            }
        }

        public override bool CanSee(ref WS_Base.BaseObject objCharacter)
        {
            if (GUID == objCharacter.GUID)
            {
                return false;
            }
            if (instance != objCharacter.instance)
            {
                return false;
            }
            if (objCharacter.MapID != MapID)
            {
                return false;
            }
            switch (objCharacter)
            {
                case WS_Creatures.CreatureObject _:
                    if (((WS_Creatures.CreatureObject)objCharacter).aiScript != null && ((WS_Creatures.CreatureObject)objCharacter).aiScript.State == AIState.AI_RESPAWN)
                    {
                        return false;
                    }

                    break;

                case WS_GameObjects.GameObject _ when ((WS_GameObjects.GameObject)objCharacter).Despawned:
                    return false;
            }
            var distance = WorldServiceLocator._WS_Combat.GetDistance(this, objCharacter);
            if (Group != null && objCharacter is CharacterObject @object && @object.Group == Group)
            {
                return distance <= objCharacter.VisibleDistance;
            }
            if (DEAD && corpseGUID != 0)
            {
                if (corpseGUID == objCharacter.GUID)
                {
                    return true;
                }
                if (WorldServiceLocator._WS_Combat.GetDistance(objCharacter, corpsePositionX, corpsePositionY, corpsePositionZ) < objCharacter.VisibleDistance)
                {
                    if (objCharacter.Invisibility > CanSeeInvisibility)
                    {
                        return false;
                    }
                    int num;
                    if (objCharacter.Invisibility == InvisibilityLevel.STEALTH)
                    {
                        WS_Base.BaseUnit objCharacter2 = (WS_Base.BaseUnit)objCharacter;
                        var stealthDistance = GetStealthDistance(ref objCharacter2);
                        objCharacter = objCharacter2;
                        num = (distance < stealthDistance) ? 1 : 0;
                    }
                    else
                    {
                        num = 0;
                    }
                    if (num != 0)
                    {
                        return true;
                    }
                    if (objCharacter.Invisibility == InvisibilityLevel.INIVISIBILITY && objCharacter.Invisibility_Value > CanSeeInvisibility_Invisibility)
                    {
                        return false;
                    }
                    return objCharacter.Invisibility != InvisibilityLevel.STEALTH || CanSeeStealth;
                }
                if (objCharacter.Invisibility != InvisibilityLevel.DEAD)
                {
                    return false;
                }
            }
            else if (Invisibility == InvisibilityLevel.INIVISIBILITY)
            {
                if (objCharacter.Invisibility != InvisibilityLevel.INIVISIBILITY)
                {
                    return objCharacter.CanSeeInvisibility_Invisibility >= Invisibility_Value;
                }
                if (Invisibility_Value < objCharacter.Invisibility_Value)
                {
                    return false;
                }
            }
            else
            {
                if (objCharacter.Invisibility > CanSeeInvisibility)
                {
                    return false;
                }
                int num2;
                if (objCharacter.Invisibility == InvisibilityLevel.STEALTH)
                {
                    WS_Base.BaseUnit objCharacter2 = (WS_Base.BaseUnit)objCharacter;
                    var stealthDistance = GetStealthDistance(ref objCharacter2);
                    objCharacter = objCharacter2;
                    num2 = (distance < stealthDistance) ? 1 : 0;
                }
                else
                {
                    num2 = 0;
                }
                if (num2 != 0)
                {
                    return true;
                }
                if (objCharacter.Invisibility == InvisibilityLevel.INIVISIBILITY && objCharacter.Invisibility_Value > CanSeeInvisibility_Invisibility)
                {
                    return false;
                }
                if (objCharacter.Invisibility == InvisibilityLevel.STEALTH && !CanSeeStealth)
                {
                    return false;
                }
            }
            return distance <= objCharacter.VisibleDistance;
        }

        public void SetUpdateFlag(int pos, int value)
        {
            UpdateMask.Set(pos, value: true);
            UpdateData[pos] = value;
        }

        public void SetUpdateFlag(int pos, uint value)
        {
            UpdateMask.Set(pos, value: true);
            UpdateData[pos] = value;
        }

        public void SetUpdateFlag(int pos, long value)
        {
            UpdateMask.Set(pos, value: true);
            checked
            {
                UpdateMask.Set(pos + 1, value: true);
                UpdateData[pos] = (int)(value & 0xFFFFFFFFu);
                UpdateData[pos + 1] = (int)((value >> 32) & 0xFFFFFFFFu);
            }
        }

        public void SetUpdateFlag(int pos, ulong value)
        {
            UpdateMask.Set(pos, value: true);
            checked
            {
                UpdateMask.Set(pos + 1, value: true);
                UpdateData[pos] = (uint)(value & 0xFFFFFFFFu);
                UpdateData[pos + 1] = (uint)((value >> 32) & 0xFFFFFFFFu);
            }
        }

        public void SetUpdateFlag(int pos, float value)
        {
            UpdateMask.Set(pos, value: true);
            UpdateData[pos] = value;
        }

        public void SendOutOfRangeUpdate()
        {
            guidsForRemoving_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
            var GUIDs = guidsForRemoving.ToArray();
            guidsForRemoving.Clear();
            guidsForRemoving_Lock.ReleaseWriterLock();
            if (GUIDs.Length <= 0)
            {
                return;
            }
            Packets.PacketClass packet = new(Opcodes.SMSG_UPDATE_OBJECT);
            try
            {
                packet.AddInt32(1);
                packet.AddInt8(0);
                packet.AddInt8(4);
                packet.AddInt32(GUIDs.Length);
                var array = GUIDs;
                foreach (var g in array)
                {
                    packet.AddPackGUID(g);
                }
                client.Send(ref packet);
            }
            finally
            {
                packet.Dispose();
            }
        }

        public void SendUpdate()
        {
            checked
            {
                var updateCount = 1 + Items.Count;
                if (OnTransport != null)
                {
                    updateCount++;
                }
                Packets.PacketClass packet = new(Opcodes.SMSG_UPDATE_OBJECT);
                try
                {
                    packet.AddInt32(updateCount);
                    packet.AddInt8(0);
                    if (OnTransport != null)
                    {
                        Packets.UpdateClass tmpUpdate2 = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_GAMEOBJECT);
                        var onTransport = OnTransport;
                        var Character = this;
                        onTransport.FillAllUpdateFlags(ref tmpUpdate2, ref Character);
                        tmpUpdate2.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, ref OnTransport);
                        tmpUpdate2.Dispose();
                        gameObjectsNear.Add(OnTransport.GUID);
                        OnTransport.SeenBy.Add(GUID);
                    }
                    PrepareUpdate(ref packet, 3);
                    foreach (var tmpItem in Items)
                    {
                        Packets.UpdateClass tmpUpdate = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_ITEM);
                        tmpItem.Value.FillAllUpdateFlags(ref tmpUpdate);
                        var updateClass = tmpUpdate;
                        var updateObject = tmpItem.Value;
                        updateClass.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, ref updateObject);
                        tmpUpdate.Dispose();
                        if (tmpItem.Value.ItemInfo.IsContainer)
                        {
                            tmpItem.Value.SendContainedItemsUpdate(ref client);
                        }
                    }
                    packet.CompressUpdatePacket();
                    client.Send(ref packet);
                }
                finally
                {
                    packet.Dispose();
                }
                if (OnTransport is not null and WS_Transports.TransportObject @object)
                {
                    var obj = @object;
                    var Character = this;
                    obj.CreateEveryoneOnTransport(ref Character);
                }
            }
        }

        public void SendItemUpdate(ItemObject Item)
        {
            Packets.PacketClass packet = new(Opcodes.SMSG_UPDATE_OBJECT);
            try
            {
                packet.AddInt32(1);
                packet.AddInt8(0);
                Packets.UpdateClass tmpUpdate = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_ITEM);
                Item.FillAllUpdateFlags(ref tmpUpdate);
                tmpUpdate.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref Item);
                tmpUpdate.Dispose();
                client.Send(ref packet);
            }
            finally
            {
                packet.Dispose();
            }
        }

        public void SendInventoryUpdate()
        {
            Packets.PacketClass packet = new(Opcodes.SMSG_UPDATE_OBJECT);
            try
            {
                packet.AddInt32(1);
                packet.AddInt8(0);
                byte i = 0;
                do
                {
                    checked
                    {
                        if (Items.ContainsKey(i))
                        {
                            SetUpdateFlag(486 + (i * 2), Items[i].GUID);
                            if (i < 19u)
                            {
                                SetUpdateFlag(260 + (i * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE), Items[i].ItemEntry);
                                SetUpdateFlag(268 + (i * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE), 0);
                            }
                        }
                        else
                        {
                            SetUpdateFlag(486 + (i * 2), 0L);
                            if (i < 19u)
                            {
                                SetUpdateFlag(260 + (i * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE), 0);
                            }
                        }
                        i = (byte)unchecked((uint)(i + 1));
                    }
                }
                while (i <= 38u);
                PrepareUpdate(ref packet, 0);
                client.Send(ref packet);
            }
            finally
            {
                packet.Dispose();
            }
        }

        public void SendItemAndCharacterUpdate(ItemObject Item, int UPDATETYPE = 0)
        {
            Packets.PacketClass packet = new(Opcodes.SMSG_UPDATE_OBJECT);
            Packets.UpdateClass tmpUpdate = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_ITEM);
            try
            {
                packet.AddInt32(2);
                packet.AddInt8(0);
                Item.FillAllUpdateFlags(ref tmpUpdate);
                tmpUpdate.AddToPacket(ref packet, (ObjectUpdateType)UPDATETYPE, ref Item);
                byte j = 0;
                do
                {
                    checked
                    {
                        if (Items.ContainsKey(j))
                        {
                            SetUpdateFlag(486 + (j * 2), Items[j].GUID);
                            if (j < 19u)
                            {
                                SetUpdateFlag(260 + (j * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE), Items[j].ItemEntry);
                                SetUpdateFlag(268 + (j * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE), Items[j].RandomProperties);
                            }
                        }
                        else
                        {
                            SetUpdateFlag(486 + (j * 2), 0uL);
                            if (j < 19u)
                            {
                                SetUpdateFlag(260 + (j * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE), 0);
                            }
                        }
                        j = (byte)unchecked((uint)(j + 1));
                    }
                }
                while (j <= 112u);
                PrepareUpdate(ref packet, 0);
                client.Send(ref packet);
            }
            finally
            {
                packet.Dispose();
                tmpUpdate.Dispose();
            }
            byte i = 0;
            do
            {
                checked
                {
                    if (Items.ContainsKey(i))
                    {
                        SetUpdateFlag(260 + (i * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE), Items[i].ItemEntry);
                        SetUpdateFlag(268 + (i * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE), Items[i].RandomProperties);
                    }
                    else
                    {
                        SetUpdateFlag(260 + (i * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE), 0);
                    }
                    i = (byte)unchecked((uint)(i + 1));
                }
            }
            while (i <= 18u);
            SendCharacterUpdate(toNear: true, notMe: true);
        }

        public void SendCharacterUpdate(bool toNear = true, bool notMe = false)
        {
            if (UpdateData.Count == 0)
            {
                return;
            }
            if (toNear && SeenBy.Count > 0)
            {
                Packets.UpdateClass updateClass = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER)
                {
                    UpdateData = (Hashtable)UpdateData.Clone(),
                    UpdateMask = (BitArray)UpdateMask.Clone()
                };
                var forOthers = updateClass;
                Packets.PacketClass packetForOthers = new(Opcodes.SMSG_UPDATE_OBJECT);
                try
                {
                    packetForOthers.AddInt32(1);
                    packetForOthers.AddInt8(0);
                    var updateObject = this;
                    forOthers.AddToPacket(ref packetForOthers, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
                    SendToNearPlayers(ref packetForOthers);
                }
                finally
                {
                    packetForOthers.Dispose();
                }
            }
            if (!notMe && client != null)
            {
                Packets.PacketClass packet = new(Opcodes.SMSG_UPDATE_OBJECT);
                try
                {
                    packet.AddInt32(1);
                    packet.AddInt8(0);
                    PrepareUpdate(ref packet, 0);
                    client.Send(ref packet);
                }
                finally
                {
                    packet.Dispose();
                }
            }
        }

        public void FillStatsUpdateFlags()
        {
            SetUpdateFlag(28, Life.Maximum);
            SetUpdateFlag(29, Mana.Maximum);
            SetUpdateFlag(30, Rage.Maximum);
            SetUpdateFlag(32, Energy.Maximum);
            SetUpdateFlag(33, 0);
            SetUpdateFlag(1106, combatBlock);
            SetUpdateFlag(134, Damage.Minimum);
            SetUpdateFlag(135, Damage.Maximum + BaseUnarmedDamage);
            SetUpdateFlag(136, OffHandDamage.Minimum);
            SetUpdateFlag(137, OffHandDamage.Maximum);
            SetUpdateFlag(171, RangedDamage.Minimum);
            SetUpdateFlag(172, RangedDamage.Maximum + BaseRangedDamage);
            SetUpdateFlag(126, GetAttackTime(WeaponAttackType.BASE_ATTACK));
            SetUpdateFlag(128, GetAttackTime(WeaponAttackType.OFF_ATTACK));
            SetUpdateFlag(128, GetAttackTime(WeaponAttackType.RANGED_ATTACK));
            var wS_Combat = WorldServiceLocator._WS_Combat;
            WS_Base.BaseUnit objCharacter = this;
            SetUpdateFlag(1106, wS_Combat.GetBasePercentBlock(ref objCharacter, 0));
            var wS_Combat2 = WorldServiceLocator._WS_Combat;
            objCharacter = this;
            SetUpdateFlag(1107, wS_Combat2.GetBasePercentDodge(ref objCharacter, 0));
            var wS_Combat3 = WorldServiceLocator._WS_Combat;
            objCharacter = this;
            SetUpdateFlag(1108, wS_Combat3.GetBasePercentParry(ref objCharacter, 0));
            var wS_Combat4 = WorldServiceLocator._WS_Combat;
            objCharacter = this;
            SetUpdateFlag(1109, wS_Combat4.GetBasePercentCrit(ref objCharacter, 0));
            SetUpdateFlag(1176, Copper);
            SetUpdateFlag(150, Strength.Base);
            SetUpdateFlag(151, Agility.Base);
            SetUpdateFlag(152, Stamina.Base);
            SetUpdateFlag(153, Intellect.Base);
            SetUpdateFlag(154, Spirit.Base);
            SetUpdateFlag(155, Resistances[0].RealBase);
            SetUpdateFlag(156, Resistances[1].RealBase);
            SetUpdateFlag(157, Resistances[2].RealBase);
            SetUpdateFlag(158, Resistances[3].RealBase);
            SetUpdateFlag(159, Resistances[4].RealBase);
            SetUpdateFlag(160, Resistances[5].RealBase);
            SetUpdateFlag(161, Resistances[6].RealBase);
        }

        public void FillAllUpdateFlags()
        {
            SetUpdateFlag(0, GUID);
            SetUpdateFlag(2, 25);
            SetUpdateFlag(4, Size);
            if (Pet != null)
            {
                SetUpdateFlag(8, Pet.GUID);
            }
            SetUpdateFlag(22, Life.Current);
            SetUpdateFlag(23, Mana.Current);
            SetUpdateFlag(24, Rage.Current);
            SetUpdateFlag(26, Energy.Current);
            SetUpdateFlag(27, 0);
            SetUpdateFlag(28, Life.Maximum);
            SetUpdateFlag(29, Mana.Maximum);
            SetUpdateFlag(30, Rage.Maximum);
            SetUpdateFlag(32, Energy.Maximum);
            SetUpdateFlag(33, 0);
            SetUpdateFlag(163, Life.Base);
            SetUpdateFlag(162, Mana.Base);
            SetUpdateFlag(34, Level);
            SetUpdateFlag(35, Faction);
            SetUpdateFlag(46, cUnitFlags);
            SetUpdateFlag(150, Strength.Base);
            SetUpdateFlag(151, Agility.Base);
            SetUpdateFlag(152, Stamina.Base);
            SetUpdateFlag(153, Spirit.Base);
            SetUpdateFlag(154, Intellect.Base);
            SetUpdateFlag(36, cBytes0);
            SetUpdateFlag(138, cBytes1);
            SetUpdateFlag(164, cBytes2);
            SetUpdateFlag(131, Model);
            SetUpdateFlag(132, Model_Native);
            SetUpdateFlag(133, Mount);
            SetUpdateFlag(143, cDynamicFlags);
            SetUpdateFlag(193, cPlayerBytes);
            SetUpdateFlag(194, cPlayerBytes2);
            SetUpdateFlag(195, cPlayerBytes3);
            SetUpdateFlag(1261, WatchedFactionIndex);
            SetUpdateFlag(716, XP);
            SetUpdateFlag(717, WorldServiceLocator._WS_Player_Initializator.XPTable[Level]);
            SetUpdateFlag(1175, RestBonus);
            SetUpdateFlag(190, (int)cPlayerFlags);
            SetUpdateFlag(1222, cPlayerFieldBytes);
            SetUpdateFlag(1260, cPlayerFieldBytes2);
            SetUpdateFlag(129, BoundingRadius);
            SetUpdateFlag(130, CombatReach);
            SetUpdateFlag(1102, TalentPoints);
            SetUpdateFlag(191, GuildID);
            SetUpdateFlag(192, GuildRank);
            SetUpdateFlag(134, Damage.Minimum);
            SetUpdateFlag(135, Damage.Maximum + BaseUnarmedDamage);
            SetUpdateFlag(126, GetAttackTime(WeaponAttackType.BASE_ATTACK));
            SetUpdateFlag(127, GetAttackTime(WeaponAttackType.OFF_ATTACK));
            SetUpdateFlag(145, 1f);
            SetUpdateFlag(165, AttackPower);
            SetUpdateFlag(168, AttackPowerRanged);
            var wS_Combat = WorldServiceLocator._WS_Combat;
            WS_Base.BaseUnit objCharacter = this;
            SetUpdateFlag(1109, wS_Combat.GetBasePercentCrit(ref objCharacter, 0));
            var wS_Combat2 = WorldServiceLocator._WS_Combat;
            objCharacter = this;
            SetUpdateFlag(1110, wS_Combat2.GetBasePercentCrit(ref objCharacter, 0));
            byte i2 = 0;
            checked
            {
                do
                {
                    SetUpdateFlag(1201 + i2, spellDamage[i2].PositiveBonus);
                    SetUpdateFlag(1208 + i2, spellDamage[i2].NegativeBonus);
                    SetUpdateFlag(1215 + i2, spellDamage[i2].Modifier);
                    i2 = (byte)unchecked((uint)(i2 + 1));
                }
                while (i2 <= 6u);
                SetUpdateFlag(155, Resistances[0].Base);
                SetUpdateFlag(156, Resistances[1].Base);
                SetUpdateFlag(157, Resistances[2].Base);
                SetUpdateFlag(158, Resistances[3].Base);
                SetUpdateFlag(159, Resistances[4].Base);
                SetUpdateFlag(160, Resistances[5].Base);
                SetUpdateFlag(161, Resistances[6].Base);
                SetUpdateFlag(1176, Copper);
                foreach (var Skill in Skills)
                {
                    SetUpdateFlag(718 + (SkillsPositions[Skill.Key] * 3), Skill.Key);
                    SetUpdateFlag(718 + (SkillsPositions[Skill.Key] * 3) + 1, Skill.Value.GetSkill);
                    SetUpdateFlag(718 + (SkillsPositions[Skill.Key] * 3) + 2, Skill.Value.Bonus);
                }
                SetUpdateFlag(128, GetAttackTime(WeaponAttackType.RANGED_ATTACK));
                SetUpdateFlag(136, OffHandDamage.Minimum);
                SetUpdateFlag(137, OffHandDamage.Maximum);
                SetUpdateFlag(150, Strength.Base);
                SetUpdateFlag(151, Agility.Base);
                SetUpdateFlag(152, Stamina.Base);
                SetUpdateFlag(153, Spirit.Base);
                SetUpdateFlag(154, Intellect.Base);
                SetUpdateFlag(166, AttackPowerMods);
                SetUpdateFlag(169, AttackPowerModsRanged);
                SetUpdateFlag(171, RangedDamage.Minimum);
                SetUpdateFlag(172, RangedDamage.Maximum + BaseRangedDamage);
                SetUpdateFlag(167, 0f);
                SetUpdateFlag(170, 0f);
                byte n = 0;
                do
                {
                    if (TalkQuests[n] == null)
                    {
                        SetUpdateFlag(198 + (n * 3), 0);
                        SetUpdateFlag(199 + (n * 3), 0);
                        SetUpdateFlag(199 + (n * 3) + 1, 0);
                    }
                    else
                    {
                        SetUpdateFlag(198 + (n * 3), TalkQuests[n].ID);
                        SetUpdateFlag(199 + (n * 3), TalkQuests[n].GetProgress());
                        SetUpdateFlag(199 + (n * 3) + 1, 0);
                    }
                    n = (byte)unchecked((uint)(n + 1));
                }
                while (n <= 24u);
                var wS_Combat3 = WorldServiceLocator._WS_Combat;
                objCharacter = this;
                SetUpdateFlag(1106, wS_Combat3.GetBasePercentBlock(ref objCharacter, 0));
                var wS_Combat4 = WorldServiceLocator._WS_Combat;
                objCharacter = this;
                SetUpdateFlag(1107, wS_Combat4.GetBasePercentDodge(ref objCharacter, 0));
                var wS_Combat5 = WorldServiceLocator._WS_Combat;
                objCharacter = this;
                SetUpdateFlag(1108, wS_Combat5.GetBasePercentParry(ref objCharacter, 0));
                var b = (byte)WorldServiceLocator._Global_Constants.PLAYER_EXPLORED_ZONES_SIZE;
                byte m = 0;
                while (m <= (uint)b)
                {
                    SetUpdateFlag(1111 + m, ZonesExplored[m]);
                    m = (byte)unchecked((uint)(m + 1));
                }
                SetUpdateFlag(1255, HonorKillsLifeTime);
                SetUpdateFlag(1256, DishonorKillsLifeTime);
                SetUpdateFlag(1250, HonorKillsToday + (DishonorKillsToday << 16));
                SetUpdateFlag(1253, HonorKillsThisWeek);
                SetUpdateFlag(1252, HonorKillsLastWeek);
                SetUpdateFlag(1251, HonorKillsYesterday);
                SetUpdateFlag(1254, HonorPointsThisWeek);
                SetUpdateFlag(1258, HonorPointsLastWeek);
                SetUpdateFlag(1257, HonorPointsYesterday);
                SetUpdateFlag(1259, StandingLastWeek);
                byte l = 0;
                do
                {
                    if (Items.ContainsKey(l))
                    {
                        if (l < 19u)
                        {
                            SetUpdateFlag(260 + (l * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE), Items[l].ItemEntry);
                            foreach (var Enchant in Items[l].Enchantments)
                            {
                                SetUpdateFlag(261 + (Enchant.Key * 3) + (l * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE), Enchant.Value.ID);
                                SetUpdateFlag(262 + (Enchant.Key * 3) + (l * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE), Enchant.Value.Charges);
                                SetUpdateFlag(263 + (Enchant.Key * 3) + (l * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE), Enchant.Value.Duration);
                            }
                            SetUpdateFlag(268 + (l * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE), Items[l].RandomProperties);
                        }
                        SetUpdateFlag(486 + (l * 2), Items[l].GUID);
                    }
                    else
                    {
                        if (l < 19u)
                        {
                            SetUpdateFlag(260 + (l * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE), 0);
                            SetUpdateFlag(268 + (l * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE), 0);
                        }
                        SetUpdateFlag(486 + (l * 2), 0);
                    }
                    l = (byte)unchecked((uint)(l + 1));
                }
                while (l <= 112u);
                SetUpdateFlag(1223, AmmoID);
                var b2 = (byte)(WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1);
                byte k = 0;
                while (k <= (uint)b2)
                {
                    if (ActiveSpells[k] != null)
                    {
                        SetUpdateFlag(47 + k, ActiveSpells[k].SpellID);
                    }
                    k = (byte)unchecked((uint)(k + 1));
                }
                var b3 = (byte)(WorldServiceLocator._Global_Constants.MAX_AURA_EFFECT_FLAGs - 1);
                byte j = 0;
                while (j <= (uint)b3)
                {
                    SetUpdateFlag(95 + j, ActiveSpells_Flags[j]);
                    j = (byte)unchecked((uint)(j + 1));
                }
                var b4 = (byte)(WorldServiceLocator._Global_Constants.MAX_AURA_EFFECT_LEVELSs - 1);
                byte i = 0;
                while (i <= (uint)b4)
                {
                    SetUpdateFlag(113 + i, ActiveSpells_Count[i]);
                    SetUpdateFlag(101 + i, ActiveSpells_Level[i]);
                    i = (byte)unchecked((uint)(i + 1));
                }
            }
        }

        public void FillAllUpdateFlags(ref Packets.UpdateClass Update)
        {
            Update.SetUpdateFlag(0, GUID);
            Update.SetUpdateFlag(4, Size);
            Update.SetUpdateFlag(2, 25);
            if (Pet != null)
            {
                SetUpdateFlag(8, Pet.GUID);
            }
            Update.SetUpdateFlag(22, Life.Current);
            Update.SetUpdateFlag(23, Mana.Current);
            Update.SetUpdateFlag(24, Rage.Current);
            Update.SetUpdateFlag(26, Energy.Current);
            Update.SetUpdateFlag(27, 0);
            Update.SetUpdateFlag(28, Life.Maximum);
            Update.SetUpdateFlag(29, Mana.Maximum);
            Update.SetUpdateFlag(30, Rage.Maximum);
            Update.SetUpdateFlag(32, Energy.Maximum);
            Update.SetUpdateFlag(33, 0);
            Update.SetUpdateFlag(46, cUnitFlags);
            Update.SetUpdateFlag(34, Level);
            Update.SetUpdateFlag(35, Faction);
            Update.SetUpdateFlag(36, cBytes0);
            Update.SetUpdateFlag(138, cBytes1);
            Update.SetUpdateFlag(164, cBytes2);
            Update.SetUpdateFlag(131, Model);
            Update.SetUpdateFlag(132, Model_Native);
            Update.SetUpdateFlag(133, Mount);
            Update.SetUpdateFlag(143, cDynamicFlags);
            Update.SetUpdateFlag(193, cPlayerBytes);
            Update.SetUpdateFlag(194, cPlayerBytes2);
            Update.SetUpdateFlag(195, cPlayerBytes3);
            Update.SetUpdateFlag(190, (int)cPlayerFlags);
            Update.SetUpdateFlag(129, BoundingRadius);
            Update.SetUpdateFlag(130, CombatReach);
            Update.SetUpdateFlag(16, TargetGUID);
            Update.SetUpdateFlag(191, GuildID);
            Update.SetUpdateFlag(192, GuildRank);
            byte l = 0;
            checked
            {
                do
                {
                    if (Items.ContainsKey(l))
                    {
                        if (l < 19u)
                        {
                            Update.SetUpdateFlag(260 + (l * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE), Items[l].ItemEntry);
                            Update.SetUpdateFlag(268 + (l * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE), Items[l].RandomProperties);
                        }
                        Update.SetUpdateFlag(486 + (l * 2), Items[l].GUID);
                    }
                    else
                    {
                        if (l < 19u)
                        {
                            Update.SetUpdateFlag(260 + (l * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE), 0);
                            Update.SetUpdateFlag(268 + (l * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE), 0);
                        }
                        Update.SetUpdateFlag(486 + (l * 2), 0);
                    }
                    l = (byte)unchecked((uint)(l + 1));
                }
                while (l <= 18u);
                var b = (byte)(WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1);
                byte k = 0;
                while (k <= (uint)b)
                {
                    if (ActiveSpells[k] != null)
                    {
                        Update.SetUpdateFlag(47 + k, ActiveSpells[k].SpellID);
                    }
                    k = (byte)unchecked((uint)(k + 1));
                }
                var b2 = (byte)(WorldServiceLocator._Global_Constants.MAX_AURA_EFFECT_FLAGs - 1);
                byte j = 0;
                while (j <= (uint)b2)
                {
                    Update.SetUpdateFlag(95 + j, ActiveSpells_Flags[j]);
                    j = (byte)unchecked((uint)(j + 1));
                }
                var b3 = (byte)(WorldServiceLocator._Global_Constants.MAX_AURA_EFFECT_LEVELSs - 1);
                byte i = 0;
                while (i <= (uint)b3)
                {
                    Update.SetUpdateFlag(113 + i, ActiveSpells_Count[i]);
                    Update.SetUpdateFlag(101 + i, ActiveSpells_Level[i]);
                    i = (byte)unchecked((uint)(i + 1));
                }
            }
        }

        public void PrepareUpdate(ref Packets.PacketClass packet, int UPDATETYPE = 2)
        {
            packet.AddInt8(checked((byte)UPDATETYPE));
            packet.AddPackGUID(GUID);
            if (UPDATETYPE is 2 or 3)
            {
                packet.AddInt8(4);
            }
            if (UPDATETYPE is 2 or 1 or 3)
            {
                var flags2 = 8192;
                if (OnTransport != null)
                {
                    flags2 |= 0x2000000;
                }
                packet.AddInt8(113);
                packet.AddInt32(flags2);
                packet.AddInt32(WorldServiceLocator._WS_Network.MsTime());
                packet.AddSingle(positionX);
                packet.AddSingle(positionY);
                packet.AddSingle(positionZ);
                packet.AddSingle(orientation);
                if (((uint)flags2 & 0x2000000u) != 0)
                {
                    packet.AddUInt64(OnTransport.GUID);
                    packet.AddSingle(transportX);
                    packet.AddSingle(transportY);
                    packet.AddSingle(transportZ);
                    packet.AddSingle(orientation);
                }
                packet.AddInt32(0);
                packet.AddInt32(0);
                packet.AddInt32(0);
                packet.AddInt32(0);
                packet.AddInt32(0);
                packet.AddSingle(WalkSpeed);
                packet.AddSingle(RunSpeed);
                packet.AddSingle(RunBackSpeed);
                packet.AddSingle(SwimSpeed);
                packet.AddSingle(SwimBackSpeed);
                packet.AddSingle(TurnRate);
                packet.AddUInt32(47u);
            }
            if (UPDATETYPE is not (2 or 0 or 3))
            {
                return;
            }
            var UpdateCount = 0;
            checked
            {
                var num = UpdateMask.Count - 1;
                for (var j = 0; j <= num; j++)
                {
                    if (UpdateMask.Get(j))
                    {
                        UpdateCount = j;
                    }
                }
                packet.AddInt8((byte)(checked(UpdateCount + 32) / 32));
                packet.AddBitArray(UpdateMask, checked((byte)(checked(UpdateCount + 32) / 32)) * 4);
                var num2 = UpdateMask.Count - 1;
                for (var i = 0; i <= num2; i++)
                {
                    if (UpdateMask.Get(i))
                    {
                        if (UpdateData[i] is uint)
                        {
                            packet.AddUInt32(Conversions.ToUInteger(UpdateData[i]));
                        }
                        else if (UpdateData[i] is float)
                        {
                            packet.AddSingle(Conversions.ToSingle(UpdateData[i]));
                        }
                        else
                        {
                            packet.AddInt32(Conversions.ToInteger(UpdateData[i]));
                        }
                    }
                }
                UpdateMask.SetAll(value: false);
            }
        }

        public void SendChatMessage(ref CharacterObject Sender, string Message, ChatMsg msgType, int msgLanguage, string ChannelName = "Global", bool SendToMe = false)
        {
            var packet = WorldServiceLocator._Functions.BuildChatMessage(Sender.GUID, Message, msgType, (LANGUAGES)msgLanguage, WorldServiceLocator._WS_Handlers_Chat.GetChatFlag(Sender), ChannelName);
            SendToNearPlayers(ref packet, 0uL, SendToMe);
            packet.Dispose();
        }

        public void CommandResponse(string Message)
        {
            var Messages = Message.Split(new string[1]
            {
                    Environment.NewLine
            }, StringSplitOptions.RemoveEmptyEntries);
            if (Messages.Length == 0)
            {
                Messages = new string[1]
                {
                        Message
                };
            }
            var array = Messages;
            foreach (var Msg in array)
            {
                var packet = WorldServiceLocator._Functions.BuildChatMessage(2147483647uL, Msg, ChatMsg.CHAT_MSG_SYSTEM, LANGUAGES.LANG_GLOBAL);
                client.Send(ref packet);
                packet.Dispose();
            }
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_MESSAGECHAT", client.IP, client.Port);
        }

        public void SystemMessage(string Message)
        {
            WorldServiceLocator._Functions.SendMessageSystem(client, Message);
        }

        public void CastOnSelf(int SpellID)
        {
            if (WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(SpellID))
            {
                WS_Spells.SpellTargets Targets = new();
                var spellTargets = Targets;
                WS_Base.BaseUnit objCharacter = this;
                spellTargets.SetTarget_UNIT(ref objCharacter);
                WS_Base.BaseObject Caster = this;
                WS_Spells.CastSpellParameters castParams = new(ref Targets, ref Caster, SpellID);
                castParams.Cast(null);
            }
        }

        public void ApplySpell(int SpellID)
        {
            if (WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(SpellID))
            {
                WS_Spells.SpellTargets t = new();
                WS_Base.BaseUnit objCharacter = this;
                t.SetTarget_SELF(ref objCharacter);
                var spellInfo = WorldServiceLocator._WS_Spells.SPELLs[SpellID];
                var Character = this;
                if (spellInfo.CanCast(ref Character, t, FirstCheck: false) == SpellFailedReason.SPELL_NO_ERROR)
                {
                    var spellInfo2 = WorldServiceLocator._WS_Spells.SPELLs[SpellID];
                    WS_Base.BaseObject caster = this;
                    spellInfo2.Apply(ref caster, t);
                }
            }
        }

        public void ProhibitSpellSchool(int School, int Time)
        {
            Packets.PacketClass packet = new(Opcodes.SMSG_SPELL_COOLDOWN);
            checked
            {
                try
                {
                    packet.AddInt32((int)GUID);
                    var curTime = WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now);
                    foreach (var Spell in Spells)
                    {
                        var SpellInfo = WorldServiceLocator._WS_Spells.SPELLs[Spell.Key];
                        if (SpellInfo.School == School && (Spell.Value.Cooldown < curTime || Spell.Value.Cooldown - curTime < Time))
                        {
                            packet.AddInt32(Spell.Key);
                            packet.AddInt32(Time);
                            Spell.Value.Cooldown = (uint)(curTime + Time);
                            Spell.Value.CooldownItem = 0;
                        }
                    }
                    client.Send(ref packet);
                }
                finally
                {
                    packet.Dispose();
                }
            }
        }

        public bool FinishAllSpells(bool OK = false)
        {
            var result1 = FinishSpell(CurrentSpellTypes.CURRENT_AUTOREPEAT_SPELL, OK);
            var result2 = FinishSpell(CurrentSpellTypes.CURRENT_CHANNELED_SPELL, OK);
            var result3 = FinishSpell(CurrentSpellTypes.CURRENT_GENERIC_SPELL, OK);
            return result1 || result2 || result3;
        }

        public bool FinishSpell(CurrentSpellTypes SpellType, bool OK = false)
        {
            if (SpellType == CurrentSpellTypes.CURRENT_CHANNELED_SPELL)
            {
                var wS_Spells = WorldServiceLocator._WS_Spells;
                var Caster = this;
                wS_Spells.SendChannelUpdate(ref Caster, 0);
            }
            if (client.Character.spellCasted[(int)SpellType] == null)
            {
                return false;
            }
            if (client.Character.spellCasted[(int)SpellType].Finished)
            {
                return false;
            }
            client.Character.spellCasted[(int)SpellType].State = SpellCastState.SPELL_STATE_IDLE;
            client.Character.spellCasted[(int)SpellType].Stopped = true;
            if (SpellType == CurrentSpellTypes.CURRENT_AUTOREPEAT_SPELL)
            {
                client.Character.AutoShotSpell = 0;
                client.Character.attackState.AttackStop();
            }
            else
            {
                var SpellID = spellCasted[(int)SpellType].SpellID;
                var spellInfo = WorldServiceLocator._WS_Spells.SPELLs[SpellID];
                ref var character = ref client.Character;
                WS_Base.BaseUnit Caster2 = character;
                spellInfo.SendInterrupted(0, ref Caster2);
                character = (CharacterObject)Caster2;
                if (!OK)
                {
                    WorldServiceLocator._WS_Spells.SendCastResult(SpellFailedReason.SPELL_FAILED_INTERRUPTED, ref client, SpellID);
                }
                client.Character.RemoveAuraBySpell(SpellID);
                var DynamicObjects = client.Character.dynamicObjects.ToArray();
                var array = DynamicObjects;
                foreach (var tmpDO in array)
                {
                    if (tmpDO.SpellID == SpellID)
                    {
                        tmpDO.Delete();
                        client.Character.dynamicObjects.Remove(tmpDO);
                        break;
                    }
                }
                var GameObjects = client.Character.gameObjects.ToArray();
                var array2 = GameObjects;
                foreach (var tmpGO in array2)
                {
                    if (tmpGO.CreatedBySpell == SpellID)
                    {
                        tmpGO.Destroy(tmpGO);
                        client.Character.gameObjects.Remove(tmpGO);
                        break;
                    }
                }
            }
            return true;
        }

        public void LearnSpell(int SpellID)
        {
            if (Spells.ContainsKey(SpellID) || !WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(SpellID))
            {
                return;
            }
            Spells.Add(SpellID, new WS_Spells.CharacterSpell(SpellID, 1, 0u, 0));
            WorldServiceLocator._WorldServer.CharacterDatabase.Update($"INSERT INTO characters_spells (guid, spellid, active, cooldown, cooldownitem) VALUES ({GUID},{SpellID},{1},{0},{0});");
            if (client == null)
            {
                return;
            }
            Packets.PacketClass SMSG_LEARNED_SPELL = new(Opcodes.SMSG_LEARNED_SPELL);
            try
            {
                SMSG_LEARNED_SPELL.AddInt32(SpellID);
                client.Send(ref SMSG_LEARNED_SPELL);
            }
            finally
            {
                SMSG_LEARNED_SPELL.Dispose();
            }
            WS_Spells.SpellTargets t = new();
            WS_Base.BaseUnit objCharacter = this;
            t.SetTarget_SELF(ref objCharacter);
            if (WorldServiceLocator._WS_Spells.SPELLs[SpellID].IsPassive)
            {
                int num;
                if (!HavePassiveAura(SpellID))
                {
                    var spellInfo = WorldServiceLocator._WS_Spells.SPELLs[SpellID];
                    var Character = this;
                    num = (spellInfo.CanCast(ref Character, t, FirstCheck: false) == SpellFailedReason.SPELL_NO_ERROR) ? 1 : 0;
                }
                else
                {
                    num = 0;
                }
                if (num != 0)
                {
                    var spellInfo2 = WorldServiceLocator._WS_Spells.SPELLs[SpellID];
                    WS_Base.BaseObject caster = this;
                    spellInfo2.Apply(ref caster, t);
                }
            }
            if (!WorldServiceLocator._WS_Spells.SPELLs[SpellID].CanStackSpellRank && WorldServiceLocator._WS_Spells.SpellChains.ContainsKey(SpellID) && Spells.ContainsKey(WorldServiceLocator._WS_Spells.SpellChains[SpellID]))
            {
                Spells[WorldServiceLocator._WS_Spells.SpellChains[SpellID]].Active = 0;
                WorldServiceLocator._WorldServer.CharacterDatabase.Update($"UPDATE characters_spells SET active = 0 WHERE guid = {GUID} AND spellid = {SpellID};");
                Packets.PacketClass packet = new(Opcodes.SMSG_SUPERCEDED_SPELL);
                try
                {
                    packet.AddInt32(WorldServiceLocator._WS_Spells.SpellChains[SpellID]);
                    packet.AddInt32(SpellID);
                    client.Send(ref packet);
                }
                finally
                {
                    packet.Dispose();
                }
            }
            checked
            {
                var maxSkill = (Level > WorldServiceLocator._WS_Player_Initializator.DEFAULT_MAX_LEVEL) ? (WorldServiceLocator._WS_Player_Initializator.DEFAULT_MAX_LEVEL * 5) : (Level * 5);
                switch (SpellID)
                {
                    case 4036:
                        LearnSpell(3918);
                        LearnSpell(3919);
                        LearnSpell(3920);
                        break;

                    case 3908:
                        LearnSpell(2387);
                        LearnSpell(2963);
                        break;

                    case 7411:
                        LearnSpell(7418);
                        LearnSpell(7421);
                        LearnSpell(13262);
                        break;

                    case 2259:
                        LearnSpell(2329);
                        LearnSpell(7183);
                        LearnSpell(2330);
                        break;

                    case 2018:
                        LearnSpell(2663);
                        LearnSpell(12260);
                        LearnSpell(2660);
                        LearnSpell(3115);
                        break;

                    case 2108:
                        LearnSpell(2152);
                        LearnSpell(9058);
                        LearnSpell(9059);
                        LearnSpell(2149);
                        LearnSpell(7126);
                        LearnSpell(2881);
                        break;

                    case 2550:
                        LearnSpell(818);
                        LearnSpell(2540);
                        LearnSpell(2538);
                        break;

                    case 3273:
                        LearnSpell(3275);
                        break;

                    case 7620:
                        LearnSpell(7738);
                        break;

                    case 2575:
                        LearnSpell(2580);
                        LearnSpell(2656);
                        LearnSpell(2657);
                        break;

                    case 2366:
                        LearnSpell(2383);
                        break;

                    case 264:
                        if (!HaveSpell(75))
                        {
                            LearnSpell(2480);
                        }
                        LearnSkill(45, 1, (short)maxSkill);
                        break;

                    case 266:
                        if (!HaveSpell(75))
                        {
                            LearnSpell(2480);
                        }
                        LearnSkill(46, 1, (short)maxSkill);
                        break;

                    case 5011:
                        if (!HaveSpell(75))
                        {
                            LearnSpell(7919);
                        }
                        LearnSkill(226, 1, (short)maxSkill);
                        break;

                    case 2567:
                        LearnSpell(2764);
                        LearnSkill(176, 1, (short)maxSkill);
                        break;

                    case 5009:
                        LearnSpell(5019);
                        LearnSkill(228, 1, (short)maxSkill);
                        break;

                    case 9078:
                        LearnSkill(415);
                        break;

                    case 9077:
                        LearnSkill(414);
                        break;

                    case 8737:
                        LearnSkill(413);
                        break;

                    case 750:
                        LearnSkill(293);
                        break;

                    case 9116:
                        LearnSkill(433);
                        break;

                    case 674:
                        LearnSkill(118);
                        break;

                    case 196:
                        LearnSkill(44, 1, (short)maxSkill);
                        break;

                    case 197:
                        LearnSkill(172, 1, (short)maxSkill);
                        break;

                    case 227:
                        LearnSkill(136, 1, (short)maxSkill);
                        break;

                    case 198:
                        LearnSkill(54, 1, (short)maxSkill);
                        break;

                    case 199:
                        LearnSkill(160, 1, (short)maxSkill);
                        break;

                    case 201:
                        LearnSkill(43, 1, (short)maxSkill);
                        break;

                    case 202:
                        LearnSkill(55, 1, (short)maxSkill);
                        break;

                    case 1180:
                        LearnSkill(173, 1, (short)maxSkill);
                        break;

                    case 15590:
                        LearnSkill(473, 1, (short)maxSkill);
                        break;

                    case 200:
                        LearnSkill(229, 1, (short)maxSkill);
                        break;

                    case 3386:
                        LearnSkill(227, 1, (short)maxSkill);
                        break;

                    case 2842:
                        LearnSkill(40, 1, (short)maxSkill);
                        break;

                    case 668:
                        LearnSkill(98, 300, 300);
                        break;

                    case 669:
                        LearnSkill(109, 300, 300);
                        break;

                    case 670:
                        LearnSkill(115, 300, 300);
                        break;

                    case 671:
                        LearnSkill(113, 300, 300);
                        break;

                    case 672:
                        LearnSkill(111, 300, 300);
                        break;

                    case 813:
                        LearnSkill(137, 300, 300);
                        break;

                    case 814:
                        LearnSkill(138, 300, 300);
                        break;

                    case 815:
                        LearnSkill(139, 300, 300);
                        break;

                    case 816:
                        LearnSkill(140, 300, 300);
                        break;

                    case 817:
                        LearnSkill(141, 300, 300);
                        break;

                    case 7340:
                        LearnSkill(313, 300, 300);
                        break;

                    case 7341:
                        LearnSkill(315, 300, 300);
                        break;

                    case 17737:
                        LearnSkill(673, 300, 300);
                        break;
                }
            }
        }

        public void UnLearnSpell(int SpellID)
        {
            if (Spells.ContainsKey(SpellID))
            {
                Spells.Remove(SpellID);
                WorldServiceLocator._WorldServer.CharacterDatabase.Update($"DELETE FROM characters_spells WHERE guid = {GUID} AND spellid = {SpellID};");
                Packets.PacketClass SMSG_REMOVED_SPELL = new(Opcodes.SMSG_REMOVED_SPELL);
                try
                {
                    SMSG_REMOVED_SPELL.AddInt32(SpellID);
                    client.Send(ref SMSG_REMOVED_SPELL);
                }
                finally
                {
                    SMSG_REMOVED_SPELL.Dispose();
                }
                client.Character.RemoveAuraBySpell(SpellID);
            }
        }

        public bool HaveSpell(int SpellID)
        {
            return Spells.ContainsKey(SpellID);
        }

        public void LearnSkill(int SkillID, short Current = 1, short Maximum = 1)
        {
            checked
            {
                if (Skills.ContainsKey(SkillID))
                {
                    Skills[SkillID].Base = Maximum;
                    if (Current != 1)
                    {
                        Skills[SkillID].Current = Current;
                    }
                }
                else
                {
                    var num = (short)WorldServiceLocator._Global_Constants.PLAYER_SKILL_INFO_SIZE;
                    short i = 0;
                    while (i <= num && SkillsPositions.ContainsValue(i))
                    {
                        i = (short)unchecked(i + 1);
                    }
                    if (i > WorldServiceLocator._Global_Constants.PLAYER_SKILL_INFO_SIZE)
                    {
                        return;
                    }
                    SkillsPositions.Add(SkillID, i);
                    Skills.Add(SkillID, new WS_PlayerHelper.TSkill(Current, Maximum));
                }
                if (client != null)
                {
                    SetUpdateFlag(718 + (SkillsPositions[SkillID] * 3), SkillID);
                    SetUpdateFlag(718 + (SkillsPositions[SkillID] * 3) + 1, Skills[SkillID].GetSkill);
                    SendCharacterUpdate();
                }
            }
        }

        public bool HaveSkill(int SkillID, int SkillValue = 0)
        {
            return Skills.ContainsKey(SkillID) && Skills[SkillID].Current >= SkillValue;
        }

        public void UpdateSkill(int SkillID, float SpeedMod = 0f)
        {
            if (SkillID != 0 && Skills[SkillID].Current < Skills[SkillID].Maximum && (Skills[SkillID].Current / (double)Skills[SkillID].Maximum) - SpeedMod < WorldServiceLocator._WorldServer.Rnd.NextDouble())
            {
                Skills[SkillID].Increment();
                SetUpdateFlag(checked(718 + (SkillsPositions[SkillID] * 3) + 1), Skills[SkillID].GetSkill);
                SendCharacterUpdate();
            }
        }

        public void SetLevel(byte SetToLevel)
        {
            var TotalXp = 0;
            if (Level <= (uint)SetToLevel)
            {
                short level = Level;
                checked
                {
                    var num = (short)(SetToLevel - 1);
                    for (var i = level; i <= num; i = (short)unchecked(i + 1))
                    {
                        TotalXp += WorldServiceLocator._WS_Player_Initializator.XPTable[i];
                    }
                    AddXP(TotalXp, 0, 0uL, LogIt: false);
                }
            }
        }

        public void AddXP(int Ammount, int RestedBonus, ulong VictimGUID = 0uL, bool LogIt = true)
        {
            if (Ammount <= 0 || Level >= WorldServiceLocator._WS_Player_Initializator.DEFAULT_MAX_LEVEL)
            {
                return;
            }
            checked
            {
                XP += Ammount;
                if (LogIt)
                {
                    LogXPGain(Ammount, RestedBonus, VictimGUID, 1f);
                }
                if (RestedBonus > 0)
                {
                    if (RestBonus <= 0)
                    {
                        RestBonus = 0;
                        RestState = XPSTATE.Normal;
                    }
                    SetUpdateFlag(194, cPlayerBytes2);
                    SetUpdateFlag(1175, RestBonus);
                }
                do
                {
                    if (XP >= WorldServiceLocator._WS_Player_Initializator.XPTable[Level])
                    {
                        XP -= WorldServiceLocator._WS_Player_Initializator.XPTable[Level];
                        Level++;
                        GroupUpdateFlag |= 64u;
                        WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.ClientUpdate(client.Index, (uint)ZoneID, Level);
                        var oldLife = Life.Maximum;
                        var oldMana = Mana.Maximum;
                        var oldStrength = Strength.Base;
                        var oldAgility = Agility.Base;
                        var oldStamina = Stamina.Base;
                        var oldIntellect = Intellect.Base;
                        var oldSpirit = Spirit.Base;
                        var wS_Player_Initializator = WorldServiceLocator._WS_Player_Initializator;
                        var objCharacter = this;
                        wS_Player_Initializator.CalculateOnLevelUP(ref objCharacter);
                        Packets.PacketClass SMSG_LEVELUP_INFO = new(Opcodes.SMSG_LEVELUP_INFO);
                        try
                        {
                            SMSG_LEVELUP_INFO.AddInt32(Level);
                            SMSG_LEVELUP_INFO.AddInt32(Life.Maximum - oldLife);
                            SMSG_LEVELUP_INFO.AddInt32(Mana.Maximum - oldMana);
                            SMSG_LEVELUP_INFO.AddInt32(0);
                            SMSG_LEVELUP_INFO.AddInt32(0);
                            SMSG_LEVELUP_INFO.AddInt32(0);
                            SMSG_LEVELUP_INFO.AddInt32(0);
                            SMSG_LEVELUP_INFO.AddInt32(0);
                            SMSG_LEVELUP_INFO.AddInt32(0);
                            SMSG_LEVELUP_INFO.AddInt32(Strength.Base - oldStrength);
                            SMSG_LEVELUP_INFO.AddInt32(Agility.Base - oldAgility);
                            SMSG_LEVELUP_INFO.AddInt32(Stamina.Base - oldStamina);
                            SMSG_LEVELUP_INFO.AddInt32(Intellect.Base - oldIntellect);
                            SMSG_LEVELUP_INFO.AddInt32(Spirit.Base - oldSpirit);
                            if (client != null)
                            {
                                client.Send(ref SMSG_LEVELUP_INFO);
                            }
                        }
                        finally
                        {
                            SMSG_LEVELUP_INFO.Dispose();
                        }
                        Life.Current = Life.Maximum;
                        Mana.Current = Mana.Maximum;
                        Resistances[0].Base += (Agility.Base - oldAgility) * 2;
                        SetUpdateFlag(716, XP);
                        SetUpdateFlag(717, WorldServiceLocator._WS_Player_Initializator.XPTable[Level]);
                        SetUpdateFlag(1102, TalentPoints);
                        SetUpdateFlag(34, Level);
                        SetUpdateFlag(150, Strength.Base);
                        SetUpdateFlag(151, Agility.Base);
                        SetUpdateFlag(152, Stamina.Base);
                        SetUpdateFlag(153, Spirit.Base);
                        SetUpdateFlag(154, Intellect.Base);
                        SetUpdateFlag(22, Life.Current);
                        SetUpdateFlag(163, Life.Base);
                        SetUpdateFlag(23, Mana.Current);
                        SetUpdateFlag(162, Mana.Base);
                        SetUpdateFlag(28, Life.Maximum);
                        SetUpdateFlag(29, Mana.Maximum);
                        SetUpdateFlag(165, AttackPower);
                        SetUpdateFlag(166, AttackPowerMods);
                        SetUpdateFlag(168, AttackPowerRanged);
                        SetUpdateFlag(169, AttackPowerModsRanged);
                        SetUpdateFlag(155, Resistances[0].Base);
                        SetUpdateFlag(134, Damage.Minimum);
                        SetUpdateFlag(135, (float)(Damage.Maximum + ((AttackPower + AttackPowerMods) * 0.071428571428571425)));
                        SetUpdateFlag(136, OffHandDamage.Minimum);
                        SetUpdateFlag(137, OffHandDamage.Maximum);
                        SetUpdateFlag(171, RangedDamage.Minimum);
                        SetUpdateFlag(172, RangedDamage.Maximum + BaseRangedDamage);
                        foreach (var Skill in Skills)
                        {
                            SetUpdateFlag(718 + (SkillsPositions[Skill.Key] * 3) + 1, Skill.Value.GetSkill);
                        }
                        if (client != null)
                        {
                            UpdateManaRegen();
                        }
                    }
                    else if (client != null)
                    {
                        SetUpdateFlag(716, XP);
                    }
                }
                while (XP >= WorldServiceLocator._WS_Player_Initializator.XPTable[Level] && Level < WorldServiceLocator._WS_Player_Initializator.DEFAULT_MAX_LEVEL);
                if (XP > WorldServiceLocator._WS_Player_Initializator.XPTable[Level])
                {
                    XP = WorldServiceLocator._WS_Player_Initializator.XPTable[Level];
                }
                if (client != null)
                {
                    SendCharacterUpdate();
                }
                SaveCharacter();
            }
        }

        public void ItemADD(int ItemEntry, byte dstBag, byte dstSlot, int Count = 1)
        {
            ItemObject tmpItem = new(ItemEntry, GUID);
            if (tmpItem.ItemInfo.Unique > 0 && ItemCOUNT(ItemEntry) > tmpItem.ItemInfo.Unique)
            {
                tmpItem.Delete();
                return;
            }
            if (Count > tmpItem.ItemInfo.Stackable)
            {
                Count = tmpItem.ItemInfo.Stackable;
            }
            tmpItem.StackCount = Count;
            if (dstBag == byte.MaxValue && dstSlot == byte.MaxValue)
            {
                ItemADD_AutoSlot(ref tmpItem);
            }
            else
            {
                ItemSETSLOT(ref tmpItem, dstBag, dstSlot);
            }
            if (dstBag == 0 && dstSlot < 23u && client != null)
            {
                UpdateAddItemStats(ref tmpItem, dstSlot);
            }
        }

        public void ItemREMOVE(byte srcBag, byte srcSlot, bool Destroy, bool Update)
        {
            checked
            {
                if (srcBag == 0)
                {
                    if (srcSlot < 23u)
                    {
                        if (srcSlot < 19u)
                        {
                            SetUpdateFlag(260 + (srcSlot * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE), 0);
                        }
                        Dictionary<byte, ItemObject> items;
                        byte key;
                        var Item = (items = Items)[key = srcSlot];
                        UpdateRemoveItemStats(ref Item, srcSlot);
                        items[key] = Item;
                    }
                    SetUpdateFlag(486 + (srcSlot * 2), 0);
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update($"UPDATE characters_inventory SET item_slot = {WorldServiceLocator._Global_Constants.ITEM_SLOT_NULL}, item_bag = {WorldServiceLocator._Global_Constants.ITEM_BAG_NULL} WHERE item_guid = {Items[srcSlot].GUID - WorldServiceLocator._Global_Constants.GUID_ITEM};");
                    if (Destroy)
                    {
                        Items[srcSlot].Delete();
                    }
                    Items.Remove(srcSlot);
                    if (Update)
                    {
                        SendCharacterUpdate();
                    }
                }
                else
                {
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update($"UPDATE characters_inventory SET item_slot = {WorldServiceLocator._Global_Constants.ITEM_SLOT_NULL}, item_bag = {WorldServiceLocator._Global_Constants.ITEM_BAG_NULL} WHERE item_guid = {Items[srcBag].Items[srcSlot].GUID - WorldServiceLocator._Global_Constants.GUID_ITEM};");
                    if (Destroy)
                    {
                        Items[srcBag].Items[srcSlot].Delete();
                    }
                    Items[srcBag].Items.Remove(srcSlot);
                    if (Update)
                    {
                        SendItemUpdate(Items[srcBag]);
                    }
                }
            }
        }

        public void ItemREMOVE(ulong itemGuid, bool Destroy, bool Update)
        {
            byte slot = 0;
            do
            {
                checked
                {
                    if (Items.ContainsKey(slot) && Items[slot].GUID == itemGuid)
                    {
                        WorldServiceLocator._WorldServer.CharacterDatabase.Update($"UPDATE characters_inventory SET item_slot = {WorldServiceLocator._Global_Constants.ITEM_SLOT_NULL}, item_bag = {WorldServiceLocator._Global_Constants.ITEM_BAG_NULL} WHERE item_guid = {Items[slot].GUID - WorldServiceLocator._Global_Constants.GUID_ITEM};");
                        if (slot < 23u)
                        {
                            if (slot < 19u)
                            {
                                SetUpdateFlag(260 + (slot * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE), 0);
                            }
                            Dictionary<byte, ItemObject> items;
                            byte key;
                            var Item = (items = Items)[key = slot];
                            UpdateRemoveItemStats(ref Item, slot);
                            items[key] = Item;
                        }
                        SetUpdateFlag(486 + (slot * 2), 0);
                        if (Destroy)
                        {
                            Items[slot].Delete();
                        }
                        Items.Remove(slot);
                        if (Update)
                        {
                            SendCharacterUpdate();
                        }
                        return;
                    }
                    slot = (byte)unchecked((uint)(slot + 1));
                }
            }
            while (slot <= 112u);
            byte bag = 19;
            do
            {
                checked
                {
                    if (Items.ContainsKey(bag))
                    {
                        var b = (byte)(Items[bag].ItemInfo.ContainerSlots - 1);
                        byte slot2 = 0;
                        while (slot2 <= (uint)b)
                        {
                            if (Items[bag].Items.ContainsKey(slot2) && Items[bag].Items[slot2].GUID == itemGuid)
                            {
                                WorldServiceLocator._WorldServer.CharacterDatabase.Update($"UPDATE characters_inventory SET item_slot = {WorldServiceLocator._Global_Constants.ITEM_SLOT_NULL}, item_bag = {WorldServiceLocator._Global_Constants.ITEM_BAG_NULL} WHERE item_guid = {Items[bag].Items[slot2].GUID - WorldServiceLocator._Global_Constants.GUID_ITEM};");
                                if (Destroy)
                                {
                                    Items[bag].Items[slot2].Delete();
                                }
                                Items[bag].Items.Remove(slot2);
                                if (Update)
                                {
                                    SendItemUpdate(Items[bag]);
                                }
                                return;
                            }
                            slot2 = (byte)unchecked((uint)(slot2 + 1));
                        }
                    }
                    bag = (byte)unchecked((uint)(bag + 1));
                }
            }
            while (bag <= 22u);
            throw new ApplicationException("Unable to remove item because character doesn't have it in inventory or bags.");
        }

        public bool ItemADD(ref ItemObject Item)
        {
            var tmpEntry = Item.ItemEntry;
            checked
            {
                var tmpCount = (byte)Item.StackCount;
                if (tmpCount > Item.ItemInfo.Stackable)
                {
                    tmpCount = (byte)Item.ItemInfo.Stackable;
                }
                if (Item.ItemInfo.Unique > 0 && ItemCOUNT(Item.ItemEntry) >= Item.ItemInfo.Unique)
                {
                    return false;
                }
                if (ItemADD_AutoSlot(ref Item) && client != null)
                {
                    var aLLQUESTS = WorldServiceLocator._WorldServer.ALLQUESTS;
                    var objCharacter = this;
                    aLLQUESTS.OnQuestItemAdd(ref objCharacter, tmpEntry, tmpCount);
                    return true;
                }
                return false;
            }
        }

        public void ItemADD_BuyBack(ref ItemObject Item)
        {
            var Slot = WorldServiceLocator._Global_Constants.ITEM_SLOT_NULL;
            checked
            {
                var OldestTime = (int)WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now);
                var OldestSlot = WorldServiceLocator._Global_Constants.ITEM_SLOT_NULL;
                byte i = 69;
                do
                {
                    if (!Items.ContainsKey(i) || BuyBackTimeStamp[(byte)unchecked((uint)(i - 69))] == 0)
                    {
                        if (Slot == WorldServiceLocator._Global_Constants.ITEM_SLOT_NULL)
                        {
                            Slot = i;
                        }
                    }
                    else if (BuyBackTimeStamp[(byte)unchecked((uint)(i - 69))] < OldestTime)
                    {
                        OldestTime = BuyBackTimeStamp[(byte)unchecked((uint)(i - 69))];
                        OldestSlot = i;
                    }
                    i = (byte)unchecked((uint)(i + 1));
                }
                while (i <= 80u);
                if (Slot == WorldServiceLocator._Global_Constants.ITEM_SLOT_NULL)
                {
                    if (OldestSlot != WorldServiceLocator._Global_Constants.ITEM_SLOT_NULL)
                    {
                        return;
                    }
                    ItemREMOVE(0, OldestSlot, Destroy: true, Update: true);
                    Slot = OldestSlot;
                }
                var eSlot = (byte)unchecked((uint)(Slot - 69));
                BuyBackTimeStamp[eSlot] = (int)WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now);
                SetUpdateFlag(1238 + eSlot, BuyBackTimeStamp[eSlot]);
                SetUpdateFlag(1226 + eSlot, Item.ItemInfo.SellPrice * Item.StackCount);
                ItemSETSLOT(ref Item, 0, Slot);
            }
        }

        public bool ItemADD_AutoSlot(ref ItemObject Item)
        {
            if (Item.ItemInfo.Stackable > 1)
            {
                if (Item.ItemInfo.BagFamily == ITEM_BAG.KEYRING || Item.ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_KEY)
                {
                    byte slot = 81;
                    do
                    {
                        checked
                        {
                            if (Items.ContainsKey(slot) && Items[slot].ItemEntry == Item.ItemEntry && Items[slot].StackCount < Items[slot].ItemInfo.Stackable)
                            {
                                var stacked = Items[slot].ItemInfo.Stackable - Items[slot].StackCount;
                                if (stacked >= Item.StackCount)
                                {
                                    Items[slot].StackCount += Item.StackCount;
                                    Item.Delete();
                                    Item = Items[slot];
                                    Items[slot].Save();
                                    SendItemUpdate(Items[slot]);
                                    return true;
                                }
                                if (stacked > 0)
                                {
                                    Items[slot].StackCount += stacked;
                                    Item.StackCount -= stacked;
                                    Items[slot].Save();
                                    Item.Save();
                                    SendItemUpdate(Items[slot]);
                                    SendItemUpdate(Item);
                                    return ItemADD_AutoSlot(ref Item);
                                }
                            }
                            slot = (byte)unchecked((uint)(slot + 1));
                        }
                    }
                    while (slot <= 112u);
                }
                else if (Item.ItemInfo.BagFamily != 0)
                {
                    byte bag2 = 19;
                    do
                    {
                        checked
                        {
                            if (Items.ContainsKey(bag2) && Items[bag2].ItemInfo.SubClass != 0 && ((Items[bag2].ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_FOOD && Item.ItemInfo.BagFamily == ITEM_BAG.SOUL_SHARD) || (Items[bag2].ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_LIQUID && Item.ItemInfo.BagFamily == ITEM_BAG.HERB) || (Items[bag2].ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_POTION && Item.ItemInfo.BagFamily == ITEM_BAG.ENCHANTING) || (Items[bag2].ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_LIQUID && Item.ItemInfo.BagFamily == ITEM_BAG.ARROW) || (Items[bag2].ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_POTION && Item.ItemInfo.BagFamily == ITEM_BAG.BULLET)))
                            {
                                foreach (var slot5 in Items[bag2].Items)
                                {
                                    if (slot5.Value.ItemEntry == Item.ItemEntry && slot5.Value.StackCount < slot5.Value.ItemInfo.Stackable)
                                    {
                                        var stacked2 = slot5.Value.ItemInfo.Stackable - slot5.Value.StackCount;
                                        if (stacked2 >= Item.StackCount)
                                        {
                                            slot5.Value.StackCount += Item.StackCount;
                                            Item.Delete();
                                            Item = slot5.Value;
                                            slot5.Value.Save();
                                            SendItemUpdate(slot5.Value);
                                            SendItemUpdate(Items[bag2]);
                                            return true;
                                        }
                                        if (stacked2 > 0)
                                        {
                                            slot5.Value.StackCount += stacked2;
                                            Item.StackCount -= stacked2;
                                            slot5.Value.Save();
                                            Item.Save();
                                            SendItemUpdate(slot5.Value);
                                            SendItemUpdate(Item);
                                            SendItemUpdate(Items[bag2]);
                                            return ItemADD_AutoSlot(ref Item);
                                        }
                                    }
                                }
                            }
                            bag2 = (byte)unchecked((uint)(bag2 + 1));
                        }
                    }
                    while (bag2 <= 22u);
                }
                byte slot6 = 23;
                do
                {
                    checked
                    {
                        if (Items.ContainsKey(slot6) && Items[slot6].ItemEntry == Item.ItemEntry && Items[slot6].StackCount < Items[slot6].ItemInfo.Stackable)
                        {
                            var stacked3 = Items[slot6].ItemInfo.Stackable - Items[slot6].StackCount;
                            if (stacked3 >= Item.StackCount)
                            {
                                Items[slot6].StackCount += Item.StackCount;
                                Item.Delete();
                                Item = Items[slot6];
                                Items[slot6].Save();
                                SendItemUpdate(Items[slot6]);
                                return true;
                            }
                            if (stacked3 > 0)
                            {
                                Items[slot6].StackCount += stacked3;
                                Item.StackCount -= stacked3;
                                Items[slot6].Save();
                                Item.Save();
                                SendItemUpdate(Items[slot6]);
                                SendItemUpdate(Item);
                                return ItemADD_AutoSlot(ref Item);
                            }
                        }
                        slot6 = (byte)unchecked((uint)(slot6 + 1));
                    }
                }
                while (slot6 <= 38u);
                byte bag4 = 19;
                do
                {
                    checked
                    {
                        if (Items.ContainsKey(bag4))
                        {
                            foreach (var slot7 in Items[bag4].Items)
                            {
                                if (slot7.Value.ItemEntry == Item.ItemEntry && slot7.Value.StackCount < slot7.Value.ItemInfo.Stackable)
                                {
                                    var stacked4 = slot7.Value.ItemInfo.Stackable - slot7.Value.StackCount;
                                    if (stacked4 >= Item.StackCount)
                                    {
                                        slot7.Value.StackCount += Item.StackCount;
                                        Item.Delete();
                                        Item = slot7.Value;
                                        slot7.Value.Save();
                                        SendItemUpdate(slot7.Value);
                                        SendItemUpdate(Items[bag4]);
                                        return true;
                                    }
                                    if (stacked4 > 0)
                                    {
                                        slot7.Value.StackCount += stacked4;
                                        Item.StackCount -= stacked4;
                                        slot7.Value.Save();
                                        Item.Save();
                                        SendItemUpdate(slot7.Value);
                                        SendItemUpdate(Item);
                                        SendItemUpdate(Items[bag4]);
                                        return ItemADD_AutoSlot(ref Item);
                                    }
                                }
                            }
                        }
                        bag4 = (byte)unchecked((uint)(bag4 + 1));
                    }
                }
                while (bag4 <= 22u);
            }
            if (Item.ItemInfo.BagFamily == ITEM_BAG.KEYRING || Item.ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_KEY)
            {
                byte slot8 = 81;
                do
                {
                    if (!Items.ContainsKey(slot8))
                    {
                        return ItemSETSLOT(ref Item, 0, slot8);
                    }
                    checked
                    {
                        slot8 = (byte)unchecked((uint)(slot8 + 1));
                    }
                }
                while (slot8 <= 112u);
            }
            else if (Item.ItemInfo.BagFamily != 0)
            {
                byte bag3 = 19;
                do
                {
                    checked
                    {
                        if (Items.ContainsKey(bag3) && Items[bag3].ItemInfo.SubClass != 0 && ((Items[bag3].ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_CONTAINER && Items[bag3].ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_FOOD && Item.ItemInfo.BagFamily == ITEM_BAG.SOUL_SHARD) || (Items[bag3].ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_CONTAINER && Items[bag3].ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_LIQUID && Item.ItemInfo.BagFamily == ITEM_BAG.HERB) || (Items[bag3].ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_CONTAINER && Items[bag3].ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_POTION && Item.ItemInfo.BagFamily == ITEM_BAG.ENCHANTING) || (Items[bag3].ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_QUIVER && Items[bag3].ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_LIQUID && Item.ItemInfo.BagFamily == ITEM_BAG.ARROW) || (Items[bag3].ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_QUIVER && Items[bag3].ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_POTION && Item.ItemInfo.BagFamily == ITEM_BAG.BULLET)))
                        {
                            var b = (byte)(Items[bag3].ItemInfo.ContainerSlots - 1);
                            byte slot4 = 0;
                            while (slot4 <= (uint)b)
                            {
                                if (!Items[bag3].Items.ContainsKey(slot4))
                                {
                                    return ItemSETSLOT(ref Item, bag3, slot4);
                                }
                                slot4 = (byte)unchecked((uint)(slot4 + 1));
                            }
                        }
                        bag3 = (byte)unchecked((uint)(bag3 + 1));
                    }
                }
                while (bag3 <= 22u);
            }
            byte slot3 = 23;
            do
            {
                if (!Items.ContainsKey(slot3))
                {
                    return ItemSETSLOT(ref Item, 0, slot3);
                }
                checked
                {
                    slot3 = (byte)unchecked((uint)(slot3 + 1));
                }
            }
            while (slot3 <= 38u);
            byte bag = 19;
            do
            {
                checked
                {
                    if (Items.ContainsKey(bag) && Items[bag].ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_CONSUMABLE)
                    {
                        var b2 = (byte)(Items[bag].ItemInfo.ContainerSlots - 1);
                        byte slot2 = 0;
                        while (slot2 <= (uint)b2)
                        {
                            if (!Items[bag].Items.ContainsKey(slot2) && ItemCANEQUIP(Item, bag, slot2) == InventoryChangeFailure.EQUIP_ERR_OK)
                            {
                                return ItemSETSLOT(ref Item, bag, slot2);
                            }
                            slot2 = (byte)unchecked((uint)(slot2 + 1));
                        }
                    }
                    bag = (byte)unchecked((uint)(bag + 1));
                }
            }
            while (bag <= 22u);
            var wS_Items = WorldServiceLocator._WS_Items;
            var objCharacter = this;
            wS_Items.SendInventoryChangeFailure(ref objCharacter, InventoryChangeFailure.EQUIP_ERR_INVENTORY_FULL, 0uL, 0uL);
            return false;
        }

        public bool ItemADD_AutoBag(ref ItemObject Item, byte dstBag)
        {
            checked
            {
                CharacterObject objCharacter;
                if (dstBag == 0)
                {
                    if (Item.ItemInfo.Stackable > 1)
                    {
                        byte slot = 23;
                        do
                        {
                            if (Items[slot].ItemEntry == Item.ItemEntry && Items[slot].StackCount < Items[slot].ItemInfo.Stackable)
                            {
                                var stacked2 = (byte)(Items[slot].ItemInfo.Stackable - Items[slot].StackCount);
                                if (stacked2 >= Item.StackCount)
                                {
                                    Items[slot].StackCount += Item.StackCount;
                                    Item.Delete();
                                    Item = Items[slot];
                                    Items[slot].Save();
                                    SendItemUpdate(Items[slot]);
                                    return true;
                                }
                                if (stacked2 > 0)
                                {
                                    Items[slot].StackCount += stacked2;
                                    Item.StackCount -= stacked2;
                                    Items[slot].Save();
                                    Item.Save();
                                    SendItemUpdate(Items[slot]);
                                    SendItemUpdate(Item);
                                    return ItemADD_AutoBag(ref Item, dstBag);
                                }
                            }
                            slot = (byte)unchecked((uint)(slot + 1));
                        }
                        while (slot <= 38u);
                    }
                    if (Item.ItemInfo.BagFamily == ITEM_BAG.KEYRING)
                    {
                        byte slot3 = 81;
                        do
                        {
                            if (!Items.ContainsKey(slot3))
                            {
                                return ItemSETSLOT(ref Item, 0, slot3);
                            }
                            slot3 = (byte)unchecked((uint)(slot3 + 1));
                        }
                        while (slot3 <= 112u);
                    }
                    byte slot4 = 23;
                    do
                    {
                        if (!Items.ContainsKey(slot4))
                        {
                            return ItemSETSLOT(ref Item, 0, slot4);
                        }
                        slot4 = (byte)unchecked((uint)(slot4 + 1));
                    }
                    while (slot4 <= 38u);
                }
                else if (Items.ContainsKey(dstBag))
                {
                    if ((Items[dstBag].ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_CONTAINER && Items[dstBag].ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_FOOD && Item.ItemInfo.BagFamily != ITEM_BAG.SOUL_SHARD) || (Items[dstBag].ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_CONTAINER && Items[dstBag].ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_LIQUID && Item.ItemInfo.BagFamily != ITEM_BAG.HERB) || (Items[dstBag].ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_CONTAINER && Items[dstBag].ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_POTION && Item.ItemInfo.BagFamily != ITEM_BAG.ENCHANTING) || (Items[dstBag].ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_QUIVER && Items[dstBag].ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_LIQUID && Item.ItemInfo.BagFamily != ITEM_BAG.ARROW) || (Items[dstBag].ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_QUIVER && Items[dstBag].ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_POTION && Item.ItemInfo.BagFamily != ITEM_BAG.BULLET))
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "{0} - {1} - {2}", Items[dstBag].ItemInfo.ObjectClass, Items[dstBag].ItemInfo.SubClass, Item.ItemInfo.BagFamily);
                        var wS_Items = WorldServiceLocator._WS_Items;
                        objCharacter = this;
                        wS_Items.SendInventoryChangeFailure(ref objCharacter, InventoryChangeFailure.EQUIP_ERR_ITEM_DOESNT_GO_INTO_BAG, Item.GUID, 0uL);
                        return false;
                    }
                    if (Item.ItemInfo.Stackable > 1)
                    {
                        foreach (var i in Items[dstBag].Items)
                        {
                            if (i.Value.ItemEntry == Item.ItemEntry && i.Value.StackCount < i.Value.ItemInfo.Stackable)
                            {
                                var stacked = (byte)(i.Value.ItemInfo.Stackable - i.Value.StackCount);
                                if (stacked >= Item.StackCount)
                                {
                                    i.Value.StackCount += Item.StackCount;
                                    Item.Delete();
                                    Item = i.Value;
                                    i.Value.Save();
                                    SendItemUpdate(i.Value);
                                    return true;
                                }
                                if (stacked > 0)
                                {
                                    i.Value.StackCount += stacked;
                                    Item.StackCount -= stacked;
                                    i.Value.Save();
                                    Item.Save();
                                    SendItemUpdate(i.Value);
                                    SendItemUpdate(Item);
                                    return ItemADD_AutoBag(ref Item, dstBag);
                                }
                            }
                        }
                    }
                    var b = (byte)(Items[dstBag].ItemInfo.ContainerSlots - 1);
                    byte slot2 = 0;
                    while (slot2 <= (uint)b)
                    {
                        if (!Items[dstBag].Items.ContainsKey(slot2) && ItemCANEQUIP(Item, dstBag, slot2) == InventoryChangeFailure.EQUIP_ERR_OK)
                        {
                            return ItemSETSLOT(ref Item, dstBag, slot2);
                        }
                        slot2 = (byte)unchecked((uint)(slot2 + 1));
                    }
                }
                var wS_Items2 = WorldServiceLocator._WS_Items;
                objCharacter = this;
                wS_Items2.SendInventoryChangeFailure(ref objCharacter, InventoryChangeFailure.EQUIP_ERR_BAG_FULL, Item.GUID, 0uL);
                return false;
            }
        }

        public bool ItemSETSLOT(ref ItemObject Item, byte dstBag, byte dstSlot)
        {
            if (Item.ItemInfo.Bonding == 1 && !Item.IsSoulBound)
            {
                var obj = Item;
                WS_Network.ClientClass clientClass = null;
                obj.SoulbindItem(clientClass);
            }
            if ((Item.ItemInfo.Bonding == 4 || Item.ItemInfo.Bonding == 5) && !Item.IsSoulBound)
            {
                var obj2 = Item;
                WS_Network.ClientClass clientClass = null;
                obj2.SoulbindItem(clientClass);
            }
            checked
            {
                if (dstBag == 0)
                {
                    Items[dstSlot] = Item;
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update($"UPDATE characters_inventory SET item_slot = {dstSlot}, item_bag = {GUID}, item_stackCount = {Item.StackCount} WHERE item_guid = {Item.GUID - WorldServiceLocator._Global_Constants.GUID_ITEM};");
                    SetUpdateFlag(486 + (dstSlot * 2), Item.GUID);
                    if (dstSlot < 19u)
                    {
                        SetUpdateFlag(260 + (dstSlot * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE), Item.ItemEntry);
                        SetUpdateFlag(268 + (dstSlot * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE), Item.RandomProperties);
                        if (Item.ItemInfo.Bonding == 2 && !Item.IsSoulBound)
                        {
                            var obj3 = Item;
                            WS_Network.ClientClass clientClass = null;
                            obj3.SoulbindItem(clientClass);
                        }
                    }
                }
                else
                {
                    Items[dstBag].Items[dstSlot] = Item;
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update($"UPDATE characters_inventory SET item_slot = {dstSlot}, item_bag = {Items[dstBag].GUID}, item_stackCount = {Item.StackCount} WHERE item_guid = {Item.GUID - WorldServiceLocator._Global_Constants.GUID_ITEM};");
                }
                if (client != null)
                {
                    SendItemAndCharacterUpdate(Item, 2);
                    if (dstBag > 0)
                    {
                        SendItemUpdate(Items[dstBag]);
                    }
                }
                return true;
            }
        }

        public int ItemCOUNT(int ItemEntry, bool Equipped = false)
        {
            var count = 0;
            byte EndSlot = 39;
            if (Equipped)
            {
                EndSlot = 19;
            }
            checked
            {
                var b = (byte)(EndSlot - 1);
                byte slot3 = 0;
                while (slot3 <= (uint)b)
                {
                    if (Items.ContainsKey(slot3) && Items[slot3].ItemEntry == ItemEntry)
                    {
                        count += Items[slot3].StackCount;
                    }
                    slot3 = (byte)unchecked((uint)(slot3 + 1));
                }
                if (Equipped)
                {
                    return count;
                }
                byte slot2 = 81;
                do
                {
                    if (Items.ContainsKey(slot2) && Items[slot2].ItemEntry == ItemEntry)
                    {
                        count += Items[slot2].StackCount;
                    }
                    slot2 = (byte)unchecked((uint)(slot2 + 1));
                }
                while (slot2 <= 112u);
                byte bag = 19;
                do
                {
                    if (Items.ContainsKey(bag))
                    {
                        var b2 = (byte)(Items[bag].ItemInfo.ContainerSlots - 1);
                        byte slot = 0;
                        while (slot <= (uint)b2)
                        {
                            if (Items[bag].Items.ContainsKey(slot) && Items[bag].Items[slot].ItemEntry == ItemEntry)
                            {
                                count += Items[bag].Items[slot].StackCount;
                            }
                            slot = (byte)unchecked((uint)(slot + 1));
                        }
                    }
                    bag = (byte)unchecked((uint)(bag + 1));
                }
                while (bag <= 22u);
                return count;
            }
        }

        public bool ItemCONSUME(int ItemEntry, int Count)
        {
            byte slot = 0;
            do
            {
                checked
                {
                    if (Items.ContainsKey(slot) && Items[slot].ItemEntry == ItemEntry)
                    {
                        if (Items[slot].StackCount > Count)
                        {
                            Items[slot].StackCount -= Count;
                            Items[slot].Save(saveAll: false);
                            SendItemUpdate(Items[slot]);
                            return true;
                        }
                        Count -= Items[slot].StackCount;
                        ItemREMOVE(0, slot, Destroy: true, Update: true);
                        if (Count <= 0)
                        {
                            return true;
                        }
                    }
                    slot = (byte)unchecked((uint)(slot + 1));
                }
            }
            while (slot <= 38u);
            byte slot2 = 81;
            do
            {
                checked
                {
                    if (Items.ContainsKey(slot2) && Items[slot2].ItemEntry == ItemEntry)
                    {
                        if (Items[slot2].StackCount > Count)
                        {
                            Items[slot2].StackCount -= Count;
                            Items[slot2].Save(saveAll: false);
                            SendItemUpdate(Items[slot2]);
                            return true;
                        }
                        Count -= Items[slot2].StackCount;
                        ItemREMOVE(0, slot2, Destroy: true, Update: true);
                        if (Count <= 0)
                        {
                            return true;
                        }
                    }
                    slot2 = (byte)unchecked((uint)(slot2 + 1));
                }
            }
            while (slot2 <= 112u);
            byte bag = 19;
            do
            {
                checked
                {
                    if (Items.ContainsKey(bag))
                    {
                        var b = (byte)(Items[bag].ItemInfo.ContainerSlots - 1);
                        byte slot3 = 0;
                        while (slot3 <= (uint)b)
                        {
                            if (Items[bag].Items.ContainsKey(slot3) && Items[bag].Items[slot3].ItemEntry == ItemEntry)
                            {
                                if (Items[bag].Items[slot3].StackCount > Count)
                                {
                                    Items[bag].Items[slot3].StackCount -= Count;
                                    Items[bag].Items[slot3].Save(saveAll: false);
                                    SendItemUpdate(Items[bag].Items[slot3]);
                                    return true;
                                }
                                Count -= Items[bag].Items[slot3].StackCount;
                                ItemREMOVE(bag, slot3, Destroy: true, Update: true);
                                if (Count <= 0)
                                {
                                    return true;
                                }
                            }
                            slot3 = (byte)unchecked((uint)(slot3 + 1));
                        }
                    }
                    bag = (byte)unchecked((uint)(bag + 1));
                }
            }
            while (bag <= 22u);
            return false;
        }

        public int ItemFREESLOTS()
        {
            var foundFreeSlots = 0;
            byte slot2 = 23;
            do
            {
                checked
                {
                    if (!Items.ContainsKey(slot2))
                    {
                        foundFreeSlots++;
                    }
                    slot2 = (byte)unchecked((uint)(slot2 + 1));
                }
            }
            while (slot2 <= 38u);
            byte bag = 19;
            do
            {
                checked
                {
                    if (Items.ContainsKey(bag))
                    {
                        var b = (byte)(Items[bag].ItemInfo.ContainerSlots - 1);
                        byte slot = 0;
                        while (slot <= (uint)b)
                        {
                            if (!Items[bag].Items.ContainsKey(slot))
                            {
                                foundFreeSlots++;
                            }
                            slot = (byte)unchecked((uint)(slot + 1));
                        }
                    }
                    bag = (byte)unchecked((uint)(bag + 1));
                }
            }
            while (bag <= 22u);
            return foundFreeSlots;
        }

        public InventoryChangeFailure ItemCANEQUIP(ItemObject Item, byte dstBag, byte dstSlot)
        {
            if (DEAD)
            {
                return InventoryChangeFailure.EQUIP_ERR_YOU_ARE_DEAD;
            }
            var ItemInfo = Item.ItemInfo;
            try
            {
                if (dstBag == 0)
                {
                    switch (dstSlot)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                        case 11:
                        case 12:
                        case 13:
                        case 14:
                        case 15:
                        case 16:
                        case 17:
                        case 18:
                            if (ItemInfo.IsContainer)
                            {
                                return InventoryChangeFailure.EQUIP_ERR_ITEM_CANT_BE_EQUIPPED;
                            }
                            checked
                            {
                                if (!WorldServiceLocator._Functions.HaveFlag(ItemInfo.AvailableClasses, (byte)((int)Classe - 1)))
                                {
                                    return InventoryChangeFailure.EQUIP_ERR_YOU_CAN_NEVER_USE_THAT_ITEM;
                                }
                                if (!WorldServiceLocator._Functions.HaveFlag(ItemInfo.AvailableRaces, (byte)((int)Race - 1)))
                                {
                                    return InventoryChangeFailure.EQUIP_ERR_YOU_CAN_NEVER_USE_THAT_ITEM2;
                                }
                                if (ItemInfo.ReqLevel > Level)
                                {
                                    return InventoryChangeFailure.EQUIP_ERR_YOU_MUST_REACH_LEVEL_N;
                                }
                                var tmp = false;
                                var getSlots = ItemInfo.GetSlots;
                                foreach (var SlotVal in getSlots)
                                {
                                    if (dstSlot == SlotVal)
                                    {
                                        tmp = true;
                                    }
                                }
                                if (!tmp)
                                {
                                    return InventoryChangeFailure.EQUIP_ERR_ITEM_DOESNT_GO_TO_SLOT;
                                }
                                if (dstSlot == 15 && ItemInfo.InventoryType == INVENTORY_TYPES.INVTYPE_TWOHAND_WEAPON && Items.ContainsKey(16))
                                {
                                    return InventoryChangeFailure.EQUIP_ERR_CANT_EQUIP_WITH_TWOHANDED;
                                }
                                if (dstSlot == 16 && Items.ContainsKey(15) && Items[15].ItemInfo.InventoryType == INVENTORY_TYPES.INVTYPE_TWOHAND_WEAPON)
                                {
                                    return InventoryChangeFailure.EQUIP_ERR_CANT_EQUIP_WITH_TWOHANDED;
                                }
                                if (dstSlot == 16 && ItemInfo.InventoryType == INVENTORY_TYPES.INVTYPE_WEAPON && !Skills.ContainsKey(118))
                                {
                                    return InventoryChangeFailure.EQUIP_ERR_CANT_DUAL_WIELD;
                                }
                                if (ItemInfo.GetReqSkill != 0 && !Skills.ContainsKey(ItemInfo.GetReqSkill))
                                {
                                    return InventoryChangeFailure.EQUIP_ERR_NO_REQUIRED_PROFICIENCY;
                                }
                                if (ItemInfo.GetReqSpell != 0 && !Spells.ContainsKey(ItemInfo.GetReqSpell))
                                {
                                    return InventoryChangeFailure.EQUIP_ERR_NO_REQUIRED_PROFICIENCY;
                                }
                                if (ItemInfo.ReqSkill != 0)
                                {
                                    if (!Skills.ContainsKey(ItemInfo.ReqSkill))
                                    {
                                        return InventoryChangeFailure.EQUIP_ERR_NO_REQUIRED_PROFICIENCY;
                                    }
                                    if (Skills[ItemInfo.ReqSkill].Current < ItemInfo.ReqSkillRank)
                                    {
                                        return InventoryChangeFailure.EQUIP_ERR_SKILL_ISNT_HIGH_ENOUGH;
                                    }
                                }
                                if (ItemInfo.ReqSpell != 0 && !Spells.ContainsKey(ItemInfo.ReqSpell))
                                {
                                    return InventoryChangeFailure.EQUIP_ERR_NO_REQUIRED_PROFICIENCY;
                                }
                            }
                            if (ItemInfo.ReqHonorRank != 0 && (int)HonorHighestRank < ItemInfo.ReqHonorRank)
                            {
                                return InventoryChangeFailure.EQUIP_ITEM_RANK_NOT_ENOUGH;
                            }
                            if (ItemInfo.ReqFaction != 0 && (int)client.Character.GetReputation(ItemInfo.ReqFaction) <= ItemInfo.ReqFactionLevel)
                            {
                                return InventoryChangeFailure.EQUIP_ITEM_REPUTATION_NOT_ENOUGH;
                            }
                            return InventoryChangeFailure.EQUIP_ERR_OK;

                        case 19:
                        case 20:
                        case 21:
                        case 22:
                            if (!ItemInfo.IsContainer)
                            {
                                return InventoryChangeFailure.EQUIP_ERR_NOT_A_BAG;
                            }
                            if (!Item.IsFree)
                            {
                                return InventoryChangeFailure.EQUIP_ERR_NONEMPTY_BAG_OVER_OTHER_BAG;
                            }
                            return InventoryChangeFailure.EQUIP_ERR_OK;

                        case 23:
                        case 24:
                        case 25:
                        case 26:
                        case 27:
                        case 28:
                        case 29:
                        case 30:
                        case 31:
                        case 32:
                        case 33:
                        case 34:
                        case 35:
                        case 36:
                        case 37:
                        case 38:
                            if (ItemInfo.IsContainer)
                            {
                                return Item.IsFree ? InventoryChangeFailure.EQUIP_ERR_OK : InventoryChangeFailure.EQUIP_ERR_CAN_ONLY_DO_WITH_EMPTY_BAGS;
                            }
                            return InventoryChangeFailure.EQUIP_ERR_OK;

                        case 39:
                        case 40:
                        case 41:
                        case 42:
                        case 43:
                        case 44:
                        case 45:
                        case 46:
                        case 47:
                        case 48:
                        case 49:
                        case 50:
                        case 51:
                        case 52:
                        case 53:
                        case 54:
                        case 55:
                        case 56:
                        case 57:
                        case 58:
                        case 59:
                        case 60:
                        case 61:
                        case 62:
                            if (ItemInfo.IsContainer)
                            {
                                return Item.IsFree ? InventoryChangeFailure.EQUIP_ERR_OK : InventoryChangeFailure.EQUIP_ERR_CAN_ONLY_DO_WITH_EMPTY_BAGS;
                            }
                            return InventoryChangeFailure.EQUIP_ERR_OK;

                        case 63:
                        case 64:
                        case 65:
                        case 66:
                        case 67:
                        case 68:
                            if (dstSlot >= (uint)checked((byte)unchecked((uint)(63 + Items_AvailableBankSlots))))
                            {
                                return InventoryChangeFailure.EQUIP_ERR_MUST_PURCHASE_THAT_BAG_SLOT;
                            }
                            if (!ItemInfo.IsContainer)
                            {
                                return InventoryChangeFailure.EQUIP_ERR_NOT_A_BAG;
                            }
                            if (!Item.IsFree)
                            {
                                return InventoryChangeFailure.EQUIP_ERR_NONEMPTY_BAG_OVER_OTHER_BAG;
                            }
                            return InventoryChangeFailure.EQUIP_ERR_OK;

                        case 69:
                        case 70:
                        case 71:
                        case 72:
                        case 73:
                        case 74:
                        case 75:
                        case 76:
                        case 77:
                        case 78:
                        case 79:
                        case 80:
                        case 81:
                        case 82:
                        case 83:
                        case 84:
                        case 85:
                        case 86:
                        case 87:
                        case 88:
                        case 89:
                        case 90:
                        case 91:
                        case 92:
                        case 93:
                        case 94:
                        case 95:
                        case 96:
                        case 97:
                        case 98:
                        case 99:
                        case 100:
                        case 101:
                        case 102:
                        case 103:
                        case 104:
                        case 105:
                        case 106:
                        case 107:
                        case 108:
                        case 109:
                        case 110:
                        case 111:
                        case 112:
                            if (ItemInfo.BagFamily != ITEM_BAG.KEYRING && ItemInfo.ObjectClass != ITEM_CLASS.ITEM_CLASS_KEY)
                            {
                                return InventoryChangeFailure.EQUIP_ERR_ITEM_DOESNT_GO_TO_SLOT;
                            }
                            return InventoryChangeFailure.EQUIP_ERR_OK;

                        default:
                            return InventoryChangeFailure.EQUIP_ERR_ITEM_CANT_BE_EQUIPPED;
                    }
                }
                if (!Items.ContainsKey(dstBag))
                {
                    return InventoryChangeFailure.EQUIP_ERR_ITEM_DOESNT_GO_INTO_BAG;
                }
                if (ItemInfo.IsContainer)
                {
                    return Item.IsFree ? InventoryChangeFailure.EQUIP_ERR_OK : InventoryChangeFailure.EQUIP_ERR_CAN_ONLY_DO_WITH_EMPTY_BAGS;
                }
                if (Items[dstBag].ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_QUIVER)
                {
                    if (ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_PROJECTILE)
                    {
                        return Items[dstBag].ItemInfo.SubClass != ItemInfo.SubClass
                            ? InventoryChangeFailure.EQUIP_ERR_ITEM_DOESNT_GO_INTO_BAG
                            : InventoryChangeFailure.EQUIP_ERR_OK;
                    }
                    return InventoryChangeFailure.EQUIP_ERR_ONLY_AMMO_CAN_GO_HERE;
                }
                return InventoryChangeFailure.EQUIP_ERR_OK;
            }
            catch (Exception ex)
            {
                ProjectData.SetProjectError(ex);
                var err = ex;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "[{0}:{1}] Unable to equip item. {2} {3}", client.IP, client.Port, Environment.NewLine, err.ToString());
                var ItemCANEQUIP = InventoryChangeFailure.EQUIP_ERR_CANT_DO_RIGHT_NOW;
                ProjectData.ClearProjectError();
                return ItemCANEQUIP;
            }
        }

        public bool ItemSTACK(byte srcBag, byte srcSlot, byte dstBag, byte dstSlot)
        {
            var srcItem = (srcBag == 0) ? Items[srcSlot] : Items[srcBag].Items[srcSlot];
            var dstItem = (dstBag == 0) ? Items[dstSlot] : Items[dstBag].Items[dstSlot];
            if ((srcItem.StackCount == dstItem.ItemInfo.Stackable) | (dstItem.StackCount == dstItem.ItemInfo.Stackable))
            {
                return false;
            }
            checked
            {
                if (srcItem.ItemEntry == dstItem.ItemEntry && dstItem.StackCount + srcItem.StackCount <= dstItem.ItemInfo.Stackable)
                {
                    dstItem.StackCount += srcItem.StackCount;
                    ItemREMOVE(srcBag, srcSlot, Destroy: true, Update: true);
                    SendItemUpdate(dstItem);
                    if (dstBag > 0)
                    {
                        SendItemUpdate(Items[dstBag]);
                    }
                    dstItem.Save(saveAll: false);
                    return true;
                }
                if (srcItem.ItemEntry == dstItem.ItemEntry)
                {
                    srcItem.StackCount -= dstItem.ItemInfo.Stackable - dstItem.StackCount;
                    dstItem.StackCount = dstItem.ItemInfo.Stackable;
                    SendItemUpdate(dstItem);
                    if (dstBag > 0)
                    {
                        SendItemUpdate(Items[dstBag]);
                    }
                    SendItemUpdate(srcItem);
                    if (srcBag > 0)
                    {
                        SendItemUpdate(Items[srcBag]);
                    }
                    srcItem.Save(saveAll: false);
                    dstItem.Save(saveAll: false);
                    return true;
                }
                return false;
            }
        }

        public void ItemSPLIT(byte srcBag, byte srcSlot, byte dstBag, byte dstSlot, int Count)
        {
            ItemObject dstItem = null;
            ItemObject srcItem;
            if (srcBag == 0)
            {
                if (!client.Character.Items.ContainsKey(srcSlot))
                {
                    Packets.PacketClass EQUIP_ERR_ITEM_NOT_FOUND = new(Opcodes.SMSG_INVENTORY_CHANGE_FAILURE);
                    try
                    {
                        EQUIP_ERR_ITEM_NOT_FOUND.AddInt8(23);
                        EQUIP_ERR_ITEM_NOT_FOUND.AddUInt64(0uL);
                        EQUIP_ERR_ITEM_NOT_FOUND.AddUInt64(0uL);
                        EQUIP_ERR_ITEM_NOT_FOUND.AddInt8(0);
                        client.Send(ref EQUIP_ERR_ITEM_NOT_FOUND);
                    }
                    finally
                    {
                        EQUIP_ERR_ITEM_NOT_FOUND.Dispose();
                    }
                    return;
                }
                srcItem = Items[srcSlot];
            }
            else
            {
                if (!client.Character.Items[srcBag].Items.ContainsKey(srcSlot))
                {
                    Packets.PacketClass EQUIP_ERR_ITEM_NOT_FOUND2 = new(Opcodes.SMSG_INVENTORY_CHANGE_FAILURE);
                    try
                    {
                        EQUIP_ERR_ITEM_NOT_FOUND2.AddInt8(23);
                        EQUIP_ERR_ITEM_NOT_FOUND2.AddUInt64(0uL);
                        EQUIP_ERR_ITEM_NOT_FOUND2.AddUInt64(0uL);
                        EQUIP_ERR_ITEM_NOT_FOUND2.AddInt8(0);
                        client.Send(ref EQUIP_ERR_ITEM_NOT_FOUND2);
                    }
                    finally
                    {
                        EQUIP_ERR_ITEM_NOT_FOUND2.Dispose();
                    }
                    return;
                }
                srcItem = Items[srcBag].Items[srcSlot];
            }
            if (dstBag == 0)
            {
                if (Items.ContainsKey(dstSlot))
                {
                    dstItem = Items[dstSlot];
                }
            }
            else if (Items[dstBag].Items.ContainsKey(dstSlot))
            {
                dstItem = Items[dstBag].Items[dstSlot];
            }
            checked
            {
                if (dstSlot == byte.MaxValue)
                {
                    Packets.PacketClass notHandledYet = new(Opcodes.SMSG_INVENTORY_CHANGE_FAILURE);
                    try
                    {
                        notHandledYet.AddInt8(27);
                        notHandledYet.AddUInt64(srcItem.GUID);
                        notHandledYet.AddUInt64(dstItem.GUID);
                        notHandledYet.AddInt8(0);
                        client.Send(ref notHandledYet);
                    }
                    finally
                    {
                        notHandledYet.Dispose();
                    }
                }
                else if (Count == srcItem.StackCount)
                {
                    ItemSWAP(srcBag, srcSlot, dstBag, dstSlot);
                }
                else if (Count > srcItem.StackCount)
                {
                    Packets.PacketClass EQUIP_ERR_TRIED_TO_SPLIT_MORE_THAN_COUNT = new(Opcodes.SMSG_INVENTORY_CHANGE_FAILURE);
                    try
                    {
                        EQUIP_ERR_TRIED_TO_SPLIT_MORE_THAN_COUNT.AddInt8(26);
                        EQUIP_ERR_TRIED_TO_SPLIT_MORE_THAN_COUNT.AddUInt64(srcItem.GUID);
                        EQUIP_ERR_TRIED_TO_SPLIT_MORE_THAN_COUNT.AddUInt64(0uL);
                        EQUIP_ERR_TRIED_TO_SPLIT_MORE_THAN_COUNT.AddInt8(0);
                        client.Send(ref EQUIP_ERR_TRIED_TO_SPLIT_MORE_THAN_COUNT);
                    }
                    finally
                    {
                        EQUIP_ERR_TRIED_TO_SPLIT_MORE_THAN_COUNT.Dispose();
                    }
                }
                else if (dstItem == null)
                {
                    srcItem.StackCount -= Count;
                    ItemObject tmpItem = new(srcItem.ItemEntry, GUID)
                    {
                        StackCount = Count
                    };
                    dstItem = tmpItem;
                    tmpItem.Save();
                    ItemSETSLOT(ref tmpItem, dstBag, dstSlot);
                    Packets.UpdatePacketClass SMSG_UPDATE_OBJECT = new();
                    Packets.UpdateClass tmpUpdate = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_ITEM);
                    try
                    {
                        tmpItem.FillAllUpdateFlags(ref tmpUpdate);
                        var updateClass = tmpUpdate;
                        Packets.PacketClass packet = SMSG_UPDATE_OBJECT;
                        updateClass.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, ref tmpItem);
                        var clientClass = client;
                        packet = SMSG_UPDATE_OBJECT;
                        clientClass.Send(ref packet);
                    }
                    finally
                    {
                        SMSG_UPDATE_OBJECT.Dispose();
                        tmpUpdate.Dispose();
                    }
                    SendItemUpdate(srcItem);
                    SendItemUpdate(dstItem);
                    if (srcBag != 0)
                    {
                        SendItemUpdate(Items[srcBag]);
                        Items[srcBag].Save(saveAll: false);
                    }
                    if (dstBag != 0)
                    {
                        SendItemUpdate(Items[dstBag]);
                        Items[dstBag].Save(saveAll: false);
                    }
                    srcItem.Save(saveAll: false);
                    dstItem.Save(saveAll: false);
                }
                else if (srcItem.ItemEntry == dstItem.ItemEntry && dstItem.StackCount + Count <= dstItem.ItemInfo.Stackable)
                {
                    srcItem.StackCount -= Count;
                    dstItem.StackCount += Count;
                    SendItemUpdate(srcItem);
                    SendItemUpdate(dstItem);
                    if (srcBag != 0)
                    {
                        SendItemUpdate(Items[srcBag]);
                        Items[srcBag].Save(saveAll: false);
                    }
                    if (dstBag != 0)
                    {
                        SendItemUpdate(Items[dstBag]);
                        Items[dstBag].Save(saveAll: false);
                    }
                    srcItem.Save(saveAll: false);
                    dstItem.Save(saveAll: false);
                    Packets.PacketClass EQUIP_ERR_OK = new(Opcodes.SMSG_INVENTORY_CHANGE_FAILURE);
                    try
                    {
                        EQUIP_ERR_OK.AddInt8(0);
                        EQUIP_ERR_OK.AddUInt64(srcItem.GUID);
                        EQUIP_ERR_OK.AddUInt64(dstItem.GUID);
                        EQUIP_ERR_OK.AddInt8(0);
                        client.Send(ref EQUIP_ERR_OK);
                    }
                    finally
                    {
                        EQUIP_ERR_OK.Dispose();
                    }
                }
                else
                {
                    Packets.PacketClass response = new(Opcodes.SMSG_INVENTORY_CHANGE_FAILURE);
                    try
                    {
                        response.AddInt8(27);
                        response.AddUInt64(srcItem.GUID);
                        response.AddUInt64(dstItem.GUID);
                        response.AddInt8(0);
                        client.Send(ref response);
                    }
                    catch (Exception projectError)
                    {
                        ProjectData.SetProjectError(projectError);
                        response.Dispose();
                        ProjectData.ClearProjectError();
                    }
                }
            }
        }

        public void ItemSWAP(byte srcBag, byte srcSlot, byte dstBag, byte dstSlot)
        {
            if (DEAD)
            {
                var wS_Items = WorldServiceLocator._WS_Items;
                var objCharacter = this;
                wS_Items.SendInventoryChangeFailure(ref objCharacter, InventoryChangeFailure.EQUIP_ERR_YOU_ARE_DEAD, ItemGetGUID(srcBag, srcSlot), ItemGetGUID(dstBag, dstSlot));
                return;
            }
            byte errCode = 21;
            if ((srcBag == 0 && srcSlot == dstBag && dstBag > 0) || (dstBag == 0 && dstSlot == srcBag && srcBag > 0))
            {
                var wS_Items2 = WorldServiceLocator._WS_Items;
                var objCharacter = this;
                wS_Items2.SendInventoryChangeFailure(ref objCharacter, (InventoryChangeFailure)errCode, Items[srcSlot].GUID, 0uL);
                return;
            }
            checked
            {
                try
                {
                    unchecked
                    {
                        if (srcBag > 0 && dstBag > 0)
                        {
                            if (!Items[srcBag].Items.ContainsKey(srcSlot))
                            {
                                errCode = 22;
                                return;
                            }
                            errCode = (byte)ItemCANEQUIP(Items[srcBag].Items[srcSlot], dstBag, dstSlot);
                            if (errCode == 0 && Items[dstBag].Items.ContainsKey(dstSlot))
                            {
                                errCode = (byte)ItemCANEQUIP(Items[dstBag].Items[dstSlot], srcBag, srcSlot);
                            }
                            if (errCode != 0)
                            {
                                return;
                            }
                            if (!Items[dstBag].Items.ContainsKey(dstSlot))
                            {
                                if (!Items[srcBag].Items.ContainsKey(srcSlot))
                                {
                                    Items[dstBag].Items.Remove(dstSlot);
                                    Items[srcBag].Items.Remove(srcSlot);
                                }
                                else
                                {
                                    Items[dstBag].Items[dstSlot] = Items[srcBag].Items[srcSlot];
                                    Items[srcBag].Items.Remove(srcSlot);
                                }
                                goto IL_02e9;
                            }
                            if (!Items[srcBag].Items.ContainsKey(srcSlot))
                            {
                                Items[srcBag].Items[srcSlot] = Items[dstBag].Items[dstSlot];
                                Items[dstBag].Items.Remove(dstSlot);
                                goto IL_02e9;
                            }
                            if (ItemSTACK(srcBag, srcSlot, dstBag, dstSlot))
                            {
                                return;
                            }
                            var tmp4 = Items[dstBag].Items[dstSlot];
                            Items[dstBag].Items[dstSlot] = Items[srcBag].Items[srcSlot];
                            Items[srcBag].Items[srcSlot] = tmp4;
                            tmp4 = null;
                            goto IL_02e9;
                        }
                        if (srcBag > 0)
                        {
                            if (!Items[srcBag].Items.ContainsKey(srcSlot))
                            {
                                errCode = 22;
                                return;
                            }
                            errCode = (byte)ItemCANEQUIP(Items[srcBag].Items[srcSlot], dstBag, dstSlot);
                            if (errCode == 0 && Items.ContainsKey(dstSlot))
                            {
                                errCode = (byte)ItemCANEQUIP(Items[dstSlot], srcBag, srcSlot);
                            }
                            if (errCode != 0)
                            {
                                return;
                            }
                            if (!Items.ContainsKey(dstSlot))
                            {
                                if (!Items[srcBag].Items.ContainsKey(srcSlot))
                                {
                                    Items.Remove(dstSlot);
                                    Items[srcBag].Items.Remove(srcSlot);
                                }
                                else
                                {
                                    Items[dstSlot] = Items[srcBag].Items[srcSlot];
                                    Items[srcBag].Items.Remove(srcSlot);
                                    if (dstSlot < 23u)
                                    {
                                        Dictionary<byte, ItemObject> items;
                                        byte key;
                                        var Item = (items = Items)[key = dstSlot];
                                        UpdateAddItemStats(ref Item, dstSlot);
                                        items[key] = Item;
                                    }
                                }
                                goto IL_06f5;
                            }
                            if (!Items[srcBag].Items.ContainsKey(srcSlot))
                            {
                                Items[srcBag].Items[srcSlot] = Items[dstSlot];
                                Items.Remove(dstSlot);
                                if (dstSlot < 23u)
                                {
                                    Dictionary<byte, ItemObject> items;
                                    byte key;
                                    var Item = (items = Items[srcBag].Items)[key = srcSlot];
                                    UpdateRemoveItemStats(ref Item, dstSlot);
                                    items[key] = Item;
                                }
                                goto IL_06f5;
                            }
                            if (ItemSTACK(srcBag, srcSlot, dstBag, dstSlot))
                            {
                                return;
                            }
                            var tmp3 = Items[dstSlot];
                            Items[dstSlot] = Items[srcBag].Items[srcSlot];
                            Items[srcBag].Items[srcSlot] = tmp3;
                            if (dstSlot < 23u)
                            {
                                Dictionary<byte, ItemObject> items;
                                byte key;
                                var Item = (items = Items)[key = dstSlot];
                                UpdateAddItemStats(ref Item, dstSlot);
                                items[key] = Item;
                                Item = (items = Items[srcBag].Items)[key = srcSlot];
                                UpdateRemoveItemStats(ref Item, dstSlot);
                                items[key] = Item;
                            }
                            tmp3 = null;
                            goto IL_06f5;
                        }
                        if (dstBag > 0)
                        {
                            if (!Items.ContainsKey(srcSlot))
                            {
                                errCode = 22;
                                return;
                            }
                            errCode = (byte)ItemCANEQUIP(Items[srcSlot], dstBag, dstSlot);
                            if (errCode == 0 && Items[dstBag].Items.ContainsKey(dstSlot))
                            {
                                errCode = (byte)ItemCANEQUIP(Items[dstBag].Items[dstSlot], srcBag, srcSlot);
                            }
                            if (errCode != 0)
                            {
                                return;
                            }
                            if (!Items[dstBag].Items.ContainsKey(dstSlot))
                            {
                                if (!Items.ContainsKey(srcSlot))
                                {
                                    Items[dstBag].Items.Remove(dstSlot);
                                    Items.Remove(srcSlot);
                                }
                                else
                                {
                                    Items[dstBag].Items[dstSlot] = Items[srcSlot];
                                    Items.Remove(srcSlot);
                                    if (srcSlot < 23u)
                                    {
                                        Dictionary<byte, ItemObject> items;
                                        byte key;
                                        var Item = (items = Items[dstBag].Items)[key = dstSlot];
                                        UpdateRemoveItemStats(ref Item, srcSlot);
                                        items[key] = Item;
                                    }
                                }
                                goto IL_0ab8;
                            }
                            if (!Items.ContainsKey(srcSlot))
                            {
                                Items[srcSlot] = Items[dstBag].Items[dstSlot];
                                Items[dstBag].Items.Remove(dstSlot);
                                if (srcSlot < 23u)
                                {
                                    Dictionary<byte, ItemObject> items;
                                    byte key;
                                    var Item = (items = Items)[key = srcSlot];
                                    UpdateAddItemStats(ref Item, srcSlot);
                                    items[key] = Item;
                                }
                                goto IL_0ab8;
                            }
                            if (ItemSTACK(srcBag, srcSlot, dstBag, dstSlot))
                            {
                                return;
                            }
                            var tmp2 = Items[dstBag].Items[dstSlot];
                            Items[dstBag].Items[dstSlot] = Items[srcSlot];
                            Items[srcSlot] = tmp2;
                            if (srcSlot < 23u)
                            {
                                Dictionary<byte, ItemObject> items;
                                byte key;
                                var Item = (items = Items)[key = srcSlot];
                                UpdateAddItemStats(ref Item, srcSlot);
                                items[key] = Item;
                                Item = (items = Items[dstBag].Items)[key = dstSlot];
                                UpdateRemoveItemStats(ref Item, srcSlot);
                                items[key] = Item;
                            }
                            tmp2 = null;
                            goto IL_0ab8;
                        }
                        if (!Items.ContainsKey(srcSlot))
                        {
                            errCode = 22;
                            return;
                        }
                        errCode = (byte)ItemCANEQUIP(Items[srcSlot], dstBag, dstSlot);
                        if (errCode == 0 && Items.ContainsKey(dstSlot))
                        {
                            errCode = (byte)ItemCANEQUIP(Items[dstSlot], srcBag, srcSlot);
                        }
                        if (errCode != 0)
                        {
                            return;
                        }
                        if (!Items.ContainsKey(dstSlot))
                        {
                            if (!Items.ContainsKey(srcSlot))
                            {
                                Items.Remove(dstSlot);
                                Items.Remove(srcSlot);
                            }
                            else
                            {
                                Items[dstSlot] = Items[srcSlot];
                                Items.Remove(srcSlot);
                                if (dstSlot < 23u)
                                {
                                    Dictionary<byte, ItemObject> items;
                                    byte key;
                                    var Item = (items = Items)[key = dstSlot];
                                    UpdateAddItemStats(ref Item, dstSlot);
                                    items[key] = Item;
                                }
                                if (srcSlot < 23u)
                                {
                                    Dictionary<byte, ItemObject> items;
                                    byte key;
                                    var Item = (items = Items)[key = dstSlot];
                                    UpdateRemoveItemStats(ref Item, srcSlot);
                                    items[key] = Item;
                                }
                            }
                            goto IL_0ec5;
                        }
                        if (!Items.ContainsKey(srcSlot))
                        {
                            Items[srcSlot] = Items[dstSlot];
                            Items.Remove(dstSlot);
                            if (dstSlot < 23u)
                            {
                                Dictionary<byte, ItemObject> items;
                                byte key;
                                var Item = (items = Items)[key = srcSlot];
                                UpdateRemoveItemStats(ref Item, dstSlot);
                                items[key] = Item;
                            }
                            if (srcSlot < 23u)
                            {
                                Dictionary<byte, ItemObject> items;
                                byte key;
                                var Item = (items = Items)[key = srcSlot];
                                UpdateAddItemStats(ref Item, srcSlot);
                                items[key] = Item;
                            }
                            goto IL_0ec5;
                        }
                        if (ItemSTACK(srcBag, srcSlot, dstBag, dstSlot))
                        {
                            return;
                        }
                        var tmp = Items[dstSlot];
                        Items[dstSlot] = Items[srcSlot];
                        Items[srcSlot] = tmp;
                        if (dstSlot < 23u)
                        {
                            Dictionary<byte, ItemObject> items;
                            byte key;
                            var Item = (items = Items)[key = dstSlot];
                            UpdateAddItemStats(ref Item, dstSlot);
                            items[key] = Item;
                            Item = (items = Items)[key = srcSlot];
                            UpdateRemoveItemStats(ref Item, dstSlot);
                            items[key] = Item;
                        }
                        if (srcSlot < 23u)
                        {
                            Dictionary<byte, ItemObject> items;
                            byte key;
                            var Item = (items = Items)[key = srcSlot];
                            UpdateAddItemStats(ref Item, srcSlot);
                            items[key] = Item;
                            Item = (items = Items)[key = dstSlot];
                            UpdateRemoveItemStats(ref Item, srcSlot);
                            items[key] = Item;
                        }
                        tmp = null;
                        goto IL_0ec5;
                    }
                IL_06f5:
                    SendItemAndCharacterUpdate(Items[srcBag]);
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update($"UPDATE characters_inventory SET item_slot = {dstSlot}, item_bag = {GUID} WHERE item_guid = {Items[dstSlot].GUID - WorldServiceLocator._Global_Constants.GUID_ITEM};");
                    if (Items[srcBag].Items.ContainsKey(srcSlot))
                    {
                        WorldServiceLocator._WorldServer.CharacterDatabase.Update($"UPDATE characters_inventory SET item_slot = {srcSlot}, item_bag = {Items[srcBag].GUID} WHERE item_guid = {Items[srcBag].Items[srcSlot].GUID - WorldServiceLocator._Global_Constants.GUID_ITEM};");
                    }
                    goto end_IL_0080;
                IL_0ab8:
                    SendItemAndCharacterUpdate(Items[dstBag]);
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update($"UPDATE characters_inventory SET item_slot = {dstSlot}, item_bag = {Items[dstBag].GUID} WHERE item_guid = {Items[dstBag].Items[dstSlot].GUID - WorldServiceLocator._Global_Constants.GUID_ITEM};");
                    if (Items.ContainsKey(srcSlot))
                    {
                        WorldServiceLocator._WorldServer.CharacterDatabase.Update($"UPDATE characters_inventory SET item_slot = {srcSlot}, item_bag = {GUID} WHERE item_guid = {Items[srcSlot].GUID - WorldServiceLocator._Global_Constants.GUID_ITEM};");
                    }
                    goto end_IL_0080;
                IL_0ec5:
                    SendItemAndCharacterUpdate(Items[dstSlot]);
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update($"UPDATE characters_inventory SET item_slot = {dstSlot}, item_bag = {GUID} WHERE item_guid = {Items[dstSlot].GUID - WorldServiceLocator._Global_Constants.GUID_ITEM};");
                    if (Items.ContainsKey(srcSlot))
                    {
                        WorldServiceLocator._WorldServer.CharacterDatabase.Update($"UPDATE characters_inventory SET item_slot = {srcSlot}, item_bag = {GUID} WHERE item_guid = {Items[srcSlot].GUID - WorldServiceLocator._Global_Constants.GUID_ITEM};");
                    }
                    goto end_IL_0080;
                IL_02e9:
                    SendItemUpdate(Items[srcBag]);
                    if (dstBag != srcBag)
                    {
                        SendItemUpdate(Items[dstBag]);
                    }
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update($"UPDATE characters_inventory SET item_slot = {dstSlot}, item_bag = {Items[dstBag].GUID} WHERE item_guid = {Items[dstBag].Items[dstSlot].GUID - WorldServiceLocator._Global_Constants.GUID_ITEM};");
                    if (Items[srcBag].Items.ContainsKey(srcSlot))
                    {
                        WorldServiceLocator._WorldServer.CharacterDatabase.Update($"UPDATE characters_inventory SET item_slot = {srcSlot}, item_bag = {Items[srcBag].GUID} WHERE item_guid = {Items[srcBag].Items[srcSlot].GUID - WorldServiceLocator._Global_Constants.GUID_ITEM};");
                    }
                end_IL_0080:
                    ;
                }
                catch (Exception ex)
                {
                    ProjectData.SetProjectError(ex);
                    var err = ex;
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] Unable to swap items. {2}{3}", client.IP, client.Port, Environment.NewLine, err.ToString());
                    ProjectData.ClearProjectError();
                }
                finally
                {
                    if (errCode != 0)
                    {
                        var wS_Items3 = WorldServiceLocator._WS_Items;
                        var objCharacter = this;
                        wS_Items3.SendInventoryChangeFailure(ref objCharacter, (InventoryChangeFailure)errCode, ItemGetGUID(srcBag, srcSlot), ItemGetGUID(dstBag, dstSlot));
                    }
                }
            }
        }

        public ItemObject ItemGET(byte srcBag, byte srcSlot)
        {
            if (srcBag == 0)
            {
                if (Items.ContainsKey(srcSlot))
                {
                    return Items[srcSlot];
                }
            }
            else if (Items.ContainsKey(srcBag) && Items[srcBag].Items != null && Items[srcBag].Items.ContainsKey(srcSlot))
            {
                return Items[srcBag].Items[srcSlot];
            }
            return null;
        }

        public ItemObject ItemGETByGUID(ulong GUID)
        {
            byte srcBag = default;
            var srcSlot = client.Character.ItemGetSLOTBAG(GUID, ref srcBag);
            return srcSlot == WorldServiceLocator._Global_Constants.ITEM_SLOT_NULL ? null : ItemGET(srcBag, srcSlot);
        }

        public ulong ItemGetGUID(byte srcBag, byte srcSlot)
        {
            if (srcBag == 0)
            {
                if (Items.ContainsKey(srcSlot))
                {
                    return Items[srcSlot].GUID;
                }
            }
            else if (Items.ContainsKey(srcBag) && Items[srcBag].Items != null && Items[srcBag].Items.ContainsKey(srcSlot))
            {
                return Items[srcBag].Items[srcSlot].GUID;
            }
            return 0uL;
        }

        public byte ItemGetSLOTBAG(ulong GUID, ref byte bag)
        {
            byte slot = 0;
            do
            {
                if (Items.ContainsKey(slot) && Items[slot].GUID == GUID)
                {
                    bag = 0;
                    return slot;
                }
                checked
                {
                    slot = (byte)unchecked((uint)(slot + 1));
                }
            }
            while (slot <= 38u);
            byte slot2 = 81;
            do
            {
                if (Items.ContainsKey(slot2) && Items[slot2].GUID == GUID)
                {
                    bag = 0;
                    return slot2;
                }
                checked
                {
                    slot2 = (byte)unchecked((uint)(slot2 + 1));
                }
            }
            while (slot2 <= 112u);
            bag = 19;
            do
            {
                if (Items.ContainsKey(bag))
                {
                    foreach (var item in Items[bag].Items)
                    {
                        if (item.Value.GUID == GUID)
                        {
                            return item.Key;
                        }
                    }
                }
                checked
                {
                    bag = (byte)unchecked((uint)(bag + 1));
                }
            }
            while (bag <= 22u);
            bag = WorldServiceLocator._Global_Constants.ITEM_SLOT_NULL;
            return WorldServiceLocator._Global_Constants.ITEM_SLOT_NULL;
        }

        public void UpdateAddItemStats(ref ItemObject Item, byte slot)
        {
            byte k = 0;
            checked
            {
                do
                {
                    switch (Item.ItemInfo.ItemBonusStatType[k])
                    {
                        case 1:
                            Life.Bonus += Item.ItemInfo.ItemBonusStatValue[k];
                            break;

                        case 3:
                            {
                                Agility.Base += Item.ItemInfo.ItemBonusStatValue[k];
                                ref var positiveBonus5 = ref Agility.PositiveBonus;
                                positiveBonus5 = (short)(positiveBonus5 + Item.ItemInfo.ItemBonusStatValue[k]);
                                Resistances[0].Base += Item.ItemInfo.ItemBonusStatValue[k] * 2;
                                break;
                            }
                        case 4:
                            {
                                Strength.Base += Item.ItemInfo.ItemBonusStatValue[k];
                                ref var positiveBonus4 = ref Strength.PositiveBonus;
                                positiveBonus4 = (short)(positiveBonus4 + Item.ItemInfo.ItemBonusStatValue[k]);
                                break;
                            }
                        case 5:
                            {
                                Intellect.Base += Item.ItemInfo.ItemBonusStatValue[k];
                                ref var positiveBonus3 = ref Intellect.PositiveBonus;
                                positiveBonus3 = (short)(positiveBonus3 + Item.ItemInfo.ItemBonusStatValue[k]);
                                Life.Bonus += Item.ItemInfo.ItemBonusStatValue[k] * 15;
                                break;
                            }
                        case 6:
                            {
                                Spirit.Base += Item.ItemInfo.ItemBonusStatValue[k];
                                ref var positiveBonus2 = ref Spirit.PositiveBonus;
                                positiveBonus2 = (short)(positiveBonus2 + Item.ItemInfo.ItemBonusStatValue[k]);
                                break;
                            }
                        case 7:
                            {
                                Stamina.Base += Item.ItemInfo.ItemBonusStatValue[k];
                                ref var positiveBonus = ref Stamina.PositiveBonus;
                                positiveBonus = (short)(positiveBonus + Item.ItemInfo.ItemBonusStatValue[k]);
                                Life.Bonus += Item.ItemInfo.ItemBonusStatValue[k] * 10;
                                break;
                            }
                        case 15:
                            combatBlockValue += Item.ItemInfo.ItemBonusStatValue[k];
                            break;
                    }
                    k = (byte)unchecked((uint)(k + 1));
                }
                while (k <= 9u);
                byte j = 0;
                do
                {
                    Resistances[j].Base += Item.ItemInfo.Resistances[j];
                    j = (byte)unchecked((uint)(j + 1));
                }
                while (j <= 6u);
                combatBlockValue += Item.ItemInfo.Block;
                if (Item.ItemInfo.Delay > 0)
                {
                    switch (slot)
                    {
                        case 17:
                            AttackTimeBase[2] = (short)Item.ItemInfo.Delay;
                            break;

                        case 15:
                            AttackTimeBase[0] = (short)Item.ItemInfo.Delay;
                            break;

                        case 16:
                            AttackTimeBase[1] = (short)Item.ItemInfo.Delay;
                            break;
                    }
                }
                byte i = 0;
                do
                {
                    if (Item.ItemInfo.Spells[i].SpellID > 0 && WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(Item.ItemInfo.Spells[i].SpellID))
                    {
                        var SpellInfo = WorldServiceLocator._WS_Spells.SPELLs[Item.ItemInfo.Spells[i].SpellID];
                        if (Item.ItemInfo.Spells[i].SpellTrigger == ITEM_SPELLTRIGGER_TYPE.ON_EQUIP)
                        {
                            ApplySpell(Item.ItemInfo.Spells[i].SpellID);
                        }
                        else if (Item.ItemInfo.Spells[i].SpellTrigger == ITEM_SPELLTRIGGER_TYPE.USE)
                        {
                            Packets.PacketClass cooldown = new(Opcodes.SMSG_ITEM_COOLDOWN);
                            try
                            {
                                cooldown.AddUInt64(Item.GUID);
                                cooldown.AddInt32(Item.ItemInfo.Spells[i].SpellID);
                                client.Send(ref cooldown);
                            }
                            finally
                            {
                                cooldown.Dispose();
                            }
                        }
                    }
                    i = (byte)unchecked((uint)(i + 1));
                }
                while (i <= 4u);
                if (Item.ItemInfo.Bonding == 2 && !Item.IsSoulBound)
                {
                    var obj = Item;
                    WS_Network.ClientClass clientClass = null;
                    obj.SoulbindItem(clientClass);
                }
                FinishAllSpells();
                CharacterObject objCharacter;
                foreach (var Enchant in Item.Enchantments)
                {
                    var obj2 = Item;
                    var key = Enchant.Key;
                    objCharacter = this;
                    obj2.AddEnchantBonus(key, objCharacter);
                }
                var wS_Combat = WorldServiceLocator._WS_Combat;
                objCharacter = this;
                wS_Combat.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.BASE_ATTACK);
                var wS_Combat2 = WorldServiceLocator._WS_Combat;
                objCharacter = this;
                wS_Combat2.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.OFF_ATTACK);
                var wS_Combat3 = WorldServiceLocator._WS_Combat;
                objCharacter = this;
                wS_Combat3.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.RANGED_ATTACK);
                if (ManaType == ManaTypes.TYPE_MANA || Classe == Classes.CLASS_DRUID)
                {
                    UpdateManaRegen();
                }
                FillStatsUpdateFlags();
            }
        }

        public void UpdateRemoveItemStats(ref ItemObject Item, byte slot)
        {
            byte k = 0;
            checked
            {
                do
                {
                    switch (Item.ItemInfo.ItemBonusStatType[k])
                    {
                        case 1:
                            Life.Bonus -= Item.ItemInfo.ItemBonusStatValue[k];
                            break;

                        case 3:
                            {
                                Agility.Base -= Item.ItemInfo.ItemBonusStatValue[k];
                                ref var positiveBonus5 = ref Agility.PositiveBonus;
                                positiveBonus5 = (short)(positiveBonus5 - Item.ItemInfo.ItemBonusStatValue[k]);
                                Resistances[0].Base -= Item.ItemInfo.ItemBonusStatValue[k] * 2;
                                break;
                            }
                        case 4:
                            {
                                Strength.Base -= Item.ItemInfo.ItemBonusStatValue[k];
                                ref var positiveBonus4 = ref Strength.PositiveBonus;
                                positiveBonus4 = (short)(positiveBonus4 - Item.ItemInfo.ItemBonusStatValue[k]);
                                break;
                            }
                        case 5:
                            {
                                Intellect.Base -= Item.ItemInfo.ItemBonusStatValue[k];
                                ref var positiveBonus3 = ref Intellect.PositiveBonus;
                                positiveBonus3 = (short)(positiveBonus3 - Item.ItemInfo.ItemBonusStatValue[k]);
                                Mana.Bonus -= Item.ItemInfo.ItemBonusStatValue[k] * 15;
                                break;
                            }
                        case 6:
                            {
                                Spirit.Base -= Item.ItemInfo.ItemBonusStatValue[k];
                                ref var positiveBonus2 = ref Spirit.PositiveBonus;
                                positiveBonus2 = (short)(positiveBonus2 - Item.ItemInfo.ItemBonusStatValue[k]);
                                break;
                            }
                        case 7:
                            {
                                Stamina.Base -= Item.ItemInfo.ItemBonusStatValue[k];
                                ref var positiveBonus = ref Stamina.PositiveBonus;
                                positiveBonus = (short)(positiveBonus - Item.ItemInfo.ItemBonusStatValue[k]);
                                Life.Bonus -= Item.ItemInfo.ItemBonusStatValue[k] * 10;
                                break;
                            }
                        case 15:
                            combatBlockValue -= Item.ItemInfo.ItemBonusStatValue[k];
                            break;
                    }
                    k = (byte)unchecked((uint)(k + 1));
                }
                while (k <= 9u);
                byte j = 0;
                do
                {
                    Resistances[j].Base -= Item.ItemInfo.Resistances[j];
                    j = (byte)unchecked((uint)(j + 1));
                }
                while (j <= 6u);
                combatBlockValue -= Item.ItemInfo.Block;
                if (Item.ItemInfo.Delay > 0)
                {
                    switch (slot)
                    {
                        case 17:
                            AttackTimeBase[2] = 0;
                            break;

                        case 15:
                            AttackTimeBase[0] = (short)(Classe == Classes.CLASS_ROGUE ? 1900 : 2000);
                            break;

                        case 16:
                            AttackTimeBase[1] = 0;
                            break;
                    }
                }
                byte i = 0;
                do
                {
                    if (Item.ItemInfo.Spells[i].SpellID > 0 && WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(Item.ItemInfo.Spells[i].SpellID))
                    {
                        var SpellInfo = WorldServiceLocator._WS_Spells.SPELLs[Item.ItemInfo.Spells[i].SpellID];
                        if (Item.ItemInfo.Spells[i].SpellTrigger == ITEM_SPELLTRIGGER_TYPE.ON_EQUIP)
                        {
                            RemoveAuraBySpell(Item.ItemInfo.Spells[i].SpellID);
                        }
                    }
                    i = (byte)unchecked((uint)(i + 1));
                }
                while (i <= 4u);
                foreach (var Enchant in Item.Enchantments)
                {
                    Item.RemoveEnchantBonus(Enchant.Key);
                }
                var wS_Combat = WorldServiceLocator._WS_Combat;
                var objCharacter = this;
                wS_Combat.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.BASE_ATTACK);
                var wS_Combat2 = WorldServiceLocator._WS_Combat;
                objCharacter = this;
                wS_Combat2.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.OFF_ATTACK);
                var wS_Combat3 = WorldServiceLocator._WS_Combat;
                objCharacter = this;
                wS_Combat3.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.RANGED_ATTACK);
                if (ManaType == ManaTypes.TYPE_MANA || Classe == Classes.CLASS_DRUID)
                {
                    UpdateManaRegen();
                }
                FillStatsUpdateFlags();
            }
        }

        public void SendGossip(ulong cGUID, int cTextID, GossipMenu Menu = null, QuestMenu qMenu = null)
        {
            Packets.PacketClass SMSG_GOSSIP_MESSAGE = new(Opcodes.SMSG_GOSSIP_MESSAGE);
            checked
            {
                try
                {
                    SMSG_GOSSIP_MESSAGE.AddUInt64(cGUID);
                    SMSG_GOSSIP_MESSAGE.AddInt32(cTextID);
                    if (Menu == null)
                    {
                        SMSG_GOSSIP_MESSAGE.AddInt32(0);
                    }
                    else
                    {
                        SMSG_GOSSIP_MESSAGE.AddInt32(Menu.Menus.Count);
                        for (var index2 = 0; index2 < Menu.Menus.Count; index2++)
                        {
                            SMSG_GOSSIP_MESSAGE.AddInt32(index2);
                            SMSG_GOSSIP_MESSAGE.AddInt8(Conversions.ToByte(Menu.Icons[index2]));
                            SMSG_GOSSIP_MESSAGE.AddInt8(Conversions.ToByte(Menu.Coded[index2]));
                            SMSG_GOSSIP_MESSAGE.AddString(Conversions.ToString(Menu.Menus[index2]));
                        }
                    }
                    if (qMenu == null)
                    {
                        SMSG_GOSSIP_MESSAGE.AddInt32(0);
                    }
                    else
                    {
                        SMSG_GOSSIP_MESSAGE.AddInt32(qMenu.Names.Count);
                        for (var index = 0; index < qMenu.Names.Count; index++)
                        {
                            SMSG_GOSSIP_MESSAGE.AddInt32(Conversions.ToInteger(qMenu.IDs[index]));
                            SMSG_GOSSIP_MESSAGE.AddInt32(Conversions.ToInteger(qMenu.Icons[index]));
                            SMSG_GOSSIP_MESSAGE.AddInt32(Conversions.ToInteger(qMenu.Levels[index]));
                            SMSG_GOSSIP_MESSAGE.AddString(Conversions.ToString(qMenu.Names[index]));
                        }
                    }
                    client.Send(ref SMSG_GOSSIP_MESSAGE);
                }
                finally
                {
                    SMSG_GOSSIP_MESSAGE.Dispose();
                }
            }
        }

        public void SendGossipComplete()
        {
            Packets.PacketClass SMSG_GOSSIP_COMPLETE = new(Opcodes.SMSG_GOSSIP_COMPLETE);
            try
            {
                client.Send(ref SMSG_GOSSIP_COMPLETE);
            }
            finally
            {
                SMSG_GOSSIP_COMPLETE.Dispose();
            }
        }

        public void SendPointOfInterest(float x, float y, int icon, int flags, int data, string name)
        {
            Packets.PacketClass SMSG_GOSSIP_POI = new(Opcodes.SMSG_GOSSIP_POI);
            try
            {
                SMSG_GOSSIP_POI.AddInt32(flags);
                SMSG_GOSSIP_POI.AddSingle(x);
                SMSG_GOSSIP_POI.AddSingle(y);
                SMSG_GOSSIP_POI.AddInt32(icon);
                SMSG_GOSSIP_POI.AddInt32(data);
                SMSG_GOSSIP_POI.AddString(name);
                client.Send(ref SMSG_GOSSIP_POI);
            }
            finally
            {
                SMSG_GOSSIP_POI.Dispose();
            }
        }

        public void SendTalking(int TextID)
        {
            if (!WorldServiceLocator._WS_Creatures.NPCTexts.ContainsKey(TextID))
            {
                WS_Creatures.NPCText tmpText = new(TextID);
            }
            Packets.PacketClass response = new(Opcodes.SMSG_NPC_TEXT_UPDATE);
            try
            {
                response.AddInt32(TextID);
                if (WorldServiceLocator._WS_Creatures.NPCTexts[TextID].Count == 0)
                {
                    response.AddInt32(0);
                    response.AddString(WorldServiceLocator._WS_Creatures.NPCTexts[TextID].TextLine1[0]);
                    response.AddString(WorldServiceLocator._WS_Creatures.NPCTexts[TextID].TextLine2[0]);
                }
                else
                {
                    var i = 0;
                    do
                    {
                        response.AddSingle(WorldServiceLocator._WS_Creatures.NPCTexts[TextID].Probability[i]);
                        response.AddString(WorldServiceLocator._WS_Creatures.NPCTexts[TextID].TextLine1[i]);
                        if (Operators.CompareString(WorldServiceLocator._WS_Creatures.NPCTexts[TextID].TextLine2[i], "", TextCompare: false) == 0)
                        {
                            response.AddString(WorldServiceLocator._WS_Creatures.NPCTexts[TextID].TextLine1[i]);
                        }
                        else
                        {
                            response.AddString(WorldServiceLocator._WS_Creatures.NPCTexts[TextID].TextLine2[i]);
                        }
                        response.AddInt32(WorldServiceLocator._WS_Creatures.NPCTexts[TextID].Language[i]);
                        response.AddInt32(WorldServiceLocator._WS_Creatures.NPCTexts[TextID].EmoteDelay1[i]);
                        response.AddInt32(WorldServiceLocator._WS_Creatures.NPCTexts[TextID].Emote1[i]);
                        response.AddInt32(WorldServiceLocator._WS_Creatures.NPCTexts[TextID].EmoteDelay2[i]);
                        response.AddInt32(WorldServiceLocator._WS_Creatures.NPCTexts[TextID].Emote2[i]);
                        response.AddInt32(WorldServiceLocator._WS_Creatures.NPCTexts[TextID].EmoteDelay3[i]);
                        response.AddInt32(WorldServiceLocator._WS_Creatures.NPCTexts[TextID].Emote3[i]);
                        i = checked(i + 1);
                    }
                    while (i <= 7);
                }
                client.Send(ref response);
            }
            finally
            {
                response.Dispose();
            }
        }

        public void BindPlayer(ulong cGUID)
        {
            bindpoint_positionX = positionX;
            bindpoint_positionY = positionY;
            bindpoint_positionZ = positionZ;
            bindpoint_map_id = checked((int)MapID);
            bindpoint_zone_id = ZoneID;
            SaveCharacter();
            Packets.PacketClass SMSG_BINDPOINTUPDATE = new(Opcodes.SMSG_BINDPOINTUPDATE);
            try
            {
                SMSG_BINDPOINTUPDATE.AddSingle(bindpoint_positionX);
                SMSG_BINDPOINTUPDATE.AddSingle(bindpoint_positionY);
                SMSG_BINDPOINTUPDATE.AddSingle(bindpoint_positionZ);
                SMSG_BINDPOINTUPDATE.AddInt32(bindpoint_map_id);
                SMSG_BINDPOINTUPDATE.AddInt32(bindpoint_zone_id);
                client.Send(ref SMSG_BINDPOINTUPDATE);
            }
            finally
            {
                SMSG_BINDPOINTUPDATE.Dispose();
            }
            Packets.PacketClass SMSG_PLAYERBOUND = new(Opcodes.SMSG_PLAYERBOUND);
            try
            {
                SMSG_PLAYERBOUND.AddUInt64(cGUID);
                SMSG_PLAYERBOUND.AddInt32(bindpoint_zone_id);
                client.Send(ref SMSG_PLAYERBOUND);
            }
            finally
            {
                SMSG_PLAYERBOUND.Dispose();
            }
        }

        public void Teleport(float posX, float posY, float posZ, float posO, int map)
        {
            if (MapID != map)
            {
                Transfer(posX, posY, posZ, posO, map);
                return;
            }
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "World: Player Teleport: X[{0}], Y[{1}], Z[{2}], O[{3}]", posX, posY, posZ, posO);
            charMovementFlags = 0;
            Packets.PacketClass packet = new(Opcodes.MSG_MOVE_TELEPORT_ACK);
            try
            {
                packet.AddPackGUID(GUID);
                packet.AddInt32(0);
                packet.AddInt32(0);
                packet.AddInt32(WorldServiceLocator._WS_Network.MsTime());
                packet.AddSingle(posX);
                packet.AddSingle(posY);
                packet.AddSingle(posZ);
                packet.AddSingle(posO);
                packet.AddInt32(0);
                client.Send(ref packet);
            }
            finally
            {
                packet.Dispose();
            }
            positionX = posX;
            positionY = posY;
            positionZ = posZ;
            orientation = posO;
            var wS_CharMovement = WorldServiceLocator._WS_CharMovement;
            var Character = this;
            wS_CharMovement.MoveCell(ref Character);
            var wS_CharMovement2 = WorldServiceLocator._WS_CharMovement;
            Character = this;
            wS_CharMovement2.UpdateCell(ref Character);
            client.Character.ZoneID = WorldServiceLocator._WS_Maps.AreaTable[WorldServiceLocator._WS_Maps.GetAreaFlag(posX, posY, checked((int)client.Character.MapID))].Zone;
        }

        public void Transfer(float posX, float posY, float posZ, float ori, int map)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "World: Player Transfer: X[{0}], Y[{1}], Z[{2}], O[{3}], MAP[{4}]", posX, posY, posZ, ori, map);
            Packets.PacketClass p = new(Opcodes.SMSG_TRANSFER_PENDING);
            checked
            {
                try
                {
                    p.AddInt32(map);
                    if (OnTransport != null)
                    {
                        p.AddInt32(OnTransport.ID);
                        p.AddInt32((int)OnTransport.MapID);
                    }
                    client.Send(ref p);
                }
                finally
                {
                    p.Dispose();
                }
                var wS_CharMovement = WorldServiceLocator._WS_CharMovement;
                var Character = this;
                wS_CharMovement.RemoveFromWorld(ref Character);
                if (OnTransport is not null and WS_Transports.TransportObject @object)
                {
                    var obj = @object;
                    WS_Base.BaseUnit Unit = this;
                    obj.RemovePassenger(ref Unit);
                }
                client.Character.charMovementFlags = 0;
                client.Character.positionX = posX;
                client.Character.positionY = posY;
                client.Character.positionZ = posZ;
                client.Character.orientation = ori;
                client.Character.MapID = (uint)map;
                client.Character.Save();
                WorldServiceLocator._WorldServer.ClsWorldServer.ClientTransfer(client.Index, posX, posY, posZ, ori, map);
            }
        }

        public void ZoneCheck()
        {
            var ZoneFlag = WorldServiceLocator._WS_Maps.GetAreaFlag(positionX, positionY, checked((int)MapID));
            if (!WorldServiceLocator._WS_Maps.AreaTable.ContainsKey(ZoneFlag))
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Zone Flag {0} does not exist.", ZoneFlag);
                return;
            }
            AreaID = WorldServiceLocator._WS_Maps.AreaTable[ZoneFlag].ID;
            ZoneID = WorldServiceLocator._WS_Maps.AreaTable[ZoneFlag].Zone == 0
                ? WorldServiceLocator._WS_Maps.AreaTable[ZoneFlag].ID
                : WorldServiceLocator._WS_Maps.AreaTable[ZoneFlag].Zone;
            GroupUpdateFlag |= 128u;
            if (WorldServiceLocator._WS_Maps.AreaTable[ZoneFlag].IsCity())
            {
                if ((cPlayerFlags & PlayerFlags.PLAYER_FLAGS_RESTING) == 0 && Level < WorldServiceLocator._WS_Player_Initializator.DEFAULT_MAX_LEVEL)
                {
                    cPlayerFlags |= PlayerFlags.PLAYER_FLAGS_RESTING;
                    SetUpdateFlag(190, (int)cPlayerFlags);
                    SendCharacterUpdate();
                }
            }
            else if ((cPlayerFlags & PlayerFlags.PLAYER_FLAGS_RESTING) != 0)
            {
                cPlayerFlags &= ~PlayerFlags.PLAYER_FLAGS_RESTING;
                SetUpdateFlag(190, (int)cPlayerFlags);
                SendCharacterUpdate();
            }
            if (WorldServiceLocator._WS_Maps.AreaTable[ZoneFlag].IsSanctuary())
            {
                if ((cUnitFlags & 0x88) < 136)
                {
                    cUnitFlags |= 136;
                    SetUpdateFlag(46, cUnitFlags);
                    SendCharacterUpdate();
                }
                return;
            }
            if ((cUnitFlags & 0x88) == 136)
            {
                cUnitFlags &= -137;
                cUnitFlags |= 8;
                SetUpdateFlag(46, cUnitFlags);
                SendCharacterUpdate();
            }
            if (WorldServiceLocator._WS_Maps.AreaTable[ZoneFlag].IsArena())
            {
                if ((cPlayerFlags & PlayerFlags.PLAYER_FLAGS_PVP_TIMER) == 0)
                {
                    cPlayerFlags |= PlayerFlags.PLAYER_FLAGS_PVP_TIMER;
                    SetUpdateFlag(190, (int)cPlayerFlags);
                    SendCharacterUpdate();
                    GroupUpdateFlag |= 1u;
                }
                return;
            }
            var tArea = WorldServiceLocator._WS_Maps.AreaTable[ZoneFlag];
            var objCharacter = this;
            if (!tArea.IsMyLand(ref objCharacter))
            {
                if ((cUnitFlags & 0x1000) == 0)
                {
                    cUnitFlags |= 4096;
                    SetUpdateFlag(46, cUnitFlags);
                    SendCharacterUpdate();
                    GroupUpdateFlag |= 1u;
                }
            }
            else if (((uint)cUnitFlags & 0x1000u) != 0)
            {
                cUnitFlags &= -4097;
                SetUpdateFlag(46, cUnitFlags);
                SendCharacterUpdate();
                GroupUpdateFlag |= 1u;
            }
        }

        public void ZoneCheckInstance()
        {
            var ZoneFlag = WorldServiceLocator._WS_Maps.GetAreaFlag(positionX, positionY, checked((int)MapID));
            if (!WorldServiceLocator._WS_Maps.AreaTable.ContainsKey(ZoneFlag))
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Zone Flag {0} does not exist.", ZoneFlag);
                return;
            }
            AreaID = WorldServiceLocator._WS_Maps.AreaTable[ZoneFlag].ID;
            if (WorldServiceLocator._WS_Maps.AreaTable[ZoneFlag].Zone == 0)
            {
                ZoneID = ZoneID == 0 ? WorldServiceLocator._WS_Maps.AreaTable[ZoneFlag].ID : WorldServiceLocator._WS_Maps.AreaTable[ZoneFlag].Zone;
            }
            GroupUpdateFlag |= 128u;
            if (WorldServiceLocator._WS_Maps.AreaTable[ZoneFlag].IsCity())
            {
                if ((cPlayerFlags & PlayerFlags.PLAYER_FLAGS_RESTING) == 0 && Level < WorldServiceLocator._WS_Player_Initializator.DEFAULT_MAX_LEVEL)
                {
                    cPlayerFlags |= PlayerFlags.PLAYER_FLAGS_RESTING;
                    SetUpdateFlag(190, (int)cPlayerFlags);
                    SendCharacterUpdate();
                }
            }
            else if ((cPlayerFlags & PlayerFlags.PLAYER_FLAGS_RESTING) != 0)
            {
                cPlayerFlags &= ~PlayerFlags.PLAYER_FLAGS_RESTING;
                SetUpdateFlag(190, (int)cPlayerFlags);
                SendCharacterUpdate();
            }
            if (WorldServiceLocator._WS_Maps.AreaTable[ZoneFlag].IsSanctuary())
            {
                if ((cUnitFlags & 0x88) < 136)
                {
                    cUnitFlags |= 136;
                    SetUpdateFlag(46, cUnitFlags);
                    SendCharacterUpdate();
                }
                return;
            }
            if ((cUnitFlags & 0x88) == 136)
            {
                cUnitFlags &= -137;
                cUnitFlags |= 8;
                SetUpdateFlag(46, cUnitFlags);
                SendCharacterUpdate();
            }
            if (WorldServiceLocator._WS_Maps.AreaTable[ZoneFlag].IsArena())
            {
                if ((cPlayerFlags & PlayerFlags.PLAYER_FLAGS_PVP_TIMER) == 0)
                {
                    cPlayerFlags |= PlayerFlags.PLAYER_FLAGS_PVP_TIMER;
                    SetUpdateFlag(190, (int)cPlayerFlags);
                    SendCharacterUpdate();
                    GroupUpdateFlag |= 1u;
                }
                return;
            }
            var tArea = WorldServiceLocator._WS_Maps.AreaTable[ZoneFlag];
            var objCharacter = this;
            if (!tArea.IsMyLand(ref objCharacter))
            {
                if ((cUnitFlags & 0x1000) == 0)
                {
                    cUnitFlags |= 4096;
                    SetUpdateFlag(46, cUnitFlags);
                    SendCharacterUpdate();
                    GroupUpdateFlag |= 1u;
                }
            }
            else if (((uint)cUnitFlags & 0x1000u) != 0)
            {
                cUnitFlags &= -4097;
                SetUpdateFlag(46, cUnitFlags);
                SendCharacterUpdate();
                GroupUpdateFlag |= 1u;
            }
        }

        public void ChangeSpeedForced(ChangeSpeedType Type, float NewSpeed)
        {
            checked
            {
                antiHackSpeedChanged_++;
                Packets.PacketClass packet = null;
                try
                {
                    switch (Type)
                    {
                        default:
                            return;

                        case ChangeSpeedType.RUN:
                            packet = new Packets.PacketClass(Opcodes.SMSG_FORCE_RUN_SPEED_CHANGE);
                            RunSpeed = NewSpeed;
                            break;

                        case ChangeSpeedType.RUNBACK:
                            packet = new Packets.PacketClass(Opcodes.SMSG_FORCE_RUN_BACK_SPEED_CHANGE);
                            RunBackSpeed = NewSpeed;
                            break;

                        case ChangeSpeedType.SWIM:
                            packet = new Packets.PacketClass(Opcodes.SMSG_FORCE_SWIM_SPEED_CHANGE);
                            SwimSpeed = NewSpeed;
                            break;

                        case ChangeSpeedType.SWIMBACK:
                            packet = new Packets.PacketClass(Opcodes.SMSG_FORCE_SWIM_BACK_SPEED_CHANGE);
                            SwimBackSpeed = NewSpeed;
                            break;

                        case ChangeSpeedType.TURNRATE:
                            packet = new Packets.PacketClass(Opcodes.SMSG_FORCE_TURN_RATE_CHANGE);
                            TurnRate = NewSpeed;
                            break;
                    }
                    packet.AddPackGUID(GUID);
                    packet.AddInt32(antiHackSpeedChanged_);
                    packet.AddSingle(NewSpeed);
                    client.Character.SendToNearPlayers(ref packet);
                }
                finally
                {
                    packet.Dispose();
                }
            }
        }

        public void ShowBank()
        {
            Packets.PacketClass SMSG_SHOW_BANK = new(Opcodes.SMSG_SHOW_BANK);
            try
            {
                SMSG_SHOW_BANK.AddUInt64(TargetGUID);
                SendToNearPlayers(ref SMSG_SHOW_BANK);
            }
            finally
            {
                SMSG_SHOW_BANK.Dispose();
            }
        }

        public void SetHover()
        {
            Packets.PacketClass SMSG_MOVE_SET_HOVER = new(Opcodes.SMSG_MOVE_SET_HOVER);
            try
            {
                SMSG_MOVE_SET_HOVER.AddPackGUID(TargetGUID);
                SMSG_MOVE_SET_HOVER.AddInt32(0);
                SendToNearPlayers(ref SMSG_MOVE_SET_HOVER);
            }
            finally
            {
                SMSG_MOVE_SET_HOVER.Dispose();
            }
        }

        public void SetWaterWalk()
        {
            Packets.PacketClass SMSG_MOVE_WATER_WALK = new(Opcodes.SMSG_MOVE_WATER_WALK);
            try
            {
                SMSG_MOVE_WATER_WALK.AddPackGUID(GUID);
                SMSG_MOVE_WATER_WALK.AddInt32(0);
                SendToNearPlayers(ref SMSG_MOVE_WATER_WALK);
            }
            finally
            {
                SMSG_MOVE_WATER_WALK.Dispose();
            }
        }

        public void SplineStartSwim()
        {
            Packets.PacketClass SMSG_SPLINE_MOVE_START_SWIM = new(Opcodes.SMSG_SPLINE_MOVE_START_SWIM);
            try
            {
                SMSG_SPLINE_MOVE_START_SWIM.AddPackGUID(GUID);
                SMSG_SPLINE_MOVE_START_SWIM.AddInt32(1);
                SendToNearPlayers(ref SMSG_SPLINE_MOVE_START_SWIM);
            }
            finally
            {
                SMSG_SPLINE_MOVE_START_SWIM.Dispose();
            }
        }

        public void SplineStopSwim()
        {
            Packets.PacketClass SMSG_SPLINE_MOVE_STOP_SWIM = new(Opcodes.SMSG_SPLINE_MOVE_STOP_SWIM);
            try
            {
                SMSG_SPLINE_MOVE_STOP_SWIM.AddPackGUID(GUID);
                SMSG_SPLINE_MOVE_STOP_SWIM.AddInt32(0);
                SendToNearPlayers(ref SMSG_SPLINE_MOVE_STOP_SWIM);
            }
            finally
            {
                SMSG_SPLINE_MOVE_STOP_SWIM.Dispose();
            }
        }

        public void SetLandWalk()
        {
            Packets.PacketClass SMSG_MOVE_LAND_WALK = new(Opcodes.SMSG_MOVE_LAND_WALK);
            try
            {
                SMSG_MOVE_LAND_WALK.AddPackGUID(GUID);
                SMSG_MOVE_LAND_WALK.AddInt32(0);
                SendToNearPlayers(ref SMSG_MOVE_LAND_WALK);
            }
            finally
            {
                SMSG_MOVE_LAND_WALK.Dispose();
            }
        }

        public void SetMoveRoot()
        {
            Packets.PacketClass SMSG_FORCE_MOVE_ROOT = new(Opcodes.SMSG_FORCE_MOVE_ROOT);
            try
            {
                SMSG_FORCE_MOVE_ROOT.AddPackGUID(GUID);
                SMSG_FORCE_MOVE_ROOT.AddInt32(0);
                client.Send(ref SMSG_FORCE_MOVE_ROOT);
            }
            finally
            {
                SMSG_FORCE_MOVE_ROOT.Dispose();
            }
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_FORCE_MOVE_ROOT", client.IP, client.Port);
        }

        public void SetMoveUnroot()
        {
            Packets.PacketClass SMSG_FORCE_MOVE_UNROOT = new(Opcodes.SMSG_FORCE_MOVE_UNROOT);
            try
            {
                SMSG_FORCE_MOVE_UNROOT.AddPackGUID(GUID);
                SMSG_FORCE_MOVE_UNROOT.AddInt32(0);
                SendToNearPlayers(ref SMSG_FORCE_MOVE_UNROOT);
            }
            finally
            {
                SMSG_FORCE_MOVE_UNROOT.Dispose();
            }
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_FORCE_MOVE_UNROOT", client.IP, client.Port);
        }

        public void StartMirrorTimer(MirrorTimer Type, int MaxValue)
        {
            Packets.PacketClass SMSG_START_MIRROR_TIMER = new(Opcodes.SMSG_START_MIRROR_TIMER);
            try
            {
                SMSG_START_MIRROR_TIMER.AddInt32((int)Type);
                SMSG_START_MIRROR_TIMER.AddInt32(MaxValue);
                SMSG_START_MIRROR_TIMER.AddInt32(MaxValue);
                SMSG_START_MIRROR_TIMER.AddInt32(-1);
                SMSG_START_MIRROR_TIMER.AddInt32(0);
                SMSG_START_MIRROR_TIMER.AddInt8(0);
                client.Send(ref SMSG_START_MIRROR_TIMER);
            }
            finally
            {
                SMSG_START_MIRROR_TIMER.Dispose();
            }
        }

        public void ModifyMirrorTimer(MirrorTimer Type, int MaxValue, int CurrentValue, int Regen)
        {
            Packets.PacketClass SMSG_START_MIRROR_TIMER = new(Opcodes.SMSG_START_MIRROR_TIMER);
            try
            {
                SMSG_START_MIRROR_TIMER.AddInt32((int)Type);
                SMSG_START_MIRROR_TIMER.AddInt32(CurrentValue);
                SMSG_START_MIRROR_TIMER.AddInt32(MaxValue);
                SMSG_START_MIRROR_TIMER.AddInt32(Regen);
                SMSG_START_MIRROR_TIMER.AddInt32(0);
                SMSG_START_MIRROR_TIMER.AddInt8(0);
                client.Send(ref SMSG_START_MIRROR_TIMER);
            }
            finally
            {
                SMSG_START_MIRROR_TIMER.Dispose();
            }
        }

        public void StopMirrorTimer(MirrorTimer Type)
        {
            Packets.PacketClass SMSG_STOP_MIRROR_TIMER = new(Opcodes.SMSG_STOP_MIRROR_TIMER);
            try
            {
                SMSG_STOP_MIRROR_TIMER.AddInt32((int)Type);
                client.Send(ref SMSG_STOP_MIRROR_TIMER);
            }
            finally
            {
                SMSG_STOP_MIRROR_TIMER.Dispose();
            }
        }

        public void HandleDrowning(object state)
        {
            checked
            {
                try
                {
                    if (positionZ > WorldServiceLocator._WS_Maps.GetWaterLevel(positionX, positionY, (int)MapID) - 1.6)
                    {
                        underWaterTimer.DrowningValue += 2000;
                        if (underWaterTimer.DrowningValue > 70000)
                        {
                            underWaterTimer.DrowningValue = 70000;
                        }
                        ModifyMirrorTimer(MirrorTimer.DROWNING, 70000, underWaterTimer.DrowningValue, 2);
                        return;
                    }
                    underWaterTimer.DrowningValue -= 1000;
                    if (underWaterTimer.DrowningValue < 0)
                    {
                        underWaterTimer.DrowningValue = 0;
                        LogEnvironmentalDamage(DamageTypes.DMG_HOLY, (int)(0.1f * Life.Maximum * underWaterTimer.DrowningDamage));
                        var damage = (int)(0.1f * Life.Maximum * underWaterTimer.DrowningDamage);
                        WS_Base.BaseUnit Attacker = null;
                        DealDamage(damage, Attacker);
                        underWaterTimer.DrowningDamage *= 2;
                        if (DEAD)
                        {
                            underWaterTimer.Dispose();
                            underWaterTimer = null;
                        }
                    }
                    if (underWaterTimer != null)
                    {
                        ModifyMirrorTimer(MirrorTimer.DROWNING, 70000, underWaterTimer.DrowningValue, -1);
                    }
                }
                catch (Exception ex)
                {
                    ProjectData.SetProjectError(ex);
                    var e = ex;
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error at HandleDrowning():", e.ToString());
                    if (underWaterTimer != null)
                    {
                        underWaterTimer.Dispose();
                    }
                    underWaterTimer = null;
                    ProjectData.ClearProjectError();
                }
            }
        }

        public void InitializeReputation(int FactionID)
        {
            if (WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].VisibleID > -1)
            {
                Reputation[WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].VisibleID].Value = 0;
                if (Reputation[WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].VisibleID].Flags == 0)
                {
                    Reputation[WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].VisibleID].Flags = 1;
                }
            }
        }

        public TReaction GetReaction(int FactionID)
        {
            if (!WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo.ContainsKey(FactionID) || !WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo.ContainsKey(Faction))
            {
                return TReaction.NEUTRAL;
            }
            if (((WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].enemyMask == 0L && WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].friendMask == 0L && WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].enemyFaction1 == 0) & (WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].enemyFaction2 == 0)) && WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].enemyFaction3 == 0 && WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].enemyFaction4 == 0)
            {
                return TReaction.NEUTRAL;
            }
            if (((WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].enemyMask == 0L && WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].friendMask == 0L && WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].enemyFaction1 != Faction) & (WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].enemyFaction2 != Faction)) && WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].enemyFaction3 != Faction && WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].enemyFaction4 != Faction)
            {
                return TReaction.NEUTRAL;
            }
            if ((WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].enemyMask & 1uL) != 0)
            {
                return TReaction.HOSTILE;
            }
            if (WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].friendFaction1 == Faction || WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].friendFaction2 == Faction || WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].friendFaction3 == Faction || WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].friendFaction4 == Faction)
            {
                return TReaction.FIGHT_SUPPORT;
            }
            if ((WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].friendMask & WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[Faction].ourMask) != 0)
            {
                return TReaction.FIGHT_SUPPORT;
            }
            if (WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].enemyFaction1 == Faction || WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].enemyFaction2 == Faction || WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].enemyFaction3 == Faction || WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].enemyFaction4 == Faction)
            {
                return TReaction.HOSTILE;
            }
            if ((WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].enemyMask & WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[Faction].ourMask) != 0)
            {
                return TReaction.HOSTILE;
            }
            return GetReputation(WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].FactionID) switch
            {
                ReputationRank.Hated or ReputationRank.Hostile => TReaction.HOSTILE,
                ReputationRank.Friendly or ReputationRank.Honored => TReaction.FRIENDLY,
                ReputationRank.Unfriendly or ReputationRank.Neutral => TReaction.NEUTRAL,
                _ => TReaction.FIGHT_SUPPORT,
            };
        }

        public int GetReputationValue(int FactionTemplateID)
        {
            if (!WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo.ContainsKey(FactionTemplateID))
            {
                return 3;
            }
            var FactionID = WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionTemplateID].FactionID;
            if (!WorldServiceLocator._WS_DBCDatabase.FactionInfo.ContainsKey(FactionID))
            {
                return 3;
            }
            if (WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].VisibleID == -1)
            {
                return 3;
            }
            checked
            {
                var points = WorldServiceLocator._Functions.HaveFlag((uint)WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].flags[0], (byte)((int)Race - 1)) ? WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].rep_stats[0] : (WorldServiceLocator._Functions.HaveFlag((uint)WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].flags[1], (byte)((int)Race - 1)) ? WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].rep_stats[1] : (WorldServiceLocator._Functions.HaveFlag((uint)WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].flags[2], (byte)((int)Race - 1)) ? WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].rep_stats[2] : (WorldServiceLocator._Functions.HaveFlag((uint)WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].flags[3], (byte)((int)Race - 1)) ? WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].rep_stats[3] : 0)));
                if (Reputation[WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].VisibleID].Flags > 0)
                {
                    points += Reputation[WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].VisibleID].Value;
                }
                return points;
            }
        }

        public ReputationRank GetReputation(int FactionTemplateID)
        {
            if (!WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo.ContainsKey(FactionTemplateID))
            {
                return ReputationRank.Neutral;
            }
            var FactionID = WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionTemplateID].FactionID;
            if (!WorldServiceLocator._WS_DBCDatabase.FactionInfo.ContainsKey(FactionID))
            {
                return ReputationRank.Neutral;
            }
            if (WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].VisibleID == -1)
            {
                return ReputationRank.Neutral;
            }
            checked
            {
                var points = WorldServiceLocator._Functions.HaveFlag((uint)WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].flags[0], (byte)((int)Race - 1)) ? WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].rep_stats[0] : (WorldServiceLocator._Functions.HaveFlag((uint)WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].flags[1], (byte)((int)Race - 1)) ? WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].rep_stats[1] : (WorldServiceLocator._Functions.HaveFlag((uint)WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].flags[2], (byte)((int)Race - 1)) ? WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].rep_stats[2] : (WorldServiceLocator._Functions.HaveFlag((uint)WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].flags[3], (byte)((int)Race - 1)) ? WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].rep_stats[3] : 0)));
                if (Reputation[WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].VisibleID].Flags > 0)
                {
                    points += Reputation[WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].VisibleID].Value;
                }
                var num = points;
                if (num > 21000)
                {
                    return ReputationRank.Exalted;
                }
                if (num > 9000)
                {
                    return ReputationRank.Revered;
                }
                if (num > 3000)
                {
                    return ReputationRank.Honored;
                }
                if (num > 0)
                {
                    return ReputationRank.Friendly;
                }
                if (num > -3000)
                {
                    return ReputationRank.Neutral;
                }
                if (num > -6000)
                {
                    return ReputationRank.Unfriendly;
                }
                return num > -42000 ? ReputationRank.Hostile : ReputationRank.Hated;
            }
        }

        public void SetReputation(int FactionID, int Value)
        {
            if (WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].VisibleID == -1)
            {
                return;
            }
            checked
            {
                Reputation[WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].VisibleID].Value += Value;
                if ((Reputation[WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].VisibleID].Flags & 1) == 0)
                {
                    Reputation[WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].VisibleID].Flags = Reputation[WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].VisibleID].Flags | 1;
                }
                if (client != null)
                {
                    Packets.PacketClass packet = new(Opcodes.SMSG_SET_FACTION_STANDING);
                    try
                    {
                        packet.AddInt32(Reputation[WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].VisibleID].Flags);
                        packet.AddInt32(WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].VisibleID);
                        packet.AddInt32(Reputation[WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].VisibleID].Value);
                        client.Send(ref packet);
                    }
                    finally
                    {
                        packet.Dispose();
                    }
                }
            }
        }

        public float GetDiscountMod(int FactionID)
        {
            var Rank = GetReputation(FactionID);
            return Rank >= ReputationRank.Honored ? 0.9f : 1f;
        }

        public override void Die(ref WS_Base.BaseUnit Attacker)
        {
            DEAD = true;
            corpseGUID = 0uL;
            if (Attacker != null && Attacker is WS_Creatures.CreatureObject @object && @object.aiScript != null)
            {
                var aiScript = @object.aiScript;
                WS_Base.BaseUnit Victim = this;
                aiScript.OnKill(ref Victim);
            }
            GroupUpdateFlag |= 1u;
            foreach (var uGuid in inCombatWith)
            {
                if (WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(uGuid) && WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(uGuid))
                {
                    WorldServiceLocator._WorldServer.CHARACTERs[uGuid].RemoveFromCombat(this);
                }
            }
            inCombatWith.Clear();
            if (IsInDuel)
            {
                DEAD = false;
                var wS_Spells = WorldServiceLocator._WS_Spells;
                ref var duelPartner = ref DuelPartner;
                var Loser = this;
                wS_Spells.DuelComplete(ref duelPartner, ref Loser);
                return;
            }
            checked
            {
                var num = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1;
                for (var j = 0; j <= num; j++)
                {
                    if (ActiveSpells[j] != null)
                    {
                        RemoveAura(j, ref ActiveSpells[j].SpellCaster, RemovedByDuration: false, SendUpdate: false);
                        SetUpdateFlag(47 + j, 0);
                    }
                }
                var Loser = this;
                repopTimer = new WS_PlayerHelper.TRepopTimer(ref Loser);
                cDynamicFlags = 32;
                cUnitFlags = 8;
                SetUpdateFlag(22, 0);
                SetUpdateFlag(46, cUnitFlags);
                SetUpdateFlag(143, cDynamicFlags);
                SendCharacterUpdate();
                if (Attacker is null or WS_Creatures.CreatureObject)
                {
                    byte i = 0;
                    do
                    {
                        if (Items.ContainsKey(i))
                        {
                            Items[i].ModifyDurability(0.1f, ref client);
                        }
                        i = (byte)unchecked((uint)(i + 1));
                    }
                    while (i <= 18u);
                    Packets.PacketClass SMSG_DURABILITY_DAMAGE_DEATH = new(Opcodes.SMSG_DURABILITY_DAMAGE_DEATH);
                    try
                    {
                        client.Send(ref SMSG_DURABILITY_DAMAGE_DEATH);
                    }
                    finally
                    {
                        SMSG_DURABILITY_DAMAGE_DEATH.Dispose();
                    }
                }
                Save();
            }
        }

        public void SendDeathReleaseLoc(float x, float y, float z, int MapID)
        {
            Packets.PacketClass p = new(Opcodes.CMSG_REPOP_REQUEST);
            try
            {
                p.AddInt32(MapID);
                p.AddSingle(x);
                p.AddSingle(y);
                p.AddSingle(z);
                client.Send(ref p);
            }
            finally
            {
                p.Dispose();
            }
        }

        public override void DealDamage(int Damage, WS_Base.BaseUnit Attacker = null)
        {
            if (DEAD)
            {
                return;
            }
            RemoveAurasByInterruptFlag(2);
            if (spellCasted[1] != null)
            {
                var castSpellParameters = spellCasted[1];
                if (!castSpellParameters.Finished)
                {
                    if (((uint)castSpellParameters.SpellInfo.interruptFlags & 0x10u) != 0)
                    {
                        FinishAllSpells();
                    }
                    else if (((uint)castSpellParameters.SpellInfo.interruptFlags & 2u) != 0)
                    {
                        castSpellParameters.Delay();
                    }
                }
            }
            if (Attacker != null)
            {
                if (!inCombatWith.Contains(Attacker.GUID))
                {
                    inCombatWith.Add(Attacker.GUID);
                    CheckCombat();
                    SendCharacterUpdate();
                }
                if (Attacker is CharacterObject @object && !@object.inCombatWith.Contains(GUID))
                {
                    @object.inCombatWith.Add(GUID);
                    if ((@object.cUnitFlags & 0x80000) == 0)
                    {
                        @object.cUnitFlags |= 0x80000;
                        @object.SetUpdateFlag(46, @object.cUnitFlags);
                        @object.SendCharacterUpdate();
                    }
                }
                var array = creaturesNear.ToArray();
                foreach (var cGUID in array)
                {
                    if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(cGUID) && WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].aiScript != null && WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].isGuard && !WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].IsDead && !WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].aiScript.InCombat && !inCombatWith.Contains(cGUID) && GetReaction(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].Faction) == TReaction.FIGHT_SUPPORT && WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID], this) <= WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].AggroRange(this))
                    {
                        WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].aiScript.OnGenerateHate(ref Attacker, Damage);
                    }
                }
            }
            GroupUpdateFlag |= 2u;
            checked
            {
                if (!Invulnerable)
                {
                    Life.Current -= Damage;
                }
                if (Life.Current == 0)
                {
                    Die(ref Attacker);
                    return;
                }
                SetUpdateFlag(22, Life.Current);
                SendCharacterUpdate();
                if (Classe == Classes.CLASS_WARRIOR || (Classe == Classes.CLASS_DRUID && (ShapeshiftForm == ShapeshiftForm.FORM_BEAR || ShapeshiftForm == ShapeshiftForm.FORM_DIREBEAR)))
                {
                    Rage.Increment((int)(2.5 * Damage / GetRageConversion));
                    SetUpdateFlag(24, Rage.Current);
                    SendCharacterUpdate();
                }
            }
        }

        public override void Heal(int Damage, WS_Base.BaseUnit Attacker = null)
        {
            checked
            {
                if (!DEAD)
                {
                    GroupUpdateFlag |= 2u;
                    Life.Current += Damage;
                    SetUpdateFlag(22, Life.Current);
                    SendCharacterUpdate();
                }
            }
        }

        public override void Energize(int Damage, ManaTypes Power, WS_Base.BaseUnit Attacker = null)
        {
            if (DEAD)
            {
                return;
            }
            GroupUpdateFlag |= 16u;
            checked
            {
                switch (Power)
                {
                    default:
                        return;

                    case ManaTypes.TYPE_MANA:
                        if (Mana.Current == Mana.Maximum)
                        {
                            return;
                        }
                        Mana.Current += Damage;
                        SetUpdateFlag(23, Mana.Current);
                        break;

                    case ManaTypes.TYPE_RAGE:
                        if (Rage.Current == Rage.Maximum)
                        {
                            return;
                        }
                        Rage.Current += Damage;
                        SetUpdateFlag(24, Rage.Current);
                        break;

                    case ManaTypes.TYPE_ENERGY:
                        if (Energy.Current == Energy.Maximum)
                        {
                            return;
                        }
                        Energy.Current += Damage;
                        SetUpdateFlag(26, Energy.Current);
                        break;

                    case ManaTypes.TYPE_FOCUS:
                        return;
                }
                SendCharacterUpdate();
            }
        }

        public void Logout(object StateObj = null)
        {
            LogoutTimer?.Dispose();
            LogoutTimer = null;

            if (this is CharacterObject _character)
            {
                if (repopTimer != null)
                {
                    repopTimer?.Dispose();
                    repopTimer = null;
                    WS_Corpses.CorpseObject myCorpse = new(ref _character);
                    myCorpse?.AddToWorld();
                    myCorpse?.Save();
                }

                if (IsInGroup)
                {
                    Group.LocalMembers.Remove(GUID);
                    if (Group.LocalMembers.Count == 0)
                    {
                        Group?.Dispose();
                        Group = null;
                    }
                }

                if (OnTransport is not null and WS_Transports.TransportObject _transport)
                {
                    WS_Base.BaseUnit Unit = this;
                    _transport.RemovePassenger(ref Unit);
                }

                if (DuelPartner != null)
                {
                    if (DuelPartner.DuelArbiter == DuelArbiter)
                    {
                        var wS_Spells = WorldServiceLocator._WS_Spells;
                        ref var duelPartner = ref DuelPartner;
                        wS_Spells.DuelComplete(ref duelPartner, ref _character);
                    }
                    else if (WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(DuelArbiter))
                    {
                        WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[DuelArbiter].Destroy(WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[DuelArbiter]);
                    }
                }
                Packets.PacketClass SMSG_LOGOUT_COMPLETE = new(Opcodes.SMSG_LOGOUT_COMPLETE);
                try
                {
                    client?.Send(ref SMSG_LOGOUT_COMPLETE);
                    SMSG_LOGOUT_COMPLETE?.Dispose();
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_LOGOUT_COMPLETE", client.IP, client.Port);
                    client.Character = null;
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.USER, "Character {0} logged off.", Name);
                    client?.Delete();
                    client = null;
                }
                finally
                {
                    Dispose();
                }
            }
        }

        public void Login()
        {
            WorldServiceLocator._WS_Handlers_Instance.InstanceMapEnter(this);
            SetOnTransport();
            if (MapID != LoginMap)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Spawned on wrong map [{0}], transferring to [{1}].", LoginMap, MapID);
                Transfer(positionX, positionY, positionZ, orientation, checked((int)MapID));
                return;
            }
            WorldServiceLocator._WS_Maps.GetMapTile(positionX, positionY, ref CellX, ref CellY);
            try
            {
                if (WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY] == null)
                {
                    WorldServiceLocator._WS_CharMovement.MAP_Load(CellX, CellY, MapID);
                }
            }
            catch (Exception ex2)
            {
                ProjectData.SetProjectError(ex2);
                var ex = ex2;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Failed loading maps at character logging in.{0}{1}", Environment.NewLine, ex.ToString());
                ProjectData.ClearProjectError();
            }
            var wS_PlayerHelper = WorldServiceLocator._WS_PlayerHelper;
            ref var reference = ref client;
            var Character = this;
            wS_PlayerHelper.SendBindPointUpdate(ref reference, ref Character);
            var wS_PlayerHelper2 = WorldServiceLocator._WS_PlayerHelper;
            ref var reference2 = ref client;
            Character = this;
            wS_PlayerHelper2.Send_SMSG_SET_REST_START(ref reference2, ref Character);
            var wS_PlayerHelper3 = WorldServiceLocator._WS_PlayerHelper;
            ref var reference3 = ref client;
            Character = this;
            wS_PlayerHelper3.SendTutorialFlags(ref reference3, ref Character);
            SendProficiencies();
            var wS_PlayerHelper4 = WorldServiceLocator._WS_PlayerHelper;
            ref var reference4 = ref client;
            Character = this;
            wS_PlayerHelper4.SendInitialSpells(ref reference4, ref Character);
            var wS_PlayerHelper5 = WorldServiceLocator._WS_PlayerHelper;
            ref var reference5 = ref client;
            Character = this;
            wS_PlayerHelper5.SendFactions(ref reference5, ref Character);
            var wS_PlayerHelper6 = WorldServiceLocator._WS_PlayerHelper;
            ref var reference6 = ref client;
            Character = this;
            wS_PlayerHelper6.SendActionButtons(ref reference6, ref Character);
            var wS_PlayerHelper7 = WorldServiceLocator._WS_PlayerHelper;
            ref var reference7 = ref client;
            Character = this;
            wS_PlayerHelper7.SendInitWorldStates(ref reference7, ref Character);
            Life.Current = Life.Maximum;
            Mana.Current = Mana.Maximum;
            FillAllUpdateFlags();
            SendUpdate();
            var wS_CharMovement = WorldServiceLocator._WS_CharMovement;
            Character = this;
            wS_CharMovement.AddToWorld(ref Character);
            WorldServiceLocator._Functions.SendTimeSyncReq(ref client);
            UpdateAuraDurations();
            FullyLoggedIn = true;
            UpdateManaRegen();
        }

        public void UpdateAuraDurations()
        {
            checked
            {
                var num = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1;
                for (var i = 0; i <= num; i++)
                {
                    if (ActiveSpells[i] != null)
                    {
                        Packets.PacketClass SMSG_UPDATE_AURA_DURATION = new(Opcodes.SMSG_UPDATE_AURA_DURATION);
                        try
                        {
                            SMSG_UPDATE_AURA_DURATION.AddInt8((byte)i);
                            SMSG_UPDATE_AURA_DURATION.AddInt32(ActiveSpells[i].SpellDuration);
                            client.Send(ref SMSG_UPDATE_AURA_DURATION);
                        }
                        finally
                        {
                            SMSG_UPDATE_AURA_DURATION.Dispose();
                        }
                    }
                }
            }
        }

        public void SetOnTransport()
        {
            if (LoginTransport == 0)
            {
                return;
            }
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Spawning on transport.");
            var TransportGUID = LoginTransport;
            LoginTransport = 0uL;
            checked
            {
                if (decimal.Compare(new decimal(TransportGUID), 0m) > 0)
                {
                    if (WorldServiceLocator._CommonGlobalFunctions.GuidIsMoTransport(TransportGUID) && WorldServiceLocator._WorldServer.WORLD_TRANSPORTs.ContainsKey(TransportGUID))
                    {
                        OnTransport = WorldServiceLocator._WorldServer.WORLD_TRANSPORTs[TransportGUID];
                        var transportObject = WorldServiceLocator._WorldServer.WORLD_TRANSPORTs[TransportGUID];
                        WS_Base.BaseUnit Unit = this;
                        transportObject.AddPassenger(ref Unit);
                        transportX = positionX;
                        transportY = positionY;
                        transportZ = positionZ;
                        positionX = OnTransport.positionX;
                        positionY = OnTransport.positionY;
                        positionZ = OnTransport.positionZ;
                        MapID = OnTransport.MapID;
                    }
                    else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsTransport(TransportGUID))
                    {
                        if (WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(TransportGUID))
                        {
                            OnTransport = WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[TransportGUID];
                            transportX = positionX;
                            transportY = positionY;
                            transportZ = positionZ;
                            positionX = OnTransport.positionX;
                            positionY = OnTransport.positionY;
                            positionZ = OnTransport.positionZ;
                        }
                        else
                        {
                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Spawning new transport!");
                            var cGUID = TransportGUID - WorldServiceLocator._Global_Constants.GUID_TRANSPORT;
                            DataRow row = null;
                            WS_GameObjects.GameObject newGameobject = new(cGUID, row);
                            newGameobject.AddToWorld();
                            OnTransport = newGameobject;
                            transportX = positionX;
                            transportY = positionY;
                            transportZ = positionZ;
                            positionX = OnTransport.positionX;
                            positionY = OnTransport.positionY;
                            positionZ = OnTransport.positionZ;
                        }
                    }
                    else
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Character logging in on non-existant transport [{0}].", TransportGUID - WorldServiceLocator._Global_Constants.GUID_MO_TRANSPORT);
                        var allGraveYards = WorldServiceLocator._WorldServer.AllGraveYards;
                        var Character = this;
                        allGraveYards.GoToNearestGraveyard(ref Character, Alive: true, Teleport: false);
                        OnTransport = null;
                    }
                }
                else
                {
                    OnTransport = null;
                }
            }
        }

        public void SendProficiencies()
        {
            var ProficiencyFlags = 0;
            checked
            {
                if (HaveSpell(9125))
                {
                    ProficiencyFlags++;
                }
                if (HaveSpell(9078))
                {
                    ProficiencyFlags += 2;
                }
                if (HaveSpell(9077))
                {
                    ProficiencyFlags += 4;
                }
                if (HaveSpell(8737))
                {
                    ProficiencyFlags += 8;
                }
                if (HaveSpell(750))
                {
                    ProficiencyFlags += 16;
                }
                if (HaveSpell(9124))
                {
                    ProficiencyFlags += 32;
                }
                if (HaveSpell(9116))
                {
                    ProficiencyFlags += 64;
                }
                if (HaveSpell(27762))
                {
                    ProficiencyFlags += 128;
                }
                if (HaveSpell(27763))
                {
                    ProficiencyFlags += 512;
                }
                if (HaveSpell(27764))
                {
                    ProficiencyFlags += 256;
                }
                WorldServiceLocator._Functions.SendProficiency(ref client, 4, ProficiencyFlags);
                ProficiencyFlags = 0;
                if (HaveSpell(196))
                {
                    ProficiencyFlags++;
                }
                if (HaveSpell(197))
                {
                    ProficiencyFlags += 2;
                }
                if (HaveSpell(264))
                {
                    ProficiencyFlags += 4;
                }
                if (HaveSpell(266))
                {
                    ProficiencyFlags += 8;
                }
                if (HaveSpell(198))
                {
                    ProficiencyFlags += 16;
                }
                if (HaveSpell(199))
                {
                    ProficiencyFlags += 32;
                }
                if (HaveSpell(200))
                {
                    ProficiencyFlags += 64;
                }
                if (HaveSpell(201))
                {
                    ProficiencyFlags += 128;
                }
                if (HaveSpell(202))
                {
                    ProficiencyFlags += 256;
                }
                if (HaveSpell(227))
                {
                    ProficiencyFlags += 1024;
                }
                if (HaveSpell(262))
                {
                    ProficiencyFlags += 2048;
                }
                if (HaveSpell(263))
                {
                    ProficiencyFlags += 4096;
                }
                if (HaveSpell(15590))
                {
                    ProficiencyFlags += 8192;
                }
                if (HaveSpell(2382))
                {
                    ProficiencyFlags += 16384;
                }
                if (HaveSpell(1180))
                {
                    ProficiencyFlags += 32768;
                }
                if (HaveSpell(2567))
                {
                    ProficiencyFlags += 65536;
                }
                if (HaveSpell(3386))
                {
                    ProficiencyFlags += 131072;
                }
                if (HaveSpell(5011))
                {
                    ProficiencyFlags += 262144;
                }
                if (HaveSpell(5009))
                {
                    ProficiencyFlags += 524288;
                }
                if (HaveSpell(7738))
                {
                    ProficiencyFlags += 1048576;
                }
                WorldServiceLocator._Functions.SendProficiency(ref client, 2, ProficiencyFlags);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                WorldServiceLocator._WorldServer.CharacterDatabase.Update($"DELETE FROM characters_inventory WHERE item_bag = {GUID} AND item_slot >= {69} AND item_slot <= {79}");
                if (underWaterTimer != null)
                {
                    underWaterTimer.Dispose();
                }
                if (repopTimer != null)
                {
                    repopTimer.Dispose();
                    repopTimer = null;
                    var Character = this;
                    WS_Corpses.CorpseObject myCorpse = new(ref Character);
                    myCorpse.Save();
                    myCorpse.AddToWorld();
                }
                if (NonCombatPet != null)
                {
                    NonCombatPet.Destroy();
                }
                if (IsInGroup)
                {
                    Group.LocalMembers.Remove(GUID);
                    if (Group.LocalMembers.Count == 0)
                    {
                        Group.Dispose();
                        Group = null;
                    }
                }
                WorldServiceLocator._WorldServer.CHARACTERs_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                WorldServiceLocator._WorldServer.CHARACTERs.Remove(GUID);
                WorldServiceLocator._WorldServer.CHARACTERs_Lock.ReleaseWriterLock();
                if (FullyLoggedIn)
                {
                    var wS_CharMovement = WorldServiceLocator._WS_CharMovement;
                    var Character = this;
                    wS_CharMovement.RemoveFromWorld(ref Character);
                }
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.USER, "Character {0} disposed.", Name);
                foreach (var item in Items)
                {
                    item.Value.Dispose();
                }
                attackState.Dispose();
                if (client != null)
                {
                    client.Character = null;
                }
                if (LogoutTimer != null)
                {
                    LogoutTimer.Dispose();
                }
                LogoutTimer = null;
                GC.Collect();
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

        public void Initialize()
        {
            CanSeeInvisibility_Stealth = 0;
            CanSeeInvisibility_Invisibility = 0;
            Model_Native = Model;
            if (WorldServiceLocator._WS_DBCDatabase.CreatureModel.ContainsKey(Model))
            {
                BoundingRadius = WorldServiceLocator._WS_DBCDatabase.CreatureModel[Model].BoundingRadius;
                CombatReach = WorldServiceLocator._WS_DBCDatabase.CreatureModel[Model].CombatReach;
            }
            if (Classe == Classes.CLASS_WARRIOR)
            {
                ApplySpell(2457);
            }
            checked
            {
                Resistances[0].Base += Agility.Base * 2;
                Damage.Type = 0;
                Damage.Minimum += 1f;
                RangedDamage.Type = 0;
                RangedDamage.Minimum += 1f;
                if (Access >= AccessLevel.GameMaster)
                {
                    GM = true;
                }
                if (!Items.ContainsKey(17) || Items[17].ItemInfo.ObjectClass != ITEM_CLASS.ITEM_CLASS_WEAPON || (Items[17].ItemInfo.SubClass != ITEM_SUBCLASS.ITEM_SUBCLASS_LIQUID && Items[17].ItemInfo.SubClass != ITEM_SUBCLASS.ITEM_SUBCLASS_CROSSBOW && Items[17].ItemInfo.SubClass != ITEM_SUBCLASS.ITEM_SUBCLASS_POTION))
                {
                    return;
                }
                var AmmoType = ITEM_SUBCLASS.ITEM_SUBCLASS_LIQUID;
                if (Items[17].ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_POTION)
                {
                    AmmoType = ITEM_SUBCLASS.ITEM_SUBCLASS_POTION;
                }
                byte i = 19;
                do
                {
                    if (Items.ContainsKey(i) && Items[i].ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_QUIVER)
                    {
                        foreach (var slot in Items[i].Items)
                        {
                            if (slot.Value.ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_PROJECTILE && slot.Value.ItemInfo.SubClass == AmmoType)
                            {
                                var charManagementHandler = WorldServiceLocator._CharManagementHandler;
                                var objCharacter = this;
                                if (charManagementHandler.CanUseAmmo(ref objCharacter, slot.Value.ItemEntry) == InventoryChangeFailure.EQUIP_ERR_OK)
                                {
                                    AmmoID = slot.Value.ItemEntry;
                                    AmmoDPS = WorldServiceLocator._WorldServer.ITEMDatabase[AmmoID].Damage[0].Minimum;
                                    var wS_Combat = WorldServiceLocator._WS_Combat;
                                    objCharacter = this;
                                    wS_Combat.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.RANGED_ATTACK);
                                    return;
                                }
                            }
                        }
                    }
                    i = (byte)unchecked((uint)(i + 1));
                }
                while (i <= 22u);
            }
        }

        public CharacterObject()
        {
            Access = AccessLevel.Player;
            FullyLoggedIn = false;
            LoginMap = 0u;
            LoginTransport = 0uL;
            TargetGUID = 0uL;
            Model_Native = 0;
            cPlayerFlags = 0;
            cPlayerBytes = 0;
            cPlayerBytes2 = 33615360;
            cPlayerBytes3 = 0;
            cPlayerFieldBytes = -287309824;
            cPlayerFieldBytes2 = 0;
            Rage = new WS_PlayerHelper.TStatBar(1, 1, 0);
            Energy = new WS_PlayerHelper.TStatBar(1, 1, 0);
            Strength = new WS_PlayerHelper.TStat();
            Agility = new WS_PlayerHelper.TStat();
            Stamina = new WS_PlayerHelper.TStat();
            Intellect = new WS_PlayerHelper.TStat();
            Spirit = new WS_PlayerHelper.TStat();
            Faction = 0;
            var Character_ = this;
            attackState = new WS_Combat.TAttackTimer(ref Character_);
            attackSelection = null;
            attackSheathState = SHEATHE_SLOT.SHEATHE_NONE;
            MenuNumber = 0;
            spellCasted = new WS_Spells.CastSpellParameters[4];
            spellCastManaRegeneration = 0;
            spellCanDualWeild = false;
            healing = new WS_PlayerHelper.TDamageBonus();
            spellDamage = new WS_PlayerHelper.TDamageBonus[7];
            spellCriticalRating = 0;
            combatCanDualWield = false;
            combatBlock = 0;
            combatBlockValue = 0;
            combatParry = 0;
            combatCrit = 0;
            combatDodge = 0;
            Damage = new WS_Items.TDamage();
            RangedDamage = new WS_Items.TDamage();
            OffHandDamage = new WS_Items.TDamage();
            AttackTimeBase = new short[3]
            {
                    2000,
                    0,
                    0
            };
            AttackTimeMods = new float[3]
            {
                    1f,
                    1f,
                    1f
            };
            ManaRegenerationModifier = WorldServiceLocator._ConfigurationProvider.GetConfiguration().ManaRegenerationRate;
            LifeRegenerationModifier = WorldServiceLocator._ConfigurationProvider.GetConfiguration().HealthRegenerationRate;
            ManaRegenBonus = 0;
            ManaRegenPercent = 1f;
            ManaRegen = 0;
            ManaRegenInterrupt = 0;
            LifeRegenBonus = 0;
            RageRegenBonus = 0;
            Spell_Language = (LANGUAGES)(-1);
            Pet = null;
            HonorPoints = 0;
            StandingLastWeek = 0;
            HonorKillsLifeTime = 0;
            DishonorKillsLifeTime = 0;
            HonorPointsLastWeek = 0;
            HonorPointsThisWeek = 0;
            HonorPointsYesterday = 0;
            HonorKillsLastWeek = 0;
            HonorKillsThisWeek = 0;
            HonorKillsYesterday = 0;
            HonorKillsToday = 0;
            DishonorKillsToday = 0;
            Copper = 0u;
            Name = "";
            ActionButtons = new Dictionary<byte, WS_PlayerHelper.TActionButton>();
            TaxiZones = new BitArray(256, defaultValue: false);
            TaxiNodes = new Queue<int>();
            ZonesExplored = new uint[128];
            WalkSpeed = WorldServiceLocator._Global_Constants.UNIT_NORMAL_WALK_SPEED;
            RunSpeed = WorldServiceLocator._Global_Constants.UNIT_NORMAL_RUN_SPEED;
            RunBackSpeed = WorldServiceLocator._Global_Constants.UNIT_NORMAL_WALK_BACK_SPEED;
            SwimSpeed = WorldServiceLocator._Global_Constants.UNIT_NORMAL_SWIM_SPEED;
            SwimBackSpeed = WorldServiceLocator._Global_Constants.UNIT_NORMAL_SWIM_BACK_SPEED;
            TurnRate = WorldServiceLocator._Global_Constants.UNIT_NORMAL_TURN_RATE;
            charMovementFlags = 0;
            ZoneID = 0;
            AreaID = 0;
            bindpoint_positionX = 0f;
            bindpoint_positionY = 0f;
            bindpoint_positionZ = 0f;
            bindpoint_map_id = 0;
            bindpoint_zone_id = 0;
            DEAD = false;
            exploreCheckQueued_ = false;
            outsideMapID_ = false;
            antiHackSpeedChanged_ = 0;
            underWaterTimer = null;
            underWaterBreathing = false;
            lootGUID = 0uL;
            repopTimer = null;
            tradeInfo = null;
            corpseGUID = 0uL;
            corpseMapID = 0;
            corpseCorpseType = CorpseType.CORPSE_BONES;
            corpsePositionX = 0f;
            corpsePositionY = 0f;
            corpsePositionZ = 0f;
            resurrectGUID = 0uL;
            resurrectMapID = 0;
            resurrectPositionX = 0f;
            resurrectPositionY = 0f;
            resurrectPositionZ = 0f;
            resurrectHealth = 0;
            resurrectMana = 0;
            guidsForRemoving_Lock = new ReaderWriterLock();
            guidsForRemoving = new List<ulong>();
            creaturesNear = new List<ulong>();
            playersNear = new List<ulong>();
            gameObjectsNear = new List<ulong>();
            dynamicObjectsNear = new List<ulong>();
            corpseObjectsNear = new List<ulong>();
            inCombatWith = new List<ulong>();
            lastPvpAction = 0;
            TutorialFlags = new byte[32];
            UpdateMask = new BitArray(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER, defaultValue: false);
            UpdateData = new Hashtable();
            TalentPoints = 0;
            AmmoID = 0;
            AmmoDPS = 0f;
            AmmoMod = 1f;
            AutoShotSpell = 0;
            NonCombatPet = null;
            TotemSlot = new ulong[4];
            Skills = new Dictionary<int, WS_PlayerHelper.TSkill>();
            SkillsPositions = new Dictionary<int, short>();
            Spells = new Dictionary<int, WS_Spells.CharacterSpell>();
            MindControl = null;
            RestBonus = 0;
            XP = 0;
            Items = new Dictionary<byte, ItemObject>();
            BuyBackTimeStamp = new int[12];
            WatchedFactionIndex = byte.MaxValue;
            Reputation = new WS_PlayerHelper.TReputation[64];
            Group = null;
            GroupUpdateFlag = 0u;
            GuildID = 0u;
            GuildRank = 0;
            GuildInvited = 0;
            GuildInvitedBy = 0;
            DuelArbiter = 0uL;
            DuelPartner = null;
            DuelOutOfBounds = 11;
            TalkMenuTypes = new ArrayList();
            TalkQuests = new WS_QuestsBase[25];
            QuestsCompleted = new List<int>();
            TalkCurrentQuest = null;
            WardenData = new WS_Handlers_Warden.WardenData();
            Level = 1;
            UpdateMask.SetAll(value: false);
            byte i = 0;
            do
            {
                spellDamage[i] = new WS_PlayerHelper.TDamageBonus();
                Resistances[i] = new WS_PlayerHelper.TStat();
                checked
                {
                    i = (byte)unchecked((uint)(i + 1));
                }
            }
            while (i <= 6u);
        }

        public CharacterObject(ref WS_Network.ClientClass ClientVal, ulong GuidVal)
        {
            Access = AccessLevel.Player;
            FullyLoggedIn = false;
            LoginMap = 0u;
            LoginTransport = 0uL;
            TargetGUID = 0uL;
            Model_Native = 0;
            cPlayerFlags = 0;
            cPlayerBytes = 0;
            cPlayerBytes2 = 33615360;
            cPlayerBytes3 = 0;
            cPlayerFieldBytes = -287309824;
            cPlayerFieldBytes2 = 0;
            Rage = new WS_PlayerHelper.TStatBar(1, 1, 0);
            Energy = new WS_PlayerHelper.TStatBar(1, 1, 0);
            Strength = new WS_PlayerHelper.TStat();
            Agility = new WS_PlayerHelper.TStat();
            Stamina = new WS_PlayerHelper.TStat();
            Intellect = new WS_PlayerHelper.TStat();
            Spirit = new WS_PlayerHelper.TStat();
            Faction = 0;
            var Character_ = this;
            attackState = new WS_Combat.TAttackTimer(ref Character_);
            attackSelection = null;
            attackSheathState = SHEATHE_SLOT.SHEATHE_NONE;
            MenuNumber = 0;
            spellCasted = new WS_Spells.CastSpellParameters[4];
            spellCastManaRegeneration = 0;
            spellCanDualWeild = false;
            healing = new WS_PlayerHelper.TDamageBonus();
            spellDamage = new WS_PlayerHelper.TDamageBonus[7];
            spellCriticalRating = 0;
            combatCanDualWield = false;
            combatBlock = 0;
            combatBlockValue = 0;
            combatParry = 0;
            combatCrit = 0;
            combatDodge = 0;
            Damage = new WS_Items.TDamage();
            RangedDamage = new WS_Items.TDamage();
            OffHandDamage = new WS_Items.TDamage();
            AttackTimeBase = new short[3]
            {
                    2000,
                    0,
                    0
            };
            AttackTimeMods = new float[3]
            {
                    1f,
                    1f,
                    1f
            };
            ManaRegenerationModifier = WorldServiceLocator._ConfigurationProvider.GetConfiguration().ManaRegenerationRate;
            LifeRegenerationModifier = WorldServiceLocator._ConfigurationProvider.GetConfiguration().HealthRegenerationRate;
            ManaRegenBonus = 0;
            ManaRegenPercent = 1f;
            ManaRegen = 0;
            ManaRegenInterrupt = 0;
            LifeRegenBonus = 0;
            RageRegenBonus = 0;
            Spell_Language = (LANGUAGES)(-1);
            Pet = null;
            HonorPoints = 0;
            StandingLastWeek = 0;
            HonorKillsLifeTime = 0;
            DishonorKillsLifeTime = 0;
            HonorPointsLastWeek = 0;
            HonorPointsThisWeek = 0;
            HonorPointsYesterday = 0;
            HonorKillsLastWeek = 0;
            HonorKillsThisWeek = 0;
            HonorKillsYesterday = 0;
            HonorKillsToday = 0;
            DishonorKillsToday = 0;
            Copper = 0u;
            Name = "";
            ActionButtons = new Dictionary<byte, WS_PlayerHelper.TActionButton>();
            TaxiZones = new BitArray(256, defaultValue: false);
            TaxiNodes = new Queue<int>();
            ZonesExplored = new uint[128];
            WalkSpeed = WorldServiceLocator._Global_Constants.UNIT_NORMAL_WALK_SPEED;
            RunSpeed = WorldServiceLocator._Global_Constants.UNIT_NORMAL_RUN_SPEED;
            RunBackSpeed = WorldServiceLocator._Global_Constants.UNIT_NORMAL_WALK_BACK_SPEED;
            SwimSpeed = WorldServiceLocator._Global_Constants.UNIT_NORMAL_SWIM_SPEED;
            SwimBackSpeed = WorldServiceLocator._Global_Constants.UNIT_NORMAL_SWIM_BACK_SPEED;
            TurnRate = WorldServiceLocator._Global_Constants.UNIT_NORMAL_TURN_RATE;
            charMovementFlags = 0;
            ZoneID = 0;
            AreaID = 0;
            bindpoint_positionX = 0f;
            bindpoint_positionY = 0f;
            bindpoint_positionZ = 0f;
            bindpoint_map_id = 0;
            bindpoint_zone_id = 0;
            DEAD = false;
            exploreCheckQueued_ = false;
            outsideMapID_ = false;
            antiHackSpeedChanged_ = 0;
            underWaterTimer = null;
            underWaterBreathing = false;
            lootGUID = 0uL;
            repopTimer = null;
            tradeInfo = null;
            corpseGUID = 0uL;
            corpseMapID = 0;
            corpseCorpseType = CorpseType.CORPSE_BONES;
            corpsePositionX = 0f;
            corpsePositionY = 0f;
            corpsePositionZ = 0f;
            resurrectGUID = 0uL;
            resurrectMapID = 0;
            resurrectPositionX = 0f;
            resurrectPositionY = 0f;
            resurrectPositionZ = 0f;
            resurrectHealth = 0;
            resurrectMana = 0;
            guidsForRemoving_Lock = new ReaderWriterLock();
            guidsForRemoving = new List<ulong>();
            creaturesNear = new List<ulong>();
            playersNear = new List<ulong>();
            gameObjectsNear = new List<ulong>();
            dynamicObjectsNear = new List<ulong>();
            corpseObjectsNear = new List<ulong>();
            inCombatWith = new List<ulong>();
            lastPvpAction = 0;
            TutorialFlags = new byte[32];
            UpdateMask = new BitArray(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER, defaultValue: false);
            UpdateData = new Hashtable();
            TalentPoints = 0;
            AmmoID = 0;
            AmmoDPS = 0f;
            AmmoMod = 1f;
            AutoShotSpell = 0;
            NonCombatPet = null;
            TotemSlot = new ulong[4];
            Skills = new Dictionary<int, WS_PlayerHelper.TSkill>();
            SkillsPositions = new Dictionary<int, short>();
            Spells = new Dictionary<int, WS_Spells.CharacterSpell>();
            MindControl = null;
            RestBonus = 0;
            XP = 0;
            Items = new Dictionary<byte, ItemObject>();
            BuyBackTimeStamp = new int[12];
            WatchedFactionIndex = byte.MaxValue;
            Reputation = new WS_PlayerHelper.TReputation[64];
            Group = null;
            GroupUpdateFlag = 0u;
            GuildID = 0u;
            GuildRank = 0;
            GuildInvited = 0;
            GuildInvitedBy = 0;
            DuelArbiter = 0uL;
            DuelPartner = null;
            DuelOutOfBounds = 11;
            TalkMenuTypes = new ArrayList();
            TalkQuests = new WS_QuestsBase[25];
            QuestsCompleted = new List<int>();
            TalkCurrentQuest = null;
            WardenData = new WS_Handlers_Warden.WardenData();
            DataTable MySQLQuery;
            checked
            {
                ActiveSpells = new WS_Base.BaseActiveSpell[WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs - 1 + 1];
                client = ClientVal;
                GUID = GuidVal;
                client.Character = this;
                var m = 0;
                do
                {
                    spellDamage[m] = new WS_PlayerHelper.TDamageBonus();
                    Resistances[m] = new WS_PlayerHelper.TStat();
                    m++;
                }
                while (m <= 6);
                MySQLQuery = new DataTable();
                WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT * FROM characters WHERE char_guid = {0}; UPDATE characters SET char_online = 1 WHERE char_guid = {0};", GUID), ref MySQLQuery);
                if (MySQLQuery.Rows.Count == 0)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] Unable to get SQLDataBase info for character [GUID={2:X}]", client.IP, client.Port, GUID);
                    Dispose();
                    return;
                }
                bindpoint_positionX = MySQLQuery.Rows[0].As<float>("bindpoint_positionX");
                bindpoint_positionY = MySQLQuery.Rows[0].As<float>("bindpoint_positionY");
                bindpoint_positionZ = MySQLQuery.Rows[0].As<float>("bindpoint_positionZ");
                bindpoint_map_id = MySQLQuery.Rows[0].As<int>("bindpoint_map_id");
                bindpoint_zone_id = MySQLQuery.Rows[0].As<int>("bindpoint_zone_id");
            }
            base.Race = (Races)MySQLQuery.Rows[0].As<byte>("char_race");
            Classe = (Classes)MySQLQuery.Rows[0].As<byte>("char_class");
            Gender = (Genders)MySQLQuery.Rows[0].As<byte>("char_gender");
            Skin = MySQLQuery.Rows[0].As<byte>("char_skin");
            Face = MySQLQuery.Rows[0].As<byte>("char_face");
            HairStyle = MySQLQuery.Rows[0].As<byte>("char_hairStyle");
            HairColor = MySQLQuery.Rows[0].As<byte>("char_hairColor");
            FacialHair = MySQLQuery.Rows[0].As<byte>("char_facialHair");
            base.ManaType = (ManaTypes)MySQLQuery.Rows[0].As<byte>("char_manaType");
            Life.Base = MySQLQuery.Rows[0].As<short>("char_life");
            Life.Current = Life.Maximum;
            Mana.Base = MySQLQuery.Rows[0].As<short>("char_mana");
            Mana.Current = Mana.Maximum;
            Rage.Base = 1000;
            Rage.Current = 0;
            Energy.Base = 100;
            Energy.Current = Energy.Maximum;
            XP = MySQLQuery.Rows[0].As<int>("char_xp");
            if (WorldServiceLocator._WS_DBCDatabase.CharRaces.ContainsKey(MySQLQuery.Rows[0].As<int>("char_race")))

            {
                Faction = WorldServiceLocator._WS_DBCDatabase.CharRaces[MySQLQuery.Rows[0].As<int>("char_race")].FactionID;
                Model = Gender == Genders.GENDER_MALE
                    ? WorldServiceLocator._WS_DBCDatabase.CharRaces[MySQLQuery.Rows[0].As<int>("char_race")].ModelMale
                    : WorldServiceLocator._WS_DBCDatabase.CharRaces[MySQLQuery.Rows[0].As<int>("char_race")].ModelFemale;
            }
            if (Model == 0)
            {
                Model = WorldServiceLocator._Functions.GetRaceModel(Race, (int)Gender);
            }
            RestBonus = MySQLQuery.Rows[0].As<int>("char_xp_rested");
            if (RestBonus > 0)
            {
                RestState = XPSTATE.Rested;
            }
            GuildID = MySQLQuery.Rows[0].As<uint>("char_guildId");
            GuildRank = MySQLQuery.Rows[0].As<byte>("char_guildRank");
            Name = MySQLQuery.Rows[0].As<string>("char_name");
            Level = MySQLQuery.Rows[0].As<byte>("char_level");
            Access = ClientVal.Access;
            Copper = MySQLQuery.Rows[0].As<uint>("char_copper");
            positionX = MySQLQuery.Rows[0].As<float>("char_positionX");
            positionY = MySQLQuery.Rows[0].As<float>("char_positionY");
            positionZ = MySQLQuery.Rows[0].As<float>("char_positionZ");
            orientation = MySQLQuery.Rows[0].As<float>("char_orientation");
            ZoneID = MySQLQuery.Rows[0].As<int>("char_zone_id");
            MapID = MySQLQuery.Rows[0].As<uint>("char_map_id");
            LoginMap = MapID;
            Strength.Base = MySQLQuery.Rows[0].As<short>("char_strength");
            Agility.Base = MySQLQuery.Rows[0].As<short>("char_agility");
            Stamina.Base = MySQLQuery.Rows[0].As<short>("char_stamina");
            Intellect.Base = MySQLQuery.Rows[0].As<short>("char_intellect");
            Spirit.Base = MySQLQuery.Rows[0].As<short>("char_spirit");
            TalentPoints = MySQLQuery.Rows[0].As<byte>("char_talentpoints");
            Items_AvailableBankSlots = MySQLQuery.Rows[0].As<byte>("char_bankSlots");
            WatchedFactionIndex = MySQLQuery.Rows[0].As<byte>("char_watchedFactionIndex");
            LoginTransport = MySQLQuery.Rows[0].As<ulong>("char_transportGuid");
            DataTable SpellQuery = new();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("UPDATE characters_spells SET cooldown = 0, cooldownitem = 0 WHERE guid = {0} AND cooldown > 0 AND cooldown < {1}; \r\n                SELECT * FROM characters_spells WHERE guid = {0}; \r\n                UPDATE characters_spells SET cooldown = 0, cooldownitem = 0 WHERE guid = {0} AND cooldown > 0 AND cooldown < {1};", GUID, WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now)), ref SpellQuery);
            IEnumerator enumerator = default;
            try
            {
                enumerator = SpellQuery.Rows.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    DataRow row = (DataRow)enumerator.Current;
                    Spells.Add(row.As<int>("spellid"), new WS_Spells.CharacterSpell(row.As<int>("spellid"), row.As<byte>("active"), row.As<uint>("cooldown"), row.As<int>("cooldownitem")));
                }
            }
            finally
            {
                if (enumerator is IDisposable)
                {
                    (enumerator as IDisposable).Dispose();
                }
            }
            SpellQuery.Clear();
            var tmp = Strings.Split(MySQLQuery.Rows[0].As<string>("char_skillList"));
            checked
            {
                if (tmp.Length > 0)
                {
                    var num = tmp.Length - 1;
                    for (var i2 = 0; i2 <= num; i2++)
                    {
                        if (Operators.CompareString(Strings.Trim(tmp[i2]), "", TextCompare: false) != 0)
                        {
                            var tmp5 = Strings.Split(tmp[i2], ":");
                            if (tmp5.Length == 3)
                            {
                                Skills[Conversions.ToInteger(tmp5[0])] = new WS_PlayerHelper.TSkill(Conversions.ToShort(tmp5[1]), Conversions.ToShort(tmp5[2]));
                                SkillsPositions[Conversions.ToInteger(tmp5[0])] = (short)i2;
                            }
                        }
                    }
                }
                tmp = Strings.Split(MySQLQuery.Rows[0].As<string>("char_auraList"));
                if (tmp.Length > 0)
                {
                    var currentTimestamp = WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now);
                    var num2 = tmp.Length - 1;
                    for (var i3 = 0; i3 <= num2; i3++)
                    {
                        if (Operators.CompareString(Strings.Trim(tmp[i3]), "", TextCompare: false) == 0)
                        {
                            continue;
                        }
                        var tmp4 = Strings.Split(tmp[i3], ":");
                        if (tmp4.Length != 3)
                        {
                            continue;
                        }
                        var AuraSlot = Conversions.ToInteger(tmp4[0]);
                        var AuraSpellID = Conversions.ToInteger(tmp4[1]);
                        var AuraExpire = Conversions.ToLong(tmp4[2]);
                        if (AuraSlot < 0 || AuraSlot >= WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE || !WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(AuraSpellID) || ActiveSpells[AuraSlot] != null)
                        {
                            continue;
                        }
                        int duration;
                        if (AuraExpire == 0)
                        {
                            duration = WorldServiceLocator._Global_Constants.SPELL_DURATION_INFINITE;
                        }
                        else if (AuraExpire < 0)
                        {
                            duration = (int)-AuraExpire;
                        }
                        else
                        {
                            if (currentTimestamp >= AuraExpire)
                            {
                                continue;
                            }
                            duration = (int)((AuraExpire - currentTimestamp) * 1000);
                        }
                        ActiveSpells[AuraSlot] = new WS_Base.BaseActiveSpell(AuraSpellID, duration)
                        {
                            SpellCaster = null
                        };
                        SetAura(AuraSpellID, AuraSlot, duration, SendUpdate: false);
                    }
                }
                tmp = Strings.Split(MySQLQuery.Rows[0].As<string>("char_tutorialFlags"));
                if (tmp.Length > 0)
                {
                    var num3 = tmp.Length - 1;
                    for (var n = 0; n <= num3; n++)
                    {
                        if (Operators.CompareString(Strings.Trim(tmp[n]), "", TextCompare: false) != 0)
                        {
                            TutorialFlags[n] = Conversions.ToByte(tmp[n]);
                        }
                    }
                }
                tmp = Strings.Split(MySQLQuery.Rows[0].As<string>("char_taxiFlags"));
                if (tmp.Length > 0)
                {
                    var num4 = tmp.Length - 1;
                    for (var l = 0; l <= num4; l++)
                    {
                        if (Operators.CompareString(Strings.Trim(tmp[l]), "", TextCompare: false) == 0)
                        {
                            continue;
                        }
                        byte j2 = 0;
                        do
                        {
                            if ((Conversions.ToLong(tmp[l]) & (1 << j2)) != 0)
                            {
                                TaxiZones.Set((l * 8) + j2, value: true);
                            }
                            j2 = (byte)unchecked((uint)(j2 + 1));
                        }
                        while (j2 <= 7u);
                    }
                }
                tmp = Strings.Split(MySQLQuery.Rows[0].As<string>("char_mapExplored"));
                if (tmp.Length > 0)
                {
                    var num5 = tmp.Length - 1;
                    for (var k = 0; k <= num5; k++)
                    {
                        if (Operators.CompareString(Strings.Trim(tmp[k]), "", TextCompare: false) != 0)
                        {
                            ZonesExplored[k] = uint.Parse(tmp[k]);
                        }
                    }
                }
                tmp = Strings.Split(MySQLQuery.Rows[0].As<string>("char_actionBar"));
                if (tmp.Length > 0)
                {
                    var num6 = tmp.Length - 1;
                    for (var j = 0; j <= num6; j++)
                    {
                        if (Operators.CompareString(Strings.Trim(tmp[j]), "", TextCompare: false) != 0)
                        {
                            var tmp3 = Strings.Split(tmp[j], ":");
                            ActionButtons[Conversions.ToByte(tmp3[0])] = new WS_PlayerHelper.TActionButton(Conversions.ToInteger(tmp3[1]), Conversions.ToByte(tmp3[2]), Conversions.ToByte(tmp3[3]));
                        }
                    }
                }
                tmp = Strings.Split(MySQLQuery.Rows[0].As<string>("char_reputation"));
                var i = 0;
                do
                {
                    var tmp2 = Strings.Split(tmp[i], ":");
                    Reputation[i] = new WS_PlayerHelper.TReputation
                    {
                        Flags = Conversions.ToInteger(Strings.Trim(tmp2[0])),
                        Value = Conversions.ToInteger(Strings.Trim(tmp2[1]))
                    };
                    i++;
                }
                while (i <= 63);
                var ForceRestrictions = MySQLQuery.Rows[0].As<uint>("force_restrictions");
                if ((ForceRestrictions & 8u) != 0)
                {
                    cPlayerFlags |= PlayerFlags.PLAYER_FLAGS_HIDE_CLOAK;
                }
                if ((ForceRestrictions & 0x10u) != 0)
                {
                    cPlayerFlags |= PlayerFlags.PLAYER_FLAGS_HIDE_HELM;
                }
                MySQLQuery.Clear();
                WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT * FROM characters_inventory WHERE item_bag = {GUID};", ref MySQLQuery);
                IEnumerator enumerator2 = default;
                try
                {
                    enumerator2 = MySQLQuery.Rows.GetEnumerator();
                    while (enumerator2.MoveNext())
                    {
                        DataRow row = (DataRow)enumerator2.Current;
                        if (Operators.ConditionalCompareObjectNotEqual(row["item_slot"], WorldServiceLocator._Global_Constants.ITEM_SLOT_NULL, TextCompare: false))
                        {
                            var tmpItem = WorldServiceLocator._WS_Items.LoadItemByGUID(row.As<long, ulong>("item_guid"), this, row.As<byte>("item_slot") < 19u);
                            Items[row.As<byte>("item_slot")] = tmpItem;
                            if (row.As<byte, uint>("item_slot") < 23u)
                            {
                                UpdateAddItemStats(ref tmpItem, row.As<byte>("item_slot"));
                            }
                        }
                    }
                }
                finally
                {
                    if (enumerator2 is IDisposable)
                    {
                        (enumerator2 as IDisposable).Dispose();
                    }
                }
                HonorLoad();
                var aLLQUESTS = WorldServiceLocator._WorldServer.ALLQUESTS;
                Character_ = this;
                aLLQUESTS.LoadQuests(ref Character_);
                Initialize();
                var wS_Pets = WorldServiceLocator._WS_Pets;
                Character_ = this;
                wS_Pets.LoadPet(ref Character_);
                MySQLQuery.Clear();
                WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT * FROM corpse WHERE player = {GUID};", ref MySQLQuery);
                if (MySQLQuery.Rows.Count > 0)
                {
                    corpseGUID = Conversions.ToULong(Operators.AddObject(MySQLQuery.Rows[0]["guid"], WorldServiceLocator._Global_Constants.GUID_CORPSE));
                    corpseMapID = MySQLQuery.Rows[0].As<int>("map");
                    corpsePositionX = MySQLQuery.Rows[0].As<float>("position_x");
                    corpsePositionY = MySQLQuery.Rows[0].As<float>("position_y");
                    corpsePositionZ = MySQLQuery.Rows[0].As<float>("position_z");
                    if (positionX == corpsePositionX && positionY == corpsePositionY && positionZ == corpsePositionZ && MapID == corpseMapID)
                    {
                        var allGraveYards = WorldServiceLocator._WorldServer.AllGraveYards;
                        Character_ = this;
                        allGraveYards.GoToNearestGraveyard(ref Character_, Alive: false, Teleport: false);
                    }
                    DEAD = true;
                    cPlayerFlags |= PlayerFlags.PLAYER_FLAGS_DEAD;
                    Invisibility = InvisibilityLevel.DEAD;
                    CanSeeInvisibility = InvisibilityLevel.DEAD;
                    SetWaterWalk();
                    if (client.Character.Race == Races.RACE_NIGHT_ELF)
                    {
                        client.Character.ApplySpell(20584);
                    }
                    else
                    {
                        client.Character.ApplySpell(8326);
                    }
                    Mana.Current = 0;
                    Rage.Current = 0;
                    Energy.Current = 0;
                    Life.Current = 1;
                    cUnitFlags = 8;
                    cDynamicFlags = 0;
                }
                else
                {
                    Life.Bonus = (Stamina.Base - 18) * 10;
                    Mana.Bonus = (Intellect.Base - 18) * 15;
                    Life.Current = Life.Maximum;
                    Mana.Current = Life.Maximum;
                }
            }
        }

        public void SaveAsNewCharacter(int Account_ID)
        {
            var tmpCMD = "INSERT INTO characters (account_id";
            var tmpValues = " VALUES (" + Conversions.ToString(Account_ID);
            ArrayList temp = new();
            tmpCMD += ", char_name";
            tmpValues = tmpValues + ", \"" + Name + "\"";
            tmpCMD += ", char_race";
            tmpValues = tmpValues + ", " + Conversions.ToString((byte)Race);
            tmpCMD += ", char_class";
            tmpValues = tmpValues + ", " + Conversions.ToString((byte)Classe);
            tmpCMD += ", char_gender";
            tmpValues = tmpValues + ", " + Conversions.ToString((byte)Gender);
            tmpCMD += ", char_skin";
            tmpValues = tmpValues + ", " + Conversions.ToString(Skin);
            tmpCMD += ", char_face";
            tmpValues = tmpValues + ", " + Conversions.ToString(Face);
            tmpCMD += ", char_hairStyle";
            tmpValues = tmpValues + ", " + Conversions.ToString(HairStyle);
            tmpCMD += ", char_hairColor";
            tmpValues = tmpValues + ", " + Conversions.ToString(HairColor);
            tmpCMD += ", char_facialHair";
            tmpValues = tmpValues + ", " + Conversions.ToString(FacialHair);
            tmpCMD += ", char_level";
            tmpValues = tmpValues + ", " + Conversions.ToString(Level);
            tmpCMD += ", char_manaType";
            tmpValues = tmpValues + ", " + Conversions.ToString((int)ManaType);
            tmpCMD += ", char_mana";
            tmpValues = tmpValues + ", " + Conversions.ToString(Mana.Base);
            tmpCMD += ", char_rage";
            tmpValues = tmpValues + ", " + Conversions.ToString(Rage.Base);
            tmpCMD += ", char_energy";
            tmpValues = tmpValues + ", " + Conversions.ToString(Energy.Base);
            tmpCMD += ", char_life";
            tmpValues = tmpValues + ", " + Conversions.ToString(Life.Base);
            tmpCMD += ", char_positionX";
            tmpValues = tmpValues + ", " + Strings.Trim(Conversion.Str(positionX));
            tmpCMD += ", char_positionY";
            tmpValues = tmpValues + ", " + Strings.Trim(Conversion.Str(positionY));
            tmpCMD += ", char_positionZ";
            tmpValues = tmpValues + ", " + Strings.Trim(Conversion.Str(positionZ));
            tmpCMD += ", char_map_id";
            tmpValues = tmpValues + ", " + Conversions.ToString(MapID);
            tmpCMD += ", char_zone_id";
            tmpValues = tmpValues + ", " + Conversions.ToString(ZoneID);
            tmpCMD += ", char_orientation";
            tmpValues = tmpValues + ", " + Strings.Trim(Conversion.Str(orientation));
            tmpCMD += ", bindpoint_positionX";
            tmpValues = tmpValues + ", " + Strings.Trim(Conversion.Str(bindpoint_positionX));
            tmpCMD += ", bindpoint_positionY";
            tmpValues = tmpValues + ", " + Strings.Trim(Conversion.Str(bindpoint_positionY));
            tmpCMD += ", bindpoint_positionZ";
            tmpValues = tmpValues + ", " + Strings.Trim(Conversion.Str(bindpoint_positionZ));
            tmpCMD += ", bindpoint_map_id";
            tmpValues = tmpValues + ", " + Conversions.ToString(bindpoint_map_id);
            tmpCMD += ", bindpoint_zone_id";
            tmpValues = tmpValues + ", " + Conversions.ToString(bindpoint_zone_id);
            tmpCMD += ", char_copper";
            tmpValues = tmpValues + ", " + Conversions.ToString(Copper);
            tmpCMD += ", char_xp";
            tmpValues = tmpValues + ", " + Conversions.ToString(XP);
            tmpCMD += ", char_xp_rested";
            tmpValues = tmpValues + ", " + Conversions.ToString(RestBonus);
            temp.Clear();
            foreach (var Skill in Skills)
            {
                temp.Add($"{Skill.Key}:{Skill.Value.Current}:{Skill.Value.Maximum}");
            }
            tmpCMD += ", char_skillList";
            tmpValues = tmpValues + ", \"" + Strings.Join(temp.ToArray()) + "\"";
            tmpCMD += ", char_auraList";
            tmpValues += ", \"\"";
            temp.Clear();
            var tutorialFlags = TutorialFlags;
            foreach (var Flag in tutorialFlags)
            {
                temp.Add(Flag);
            }
            tmpCMD += ", char_tutorialFlags";
            tmpValues = tmpValues + ", \"" + Strings.Join(temp.ToArray()) + "\"";
            temp.Clear();
            var zonesExplored = ZonesExplored;
            checked
            {
                for (var j = 0; j < zonesExplored.Length; j++)
                {
                    var Flag2 = (byte)zonesExplored[j];
                    temp.Add(Flag2);
                }
                tmpCMD += ", char_mapExplored";
                tmpValues = tmpValues + ", \"" + Strings.Join(temp.ToArray()) + "\"";
                temp.Clear();
                var reputation = Reputation;
                foreach (var Reputation_Point in reputation)
                {
                    temp.Add(Conversions.ToString(Reputation_Point.Flags) + ":" + Conversions.ToString(Reputation_Point.Value));
                }
                tmpCMD += ", char_reputation";
                tmpValues = tmpValues + ", \"" + Strings.Join(temp.ToArray()) + "\"";
                temp.Clear();
                foreach (var ActionButton in ActionButtons)
                {
                    temp.Add($"{ActionButton.Key}:{ActionButton.Value.Action}:{ActionButton.Value.ActionType}:{ActionButton.Value.ActionMisc}");
                }
                tmpCMD += ", char_actionBar";
                tmpValues = tmpValues + ", \"" + Strings.Join(temp.ToArray()) + "\"";
                tmpCMD += ", char_strength";
                tmpValues = tmpValues + ", " + Conversions.ToString(Strength.RealBase);
                tmpCMD += ", char_agility";
                tmpValues = tmpValues + ", " + Conversions.ToString(Agility.RealBase);
                tmpCMD += ", char_stamina";
                tmpValues = tmpValues + ", " + Conversions.ToString(Stamina.RealBase);
                tmpCMD += ", char_intellect";
                tmpValues = tmpValues + ", " + Conversions.ToString(Intellect.RealBase);
                tmpCMD += ", char_spirit";
                tmpValues = tmpValues + ", " + Conversions.ToString(Spirit.RealBase);
                var ForceRestrictions = 0u;
                if ((cPlayerFlags & PlayerFlags.PLAYER_FLAGS_HIDE_CLOAK) != 0)
                {
                    ForceRestrictions |= 8u;
                }
                if ((cPlayerFlags & PlayerFlags.PLAYER_FLAGS_HIDE_HELM) != 0)
                {
                    ForceRestrictions |= 0x10u;
                }
                tmpCMD += ", force_restrictions";
                tmpValues = tmpValues + ", " + Conversions.ToString(ForceRestrictions);
                tmpCMD = tmpCMD + ") " + tmpValues + ");";
                WorldServiceLocator._WorldServer.CharacterDatabase.Update(tmpCMD);
                DataTable MySQLQuery = new();
                WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT char_guid FROM characters WHERE char_name = '{Name}';", ref MySQLQuery);
                GUID = (ulong)Conversions.ToLong(MySQLQuery.Rows[0]["char_guid"]);
                HonorSaveAsNew();
            }
        }

        public void Save()
        {
            SaveCharacter();
            foreach (var item in Items)
            {
                item.Value.Save();
            }
        }

        public void SaveCharacter()
        {
            var tmp = "UPDATE characters SET";
            tmp = tmp + " char_name=\"" + Name + "\"";
            tmp = tmp + ", char_race=" + Conversions.ToString((byte)Race);
            tmp = tmp + ", char_class=" + Conversions.ToString((byte)Classe);
            tmp = tmp + ", char_gender=" + Conversions.ToString((byte)Gender);
            tmp = tmp + ", char_skin=" + Conversions.ToString(Skin);
            tmp = tmp + ", char_face=" + Conversions.ToString(Face);
            tmp = tmp + ", char_hairStyle=" + Conversions.ToString(HairStyle);
            tmp = tmp + ", char_hairColor=" + Conversions.ToString(HairColor);
            tmp = tmp + ", char_facialHair=" + Conversions.ToString(FacialHair);
            tmp = tmp + ", char_level=" + Conversions.ToString(Level);
            tmp = tmp + ", char_manaType=" + Conversions.ToString((int)ManaType);
            tmp = tmp + ", char_life=" + Conversions.ToString(Life.Base);
            tmp = tmp + ", char_rage=" + Conversions.ToString(Rage.Base);
            tmp = tmp + ", char_mana=" + Conversions.ToString(Mana.Base);
            tmp = tmp + ", char_energy=" + Conversions.ToString(Energy.Base);
            tmp = tmp + ", char_strength=" + Conversions.ToString(Strength.RealBase);
            tmp = tmp + ", char_agility=" + Conversions.ToString(Agility.RealBase);
            tmp = tmp + ", char_stamina=" + Conversions.ToString(Stamina.RealBase);
            tmp = tmp + ", char_intellect=" + Conversions.ToString(Intellect.RealBase);
            tmp = tmp + ", char_spirit=" + Conversions.ToString(Spirit.RealBase);
            tmp = tmp + ", char_map_id=" + Conversions.ToString(MapID);
            tmp = tmp + ", char_zone_id=" + Conversions.ToString(ZoneID);
            if (OnTransport != null)
            {
                tmp = tmp + ", char_positionX=" + Strings.Trim(Conversion.Str(transportX));
                tmp = tmp + ", char_positionY=" + Strings.Trim(Conversion.Str(transportY));
                tmp = tmp + ", char_positionZ=" + Strings.Trim(Conversion.Str(transportZ));
                tmp = tmp + ", char_orientation=" + Strings.Trim(Conversion.Str(transportO));
                tmp = tmp + ", char_transportGuid=" + Strings.Trim(Conversion.Str(OnTransport.GUID));
            }
            else
            {
                tmp = tmp + ", char_positionX=" + Strings.Trim(Conversion.Str(positionX));
                tmp = tmp + ", char_positionY=" + Strings.Trim(Conversion.Str(positionY));
                tmp = tmp + ", char_positionZ=" + Strings.Trim(Conversion.Str(positionZ));
                tmp = tmp + ", char_orientation=" + Strings.Trim(Conversion.Str(orientation));
                tmp += ", char_transportGuid=0";
            }
            tmp = tmp + ", bindpoint_positionX=" + Strings.Trim(Conversion.Str(bindpoint_positionX));
            tmp = tmp + ", bindpoint_positionY=" + Strings.Trim(Conversion.Str(bindpoint_positionY));
            tmp = tmp + ", bindpoint_positionZ=" + Strings.Trim(Conversion.Str(bindpoint_positionZ));
            tmp = tmp + ", bindpoint_map_id=" + Conversions.ToString(bindpoint_map_id);
            tmp = tmp + ", bindpoint_zone_id=" + Conversions.ToString(bindpoint_zone_id);
            tmp = tmp + ", char_copper=" + Conversions.ToString(Copper);
            tmp = tmp + ", char_xp=" + Conversions.ToString(XP);
            tmp = tmp + ", char_xp_rested=" + Conversions.ToString(RestBonus);
            tmp = tmp + ", char_guildId=" + Conversions.ToString(GuildID);
            tmp = tmp + ", char_guildRank=" + Conversions.ToString(GuildRank);
            ArrayList temp = new();
            temp.Clear();
            foreach (var Skill in Skills)
            {
                temp.Add($"{Skill.Key}:{Skill.Value.Current}:{Skill.Value.Maximum}");
            }
            tmp = tmp + ", char_skillList=\"" + Strings.Join(temp.ToArray()) + "\"";
            temp.Clear();
            checked
            {
                var num = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1;
                for (var i = 0; i <= num; i++)
                {
                    if (ActiveSpells[i] != null && (ActiveSpells[i].SpellDuration == WorldServiceLocator._Global_Constants.SPELL_DURATION_INFINITE || ActiveSpells[i].SpellDuration > 10000))
                    {
                        var expire = 0L;
                        if (ActiveSpells[i].SpellDuration != WorldServiceLocator._Global_Constants.SPELL_DURATION_INFINITE)
                        {
                            expire = WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now) + (ActiveSpells[i].SpellDuration / 1000);
                        }
                        temp.Add($"{i}:{ActiveSpells[i].SpellID}:{expire}");
                    }
                }
                tmp = tmp + ", char_auraList=\"" + Strings.Join(temp.ToArray()) + "\"";
                temp.Clear();
                var tutorialFlags = TutorialFlags;
                foreach (var Flag in tutorialFlags)
                {
                    temp.Add(Flag);
                }
                tmp = tmp + ", char_tutorialFlags=\"" + Strings.Join(temp.ToArray()) + "\"";
                temp.Clear();
                var TmpArray = new byte[32];
                TaxiZones.CopyTo(TmpArray, 0);
                var array = TmpArray;
                foreach (var Flag2 in array)
                {
                    temp.Add(Flag2);
                }
                tmp = tmp + ", char_taxiFlags=\"" + Strings.Join(temp.ToArray()) + "\"";
                temp.Clear();
                var zonesExplored = ZonesExplored;
                foreach (var Flag3 in zonesExplored)
                {
                    temp.Add(Flag3);
                }
                tmp = tmp + ", char_mapExplored=\"" + Strings.Join(temp.ToArray()) + "\"";
                temp.Clear();
                var reputation = Reputation;
                foreach (var Reputation_Point in reputation)
                {
                    temp.Add(Conversions.ToString(Reputation_Point.Flags) + ":" + Conversions.ToString(Reputation_Point.Value));
                }
                tmp = tmp + ", char_reputation=\"" + Strings.Join(temp.ToArray()) + "\"";
                temp.Clear();
                foreach (var ActionButton in ActionButtons)
                {
                    temp.Add($"{ActionButton.Key}:{ActionButton.Value.Action}:{ActionButton.Value.ActionType}:{ActionButton.Value.ActionMisc}");
                }
                tmp = tmp + ", char_actionBar=\"" + Strings.Join(temp.ToArray()) + "\"";
                tmp = tmp + ", char_talentpoints=" + Conversions.ToString(TalentPoints);
                var ForceRestrictions = 0u;
                if ((cPlayerFlags & PlayerFlags.PLAYER_FLAGS_HIDE_CLOAK) != 0)
                {
                    ForceRestrictions |= 8u;
                }
                if ((cPlayerFlags & PlayerFlags.PLAYER_FLAGS_HIDE_HELM) != 0)
                {
                    ForceRestrictions |= 0x10u;
                }
                tmp = tmp + ", force_restrictions=" + Conversions.ToString(ForceRestrictions);
                tmp += $" WHERE char_guid = \"{GUID}\";";
                WorldServiceLocator._WorldServer.CharacterDatabase.Update(tmp);
            }
        }

        public void SavePosition()
        {
            var tmp = "UPDATE characters SET";
            tmp = tmp + ", char_positionX=" + Strings.Trim(Conversion.Str(positionX));
            tmp = tmp + ", char_positionY=" + Strings.Trim(Conversion.Str(positionY));
            tmp = tmp + ", char_positionZ=" + Strings.Trim(Conversion.Str(positionZ));
            tmp = tmp + ", char_map_id=" + Conversions.ToString(MapID);
            tmp += $" WHERE char_guid = \"{GUID}\";";
            WorldServiceLocator._WorldServer.CharacterDatabase.Update(tmp);
        }

        public void GroupUpdate()
        {
            if (Group != null && (ulong)GroupUpdateFlag != 0)
            {
                var wS_Group = WorldServiceLocator._WS_Group;
                var objCharacter = this;
                var Packet = wS_Group.BuildPartyMemberStats(ref objCharacter, GroupUpdateFlag);
                GroupUpdateFlag = 0u;
                if (Packet != null)
                {
                    Group.Broadcast(Packet);
                }
            }
        }

        public void GroupUpdate(int Flag)
        {
            if (Group != null)
            {
                var wS_Group = WorldServiceLocator._WS_Group;
                var objCharacter = this;
                var Packet = wS_Group.BuildPartyMemberStats(ref objCharacter, checked((uint)Flag));
                if (Packet != null)
                {
                    Group.Broadcast(Packet);
                }
            }
        }

        public void StartDuel()
        {
            Thread.Sleep(3000);
            if (decimal.Compare(new decimal(DuelArbiter), 0m) != 0 && DuelPartner != null)
            {
                SetUpdateFlag(196, 1);
                DuelPartner.SetUpdateFlag(196, 2);
                DuelPartner.SendCharacterUpdate();
                SendCharacterUpdate();
            }
        }

        public bool TalkAddQuest(ref WS_QuestInfo Quest)
        {
            var i = 0;
            checked
            {
                do
                {
                    if (TalkQuests[i] == null)
                    {
                        WorldServiceLocator._WorldServer.ALLQUESTS.CreateQuest(ref TalkQuests[i], ref Quest);
                        if (TalkQuests[i] is WS_QuestsBaseScripted obj)
                        {
                            var objCharacter = this;
                            obj.OnQuestStart(ref objCharacter);
                        }
                        else
                        {
                            var obj2 = TalkQuests[i];
                            var objCharacter = this;
                            obj2.Initialize(ref objCharacter);
                        }
                        TalkQuests[i].Slot = (byte)i;
                        var updateDataCount = UpdateData.Count;
                        var questState = TalkQuests[i].GetProgress();
                        SetUpdateFlag(198 + (i * 3), TalkQuests[i].ID);
                        SetUpdateFlag(199 + (i * 3), questState);
                        SetUpdateFlag(199 + (i * 3) + 1, 0);
                        WorldServiceLocator._WorldServer.CharacterDatabase.Update($"INSERT INTO characters_quests (char_guid, quest_id, quest_status) VALUES ({GUID}, {TalkQuests[i].ID}, {questState});");
                        SendCharacterUpdate(updateDataCount != 0);
                        return true;
                    }
                    i++;
                }
                while (i <= 24);
                return false;
            }
        }

        public bool TalkDeleteQuest(byte QuestSlot)
        {
            if (TalkQuests[QuestSlot] == null)
            {
                return false;
            }
            if (TalkQuests[QuestSlot] is WS_QuestsBaseScripted obj)
            {
                var objCharacter = this;
                obj.OnQuestCancel(ref objCharacter);
            }
            var updateDataCount = UpdateData.Count;
            checked
            {
                SetUpdateFlag(198 + (QuestSlot * 3), 0);
                SetUpdateFlag(199 + (QuestSlot * 3), 0);
                SetUpdateFlag(199 + (QuestSlot * 3) + 1, 0);
                WorldServiceLocator._WorldServer.CharacterDatabase.Update($"DELETE  FROM characters_quests WHERE char_guid = {GUID} AND quest_id = {TalkQuests[QuestSlot].ID};");
                TalkQuests[QuestSlot] = null;
                SendCharacterUpdate(updateDataCount != 0);
                return true;
            }
        }

        public bool TalkCompleteQuest(byte QuestSlot)
        {
            if (TalkQuests[QuestSlot] == null)
            {
                return false;
            }
            if (TalkQuests[QuestSlot] is WS_QuestsBaseScripted obj)
            {
                var objCharacter = this;
                obj.OnQuestComplete(ref objCharacter);
            }
            var updateDataCount = UpdateData.Count;
            checked
            {
                SetUpdateFlag(198 + (QuestSlot * 3), 0);
                SetUpdateFlag(199 + (QuestSlot * 3), 0);
                SetUpdateFlag(199 + (QuestSlot * 3) + 1, 0);
                QuestsCompleted.Add(TalkQuests[QuestSlot].ID);
                WorldServiceLocator._WorldServer.CharacterDatabase.Update($"UPDATE characters_quests SET quest_status = -1 WHERE char_guid = {GUID} AND quest_id = {TalkQuests[QuestSlot].ID};");
                TalkQuests[QuestSlot] = null;
                return true;
            }
        }

        public bool TalkUpdateQuest(byte QuestSlot)
        {
            if (TalkQuests[QuestSlot] == null)
            {
                return false;
            }
            var updateDataCount = UpdateData.Count;
            var tmpState = TalkQuests[QuestSlot].GetState();
            var tmpProgress = TalkQuests[QuestSlot].GetProgress();
            checked
            {
                if (TalkQuests[QuestSlot].TimeEnd > 0)
                {
                    var tmpTimer = (int)(TalkQuests[QuestSlot].TimeEnd - WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now));
                }
                WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_quests SET quest_status = {2} WHERE char_guid = {0} AND quest_id = {1};", GUID, TalkQuests[QuestSlot].ID, tmpProgress));
                SetUpdateFlag(199 + (QuestSlot * 3), tmpProgress);
                SetUpdateFlag(199 + (QuestSlot * 3) + 1, 0);
                SendCharacterUpdate(updateDataCount != 0);
                return true;
            }
        }

        public bool TalkCanAccept(ref WS_QuestInfo Quest)
        {
            DataTable DBResult = new();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT quest_status FROM characters_quests WHERE char_guid = {GUID} AND quest_id = {Quest.ID} LIMIT 1;", ref DBResult);
            if (DBResult.Rows.Count > 0)
            {
                var status = Conversions.ToInteger(DBResult.Rows[0]["quest_status"]);
                if (status == -1)
                {
                    Packets.PacketClass packet = new(Opcodes.SMSG_QUESTGIVER_QUEST_INVALID);
                    try
                    {
                        packet.AddInt32(7);
                        client.Send(ref packet);
                    }
                    finally
                    {
                        packet.Dispose();
                    }
                }
                else
                {
                    Packets.PacketClass packet2 = new(Opcodes.SMSG_QUESTGIVER_QUEST_INVALID);
                    try
                    {
                        packet2.AddInt32(13);
                        client.Send(ref packet2);
                    }
                    finally
                    {
                        packet2.Dispose();
                    }
                }
                return false;
            }
            checked
            {
                if (Quest.RequiredRace != 0 && (Quest.RequiredRace & (1 << ((int)Race - 1))) == 0)
                {
                    Packets.PacketClass packet4 = new(Opcodes.SMSG_QUESTGIVER_QUEST_INVALID);
                    try
                    {
                        packet4.AddInt32(6);
                        client.Send(ref packet4);
                    }
                    finally
                    {
                        packet4.Dispose();
                    }
                    return false;
                }
                if (Quest.RequiredClass != 0 && (Quest.RequiredClass & (1 << ((int)Classe - 1))) == 0)
                {
                    Packets.PacketClass packet5 = new(Opcodes.SMSG_QUESTGIVER_QUEST_INVALID);
                    try
                    {
                        packet5.AddInt32(0);
                        client.Send(ref packet5);
                    }
                    finally
                    {
                        packet5.Dispose();
                    }
                    return false;
                }
                if (Quest.RequiredTradeSkill != 0 && !Skills.ContainsKey(Quest.RequiredTradeSkill))
                {
                    Packets.PacketClass packet3 = new(Opcodes.SMSG_QUESTGIVER_QUEST_INVALID);
                    try
                    {
                        packet3.AddInt32(0);
                        client.Send(ref packet3);
                    }
                    finally
                    {
                        packet3.Dispose();
                    }
                    return false;
                }
                return true;
            }
        }

        public bool IsQuestCompleted(int QuestID)
        {
            DataTable q = new();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT quest_id FROM characters_quests WHERE char_guid = {GUID} AND quest_status = -1 AND quest_id = {QuestID};", ref q);
            return q.Rows.Count != 0;
        }

        public bool IsQuestInProgress(int QuestID)
        {
            var i = 0;
            do
            {
                if (TalkQuests[i] != null && TalkQuests[i].ID == QuestID)
                {
                    return true;
                }
                i = checked(i + 1);
            }
            while (i <= 24);
            return false;
        }

        public void LogXPGain(int Ammount, int Rested, ulong VictimGUID, float Group)
        {
            Packets.PacketClass SMSG_LOG_XPGAIN = new(Opcodes.SMSG_LOG_XPGAIN);
            try
            {
                SMSG_LOG_XPGAIN.AddUInt64(VictimGUID);
                SMSG_LOG_XPGAIN.AddInt32(Ammount);
                if (decimal.Compare(new decimal(VictimGUID), 0m) != 0)
                {
                    SMSG_LOG_XPGAIN.AddInt8(0);
                }
                else
                {
                    SMSG_LOG_XPGAIN.AddInt8(1);
                }
                SMSG_LOG_XPGAIN.AddInt32(checked(Ammount - Rested));
                SMSG_LOG_XPGAIN.AddSingle(Group);
                client.Send(ref SMSG_LOG_XPGAIN);
            }
            finally
            {
                SMSG_LOG_XPGAIN.Dispose();
            }
        }

        public void LogHonorGain(int Ammount, ulong VictimGUID = 0uL, byte VictimRANK = 0)
        {
            Packets.PacketClass SMSG_PVP_CREDIT = new(Opcodes.SMSG_PVP_CREDIT);
            try
            {
                SMSG_PVP_CREDIT.AddInt32(Ammount);
                SMSG_PVP_CREDIT.AddUInt64(VictimGUID);
                SMSG_PVP_CREDIT.AddInt32(VictimRANK);
                client.Send(ref SMSG_PVP_CREDIT);
            }
            finally
            {
                SMSG_PVP_CREDIT.Dispose();
            }
        }

        public void LogLootItem(ItemObject Item, byte ItemCount, bool Recieved, bool Created)
        {
            Packets.PacketClass response = new(Opcodes.SMSG_ITEM_PUSH_RESULT);
            try
            {
                response.AddUInt64(GUID);
                response.AddInt32(0 - (Recieved ? 1 : 0));
                response.AddInt32(0 - (Created ? 1 : 0));
                response.AddInt32(1);
                response.AddInt8(Item.GetBagSlot);
                if (Item.StackCount == ItemCount)
                {
                    response.AddInt32(Item.GetSlot);
                }
                else
                {
                    response.AddInt32(-1);
                }
                response.AddInt32(Item.ItemEntry);
                response.AddInt32(Item.SuffixFactor);
                response.AddInt32(Item.RandomProperties);
                response.AddInt32(ItemCount);
                response.AddInt32(ItemCOUNT(Item.ItemEntry));
                client.SendMultiplyPackets(ref response);
                if (IsInGroup)
                {
                    Group.Broadcast(response);
                }
            }
            finally
            {
                response.Dispose();
            }
        }

        public void LogEnvironmentalDamage(DamageTypes dmgType, int Damage)
        {
            Packets.PacketClass SMSG_ENVIRONMENTALDAMAGELOG = new(Opcodes.SMSG_ENVIRONMENTALDAMAGELOG);
            try
            {
                SMSG_ENVIRONMENTALDAMAGELOG.AddUInt64(GUID);
                SMSG_ENVIRONMENTALDAMAGELOG.AddInt8((byte)dmgType);
                SMSG_ENVIRONMENTALDAMAGELOG.AddInt32(Damage);
                SMSG_ENVIRONMENTALDAMAGELOG.AddInt32(0);
                SMSG_ENVIRONMENTALDAMAGELOG.AddInt32(0);
                SendToNearPlayers(ref SMSG_ENVIRONMENTALDAMAGELOG);
            }
            finally
            {
                SMSG_ENVIRONMENTALDAMAGELOG.Dispose();
            }
        }

        public float GetStealthDistance(ref WS_Base.BaseUnit objCharacter)
        {
            var VisibleDistance = (float)(10.5 - (Invisibility_Value / 100.0));
            checked
            {
                VisibleDistance += objCharacter.Level - Level;
                return (float)(VisibleDistance + ((objCharacter.CanSeeInvisibility_Stealth - Invisibility_Bonus) / 5.0));
            }
        }
    }
}
