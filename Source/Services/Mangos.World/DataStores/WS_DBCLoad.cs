//
//  Copyright (C) 2013-2021 getMaNGOS <https://getmangos.eu>
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
using System.IO;
using System.Threading.Tasks;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Spell;
using Mangos.Common.Legacy;
using Mangos.DataStores;
using Mangos.World.Loots;
using Mangos.World.Maps;
using Mangos.World.Spells;
using Mangos.World.Weather;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.DataStores
{
    public class WS_DBCLoad
    {
        private readonly DataStoreProvider dataStoreProvider;

        public WS_DBCLoad(DataStoreProvider dataStoreProvider)
        {
            this.dataStoreProvider = dataStoreProvider;
        }

        public async Task InitializeSpellRadiusAsync()
        {
            checked
            {
                try
                {
                    var tmpDBC = await dataStoreProvider.GetDataStoreAsync("SpellRadius.dbc");
                    int num = tmpDBC.Rows - 1;
                    for (int i = 0; i <= num; i++)
                    {
                        int radiusID = tmpDBC.ReadInt(i, 0);
                        float radiusValue = tmpDBC.ReadFloat(i, 1);
                        WorldServiceLocator._WS_Spells.SpellRadius[radiusID] = radiusValue;
                    }
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SpellRadius initialized.", tmpDBC.Rows - 1);
                }
                catch (DirectoryNotFoundException ex)
                {
                    ProjectData.SetProjectError(ex);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DBC File : SpellRadius missing.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    ProjectData.ClearProjectError();
                }
            }
        }

        public async Task InitializeSpellCastTimeAsync()
        {
            checked
            {
                try
                {
                    var tmpDBC = await dataStoreProvider.GetDataStoreAsync("SpellCastTimes.dbc");
                    int num = tmpDBC.Rows - 1;
                    for (int i = 0; i <= num; i++)
                    {
                        int spellCastID = tmpDBC.ReadInt(i, 0);
                        int spellCastTimeS = tmpDBC.ReadInt(i, 1);
                        WorldServiceLocator._WS_Spells.SpellCastTime[spellCastID] = spellCastTimeS;
                    }
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SpellCastTimes initialized.", tmpDBC.Rows - 1);
                }
                catch (DirectoryNotFoundException ex)
                {
                    ProjectData.SetProjectError(ex);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DBC File : SpellCastTimes missing.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    ProjectData.ClearProjectError();
                }
            }
        }

        public async Task InitializeSpellRangeAsync()
        {
            checked
            {
                try
                {
                    var tmpDBC = await dataStoreProvider.GetDataStoreAsync("SpellRange.dbc");
                    int num = tmpDBC.Rows - 1;
                    for (int i = 0; i <= num; i++)
                    {
                        int spellRangeIndex = tmpDBC.ReadInt(i, 0);
                        float spellRangeMax = tmpDBC.ReadFloat(i, 2);
                        WorldServiceLocator._WS_Spells.SpellRange[spellRangeIndex] = spellRangeMax;
                    }
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SpellRanges initialized.", tmpDBC.Rows - 1);
                }
                catch (DirectoryNotFoundException ex)
                {
                    ProjectData.SetProjectError(ex);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DBC File : SpellRanges missing.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    ProjectData.ClearProjectError();
                }
            }
        }

        public async Task InitializeSpellShapeShiftAsync()
        {
            checked
            {
                try
                {
                    var tmpDBC = await dataStoreProvider.GetDataStoreAsync("SpellShapeshiftForm.dbc");
                    int num = tmpDBC.Rows - 1;
                    for (int i = 0; i <= num; i++)
                    {
                        int id = tmpDBC.ReadInt(i, 0);
                        int flags1 = tmpDBC.ReadInt(i, 11);
                        int creatureType = tmpDBC.ReadInt(i, 12);
                        int attackSpeed = tmpDBC.ReadInt(i, 13);
                        WorldServiceLocator._WS_DBCDatabase.SpellShapeShiftForm.Add(new WS_DBCDatabase.TSpellShapeshiftForm(id, flags1, creatureType, attackSpeed));
                    }
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SpellShapeshiftForms initialized.", tmpDBC.Rows - 1);
                }
                catch (DirectoryNotFoundException ex)
                {
                    ProjectData.SetProjectError(ex);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DBC File : SpellShapeshiftForms missing.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    ProjectData.ClearProjectError();
                }
            }
        }

        public async Task InitializeSpellFocusObjectAsync()
        {
            checked
            {
                try
                {
                    var tmpDBC = await dataStoreProvider.GetDataStoreAsync("SpellFocusObject.dbc");
                    int num = tmpDBC.Rows - 1;
                    for (int i = 0; i <= num; i++)
                    {
                        int spellFocusIndex = tmpDBC.ReadInt(i, 0);
                        string spellFocusObjectName = tmpDBC.ReadString(i, 1);
                        WorldServiceLocator._WS_Spells.SpellFocusObject[spellFocusIndex] = spellFocusObjectName;
                    }
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SpellFocusObjects initialized.", tmpDBC.Rows - 1);
                }
                catch (DirectoryNotFoundException ex)
                {
                    ProjectData.SetProjectError(ex);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DBC File : SpellFocusObjects missing.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    ProjectData.ClearProjectError();
                }
            }
        }

        public async Task InitializeSpellDurationAsync()
        {
            checked
            {
                try
                {
                    var tmpDBC = await dataStoreProvider.GetDataStoreAsync("SpellDuration.dbc");
                    int num = tmpDBC.Rows - 1;
                    for (int i = 0; i <= num; i++)
                    {
                        int spellDurationIndex = tmpDBC.ReadInt(i, 0);
                        int spellDurationValue = tmpDBC.ReadInt(i, 1);
                        WorldServiceLocator._WS_Spells.SpellDuration[spellDurationIndex] = spellDurationValue;
                    }
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SpellDurations initialized.", tmpDBC.Rows - 1);
                }
                catch (DirectoryNotFoundException ex)
                {
                    ProjectData.SetProjectError(ex);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DBC File : SpellDurations missing.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    ProjectData.ClearProjectError();
                }
            }
        }

        public async Task InitializeSpellsAsync()
        {
            checked
            {
                try
                {
                    var spellDBC = await dataStoreProvider.GetDataStoreAsync("Spell.dbc");
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: Initializing Spells - This may take a few moments....");
                    for (int i = 0; i <= spellDBC.Rows - 1; i++)
                    {
                        try
                        {
                            int id = spellDBC.ReadInt(i, 0);
                            WorldServiceLocator._WS_Spells.SPELLs[id] = new WS_Spells.SpellInfo
                            {
                                ID = id,
                                School = spellDBC.ReadInt(i, 1),
                                Category = spellDBC.ReadInt(i, 2),
                                DispellType = spellDBC.ReadInt(i, 4),
                                Mechanic = spellDBC.ReadInt(i, 5),
                                Attributes = spellDBC.ReadInt(i, 6),
                                AttributesEx = spellDBC.ReadInt(i, 7),
                                AttributesEx2 = spellDBC.ReadInt(i, 8),
                                RequredCasterStance = spellDBC.ReadInt(i, 11),
                                ShapeshiftExclude = spellDBC.ReadInt(i, 12),
                                Target = spellDBC.ReadInt(i, 13),
                                TargetCreatureType = spellDBC.ReadInt(i, 14),
                                FocusObjectIndex = spellDBC.ReadInt(i, 15),
                                CasterAuraState = spellDBC.ReadInt(i, 16),
                                TargetAuraState = spellDBC.ReadInt(i, 17),
                                SpellCastTimeIndex = spellDBC.ReadInt(i, 18),
                                SpellCooldown = spellDBC.ReadInt(i, 19),
                                CategoryCooldown = spellDBC.ReadInt(i, 20),
                                interruptFlags = spellDBC.ReadInt(i, 21),
                                auraInterruptFlags = spellDBC.ReadInt(i, 22),
                                channelInterruptFlags = spellDBC.ReadInt(i, 23),
                                procFlags = spellDBC.ReadInt(i, 24),
                                procChance = spellDBC.ReadInt(i, 25),
                                procCharges = spellDBC.ReadInt(i, 26),
                                maxLevel = spellDBC.ReadInt(i, 27),
                                baseLevel = spellDBC.ReadInt(i, 28),
                                spellLevel = spellDBC.ReadInt(i, 29),
                                DurationIndex = spellDBC.ReadInt(i, 30),
                                powerType = spellDBC.ReadInt(i, 31),
                                manaCost = spellDBC.ReadInt(i, 32),
                                manaCostPerlevel = spellDBC.ReadInt(i, 33),
                                manaPerSecond = spellDBC.ReadInt(i, 34),
                                manaPerSecondPerLevel = spellDBC.ReadInt(i, 35),
                                rangeIndex = spellDBC.ReadInt(i, 36),
                                Speed = spellDBC.ReadFloat(i, 37),
                                modalNextSpell = spellDBC.ReadInt(i, 38),
                                maxStack = spellDBC.ReadInt(i, 39)
                            };

                            WorldServiceLocator._WS_Spells.SPELLs[id].Totem[0] = spellDBC.ReadInt(i, 40);
                            WorldServiceLocator._WS_Spells.SPELLs[id].Totem[1] = spellDBC.ReadInt(i, 41);
                            WorldServiceLocator._WS_Spells.SPELLs[id].Reagents[0] = spellDBC.ReadInt(i, 42);
                            WorldServiceLocator._WS_Spells.SPELLs[id].Reagents[1] = spellDBC.ReadInt(i, 43);
                            WorldServiceLocator._WS_Spells.SPELLs[id].Reagents[2] = spellDBC.ReadInt(i, 44);
                            WorldServiceLocator._WS_Spells.SPELLs[id].Reagents[3] = spellDBC.ReadInt(i, 45);
                            WorldServiceLocator._WS_Spells.SPELLs[id].Reagents[4] = spellDBC.ReadInt(i, 46);
                            WorldServiceLocator._WS_Spells.SPELLs[id].Reagents[5] = spellDBC.ReadInt(i, 47);
                            WorldServiceLocator._WS_Spells.SPELLs[id].Reagents[6] = spellDBC.ReadInt(i, 48);
                            WorldServiceLocator._WS_Spells.SPELLs[id].Reagents[7] = spellDBC.ReadInt(i, 49);
                            WorldServiceLocator._WS_Spells.SPELLs[id].ReagentsCount[0] = spellDBC.ReadInt(i, 50);
                            WorldServiceLocator._WS_Spells.SPELLs[id].ReagentsCount[1] = spellDBC.ReadInt(i, 51);
                            WorldServiceLocator._WS_Spells.SPELLs[id].ReagentsCount[2] = spellDBC.ReadInt(i, 52);
                            WorldServiceLocator._WS_Spells.SPELLs[id].ReagentsCount[3] = spellDBC.ReadInt(i, 53);
                            WorldServiceLocator._WS_Spells.SPELLs[id].ReagentsCount[4] = spellDBC.ReadInt(i, 54);
                            WorldServiceLocator._WS_Spells.SPELLs[id].ReagentsCount[5] = spellDBC.ReadInt(i, 55);
                            WorldServiceLocator._WS_Spells.SPELLs[id].ReagentsCount[6] = spellDBC.ReadInt(i, 56);
                            WorldServiceLocator._WS_Spells.SPELLs[id].ReagentsCount[7] = spellDBC.ReadInt(i, 57);
                            WorldServiceLocator._WS_Spells.SPELLs[id].EquippedItemClass = spellDBC.ReadInt(i, 58);
                            WorldServiceLocator._WS_Spells.SPELLs[id].EquippedItemSubClass = spellDBC.ReadInt(i, 59);
                            WorldServiceLocator._WS_Spells.SPELLs[id].EquippedItemInventoryType = spellDBC.ReadInt(i, 60);

                            int k = 0;
                            do
                            {
                                if (spellDBC.ReadInt(i, 61 + k) != 0)
                                {
                                    WS_Spells.SpellEffect[] spellEffects = WorldServiceLocator._WS_Spells.SPELLs[id].SpellEffects;
                                    int num2 = k;
                                    Dictionary<int, WS_Spells.SpellInfo> sPELLs;
                                    int key;
                                    WS_Spells.SpellInfo Spell = (sPELLs = WorldServiceLocator._WS_Spells.SPELLs)[key = id];
                                    WS_Spells.SpellEffect spellEffect = new WS_Spells.SpellEffect(ref Spell);
                                    sPELLs[key] = Spell;
                                    WS_Spells.SpellEffect spellEffect2 = spellEffect;

                                    spellEffect2.ID = (SpellEffects_Names)spellDBC.ReadInt(i, 61 + k);
                                    spellEffect2.valueDie = spellDBC.ReadInt(i, 64 + k);
                                    spellEffect2.diceBase = spellDBC.ReadInt(i, 67 + k);
                                    spellEffect2.dicePerLevel = spellDBC.ReadFloat(i, 70 + k);
                                    spellEffect2.valuePerLevel = (int)spellDBC.ReadFloat(i, 73 + k);
                                    spellEffect2.valueBase = spellDBC.ReadInt(i, 76 + k);
                                    spellEffect2.Mechanic = spellDBC.ReadInt(i, 79 + k);
                                    spellEffect2.implicitTargetA = spellDBC.ReadInt(i, 82 + k);
                                    spellEffect2.implicitTargetB = spellDBC.ReadInt(i, 85 + k);
                                    spellEffect2.RadiusIndex = spellDBC.ReadInt(i, 88 + k);
                                    spellEffect2.ApplyAuraIndex = spellDBC.ReadInt(i, 91 + k);
                                    spellEffect2.Amplitude = spellDBC.ReadInt(i, 94 + k);
                                    spellEffect2.MultipleValue = spellDBC.ReadInt(i, 97 + k);
                                    spellEffect2.ChainTarget = spellDBC.ReadInt(i, 100 + k);
                                    spellEffect2.ItemType = spellDBC.ReadInt(i, 103 + k);
                                    spellEffect2.MiscValue = spellDBC.ReadInt(i, 106 + k);
                                    spellEffect2.TriggerSpell = spellDBC.ReadInt(i, 109 + k);
                                    spellEffect2.valuePerComboPoint = spellDBC.ReadInt(i, 112 + k);
                                    spellEffects[num2] = spellEffect2;
                                }
                                else
                                {
                                    WorldServiceLocator._WS_Spells.SPELLs[id].SpellEffects[k] = null;
                                }
                                k++;
                            }
                            while (k <= 2);
                            WorldServiceLocator._WS_Spells.SPELLs[id].SpellVisual = spellDBC.ReadInt(i, 115);
                            WorldServiceLocator._WS_Spells.SPELLs[id].SpellIconID = spellDBC.ReadInt(i, 117);
                            WorldServiceLocator._WS_Spells.SPELLs[id].ActiveIconID = spellDBC.ReadInt(i, 118);
                            WorldServiceLocator._WS_Spells.SPELLs[id].Name = spellDBC.ReadString(i, 120);
                            WorldServiceLocator._WS_Spells.SPELLs[id].Rank = spellDBC.ReadString(i, 129);
                            WorldServiceLocator._WS_Spells.SPELLs[id].manaCostPercent = spellDBC.ReadInt(i, 156);
                            WorldServiceLocator._WS_Spells.SPELLs[id].StartRecoveryCategory = spellDBC.ReadInt(i, 157);
                            WorldServiceLocator._WS_Spells.SPELLs[id].StartRecoveryTime = spellDBC.ReadInt(i, 158);
                            WorldServiceLocator._WS_Spells.SPELLs[id].AffectedTargetLevel = spellDBC.ReadInt(i, 159);
                            WorldServiceLocator._WS_Spells.SPELLs[id].SpellFamilyName = spellDBC.ReadInt(i, 160);
                            WorldServiceLocator._WS_Spells.SPELLs[id].SpellFamilyFlags = spellDBC.ReadInt(i, 161);
                            WorldServiceLocator._WS_Spells.SPELLs[id].MaxTargets = spellDBC.ReadInt(i, 163);
                            WorldServiceLocator._WS_Spells.SPELLs[id].DamageType = spellDBC.ReadInt(i, 164);
                            int j = 0;
                            do
                            {
                                if (WorldServiceLocator._WS_Spells.SPELLs[id].SpellEffects[j] != null)
                                    WorldServiceLocator._WS_Spells.SPELLs[id].SpellEffects[j].DamageMultiplier = spellDBC.ReadFloat(i, 167 + j);
                                j++;
                            }
                            while (j <= 2);
                            WorldServiceLocator._WS_Spells.SPELLs[id].InitCustomAttributes();
                        }
                        catch (Exception ex)
                        {
                            ProjectData.SetProjectError(ex);
                            Exception e2 = ex;
                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Line {0} caused error: {1}", i, e2.ToString());
                            ProjectData.ClearProjectError();
                        }
                    }
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} Spells initialized.", spellDBC.Rows - 1);
                }
                catch (DirectoryNotFoundException ex2)
                {
                    ProjectData.SetProjectError(ex2);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DBC File : Spells missing.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    ProjectData.ClearProjectError();
                }
            }
        }

        public void InitializeSpellChains()
        {
            try
            {
                DataTable spellChainQuery = new DataTable();
                WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT spell_id, prev_spell FROM spell_chain", ref spellChainQuery);
                IEnumerator enumerator = default;
                try
                {
                    enumerator = spellChainQuery.Rows.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        DataRow row = (DataRow)enumerator.Current;
                        WorldServiceLocator._WS_Spells.SpellChains.Add(row.As<int>("spell_id"), row.As<int>("prev_spell"));
                    }
                }
                finally
                {
                    if (enumerator is IDisposable)
                    {
                        (enumerator as IDisposable).Dispose();
                    }
                }
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Database: {0} SpellChains initialized.", spellChainQuery.Rows.Count);
            }
            catch (DirectoryNotFoundException ex)
            {
                ProjectData.SetProjectError(ex);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Database : SpellChains missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
                ProjectData.ClearProjectError();
            }
        }

        public async Task InitializeTaxiNodesAsync()
        {
            checked
            {
                try
                {
                    var tmpDBC = await dataStoreProvider.GetDataStoreAsync("TaxiNodes.dbc");
                    int num = tmpDBC.Rows - 1;
                    for (int i = 0; i <= num; i++)
                    {
                        int taxiNode = tmpDBC.ReadInt(i, 0);
                        int taxiMapID = tmpDBC.ReadInt(i, 1);
                        float taxiPosX = tmpDBC.ReadFloat(i, 2);
                        float taxiPosY = tmpDBC.ReadFloat(i, 3);
                        float taxiPosZ = tmpDBC.ReadFloat(i, 4);
                        int taxiMountTypeHorde = tmpDBC.ReadInt(i, 14);
                        int taxiMountTypeAlliance = tmpDBC.ReadInt(i, 15);

                        if (WorldServiceLocator._ConfigurationProvider.GetConfiguration().Maps.Contains(taxiMapID.ToString()))
                            WorldServiceLocator._WS_DBCDatabase.TaxiNodes.Add(taxiNode, new WS_DBCDatabase.TTaxiNode(taxiPosX, taxiPosY, taxiPosZ, taxiMapID, taxiMountTypeHorde, taxiMountTypeAlliance));
                    }
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} TaxiNodes initialized.", tmpDBC.Rows - 1);
                }
                catch (DirectoryNotFoundException ex)
                {
                    ProjectData.SetProjectError(ex);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DBC File : TaxiNodes missing.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    ProjectData.ClearProjectError();
                }
            }
        }

        public async Task InitializeTaxiPathsAsync()
        {
            checked
            {
                try
                {
                    var tmpDBC = await dataStoreProvider.GetDataStoreAsync("TaxiPath.dbc");
                    int num = tmpDBC.Rows - 1;
                    for (int i = 0; i <= num; i++)
                    {
                        int taxiNode = tmpDBC.ReadInt(i, 0);
                        int taxiFrom = tmpDBC.ReadInt(i, 1);
                        int taxiTo = tmpDBC.ReadInt(i, 2);
                        int taxiPrice = tmpDBC.ReadInt(i, 3);
                        WorldServiceLocator._WS_DBCDatabase.TaxiPaths.Add(taxiNode, new WS_DBCDatabase.TTaxiPath(taxiFrom, taxiTo, taxiPrice));
                    }
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} TaxiPaths initialized.", tmpDBC.Rows - 1);
                }
                catch (DirectoryNotFoundException ex)
                {
                    ProjectData.SetProjectError(ex);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DBC File : TaxiPath missing.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    ProjectData.ClearProjectError();
                }
            }
        }

        public async Task InitializeTaxiPathNodesAsync()
        {
            checked
            {
                try
                {
                    var tmpDBC = await dataStoreProvider.GetDataStoreAsync("TaxiPathNode.dbc");
                    int num = tmpDBC.Rows - 1;
                    for (int i = 0; i <= num; i++)
                    {
                        int taxiPath = tmpDBC.ReadInt(i, 1);
                        int taxiSeq = tmpDBC.ReadInt(i, 2);
                        int taxiMapID = tmpDBC.ReadInt(i, 3);
                        float taxiPosX = tmpDBC.ReadFloat(i, 4);
                        float taxiPosY = tmpDBC.ReadFloat(i, 5);
                        float taxiPosZ = tmpDBC.ReadFloat(i, 6);
                        int taxiAction = tmpDBC.ReadInt(i, 7);
                        int taxiWait = tmpDBC.ReadInt(i, 8);
                        if (WorldServiceLocator._ConfigurationProvider.GetConfiguration().Maps.Contains(taxiMapID.ToString()))
                        {
                            if (!WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes.ContainsKey(taxiPath))
                            {
                                WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes.Add(taxiPath, new Dictionary<int, WS_DBCDatabase.TTaxiPathNode>());
                            }
                            WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes[taxiPath].Add(taxiSeq, new WS_DBCDatabase.TTaxiPathNode(taxiPosX, taxiPosY, taxiPosZ, taxiMapID, taxiPath, taxiSeq, taxiAction, taxiWait));
                        }
                    }
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} TaxiPathNodes initialized.", tmpDBC.Rows - 1);
                }
                catch (DirectoryNotFoundException ex)
                {
                    ProjectData.SetProjectError(ex);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DBC File : TaxiPathNode missing.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    ProjectData.ClearProjectError();
                }
            }
        }

        public async Task InitializeSkillLinesAsync()
        {
            checked
            {
                try
                {
                    var tmpDBC = await dataStoreProvider.GetDataStoreAsync("SkillLine.dbc");
                    int num = tmpDBC.Rows - 1;
                    for (int i = 0; i <= num; i++)
                    {
                        int skillID = tmpDBC.ReadInt(i, 0);
                        int skillLine = tmpDBC.ReadInt(i, 1);
                        WorldServiceLocator._WS_DBCDatabase.SkillLines[skillID] = skillLine;
                    }
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SkillLines initialized.", tmpDBC.Rows - 1);
                }
                catch (DirectoryNotFoundException ex)
                {
                    ProjectData.SetProjectError(ex);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DBC File : SkillLines missing.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    ProjectData.ClearProjectError();
                }
            }
        }

        public async Task InitializeSkillLineAbilityAsync()
        {
            checked
            {
                try
                {
                    var tmpDBC = await dataStoreProvider.GetDataStoreAsync("SkillLineAbility.dbc");
                    int num = tmpDBC.Rows - 1;
                    for (int i = 0; i <= num; i++)
                    {
                        WS_DBCDatabase.TSkillLineAbility tmpSkillLineAbility = new WS_DBCDatabase.TSkillLineAbility
                        {
                            ID = tmpDBC.ReadInt(i, 0),
                            SkillID = tmpDBC.ReadInt(i, 1),
                            SpellID = tmpDBC.ReadInt(i, 2),
                            Unknown1 = tmpDBC.ReadInt(i, 3),
                            Unknown2 = tmpDBC.ReadInt(i, 4),
                            Unknown3 = tmpDBC.ReadInt(i, 5),
                            Unknown4 = tmpDBC.ReadInt(i, 6),
                            Required_Skill_Value = tmpDBC.ReadInt(i, 7),
                            Forward_SpellID = tmpDBC.ReadInt(i, 8),
                            Unknown5 = tmpDBC.ReadInt(i, 9),
                            Max_Value = tmpDBC.ReadInt(i, 10),
                            Min_Value = tmpDBC.ReadInt(i, 11)
                        };
                        WorldServiceLocator._WS_DBCDatabase.SkillLineAbility.Add(tmpSkillLineAbility.ID, tmpSkillLineAbility);
                    }
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SkillLineAbilitys initialized.", tmpDBC.Rows - 1);
                }
                catch (DirectoryNotFoundException ex)
                {
                    ProjectData.SetProjectError(ex);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DBC File : SkillLineAbility missing.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    ProjectData.ClearProjectError();
                }
            }
        }

        public async Task InitializeLocksAsync()
        {
            checked
            {
                try
                {
                    var tmpDBC = await dataStoreProvider.GetDataStoreAsync("Lock.dbc");
                    byte[] keyType = new byte[5];
                    int[] key = new int[5];
                    int num = tmpDBC.Rows - 1;
                    for (int i = 0; i <= num; i++)
                    {
                        int lockID = tmpDBC.ReadInt(i, 0);
                        keyType[0] = (byte)tmpDBC.ReadInt(i, 1);
                        keyType[1] = (byte)tmpDBC.ReadInt(i, 2);
                        keyType[2] = (byte)tmpDBC.ReadInt(i, 3);
                        keyType[3] = (byte)tmpDBC.ReadInt(i, 4);
                        keyType[4] = (byte)tmpDBC.ReadInt(i, 5);
                        key[0] = tmpDBC.ReadInt(i, 9);
                        key[1] = tmpDBC.ReadInt(i, 10);
                        key[2] = tmpDBC.ReadInt(i, 11);
                        key[3] = tmpDBC.ReadInt(i, 12);
                        key[4] = tmpDBC.ReadInt(i, 13);
                        int reqMining = tmpDBC.ReadInt(i, 17);
                        int reqLockSkill = tmpDBC.ReadInt(i, 17);
                        WorldServiceLocator._WS_Loot.Locks[lockID] = new WS_Loot.TLock(keyType, key, (short)reqMining, (short)reqLockSkill);
                    }
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} Locks initialized.", tmpDBC.Rows - 1);
                }
                catch (DirectoryNotFoundException ex)
                {
                    ProjectData.SetProjectError(ex);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DBC File : Locks missing.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    ProjectData.ClearProjectError();
                }
            }
        }

        public async Task InitializeAreaTableAsync()
        {
            checked
            {
                try
                {
                    var tmpDBC = await dataStoreProvider.GetDataStoreAsync("AreaTable.dbc");
                    int num = tmpDBC.Rows - 1;
                    for (int i = 0; i <= num; i++)
                    {
                        int areaID = tmpDBC.ReadInt(i, 0);
                        int areaMapID = tmpDBC.ReadInt(i, 1);
                        int areaZone = tmpDBC.ReadInt(i, 2);
                        int areaExploreFlag = tmpDBC.ReadInt(i, 3);
                        int areaZoneType = tmpDBC.ReadInt(i, 4);
                        int areaLevel = tmpDBC.ReadInt(i, 10);

                        if (areaLevel > 255)
                            areaLevel = 255;
                        if (areaLevel < 0)
                            areaLevel = 0;

                        WorldServiceLocator._WS_Maps.AreaTable[areaExploreFlag] = new WS_Maps.TArea
                        {
                            ID = areaID,
                            mapId = areaMapID,
                            Level = (byte)areaLevel,
                            Zone = areaZone,
                            ZoneType = areaZoneType
                        };
                    }
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} Areas initialized.", tmpDBC.Rows - 1);
                }
                catch (DirectoryNotFoundException ex)
                {
                    ProjectData.SetProjectError(ex);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DBC File : AreaTable missing.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    ProjectData.ClearProjectError();
                }
            }
        }

        public async Task InitializeEmotesAsync()
        {
            checked
            {
                try
                {
                    var tmpDBC = await dataStoreProvider.GetDataStoreAsync("Emotes.dbc");
                    int num = tmpDBC.Rows - 1;
                    for (int i = 0; i <= num; i++)
                    {
                        int emoteID = tmpDBC.ReadInt(i, 0);
                        int emoteState = tmpDBC.ReadInt(i, 4);

                        if (emoteID != 0)
                            WorldServiceLocator._WS_DBCDatabase.EmotesState[emoteID] = emoteState;
                    }
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} Emotes initialized.", tmpDBC.Rows - 1);
                }
                catch (DirectoryNotFoundException ex)
                {
                    ProjectData.SetProjectError(ex);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DBC File : Emotes missing.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    ProjectData.ClearProjectError();
                }
            }
        }

        public async Task InitializeEmotesTextAsync()
        {
            checked
            {
                try
                {
                    var tmpDBC = await dataStoreProvider.GetDataStoreAsync("EmotesText.dbc");
                    int num = tmpDBC.Rows - 1;
                    for (int i = 0; i <= num; i++)
                    {
                        int textEmoteID = tmpDBC.ReadInt(i, 0);
                        int emoteID = tmpDBC.ReadInt(i, 2);

                        if (emoteID != 0)
                            WorldServiceLocator._WS_DBCDatabase.EmotesText[textEmoteID] = emoteID;
                    }
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} EmotesText initialized.", tmpDBC.Rows - 1);
                }
                catch (DirectoryNotFoundException ex)
                {
                    ProjectData.SetProjectError(ex);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DBC File : EmotesText missing.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    ProjectData.ClearProjectError();
                }
            }
        }

        public async Task InitializeFactionsAsync()
        {
            checked
            {
                try
                {
                    var tmpDBC = await dataStoreProvider.GetDataStoreAsync("Faction.dbc");
                    int[] flags = new int[4];
                    int[] reputationStats = new int[4];
                    int[] reputationFlags = new int[4];
                    int num = tmpDBC.Rows - 1;
                    for (int i = 0; i <= num; i++)
                    {
                        int factionID = tmpDBC.ReadInt(i, 0);
                        int factionFlag = tmpDBC.ReadInt(i, 1);
                        flags[0] = tmpDBC.ReadInt(i, 2);
                        flags[1] = tmpDBC.ReadInt(i, 3);
                        flags[2] = tmpDBC.ReadInt(i, 4);
                        flags[3] = tmpDBC.ReadInt(i, 5);
                        reputationStats[0] = tmpDBC.ReadInt(i, 10);
                        reputationStats[1] = tmpDBC.ReadInt(i, 11);
                        reputationStats[2] = tmpDBC.ReadInt(i, 12);
                        reputationStats[3] = tmpDBC.ReadInt(i, 13);
                        reputationFlags[0] = tmpDBC.ReadInt(i, 14);
                        reputationFlags[1] = tmpDBC.ReadInt(i, 15);
                        reputationFlags[2] = tmpDBC.ReadInt(i, 16);
                        reputationFlags[3] = tmpDBC.ReadInt(i, 17);
                        WorldServiceLocator._WS_DBCDatabase.FactionInfo[factionID] = new WS_DBCDatabase.TFaction((short)factionID, (short)factionFlag, flags[0], flags[1], flags[2], flags[3], reputationStats[0], reputationStats[1], reputationStats[2], reputationStats[3], reputationFlags[0], reputationFlags[1], reputationFlags[2], reputationFlags[3]);
                    }
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} Factions initialized.", tmpDBC.Rows - 1);
                }
                catch (DirectoryNotFoundException ex)
                {
                    ProjectData.SetProjectError(ex);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DBC File : Factions missing.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    ProjectData.ClearProjectError();
                }
            }
        }

        public async Task InitializeFactionTemplatesAsync()
        {
            checked
            {
                try
                {
                    var tmpDBC = await dataStoreProvider.GetDataStoreAsync("FactionTemplate.dbc");
                    int num = tmpDBC.Rows - 1;
                    for (int i = 0; i <= num; i++)
                    {
                        int templateID = tmpDBC.ReadInt(i, 0);
                        WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo.Add(templateID, new WS_DBCDatabase.TFactionTemplate());
                        WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].FactionID = tmpDBC.ReadInt(i, 1);
                        WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].ourMask = (uint)tmpDBC.ReadInt(i, 3);
                        WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].friendMask = (uint)tmpDBC.ReadInt(i, 4);
                        WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].enemyMask = (uint)tmpDBC.ReadInt(i, 5);
                        WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].enemyFaction1 = tmpDBC.ReadInt(i, 6);
                        WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].enemyFaction2 = tmpDBC.ReadInt(i, 7);
                        WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].enemyFaction3 = tmpDBC.ReadInt(i, 8);
                        WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].enemyFaction4 = tmpDBC.ReadInt(i, 9);
                        WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].friendFaction1 = tmpDBC.ReadInt(i, 10);
                        WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].friendFaction2 = tmpDBC.ReadInt(i, 11);
                        WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].friendFaction3 = tmpDBC.ReadInt(i, 12);
                        WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].friendFaction4 = tmpDBC.ReadInt(i, 13);
                    }
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} FactionTemplates initialized.", tmpDBC.Rows - 1);
                }
                catch (DirectoryNotFoundException ex)
                {
                    ProjectData.SetProjectError(ex);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DBC File : FactionsTemplates missing.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    ProjectData.ClearProjectError();
                }
            }
        }

        public async Task InitializeCharRacesAsync()
        {
            checked
            {
                try
                {
                    var tmpDBC = await dataStoreProvider.GetDataStoreAsync("ChrRaces.dbc");
                    int num = tmpDBC.Rows - 1;
                    for (int i = 0; i <= num; i++)
                    {
                        int raceID = tmpDBC.ReadInt(i, 0);
                        int factionID = tmpDBC.ReadInt(i, 2);
                        int modelM = tmpDBC.ReadInt(i, 4);
                        int modelF = tmpDBC.ReadInt(i, 5);
                        int teamID = tmpDBC.ReadInt(i, 8);
                        uint taxiMask = (uint)tmpDBC.ReadInt(i, 14);
                        int cinematicID = tmpDBC.ReadInt(i, 16);
                        string name = tmpDBC.ReadString(i, 17);
                        WorldServiceLocator._WS_DBCDatabase.CharRaces[(byte)raceID] = new WS_DBCDatabase.TCharRace((short)factionID, modelM, modelF, (byte)teamID, taxiMask, cinematicID, name);
                    }
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} CharRaces initialized.", tmpDBC.Rows - 1);
                }
                catch (DirectoryNotFoundException ex)
                {
                    ProjectData.SetProjectError(ex);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DBC File : CharRaces missing.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    ProjectData.ClearProjectError();
                }
            }
        }

        public async Task InitializeCharClassesAsync()
        {
            checked
            {
                try
                {
                    var tmpDBC = await dataStoreProvider.GetDataStoreAsync("ChrClasses.dbc");
                    int num = tmpDBC.Rows - 1;
                    for (int i = 0; i <= num; i++)
                    {
                        int classID = tmpDBC.ReadInt(i, 0);
                        int cinematicID = tmpDBC.ReadInt(i, 5);
                        WorldServiceLocator._WS_DBCDatabase.CharClasses[(byte)classID] = new WS_DBCDatabase.TCharClass(cinematicID);
                    }
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} CharClasses initialized.", tmpDBC.Rows - 1);
                }
                catch (DirectoryNotFoundException ex)
                {
                    ProjectData.SetProjectError(ex);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DBC File : CharRaces missing.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    ProjectData.ClearProjectError();
                }
            }
        }

        public async Task InitializeDurabilityCostsAsync()
        {
            checked
            {
                try
                {
                    var tmpDBC = await dataStoreProvider.GetDataStoreAsync("DurabilityCosts.dbc");
                    int num = tmpDBC.Rows - 1;
                    for (int i = 0; i <= num; i++)
                    {
                        int itemBroken = tmpDBC.ReadInt(i, 0);
                        int num2 = tmpDBC.Columns - 1;
                        for (int itemType = 1; itemType <= num2; itemType++)
                        {
                            int itemPrice = tmpDBC.ReadInt(i, itemType);
                            WorldServiceLocator._WS_DBCDatabase.DurabilityCosts[itemBroken, itemType - 1] = (short)itemPrice;
                        }
                    }
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} DurabilityCosts initialized.", tmpDBC.Rows - 1);
                }
                catch (DirectoryNotFoundException ex)
                {
                    ProjectData.SetProjectError(ex);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DBC File : DurabilityCosts missing.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    ProjectData.ClearProjectError();
                }
            }
        }

        public async Task LoadTalentDbcAsync()
        {
            checked
            {
                try
                {
                    var dbc = await dataStoreProvider.GetDataStoreAsync("Talent.dbc");
                    int num = dbc.Rows - 1;
                    for (int i = 0; i <= num; i++)
                    {
                        WS_DBCDatabase.TalentInfo tmpInfo = new WS_DBCDatabase.TalentInfo
                        {
                            TalentID = dbc.ReadInt(i, 0),
                            TalentTab = dbc.ReadInt(i, 1),
                            Row = dbc.ReadInt(i, 2),
                            Col = dbc.ReadInt(i, 3)
                        };
                        tmpInfo.RankID[0] = dbc.ReadInt(i, 4);
                        tmpInfo.RankID[1] = dbc.ReadInt(i, 5);
                        tmpInfo.RankID[2] = dbc.ReadInt(i, 6);
                        tmpInfo.RankID[3] = dbc.ReadInt(i, 7);
                        tmpInfo.RankID[4] = dbc.ReadInt(i, 8);
                        tmpInfo.RequiredTalent[0] = dbc.ReadInt(i, 13);
                        tmpInfo.RequiredPoints[0] = dbc.ReadInt(i, 16);
                        WorldServiceLocator._WS_DBCDatabase.Talents.Add(tmpInfo.TalentID, tmpInfo);
                    }
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} Talents initialized.", dbc.Rows - 1);
                }
                catch (DirectoryNotFoundException ex)
                {
                    ProjectData.SetProjectError(ex);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DBC File : Talents missing.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    ProjectData.ClearProjectError();
                }
            }
        }

        public async Task LoadTalentTabDbcAsync()
        {
            checked
            {
                try
                {
                    var dbc = await dataStoreProvider.GetDataStoreAsync("TalentTab.dbc");
                    int num = dbc.Rows - 1;
                    for (int i = 0; i <= num; i++)
                    {
                        int talentTab = dbc.ReadInt(i, 0);
                        int talentMask = dbc.ReadInt(i, 12);
                        WorldServiceLocator._WS_DBCDatabase.TalentsTab.Add(talentTab, talentMask);
                    }
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} Talent tabs initialized.", dbc.Rows - 1);
                }
                catch (DirectoryNotFoundException ex)
                {
                    ProjectData.SetProjectError(ex);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DBC File : TalentTab missing.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    ProjectData.ClearProjectError();
                }
            }
        }

        public async Task LoadAuctionHouseDbcAsync()
        {
            checked
            {
                try
                {
                    var dbc = await dataStoreProvider.GetDataStoreAsync("AuctionHouse.dbc");
                    int num = dbc.Rows - 1;
                    for (int i = 0; i <= num; i++)
                    {
                        int ahId = dbc.ReadInt(i, 0);
                        int fee = dbc.ReadInt(i, 2);
                        int tax = dbc.ReadInt(i, 3);
                        WorldServiceLocator._WS_Auction.AuctionID = ahId;
                        WorldServiceLocator._WS_Auction.AuctionFee = fee;
                        WorldServiceLocator._WS_Auction.AuctionTax = tax;
                    }
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} AuctionHouses initialized.", dbc.Rows - 1);
                }
                catch (DirectoryNotFoundException ex)
                {
                    ProjectData.SetProjectError(ex);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DBC File : AuctionHouse missing.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    ProjectData.ClearProjectError();
                }
            }
        }

        public async Task LoadSpellItemEnchantmentsAsync()
        {
            checked
            {
                try
                {
                    var dbc = await dataStoreProvider.GetDataStoreAsync("SpellItemEnchantment.dbc");
                    int[] type = new int[3];
                    int[] amount = new int[3];
                    int[] spellID = new int[3];
                    int num = dbc.Rows - 1;
                    for (int i = 0; i <= num; i++)
                    {
                        int id = dbc.ReadInt(i, 0);
                        type[0] = dbc.ReadInt(i, 1);
                        type[1] = dbc.ReadInt(i, 2);
                        amount[0] = dbc.ReadInt(i, 4);
                        amount[1] = dbc.ReadInt(i, 7);
                        spellID[0] = dbc.ReadInt(i, 10);
                        spellID[1] = dbc.ReadInt(i, 11);
                        int auraID = dbc.ReadInt(i, 22);
                        int slot = dbc.ReadInt(i, 23);
                        WorldServiceLocator._WS_DBCDatabase.SpellItemEnchantments.Add(id, new WS_DBCDatabase.TSpellItemEnchantment(type, amount, spellID, auraID, slot));
                    }
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SpellItemEnchantments initialized.", dbc.Rows - 1);
                }
                catch (DirectoryNotFoundException ex)
                {
                    ProjectData.SetProjectError(ex);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DBC File : SpellItemEnchantments missing.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    ProjectData.ClearProjectError();
                }
            }
        }

        public async Task LoadItemSetAsync()
        {
            checked
            {
                try
                {
                    var dbc = await dataStoreProvider.GetDataStoreAsync("ItemSet.dbc");
                    int[] itemID = new int[8];
                    int[] spellID = new int[8];
                    int[] itemCount = new int[8];
                    int num = dbc.Rows - 1;
                    int requiredSkillID = default;
                    int requiredSkillValue = default;
                    for (int i = 0; i <= num; i++)
                    {
                        int id = dbc.ReadInt(i, 0);
                        string name = dbc.ReadString(i, 1);
                        itemID[0] = dbc.ReadInt(i, 10);
                        itemID[1] = dbc.ReadInt(i, 11);
                        itemID[2] = dbc.ReadInt(i, 12);
                        itemID[3] = dbc.ReadInt(i, 13);
                        itemID[4] = dbc.ReadInt(i, 14);
                        itemID[5] = dbc.ReadInt(i, 15);
                        itemID[6] = dbc.ReadInt(i, 16);
                        itemID[7] = dbc.ReadInt(i, 17);
                        WorldServiceLocator._WS_DBCDatabase.ItemSet.Add(id, new WS_DBCDatabase.TItemSet(name, itemID, spellID, itemCount, requiredSkillID, requiredSkillValue));
                    }
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} ItemSets initialized.", dbc.Rows - 1);
                }
                catch (DirectoryNotFoundException ex)
                {
                    ProjectData.SetProjectError(ex);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DBC File : ItemSet missing.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    ProjectData.ClearProjectError();
                }
            }
        }

        public async Task LoadItemDisplayInfoDbcAsync()
        {
            checked
            {
                try
                {
                    var dbc = await dataStoreProvider.GetDataStoreAsync("ItemDisplayInfo.dbc");
                    int num = dbc.Rows - 1;
                    for (int i = 0; i <= num; i++)
                    {
                        WS_DBCDatabase.TItemDisplayInfo tmpItemDisplayInfo = new WS_DBCDatabase.TItemDisplayInfo
                        {
                            ID = dbc.ReadInt(i, 0),
                            RandomPropertyChance = dbc.ReadInt(i, 11),
                            Unknown = dbc.ReadInt(i, 22)
                        };
                        WorldServiceLocator._WS_DBCDatabase.ItemDisplayInfo.Add(tmpItemDisplayInfo.ID, tmpItemDisplayInfo);
                    }
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} ItemDisplayInfos initialized.", dbc.Rows - 1);
                }
                catch (DirectoryNotFoundException ex)
                {
                    ProjectData.SetProjectError(ex);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DBC File : ItemDisplayInfo missing.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    ProjectData.ClearProjectError();
                }
            }
        }

        public async Task LoadItemRandomPropertiesDbcAsync()
        {
            checked
            {
                try
                {
                    var dbc = await dataStoreProvider.GetDataStoreAsync("ItemRandomProperties.dbc");
                    int num = dbc.Rows - 1;
                    for (int i = 0; i <= num; i++)
                    {
                        WS_DBCDatabase.TItemRandomPropertiesInfo tmpInfo = new WS_DBCDatabase.TItemRandomPropertiesInfo
                        {
                            ID = dbc.ReadInt(i, 0)
                        };
                        tmpInfo.Enchant_ID[0] = dbc.ReadInt(i, 2);
                        tmpInfo.Enchant_ID[1] = dbc.ReadInt(i, 3);
                        tmpInfo.Enchant_ID[2] = dbc.ReadInt(i, 4);
                        WorldServiceLocator._WS_DBCDatabase.ItemRandomPropertiesInfo.Add(tmpInfo.ID, tmpInfo);
                    }
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} ItemRandomProperties initialized.", dbc.Rows - 1);
                }
                catch (DirectoryNotFoundException ex)
                {
                    ProjectData.SetProjectError(ex);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DBC File : ItemRandomProperties missing.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    ProjectData.ClearProjectError();
                }
            }
        }

        public async Task LoadCreatureGossipAsync()
        {
            try
            {
                DataTable gossipQuery = new DataTable();
                WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT * FROM npc_gossip;", ref gossipQuery);
                IEnumerator enumerator = default;
                try
                {
                    enumerator = gossipQuery.Rows.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        DataRow row = (DataRow)enumerator.Current;
                        ulong guid = row.As<ulong>("npc_guid");
                        if (!WorldServiceLocator._WS_DBCDatabase.CreatureGossip.ContainsKey(guid))
                        {
                            WorldServiceLocator._WS_DBCDatabase.CreatureGossip.Add(guid, row.As<int>("textid"));
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
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Database: {0} creature gossips initialized.", WorldServiceLocator._WS_DBCDatabase.CreatureGossip.Count);
            }
            catch (DirectoryNotFoundException ex)
            {
                ProjectData.SetProjectError(ex);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Database : npc_gossip missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
                ProjectData.ClearProjectError();
            }
        }

        public async Task LoadCreatureFamilyDbcAsync()
        {
            checked
            {
                try
                {
                    var dbc = await dataStoreProvider.GetDataStoreAsync("CreatureFamily.dbc");
                    int num = dbc.Rows - 1;
                    for (int i = 0; i <= num; i++)
                    {
                        WS_DBCDatabase.CreatureFamilyInfo tmpInfo = new WS_DBCDatabase.CreatureFamilyInfo
                        {
                            ID = dbc.ReadInt(i, 0),
                            Unknown1 = dbc.ReadInt(i, 5),
                            Unknown2 = dbc.ReadInt(i, 6),
                            PetFoodID = dbc.ReadInt(i, 7),
                            Name = dbc.ReadString(i, 12)
                        };
                        WorldServiceLocator._WS_DBCDatabase.CreaturesFamily.Add(tmpInfo.ID, tmpInfo);
                    }
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} CreatureFamilys initialized.", dbc.Rows - 1);
                }
                catch (DirectoryNotFoundException ex)
                {
                    ProjectData.SetProjectError(ex);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DBC File : CreatureFamily missing.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    ProjectData.ClearProjectError();
                }
            }
        }

        public void LoadCreatureMovements()
        {
            try
            {
                DataTable movementsQuery = new DataTable();
                WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT * FROM waypoint_data ORDER BY id, point;", ref movementsQuery);
                IEnumerator enumerator = default;
                try
                {
                    enumerator = movementsQuery.Rows.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        DataRow row = (DataRow)enumerator.Current;
                        int id = row.As<int>("id");
                        if (!WorldServiceLocator._WS_DBCDatabase.CreatureMovement.ContainsKey(id))
                        {
                            WorldServiceLocator._WS_DBCDatabase.CreatureMovement.Add(id, new Dictionary<int, WS_DBCDatabase.CreatureMovePoint>());
                        }
                        WorldServiceLocator._WS_DBCDatabase.CreatureMovement[id].Add(row.As<int>("point"), new WS_DBCDatabase.CreatureMovePoint(row.As<float>("position_x"), row.As<float>("position_y"), row.As<float>("position_z"), row.As<int>("delay"), row.As<int>("move_flag"), row.As<int>("action"), row.As<int>("action_chance")));
                    }
                }
                finally
                {
                    if (enumerator is IDisposable)
                    {
                        (enumerator as IDisposable).Dispose();
                    }
                }
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Database: {0} creature movements for {1} creatures initialized.", movementsQuery.Rows.Count, WorldServiceLocator._WS_DBCDatabase.CreatureMovement.Count);
            }
            catch (DirectoryNotFoundException ex)
            {
                ProjectData.SetProjectError(ex);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Database : Waypoint_Data missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
                ProjectData.ClearProjectError();
            }
        }

        public void LoadCreatureEquipTable()
        {
            try
            {
                DataTable equipQuery = new DataTable();
                WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT * FROM creature_equip_template_raw;", ref equipQuery);
                IEnumerator enumerator = default;
                try
                {
                    enumerator = equipQuery.Rows.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        DataRow row = (DataRow)enumerator.Current;
                        int entry = row.As<int>("entry");
                        if (!WorldServiceLocator._WS_DBCDatabase.CreatureEquip.ContainsKey(entry))
                        {
                            try
                            {
                                WorldServiceLocator._WS_DBCDatabase.CreatureEquip.Add(entry, new WS_DBCDatabase.CreatureEquipInfo(row.As<int>("equipmodel1"), row.As<int>("equipmodel2"), row.As<int>("equipmodel3"), row.As<uint>("equipinfo1"), row.As<uint>("equipinfo2"), row.As<uint>("equipinfo3"), row.As<int>("equipslot1"), row.As<int>("equipslot2"), row.As<int>("equipslot3")));
                            }
                            catch (DataException ex)
                            {
                                ProjectData.SetProjectError(ex);
                                DataException e2 = ex;
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Creature_Equip_Template_raw : Unable to equip items {entry} for Creature ");
                                Console.ForegroundColor = ConsoleColor.Gray;
                                ProjectData.ClearProjectError();
                            }
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
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Database: {0} creature equips initialized.", equipQuery.Rows.Count);
            }
            catch (DataException ex2)
            {
                ProjectData.SetProjectError(ex2);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Database : Creature_Equip_Template_raw missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
                ProjectData.ClearProjectError();
            }
        }

        public void LoadCreatureModelInfo()
        {
            try
            {
                DataTable modelQuery = new DataTable();
                WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT * FROM creature_model_info;", ref modelQuery);
                IEnumerator enumerator = default;
                try
                {
                    enumerator = modelQuery.Rows.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        DataRow row = (DataRow)enumerator.Current;
                        int entry = row.As<int>("modelid");
                        if (!WorldServiceLocator._WS_DBCDatabase.CreatureModel.ContainsKey(entry))
                        {
                            WorldServiceLocator._WS_DBCDatabase.CreatureModel.Add(entry, new WS_DBCDatabase.CreatureModelInfo(row.As<float>("bounding_radius"), row.As<float>("combat_reach"), row.As<byte>("gender"), row.As<int>("modelid_other_gender")));
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
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Database: {0} creature models initialized.", modelQuery.Rows.Count);
            }
            catch (DirectoryNotFoundException ex)
            {
                ProjectData.SetProjectError(ex);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Database : Creature_Model_Info missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
                ProjectData.ClearProjectError();
            }
        }

        public void LoadQuestStartersAndFinishers()
        {
            DataTable questStarters = new DataTable();
            WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT * FROM quest_relations where actor=0 and role =0;", ref questStarters);
            IEnumerator enumerator = default;
            try
            {
                enumerator = questStarters.Rows.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    DataRow row = (DataRow)enumerator.Current;
                    int entry4 = row.As<int>("entry");
                    int quest4 = row.As<int>("quest");
                    if (!WorldServiceLocator._WorldServer.CreatureQuestStarters.ContainsKey(entry4))
                    {
                        WorldServiceLocator._WorldServer.CreatureQuestStarters.Add(entry4, new List<int>());
                    }
                    WorldServiceLocator._WorldServer.CreatureQuestStarters[entry4].Add(quest4);
                }
            }
            finally
            {
                if (enumerator is IDisposable)
                {
                    (enumerator as IDisposable).Dispose();
                }
            }
            int questStartersAmount = questStarters.Rows.Count;
            questStarters.Clear();
            WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT * FROM quest_relations where actor=1 and role=0;", ref questStarters);
            IEnumerator enumerator2 = default;
            try
            {
                enumerator2 = questStarters.Rows.GetEnumerator();
                while (enumerator2.MoveNext())
                {
                    DataRow row = (DataRow)enumerator2.Current;
                    int entry3 = row.As<int>("entry");
                    int quest3 = row.As<int>("quest");
                    if (!WorldServiceLocator._WorldServer.GameobjectQuestStarters.ContainsKey(entry3))
                    {
                        WorldServiceLocator._WorldServer.GameobjectQuestStarters.Add(entry3, new List<int>());
                    }
                    WorldServiceLocator._WorldServer.GameobjectQuestStarters[entry3].Add(quest3);
                }
            }
            finally
            {
                if (enumerator2 is IDisposable)
                {
                    (enumerator2 as IDisposable).Dispose();
                }
            }
            checked
            {
                questStartersAmount += questStarters.Rows.Count;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Database: {0} queststarters initated for {1} creatures and {2} gameobjects.", questStartersAmount, WorldServiceLocator._WorldServer.CreatureQuestStarters.Count, WorldServiceLocator._WorldServer.GameobjectQuestStarters.Count);
                questStarters.Clear();
                DataTable questFinishers = new DataTable();
                WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT * FROM quest_relations where actor=0 and role=1;", ref questFinishers);
                IEnumerator enumerator3 = default;
                try
                {
                    enumerator3 = questFinishers.Rows.GetEnumerator();
                    while (enumerator3.MoveNext())
                    {
                        DataRow row = (DataRow)enumerator3.Current;
                        int entry2 = row.As<int>("entry");
                        int quest2 = row.As<int>("quest");
                        if (!WorldServiceLocator._WorldServer.CreatureQuestFinishers.ContainsKey(entry2))
                        {
                            WorldServiceLocator._WorldServer.CreatureQuestFinishers.Add(entry2, new List<int>());
                        }
                        WorldServiceLocator._WorldServer.CreatureQuestFinishers[entry2].Add(quest2);
                    }
                }
                finally
                {
                    if (enumerator3 is IDisposable)
                    {
                        (enumerator3 as IDisposable).Dispose();
                    }
                }
                int questFinishersAmount = questFinishers.Rows.Count;
                questFinishers.Clear();
                WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT * FROM quest_relations where actor=1 and role=1;", ref questFinishers);
                IEnumerator enumerator4 = default;
                try
                {
                    enumerator4 = questFinishers.Rows.GetEnumerator();
                    while (enumerator4.MoveNext())
                    {
                        DataRow row = (DataRow)enumerator4.Current;
                        int entry = row.As<int>("entry");
                        int quest = row.As<int>("quest");
                        if (!WorldServiceLocator._WorldServer.GameobjectQuestFinishers.ContainsKey(entry))
                        {
                            WorldServiceLocator._WorldServer.GameobjectQuestFinishers.Add(entry, new List<int>());
                        }
                        WorldServiceLocator._WorldServer.GameobjectQuestFinishers[entry].Add(quest);
                    }
                }
                finally
                {
                    if (enumerator4 is IDisposable)
                    {
                        (enumerator4 as IDisposable).Dispose();
                    }
                }
                questFinishersAmount += questFinishers.Rows.Count;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Database: {0} questfinishers initated for {1} creatures and {2} gameobjects.", questFinishersAmount, WorldServiceLocator._WorldServer.CreatureQuestFinishers.Count, WorldServiceLocator._WorldServer.GameobjectQuestFinishers.Count);
                questFinishers.Clear();
            }
        }

        public void LoadLootStores()
        {
            WorldServiceLocator._WS_Loot.LootTemplates_Creature = new WS_Loot.LootStore("creature_loot_template");
            WorldServiceLocator._WS_Loot.LootTemplates_Disenchant = new WS_Loot.LootStore("disenchant_loot_template");
            WorldServiceLocator._WS_Loot.LootTemplates_Fishing = new WS_Loot.LootStore("fishing_loot_template");
            WorldServiceLocator._WS_Loot.LootTemplates_Gameobject = new WS_Loot.LootStore("gameobject_loot_template");
            WorldServiceLocator._WS_Loot.LootTemplates_Item = new WS_Loot.LootStore("item_loot_template");
            WorldServiceLocator._WS_Loot.LootTemplates_Pickpocketing = new WS_Loot.LootStore("pickpocketing_loot_template");
            WorldServiceLocator._WS_Loot.LootTemplates_QuestMail = new WS_Loot.LootStore("quest_mail_loot_template");
            WorldServiceLocator._WS_Loot.LootTemplates_Reference = new WS_Loot.LootStore("reference_loot_template");
            WorldServiceLocator._WS_Loot.LootTemplates_Skinning = new WS_Loot.LootStore("skinning_loot_template");
        }

        public void LoadWeather()
        {
            try
            {
                DataTable weatherQuery = new DataTable();
                WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT * FROM game_weather;", ref weatherQuery);
                IEnumerator enumerator = default;
                try
                {
                    enumerator = weatherQuery.Rows.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        DataRow row = (DataRow)enumerator.Current;
                        int zone = row.As<int>("zone");
                        if (!WorldServiceLocator._WS_Weather.WeatherZones.ContainsKey(zone))
                        {
                            WS_Weather.WeatherZone zoneChanges = new WS_Weather.WeatherZone(zone);
                            zoneChanges.Seasons[0] = new WS_Weather.WeatherSeasonChances(row.As<int>("spring_rain_chance"), row.As<int>("spring_snow_chance"), row.As<int>("spring_storm_chance"));
                            zoneChanges.Seasons[1] = new WS_Weather.WeatherSeasonChances(row.As<int>("summer_rain_chance"), row.As<int>("summer_snow_chance"), row.As<int>("summer_storm_chance"));
                            zoneChanges.Seasons[2] = new WS_Weather.WeatherSeasonChances(row.As<int>("fall_rain_chance"), row.As<int>("fall_snow_chance"), row.As<int>("fall_storm_chance"));
                            zoneChanges.Seasons[3] = new WS_Weather.WeatherSeasonChances(row.As<int>("winter_rain_chance"), row.As<int>("winter_snow_chance"), row.As<int>("winter_storm_chance"));
                            WorldServiceLocator._WS_Weather.WeatherZones.Add(zone, zoneChanges);
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
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Database: {0} Weather zones initialized.", weatherQuery.Rows.Count);
            }
            catch (DirectoryNotFoundException ex)
            {
                ProjectData.SetProjectError(ex);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Database : TransportQuery missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
                ProjectData.ClearProjectError();
            }
        }
    }
}
