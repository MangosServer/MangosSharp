//
//  Copyright (C) 2013-2020 getMaNGOS <https:\\getmangos.eu>
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
using Mangos.Common;
using Mangos.Common.DataStores;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Spell;
using Mangos.World.Loots;
using Mangos.World.Maps;
using Mangos.World.Spells;
using Mangos.World.Weather;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.DataStores
{
	public class WS_DBCLoad
	{
		public void InitializeSpellRadius()
		{
			checked
			{
				try
				{
					BufferedDbc tmpDBC = new BufferedDbc("dbc" + Conversions.ToString(Path.DirectorySeparatorChar) + "SpellRadius.dbc");
					int num = tmpDBC.Rows - 1;
					for (int i = 0; i <= num; i++)
					{
						int radiusID = tmpDBC.Read<int>(i, 0);
						float radiusValue = tmpDBC.Read<float>(i, 1);
						WorldServiceLocator._WS_Spells.SpellRadius[radiusID] = radiusValue;
					}
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SpellRadius initialized.", tmpDBC.Rows - 1);
					tmpDBC.Dispose();
				}
				catch (DirectoryNotFoundException ex)
				{
					ProjectData.SetProjectError(ex);
					DirectoryNotFoundException e = ex;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("DBC File : SpellRadius missing.");
					Console.ForegroundColor = ConsoleColor.Gray;
					ProjectData.ClearProjectError();
				}
			}
		}

		public void InitializeSpellCastTime()
		{
			checked
			{
				try
				{
					BufferedDbc tmpDBC = new BufferedDbc("dbc" + Conversions.ToString(Path.DirectorySeparatorChar) + "SpellCastTimes.dbc");
					int num = tmpDBC.Rows - 1;
					for (int i = 0; i <= num; i++)
					{
						int spellCastID = tmpDBC.Read<int>(i, 0);
						int spellCastTimeS = tmpDBC.Read<int>(i, 1);
						WorldServiceLocator._WS_Spells.SpellCastTime[spellCastID] = spellCastTimeS;
					}
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SpellCastTimes initialized.", tmpDBC.Rows - 1);
					tmpDBC.Dispose();
				}
				catch (DirectoryNotFoundException ex)
				{
					ProjectData.SetProjectError(ex);
					DirectoryNotFoundException e = ex;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("DBC File : SpellCastTimes missing.");
					Console.ForegroundColor = ConsoleColor.Gray;
					ProjectData.ClearProjectError();
				}
			}
		}

		public void InitializeSpellRange()
		{
			checked
			{
				try
				{
					BufferedDbc tmpDBC = new BufferedDbc("dbc" + Conversions.ToString(Path.DirectorySeparatorChar) + "SpellRange.dbc");
					int num = tmpDBC.Rows - 1;
					for (int i = 0; i <= num; i++)
					{
						int spellRangeIndex = tmpDBC.Read<int>(i, 0);
						float spellRangeMax = tmpDBC.Read<float>(i, 2);
						WorldServiceLocator._WS_Spells.SpellRange[spellRangeIndex] = spellRangeMax;
					}
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SpellRanges initialized.", tmpDBC.Rows - 1);
					tmpDBC.Dispose();
				}
				catch (DirectoryNotFoundException ex)
				{
					ProjectData.SetProjectError(ex);
					DirectoryNotFoundException e = ex;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("DBC File : SpellRanges missing.");
					Console.ForegroundColor = ConsoleColor.Gray;
					ProjectData.ClearProjectError();
				}
			}
		}

		public void InitializeSpellShapeShift()
		{
			checked
			{
				try
				{
					BufferedDbc tmpDBC = new BufferedDbc("dbc" + Conversions.ToString(Path.DirectorySeparatorChar) + "SpellShapeshiftForm.dbc");
					int num = tmpDBC.Rows - 1;
					for (int i = 0; i <= num; i++)
					{
						int id = tmpDBC.Read<int>(i, 0);
						int flags1 = tmpDBC.Read<int>(i, 11);
						int creatureType = tmpDBC.Read<int>(i, 12);
						int attackSpeed = tmpDBC.Read<int>(i, 13);
						WorldServiceLocator._WS_DBCDatabase.SpellShapeShiftForm.Add(new WS_DBCDatabase.TSpellShapeshiftForm(id, flags1, creatureType, attackSpeed));
					}
					tmpDBC.Dispose();
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SpellShapeshiftForms initialized.", tmpDBC.Rows - 1);
				}
				catch (DirectoryNotFoundException ex)
				{
					ProjectData.SetProjectError(ex);
					DirectoryNotFoundException e = ex;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("DBC File : SpellShapeshiftForms missing.");
					Console.ForegroundColor = ConsoleColor.Gray;
					ProjectData.ClearProjectError();
				}
			}
		}

		public void InitializeSpellFocusObject()
		{
			checked
			{
				try
				{
					BufferedDbc tmpDBC = new BufferedDbc("dbc" + Conversions.ToString(Path.DirectorySeparatorChar) + "SpellFocusObject.dbc");
					int num = tmpDBC.Rows - 1;
					for (int i = 0; i <= num; i++)
					{
						int spellFocusIndex = tmpDBC.Read<int>(i, 0);
						string spellFocusObjectName = tmpDBC.Read<string>(i, 1);
						WorldServiceLocator._WS_Spells.SpellFocusObject[spellFocusIndex] = spellFocusObjectName;
					}
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SpellFocusObjects initialized.", tmpDBC.Rows - 1);
					tmpDBC.Dispose();
				}
				catch (DirectoryNotFoundException ex)
				{
					ProjectData.SetProjectError(ex);
					DirectoryNotFoundException e = ex;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("DBC File : SpellFocusObjects missing.");
					Console.ForegroundColor = ConsoleColor.Gray;
					ProjectData.ClearProjectError();
				}
			}
		}

		public void InitializeSpellDuration()
		{
			checked
			{
				try
				{
					BufferedDbc tmpDBC = new BufferedDbc("dbc" + Conversions.ToString(Path.DirectorySeparatorChar) + "SpellDuration.dbc");
					int num = tmpDBC.Rows - 1;
					for (int i = 0; i <= num; i++)
					{
						int spellDurationIndex = tmpDBC.Read<int>(i, 0);
						int spellDurationValue = tmpDBC.Read<int>(i, 1);
						WorldServiceLocator._WS_Spells.SpellDuration[spellDurationIndex] = spellDurationValue;
					}
					tmpDBC.Dispose();
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SpellDurations initialized.", tmpDBC.Rows - 1);
				}
				catch (DirectoryNotFoundException ex)
				{
					ProjectData.SetProjectError(ex);
					DirectoryNotFoundException e = ex;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("DBC File : SpellDurations missing.");
					Console.ForegroundColor = ConsoleColor.Gray;
					ProjectData.ClearProjectError();
				}
			}
		}

		public void InitializeSpells()
		{
			checked
			{
				try
				{
					BufferedDbc spellDBC = new BufferedDbc("dbc" + Conversions.ToString(Path.DirectorySeparatorChar) + "Spell.dbc");
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: Initializing Spells - This may take a few moments....");
					long num = spellDBC.Rows - 1;
					for (long i = 0L; i <= num; i++)
					{
						try
						{
							int id = spellDBC.Read<int>(i, 0);
							WorldServiceLocator._WS_Spells.SPELLs[id] = new WS_Spells.SpellInfo
							{
								ID = id,
								School = spellDBC.Read<int>(i, 1),
								Category = spellDBC.Read<int>(i, 2),
								DispellType = spellDBC.Read<int>(i, 4),
								Mechanic = spellDBC.Read<int>(i, 5),
								Attributes = spellDBC.Read<int>(i, 6),
								AttributesEx = spellDBC.Read<int>(i, 7),
								AttributesEx2 = spellDBC.Read<int>(i, 8),
								RequredCasterStance = spellDBC.Read<int>(i, 11),
								ShapeshiftExclude = spellDBC.Read<int>(i, 12),
								Target = spellDBC.Read<int>(i, 13),
								TargetCreatureType = spellDBC.Read<int>(i, 14),
								FocusObjectIndex = spellDBC.Read<int>(i, 15),
								CasterAuraState = spellDBC.Read<int>(i, 16),
								TargetAuraState = spellDBC.Read<int>(i, 17),
								SpellCastTimeIndex = spellDBC.Read<int>(i, 18),
								SpellCooldown = spellDBC.Read<int>(i, 19),
								CategoryCooldown = spellDBC.Read<int>(i, 20),
								interruptFlags = spellDBC.Read<int>(i, 21),
								auraInterruptFlags = spellDBC.Read<int>(i, 22),
								channelInterruptFlags = spellDBC.Read<int>(i, 23),
								procFlags = spellDBC.Read<int>(i, 24),
								procChance = spellDBC.Read<int>(i, 25),
								procCharges = spellDBC.Read<int>(i, 26),
								maxLevel = spellDBC.Read<int>(i, 27),
								baseLevel = spellDBC.Read<int>(i, 28),
								spellLevel = spellDBC.Read<int>(i, 29),
								DurationIndex = spellDBC.Read<int>(i, 30),
								powerType = spellDBC.Read<int>(i, 31),
								manaCost = spellDBC.Read<int>(i, 32),
								manaCostPerlevel = spellDBC.Read<int>(i, 33),
								manaPerSecond = spellDBC.Read<int>(i, 34),
								manaPerSecondPerLevel = spellDBC.Read<int>(i, 35),
								rangeIndex = spellDBC.Read<int>(i, 36),
								Speed = spellDBC.Read<float>(i, 37),
								modalNextSpell = spellDBC.Read<int>(i, 38),
								maxStack = spellDBC.Read<int>(i, 39)
							};

							WorldServiceLocator._WS_Spells.SPELLs[id].Totem[0] = spellDBC.Read<int>(i, 40);
							WorldServiceLocator._WS_Spells.SPELLs[id].Totem[1] = spellDBC.Read<int>(i, 41);
							WorldServiceLocator._WS_Spells.SPELLs[id].Reagents[0] = spellDBC.Read<int>(i, 42);
							WorldServiceLocator._WS_Spells.SPELLs[id].Reagents[1] = spellDBC.Read<int>(i, 43);
							WorldServiceLocator._WS_Spells.SPELLs[id].Reagents[2] = spellDBC.Read<int>(i, 44);
							WorldServiceLocator._WS_Spells.SPELLs[id].Reagents[3] = spellDBC.Read<int>(i, 45);
							WorldServiceLocator._WS_Spells.SPELLs[id].Reagents[4] = spellDBC.Read<int>(i, 46);
							WorldServiceLocator._WS_Spells.SPELLs[id].Reagents[5] = spellDBC.Read<int>(i, 47);
							WorldServiceLocator._WS_Spells.SPELLs[id].Reagents[6] = spellDBC.Read<int>(i, 48);
							WorldServiceLocator._WS_Spells.SPELLs[id].Reagents[7] = spellDBC.Read<int>(i, 49);
							WorldServiceLocator._WS_Spells.SPELLs[id].ReagentsCount[0] = spellDBC.Read<int>(i, 50);
							WorldServiceLocator._WS_Spells.SPELLs[id].ReagentsCount[1] = spellDBC.Read<int>(i, 51);
							WorldServiceLocator._WS_Spells.SPELLs[id].ReagentsCount[2] = spellDBC.Read<int>(i, 52);
							WorldServiceLocator._WS_Spells.SPELLs[id].ReagentsCount[3] = spellDBC.Read<int>(i, 53);
							WorldServiceLocator._WS_Spells.SPELLs[id].ReagentsCount[4] = spellDBC.Read<int>(i, 54);
							WorldServiceLocator._WS_Spells.SPELLs[id].ReagentsCount[5] = spellDBC.Read<int>(i, 55);
							WorldServiceLocator._WS_Spells.SPELLs[id].ReagentsCount[6] = spellDBC.Read<int>(i, 56);
							WorldServiceLocator._WS_Spells.SPELLs[id].ReagentsCount[7] = spellDBC.Read<int>(i, 57);
							WorldServiceLocator._WS_Spells.SPELLs[id].EquippedItemClass = spellDBC.Read<int>(i, 58);
							WorldServiceLocator._WS_Spells.SPELLs[id].EquippedItemSubClass = spellDBC.Read<int>(i, 59);
							WorldServiceLocator._WS_Spells.SPELLs[id].EquippedItemInventoryType = spellDBC.Read<int>(i, 60);

							int k = 0;
							do
							{
								if (spellDBC.Read<int>(i, 61 + k) != 0)
								{
									WS_Spells.SpellEffect[] spellEffects = WorldServiceLocator._WS_Spells.SPELLs[id].SpellEffects;
									int num2 = k;
									Dictionary<int, WS_Spells.SpellInfo> sPELLs;
									int key;
									WS_Spells.SpellInfo Spell = (sPELLs = WorldServiceLocator._WS_Spells.SPELLs)[key = id];
									WS_Spells.SpellEffect spellEffect = new WS_Spells.SpellEffect(ref Spell);
									sPELLs[key] = Spell;
									WS_Spells.SpellEffect spellEffect2 = spellEffect;

									spellEffect2.ID = (SpellEffects_Names)spellDBC.Read<int>(i, 61 + k);
									spellEffect2.valueDie = spellDBC.Read<int>(i, 64 + k);
									spellEffect2.diceBase = spellDBC.Read<int>(i, 67 + k);
									spellEffect2.dicePerLevel = spellDBC.Read<float>(i, 70 + k);
									spellEffect2.valuePerLevel = (int)spellDBC.Read<float>(i, 73 + k);
									spellEffect2.valueBase = spellDBC.Read<int>(i, 76 + k);
									spellEffect2.Mechanic = spellDBC.Read<int>(i, 79 + k);
									spellEffect2.implicitTargetA = spellDBC.Read<int>(i, 82 + k);
									spellEffect2.implicitTargetB = spellDBC.Read<int>(i, 85 + k);
									spellEffect2.RadiusIndex = spellDBC.Read<int>(i, 88 + k);
									spellEffect2.ApplyAuraIndex = spellDBC.Read<int>(i, 91 + k);
									spellEffect2.Amplitude = spellDBC.Read<int>(i, 94 + k);
									spellEffect2.MultipleValue = spellDBC.Read<int>(i, 97 + k);
									spellEffect2.ChainTarget = spellDBC.Read<int>(i, 100 + k);
									spellEffect2.ItemType = spellDBC.Read<int>(i, 103 + k);
									spellEffect2.MiscValue = spellDBC.Read<int>(i, 106 + k);
									spellEffect2.TriggerSpell = spellDBC.Read<int>(i, 109 + k);
									spellEffect2.valuePerComboPoint = spellDBC.Read<int>(i, 112 + k);
									spellEffects[num2] = spellEffect2;
								}
								else
								{
									WorldServiceLocator._WS_Spells.SPELLs[id].SpellEffects[k] = null;
								}
								k++;
							}
							while (k <= 2);
							WorldServiceLocator._WS_Spells.SPELLs[id].SpellVisual = spellDBC.Read<int>(i, 115);
							WorldServiceLocator._WS_Spells.SPELLs[id].SpellIconID = spellDBC.Read<int>(i, 117);
							WorldServiceLocator._WS_Spells.SPELLs[id].ActiveIconID = spellDBC.Read<int>(i, 118);
							WorldServiceLocator._WS_Spells.SPELLs[id].Name = spellDBC.Read<string>(i, 120);
							WorldServiceLocator._WS_Spells.SPELLs[id].Rank = spellDBC.Read<string>(i, 129);
							WorldServiceLocator._WS_Spells.SPELLs[id].manaCostPercent = spellDBC.Read<int>(i, 156);
							WorldServiceLocator._WS_Spells.SPELLs[id].StartRecoveryCategory = spellDBC.Read<int>(i, 157);
							WorldServiceLocator._WS_Spells.SPELLs[id].StartRecoveryTime = spellDBC.Read<int>(i, 158);
							WorldServiceLocator._WS_Spells.SPELLs[id].AffectedTargetLevel = spellDBC.Read<int>(i, 159);
							WorldServiceLocator._WS_Spells.SPELLs[id].SpellFamilyName = spellDBC.Read<int>(i, 160);
							WorldServiceLocator._WS_Spells.SPELLs[id].SpellFamilyFlags = spellDBC.Read<int>(i, 161);
							WorldServiceLocator._WS_Spells.SPELLs[id].MaxTargets = spellDBC.Read<int>(i, 163);
							WorldServiceLocator._WS_Spells.SPELLs[id].DamageType = spellDBC.Read<int>(i, 164);
							int j = 0;
							do
							{
								if (WorldServiceLocator._WS_Spells.SPELLs[id].SpellEffects[j] != null)
									WorldServiceLocator._WS_Spells.SPELLs[id].SpellEffects[j].DamageMultiplier = spellDBC.Read<float>(i, 167 + j);
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
					spellDBC.Dispose();
				}
				catch (DirectoryNotFoundException ex2)
				{
					ProjectData.SetProjectError(ex2);
					DirectoryNotFoundException e = ex2;
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
				IEnumerator enumerator = default(IEnumerator);
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
				DirectoryNotFoundException e = ex;
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Database : SpellChains missing.");
				Console.ForegroundColor = ConsoleColor.Gray;
				ProjectData.ClearProjectError();
			}
		}

		public void InitializeTaxiNodes()
		{
			checked
			{
				try
				{
					BufferedDbc tmpDBC = new BufferedDbc("dbc" + Conversions.ToString(Path.DirectorySeparatorChar) + "TaxiNodes.dbc");
					int num = tmpDBC.Rows - 1;
					for (int i = 0; i <= num; i++)
					{
						int taxiNode = tmpDBC.Read<int>(i, 0);
						int taxiMapID = tmpDBC.Read<int>(i, 1);
						float taxiPosX = tmpDBC.Read<float>(i, 2);
						float taxiPosY = tmpDBC.Read<float>(i, 3);
						float taxiPosZ = tmpDBC.Read<float>(i, 4);
						int taxiMountTypeHorde = tmpDBC.Read<int>(i, 14);
						int taxiMountTypeAlliance = tmpDBC.Read<int>(i, 15);

						if (WorldServiceLocator._ConfigurationProvider.GetConfiguration().Maps.Contains(taxiMapID.ToString()))
							WorldServiceLocator._WS_DBCDatabase.TaxiNodes.Add(taxiNode, new WS_DBCDatabase.TTaxiNode(taxiPosX, taxiPosY, taxiPosZ, taxiMapID, taxiMountTypeHorde, taxiMountTypeAlliance));
					}
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} TaxiNodes initialized.", tmpDBC.Rows - 1);
					tmpDBC.Dispose();
				}
				catch (DirectoryNotFoundException ex)
				{
					ProjectData.SetProjectError(ex);
					DirectoryNotFoundException e = ex;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("DBC File : TaxiNodes missing.");
					Console.ForegroundColor = ConsoleColor.Gray;
					ProjectData.ClearProjectError();
				}
			}
		}

		public void InitializeTaxiPaths()
		{
			checked
			{
				try
				{
					BufferedDbc tmpDBC = new BufferedDbc("dbc" + Conversions.ToString(Path.DirectorySeparatorChar) + "TaxiPath.dbc");
					int num = tmpDBC.Rows - 1;
					for (int i = 0; i <= num; i++)
					{
						int taxiNode = tmpDBC.Read<int>(i, 0);
						int taxiFrom = tmpDBC.Read<int>(i, 1);
						int taxiTo = tmpDBC.Read<int>(i, 2);
						int taxiPrice = tmpDBC.Read<int>(i, 3);
						WorldServiceLocator._WS_DBCDatabase.TaxiPaths.Add(taxiNode, new WS_DBCDatabase.TTaxiPath(taxiFrom, taxiTo, taxiPrice));
					}
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} TaxiPaths initialized.", tmpDBC.Rows - 1);
					tmpDBC.Dispose();
				}
				catch (DirectoryNotFoundException ex)
				{
					ProjectData.SetProjectError(ex);
					DirectoryNotFoundException e = ex;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("DBC File : TaxiPath missing.");
					Console.ForegroundColor = ConsoleColor.Gray;
					ProjectData.ClearProjectError();
				}
			}
		}

		public void InitializeTaxiPathNodes()
		{
			checked
			{
				try
				{
					BufferedDbc tmpDBC = new BufferedDbc("dbc" + Conversions.ToString(Path.DirectorySeparatorChar) + "TaxiPathNode.dbc");
					int num = tmpDBC.Rows - 1;
					for (int i = 0; i <= num; i++)
					{
						int taxiPath = tmpDBC.Read<int>(i, 1);
						int taxiSeq = tmpDBC.Read<int>(i, 2);
						int taxiMapID = tmpDBC.Read<int>(i, 3);
						float taxiPosX = tmpDBC.Read<float>(i, 4);
						float taxiPosY = tmpDBC.Read<float>(i, 5);
						float taxiPosZ = tmpDBC.Read<float>(i, 6);
						int taxiAction = tmpDBC.Read<int>(i, 7);
						int taxiWait = tmpDBC.Read<int>(i, 8);
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
					tmpDBC.Dispose();
				}
				catch (DirectoryNotFoundException ex)
				{
					ProjectData.SetProjectError(ex);
					DirectoryNotFoundException e = ex;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("DBC File : TaxiPathNode missing.");
					Console.ForegroundColor = ConsoleColor.Gray;
					ProjectData.ClearProjectError();
				}
			}
		}

		public void InitializeSkillLines()
		{
			checked
			{
				try
				{
					BufferedDbc tmpDBC = new BufferedDbc("dbc" + Conversions.ToString(Path.DirectorySeparatorChar) + "SkillLine.dbc");
					int num = tmpDBC.Rows - 1;
					for (int i = 0; i <= num; i++)
					{
						int skillID = tmpDBC.Read<int>(i, 0);
						int skillLine = tmpDBC.Read<int>(i, 1);
						WorldServiceLocator._WS_DBCDatabase.SkillLines[skillID] = skillLine;
					}
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SkillLines initialized.", tmpDBC.Rows - 1);
					tmpDBC.Dispose();
				}
				catch (DirectoryNotFoundException ex)
				{
					ProjectData.SetProjectError(ex);
					DirectoryNotFoundException e = ex;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("DBC File : SkillLines missing.");
					Console.ForegroundColor = ConsoleColor.Gray;
					ProjectData.ClearProjectError();
				}
			}
		}

		public void InitializeSkillLineAbility()
		{
			checked
			{
				try
				{
					BufferedDbc tmpDBC = new BufferedDbc("dbc" + Conversions.ToString(Path.DirectorySeparatorChar) + "SkillLineAbility.dbc");
					int num = tmpDBC.Rows - 1;
					for (int i = 0; i <= num; i++)
					{
						WS_DBCDatabase.TSkillLineAbility tmpSkillLineAbility = new WS_DBCDatabase.TSkillLineAbility
						{
							ID = tmpDBC.Read<int>(i, 0),
							SkillID = tmpDBC.Read<int>(i, 1),
							SpellID = tmpDBC.Read<int>(i, 2),
							Unknown1 = tmpDBC.Read<int>(i, 3),
							Unknown2 = tmpDBC.Read<int>(i, 4),
							Unknown3 = tmpDBC.Read<int>(i, 5),
							Unknown4 = tmpDBC.Read<int>(i, 6),
							Required_Skill_Value = tmpDBC.Read<int>(i, 7),
							Forward_SpellID = tmpDBC.Read<int>(i, 8),
							Unknown5 = tmpDBC.Read<int>(i, 9),
							Max_Value = tmpDBC.Read<int>(i, 10),
							Min_Value = tmpDBC.Read<int>(i, 11)
						};
						WorldServiceLocator._WS_DBCDatabase.SkillLineAbility.Add(tmpSkillLineAbility.ID, tmpSkillLineAbility);
					}
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SkillLineAbilitys initialized.", tmpDBC.Rows - 1);
					tmpDBC.Dispose();
				}
				catch (DirectoryNotFoundException ex)
				{
					ProjectData.SetProjectError(ex);
					DirectoryNotFoundException e = ex;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("DBC File : SkillLineAbility missing.");
					Console.ForegroundColor = ConsoleColor.Gray;
					ProjectData.ClearProjectError();
				}
			}
		}

		public void InitializeLocks()
		{
			checked
			{
				try
				{
					BufferedDbc tmpDBC = new BufferedDbc("dbc" + Conversions.ToString(Path.DirectorySeparatorChar) + "Lock.dbc");
					byte[] keyType = new byte[5];
					int[] key = new int[5];
					int num = tmpDBC.Rows - 1;
					for (int i = 0; i <= num; i++)
					{
						int lockID = tmpDBC.Read<int>(i, 0);
						keyType[0] = tmpDBC.Read<byte>(i, 1);
						keyType[1] = tmpDBC.Read<byte>(i, 2);
						keyType[2] = tmpDBC.Read<byte>(i, 3);
						keyType[3] = tmpDBC.Read<byte>(i, 4);
						keyType[4] = tmpDBC.Read<byte>(i, 5);
						key[0] = tmpDBC.Read<int>(i, 9);
						key[1] = tmpDBC.Read<int>(i, 10);
						key[2] = tmpDBC.Read<int>(i, 11);
						key[3] = tmpDBC.Read<int>(i, 12);
						key[4] = tmpDBC.Read<int>(i, 13);
						int reqMining = tmpDBC.Read<int>(i, 17);
						int reqLockSkill = tmpDBC.Read<int>(i, 17);
						WorldServiceLocator._WS_Loot.Locks[lockID] = new WS_Loot.TLock(keyType, key, (short)reqMining, (short)reqLockSkill);
					}
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} Locks initialized.", tmpDBC.Rows - 1);
					tmpDBC.Dispose();
				}
				catch (DirectoryNotFoundException ex)
				{
					ProjectData.SetProjectError(ex);
					DirectoryNotFoundException e = ex;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("DBC File : Locks missing.");
					Console.ForegroundColor = ConsoleColor.Gray;
					ProjectData.ClearProjectError();
				}
			}
		}

		public void InitializeAreaTable()
		{
			checked
			{
				try
				{
					BufferedDbc tmpDBC = new BufferedDbc("dbc" + Conversions.ToString(Path.DirectorySeparatorChar) + "AreaTable.dbc");
					int num = tmpDBC.Rows - 1;
					for (int i = 0; i <= num; i++)
					{
						int areaID = tmpDBC.Read<int>(i, 0);
						int areaMapID = tmpDBC.Read<int>(i, 1);
						int areaZone = tmpDBC.Read<int>(i, 2);
						int areaExploreFlag = tmpDBC.Read<int>(i, 3);
						int areaZoneType = tmpDBC.Read<int>(i, 4);
						int areaLevel = tmpDBC.Read<int>(i, 10);

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
					tmpDBC.Dispose();
				}
				catch (DirectoryNotFoundException ex)
				{
					ProjectData.SetProjectError(ex);
					DirectoryNotFoundException e = ex;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("DBC File : AreaTable missing.");
					Console.ForegroundColor = ConsoleColor.Gray;
					ProjectData.ClearProjectError();
				}
			}
		}

		public void InitializeEmotes()
		{
			checked
			{
				try
				{
					BufferedDbc tmpDBC = new BufferedDbc("dbc" + Conversions.ToString(Path.DirectorySeparatorChar) + "Emotes.dbc");
					int num = tmpDBC.Rows - 1;
					for (int i = 0; i <= num; i++)
					{
						int emoteID = tmpDBC.Read<int>(i, 0);
						int emoteState = tmpDBC.Read<int>(i, 4);

						if (emoteID != 0)
							WorldServiceLocator._WS_DBCDatabase.EmotesState[emoteID] = emoteState;
					}
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} Emotes initialized.", tmpDBC.Rows - 1);
					tmpDBC.Dispose();
				}
				catch (DirectoryNotFoundException ex)
				{
					ProjectData.SetProjectError(ex);
					DirectoryNotFoundException e = ex;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("DBC File : Emotes missing.");
					Console.ForegroundColor = ConsoleColor.Gray;
					ProjectData.ClearProjectError();
				}
			}
		}

		public void InitializeEmotesText()
		{
			checked
			{
				try
				{
					BufferedDbc tmpDBC = new BufferedDbc("dbc" + Conversions.ToString(Path.DirectorySeparatorChar) + "EmotesText.dbc");
					int num = tmpDBC.Rows - 1;
					for (int i = 0; i <= num; i++)
					{
						int textEmoteID = tmpDBC.Read<int>(i, 0);
						int emoteID = tmpDBC.Read<int>(i, 2);

						if (emoteID != 0)
							WorldServiceLocator._WS_DBCDatabase.EmotesText[textEmoteID] = emoteID;
					}
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} EmotesText initialized.", tmpDBC.Rows - 1);
					tmpDBC.Dispose();
				}
				catch (DirectoryNotFoundException ex)
				{
					ProjectData.SetProjectError(ex);
					DirectoryNotFoundException e = ex;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("DBC File : EmotesText missing.");
					Console.ForegroundColor = ConsoleColor.Gray;
					ProjectData.ClearProjectError();
				}
			}
		}

		public void InitializeFactions()
		{
			checked
			{
				try
				{
					BufferedDbc tmpDBC = new BufferedDbc("dbc" + Conversions.ToString(Path.DirectorySeparatorChar) + "Faction.dbc");
					int[] flags = new int[4];
					int[] reputationStats = new int[4];
					int[] reputationFlags = new int[4];
					int num = tmpDBC.Rows - 1;
					for (int i = 0; i <= num; i++)
					{
						int factionID = tmpDBC.Read<int>(i, 0);
						int factionFlag = tmpDBC.Read<int>(i, 1);
						flags[0] = tmpDBC.Read<int>(i, 2);
						flags[1] = tmpDBC.Read<int>(i, 3);
						flags[2] = tmpDBC.Read<int>(i, 4);
						flags[3] = tmpDBC.Read<int>(i, 5);
						reputationStats[0] = tmpDBC.Read<int>(i, 10);
						reputationStats[1] = tmpDBC.Read<int>(i, 11);
						reputationStats[2] = tmpDBC.Read<int>(i, 12);
						reputationStats[3] = tmpDBC.Read<int>(i, 13);
						reputationFlags[0] = tmpDBC.Read<int>(i, 14);
						reputationFlags[1] = tmpDBC.Read<int>(i, 15);
						reputationFlags[2] = tmpDBC.Read<int>(i, 16);
						reputationFlags[3] = tmpDBC.Read<int>(i, 17);
						WorldServiceLocator._WS_DBCDatabase.FactionInfo[factionID] = new WS_DBCDatabase.TFaction((short)factionID, (short)factionFlag, flags[0], flags[1], flags[2], flags[3], reputationStats[0], reputationStats[1], reputationStats[2], reputationStats[3], reputationFlags[0], reputationFlags[1], reputationFlags[2], reputationFlags[3]);
					}
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} Factions initialized.", tmpDBC.Rows - 1);
					tmpDBC.Dispose();
				}
				catch (DirectoryNotFoundException ex)
				{
					ProjectData.SetProjectError(ex);
					DirectoryNotFoundException e = ex;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("DBC File : Factions missing.");
					Console.ForegroundColor = ConsoleColor.Gray;
					ProjectData.ClearProjectError();
				}
			}
		}

		public void InitializeFactionTemplates()
		{
			checked
			{
				try
				{
					BufferedDbc tmpDBC = new BufferedDbc("dbc" + Conversions.ToString(Path.DirectorySeparatorChar) + "FactionTemplate.dbc");
					int num = tmpDBC.Rows - 1;
					for (int i = 0; i <= num; i++)
					{
						int templateID = tmpDBC.Read<int>(i, 0);
						WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo.Add(templateID, new WS_DBCDatabase.TFactionTemplate());
						WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].FactionID = tmpDBC.Read<int>(i, 1);
						WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].ourMask = tmpDBC.Read<uint>(i, 3);
						WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].friendMask = tmpDBC.Read<uint>(i, 4);
						WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].enemyMask = tmpDBC.Read<uint>(i, 5);
						WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].enemyFaction1 = tmpDBC.Read<int>(i, 6);
						WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].enemyFaction2 = tmpDBC.Read<int>(i, 7);
						WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].enemyFaction3 = tmpDBC.Read<int>(i, 8);
						WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].enemyFaction4 = tmpDBC.Read<int>(i, 9);
						WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].friendFaction1 = tmpDBC.Read<int>(i, 10);
						WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].friendFaction2 = tmpDBC.Read<int>(i, 11);
						WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].friendFaction3 = tmpDBC.Read<int>(i, 12);
						WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].friendFaction4 = tmpDBC.Read<int>(i, 13);
					}
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} FactionTemplates initialized.", tmpDBC.Rows - 1);
					tmpDBC.Dispose();
				}
				catch (DirectoryNotFoundException ex)
				{
					ProjectData.SetProjectError(ex);
					DirectoryNotFoundException e = ex;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("DBC File : FactionsTemplates missing.");
					Console.ForegroundColor = ConsoleColor.Gray;
					ProjectData.ClearProjectError();
				}
			}
		}

		public void InitializeCharRaces()
		{
			checked
			{
				try
				{
					BufferedDbc tmpDBC = new BufferedDbc("dbc" + Conversions.ToString(Path.DirectorySeparatorChar) + "ChrRaces.dbc");
					int num = tmpDBC.Rows - 1;
					for (int i = 0; i <= num; i++)
					{
						int raceID = tmpDBC.Read<int>(i, 0);
						int factionID = tmpDBC.Read<int>(i, 2);
						int modelM = tmpDBC.Read<int>(i, 4);
						int modelF = tmpDBC.Read<int>(i, 5);
						int teamID = tmpDBC.Read<int>(i, 8);
						uint taxiMask = tmpDBC.Read<uint>(i, 14);
						int cinematicID = tmpDBC.Read<int>(i, 16);
						string name = tmpDBC.Read<string>(i, 17);
						WorldServiceLocator._WS_DBCDatabase.CharRaces[(byte)raceID] = new WS_DBCDatabase.TCharRace((short)factionID, modelM, modelF, (byte)teamID, taxiMask, cinematicID, name);
					}
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} CharRaces initialized.", tmpDBC.Rows - 1);
					tmpDBC.Dispose();
				}
				catch (DirectoryNotFoundException ex)
				{
					ProjectData.SetProjectError(ex);
					DirectoryNotFoundException e = ex;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("DBC File : CharRaces missing.");
					Console.ForegroundColor = ConsoleColor.Gray;
					ProjectData.ClearProjectError();
				}
			}
		}

		public void InitializeCharClasses()
		{
			checked
			{
				try
				{
					BufferedDbc tmpDBC = new BufferedDbc("dbc" + Conversions.ToString(Path.DirectorySeparatorChar) + "ChrClasses.dbc");
					int num = tmpDBC.Rows - 1;
					for (int i = 0; i <= num; i++)
					{
						int classID = tmpDBC.Read<int>(i, 0);
						int cinematicID = tmpDBC.Read<int>(i, 5);
						WorldServiceLocator._WS_DBCDatabase.CharClasses[(byte)classID] = new WS_DBCDatabase.TCharClass(cinematicID);
					}
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} CharClasses initialized.", tmpDBC.Rows - 1);
					tmpDBC.Dispose();
				}
				catch (DirectoryNotFoundException ex)
				{
					ProjectData.SetProjectError(ex);
					DirectoryNotFoundException e = ex;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("DBC File : CharRaces missing.");
					Console.ForegroundColor = ConsoleColor.Gray;
					ProjectData.ClearProjectError();
				}
			}
		}

		public void InitializeDurabilityCosts()
		{
			checked
			{
				try
				{
					BufferedDbc tmpDBC = new BufferedDbc("dbc" + Conversions.ToString(Path.DirectorySeparatorChar) + "DurabilityCosts.dbc");
					int num = tmpDBC.Rows - 1;
					for (int i = 0; i <= num; i++)
					{
						int itemBroken = tmpDBC.Read<int>(i, 0);
						int num2 = tmpDBC.Columns - 1;
						for (int itemType = 1; itemType <= num2; itemType++)
						{
							int itemPrice = tmpDBC.Read<int>(i, itemType);
							WorldServiceLocator._WS_DBCDatabase.DurabilityCosts[itemBroken, itemType - 1] = (short)itemPrice;
						}
					}
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} DurabilityCosts initialized.", tmpDBC.Rows - 1);
					tmpDBC.Dispose();
				}
				catch (DirectoryNotFoundException ex)
				{
					ProjectData.SetProjectError(ex);
					DirectoryNotFoundException e = ex;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("DBC File : DurabilityCosts missing.");
					Console.ForegroundColor = ConsoleColor.Gray;
					ProjectData.ClearProjectError();
				}
			}
		}

		public void LoadTalentDbc()
		{
			checked
			{
				try
				{
					BufferedDbc dbc = new BufferedDbc("dbc" + Conversions.ToString(Path.DirectorySeparatorChar) + "Talent.dbc");
					int num = dbc.Rows - 1;
					for (int i = 0; i <= num; i++)
					{
						WS_DBCDatabase.TalentInfo tmpInfo = new WS_DBCDatabase.TalentInfo
						{
							TalentID = dbc.Read<int>(i, 0),
							TalentTab = dbc.Read<int>(i, 1),
							Row = dbc.Read<int>(i, 2),
							Col = dbc.Read<int>(i, 3)
						};
						tmpInfo.RankID[0] = dbc.Read<int>(i, 4);
						tmpInfo.RankID[1] = dbc.Read<int>(i, 5);
						tmpInfo.RankID[2] = dbc.Read<int>(i, 6);
						tmpInfo.RankID[3] = dbc.Read<int>(i, 7);
						tmpInfo.RankID[4] = dbc.Read<int>(i, 8);
						tmpInfo.RequiredTalent[0] = dbc.Read<int>(i, 13);
						tmpInfo.RequiredPoints[0] = dbc.Read<int>(i, 16);
						WorldServiceLocator._WS_DBCDatabase.Talents.Add(tmpInfo.TalentID, tmpInfo);
					}
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} Talents initialized.", dbc.Rows - 1);
					dbc.Dispose();
				}
				catch (DirectoryNotFoundException ex)
				{
					ProjectData.SetProjectError(ex);
					DirectoryNotFoundException e = ex;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("DBC File : Talents missing.");
					Console.ForegroundColor = ConsoleColor.Gray;
					ProjectData.ClearProjectError();
				}
			}
		}

		public void LoadTalentTabDbc()
		{
			checked
			{
				try
				{
					BufferedDbc dbc = new BufferedDbc("dbc" + Conversions.ToString(Path.DirectorySeparatorChar) + "TalentTab.dbc");
					int num = dbc.Rows - 1;
					for (int i = 0; i <= num; i++)
					{
						int talentTab = dbc.Read<int>(i, 0);
						int talentMask = dbc.Read<int>(i, 12);
						WorldServiceLocator._WS_DBCDatabase.TalentsTab.Add(talentTab, talentMask);
					}
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} Talent tabs initialized.", dbc.Rows - 1);
					dbc.Dispose();
				}
				catch (DirectoryNotFoundException ex)
				{
					ProjectData.SetProjectError(ex);
					DirectoryNotFoundException e = ex;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("DBC File : TalentTab missing.");
					Console.ForegroundColor = ConsoleColor.Gray;
					ProjectData.ClearProjectError();
				}
			}
		}

		public void LoadAuctionHouseDbc()
		{
			checked
			{
				try
				{
					BufferedDbc dbc = new BufferedDbc("dbc" + Conversions.ToString(Path.DirectorySeparatorChar) + "AuctionHouse.dbc");
					int num = dbc.Rows - 1;
					for (int i = 0; i <= num; i++)
					{
						int ahId = dbc.Read<int>(i, 0);
						int fee = dbc.Read<int>(i, 2);
						int tax = dbc.Read<int>(i, 3);
						WorldServiceLocator._WS_Auction.AuctionID = ahId;
						WorldServiceLocator._WS_Auction.AuctionFee = fee;
						WorldServiceLocator._WS_Auction.AuctionTax = tax;
					}
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} AuctionHouses initialized.", dbc.Rows - 1);
					dbc.Dispose();
				}
				catch (DirectoryNotFoundException ex)
				{
					ProjectData.SetProjectError(ex);
					DirectoryNotFoundException e = ex;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("DBC File : AuctionHouse missing.");
					Console.ForegroundColor = ConsoleColor.Gray;
					ProjectData.ClearProjectError();
				}
			}
		}

		public void LoadSpellItemEnchantments()
		{
			checked
			{
				try
				{
					BufferedDbc dbc = new BufferedDbc("dbc" + Conversions.ToString(Path.DirectorySeparatorChar) + "SpellItemEnchantment.dbc");
					int[] type = new int[3];
					int[] amount = new int[3];
					int[] spellID = new int[3];
					int num = dbc.Rows - 1;
					for (int i = 0; i <= num; i++)
					{
						int id = dbc.Read<int>(i, 0);
						type[0] = dbc.Read<int>(i, 1);
						type[1] = dbc.Read<int>(i, 2);
						amount[0] = dbc.Read<int>(i, 4);
						amount[1] = dbc.Read<int>(i, 7);
						spellID[0] = dbc.Read<int>(i, 10);
						spellID[1] = dbc.Read<int>(i, 11);
						int auraID = dbc.Read<int>(i, 22);
						int slot = dbc.Read<int>(i, 23);
						WorldServiceLocator._WS_DBCDatabase.SpellItemEnchantments.Add(id, new WS_DBCDatabase.TSpellItemEnchantment(type, amount, spellID, auraID, slot));
					}
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} SpellItemEnchantments initialized.", dbc.Rows - 1);
					dbc.Dispose();
				}
				catch (DirectoryNotFoundException ex)
				{
					ProjectData.SetProjectError(ex);
					DirectoryNotFoundException e = ex;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("DBC File : SpellItemEnchantments missing.");
					Console.ForegroundColor = ConsoleColor.Gray;
					ProjectData.ClearProjectError();
				}
			}
		}

		public void LoadItemSet()
		{
			checked
			{
				try
				{
					BufferedDbc dbc = new BufferedDbc("dbc" + Conversions.ToString(Path.DirectorySeparatorChar) + "ItemSet.dbc");
					int[] itemID = new int[8];
					int[] spellID = new int[8];
					int[] itemCount = new int[8];
					int num = dbc.Rows - 1;
					int requiredSkillID = default(int);
					int requiredSkillValue = default(int);
					for (int i = 0; i <= num; i++)
					{
						int id = dbc.Read<int>(i, 0);
						string name = dbc.Read<string>(i, 1);
						itemID[0] = dbc.Read<int>(i, 10);
						itemID[1] = dbc.Read<int>(i, 11);
						itemID[2] = dbc.Read<int>(i, 12);
						itemID[3] = dbc.Read<int>(i, 13);
						itemID[4] = dbc.Read<int>(i, 14);
						itemID[5] = dbc.Read<int>(i, 15);
						itemID[6] = dbc.Read<int>(i, 16);
						itemID[7] = dbc.Read<int>(i, 17);
						WorldServiceLocator._WS_DBCDatabase.ItemSet.Add(id, new WS_DBCDatabase.TItemSet(name, itemID, spellID, itemCount, requiredSkillID, requiredSkillValue));
					}
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} ItemSets initialized.", dbc.Rows - 1);
					dbc.Dispose();
				}
				catch (DirectoryNotFoundException ex)
				{
					ProjectData.SetProjectError(ex);
					DirectoryNotFoundException e = ex;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("DBC File : ItemSet missing.");
					Console.ForegroundColor = ConsoleColor.Gray;
					ProjectData.ClearProjectError();
				}
			}
		}

		public void LoadItemDisplayInfoDbc()
		{
			checked
			{
				try
				{
					BufferedDbc dbc = new BufferedDbc("dbc" + Conversions.ToString(Path.DirectorySeparatorChar) + "ItemDisplayInfo.dbc");
					int num = dbc.Rows - 1;
					for (int i = 0; i <= num; i++)
					{
						WS_DBCDatabase.TItemDisplayInfo tmpItemDisplayInfo = new WS_DBCDatabase.TItemDisplayInfo
						{
							ID = dbc.Read<int>(i, 0),
							RandomPropertyChance = dbc.Read<int>(i, 11),
							Unknown = dbc.Read<int>(i, 22)
						};
						WorldServiceLocator._WS_DBCDatabase.ItemDisplayInfo.Add(tmpItemDisplayInfo.ID, tmpItemDisplayInfo);
					}
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} ItemDisplayInfos initialized.", dbc.Rows - 1);
					dbc.Dispose();
				}
				catch (DirectoryNotFoundException ex)
				{
					ProjectData.SetProjectError(ex);
					DirectoryNotFoundException e = ex;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("DBC File : ItemDisplayInfo missing.");
					Console.ForegroundColor = ConsoleColor.Gray;
					ProjectData.ClearProjectError();
				}
			}
		}

		public void LoadItemRandomPropertiesDbc()
		{
			checked
			{
				try
				{
					BufferedDbc dbc = new BufferedDbc("dbc" + Conversions.ToString(Path.DirectorySeparatorChar) + "ItemRandomProperties.dbc");
					int num = dbc.Rows - 1;
					for (int i = 0; i <= num; i++)
					{
						WS_DBCDatabase.TItemRandomPropertiesInfo tmpInfo = new WS_DBCDatabase.TItemRandomPropertiesInfo
						{
							ID = dbc.Read<int>(i, 0)
						};
						tmpInfo.Enchant_ID[0] = dbc.Read<int>(i, 2);
						tmpInfo.Enchant_ID[1] = dbc.Read<int>(i, 3);
						tmpInfo.Enchant_ID[2] = dbc.Read<int>(i, 4);
						WorldServiceLocator._WS_DBCDatabase.ItemRandomPropertiesInfo.Add(tmpInfo.ID, tmpInfo);
					}
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} ItemRandomProperties initialized.", dbc.Rows - 1);
					dbc.Dispose();
				}
				catch (DirectoryNotFoundException ex)
				{
					ProjectData.SetProjectError(ex);
					DirectoryNotFoundException e = ex;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("DBC File : ItemRandomProperties missing.");
					Console.ForegroundColor = ConsoleColor.Gray;
					ProjectData.ClearProjectError();
				}
			}
		}

		public void LoadCreatureGossip()
		{
			try
			{
				DataTable gossipQuery = new DataTable();
				WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT * FROM npc_gossip;", ref gossipQuery);
				IEnumerator enumerator = default(IEnumerator);
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
				DirectoryNotFoundException e = ex;
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Database : npc_gossip missing.");
				Console.ForegroundColor = ConsoleColor.Gray;
				ProjectData.ClearProjectError();
			}
		}

		public void LoadCreatureFamilyDbc()
		{
			checked
			{
				try
				{
					BufferedDbc dbc = new BufferedDbc("dbc" + Conversions.ToString(Path.DirectorySeparatorChar) + "CreatureFamily.dbc");
					int num = dbc.Rows - 1;
					for (int i = 0; i <= num; i++)
					{
						WS_DBCDatabase.CreatureFamilyInfo tmpInfo = new WS_DBCDatabase.CreatureFamilyInfo
						{
							ID = dbc.Read<int>(i, 0),
							Unknown1 = dbc.Read<int>(i, 5),
							Unknown2 = dbc.Read<int>(i, 6),
							PetFoodID = dbc.Read<int>(i, 7),
							Name = dbc.Read<string>(i, 12)
						};
						WorldServiceLocator._WS_DBCDatabase.CreaturesFamily.Add(tmpInfo.ID, tmpInfo);
					}
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} CreatureFamilys initialized.", dbc.Rows - 1);
					dbc.Dispose();
				}
				catch (DirectoryNotFoundException ex)
				{
					ProjectData.SetProjectError(ex);
					DirectoryNotFoundException e = ex;
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
				IEnumerator enumerator = default(IEnumerator);
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
				DirectoryNotFoundException e = ex;
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
				IEnumerator enumerator = default(IEnumerator);
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
				DataException e = ex2;
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
				IEnumerator enumerator = default(IEnumerator);
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
				DirectoryNotFoundException e = ex;
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
			IEnumerator enumerator = default(IEnumerator);
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
			IEnumerator enumerator2 = default(IEnumerator);
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
				IEnumerator enumerator3 = default(IEnumerator);
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
				IEnumerator enumerator4 = default(IEnumerator);
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
				IEnumerator enumerator = default(IEnumerator);
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
				DirectoryNotFoundException e = ex;
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Database : TransportQuery missing.");
				Console.ForegroundColor = ConsoleColor.Gray;
				ProjectData.ClearProjectError();
			}
		}
	}
}
