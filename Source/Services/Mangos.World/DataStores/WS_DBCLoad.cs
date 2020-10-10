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
using System.IO;
using Mangos.Common.DataStores;
using Mangos.Common.Enums.Global;
using Mangos.World.Loots;
using Mangos.World.Maps;
using Mangos.World.Spells;
using Mangos.World.Weather;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.DataStores
{
    public class WS_DBCLoad
    {

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public void InitializeSpellRadius()
        {
            try
            {
                var tmpDBC = new BufferedDbc("dbc" + Path.DirectorySeparatorChar + "SpellRadius.dbc");
                int radiusID;
                float radiusValue;
                // Dim radiusValue2 As Single

                for (int i = 0, loopTo = tmpDBC.Rows - 1; i <= loopTo; i++)
                {
                    radiusID = (int)tmpDBC.Item(i, 0);
                    radiusValue = (float)tmpDBC.Item(i, 1, DBCValueType.DBC_FLOAT);
                    // radiusValue2 = tmpDBC.Item(i, 3, DBCValueType.DBC_FLOAT) ' May be needed in the future

                    WorldServiceLocator._WS_Spells.SpellRadius[radiusID] = radiusValue;
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SpellRadius initialized.", tmpDBC.Rows - 1);
                tmpDBC.Dispose();
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DBC File : SpellRadius missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public void InitializeSpellCastTime()
        {
            try
            {
                var tmpDBC = new BufferedDbc("dbc" + Path.DirectorySeparatorChar + "SpellCastTimes.dbc");
                int spellCastID;
                int spellCastTimeS;
                for (int i = 0, loopTo = tmpDBC.Rows - 1; i <= loopTo; i++)
                {
                    spellCastID = (int)tmpDBC.Item(i, 0);
                    spellCastTimeS = (int)tmpDBC.Item(i, 1);
                    WorldServiceLocator._WS_Spells.SpellCastTime[spellCastID] = spellCastTimeS;
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SpellCastTimes initialized.", tmpDBC.Rows - 1);
                tmpDBC.Dispose();
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DBC File : SpellCastTimes missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public void InitializeSpellRange()
        {
            try
            {
                var tmpDBC = new BufferedDbc("dbc" + Path.DirectorySeparatorChar + "SpellRange.dbc");
                int spellRangeIndex;
                // Dim spellRangeMin As Single
                float spellRangeMax;
                for (int i = 0, loopTo = tmpDBC.Rows - 1; i <= loopTo; i++)
                {
                    spellRangeIndex = (int)tmpDBC.Item(i, 0);
                    // spellRangeMin = tmpDBC.Item(i, 1, DBCValueType.DBC_FLOAT) ' Added back may be needed in the future
                    spellRangeMax = (float)tmpDBC.Item(i, 2, DBCValueType.DBC_FLOAT);
                    WorldServiceLocator._WS_Spells.SpellRange[spellRangeIndex] = spellRangeMax;
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SpellRanges initialized.", tmpDBC.Rows - 1);
                tmpDBC.Dispose();
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DBC File : SpellRanges missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public void InitializeSpellShapeShift()
        {
            try
            {
                var tmpDBC = new BufferedDbc("dbc" + Path.DirectorySeparatorChar + "SpellShapeshiftForm.dbc");
                int id;
                int flags1;
                int creatureType;
                int attackSpeed;
                for (int i = 0, loopTo = tmpDBC.Rows - 1; i <= loopTo; i++)
                {
                    id = (int)tmpDBC.Item(i, 0);
                    flags1 = (int)tmpDBC.Item(i, 11);
                    creatureType = (int)tmpDBC.Item(i, 12);
                    attackSpeed = (int)tmpDBC.Item(i, 13);
                    WorldServiceLocator._WS_DBCDatabase.SpellShapeShiftForm.Add(new WS_DBCDatabase.TSpellShapeshiftForm(id, flags1, creatureType, attackSpeed));
                }

                tmpDBC.Dispose();
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SpellShapeshiftForms initialized.", tmpDBC.Rows - 1);
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DBC File : SpellShapeshiftForms missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public void InitializeSpellFocusObject()
        {
            try
            {
                var tmpDBC = new BufferedDbc("dbc" + Path.DirectorySeparatorChar + "SpellFocusObject.dbc");
                int spellFocusIndex;
                string spellFocusObjectName;
                for (int i = 0, loopTo = tmpDBC.Rows - 1; i <= loopTo; i++)
                {
                    spellFocusIndex = (int)tmpDBC.Item(i, 0);
                    spellFocusObjectName = (string)tmpDBC.Item(i, 1, DBCValueType.DBC_STRING);
                    WorldServiceLocator._WS_Spells.SpellFocusObject[spellFocusIndex] = spellFocusObjectName;
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SpellFocusObjects initialized.", tmpDBC.Rows - 1);
                tmpDBC.Dispose();
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DBC File : SpellFocusObjects missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public void InitializeSpellDuration()
        {
            try
            {
                var tmpDBC = new BufferedDbc("dbc" + Path.DirectorySeparatorChar + "SpellDuration.dbc");
                int spellDurationIndex;
                int spellDurationValue;
                // Dim SpellDurationValue2 As Integer
                // Dim SpellDurationValue3 As Integer

                for (int i = 0, loopTo = tmpDBC.Rows - 1; i <= loopTo; i++)
                {
                    spellDurationIndex = (int)tmpDBC.Item(i, 0);
                    spellDurationValue = (int)tmpDBC.Item(i, 1);
                    // SpellDurationValue2 = tmpDBC.Item(i, 2) ' May be needed in the future
                    // SpellDurationValue3 = tmpDBC.Item(i, 3) ' May be needed in the future

                    WorldServiceLocator._WS_Spells.SpellDuration[spellDurationIndex] = spellDurationValue;
                }

                tmpDBC.Dispose();
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SpellDurations initialized.", tmpDBC.Rows - 1);
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DBC File : SpellDurations missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public void InitializeSpells()
        {
            try
            {
                var spellDBC = new BufferedDbc("dbc" + Path.DirectorySeparatorChar + "Spell.dbc");
                // Console.WriteLine("[" & Format(TimeOfDay, "hh:mm:ss") & "] " & SpellDBC.GetFileInformation)
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: Initializing Spells - This may take a few moments....");
                int id;
                for (long i = 0L, loopTo = spellDBC.Rows - 1; i <= loopTo; i++)
                {
                    try
                    {
                        id = (int)spellDBC.Item((int)i, 0);
                        // 3 = Not Used
                        // AttributesEx3 = SpellDBC.Item(i, 9)
                        // AttributesEx4 = SpellDBC.Item(i, 10)
                        WorldServiceLocator._WS_Spells.SPELLs[id] = new WS_Spells.SpellInfo()
                        {
                            ID = id,
                            School = (int)spellDBC.Item((int)i, 1),
                            Category = (int)spellDBC.Item((int)i, 2),
                            DispellType = (int)spellDBC.Item((int)i, 4),
                            Mechanic = (int)spellDBC.Item((int)i, 5),
                            Attributes = (int)spellDBC.Item((int)i, 6),
                            AttributesEx = (int)spellDBC.Item((int)i, 7),
                            AttributesEx2 = (int)spellDBC.Item((int)i, 8),
                            RequredCasterStance = (int)spellDBC.Item((int)i, 11), // RequiredShapeShift
                            ShapeshiftExclude = (int)spellDBC.Item((int)i, 12),
                            Target = (int)spellDBC.Item((int)i, 13),
                            TargetCreatureType = (int)spellDBC.Item((int)i, 14),
                            FocusObjectIndex = (int)spellDBC.Item((int)i, 15),
                            CasterAuraState = (int)spellDBC.Item((int)i, 16),
                            TargetAuraState = (int)spellDBC.Item((int)i, 17),
                            SpellCastTimeIndex = (int)spellDBC.Item((int)i, 18),
                            SpellCooldown = (int)spellDBC.Item((int)i, 19),
                            CategoryCooldown = (int)spellDBC.Item((int)i, 20),
                            interruptFlags = (int)spellDBC.Item((int)i, 21),
                            auraInterruptFlags = (int)spellDBC.Item((int)i, 22),
                            channelInterruptFlags = (int)spellDBC.Item((int)i, 23),
                            procFlags = (int)spellDBC.Item((int)i, 24),
                            procChance = (int)spellDBC.Item((int)i, 25),
                            procCharges = (int)spellDBC.Item((int)i, 26),
                            maxLevel = (int)spellDBC.Item((int)i, 27),
                            baseLevel = (int)spellDBC.Item((int)i, 28),
                            spellLevel = (int)spellDBC.Item((int)i, 29),
                            DurationIndex = (int)spellDBC.Item((int)i, 30),
                            powerType = (int)spellDBC.Item((int)i, 31),
                            manaCost = (int)spellDBC.Item((int)i, 32),
                            manaCostPerlevel = (int)spellDBC.Item((int)i, 33),
                            manaPerSecond = (int)spellDBC.Item((int)i, 34),
                            manaPerSecondPerLevel = (int)spellDBC.Item((int)i, 35),
                            rangeIndex = (int)spellDBC.Item((int)i, 36),
                            Speed = (float)spellDBC.Item((int)i, 37, DBCValueType.DBC_FLOAT),
                            modalNextSpell = (int)spellDBC.Item((int)i, 38), // Not Used
                            maxStack = (int)spellDBC.Item((int)i, 39)
                        };
                        WorldServiceLocator._WS_Spells.SPELLs[id].Totem[0] = (int)spellDBC.Item((int)i, 40);
                        WorldServiceLocator._WS_Spells.SPELLs[id].Totem[1] = (int)spellDBC.Item((int)i, 41);

                        // -CORRECT-
                        WorldServiceLocator._WS_Spells.SPELLs[id].Reagents[0] = (int)spellDBC.Item((int)i, 42);
                        WorldServiceLocator._WS_Spells.SPELLs[id].Reagents[1] = (int)spellDBC.Item((int)i, 43);
                        WorldServiceLocator._WS_Spells.SPELLs[id].Reagents[2] = (int)spellDBC.Item((int)i, 44);
                        WorldServiceLocator._WS_Spells.SPELLs[id].Reagents[3] = (int)spellDBC.Item((int)i, 45);
                        WorldServiceLocator._WS_Spells.SPELLs[id].Reagents[4] = (int)spellDBC.Item((int)i, 46);
                        WorldServiceLocator._WS_Spells.SPELLs[id].Reagents[5] = (int)spellDBC.Item((int)i, 47);
                        WorldServiceLocator._WS_Spells.SPELLs[id].Reagents[6] = (int)spellDBC.Item((int)i, 48);
                        WorldServiceLocator._WS_Spells.SPELLs[id].Reagents[7] = (int)spellDBC.Item((int)i, 49);
                        WorldServiceLocator._WS_Spells.SPELLs[id].ReagentsCount[0] = (int)spellDBC.Item((int)i, 50);
                        WorldServiceLocator._WS_Spells.SPELLs[id].ReagentsCount[1] = (int)spellDBC.Item((int)i, 51);
                        WorldServiceLocator._WS_Spells.SPELLs[id].ReagentsCount[2] = (int)spellDBC.Item((int)i, 52);
                        WorldServiceLocator._WS_Spells.SPELLs[id].ReagentsCount[3] = (int)spellDBC.Item((int)i, 53);
                        WorldServiceLocator._WS_Spells.SPELLs[id].ReagentsCount[4] = (int)spellDBC.Item((int)i, 54);
                        WorldServiceLocator._WS_Spells.SPELLs[id].ReagentsCount[5] = (int)spellDBC.Item((int)i, 55);
                        WorldServiceLocator._WS_Spells.SPELLs[id].ReagentsCount[6] = (int)spellDBC.Item((int)i, 56);
                        WorldServiceLocator._WS_Spells.SPELLs[id].ReagentsCount[7] = (int)spellDBC.Item((int)i, 57);
                        // -/CORRECT-

                        WorldServiceLocator._WS_Spells.SPELLs[id].EquippedItemClass = (int)spellDBC.Item((int)i, 58); // Value
                        WorldServiceLocator._WS_Spells.SPELLs[id].EquippedItemSubClass = (int)spellDBC.Item((int)i, 59); // Mask
                        WorldServiceLocator._WS_Spells.SPELLs[id].EquippedItemInventoryType = (int)spellDBC.Item((int)i, 60); // Mask
                        for (int j = 0; j <= 2; j++)
                        {
                            if (spellDBC.Item((int)i, 61 + j) != 0)
                            {
                                var tmp = WorldServiceLocator._WS_Spells.SPELLs;
                                var argSpell = tmp[id];
                                WorldServiceLocator._WS_Spells.SPELLs[id].SpellEffects[j] = new WS_Spells.SpellEffect(ref argSpell)
                                {
                                    ID = (Common.Enums.Spell.SpellEffects_Names)spellDBC.Item((int)i, 61 + j),
                                    valueDie = (int)spellDBC.Item((int)i, 64 + j),
                                    diceBase = (int)spellDBC.Item((int)i, 67 + j),
                                    dicePerLevel = (float)spellDBC.Item((int)i, 70 + j, DBCValueType.DBC_FLOAT),
                                    valuePerLevel = (int)spellDBC.Item((int)i, 73 + j, DBCValueType.DBC_FLOAT),
                                    valueBase = (int)spellDBC.Item((int)i, 76 + j),
                                    Mechanic = (int)spellDBC.Item((int)i, 79 + j),
                                    implicitTargetA = (int)spellDBC.Item((int)i, 82 + j),
                                    implicitTargetB = (int)spellDBC.Item((int)i, 85 + j),
                                    RadiusIndex = (int)spellDBC.Item((int)i, 88 + j), // spellradius.dbc
                                    ApplyAuraIndex = (int)spellDBC.Item((int)i, 91 + j),
                                    Amplitude = (int)spellDBC.Item((int)i, 94 + j),
                                    MultipleValue = (int)spellDBC.Item((int)i, 97 + j),
                                    ChainTarget = (int)spellDBC.Item((int)i, 100 + j),
                                    ItemType = (int)spellDBC.Item((int)i, 103 + j),
                                    MiscValue = (int)spellDBC.Item((int)i, 106 + j),
                                    TriggerSpell = (int)spellDBC.Item((int)i, 109 + j),
                                    valuePerComboPoint = (int)spellDBC.Item((int)i, 112 + j)
                                };
                                tmp[id] = argSpell;
                            }
                            else
                            {
                                WorldServiceLocator._WS_Spells.SPELLs[id].SpellEffects[j] = null;
                            }
                        }

                        WorldServiceLocator._WS_Spells.SPELLs[id].SpellVisual = (int)spellDBC.Item((int)i, 115);
                        // 116 = Always zero? - SpellVisual2 - Not Used
                        WorldServiceLocator._WS_Spells.SPELLs[id].SpellIconID = (int)spellDBC.Item((int)i, 117);
                        WorldServiceLocator._WS_Spells.SPELLs[id].ActiveIconID = (int)spellDBC.Item((int)i, 118);
                        // 119 = spellPriority
                        WorldServiceLocator._WS_Spells.SPELLs[id].Name = (string)spellDBC.Item((int)i, 120, DBCValueType.DBC_STRING);
                        // 121 = Always zero?
                        // 122 = Always zero?
                        // 123 = Always zero?
                        // 124 = Always zero?
                        // 125 = Always zero?
                        // 126 = Always zero?
                        // 127 = Always zero?
                        // 128 = Always zero?
                        WorldServiceLocator._WS_Spells.SPELLs[id].Rank = (string)spellDBC.Item((int)i, 129, DBCValueType.DBC_STRING);
                        // 130 = Always zero?
                        // 131 = Always zero?
                        // 132 = Always zero?
                        // 133 = Always zero?
                        // 134 = Always zero?
                        // 135 = Always zero?
                        // 136 = Always zero?
                        // 137 = RankFlags
                        // 138 = Description - Not Used
                        // 139 = Always zero?
                        // 140 = Always zero?
                        // 141 = Always zero?
                        // 142 = Always zero?
                        // 143 = Always zero?
                        // 144 = Always zero?
                        // 145 = Always zero?
                        // 146 = DescriptionFlags - Not Used
                        // 147 = ToolTip - Not USed
                        // 148 = Always zero?
                        // 149 = Always zero?
                        // 150 = Always zero?
                        // 151 = Always zero?
                        // 152 = Always zero?
                        // 153 = Always zero?
                        // 154 = Always zero?
                        // 155 = ToolTipFlags - Not Used
                        WorldServiceLocator._WS_Spells.SPELLs[id].manaCostPercent = (int)spellDBC.Item((int)i, 156);
                        WorldServiceLocator._WS_Spells.SPELLs[id].StartRecoveryCategory = (int)spellDBC.Item((int)i, 157);
                        WorldServiceLocator._WS_Spells.SPELLs[id].StartRecoveryTime = (int)spellDBC.Item((int)i, 158);
                        WorldServiceLocator._WS_Spells.SPELLs[id].AffectedTargetLevel = (int)spellDBC.Item((int)i, 159);
                        WorldServiceLocator._WS_Spells.SPELLs[id].SpellFamilyName = (int)spellDBC.Item((int)i, 160);
                        WorldServiceLocator._WS_Spells.SPELLs[id].SpellFamilyFlags = (int)spellDBC.Item((int)i, 161); // ClassFamilyMask SpellFamilyFlags;                   // 161+162
                        WorldServiceLocator._WS_Spells.SPELLs[id].MaxTargets = (int)spellDBC.Item((int)i, 163);
                        WorldServiceLocator._WS_Spells.SPELLs[id].DamageType = (int)spellDBC.Item((int)i, 164); // defenseType
                        // SPELLs(ID).PreventionType = SpellDBC.Item(i, 165)
                        // 166 = StanceBarOrder - Not Used

                        for (int j = 0; j <= 2; j++)
                        {
                            if (WorldServiceLocator._WS_Spells.SPELLs[id].SpellEffects[j] is object)
                            {
                                WorldServiceLocator._WS_Spells.SPELLs[id].SpellEffects[j].DamageMultiplier = (float)spellDBC.Item((int)i, 167 + j, DBCValueType.DBC_FLOAT);
                            }
                        }

                        // 170 = MinFactionId - Not Used
                        // 171 = MinReputation - Not Used
                        // 172 = RequiredAuraVision - Not Used

                        WorldServiceLocator._WS_Spells.SPELLs[id].InitCustomAttributes();
                    }
                    catch (Exception e)
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Line {0} caused error: {1}", i, e.ToString());
                    }
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} Spells initialized.", spellDBC.Rows - 1);
                spellDBC.Dispose();
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DBC File : Spells missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public void InitializeSpellChains()
        {
            try
            {
                var spellChainQuery = new DataTable();
                WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT spell_id, prev_spell FROM spell_chain", ref spellChainQuery);
                foreach (DataRow spellChain in spellChainQuery.Rows)
                    WorldServiceLocator._WS_Spells.SpellChains.Add(Conversions.ToInteger(spellChain["spell_id"]), Conversions.ToInteger(spellChain["prev_spell"]));
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Database: {0} SpellChains initialized.", spellChainQuery.Rows.Count);
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Database : SpellChains missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public void InitializeTaxiNodes()
        {
            try
            {
                var tmpDBC = new BufferedDbc("dbc" + Path.DirectorySeparatorChar + "TaxiNodes.dbc");
                float taxiPosX;
                float taxiPosY;
                float taxiPosZ;
                int taxiMapID;
                int taxiNode;
                int taxiMountTypeHorde;
                int taxiMountTypeAlliance;
                for (int i = 0, loopTo = tmpDBC.Rows - 1; i <= loopTo; i++)
                {
                    taxiNode = (int)tmpDBC.Item(i, 0);
                    taxiMapID = (int)tmpDBC.Item(i, 1);
                    taxiPosX = (float)tmpDBC.Item(i, 2, DBCValueType.DBC_FLOAT);
                    taxiPosY = (float)tmpDBC.Item(i, 3, DBCValueType.DBC_FLOAT);
                    taxiPosZ = (float)tmpDBC.Item(i, 4, DBCValueType.DBC_FLOAT);
                    taxiMountTypeHorde = (int)tmpDBC.Item(i, 14);
                    taxiMountTypeAlliance = (int)tmpDBC.Item(i, 15);
                    if (WorldServiceLocator._WorldServer.Config.Maps.Contains(taxiMapID.ToString()))
                    {
                        WorldServiceLocator._WS_DBCDatabase.TaxiNodes.Add(taxiNode, new WS_DBCDatabase.TTaxiNode(taxiPosX, taxiPosY, taxiPosZ, taxiMapID, taxiMountTypeHorde, taxiMountTypeAlliance));
                    }
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} TaxiNodes initialized.", tmpDBC.Rows - 1);
                tmpDBC.Dispose();
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DBC File : TaxiNodes missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public void InitializeTaxiPaths()
        {
            try
            {
                var tmpDBC = new BufferedDbc("dbc" + Path.DirectorySeparatorChar + "TaxiPath.dbc");
                int taxiNode;
                int taxiFrom;
                int taxiTo;
                int taxiPrice;
                for (int i = 0, loopTo = tmpDBC.Rows - 1; i <= loopTo; i++)
                {
                    taxiNode = (int)tmpDBC.Item(i, 0);
                    taxiFrom = (int)tmpDBC.Item(i, 1);
                    taxiTo = (int)tmpDBC.Item(i, 2);
                    taxiPrice = (int)tmpDBC.Item(i, 3);
                    WorldServiceLocator._WS_DBCDatabase.TaxiPaths.Add(taxiNode, new WS_DBCDatabase.TTaxiPath(taxiFrom, taxiTo, taxiPrice));
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} TaxiPaths initialized.", tmpDBC.Rows - 1);
                tmpDBC.Dispose();
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DBC File : TaxiPath missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public void InitializeTaxiPathNodes()
        {
            try
            {
                var tmpDBC = new BufferedDbc("dbc" + Path.DirectorySeparatorChar + "TaxiPathNode.dbc");

                // Dim taxiNode As Integer
                int taxiPath;
                int taxiSeq;
                int taxiMapID;
                float taxiPosX;
                float taxiPosY;
                float taxiPosZ;
                int taxiAction;
                int taxiWait;
                for (int i = 0, loopTo = tmpDBC.Rows - 1; i <= loopTo; i++)
                {
                    // taxiNode = tmpDBC.Item(i, 0)
                    taxiPath = (int)tmpDBC.Item(i, 1);
                    taxiSeq = (int)tmpDBC.Item(i, 2);
                    taxiMapID = (int)tmpDBC.Item(i, 3);
                    taxiPosX = (float)tmpDBC.Item(i, 4, DBCValueType.DBC_FLOAT);
                    taxiPosY = (float)tmpDBC.Item(i, 5, DBCValueType.DBC_FLOAT);
                    taxiPosZ = (float)tmpDBC.Item(i, 6, DBCValueType.DBC_FLOAT);
                    taxiAction = (int)tmpDBC.Item(i, 7);
                    taxiWait = (int)tmpDBC.Item(i, 8);
                    if (WorldServiceLocator._WorldServer.Config.Maps.Contains(taxiMapID.ToString()))
                    {
                        if (WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes.ContainsKey(taxiPath) == false)
                        {
                            WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes.Add(taxiPath, new Dictionary<int, WS_DBCDatabase.TTaxiPathNode>());
                        }

                        WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes[taxiPath].Add(taxiSeq, new WS_DBCDatabase.TTaxiPathNode(taxiPosX, taxiPosY, taxiPosZ, taxiMapID, taxiPath, taxiSeq, taxiAction, taxiWait));
                    }
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} TaxiPathNodes initialized.", tmpDBC.Rows - 1);
                tmpDBC.Dispose();
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DBC File : TaxiPathNode missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public void InitializeSkillLines()
        {
            try
            {
                var tmpDBC = new BufferedDbc("dbc" + Path.DirectorySeparatorChar + "SkillLine.dbc");
                int skillID;
                int skillLine;
                // Dim skillUnk1 As Integer
                // Dim skillName As String
                // Dim skillDescription As String
                // Dim skillSpellIcon As Integer

                for (int i = 0, loopTo = tmpDBC.Rows - 1; i <= loopTo; i++)
                {
                    skillID = (int)tmpDBC.Item(i, 0);
                    skillLine = (int)tmpDBC.Item(i, 1); // Type or Category?
                    // skillUnk1 = tmpDBC.Item(i, 2) ' May be needed in the future
                    // skillName = tmpDBC.Item(i, 3) ' May be needed in the future
                    // skillName = tmpDBC.Item(i, 3, DBCValueType.DBC_STRING)
                    // skillDescription = tmpDBC.Item(i, 12, DBCValueType.DBC_STRING)
                    // skillSpellIcon = tmpDBC.Item(i, 21)

                    WorldServiceLocator._WS_DBCDatabase.SkillLines[skillID] = skillLine;
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SkillLines initialized.", tmpDBC.Rows - 1);
                tmpDBC.Dispose();
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DBC File : SkillLines missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public void InitializeSkillLineAbility()
        {
            try
            {
                var tmpDBC = new BufferedDbc("dbc" + Path.DirectorySeparatorChar + "SkillLineAbility.dbc");
                for (int i = 0, loopTo = tmpDBC.Rows - 1; i <= loopTo; i++)
                {
                    var tmpSkillLineAbility = new WS_DBCDatabase.TSkillLineAbility()
                    {
                        ID = (int)tmpDBC.Item(i, 0),
                        SkillID = (int)tmpDBC.Item(i, 1),
                        SpellID = (int)tmpDBC.Item(i, 2),
                        Unknown1 = (int)tmpDBC.Item(i, 3), // May be needed in the future
                        Unknown2 = (int)tmpDBC.Item(i, 4), // May be needed in the future
                        Unknown3 = (int)tmpDBC.Item(i, 5), // May be needed in the future
                        Unknown4 = (int)tmpDBC.Item(i, 6), // May be needed in the future
                        Required_Skill_Value = (int)tmpDBC.Item(i, 7),
                        Forward_SpellID = (int)tmpDBC.Item(i, 8),
                        Unknown5 = (int)tmpDBC.Item(i, 9), // May be needed in the future
                        Max_Value = (int)tmpDBC.Item(i, 10),
                        Min_Value = (int)tmpDBC.Item(i, 11)
                    };
                    WorldServiceLocator._WS_DBCDatabase.SkillLineAbility.Add(tmpSkillLineAbility.ID, tmpSkillLineAbility);
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SkillLineAbilitys initialized.", tmpDBC.Rows - 1);
                tmpDBC.Dispose();
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DBC File : SkillLineAbility missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public void InitializeLocks()
        {
            try
            {
                var tmpDBC = new BufferedDbc("dbc" + Path.DirectorySeparatorChar + "Lock.dbc");
                int lockID;
                var keyType = new byte[5];
                var key = new int[5];
                int reqMining;
                int reqLockSkill;
                for (int i = 0, loopTo = tmpDBC.Rows - 1; i <= loopTo; i++)
                {
                    lockID = (int)tmpDBC.Item(i, 0);
                    keyType[0] = (byte)tmpDBC.Item(i, 1);
                    keyType[1] = (byte)tmpDBC.Item(i, 2);
                    keyType[2] = (byte)tmpDBC.Item(i, 3);
                    keyType[3] = (byte)tmpDBC.Item(i, 4);
                    keyType[4] = (byte)tmpDBC.Item(i, 5);
                    key[0] = (int)tmpDBC.Item(i, 9);
                    key[1] = (int)tmpDBC.Item(i, 10);
                    key[2] = (int)tmpDBC.Item(i, 11);
                    key[3] = (int)tmpDBC.Item(i, 12);
                    key[4] = (int)tmpDBC.Item(i, 13);
                    reqMining = (int)tmpDBC.Item(i, 17); // Not sure about this one leaving it like it is
                    reqLockSkill = (int)tmpDBC.Item(i, 17);
                    WorldServiceLocator._WS_Loot.Locks[lockID] = new WS_Loot.TLock(keyType, key, (short)reqMining, (short)reqLockSkill);
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} Locks initialized.", tmpDBC.Rows - 1);
                tmpDBC.Dispose();
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DBC File : Locks missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public void InitializeAreaTable()
        {
            try
            {
                var tmpDbc = new BufferedDbc("dbc" + Path.DirectorySeparatorChar + "AreaTable.dbc");
                int areaID;
                int areaMapID;
                int areaExploreFlag;
                int areaLevel;
                int areaZone;
                int areaZoneType;
                // Dim areaEXP As Integer
                // Dim areaTeam As Integer
                // Dim areaName As String

                for (int i = 0, loopTo = tmpDbc.Rows - 1; i <= loopTo; i++)
                {
                    areaID = (int)tmpDbc.Item(i, 0);
                    areaMapID = (int)tmpDbc.Item(i, 1); // May be needed in the future
                    areaZone = (int)tmpDbc.Item(i, 2);    // Parent Map
                    areaExploreFlag = (int)tmpDbc.Item(i, 3);
                    areaZoneType = (int)tmpDbc.Item(i, 4); // 312 For Cities - Flags
                    // 5        m_SoundProviderPref
                    // 6        m_SoundProviderPrefUnderwater
                    // 7        m_AmbienceID
                    // areaEXP = tmpDBC.Item(i, 8) ' May be needed in the future - m_ZoneMusic
                    // 9        m_IntroSound
                    areaLevel = (int)tmpDbc.Item(i, 10);
                    // areaName = tmpDBC.Item(i, 11) ' May be needed in the future
                    // 19 string flags
                    // areaTeam = tmpDBC.Item(i, 20)
                    // 24 = LiquidTypeOverride

                    if (areaLevel > 255)
                        areaLevel = 255;
                    if (areaLevel < 0)
                        areaLevel = 0;

                    // AreaTable(areaExploreFlag).Name = areaName
                    WorldServiceLocator._WS_Maps.AreaTable[areaExploreFlag] = new WS_Maps.TArea()
                    {
                        ID = areaID,
                        mapId = areaMapID,
                        Level = (byte)areaLevel,
                        Zone = areaZone,
                        ZoneType = areaZoneType
                    };
                    // AreaTable(areaExploreFlag).Team = areaTeam
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} Areas initialized.", tmpDbc.Rows - 1);
                tmpDbc.Dispose();
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DBC File : AreaTable missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public void InitializeEmotes()
        {
            try
            {
                var tmpDBC = new BufferedDbc("dbc" + Path.DirectorySeparatorChar + "Emotes.dbc");
                int emoteID;
                int emoteState;
                for (int i = 0, loopTo = tmpDBC.Rows - 1; i <= loopTo; i++)
                {
                    emoteID = (int)tmpDBC.Item(i, 0);
                    emoteState = (int)tmpDBC.Item(i, 4);
                    if (emoteID != 0)
                        WorldServiceLocator._WS_DBCDatabase.EmotesState[emoteID] = emoteState;
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} Emotes initialized.", tmpDBC.Rows - 1);
                tmpDBC.Dispose();
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DBC File : Emotes missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public void InitializeEmotesText()
        {
            try
            {
                var tmpDbc = new BufferedDbc("dbc" + Path.DirectorySeparatorChar + "EmotesText.dbc");
                int textEmoteID;
                int emoteID;
                // Dim EmoteID2 As Integer
                // Dim EmoteID3 As Integer
                // Dim EmoteID4 As Integer
                // Dim EmoteID5 As Integer
                // Dim EmoteID6 As Integer

                for (int i = 0, loopTo = tmpDbc.Rows - 1; i <= loopTo; i++)
                {
                    textEmoteID = (int)tmpDbc.Item(i, 0);
                    emoteID = (int)tmpDbc.Item(i, 2);
                    // EmoteID2 = tmpDBC.Item(i, 3) ' May be needed in the future
                    // EmoteID3 = tmpDBC.Item(i, 4) ' May be needed in the future
                    // EmoteID4 = tmpDBC.Item(i, 5) ' May be needed in the future
                    // EmoteID5 = tmpDBC.Item(i, 7) ' May be needed in the future
                    // EmoteID6 = tmpDBC.Item(i, 8) ' May be needed in the future

                    if (emoteID != 0)
                        WorldServiceLocator._WS_DBCDatabase.EmotesText[textEmoteID] = emoteID;
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} EmotesText initialized.", tmpDbc.Rows - 1);
                tmpDbc.Dispose();
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DBC File : EmotesText missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public void InitializeFactions()
        {
            try
            {
                var tmpDBC = new BufferedDbc("dbc" + Path.DirectorySeparatorChar + "Faction.dbc");
                int factionID;
                int factionFlag;
                var flags = new int[4];
                var reputationStats = new int[4];
                var reputationFlags = new int[4];
                // Dim factionName As String

                for (int i = 0, loopTo = tmpDBC.Rows - 1; i <= loopTo; i++)
                {
                    factionID = (int)tmpDBC.Item(i, 0);
                    factionFlag = (int)tmpDBC.Item(i, 1);
                    flags[0] = (int)tmpDBC.Item(i, 2);
                    flags[1] = (int)tmpDBC.Item(i, 3);
                    flags[2] = (int)tmpDBC.Item(i, 4);
                    flags[3] = (int)tmpDBC.Item(i, 5);
                    reputationStats[0] = (int)tmpDBC.Item(i, 10);
                    reputationStats[1] = (int)tmpDBC.Item(i, 11);
                    reputationStats[2] = (int)tmpDBC.Item(i, 12);
                    reputationStats[3] = (int)tmpDBC.Item(i, 13);
                    reputationFlags[0] = (int)tmpDBC.Item(i, 14);
                    reputationFlags[1] = (int)tmpDBC.Item(i, 15);
                    reputationFlags[2] = (int)tmpDBC.Item(i, 16);
                    reputationFlags[3] = (int)tmpDBC.Item(i, 17);
                    // factionName = tmpDBC.Item(i, 19) ' May be needed in the future

                    WorldServiceLocator._WS_DBCDatabase.FactionInfo[factionID] = new WS_DBCDatabase.TFaction((short)factionID, (short)factionFlag, flags[0], flags[1], flags[2], flags[3], reputationStats[0], reputationStats[1], reputationStats[2], reputationStats[3], reputationFlags[0], reputationFlags[1], reputationFlags[2], reputationFlags[3]);
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} Factions initialized.", tmpDBC.Rows - 1);
                tmpDBC.Dispose();
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DBC File : Factions missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public void InitializeFactionTemplates()
        {
            try
            {
                // Loading from DBC
                var tmpDBC = new BufferedDbc("dbc" + Path.DirectorySeparatorChar + "FactionTemplate.dbc");
                int templateID;
                for (int i = 0, loopTo = tmpDBC.Rows - 1; i <= loopTo; i++)
                {
                    templateID = (int)tmpDBC.Item(i, 0);
                    WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo.Add(templateID, new WS_DBCDatabase.TFactionTemplate());
                    WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].FactionID = (int)tmpDBC.Item(i, 1);
                    WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].ourMask = (uint)tmpDBC.Item(i, 3);
                    WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].friendMask = (uint)tmpDBC.Item(i, 4);
                    WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].enemyMask = (uint)tmpDBC.Item(i, 5);
                    WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].enemyFaction1 = (int)tmpDBC.Item(i, 6);
                    WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].enemyFaction2 = (int)tmpDBC.Item(i, 7);
                    WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].enemyFaction3 = (int)tmpDBC.Item(i, 8);
                    WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].enemyFaction4 = (int)tmpDBC.Item(i, 9);
                    WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].friendFaction1 = (int)tmpDBC.Item(i, 10);
                    WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].friendFaction2 = (int)tmpDBC.Item(i, 11);
                    WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].friendFaction3 = (int)tmpDBC.Item(i, 12);
                    WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].friendFaction4 = (int)tmpDBC.Item(i, 13);
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} FactionTemplates initialized.", tmpDBC.Rows - 1);
                tmpDBC.Dispose();
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DBC File : FactionsTemplates missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public void InitializeCharRaces()
        {
            try
            {
                // Loading from DBC
                var tmpDBC = new BufferedDbc("dbc" + Path.DirectorySeparatorChar + "ChrRaces.dbc");
                int raceID;
                int factionID;
                int modelM;
                int modelF;
                int teamID; // 1 = Horde / 7 = Alliance
                uint taxiMask;
                int cinematicID;
                string name;
                for (int i = 0, loopTo = tmpDBC.Rows - 1; i <= loopTo; i++)
                {
                    raceID = (int)tmpDBC.Item(i, 0);
                    factionID = (int)tmpDBC.Item(i, 2);
                    modelM = (int)tmpDBC.Item(i, 4);
                    modelF = (int)tmpDBC.Item(i, 5);
                    teamID = (int)tmpDBC.Item(i, 8);
                    taxiMask = (uint)tmpDBC.Item(i, 14);
                    cinematicID = (int)tmpDBC.Item(i, 16);
                    name = (string)tmpDBC.Item(i, 17, DBCValueType.DBC_STRING);
                    WorldServiceLocator._WS_DBCDatabase.CharRaces[(byte)raceID] = new WS_DBCDatabase.TCharRace((short)factionID, modelM, modelF, (byte)teamID, taxiMask, cinematicID, name);
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} CharRaces initialized.", tmpDBC.Rows - 1);
                tmpDBC.Dispose();
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DBC File : CharRaces missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public void InitializeCharClasses()
        {
            try
            {
                // Loading from DBC
                var tmpDBC = new BufferedDbc("dbc" + Path.DirectorySeparatorChar + "ChrClasses.dbc");
                int classID;
                int cinematicID;
                for (int i = 0, loopTo = tmpDBC.Rows - 1; i <= loopTo; i++)
                {
                    classID = (int)tmpDBC.Item(i, 0);
                    cinematicID = (int)tmpDBC.Item(i, 5);
                    WorldServiceLocator._WS_DBCDatabase.CharClasses[(byte)classID] = new WS_DBCDatabase.TCharClass(cinematicID);
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} CharClasses initialized.", tmpDBC.Rows - 1);
                tmpDBC.Dispose();
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DBC File : CharRaces missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public void InitializeDurabilityCosts()
        {
            try
            {
                var tmpDBC = new BufferedDbc("dbc" + Path.DirectorySeparatorChar + "DurabilityCosts.dbc");
                int itemBroken;
                int itemType;
                int itemPrice;
                for (int i = 0, loopTo = tmpDBC.Rows - 1; i <= loopTo; i++)
                {
                    itemBroken = (int)tmpDBC.Item(i, 0);
                    var loopTo1 = tmpDBC.Columns - 1;
                    for (itemType = 1; itemType <= loopTo1; itemType++)
                    {
                        itemPrice = (int)tmpDBC.Item(i, itemType);
                        WorldServiceLocator._WS_DBCDatabase.DurabilityCosts[itemBroken, itemType - 1] = (short)itemPrice;
                    }
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} DurabilityCosts initialized.", tmpDBC.Rows - 1);
                tmpDBC.Dispose();
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DBC File : DurabilityCosts missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public void LoadTalentDbc()
        {
            try
            {
                var dbc = new BufferedDbc("dbc" + Path.DirectorySeparatorChar + "Talent.dbc");
                WS_DBCDatabase.TalentInfo tmpInfo;
                for (int i = 0, loopTo = dbc.Rows - 1; i <= loopTo; i++)
                {
                    tmpInfo = new WS_DBCDatabase.TalentInfo()
                    {
                        TalentID = (int)dbc.Item(i, 0),
                        TalentTab = (int)dbc.Item(i, 1),
                        Row = (int)dbc.Item(i, 2),
                        Col = (int)dbc.Item(i, 3)
                    };
                    tmpInfo.RankID[0] = (int)dbc.Item(i, 4);
                    tmpInfo.RankID[1] = (int)dbc.Item(i, 5);
                    tmpInfo.RankID[2] = (int)dbc.Item(i, 6);
                    tmpInfo.RankID[3] = (int)dbc.Item(i, 7);
                    tmpInfo.RankID[4] = (int)dbc.Item(i, 8);
                    tmpInfo.RequiredTalent[0] = (int)dbc.Item(i, 13); // dependson
                    // tmpInfo.RequiredTalent(1) = DBC.Item(i, 14) ' ???
                    // tmpInfo.RequiredTalent(2) = DBC.Item(i, 15) ' ???
                    tmpInfo.RequiredPoints[0] = (int)dbc.Item(i, 16); // dependsonrank
                    // tmpInfo.RequiredPoints(1) = DBC.Item(i, 17) ' ???
                    // tmpInfo.RequiredPoints(2) = DBC.Item(i, 18) ' ???

                    WorldServiceLocator._WS_DBCDatabase.Talents.Add(tmpInfo.TalentID, tmpInfo);
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} Talents initialized.", dbc.Rows - 1);
                dbc.Dispose();
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DBC File : Talents missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public void LoadTalentTabDbc()
        {
            try
            {
                var dbc = new BufferedDbc("dbc" + Path.DirectorySeparatorChar + "TalentTab.dbc");
                int talentTab;
                int talentMask;
                // Dim TalentTabPage As Integer

                for (int i = 0, loopTo = dbc.Rows - 1; i <= loopTo; i++)
                {
                    talentTab = (int)dbc.Item(i, 0);
                    talentMask = (int)dbc.Item(i, 12);
                    // TalentTabPage = dbc.Item(i, 13) ' May be needed in the future

                    WorldServiceLocator._WS_DBCDatabase.TalentsTab.Add(talentTab, talentMask);
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} Talent tabs initialized.", dbc.Rows - 1);
                dbc.Dispose();
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DBC File : TalentTab missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public void LoadAuctionHouseDbc()
        {
            try
            {
                var dbc = new BufferedDbc("dbc" + Path.DirectorySeparatorChar + "AuctionHouse.dbc");
                int ahId;
                int fee;
                int tax;

                // What the hell is this doing? o_O

                for (int i = 0, loopTo = dbc.Rows - 1; i <= loopTo; i++)
                {
                    ahId = (int)dbc.Item(i, 0);
                    fee = (int)dbc.Item(i, 2);
                    tax = (int)dbc.Item(i, 3);

                    // TODO: This needs to be put into a class or dictionary collection
                    WorldServiceLocator._WS_Auction.AuctionID = ahId;
                    WorldServiceLocator._WS_Auction.AuctionFee = fee;
                    WorldServiceLocator._WS_Auction.AuctionTax = tax;
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} AuctionHouses initialized.", dbc.Rows - 1);
                dbc.Dispose();
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DBC File : AuctionHouse missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public void LoadSpellItemEnchantments()
        {
            try
            {
                var dbc = new BufferedDbc("dbc" + Path.DirectorySeparatorChar + "SpellItemEnchantment.dbc");
                int id;
                var type = new int[3];
                var amount = new int[3];
                var spellID = new int[3];
                int auraID;
                int slot;
                // Dim EnchantmentConditions As Integer

                for (int i = 0, loopTo = dbc.Rows - 1; i <= loopTo; i++)
                {
                    id = (int)dbc.Item(i, 0);
                    type[0] = (int)dbc.Item(i, 1);
                    type[1] = (int)dbc.Item(i, 2);
                    // Type(2) = DBC.Item(i, 3)
                    amount[0] = (int)dbc.Item(i, 4);
                    amount[1] = (int)dbc.Item(i, 7);
                    // Amount(2) = DBC.Item(i, 6)
                    spellID[0] = (int)dbc.Item(i, 10);
                    spellID[1] = (int)dbc.Item(i, 11);
                    // SpellID(2) = DBC.Item(i, 12)
                    auraID = (int)dbc.Item(i, 22);
                    slot = (int)dbc.Item(i, 23);
                    // EnchantmentConditions = DBC.Item(i, 23) ' TODO: Correct?

                    WorldServiceLocator._WS_DBCDatabase.SpellItemEnchantments.Add(id, new WS_DBCDatabase.TSpellItemEnchantment(type, amount, spellID, auraID, slot)); // , EnchantmentConditions))
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SpellItemEnchantments initialized.", dbc.Rows - 1);
                dbc.Dispose();
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DBC File : SpellItemEnchantments missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public void LoadItemSet()
        {
            try
            {
                var dbc = new BufferedDbc("dbc" + Path.DirectorySeparatorChar + "ItemSet.dbc");
                int id;
                string name;
                var itemID = new int[8];
                var spellID = new int[8];
                var itemCount = new int[8];
                var requiredSkillID = default(int);
                var requiredSkillValue = default(int);
                for (int i = 0, loopTo = dbc.Rows - 1; i <= loopTo; i++)
                {
                    id = (int)dbc.Item(i, 0);
                    name = (string)dbc.Item(i, 1, DBCValueType.DBC_STRING);
                    itemID[0] = (int)dbc.Item(i, 10); // 10 - 26
                    itemID[1] = (int)dbc.Item(i, 11);
                    itemID[2] = (int)dbc.Item(i, 12);
                    itemID[3] = (int)dbc.Item(i, 13);
                    itemID[4] = (int)dbc.Item(i, 14);
                    itemID[5] = (int)dbc.Item(i, 15);
                    itemID[6] = (int)dbc.Item(i, 16);
                    itemID[7] = (int)dbc.Item(i, 17);
                    // SpellID(0) = DBC.Item(i, 27) ' 27 - 34
                    // SpellID(1) = DBC.Item(i, 28)
                    // SpellID(2) = DBC.Item(i, 29)
                    // SpellID(3) = DBC.Item(i, 30)
                    // SpellID(4) = DBC.Item(i, 31)
                    // SpellID(5) = DBC.Item(i, 32)
                    // SpellID(6) = DBC.Item(i, 33)
                    // SpellID(7) = DBC.Item(i, 34)
                    // ItemCount(0) = DBC.Item(i, 35) ' Items To Trigger Spell?
                    // ItemCount(1) = DBC.Item(i, 36)
                    // ItemCount(2) = DBC.Item(i, 37)
                    // ItemCount(3) = DBC.Item(i, 38)
                    // ItemCount(4) = DBC.Item(i, 39)
                    // ItemCount(5) = DBC.Item(i, 40)
                    // ItemCount(6) = DBC.Item(i, 41)
                    // ItemCount(7) = DBC.Item(i, 42)
                    // Required_Skill_ID = DBC.Item(i, 43)
                    // Required_Skill_Value = DBC.Item(i, 44)

                    WorldServiceLocator._WS_DBCDatabase.ItemSet.Add(id, new WS_DBCDatabase.TItemSet(name, itemID, spellID, itemCount, requiredSkillID, requiredSkillValue));
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} ItemSets initialized.", dbc.Rows - 1);
                dbc.Dispose();
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DBC File : ItemSet missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public void LoadItemDisplayInfoDbc()
        {
            try
            {
                var dbc = new BufferedDbc("dbc" + Path.DirectorySeparatorChar + "ItemDisplayInfo.dbc");
                WS_DBCDatabase.TItemDisplayInfo tmpItemDisplayInfo;
                for (int i = 0, loopTo = dbc.Rows - 1; i <= loopTo; i++)
                {
                    tmpItemDisplayInfo = new WS_DBCDatabase.TItemDisplayInfo()
                    {
                        ID = (int)dbc.Item(i, 0),
                        RandomPropertyChance = (int)dbc.Item(i, 11),
                        Unknown = (int)dbc.Item(i, 22)
                    };
                    WorldServiceLocator._WS_DBCDatabase.ItemDisplayInfo.Add(tmpItemDisplayInfo.ID, tmpItemDisplayInfo);
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} ItemDisplayInfos initialized.", dbc.Rows - 1);
                dbc.Dispose();
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DBC File : ItemDisplayInfo missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public void LoadItemRandomPropertiesDbc()
        {
            try
            {
                var dbc = new BufferedDbc("dbc" + Path.DirectorySeparatorChar + "ItemRandomProperties.dbc");
                WS_DBCDatabase.TItemRandomPropertiesInfo tmpInfo;
                for (int i = 0, loopTo = dbc.Rows - 1; i <= loopTo; i++)
                {
                    tmpInfo = new WS_DBCDatabase.TItemRandomPropertiesInfo() { ID = (int)dbc.Item(i, 0) };
                    tmpInfo.Enchant_ID[0] = (int)dbc.Item(i, 2);
                    tmpInfo.Enchant_ID[1] = (int)dbc.Item(i, 3);
                    tmpInfo.Enchant_ID[2] = (int)dbc.Item(i, 4);
                    WorldServiceLocator._WS_DBCDatabase.ItemRandomPropertiesInfo.Add(tmpInfo.ID, tmpInfo);
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} ItemRandomProperties initialized.", dbc.Rows - 1);
                dbc.Dispose();
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DBC File : ItemRandomProperties missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public void LoadCreatureGossip()
        {
            try
            {
                var gossipQuery = new DataTable();
                WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT * FROM npc_gossip;", ref gossipQuery);
                ulong guid;
                foreach (DataRow gossip in gossipQuery.Rows)
                {
                    guid = Conversions.ToULong(gossip["npc_guid"]);
                    if (WorldServiceLocator._WS_DBCDatabase.CreatureGossip.ContainsKey(guid) == false)
                    {
                        WorldServiceLocator._WS_DBCDatabase.CreatureGossip.Add(guid, Conversions.ToInteger(gossip["textid"]));
                    }
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Database: {0} creature gossips initialized.", WorldServiceLocator._WS_DBCDatabase.CreatureGossip.Count);
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Database : npc_gossip missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public void LoadCreatureFamilyDbc()
        {
            try
            {
                var dbc = new BufferedDbc("dbc" + Path.DirectorySeparatorChar + "CreatureFamily.dbc");
                WS_DBCDatabase.CreatureFamilyInfo tmpInfo;
                for (int i = 0, loopTo = dbc.Rows - 1; i <= loopTo; i++)
                {
                    tmpInfo = new WS_DBCDatabase.CreatureFamilyInfo()
                    {
                        ID = (int)dbc.Item(i, 0),
                        Unknown1 = (int)dbc.Item(i, 5),
                        Unknown2 = (int)dbc.Item(i, 6),
                        PetFoodID = (int)dbc.Item(i, 7),
                        Name = (string)dbc.Item(i, 12, DBCValueType.DBC_STRING)
                    };
                    WorldServiceLocator._WS_DBCDatabase.CreaturesFamily.Add(tmpInfo.ID, tmpInfo);
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} CreatureFamilys initialized.", dbc.Rows - 1);
                dbc.Dispose();
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DBC File : CreatureFamily missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public void LoadCreatureMovements()
        {
            try
            {
                var movementsQuery = new DataTable();
                WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT * FROM waypoint_data ORDER BY id, point;", ref movementsQuery);
                int id;
                foreach (DataRow movement in movementsQuery.Rows)
                {
                    id = Conversions.ToInteger(movement["id"]);
                    if (WorldServiceLocator._WS_DBCDatabase.CreatureMovement.ContainsKey(id) == false)
                    {
                        WorldServiceLocator._WS_DBCDatabase.CreatureMovement.Add(id, new Dictionary<int, WS_DBCDatabase.CreatureMovePoint>());
                    }

                    WorldServiceLocator._WS_DBCDatabase.CreatureMovement[id].Add(Conversions.ToInteger(movement["point"]), new WS_DBCDatabase.CreatureMovePoint(Conversions.ToSingle(movement["position_x"]), Conversions.ToSingle(movement["position_y"]), Conversions.ToSingle(movement["position_z"]), Conversions.ToInteger(movement["delay"]), Conversions.ToInteger(movement["move_flag"]), Conversions.ToInteger(movement["action"]), Conversions.ToInteger(movement["action_chance"])));
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Database: {0} creature movements for {1} creatures initialized.", movementsQuery.Rows.Count, WorldServiceLocator._WS_DBCDatabase.CreatureMovement.Count);
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Database : Waypoint_Data missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public void LoadCreatureEquipTable()
        {
            try
            {
                var equipQuery = new DataTable();
                WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT * FROM creature_equip_template_raw;", ref equipQuery);
                // _WorldServer.WorldDatabase.Query("SELECT creature_equip_template.*, COALESCE(A.displayid,0) AS equipmodel1, COALESCE(B.displayid,0) AS equipmodel2,  COALESCE(C.displayid,0) AS equipmodel3, COALESCE(A.`inventory_type`,0) AS equipslot1, COALESCE(B.`inventory_type`,0) AS equipslot2,  COALESCE(C.`inventory_type`,0) AS equipslot3, 0 AS equipinfo1,  0 AS equipinfo2,  0 AS equipinfo3  FROM creature_equip_template LEFT JOIN `creature_item_template` A ON creature_equip_template.equipentry1 = A.entry LEFT JOIN `creature_item_template` B ON creature_equip_template.equipentry2 = B.entry LEFT JOIN `creature_item_template` C ON creature_equip_template.equipentry3 = C.entry;", equipQuery)

                int entry;
                foreach (DataRow equipInfo in equipQuery.Rows)
                {
                    entry = Conversions.ToInteger(equipInfo["entry"]);
                    if (WorldServiceLocator._WS_DBCDatabase.CreatureEquip.ContainsKey(entry))
                        continue;
                    try
                    {
                        WorldServiceLocator._WS_DBCDatabase.CreatureEquip.Add(entry, new WS_DBCDatabase.CreatureEquipInfo(Conversions.ToInteger(equipInfo["equipmodel1"]), Conversions.ToInteger(equipInfo["equipmodel2"]), Conversions.ToInteger(equipInfo["equipmodel3"]), Conversions.ToUInteger(equipInfo["equipinfo1"]), Conversions.ToUInteger(equipInfo["equipinfo2"]), Conversions.ToUInteger(equipInfo["equipinfo3"]), Conversions.ToInteger(equipInfo["equipslot1"]), Conversions.ToInteger(equipInfo["equipslot2"]), Conversions.ToInteger(equipInfo["equipslot3"])));
                    }
                    catch (DataException)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(string.Format("Creature_Equip_Template_raw : Unable to equip items {0} for Creature ", entry));
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Database: {0} creature equips initialized.", equipQuery.Rows.Count);
            }
            catch (DataException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Database : Creature_Equip_Template_raw missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public void LoadCreatureModelInfo()
        {
            try
            {
                var modelQuery = new DataTable();
                WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT * FROM creature_model_info;", ref modelQuery);
                int entry;
                foreach (DataRow modelInfo in modelQuery.Rows)
                {
                    entry = Conversions.ToInteger(modelInfo["modelid"]);
                    if (WorldServiceLocator._WS_DBCDatabase.CreatureModel.ContainsKey(entry))
                        continue;
                    WorldServiceLocator._WS_DBCDatabase.CreatureModel.Add(entry, new WS_DBCDatabase.CreatureModelInfo(Conversions.ToSingle(modelInfo["bounding_radius"]), Conversions.ToSingle(modelInfo["combat_reach"]), Conversions.ToByte(modelInfo["gender"]), Conversions.ToInteger(modelInfo["modelid_other_gender"])));
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Database: {0} creature models initialized.", modelQuery.Rows.Count);
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Database : Creature_Model_Info missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public void LoadQuestStartersAndFinishers()
        {
            var questStarters = new DataTable();
            WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT * FROM quest_relations where actor=0 and role =0;", ref questStarters);
            foreach (DataRow starter in questStarters.Rows)
            {
                int entry = Conversions.ToInteger(starter["entry"]);
                int quest = Conversions.ToInteger(starter["quest"]);
                if (WorldServiceLocator._WorldServer.CreatureQuestStarters.ContainsKey(entry) == false)
                    WorldServiceLocator._WorldServer.CreatureQuestStarters.Add(entry, new List<int>());
                WorldServiceLocator._WorldServer.CreatureQuestStarters[entry].Add(quest);
            }

            int questStartersAmount = questStarters.Rows.Count;
            questStarters.Clear();
            WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT * FROM quest_relations where actor=1 and role=0;", ref questStarters);
            foreach (DataRow starter in questStarters.Rows)
            {
                int entry = Conversions.ToInteger(starter["entry"]);
                int quest = Conversions.ToInteger(starter["quest"]);
                if (WorldServiceLocator._WorldServer.GameobjectQuestStarters.ContainsKey(entry) == false)
                    WorldServiceLocator._WorldServer.GameobjectQuestStarters.Add(entry, new List<int>());
                WorldServiceLocator._WorldServer.GameobjectQuestStarters[entry].Add(quest);
            }

            questStartersAmount += questStarters.Rows.Count;
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Database: {0} queststarters initated for {1} creatures and {2} gameobjects.", questStartersAmount, WorldServiceLocator._WorldServer.CreatureQuestStarters.Count, WorldServiceLocator._WorldServer.GameobjectQuestStarters.Count);
            questStarters.Clear();
            var questFinishers = new DataTable();
            WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT * FROM quest_relations where actor=0 and role=1;", ref questFinishers);
            foreach (DataRow starter in questFinishers.Rows)
            {
                int entry = Conversions.ToInteger(starter["entry"]);
                int quest = Conversions.ToInteger(starter["quest"]);
                if (WorldServiceLocator._WorldServer.CreatureQuestFinishers.ContainsKey(entry) == false)
                    WorldServiceLocator._WorldServer.CreatureQuestFinishers.Add(entry, new List<int>());
                WorldServiceLocator._WorldServer.CreatureQuestFinishers[entry].Add(quest);
            }

            int questFinishersAmount = questFinishers.Rows.Count;
            questFinishers.Clear();
            WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT * FROM quest_relations where actor=1 and role=1;", ref questFinishers);
            foreach (DataRow starter in questFinishers.Rows)
            {
                int entry = Conversions.ToInteger(starter["entry"]);
                int quest = Conversions.ToInteger(starter["quest"]);
                if (WorldServiceLocator._WorldServer.GameobjectQuestFinishers.ContainsKey(entry) == false)
                    WorldServiceLocator._WorldServer.GameobjectQuestFinishers.Add(entry, new List<int>());
                WorldServiceLocator._WorldServer.GameobjectQuestFinishers[entry].Add(quest);
            }

            questFinishersAmount += questFinishers.Rows.Count;
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Database: {0} questfinishers initated for {1} creatures and {2} gameobjects.", questFinishersAmount, WorldServiceLocator._WorldServer.CreatureQuestFinishers.Count, WorldServiceLocator._WorldServer.GameobjectQuestFinishers.Count);
            questFinishers.Clear();
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
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
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public void LoadWeather()
        {
            try
            {
                var weatherQuery = new DataTable();
                WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT * FROM game_weather;", ref weatherQuery);
                foreach (DataRow weather in weatherQuery.Rows)
                {
                    int zone = Conversions.ToInteger(weather["zone"]);
                    if (WorldServiceLocator._WS_Weather.WeatherZones.ContainsKey(zone) == false)
                    {
                        var zoneChanges = new WS_Weather.WeatherZone(zone);
                        zoneChanges.Seasons[0] = new WS_Weather.WeatherSeasonChances(Conversions.ToInteger(weather["spring_rain_chance"]), Conversions.ToInteger(weather["spring_snow_chance"]), Conversions.ToInteger(weather["spring_storm_chance"]));
                        zoneChanges.Seasons[1] = new WS_Weather.WeatherSeasonChances(Conversions.ToInteger(weather["summer_rain_chance"]), Conversions.ToInteger(weather["summer_snow_chance"]), Conversions.ToInteger(weather["summer_storm_chance"]));
                        zoneChanges.Seasons[2] = new WS_Weather.WeatherSeasonChances(Conversions.ToInteger(weather["fall_rain_chance"]), Conversions.ToInteger(weather["fall_snow_chance"]), Conversions.ToInteger(weather["fall_storm_chance"]));
                        zoneChanges.Seasons[3] = new WS_Weather.WeatherSeasonChances(Conversions.ToInteger(weather["winter_rain_chance"]), Conversions.ToInteger(weather["winter_snow_chance"]), Conversions.ToInteger(weather["winter_storm_chance"]));
                        WorldServiceLocator._WS_Weather.WeatherZones.Add(zone, zoneChanges);
                    }
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Database: {0} Weather zones initialized.", weatherQuery.Rows.Count);
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Database : TransportQuery missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
    }
}