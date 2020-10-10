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
using Mangos.Common.Enums.Faction;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Unit;
using Mangos.World.Globals;
using Mangos.World.Gossip;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Objects
{

    // WARNING: Use only with CREATUREsDatabase()
    public class CreatureInfo : IDisposable
    {
        public CreatureInfo(int CreatureID) : this()
        {
            Id = CreatureID;
            WorldServiceLocator._WorldServer.CREATURESDatabase.Add(Id, this);

            // DONE: Load Item Data from MySQL
            var MySQLQuery = new DataTable();
            WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT * FROM creature_template LEFT JOIN creature_template_spells ON creature_template.entry = creature_template_spells.`entry` WHERE creature_template.entry = {0};", (object)CreatureID), MySQLQuery);
            if (MySQLQuery.Rows.Count == 0)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "CreatureID {0} not found in SQL database.", CreatureID);
                found_ = false;
                // Throw New ApplicationException(String.Format("CreatureID {0} not found in SQL database.", CreatureID))
                return;
            }

            found_ = true;
            ModelA1 = Conversions.ToInteger(MySQLQuery.Rows[0]["modelid1"]);
            ModelA2 = Conversions.ToInteger(MySQLQuery.Rows[0]["Modelid2"]);
            ModelH1 = Conversions.ToInteger(MySQLQuery.Rows[0]["modelid3"]);
            ModelH2 = Conversions.ToInteger(MySQLQuery.Rows[0]["modelid4"]);
            Name = Conversions.ToString(MySQLQuery.Rows[0]["name"]);
            try
            {
                SubName = Conversions.ToString(MySQLQuery.Rows[0]["subname"]);
            }
            catch
            {
                SubName = "";
            }

            Size = Conversions.ToSingle(MySQLQuery.Rows[0]["scale"]);
            MinLife = Conversions.ToInteger(MySQLQuery.Rows[0]["MinLevelHealth"]);
            MaxLife = Conversions.ToInteger(MySQLQuery.Rows[0]["MaxLevelHealth"]);
            MinMana = Conversions.ToInteger(MySQLQuery.Rows[0]["MinLevelMana"]);
            MaxMana = Conversions.ToInteger(MySQLQuery.Rows[0]["MaxLevelMana"]);
            ManaType = (byte)ManaTypes.TYPE_MANA;
            Faction = Conversions.ToShort(MySQLQuery.Rows[0]["factionAlliance"]);
            // TODO: factionHorde?
            Elite = Conversions.ToByte(MySQLQuery.Rows[0]["rank"]);
            Damage.Maximum = Conversions.ToSingle(MySQLQuery.Rows[0]["MaxMeleeDmg"]);
            RangedDamage.Maximum = Conversions.ToSingle(MySQLQuery.Rows[0]["MaxRangedDmg"]);
            Damage.Minimum = Conversions.ToSingle(MySQLQuery.Rows[0]["MinMeleeDmg"]);
            RangedDamage.Minimum = Conversions.ToSingle(MySQLQuery.Rows[0]["MinRangedDmg"]);
            AttackPower = Conversions.ToInteger(MySQLQuery.Rows[0]["MeleeAttackPower"]);
            RangedAttackPower = Conversions.ToInteger(MySQLQuery.Rows[0]["RangedAttackPower"]);

            // TODO: What exactly is the speed column in the DB? It sure as hell wasn't runspeed :P
            WalkSpeed = Conversions.ToSingle(MySQLQuery.Rows[0]["SpeedWalk"]);
            RunSpeed = Conversions.ToSingle(MySQLQuery.Rows[0]["SpeedRun"]);
            BaseAttackTime = Conversions.ToShort(MySQLQuery.Rows[0]["MeleeBaseAttackTime"]);
            BaseRangedAttackTime = Conversions.ToShort(MySQLQuery.Rows[0]["RangedBaseAttackTime"]);
            cNpcFlags = Conversions.ToInteger(MySQLQuery.Rows[0]["NpcFlags"]);
            DynFlags = Conversions.ToInteger(MySQLQuery.Rows[0]["DynamicFlags"]);
            cFlags = Conversions.ToInteger(MySQLQuery.Rows[0]["UnitFlags"]);
            TypeFlags = Conversions.ToUInteger(MySQLQuery.Rows[0]["CreatureTypeFlags"]);
            CreatureType = Conversions.ToByte(MySQLQuery.Rows[0]["CreatureType"]);
            CreatureFamily = Conversions.ToByte(MySQLQuery.Rows[0]["Family"]);
            LevelMin = Conversions.ToByte(MySQLQuery.Rows[0]["MinLevel"]);
            LevelMax = Conversions.ToByte(MySQLQuery.Rows[0]["MaxLevel"]);
            TrainerType = Conversions.ToInteger(MySQLQuery.Rows[0]["TrainerType"]);
            TrainerSpell = Conversions.ToInteger(MySQLQuery.Rows[0]["TrainerSpell"]);
            Classe = Conversions.ToByte(MySQLQuery.Rows[0]["TrainerClass"]);
            Race = Conversions.ToByte(MySQLQuery.Rows[0]["TrainerRace"]);
            Leader = Conversions.ToByte(MySQLQuery.Rows[0]["RacialLeader"]);
            if (!Information.IsDBNull(MySQLQuery.Rows[0]["spell1"]))
            {
                Spells[0] = Conversions.ToInteger(MySQLQuery.Rows[0]["spell1"]);
            }
            else
            {
                Spells[0] = 0;
            }

            if (!Information.IsDBNull(MySQLQuery.Rows[0]["spell2"]))
            {
                Spells[1] = Conversions.ToInteger(MySQLQuery.Rows[0]["spell2"]);
            }
            else
            {
                Spells[1] = 0;
            }

            if (!Information.IsDBNull(MySQLQuery.Rows[0]["spell3"]))
            {
                Spells[2] = Conversions.ToInteger(MySQLQuery.Rows[0]["spell3"]);
            }
            else
            {
                Spells[2] = 0;
            }

            if (!Information.IsDBNull(MySQLQuery.Rows[0]["spell4"]))
            {
                Spells[3] = Conversions.ToInteger(MySQLQuery.Rows[0]["spell4"]);
            }
            else
            {
                Spells[3] = 0;
            }

            PetSpellDataID = Conversions.ToInteger(MySQLQuery.Rows[0]["PetSpellDataId"]);
            LootID = Conversions.ToInteger(MySQLQuery.Rows[0]["LootId"]);
            SkinLootID = Conversions.ToInteger(MySQLQuery.Rows[0]["SkinningLootId"]);
            PocketLootID = Conversions.ToInteger(MySQLQuery.Rows[0]["PickpocketLootId"]);
            MinGold = Conversions.ToUInteger(MySQLQuery.Rows[0]["MinLootGold"]);
            MaxGold = Conversions.ToUInteger(MySQLQuery.Rows[0]["MaxLootGold"]);
            Resistances[0] = Conversions.ToInteger(MySQLQuery.Rows[0]["Armor"]);
            Resistances[1] = Conversions.ToInteger(MySQLQuery.Rows[0]["ResistanceHoly"]);
            Resistances[2] = Conversions.ToInteger(MySQLQuery.Rows[0]["ResistanceFire"]);
            Resistances[3] = Conversions.ToInteger(MySQLQuery.Rows[0]["ResistanceNature"]);
            Resistances[4] = Conversions.ToInteger(MySQLQuery.Rows[0]["ResistanceFrost"]);
            Resistances[5] = Conversions.ToInteger(MySQLQuery.Rows[0]["ResistanceShadow"]);
            Resistances[6] = Conversions.ToInteger(MySQLQuery.Rows[0]["ResistanceArcane"]);

            // InhabitType
            // RegenHealth
            EquipmentID = Conversions.ToInteger(MySQLQuery.Rows[0]["EquipmentTemplateId"]);
            MechanicImmune = Conversions.ToUInteger(MySQLQuery.Rows[0]["SchoolImmuneMask"]);
            // flags_extra

            // AIScriptSource = MySQLQuery.Rows(0).Item("ScriptName")

            if (System.IO.File.Exists(@"scripts\gossip\" + WorldServiceLocator._Functions.FixName(Name) + ".vb"))
            {
                var tmpScript = new ScriptedObject(@"scripts\gossip\" + WorldServiceLocator._Functions.FixName(Name) + ".vb", "", true);
                TalkScript = (TBaseTalk)tmpScript.InvokeConstructor("TalkScript");
                tmpScript.Dispose();
            }
            else if (cNpcFlags & NPCFlags.UNIT_NPC_FLAG_TRAINER)
            {
                TalkScript = new WS_NPCs.TDefaultTalk();
            }
            else if (cNpcFlags & NPCFlags.UNIT_NPC_FLAG_GUARD)
            {
                TalkScript = new WS_GuardGossip.TGuardTalk();
            }
            else if (cNpcFlags == 0)
            {
                TalkScript = null;
            }
            else if (cNpcFlags == NPCFlags.UNIT_NPC_FLAG_GOSSIP)
            {
                TalkScript = new WS_NPCs.TDefaultTalk();
            }
            else
            {
                TalkScript = new WS_NPCs.TDefaultTalk();
            }
        }

        public CreatureInfo()
        {
            Damage.Minimum = 0.8f * BaseAttackTime / 1000.0f * (LevelMin * 10.0f);
            Damage.Maximum = 1.2f * BaseAttackTime / 1000.0f * (LevelMax * 10.0f);
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
                WorldServiceLocator._WorldServer.CREATURESDatabase.Remove(Id);
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
        public int Life
        {
            get
            {
                return WorldServiceLocator._WorldServer.Rnd.Next(MinLife, MaxLife);
            }
        }

        public int Mana
        {
            get
            {
                return WorldServiceLocator._WorldServer.Rnd.Next(MinMana, MaxMana);
            }
        }

        public int GetRandomModel
        {
            get
            {
                var modelIDs = new int[4];
                int current = 0;
                if (Conversions.ToBoolean(ModelA1))
                {
                    modelIDs[current] = ModelA1;
                    current += 1;
                }

                if (Conversions.ToBoolean(ModelA2))
                {
                    modelIDs[current] = ModelA2;
                    current += 1;
                }

                if (Conversions.ToBoolean(ModelH1))
                {
                    modelIDs[current] = ModelH1;
                    current += 1;
                }

                if (Conversions.ToBoolean(ModelH2))
                {
                    modelIDs[current] = ModelH2;
                    current += 1;
                }

                if (current == 0)
                    return 0;
                return modelIDs[WorldServiceLocator._WorldServer.Rnd.Next(0, current)];
            }
        }

        public int GetFirstModel
        {
            get
            {
                if (Conversions.ToBoolean(ModelA1))
                {
                    return ModelA1;
                }
                else if (Conversions.ToBoolean(ModelA2))
                {
                    return ModelA2;
                }
                else if (Conversions.ToBoolean(ModelH1))
                {
                    return ModelH1;
                }
                else if (Conversions.ToBoolean(ModelH2))
                {
                    return ModelH2;
                }
                else
                {
                    return 0;
                }
            }
        }

        private readonly bool found_ = false;
        public int Id = 0;
        public string Name = "MISSING_CREATURE_INFO";
        public string SubName = "";
        public float Size = 1f;
        public int ModelA1 = 262;
        public int ModelA2 = 0;
        public int ModelH1 = 0;
        public int ModelH2 = 0;
        public int MinLife = 1;
        public int MaxLife = 1;
        public int MinMana = 1;
        public int MaxMana = 1;
        public byte ManaType = 0;
        public short Faction = (short)FactionTemplates.None;
        public byte CreatureType = (byte)UNIT_TYPE.NONE;
        public byte CreatureFamily = (byte)CREATURE_FAMILY.NONE;
        public byte Elite = (byte)CREATURE_ELITE.NORMAL;
        public byte HonorRank = 0;
        public WS_Items.TDamage Damage = new WS_Items.TDamage();
        public WS_Items.TDamage RangedDamage = new WS_Items.TDamage();
        public int AttackPower = 0;
        public int RangedAttackPower = 0;
        public int[] Resistances = new int[] { 0, 0, 0, 0, 0, 0, 0 };
        public float WalkSpeed = WorldServiceLocator._Global_Constants.UNIT_NORMAL_WALK_SPEED;
        public float RunSpeed = WorldServiceLocator._Global_Constants.UNIT_NORMAL_RUN_SPEED;
        public short BaseAttackTime = 2000;
        public short BaseRangedAttackTime = 2000;
        public int cNpcFlags;
        public int DynFlags;
        public int cFlags;
        public uint TypeFlags;
        public byte LevelMin = 1;
        public byte LevelMax = 1;
        public byte Leader = 0;
        public int TrainerType;
        public int TrainerSpell = 0;
        public byte Classe = 0;
        public byte Race = 0;
        public int PetSpellDataID = 0;
        public int[] Spells = new int[] { 0, 0, 0, 0 };
        public int LootID = 0;
        public int SkinLootID = 0;
        public int PocketLootID = 0;
        public uint MinGold = 0U;
        public uint MaxGold = 0U;
        public int EquipmentID = 0;
        public uint MechanicImmune = 0U;
        public float UnkFloat1 = 1f;
        public float UnkFloat2 = 1f;
        public string AIScriptSource = "";
        public TBaseTalk TalkScript = null;
    }
}