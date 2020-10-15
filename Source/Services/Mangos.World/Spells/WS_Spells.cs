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

using Mangos.Common.Enums.Faction;
using Mangos.Common.Enums.GameObject;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Item;
using Mangos.Common.Enums.Misc;
using Mangos.Common.Enums.Player;
using Mangos.Common.Enums.Spell;
using Mangos.Common.Globals;
using Mangos.World.AI;
using Mangos.World.DataStores;
using Mangos.World.Globals;
using Mangos.World.Handlers;
using Mangos.World.Loots;
using Mangos.World.Maps;
using Mangos.World.Objects;
using Mangos.World.Player;
using Mangos.World.Quests;
using Mangos.World.Server;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Mangos.World.Spells
{
    public class WS_Spells
	{
		public class SpellInfo
		{
			public int ID;

			public int School;

			public int Category;

			public int DispellType;

			public int Mechanic;

			public int Attributes;

			public int AttributesEx;

			public int AttributesEx2;

			public int RequredCasterStance;

			public int ShapeshiftExclude;

			public int Target;

			public int TargetCreatureType;

			public int FocusObjectIndex;

			public int FacingCasterFlags;

			public int CasterAuraState;

			public int TargetAuraState;

			public int ExcludeCasterAuraState;

			public int ExcludeTargetAuraState;

			public int SpellCastTimeIndex;

			public int CategoryCooldown;

			public int SpellCooldown;

			public int interruptFlags;

			public int auraInterruptFlags;

			public int channelInterruptFlags;

			public int procFlags;

			public int procChance;

			public int procCharges;

			public int maxLevel;

			public int baseLevel;

			public int spellLevel;

			public int maxStack;

			public int DurationIndex;

			public int powerType;

			public int manaCost;

			public int manaCostPerlevel;

			public int manaPerSecond;

			public int manaPerSecondPerLevel;

			public int manaCostPercent;

			public int rangeIndex;

			public float Speed;

			public int modalNextSpell;

			public int[] Totem;

			public int[] TotemCategory;

			public int[] Reagents;

			public int[] ReagentsCount;

			public int EquippedItemClass;

			public int EquippedItemSubClass;

			public int EquippedItemInventoryType;

			public SpellEffect[] SpellEffects;

			public int MaxTargets;

			public int RequiredAreaID;

			public int SpellVisual;

			public int SpellPriority;

			public int AffectedTargetLevel;

			public int SpellIconID;

			public int ActiveIconID;

			public int SpellNameFlag;

			public string Rank;

			public int RankFlags;

			public int StartRecoveryCategory;

			public int StartRecoveryTime;

			public int SpellFamilyName;

			public int SpellFamilyFlags;

			public int DamageType;

			public string Name;

			public uint CustomAttributs;

			public SpellSchoolMask SchoolMask => (SpellSchoolMask)(1 << School);

			public int GetDuration
			{
				get
				{
					if (WorldServiceLocator._WS_Spells.SpellDuration.ContainsKey(DurationIndex))
					{
						return WorldServiceLocator._WS_Spells.SpellDuration[DurationIndex];
					}
					return 0;
				}
			}

			public int GetRange
			{
				get
				{
					if (WorldServiceLocator._WS_Spells.SpellRange.ContainsKey(rangeIndex))
					{
						return checked((int)Math.Round(WorldServiceLocator._WS_Spells.SpellRange[rangeIndex]));
					}
					return 0;
				}
			}

			public string GetFocusObject
			{
				get
				{
					if (WorldServiceLocator._WS_Spells.SpellFocusObject.ContainsKey(FocusObjectIndex))
					{
						return WorldServiceLocator._WS_Spells.SpellFocusObject[FocusObjectIndex];
					}
					return Conversions.ToString(0);
				}
			}

			public int GetCastTime
			{
				get
				{
					if (WorldServiceLocator._WS_Spells.SpellCastTime.ContainsKey(SpellCastTimeIndex))
					{
						return WorldServiceLocator._WS_Spells.SpellCastTime[SpellCastTimeIndex];
					}
					return 0;
				}
			}

			public int GetManaCost(int level, int Mana) => checked((int)Math.Round(manaCost + manaCostPerlevel * level + Mana * (manaCostPercent / 100.0)));

			public bool IsAura
			{
				get
				{
					if (SpellEffects[0] != null && SpellEffects[0].ApplyAuraIndex != 0)
					{
						return true;
					}
					if (SpellEffects[1] != null && SpellEffects[1].ApplyAuraIndex != 0)
					{
						return true;
					}
					if (SpellEffects[2] != null && SpellEffects[2].ApplyAuraIndex != 0)
					{
						return true;
					}
					return false;
				}
			}

			public bool IsAOE
			{
				get
				{
					if (SpellEffects[0] != null && SpellEffects[0].IsAOE)
					{
						return true;
					}
					if (SpellEffects[1] != null && SpellEffects[1].IsAOE)
					{
						return true;
					}
					if (SpellEffects[2] != null && SpellEffects[2].IsAOE)
					{
						return true;
					}
					return false;
				}
			}

			public bool IsDispell
			{
				get
				{
					if (SpellEffects[0] != null && SpellEffects[0].ID == SpellEffects_Names.SPELL_EFFECT_DISPEL)
					{
						return true;
					}
					if (SpellEffects[1] != null && SpellEffects[1].ID == SpellEffects_Names.SPELL_EFFECT_DISPEL)
					{
						return true;
					}
					if (SpellEffects[2] != null && SpellEffects[2].ID == SpellEffects_Names.SPELL_EFFECT_DISPEL)
					{
						return true;
					}
					return false;
				}
			}

			public bool IsPassive => ((uint)Attributes & 0x40u) != 0 && (AttributesEx & 0x80) == 0;

			public bool IsNegative
			{
				get
				{
					byte i = 0;
					do
					{
						if (SpellEffects[i] != null && SpellEffects[i].IsNegative)
						{
							return true;
						}
						checked
						{
							i = (byte)unchecked((uint)(i + 1));
						}
					}
					while (i <= 2u);
					return (AttributesEx & 0x80) != 0;
				}
			}

			public bool IsAutoRepeat => (AttributesEx2 & 0x20) != 0;

			public bool IsRanged => DamageType == 3;

			public bool IsMelee => DamageType == 2;

			public bool CanStackSpellRank
			{
				get
				{
					if (!WorldServiceLocator._WS_Spells.SpellChains.ContainsKey(ID) || WorldServiceLocator._WS_Spells.SpellChains[ID] == 0)
					{
						return true;
					}
					if (powerType == 0)
					{
						if (manaCost > 0)
						{
							return true;
						}
						if (manaCostPercent > 0)
						{
							return true;
						}
						if (manaCostPerlevel > 0)
						{
							return true;
						}
						if (manaPerSecond > 0)
						{
							return true;
						}
						if (manaPerSecondPerLevel > 0)
						{
							return true;
						}
					}
					return false;
				}
			}

			public SpellInfo()
			{
				ID = 0;
				School = 0;
				Category = 0;
				DispellType = 0;
				Mechanic = 0;
				Attributes = 0;
				AttributesEx = 0;
				AttributesEx2 = 0;
				RequredCasterStance = 0;
				ShapeshiftExclude = 0;
				Target = 0;
				TargetCreatureType = 0;
				FocusObjectIndex = 0;
				FacingCasterFlags = 0;
				CasterAuraState = 0;
				TargetAuraState = 0;
				ExcludeCasterAuraState = 0;
				ExcludeTargetAuraState = 0;
				SpellCastTimeIndex = 0;
				CategoryCooldown = 0;
				SpellCooldown = 0;
				interruptFlags = 0;
				auraInterruptFlags = 0;
				channelInterruptFlags = 0;
				procFlags = 0;
				procChance = 0;
				procCharges = 0;
				maxLevel = 0;
				baseLevel = 0;
				spellLevel = 0;
				maxStack = 0;
				DurationIndex = 0;
				powerType = 0;
				manaCost = 0;
				manaCostPerlevel = 0;
				manaPerSecond = 0;
				manaPerSecondPerLevel = 0;
				manaCostPercent = 0;
				rangeIndex = 0;
				Speed = 0f;
				modalNextSpell = 0;
				Totem = new int[2];
				TotemCategory = new int[2];
				Reagents = new int[8];
				ReagentsCount = new int[8];
				EquippedItemClass = 0;
				EquippedItemSubClass = 0;
				EquippedItemInventoryType = 0;
				SpellEffects = new SpellEffect[3];
				MaxTargets = 0;
				RequiredAreaID = 0;
				SpellVisual = 0;
				SpellPriority = 0;
				AffectedTargetLevel = 0;
				SpellIconID = 0;
				ActiveIconID = 0;
				SpellNameFlag = 0;
				Rank = "";
				RankFlags = 0;
				StartRecoveryCategory = 0;
				StartRecoveryTime = 0;
				SpellFamilyName = 0;
				SpellFamilyFlags = 0;
				DamageType = 0;
				Name = "";
				CustomAttributs = 0u;
			}

			public Dictionary<WS_Base.BaseObject, SpellMissInfo> GetTargets(ref WS_Base.BaseObject Caster, SpellTargets Targets, byte Index)
			{
				List<WS_Base.BaseObject> TargetsInfected = new List<WS_Base.BaseObject>();
				WS_Base.BaseUnit Ref = null;
				if (Caster is WS_Totems.TotemObject @object)
				{
					Ref = @object.Caster;
				}
				if (SpellEffects[Index] != null)
				{
					byte i = 0;
					do
					{
						SpellImplicitTargets ImplicitTarget = (SpellImplicitTargets)checked((byte)SpellEffects[Index].implicitTargetA);
						if (i == 1)
						{
							ImplicitTarget = (SpellImplicitTargets)checked((byte)SpellEffects[Index].implicitTargetB);
						}
						WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "{0}: {1}", Conversions.ToString(Interaction.IIf(i == 1, "ImplicitTargetB", "ImplicitTargetA")), ImplicitTarget);
						checked
						{
							switch (ImplicitTarget)
							{
							case SpellImplicitTargets.TARGET_ALL_ENEMY_IN_AREA:
							case SpellImplicitTargets.TARGET_ALL_ENEMY_IN_AREA_INSTANT:
							{
								List<WS_Base.BaseUnit> EnemyTargets2;
								if ((unchecked((uint)Targets.targetMask) & 0x40u) != 0)
								{
									WS_Spells wS_Spells6 = WorldServiceLocator._WS_Spells;
									WS_Base.BaseUnit objCharacter = (WS_Base.BaseUnit)Caster;
									List<WS_Base.BaseUnit> enemyAroundMe = wS_Spells6.GetEnemyAtPoint(ref objCharacter, Targets.dstX, Targets.dstY, Targets.dstZ, SpellEffects[Index].GetRadius);
									Caster = objCharacter;
									EnemyTargets2 = enemyAroundMe;
								}
								else if (Caster is WS_DynamicObjects.DynamicObjectObject object1)
								{
									EnemyTargets2 = WorldServiceLocator._WS_Spells.GetEnemyAtPoint(ref object1.Caster, Caster.positionX, Caster.positionY, Caster.positionZ, SpellEffects[Index].GetRadius);
								}
								else
								{
									WS_Spells wS_Spells7 = WorldServiceLocator._WS_Spells;
									WS_Base.BaseUnit objCharacter = (WS_Base.BaseUnit)Caster;
									List<WS_Base.BaseUnit> enemyAroundMe = wS_Spells7.GetEnemyAtPoint(ref objCharacter, Caster.positionX, Caster.positionY, Caster.positionZ, SpellEffects[Index].GetRadius);
									Caster = objCharacter;
									EnemyTargets2 = enemyAroundMe;
								}
								foreach (WS_Base.BaseUnit EnemyTarget in EnemyTargets2)
								{
									if (!TargetsInfected.Contains(EnemyTarget))
									{
										TargetsInfected.Add(EnemyTarget);
									}
								}
								break;
							}
							case SpellImplicitTargets.TARGET_ALL_FRIENDLY_UNITS_AROUND_CASTER:
							{
								WS_Spells wS_Spells5 = WorldServiceLocator._WS_Spells;
								WS_Base.BaseUnit objCharacter = (WS_Base.BaseUnit)Caster;
								List<WS_Base.BaseUnit> enemyAroundMe = wS_Spells5.GetEnemyAroundMe(ref objCharacter, SpellEffects[Index].GetRadius, Ref);
								Caster = objCharacter;
								List<WS_Base.BaseUnit> EnemyTargets6 = enemyAroundMe;
								foreach (WS_Base.BaseUnit EnemyTarget5 in EnemyTargets6)
								{
									if (!TargetsInfected.Contains(EnemyTarget5))
									{
										TargetsInfected.Add(EnemyTarget5);
									}
								}
								break;
							}
							case SpellImplicitTargets.TARGET_ALL_PARTY:
							{
								WS_Spells wS_Spells10 = WorldServiceLocator._WS_Spells;
								WS_PlayerData.CharacterObject objCharacter2 = (WS_PlayerData.CharacterObject)Caster;
								List<WS_Base.BaseUnit> enemyAroundMe = wS_Spells10.GetPartyMembersAroundMe(ref objCharacter2, 9999999f);
								Caster = objCharacter2;
								List<WS_Base.BaseUnit> PartyTargets2 = enemyAroundMe;
								foreach (WS_Base.BaseUnit PartyTarget2 in PartyTargets2)
								{
									if (!TargetsInfected.Contains(PartyTarget2))
									{
										TargetsInfected.Add(PartyTarget2);
									}
								}
								break;
							}
							case SpellImplicitTargets.TARGET_AROUND_CASTER_PARTY:
							case SpellImplicitTargets.TARGET_ALL_PARTY_AROUND_CASTER_2:
							case SpellImplicitTargets.TARGET_AREAEFFECT_PARTY:
							{
								List<WS_Base.BaseUnit> PartyTargets;
                                        switch (Caster)
                                        {
                                            case WS_Totems.TotemObject object2:
                                                {
                                                    WS_Spells wS_Spells8 = WorldServiceLocator._WS_Spells;
                                                    ref WS_Base.BaseUnit caster = ref object2.Caster;
                                                    WS_PlayerData.CharacterObject objCharacter2 = (WS_PlayerData.CharacterObject)caster;
                                                    List<WS_Base.BaseUnit> enemyAroundMe = wS_Spells8.GetPartyMembersAtPoint(ref objCharacter2, SpellEffects[Index].GetRadius, Caster.positionX, Caster.positionY, Caster.positionZ);
                                                    caster = objCharacter2;
                                                    PartyTargets = enemyAroundMe;
                                                    break;
                                                }

                                            default:
                                                {
                                                    WS_Spells wS_Spells9 = WorldServiceLocator._WS_Spells;
                                                    WS_PlayerData.CharacterObject objCharacter2 = (WS_PlayerData.CharacterObject)Caster;
                                                    List<WS_Base.BaseUnit> enemyAroundMe = wS_Spells9.GetPartyMembersAroundMe(ref objCharacter2, SpellEffects[Index].GetRadius);
                                                    Caster = objCharacter2;
                                                    PartyTargets = enemyAroundMe;
                                                    break;
                                                }
                                        }
                                        foreach (WS_Base.BaseUnit PartyTarget in PartyTargets)
								{
									if (!TargetsInfected.Contains(PartyTarget))
									{
										TargetsInfected.Add(PartyTarget);
									}
								}
								break;
							}
							case SpellImplicitTargets.TARGET_CHAIN_DAMAGE:
							case SpellImplicitTargets.TARGET_CHAIN_HEAL:
							{
								List<WS_Base.BaseUnit> UsedTargets = new List<WS_Base.BaseUnit>();
								WS_Base.BaseUnit TargetUnit = null;
								if (!TargetsInfected.Contains(Targets.unitTarget))
								{
									TargetsInfected.Add(Targets.unitTarget);
								}
								UsedTargets.Add(Targets.unitTarget);
								TargetUnit = Targets.unitTarget;
								if (SpellEffects[Index].ChainTarget <= 1)
								{
									break;
								}
								byte b = (byte)SpellEffects[Index].ChainTarget;
								byte j = 2;
								while (unchecked(j <= (uint)b))
								{
									WS_Spells wS_Spells2 = WorldServiceLocator._WS_Spells;
									WS_Base.BaseUnit objCharacter = (WS_Base.BaseUnit)Caster;
									List<WS_Base.BaseUnit> enemyAroundMe = wS_Spells2.GetEnemyAroundMe(ref TargetUnit, 10f, objCharacter);
									Caster = objCharacter;
									List<WS_Base.BaseUnit> EnemyTargets = enemyAroundMe;
									TargetUnit = null;
									float LowHealth = 1.01f;
									foreach (WS_Base.BaseUnit tmpUnit in EnemyTargets)
									{
										if (!UsedTargets.Contains(tmpUnit))
										{
											float TmpLife = (float)(tmpUnit.Life.Current / (double)tmpUnit.Life.Maximum);
											if (TmpLife < LowHealth)
											{
												LowHealth = TmpLife;
												TargetUnit = tmpUnit;
											}
										}
									}
									if (TargetUnit != null)
									{
										if (!TargetsInfected.Contains(TargetUnit))
										{
											TargetsInfected.Add(TargetUnit);
										}
										UsedTargets.Add(TargetUnit);
										j = (byte)unchecked((uint)(j + 1));
										continue;
									}
									break;
								}
								break;
							}
							case SpellImplicitTargets.TARGET_AROUND_CASTER_ENEMY:
							{
								WS_Spells wS_Spells = WorldServiceLocator._WS_Spells;
								WS_Base.BaseUnit objCharacter = (WS_Base.BaseUnit)Caster;
								List<WS_Base.BaseUnit> enemyAroundMe = wS_Spells.GetEnemyAroundMe(ref objCharacter, SpellEffects[Index].GetRadius, Ref);
								Caster = objCharacter;
								List<WS_Base.BaseUnit> EnemyTargets3 = enemyAroundMe;
								foreach (WS_Base.BaseUnit EnemyTarget2 in EnemyTargets3)
								{
									if (!TargetsInfected.Contains(EnemyTarget2))
									{
										TargetsInfected.Add(EnemyTarget2);
									}
								}
								break;
							}
							case SpellImplicitTargets.TARGET_DYNAMIC_OBJECT:
								if (Targets.goTarget != null)
								{
									TargetsInfected.Add(Targets.goTarget);
								}
								break;
							case SpellImplicitTargets.TARGET_INFRONT:
								if ((CustomAttributs & (true ? 1u : 0u)) != 0)
								{
									WS_Spells wS_Spells3 = WorldServiceLocator._WS_Spells;
									WS_Base.BaseUnit objCharacter = (WS_Base.BaseUnit)Caster;
									List<WS_Base.BaseUnit> enemyAroundMe = wS_Spells3.GetEnemyInBehindMe(ref objCharacter, SpellEffects[Index].GetRadius);
									Caster = objCharacter;
									List<WS_Base.BaseUnit> EnemyTargets5 = enemyAroundMe;
									foreach (WS_Base.BaseUnit EnemyTarget4 in EnemyTargets5)
									{
										if (!TargetsInfected.Contains(EnemyTarget4))
										{
											TargetsInfected.Add(EnemyTarget4);
										}
									}
								}
								else
								{
									if ((CustomAttributs & 2u) != 0)
									{
										break;
									}
									WS_Spells wS_Spells4 = WorldServiceLocator._WS_Spells;
									WS_Base.BaseUnit objCharacter = (WS_Base.BaseUnit)Caster;
									List<WS_Base.BaseUnit> enemyAroundMe = wS_Spells4.GetEnemyInFrontOfMe(ref objCharacter, SpellEffects[Index].GetRadius);
									Caster = objCharacter;
									List<WS_Base.BaseUnit> EnemyTargets4 = enemyAroundMe;
									foreach (WS_Base.BaseUnit EnemyTarget3 in EnemyTargets4)
									{
										if (!TargetsInfected.Contains(EnemyTarget3))
										{
											TargetsInfected.Add(EnemyTarget3);
										}
									}
								}
								break;
							case SpellImplicitTargets.TARGET_SELECTED_GAMEOBJECT:
							case SpellImplicitTargets.TARGET_GAMEOBJECT_AND_ITEM:
								if (Targets.goTarget != null)
								{
									TargetsInfected.Add(Targets.goTarget);
								}
								break;
							case SpellImplicitTargets.TARGET_SELF:
							case SpellImplicitTargets.TARGET_DUEL_VS_PLAYER:
							case SpellImplicitTargets.TARGET_MASTER:
							case SpellImplicitTargets.TARGET_SELF_FISHING:
							case SpellImplicitTargets.TARGET_SELF2:
								if (!TargetsInfected.Contains(Caster))
								{
									TargetsInfected.Add(Caster);
								}
								break;
							case SpellImplicitTargets.TARGET_NONCOMBAT_PET:
								if (Caster is WS_PlayerData.CharacterObject object3 && object3.NonCombatPet != null)
								{
									TargetsInfected.Add(object3.NonCombatPet);
								}
								break;
							case SpellImplicitTargets.TARGET_SELECTED_FRIEND:
							case SpellImplicitTargets.TARGET_SINGLE_PARTY:
							case SpellImplicitTargets.TARGET_SINGLE_FRIEND_2:
							case SpellImplicitTargets.TARGET_SINGLE_ENEMY:
								if (!TargetsInfected.Contains(Targets.unitTarget))
								{
									TargetsInfected.Add(Targets.unitTarget);
								}
								break;
							default:
								if (Targets.unitTarget != null)
								{
									if (!TargetsInfected.Contains(Targets.unitTarget))
									{
										TargetsInfected.Add(Targets.unitTarget);
									}
								}
								else if (!TargetsInfected.Contains(Caster))
								{
									TargetsInfected.Add(Caster);
								}
								break;
							case SpellImplicitTargets.TARGET_NOTHING:
							case SpellImplicitTargets.TARGET_PET:
							case SpellImplicitTargets.TARGET_EFFECT_SELECT:
							case SpellImplicitTargets.TARGET_MINION:
							case SpellImplicitTargets.TARGET_RANDOM_RAID_MEMBER:
							case SpellImplicitTargets.TARGET_BEHIND_VICTIM:
								break;
							}
							i = (byte)unchecked((uint)(i + 1));
						}
					}
					while (i <= 1u);
					if (SpellEffects[Index].implicitTargetA == 0 && SpellEffects[Index].implicitTargetB == 0 && TargetsInfected.Count == 0)
					{
						if (Targets.unitTarget != null)
						{
							if (!TargetsInfected.Contains(Targets.unitTarget))
							{
								TargetsInfected.Add(Targets.unitTarget);
							}
						}
						else if (!TargetsInfected.Contains(Caster))
						{
							TargetsInfected.Add(Caster);
						}
					}
					return CalculateMisses(ref Caster, ref TargetsInfected, ref SpellEffects[Index]);
				}
				return new Dictionary<WS_Base.BaseObject, SpellMissInfo>();
			}

			public Dictionary<WS_Base.BaseObject, SpellMissInfo> CalculateMisses(ref WS_Base.BaseObject Caster, ref List<WS_Base.BaseObject> Targets, ref SpellEffect SpellEffect)
			{
				Dictionary<WS_Base.BaseObject, SpellMissInfo> newTargets = new Dictionary<WS_Base.BaseObject, SpellMissInfo>();
				foreach (WS_Base.BaseObject Target in Targets)
				{
					if (Target != Caster && Caster is WS_Base.BaseUnit unit && Target is WS_Base.BaseUnit unit1)
					{
						WS_Base.BaseUnit baseUnit = unit1;
						if (SpellEffect.Mechanic > 0 && (baseUnit.MechanicImmunity & (1 << checked(SpellEffect.Mechanic - 1))) != 0)
						{
							newTargets.Add(Target, SpellMissInfo.SPELL_MISS_IMMUNE2);
						}
						else if (!IsNegative)
						{
							newTargets.Add(Target, SpellMissInfo.SPELL_MISS_NONE);
						}
						else if ((AttributesEx & 0x10000) == 0 && (baseUnit.SchoolImmunity & (1 << School)) != 0)
						{
							newTargets.Add(Target, SpellMissInfo.SPELL_MISS_IMMUNE2);
						}
						else if (Target is WS_Creatures.CreatureObject @object && @object.Evade)
						{
							newTargets.Add(Target, SpellMissInfo.SPELL_MISS_EVADE);
						}
						else if (!(Caster is WS_PlayerData.CharacterObject @object4) || !@object4.GM)
						{
							switch (DamageType)
							{
							case 0:
								newTargets.Add(Target, SpellMissInfo.SPELL_MISS_NONE);
								break;
							case 1:
							{
								WS_Base.BaseUnit baseUnit3 = baseUnit;
								WS_Base.BaseUnit Caster2 = unit;
								SpellMissInfo meleeSpellHitResult = baseUnit3.GetMagicSpellHitResult(ref Caster2, this);
								Caster = Caster2;
								newTargets.Add(Target, meleeSpellHitResult);
								break;
							}
							case 2:
							case 3:
							{
								WS_Base.BaseUnit baseUnit2 = baseUnit;
								WS_Base.BaseUnit Caster2 = unit;
								SpellMissInfo meleeSpellHitResult = baseUnit2.GetMeleeSpellHitResult(ref Caster2, this);
								Caster = Caster2;
								newTargets.Add(Target, meleeSpellHitResult);
								break;
							}
							}
						}
						baseUnit = null;
					}
					else
					{
						newTargets.Add(Target, SpellMissInfo.SPELL_MISS_NONE);
					}
				}
				return newTargets;
			}

			public List<WS_Base.BaseObject> GetHits(ref Dictionary<WS_Base.BaseObject, SpellMissInfo> Targets)
			{
				List<WS_Base.BaseObject> targetHits = new List<WS_Base.BaseObject>();
				foreach (KeyValuePair<WS_Base.BaseObject, SpellMissInfo> Target in Targets)
				{
					if (Target.Value == SpellMissInfo.SPELL_MISS_NONE)
					{
						targetHits.Add(Target.Key);
					}
				}
				return targetHits;
			}

			public void InitCustomAttributes()
			{
				CustomAttributs = 0u;
				bool auraSpell = true;
				int i = 0;
				do
				{
					if (SpellEffects[i] != null && SpellEffects[i].ID != SpellEffects_Names.SPELL_EFFECT_APPLY_AURA)
					{
						auraSpell = false;
						break;
					}
					i = checked(i + 1);
				}
				while (i <= 2);
				if (auraSpell)
				{
					CustomAttributs |= 64u;
				}
				if (SpellFamilyName == 10 && ((uint)SpellFamilyFlags & 0xC0000000u) != 0 && SpellEffects[0] != null)
				{
					SpellEffects[0].ID = SpellEffects_Names.SPELL_EFFECT_HEAL;
				}
				int j = 0;
				do
				{
					if (SpellEffects[j] != null)
					{
						switch (SpellEffects[j].ApplyAuraIndex)
						{
						case 3:
						case 53:
						case 89:
							CustomAttributs |= 16u;
							break;
						case 8:
						case 20:
							CustomAttributs |= 8u;
							break;
						case 26:
							CustomAttributs = CustomAttributs | 0x20u | 0x2000u;
							break;
						case 33:
							CustomAttributs |= 8192u;
							break;
						}
						switch (SpellEffects[j].ID)
						{
						case SpellEffects_Names.SPELL_EFFECT_SCHOOL_DAMAGE:
						case SpellEffects_Names.SPELL_EFFECT_HEAL:
						case SpellEffects_Names.SPELL_EFFECT_WEAPON_DAMAGE_NOSCHOOL:
						case SpellEffects_Names.SPELL_EFFECT_WEAPON_PERCENT_DAMAGE:
						case SpellEffects_Names.SPELL_EFFECT_WEAPON_DAMAGE:
							CustomAttributs |= 128u;
							break;
						case SpellEffects_Names.SPELL_EFFECT_CHARGE:
							if (Speed == 0f && SpellFamilyName == 0)
							{
								Speed = 42f;
							}
							CustomAttributs |= 256u;
							break;
						}
					}
					j = checked(j + 1);
				}
				while (j <= 2);
				int k = 0;
				do
				{
					if (SpellEffects[k] != null)
					{
						int applyAuraIndex = SpellEffects[k].ApplyAuraIndex;
						if (applyAuraIndex == 2 || (uint)(applyAuraIndex - 5) <= 2u || applyAuraIndex == 12)
						{
							CustomAttributs = (CustomAttributs | 0x20u) & 0xFFFFDFFFu;
						}
					}
					k = checked(k + 1);
				}
				while (k <= 2);
				if (SpellVisual == 3879)
				{
					CustomAttributs |= 1u;
				}
				switch (ID)
				{
				case 26029:
					CustomAttributs |= 2u;
					break;
				case 24340:
				case 26558:
				case 26789:
				case 28884:
					CustomAttributs |= 4u;
					break;
				case 8122:
				case 8124:
				case 10888:
				case 10890:
				case 12494:
					Attributes |= 1073741824;
					break;
				}
			}

			public void Cast(ref CastSpellParameters castParams)
			{
				try
				{
					WS_Base.BaseObject Caster = castParams.Caster;
					SpellTargets Targets = castParams.Targets;
					short CastFlags = 2;
					if (IsRanged)
					{
						CastFlags = checked((short)(CastFlags | 0x20));
					}
					Packets.PacketClass spellStart = new Packets.PacketClass(Opcodes.SMSG_SPELL_START);
					if (castParams.Item != null)
					{
						spellStart.AddPackGUID(castParams.Item.GUID);
					}
					else
					{
						spellStart.AddPackGUID(castParams.Caster.GUID);
					}
					spellStart.AddPackGUID(castParams.Caster.GUID);
					spellStart.AddInt32(ID);
					spellStart.AddInt16(CastFlags);
					if (castParams.InstantCast)
					{
						spellStart.AddInt32(0);
					}
					else
					{
						spellStart.AddInt32(GetCastTime);
					}
					Targets.WriteTargets(ref spellStart);
					if (((uint)CastFlags & 0x20u) != 0)
					{
						WS_Base.BaseUnit Caster2 = (WS_Base.BaseUnit)Caster;
						WriteAmmoToPacket(ref spellStart, ref Caster2);
						Caster = Caster2;
					}
					Caster.SendToNearPlayers(ref spellStart, 0uL);
					spellStart.Dispose();
					castParams.State = SpellCastState.SPELL_STATE_PREPARING;
					castParams.Stopped = false;
					if (Caster is WS_Creatures.CreatureObject @object)
					{
						if (Targets.unitTarget != null)
						{
							WS_Creatures.CreatureObject obj = @object;
							WS_Base.BaseObject unitTarget = Targets.unitTarget;
							obj.TurnTo(ref unitTarget);
						}
						else if (((uint)Targets.targetMask & 0x40u) != 0)
						{
							@object.TurnTo(Targets.dstX, Targets.dstY);
						}
					}
					bool NeedSpellLog = true;
					byte k = 0;
					do
					{
						if (SpellEffects[k] != null && SpellEffects[k].ID == SpellEffects_Names.SPELL_EFFECT_SCHOOL_DAMAGE)
						{
							NeedSpellLog = false;
						}
						checked
						{
							k = (byte)unchecked((uint)(k + 1));
						}
					}
					while (k <= 2u);
					if (NeedSpellLog)
					{
						SendSpellLog(ref Caster, ref Targets);
					}
					if (!castParams.InstantCast && GetCastTime > 0)
					{
						Thread.Sleep(GetCastTime);
						while (castParams.Delayed > 0)
						{
							int delayTime = castParams.Delayed;
							castParams.Delayed = 0;
							Thread.Sleep(delayTime);
						}
					}
					if (castParams.Stopped || castParams.State != SpellCastState.SPELL_STATE_PREPARING)
					{
						castParams.Dispose();
						return;
					}
					castParams.State = SpellCastState.SPELL_STATE_CASTING;
					int SpellTime = 0;
					float SpellDistance = 0f;
					if (Speed > 0f)
					{
						if (((uint)Targets.targetMask & 2u) != 0 && Targets.unitTarget != null)
						{
							SpellDistance = WorldServiceLocator._WS_Combat.GetDistance(Caster, Targets.unitTarget);
						}
						if (((uint)Targets.targetMask & 0x40u) != 0 && (Targets.dstX != 0f || Targets.dstY != 0f || Targets.dstZ != 0f))
						{
							SpellDistance = WorldServiceLocator._WS_Combat.GetDistance(Caster, Targets.dstX, Targets.dstY, Targets.dstZ);
						}
						if (((uint)Targets.targetMask & 0x800u) != 0 && Targets.goTarget != null)
						{
							SpellDistance = WorldServiceLocator._WS_Combat.GetDistance(Caster, Targets.goTarget);
						}
						if (SpellDistance > 0f)
						{
							SpellTime = checked((int)(SpellDistance / Speed * 1000f));
						}
					}
					SpellFailedReason SpellCastError = SpellFailedReason.SPELL_NO_ERROR;
					if ((!castParams.InstantCast || GetCastTime == 0) && Caster is WS_PlayerData.CharacterObject object1)
					{
						WS_PlayerData.CharacterObject Character = object1;
						SpellCastError = CanCast(ref Character, Targets, FirstCheck: false);
						if (SpellCastError != SpellFailedReason.SPELL_NO_ERROR)
						{
							WorldServiceLocator._WS_Spells.SendCastResult(SpellCastError, ref object1.client, ID);
							castParams.State = SpellCastState.SPELL_STATE_IDLE;
							castParams.Dispose();
							return;
						}
					}
					Dictionary<WS_Base.BaseObject, SpellMissInfo>[] TargetsInfected = new Dictionary<WS_Base.BaseObject, SpellMissInfo>[3]
					{
						GetTargets(ref Caster, Targets, 0),
						GetTargets(ref Caster, Targets, 1),
						GetTargets(ref Caster, Targets, 2)
					};
					if ((((uint)Attributes & 4u) != 0 || ((uint)Attributes & 0x400u) != 0) && Caster is WS_PlayerData.CharacterObject object2)
					{
						if (object2.attackState.combatNextAttackSpell)
						{
							WorldServiceLocator._WS_Spells.SendCastResult(SpellFailedReason.SPELL_FAILED_SPELL_IN_PROGRESS, ref object2.client, ID);
							castParams.Dispose();
							return;
						}
						object2.attackState.combatNextAttackSpell = true;
						object2.attackState.combatNextAttack.WaitOne();
					}
					List<WS_Base.BaseObject>[] TargetHits;
					byte i;
					checked
					{
                        if (Caster is WS_PlayerData.CharacterObject Character)
                        {
                            ItemObject castItem = null;
                            SendSpellCooldown(ref Character, castItem);
                            byte l = 0;
                            WS_PlayerData.CharacterObject characterObject = (WS_PlayerData.CharacterObject)Caster;
                            do
                            {
                                if (Reagents[l] != 0 && ReagentsCount[l] != 0)
                                {
                                    characterObject.ItemCONSUME(Reagents[l], ReagentsCount[l]);
                                }
                                l = (byte)unchecked((uint)(l + 1));
                            }
                            while (unchecked(l) <= 7u);
                            if (IsRanged && characterObject.AmmoID > 0)
                            {
                                characterObject.ItemCONSUME(characterObject.AmmoID, 1);
                            }
                            switch (powerType)
                            {
                                case 0:
                                    {
                                        int ManaCost = 0;
                                        if ((unchecked((uint)AttributesEx) & 2u) != 0)
                                        {
                                            characterObject.Mana.Current = 0;
                                            ManaCost = 1;
                                        }
                                        else
                                        {
                                            ManaCost = GetManaCost(unchecked(characterObject.Level), characterObject.Mana.Base);
                                            characterObject.Mana.Current -= ManaCost;
                                        }
                                        if (ManaCost > 0)
                                        {
                                            characterObject.spellCastManaRegeneration = 5;
                                            characterObject.SetUpdateFlag(23, characterObject.Mana.Current);
                                            characterObject.GroupUpdateFlag |= 16u;
                                            characterObject.SendCharacterUpdate();
                                        }
                                        break;
                                    }
                                case 1:
                                    if ((unchecked((uint)AttributesEx) & 2u) != 0)
                                    {
                                        characterObject.Rage.Current = 0;
                                    }
                                    else
                                    {
                                        characterObject.Rage.Current -= GetManaCost(unchecked(characterObject.Level), characterObject.Rage.Base);
                                    }
                                    characterObject.SetUpdateFlag(24, characterObject.Rage.Current);
                                    characterObject.GroupUpdateFlag |= 16u;
                                    characterObject.SendCharacterUpdate();
                                    break;
                                case -2:
                                    if ((unchecked((uint)AttributesEx) & 2u) != 0)
                                    {
                                        characterObject.Life.Current = 1;
                                    }
                                    else
                                    {
                                        characterObject.Life.Current -= GetManaCost(unchecked(characterObject.Level), characterObject.Life.Base);
                                    }
                                    characterObject.SetUpdateFlag(22, characterObject.Life.Current);
                                    characterObject.GroupUpdateFlag |= 2u;
                                    characterObject.SendCharacterUpdate();
                                    break;
                                case 3:
                                    if ((unchecked((uint)AttributesEx) & 2u) != 0)
                                    {
                                        characterObject.Energy.Current = 0;
                                    }
                                    else
                                    {
                                        characterObject.Energy.Current -= GetManaCost(unchecked(characterObject.Level), characterObject.Energy.Base);
                                    }
                                    characterObject.SetUpdateFlag(26, characterObject.Energy.Current);
                                    characterObject.GroupUpdateFlag |= 16u;
                                    characterObject.SendCharacterUpdate();
                                    break;
                            }
                            characterObject.RemoveAurasByInterruptFlag(32768);
                            if (Targets.unitTarget != null && Targets.unitTarget is WS_Creatures.CreatureObject object3)
                            {
                                WS_Quests aLLQUESTS = WorldServiceLocator._WorldServer.ALLQUESTS;
                                Character = (WS_PlayerData.CharacterObject)Caster;
                                WS_Creatures.CreatureObject creature = object3;
                                aLLQUESTS.OnQuestCastSpell(ref Character, ref creature, ID);
                            }
                            if (Targets.goTarget != null)
                            {
                                WS_Quests aLLQUESTS2 = WorldServiceLocator._WorldServer.ALLQUESTS;
                                Character = (WS_PlayerData.CharacterObject)Caster;
                                WS_GameObjects.GameObjectObject gameObject = (WS_GameObjects.GameObjectObject)Targets.goTarget;
                                aLLQUESTS2.OnQuestCastSpell(ref Character, ref gameObject, ID);
                            }
                            characterObject = null;
                        }
                        else if (Caster is WS_Creatures.CreatureObject creatureObject)
                        {
                            switch (powerType)
                            {
                                case 0:
                                    {
                                        creatureObject.Mana.Current -= GetManaCost(unchecked(creatureObject.Level), creatureObject.Mana.Base);
                                        Packets.UpdatePacketClass updatePacket2 = new Packets.UpdatePacketClass();
                                        Packets.UpdateClass powerUpdate2 = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
                                        powerUpdate2.SetUpdateFlag(23, creatureObject.Mana.Current);
                                        Packets.PacketClass packet = updatePacket2;
                                        WS_Creatures.CreatureObject creature = (WS_Creatures.CreatureObject)Caster;
                                        powerUpdate2.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref creature);
                                        updatePacket2 = (Packets.UpdatePacketClass)packet;
                                        WS_Creatures.CreatureObject creatureObject3 = creatureObject;
                                        packet = updatePacket2;
                                        creatureObject3.SendToNearPlayers(ref packet, 0uL);
                                        updatePacket2 = (Packets.UpdatePacketClass)packet;
                                        powerUpdate2.Dispose();
                                        break;
                                    }
                                case -2:
                                    {
                                        creatureObject.Life.Current -= GetManaCost(unchecked(creatureObject.Level), creatureObject.Life.Base);
                                        Packets.UpdatePacketClass updatePacket = new Packets.UpdatePacketClass();
                                        Packets.UpdateClass powerUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
                                        powerUpdate.SetUpdateFlag(22, creatureObject.Life.Current);
                                        Packets.PacketClass packet = updatePacket;
                                        WS_Creatures.CreatureObject creature = (WS_Creatures.CreatureObject)Caster;
                                        powerUpdate.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref creature);
                                        updatePacket = (Packets.UpdatePacketClass)packet;
                                        WS_Creatures.CreatureObject creatureObject2 = creatureObject;
                                        packet = updatePacket;
                                        creatureObject2.SendToNearPlayers(ref packet, 0uL);
                                        updatePacket = (Packets.UpdatePacketClass)packet;
                                        powerUpdate.Dispose();
                                        break;
                                    }
                            }
                        }
                        castParams.State = SpellCastState.SPELL_STATE_FINISHED;
						if (Caster is WS_PlayerData.CharacterObject object4)
						{
							WorldServiceLocator._WS_Spells.SendCastResult(SpellFailedReason.SPELL_NO_ERROR, ref object4.client, ID);
						}
						Dictionary<ulong, SpellMissInfo> tmpTargets = new Dictionary<ulong, SpellMissInfo>();
						byte j = 0;
						do
						{
							foreach (KeyValuePair<WS_Base.BaseObject, SpellMissInfo> tmpTarget in TargetsInfected[j])
							{
								if (!tmpTargets.ContainsKey(tmpTarget.Key.GUID))
								{
									tmpTargets.Add(tmpTarget.Key.GUID, tmpTarget.Value);
								}
							}
							j = (byte)unchecked((uint)(j + 1));
						}
						while (unchecked(j) <= 2u);
						SendSpellGO(ref Caster, ref Targets, ref tmpTargets, ref castParams.Item);
						if (channelInterruptFlags != 0)
						{
							castParams.State = SpellCastState.SPELL_STATE_CASTING;
							WS_Base.BaseUnit Caster2 = (WS_Base.BaseUnit)Caster;
							StartChannel(ref Caster2, GetDuration, ref Targets);
							Caster = Caster2;
						}
						if (Caster is WS_PlayerData.CharacterObject object5 && castParams.Item != null)
						{
							if (castParams.Item.ChargesLeft > 0)
							{
								castParams.Item.ChargesLeft--;
								object5.SendItemUpdate(castParams.Item);
							}
							if (castParams.Item.ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_CONSUMABLE)
							{
								castParams.Item.StackCount--;
								if (castParams.Item.StackCount <= 0)
								{
									byte bag = default;
									byte slot = object5.ItemGetSLOTBAG(castParams.Item.GUID, ref bag);
									if ((bag != WorldServiceLocator._Global_Constants.ITEM_SLOT_NULL) & (slot != WorldServiceLocator._Global_Constants.ITEM_SLOT_NULL))
									{
										object5.ItemREMOVE(bag, slot, Destroy: true, Update: true);
									}
								}
								else
								{
									object5.SendItemUpdate(castParams.Item);
								}
							}
						}
						if (castParams.State == SpellCastState.SPELL_STATE_FINISHED)
						{
							castParams.State = SpellCastState.SPELL_STATE_IDLE;
						}
						if (SpellTime > 0)
						{
							Thread.Sleep(SpellTime);
						}
						TargetHits = new List<WS_Base.BaseObject>[3]
						{
							GetHits(ref TargetsInfected[0]),
							GetHits(ref TargetsInfected[1]),
							GetHits(ref TargetsInfected[2])
						};
						i = 0;
					}
					do
					{
						if (SpellEffects[i] != null)
						{
							WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "DEBUG: Casting effect: {0}", SpellEffects[i].ID);
							SpellCastError = WorldServiceLocator._WS_Spells.SPELL_EFFECTs[(int)SpellEffects[i].ID](ref Targets, ref Caster, ref SpellEffects[i], ID, ref TargetHits[i], ref castParams.Item);
							if (SpellCastError != SpellFailedReason.SPELL_NO_ERROR)
							{
								break;
							}
						}
						checked
						{
							i = (byte)unchecked((uint)(i + 1));
						}
					}
					while (i <= 2u);
					if (SpellCastError != SpellFailedReason.SPELL_NO_ERROR)
					{
                        switch (Caster)
                        {
                            case WS_PlayerData.CharacterObject _:
                                {
                                    WorldServiceLocator._WS_Spells.SendCastResult(SpellCastError, ref ((WS_PlayerData.CharacterObject)Caster).client, ID);
                                    WS_Base.BaseUnit Caster2 = (WS_Base.BaseUnit)Caster;
                                    SendInterrupted(0, ref Caster2);
                                    Caster = Caster2;
                                    castParams.Dispose();
                                    break;
                                }

                            default:
                                {
                                    WS_Base.BaseUnit Caster2 = (WS_Base.BaseUnit)Caster;
                                    SendInterrupted(0, ref Caster2);
                                    Caster = Caster2;
                                    castParams.Dispose();
                                    break;
                                }
                        }
                        return;
					}
					if (castParams.State == SpellCastState.SPELL_STATE_CASTING && channelInterruptFlags != 0)
					{
						Thread.Sleep(GetDuration);
						castParams.State = SpellCastState.SPELL_STATE_IDLE;
					}
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception e = ex;
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error when casting spell.{0}", Environment.NewLine + e.ToString());
					ProjectData.ClearProjectError();
				}
				if (castParams != null)
				{
					try
					{
						castParams.Dispose();
					}
					catch (Exception projectError)
					{
						ProjectData.SetProjectError(projectError);
						ProjectData.ClearProjectError();
					}
				}
			}

			public void Apply(ref WS_Base.BaseObject caster, SpellTargets Targets)
			{
				List<WS_Base.BaseObject>[] TargetsInfected = new List<WS_Base.BaseObject>[3];
				Dictionary<WS_Base.BaseObject, SpellMissInfo> Targets2 = GetTargets(ref caster, Targets, 0);
				TargetsInfected[0] = GetHits(ref Targets2);
				Targets2 = GetTargets(ref caster, Targets, 1);
				TargetsInfected[1] = GetHits(ref Targets2);
				Targets2 = GetTargets(ref caster, Targets, 2);
				TargetsInfected[2] = GetHits(ref Targets2);
				if (SpellEffects[0] != null)
				{
					SpellEffectHandler obj = WorldServiceLocator._WS_Spells.SPELL_EFFECTs[(int)SpellEffects[0].ID];
					ref SpellEffect spellInfo = ref SpellEffects[0];
					int iD = ID;
					ref List<WS_Base.BaseObject> infected = ref TargetsInfected[0];
					ItemObject Item = null;
					obj(ref Targets, ref caster, ref spellInfo, iD, ref infected, ref Item);
				}
				if (SpellEffects[1] != null)
				{
					SpellEffectHandler obj2 = WorldServiceLocator._WS_Spells.SPELL_EFFECTs[(int)SpellEffects[1].ID];
					ref SpellEffect spellInfo2 = ref SpellEffects[1];
					int iD2 = ID;
					ref List<WS_Base.BaseObject> infected2 = ref TargetsInfected[1];
					ItemObject Item = null;
					obj2(ref Targets, ref caster, ref spellInfo2, iD2, ref infected2, ref Item);
				}
				if (SpellEffects[2] != null)
				{
					SpellEffectHandler obj3 = WorldServiceLocator._WS_Spells.SPELL_EFFECTs[(int)SpellEffects[2].ID];
					ref SpellEffect spellInfo3 = ref SpellEffects[2];
					int iD3 = ID;
					ref List<WS_Base.BaseObject> infected3 = ref TargetsInfected[2];
					ItemObject Item = null;
					obj3(ref Targets, ref caster, ref spellInfo3, iD3, ref infected3, ref Item);
				}
			}

			public SpellFailedReason CanCast(ref WS_PlayerData.CharacterObject Character, SpellTargets Targets, bool FirstCheck)
			{
				if (Character.Spell_Silenced)
				{
					return SpellFailedReason.SPELL_FAILED_SILENCED;
				}
				if (((uint)Character.cUnitFlags & 0x100000u) != 0)
				{
					return SpellFailedReason.SPELL_FAILED_ERROR;
				}
				if (Targets.unitTarget != null && Targets.unitTarget != Character)
				{
					if (((uint)FacingCasterFlags & (true ? 1u : 0u)) != 0)
					{
						WS_Combat wS_Combat = WorldServiceLocator._WS_Combat;
						WS_Base.BaseObject Object = Character;
						WS_Base.BaseObject Object2 = Targets.unitTarget;
						bool flag = wS_Combat.IsInFrontOf(ref Object, ref Object2);
						Character = (WS_PlayerData.CharacterObject)Object;
						if (!flag)
						{
							return SpellFailedReason.SPELL_FAILED_NOT_INFRONT;
						}
					}
					if (((uint)FacingCasterFlags & 2u) != 0)
					{
						WS_Combat wS_Combat2 = WorldServiceLocator._WS_Combat;
						WS_Base.BaseObject Object2 = Character;
						WS_Base.BaseObject Object = Targets.unitTarget;
						bool flag = wS_Combat2.IsInBackOf(ref Object2, ref Object);
						Character = (WS_PlayerData.CharacterObject)Object2;
						if (!flag)
						{
							return SpellFailedReason.SPELL_FAILED_NOT_BEHIND;
						}
					}
				}
				if (Targets.unitTarget != null && Targets.unitTarget != Character && Targets.unitTarget is WS_PlayerData.CharacterObject @object && @object.GM)
				{
					return SpellFailedReason.SPELL_FAILED_BAD_TARGETS;
				}
				if ((Attributes & 0x800000) == 0 && Character.IsDead)
				{
					return SpellFailedReason.SPELL_FAILED_CASTER_DEAD;
				}
				if (((uint)Attributes & 0x10000000u) != 0 && Character.IsInCombat)
				{
					return SpellFailedReason.SPELL_FAILED_INTERRUPTED_COMBAT;
				}
				int StanceMask = 0;
				checked
				{
					if (Character.ShapeshiftForm != 0)
					{
						StanceMask = 1 << unchecked((int)Character.ShapeshiftForm) - 1;
					}
					if ((StanceMask & ShapeshiftExclude) != 0)
					{
						return SpellFailedReason.SPELL_FAILED_NOT_SHAPESHIFT;
					}
				}
				if ((StanceMask & RequredCasterStance) == 0)
				{
					bool actAsShifted = false;
					if (Character.ShapeshiftForm != 0)
					{
						WS_DBCDatabase.TSpellShapeshiftForm ShapeShift = WorldServiceLocator._WS_DBCDatabase.FindShapeshiftForm((int)Character.ShapeshiftForm);
						if (ShapeShift == null)
						{
							goto IL_028d;
						}
						actAsShifted = ((ShapeShift.Flags1 & 1) == 0);
					}
					if (actAsShifted)
					{
						if (((uint)Attributes & 0x10000u) != 0)
						{
							return SpellFailedReason.SPELL_FAILED_ONLY_SHAPESHIFT;
						}
						if (RequredCasterStance != 0)
						{
							return SpellFailedReason.SPELL_FAILED_ONLY_SHAPESHIFT;
						}
					}
					else if ((AttributesEx2 & 0x80000) == 0 && RequredCasterStance != 0)
					{
						return SpellFailedReason.SPELL_FAILED_ONLY_SHAPESHIFT;
					}
				}
				goto IL_028d;
				IL_028d:
				if (((uint)Attributes & 0x20000u & (0u - ((Character.Invisibility != InvisibilityLevel.STEALTH) ? 1u : 0u))) != 0)
				{
					return SpellFailedReason.SPELL_FAILED_ONLY_STEALTHED;
				}
				if ((Character.charMovementFlags & WorldServiceLocator._Global_Constants.movementFlagsMask) != 0 && ((Character.charMovementFlags & 0x4000) == 0 || SpellEffects[0].ID != SpellEffects_Names.SPELL_EFFECT_STUCK) && (IsAutoRepeat || ((uint)auraInterruptFlags & 0x40000u) != 0))
				{
					return SpellFailedReason.SPELL_FAILED_MOVING;
				}
				int ManaCost = GetManaCost(Character.Level, Character.Mana.Base);
				if (ManaCost > 0)
				{
					if (powerType != (int)Character.ManaType)
					{
						return SpellFailedReason.SPELL_FAILED_NO_POWER;
					}
					switch (powerType)
					{
					case 0:
						if (ManaCost > Character.Mana.Current)
						{
							return SpellFailedReason.SPELL_FAILED_NO_POWER;
						}
						break;
					case 1:
						if (ManaCost > Character.Rage.Current)
						{
							return SpellFailedReason.SPELL_FAILED_NO_POWER;
						}
						break;
					case -2:
						if (ManaCost > Character.Life.Current)
						{
							return SpellFailedReason.SPELL_FAILED_NO_POWER;
						}
						break;
					case 3:
						if (ManaCost > Character.Energy.Current)
						{
							return SpellFailedReason.SPELL_FAILED_NO_POWER;
						}
						break;
					default:
						return SpellFailedReason.SPELL_FAILED_UNKNOWN;
					}
				}
				if (!FirstCheck && Mechanic != 0 && Targets.unitTarget != null && (Targets.unitTarget.MechanicImmunity & (1 << checked(Mechanic - 1))) != 0)
				{
					return SpellFailedReason.SPELL_FAILED_IMMUNE;
				}
				if (EquippedItemClass > 0 && EquippedItemSubClass > 0 && EquippedItemClass == 2)
				{
					bool FoundItem = false;
					int i = 0;
					do
					{
						if ((EquippedItemSubClass & (1 << i)) != 0)
						{
							switch (i)
							{
							case 1:
							case 5:
							case 6:
							case 8:
							case 10:
							case 17:
							case 20:
								if (Character.Items.ContainsKey(15) && !Character.Items[15].IsBroken() && (int)Character.Items[15].ItemInfo.ObjectClass == EquippedItemClass)
								{
									FoundItem = true;
									break;
								}
								goto IL_0721;
							case 0:
							case 4:
							case 7:
							case 13:
							case 15:
								if (Character.Items.ContainsKey(15) && !Character.Items[15].IsBroken() && (int)Character.Items[15].ItemInfo.ObjectClass == EquippedItemClass)
								{
									FoundItem = true;
									break;
								}
								if (Character.Items.ContainsKey(16) && !Character.Items[16].IsBroken() && (int)Character.Items[16].ItemInfo.ObjectClass == EquippedItemClass)
								{
									FoundItem = true;
									break;
								}
								goto IL_0721;
							case 2:
							case 3:
							case 16:
							case 18:
							case 19:
								if (Character.Items.ContainsKey(17) && !Character.Items[17].IsBroken() && (int)Character.Items[17].ItemInfo.ObjectClass == EquippedItemClass)
								{
									if (i == 2 || i == 18 || i == 3)
									{
										if (Character.AmmoID == 0)
										{
											return SpellFailedReason.SPELL_FAILED_NO_AMMO;
										}
										if (Character.ItemCOUNT(Character.AmmoID) == 0)
										{
											return SpellFailedReason.SPELL_FAILED_NO_AMMO;
										}
									}
									else if (i == 16 && Character.ItemCOUNT(Character.Items[17].ItemEntry) != 0)
									{
										return SpellFailedReason.SPELL_FAILED_NO_AMMO;
									}
									FoundItem = true;
									break;
								}
								goto IL_0721;
							default:
								goto IL_0721;
							}
							break;
						}
						goto IL_0721;
						IL_0721:
						i = checked(i + 1);
					}
					while (i <= 20);
					if (!FoundItem)
					{
						return SpellFailedReason.SPELL_FAILED_EQUIPPED_ITEM;
					}
				}
				byte j = 0;
				do
				{
					checked
					{
						if (SpellEffects[j] != null)
						{
							switch (SpellEffects[j].ID)
							{
							case SpellEffects_Names.SPELL_EFFECT_DUMMY:
								if (ID == 1648 && (Targets.unitTarget == null || Targets.unitTarget.Life.Current > Targets.unitTarget.Life.Maximum * 0.2))
								{
									return SpellFailedReason.SPELL_FAILED_BAD_TARGETS;
								}
								break;
							case SpellEffects_Names.SPELL_EFFECT_SCHOOL_DAMAGE:
								if (SpellVisual == 7250)
								{
									if (Targets.unitTarget == null)
									{
										return SpellFailedReason.SPELL_FAILED_BAD_IMPLICIT_TARGETS;
									}
									if (Targets.unitTarget.Life.Current > Targets.unitTarget.Life.Maximum * 0.2)
									{
										return SpellFailedReason.SPELL_FAILED_BAD_TARGETS;
									}
								}
								break;
							case SpellEffects_Names.SPELL_EFFECT_CHARGE:
								if (Character.IsRooted)
								{
									return SpellFailedReason.SPELL_FAILED_ROOTED;
								}
								break;
							case SpellEffects_Names.SPELL_EFFECT_SUMMON_OBJECT:
								if (SpellEffects[j].MiscValue == 35591)
								{
									float selectedX;
									float selectedY;
									if (SpellEffects[j].RadiusIndex > 0)
									{
										selectedX = (float)(Character.positionX + Math.Cos(Character.orientation) * SpellEffects[j].GetRadius);
										selectedY = (float)(Character.positionY + Math.Sin(Character.orientation) * SpellEffects[j].GetRadius);
									}
									else
									{
										selectedX = (float)(Character.positionX + Math.Cos(Character.orientation) * GetRange);
										selectedY = (float)(Character.positionY + Math.Sin(Character.orientation) * GetRange);
									}
									if (WorldServiceLocator._WS_Maps.GetZCoord(selectedX, selectedY, Character.MapID) > WorldServiceLocator._WS_Maps.GetWaterLevel(selectedX, selectedY, (int)Character.MapID))
									{
										return SpellFailedReason.SPELL_FAILED_NOT_FISHABLE;
									}
								}
								break;
							}
						}
						j = (byte)unchecked((uint)(j + 1));
					}
				}
				while (j <= 2u);
				if (Targets.unitTarget != null)
				{
					WS_Maps wS_Maps = WorldServiceLocator._WS_Maps;
					WS_Base.BaseObject Object = Character;
					ref WS_Base.BaseUnit unitTarget = ref Targets.unitTarget;
					WS_Base.BaseObject Object2 = unitTarget;
					bool flag = wS_Maps.IsInLineOfSight(ref Object, ref Object2);
					unitTarget = (WS_Base.BaseUnit)Object2;
					Character = (WS_PlayerData.CharacterObject)Object;
					if (!flag)
					{
						return SpellFailedReason.SPELL_FAILED_LINE_OF_SIGHT;
					}
				}
				else if (((uint)Targets.targetMask & 0x40u) != 0)
				{
					WS_Maps wS_Maps2 = WorldServiceLocator._WS_Maps;
					WS_Base.BaseObject Object2 = Character;
					bool flag = wS_Maps2.IsInLineOfSight(ref Object2, Targets.dstX, Targets.dstY, Targets.dstZ);
					Character = (WS_PlayerData.CharacterObject)Object2;
					if (!flag)
					{
						return SpellFailedReason.SPELL_FAILED_LINE_OF_SIGHT;
					}
				}
				return SpellFailedReason.SPELL_NO_ERROR;
			}

			public void StartChannel(ref WS_Base.BaseUnit Caster, int Duration, ref SpellTargets Targets)
			{
                switch (Caster)
                {
                    case WS_PlayerData.CharacterObject _:
                        {
                            Packets.PacketClass packet = new Packets.PacketClass(Opcodes.MSG_CHANNEL_START);
                            packet.AddInt32(ID);
                            packet.AddInt32(Duration);
                            ((WS_PlayerData.CharacterObject)Caster).client.Send(ref packet);
                            packet.Dispose();
                            break;
                        }

                    default:
                        if (!(Caster is WS_Creatures.CreatureObject))
                        {
                            return;
                        }

                        break;
                }
                Packets.UpdatePacketClass updatePacket = new Packets.UpdatePacketClass();
				Packets.UpdateClass updateBlock = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
				updateBlock.SetUpdateFlag(144, ID);
				if (Targets.unitTarget != null)
				{
					updateBlock.SetUpdateFlag(20, Targets.unitTarget.GUID);
				}
				Packets.PacketClass packet2;
				if (Caster is WS_Creatures.CreatureObject)
				{
					packet2 = updatePacket;
					WS_PlayerData.CharacterObject updateObject = (WS_PlayerData.CharacterObject)Caster;
					updateBlock.AddToPacket(ref packet2, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
					updatePacket = (Packets.UpdatePacketClass)packet2;
				}
				else if (Caster is WS_Creatures.CreatureObject @object)
				{
					packet2 = updatePacket;
					WS_Creatures.CreatureObject updateObject2 = @object;
					updateBlock.AddToPacket(ref packet2, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject2);
					updatePacket = (Packets.UpdatePacketClass)packet2;
				}
				WS_Base.BaseUnit obj = Caster;
				packet2 = updatePacket;
				obj.SendToNearPlayers(ref packet2, 0uL);
				updatePacket = (Packets.UpdatePacketClass)packet2;
				updatePacket.Dispose();
			}

			public void WriteAmmoToPacket(ref Packets.PacketClass Packet, ref WS_Base.BaseUnit Caster)
			{
				WS_Items.ItemInfo ItemInfo = null;
                if (Caster is WS_PlayerData.CharacterObject characterObject)
                {
                    ItemObject RangedItem = characterObject.ItemGET(0, 17);
                    if (RangedItem != null)
                    {
                        if (RangedItem.ItemInfo.InventoryType == INVENTORY_TYPES.INVTYPE_THROWN)
                        {
                            ItemInfo = RangedItem.ItemInfo;
                        }
                        else if (characterObject.AmmoID != 0 && WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(characterObject.AmmoID))
                        {
                            ItemInfo = WorldServiceLocator._WorldServer.ITEMDatabase[characterObject.AmmoID];
                        }
                    }
                }
                if (ItemInfo == null)
				{
					if (!WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(2512))
					{
						WS_Items.ItemInfo tmpInfo = new WS_Items.ItemInfo(2512);
						WorldServiceLocator._WorldServer.ITEMDatabase.Add(2512, tmpInfo);
					}
					ItemInfo = WorldServiceLocator._WorldServer.ITEMDatabase[2512];
				}
				Packet.AddInt32(ItemInfo.Model);
				Packet.AddInt32((int)ItemInfo.InventoryType);
			}

			public void SendInterrupted(byte result, ref WS_Base.BaseUnit Caster)
			{
				if (Caster is WS_PlayerData.CharacterObject @object)
				{
					Packets.PacketClass packet = new Packets.PacketClass(Opcodes.SMSG_SPELL_FAILURE);
					packet.AddUInt64(Caster.GUID);
					packet.AddInt32(ID);
					packet.AddInt8(result);
					@object.client.Send(ref packet);
					packet.Dispose();
				}
				Packets.PacketClass packet2 = new Packets.PacketClass(Opcodes.SMSG_SPELL_FAILED_OTHER);
				packet2.AddUInt64(Caster.GUID);
				packet2.AddInt32(ID);
				Caster.SendToNearPlayers(ref packet2, 0uL);
				packet2.Dispose();
			}

			public void SendSpellGO(ref WS_Base.BaseObject Caster, ref SpellTargets Targets, ref Dictionary<ulong, SpellMissInfo> InfectedTargets, ref ItemObject Item)
			{
				short castFlags = 256;
				Packets.PacketClass packet;
				checked
				{
					if (IsRanged)
					{
						castFlags = (short)(castFlags | 0x20);
					}
					if (Item != null)
					{
						castFlags = (short)(castFlags | 0x100);
					}
					int hits = 0;
					int misses = 0;
					foreach (KeyValuePair<ulong, SpellMissInfo> InfectedTarget in InfectedTargets)
					{
						if (InfectedTarget.Value == SpellMissInfo.SPELL_MISS_NONE)
						{
							hits++;
						}
						else
						{
							misses++;
						}
					}
					packet = new Packets.PacketClass(Opcodes.SMSG_SPELL_GO);
					if (Item != null)
					{
						packet.AddPackGUID(Item.GUID);
					}
					else
					{
						packet.AddPackGUID(Caster.GUID);
					}
					packet.AddPackGUID(Caster.GUID);
					packet.AddInt32(ID);
					packet.AddInt16(castFlags);
					packet.AddInt8((byte)hits);
					foreach (KeyValuePair<ulong, SpellMissInfo> Target2 in InfectedTargets)
					{
						if (Target2.Value == SpellMissInfo.SPELL_MISS_NONE)
						{
							packet.AddUInt64(Target2.Key);
						}
					}
					packet.AddInt8((byte)misses);
				}
				foreach (KeyValuePair<ulong, SpellMissInfo> Target in InfectedTargets)
				{
					if (Target.Value != 0)
					{
						packet.AddUInt64(Target.Key);
						packet.AddInt8((byte)Target.Value);
					}
				}
				Targets.WriteTargets(ref packet);
				if (((uint)castFlags & 0x20u) != 0)
				{
					WS_Base.BaseUnit Caster2 = (WS_Base.BaseUnit)Caster;
					WriteAmmoToPacket(ref packet, ref Caster2);
					Caster = Caster2;
				}
				Caster.SendToNearPlayers(ref packet, 0uL);
				packet.Dispose();
			}

			public void SendSpellMiss(ref WS_Base.BaseObject Caster, ref WS_Base.BaseUnit Target, SpellMissInfo MissInfo)
			{
				Packets.PacketClass packet = new Packets.PacketClass(Opcodes.SMSG_SPELLLOGMISS);
				packet.AddInt32(ID);
				packet.AddUInt64(Caster.GUID);
				packet.AddInt8(0);
				packet.AddInt32(1);
				packet.AddUInt64(Target.GUID);
				packet.AddInt8((byte)MissInfo);
				Caster.SendToNearPlayers(ref packet, 0uL);
				packet.Dispose();
			}

			public void SendSpellLog(ref WS_Base.BaseObject Caster, ref SpellTargets Targets)
			{
				Packets.PacketClass packet = new Packets.PacketClass(Opcodes.SMSG_SPELLLOGEXECUTE);
				if (Caster is WS_PlayerData.CharacterObject)
				{
					packet.AddPackGUID(Caster.GUID);
				}
				else if (Targets.unitTarget != null)
				{
					packet.AddPackGUID(Targets.unitTarget.GUID);
				}
				else
				{
					packet.AddPackGUID(Caster.GUID);
				}
				packet.AddInt32(ID);
				int numOfSpellEffects = 1;
				packet.AddInt32(numOfSpellEffects);
				ulong UnitTargetGUID = 0uL;
				if (Targets.unitTarget != null)
				{
					UnitTargetGUID = Targets.unitTarget.GUID;
				}
				ulong ItemTargetGUID = 0uL;
				if (Targets.itemTarget != null)
				{
					ItemTargetGUID = Targets.itemTarget.GUID;
				}
				int num = checked(numOfSpellEffects - 1);
				for (int i = 0; i <= num; i = checked(i + 1))
				{
					packet.AddInt32((int)SpellEffects[i].ID);
					packet.AddInt32(1);
					switch (SpellEffects[i].ID)
					{
					default:
						return;
					case SpellEffects_Names.SPELL_EFFECT_MANA_DRAIN:
						packet.AddPackGUID(UnitTargetGUID);
						packet.AddInt32(0);
						packet.AddInt32(0);
						packet.AddSingle(0f);
						break;
					case SpellEffects_Names.SPELL_EFFECT_ADD_EXTRA_ATTACKS:
						packet.AddPackGUID(UnitTargetGUID);
						packet.AddInt32(0);
						break;
					case SpellEffects_Names.SPELL_EFFECT_INTERRUPT_CAST:
						packet.AddPackGUID(UnitTargetGUID);
						packet.AddInt32(0);
						break;
					case SpellEffects_Names.SPELL_EFFECT_DURABILITY_DAMAGE:
						packet.AddPackGUID(UnitTargetGUID);
						packet.AddInt32(0);
						packet.AddInt32(0);
						break;
					case SpellEffects_Names.SPELL_EFFECT_OPEN_LOCK:
					case SpellEffects_Names.SPELL_EFFECT_OPEN_LOCK_ITEM:
						packet.AddPackGUID(ItemTargetGUID);
						break;
					case SpellEffects_Names.SPELL_EFFECT_CREATE_ITEM:
						packet.AddInt32(SpellEffects[i].ItemType);
						break;
					case SpellEffects_Names.SPELL_EFFECT_SUMMON:
					case SpellEffects_Names.SPELL_EFFECT_SUMMON_WILD:
					case SpellEffects_Names.SPELL_EFFECT_SUMMON_GUARDIAN:
					case SpellEffects_Names.SPELL_EFFECT_SUMMON_OBJECT:
					case SpellEffects_Names.SPELL_EFFECT_SUMMON_PET:
					case SpellEffects_Names.SPELL_EFFECT_SUMMON_POSSESSED:
					case SpellEffects_Names.SPELL_EFFECT_SUMMON_TOTEM:
					case SpellEffects_Names.SPELL_EFFECT_SUMMON_OBJECT_WILD:
					case SpellEffects_Names.SPELL_EFFECT_CREATE_HOUSE:
					case SpellEffects_Names.SPELL_EFFECT_DUEL:
					case SpellEffects_Names.SPELL_EFFECT_SUMMON_TOTEM_SLOT1:
					case SpellEffects_Names.SPELL_EFFECT_SUMMON_TOTEM_SLOT2:
					case SpellEffects_Names.SPELL_EFFECT_SUMMON_TOTEM_SLOT3:
					case SpellEffects_Names.SPELL_EFFECT_SUMMON_TOTEM_SLOT4:
					case SpellEffects_Names.SPELL_EFFECT_SUMMON_PHANTASM:
					case SpellEffects_Names.SPELL_EFFECT_SUMMON_CRITTER:
					case SpellEffects_Names.SPELL_EFFECT_SUMMON_OBJECT_SLOT1:
					case SpellEffects_Names.SPELL_EFFECT_SUMMON_OBJECT_SLOT2:
					case SpellEffects_Names.SPELL_EFFECT_SUMMON_OBJECT_SLOT3:
					case SpellEffects_Names.SPELL_EFFECT_SUMMON_OBJECT_SLOT4:
					case SpellEffects_Names.SPELL_EFFECT_SUMMON_DEMON:
					case SpellEffects_Names.SPELL_EFFECT_150:
						if (Targets.unitTarget != null)
						{
							packet.AddPackGUID(Targets.unitTarget.GUID);
						}
						else if (Targets.itemTarget != null)
						{
							packet.AddPackGUID(Targets.itemTarget.GUID);
						}
						else if (Targets.goTarget != null)
						{
							packet.AddPackGUID(Targets.goTarget.GUID);
						}
						else
						{
							packet.AddInt8(0);
						}
						break;
					case SpellEffects_Names.SPELL_EFFECT_FEED_PET:
						packet.AddInt32(Targets.itemTarget.ItemEntry);
						break;
					case SpellEffects_Names.SPELL_EFFECT_DISMISS_PET:
						packet.AddPackGUID(UnitTargetGUID);
						break;
					}
				}
				Caster.SendToNearPlayers(ref packet, 0uL);
				packet.Dispose();
			}

			public void SendSpellCooldown(ref WS_PlayerData.CharacterObject objCharacter, ItemObject castItem = null)
			{
				if (!objCharacter.Spells.ContainsKey(ID))
				{
					return;
				}
				int Recovery = SpellCooldown;
				int CatRecovery = CategoryCooldown;
				if (ID == 2764)
				{
					Recovery = objCharacter.GetAttackTime(WeaponAttackType.RANGED_ATTACK);
				}
				if (ID == 5019 && objCharacter.Items.ContainsKey(17))
				{
					Recovery = objCharacter.Items[17].ItemInfo.Delay;
				}
				checked
				{
					if (CatRecovery == 0 && Recovery == 0 && castItem != null)
					{
						int i = 0;
						do
						{
							if (castItem.ItemInfo.Spells[i].SpellID == ID)
							{
								Recovery = castItem.ItemInfo.Spells[i].SpellCooldown;
								CatRecovery = castItem.ItemInfo.Spells[i].SpellCategoryCooldown;
								break;
							}
							i++;
						}
						while (i <= 4);
					}
					if (CatRecovery == 0 && Recovery == 0)
					{
						return;
					}
					objCharacter.Spells[ID].Cooldown = (uint)(unchecked(WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now)) + unchecked(Recovery / 1000));
					if (castItem != null)
					{
						objCharacter.Spells[ID].CooldownItem = castItem.ItemEntry;
					}
					if (Recovery > 10000)
					{
						WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_spells SET cooldown={2}, cooldownitem={3} WHERE guid = {0} AND spellid = {1};", objCharacter.GUID, ID, objCharacter.Spells[ID].Cooldown, objCharacter.Spells[ID].CooldownItem));
					}
					Packets.PacketClass packet = new Packets.PacketClass(Opcodes.SMSG_SPELL_COOLDOWN);
					packet.AddUInt64(objCharacter.GUID);
					if (CatRecovery > 0)
					{
						foreach (KeyValuePair<int, CharacterSpell> Spell in objCharacter.Spells)
						{
							if (WorldServiceLocator._WS_Spells.SPELLs[Spell.Key].Category == Category)
							{
								packet.AddInt32(Spell.Key);
								if (Spell.Key != ID || Recovery == 0)
								{
									packet.AddInt32(CatRecovery);
								}
								else
								{
									packet.AddInt32(Recovery);
								}
							}
						}
					}
					else if (Recovery > 0)
					{
						packet.AddInt32(ID);
						packet.AddInt32(Recovery);
					}
					objCharacter.client.Send(ref packet);
					packet.Dispose();
					if (castItem != null)
					{
						packet = new Packets.PacketClass(Opcodes.SMSG_ITEM_COOLDOWN);
						packet.AddUInt64(castItem.GUID);
						packet.AddInt32(ID);
						objCharacter.client.Send(ref packet);
						packet.Dispose();
					}
				}
			}
		}

		public class SpellEffect
		{
			public SpellEffects_Names ID;

			public SpellInfo Spell;

			public int diceSides;

			public int diceBase;

			public float dicePerLevel;

			public int valueBase;

			public int valueDie;

			public int valuePerLevel;

			public int valuePerComboPoint;

			public int Mechanic;

			public int implicitTargetA;

			public int implicitTargetB;

			public int RadiusIndex;

			public int ApplyAuraIndex;

			public int Amplitude;

			public int MultipleValue;

			public int ChainTarget;

			public int ItemType;

			public int MiscValue;

			public int TriggerSpell;

			public float DamageMultiplier;

			public float GetRadius
			{
				get
				{
					if (WorldServiceLocator._WS_Spells.SpellRadius.ContainsKey(RadiusIndex))
					{
						return WorldServiceLocator._WS_Spells.SpellRadius[RadiusIndex];
					}
					return 0f;
				}
			}

			public int GetValue(int Level, int ComboPoints)
			{
				checked
				{
					try
					{
						return valueBase + Level * valuePerLevel + ComboPoints * valuePerComboPoint + WorldServiceLocator._WorldServer.Rnd.Next(1, (int)Math.Round(valueDie + Level * dicePerLevel));
					}
					catch (Exception projectError)
					{
						ProjectData.SetProjectError(projectError);
						int GetValue = valueBase + Level * valuePerLevel + ComboPoints * valuePerComboPoint + 1;
						ProjectData.ClearProjectError();
						return GetValue;
					}
				}
			}

			public bool IsNegative
			{
				get
				{
					if (ID != SpellEffects_Names.SPELL_EFFECT_APPLY_AURA)
					{
						return false;
					}
					switch (ApplyAuraIndex)
					{
					case 5:
					case 7:
					case 33:
					case 95:
						return true;
					case 2:
					case 3:
					case 25:
					case 60:
						return true;
					case 12:
					case 26:
					case 27:
					case 128:
						return true;
					case 43:
					case 53:
					case 64:
					case 89:
						return true;
					case 56:
					case 81:
					case 153:
					case 162:
						return true;
					case 13:
					case 29:
					case 80:
					case 137:
						if (valueBase < 0)
						{
							return true;
						}
						return false;
					default:
						return false;
					}
				}
			}

			public bool IsAOE
			{
				get
				{
					int i = 0;
					checked
					{
						do
						{
							switch ((i != 0) ? ((byte)implicitTargetB) : ((byte)implicitTargetA))
							{
							case 8:
							case 15:
							case 16:
							case 17:
							case 20:
							case 22:
							case 24:
							case 28:
							case 30:
							case 31:
							case 33:
							case 34:
							case 37:
							case 53:
							case 61:
							case 65:
								return true;
							}
							i++;
						}
						while (i <= 1);
						return false;
					}
				}
			}

			public SpellEffect(ref SpellInfo Spell)
			{
				ID = SpellEffects_Names.SPELL_EFFECT_NOTHING;
				diceSides = 0;
				diceBase = 0;
				dicePerLevel = 0f;
				valueBase = 0;
				valueDie = 0;
				valuePerLevel = 0;
				valuePerComboPoint = 0;
				Mechanic = 0;
				implicitTargetA = 0;
				implicitTargetB = 0;
				RadiusIndex = 0;
				ApplyAuraIndex = 0;
				Amplitude = 0;
				MultipleValue = 0;
				ChainTarget = 0;
				ItemType = 0;
				MiscValue = 0;
				TriggerSpell = 0;
				DamageMultiplier = 1f;
				this.Spell = Spell;
			}
		}

		public class SpellTargets
		{
			public WS_Base.BaseUnit unitTarget;

			public WS_Base.BaseObject goTarget;

			public WS_Corpses.CorpseObject corpseTarget;

			public ItemObject itemTarget;

			public float srcX;

			public float srcY;

			public float srcZ;

			public float dstX;

			public float dstY;

			public float dstZ;

			public string stringTarget;

			public int targetMask;

			public SpellTargets()
			{
				unitTarget = null;
				goTarget = null;
				corpseTarget = null;
				itemTarget = null;
				srcX = 0f;
				srcY = 0f;
				srcZ = 0f;
				dstX = 0f;
				dstY = 0f;
				dstZ = 0f;
				stringTarget = "";
				targetMask = 0;
			}

			public void ReadTargets(ref Packets.PacketClass packet, ref WS_Base.BaseObject Caster)
			{
				targetMask = packet.GetInt16();
				if (targetMask == 0)
				{
					unitTarget = (WS_Base.BaseUnit)Caster;
					return;
				}
				if (((uint)targetMask & 2u) != 0)
				{
					ulong GUID = packet.GetPackGuid();
					if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(GUID) || WorldServiceLocator._CommonGlobalFunctions.GuidIsPet(GUID))
					{
						unitTarget = WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID];
					}
					else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(GUID))
					{
						unitTarget = WorldServiceLocator._WorldServer.CHARACTERs[GUID];
					}
				}
				if (((uint)targetMask & 0x800u) != 0)
				{
					ulong GUID3 = packet.GetPackGuid();
					if (WorldServiceLocator._CommonGlobalFunctions.GuidIsGameObject(GUID3))
					{
						goTarget = WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID3];
					}
					else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsDnyamicObject(GUID3))
					{
						goTarget = WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs[GUID3];
					}
				}
				if (((uint)targetMask & 0x10u) != 0 || ((uint)targetMask & 0x1000u) != 0)
				{
					ulong GUID4 = packet.GetPackGuid();
					if (WorldServiceLocator._CommonGlobalFunctions.GuidIsItem(GUID4))
					{
						itemTarget = WorldServiceLocator._WorldServer.WORLD_ITEMs[GUID4];
					}
				}
				if (((uint)targetMask & 0x20u) != 0)
				{
					srcX = packet.GetFloat();
					srcY = packet.GetFloat();
					srcZ = packet.GetFloat();
				}
				if (((uint)targetMask & 0x40u) != 0)
				{
					dstX = packet.GetFloat();
					dstY = packet.GetFloat();
					dstZ = packet.GetFloat();
				}
				if (((uint)targetMask & 0x2000u) != 0)
				{
					stringTarget = packet.GetString();
				}
				if (((uint)targetMask & 0x8000u) != 0 || ((uint)targetMask & 0x200u) != 0)
				{
					ulong GUID2 = packet.GetPackGuid();
					if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCorpse(GUID2))
					{
						corpseTarget = WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs[GUID2];
					}
				}
			}

			public void WriteTargets(ref Packets.PacketClass packet)
			{
				packet.AddInt16(checked((short)targetMask));
				if (((uint)targetMask & 2u) != 0)
				{
					packet.AddPackGUID(unitTarget.GUID);
				}
				if (((uint)targetMask & 0x800u) != 0)
				{
					packet.AddPackGUID(goTarget.GUID);
				}
				if (((uint)targetMask & 0x10u) != 0 || ((uint)targetMask & 0x1000u) != 0)
				{
					packet.AddPackGUID(itemTarget.GUID);
				}
				if (((uint)targetMask & 0x20u) != 0)
				{
					packet.AddSingle(srcX);
					packet.AddSingle(srcY);
					packet.AddSingle(srcZ);
				}
				if (((uint)targetMask & 0x40u) != 0)
				{
					packet.AddSingle(dstX);
					packet.AddSingle(dstY);
					packet.AddSingle(dstZ);
				}
				if (((uint)targetMask & 0x2000u) != 0)
				{
					packet.AddString(stringTarget);
				}
				if (((uint)targetMask & 0x8000u) != 0 || ((uint)targetMask & 0x200u) != 0)
				{
					packet.AddPackGUID(corpseTarget.GUID);
				}
			}

			public void SetTarget_SELF(ref WS_Base.BaseUnit objCharacter)
			{
				unitTarget = objCharacter;
				checked
				{
					targetMask += 0;
				}
			}

			public void SetTarget_UNIT(ref WS_Base.BaseUnit objCharacter)
			{
				unitTarget = objCharacter;
				checked
				{
					targetMask += 2;
				}
			}

			public void SetTarget_OBJECT(ref WS_Base.BaseObject o)
			{
				goTarget = o;
				checked
				{
					targetMask += 2048;
				}
			}

			public void SetTarget_ITEM(ref ItemObject i)
			{
				itemTarget = i;
				checked
				{
					targetMask += 16;
				}
			}

			public void SetTarget_SOURCELOCATION(float x, float y, float z)
			{
				srcX = x;
				srcY = y;
				srcZ = z;
				checked
				{
					targetMask += 32;
				}
			}

			public void SetTarget_DESTINATIONLOCATION(float x, float y, float z)
			{
				dstX = x;
				dstY = y;
				dstZ = z;
				checked
				{
					targetMask += 64;
				}
			}

			public void SetTarget_STRING(string str)
			{
				stringTarget = str;
				checked
				{
					targetMask += 8192;
				}
			}
		}

		public class CastSpellParameters : IDisposable
		{
			public SpellTargets Targets;

			public WS_Base.BaseObject Caster;

			public int SpellID;

			public ItemObject Item;

			public bool InstantCast;

			public int Delayed;

			public bool Stopped;

			public SpellCastState State;

			private bool _disposedValue;

			public SpellInfo SpellInfo => WorldServiceLocator._WS_Spells.SPELLs[SpellID];

			public bool Finished => Stopped || State == SpellCastState.SPELL_STATE_FINISHED || State == SpellCastState.SPELL_STATE_IDLE;

			public CastSpellParameters(ref SpellTargets Targets, ref WS_Base.BaseObject Caster, int SpellID)
			{
				Item = null;
				InstantCast = false;
				Delayed = 0;
				Stopped = false;
				State = SpellCastState.SPELL_STATE_IDLE;
				this.Targets = Targets;
				this.Caster = Caster;
				this.SpellID = SpellID;
				Item = null;
				InstantCast = false;
			}

			public CastSpellParameters(ref SpellTargets Targets, ref WS_Base.BaseObject Caster, int SpellID, bool Instant)
			{
				Item = null;
				InstantCast = false;
				Delayed = 0;
				Stopped = false;
				State = SpellCastState.SPELL_STATE_IDLE;
				this.Targets = Targets;
				this.Caster = Caster;
				this.SpellID = SpellID;
				Item = null;
				InstantCast = Instant;
			}

			public CastSpellParameters(ref SpellTargets Targets, ref WS_Base.BaseObject Caster, int SpellID, ref ItemObject Item)
			{
				this.Item = null;
				InstantCast = false;
				Delayed = 0;
				Stopped = false;
				State = SpellCastState.SPELL_STATE_IDLE;
				this.Targets = Targets;
				this.Caster = Caster;
				this.SpellID = SpellID;
				this.Item = Item;
				InstantCast = false;
			}

			public CastSpellParameters(ref SpellTargets Targets, ref WS_Base.BaseObject Caster, int SpellID, ref ItemObject Item, bool Instant)
			{
				this.Item = null;
				InstantCast = false;
				Delayed = 0;
				Stopped = false;
				State = SpellCastState.SPELL_STATE_IDLE;
				this.Targets = Targets;
				this.Caster = Caster;
				this.SpellID = SpellID;
				this.Item = Item;
				InstantCast = Instant;
			}

			public void Cast(object status)
			{
				try
				{
					Stopped = false;
					SpellInfo spellInfo = WorldServiceLocator._WS_Spells.SPELLs[SpellID];
					CastSpellParameters castParams = this;
					spellInfo.Cast(ref castParams);
				}
				catch (Exception ex2)
				{
					ProjectData.SetProjectError(ex2);
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Cast Exception {0} : Interrupted {1}", SpellID, Stopped);
					ProjectData.ClearProjectError();
				}
			}

			public void StopCast()
			{
				try
				{
					Stopped = true;
				}
				catch (Exception ex2)
				{
					ProjectData.SetProjectError(ex2);
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "StopCast Exception {0} : Interrupted {1}", SpellID, Stopped);
					ProjectData.ClearProjectError();
				}
			}

			public void Delay()
			{
				checked
				{
					if (Caster != null && !Finished)
					{
						int resistChance = ((WS_Base.BaseUnit)Caster).GetAuraModifier(AuraEffects_Names.SPELL_AURA_RESIST_PUSHBACK);
						if (resistChance <= 0 || resistChance <= WorldServiceLocator._WorldServer.Rnd.Next(0, 100))
						{
							int delaytime = 200;
							Delayed += delaytime;
							Packets.PacketClass packet = new Packets.PacketClass(Opcodes.SMSG_SPELL_DELAYED);
							packet.AddPackGUID(Caster.GUID);
							packet.AddInt32(delaytime);
							Caster.SendToNearPlayers(ref packet, 0uL);
							packet.Dispose();
						}
					}
				}
			}

			protected virtual void Dispose(bool disposing)
			{
				if (!_disposedValue)
				{
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

		public class CharacterSpell
		{
			public int SpellID;

			public byte Active;

			public uint Cooldown;

			public int CooldownItem;

			public CharacterSpell(int SpellID, byte Active, uint Cooldown, int CooldownItem)
			{
				this.SpellID = SpellID;
				this.Active = Active;
				this.Cooldown = Cooldown;
				this.CooldownItem = CooldownItem;
			}
		}

		public delegate SpellFailedReason SpellEffectHandler(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item);

		public delegate void ApplyAuraHandler(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action);

		public Dictionary<int, int> SpellChains;

		public Dictionary<int, SpellInfo> SPELLs;

		public Dictionary<int, int> SpellCastTime;

		public Dictionary<int, float> SpellRadius;

		public Dictionary<int, float> SpellRange;

		public Dictionary<int, int> SpellDuration;

		public Dictionary<int, string> SpellFocusObject;

		public const int SPELL_EFFECT_COUNT = 153;

		public SpellEffectHandler[] SPELL_EFFECTs;

		private const int SLOT_NOT_FOUND = -1;

		private const int SLOT_CREATE_NEW = -2;

		private const int SLOT_NO_SPACE = int.MaxValue;

		public const int AURAs_COUNT = 261;

		public ApplyAuraHandler[] AURAs;

		public const int DUEL_COUNTDOWN = 3000;

		private const float DUEL_OUTOFBOUNDS_DISTANCE = 40f;

		public const byte DUEL_COUNTER_START = 10;

		public const byte DUEL_COUNTER_DISABLED = 11;

		public WS_Spells()
		{
			SpellChains = new Dictionary<int, int>();
			SPELLs = new Dictionary<int, SpellInfo>(29000);
			SpellCastTime = new Dictionary<int, int>();
			SpellRadius = new Dictionary<int, float>();
			SpellRange = new Dictionary<int, float>();
			SpellDuration = new Dictionary<int, int>();
			SpellFocusObject = new Dictionary<int, string>();
			SPELL_EFFECTs = new SpellEffectHandler[154];
			AURAs = new ApplyAuraHandler[262];
		}

		public void SendCastResult(SpellFailedReason result, ref WS_Network.ClientClass client, int id)
		{
			Packets.PacketClass packet = new Packets.PacketClass(Opcodes.SMSG_CAST_RESULT);
			packet.AddInt32(id);
			if (result != SpellFailedReason.SPELL_NO_ERROR)
			{
				packet.AddInt8(2);
				packet.AddInt8((byte)result);
			}
			else
			{
				packet.AddInt8(0);
			}
			client.Send(ref packet);
			packet.Dispose();
		}

		public void SendNonMeleeDamageLog(ref WS_Base.BaseUnit Caster, ref WS_Base.BaseUnit Target, int SpellID, int SchoolType, int Damage, int Resist, int Absorbed, bool CriticalHit)
		{
			Packets.PacketClass packet = new Packets.PacketClass(Opcodes.SMSG_SPELLNONMELEEDAMAGELOG);
			packet.AddPackGUID(Target.GUID);
			packet.AddPackGUID(Caster.GUID);
			packet.AddInt32(SpellID);
			packet.AddInt32(Damage);
			packet.AddInt8(checked((byte)SchoolType));
			packet.AddInt32(Absorbed);
			packet.AddInt32(Resist);
			packet.AddInt8(0);
			packet.AddInt8(0);
			packet.AddInt32(0);
			if (CriticalHit)
			{
				packet.AddInt8(2);
			}
			else
			{
				packet.AddInt8(0);
			}
			packet.AddInt32(0);
			Caster.SendToNearPlayers(ref packet, 0uL);
		}

		public void SendHealSpellLog(ref WS_Base.BaseUnit Caster, ref WS_Base.BaseUnit Target, int SpellID, int Damage, bool CriticalHit)
		{
			Packets.PacketClass packet = new Packets.PacketClass(Opcodes.SMSG_HEALSPELL_ON_PLAYER_OBSOLETE);
			packet.AddPackGUID(Target.GUID);
			packet.AddPackGUID(Caster.GUID);
			packet.AddInt32(SpellID);
			packet.AddInt32(Damage);
			if (CriticalHit)
			{
				packet.AddInt8(1);
			}
			else
			{
				packet.AddInt8(0);
			}
			Caster.SendToNearPlayers(ref packet, 0uL);
		}

		public void SendEnergizeSpellLog(ref WS_Base.BaseUnit Caster, ref WS_Base.BaseUnit Target, int SpellID, int Damage, int PowerType)
		{
		}

		public void SendPeriodicAuraLog(ref WS_Base.BaseUnit Caster, ref WS_Base.BaseUnit Target, int SpellID, int School, int Damage, int AuraIndex)
		{
			Packets.PacketClass packet = new Packets.PacketClass(Opcodes.SMSG_PERIODICAURALOG);
			packet.AddPackGUID(Target.GUID);
			packet.AddPackGUID(Caster.GUID);
			packet.AddInt32(SpellID);
			packet.AddInt32(1);
			packet.AddInt8(checked((byte)AuraIndex));
			packet.AddInt32(Damage);
			packet.AddInt32(School);
			packet.AddInt32(0);
			Caster.SendToNearPlayers(ref packet, 0uL);
			packet.Dispose();
		}

		public void SendPlaySpellVisual(ref WS_Base.BaseUnit Caster, int SpellId)
		{
			Packets.PacketClass packet = new Packets.PacketClass(Opcodes.SMSG_PLAY_SPELL_VISUAL);
			packet.AddUInt64(Caster.GUID);
			packet.AddInt32(SpellId);
			Caster.SendToNearPlayers(ref packet, 0uL);
			packet.Dispose();
		}

		public void SendChannelUpdate(ref WS_PlayerData.CharacterObject Caster, int Time)
		{
			Packets.PacketClass packet = new Packets.PacketClass(Opcodes.MSG_CHANNEL_UPDATE);
			packet.AddInt32(Time);
			Caster.client.Send(ref packet);
			packet.Dispose();
			if (Time == 0)
			{
				Caster.SetUpdateFlag(20, 0L);
				Caster.SetUpdateFlag(144, 0);
				Caster.SendCharacterUpdate();
			}
		}

		public void InitializeSpellDB()
		{
			int j = 0;
			checked
			{
				do
				{
					SPELL_EFFECTs[j] = SPELL_EFFECT_NOTHING;
					j++;
				}
				while (j <= 153);
				SPELL_EFFECTs[0] = SPELL_EFFECT_NOTHING;
				SPELL_EFFECTs[1] = SPELL_EFFECT_INSTAKILL;
				SPELL_EFFECTs[2] = SPELL_EFFECT_SCHOOL_DAMAGE;
				SPELL_EFFECTs[3] = SPELL_EFFECT_DUMMY;
				SPELL_EFFECTs[5] = SPELL_EFFECT_TELEPORT_UNITS;
				SPELL_EFFECTs[6] = SPELL_EFFECT_APPLY_AURA;
				SPELL_EFFECTs[7] = SPELL_EFFECT_ENVIRONMENTAL_DAMAGE;
				SPELL_EFFECTs[8] = SPELL_EFFECT_MANA_DRAIN;
				SPELL_EFFECTs[10] = SPELL_EFFECT_HEAL;
				SPELL_EFFECTs[11] = SPELL_EFFECT_BIND;
				SPELL_EFFECTs[16] = SPELL_EFFECT_QUEST_COMPLETE;
				SPELL_EFFECTs[17] = SPELL_EFFECT_WEAPON_DAMAGE_NOSCHOOL;
				SPELL_EFFECTs[18] = SPELL_EFFECT_RESURRECT;
				SPELL_EFFECTs[20] = SPELL_EFFECT_DODGE;
				SPELL_EFFECTs[21] = SPELL_EFFECT_EVADE;
				SPELL_EFFECTs[22] = SPELL_EFFECT_PARRY;
				SPELL_EFFECTs[23] = SPELL_EFFECT_BLOCK;
				SPELL_EFFECTs[24] = SPELL_EFFECT_CREATE_ITEM;
				SPELL_EFFECTs[27] = SPELL_EFFECT_PERSISTENT_AREA_AURA;
				SPELL_EFFECTs[28] = SPELL_EFFECT_SUMMON;
				SPELL_EFFECTs[29] = SPELL_EFFECT_LEAP;
				SPELL_EFFECTs[30] = SPELL_EFFECT_ENERGIZE;
				SPELL_EFFECTs[33] = SPELL_EFFECT_OPEN_LOCK;
				SPELL_EFFECTs[35] = SPELL_EFFECT_APPLY_AREA_AURA;
				SPELL_EFFECTs[36] = SPELL_EFFECT_LEARN_SPELL;
				SPELL_EFFECTs[38] = SPELL_EFFECT_DISPEL;
				SPELL_EFFECTs[40] = SPELL_EFFECT_DUAL_WIELD;
				SPELL_EFFECTs[41] = SPELL_EFFECT_SUMMON_WILD;
				SPELL_EFFECTs[42] = SPELL_EFFECT_SUMMON_WILD;
				SPELL_EFFECTs[44] = SPELL_EFFECT_SKILL_STEP;
				SPELL_EFFECTs[45] = SPELL_EFFECT_HONOR;
				SPELL_EFFECTs[48] = SPELL_EFFECT_STEALTH;
				SPELL_EFFECTs[49] = SPELL_EFFECT_DETECT;
				SPELL_EFFECTs[50] = SPELL_EFFECT_SUMMON_OBJECT;
				SPELL_EFFECTs[53] = SPELL_EFFECT_ENCHANT_ITEM;
				SPELL_EFFECTs[54] = SPELL_EFFECT_ENCHANT_ITEM_TEMPORARY;
				SPELL_EFFECTs[58] = SPELL_EFFECT_WEAPON_DAMAGE;
				SPELL_EFFECTs[59] = SPELL_EFFECT_OPEN_LOCK;
				SPELL_EFFECTs[60] = SPELL_EFFECT_PROFICIENCY;
				SPELL_EFFECTs[64] = SPELL_EFFECT_TRIGGER_SPELL;
				SPELL_EFFECTs[67] = SPELL_EFFECT_HEAL_MAX_HEALTH;
				SPELL_EFFECTs[68] = SPELL_EFFECT_INTERRUPT_CAST;
				SPELL_EFFECTs[71] = SPELL_EFFECT_PICKPOCKET;
				SPELL_EFFECTs[74] = SPELL_EFFECT_SUMMON_TOTEM;
				SPELL_EFFECTs[77] = SPELL_EFFECT_SCRIPT_EFFECT;
				SPELL_EFFECTs[83] = SPELL_EFFECT_DUEL;
				SPELL_EFFECTs[87] = SPELL_EFFECT_SUMMON_TOTEM;
				SPELL_EFFECTs[88] = SPELL_EFFECT_SUMMON_TOTEM;
				SPELL_EFFECTs[89] = SPELL_EFFECT_SUMMON_TOTEM;
				SPELL_EFFECTs[90] = SPELL_EFFECT_SUMMON_TOTEM;
				SPELL_EFFECTs[92] = SPELL_EFFECT_ENCHANT_HELD_ITEM;
				SPELL_EFFECTs[95] = SPELL_EFFECT_SKINNING;
				SPELL_EFFECTs[96] = SPELL_EFFECT_CHARGE;
				SPELL_EFFECTs[98] = SPELL_EFFECT_KNOCK_BACK;
				SPELL_EFFECTs[99] = SPELL_EFFECT_DISENCHANT;
				SPELL_EFFECTs[113] = SPELL_EFFECT_RESURRECT_NEW;
				SPELL_EFFECTs[120] = SPELL_EFFECT_TELEPORT_GRAVEYARD;
				SPELL_EFFECTs[121] = SPELL_EFFECT_ADICIONAL_DMG;
				SPELL_EFFECTs[137] = SPELL_EFFECT_ENERGIZE_PCT;
				int i = 0;
				do
				{
					AURAs[i] = SPELL_AURA_NONE;
					i++;
				}
				while (i <= 261);
				AURAs[0] = SPELL_AURA_NONE;
				AURAs[1] = SPELL_AURA_BIND_SIGHT;
				AURAs[2] = SPELL_AURA_MOD_POSSESS;
				AURAs[3] = SPELL_AURA_PERIODIC_DAMAGE;
				AURAs[4] = SPELL_AURA_DUMMY;
				AURAs[7] = SPELL_AURA_MOD_FEAR;
				AURAs[8] = SPELL_AURA_PERIODIC_HEAL;
				AURAs[10] = SPELL_AURA_MOD_THREAT;
				AURAs[11] = SPELL_AURA_MOD_TAUNT;
				AURAs[12] = SPELL_AURA_MOD_STUN;
				AURAs[13] = SPELL_AURA_MOD_DAMAGE_DONE;
				AURAs[16] = SPELL_AURA_MOD_STEALTH;
				AURAs[17] = SPELL_AURA_MOD_DETECT;
				AURAs[18] = SPELL_AURA_MOD_INVISIBILITY;
				AURAs[19] = SPELL_AURA_MOD_INVISIBILITY_DETECTION;
				AURAs[20] = SPELL_AURA_PERIODIC_HEAL_PERCENT;
				AURAs[21] = SPELL_AURA_PERIODIC_ENERGIZE_PERCENT;
				AURAs[22] = SPELL_AURA_MOD_RESISTANCE;
				AURAs[23] = SPELL_AURA_PERIODIC_TRIGGER_SPELL;
				AURAs[24] = SPELL_AURA_PERIODIC_ENERGIZE;
				AURAs[25] = SPELL_AURA_MOD_PACIFY;
				AURAs[26] = SPELL_AURA_MOD_ROOT;
				AURAs[27] = SPELL_AURA_MOD_SILENCE;
				AURAs[29] = SPELL_AURA_MOD_STAT;
				AURAs[30] = SPELL_AURA_MOD_SKILL;
				AURAs[31] = SPELL_AURA_MOD_INCREASE_SPEED;
				AURAs[32] = SPELL_AURA_MOD_INCREASE_MOUNTED_SPEED;
				AURAs[33] = SPELL_AURA_MOD_DECREASE_SPEED;
				AURAs[34] = SPELL_AURA_MOD_INCREASE_HEALTH;
				AURAs[35] = SPELL_AURA_MOD_INCREASE_ENERGY;
				AURAs[36] = SPELL_AURA_MOD_SHAPESHIFT;
				AURAs[39] = SPELL_AURA_SCHOOL_IMMUNITY;
				AURAs[41] = SPELL_AURA_DISPEL_IMMUNITY;
				AURAs[42] = SPELL_AURA_PROC_TRIGGER_SPELL;
				AURAs[44] = SPELL_AURA_TRACK_CREATURES;
				AURAs[45] = SPELL_AURA_TRACK_RESOURCES;
				AURAs[53] = SPELL_AURA_PERIODIC_LEECH;
				AURAs[56] = SPELL_AURA_TRANSFORM;
				AURAs[58] = SPELL_AURA_MOD_INCREASE_SWIM_SPEED;
				AURAs[61] = SPELL_AURA_MOD_SCALE;
				AURAs[64] = SPELL_AURA_PERIODIC_MANA_LEECH;
				AURAs[67] = SPELL_AURA_MOD_DISARM;
				AURAs[69] = SPELL_AURA_SCHOOL_ABSORB;
				AURAs[75] = SPELL_AURA_MOD_LANGUAGE;
				AURAs[76] = SPELL_AURA_FAR_SIGHT;
				AURAs[77] = SPELL_AURA_MECHANIC_IMMUNITY;
				AURAs[78] = SPELL_AURA_MOUNTED;
				AURAs[79] = SPELL_AURA_MOD_DAMAGE_DONE_PCT;
				AURAs[80] = SPELL_AURA_MOD_STAT_PERCENT;
				AURAs[82] = SPELL_AURA_WATER_BREATHING;
				AURAs[83] = SPELL_AURA_MOD_BASE_RESISTANCE;
				AURAs[84] = SPELL_AURA_MOD_REGEN;
				AURAs[85] = SPELL_AURA_MOD_POWER_REGEN;
				AURAs[89] = SPELL_AURA_PERIODIC_DAMAGE_PERCENT;
				AURAs[95] = SPELL_AURA_GHOST;
				AURAs[99] = SPELL_AURA_MOD_ATTACK_POWER;
				AURAs[101] = SPELL_AURA_MOD_RESISTANCE_PCT;
				AURAs[103] = SPELL_AURA_MOD_TOTAL_THREAT;
				AURAs[104] = SPELL_AURA_WATER_WALK;
				AURAs[105] = SPELL_AURA_FEATHER_FALL;
				AURAs[106] = SPELL_AURA_HOVER;
				AURAs[107] = SPELL_AURA_ADD_FLAT_MODIFIER;
				AURAs[108] = SPELL_AURA_ADD_PCT_MODIFIER;
				AURAs[110] = SPELL_AURA_MOD_POWER_REGEN_PERCENT;
				AURAs[121] = SPELL_AURA_EMPATHY;
				AURAs[124] = SPELL_AURA_MOD_RANGED_ATTACK_POWER;
				AURAs[129] = SPELL_AURA_MOD_INCREASE_SPEED_ALWAYS;
				AURAs[130] = SPELL_AURA_MOD_INCREASE_MOUNTED_SPEED_ALWAYS;
				AURAs[135] = SPELL_AURA_MOD_HEALING_DONE;
				AURAs[136] = SPELL_AURA_MOD_HEALING_DONE_PCT;
				AURAs[137] = SPELL_AURA_MOD_TOTAL_STAT_PERCENTAGE;
				AURAs[138] = SPELL_AURA_MOD_HASTE;
				AURAs[140] = SPELL_AURA_MOD_RANGED_HASTE;
				AURAs[141] = SPELL_AURA_MOD_RANGED_AMMO_HASTE;
				AURAs[142] = SPELL_AURA_MOD_BASE_RESISTANCE_PCT;
				AURAs[143] = SPELL_AURA_MOD_RESISTANCE_EXCLUSIVE;
				AURAs[144] = SPELL_AURA_SAFE_FALL;
				AURAs[154] = SPELL_AURA_MOD_STEALTH_LEVEL;
				AURAs[226] = SPELL_AURA_PERIODIC_DUMMY;
				AURAs[228] = SPELL_AURA_DETECT_STEALTH;
			}
		}

		public SpellFailedReason SPELL_EFFECT_NOTHING(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_BIND(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			foreach (WS_Base.BaseUnit Unit in Infected)
			{
				if (Unit is WS_PlayerData.CharacterObject @object)
				{
					@object.BindPlayer(Caster.GUID);
				}
			}
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_DUMMY(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_INSTAKILL(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			foreach (WS_Base.BaseUnit Unit in Infected)
			{
				WS_Base.BaseUnit Attacker = (WS_Base.BaseUnit)Caster;
				Unit.Die(ref Attacker);
				Caster = Attacker;
			}
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_SCHOOL_DAMAGE(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			int Current = 0;
			foreach (WS_Base.BaseUnit Unit in Infected)
			{
				int Damage = (!(Caster is WS_DynamicObjects.DynamicObjectObject @object)) ? SpellInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) : SpellInfo.GetValue(@object.Caster.Level, 0);
				if (Current > 0)
				{
					Damage = checked((int)Math.Round(Damage * Math.Pow(SpellInfo.DamageMultiplier, Current)));
				}
				WS_Base.BaseUnit realCaster = null;
                switch (Caster)
                {
                    case WS_Base.BaseUnit _:
                        realCaster = (WS_Base.BaseUnit)Caster;
                        break;
                    case WS_DynamicObjects.DynamicObjectObject _:
                        realCaster = ((WS_DynamicObjects.DynamicObjectObject)Caster).Caster;
                        break;
                }
                if (realCaster != null)
				{
					Unit.DealSpellDamage(ref realCaster, ref SpellInfo, SpellID, Damage, (DamageTypes)checked((byte)SPELLs[SpellID].School), SpellType.SPELL_TYPE_NONMELEE);
				}
				Current = checked(Current + 1);
			}
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_ENVIRONMENTAL_DAMAGE(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			int Damage = SpellInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
			foreach (WS_Base.BaseUnit Unit in Infected)
			{
				WS_Base.BaseUnit Attacker = (WS_Base.BaseUnit)Caster;
				Unit.DealDamage(Damage, Attacker);
				Caster = Attacker;
				if (Unit is WS_PlayerData.CharacterObject @object)
				{
					@object.LogEnvironmentalDamage((DamageTypes)checked((byte)SPELLs[SpellID].School), Damage);
				}
			}
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_TRIGGER_SPELL(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			if (!SPELLs.ContainsKey(SpellInfo.TriggerSpell))
			{
				return SpellFailedReason.SPELL_NO_ERROR;
			}
			if (Target.unitTarget == null)
			{
				return SpellFailedReason.SPELL_NO_ERROR;
			}
			switch (SpellInfo.TriggerSpell)
			{
			case 18461:
			{
				WS_Base.BaseUnit unitTarget = Target.unitTarget;
				int NotSpellID = 0;
				unitTarget.RemoveAurasOfType(AuraEffects_Names.SPELL_AURA_MOD_ROOT, NotSpellID);
				WS_Base.BaseUnit unitTarget2 = Target.unitTarget;
				NotSpellID = 0;
				unitTarget2.RemoveAurasOfType(AuraEffects_Names.SPELL_AURA_MOD_DECREASE_SPEED, NotSpellID);
				WS_Base.BaseUnit unitTarget3 = Target.unitTarget;
				NotSpellID = 0;
				unitTarget3.RemoveAurasOfType(AuraEffects_Names.SPELL_AURA_MOD_STALKED, NotSpellID);
				break;
			}
			case 35729:
			{
				byte b2;
				byte i;
				checked
				{
					byte b = (byte)WorldServiceLocator._Global_Constants.MAX_POSITIVE_AURA_EFFECTs;
					b2 = (byte)(WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1);
					i = b;
				}
				while (i <= (uint)b2)
				{
					if (Target.unitTarget.ActiveSpells[i] != null && (SPELLs[Target.unitTarget.ActiveSpells[i].SpellID].School & 1) == 0 && ((uint)SPELLs[Target.unitTarget.ActiveSpells[i].SpellID].Attributes & 0x10000u) != 0)
					{
						Target.unitTarget.RemoveAura(i, ref Target.unitTarget.ActiveSpells[i].SpellCaster);
					}
					checked
					{
						i = (byte)unchecked((uint)(i + 1));
					}
				}
				break;
			}
			}
			if ((SPELLs[SpellInfo.TriggerSpell].EquippedItemClass >= 0) & (Caster is WS_PlayerData.CharacterObject))
			{
			}
			CastSpellParameters castParams = new CastSpellParameters(ref Target, ref Caster, SpellInfo.TriggerSpell);
			ThreadPool.QueueUserWorkItem(new WaitCallback(castParams.Cast));
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_TELEPORT_UNITS(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			foreach (WS_Base.BaseUnit Unit in Infected)
			{
				if (Unit is WS_PlayerData.CharacterObject @object)
                {
                    using WS_PlayerData.CharacterObject characterObject = @object;
                    if (SpellID == 8690)
                    {
                        characterObject.Teleport(characterObject.bindpoint_positionX, characterObject.bindpoint_positionY, characterObject.bindpoint_positionZ, characterObject.orientation, characterObject.bindpoint_map_id);
                    }
                    else if (WorldServiceLocator._WS_DBCDatabase.TeleportCoords.ContainsKey(SpellID))
                    {
                        characterObject.Teleport(WorldServiceLocator._WS_DBCDatabase.TeleportCoords[SpellID].PosX, WorldServiceLocator._WS_DBCDatabase.TeleportCoords[SpellID].PosY, WorldServiceLocator._WS_DBCDatabase.TeleportCoords[SpellID].PosZ, characterObject.orientation, checked((int)WorldServiceLocator._WS_DBCDatabase.TeleportCoords[SpellID].MapID));
                    }
                    else
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "WARNING: Spell {0} did not have any teleport coordinates.", SpellID);
                    }
                }
            }
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_MANA_DRAIN(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			checked
			{
				foreach (WS_Base.BaseUnit Unit in Infected)
				{
					int Damage = SpellInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0);
					if (Caster is WS_PlayerData.CharacterObject @object)
					{
						Damage += SpellInfo.valuePerLevel * unchecked(@object.Level);
					}
					int TargetPower = 0;
					switch (SpellInfo.MiscValue)
					{
					case 0:
						if (Damage > Unit.Mana.Current)
						{
							Damage = Unit.Mana.Current;
						}
						Unit.Mana.Current -= Damage;
						((WS_Base.BaseUnit)Caster).Mana.Current += Damage;
						TargetPower = Unit.Mana.Current;
						break;
					case 1:
						if (Unit is WS_PlayerData.CharacterObject object1 && Caster is WS_PlayerData.CharacterObject object2)
						{
							if (Damage > object1.Rage.Current)
							{
								Damage = object1.Rage.Current;
							}
							object1.Rage.Current -= Damage;
							object2.Rage.Current += Damage;
							TargetPower = object1.Rage.Current;
						}
						break;
					case 3:
						if (Unit is WS_PlayerData.CharacterObject object3 && Caster is WS_PlayerData.CharacterObject object4)
						{
							if (Damage > object3.Energy.Current)
							{
								Damage = object3.Energy.Current;
							}
							object3.Energy.Current -= Damage;
							object4.Energy.Current += Damage;
							TargetPower = object3.Energy.Current;
						}
						break;
					default:
						Unit.Mana.Current -= Damage;
						((WS_Base.BaseUnit)Caster).Mana.Current += Damage;
						TargetPower = Unit.Mana.Current;
						break;
					}
                    switch (Unit)
                    {
                        case WS_Creatures.CreatureObject _:
                            {
                                Packets.UpdateClass myTmpUpdate = new Packets.UpdateClass(188);
                                Packets.UpdatePacketClass myPacket = new Packets.UpdatePacketClass();
                                myTmpUpdate.SetUpdateFlag(23 + SpellInfo.MiscValue, TargetPower);
                                Packets.PacketClass packet = myPacket;
                                WS_Creatures.CreatureObject updateObject = (WS_Creatures.CreatureObject)Unit;
                                myTmpUpdate.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
                                myPacket = (Packets.UpdatePacketClass)packet;
                                packet = myPacket;
                                Unit.SendToNearPlayers(ref packet, 0uL);
                                myPacket = (Packets.UpdatePacketClass)packet;
                                myPacket.Dispose();
                                myTmpUpdate.Dispose();
                                break;
                            }

                        case WS_PlayerData.CharacterObject object1:
                            object1.SetUpdateFlag(23 + SpellInfo.MiscValue, TargetPower);
                            object1.SendCharacterUpdate();
                            break;
                    }
                }
				int CasterPower = 0;
				switch (SpellInfo.MiscValue)
				{
				case 0:
					CasterPower = ((WS_Base.BaseUnit)Caster).Mana.Current;
					break;
				case 1:
					if (Caster is WS_PlayerData.CharacterObject @object)
					{
						CasterPower = @object.Rage.Current;
					}
					break;
				case 3:
					if (Caster is WS_PlayerData.CharacterObject object1)
					{
						CasterPower = object1.Energy.Current;
					}
					break;
				default:
					CasterPower = ((WS_Base.BaseUnit)Caster).Mana.Current;
					break;
				}
                switch (Caster)
                {
                    case WS_Creatures.CreatureObject _:
                        {
                            Packets.UpdateClass TmpUpdate = new Packets.UpdateClass(188);
                            Packets.UpdatePacketClass Packet = new Packets.UpdatePacketClass();
                            TmpUpdate.SetUpdateFlag(23 + SpellInfo.MiscValue, CasterPower);
                            Packets.PacketClass packet = Packet;
                            WS_Creatures.CreatureObject updateObject = (WS_Creatures.CreatureObject)Caster;
                            TmpUpdate.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
                            Packet = (Packets.UpdatePacketClass)packet;
                            WS_Base.BaseUnit unitTarget = Target.unitTarget;
                            packet = Packet;
                            unitTarget.SendToNearPlayers(ref packet, 0uL);
                            Packet = (Packets.UpdatePacketClass)packet;
                            Packet.Dispose();
                            TmpUpdate.Dispose();
                            break;
                        }

                    case WS_PlayerData.CharacterObject _:
                        ((WS_PlayerData.CharacterObject)Caster).SetUpdateFlag(23 + SpellInfo.MiscValue, CasterPower);
                        ((WS_PlayerData.CharacterObject)Caster).SendCharacterUpdate();
                        break;
                }
                return SpellFailedReason.SPELL_NO_ERROR;
			}
		}

		public SpellFailedReason SPELL_EFFECT_HEAL(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			int Current = 0;
			foreach (WS_Base.BaseUnit Unit in Infected)
			{
				int Damage = SpellInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
				if (Current > 0)
				{
					Damage = checked((int)Math.Round(Damage * Math.Pow(SpellInfo.DamageMultiplier, Current)));
				}
				WS_Base.BaseUnit Caster2 = (WS_Base.BaseUnit)Caster;
				Unit.DealSpellDamage(ref Caster2, ref SpellInfo, SpellID, Damage, (DamageTypes)checked((byte)SPELLs[SpellID].School), SpellType.SPELL_TYPE_HEAL);
				Caster = Caster2;
				Current = checked(Current + 1);
			}
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_HEAL_MAX_HEALTH(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			int Current = 0;
			foreach (WS_Base.BaseUnit Unit in Infected)
			{
				int Damage = ((WS_Base.BaseUnit)Caster).Life.Maximum;
				if (Current > 0 && SpellInfo.DamageMultiplier < 1f)
				{
					Damage = checked((int)Math.Round(Damage * Math.Pow(SpellInfo.DamageMultiplier, Current)));
				}
				WS_Base.BaseUnit Caster2 = (WS_Base.BaseUnit)Caster;
				Unit.DealSpellDamage(ref Caster2, ref SpellInfo, SpellID, Damage, (DamageTypes)checked((byte)SPELLs[SpellID].School), SpellType.SPELL_TYPE_HEAL);
				Caster = Caster2;
				Current = checked(Current + 1);
			}
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_ENERGIZE(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			int Damage = SpellInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
			foreach (WS_Base.BaseUnit Unit in Infected)
			{
				WS_Base.BaseUnit Caster2 = (WS_Base.BaseUnit)Caster;
				SendEnergizeSpellLog(ref Caster2, ref Target.unitTarget, SpellID, Damage, SpellInfo.MiscValue);
				Caster = Caster2;
				int miscValue = SpellInfo.MiscValue;
				Caster2 = (WS_Base.BaseUnit)Caster;
				Unit.Energize(Damage, (ManaTypes)miscValue, Caster2);
				Caster = Caster2;
			}
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_ENERGIZE_PCT(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			int Damage = 0;
			foreach (WS_Base.BaseUnit Unit in Infected)
			{
				int damage;
				int miscValue;
				WS_Base.BaseUnit Caster2;
				checked
				{
					if (SpellInfo.MiscValue == 0)
					{
						Damage = (int)Math.Round(SpellInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0) / 100.0 * Unit.Mana.Maximum);
					}
					Caster2 = (WS_Base.BaseUnit)Caster;
					SendEnergizeSpellLog(ref Caster2, ref Target.unitTarget, SpellID, Damage, SpellInfo.MiscValue);
					Caster = Caster2;
					damage = Damage;
					miscValue = SpellInfo.MiscValue;
					Caster2 = (WS_Base.BaseUnit)Caster;
				}
				Unit.Energize(damage, (ManaTypes)miscValue, Caster2);
				Caster = Caster2;
			}
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_OPEN_LOCK(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			if (!(Caster is WS_PlayerData.CharacterObject))
			{
				return SpellFailedReason.SPELL_FAILED_ERROR;
			}
			LootType LootType = LootType.LOOTTYPE_CORPSE;
			ulong targetGUID;
			int lockID;
			if (Target.goTarget != null)
			{
				targetGUID = Target.goTarget.GUID;
				lockID = ((WS_GameObjects.GameObjectObject)Target.goTarget).LockID;
			}
			else
			{
				if (Target.itemTarget == null)
				{
					return SpellFailedReason.SPELL_FAILED_BAD_TARGETS;
				}
				targetGUID = Target.itemTarget.GUID;
				lockID = Target.itemTarget.ItemInfo.LockID;
			}
			if (lockID == 0)
			{
				if (WorldServiceLocator._CommonGlobalFunctions.GuidIsGameObject(targetGUID) && WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(targetGUID))
				{
					WS_GameObjects.GameObjectObject gameObjectObject = WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[targetGUID];
					WS_PlayerData.CharacterObject Character = (WS_PlayerData.CharacterObject)Caster;
					gameObjectObject.LootObject(ref Character, LootType);
				}
				return SpellFailedReason.SPELL_NO_ERROR;
			}
			if (!WorldServiceLocator._WS_Loot.Locks.ContainsKey(lockID))
			{
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[DEBUG] Lock {0} did not exist.", lockID);
				return SpellFailedReason.SPELL_FAILED_ERROR;
			}
			byte i = 0;
			do
			{
				if (Item != null && WorldServiceLocator._WS_Loot.Locks[lockID].KeyType[i] == 1 && WorldServiceLocator._WS_Loot.Locks[lockID].Keys[i] == Item.ItemEntry)
				{
					if (WorldServiceLocator._CommonGlobalFunctions.GuidIsGameObject(targetGUID) && WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(targetGUID))
					{
						WS_GameObjects.GameObjectObject gameObjectObject2 = WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[targetGUID];
						WS_PlayerData.CharacterObject Character = (WS_PlayerData.CharacterObject)Caster;
						gameObjectObject2.LootObject(ref Character, LootType);
					}
					return SpellFailedReason.SPELL_NO_ERROR;
				}
				checked
				{
					i = (byte)unchecked((uint)(i + 1));
				}
			}
			while (i <= 4u);
			int SkillID = 0;
			if (SPELLs[SpellID].SpellEffects[1] != null && SPELLs[SpellID].SpellEffects[1].ID == SpellEffects_Names.SPELL_EFFECT_SKILL)
			{
				SkillID = SPELLs[SpellID].SpellEffects[1].MiscValue;
			}
			else if (SPELLs[SpellID].SpellEffects[0] != null && SPELLs[SpellID].SpellEffects[0].MiscValue == 1)
			{
				SkillID = 633;
			}
			short ReqSkillValue = WorldServiceLocator._WS_Loot.Locks[lockID].RequiredMiningSkill;
			if (WorldServiceLocator._WS_Loot.Locks[lockID].RequiredLockingSkill > 0)
			{
				if (SkillID != 633)
				{
					return SpellFailedReason.SPELL_FAILED_FIZZLE;
				}
				ReqSkillValue = WorldServiceLocator._WS_Loot.Locks[lockID].RequiredLockingSkill;
			}
			else if (SkillID == 633)
			{
				return SpellFailedReason.SPELL_FAILED_BAD_TARGETS;
			}
			if (SkillID != 0)
			{
				LootType = LootType.LOOTTYPE_SKINNING;
				if (!((WS_PlayerData.CharacterObject)Caster).Skills.ContainsKey(SkillID) || ((WS_PlayerData.CharacterObject)Caster).Skills[SkillID].Current < ReqSkillValue)
				{
					return SpellFailedReason.SPELL_FAILED_LOW_CASTLEVEL;
				}
			}
			if (WorldServiceLocator._CommonGlobalFunctions.GuidIsGameObject(targetGUID) && WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(targetGUID))
			{
				WS_GameObjects.GameObjectObject gameObjectObject3 = WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[targetGUID];
				WS_PlayerData.CharacterObject Character = (WS_PlayerData.CharacterObject)Caster;
				gameObjectObject3.LootObject(ref Character, LootType);
			}
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_PICKPOCKET(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			if (!(Caster is WS_PlayerData.CharacterObject))
			{
				return SpellFailedReason.SPELL_FAILED_ERROR;
			}
			checked
			{
				if (Target.unitTarget is WS_Creatures.CreatureObject @object && !((WS_Base.BaseUnit)Caster).IsFriendlyTo(ref Target.unitTarget))
				{
					WS_Creatures.CreatureObject creatureObject = @object;
					if (creatureObject.CreatureInfo.CreatureType == 7 || creatureObject.CreatureInfo.CreatureType == 6)
					{
						if (!creatureObject.IsDead)
						{
							int chance = 10 + unchecked(((WS_Base.BaseUnit)Caster).Level) - unchecked(creatureObject.Level);
							if (chance > WorldServiceLocator._WorldServer.Rnd.Next(0, 20))
							{
								if (creatureObject.CreatureInfo.PocketLootID > 0)
								{
									WS_Loot.LootObject Loot = new WS_Loot.LootObject(creatureObject.GUID, LootType.LOOTTYPE_PICKPOCKETING)
									{
										LootOwner = Caster.GUID
									};
									WorldServiceLocator._WS_Loot.LootTemplates_Pickpocketing.GetLoot(creatureObject.CreatureInfo.PocketLootID)?.Process(ref Loot, 0);
									Loot.SendLoot(ref ((WS_PlayerData.CharacterObject)Caster).client);
								}
								else
								{
									WorldServiceLocator._WS_Loot.SendEmptyLoot(creatureObject.GUID, LootType.LOOTTYPE_PICKPOCKETING, ref ((WS_PlayerData.CharacterObject)Caster).client);
								}
							}
							else
							{
								((WS_Base.BaseUnit)Caster).RemoveAurasByInterruptFlag(1024);
								if (creatureObject.aiScript != null)
								{
									WS_Creatures_AI.TBaseAI aiScript = creatureObject.aiScript;
									WS_Base.BaseUnit Attacker = (WS_Base.BaseUnit)Caster;
									aiScript.OnGenerateHate(ref Attacker, 100);
								}
							}
							return SpellFailedReason.SPELL_NO_ERROR;
						}
						return SpellFailedReason.SPELL_FAILED_TARGETS_DEAD;
					}
                }
                return SpellFailedReason.SPELL_FAILED_BAD_TARGETS;
			}
		}

		public SpellFailedReason SPELL_EFFECT_SKINNING(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			if (!(Caster is WS_PlayerData.CharacterObject))
			{
				return SpellFailedReason.SPELL_FAILED_ERROR;
			}
            switch (Target.unitTarget)
            {
                case WS_Creatures.CreatureObject _:
                    {
                        using (WS_Creatures.CreatureObject creatureObject = (WS_Creatures.CreatureObject)Target.unitTarget)
                        {
                            if (creatureObject.IsDead && WorldServiceLocator._Functions.HaveFlags(creatureObject.cUnitFlags, 67108864))
                            {
                                creatureObject.cUnitFlags &= -67108865;
                                if (creatureObject.CreatureInfo.SkinLootID > 0)
                                {
                                    WS_Loot.LootObject Loot = new WS_Loot.LootObject(creatureObject.GUID, LootType.LOOTTYPE_SKINNING)
                                    {
                                        LootOwner = Caster.GUID
                                    };
                                    WorldServiceLocator._WS_Loot.LootTemplates_Skinning.GetLoot(creatureObject.CreatureInfo.SkinLootID)?.Process(ref Loot, 0);
                                    Loot.SendLoot(ref ((WS_PlayerData.CharacterObject)Caster).client);
                                }
                                else
                                {
                                    WorldServiceLocator._WS_Loot.SendEmptyLoot(creatureObject.GUID, LootType.LOOTTYPE_SKINNING, ref ((WS_PlayerData.CharacterObject)Caster).client);
                                }
                                Packets.UpdateClass TmpUpdate = new Packets.UpdateClass(188);
                                Packets.UpdatePacketClass Packet = new Packets.UpdatePacketClass();
                                TmpUpdate.SetUpdateFlag(46, creatureObject.cUnitFlags);
                                Packets.PacketClass packet = Packet;
                                WS_Creatures.CreatureObject updateObject = (WS_Creatures.CreatureObject)Target.unitTarget;
                                TmpUpdate.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
                                Packet = (Packets.UpdatePacketClass)packet;
                                WS_Base.BaseUnit unitTarget = Target.unitTarget;
                                packet = Packet;
                                unitTarget.SendToNearPlayers(ref packet, 0uL);
                                Packet = (Packets.UpdatePacketClass)packet;
                                Packet.Dispose();
                                TmpUpdate.Dispose();
                            }
                        }
                        return SpellFailedReason.SPELL_NO_ERROR;
                    }

                default:
                    return SpellFailedReason.SPELL_FAILED_BAD_TARGETS;
            }
        }

		public SpellFailedReason SPELL_EFFECT_DISENCHANT(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			if (!(Caster is WS_PlayerData.CharacterObject))
			{
				return SpellFailedReason.SPELL_FAILED_ERROR;
			}
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_PROFICIENCY(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			foreach (WS_Base.BaseUnit Unit in Infected)
			{
				if (Unit is WS_PlayerData.CharacterObject @object)
				{
					@object.SendProficiencies();
				}
			}
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_LEARN_SPELL(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			if (SpellInfo.TriggerSpell != 0)
			{
				foreach (WS_Base.BaseUnit Unit in Infected)
				{
					if (Unit is WS_PlayerData.CharacterObject @object)
					{
						@object.LearnSpell(SpellInfo.TriggerSpell);
					}
				}
			}
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_SKILL_STEP(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			if (SpellInfo.MiscValue != 0)
			{
				foreach (WS_Base.BaseUnit Unit in Infected)
				{
					if (Unit is WS_PlayerData.CharacterObject @object)
					{
						@object.LearnSkill(SpellInfo.MiscValue, 1, checked((short)((SpellInfo.valueBase + 1) * 75)));
						@object.SendCharacterUpdate(toNear: false);
					}
				}
			}
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_DISPEL(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			foreach (WS_Base.BaseUnit Unit in Infected)
			{
				if ((Unit.DispellImmunity & (1 << SpellInfo.MiscValue)) == 0)
				{
					Unit.RemoveAurasByDispellType(SpellInfo.MiscValue, SpellInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0));
				}
			}
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_EVADE(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			foreach (WS_Base.BaseUnit Unit in Infected)
			{
			}
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_DODGE(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			checked
			{
				foreach (WS_Base.BaseUnit Unit in Infected)
				{
					if (Unit is WS_PlayerData.CharacterObject @object)
					{
						@object.combatDodge += SpellInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0);
					}
				}
				return SpellFailedReason.SPELL_NO_ERROR;
			}
		}

		public SpellFailedReason SPELL_EFFECT_PARRY(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			checked
			{
				foreach (WS_Base.BaseUnit Unit in Infected)
				{
					if (Unit is WS_PlayerData.CharacterObject @object)
					{
						@object.combatParry += SpellInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0);
					}
				}
				return SpellFailedReason.SPELL_NO_ERROR;
			}
		}

		public SpellFailedReason SPELL_EFFECT_BLOCK(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			checked
			{
				foreach (WS_Base.BaseUnit Unit in Infected)
				{
					if (Unit is WS_PlayerData.CharacterObject @object)
					{
						@object.combatBlock += SpellInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0);
					}
				}
				return SpellFailedReason.SPELL_NO_ERROR;
			}
		}

		public SpellFailedReason SPELL_EFFECT_DUAL_WIELD(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			foreach (WS_Base.BaseUnit Unit in Infected)
			{
				if (Unit is WS_PlayerData.CharacterObject @object)
				{
					@object.spellCanDualWeild = true;
				}
			}
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_WEAPON_DAMAGE(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			bool Ranged = false;
			bool Offhand = false;
			if (SPELLs[SpellID].IsRanged)
			{
				Ranged = true;
			}
			foreach (WS_Base.BaseUnit item in Infected)
			{
				WS_Base.BaseUnit Unit = item;
				WS_Combat wS_Combat = WorldServiceLocator._WS_Combat;
				WS_Base.BaseUnit Attacker = (WS_Base.BaseUnit)Caster;
				WS_Combat.DamageInfo damageInfo2 = wS_Combat.CalculateDamage(ref Attacker, ref Unit, Offhand, Ranged, SPELLs[SpellID], SpellInfo);
				Caster = Attacker;
				WS_Combat.DamageInfo damageInfo = damageInfo2;
				if (((uint)damageInfo.HitInfo & 0x40u) != 0)
				{
					SPELLs[SpellID].SendSpellMiss(ref Caster, ref Unit, SpellMissInfo.SPELL_MISS_RESIST);
					continue;
				}
				if (((uint)damageInfo.HitInfo & 0x10u) != 0)
				{
					SPELLs[SpellID].SendSpellMiss(ref Caster, ref Unit, SpellMissInfo.SPELL_MISS_MISS);
					continue;
				}
				if (((uint)damageInfo.HitInfo & 0x20u) != 0)
				{
					SPELLs[SpellID].SendSpellMiss(ref Caster, ref Unit, SpellMissInfo.SPELL_MISS_ABSORB);
					continue;
				}
				if (((uint)damageInfo.HitInfo & 0x800u) != 0)
				{
					SPELLs[SpellID].SendSpellMiss(ref Caster, ref Unit, SpellMissInfo.SPELL_MISS_BLOCK);
					continue;
				}
				Attacker = (WS_Base.BaseUnit)Caster;
				SendNonMeleeDamageLog(ref Attacker, ref Unit, SpellID, (int)damageInfo.DamageType, damageInfo.GetDamage, damageInfo.Resist, damageInfo.Absorbed, (damageInfo.HitInfo & 0x200) != 0);
				Caster = Attacker;
				WS_Base.BaseUnit baseUnit = Unit;
				int getDamage = damageInfo.GetDamage;
				Attacker = (WS_Base.BaseUnit)Caster;
				baseUnit.DealDamage(getDamage, Attacker);
				Caster = Attacker;
			}
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_WEAPON_DAMAGE_NOSCHOOL(ref SpellTargets target, ref WS_Base.BaseObject caster, ref SpellEffect spellInfo, int spellId, ref List<WS_Base.BaseObject> infected, ref ItemObject item)
		{
			foreach (WS_Base.BaseUnit unit in infected)
			{
				if (caster is WS_PlayerData.CharacterObject character && unit is WS_Base.BaseObject victim && caster is WS_Base.BaseUnit casterUnit)
				{
					character.attackState.DoMeleeDamageBySpell(ref character, ref victim, spellInfo.GetValue(casterUnit.Level, 0), spellId);
				}
			}

			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_ADICIONAL_DMG(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			bool Ranged = false;
			bool Offhand = false;
			if (SPELLs[SpellID].IsRanged)
			{
				Ranged = true;
			}
			foreach (WS_Base.BaseUnit item in Infected)
			{
				WS_Base.BaseUnit Unit = item;
				WS_Combat wS_Combat = WorldServiceLocator._WS_Combat;
				WS_Base.BaseUnit Attacker = (WS_Base.BaseUnit)Caster;
				WS_Combat.DamageInfo damageInfo2 = wS_Combat.CalculateDamage(ref Attacker, ref Unit, Offhand, Ranged, SPELLs[SpellID], SpellInfo);
				Caster = Attacker;
				WS_Combat.DamageInfo damageInfo = damageInfo2;
				Attacker = (WS_Base.BaseUnit)Caster;
				SendNonMeleeDamageLog(ref Attacker, ref Unit, SpellID, (int)damageInfo.DamageType, damageInfo.GetDamage, damageInfo.Resist, damageInfo.Absorbed, (damageInfo.HitInfo & 0x200) != 0);
				Caster = Attacker;
				WS_Base.BaseUnit baseUnit = Unit;
				int getDamage = damageInfo.GetDamage;
				Attacker = (WS_Base.BaseUnit)Caster;
				baseUnit.DealDamage(getDamage, Attacker);
				Caster = Attacker;
			}
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_HONOR(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			checked
			{
				foreach (WS_Base.BaseUnit Unit in Infected)
				{
					if (Unit is WS_PlayerData.CharacterObject @object)
					{
						@object.HonorPoints += SpellInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0);
						if (@object.HonorPoints > 75000)
						{
							@object.HonorPoints = 75000;
						}
						@object.HonorSave();
					}
				}
				return SpellFailedReason.SPELL_NO_ERROR;
			}
		}

		public SpellFailedReason ApplyAura(ref WS_Base.BaseUnit auraTarget, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID)
		{
			checked
			{
				try
				{
					int spellCasted = -1;
					do
					{
						int AuraStart = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1;
						int AuraEnd = 0;
						if (SPELLs[SpellID].IsPassive)
						{
							AuraStart = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs - 1;
							AuraEnd = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE;
						}
						int Duration = SPELLs[SpellID].GetDuration;
						if (SpellID == 15007)
						{
							switch (auraTarget.Level)
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
								Duration = 0;
								break;
							default:
								Duration = 600000;
								break;
							case 11:
							case 12:
							case 13:
							case 14:
							case 15:
							case 16:
							case 17:
							case 18:
							case 19:
								Duration = (unchecked(auraTarget.Level) - 10) * 60 * 1000;
								break;
							}
						}
						int num = AuraStart;
						int num2 = AuraEnd;
						for (int i = num; i >= num2; i += -1)
						{
							if (auraTarget.ActiveSpells[i] == null || auraTarget.ActiveSpells[i].SpellID != SpellID)
							{
								continue;
							}
							spellCasted = i;
							if ((auraTarget.ActiveSpells[i].Aura_Info[0] != null && auraTarget.ActiveSpells[i].Aura_Info[0] == SpellInfo) || (auraTarget.ActiveSpells[i].Aura_Info[1] != null && auraTarget.ActiveSpells[i].Aura_Info[1] == SpellInfo) || (auraTarget.ActiveSpells[i].Aura_Info[2] != null && auraTarget.ActiveSpells[i].Aura_Info[2] == SpellInfo))
							{
								if (auraTarget.ActiveSpells[i].Aura_Info[0] != null && auraTarget.ActiveSpells[i].Aura_Info[0] == SpellInfo)
								{
									auraTarget.ActiveSpells[i].SpellDuration = Duration;
									if (SPELLs[SpellID].maxStack > 0 && auraTarget.ActiveSpells[i].StackCount < SPELLs[SpellID].maxStack)
									{
										auraTarget.ActiveSpells[i].StackCount++;
										AURAs[SpellInfo.ApplyAuraIndex](ref auraTarget, ref Caster, ref SpellInfo, SpellID, 1, AuraAction.AURA_ADD);
                                        switch (auraTarget)
                                        {
                                            case WS_PlayerData.CharacterObject _:
                                                ((WS_PlayerData.CharacterObject)auraTarget).GroupUpdateFlag = ((WS_PlayerData.CharacterObject)auraTarget).GroupUpdateFlag | 0x200u;
                                                break;
                                            case WS_Pets.PetObject _ when ((WS_Pets.PetObject)auraTarget).Owner is WS_PlayerData.CharacterObject @object:
                                                @object.GroupUpdateFlag |= 0x40000u;
                                                break;
                                        }
                                    }
									auraTarget.UpdateAura(i);
								}
								return SpellFailedReason.SPELL_NO_ERROR;
							}
							unchecked
							{
								if (auraTarget.ActiveSpells[i].Aura[0] == null)
								{
									auraTarget.ActiveSpells[i].Aura[0] = AURAs[SpellInfo.ApplyAuraIndex];
									auraTarget.ActiveSpells[i].Aura_Info[0] = SpellInfo;
									WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "APPLYING AURA {0}", (AuraEffects_Names)SpellInfo.ApplyAuraIndex);
									break;
								}
								if (auraTarget.ActiveSpells[i].Aura[1] == null)
								{
									auraTarget.ActiveSpells[i].Aura[1] = AURAs[SpellInfo.ApplyAuraIndex];
									auraTarget.ActiveSpells[i].Aura_Info[1] = SpellInfo;
									WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "APPLYING AURA {0}", (AuraEffects_Names)SpellInfo.ApplyAuraIndex);
									break;
								}
								if (auraTarget.ActiveSpells[i].Aura[2] == null)
								{
									auraTarget.ActiveSpells[i].Aura[2] = AURAs[SpellInfo.ApplyAuraIndex];
									auraTarget.ActiveSpells[i].Aura_Info[2] = SpellInfo;
									WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "APPLYING AURA {0}", (AuraEffects_Names)SpellInfo.ApplyAuraIndex);
									break;
								}
								spellCasted = int.MaxValue;
							}
						}
						if (spellCasted == -1)
						{
							WS_Base.BaseUnit obj = auraTarget;
							int duration = Duration;
							WS_Base.BaseUnit Caster2 = (WS_Base.BaseUnit)Caster;
							obj.AddAura(SpellID, duration, ref Caster2);
							Caster = Caster2;
						}
						if (spellCasted == -2)
						{
							spellCasted = int.MaxValue;
						}
						if (spellCasted < 0)
						{
							spellCasted--;
						}
					}
					while (spellCasted < 0);
					if (spellCasted == int.MaxValue)
					{
						return SpellFailedReason.SPELL_FAILED_TRY_AGAIN;
					}
					AURAs[SpellInfo.ApplyAuraIndex](ref auraTarget, ref Caster, ref SpellInfo, SpellID, 1, AuraAction.AURA_ADD);
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception e = ex;
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error while applying aura for spell {0}:{1}", SpellID, Environment.NewLine + e.ToString());
					ProjectData.ClearProjectError();
				}
				return SpellFailedReason.SPELL_NO_ERROR;
			}
		}

		public SpellFailedReason SPELL_EFFECT_APPLY_AURA(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			if ((((uint)Target.targetMask & 2u) != 0 || Target.targetMask == 0) && Target.unitTarget == null)
			{
				return SpellFailedReason.SPELL_FAILED_BAD_IMPLICIT_TARGETS;
			}
			SpellFailedReason result = SpellFailedReason.SPELL_NO_ERROR;
			if (Caster is WS_PlayerData.CharacterObject @object && ((uint)SPELLs[SpellID].auraInterruptFlags & 0x40000u) != 0)
			{
				((WS_Base.BaseUnit)Caster).StandState = 1;
				@object.SetUpdateFlag(138, ((WS_Base.BaseUnit)Caster).cBytes1);
				@object.SendCharacterUpdate();
				Packets.PacketClass packetACK = new Packets.PacketClass(Opcodes.SMSG_STANDSTATE_CHANGE_ACK);
				packetACK.AddInt8(((WS_Base.BaseUnit)Caster).StandState);
				@object.client.Send(ref packetACK);
				packetACK.Dispose();
			}
			if (((uint)Target.targetMask & 2u) != 0 || Target.targetMask == 0)
			{
				int count = SPELLs[SpellID].MaxTargets;
				foreach (WS_Base.BaseUnit item in Infected)
				{
					WS_Base.BaseUnit u = item;
					ApplyAura(ref u, ref Caster, ref SpellInfo, SpellID);
					count = checked(count - 1);
					if (count <= 0 && SPELLs[SpellID].MaxTargets > 0)
					{
						break;
					}
				}
			}
			else if (((uint)Target.targetMask & 0x40u) != 0)
			{
				WS_DynamicObjects.DynamicObjectObject[] array = ((WS_Base.BaseUnit)Caster).dynamicObjects.ToArray();
				foreach (WS_DynamicObjects.DynamicObjectObject dynamic in array)
				{
					if (dynamic.SpellID == SpellID)
					{
						dynamic.AddEffect(SpellInfo);
						break;
					}
				}
			}
			return result;
		}

		public SpellFailedReason SPELL_EFFECT_APPLY_AREA_AURA(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			foreach (WS_Base.BaseUnit item in Infected)
			{
				WS_Base.BaseUnit u = item;
				ApplyAura(ref u, ref Caster, ref SpellInfo, SpellID);
			}
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_PERSISTENT_AREA_AURA(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			if ((Target.targetMask & 0x40) == 0)
			{
				return SpellFailedReason.SPELL_FAILED_BAD_IMPLICIT_TARGETS;
			}
			WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Amplitude: {0}", SpellInfo.Amplitude);
			WS_Base.BaseUnit Caster_ = (WS_Base.BaseUnit)Caster;
			WS_DynamicObjects.DynamicObjectObject dynamicObjectObject = new WS_DynamicObjects.DynamicObjectObject(ref Caster_, SpellID, Target.dstX, Target.dstY, Target.dstZ, SPELLs[SpellID].GetDuration, SpellInfo.GetRadius);
			Caster = Caster_;
			WS_DynamicObjects.DynamicObjectObject tmpDO = dynamicObjectObject;
			tmpDO.AddEffect(SpellInfo);
			tmpDO.Bytes = 32435950;
			((WS_Base.BaseUnit)Caster).dynamicObjects.Add(tmpDO);
			tmpDO.Spawn();
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_CREATE_ITEM(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			if (!(Target.unitTarget is WS_PlayerData.CharacterObject))
			{
				return SpellFailedReason.SPELL_FAILED_BAD_TARGETS;
			}
			checked
			{
				int Amount = SpellInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level) - SPELLs[SpellID].spellLevel, 0);
				if (Amount < 0)
				{
					return SpellFailedReason.SPELL_FAILED_ERROR;
				}
				if (!WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(SpellInfo.ItemType))
				{
					WS_Items.ItemInfo tmpInfo = new WS_Items.ItemInfo(SpellInfo.ItemType);
					WorldServiceLocator._WorldServer.ITEMDatabase.Add(SpellInfo.ItemType, tmpInfo);
				}
				if (Amount > WorldServiceLocator._WorldServer.ITEMDatabase[SpellInfo.ItemType].Stackable)
				{
					Amount = WorldServiceLocator._WorldServer.ITEMDatabase[SpellInfo.ItemType].Stackable;
				}
				WS_Base.BaseUnit objCharacter = (WS_Base.BaseUnit)Caster;
				List<WS_Base.BaseUnit> friendPlayersAroundMe = GetFriendPlayersAroundMe(ref objCharacter, SpellInfo.GetRadius);
				Caster = objCharacter;
                foreach (WS_Base.BaseUnit Unit in Infected)
				{
					if (Unit is WS_PlayerData.CharacterObject @object)
					{
						ItemObject tmpItem = new ItemObject(SpellInfo.ItemType, Unit.GUID)
						{
							StackCount = Amount
						};
						if (!@object.ItemADD(ref tmpItem))
						{
							tmpItem.Delete();
						}
						else
						{
							((WS_PlayerData.CharacterObject)Target.unitTarget).LogLootItem(tmpItem, (byte)tmpItem.StackCount, Recieved: false, Created: true);
						}
					}
				}
				return SpellFailedReason.SPELL_NO_ERROR;
			}
		}

		public SpellFailedReason SPELL_EFFECT_RESURRECT(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			foreach (WS_Base.BaseObject Unit in Infected)
			{
                switch (Unit)
                {
                    case WS_PlayerData.CharacterObject _:
                        {
                            if (decimal.Compare(new decimal(((WS_PlayerData.CharacterObject)Unit).resurrectGUID), 0m) != 0)
                            {
                                if (Caster is WS_PlayerData.CharacterObject @object)
                                {
                                    Packets.PacketClass RessurectFailed = new Packets.PacketClass(Opcodes.SMSG_RESURRECT_FAILED);
                                    @object.client.Send(ref RessurectFailed);
                                    RessurectFailed.Dispose();
                                }
                                return SpellFailedReason.SPELL_NO_ERROR;
                            }
                                        ((WS_PlayerData.CharacterObject)Unit).resurrectGUID = Caster.GUID;
                            ((WS_PlayerData.CharacterObject)Unit).resurrectMapID = checked((int)Caster.MapID);
                            ((WS_PlayerData.CharacterObject)Unit).resurrectPositionX = Caster.positionX;
                            ((WS_PlayerData.CharacterObject)Unit).resurrectPositionY = Caster.positionY;
                            ((WS_PlayerData.CharacterObject)Unit).resurrectPositionZ = Caster.positionZ;
                            ((WS_PlayerData.CharacterObject)Unit).resurrectHealth = checked(((WS_PlayerData.CharacterObject)Unit).Life.Maximum * SpellInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0)) / 100;
                            ((WS_PlayerData.CharacterObject)Unit).resurrectMana = checked(((WS_PlayerData.CharacterObject)Unit).Mana.Maximum * SpellInfo.MiscValue) / 100;
                            Packets.PacketClass RessurectRequest2 = new Packets.PacketClass(Opcodes.SMSG_RESURRECT_REQUEST);
                            RessurectRequest2.AddUInt64(Caster.GUID);
                            RessurectRequest2.AddUInt32(1u);
                            RessurectRequest2.AddUInt16(0);
                            RessurectRequest2.AddUInt32(1u);
                            ((WS_PlayerData.CharacterObject)Unit).client.Send(ref RessurectRequest2);
                            RessurectRequest2.Dispose();
                            break;
                        }

                    case WS_Creatures.CreatureObject _:
                        {
                            Target.unitTarget.Life.Current = checked(((WS_Creatures.CreatureObject)Unit).Life.Maximum * SpellInfo.valueBase) / 100;
                            Target.unitTarget.cUnitFlags &= -16385;
                            Packets.UpdatePacketClass packetForNear = new Packets.UpdatePacketClass();
                            Packets.UpdateClass UpdateData = new Packets.UpdateClass(188);
                            UpdateData.SetUpdateFlag(22, ((WS_Creatures.CreatureObject)Unit).Life.Current);
                            UpdateData.SetUpdateFlag(46, Target.unitTarget.cUnitFlags);
                            Packets.PacketClass packet = packetForNear;
                            WS_Creatures.CreatureObject updateObject = (WS_Creatures.CreatureObject)Unit;
                            UpdateData.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
                            packetForNear = (Packets.UpdatePacketClass)packet;
                            WS_Creatures.CreatureObject obj = (WS_Creatures.CreatureObject)Unit;
                            packet = packetForNear;
                            obj.SendToNearPlayers(ref packet, 0uL);
                            packetForNear = (Packets.UpdatePacketClass)packet;
                            packetForNear.Dispose();
                            UpdateData.Dispose();
                            ((WS_Creatures.CreatureObject)Target.unitTarget).MoveToInstant(Caster.positionX, Caster.positionY, Caster.positionZ, Caster.orientation);
                            break;
                        }

                    case WS_Corpses.CorpseObject _ when WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(((WS_Corpses.CorpseObject)Unit).Owner):
                        {
                            WS_PlayerData.CharacterObject characterObject = WorldServiceLocator._WorldServer.CHARACTERs[((WS_Corpses.CorpseObject)Unit).Owner];
                            characterObject.resurrectGUID = Caster.GUID;
                            characterObject.resurrectMapID = checked((int)Caster.MapID);
                            characterObject.resurrectPositionX = Caster.positionX;
                            characterObject.resurrectPositionY = Caster.positionY;
                            characterObject.resurrectPositionZ = Caster.positionZ;
                            characterObject.resurrectHealth = checked(characterObject.Life.Maximum * SpellInfo.valueBase) / 100;
                            characterObject.resurrectMana = checked(characterObject.Mana.Maximum * SpellInfo.MiscValue) / 100;
                            Packets.PacketClass RessurectRequest = new Packets.PacketClass(Opcodes.SMSG_RESURRECT_REQUEST);
                            RessurectRequest.AddUInt64(Caster.GUID);
                            RessurectRequest.AddUInt32(1u);
                            RessurectRequest.AddUInt16(0);
                            RessurectRequest.AddUInt32(1u);
                            characterObject.client.Send(ref RessurectRequest);
                            RessurectRequest.Dispose();
                            break;
                        }
                }
            }
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_RESURRECT_NEW(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			foreach (WS_Base.BaseObject Unit in Infected)
			{
                switch (Unit)
                {
                    case WS_PlayerData.CharacterObject _:
                        {
                            if (decimal.Compare(new decimal(((WS_PlayerData.CharacterObject)Unit).resurrectGUID), 0m) != 0)
                            {
                                if (Caster is WS_PlayerData.CharacterObject @object)
                                {
                                    Packets.PacketClass RessurectFailed = new Packets.PacketClass(Opcodes.SMSG_RESURRECT_FAILED);
                                    @object.client.Send(ref RessurectFailed);
                                    RessurectFailed.Dispose();
                                }
                                return SpellFailedReason.SPELL_NO_ERROR;
                            }
                                        ((WS_PlayerData.CharacterObject)Unit).resurrectGUID = Caster.GUID;
                            ((WS_PlayerData.CharacterObject)Unit).resurrectMapID = checked((int)Caster.MapID);
                            ((WS_PlayerData.CharacterObject)Unit).resurrectPositionX = Caster.positionX;
                            ((WS_PlayerData.CharacterObject)Unit).resurrectPositionY = Caster.positionY;
                            ((WS_PlayerData.CharacterObject)Unit).resurrectPositionZ = Caster.positionZ;
                            ((WS_PlayerData.CharacterObject)Unit).resurrectHealth = SpellInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                            ((WS_PlayerData.CharacterObject)Unit).resurrectMana = SpellInfo.MiscValue;
                            Packets.PacketClass RessurectRequest2 = new Packets.PacketClass(Opcodes.SMSG_RESURRECT_REQUEST);
                            RessurectRequest2.AddUInt64(Caster.GUID);
                            RessurectRequest2.AddUInt32(1u);
                            RessurectRequest2.AddUInt16(0);
                            RessurectRequest2.AddUInt32(1u);
                            ((WS_PlayerData.CharacterObject)Unit).client.Send(ref RessurectRequest2);
                            RessurectRequest2.Dispose();
                            break;
                        }

                    case WS_Creatures.CreatureObject _:
                        {
                            ((WS_Creatures.CreatureObject)Unit).Life.Current = checked(((WS_Creatures.CreatureObject)Unit).Life.Maximum * SpellInfo.valueBase) / 100;
                            Packets.UpdatePacketClass packetForNear = new Packets.UpdatePacketClass();
                            Packets.UpdateClass UpdateData = new Packets.UpdateClass(188);
                            UpdateData.SetUpdateFlag(22, ((WS_Creatures.CreatureObject)Unit).Life.Current);
                            Packets.PacketClass packet = packetForNear;
                            WS_Creatures.CreatureObject updateObject = (WS_Creatures.CreatureObject)Unit;
                            UpdateData.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
                            packetForNear = (Packets.UpdatePacketClass)packet;
                            WS_Creatures.CreatureObject obj = (WS_Creatures.CreatureObject)Unit;
                            packet = packetForNear;
                            obj.SendToNearPlayers(ref packet, 0uL);
                            packetForNear = (Packets.UpdatePacketClass)packet;
                            packetForNear.Dispose();
                            UpdateData.Dispose();
                            ((WS_Creatures.CreatureObject)Target.unitTarget).MoveToInstant(Caster.positionX, Caster.positionY, Caster.positionZ, Caster.orientation);
                            break;
                        }

                    case WS_Corpses.CorpseObject _ when WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(((WS_Corpses.CorpseObject)Unit).Owner):
                        {
                            WorldServiceLocator._WorldServer.CHARACTERs[((WS_Corpses.CorpseObject)Unit).Owner].resurrectGUID = Caster.GUID;
                            WorldServiceLocator._WorldServer.CHARACTERs[((WS_Corpses.CorpseObject)Unit).Owner].resurrectMapID = checked((int)Caster.MapID);
                            WorldServiceLocator._WorldServer.CHARACTERs[((WS_Corpses.CorpseObject)Unit).Owner].resurrectPositionX = Caster.positionX;
                            WorldServiceLocator._WorldServer.CHARACTERs[((WS_Corpses.CorpseObject)Unit).Owner].resurrectPositionY = Caster.positionY;
                            WorldServiceLocator._WorldServer.CHARACTERs[((WS_Corpses.CorpseObject)Unit).Owner].resurrectPositionZ = Caster.positionZ;
                            WorldServiceLocator._WorldServer.CHARACTERs[((WS_Corpses.CorpseObject)Unit).Owner].resurrectHealth = SpellInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                            WorldServiceLocator._WorldServer.CHARACTERs[((WS_Corpses.CorpseObject)Unit).Owner].resurrectMana = SpellInfo.MiscValue;
                            Packets.PacketClass RessurectRequest = new Packets.PacketClass(Opcodes.SMSG_RESURRECT_REQUEST);
                            RessurectRequest.AddUInt64(Caster.GUID);
                            RessurectRequest.AddUInt32(1u);
                            RessurectRequest.AddUInt16(0);
                            RessurectRequest.AddUInt32(1u);
                            WorldServiceLocator._WorldServer.CHARACTERs[((WS_Corpses.CorpseObject)Unit).Owner].client.Send(ref RessurectRequest);
                            RessurectRequest.Dispose();
                            break;
                        }
                }
            }
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_TELEPORT_GRAVEYARD(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			foreach (WS_Base.BaseUnit Unit in Infected)
			{
				if (Unit is WS_PlayerData.CharacterObject @object)
				{
					WS_GraveYards allGraveYards = WorldServiceLocator._WorldServer.AllGraveYards;
					WS_PlayerData.CharacterObject Character = @object;
					allGraveYards.GoToNearestGraveyard(ref Character, Alive: false, Teleport: true);
				}
			}
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_INTERRUPT_CAST(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			foreach (WS_Base.BaseUnit Unit in Infected)
			{
                switch (Unit)
                {
                    case WS_PlayerData.CharacterObject _:
                        if (((WS_PlayerData.CharacterObject)Unit).FinishAllSpells())
                        {
                            ((WS_PlayerData.CharacterObject)Unit).ProhibitSpellSchool(SPELLs[SpellID].School, SPELLs[SpellID].GetDuration);
                        }

                        break;
                    case WS_Creatures.CreatureObject _:
                        ((WS_Creatures.CreatureObject)Unit).StopCasting();
                        break;
                }
            }
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_STEALTH(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			foreach (WS_Base.BaseUnit Unit in Infected)
			{
                Globals.Functions functions = WorldServiceLocator._Functions;
				ref int cBytes = ref Unit.cBytes1;
				checked
				{
					uint value = (uint)cBytes;
					functions.SetFlag(ref value, 25, flagValue: true);
					cBytes = (int)value;
					Unit.Invisibility = InvisibilityLevel.INIVISIBILITY;
				}
				Unit.Invisibility_Value = SpellInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
				if (Unit is WS_PlayerData.CharacterObject @object)
				{
					WS_CharMovement wS_CharMovement = WorldServiceLocator._WS_CharMovement;
					WS_PlayerData.CharacterObject Character = @object;
					wS_CharMovement.UpdateCell(ref Character);
				}
			}
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_DETECT(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			foreach (WS_Base.BaseUnit Unit in Infected)
			{
				Unit.CanSeeInvisibility = InvisibilityLevel.INIVISIBILITY;
				Unit.CanSeeInvisibility_Stealth = SpellInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
				if (Unit is WS_PlayerData.CharacterObject @object)
				{
					WS_CharMovement wS_CharMovement = WorldServiceLocator._WS_CharMovement;
					WS_PlayerData.CharacterObject Character = @object;
					wS_CharMovement.UpdateCell(ref Character);
				}
			}
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_LEAP(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			float selectedX = (float)(Caster.positionX + Math.Cos(Caster.orientation) * SpellInfo.GetRadius);
			float selectedY = (float)(Caster.positionY + Math.Sin(Caster.orientation) * SpellInfo.GetRadius);
			float selectedZ = WorldServiceLocator._WS_Maps.GetZCoord(selectedX, selectedY, Caster.positionZ, Caster.MapID);
			if (Math.Abs(Caster.positionZ - selectedZ) > SpellInfo.GetRadius)
			{
				selectedX = Caster.positionX;
				selectedY = Caster.positionY;
				selectedZ = Caster.positionZ - SpellInfo.GetRadius;
			}
			float hitX = default;
			float hitY = default;
			float hitZ = default;
			if (WorldServiceLocator._WS_Maps.GetObjectHitPos(ref Caster, selectedX, selectedY, selectedZ + 2f, ref hitX, ref hitY, ref hitZ, -1f))
			{
				selectedX = hitX;
				selectedY = hitY;
				selectedZ = hitZ + 0.2f;
			}
            switch (Caster)
            {
                case WS_PlayerData.CharacterObject _:
                    ((WS_PlayerData.CharacterObject)Caster).Teleport(selectedX, selectedY, selectedZ, Caster.orientation, checked((int)Caster.MapID));
                    break;
                default:
                    ((WS_Creatures.CreatureObject)Caster).MoveToInstant(selectedX, selectedY, selectedZ, Caster.orientation);
                    break;
            }
            return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_SUMMON(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_SUMMON_WILD(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			int Duration = SPELLs[SpellID].GetDuration;
			if (Duration == 0)
			{
				float SelectedX;
				float SelectedY;
				float SelectedZ;
				if (((uint)Target.targetMask & 0x40u) != 0)
				{
					SelectedX = Target.dstX;
					SelectedY = Target.dstY;
					SelectedZ = Target.dstZ;
				}
				else
				{
					SelectedX = Caster.positionX;
					SelectedY = Caster.positionY;
					SelectedZ = Caster.positionZ;
				}
                WS_Creatures.CreatureObject creatureObject = new WS_Creatures.CreatureObject(SpellInfo.MiscValue, SelectedX, SelectedY, SelectedZ, Caster.orientation, checked((int)Caster.MapID), Duration)
                {
                    Level = ((WS_Base.BaseUnit)Caster).Level,
                    CreatedBy = Caster.GUID,
                    CreatedBySpell = SpellID
                };
                WS_Creatures.CreatureObject tmpCreature = creatureObject;
				tmpCreature.AddToWorld();
				return SpellFailedReason.SPELL_NO_ERROR;
			}
			SpellFailedReason SPELL_EFFECT_SUMMON_WILD = default;
			return SPELL_EFFECT_SUMMON_WILD;
		}

		public SpellFailedReason SPELL_EFFECT_SUMMON_TOTEM(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			checked
			{
				byte Slot;
				float angle;
				float selectedX;
				float selectedY;
				float selectedZ;
				WS_Totems.TotemObject NewTotem;
				switch (SpellInfo.ID)
				{
				case SpellEffects_Names.SPELL_EFFECT_SUMMON_TOTEM_SLOT1:
					Slot = 0;
					goto IL_0041;
				case SpellEffects_Names.SPELL_EFFECT_SUMMON_TOTEM_SLOT2:
					Slot = 1;
					goto IL_0041;
				case SpellEffects_Names.SPELL_EFFECT_SUMMON_TOTEM_SLOT3:
					Slot = 2;
					goto IL_0041;
				case SpellEffects_Names.SPELL_EFFECT_SUMMON_TOTEM_SLOT4:
					Slot = 3;
					goto IL_0041;
				default:
					{
						SpellFailedReason SPELL_EFFECT_SUMMON_TOTEM = default;
						return SPELL_EFFECT_SUMMON_TOTEM;
					}
					IL_0041:
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[DEBUG] Totem Slot [{0}].", Slot);
					if (Slot < 4)
					{
						ulong GUID = ((WS_PlayerData.CharacterObject)Caster).TotemSlot[Slot];
						if (decimal.Compare(new decimal(GUID), 0m) != 0 && WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(GUID))
						{
							WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[DEBUG] Destroyed old totem.");
							WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].Destroy();
						}
					}
					angle = 0f;
					if (Slot < 4)
					{
						angle = (float)(Math.PI / 4.0 - unchecked(Slot) * 2 * Math.PI / 4.0);
					}
					selectedX = (float)(Caster.positionX + Math.Cos(Caster.orientation) * 2.0);
					selectedY = (float)(Caster.positionY + Math.Sin(Caster.orientation) * 2.0);
					selectedZ = WorldServiceLocator._WS_Maps.GetZCoord(selectedX, selectedY, Caster.positionZ, Caster.MapID);
					NewTotem = new WS_Totems.TotemObject(SpellInfo.MiscValue, selectedX, selectedY, selectedZ, angle, (int)Caster.MapID, SPELLs[SpellID].GetDuration);
					NewTotem.Life.Base = SpellInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0);
					NewTotem.Life.Current = NewTotem.Life.Maximum;
					NewTotem.Caster = (WS_Base.BaseUnit)Caster;
					NewTotem.Level = ((WS_Base.BaseUnit)Caster).Level;
					NewTotem.SummonedBy = Caster.GUID;
					NewTotem.CreatedBy = Caster.GUID;
					NewTotem.CreatedBySpell = SpellID;
					if (Caster is WS_PlayerData.CharacterObject @object)
					{
						NewTotem.Faction = @object.Faction;
					}
					else if (Caster is WS_Creatures.CreatureObject object2)
					{
						NewTotem.Faction = object2.Faction;
					}
					if (!WorldServiceLocator._WorldServer.CREATURESDatabase.ContainsKey(SpellInfo.MiscValue))
					{
						CreatureInfo tmpInfo = new CreatureInfo(SpellInfo.MiscValue);
						WorldServiceLocator._WorldServer.CREATURESDatabase.Add(SpellInfo.MiscValue, tmpInfo);
					}
					NewTotem.InitSpell(WorldServiceLocator._WorldServer.CREATURESDatabase[SpellInfo.MiscValue].Spells[0]);
					NewTotem.AddToWorld();
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[DEBUG] Totem spawned [{0:X}].", NewTotem.GUID);
					if (Slot < 4 && Caster is WS_PlayerData.CharacterObject object1)
					{
						object1.TotemSlot[Slot] = NewTotem.GUID;
					}
					return SpellFailedReason.SPELL_NO_ERROR;
				}
			}
		}

		public SpellFailedReason SPELL_EFFECT_SUMMON_OBJECT(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			if (!(Caster is WS_Base.BaseUnit))
			{
				return SpellFailedReason.SPELL_FAILED_CASTER_DEAD;
			}
			float selectedX;
			float selectedY;
			if (SpellInfo.RadiusIndex > 0)
			{
				selectedX = (float)(Caster.positionX + Math.Cos(Caster.orientation) * SpellInfo.GetRadius);
				selectedY = (float)(Caster.positionY + Math.Sin(Caster.orientation) * SpellInfo.GetRadius);
			}
			else
			{
				selectedX = (float)(Caster.positionX + Math.Cos(Caster.orientation) * SPELLs[SpellID].GetRange);
				selectedY = (float)(Caster.positionY + Math.Sin(Caster.orientation) * SPELLs[SpellID].GetRange);
			}
			WS_GameObjects.GameObjectInfo GameobjectInfo = (WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase.ContainsKey(SpellInfo.MiscValue) ? WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase[SpellInfo.MiscValue] : new WS_GameObjects.GameObjectInfo(SpellInfo.MiscValue));
            WS_GameObjects.GameObjectObject gameObjectObject = new WS_GameObjects.GameObjectObject(PosZ: (GameobjectInfo.Type != GameObjectType.GAMEOBJECT_TYPE_FISHINGNODE) ? WorldServiceLocator._WS_Maps.GetZCoord(selectedX, selectedY, Caster.positionZ, Caster.MapID) : WorldServiceLocator._WS_Maps.GetWaterLevel(selectedX, selectedY, checked((int)Caster.MapID)), ID_: SpellInfo.MiscValue, MapID_: Caster.MapID, PosX: selectedX, PosY: selectedY, Rotation: Caster.orientation, Owner_: Caster.GUID)
            {
                CreatedBySpell = SpellID,
                Level = ((WS_Base.BaseUnit)Caster).Level,
                instance = Caster.instance
            };
            WS_GameObjects.GameObjectObject tmpGO = gameObjectObject;
			((WS_Base.BaseUnit)Caster).gameObjects.Add(tmpGO);
			if (GameobjectInfo.Type == GameObjectType.GAMEOBJECT_TYPE_FISHINGNODE)
			{
				tmpGO.SetupFishingNode();
			}
			tmpGO.AddToWorld();
			Packets.PacketClass packet = new Packets.PacketClass(Opcodes.SMSG_GAMEOBJECT_SPAWN_ANIM);
			packet.AddUInt64(tmpGO.GUID);
			tmpGO.SendToNearPlayers(ref packet, 0uL);
			packet.Dispose();
			if (GameobjectInfo.Type == GameObjectType.GAMEOBJECT_TYPE_FISHINGNODE)
			{
				((WS_PlayerData.CharacterObject)Caster).SetUpdateFlag(144, SpellID);
				((WS_PlayerData.CharacterObject)Caster).SetUpdateFlag(20, tmpGO.GUID);
				((WS_PlayerData.CharacterObject)Caster).SendCharacterUpdate();
			}
			if (SPELLs[SpellID].GetDuration > 0)
			{
				tmpGO.Despawn(SPELLs[SpellID].GetDuration);
			}
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_ENCHANT_ITEM(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			if (Target.itemTarget == null)
			{
				return SpellFailedReason.SPELL_FAILED_ITEM_NOT_FOUND;
			}
			Target.itemTarget.AddEnchantment(SpellInfo.MiscValue, 0);
			if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(Target.itemTarget.OwnerGUID))
			{
				WorldServiceLocator._WorldServer.CHARACTERs[Target.itemTarget.OwnerGUID].SendItemUpdate(Target.itemTarget);
				Packets.PacketClass EnchantLog = new Packets.PacketClass(Opcodes.SMSG_ENCHANTMENTLOG);
				EnchantLog.AddUInt64(Target.itemTarget.OwnerGUID);
				EnchantLog.AddUInt64(Caster.GUID);
				EnchantLog.AddInt32(Target.itemTarget.ItemEntry);
				EnchantLog.AddInt32(SpellInfo.MiscValue);
				EnchantLog.AddInt8(0);
				WorldServiceLocator._WorldServer.CHARACTERs[Target.itemTarget.OwnerGUID].client.Send(ref EnchantLog);
				if (WorldServiceLocator._WorldServer.CHARACTERs[Target.itemTarget.OwnerGUID].tradeInfo != null)
				{
					if (WorldServiceLocator._WorldServer.CHARACTERs[Target.itemTarget.OwnerGUID].tradeInfo.Trader == WorldServiceLocator._WorldServer.CHARACTERs[Target.itemTarget.OwnerGUID])
					{
						WorldServiceLocator._WorldServer.CHARACTERs[Target.itemTarget.OwnerGUID].tradeInfo.SendTradeUpdateToTarget();
					}
					else
					{
						WorldServiceLocator._WorldServer.CHARACTERs[Target.itemTarget.OwnerGUID].tradeInfo.SendTradeUpdateToTrader();
					}
				}
				EnchantLog.Dispose();
			}
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_ENCHANT_ITEM_TEMPORARY(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			if (Target.itemTarget == null)
			{
				return SpellFailedReason.SPELL_FAILED_ITEM_NOT_FOUND;
			}
			int Duration = SPELLs[SpellID].GetDuration;
			if (Duration == 0)
			{
				Duration = ((SPELLs[SpellID].SpellVisual == 563) ? 600 : ((SPELLs[SpellID].SpellFamilyName == 8) ? 3600 : ((SPELLs[SpellID].SpellFamilyName == 11) ? 1800 : ((SPELLs[SpellID].SpellVisual == 215) ? 1800 : ((SPELLs[SpellID].SpellVisual != 0) ? 3600 : 1800)))));
				Duration = checked(Duration * 1000);
			}
			WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[DEBUG] Enchant duration [{0}]", Duration);
			Target.itemTarget.AddEnchantment(SpellInfo.MiscValue, 1, Duration);
			if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(Target.itemTarget.OwnerGUID))
			{
				WorldServiceLocator._WorldServer.CHARACTERs[Target.itemTarget.OwnerGUID].SendItemUpdate(Target.itemTarget);
				Packets.PacketClass EnchantLog = new Packets.PacketClass(Opcodes.SMSG_ENCHANTMENTLOG);
				EnchantLog.AddUInt64(Target.itemTarget.OwnerGUID);
				EnchantLog.AddUInt64(Caster.GUID);
				EnchantLog.AddInt32(Target.itemTarget.ItemEntry);
				EnchantLog.AddInt32(SpellInfo.MiscValue);
				EnchantLog.AddInt8(0);
				WorldServiceLocator._WorldServer.CHARACTERs[Target.itemTarget.OwnerGUID].client.Send(ref EnchantLog);
				EnchantLog.Dispose();
			}
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_ENCHANT_HELD_ITEM(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			int Duration = SPELLs[SpellID].GetDuration;
			if (Duration == 0)
			{
				Duration = checked((SpellInfo.valueBase + 1) * 1000);
			}
			if (Duration == 0)
			{
				Duration = 10000;
			}
			WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[DEBUG] Enchant duration [{0}]({1})", Duration, SpellInfo.valueBase);
			foreach (WS_Base.BaseUnit Unit in Infected)
			{
				if (Unit is WS_PlayerData.CharacterObject @object && @object.Items.ContainsKey(15) && @object.Items[15].Enchantments.ContainsKey(1) && @object.Items[15].Enchantments[1].ID == SpellInfo.MiscValue)
				{
					@object.Items[15].AddEnchantment(SpellInfo.MiscValue, 1, Duration);
					@object.SendItemUpdate(@object.Items[15]);
				}
			}
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_CHARGE(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			if (Caster is WS_Creatures.CreatureObject @object)
			{
				@object.SetToRealPosition();
			}
			float NearX = Target.unitTarget.positionX;
			NearX = ((!(Target.unitTarget.positionX > Caster.positionX)) ? (NearX + 1f) : (NearX - 1f));
			float NearY = Target.unitTarget.positionY;
			NearY = ((!(Target.unitTarget.positionY > Caster.positionY)) ? (NearY + 1f) : (NearY - 1f));
			float NearZ = WorldServiceLocator._WS_Maps.GetZCoord(NearX, NearY, Caster.positionZ, Caster.MapID);
			if ((NearZ > Target.unitTarget.positionZ + 2f) | (NearZ < Target.unitTarget.positionZ - 2f))
			{
				NearZ = Target.unitTarget.positionZ;
			}
			float moveDist = WorldServiceLocator._WS_Combat.GetDistance(Caster, NearX, NearY, NearZ);
			int TimeToMove = checked((int)Math.Round(moveDist / SPELLs[SpellID].Speed * 1000f));
			Packets.PacketClass SMSG_MONSTER_MOVE = new Packets.PacketClass(Opcodes.SMSG_MONSTER_MOVE);
			SMSG_MONSTER_MOVE.AddPackGUID(Caster.GUID);
			SMSG_MONSTER_MOVE.AddSingle(Caster.positionX);
			SMSG_MONSTER_MOVE.AddSingle(Caster.positionY);
			SMSG_MONSTER_MOVE.AddSingle(Caster.positionZ);
			SMSG_MONSTER_MOVE.AddInt32(WorldServiceLocator._NativeMethods.timeGetTime(""));
			SMSG_MONSTER_MOVE.AddInt8(0);
			SMSG_MONSTER_MOVE.AddInt32(256);
			SMSG_MONSTER_MOVE.AddInt32(TimeToMove);
			SMSG_MONSTER_MOVE.AddInt32(1);
			SMSG_MONSTER_MOVE.AddSingle(NearX);
			SMSG_MONSTER_MOVE.AddSingle(NearY);
			SMSG_MONSTER_MOVE.AddSingle(NearZ);
			Caster.SendToNearPlayers(ref SMSG_MONSTER_MOVE, 0uL);
			SMSG_MONSTER_MOVE.Dispose();
			if (Caster is WS_PlayerData.CharacterObject object1)
			{
				WorldServiceLocator._WS_Combat.SendAttackStart(Caster.GUID, Target.unitTarget.GUID, object1.client);
				object1.attackState.AttackStart(Target.unitTarget);
			}
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_KNOCK_BACK(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			foreach (WS_Base.BaseUnit Unit in Infected)
			{
				float Direction = WorldServiceLocator._WS_Combat.GetOrientation(Caster.positionX, Unit.positionX, Caster.positionY, Unit.positionY);
				Packets.PacketClass packet = new Packets.PacketClass(Opcodes.SMSG_MOVE_KNOCK_BACK);
				packet.AddPackGUID(Unit.GUID);
				packet.AddInt32(0);
				packet.AddSingle((float)Math.Cos(Direction));
				packet.AddSingle((float)Math.Sin(Direction));
				packet.AddSingle(SpellInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 10f);
				packet.AddSingle(SpellInfo.MiscValue / -10f);
				Unit.SendToNearPlayers(ref packet, 0uL);
				packet.Dispose();
				if (!(Unit is WS_Creatures.CreatureObject))
				{
				}
			}
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_SCRIPT_EFFECT(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			if (SPELLs[SpellID].SpellFamilyName == 10)
			{
				if (SPELLs[SpellID].SpellIconID == 70 || SPELLs[SpellID].SpellIconID == 242)
				{
					return SPELL_EFFECT_HEAL(ref Target, ref Caster, ref SpellInfo, SpellID, ref Infected, ref Item);
				}
				if (((uint)SPELLs[SpellID].SpellFamilyFlags & 0x800000u) != 0)
				{
					if (Target.unitTarget == null || Target.unitTarget.IsDead)
					{
						return SpellFailedReason.SPELL_FAILED_TARGETS_DEAD;
					}
					int SpellID2 = 0;
					checked
					{
						int num = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1;
						for (int i = 0; i <= num; i++)
						{
							if (((WS_Base.BaseUnit)Caster).ActiveSpells[i] != null && ((WS_Base.BaseUnit)Caster).ActiveSpells[i].GetSpellInfo.SpellVisual == 5622 && ((WS_Base.BaseUnit)Caster).ActiveSpells[i].GetSpellInfo.SpellFamilyName == 10 && ((WS_Base.BaseUnit)Caster).ActiveSpells[i].Aura_Info[2] != null)
							{
								SpellID2 = ((WS_Base.BaseUnit)Caster).ActiveSpells[i].Aura_Info[2].valueBase + 1;
								break;
							}
						}
						if (SpellID2 == 0 || !SPELLs.ContainsKey(SpellID2))
						{
							return SpellFailedReason.SPELL_FAILED_UNKNOWN;
						}
						CastSpellParameters castParams = new CastSpellParameters(ref Target, ref Caster, SpellID2);
						ThreadPool.QueueUserWorkItem(new WaitCallback(castParams.Cast));
					}
				}
			}
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public SpellFailedReason SPELL_EFFECT_DUEL(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			int implicitTargetA = SpellInfo.implicitTargetA;
			if (implicitTargetA == 25)
			{
				if (!(Target.unitTarget is WS_PlayerData.CharacterObject))
				{
					return SpellFailedReason.SPELL_FAILED_TARGET_NOT_PLAYER;
				}
				if (Caster is WS_PlayerData.CharacterObject @object)
				{
					if (decimal.Compare(new decimal(@object.DuelArbiter), 0m) != 0)
					{
						return SpellFailedReason.SPELL_FAILED_SPELL_IN_PROGRESS;
					}
					if (((WS_PlayerData.CharacterObject)Target.unitTarget).IsInDuel)
					{
						return SpellFailedReason.SPELL_FAILED_TARGET_DUELING;
					}
					if (((WS_PlayerData.CharacterObject)Target.unitTarget).inCombatWith.Count > 0)
					{
						return SpellFailedReason.SPELL_FAILED_TARGET_IN_COMBAT;
					}
					if (Caster.Invisibility != 0)
					{
						return SpellFailedReason.SPELL_FAILED_CANT_DUEL_WHILE_INVISIBLE;
					}
					float flagX = Caster.positionX + (Target.unitTarget.positionX - Caster.positionX) / 2f;
					float flagY = Caster.positionY + (Target.unitTarget.positionY - Caster.positionY) / 2f;
					float flagZ = WorldServiceLocator._WS_Maps.GetZCoord(flagX, flagY, Caster.positionZ + 3f, Caster.MapID);
					WS_GameObjects.GameObjectObject tmpGO = new WS_GameObjects.GameObjectObject(SpellInfo.MiscValue, Caster.MapID, flagX, flagY, flagZ, 0f, Caster.GUID);
					tmpGO.AddToWorld();
					((WS_PlayerData.CharacterObject)Target.unitTarget).DuelArbiter = tmpGO.GUID;
					@object.DuelPartner = (WS_PlayerData.CharacterObject)Target.unitTarget;
					((WS_PlayerData.CharacterObject)Target.unitTarget).DuelPartner = @object;
					Packets.PacketClass packet = new Packets.PacketClass(Opcodes.SMSG_DUEL_REQUESTED);
					packet.AddUInt64(tmpGO.GUID);
					packet.AddUInt64(Caster.GUID);
					((WS_PlayerData.CharacterObject)Target.unitTarget).client.SendMultiplyPackets(ref packet);
					@object.client.SendMultiplyPackets(ref packet);
					packet.Dispose();
					return SpellFailedReason.SPELL_NO_ERROR;
				}
				SpellFailedReason SPELL_EFFECT_DUEL = default;
				return SPELL_EFFECT_DUEL;
			}
			return SpellFailedReason.SPELL_FAILED_BAD_IMPLICIT_TARGETS;
		}

		public SpellFailedReason SPELL_EFFECT_QUEST_COMPLETE(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
		{
			foreach (WS_Base.BaseUnit Unit in Infected)
			{
				if (Unit is WS_PlayerData.CharacterObject @object)
				{
					WS_Quests aLLQUESTS = WorldServiceLocator._WorldServer.ALLQUESTS;
					WS_PlayerData.CharacterObject objCharacter = @object;
					aLLQUESTS.CompleteQuest(ref objCharacter, SpellInfo.MiscValue, Caster.GUID);
				}
			}
			return SpellFailedReason.SPELL_NO_ERROR;
		}

		public List<WS_Base.BaseUnit> GetEnemyAtPoint(ref WS_Base.BaseUnit objCharacter, float PosX, float PosY, float PosZ, float Distance)
		{
			List<WS_Base.BaseUnit> result = new List<WS_Base.BaseUnit>();
            switch (objCharacter)
            {
                case WS_PlayerData.CharacterObject _:
                    {
                        ulong[] array = ((WS_PlayerData.CharacterObject)objCharacter).playersNear.ToArray();
                        foreach (ulong pGUID2 in array)
                        {
                            if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(pGUID2) && (((WS_PlayerData.CharacterObject)objCharacter).IsHorde != WorldServiceLocator._WorldServer.CHARACTERs[pGUID2].IsHorde || ((WS_PlayerData.CharacterObject)objCharacter).DuelPartner != null && ((WS_PlayerData.CharacterObject)objCharacter).DuelPartner == WorldServiceLocator._WorldServer.CHARACTERs[pGUID2]) && !WorldServiceLocator._WorldServer.CHARACTERs[pGUID2].IsDead && WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.CHARACTERs[pGUID2], PosX, PosY, PosZ) < Distance)
                            {
                                result.Add(WorldServiceLocator._WorldServer.CHARACTERs[pGUID2]);
                            }
                        }
                        ulong[] array2 = ((WS_PlayerData.CharacterObject)objCharacter).creaturesNear.ToArray();
                        foreach (ulong cGUID in array2)
                        {
                            if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(cGUID) && !(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID] is WS_Totems.TotemObject) && !WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].IsDead && ((WS_PlayerData.CharacterObject)objCharacter).GetReaction(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].Faction) <= TReaction.NEUTRAL && WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID], PosX, PosY, PosZ) < Distance)
                            {
                                result.Add(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID]);
                            }
                        }

                        break;
                    }

                case WS_Creatures.CreatureObject _:
                    {
                        ulong[] array3 = objCharacter.SeenBy.ToArray();
                        foreach (ulong pGUID in array3)
                        {
                            if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(pGUID) && !WorldServiceLocator._WorldServer.CHARACTERs[pGUID].IsDead && WorldServiceLocator._WorldServer.CHARACTERs[pGUID].GetReaction(((WS_Creatures.CreatureObject)objCharacter).Faction) <= TReaction.NEUTRAL && WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.CHARACTERs[pGUID], PosX, PosY, PosZ) < Distance)
                            {
                                result.Add(WorldServiceLocator._WorldServer.CHARACTERs[pGUID]);
                            }
                        }

                        break;
                    }
            }
            return result;
		}

		public List<WS_Base.BaseUnit> GetEnemyAroundMe(ref WS_Base.BaseUnit objCharacter, float Distance, WS_Base.BaseUnit r = null)
		{
			List<WS_Base.BaseUnit> result = new List<WS_Base.BaseUnit>();
			if (r == null)
			{
				r = objCharacter;
			}
            switch (r)
            {
                case WS_PlayerData.CharacterObject _:
                    {
                        ulong[] array = ((WS_PlayerData.CharacterObject)r).playersNear.ToArray();
                        foreach (ulong pGUID2 in array)
                        {
                            if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(pGUID2) && (((WS_PlayerData.CharacterObject)r).IsHorde != WorldServiceLocator._WorldServer.CHARACTERs[pGUID2].IsHorde || ((WS_PlayerData.CharacterObject)r).DuelPartner != null && ((WS_PlayerData.CharacterObject)r).DuelPartner == WorldServiceLocator._WorldServer.CHARACTERs[pGUID2]) && !WorldServiceLocator._WorldServer.CHARACTERs[pGUID2].IsDead && WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.CHARACTERs[pGUID2], objCharacter) < Distance)
                            {
                                result.Add(WorldServiceLocator._WorldServer.CHARACTERs[pGUID2]);
                            }
                        }
                        ulong[] array2 = ((WS_PlayerData.CharacterObject)r).creaturesNear.ToArray();
                        foreach (ulong cGUID in array2)
                        {
                            if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(cGUID) && !(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID] is WS_Totems.TotemObject) && !WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].IsDead && ((WS_PlayerData.CharacterObject)r).GetReaction(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].Faction) <= TReaction.NEUTRAL && WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID], objCharacter) < Distance)
                            {
                                result.Add(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID]);
                            }
                        }

                        break;
                    }

                case WS_Creatures.CreatureObject _:
                    {
                        ulong[] array3 = r.SeenBy.ToArray();
                        foreach (ulong pGUID in array3)
                        {
                            if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(pGUID) && !WorldServiceLocator._WorldServer.CHARACTERs[pGUID].IsDead && WorldServiceLocator._WorldServer.CHARACTERs[pGUID].GetReaction(((WS_Creatures.CreatureObject)r).Faction) <= TReaction.NEUTRAL && WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.CHARACTERs[pGUID], objCharacter) < Distance)
                            {
                                result.Add(WorldServiceLocator._WorldServer.CHARACTERs[pGUID]);
                            }
                        }

                        break;
                    }
            }
            return result;
		}

		public List<WS_Base.BaseUnit> GetFriendAroundMe(ref WS_Base.BaseUnit objCharacter, float Distance)
		{
			List<WS_Base.BaseUnit> result = new List<WS_Base.BaseUnit>();
            switch (objCharacter)
            {
                case WS_PlayerData.CharacterObject _:
                    {
                        ulong[] array = ((WS_PlayerData.CharacterObject)objCharacter).playersNear.ToArray();
                        foreach (ulong pGUID2 in array)
                        {
                            if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(pGUID2) && ((WS_PlayerData.CharacterObject)objCharacter).IsHorde == WorldServiceLocator._WorldServer.CHARACTERs[pGUID2].IsHorde && !WorldServiceLocator._WorldServer.CHARACTERs[pGUID2].IsDead && WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.CHARACTERs[pGUID2], objCharacter) < Distance)
                            {
                                result.Add(WorldServiceLocator._WorldServer.CHARACTERs[pGUID2]);
                            }
                        }
                        ulong[] array2 = ((WS_PlayerData.CharacterObject)objCharacter).creaturesNear.ToArray();
                        foreach (ulong cGUID in array2)
                        {
                            if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(cGUID) && !(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID] is WS_Totems.TotemObject) && !WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].IsDead && ((WS_PlayerData.CharacterObject)objCharacter).GetReaction(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].Faction) > TReaction.NEUTRAL && WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID], objCharacter) < Distance)
                            {
                                result.Add(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID]);
                            }
                        }

                        break;
                    }

                case WS_Creatures.CreatureObject _:
                    {
                        ulong[] array3 = objCharacter.SeenBy.ToArray();
                        foreach (ulong pGUID in array3)
                        {
                            if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(pGUID) && !WorldServiceLocator._WorldServer.CHARACTERs[pGUID].IsDead && WorldServiceLocator._WorldServer.CHARACTERs[pGUID].GetReaction(((WS_Creatures.CreatureObject)objCharacter).Faction) > TReaction.NEUTRAL && WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.CHARACTERs[pGUID], objCharacter) < Distance)
                            {
                                result.Add(WorldServiceLocator._WorldServer.CHARACTERs[pGUID]);
                            }
                        }

                        break;
                    }
            }
            return result;
		}

		public List<WS_Base.BaseUnit> GetFriendPlayersAroundMe(ref WS_Base.BaseUnit objCharacter, float Distance)
		{
			List<WS_Base.BaseUnit> result = new List<WS_Base.BaseUnit>();
            switch (objCharacter)
            {
                case WS_PlayerData.CharacterObject _:
                    {
                        ulong[] array = ((WS_PlayerData.CharacterObject)objCharacter).playersNear.ToArray();
                        foreach (ulong pGUID2 in array)
                        {
                            if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(pGUID2) && ((WS_PlayerData.CharacterObject)objCharacter).IsHorde == WorldServiceLocator._WorldServer.CHARACTERs[pGUID2].IsHorde && !WorldServiceLocator._WorldServer.CHARACTERs[pGUID2].IsDead && WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.CHARACTERs[pGUID2], objCharacter) < Distance)
                            {
                                result.Add(WorldServiceLocator._WorldServer.CHARACTERs[pGUID2]);
                            }
                        }

                        break;
                    }

                case WS_Creatures.CreatureObject _:
                    {
                        ulong[] array2 = objCharacter.SeenBy.ToArray();
                        foreach (ulong pGUID in array2)
                        {
                            if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(pGUID) && !WorldServiceLocator._WorldServer.CHARACTERs[pGUID].IsDead && WorldServiceLocator._WorldServer.CHARACTERs[pGUID].GetReaction(((WS_Creatures.CreatureObject)objCharacter).Faction) > TReaction.NEUTRAL && WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.CHARACTERs[pGUID], objCharacter) < Distance)
                            {
                                result.Add(WorldServiceLocator._WorldServer.CHARACTERs[pGUID]);
                            }
                        }

                        break;
                    }
            }
            return result;
		}

		public List<WS_Base.BaseUnit> GetPartyMembersAroundMe(ref WS_PlayerData.CharacterObject objCharacter, float distance)
		{
            List<WS_Base.BaseUnit> list = new List<WS_Base.BaseUnit>
            {
                objCharacter
            };
            List<WS_Base.BaseUnit> result = list;
			if (!objCharacter.IsInGroup)
			{
				return result;
			}
			ulong[] array = objCharacter.Group.LocalMembers.ToArray();
			foreach (ulong GUID in array)
			{
				if (objCharacter.playersNear.Contains(GUID) && WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(GUID) && WorldServiceLocator._WS_Combat.GetDistance(objCharacter, WorldServiceLocator._WorldServer.CHARACTERs[GUID]) < distance)
				{
					result.Add(WorldServiceLocator._WorldServer.CHARACTERs[GUID]);
				}
			}
			return result;
		}

		public List<WS_Base.BaseUnit> GetPartyMembersAtPoint(ref WS_PlayerData.CharacterObject objCharacter, float Distance, float PosX, float PosY, float PosZ)
		{
			List<WS_Base.BaseUnit> result = new List<WS_Base.BaseUnit>();
			if (WorldServiceLocator._WS_Combat.GetDistance(objCharacter, PosX, PosY, PosZ) < Distance)
			{
				result.Add(objCharacter);
			}
			if (!objCharacter.IsInGroup)
			{
				return result;
			}
			ulong[] array = objCharacter.Group.LocalMembers.ToArray();
			foreach (ulong GUID in array)
			{
				if (objCharacter.playersNear.Contains(GUID) && WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(GUID) && WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.CHARACTERs[GUID], PosX, PosY, PosZ) < Distance)
				{
					result.Add(WorldServiceLocator._WorldServer.CHARACTERs[GUID]);
				}
			}
			return result;
		}

		public List<WS_Base.BaseUnit> GetEnemyInFrontOfMe(ref WS_Base.BaseUnit objCharacter, float Distance)
		{
			List<WS_Base.BaseUnit> result = new List<WS_Base.BaseUnit>();
			WS_Base.BaseUnit r = null;
			List<WS_Base.BaseUnit> tmp = GetEnemyAroundMe(ref objCharacter, Distance, r);
			foreach (WS_Base.BaseUnit unit in tmp)
			{
				WS_Combat wS_Combat = WorldServiceLocator._WS_Combat;
				WS_Base.BaseObject Object = objCharacter;
				WS_Base.BaseObject Object2 = unit;
				bool flag = wS_Combat.IsInFrontOf(ref Object, ref Object2);
				objCharacter = (WS_Base.BaseUnit)Object;
				if (flag)
				{
					result.Add(unit);
				}
			}
			return result;
		}

		public List<WS_Base.BaseUnit> GetEnemyInBehindMe(ref WS_Base.BaseUnit objCharacter, float Distance)
		{
			List<WS_Base.BaseUnit> result = new List<WS_Base.BaseUnit>();
			WS_Base.BaseUnit r = null;
			List<WS_Base.BaseUnit> tmp = GetEnemyAroundMe(ref objCharacter, Distance, r);
			foreach (WS_Base.BaseUnit unit in tmp)
			{
				WS_Combat wS_Combat = WorldServiceLocator._WS_Combat;
				WS_Base.BaseObject Object = objCharacter;
				WS_Base.BaseObject Object2 = unit;
				bool flag = wS_Combat.IsInBackOf(ref Object, ref Object2);
				objCharacter = (WS_Base.BaseUnit)Object;
				if (flag)
				{
					result.Add(unit);
				}
			}
			return result;
		}

		public void SPELL_AURA_NONE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
		}

		public void SPELL_AURA_DUMMY(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[DEBUG] Aura Dummy for spell {0}.", SpellID);
			AuraAction auraAction = Action;
			if (auraAction == AuraAction.AURA_REMOVEBYDURATION)
			{
				if (SpellID == 33763)
				{
					int Damage = EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
					WS_Base.BaseUnit Caster2 = (WS_Base.BaseUnit)Caster;
					SendHealSpellLog(ref Caster2, ref Target, SpellID, Damage, CriticalHit: false);
					Caster = Caster2;
					WS_Base.BaseUnit obj = Target;
					Caster2 = null;
					obj.Heal(Damage, Caster2);
				}
			}
		}

		public void SPELL_AURA_BIND_SIGHT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			switch (Action)
			{
			case AuraAction.AURA_UPDATE:
				break;
			case AuraAction.AURA_ADD:
				if (Caster is WS_PlayerData.CharacterObject @object)
				{
					@object.DuelArbiter = Target.GUID;
					@object.SetUpdateFlag(712, Target.GUID);
					@object.SendCharacterUpdate();
				}
				break;
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
				if (Caster is WS_PlayerData.CharacterObject object1)
				{
					object1.DuelArbiter = 0uL;
					object1.SetUpdateFlag(712, 0L);
					object1.SendCharacterUpdate();
				}
				break;
			}
		}

		public void SPELL_AURA_FAR_SIGHT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			switch (Action)
			{
			case AuraAction.AURA_UPDATE:
				break;
			case AuraAction.AURA_ADD:
				if (Target is WS_PlayerData.CharacterObject @object)
				{
					@object.SetUpdateFlag(712, EffectInfo.MiscValue);
					@object.SendCharacterUpdate();
				}
				break;
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
				if (Target is WS_PlayerData.CharacterObject object1)
				{
					object1.SetUpdateFlag(712, 0);
					object1.SendCharacterUpdate();
				}
				break;
			}
		}

		public void SPELL_AURA_SCHOOL_IMMUNITY(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			checked
			{
				switch (Action)
				{
				case AuraAction.AURA_UPDATE:
					break;
				case AuraAction.AURA_ADD:
					Target.SchoolImmunity = (byte)(Target.SchoolImmunity | (1 << EffectInfo.MiscValue));
					break;
				case AuraAction.AURA_REMOVE:
				case AuraAction.AURA_REMOVEBYDURATION:
					Target.SchoolImmunity = (byte)(Target.SchoolImmunity & ~(1 << EffectInfo.MiscValue));
					break;
				}
			}
		}

		public void SPELL_AURA_MECHANIC_IMMUNITY(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			checked
			{
				switch (Action)
				{
				case AuraAction.AURA_UPDATE:
					break;
				case AuraAction.AURA_ADD:
					Target.MechanicImmunity = (uint)(Target.MechanicImmunity | (1 << EffectInfo.MiscValue));
					Target.RemoveAurasByMechanic(EffectInfo.MiscValue);
					break;
				case AuraAction.AURA_REMOVE:
				case AuraAction.AURA_REMOVEBYDURATION:
					Target.MechanicImmunity = (uint)(Target.MechanicImmunity & ~(1 << EffectInfo.MiscValue));
					break;
				}
			}
		}

		public void SPELL_AURA_DISPEL_IMMUNITY(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			checked
			{
				switch (Action)
				{
				case AuraAction.AURA_UPDATE:
					break;
				case AuraAction.AURA_ADD:
					Target.DispellImmunity = (uint)(Target.DispellImmunity | (1 << EffectInfo.MiscValue));
					break;
				case AuraAction.AURA_REMOVE:
				case AuraAction.AURA_REMOVEBYDURATION:
					Target.DispellImmunity = (uint)(Target.DispellImmunity & ~(1 << EffectInfo.MiscValue));
					break;
				}
			}
		}

		public void SPELL_AURA_TRACK_CREATURES(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			switch (Action)
			{
			case AuraAction.AURA_UPDATE:
				break;
			case AuraAction.AURA_ADD:
				if (Target is WS_PlayerData.CharacterObject @object)
				{
					@object.SetUpdateFlag(1104, 1 << checked(EffectInfo.MiscValue - 1));
					@object.SendCharacterUpdate();
				}
				break;
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
				if (Target is WS_PlayerData.CharacterObject object1)
				{
					object1.SetUpdateFlag(1104, 0);
					object1.SendCharacterUpdate();
				}
				break;
			}
		}

		public void SPELL_AURA_TRACK_RESOURCES(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			switch (Action)
			{
			case AuraAction.AURA_UPDATE:
				break;
			case AuraAction.AURA_ADD:
				if (Target is WS_PlayerData.CharacterObject @object)
				{
					@object.SetUpdateFlag(1105, 1 << checked(EffectInfo.MiscValue - 1));
					@object.SendCharacterUpdate();
				}
				break;
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
				if (Target is WS_PlayerData.CharacterObject object1)
				{
					object1.SetUpdateFlag(1105, 0);
					object1.SendCharacterUpdate();
				}
				break;
			}
		}

		public void SPELL_AURA_MOD_SCALE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			switch (Action)
			{
			case AuraAction.AURA_UPDATE:
				return;
			case AuraAction.AURA_ADD:
			{
				ref float size2 = ref Target.Size;
				size2 = (float)(size2 * (EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100.0 + 1.0));
				break;
			}
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
			{
				ref float size = ref Target.Size;
				size = (float)(size / (EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100.0 + 1.0));
				break;
			}
			}
			if (Target is WS_PlayerData.CharacterObject @object)
			{
				@object.SetUpdateFlag(4, Target.Size);
				@object.SendCharacterUpdate();
				return;
			}
			Packets.UpdatePacketClass packet = new Packets.UpdatePacketClass();
			Packets.UpdateClass tmpUpdate = new Packets.UpdateClass(6);
			tmpUpdate.SetUpdateFlag(4, Target.Size);
			Packets.PacketClass packet2 = packet;
			WS_Creatures.CreatureObject updateObject = (WS_Creatures.CreatureObject)Target;
			tmpUpdate.AddToPacket(ref packet2, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
			packet = (Packets.UpdatePacketClass)packet2;
			WS_Base.BaseUnit obj = Target;
			packet2 = packet;
			obj.SendToNearPlayers(ref packet2, 0uL);
			packet = (Packets.UpdatePacketClass)packet2;
			tmpUpdate.Dispose();
			packet.Dispose();
		}

		public void SPELL_AURA_MOD_SKILL(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			checked
			{
				switch (Action)
				{
				case AuraAction.AURA_UPDATE:
					break;
				case AuraAction.AURA_ADD:
					if (Target is WS_PlayerData.CharacterObject @object && @object.Skills.ContainsKey(EffectInfo.MiscValue))
					{
						WS_PlayerData.CharacterObject characterObject2 = @object;
						ref short bonus2 = ref characterObject2.Skills[EffectInfo.MiscValue].Bonus;
						bonus2 = (short)(bonus2 + EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0));
						characterObject2.SetUpdateFlag(718 + characterObject2.SkillsPositions[EffectInfo.MiscValue] * 3 + 2, characterObject2.Skills[EffectInfo.MiscValue].Bonus);
						characterObject2.SendCharacterUpdate();
                        }
                        break;
				case AuraAction.AURA_REMOVE:
				case AuraAction.AURA_REMOVEBYDURATION:
					if (Target is WS_PlayerData.CharacterObject object1 && object1.Skills.ContainsKey(EffectInfo.MiscValue))
					{
						WS_PlayerData.CharacterObject characterObject = object1;
						ref short bonus = ref characterObject.Skills[EffectInfo.MiscValue].Bonus;
						bonus = (short)(bonus - EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0));
						characterObject.SetUpdateFlag(718 + characterObject.SkillsPositions[EffectInfo.MiscValue] * 3 + 2, characterObject.Skills[EffectInfo.MiscValue].Bonus);
						characterObject.SendCharacterUpdate();
                        }
                        break;
				}
			}
		}

		public void SPELL_AURA_PERIODIC_DUMMY(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			checked
			{
				switch (Action)
				{
				case AuraAction.AURA_ADD:
					if (Target is WS_PlayerData.CharacterObject @object)
					{
						switch (SpellID)
						{
						case 430:
						case 431:
						case 432:
						case 1133:
						case 1135:
						case 1137:
						case 10250:
						case 22734:
						case 27089:
						case 34291:
						case 43706:
						case 46755:
						{
							int Damage = EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0) * StackCount;
							@object.ManaRegenBonus += Damage;
							@object.UpdateManaRegen();
							break;
						}
						}
					}
					break;
				case AuraAction.AURA_REMOVE:
				case AuraAction.AURA_REMOVEBYDURATION:
					if (Caster is WS_PlayerData.CharacterObject)
					{
						switch (SpellID)
						{
						case 430:
						case 431:
						case 432:
						case 1133:
						case 1135:
						case 1137:
						case 10250:
						case 22734:
						case 27089:
						case 34291:
						case 43706:
						case 46755:
						{
							int Damage2 = EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0) * StackCount;
							((WS_PlayerData.CharacterObject)Target).ManaRegenBonus -= Damage2;
							((WS_PlayerData.CharacterObject)Target).UpdateManaRegen();
							break;
						}
						}
					}
					break;
				case AuraAction.AURA_UPDATE:
					if (SpellID == 43265 || unchecked((uint)(SpellID - 49936)) <= 2u)
					{
						int Damage3;
						if (Caster is WS_DynamicObjects.DynamicObjectObject object1)
						{
							Damage3 = EffectInfo.GetValue(unchecked(object1.Caster.Level), 0) * StackCount;
							Target.DealDamage(Damage3, object1.Caster);
							break;
						}
						Damage3 = EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0) * StackCount;
						WS_Base.BaseUnit obj = Target;
						int damage = Damage3;
						WS_Base.BaseUnit Attacker = (WS_Base.BaseUnit)Caster;
						obj.DealDamage(damage, Attacker);
					}
					break;
				}
			}
		}

		public void SPELL_AURA_PERIODIC_DAMAGE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			switch (Action)
			{
			case AuraAction.AURA_ADD:
				break;
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
				break;
			case AuraAction.AURA_UPDATE:
			{
				if (Caster is WS_DynamicObjects.DynamicObjectObject @object)
				{
					int Damage2;
					checked
					{
						Damage2 = EffectInfo.GetValue(unchecked(@object.Caster.Level), 0) * StackCount;
					}
					Target.DealSpellDamage(ref @object.Caster, ref EffectInfo, SpellID, Damage2, (DamageTypes)checked((byte)SPELLs[SpellID].School), SpellType.SPELL_TYPE_DOT);
					break;
				}
				int Damage;
				WS_Base.BaseUnit obj;
				WS_Base.BaseUnit Caster2;
				checked
				{
					Damage = EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0) * StackCount;
					obj = Target;
					Caster2 = (WS_Base.BaseUnit)Caster;
				}
				obj.DealSpellDamage(ref Caster2, ref EffectInfo, SpellID, Damage, (DamageTypes)checked((byte)SPELLs[SpellID].School), SpellType.SPELL_TYPE_DOT);
				Caster = Caster2;
				break;
			}
			}
		}

		public void SPELL_AURA_PERIODIC_HEAL(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			switch (Action)
			{
			case AuraAction.AURA_ADD:
				break;
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
				break;
			case AuraAction.AURA_UPDATE:
			{
				int Damage;
				WS_Base.BaseUnit obj;
				WS_Base.BaseUnit Caster2;
				checked
				{
					Damage = EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0) * StackCount;
					obj = Target;
					Caster2 = (WS_Base.BaseUnit)Caster;
				}
				obj.DealSpellDamage(ref Caster2, ref EffectInfo, SpellID, Damage, (DamageTypes)checked((byte)SPELLs[SpellID].School), SpellType.SPELL_TYPE_HEALDOT);
				Caster = Caster2;
				break;
			}
			}
		}

		public void SPELL_AURA_PERIODIC_ENERGIZE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			switch (Action)
			{
			case AuraAction.AURA_ADD:
				break;
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
				break;
			case AuraAction.AURA_UPDATE:
			{
				ManaTypes Power = (ManaTypes)EffectInfo.MiscValue;
				int Damage;
				WS_Base.BaseUnit Caster2;
				checked
				{
					Damage = EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0) * StackCount;
					Caster2 = (WS_Base.BaseUnit)Caster;
				}
				SendPeriodicAuraLog(ref Caster2, ref Target, SpellID, (int)Power, Damage, EffectInfo.ApplyAuraIndex);
				Caster = Caster2;
				WS_Base.BaseUnit obj = Target;
				Caster2 = (WS_Base.BaseUnit)Caster;
				obj.Energize(Damage, Power, Caster2);
				Caster = Caster2;
				break;
			}
			}
		}

		public void SPELL_AURA_PERIODIC_LEECH(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			checked
			{
				switch (Action)
				{
				case AuraAction.AURA_ADD:
					break;
				case AuraAction.AURA_REMOVE:
				case AuraAction.AURA_REMOVEBYDURATION:
					break;
				case AuraAction.AURA_UPDATE:
				{
					int Damage = EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0) * StackCount;
					WS_Base.BaseUnit Caster2 = (WS_Base.BaseUnit)Caster;
					SendPeriodicAuraLog(ref Caster2, ref Target, SpellID, SPELLs[SpellID].School, Damage, EffectInfo.ApplyAuraIndex);
					Caster = Caster2;
					Caster2 = (WS_Base.BaseUnit)Caster;
					SendPeriodicAuraLog(ref Target, ref Caster2, SpellID, SPELLs[SpellID].School, Damage, EffectInfo.ApplyAuraIndex);
					Caster = Caster2;
					WS_Base.BaseUnit obj = Target;
					Caster2 = (WS_Base.BaseUnit)Caster;
					obj.DealDamage(Damage, Caster2);
					Caster = Caster2;
					((WS_Base.BaseUnit)Caster).Heal(Damage, Target);
					break;
				}
				}
			}
		}

		public void SPELL_AURA_PERIODIC_MANA_LEECH(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			switch (Action)
			{
			case AuraAction.AURA_ADD:
				break;
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
				break;
			case AuraAction.AURA_UPDATE:
			{
				ManaTypes Power = (ManaTypes)EffectInfo.MiscValue;
				checked
				{
					int Damage = EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0) * StackCount;
					WS_Base.BaseUnit Target2 = (WS_Base.BaseUnit)Caster;
					SendPeriodicAuraLog(ref Target, ref Target2, SpellID, unchecked((int)Power), Damage, EffectInfo.ApplyAuraIndex);
					Caster = Target2;
					WS_Base.BaseUnit obj = Target;
					int damage = -Damage;
					Target2 = (WS_Base.BaseUnit)Caster;
					obj.Energize(damage, Power, Target2);
					Caster = Target2;
					((WS_Base.BaseUnit)Caster).Energize(Damage, Power, Target);
					break;
				}
			}
			}
		}

		public void SPELL_AURA_PERIODIC_TRIGGER_SPELL(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			switch (Action)
			{
			case AuraAction.AURA_ADD:
				break;
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
				break;
			case AuraAction.AURA_UPDATE:
			{
				SpellTargets Targets = new SpellTargets();
				Targets.SetTarget_UNIT(ref Target);
				CastSpellParameters castParams = new CastSpellParameters(ref Targets, ref Caster, EffectInfo.TriggerSpell);
				ThreadPool.QueueUserWorkItem(new WaitCallback(castParams.Cast));
                        if (Caster is WS_Base.BaseUnit Caster2)
                        {
                            SendPeriodicAuraLog(ref Caster2, ref Target, SpellID, SPELLs[SpellID].School, 0, EffectInfo.ApplyAuraIndex);
                            Caster = Caster2;
                        }
                        break;
			}
			}
		}

		public void SPELL_AURA_PERIODIC_DAMAGE_PERCENT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			switch (Action)
			{
			case AuraAction.AURA_ADD:
				break;
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
				break;
			case AuraAction.AURA_UPDATE:
			{
				int Damage;
				WS_Base.BaseUnit obj;
				WS_Base.BaseUnit Caster2;
				checked
				{
					Damage = (int)Math.Round(Target.Life.Maximum * EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0) / 100.0 * StackCount);
					obj = Target;
					Caster2 = (WS_Base.BaseUnit)Caster;
				}
				obj.DealSpellDamage(ref Caster2, ref EffectInfo, SpellID, Damage, (DamageTypes)checked((byte)SPELLs[SpellID].School), SpellType.SPELL_TYPE_DOT);
				Caster = Caster2;
				break;
			}
			}
		}

		public void SPELL_AURA_PERIODIC_HEAL_PERCENT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			switch (Action)
			{
			case AuraAction.AURA_UPDATE:
			{
				int Damage;
				WS_Base.BaseUnit obj;
				WS_Base.BaseUnit Caster2;
				checked
				{
					Damage = (int)Math.Round(Target.Life.Maximum * EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0) / 100.0 * StackCount);
					obj = Target;
					Caster2 = (WS_Base.BaseUnit)Caster;
				}
				obj.DealSpellDamage(ref Caster2, ref EffectInfo, SpellID, Damage, (DamageTypes)checked((byte)SPELLs[SpellID].School), SpellType.SPELL_TYPE_HEALDOT);
				Caster = Caster2;
				break;
			}
			case AuraAction.AURA_ADD:
				break;
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
				break;
			}
		}

		public void SPELL_AURA_PERIODIC_ENERGIZE_PERCENT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			switch (Action)
			{
			case AuraAction.AURA_UPDATE:
			{
				ManaTypes Power = (ManaTypes)EffectInfo.MiscValue;
				int Damage;
				WS_Base.BaseUnit Caster2;
				checked
				{
					Damage = (int)Math.Round(Target.Mana.Maximum * EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0) / 100.0 * StackCount);
					Caster2 = (WS_Base.BaseUnit)Caster;
				}
				SendPeriodicAuraLog(ref Caster2, ref Target, SpellID, (int)Power, Damage, EffectInfo.ApplyAuraIndex);
				Caster = Caster2;
				WS_Base.BaseUnit obj = Target;
				Caster2 = (WS_Base.BaseUnit)Caster;
				obj.Energize(Damage, Power, Caster2);
				Caster = Caster2;
				break;
			}
			case AuraAction.AURA_ADD:
				break;
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
				break;
			}
		}

		public void SPELL_AURA_MOD_REGEN(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			if (!(Target is WS_PlayerData.CharacterObject))
			{
				return;
			}
			checked
			{
				switch (Action)
				{
				case AuraAction.AURA_UPDATE:
					break;
				case AuraAction.AURA_ADD:
				{
					int Damage2 = EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0) * StackCount;
					((WS_PlayerData.CharacterObject)Target).LifeRegenBonus += Damage2;
					if ((unchecked((uint)SPELLs[SpellID].auraInterruptFlags) & 0x40000u) != 0)
					{
						Target.DoEmote(7);
					}
					else if (SpellID == 20577)
					{
						Target.DoEmote(398);
					}
					break;
				}
				case AuraAction.AURA_REMOVE:
				case AuraAction.AURA_REMOVEBYDURATION:
				{
					int Damage = EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0) * StackCount;
					((WS_PlayerData.CharacterObject)Target).LifeRegenBonus -= Damage;
					break;
				}
				}
			}
		}

		public void SPELL_AURA_MOD_POWER_REGEN(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			if (!(Target is WS_PlayerData.CharacterObject))
			{
				return;
			}
			checked
			{
				switch (Action)
				{
				case AuraAction.AURA_UPDATE:
					break;
				case AuraAction.AURA_ADD:
				{
					int Damage = EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0) * StackCount;
					if (EffectInfo.MiscValue == 0)
					{
						((WS_PlayerData.CharacterObject)Target).ManaRegenBonus += Damage;
						((WS_PlayerData.CharacterObject)Target).UpdateManaRegen();
					}
					else if (EffectInfo.MiscValue == 1)
					{
						ref int rageRegenBonus2 = ref ((WS_PlayerData.CharacterObject)Target).RageRegenBonus;
						rageRegenBonus2 = (int)Math.Round(rageRegenBonus2 + Damage / 17.0 * 10.0);
					}
					if ((unchecked((uint)SPELLs[SpellID].auraInterruptFlags) & 0x40000u) != 0)
					{
						Target.DoEmote(7);
					}
					else if (SpellID == 20577)
					{
						Target.DoEmote(398);
					}
					break;
				}
				case AuraAction.AURA_REMOVE:
				case AuraAction.AURA_REMOVEBYDURATION:
				{
					int Damage2 = EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0) * StackCount;
					if (EffectInfo.MiscValue == 0)
					{
						((WS_PlayerData.CharacterObject)Target).ManaRegenBonus -= Damage2;
						((WS_PlayerData.CharacterObject)Target).UpdateManaRegen();
					}
					else if (EffectInfo.MiscValue == 1)
					{
						ref int rageRegenBonus = ref ((WS_PlayerData.CharacterObject)Target).RageRegenBonus;
						rageRegenBonus = (int)Math.Round(rageRegenBonus - Damage2 / 17.0 * 10.0);
					}
					break;
				}
				}
			}
		}

		public void SPELL_AURA_MOD_POWER_REGEN_PERCENT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			if (!(Target is WS_PlayerData.CharacterObject))
			{
				return;
			}
			checked
			{
				switch (Action)
				{
				case AuraAction.AURA_UPDATE:
					break;
				case AuraAction.AURA_ADD:
					if (EffectInfo.MiscValue == 0)
					{
						int Damage2 = (int)Math.Round(EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0) * StackCount / 100.0);
						((WS_PlayerData.CharacterObject)Target).ManaRegenerationModifier += Damage2;
						((WS_PlayerData.CharacterObject)Target).UpdateManaRegen();
					}
					break;
				case AuraAction.AURA_REMOVE:
				case AuraAction.AURA_REMOVEBYDURATION:
				{
					int Damage = (int)Math.Round(EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0) * StackCount / 100.0);
					((WS_PlayerData.CharacterObject)Target).ManaRegenerationModifier -= Damage;
					((WS_PlayerData.CharacterObject)Target).UpdateManaRegen();
					break;
				}
				}
			}
		}

		public void SPELL_AURA_TRANSFORM(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			switch (Action)
			{
			case AuraAction.AURA_ADD:
				if (!WorldServiceLocator._WorldServer.CREATURESDatabase.ContainsKey(EffectInfo.MiscValue))
				{
					CreatureInfo creature = new CreatureInfo(EffectInfo.MiscValue);
					WorldServiceLocator._WorldServer.CREATURESDatabase.Add(EffectInfo.MiscValue, creature);
				}
				Target.Model = WorldServiceLocator._WorldServer.CREATURESDatabase[EffectInfo.MiscValue].GetFirstModel;
				break;
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
				if (Target is WS_PlayerData.CharacterObject @object)
				{
					Target.Model = WorldServiceLocator._Functions.GetRaceModel(@object.Race, (int)@object.Gender);
				}
				else
				{
					Target.Model = ((WS_Creatures.CreatureObject)Target).CreatureInfo.GetRandomModel;
				}
				break;
			case AuraAction.AURA_UPDATE:
				return;
			}
			if (Target is WS_PlayerData.CharacterObject @object1)
			{
				@object1.SetUpdateFlag(131, Target.Model);
				@object1.SendCharacterUpdate();
				return;
			}
			Packets.UpdatePacketClass packet = new Packets.UpdatePacketClass();
			Packets.UpdateClass tmpUpdate = new Packets.UpdateClass(188);
			tmpUpdate.SetUpdateFlag(131, Target.Model);
			Packets.PacketClass packet2 = packet;
			WS_Creatures.CreatureObject updateObject = (WS_Creatures.CreatureObject)Target;
			tmpUpdate.AddToPacket(ref packet2, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
			packet = (Packets.UpdatePacketClass)packet2;
			WS_Base.BaseUnit obj = Target;
			packet2 = packet;
			obj.SendToNearPlayers(ref packet2, 0uL);
			packet = (Packets.UpdatePacketClass)packet2;
			tmpUpdate.Dispose();
			packet.Dispose();
		}

		public void SPELL_AURA_GHOST(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			if (Target is WS_PlayerData.CharacterObject @object)
			{
				switch (Action)
				{
				case AuraAction.AURA_UPDATE:
					break;
				case AuraAction.AURA_ADD:
				{
					Target.Invisibility = InvisibilityLevel.DEAD;
					Target.CanSeeInvisibility = InvisibilityLevel.DEAD;
					WS_CharMovement wS_CharMovement2 = WorldServiceLocator._WS_CharMovement;
					WS_PlayerData.CharacterObject Character = @object;
					wS_CharMovement2.UpdateCell(ref Character);
					Target = Character;
					break;
				}
				case AuraAction.AURA_REMOVE:
				case AuraAction.AURA_REMOVEBYDURATION:
				{
					Target.Invisibility = InvisibilityLevel.VISIBLE;
					Target.CanSeeInvisibility = InvisibilityLevel.INIVISIBILITY;
					WS_CharMovement wS_CharMovement = WorldServiceLocator._WS_CharMovement;
					WS_PlayerData.CharacterObject Character = @object;
					wS_CharMovement.UpdateCell(ref Character);
					Target = Character;
					break;
				}
				}
			}
		}

		public void SPELL_AURA_MOD_INVISIBILITY(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			checked
			{
				switch (Action)
				{
				case AuraAction.AURA_UPDATE:
					return;
				case AuraAction.AURA_ADD:
					((WS_PlayerData.CharacterObject)Target).cPlayerFieldBytes2 = ((WS_PlayerData.CharacterObject)Target).cPlayerFieldBytes2 | 0x4000;
					Target.Invisibility = InvisibilityLevel.INIVISIBILITY;
					Target.Invisibility_Value += EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0);
					break;
				case AuraAction.AURA_REMOVE:
				case AuraAction.AURA_REMOVEBYDURATION:
					((WS_PlayerData.CharacterObject)Target).cPlayerFieldBytes2 = ((WS_PlayerData.CharacterObject)Target).cPlayerFieldBytes2 & -16385;
					Target.Invisibility = InvisibilityLevel.VISIBLE;
					Target.Invisibility_Value -= EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0);
					break;
				}
				if (Target is WS_PlayerData.CharacterObject @object)
				{
					@object.SetUpdateFlag(1260, @object.cPlayerFieldBytes2);
					@object.SendCharacterUpdate();
					WS_CharMovement wS_CharMovement = WorldServiceLocator._WS_CharMovement;
					WS_PlayerData.CharacterObject Character = @object;
					wS_CharMovement.UpdateCell(ref Character);
				}
			}
		}

		public void SPELL_AURA_MOD_STEALTH(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			checked
			{
				switch (Action)
				{
				case AuraAction.AURA_UPDATE:
					return;
				case AuraAction.AURA_ADD:
					Target.Invisibility = InvisibilityLevel.STEALTH;
					Target.Invisibility_Value += EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0);
					break;
				case AuraAction.AURA_REMOVE:
				case AuraAction.AURA_REMOVEBYDURATION:
					Target.Invisibility = InvisibilityLevel.VISIBLE;
					Target.Invisibility_Value -= EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0);
					break;
				}
				WS_CharMovement wS_CharMovement = WorldServiceLocator._WS_CharMovement;
				WS_PlayerData.CharacterObject Character = (WS_PlayerData.CharacterObject)Target;
				wS_CharMovement.UpdateCell(ref Character);
			}
		}

		public void SPELL_AURA_MOD_STEALTH_LEVEL(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			checked
			{
				switch (Action)
				{
				case AuraAction.AURA_UPDATE:
					break;
				case AuraAction.AURA_ADD:
					Target.Invisibility_Bonus += EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0);
					break;
				case AuraAction.AURA_REMOVE:
				case AuraAction.AURA_REMOVEBYDURATION:
					Target.Invisibility_Bonus -= EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0);
					break;
				}
			}
		}

		public void SPELL_AURA_MOD_INVISIBILITY_DETECTION(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			checked
			{
				switch (Action)
				{
				case AuraAction.AURA_UPDATE:
					return;
				case AuraAction.AURA_ADD:
					Target.CanSeeInvisibility_Invisibility += EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0);
					break;
				case AuraAction.AURA_REMOVE:
				case AuraAction.AURA_REMOVEBYDURATION:
					Target.CanSeeInvisibility_Invisibility -= EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0);
					break;
				}
				if (Target is WS_PlayerData.CharacterObject @object)
				{
					WS_CharMovement wS_CharMovement = WorldServiceLocator._WS_CharMovement;
					WS_PlayerData.CharacterObject Character = @object;
					wS_CharMovement.UpdateCell(ref Character);
				}
			}
		}

		public void SPELL_AURA_MOD_DETECT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			checked
			{
				switch (Action)
				{
				case AuraAction.AURA_UPDATE:
					return;
				case AuraAction.AURA_ADD:
					Target.CanSeeInvisibility_Stealth += EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0);
					break;
				case AuraAction.AURA_REMOVE:
				case AuraAction.AURA_REMOVEBYDURATION:
					Target.CanSeeInvisibility_Stealth -= EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0);
					break;
				}
				if (Target is WS_PlayerData.CharacterObject @object)
				{
					WS_CharMovement wS_CharMovement = WorldServiceLocator._WS_CharMovement;
					WS_PlayerData.CharacterObject Character = @object;
					wS_CharMovement.UpdateCell(ref Character);
				}
			}
		}

		public void SPELL_AURA_DETECT_STEALTH(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			switch (Action)
			{
			case AuraAction.AURA_UPDATE:
				return;
			case AuraAction.AURA_ADD:
				Target.CanSeeStealth = true;
				break;
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
				Target.CanSeeStealth = false;
				break;
			}
			if (Target is WS_PlayerData.CharacterObject @object)
			{
				WS_CharMovement wS_CharMovement = WorldServiceLocator._WS_CharMovement;
				WS_PlayerData.CharacterObject Character = @object;
				wS_CharMovement.UpdateCell(ref Character);
			}
		}

		public void SPELL_AURA_MOD_DISARM(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			if (!(Target is WS_PlayerData.CharacterObject))
			{
				return;
			}
			switch (Action)
			{
			case AuraAction.AURA_UPDATE:
				break;
			case AuraAction.AURA_ADD:
				if (Target is WS_PlayerData.CharacterObject @object)
				{
					@object.Disarmed = true;
					@object.cUnitFlags = 2097152;
					@object.SetUpdateFlag(46, @object.cUnitFlags);
					@object.SendCharacterUpdate();
				}
				break;
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
				if (Target is WS_PlayerData.CharacterObject object1)
				{
					object1.Disarmed = false;
					object1.cUnitFlags &= -2097153;
					object1.SetUpdateFlag(46, object1.cUnitFlags);
					object1.SendCharacterUpdate();
				}
				break;
			}
		}

		public void SPELL_AURA_SCHOOL_ABSORB(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			if (!(Caster is WS_Base.BaseUnit))
			{
				return;
			}
			checked
			{
				switch (Action)
				{
				case AuraAction.AURA_UPDATE:
					break;
				case AuraAction.AURA_ADD:
					if (!Target.AbsorbSpellLeft.ContainsKey(SpellID))
					{
						Target.AbsorbSpellLeft.Add(SpellID, (uint)EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0) + ((uint)EffectInfo.MiscValue << 23));
					}
					break;
				case AuraAction.AURA_REMOVE:
				case AuraAction.AURA_REMOVEBYDURATION:
					if (Target.AbsorbSpellLeft.ContainsKey(SpellID))
					{
						Target.AbsorbSpellLeft.Remove(SpellID);
					}
					break;
				}
			}
		}

		public void SPELL_AURA_MOD_SHAPESHIFT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			switch (Action)
			{
			case AuraAction.AURA_ADD:
			{
				Target.RemoveAurasOfType(AuraEffects_Names.SPELL_AURA_MOD_SHAPESHIFT, SpellID);
				WS_Base.BaseUnit obj = Target;
				int NotSpellID = 0;
				obj.RemoveAurasOfType(AuraEffects_Names.SPELL_AURA_MOUNTED, NotSpellID);
						if (Target is WS_PlayerData.CharacterObject object1 && object1.Classe == Classes.CLASS_DRUID && (EffectInfo.MiscValue == 4 || EffectInfo.MiscValue == 1 || EffectInfo.MiscValue == 5 || EffectInfo.MiscValue == 8 || EffectInfo.MiscValue == 3 || EffectInfo.MiscValue == 29 || EffectInfo.MiscValue == 27 || EffectInfo.MiscValue == 31))
						{
					WS_Base.BaseUnit obj2 = Target;
					NotSpellID = 0;
					obj2.RemoveAurasOfType(AuraEffects_Names.SPELL_AURA_MOD_ROOT, NotSpellID);
					WS_Base.BaseUnit obj3 = Target;
					NotSpellID = 0;
					obj3.RemoveAurasOfType(AuraEffects_Names.SPELL_AURA_MOD_DECREASE_SPEED, NotSpellID);
				}
				Target.ShapeshiftForm = (ShapeshiftForm)checked((byte)EffectInfo.MiscValue);
				Target.ManaType = WorldServiceLocator._CommonGlobalFunctions.GetShapeshiftManaType((ShapeshiftForm)checked((byte)EffectInfo.MiscValue), Target.ManaType);
                        Target.Model = Target switch
                        {
                            WS_PlayerData.CharacterObject _ => WorldServiceLocator._CommonGlobalFunctions.GetShapeshiftModel((ShapeshiftForm)checked((byte)EffectInfo.MiscValue), ((WS_PlayerData.CharacterObject)Target).Race, Target.Model),
                            _ => WorldServiceLocator._CommonGlobalFunctions.GetShapeshiftModel((ShapeshiftForm)checked((byte)EffectInfo.MiscValue), 0, Target.Model),
                        };
                        break;
			}
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
				Target.ShapeshiftForm = ShapeshiftForm.FORM_NORMAL;
				if (Target is WS_PlayerData.CharacterObject @object)
				{
					Target.ManaType = WorldServiceLocator._WS_Player_Initializator.GetClassManaType(@object.Classe);
					Target.Model = WorldServiceLocator._Functions.GetRaceModel(@object.Race, (int)@object.Gender);
				}
				else
				{
					Target.ManaType = (ManaTypes)((WS_Creatures.CreatureObject)Target).CreatureInfo.ManaType;
					Target.Model = ((WS_Creatures.CreatureObject)Target).CreatureInfo.GetRandomModel;
				}
				break;
			case AuraAction.AURA_UPDATE:
				return;
			}
            if (Target is WS_PlayerData.CharacterObject characterObject)
            {
                characterObject.SetUpdateFlag(164, characterObject.cBytes2);
                characterObject.SetUpdateFlag(131, characterObject.Model);
                characterObject.SetUpdateFlag(36, characterObject.cBytes0);
                if (characterObject.ManaType == ManaTypes.TYPE_MANA)
                {
                    characterObject.SetUpdateFlag(23, characterObject.Mana.Current);
                    characterObject.SetUpdateFlag(23, characterObject.Mana.Maximum);
                }
                else if (characterObject.ManaType == ManaTypes.TYPE_RAGE)
                {
                    characterObject.SetUpdateFlag(24, characterObject.Rage.Current);
                    characterObject.SetUpdateFlag(24, characterObject.Rage.Maximum);
                }
                else if (characterObject.ManaType == ManaTypes.TYPE_ENERGY)
                {
                    characterObject.SetUpdateFlag(26, characterObject.Energy.Current);
                    characterObject.SetUpdateFlag(26, characterObject.Energy.Maximum);
                }
                WS_Combat wS_Combat = WorldServiceLocator._WS_Combat;
                WS_PlayerData.CharacterObject objCharacter = (WS_PlayerData.CharacterObject)Target;
                wS_Combat.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.BASE_ATTACK);
                characterObject.SetUpdateFlag(134, characterObject.Damage.Minimum);
                characterObject.SetUpdateFlag(135, characterObject.Damage.Maximum);
                characterObject.SendCharacterUpdate();
                characterObject.GroupUpdateFlag |= 8u;
                characterObject.GroupUpdateFlag |= 16u;
                characterObject.GroupUpdateFlag |= 32u;
                WorldServiceLocator._WS_PlayerHelper.InitializeTalentSpells((WS_PlayerData.CharacterObject)Target);
            }
            else
            {
                Packets.UpdatePacketClass packet = new Packets.UpdatePacketClass();
                Packets.UpdateClass tmpUpdate = new Packets.UpdateClass(188);
                tmpUpdate.SetUpdateFlag(164, Target.cBytes2);
                tmpUpdate.SetUpdateFlag(131, Target.Model);
                Packets.PacketClass packet2 = packet;
                WS_Creatures.CreatureObject updateObject = (WS_Creatures.CreatureObject)Target;
                tmpUpdate.AddToPacket(ref packet2, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
                packet = (Packets.UpdatePacketClass)packet2;
                WS_Base.BaseUnit obj4 = Target;
                packet2 = packet;
                obj4.SendToNearPlayers(ref packet2, 0uL);
                packet = (Packets.UpdatePacketClass)packet2;
                tmpUpdate.Dispose();
                packet.Dispose();
            }
            if (!(Target is WS_PlayerData.CharacterObject))
			{
				return;
			}
			if (Action == AuraAction.AURA_ADD)
			{
				if (EffectInfo.MiscValue == 3)
				{
					((WS_PlayerData.CharacterObject)Target).ApplySpell(5419);
				}
				if (EffectInfo.MiscValue == 1)
				{
					((WS_PlayerData.CharacterObject)Target).ApplySpell(3025);
				}
				if (EffectInfo.MiscValue == 5)
				{
					((WS_PlayerData.CharacterObject)Target).ApplySpell(1178);
				}
				if (EffectInfo.MiscValue == 8)
				{
					((WS_PlayerData.CharacterObject)Target).ApplySpell(9635);
				}
				if (EffectInfo.MiscValue == 4)
				{
					((WS_PlayerData.CharacterObject)Target).ApplySpell(5421);
				}
				if (EffectInfo.MiscValue == 31)
				{
					((WS_PlayerData.CharacterObject)Target).ApplySpell(24905);
				}
				if (EffectInfo.MiscValue == 29)
				{
					((WS_PlayerData.CharacterObject)Target).ApplySpell(33948);
					((WS_PlayerData.CharacterObject)Target).ApplySpell(34764);
				}
				if (EffectInfo.MiscValue == 27)
				{
					((WS_PlayerData.CharacterObject)Target).ApplySpell(40121);
					((WS_PlayerData.CharacterObject)Target).ApplySpell(40122);
				}
				if (EffectInfo.MiscValue == 17)
				{
					((WS_PlayerData.CharacterObject)Target).ApplySpell(21156);
				}
				if (EffectInfo.MiscValue == 19)
				{
					((WS_PlayerData.CharacterObject)Target).ApplySpell(7381);
				}
				if (EffectInfo.MiscValue == 18)
				{
					((WS_PlayerData.CharacterObject)Target).ApplySpell(7376);
				}
				return;
			}
			if (EffectInfo.MiscValue == 3)
			{
				Target.RemoveAuraBySpell(5419);
			}
			if (EffectInfo.MiscValue == 1)
			{
				Target.RemoveAuraBySpell(3025);
			}
			if (EffectInfo.MiscValue == 5)
			{
				Target.RemoveAuraBySpell(1178);
			}
			if (EffectInfo.MiscValue == 8)
			{
				Target.RemoveAuraBySpell(9635);
			}
			if (EffectInfo.MiscValue == 4)
			{
				Target.RemoveAuraBySpell(5421);
			}
			if (EffectInfo.MiscValue == 31)
			{
				Target.RemoveAuraBySpell(24905);
			}
			if (EffectInfo.MiscValue == 29)
			{
				Target.RemoveAuraBySpell(33948);
				Target.RemoveAuraBySpell(34764);
			}
			if (EffectInfo.MiscValue == 27)
			{
				Target.RemoveAuraBySpell(40121);
				Target.RemoveAuraBySpell(40122);
			}
			if (EffectInfo.MiscValue == 17)
			{
				Target.RemoveAuraBySpell(21156);
			}
			if (EffectInfo.MiscValue == 19)
			{
				Target.RemoveAuraBySpell(7381);
			}
			if (EffectInfo.MiscValue == 18)
			{
				Target.RemoveAuraBySpell(7376);
			}
			if (EffectInfo.MiscValue == 16)
			{
				Target.RemoveAuraBySpell(7376);
			}
		}

		public void SPELL_AURA_PROC_TRIGGER_SPELL(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			switch (Action)
			{
			case AuraAction.AURA_ADD:
				break;
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
				break;
			case AuraAction.AURA_UPDATE:
				break;
			}
		}

		public void SPELL_AURA_MOD_INCREASE_SPEED(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			switch (Action)
			{
			case AuraAction.AURA_ADD:
				if (Target is WS_PlayerData.CharacterObject @object)
				{
					float newSpeed = @object.RunSpeed;
					newSpeed = (float)(newSpeed * (EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100.0 + 1.0));
					@object.ChangeSpeedForced(ChangeSpeedType.RUN, newSpeed);
				}
				else if (Target is WS_Creatures.CreatureObject object1)
				{
					object1.SetToRealPosition();
					ref float speedMod2 = ref object1.SpeedMod;
					speedMod2 = (float)(speedMod2 * (EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100.0 + 1.0));
				}
				break;
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
				if (Target is WS_PlayerData.CharacterObject object2)
				{
					float newSpeed2 = object2.RunSpeed;
					if (Caster != null)
					{
						newSpeed2 = (float)(newSpeed2 / (EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100.0 + 1.0));
					}
					object2.ChangeSpeedForced(ChangeSpeedType.RUN, newSpeed2);
				}
				else if (Target is WS_Creatures.CreatureObject object1)
				{
					object1.SetToRealPosition();
					ref float speedMod = ref object1.SpeedMod;
					speedMod = (float)(speedMod / (EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100.0 + 1.0));
				}
				break;
			case AuraAction.AURA_UPDATE:
				break;
			}
		}

		public void SPELL_AURA_MOD_DECREASE_SPEED(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			switch (Action)
			{
			case AuraAction.AURA_ADD:
				if (Target is WS_PlayerData.CharacterObject @object)
				{
					float newSpeed = @object.RunSpeed;
					newSpeed = ((EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) >= 0) ? ((float)(newSpeed / (EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100.0 + 1.0))) : ((float)(newSpeed / (Math.Abs(EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100.0) + 1.0))));
					@object.ChangeSpeedForced(ChangeSpeedType.RUN, newSpeed);
					@object.RemoveAurasByInterruptFlag(128);
				}
				else if (Target is WS_Creatures.CreatureObject object1)
				{
					object1.SetToRealPosition();
					if (EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) < 0)
					{
						ref float speedMod3 = ref object1.SpeedMod;
						speedMod3 = (float)(speedMod3 / (Math.Abs(EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100.0) + 1.0));
					}
					else
					{
						ref float speedMod4 = ref object1.SpeedMod;
						speedMod4 = (float)(speedMod4 / (EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100.0 + 1.0));
					}
				}
				break;
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
				if (Target is WS_PlayerData.CharacterObject object2)
				{
					float newSpeed2 = object2.RunSpeed;
					newSpeed2 = ((EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) >= 0) ? ((float)(newSpeed2 * (EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100.0 + 1.0))) : ((float)(newSpeed2 * (Math.Abs(EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100.0) + 1.0))));
					object2.ChangeSpeedForced(ChangeSpeedType.RUN, newSpeed2);
				}
				else if (Target is WS_Creatures.CreatureObject object1)
				{
					object1.SetToRealPosition();
					if (EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) < 0)
					{
						ref float speedMod = ref object1.SpeedMod;
						speedMod = (float)(speedMod * (Math.Abs(EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100.0) + 1.0));
					}
					else
					{
						ref float speedMod2 = ref object1.SpeedMod;
						speedMod2 = (float)(speedMod2 * (EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100.0 + 1.0));
					}
				}
				break;
			case AuraAction.AURA_UPDATE:
				break;
			}
		}

		public void SPELL_AURA_MOD_INCREASE_SPEED_ALWAYS(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			if (Target is WS_PlayerData.CharacterObject @object)
			{
				switch (Action)
				{
				case AuraAction.AURA_ADD:
				{
					float newSpeed = @object.RunSpeed;
					newSpeed = (float)(newSpeed * (EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100.0 + 1.0));
					@object.ChangeSpeedForced(ChangeSpeedType.RUN, newSpeed);
					break;
				}
				case AuraAction.AURA_REMOVE:
				case AuraAction.AURA_REMOVEBYDURATION:
				{
					float newSpeed2 = @object.RunSpeed;
					newSpeed2 = (float)(newSpeed2 / (EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100.0 + 1.0));
					@object.ChangeSpeedForced(ChangeSpeedType.RUN, newSpeed2);
					break;
				}
				case AuraAction.AURA_UPDATE:
					break;
				}
			}
		}

		public void SPELL_AURA_MOD_INCREASE_MOUNTED_SPEED(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			if (Target is WS_PlayerData.CharacterObject @object)
			{
				switch (Action)
				{
				case AuraAction.AURA_ADD:
				{
					float newSpeed = @object.RunSpeed;
					newSpeed = (float)(newSpeed * (EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100.0 + 1.0));
					@object.ChangeSpeedForced(ChangeSpeedType.RUN, newSpeed);
					break;
				}
				case AuraAction.AURA_REMOVE:
				case AuraAction.AURA_REMOVEBYDURATION:
				{
					float newSpeed2 = @object.RunSpeed;
					newSpeed2 = (float)(newSpeed2 / (EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100.0 + 1.0));
					@object.ChangeSpeedForced(ChangeSpeedType.RUN, newSpeed2);
					break;
				}
				case AuraAction.AURA_UPDATE:
					break;
				}
			}
		}

		public void SPELL_AURA_MOD_INCREASE_MOUNTED_SPEED_ALWAYS(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			if (Target is WS_PlayerData.CharacterObject @object)
			{
				switch (Action)
				{
				case AuraAction.AURA_ADD:
				{
					float newSpeed = @object.RunSpeed;
					newSpeed = (float)(newSpeed * (EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100.0 + 1.0));
					@object.ChangeSpeedForced(ChangeSpeedType.RUN, newSpeed);
					break;
				}
				case AuraAction.AURA_REMOVE:
				case AuraAction.AURA_REMOVEBYDURATION:
				{
					float newSpeed2 = @object.RunSpeed;
					newSpeed2 = (float)(newSpeed2 / (EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100.0 + 1.0));
					@object.ChangeSpeedForced(ChangeSpeedType.RUN, newSpeed2);
					break;
				}
				case AuraAction.AURA_UPDATE:
					break;
				}
			}
		}

		public void SPELL_AURA_MOD_INCREASE_SWIM_SPEED(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			if (!(Target is WS_PlayerData.CharacterObject))
			{
				return;
			}
			switch (Action)
			{
			case AuraAction.AURA_ADD:
			{
				float newSpeed = ((WS_PlayerData.CharacterObject)Target).SwimSpeed;
				newSpeed = (float)(newSpeed * (EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100.0 + 1.0));
				((WS_PlayerData.CharacterObject)Target).SwimSpeed = newSpeed;
				((WS_PlayerData.CharacterObject)Target).ChangeSpeedForced(ChangeSpeedType.SWIM, newSpeed);
				break;
			}
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
			{
				float newSpeed2 = ((WS_PlayerData.CharacterObject)Target).SwimSpeed;
				if (Caster != null)
				{
					newSpeed2 = (float)(newSpeed2 / (EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100.0 + 1.0));
				}
				((WS_PlayerData.CharacterObject)Target).SwimSpeed = newSpeed2;
				((WS_PlayerData.CharacterObject)Target).ChangeSpeedForced(ChangeSpeedType.SWIM, newSpeed2);
				break;
			}
			case AuraAction.AURA_UPDATE:
				break;
			}
		}

		public void SPELL_AURA_MOUNTED(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			switch (Action)
			{
			case AuraAction.AURA_ADD:
			{
				WS_Base.BaseUnit obj = Target;
				int NotSpellID = 0;
				obj.RemoveAurasOfType(AuraEffects_Names.SPELL_AURA_MOD_SHAPESHIFT, NotSpellID);
				Target.RemoveAurasOfType(AuraEffects_Names.SPELL_AURA_MOUNTED, SpellID);
				Target.RemoveAurasByInterruptFlag(131072);
				if (!WorldServiceLocator._WorldServer.CREATURESDatabase.ContainsKey(EffectInfo.MiscValue))
				{
					CreatureInfo creature = new CreatureInfo(EffectInfo.MiscValue);
					WorldServiceLocator._WorldServer.CREATURESDatabase.Add(EffectInfo.MiscValue, creature);
				}
				if (WorldServiceLocator._WorldServer.CREATURESDatabase.ContainsKey(EffectInfo.MiscValue))
				{
					Target.Mount = WorldServiceLocator._WorldServer.CREATURESDatabase[EffectInfo.MiscValue].GetFirstModel;
				}
				else
				{
					Target.Mount = 0;
				}
				break;
			}
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
				Target.Mount = 0;
				Target.RemoveAurasByInterruptFlag(64);
				break;
			case AuraAction.AURA_UPDATE:
				return;
			}
			if (Target is WS_PlayerData.CharacterObject @object)
			{
				@object.SetUpdateFlag(133, Target.Mount);
				@object.SendCharacterUpdate();
				return;
			}
			Packets.UpdatePacketClass packet = new Packets.UpdatePacketClass();
			Packets.UpdateClass tmpUpdate = new Packets.UpdateClass(188);
			tmpUpdate.SetUpdateFlag(133, Target.Mount);
			Packets.PacketClass packet2 = packet;
			WS_Creatures.CreatureObject updateObject = (WS_Creatures.CreatureObject)Target;
			tmpUpdate.AddToPacket(ref packet2, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
			packet = (Packets.UpdatePacketClass)packet2;
			WS_Base.BaseUnit obj2 = Target;
			packet2 = packet;
			obj2.SendToNearPlayers(ref packet2, 0uL);
			packet = (Packets.UpdatePacketClass)packet2;
			tmpUpdate.Dispose();
			packet.Dispose();
		}

		public void SPELL_AURA_MOD_HASTE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			if (Target is WS_PlayerData.CharacterObject @object)
            {
                using WS_PlayerData.CharacterObject characterObject = @object;
                switch (Action)
                {
                    case AuraAction.AURA_ADD:
                        {
                            ref float reference3 = ref characterObject.AttackTimeMods[0];
                            reference3 = (float)(reference3 / (EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100.0 + 1.0));
                            ref float reference4 = ref characterObject.AttackTimeMods[1];
                            reference4 = (float)(reference4 / (EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100.0 + 1.0));
                            break;
                        }
                    case AuraAction.AURA_REMOVE:
                    case AuraAction.AURA_REMOVEBYDURATION:
                        {
                            ref float reference = ref characterObject.AttackTimeMods[0];
                            reference = (float)(reference * (EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100.0 + 1.0));
                            ref float reference2 = ref characterObject.AttackTimeMods[1];
                            reference2 = (float)(reference2 * (EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100.0 + 1.0));
                            break;
                        }
                    case AuraAction.AURA_UPDATE:
                        return;
                }
                characterObject.SetUpdateFlag(126, characterObject.GetAttackTime(WeaponAttackType.BASE_ATTACK));
                characterObject.SetUpdateFlag(128, characterObject.GetAttackTime(WeaponAttackType.OFF_ATTACK));
                characterObject.SendCharacterUpdate(toNear: false);
            }
        }

		public void SPELL_AURA_MOD_RANGED_HASTE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			if (Target is WS_PlayerData.CharacterObject @object)
            {
                using WS_PlayerData.CharacterObject characterObject = @object;
                switch (Action)
                {
                    case AuraAction.AURA_ADD:
                        {
                            ref float reference2 = ref characterObject.AttackTimeMods[2];
                            reference2 = (float)(reference2 / (EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100.0 + 1.0));
                            break;
                        }
                    case AuraAction.AURA_REMOVE:
                    case AuraAction.AURA_REMOVEBYDURATION:
                        {
                            ref float reference = ref characterObject.AttackTimeMods[2];
                            reference = (float)(reference * (EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100.0 + 1.0));
                            break;
                        }
                    case AuraAction.AURA_UPDATE:
                        return;
                }
                characterObject.SetUpdateFlag(128, characterObject.GetAttackTime(WeaponAttackType.RANGED_ATTACK));
                characterObject.SendCharacterUpdate(toNear: false);
            }
        }

		public void SPELL_AURA_MOD_RANGED_AMMO_HASTE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			if (Target is WS_PlayerData.CharacterObject @object)
            {
                using WS_PlayerData.CharacterObject characterObject = @object;
                switch (Action)
                {
                    case AuraAction.AURA_ADD:
                        {
                            ref float ammoMod2 = ref characterObject.AmmoMod;
                            ammoMod2 = (float)(ammoMod2 * (EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100.0 + 1.0));
                            break;
                        }
                    case AuraAction.AURA_REMOVE:
                    case AuraAction.AURA_REMOVEBYDURATION:
                        {
                            ref float ammoMod = ref characterObject.AmmoMod;
                            ammoMod = (float)(ammoMod / (EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100.0 + 1.0));
                            break;
                        }
                    case AuraAction.AURA_UPDATE:
                        return;
                }
                WS_Combat wS_Combat = WorldServiceLocator._WS_Combat;
                WS_PlayerData.CharacterObject objCharacter = @object;
                wS_Combat.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.RANGED_ATTACK);
                characterObject.SendCharacterUpdate(toNear: false);
            }
        }

		public void SPELL_AURA_MOD_ROOT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			switch (Action)
			{
			case AuraAction.AURA_ADD:
				if (Target is WS_PlayerData.CharacterObject @object)
				{
					@object.SetMoveRoot();
					@object.SetUpdateFlag(16, 0L);
				}
				else if (Target is WS_Creatures.CreatureObject object1)
				{
					if (object1.aiScript != null)
					{
						WS_Creatures_AI.TBaseAI aiScript = object1.aiScript;
						WS_Base.BaseUnit Attacker = (WS_Base.BaseUnit)Caster;
						aiScript.OnGenerateHate(ref Attacker, 1);
					}
					object1.StopMoving();
				}
				break;
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
				if (Target is WS_PlayerData.CharacterObject object2)
				{
					object2.SetMoveUnroot();
				}
				else if (Target is WS_Creatures.CreatureObject object3)
				{
					object3.StopMoving();
				}
				break;
			case AuraAction.AURA_UPDATE:
				return;
			}
			if (Target is WS_PlayerData.CharacterObject @object4)
			{
				@object4.SetUpdateFlag(46, Target.cUnitFlags);
				@object4.SendCharacterUpdate();
				return;
			}
			Packets.UpdatePacketClass packet = new Packets.UpdatePacketClass();
			Packets.UpdateClass tmpUpdate = new Packets.UpdateClass(188);
			tmpUpdate.SetUpdateFlag(46, Target.cUnitFlags);
			Packets.PacketClass packet2 = packet;
			WS_Creatures.CreatureObject updateObject = (WS_Creatures.CreatureObject)Target;
			tmpUpdate.AddToPacket(ref packet2, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
			packet = (Packets.UpdatePacketClass)packet2;
			WS_Base.BaseUnit obj = Target;
			packet2 = packet;
			obj.SendToNearPlayers(ref packet2, 0uL);
			packet = (Packets.UpdatePacketClass)packet2;
			tmpUpdate.Dispose();
			packet.Dispose();
		}

		public void SPELL_AURA_MOD_STUN(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			switch (Action)
			{
			case AuraAction.AURA_ADD:
				Target.cUnitFlags |= 0x40000;
				if (Target is WS_PlayerData.CharacterObject @object)
				{
					@object.SetMoveRoot();
					@object.SetUpdateFlag(16, 0uL);
				}
				else if (Target is WS_Creatures.CreatureObject object1)
				{
					object1.StopMoving();
					if (object1.aiScript != null)
					{
						WS_Creatures_AI.TBaseAI aiScript = object1.aiScript;
						WS_Base.BaseUnit Attacker = (WS_Base.BaseUnit)Caster;
						aiScript.OnGenerateHate(ref Attacker, 1);
					}
				}
				break;
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
				Target.cUnitFlags &= -262145;
				if (Target is WS_PlayerData.CharacterObject object2)
				{
					object2.SetMoveUnroot();
				}
				break;
			case AuraAction.AURA_UPDATE:
				return;
			}
			if (Target is WS_PlayerData.CharacterObject @object3)
			{
				@object3.SetUpdateFlag(46, Target.cUnitFlags);
				@object3.SendCharacterUpdate();
				return;
			}
			Packets.UpdatePacketClass packet = new Packets.UpdatePacketClass();
			Packets.UpdateClass tmpUpdate = new Packets.UpdateClass(188);
			tmpUpdate.SetUpdateFlag(46, Target.cUnitFlags);
			Packets.PacketClass packet2 = packet;
			WS_Creatures.CreatureObject updateObject = (WS_Creatures.CreatureObject)Target;
			tmpUpdate.AddToPacket(ref packet2, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
			packet = (Packets.UpdatePacketClass)packet2;
			WS_Base.BaseUnit obj = Target;
			packet2 = packet;
			obj.SendToNearPlayers(ref packet2, 0uL);
			packet = (Packets.UpdatePacketClass)packet2;
			tmpUpdate.Dispose();
			packet.Dispose();
		}

		public void SPELL_AURA_MOD_FEAR(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			Packets.PacketClass response = new Packets.PacketClass(Opcodes.SMSG_DEATH_NOTIFY_OBSOLETE);
			response.AddPackGUID(Target.GUID);
			switch (Action)
			{
			case AuraAction.AURA_ADD:
				Target.cUnitFlags |= 0x800000;
				response.AddInt8(0);
				break;
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
				Target.cUnitFlags &= -8388609;
				response.AddInt8(1);
				break;
			case AuraAction.AURA_UPDATE:
				return;
			}
            switch (Target)
            {
                case WS_PlayerData.CharacterObject _:
                    ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(46, Target.cUnitFlags);
                    ((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate();
                    ((WS_PlayerData.CharacterObject)Target).client.Send(ref response);
                    break;
                default:
                    {
                        Packets.UpdatePacketClass packet = new Packets.UpdatePacketClass();
                        Packets.UpdateClass tmpUpdate = new Packets.UpdateClass(188);
                        tmpUpdate.SetUpdateFlag(46, Target.cUnitFlags);
                        Packets.PacketClass packet2 = packet;
                        WS_Creatures.CreatureObject updateObject = (WS_Creatures.CreatureObject)Target;
                        tmpUpdate.AddToPacket(ref packet2, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
                        WS_Base.BaseUnit obj = Target;
                        packet2 = packet;
                        obj.SendToNearPlayers(ref packet2, 0uL);
                        packet = (Packets.UpdatePacketClass)packet2;
                        tmpUpdate.Dispose();
                        packet.Dispose();
                        break;
                    }
            }
            response.Dispose();
		}

		public void SPELL_AURA_SAFE_FALL(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			switch (Action)
			{
			case AuraAction.AURA_UPDATE:
				break;
			case AuraAction.AURA_ADD:
			{
				Packets.PacketClass packet = new Packets.PacketClass(Opcodes.SMSG_MOVE_FEATHER_FALL);
				packet.AddPackGUID(Target.GUID);
				Target.SendToNearPlayers(ref packet, 0uL);
				packet.Dispose();
				break;
			}
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
			{
				Packets.PacketClass packet2 = new Packets.PacketClass(Opcodes.SMSG_MOVE_NORMAL_FALL);
				packet2.AddPackGUID(Target.GUID);
				Target.SendToNearPlayers(ref packet2, 0uL);
				packet2.Dispose();
				break;
			}
			}
		}

		public void SPELL_AURA_FEATHER_FALL(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			switch (Action)
			{
			case AuraAction.AURA_UPDATE:
				break;
			case AuraAction.AURA_ADD:
			{
				Packets.PacketClass packet = new Packets.PacketClass(Opcodes.SMSG_MOVE_FEATHER_FALL);
				packet.AddPackGUID(Target.GUID);
				Target.SendToNearPlayers(ref packet, 0uL);
				packet.Dispose();
				break;
			}
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
			{
				Packets.PacketClass packet2 = new Packets.PacketClass(Opcodes.SMSG_MOVE_NORMAL_FALL);
				packet2.AddPackGUID(Target.GUID);
				Target.SendToNearPlayers(ref packet2, 0uL);
				packet2.Dispose();
				break;
			}
			}
		}

		public void SPELL_AURA_WATER_WALK(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			switch (Action)
			{
			case AuraAction.AURA_UPDATE:
				break;
			case AuraAction.AURA_ADD:
			{
				Packets.PacketClass packet = new Packets.PacketClass(Opcodes.SMSG_MOVE_WATER_WALK);
				packet.AddPackGUID(Target.GUID);
				Target.SendToNearPlayers(ref packet, 0uL);
				packet.Dispose();
				break;
			}
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
			{
				Packets.PacketClass packet2 = new Packets.PacketClass(Opcodes.SMSG_MOVE_LAND_WALK);
				packet2.AddPackGUID(Target.GUID);
				Target.SendToNearPlayers(ref packet2, 0uL);
				packet2.Dispose();
				break;
			}
			}
		}

		public void SPELL_AURA_HOVER(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			switch (Action)
			{
			case AuraAction.AURA_UPDATE:
				break;
			case AuraAction.AURA_ADD:
			{
				Packets.PacketClass packet = new Packets.PacketClass(Opcodes.SMSG_MOVE_SET_HOVER);
				packet.AddPackGUID(Target.GUID);
				Target.SendToNearPlayers(ref packet, 0uL);
				packet.Dispose();
				break;
			}
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
			{
				Packets.PacketClass packet2 = new Packets.PacketClass(Opcodes.SMSG_MOVE_UNSET_HOVER);
				packet2.AddPackGUID(Target.GUID);
				Target.SendToNearPlayers(ref packet2, 0uL);
				packet2.Dispose();
				break;
			}
			}
		}

		public void SPELL_AURA_WATER_BREATHING(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			if (Target is WS_PlayerData.CharacterObject @object)
			{
				switch (Action)
				{
				case AuraAction.AURA_UPDATE:
					break;
				case AuraAction.AURA_ADD:
					@object.underWaterBreathing = true;
					break;
				case AuraAction.AURA_REMOVE:
				case AuraAction.AURA_REMOVEBYDURATION:
					@object.underWaterBreathing = false;
					break;
				}
			}
		}

		public void SPELL_AURA_ADD_FLAT_MODIFIER(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			if (!(Target is WS_PlayerData.CharacterObject) || EffectInfo.MiscValue > 32)
			{
				return;
			}
			SpellModOp op = (SpellModOp)EffectInfo.MiscValue;
			int value = EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
			int mask = EffectInfo.ItemType;
			checked
			{
				int num;
				switch (Action)
				{
				case AuraAction.AURA_ADD:
				{
					short tmpval = (short)EffectInfo.valueBase;
					uint shiftdata = 1u;
					ushort send_val = default;
					ushort send_mark = default;
					if (tmpval != 0)
					{
						if (tmpval > 0)
						{
							send_val = (ushort)(tmpval + 1);
							send_mark = 0;
						}
						else
						{
							send_val = (ushort)((65535 + (tmpval + 2)) & 0xFFFF);
							send_mark = ushort.MaxValue;
						}
					}
					int eff = 0;
					do
					{
						if ((mask & shiftdata) != 0)
						{
							Packets.PacketClass packet = new Packets.PacketClass(Opcodes.SMSG_SET_FLAT_SPELL_MODIFIER);
							packet.AddInt8((byte)eff);
							packet.AddInt8((byte)op);
							packet.AddUInt16(send_val);
							packet.AddUInt16(send_mark);
							((WS_PlayerData.CharacterObject)Caster).client.Send(ref packet);
							packet.Dispose();
						}
						shiftdata <<= 1;
						eff++;
					}
					while (eff <= 31);
					return;
				}
				default:
					num = ((Action == AuraAction.AURA_REMOVEBYDURATION) ? 1 : 0);
					break;
				case AuraAction.AURA_REMOVE:
					num = 1;
					break;
				}
				if (num == 0)
				{
				}
			}
		}

		public void SPELL_AURA_ADD_PCT_MODIFIER(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			if (!(Target is WS_PlayerData.CharacterObject) || EffectInfo.MiscValue > 32)
			{
				return;
			}
			SpellModOp op = (SpellModOp)EffectInfo.MiscValue;
			int value = EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
			int mask = EffectInfo.ItemType;
			checked
			{
				int num;
				switch (Action)
				{
				case AuraAction.AURA_ADD:
				{
					short tmpval = (short)EffectInfo.valueBase;
					uint shiftdata = 1u;
					ushort send_val = default;
					ushort send_mark = default;
					if (tmpval != 0)
					{
						if (tmpval > 0)
						{
							send_val = (ushort)(tmpval + 1);
							send_mark = 0;
						}
						else
						{
							send_val = (ushort)((65535 + (tmpval + 2)) & 0xFFFF);
							send_mark = ushort.MaxValue;
						}
					}
					int eff = 0;
					do
					{
						if ((mask & shiftdata) != 0)
						{
							Packets.PacketClass packet = new Packets.PacketClass(Opcodes.SMSG_SET_PCT_SPELL_MODIFIER);
							packet.AddInt8((byte)eff);
							packet.AddInt8((byte)op);
							packet.AddUInt16(send_val);
							packet.AddUInt16(send_mark);
							((WS_PlayerData.CharacterObject)Caster).client.Send(ref packet);
							packet.Dispose();
						}
						shiftdata <<= 1;
						eff++;
					}
					while (eff <= 31);
					return;
				}
				default:
					num = ((Action == AuraAction.AURA_REMOVEBYDURATION) ? 1 : 0);
					break;
				case AuraAction.AURA_REMOVE:
					num = 1;
					break;
				}
				if (num == 0)
				{
				}
			}
		}

		public void SPELL_AURA_MOD_STAT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			if (Action == AuraAction.AURA_UPDATE || !(Target is WS_PlayerData.CharacterObject))
			{
				return;
			}
			int value = EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
			int value_sign = value;
			checked
			{
				if (Action == AuraAction.AURA_REMOVE)
				{
					value = -value;
				}
				switch (EffectInfo.MiscValue)
				{
				case -1:
				{
					ref int base9 = ref ((WS_PlayerData.CharacterObject)Target).Strength.Base;
					base9 = (int)Math.Round(base9 / ((WS_PlayerData.CharacterObject)Target).Strength.Modifier);
					((WS_PlayerData.CharacterObject)Target).Strength.Base += value;
					ref int base10 = ref ((WS_PlayerData.CharacterObject)Target).Strength.Base;
					base10 = (int)Math.Round(base10 * ((WS_PlayerData.CharacterObject)Target).Strength.Modifier);
					ref int base11 = ref ((WS_PlayerData.CharacterObject)Target).Agility.Base;
					base11 = (int)Math.Round(base11 / ((WS_PlayerData.CharacterObject)Target).Agility.Modifier);
					((WS_PlayerData.CharacterObject)Target).Agility.Base += value;
					ref int base12 = ref ((WS_PlayerData.CharacterObject)Target).Agility.Base;
					base12 = (int)Math.Round(base12 * ((WS_PlayerData.CharacterObject)Target).Agility.Modifier);
					ref int base13 = ref ((WS_PlayerData.CharacterObject)Target).Stamina.Base;
					base13 = (int)Math.Round(base13 / ((WS_PlayerData.CharacterObject)Target).Stamina.Modifier);
					((WS_PlayerData.CharacterObject)Target).Stamina.Base += value;
					ref int base14 = ref ((WS_PlayerData.CharacterObject)Target).Stamina.Base;
					base14 = (int)Math.Round(base14 * ((WS_PlayerData.CharacterObject)Target).Stamina.Modifier);
					ref int base15 = ref ((WS_PlayerData.CharacterObject)Target).Spirit.Base;
					base15 = (int)Math.Round(base15 / ((WS_PlayerData.CharacterObject)Target).Spirit.Modifier);
					((WS_PlayerData.CharacterObject)Target).Spirit.Base += value;
					ref int base16 = ref ((WS_PlayerData.CharacterObject)Target).Spirit.Base;
					base16 = (int)Math.Round(base16 * ((WS_PlayerData.CharacterObject)Target).Spirit.Modifier);
					ref int base17 = ref ((WS_PlayerData.CharacterObject)Target).Intellect.Base;
					base17 = (int)Math.Round(base17 / ((WS_PlayerData.CharacterObject)Target).Intellect.Modifier);
					((WS_PlayerData.CharacterObject)Target).Intellect.Base += value;
					ref int base18 = ref ((WS_PlayerData.CharacterObject)Target).Intellect.Base;
					base18 = (int)Math.Round(base18 * ((WS_PlayerData.CharacterObject)Target).Intellect.Modifier);
					if (value_sign > 0)
					{
						ref short positiveBonus5 = ref ((WS_PlayerData.CharacterObject)Target).Strength.PositiveBonus;
						positiveBonus5 = (short)(positiveBonus5 + value);
						ref short positiveBonus6 = ref ((WS_PlayerData.CharacterObject)Target).Agility.PositiveBonus;
						positiveBonus6 = (short)(positiveBonus6 + value);
						ref short positiveBonus7 = ref ((WS_PlayerData.CharacterObject)Target).Stamina.PositiveBonus;
						positiveBonus7 = (short)(positiveBonus7 + value);
						ref short positiveBonus8 = ref ((WS_PlayerData.CharacterObject)Target).Spirit.PositiveBonus;
						positiveBonus8 = (short)(positiveBonus8 + value);
						ref short positiveBonus9 = ref ((WS_PlayerData.CharacterObject)Target).Intellect.PositiveBonus;
						positiveBonus9 = (short)(positiveBonus9 + value);
					}
					else
					{
						ref short negativeBonus5 = ref ((WS_PlayerData.CharacterObject)Target).Strength.NegativeBonus;
						negativeBonus5 = (short)(negativeBonus5 - value);
						ref short negativeBonus6 = ref ((WS_PlayerData.CharacterObject)Target).Agility.NegativeBonus;
						negativeBonus6 = (short)(negativeBonus6 - value);
						ref short negativeBonus7 = ref ((WS_PlayerData.CharacterObject)Target).Stamina.NegativeBonus;
						negativeBonus7 = (short)(negativeBonus7 - value);
						ref short negativeBonus8 = ref ((WS_PlayerData.CharacterObject)Target).Spirit.NegativeBonus;
						negativeBonus8 = (short)(negativeBonus8 - value);
						ref short negativeBonus9 = ref ((WS_PlayerData.CharacterObject)Target).Intellect.NegativeBonus;
						negativeBonus9 = (short)(negativeBonus9 - value);
					}
					break;
				}
				case 0:
				{
					ref int base5 = ref ((WS_PlayerData.CharacterObject)Target).Strength.Base;
					base5 = (int)Math.Round(base5 / ((WS_PlayerData.CharacterObject)Target).Strength.Modifier);
					((WS_PlayerData.CharacterObject)Target).Strength.Base += value;
					ref int base6 = ref ((WS_PlayerData.CharacterObject)Target).Strength.Base;
					base6 = (int)Math.Round(base6 * ((WS_PlayerData.CharacterObject)Target).Strength.Modifier);
					if (value_sign > 0)
					{
						ref short positiveBonus3 = ref ((WS_PlayerData.CharacterObject)Target).Strength.PositiveBonus;
						positiveBonus3 = (short)(positiveBonus3 + value);
					}
					else
					{
						ref short negativeBonus3 = ref ((WS_PlayerData.CharacterObject)Target).Strength.NegativeBonus;
						negativeBonus3 = (short)(negativeBonus3 - value);
					}
					break;
				}
				case 1:
				{
					ref int base19 = ref ((WS_PlayerData.CharacterObject)Target).Agility.Base;
					base19 = (int)Math.Round(base19 / ((WS_PlayerData.CharacterObject)Target).Agility.Modifier);
					((WS_PlayerData.CharacterObject)Target).Agility.Base += value;
					ref int base20 = ref ((WS_PlayerData.CharacterObject)Target).Agility.Base;
					base20 = (int)Math.Round(base20 * ((WS_PlayerData.CharacterObject)Target).Agility.Modifier);
					if (value_sign > 0)
					{
						ref short positiveBonus10 = ref ((WS_PlayerData.CharacterObject)Target).Agility.PositiveBonus;
						positiveBonus10 = (short)(positiveBonus10 + value);
					}
					else
					{
						ref short negativeBonus10 = ref ((WS_PlayerData.CharacterObject)Target).Agility.NegativeBonus;
						negativeBonus10 = (short)(negativeBonus10 - value);
					}
					break;
				}
				case 2:
				{
					ref int base3 = ref ((WS_PlayerData.CharacterObject)Target).Stamina.Base;
					base3 = (int)Math.Round(base3 / ((WS_PlayerData.CharacterObject)Target).Stamina.Modifier);
					((WS_PlayerData.CharacterObject)Target).Stamina.Base += value;
					ref int base4 = ref ((WS_PlayerData.CharacterObject)Target).Stamina.Base;
					base4 = (int)Math.Round(base4 * ((WS_PlayerData.CharacterObject)Target).Stamina.Modifier);
					if (value_sign > 0)
					{
						ref short positiveBonus2 = ref ((WS_PlayerData.CharacterObject)Target).Stamina.PositiveBonus;
						positiveBonus2 = (short)(positiveBonus2 + value);
					}
					else
					{
						ref short negativeBonus2 = ref ((WS_PlayerData.CharacterObject)Target).Stamina.NegativeBonus;
						negativeBonus2 = (short)(negativeBonus2 - value);
					}
					break;
				}
				case 3:
				{
					ref int base7 = ref ((WS_PlayerData.CharacterObject)Target).Intellect.Base;
					base7 = (int)Math.Round(base7 / ((WS_PlayerData.CharacterObject)Target).Intellect.Modifier);
					((WS_PlayerData.CharacterObject)Target).Intellect.Base += value;
					ref int base8 = ref ((WS_PlayerData.CharacterObject)Target).Intellect.Base;
					base8 = (int)Math.Round(base8 * ((WS_PlayerData.CharacterObject)Target).Intellect.Modifier);
					if (value_sign > 0)
					{
						ref short positiveBonus4 = ref ((WS_PlayerData.CharacterObject)Target).Intellect.PositiveBonus;
						positiveBonus4 = (short)(positiveBonus4 + value);
					}
					else
					{
						ref short negativeBonus4 = ref ((WS_PlayerData.CharacterObject)Target).Intellect.NegativeBonus;
						negativeBonus4 = (short)(negativeBonus4 - value);
					}
					break;
				}
				case 4:
				{
					ref int @base = ref ((WS_PlayerData.CharacterObject)Target).Spirit.Base;
					@base = (int)Math.Round(@base / ((WS_PlayerData.CharacterObject)Target).Spirit.Modifier);
					((WS_PlayerData.CharacterObject)Target).Spirit.Base += value;
					ref int base2 = ref ((WS_PlayerData.CharacterObject)Target).Spirit.Base;
					base2 = (int)Math.Round(base2 * ((WS_PlayerData.CharacterObject)Target).Spirit.Modifier);
					if (value_sign > 0)
					{
						ref short positiveBonus = ref ((WS_PlayerData.CharacterObject)Target).Spirit.PositiveBonus;
						positiveBonus = (short)(positiveBonus + value);
					}
					else
					{
						ref short negativeBonus = ref ((WS_PlayerData.CharacterObject)Target).Spirit.NegativeBonus;
						negativeBonus = (short)(negativeBonus - value);
					}
					break;
				}
				}
				((WS_PlayerData.CharacterObject)Target).Life.Bonus = (((WS_PlayerData.CharacterObject)Target).Stamina.Base - 18) * 10;
				((WS_PlayerData.CharacterObject)Target).Mana.Bonus = (((WS_PlayerData.CharacterObject)Target).Intellect.Base - 18) * 15;
				((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag = ((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag | 4u | 0x20u;
				((WS_PlayerData.CharacterObject)Target).Resistances[0].Base += value * 2;
				((WS_PlayerData.CharacterObject)Target).UpdateManaRegen();
				WS_Combat wS_Combat = WorldServiceLocator._WS_Combat;
				WS_PlayerData.CharacterObject objCharacter = (WS_PlayerData.CharacterObject)Target;
				wS_Combat.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.BASE_ATTACK);
				WS_Combat wS_Combat2 = WorldServiceLocator._WS_Combat;
				objCharacter = (WS_PlayerData.CharacterObject)Target;
				wS_Combat2.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.OFF_ATTACK);
				WS_Combat wS_Combat3 = WorldServiceLocator._WS_Combat;
				objCharacter = (WS_PlayerData.CharacterObject)Target;
				wS_Combat3.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.RANGED_ATTACK);
				((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(150, ((WS_PlayerData.CharacterObject)Target).Strength.Base);
				((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(151, ((WS_PlayerData.CharacterObject)Target).Agility.Base);
				((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(152, ((WS_PlayerData.CharacterObject)Target).Stamina.Base);
				((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(153, ((WS_PlayerData.CharacterObject)Target).Spirit.Base);
				((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(154, ((WS_PlayerData.CharacterObject)Target).Intellect.Base);
				((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(22, ((WS_PlayerData.CharacterObject)Target).Life.Current);
				((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(28, ((WS_PlayerData.CharacterObject)Target).Life.Maximum);
				if (WorldServiceLocator._WS_Player_Initializator.GetClassManaType(((WS_PlayerData.CharacterObject)Target).Classe) == ManaTypes.TYPE_MANA)
				{
					((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(23, ((WS_PlayerData.CharacterObject)Target).Mana.Current);
					((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(29, ((WS_PlayerData.CharacterObject)Target).Mana.Maximum);
				}
				((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(155, ((WS_PlayerData.CharacterObject)Target).Resistances[0].Base);
				((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate(toNear: false);
				((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag = ((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag | 4u;
				((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag = ((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag | 0x20u;
			}
		}

		public void SPELL_AURA_MOD_STAT_PERCENT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			if (Action != AuraAction.AURA_UPDATE && Target is WS_PlayerData.CharacterObject @object)
			{
				float value = (float)(EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100.0);
				int value_sign = EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
				if (Action == AuraAction.AURA_REMOVE)
				{
					value = 0f - value;
				}
				checked
				{
					short OldStr = (short)@object.Strength.Base;
					short OldAgi = (short)@object.Agility.Base;
					short OldSta = (short)@object.Stamina.Base;
					short OldSpi = (short)@object.Spirit.Base;
					short OldInt = (short)@object.Intellect.Base;
					switch (EffectInfo.MiscValue)
					{
					case -1:
					{
						WS_PlayerHelper.TStat spirit;
						(spirit = @object.Strength).RealBase = (int)Math.Round(spirit.RealBase / @object.Strength.BaseModifier);
						@object.Strength.BaseModifier += value;
						(spirit = @object.Strength).RealBase = (int)Math.Round(spirit.RealBase * @object.Strength.BaseModifier);
						(spirit = @object.Agility).RealBase = (int)Math.Round(spirit.RealBase / @object.Agility.BaseModifier);
						@object.Agility.BaseModifier += value;
						(spirit = @object.Agility).RealBase = (int)Math.Round(spirit.RealBase * @object.Agility.BaseModifier);
						(spirit = @object.Stamina).RealBase = (int)Math.Round(spirit.RealBase / @object.Stamina.BaseModifier);
						@object.Stamina.BaseModifier += value;
						(spirit = @object.Stamina).RealBase = (int)Math.Round(spirit.RealBase * @object.Stamina.BaseModifier);
						(spirit = @object.Spirit).RealBase = (int)Math.Round(spirit.RealBase / @object.Spirit.BaseModifier);
						@object.Spirit.BaseModifier += value;
						(spirit = @object.Spirit).RealBase = (int)Math.Round(spirit.RealBase * @object.Spirit.BaseModifier);
						(spirit = @object.Intellect).RealBase = (int)Math.Round(spirit.RealBase / @object.Intellect.BaseModifier);
						@object.Intellect.BaseModifier += value;
						(spirit = @object.Intellect).RealBase = (int)Math.Round(spirit.RealBase * @object.Intellect.BaseModifier);
						break;
					}
					case 0:
					{
						WS_PlayerHelper.TStat spirit;
						(spirit = @object.Strength).RealBase = (int)Math.Round(spirit.RealBase / @object.Strength.BaseModifier);
						@object.Strength.BaseModifier += value;
						(spirit = @object.Strength).RealBase = (int)Math.Round(spirit.RealBase * @object.Strength.BaseModifier);
						break;
					}
					case 1:
					{
						WS_PlayerHelper.TStat spirit;
						(spirit = @object.Agility).RealBase = (int)Math.Round(spirit.RealBase / @object.Agility.BaseModifier);
						@object.Agility.BaseModifier += value;
						(spirit = @object.Agility).RealBase = (int)Math.Round(spirit.RealBase * @object.Agility.BaseModifier);
						break;
					}
					case 2:
					{
						WS_PlayerHelper.TStat spirit;
						(spirit = @object.Stamina).RealBase = (int)Math.Round(spirit.RealBase / @object.Stamina.BaseModifier);
						@object.Stamina.BaseModifier += value;
						(spirit = @object.Stamina).RealBase = (int)Math.Round(spirit.RealBase * @object.Stamina.BaseModifier);
						break;
					}
					case 3:
					{
						WS_PlayerHelper.TStat spirit;
						(spirit = @object.Intellect).RealBase = (int)Math.Round(spirit.RealBase / @object.Intellect.BaseModifier);
						@object.Intellect.BaseModifier += value;
						(spirit = @object.Intellect).RealBase = (int)Math.Round(spirit.RealBase * @object.Intellect.BaseModifier);
						break;
					}
					case 4:
					{
						WS_PlayerHelper.TStat spirit;
						(spirit = @object.Spirit).RealBase = (int)Math.Round(spirit.RealBase / @object.Spirit.BaseModifier);
						@object.Spirit.BaseModifier += value;
						(spirit = @object.Spirit).RealBase = (int)Math.Round(spirit.RealBase * @object.Spirit.BaseModifier);
						break;
					}
					}
					@object.Life.Bonus += (@object.Stamina.Base - OldSta) * 10;
					@object.Mana.Bonus += (@object.Intellect.Base - OldInt) * 15;
					@object.Resistances[0].Base += (@object.Agility.Base - OldAgi) * 2;
					@object.GroupUpdateFlag = @object.GroupUpdateFlag | 4u | 0x20u;
					@object.UpdateManaRegen();
					WS_Combat wS_Combat = WorldServiceLocator._WS_Combat;
					WS_PlayerData.CharacterObject objCharacter = @object;
					wS_Combat.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.BASE_ATTACK);
					WS_Combat wS_Combat2 = WorldServiceLocator._WS_Combat;
					objCharacter = @object;
					wS_Combat2.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.OFF_ATTACK);
					WS_Combat wS_Combat3 = WorldServiceLocator._WS_Combat;
					objCharacter = @object;
					wS_Combat3.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.RANGED_ATTACK);
					@object.SetUpdateFlag(150, @object.Strength.Base);
					@object.SetUpdateFlag(151, @object.Agility.Base);
					@object.SetUpdateFlag(152, @object.Stamina.Base);
					@object.SetUpdateFlag(153, @object.Spirit.Base);
					@object.SetUpdateFlag(154, @object.Intellect.Base);
					@object.SetUpdateFlag(22, @object.Life.Current);
					@object.SetUpdateFlag(28, @object.Life.Maximum);
					if (WorldServiceLocator._WS_Player_Initializator.GetClassManaType(@object.Classe) == ManaTypes.TYPE_MANA)
					{
						@object.SetUpdateFlag(23, @object.Mana.Current);
						@object.SetUpdateFlag(29, @object.Mana.Maximum);
					}
					@object.SetUpdateFlag(155, @object.Resistances[0].Base);
					@object.SendCharacterUpdate(toNear: false);
					@object.GroupUpdateFlag |= 4u;
					@object.GroupUpdateFlag |= 0x20u;
				}
			}
		}

		public void SPELL_AURA_MOD_TOTAL_STAT_PERCENTAGE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			if (Action == AuraAction.AURA_UPDATE || !(Target is WS_PlayerData.CharacterObject))
			{
				return;
			}
			float value = (float)(EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100.0);
			int value_sign = EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
			if (Action == AuraAction.AURA_REMOVE)
			{
				value = 0f - value;
			}
			checked
			{
				short OldStr = (short)((WS_PlayerData.CharacterObject)Target).Strength.Base;
				short OldAgi = (short)((WS_PlayerData.CharacterObject)Target).Agility.Base;
				short OldSta = (short)((WS_PlayerData.CharacterObject)Target).Stamina.Base;
				short OldSpi = (short)((WS_PlayerData.CharacterObject)Target).Spirit.Base;
				short OldInt = (short)((WS_PlayerData.CharacterObject)Target).Intellect.Base;
				switch (EffectInfo.MiscValue)
				{
				case -1:
				{
					ref int base3 = ref ((WS_PlayerData.CharacterObject)Target).Strength.Base;
					base3 = (int)Math.Round(base3 / ((WS_PlayerData.CharacterObject)Target).Strength.Modifier);
					((WS_PlayerData.CharacterObject)Target).Strength.Modifier += value;
					ref int base4 = ref ((WS_PlayerData.CharacterObject)Target).Strength.Base;
					base4 = (int)Math.Round(base4 * ((WS_PlayerData.CharacterObject)Target).Strength.Modifier);
					ref int base5 = ref ((WS_PlayerData.CharacterObject)Target).Agility.Base;
					base5 = (int)Math.Round(base5 / ((WS_PlayerData.CharacterObject)Target).Agility.Modifier);
					((WS_PlayerData.CharacterObject)Target).Agility.Modifier += value;
					ref int base6 = ref ((WS_PlayerData.CharacterObject)Target).Agility.Base;
					base6 = (int)Math.Round(base6 * ((WS_PlayerData.CharacterObject)Target).Agility.Modifier);
					ref int base7 = ref ((WS_PlayerData.CharacterObject)Target).Stamina.Base;
					base7 = (int)Math.Round(base7 / ((WS_PlayerData.CharacterObject)Target).Stamina.Modifier);
					((WS_PlayerData.CharacterObject)Target).Stamina.Modifier += value;
					ref int base8 = ref ((WS_PlayerData.CharacterObject)Target).Stamina.Base;
					base8 = (int)Math.Round(base8 * ((WS_PlayerData.CharacterObject)Target).Stamina.Modifier);
					ref int base9 = ref ((WS_PlayerData.CharacterObject)Target).Spirit.Base;
					base9 = (int)Math.Round(base9 / ((WS_PlayerData.CharacterObject)Target).Spirit.Modifier);
					((WS_PlayerData.CharacterObject)Target).Spirit.Modifier += value;
					ref int base10 = ref ((WS_PlayerData.CharacterObject)Target).Spirit.Base;
					base10 = (int)Math.Round(base10 * ((WS_PlayerData.CharacterObject)Target).Spirit.Modifier);
					ref int base11 = ref ((WS_PlayerData.CharacterObject)Target).Intellect.Base;
					base11 = (int)Math.Round(base11 / ((WS_PlayerData.CharacterObject)Target).Intellect.Modifier);
					((WS_PlayerData.CharacterObject)Target).Intellect.Modifier += value;
					ref int base12 = ref ((WS_PlayerData.CharacterObject)Target).Intellect.Base;
					base12 = (int)Math.Round(base12 * ((WS_PlayerData.CharacterObject)Target).Intellect.Modifier);
					if (value_sign > 0)
					{
						ref short positiveBonus2 = ref ((WS_PlayerData.CharacterObject)Target).Strength.PositiveBonus;
						positiveBonus2 = (short)(positiveBonus2 + (((WS_PlayerData.CharacterObject)Target).Strength.Base - OldStr));
						ref short positiveBonus3 = ref ((WS_PlayerData.CharacterObject)Target).Agility.PositiveBonus;
						positiveBonus3 = (short)(positiveBonus3 + (((WS_PlayerData.CharacterObject)Target).Agility.Base - OldAgi));
						ref short positiveBonus4 = ref ((WS_PlayerData.CharacterObject)Target).Stamina.PositiveBonus;
						positiveBonus4 = (short)(positiveBonus4 + (((WS_PlayerData.CharacterObject)Target).Stamina.Base - OldSta));
						ref short positiveBonus5 = ref ((WS_PlayerData.CharacterObject)Target).Spirit.PositiveBonus;
						positiveBonus5 = (short)(positiveBonus5 + (((WS_PlayerData.CharacterObject)Target).Spirit.Base - OldSpi));
						ref short positiveBonus6 = ref ((WS_PlayerData.CharacterObject)Target).Intellect.PositiveBonus;
						positiveBonus6 = (short)(positiveBonus6 + (((WS_PlayerData.CharacterObject)Target).Intellect.Base - OldInt));
					}
					else
					{
						ref short negativeBonus2 = ref ((WS_PlayerData.CharacterObject)Target).Strength.NegativeBonus;
						negativeBonus2 = (short)(negativeBonus2 - (((WS_PlayerData.CharacterObject)Target).Strength.Base - OldStr));
						ref short negativeBonus3 = ref ((WS_PlayerData.CharacterObject)Target).Agility.NegativeBonus;
						negativeBonus3 = (short)(negativeBonus3 - (((WS_PlayerData.CharacterObject)Target).Agility.Base - OldAgi));
						ref short negativeBonus4 = ref ((WS_PlayerData.CharacterObject)Target).Stamina.NegativeBonus;
						negativeBonus4 = (short)(negativeBonus4 - (((WS_PlayerData.CharacterObject)Target).Stamina.Base - OldSta));
						ref short negativeBonus5 = ref ((WS_PlayerData.CharacterObject)Target).Spirit.NegativeBonus;
						negativeBonus5 = (short)(negativeBonus5 - (((WS_PlayerData.CharacterObject)Target).Spirit.Base - OldSpi));
						ref short negativeBonus6 = ref ((WS_PlayerData.CharacterObject)Target).Intellect.NegativeBonus;
						negativeBonus6 = (short)(negativeBonus6 - (((WS_PlayerData.CharacterObject)Target).Intellect.Base - OldInt));
					}
					break;
				}
				case 0:
				{
					ref int base17 = ref ((WS_PlayerData.CharacterObject)Target).Strength.Base;
					base17 = (int)Math.Round(base17 / ((WS_PlayerData.CharacterObject)Target).Strength.Modifier);
					((WS_PlayerData.CharacterObject)Target).Strength.Modifier += value;
					ref int base18 = ref ((WS_PlayerData.CharacterObject)Target).Strength.Base;
					base18 = (int)Math.Round(base18 * ((WS_PlayerData.CharacterObject)Target).Strength.Modifier);
					if (value_sign > 0)
					{
						ref short positiveBonus9 = ref ((WS_PlayerData.CharacterObject)Target).Strength.PositiveBonus;
						positiveBonus9 = (short)(positiveBonus9 + (((WS_PlayerData.CharacterObject)Target).Strength.Base - OldStr));
					}
					else
					{
						ref short negativeBonus9 = ref ((WS_PlayerData.CharacterObject)Target).Strength.NegativeBonus;
						negativeBonus9 = (short)(negativeBonus9 - (((WS_PlayerData.CharacterObject)Target).Strength.Base - OldStr));
					}
					break;
				}
				case 1:
				{
					ref int base13 = ref ((WS_PlayerData.CharacterObject)Target).Agility.Base;
					base13 = (int)Math.Round(base13 / ((WS_PlayerData.CharacterObject)Target).Agility.Modifier);
					((WS_PlayerData.CharacterObject)Target).Agility.Modifier += value;
					ref int base14 = ref ((WS_PlayerData.CharacterObject)Target).Agility.Base;
					base14 = (int)Math.Round(base14 * ((WS_PlayerData.CharacterObject)Target).Agility.Modifier);
					if (value_sign > 0)
					{
						ref short positiveBonus7 = ref ((WS_PlayerData.CharacterObject)Target).Agility.PositiveBonus;
						positiveBonus7 = (short)(positiveBonus7 + (((WS_PlayerData.CharacterObject)Target).Agility.Base - OldAgi));
					}
					else
					{
						ref short negativeBonus7 = ref ((WS_PlayerData.CharacterObject)Target).Agility.NegativeBonus;
						negativeBonus7 = (short)(negativeBonus7 - (((WS_PlayerData.CharacterObject)Target).Agility.Base - OldAgi));
					}
					break;
				}
				case 2:
				{
					ref int base15 = ref ((WS_PlayerData.CharacterObject)Target).Stamina.Base;
					base15 = (int)Math.Round(base15 / ((WS_PlayerData.CharacterObject)Target).Stamina.Modifier);
					((WS_PlayerData.CharacterObject)Target).Stamina.Modifier += value;
					ref int base16 = ref ((WS_PlayerData.CharacterObject)Target).Stamina.Base;
					base16 = (int)Math.Round(base16 * ((WS_PlayerData.CharacterObject)Target).Stamina.Modifier);
					if (value_sign > 0)
					{
						ref short positiveBonus8 = ref ((WS_PlayerData.CharacterObject)Target).Stamina.PositiveBonus;
						positiveBonus8 = (short)(positiveBonus8 + (((WS_PlayerData.CharacterObject)Target).Stamina.Base - OldSta));
					}
					else
					{
						ref short negativeBonus8 = ref ((WS_PlayerData.CharacterObject)Target).Stamina.NegativeBonus;
						negativeBonus8 = (short)(negativeBonus8 - (((WS_PlayerData.CharacterObject)Target).Stamina.Base - OldSta));
					}
					break;
				}
				case 4:
				{
					ref int @base = ref ((WS_PlayerData.CharacterObject)Target).Spirit.Base;
					@base = (int)Math.Round(@base / ((WS_PlayerData.CharacterObject)Target).Spirit.Modifier);
					((WS_PlayerData.CharacterObject)Target).Spirit.Modifier += value;
					ref int base2 = ref ((WS_PlayerData.CharacterObject)Target).Spirit.Base;
					base2 = (int)Math.Round(base2 * ((WS_PlayerData.CharacterObject)Target).Spirit.Modifier);
					if (value_sign > 0)
					{
						ref short positiveBonus = ref ((WS_PlayerData.CharacterObject)Target).Spirit.PositiveBonus;
						positiveBonus = (short)(positiveBonus + (((WS_PlayerData.CharacterObject)Target).Spirit.Base - OldSpi));
					}
					else
					{
						ref short negativeBonus = ref ((WS_PlayerData.CharacterObject)Target).Spirit.NegativeBonus;
						negativeBonus = (short)(negativeBonus - (((WS_PlayerData.CharacterObject)Target).Spirit.Base - OldSpi));
					}
					break;
				}
				}
				((WS_PlayerData.CharacterObject)Target).Life.Bonus = (((WS_PlayerData.CharacterObject)Target).Stamina.Base - 18) * 10;
				((WS_PlayerData.CharacterObject)Target).Mana.Bonus = (((WS_PlayerData.CharacterObject)Target).Intellect.Base - 18) * 15;
				((WS_PlayerData.CharacterObject)Target).Resistances[0].Base += (((WS_PlayerData.CharacterObject)Target).Agility.Base - OldAgi) * 2;
				((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag = ((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag | 4u | 0x20u;
				((WS_PlayerData.CharacterObject)Target).UpdateManaRegen();
				WS_Combat wS_Combat = WorldServiceLocator._WS_Combat;
				WS_PlayerData.CharacterObject objCharacter = (WS_PlayerData.CharacterObject)Target;
				wS_Combat.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.BASE_ATTACK);
				WS_Combat wS_Combat2 = WorldServiceLocator._WS_Combat;
				objCharacter = (WS_PlayerData.CharacterObject)Target;
				wS_Combat2.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.OFF_ATTACK);
				WS_Combat wS_Combat3 = WorldServiceLocator._WS_Combat;
				objCharacter = (WS_PlayerData.CharacterObject)Target;
				wS_Combat3.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.RANGED_ATTACK);
				((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(150, ((WS_PlayerData.CharacterObject)Target).Strength.Base);
				((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(151, ((WS_PlayerData.CharacterObject)Target).Agility.Base);
				((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(152, ((WS_PlayerData.CharacterObject)Target).Stamina.Base);
				((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(153, ((WS_PlayerData.CharacterObject)Target).Spirit.Base);
				((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(154, ((WS_PlayerData.CharacterObject)Target).Intellect.Base);
				((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(22, ((WS_PlayerData.CharacterObject)Target).Life.Current);
				((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(28, ((WS_PlayerData.CharacterObject)Target).Life.Maximum);
				if (WorldServiceLocator._WS_Player_Initializator.GetClassManaType(((WS_PlayerData.CharacterObject)Target).Classe) == ManaTypes.TYPE_MANA)
				{
					((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(23, ((WS_PlayerData.CharacterObject)Target).Mana.Current);
					((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(29, ((WS_PlayerData.CharacterObject)Target).Mana.Maximum);
				}
				((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(155, ((WS_PlayerData.CharacterObject)Target).Resistances[0].Base);
				((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate(toNear: false);
				((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag = ((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag | 4u;
				((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag = ((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag | 0x20u;
			}
		}

		public void SPELL_AURA_MOD_INCREASE_HEALTH(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			checked
			{
				switch (Action)
				{
				case AuraAction.AURA_UPDATE:
					return;
				case AuraAction.AURA_ADD:
					Target.Life.Bonus += EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0);
					break;
				case AuraAction.AURA_REMOVE:
				case AuraAction.AURA_REMOVEBYDURATION:
					Target.Life.Bonus -= EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0);
					break;
				}
				if (Target is WS_PlayerData.CharacterObject @object)
				{
					@object.SetUpdateFlag(28, Target.Life.Maximum);
					@object.SendCharacterUpdate();
					@object.GroupUpdateFlag |= 4u;
					return;
				}
				Packets.UpdatePacketClass packet = new Packets.UpdatePacketClass();
				Packets.UpdateClass UpdateData = new Packets.UpdateClass(188);
				UpdateData.SetUpdateFlag(28, Target.Life.Maximum);
				Packets.PacketClass packet2 = packet;
				WS_Creatures.CreatureObject updateObject = (WS_Creatures.CreatureObject)Target;
				UpdateData.AddToPacket(ref packet2, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
				packet = (Packets.UpdatePacketClass)packet2;
				WS_Creatures.CreatureObject obj = (WS_Creatures.CreatureObject)Target;
				packet2 = packet;
				obj.SendToNearPlayers(ref packet2, 0uL);
				packet = (Packets.UpdatePacketClass)packet2;
				packet.Dispose();
				UpdateData.Dispose();
			}
		}

		public void SPELL_AURA_MOD_INCREASE_HEALTH_PERCENT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			switch (Action)
			{
			case AuraAction.AURA_UPDATE:
				return;
			case AuraAction.AURA_ADD:
			{
				ref float modifier2 = ref Target.Life.Modifier;
				modifier2 = (float)(modifier2 + EffectInfo.GetValue(Target.Level, 0) / 100.0);
				break;
			}
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
			{
				ref float modifier = ref Target.Life.Modifier;
				modifier = (float)(modifier - EffectInfo.GetValue(Target.Level, 0) / 100.0);
				break;
			}
			}
			if (Target is WS_PlayerData.CharacterObject @object)
			{
				@object.SetUpdateFlag(28, Target.Life.Maximum);
				@object.SendCharacterUpdate();
				@object.GroupUpdateFlag |= 4u;
				return;
			}
			Packets.UpdatePacketClass packet = new Packets.UpdatePacketClass();
			Packets.UpdateClass UpdateData = new Packets.UpdateClass(188);
			UpdateData.SetUpdateFlag(28, Target.Life.Maximum);
			Packets.PacketClass packet2 = packet;
			WS_Creatures.CreatureObject updateObject = (WS_Creatures.CreatureObject)Target;
			UpdateData.AddToPacket(ref packet2, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
			packet = (Packets.UpdatePacketClass)packet2;
			WS_Creatures.CreatureObject obj = (WS_Creatures.CreatureObject)Target;
			packet2 = packet;
			obj.SendToNearPlayers(ref packet2, 0uL);
			packet = (Packets.UpdatePacketClass)packet2;
			packet.Dispose();
			UpdateData.Dispose();
		}

		public void SPELL_AURA_MOD_INCREASE_ENERGY(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			switch (Action)
			{
			case AuraAction.AURA_UPDATE:
				return;
			case AuraAction.AURA_ADD:
				if (EffectInfo.MiscValue != (int)Target.ManaType)
				{
					break;
				}
				checked
				{
					if (!(Target is WS_PlayerData.CharacterObject))
					{
						Target.Mana.Bonus += EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0);
						break;
					}
					switch (Target.ManaType)
					{
					case ManaTypes.TYPE_ENERGY:
						((WS_PlayerData.CharacterObject)Target).Energy.Bonus += EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0);
						break;
					case ManaTypes.TYPE_MANA:
						((WS_PlayerData.CharacterObject)Target).Mana.Bonus += EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0);
						break;
					case ManaTypes.TYPE_RAGE:
						((WS_PlayerData.CharacterObject)Target).Rage.Bonus += EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0);
						break;
					}
					break;
				}
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
				if (EffectInfo.MiscValue != (int)Target.ManaType)
				{
					break;
				}
				checked
				{
					if (!(Target is WS_PlayerData.CharacterObject))
					{
						Target.Mana.Bonus -= EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0);
						break;
					}
					switch (Target.ManaType)
					{
					case ManaTypes.TYPE_ENERGY:
						((WS_PlayerData.CharacterObject)Target).Energy.Bonus -= EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0);
						break;
					case ManaTypes.TYPE_MANA:
						((WS_PlayerData.CharacterObject)Target).Mana.Bonus -= EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0);
						break;
					case ManaTypes.TYPE_RAGE:
						((WS_PlayerData.CharacterObject)Target).Rage.Bonus -= EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0);
						break;
					}
					break;
				}
			}
			if (Target is WS_PlayerData.CharacterObject @object)
			{
				@object.SetUpdateFlag(32, @object.Energy.Maximum);
				@object.SetUpdateFlag(29, @object.Mana.Maximum);
				@object.SetUpdateFlag(30, @object.Rage.Maximum);
				return;
			}
			Packets.UpdatePacketClass packet = new Packets.UpdatePacketClass();
			Packets.UpdateClass UpdateData = new Packets.UpdateClass(188);
			UpdateData.SetUpdateFlag((int)checked(29 + Target.ManaType), Target.Mana.Maximum);
			Packets.PacketClass packet2 = packet;
			WS_Creatures.CreatureObject updateObject = (WS_Creatures.CreatureObject)Target;
			UpdateData.AddToPacket(ref packet2, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
			packet = (Packets.UpdatePacketClass)packet2;
			WS_Creatures.CreatureObject obj = (WS_Creatures.CreatureObject)Target;
			packet2 = packet;
			obj.SendToNearPlayers(ref packet2, 0uL);
			packet = (Packets.UpdatePacketClass)packet2;
			packet.Dispose();
			UpdateData.Dispose();
		}

		public void SPELL_AURA_MOD_BASE_RESISTANCE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			if (!(Target is WS_PlayerData.CharacterObject))
			{
				return;
			}
			switch (Action)
			{
			case AuraAction.AURA_UPDATE:
				break;
			case AuraAction.AURA_ADD:
			{
				DamageTypes i = DamageTypes.DMG_PHYSICAL;
				do
				{
					if (WorldServiceLocator._Functions.HaveFlag(checked((uint)EffectInfo.MiscValue), (byte)i))
					{
						ref int base3 = ref ((WS_PlayerData.CharacterObject)Target).Resistances[(uint)i].Base;
						checked
						{
							base3 = (int)Math.Round(base3 / ((WS_PlayerData.CharacterObject)Target).Resistances[unchecked((uint)i)].Modifier);
							((WS_PlayerData.CharacterObject)Target).Resistances[unchecked((uint)i)].Base += EffectInfo.GetValue(unchecked(Target.Level), 0);
							ref int base4 = ref ((WS_PlayerData.CharacterObject)Target).Resistances[unchecked((uint)i)].Base;
							base4 = (int)Math.Round(base4 * ((WS_PlayerData.CharacterObject)Target).Resistances[unchecked((uint)i)].Modifier);
							((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(155 + unchecked((int)i), ((WS_PlayerData.CharacterObject)Target).Resistances[unchecked((uint)i)].Base);
						}
					}
					i++;
				}
				while ((int)i <= 6);
				break;
			}
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
			{
				DamageTypes j = DamageTypes.DMG_PHYSICAL;
				do
				{
					if (WorldServiceLocator._Functions.HaveFlag(checked((uint)EffectInfo.MiscValue), (byte)j))
					{
						ref int @base = ref ((WS_PlayerData.CharacterObject)Target).Resistances[(uint)j].Base;
						checked
						{
							@base = (int)Math.Round(@base / ((WS_PlayerData.CharacterObject)Target).Resistances[unchecked((uint)j)].Modifier);
							((WS_PlayerData.CharacterObject)Target).Resistances[unchecked((uint)j)].Base -= EffectInfo.GetValue(unchecked(Target.Level), 0);
							ref int base2 = ref ((WS_PlayerData.CharacterObject)Target).Resistances[unchecked((uint)j)].Base;
							base2 = (int)Math.Round(base2 * ((WS_PlayerData.CharacterObject)Target).Resistances[unchecked((uint)j)].Modifier);
							((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(155 + unchecked((int)j), ((WS_PlayerData.CharacterObject)Target).Resistances[unchecked((uint)j)].Base);
						}
					}
					j++;
				}
				while ((int)j <= 6);
				break;
			}
			}
		}

		public void SPELL_AURA_MOD_BASE_RESISTANCE_PCT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			if (!(Target is WS_PlayerData.CharacterObject))
			{
				return;
			}
			switch (Action)
			{
			case AuraAction.AURA_UPDATE:
				break;
			case AuraAction.AURA_ADD:
			{
				byte i = 0;
				do
				{
					checked
					{
						if (WorldServiceLocator._Functions.HaveFlag((uint)EffectInfo.MiscValue, i))
						{
							ref int base3 = ref ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base;
							base3 = (int)Math.Round(base3 / ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
							ref float modifier2 = ref ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier;
							modifier2 = (float)(modifier2 + EffectInfo.GetValue(unchecked(Target.Level), 0) / 100.0);
							ref int base4 = ref ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base;
							base4 = (int)Math.Round(base4 * ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
							((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(155 + unchecked(i), ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base);
						}
						i = (byte)unchecked((uint)(i + 1));
					}
				}
				while (i <= 6u);
				break;
			}
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
			{
				byte j = 0;
				do
				{
					checked
					{
						if (WorldServiceLocator._Functions.HaveFlag((uint)EffectInfo.MiscValue, j))
						{
							ref int @base = ref ((WS_PlayerData.CharacterObject)Target).Resistances[j].Base;
							@base = (int)Math.Round(@base / ((WS_PlayerData.CharacterObject)Target).Resistances[j].Modifier);
							ref float modifier = ref ((WS_PlayerData.CharacterObject)Target).Resistances[j].Modifier;
							modifier = (float)(modifier - EffectInfo.GetValue(unchecked(Target.Level), 0) / 100.0);
							ref int base2 = ref ((WS_PlayerData.CharacterObject)Target).Resistances[j].Base;
							base2 = (int)Math.Round(base2 * ((WS_PlayerData.CharacterObject)Target).Resistances[j].Modifier);
							((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(155 + unchecked(j), ((WS_PlayerData.CharacterObject)Target).Resistances[j].Base);
						}
						j = (byte)unchecked((uint)(j + 1));
					}
				}
				while (j <= 6u);
				break;
			}
			}
		}

		public void SPELL_AURA_MOD_RESISTANCE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			if (!(Target is WS_PlayerData.CharacterObject))
			{
				return;
			}
			switch (Action)
			{
			case AuraAction.AURA_UPDATE:
				return;
			case AuraAction.AURA_ADD:
			{
				byte i = 0;
				do
				{
					checked
					{
						if (WorldServiceLocator._Functions.HaveFlag((uint)EffectInfo.MiscValue, i))
						{
							if (EffectInfo.GetValue(unchecked(Target.Level), 0) > 0)
							{
								ref int base5 = ref Target.Resistances[i].Base;
								base5 = (int)Math.Round(base5 / ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
								Target.Resistances[i].Base += EffectInfo.GetValue(unchecked(Target.Level), 0);
								ref int base6 = ref Target.Resistances[i].Base;
								base6 = (int)Math.Round(base6 * ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
								ref short positiveBonus2 = ref Target.Resistances[i].PositiveBonus;
								positiveBonus2 = (short)(positiveBonus2 + EffectInfo.GetValue(unchecked(Target.Level), 0));
							}
							else
							{
								ref int base7 = ref Target.Resistances[i].Base;
								base7 = (int)Math.Round(base7 / ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
								Target.Resistances[i].Base += EffectInfo.GetValue(unchecked(Target.Level), 0);
								ref int base8 = ref Target.Resistances[i].Base;
								base8 = (int)Math.Round(base8 * ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
								ref short negativeBonus2 = ref Target.Resistances[i].NegativeBonus;
								negativeBonus2 = (short)(negativeBonus2 - EffectInfo.GetValue(unchecked(Target.Level), 0));
							}
							((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(155 + unchecked(i), ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base);
						}
						i = (byte)unchecked((uint)(i + 1));
					}
				}
				while (i <= 6u);
				break;
			}
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
			{
				byte j = 0;
				do
				{
					checked
					{
						if (WorldServiceLocator._Functions.HaveFlag((uint)EffectInfo.MiscValue, j))
						{
							if (EffectInfo.GetValue(unchecked(Target.Level), 0) > 0)
							{
								ref int @base = ref Target.Resistances[j].Base;
								@base = (int)Math.Round(@base / ((WS_PlayerData.CharacterObject)Target).Resistances[j].Modifier);
								Target.Resistances[j].Base -= EffectInfo.GetValue(unchecked(Target.Level), 0);
								ref int base2 = ref Target.Resistances[j].Base;
								base2 = (int)Math.Round(base2 * ((WS_PlayerData.CharacterObject)Target).Resistances[j].Modifier);
								ref short positiveBonus = ref Target.Resistances[j].PositiveBonus;
								positiveBonus = (short)(positiveBonus - EffectInfo.GetValue(unchecked(Target.Level), 0));
							}
							else
							{
								ref int base3 = ref Target.Resistances[j].Base;
								base3 = (int)Math.Round(base3 / ((WS_PlayerData.CharacterObject)Target).Resistances[j].Modifier);
								Target.Resistances[j].Base -= EffectInfo.GetValue(unchecked(Target.Level), 0);
								ref int base4 = ref Target.Resistances[j].Base;
								base4 = (int)Math.Round(base4 * ((WS_PlayerData.CharacterObject)Target).Resistances[j].Modifier);
								ref short negativeBonus = ref Target.Resistances[j].NegativeBonus;
								negativeBonus = (short)(negativeBonus + EffectInfo.GetValue(unchecked(Target.Level), 0));
							}
							((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(155 + unchecked(j), ((WS_PlayerData.CharacterObject)Target).Resistances[j].Base);
						}
						j = (byte)unchecked((uint)(j + 1));
					}
				}
				while (j <= 6u);
				break;
			}
			}
			((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate(toNear: false);
		}

		public void SPELL_AURA_MOD_RESISTANCE_PCT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			if (!(Target is WS_PlayerData.CharacterObject))
			{
				return;
			}
			switch (Action)
			{
			case AuraAction.AURA_UPDATE:
				return;
			case AuraAction.AURA_ADD:
			{
				byte i = 0;
				do
				{
					checked
					{
						if (WorldServiceLocator._Functions.HaveFlag((uint)EffectInfo.MiscValue, i))
						{
							short OldBase = (short)((WS_PlayerData.CharacterObject)Target).Resistances[i].Base;
							if (EffectInfo.GetValue(unchecked(Target.Level), 0) > 0)
							{
								ref int base5 = ref ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base;
								base5 = (int)Math.Round(base5 / ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
								((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier += EffectInfo.GetValue(unchecked(Target.Level), 0);
								ref int base6 = ref ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base;
								base6 = (int)Math.Round(base6 * ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
								ref short positiveBonus2 = ref ((WS_PlayerData.CharacterObject)Target).Resistances[i].PositiveBonus;
								positiveBonus2 = (short)(positiveBonus2 + (((WS_PlayerData.CharacterObject)Target).Resistances[i].Base - OldBase));
							}
							else
							{
								ref int base7 = ref ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base;
								base7 = (int)Math.Round(base7 / ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
								((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier -= EffectInfo.GetValue(unchecked(Target.Level), 0);
								ref int base8 = ref ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base;
								base8 = (int)Math.Round(base8 * ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
								ref short positiveBonus3 = ref ((WS_PlayerData.CharacterObject)Target).Resistances[i].PositiveBonus;
								positiveBonus3 = (short)(positiveBonus3 + (((WS_PlayerData.CharacterObject)Target).Resistances[i].Base - OldBase));
							}
							((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(155 + unchecked(i), ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base);
						}
						i = (byte)unchecked((uint)(i + 1));
					}
				}
				while (i <= 6u);
				break;
			}
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
			{
				byte j = 0;
				do
				{
					checked
					{
						if (WorldServiceLocator._Functions.HaveFlag((uint)EffectInfo.MiscValue, j))
						{
							short OldBase2 = (short)((WS_PlayerData.CharacterObject)Target).Resistances[j].Base;
							if (EffectInfo.GetValue(unchecked(Target.Level), 0) > 0)
							{
								ref int @base = ref ((WS_PlayerData.CharacterObject)Target).Resistances[j].Base;
								@base = (int)Math.Round(@base / ((WS_PlayerData.CharacterObject)Target).Resistances[j].Modifier);
								((WS_PlayerData.CharacterObject)Target).Resistances[j].Modifier -= EffectInfo.GetValue(unchecked(Target.Level), 0);
								ref int base2 = ref ((WS_PlayerData.CharacterObject)Target).Resistances[j].Base;
								base2 = (int)Math.Round(base2 * ((WS_PlayerData.CharacterObject)Target).Resistances[j].Modifier);
								ref short positiveBonus = ref ((WS_PlayerData.CharacterObject)Target).Resistances[j].PositiveBonus;
								positiveBonus = (short)(positiveBonus - (((WS_PlayerData.CharacterObject)Target).Resistances[j].Base - OldBase2));
							}
							else
							{
								ref int base3 = ref ((WS_PlayerData.CharacterObject)Target).Resistances[j].Base;
								base3 = (int)Math.Round(base3 / ((WS_PlayerData.CharacterObject)Target).Resistances[j].Modifier);
								((WS_PlayerData.CharacterObject)Target).Resistances[j].Modifier += EffectInfo.GetValue(unchecked(Target.Level), 0);
								ref int base4 = ref ((WS_PlayerData.CharacterObject)Target).Resistances[j].Base;
								base4 = (int)Math.Round(base4 * ((WS_PlayerData.CharacterObject)Target).Resistances[j].Modifier);
								ref short negativeBonus = ref ((WS_PlayerData.CharacterObject)Target).Resistances[j].NegativeBonus;
								negativeBonus = (short)(negativeBonus - (((WS_PlayerData.CharacterObject)Target).Resistances[j].Base - OldBase2));
							}
							((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(155 + unchecked(j), ((WS_PlayerData.CharacterObject)Target).Resistances[j].Base);
						}
						j = (byte)unchecked((uint)(j + 1));
					}
				}
				while (j <= 6u);
				break;
			}
			}
			((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate(toNear: false);
		}

		public void SPELL_AURA_MOD_RESISTANCE_EXCLUSIVE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			if (!(Target is WS_PlayerData.CharacterObject))
			{
				return;
			}
			switch (Action)
			{
			case AuraAction.AURA_UPDATE:
				return;
			case AuraAction.AURA_ADD:
			{
				byte i = 0;
				do
				{
					checked
					{
						if (WorldServiceLocator._Functions.HaveFlag((uint)EffectInfo.MiscValue, i))
						{
							if (EffectInfo.GetValue(unchecked(Target.Level), 0) > 0)
							{
								ref int base5 = ref ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base;
								base5 = (int)Math.Round(base5 / ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
								((WS_PlayerData.CharacterObject)Target).Resistances[i].Base += EffectInfo.GetValue(unchecked(Target.Level), 0);
								ref int base6 = ref ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base;
								base6 = (int)Math.Round(base6 * ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
								ref short positiveBonus3 = ref ((WS_PlayerData.CharacterObject)Target).Resistances[i].PositiveBonus;
								positiveBonus3 = (short)(positiveBonus3 + EffectInfo.GetValue(unchecked(Target.Level), 0));
								((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(155 + unchecked(i), ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base);
							}
							else
							{
								ref int base7 = ref ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base;
								base7 = (int)Math.Round(base7 / ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
								((WS_PlayerData.CharacterObject)Target).Resistances[i].Base -= EffectInfo.GetValue(unchecked(Target.Level), 0);
								ref int base8 = ref ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base;
								base8 = (int)Math.Round(base8 * ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
								ref short negativeBonus = ref ((WS_PlayerData.CharacterObject)Target).Resistances[i].NegativeBonus;
								negativeBonus = (short)(negativeBonus - EffectInfo.GetValue(unchecked(Target.Level), 0));
								((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(155 + unchecked(i), ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base);
							}
						}
						i = (byte)unchecked((uint)(i + 1));
					}
				}
				while (i <= 6u);
				break;
			}
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
			{
				byte j = 0;
				do
				{
					checked
					{
						if (WorldServiceLocator._Functions.HaveFlag((uint)EffectInfo.MiscValue, j))
						{
							if (EffectInfo.GetValue(unchecked(Target.Level), 0) > 0)
							{
								ref int @base = ref ((WS_PlayerData.CharacterObject)Target).Resistances[j].Base;
								@base = (int)Math.Round(@base / ((WS_PlayerData.CharacterObject)Target).Resistances[j].Modifier);
								((WS_PlayerData.CharacterObject)Target).Resistances[j].Base -= EffectInfo.GetValue(unchecked(Target.Level), 0);
								ref int base2 = ref ((WS_PlayerData.CharacterObject)Target).Resistances[j].Base;
								base2 = (int)Math.Round(base2 * ((WS_PlayerData.CharacterObject)Target).Resistances[j].Modifier);
								ref short positiveBonus = ref ((WS_PlayerData.CharacterObject)Target).Resistances[j].PositiveBonus;
								positiveBonus = (short)(positiveBonus - EffectInfo.GetValue(unchecked(Target.Level), 0));
								((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(155 + unchecked(j), ((WS_PlayerData.CharacterObject)Target).Resistances[j].Base);
							}
							else
							{
								ref int base3 = ref ((WS_PlayerData.CharacterObject)Target).Resistances[j].Base;
								base3 = (int)Math.Round(base3 / ((WS_PlayerData.CharacterObject)Target).Resistances[j].Modifier);
								((WS_PlayerData.CharacterObject)Target).Resistances[j].Base += EffectInfo.GetValue(unchecked(Target.Level), 0);
								ref int base4 = ref ((WS_PlayerData.CharacterObject)Target).Resistances[j].Base;
								base4 = (int)Math.Round(base4 * ((WS_PlayerData.CharacterObject)Target).Resistances[j].Modifier);
								ref short positiveBonus2 = ref ((WS_PlayerData.CharacterObject)Target).Resistances[j].PositiveBonus;
								positiveBonus2 = (short)(positiveBonus2 + EffectInfo.GetValue(unchecked(Target.Level), 0));
								((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(155 + unchecked(j), ((WS_PlayerData.CharacterObject)Target).Resistances[j].Base);
							}
						}
						j = (byte)unchecked((uint)(j + 1));
					}
				}
				while (j <= 6u);
				break;
			}
			}
			((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate(toNear: false);
		}

		public void SPELL_AURA_MOD_ATTACK_POWER(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			checked
			{
				switch (Action)
				{
				case AuraAction.AURA_UPDATE:
					return;
				case AuraAction.AURA_ADD:
					Target.AttackPowerMods += EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0) * StackCount;
					break;
				case AuraAction.AURA_REMOVE:
				case AuraAction.AURA_REMOVEBYDURATION:
					Target.AttackPowerMods -= EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0) * StackCount;
					break;
				}
				if (Target is WS_PlayerData.CharacterObject @object)
				{
					@object.SetUpdateFlag(165, @object.AttackPower);
					@object.SetUpdateFlag(166, @object.AttackPowerMods);
					WS_Combat wS_Combat = WorldServiceLocator._WS_Combat;
					WS_PlayerData.CharacterObject objCharacter = @object;
					wS_Combat.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.BASE_ATTACK);
					WS_Combat wS_Combat2 = WorldServiceLocator._WS_Combat;
					objCharacter = @object;
					wS_Combat2.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.OFF_ATTACK);
					@object.SendCharacterUpdate(toNear: false);
				}
			}
		}

		public void SPELL_AURA_MOD_RANGED_ATTACK_POWER(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			checked
			{
				switch (Action)
				{
				case AuraAction.AURA_UPDATE:
					return;
				case AuraAction.AURA_ADD:
					Target.AttackPowerModsRanged += EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0) * StackCount;
					break;
				case AuraAction.AURA_REMOVE:
				case AuraAction.AURA_REMOVEBYDURATION:
					Target.AttackPowerModsRanged -= EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0) * StackCount;
					break;
				}
				if (Target is WS_PlayerData.CharacterObject @object)
				{
					@object.SetUpdateFlag(168, @object.AttackPowerRanged);
					@object.SetUpdateFlag(169, @object.AttackPowerModsRanged);
					WS_Combat wS_Combat = WorldServiceLocator._WS_Combat;
					WS_PlayerData.CharacterObject objCharacter = @object;
					wS_Combat.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.RANGED_ATTACK);
					@object.SendCharacterUpdate(toNear: false);
				}
			}
		}

		public void SPELL_AURA_MOD_HEALING_DONE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			if (Target is WS_PlayerData.CharacterObject @object)
			{
				int Value = EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
				checked
				{
					switch (Action)
					{
					case AuraAction.AURA_UPDATE:
						break;
					case AuraAction.AURA_ADD:
						@object.healing.PositiveBonus += Value;
						break;
					case AuraAction.AURA_REMOVE:
					case AuraAction.AURA_REMOVEBYDURATION:
						@object.healing.PositiveBonus -= Value;
						break;
					}
				}
			}
		}

		public void SPELL_AURA_MOD_HEALING_DONE_PCT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			if (Target is WS_PlayerData.CharacterObject @object)
			{
				float Value = (float)(EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100.0);
				switch (Action)
				{
				case AuraAction.AURA_UPDATE:
					break;
				case AuraAction.AURA_ADD:
					@object.healing.Modifier += Value;
					break;
				case AuraAction.AURA_REMOVE:
				case AuraAction.AURA_REMOVEBYDURATION:
					@object.healing.Modifier -= Value;
					break;
				}
			}
		}

		public void SPELL_AURA_MOD_DAMAGE_DONE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			if (!(Target is WS_PlayerData.CharacterObject))
			{
				return;
			}
			switch (Action)
			{
			case AuraAction.AURA_UPDATE:
				return;
			case AuraAction.AURA_ADD:
			{
				DamageTypes i = DamageTypes.DMG_PHYSICAL;
				do
				{
					checked
					{
						if (WorldServiceLocator._Functions.HaveFlag((uint)EffectInfo.MiscValue, unchecked((byte)i)))
						{
							if (EffectInfo.GetValue(unchecked(Target.Level), 0) > 0)
							{
								((WS_PlayerData.CharacterObject)Target).spellDamage[unchecked((uint)i)].PositiveBonus += EffectInfo.GetValue(unchecked(Target.Level), 0);
								((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(1201 + unchecked((int)i), ((WS_PlayerData.CharacterObject)Target).spellDamage[unchecked((uint)i)].PositiveBonus);
							}
							else
							{
								((WS_PlayerData.CharacterObject)Target).spellDamage[unchecked((uint)i)].NegativeBonus -= EffectInfo.GetValue(unchecked(Target.Level), 0);
								((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(1208 + unchecked((int)i), ((WS_PlayerData.CharacterObject)Target).spellDamage[unchecked((uint)i)].NegativeBonus);
							}
						}
					}
					i++;
				}
				while ((int)i <= 6);
				break;
			}
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
			{
				DamageTypes j = DamageTypes.DMG_PHYSICAL;
				do
				{
					checked
					{
						if (WorldServiceLocator._Functions.HaveFlag((uint)EffectInfo.MiscValue, unchecked((byte)j)))
						{
							if (EffectInfo.GetValue(unchecked(Target.Level), 0) > 0)
							{
								((WS_PlayerData.CharacterObject)Target).spellDamage[unchecked((uint)j)].PositiveBonus -= EffectInfo.GetValue(unchecked(Target.Level), 0);
								((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(1201 + unchecked((int)j), ((WS_PlayerData.CharacterObject)Target).spellDamage[unchecked((uint)j)].PositiveBonus);
							}
							else
							{
								((WS_PlayerData.CharacterObject)Target).spellDamage[unchecked((uint)j)].NegativeBonus += EffectInfo.GetValue(unchecked(Target.Level), 0);
								((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(1208 + unchecked((int)j), ((WS_PlayerData.CharacterObject)Target).spellDamage[unchecked((uint)j)].NegativeBonus);
							}
						}
					}
					j++;
				}
				while ((int)j <= 6);
				break;
			}
			}
			((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate(toNear: false);
		}

		public void SPELL_AURA_MOD_DAMAGE_DONE_PCT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			if (!(Target is WS_PlayerData.CharacterObject))
			{
				return;
			}
			switch (Action)
			{
			case AuraAction.AURA_UPDATE:
				return;
			case AuraAction.AURA_ADD:
			{
				DamageTypes i = DamageTypes.DMG_PHYSICAL;
				do
				{
					if (WorldServiceLocator._Functions.HaveFlag(checked((uint)EffectInfo.MiscValue), (byte)i))
					{
						ref float modifier2 = ref ((WS_PlayerData.CharacterObject)Target).spellDamage[(uint)i].Modifier;
						modifier2 = (float)(modifier2 + EffectInfo.GetValue(Target.Level, 0) / 100.0);
						((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(checked(1215 + unchecked((int)i)), ((WS_PlayerData.CharacterObject)Target).spellDamage[(uint)i].Modifier);
					}
					i++;
				}
				while ((int)i <= 6);
				break;
			}
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
			{
				DamageTypes j = DamageTypes.DMG_PHYSICAL;
				do
				{
					if (WorldServiceLocator._Functions.HaveFlag(checked((uint)EffectInfo.MiscValue), (byte)j))
					{
						ref float modifier = ref ((WS_PlayerData.CharacterObject)Target).spellDamage[(uint)j].Modifier;
						modifier = (float)(modifier - EffectInfo.GetValue(Target.Level, 0) / 100.0);
						((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(checked(1215 + unchecked((int)j)), ((WS_PlayerData.CharacterObject)Target).spellDamage[(uint)j].Modifier);
					}
					j++;
				}
				while ((int)j <= 6);
				break;
			}
			}
			((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate(toNear: false);
		}

		public void SPELL_AURA_EMPATHY(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			switch (Action)
			{
			case AuraAction.AURA_UPDATE:
				break;
			case AuraAction.AURA_ADD:
				if (Target is WS_Creatures.CreatureObject @object && @object.CreatureInfo.CreatureType == 1)
				{
					Packets.UpdatePacketClass packet = new Packets.UpdatePacketClass();
					Packets.UpdateClass tmpUpdate = new Packets.UpdateClass(188);
					tmpUpdate.SetUpdateFlag(143, Target.cDynamicFlags | 0x10);
					Packets.PacketClass packet3 = packet;
					WS_Creatures.CreatureObject updateObject = @object;
					tmpUpdate.AddToPacket(ref packet3, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
					WS_Network.ClientClass client2 = ((WS_PlayerData.CharacterObject)Caster).client;
					packet3 = packet;
					client2.Send(ref packet3);
					tmpUpdate.Dispose();
					packet.Dispose();
				}
				break;
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
				if (Target is WS_Creatures.CreatureObject object1 && object1.CreatureInfo.CreatureType == 1)
				{
					Packets.UpdatePacketClass packet2 = new Packets.UpdatePacketClass();
					Packets.UpdateClass tmpUpdate2 = new Packets.UpdateClass(188);
					tmpUpdate2.SetUpdateFlag(143, Target.cDynamicFlags);
					Packets.PacketClass packet3 = packet2;
					WS_Creatures.CreatureObject updateObject = object1;
					tmpUpdate2.AddToPacket(ref packet3, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
					WS_Network.ClientClass client = ((WS_PlayerData.CharacterObject)Caster).client;
					packet3 = packet2;
					client.Send(ref packet3);
					tmpUpdate2.Dispose();
					packet2.Dispose();
				}
				break;
			}
		}

		public void SPELL_AURA_MOD_SILENCE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			switch (Action)
			{
			case AuraAction.AURA_UPDATE:
				break;
			case AuraAction.AURA_ADD:
				Target.Spell_Silenced = true;
				if (Target is WS_Creatures.CreatureObject @object && @object.aiScript != null)
				{
					WS_Creatures_AI.TBaseAI aiScript = @object.aiScript;
					WS_Base.BaseUnit Attacker = (WS_Base.BaseUnit)Caster;
					aiScript.OnGenerateHate(ref Attacker, 1);
				}
				break;
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
				Target.Spell_Silenced = false;
				break;
			}
		}

		public void SPELL_AURA_MOD_PACIFY(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			switch (Action)
			{
			case AuraAction.AURA_UPDATE:
				break;
			case AuraAction.AURA_ADD:
				Target.Spell_Pacifyed = true;
				break;
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
				Target.Spell_Pacifyed = false;
				break;
			}
		}

		public void SPELL_AURA_MOD_LANGUAGE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			if (Target is WS_PlayerData.CharacterObject @object)
			{
				switch (Action)
				{
				case AuraAction.AURA_UPDATE:
					break;
				case AuraAction.AURA_ADD:
					@object.Spell_Language = (LANGUAGES)EffectInfo.MiscValue;
					break;
				case AuraAction.AURA_REMOVE:
				case AuraAction.AURA_REMOVEBYDURATION:
					@object.Spell_Language = (LANGUAGES)(-1);
					break;
				}
			}
		}

		public void SPELL_AURA_MOD_POSSESS(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			if (!(Target is WS_Creatures.CreatureObject) && !(Target is WS_PlayerData.CharacterObject))
			{
				return;
			}
			switch (Action)
			{
			case AuraAction.AURA_UPDATE:
				break;
			case AuraAction.AURA_ADD:
				if (Target.Level <= EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0))
				{
					Packets.UpdatePacketClass packet = new Packets.UpdatePacketClass();
					Packets.UpdateClass tmpUpdate = new Packets.UpdateClass(188);
					tmpUpdate.SetUpdateFlag(10, Caster.GUID);
					tmpUpdate.SetUpdateFlag(35, ((WS_PlayerData.CharacterObject)Caster).Faction);
					Packets.PacketClass packet8;
                        switch (Target)
                        {
                            case WS_PlayerData.CharacterObject _:
                                {
                                    Packets.UpdateClass updateClass5 = tmpUpdate;
                                    packet8 = packet;
                                    WS_PlayerData.CharacterObject updateObject = (WS_PlayerData.CharacterObject)Target;
                                    updateClass5.AddToPacket(ref packet8, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
                                    packet = (Packets.UpdatePacketClass)packet8;
                                    break;
                                }

                            case WS_Creatures.CreatureObject _:
                                {
                                    Packets.UpdateClass updateClass6 = tmpUpdate;
                                    packet8 = packet;
                                    WS_Creatures.CreatureObject updateObject2 = (WS_Creatures.CreatureObject)Target;
                                    updateClass6.AddToPacket(ref packet8, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject2);
                                    packet = (Packets.UpdatePacketClass)packet8;
                                    break;
                                }
                        }
                        WS_Base.BaseUnit obj3 = Target;
					packet8 = packet;
					obj3.SendToNearPlayers(ref packet8, 0uL);
					packet = (Packets.UpdatePacketClass)packet8;
					packet.Dispose();
					tmpUpdate.Dispose();
					packet = new Packets.UpdatePacketClass();
					tmpUpdate = new Packets.UpdateClass(188);
					tmpUpdate.SetUpdateFlag(6, Target.GUID);
                        switch (Caster)
                        {
                            case WS_PlayerData.CharacterObject _:
                                {
                                    Packets.UpdateClass updateClass7 = tmpUpdate;
                                    packet8 = packet;
                                    WS_PlayerData.CharacterObject updateObject = (WS_PlayerData.CharacterObject)Caster;
                                    updateClass7.AddToPacket(ref packet8, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
                                    packet = (Packets.UpdatePacketClass)packet8;
                                    break;
                                }

                            case WS_Creatures.CreatureObject _:
                                {
                                    Packets.UpdateClass updateClass8 = tmpUpdate;
                                    packet8 = packet;
                                    WS_Creatures.CreatureObject updateObject2 = (WS_Creatures.CreatureObject)Caster;
                                    updateClass8.AddToPacket(ref packet8, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject2);
                                    packet = (Packets.UpdatePacketClass)packet8;
                                    break;
                                }
                        }
                        WS_Base.BaseObject obj4 = Caster;
					packet8 = packet;
					obj4.SendToNearPlayers(ref packet8, 0uL);
					packet = (Packets.UpdatePacketClass)packet8;
					packet.Dispose();
					tmpUpdate.Dispose();
					if (Caster is WS_PlayerData.CharacterObject @object)
					{
						WS_Pets wS_Pets = WorldServiceLocator._WS_Pets;
						WS_PlayerData.CharacterObject updateObject = @object;
						wS_Pets.SendPetInitialize(ref updateObject, ref Target);
						Caster = updateObject;
						Packets.PacketClass packet6 = new Packets.PacketClass(Opcodes.SMSG_DEATH_NOTIFY_OBSOLETE);
						packet6.AddPackGUID(Target.GUID);
						packet6.AddInt8(1);
						@object.client.Send(ref packet6);
						packet6.Dispose();
						@object.cUnitFlags |= 0x1000000;
						@object.SetUpdateFlag(712, Target.GUID);
						@object.SetUpdateFlag(46, @object.cUnitFlags);
						@object.SendCharacterUpdate(toNear: false);
						@object.MindControl = Target;
					}
                        switch (Target)
                        {
                            case WS_Creatures.CreatureObject _:
                                ((WS_Creatures.CreatureObject)Target).aiScript.Reset();
                                break;
                            case WS_PlayerData.CharacterObject object1:
                                {
                                    Packets.PacketClass packet3 = new Packets.PacketClass(Opcodes.SMSG_DEATH_NOTIFY_OBSOLETE);
                                    packet3.AddPackGUID(Target.GUID);
                                    packet3.AddInt8(0);
                                    object1.client.Send(ref packet3);
                                    packet3.Dispose();
                                    break;
                                }
                        }
                    }
				break;
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
			{
				Packets.UpdatePacketClass packet2 = new Packets.UpdatePacketClass();
				Packets.UpdateClass tmpUpdate2 = new Packets.UpdateClass(188);
				tmpUpdate2.SetUpdateFlag(10, 0);
				Packets.PacketClass packet8;
                        switch (Target)
                        {
                            case WS_PlayerData.CharacterObject _:
                                {
                                    tmpUpdate2.SetUpdateFlag(35, ((WS_PlayerData.CharacterObject)Target).Faction);
                                    Packets.UpdateClass updateClass = tmpUpdate2;
                                    packet8 = packet2;
                                    WS_PlayerData.CharacterObject updateObject = (WS_PlayerData.CharacterObject)Target;
                                    updateClass.AddToPacket(ref packet8, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
                                    packet2 = (Packets.UpdatePacketClass)packet8;
                                    break;
                                }

                            case WS_Creatures.CreatureObject _:
                                {
                                    tmpUpdate2.SetUpdateFlag(35, ((WS_Creatures.CreatureObject)Target).Faction);
                                    Packets.UpdateClass updateClass2 = tmpUpdate2;
                                    packet8 = packet2;
                                    WS_Creatures.CreatureObject updateObject2 = (WS_Creatures.CreatureObject)Target;
                                    updateClass2.AddToPacket(ref packet8, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject2);
                                    packet2 = (Packets.UpdatePacketClass)packet8;
                                    break;
                                }
                        }
                        WS_Base.BaseUnit obj = Target;
				packet8 = packet2;
				obj.SendToNearPlayers(ref packet8, 0uL);
				packet2 = (Packets.UpdatePacketClass)packet8;
				packet2.Dispose();
				tmpUpdate2.Dispose();
				packet2 = new Packets.UpdatePacketClass();
				tmpUpdate2 = new Packets.UpdateClass(188);
				tmpUpdate2.SetUpdateFlag(6, 0);
                        switch (Caster)
                        {
                            case WS_PlayerData.CharacterObject _:
                                {
                                    Packets.UpdateClass updateClass3 = tmpUpdate2;
                                    packet8 = packet2;
                                    WS_PlayerData.CharacterObject updateObject = (WS_PlayerData.CharacterObject)Caster;
                                    updateClass3.AddToPacket(ref packet8, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
                                    break;
                                }

                            case WS_Creatures.CreatureObject _:
                                {
                                    Packets.UpdateClass updateClass4 = tmpUpdate2;
                                    packet8 = packet2;
                                    WS_Creatures.CreatureObject updateObject2 = (WS_Creatures.CreatureObject)Caster;
                                    updateClass4.AddToPacket(ref packet8, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject2);
                                    break;
                                }
                        }
                        WS_Base.BaseObject obj2 = Caster;
				packet8 = packet2;
				obj2.SendToNearPlayers(ref packet8, 0uL);
				packet2 = (Packets.UpdatePacketClass)packet8;
				packet2.Dispose();
				tmpUpdate2.Dispose();
				if (Caster is WS_PlayerData.CharacterObject @object)
				{
					Packets.PacketClass packet5 = new Packets.PacketClass(Opcodes.SMSG_PET_SPELLS);
					packet5.AddUInt64(0uL);
					@object.client.Send(ref packet5);
					packet5.Dispose();
					Packets.PacketClass packet7 = new Packets.PacketClass(Opcodes.SMSG_DEATH_NOTIFY_OBSOLETE);
					packet7.AddPackGUID(Target.GUID);
					packet7.AddInt8(0);
					@object.client.Send(ref packet7);
					packet7.Dispose();
					@object.cUnitFlags &= -16777217;
					@object.SetUpdateFlag(712, 0);
					@object.SetUpdateFlag(46, @object.cUnitFlags);
					@object.SendCharacterUpdate(toNear: false);
					@object.MindControl = null;
				}
                        switch (Target)
                        {
                            case WS_Creatures.CreatureObject _:
                                ((WS_Creatures.CreatureObject)Target).aiScript.State = AIState.AI_ATTACKING;
                                break;
                            case WS_PlayerData.CharacterObject object1:
                                {
                                    Packets.PacketClass packet4 = new Packets.PacketClass(Opcodes.SMSG_DEATH_NOTIFY_OBSOLETE);
                                    packet4.AddPackGUID(Target.GUID);
                                    packet4.AddInt8(1);
                                    object1.client.Send(ref packet4);
                                    packet4.Dispose();
                                    break;
                                }
                        }
                        break;
			}
			}
		}

		public void SPELL_AURA_MOD_THREAT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			switch (Action)
			{
			case AuraAction.AURA_UPDATE:
				break;
			case AuraAction.AURA_ADD:
				break;
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
				break;
			}
		}

		public void SPELL_AURA_MOD_TOTAL_THREAT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			int Value = default;
			switch (Action)
			{
			case AuraAction.AURA_UPDATE:
				return;
			case AuraAction.AURA_ADD:
				Value = ((!(Target is WS_PlayerData.CharacterObject)) ? EffectInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0) : EffectInfo.GetValue(Target.Level, 0));
				break;
			case AuraAction.AURA_REMOVE:
			case AuraAction.AURA_REMOVEBYDURATION:
				checked
				{
					Value = ((!(Target is WS_PlayerData.CharacterObject)) ? (-EffectInfo.GetValue(unchecked(((WS_Base.BaseUnit)Caster).Level), 0)) : (-EffectInfo.GetValue(unchecked(Target.Level), 0)));
					break;
				}
			}
            switch (Target)
            {
                case WS_PlayerData.CharacterObject _:
                    {
                        foreach (ulong CreatureGUID in ((WS_PlayerData.CharacterObject)Target).creaturesNear)
                        {
                            if (WorldServiceLocator._WorldServer.WORLD_CREATUREs[CreatureGUID].aiScript != null && WorldServiceLocator._WorldServer.WORLD_CREATUREs[CreatureGUID].aiScript.aiHateTable.ContainsKey(Target))
                            {
                                WorldServiceLocator._WorldServer.WORLD_CREATUREs[CreatureGUID].aiScript.OnGenerateHate(ref Target, Value);
                            }
                        }

                        break;
                    }

                default:
                    if (((WS_Creatures.CreatureObject)Target).aiScript != null && ((WS_Creatures.CreatureObject)Target).aiScript.aiHateTable.ContainsKey((WS_Base.BaseUnit)Caster))
                    {
                        WS_Creatures_AI.TBaseAI aiScript = ((WS_Creatures.CreatureObject)Target).aiScript;
                        WS_Base.BaseUnit Attacker = (WS_Base.BaseUnit)Caster;
                        aiScript.OnGenerateHate(ref Attacker, Value);
                        Caster = Attacker;
                    }

                    break;
            }
        }

		public void SPELL_AURA_MOD_TAUNT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
		{
			if (Target is WS_Creatures.CreatureObject @object)
			{
				switch (Action)
				{
				case AuraAction.AURA_UPDATE:
					break;
				case AuraAction.AURA_ADD:
				{
					WS_Creatures_AI.TBaseAI aiScript2 = @object.aiScript;
					WS_Base.BaseUnit Attacker = (WS_Base.BaseUnit)Caster;
					aiScript2.OnGenerateHate(ref Attacker, 9999999);
					Caster = Attacker;
					break;
				}
				case AuraAction.AURA_REMOVE:
				case AuraAction.AURA_REMOVEBYDURATION:
				{
					WS_Creatures_AI.TBaseAI aiScript = @object.aiScript;
					WS_Base.BaseUnit Attacker = (WS_Base.BaseUnit)Caster;
					aiScript.OnGenerateHate(ref Attacker, -9999999);
					Caster = Attacker;
					break;
				}
				}
			}
		}

		public void CheckDuelDistance(ref WS_PlayerData.CharacterObject objCharacter)
		{
			if (!WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(objCharacter.DuelArbiter))
			{
				return;
			}
			if (WorldServiceLocator._WS_Combat.GetDistance(objCharacter, WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[objCharacter.DuelArbiter]) > 40f)
			{
				if (objCharacter.DuelOutOfBounds == 11)
				{
					Packets.PacketClass packet2 = new Packets.PacketClass(Opcodes.SMSG_DUEL_OUTOFBOUNDS);
					objCharacter.client.Send(ref packet2);
					packet2.Dispose();
					objCharacter.DuelOutOfBounds = 10;
				}
			}
			else if (objCharacter.DuelOutOfBounds != 11)
			{
				objCharacter.DuelOutOfBounds = 11;
				Packets.PacketClass packet = new Packets.PacketClass(Opcodes.SMSG_DUEL_INBOUNDS);
				objCharacter.client.Send(ref packet);
				packet.Dispose();
			}
		}

		public void DuelComplete(ref WS_PlayerData.CharacterObject Winner, ref WS_PlayerData.CharacterObject Loser)
		{
			if (Winner == null || Loser == null)
			{
				return;
			}
			Packets.PacketClass response = new Packets.PacketClass(Opcodes.SMSG_DUEL_COMPLETE);
			response.AddInt8(1);
			Winner.client.SendMultiplyPackets(ref response);
			Loser.client.SendMultiplyPackets(ref response);
			response.Dispose();
			Winner.FinishSpell(CurrentSpellTypes.CURRENT_AUTOREPEAT_SPELL);
			Winner.FinishSpell(CurrentSpellTypes.CURRENT_CHANNELED_SPELL);
			Winner.AutoShotSpell = 0;
			Winner.attackState.AttackStop();
			Loser.FinishSpell(CurrentSpellTypes.CURRENT_AUTOREPEAT_SPELL);
			Loser.FinishSpell(CurrentSpellTypes.CURRENT_CHANNELED_SPELL);
			Loser.AutoShotSpell = 0;
			Loser.attackState.AttackStop();
			if (WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(Winner.DuelArbiter))
			{
				WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[Winner.DuelArbiter].Destroy(WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[Winner.DuelArbiter]);
			}
			Winner.DuelOutOfBounds = 11;
			Winner.DuelArbiter = 0uL;
			Winner.SetUpdateFlag(188, 0);
			Winner.SetUpdateFlag(196, 0);
			Winner.RemoveFromCombat(Loser);
			Loser.DuelOutOfBounds = 11;
			Loser.DuelArbiter = 0uL;
			Loser.SetUpdateFlag(188, 0);
			Loser.SetUpdateFlag(196, 0);
			Loser.RemoveFromCombat(Winner);
			checked
			{
				int num = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1;
				for (int i = 0; i <= num; i++)
				{
					if (Winner.ActiveSpells[i] != null)
					{
						Winner.RemoveAura(i, ref Winner.ActiveSpells[i].SpellCaster);
					}
					if (Loser.ActiveSpells[i] != null)
					{
						Loser.RemoveAura(i, ref Loser.ActiveSpells[i].SpellCaster);
					}
				}
				if (Loser.Life.Current == 0)
				{
					Loser.Life.Current = 1;
					Loser.SetUpdateFlag(22, 1);
					Loser.SetUpdateFlag(148, 79);
				}
				Loser.SendCharacterUpdate();
				Winner.SendCharacterUpdate();
				Packets.PacketClass packet = new Packets.PacketClass(Opcodes.SMSG_DUEL_WINNER);
				packet.AddInt8(0);
				packet.AddString(Winner.Name);
				packet.AddInt8(1);
				packet.AddString(Loser.Name);
				Winner.SendToNearPlayers(ref packet, 0uL);
				packet.Dispose();
				Packets.PacketClass SMSG_EMOTE = new Packets.PacketClass(Opcodes.SMSG_EMOTE);
				SMSG_EMOTE.AddInt32(20);
				SMSG_EMOTE.AddUInt64(Loser.GUID);
				Loser.SendToNearPlayers(ref SMSG_EMOTE, 0uL);
				SMSG_EMOTE.Dispose();
				WS_PlayerData.CharacterObject tmpCharacter = Winner;
				Loser.DuelPartner = null;
				tmpCharacter.DuelPartner = null;
			}
		}

		public void On_CMSG_DUEL_ACCEPTED(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
		{
			if (checked(packet.Data.Length - 1) >= 13)
			{
				packet.GetInt16();
				ulong GUID = packet.GetUInt64();
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_DUEL_ACCEPTED [{2:X}]", client.IP, client.Port, GUID);
				if (client.Character.DuelArbiter == GUID)
				{
					WS_PlayerData.CharacterObject c1 = client.Character.DuelPartner;
					WS_PlayerData.CharacterObject c2 = client.Character;
					c1.DuelArbiter = GUID;
					c2.DuelArbiter = GUID;
					c1.SetUpdateFlag(188, c1.DuelArbiter);
					c2.SetUpdateFlag(188, c2.DuelArbiter);
					c2.SendCharacterUpdate();
					c1.SendCharacterUpdate();
					Packets.PacketClass response = new Packets.PacketClass(Opcodes.SMSG_DUEL_COUNTDOWN);
					response.AddInt32(3000);
					c1.client.SendMultiplyPackets(ref response);
					c2.client.SendMultiplyPackets(ref response);
					response.Dispose();
					Thread StartDuel = new Thread(new ThreadStart(c2.StartDuel))
					{
						Name = "Duel timer"
					};
					StartDuel.Start();
				}
			}
		}

		public void On_CMSG_DUEL_CANCELLED(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
		{
			if (checked(packet.Data.Length - 1) >= 13)
			{
				packet.GetInt16();
				ulong GUID = packet.GetUInt64();
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_DUEL_CANCELLED [{2:X}]", client.IP, client.Port, GUID);
				WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[client.Character.DuelArbiter].Despawn();
				client.Character.DuelArbiter = 0uL;
				client.Character.DuelPartner.DuelArbiter = 0uL;
				Packets.PacketClass response = new Packets.PacketClass(Opcodes.SMSG_DUEL_COMPLETE);
				response.AddInt8(0);
				client.Character.client.SendMultiplyPackets(ref response);
				client.Character.DuelPartner.client.SendMultiplyPackets(ref response);
				response.Dispose();
				client.Character.DuelPartner.DuelPartner = null;
				client.Character.DuelPartner = null;
			}
		}

		public void On_CMSG_RESURRECT_RESPONSE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
		{
			if (checked(packet.Data.Length - 1) < 14)
			{
				return;
			}
			packet.GetInt16();
			ulong GUID = packet.GetUInt64();
			byte Status = packet.GetInt8();
			WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_RESURRECT_RESPONSE [GUID={2:X} Status={3}]", client.IP, client.Port, GUID, Status);
			if (Status == 0)
			{
				client.Character.resurrectGUID = 0uL;
				client.Character.resurrectMapID = 0;
				client.Character.resurrectPositionX = 0f;
				client.Character.resurrectPositionY = 0f;
				client.Character.resurrectPositionZ = 0f;
				client.Character.resurrectHealth = 0;
				client.Character.resurrectMana = 0;
			}
			else if (GUID == client.Character.resurrectGUID)
			{
				WorldServiceLocator._WS_Handlers_Misc.CharacterResurrect(ref client.Character);
				client.Character.Life.Current = client.Character.resurrectHealth;
				if (client.Character.ManaType == ManaTypes.TYPE_MANA)
				{
					client.Character.Mana.Current = client.Character.resurrectMana;
				}
				client.Character.SetUpdateFlag(22, client.Character.Life.Current);
				if (client.Character.ManaType == ManaTypes.TYPE_MANA)
				{
					client.Character.SetUpdateFlag(23, client.Character.Mana.Current);
				}
				client.Character.SendCharacterUpdate();
				client.Character.Teleport(client.Character.resurrectPositionX, client.Character.resurrectPositionY, client.Character.resurrectPositionZ, client.Character.orientation, client.Character.resurrectMapID);
			}
		}

		public void On_CMSG_CAST_SPELL(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
		{
			packet.GetInt16();
			int spellID = packet.GetInt32();
			WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CSMG_CAST_SPELL [spellID={2}]", client.IP, client.Port, spellID);
			if (!client.Character.HaveSpell(spellID))
			{
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "[{0}:{1}] CHEAT: Character {2} casting unlearned spell {3}!", client.IP, client.Port, client.Character.Name, spellID);
				return;
			}
			if (!SPELLs.ContainsKey(spellID))
			{
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "[{0}:{1}] Character tried to cast a spell that didn't exist: {2}!", client.IP, client.Port, spellID);
				return;
			}
			uint spellCooldown = client.Character.Spells[spellID].Cooldown;
			if (spellCooldown >= 0)
			{
				uint timeNow = WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now);
				if (timeNow < spellCooldown)
				{
					return;
				}
				client.Character.Spells[spellID].Cooldown = 0u;
				client.Character.Spells[spellID].CooldownItem = 0;
			}
			CurrentSpellTypes SpellType = CurrentSpellTypes.CURRENT_GENERIC_SPELL;
			if (SPELLs[spellID].IsAutoRepeat)
			{
				SpellType = CurrentSpellTypes.CURRENT_AUTOREPEAT_SPELL;
				int tmpSpellID = 0;
				if (client.Character.Items.ContainsKey(17) && client.Character.AutoShotSpell == 0)
				{
					try
					{
						client.Character.AutoShotSpell = spellID;
						client.Character.attackState.Ranged = true;
						client.Character.attackState.AttackStart(client.Character.GetTarget);
					}
					catch (Exception ex)
					{
						ProjectData.SetProjectError(ex);
						Exception e2 = ex;
						WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error casting auto-shoot {0}.{1}", spellID, Environment.NewLine + e2.ToString());
						ProjectData.ClearProjectError();
					}
				}
				return;
			}
			if (SPELLs[spellID].channelInterruptFlags != 0)
			{
				SpellType = CurrentSpellTypes.CURRENT_CHANNELED_SPELL;
			}
			else if (SPELLs[spellID].IsMelee)
			{
				SpellType = CurrentSpellTypes.CURRENT_MELEE_SPELL;
			}
			SpellTargets Targets = new SpellTargets();
			SpellFailedReason castResult = SpellFailedReason.SPELL_FAILED_ERROR;
			try
			{
				SpellTargets spellTargets = Targets;
				WS_Base.BaseObject Caster = client.Character;
				spellTargets.ReadTargets(ref packet, ref Caster);
				castResult = SPELLs[spellID].CanCast(ref client.Character, Targets, FirstCheck: true);
				if (client.Character.spellCasted[(int)SpellType] != null && !client.Character.spellCasted[(int)SpellType].Finished)
				{
					castResult = SpellFailedReason.SPELL_FAILED_SPELL_IN_PROGRESS;
				}
				if (castResult == SpellFailedReason.SPELL_NO_ERROR)
				{
					ref WS_PlayerData.CharacterObject character = ref client.Character;
					Caster = character;
					CastSpellParameters castSpellParameters = new CastSpellParameters(ref Targets, ref Caster, spellID);
					character = (WS_PlayerData.CharacterObject)Caster;
					CastSpellParameters tmpSpell = castSpellParameters;
					client.Character.spellCasted[(int)SpellType] = tmpSpell;
					ThreadPool.QueueUserWorkItem(new WaitCallback(tmpSpell.Cast));
				}
				else
				{
					SendCastResult(castResult, ref client, spellID);
				}
			}
			catch (Exception ex2)
			{
				ProjectData.SetProjectError(ex2);
				Exception e = ex2;
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error casting spell {0}.{1}", spellID, Environment.NewLine + e.ToString());
				SendCastResult(castResult, ref client, spellID);
				ProjectData.ClearProjectError();
			}
		}

		public void On_CMSG_CANCEL_CAST(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
		{
			if (checked(packet.Data.Length - 1) >= 9)
			{
				packet.GetInt16();
				int SpellID = packet.GetInt32();
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CANCEL_CAST", client.IP, client.Port);
				if (client.Character.spellCasted[1] != null && client.Character.spellCasted[1].SpellID == SpellID)
				{
					client.Character.FinishSpell(CurrentSpellTypes.CURRENT_GENERIC_SPELL);
				}
				else if (client.Character.spellCasted[2] != null && client.Character.spellCasted[2].SpellID == SpellID)
				{
					client.Character.FinishSpell(CurrentSpellTypes.CURRENT_AUTOREPEAT_SPELL);
				}
				else if (client.Character.spellCasted[3] != null && client.Character.spellCasted[3].SpellID == SpellID)
				{
					client.Character.FinishSpell(CurrentSpellTypes.CURRENT_CHANNELED_SPELL);
				}
				else if (client.Character.spellCasted[0] != null && client.Character.spellCasted[0].SpellID == SpellID)
				{
					client.Character.FinishSpell(CurrentSpellTypes.CURRENT_MELEE_SPELL);
				}
			}
		}

		public void On_CMSG_CANCEL_AUTO_REPEAT_SPELL(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
		{
			WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CANCEL_AUTO_REPEAT_SPELL", client.IP, client.Port);
			client.Character.FinishSpell(CurrentSpellTypes.CURRENT_AUTOREPEAT_SPELL);
		}

		public void On_CMSG_CANCEL_CHANNELLING(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
		{
			WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CANCEL_CHANNELLING", client.IP, client.Port);
			client.Character.FinishSpell(CurrentSpellTypes.CURRENT_CHANNELED_SPELL);
		}

		public void On_CMSG_CANCEL_AURA(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
		{
			if (checked(packet.Data.Length - 1) >= 9)
			{
				packet.GetInt16();
				int spellID = packet.GetInt32();
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CANCEL_AURA [spellID={2}]", client.IP, client.Port, spellID);
				client.Character.RemoveAuraBySpell(spellID);
			}
		}

		public void On_CMSG_LEARN_TALENT(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
		{
			checked
			{
				try
				{
					if (packet.Data.Length - 1 < 13)
					{
						return;
					}
					packet.GetInt16();
					int TalentID = packet.GetInt32();
					int RequestedRank = packet.GetInt32();
					byte CurrentTalentPoints = client.Character.TalentPoints;
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_LEARN_TALENT [{2}:{3}]", client.IP, client.Port, TalentID, RequestedRank);
					if (CurrentTalentPoints == 0 || RequestedRank > 4 || (RequestedRank > 0 && !client.Character.HaveSpell(WorldServiceLocator._WS_DBCDatabase.Talents[TalentID].RankID[RequestedRank - 1])))
					{
						return;
					}
					int k = 0;
					do
					{
						if (WorldServiceLocator._WS_DBCDatabase.Talents[TalentID].RequiredTalent[k] > 0)
						{
							bool HasEnoughRank = false;
							int DependsOn = WorldServiceLocator._WS_DBCDatabase.Talents[TalentID].RequiredTalent[k];
							int num = WorldServiceLocator._WS_DBCDatabase.Talents[TalentID].RequiredPoints[k];
							for (int j = num; j <= 4; j++)
							{
								if (WorldServiceLocator._WS_DBCDatabase.Talents[DependsOn].RankID[j] != 0 && client.Character.HaveSpell(WorldServiceLocator._WS_DBCDatabase.Talents[DependsOn].RankID[j]))
								{
									HasEnoughRank = true;
								}
							}
							if (!HasEnoughRank)
							{
								return;
							}
						}
						k++;
					}
					while (k <= 2);
					int SpentPoints = 0;
					if (WorldServiceLocator._WS_DBCDatabase.Talents[TalentID].Row > 0)
					{
						foreach (KeyValuePair<int, WS_DBCDatabase.TalentInfo> TalentInfo in WorldServiceLocator._WS_DBCDatabase.Talents)
						{
							if (WorldServiceLocator._WS_DBCDatabase.Talents[TalentID].TalentTab != TalentInfo.Value.TalentTab)
							{
								continue;
							}
							int i = 0;
							do
							{
								if (TalentInfo.Value.RankID[i] != 0 && client.Character.HaveSpell(TalentInfo.Value.RankID[i]))
								{
									SpentPoints += i + 1;
								}
								i++;
							}
							while (i <= 4);
						}
					}
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "_WS_DBCDatabase.Talents spent: {0}", SpentPoints);
					if (SpentPoints < WorldServiceLocator._WS_DBCDatabase.Talents[TalentID].Row * 5)
					{
						return;
					}
					int SpellID = WorldServiceLocator._WS_DBCDatabase.Talents[TalentID].RankID[RequestedRank];
					if (SpellID != 0 && !client.Character.HaveSpell(SpellID))
					{
						client.Character.LearnSpell(SpellID);
						if (SPELLs.ContainsKey(SpellID) && SPELLs[SpellID].IsPassive)
						{
							client.Character.ApplySpell(SpellID);
						}
						if (RequestedRank > 0)
						{
							int ReSpellID = WorldServiceLocator._WS_DBCDatabase.Talents[TalentID].RankID[RequestedRank - 1];
							client.Character.UnLearnSpell(ReSpellID);
							client.Character.RemoveAuraBySpell(ReSpellID);
						}
						client.Character.TalentPoints--;
						client.Character.SetUpdateFlag(1102, client.Character.TalentPoints);
						client.Character.SendCharacterUpdate();
						client.Character.SaveCharacter();
					}
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception e = ex;
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error learning talen: {0}{1}", Environment.NewLine, e.ToString());
					ProjectData.ClearProjectError();
				}
			}
		}

		public void SendLoot(WS_PlayerData.CharacterObject Player, ulong GUID, LootType LootingType)
		{
			if (WorldServiceLocator._CommonGlobalFunctions.GuidIsGameObject(GUID))
			{
				switch (WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].ObjectInfo.Type)
				{
				case GameObjectType.GAMEOBJECT_TYPE_DOOR:
				case GameObjectType.GAMEOBJECT_TYPE_BUTTON:
					return;
				case GameObjectType.GAMEOBJECT_TYPE_QUESTGIVER:
					return;
				case GameObjectType.GAMEOBJECT_TYPE_SPELL_FOCUS:
					return;
				case GameObjectType.GAMEOBJECT_TYPE_GOOBER:
					return;
				}
			}
			WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].LootObject(ref Player, LootingType);
		}
	}
}
