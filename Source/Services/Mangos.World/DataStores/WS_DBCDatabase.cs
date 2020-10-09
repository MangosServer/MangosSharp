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
using System.Collections.Generic;
using System.Data;
using Mangos.Common.Enums.Global;
using Mangos.World.Server;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.DataStores
{
    public class WS_DBCDatabase
    {

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public Dictionary<int, int> EmotesState = new Dictionary<int, int>();
        public Dictionary<int, int> EmotesText = new Dictionary<int, int>();
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public Dictionary<int, int> SkillLines = new Dictionary<int, int>();
        public Dictionary<int, TSkillLineAbility> SkillLineAbility = new Dictionary<int, TSkillLineAbility>();

        public class TSkillLineAbility
        {
            public int ID;
            public int SkillID;
            public int SpellID;
            public int Unknown1;
            public int Unknown2;
            public int Unknown3;
            public int Unknown4;
            public int Required_Skill_Value; // For Trade Skill, Not For Training
            public int Forward_SpellID;
            public int Unknown5;
            public int Max_Value;
            public int Min_Value;
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public Dictionary<int, TTaxiNode> TaxiNodes = new Dictionary<int, TTaxiNode>();
        public Dictionary<int, TTaxiPath> TaxiPaths = new Dictionary<int, TTaxiPath>();
        public Dictionary<int, Dictionary<int, TTaxiPathNode>> TaxiPathNodes = new Dictionary<int, Dictionary<int, TTaxiPathNode>>();

        public class TTaxiNode
        {
            public float x;
            public float y;
            public float z;
            public int MapID;
            public int HordeMount = 0;
            public int AllianceMount = 0;

            public TTaxiNode(float px, float py, float pz, int pMapID, int pHMount, int pAMount)
            {
                x = px;
                y = py;
                z = pz;
                MapID = pMapID;
                HordeMount = pHMount;
                AllianceMount = pAMount;
            }
        }

        public class TTaxiPath
        {
            public int TFrom;
            public int TTo;
            public int Price;

            public TTaxiPath(int pFrom, int pTo, int pPrice)
            {
                TFrom = pFrom;
                TTo = pTo;
                Price = pPrice;
            }
        }

        public class TTaxiPathNode
        {
            public int Path;
            public int Seq;
            public int MapID;
            public float x;
            public float y;
            public float z;
            public int action;
            public int waittime;

            public TTaxiPathNode(float px, float py, float pz, int pMapID, int pPath, int pSeq, int pAction, int pWaittime)
            {
                x = px;
                y = py;
                z = pz;
                MapID = pMapID;
                Path = pPath;
                Seq = pSeq;
                action = pAction;
                waittime = pWaittime;
            }
        }

        public int GetNearestTaxi(float x, float y, int map)
        {
            float minDistance = 1.0E+8f;
            int selectedTaxiNode = 0;
            float tmp;
            foreach (KeyValuePair<int, TTaxiNode> TaxiNode in TaxiNodes)
            {
                if (TaxiNode.Value.MapID == map)
                {
                    tmp = WorldServiceLocator._WS_Combat.GetDistance(x, TaxiNode.Value.x, y, TaxiNode.Value.y);
                    if (tmp < minDistance)
                    {
                        minDistance = tmp;
                        selectedTaxiNode = TaxiNode.Key;
                    }
                }
            }

            return selectedTaxiNode;
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public Dictionary<int, int> TalentsTab = new Dictionary<int, int>(30);
        public Dictionary<int, TalentInfo> Talents = new Dictionary<int, TalentInfo>(500);

        public class TalentInfo
        {
            public int TalentID;
            public int TalentTab;
            public int Row;
            public int Col;
            public int[] RankID = new int[5];
            public int[] RequiredTalent = new int[3];
            public int[] RequiredPoints = new int[3];
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public const int FACTION_TEMPLATES_COUNT = 2074;
        public Dictionary<int, TCharRace> CharRaces = new Dictionary<int, TCharRace>();

        public class TCharRace
        {
            public short FactionID;
            public int ModelMale;
            public int ModelFemale;
            public byte TeamID;
            public uint TaxiMask;
            public int CinematicID;
            public string RaceName;

            public TCharRace(short Faction, int ModelM, int ModelF, byte Team, uint Taxi, int Cinematic, string Name)
            {
                FactionID = Faction;
                ModelMale = ModelM;
                ModelFemale = ModelF;
                TeamID = Team;
                TaxiMask = Taxi;
                CinematicID = Cinematic;
                RaceName = Name;
            }
        }

        public Dictionary<int, TCharClass> CharClasses = new Dictionary<int, TCharClass>();

        public class TCharClass
        {
            public int CinematicID;

            public TCharClass(int Cinematic)
            {
                CinematicID = Cinematic;
            }
        }

        public Dictionary<int, TFaction> FactionInfo = new Dictionary<int, TFaction>();

        public class TFaction
        {
            public short ID;
            public short VisibleID;
            public short[] flags = new short[4];
            public int[] rep_stats = new int[4];
            public byte[] rep_flags = new byte[4];

            public TFaction(short Id_, short VisibleID_, int flags1, int flags2, int flags3, int flags4, int rep_stats1, int rep_stats2, int rep_stats3, int rep_stats4, int rep_flags1, int rep_flags2, int rep_flags3, int rep_flags4)
            {
                ID = Id_;
                VisibleID = VisibleID_;
                flags[0] = (short)flags1;
                flags[1] = (short)flags2;
                flags[2] = (short)flags3;
                flags[3] = (short)flags4;
                rep_stats[0] = rep_stats1;
                rep_stats[1] = rep_stats2;
                rep_stats[2] = rep_stats3;
                rep_stats[3] = rep_stats4;
                rep_flags[0] = (byte)rep_flags1;
                rep_flags[1] = (byte)rep_flags2;
                rep_flags[2] = (byte)rep_flags3;
                rep_flags[3] = (byte)rep_flags4;
            }
        }

        public Dictionary<int, TFactionTemplate> FactionTemplatesInfo = new Dictionary<int, TFactionTemplate>();

        public class TFactionTemplate
        {
            public int FactionID;
            public uint ourMask;
            public uint friendMask;
            public uint enemyMask;
            public int enemyFaction1;
            public int enemyFaction2;
            public int enemyFaction3;
            public int enemyFaction4;
            public int friendFaction1;
            public int friendFaction2;
            public int friendFaction3;
            public int friendFaction4;
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public List<TSpellShapeshiftForm> SpellShapeShiftForm = new List<TSpellShapeshiftForm>();

        public class TSpellShapeshiftForm
        {
            public int ID = 0;
            public int Flags1 = 0;
            public int CreatureType;
            public int AttackSpeed;

            public TSpellShapeshiftForm(int ID_, int Flags1_, int CreatureType_, int AttackSpeed_)
            {
                ID = ID_;
                Flags1 = Flags1_;
                CreatureType = CreatureType_;
                AttackSpeed = AttackSpeed_;
            }
        }

        public TSpellShapeshiftForm FindShapeshiftForm(int ID)
        {
            foreach (TSpellShapeshiftForm Form in SpellShapeShiftForm)
            {
                if (Form.ID == ID)
                {
                    return Form;
                }
            }

            return null;
        }

        public List<float> gtOCTRegenHP = new List<float>();
        public List<float> gtOCTRegenMP = new List<float>();
        public List<float> gtRegenHPPerSpt = new List<float>();
        public List<float> gtRegenMPPerSpt = new List<float>();
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public const int DurabilityCosts_MAX = 300;
        public short[,] DurabilityCosts = new short[301, 29];
        public Dictionary<int, TSpellItemEnchantment> SpellItemEnchantments = new Dictionary<int, TSpellItemEnchantment>();

        public class TSpellItemEnchantment
        {
            public int[] Type = new int[3];
            public int[] Amount = new int[3];
            public int[] SpellID = new int[3];
            public int AuraID;
            public int Slot;
            // Public EnchantmentConditions As Integer

            public TSpellItemEnchantment(int[] Types, int[] Amounts, int[] SpellIDs, int AuraID_, int Slot_) // , ByVal EnchantmentConditions_ As Integer)
            {
                for (byte i = 0; i <= 2; i++)
                {
                    Type[i] = Types[i];
                    Amount[i] = Amounts[i];
                    SpellID[i] = SpellIDs[i];
                }

                AuraID = AuraID_;
                Slot = Slot_;
                // EnchantmentConditions = EnchantmentConditions_
            }
        }

        public Dictionary<int, TItemSet> ItemSet = new Dictionary<int, TItemSet>();

        public class TItemSet
        {
            public int ID; // 0
            public string Name; // 1
            public int[] ItemID = new int[8]; // 10-17
            public int[] SpellID = new int[8]; // 66-73
            public int[] ItemCount = new int[8]; // 74-81
            public int Required_Skill_ID; // 82
            public int Required_Skill_Value; // 83

            public TItemSet(string Name_, int[] ItemID_, int[] SpellID_, int[] ItemCount_, int Required_Skill_ID_, int Required_Skill_Value_)
            {
                for (byte i = 0; i <= 7; i++)
                {
                    SpellID[i] = SpellID_[i];
                    ItemID[i] = ItemID_[i];
                    ItemCount[i] = ItemCount_[i];
                }

                Name = Name_;
                Required_Skill_ID = Required_Skill_ID_;
                Required_Skill_Value = Required_Skill_Value_;
            }
        }

        public Dictionary<int, TItemDisplayInfo> ItemDisplayInfo = new Dictionary<int, TItemDisplayInfo>();

        public class TItemDisplayInfo
        {
            public int ID;
            public int RandomPropertyChance;
            public int Unknown;
        }

        public Dictionary<int, TItemRandomPropertiesInfo> ItemRandomPropertiesInfo = new Dictionary<int, TItemRandomPropertiesInfo>();

        public class TItemRandomPropertiesInfo
        {
            public int ID;
            public int[] Enchant_ID = new int[4];
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */        /// <summary>
        /// Initializes the xp lookup table from db.
        /// </summary>
        /// <returns></returns>
        private void InitializeXpTableFromDb()
        {
            DataTable result = null;
            int dbLvl;
            long dbXp;
            try
            {
                WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT * FROM player_xp_for_level order by lvl;"), result);
                if (result.Rows.Count > 0)
                {
                    foreach (DataRow row in result.Rows)
                    {
                        dbLvl = Conversions.ToInteger(row["lvl"]);
                        dbXp = Conversions.ToLong(row["xp_for_next_level"]);
                        WorldServiceLocator._WS_Player_Initializator.XPTable[dbLvl] = (int)dbXp;
                    }
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Initalizing: XPTable initialized.");
            }
            catch (Exception ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "XPTable initialization failed.");
            }
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public Dictionary<int, byte> Battlemasters = new Dictionary<int, byte>();

        public void InitializeBattlemasters()
        {
            var MySQLQuery = new DataTable();
            WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT * FROM battlemaster_entry"), MySQLQuery);
            foreach (DataRow row in MySQLQuery.Rows)
                Battlemasters.Add(Conversions.ToInteger(row["entry"]), Conversions.ToByte(row["bg_template"]));
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "World: {0} Battlemasters Loaded.", MySQLQuery.Rows.Count);
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public Dictionary<byte, TBattleground> Battlegrounds = new Dictionary<byte, TBattleground>();

        public void InitializeBattlegrounds()
        {
            byte entry;
            var mySqlQuery = new DataTable();
            WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT * FROM battleground_template"), mySqlQuery);
            foreach (DataRow row in mySqlQuery.Rows)
            {
                entry = Conversions.ToByte(row["id"]);
                Battlegrounds.Add(entry, new TBattleground());

                // Battlegrounds(Entry).Map = row.Item("Map")
                Battlegrounds[entry].MinPlayersPerTeam = Conversions.ToByte(row["MinPlayersPerTeam"]);
                Battlegrounds[entry].MaxPlayersPerTeam = Conversions.ToByte(row["MaxPlayersPerTeam"]);
                Battlegrounds[entry].MinLevel = Conversions.ToByte(row["MinLvl"]);
                Battlegrounds[entry].MaxLevel = Conversions.ToByte(row["MaxLvl"]);
                Battlegrounds[entry].AllianceStartLoc = Conversions.ToSingle(row["AllianceStartLoc"]);
                Battlegrounds[entry].AllianceStartO = Conversions.ToSingle(row["AllianceStartO"]);
                Battlegrounds[entry].HordeStartLoc = Conversions.ToSingle(row["HordeStartLoc"]);
                Battlegrounds[entry].HordeStartO = Conversions.ToSingle(row["HordeStartO"]);
            }

            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "World: {0} Battlegrounds Loaded.", mySqlQuery.Rows.Count);
        }

        public class TBattleground
        {
            // Public Map As Integer
            public byte MinPlayersPerTeam;
            public byte MaxPlayersPerTeam;
            public byte MinLevel;
            public byte MaxLevel;
            public float AllianceStartLoc;
            public float AllianceStartO;
            public float HordeStartLoc;
            public float HordeStartO;
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public Dictionary<int, TTeleportCoords> TeleportCoords = new Dictionary<int, TTeleportCoords>();

        public void InitializeTeleportCoords()
        {
            int SpellID;
            var MySQLQuery = new DataTable();
            WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT * FROM spells_teleport_coords"), MySQLQuery);
            foreach (DataRow row in MySQLQuery.Rows)
            {
                SpellID = Conversions.ToInteger(row["id"]);
                TeleportCoords.Add(SpellID, new TTeleportCoords());
                TeleportCoords[SpellID].Name = Conversions.ToString(row["name"]);
                TeleportCoords[SpellID].MapID = Conversions.ToUInteger(row["mapId"]);
                TeleportCoords[SpellID].PosX = Conversions.ToSingle(row["position_x"]);
                TeleportCoords[SpellID].PosY = Conversions.ToSingle(row["position_y"]);
                TeleportCoords[SpellID].PosZ = Conversions.ToSingle(row["position_z"]);
            }

            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "World: {0} Teleport Coords Loaded.", MySQLQuery.Rows.Count);
        }

        public class TTeleportCoords
        {
            public string Name;
            public uint MapID;
            public float PosX;
            public float PosY;
            public float PosZ;
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */        // Public Sub InitializeMonsterSayCombat()
                                                               // ' Load the MonsterSayCombat Hashtable.
                                                               // Dim Entry As Integer = 0
                                                               // Dim EventNo As Integer = 0
                                                               // Dim Chance As Single = 0.0F
                                                               // Dim Language As Integer = 0
                                                               // Dim Type As Integer = 0
                                                               // Dim MonsterName As String = ""
                                                               // Dim Text0 As String = ""
                                                               // Dim Text1 As String = ""
                                                               // Dim Text2 As String = ""
                                                               // Dim Text3 As String = ""
                                                               // Dim Text4 As String = ""
                                                               // Dim Count As Integer = 0

        // Dim MySQLQuery As New DataTable
        // _WorldServer.WorldDatabase.Query(String.Format("SELECT * FROM npc_monstersay"), MySQLQuery)
        // For Each MonsterRow As DataRow In MySQLQuery.Rows
        // Count = Count + 1
        // Entry = MonsterRow.Item("entry")
        // EventNo = MonsterRow.Item("event")
        // Chance = MonsterRow.Item("chance")
        // Language = MonsterRow.Item("language")
        // Type = MonsterRow.Item("type")
        // If Not MonsterRow.Item("monstername") Is DBNull.Value Then
        // MonsterName = MonsterRow.Item("monstername")
        // Else
        // MonsterName = ""
        // End If

        // If Not MonsterRow.Item("text0") Is DBNull.Value Then
        // Text0 = MonsterRow.Item("text0")
        // Else
        // Text0 = ""
        // End If

        // If Not MonsterRow.Item("text1") Is DBNull.Value Then
        // Text1 = MonsterRow.Item("text1")
        // Else
        // Text1 = ""
        // End If

        // If Not MonsterRow.Item("text2") Is DBNull.Value Then
        // Text2 = MonsterRow.Item("text2")
        // Else
        // Text2 = ""
        // End If

        // If Not MonsterRow.Item("text3") Is DBNull.Value Then
        // Text3 = MonsterRow("text3")
        // Else
        // Text3 = ""
        // End If

        // If Not MonsterRow.Item("text4") Is DBNull.Value Then
        // Text4 = MonsterRow("text4")
        // Else
        // Text4 = ""
        // End If

        // If EventNo = MonsterSayEvents.MONSTER_SAY_EVENT_COMBAT Then
        // MonsterSayCombat(Entry) = New TMonsterSayCombat(Entry, EventNo, Chance, Language, Type, MonsterName, Text0, Text1, Text2, Text3, Text4)
        // End If

        // Next

        // _WorldServer.Log.WriteLine(LogType.INFORMATION, "World: {0} Monster Say(s) Loaded.", Count)

        // End Sub
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public Dictionary<ulong, int> CreatureGossip = new Dictionary<ulong, int>();
        public Dictionary<int, CreatureFamilyInfo> CreaturesFamily = new Dictionary<int, CreatureFamilyInfo>();

        public class CreatureFamilyInfo
        {
            public int ID;
            public int Unknown1;
            public int Unknown2;
            public int PetFoodID;
            public string Name;
        }

        public Dictionary<int, Dictionary<int, CreatureMovePoint>> CreatureMovement = new Dictionary<int, Dictionary<int, CreatureMovePoint>>();

        public class CreatureMovePoint
        {
            public float x;
            public float y;
            public float z;
            public int waittime;
            public int moveflag;
            public int action;
            public int actionchance;

            public CreatureMovePoint(float PosX, float PosY, float PosZ, int Wait, int MoveFlag, int Action, int ActionChance)
            {
                x = PosX;
                y = PosY;
                z = PosZ;
                waittime = Wait;
                moveflag = MoveFlag;
                action = Action;
                actionchance = ActionChance;
            }
        }

        public Dictionary<int, CreatureEquipInfo> CreatureEquip = new Dictionary<int, CreatureEquipInfo>();

        public class CreatureEquipInfo
        {
            public int[] EquipModel = new int[3];
            public uint[] EquipInfo = new uint[3];
            public int[] EquipSlot = new int[3];

            public CreatureEquipInfo(int EquipModel1, int EquipModel2, int EquipModel3, uint EquipInfo1, uint EquipInfo2, uint EquipInfo3, int EquipSlot1, int EquipSlot2, int EquipSlot3)
            {
                EquipModel[0] = EquipModel1;
                EquipModel[1] = EquipModel2;
                EquipModel[2] = EquipModel3;
                EquipInfo[0] = EquipInfo1;
                EquipInfo[1] = EquipInfo2;
                EquipInfo[2] = EquipInfo3;
                EquipSlot[0] = EquipSlot1;
                EquipSlot[1] = EquipSlot2;
                EquipSlot[2] = EquipSlot3;
            }
        }

        public Dictionary<int, CreatureModelInfo> CreatureModel = new Dictionary<int, CreatureModelInfo>();

        public class CreatureModelInfo
        {
            public float BoundingRadius;
            public float CombatReach;
            public byte Gender;
            public int ModelIDOtherGender;

            public CreatureModelInfo(float BoundingRadius, float CombatReach, byte Gender, int ModelIDOtherGender)
            {
                this.BoundingRadius = BoundingRadius;
                this.CombatReach = CombatReach;
                this.Gender = Gender;
                this.ModelIDOtherGender = ModelIDOtherGender;
            }
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public void InitializeInternalDatabase()
        {
            InitializeLoadDBCs();
            WorldServiceLocator._WS_Spells.InitializeSpellDB();
            WorldServiceLocator._WS_Commands.RegisterChatCommands();
            try
            {
                WorldServiceLocator._WS_TimerBasedEvents.Regenerator = new WS_TimerBasedEvents.TRegenerator();
                WorldServiceLocator._WS_TimerBasedEvents.AIManager = new WS_TimerBasedEvents.TAIManager();
                WorldServiceLocator._WS_TimerBasedEvents.SpellManager = new WS_TimerBasedEvents.TSpellManager();
                WorldServiceLocator._WS_TimerBasedEvents.CharacterSaver = new WS_TimerBasedEvents.TCharacterSaver();
                WorldServiceLocator._WS_TimerBasedEvents.WeatherChanger = new WS_TimerBasedEvents.TWeatherChanger();
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "World: Loading Maps and Spawns....");

                // DONE: Initializing Counters
                var MySQLQuery = new DataTable();
                try
                {
                    WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT MAX(item_guid) FROM characters_inventory;"), MySQLQuery);
                    if (!ReferenceEquals(MySQLQuery.Rows[0][0], DBNull.Value))
                    {
                        WorldServiceLocator._WorldServer.itemGuidCounter = MySQLQuery.Rows[0][0] + WorldServiceLocator._Global_Constants.GUID_ITEM;
                    }
                    else
                    {
                        WorldServiceLocator._WorldServer.itemGuidCounter = 0 + WorldServiceLocator._Global_Constants.GUID_ITEM;
                    }
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "World: Failed loading characters_inventory....");
                }

                MySQLQuery = new DataTable();
                try
                {
                    WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT MAX(guid) FROM creature;"), MySQLQuery);
                    if (!ReferenceEquals(MySQLQuery.Rows[0][0], DBNull.Value))
                    {
                        WorldServiceLocator._WorldServer.CreatureGUIDCounter = MySQLQuery.Rows[0][0] + WorldServiceLocator._Global_Constants.GUID_UNIT;
                    }
                    else
                    {
                        WorldServiceLocator._WorldServer.CreatureGUIDCounter = 0 + WorldServiceLocator._Global_Constants.GUID_UNIT;
                    }
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "World: Failed loading creatures....");
                }

                MySQLQuery = new DataTable();
                try
                {
                    WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT MAX(guid) FROM gameobject;"), MySQLQuery);
                    if (!ReferenceEquals(MySQLQuery.Rows[0][0], DBNull.Value))
                    {
                        WorldServiceLocator._WorldServer.GameObjectsGUIDCounter = MySQLQuery.Rows[0][0] + WorldServiceLocator._Global_Constants.GUID_GAMEOBJECT;
                    }
                    else
                    {
                        WorldServiceLocator._WorldServer.GameObjectsGUIDCounter = 0 + WorldServiceLocator._Global_Constants.GUID_GAMEOBJECT;
                    }
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "World: Failed loading gameobjects....");
                }

                MySQLQuery = new DataTable();
                try
                {
                    WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT MAX(guid) FROM corpse"), MySQLQuery);
                    if (!ReferenceEquals(MySQLQuery.Rows[0][0], DBNull.Value))
                    {
                        WorldServiceLocator._WorldServer.CorpseGUIDCounter = MySQLQuery.Rows[0][0] + WorldServiceLocator._Global_Constants.GUID_CORPSE;
                    }
                    else
                    {
                        WorldServiceLocator._WorldServer.CorpseGUIDCounter = 0 + WorldServiceLocator._Global_Constants.GUID_CORPSE;
                    }
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "World: Failed loading corpse....");
                }
            }
            catch (Exception e)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Internal database initialization failed! [{0}]{1}{2}", e.Message, Environment.NewLine, e.ToString());
            }
        }

        public void InitializeLoadDBCs()
        {
            WorldServiceLocator._WS_Maps.InitializeMaps();
            InitializeXpTableFromDb();
            WorldServiceLocator._WS_DBCLoad.InitializeEmotes();
            WorldServiceLocator._WS_DBCLoad.InitializeEmotesText();
            WorldServiceLocator._WS_DBCLoad.InitializeAreaTable();
            WorldServiceLocator._WS_DBCLoad.InitializeFactions();
            WorldServiceLocator._WS_DBCLoad.InitializeFactionTemplates();
            WorldServiceLocator._WS_DBCLoad.InitializeCharRaces();
            WorldServiceLocator._WS_DBCLoad.InitializeCharClasses();
            WorldServiceLocator._WS_DBCLoad.InitializeSkillLines();
            WorldServiceLocator._WS_DBCLoad.InitializeSkillLineAbility();
            WorldServiceLocator._WS_DBCLoad.InitializeLocks();
            // _WorldServer.AllGraveYards.InitializeGraveyards()
            WorldServiceLocator._WS_DBCLoad.InitializeTaxiNodes();
            WorldServiceLocator._WS_DBCLoad.InitializeTaxiPaths();
            WorldServiceLocator._WS_DBCLoad.InitializeTaxiPathNodes();
            WorldServiceLocator._WS_DBCLoad.InitializeDurabilityCosts();
            WorldServiceLocator._WS_DBCLoad.LoadSpellItemEnchantments();
            WorldServiceLocator._WS_DBCLoad.LoadItemSet();
            WorldServiceLocator._WS_DBCLoad.LoadItemDisplayInfoDbc();
            WorldServiceLocator._WS_DBCLoad.LoadItemRandomPropertiesDbc();
            WorldServiceLocator._WS_DBCLoad.LoadTalentDbc();
            WorldServiceLocator._WS_DBCLoad.LoadTalentTabDbc();
            WorldServiceLocator._WS_DBCLoad.LoadAuctionHouseDbc();
            WorldServiceLocator._WS_DBCLoad.LoadLootStores();
            WorldServiceLocator._WS_DBCLoad.LoadWeather();
            InitializeBattlemasters();
            InitializeBattlegrounds();
            InitializeTeleportCoords();
            // InitializeMonsterSayCombat()
            WorldServiceLocator._WS_DBCLoad.LoadCreatureFamilyDbc();
            WorldServiceLocator._WS_DBCLoad.InitializeSpellRadius();
            WorldServiceLocator._WS_DBCLoad.InitializeSpellDuration();
            WorldServiceLocator._WS_DBCLoad.InitializeSpellCastTime();
            WorldServiceLocator._WS_DBCLoad.InitializeSpellRange();
            WorldServiceLocator._WS_DBCLoad.InitializeSpellFocusObject();
            WorldServiceLocator._WS_DBCLoad.InitializeSpells();
            WorldServiceLocator._WS_DBCLoad.InitializeSpellShapeShift();
            WorldServiceLocator._WS_DBCLoad.InitializeSpellChains();
            WorldServiceLocator._WS_DBCLoad.LoadCreatureGossip();
            WorldServiceLocator._WS_DBCLoad.LoadCreatureMovements();
            WorldServiceLocator._WS_DBCLoad.LoadCreatureEquipTable();
            WorldServiceLocator._WS_DBCLoad.LoadCreatureModelInfo();
            WorldServiceLocator._WS_DBCLoad.LoadQuestStartersAndFinishers();

            // LoadTransports()

        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
    }
}