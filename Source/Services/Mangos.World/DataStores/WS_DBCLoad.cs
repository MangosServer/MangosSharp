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
						int radiusID = Conversions.ToInteger(tmpDBC[i, 0, DBCValueType.DBC_INTEGER]);
						float radiusValue = Conversions.ToSingle(tmpDBC[i, 1, DBCValueType.DBC_FLOAT]);
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
						int spellCastID = Conversions.ToInteger(tmpDBC[i, 0, DBCValueType.DBC_INTEGER]);
						int spellCastTimeS = Conversions.ToInteger(tmpDBC[i, 1, DBCValueType.DBC_INTEGER]);
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
						int spellRangeIndex = Conversions.ToInteger(tmpDBC[i, 0, DBCValueType.DBC_INTEGER]);
						float spellRangeMax = Conversions.ToSingle(tmpDBC[i, 2, DBCValueType.DBC_FLOAT]);
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
						int id = Conversions.ToInteger(tmpDBC[i, 0, DBCValueType.DBC_INTEGER]);
						int flags1 = Conversions.ToInteger(tmpDBC[i, 11, DBCValueType.DBC_INTEGER]);
						int creatureType = Conversions.ToInteger(tmpDBC[i, 12, DBCValueType.DBC_INTEGER]);
						int attackSpeed = Conversions.ToInteger(tmpDBC[i, 13, DBCValueType.DBC_INTEGER]);
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
						int spellFocusIndex = Conversions.ToInteger(tmpDBC[i, 0, DBCValueType.DBC_INTEGER]);
						string spellFocusObjectName = Conversions.ToString(tmpDBC[i, 1, DBCValueType.DBC_STRING]);
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
						int spellDurationIndex = Conversions.ToInteger(tmpDBC[i, 0, DBCValueType.DBC_INTEGER]);
						int spellDurationValue = Conversions.ToInteger(tmpDBC[i, 1, DBCValueType.DBC_INTEGER]);
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
							int id = Conversions.ToInteger(spellDBC[(int)i, 0, DBCValueType.DBC_INTEGER]);
							WorldServiceLocator._WS_Spells.SPELLs[id] = new WS_Spells.SpellInfo
							{
								ID = id,
								School = Conversions.ToInteger(spellDBC[(int)i, 1, DBCValueType.DBC_INTEGER]),
								Category = Conversions.ToInteger(spellDBC[(int)i, 2, DBCValueType.DBC_INTEGER]),
								DispellType = Conversions.ToInteger(spellDBC[(int)i, 4, DBCValueType.DBC_INTEGER]),
								Mechanic = Conversions.ToInteger(spellDBC[(int)i, 5, DBCValueType.DBC_INTEGER]),
								Attributes = Conversions.ToInteger(spellDBC[(int)i, 6, DBCValueType.DBC_INTEGER]),
								AttributesEx = Conversions.ToInteger(spellDBC[(int)i, 7, DBCValueType.DBC_INTEGER]),
								AttributesEx2 = Conversions.ToInteger(spellDBC[(int)i, 8, DBCValueType.DBC_INTEGER]),
								RequredCasterStance = Conversions.ToInteger(spellDBC[(int)i, 11, DBCValueType.DBC_INTEGER]),
								ShapeshiftExclude = Conversions.ToInteger(spellDBC[(int)i, 12, DBCValueType.DBC_INTEGER]),
								Target = Conversions.ToInteger(spellDBC[(int)i, 13, DBCValueType.DBC_INTEGER]),
								TargetCreatureType = Conversions.ToInteger(spellDBC[(int)i, 14, DBCValueType.DBC_INTEGER]),
								FocusObjectIndex = Conversions.ToInteger(spellDBC[(int)i, 15, DBCValueType.DBC_INTEGER]),
								CasterAuraState = Conversions.ToInteger(spellDBC[(int)i, 16, DBCValueType.DBC_INTEGER]),
								TargetAuraState = Conversions.ToInteger(spellDBC[(int)i, 17, DBCValueType.DBC_INTEGER]),
								SpellCastTimeIndex = Conversions.ToInteger(spellDBC[(int)i, 18, DBCValueType.DBC_INTEGER]),
								SpellCooldown = Conversions.ToInteger(spellDBC[(int)i, 19, DBCValueType.DBC_INTEGER]),
								CategoryCooldown = Conversions.ToInteger(spellDBC[(int)i, 20, DBCValueType.DBC_INTEGER]),
								interruptFlags = Conversions.ToInteger(spellDBC[(int)i, 21, DBCValueType.DBC_INTEGER]),
								auraInterruptFlags = Conversions.ToInteger(spellDBC[(int)i, 22, DBCValueType.DBC_INTEGER]),
								channelInterruptFlags = Conversions.ToInteger(spellDBC[(int)i, 23, DBCValueType.DBC_INTEGER]),
								procFlags = Conversions.ToInteger(spellDBC[(int)i, 24, DBCValueType.DBC_INTEGER]),
								procChance = Conversions.ToInteger(spellDBC[(int)i, 25, DBCValueType.DBC_INTEGER]),
								procCharges = Conversions.ToInteger(spellDBC[(int)i, 26, DBCValueType.DBC_INTEGER]),
								maxLevel = Conversions.ToInteger(spellDBC[(int)i, 27, DBCValueType.DBC_INTEGER]),
								baseLevel = Conversions.ToInteger(spellDBC[(int)i, 28, DBCValueType.DBC_INTEGER]),
								spellLevel = Conversions.ToInteger(spellDBC[(int)i, 29, DBCValueType.DBC_INTEGER]),
								DurationIndex = Conversions.ToInteger(spellDBC[(int)i, 30, DBCValueType.DBC_INTEGER]),
								powerType = Conversions.ToInteger(spellDBC[(int)i, 31, DBCValueType.DBC_INTEGER]),
								manaCost = Conversions.ToInteger(spellDBC[(int)i, 32, DBCValueType.DBC_INTEGER]),
								manaCostPerlevel = Conversions.ToInteger(spellDBC[(int)i, 33, DBCValueType.DBC_INTEGER]),
								manaPerSecond = Conversions.ToInteger(spellDBC[(int)i, 34, DBCValueType.DBC_INTEGER]),
								manaPerSecondPerLevel = Conversions.ToInteger(spellDBC[(int)i, 35, DBCValueType.DBC_INTEGER]),
								rangeIndex = Conversions.ToInteger(spellDBC[(int)i, 36, DBCValueType.DBC_INTEGER]),
								Speed = Conversions.ToSingle(spellDBC[(int)i, 37, DBCValueType.DBC_FLOAT]),
								modalNextSpell = Conversions.ToInteger(spellDBC[(int)i, 38, DBCValueType.DBC_INTEGER]),
								maxStack = Conversions.ToInteger(spellDBC[(int)i, 39, DBCValueType.DBC_INTEGER])
							};
							WorldServiceLocator._WS_Spells.SPELLs[id].Totem[0] = Conversions.ToInteger(spellDBC[(int)i, 40, DBCValueType.DBC_INTEGER]);
							WorldServiceLocator._WS_Spells.SPELLs[id].Totem[1] = Conversions.ToInteger(spellDBC[(int)i, 41, DBCValueType.DBC_INTEGER]);
							WorldServiceLocator._WS_Spells.SPELLs[id].Reagents[0] = Conversions.ToInteger(spellDBC[(int)i, 42, DBCValueType.DBC_INTEGER]);
							WorldServiceLocator._WS_Spells.SPELLs[id].Reagents[1] = Conversions.ToInteger(spellDBC[(int)i, 43, DBCValueType.DBC_INTEGER]);
							WorldServiceLocator._WS_Spells.SPELLs[id].Reagents[2] = Conversions.ToInteger(spellDBC[(int)i, 44, DBCValueType.DBC_INTEGER]);
							WorldServiceLocator._WS_Spells.SPELLs[id].Reagents[3] = Conversions.ToInteger(spellDBC[(int)i, 45, DBCValueType.DBC_INTEGER]);
							WorldServiceLocator._WS_Spells.SPELLs[id].Reagents[4] = Conversions.ToInteger(spellDBC[(int)i, 46, DBCValueType.DBC_INTEGER]);
							WorldServiceLocator._WS_Spells.SPELLs[id].Reagents[5] = Conversions.ToInteger(spellDBC[(int)i, 47, DBCValueType.DBC_INTEGER]);
							WorldServiceLocator._WS_Spells.SPELLs[id].Reagents[6] = Conversions.ToInteger(spellDBC[(int)i, 48, DBCValueType.DBC_INTEGER]);
							WorldServiceLocator._WS_Spells.SPELLs[id].Reagents[7] = Conversions.ToInteger(spellDBC[(int)i, 49, DBCValueType.DBC_INTEGER]);
							WorldServiceLocator._WS_Spells.SPELLs[id].ReagentsCount[0] = Conversions.ToInteger(spellDBC[(int)i, 50, DBCValueType.DBC_INTEGER]);
							WorldServiceLocator._WS_Spells.SPELLs[id].ReagentsCount[1] = Conversions.ToInteger(spellDBC[(int)i, 51, DBCValueType.DBC_INTEGER]);
							WorldServiceLocator._WS_Spells.SPELLs[id].ReagentsCount[2] = Conversions.ToInteger(spellDBC[(int)i, 52, DBCValueType.DBC_INTEGER]);
							WorldServiceLocator._WS_Spells.SPELLs[id].ReagentsCount[3] = Conversions.ToInteger(spellDBC[(int)i, 53, DBCValueType.DBC_INTEGER]);
							WorldServiceLocator._WS_Spells.SPELLs[id].ReagentsCount[4] = Conversions.ToInteger(spellDBC[(int)i, 54, DBCValueType.DBC_INTEGER]);
							WorldServiceLocator._WS_Spells.SPELLs[id].ReagentsCount[5] = Conversions.ToInteger(spellDBC[(int)i, 55, DBCValueType.DBC_INTEGER]);
							WorldServiceLocator._WS_Spells.SPELLs[id].ReagentsCount[6] = Conversions.ToInteger(spellDBC[(int)i, 56, DBCValueType.DBC_INTEGER]);
							WorldServiceLocator._WS_Spells.SPELLs[id].ReagentsCount[7] = Conversions.ToInteger(spellDBC[(int)i, 57, DBCValueType.DBC_INTEGER]);
							WorldServiceLocator._WS_Spells.SPELLs[id].EquippedItemClass = Conversions.ToInteger(spellDBC[(int)i, 58, DBCValueType.DBC_INTEGER]);
							WorldServiceLocator._WS_Spells.SPELLs[id].EquippedItemSubClass = Conversions.ToInteger(spellDBC[(int)i, 59, DBCValueType.DBC_INTEGER]);
							WorldServiceLocator._WS_Spells.SPELLs[id].EquippedItemInventoryType = Conversions.ToInteger(spellDBC[(int)i, 60, DBCValueType.DBC_INTEGER]);
							int k = 0;
							do
							{
								if (Conversions.ToInteger(spellDBC[(int)i, 61 + k, DBCValueType.DBC_INTEGER]) != 0)
								{
									WS_Spells.SpellEffect[] spellEffects = WorldServiceLocator._WS_Spells.SPELLs[id].SpellEffects;
									int num2 = k;
									Dictionary<int, WS_Spells.SpellInfo> sPELLs;
									int key;
									WS_Spells.SpellInfo Spell = (sPELLs = WorldServiceLocator._WS_Spells.SPELLs)[key = id];
									WS_Spells.SpellEffect spellEffect = new WS_Spells.SpellEffect(ref Spell);
									sPELLs[key] = Spell;
									WS_Spells.SpellEffect spellEffect2 = spellEffect;
									unchecked
									{
										spellEffect2.ID = (SpellEffects_Names)Conversions.ToInteger(checked(spellDBC[(int)i, 61 + k, DBCValueType.DBC_INTEGER]));
									}
									spellEffect2.valueDie = Conversions.ToInteger(spellDBC[(int)i, 64 + k, DBCValueType.DBC_INTEGER]);
									spellEffect2.diceBase = Conversions.ToInteger(spellDBC[(int)i, 67 + k, DBCValueType.DBC_INTEGER]);
									spellEffect2.dicePerLevel = Conversions.ToSingle(spellDBC[(int)i, 70 + k, DBCValueType.DBC_FLOAT]);
									spellEffect2.valuePerLevel = Conversions.ToInteger(spellDBC[(int)i, 73 + k, DBCValueType.DBC_FLOAT]);
									spellEffect2.valueBase = Conversions.ToInteger(spellDBC[(int)i, 76 + k, DBCValueType.DBC_INTEGER]);
									spellEffect2.Mechanic = Conversions.ToInteger(spellDBC[(int)i, 79 + k, DBCValueType.DBC_INTEGER]);
									spellEffect2.implicitTargetA = Conversions.ToInteger(spellDBC[(int)i, 82 + k, DBCValueType.DBC_INTEGER]);
									spellEffect2.implicitTargetB = Conversions.ToInteger(spellDBC[(int)i, 85 + k, DBCValueType.DBC_INTEGER]);
									spellEffect2.RadiusIndex = Conversions.ToInteger(spellDBC[(int)i, 88 + k, DBCValueType.DBC_INTEGER]);
									spellEffect2.ApplyAuraIndex = Conversions.ToInteger(spellDBC[(int)i, 91 + k, DBCValueType.DBC_INTEGER]);
									spellEffect2.Amplitude = Conversions.ToInteger(spellDBC[(int)i, 94 + k, DBCValueType.DBC_INTEGER]);
									spellEffect2.MultipleValue = Conversions.ToInteger(spellDBC[(int)i, 97 + k, DBCValueType.DBC_INTEGER]);
									spellEffect2.ChainTarget = Conversions.ToInteger(spellDBC[(int)i, 100 + k, DBCValueType.DBC_INTEGER]);
									spellEffect2.ItemType = Conversions.ToInteger(spellDBC[(int)i, 103 + k, DBCValueType.DBC_INTEGER]);
									spellEffect2.MiscValue = Conversions.ToInteger(spellDBC[(int)i, 106 + k, DBCValueType.DBC_INTEGER]);
									spellEffect2.TriggerSpell = Conversions.ToInteger(spellDBC[(int)i, 109 + k, DBCValueType.DBC_INTEGER]);
									spellEffect2.valuePerComboPoint = Conversions.ToInteger(spellDBC[(int)i, 112 + k, DBCValueType.DBC_INTEGER]);
									spellEffects[num2] = spellEffect2;
								}
								else
								{
									WorldServiceLocator._WS_Spells.SPELLs[id].SpellEffects[k] = null;
								}
								k++;
							}
							while (k <= 2);
							WorldServiceLocator._WS_Spells.SPELLs[id].SpellVisual = Conversions.ToInteger(spellDBC[(int)i, 115, DBCValueType.DBC_INTEGER]);
							WorldServiceLocator._WS_Spells.SPELLs[id].SpellIconID = Conversions.ToInteger(spellDBC[(int)i, 117, DBCValueType.DBC_INTEGER]);
							WorldServiceLocator._WS_Spells.SPELLs[id].ActiveIconID = Conversions.ToInteger(spellDBC[(int)i, 118, DBCValueType.DBC_INTEGER]);
							WorldServiceLocator._WS_Spells.SPELLs[id].Name = Conversions.ToString(spellDBC[(int)i, 120, DBCValueType.DBC_STRING]);
							WorldServiceLocator._WS_Spells.SPELLs[id].Rank = Conversions.ToString(spellDBC[(int)i, 129, DBCValueType.DBC_STRING]);
							WorldServiceLocator._WS_Spells.SPELLs[id].manaCostPercent = Conversions.ToInteger(spellDBC[(int)i, 156, DBCValueType.DBC_INTEGER]);
							WorldServiceLocator._WS_Spells.SPELLs[id].StartRecoveryCategory = Conversions.ToInteger(spellDBC[(int)i, 157, DBCValueType.DBC_INTEGER]);
							WorldServiceLocator._WS_Spells.SPELLs[id].StartRecoveryTime = Conversions.ToInteger(spellDBC[(int)i, 158, DBCValueType.DBC_INTEGER]);
							WorldServiceLocator._WS_Spells.SPELLs[id].AffectedTargetLevel = Conversions.ToInteger(spellDBC[(int)i, 159, DBCValueType.DBC_INTEGER]);
							WorldServiceLocator._WS_Spells.SPELLs[id].SpellFamilyName = Conversions.ToInteger(spellDBC[(int)i, 160, DBCValueType.DBC_INTEGER]);
							WorldServiceLocator._WS_Spells.SPELLs[id].SpellFamilyFlags = Conversions.ToInteger(spellDBC[(int)i, 161, DBCValueType.DBC_INTEGER]);
							WorldServiceLocator._WS_Spells.SPELLs[id].MaxTargets = Conversions.ToInteger(spellDBC[(int)i, 163, DBCValueType.DBC_INTEGER]);
							WorldServiceLocator._WS_Spells.SPELLs[id].DamageType = Conversions.ToInteger(spellDBC[(int)i, 164, DBCValueType.DBC_INTEGER]);
							int j = 0;
							do
							{
								if (WorldServiceLocator._WS_Spells.SPELLs[id].SpellEffects[j] != null)
								{
									WorldServiceLocator._WS_Spells.SPELLs[id].SpellEffects[j].DamageMultiplier = Conversions.ToSingle(spellDBC[(int)i, 167 + j, DBCValueType.DBC_FLOAT]);
								}
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
						DataRow spellChain = (DataRow)enumerator.Current;
						WorldServiceLocator._WS_Spells.SpellChains.Add(Conversions.ToInteger(spellChain["spell_id"]), Conversions.ToInteger(spellChain["prev_spell"]));
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
						int taxiNode = Conversions.ToInteger(tmpDBC[i, 0, DBCValueType.DBC_INTEGER]);
						int taxiMapID = Conversions.ToInteger(tmpDBC[i, 1, DBCValueType.DBC_INTEGER]);
						float taxiPosX = Conversions.ToSingle(tmpDBC[i, 2, DBCValueType.DBC_FLOAT]);
						float taxiPosY = Conversions.ToSingle(tmpDBC[i, 3, DBCValueType.DBC_FLOAT]);
						float taxiPosZ = Conversions.ToSingle(tmpDBC[i, 4, DBCValueType.DBC_FLOAT]);
						int taxiMountTypeHorde = Conversions.ToInteger(tmpDBC[i, 14, DBCValueType.DBC_INTEGER]);
						int taxiMountTypeAlliance = Conversions.ToInteger(tmpDBC[i, 15, DBCValueType.DBC_INTEGER]);
						if (WorldServiceLocator._ConfigurationProvider.GetConfiguration().Maps.Contains(taxiMapID.ToString()))
						{
							WorldServiceLocator._WS_DBCDatabase.TaxiNodes.Add(taxiNode, new WS_DBCDatabase.TTaxiNode(taxiPosX, taxiPosY, taxiPosZ, taxiMapID, taxiMountTypeHorde, taxiMountTypeAlliance));
						}
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
						int taxiNode = Conversions.ToInteger(tmpDBC[i, 0, DBCValueType.DBC_INTEGER]);
						int taxiFrom = Conversions.ToInteger(tmpDBC[i, 1, DBCValueType.DBC_INTEGER]);
						int taxiTo = Conversions.ToInteger(tmpDBC[i, 2, DBCValueType.DBC_INTEGER]);
						int taxiPrice = Conversions.ToInteger(tmpDBC[i, 3, DBCValueType.DBC_INTEGER]);
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
						int taxiPath = Conversions.ToInteger(tmpDBC[i, 1, DBCValueType.DBC_INTEGER]);
						int taxiSeq = Conversions.ToInteger(tmpDBC[i, 2, DBCValueType.DBC_INTEGER]);
						int taxiMapID = Conversions.ToInteger(tmpDBC[i, 3, DBCValueType.DBC_INTEGER]);
						float taxiPosX = Conversions.ToSingle(tmpDBC[i, 4, DBCValueType.DBC_FLOAT]);
						float taxiPosY = Conversions.ToSingle(tmpDBC[i, 5, DBCValueType.DBC_FLOAT]);
						float taxiPosZ = Conversions.ToSingle(tmpDBC[i, 6, DBCValueType.DBC_FLOAT]);
						int taxiAction = Conversions.ToInteger(tmpDBC[i, 7, DBCValueType.DBC_INTEGER]);
						int taxiWait = Conversions.ToInteger(tmpDBC[i, 8, DBCValueType.DBC_INTEGER]);
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
						int skillID = Conversions.ToInteger(tmpDBC[i, 0, DBCValueType.DBC_INTEGER]);
						int skillLine = Conversions.ToInteger(tmpDBC[i, 1, DBCValueType.DBC_INTEGER]);
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
							ID = Conversions.ToInteger(tmpDBC[i, 0, DBCValueType.DBC_INTEGER]),
							SkillID = Conversions.ToInteger(tmpDBC[i, 1, DBCValueType.DBC_INTEGER]),
							SpellID = Conversions.ToInteger(tmpDBC[i, 2, DBCValueType.DBC_INTEGER]),
							Unknown1 = Conversions.ToInteger(tmpDBC[i, 3, DBCValueType.DBC_INTEGER]),
							Unknown2 = Conversions.ToInteger(tmpDBC[i, 4, DBCValueType.DBC_INTEGER]),
							Unknown3 = Conversions.ToInteger(tmpDBC[i, 5, DBCValueType.DBC_INTEGER]),
							Unknown4 = Conversions.ToInteger(tmpDBC[i, 6, DBCValueType.DBC_INTEGER]),
							Required_Skill_Value = Conversions.ToInteger(tmpDBC[i, 7, DBCValueType.DBC_INTEGER]),
							Forward_SpellID = Conversions.ToInteger(tmpDBC[i, 8, DBCValueType.DBC_INTEGER]),
							Unknown5 = Conversions.ToInteger(tmpDBC[i, 9, DBCValueType.DBC_INTEGER]),
							Max_Value = Conversions.ToInteger(tmpDBC[i, 10, DBCValueType.DBC_INTEGER]),
							Min_Value = Conversions.ToInteger(tmpDBC[i, 11, DBCValueType.DBC_INTEGER])
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
						int lockID = Conversions.ToInteger(tmpDBC[i, 0, DBCValueType.DBC_INTEGER]);
						keyType[0] = Conversions.ToByte(tmpDBC[i, 1, DBCValueType.DBC_INTEGER]);
						keyType[1] = Conversions.ToByte(tmpDBC[i, 2, DBCValueType.DBC_INTEGER]);
						keyType[2] = Conversions.ToByte(tmpDBC[i, 3, DBCValueType.DBC_INTEGER]);
						keyType[3] = Conversions.ToByte(tmpDBC[i, 4, DBCValueType.DBC_INTEGER]);
						keyType[4] = Conversions.ToByte(tmpDBC[i, 5, DBCValueType.DBC_INTEGER]);
						key[0] = Conversions.ToInteger(tmpDBC[i, 9, DBCValueType.DBC_INTEGER]);
						key[1] = Conversions.ToInteger(tmpDBC[i, 10, DBCValueType.DBC_INTEGER]);
						key[2] = Conversions.ToInteger(tmpDBC[i, 11, DBCValueType.DBC_INTEGER]);
						key[3] = Conversions.ToInteger(tmpDBC[i, 12, DBCValueType.DBC_INTEGER]);
						key[4] = Conversions.ToInteger(tmpDBC[i, 13, DBCValueType.DBC_INTEGER]);
						int reqMining = Conversions.ToInteger(tmpDBC[i, 17, DBCValueType.DBC_INTEGER]);
						int reqLockSkill = Conversions.ToInteger(tmpDBC[i, 17, DBCValueType.DBC_INTEGER]);
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
					BufferedDbc tmpDbc = new BufferedDbc("dbc" + Conversions.ToString(Path.DirectorySeparatorChar) + "AreaTable.dbc");
					int num = tmpDbc.Rows - 1;
					for (int i = 0; i <= num; i++)
					{
						int areaID = Conversions.ToInteger(tmpDbc[i, 0, DBCValueType.DBC_INTEGER]);
						int areaMapID = Conversions.ToInteger(tmpDbc[i, 1, DBCValueType.DBC_INTEGER]);
						int areaZone = Conversions.ToInteger(tmpDbc[i, 2, DBCValueType.DBC_INTEGER]);
						int areaExploreFlag = Conversions.ToInteger(tmpDbc[i, 3, DBCValueType.DBC_INTEGER]);
						int areaZoneType = Conversions.ToInteger(tmpDbc[i, 4, DBCValueType.DBC_INTEGER]);
						int areaLevel = Conversions.ToInteger(tmpDbc[i, 10, DBCValueType.DBC_INTEGER]);
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
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} Areas initialized.", tmpDbc.Rows - 1);
					tmpDbc.Dispose();
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
						int emoteID = Conversions.ToInteger(tmpDBC[i, 0, DBCValueType.DBC_INTEGER]);
						int emoteState = Conversions.ToInteger(tmpDBC[i, 4, DBCValueType.DBC_INTEGER]);
						if (emoteID != 0)
						{
							WorldServiceLocator._WS_DBCDatabase.EmotesState[emoteID] = emoteState;
						}
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
					BufferedDbc tmpDbc = new BufferedDbc("dbc" + Conversions.ToString(Path.DirectorySeparatorChar) + "EmotesText.dbc");
					int num = tmpDbc.Rows - 1;
					for (int i = 0; i <= num; i++)
					{
						int textEmoteID = Conversions.ToInteger(tmpDbc[i, 0, DBCValueType.DBC_INTEGER]);
						int emoteID = Conversions.ToInteger(tmpDbc[i, 2, DBCValueType.DBC_INTEGER]);
						if (emoteID != 0)
						{
							WorldServiceLocator._WS_DBCDatabase.EmotesText[textEmoteID] = emoteID;
						}
					}
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} EmotesText initialized.", tmpDbc.Rows - 1);
					tmpDbc.Dispose();
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
						int factionID = Conversions.ToInteger(tmpDBC[i, 0, DBCValueType.DBC_INTEGER]);
						int factionFlag = Conversions.ToInteger(tmpDBC[i, 1, DBCValueType.DBC_INTEGER]);
						flags[0] = Conversions.ToInteger(tmpDBC[i, 2, DBCValueType.DBC_INTEGER]);
						flags[1] = Conversions.ToInteger(tmpDBC[i, 3, DBCValueType.DBC_INTEGER]);
						flags[2] = Conversions.ToInteger(tmpDBC[i, 4, DBCValueType.DBC_INTEGER]);
						flags[3] = Conversions.ToInteger(tmpDBC[i, 5, DBCValueType.DBC_INTEGER]);
						reputationStats[0] = Conversions.ToInteger(tmpDBC[i, 10, DBCValueType.DBC_INTEGER]);
						reputationStats[1] = Conversions.ToInteger(tmpDBC[i, 11, DBCValueType.DBC_INTEGER]);
						reputationStats[2] = Conversions.ToInteger(tmpDBC[i, 12, DBCValueType.DBC_INTEGER]);
						reputationStats[3] = Conversions.ToInteger(tmpDBC[i, 13, DBCValueType.DBC_INTEGER]);
						reputationFlags[0] = Conversions.ToInteger(tmpDBC[i, 14, DBCValueType.DBC_INTEGER]);
						reputationFlags[1] = Conversions.ToInteger(tmpDBC[i, 15, DBCValueType.DBC_INTEGER]);
						reputationFlags[2] = Conversions.ToInteger(tmpDBC[i, 16, DBCValueType.DBC_INTEGER]);
						reputationFlags[3] = Conversions.ToInteger(tmpDBC[i, 17, DBCValueType.DBC_INTEGER]);
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
						int templateID = Conversions.ToInteger(tmpDBC[i, 0, DBCValueType.DBC_INTEGER]);
						WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo.Add(templateID, new WS_DBCDatabase.TFactionTemplate());
						WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].FactionID = Conversions.ToInteger(tmpDBC[i, 1, DBCValueType.DBC_INTEGER]);
						WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].ourMask = Conversions.ToUInteger(tmpDBC[i, 3, DBCValueType.DBC_INTEGER]);
						WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].friendMask = Conversions.ToUInteger(tmpDBC[i, 4, DBCValueType.DBC_INTEGER]);
						WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].enemyMask = Conversions.ToUInteger(tmpDBC[i, 5, DBCValueType.DBC_INTEGER]);
						WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].enemyFaction1 = Conversions.ToInteger(tmpDBC[i, 6, DBCValueType.DBC_INTEGER]);
						WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].enemyFaction2 = Conversions.ToInteger(tmpDBC[i, 7, DBCValueType.DBC_INTEGER]);
						WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].enemyFaction3 = Conversions.ToInteger(tmpDBC[i, 8, DBCValueType.DBC_INTEGER]);
						WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].enemyFaction4 = Conversions.ToInteger(tmpDBC[i, 9, DBCValueType.DBC_INTEGER]);
						WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].friendFaction1 = Conversions.ToInteger(tmpDBC[i, 10, DBCValueType.DBC_INTEGER]);
						WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].friendFaction2 = Conversions.ToInteger(tmpDBC[i, 11, DBCValueType.DBC_INTEGER]);
						WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].friendFaction3 = Conversions.ToInteger(tmpDBC[i, 12, DBCValueType.DBC_INTEGER]);
						WorldServiceLocator._WS_DBCDatabase.FactionTemplatesInfo[templateID].friendFaction4 = Conversions.ToInteger(tmpDBC[i, 13, DBCValueType.DBC_INTEGER]);
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
						int raceID = Conversions.ToInteger(tmpDBC[i, 0, DBCValueType.DBC_INTEGER]);
						int factionID = Conversions.ToInteger(tmpDBC[i, 2, DBCValueType.DBC_INTEGER]);
						int modelM = Conversions.ToInteger(tmpDBC[i, 4, DBCValueType.DBC_INTEGER]);
						int modelF = Conversions.ToInteger(tmpDBC[i, 5, DBCValueType.DBC_INTEGER]);
						int teamID = Conversions.ToInteger(tmpDBC[i, 8, DBCValueType.DBC_INTEGER]);
						uint taxiMask = Conversions.ToUInteger(tmpDBC[i, 14, DBCValueType.DBC_INTEGER]);
						int cinematicID = Conversions.ToInteger(tmpDBC[i, 16, DBCValueType.DBC_INTEGER]);
						string name = Conversions.ToString(tmpDBC[i, 17, DBCValueType.DBC_STRING]);
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
						int classID = Conversions.ToInteger(tmpDBC[i, 0, DBCValueType.DBC_INTEGER]);
						int cinematicID = Conversions.ToInteger(tmpDBC[i, 5, DBCValueType.DBC_INTEGER]);
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
						int itemBroken = Conversions.ToInteger(tmpDBC[i, 0, DBCValueType.DBC_INTEGER]);
						int num2 = tmpDBC.Columns - 1;
						for (int itemType = 1; itemType <= num2; itemType++)
						{
							int itemPrice = Conversions.ToInteger(tmpDBC[i, itemType, DBCValueType.DBC_INTEGER]);
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
							TalentID = Conversions.ToInteger(dbc[i, 0, DBCValueType.DBC_INTEGER]),
							TalentTab = Conversions.ToInteger(dbc[i, 1, DBCValueType.DBC_INTEGER]),
							Row = Conversions.ToInteger(dbc[i, 2, DBCValueType.DBC_INTEGER]),
							Col = Conversions.ToInteger(dbc[i, 3, DBCValueType.DBC_INTEGER])
						};
						tmpInfo.RankID[0] = Conversions.ToInteger(dbc[i, 4, DBCValueType.DBC_INTEGER]);
						tmpInfo.RankID[1] = Conversions.ToInteger(dbc[i, 5, DBCValueType.DBC_INTEGER]);
						tmpInfo.RankID[2] = Conversions.ToInteger(dbc[i, 6, DBCValueType.DBC_INTEGER]);
						tmpInfo.RankID[3] = Conversions.ToInteger(dbc[i, 7, DBCValueType.DBC_INTEGER]);
						tmpInfo.RankID[4] = Conversions.ToInteger(dbc[i, 8, DBCValueType.DBC_INTEGER]);
						tmpInfo.RequiredTalent[0] = Conversions.ToInteger(dbc[i, 13, DBCValueType.DBC_INTEGER]);
						tmpInfo.RequiredPoints[0] = Conversions.ToInteger(dbc[i, 16, DBCValueType.DBC_INTEGER]);
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
						int talentTab = Conversions.ToInteger(dbc[i, 0, DBCValueType.DBC_INTEGER]);
						int talentMask = Conversions.ToInteger(dbc[i, 12, DBCValueType.DBC_INTEGER]);
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
						int ahId = Conversions.ToInteger(dbc[i, 0, DBCValueType.DBC_INTEGER]);
						int fee = Conversions.ToInteger(dbc[i, 2, DBCValueType.DBC_INTEGER]);
						int tax = Conversions.ToInteger(dbc[i, 3, DBCValueType.DBC_INTEGER]);
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
						int id = Conversions.ToInteger(dbc[i, 0, DBCValueType.DBC_INTEGER]);
						type[0] = Conversions.ToInteger(dbc[i, 1, DBCValueType.DBC_INTEGER]);
						type[1] = Conversions.ToInteger(dbc[i, 2, DBCValueType.DBC_INTEGER]);
						amount[0] = Conversions.ToInteger(dbc[i, 4, DBCValueType.DBC_INTEGER]);
						amount[1] = Conversions.ToInteger(dbc[i, 7, DBCValueType.DBC_INTEGER]);
						spellID[0] = Conversions.ToInteger(dbc[i, 10, DBCValueType.DBC_INTEGER]);
						spellID[1] = Conversions.ToInteger(dbc[i, 11, DBCValueType.DBC_INTEGER]);
						int auraID = Conversions.ToInteger(dbc[i, 22, DBCValueType.DBC_INTEGER]);
						int slot = Conversions.ToInteger(dbc[i, 23, DBCValueType.DBC_INTEGER]);
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
						int id = Conversions.ToInteger(dbc[i, 0, DBCValueType.DBC_INTEGER]);
						string name = Conversions.ToString(dbc[i, 1, DBCValueType.DBC_STRING]);
						itemID[0] = Conversions.ToInteger(dbc[i, 10, DBCValueType.DBC_INTEGER]);
						itemID[1] = Conversions.ToInteger(dbc[i, 11, DBCValueType.DBC_INTEGER]);
						itemID[2] = Conversions.ToInteger(dbc[i, 12, DBCValueType.DBC_INTEGER]);
						itemID[3] = Conversions.ToInteger(dbc[i, 13, DBCValueType.DBC_INTEGER]);
						itemID[4] = Conversions.ToInteger(dbc[i, 14, DBCValueType.DBC_INTEGER]);
						itemID[5] = Conversions.ToInteger(dbc[i, 15, DBCValueType.DBC_INTEGER]);
						itemID[6] = Conversions.ToInteger(dbc[i, 16, DBCValueType.DBC_INTEGER]);
						itemID[7] = Conversions.ToInteger(dbc[i, 17, DBCValueType.DBC_INTEGER]);
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
							ID = Conversions.ToInteger(dbc[i, 0, DBCValueType.DBC_INTEGER]),
							RandomPropertyChance = Conversions.ToInteger(dbc[i, 11, DBCValueType.DBC_INTEGER]),
							Unknown = Conversions.ToInteger(dbc[i, 22, DBCValueType.DBC_INTEGER])
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
							ID = Conversions.ToInteger(dbc[i, 0, DBCValueType.DBC_INTEGER])
						};
						tmpInfo.Enchant_ID[0] = Conversions.ToInteger(dbc[i, 2, DBCValueType.DBC_INTEGER]);
						tmpInfo.Enchant_ID[1] = Conversions.ToInteger(dbc[i, 3, DBCValueType.DBC_INTEGER]);
						tmpInfo.Enchant_ID[2] = Conversions.ToInteger(dbc[i, 4, DBCValueType.DBC_INTEGER]);
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
						DataRow gossip = (DataRow)enumerator.Current;
						ulong guid = Conversions.ToULong(gossip["npc_guid"]);
						if (!WorldServiceLocator._WS_DBCDatabase.CreatureGossip.ContainsKey(guid))
						{
							WorldServiceLocator._WS_DBCDatabase.CreatureGossip.Add(guid, Conversions.ToInteger(gossip["textid"]));
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
							ID = Conversions.ToInteger(dbc[i, 0, DBCValueType.DBC_INTEGER]),
							Unknown1 = Conversions.ToInteger(dbc[i, 5, DBCValueType.DBC_INTEGER]),
							Unknown2 = Conversions.ToInteger(dbc[i, 6, DBCValueType.DBC_INTEGER]),
							PetFoodID = Conversions.ToInteger(dbc[i, 7, DBCValueType.DBC_INTEGER]),
							Name = Conversions.ToString(dbc[i, 12, DBCValueType.DBC_STRING])
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
						DataRow movement = (DataRow)enumerator.Current;
						int id = Conversions.ToInteger(movement["id"]);
						if (!WorldServiceLocator._WS_DBCDatabase.CreatureMovement.ContainsKey(id))
						{
							WorldServiceLocator._WS_DBCDatabase.CreatureMovement.Add(id, new Dictionary<int, WS_DBCDatabase.CreatureMovePoint>());
						}
						WorldServiceLocator._WS_DBCDatabase.CreatureMovement[id].Add(Conversions.ToInteger(movement["point"]), new WS_DBCDatabase.CreatureMovePoint(Conversions.ToSingle(movement["position_x"]), Conversions.ToSingle(movement["position_y"]), Conversions.ToSingle(movement["position_z"]), Conversions.ToInteger(movement["delay"]), Conversions.ToInteger(movement["move_flag"]), Conversions.ToInteger(movement["action"]), Conversions.ToInteger(movement["action_chance"])));
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
						DataRow equipInfo = (DataRow)enumerator.Current;
						int entry = Conversions.ToInteger(equipInfo["entry"]);
						if (!WorldServiceLocator._WS_DBCDatabase.CreatureEquip.ContainsKey(entry))
						{
							try
							{
								WorldServiceLocator._WS_DBCDatabase.CreatureEquip.Add(entry, new WS_DBCDatabase.CreatureEquipInfo(Conversions.ToInteger(equipInfo["equipmodel1"]), Conversions.ToInteger(equipInfo["equipmodel2"]), Conversions.ToInteger(equipInfo["equipmodel3"]), Conversions.ToUInteger(equipInfo["equipinfo1"]), Conversions.ToUInteger(equipInfo["equipinfo2"]), Conversions.ToUInteger(equipInfo["equipinfo3"]), Conversions.ToInteger(equipInfo["equipslot1"]), Conversions.ToInteger(equipInfo["equipslot2"]), Conversions.ToInteger(equipInfo["equipslot3"])));
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
						DataRow modelInfo = (DataRow)enumerator.Current;
						int entry = Conversions.ToInteger(modelInfo["modelid"]);
						if (!WorldServiceLocator._WS_DBCDatabase.CreatureModel.ContainsKey(entry))
						{
							WorldServiceLocator._WS_DBCDatabase.CreatureModel.Add(entry, new WS_DBCDatabase.CreatureModelInfo(Conversions.ToSingle(modelInfo["bounding_radius"]), Conversions.ToSingle(modelInfo["combat_reach"]), Conversions.ToByte(modelInfo["gender"]), Conversions.ToInteger(modelInfo["modelid_other_gender"])));
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
					DataRow starter3 = (DataRow)enumerator.Current;
					int entry4 = Conversions.ToInteger(starter3["entry"]);
					int quest4 = Conversions.ToInteger(starter3["quest"]);
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
					DataRow starter4 = (DataRow)enumerator2.Current;
					int entry3 = Conversions.ToInteger(starter4["entry"]);
					int quest3 = Conversions.ToInteger(starter4["quest"]);
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
						DataRow starter2 = (DataRow)enumerator3.Current;
						int entry2 = Conversions.ToInteger(starter2["entry"]);
						int quest2 = Conversions.ToInteger(starter2["quest"]);
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
						DataRow starter = (DataRow)enumerator4.Current;
						int entry = Conversions.ToInteger(starter["entry"]);
						int quest = Conversions.ToInteger(starter["quest"]);
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
						DataRow weather = (DataRow)enumerator.Current;
						int zone = Conversions.ToInteger(weather["zone"]);
						if (!WorldServiceLocator._WS_Weather.WeatherZones.ContainsKey(zone))
						{
							WS_Weather.WeatherZone zoneChanges = new WS_Weather.WeatherZone(zone);
							zoneChanges.Seasons[0] = new WS_Weather.WeatherSeasonChances(Conversions.ToInteger(weather["spring_rain_chance"]), Conversions.ToInteger(weather["spring_snow_chance"]), Conversions.ToInteger(weather["spring_storm_chance"]));
							zoneChanges.Seasons[1] = new WS_Weather.WeatherSeasonChances(Conversions.ToInteger(weather["summer_rain_chance"]), Conversions.ToInteger(weather["summer_snow_chance"]), Conversions.ToInteger(weather["summer_storm_chance"]));
							zoneChanges.Seasons[2] = new WS_Weather.WeatherSeasonChances(Conversions.ToInteger(weather["fall_rain_chance"]), Conversions.ToInteger(weather["fall_snow_chance"]), Conversions.ToInteger(weather["fall_storm_chance"]));
							zoneChanges.Seasons[3] = new WS_Weather.WeatherSeasonChances(Conversions.ToInteger(weather["winter_rain_chance"]), Conversions.ToInteger(weather["winter_snow_chance"]), Conversions.ToInteger(weather["winter_storm_chance"]));
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
