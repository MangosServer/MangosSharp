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
using Mangos.Common.Enums.Spell;
using Mangos.Common.Legacy;
using Mangos.DataStores;
using Mangos.World.Loots;
using Mangos.World.Maps;
using Mangos.World.Spells;
using Mangos.World.Weather;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace Mangos.World.DataStores;

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
                var num = tmpDBC.Rows - 1;
                for (var i = 0; i <= num; i++)
                {
                    var radiusID = tmpDBC.ReadInt(i, 0);
                    var radiusValue = tmpDBC.ReadFloat(i, 1);
                    WorldServiceLocator._WS_Spells.SpellRadius[radiusID] = radiusValue;
                }
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SpellRadius initialized.", tmpDBC.Rows - 1);
            }
            catch (DirectoryNotFoundException ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DBC File: SpellRadius is Missing.", ex);
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
                var num = tmpDBC.Rows - 1;
                for (var i = 0; i <= num; i++)
                {
                    var spellCastID = tmpDBC.ReadInt(i, 0);
                    var spellCastTimeS = tmpDBC.ReadInt(i, 1);
                    WorldServiceLocator._WS_Spells.SpellCastTime[spellCastID] = spellCastTimeS;
                }
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SpellCastTimes initialized.", tmpDBC.Rows - 1);
            }
            catch (DirectoryNotFoundException ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DBC File: SpellCastTimes is Missing.", ex);
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
                var num = tmpDBC.Rows - 1;
                for (var i = 0; i <= num; i++)
                {
                    var spellRangeIndex = tmpDBC.ReadInt(i, 0);
                    var spellRangeMax = tmpDBC.ReadFloat(i, 2);
                    WorldServiceLocator._WS_Spells.SpellRange[spellRangeIndex] = spellRangeMax;
                }
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SpellRanges initialized.", tmpDBC.Rows - 1);
            }
            catch (DirectoryNotFoundException ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DBC File: SpellRanges is Missing.", ex);
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
                var num = tmpDBC.Rows - 1;
                for (var i = 0; i <= num; i++)
                {
                    var id = tmpDBC.ReadInt(i, 0);
                    var flags1 = tmpDBC.ReadInt(i, 11);
                    var creatureType = tmpDBC.ReadInt(i, 12);
                    var attackSpeed = tmpDBC.ReadInt(i, 13);
                    WorldServiceLocator._WS_DBCDatabase.SpellShapeShiftForm.Add(new WS_DBCDatabase.TSpellShapeshiftForm(id, flags1, creatureType, attackSpeed));
                }
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SpellShapeshiftForms initialized.", tmpDBC.Rows - 1);
            }
            catch (DirectoryNotFoundException ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DBC File: SpellShapeShiftForms is Missing.", ex);
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
                var num = tmpDBC.Rows - 1;
                for (var i = 0; i <= num; i++)
                {
                    var spellFocusIndex = tmpDBC.ReadInt(i, 0);
                    var spellFocusObjectName = tmpDBC.ReadString(i, 1);
                    WorldServiceLocator._WS_Spells.SpellFocusObject[spellFocusIndex] = spellFocusObjectName;
                }
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SpellFocusObjects initialized.", tmpDBC.Rows - 1);
            }
            catch (DirectoryNotFoundException ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DBC File: SpellFocusObjects is Missing.", ex);
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
                var num = tmpDBC.Rows - 1;
                for (var i = 0; i <= num; i++)
                {
                    var spellDurationIndex = tmpDBC.ReadInt(i, 0);
                    var spellDurationValue = tmpDBC.ReadInt(i, 1);
                    WorldServiceLocator._WS_Spells.SpellDuration[spellDurationIndex] = spellDurationValue;
                }
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SpellDurations initialized.", tmpDBC.Rows - 1);
            }
            catch (DirectoryNotFoundException ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DBC File: SpellDurations is Missing.", ex);
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
                for (var i = 0; i <= spellDBC.Rows - 1; i++)
                {
                    try
                    {
                        var id = spellDBC.ReadInt(i, 0);
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

                        var k = 0;
                        do
                        {
                            if (spellDBC.ReadInt(i, 61 + k) != 0)
                            {
                                var spellEffects = WorldServiceLocator._WS_Spells.SPELLs[id].SpellEffects;
                                int key;
                                Dictionary<int, WS_Spells.SpellInfo> Spells;
                                var Spell = (Spells = WorldServiceLocator._WS_Spells.SPELLs)[key = id];
                                Spells[key] = Spell;
                                WS_Spells.SpellEffect spellEffect2 = new(ref Spell)
                                {
                                    ID = (SpellEffects_Names)spellDBC.ReadInt(i, 61 + k),
                                    valueDie = spellDBC.ReadInt(i, 64 + k),
                                    diceBase = spellDBC.ReadInt(i, 67 + k),
                                    dicePerLevel = spellDBC.ReadFloat(i, 70 + k),
                                    valuePerLevel = (int)spellDBC.ReadFloat(i, 73 + k),
                                    valueBase = spellDBC.ReadInt(i, 76 + k),
                                    Mechanic = spellDBC.ReadInt(i, 79 + k),
                                    implicitTargetA = spellDBC.ReadInt(i, 82 + k),
                                    implicitTargetB = spellDBC.ReadInt(i, 85 + k),
                                    RadiusIndex = spellDBC.ReadInt(i, 88 + k),
                                    ApplyAuraIndex = spellDBC.ReadInt(i, 91 + k),
                                    Amplitude = spellDBC.ReadInt(i, 94 + k),
                                    MultipleValue = spellDBC.ReadInt(i, 97 + k),
                                    ChainTarget = spellDBC.ReadInt(i, 100 + k),
                                    ItemType = spellDBC.ReadInt(i, 103 + k),
                                    MiscValue = spellDBC.ReadInt(i, 106 + k),
                                    TriggerSpell = spellDBC.ReadInt(i, 109 + k),
                                    valuePerComboPoint = spellDBC.ReadInt(i, 112 + k)
                                };
                                spellEffects[k] = spellEffect2;
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
                        var j = 0;
                        do
                        {
                            if (WorldServiceLocator._WS_Spells.SPELLs[id].SpellEffects[j] != null)
                            {
                                WorldServiceLocator._WS_Spells.SPELLs[id].SpellEffects[j].DamageMultiplier = spellDBC.ReadFloat(i, 167 + j);
                            }

                            j++;
                        }
                        while (j <= 2);
                        WorldServiceLocator._WS_Spells.SPELLs[id].InitCustomAttributes();
                    }
                    catch (Exception ex)
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Line {0} caused error: {1}", i, ex.ToString());
                    }
                }
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} Spells initialized.", spellDBC.Rows - 1);
            }
            catch (DirectoryNotFoundException ex2)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DBC File: Spell is Missing.", ex2);
            }
        }
    }

    public void InitializeSpellChains()
    {
        try
        {
            DataTable spellChainQuery = new();
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
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Database: SpellChains is Missing.", ex);
        }
    }

    public async Task InitializeTaxiNodesAsync()
    {
        checked
        {
            try
            {
                var tmpDBC = await dataStoreProvider.GetDataStoreAsync("TaxiNodes.dbc");
                var num = tmpDBC.Rows - 1;
                for (var i = 0; i <= num; i++)
                {
                    var taxiNode = tmpDBC.ReadInt(i, 0);
                    var taxiMapID = tmpDBC.ReadInt(i, 1);
                    var taxiPosX = tmpDBC.ReadFloat(i, 2);
                    var taxiPosY = tmpDBC.ReadFloat(i, 3);
                    var taxiPosZ = tmpDBC.ReadFloat(i, 4);
                    var taxiMountTypeHorde = tmpDBC.ReadInt(i, 14);
                    var taxiMountTypeAlliance = tmpDBC.ReadInt(i, 15);

                    if (WorldServiceLocator._ConfigurationProvider.GetConfiguration().Maps.Contains(taxiMapID.ToString()))
                    {
                        WorldServiceLocator._WS_DBCDatabase.TaxiNodes.Add(taxiNode, new WS_DBCDatabase.TTaxiNode(taxiPosX, taxiPosY, taxiPosZ, taxiMapID, taxiMountTypeHorde, taxiMountTypeAlliance));
                    }
                }
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} TaxiNodes initialized.", tmpDBC.Rows - 1);
            }
            catch (DirectoryNotFoundException ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DBC File: TaxiNodes is Missing.", ex);
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
                var num = tmpDBC.Rows - 1;
                for (var i = 0; i <= num; i++)
                {
                    var taxiNode = tmpDBC.ReadInt(i, 0);
                    var taxiFrom = tmpDBC.ReadInt(i, 1);
                    var taxiTo = tmpDBC.ReadInt(i, 2);
                    var taxiPrice = tmpDBC.ReadInt(i, 3);
                    WorldServiceLocator._WS_DBCDatabase.TaxiPaths.Add(taxiNode, new WS_DBCDatabase.TTaxiPath(taxiFrom, taxiTo, taxiPrice));
                }
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} TaxiPaths initialized.", tmpDBC.Rows - 1);
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine("DBC File : TaxiPath missing.");
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DBC File: TaxiPath is Missing.", ex);
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
                var num = tmpDBC.Rows - 1;
                for (var i = 0; i <= num; i++)
                {
                    var taxiPath = tmpDBC.ReadInt(i, 1);
                    var taxiSeq = tmpDBC.ReadInt(i, 2);
                    var taxiMapID = tmpDBC.ReadInt(i, 3);
                    var taxiPosX = tmpDBC.ReadFloat(i, 4);
                    var taxiPosY = tmpDBC.ReadFloat(i, 5);
                    var taxiPosZ = tmpDBC.ReadFloat(i, 6);
                    var taxiAction = tmpDBC.ReadInt(i, 7);
                    var taxiWait = tmpDBC.ReadInt(i, 8);
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
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DBC File: TaxiPathNode is Missing.", ex);
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
                var num = tmpDBC.Rows - 1;
                for (var i = 0; i <= num; i++)
                {
                    var skillID = tmpDBC.ReadInt(i, 0);
                    var skillLine = tmpDBC.ReadInt(i, 1);
                    WorldServiceLocator._WS_DBCDatabase.SkillLines[skillID] = skillLine;
                }
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SkillLines initialized.", tmpDBC.Rows - 1);
            }
            catch (DirectoryNotFoundException ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DBC File: SkillLine is Missing.", ex);
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
                var num = tmpDBC.Rows - 1;
                for (var i = 0; i <= num; i++)
                {
                    WS_DBCDatabase.TSkillLineAbility tmpSkillLineAbility = new()
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
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DBC File: SkillLineAbility is Missing.", ex);
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
                var keyType = new byte[5];
                var key = new int[5];
                var num = tmpDBC.Rows - 1;
                for (var i = 0; i <= num; i++)
                {
                    var lockID = tmpDBC.ReadInt(i, 0);
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
                    var reqMining = tmpDBC.ReadInt(i, 17);
                    var reqLockSkill = tmpDBC.ReadInt(i, 17);
                    WorldServiceLocator._WS_Loot.Locks[lockID] = new WS_Loot.TLock(keyType, key, (short)reqMining, (short)reqLockSkill);
                }
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} Locks initialized.", tmpDBC.Rows - 1);
            }
            catch (DirectoryNotFoundException ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DBC File: Lock is Missing.", ex);
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
                var num = tmpDBC.Rows - 1;
                for (var i = 0; i <= num; i++)
                {
                    var areaID = tmpDBC.ReadInt(i, 0);
                    var areaMapID = tmpDBC.ReadInt(i, 1);
                    var areaZone = tmpDBC.ReadInt(i, 2);
                    var areaExploreFlag = tmpDBC.ReadInt(i, 3);
                    var areaZoneType = tmpDBC.ReadInt(i, 4);
                    var areaLevel = tmpDBC.ReadInt(i, 10);

                    if (areaLevel > 255)
                    {
                        areaLevel = 255;
                    }

                    if (areaLevel < 0)
                    {
                        areaLevel = 0;
                    }

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
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DBC File: AreaTable is Missing.", ex);
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
                var num = tmpDBC.Rows - 1;
                for (var i = 0; i <= num; i++)
                {
                    var emoteID = tmpDBC.ReadInt(i, 0);
                    var emoteState = tmpDBC.ReadInt(i, 4);

                    if (emoteID != 0)
                    {
                        WorldServiceLocator._WS_DBCDatabase.EmotesState[emoteID] = emoteState;
                    }
                }
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} Emotes initialized.", tmpDBC.Rows - 1);
            }
            catch (DirectoryNotFoundException ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DBC File: Emotes is Missing.", ex);
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
                var num = tmpDBC.Rows - 1;
                for (var i = 0; i <= num; i++)
                {
                    var textEmoteID = tmpDBC.ReadInt(i, 0);
                    var emoteID = tmpDBC.ReadInt(i, 2);

                    if (emoteID != 0)
                    {
                        WorldServiceLocator._WS_DBCDatabase.EmotesText[textEmoteID] = emoteID;
                    }
                }
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} EmotesText initialized.", tmpDBC.Rows - 1);
            }
            catch (DirectoryNotFoundException ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DBC File: EmotesText is Missing.", ex);
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
                var flags = new int[4];
                var reputationStats = new int[4];
                var reputationFlags = new int[4];
                var num = tmpDBC.Rows - 1;
                for (var i = 0; i <= num; i++)
                {
                    var factionID = tmpDBC.ReadInt(i, 0);
                    var factionFlag = tmpDBC.ReadInt(i, 1);
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
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DBC File: Faction is Missing.", ex);
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
                var num = tmpDBC.Rows - 1;
                for (var i = 0; i <= num; i++)
                {
                    var templateID = tmpDBC.ReadInt(i, 0);
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
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DBC File: FactionTemplate is Missing.", ex);
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
                var num = tmpDBC.Rows - 1;
                for (var i = 0; i <= num; i++)
                {
                    var raceID = tmpDBC.ReadInt(i, 0);
                    var factionID = tmpDBC.ReadInt(i, 2);
                    var modelM = tmpDBC.ReadInt(i, 4);
                    var modelF = tmpDBC.ReadInt(i, 5);
                    var teamID = tmpDBC.ReadInt(i, 8);
                    var taxiMask = (uint)tmpDBC.ReadInt(i, 14);
                    var cinematicID = tmpDBC.ReadInt(i, 16);
                    var name = tmpDBC.ReadString(i, 17);
                    WorldServiceLocator._WS_DBCDatabase.CharRaces[(byte)raceID] = new WS_DBCDatabase.TCharRace((short)factionID, modelM, modelF, (byte)teamID, taxiMask, cinematicID, name);
                }
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} CharRaces initialized.", tmpDBC.Rows - 1);
            }
            catch (DirectoryNotFoundException ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DBC File: ChrRaces is Missing.", ex);
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
                var num = tmpDBC.Rows - 1;
                for (var i = 0; i <= num; i++)
                {
                    var classID = tmpDBC.ReadInt(i, 0);
                    var cinematicID = tmpDBC.ReadInt(i, 5);
                    WorldServiceLocator._WS_DBCDatabase.CharClasses[(byte)classID] = new WS_DBCDatabase.TCharClass(cinematicID);
                }
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} CharClasses initialized.", tmpDBC.Rows - 1);
            }
            catch (DirectoryNotFoundException ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DBC File: ChrClasses is Missing.", ex);
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
                var num = tmpDBC.Rows - 1;
                for (var i = 0; i <= num; i++)
                {
                    var itemBroken = tmpDBC.ReadInt(i, 0);
                    var num2 = tmpDBC.Columns - 1;
                    for (var itemType = 1; itemType <= num2; itemType++)
                    {
                        var itemPrice = tmpDBC.ReadInt(i, itemType);
                        WorldServiceLocator._WS_DBCDatabase.DurabilityCosts[itemBroken, itemType - 1] = (short)itemPrice;
                    }
                }
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} DurabilityCosts initialized.", tmpDBC.Rows - 1);
            }
            catch (DirectoryNotFoundException ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DBC File: DurabilityCosts is Missing.", ex);
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
                var num = dbc.Rows - 1;
                for (var i = 0; i <= num; i++)
                {
                    WS_DBCDatabase.TalentInfo tmpInfo = new()
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
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DBC File: Talent is Missing.", ex);
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
                var num = dbc.Rows - 1;
                for (var i = 0; i <= num; i++)
                {
                    var talentTab = dbc.ReadInt(i, 0);
                    var talentMask = dbc.ReadInt(i, 12);
                    WorldServiceLocator._WS_DBCDatabase.TalentsTab.Add(talentTab, talentMask);
                }
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} Talent tabs initialized.", dbc.Rows - 1);
            }
            catch (DirectoryNotFoundException ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DBC File: TalentTab is Missing.", ex);
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
                var num = dbc.Rows - 1;
                for (var i = 0; i <= num; i++)
                {
                    var ahId = dbc.ReadInt(i, 0);
                    var fee = dbc.ReadInt(i, 2);
                    var tax = dbc.ReadInt(i, 3);
                    WorldServiceLocator._WS_Auction.AuctionID = ahId;
                    WorldServiceLocator._WS_Auction.AuctionFee = fee;
                    WorldServiceLocator._WS_Auction.AuctionTax = tax;
                }
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} AuctionHouses initialized.", dbc.Rows - 1);
            }
            catch (DirectoryNotFoundException ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DBC File: AuctionHouse is Missing.", ex);
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
                var type = new int[3];
                var amount = new int[3];
                var spellID = new int[3];
                var num = dbc.Rows - 1;
                for (var i = 0; i <= num; i++)
                {
                    var id = dbc.ReadInt(i, 0);
                    type[0] = dbc.ReadInt(i, 1);
                    type[1] = dbc.ReadInt(i, 2);
                    amount[0] = dbc.ReadInt(i, 4);
                    amount[1] = dbc.ReadInt(i, 7);
                    spellID[0] = dbc.ReadInt(i, 10);
                    spellID[1] = dbc.ReadInt(i, 11);
                    var auraID = dbc.ReadInt(i, 22);
                    var slot = dbc.ReadInt(i, 23);
                    WorldServiceLocator._WS_DBCDatabase.SpellItemEnchantments.Add(id, new WS_DBCDatabase.TSpellItemEnchantment(type, amount, spellID, auraID, slot));
                }
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SpellItemEnchantments initialized.", dbc.Rows - 1);
            }
            catch (DirectoryNotFoundException ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DBC File: SpellItemEnchantment is Missing.", ex);
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
                var itemID = new int[8];
                var spellID = new int[8];
                var itemCount = new int[8];
                var num = dbc.Rows - 1;
                int requiredSkillID = default;
                int requiredSkillValue = default;
                for (var i = 0; i <= num; i++)
                {
                    var id = dbc.ReadInt(i, 0);
                    var name = dbc.ReadString(i, 1);
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
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DBC File: ItemSet is Missing.", ex);
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
                var num = dbc.Rows - 1;
                for (var i = 0; i <= num; i++)
                {
                    WS_DBCDatabase.TItemDisplayInfo tmpItemDisplayInfo = new()
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
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DBC File: ItemDisplayInfo is Missing.", ex);
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
                var num = dbc.Rows - 1;
                for (var i = 0; i <= num; i++)
                {
                    WS_DBCDatabase.TItemRandomPropertiesInfo tmpInfo = new()
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
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DBC File: ItemRandomProperties is Missing.", ex);
            }
        }
    }

    public void LoadCreatureGossip()
    {
        try
        {
            DataTable gossipQuery = new();
            WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT * FROM npc_gossip;", ref gossipQuery);
            IEnumerator enumerator = default;
            try
            {
                enumerator = gossipQuery.Rows.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    DataRow row = (DataRow)enumerator.Current;
                    var guid = row.As<ulong>("npc_guid");
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
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Datanase: npc_gossip is Missing.", ex);
        }
    }

    public async Task LoadCreatureFamilyDbcAsync()
    {
        checked
        {
            try
            {
                var dbc = await dataStoreProvider.GetDataStoreAsync("CreatureFamily.dbc");
                var num = dbc.Rows - 1;
                for (var i = 0; i <= num; i++)
                {
                    WS_DBCDatabase.CreatureFamilyInfo tmpInfo = new()
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
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "DBC File: CreatureFamily is Missing.", ex);
            }
        }
    }

    public void LoadCreatureMovements()
    {
        try
        {
            DataTable movementsQuery = new();
            WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT * FROM waypoint_data ORDER BY id, point;", ref movementsQuery);
            IEnumerator enumerator = default;
            try
            {
                enumerator = movementsQuery.Rows.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    DataRow row = (DataRow)enumerator.Current;
                    var id = row.As<int>("id");
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
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Database: Waypoint_Data is Missing.", ex);
        }
    }

    public void LoadCreatureEquipTable()
    {
        try
        {
            DataTable equipQuery = new();
            WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT * FROM creature_equip_template_raw;", ref equipQuery);
            IEnumerator enumerator = default;
            try
            {
                enumerator = equipQuery.Rows.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    DataRow row = (DataRow)enumerator.Current;
                    var entry = row.As<int>("entry");
                    if (!WorldServiceLocator._WS_DBCDatabase.CreatureEquip.ContainsKey(entry))
                    {
                        try
                        {
                            WorldServiceLocator._WS_DBCDatabase.CreatureEquip.Add(entry, new WS_DBCDatabase.CreatureEquipInfo(row.As<int>("equipmodel1"), row.As<int>("equipmodel2"), row.As<int>("equipmodel3"), row.As<uint>("equipinfo1"), row.As<uint>("equipinfo2"), row.As<uint>("equipinfo3"), row.As<int>("equipslot1"), row.As<int>("equipslot2"), row.As<int>("equipslot3")));
                        }
                        catch (DataException ex)
                        {
                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, $"Creature_Equip_Template_raw: Unable to equip items {entry} for Creature ", entry, ex);
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
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Database: Creature_Equip_Template_raw is Missing.", ex2);
        }
    }

    public void LoadCreatureModelInfo()
    {
        try
        {
            DataTable modelQuery = new();
            WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT * FROM creature_model_info;", ref modelQuery);
            IEnumerator enumerator = default;
            try
            {
                enumerator = modelQuery.Rows.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    DataRow row = (DataRow)enumerator.Current;
                    var entry = row.As<int>("modelid");
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
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Database: Creature_Model_Info is Missing.", ex);
        }
    }

    public void LoadQuestStartersAndFinishers()
    {
        DataTable questStarters = new();
        WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT * FROM quest_relations where actor=0 and role =0;", ref questStarters);
        IEnumerator enumerator = default;
        try
        {
            enumerator = questStarters.Rows.GetEnumerator();
            while (enumerator.MoveNext())
            {
                DataRow row = (DataRow)enumerator.Current;
                var entry4 = row.As<int>("entry");
                var quest4 = row.As<int>("quest");
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
        var questStartersAmount = questStarters.Rows.Count;
        questStarters.Clear();
        WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT * FROM quest_relations where actor=1 and role=0;", ref questStarters);
        IEnumerator enumerator2 = default;
        try
        {
            enumerator2 = questStarters.Rows.GetEnumerator();
            while (enumerator2.MoveNext())
            {
                DataRow row = (DataRow)enumerator2.Current;
                var entry3 = row.As<int>("entry");
                var quest3 = row.As<int>("quest");
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
            DataTable questFinishers = new();
            WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT * FROM quest_relations where actor=0 and role=1;", ref questFinishers);
            IEnumerator enumerator3 = default;
            try
            {
                enumerator3 = questFinishers.Rows.GetEnumerator();
                while (enumerator3.MoveNext())
                {
                    DataRow row = (DataRow)enumerator3.Current;
                    var entry2 = row.As<int>("entry");
                    var quest2 = row.As<int>("quest");
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
            var questFinishersAmount = questFinishers.Rows.Count;
            questFinishers.Clear();
            WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT * FROM quest_relations where actor=1 and role=1;", ref questFinishers);
            IEnumerator enumerator4 = default;
            try
            {
                enumerator4 = questFinishers.Rows.GetEnumerator();
                while (enumerator4.MoveNext())
                {
                    DataRow row = (DataRow)enumerator4.Current;
                    var entry = row.As<int>("entry");
                    var quest = row.As<int>("quest");
                    if (!WorldServiceLocator._WorldServer.GameobjectQuestFinishers.ContainsKey(entry))
                    {
                        WorldServiceLocator._WorldServer.GameobjectQuestFinishers.Add(entry, new List<int>());
                    }
                    WorldServiceLocator._WorldServer.GameobjectQuestFinishers[entry].Add(quest);
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Database: Quest_Relations is Missing.", ex);
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
            DataTable weatherQuery = new();
            WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT * FROM game_weather;", ref weatherQuery);
            IEnumerator enumerator = default;
            try
            {
                enumerator = weatherQuery.Rows.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    DataRow row = (DataRow)enumerator.Current;
                    var zone = row.As<int>("zone");
                    if (!WorldServiceLocator._WS_Weather.WeatherZones.ContainsKey(zone))
                    {
                        WS_Weather.WeatherZone zoneChanges = new(zone);
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
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Database: Game_Weather is Missing.", ex);
        }
    }
}
