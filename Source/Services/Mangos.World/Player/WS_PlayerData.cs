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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;
using System.Threading;
using Mangos.Common.Enums.Chat;
using Mangos.Common.Enums.Faction;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Group;
using Mangos.Common.Enums.Item;
using Mangos.Common.Enums.Misc;
using Mangos.Common.Enums.Player;
using Mangos.Common.Enums.Quest;
using Mangos.Common.Enums.Spell;
using Mangos.Common.Enums.Unit;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Handlers;
using Mangos.World.Objects;
using Mangos.World.Quests;
using Mangos.World.Server;
using Mangos.World.Social;
using Mangos.World.Spells;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Player
{
    public class WS_PlayerData
    {
        public class CharacterObject : WS_Base.BaseUnit, IDisposable
        {
            public CharacterObject(ref WS_Network.ClientClass ClientVal, ulong GuidVal)
            {

                // Combat related
                attackState = new WS_Combat.TAttackTimer(ref phe797c28db562431bb1d7cda496ac4706);
                corpseCorpseType = this.CorpseType.CORPSE_BONES;
                // DONE: Add space for passive auras
                ActiveSpells = new WS_Base.BaseActiveSpell[WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs];

                // DONE: Initialize Defaults
                client = ClientVal;
                GUID = GuidVal;
                client.Character = this;
                for (int i = DamageTypes.DMG_PHYSICAL, loopTo = DamageTypes.DMG_ARCANE; i <= loopTo; i++)
                {
                    spellDamage[i] = new WS_PlayerHelper.TDamageBonus();
                    Resistances[i] = new WS_PlayerHelper.TStat();
                }

                // DONE: Get character info from DB
                var MySQLQuery = new DataTable();
                WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT * FROM characters WHERE char_guid = {0}; UPDATE characters SET char_online = 1 WHERE char_guid = {0};", (object)GUID), MySQLQuery);
                if (MySQLQuery.Rows.Count == 0)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] Unable to get SQLDataBase info for character [GUID={2:X}]", client.IP, client.Port, GUID);
                    Dispose();
                    return;
                }

                // DONE: Get BindPoint Coords
                bindpoint_positionX = Conversions.ToSingle(MySQLQuery.Rows[0]["bindpoint_positionX"]);
                bindpoint_positionY = Conversions.ToSingle(MySQLQuery.Rows[0]["bindpoint_positionY"]);
                bindpoint_positionZ = Conversions.ToSingle(MySQLQuery.Rows[0]["bindpoint_positionZ"]);
                bindpoint_map_id = Conversions.ToInteger(MySQLQuery.Rows[0]["bindpoint_map_id"]);
                bindpoint_zone_id = Conversions.ToInteger(MySQLQuery.Rows[0]["bindpoint_zone_id"]);

                // DONE: Get CharCreate Vars
                base.Race = Conversions.ToByte(MySQLQuery.Rows[0]["char_race"]);
                Classe = Conversions.ToByte(MySQLQuery.Rows[0]["char_class"]);
                Gender = Conversions.ToByte(MySQLQuery.Rows[0]["char_gender"]);
                Skin = Conversions.ToByte(MySQLQuery.Rows[0]["char_skin"]);
                Face = Conversions.ToByte(MySQLQuery.Rows[0]["char_face"]);
                HairStyle = Conversions.ToByte(MySQLQuery.Rows[0]["char_hairStyle"]);
                HairColor = Conversions.ToByte(MySQLQuery.Rows[0]["char_hairColor"]);
                FacialHair = Conversions.ToByte(MySQLQuery.Rows[0]["char_facialHair"]);
                base.ManaType = Conversions.ToByte(MySQLQuery.Rows[0]["char_manaType"]);
                Life.Base = Conversions.ToShort(MySQLQuery.Rows[0]["char_life"]);
                Life.Current = Life.Maximum;
                Mana.Base = Conversions.ToShort(MySQLQuery.Rows[0]["char_mana"]);
                Mana.Current = Mana.Maximum;
                Rage.Base = 1000;
                Rage.Current = 0;
                Energy.Base = 100;
                Energy.Current = Energy.Maximum;
                XP = Conversions.ToInteger(MySQLQuery.Rows[0]["char_xp"]);
                if (WorldServiceLocator._WS_DBCDatabase.CharRaces.ContainsKey(Conversions.ToInteger(MySQLQuery.Rows[0]["char_race"])))
                {
                    Faction = WorldServiceLocator._WS_DBCDatabase.CharRaces[Conversions.ToInteger(MySQLQuery.Rows[0]["char_race"])].FactionID;
                    if (Gender == Genders.GENDER_MALE)
                    {
                        Model = WorldServiceLocator._WS_DBCDatabase.CharRaces[Conversions.ToInteger(MySQLQuery.Rows[0]["char_race"])].ModelMale;
                    }
                    else
                    {
                        Model = WorldServiceLocator._WS_DBCDatabase.CharRaces[Conversions.ToInteger(MySQLQuery.Rows[0]["char_race"])].ModelFemale;
                    }
                }

                if (Model == 0)
                    Model = WorldServiceLocator._Functions.GetRaceModel(Race, Gender);

                // DONE: Get Rested Bonus XP and Rest State
                RestBonus = Conversions.ToInteger(MySQLQuery.Rows[0]["char_xp_rested"]);
                if (RestBonus > 0)
                    RestState = XPSTATE.Rested;

                // DONE: Get Guild Info
                GuildID = Conversions.ToUInteger(MySQLQuery.Rows[0]["char_guildId"]);
                GuildRank = Conversions.ToByte(MySQLQuery.Rows[0]["char_guildRank"]);

                // DONE: Get all other vars
                Name = Conversions.ToString(MySQLQuery.Rows[0]["char_name"]);
                Level = Conversions.ToByte(MySQLQuery.Rows[0]["char_level"]);
                Access = ClientVal.Access;
                Copper = Conversions.ToUInteger(MySQLQuery.Rows[0]["char_copper"]);
                positionX = Conversions.ToSingle(MySQLQuery.Rows[0]["char_positionX"]);
                positionY = Conversions.ToSingle(MySQLQuery.Rows[0]["char_positionY"]);
                positionZ = Conversions.ToSingle(MySQLQuery.Rows[0]["char_positionZ"]);
                orientation = Conversions.ToSingle(MySQLQuery.Rows[0]["char_orientation"]);
                ZoneID = Conversions.ToInteger(MySQLQuery.Rows[0]["char_zone_id"]);
                MapID = Conversions.ToUInteger(MySQLQuery.Rows[0]["char_map_id"]);
                LoginMap = MapID;
                Strength.Base = Conversions.ToShort(MySQLQuery.Rows[0]["char_strength"]);
                Agility.Base = Conversions.ToShort(MySQLQuery.Rows[0]["char_agility"]);
                Stamina.Base = Conversions.ToShort(MySQLQuery.Rows[0]["char_stamina"]);
                Intellect.Base = Conversions.ToShort(MySQLQuery.Rows[0]["char_intellect"]);
                Spirit.Base = Conversions.ToShort(MySQLQuery.Rows[0]["char_spirit"]);
                TalentPoints = Conversions.ToByte(MySQLQuery.Rows[0]["char_talentpoints"]);
                Items_AvailableBankSlots = Conversions.ToByte(MySQLQuery.Rows[0]["char_bankSlots"]);
                WatchedFactionIndex = Conversions.ToByte(MySQLQuery.Rows[0]["char_watchedFactionIndex"]);
                LoginTransport = Conversions.ToULong(MySQLQuery.Rows[0]["char_transportGuid"]);
                string[] tmp;
                var SpellQuery = new DataTable();
                // ToDo: Need better string to query the data correctly. An ugly method.
                WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format(@"UPDATE characters_spells SET cooldown = 0, cooldownitem = 0 WHERE guid = {0} AND cooldown > 0 AND cooldown < {1}; 
                SELECT * FROM characters_spells WHERE guid = {0}; 
                UPDATE characters_spells SET cooldown = 0, cooldownitem = 0 WHERE guid = {0} AND cooldown > 0 AND cooldown < {1};", (object)GUID, (object)WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now)), SpellQuery);

                // DONE: Get SpellList
                foreach (DataRow Spell in SpellQuery.Rows)
                    Spells.Add(Conversions.ToInteger(Spell["spellid"]), new WS_Spells.CharacterSpell(Conversions.ToInteger(Spell["spellid"]), Conversions.ToByte(Spell["active"]), Conversions.ToUInteger(Spell["cooldown"]), Conversions.ToInteger(Spell["cooldownitem"])));
                SpellQuery.Clear();

                // DONE: Get SkillList -> Saved as STRING like "SkillID1:Current:Maximum SkillID2:Current:Maximum SkillID3:Current:Maximum"
                tmp = Strings.Split(Conversions.ToString(MySQLQuery.Rows[0]["char_skillList"]), " ");
                if (tmp.Length > 0)
                {
                    for (int i = 0, loopTo1 = tmp.Length - 1; i <= loopTo1; i++)
                    {
                        if (!string.IsNullOrEmpty(Strings.Trim(tmp[i])))
                        {
                            var tmp2 = Strings.Split(tmp[i], ":");
                            if (tmp2.Length == 3)
                            {
                                Skills[Conversions.ToInteger(tmp2[0])] = new WS_PlayerHelper.TSkill(Conversions.ToShort(tmp2[1]), Conversions.ToShort(tmp2[2]));
                                SkillsPositions[Conversions.ToInteger(tmp2[0])] = (short)i;
                            }
                        }
                    }
                }

                // DONE: Get AuraList
                tmp = Strings.Split(Conversions.ToString(MySQLQuery.Rows[0]["char_auraList"]), " ");
                if (tmp.Length > 0)
                {
                    uint currentTimestamp = WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now);
                    for (int i = 0, loopTo2 = tmp.Length - 1; i <= loopTo2; i++)
                    {
                        if (!string.IsNullOrEmpty(Strings.Trim(tmp[i])))
                        {
                            var tmp2 = Strings.Split(tmp[i], ":");
                            if (tmp2.Length == 3)
                            {
                                int AuraSlot = Conversions.ToInteger(tmp2[0]);
                                int AuraSpellID = Conversions.ToInteger(tmp2[1]);
                                long AuraExpire = Conversions.ToLong(tmp2[2]);
                                if (AuraSlot < 0 || AuraSlot >= WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE)
                                    continue; // Not acceptable slot
                                if (WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(AuraSpellID) == false)
                                    continue; // Non-existant spell
                                if (ActiveSpells[AuraSlot] is null)
                                {
                                    int duration;
                                    if (AuraExpire == 0L) // Infinite duration aura
                                    {
                                        duration = WorldServiceLocator._Global_Constants.SPELL_DURATION_INFINITE;
                                    }
                                    else if (AuraExpire < 0L) // Duration paused during offline-time
                                    {
                                        duration = (int)-AuraExpire;
                                    }
                                    else // Duration continued during offline-time
                                    {
                                        if (currentTimestamp >= AuraExpire)
                                            continue; // Duration has expired
                                        duration = (int)((AuraExpire - currentTimestamp) * 1000L);
                                    }

                                    ActiveSpells[AuraSlot] = new WS_Base.BaseActiveSpell(AuraSpellID, duration) { SpellCaster = null };
                                    SetAura(AuraSpellID, AuraSlot, duration, false);
                                }
                            }
                        }
                    }
                }

                // DONE: Get TutorialFlags -> Saved as STRING like "Flag1 Flag2 Flag3"
                tmp = Strings.Split(Conversions.ToString(MySQLQuery.Rows[0]["char_tutorialFlags"]), " ");
                if (tmp.Length > 0)
                {
                    for (int i = 0, loopTo3 = tmp.Length - 1; i <= loopTo3; i++)
                    {
                        if (!string.IsNullOrEmpty(Strings.Trim(tmp[i])))
                            TutorialFlags[i] = Conversions.ToByte(tmp[i]);
                    }
                }

                // DONE: Get TaxiFlags -> Saved as STRING like "Flag1 Flag2 Flag3"
                tmp = Strings.Split(Conversions.ToString(MySQLQuery.Rows[0]["char_taxiFlags"]), " ");
                if (tmp.Length > 0)
                {
                    for (int i = 0, loopTo4 = tmp.Length - 1; i <= loopTo4; i++)
                    {
                        if (!string.IsNullOrEmpty(Strings.Trim(tmp[i])))
                        {
                            for (byte j = 0; j <= 7; j++)
                            {
                                if (Conversions.ToBoolean(Conversions.ToLong(tmp[i]) & 1 << j))
                                {
                                    TaxiZones.Set(i * 8 + j, true);
                                }
                            }
                        }
                    }
                }

                // DONE: Get ZonesExplored -> Saved as STRING like "Flag1 Flag2 Flag3"
                tmp = Strings.Split(Conversions.ToString(MySQLQuery.Rows[0]["char_mapExplored"]), " ");
                if (tmp.Length > 0)
                {
                    for (int i = 0, loopTo5 = tmp.Length - 1; i <= loopTo5; i++)
                    {
                        if (!string.IsNullOrEmpty(Strings.Trim(tmp[i])))
                            ZonesExplored[i] = uint.Parse(tmp[i]);
                    }
                }

                // DONE: Get ActionButtons -> Saved as STRING like "Button1:Action1:Type1:Misc1 Button2:Action2:Type2:Misc2"
                tmp = Strings.Split(Conversions.ToString(MySQLQuery.Rows[0]["char_actionBar"]), " ");
                if (tmp.Length > 0)
                {
                    for (int i = 0, loopTo6 = tmp.Length - 1; i <= loopTo6; i++)
                    {
                        if (!string.IsNullOrEmpty(Strings.Trim(tmp[i])))
                        {
                            string[] tmp2;
                            tmp2 = Strings.Split(tmp[i], ":");
                            ActionButtons[Conversions.ToByte(tmp2[0])] = new WS_PlayerHelper.TActionButton(Conversions.ToInteger(tmp2[1]), Conversions.ToByte(tmp2[2]), Conversions.ToByte(tmp2[3]));
                        }
                    }
                }

                // DONE: Get ReputationPoints -> Saved as STRING like "Flags1:Standing1 Flags2:Standing2"
                tmp = Strings.Split(Conversions.ToString(MySQLQuery.Rows[0]["char_reputation"]), " ");
                for (int i = 0; i <= 63; i++)
                {
                    string[] tmp2;
                    tmp2 = Strings.Split(tmp[i], ":");
                    Reputation[i] = new WS_PlayerHelper.TReputation()
                    {
                        Flags = Conversions.ToInteger(Strings.Trim(tmp2[0])),
                        Value = Conversions.ToInteger(Strings.Trim(tmp2[1]))
                    };
                }

                // DONE: Get playerflags from force restrictions
                uint ForceRestrictions = Conversions.ToUInteger(MySQLQuery.Rows[0]["force_restrictions"]);
                if (ForceRestrictions & ForceRestrictionFlags.RESTRICT_HIDECLOAK)
                    cPlayerFlags = cPlayerFlags | PlayerFlags.PLAYER_FLAGS_HIDE_CLOAK;
                if (ForceRestrictions & ForceRestrictionFlags.RESTRICT_HIDEHELM)
                    cPlayerFlags = cPlayerFlags | PlayerFlags.PLAYER_FLAGS_HIDE_HELM;

                // DONE: Get Items
                MySQLQuery.Clear();
                WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT * FROM characters_inventory WHERE item_bag = {0};", (object)GUID), MySQLQuery);
                foreach (DataRow row in MySQLQuery.Rows)
                {
                    if (!Operators.ConditionalCompareObjectEqual(row["item_slot"], WorldServiceLocator._Global_Constants.ITEM_SLOT_NULL, false))
                    {
                        var tmpItem = WorldServiceLocator._WS_Items.LoadItemByGUID(Conversions.ToLong(row["item_guid"]), this, Conversions.ToByte(row["item_slot"]) < EquipmentSlots.EQUIPMENT_SLOT_END);
                        Items[Conversions.ToByte(row["item_slot"])] = tmpItem;
                        if (Conversions.ToByte(row["item_slot"]) < InventorySlots.INVENTORY_SLOT_BAG_END)
                            UpdateAddItemStats(ref tmpItem, Conversions.ToByte(row["item_slot"]));
                    }
                }

                // DONE: Get Honor Point
                HonorLoad();

                // DONE: Load quests in progress
                var argobjCharacter = this;
                WorldServiceLocator._WorldServer.ALLQUESTS.LoadQuests(ref argobjCharacter);

                // DONE: Initialize Internal fields
                Initialize();

                // DONE: Load current pet if any
                var argobjCharacter1 = this;
                WorldServiceLocator._WS_Pets.LoadPet(ref argobjCharacter1);

                // DONE: Load corpse if present
                MySQLQuery.Clear();
                WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT * FROM corpse WHERE player = {0};", (object)GUID), MySQLQuery);
                if (MySQLQuery.Rows.Count > 0)
                {
                    corpseGUID = MySQLQuery.Rows[0]["guid"] + WorldServiceLocator._Global_Constants.GUID_CORPSE;
                    corpseMapID = Conversions.ToInteger(MySQLQuery.Rows[0]["map"]);
                    corpsePositionX = Conversions.ToSingle(MySQLQuery.Rows[0]["position_x"]);
                    corpsePositionY = Conversions.ToSingle(MySQLQuery.Rows[0]["position_y"]);
                    corpsePositionZ = Conversions.ToSingle(MySQLQuery.Rows[0]["position_z"]);

                    // DONE: If you logout before releasing your corpse you will now go to the graveyard
                    if (positionX == corpsePositionX && positionY == corpsePositionY && positionZ == corpsePositionZ && MapID == corpseMapID)
                    {
                        var argCharacter = this;
                        WorldServiceLocator._WorldServer.AllGraveYards.GoToNearestGraveyard(ref argCharacter, false, false);
                    }

                    // DONE: Make Dead
                    DEAD = true;
                    cPlayerFlags = cPlayerFlags | PlayerFlags.PLAYER_FLAGS_DEAD;

                    // DONE: Update to see only dead
                    Invisibility = InvisibilityLevel.DEAD;
                    CanSeeInvisibility = InvisibilityLevel.DEAD;
                    SetWaterWalk();

                    // DONE: Set Auras
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
                    cUnitFlags = UnitFlags.UNIT_FLAG_ATTACKABLE;
                    cDynamicFlags = 0;
                }
                else
                {
                    // DONE: Calculate the bonus health and mana from stats
                    Life.Bonus = (Stamina.Base - 18) * 10;
                    Mana.Bonus = (Intellect.Base - 18) * 15;
                    Life.Current = Life.Maximum;
                    Mana.Current = Life.Maximum;
                }
            }

            public CharacterObject() : base()
            {
                attackState = new WS_Combat.TAttackTimer(ref phe797c28db562431bb1d7cda496ac4706);
                corpseCorpseType = this.CorpseType.CORPSE_BONES;
                Level = 1;
                UpdateMask.SetAll(false);
                for (byte i = DamageTypes.DMG_PHYSICAL, loopTo = DamageTypes.DMG_ARCANE; i <= loopTo; i++)
                {
                    spellDamage[i] = new WS_PlayerHelper.TDamageBonus();
                    Resistances[i] = new WS_PlayerHelper.TStat();
                }
            }

            // Connection Information
            public WS_Network.ClientClass client;
            public AccessLevel Access = AccessLevel.Player;
            public Timer LogoutTimer;
            public bool FullyLoggedIn = false;
            public uint LoginMap = 0U;
            public ulong LoginTransport = 0LU;

            // Character Information
            public ulong TargetGUID = 0UL;
            public int Model_Native = 0;
            public PlayerFlags cPlayerFlags = 0;
            // <<0                <<8             <<16                <<24
            public int cPlayerBytes = 0;                  // Skin,              Face,           HairStyle,          HairColor
            public int cPlayerBytes2 = 0x200EE00;         // FacialHair,        ?,              BankSlotsAvailable, RestState
            public int cPlayerBytes3 = 0;                 // Gender,            Alchohol,       Defender?,          LastWeekHonorRank
            public int cPlayerFieldBytes = 0xEEE00000;    // ?,                 ComboPoints,    ActionBar,          HighestHonorRank
            public int cPlayerFieldBytes2 = 0;            // HonorBar

            // cPlayerBytes subfields
            // (Skin + (CType(Face, Integer) << 8) + (CType(HairStyle, Integer) << 16) + (CType(HairColor, Integer) << 24))
            public byte HairColor
            {
                get
                {
                    return (byte)((cPlayerBytes & 0xFF000000) >> 24);
                }

                set
                {
                    cPlayerBytes = cPlayerBytes & 0xFFFFFF | value << 24;
                }
            }

            public byte HairStyle
            {
                get
                {
                    return (byte)((cPlayerBytes & 0xFF0000) >> 16);
                }

                set
                {
                    cPlayerBytes = cPlayerBytes & 0xFF00FFFF | value << 16;
                }
            }

            public byte Face
            {
                get
                {
                    return (byte)((cPlayerBytes & 0xFF00) >> 8);
                }

                set
                {
                    cPlayerBytes = cPlayerBytes & 0xFFFF00FF | value << 8;
                }
            }

            public byte Skin
            {
                get
                {
                    return (byte)((cPlayerBytes & 0xFF) >> 0);
                }

                set
                {
                    cPlayerBytes = cPlayerBytes & 0xFFFFFF00 | value << 0;
                }
            }

            // cPlayerBytes2 subfields
            // (FacialHair + (&HEE << 8) + (CType(Items_AvailableBankSlots, Integer) << 16) + (CType(RestState, Integer) << 24))
            public XPSTATE RestState
            {
                get
                {
                    return (cPlayerBytes2 & 0xFF000000) >> 24;
                }

                set
                {
                    cPlayerBytes2 = cPlayerBytes2 & 0xFFFFFF | value << 24;
                }
            }

            public byte Items_AvailableBankSlots
            {
                get
                {
                    return (byte)((cPlayerBytes2 & 0xFF0000) >> 16);
                }

                set
                {
                    cPlayerBytes2 = cPlayerBytes2 & 0xFF00FFFF | value << 16;
                }
            }

            public byte FacialHair
            {
                get
                {
                    return (byte)((cPlayerBytes2 & 0xFF) >> 0);
                }

                set
                {
                    cPlayerBytes2 = cPlayerBytes2 & 0xFFFFFF00 | value << 0;
                }
            }

            // cPlayerBytes3 subfields
            // (CInt(Gender) Or (CInt(HonorRank) << 24UI))
            public override Genders Gender
            {
                get
                {
                    return (cBytes0 & 0xFF0000) >> 16;
                }

                set
                {
                    cBytes0 = cBytes0 & 0xFF00FFFF | value << 16;
                    cPlayerBytes3 = cPlayerBytes3 & 0xFFFFFF00 | value << 0;
                }
            }

            public PlayerHonorRank HonorRank
            {
                get
                {
                    return (cPlayerBytes3 & 0xFF000000) >> 24;
                }

                set
                {
                    cPlayerBytes3 = cPlayerBytes3 & 0xFFFFFF | value << 24;
                }
            }

            // cPlayerFieldBytes subfields
            public PlayerHonorRank HonorHighestRank
            {
                get
                {
                    return (cPlayerFieldBytes & 0xFF000000) >> 24;
                }

                set
                {
                    cPlayerFieldBytes = cPlayerFieldBytes & 0xFFFFFF | value << 24;
                }
            }

            // cPlayerFieldBytes2 subfields
            public byte HonorBar
            {
                get
                {
                    return (byte)((cPlayerFieldBytes2 & 0xFF) >> 0);
                }

                set
                {
                    cPlayerFieldBytes2 = cPlayerFieldBytes2 & 0xFFFFFF00 | value << 0;
                }
            }

            public WS_PlayerHelper.TStatBar Rage = new WS_PlayerHelper.TStatBar(1, 1, 0);
            public WS_PlayerHelper.TStatBar Energy = new WS_PlayerHelper.TStatBar(1, 1, 0);
            public WS_PlayerHelper.TStat Strength = new WS_PlayerHelper.TStat();
            public WS_PlayerHelper.TStat Agility = new WS_PlayerHelper.TStat();
            public WS_PlayerHelper.TStat Stamina = new WS_PlayerHelper.TStat();
            public WS_PlayerHelper.TStat Intellect = new WS_PlayerHelper.TStat();
            public WS_PlayerHelper.TStat Spirit = new WS_PlayerHelper.TStat();
            public short Faction = FactionTemplates.None;

            public Mangos.World.Handlers.WS_Combat.TAttackTimer attackState =
                new Mangos.World.Handlers.WS_Combat.TAttackTimer(this);

            public WS_Base.BaseObject attackSelection = null;
            public SHEATHE_SLOT attackSheathState = SHEATHE_SLOT.SHEATHE_NONE;
            public bool Disarmed;

            // Miscellaneous Information
            public int MenuNumber = 0;

            public WS_Base.BaseUnit GetTarget
            {
                get
                {
                    if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(TargetGUID))
                        return WorldServiceLocator._WorldServer.WORLD_CREATUREs[TargetGUID];
                    if (WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(TargetGUID))
                        return WorldServiceLocator._WorldServer.CHARACTERs[TargetGUID];
                    if (WorldServiceLocator._CommonGlobalFunctions.GuidIsPet(TargetGUID))
                        return WorldServiceLocator._WorldServer.WORLD_CREATUREs[TargetGUID];
                    return null;
                }
            }

            public bool CanShootRanged
            {
                get
                {
                    return AmmoID > 0 && Items.ContainsKey(EquipmentSlots.EQUIPMENT_SLOT_RANGED) && this.Items(EquipmentSlots.EQUIPMENT_SLOT_RANGED).IsRanged && this.Items(EquipmentSlots.EQUIPMENT_SLOT_RANGED).IsBroken == false && ItemCOUNT(AmmoID) > 0;
                }
            }

            public float GetRageConversion
            {
                // From http://www.wowwiki.com/Formulas:Rage_generation
                get
                {
                    return (float)(0.0091107836d * Level * Level + 3.225598133d * Level + 4.2652911d);
                }
            }

            public float get_GetHitFactor(bool MainHand, bool Critical)
            {
                float HitFactor = 1.75f;
                if (MainHand)
                    HitFactor *= 2f;
                if (Critical)
                    HitFactor *= 2f;
                return HitFactor;
            }

            public float GetCriticalWithSpells
            {
                // From http://www.wowwiki.com/Spell_critical_strike
                // TODO: Need to add SpellCritical Value in this format -- (Intellect/80 '82 for Warlocks) + (Spell Critical Strike Rating/22.08) + Class Specific Constant
                // How do you generate the base spell crit rating... and then we can fix the formula
                get
                {
                    switch (Classe)
                    {
                        case var @case when @case == Classes.CLASS_DRUID:
                            {
                                return (float)Conversion.Fix(Intellect.Base / 80d + 1.85d);
                            }

                        case var case1 when case1 == Classes.CLASS_MAGE:
                            {
                                return (float)Conversion.Fix(Intellect.Base / 80d + 0.91d);
                            }

                        case var case2 when case2 == Classes.CLASS_PRIEST:
                            {
                                return (float)Conversion.Fix(Intellect.Base / 80d + 1.24d);
                            }

                        case var case3 when case3 == Classes.CLASS_WARLOCK:
                            {
                                return (float)Conversion.Fix(Intellect.Base / 82d + 1.701d);
                            }

                        case var case4 when case4 == Classes.CLASS_PALADIN:
                            {
                                return (float)Conversion.Fix(Intellect.Base / 80d + 3.336d);
                            }

                        case var case5 when case5 == Classes.CLASS_SHAMAN:
                            {
                                return (float)Conversion.Fix(Intellect.Base / 80d + 2.2d);
                            }

                        case var case6 when case6 == Classes.CLASS_HUNTER:
                            {
                                return (float)Conversion.Fix(Intellect.Base / 80d + 3.6d);
                            }

                        default:
                            {
                                // CLASS_ROGUE
                                // CLASS_WARRIOR
                                return 0f;
                            }
                    }
                }
            }

            public WS_Spells.CastSpellParameters[] spellCasted = new WS_Spells.CastSpellParameters[] { null, null, null, null };
            public byte spellCastManaRegeneration = 0;
            public bool spellCanDualWeild = false;
            public WS_PlayerHelper.TDamageBonus healing = new WS_PlayerHelper.TDamageBonus();
            public WS_PlayerHelper.TDamageBonus[] spellDamage = new WS_PlayerHelper.TDamageBonus[7];
            public int spellCriticalRating = 0;
            public bool combatCanDualWield = false;
            public int combatBlock = 0;
            public int combatBlockValue = 0;
            public int combatParry = 0;
            public int combatCrit = 0;
            public int combatDodge = 0;
            public WS_Items.TDamage Damage = new WS_Items.TDamage();
            public WS_Items.TDamage RangedDamage = new WS_Items.TDamage();
            public WS_Items.TDamage OffHandDamage = new WS_Items.TDamage();

            public int BaseUnarmedDamage
            {
                get
                {
                    return (int)((AttackPower + AttackPowerMods) * 0.071428571428571425d);
                }
            }

            public int BaseRangedDamage
            {
                get
                {
                    return (int)((AttackPowerRanged + AttackPowerModsRanged) * 0.071428571428571425d);
                }
            }

            public int AttackPower
            {
                // From http://www.wowwiki.com/Attack_power
                get
                {
                    switch (Classe)
                    {
                        case var @case when @case == Classes.CLASS_WARRIOR:
                        case var case1 when case1 == Classes.CLASS_PALADIN:
                            {
                                return Level * 3 + Strength.Base * 3 - 20;
                            }

                        case var case2 when case2 == Classes.CLASS_SHAMAN:
                            {
                                return Level * 2 + Strength.Base * 2 - 20;
                            }

                        case var case3 when case3 == Classes.CLASS_MAGE:
                        case var case4 when case4 == Classes.CLASS_PRIEST:
                        case var case5 when case5 == Classes.CLASS_WARLOCK:
                            {
                                return Strength.Base - 10;
                            }

                        case var case6 when case6 == Classes.CLASS_ROGUE:
                        case var case7 when case7 == Classes.CLASS_HUNTER:
                            {
                                return Level * 2 + Strength.Base + Agility.Base - 20;
                            }

                        case var case8 when case8 == Classes.CLASS_DRUID:
                            {
                                if (ShapeshiftForm == this.ShapeshiftForm.FORM_CAT)
                                {
                                    return Level * 2 + Strength.Base * 2 + Agility.Base - 20;
                                }
                                else if (ShapeshiftForm == this.ShapeshiftForm.FORM_BEAR | ShapeshiftForm == this.ShapeshiftForm.FORM_DIREBEAR)
                                {
                                    return Level * 3 + Strength.Base * 2 - 20;
                                }
                                else if (ShapeshiftForm == this.ShapeshiftForm.FORM_MOONKIN)
                                {
                                    return (int)(Level * 1.5d + Agility.Base + Strength.Base * 2 - 20d);
                                }
                                else
                                {
                                    return Strength.Base * 2 - 20;
                                }

                                break;
                            }
                    }

                    return 0;
                }
            }

            public int AttackPowerRanged
            {
                // From http://www.wowwiki.com/Attack_power
                get
                {
                    switch (Classe)
                    {
                        case var @case when @case == Classes.CLASS_WARRIOR:
                        case var case1 when case1 == Classes.CLASS_ROGUE:
                            {
                                return Level + Agility.Base - 10;
                            }

                        case var case2 when case2 == Classes.CLASS_HUNTER:
                            {
                                return Level * 2 + Agility.Base - 10;
                            }

                        case var case3 when case3 == Classes.CLASS_PALADIN:
                        case var case4 when case4 == Classes.CLASS_SHAMAN:
                        case var case5 when case5 == Classes.CLASS_MAGE:
                        case var case6 when case6 == Classes.CLASS_PRIEST:
                        case var case7 when case7 == Classes.CLASS_WARLOCK:
                            {
                                return Agility.Base - 10;
                            }

                        case var case8 when case8 == Classes.CLASS_DRUID:
                            {
                                if (ShapeshiftForm == this.ShapeshiftForm.FORM_CAT | ShapeshiftForm == this.ShapeshiftForm.FORM_BEAR | ShapeshiftForm == this.ShapeshiftForm.FORM_DIREBEAR | ShapeshiftForm == this.ShapeshiftForm.FORM_MOONKIN)
                                {
                                    return 0;
                                }
                                else
                                {
                                    return Agility.Base - 10;
                                }

                                break;
                            }
                    }

                    return 0;
                }
            }

            public short get_AttackTime(WeaponAttackType index)
            {
                return Fix(AttackTimeBase[index] * AttackTimeMods[index]);
            }

            public short[] AttackTimeBase = new short[] { 2000, 0, 0 };
            public float[] AttackTimeMods = new float[] { 1.0f, 1.0f, 1.0f };

            // Item Bonuses
            public float ManaRegenerationModifier = WorldServiceLocator._WorldServer.Config.ManaRegenerationRate;
            public float LifeRegenerationModifier = WorldServiceLocator._WorldServer.Config.HealthRegenerationRate;
            public int ManaRegenBonus = 0;
            public float ManaRegenPercent = 1f;
            public int ManaRegen = 0;
            public int ManaRegenInterrupt = 0;
            public int LifeRegenBonus = 0;
            public int RageRegenBonus = 0;

            public short GetStat(byte Type)
            {
                switch (Type)
                {
                    case 0:
                        {
                            return (short)Strength.Base;
                        }

                    case 1:
                        {
                            return (short)Agility.Base;
                        }

                    case 2:
                        {
                            return (short)Stamina.Base;
                        }

                    case 3:
                        {
                            return (short)Intellect.Base;
                        }

                    case 4:
                        {
                            return (short)Spirit.Base;
                        }

                    default:
                        {
                            return 0;
                        }
                }
            }

            public void UpdateManaRegen()
            {
                if (FullyLoggedIn == false)
                    return;
                float PowerRegen = (float)(Math.Sqrt(Intellect.Base) * 1d); // GetOCTRegenMP()
                if (float.IsNaN(PowerRegen))
                    PowerRegen = 1f; // Clear an invalid PowerRegen
                PowerRegen *= ManaRegenPercent;
                float PowerRegenMP5 = (float)(ManaRegenBonus / 5d);
                int PowerRegenInterrupt = 0;
                for (int i = 0, loopTo = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs - 1; i <= loopTo; i++)
                {
                    if (ActiveSpells[i] is object)
                    {
                        for (byte j = 0; j <= 2; j++)
                        {
                            if (ActiveSpells[i].Aura_Info[j] is object)
                            {
                                if (ActiveSpells[i].Aura_Info[j].ApplyAuraIndex == AuraEffects_Names.SPELL_AURA_MOD_MANA_REGEN_FROM_STAT)
                                {
                                    PowerRegenMP5 += GetStat((byte)ActiveSpells[i].Aura_Info[j].MiscValue) * ActiveSpells[i].Aura_Info[j].get_GetValue(Level, 0) / 500.0f;
                                }
                                else if (ActiveSpells[i].SpellID == 34074 && ActiveSpells[i].Aura_Info[j].ApplyAuraIndex == AuraEffects_Names.SPELL_AURA_PERIODIC_DUMMY)
                                {
                                    PowerRegenMP5 = (float)(PowerRegenMP5 + (ActiveSpells[i].Aura_Info[j].get_GetValue(Level, 0) * Intellect.Base / 500.0f + Level * 35 / 100d));
                                }
                                else if (ActiveSpells[i].Aura_Info[j].ApplyAuraIndex == AuraEffects_Names.SPELL_AURA_MOD_MANA_REGEN_INTERRUPT)
                                {
                                    PowerRegenInterrupt += ActiveSpells[i].Aura_Info[j].get_GetValue(Level, 0);
                                }
                            }
                        }
                    }
                }

                if (PowerRegenInterrupt > 100)
                    PowerRegenInterrupt = 100;
                PowerRegenInterrupt = (int)(PowerRegenMP5 + PowerRegen * PowerRegenInterrupt / 100.0f);
                PowerRegen = (int)(PowerRegenMP5 + PowerRegen);
                ManaRegen = (int)PowerRegen;
                ManaRegenInterrupt = PowerRegenInterrupt;
            }

            // Temporaly variables
            public LANGUAGES Spell_Language = -1;

            // Pets
            public WS_Pets.PetObject Pet = null;

            // Honor And Arena
            public int HonorPoints = 0;
            public int StandingLastWeek = 0;
            public int HonorKillsLifeTime = 0;
            public int DishonorKillsLifeTime = 0;
            public int HonorPointsLastWeek = 0;
            public int HonorPointsThisWeek = 0;
            public int HonorPointsYesterday = 0;
            public int HonorKillsLastWeek = 0;
            public int HonorKillsThisWeek = 0;
            public short HonorKillsYesterday = 0;
            public short HonorKillsToday = 0;
            public short DishonorKillsToday = 0;

            public void HonorSaveAsNew()
            {
                WorldServiceLocator._WorldServer.CharacterDatabase.Update("INSERT INTO characters_honor (char_guid)  VALUES (" + GUID + ");");
            }

            // Done: Player Honor Save
            public void HonorSave()
            {
                string honor = "UPDATE characters_honor SET";
                honor = honor + ", honor_points =" + HonorPoints;
                honor = honor + ", kills_honor =" + HonorKillsLifeTime;
                honor = honor + ", kills_dishonor =" + DishonorKillsLifeTime;
                honor = honor + ", honor_yesterday =" + HonorPointsYesterday;
                honor = honor + ", honor_thisWeek =" + HonorPointsThisWeek;
                honor = honor + ", kills_thisWeek =" + HonorKillsThisWeek;
                honor = honor + ", kills_today =" + HonorKillsToday;
                honor = honor + ", kills_dishonortoday =" + DishonorKillsToday;
                honor += string.Format(" WHERE char_guid = \"{0}\";", GUID);
                WorldServiceLocator._WorldServer.CharacterDatabase.Update(honor);
            }

            // Done: Player Honor Load
            public void HonorLoad()
            {
                var MySQLQuery = new DataTable();
                WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT * FROM characters_honor WHERE char_guid = {0};", (object)GUID), MySQLQuery);
                if (MySQLQuery.Rows.Count == 0)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Unable to get SQLDataBase honor info for character [GUID={0:X}]", GUID);
                    return;
                }

                HonorPoints = Conversions.ToInteger(MySQLQuery.Rows[0]["honor_points"]);
                HonorRank = MySQLQuery.Rows[0]["honor_rank"];
                HonorHighestRank = MySQLQuery.Rows[0]["honor_hightestRank"];
                StandingLastWeek = Conversions.ToInteger(MySQLQuery.Rows[0]["standing_lastweek"]);
                HonorKillsLifeTime = Conversions.ToInteger(MySQLQuery.Rows[0]["kills_honor"]);
                DishonorKillsLifeTime = Conversions.ToInteger(MySQLQuery.Rows[0]["kills_dishonor"]);
                HonorPointsLastWeek = Conversions.ToInteger(MySQLQuery.Rows[0]["honor_lastWeek"]);
                HonorPointsThisWeek = Conversions.ToInteger(MySQLQuery.Rows[0]["honor_thisWeek"]);
                HonorPointsYesterday = Conversions.ToInteger(MySQLQuery.Rows[0]["honor_yesterday"]);
                HonorKillsLastWeek = Conversions.ToInteger(MySQLQuery.Rows[0]["kills_lastWeek"]);
                HonorKillsThisWeek = Conversions.ToInteger(MySQLQuery.Rows[0]["kills_thisWeek"]);
                HonorKillsYesterday = Conversions.ToShort(MySQLQuery.Rows[0]["kills_yesterday"]);
                HonorKillsToday = Conversions.ToShort(MySQLQuery.Rows[0]["kills_today"]);
                DishonorKillsToday = Conversions.ToShort(MySQLQuery.Rows[0]["kills_dishonortoday"]);
                MySQLQuery.Dispose();
            }

            public void HonorLog(int honorPoints, ulong victimGUID, int victimRank)
            {
                // GUID = 0 : You have been awarded %h honor points.
                // GUID <>0 : %p dies, honorable kill Rank: %r (Estimated Honor Points: %h)

                var packet = new Packets.PacketClass(OPCODES.SMSG_PVP_CREDIT);
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

            public uint Copper = 0U;
            public string Name = "";
            public Dictionary<byte, WS_PlayerHelper.TActionButton> ActionButtons = new Dictionary<byte, WS_PlayerHelper.TActionButton>();
            public BitArray TaxiZones = new BitArray(8 * 32, false);
            public Queue<int> TaxiNodes = new Queue<int>();
            public uint[] ZonesExplored = new uint[] { 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U };
            public float WalkSpeed = WorldServiceLocator._Global_Constants.UNIT_NORMAL_WALK_SPEED;
            public float RunSpeed = WorldServiceLocator._Global_Constants.UNIT_NORMAL_RUN_SPEED;
            public float RunBackSpeed = WorldServiceLocator._Global_Constants.UNIT_NORMAL_WALK_BACK_SPEED;
            public float SwimSpeed = WorldServiceLocator._Global_Constants.UNIT_NORMAL_SWIM_SPEED;
            public float SwimBackSpeed = WorldServiceLocator._Global_Constants.UNIT_NORMAL_SWIM_BACK_SPEED;
            public float TurnRate = WorldServiceLocator._Global_Constants.UNIT_NORMAL_TURN_RATE;
            public int charMovementFlags = 0;
            public int ZoneID = 0;
            public int AreaID = 0;
            public float bindpoint_positionX = 0f;
            public float bindpoint_positionY = 0f;
            public float bindpoint_positionZ = 0f;
            public int bindpoint_map_id = 0;
            public int bindpoint_zone_id = 0;
            public bool DEAD = false;

            public uint ClassMask
            {
                get
                {
                    return 1 << Classe - 1;
                }
            }

            public uint RaceMask
            {
                get
                {
                    return 1 << Race - 1;
                }
            }

            public override bool IsDead
            {
                get
                {
                    return DEAD;
                }
            }

            public bool isMoving
            {
                get
                {
                    return WorldServiceLocator._Global_Constants.movementFlagsMask & charMovementFlags;
                }
            }

            public bool isTurning
            {
                get
                {
                    return WorldServiceLocator._Global_Constants.TurningFlagsMask & charMovementFlags;
                }
            }

            public bool isMovingOrTurning
            {
                get
                {
                    return WorldServiceLocator._Global_Constants.movementOrTurningFlagsMask & charMovementFlags;
                }
            }

            public bool isPvP
            {
                get
                {
                    return cUnitFlags & UnitFlags.UNIT_FLAG_PVP;
                }

                set
                {
                    if (value)
                    {
                        cUnitFlags = cUnitFlags | UnitFlags.UNIT_FLAG_PVP;
                    }
                    else
                    {
                        cUnitFlags = cUnitFlags & !UnitFlags.UNIT_FLAG_PVP;
                    }
                }
            }

            public bool isResting
            {
                get
                {
                    return cPlayerFlags & PlayerFlags.PLAYER_FLAGS_RESTING;
                }
            }

            public bool exploreCheckQueued_ = false;
            public bool outsideMapID_ = false;
            public int antiHackSpeedChanged_ = 0;
            public WS_PlayerHelper.TDrowningTimer underWaterTimer = null;
            public bool underWaterBreathing = false;
            public ulong lootGUID = 0UL;
            public WS_PlayerHelper.TRepopTimer repopTimer = null;
            public WS_Handlers_Trade.TTradeInfo tradeInfo = null;
            public ulong corpseGUID = 0UL;
            public int corpseMapID = 0;
            public CorpseType corpseCorpseType;
            public float corpsePositionX = 0f;
            public float corpsePositionY = 0f;
            public float corpsePositionZ = 0f;
            public ulong resurrectGUID = 0UL;
            public int resurrectMapID = 0;
            public float resurrectPositionX = 0f;
            public float resurrectPositionY = 0f;
            public float resurrectPositionZ = 0f;
            public int resurrectHealth = 0;
            public int resurrectMana = 0;
            public ReaderWriterLock guidsForRemoving_Lock = new ReaderWriterLock();
            public List<ulong> guidsForRemoving = new List<ulong>();
            public List<ulong> creaturesNear = new List<ulong>();
            public List<ulong> playersNear = new List<ulong>();
            public List<ulong> gameObjectsNear = new List<ulong>();
            public List<ulong> dynamicObjectsNear = new List<ulong>();
            public List<ulong> corpseObjectsNear = new List<ulong>();
            public List<ulong> inCombatWith = new List<ulong>();
            public int lastPvpAction = 0;

            public override bool IsFriendlyTo(ref WS_Base.BaseUnit Unit)
            {
                if (ReferenceEquals(Unit, this))
                    return true;
                if (Unit is CharacterObject)
                {
                    {
                        var withBlock = (CharacterObject)Unit;
                        if (withBlock.GM)
                            return true;
                        if (DuelPartner is object && ReferenceEquals(DuelPartner, Unit))
                            return false;
                        if (withBlock.DuelPartner is object && ReferenceEquals(withBlock.DuelPartner, this))
                            return false;
                        if (IsInGroup && withBlock.IsInGroup && ReferenceEquals(Group, withBlock.Group))
                            return true;
                        if (WorldServiceLocator._Functions.HaveFlags(cPlayerFlags, PlayerFlags.PLAYER_FLAGS_FFA_PVP) && WorldServiceLocator._Functions.HaveFlags(withBlock.cPlayerFlags, PlayerFlags.PLAYER_FLAGS_FFA_PVP))
                            return false;
                        if (Team == withBlock.Team)
                            return true;
                        return !withBlock.isPvP;
                    }
                }
                else if (Unit is WS_Creatures.CreatureObject)
                {
                    {
                        var withBlock1 = (WS_Creatures.CreatureObject)Unit;
                        if (GetReputation(withBlock1.Faction) < ReputationRank.Friendly)
                            return false;
                        if (GetReaction(withBlock1.Faction) < TReaction.NEUTRAL)
                            return false;

                        // TODO: At war with faction?
                    }
                }

                return true;
            }

            public override bool IsEnemyTo(ref WS_Base.BaseUnit Unit)
            {
                if (ReferenceEquals(Unit, this))
                    return false;
                if (Unit is CharacterObject)
                {
                    {
                        var withBlock = (CharacterObject)Unit;
                        if (withBlock.GM)
                            return false;
                        if (DuelPartner is object && ReferenceEquals(DuelPartner, Unit))
                            return true;
                        if (withBlock.DuelPartner is object && ReferenceEquals(withBlock.DuelPartner, this))
                            return true;
                        if (IsInGroup && withBlock.IsInGroup && ReferenceEquals(Group, withBlock.Group))
                            return false;
                        if (WorldServiceLocator._Functions.HaveFlags(cPlayerFlags, PlayerFlags.PLAYER_FLAGS_FFA_PVP) && WorldServiceLocator._Functions.HaveFlags(withBlock.cPlayerFlags, PlayerFlags.PLAYER_FLAGS_FFA_PVP))
                            return true;
                        if (Team == withBlock.Team)
                            return false;
                        return withBlock.isPvP;
                    }
                }
                else if (Unit is WS_Creatures.CreatureObject)
                {
                    {
                        var withBlock1 = (WS_Creatures.CreatureObject)Unit;
                        if (GetReputation(withBlock1.Faction) < ReputationRank.Neutral)
                            return true;
                        if (GetReaction(withBlock1.Faction) < TReaction.NEUTRAL)
                            return true;

                        // TODO: At war with faction?
                    }
                }

                return false;
            }

            public bool IsInCombat
            {
                get
                {
                    return inCombatWith.Count > 0 || WorldServiceLocator._NativeMethods.timeGetTime("") - lastPvpAction < WorldServiceLocator._Global_Constants.DEFAULT_PVP_COMBAT_TIME;
                }
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
                        return;
                    inCombatWith.Add(Unit.GUID);
                }

                CheckCombat();
            }

            public void RemoveFromCombat(WS_Base.BaseUnit Unit)
            {
                if (!inCombatWith.Contains(Unit.GUID))
                    return;
                inCombatWith.Remove(Unit.GUID);
                CheckCombat();
            }

            // NOTE: This function removes combat if there's no one else in your combat array
            public void CheckCombat()
            {
                if (cUnitFlags & UnitFlags.UNIT_FLAG_IN_COMBAT)
                {
                    if (IsInCombat)
                        return;
                    var argobjCharacter = this;
                    WorldServiceLocator._WS_Combat.SetPlayerOutOfCombat(ref argobjCharacter);
                }
                else
                {
                    if (!IsInCombat)
                        return;
                    var argobjCharacter1 = this;
                    WorldServiceLocator._WS_Combat.SetPlayerInCombat(ref argobjCharacter1);
                }
            }

            public override bool CanSee(ref WS_Base.BaseObject objCharacter)
            {
                if (GUID == objCharacter.GUID)
                    return false;
                if (instance != objCharacter.instance)
                    return false;
                if (objCharacter.MapID != MapID)
                    return false;
                if (objCharacter is WS_Creatures.CreatureObject)
                {
                    if (((WS_Creatures.CreatureObject)objCharacter).aiScript is object)
                    {
                        if (((WS_Creatures.CreatureObject)objCharacter).aiScript.State == AIState.AI_RESPAWN)
                            return false;
                    }
                }
                else if (objCharacter is WS_GameObjects.GameObjectObject)
                {
                    if (((WS_GameObjects.GameObjectObject)objCharacter).Despawned)
                        return false;
                }

                float distance = WorldServiceLocator._WS_Combat.GetDistance(this, objCharacter);

                // DONE: See party members
                if (Group is object && objCharacter is CharacterObject)
                {
                    if (ReferenceEquals(((CharacterObject)objCharacter).Group, Group))
                    {
                        if (distance > objCharacter.VisibleDistance)
                            return false;
                        else
                            return true;
                    }
                }

                // DONE: Check dead
                if (DEAD && corpseGUID != 0UL)
                {
                    // DONE: See only dead
                    if (corpseGUID == objCharacter.GUID)
                        return true;
                    if (WorldServiceLocator._WS_Combat.GetDistance(objCharacter, corpsePositionX, corpsePositionY, corpsePositionZ) < objCharacter.VisibleDistance)
                    {
                        // DONE: GM and DEAD invisibility
                        if (objCharacter.Invisibility > CanSeeInvisibility)
                            return false;
                        // DONE: Stealth Detection
                        float localGetStealthDistance() { WS_Base.BaseUnit argobjCharacter = (WS_Base.BaseUnit)objCharacter; var ret = GetStealthDistance(ref argobjCharacter); return ret; }

                        if (objCharacter.Invisibility == InvisibilityLevel.STEALTH && distance < localGetStealthDistance())
                            return true;
                        // DONE: Check invisibility
                        if (objCharacter.Invisibility == InvisibilityLevel.INIVISIBILITY && objCharacter.Invisibility_Value > CanSeeInvisibility_Invisibility)
                            return false;
                        if (objCharacter.Invisibility == InvisibilityLevel.STEALTH && CanSeeStealth == false)
                            return false;
                        return true;
                    }

                    if (objCharacter.Invisibility != InvisibilityLevel.DEAD)
                        return false;
                }
                else if (Invisibility == InvisibilityLevel.INIVISIBILITY)
                {
                    // DONE: See only invisible, or people who can see invisibility
                    if (objCharacter.Invisibility != InvisibilityLevel.INIVISIBILITY)
                    {
                        if (objCharacter.CanSeeInvisibility_Invisibility >= Invisibility_Value)
                            return true;
                        return false;
                    }

                    if (Invisibility_Value < objCharacter.Invisibility_Value)
                        return false;
                }
                else
                {
                    // DONE: GM and DEAD invisibility
                    if (objCharacter.Invisibility > CanSeeInvisibility)
                        return false;
                    // DONE: Stealth Detection
                    float localGetStealthDistance1() { WS_Base.BaseUnit argobjCharacter = (WS_Base.BaseUnit)objCharacter; var ret = GetStealthDistance(ref argobjCharacter); return ret; }

                    if (objCharacter.Invisibility == InvisibilityLevel.STEALTH && distance < localGetStealthDistance1())
                        return true;
                    // DONE: Check invisibility
                    if (objCharacter.Invisibility == InvisibilityLevel.INIVISIBILITY && objCharacter.Invisibility_Value > CanSeeInvisibility_Invisibility)
                        return false;
                    if (objCharacter.Invisibility == InvisibilityLevel.STEALTH && CanSeeStealth == false)
                        return false;
                }

                // DONE: Check distance
                if (distance > objCharacter.VisibleDistance)
                    return false;
                return true;
            }

            public byte[] TutorialFlags = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            // Updating
            private readonly BitArray UpdateMask = new BitArray(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER, false);
            private readonly Hashtable UpdateData = new Hashtable();

            public void SetUpdateFlag(int pos, int value)
            {
                UpdateMask.Set(pos, true);
                UpdateData[pos] = value;
            }

            public void SetUpdateFlag(int pos, uint value)
            {
                UpdateMask.Set(pos, true);
                UpdateData[pos] = value;
            }

            public void SetUpdateFlag(int pos, long value)
            {
                UpdateMask.Set(pos, true);
                UpdateMask.Set(pos + 1, true);
                UpdateData[pos] = Conversions.ToInteger(value & uint.MaxValue);
                UpdateData[pos + 1] = Conversions.ToInteger(value >> 32 & uint.MaxValue);
            }

            public void SetUpdateFlag(int pos, ulong value)
            {
                UpdateMask.Set(pos, true);
                UpdateMask.Set(pos + 1, true);
                UpdateData[pos] = Conversions.ToUInteger(value & uint.MaxValue);
                UpdateData[pos + 1] = Conversions.ToUInteger(value >> 32 & uint.MaxValue);
            }

            public void SetUpdateFlag(int pos, float value)
            {
                UpdateMask.Set(pos, true);
                UpdateData[pos] = value;
            }

            public void SendOutOfRangeUpdate()
            {
                ulong[] GUIDs;
                guidsForRemoving_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                GUIDs = guidsForRemoving.ToArray();
                guidsForRemoving.Clear();
                guidsForRemoving_Lock.ReleaseWriterLock();
                if (GUIDs.Length > 0)
                {
                    var packet = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                    try
                    {
                        packet.AddInt32(1);      // Operations.Count
                        packet.AddInt8(0);
                        packet.AddInt8(ObjectUpdateType.UPDATETYPE_OUT_OF_RANGE_OBJECTS);
                        packet.AddInt32(GUIDs.Length);
                        foreach (ulong g in GUIDs)
                            packet.AddPackGUID(g);
                        client.Send(ref packet);
                    }
                    finally
                    {
                        packet.Dispose();
                    }
                }
            }

            public void SendUpdate()
            {
                int updateCount = 1 + Items.Count;
                if (OnTransport is object)
                    updateCount += 1;
                var packet = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                try
                {
                    packet.AddInt32(updateCount);
                    packet.AddInt8(0);

                    // DONE: If character is on a transport, create the transport right here
                    if (OnTransport is object)
                    {
                        var tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_GAMEOBJECT);
                        var argCharacter = this;
                        OnTransport.FillAllUpdateFlags(ref tmpUpdate, ref argCharacter);
                        tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, OnTransport);
                        tmpUpdate.Dispose();
                        gameObjectsNear.Add(OnTransport.GUID);
                        OnTransport.SeenBy.Add(GUID);
                    }

                    PrepareUpdate(ref packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF);
                    foreach (KeyValuePair<byte, ItemObject> tmpItem in Items)
                    {
                        var tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_ITEM);
                        tmpItem.Value.FillAllUpdateFlags(ref tmpUpdate);
                        tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, tmpItem.Value);
                        tmpUpdate.Dispose();

                        // DONE: Update Items In bag
                        if (tmpItem.Value.ItemInfo.IsContainer)
                        {
                            tmpItem.Value.SendContainedItemsUpdate(ref client, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT);
                        }
                    }

                    packet.CompressUpdatePacket();
                    client.Send(ref packet);
                }
                finally
                {
                    packet.Dispose();
                }
                // DONE: Create everyone on the transport if we are located on one
                if (OnTransport is object && OnTransport is WS_Transports.TransportObject)
                {
                    var argCharacter1 = this;
                    ((WS_Transports.TransportObject)OnTransport).CreateEveryoneOnTransport(ref argCharacter1);
                }
            }

            public void SendItemUpdate(ItemObject Item)
            {
                var packet = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                try
                {
                    packet.AddInt32(1);      // Operations.Count
                    packet.AddInt8(0);
                    var tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_ITEM);
                    Item.FillAllUpdateFlags(ref tmpUpdate);
                    tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, Item);
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
                var packet = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                try
                {
                    packet.AddInt32(1);      // Operations.Count
                    packet.AddInt8(0);
                    for (byte i = 0, loopTo = InventoryPackSlots.INVENTORY_SLOT_ITEM_END - 1; i <= loopTo; i++)
                    {
                        if (Items.ContainsKey(i))
                        {
                            SetUpdateFlag(EPlayerFields.PLAYER_FIELD_INV_SLOT_HEAD + (int)i * 2, Items[i].GUID);
                            if (i < EquipmentSlots.EQUIPMENT_SLOT_END)
                            {
                                SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_0 + i * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE, Items[i].ItemEntry);

                                // SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_1 + (i * _Global_Constants.PLAYER_VISIBLE_ITEM_SIZE), 0)           'ITEM_FIELD_ENCHANTMENT
                                // SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_2 + (i * _Global_Constants.PLAYER_VISIBLE_ITEM_SIZE), 0)           'ITEM_FIELD_ENCHANTMENT + 3
                                // SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_3 + (i * _Global_Constants.PLAYER_VISIBLE_ITEM_SIZE), 0)           'ITEM_FIELD_ENCHANTMENT + 6
                                // SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_4 + (i * _Global_Constants.PLAYER_VISIBLE_ITEM_SIZE), 0)           'ITEM_FIELD_ENCHANTMENT + 9
                                // SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_5 + (i * _Global_Constants.PLAYER_VISIBLE_ITEM_SIZE), 0)           'ITEM_FIELD_ENCHANTMENT + 12
                                // SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_6 + (i * _Global_Constants.PLAYER_VISIBLE_ITEM_SIZE), 0)           'ITEM_FIELD_ENCHANTMENT + 15
                                // SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_7 + (i * _Global_Constants.PLAYER_VISIBLE_ITEM_SIZE), 0)           'ITEM_FIELD_ENCHANTMENT + 18
                                SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_PROPERTIES + i * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE, 0);   // ITEM_FIELD_RANDOM_PROPERTIES_ID
                            }
                        }
                        else
                        {
                            SetUpdateFlag(EPlayerFields.PLAYER_FIELD_INV_SLOT_HEAD + (int)i * 2, Conversions.ToLong(0));
                            if (i < EquipmentSlots.EQUIPMENT_SLOT_END)
                            {
                                SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_0 + i * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE, 0);
                            }
                        }
                    }

                    PrepareUpdate(ref packet, ObjectUpdateType.UPDATETYPE_VALUES);
                    client.Send(ref packet);
                }
                finally
                {
                    packet.Dispose();
                }
            }

            public void SendItemAndCharacterUpdate(ItemObject Item, int UPDATETYPE = ObjectUpdateType.UPDATETYPE_VALUES)
            {
                var packet = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                var tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_ITEM);
                try
                {
                    packet.AddInt32(2);      // Operations.Count
                    packet.AddInt8(0);

                    // DONE: Send to self
                    Item.FillAllUpdateFlags(ref tmpUpdate);
                    tmpUpdate.AddToPacket(packet, UPDATETYPE, Item);
                    for (byte i = EquipmentSlots.EQUIPMENT_SLOT_START, loopTo = KeyRingSlots.KEYRING_SLOT_END - 1; i <= loopTo; i++)
                    {
                        if (Items.ContainsKey(i))
                        {
                            SetUpdateFlag(EPlayerFields.PLAYER_FIELD_INV_SLOT_HEAD + (int)i * 2, Items[i].GUID);
                            if (i < EquipmentSlots.EQUIPMENT_SLOT_END)
                            {
                                SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_0 + i * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE, Items[i].ItemEntry);
                                SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_PROPERTIES + i * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE, Items[i].RandomProperties);   // ITEM_FIELD_RANDOM_PROPERTIES_ID
                            }
                        }
                        else
                        {
                            SetUpdateFlag(EPlayerFields.PLAYER_FIELD_INV_SLOT_HEAD + (int)i * 2, Conversions.ToULong(0));
                            if (i < EquipmentSlots.EQUIPMENT_SLOT_END)
                            {
                                SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_0 + i * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE, 0);
                            }
                        }
                    }

                    PrepareUpdate(ref packet, ObjectUpdateType.UPDATETYPE_VALUES);
                    client.Send(ref packet);
                }
                finally
                {
                    packet.Dispose();
                    tmpUpdate.Dispose();
                }

                // DONE: Send to others
                for (byte i = EquipmentSlots.EQUIPMENT_SLOT_START, loopTo1 = EquipmentSlots.EQUIPMENT_SLOT_END - 1; i <= loopTo1; i++)
                {
                    if (Items.ContainsKey(i))
                    {
                        SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_0 + i * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE, Items[i].ItemEntry);
                        SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_PROPERTIES + i * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE, Items[i].RandomProperties);   // ITEM_FIELD_RANDOM_PROPERTIES_ID
                    }
                    else
                    {
                        SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_0 + i * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE, 0);
                    }
                }

                SendCharacterUpdate(true, true);
            }

            public void SendCharacterUpdate(bool toNear = true, bool notMe = false)
            {
                if (UpdateData.Count == 0)
                    return;

                // DONE: Send to near
                if (toNear && SeenBy.Count > 0)
                {
                    var forOthers = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER)
                    {
                        UpdateData = (Hashtable)UpdateData.Clone(),
                        UpdateMask = (BitArray)UpdateMask.Clone()
                    };
                    var packetForOthers = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                    try
                    {
                        packetForOthers.AddInt32(1);       // Operations.Count
                        packetForOthers.AddInt8(0);
                        forOthers.AddToPacket(packetForOthers, ObjectUpdateType.UPDATETYPE_VALUES, this);
                        SendToNearPlayers(ref packetForOthers);
                    }
                    finally
                    {
                        packetForOthers.Dispose();
                    }
                }

                if (notMe)
                    return;
                if (client is null)
                    return;

                // DONE: Send to me
                var packet = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                try
                {
                    packet.AddInt32(1);       // Operations.Count
                    packet.AddInt8(0);
                    PrepareUpdate(ref packet, ObjectUpdateType.UPDATETYPE_VALUES);
                    client.Send(ref packet);
                }
                finally
                {
                    packet.Dispose();
                }
            }                                      // Sends update for character to him and near players

            public void FillStatsUpdateFlags()
            {
                SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXHEALTH, Life.Maximum);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXPOWER1, Mana.Maximum);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXPOWER2, Rage.Maximum);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXPOWER4, Energy.Maximum);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXPOWER5, 0);
                SetUpdateFlag(EPlayerFields.PLAYER_BLOCK_PERCENTAGE, combatBlock);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_MINDAMAGE, Damage.Minimum);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXDAMAGE, Damage.Maximum + (float)BaseUnarmedDamage);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_MINOFFHANDDAMAGE, OffHandDamage.Minimum);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXOFFHANDDAMAGE, OffHandDamage.Maximum);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_MINRANGEDDAMAGE, RangedDamage.Minimum);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXRANGEDDAMAGE, RangedDamage.Maximum + (float)BaseRangedDamage);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_BASEATTACKTIME, AttackTime(0));
                SetUpdateFlag(EUnitFields.UNIT_FIELD_RANGEDATTACKTIME, AttackTime(1));
                SetUpdateFlag(EUnitFields.UNIT_FIELD_RANGEDATTACKTIME, AttackTime(2));
                WS_Base.BaseUnit argobjCharacter = this;
                SetUpdateFlag(EPlayerFields.PLAYER_BLOCK_PERCENTAGE, WorldServiceLocator._WS_Combat.GetBasePercentBlock(ref argobjCharacter, 0));
                WS_Base.BaseUnit argobjCharacter1 = this;
                SetUpdateFlag(EPlayerFields.PLAYER_DODGE_PERCENTAGE, WorldServiceLocator._WS_Combat.GetBasePercentDodge(ref argobjCharacter1, 0));
                WS_Base.BaseUnit argobjCharacter2 = this;
                SetUpdateFlag(EPlayerFields.PLAYER_PARRY_PERCENTAGE, WorldServiceLocator._WS_Combat.GetBasePercentParry(ref argobjCharacter2, 0));
                WS_Base.BaseUnit argobjCharacter3 = this;
                SetUpdateFlag(EPlayerFields.PLAYER_CRIT_PERCENTAGE, WorldServiceLocator._WS_Combat.GetBasePercentCrit(ref argobjCharacter3, 0));
                SetUpdateFlag(EPlayerFields.PLAYER_FIELD_COINAGE, Copper);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_STAT0, Strength.Base);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_STAT1, Agility.Base);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_STAT2, Stamina.Base);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_STAT3, Intellect.Base);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_STAT4, Spirit.Base);
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_POSSTAT0, CType(Strength.PositiveBonus, Integer))
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_POSSTAT1, CType(Agility.PositiveBonus, Integer))
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_POSSTAT2, CType(Stamina.PositiveBonus, Integer))
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_POSSTAT3, CType(Intellect.PositiveBonus, Integer))
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_POSSTAT4, CType(Spirit.PositiveBonus, Integer))
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_NEGSTAT0, CType(Strength.NegativeBonus, Integer))
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_NEGSTAT1, CType(Agility.NegativeBonus, Integer))
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_NEGSTAT2, CType(Stamina.NegativeBonus, Integer))
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_NEGSTAT3, CType(Intellect.NegativeBonus, Integer))
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_NEGSTAT4, CType(Spirit.NegativeBonus, Integer))

                SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_PHYSICAL, Resistances[DamageTypes.DMG_PHYSICAL].RealBase);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_HOLY, Resistances[DamageTypes.DMG_HOLY].RealBase);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_FIRE, Resistances[DamageTypes.DMG_FIRE].RealBase);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_NATURE, Resistances[DamageTypes.DMG_NATURE].RealBase);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_FROST, Resistances[DamageTypes.DMG_FROST].RealBase);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_SHADOW, Resistances[DamageTypes.DMG_SHADOW].RealBase);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_ARCANE, Resistances[DamageTypes.DMG_ARCANE].RealBase);

                // SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + DamageTypes.DMG_PHYSICAL, Resistances(DamageTypes.DMG_PHYSICAL).PositiveBonus)
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + DamageTypes.DMG_HOLY, Resistances(DamageTypes.DMG_HOLY).PositiveBonus)
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + DamageTypes.DMG_FIRE, Resistances(DamageTypes.DMG_FIRE).PositiveBonus)
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + DamageTypes.DMG_NATURE, Resistances(DamageTypes.DMG_NATURE).PositiveBonus)
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + DamageTypes.DMG_FROST, Resistances(DamageTypes.DMG_FROST).PositiveBonus)
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + DamageTypes.DMG_SHADOW, Resistances(DamageTypes.DMG_SHADOW).PositiveBonus)
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + DamageTypes.DMG_ARCANE, Resistances(DamageTypes.DMG_ARCANE).PositiveBonus)

                // SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + DamageTypes.DMG_PHYSICAL, Resistances(DamageTypes.DMG_PHYSICAL).NegativeBonus)
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + DamageTypes.DMG_HOLY, Resistances(DamageTypes.DMG_HOLY).NegativeBonus)
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + DamageTypes.DMG_FIRE, Resistances(DamageTypes.DMG_FIRE).NegativeBonus)
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + DamageTypes.DMG_NATURE, Resistances(DamageTypes.DMG_NATURE).NegativeBonus)
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + DamageTypes.DMG_FROST, Resistances(DamageTypes.DMG_FROST).NegativeBonus)
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + DamageTypes.DMG_SHADOW, Resistances(DamageTypes.DMG_SHADOW).NegativeBonus)
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + DamageTypes.DMG_ARCANE, Resistances(DamageTypes.DMG_ARCANE).NegativeBonus)
            }                                     // Used for this player's stats updates

            public void FillAllUpdateFlags()
            {
                SetUpdateFlag(EObjectFields.OBJECT_FIELD_GUID, GUID);
                SetUpdateFlag(EObjectFields.OBJECT_FIELD_TYPE, 25);
                SetUpdateFlag(EObjectFields.OBJECT_FIELD_SCALE_X, Size);
                if (Pet is object)
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_SUMMON, Pet.GUID);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_HEALTH, Life.Current);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER1, Mana.Current);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER2, Rage.Current);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER4, Energy.Current);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER5, 0);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXHEALTH, Life.Maximum);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXPOWER1, Mana.Maximum);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXPOWER2, Rage.Maximum);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXPOWER4, Energy.Maximum);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXPOWER5, 0);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_BASE_HEALTH, Life.Base);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_BASE_MANA, Mana.Base);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_LEVEL, Level);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_FACTIONTEMPLATE, Faction);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, cUnitFlags);
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS_2, 0)
                SetUpdateFlag(EUnitFields.UNIT_FIELD_STAT0, Strength.Base);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_STAT1, Agility.Base);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_STAT2, Stamina.Base);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_STAT3, Spirit.Base);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_STAT4, Intellect.Base);
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_POSSTAT0, CType(Strength.PositiveBonus, Integer))
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_POSSTAT1, CType(Agility.PositiveBonus, Integer))
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_POSSTAT2, CType(Stamina.PositiveBonus, Integer))
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_POSSTAT3, CType(Intellect.PositiveBonus, Integer))
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_POSSTAT4, CType(Spirit.PositiveBonus, Integer))
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_NEGSTAT0, CType(Strength.NegativeBonus, Integer))
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_NEGSTAT1, CType(Agility.NegativeBonus, Integer))
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_NEGSTAT2, CType(Stamina.NegativeBonus, Integer))
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_NEGSTAT3, CType(Intellect.NegativeBonus, Integer))
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_NEGSTAT4, CType(Spirit.NegativeBonus, Integer))
                SetUpdateFlag(EUnitFields.UNIT_FIELD_BYTES_0, cBytes0);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_BYTES_1, cBytes1);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_BYTES_2, cBytes2);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_DISPLAYID, Model);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_NATIVEDISPLAYID, Model_Native);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_MOUNTDISPLAYID, Mount);
                SetUpdateFlag(EUnitFields.UNIT_DYNAMIC_FLAGS, cDynamicFlags);
                SetUpdateFlag(EPlayerFields.PLAYER_BYTES, cPlayerBytes);
                SetUpdateFlag(EPlayerFields.PLAYER_BYTES_2, cPlayerBytes2);
                SetUpdateFlag(EPlayerFields.PLAYER_BYTES_3, cPlayerBytes3);
                SetUpdateFlag(EPlayerFields.PLAYER_FIELD_WATCHED_FACTION_INDEX, WatchedFactionIndex);
                SetUpdateFlag(EPlayerFields.PLAYER_XP, XP);
                SetUpdateFlag(EPlayerFields.PLAYER_NEXT_LEVEL_XP, WorldServiceLocator._WS_Player_Initializator.XPTable[(int)Level]);
                SetUpdateFlag(EPlayerFields.PLAYER_REST_STATE_EXPERIENCE, RestBonus);
                SetUpdateFlag(EPlayerFields.PLAYER_FLAGS, cPlayerFlags);
                SetUpdateFlag(EPlayerFields.PLAYER_FIELD_BYTES, cPlayerFieldBytes);
                SetUpdateFlag(EPlayerFields.PLAYER_FIELD_BYTES2, cPlayerFieldBytes2);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_BOUNDINGRADIUS, BoundingRadius);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_COMBATREACH, CombatReach);
                SetUpdateFlag(EPlayerFields.PLAYER_CHARACTER_POINTS1, TalentPoints);
                // SetUpdateFlag(EPlayerFields.PLAYER_CHARACTER_POINTS2, 0)

                SetUpdateFlag(EPlayerFields.PLAYER_GUILDID, GuildID);
                SetUpdateFlag(EPlayerFields.PLAYER_GUILDRANK, GuildRank);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_MINDAMAGE, Damage.Minimum);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXDAMAGE, Damage.Maximum + (float)BaseUnarmedDamage);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_BASEATTACKTIME, AttackTime(WeaponAttackType.BASE_ATTACK));
                SetUpdateFlag(EUnitFields.UNIT_FIELD_BASEATTACKTIME + 1, AttackTime(WeaponAttackType.OFF_ATTACK));
                SetUpdateFlag(EUnitFields.UNIT_MOD_CAST_SPEED, 1.0f);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_ATTACK_POWER, AttackPower);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_RANGED_ATTACK_POWER, AttackPowerRanged);
                WS_Base.BaseUnit argobjCharacter = this;
                SetUpdateFlag(EPlayerFields.PLAYER_CRIT_PERCENTAGE, WorldServiceLocator._WS_Combat.GetBasePercentCrit(ref argobjCharacter, 0));
                WS_Base.BaseUnit argobjCharacter1 = this;
                SetUpdateFlag(EPlayerFields.PLAYER_RANGED_CRIT_PERCENTAGE, WorldServiceLocator._WS_Combat.GetBasePercentCrit(ref argobjCharacter1, 0));
                // SetUpdateFlag(EPlayerFields.PLAYER_FIELD_MOD_HEALING_DONE_POS, healing.PositiveBonus)

                for (byte i = 0; i <= 6; i++)
                {
                    // SetUpdateFlag(EPlayerFields.PLAYER_SPELL_CRIT_PERCENTAGE1 + i, CType(0, Single))
                    SetUpdateFlag(EPlayerFields.PLAYER_FIELD_MOD_DAMAGE_DONE_POS + i, spellDamage[(int)i].PositiveBonus);
                    SetUpdateFlag(EPlayerFields.PLAYER_FIELD_MOD_DAMAGE_DONE_NEG + i, spellDamage[(int)i].NegativeBonus);
                    SetUpdateFlag(EPlayerFields.PLAYER_FIELD_MOD_DAMAGE_DONE_PCT + i, spellDamage[(int)i].Modifier);
                }

                SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_PHYSICAL, Resistances[DamageTypes.DMG_PHYSICAL].Base);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_HOLY, Resistances[DamageTypes.DMG_HOLY].Base);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_FIRE, Resistances[DamageTypes.DMG_FIRE].Base);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_NATURE, Resistances[DamageTypes.DMG_NATURE].Base);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_FROST, Resistances[DamageTypes.DMG_FROST].Base);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_SHADOW, Resistances[DamageTypes.DMG_SHADOW].Base);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_ARCANE, Resistances[DamageTypes.DMG_ARCANE].Base);

                // SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + DamageTypes.DMG_PHYSICAL, Resistances(DamageTypes.DMG_PHYSICAL).PositiveBonus)
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + DamageTypes.DMG_HOLY, Resistances(DamageTypes.DMG_HOLY).PositiveBonus)
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + DamageTypes.DMG_FIRE, Resistances(DamageTypes.DMG_FIRE).PositiveBonus)
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + DamageTypes.DMG_NATURE, Resistances(DamageTypes.DMG_NATURE).PositiveBonus)
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + DamageTypes.DMG_FROST, Resistances(DamageTypes.DMG_FROST).PositiveBonus)
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + DamageTypes.DMG_SHADOW, Resistances(DamageTypes.DMG_SHADOW).PositiveBonus)
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + DamageTypes.DMG_ARCANE, Resistances(DamageTypes.DMG_ARCANE).PositiveBonus)

                // SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + DamageTypes.DMG_PHYSICAL, Resistances(DamageTypes.DMG_PHYSICAL).NegativeBonus)
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + DamageTypes.DMG_HOLY, Resistances(DamageTypes.DMG_HOLY).NegativeBonus)
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + DamageTypes.DMG_FIRE, Resistances(DamageTypes.DMG_FIRE).NegativeBonus)
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + DamageTypes.DMG_NATURE, Resistances(DamageTypes.DMG_NATURE).NegativeBonus)
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + DamageTypes.DMG_FROST, Resistances(DamageTypes.DMG_FROST).NegativeBonus)
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + DamageTypes.DMG_SHADOW, Resistances(DamageTypes.DMG_SHADOW).NegativeBonus)
                // SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + DamageTypes.DMG_ARCANE, Resistances(DamageTypes.DMG_ARCANE).NegativeBonus)

                SetUpdateFlag(EPlayerFields.PLAYER_FIELD_COINAGE, Copper);
                foreach (KeyValuePair<int, WS_PlayerHelper.TSkill> Skill in Skills)
                {
                    SetUpdateFlag(EPlayerFields.PLAYER_SKILL_INFO_1_1 + (int)SkillsPositions[Skill.Key] * 3, Skill.Key);                                    // skill1.Id
                    SetUpdateFlag(EPlayerFields.PLAYER_SKILL_INFO_1_1 + (int)SkillsPositions[Skill.Key] * 3 + 1, Skill.Value.GetSkill);      // CType((skill1.CurrentVal(Me) + (skill1.Cap(Me) << 16)), Integer)
                    SetUpdateFlag(EPlayerFields.PLAYER_SKILL_INFO_1_1 + (int)SkillsPositions[Skill.Key] * 3 + 2, Skill.Value.Bonus);         // skill1.Bonus
                }

                SetUpdateFlag(EUnitFields.UNIT_FIELD_RANGEDATTACKTIME, AttackTime(WeaponAttackType.RANGED_ATTACK));
                SetUpdateFlag(EUnitFields.UNIT_FIELD_MINOFFHANDDAMAGE, OffHandDamage.Minimum);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXOFFHANDDAMAGE, OffHandDamage.Maximum);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_STRENGTH, Strength.Base);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_AGILITY, Agility.Base);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_STAMINA, Stamina.Base);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_SPIRIT, Spirit.Base);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_INTELLECT, Intellect.Base);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_ATTACK_POWER_MODS, AttackPowerMods);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_RANGED_ATTACK_POWER_MODS, AttackPowerModsRanged);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_MINRANGEDDAMAGE, RangedDamage.Minimum);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXRANGEDDAMAGE, RangedDamage.Maximum + (float)BaseRangedDamage);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_ATTACK_POWER_MULTIPLIER, 0.0f);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_RANGED_ATTACK_POWER_MULTIPLIER, 0.0f);
                for (byte i = 0, loopTo = QuestInfo.QUEST_SLOTS; i <= loopTo; i++)
                {
                    if (TalkQuests[i] is null)
                    {
                        SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_1 + (int)i * 3, 0); // ID
                        SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_2 + (int)i * 3, 0); // State
                        SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_2 + (int)i * 3 + 1, 0); // Timer
                    }
                    else
                    {
                        SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_1 + (int)i * 3, TalkQuests[(int)i].ID);
                        SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_2 + (int)i * 3, TalkQuests[(int)i].GetProgress());
                        SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_2 + (int)i * 3 + 1, 0);
                    } // Timer
                }

                WS_Base.BaseUnit argobjCharacter2 = this;
                SetUpdateFlag(EPlayerFields.PLAYER_BLOCK_PERCENTAGE, WorldServiceLocator._WS_Combat.GetBasePercentBlock(ref argobjCharacter2, 0));
                WS_Base.BaseUnit argobjCharacter3 = this;
                SetUpdateFlag(EPlayerFields.PLAYER_DODGE_PERCENTAGE, WorldServiceLocator._WS_Combat.GetBasePercentDodge(ref argobjCharacter3, 0));
                WS_Base.BaseUnit argobjCharacter4 = this;
                SetUpdateFlag(EPlayerFields.PLAYER_PARRY_PERCENTAGE, WorldServiceLocator._WS_Combat.GetBasePercentParry(ref argobjCharacter4, 0));
                for (byte i = 0, loopTo1 = WorldServiceLocator._Global_Constants.PLAYER_EXPLORED_ZONES_SIZE; i <= loopTo1; i++)
                    SetUpdateFlag(EPlayerFields.PLAYER_EXPLORED_ZONES_1 + i, ZonesExplored[(int)i]);

                // SetUpdateFlag(EPlayerFields.PLAYER_FIELD_PVP_MEDALS, 0)
                SetUpdateFlag(EPlayerFields.PLAYER_FIELD_LIFETIME_HONORBALE_KILLS, HonorKillsLifeTime);
                SetUpdateFlag(EPlayerFields.PLAYER_FIELD_LIFETIME_DISHONORBALE_KILLS, DishonorKillsLifeTime);
                SetUpdateFlag(EPlayerFields.PLAYER_FIELD_SESSION_KILLS, (int)HonorKillsToday + ((int)DishonorKillsToday << 16));
                SetUpdateFlag(EPlayerFields.PLAYER_FIELD_THIS_WEEK_KILLS, HonorKillsThisWeek);
                SetUpdateFlag(EPlayerFields.PLAYER_FIELD_LAST_WEEK_KILLS, HonorKillsLastWeek);
                SetUpdateFlag(EPlayerFields.PLAYER_FIELD_YESTERDAY_KILLS, HonorKillsYesterday);
                SetUpdateFlag(EPlayerFields.PLAYER_FIELD_THIS_WEEK_CONTRIBUTION, HonorPointsThisWeek);
                SetUpdateFlag(EPlayerFields.PLAYER_FIELD_LAST_WEEK_CONTRIBUTION, HonorPointsLastWeek);
                SetUpdateFlag(EPlayerFields.PLAYER_FIELD_YESTERDAY_CONTRIBUTION, HonorPointsYesterday);
                SetUpdateFlag(EPlayerFields.PLAYER_FIELD_LAST_WEEK_RANK, StandingLastWeek);
                for (byte i = EquipmentSlots.EQUIPMENT_SLOT_START, loopTo2 = KeyRingSlots.KEYRING_SLOT_END - 1; i <= loopTo2; i++)
                {
                    if (Items.ContainsKey(i))
                    {
                        if (i < EquipmentSlots.EQUIPMENT_SLOT_END)
                        {
                            SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_0 + i * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE, Items[i].ItemEntry);

                            // DONE: Include enchantment info
                            foreach (KeyValuePair<byte, WS_Items.TEnchantmentInfo> Enchant in Items[i].Enchantments)
                            {
                                SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_0 + 1 + (int)Enchant.Key * 3 + i * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE, Enchant.Value.ID);
                                SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_0 + 2 + (int)Enchant.Key * 3 + i * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE, Enchant.Value.Charges); // Correct?
                                SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_0 + 3 + (int)Enchant.Key * 3 + i * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE, Enchant.Value.Duration); // Correct?
                            }

                            SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_PROPERTIES + i * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE, Items[i].RandomProperties);
                        }

                        SetUpdateFlag(EPlayerFields.PLAYER_FIELD_INV_SLOT_HEAD + (int)i * 2, Items[i].GUID);
                    }
                    else
                    {
                        if (i < EquipmentSlots.EQUIPMENT_SLOT_END)
                        {
                            SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_0 + i * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE, 0);
                            SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_PROPERTIES + i * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE, 0);
                        }

                        SetUpdateFlag(EPlayerFields.PLAYER_FIELD_INV_SLOT_HEAD + (int)i * 2, 0);
                    }
                }

                SetUpdateFlag(EPlayerFields.PLAYER_AMMO_ID, AmmoID);
                for (byte i = 0, loopTo3 = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1; i <= loopTo3; i++)
                {
                    if (ActiveSpells[i] is object)
                    {
                        SetUpdateFlag(EUnitFields.UNIT_FIELD_AURA + i, ActiveSpells[(int)i].SpellID);
                    }
                }

                for (byte i = 0, loopTo4 = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECT_FLAGs - 1; i <= loopTo4; i++)
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_AURAFLAGS + i, ActiveSpells_Flags[(int)i]);
                for (byte i = 0, loopTo5 = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECT_LEVELSs - 1; i <= loopTo5; i++)
                {
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_AURAAPPLICATIONS + i, ActiveSpells_Count[(int)i]);
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_AURALEVELS + i, ActiveSpells_Level[(int)i]);
                }
            }                                       // Used for this player's update packets

            public void FillAllUpdateFlags(ref Packets.UpdateClass Update)
            {
                Update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_GUID, GUID);
                Update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_SCALE_X, Size);
                Update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_TYPE, 25);
                if (Pet is object)
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_SUMMON, Pet.GUID);
                Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_HEALTH, Life.Current);
                Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER1, Mana.Current);
                Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER2, Rage.Current);
                Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER4, Energy.Current);
                Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER5, 0);
                Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXHEALTH, Life.Maximum);
                Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXPOWER1, Mana.Maximum);
                Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXPOWER2, Rage.Maximum);
                Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXPOWER4, Energy.Maximum);
                Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXPOWER5, 0);
                Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, cUnitFlags);
                Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_LEVEL, Level);
                Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_FACTIONTEMPLATE, Faction);
                Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_BYTES_0, cBytes0);
                Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_BYTES_1, cBytes1);
                Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_BYTES_2, cBytes2);
                Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_DISPLAYID, Model);
                Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_NATIVEDISPLAYID, Model_Native);
                Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_MOUNTDISPLAYID, Mount);
                Update.SetUpdateFlag(EUnitFields.UNIT_DYNAMIC_FLAGS, cDynamicFlags);
                Update.SetUpdateFlag(EPlayerFields.PLAYER_BYTES, cPlayerBytes);
                Update.SetUpdateFlag(EPlayerFields.PLAYER_BYTES_2, cPlayerBytes2);
                Update.SetUpdateFlag(EPlayerFields.PLAYER_BYTES_3, cPlayerBytes3);
                Update.SetUpdateFlag(EPlayerFields.PLAYER_FLAGS, cPlayerFlags);
                Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_BOUNDINGRADIUS, BoundingRadius);
                Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_COMBATREACH, CombatReach);
                Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_TARGET, TargetGUID);
                Update.SetUpdateFlag(EPlayerFields.PLAYER_GUILDID, GuildID);
                Update.SetUpdateFlag(EPlayerFields.PLAYER_GUILDRANK, GuildRank);
                for (byte i = EquipmentSlots.EQUIPMENT_SLOT_START, loopTo = EquipmentSlots.EQUIPMENT_SLOT_END - 1; i <= loopTo; i++)
                {
                    if (Items.ContainsKey(i))
                    {
                        if (i < EquipmentSlots.EQUIPMENT_SLOT_END)
                        {
                            Update.SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_0 + i * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE, Items[i].ItemEntry);

                            // DONE: Include enchantment info
                            // For Each Enchant As KeyValuePair(Of Byte, TEnchantmentInfo) In Items(i).Enchantments
                            // Update.SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_0_1 + Enchant.Key + i * _Global_Constants.PLAYER_VISIBLE_ITEM_SIZE, Enchant.Value.ID)
                            // Next
                            Update.SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_PROPERTIES + i * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE, Items[i].RandomProperties);
                        }

                        Update.SetUpdateFlag(EPlayerFields.PLAYER_FIELD_INV_SLOT_HEAD + (int)i * 2, Items[i].GUID);
                    }
                    else
                    {
                        if (i < EquipmentSlots.EQUIPMENT_SLOT_END)
                        {
                            Update.SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_0 + i * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE, 0);
                            Update.SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_PROPERTIES + i * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE, 0);
                        }

                        Update.SetUpdateFlag(EPlayerFields.PLAYER_FIELD_INV_SLOT_HEAD + (int)i * 2, 0);
                    }
                }

                for (byte i = 0, loopTo1 = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1; i <= loopTo1; i++)
                {
                    if (ActiveSpells[i] is object)
                    {
                        Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_AURA + i, ActiveSpells[(int)i].SpellID);
                    }
                }

                for (byte i = 0, loopTo2 = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECT_FLAGs - 1; i <= loopTo2; i++)
                    Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_AURAFLAGS + i, ActiveSpells_Flags[(int)i]);
                for (byte i = 0, loopTo3 = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECT_LEVELSs - 1; i <= loopTo3; i++)
                {
                    Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_AURAAPPLICATIONS + i, ActiveSpells_Count[(int)i]);
                    Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_AURALEVELS + i, ActiveSpells_Level[(int)i]);
                }
            }                                       // Used for other players' update packets

            public void PrepareUpdate(ref Packets.PacketClass packet, int UPDATETYPE = ObjectUpdateType.UPDATETYPE_CREATE_OBJECT)
            {
                packet.AddInt8((byte)UPDATETYPE);
                packet.AddPackGUID(GUID);
                if (UPDATETYPE == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT | UPDATETYPE == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF)
                {
                    packet.AddInt8(ObjectTypeID.TYPEID_PLAYER);
                }

                if (UPDATETYPE == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT | UPDATETYPE == ObjectUpdateType.UPDATETYPE_MOVEMENT | UPDATETYPE == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF)
                {
                    int flags2 = 0x2000;
                    if (OnTransport is object)
                    {
                        flags2 = flags2 | MovementFlags.MOVEMENTFLAG_ONTRANSPORT;
                    }

                    packet.AddInt8(0x71); // flags
                    packet.AddInt32(flags2); // flags2
                    packet.AddInt32(WorldServiceLocator._WS_Network.MsTime());
                    packet.AddSingle(positionX);
                    packet.AddSingle(positionY);
                    packet.AddSingle(positionZ);
                    packet.AddSingle(orientation);
                    if (flags2 & MovementFlags.MOVEMENTFLAG_ONTRANSPORT)
                    {
                        packet.AddUInt64(OnTransport.GUID);
                        packet.AddSingle(transportX);
                        packet.AddSingle(transportY);
                        packet.AddSingle(transportZ);
                        packet.AddSingle(orientation);
                    }

                    packet.AddInt32(0); // Unk
                    packet.AddInt32(0); // Unk
                    packet.AddInt32(0); // AttackCycle?
                    packet.AddInt32(0); // TimeID?
                    packet.AddInt32(0); // VictimGUID?
                    packet.AddSingle(WalkSpeed);
                    packet.AddSingle(RunSpeed);
                    packet.AddSingle(RunBackSpeed);
                    packet.AddSingle(SwimSpeed);
                    packet.AddSingle(SwimBackSpeed);
                    packet.AddSingle(TurnRate);
                    packet.AddUInt32(0x2FU);
                }

                if (UPDATETYPE == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT | UPDATETYPE == ObjectUpdateType.UPDATETYPE_VALUES | UPDATETYPE == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF)
                {
                    int UpdateCount = 0;
                    for (int i = 0, loopTo = UpdateMask.Count - 1; i <= loopTo; i++)
                    {
                        if (UpdateMask.Get(i))
                            UpdateCount = i;
                    }

                    packet.AddInt8((byte)((UpdateCount + 32) / 32));
                    packet.AddBitArray(UpdateMask, Conversions.ToByte((UpdateCount + 32) / 32) * 4);      // OK Flags are Int32, so to byte -> *4
                    for (int i = 0, loopTo1 = UpdateMask.Count - 1; i <= loopTo1; i++)
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

                    UpdateMask.SetAll(false);
                }
            }

            // Packets and Events
            public bool AFK
            {
                get
                {
                    return cPlayerFlags & PlayerFlags.PLAYER_FLAGS_AFK;
                }

                set
                {
                    if (value)
                    {
                        cPlayerFlags = cPlayerFlags | PlayerFlags.PLAYER_FLAGS_AFK;
                        WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.ClientSetChatFlag(client.Index, ChatFlag.FLAGS_AFK);
                    }
                    else
                    {
                        cPlayerFlags = cPlayerFlags & !PlayerFlags.PLAYER_FLAGS_AFK;
                        WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.ClientSetChatFlag(client.Index, 0);
                    }
                }
            }

            public bool DND
            {
                get
                {
                    return cPlayerFlags & PlayerFlags.PLAYER_FLAGS_DND;
                }

                set
                {
                    if (value)
                    {
                        cPlayerFlags = cPlayerFlags | PlayerFlags.PLAYER_FLAGS_DND;
                        WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.ClientSetChatFlag(client.Index, ChatFlag.FLAGS_DND);
                    }
                    else
                    {
                        cPlayerFlags = cPlayerFlags & !PlayerFlags.PLAYER_FLAGS_DND;
                        WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.ClientSetChatFlag(client.Index, 0);
                    }
                }
            }

            public bool GM
            {
                get
                {
                    return cPlayerFlags & PlayerFlags.PLAYER_FLAGS_GM;
                }

                set
                {
                    if (value)
                    {
                        cPlayerFlags = cPlayerFlags | PlayerFlags.PLAYER_FLAGS_GM;
                        WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.ClientSetChatFlag(client.Index, ChatFlag.FLAGS_GM);
                    }
                    else
                    {
                        cPlayerFlags = cPlayerFlags & !PlayerFlags.PLAYER_FLAGS_GM;
                        WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.ClientSetChatFlag(client.Index, 0);
                    }
                }
            }

            // Chat
            public void SendChatMessage(ref CharacterObject Sender, string Message, ChatMsg msgType, int msgLanguage, string ChannelName = "Global", bool SendToMe = false)
            {
                var packet = WorldServiceLocator._Functions.BuildChatMessage(Sender.GUID, Message, msgType, msgLanguage, WorldServiceLocator._WS_Handlers_Chat.GetChatFlag(Sender), ChannelName);
                SendToNearPlayers(ref packet, ToSelf: SendToMe);
                packet.Dispose();
            }

            public void CommandResponse(string Message)
            {
                var Messages = Message.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                if (Messages.Length == 0)
                {
                    Messages = new string[1];
                    Messages[0] = Message;
                }

                foreach (string Msg in Messages)
                {
                    var packet = WorldServiceLocator._Functions.BuildChatMessage(WS_Commands.SystemGUID, Msg, ChatMsg.CHAT_MSG_SYSTEM, LANGUAGES.LANG_UNIVERSAL);
                    client.Send(ref packet);
                    packet.Dispose();
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_MESSAGECHAT", client.IP, client.Port);
            }

            public void SystemMessage(string Message)
            {
                WorldServiceLocator._Functions.SendMessageSystem(client, Message);
            }

            // Spell/Skill/_WS_DBCDatabase.Talents System
            public byte TalentPoints = 0;
            public int AmmoID = 0;
            public float AmmoDPS = 0.0f;
            public float AmmoMod = 1.0f;
            public int AutoShotSpell = 0;
            public WS_Creatures.CreatureObject NonCombatPet = null;
            public ulong[] TotemSlot = new ulong[4];
            public Dictionary<int, WS_PlayerHelper.TSkill> Skills = new Dictionary<int, WS_PlayerHelper.TSkill>();
            public Dictionary<int, short> SkillsPositions = new Dictionary<int, short>();
            public Dictionary<int, WS_Spells.CharacterSpell> Spells = new Dictionary<int, WS_Spells.CharacterSpell>();
            public WS_Base.BaseUnit MindControl = null;

            public void CastOnSelf(int SpellID)
            {
                if (WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(SpellID) == false)
                    return;
                var Targets = new WS_Spells.SpellTargets();
                WS_Base.BaseUnit argobjCharacter = this;
                Targets.SetTarget_UNIT(ref argobjCharacter);
                WS_Base.BaseObject argCaster = this;
                var castParams = new WS_Spells.CastSpellParameters(ref Targets, ref argCaster, SpellID);
                castParams.Cast(null);
            }

            public void ApplySpell(int SpellID)
            {
                if (WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(SpellID) == false)
                    return;
                var t = new WS_Spells.SpellTargets();
                WS_Base.BaseUnit argobjCharacter = this;
                t.SetTarget_SELF(ref argobjCharacter);
                var argCharacter = this;
                if (WorldServiceLocator._WS_Spells.SPELLs[SpellID].CanCast(ref argCharacter, t, false) == SpellFailedReason.SPELL_NO_ERROR)
                {
                    WS_Base.BaseObject argcaster = this;
                    WorldServiceLocator._WS_Spells.SPELLs[SpellID].Apply(ref argcaster, t);
                }
            }

            public void ProhibitSpellSchool(int School, int Time)
            {
                var packet = new Packets.PacketClass(OPCODES.SMSG_SPELL_COOLDOWN);
                try
                {
                    packet.AddInt32((int)GUID);
                    uint curTime = WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now);
                    foreach (KeyValuePair<int, WS_Spells.CharacterSpell> Spell in Spells)
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

            public bool FinishAllSpells(bool OK = false)
            {
                bool result1 = FinishSpell(CurrentSpellTypes.CURRENT_AUTOREPEAT_SPELL, OK);
                bool result2 = FinishSpell(CurrentSpellTypes.CURRENT_CHANNELED_SPELL, OK);
                bool result3 = FinishSpell(CurrentSpellTypes.CURRENT_GENERIC_SPELL, OK);
                return result1 || result2 || result3;
            }

            public bool FinishSpell(CurrentSpellTypes SpellType, bool OK = false)
            {
                if (SpellType == CurrentSpellTypes.CURRENT_CHANNELED_SPELL)
                {
                    var argCaster = this;
                    WorldServiceLocator._WS_Spells.SendChannelUpdate(ref argCaster, 0);
                }

                if (client.Character.spellCasted[SpellType] is null)
                    return false;
                if (client.Character.spellCasted[SpellType].Finished)
                    return false;
                client.Character.spellCasted[SpellType].State = SpellCastState.SPELL_STATE_IDLE;
                client.Character.spellCasted[SpellType].Stopped = true;
                if (SpellType == CurrentSpellTypes.CURRENT_AUTOREPEAT_SPELL)
                {
                    client.Character.AutoShotSpell = 0;
                    client.Character.attackState.AttackStop();
                }
                else
                {
                    int SpellID = spellCasted[SpellType].SpellID;
                    WS_Base.BaseUnit argCaster1 = client.Character;
                    WorldServiceLocator._WS_Spells.SPELLs[SpellID].SendInterrupted(0, ref argCaster1);
                    if (!OK)
                    {
                        WorldServiceLocator._WS_Spells.SendCastResult(SpellFailedReason.SPELL_FAILED_INTERRUPTED, ref client, SpellID);
                    }

                    client.Character.RemoveAuraBySpell(SpellID);

                    // DONE: Remove dynamic objects created with spell
                    var DynamicObjects = client.Character.dynamicObjects.ToArray();
                    foreach (WS_DynamicObjects.DynamicObjectObject tmpDO in DynamicObjects)
                    {
                        if (tmpDO.SpellID == SpellID)
                        {
                            tmpDO.Delete();
                            client.Character.dynamicObjects.Remove(tmpDO);
                            break;
                        }
                    }

                    // DONE: Remove game objects created with spell
                    var GameObjects = client.Character.gameObjects.ToArray();
                    foreach (WS_GameObjects.GameObjectObject tmpGO in GameObjects)
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
                if (Spells.ContainsKey(SpellID))
                    return;
                if (!WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(SpellID))
                    return;
                Spells.Add(SpellID, new WS_Spells.CharacterSpell(SpellID, 1, 0U, 0));

                // DONE: Save it to the database
                WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("INSERT INTO characters_spells (guid, spellid, active, cooldown, cooldownitem) VALUES ({0},{1},{2},{3},{4});", GUID, SpellID, 1, 0, 0));
                if (client is null)
                    return;
                var SMSG_LEARNED_SPELL = new Packets.PacketClass(OPCODES.SMSG_LEARNED_SPELL);
                try
                {
                    SMSG_LEARNED_SPELL.AddInt32(SpellID);
                    client.Send(ref SMSG_LEARNED_SPELL);
                }
                finally
                {
                    SMSG_LEARNED_SPELL.Dispose();
                }

                var t = new WS_Spells.SpellTargets();
                WS_Base.BaseUnit argobjCharacter = this;
                t.SetTarget_SELF(ref argobjCharacter);
                if (WorldServiceLocator._WS_Spells.SPELLs[SpellID].IsPassive)
                {
                    // DONE: Apply passive spell we don't have
                    var argCharacter = this;
                    if (HavePassiveAura(SpellID) == false && WorldServiceLocator._WS_Spells.SPELLs[SpellID].CanCast(ref argCharacter, t, false) == SpellFailedReason.SPELL_NO_ERROR)
                    {
                        WS_Base.BaseObject argcaster = this;
                        WorldServiceLocator._WS_Spells.SPELLs[SpellID].Apply(ref argcaster, t);
                    }
                }

                // DONE: Deactivate old ranks
                if (!WorldServiceLocator._WS_Spells.SPELLs[SpellID].CanStackSpellRank)
                {
                    if (WorldServiceLocator._WS_Spells.SpellChains.ContainsKey(SpellID))
                    {
                        if (Spells.ContainsKey(WorldServiceLocator._WS_Spells.SpellChains[SpellID]))
                        {
                            Spells[WorldServiceLocator._WS_Spells.SpellChains[SpellID]].Active = 0; // NOTE: Deactivate spell, don't remove it

                            // DONE: Save it to the database
                            WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_spells SET active = 0 WHERE guid = {0} AND spellid = {1};", GUID, SpellID));
                            var packet = new Packets.PacketClass(OPCODES.SMSG_SUPERCEDED_SPELL);
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
                    }
                }

                int maxSkill = Level > WorldServiceLocator._WS_Player_Initializator.DEFAULT_MAX_LEVEL ? WorldServiceLocator._WS_Player_Initializator.DEFAULT_MAX_LEVEL * 5 : Level * 5;
                switch (SpellID)
                {
                    case 4036: // SKILL_ENGINERING
                        {
                            LearnSpell(3918);
                            LearnSpell(3919);
                            LearnSpell(3920);
                            break;
                        }

                    case 3908: // SKILL_TAILORING
                        {
                            LearnSpell(2387);
                            LearnSpell(2963);
                            break;
                        }

                    case 7411: // SKILL_ENCHANTING
                        {
                            LearnSpell(7418);
                            LearnSpell(7421);
                            LearnSpell(13262);
                            break;
                        }

                    case 2259: // SKILL_ALCHEMY
                        {
                            LearnSpell(2329);
                            LearnSpell(7183);
                            LearnSpell(2330);
                            break;
                        }

                    case 2018: // SKILL_BLACKSMITHING
                        {
                            LearnSpell(2663);
                            LearnSpell(12260);
                            LearnSpell(2660);
                            LearnSpell(3115);
                            break;
                        }

                    case 2108: // SKILL_LEATHERWORKING
                        {
                            LearnSpell(2152);
                            LearnSpell(9058);
                            LearnSpell(9059);
                            LearnSpell(2149);
                            LearnSpell(7126);
                            LearnSpell(2881);
                            break;
                        }

                    case 2550: // SKILL_COOKING
                        {
                            LearnSpell(818);
                            LearnSpell(2540);
                            LearnSpell(2538);
                            break;
                        }

                    case 3273: // SKILL_FIRST_AID
                        {
                            LearnSpell(3275);
                            break;
                        }

                    case 7620: // SKILL_FISHING
                        {
                            LearnSpell(7738);
                            break;
                        }

                    case 2575: // SKILL_MINING
                        {
                            LearnSpell(2580);
                            LearnSpell(2656);
                            LearnSpell(2657);
                            break;
                        }

                    case 2366: // SKILL_HERBALISM
                        {
                            LearnSpell(2383);
                            break;
                        }

                    case 264: // WEAPON_BOWS
                        {
                            if (!HaveSpell(75))
                                LearnSpell(2480);
                            LearnSkill(SKILL_IDs.SKILL_BOWS, 1, maxSkill);
                            break;
                        }

                    case 266: // WEAPON_GUNS
                        {
                            if (!HaveSpell(75))
                                LearnSpell(2480);
                            LearnSkill(SKILL_IDs.SKILL_GUNS, 1, maxSkill);
                            break;
                        }

                    case 5011: // WEAPON_CROSSBOWS
                        {
                            if (!HaveSpell(75))
                                LearnSpell(7919);
                            LearnSkill(SKILL_IDs.SKILL_CROSSBOWS, 1, maxSkill);
                            break;
                        }

                    case 2567: // WEAPON_THROWN
                        {
                            LearnSpell(2764);
                            LearnSkill(SKILL_IDs.SKILL_THROWN, 1, maxSkill);
                            break;
                        }

                    case 5009: // WEAPON_WANDS
                        {
                            LearnSpell(5019);
                            LearnSkill(SKILL_IDs.SKILL_WANDS, 1, maxSkill);
                            break;
                        }

                    case 9078: // ARMOR_CLOTH
                        {
                            LearnSkill(SKILL_IDs.SKILL_CLOTH);
                            break;
                        }

                    case 9077: // ARMOR_LEATHER
                        {
                            LearnSkill(SKILL_IDs.SKILL_LEATHER);
                            break;
                        }

                    case 8737: // ARMOR_MAIL
                        {
                            LearnSkill(SKILL_IDs.SKILL_MAIL);
                            break;
                        }

                    case 750: // ARMOR_PLATE
                        {
                            LearnSkill(SKILL_IDs.SKILL_PLATE_MAIL);
                            break;
                        }

                    case 9116: // ARMOR_SHIELD
                        {
                            LearnSkill(SKILL_IDs.SKILL_SHIELD);
                            break;
                        }

                    case 674: // DUAL_WIELD
                        {
                            LearnSkill(SKILL_IDs.SKILL_DUAL_WIELD);
                            break;
                        }

                    case 196: // WEAPON_AXES
                        {
                            LearnSkill(SKILL_IDs.SKILL_AXES, 1, maxSkill);
                            break;
                        }

                    case 197: // WEAPON_TWOHAND_AXES
                        {
                            LearnSkill(SKILL_IDs.SKILL_TWO_HANDED_AXES, 1, maxSkill);
                            break;
                        }

                    case 227: // WEAPON_STAVES
                        {
                            LearnSkill(SKILL_IDs.SKILL_STAVES, 1, maxSkill);
                            break;
                        }

                    case 198: // WEAPON_MACES
                        {
                            LearnSkill(SKILL_IDs.SKILL_MACES, 1, maxSkill);
                            break;
                        }

                    case 199: // WEAPON_TWOHAND_MACES
                        {
                            LearnSkill(SKILL_IDs.SKILL_TWO_HANDED_MACES, 1, maxSkill);
                            break;
                        }

                    case 201: // WEAPON_SWORDS
                        {
                            LearnSkill(SKILL_IDs.SKILL_SWORDS, 1, maxSkill);
                            break;
                        }

                    case 202: // WEAPON_TWOHAND_SWORDS
                        {
                            LearnSkill(SKILL_IDs.SKILL_TWO_HANDED_SWORDS, 1, maxSkill);
                            break;
                        }

                    case 1180: // WEAPON_DAGGERS
                        {
                            LearnSkill(SKILL_IDs.SKILL_DAGGERS, 1, maxSkill);
                            break;
                        }

                    case 15590: // WEAPON_FIST_WEAPONS
                        {
                            LearnSkill(SKILL_IDs.SKILL_FIST_WEAPONS, 1, maxSkill);
                            break;
                        }

                    case 200: // WEAPON_POLEARMS
                        {
                            LearnSkill(SKILL_IDs.SKILL_POLEARMS, 1, maxSkill);
                            break;
                        }

                    case 3386: // WEAPON_SPEARS
                        {
                            LearnSkill(SKILL_IDs.SKILL_SPEARS, 1, maxSkill);
                            break;
                        }

                    case 2842: // OTHER_POISONS
                        {
                            LearnSkill(SKILL_IDs.SKILL_POISONS, 1, maxSkill);
                            break;
                        }

                    case 668: // LANGUAGE_COMMON
                        {
                            LearnSkill(SKILL_IDs.SKILL_LANGUAGE_COMMON, 300, 300);
                            break;
                        }

                    case 669: // LANGUAGE_ORCISH
                        {
                            LearnSkill(SKILL_IDs.SKILL_LANGUAGE_ORCISH, 300, 300);
                            break;
                        }

                    case 670: // LANGUAGE_TAURAHE
                        {
                            LearnSkill(SKILL_IDs.SKILL_LANGUAGE_TAURAHE, 300, 300);
                            break;
                        }

                    case 671: // LANGUAGE_DARNASSIAN
                        {
                            LearnSkill(SKILL_IDs.SKILL_LANGUAGE_DARNASSIAN, 300, 300);
                            break;
                        }

                    case 672: // LANGUAGE_DWARVEN
                        {
                            LearnSkill(SKILL_IDs.SKILL_LANGUAGE_DWARVEN, 300, 300);
                            break;
                        }

                    case 813: // LANGUAGE_THALASSIAN
                        {
                            LearnSkill(SKILL_IDs.SKILL_LANGUAGE_THALASSIAN, 300, 300);
                            break;
                        }

                    case 814: // LANGUAGE_DRACONIC
                        {
                            LearnSkill(SKILL_IDs.SKILL_LANGUAGE_DRACONIC, 300, 300);
                            break;
                        }

                    case 815: // LANGUAGE_DEMON_TONGUE
                        {
                            LearnSkill(SKILL_IDs.SKILL_LANGUAGE_DEMON_TONGUE, 300, 300);
                            break;
                        }

                    case 816: // LANGUAGE_TITAN
                        {
                            LearnSkill(SKILL_IDs.SKILL_LANGUAGE_TITAN, 300, 300);
                            break;
                        }

                    case 817: // LANGUAGE_OLD_TONGUE
                        {
                            LearnSkill(SKILL_IDs.SKILL_LANGUAGE_OLD_TONGUE, 300, 300);
                            break;
                        }

                    case 7340: // LANGUAGE_GNOMISH
                        {
                            LearnSkill(SKILL_IDs.SKILL_LANGUAGE_GNOMISH, 300, 300);
                            break;
                        }

                    case 7341: // LANGUAGE_TROLL
                        {
                            LearnSkill(SKILL_IDs.SKILL_LANGUAGE_TROLL, 300, 300);
                            break;
                        }

                    case 17737: // LANGUAGE_GUTTERSPEAK
                        {
                            LearnSkill(SKILL_IDs.SKILL_LANGUAGE_GUTTERSPEAK, 300, 300);
                            break;
                        }
                }
            }

            public void UnLearnSpell(int SpellID)
            {
                if (!Spells.ContainsKey(SpellID))
                    return;
                Spells.Remove(SpellID);

                // DONE: Save it to the database
                WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("DELETE FROM characters_spells WHERE guid = {0} AND spellid = {1};", GUID, SpellID));
                var SMSG_REMOVED_SPELL = new Packets.PacketClass(OPCODES.SMSG_REMOVED_SPELL);
                try
                {
                    SMSG_REMOVED_SPELL.AddInt32(SpellID);
                    client.Send(ref SMSG_REMOVED_SPELL);
                }
                finally
                {
                    SMSG_REMOVED_SPELL.Dispose();
                }

                // DONE: Remove Aura by this spell
                client.Character.RemoveAuraBySpell(SpellID);
            }

            public bool HaveSpell(int SpellID)
            {
                return Spells.ContainsKey(SpellID);
            }

            public void LearnSkill(int SkillID, short Current = 1, short Maximum = 1)
            {
                if (Skills.ContainsKey(SkillID))
                {

                    // DONE: Already know this skill, just increase value
                    Skills[SkillID].Base = Maximum;
                    if (Current != 1)
                        Skills[SkillID].Current = Current;
                }
                else
                {

                    // DONE: Learn this skill as new
                    // TODO: Needs to be tidied up
                    short i;
                    var loopTo = WorldServiceLocator._Global_Constants.PLAYER_SKILL_INFO_SIZE;
                    for (i = 0; i <= loopTo; i++)
                    {
                        if (!SkillsPositions.ContainsValue(i))
                        {
                            break;
                        }
                    }

                    if (i > WorldServiceLocator._Global_Constants.PLAYER_SKILL_INFO_SIZE)
                        return;
                    SkillsPositions.Add(SkillID, i);
                    Skills.Add(SkillID, new WS_PlayerHelper.TSkill(Current, Maximum));
                }

                if (client is null)
                    return;

                // DONE: Set update parameters
                SetUpdateFlag(EPlayerFields.PLAYER_SKILL_INFO_1_1 + (int)SkillsPositions[SkillID] * 3, SkillID);                            // skill1.Id
                SetUpdateFlag(EPlayerFields.PLAYER_SKILL_INFO_1_1 + (int)SkillsPositions[SkillID] * 3 + 1, Skills[SkillID].GetSkill);       // CType((skill1.CurrentVal(Me) + (skill1.Cap(Me) << 16)), Integer)
                SendCharacterUpdate();
            }

            public bool HaveSkill(int SkillID, int SkillValue = 0)
            {
                if (Skills.ContainsKey(SkillID))
                {
                    return Skills[SkillID].Current >= SkillValue;
                }
                else
                {
                    return false;
                }
            }

            public void UpdateSkill(int SkillID, float SpeedMod = 0f)
            {
                if (SkillID == 0)
                    return;
                if (Skills[SkillID].Current >= Skills[SkillID].Maximum)
                    return;
                if (Skills[SkillID].Current / (double)Skills[SkillID].Maximum - SpeedMod < WorldServiceLocator._WorldServer.Rnd.NextDouble())
                {
                    Skills[SkillID].Increment();
                    SetUpdateFlag(EPlayerFields.PLAYER_SKILL_INFO_1_1 + (int)SkillsPositions[SkillID] * 3 + 1, Skills[SkillID].GetSkill);
                    SendCharacterUpdate();
                }
            }

            // XP and Level Managment
            public int RestBonus = 0;
            public int XP = 0;

            public void SetLevel(byte SetToLevel)
            {
                int TotalXp = 0;
                // TODO: If it's a level decrease, decrease stats etc instead of increasing them
                if (Level > SetToLevel)
                    return;
                for (short i = Level, loopTo = (short)(SetToLevel - 1); i <= loopTo; i++)
                    TotalXp += WorldServiceLocator._WS_Player_Initializator.XPTable[i];
                AddXP(TotalXp, 0, 0UL, false);
            }

            public void AddXP(int Ammount, int RestedBonus, ulong VictimGUID = 0UL, bool LogIt = true)
            {
                if (Ammount <= 0)
                    return;
                if (Level < WorldServiceLocator._WS_Player_Initializator.DEFAULT_MAX_LEVEL)
                {
                    XP += Ammount;
                    if (LogIt)
                        LogXPGain(Ammount, RestedBonus, VictimGUID, 1.0f);

                    // Update rested state if needed
                    if (RestedBonus > 0)
                    {
                        if (RestBonus <= 0)
                        {
                            RestBonus = 0;
                            RestState = XPSTATE.Normal;
                        }

                        SetUpdateFlag(EPlayerFields.PLAYER_BYTES_2, cPlayerBytes2);
                        SetUpdateFlag(EPlayerFields.PLAYER_REST_STATE_EXPERIENCE, RestBonus);
                    }

                CheckXPAgain:
                    ;
                    if (XP >= WorldServiceLocator._WS_Player_Initializator.XPTable[Level])
                    {
                        XP -= WorldServiceLocator._WS_Player_Initializator.XPTable[Level];
                        Level = (byte)(Level + 1);
                        GroupUpdateFlag = GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_LEVEL;

                        // DONE: Send update to cluster
                        WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.ClientUpdate(client.Index, ZoneID, Level);
                        int oldLife = Life.Maximum;
                        int oldMana = Mana.Maximum;
                        int oldStrength = Strength.Base;
                        int oldAgility = Agility.Base;
                        int oldStamina = Stamina.Base;
                        int oldIntellect = Intellect.Base;
                        int oldSpirit = Spirit.Base;
                        var argobjCharacter = this;
                        WorldServiceLocator._WS_Player_Initializator.CalculateOnLevelUP(ref argobjCharacter);
                        var SMSG_LEVELUP_INFO = new Packets.PacketClass(OPCODES.SMSG_LEVELUP_INFO);
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
                            if (client is object)
                                client.Send(ref SMSG_LEVELUP_INFO);
                        }
                        finally
                        {
                            SMSG_LEVELUP_INFO.Dispose();
                        }

                        Life.Current = Life.Maximum;
                        Mana.Current = Mana.Maximum;
                        Resistances[DamageTypes.DMG_PHYSICAL].Base += (Agility.Base - oldAgility) * 2;
                        SetUpdateFlag(EPlayerFields.PLAYER_XP, XP);
                        SetUpdateFlag(EPlayerFields.PLAYER_NEXT_LEVEL_XP, WorldServiceLocator._WS_Player_Initializator.XPTable[(int)Level]);
                        SetUpdateFlag(EPlayerFields.PLAYER_CHARACTER_POINTS1, TalentPoints);
                        SetUpdateFlag(EUnitFields.UNIT_FIELD_LEVEL, Level);
                        SetUpdateFlag(EUnitFields.UNIT_FIELD_STRENGTH, Strength.Base);
                        SetUpdateFlag(EUnitFields.UNIT_FIELD_AGILITY, Agility.Base);
                        SetUpdateFlag(EUnitFields.UNIT_FIELD_STAMINA, Stamina.Base);
                        SetUpdateFlag(EUnitFields.UNIT_FIELD_SPIRIT, Spirit.Base);
                        SetUpdateFlag(EUnitFields.UNIT_FIELD_INTELLECT, Intellect.Base);
                        SetUpdateFlag(EUnitFields.UNIT_FIELD_HEALTH, Life.Current);
                        SetUpdateFlag(EUnitFields.UNIT_FIELD_BASE_HEALTH, Life.Base);
                        SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER1, Mana.Current);
                        SetUpdateFlag(EUnitFields.UNIT_FIELD_BASE_MANA, Mana.Base);
                        SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXHEALTH, Life.Maximum);
                        SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXPOWER1, Mana.Maximum);
                        SetUpdateFlag(EUnitFields.UNIT_FIELD_ATTACK_POWER, AttackPower);
                        SetUpdateFlag(EUnitFields.UNIT_FIELD_ATTACK_POWER_MODS, AttackPowerMods);
                        SetUpdateFlag(EUnitFields.UNIT_FIELD_RANGED_ATTACK_POWER, AttackPowerRanged);
                        SetUpdateFlag(EUnitFields.UNIT_FIELD_RANGED_ATTACK_POWER_MODS, AttackPowerModsRanged);
                        SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_PHYSICAL, Resistances[DamageTypes.DMG_PHYSICAL].Base);
                        SetUpdateFlag(EUnitFields.UNIT_FIELD_MINDAMAGE, Damage.Minimum);
                        SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXDAMAGE, Conversions.ToSingle((double)Damage.Maximum + (double)(AttackPower + AttackPowerMods) * 0.071428571428571425d));
                        SetUpdateFlag(EUnitFields.UNIT_FIELD_MINOFFHANDDAMAGE, OffHandDamage.Minimum);
                        SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXOFFHANDDAMAGE, OffHandDamage.Maximum);
                        SetUpdateFlag(EUnitFields.UNIT_FIELD_MINRANGEDDAMAGE, RangedDamage.Minimum);
                        SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXRANGEDDAMAGE, RangedDamage.Maximum + (float)BaseRangedDamage);
                        foreach (KeyValuePair<int, WS_PlayerHelper.TSkill> Skill in Skills)
                            SetUpdateFlag(EPlayerFields.PLAYER_SKILL_INFO_1_1 + (int)SkillsPositions[Skill.Key] * 3 + 1, Skill.Value.GetSkill);       // CType((skill1.CurrentVal(Me) + (skill1.Cap(Me) << 16)), Integer)
                        if (client is object)
                            UpdateManaRegen();
                    }
                    else if (client is object)
                        SetUpdateFlag(EPlayerFields.PLAYER_XP, XP);

                    // We just dinged more than one level
                    if (XP >= WorldServiceLocator._WS_Player_Initializator.XPTable[Level] && Level < WorldServiceLocator._WS_Player_Initializator.DEFAULT_MAX_LEVEL)
                        goto CheckXPAgain;

                    // Fix if we add very big number XP
                    if (XP > WorldServiceLocator._WS_Player_Initializator.XPTable[Level])
                        XP = WorldServiceLocator._WS_Player_Initializator.XPTable[Level];
                    if (client is object)
                        SendCharacterUpdate();
                    SaveCharacter();
                }
            }

            // Item Managment
            public Dictionary<byte, ItemObject> Items = new Dictionary<byte, ItemObject>();

            public void ItemADD(int ItemEntry, byte dstBag, byte dstSlot, int Count = 1)
            {
                var tmpItem = new ItemObject(ItemEntry, GUID);
                // DONE: Check for unique
                if (tmpItem.ItemInfo.Unique > 0 && ItemCOUNT(ItemEntry) > tmpItem.ItemInfo.Unique)
                {
                    tmpItem.Delete();
                    return;
                }
                // DONE: Check for max stacking
                if (Count > tmpItem.ItemInfo.Stackable)
                    Count = tmpItem.ItemInfo.Stackable;
                tmpItem.StackCount = Count;
                if (dstBag == 255 & dstSlot == 255)
                {
                    ItemADD_AutoSlot(ref tmpItem);
                }
                else
                {
                    ItemSETSLOT(ref tmpItem, dstBag, dstSlot);
                }

                if (dstBag == 0 & dstSlot < InventorySlots.INVENTORY_SLOT_BAG_END && client is object)
                    UpdateAddItemStats(ref tmpItem, dstSlot);
            }

            public void ItemREMOVE(byte srcBag, byte srcSlot, bool Destroy, bool Update)
            {
                if (srcBag == 0)
                {
                    if (srcSlot < InventorySlots.INVENTORY_SLOT_BAG_END)
                    {
                        if (srcSlot < EquipmentSlots.EQUIPMENT_SLOT_END)
                            SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_0 + srcSlot * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE, 0);
                        var tmp = Items;
                        var argItem = tmp[srcSlot];
                        UpdateRemoveItemStats(ref argItem, srcSlot);
                        tmp[srcSlot] = argItem;
                    }

                    SetUpdateFlag(EPlayerFields.PLAYER_FIELD_INV_SLOT_HEAD + (int)srcSlot * 2, 0);
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_inventory SET item_slot = {0}, item_bag = {1} WHERE item_guid = {2};", WorldServiceLocator._Global_Constants.ITEM_SLOT_NULL, WorldServiceLocator._Global_Constants.ITEM_BAG_NULL, Items[srcSlot].GUID - WorldServiceLocator._Global_Constants.GUID_ITEM));
                    if (Destroy)
                        Items[srcSlot].Delete();
                    Items.Remove(srcSlot);
                    if (Update)
                        SendCharacterUpdate();
                }
                else
                {
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_inventory SET item_slot = {0}, item_bag = {1} WHERE item_guid = {2};", WorldServiceLocator._Global_Constants.ITEM_SLOT_NULL, WorldServiceLocator._Global_Constants.ITEM_BAG_NULL, Items[srcBag].Items[srcSlot].GUID - WorldServiceLocator._Global_Constants.GUID_ITEM));
                    if (Destroy)
                        Items[srcBag].Items[srcSlot].Delete();
                    Items[srcBag].Items.Remove(srcSlot);
                    if (Update)
                        SendItemUpdate(Items[srcBag]);
                }
            }

            public void ItemREMOVE(ulong itemGuid, bool Destroy, bool Update)
            {
                // DONE: Search in inventory
                for (byte slot = EquipmentSlots.EQUIPMENT_SLOT_START, loopTo = KeyRingSlots.KEYRING_SLOT_END - 1; slot <= loopTo; slot++)
                {
                    if (Items.ContainsKey(slot))
                    {
                        if (Items[slot].GUID == itemGuid)
                        {
                            WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_inventory SET item_slot = {0}, item_bag = {1} WHERE item_guid = {2};", WorldServiceLocator._Global_Constants.ITEM_SLOT_NULL, WorldServiceLocator._Global_Constants.ITEM_BAG_NULL, Items[slot].GUID - WorldServiceLocator._Global_Constants.GUID_ITEM));
                            if (slot < InventorySlots.INVENTORY_SLOT_BAG_END)
                            {
                                if (slot < EquipmentSlots.EQUIPMENT_SLOT_END)
                                    SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_0 + slot * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE, 0);
                                var tmp = Items;
                                var argItem = tmp[slot];
                                UpdateRemoveItemStats(ref argItem, slot);
                                tmp[slot] = argItem;
                            }

                            SetUpdateFlag(EPlayerFields.PLAYER_FIELD_INV_SLOT_HEAD + (int)slot * 2, 0);
                            if (Destroy)
                                Items[slot].Delete();
                            Items.Remove(slot);
                            if (Update)
                                SendCharacterUpdate(true);
                            return;
                        }
                    }
                }

                // DONE: Search in bags
                for (byte bag = InventorySlots.INVENTORY_SLOT_BAG_1, loopTo1 = InventorySlots.INVENTORY_SLOT_BAG_END - 1; bag <= loopTo1; bag++)
                {
                    if (Items.ContainsKey(bag))
                    {

                        // DONE: Search this bag
                        byte slot;
                        var loopTo2 = (byte)(Items[bag].ItemInfo.ContainerSlots - 1);
                        for (slot = 0; slot <= loopTo2; slot++)
                        {
                            if (Items[bag].Items.ContainsKey(slot))
                            {
                                if (Items[bag].Items[slot].GUID == itemGuid)
                                {
                                    WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_inventory SET item_slot = {0}, item_bag = {1} WHERE item_guid = {2};", WorldServiceLocator._Global_Constants.ITEM_SLOT_NULL, WorldServiceLocator._Global_Constants.ITEM_BAG_NULL, Items[bag].Items[slot].GUID - WorldServiceLocator._Global_Constants.GUID_ITEM));
                                    if (Destroy)
                                        Items[bag].Items[slot].Delete();
                                    Items[bag].Items.Remove(slot);
                                    if (Update)
                                        SendItemUpdate(Items[bag]);
                                    return;
                                }
                            }
                        }
                    }
                }

                throw new ApplicationException("Unable to remove item because character doesn't have it in inventory or bags.");
            }

            public bool ItemADD(ref ItemObject Item)
            {
                int tmpEntry = Item.ItemEntry;
                byte tmpCount = (byte)Item.StackCount;
                // DONE: Check for max stack
                if (tmpCount > Item.ItemInfo.Stackable)
                    tmpCount = (byte)Item.ItemInfo.Stackable;
                // DONE: Check for unique
                if (Item.ItemInfo.Unique > 0 && ItemCOUNT(Item.ItemEntry) >= Item.ItemInfo.Unique)
                    return false;
                // DONE: Add the item
                if (ItemADD_AutoSlot(ref Item) && client is object)
                {
                    // DONE: Fire quest event to check for if this item is required for quest
                    // TODO: This needs to be fired BEFORE the client has the item in the bag...
                    // NOTE: Not only quest items are needed for quests
                    var argobjCharacter = this;
                    WorldServiceLocator._WorldServer.ALLQUESTS.OnQuestItemAdd(ref argobjCharacter, tmpEntry, tmpCount);
                    return true;
                }

                return false;
            }

            public int[] BuyBackTimeStamp = new int[BuyBackSlots.BUYBACK_SLOT_END - BuyBackSlots.BUYBACK_SLOT_START - 1 + 1];

            public void ItemADD_BuyBack(ref ItemObject Item)
            {
                byte i;
                byte Slot;
                byte eSlot;
                int OldestTime;
                byte OldestSlot;
                Slot = WorldServiceLocator._Global_Constants.ITEM_SLOT_NULL;
                OldestTime = (int)WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now);
                OldestSlot = WorldServiceLocator._Global_Constants.ITEM_SLOT_NULL;
                var loopTo = BuyBackSlots.BUYBACK_SLOT_END - 1;
                for (i = BuyBackSlots.BUYBACK_SLOT_START; i <= loopTo; i++)
                {
                    if (Items.ContainsKey(i) == false || BuyBackTimeStamp[i - BuyBackSlots.BUYBACK_SLOT_START] == 0) // Woho we found a empty slot to use!
                    {
                        if (Slot == WorldServiceLocator._Global_Constants.ITEM_SLOT_NULL)
                            Slot = i;
                    }
                    else if (BuyBackTimeStamp[i - BuyBackSlots.BUYBACK_SLOT_START] < OldestTime) // If not let's find out the oldest item in the buyback
                    {
                        OldestTime = BuyBackTimeStamp[i - BuyBackSlots.BUYBACK_SLOT_START];
                        OldestSlot = i;
                    }
                }

                if (Slot == WorldServiceLocator._Global_Constants.ITEM_SLOT_NULL) // We never found a empty slot so let's just remove the oldest item
                {
                    if (OldestSlot != WorldServiceLocator._Global_Constants.ITEM_SLOT_NULL)
                        return; // Somehow it all got very wrong o_O
                    ItemREMOVE(0, OldestSlot, true, true);
                    Slot = OldestSlot;
                }
                // Now we have a empty slow so let's just put our item there
                eSlot = Slot - BuyBackSlots.BUYBACK_SLOT_START;
                BuyBackTimeStamp[eSlot] = (int)WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now);
                SetUpdateFlag(EPlayerFields.PLAYER_FIELD_BUYBACK_TIMESTAMP_1 + eSlot, BuyBackTimeStamp[(int)eSlot]);
                SetUpdateFlag(EPlayerFields.PLAYER_FIELD_BUYBACK_PRICE_1 + eSlot, Item.ItemInfo.SellPrice * Item.StackCount);
                ItemSETSLOT(ref Item, 0, Slot);
            }

            public bool ItemADD_AutoSlot(ref ItemObject Item)
            {
                if (Item.ItemInfo.Stackable > 1)
                {
                    // DONE: Search for stackable in special bags
                    if (Item.ItemInfo.BagFamily == ITEM_BAG.KEYRING || Item.ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_KEY)
                    {
                        for (byte slot = KeyRingSlots.KEYRING_SLOT_START, loopTo = KeyRingSlots.KEYRING_SLOT_END - 1; slot <= loopTo; slot++)
                        {
                            if (Items.ContainsKey(slot) && Items[slot].ItemEntry == Item.ItemEntry && Items[slot].StackCount < Items[slot].ItemInfo.Stackable)
                            {
                                int stacked = Items[slot].ItemInfo.Stackable - Items[slot].StackCount;
                                if (stacked >= Item.StackCount)
                                {
                                    Items[slot].StackCount += Item.StackCount;
                                    Item.Delete();
                                    Item = Items[slot];
                                    Items[slot].Save();
                                    SendItemUpdate(Items[slot]);
                                    return true;
                                }
                                else if (stacked > 0)
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
                        }
                    }
                    else if (Item.ItemInfo.BagFamily != 0)
                    {
                        for (byte bag = InventorySlots.INVENTORY_SLOT_BAG_START, loopTo1 = InventorySlots.INVENTORY_SLOT_BAG_END - 1; bag <= loopTo1; bag++)
                        {
                            if (Items.ContainsKey(bag) && Items[bag].ItemInfo.SubClass != ITEM_SUBCLASS.ITEM_SUBCLASS_BAG)
                            {
                                if (Items[bag].ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_SOUL_BAG && Item.ItemInfo.BagFamily == ITEM_BAG.SOUL_SHARD || Items[bag].ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_HERB_BAG && Item.ItemInfo.BagFamily == ITEM_BAG.HERB || Items[bag].ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_ENCHANTING_BAG && Item.ItemInfo.BagFamily == ITEM_BAG.ENCHANTING || Items[bag].ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_QUIVER && Item.ItemInfo.BagFamily == ITEM_BAG.ARROW || Items[bag].ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_AMMO_POUCH && Item.ItemInfo.BagFamily == ITEM_BAG.BULLET)
                                {
                                    foreach (KeyValuePair<byte, ItemObject> slot in Items[bag].Items)
                                    {
                                        if (slot.Value.ItemEntry == Item.ItemEntry && slot.Value.StackCount < slot.Value.ItemInfo.Stackable)
                                        {
                                            int stacked = slot.Value.ItemInfo.Stackable - slot.Value.StackCount;
                                            if (stacked >= Item.StackCount)
                                            {
                                                slot.Value.StackCount += Item.StackCount;
                                                Item.Delete();
                                                Item = slot.Value;
                                                slot.Value.Save();
                                                SendItemUpdate(slot.Value);
                                                SendItemUpdate(Items[bag]);
                                                return true;
                                            }
                                            else if (stacked > 0)
                                            {
                                                slot.Value.StackCount += stacked;
                                                Item.StackCount -= stacked;
                                                slot.Value.Save();
                                                Item.Save();
                                                SendItemUpdate(slot.Value);
                                                SendItemUpdate(Item);
                                                SendItemUpdate(Items[bag]);
                                                return ItemADD_AutoSlot(ref Item);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    // DONE: Search for stackable in main bag
                    for (byte slot = InventoryPackSlots.INVENTORY_SLOT_ITEM_START, loopTo2 = InventoryPackSlots.INVENTORY_SLOT_ITEM_END - 1; slot <= loopTo2; slot++)
                    {
                        if (Items.ContainsKey(slot) && Items[slot].ItemEntry == Item.ItemEntry && Items[slot].StackCount < Items[slot].ItemInfo.Stackable)
                        {
                            int stacked = Items[slot].ItemInfo.Stackable - Items[slot].StackCount;
                            if (stacked >= Item.StackCount)
                            {
                                Items[slot].StackCount += Item.StackCount;
                                Item.Delete();
                                Item = Items[slot];
                                Items[slot].Save();
                                SendItemUpdate(Items[slot]);
                                return true;
                            }
                            else if (stacked > 0)
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
                    }
                    // DONE: Search for stackable in bags
                    for (byte bag = InventorySlots.INVENTORY_SLOT_BAG_START, loopTo3 = InventorySlots.INVENTORY_SLOT_BAG_END - 1; bag <= loopTo3; bag++)
                    {
                        if (Items.ContainsKey(bag))
                        {
                            foreach (KeyValuePair<byte, ItemObject> slot in Items[bag].Items)
                            {
                                if (slot.Value.ItemEntry == Item.ItemEntry && slot.Value.StackCount < slot.Value.ItemInfo.Stackable)
                                {
                                    int stacked = slot.Value.ItemInfo.Stackable - slot.Value.StackCount;
                                    if (stacked >= Item.StackCount)
                                    {
                                        slot.Value.StackCount += Item.StackCount;
                                        Item.Delete();
                                        Item = slot.Value;
                                        slot.Value.Save();
                                        SendItemUpdate(slot.Value);
                                        SendItemUpdate(Items[bag]);
                                        return true;
                                    }
                                    else if (stacked > 0)
                                    {
                                        slot.Value.StackCount += stacked;
                                        Item.StackCount -= stacked;
                                        slot.Value.Save();
                                        Item.Save();
                                        SendItemUpdate(slot.Value);
                                        SendItemUpdate(Item);
                                        SendItemUpdate(Items[bag]);
                                        return ItemADD_AutoSlot(ref Item);
                                    }
                                }
                            }
                        }
                    }
                }

                if (Item.ItemInfo.BagFamily == ITEM_BAG.KEYRING || Item.ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_KEY)
                {
                    // DONE: Insert as keyring
                    for (byte slot = KeyRingSlots.KEYRING_SLOT_START, loopTo4 = KeyRingSlots.KEYRING_SLOT_END - 1; slot <= loopTo4; slot++)
                    {
                        if (!Items.ContainsKey(slot))
                        {
                            return ItemSETSLOT(ref Item, 0, slot);
                        }
                    }
                }
                else if (Item.ItemInfo.BagFamily != 0)
                {
                    // DONE: Insert in free special bag
                    for (byte bag = InventorySlots.INVENTORY_SLOT_BAG_START, loopTo5 = InventorySlots.INVENTORY_SLOT_BAG_END - 1; bag <= loopTo5; bag++)
                    {
                        if (Items.ContainsKey(bag) && Items[bag].ItemInfo.SubClass != ITEM_SUBCLASS.ITEM_SUBCLASS_BAG)
                        {
                            if (Items[bag].ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_CONTAINER && Items[bag].ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_SOUL_BAG && Item.ItemInfo.BagFamily == ITEM_BAG.SOUL_SHARD || Items[bag].ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_CONTAINER && Items[bag].ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_HERB_BAG && Item.ItemInfo.BagFamily == ITEM_BAG.HERB || Items[bag].ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_CONTAINER && Items[bag].ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_ENCHANTING_BAG && Item.ItemInfo.BagFamily == ITEM_BAG.ENCHANTING || Items[bag].ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_QUIVER && Items[bag].ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_QUIVER && Item.ItemInfo.BagFamily == ITEM_BAG.ARROW || Items[bag].ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_QUIVER && Items[bag].ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_AMMO_POUCH && Item.ItemInfo.BagFamily == ITEM_BAG.BULLET)
                            {
                                for (byte slot = 0, loopTo6 = (byte)(Items[bag].ItemInfo.ContainerSlots - 1); slot <= loopTo6; slot++)
                                {
                                    if (!Items[bag].Items.ContainsKey(slot))
                                    {
                                        return ItemSETSLOT(ref Item, bag, slot);
                                    }
                                }
                            }
                        }
                    }
                }

                // DONE: Insert as new item in inventory
                for (byte slot = InventoryPackSlots.INVENTORY_SLOT_ITEM_START, loopTo7 = InventoryPackSlots.INVENTORY_SLOT_ITEM_END - 1; slot <= loopTo7; slot++)
                {
                    if (!Items.ContainsKey(slot))
                    {
                        return ItemSETSLOT(ref Item, 0, slot);
                    }
                }
                // DONE: Insert as new item in bag
                for (byte bag = InventorySlots.INVENTORY_SLOT_BAG_START, loopTo8 = InventorySlots.INVENTORY_SLOT_BAG_END - 1; bag <= loopTo8; bag++)
                {
                    if (Items.ContainsKey(bag) && Items[bag].ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_BAG)
                    {
                        for (byte slot = 0, loopTo9 = (byte)(Items[bag].ItemInfo.ContainerSlots - 1); slot <= loopTo9; slot++)
                        {
                            if (!Items[bag].Items.ContainsKey(slot) && ItemCANEQUIP(Item, bag, slot) == InventoryChangeFailure.EQUIP_ERR_OK)
                            {
                                return ItemSETSLOT(ref Item, bag, slot);
                            }
                        }
                    }
                }

                // DONE: Send error, not free slot
                var argobjCharacter = this;
                WorldServiceLocator._WS_Items.SendInventoryChangeFailure(ref argobjCharacter, InventoryChangeFailure.EQUIP_ERR_INVENTORY_FULL, 0, 0);
                return false;
            }

            public bool ItemADD_AutoBag(ref ItemObject Item, byte dstBag)
            {
                if (dstBag == 0)
                {
                    if (Item.ItemInfo.Stackable > 1)
                    {
                        // DONE: Search for stackable in main bag
                        for (byte slot = InventoryPackSlots.INVENTORY_SLOT_ITEM_START, loopTo = InventoryPackSlots.INVENTORY_SLOT_ITEM_END - 1; slot <= loopTo; slot++)
                        {
                            if (Items[slot].ItemEntry == Item.ItemEntry && Items[slot].StackCount < Items[slot].ItemInfo.Stackable)
                            {
                                byte stacked = (byte)(Items[slot].ItemInfo.Stackable - Items[slot].StackCount);
                                if (stacked >= Item.StackCount)
                                {
                                    Items[slot].StackCount += Item.StackCount;
                                    Item.Delete();
                                    Item = Items[slot];
                                    Items[slot].Save();
                                    SendItemUpdate(Items[slot]);
                                    return true;
                                }
                                else if (stacked > 0)
                                {
                                    Items[slot].StackCount += stacked;
                                    Item.StackCount -= stacked;
                                    Items[slot].Save();
                                    Item.Save();
                                    SendItemUpdate(Items[slot]);
                                    SendItemUpdate(Item);
                                    return ItemADD_AutoBag(ref Item, dstBag);
                                }
                            }
                        }
                    }
                    // DONE: Insert as keyring
                    if (Item.ItemInfo.BagFamily == ITEM_BAG.KEYRING)
                    {
                        for (byte slot = KeyRingSlots.KEYRING_SLOT_START, loopTo1 = KeyRingSlots.KEYRING_SLOT_END - 1; slot <= loopTo1; slot++)
                        {
                            if (!Items.ContainsKey(slot))
                            {
                                return ItemSETSLOT(ref Item, 0, slot);
                            }
                        }
                    }
                    // DONE: Insert as new item in inventory
                    for (byte slot = InventoryPackSlots.INVENTORY_SLOT_ITEM_START, loopTo2 = InventoryPackSlots.INVENTORY_SLOT_ITEM_END - 1; slot <= loopTo2; slot++)
                    {
                        if (!Items.ContainsKey(slot))
                        {
                            return ItemSETSLOT(ref Item, 0, slot);
                        }
                    }
                }
                else if (Items.ContainsKey(dstBag))
                {
                    if (Items[dstBag].ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_CONTAINER && Items[dstBag].ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_SOUL_BAG && Item.ItemInfo.BagFamily != ITEM_BAG.SOUL_SHARD || Items[dstBag].ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_CONTAINER && Items[dstBag].ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_HERB_BAG && Item.ItemInfo.BagFamily != ITEM_BAG.HERB || Items[dstBag].ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_CONTAINER && Items[dstBag].ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_ENCHANTING_BAG && Item.ItemInfo.BagFamily != ITEM_BAG.ENCHANTING || Items[dstBag].ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_QUIVER && Items[dstBag].ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_QUIVER && Item.ItemInfo.BagFamily != ITEM_BAG.ARROW || Items[dstBag].ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_QUIVER && Items[dstBag].ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_BULLET && Item.ItemInfo.BagFamily != ITEM_BAG.BULLET)
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "{0} - {1} - {2}", Items[dstBag].ItemInfo.ObjectClass, Items[dstBag].ItemInfo.SubClass, Item.ItemInfo.BagFamily);
                        var argobjCharacter = this;
                        WorldServiceLocator._WS_Items.SendInventoryChangeFailure(ref argobjCharacter, InventoryChangeFailure.EQUIP_ERR_ITEM_DOESNT_GO_INTO_BAG, Item.GUID, 0);
                        return false;
                    }

                    if (Item.ItemInfo.Stackable > 1)
                    {
                        // DONE: Search for stackable in bag
                        foreach (KeyValuePair<byte, ItemObject> i in Items[dstBag].Items)
                        {
                            if (i.Value.ItemEntry == Item.ItemEntry && i.Value.StackCount < i.Value.ItemInfo.Stackable)
                            {
                                byte stacked = (byte)(i.Value.ItemInfo.Stackable - i.Value.StackCount);
                                if (stacked >= Item.StackCount)
                                {
                                    i.Value.StackCount += Item.StackCount;
                                    Item.Delete();
                                    Item = i.Value;
                                    i.Value.Save();
                                    SendItemUpdate(i.Value);
                                    return true;
                                }
                                else if (stacked > 0)
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
                    // DONE: Insert as new item in bag
                    for (byte slot = 0, loopTo3 = (byte)(Items[dstBag].ItemInfo.ContainerSlots - 1); slot <= loopTo3; slot++)
                    {
                        if (!Items[dstBag].Items.ContainsKey(slot) && ItemCANEQUIP(Item, dstBag, slot) == InventoryChangeFailure.EQUIP_ERR_OK)
                        {
                            return ItemSETSLOT(ref Item, dstBag, slot);
                        }
                    }
                }

                // DONE: Send error, not free slot
                var argobjCharacter1 = this;
                WorldServiceLocator._WS_Items.SendInventoryChangeFailure(ref argobjCharacter1, InventoryChangeFailure.EQUIP_ERR_BAG_FULL, Item.GUID, 0);
                return false;
            }

            public bool ItemSETSLOT(ref ItemObject Item, byte dstBag, byte dstSlot)
            {
                if (Item.ItemInfo.Bonding == ITEM_BONDING_TYPE.BIND_WHEN_PICKED_UP && Item.IsSoulBound == false)
                {
                    WS_Network.ClientClass argclient = null;
                    Item.SoulbindItem(client: ref argclient);
                }

                if ((Item.ItemInfo.Bonding == ITEM_BONDING_TYPE.BIND_UNK_QUESTITEM1 || Item.ItemInfo.Bonding == ITEM_BONDING_TYPE.BIND_UNK_QUESTITEM2) && Item.IsSoulBound == false)
                {
                    WS_Network.ClientClass argclient1 = null;
                    Item.SoulbindItem(client: ref argclient1);
                }

                if (dstBag == 0)
                {
                    // DONE: Bind a nonbinded BIND WHEN PICKED UP item or a nonbinded quest item
                    // DONE: Put in inventory
                    Items[dstSlot] = Item;
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_inventory SET item_slot = {0}, item_bag = {1}, item_stackCount = {2} WHERE item_guid = {3};", dstSlot, GUID, Item.StackCount, Item.GUID - WorldServiceLocator._Global_Constants.GUID_ITEM));
                    SetUpdateFlag(EPlayerFields.PLAYER_FIELD_INV_SLOT_HEAD + (int)dstSlot * 2, Item.GUID);
                    if (dstSlot < EquipmentSlots.EQUIPMENT_SLOT_END)
                    {
                        SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_0 + dstSlot * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE, Item.ItemEntry);
                        // For Each Enchant As KeyValuePair(Of Byte, TEnchantmentInfo) In Item.Enchantments
                        // SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_0 + Enchant.Key + dstSlot * _Global_Constants.PLAYER_VISIBLE_ITEM_SIZE, Enchant.Value.ID)
                        // Next
                        SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_PROPERTIES + dstSlot * WorldServiceLocator._Global_Constants.PLAYER_VISIBLE_ITEM_SIZE, Item.RandomProperties);
                        // DONE: Bind a nonbinded BIND WHEN EQUIPPED item
                        if (Item.ItemInfo.Bonding == ITEM_BONDING_TYPE.BIND_WHEN_EQUIPED && Item.IsSoulBound == false)
                        {
                            WS_Network.ClientClass argclient2 = null;
                            Item.SoulbindItem(client: ref argclient2);
                        }
                    }
                }
                else
                {
                    // DONE: Put in bag
                    Items[dstBag].Items[dstSlot] = Item;
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_inventory SET item_slot = {0}, item_bag = {1}, item_stackCount = {2} WHERE item_guid = {3};", dstSlot, Items[dstBag].GUID, Item.StackCount, Item.GUID - WorldServiceLocator._Global_Constants.GUID_ITEM));
                }

                // DONE: Send updates
                if (client is object)
                {
                    SendItemAndCharacterUpdate(Item, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT);
                    if (dstBag > 0)
                        SendItemUpdate(Items[dstBag]);
                }

                return true;
            }

            public int ItemCOUNT(int ItemEntry, bool Equipped = false)
            {
                int count = 0;

                // DONE: Search in inventory
                byte EndSlot = InventoryPackSlots.INVENTORY_SLOT_ITEM_END;
                if (Equipped)
                    EndSlot = EquipmentSlots.EQUIPMENT_SLOT_END;
                for (byte slot = EquipmentSlots.EQUIPMENT_SLOT_START, loopTo = (byte)(EndSlot - 1); slot <= loopTo; slot++)
                {
                    if (Items.ContainsKey(slot))
                    {
                        if (Items[slot].ItemEntry == ItemEntry)
                            count += Items[slot].StackCount;
                    }
                }

                if (Equipped)
                    return count;

                // DONE: Search in keyring
                for (byte slot = KeyRingSlots.KEYRING_SLOT_START, loopTo1 = KeyRingSlots.KEYRING_SLOT_END - 1; slot <= loopTo1; slot++)
                {
                    if (Items.ContainsKey(slot))
                    {
                        if (Items[slot].ItemEntry == ItemEntry)
                            count += Items[slot].StackCount;
                    }
                }

                // DONE: Search in bags
                for (byte bag = InventorySlots.INVENTORY_SLOT_BAG_1, loopTo2 = InventorySlots.INVENTORY_SLOT_BAG_END - 1; bag <= loopTo2; bag++)
                {
                    if (Items.ContainsKey(bag))
                    {

                        // DONE: Search this bag
                        byte slot;
                        var loopTo3 = (byte)(Items[bag].ItemInfo.ContainerSlots - 1);
                        for (slot = 0; slot <= loopTo3; slot++)
                        {
                            if (Items[bag].Items.ContainsKey(slot))
                            {
                                if (Items[bag].Items[slot].ItemEntry == ItemEntry)
                                    count += Items[bag].Items[slot].StackCount;
                            }
                        }
                    }
                }

                return count;
            }

            public bool ItemCONSUME(int ItemEntry, int Count)
            {
                // DONE: Search in inventory
                for (byte slot = EquipmentSlots.EQUIPMENT_SLOT_START, loopTo = InventoryPackSlots.INVENTORY_SLOT_ITEM_END - 1; slot <= loopTo; slot++)
                {
                    if (Items.ContainsKey(slot))
                    {
                        if (Items[slot].ItemEntry == ItemEntry)
                        {
                            if (Items[slot].StackCount <= Count)
                            {
                                Count -= Items[slot].StackCount;
                                ItemREMOVE(0, slot, true, true);
                                if (Count <= 0)
                                    return true;
                            }
                            else
                            {
                                Items[slot].StackCount -= Count;
                                Items[slot].Save(false);
                                SendItemUpdate(Items[slot]);
                                return true;
                            }
                        }
                    }
                }

                // DONE: Search in keyring slot
                for (byte slot = KeyRingSlots.KEYRING_SLOT_START, loopTo1 = KeyRingSlots.KEYRING_SLOT_END - 1; slot <= loopTo1; slot++)
                {
                    if (Items.ContainsKey(slot))
                    {
                        if (Items[slot].ItemEntry == ItemEntry)
                        {
                            if (Items[slot].StackCount <= Count)
                            {
                                Count -= Items[slot].StackCount;
                                ItemREMOVE(0, slot, true, true);
                                if (Count <= 0)
                                    return true;
                            }
                            else
                            {
                                Items[slot].StackCount -= Count;
                                Items[slot].Save(false);
                                SendItemUpdate(Items[slot]);
                                return true;
                            }
                        }
                    }
                }

                // DONE: Search in bags
                for (byte bag = InventorySlots.INVENTORY_SLOT_BAG_1, loopTo2 = InventorySlots.INVENTORY_SLOT_BAG_END - 1; bag <= loopTo2; bag++)
                {
                    if (Items.ContainsKey(bag))
                    {

                        // DONE: Search this bag
                        byte slot;
                        var loopTo3 = (byte)(Items[bag].ItemInfo.ContainerSlots - 1);
                        for (slot = 0; slot <= loopTo3; slot++)
                        {
                            if (Items[bag].Items.ContainsKey(slot))
                            {
                                if (Items[bag].Items[slot].ItemEntry == ItemEntry)
                                {
                                    if (Items[bag].Items[slot].StackCount <= Count)
                                    {
                                        Count -= Items[bag].Items[slot].StackCount;
                                        ItemREMOVE(bag, slot, true, true);
                                        if (Count <= 0)
                                            return true;
                                    }
                                    else
                                    {
                                        Items[bag].Items[slot].StackCount -= Count;
                                        Items[bag].Items[slot].Save(false);
                                        SendItemUpdate(Items[bag].Items[slot]);
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }

                return false;
            }

            public int ItemFREESLOTS()
            {
                int foundFreeSlots = 0;

                // DONE Find space in main bag
                for (byte slot = InventoryPackSlots.INVENTORY_SLOT_ITEM_START, loopTo = InventoryPackSlots.INVENTORY_SLOT_ITEM_END - 1; slot <= loopTo; slot++)
                {
                    if (!Items.ContainsKey(slot))
                    {
                        foundFreeSlots += 1;
                    }
                }

                // DONE: Find space in other bags
                for (byte bag = InventorySlots.INVENTORY_SLOT_BAG_START, loopTo1 = InventorySlots.INVENTORY_SLOT_BAG_END - 1; bag <= loopTo1; bag++)
                {
                    if (Items.ContainsKey(bag))
                    {
                        for (byte slot = 0, loopTo2 = (byte)(Items[bag].ItemInfo.ContainerSlots - 1); slot <= loopTo2; slot++)
                        {
                            if (!Items[bag].Items.ContainsKey(slot))
                            {
                                foundFreeSlots += 1;
                            }
                        }
                    }
                }

                return foundFreeSlots;
            }

            public InventoryChangeFailure ItemCANEQUIP(ItemObject Item, byte dstBag, byte dstSlot)
            {
                // DONE: if dead then EQUIP_ERR_YOU_ARE_DEAD
                if (DEAD)
                    return InventoryChangeFailure.EQUIP_ERR_YOU_ARE_DEAD;
                var ItemInfo = Item.ItemInfo;
                try
                {
                    if (dstBag == 0)
                    {
                        // DONE: items in inventory
                        switch (dstSlot)
                        {
                            case var @case when @case < EquipmentSlots.EQUIPMENT_SLOT_END:
                                {
                                    if (ItemInfo.IsContainer)
                                    {
                                        return InventoryChangeFailure.EQUIP_ERR_ITEM_CANT_BE_EQUIPPED;
                                    }

                                    if (!WorldServiceLocator._Functions.HaveFlag(ItemInfo.AvailableClasses, Classe - 1))
                                    {
                                        return InventoryChangeFailure.EQUIP_ERR_YOU_CAN_NEVER_USE_THAT_ITEM;
                                    }

                                    if (!WorldServiceLocator._Functions.HaveFlag(ItemInfo.AvailableRaces, Race - 1))
                                    {
                                        return InventoryChangeFailure.EQUIP_ERR_YOU_CAN_NEVER_USE_THAT_ITEM2;
                                    }

                                    if (ItemInfo.ReqLevel > Level)
                                    {
                                        return InventoryChangeFailure.EQUIP_ERR_YOU_MUST_REACH_LEVEL_N;
                                    }

                                    bool tmp = false;
                                    foreach (byte SlotVal in ItemInfo.GetSlots)
                                    {
                                        if (dstSlot == SlotVal)
                                            tmp = true;
                                    }

                                    if (!tmp)
                                        return InventoryChangeFailure.EQUIP_ERR_ITEM_DOESNT_GO_TO_SLOT;
                                    if (dstSlot == EquipmentSlots.EQUIPMENT_SLOT_MAINHAND && ItemInfo.InventoryType == INVENTORY_TYPES.INVTYPE_TWOHAND_WEAPON && Items.ContainsKey(EquipmentSlots.EQUIPMENT_SLOT_OFFHAND))
                                    {
                                        return InventoryChangeFailure.EQUIP_ERR_CANT_EQUIP_WITH_TWOHANDED;
                                    }

                                    if (dstSlot == EquipmentSlots.EQUIPMENT_SLOT_OFFHAND && Items.ContainsKey(EquipmentSlots.EQUIPMENT_SLOT_MAINHAND))
                                    {
                                        if (this.Items(EquipmentSlots.EQUIPMENT_SLOT_MAINHAND).ItemInfo.InventoryType == INVENTORY_TYPES.INVTYPE_TWOHAND_WEAPON)
                                        {
                                            return InventoryChangeFailure.EQUIP_ERR_CANT_EQUIP_WITH_TWOHANDED;
                                        }
                                    }

                                    if (dstSlot == EquipmentSlots.EQUIPMENT_SLOT_OFFHAND && ItemInfo.InventoryType == INVENTORY_TYPES.INVTYPE_WEAPON)
                                    {
                                        if (!Skills.ContainsKey(SKILL_IDs.SKILL_DUAL_WIELD))
                                            return InventoryChangeFailure.EQUIP_ERR_CANT_DUAL_WIELD;
                                    }

                                    if (ItemInfo.GetReqSkill != 0)
                                    {
                                        if (!Skills.ContainsKey(ItemInfo.GetReqSkill))
                                            return InventoryChangeFailure.EQUIP_ERR_NO_REQUIRED_PROFICIENCY;
                                    }

                                    if (ItemInfo.GetReqSpell != 0)
                                    {
                                        if (!Spells.ContainsKey(ItemInfo.GetReqSpell))
                                            return InventoryChangeFailure.EQUIP_ERR_NO_REQUIRED_PROFICIENCY;
                                    }

                                    if (ItemInfo.ReqSkill != 0)
                                    {
                                        if (!Skills.ContainsKey(ItemInfo.ReqSkill))
                                            return InventoryChangeFailure.EQUIP_ERR_NO_REQUIRED_PROFICIENCY;
                                        if (Skills[ItemInfo.ReqSkill].Current < ItemInfo.ReqSkillRank)
                                            return InventoryChangeFailure.EQUIP_ERR_SKILL_ISNT_HIGH_ENOUGH;
                                    }

                                    if (ItemInfo.ReqSpell != 0)
                                    {
                                        if (!Spells.ContainsKey(ItemInfo.ReqSpell))
                                            return InventoryChangeFailure.EQUIP_ERR_NO_REQUIRED_PROFICIENCY;
                                    }
                                    // NOTE: Not used anymore in new honor system
                                    if (ItemInfo.ReqHonorRank != 0)
                                    {
                                        if (HonorHighestRank < ItemInfo.ReqHonorRank)
                                            return InventoryChangeFailure.EQUIP_ITEM_RANK_NOT_ENOUGH;
                                    }

                                    if (ItemInfo.ReqFaction != 0)
                                    {
                                        if (client.Character.GetReputation(ItemInfo.ReqFaction) <= ItemInfo.ReqFactionLevel)
                                            return InventoryChangeFailure.EQUIP_ITEM_REPUTATION_NOT_ENOUGH;
                                    }

                                    return InventoryChangeFailure.EQUIP_ERR_OK;
                                }

                            case var case1 when case1 < InventorySlots.INVENTORY_SLOT_BAG_END:
                                {
                                    if (!ItemInfo.IsContainer)
                                        return InventoryChangeFailure.EQUIP_ERR_NOT_A_BAG;
                                    if (!Item.IsFree)
                                        return InventoryChangeFailure.EQUIP_ERR_NONEMPTY_BAG_OVER_OTHER_BAG;
                                    return InventoryChangeFailure.EQUIP_ERR_OK;
                                }

                            case var case2 when case2 < InventoryPackSlots.INVENTORY_SLOT_ITEM_END:
                                {
                                    if (ItemInfo.IsContainer)
                                    {
                                        // DONE: Move only empty bags
                                        if (Item.IsFree)
                                        {
                                            return InventoryChangeFailure.EQUIP_ERR_OK;
                                        }
                                        else
                                        {
                                            return InventoryChangeFailure.EQUIP_ERR_CAN_ONLY_DO_WITH_EMPTY_BAGS;
                                        }
                                    }

                                    return InventoryChangeFailure.EQUIP_ERR_OK;
                                }

                            case var case3 when case3 < BankItemSlots.BANK_SLOT_ITEM_END:
                                {
                                    if (ItemInfo.IsContainer)
                                    {
                                        // DONE: Move only empty bags
                                        if (Item.IsFree)
                                        {
                                            return InventoryChangeFailure.EQUIP_ERR_OK;
                                        }
                                        else
                                        {
                                            return InventoryChangeFailure.EQUIP_ERR_CAN_ONLY_DO_WITH_EMPTY_BAGS;
                                        }
                                    }

                                    return InventoryChangeFailure.EQUIP_ERR_OK;
                                }

                            case var case4 when case4 < BankBagSlots.BANK_SLOT_BAG_END:
                                {
                                    if (dstSlot >= BankBagSlots.BANK_SLOT_BAG_START + Items_AvailableBankSlots)
                                        return InventoryChangeFailure.EQUIP_ERR_MUST_PURCHASE_THAT_BAG_SLOT;
                                    if (!ItemInfo.IsContainer)
                                        return InventoryChangeFailure.EQUIP_ERR_NOT_A_BAG;
                                    if (!Item.IsFree)
                                        return InventoryChangeFailure.EQUIP_ERR_NONEMPTY_BAG_OVER_OTHER_BAG;
                                    return InventoryChangeFailure.EQUIP_ERR_OK;
                                }

                            case var case5 when case5 < KeyRingSlots.KEYRING_SLOT_END:
                                {
                                    if (ItemInfo.BagFamily != ITEM_BAG.KEYRING && ItemInfo.ObjectClass != ITEM_CLASS.ITEM_CLASS_KEY)
                                        return InventoryChangeFailure.EQUIP_ERR_ITEM_DOESNT_GO_TO_SLOT;
                                    return InventoryChangeFailure.EQUIP_ERR_OK;
                                }

                            default:
                                {
                                    return InventoryChangeFailure.EQUIP_ERR_ITEM_CANT_BE_EQUIPPED;
                                }
                        }
                    }
                    else
                    {
                        // DONE: Items in bags
                        if (!Items.ContainsKey(dstBag))
                            return InventoryChangeFailure.EQUIP_ERR_ITEM_DOESNT_GO_INTO_BAG;
                        if (ItemInfo.IsContainer)
                        {
                            if (Item.IsFree)
                            {
                                return InventoryChangeFailure.EQUIP_ERR_OK;
                            }
                            else
                            {
                                return InventoryChangeFailure.EQUIP_ERR_CAN_ONLY_DO_WITH_EMPTY_BAGS;
                            }
                        }

                        if (Items[dstBag].ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_QUIVER)
                        {
                            if (ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_PROJECTILE)
                            {
                                if (Items[dstBag].ItemInfo.SubClass != ItemInfo.SubClass)
                                {
                                    // Inserting Ammo in not proper AmmoType bag
                                    return InventoryChangeFailure.EQUIP_ERR_ITEM_DOESNT_GO_INTO_BAG;
                                }
                                else
                                {
                                    // Inserting Ammo in proper AmmoType bag
                                    return InventoryChangeFailure.EQUIP_ERR_OK;
                                }
                            }
                            else
                            {
                                return InventoryChangeFailure.EQUIP_ERR_ONLY_AMMO_CAN_GO_HERE;
                            }
                        }
                        else
                        {
                            return InventoryChangeFailure.EQUIP_ERR_OK;
                        }
                    }
                }
                catch (Exception err)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "[{0}:{1}] Unable to equip item. {2} {3}", client.IP, client.Port, Environment.NewLine, err.ToString());
                    return InventoryChangeFailure.EQUIP_ERR_CANT_DO_RIGHT_NOW;
                }
            }

            public bool ItemSTACK(byte srcBag, byte srcSlot, byte dstBag, byte dstSlot)
            {
                ItemObject srcItem;
                if (srcBag != 0)
                {
                    srcItem = Items[srcBag].Items[srcSlot];
                }
                else
                {
                    srcItem = Items[srcSlot];
                }

                ItemObject dstItem;
                if (dstBag != 0)
                {
                    dstItem = Items[dstBag].Items[dstSlot];
                }
                else
                {
                    dstItem = Items[dstSlot];
                }

                // DONE: If already full, just swap
                if (srcItem.StackCount == dstItem.ItemInfo.Stackable | dstItem.StackCount == dstItem.ItemInfo.Stackable)
                    return false;

                // DONE: Same item types -> stack if not full, else just swap !Nooo, else fill
                if (srcItem.ItemEntry == dstItem.ItemEntry && dstItem.StackCount + srcItem.StackCount <= dstItem.ItemInfo.Stackable)
                {
                    dstItem.StackCount += srcItem.StackCount;
                    ItemREMOVE(srcBag, srcSlot, true, true);
                    SendItemUpdate(dstItem);
                    if (dstBag > 0)
                        SendItemUpdate(Items[dstBag]);
                    dstItem.Save(false);
                    return true;
                }
                // DONE: Same item types, but bigger than max count -> fill destination
                if (srcItem.ItemEntry == dstItem.ItemEntry)
                {
                    srcItem.StackCount -= dstItem.ItemInfo.Stackable - dstItem.StackCount;
                    dstItem.StackCount = dstItem.ItemInfo.Stackable;
                    SendItemUpdate(dstItem);
                    if (dstBag > 0)
                        SendItemUpdate(Items[dstBag]);
                    SendItemUpdate(srcItem);
                    if (srcBag > 0)
                        SendItemUpdate(Items[srcBag]);
                    srcItem.Save(false);
                    dstItem.Save(false);
                    return true;
                }

                return false;
            }

            public void ItemSPLIT(byte srcBag, byte srcSlot, byte dstBag, byte dstSlot, int Count)
            {
                ItemObject dstItem = null;
                ItemObject srcItem;
                // DONE: Get source item
                if (srcBag == 0)
                {
                    if (!client.Character.Items.ContainsKey(srcSlot))
                    {
                        var EQUIP_ERR_ITEM_NOT_FOUND = new Packets.PacketClass(OPCODES.SMSG_INVENTORY_CHANGE_FAILURE);
                        try
                        {
                            EQUIP_ERR_ITEM_NOT_FOUND.AddInt8(InventoryChangeFailure.EQUIP_ERR_ITEM_NOT_FOUND);
                            EQUIP_ERR_ITEM_NOT_FOUND.AddUInt64(0UL);
                            EQUIP_ERR_ITEM_NOT_FOUND.AddUInt64(0UL);
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
                        var EQUIP_ERR_ITEM_NOT_FOUND = new Packets.PacketClass(OPCODES.SMSG_INVENTORY_CHANGE_FAILURE);
                        try
                        {
                            EQUIP_ERR_ITEM_NOT_FOUND.AddInt8(InventoryChangeFailure.EQUIP_ERR_ITEM_NOT_FOUND);
                            EQUIP_ERR_ITEM_NOT_FOUND.AddUInt64(0UL);
                            EQUIP_ERR_ITEM_NOT_FOUND.AddUInt64(0UL);
                            EQUIP_ERR_ITEM_NOT_FOUND.AddInt8(0);
                            client.Send(ref EQUIP_ERR_ITEM_NOT_FOUND);
                        }
                        finally
                        {
                            EQUIP_ERR_ITEM_NOT_FOUND.Dispose();
                        }

                        return;
                    }

                    srcItem = Items[srcBag].Items[srcSlot];
                }

                // DONE: Get destination item
                if (dstBag == 0)
                {
                    if (Items.ContainsKey(dstSlot))
                        dstItem = Items[dstSlot];
                }
                else if (Items[dstBag].Items.ContainsKey(dstSlot))
                    dstItem = Items[dstBag].Items[dstSlot];
                if (dstSlot == 255)
                {
                    var notHandledYet = new Packets.PacketClass(OPCODES.SMSG_INVENTORY_CHANGE_FAILURE);
                    try
                    {
                        notHandledYet.AddInt8(InventoryChangeFailure.EQUIP_ERR_COULDNT_SPLIT_ITEMS);
                        notHandledYet.AddUInt64(srcItem.GUID);
                        notHandledYet.AddUInt64(dstItem.GUID);
                        notHandledYet.AddInt8(0);
                        client.Send(ref notHandledYet);
                    }
                    finally
                    {
                        notHandledYet.Dispose();
                    }

                    return;
                }

                if (Count == srcItem.StackCount)
                {
                    ItemSWAP(srcBag, srcSlot, dstBag, dstSlot);
                    return;
                }

                if (Count > srcItem.StackCount)
                {
                    var EQUIP_ERR_TRIED_TO_SPLIT_MORE_THAN_COUNT = new Packets.PacketClass(OPCODES.SMSG_INVENTORY_CHANGE_FAILURE);
                    try
                    {
                        EQUIP_ERR_TRIED_TO_SPLIT_MORE_THAN_COUNT.AddInt8(InventoryChangeFailure.EQUIP_ERR_TRIED_TO_SPLIT_MORE_THAN_COUNT);
                        EQUIP_ERR_TRIED_TO_SPLIT_MORE_THAN_COUNT.AddUInt64(srcItem.GUID);
                        EQUIP_ERR_TRIED_TO_SPLIT_MORE_THAN_COUNT.AddUInt64(0UL);
                        EQUIP_ERR_TRIED_TO_SPLIT_MORE_THAN_COUNT.AddInt8(0);
                        client.Send(ref EQUIP_ERR_TRIED_TO_SPLIT_MORE_THAN_COUNT);
                    }
                    finally
                    {
                        EQUIP_ERR_TRIED_TO_SPLIT_MORE_THAN_COUNT.Dispose();
                    }

                    return;
                }

                // DONE: Create new item if needed
                if (dstItem is null)
                {
                    srcItem.StackCount -= Count;
                    var tmpItem = new ItemObject(srcItem.ItemEntry, GUID) { StackCount = Count };
                    dstItem = tmpItem;
                    tmpItem.Save();
                    ItemSETSLOT(ref tmpItem, dstBag, dstSlot);
                    var SMSG_UPDATE_OBJECT = new Packets.UpdatePacketClass();
                    var tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_ITEM);
                    try
                    {
                        tmpItem.FillAllUpdateFlags(ref tmpUpdate);
                        tmpUpdate.AddToPacket(SMSG_UPDATE_OBJECT, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, tmpItem);
                        client.Send(ref (Packets.PacketClass)SMSG_UPDATE_OBJECT);
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
                        Items[srcBag].Save(false);
                    }

                    if (dstBag != 0)
                    {
                        SendItemUpdate(Items[dstBag]);
                        Items[dstBag].Save(false);
                    }

                    srcItem.Save(false);
                    dstItem.Save(false);
                    return;
                }

                // DONE: Split
                if (srcItem.ItemEntry == dstItem.ItemEntry)
                {
                    if (dstItem.StackCount + Count <= dstItem.ItemInfo.Stackable)
                    {
                        srcItem.StackCount -= Count;
                        dstItem.StackCount += Count;
                        SendItemUpdate(srcItem);
                        SendItemUpdate(dstItem);
                        if (srcBag != 0)
                        {
                            SendItemUpdate(Items[srcBag]);
                            Items[srcBag].Save(false);
                        }

                        if (dstBag != 0)
                        {
                            SendItemUpdate(Items[dstBag]);
                            Items[dstBag].Save(false);
                        }

                        srcItem.Save(false);
                        dstItem.Save(false);
                        var EQUIP_ERR_OK = new Packets.PacketClass(OPCODES.SMSG_INVENTORY_CHANGE_FAILURE);
                        try
                        {
                            EQUIP_ERR_OK.AddInt8(InventoryChangeFailure.EQUIP_ERR_OK);
                            EQUIP_ERR_OK.AddUInt64(srcItem.GUID);
                            EQUIP_ERR_OK.AddUInt64(dstItem.GUID);
                            EQUIP_ERR_OK.AddInt8(0);
                            client.Send(ref EQUIP_ERR_OK);
                        }
                        finally
                        {
                            EQUIP_ERR_OK.Dispose();
                        }

                        return;
                    }
                }

                var response = new Packets.PacketClass(OPCODES.SMSG_INVENTORY_CHANGE_FAILURE);
                try
                {
                    response.AddInt8(InventoryChangeFailure.EQUIP_ERR_COULDNT_SPLIT_ITEMS);
                    response.AddUInt64(srcItem.GUID);
                    response.AddUInt64(dstItem.GUID);
                    response.AddInt8(0);
                    client.Send(ref response);
                }
                catch
                {
                    response.Dispose();
                }
            }

            public void ItemSWAP(byte srcBag, byte srcSlot, byte dstBag, byte dstSlot)
            {
                // DONE: Disable when dead, attackTarget<>0
                if (DEAD)
                {
                    var argobjCharacter = this;
                    WorldServiceLocator._WS_Items.SendInventoryChangeFailure(ref argobjCharacter, InventoryChangeFailure.EQUIP_ERR_YOU_ARE_DEAD, ItemGetGUID(srcBag, srcSlot), ItemGetGUID(dstBag, dstSlot));
                    return;
                }

                byte errCode = InventoryChangeFailure.EQUIP_ERR_ITEMS_CANT_BE_SWAPPED;

                // Disable moving the bag into same bag
                if (srcBag == 0 && srcSlot == dstBag && dstBag > 0 || dstBag == 0 && dstSlot == srcBag && srcBag > 0)
                {
                    var argobjCharacter1 = this;
                    WorldServiceLocator._WS_Items.SendInventoryChangeFailure(ref argobjCharacter1, errCode, Items[srcSlot].GUID, 0);
                    return;
                }

                try
                {
                    if (srcBag > 0 && dstBag > 0)
                    {
                        // DONE: Betwen Bags Moving
                        if (!Items[srcBag].Items.ContainsKey(srcSlot))
                        {
                            errCode = InventoryChangeFailure.EQUIP_ERR_SLOT_IS_EMPTY;
                        }
                        else
                        {
                            errCode = ItemCANEQUIP(Items[srcBag].Items[srcSlot], dstBag, dstSlot);
                            if (errCode == InventoryChangeFailure.EQUIP_ERR_OK && Items[dstBag].Items.ContainsKey(dstSlot))
                            {
                                errCode = ItemCANEQUIP(Items[dstBag].Items[dstSlot], srcBag, srcSlot);
                            }

                            // DONE: Moving item
                            if (errCode == InventoryChangeFailure.EQUIP_ERR_OK)
                            {
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
                                }
                                else if (!Items[srcBag].Items.ContainsKey(srcSlot))
                                {
                                    Items[srcBag].Items[srcSlot] = Items[dstBag].Items[dstSlot];
                                    Items[dstBag].Items.Remove(dstSlot);
                                }
                                else
                                {
                                    if (ItemSTACK(srcBag, srcSlot, dstBag, dstSlot))
                                        return;
                                    var tmp = Items[dstBag].Items[dstSlot];
                                    Items[dstBag].Items[dstSlot] = Items[srcBag].Items[srcSlot];
                                    Items[srcBag].Items[srcSlot] = tmp;
                                    tmp = null;
                                }

                                SendItemUpdate(Items[srcBag]);
                                if (dstBag != srcBag)
                                {
                                    SendItemUpdate(Items[dstBag]);
                                }

                                WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_inventory SET item_slot = {0}, item_bag = {1} WHERE item_guid = {2};", dstSlot, Items[dstBag].GUID, Items[dstBag].Items[dstSlot].GUID - WorldServiceLocator._Global_Constants.GUID_ITEM));
                                if (Items[srcBag].Items.ContainsKey(srcSlot))
                                    WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_inventory SET item_slot = {0}, item_bag = {1} WHERE item_guid = {2};", srcSlot, Items[srcBag].GUID, Items[srcBag].Items[srcSlot].GUID - WorldServiceLocator._Global_Constants.GUID_ITEM));
                            }
                        }
                    }
                    else if (srcBag > 0)
                    {
                        // DONE: from Bag to Inventory
                        if (!Items[srcBag].Items.ContainsKey(srcSlot))
                        {
                            errCode = InventoryChangeFailure.EQUIP_ERR_SLOT_IS_EMPTY;
                        }
                        else
                        {
                            errCode = ItemCANEQUIP(Items[srcBag].Items[srcSlot], dstBag, dstSlot);
                            if (errCode == InventoryChangeFailure.EQUIP_ERR_OK && Items.ContainsKey(dstSlot))
                            {
                                errCode = ItemCANEQUIP(Items[dstSlot], srcBag, srcSlot);
                            }

                            // DONE: Moving item
                            if (errCode == InventoryChangeFailure.EQUIP_ERR_OK)
                            {
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
                                        if (dstSlot < InventorySlots.INVENTORY_SLOT_BAG_END)
                                        {
                                            var tmp8 = Items;
                                            var argItem8 = tmp8[dstSlot];
                                            UpdateAddItemStats(ref argItem8, dstSlot);
                                            tmp8[dstSlot] = argItem8;
                                        }
                                    }
                                }
                                else if (!Items[srcBag].Items.ContainsKey(srcSlot))
                                {
                                    Items[srcBag].Items[srcSlot] = Items[dstSlot];
                                    Items.Remove(dstSlot);
                                    if (dstSlot < InventorySlots.INVENTORY_SLOT_BAG_END)
                                    {
                                        var tmp9 = Items[srcBag].Items;
                                        var argItem9 = tmp9[srcSlot];
                                        UpdateRemoveItemStats(ref argItem9, dstSlot);
                                        tmp9[srcSlot] = argItem9;
                                    }
                                }
                                else
                                {
                                    if (ItemSTACK(srcBag, srcSlot, dstBag, dstSlot))
                                        return;
                                    var tmp = Items[dstSlot];
                                    Items[dstSlot] = Items[srcBag].Items[srcSlot];
                                    Items[srcBag].Items[srcSlot] = tmp;
                                    if (dstSlot < InventorySlots.INVENTORY_SLOT_BAG_END)
                                    {
                                        var tmp10 = Items;
                                        var argItem10 = tmp10[dstSlot];
                                        UpdateAddItemStats(ref argItem10, dstSlot);
                                        tmp10[dstSlot] = argItem10;
                                        var tmp11 = Items[srcBag].Items;
                                        var argItem11 = tmp11[srcSlot];
                                        UpdateRemoveItemStats(ref argItem11, dstSlot);
                                        tmp11[srcSlot] = argItem11;
                                    }

                                    tmp = null;
                                }

                                SendItemAndCharacterUpdate(Items[srcBag]);
                                WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_inventory SET item_slot = {0}, item_bag = {1} WHERE item_guid = {2};", dstSlot, GUID, Items[dstSlot].GUID - WorldServiceLocator._Global_Constants.GUID_ITEM));
                                if (Items[srcBag].Items.ContainsKey(srcSlot))
                                    WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_inventory SET item_slot = {0}, item_bag = {1} WHERE item_guid = {2};", srcSlot, Items[srcBag].GUID, Items[srcBag].Items[srcSlot].GUID - WorldServiceLocator._Global_Constants.GUID_ITEM));
                            }
                        }
                    }
                    else if (dstBag > 0)
                    {
                        // DONE: from Inventory to Bag
                        if (!Items.ContainsKey(srcSlot))
                        {
                            errCode = InventoryChangeFailure.EQUIP_ERR_SLOT_IS_EMPTY;
                        }
                        else
                        {
                            errCode = ItemCANEQUIP(Items[srcSlot], dstBag, dstSlot);
                            if (errCode == InventoryChangeFailure.EQUIP_ERR_OK && Items[dstBag].Items.ContainsKey(dstSlot))
                            {
                                errCode = ItemCANEQUIP(Items[dstBag].Items[dstSlot], srcBag, srcSlot);
                            }

                            // DONE: Moving item
                            if (errCode == InventoryChangeFailure.EQUIP_ERR_OK)
                            {
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
                                        if (srcSlot < InventorySlots.INVENTORY_SLOT_BAG_END)
                                        {
                                            var tmp12 = Items[dstBag].Items;
                                            var argItem12 = tmp12[dstSlot];
                                            UpdateRemoveItemStats(ref argItem12, srcSlot);
                                            tmp12[dstSlot] = argItem12;
                                        }
                                    }
                                }
                                else if (!Items.ContainsKey(srcSlot))
                                {
                                    Items[srcSlot] = Items[dstBag].Items[dstSlot];
                                    Items[dstBag].Items.Remove(dstSlot);
                                    if (srcSlot < InventorySlots.INVENTORY_SLOT_BAG_END)
                                    {
                                        var tmp13 = Items;
                                        var argItem13 = tmp13[srcSlot];
                                        UpdateAddItemStats(ref argItem13, srcSlot);
                                        tmp13[srcSlot] = argItem13;
                                    }
                                }
                                else
                                {
                                    if (ItemSTACK(srcBag, srcSlot, dstBag, dstSlot))
                                        return;
                                    var tmp = Items[dstBag].Items[dstSlot];
                                    Items[dstBag].Items[dstSlot] = Items[srcSlot];
                                    Items[srcSlot] = tmp;
                                    if (srcSlot < InventorySlots.INVENTORY_SLOT_BAG_END)
                                    {
                                        var tmp14 = Items;
                                        var argItem14 = tmp14[srcSlot];
                                        UpdateAddItemStats(ref argItem14, srcSlot);
                                        tmp14[srcSlot] = argItem14;
                                        var tmp15 = Items[dstBag].Items;
                                        var argItem15 = tmp15[dstSlot];
                                        UpdateRemoveItemStats(ref argItem15, srcSlot);
                                        tmp15[dstSlot] = argItem15;
                                    }

                                    tmp = null;
                                }

                                SendItemAndCharacterUpdate(Items[dstBag]);
                                WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_inventory SET item_slot = {0}, item_bag = {1} WHERE item_guid = {2};", dstSlot, Items[dstBag].GUID, Items[dstBag].Items[dstSlot].GUID - WorldServiceLocator._Global_Constants.GUID_ITEM));
                                if (Items.ContainsKey(srcSlot))
                                    WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_inventory SET item_slot = {0}, item_bag = {1} WHERE item_guid = {2};", srcSlot, GUID, Items[srcSlot].GUID - WorldServiceLocator._Global_Constants.GUID_ITEM));
                            }
                        }
                    }
                    // DONE: Inventory Moving
                    else if (!Items.ContainsKey(srcSlot))
                    {
                        errCode = InventoryChangeFailure.EQUIP_ERR_SLOT_IS_EMPTY;
                    }
                    else
                    {
                        errCode = ItemCANEQUIP(Items[srcSlot], dstBag, dstSlot);
                        if (errCode == InventoryChangeFailure.EQUIP_ERR_OK && Items.ContainsKey(dstSlot))
                        {
                            errCode = ItemCANEQUIP(Items[dstSlot], srcBag, srcSlot);
                        }

                        // DONE: Moving item
                        if (errCode == InventoryChangeFailure.EQUIP_ERR_OK)
                        {
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
                                    if (dstSlot < InventorySlots.INVENTORY_SLOT_BAG_END)
                                    {
                                        var tmp = Items;
                                        var argItem = tmp[dstSlot];
                                        UpdateAddItemStats(ref argItem, dstSlot);
                                        tmp[dstSlot] = argItem;
                                    }

                                    if (srcSlot < InventorySlots.INVENTORY_SLOT_BAG_END)
                                    {
                                        var tmp1 = Items;
                                        var argItem1 = tmp1[dstSlot];
                                        UpdateRemoveItemStats(ref argItem1, srcSlot);
                                        tmp1[dstSlot] = argItem1;
                                    }
                                }
                            }
                            else if (!Items.ContainsKey(srcSlot))
                            {
                                Items[srcSlot] = Items[dstSlot];
                                Items.Remove(dstSlot);
                                if (dstSlot < InventorySlots.INVENTORY_SLOT_BAG_END)
                                {
                                    var tmp2 = Items;
                                    var argItem2 = tmp2[srcSlot];
                                    UpdateRemoveItemStats(ref argItem2, dstSlot);
                                    tmp2[srcSlot] = argItem2;
                                }

                                if (srcSlot < InventorySlots.INVENTORY_SLOT_BAG_END)
                                {
                                    var tmp3 = Items;
                                    var argItem3 = tmp3[srcSlot];
                                    UpdateAddItemStats(ref argItem3, srcSlot);
                                    tmp3[srcSlot] = argItem3;
                                }
                            }
                            else
                            {
                                if (ItemSTACK(srcBag, srcSlot, dstBag, dstSlot))
                                    return;
                                var tmp = Items[dstSlot];
                                Items[dstSlot] = Items[srcSlot];
                                Items[srcSlot] = tmp;
                                if (dstSlot < InventorySlots.INVENTORY_SLOT_BAG_END)
                                {
                                    var tmp4 = Items;
                                    var argItem4 = tmp4[dstSlot];
                                    UpdateAddItemStats(ref argItem4, dstSlot);
                                    tmp4[dstSlot] = argItem4;
                                    var tmp5 = Items;
                                    var argItem5 = tmp5[srcSlot];
                                    UpdateRemoveItemStats(ref argItem5, dstSlot);
                                    tmp5[srcSlot] = argItem5;
                                }

                                if (srcSlot < InventorySlots.INVENTORY_SLOT_BAG_END)
                                {
                                    var tmp6 = Items;
                                    var argItem6 = tmp6[srcSlot];
                                    UpdateAddItemStats(ref argItem6, srcSlot);
                                    tmp6[srcSlot] = argItem6;
                                    var tmp7 = Items;
                                    var argItem7 = tmp7[dstSlot];
                                    UpdateRemoveItemStats(ref argItem7, srcSlot);
                                    tmp7[dstSlot] = argItem7;
                                }

                                tmp = null;
                            }

                            SendItemAndCharacterUpdate(Items[dstSlot]);
                            WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_inventory SET item_slot = {0}, item_bag = {1} WHERE item_guid = {2};", dstSlot, GUID, Items[dstSlot].GUID - WorldServiceLocator._Global_Constants.GUID_ITEM));
                            if (Items.ContainsKey(srcSlot))
                                WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_inventory SET item_slot = {0}, item_bag = {1} WHERE item_guid = {2};", srcSlot, GUID, Items[srcSlot].GUID - WorldServiceLocator._Global_Constants.GUID_ITEM));
                        }
                    }
                }
                catch (Exception err)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] Unable to swap items. {2}{3}", client.IP, client.Port, Environment.NewLine, err.ToString());
                }
                finally
                {
                    if (errCode != InventoryChangeFailure.EQUIP_ERR_OK)
                    {
                        var argobjCharacter = this;
                        WorldServiceLocator._WS_Items.SendInventoryChangeFailure(ref argobjCharacter, errCode, ItemGetGUID(srcBag, srcSlot), ItemGetGUID(dstBag, dstSlot));
                    }
                }
            }

            public ItemObject ItemGET(byte srcBag, byte srcSlot)
            {
                if (srcBag == 0)
                {
                    if (Items.ContainsKey(srcSlot))
                        return Items[srcSlot];
                }
                else if (Items.ContainsKey(srcBag) && Items[srcBag].Items is object && Items[srcBag].Items.ContainsKey(srcSlot))
                    return Items[srcBag].Items[srcSlot];
                return null;
            }

            public ItemObject ItemGETByGUID(ulong GUID)
            {
                var srcBag = default(byte);
                byte srcSlot;
                srcSlot = client.Character.ItemGetSLOTBAG(GUID, ref srcBag);
                if (srcSlot == WorldServiceLocator._Global_Constants.ITEM_SLOT_NULL)
                    return null;
                return ItemGET(srcBag, srcSlot);
            }

            public ulong ItemGetGUID(byte srcBag, byte srcSlot)
            {
                if (srcBag == 0)
                {
                    if (Items.ContainsKey(srcSlot))
                        return Items[srcSlot].GUID;
                }
                else if (Items.ContainsKey(srcBag) && Items[srcBag].Items is object && Items[srcBag].Items.ContainsKey(srcSlot))
                    return Items[srcBag].Items[srcSlot].GUID;
                return 0UL;
            }

            public byte ItemGetSLOTBAG(ulong GUID, ref byte bag)
            {
                for (byte slot = EquipmentSlots.EQUIPMENT_SLOT_START, loopTo = InventoryPackSlots.INVENTORY_SLOT_ITEM_END - 1; slot <= loopTo; slot++)
                {
                    if (Items.ContainsKey(slot) && Items[slot].GUID == GUID)
                    {
                        bag = 0;
                        return slot;
                    }
                }

                for (byte slot = KeyRingSlots.KEYRING_SLOT_START, loopTo1 = KeyRingSlots.KEYRING_SLOT_END - 1; slot <= loopTo1; slot++)
                {
                    if (Items.ContainsKey(slot) && Items[slot].GUID == GUID)
                    {
                        bag = 0;
                        return slot;
                    }
                }

                var loopTo2 = InventorySlots.INVENTORY_SLOT_BAG_END - 1;
                for (bag = InventorySlots.INVENTORY_SLOT_BAG_START; bag <= loopTo2; bag++)
                {
                    if (Items.ContainsKey(bag))
                    {
                        foreach (KeyValuePair<byte, ItemObject> item in Items[bag].Items)
                        {
                            if (item.Value.GUID == GUID)
                                return item.Key;
                        }
                    }
                }

                bag = WorldServiceLocator._Global_Constants.ITEM_SLOT_NULL;
                return WorldServiceLocator._Global_Constants.ITEM_SLOT_NULL;
            }

            public void UpdateAddItemStats(ref ItemObject Item, byte slot)
            {
                // TODO: Fill in the other item stat types also
                for (byte i = 0; i <= 9; i++)
                {
                    switch (Item.ItemInfo.ItemBonusStatType[i])
                    {
                        case var @case when @case == ITEM_STAT_TYPE.HEALTH:
                            {
                                Life.Bonus += Item.ItemInfo.ItemBonusStatValue[i];
                                break;
                            }

                        case var case1 when case1 == ITEM_STAT_TYPE.AGILITY:
                            {
                                Agility.Base += Item.ItemInfo.ItemBonusStatValue[i];
                                Agility.PositiveBonus = (short)(Agility.PositiveBonus + Item.ItemInfo.ItemBonusStatValue[i]);
                                Resistances[DamageTypes.DMG_PHYSICAL].Base += Item.ItemInfo.ItemBonusStatValue[i] * 2;
                                break;
                            }

                        case var case2 when case2 == ITEM_STAT_TYPE.STRENGTH:
                            {
                                Strength.Base += Item.ItemInfo.ItemBonusStatValue[i];
                                Strength.PositiveBonus = (short)(Strength.PositiveBonus + Item.ItemInfo.ItemBonusStatValue[i]);
                                break;
                            }

                        case var case3 when case3 == ITEM_STAT_TYPE.INTELLECT:
                            {
                                Intellect.Base += Item.ItemInfo.ItemBonusStatValue[i];
                                Intellect.PositiveBonus = (short)(Intellect.PositiveBonus + Item.ItemInfo.ItemBonusStatValue[i]);
                                Life.Bonus += Item.ItemInfo.ItemBonusStatValue[i] * 15;
                                break;
                            }

                        case var case4 when case4 == ITEM_STAT_TYPE.SPIRIT:
                            {
                                Spirit.Base += Item.ItemInfo.ItemBonusStatValue[i];
                                Spirit.PositiveBonus = (short)(Spirit.PositiveBonus + Item.ItemInfo.ItemBonusStatValue[i]);
                                break;
                            }

                        case var case5 when case5 == ITEM_STAT_TYPE.STAMINA:
                            {
                                Stamina.Base += Item.ItemInfo.ItemBonusStatValue[i];
                                Stamina.PositiveBonus = (short)(Stamina.PositiveBonus + Item.ItemInfo.ItemBonusStatValue[i]);
                                Life.Bonus += Item.ItemInfo.ItemBonusStatValue[i] * 10;
                                break;
                            }

                        case var case6 when case6 == ITEM_STAT_TYPE.BLOCK:
                            {
                                combatBlockValue += Item.ItemInfo.ItemBonusStatValue[i];
                                break;
                            }
                    }
                }

                for (byte i = DamageTypes.DMG_PHYSICAL, loopTo = DamageTypes.DMG_ARCANE; i <= loopTo; i++)
                    Resistances[i].Base += Item.ItemInfo.Resistances[i];
                combatBlockValue += Item.ItemInfo.Block;
                if (Item.ItemInfo.Delay > 0)
                {
                    if (slot == EquipmentSlots.EQUIPMENT_SLOT_RANGED)
                    {
                        AttackTimeBase[2] = (short)Item.ItemInfo.Delay;
                    }
                    else if (slot == EquipmentSlots.EQUIPMENT_SLOT_MAINHAND)
                    {
                        AttackTimeBase[0] = (short)Item.ItemInfo.Delay;
                    }
                    else if (slot == EquipmentSlots.EQUIPMENT_SLOT_OFFHAND)
                    {
                        AttackTimeBase[1] = (short)Item.ItemInfo.Delay;
                    }
                }

                // DONE: Add the equip spells to the character
                for (byte i = 0; i <= 4; i++)
                {
                    if (Item.ItemInfo.Spells[i].SpellID > 0)
                    {
                        if (WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(Item.ItemInfo.Spells[i].SpellID))
                        {
                            var SpellInfo = WorldServiceLocator._WS_Spells.SPELLs[Item.ItemInfo.Spells[i].SpellID];
                            if (Item.ItemInfo.Spells[i].SpellTrigger == ITEM_SPELLTRIGGER_TYPE.ON_EQUIP)
                            {
                                ApplySpell(Item.ItemInfo.Spells[i].SpellID);
                            }
                            else if (Item.ItemInfo.Spells[i].SpellTrigger == ITEM_SPELLTRIGGER_TYPE.USE)
                            {
                                // DONE: Show item cooldown when equipped
                                var cooldown = new Packets.PacketClass(OPCODES.SMSG_ITEM_COOLDOWN);
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
                    }
                }

                // DONE: Bind item to player
                if (Item.ItemInfo.Bonding == ITEM_BONDING_TYPE.BIND_WHEN_EQUIPED && !Item.IsSoulBound)
                {
                    WS_Network.ClientClass argclient = null;
                    Item.SoulbindItem(client: ref argclient);
                }

                // DONE: Cancel any spells that are being casted while equipping an item
                FinishAllSpells();
                foreach (KeyValuePair<byte, WS_Items.TEnchantmentInfo> Enchant in Item.Enchantments)
                {
                    var argobjCharacter = this;
                    Item.AddEnchantBonus(Enchant.Key, ref argobjCharacter);
                }

                var argobjCharacter1 = this;
                WorldServiceLocator._WS_Combat.CalculateMinMaxDamage(ref argobjCharacter1, WeaponAttackType.BASE_ATTACK);
                var argobjCharacter2 = this;
                WorldServiceLocator._WS_Combat.CalculateMinMaxDamage(ref argobjCharacter2, WeaponAttackType.OFF_ATTACK);
                var argobjCharacter3 = this;
                WorldServiceLocator._WS_Combat.CalculateMinMaxDamage(ref argobjCharacter3, WeaponAttackType.RANGED_ATTACK);
                if (ManaType == ManaTypes.TYPE_MANA || Classe == Classes.CLASS_DRUID)
                    UpdateManaRegen();
                FillStatsUpdateFlags();
            }

            public void UpdateRemoveItemStats(ref ItemObject Item, byte slot)
            {
                // TODO: Add the other item stat types here also
                for (byte i = 0; i <= 9; i++)
                {
                    switch (Item.ItemInfo.ItemBonusStatType[i])
                    {
                        case var @case when @case == ITEM_STAT_TYPE.HEALTH:
                            {
                                Life.Bonus -= Item.ItemInfo.ItemBonusStatValue[i];
                                break;
                            }

                        case var case1 when case1 == ITEM_STAT_TYPE.AGILITY:
                            {
                                Agility.Base -= Item.ItemInfo.ItemBonusStatValue[i];
                                Agility.PositiveBonus = (short)(Agility.PositiveBonus - Item.ItemInfo.ItemBonusStatValue[i]);
                                Resistances[DamageTypes.DMG_PHYSICAL].Base -= Item.ItemInfo.ItemBonusStatValue[i] * 2;
                                break;
                            }

                        case var case2 when case2 == ITEM_STAT_TYPE.STRENGTH:
                            {
                                Strength.Base -= Item.ItemInfo.ItemBonusStatValue[i];
                                Strength.PositiveBonus = (short)(Strength.PositiveBonus - Item.ItemInfo.ItemBonusStatValue[i]);
                                break;
                            }

                        case var case3 when case3 == ITEM_STAT_TYPE.INTELLECT:
                            {
                                Intellect.Base -= Item.ItemInfo.ItemBonusStatValue[i];
                                Intellect.PositiveBonus = (short)(Intellect.PositiveBonus - Item.ItemInfo.ItemBonusStatValue[i]);
                                Mana.Bonus -= Item.ItemInfo.ItemBonusStatValue[i] * 15;
                                break;
                            }

                        case var case4 when case4 == ITEM_STAT_TYPE.SPIRIT:
                            {
                                Spirit.Base -= Item.ItemInfo.ItemBonusStatValue[i];
                                Spirit.PositiveBonus = (short)(Spirit.PositiveBonus - Item.ItemInfo.ItemBonusStatValue[i]);
                                break;
                            }

                        case var case5 when case5 == ITEM_STAT_TYPE.STAMINA:
                            {
                                Stamina.Base -= Item.ItemInfo.ItemBonusStatValue[i];
                                Stamina.PositiveBonus = (short)(Stamina.PositiveBonus - Item.ItemInfo.ItemBonusStatValue[i]);
                                Life.Bonus -= Item.ItemInfo.ItemBonusStatValue[i] * 10;
                                break;
                            }

                        case var case6 when case6 == ITEM_STAT_TYPE.BLOCK:
                            {
                                combatBlockValue -= Item.ItemInfo.ItemBonusStatValue[i];
                                break;
                            }
                    }
                }

                for (byte i = DamageTypes.DMG_PHYSICAL, loopTo = DamageTypes.DMG_ARCANE; i <= loopTo; i++)
                    Resistances[i].Base -= Item.ItemInfo.Resistances[i];
                combatBlockValue -= Item.ItemInfo.Block;
                if (Item.ItemInfo.Delay > 0)
                {
                    if (slot == EquipmentSlots.EQUIPMENT_SLOT_RANGED)
                    {
                        AttackTimeBase[2] = 0;
                    }
                    else if (slot == EquipmentSlots.EQUIPMENT_SLOT_MAINHAND)
                    {
                        if (Classe == Classes.CLASS_ROGUE)
                            AttackTimeBase[0] = 1900;
                        else
                            AttackTimeBase[0] = 2000;
                    }
                    else if (slot == EquipmentSlots.EQUIPMENT_SLOT_OFFHAND)
                    {
                        AttackTimeBase[1] = 0;
                    }
                }

                // DONE: Remove the equip spells to the character
                for (byte i = 0; i <= 4; i++)
                {
                    if (Item.ItemInfo.Spells[i].SpellID > 0)
                    {
                        if (WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(Item.ItemInfo.Spells[i].SpellID))
                        {
                            var SpellInfo = WorldServiceLocator._WS_Spells.SPELLs[Item.ItemInfo.Spells[i].SpellID];
                            if (Item.ItemInfo.Spells[i].SpellTrigger == ITEM_SPELLTRIGGER_TYPE.ON_EQUIP)
                            {
                                RemoveAuraBySpell(Item.ItemInfo.Spells[i].SpellID);
                            }
                        }
                    }
                }

                foreach (KeyValuePair<byte, WS_Items.TEnchantmentInfo> Enchant in Item.Enchantments)
                    Item.RemoveEnchantBonus(Enchant.Key);
                var argobjCharacter = this;
                WorldServiceLocator._WS_Combat.CalculateMinMaxDamage(ref argobjCharacter, WeaponAttackType.BASE_ATTACK);
                var argobjCharacter1 = this;
                WorldServiceLocator._WS_Combat.CalculateMinMaxDamage(ref argobjCharacter1, WeaponAttackType.OFF_ATTACK);
                var argobjCharacter2 = this;
                WorldServiceLocator._WS_Combat.CalculateMinMaxDamage(ref argobjCharacter2, WeaponAttackType.RANGED_ATTACK);
                if (ManaType == ManaTypes.TYPE_MANA || Classe == Classes.CLASS_DRUID)
                    UpdateManaRegen();
                FillStatsUpdateFlags();
            }

            // Creature Interactions
            public void SendGossip(ulong cGUID, int cTextID, [Optional, DefaultParameterValue(null)] ref GossipMenu Menu, [Optional, DefaultParameterValue(null)] ref QuestMenu qMenu)
            {
                var SMSG_GOSSIP_MESSAGE = new Packets.PacketClass(OPCODES.SMSG_GOSSIP_MESSAGE);
                try
                {
                    SMSG_GOSSIP_MESSAGE.AddUInt64(cGUID);
                    SMSG_GOSSIP_MESSAGE.AddInt32(cTextID);
                    if (Menu is null)
                    {
                        SMSG_GOSSIP_MESSAGE.AddInt32(0);
                    }
                    else
                    {
                        SMSG_GOSSIP_MESSAGE.AddInt32(Menu.Menus.Count);
                        int index = 0;
                        while (index < Menu.Menus.Count)
                        {
                            SMSG_GOSSIP_MESSAGE.AddInt32(index);
                            SMSG_GOSSIP_MESSAGE.AddInt8(Conversions.ToByte(Menu.Icons[index]));
                            SMSG_GOSSIP_MESSAGE.AddInt8(Conversions.ToByte(Menu.Coded[index]));
                            SMSG_GOSSIP_MESSAGE.AddString(Conversions.ToString(Menu.Menus[index]));
                            index += 1;
                        }
                    }

                    if (qMenu is null)
                    {
                        SMSG_GOSSIP_MESSAGE.AddInt32(0);
                    }
                    else
                    {
                        SMSG_GOSSIP_MESSAGE.AddInt32(qMenu.Names.Count);
                        int index = 0;
                        while (index < qMenu.Names.Count)
                        {
                            SMSG_GOSSIP_MESSAGE.AddInt32(Conversions.ToInteger(qMenu.IDs[index]));
                            SMSG_GOSSIP_MESSAGE.AddInt32(Conversions.ToInteger(qMenu.Icons[index]));
                            SMSG_GOSSIP_MESSAGE.AddInt32(Conversions.ToInteger(qMenu.Levels[index]));
                            SMSG_GOSSIP_MESSAGE.AddString(Conversions.ToString(qMenu.Names[index]));
                            index += 1;
                        }
                    }

                    client.Send(ref SMSG_GOSSIP_MESSAGE);
                }
                finally
                {
                    SMSG_GOSSIP_MESSAGE.Dispose();
                }
            }

            public void SendGossipComplete()
            {
                var SMSG_GOSSIP_COMPLETE = new Packets.PacketClass(OPCODES.SMSG_GOSSIP_COMPLETE);
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
                var SMSG_GOSSIP_POI = new Packets.PacketClass(OPCODES.SMSG_GOSSIP_POI);
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
                if (WorldServiceLocator._WS_Creatures.NPCTexts.ContainsKey(TextID) == false)
                {
                    var tmpText = new WS_Creatures.NPCText(TextID);
                    // The New does a an add to the .Containskey collection above
                }

                // DONE: Load TextID
                var response = new Packets.PacketClass(OPCODES.SMSG_NPC_TEXT_UPDATE);
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
                        for (int i = 0; i <= 7; i++)
                        {
                            response.AddSingle(WorldServiceLocator._WS_Creatures.NPCTexts[TextID].Probability[i]);     // Probability
                            response.AddString(WorldServiceLocator._WS_Creatures.NPCTexts[TextID].TextLine1[i]);       // Text1
                            if (string.IsNullOrEmpty(WorldServiceLocator._WS_Creatures.NPCTexts[TextID].TextLine2[i]))
                            {
                                response.AddString(WorldServiceLocator._WS_Creatures.NPCTexts[TextID].TextLine1[i]);   // Text2
                            }
                            else
                            {
                                response.AddString(WorldServiceLocator._WS_Creatures.NPCTexts[TextID].TextLine2[i]);
                            }   // Text2

                            response.AddInt32(WorldServiceLocator._WS_Creatures.NPCTexts[TextID].Language[i]);         // Language
                            response.AddInt32(WorldServiceLocator._WS_Creatures.NPCTexts[TextID].EmoteDelay1[i]);      // Emote1.Delay
                            response.AddInt32(WorldServiceLocator._WS_Creatures.NPCTexts[TextID].Emote1[i]);           // Emote1.Emote
                            response.AddInt32(WorldServiceLocator._WS_Creatures.NPCTexts[TextID].EmoteDelay2[i]);      // Emote2.Delay
                            response.AddInt32(WorldServiceLocator._WS_Creatures.NPCTexts[TextID].Emote2[i]);           // Emote2.Emote
                            response.AddInt32(WorldServiceLocator._WS_Creatures.NPCTexts[TextID].EmoteDelay3[i]);      // Emote3.Delay
                            response.AddInt32(WorldServiceLocator._WS_Creatures.NPCTexts[TextID].Emote3[i]);           // Emote3.Emote
                        }
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
                bindpoint_map_id = (int)MapID;
                bindpoint_zone_id = ZoneID;
                SaveCharacter();
                var SMSG_BINDPOINTUPDATE = new Packets.PacketClass(OPCODES.SMSG_BINDPOINTUPDATE);
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

                var SMSG_PLAYERBOUND = new Packets.PacketClass(OPCODES.SMSG_PLAYERBOUND);
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

            // Character Movement
            public void Teleport(float posX, float posY, float posZ, float ori, int map)
            {
                if (MapID != map)
                {
                    Transfer(posX, posY, posZ, ori, map);
                    return;
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "World: Player Teleport: X[{0}], Y[{1}], Z[{2}], O[{3}]", posX, posY, posZ, ori);
                charMovementFlags = 0;
                var packet = new Packets.PacketClass(OPCODES.MSG_MOVE_TELEPORT_ACK);
                try
                {
                    packet.AddPackGUID(GUID);
                    packet.AddInt32(0);              // Counter
                    packet.AddInt32(0);              // Movement flags
                    packet.AddInt32(WorldServiceLocator._WS_Network.MsTime());
                    packet.AddSingle(posX);
                    packet.AddSingle(posY);
                    packet.AddSingle(posZ);
                    packet.AddSingle(ori);
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
                orientation = ori;
                var argCharacter = this;
                WorldServiceLocator._WS_CharMovement.MoveCell(ref argCharacter);
                var argCharacter1 = this;
                WorldServiceLocator._WS_CharMovement.UpdateCell(ref argCharacter1);
                client.Character.ZoneID = WorldServiceLocator._WS_Maps.AreaTable[WorldServiceLocator._WS_Maps.GetAreaFlag(posX, posY, (int)client.Character.MapID)].Zone;
            }

            public void Transfer(float posX, float posY, float posZ, float ori, int map)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "World: Player Transfer: X[{0}], Y[{1}], Z[{2}], O[{3}], MAP[{4}]", posX, posY, posZ, ori, map);
                var p = new Packets.PacketClass(OPCODES.SMSG_TRANSFER_PENDING);
                try
                {
                    p.AddInt32(map);
                    if (OnTransport is object)
                    {
                        p.AddInt32(OnTransport.ID);      // Only if on transport
                        p.AddInt32((int)OnTransport.MapID);   // Only if on transport
                    }

                    client.Send(ref p);
                }
                finally
                {
                    p.Dispose();
                }
                // Actions Here
                var argCharacter = this;
                WorldServiceLocator._WS_CharMovement.RemoveFromWorld(ref argCharacter);
                if (OnTransport is object && OnTransport is WS_Transports.TransportObject)
                {
                    WS_Base.BaseUnit argUnit = this;
                    ((WS_Transports.TransportObject)OnTransport).RemovePassenger(ref argUnit);
                }

                client.Character.charMovementFlags = 0;
                client.Character.positionX = posX;
                client.Character.positionY = posY;
                client.Character.positionZ = posZ;
                client.Character.orientation = ori;
                client.Character.MapID = (uint)map;
                client.Character.Save();

                // Do global transfer
                WorldServiceLocator._WorldServer.ClsWorldServer.ClientTransfer(client.Index, posX, posY, posZ, ori, map);
            }

            public void ZoneCheck()
            {
                int ZoneFlag = WorldServiceLocator._WS_Maps.GetAreaFlag(positionX, positionY, (int)MapID);
                if (WorldServiceLocator._WS_Maps.AreaTable.ContainsKey(ZoneFlag) == false)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Zone Flag {0} does not exist.", ZoneFlag);
                    return;
                }

                AreaID = WorldServiceLocator._WS_Maps.AreaTable[ZoneFlag].ID;
                if (WorldServiceLocator._WS_Maps.AreaTable[ZoneFlag].Zone == 0)
                {
                    ZoneID = WorldServiceLocator._WS_Maps.AreaTable[ZoneFlag].ID;
                }
                else
                {
                    ZoneID = WorldServiceLocator._WS_Maps.AreaTable[ZoneFlag].Zone;
                }

                GroupUpdateFlag = GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_ZONE;

                // DONE: Set rested in citys
                if (WorldServiceLocator._WS_Maps.AreaTable[ZoneFlag].IsCity())
                {
                    if ((cPlayerFlags & PlayerFlags.PLAYER_FLAGS_RESTING) == 0 && Level < WorldServiceLocator._WS_Player_Initializator.DEFAULT_MAX_LEVEL)
                    {
                        cPlayerFlags = cPlayerFlags | PlayerFlags.PLAYER_FLAGS_RESTING;
                        SetUpdateFlag(EPlayerFields.PLAYER_FLAGS, cPlayerFlags);
                        SendCharacterUpdate();
                    }
                }
                else if (cPlayerFlags & PlayerFlags.PLAYER_FLAGS_RESTING)
                {
                    cPlayerFlags = cPlayerFlags & !PlayerFlags.PLAYER_FLAGS_RESTING;
                    SetUpdateFlag(EPlayerFields.PLAYER_FLAGS, cPlayerFlags);
                    SendCharacterUpdate();
                }
                // DONE: Sanctuary turns players into blue and not attackable
                if (WorldServiceLocator._WS_Maps.AreaTable[ZoneFlag].IsSanctuary())
                {
                    if ((cUnitFlags & UnitFlags.UNIT_FLAG_NON_PVP_PLAYER) < UnitFlags.UNIT_FLAG_NON_PVP_PLAYER)
                    {
                        cUnitFlags = cUnitFlags | UnitFlags.UNIT_FLAG_NON_PVP_PLAYER;
                        SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, cUnitFlags);
                        SendCharacterUpdate();
                    }
                }
                else
                {
                    if ((cUnitFlags & UnitFlags.UNIT_FLAG_NON_PVP_PLAYER) == UnitFlags.UNIT_FLAG_NON_PVP_PLAYER)
                    {
                        cUnitFlags = cUnitFlags & !UnitFlags.UNIT_FLAG_NON_PVP_PLAYER;
                        cUnitFlags = cUnitFlags | UnitFlags.UNIT_FLAG_ATTACKABLE; // To still be able to attack neutral
                        SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, cUnitFlags);
                        SendCharacterUpdate();
                    }

                    // DONE: Activate Arena PvP (Can attack people from your own faction)
                    if (WorldServiceLocator._WS_Maps.AreaTable[ZoneFlag].IsArena())
                    {
                        if ((cPlayerFlags & PlayerFlags.PLAYER_FLAGS_PVP_TIMER) == 0)
                        {
                            cPlayerFlags = cPlayerFlags | PlayerFlags.PLAYER_FLAGS_PVP_TIMER;
                            SetUpdateFlag(EPlayerFields.PLAYER_FLAGS, cPlayerFlags);
                            SendCharacterUpdate();
                            GroupUpdateFlag = GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_STATUS;
                        }
                    }
                    else
                    {
                        // DONE: Activate PvP
                        // TODO: Only for PvP realms
                        var argobjCharacter = this;
                        if (WorldServiceLocator._WS_Maps.AreaTable[ZoneFlag].IsMyLand(ref argobjCharacter) == false)
                        {
                            if ((cUnitFlags & UnitFlags.UNIT_FLAG_PVP) == 0)
                            {
                                cUnitFlags = cUnitFlags | UnitFlags.UNIT_FLAG_PVP;
                                SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, cUnitFlags);
                                SendCharacterUpdate();
                                GroupUpdateFlag = GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_STATUS;
                            }
                        }
                        // TODO: It takes 5 minutes before the PVP flag wears off
                        else if (cUnitFlags & UnitFlags.UNIT_FLAG_PVP)
                        {
                            cUnitFlags = cUnitFlags & !UnitFlags.UNIT_FLAG_PVP;
                            SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, cUnitFlags);
                            SendCharacterUpdate();
                            GroupUpdateFlag = GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_STATUS;
                        }
                    }
                }
            }

            public void ZoneCheckInstance()
            {
                int ZoneFlag = WorldServiceLocator._WS_Maps.GetAreaFlag(positionX, positionY, (int)MapID);
                if (WorldServiceLocator._WS_Maps.AreaTable.ContainsKey(ZoneFlag) == false)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Zone Flag {0} does not exist.", ZoneFlag);
                    return;
                }

                AreaID = WorldServiceLocator._WS_Maps.AreaTable[ZoneFlag].ID;
                if (WorldServiceLocator._WS_Maps.AreaTable[ZoneFlag].Zone == 0)
                {
                    if (ZoneID == 0)
                    {
                        ZoneID = WorldServiceLocator._WS_Maps.AreaTable[ZoneFlag].ID;
                    }
                    else
                    {
                        ZoneID = WorldServiceLocator._WS_Maps.AreaTable[ZoneFlag].Zone;
                    }
                }

                GroupUpdateFlag = GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_ZONE;

                // DONE: Set rested in citys
                if (WorldServiceLocator._WS_Maps.AreaTable[ZoneFlag].IsCity())
                {
                    if ((cPlayerFlags & PlayerFlags.PLAYER_FLAGS_RESTING) == 0 && Level < WorldServiceLocator._WS_Player_Initializator.DEFAULT_MAX_LEVEL)
                    {
                        cPlayerFlags = cPlayerFlags | PlayerFlags.PLAYER_FLAGS_RESTING;
                        SetUpdateFlag(EPlayerFields.PLAYER_FLAGS, cPlayerFlags);
                        SendCharacterUpdate();
                    }
                }
                else if (cPlayerFlags & PlayerFlags.PLAYER_FLAGS_RESTING)
                {
                    cPlayerFlags = cPlayerFlags & !PlayerFlags.PLAYER_FLAGS_RESTING;
                    SetUpdateFlag(EPlayerFields.PLAYER_FLAGS, cPlayerFlags);
                    SendCharacterUpdate();
                }
                // DONE: Sanctuary turns players into blue and not attackable
                if (WorldServiceLocator._WS_Maps.AreaTable[ZoneFlag].IsSanctuary())
                {
                    if ((cUnitFlags & UnitFlags.UNIT_FLAG_NON_PVP_PLAYER) < UnitFlags.UNIT_FLAG_NON_PVP_PLAYER)
                    {
                        cUnitFlags = cUnitFlags | UnitFlags.UNIT_FLAG_NON_PVP_PLAYER;
                        SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, cUnitFlags);
                        SendCharacterUpdate();
                    }
                }
                else
                {
                    if ((cUnitFlags & UnitFlags.UNIT_FLAG_NON_PVP_PLAYER) == UnitFlags.UNIT_FLAG_NON_PVP_PLAYER)
                    {
                        cUnitFlags = cUnitFlags & !UnitFlags.UNIT_FLAG_NON_PVP_PLAYER;
                        cUnitFlags = cUnitFlags | UnitFlags.UNIT_FLAG_ATTACKABLE; // To still be able to attack neutral
                        SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, cUnitFlags);
                        SendCharacterUpdate();
                    }

                    // DONE: Activate Arena PvP (Can attack people from your own faction)
                    if (WorldServiceLocator._WS_Maps.AreaTable[ZoneFlag].IsArena())
                    {
                        if ((cPlayerFlags & PlayerFlags.PLAYER_FLAGS_PVP_TIMER) == 0)
                        {
                            cPlayerFlags = cPlayerFlags | PlayerFlags.PLAYER_FLAGS_PVP_TIMER;
                            SetUpdateFlag(EPlayerFields.PLAYER_FLAGS, cPlayerFlags);
                            SendCharacterUpdate();
                            GroupUpdateFlag = GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_STATUS;
                        }
                    }
                    else
                    {
                        // DONE: Activate PvP
                        // TODO: Only for PvP realms
                        var argobjCharacter = this;
                        if (WorldServiceLocator._WS_Maps.AreaTable[ZoneFlag].IsMyLand(ref argobjCharacter) == false)
                        {
                            if ((cUnitFlags & UnitFlags.UNIT_FLAG_PVP) == 0)
                            {
                                cUnitFlags = cUnitFlags | UnitFlags.UNIT_FLAG_PVP;
                                SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, cUnitFlags);
                                SendCharacterUpdate();
                                GroupUpdateFlag = GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_STATUS;
                            }
                        }
                        // TODO: It takes 5 minutes before the PVP flag wears off
                        else if (cUnitFlags & UnitFlags.UNIT_FLAG_PVP)
                        {
                            cUnitFlags = cUnitFlags & !UnitFlags.UNIT_FLAG_PVP;
                            SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, cUnitFlags);
                            SendCharacterUpdate();
                            GroupUpdateFlag = GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_STATUS;
                        }
                    }
                }
            }

            // Public Sub ChangeSpeed(ByVal Type As ChangeSpeedType, ByVal NewSpeed As Single)
            // Dim packet As PacketClass = Nothing
            // Try
            // Select Case Type
            // Case ChangeSpeedType.RUN
            // RunSpeed = NewSpeed
            // packet = New PacketClass(OPCODES.MSG_MOVE_SET_RUN_SPEED)
            // Case ChangeSpeedType.RUNBACK
            // RunBackSpeed = NewSpeed
            // packet = New PacketClass(OPCODES.MSG_MOVE_SET_RUN_BACK_SPEED)
            // Case ChangeSpeedType.SWIM
            // SwimSpeed = NewSpeed
            // packet = New PacketClass(OPCODES.MSG_MOVE_SET_SWIM_SPEED)
            // Case ChangeSpeedType.SWIMBACK
            // SwimSpeed = NewSpeed
            // packet = New PacketClass(OPCODES.MSG_MOVE_SET_SWIM_BACK_SPEED)
            // Case ChangeSpeedType.TURNRATE
            // TurnRate = NewSpeed
            // packet = New PacketClass(OPCODES.MSG_MOVE_SET_TURN_RATE)
            // End Select

            // 'DONE: Send to nearby players
            // packet.AddPackGUID(Client.Character.GUID)
            // packet.AddInt32(0) 'Movement flags
            // packet.AddInt32(msTime)
            // packet.AddSingle(positionX)
            // packet.AddSingle(positionY)
            // packet.AddSingle(positionZ)
            // packet.AddSingle(orientation)
            // packet.AddInt32(0) 'Unk flag
            // packet.AddSingle(NewSpeed)
            // client.Character.SendToNearPlayers(packet)
            // Finally
            // packet.Dispose()
            // End Try
            // End Sub

            public void ChangeSpeedForced(ChangeSpeedType Type, float NewSpeed)
            {
                antiHackSpeedChanged_ += 1;
                Packets.PacketClass packet = null;
                try
                {
                    switch (Type)
                    {
                        case var @case when @case == ChangeSpeedType.RUN:
                            {
                                packet = new Packets.PacketClass(OPCODES.SMSG_FORCE_RUN_SPEED_CHANGE);
                                RunSpeed = NewSpeed;
                                break;
                            }

                        case var case1 when case1 == ChangeSpeedType.RUNBACK:
                            {
                                packet = new Packets.PacketClass(OPCODES.SMSG_FORCE_RUN_BACK_SPEED_CHANGE);
                                RunBackSpeed = NewSpeed;
                                break;
                            }

                        case var case2 when case2 == ChangeSpeedType.SWIM:
                            {
                                packet = new Packets.PacketClass(OPCODES.SMSG_FORCE_SWIM_SPEED_CHANGE);
                                SwimSpeed = NewSpeed;
                                break;
                            }

                        case var case3 when case3 == ChangeSpeedType.SWIMBACK:
                            {
                                packet = new Packets.PacketClass(OPCODES.SMSG_FORCE_SWIM_BACK_SPEED_CHANGE);
                                SwimBackSpeed = NewSpeed;
                                break;
                            }

                        case var case4 when case4 == ChangeSpeedType.TURNRATE:
                            {
                                packet = new Packets.PacketClass(OPCODES.SMSG_FORCE_TURN_RATE_CHANGE);
                                TurnRate = NewSpeed;
                                break;
                            }

                        default:
                            {
                                return;
                            }
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

            public void SetWaterWalk()
            {
                var SMSG_MOVE_WATER_WALK = new Packets.PacketClass(OPCODES.SMSG_MOVE_WATER_WALK);
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

            public void SetLandWalk()
            {
                var SMSG_MOVE_LAND_WALK = new Packets.PacketClass(OPCODES.SMSG_MOVE_LAND_WALK);
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
                var SMSG_FORCE_MOVE_ROOT = new Packets.PacketClass(OPCODES.SMSG_FORCE_MOVE_ROOT);
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
                var SMSG_FORCE_MOVE_UNROOT = new Packets.PacketClass(OPCODES.SMSG_FORCE_MOVE_UNROOT);
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
                var SMSG_START_MIRROR_TIMER = new Packets.PacketClass(OPCODES.SMSG_START_MIRROR_TIMER);
                try
                {
                    SMSG_START_MIRROR_TIMER.AddInt32(Type);
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
                // TYPE: 0 = fartigua 1 = breath 2 = fire
                var SMSG_START_MIRROR_TIMER = new Packets.PacketClass(OPCODES.SMSG_START_MIRROR_TIMER);
                try
                {
                    SMSG_START_MIRROR_TIMER.AddInt32(Type);
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
                var SMSG_STOP_MIRROR_TIMER = new Packets.PacketClass(OPCODES.SMSG_STOP_MIRROR_TIMER);
                try
                {
                    SMSG_STOP_MIRROR_TIMER.AddInt32(Type);
                    client.Send(ref SMSG_STOP_MIRROR_TIMER);
                }
                finally
                {
                    SMSG_STOP_MIRROR_TIMER.Dispose();
                }
                // If Type = 1 And (Not (underWaterTimer Is Nothing)) Then
                // underWaterTimer.Dispose()
                // underWaterTimer = Nothing
                // End If
            }

            public void HandleDrowning(object state)
            {
                try
                {
                    if (positionZ > WorldServiceLocator._WS_Maps.GetWaterLevel(positionX, positionY, (int)MapID) - 1.6d)
                    {
                        underWaterTimer.DrowningValue += 2000;
                        if (underWaterTimer.DrowningValue > 70000)
                            underWaterTimer.DrowningValue = 70000;
                        ModifyMirrorTimer(MirrorTimer.DROWNING, 70000, underWaterTimer.DrowningValue, 2);
                    }
                    else
                    {
                        underWaterTimer.DrowningValue -= 1000;
                        if (underWaterTimer.DrowningValue < 0)
                        {
                            underWaterTimer.DrowningValue = 0;
                            LogEnvironmentalDamage(EnvironmentalDamage.DAMAGE_DROWNING, Conversion.Fix(0.1f * (float)Life.Maximum * (float)underWaterTimer.DrowningDamage));
                            WS_Base.BaseUnit argAttacker = null;
                            DealDamage((int)Conversion.Fix(0.1f * Life.Maximum * underWaterTimer.DrowningDamage), Attacker: ref argAttacker);
                            underWaterTimer.DrowningDamage = (byte)(underWaterTimer.DrowningDamage * 2);
                            if (DEAD)
                            {
                                underWaterTimer.Dispose();
                                underWaterTimer = null;
                            }
                        }

                        ModifyMirrorTimer(MirrorTimer.DROWNING, 70000, underWaterTimer.DrowningValue, -1);
                    }
                }
                catch (Exception e)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error at HandleDrowning():", e.ToString());
                    if (underWaterTimer is object)
                        underWaterTimer.Dispose();
                    underWaterTimer = null;
                }
            }

            // Reputation
            public byte WatchedFactionIndex = 0xFF;
            public WS_PlayerHelper.TReputation[] Reputation = new WS_PlayerHelper.TReputation[64];

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
                if (WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo.ContainsKey(FactionID) == false || WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo.ContainsKey(Faction) == false)
                    return TReaction.NEUTRAL;

                // DONE: Neutral to everyone
                if ((WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].enemyMask == 0L && WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].friendMask == 0L && WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].enemyFaction1 == 0) & WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].enemyFaction2 == 0 && WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].enemyFaction3 == 0 && WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].enemyFaction4 == 0)
                    return TReaction.NEUTRAL;

                // DONE: Neutral to your faction
                if ((WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].enemyMask == 0L && WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].friendMask == 0L && WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].enemyFaction1 != Faction) & WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].enemyFaction2 != Faction && WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].enemyFaction3 != Faction && WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].enemyFaction4 != Faction)
                    return TReaction.NEUTRAL;

                // DONE: Hostile to any players
                if (WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].enemyMask & FactionMasks.FACTION_MASK_PLAYER)
                    return TReaction.HOSTILE;

                // DONE: Friendly to your faction
                if (WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].friendFaction1 == Faction || WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].friendFaction2 == Faction || WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].friendFaction3 == Faction || WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].friendFaction4 == Faction)
                    return TReaction.FIGHT_SUPPORT;

                // DONE: Friendly to your faction mask
                if (Conversions.ToBoolean(WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].friendMask & WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[Faction].ourMask))
                    return TReaction.FIGHT_SUPPORT;

                // DONE: Hostile to your faction
                if (WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].enemyFaction1 == Faction || WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].enemyFaction2 == Faction || WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].enemyFaction3 == Faction || WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].enemyFaction4 == Faction)
                    return TReaction.HOSTILE;

                // DONE: Hostile to your faction mask
                if (Conversions.ToBoolean(WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].enemyMask & WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[Faction].ourMask))
                    return TReaction.HOSTILE;

                // DONE: Hostile by reputation
                var Rank = GetReputation(WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionID].FactionID);
                if (Rank <= ReputationRank.Hostile)
                {
                    return TReaction.HOSTILE;
                }
                else if (Rank >= ReputationRank.Revered)
                {
                    return TReaction.FIGHT_SUPPORT;
                }
                else if (Rank >= ReputationRank.Friendly)
                {
                    return TReaction.FRIENDLY;
                }
                else
                {
                    return TReaction.NEUTRAL;
                }
            }

            public int GetReputationValue(int FactionTemplateID)
            {
                if (!WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo.ContainsKey(FactionTemplateID))
                    return ReputationRank.Neutral;
                int FactionID = WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionTemplateID].FactionID;
                if (!WorldServiceLocator._WS_DBCDatabase.FactionInfo.ContainsKey(FactionID))
                    return ReputationRank.Neutral;
                if (WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].VisibleID == -1)
                    return ReputationRank.Neutral;
                int points;
                if (WorldServiceLocator._Functions.HaveFlag(WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].flags[0], Race - 1))
                {
                    points = WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].rep_stats[0];
                }
                else if (WorldServiceLocator._Functions.HaveFlag(WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].flags[1], Race - 1))
                {
                    points = WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].rep_stats[1];
                }
                else if (WorldServiceLocator._Functions.HaveFlag(WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].flags[2], Race - 1))
                {
                    points = WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].rep_stats[2];
                }
                else if (WorldServiceLocator._Functions.HaveFlag(WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].flags[3], Race - 1))
                {
                    points = WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].rep_stats[3];
                }
                else
                {
                    points = 0;
                }

                if (Reputation[WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].VisibleID].Flags > 0)
                {
                    points += Reputation[WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].VisibleID].Value;
                }

                return points;
            }

            public ReputationRank GetReputation(int FactionTemplateID)
            {
                if (!WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo.ContainsKey(FactionTemplateID))
                    return ReputationRank.Neutral;
                int FactionID = WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[FactionTemplateID].FactionID;
                if (!WorldServiceLocator._WS_DBCDatabase.FactionInfo.ContainsKey(FactionID))
                    return ReputationRank.Neutral;
                if (WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].VisibleID == -1)
                    return ReputationRank.Neutral;
                int points;
                if (WorldServiceLocator._Functions.HaveFlag(WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].flags[0], Race - 1))
                {
                    points = WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].rep_stats[0];
                }
                else if (WorldServiceLocator._Functions.HaveFlag(WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].flags[1], Race - 1))
                {
                    points = WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].rep_stats[1];
                }
                else if (WorldServiceLocator._Functions.HaveFlag(WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].flags[2], Race - 1))
                {
                    points = WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].rep_stats[2];
                }
                else if (WorldServiceLocator._Functions.HaveFlag(WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].flags[3], Race - 1))
                {
                    points = WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].rep_stats[3];
                }
                else
                {
                    points = 0;
                }

                if (Reputation[WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].VisibleID].Flags > 0)
                {
                    points += Reputation[WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].VisibleID].Value;
                }

                switch (points)
                {
                    case var @case when @case > ReputationPoints.Revered:
                        {
                            return ReputationRank.Exalted;
                        }

                    case var case1 when case1 > ReputationPoints.Honored:
                        {
                            return ReputationRank.Revered;
                        }

                    case var case2 when case2 > ReputationPoints.Friendly:
                        {
                            return ReputationRank.Honored;
                        }

                    case var case3 when case3 > ReputationPoints.Neutral:
                        {
                            return ReputationRank.Friendly;
                        }

                    case var case4 when case4 > ReputationPoints.Unfriendly:
                        {
                            return ReputationRank.Neutral;
                        }

                    case var case5 when case5 > ReputationPoints.Hostile:
                        {
                            return ReputationRank.Unfriendly;
                        }

                    case var case6 when case6 > ReputationPoints.Hated:
                        {
                            return ReputationRank.Hostile;
                        }

                    default:
                        {
                            return ReputationRank.Hated;
                        }
                }
            }

            public void SetReputation(int FactionID, int Value)
            {
                if (WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].VisibleID == -1)
                    return;
                Reputation[WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].VisibleID].Value += Value;
                if ((Reputation[WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].VisibleID].Flags & 1) == 0)
                {
                    Reputation[WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].VisibleID].Flags = Reputation[WorldServiceLocator._WS_DBCDatabase.FactionInfo[FactionID].VisibleID].Flags | 1;
                }

                if (client is object)
                {
                    var packet = new Packets.PacketClass(OPCODES.SMSG_SET_FACTION_STANDING);
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

            public float GetDiscountMod(int FactionID)
            {
                var Rank = GetReputation(FactionID);
                if (Rank >= ReputationRank.Honored)
                    return 0.9f;
                return 1.0f;
            }

            // Death
            public override void Die(ref WS_Base.BaseUnit Attacker)
            {
                // NOTE: Do this first to prevent problems
                DEAD = true;
                corpseGUID = 0UL;
                if (Attacker is object && Attacker is WS_Creatures.CreatureObject)
                {
                    if (((WS_Creatures.CreatureObject)Attacker).aiScript is object)
                    {
                        WS_Base.BaseUnit argVictim = this;
                        ((WS_Creatures.CreatureObject)Attacker).aiScript.OnKill(ref argVictim);
                    }
                }

                GroupUpdateFlag = GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_STATUS;
                foreach (ulong uGuid in inCombatWith)
                {
                    if (WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(uGuid) && WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(uGuid))
                    {
                        // DONE: Remove combat from players who had you in combat
                        WorldServiceLocator._WorldServer.CHARACTERs[uGuid].RemoveFromCombat(this);
                    }
                }

                inCombatWith.Clear();

                // DONE: Check if player is in duel
                if (IsInDuel)
                {
                    DEAD = false;
                    var argLoser = this;
                    WorldServiceLocator._WS_Spells.DuelComplete(ref DuelPartner, ref argLoser);
                    return;
                }

                // DONE: Remove all spells when you die
                for (int i = 0, loopTo = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1; i <= loopTo; i++)
                {
                    if (ActiveSpells[i] is object)
                    {
                        RemoveAura(i, ref ActiveSpells[i].SpellCaster, SendUpdate: false);
                        SetUpdateFlag(EUnitFields.UNIT_FIELD_AURA + i, 0);
                    }
                }

                // DONE: Save as DEAD (GHOST)!
                var argCharacter = this;
                repopTimer = new WS_PlayerHelper.TRepopTimer(ref argCharacter);
                cDynamicFlags = DynamicFlags.UNIT_DYNFLAG_DEAD;
                cUnitFlags = 8;          // player death animation, also can be used with cDynamicFlags
                SetUpdateFlag(EUnitFields.UNIT_FIELD_HEALTH, 0);
                SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, cUnitFlags);
                SetUpdateFlag(EUnitFields.UNIT_DYNAMIC_FLAGS, cDynamicFlags);
                SendCharacterUpdate(true);

                // DONE: 10% Durability lost, and only if the killer is a creature or you died by enviromental damage
                if (Attacker is null || Attacker is WS_Creatures.CreatureObject)
                {
                    for (byte i = 0, loopTo1 = EquipmentSlots.EQUIPMENT_SLOT_END - 1; i <= loopTo1; i++)
                    {
                        if (Items.ContainsKey(i))
                            Items[i].ModifyDurability(0.1f, ref client);
                    }

                    var SMSG_DURABILITY_DAMAGE_DEATH = new Packets.PacketClass(OPCODES.SMSG_DURABILITY_DAMAGE_DEATH);
                    try
                    {
                        client.Send(ref SMSG_DURABILITY_DAMAGE_DEATH);
                    }
                    finally
                    {
                        SMSG_DURABILITY_DAMAGE_DEATH.Dispose();
                    }
                }

                // DONE: Save the character
                Save();
            }

            public void SendDeathReleaseLoc(float x, float y, float z, int MapID)
            {
                // Show spirit healer position on minimap
                var p = new Packets.PacketClass(OPCODES.CMSG_REPOP_REQUEST);
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

            // Combat
            public override void DealDamage(int Damage, [Optional, DefaultParameterValue(null)] ref WS_Base.BaseUnit Attacker)
            {
                // DONE: Check for dead
                if (DEAD)
                    return;

                // DONE: Break some spells when taking any damage
                RemoveAurasByInterruptFlag(SpellAuraInterruptFlags.AURA_INTERRUPT_FLAG_DAMAGE);
                if (spellCasted[CurrentSpellTypes.CURRENT_GENERIC_SPELL] is object)
                {
                    {
                        var withBlock = spellCasted[CurrentSpellTypes.CURRENT_GENERIC_SPELL];
                        if (withBlock.Finished == false)
                        {
                            if (withBlock.SpellInfo.interruptFlags & SpellInterruptFlags.SPELL_INTERRUPT_FLAG_DAMAGE)
                            {
                                FinishAllSpells();
                            }
                            else if (withBlock.SpellInfo.interruptFlags & SpellInterruptFlags.SPELL_INTERRUPT_FLAG_PUSH_BACK)
                            {
                                withBlock.Delay();
                            }
                        }
                    }
                }

                if (Attacker is object)
                {
                    // DONE: Add into combat if not already
                    if (!inCombatWith.Contains(Attacker.GUID))
                    {
                        inCombatWith.Add(Attacker.GUID);
                        CheckCombat();
                        SendCharacterUpdate();
                    }

                    // DONE: Add the attacker into combat if not already
                    if (Attacker is CharacterObject && ((CharacterObject)Attacker).inCombatWith.Contains(GUID) == false)
                    {
                        ((CharacterObject)Attacker).inCombatWith.Add(GUID);
                        if ((((CharacterObject)Attacker).cUnitFlags & UnitFlags.UNIT_FLAG_IN_COMBAT) == 0)
                        {
                            ((CharacterObject)Attacker).cUnitFlags = ((CharacterObject)Attacker).cUnitFlags | UnitFlags.UNIT_FLAG_IN_COMBAT;
                            ((CharacterObject)Attacker).SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, ((CharacterObject)Attacker).cUnitFlags);
                            ((CharacterObject)Attacker).SendCharacterUpdate();
                        }
                    }

                    // DONE: Fight support by NPCs
                    foreach (ulong cGUID in creaturesNear.ToArray())
                    {
                        if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(cGUID) && WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].aiScript is object && WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].isGuard)
                        {
                            if (WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].IsDead == false && WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].aiScript.InCombat() == false)
                            {
                                if (inCombatWith.Contains(cGUID))
                                    continue;
                                if (GetReaction(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].Faction) == TReaction.FIGHT_SUPPORT && WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID], this) <= WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].AggroRange(this))
                                {
                                    WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].aiScript.OnGenerateHate(ref Attacker, Damage);
                                }
                            }
                        }
                    }
                }

                GroupUpdateFlag = GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_CUR_HP;
                if (!Invulnerable)
                    Life.Current -= Damage;
                if (Life.Current == 0)
                {
                    Die(ref Attacker);
                    return;
                }
                else
                {
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_HEALTH, Life.Current);
                    SendCharacterUpdate();
                }

                // TODO: Need a better generation for Range
                // http://www.wowwiki.com/Formulas:Rage_generation
                if (Classe == Classes.CLASS_WARRIOR || Classe == Classes.CLASS_DRUID && (ShapeshiftForm == this.ShapeshiftForm.FORM_BEAR || ShapeshiftForm == this.ShapeshiftForm.FORM_DIREBEAR))
                {
                    Rage.Increment((int)Conversion.Fix(2.5d * Damage / GetRageConversion));
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER1 + ManaTypes.TYPE_RAGE, Rage.Current);
                    SendCharacterUpdate(true);
                }
            }

            public override void Heal(int Damage, [Optional, DefaultParameterValue(null)] ref WS_Base.BaseUnit Attacker)
            {
                if (DEAD)
                    return;
                GroupUpdateFlag = GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_CUR_HP;

                // TODO: Healing generates thread on the NPCs that has this character in their combat array

                Life.Current += Damage;
                SetUpdateFlag(EUnitFields.UNIT_FIELD_HEALTH, Life.Current);
                SendCharacterUpdate();
            }

            public override void Energize(int Damage, ManaTypes Power, [Optional, DefaultParameterValue(null)] ref WS_Base.BaseUnit Attacker)
            {
                if (DEAD)
                    return;
                GroupUpdateFlag = GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_CUR_POWER;
                switch (Power)
                {
                    case var @case when @case == ManaTypes.TYPE_MANA:
                        {
                            if (Mana.Current == Mana.Maximum)
                                return;
                            Mana.Current += Damage;
                            SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER1, Mana.Current);
                            break;
                        }

                    case var case1 when case1 == ManaTypes.TYPE_RAGE:
                        {
                            if (Rage.Current == Rage.Maximum)
                                return;
                            Rage.Current += Damage;
                            SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER2, Rage.Current);
                            break;
                        }

                    case var case2 when case2 == ManaTypes.TYPE_ENERGY:
                        {
                            if (Energy.Current == Energy.Maximum)
                                return;
                            Energy.Current += Damage;
                            SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER4, Energy.Current);
                            break;
                        }

                    default:
                        {
                            return;
                        }
                }

                SendCharacterUpdate();
            }

            // System
            public void Logout(object StateObj = null)
            {
                try
                {
                    LogoutTimer.Dispose();
                    LogoutTimer = null;
                }
                catch
                {
                }

                // DONE: Spawn corpse and remove repop timer if present
                if (repopTimer is object)
                {
                    repopTimer.Dispose();
                    repopTimer = null;
                    // DONE: Spawn Corpse
                    var argCharacter = this;
                    var myCorpse = new WS_Corpses.CorpseObject(ref argCharacter);
                    myCorpse.AddToWorld();
                    myCorpse.Save();
                }

                // DONE: Leave local group
                if (IsInGroup)
                {
                    Group.LocalMembers.Remove(GUID);
                    if (Group.LocalMembers.Count == 0)
                    {
                        Group.Dispose();
                        Group = null;
                    }
                }

                // DONE: Leave transports
                if (OnTransport is object && OnTransport is WS_Transports.TransportObject)
                {
                    WS_Base.BaseUnit argUnit = this;
                    ((WS_Transports.TransportObject)OnTransport).RemovePassenger(ref argUnit);
                }

                // DONE: Cancel duels
                if (DuelPartner is object)
                {
                    if (DuelPartner.DuelArbiter == DuelArbiter)
                    {
                        var argLoser = this;
                        WorldServiceLocator._WS_Spells.DuelComplete(ref DuelPartner, ref argLoser);
                    }
                    else if (WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(DuelArbiter))
                    {
                        WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[DuelArbiter].Destroy(WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[DuelArbiter]);
                    }
                }

                // DONE: Disconnect the client
                var SMSG_LOGOUT_COMPLETE = new Packets.PacketClass(OPCODES.SMSG_LOGOUT_COMPLETE);
                try
                {
                    client.Send(ref SMSG_LOGOUT_COMPLETE);
                    SMSG_LOGOUT_COMPLETE.Dispose();
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_LOGOUT_COMPLETE", client.IP, client.Port);
                    client.Character = null;
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.USER, "Character {0} logged off.", Name);
                    client.Delete();
                    client = null;
                }
                finally
                {
                    Dispose();
                }
            }

            public void Login()
            {
                // DONE: Setting instance ID
                WorldServiceLocator._WS_Handlers_Instance.InstanceMapEnter(this);

                // Set player to transport
                SetOnTransport();

                // DONE: If we have changed map
                if (MapID != LoginMap)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Spawned on wrong map [{0}], transferring to [{1}].", LoginMap, MapID);
                    Transfer(positionX, positionY, positionZ, orientation, (int)MapID);
                    return;
                }

                // Loading map cell if not loaded
                WorldServiceLocator._WS_Maps.GetMapTile(positionX, positionY, ref CellX, ref CellY);
                try
                {
                    if (WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY] is null)
                        WorldServiceLocator._WS_CharMovement.MAP_Load(CellX, CellY, MapID);
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Failed loading maps at character logging in.{0}{1}", Environment.NewLine, ex.ToString());
                }

                // DONE: SMSG_BINDPOINTUPDATE
                var argCharacter = this;
                WorldServiceLocator._WS_PlayerHelper.SendBindPointUpdate(ref client, ref argCharacter);

                // TODO: SMSG_SET_REST_START
                var argCharacter1 = this;
                WorldServiceLocator._WS_PlayerHelper.Send_SMSG_SET_REST_START(ref client, ref argCharacter1);

                // DONE: SMSG_TUTORIAL_FLAGS
                var argCharacter2 = this;
                WorldServiceLocator._WS_PlayerHelper.SendTutorialFlags(ref client, ref argCharacter2);

                // DONE: SMSG_SET_PROFICIENCY
                SendProficiencies();

                // TODO: SMSG_UPDATE_AURA_DURATION

                // DONE: SMSG_INITIAL_SPELLS
                var argCharacter3 = this;
                WorldServiceLocator._WS_PlayerHelper.SendInitialSpells(ref client, ref argCharacter3);
                // DONE: SMSG_INITIALIZE_FACTIONS
                var argCharacter4 = this;
                WorldServiceLocator._WS_PlayerHelper.SendFactions(ref client, ref argCharacter4);
                // DONE: SMSG_ACTION_BUTTONS
                var argCharacter5 = this;
                WorldServiceLocator._WS_PlayerHelper.SendActionButtons(ref client, ref argCharacter5);
                // DONE: SMSG_INIT_WORLD_STATES
                var argCharacter6 = this;
                WorldServiceLocator._WS_PlayerHelper.SendInitWorldStates(ref client, ref argCharacter6);

                // DONE: SMSG_UPDATE_OBJECT for ourself
                Life.Current = Life.Maximum;
                Mana.Current = Mana.Maximum;
                FillAllUpdateFlags();
                SendUpdate();

                // DONE: Adding to World
                var argCharacter7 = this;
                WorldServiceLocator._WS_CharMovement.AddToWorld(ref argCharacter7);

                // DONE: Enable client moving
                WorldServiceLocator._Functions.SendTimeSyncReq(ref client);

                // DONE: Send update on aura durations
                UpdateAuraDurations();
                FullyLoggedIn = true;
                UpdateManaRegen();
            }

            public void UpdateAuraDurations()
            {
                for (int i = 0, loopTo = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1; i <= loopTo; i++)
                {
                    if (ActiveSpells[i] is object)
                    {
                        var SMSG_UPDATE_AURA_DURATION = new Packets.PacketClass(OPCODES.SMSG_UPDATE_AURA_DURATION);
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

            public void SetOnTransport()
            {
                if (LoginTransport == 0UL)
                    return;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Spawning on transport.");
                ulong TransportGUID = LoginTransport;
                LoginTransport = 0UL;
                if (TransportGUID > 0m)
                {
                    if (WorldServiceLocator._CommonGlobalFunctions.GuidIsMoTransport(TransportGUID) && WorldServiceLocator._WorldServer.WORLD_TRANSPORTs.ContainsKey(TransportGUID))
                    {
                        OnTransport = WorldServiceLocator._WorldServer.WORLD_TRANSPORTs[TransportGUID];
                        WS_Base.BaseUnit argUnit = this;
                        WorldServiceLocator._WorldServer.WORLD_TRANSPORTs[TransportGUID].AddPassenger(ref argUnit);
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
                            var newGameobject = new WS_GameObjects.GameObjectObject(TransportGUID - WorldServiceLocator._Global_Constants.GUID_TRANSPORT);
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
                        var argCharacter = this;
                        WorldServiceLocator._WorldServer.AllGraveYards.GoToNearestGraveyard(ref argCharacter, true, false);
                        OnTransport = null;
                    }
                }
                else
                {
                    OnTransport = null;
                }
            }

            public void SendProficiencies()
            {
                int ProficiencyFlags = 0;
                if (HaveSpell(9125))
                    ProficiencyFlags += 1 << ITEM_SUBCLASS.ITEM_SUBCLASS_MISC; // Here using spell "Generic"
                if (HaveSpell(9078))
                    ProficiencyFlags += 1 << ITEM_SUBCLASS.ITEM_SUBCLASS_CLOTH;
                if (HaveSpell(9077))
                    ProficiencyFlags += 1 << ITEM_SUBCLASS.ITEM_SUBCLASS_LEATHER;
                if (HaveSpell(8737))
                    ProficiencyFlags += 1 << ITEM_SUBCLASS.ITEM_SUBCLASS_MAIL;
                if (HaveSpell(750))
                    ProficiencyFlags += 1 << ITEM_SUBCLASS.ITEM_SUBCLASS_PLATE;
                if (HaveSpell(9124))
                    ProficiencyFlags += 1 << ITEM_SUBCLASS.ITEM_SUBCLASS_BUCKLER;
                if (HaveSpell(9116))
                    ProficiencyFlags += 1 << ITEM_SUBCLASS.ITEM_SUBCLASS_SHIELD;
                if (HaveSpell(27762))
                    ProficiencyFlags += 1 << ITEM_SUBCLASS.ITEM_SUBCLASS_LIBRAM;
                if (HaveSpell(27763))
                    ProficiencyFlags += 1 << ITEM_SUBCLASS.ITEM_SUBCLASS_TOTEM;
                if (HaveSpell(27764))
                    ProficiencyFlags += 1 << ITEM_SUBCLASS.ITEM_SUBCLASS_IDOL;
                WorldServiceLocator._Functions.SendProficiency(ref client, ITEM_CLASS.ITEM_CLASS_ARMOR, ProficiencyFlags);
                ProficiencyFlags = 0;
                if (HaveSpell(196))
                    ProficiencyFlags += 1 << ITEM_SUBCLASS.ITEM_SUBCLASS_AXE;
                if (HaveSpell(197))
                    ProficiencyFlags += 1 << ITEM_SUBCLASS.ITEM_SUBCLASS_TWOHAND_AXE;
                if (HaveSpell(264))
                    ProficiencyFlags += 1 << ITEM_SUBCLASS.ITEM_SUBCLASS_BOW;
                if (HaveSpell(266))
                    ProficiencyFlags += 1 << ITEM_SUBCLASS.ITEM_SUBCLASS_GUN;
                if (HaveSpell(198))
                    ProficiencyFlags += 1 << ITEM_SUBCLASS.ITEM_SUBCLASS_MACE;
                if (HaveSpell(199))
                    ProficiencyFlags += 1 << ITEM_SUBCLASS.ITEM_SUBCLASS_TWOHAND_MACE;
                if (HaveSpell(200))
                    ProficiencyFlags += 1 << ITEM_SUBCLASS.ITEM_SUBCLASS_POLEARM;
                if (HaveSpell(201))
                    ProficiencyFlags += 1 << ITEM_SUBCLASS.ITEM_SUBCLASS_SWORD;
                if (HaveSpell(202))
                    ProficiencyFlags += 1 << ITEM_SUBCLASS.ITEM_SUBCLASS_TWOHAND_SWORD;
                // If Spells.Contains() Then ProficiencyFlags += (1 << ITEM_SUBCLASS.ITEM_SUBCLASS_WEAPON_obsolete)
                if (HaveSpell(227))
                    ProficiencyFlags += 1 << ITEM_SUBCLASS.ITEM_SUBCLASS_STAFF;
                if (HaveSpell(262))
                    ProficiencyFlags += 1 << ITEM_SUBCLASS.ITEM_SUBCLASS_WEAPON_EXOTIC;
                if (HaveSpell(263))
                    ProficiencyFlags += 1 << ITEM_SUBCLASS.ITEM_SUBCLASS_WEAPON_EXOTIC2;
                if (HaveSpell(15590))
                    ProficiencyFlags += 1 << ITEM_SUBCLASS.ITEM_SUBCLASS_FIST_WEAPON;
                if (HaveSpell(2382))
                    ProficiencyFlags += 1 << ITEM_SUBCLASS.ITEM_SUBCLASS_MISC_WEAPON; // Here using spell "Generic"
                if (HaveSpell(1180))
                    ProficiencyFlags += 1 << ITEM_SUBCLASS.ITEM_SUBCLASS_DAGGER;
                if (HaveSpell(2567))
                    ProficiencyFlags += 1 << ITEM_SUBCLASS.ITEM_SUBCLASS_THROWN;
                if (HaveSpell(3386))
                    ProficiencyFlags += 1 << ITEM_SUBCLASS.ITEM_SUBCLASS_SPEAR;
                if (HaveSpell(5011))
                    ProficiencyFlags += 1 << ITEM_SUBCLASS.ITEM_SUBCLASS_CROSSBOW;
                if (HaveSpell(5009))
                    ProficiencyFlags += 1 << ITEM_SUBCLASS.ITEM_SUBCLASS_WAND;
                if (HaveSpell(7738))
                    ProficiencyFlags += 1 << ITEM_SUBCLASS.ITEM_SUBCLASS_FISHING_POLE;
                WorldServiceLocator._Functions.SendProficiency(ref client, ITEM_CLASS.ITEM_CLASS_WEAPON, ProficiencyFlags);
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
                    // WARNING: Do not save character here!!!

                    // DONE: Remove buyback items when logged out
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("DELETE FROM characters_inventory WHERE item_bag = {0} AND item_slot >= {1} AND item_slot <= {2}", GUID, 69, 80 - 1));
                    if (underWaterTimer is object)
                        underWaterTimer.Dispose();

                    // DONE: Spawn corpse and remove repop timer if present
                    if (repopTimer is object)
                    {
                        repopTimer.Dispose();
                        repopTimer = null;
                        // DONE: Spawn Corpse
                        var argCharacter = this;
                        var myCorpse = new WS_Corpses.CorpseObject(ref argCharacter);
                        myCorpse.Save();
                        myCorpse.AddToWorld();
                    }

                    // DONE: Remove non-combat pets
                    if (NonCombatPet is object)
                        NonCombatPet.Destroy();

                    // DONE: Leave local group
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
                        var argCharacter1 = this;
                        WorldServiceLocator._WS_CharMovement.RemoveFromWorld(ref argCharacter1);
                    }

                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.USER, "Character {0} disposed.", Name);
                    foreach (KeyValuePair<byte, ItemObject> Item in Items)
                        // DONE: Dispose items in bags (done in Item.Dispose)
                        Item.Value.Dispose();
                    this.attackState.Dispose();
                    if (client is object)
                        client.Character = null;
                    if (LogoutTimer is object)
                        LogoutTimer.Dispose();
                    LogoutTimer = null;
                    GC.Collect();
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

                // If Classe = Classes.CLASS_WARRIOR Then Me.ShapeshiftForm = WS_Spells.ShapeshiftForm.FORM_BATTLESTANCE
                if (Classe == Classes.CLASS_WARRIOR)
                    ApplySpell(2457);
                Resistances[DamageTypes.DMG_PHYSICAL].Base += Agility.Base * 2;
                Damage.Type = DamageTypes.DMG_PHYSICAL;
                Damage.Minimum += 1f;
                RangedDamage.Type = DamageTypes.DMG_PHYSICAL;
                RangedDamage.Minimum += 1f;
                // TODO: Calculate base dodge, parry, block

                if (Access >= AccessLevel.GameMaster)
                    GM = true;

                // DONE: Set ammo automatically
                if (Items.ContainsKey(EquipmentSlots.EQUIPMENT_SLOT_RANGED) && this.Items(EquipmentSlots.EQUIPMENT_SLOT_RANGED).ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_WEAPON && (this.Items(EquipmentSlots.EQUIPMENT_SLOT_RANGED).ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_BOW || this.Items(EquipmentSlots.EQUIPMENT_SLOT_RANGED).ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_CROSSBOW || this.Items(EquipmentSlots.EQUIPMENT_SLOT_RANGED).ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_GUN))
                {
                    ITEM_SUBCLASS AmmoType = ITEM_SUBCLASS.ITEM_SUBCLASS_ARROW;
                    if (this.Items(EquipmentSlots.EQUIPMENT_SLOT_RANGED).ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_GUN)
                        AmmoType = ITEM_SUBCLASS.ITEM_SUBCLASS_BULLET;
                    for (byte i = InventorySlots.INVENTORY_SLOT_BAG_START, loopTo = InventorySlots.INVENTORY_SLOT_BAG_END - 1; i <= loopTo; i++)
                    {
                        if (Items.ContainsKey(i) && Items[i].ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_QUIVER)
                        {
                            foreach (KeyValuePair<byte, ItemObject> slot in Items[i].Items)
                            {
                                if (slot.Value.ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_PROJECTILE && slot.Value.ItemInfo.SubClass == AmmoType)
                                {
                                    var argobjCharacter = this;
                                    var CanUse = WorldServiceLocator._CharManagementHandler.CanUseAmmo(ref argobjCharacter, slot.Value.ItemEntry);
                                    if (CanUse == InventoryChangeFailure.EQUIP_ERR_OK)
                                    {
                                        AmmoID = slot.Value.ItemEntry;
                                        AmmoDPS = WorldServiceLocator._WorldServer.ITEMDatabase[AmmoID].Damage[0].Minimum;
                                        var argobjCharacter1 = this;
                                        WorldServiceLocator._WS_Combat.CalculateMinMaxDamage(ref argobjCharacter1, WeaponAttackType.RANGED_ATTACK);
                                        goto DoneAmmo;
                                    }
                                }
                            }
                        }
                    }

                DoneAmmo:
                    ;
                }
            }

            public void SaveAsNewCharacter(int Account_ID)
            {
                // Only for creating New Character
                string tmpCMD = "INSERT INTO characters (account_id";
                string tmpValues = " VALUES (" + Account_ID;
                var temp = new ArrayList();
                tmpCMD += ", char_name";
                tmpValues = tmpValues + ", \"" + Name + "\"";
                tmpCMD += ", char_race";
                tmpValues = tmpValues + ", " + Race;
                tmpCMD += ", char_class";
                tmpValues = tmpValues + ", " + Classe;
                tmpCMD += ", char_gender";
                tmpValues = tmpValues + ", " + Gender;
                tmpCMD += ", char_skin";
                tmpValues = tmpValues + ", " + Skin;
                tmpCMD += ", char_face";
                tmpValues = tmpValues + ", " + Face;
                tmpCMD += ", char_hairStyle";
                tmpValues = tmpValues + ", " + HairStyle;
                tmpCMD += ", char_hairColor";
                tmpValues = tmpValues + ", " + HairColor;
                tmpCMD += ", char_facialHair";
                tmpValues = tmpValues + ", " + FacialHair;
                tmpCMD += ", char_level";
                tmpValues = tmpValues + ", " + Level;
                tmpCMD += ", char_manaType";
                tmpValues = tmpValues + ", " + ManaType;
                tmpCMD += ", char_mana";
                tmpValues = tmpValues + ", " + Mana.Base;
                tmpCMD += ", char_rage";
                tmpValues = tmpValues + ", " + Rage.Base;
                tmpCMD += ", char_energy";
                tmpValues = tmpValues + ", " + Energy.Base;
                tmpCMD += ", char_life";
                tmpValues = tmpValues + ", " + Life.Base;
                tmpCMD += ", char_positionX";
                tmpValues = tmpValues + ", " + Strings.Trim(Conversion.Str(positionX));
                tmpCMD += ", char_positionY";
                tmpValues = tmpValues + ", " + Strings.Trim(Conversion.Str(positionY));
                tmpCMD += ", char_positionZ";
                tmpValues = tmpValues + ", " + Strings.Trim(Conversion.Str(positionZ));
                tmpCMD += ", char_map_id";
                tmpValues = tmpValues + ", " + MapID;
                tmpCMD += ", char_zone_id";
                tmpValues = tmpValues + ", " + ZoneID;
                tmpCMD += ", char_orientation";
                tmpValues = tmpValues + ", " + Strings.Trim(Conversion.Str(orientation));
                tmpCMD += ", bindpoint_positionX";
                tmpValues = tmpValues + ", " + Strings.Trim(Conversion.Str(bindpoint_positionX));
                tmpCMD += ", bindpoint_positionY";
                tmpValues = tmpValues + ", " + Strings.Trim(Conversion.Str(bindpoint_positionY));
                tmpCMD += ", bindpoint_positionZ";
                tmpValues = tmpValues + ", " + Strings.Trim(Conversion.Str(bindpoint_positionZ));
                tmpCMD += ", bindpoint_map_id";
                tmpValues = tmpValues + ", " + bindpoint_map_id;
                tmpCMD += ", bindpoint_zone_id";
                tmpValues = tmpValues + ", " + bindpoint_zone_id;
                tmpCMD += ", char_copper";
                tmpValues = tmpValues + ", " + Copper;
                tmpCMD += ", char_xp";
                tmpValues = tmpValues + ", " + XP;
                tmpCMD += ", char_xp_rested";
                tmpValues = tmpValues + ", " + RestBonus;

                // char_skillList
                temp.Clear();
                foreach (KeyValuePair<int, WS_PlayerHelper.TSkill> Skill in Skills)
                    temp.Add(string.Format("{0}:{1}:{2}", Skill.Key, Skill.Value.Current, Skill.Value.Maximum));
                tmpCMD += ", char_skillList";
                tmpValues = tmpValues + ", \"" + Strings.Join(temp.ToArray(), " ") + "\"";
                tmpCMD += ", char_auraList";
                tmpValues += ", \"\"";

                // char_tutorialFlags
                temp.Clear();
                foreach (byte Flag in TutorialFlags)
                    temp.Add(Flag);
                tmpCMD += ", char_tutorialFlags";
                tmpValues = tmpValues + ", \"" + Strings.Join(temp.ToArray(), " ") + "\"";

                // char_mapExplored
                temp.Clear();
                foreach (byte Flag in ZonesExplored)
                    temp.Add(Flag);
                tmpCMD += ", char_mapExplored";
                tmpValues = tmpValues + ", \"" + Strings.Join(temp.ToArray(), " ") + "\"";

                // char_reputation
                temp.Clear();
                foreach (WS_PlayerHelper.TReputation Reputation_Point in Reputation)
                    temp.Add(Reputation_Point.Flags + ":" + Reputation_Point.Value);
                tmpCMD += ", char_reputation";
                tmpValues = tmpValues + ", \"" + Strings.Join(temp.ToArray(), " ") + "\"";

                // char_actionBar
                temp.Clear();
                foreach (KeyValuePair<byte, WS_PlayerHelper.TActionButton> ActionButton in ActionButtons)
                    temp.Add(string.Format("{0}:{1}:{2}:{3}", ActionButton.Key, ActionButton.Value.Action, ActionButton.Value.ActionType, ActionButton.Value.ActionMisc));
                tmpCMD += ", char_actionBar";
                tmpValues = tmpValues + ", \"" + Strings.Join(temp.ToArray(), " ") + "\"";
                tmpCMD += ", char_strength";
                tmpValues = tmpValues + ", " + Strength.RealBase;
                tmpCMD += ", char_agility";
                tmpValues = tmpValues + ", " + Agility.RealBase;
                tmpCMD += ", char_stamina";
                tmpValues = tmpValues + ", " + Stamina.RealBase;
                tmpCMD += ", char_intellect";
                tmpValues = tmpValues + ", " + Intellect.RealBase;
                tmpCMD += ", char_spirit";
                tmpValues = tmpValues + ", " + Spirit.RealBase;
                uint ForceRestrictions = 0U;
                if (cPlayerFlags & PlayerFlags.PLAYER_FLAGS_HIDE_CLOAK)
                    ForceRestrictions = ForceRestrictions | ForceRestrictionFlags.RESTRICT_HIDECLOAK;
                if (cPlayerFlags & PlayerFlags.PLAYER_FLAGS_HIDE_HELM)
                    ForceRestrictions = ForceRestrictions | ForceRestrictionFlags.RESTRICT_HIDEHELM;
                tmpCMD += ", force_restrictions";
                tmpValues = tmpValues + ", " + ForceRestrictions;
                tmpCMD = tmpCMD + ") " + tmpValues + ");";
                WorldServiceLocator._WorldServer.CharacterDatabase.Update(tmpCMD);
                var MySQLQuery = new DataTable();
                WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT char_guid FROM characters WHERE char_name = '{0}';", Name), MySQLQuery);
                GUID = (ulong)Conversions.ToLong(MySQLQuery.Rows[0]["char_guid"]);
                HonorSaveAsNew();
            }

            public void Save()
            {
                SaveCharacter();
                foreach (KeyValuePair<byte, ItemObject> Item in Items)
                    Item.Value.Save();
            }

            public void SaveCharacter()
            {
                string tmp = "UPDATE characters SET";
                tmp = tmp + " char_name=\"" + Name + "\"";
                tmp = tmp + ", char_race=" + Race;
                tmp = tmp + ", char_class=" + Classe;
                tmp = tmp + ", char_gender=" + Gender;
                tmp = tmp + ", char_skin=" + Skin;
                tmp = tmp + ", char_face=" + Face;
                tmp = tmp + ", char_hairStyle=" + HairStyle;
                tmp = tmp + ", char_hairColor=" + HairColor;
                tmp = tmp + ", char_facialHair=" + FacialHair;
                tmp = tmp + ", char_level=" + Level;
                tmp = tmp + ", char_manaType=" + ManaType;
                tmp = tmp + ", char_life=" + Life.Base;
                tmp = tmp + ", char_rage=" + Rage.Base;
                tmp = tmp + ", char_mana=" + Mana.Base;
                tmp = tmp + ", char_energy=" + Energy.Base;
                tmp = tmp + ", char_strength=" + Strength.RealBase;
                tmp = tmp + ", char_agility=" + Agility.RealBase;
                tmp = tmp + ", char_stamina=" + Stamina.RealBase;
                tmp = tmp + ", char_intellect=" + Intellect.RealBase;
                tmp = tmp + ", char_spirit=" + Spirit.RealBase;
                tmp = tmp + ", char_map_id=" + MapID;
                tmp = tmp + ", char_zone_id=" + ZoneID;
                if (OnTransport is object)
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
                tmp = tmp + ", bindpoint_map_id=" + bindpoint_map_id;
                tmp = tmp + ", bindpoint_zone_id=" + bindpoint_zone_id;
                tmp = tmp + ", char_copper=" + Copper;
                tmp = tmp + ", char_xp=" + XP;
                tmp = tmp + ", char_xp_rested=" + RestBonus;
                tmp = tmp + ", char_guildId=" + GuildID;
                tmp = tmp + ", char_guildRank=" + GuildRank;
                var temp = new ArrayList();

                // char_skillList
                temp.Clear();
                foreach (KeyValuePair<int, WS_PlayerHelper.TSkill> Skill in Skills)
                    temp.Add(string.Format("{0}:{1}:{2}", Skill.Key, Skill.Value.Current, Skill.Value.Maximum));
                tmp = tmp + ", char_skillList=\"" + Strings.Join(temp.ToArray(), " ") + "\"";

                // char_auraList
                temp.Clear();
                for (int i = 0, loopTo = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1; i <= loopTo; i++)
                {
                    if (ActiveSpells[i] is object && (ActiveSpells[i].SpellDuration == WorldServiceLocator._Global_Constants.SPELL_DURATION_INFINITE || ActiveSpells[i].SpellDuration > 10000)) // If the aura exists and if it's worth saving
                    {
                        long expire = 0L;
                        if (ActiveSpells[i].SpellDuration != WorldServiceLocator._Global_Constants.SPELL_DURATION_INFINITE)
                            expire = WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now) + ActiveSpells[i].SpellDuration / 1000;
                        // TODO: If Not_Tick_While_Offline Then expire = -ActiveSpells(i).SpellDuration
                        temp.Add(string.Format("{0}:{1}:{2}", i, ActiveSpells[i].SpellID, expire));
                    }
                }

                tmp = tmp + ", char_auraList=\"" + Strings.Join(temp.ToArray(), " ") + "\"";

                // char_tutorialFlags
                temp.Clear();
                foreach (byte Flag in TutorialFlags)
                    temp.Add(Flag);
                tmp = tmp + ", char_tutorialFlags=\"" + Strings.Join(temp.ToArray(), " ") + "\"";

                // char_taxiFlags
                temp.Clear();
                var TmpArray = new byte[32];
                TaxiZones.CopyTo(TmpArray, 0);
                foreach (byte Flag in TmpArray)
                    temp.Add(Flag);
                tmp = tmp + ", char_taxiFlags=\"" + Strings.Join(temp.ToArray(), " ") + "\"";

                // char_mapExplored
                temp.Clear();
                foreach (uint Flag in ZonesExplored)
                    temp.Add(Flag);
                tmp = tmp + ", char_mapExplored=\"" + Strings.Join(temp.ToArray(), " ") + "\"";

                // char_reputation
                temp.Clear();
                foreach (WS_PlayerHelper.TReputation Reputation_Point in Reputation)
                    temp.Add(Reputation_Point.Flags + ":" + Reputation_Point.Value);
                tmp = tmp + ", char_reputation=\"" + Strings.Join(temp.ToArray(), " ") + "\"";

                // char_actionBar
                temp.Clear();
                foreach (KeyValuePair<byte, WS_PlayerHelper.TActionButton> ActionButton in ActionButtons)
                    temp.Add(string.Format("{0}:{1}:{2}:{3}", ActionButton.Key, ActionButton.Value.Action, ActionButton.Value.ActionType, ActionButton.Value.ActionMisc));
                tmp = tmp + ", char_actionBar=\"" + Strings.Join(temp.ToArray(), " ") + "\"";
                tmp = tmp + ", char_talentpoints=" + TalentPoints;
                uint ForceRestrictions = 0U;
                if (cPlayerFlags & PlayerFlags.PLAYER_FLAGS_HIDE_CLOAK)
                    ForceRestrictions = ForceRestrictions | ForceRestrictionFlags.RESTRICT_HIDECLOAK;
                if (cPlayerFlags & PlayerFlags.PLAYER_FLAGS_HIDE_HELM)
                    ForceRestrictions = ForceRestrictions | ForceRestrictionFlags.RESTRICT_HIDEHELM;
                tmp = tmp + ", force_restrictions=" + ForceRestrictions;
                tmp += string.Format(" WHERE char_guid = \"{0}\";", GUID);
                WorldServiceLocator._WorldServer.CharacterDatabase.Update(tmp);
            }

            public void SavePosition()
            {
                string tmp = "UPDATE characters SET";
                tmp = tmp + ", char_positionX=" + Strings.Trim(Conversion.Str(positionX));
                tmp = tmp + ", char_positionY=" + Strings.Trim(Conversion.Str(positionY));
                tmp = tmp + ", char_positionZ=" + Strings.Trim(Conversion.Str(positionZ));
                tmp = tmp + ", char_map_id=" + MapID;
                tmp += string.Format(" WHERE char_guid = \"{0}\";", GUID);
                WorldServiceLocator._WorldServer.CharacterDatabase.Update(tmp);
            }

            // Party/Raid
            public WS_Group.Group Group = null;
            public uint GroupUpdateFlag = 0U;

            public bool IsInGroup
            {
                get
                {
                    return Group is object;
                }
            }

            public bool IsInRaid
            {
                get
                {
                    return Group is object && Group.Type == GroupType.RAID;
                }
            }

            public bool IsGroupLeader
            {
                get
                {
                    return Group.Leader == GUID;
                }
            }

            public void GroupUpdate()
            {
                if (Group is null)
                    return;
                if (GroupUpdateFlag == 0L)
                    return;
                var argobjCharacter = this;
                var Packet = WorldServiceLocator._WS_Group.BuildPartyMemberStats(ref argobjCharacter, GroupUpdateFlag);
                GroupUpdateFlag = 0U;
                if (Packet is object)
                    Group.Broadcast(Packet);
            }

            public void GroupUpdate(int Flag)
            {
                if (Group is null)
                    return;
                var argobjCharacter = this;
                var Packet = WorldServiceLocator._WS_Group.BuildPartyMemberStats(ref argobjCharacter, (uint)Flag);
                if (Packet is object)
                    Group.Broadcast(Packet);
            }

            // Guilds
            public uint GuildID = 0U;
            public byte GuildRank = 0;
            public int GuildInvited = 0;
            public int GuildInvitedBy = 0;

            public bool IsInGuild
            {
                get
                {
                    return GuildID != 0L;
                }
            }

            // Duel
            public ulong DuelArbiter = 0UL;
            public CharacterObject DuelPartner = null;
            public byte DuelOutOfBounds = WS_Spells.DUEL_COUNTER_DISABLED;

            public bool IsInDuel
            {
                get
                {
                    return DuelPartner is object;
                }
            }

            public void StartDuel()
            {
                Thread.Sleep(WS_Spells.DUEL_COUNTDOWN);
                if (DuelArbiter == 0m)
                    return;
                if (DuelPartner is null)
                    return;

                // DONE: Do updates
                SetUpdateFlag(EPlayerFields.PLAYER_DUEL_TEAM, 1);
                DuelPartner.SetUpdateFlag(EPlayerFields.PLAYER_DUEL_TEAM, 2);
                DuelPartner.SendCharacterUpdate(true);
                SendCharacterUpdate(true);
            }

            // NPC Talking and Quests
            public ArrayList TalkMenuTypes = new ArrayList();
            public WS_QuestsBase[] TalkQuests = new WS_QuestsBase[QuestInfo.QUEST_SLOTS + 1];
            public List<int> QuestsCompleted = new List<int>();
            public WS_QuestInfo TalkCurrentQuest = null;

            public bool TalkAddQuest(ref WS_QuestInfo Quest)
            {
                for (int i = 0, loopTo = QuestInfo.QUEST_SLOTS; i <= loopTo; i++)
                {
                    if (TalkQuests[i] is null)
                    {
                        // DONE: Initialize quest info
                        WorldServiceLocator._WorldServer.ALLQUESTS.CreateQuest(ref TalkQuests[i], ref Quest);

                        // DONE: Initialize quest
                        if (TalkQuests[i] is WS_QuestsBaseScripted)
                        {
                            var argobjCharacter = this;
                            ((WS_QuestsBaseScripted)TalkQuests[i]).OnQuestStart(ref argobjCharacter);
                        }
                        else
                        {
                            var argobjCharacter1 = this;
                            TalkQuests[i].Initialize(ref argobjCharacter1);
                        }

                        TalkQuests[i].Slot = (byte)i;
                        int updateDataCount = UpdateData.Count;
                        int questState = TalkQuests[i].GetProgress();
                        SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_1 + i * 3, TalkQuests[i].ID);
                        SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_2 + i * 3, questState);
                        SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_2 + i * 3 + 1, 0); // Timer
                        WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("INSERT INTO characters_quests (char_guid, quest_id, quest_status) VALUES ({0}, {1}, {2});", GUID, TalkQuests[i].ID, questState));
                        SendCharacterUpdate(updateDataCount != 0);
                        return true;
                    }
                }

                return false;
            }

            public bool TalkDeleteQuest(byte QuestSlot)
            {
                if (TalkQuests[QuestSlot] is null)
                {
                    return false;
                }
                else
                {
                    if (TalkQuests[QuestSlot] is WS_QuestsBaseScripted)
                    {
                        var argobjCharacter = this;
                        ((WS_QuestsBaseScripted)TalkQuests[QuestSlot]).OnQuestCancel(ref argobjCharacter);
                    }

                    int updateDataCount = UpdateData.Count;
                    SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_1 + (int)QuestSlot * 3, 0);
                    SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_2 + (int)QuestSlot * 3, 0);
                    SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_2 + (int)QuestSlot * 3 + 1, 0);
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("DELETE  FROM characters_quests WHERE char_guid = {0} AND quest_id = {1};", GUID, TalkQuests[QuestSlot].ID));
                    TalkQuests[QuestSlot] = null;
                    SendCharacterUpdate(updateDataCount != 0);
                    return true;
                }
            }

            public bool TalkCompleteQuest(byte QuestSlot)
            {
                if (TalkQuests[QuestSlot] is null)
                {
                    return false;
                }
                else
                {
                    if (TalkQuests[QuestSlot] is WS_QuestsBaseScripted)
                    {
                        var argobjCharacter = this;
                        ((WS_QuestsBaseScripted)TalkQuests[QuestSlot]).OnQuestComplete(ref argobjCharacter);
                    }

                    int updateDataCount = UpdateData.Count;
                    SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_1 + (int)QuestSlot * 3, 0);
                    SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_2 + (int)QuestSlot * 3, 0);
                    SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_2 + (int)QuestSlot * 3 + 1, 0);
                    QuestsCompleted.Add(TalkQuests[QuestSlot].ID);
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_quests SET quest_status = -1 WHERE char_guid = {0} AND quest_id = {1};", GUID, TalkQuests[QuestSlot].ID));
                    TalkQuests[QuestSlot] = null;

                    // SendCharacterUpdate(updateDataCount <> 0)
                    return true;
                }
            }

            public bool TalkUpdateQuest(byte QuestSlot)
            {
                if (TalkQuests[QuestSlot] is null)
                {
                    return false;
                }
                else
                {
                    int updateDataCount = UpdateData.Count;
                    int tmpState = TalkQuests[QuestSlot].GetState();
                    int tmpProgress = TalkQuests[QuestSlot].GetProgress();
                    if (TalkQuests[QuestSlot].TimeEnd > 0)
                        int tmpTimer = (int)(TalkQuests[QuestSlot].TimeEnd - WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now));
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_quests SET quest_status = {2} WHERE char_guid = {0} AND quest_id = {1};", GUID, TalkQuests[QuestSlot].ID, tmpProgress));
                    SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_2 + (int)QuestSlot * 3, tmpProgress);
                    SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_2 + (int)QuestSlot * 3 + 1, 0); // Timer
                    SendCharacterUpdate(updateDataCount != 0);
                    return true;
                }
            }

            public bool TalkCanAccept(ref WS_QuestInfo Quest)
            {
                var DBResult = new DataTable();
                WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT quest_status FROM characters_quests WHERE char_guid = {0} AND quest_id = {1} LIMIT 1;", (object)GUID, (object)Quest.ID), DBResult);
                if (DBResult.Rows.Count > 0)
                {
                    int status = Conversions.ToInteger(DBResult.Rows[0]["quest_status"]);
                    if (status == -1) // Quest is completed
                    {
                        var packet = new Packets.PacketClass(OPCODES.SMSG_QUESTGIVER_QUEST_INVALID);
                        try
                        {
                            packet.AddInt32(QuestInvalidError.INVALIDREASON_COMPLETED_QUEST);
                            client.Send(ref packet);
                        }
                        finally
                        {
                            packet.Dispose();
                        }
                    }
                    else
                    {
                        var packet = new Packets.PacketClass(OPCODES.SMSG_QUESTGIVER_QUEST_INVALID);
                        try
                        {
                            packet.AddInt32(QuestInvalidError.INVALIDREASON_HAVE_QUEST);
                            client.Send(ref packet);
                        }
                        finally
                        {
                            packet.Dispose();
                        }
                    }

                    return false;
                }

                if (Quest.RequiredRace != 0 && (Quest.RequiredRace & 1 << Race - 1) == 0)
                {
                    var packet = new Packets.PacketClass(OPCODES.SMSG_QUESTGIVER_QUEST_INVALID);
                    try
                    {
                        packet.AddInt32(QuestInvalidError.INVALIDREASON_DONT_HAVE_RACE);
                        client.Send(ref packet);
                    }
                    finally
                    {
                        packet.Dispose();
                    }

                    return false;
                }

                if (Quest.RequiredClass != 0 && (Quest.RequiredClass & 1 << Classe - 1) == 0)
                {
                    // TODO: Find constant for INVALIDREASON_DONT_HAVE_CLASS if exists
                    var packet = new Packets.PacketClass(OPCODES.SMSG_QUESTGIVER_QUEST_INVALID);
                    try
                    {
                        packet.AddInt32(QuestInvalidError.INVALIDREASON_DONT_HAVE_REQ);
                        client.Send(ref packet);
                    }
                    finally
                    {
                        packet.Dispose();
                    }

                    return false;
                }

                if (Quest.RequiredTradeSkill != 0 && !Skills.ContainsKey(Quest.RequiredTradeSkill))
                {
                    // TODO: Find constant for INVALIDREASON_DONT_HAVE_SKILL if exists
                    var packet = new Packets.PacketClass(OPCODES.SMSG_QUESTGIVER_QUEST_INVALID);
                    try
                    {
                        packet.AddInt32(QuestInvalidError.INVALIDREASON_DONT_HAVE_REQ);
                        client.Send(ref packet);
                    }
                    finally
                    {
                        packet.Dispose();
                    }

                    return false;
                }

                // TODO: Check requirements for reputation
                // TODO: Check requirements for honor?

                return true;
            }

            public bool IsQuestCompleted(int QuestID)
            {
                var q = new DataTable();
                WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT quest_id FROM characters_quests WHERE char_guid = {0} AND quest_status = -1 AND quest_id = {1};", (object)GUID, (object)QuestID), q);
                return q.Rows.Count != 0;
            }

            public bool IsQuestInProgress(int QuestID)
            {
                for (int i = 0, loopTo = QuestInfo.QUEST_SLOTS; i <= loopTo; i++)
                {
                    if (TalkQuests[i] is object)
                    {
                        if (TalkQuests[i].ID == QuestID)
                            return true;
                    }
                }

                return false;
            }

            // Helper Funtions
            public void LogXPGain(int Ammount, int Rested, ulong VictimGUID, float Group)
            {
                var SMSG_LOG_XPGAIN = new Packets.PacketClass(OPCODES.SMSG_LOG_XPGAIN);
                try
                {
                    SMSG_LOG_XPGAIN.AddUInt64(VictimGUID);

                    // Total XP
                    SMSG_LOG_XPGAIN.AddInt32(Ammount);
                    if (VictimGUID != 0m)
                    {
                        // XP from kill
                        SMSG_LOG_XPGAIN.AddInt8(0);
                    }
                    else
                    {
                        // XP from other source
                        SMSG_LOG_XPGAIN.AddInt8(1);
                    }

                    // Rested XP
                    SMSG_LOG_XPGAIN.AddInt32(Ammount - Rested);

                    // Group bonus percent, 100% for none (1.0F)
                    SMSG_LOG_XPGAIN.AddSingle(Group);
                    client.Send(ref SMSG_LOG_XPGAIN);
                }
                finally
                {
                    SMSG_LOG_XPGAIN.Dispose();
                }
            }

            public void LogHonorGain(int Ammount, ulong VictimGUID = 0UL, byte VictimRANK = 0)
            {
                var SMSG_PVP_CREDIT = new Packets.PacketClass(OPCODES.SMSG_PVP_CREDIT);
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
                var response = new Packets.PacketClass(OPCODES.SMSG_ITEM_PUSH_RESULT);
                try
                {
                    response.AddUInt64(GUID);
                    response.AddInt32(Conversions.ToInteger(Recieved)); // 0 = Looted, 1 = From NPC?
                    response.AddInt32(Conversions.ToInteger(Created)); // 0 = Recieved, 1 = Created
                    response.AddInt32(1); // Unk, always 1
                    response.AddInt8(Item.GetBagSlot);
                    if (Item.StackCount == ItemCount)
                    {
                        response.AddInt32(Item.GetSlot); // Item Slot (When added to stack: 0xFFFFFFFF)
                    }
                    else // Added to stack
                    {
                        response.AddInt32(0xFFFFFFFF);
                    }

                    response.AddInt32(Item.ItemEntry);
                    response.AddInt32(Item.SuffixFactor);
                    response.AddInt32(Item.RandomProperties);
                    response.AddInt32(ItemCount); // Count of items
                    response.AddInt32(ItemCOUNT(Item.ItemEntry)); // Count of items in inventory
                    client.SendMultiplyPackets(ref response);
                    if (IsInGroup)
                        Group.Broadcast(response);
                }
                finally
                {
                    response.Dispose();
                }
            }

            public void LogEnvironmentalDamage(DamageTypes dmgType, int Damage)
            {
                var SMSG_ENVIRONMENTALDAMAGELOG = new Packets.PacketClass(OPCODES.SMSG_ENVIRONMENTALDAMAGELOG);
                try
                {
                    SMSG_ENVIRONMENTALDAMAGELOG.AddUInt64(GUID);
                    SMSG_ENVIRONMENTALDAMAGELOG.AddInt8(dmgType);
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

            public bool IsHorde
            {
                get
                {
                    switch (Race)
                    {
                        case var @case when @case == Races.RACE_DWARF:
                        case var case1 when case1 == Races.RACE_GNOME:
                        case var case2 when case2 == Races.RACE_HUMAN:
                        case var case3 when case3 == Races.RACE_NIGHT_ELF:
                            {
                                return false;
                            }

                        default:
                            {
                                return true;
                            }
                    }
                }
            }

            public int Team
            {
                get
                {
                    switch (Race)
                    {
                        case var @case when @case == Races.RACE_DWARF:
                        case var case1 when case1 == Races.RACE_GNOME:
                        case var case2 when case2 == Races.RACE_HUMAN:
                        case var case3 when case3 == Races.RACE_NIGHT_ELF:
                            {
                                return 469;
                            }

                        default:
                            {
                                return 67;
                            }
                    }
                }
            }

            public float GetStealthDistance(ref WS_Base.BaseUnit objCharacter)
            {
                float VisibleDistance = (float)(10.5d - Invisibility_Value / 100d);
                VisibleDistance += objCharacter.Level - Level;
                VisibleDistance = (float)(VisibleDistance + (objCharacter.CanSeeInvisibility_Stealth - Invisibility_Bonus) / 5d);
                return VisibleDistance;
            }

            // Warden AntiCheat Engine
            public WS_Handlers_Warden.WardenData WardenData = new WS_Handlers_Warden.WardenData();
        }
    }
}