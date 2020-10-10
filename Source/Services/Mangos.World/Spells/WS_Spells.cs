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
using System.Runtime.InteropServices;
using System.Threading;
using Mangos.Common.Enums.Faction;
using Mangos.Common.Enums.GameObject;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Item;
using Mangos.Common.Enums.Player;
using Mangos.Common.Enums.Spell;
using Mangos.Common.Enums.Unit;
using Mangos.Common.Globals;
using Mangos.World.DataStores;
using Mangos.World.Globals;
using Mangos.World.Handlers;
using Mangos.World.Loots;
using Mangos.World.Objects;
using Mangos.World.Player;
using Mangos.World.Server;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Spells
{
    public class WS_Spells
    {

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        // WARNING: Use only with SPELLs()
        public class SpellInfo
        {
            public int ID = 0;
            public int School = 0;
            public int Category = 0;
            public int DispellType = 0;
            public int Mechanic = 0;
            public int Attributes = 0;
            public int AttributesEx = 0;
            public int AttributesEx2 = 0;
            public int RequredCasterStance = 0;
            public int ShapeshiftExclude = 0;
            public int Target = 0;
            public int TargetCreatureType = 0;
            public int FocusObjectIndex = 0;
            public int FacingCasterFlags = 0;
            public int CasterAuraState = 0;
            public int TargetAuraState = 0;
            public int ExcludeCasterAuraState = 0;
            public int ExcludeTargetAuraState = 0;
            public int SpellCastTimeIndex = 0;
            public int CategoryCooldown = 0;
            public int SpellCooldown = 0;
            public int interruptFlags = 0;
            public int auraInterruptFlags = 0;
            public int channelInterruptFlags = 0;
            public int procFlags = 0;
            public int procChance = 0;
            public int procCharges = 0;
            public int maxLevel = 0;
            public int baseLevel = 0;
            public int spellLevel = 0;
            public int maxStack = 0;
            public int DurationIndex = 0;
            public int powerType = 0;
            public int manaCost = 0;
            public int manaCostPerlevel = 0;
            public int manaPerSecond = 0;
            public int manaPerSecondPerLevel = 0;
            public int manaCostPercent = 0;
            public int rangeIndex = 0;
            public float Speed = 0f;
            public int modalNextSpell = 0;
            public int[] Totem = new int[] { 0, 0 };
            public int[] TotemCategory = new int[] { 0, 0 };
            public int[] Reagents = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            public int[] ReagentsCount = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            public int EquippedItemClass = 0;
            public int EquippedItemSubClass = 0;
            public int EquippedItemInventoryType = 0;
            public SpellEffect[] SpellEffects = new SpellEffect[] { null, null, null };
            public int MaxTargets = 0;
            public int RequiredAreaID = 0;
            public int SpellVisual = 0;
            public int SpellPriority = 0;
            public int AffectedTargetLevel = 0;
            public int SpellIconID = 0;
            public int ActiveIconID = 0;
            public int SpellNameFlag = 0;
            public string Rank = "";
            public int RankFlags = 0;
            public int StartRecoveryCategory = 0;
            public int StartRecoveryTime = 0;
            public int SpellFamilyName = 0;
            public int SpellFamilyFlags = 0;
            // Public MaxAffectedTargets As Integer = 0
            public int DamageType = 0;
            public string Name = "";
            public uint CustomAttributs = 0U;

            public SpellSchoolMask SchoolMask
            {
                get
                {
                    return (SpellSchoolMask)(1 << School);
                }
            }

            public int GetDuration
            {
                get
                {
                    if (WorldServiceLocator._WS_Spells.SpellDuration.ContainsKey(DurationIndex))
                        return WorldServiceLocator._WS_Spells.SpellDuration[DurationIndex];
                    return 0;
                }
            }

            public int GetRange
            {
                get
                {
                    if (WorldServiceLocator._WS_Spells.SpellRange.ContainsKey(rangeIndex))
                        return (int)WorldServiceLocator._WS_Spells.SpellRange[rangeIndex];
                    return 0;
                }
            }

            public string GetFocusObject
            {
                get
                {
                    if (WorldServiceLocator._WS_Spells.SpellFocusObject.ContainsKey(FocusObjectIndex))
                        return WorldServiceLocator._WS_Spells.SpellFocusObject[FocusObjectIndex];
                    return 0.ToString();
                }
            }

            public int GetCastTime
            {
                get
                {
                    if (WorldServiceLocator._WS_Spells.SpellCastTime.ContainsKey(SpellCastTimeIndex))
                        return WorldServiceLocator._WS_Spells.SpellCastTime[SpellCastTimeIndex];
                    return 0;
                }
            }

            public int get_GetManaCost(int level, int Mana)
            {
                return (int)(manaCost + manaCostPerlevel * level + Mana * (manaCostPercent / 100d));
            }

            public bool IsAura
            {
                get
                {
                    if (SpellEffects[0] is object && SpellEffects[0].ApplyAuraIndex != 0)
                        return true;
                    if (SpellEffects[1] is object && SpellEffects[1].ApplyAuraIndex != 0)
                        return true;
                    if (SpellEffects[2] is object && SpellEffects[2].ApplyAuraIndex != 0)
                        return true;
                    return false;
                }
            }

            public bool IsAOE
            {
                get
                {
                    if (SpellEffects[0] is object && SpellEffects[0].IsAOE)
                        return true;
                    if (SpellEffects[1] is object && SpellEffects[1].IsAOE)
                        return true;
                    if (SpellEffects[2] is object && SpellEffects[2].IsAOE)
                        return true;
                    return false;
                }
            }

            public bool IsDispell
            {
                get
                {
                    if (SpellEffects[0] is object && SpellEffects[0].ID == SpellEffects_Names.SPELL_EFFECT_DISPEL)
                        return true;
                    if (SpellEffects[1] is object && SpellEffects[1].ID == SpellEffects_Names.SPELL_EFFECT_DISPEL)
                        return true;
                    if (SpellEffects[2] is object && SpellEffects[2].ID == SpellEffects_Names.SPELL_EFFECT_DISPEL)
                        return true;
                    return false;
                }
            }

            public bool IsPassive
            {
                get
                {
                    return ((Attributes & (uint)SpellAttributes.SPELL_ATTR_PASSIVE) != 0) && (AttributesEx & (uint)SpellAttributesEx.SPELL_ATTR_EX_NEGATIVE) == 0;
                }
            }

            public bool IsNegative
            {
                get
                {
                    for (byte i = 0; i <= 2; i++)
                    {
                        if (SpellEffects[i] is object && SpellEffects[i].IsNegative)
                            return true;
                    }

                    return (AttributesEx & (uint)SpellAttributesEx.SPELL_ATTR_EX_NEGATIVE) != 0;
                }
            }

            public bool IsAutoRepeat
            {
                get
                {
                    return (AttributesEx2 & (uint)SpellAttributesEx2.SPELL_ATTR_EX2_AUTO_SHOOT) != 0;
                }
            }

            public bool IsRanged
            {
                get
                {
                    return DamageType == (uint)SpellDamageType.SPELL_DMG_TYPE_RANGED;
                }
            }

            public bool IsMelee
            {
                get
                {
                    return DamageType == (uint)SpellDamageType.SPELL_DMG_TYPE_MELEE;
                }
            }

            public bool CanStackSpellRank
            {
                get
                {
                    if (!WorldServiceLocator._WS_Spells.SpellChains.ContainsKey(ID) || WorldServiceLocator._WS_Spells.SpellChains[ID] == 0)
                        return true;
                    if (powerType == (uint)ManaTypes.TYPE_MANA)
                    {
                        if (manaCost > 0)
                            return true;
                        if (manaCostPercent > 0)
                            return true;
                        if (manaCostPerlevel > 0)
                            return true;
                        if (manaPerSecond > 0)
                            return true;
                        if (manaPerSecondPerLevel > 0)
                            return true;
                    }

                    return false;
                }
            }

            public Dictionary<WS_Base.BaseObject, SpellMissInfo> GetTargets(ref WS_Base.BaseObject Caster, SpellTargets Targets, byte Index)
            {
                var TargetsInfected = new List<WS_Base.BaseObject>();
                WS_Base.BaseUnit Ref = null;
                if (Caster is WS_Totems.TotemObject)
                    Ref = ((WS_Totems.TotemObject)Caster).Caster;
                if (SpellEffects[Index] is object)
                {
                    for (byte j = 0; j <= 1; j++)
                    {
                        SpellImplicitTargets ImplicitTarget = (SpellImplicitTargets)SpellEffects[Index].implicitTargetA;
                        if (j == 1)
                            ImplicitTarget = (SpellImplicitTargets)SpellEffects[Index].implicitTargetB;
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "{0}: {1}", Conversions.ToString(Interaction.IIf(j == 1, "ImplicitTargetB", "ImplicitTargetA")), ImplicitTarget);
                        if (ImplicitTarget == SpellImplicitTargets.TARGET_NOTHING)
                            continue;
                        switch (ImplicitTarget)
                        {
                            case var @case when @case == SpellImplicitTargets.TARGET_ALL_ENEMY_IN_AREA:
                            case var case1 when case1 == SpellImplicitTargets.TARGET_ALL_ENEMY_IN_AREA_INSTANT:
                                {
                                    List<WS_Base.BaseUnit> EnemyTargets;
                                    if ((Targets.targetMask & (uint)SpellCastTargetFlags.TARGET_FLAG_DEST_LOCATION) != 0)
                                    {
                                        WS_Base.BaseUnit argobjCharacter = (WS_Base.BaseUnit)Caster;
                                        EnemyTargets = WorldServiceLocator._WS_Spells.GetEnemyAtPoint(ref argobjCharacter, Targets.dstX, Targets.dstY, Targets.dstZ, SpellEffects[Index].GetRadius);
                                    }
                                    else if (Caster is WS_DynamicObjects.DynamicObjectObject)
                                    {
                                        EnemyTargets = WorldServiceLocator._WS_Spells.GetEnemyAtPoint(ref ((WS_DynamicObjects.DynamicObjectObject)Caster).Caster, Caster.positionX, Caster.positionY, Caster.positionZ, SpellEffects[Index].GetRadius);
                                    }
                                    else
                                    {
                                        WS_Base.BaseUnit argobjCharacter = (WS_Base.BaseUnit)Caster;
                                        EnemyTargets = WorldServiceLocator._WS_Spells.GetEnemyAtPoint(ref argobjCharacter, Caster.positionX, Caster.positionY, Caster.positionZ, SpellEffects[Index].GetRadius);
                                    }

                                    foreach (WS_Base.BaseUnit EnemyTarget in EnemyTargets)
                                    {
                                        if (!TargetsInfected.Contains(EnemyTarget))
                                            TargetsInfected.Add(EnemyTarget);
                                    }

                                    break;
                                }

                            case var case2 when case2 == SpellImplicitTargets.TARGET_ALL_FRIENDLY_UNITS_AROUND_CASTER:
                                {
                                    WS_Base.BaseUnit argobjCharacter = (WS_Base.BaseUnit)Caster;
                                    var EnemyTargets = WorldServiceLocator._WS_Spells.GetEnemyAroundMe(ref argobjCharacter, SpellEffects[Index].GetRadius, ref Ref);
                                    foreach (WS_Base.BaseUnit EnemyTarget in EnemyTargets)
                                    {
                                        if (!TargetsInfected.Contains(EnemyTarget))
                                            TargetsInfected.Add(EnemyTarget);
                                    }

                                    break;
                                }

                            case var case3 when case3 == SpellImplicitTargets.TARGET_ALL_PARTY:
                                {
                                    WS_PlayerData.CharacterObject argobjCharacter1 = (WS_PlayerData.CharacterObject)Caster;
                                    var PartyTargets = WorldServiceLocator._WS_Spells.GetPartyMembersAroundMe(ref argobjCharacter1, 9999999f);
                                    foreach (WS_Base.BaseUnit PartyTarget in PartyTargets)
                                    {
                                        if (!TargetsInfected.Contains(PartyTarget))
                                            TargetsInfected.Add(PartyTarget);
                                    }

                                    break;
                                }

                            case var case4 when case4 == SpellImplicitTargets.TARGET_ALL_PARTY_AROUND_CASTER_2:
                            case var case5 when case5 == SpellImplicitTargets.TARGET_AROUND_CASTER_PARTY:
                            case var case6 when case6 == SpellImplicitTargets.TARGET_AREAEFFECT_PARTY:
                                {
                                    List<WS_Base.BaseUnit> PartyTargets;
                                    if (Caster is WS_Totems.TotemObject)
                                    {
                                        WS_PlayerData.CharacterObject argobjCharacter = (WS_PlayerData.CharacterObject)((WS_Totems.TotemObject)Caster).Caster;
                                        PartyTargets = WorldServiceLocator._WS_Spells.GetPartyMembersAtPoint(ref argobjCharacter, SpellEffects[Index].GetRadius, Caster.positionX, Caster.positionY, Caster.positionZ);
                                    }
                                    else
                                    {
                                        WS_PlayerData.CharacterObject argobjCharacter = (WS_PlayerData.CharacterObject)Caster;
                                        PartyTargets = WorldServiceLocator._WS_Spells.GetPartyMembersAroundMe(ref argobjCharacter, SpellEffects[Index].GetRadius);
                                    }

                                    foreach (WS_Base.BaseUnit PartyTarget in PartyTargets)
                                    {
                                        if (!TargetsInfected.Contains(PartyTarget))
                                            TargetsInfected.Add(PartyTarget);
                                    }

                                    break;
                                }

                            case var case7 when case7 == SpellImplicitTargets.TARGET_CHAIN_DAMAGE:
                            case var case8 when case8 == SpellImplicitTargets.TARGET_CHAIN_HEAL:
                                {
                                    var UsedTargets = new List<WS_Base.BaseUnit>();
                                    WS_Base.BaseUnit TargetUnit = null;
                                    if (!TargetsInfected.Contains(Targets.unitTarget))
                                        TargetsInfected.Add(Targets.unitTarget);
                                    UsedTargets.Add(Targets.unitTarget);
                                    TargetUnit = Targets.unitTarget;
                                    if (SpellEffects[Index].ChainTarget > 1)
                                    {
                                        for (byte k = 2, loopTo = (byte)SpellEffects[Index].ChainTarget; k <= loopTo; k++)
                                        {
                                            WS_Base.BaseUnit argr = (WS_Base.BaseUnit)Caster;
                                            var EnemyTargets = WorldServiceLocator._WS_Spells.GetEnemyAroundMe(ref TargetUnit, 10f, ref argr);
                                            TargetUnit = null;
                                            float LowHealth = 1.01f;
                                            foreach (WS_Base.BaseUnit tmpUnit in EnemyTargets)
                                            {
                                                if (UsedTargets.Contains(tmpUnit) == false)
                                                {
                                                    float TmpLife = (float)(tmpUnit.Life.Current / (double)tmpUnit.Life.Maximum);
                                                    if (TmpLife < LowHealth)
                                                    {
                                                        LowHealth = TmpLife;
                                                        TargetUnit = tmpUnit;
                                                    }
                                                }
                                            }

                                            if (TargetUnit is object)
                                            {
                                                if (!TargetsInfected.Contains(TargetUnit))
                                                    TargetsInfected.Add(TargetUnit);
                                                UsedTargets.Add(TargetUnit);
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                    }

                                    break;
                                }

                            case var case9 when case9 == SpellImplicitTargets.TARGET_AROUND_CASTER_ENEMY:
                                {
                                    WS_Base.BaseUnit argobjCharacter2 = (WS_Base.BaseUnit)Caster;
                                    var EnemyTargets = WorldServiceLocator._WS_Spells.GetEnemyAroundMe(ref argobjCharacter2, SpellEffects[Index].GetRadius, ref Ref);
                                    foreach (WS_Base.BaseUnit EnemyTarget in EnemyTargets)
                                    {
                                        if (!TargetsInfected.Contains(EnemyTarget))
                                            TargetsInfected.Add(EnemyTarget);
                                    }

                                    break;
                                }

                            case var case10 when case10 == SpellImplicitTargets.TARGET_DYNAMIC_OBJECT:
                                {
                                    if (Targets.goTarget is object)
                                        TargetsInfected.Add(Targets.goTarget);
                                    break;
                                }

                            case var case11 when case11 == SpellImplicitTargets.TARGET_INFRONT:
                                {
                                    if ((CustomAttributs & (uint)SpellAttributesCustom.SPELL_ATTR_CU_CONE_BACK) != 0)
                                    {
                                        WS_Base.BaseUnit argobjCharacter3 = (WS_Base.BaseUnit)Caster;
                                        var EnemyTargets = WorldServiceLocator._WS_Spells.GetEnemyInBehindMe(ref argobjCharacter3, SpellEffects[Index].GetRadius);
                                        foreach (WS_Base.BaseUnit EnemyTarget in EnemyTargets)
                                        {
                                            if (!TargetsInfected.Contains(EnemyTarget))
                                                TargetsInfected.Add(EnemyTarget);
                                        }
                                    }
                                    else if ((CustomAttributs & (uint)SpellAttributesCustom.SPELL_ATTR_CU_CONE_LINE) != 0)
                                    {
                                    }
                                    // TODO!
                                    else
                                    {
                                        WS_Base.BaseUnit argobjCharacter4 = (WS_Base.BaseUnit)Caster;
                                        var EnemyTargets = WorldServiceLocator._WS_Spells.GetEnemyInFrontOfMe(ref argobjCharacter4, SpellEffects[Index].GetRadius);
                                        foreach (WS_Base.BaseUnit EnemyTarget in EnemyTargets)
                                        {
                                            if (!TargetsInfected.Contains(EnemyTarget))
                                                TargetsInfected.Add(EnemyTarget);
                                        }
                                    }

                                    break;
                                }

                            case var case12 when case12 == SpellImplicitTargets.TARGET_BEHIND_VICTIM:
                                {
                                    break;
                                }
                            // TODO: Behind victim? What spells has this really?
                            case var case13 when case13 == SpellImplicitTargets.TARGET_GAMEOBJECT_AND_ITEM:
                            case var case14 when case14 == SpellImplicitTargets.TARGET_SELECTED_GAMEOBJECT:
                                {
                                    if (Targets.goTarget is object)
                                        TargetsInfected.Add(Targets.goTarget);
                                    break;
                                }

                            case var case15 when case15 == SpellImplicitTargets.TARGET_SELF:
                            case var case16 when case16 == SpellImplicitTargets.TARGET_SELF2:
                            case var case17 when case17 == SpellImplicitTargets.TARGET_SELF_FISHING:
                            case var case18 when case18 == SpellImplicitTargets.TARGET_MASTER:
                            case var case19 when case19 == SpellImplicitTargets.TARGET_DUEL_VS_PLAYER:
                                {
                                    if (!TargetsInfected.Contains(Caster))
                                        TargetsInfected.Add(Caster);
                                    break;
                                }

                            case var case20 when case20 == SpellImplicitTargets.TARGET_RANDOM_RAID_MEMBER:
                                {
                                    break;
                                }
                            // TODO
                            case var case21 when case21 == SpellImplicitTargets.TARGET_PET:
                            case var case22 when case22 == SpellImplicitTargets.TARGET_MINION:
                                {
                                    break;
                                }
                            // TODO
                            case var case23 when case23 == SpellImplicitTargets.TARGET_NONCOMBAT_PET:
                                {
                                    if (Caster is WS_PlayerData.CharacterObject && ((WS_PlayerData.CharacterObject)Caster).NonCombatPet is object)
                                        TargetsInfected.Add(((WS_PlayerData.CharacterObject)Caster).NonCombatPet);
                                    break;
                                }

                            case var case24 when case24 == SpellImplicitTargets.TARGET_SINGLE_ENEMY:
                            case var case25 when case25 == SpellImplicitTargets.TARGET_SINGLE_FRIEND_2:
                            case var case26 when case26 == SpellImplicitTargets.TARGET_SELECTED_FRIEND:
                            case var case27 when case27 == SpellImplicitTargets.TARGET_SINGLE_PARTY:
                                {
                                    if (!TargetsInfected.Contains(Targets.unitTarget))
                                        TargetsInfected.Add(Targets.unitTarget);
                                    break;
                                }
                            // TODO: What is this? Used in warstomp.
                            case var case28 when case28 == SpellImplicitTargets.TARGET_EFFECT_SELECT:
                                {
                                    break;
                                }

                            default:
                                {
                                    if (Targets.unitTarget is object)
                                    {
                                        if (!TargetsInfected.Contains(Targets.unitTarget))
                                            TargetsInfected.Add(Targets.unitTarget);
                                    }
                                    else if (!TargetsInfected.Contains(Caster))
                                        TargetsInfected.Add(Caster);
                                    break;
                                }
                        }
                    }

                    // DONE: If no targets were taken, use our target, or else the caster, but ONLY if spell doesn't have any target specifications
                    if (SpellEffects[Index].implicitTargetA == (int)SpellImplicitTargets.TARGET_NOTHING && SpellEffects[Index].implicitTargetB == (int)SpellImplicitTargets.TARGET_NOTHING)
                    {
                        if (TargetsInfected.Count == 0)
                        {
                            if (Targets.unitTarget is object)
                            {
                                if (!TargetsInfected.Contains(Targets.unitTarget))
                                    TargetsInfected.Add(Targets.unitTarget);
                            }
                            else if (!TargetsInfected.Contains(Caster))
                                TargetsInfected.Add(Caster);
                        }
                    }

                    return CalculateMisses(ref Caster, ref TargetsInfected, ref SpellEffects[Index]);
                }
                else
                {
                    return new Dictionary<WS_Base.BaseObject, SpellMissInfo>();
                }
            }

            public Dictionary<WS_Base.BaseObject, SpellMissInfo> CalculateMisses(ref WS_Base.BaseObject Caster, ref List<WS_Base.BaseObject> Targets, ref SpellEffect SpellEffect)
            {
                var newTargets = new Dictionary<WS_Base.BaseObject, SpellMissInfo>();
                foreach (WS_Base.BaseObject Target in Targets)
                {
                    if (!ReferenceEquals(Target, Caster) && Caster is WS_Base.BaseUnit && Target is WS_Base.BaseUnit)
                    {
                        {
                            var withBlock = (WS_Base.BaseUnit)Target;
                            if (SpellEffect.Mechanic > 0 && Conversions.ToBoolean(withBlock.MechanicImmunity & 1 << SpellEffect.Mechanic - 1)) // Immune to mechanic
                            {
                                newTargets.Add(Target, SpellMissInfo.SPELL_MISS_IMMUNE2);
                            }
                            else if (!IsNegative) // Positive spells can't miss
                            {
                                newTargets.Add(Target, SpellMissInfo.SPELL_MISS_NONE);
                            }
                            else if ((AttributesEx & (uint)SpellAttributesEx.SPELL_ATTR_EX_UNAFFECTED_BY_SCHOOL_IMMUNE) == 0 && (withBlock.SchoolImmunity & 1 << School) != 0) // Immune to school
                            {
                                newTargets.Add(Target, SpellMissInfo.SPELL_MISS_IMMUNE2);
                            }
                            else if (Target is WS_Creatures.CreatureObject && ((WS_Creatures.CreatureObject)Target).Evade) // Creature is evading
                            {
                                newTargets.Add(Target, SpellMissInfo.SPELL_MISS_EVADE);
                            }
                            else if (Caster is WS_PlayerData.CharacterObject && ((WS_PlayerData.CharacterObject)Caster).GM) // Only GM itself can cast on himself when in GM mode
                            {
                            }
                            // Don't even show up in misses
                            else
                            {
                                switch ((SpellDamageType)DamageType)
                                {
                                    case var @case when @case == SpellDamageType.SPELL_DMG_TYPE_NONE:
                                        {
                                            newTargets.Add(Target, SpellMissInfo.SPELL_MISS_NONE);
                                            break;
                                        }

                                    case var case1 when case1 == SpellDamageType.SPELL_DMG_TYPE_MAGIC:
                                        {
                                            WS_Base.BaseUnit argCaster = (WS_Base.BaseUnit)Caster;
                                            newTargets.Add(Target, withBlock.GetMagicSpellHitResult(ref argCaster, this));
                                            break;
                                        }

                                    case var case2 when case2 == SpellDamageType.SPELL_DMG_TYPE_MELEE:
                                    case var case3 when case3 == SpellDamageType.SPELL_DMG_TYPE_RANGED:
                                        {
                                            WS_Base.BaseUnit argCaster = (WS_Base.BaseUnit)Caster;
                                            newTargets.Add(Target, withBlock.GetMeleeSpellHitResult(ref argCaster, this));
                                            break;
                                        }
                                }
                            }
                        }
                    }
                    else
                    {
                        // DONE: Only units can be missed
                        newTargets.Add(Target, SpellMissInfo.SPELL_MISS_NONE);
                    }
                }

                return newTargets;
            }

            public List<WS_Base.BaseObject> GetHits(ref Dictionary<WS_Base.BaseObject, SpellMissInfo> Targets)
            {
                var targetHits = new List<WS_Base.BaseObject>();
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
                // SpellAttributesCustom
                CustomAttributs = 0U;
                bool auraSpell = true;
                for (int i = 0; i <= 2; i++)
                {
                    if (SpellEffects[i] is object && SpellEffects[i].ID != SpellEffects_Names.SPELL_EFFECT_APPLY_AURA)
                    {
                        auraSpell = false;
                        break;
                    }
                }

                if (auraSpell)
                    CustomAttributs = CustomAttributs | (uint)SpellAttributesCustom.SPELL_ATTR_CU_AURA_SPELL;
                if (SpellFamilyName == (int)SpellFamilyNames.SPELLFAMILY_PALADIN && (SpellFamilyFlags & 0xC0000000) != 0)
                {
                    if (SpellEffects[0] is object)
                        SpellEffects[0].ID = SpellEffects_Names.SPELL_EFFECT_HEAL;
                }

                for (int i = 0; i <= 2; i++)
                {
                    if (SpellEffects[i] is object)
                    {
                        switch ((AuraEffects_Names)SpellEffects[i].ApplyAuraIndex)
                        {
                            case var @case when @case == AuraEffects_Names.SPELL_AURA_PERIODIC_DAMAGE:
                            case var case1 when case1 == AuraEffects_Names.SPELL_AURA_PERIODIC_DAMAGE_PERCENT:
                            case var case2 when case2 == AuraEffects_Names.SPELL_AURA_PERIODIC_LEECH:
                                {
                                    CustomAttributs = CustomAttributs | (uint)SpellAttributesCustom.SPELL_ATTR_CU_AURA_DOT;
                                    break;
                                }

                            case var case3 when case3 == AuraEffects_Names.SPELL_AURA_PERIODIC_HEAL:
                            case var case4 when case4 == AuraEffects_Names.SPELL_AURA_OBS_MOD_HEALTH:
                                {
                                    CustomAttributs = CustomAttributs | (uint)SpellAttributesCustom.SPELL_ATTR_CU_AURA_HOT;
                                    break;
                                }

                            case var case5 when case5 == AuraEffects_Names.SPELL_AURA_MOD_ROOT:
                                {
                                    CustomAttributs = CustomAttributs | (uint)SpellAttributesCustom.SPELL_ATTR_CU_AURA_CC | (uint)SpellAttributesCustom.SPELL_ATTR_CU_MOVEMENT_IMPAIR;
                                    break;
                                }

                            case var case6 when case6 == AuraEffects_Names.SPELL_AURA_MOD_DECREASE_SPEED:
                                {
                                    CustomAttributs = CustomAttributs | (uint)SpellAttributesCustom.SPELL_ATTR_CU_MOVEMENT_IMPAIR;
                                    break;
                                }
                        }

                        switch (SpellEffects[i].ID)
                        {
                            case var case7 when case7 == SpellEffects_Names.SPELL_EFFECT_SCHOOL_DAMAGE:
                            case var case8 when case8 == SpellEffects_Names.SPELL_EFFECT_WEAPON_DAMAGE:
                            case var case9 when case9 == SpellEffects_Names.SPELL_EFFECT_WEAPON_DAMAGE_NOSCHOOL:
                            case var case10 when case10 == SpellEffects_Names.SPELL_EFFECT_WEAPON_PERCENT_DAMAGE:
                            case var case11 when case11 == SpellEffects_Names.SPELL_EFFECT_HEAL:
                                {
                                    CustomAttributs = CustomAttributs | (uint)SpellAttributesCustom.SPELL_ATTR_CU_DIRECT_DAMAGE;
                                    break;
                                }

                            case var case12 when case12 == SpellEffects_Names.SPELL_EFFECT_CHARGE:
                                {
                                    if (Speed == 0.0f && SpellFamilyName == 0)
                                    {
                                        Speed = 42.0f; // Charge default speed
                                    }

                                    CustomAttributs = CustomAttributs | (uint)SpellAttributesCustom.SPELL_ATTR_CU_CHARGE;
                                    break;
                                }
                        }
                    }
                }

                for (int i = 0; i <= 2; i++)
                {
                    if (SpellEffects[i] is object)
                    {
                        switch ((AuraEffects_Names)SpellEffects[i].ApplyAuraIndex)
                        {
                            case var case13 when case13 == AuraEffects_Names.SPELL_AURA_MOD_POSSESS:
                            case var case14 when case14 == AuraEffects_Names.SPELL_AURA_MOD_CONFUSE:
                            case var case15 when case15 == AuraEffects_Names.SPELL_AURA_MOD_CHARM:
                            case var case16 when case16 == AuraEffects_Names.SPELL_AURA_MOD_FEAR:
                            case var case17 when case17 == AuraEffects_Names.SPELL_AURA_MOD_STUN:
                                {
                                    // TODO : Find a way to handle this? Likely have to check original source in VB to figure out what it's doing.
                                    CustomAttributs = (CustomAttributs | (uint)SpellAttributesCustom.SPELL_ATTR_CU_AURA_CC) & !SpellAttributesCustom.SPELL_ATTR_CU_MOVEMENT_IMPAIR;
                                    break;
                                }
                        }
                    }
                }

                if (SpellVisual == 3879)
                {
                    CustomAttributs = CustomAttributs | (uint)SpellAttributesCustom.SPELL_ATTR_CU_CONE_BACK;
                }

                switch (ID)
                {
                    case 26029: // Dark Glare
                        {
                            CustomAttributs = CustomAttributs | (uint)SpellAttributesCustom.SPELL_ATTR_CU_CONE_LINE;
                            break;
                        }

                    case 24340:
                    case 26558:
                    case 28884:
                    case 26789: // Meteor
                        {
                            CustomAttributs = CustomAttributs | (uint)SpellAttributesCustom.SPELL_ATTR_CU_SHARE_DAMAGE;
                            break;
                        }

                    case 8122:
                    case 8124:
                    case 10888:
                    case 10890:
                    case 12494: // Psychic Scream, Frostbite
                        {
                            Attributes = Attributes | (int)SpellAttributes.SPELL_ATTR_BREAKABLE_BY_DAMAGE;
                            break;
                        }
                }
            }

            public void Cast(ref CastSpellParameters castParams)
            {
                try
                {
                    var Caster = castParams.Caster;
                    var Targets = castParams.Targets;
                    short CastFlags = 2;
                    if (IsRanged)
                        CastFlags = (short)(CastFlags | (short)SpellCastFlags.CAST_FLAG_RANGED); // Ranged
                    var spellStart = new Packets.PacketClass(OPCODES.SMSG_SPELL_START);
                    // SpellCaster (If the spell is casted by a item, then it's the item guid here, else caster guid)
                    if (castParams.Item is object)
                    {
                        spellStart.AddPackGUID(castParams.Item.GUID);
                    }
                    else
                    {
                        spellStart.AddPackGUID(castParams.Caster.GUID);
                    }

                    spellStart.AddPackGUID(castParams.Caster.GUID); // SpellCaster
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

                    // DONE: Write ammo to packet
                    if ((CastFlags & (short)SpellCastFlags.CAST_FLAG_RANGED) != 0)
                    {
                        WS_Base.BaseUnit argCaster = (WS_Base.BaseUnit)Caster;
                        WriteAmmoToPacket(ref spellStart, ref argCaster);
                    }

                    Caster.SendToNearPlayers(ref spellStart);
                    spellStart.Dispose();

                    // PREPEARING SPELL
                    castParams.State = SpellCastState.SPELL_STATE_PREPARING;
                    castParams.Stopped = false;
                    if (Caster is WS_Creatures.CreatureObject)
                    {
                        if (Targets.unitTarget is object)
                        {
                            WS_Base.BaseObject uTarget = Targets.unitTarget;
                            ((WS_Creatures.CreatureObject)Caster).TurnTo(ref uTarget);
                        }
                        else if ((Targets.targetMask & (int)SpellCastTargetFlags.TARGET_FLAG_DEST_LOCATION) != 0)
                        {
                            ((WS_Creatures.CreatureObject)Caster).TurnTo(Targets.dstX, Targets.dstY);
                        }
                    }

                    // DONE: Log spell
                    bool NeedSpellLog = true;
                    for (byte i = 0; i <= 2; i++)
                    {
                        if (SpellEffects[i] is object)
                        {
                            if (SpellEffects[i].ID == SpellEffects_Names.SPELL_EFFECT_SCHOOL_DAMAGE)
                                NeedSpellLog = false;
                        }
                    }

                    if (NeedSpellLog)
                        SendSpellLog(ref Caster, ref Targets);

                    // DONE: Wait for the castingtime
                    if (castParams.InstantCast == false && GetCastTime > 0)
                    {
                        Thread.Sleep(GetCastTime);
                        // DONE: Delayed spells
                        while (castParams.Delayed > 0)
                        {
                            int delayTime = castParams.Delayed;
                            castParams.Delayed = 0;
                            Thread.Sleep(delayTime);
                        }
                    }

                    if (castParams.Stopped || castParams.State != SpellCastState.SPELL_STATE_PREPARING)
                    {
                        // Has been interrupted, please abort
                        castParams.Dispose(); // Clean up when we don't need it anymore
                        return;
                    }

                    // CASTING SPELL
                    castParams.State = SpellCastState.SPELL_STATE_CASTING;

                    // DONE: Calculate the time it takes until the spell is at the target
                    int SpellTime = 0;
                    float SpellDistance = 0f;
                    if (Speed > 0f)
                    {
                        if ((Targets.targetMask & (int)SpellCastTargetFlags.TARGET_FLAG_UNIT) != 0 && Targets.unitTarget is object)
                            SpellDistance = WorldServiceLocator._WS_Combat.GetDistance(Caster, Targets.unitTarget);
                        if ((Targets.targetMask & (int)SpellCastTargetFlags.TARGET_FLAG_DEST_LOCATION) != 0 && (Targets.dstX != 0f || Targets.dstY != 0f || Targets.dstZ != 0f))
                            SpellDistance = WorldServiceLocator._WS_Combat.GetDistance(Caster, Targets.dstX, Targets.dstY, Targets.dstZ);
                        if ((Targets.targetMask & (int)SpellCastTargetFlags.TARGET_FLAG_OBJECT) != 0 && Targets.goTarget is object)
                            SpellDistance = WorldServiceLocator._WS_Combat.GetDistance(Caster, Targets.goTarget);
                        if (SpellDistance > 0f)
                            SpellTime = (int)Conversion.Fix(SpellDistance / Speed * 1000f);
                    }

                    // DONE: Do one more control to see if you still can cast the spell (only if it's not instant)
                    SpellFailedReason SpellCastError = SpellFailedReason.SPELL_NO_ERROR;
                    if ((castParams.InstantCast == false || GetCastTime == 0) && Caster is WS_PlayerData.CharacterObject)
                    {
                        SpellFailedReason localCanCast() { WS_PlayerData.CharacterObject argCharacter = (WS_PlayerData.CharacterObject)Caster; var ret = CanCast(ref argCharacter, Targets, false); return ret; }

                        SpellCastError = localCanCast();
                        if (SpellCastError != SpellFailedReason.SPELL_NO_ERROR)
                        {
                            WorldServiceLocator._WS_Spells.SendCastResult(SpellCastError, ref ((WS_PlayerData.CharacterObject)Caster).client, ID);
                            castParams.State = SpellCastState.SPELL_STATE_IDLE;
                            castParams.Dispose(); // Clean up when we don't need it anymore
                            return;
                        }
                    }

                    // DONE: Get the targets
                    var TargetsInfected = new Dictionary<WS_Base.BaseObject, SpellMissInfo>[3];
                    TargetsInfected[0] = GetTargets(ref Caster, Targets, 0);
                    TargetsInfected[1] = GetTargets(ref Caster, Targets, 1);
                    TargetsInfected[2] = GetTargets(ref Caster, Targets, 2);

                    // DONE: On next attack
                    if ((Attributes & (int)SpellAttributes.SPELL_ATTR_NEXT_ATTACK) != 0 || (Attributes & (int)SpellAttributes.SPELL_ATTR_NEXT_ATTACK2) != 0)
                    {
                        if (Caster is WS_PlayerData.CharacterObject)
                        {
                            if (((WS_PlayerData.CharacterObject)Caster).attackState.combatNextAttackSpell)
                            {
                                WorldServiceLocator._WS_Spells.SendCastResult(SpellFailedReason.SPELL_FAILED_SPELL_IN_PROGRESS, ref ((WS_PlayerData.CharacterObject)Caster).client, ID);
                                castParams.Dispose(); // Clean up when we don't need it anymore
                                return;
                            } ((WS_PlayerData.CharacterObject)Caster).attackState.combatNextAttackSpell = true;
                            ((WS_PlayerData.CharacterObject)Caster).attackState.combatNextAttack.WaitOne();
                        }
                    }

                    // Send cooldown, Drain power and reagents
                    if (Caster is WS_PlayerData.CharacterObject)
                    {
                        {
                            var withBlock = (WS_PlayerData.CharacterObject)Caster;
                            // DONE: Spell cooldown
                            WS_PlayerData.CharacterObject argobjCharacter = (WS_PlayerData.CharacterObject)Caster;
                            ItemObject argcastItem = null;
                            SendSpellCooldown(ref argobjCharacter, castItem: ref argcastItem);

                            // DONE: Get reagents
                            for (byte i = 0; i <= 7; i++)
                            {
                                if (Conversions.ToBoolean(Reagents[i]) && Conversions.ToBoolean(ReagentsCount[i]))
                                {
                                    withBlock.ItemCONSUME(Reagents[i], ReagentsCount[i]);
                                }
                            }

                            // DONE: Get arrows for ranged spells
                            if (IsRanged)
                            {
                                if (withBlock.AmmoID > 0)
                                    withBlock.ItemCONSUME(withBlock.AmmoID, 1);
                            }

                            // DONE: Get mana
                            switch (powerType)
                            {
                                case var @case when @case == ManaTypes.TYPE_MANA:
                                    {
                                        // DONE: Drain all power for some spells
                                        int ManaCost = 0;
                                        if (AttributesEx & SpellAttributesEx.SPELL_ATTR_EX_DRAIN_ALL_POWER)
                                        {
                                            withBlock.Mana.Current = 0;
                                            ManaCost = 1; // To add the 5 second rule :)
                                        }
                                        else
                                        {
                                            ManaCost = get_GetManaCost(withBlock.Level, withBlock.Mana.Base);
                                            withBlock.Mana.Current -= ManaCost;
                                        }
                                        // DONE: 5 second rule
                                        if (ManaCost > 0)
                                        {
                                            withBlock.spellCastManaRegeneration = 5;
                                            withBlock.SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER1, withBlock.Mana.Current);
                                            withBlock.GroupUpdateFlag = withBlock.GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_CUR_POWER;
                                            withBlock.SendCharacterUpdate();
                                        }

                                        break;
                                    }

                                case var case1 when case1 == ManaTypes.TYPE_RAGE:
                                    {
                                        // DONE: Drain all power for some spells
                                        if (AttributesEx & SpellAttributesEx.SPELL_ATTR_EX_DRAIN_ALL_POWER)
                                        {
                                            withBlock.Rage.Current = 0;
                                        }
                                        else
                                        {
                                            withBlock.Rage.Current -= get_GetManaCost(withBlock.Level, withBlock.Rage.Base);
                                        }

                                        withBlock.SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER2, withBlock.Rage.Current);
                                        withBlock.GroupUpdateFlag = withBlock.GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_CUR_POWER;
                                        withBlock.SendCharacterUpdate();
                                        break;
                                    }

                                case var case2 when case2 == ManaTypes.TYPE_HEALTH:
                                    {
                                        // DONE: Drain all power for some spells
                                        // TODO: If there are spells using it, should you die or end up with 1 hp?
                                        if (AttributesEx & SpellAttributesEx.SPELL_ATTR_EX_DRAIN_ALL_POWER)
                                        {
                                            withBlock.Life.Current = 1;
                                        }
                                        else
                                        {
                                            withBlock.Life.Current -= get_GetManaCost(withBlock.Level, withBlock.Life.Base);
                                        }

                                        withBlock.SetUpdateFlag(EUnitFields.UNIT_FIELD_HEALTH, withBlock.Life.Current);
                                        withBlock.GroupUpdateFlag = withBlock.GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_CUR_HP;
                                        withBlock.SendCharacterUpdate();
                                        break;
                                    }

                                case var case3 when case3 == ManaTypes.TYPE_ENERGY:
                                    {
                                        // DONE: Drain all power for some spells
                                        if (AttributesEx & SpellAttributesEx.SPELL_ATTR_EX_DRAIN_ALL_POWER)
                                        {
                                            withBlock.Energy.Current = 0;
                                        }
                                        else
                                        {
                                            withBlock.Energy.Current -= get_GetManaCost(withBlock.Level, withBlock.Energy.Base);
                                        }

                                        withBlock.SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER4, withBlock.Energy.Current);
                                        withBlock.GroupUpdateFlag = withBlock.GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_CUR_POWER;
                                        withBlock.SendCharacterUpdate();
                                        break;
                                    }
                            }

                            // DONE: Remove auras when casting a spell
                            withBlock.RemoveAurasByInterruptFlag(SpellAuraInterruptFlags.AURA_INTERRUPT_FLAG_CAST_SPELL);

                            // DONE: Check if the spell was needed for a quest
                            if (Targets.unitTarget is object && Targets.unitTarget is WS_Creatures.CreatureObject)
                            {
                                WS_PlayerData.CharacterObject argobjCharacter1 = (WS_PlayerData.CharacterObject)Caster;
                                WS_Creatures.CreatureObject argcreature = (WS_Creatures.CreatureObject)Targets.unitTarget;
                                WorldServiceLocator._WorldServer.ALLQUESTS.OnQuestCastSpell(ref argobjCharacter1, ref argcreature, ID);
                            }

                            if (Targets.goTarget is object)
                            {
                                WS_PlayerData.CharacterObject argobjCharacter2 = (WS_PlayerData.CharacterObject)Caster;
                                WS_GameObjects.GameObjectObject arggameObject = (WS_GameObjects.GameObjectObject)Targets.goTarget;
                                WorldServiceLocator._WorldServer.ALLQUESTS.OnQuestCastSpell(ref argobjCharacter2, ref arggameObject, ID);
                            }
                        }
                    }
                    else if (Caster is WS_Creatures.CreatureObject)
                    {
                        {
                            var withBlock1 = (WS_Creatures.CreatureObject)Caster;
                            // DONE: Get mana from creatures
                            switch (powerType)
                            {
                                case var case4 when case4 == ManaTypes.TYPE_MANA:
                                    {
                                        withBlock1.Mana.Current -= get_GetManaCost(withBlock1.Level, withBlock1.Mana.Base);

                                        // DONE: Send the update
                                        var updatePacket = new Packets.UpdatePacketClass();
                                        var powerUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
                                        powerUpdate.SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER1, withBlock1.Mana.Current);
                                        powerUpdate.AddToPacket(updatePacket, ObjectUpdateType.UPDATETYPE_VALUES, (WS_Creatures.CreatureObject)Caster);
                                        Packets.PacketClass argpacket = updatePacket;
                                        withBlock1.SendToNearPlayers(ref argpacket);
                                        powerUpdate.Dispose();
                                        break;
                                    }

                                case var case5 when case5 == ManaTypes.TYPE_HEALTH:
                                    {
                                        withBlock1.Life.Current -= get_GetManaCost(withBlock1.Level, withBlock1.Life.Base);

                                        // DONE: Send the update
                                        var updatePacket = new Packets.UpdatePacketClass();
                                        var powerUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
                                        powerUpdate.SetUpdateFlag(EUnitFields.UNIT_FIELD_HEALTH, withBlock1.Life.Current);
                                        powerUpdate.AddToPacket(updatePacket, ObjectUpdateType.UPDATETYPE_VALUES, (WS_Creatures.CreatureObject)Caster);
                                        Packets.PacketClass argpacket1 = updatePacket;
                                        withBlock1.SendToNearPlayers(ref argpacket1);
                                        powerUpdate.Dispose();
                                        break;
                                    }
                            }
                        }
                    }

                    castParams.State = SpellCastState.SPELL_STATE_FINISHED;

                    // DONE: Send the cast result
                    if (Caster is WS_PlayerData.CharacterObject)
                        WorldServiceLocator._WS_Spells.SendCastResult(SpellFailedReason.SPELL_NO_ERROR, ref ((WS_PlayerData.CharacterObject)Caster).client, ID);

                    // DONE: Send the GO message
                    var tmpTargets = new Dictionary<ulong, SpellMissInfo>();
                    for (byte i = 0; i <= 2; i++)
                    {
                        foreach (KeyValuePair<WS_Base.BaseObject, SpellMissInfo> tmpTarget in TargetsInfected[i])
                        {
                            if (!tmpTargets.ContainsKey(tmpTarget.Key.GUID))
                                tmpTargets.Add(tmpTarget.Key.GUID, tmpTarget.Value);
                        }
                    }

                    SendSpellGO(ref Caster, ref Targets, ref tmpTargets, ref castParams.Item);

                    // DONE: Start channel if it's a channeling spell
                    if (channelInterruptFlags != 0)
                    {
                        castParams.State = SpellCastState.SPELL_STATE_CASTING;
                        WS_Base.BaseUnit argCaster1 = (WS_Base.BaseUnit)Caster;
                        StartChannel(ref argCaster1, GetDuration, ref Targets);
                    }

                    if (Caster is WS_PlayerData.CharacterObject)
                    {
                        if (castParams.Item is object)
                        {
                            // DONE: If this spell use charges then reduce by one when we cast this spell
                            if (castParams.Item.ChargesLeft > 0)
                            {
                                castParams.Item.ChargesLeft -= 1;
                                ((WS_PlayerData.CharacterObject)Caster).SendItemUpdate(castParams.Item);
                            }

                            // DONE: Consume the item
                            if (castParams.Item.ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_CONSUMABLE)
                            {
                                castParams.Item.StackCount -= 1;
                                if (castParams.Item.StackCount <= 0)
                                {
                                    var bag = default(byte);
                                    byte slot;
                                    slot = ((WS_PlayerData.CharacterObject)Caster).ItemGetSLOTBAG(castParams.Item.GUID, ref bag);
                                    if (bag != WorldServiceLocator._Global_Constants.ITEM_SLOT_NULL & slot != WorldServiceLocator._Global_Constants.ITEM_SLOT_NULL)
                                    {
                                        ((WS_PlayerData.CharacterObject)Caster).ItemREMOVE(bag, slot, true, true);
                                    }
                                }
                                else
                                {
                                    ((WS_PlayerData.CharacterObject)Caster).SendItemUpdate(castParams.Item);
                                }
                            }
                        }
                    }

                    if (castParams.State == SpellCastState.SPELL_STATE_FINISHED)
                        castParams.State = SpellCastState.SPELL_STATE_IDLE;
                    if (SpellTime > 0)
                        Thread.Sleep(SpellTime);

                    // APPLYING EFFECTS
                    var TargetHits = new List<WS_Base.BaseObject>[3];
                    TargetHits[0] = GetHits(ref TargetsInfected[0]);
                    TargetHits[1] = GetHits(ref TargetsInfected[1]);
                    TargetHits[2] = GetHits(ref TargetsInfected[2]);
                    for (byte i = 0; i <= 2; i++)
                    {
                        if (SpellEffects[i] is object)
                        {
                            /* TODO ERROR: Skipped IfDirectiveTrivia */
                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "DEBUG: Casting effect: {0}", SpellEffects[i].ID);
                            /* TODO ERROR: Skipped EndIfDirectiveTrivia */
                            SpellCastError = WorldServiceLocator._WS_Spells.SPELL_EFFECTs[SpellEffects[i].ID].Invoke(ref Targets, ref Caster, ref SpellEffects[i], ID, ref TargetHits[i], ref castParams.Item);
                            if (SpellCastError != SpellFailedReason.SPELL_NO_ERROR)
                                break;
                        }
                    }

                    if (SpellCastError != SpellFailedReason.SPELL_NO_ERROR)
                    {
                        if (Caster is WS_PlayerData.CharacterObject)
                        {
                            WorldServiceLocator._WS_Spells.SendCastResult(SpellCastError, ref ((WS_PlayerData.CharacterObject)Caster).client, ID);
                            WS_Base.BaseUnit argCaster2 = (WS_Base.BaseUnit)Caster;
                            SendInterrupted(0, ref argCaster2);
                            castParams.Dispose(); // Clean up when we don't need it anymore
                            return;
                        }
                        else
                        {
                            WS_Base.BaseUnit argCaster3 = (WS_Base.BaseUnit)Caster;
                            SendInterrupted(0, ref argCaster3);
                            castParams.Dispose(); // Clean up when we don't need it anymore
                            return;
                        }
                    }

                    // DONE: If channel, wait for the duration before we finish it
                    if (castParams.State == SpellCastState.SPELL_STATE_CASTING)
                    {
                        if (channelInterruptFlags != 0)
                        {
                            Thread.Sleep(GetDuration);
                            castParams.State = SpellCastState.SPELL_STATE_IDLE;
                        }
                    }
                }
                catch (Exception e)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error when casting spell.{0}", Environment.NewLine + e.ToString());
                }

                if (castParams is object)
                {
                    try
                    {
                        castParams.Dispose(); // Clean up when we don't need it anymore
                    }
                    catch
                    {
                    }
                }
            }

            public void Apply(ref WS_Base.BaseObject caster, SpellTargets Targets)
            {
                var TargetsInfected = new List<WS_Base.BaseObject>[3];
                List<WS_Base.BaseObject> localGetHits() { var argTargets = GetTargets(ref caster, Targets, 0); var ret = GetHits(ref argTargets); return ret; }

                TargetsInfected[0] = localGetHits();
                List<WS_Base.BaseObject> localGetHits1() { var argTargets = GetTargets(ref caster, Targets, 1); var ret = GetHits(ref argTargets); return ret; }

                TargetsInfected[1] = localGetHits1();
                List<WS_Base.BaseObject> localGetHits2() { var argTargets = GetTargets(ref caster, Targets, 2); var ret = GetHits(ref argTargets); return ret; }

                TargetsInfected[2] = localGetHits2();
                if (SpellEffects[0] is object)
                {
                    ItemObject argItem = null;
                    WorldServiceLocator._WS_Spells.SPELL_EFFECTs[SpellEffects[0].ID].Invoke(ref Targets, ref caster, ref SpellEffects[0], ID, ref TargetsInfected[0], ref argItem);
                }

                if (SpellEffects[1] is object)
                {
                    ItemObject argItem1 = null;
                    WorldServiceLocator._WS_Spells.SPELL_EFFECTs[SpellEffects[1].ID].Invoke(ref Targets, ref caster, ref SpellEffects[1], ID, ref TargetsInfected[1], ref argItem1);
                }

                if (SpellEffects[2] is object)
                {
                    ItemObject argItem2 = null;
                    WorldServiceLocator._WS_Spells.SPELL_EFFECTs[SpellEffects[2].ID].Invoke(ref Targets, ref caster, ref SpellEffects[2], ID, ref TargetsInfected[2], ref argItem2);
                }
            }

            public SpellFailedReason CanCast(ref WS_PlayerData.CharacterObject Character, SpellTargets Targets, bool FirstCheck)
            {
                if (Character.Spell_Silenced)
                    return SpellFailedReason.SPELL_FAILED_SILENCED;
                if (Character.cUnitFlags & UnitFlags.UNIT_FLAG_TAXI_FLIGHT)
                    return SpellFailedReason.SPELL_FAILED_ERROR;
                if (Targets.unitTarget is object && !ReferenceEquals(Targets.unitTarget, Character))
                {
                    if (Conversions.ToBoolean(FacingCasterFlags & 1))
                    {
                        WS_Base.BaseObject argObject1 = Character;
                        if (WorldServiceLocator._WS_Combat.IsInFrontOf(ref argObject1, ref (WS_Base.BaseObject)Targets.unitTarget) == false)
                            return SpellFailedReason.SPELL_FAILED_NOT_INFRONT;
                    }

                    if (Conversions.ToBoolean(FacingCasterFlags & 2))
                    {
                        WS_Base.BaseObject argObject11 = Character;
                        if (WorldServiceLocator._WS_Combat.IsInBackOf(ref argObject11, ref (WS_Base.BaseObject)Targets.unitTarget) == false)
                            return SpellFailedReason.SPELL_FAILED_NOT_BEHIND;
                    }
                }

                if (Targets.unitTarget is object && !ReferenceEquals(Targets.unitTarget, Character) && Targets.unitTarget is WS_PlayerData.CharacterObject && ((WS_PlayerData.CharacterObject)Targets.unitTarget).GM)
                    return SpellFailedReason.SPELL_FAILED_BAD_TARGETS;
                if ((Attributes & SpellAttributes.SPELL_ATTR_WHILE_DEAD) == 0 && Character.IsDead)
                    return SpellFailedReason.SPELL_FAILED_CASTER_DEAD;
                if (Attributes & SpellAttributes.SPELL_ATTR_NOT_WHILE_COMBAT)
                {
                    if (Character.IsInCombat)
                        return SpellFailedReason.SPELL_FAILED_INTERRUPTED_COMBAT;
                }

                int StanceMask = 0;
                if (Character.ShapeshiftForm > ShapeshiftForm.FORM_NORMAL)
                    StanceMask = 1 << Character.ShapeshiftForm - 1;
                if (Conversions.ToBoolean(StanceMask & ShapeshiftExclude))
                    return SpellFailedReason.SPELL_FAILED_NOT_SHAPESHIFT;
                if ((StanceMask & RequredCasterStance) == 0)
                {
                    bool actAsShifted = false;
                    if (Character.ShapeshiftForm > ShapeshiftForm.FORM_NORMAL)
                    {
                        var ShapeShift = WorldServiceLocator._WS_DBCDatabase.FindShapeshiftForm(Character.ShapeshiftForm);
                        if (ShapeShift is object)
                        {
                            if ((ShapeShift.Flags1 & 1) == 0)
                            {
                                actAsShifted = true;
                            }
                            else
                            {
                                actAsShifted = false;
                            }
                        }
                        else
                        {
                            goto SkipShapeShift;
                        }
                    }

                    if (actAsShifted)
                    {
                        if (Attributes & SpellAttributes.SPELL_ATTR_NOT_WHILE_SHAPESHIFTED)
                        {
                            return SpellFailedReason.SPELL_FAILED_ONLY_SHAPESHIFT;
                        }
                        else if (RequredCasterStance != 0)
                        {
                            return SpellFailedReason.SPELL_FAILED_ONLY_SHAPESHIFT;
                        }
                    }
                    else if ((AttributesEx2 & SpellAttributesEx2.SPELL_ATTR_EX2_NOT_NEED_SHAPESHIFT) == 0 && RequredCasterStance != 0)
                    {
                        return SpellFailedReason.SPELL_FAILED_ONLY_SHAPESHIFT;
                    }
                }

            SkipShapeShift:
                ;
                if (Attributes & SpellAttributes.SPELL_ATTR_REQ_STEALTH & Character.Invisibility != InvisibilityLevel.STEALTH)
                {
                    return SpellFailedReason.SPELL_FAILED_ONLY_STEALTHED;
                }

                if (Character.charMovementFlags & WorldServiceLocator._Global_Constants.movementFlagsMask)
                {
                    if (((Character.charMovementFlags & MovementFlags.MOVEMENTFLAG_FALLING) == 0 || SpellEffects[0].ID != SpellEffects_Names.SPELL_EFFECT_STUCK) && (IsAutoRepeat || auraInterruptFlags & SpellAuraInterruptFlags.AURA_INTERRUPT_FLAG_NOT_SEATED))
                    {
                        return SpellFailedReason.SPELL_FAILED_MOVING;
                    }
                }

                int ManaCost = get_GetManaCost(Character.Level, Character.Mana.Base);
                if (ManaCost > 0)
                {
                    if (powerType != Character.ManaType)
                        return SpellFailedReason.SPELL_FAILED_NO_POWER;
                    switch (powerType)
                    {
                        case var @case when @case == ManaTypes.TYPE_MANA:
                            {
                                if (ManaCost > Character.Mana.Current)
                                    return SpellFailedReason.SPELL_FAILED_NO_POWER;
                                break;
                            }

                        case var case1 when case1 == ManaTypes.TYPE_RAGE:
                            {
                                if (ManaCost > Character.Rage.Current)
                                    return SpellFailedReason.SPELL_FAILED_NO_POWER;
                                break;
                            }

                        case var case2 when case2 == ManaTypes.TYPE_HEALTH:
                            {
                                if (ManaCost > Character.Life.Current)
                                    return SpellFailedReason.SPELL_FAILED_NO_POWER;
                                break;
                            }

                        case var case3 when case3 == ManaTypes.TYPE_ENERGY:
                            {
                                if (ManaCost > Character.Energy.Current)
                                    return SpellFailedReason.SPELL_FAILED_NO_POWER;
                                break;
                            }

                        default:
                            {
                                return SpellFailedReason.SPELL_FAILED_UNKNOWN;
                            }
                    }
                }

                if (!FirstCheck)
                {
                    if (Mechanic != 0 && Targets.unitTarget is object && (Targets.unitTarget.MechanicImmunity & 1 << Mechanic - 1) != 0L)
                        return SpellFailedReason.SPELL_FAILED_IMMUNE;
                }

                if (EquippedItemClass > 0 && EquippedItemSubClass > 0)
                {
                    if (EquippedItemClass == ITEM_CLASS.ITEM_CLASS_WEAPON)
                    {
                        bool FoundItem = false;
                        for (int i = ITEM_SUBCLASS.ITEM_SUBCLASS_AXE, loopTo = ITEM_SUBCLASS.ITEM_SUBCLASS_FISHING_POLE; i <= loopTo; i++)
                        {
                            if (Conversions.ToBoolean(EquippedItemSubClass & 1 << i))
                            {
                                switch (i)
                                {
                                    case var case4 when case4 == ITEM_SUBCLASS.ITEM_SUBCLASS_TWOHAND_AXE:
                                    case var case5 when case5 == ITEM_SUBCLASS.ITEM_SUBCLASS_TWOHAND_MACE:
                                    case var case6 when case6 == ITEM_SUBCLASS.ITEM_SUBCLASS_TWOHAND_SWORD:
                                    case var case7 when case7 == ITEM_SUBCLASS.ITEM_SUBCLASS_STAFF:
                                    case var case8 when case8 == ITEM_SUBCLASS.ITEM_SUBCLASS_POLEARM:
                                    case var case9 when case9 == ITEM_SUBCLASS.ITEM_SUBCLASS_SPEAR:
                                    case var case10 when case10 == ITEM_SUBCLASS.ITEM_SUBCLASS_FISHING_POLE:
                                        {
                                            if (Character.Items.ContainsKey(EquipmentSlots.EQUIPMENT_SLOT_MAINHAND) && Character.Items(EquipmentSlots.EQUIPMENT_SLOT_MAINHAND).IsBroken == false && Character.Items(EquipmentSlots.EQUIPMENT_SLOT_MAINHAND).ItemInfo.ObjectClass == EquippedItemClass)
                                            {
                                                FoundItem = true;
                                                break;
                                            }

                                            break;
                                        }

                                    case var case11 when case11 == ITEM_SUBCLASS.ITEM_SUBCLASS_AXE:
                                    case var case12 when case12 == ITEM_SUBCLASS.ITEM_SUBCLASS_MACE:
                                    case var case13 when case13 == ITEM_SUBCLASS.ITEM_SUBCLASS_SWORD:
                                    case var case14 when case14 == ITEM_SUBCLASS.ITEM_SUBCLASS_FIST_WEAPON:
                                    case var case15 when case15 == ITEM_SUBCLASS.ITEM_SUBCLASS_DAGGER:
                                        {
                                            if (Character.Items.ContainsKey(EquipmentSlots.EQUIPMENT_SLOT_MAINHAND) && Character.Items(EquipmentSlots.EQUIPMENT_SLOT_MAINHAND).IsBroken == false && Character.Items(EquipmentSlots.EQUIPMENT_SLOT_MAINHAND).ItemInfo.ObjectClass == EquippedItemClass)
                                            {
                                                FoundItem = true;
                                                break;
                                            }

                                            if (Character.Items.ContainsKey(EquipmentSlots.EQUIPMENT_SLOT_OFFHAND) && Character.Items(EquipmentSlots.EQUIPMENT_SLOT_OFFHAND).IsBroken == false && Character.Items(EquipmentSlots.EQUIPMENT_SLOT_OFFHAND).ItemInfo.ObjectClass == EquippedItemClass)
                                            {
                                                FoundItem = true;
                                                break;
                                            }

                                            break;
                                        }

                                    case var case16 when case16 == ITEM_SUBCLASS.ITEM_SUBCLASS_BOW:
                                    case var case17 when case17 == ITEM_SUBCLASS.ITEM_SUBCLASS_CROSSBOW:
                                    case var case18 when case18 == ITEM_SUBCLASS.ITEM_SUBCLASS_GUN:
                                    case var case19 when case19 == ITEM_SUBCLASS.ITEM_SUBCLASS_WAND:
                                    case var case20 when case20 == ITEM_SUBCLASS.ITEM_SUBCLASS_THROWN:
                                        {
                                            if (Character.Items.ContainsKey(EquipmentSlots.EQUIPMENT_SLOT_RANGED) && Character.Items(EquipmentSlots.EQUIPMENT_SLOT_RANGED).IsBroken == false && Character.Items(EquipmentSlots.EQUIPMENT_SLOT_RANGED).ItemInfo.ObjectClass == EquippedItemClass)
                                            {
                                                if (i == ITEM_SUBCLASS.ITEM_SUBCLASS_BOW || i == ITEM_SUBCLASS.ITEM_SUBCLASS_CROSSBOW || i == ITEM_SUBCLASS.ITEM_SUBCLASS_GUN)
                                                {
                                                    if (Character.AmmoID == 0)
                                                        return SpellFailedReason.SPELL_FAILED_NO_AMMO;
                                                    if (Character.ItemCOUNT(Character.AmmoID) == 0)
                                                        return SpellFailedReason.SPELL_FAILED_NO_AMMO;
                                                }
                                                else if (i == ITEM_SUBCLASS.ITEM_SUBCLASS_THROWN)
                                                {
                                                    if (Conversions.ToBoolean(Character.ItemCOUNT(Character.Items(EquipmentSlots.EQUIPMENT_SLOT_RANGED).ItemEntry)))
                                                        return SpellFailedReason.SPELL_FAILED_NO_AMMO;
                                                }

                                                FoundItem = true;
                                                break;
                                            }

                                            break;
                                        }
                                }
                            }
                        }

                        if (FoundItem == false)
                            return SpellFailedReason.SPELL_FAILED_EQUIPPED_ITEM;
                    }
                }

                for (byte i = 0; i <= 2; i++)
                {
                    if (SpellEffects[i] is object)
                    {
                        switch (SpellEffects[i].ID)
                        {
                            case var case21 when case21 == SpellEffects_Names.SPELL_EFFECT_DUMMY:
                                {
                                    if (ID == 1648) // Execute
                                    {
                                        if (Targets.unitTarget is null || Targets.unitTarget.Life.Current > Targets.unitTarget.Life.Maximum * 0.2d)
                                            return SpellFailedReason.SPELL_FAILED_BAD_TARGETS;
                                    }

                                    break;
                                }

                            case var case22 when case22 == SpellEffects_Names.SPELL_EFFECT_SCHOOL_DAMAGE:
                                {
                                    if (SpellVisual == 7250) // Hammer of Wrath
                                    {
                                        if (Targets.unitTarget is null)
                                            return SpellFailedReason.SPELL_FAILED_BAD_IMPLICIT_TARGETS;
                                        if (Targets.unitTarget.Life.Current > Targets.unitTarget.Life.Maximum * 0.2d)
                                            return SpellFailedReason.SPELL_FAILED_BAD_TARGETS;
                                    }

                                    break;
                                }

                            case var case23 when case23 == SpellEffects_Names.SPELL_EFFECT_CHARGE:
                                {
                                    if (Character.IsRooted)
                                        return SpellFailedReason.SPELL_FAILED_ROOTED;
                                    break;
                                }

                            case var case24 when case24 == SpellEffects_Names.SPELL_EFFECT_SUMMON_OBJECT:
                                {
                                    if (SpellEffects[i].MiscValue == 35591)
                                    {
                                        float selectedX;
                                        float selectedY;
                                        if (SpellEffects[i].RadiusIndex > 0)
                                        {
                                            selectedX = (float)(Character.positionX + Math.Cos(Character.orientation) * SpellEffects[i].GetRadius);
                                            selectedY = (float)(Character.positionY + Math.Sin(Character.orientation) * SpellEffects[i].GetRadius);
                                        }
                                        else
                                        {
                                            selectedX = (float)(Character.positionX + Math.Cos(Character.orientation) * GetRange);
                                            selectedY = (float)(Character.positionY + Math.Sin(Character.orientation) * GetRange);
                                        }

                                        if (WorldServiceLocator._WS_Maps.GetZCoord(selectedX, selectedY, Character.MapID) > WorldServiceLocator._WS_Maps.GetWaterLevel(selectedX, selectedY, (int)Character.MapID))
                                            return SpellFailedReason.SPELL_FAILED_NOT_FISHABLE;
                                    }

                                    break;
                                }

                            case var case25 when case25 == SpellEffects_Names.SPELL_EFFECT_OPEN_LOCK:
                                {
                                    break;
                                }
                                // TODO: Fix this :P
                        }
                    }
                }

                // TODO: See if there was some thing that replaced this Spell Failed Reason, since it was not in 1.12
                // For i As Byte = 0 To 7
                // If Reagents(i) > 0 AndAlso ReagentsCount(i) > 0 Then
                // If Character.ItemCOUNT(Reagents(i)) < ReagentsCount(i) Then Return SpellFailedReason.SPELL_FAILED_REAGENTS
                // End If
                // Next

                // TODO: Check for same category - more powerful spell
                // If (Not Targets.unitTarget Is Nothing) Then
                // For i As Integer = 0 To _Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1
                // If Not Targets.unitTarget.ActiveSpells(i) Is Nothing Then
                // If Targets.unitTarget.ActiveSpells(i).SpellID <> 0 AndAlso _
                // CType(SPELLs(Targets.unitTarget.ActiveSpells(i).SpellID), SpellInfo).Category = Category AndAlso _
                // CType(SPELLs(Targets.unitTarget.ActiveSpells(i).SpellID), SpellInfo).spellLevel >= spellLevel Then
                // Return SpellFailedReason.SPELL_FAILED_AURA_BOUNCED
                // End If
                // End If
                // Next
                // End If

                // DONE: Check if the target is out of line of sight
                if (Targets.unitTarget is object)
                {
                    WS_Base.BaseObject argobj = Character;
                    WS_Base.BaseObject argobj2 = Targets.unitTarget;
                    if (WorldServiceLocator._WS_Maps.IsInLineOfSight(ref argobj, ref argobj2) == false)
                    {
                        return SpellFailedReason.SPELL_FAILED_LINE_OF_SIGHT;
                    }
                }
                else if (Targets.targetMask & SpellCastTargetFlags.TARGET_FLAG_DEST_LOCATION)
                {
                    WS_Base.BaseObject argobj1 = Character;
                    if (WorldServiceLocator._WS_Maps.IsInLineOfSight(ref argobj1, Targets.dstX, Targets.dstY, Targets.dstZ) == false)
                    {
                        return SpellFailedReason.SPELL_FAILED_LINE_OF_SIGHT;
                    }
                }

                return SpellFailedReason.SPELL_NO_ERROR;
            }

            public void StartChannel(ref WS_Base.BaseUnit Caster, int Duration, ref SpellTargets Targets)
            {
                if (Caster is WS_PlayerData.CharacterObject)
                {
                    var packet = new Packets.PacketClass(OPCODES.MSG_CHANNEL_START);
                    packet.AddInt32(ID);
                    packet.AddInt32(Duration);
                    ((WS_PlayerData.CharacterObject)Caster).client.Send(ref packet);
                    packet.Dispose();
                }
                else if (!(Caster is WS_Creatures.CreatureObject)) // Only characters and creatures can channel spells
                {
                    return;
                }

                var updatePacket = new Packets.UpdatePacketClass();
                var updateBlock = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
                updateBlock.SetUpdateFlag(EUnitFields.UNIT_CHANNEL_SPELL, ID);

                // DONE: Let others know what target we channel against
                if (Targets.unitTarget is object)
                {
                    updateBlock.SetUpdateFlag(EUnitFields.UNIT_FIELD_CHANNEL_OBJECT, Targets.unitTarget.GUID);
                }

                if (Caster is WS_Creatures.CreatureObject)
                {
                    updateBlock.AddToPacket(updatePacket, ObjectUpdateType.UPDATETYPE_VALUES, (WS_PlayerData.CharacterObject)Caster);
                }
                else if (Caster is WS_Creatures.CreatureObject)
                {
                    updateBlock.AddToPacket(updatePacket, ObjectUpdateType.UPDATETYPE_VALUES, (WS_Creatures.CreatureObject)Caster);
                }

                Packets.PacketClass argpacket = updatePacket;
                Caster.SendToNearPlayers(ref argpacket);
                updatePacket.Dispose();
            }

            public void WriteAmmoToPacket(ref Packets.PacketClass Packet, ref WS_Base.BaseUnit Caster)
            {
                WS_Items.ItemInfo ItemInfo = null;
                if (Caster is WS_PlayerData.CharacterObject)
                {
                    {
                        var withBlock = (WS_PlayerData.CharacterObject)Caster;
                        var RangedItem = withBlock.ItemGET(0, EquipmentSlots.EQUIPMENT_SLOT_RANGED);
                        if (RangedItem is object)
                        {
                            if (RangedItem.ItemInfo.InventoryType == INVENTORY_TYPES.INVTYPE_THROWN)
                            {
                                ItemInfo = RangedItem.ItemInfo;
                            }
                            else if (withBlock.AmmoID != 0 && WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(withBlock.AmmoID))
                            {
                                ItemInfo = WorldServiceLocator._WorldServer.ITEMDatabase[withBlock.AmmoID];
                            }
                        }
                    }
                }

                if (ItemInfo is null)
                {
                    if (WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(2512) == false)
                    {
                        var tmpInfo = new WS_Items.ItemInfo(2512);
                        WorldServiceLocator._WorldServer.ITEMDatabase.Add(2512, tmpInfo);
                    }

                    ItemInfo = WorldServiceLocator._WorldServer.ITEMDatabase[2512];
                }

                Packet.AddInt32(ItemInfo.Model); // Ammo Display ID
                Packet.AddInt32(ItemInfo.InventoryType); // Ammo Inventory Type
            }

            public void SendInterrupted(byte result, ref WS_Base.BaseUnit Caster)
            {
                if (Caster is WS_PlayerData.CharacterObject)
                {
                    var packet = new Packets.PacketClass(OPCODES.SMSG_SPELL_FAILURE);
                    packet.AddUInt64(Caster.GUID); // PackGuid?
                    packet.AddInt32(ID);
                    packet.AddInt8(result);
                    ((WS_PlayerData.CharacterObject)Caster).client.Send(ref packet);
                    packet.Dispose();
                }

                var packet2 = new Packets.PacketClass(OPCODES.SMSG_SPELL_FAILED_OTHER);
                packet2.AddUInt64(Caster.GUID); // PackGuid?
                packet2.AddInt32(ID);
                Caster.SendToNearPlayers(ref packet2);
                packet2.Dispose();
            }

            public void SendSpellGO(ref WS_Base.BaseObject Caster, ref SpellTargets Targets, ref Dictionary<ulong, SpellMissInfo> InfectedTargets, ref ItemObject Item)
            {
                short castFlags = 256;
                if (IsRanged)
                    castFlags = castFlags | SpellCastFlags.CAST_FLAG_RANGED;
                if (Item is object)
                    castFlags = castFlags | SpellCastFlags.CAST_FLAG_ITEM_CASTER;
                // TODO: If missed anyone SpellGoFlags.CAST_FLAG_EXTRA_MSG

                int hits = 0;
                int misses = 0;
                foreach (KeyValuePair<ulong, SpellMissInfo> Target in InfectedTargets)
                {
                    if (Target.Value == SpellMissInfo.SPELL_MISS_NONE)
                    {
                        hits += 1;
                    }
                    else
                    {
                        misses += 1;
                    }
                }

                var packet = new Packets.PacketClass(OPCODES.SMSG_SPELL_GO);
                // SpellCaster (If the spell is casted by a item, then it's the item guid here, else caster guid)
                if (Item is object)
                {
                    packet.AddPackGUID(Item.GUID);
                }
                else
                {
                    packet.AddPackGUID(Caster.GUID);
                }

                packet.AddPackGUID(Caster.GUID);                                 // SpellCaster
                packet.AddInt32(ID);                                             // SpellID
                packet.AddInt16(castFlags);                                      // Flags (&h20 - Ranged, &h100 - Item caster, &h400 - Targets resisted
                packet.AddInt8((byte)hits);                                            // Targets Count
                foreach (KeyValuePair<ulong, SpellMissInfo> Target in InfectedTargets)
                {
                    if (Target.Value == SpellMissInfo.SPELL_MISS_NONE)
                    {
                        packet.AddUInt64(Target.Key);                            // GUID
                    }
                }

                packet.AddInt8((byte)misses);                                          // Misses Count
                foreach (KeyValuePair<ulong, SpellMissInfo> Target in InfectedTargets)
                {
                    if (Target.Value != SpellMissInfo.SPELL_MISS_NONE)
                    {
                        packet.AddUInt64(Target.Key);                            // GUID
                        packet.AddInt8(Target.Value);                            // SpellMissInfo
                    }
                }

                Targets.WriteTargets(ref packet);

                // DONE: Write ammo to packet
                if (castFlags & SpellCastFlags.CAST_FLAG_RANGED)
                {
                    WS_Base.BaseUnit argCaster = (WS_Base.BaseUnit)Caster;
                    WriteAmmoToPacket(ref packet, ref argCaster);
                }

                Caster.SendToNearPlayers(ref packet);
                packet.Dispose();
            }

            public void SendSpellMiss(ref WS_Base.BaseObject Caster, ref WS_Base.BaseUnit Target, SpellMissInfo MissInfo)
            {
                var packet = new Packets.PacketClass(OPCODES.SMSG_SPELLLOGMISS);
                packet.AddInt32(ID);
                packet.AddUInt64(Caster.GUID);
                packet.AddInt8(0); // 0 or 1?
                packet.AddInt32(1); // Target Count
                packet.AddUInt64(Target.GUID);
                packet.AddInt8(MissInfo);
                Caster.SendToNearPlayers(ref packet);
                packet.Dispose();
            }

            public void SendSpellLog(ref WS_Base.BaseObject Caster, ref SpellTargets Targets)
            {
                var packet = new Packets.PacketClass(OPCODES.SMSG_SPELLLOGEXECUTE);
                if (Caster is WS_PlayerData.CharacterObject)
                {
                    packet.AddPackGUID(Caster.GUID);
                }
                else if (Targets.unitTarget is object)
                {
                    packet.AddPackGUID(Targets.unitTarget.GUID);
                }
                else
                {
                    packet.AddPackGUID(Caster.GUID);
                }

                packet.AddInt32(ID);
                int numOfSpellEffects = 1;
                packet.AddInt32(numOfSpellEffects); // EffectCount
                ulong UnitTargetGUID = 0UL;
                if (Targets.unitTarget is object)
                    UnitTargetGUID = Targets.unitTarget.GUID;
                ulong ItemTargetGUID = 0UL;
                if (Targets.itemTarget is object)
                    ItemTargetGUID = Targets.itemTarget.GUID;
                for (int i = 0, loopTo = numOfSpellEffects - 1; i <= loopTo; i++)
                {
                    packet.AddInt32(SpellEffects[i].ID); // EffectID
                    packet.AddInt32(1); // TargetCount
                    switch (SpellEffects[i].ID)
                    {
                        case var @case when @case == SpellEffects_Names.SPELL_EFFECT_MANA_DRAIN:
                            {
                                packet.AddPackGUID(UnitTargetGUID);
                                packet.AddInt32(0);
                                packet.AddInt32(0);
                                packet.AddSingle(0.0f);
                                break;
                            }

                        case var case1 when case1 == SpellEffects_Names.SPELL_EFFECT_ADD_EXTRA_ATTACKS:
                            {
                                packet.AddPackGUID(UnitTargetGUID);
                                packet.AddInt32(0); // Count?
                                break;
                            }

                        case var case2 when case2 == SpellEffects_Names.SPELL_EFFECT_INTERRUPT_CAST:
                            {
                                packet.AddPackGUID(UnitTargetGUID);
                                packet.AddInt32(0); // SpellID?
                                break;
                            }

                        case var case3 when case3 == SpellEffects_Names.SPELL_EFFECT_DURABILITY_DAMAGE:
                            {
                                packet.AddPackGUID(UnitTargetGUID);
                                packet.AddInt32(0);
                                packet.AddInt32(0);
                                break;
                            }

                        case var case4 when case4 == SpellEffects_Names.SPELL_EFFECT_OPEN_LOCK:
                        case var case5 when case5 == SpellEffects_Names.SPELL_EFFECT_OPEN_LOCK_ITEM:
                            {
                                packet.AddPackGUID(ItemTargetGUID);
                                break;
                            }

                        case var case6 when case6 == SpellEffects_Names.SPELL_EFFECT_CREATE_ITEM:
                            {
                                packet.AddInt32(SpellEffects[i].ItemType);
                                break;
                            }

                        case var case7 when case7 == SpellEffects_Names.SPELL_EFFECT_SUMMON:
                        case var case8 when case8 == SpellEffects_Names.SPELL_EFFECT_SUMMON_WILD:
                        case var case9 when case9 == SpellEffects_Names.SPELL_EFFECT_SUMMON_GUARDIAN:
                        case var case10 when case10 == SpellEffects_Names.SPELL_EFFECT_SUMMON_OBJECT:
                        case var case11 when case11 == SpellEffects_Names.SPELL_EFFECT_SUMMON_PET:
                        case var case12 when case12 == SpellEffects_Names.SPELL_EFFECT_SUMMON_POSSESSED:
                        case var case13 when case13 == SpellEffects_Names.SPELL_EFFECT_SUMMON_TOTEM:
                        case var case14 when case14 == SpellEffects_Names.SPELL_EFFECT_SUMMON_OBJECT_WILD:
                        case var case15 when case15 == SpellEffects_Names.SPELL_EFFECT_CREATE_HOUSE:
                        case var case16 when case16 == SpellEffects_Names.SPELL_EFFECT_DUEL:
                        case var case17 when case17 == SpellEffects_Names.SPELL_EFFECT_SUMMON_TOTEM_SLOT1:
                        case var case18 when case18 == SpellEffects_Names.SPELL_EFFECT_SUMMON_TOTEM_SLOT2:
                        case var case19 when case19 == SpellEffects_Names.SPELL_EFFECT_SUMMON_TOTEM_SLOT3:
                        case var case20 when case20 == SpellEffects_Names.SPELL_EFFECT_SUMMON_TOTEM_SLOT4:
                        case var case21 when case21 == SpellEffects_Names.SPELL_EFFECT_SUMMON_PHANTASM:
                        case var case22 when case22 == SpellEffects_Names.SPELL_EFFECT_SUMMON_CRITTER:
                        case var case23 when case23 == SpellEffects_Names.SPELL_EFFECT_SUMMON_OBJECT_SLOT1:
                        case var case24 when case24 == SpellEffects_Names.SPELL_EFFECT_SUMMON_OBJECT_SLOT2:
                        case var case25 when case25 == SpellEffects_Names.SPELL_EFFECT_SUMMON_OBJECT_SLOT3:
                        case var case26 when case26 == SpellEffects_Names.SPELL_EFFECT_SUMMON_OBJECT_SLOT4:
                        case var case27 when case27 == SpellEffects_Names.SPELL_EFFECT_SUMMON_DEMON:
                        case var case28 when case28 == SpellEffects_Names.SPELL_EFFECT_150:
                            {
                                if (Targets.unitTarget is object)
                                {
                                    packet.AddPackGUID(Targets.unitTarget.GUID);
                                }
                                else if (Targets.itemTarget is object)
                                {
                                    packet.AddPackGUID(Targets.itemTarget.GUID);
                                }
                                else if (Targets.goTarget is object)
                                {
                                    packet.AddPackGUID(Targets.goTarget.GUID);
                                }
                                else
                                {
                                    packet.AddInt8(0);
                                }

                                break;
                            }

                        case var case29 when case29 == SpellEffects_Names.SPELL_EFFECT_FEED_PET:
                            {
                                packet.AddInt32(Targets.itemTarget.ItemEntry);
                                break;
                            }

                        case var case30 when case30 == SpellEffects_Names.SPELL_EFFECT_DISMISS_PET:
                            {
                                packet.AddPackGUID(UnitTargetGUID);
                                break;
                            }

                        default:
                            {
                                return;
                            }
                    }
                }

                Caster.SendToNearPlayers(ref packet);
                packet.Dispose();
            }

            public void SendSpellCooldown(ref WS_PlayerData.CharacterObject objCharacter, [Optional, DefaultParameterValue(null)] ref ItemObject castItem)
            {
                if (!objCharacter.Spells.ContainsKey(ID))
                    return; // This is a trigger spell or something, exit
                int Recovery = SpellCooldown;
                int CatRecovery = CategoryCooldown;

                // DONE: Throw spell uses the equipped ranged item's attackspeed
                if (ID == 2764)
                    Recovery = (int)objCharacter.AttackTime(WeaponAttackType.RANGED_ATTACK);

                // DONE: Shoot spell uses the equipped wand's attackspeed
                if (ID == 5019)
                {
                    if (objCharacter.Items.ContainsKey(EquipmentSlots.EQUIPMENT_SLOT_RANGED))
                    {
                        Recovery = objCharacter.Items(EquipmentSlots.EQUIPMENT_SLOT_RANGED).ItemInfo.Delay;
                    }
                }

                // DONE: Get the recovery times from the item if the spell misses them
                if (CatRecovery == 0 && Recovery == 0 && castItem is object)
                {
                    for (int i = 0; i <= 4; i++)
                    {
                        if (castItem.ItemInfo.Spells[i].SpellID == ID)
                        {
                            Recovery = castItem.ItemInfo.Spells[i].SpellCooldown;
                            CatRecovery = castItem.ItemInfo.Spells[i].SpellCategoryCooldown;
                            break;
                        }
                    }
                }

                if (CatRecovery == 0 && Recovery == 0)
                    return; // There is no cooldown
                objCharacter.Spells[ID].Cooldown = (uint)(WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now) + Recovery / 1000);
                if (castItem is object)
                    objCharacter.Spells[ID].CooldownItem = castItem.ItemEntry;

                // DONE: Save the cooldown, but only if it's really worth saving
                if (Recovery > 10000)
                {
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_spells SET cooldown={2}, cooldownitem={3} WHERE guid = {0} AND spellid = {1};", objCharacter.GUID, ID, objCharacter.Spells[ID].Cooldown, objCharacter.Spells[ID].CooldownItem));
                }

                // DONE: Send the cooldown
                var packet = new Packets.PacketClass(OPCODES.SMSG_SPELL_COOLDOWN);
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

                // DONE: Send item cooldown
                if (castItem is object)
                {
                    packet = new Packets.PacketClass(OPCODES.SMSG_ITEM_COOLDOWN);
                    packet.AddUInt64(castItem.GUID);
                    packet.AddInt32(ID);
                    objCharacter.client.Send(ref packet);
                    packet.Dispose();
                }
            }
        }

        public class SpellEffect
        {
            public SpellEffects_Names ID = SpellEffects_Names.SPELL_EFFECT_NOTHING;
            public SpellInfo Spell;
            public int diceSides = 0;
            public int diceBase = 0;
            public float dicePerLevel = 0f;
            public int valueBase = 0;
            public int valueDie = 0;
            public int valuePerLevel = 0;
            public int valuePerComboPoint = 0;
            public int Mechanic = 0;
            public int implicitTargetA = 0;
            public int implicitTargetB = 0;
            public int RadiusIndex = 0;
            public int ApplyAuraIndex = 0;
            public int Amplitude = 0;
            public int MultipleValue = 0;
            public int ChainTarget = 0;
            public int ItemType = 0;
            public int MiscValue = 0;
            public int TriggerSpell = 0;
            public float DamageMultiplier = 1f;

            public SpellEffect(ref SpellInfo Spell)
            {
                this.Spell = Spell;
            }

            public float GetRadius
            {
                get
                {
                    if (WorldServiceLocator._WS_Spells.SpellRadius.ContainsKey(RadiusIndex))
                        return WorldServiceLocator._WS_Spells.SpellRadius[RadiusIndex];
                    return 0f;
                }
            }

            public int get_GetValue(int Level, int ComboPoints)
            {
                try
                {
                    return valueBase + Level * valuePerLevel + ComboPoints * valuePerComboPoint + WorldServiceLocator._WorldServer.Rnd.Next(1, (int)(valueDie + Level * dicePerLevel));
                }
                catch
                {
                    return valueBase + Level * valuePerLevel + ComboPoints * valuePerComboPoint + 1;
                }
            }

            public bool IsNegative
            {
                get
                {
                    if (ID != SpellEffects_Names.SPELL_EFFECT_APPLY_AURA)
                        return false;
                    switch (ApplyAuraIndex)
                    {
                        case var @case when @case == AuraEffects_Names.SPELL_AURA_GHOST:
                        case var case1 when case1 == AuraEffects_Names.SPELL_AURA_MOD_CONFUSE:
                        case var case2 when case2 == AuraEffects_Names.SPELL_AURA_MOD_DECREASE_SPEED:
                        case var case3 when case3 == AuraEffects_Names.SPELL_AURA_MOD_FEAR:
                            {
                                return true;
                            }

                        case var case4 when case4 == AuraEffects_Names.SPELL_AURA_MOD_PACIFY:
                        case var case5 when case5 == AuraEffects_Names.SPELL_AURA_MOD_PACIFY_SILENCE:
                        case var case6 when case6 == AuraEffects_Names.SPELL_AURA_MOD_POSSESS:
                        case var case7 when case7 == AuraEffects_Names.SPELL_AURA_PERIODIC_DAMAGE:
                            {
                                return true;
                            }

                        case var case8 when case8 == AuraEffects_Names.SPELL_AURA_MOD_POSSESS_PET:
                        case var case9 when case9 == AuraEffects_Names.SPELL_AURA_MOD_ROOT:
                        case var case10 when case10 == AuraEffects_Names.SPELL_AURA_MOD_SILENCE:
                        case var case11 when case11 == AuraEffects_Names.SPELL_AURA_MOD_STUN:
                            {
                                return true;
                            }

                        case var case12 when case12 == AuraEffects_Names.SPELL_AURA_PERIODIC_DAMAGE_PERCENT:
                        case var case13 when case13 == AuraEffects_Names.SPELL_AURA_PERIODIC_LEECH:
                        case var case14 when case14 == AuraEffects_Names.SPELL_AURA_PERIODIC_MANA_LEECH:
                        case var case15 when case15 == AuraEffects_Names.SPELL_AURA_PROC_TRIGGER_DAMAGE:
                            {
                                return true;
                            }

                        case var case16 when case16 == AuraEffects_Names.SPELL_AURA_TRANSFORM:
                        case var case17 when case17 == AuraEffects_Names.SPELL_AURA_SPLIT_DAMAGE_FLAT:
                        case var case18 when case18 == AuraEffects_Names.SPELL_AURA_SPLIT_DAMAGE_PCT:
                        case var case19 when case19 == AuraEffects_Names.SPELL_AURA_POWER_BURN_MANA:
                            {
                                return true;
                            }

                        case var case20 when case20 == AuraEffects_Names.SPELL_AURA_MOD_DAMAGE_DONE:
                        case var case21 when case21 == AuraEffects_Names.SPELL_AURA_MOD_STAT:
                        case var case22 when case22 == AuraEffects_Names.SPELL_AURA_MOD_PERCENT_STAT:
                        case var case23 when case23 == AuraEffects_Names.SPELL_AURA_MOD_TOTAL_STAT_PERCENTAGE:
                            {
                                if (valueBase < 0)
                                    return true;
                                else
                                    return false;
                                break;
                            }

                        default:
                            {
                                return false;
                            }
                    }
                }
            }

            public bool IsAOE
            {
                get
                {
                    for (int i = 0; i <= 1; i++)
                    {
                        SpellImplicitTargets targets;
                        if (i == 0)
                        {
                            targets = implicitTargetA;
                        }
                        else
                        {
                            targets = implicitTargetB;
                        }

                        switch (targets)
                        {
                            case var @case when @case == SpellImplicitTargets.TARGET_ALL_ENEMY_IN_AREA:
                            case var case1 when case1 == SpellImplicitTargets.TARGET_ALL_ENEMY_IN_AREA_INSTANT:
                            case var case2 when case2 == SpellImplicitTargets.TARGET_ALL_FRIENDLY_UNITS_AROUND_CASTER:
                            case var case3 when case3 == SpellImplicitTargets.TARGET_ALL_FRIENDLY_UNITS_IN_AREA:
                            case var case4 when case4 == SpellImplicitTargets.TARGET_ALL_PARTY:
                            case var case5 when case5 == SpellImplicitTargets.TARGET_ALL_PARTY_AROUND_CASTER_2:
                            case var case6 when case6 == SpellImplicitTargets.TARGET_AREA_EFFECT_ENEMY_CHANNEL:
                            case var case7 when case7 == SpellImplicitTargets.TARGET_AREA_EFFECT_SELECTED:
                            case var case8 when case8 == SpellImplicitTargets.TARGET_AREAEFFECT_CUSTOM:
                            case var case9 when case9 == SpellImplicitTargets.TARGET_AREAEFFECT_PARTY:
                            case var case10 when case10 == SpellImplicitTargets.TARGET_AREAEFFECT_PARTY_AND_CLASS:
                            case var case11 when case11 == SpellImplicitTargets.TARGET_AROUND_CASTER_ENEMY:
                            case var case12 when case12 == SpellImplicitTargets.TARGET_AROUND_CASTER_PARTY:
                            case var case13 when case13 == SpellImplicitTargets.TARGET_INFRONT:
                            case var case14 when case14 == SpellImplicitTargets.TARGET_TABLE_X_Y_Z_COORDINATES:
                            case var case15 when case15 == SpellImplicitTargets.TARGET_BEHIND_VICTIM:
                                {
                                    return true;
                                }
                        }
                    }

                    return false;
                }
            }
        }

        public class SpellTargets
        {
            public WS_Base.BaseUnit unitTarget = null;
            public WS_Base.BaseObject goTarget = null;
            public WS_Corpses.CorpseObject corpseTarget = null;
            public ItemObject itemTarget = null;
            public float srcX = 0f;
            public float srcY = 0f;
            public float srcZ = 0f;
            public float dstX = 0f;
            public float dstY = 0f;
            public float dstZ = 0f;
            public string stringTarget = "";
            public int targetMask = 0;

            public void ReadTargets(ref Packets.PacketClass packet, ref WS_Base.BaseObject Caster)
            {
                targetMask = packet.GetInt16();
                if (targetMask == SpellCastTargetFlags.TARGET_FLAG_SELF)
                {
                    unitTarget = (WS_Base.BaseUnit)Caster;
                    return;
                }

                if (targetMask & SpellCastTargetFlags.TARGET_FLAG_UNIT)
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

                if (targetMask & SpellCastTargetFlags.TARGET_FLAG_OBJECT)
                {
                    ulong GUID = packet.GetPackGuid();
                    if (WorldServiceLocator._CommonGlobalFunctions.GuidIsGameObject(GUID))
                    {
                        goTarget = WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID];
                    }
                    else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsDnyamicObject(GUID))
                    {
                        goTarget = WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs[GUID];
                    }
                }

                if (targetMask & SpellCastTargetFlags.TARGET_FLAG_ITEM || targetMask & SpellCastTargetFlags.TARGET_FLAG_TRADE_ITEM)
                {
                    ulong GUID = packet.GetPackGuid();
                    if (WorldServiceLocator._CommonGlobalFunctions.GuidIsItem(GUID))
                    {
                        itemTarget = WorldServiceLocator._WorldServer.WORLD_ITEMs[GUID];
                    }
                }

                if (targetMask & SpellCastTargetFlags.TARGET_FLAG_SOURCE_LOCATION)
                {
                    srcX = packet.GetFloat();
                    srcY = packet.GetFloat();
                    srcZ = packet.GetFloat();
                }

                if (targetMask & SpellCastTargetFlags.TARGET_FLAG_DEST_LOCATION)
                {
                    dstX = packet.GetFloat();
                    dstY = packet.GetFloat();
                    dstZ = packet.GetFloat();
                }

                if (targetMask & SpellCastTargetFlags.TARGET_FLAG_STRING)
                    stringTarget = packet.GetString();
                if (targetMask & SpellCastTargetFlags.TARGET_FLAG_CORPSE || targetMask & SpellCastTargetFlags.TARGET_FLAG_PVP_CORPSE)
                {
                    ulong GUID = packet.GetPackGuid();
                    if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCorpse(GUID))
                    {
                        corpseTarget = WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs[GUID];
                    }
                }
            }

            public void WriteTargets(ref Packets.PacketClass packet)
            {
                packet.AddInt16((short)targetMask);
                if (targetMask & SpellCastTargetFlags.TARGET_FLAG_UNIT)
                    packet.AddPackGUID(unitTarget.GUID);
                if (targetMask & SpellCastTargetFlags.TARGET_FLAG_OBJECT)
                    packet.AddPackGUID(goTarget.GUID);
                if (targetMask & SpellCastTargetFlags.TARGET_FLAG_ITEM || targetMask & SpellCastTargetFlags.TARGET_FLAG_TRADE_ITEM)
                    packet.AddPackGUID(itemTarget.GUID);
                if (targetMask & SpellCastTargetFlags.TARGET_FLAG_SOURCE_LOCATION)
                {
                    packet.AddSingle(srcX);
                    packet.AddSingle(srcY);
                    packet.AddSingle(srcZ);
                }

                if (targetMask & SpellCastTargetFlags.TARGET_FLAG_DEST_LOCATION)
                {
                    packet.AddSingle(dstX);
                    packet.AddSingle(dstY);
                    packet.AddSingle(dstZ);
                }

                if (targetMask & SpellCastTargetFlags.TARGET_FLAG_STRING)
                    packet.AddString(stringTarget);
                if (targetMask & SpellCastTargetFlags.TARGET_FLAG_CORPSE || targetMask & SpellCastTargetFlags.TARGET_FLAG_PVP_CORPSE)
                {
                    packet.AddPackGUID(corpseTarget.GUID);
                }
            }

            public void SetTarget_SELF(ref WS_Base.BaseUnit objCharacter)
            {
                unitTarget = objCharacter;
                targetMask += SpellCastTargetFlags.TARGET_FLAG_SELF;
            }

            public void SetTarget_UNIT(ref WS_Base.BaseUnit objCharacter)
            {
                unitTarget = objCharacter;
                targetMask += SpellCastTargetFlags.TARGET_FLAG_UNIT;
            }

            public void SetTarget_OBJECT(ref WS_Base.BaseObject o)
            {
                goTarget = o;
                targetMask += SpellCastTargetFlags.TARGET_FLAG_OBJECT;
            }

            public void SetTarget_ITEM(ref ItemObject i)
            {
                itemTarget = i;
                targetMask += SpellCastTargetFlags.TARGET_FLAG_ITEM;
            }

            public void SetTarget_SOURCELOCATION(float x, float y, float z)
            {
                srcX = x;
                srcY = y;
                srcZ = z;
                targetMask += SpellCastTargetFlags.TARGET_FLAG_SOURCE_LOCATION;
            }

            public void SetTarget_DESTINATIONLOCATION(float x, float y, float z)
            {
                dstX = x;
                dstY = y;
                dstZ = z;
                targetMask += SpellCastTargetFlags.TARGET_FLAG_DEST_LOCATION;
            }

            public void SetTarget_STRING(string str)
            {
                stringTarget = str;
                targetMask += SpellCastTargetFlags.TARGET_FLAG_STRING;
            }
        }

        public class CastSpellParameters : IDisposable
        {
            public SpellTargets Targets;
            public WS_Base.BaseObject Caster;
            public int SpellID;
            public ItemObject Item = null;
            public bool InstantCast = false;
            public int Delayed = 0;
            public bool Stopped = false;
            public SpellCastState State = SpellCastState.SPELL_STATE_IDLE;

            public CastSpellParameters(ref SpellTargets Targets, ref WS_Base.BaseObject Caster, int SpellID)
            {
                this.Targets = Targets;
                this.Caster = Caster;
                this.SpellID = SpellID;
                Item = null;
                InstantCast = false;
            }

            public CastSpellParameters(ref SpellTargets Targets, ref WS_Base.BaseObject Caster, int SpellID, bool Instant)
            {
                this.Targets = Targets;
                this.Caster = Caster;
                this.SpellID = SpellID;
                Item = null;
                InstantCast = Instant;
            }

            public CastSpellParameters(ref SpellTargets Targets, ref WS_Base.BaseObject Caster, int SpellID, ref ItemObject Item)
            {
                this.Targets = Targets;
                this.Caster = Caster;
                this.SpellID = SpellID;
                this.Item = Item;
                InstantCast = false;
            }

            public CastSpellParameters(ref SpellTargets Targets, ref WS_Base.BaseObject Caster, int SpellID, ref ItemObject Item, bool Instant)
            {
                this.Targets = Targets;
                this.Caster = Caster;
                this.SpellID = SpellID;
                this.Item = Item;
                InstantCast = Instant;
            }

            public SpellInfo SpellInfo
            {
                get
                {
                    return WorldServiceLocator._WS_Spells.SPELLs[SpellID];
                }
            }

            public bool Finished
            {
                get
                {
                    return Stopped || State == SpellCastState.SPELL_STATE_FINISHED || State == SpellCastState.SPELL_STATE_IDLE;
                }
            }

            public void Cast(object status)
            {
                try
                {
                    Stopped = false;
                    var argcastParams = this;
                    WorldServiceLocator._WS_Spells.SPELLs[SpellID].Cast(ref argcastParams);
                }
                catch (Exception)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Cast Exception {0} : Interrupted {1}", SpellID, Stopped);
                }
            }

            public void StopCast()
            {
                try
                {
                    Stopped = true;
                }
                catch (Exception)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "StopCast Exception {0} : Interrupted {1}", SpellID, Stopped);
                }
            }

            public void Delay()
            {
                if (Caster is null || Finished)
                    return;

                // Calculate resist chance
                int resistChance = ((WS_Base.BaseUnit)Caster).GetAuraModifier(AuraEffects_Names.SPELL_AURA_RESIST_PUSHBACK);
                if (resistChance > 0 && resistChance > WorldServiceLocator._WorldServer.Rnd.Next(0, 100))
                    return; // Resisted pushback

                // TODO: Calculate delay time?
                int delaytime = 200;
                Delayed += delaytime;
                var packet = new Packets.PacketClass(OPCODES.SMSG_SPELL_DELAYED);
                packet.AddPackGUID(Caster.GUID);
                packet.AddInt32(delaytime);
                Caster.SendToNearPlayers(ref packet);
                packet.Dispose();
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

        public void SendCastResult(SpellFailedReason result, ref WS_Network.ClientClass client, int id)
        {
            var packet = new Packets.PacketClass(OPCODES.SMSG_CAST_RESULT);
            packet.AddInt32(id);
            if (result != SpellFailedReason.SPELL_NO_ERROR)
            {
                packet.AddInt8(2);
                packet.AddInt8(result);
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
            var packet = new Packets.PacketClass(OPCODES.SMSG_SPELLNONMELEEDAMAGELOG);
            packet.AddPackGUID(Target.GUID);
            packet.AddPackGUID(Caster.GUID);
            packet.AddInt32(SpellID);
            packet.AddInt32(Damage);
            packet.AddInt8((byte)SchoolType);
            packet.AddInt32(Absorbed);       // AbsorbedDamage
            packet.AddInt32(Resist);         // Resist
            packet.AddInt8(0);               // 1=Physical/0=Not Physical
            packet.AddInt8(0);               // Unk
            packet.AddInt32(0);              // Blocked
            if (CriticalHit)
            {
                packet.AddInt8(0x2);
            }
            else
            {
                packet.AddInt8(0x0);
            }

            packet.AddInt32(0);               // Unk
            Caster.SendToNearPlayers(ref packet);
        }

        public void SendHealSpellLog(ref WS_Base.BaseUnit Caster, ref WS_Base.BaseUnit Target, int SpellID, int Damage, bool CriticalHit)
        {
            var packet = new Packets.PacketClass(OPCODES.SMSG_HEALSPELL_ON_PLAYER_OBSOLETE);
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

            Caster.SendToNearPlayers(ref packet);
        }

        public void SendEnergizeSpellLog(ref WS_Base.BaseUnit Caster, ref WS_Base.BaseUnit Target, int SpellID, int Damage, int PowerType)
        {
            // Dim packet As New PacketClass(OPCODES.SMSG_SPELLENERGIZELOG)
            // packet.AddPackGUID(Target.GUID)
            // packet.AddPackGUID(Caster.GUID)
            // packet.AddInt32(SpellID)
            // packet.AddInt32(PowerType)
            // packet.AddInt32(Damage)
            // Caster.SendToNearPlayers(packet)
            // packet.Dispose()
        }

        public void SendPeriodicAuraLog(ref WS_Base.BaseUnit Caster, ref WS_Base.BaseUnit Target, int SpellID, int School, int Damage, int AuraIndex)
        {
            var packet = new Packets.PacketClass(OPCODES.SMSG_PERIODICAURALOG);
            packet.AddPackGUID(Target.GUID);
            packet.AddPackGUID(Caster.GUID);
            packet.AddInt32(SpellID);
            packet.AddInt32(1);
            packet.AddInt8((byte)AuraIndex);
            packet.AddInt32(Damage);
            packet.AddInt32(School);
            packet.AddInt32(0);
            Caster.SendToNearPlayers(ref packet);
            packet.Dispose();
        }

        public void SendPlaySpellVisual(ref WS_Base.BaseUnit Caster, int SpellId)
        {
            var packet = new Packets.PacketClass(OPCODES.SMSG_PLAY_SPELL_VISUAL);
            packet.AddUInt64(Caster.GUID);
            packet.AddInt32(SpellId);
            Caster.SendToNearPlayers(ref packet);
            packet.Dispose();
        }

        public void SendChannelUpdate(ref WS_PlayerData.CharacterObject Caster, int Time)
        {
            // DONE: Update time for self
            var packet = new Packets.PacketClass(OPCODES.MSG_CHANNEL_UPDATE);
            packet.AddInt32(Time);
            Caster.client.Send(ref packet);
            packet.Dispose();
            if (Time == 0)
            {
                // DONE: Stop channeling for others
                Caster.SetUpdateFlag(EUnitFields.UNIT_FIELD_CHANNEL_OBJECT, 0L);
                Caster.SetUpdateFlag(EUnitFields.UNIT_CHANNEL_SPELL, 0);
                Caster.SendCharacterUpdate();
            }
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public Dictionary<int, int> SpellChains = new Dictionary<int, int>();
        public Dictionary<int, SpellInfo> SPELLs = new Dictionary<int, SpellInfo>(29000);
        public Dictionary<int, int> SpellCastTime = new Dictionary<int, int>();
        public Dictionary<int, float> SpellRadius = new Dictionary<int, float>();
        public Dictionary<int, float> SpellRange = new Dictionary<int, float>();
        public Dictionary<int, int> SpellDuration = new Dictionary<int, int>();
        public Dictionary<int, string> SpellFocusObject = new Dictionary<int, string>();

        public void InitializeSpellDB()
        {
            for (int i = 0; i <= SPELL_EFFECT_COUNT; i++)
                SPELL_EFFECTs[i] = SPELL_EFFECT_NOTHING;
            SPELL_EFFECTs[0] = SPELL_EFFECT_NOTHING;                   // None
            SPELL_EFFECTs[1] = SPELL_EFFECT_INSTAKILL;                 // Instakill
            SPELL_EFFECTs[2] = SPELL_EFFECT_SCHOOL_DAMAGE;             // School Damage
            SPELL_EFFECTs[3] = SPELL_EFFECT_DUMMY;                     // Dummy
            // SPELL_EFFECTs(4) = AddressOf SPELL_EFFECT_PORTAL_TELEPORT           'Portal Teleport
            SPELL_EFFECTs[5] = SPELL_EFFECT_TELEPORT_UNITS;            // Teleport Units
            SPELL_EFFECTs[6] = SPELL_EFFECT_APPLY_AURA;                // Apply Aura
            SPELL_EFFECTs[7] = SPELL_EFFECT_ENVIRONMENTAL_DAMAGE;      // Environmental Damage
            SPELL_EFFECTs[8] = SPELL_EFFECT_MANA_DRAIN;                // Power Drain
            // SPELL_EFFECTs(9) = AddressOf SPELL_EFFECT_HEALTH_LEECH              'Health Leech
            SPELL_EFFECTs[10] = SPELL_EFFECT_HEAL;                     // Heal
            SPELL_EFFECTs[11] = SPELL_EFFECT_BIND;                     // Bind
            // SPELL_EFFECTs(12) = AddressOf SPELL_EFFECT_PORTAL                   'Portal
            // SPELL_EFFECTs(13) = AddressOf SPELL_EFFECT_RITUAL_BASE              'Ritual Base
            // SPELL_EFFECTs(14) = AddressOf SPELL_EFFECT_RITUAL_SPECIALIZE        'Ritual Specialize
            // SPELL_EFFECTs(15) = AddressOf SPELL_EFFECT_RITUAL_ACTIVATE_PORTAL   'Ritual Activate Portal
            SPELL_EFFECTs[16] = SPELL_EFFECT_QUEST_COMPLETE;           // Quest Complete
            SPELL_EFFECTs[17] = SPELL_EFFECT_WEAPON_DAMAGE_NOSCHOOL;   // Weapon Damage + (noschool)
            SPELL_EFFECTs[18] = SPELL_EFFECT_RESURRECT;                // Resurrect
            // !! SPELL_EFFECTs(19) = AddressOf SPELL_EFFECT_ADD_EXTRA_ATTACKS        'Extra Attacks
            SPELL_EFFECTs[20] = SPELL_EFFECT_DODGE;                    // Dodge
            SPELL_EFFECTs[21] = SPELL_EFFECT_EVADE;                    // Evade
            SPELL_EFFECTs[22] = SPELL_EFFECT_PARRY;                    // Parry
            SPELL_EFFECTs[23] = SPELL_EFFECT_BLOCK;                    // Block
            SPELL_EFFECTs[24] = SPELL_EFFECT_CREATE_ITEM;              // Create Item
            // SPELL_EFFECTs(25) = AddressOf SPELL_EFFECT_WEAPON                   'Weapon
            // SPELL_EFFECTs(26) = AddressOf SPELL_EFFECT_DEFENSE                  'Defense
            SPELL_EFFECTs[27] = SPELL_EFFECT_PERSISTENT_AREA_AURA;     // Persistent Area Aura
            SPELL_EFFECTs[28] = SPELL_EFFECT_SUMMON;                   // Summon
            SPELL_EFFECTs[29] = SPELL_EFFECT_LEAP;                     // Leap
            SPELL_EFFECTs[30] = SPELL_EFFECT_ENERGIZE;                 // Energize
            // SPELL_EFFECTs(31) = AddressOf SPELL_EFFECT_WEAPON_PERCENT_DAMAGE    'Weapon % Dmg
            // SPELL_EFFECTs(32) = AddressOf SPELL_EFFECT_TRIGGER_MISSILE          'Trigger Missile
            SPELL_EFFECTs[33] = SPELL_EFFECT_OPEN_LOCK;                // Open Lock
            // SPELL_EFFECTs(34) = AddressOf SPELL_EFFECT_SUMMON_MOUNT_OBSOLETE
            SPELL_EFFECTs[35] = SPELL_EFFECT_APPLY_AREA_AURA;          // Apply Area Aura
            SPELL_EFFECTs[36] = SPELL_EFFECT_LEARN_SPELL;              // Learn Spell
            // SPELL_EFFECTs(37) = AddressOf SPELL_EFFECT_SPELL_DEFENSE            'Spell Defense
            SPELL_EFFECTs[38] = SPELL_EFFECT_DISPEL;                   // Dispel
            // SPELL_EFFECTs(39) = AddressOf SPELL_EFFECT_LANGUAGE                 'Language
            SPELL_EFFECTs[40] = SPELL_EFFECT_DUAL_WIELD;               // Dual Wield
            SPELL_EFFECTs[41] = SPELL_EFFECT_SUMMON_WILD;          // Summon Wild
            SPELL_EFFECTs[42] = SPELL_EFFECT_SUMMON_WILD;             // Summon Guardian
            // ! SPELL_EFFECTs(43) = AddressOf SPELL_EFFECT_TELEPORT_UNITS_FACE_CASTER
            SPELL_EFFECTs[44] = SPELL_EFFECT_SKILL_STEP;               // Skill Step
            SPELL_EFFECTs[45] = SPELL_EFFECT_HONOR;
            // SPELL_EFFECTs(46) = AddressOf SPELL_EFFECT_SPAWN                    'Spawn
            // SPELL_EFFECTs(47) = AddressOf SPELL_EFFECT_TRADE_SKILL              'Spell Cast UI
            SPELL_EFFECTs[48] = SPELL_EFFECT_STEALTH;                  // Stealth
            SPELL_EFFECTs[49] = SPELL_EFFECT_DETECT;                   // Detect
            SPELL_EFFECTs[50] = SPELL_EFFECT_SUMMON_OBJECT;            // Summon Object
            // SPELL_EFFECTs(51) = AddressOf SPELL_EFFECT_FORCE_CRITICAL_HIT       'Force Critical Hit
            // SPELL_EFFECTs(52) = AddressOf SPELL_EFFECT_GUARANTEE_HIT            'Guarantee Hit
            SPELL_EFFECTs[53] = SPELL_EFFECT_ENCHANT_ITEM;             // Enchant Item Permanent
            SPELL_EFFECTs[54] = SPELL_EFFECT_ENCHANT_ITEM_TEMPORARY;   // Enchant Item Temporary
            // SPELL_EFFECTs(55) = AddressOf SPELL_EFFECT_TAMECREATURE             'Tame Creature
            // SPELL_EFFECTs(56) = AddressOf SPELL_EFFECT_SUMMON_PET               'Summon Pet
            // SPELL_EFFECTs(57) = AddressOf SPELL_EFFECT_LEARN_PET_SPELL          'Learn Pet Spell
            SPELL_EFFECTs[58] = SPELL_EFFECT_WEAPON_DAMAGE;            // Weapon Damage +
            SPELL_EFFECTs[59] = SPELL_EFFECT_OPEN_LOCK;                // Open Lock (Item)
            SPELL_EFFECTs[60] = SPELL_EFFECT_PROFICIENCY;              // Proficiency
            // SPELL_EFFECTs(61) = AddressOf SPELL_EFFECT_SEND_EVENT               'Send Event
            // SPELL_EFFECTs(62) = AddressOf SPELL_EFFECT_POWER_BURN               'Power Burn
            // SPELL_EFFECTs(63) = AddressOf SPELL_EFFECT_THREAT                   'Threat
            SPELL_EFFECTs[64] = SPELL_EFFECT_TRIGGER_SPELL;            // Trigger Spell
            // SPELL_EFFECTs(65) = AddressOf SPELL_EFFECT_HEALTH_FUNNEL            'Health Funnel
            // SPELL_EFFECTs(66) = AddressOf SPELL_EFFECT_POWER_FUNNEL             'Power Funnel
            SPELL_EFFECTs[67] = SPELL_EFFECT_HEAL_MAX_HEALTH;          // Heal Max Health
            SPELL_EFFECTs[68] = SPELL_EFFECT_INTERRUPT_CAST;           // Interrupt Cast
            // SPELL_EFFECTs(69) = AddressOf SPELL_EFFECT_DISTRACT                 'Distract
            // SPELL_EFFECTs(70) = AddressOf SPELL_EFFECT_PULL                     'Pull
            SPELL_EFFECTs[71] = SPELL_EFFECT_PICKPOCKET;               // Pickpocket
            // SPELL_EFFECTs(72) = AddressOf SPELL_EFFECT_ADD_FARSIGHT             'Add Farsight
            // SPELL_EFFECTs(73) = AddressOf SPELL_EFFECT_SUMMON_POSSESSED         'Summon Possessed
            SPELL_EFFECTs[74] = SPELL_EFFECT_SUMMON_TOTEM;             // Summon Totem
            // SPELL_EFFECTs(75) = AddressOf SPELL_EFFECT_HEAL_MECHANICAL          'Heal Mechanical
            // SPELL_EFFECTs(76) = AddressOf SPELL_EFFECT_SUMMON_OBJECT_WILD       'Summon Object (Wild)
            SPELL_EFFECTs[77] = SPELL_EFFECT_SCRIPT_EFFECT;            // Script Effect
            // SPELL_EFFECTs(78) = AddressOf SPELL_EFFECT_ATTACK                   'Attack
            // SPELL_EFFECTs(79) = AddressOf SPELL_EFFECT_SANCTUARY                'Sanctuary
            // SPELL_EFFECTs(80) = AddressOf SPELL_EFFECT_ADD_COMBO_POINTS         'Add Combo Points
            // SPELL_EFFECTs(81) = AddressOf SPELL_EFFECT_CREATE_HOUSE             'Create House
            // SPELL_EFFECTs(82) = AddressOf SPELL_EFFECT_BIND_SIGHT               'Bind Sight
            SPELL_EFFECTs[83] = SPELL_EFFECT_DUEL;                     // Duel
            // SPELL_EFFECTs(84) = AddressOf SPELL_EFFECT_STUCK                    'Stuck
            // SPELL_EFFECTs(85) = AddressOf SPELL_EFFECT_SUMMON_PLAYER            'Summon Player
            // SPELL_EFFECTs(86) = AddressOf SPELL_EFFECT_ACTIVATE_OBJECT          'Activate Object
            SPELL_EFFECTs[87] = SPELL_EFFECT_SUMMON_TOTEM;             // Summon Totem (slot 1)
            SPELL_EFFECTs[88] = SPELL_EFFECT_SUMMON_TOTEM;             // Summon Totem (slot 2)
            SPELL_EFFECTs[89] = SPELL_EFFECT_SUMMON_TOTEM;             // Summon Totem (slot 3)
            SPELL_EFFECTs[90] = SPELL_EFFECT_SUMMON_TOTEM;             // Summon Totem (slot 4)
            // SPELL_EFFECTs(91) = AddressOf SPELL_EFFECT_THREAT_ALL               'Threat (All)
            SPELL_EFFECTs[92] = SPELL_EFFECT_ENCHANT_HELD_ITEM;        // Enchant Held Item
            // SPELL_EFFECTs(93) = AddressOf SPELL_EFFECT_SUMMON_PHANTASM          'Summon Phantasm
            // SPELL_EFFECTs(94) = AddressOf SPELL_EFFECT_SELF_RESURRECT           'Self Resurrect
            SPELL_EFFECTs[95] = SPELL_EFFECT_SKINNING;                 // Skinning
            SPELL_EFFECTs[96] = SPELL_EFFECT_CHARGE;                   // Charge
            // SPELL_EFFECTs(97) = AddressOf SPELL_EFFECT_SUMMON_CRITTER           'Summon Critter
            SPELL_EFFECTs[98] = SPELL_EFFECT_KNOCK_BACK;               // Knock Back
            SPELL_EFFECTs[99] = SPELL_EFFECT_DISENCHANT;               // Disenchant
            // SPELL_EFFECTs(100) = AddressOf SPELL_EFFECT_INEBRIATE               'Inebriate
            // SPELL_EFFECTs(101) = AddressOf SPELL_EFFECT_FEED_PET                'Feed Pet
            // SPELL_EFFECTs(102) = AddressOf SPELL_EFFECT_DISMISS_PET             'Dismiss Pet
            // SPELL_EFFECTs(103) = AddressOf SPELL_EFFECT_REPUTATION              'Reputation
            // SPELL_EFFECTs(104) = AddressOf SPELL_EFFECT_SUMMON_OBJECT_SLOT1     'Summon Object (slot 1)
            // SPELL_EFFECTs(105) = AddressOf SPELL_EFFECT_SUMMON_OBJECT_SLOT2     'Summon Object (slot 2)
            // SPELL_EFFECTs(106) = AddressOf SPELL_EFFECT_SUMMON_OBJECT_SLOT3     'Summon Object (slot 3)
            // SPELL_EFFECTs(107) = AddressOf SPELL_EFFECT_SUMMON_OBJECT_SLOT4     'Summon Object (slot 4)
            // SPELL_EFFECTs(108) = AddressOf SPELL_EFFECT_DISPEL_MECHANIC         'Dispel Mechanic
            // SPELL_EFFECTs(109) = AddressOf SPELL_EFFECT_SUMMON_DEAD_PET         'Summon Dead Pet
            // SPELL_EFFECTs(110) = AddressOf SPELL_EFFECT_DESTROY_ALL_TOTEMS      'Destroy All Totems
            // SPELL_EFFECTs(111) = AddressOf SPELL_EFFECT_DURABILITY_DAMAGE       'Durability Damage
            // SPELL_EFFECTs(112) = AddressOf SPELL_EFFECT_SUMMON_DEMON            'Summon Demon
            SPELL_EFFECTs[113] = SPELL_EFFECT_RESURRECT_NEW;           // Resurrect (Flat)
            // SPELL_EFFECTs(114) = AddressOf SPELL_EFFECT_ATTACK_ME               'Attack Me
            // SPELL_EFFECTs(115) = AddressOf SPELL_EFFECT_DURABILITY_DAMAGE_PCT
            // SPELL_EFFECTs(116) = AddressOf SPELL_EFFECT_SKIN_PLAYER_CORPSE
            // SPELL_EFFECTs(117) = AddressOf SPELL_EFFECT_SPIRIT_HEAL
            // SPELL_EFFECTs(118) = AddressOf SPELL_EFFECT_SKILL
            // SPELL_EFFECTs(119) = AddressOf SPELL_EFFECT_APPLY_AURA_NEW
            SPELL_EFFECTs[120] = SPELL_EFFECT_TELEPORT_GRAVEYARD;
            SPELL_EFFECTs[121] = SPELL_EFFECT_ADICIONAL_DMG;
            // SPELL_EFFECTs(122) = AddressOf SPELL_EFFECT_?
            // SPELL_EFFECTs(123) = AddressOf SPELL_EFFECT_TAXI                   'Taxi Flight
            // SPELL_EFFECTs(124) = AddressOf SPELL_EFFECT_PULL_TOWARD            'Pull target towards you
            // SPELL_EFFECTs(125) = AddressOf SPELL_EFFECT_INVISIBILITY_NEW       '
            // SPELL_EFFECTs(126) = AddressOf SPELL_EFFECT_SPELL_STEAL            'Steal benefical effect
            // SPELL_EFFECTs(127) = AddressOf SPELL_EFFECT_PROSPECT               'Search ore for gems
            // SPELL_EFFECTs(128) = AddressOf SPELL_EFFECT_APPLY_AURA_NEW2
            // SPELL_EFFECTs(129) = AddressOf SPELL_EFFECT_APPLY_AURA_NEW3
            // SPELL_EFFECTs(130) = AddressOf SPELL_EFFECT_REDIRECT_THREAT
            // SPELL_EFFECTs(131) = AddressOf SPELL_EFFECT_?
            // SPELL_EFFECTs(132) = AddressOf SPELL_EFFECT_?
            // SPELL_EFFECTs(133) = AddressOf SPELL_EFFECT_FORGET
            // SPELL_EFFECTs(134) = AddressOf SPELL_EFFECT_KILL_CREDIT
            // SPELL_EFFECTs(135) = AddressOf SPELL_EFFECT_SUMMON_PET_NEW
            // SPELL_EFFECTs(136) = AddressOf SPELL_EFFECT_HEAL_PCT
            SPELL_EFFECTs[137] = SPELL_EFFECT_ENERGIZE_PCT;
            for (int i = 0; i <= AURAs_COUNT; i++)
                AURAs[i] = SPELL_AURA_NONE;
            AURAs[0] = SPELL_AURA_NONE;                                            // None
            AURAs[1] = SPELL_AURA_BIND_SIGHT;                                      // Bind Sight
            AURAs[2] = SPELL_AURA_MOD_POSSESS;                                     // Mod Possess
            AURAs[3] = SPELL_AURA_PERIODIC_DAMAGE;                                 // Periodic Damage
            AURAs[4] = SPELL_AURA_DUMMY;                                           // Dummy
            // AURAs(	5	) = AddressOf 	SPELL_AURA_MOD_CONFUSE				                'Mod Confuse
            // AURAs(	6	) = AddressOf 	SPELL_AURA_MOD_CHARM				                'Mod Charm
            AURAs[7] = SPELL_AURA_MOD_FEAR;                                        // Mod Fear
            AURAs[8] = SPELL_AURA_PERIODIC_HEAL;                                   // Periodic Heal
            // AURAs(	9	) = AddressOf 	SPELL_AURA_MOD_ATTACKSPEED			                'Mod Attack Speed
            AURAs[10] = SPELL_AURA_MOD_THREAT;                                     // Mod Threat
            AURAs[11] = SPELL_AURA_MOD_TAUNT;                                      // Taunt
            AURAs[12] = SPELL_AURA_MOD_STUN;                                       // Stun
            AURAs[13] = SPELL_AURA_MOD_DAMAGE_DONE;                                // Mod Damage Done
            // AURAs(14) = AddressOf SPELL_AURA_MOD_DAMAGE_TAKEN                              'Mod Damage Taken
            // AURAs(	15	) = AddressOf 	SPELL_AURA_DAMAGE_SHIELD			                'Damage Shield
            AURAs[16] = SPELL_AURA_MOD_STEALTH;                                    // Mod Stealth
            AURAs[17] = SPELL_AURA_MOD_DETECT;                                     // Mod Detect
            AURAs[18] = SPELL_AURA_MOD_INVISIBILITY;                               // Mod Invisibility
            AURAs[19] = SPELL_AURA_MOD_INVISIBILITY_DETECTION;                     // Mod Invisibility Detection
            AURAs[20] = SPELL_AURA_PERIODIC_HEAL_PERCENT;                          // Mod Health Regeneration %
            AURAs[21] = SPELL_AURA_PERIODIC_ENERGIZE_PERCENT;                      // Mod Mana Regeneration %
            AURAs[22] = SPELL_AURA_MOD_RESISTANCE;                                 // Mod Resistance
            AURAs[23] = SPELL_AURA_PERIODIC_TRIGGER_SPELL;                         // Periodic Trigger
            AURAs[24] = SPELL_AURA_PERIODIC_ENERGIZE;                              // Periodic Energize
            AURAs[25] = SPELL_AURA_MOD_PACIFY;                                     // Pacify
            AURAs[26] = SPELL_AURA_MOD_ROOT;                                       // Root
            AURAs[27] = SPELL_AURA_MOD_SILENCE;                                    // Silence
            // AURAs(	28	) = AddressOf 	SPELL_AURA_REFLECT_SPELLS			                'Reflect Spells %
            AURAs[29] = SPELL_AURA_MOD_STAT;                                       // Mod Stat
            AURAs[30] = SPELL_AURA_MOD_SKILL;                                      // Mod Skill
            AURAs[31] = SPELL_AURA_MOD_INCREASE_SPEED;                             // Mod Speed
            AURAs[32] = SPELL_AURA_MOD_INCREASE_MOUNTED_SPEED;                     // Mod Speed Mounted
            AURAs[33] = SPELL_AURA_MOD_DECREASE_SPEED;                             // Mod Speed Slow
            AURAs[34] = SPELL_AURA_MOD_INCREASE_HEALTH;                            // Mod Increase Health
            AURAs[35] = SPELL_AURA_MOD_INCREASE_ENERGY;                            // Mod Increase Energy
            AURAs[36] = SPELL_AURA_MOD_SHAPESHIFT;                                 // Shapeshift
            // AURAs(	37	) = AddressOf 	SPELL_AURA_EFFECT_IMMUNITY			                'Immune Effect
            // AURAs(	38	) = AddressOf 	SPELL_AURA_STATE_IMMUNITY			                'Immune State
            AURAs[39] = SPELL_AURA_SCHOOL_IMMUNITY;                                // Immune School
            // AURAs(	40	) = AddressOf 	SPELL_AURA_DAMAGE_IMMUNITY			                'Immune Damage
            AURAs[41] = SPELL_AURA_DISPEL_IMMUNITY;                                // Immune Dispel Type
            AURAs[42] = SPELL_AURA_PROC_TRIGGER_SPELL;                             // Proc Trigger Spell
            // AURAs(	43	) = AddressOf 	SPELL_AURA_PROC_TRIGGER_DAMAGE		                'Proc Trigger Damage
            AURAs[44] = SPELL_AURA_TRACK_CREATURES;                                // Track Creatures
            AURAs[45] = SPELL_AURA_TRACK_RESOURCES;                                // Track Resources
            // AURAs(	46	) = AddressOf 	SPELL_AURA_MOD_PARRY_SKILL			                'Mod Parry Skill
            // AURAs(	47	) = AddressOf 	SPELL_AURA_MOD_PARRY_PERCENT		                'Mod Parry Percent
            // AURAs(	48	) = AddressOf 	SPELL_AURA_MOD_DODGE_SKILL			                'Mod Dodge Skill
            // AURAs(	49	) = AddressOf 	SPELL_AURA_MOD_DODGE_PERCENT		                'Mod Dodge Percent
            // AURAs(	50	) = AddressOf 	SPELL_AURA_MOD_BLOCK_SKILL			                'Mod Block Skill
            // AURAs(	51	) = AddressOf 	SPELL_AURA_MOD_BLOCK_PERCENT		                'Mod Block Percent
            // AURAs(	52	) = AddressOf 	SPELL_AURA_MOD_CRIT_PERCENT			                'Mod Crit Percent
            AURAs[53] = SPELL_AURA_PERIODIC_LEECH;                                 // Periodic Leech
            // AURAs(	54	) = AddressOf 	SPELL_AURA_MOD_HIT_CHANCE			                'Mod Hit Chance
            // AURAs(	55	) = AddressOf 	SPELL_AURA_MOD_SPELL_HIT_CHANCE		                'Mod Spell Hit Chance
            AURAs[56] = SPELL_AURA_TRANSFORM;                                      // Transform
            // AURAs(	57	) = AddressOf 	SPELL_AURA_MOD_SPELL_CRIT_CHANCE	                'Mod Spell Crit Chance
            AURAs[58] = SPELL_AURA_MOD_INCREASE_SWIM_SPEED;                        // Mod Speed Swim
            // AURAs(	59	) = AddressOf 	SPELL_AURA_MOD_DAMAGE_DONE_CREATURE	                'Mod Creature Dmg Done
            // AURAs(	60	) = AddressOf 	SPELL_AURA_MOD_PACIFY_SILENCE		                'Pacify & Silence
            AURAs[61] = SPELL_AURA_MOD_SCALE;                                      // Mod Scale
            // AURAs(	62	) = AddressOf 	SPELL_AURA_PERIODIC_HEALTH_FUNNEL	                'Periodic Health Funnel
            // AURAs(	63	) = AddressOf 	SPELL_AURA_PERIODIC_MANA_FUNNEL		                'Periodic Mana Funnel
            AURAs[64] = SPELL_AURA_PERIODIC_MANA_LEECH;                            // Periodic Mana Leech
            // AURAs(	65	) = AddressOf 	SPELL_AURA_MOD_CASTING_SPEED		                'Haste - Spells
            // AURAs(	66	) = AddressOf 	SPELL_AURA_FEIGN_DEATH				                'Feign Death
            AURAs[67] = SPELL_AURA_MOD_DISARM;                                     // Disarm
            // AURAs(	68	) = AddressOf 	SPELL_AURA_MOD_STALKED				                'Mod Stalked
            AURAs[69] = SPELL_AURA_SCHOOL_ABSORB;                                  // School Absorb
            // AURAs(	70	) = AddressOf 	SPELL_AURA_EXTRA_ATTACKS			                'Extra Attacks
            // AURAs(	71	) = AddressOf 	SPELL_AURA_MOD_SPELL_CRIT_CHANCE_SCHOOL				'Mod School Spell Crit Chance
            // AURAs(	72	) = AddressOf 	SPELL_AURA_MOD_POWER_COST			                'Mod Power Cost
            // AURAs(	73	) = AddressOf 	SPELL_AURA_MOD_POWER_COST_SCHOOL	                'Mod School Power Cost
            // AURAs(	74	) = AddressOf 	SPELL_AURA_REFLECT_SPELLS_SCHOOL	                'Reflect School Spells %
            AURAs[75] = SPELL_AURA_MOD_LANGUAGE;                                   // Mod Language
            AURAs[76] = SPELL_AURA_FAR_SIGHT;                                      // Far Sight
            AURAs[77] = SPELL_AURA_MECHANIC_IMMUNITY;                              // Immune Mechanic
            AURAs[78] = SPELL_AURA_MOUNTED;                                        // Mounted
            AURAs[79] = SPELL_AURA_MOD_DAMAGE_DONE_PCT;                            // Mod Dmg %
            AURAs[80] = SPELL_AURA_MOD_STAT_PERCENT;                               // Mod Stat %
            // AURAs(	81	) = AddressOf 	SPELL_AURA_SPLIT_DAMAGE				                'Split Damage
            AURAs[82] = SPELL_AURA_WATER_BREATHING;                                // Water Breathing
            AURAs[83] = SPELL_AURA_MOD_BASE_RESISTANCE;                            // Mod Base Resistance
            AURAs[84] = SPELL_AURA_MOD_REGEN;                                      // Mod Health Regen
            AURAs[85] = SPELL_AURA_MOD_POWER_REGEN;                                // Mod Power Regen
            // AURAs(	86	) = AddressOf 	SPELL_AURA_CHANNEL_DEATH_ITEM		                'Create Death Item
            // AURAs(	87	) = AddressOf 	SPELL_AURA_MOD_DAMAGE_TAKEN_PCT			            'Mod Dmg % Taken
            // AURAs(	88	) = AddressOf 	SPELL_AURA_MOD_REGEN				                'Mod Health Regen Percent
            AURAs[89] = SPELL_AURA_PERIODIC_DAMAGE_PERCENT;                        // Periodic Damage Percent
            // AURAs(	90	) = AddressOf 	SPELL_AURA_MOD_RESIST_CHANCE		                'Mod Resist Chance
            // AURAs(	91	) = AddressOf 	SPELL_AURA_MOD_DETECT_RANGE			                'Mod Detect Range
            // AURAs(	92	) = AddressOf 	SPELL_AURA_PREVENTS_FLEEING			                'Prevent Fleeing
            // AURAs(	93	) = AddressOf 	SPELL_AURA_MOD_UNATTACKABLE			                'Mod Uninteractible
            // AURAs(	94	) = AddressOf 	SPELL_AURA_INTERRUPT_REGEN			                'Interrupt Regen
            AURAs[95] = SPELL_AURA_GHOST;                                          // Ghost
            // AURAs(	96	) = AddressOf 	SPELL_AURA_SPELL_MAGNET				                'Spell Magnet
            // AURAs(	97	) = AddressOf 	SPELL_AURA_MANA_SHIELD				                'Mana Shield
            // AURAs(	98	) = AddressOf 	SPELL_AURA_MOD_SKILL_TALENT			                'Mod Skill Talent
            AURAs[99] = SPELL_AURA_MOD_ATTACK_POWER;                               // Mod Attack Power
            // AURAs(	100	) = AddressOf 	SPELL_AURA_AURAS_VISIBLE			                'Auras Visible
            AURAs[101] = SPELL_AURA_MOD_RESISTANCE_PCT;                            // Mod Resistance %
            // AURAs(	102	) = AddressOf 	SPELL_AURA_MOD_CREATURE_ATTACK_POWER			    'Mod Creature Attack Power
            AURAs[103] = SPELL_AURA_MOD_TOTAL_THREAT;                              // Mod Total Threat (Fade)
            AURAs[104] = SPELL_AURA_WATER_WALK;                                    // Water Walk
            AURAs[105] = SPELL_AURA_FEATHER_FALL;                                  // Feather Fall
            AURAs[106] = SPELL_AURA_HOVER;                                         // Hover
            AURAs[107] = SPELL_AURA_ADD_FLAT_MODIFIER;                             // Add Flat Modifier
            AURAs[108] = SPELL_AURA_ADD_PCT_MODIFIER;                              // Add % Modifier
            // AURAs(	109	) = AddressOf 	SPELL_AURA_ADD_TARGET_TRIGGER		                'Add Class Target Trigger
            AURAs[110] = SPELL_AURA_MOD_POWER_REGEN_PERCENT;                       // Mod Power Regen %
            // AURAs(	111	) = AddressOf 	SPELL_AURA_ADD_CASTER_HIT_TRIGGER	                'Add Class Caster Hit Trigger
            // AURAs(	112	) = AddressOf 	SPELL_AURA_OVERRIDE_CLASS_SCRIPTS	                'Override Class Scripts
            // AURAs(	113	) = AddressOf 	SPELL_AURA_MOD_RANGED_DAMAGE_TAKEN	                'Mod Ranged Dmg Taken
            // AURAs(	114	) = AddressOf 	SPELL_AURA_MOD_RANGED_DAMAGE_TAKEN_PCT			    'Mod Ranged % Dmg Taken
            // AURAs(115) = AddressOf SPELL_AURA_MOD_HEALING                                  'Mod Healing
            // AURAs(	116	) = AddressOf 	SPELL_AURA_IGNORE_REGEN_INTERRUPT	                'Regen During Combat
            // AURAs(	117	) = AddressOf 	SPELL_AURA_MOD_MECHANIC_RESISTANCE	                'Mod Mechanic Resistance
            // AURAs(118) = AddressOf SPELL_AURA_MOD_HEALING_PCT                              'Mod Healing %
            // AURAs(	119	) = AddressOf 	SPELL_AURA_SHARE_PET_TRACKING		                'Share Pet Tracking
            // AURAs(	120	) = AddressOf 	SPELL_AURA_UNTRACKABLE				                'Untrackable
            AURAs[121] = SPELL_AURA_EMPATHY;                                       // Empathy (Lore, whatever)
            // AURAs(	122	) = AddressOf 	SPELL_AURA_MOD_OFFHAND_DAMAGE_PCT	                'Mod Offhand Dmg %
            // AURAs(	123	) = AddressOf 	SPELL_AURA_MOD_POWER_COST_PCT		                'Mod Power Cost %
            AURAs[124] = SPELL_AURA_MOD_RANGED_ATTACK_POWER;                       // Mod Ranged Attack Power
            // AURAs(	125	) = AddressOf 	SPELL_AURA_MOD_MELEE_DAMAGE_TAKEN	                'Mod Melee Dmg Taken
            // AURAs(	126	) = AddressOf 	SPELL_AURA_MOD_MELEE_DAMAGE_TAKEN_PCT			    'Mod Melee % Dmg Taken
            // AURAs(	127	) = AddressOf 	SPELL_AURA_RANGED_ATTACK_POWER_ATTACKER_BONUS	    'Rngd Atk Pwr Attckr Bonus
            // AURAs(	128	) = AddressOf 	SPELL_AURA_MOD_POSSESS_PET			                'Mod Possess Pet
            AURAs[129] = SPELL_AURA_MOD_INCREASE_SPEED_ALWAYS;                     // Mod Speed Always
            AURAs[130] = SPELL_AURA_MOD_INCREASE_MOUNTED_SPEED_ALWAYS;             // Mod Mounted Speed Always
            // AURAs(	131	) = AddressOf 	SPELL_AURA_MOD_CREATURE_RANGED_ATTACK_POWER		    'Mod Creature Ranged Attack Power
            // AURAs(	132	) = AddressOf 	SPELL_AURA_MOD_INCREASE_ENERGY_PERCENT			    'Mod Increase Energy %
            // AURAs(	133	) = AddressOf 	SPELL_AURA_MOD_INCREASE_HEALTH_PERCENT			    'Mod Max Health %
            // AURAs(	134	) = AddressOf 	SPELL_AURA_MOD_MANA_REGEN_INTERRUPT				    'Mod Interrupted Mana Regen
            AURAs[135] = SPELL_AURA_MOD_HEALING_DONE;                              // Mod Healing Done
            AURAs[136] = SPELL_AURA_MOD_HEALING_DONE_PCT;                          // Mod Healing Done %
            AURAs[137] = SPELL_AURA_MOD_TOTAL_STAT_PERCENTAGE;                     // Mod Total Stat %
            AURAs[138] = SPELL_AURA_MOD_HASTE;                                     // Haste - Melee
            // AURAs(	139	) = AddressOf 	SPELL_AURA_FORCE_REACTION			                'Force Reaction
            AURAs[140] = SPELL_AURA_MOD_RANGED_HASTE;                              // Haste - Ranged
            AURAs[141] = SPELL_AURA_MOD_RANGED_AMMO_HASTE;                         // Haste - Ranged (Ammo Only)
            AURAs[142] = SPELL_AURA_MOD_BASE_RESISTANCE_PCT;                       // Mod Base Resistance %
            AURAs[143] = SPELL_AURA_MOD_RESISTANCE_EXCLUSIVE;                      // Mod Resistance Exclusive
            AURAs[144] = SPELL_AURA_SAFE_FALL;                                     // Safe Fall
            // AURAs(	145	) = AddressOf 	SPELL_AURA_CHARISMA				                    'Charisma
            // AURAs(	146	) = AddressOf 	SPELL_AURA_PERSUADED				                'Persuaded
            // AURAs(	147	) = AddressOf 	SPELL_AURA_ADD_CREATURE_IMMUNITY	                'Add Creature Immunity
            // AURAs(	148	) = AddressOf 	SPELL_AURA_RETAIN_COMBO_POINTS		                'Retain Combo Points
            // AURAs(	149	) = AddressOf 	SPELL_AURA_RESIST_PUSHBACK			                'Resist Pushback
            // AURAs(	150	) = AddressOf 	SPELL_AURA_MOD_SHIELD_BLOCK			                'Mod Shield Block %
            // AURAs(	151	) = AddressOf 	SPELL_AURA_TRACK_STEALTHED			                'Track Stealthed
            // AURAs(	152	) = AddressOf 	SPELL_AURA_MOD_DETECTED_RANGE		                'Mod Aggro Range
            // AURAs(	153	) = AddressOf 	SPELL_AURA_SPLIT_DAMAGE_FLAT		                'Split Damage Flat
            AURAs[154] = SPELL_AURA_MOD_STEALTH_LEVEL;                             // Stealth Level Modifier
            // AURAs(	155	) = AddressOf 	SPELL_AURA_MOD_WATER_BREATHING		                'Mod Water Breathing
            // AURAs(	156	) = AddressOf 	SPELL_AURA_MOD_REPUTATION_ADJUST	                'Mod Reputation Gain
            // AURAs(	157	) = AddressOf 	SPELL_AURA_PET_DAMAGE_MULTI			                'Mod Pet Damage
            // AURAs(	158	) = AddressOf   SPELL_AURA_MOD_SHIELD_BLOCKVALUE                    'Mod Shield Block
            // AURAs(	159	) = AddressOf   SPELL_AURA_NO_PVP_CREDIT                            'Honorless
            // AURAs(	160 ) = AddressOf 	SPELL_AURA_MOD_AOE_AVOIDANCE		                'Mod Side/Rear PBAE Damage Taken %
            // AURAs(	161 ) = AddressOf 	SPELL_AURA_MOD_HEALTH_REGEN_IN_COMBAT               'Mod Health Regen In Combat
            // AURAs(	162 ) = AddressOf 	SPELL_AURA_POWER_BURN_MANA                        	'Power Burn (Mana)
            // AURAs(	163 ) = AddressOf 	SPELL_AURA_MOD_CRIT_DAMAGE_BONUS_MELEE              'Mod Critical Damage
            // AURAs(	164 ) = AddressOf  	SPELL_AURA_164                        			    'TEST
            // AURAs(	165 ) = AddressOf  	SPELL_AURA_MELEE_ATTACK_POWER_ATTACKER_BONUS        '
            // AURAs(	166 ) = AddressOf 	SPELL_AURA_MOD_ATTACK_POWER_PCT                     'Mod Attack Power %
            // AURAs( 167 ) = AddressOf   SPELL_AURA_MOD_RANGED_ATTACK_POWER_PCT              'Mod Ranged Attack Power %
            // AURAs(	168 ) = AddressOf 	SPELL_AURA_MOD_DAMAGE_DONE_VERSUS                   'Increase Damage % (vs. %X)
            // AURAs(	169 ) = AddressOf 	SPELL_AURA_MOD_CRIT_PERCENT_VERSUS                  'Increase Critical % (vs. %X)
            // AURAs(	170 ) = AddressOf  	SPELL_AURA_DETECT_AMORE                       		'
            // AURAs(	171 ) = AddressOf  	SPELL_AURA_MOD_SPEED_NOT_STACK                      '
            // AURAs(	172 ) = AddressOf  	SPELL_AURA_MOD_MOUNTED_SPEED_NOT_STACK              '
            // AURAs(	173 ) = AddressOf  	SPELL_AURA_ALLOW_CHAMPION_SPELLS                    '
            // AURAs(	174 ) = AddressOf 	SPELL_AURA_MOD_SPELL_DAMAGE_OF_STAT_PERCENT	        'Increase Spell Damage by % Spirit (Spells)
            // AURAs(	175 ) = AddressOf 	SPELL_AURA_MOD_SPELL_HEALING_OF_STAT_PERCENT        'Increase Spell Healing by % Spirit
            // AURAs(	176 ) = AddressOf  	SPELL_AURA_SPIRIT_OF_REDEMPTION                     '
            // AURAs(	177 ) = AddressOf 	SPELL_AURA_AOE_CHARM                        		'Charm
            // AURAs(	178 ) = AddressOf  	SPELL_AURA_MOD_DEBUFF_RESISTANCE                    '
            // AURAs(	179 ) = AddressOf  	SPELL_AURA_MOD_ATTACKER_SPELL_CRIT_CHANCE           '
            // AURAs(	180	) = AddressOf 	SPELL_AURA_MOD_FLAT_SPELL_DAMAGE_VERSUS             'Increase Spell Damage (vs. %X)
            // AURAs(	171 ) = AddressOf  	SPELL_AURA_MOD_FLAT_SPELL_CRIT_DAMAGE_VERSUS        '
            // AURAs(	182	) = AddressOf 	SPELL_AURA_MOD_RESISTANCE_OF_STAT_PERCENT           'Increase Resist by % of Intellect (%X)
            // AURAs(	183	) = AddressOf 	SPELL_AURA_MOD_CRITICAL_THREAT                      'Decrease Critical Threat by % (Spells)
            // AURAs(	184	) = AddressOf   SPELL_AURA_MOD_ATTACKER_MELEE_HIT_CHANCE            'Mod Melee GetHit Chance
            // AURAs(	185	) = AddressOf   SPELL_AURA_MOD_ATTACKER_RANGED_HIT_CHANCE           'Mod Ranged GetHit Chance
            // AURAs(	186	) = AddressOf   SPELL_AURA_MOD_ATTACKER_SPELL_HIT_CHANCE            'Mod Spell GetHit Chance
            // AURAs(	187	) = AddressOf   SPELL_AURA_MOD_ATTACKER_MELEE_CRIT_CHANCE           'Mod Melee Critical GetHit Chance
            // AURAs(	188	) = AddressOf   SPELL_AURA_MOD_ATTACKER_RANGED_CRIT_CHANCE          'Mod Ranged Critical GetHit Chance
            // AURAs(	189	) = AddressOf   SPELL_AURA_MOD_RATING                               'Mod Skill Rating
            // AURAs(	190	) = AddressOf   SPELL_AURA_MOD_FACTION_REPUTATION_GAIN              'Mod Reputation Gain
            // AURAs(	191	) = AddressOf   SPELL_AURA_USE_NORMAL_MOVEMENT_SPEED                '
            // AURAs(	192	) = AddressOf   SPELL_AURA_HASTE_MELEE                              '
            // AURAs(	193	) = AddressOf   SPELL_AURA_MELEE_SLOW                               '
            // AURAs(	194	) = AddressOf   SPELL_AURA_MOD_DEPRICATED_1                         '
            // AURAs(	195	) = AddressOf   SPELL_AURA_MOD_DEPRICATED_2                         '
            // AURAs(	196	) = AddressOf   SPELL_AURA_MOD_COOLDOWN                             'Mod Global Cooldowns
            // AURAs(	197	) = AddressOf   SPELL_AURA_MOD_ATTACKER_SPELL_AND_WEAPON_CRIT_CHANCE'No Critical Damage Taken
            // AURAs(	198	) = AddressOf   SPELL_AURA_MOD_ALL_WEAPON_SKILLS                    'Mod Weapon Skills
            // AURAs(	199	) = AddressOf   SPELL_AURA_MOD_INCREASES_SPELL_PCT_TO_HIT           'Mod Hit Chance
            // AURAs(	200	) = AddressOf   SPELL_AURA_MOD_XP_PCT                               'Mod Gained XP
            // AURAs(	201	) = AddressOf   SPELL_AURA_FLY                                      'Fly
            // AURAs(	202	) = AddressOf   SPELL_AURA_IGNORE_COMBAT_RESULT                     '
            // AURAs(	203	) = AddressOf   SPELL_AURA_MOD_ATTACKER_MELEE_CRIT_DAMAGE           'Mod Melee Critical Damage Taken
            // AURAs(	204	) = AddressOf   SPELL_AURA_MOD_ATTACKER_RANGED_CRIT_DAMAGE          'Mod Ranged Critical Damage Taken
            // AURAs(	205	) = AddressOf   SPELL_AURA_205                                      '
            // AURAs(	206	) = AddressOf 	SPELL_AURA_MOD_SPEED_MOUNTED                        'Mod Fly Speed Always
            // AURAs(207) = AddressOf SPELL_AURA_MOD_INCREASE_MOUNTED_FLY_SPEED                'Mod Fly Speed Mounted
            // AURAs(208) = AddressOf SPELL_AURA_MOD_INCREASE_FLY_SPEED                        'Mod Fly Speed
            // AURAs(209) = AddressOf SPELL_AURA_MOD_INCREASE_MOUNTED_FLY_SPEED_ALWAYS         'Mod Fly Speed Mounted Always
            // AURAs(	210	) = AddressOf 	SPELL_AURA_210                                      '
            // AURAs(	211	) = AddressOf 	SPELL_AURA_MOD_FLIGHT_SPEED_NOT_STACK               '
            // AURAs(	212	) = AddressOf 	SPELL_AURA_MOD_RANGED_ATTACK_POWER_OF_STAT_PERCENT  'Mod Ranged Attack Power by % of Intellect
            // AURAs(	213	) = AddressOf 	SPELL_AURA_MOD_RAGE_FROM_DAMAGE_DEALT               'Mod Rage From Damage
            // AURAs(	214	) = AddressOf 	SPELL_AURA_214                                      '
            // AURAs(	215	) = AddressOf 	SPELL_AURA_ARENA_PREPARATION                        'TEST
            // AURAs(	216	) = AddressOf 	SPELL_AURA_HASTE_SPELLS                             'Mod Casting Speed
            // AURAs(	217	) = AddressOf 	SPELL_AURA_217                                      '
            // AURAs(	218	) = AddressOf 	SPELL_AURA_HASTE_RANGED                             '
            // AURAs(	219	) = AddressOf 	SPELL_AURA_MOD_MANA_REGEN_FROM_STAT                 'Mod Regenerate by % of Intellect
            // AURAs(	220	) = AddressOf 	SPELL_AURA_MOD_RATING_FROM_STAT                     '
            // AURAs(	221	) = AddressOf 	SPELL_AURA_221                                      '
            // AURAs(	222	) = AddressOf 	SPELL_AURA_222                                      '
            // AURAs(	223	) = AddressOf 	SPELL_AURA_223                                      '
            // AURAs(	224	) = AddressOf 	SPELL_AURA_224                                      '
            // AURAs(	225	) = AddressOf 	SPELL_AURA_PRAYER_OF_MENDING                        '
            AURAs[226] = SPELL_AURA_PERIODIC_DUMMY;                                // Periodic dummy
            // AURAs( 227 ) = AddressOf   SPELL_AURA_227                                      '
            AURAs[228] = SPELL_AURA_DETECT_STEALTH;                                // Detect stealth
            // AURAs( 229 ) = AddressOf   SPELL_AURA_MOD_AOE_DAMAGE_AVOIDANCE                 '
            // AURAs( 230 ) = AddressOf   SPELL_AURA_230                                      '
            // AURAs( 231 ) = AddressOf   SPELL_AURA_231                                      '
            // AURAs( 232 ) = AddressOf   SPELL_AURA_MECHANIC_DURATION_MOD                    '
            // AURAs( 233 ) = AddressOf   SPELL_AURA_233                                      '
            // AURAs( 234 ) = AddressOf   AURA_MECHANIC_DURATION_MOD_NOT_STACK                '
            // AURAs( 235 ) = AddressOf   SPELL_AURA_MOD_DISPEL_RESIST                        '
            // AURAs( 236 ) = AddressOf   SPELL_AURA_236                                      '
            // AURAs( 237 ) = AddressOf   SPELL_AURA_MOD_SPELL_DAMAGE_OF_ATTACK_POWER         '
            // AURAs( 238 ) = AddressOf   SPELL_AURA_MOD_SPELL_HEALING_OF_ATTACK_POWER        '
            // AURAs( 239 ) = AddressOf   SPELL_AURA_MOD_SCALE_2                              '
            // AURAs( 240 ) = AddressOf   SPELL_AURA_MOD_EXPERTISE                            '
            // AURAs( 241 ) = AddressOf   SPELL_AURA_241                                      '
            // AURAs( 242 ) = AddressOf   SPELL_AURA_MOD_SPELL_DAMAGE_FROM_HEALING            '
            // AURAs( 243 ) = AddressOf   SPELL_AURA_243                                      '
            // AURAs( 244 ) = AddressOf   SPELL_AURA_244                                      '
            // AURAs( 245 ) = AddressOf   SPELL_AURA_MOD_DURATION_OF_MAGIC_EFFECTS            '
            // AURAs( 246 ) = AddressOf   SPELL_AURA_246                                      '
            // AURAs( 247 ) = AddressOf   SPELL_AURA_247                                      '
            // AURAs( 248 ) = AddressOf   SPELL_AURA_MOD_COMBAT_RESULT_CHANCE                 '
            // AURAs( 249 ) = AddressOf   SPELL_AURA_249                                      '
            // AURAs( 250 ) = AddressOf   SPELL_AURA_MOD_INCREASE_HEALTH_2                    '
            // AURAs( 251 ) = AddressOf   SPELL_AURA_MOD_ENEMY_DODGE                          '
            // AURAs( 252 ) = AddressOf   SPELL_AURA_252                                      '
            // AURAs( 253 ) = AddressOf   SPELL_AURA_253                                      '
            // AURAs( 254 ) = AddressOf   SPELL_AURA_254                                      '
            // AURAs( 255 ) = AddressOf   SPELL_AURA_255                                      '
            // AURAs( 256 ) = AddressOf   SPELL_AURA_256                                      '
            // AURAs( 257 ) = AddressOf   SPELL_AURA_257                                      '
            // AURAs( 258 ) = AddressOf   SPELL_AURA_258                                      '
            // AURAs( 259 ) = AddressOf   SPELL_AURA_259                                      '
            // AURAs( 260 ) = AddressOf   SPELL_AURA_260                                      '
            // AURAs( 261 ) = AddressOf   SPELL_AURA_261                                      '
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public delegate SpellFailedReason SpellEffectHandler(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item);

        public const int SPELL_EFFECT_COUNT = 153;
        public SpellEffectHandler[] SPELL_EFFECTs = new SpellEffectHandler[154];

        public SpellFailedReason SPELL_EFFECT_NOTHING(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_BIND(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            foreach (WS_Base.BaseUnit Unit in Infected)
            {
                if (Unit is WS_PlayerData.CharacterObject)
                {
                    ((WS_PlayerData.CharacterObject)Unit).BindPlayer(Caster.GUID);
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
                WS_Base.BaseUnit argAttacker = (WS_Base.BaseUnit)Caster;
                Unit.Die(ref argAttacker);
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_SCHOOL_DAMAGE(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            int Current = 0;
            foreach (WS_Base.BaseUnit Unit in Infected)
            {
                int Damage;
                if (Caster is WS_DynamicObjects.DynamicObjectObject)
                {
                    Damage = SpellInfo.get_GetValue(((WS_DynamicObjects.DynamicObjectObject)Caster).Caster.Level, 0);
                }
                else
                {
                    Damage = SpellInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                }

                if (Current > 0)
                    Damage = (int)(Damage * Math.Pow(SpellInfo.DamageMultiplier, Current));
                WS_Base.BaseUnit realCaster = null;
                if (Caster is WS_Base.BaseUnit)
                {
                    realCaster = (WS_Base.BaseUnit)Caster;
                }
                else if (Caster is WS_DynamicObjects.DynamicObjectObject)
                {
                    realCaster = ((WS_DynamicObjects.DynamicObjectObject)Caster).Caster;
                }

                if (realCaster is object)
                    Unit.DealSpellDamage(ref realCaster, ref SpellInfo, SpellID, Damage, SPELLs[SpellID].School, SpellType.SPELL_TYPE_NONMELEE);
                Current += 1;
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_ENVIRONMENTAL_DAMAGE(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            int Damage = SpellInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
            foreach (WS_Base.BaseUnit Unit in Infected)
            {
                WS_Base.BaseUnit argAttacker = (WS_Base.BaseUnit)Caster;
                Unit.DealDamage(Damage, ref argAttacker);
                if (Unit is WS_PlayerData.CharacterObject)
                    ((WS_PlayerData.CharacterObject)Unit).LogEnvironmentalDamage(SPELLs[SpellID].School, Damage);
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_TRIGGER_SPELL(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            // NOTE: Trigger spell shouldn't add a cast error?
            if (SPELLs.ContainsKey(SpellInfo.TriggerSpell) == false)
                return SpellFailedReason.SPELL_NO_ERROR;
            if (Target.unitTarget is null)
                return SpellFailedReason.SPELL_NO_ERROR;
            switch (SpellInfo.TriggerSpell)
            {
                case 18461:
                    {
                        int argNotSpellID = 0;
                        Target.unitTarget.RemoveAurasOfType(AuraEffects_Names.SPELL_AURA_MOD_ROOT, NotSpellID: ref argNotSpellID);
                        int argNotSpellID1 = 0;
                        Target.unitTarget.RemoveAurasOfType(AuraEffects_Names.SPELL_AURA_MOD_DECREASE_SPEED, NotSpellID: ref argNotSpellID1);
                        int argNotSpellID2 = 0;
                        Target.unitTarget.RemoveAurasOfType(AuraEffects_Names.SPELL_AURA_MOD_STALKED, NotSpellID: ref argNotSpellID2);
                        break;
                    }

                // TODO: Cast highest rank of stealth
                case 35729:
                    {
                        for (byte i = WorldServiceLocator._Global_Constants.MAX_POSITIVE_AURA_EFFECTs, loopTo = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1; i <= loopTo; i++)
                        {
                            if (Target.unitTarget.ActiveSpells[i] is object)
                            {
                                if ((SPELLs[Target.unitTarget.ActiveSpells[i].SpellID].School & 1) == 0) // No physical spells
                                {
                                    if (Conversions.ToBoolean(SPELLs[Target.unitTarget.ActiveSpells[i].SpellID].Attributes & 0x10000))
                                    {
                                        Target.unitTarget.RemoveAura(i, ref Target.unitTarget.ActiveSpells[i].SpellCaster);
                                    }
                                }
                            }
                        }

                        break;
                    }
            }

            if (SPELLs[SpellInfo.TriggerSpell].EquippedItemClass >= 0 & Caster is WS_PlayerData.CharacterObject)
            {
                // If (SPELLs(SpellInfo.TriggerSpell).AttributesEx3 And SpellAttributesEx3.SPELL_ATTR_EX3_MAIN_HAND) Then
                // If CType(Caster, CharacterObject).Items.ContainsKey(EQUIPMENT_SLOT_MAINHAND) = False Then Return SpellFailedReason.SPELL_NO_ERROR
                // If CType(Caster, CharacterObject).Items(EQUIPMENT_SLOT_MAINHAND).IsBroken Then Return SpellFailedReason.SPELL_NO_ERROR
                // End If
                // If (SPELLs(SpellInfo.TriggerSpell).AttributesEx3 And SpellAttributesEx3.SPELL_ATTR_EX3_REQ_OFFHAND) Then
                // If CType(Caster, CharacterObject).Items.ContainsKey(EQUIPMENT_SLOT_OFFHAND) = False Then Return SpellFailedReason.SPELL_NO_ERROR
                // If CType(Caster, CharacterObject).Items(EQUIPMENT_SLOT_OFFHAND).IsBroken Then Return SpellFailedReason.SPELL_NO_ERROR
                // End If
            }

            var castParams = new CastSpellParameters(ref Target, ref Caster, SpellInfo.TriggerSpell);
            ThreadPool.QueueUserWorkItem(new WaitCallback(castParams.Cast));
            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_TELEPORT_UNITS(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            foreach (WS_Base.BaseUnit Unit in Infected)
            {
                if (Unit is WS_PlayerData.CharacterObject)
                {
                    {
                        var withBlock = (WS_PlayerData.CharacterObject)Unit;
                        switch (SpellID)
                        {
                            case 8690: // Hearthstone
                                {
                                    withBlock.Teleport(withBlock.bindpoint_positionX, withBlock.bindpoint_positionY, withBlock.bindpoint_positionZ, withBlock.orientation, withBlock.bindpoint_map_id);
                                    break;
                                }

                            default:
                                {
                                    if (WorldServiceLocator._WS_DBCDatabase.TeleportCoords.ContainsKey(SpellID))
                                    {
                                        withBlock.Teleport(WorldServiceLocator._WS_DBCDatabase.TeleportCoords[SpellID].PosX, WorldServiceLocator._WS_DBCDatabase.TeleportCoords[SpellID].PosY, WorldServiceLocator._WS_DBCDatabase.TeleportCoords[SpellID].PosZ, withBlock.orientation, (int)WorldServiceLocator._WS_DBCDatabase.TeleportCoords[SpellID].MapID);
                                    }
                                    else
                                    {
                                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "WARNING: Spell {0} did not have any teleport coordinates.", SpellID);
                                    }

                                    break;
                                }
                        }
                    }
                }
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_MANA_DRAIN(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            foreach (WS_Base.BaseUnit Unit in Infected)
            {
                int Damage = SpellInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                if (Caster is WS_PlayerData.CharacterObject)
                    Damage += SpellInfo.valuePerLevel * ((WS_PlayerData.CharacterObject)Caster).Level;

                // DONE: Take the power from the target and give to the caster
                // TODO: Rune power?
                int TargetPower = 0;
                switch (SpellInfo.MiscValue)
                {
                    case var @case when @case == ManaTypes.TYPE_MANA:
                        {
                            if (Damage > Unit.Mana.Current)
                                Damage = Unit.Mana.Current;
                            Unit.Mana.Current -= Damage;
                            ((WS_Base.BaseUnit)Caster).Mana.Current += Damage;
                            TargetPower = Unit.Mana.Current;
                            break;
                        }

                    case var case1 when case1 == ManaTypes.TYPE_RAGE:
                        {
                            if (Unit is WS_PlayerData.CharacterObject && Caster is WS_PlayerData.CharacterObject)
                            {
                                if (Damage > ((WS_PlayerData.CharacterObject)Unit).Rage.Current)
                                    Damage = ((WS_PlayerData.CharacterObject)Unit).Rage.Current;
                                ((WS_PlayerData.CharacterObject)Unit).Rage.Current -= Damage;
                                ((WS_PlayerData.CharacterObject)Caster).Rage.Current += Damage;
                                TargetPower = ((WS_PlayerData.CharacterObject)Unit).Rage.Current;
                            }

                            break;
                        }

                    case var case2 when case2 == ManaTypes.TYPE_ENERGY:
                        {
                            if (Unit is WS_PlayerData.CharacterObject && Caster is WS_PlayerData.CharacterObject)
                            {
                                if (Damage > ((WS_PlayerData.CharacterObject)Unit).Energy.Current)
                                    Damage = ((WS_PlayerData.CharacterObject)Unit).Energy.Current;
                                ((WS_PlayerData.CharacterObject)Unit).Energy.Current -= Damage;
                                ((WS_PlayerData.CharacterObject)Caster).Energy.Current += Damage;
                                TargetPower = ((WS_PlayerData.CharacterObject)Unit).Energy.Current;
                            }

                            break;
                        }

                    default:
                        {
                            Unit.Mana.Current -= Damage;
                            ((WS_Base.BaseUnit)Caster).Mana.Current += Damage;
                            TargetPower = Unit.Mana.Current;
                            break;
                        }
                }

                // DONE: Send victim mana update, for near
                if (Unit is WS_Creatures.CreatureObject)
                {
                    var myTmpUpdate = new Packets.UpdateClass(EUnitFields.UNIT_END);
                    var myPacket = new Packets.UpdatePacketClass();
                    myTmpUpdate.SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER1 + SpellInfo.MiscValue, TargetPower);
                    myTmpUpdate.AddToPacket(myPacket, ObjectUpdateType.UPDATETYPE_VALUES, (WS_Creatures.CreatureObject)Unit);
                    Packets.PacketClass argpacket = myPacket;
                    Unit.SendToNearPlayers(ref argpacket);
                    myPacket.Dispose();
                    myTmpUpdate.Dispose();
                }
                else if (Unit is WS_PlayerData.CharacterObject)
                {
                    ((WS_PlayerData.CharacterObject)Unit).SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER1 + SpellInfo.MiscValue, TargetPower);
                    ((WS_PlayerData.CharacterObject)Unit).SendCharacterUpdate();
                }
            }

            // TODO: SpellFailedReason.SPELL_FAILED_ALREADY_FULL_MANA
            // DONE: Send caster mana update, for near
            int CasterPower = 0;
            switch (SpellInfo.MiscValue)
            {
                case var case3 when case3 == ManaTypes.TYPE_MANA:
                    {
                        CasterPower = ((WS_Base.BaseUnit)Caster).Mana.Current;
                        break;
                    }

                case var case4 when case4 == ManaTypes.TYPE_RAGE:
                    {
                        if (Caster is WS_PlayerData.CharacterObject)
                            CasterPower = ((WS_PlayerData.CharacterObject)Caster).Rage.Current;
                        break;
                    }

                case var case5 when case5 == ManaTypes.TYPE_ENERGY:
                    {
                        if (Caster is WS_PlayerData.CharacterObject)
                            CasterPower = ((WS_PlayerData.CharacterObject)Caster).Energy.Current;
                        break;
                    }

                default:
                    {
                        CasterPower = ((WS_Base.BaseUnit)Caster).Mana.Current;
                        break;
                    }
            }

            if (Caster is WS_Creatures.CreatureObject)
            {
                var TmpUpdate = new Packets.UpdateClass(EUnitFields.UNIT_END);
                var Packet = new Packets.UpdatePacketClass();
                TmpUpdate.SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER1 + SpellInfo.MiscValue, CasterPower);
                TmpUpdate.AddToPacket(Packet, ObjectUpdateType.UPDATETYPE_VALUES, (WS_Creatures.CreatureObject)Caster);
                Packets.PacketClass argpacket1 = Packet;
                Target.unitTarget.SendToNearPlayers(ref argpacket1);
                Packet.Dispose();
                TmpUpdate.Dispose();
            }
            else if (Caster is WS_PlayerData.CharacterObject)
            {
                ((WS_PlayerData.CharacterObject)Caster).SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER1 + SpellInfo.MiscValue, CasterPower);
                ((WS_PlayerData.CharacterObject)Caster).SendCharacterUpdate();
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_HEAL(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            int Current = 0;
            foreach (WS_Base.BaseUnit Unit in Infected)
            {
                int Damage = SpellInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                if (Current > 0)
                    Damage = (int)(Damage * Math.Pow(SpellInfo.DamageMultiplier, Current));

                // NOTE: This function heals as well
                Unit.DealSpellDamage(ref Caster, ref SpellInfo, SpellID, Damage, SPELLs[SpellID].School, SpellType.SPELL_TYPE_HEAL);
                Current += 1;
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
                    Damage = (int)(Damage * Math.Pow(SpellInfo.DamageMultiplier, Current));

                // NOTE: This function heals as well
                Unit.DealSpellDamage(ref Caster, ref SpellInfo, SpellID, Damage, SPELLs[SpellID].School, SpellType.SPELL_TYPE_HEAL);
                Current += 1;
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_ENERGIZE(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            int Damage = SpellInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
            foreach (WS_Base.BaseUnit Unit in Infected)
            {
                WS_Base.BaseUnit argCaster = (WS_Base.BaseUnit)Caster;
                SendEnergizeSpellLog(ref argCaster, ref Target.unitTarget, SpellID, Damage, SpellInfo.MiscValue);
                Unit.Energize(Damage, SpellInfo.MiscValue, ref Caster);
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_ENERGIZE_PCT(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            int Damage = 0;
            foreach (WS_Base.BaseUnit Unit in Infected)
            {
                switch (SpellInfo.MiscValue)
                {
                    case var @case when @case == ManaTypes.TYPE_MANA:
                        {
                            Damage = (int)(SpellInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d * Unit.Mana.Maximum);
                            break;
                        }
                }

                WS_Base.BaseUnit argCaster = (WS_Base.BaseUnit)Caster;
                SendEnergizeSpellLog(ref argCaster, ref Target.unitTarget, SpellID, Damage, SpellInfo.MiscValue);
                Unit.Energize(Damage, SpellInfo.MiscValue, ref Caster);
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_OPEN_LOCK(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            if (!(Caster is WS_PlayerData.CharacterObject))
                return SpellFailedReason.SPELL_FAILED_ERROR;
            LootType LootType = LootType.LOOTTYPE_CORPSE;
            ulong targetGUID;
            int lockID;
            if (Target.goTarget is object) // GO Target
            {
                targetGUID = Target.goTarget.GUID;
                lockID = ((WS_GameObjects.GameObjectObject)Target.goTarget).LockID;
            }
            else if (Target.itemTarget is object) // Item Target
            {
                targetGUID = Target.itemTarget.GUID;
                lockID = Target.itemTarget.ItemInfo.LockID;
            }
            else
            {
                return SpellFailedReason.SPELL_FAILED_BAD_TARGETS;
            }

            // TODO: Check if it's a battlegroundflag

            if (lockID == 0)
            {
                // TODO: Send loot for items
                if (WorldServiceLocator._CommonGlobalFunctions.GuidIsGameObject(targetGUID) && WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(targetGUID))
                {
                    WS_PlayerData.CharacterObject argCharacter = (WS_PlayerData.CharacterObject)Caster;
                    WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[targetGUID].LootObject(ref argCharacter, LootType);
                }

                return SpellFailedReason.SPELL_NO_ERROR;
            }

            if (WorldServiceLocator._WS_Loot.Locks.ContainsKey(lockID) == false)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[DEBUG] Lock {0} did not exist.", lockID);
                return SpellFailedReason.SPELL_FAILED_ERROR;
            }

            for (byte i = 0; i <= 4; i++)
            {
                if (Item is object && WorldServiceLocator._WS_Loot.Locks[lockID].KeyType[i] == LockKeyType.LOCK_KEY_ITEM && WorldServiceLocator._WS_Loot.Locks[lockID].Keys[i] == Item.ItemEntry)
                {
                    // TODO: Send loot for items
                    if (WorldServiceLocator._CommonGlobalFunctions.GuidIsGameObject(targetGUID) && WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(targetGUID))
                    {
                        WS_PlayerData.CharacterObject argCharacter1 = (WS_PlayerData.CharacterObject)Caster;
                        WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[targetGUID].LootObject(ref argCharacter1, LootType);
                    }

                    return SpellFailedReason.SPELL_NO_ERROR;
                }
            }

            int SkillID = 0;
            if (SPELLs[SpellID].SpellEffects[1] is object && SPELLs[SpellID].SpellEffects[1].ID == SpellEffects_Names.SPELL_EFFECT_SKILL)
            {
                SkillID = SPELLs[SpellID].SpellEffects[1].MiscValue;
            }
            else if (SPELLs[SpellID].SpellEffects[0] is object && SPELLs[SpellID].SpellEffects[0].MiscValue == LockType.LOCKTYPE_PICKLOCK)
            {
                SkillID = SKILL_IDs.SKILL_LOCKPICKING;
            }

            short ReqSkillValue = WorldServiceLocator._WS_Loot.Locks[lockID].RequiredMiningSkill;
            if (WorldServiceLocator._WS_Loot.Locks[lockID].RequiredLockingSkill > 0)
            {
                if (SkillID != SKILL_IDs.SKILL_LOCKPICKING) // Cheat attempt?
                {
                    return SpellFailedReason.SPELL_FAILED_FIZZLE;
                }

                ReqSkillValue = WorldServiceLocator._WS_Loot.Locks[lockID].RequiredLockingSkill;
            }
            else if (SkillID == SKILL_IDs.SKILL_LOCKPICKING) // Apply picklock skill to wrong target
            {
                return SpellFailedReason.SPELL_FAILED_BAD_TARGETS;
            }

            if (Conversions.ToBoolean(SkillID))
            {
                LootType = LootType.LOOTTYPE_SKINNING;
                if (((WS_PlayerData.CharacterObject)Caster).Skills.ContainsKey(SkillID) == false || ((WS_PlayerData.CharacterObject)Caster).Skills[SkillID].Current < ReqSkillValue)
                {
                    return SpellFailedReason.SPELL_FAILED_LOW_CASTLEVEL;
                }

                // TODO: Update skill
            }

            // TODO: Send loot for items
            if (WorldServiceLocator._CommonGlobalFunctions.GuidIsGameObject(targetGUID) && WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(targetGUID))
            {
                WS_PlayerData.CharacterObject argCharacter2 = (WS_PlayerData.CharacterObject)Caster;
                WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[targetGUID].LootObject(ref argCharacter2, LootType);
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_PICKPOCKET(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            if (!(Caster is WS_PlayerData.CharacterObject))
                return SpellFailedReason.SPELL_FAILED_ERROR;

            // DONE: Pickpocket the creature!
            if (Target.unitTarget is WS_Creatures.CreatureObject && ((WS_Base.BaseUnit)Caster).IsFriendlyTo(ref Target.unitTarget) == false)
            {
                {
                    var withBlock = (WS_Creatures.CreatureObject)Target.unitTarget;
                    if (withBlock.CreatureInfo.CreatureType == UNIT_TYPE.HUMANOID || withBlock.CreatureInfo.CreatureType == UNIT_TYPE.UNDEAD)
                    {
                        if (withBlock.IsDead == false)
                        {
                            int chance = 10 + ((WS_Base.BaseUnit)Caster).Level - withBlock.Level;
                            if (chance > WorldServiceLocator._WorldServer.Rnd.Next(0, 20))
                            {
                                // Successful pickpocket
                                if (withBlock.CreatureInfo.PocketLootID > 0)
                                {
                                    var Loot = new WS_Loot.LootObject(withBlock.GUID, LootType.LOOTTYPE_PICKPOCKETING) { LootOwner = Caster.GUID };
                                    var Template = WorldServiceLocator._WS_Loot.LootTemplates_Pickpocketing.GetLoot(withBlock.CreatureInfo.PocketLootID);
                                    if (Template is object)
                                    {
                                        Template.Process(ref Loot, 0);
                                    }

                                    Loot.SendLoot(ref ((WS_PlayerData.CharacterObject)Caster).client);
                                }
                                else
                                {
                                    WorldServiceLocator._WS_Loot.SendEmptyLoot(withBlock.GUID, LootType.LOOTTYPE_PICKPOCKETING, ref ((WS_PlayerData.CharacterObject)Caster).client);
                                }
                            }
                            else
                            {
                                // Failed pickpocket
                                ((WS_Base.BaseUnit)Caster).RemoveAurasByInterruptFlag(SpellAuraInterruptFlags.AURA_INTERRUPT_FLAG_TALK);
                                if (withBlock.aiScript is object)
                                {
                                    WS_Base.BaseUnit argAttacker = (WS_Base.BaseUnit)Caster;
                                    withBlock.aiScript.OnGenerateHate(ref argAttacker, 100);
                                }
                            }

                            return SpellFailedReason.SPELL_NO_ERROR;
                        }
                        else
                        {
                            return SpellFailedReason.SPELL_FAILED_TARGETS_DEAD;
                        }
                    }
                }
            }

            return SpellFailedReason.SPELL_FAILED_BAD_TARGETS;
        }

        public SpellFailedReason SPELL_EFFECT_SKINNING(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            if (!(Caster is WS_PlayerData.CharacterObject))
                return SpellFailedReason.SPELL_FAILED_ERROR;

            // DONE: Skin the creature!
            if (Target.unitTarget is WS_Creatures.CreatureObject)
            {
                {
                    var withBlock = (WS_Creatures.CreatureObject)Target.unitTarget;
                    if (withBlock.IsDead && WorldServiceLocator._Functions.HaveFlags(withBlock.cUnitFlags, UnitFlags.UNIT_FLAG_SKINNABLE))
                    {
                        withBlock.cUnitFlags = withBlock.cUnitFlags & !UnitFlags.UNIT_FLAG_SKINNABLE;
                        // TODO: Is skinning skill requirement met?
                        // TODO: Update skinning skill!

                        if (withBlock.CreatureInfo.SkinLootID > 0)
                        {
                            var Loot = new WS_Loot.LootObject(withBlock.GUID, LootType.LOOTTYPE_SKINNING) { LootOwner = Caster.GUID };
                            var Template = WorldServiceLocator._WS_Loot.LootTemplates_Skinning.GetLoot(withBlock.CreatureInfo.SkinLootID);
                            if (Template is object)
                            {
                                Template.Process(ref Loot, 0);
                            }

                            Loot.SendLoot(ref ((WS_PlayerData.CharacterObject)Caster).client);
                        }
                        else
                        {
                            WorldServiceLocator._WS_Loot.SendEmptyLoot(withBlock.GUID, LootType.LOOTTYPE_SKINNING, ref ((WS_PlayerData.CharacterObject)Caster).client);
                        }

                        var TmpUpdate = new Packets.UpdateClass(EUnitFields.UNIT_END);
                        var Packet = new Packets.UpdatePacketClass();
                        TmpUpdate.SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, withBlock.cUnitFlags);
                        TmpUpdate.AddToPacket(Packet, ObjectUpdateType.UPDATETYPE_VALUES, (WS_Creatures.CreatureObject)Target.unitTarget);
                        Packets.PacketClass argpacket = Packet;
                        Target.unitTarget.SendToNearPlayers(ref argpacket);
                        Packet.Dispose();
                        TmpUpdate.Dispose();
                    }

                    return SpellFailedReason.SPELL_NO_ERROR;
                }
            }

            return SpellFailedReason.SPELL_FAILED_BAD_TARGETS;
        }

        public SpellFailedReason SPELL_EFFECT_DISENCHANT(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            if (!(Caster is WS_PlayerData.CharacterObject))
                return SpellFailedReason.SPELL_FAILED_ERROR;
            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_PROFICIENCY(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            foreach (WS_Base.BaseUnit Unit in Infected)
            {
                if (Unit is WS_PlayerData.CharacterObject)
                {
                    ((WS_PlayerData.CharacterObject)Unit).SendProficiencies();
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
                    if (Unit is WS_PlayerData.CharacterObject)
                    {
                        ((WS_PlayerData.CharacterObject)Unit).LearnSpell(SpellInfo.TriggerSpell);
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
                    if (Unit is WS_PlayerData.CharacterObject)
                    {
                        ((WS_PlayerData.CharacterObject)Unit).LearnSkill(SpellInfo.MiscValue, Maximum: (short)((SpellInfo.valueBase + 1) * 75));
                        ((WS_PlayerData.CharacterObject)Unit).SendCharacterUpdate(false);
                    }
                }
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_DISPEL(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            foreach (WS_Base.BaseUnit Unit in Infected)
            {
                // TODO: Remove friendly or enemy spells depending on the reaction?
                if ((Unit.DispellImmunity & 1 << SpellInfo.MiscValue) == 0L)
                {
                    Unit.RemoveAurasByDispellType(SpellInfo.MiscValue, SpellInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0));
                }
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_EVADE(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            foreach (WS_Base.BaseUnit Unit in Infected)
            {
                // TODO: Evade
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_DODGE(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            foreach (WS_Base.BaseUnit Unit in Infected)
            {
                if (Unit is WS_PlayerData.CharacterObject)
                {
                    ((WS_PlayerData.CharacterObject)Unit).combatDodge += SpellInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                }
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_PARRY(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            foreach (WS_Base.BaseUnit Unit in Infected)
            {
                if (Unit is WS_PlayerData.CharacterObject)
                {
                    ((WS_PlayerData.CharacterObject)Unit).combatParry += SpellInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                }
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_BLOCK(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            foreach (WS_Base.BaseUnit Unit in Infected)
            {
                if (Unit is WS_PlayerData.CharacterObject)
                {
                    ((WS_PlayerData.CharacterObject)Unit).combatBlock += SpellInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                }
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_DUAL_WIELD(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            foreach (WS_Base.BaseUnit Unit in Infected)
            {
                if (Unit is WS_PlayerData.CharacterObject)
                {
                    ((WS_PlayerData.CharacterObject)Unit).spellCanDualWeild = true;
                }
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_WEAPON_DAMAGE(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            WS_Combat.DamageInfo damageInfo;
            bool Ranged = false;
            bool Offhand = false;
            if (SPELLs[SpellID].IsRanged)
            {
                Ranged = true;
            }

            foreach (WS_Base.BaseUnit Unit in Infected)
            {
                WS_Combat.DamageInfo localCalculateDamage() { WS_Base.BaseUnit argAttacker = (WS_Base.BaseUnit)Caster; var ret = WorldServiceLocator._WS_Combat.CalculateDamage(ref argAttacker, ref Unit, Offhand, Ranged, SPELLs[SpellID], SpellInfo); return ret; }

                damageInfo = localCalculateDamage();
                if (damageInfo.HitInfo & AttackHitState.HIT_RESIST)
                {
                    SPELLs[SpellID].SendSpellMiss(ref Caster, ref Unit, SpellMissInfo.SPELL_MISS_RESIST);
                }
                else if (damageInfo.HitInfo & AttackHitState.HIT_MISS)
                {
                    SPELLs[SpellID].SendSpellMiss(ref Caster, ref Unit, SpellMissInfo.SPELL_MISS_MISS);
                }
                else if (damageInfo.HitInfo & AttackHitState.HITINFO_ABSORB)
                {
                    SPELLs[SpellID].SendSpellMiss(ref Caster, ref Unit, SpellMissInfo.SPELL_MISS_ABSORB);
                }
                else if (damageInfo.HitInfo & AttackHitState.HITINFO_BLOCK)
                {
                    SPELLs[SpellID].SendSpellMiss(ref Caster, ref Unit, SpellMissInfo.SPELL_MISS_BLOCK);
                }
                else
                {
                    SendNonMeleeDamageLog(ref Caster, ref Unit, SpellID, damageInfo.DamageType, damageInfo.GetDamage, damageInfo.Resist, damageInfo.Absorbed, damageInfo.HitInfo & AttackHitState.HITINFO_CRITICALHIT);
                    WS_Base.BaseUnit argAttacker = (WS_Base.BaseUnit)Caster;
                    Unit.DealDamage(damageInfo.GetDamage, ref argAttacker);
                }
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_WEAPON_DAMAGE_NOSCHOOL(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            foreach (WS_Base.BaseUnit Unit in Infected)
            {
                if (Caster is WS_PlayerData.CharacterObject)
                {
                    WS_PlayerData.CharacterObject argCharacter = (WS_PlayerData.CharacterObject)Caster;
                    WS_Base.BaseObject argVictim2 = Unit;
                    ((WS_PlayerData.CharacterObject)Caster).attackState.DoMeleeDamageBySpell(ref argCharacter, ref argVictim2, SpellInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0), SpellID);
                }
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_ADICIONAL_DMG(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            WS_Combat.DamageInfo damageInfo;
            bool Ranged = false;
            bool Offhand = false;
            if (SPELLs[SpellID].IsRanged)
            {
                Ranged = true;
            }

            foreach (WS_Base.BaseUnit Unit in Infected)
            {
                WS_Combat.DamageInfo localCalculateDamage() { WS_Base.BaseUnit argAttacker = (WS_Base.BaseUnit)Caster; var ret = WorldServiceLocator._WS_Combat.CalculateDamage(ref argAttacker, ref Unit, Offhand, Ranged, SPELLs[SpellID], SpellInfo); return ret; }

                damageInfo = localCalculateDamage();
                SendNonMeleeDamageLog(ref Caster, ref Unit, SpellID, damageInfo.DamageType, damageInfo.GetDamage, damageInfo.Resist, damageInfo.Absorbed, damageInfo.HitInfo & AttackHitState.HITINFO_CRITICALHIT);
                WS_Base.BaseUnit argAttacker = (WS_Base.BaseUnit)Caster;
                Unit.DealDamage(damageInfo.GetDamage, ref argAttacker);
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_HONOR(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            foreach (WS_Base.BaseUnit Unit in Infected)
            {
                if (Unit is WS_PlayerData.CharacterObject)
                {
                    ((WS_PlayerData.CharacterObject)Unit).HonorPoints += SpellInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                    if (((WS_PlayerData.CharacterObject)Unit).HonorPoints > 75000)
                        ((WS_PlayerData.CharacterObject)Unit).HonorPoints = 75000;
                    ((WS_PlayerData.CharacterObject)Unit).HonorSave();
                    // CType(Unit, CharacterObject).SetUpdateFlag(EPlayerFields.PLAYER_FIELD_HONOR_CURRENCY, CType(Unit, CharacterObject).HonorCurrency)
                    // CType(Unit, CharacterObject).SendCharacterUpdate(False)
                }
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        private const int SLOT_NOT_FOUND = -1;
        private const int SLOT_CREATE_NEW = -2;
        private const int SLOT_NO_SPACE = int.MaxValue;

        public SpellFailedReason ApplyAura(ref WS_Base.BaseUnit auraTarget, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID)
        {
            try
            {
                int spellCasted = SLOT_NOT_FOUND;
                do
                {
                    // DONE: If active add to visible
                    // TODO: If positive effect add to upper part spells
                    int AuraStart = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1;
                    int AuraEnd = 0;

                    // DONE: Passives are not displayed
                    if (SPELLs[SpellID].IsPassive)
                    {
                        AuraStart = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs - 1;
                        AuraEnd = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE;
                    }

                    // DONE: Get spell duration
                    int Duration = SPELLs[SpellID].GetDuration;

                    // HACK: Set duration for Resurrection Sickness spell
                    if (SpellID == 15007)
                    {
                        switch (auraTarget.Level)
                        {
                            case var @case when @case < 11:
                                {
                                    Duration = 0;
                                    break;
                                }

                            case var case1 when case1 > 19:
                                {
                                    Duration = 10 * 60 * 1000;
                                    break;
                                }

                            default:
                                {
                                    Duration = (auraTarget.Level - 10) * 60 * 1000;
                                    break;
                                }
                        }
                    }

                    // DONE: Find spell aura slot
                    for (int i = AuraStart, loopTo = AuraEnd; i >= loopTo; i -= 1)
                    {
                        if (auraTarget.ActiveSpells[i] is object && auraTarget.ActiveSpells[i].SpellID == SpellID)
                        {
                            spellCasted = i;
                            if (auraTarget.ActiveSpells[i].Aura_Info[0] is object && ReferenceEquals(auraTarget.ActiveSpells[i].Aura_Info[0], SpellInfo) || auraTarget.ActiveSpells[i].Aura_Info[1] is object && ReferenceEquals(auraTarget.ActiveSpells[i].Aura_Info[1], SpellInfo) || auraTarget.ActiveSpells[i].Aura_Info[2] is object && ReferenceEquals(auraTarget.ActiveSpells[i].Aura_Info[2], SpellInfo))
                            {
                                if (auraTarget.ActiveSpells[i].Aura_Info[0] is object && ReferenceEquals(auraTarget.ActiveSpells[i].Aura_Info[0], SpellInfo))
                                {
                                    // DONE: Update the duration
                                    auraTarget.ActiveSpells[i].SpellDuration = Duration;
                                    // DONE: Update the stack if possible
                                    if (SPELLs[SpellID].maxStack > 0 && auraTarget.ActiveSpells[i].StackCount < SPELLs[SpellID].maxStack)
                                    {
                                        auraTarget.ActiveSpells[i].StackCount += 1;
                                        AURAs[SpellInfo.ApplyAuraIndex].Invoke(ref auraTarget, ref Caster, ref SpellInfo, SpellID, 1, AuraAction.AURA_ADD);
                                        if (auraTarget is WS_PlayerData.CharacterObject)
                                        {
                                            ((WS_PlayerData.CharacterObject)auraTarget).GroupUpdateFlag = ((WS_PlayerData.CharacterObject)auraTarget).GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_AURAS;
                                        }
                                        else if (auraTarget is WS_Pets.PetObject && ((WS_Pets.PetObject)auraTarget).Owner is WS_PlayerData.CharacterObject)
                                        {
                                            ((WS_PlayerData.CharacterObject)((WS_Pets.PetObject)auraTarget).Owner).GroupUpdateFlag = ((WS_PlayerData.CharacterObject)((WS_Pets.PetObject)auraTarget).Owner).GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_PET_AURAS;
                                        }
                                    }

                                    auraTarget.UpdateAura(i);
                                }

                                return SpellFailedReason.SPELL_NO_ERROR;
                            }
                            else if (auraTarget.ActiveSpells[i].Aura[0] is null)
                            {
                                auraTarget.ActiveSpells[i].Aura[0] = AURAs[SpellInfo.ApplyAuraIndex];
                                auraTarget.ActiveSpells[i].Aura_Info[0] = SpellInfo;
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "APPLYING AURA {0}", (AuraEffects_Names)SpellInfo.ApplyAuraIndex);
                                break;
                            }
                            else if (auraTarget.ActiveSpells[i].Aura[1] is null)
                            {
                                auraTarget.ActiveSpells[i].Aura[1] = AURAs[SpellInfo.ApplyAuraIndex];
                                auraTarget.ActiveSpells[i].Aura_Info[1] = SpellInfo;
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "APPLYING AURA {0}", (AuraEffects_Names)SpellInfo.ApplyAuraIndex);
                                break;
                            }
                            else if (auraTarget.ActiveSpells[i].Aura[2] is null)
                            {
                                auraTarget.ActiveSpells[i].Aura[2] = AURAs[SpellInfo.ApplyAuraIndex];
                                auraTarget.ActiveSpells[i].Aura_Info[2] = SpellInfo;
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "APPLYING AURA {0}", (AuraEffects_Names)SpellInfo.ApplyAuraIndex);
                                break;
                            }
                            else
                            {
                                spellCasted = SLOT_NO_SPACE;
                            }
                        }
                    }

                    // DONE: Not found same active aura on that player, create new
                    if (spellCasted == SLOT_NOT_FOUND)
                    {
                        WS_Base.BaseUnit argCaster = (WS_Base.BaseUnit)Caster;
                        auraTarget.AddAura(SpellID, Duration, ref argCaster);
                    }

                    if (spellCasted == SLOT_CREATE_NEW)
                        spellCasted = SLOT_NO_SPACE;
                    if (spellCasted < 0)
                        spellCasted -= 1;
                }
                while (spellCasted < 0);

                // DONE: No more space for auras
                if (spellCasted == SLOT_NO_SPACE)
                    return SpellFailedReason.SPELL_FAILED_TRY_AGAIN;

                // DONE: Cast the aura
                AURAs[SpellInfo.ApplyAuraIndex].Invoke(ref auraTarget, ref Caster, ref SpellInfo, SpellID, 1, AuraAction.AURA_ADD);
            }
            catch (Exception e)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error while applying aura for spell {0}:{1}", SpellID, Environment.NewLine + e.ToString());
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_APPLY_AURA(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            if ((Target.targetMask & SpellCastTargetFlags.TARGET_FLAG_UNIT || Target.targetMask == SpellCastTargetFlags.TARGET_FLAG_SELF) && Target.unitTarget is null)
                return SpellFailedReason.SPELL_FAILED_BAD_IMPLICIT_TARGETS;
            SpellFailedReason result = SpellFailedReason.SPELL_NO_ERROR;

            // DONE: Sit down on some spells
            if (Caster is WS_PlayerData.CharacterObject && SPELLs[SpellID].auraInterruptFlags & SpellAuraInterruptFlags.AURA_INTERRUPT_FLAG_NOT_SEATED)
            {
                ((WS_Base.BaseUnit)Caster).StandState = StandStates.STANDSTATE_SIT;
                ((WS_PlayerData.CharacterObject)Caster).SetUpdateFlag(EUnitFields.UNIT_FIELD_BYTES_1, ((WS_Base.BaseUnit)Caster).cBytes1);
                ((WS_PlayerData.CharacterObject)Caster).SendCharacterUpdate(true);
                var packetACK = new Packets.PacketClass(OPCODES.SMSG_STANDSTATE_CHANGE_ACK);
                packetACK.AddInt8(((WS_Base.BaseUnit)Caster).StandState);
                ((WS_PlayerData.CharacterObject)Caster).client.Send(ref packetACK);
                packetACK.Dispose();
            }

            if (Target.targetMask & SpellCastTargetFlags.TARGET_FLAG_UNIT || Target.targetMask == SpellCastTargetFlags.TARGET_FLAG_SELF)
            {
                int count = SPELLs[SpellID].MaxTargets;
                foreach (WS_Base.BaseUnit u in Infected)
                {
                    ApplyAura(ref u, ref Caster, ref SpellInfo, SpellID);
                    count -= 1;
                    if (count <= 0 && SPELLs[SpellID].MaxTargets > 0)
                        break;
                }
            }
            else if (Target.targetMask & SpellCastTargetFlags.TARGET_FLAG_DEST_LOCATION)
            {
                foreach (WS_DynamicObjects.DynamicObjectObject dynamic in ((WS_Base.BaseUnit)Caster).dynamicObjects.ToArray())
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
            foreach (WS_Base.BaseUnit u in Infected)
                ApplyAura(ref u, ref Caster, ref SpellInfo, SpellID);
            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_PERSISTENT_AREA_AURA(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            if ((Target.targetMask & SpellCastTargetFlags.TARGET_FLAG_DEST_LOCATION) == 0)
                return SpellFailedReason.SPELL_FAILED_BAD_IMPLICIT_TARGETS;
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Amplitude: {0}", SpellInfo.Amplitude);
            WS_Base.BaseUnit argCaster_ = (WS_Base.BaseUnit)Caster;
            var tmpDO = new WS_DynamicObjects.DynamicObjectObject(ref argCaster_, SpellID, Target.dstX, Target.dstY, Target.dstZ, SPELLs[SpellID].GetDuration, SpellInfo.GetRadius);
            tmpDO.AddEffect(SpellInfo);
            tmpDO.Bytes = 0x1EEEEEE;
            ((WS_Base.BaseUnit)Caster).dynamicObjects.Add(tmpDO);
            tmpDO.Spawn();
            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_CREATE_ITEM(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            if (!(Target.unitTarget is WS_PlayerData.CharacterObject))
                return SpellFailedReason.SPELL_FAILED_BAD_TARGETS;
            int Amount = SpellInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level - SPELLs[SpellID].spellLevel, 0);
            if (Amount < 0)
                return SpellFailedReason.SPELL_FAILED_ERROR;
            if (WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(SpellInfo.ItemType) == false)
            {
                var tmpInfo = new WS_Items.ItemInfo(SpellInfo.ItemType);
                WorldServiceLocator._WorldServer.ITEMDatabase.Add(SpellInfo.ItemType, tmpInfo);
            }

            if (Amount > WorldServiceLocator._WorldServer.ITEMDatabase[SpellInfo.ItemType].Stackable)
                Amount = WorldServiceLocator._WorldServer.ITEMDatabase[SpellInfo.ItemType].Stackable;
            WS_Base.BaseUnit argobjCharacter = (WS_Base.BaseUnit)Caster;
            var Targets = GetFriendPlayersAroundMe(ref argobjCharacter, SpellInfo.GetRadius);
            foreach (WS_Base.BaseUnit Unit in Infected)
            {
                if (Unit is WS_PlayerData.CharacterObject)
                {
                    var tmpItem = new ItemObject(SpellInfo.ItemType, Unit.GUID) { StackCount = Amount };
                    if (!((WS_PlayerData.CharacterObject)Unit).ItemADD(ref tmpItem))
                    {
                        tmpItem.Delete();
                    }
                    else
                    {
                        ((WS_PlayerData.CharacterObject)Target.unitTarget).LogLootItem(tmpItem, (byte)tmpItem.StackCount, false, true);
                    }
                }
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_RESURRECT(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            foreach (WS_Base.BaseObject Unit in Infected)
            {
                if (Unit is WS_PlayerData.CharacterObject)
                {
                    // DONE: Character has already been requested a resurrect
                    if (((WS_PlayerData.CharacterObject)Unit).resurrectGUID != 0m)
                    {
                        if (Caster is WS_PlayerData.CharacterObject)
                        {
                            var RessurectFailed = new Packets.PacketClass(OPCODES.SMSG_RESURRECT_FAILED);
                            ((WS_PlayerData.CharacterObject)Caster).client.Send(ref RessurectFailed);
                            RessurectFailed.Dispose();
                        }

                        return SpellFailedReason.SPELL_NO_ERROR;

                        // DONE: Save resurrection data
                    } ((WS_PlayerData.CharacterObject)Unit).resurrectGUID = Caster.GUID;
                    ((WS_PlayerData.CharacterObject)Unit).resurrectMapID = (int)Caster.MapID;
                    ((WS_PlayerData.CharacterObject)Unit).resurrectPositionX = Caster.positionX;
                    ((WS_PlayerData.CharacterObject)Unit).resurrectPositionY = Caster.positionY;
                    ((WS_PlayerData.CharacterObject)Unit).resurrectPositionZ = Caster.positionZ;
                    ((WS_PlayerData.CharacterObject)Unit).resurrectHealth = ((WS_PlayerData.CharacterObject)Unit).Life.Maximum * SpellInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100;
                    ((WS_PlayerData.CharacterObject)Unit).resurrectMana = ((WS_PlayerData.CharacterObject)Unit).Mana.Maximum * SpellInfo.MiscValue / 100;

                    // DONE: Send a resurrection request
                    var RessurectRequest = new Packets.PacketClass(OPCODES.SMSG_RESURRECT_REQUEST);
                    RessurectRequest.AddUInt64(Caster.GUID);
                    RessurectRequest.AddUInt32(1U);
                    RessurectRequest.AddUInt16(0);
                    RessurectRequest.AddUInt32(1U);
                    ((WS_PlayerData.CharacterObject)Unit).client.Send(ref RessurectRequest);
                    RessurectRequest.Dispose();
                }
                else if (Unit is WS_Creatures.CreatureObject)
                {
                    // DONE: Ressurect pets
                    Target.unitTarget.Life.Current = ((WS_Creatures.CreatureObject)Unit).Life.Maximum * SpellInfo.valueBase / 100;
                    Target.unitTarget.cUnitFlags = Target.unitTarget.cUnitFlags & !UnitFlags.UNIT_FLAG_DEAD;
                    var packetForNear = new Packets.UpdatePacketClass();
                    var UpdateData = new Packets.UpdateClass(EUnitFields.UNIT_END);
                    UpdateData.SetUpdateFlag(EUnitFields.UNIT_FIELD_HEALTH, ((WS_Creatures.CreatureObject)Unit).Life.Current);
                    UpdateData.SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, Target.unitTarget.cUnitFlags);
                    UpdateData.AddToPacket(packetForNear, ObjectUpdateType.UPDATETYPE_VALUES, (WS_Creatures.CreatureObject)Unit);
                    Packets.PacketClass argpacket = packetForNear;
                    ((WS_Creatures.CreatureObject)Unit).SendToNearPlayers(ref argpacket);
                    packetForNear.Dispose();
                    UpdateData.Dispose();
                    ((WS_Creatures.CreatureObject)Target.unitTarget).MoveToInstant(Caster.positionX, Caster.positionY, Caster.positionZ, Caster.orientation);
                }
                else if (Unit is WS_Corpses.CorpseObject)
                {
                    if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(((WS_Corpses.CorpseObject)Unit).Owner))
                    {
                        // DONE: Save resurrection data
                        {
                            var withBlock = WorldServiceLocator._WorldServer.CHARACTERs[((WS_Corpses.CorpseObject)Unit).Owner];
                            withBlock.resurrectGUID = Caster.GUID;
                            withBlock.resurrectMapID = (int)Caster.MapID;
                            withBlock.resurrectPositionX = Caster.positionX;
                            withBlock.resurrectPositionY = Caster.positionY;
                            withBlock.resurrectPositionZ = Caster.positionZ;
                            withBlock.resurrectHealth = withBlock.Life.Maximum * SpellInfo.valueBase / 100;
                            withBlock.resurrectMana = withBlock.Mana.Maximum * SpellInfo.MiscValue / 100;

                            // DONE: Send request to corpse owner
                            var RessurectRequest = new Packets.PacketClass(OPCODES.SMSG_RESURRECT_REQUEST);
                            RessurectRequest.AddUInt64(Caster.GUID);
                            RessurectRequest.AddUInt32(1U);
                            RessurectRequest.AddUInt16(0);
                            RessurectRequest.AddUInt32(1U);
                            withBlock.client.Send(ref RessurectRequest);
                            RessurectRequest.Dispose();
                        }
                    }
                }
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_RESURRECT_NEW(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            foreach (WS_Base.BaseObject Unit in Infected)
            {
                if (Unit is WS_PlayerData.CharacterObject)
                {
                    // DONE: Character has already been requested a resurrect
                    if (((WS_PlayerData.CharacterObject)Unit).resurrectGUID != 0m)
                    {
                        if (Caster is WS_PlayerData.CharacterObject)
                        {
                            var RessurectFailed = new Packets.PacketClass(OPCODES.SMSG_RESURRECT_FAILED);
                            ((WS_PlayerData.CharacterObject)Caster).client.Send(ref RessurectFailed);
                            RessurectFailed.Dispose();
                        }

                        return SpellFailedReason.SPELL_NO_ERROR;

                        // DONE: Save resurrection data
                    } ((WS_PlayerData.CharacterObject)Unit).resurrectGUID = Caster.GUID;
                    ((WS_PlayerData.CharacterObject)Unit).resurrectMapID = (int)Caster.MapID;
                    ((WS_PlayerData.CharacterObject)Unit).resurrectPositionX = Caster.positionX;
                    ((WS_PlayerData.CharacterObject)Unit).resurrectPositionY = Caster.positionY;
                    ((WS_PlayerData.CharacterObject)Unit).resurrectPositionZ = Caster.positionZ;
                    ((WS_PlayerData.CharacterObject)Unit).resurrectHealth = SpellInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                    ((WS_PlayerData.CharacterObject)Unit).resurrectMana = SpellInfo.MiscValue;

                    // DONE: Send a resurrection request
                    var RessurectRequest = new Packets.PacketClass(OPCODES.SMSG_RESURRECT_REQUEST);
                    RessurectRequest.AddUInt64(Caster.GUID);
                    RessurectRequest.AddUInt32(1U);
                    RessurectRequest.AddUInt16(0);
                    RessurectRequest.AddUInt32(1U);
                    ((WS_PlayerData.CharacterObject)Unit).client.Send(ref RessurectRequest);
                    RessurectRequest.Dispose();
                }
                else if (Unit is WS_Creatures.CreatureObject)
                {
                    // DONE: Ressurect pets
                    ((WS_Creatures.CreatureObject)Unit).Life.Current = ((WS_Creatures.CreatureObject)Unit).Life.Maximum * SpellInfo.valueBase / 100;
                    var packetForNear = new Packets.UpdatePacketClass();
                    var UpdateData = new Packets.UpdateClass(EUnitFields.UNIT_END);
                    UpdateData.SetUpdateFlag(EUnitFields.UNIT_FIELD_HEALTH, ((WS_Creatures.CreatureObject)Unit).Life.Current);
                    UpdateData.AddToPacket(packetForNear, ObjectUpdateType.UPDATETYPE_VALUES, (WS_Creatures.CreatureObject)Unit);
                    Packets.PacketClass argpacket = packetForNear;
                    ((WS_Creatures.CreatureObject)Unit).SendToNearPlayers(ref argpacket);
                    packetForNear.Dispose();
                    UpdateData.Dispose();
                    ((WS_Creatures.CreatureObject)Target.unitTarget).MoveToInstant(Caster.positionX, Caster.positionY, Caster.positionZ, Caster.orientation);
                }
                else if (Unit is WS_Corpses.CorpseObject)
                {
                    if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(((WS_Corpses.CorpseObject)Unit).Owner))
                    {
                        // DONE: Save resurrection data
                        WorldServiceLocator._WorldServer.CHARACTERs[((WS_Corpses.CorpseObject)Unit).Owner].resurrectGUID = Caster.GUID;
                        WorldServiceLocator._WorldServer.CHARACTERs[((WS_Corpses.CorpseObject)Unit).Owner].resurrectMapID = (int)Caster.MapID;
                        WorldServiceLocator._WorldServer.CHARACTERs[((WS_Corpses.CorpseObject)Unit).Owner].resurrectPositionX = Caster.positionX;
                        WorldServiceLocator._WorldServer.CHARACTERs[((WS_Corpses.CorpseObject)Unit).Owner].resurrectPositionY = Caster.positionY;
                        WorldServiceLocator._WorldServer.CHARACTERs[((WS_Corpses.CorpseObject)Unit).Owner].resurrectPositionZ = Caster.positionZ;
                        WorldServiceLocator._WorldServer.CHARACTERs[((WS_Corpses.CorpseObject)Unit).Owner].resurrectHealth = SpellInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                        WorldServiceLocator._WorldServer.CHARACTERs[((WS_Corpses.CorpseObject)Unit).Owner].resurrectMana = SpellInfo.MiscValue;

                        // DONE: Send request to corpse owner
                        var RessurectRequest = new Packets.PacketClass(OPCODES.SMSG_RESURRECT_REQUEST);
                        RessurectRequest.AddUInt64(Caster.GUID);
                        RessurectRequest.AddUInt32(1U);
                        RessurectRequest.AddUInt16(0);
                        RessurectRequest.AddUInt32(1U);
                        WorldServiceLocator._WorldServer.CHARACTERs[((WS_Corpses.CorpseObject)Unit).Owner].client.Send(ref RessurectRequest);
                        RessurectRequest.Dispose();
                    }
                }
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_TELEPORT_GRAVEYARD(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            foreach (WS_Base.BaseUnit Unit in Infected)
            {
                if (Unit is WS_PlayerData.CharacterObject)
                {
                    WS_PlayerData.CharacterObject argCharacter = (WS_PlayerData.CharacterObject)Unit;
                    WorldServiceLocator._WorldServer.AllGraveYards.GoToNearestGraveyard(ref argCharacter, false, true);
                }
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_INTERRUPT_CAST(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            foreach (WS_Base.BaseUnit Unit in Infected)
            {
                if (Unit is WS_PlayerData.CharacterObject)
                {
                    if (((WS_PlayerData.CharacterObject)Unit).FinishAllSpells(false))
                    {
                        ((WS_PlayerData.CharacterObject)Unit).ProhibitSpellSchool(SPELLs[SpellID].School, SPELLs[SpellID].GetDuration);
                    }
                }
                else if (Unit is WS_Creatures.CreatureObject)
                {
                    ((WS_Creatures.CreatureObject)Unit).StopCasting();
                    // TODO: Prohibit spell school as with creatures
                }
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_STEALTH(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            foreach (WS_Base.BaseUnit Unit in Infected)
            {
                uint argvalue = (uint)Unit.cBytes1;
                WorldServiceLocator._Functions.SetFlag(ref argvalue, 25, true);
                Unit.Invisibility = InvisibilityLevel.INIVISIBILITY;
                Unit.Invisibility_Value = SpellInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                if (Unit is WS_PlayerData.CharacterObject)
                {
                    WS_PlayerData.CharacterObject argCharacter = (WS_PlayerData.CharacterObject)Unit;
                    WorldServiceLocator._WS_CharMovement.UpdateCell(ref argCharacter);
                }
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_DETECT(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            foreach (WS_Base.BaseUnit Unit in Infected)
            {
                Unit.CanSeeInvisibility = InvisibilityLevel.INIVISIBILITY;
                Unit.CanSeeInvisibility_Stealth = SpellInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                if (Unit is WS_PlayerData.CharacterObject)
                {
                    WS_PlayerData.CharacterObject argCharacter = (WS_PlayerData.CharacterObject)Unit;
                    WorldServiceLocator._WS_CharMovement.UpdateCell(ref argCharacter);
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
                // DONE: Special case if caster is above the ground
                selectedX = Caster.positionX;
                selectedY = Caster.positionY;
                selectedZ = Caster.positionZ - SpellInfo.GetRadius;
            }

            // DONE: Check if we hit something
            var hitX = default(float);
            var hitY = default(float);
            var hitZ = default(float);
            if (WorldServiceLocator._WS_Maps.GetObjectHitPos(ref Caster, selectedX, selectedY, selectedZ + 2.0f, ref hitX, ref hitY, ref hitZ, -1.0f))
            {
                selectedX = hitX;
                selectedY = hitY;
                selectedZ = hitZ + 0.2f;
            }

            if (Caster is WS_PlayerData.CharacterObject)
            {
                ((WS_PlayerData.CharacterObject)Caster).Teleport(selectedX, selectedY, selectedZ, Caster.orientation, (int)Caster.MapID);
            }
            else
            {
                ((WS_Creatures.CreatureObject)Caster).MoveToInstant(selectedX, selectedY, selectedZ, Caster.orientation);
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_SUMMON(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            // Select Case SpellInfo.MiscValueB
            // Case SummonType.SUMMON_TYPE_GUARDIAN, SummonType.SUMMON_TYPE_POSESSED, SummonType.SUMMON_TYPE_POSESSED2
            // _WorldServer.Log.WriteLine(LogType.DEBUG, "[DEBUG] Summon Guardian")
            // Case SummonType.SUMMON_TYPE_WILD
            // Dim Duration As Integer = SPELLs(SpellID).GetDuration
            // Dim Type As TempSummonType = TempSummonType.TEMPSUMMON_TIMED_OR_DEAD_DESPAWN
            // If Duration = 0 Then Type = TempSummonType.TEMPSUMMON_DEAD_DESPAWN

            // Dim SelectedX As Single, SelectedY As Single, SelectedZ As Single
            // If (Target.targetMask And SpellCastTargetFlags.TARGET_FLAG_DEST_LOCATION) Then
            // SelectedX = Target.dstX
            // SelectedY = Target.dstY
            // SelectedZ = Target.dstZ
            // Else
            // SelectedX = Caster.positionX
            // SelectedY = Caster.positionY
            // SelectedZ = Caster.positionZ
            // End If

            // Dim tmpCreature As New CreatureObject(SpellInfo.MiscValue, SelectedX, SelectedY, SelectedZ, Caster.orientation, Caster.MapID, Duration)
            // 'TODO: Level by engineering skill level
            // tmpCreature.Level = CType(Caster, BaseUnit).Level
            // tmpCreature.CreatedBy = Caster.GUID
            // tmpCreature.CreatedBySpell = SpellID
            // tmpCreature.AddToWorld()
            // Case SummonType.SUMMON_TYPE_DEMON
            // _WorldServer.Log.WriteLine(LogType.DEBUG, "[DEBUG] Summon Demon")
            // Case SummonType.SUMMON_TYPE_SUMMON
            // _WorldServer.Log.WriteLine(LogType.DEBUG, "[DEBUG] Summon")
            // Case SummonType.SUMMON_TYPE_CRITTER, SummonType.SUMMON_TYPE_CRITTER2
            // If CType(Caster, CharacterObject).NonCombatPet IsNot Nothing AndAlso CType(Caster, CharacterObject).NonCombatPet.ID = SpellInfo.MiscValue Then
            // CType(Caster, CharacterObject).NonCombatPet.Destroy()
            // CType(Caster, CharacterObject).NonCombatPet = Nothing
            // Return SpellFailedReason.SPELL_NO_ERROR
            // End If
            // If CType(Caster, CharacterObject).NonCombatPet IsNot Nothing Then
            // CType(Caster, CharacterObject).NonCombatPet.Destroy()
            // End If

            // Dim SelectedX As Single, SelectedY As Single, SelectedZ As Single
            // If (Target.targetMask And SpellCastTargetFlags.TARGET_FLAG_DEST_LOCATION) Then
            // SelectedX = Target.dstX
            // SelectedY = Target.dstY
            // SelectedZ = Target.dstZ
            // Else
            // SelectedX = Caster.positionX
            // SelectedY = Caster.positionY
            // SelectedZ = Caster.positionZ
            // End If
            // CType(Caster, CharacterObject).NonCombatPet = New CreatureObject(SpellInfo.MiscValue, SelectedX, SelectedY, SelectedZ, Caster.orientation, Caster.MapID, SPELLs(SpellID).GetDuration)
            // CType(Caster, CharacterObject).NonCombatPet.SummonedBy = Caster.GUID
            // CType(Caster, CharacterObject).NonCombatPet.CreatedBy = Caster.GUID
            // CType(Caster, CharacterObject).NonCombatPet.CreatedBySpell = SpellID
            // CType(Caster, CharacterObject).NonCombatPet.Faction = CType(Caster, CharacterObject).Faction
            // CType(Caster, CharacterObject).NonCombatPet.Level = 1
            // CType(Caster, CharacterObject).NonCombatPet.Life.Base = 1
            // CType(Caster, CharacterObject).NonCombatPet.Life.Current = 1
            // CType(Caster, CharacterObject).NonCombatPet.AddToWorld()

            // Case SummonType.SUMMON_TYPE_TOTEM, SummonType.SUMMON_TYPE_TOTEM_SLOT1, SummonType.SUMMON_TYPE_TOTEM_SLOT2, SummonType.SUMMON_TYPE_TOTEM_SLOT3, SummonType.SUMMON_TYPE_TOTEM_SLOT4
            // Dim Slot As Byte = 0
            // Select Case SpellInfo.MiscValueB
            // Case SummonType.SUMMON_TYPE_TOTEM_SLOT1
            // Slot = 0
            // Case SummonType.SUMMON_TYPE_TOTEM_SLOT2
            // Slot = 1
            // Case SummonType.SUMMON_TYPE_TOTEM_SLOT3
            // Slot = 2
            // Case SummonType.SUMMON_TYPE_TOTEM_SLOT4
            // Slot = 3
            // Case SummonType.SUMMON_TYPE_TOTEM
            // Slot = 254
            // Case SummonType.SUMMON_TYPE_GUARDIAN
            // Slot = 255
            // End Select

            // _WorldServer.Log.WriteLine(LogType.DEBUG, "[DEBUG] Totem Slot [{0}].", Slot)

            // 'Normal shaman totem
            // If Slot < 4 Then
            // Dim GUID As ULong = CType(Caster, CharacterObject).TotemSlot(Slot)
            // If GUID <> 0 Then
            // If _WorldServer.WORLD_CREATUREs.ContainsKey(GUID) Then
            // _WorldServer.Log.WriteLine(LogType.DEBUG, "[DEBUG] Destroyed old totem.")
            // _WorldServer.WORLD_CREATUREs(GUID).Destroy()
            // End If
            // End If
            // End If

            // Dim angle As Single = 0
            // If Slot < 4 Then angle = Math.PI / 4 - (Slot * 2 * Math.PI / 4)

            // Dim selectedX As Single = Caster.positionX + Math.Cos(Caster.orientation) * 2
            // Dim selectedY As Single = Caster.positionY + Math.Sin(Caster.orientation) * 2
            // Dim selectedZ As Single = GetZCoord(selectedX, selectedY, Caster.positionZ, Caster.MapID)
            // If Math.Abs(Caster.positionZ - selectedZ) > 5 Then selectedZ = Caster.positionZ

            // Dim NewTotem As New TotemObject(SpellInfo.MiscValue, selectedX, selectedY, selectedZ, angle, Caster.MapID, SPELLs(SpellID).GetDuration)
            // NewTotem.Life.Base = SpellInfo.GetValue(CType(Caster, BaseUnit).Level)
            // NewTotem.Life.Current = NewTotem.Life.Maximum
            // NewTotem.Caster = Caster
            // NewTotem.Level = CType(Caster, BaseUnit).Level
            // NewTotem.SummonedBy = Caster.GUID
            // NewTotem.CreatedBy = Caster.GUID
            // NewTotem.CreatedBySpell = SpellID
            // If TypeOf Caster Is CharacterObject Then
            // NewTotem.Faction = CType(Caster, CharacterObject).Faction
            // ElseIf TypeOf Caster Is CreatureObject Then
            // NewTotem.Faction = CType(Caster, CreatureObject).Faction
            // End If
            // Select Case SpellID
            // Case 25547
            // NewTotem.InitSpell(25539)
            // Case 25359
            // NewTotem.InitSpell(25360)
            // Case 2484
            // NewTotem.InitSpell(6474)
            // Case 8170
            // NewTotem.InitSpell(8172)
            // Case 8166
            // NewTotem.InitSpell(8179)
            // Case 8177
            // NewTotem.InitSpell(8167)
            // Case 5675
            // NewTotem.InitSpell(5677)
            // Case 10495
            // NewTotem.InitSpell(10491)
            // Case 10496
            // NewTotem.InitSpell(10493)
            // Case 10497
            // NewTotem.InitSpell(10494)
            // Case 25570
            // NewTotem.InitSpell(25569)
            // Case 25552
            // NewTotem.InitSpell(25551)
            // Case 25587
            // NewTotem.InitSpell(25582)
            // Case 16190
            // NewTotem.InitSpell(16191)
            // Case 25528
            // NewTotem.InitSpell(25527)
            // Case 8143
            // NewTotem.InitSpell(8145)
            // End Select
            // NewTotem.AddToWorld()
            // _WorldServer.Log.WriteLine(LogType.DEBUG, "[DEBUG] Totem spawned [{0:X}].", NewTotem.GUID)

            // If Slot < 4 AndAlso TypeOf Caster Is CharacterObject Then
            // CType(Caster, CharacterObject).TotemSlot(Slot) = NewTotem.GUID

            // 'Dim TotemCreated As New PacketClass(OPCODES.SMSG_TOTEM_CREATED)
            // 'TotemCreated.AddInt8(Slot)
            // 'TotemCreated.AddUInt64(NewTotem.GUID)
            // 'TotemCreated.AddInt32(SPELLs(SpellID).GetDuration)
            // 'TotemCreated.AddInt32(SpellID)
            // 'CType(Caster, CharacterObject).Client.Send(TotemCreated)
            // 'TotemCreated.Dispose()
            // End If
            // Case Else
            // _WorldServer.Log.WriteLine(LogType.DEBUG, "Unknown SummonType: {0}", SpellInfo.MiscValueB)
            // Exit Function
            // End Select

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
                if (Target.targetMask & SpellCastTargetFlags.TARGET_FLAG_DEST_LOCATION)
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

                // TODO: Level by engineering skill level
                var tmpCreature = new WS_Creatures.CreatureObject(SpellInfo.MiscValue, SelectedX, SelectedY, SelectedZ, Caster.orientation, (int)Caster.MapID, Duration)
                {
                    Level = ((WS_Base.BaseUnit)Caster).Level,
                    CreatedBy = Caster.GUID,
                    CreatedBySpell = SpellID
                };
                tmpCreature.AddToWorld();
                return SpellFailedReason.SPELL_NO_ERROR;
            }

            return default;
        }

        public SpellFailedReason SPELL_EFFECT_SUMMON_TOTEM(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            byte Slot;
            switch (SpellInfo.ID)
            {
                case var @case when @case == SpellEffects_Names.SPELL_EFFECT_SUMMON_TOTEM_SLOT1:
                    {
                        Slot = 0;
                        break;
                    }

                case var case1 when case1 == SpellEffects_Names.SPELL_EFFECT_SUMMON_TOTEM_SLOT2:
                    {
                        Slot = 1;
                        break;
                    }

                case var case2 when case2 == SpellEffects_Names.SPELL_EFFECT_SUMMON_TOTEM_SLOT3:
                    {
                        Slot = 2;
                        break;
                    }

                case var case3 when case3 == SpellEffects_Names.SPELL_EFFECT_SUMMON_TOTEM_SLOT4:
                    {
                        Slot = 3;
                        break;
                    }

                default:
                    {
                        return default;
                    }
            }

            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[DEBUG] Totem Slot [{0}].", Slot);

            // Normal shaman totem
            if (Slot < 4)
            {
                ulong GUID = ((WS_PlayerData.CharacterObject)Caster).TotemSlot[Slot];
                if (GUID != 0m)
                {
                    if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(GUID))
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[DEBUG] Destroyed old totem.");
                        WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].Destroy();
                    }
                }
            }

            float angle = 0f;
            if (Slot < 4)
                angle = (float)(Math.PI / 4d - Slot * 2 * Math.PI / 4d);
            float selectedX = (float)(Caster.positionX + Math.Cos(Caster.orientation) * 2d);
            float selectedY = (float)(Caster.positionY + Math.Sin(Caster.orientation) * 2d);
            float selectedZ = WorldServiceLocator._WS_Maps.GetZCoord(selectedX, selectedY, Caster.positionZ, Caster.MapID);
            var NewTotem = new WS_Totems.TotemObject(SpellInfo.MiscValue, selectedX, selectedY, selectedZ, angle, (int)Caster.MapID, SPELLs[SpellID].GetDuration);
            NewTotem.Life.Base = SpellInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
            NewTotem.Life.Current = NewTotem.Life.Maximum;
            NewTotem.Caster = (WS_Base.BaseUnit)Caster;
            NewTotem.Level = ((WS_Base.BaseUnit)Caster).Level;
            NewTotem.SummonedBy = Caster.GUID;
            NewTotem.CreatedBy = Caster.GUID;
            NewTotem.CreatedBySpell = SpellID;
            if (Caster is WS_PlayerData.CharacterObject)
            {
                NewTotem.Faction = ((WS_PlayerData.CharacterObject)Caster).Faction;
            }
            else if (Caster is WS_Creatures.CreatureObject)
            {
                NewTotem.Faction = ((WS_Creatures.CreatureObject)Caster).Faction;
            }

            if (WorldServiceLocator._WorldServer.CREATURESDatabase.ContainsKey(SpellInfo.MiscValue) == false)
            {
                var tmpInfo = new CreatureInfo(SpellInfo.MiscValue);
                WorldServiceLocator._WorldServer.CREATURESDatabase.Add(SpellInfo.MiscValue, tmpInfo);
            }

            NewTotem.InitSpell(WorldServiceLocator._WorldServer.CREATURESDatabase[SpellInfo.MiscValue].Spells[0]);
            NewTotem.AddToWorld();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[DEBUG] Totem spawned [{0:X}].", NewTotem.GUID);
            if (Slot < 4 && Caster is WS_PlayerData.CharacterObject)
            {
                ((WS_PlayerData.CharacterObject)Caster).TotemSlot[Slot] = NewTotem.GUID;

                // Dim TotemCreated As New PacketClass(OPCODES.SMSG_TOTEM_CREATED)
                // TotemCreated.AddInt8(Slot)
                // TotemCreated.AddUInt64(NewTotem.GUID)
                // TotemCreated.AddInt32(SPELLs(SpellID).GetDuration)
                // TotemCreated.AddInt32(SpellID)
                // CType(Caster, CharacterObject).Client.Send(TotemCreated)
                // TotemCreated.Dispose()
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_SUMMON_OBJECT(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            if (!(Caster is WS_Base.BaseUnit))
                return SpellFailedReason.SPELL_FAILED_CASTER_DEAD;
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

            WS_GameObjects.GameObjectInfo GameobjectInfo;
            if (WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase.ContainsKey(SpellInfo.MiscValue) == false)
            {
                GameobjectInfo = new WS_GameObjects.GameObjectInfo(SpellInfo.MiscValue);
            }
            else
            {
                GameobjectInfo = WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase[SpellInfo.MiscValue];
            }

            float selectedZ;
            if (GameobjectInfo.Type == GameObjectType.GAMEOBJECT_TYPE_FISHINGNODE)
            {
                selectedZ = WorldServiceLocator._WS_Maps.GetWaterLevel(selectedX, selectedY, (int)Caster.MapID);
            }
            else
            {
                selectedZ = WorldServiceLocator._WS_Maps.GetZCoord(selectedX, selectedY, Caster.positionZ, Caster.MapID);
            }

            var tmpGO = new WS_GameObjects.GameObjectObject(SpellInfo.MiscValue, Caster.MapID, selectedX, selectedY, selectedZ, Caster.orientation, Caster.GUID)
            {
                CreatedBySpell = SpellID,
                Level = ((WS_Base.BaseUnit)Caster).Level,
                instance = Caster.instance
            };
            ((WS_Base.BaseUnit)Caster).gameObjects.Add(tmpGO);
            if (GameobjectInfo.Type == GameObjectType.GAMEOBJECT_TYPE_FISHINGNODE)
            {
                tmpGO.SetupFishingNode();
            }

            tmpGO.AddToWorld();
            var packet = new Packets.PacketClass(OPCODES.SMSG_GAMEOBJECT_SPAWN_ANIM);
            packet.AddUInt64(tmpGO.GUID);
            tmpGO.SendToNearPlayers(ref packet);
            packet.Dispose();
            if (GameobjectInfo.Type == GameObjectType.GAMEOBJECT_TYPE_FISHINGNODE)
            {
                ((WS_PlayerData.CharacterObject)Caster).SetUpdateFlag(EUnitFields.UNIT_CHANNEL_SPELL, SpellID);
                ((WS_PlayerData.CharacterObject)Caster).SetUpdateFlag(EUnitFields.UNIT_FIELD_CHANNEL_OBJECT, tmpGO.GUID);
                ((WS_PlayerData.CharacterObject)Caster).SendCharacterUpdate();
            }

            // DONE: Despawn the object after the duration
            if (SPELLs[SpellID].GetDuration > 0)
            {
                tmpGO.Despawn(SPELLs[SpellID].GetDuration);
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_ENCHANT_ITEM(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            if (Target.itemTarget is null)
                return SpellFailedReason.SPELL_FAILED_ITEM_NOT_FOUND;

            // TODO: If there already is an enchantment here, ask for permission?

            Target.itemTarget.AddEnchantment(SpellInfo.MiscValue, EnchantSlots.ENCHANTMENT_PERM);
            if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(Target.itemTarget.OwnerGUID))
            {
                WorldServiceLocator._WorldServer.CHARACTERs[Target.itemTarget.OwnerGUID].SendItemUpdate(Target.itemTarget);
                var EnchantLog = new Packets.PacketClass(OPCODES.SMSG_ENCHANTMENTLOG);
                EnchantLog.AddUInt64(Target.itemTarget.OwnerGUID);
                EnchantLog.AddUInt64(Caster.GUID);
                EnchantLog.AddInt32(Target.itemTarget.ItemEntry);
                EnchantLog.AddInt32(SpellInfo.MiscValue);
                EnchantLog.AddInt8(0);
                WorldServiceLocator._WorldServer.CHARACTERs[Target.itemTarget.OwnerGUID].client.Send(ref EnchantLog);
                // DONE: Send to trader also
                if (WorldServiceLocator._WorldServer.CHARACTERs[Target.itemTarget.OwnerGUID].tradeInfo is object)
                {
                    if (ReferenceEquals(WorldServiceLocator._WorldServer.CHARACTERs[Target.itemTarget.OwnerGUID].tradeInfo.Trader, WorldServiceLocator._WorldServer.CHARACTERs[Target.itemTarget.OwnerGUID]))
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
            if (Target.itemTarget is null)
                return SpellFailedReason.SPELL_FAILED_ITEM_NOT_FOUND;

            // TODO: If there already is an enchantment here, ask for permission?

            int Duration = SPELLs[SpellID].GetDuration;
            if (Duration == 0)
            {
                if (SPELLs[SpellID].SpellVisual == 563) // Fishing
                {
                    Duration = 600; // 10 mins
                }
                else if (SPELLs[SpellID].SpellFamilyName == SpellFamilyNames.SPELLFAMILY_ROGUE)
                {
                    Duration = 3600; // 1 hour
                }
                else if (SPELLs[SpellID].SpellFamilyName == SpellFamilyNames.SPELLFAMILY_SHAMAN)
                {
                    Duration = 1800; // 30 mins
                }
                else if (SPELLs[SpellID].SpellVisual == 215)
                {
                    Duration = 1800; // 30 mins
                }
                else if (SPELLs[SpellID].SpellVisual == 0) // Shaman Rockbiter Weapon
                {
                    Duration = 1800; // 30 mins
                }
                else
                {
                    Duration = 3600;
                } // 1 hour

                Duration *= 1000;
            }

            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[DEBUG] Enchant duration [{0}]", Duration);
            Target.itemTarget.AddEnchantment(SpellInfo.MiscValue, EnchantSlots.ENCHANTMENT_TEMP, Duration);
            if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(Target.itemTarget.OwnerGUID))
            {
                WorldServiceLocator._WorldServer.CHARACTERs[Target.itemTarget.OwnerGUID].SendItemUpdate(Target.itemTarget);
                var EnchantLog = new Packets.PacketClass(OPCODES.SMSG_ENCHANTMENTLOG);
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
            // TODO: If there already is an enchantment here, ask for permission?

            int Duration = SPELLs[SpellID].GetDuration;
            if (Duration == 0)
                Duration = (SpellInfo.valueBase + 1) * 1000;
            if (Duration == 0)
                Duration = 10000;
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[DEBUG] Enchant duration [{0}]({1})", Duration, SpellInfo.valueBase);
            foreach (WS_Base.BaseUnit Unit in Infected)
            {
                if (Unit is WS_PlayerData.CharacterObject && ((WS_PlayerData.CharacterObject)Unit).Items.ContainsKey(EquipmentSlots.EQUIPMENT_SLOT_MAINHAND))
                {
                    if (((WS_PlayerData.CharacterObject)Unit).Items(EquipmentSlots.EQUIPMENT_SLOT_MAINHAND).Enchantments.ContainsKey(EnchantSlots.ENCHANTMENT_TEMP) && ((WS_PlayerData.CharacterObject)Unit).Items(EquipmentSlots.EQUIPMENT_SLOT_MAINHAND).Enchantments(EnchantSlots.ENCHANTMENT_TEMP).ID == SpellInfo.MiscValue)
                    {
                        ((WS_PlayerData.CharacterObject)Unit).Items(EquipmentSlots.EQUIPMENT_SLOT_MAINHAND).AddEnchantment(SpellInfo.MiscValue, EnchantSlots.ENCHANTMENT_TEMP, Duration);
                        ((WS_PlayerData.CharacterObject)Unit).SendItemUpdate(((WS_PlayerData.CharacterObject)Unit).Items(EquipmentSlots.EQUIPMENT_SLOT_MAINHAND));
                    }
                }
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_CHARGE(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            if (Caster is WS_Creatures.CreatureObject)
            {
                ((WS_Creatures.CreatureObject)Caster).SetToRealPosition();
            }

            float NearX = Target.unitTarget.positionX;
            if (Target.unitTarget.positionX > Caster.positionX)
                NearX -= 1.0f;
            else
                NearX += 1.0f;
            float NearY = Target.unitTarget.positionY;
            if (Target.unitTarget.positionY > Caster.positionY)
                NearY -= 1.0f;
            else
                NearY += 1.0f;
            float NearZ = WorldServiceLocator._WS_Maps.GetZCoord(NearX, NearY, Caster.positionZ, Caster.MapID);
            if (NearZ > Target.unitTarget.positionZ + 2f | NearZ < Target.unitTarget.positionZ - 2f)
                NearZ = Target.unitTarget.positionZ;
            float moveDist = WorldServiceLocator._WS_Combat.GetDistance(Caster, NearX, NearY, NearZ);
            int TimeToMove = (int)(moveDist / SPELLs[SpellID].Speed * 1000.0f);
            var SMSG_MONSTER_MOVE = new Packets.PacketClass(OPCODES.SMSG_MONSTER_MOVE);
            SMSG_MONSTER_MOVE.AddPackGUID(Caster.GUID);
            SMSG_MONSTER_MOVE.AddSingle(Caster.positionX);
            SMSG_MONSTER_MOVE.AddSingle(Caster.positionY);
            SMSG_MONSTER_MOVE.AddSingle(Caster.positionZ);
            SMSG_MONSTER_MOVE.AddInt32(WorldServiceLocator._NativeMethods.timeGetTime(""));         // Sequence/MSTime?
            SMSG_MONSTER_MOVE.AddInt8(0);
            SMSG_MONSTER_MOVE.AddInt32(0x100);
            SMSG_MONSTER_MOVE.AddInt32(TimeToMove);  // Time
            SMSG_MONSTER_MOVE.AddInt32(1);           // Points Count
            SMSG_MONSTER_MOVE.AddSingle(NearX);          // First Point X
            SMSG_MONSTER_MOVE.AddSingle(NearY);          // First Point Y
            SMSG_MONSTER_MOVE.AddSingle(NearZ);          // First Point Z
            Caster.SendToNearPlayers(ref SMSG_MONSTER_MOVE);
            SMSG_MONSTER_MOVE.Dispose();
            if (Caster is WS_PlayerData.CharacterObject)
            {
                WorldServiceLocator._WS_Combat.SendAttackStart(Caster.GUID, Target.unitTarget.GUID, ref ((WS_PlayerData.CharacterObject)Caster).client);
                ((WS_PlayerData.CharacterObject)Caster).attackState.AttackStart(Target.unitTarget);
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_KNOCK_BACK(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            foreach (WS_Base.BaseUnit Unit in Infected)
            {
                float Direction = WorldServiceLocator._WS_Combat.GetOrientation(Caster.positionX, Unit.positionX, Caster.positionY, Unit.positionY);
                var packet = new Packets.PacketClass(OPCODES.SMSG_MOVE_KNOCK_BACK);
                packet.AddPackGUID(Unit.GUID);
                packet.AddInt32(0);
                packet.AddSingle((float)Math.Cos(Direction)); // X-direction
                packet.AddSingle((float)Math.Sin(Direction)); // Y-direction
                packet.AddSingle(SpellInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 10.0f); // horizontal speed
                packet.AddSingle(SpellInfo.MiscValue / -10.0f); // Z-speed
                Unit.SendToNearPlayers(ref packet);
                packet.Dispose();
                if (Unit is WS_Creatures.CreatureObject)
                {
                    // TODO: Calculate were the creature would fall, and pause the AI until it lands
                }
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_SCRIPT_EFFECT(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            if (SPELLs[SpellID].SpellFamilyName == SpellFamilyNames.SPELLFAMILY_PALADIN)
            {
                if (SPELLs[SpellID].SpellIconID == 70 || SPELLs[SpellID].SpellIconID == 242)
                {
                    return SPELL_EFFECT_HEAL(ref Target, ref Caster, ref SpellInfo, SpellID, ref Infected, ref Item);
                }
                else if (Conversions.ToBoolean(SPELLs[SpellID].SpellFamilyFlags & 1 << 23))
                {
                    if (Target.unitTarget is null || Target.unitTarget.IsDead)
                        return SpellFailedReason.SPELL_FAILED_TARGETS_DEAD;
                    int SpellID2 = 0;
                    for (int i = 0, loopTo = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1; i <= loopTo; i++)
                    {
                        if (((WS_Base.BaseUnit)Caster).ActiveSpells[i] is object && ((WS_Base.BaseUnit)Caster).ActiveSpells[i].GetSpellInfo.SpellVisual == 5622 && ((WS_Base.BaseUnit)Caster).ActiveSpells[i].GetSpellInfo.SpellFamilyName == SpellFamilyNames.SPELLFAMILY_PALADIN)
                        {
                            if (((WS_Base.BaseUnit)Caster).ActiveSpells[i].Aura_Info[2] is object)
                            {
                                SpellID2 = ((WS_Base.BaseUnit)Caster).ActiveSpells[i].Aura_Info[2].valueBase + 1;
                                break;
                            }
                        }
                    }

                    if (SpellID2 == 0 || SPELLs.ContainsKey(SpellID2) == false)
                        return SpellFailedReason.SPELL_FAILED_UNKNOWN;
                    var castParams = new CastSpellParameters(ref Target, ref Caster, SpellID2);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(castParams.Cast));
                }
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_DUEL(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            switch (SpellInfo.implicitTargetA)
            {
                case var @case when @case == SpellImplicitTargets.TARGET_DUEL_VS_PLAYER:
                    {
                        if (!(Target.unitTarget is WS_PlayerData.CharacterObject))
                            return SpellFailedReason.SPELL_FAILED_TARGET_NOT_PLAYER;
                        if (!(Caster is WS_PlayerData.CharacterObject))
                            return default;

                        // TODO: Some more checks
                        if (((WS_PlayerData.CharacterObject)Caster).DuelArbiter != 0m)
                            return SpellFailedReason.SPELL_FAILED_SPELL_IN_PROGRESS;
                        if (((WS_PlayerData.CharacterObject)Target.unitTarget).IsInDuel)
                            return SpellFailedReason.SPELL_FAILED_TARGET_DUELING;
                        if (((WS_PlayerData.CharacterObject)Target.unitTarget).inCombatWith.Count > 0)
                            return SpellFailedReason.SPELL_FAILED_TARGET_IN_COMBAT;
                        if (Caster.Invisibility != InvisibilityLevel.VISIBLE)
                            return SpellFailedReason.SPELL_FAILED_CANT_DUEL_WHILE_INVISIBLE;
                        // CAST_FAIL_CANT_START_DUEL_STEALTHED
                        // CAST_FAIL_NO_DUELING_HERE

                        // DONE: Get middle coordinate
                        float flagX = Caster.positionX + (Target.unitTarget.positionX - Caster.positionX) / 2f;
                        float flagY = Caster.positionY + (Target.unitTarget.positionY - Caster.positionY) / 2f;
                        float flagZ = WorldServiceLocator._WS_Maps.GetZCoord(flagX, flagY, Caster.positionZ + 3.0f, Caster.MapID);

                        // DONE: Spawn duel flag (GO Entry in SpellInfo.MiscValue) in middle of the 2 players
                        var tmpGO = new WS_GameObjects.GameObjectObject(SpellInfo.MiscValue, Caster.MapID, flagX, flagY, flagZ, 0f, Caster.GUID);
                        tmpGO.AddToWorld();

                        // DONE: Set duel arbiter and parner
                        // CType(Caster, CharacterObject).DuelArbiter = tmpGO.GUID        Commented to fix 2 packets for duel accept
                        ((WS_PlayerData.CharacterObject)Target.unitTarget).DuelArbiter = tmpGO.GUID;
                        ((WS_PlayerData.CharacterObject)Caster).DuelPartner = (WS_PlayerData.CharacterObject)Target.unitTarget;
                        ((WS_PlayerData.CharacterObject)Target.unitTarget).DuelPartner = (WS_PlayerData.CharacterObject)Caster;

                        // DONE: Send duel request packet
                        var packet = new Packets.PacketClass(OPCODES.SMSG_DUEL_REQUESTED);
                        packet.AddUInt64(tmpGO.GUID);
                        packet.AddUInt64(Caster.GUID);
                        ((WS_PlayerData.CharacterObject)Target.unitTarget).client.SendMultiplyPackets(ref packet);
                        ((WS_PlayerData.CharacterObject)Caster).client.SendMultiplyPackets(ref packet);
                        packet.Dispose();
                        break;
                    }

                default:
                    {
                        return SpellFailedReason.SPELL_FAILED_BAD_IMPLICIT_TARGETS;
                    }
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public SpellFailedReason SPELL_EFFECT_QUEST_COMPLETE(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
        {
            foreach (WS_Base.BaseUnit Unit in Infected)
            {
                if (Unit is WS_PlayerData.CharacterObject)
                {
                    WS_PlayerData.CharacterObject argobjCharacter = (WS_PlayerData.CharacterObject)Unit;
                    WorldServiceLocator._WorldServer.ALLQUESTS.CompleteQuest(ref argobjCharacter, SpellInfo.MiscValue, Caster.GUID);
                }
            }

            return SpellFailedReason.SPELL_NO_ERROR;
        }

        public List<WS_Base.BaseUnit> GetEnemyAtPoint(ref WS_Base.BaseUnit objCharacter, float PosX, float PosY, float PosZ, float Distance)
        {
            var result = new List<WS_Base.BaseUnit>();
            if (objCharacter is WS_PlayerData.CharacterObject)
            {
                foreach (ulong pGUID in ((WS_PlayerData.CharacterObject)objCharacter).playersNear.ToArray())
                {
                    if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(pGUID) && (((WS_PlayerData.CharacterObject)objCharacter).IsHorde != WorldServiceLocator._WorldServer.CHARACTERs[pGUID].IsHorde || ((WS_PlayerData.CharacterObject)objCharacter).DuelPartner is object && ReferenceEquals(((WS_PlayerData.CharacterObject)objCharacter).DuelPartner, WorldServiceLocator._WorldServer.CHARACTERs[pGUID])) && WorldServiceLocator._WorldServer.CHARACTERs[pGUID].IsDead == false)
                    {
                        if (WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.CHARACTERs[pGUID], PosX, PosY, PosZ) < Distance)
                            result.Add(WorldServiceLocator._WorldServer.CHARACTERs[pGUID]);
                    }
                }

                foreach (ulong cGUID in ((WS_PlayerData.CharacterObject)objCharacter).creaturesNear.ToArray())
                {
                    if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(cGUID) && !(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID] is WS_Totems.TotemObject) && WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].IsDead == false && ((WS_PlayerData.CharacterObject)objCharacter).GetReaction(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].Faction) <= TReaction.NEUTRAL)
                    {
                        if (WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID], PosX, PosY, PosZ) < Distance)
                            result.Add(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID]);
                    }
                }
            }
            else if (objCharacter is WS_Creatures.CreatureObject)
            {
                foreach (ulong pGUID in objCharacter.SeenBy.ToArray())
                {
                    if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(pGUID) && WorldServiceLocator._WorldServer.CHARACTERs[pGUID].IsDead == false && WorldServiceLocator._WorldServer.CHARACTERs[pGUID].GetReaction(((WS_Creatures.CreatureObject)objCharacter).Faction) <= TReaction.NEUTRAL)
                    {
                        if (WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.CHARACTERs[pGUID], PosX, PosY, PosZ) < Distance)
                            result.Add(WorldServiceLocator._WorldServer.CHARACTERs[pGUID]);
                    }
                }
            }

            return result;
        }

        public List<WS_Base.BaseUnit> GetEnemyAroundMe(ref WS_Base.BaseUnit objCharacter, float Distance, [Optional, DefaultParameterValue(null)] ref WS_Base.BaseUnit r)
        {
            var result = new List<WS_Base.BaseUnit>();
            if (r is null)
                r = objCharacter;
            if (r is WS_PlayerData.CharacterObject)
            {
                foreach (ulong pGUID in ((WS_PlayerData.CharacterObject)r).playersNear.ToArray())
                {
                    if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(pGUID) && (((WS_PlayerData.CharacterObject)r).IsHorde != WorldServiceLocator._WorldServer.CHARACTERs[pGUID].IsHorde || ((WS_PlayerData.CharacterObject)r).DuelPartner is object && ReferenceEquals(((WS_PlayerData.CharacterObject)r).DuelPartner, WorldServiceLocator._WorldServer.CHARACTERs[pGUID])) && WorldServiceLocator._WorldServer.CHARACTERs[pGUID].IsDead == false)
                    {
                        if (WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.CHARACTERs[pGUID], objCharacter) < Distance)
                            result.Add(WorldServiceLocator._WorldServer.CHARACTERs[pGUID]);
                    }
                }

                foreach (ulong cGUID in ((WS_PlayerData.CharacterObject)r).creaturesNear.ToArray())
                {
                    if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(cGUID) && !(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID] is WS_Totems.TotemObject) && WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].IsDead == false && ((WS_PlayerData.CharacterObject)r).GetReaction(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].Faction) <= TReaction.NEUTRAL)
                    {
                        if (WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID], objCharacter) < Distance)
                            result.Add(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID]);
                    }
                }
            }
            else if (r is WS_Creatures.CreatureObject)
            {
                foreach (ulong pGUID in r.SeenBy.ToArray())
                {
                    if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(pGUID) && WorldServiceLocator._WorldServer.CHARACTERs[pGUID].IsDead == false && WorldServiceLocator._WorldServer.CHARACTERs[pGUID].GetReaction(((WS_Creatures.CreatureObject)r).Faction) <= TReaction.NEUTRAL)
                    {
                        if (WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.CHARACTERs[pGUID], objCharacter) < Distance)
                            result.Add(WorldServiceLocator._WorldServer.CHARACTERs[pGUID]);
                    }
                }
            }

            return result;
        }

        public List<WS_Base.BaseUnit> GetFriendAroundMe(ref WS_Base.BaseUnit objCharacter, float Distance)
        {
            var result = new List<WS_Base.BaseUnit>();
            if (objCharacter is WS_PlayerData.CharacterObject)
            {
                foreach (ulong pGUID in ((WS_PlayerData.CharacterObject)objCharacter).playersNear.ToArray())
                {
                    if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(pGUID) && ((WS_PlayerData.CharacterObject)objCharacter).IsHorde == WorldServiceLocator._WorldServer.CHARACTERs[pGUID].IsHorde && WorldServiceLocator._WorldServer.CHARACTERs[pGUID].IsDead == false)
                    {
                        if (WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.CHARACTERs[pGUID], objCharacter) < Distance)
                            result.Add(WorldServiceLocator._WorldServer.CHARACTERs[pGUID]);
                    }
                }

                foreach (ulong cGUID in ((WS_PlayerData.CharacterObject)objCharacter).creaturesNear.ToArray())
                {
                    if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(cGUID) && !(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID] is WS_Totems.TotemObject) && WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].IsDead == false && ((WS_PlayerData.CharacterObject)objCharacter).GetReaction(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].Faction) > TReaction.NEUTRAL)
                    {
                        if (WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID], objCharacter) < Distance)
                            result.Add(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID]);
                    }
                }
            }
            else if (objCharacter is WS_Creatures.CreatureObject)
            {
                foreach (ulong pGUID in objCharacter.SeenBy.ToArray())
                {
                    if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(pGUID) && WorldServiceLocator._WorldServer.CHARACTERs[pGUID].IsDead == false && WorldServiceLocator._WorldServer.CHARACTERs[pGUID].GetReaction(((WS_Creatures.CreatureObject)objCharacter).Faction) > TReaction.NEUTRAL)
                    {
                        if (WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.CHARACTERs[pGUID], objCharacter) < Distance)
                            result.Add(WorldServiceLocator._WorldServer.CHARACTERs[pGUID]);
                    }
                }
            }

            return result;
        }

        public List<WS_Base.BaseUnit> GetFriendPlayersAroundMe(ref WS_Base.BaseUnit objCharacter, float Distance)
        {
            var result = new List<WS_Base.BaseUnit>();
            if (objCharacter is WS_PlayerData.CharacterObject)
            {
                foreach (ulong pGUID in ((WS_PlayerData.CharacterObject)objCharacter).playersNear.ToArray())
                {
                    if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(pGUID) && ((WS_PlayerData.CharacterObject)objCharacter).IsHorde == WorldServiceLocator._WorldServer.CHARACTERs[pGUID].IsHorde && WorldServiceLocator._WorldServer.CHARACTERs[pGUID].IsDead == false)
                    {
                        if (WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.CHARACTERs[pGUID], objCharacter) < Distance)
                            result.Add(WorldServiceLocator._WorldServer.CHARACTERs[pGUID]);
                    }
                }
            }
            else if (objCharacter is WS_Creatures.CreatureObject)
            {
                foreach (ulong pGUID in objCharacter.SeenBy.ToArray())
                {
                    if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(pGUID) && WorldServiceLocator._WorldServer.CHARACTERs[pGUID].IsDead == false && WorldServiceLocator._WorldServer.CHARACTERs[pGUID].GetReaction(((WS_Creatures.CreatureObject)objCharacter).Faction) > TReaction.NEUTRAL)
                    {
                        if (WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.CHARACTERs[pGUID], objCharacter) < Distance)
                            result.Add(WorldServiceLocator._WorldServer.CHARACTERs[pGUID]);
                    }
                }
            }

            return result;
        }

        public List<WS_Base.BaseUnit> GetPartyMembersAroundMe(ref WS_PlayerData.CharacterObject objCharacter, float distance)
        {
            var result = new List<WS_Base.BaseUnit>() { objCharacter };
            if (!objCharacter.IsInGroup)
                return result;
            foreach (ulong GUID in objCharacter.Group.LocalMembers.ToArray())
            {
                if (objCharacter.playersNear.Contains(GUID) && WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(GUID))
                {
                    if (WorldServiceLocator._WS_Combat.GetDistance(objCharacter, WorldServiceLocator._WorldServer.CHARACTERs[GUID]) < distance)
                        result.Add(WorldServiceLocator._WorldServer.CHARACTERs[GUID]);
                }
            }

            return result;
        }

        public List<WS_Base.BaseUnit> GetPartyMembersAtPoint(ref WS_PlayerData.CharacterObject objCharacter, float Distance, float PosX, float PosY, float PosZ)
        {
            var result = new List<WS_Base.BaseUnit>();
            if (WorldServiceLocator._WS_Combat.GetDistance(objCharacter, PosX, PosY, PosZ) < Distance)
                result.Add(objCharacter);
            if (!objCharacter.IsInGroup)
                return result;
            foreach (ulong GUID in objCharacter.Group.LocalMembers.ToArray())
            {
                if (objCharacter.playersNear.Contains(GUID) && WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(GUID))
                {
                    if (WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.CHARACTERs[GUID], PosX, PosY, PosZ) < Distance)
                        result.Add(WorldServiceLocator._WorldServer.CHARACTERs[GUID]);
                }
            }

            return result;
        }

        public List<WS_Base.BaseUnit> GetEnemyInFrontOfMe(ref WS_Base.BaseUnit objCharacter, float Distance)
        {
            var result = new List<WS_Base.BaseUnit>();
            WS_Base.BaseUnit argr = null;
            var tmp = GetEnemyAroundMe(ref objCharacter, Distance, r: ref argr);
            foreach (WS_Base.BaseUnit unit in tmp)
            {
                WS_Base.BaseObject argObject1 = objCharacter;
                WS_Base.BaseObject argObject2 = unit;
                if (WorldServiceLocator._WS_Combat.IsInFrontOf(ref argObject1, ref argObject2))
                    result.Add(unit);
            }

            return result;
        }

        public List<WS_Base.BaseUnit> GetEnemyInBehindMe(ref WS_Base.BaseUnit objCharacter, float Distance)
        {
            var result = new List<WS_Base.BaseUnit>();
            WS_Base.BaseUnit argr = null;
            var tmp = GetEnemyAroundMe(ref objCharacter, Distance, r: ref argr);
            foreach (WS_Base.BaseUnit unit in tmp)
            {
                WS_Base.BaseObject argObject1 = objCharacter;
                WS_Base.BaseObject argObject2 = unit;
                if (WorldServiceLocator._WS_Combat.IsInBackOf(ref argObject1, ref argObject2))
                    result.Add(unit);
            }

            return result;
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia *//* TODO ERROR: Skipped RegionDirectiveTrivia */
        public delegate void ApplyAuraHandler(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action);

        public const int AURAs_COUNT = 261;
        public ApplyAuraHandler[] AURAs = new ApplyAuraHandler[262];

        public void SPELL_AURA_NONE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
        }

        public void SPELL_AURA_DUMMY(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[DEBUG] Aura Dummy for spell {0}.", SpellID);
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        switch (SpellID)
                        {
                            case 33763:
                                {
                                    // HACK: Lifebloom
                                    // TODO: Can lifebloom crit (the end damage)?
                                    int Damage = EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                                    WS_Base.BaseUnit argCaster = (WS_Base.BaseUnit)Caster;
                                    SendHealSpellLog(ref argCaster, ref Target, SpellID, Damage, false);
                                    WS_Base.BaseUnit argAttacker = null;
                                    Target.Heal(Damage, Attacker: ref argAttacker);
                                    break;
                                }
                        }

                        break;
                    }
            }
        }

        public void SPELL_AURA_BIND_SIGHT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        if (Caster is WS_PlayerData.CharacterObject)
                        {
                            ((WS_PlayerData.CharacterObject)Caster).DuelArbiter = Target.GUID;
                            ((WS_PlayerData.CharacterObject)Caster).SetUpdateFlag(EPlayerFields.PLAYER_FARSIGHT, Target.GUID);
                            ((WS_PlayerData.CharacterObject)Caster).SendCharacterUpdate(true);
                        }

                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        if (Caster is WS_PlayerData.CharacterObject)
                        {
                            ((WS_PlayerData.CharacterObject)Caster).DuelArbiter = 0UL;
                            ((WS_PlayerData.CharacterObject)Caster).SetUpdateFlag(EPlayerFields.PLAYER_FARSIGHT, Conversions.ToLong(0));
                            ((WS_PlayerData.CharacterObject)Caster).SendCharacterUpdate(true);
                        }

                        break;
                    }
            }
        }

        public void SPELL_AURA_FAR_SIGHT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        if (Target is WS_PlayerData.CharacterObject)
                        {
                            ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EPlayerFields.PLAYER_FARSIGHT, EffectInfo.MiscValue);
                            ((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate(true);
                        }

                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        if (Target is WS_PlayerData.CharacterObject)
                        {
                            ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EPlayerFields.PLAYER_FARSIGHT, 0);
                            ((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate(true);
                        }

                        break;
                    }
            }
        }

        public void SPELL_AURA_SCHOOL_IMMUNITY(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        Target.SchoolImmunity = (byte)(Target.SchoolImmunity | 1 << EffectInfo.MiscValue);
                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        Target.SchoolImmunity = (byte)(Target.SchoolImmunity & ~(1 << EffectInfo.MiscValue));
                        break;
                    }
            }
        }

        public void SPELL_AURA_MECHANIC_IMMUNITY(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        Target.MechanicImmunity = (uint)(Target.MechanicImmunity | (long)(1 << EffectInfo.MiscValue));
                        Target.RemoveAurasByMechanic(EffectInfo.MiscValue);
                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        Target.MechanicImmunity = (uint)(Target.MechanicImmunity & (long)~(1 << EffectInfo.MiscValue));
                        break;
                    }
            }
        }

        public void SPELL_AURA_DISPEL_IMMUNITY(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        Target.DispellImmunity = (uint)(Target.DispellImmunity | (long)(1 << EffectInfo.MiscValue));
                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        Target.DispellImmunity = (uint)(Target.DispellImmunity & (long)~(1 << EffectInfo.MiscValue));
                        break;
                    }
            }
        }

        public void SPELL_AURA_TRACK_CREATURES(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        if (Target is WS_PlayerData.CharacterObject)
                        {
                            ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EPlayerFields.PLAYER_TRACK_CREATURES, 1 << EffectInfo.MiscValue - 1);
                            ((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate(true);
                        }

                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        if (Target is WS_PlayerData.CharacterObject)
                        {
                            ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EPlayerFields.PLAYER_TRACK_CREATURES, 0);
                            ((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate(true);
                        }

                        break;
                    }
            }
        }

        public void SPELL_AURA_TRACK_RESOURCES(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        if (Target is WS_PlayerData.CharacterObject)
                        {
                            ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EPlayerFields.PLAYER_TRACK_RESOURCES, 1 << EffectInfo.MiscValue - 1);
                            ((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate(true);
                        }

                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        if (Target is WS_PlayerData.CharacterObject)
                        {
                            ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EPlayerFields.PLAYER_TRACK_RESOURCES, 0);
                            ((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate(true);
                        }

                        break;
                    }
            }
        }

        public void SPELL_AURA_MOD_SCALE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        Target.Size = (float)(Target.Size * (EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d + 1d));
                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        Target.Size = (float)(Target.Size / (EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d + 1d));
                        break;
                    }
            }

            // DONE: Send update
            if (Target is WS_PlayerData.CharacterObject)
            {
                ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EObjectFields.OBJECT_FIELD_SCALE_X, Target.Size);
                ((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate(true);
            }
            else
            {
                var packet = new Packets.UpdatePacketClass();
                var tmpUpdate = new Packets.UpdateClass(EObjectFields.OBJECT_END);
                tmpUpdate.SetUpdateFlag(EObjectFields.OBJECT_FIELD_SCALE_X, Target.Size);
                tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, (WS_Creatures.CreatureObject)Target);
                Packets.PacketClass argpacket = packet;
                Target.SendToNearPlayers(ref argpacket);
                tmpUpdate.Dispose();
                packet.Dispose();
            }
        }

        public void SPELL_AURA_MOD_SKILL(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        if (Target is WS_PlayerData.CharacterObject && ((WS_PlayerData.CharacterObject)Target).Skills.ContainsKey(EffectInfo.MiscValue))
                        {
                            {
                                var withBlock = (WS_PlayerData.CharacterObject)Target;
                                withBlock.Skills[EffectInfo.MiscValue].Bonus = (short)(withBlock.Skills[EffectInfo.MiscValue].Bonus + EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0));
                                withBlock.SetUpdateFlag(EPlayerFields.PLAYER_SKILL_INFO_1_1 + (int)withBlock.SkillsPositions[EffectInfo.MiscValue] * 3 + 2, withBlock.Skills[EffectInfo.MiscValue].Bonus);                      // skill1.Bonus
                                withBlock.SendCharacterUpdate(true);
                            }
                        }

                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        if (Target is WS_PlayerData.CharacterObject && ((WS_PlayerData.CharacterObject)Target).Skills.ContainsKey(EffectInfo.MiscValue))
                        {
                            {
                                var withBlock1 = (WS_PlayerData.CharacterObject)Target;
                                withBlock1.Skills[EffectInfo.MiscValue].Bonus = (short)(withBlock1.Skills[EffectInfo.MiscValue].Bonus - EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0));
                                withBlock1.SetUpdateFlag(EPlayerFields.PLAYER_SKILL_INFO_1_1 + (int)withBlock1.SkillsPositions[EffectInfo.MiscValue] * 3 + 2, withBlock1.Skills[EffectInfo.MiscValue].Bonus);                      // skill1.Bonus
                                withBlock1.SendCharacterUpdate(true);
                            }
                        }

                        break;
                    }
            }
        }

        public void SPELL_AURA_PERIODIC_DUMMY(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_ADD:
                    {
                        if (!(Target is WS_PlayerData.CharacterObject))
                            return;
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
                                    // HACK: Drink
                                    int Damage = EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) * StackCount;
                                    ((WS_PlayerData.CharacterObject)Target).ManaRegenBonus += Damage;
                                    ((WS_PlayerData.CharacterObject)Target).UpdateManaRegen();
                                    break;
                                }
                        }

                        break;
                    }

                case var case1 when case1 == AuraAction.AURA_REMOVE:
                case var case2 when case2 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        if (!(Caster is WS_PlayerData.CharacterObject))
                            return;
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
                                    // HACK: Drink
                                    int Damage = EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) * StackCount;
                                    ((WS_PlayerData.CharacterObject)Target).ManaRegenBonus -= Damage;
                                    ((WS_PlayerData.CharacterObject)Target).UpdateManaRegen();
                                    break;
                                }
                        }

                        break;
                    }

                case var case3 when case3 == AuraAction.AURA_UPDATE:
                    {
                        switch (SpellID)
                        {
                            case 43265:
                            case 49936:
                            case 49937:
                            case 49938:
                                {
                                    // HACK: Death and Decay
                                    int Damage;
                                    if (Caster is WS_DynamicObjects.DynamicObjectObject)
                                    {
                                        Damage = EffectInfo.get_GetValue(((WS_DynamicObjects.DynamicObjectObject)Caster).Caster.Level, 0) * StackCount;
                                        Target.DealDamage(Damage, ref ((WS_DynamicObjects.DynamicObjectObject)Caster).Caster);
                                    }
                                    else
                                    {
                                        Damage = EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) * StackCount;
                                        WS_Base.BaseUnit argAttacker = (WS_Base.BaseUnit)Caster;
                                        Target.DealDamage(Damage, ref argAttacker);
                                    }

                                    break;
                                }
                        }

                        break;
                    }
            }
        }

        public void SPELL_AURA_PERIODIC_DAMAGE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_ADD:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_REMOVE:
                case var case2 when case2 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        return;
                    }

                case var case3 when case3 == AuraAction.AURA_UPDATE:
                    {
                        if (Caster is WS_DynamicObjects.DynamicObjectObject)
                        {
                            int Damage = EffectInfo.get_GetValue(((WS_DynamicObjects.DynamicObjectObject)Caster).Caster.Level, 0) * StackCount;
                            Target.DealSpellDamage(ref ((WS_DynamicObjects.DynamicObjectObject)Caster).Caster, ref EffectInfo, SpellID, Damage, SPELLs[SpellID].School, SpellType.SPELL_TYPE_DOT);
                        }
                        else
                        {
                            int Damage = EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) * StackCount;
                            Target.DealSpellDamage(ref Caster, ref EffectInfo, SpellID, Damage, SPELLs[SpellID].School, SpellType.SPELL_TYPE_DOT);
                        }

                        break;
                    }
            }
        }

        public void SPELL_AURA_PERIODIC_HEAL(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_ADD:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_REMOVE:
                case var case2 when case2 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        return;
                    }

                case var case3 when case3 == AuraAction.AURA_UPDATE:
                    {
                        int Damage = EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) * StackCount;

                        // NOTE: This function heals as well
                        Target.DealSpellDamage(ref Caster, ref EffectInfo, SpellID, Damage, SPELLs[SpellID].School, SpellType.SPELL_TYPE_HEALDOT);
                        break;
                    }
            }
        }

        public void SPELL_AURA_PERIODIC_ENERGIZE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_ADD:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_REMOVE:
                case var case2 when case2 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        return;
                    }

                case var case3 when case3 == AuraAction.AURA_UPDATE:
                    {
                        ManaTypes Power = EffectInfo.MiscValue;
                        int Damage = EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) * StackCount;
                        SendPeriodicAuraLog(ref Caster, ref Target, SpellID, Power, Damage, EffectInfo.ApplyAuraIndex);
                        Target.Energize(Damage, Power, ref Caster);
                        break;
                    }
            }
        }

        public void SPELL_AURA_PERIODIC_LEECH(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_ADD:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_REMOVE:
                case var case2 when case2 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        return;
                    }

                case var case3 when case3 == AuraAction.AURA_UPDATE:
                    {
                        int Damage = EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) * StackCount;
                        WS_Base.BaseUnit argCaster = (WS_Base.BaseUnit)Caster;
                        SendPeriodicAuraLog(ref argCaster, ref Target, SpellID, SPELLs[SpellID].School, Damage, EffectInfo.ApplyAuraIndex);
                        WS_Base.BaseUnit argTarget = (WS_Base.BaseUnit)Caster;
                        SendPeriodicAuraLog(ref Target, ref argTarget, SpellID, SPELLs[SpellID].School, Damage, EffectInfo.ApplyAuraIndex);
                        WS_Base.BaseUnit argAttacker = (WS_Base.BaseUnit)Caster;
                        Target.DealDamage(Damage, ref argAttacker);
                        ((WS_Base.BaseUnit)Caster).Heal(Damage, ref Target);
                        break;
                    }
            }
        }

        public void SPELL_AURA_PERIODIC_MANA_LEECH(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_ADD:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_REMOVE:
                case var case2 when case2 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        return;
                    }

                case var case3 when case3 == AuraAction.AURA_UPDATE:
                    {
                        ManaTypes Power = EffectInfo.MiscValue;
                        int Damage = EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) * StackCount;
                        SendPeriodicAuraLog(ref Target, ref Caster, SpellID, Power, Damage, EffectInfo.ApplyAuraIndex);
                        Target.Energize(-Damage, Power, ref Caster);
                        ((WS_Base.BaseUnit)Caster).Energize(Damage, Power, ref Target);
                        break;
                    }
            }
        }

        public void SPELL_AURA_PERIODIC_TRIGGER_SPELL(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_ADD:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_REMOVE:
                case var case2 when case2 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        return;
                    }

                case var case3 when case3 == AuraAction.AURA_UPDATE:
                    {
                        // TODO: Arcane missiles are casted on yourself
                        var Targets = new SpellTargets();
                        Targets.SetTarget_UNIT(ref Target);
                        var castParams = new CastSpellParameters(ref Targets, ref Caster, EffectInfo.TriggerSpell);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(castParams.Cast));
                        if (Caster is WS_Base.BaseUnit)
                        {
                            WS_Base.BaseUnit argCaster = (WS_Base.BaseUnit)Caster;
                            SendPeriodicAuraLog(ref argCaster, ref Target, SpellID, SPELLs[SpellID].School, 0, EffectInfo.ApplyAuraIndex);
                        }

                        break;
                    }
            }
        }

        public void SPELL_AURA_PERIODIC_DAMAGE_PERCENT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_ADD:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_REMOVE:
                case var case2 when case2 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        return;
                    }

                case var case3 when case3 == AuraAction.AURA_UPDATE:
                    {
                        int Damage = (int)(Target.Life.Maximum * EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d * StackCount);
                        Target.DealSpellDamage(ref Caster, ref EffectInfo, SpellID, Damage, SPELLs[SpellID].School, SpellType.SPELL_TYPE_DOT);
                        break;
                    }
            }
        }

        public void SPELL_AURA_PERIODIC_HEAL_PERCENT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        int Damage = (int)(Target.Life.Maximum * EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d * StackCount);

                        // NOTE: This function heals as well
                        Target.DealSpellDamage(ref Caster, ref EffectInfo, SpellID, Damage, SPELLs[SpellID].School, SpellType.SPELL_TYPE_HEALDOT);
                        break;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        return;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        return;
                    }
            }
        }

        public void SPELL_AURA_PERIODIC_ENERGIZE_PERCENT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        ManaTypes Power = EffectInfo.MiscValue;
                        int Damage = (int)(Target.Mana.Maximum * EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d * StackCount);
                        SendPeriodicAuraLog(ref Caster, ref Target, SpellID, Power, Damage, EffectInfo.ApplyAuraIndex);
                        Target.Energize(Damage, Power, ref Caster);
                        break;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        return;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        return;
                    }
            }
        }

        public void SPELL_AURA_MOD_REGEN(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            if (!(Target is WS_PlayerData.CharacterObject))
                return;
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        int Damage = EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) * StackCount;
                        ((WS_PlayerData.CharacterObject)Target).LifeRegenBonus += Damage;

                        // TODO: Increase threat (gain * 0.5)

                        if (SPELLs[SpellID].auraInterruptFlags & SpellAuraInterruptFlags.AURA_INTERRUPT_FLAG_NOT_SEATED)
                        {
                            // Eat emote
                            Target.DoEmote(Emotes.ONESHOT_EAT);
                        }
                        else if (SpellID == 20577)
                        {
                            // HACK: Cannibalize emote
                            Target.DoEmote(Emotes.STATE_CANNIBALIZE);
                        }

                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        int Damage = EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) * StackCount;
                        ((WS_PlayerData.CharacterObject)Target).LifeRegenBonus -= Damage;
                        break;
                    }
            }
        }

        public void SPELL_AURA_MOD_POWER_REGEN(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            if (!(Target is WS_PlayerData.CharacterObject))
                return;
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        int Damage = EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) * StackCount;
                        if (EffectInfo.MiscValue == ManaTypes.TYPE_MANA)
                        {
                            ((WS_PlayerData.CharacterObject)Target).ManaRegenBonus += Damage;
                            ((WS_PlayerData.CharacterObject)Target).UpdateManaRegen();
                        }
                        else if (EffectInfo.MiscValue == ManaTypes.TYPE_RAGE)
                        {
                            ((WS_PlayerData.CharacterObject)Target).RageRegenBonus = (int)(((WS_PlayerData.CharacterObject)Target).RageRegenBonus + Damage / 17d * 10d);
                        }

                        if (SPELLs[SpellID].auraInterruptFlags & SpellAuraInterruptFlags.AURA_INTERRUPT_FLAG_NOT_SEATED)
                        {
                            // Eat emote
                            Target.DoEmote(Emotes.ONESHOT_EAT);
                        }
                        else if (SpellID == 20577)
                        {
                            // HACK: Cannibalize emote
                            Target.DoEmote(Emotes.STATE_CANNIBALIZE);
                        }

                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        int Damage = EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) * StackCount;
                        if (EffectInfo.MiscValue == ManaTypes.TYPE_MANA)
                        {
                            ((WS_PlayerData.CharacterObject)Target).ManaRegenBonus -= Damage;
                            ((WS_PlayerData.CharacterObject)Target).UpdateManaRegen();
                        }
                        else if (EffectInfo.MiscValue == ManaTypes.TYPE_RAGE)
                        {
                            ((WS_PlayerData.CharacterObject)Target).RageRegenBonus = (int)(((WS_PlayerData.CharacterObject)Target).RageRegenBonus - Damage / 17d * 10d);
                        }

                        break;
                    }
            }
        }

        public void SPELL_AURA_MOD_POWER_REGEN_PERCENT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            if (!(Target is WS_PlayerData.CharacterObject))
                return;
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        if (EffectInfo.MiscValue == ManaTypes.TYPE_MANA)
                        {
                            int Damage = (int)(EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) * StackCount / 100d);
                            ((WS_PlayerData.CharacterObject)Target).ManaRegenerationModifier += Damage;
                            ((WS_PlayerData.CharacterObject)Target).UpdateManaRegen();
                        }

                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        int Damage = (int)(EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) * StackCount / 100d);
                        ((WS_PlayerData.CharacterObject)Target).ManaRegenerationModifier -= Damage;
                        ((WS_PlayerData.CharacterObject)Target).UpdateManaRegen();
                        break;
                    }
            }
        }

        public void SPELL_AURA_TRANSFORM(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_ADD:
                    {
                        if (!WorldServiceLocator._WorldServer.CREATURESDatabase.ContainsKey(EffectInfo.MiscValue))
                        {
                            var creature = new CreatureInfo(EffectInfo.MiscValue);
                            WorldServiceLocator._WorldServer.CREATURESDatabase.Add(EffectInfo.MiscValue, creature);
                        }

                        Target.Model = WorldServiceLocator._WorldServer.CREATURESDatabase[EffectInfo.MiscValue].GetFirstModel;
                        break;
                    }

                case var case1 when case1 == AuraAction.AURA_REMOVE:
                case var case2 when case2 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        if (Target is WS_PlayerData.CharacterObject)
                        {
                            Target.Model = WorldServiceLocator._Functions.GetRaceModel(((WS_PlayerData.CharacterObject)Target).Race, ((WS_PlayerData.CharacterObject)Target).Gender);
                        }
                        else
                        {
                            Target.Model = ((WS_Creatures.CreatureObject)Target).CreatureInfo.GetRandomModel;
                        }

                        break;
                    }

                case var case3 when case3 == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }
            }

            // DONE: Model update
            if (Target is WS_PlayerData.CharacterObject)
            {
                ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_DISPLAYID, Target.Model);
                ((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate(true);
            }
            else
            {
                var packet = new Packets.UpdatePacketClass();
                var tmpUpdate = new Packets.UpdateClass(EUnitFields.UNIT_END);
                tmpUpdate.SetUpdateFlag(EUnitFields.UNIT_FIELD_DISPLAYID, Target.Model);
                tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, (WS_Creatures.CreatureObject)Target);
                Packets.PacketClass argpacket = packet;
                Target.SendToNearPlayers(ref argpacket);
                tmpUpdate.Dispose();
                packet.Dispose();
            }
        }

        public void SPELL_AURA_GHOST(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            if (!(Target is WS_PlayerData.CharacterObject))
                return;
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        Target.Invisibility = InvisibilityLevel.DEAD;
                        Target.CanSeeInvisibility = InvisibilityLevel.DEAD;
                        WS_PlayerData.CharacterObject argCharacter = (WS_PlayerData.CharacterObject)Target;
                        WorldServiceLocator._WS_CharMovement.UpdateCell(ref argCharacter);
                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        Target.Invisibility = InvisibilityLevel.VISIBLE;
                        Target.CanSeeInvisibility = InvisibilityLevel.INIVISIBILITY;
                        WS_PlayerData.CharacterObject argCharacter1 = (WS_PlayerData.CharacterObject)Target;
                        WorldServiceLocator._WS_CharMovement.UpdateCell(ref argCharacter1);
                        break;
                    }
            }
        }

        public void SPELL_AURA_MOD_INVISIBILITY(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        ((WS_PlayerData.CharacterObject)Target).cPlayerFieldBytes2 = ((WS_PlayerData.CharacterObject)Target).cPlayerFieldBytes2 | 0x4000;
                        Target.Invisibility = InvisibilityLevel.INIVISIBILITY;
                        Target.Invisibility_Value += EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        ((WS_PlayerData.CharacterObject)Target).cPlayerFieldBytes2 = ((WS_PlayerData.CharacterObject)Target).cPlayerFieldBytes2 & ~0x4000;
                        Target.Invisibility = InvisibilityLevel.VISIBLE;
                        Target.Invisibility_Value -= EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                        break;
                    }
            }

            // DONE: Send update
            if (Target is WS_PlayerData.CharacterObject)
            {
                ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EPlayerFields.PLAYER_FIELD_BYTES2, ((WS_PlayerData.CharacterObject)Target).cPlayerFieldBytes2);
                ((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate(true);
                WS_PlayerData.CharacterObject argCharacter = (WS_PlayerData.CharacterObject)Target;
                WorldServiceLocator._WS_CharMovement.UpdateCell(ref argCharacter);
            }
            else
            {
                // TODO: Still not done for creatures
            }
        }

        public void SPELL_AURA_MOD_STEALTH(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        Target.Invisibility = InvisibilityLevel.STEALTH;
                        Target.Invisibility_Value += EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        Target.Invisibility = InvisibilityLevel.VISIBLE;
                        Target.Invisibility_Value -= EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                        break;
                    }
            }

            // DONE: Update the cell
            WS_PlayerData.CharacterObject argCharacter = (WS_PlayerData.CharacterObject)Target;
            WorldServiceLocator._WS_CharMovement.UpdateCell(ref argCharacter);
        }

        public void SPELL_AURA_MOD_STEALTH_LEVEL(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        Target.Invisibility_Bonus += EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        Target.Invisibility_Bonus -= EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                        break;
                    }
            }
        }

        public void SPELL_AURA_MOD_INVISIBILITY_DETECTION(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        Target.CanSeeInvisibility_Invisibility += EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        Target.CanSeeInvisibility_Invisibility -= EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                        break;
                    }
            }

            if (Target is WS_PlayerData.CharacterObject)
            {
                WS_PlayerData.CharacterObject argCharacter = (WS_PlayerData.CharacterObject)Target;
                WorldServiceLocator._WS_CharMovement.UpdateCell(ref argCharacter);
            }
        }

        public void SPELL_AURA_MOD_DETECT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        Target.CanSeeInvisibility_Stealth += EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        Target.CanSeeInvisibility_Stealth -= EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                        break;
                    }
            }

            if (Target is WS_PlayerData.CharacterObject)
            {
                WS_PlayerData.CharacterObject argCharacter = (WS_PlayerData.CharacterObject)Target;
                WorldServiceLocator._WS_CharMovement.UpdateCell(ref argCharacter);
            }
        }

        public void SPELL_AURA_DETECT_STEALTH(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        Target.CanSeeStealth = true;
                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        Target.CanSeeStealth = false;
                        break;
                    }
            }

            if (Target is WS_PlayerData.CharacterObject)
            {
                WS_PlayerData.CharacterObject argCharacter = (WS_PlayerData.CharacterObject)Target;
                WorldServiceLocator._WS_CharMovement.UpdateCell(ref argCharacter);
            }
        }

        public void SPELL_AURA_MOD_DISARM(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            if (!(Target is WS_PlayerData.CharacterObject))
                return;
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        if (Target is WS_PlayerData.CharacterObject)
                        {
                            ((WS_PlayerData.CharacterObject)Target).Disarmed = true;
                            ((WS_PlayerData.CharacterObject)Target).cUnitFlags = UnitFlags.UNIT_FLAG_DISARMED;
                            ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, ((WS_PlayerData.CharacterObject)Target).cUnitFlags);
                            ((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate(true);
                        }

                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        if (Target is WS_PlayerData.CharacterObject)
                        {
                            ((WS_PlayerData.CharacterObject)Target).Disarmed = false;
                            ((WS_PlayerData.CharacterObject)Target).cUnitFlags = ((WS_PlayerData.CharacterObject)Target).cUnitFlags & !UnitFlags.UNIT_FLAG_DISARMED;
                            ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, ((WS_PlayerData.CharacterObject)Target).cUnitFlags);
                            ((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate(true);
                        }

                        break;
                    }
            }
        }

        public void SPELL_AURA_SCHOOL_ABSORB(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            if (!(Caster is WS_Base.BaseUnit))
                return;
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        if (Target.AbsorbSpellLeft.ContainsKey(SpellID))
                            return;
                        Target.AbsorbSpellLeft.Add(SpellID, (uint)EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) + ((uint)EffectInfo.MiscValue << 23U));
                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        if (!Target.AbsorbSpellLeft.ContainsKey(SpellID))
                            return;
                        Target.AbsorbSpellLeft.Remove(SpellID);
                        break;
                    }
            }
        }

        public void SPELL_AURA_MOD_SHAPESHIFT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_ADD:
                    {
                        Target.RemoveAurasOfType(AuraEffects_Names.SPELL_AURA_MOD_SHAPESHIFT, ref SpellID);  // Remove other shapeshift forms
                        int argNotSpellID = 0;
                        Target.RemoveAurasOfType(AuraEffects_Names.SPELL_AURA_MOUNTED, NotSpellID: ref argNotSpellID);                  // Remove mounted spells

                        // Druid
                        if (Target is WS_PlayerData.CharacterObject && ((WS_PlayerData.CharacterObject)Target).Classe == Classes.CLASS_DRUID)
                        {
                            if (EffectInfo.MiscValue == ShapeshiftForm.FORM_AQUA || EffectInfo.MiscValue == ShapeshiftForm.FORM_CAT || EffectInfo.MiscValue == ShapeshiftForm.FORM_BEAR || EffectInfo.MiscValue == ShapeshiftForm.FORM_DIREBEAR || EffectInfo.MiscValue == ShapeshiftForm.FORM_TRAVEL || EffectInfo.MiscValue == ShapeshiftForm.FORM_FLIGHT || EffectInfo.MiscValue == ShapeshiftForm.FORM_SWIFT || EffectInfo.MiscValue == ShapeshiftForm.FORM_MOONKIN)
                            {
                                int argNotSpellID1 = 0;
                                Target.RemoveAurasOfType(26, NotSpellID: ref argNotSpellID1); // Remove Root
                                int argNotSpellID2 = 0;
                                Target.RemoveAurasOfType(33, NotSpellID: ref argNotSpellID2); // Remove Slow
                            }
                        }

                        Target.ShapeshiftForm = EffectInfo.MiscValue;
                        Target.ManaType = WorldServiceLocator._CommonGlobalFunctions.GetShapeshiftManaType(EffectInfo.MiscValue, Target.ManaType);
                        if (Target is WS_PlayerData.CharacterObject)
                        {
                            Target.Model = WorldServiceLocator._CommonGlobalFunctions.GetShapeshiftModel(EffectInfo.MiscValue, ((WS_PlayerData.CharacterObject)Target).Race, Target.Model);
                        }
                        else
                        {
                            Target.Model = WorldServiceLocator._CommonGlobalFunctions.GetShapeshiftModel(EffectInfo.MiscValue, 0, Target.Model);
                        }

                        break;
                    }

                case var case1 when case1 == AuraAction.AURA_REMOVE:
                case var case2 when case2 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        Target.ShapeshiftForm = ShapeshiftForm.FORM_NORMAL;
                        if (Target is WS_PlayerData.CharacterObject)
                        {
                            Target.ManaType = WorldServiceLocator._WS_Player_Initializator.GetClassManaType(((WS_PlayerData.CharacterObject)Target).Classe);
                            Target.Model = WorldServiceLocator._Functions.GetRaceModel(((WS_PlayerData.CharacterObject)Target).Race, ((WS_PlayerData.CharacterObject)Target).Gender);
                        }
                        else
                        {
                            Target.ManaType = ((WS_Creatures.CreatureObject)Target).CreatureInfo.ManaType;
                            Target.Model = ((WS_Creatures.CreatureObject)Target).CreatureInfo.GetRandomModel;
                        }

                        break;
                    }

                case var case3 when case3 == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }
            }

            // DONE: Send update
            if (Target is WS_PlayerData.CharacterObject)
            {
                {
                    var withBlock = (WS_PlayerData.CharacterObject)Target;
                    withBlock.SetUpdateFlag(EUnitFields.UNIT_FIELD_BYTES_2, withBlock.cBytes2);
                    withBlock.SetUpdateFlag(EUnitFields.UNIT_FIELD_DISPLAYID, withBlock.Model);
                    withBlock.SetUpdateFlag(EUnitFields.UNIT_FIELD_BYTES_0, withBlock.cBytes0);
                    if (withBlock.ManaType == ManaTypes.TYPE_MANA)
                    {
                        withBlock.SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER1, withBlock.Mana.Current);
                        withBlock.SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER1, withBlock.Mana.Maximum);
                    }
                    else if (withBlock.ManaType == ManaTypes.TYPE_RAGE)
                    {
                        withBlock.SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER2, withBlock.Rage.Current);
                        withBlock.SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER2, withBlock.Rage.Maximum);
                    }
                    else if (withBlock.ManaType == ManaTypes.TYPE_ENERGY)
                    {
                        withBlock.SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER4, withBlock.Energy.Current);
                        withBlock.SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER4, withBlock.Energy.Maximum);
                    }

                    WS_PlayerData.CharacterObject argobjCharacter = (WS_PlayerData.CharacterObject)Target;
                    WorldServiceLocator._WS_Combat.CalculateMinMaxDamage(ref argobjCharacter, WeaponAttackType.BASE_ATTACK);
                    withBlock.SetUpdateFlag(EUnitFields.UNIT_FIELD_MINDAMAGE, withBlock.Damage.Minimum);
                    withBlock.SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXDAMAGE, withBlock.Damage.Maximum);
                    withBlock.SendCharacterUpdate(true);
                    withBlock.GroupUpdateFlag = withBlock.GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_POWER_TYPE;
                    withBlock.GroupUpdateFlag = withBlock.GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_CUR_POWER;
                    withBlock.GroupUpdateFlag = withBlock.GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_MAX_POWER;
                    WorldServiceLocator._WS_PlayerHelper.InitializeTalentSpells((WS_PlayerData.CharacterObject)Target);
                }
            }
            else
            {
                var packet = new Packets.UpdatePacketClass();
                var tmpUpdate = new Packets.UpdateClass(EUnitFields.UNIT_END);
                tmpUpdate.SetUpdateFlag(EUnitFields.UNIT_FIELD_BYTES_2, Target.cBytes2);
                tmpUpdate.SetUpdateFlag(EUnitFields.UNIT_FIELD_DISPLAYID, Target.Model);
                tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, (WS_Creatures.CreatureObject)Target);
                Packets.PacketClass argpacket = packet;
                Target.SendToNearPlayers(ref argpacket);
                tmpUpdate.Dispose();
                packet.Dispose();
            }

            // TODO: The running emote is fucked up
            if (Target is WS_PlayerData.CharacterObject)
            {
                if (Action == AuraAction.AURA_ADD)
                {
                    if (EffectInfo.MiscValue == ShapeshiftForm.FORM_TRAVEL)
                        ((WS_PlayerData.CharacterObject)Target).ApplySpell(5419);
                    if (EffectInfo.MiscValue == ShapeshiftForm.FORM_CAT)
                        ((WS_PlayerData.CharacterObject)Target).ApplySpell(3025);
                    if (EffectInfo.MiscValue == ShapeshiftForm.FORM_BEAR)
                        ((WS_PlayerData.CharacterObject)Target).ApplySpell(1178);
                    if (EffectInfo.MiscValue == ShapeshiftForm.FORM_DIREBEAR)
                        ((WS_PlayerData.CharacterObject)Target).ApplySpell(9635);
                    if (EffectInfo.MiscValue == ShapeshiftForm.FORM_AQUA)
                        ((WS_PlayerData.CharacterObject)Target).ApplySpell(5421);
                    if (EffectInfo.MiscValue == ShapeshiftForm.FORM_MOONKIN)
                        ((WS_PlayerData.CharacterObject)Target).ApplySpell(24905);
                    if (EffectInfo.MiscValue == ShapeshiftForm.FORM_FLIGHT)
                    {
                        ((WS_PlayerData.CharacterObject)Target).ApplySpell(33948);
                        ((WS_PlayerData.CharacterObject)Target).ApplySpell(34764);
                    }

                    if (EffectInfo.MiscValue == ShapeshiftForm.FORM_SWIFT)
                    {
                        ((WS_PlayerData.CharacterObject)Target).ApplySpell(40121);
                        ((WS_PlayerData.CharacterObject)Target).ApplySpell(40122);
                    }

                    if (EffectInfo.MiscValue == ShapeshiftForm.FORM_BATTLESTANCE)
                        ((WS_PlayerData.CharacterObject)Target).ApplySpell(21156);
                    if (EffectInfo.MiscValue == ShapeshiftForm.FORM_BERSERKERSTANCE)
                        ((WS_PlayerData.CharacterObject)Target).ApplySpell(7381);
                    if (EffectInfo.MiscValue == ShapeshiftForm.FORM_DEFENSIVESTANCE)
                        ((WS_PlayerData.CharacterObject)Target).ApplySpell(7376);
                }
                else
                {
                    if (EffectInfo.MiscValue == ShapeshiftForm.FORM_TRAVEL)
                        Target.RemoveAuraBySpell(5419);
                    if (EffectInfo.MiscValue == ShapeshiftForm.FORM_CAT)
                        Target.RemoveAuraBySpell(3025);
                    if (EffectInfo.MiscValue == ShapeshiftForm.FORM_BEAR)
                        Target.RemoveAuraBySpell(1178);
                    if (EffectInfo.MiscValue == ShapeshiftForm.FORM_DIREBEAR)
                        Target.RemoveAuraBySpell(9635);
                    if (EffectInfo.MiscValue == ShapeshiftForm.FORM_AQUA)
                        Target.RemoveAuraBySpell(5421);
                    if (EffectInfo.MiscValue == ShapeshiftForm.FORM_MOONKIN)
                        Target.RemoveAuraBySpell(24905);
                    if (EffectInfo.MiscValue == ShapeshiftForm.FORM_FLIGHT)
                    {
                        Target.RemoveAuraBySpell(33948);
                        Target.RemoveAuraBySpell(34764);
                    }

                    if (EffectInfo.MiscValue == ShapeshiftForm.FORM_SWIFT)
                    {
                        Target.RemoveAuraBySpell(40121);
                        Target.RemoveAuraBySpell(40122);
                    }

                    if (EffectInfo.MiscValue == ShapeshiftForm.FORM_BATTLESTANCE)
                        Target.RemoveAuraBySpell(21156);
                    if (EffectInfo.MiscValue == ShapeshiftForm.FORM_BERSERKERSTANCE)
                        Target.RemoveAuraBySpell(7381);
                    if (EffectInfo.MiscValue == ShapeshiftForm.FORM_DEFENSIVESTANCE)
                        Target.RemoveAuraBySpell(7376);
                    if (EffectInfo.MiscValue == ShapeshiftForm.FORM_GHOSTWOLF)
                        Target.RemoveAuraBySpell(7376);
                }
            }
        }

        public void SPELL_AURA_PROC_TRIGGER_SPELL(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_ADD:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_REMOVE:
                case var case2 when case2 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        return;
                    }

                case var case3 when case3 == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }
            }
        }

        public void SPELL_AURA_MOD_INCREASE_SPEED(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_ADD:
                    {
                        if (Target is WS_PlayerData.CharacterObject)
                        {
                            float newSpeed = ((WS_PlayerData.CharacterObject)Target).RunSpeed;
                            newSpeed = (float)(newSpeed * (EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d + 1d));
                            ((WS_PlayerData.CharacterObject)Target).ChangeSpeedForced(ChangeSpeedType.RUN, newSpeed);
                        }
                        else if (Target is WS_Creatures.CreatureObject)
                        {
                            ((WS_Creatures.CreatureObject)Target).SetToRealPosition();
                            ((WS_Creatures.CreatureObject)Target).SpeedMod = (float)(((WS_Creatures.CreatureObject)Target).SpeedMod * (EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d + 1d));
                        }

                        break;
                    }

                case var case1 when case1 == AuraAction.AURA_REMOVE:
                case var case2 when case2 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        if (Target is WS_PlayerData.CharacterObject)
                        {
                            float newSpeed = ((WS_PlayerData.CharacterObject)Target).RunSpeed;
                            if (Caster is null)
                            {
                            }
                            // do nothing?
                            else
                            {
                                newSpeed = (float)(newSpeed / (EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d + 1d));
                            } ((WS_PlayerData.CharacterObject)Target).ChangeSpeedForced(ChangeSpeedType.RUN, newSpeed);
                        }
                        else if (Target is WS_Creatures.CreatureObject)
                        {
                            ((WS_Creatures.CreatureObject)Target).SetToRealPosition();
                            ((WS_Creatures.CreatureObject)Target).SpeedMod = (float)(((WS_Creatures.CreatureObject)Target).SpeedMod / (EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d + 1d));
                        }

                        break;
                    }

                case var case3 when case3 == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }
            }
        }

        public void SPELL_AURA_MOD_DECREASE_SPEED(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            // NOTE: Some values of EffectInfo.GetValue are in old format, new format uses (-) values

            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_ADD:
                    {
                        if (Target is WS_PlayerData.CharacterObject)
                        {
                            float newSpeed = ((WS_PlayerData.CharacterObject)Target).RunSpeed;
                            if (EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) < 0)
                            {
                                newSpeed = (float)(newSpeed / (Math.Abs(EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d) + 1d));
                            }
                            else
                            {
                                newSpeed = (float)(newSpeed / (EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d + 1d));
                            } ((WS_PlayerData.CharacterObject)Target).ChangeSpeedForced(ChangeSpeedType.RUN, newSpeed);

                            // DONE: Remove some auras when slowed
                            ((WS_PlayerData.CharacterObject)Target).RemoveAurasByInterruptFlag(SpellAuraInterruptFlags.AURA_INTERRUPT_FLAG_SLOWED);
                        }
                        else if (Target is WS_Creatures.CreatureObject)
                        {
                            ((WS_Creatures.CreatureObject)Target).SetToRealPosition();
                            if (EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) < 0)
                            {
                                ((WS_Creatures.CreatureObject)Target).SpeedMod = (float)(((WS_Creatures.CreatureObject)Target).SpeedMod / (Math.Abs(EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d) + 1d));
                            }
                            else
                            {
                                ((WS_Creatures.CreatureObject)Target).SpeedMod = (float)(((WS_Creatures.CreatureObject)Target).SpeedMod / (EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d + 1d));
                            }
                        }

                        break;
                    }

                case var case1 when case1 == AuraAction.AURA_REMOVE:
                case var case2 when case2 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        if (Target is WS_PlayerData.CharacterObject)
                        {
                            float newSpeed = ((WS_PlayerData.CharacterObject)Target).RunSpeed;
                            if (EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) < 0)
                            {
                                newSpeed = (float)(newSpeed * (Math.Abs(EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d) + 1d));
                            }
                            else
                            {
                                newSpeed = (float)(newSpeed * (EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d + 1d));
                            } ((WS_PlayerData.CharacterObject)Target).ChangeSpeedForced(ChangeSpeedType.RUN, newSpeed);
                        }
                        else if (Target is WS_Creatures.CreatureObject)
                        {
                            ((WS_Creatures.CreatureObject)Target).SetToRealPosition();
                            if (EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) < 0)
                            {
                                ((WS_Creatures.CreatureObject)Target).SpeedMod = (float)(((WS_Creatures.CreatureObject)Target).SpeedMod * (Math.Abs(EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d) + 1d));
                            }
                            else
                            {
                                ((WS_Creatures.CreatureObject)Target).SpeedMod = (float)(((WS_Creatures.CreatureObject)Target).SpeedMod * (EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d + 1d));
                            }
                        }

                        break;
                    }

                case var case3 when case3 == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }
            }
        }

        public void SPELL_AURA_MOD_INCREASE_SPEED_ALWAYS(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            if (!(Target is WS_PlayerData.CharacterObject))
                return;
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_ADD:
                    {
                        float newSpeed = ((WS_PlayerData.CharacterObject)Target).RunSpeed;
                        newSpeed = (float)(newSpeed * (EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d + 1d));
                        ((WS_PlayerData.CharacterObject)Target).ChangeSpeedForced(ChangeSpeedType.RUN, newSpeed);
                        break;
                    }

                case var case1 when case1 == AuraAction.AURA_REMOVE:
                case var case2 when case2 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        float newSpeed = ((WS_PlayerData.CharacterObject)Target).RunSpeed;
                        newSpeed = (float)(newSpeed / (EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d + 1d));
                        ((WS_PlayerData.CharacterObject)Target).ChangeSpeedForced(ChangeSpeedType.RUN, newSpeed);
                        break;
                    }

                case var case3 when case3 == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }
            }
        }

        public void SPELL_AURA_MOD_INCREASE_MOUNTED_SPEED(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            if (!(Target is WS_PlayerData.CharacterObject))
                return;
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_ADD:
                    {
                        float newSpeed = ((WS_PlayerData.CharacterObject)Target).RunSpeed;
                        newSpeed = (float)(newSpeed * (EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d + 1d));
                        ((WS_PlayerData.CharacterObject)Target).ChangeSpeedForced(ChangeSpeedType.RUN, newSpeed);
                        break;
                    }

                case var case1 when case1 == AuraAction.AURA_REMOVE:
                case var case2 when case2 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        float newSpeed = ((WS_PlayerData.CharacterObject)Target).RunSpeed;
                        newSpeed = (float)(newSpeed / (EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d + 1d));
                        ((WS_PlayerData.CharacterObject)Target).ChangeSpeedForced(ChangeSpeedType.RUN, newSpeed);
                        break;
                    }

                case var case3 when case3 == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }
            }
        }

        public void SPELL_AURA_MOD_INCREASE_MOUNTED_SPEED_ALWAYS(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            if (!(Target is WS_PlayerData.CharacterObject))
                return;
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_ADD:
                    {
                        float newSpeed = ((WS_PlayerData.CharacterObject)Target).RunSpeed;
                        newSpeed = (float)(newSpeed * (EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d + 1d));
                        ((WS_PlayerData.CharacterObject)Target).ChangeSpeedForced(ChangeSpeedType.RUN, newSpeed);
                        break;
                    }

                case var case1 when case1 == AuraAction.AURA_REMOVE:
                case var case2 when case2 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        float newSpeed = ((WS_PlayerData.CharacterObject)Target).RunSpeed;
                        newSpeed = (float)(newSpeed / (EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d + 1d));
                        ((WS_PlayerData.CharacterObject)Target).ChangeSpeedForced(ChangeSpeedType.RUN, newSpeed);
                        break;
                    }

                case var case3 when case3 == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }
            }
        }

        public void SPELL_AURA_MOD_INCREASE_SWIM_SPEED(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            if (!(Target is WS_PlayerData.CharacterObject))
                return;
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_ADD:
                    {
                        float newSpeed = ((WS_PlayerData.CharacterObject)Target).SwimSpeed;
                        newSpeed = (float)(newSpeed * (EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d + 1d));
                        ((WS_PlayerData.CharacterObject)Target).SwimSpeed = newSpeed;
                        ((WS_PlayerData.CharacterObject)Target).ChangeSpeedForced(ChangeSpeedType.SWIM, newSpeed);
                        break;
                    }

                case var case1 when case1 == AuraAction.AURA_REMOVE:
                case var case2 when case2 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        float newSpeed = ((WS_PlayerData.CharacterObject)Target).SwimSpeed;
                        if (Caster is null)
                        {
                        }
                        // do nothing?
                        else
                        {
                            newSpeed = (float)(newSpeed / (EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d + 1d));
                        } ((WS_PlayerData.CharacterObject)Target).SwimSpeed = newSpeed;
                        ((WS_PlayerData.CharacterObject)Target).ChangeSpeedForced(ChangeSpeedType.SWIM, newSpeed);
                        break;
                    }

                case var case3 when case3 == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }
            }
        }

        public void SPELL_AURA_MOUNTED(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_ADD:
                    {
                        int argNotSpellID = 0;
                        Target.RemoveAurasOfType(AuraEffects_Names.SPELL_AURA_MOD_SHAPESHIFT, NotSpellID: ref argNotSpellID);       // Remove shapeshift forms
                        Target.RemoveAurasOfType(AuraEffects_Names.SPELL_AURA_MOUNTED, ref SpellID);     // Remove other mounted spells
                        Target.RemoveAurasByInterruptFlag(SpellAuraInterruptFlags.AURA_INTERRUPT_FLAG_MOUNTING);
                        if (!WorldServiceLocator._WorldServer.CREATURESDatabase.ContainsKey(EffectInfo.MiscValue))
                        {
                            var creature = new CreatureInfo(EffectInfo.MiscValue);
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

                case var case1 when case1 == AuraAction.AURA_REMOVE:
                case var case2 when case2 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        Target.Mount = 0;
                        Target.RemoveAurasByInterruptFlag(SpellAuraInterruptFlags.AURA_INTERRUPT_FLAG_NOT_MOUNTED);
                        break;
                    }

                case var case3 when case3 == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }
            }

            // DONE: Model update
            if (Target is WS_PlayerData.CharacterObject)
            {
                ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_MOUNTDISPLAYID, Target.Mount);
                ((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate(true);
            }
            else
            {
                var packet = new Packets.UpdatePacketClass();
                var tmpUpdate = new Packets.UpdateClass(EUnitFields.UNIT_END);
                tmpUpdate.SetUpdateFlag(EUnitFields.UNIT_FIELD_MOUNTDISPLAYID, Target.Mount);
                tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, (WS_Creatures.CreatureObject)Target);
                Packets.PacketClass argpacket = packet;
                Target.SendToNearPlayers(ref argpacket);
                tmpUpdate.Dispose();
                packet.Dispose();
            }
        }

        public void SPELL_AURA_MOD_HASTE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            if (!(Target is WS_PlayerData.CharacterObject))
                return;
            {
                var withBlock = (WS_PlayerData.CharacterObject)Target;
                switch (Action)
                {
                    case var @case when @case == AuraAction.AURA_ADD:
                        {
                            withBlock.AttackTimeMods[0] = (float)(withBlock.AttackTimeMods[0] / (EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d + 1d));
                            withBlock.AttackTimeMods[1] = (float)(withBlock.AttackTimeMods[1] / (EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d + 1d));
                            break;
                        }

                    case var case1 when case1 == AuraAction.AURA_REMOVE:
                    case var case2 when case2 == AuraAction.AURA_REMOVEBYDURATION:
                        {
                            withBlock.AttackTimeMods[0] = (float)(withBlock.AttackTimeMods[0] * (EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d + 1d));
                            withBlock.AttackTimeMods[1] = (float)(withBlock.AttackTimeMods[1] * (EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d + 1d));
                            break;
                        }

                    case var case3 when case3 == AuraAction.AURA_UPDATE:
                        {
                            return;
                        }
                }

                withBlock.SetUpdateFlag(EUnitFields.UNIT_FIELD_BASEATTACKTIME, withBlock.AttackTime(0));
                withBlock.SetUpdateFlag(EUnitFields.UNIT_FIELD_RANGEDATTACKTIME, withBlock.AttackTime(1));
                withBlock.SendCharacterUpdate(false);
            }
        }

        public void SPELL_AURA_MOD_RANGED_HASTE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            if (!(Target is WS_PlayerData.CharacterObject))
                return;
            {
                var withBlock = (WS_PlayerData.CharacterObject)Target;
                switch (Action)
                {
                    case var @case when @case == AuraAction.AURA_ADD:
                        {
                            withBlock.AttackTimeMods[2] = (float)(withBlock.AttackTimeMods[2] / (EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d + 1d));
                            break;
                        }

                    case var case1 when case1 == AuraAction.AURA_REMOVE:
                    case var case2 when case2 == AuraAction.AURA_REMOVEBYDURATION:
                        {
                            withBlock.AttackTimeMods[2] = (float)(withBlock.AttackTimeMods[2] * (EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d + 1d));
                            break;
                        }

                    case var case3 when case3 == AuraAction.AURA_UPDATE:
                        {
                            return;
                        }
                }

                withBlock.SetUpdateFlag(EUnitFields.UNIT_FIELD_RANGEDATTACKTIME, withBlock.AttackTime(2));
                withBlock.SendCharacterUpdate(false);
            }
        }

        public void SPELL_AURA_MOD_RANGED_AMMO_HASTE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            if (!(Target is WS_PlayerData.CharacterObject))
                return;
            {
                var withBlock = (WS_PlayerData.CharacterObject)Target;
                switch (Action)
                {
                    case var @case when @case == AuraAction.AURA_ADD:
                        {
                            withBlock.AmmoMod = (float)(withBlock.AmmoMod * (EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d + 1d));
                            break;
                        }

                    case var case1 when case1 == AuraAction.AURA_REMOVE:
                    case var case2 when case2 == AuraAction.AURA_REMOVEBYDURATION:
                        {
                            withBlock.AmmoMod = (float)(withBlock.AmmoMod / (EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d + 1d));
                            break;
                        }

                    case var case3 when case3 == AuraAction.AURA_UPDATE:
                        {
                            return;
                        }
                }

                WS_PlayerData.CharacterObject argobjCharacter = (WS_PlayerData.CharacterObject)Target;
                WorldServiceLocator._WS_Combat.CalculateMinMaxDamage(ref argobjCharacter, WeaponAttackType.RANGED_ATTACK);
                withBlock.SendCharacterUpdate(false);
            }
        }

        public void SPELL_AURA_MOD_ROOT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_ADD:
                    {
                        // Target.cUnitFlags = Target.cUnitFlags Or UnitFlags.UNIT_FLAG_ROOTED
                        if (Target is WS_PlayerData.CharacterObject)
                        {
                            ((WS_PlayerData.CharacterObject)Target).SetMoveRoot();
                            ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_TARGET, Conversions.ToLong(0));
                        }
                        else if (Target is WS_Creatures.CreatureObject)
                        {
                            if (((WS_Creatures.CreatureObject)Target).aiScript is object)
                            {
                                WS_Base.BaseUnit argAttacker = (WS_Base.BaseUnit)Caster;
                                ((WS_Creatures.CreatureObject)Target).aiScript.OnGenerateHate(ref argAttacker, 1);
                            } ((WS_Creatures.CreatureObject)Target).StopMoving();
                        }

                        break;
                    }

                case var case1 when case1 == AuraAction.AURA_REMOVE:
                case var case2 when case2 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        // Target.cUnitFlags = Target.cUnitFlags And (Not UnitFlags.UNIT_FLAG_ROOTED)
                        if (Target is WS_PlayerData.CharacterObject)
                        {
                            ((WS_PlayerData.CharacterObject)Target).SetMoveUnroot();
                        }
                        else if (Target is WS_Creatures.CreatureObject)
                        {
                            ((WS_Creatures.CreatureObject)Target).StopMoving();
                        }

                        break;
                    }

                case var case3 when case3 == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }
            }

            // DONE: Send update
            if (Target is WS_PlayerData.CharacterObject)
            {
                ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, Target.cUnitFlags);
                ((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate(true);
            }
            else
            {
                var packet = new Packets.UpdatePacketClass();
                var tmpUpdate = new Packets.UpdateClass(EUnitFields.UNIT_END);
                tmpUpdate.SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, Target.cUnitFlags);
                tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, (WS_Creatures.CreatureObject)Target);
                Packets.PacketClass argpacket = packet;
                Target.SendToNearPlayers(ref argpacket);
                tmpUpdate.Dispose();
                packet.Dispose();
            }
        }

        public void SPELL_AURA_MOD_STUN(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_ADD:
                    {
                        Target.cUnitFlags = Target.cUnitFlags | UnitFlags.UNIT_FLAG_STUNTED;
                        if (Target is WS_PlayerData.CharacterObject)
                        {
                            ((WS_PlayerData.CharacterObject)Target).SetMoveRoot();
                            ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_TARGET, 0UL);
                        }
                        else if (Target is WS_Creatures.CreatureObject)
                        {
                            ((WS_Creatures.CreatureObject)Target).StopMoving();
                            if (((WS_Creatures.CreatureObject)Target).aiScript is object)
                            {
                                WS_Base.BaseUnit argAttacker = (WS_Base.BaseUnit)Caster;
                                ((WS_Creatures.CreatureObject)Target).aiScript.OnGenerateHate(ref argAttacker, 1);
                            }
                        }

                        break;
                    }

                case var case1 when case1 == AuraAction.AURA_REMOVE:
                case var case2 when case2 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        Target.cUnitFlags = Target.cUnitFlags & !UnitFlags.UNIT_FLAG_STUNTED;
                        if (Target is WS_PlayerData.CharacterObject)
                        {
                            ((WS_PlayerData.CharacterObject)Target).SetMoveUnroot();
                        }

                        break;
                    }

                case var case3 when case3 == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }
            }

            // DONE: Send update
            if (Target is WS_PlayerData.CharacterObject)
            {
                ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, Target.cUnitFlags);
                ((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate(true);
            }
            else
            {
                var packet = new Packets.UpdatePacketClass();
                var tmpUpdate = new Packets.UpdateClass(EUnitFields.UNIT_END);
                tmpUpdate.SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, Target.cUnitFlags);
                tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, (WS_Creatures.CreatureObject)Target);
                Packets.PacketClass argpacket = packet;
                Target.SendToNearPlayers(ref argpacket);
                tmpUpdate.Dispose();
                packet.Dispose();
            }
        }

        public void SPELL_AURA_MOD_FEAR(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            var response = new Packets.PacketClass(OPCODES.SMSG_DEATH_NOTIFY_OBSOLETE);
            response.AddPackGUID(Target.GUID);
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_ADD:
                    {
                        Target.cUnitFlags = Target.cUnitFlags | UnitFlags.UNIT_FLAG_FLEEING;
                        response.AddInt8(0);
                        break;
                    }

                case var case1 when case1 == AuraAction.AURA_REMOVE:
                case var case2 when case2 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        Target.cUnitFlags = Target.cUnitFlags & !UnitFlags.UNIT_FLAG_FLEEING;
                        response.AddInt8(1);
                        break;
                    }

                case var case3 when case3 == AuraAction.AURA_UPDATE:
                    {
                        // TODO: Random movement
                        return;
                    }
            }

            // DONE: Send update
            if (Target is WS_PlayerData.CharacterObject)
            {
                ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, Target.cUnitFlags);
                ((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate(true);
                ((WS_PlayerData.CharacterObject)Target).client.Send(ref response);
            }
            else
            {
                var packet = new Packets.UpdatePacketClass();
                var tmpUpdate = new Packets.UpdateClass(EUnitFields.UNIT_END);
                tmpUpdate.SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, Target.cUnitFlags);
                tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, (WS_Creatures.CreatureObject)Target);
                Packets.PacketClass argpacket = packet;
                Target.SendToNearPlayers(ref argpacket);
                tmpUpdate.Dispose();
                packet.Dispose();
            }

            response.Dispose();
        }

        public void SPELL_AURA_SAFE_FALL(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        var packet = new Packets.PacketClass(OPCODES.SMSG_MOVE_FEATHER_FALL);
                        packet.AddPackGUID(Target.GUID);
                        Target.SendToNearPlayers(ref packet);
                        packet.Dispose();
                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        var packet = new Packets.PacketClass(OPCODES.SMSG_MOVE_NORMAL_FALL);
                        packet.AddPackGUID(Target.GUID);
                        Target.SendToNearPlayers(ref packet);
                        packet.Dispose();
                        break;
                    }
            }
        }

        public void SPELL_AURA_FEATHER_FALL(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        var packet = new Packets.PacketClass(OPCODES.SMSG_MOVE_FEATHER_FALL);
                        packet.AddPackGUID(Target.GUID);
                        Target.SendToNearPlayers(ref packet);
                        packet.Dispose();
                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        var packet = new Packets.PacketClass(OPCODES.SMSG_MOVE_NORMAL_FALL);
                        packet.AddPackGUID(Target.GUID);
                        Target.SendToNearPlayers(ref packet);
                        packet.Dispose();
                        break;
                    }
            }
        }

        public void SPELL_AURA_WATER_WALK(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        var packet = new Packets.PacketClass(OPCODES.SMSG_MOVE_WATER_WALK);
                        packet.AddPackGUID(Target.GUID);
                        Target.SendToNearPlayers(ref packet);
                        packet.Dispose();
                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        var packet = new Packets.PacketClass(OPCODES.SMSG_MOVE_LAND_WALK);
                        packet.AddPackGUID(Target.GUID);
                        Target.SendToNearPlayers(ref packet);
                        packet.Dispose();
                        break;
                    }
            }
        }

        public void SPELL_AURA_HOVER(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        var packet = new Packets.PacketClass(OPCODES.SMSG_MOVE_SET_HOVER);
                        packet.AddPackGUID(Target.GUID);
                        Target.SendToNearPlayers(ref packet);
                        packet.Dispose();
                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        var packet = new Packets.PacketClass(OPCODES.SMSG_MOVE_UNSET_HOVER);
                        packet.AddPackGUID(Target.GUID);
                        Target.SendToNearPlayers(ref packet);
                        packet.Dispose();
                        break;
                    }
            }
        }

        public void SPELL_AURA_WATER_BREATHING(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            if (!(Target is WS_PlayerData.CharacterObject))
                return;
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        ((WS_PlayerData.CharacterObject)Target).underWaterBreathing = true;
                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        ((WS_PlayerData.CharacterObject)Target).underWaterBreathing = false;
                        break;
                    }
            }
        }

        public void SPELL_AURA_ADD_FLAT_MODIFIER(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            if (!(Target is WS_PlayerData.CharacterObject))
                return;
            if (EffectInfo.MiscValue > 32)
                return;
            SpellModOp op = EffectInfo.MiscValue;
            int value = EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
            int mask = EffectInfo.ItemType;
            if (Action == AuraAction.AURA_ADD)
            {
                // TODO: Add spell modifier!

                var send_val = default(ushort);
                var send_mark = default(ushort);
                short tmpval = (short)EffectInfo.valueBase;
                uint shiftdata = 0x1U;
                if (tmpval != 0)
                {
                    if (tmpval > 0)
                    {
                        send_val = (ushort)(tmpval + 1);
                        send_mark = 0x0;
                    }
                    else
                    {
                        send_val = (ushort)(0xFFFFU + tmpval + 2 & 0xFFFFU);
                        send_mark = 0xFFFF;
                    }
                }

                for (int eff = 0; eff <= 31; eff++)
                {
                    if (Conversions.ToBoolean(mask & shiftdata))
                    {
                        var packet = new Packets.PacketClass(OPCODES.SMSG_SET_FLAT_SPELL_MODIFIER);
                        packet.AddInt8((byte)eff);
                        packet.AddInt8((byte)op);
                        packet.AddUInt16(send_val);
                        packet.AddUInt16(send_mark);
                        ((WS_PlayerData.CharacterObject)Caster).client.Send(ref packet);
                        packet.Dispose();
                    }

                    shiftdata = shiftdata << 1;
                }
            }
            else if (Action == AuraAction.AURA_REMOVE || Action == AuraAction.AURA_REMOVEBYDURATION)
            {
                // TODO: Remove spell modifier!
            }
        }

        public void SPELL_AURA_ADD_PCT_MODIFIER(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            if (!(Target is WS_PlayerData.CharacterObject))
                return;
            if (EffectInfo.MiscValue > 32)
                return;
            SpellModOp op = EffectInfo.MiscValue;
            int value = EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
            int mask = EffectInfo.ItemType;
            if (Action == AuraAction.AURA_ADD)
            {
                // TODO: Add spell modifier!

                var send_val = default(ushort);
                var send_mark = default(ushort);
                short tmpval = (short)EffectInfo.valueBase;
                uint shiftdata = 0x1U;
                if (tmpval != 0)
                {
                    if (tmpval > 0)
                    {
                        send_val = (ushort)(tmpval + 1);
                        send_mark = 0x0;
                    }
                    else
                    {
                        send_val = (ushort)(0xFFFF + tmpval + 2 & 0xFFFF);
                        send_mark = 0xFFFF;
                    }
                }

                for (int eff = 0; eff <= 31; eff++)
                {
                    if (Conversions.ToBoolean(mask & shiftdata))
                    {
                        var packet = new Packets.PacketClass(OPCODES.SMSG_SET_PCT_SPELL_MODIFIER);
                        packet.AddInt8((byte)eff);
                        packet.AddInt8(op);
                        packet.AddUInt16(send_val);
                        packet.AddUInt16(send_mark);
                        ((WS_PlayerData.CharacterObject)Caster).client.Send(ref packet);
                        packet.Dispose();
                    }

                    shiftdata = shiftdata << 1;
                }
            }
            else if (Action == AuraAction.AURA_REMOVE || Action == AuraAction.AURA_REMOVEBYDURATION)
            {
                // TODO: Remove spell modifier!
            }
        }

        // TODO: Update values based on stats
        public void SPELL_AURA_MOD_STAT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            if (Action == AuraAction.AURA_UPDATE)
                return;
            if (!(Target is WS_PlayerData.CharacterObject))
                return;
            int value = EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
            int value_sign = value;
            if (Action == AuraAction.AURA_REMOVE)
                value = -value;
            switch (EffectInfo.MiscValue)
            {
                case -1:
                    {
                        ((WS_PlayerData.CharacterObject)Target).Strength.Base = (int)(((WS_PlayerData.CharacterObject)Target).Strength.Base / ((WS_PlayerData.CharacterObject)Target).Strength.Modifier);
                        ((WS_PlayerData.CharacterObject)Target).Strength.Base += value;
                        ((WS_PlayerData.CharacterObject)Target).Strength.Base = (int)(((WS_PlayerData.CharacterObject)Target).Strength.Base * ((WS_PlayerData.CharacterObject)Target).Strength.Modifier);
                        ((WS_PlayerData.CharacterObject)Target).Agility.Base = (int)(((WS_PlayerData.CharacterObject)Target).Agility.Base / ((WS_PlayerData.CharacterObject)Target).Agility.Modifier);
                        ((WS_PlayerData.CharacterObject)Target).Agility.Base += value;
                        ((WS_PlayerData.CharacterObject)Target).Agility.Base = (int)(((WS_PlayerData.CharacterObject)Target).Agility.Base * ((WS_PlayerData.CharacterObject)Target).Agility.Modifier);
                        ((WS_PlayerData.CharacterObject)Target).Stamina.Base = (int)(((WS_PlayerData.CharacterObject)Target).Stamina.Base / ((WS_PlayerData.CharacterObject)Target).Stamina.Modifier);
                        ((WS_PlayerData.CharacterObject)Target).Stamina.Base += value;
                        ((WS_PlayerData.CharacterObject)Target).Stamina.Base = (int)(((WS_PlayerData.CharacterObject)Target).Stamina.Base * ((WS_PlayerData.CharacterObject)Target).Stamina.Modifier);
                        ((WS_PlayerData.CharacterObject)Target).Spirit.Base = (int)(((WS_PlayerData.CharacterObject)Target).Spirit.Base / ((WS_PlayerData.CharacterObject)Target).Spirit.Modifier);
                        ((WS_PlayerData.CharacterObject)Target).Spirit.Base += value;
                        ((WS_PlayerData.CharacterObject)Target).Spirit.Base = (int)(((WS_PlayerData.CharacterObject)Target).Spirit.Base * ((WS_PlayerData.CharacterObject)Target).Spirit.Modifier);
                        ((WS_PlayerData.CharacterObject)Target).Intellect.Base = (int)(((WS_PlayerData.CharacterObject)Target).Intellect.Base / ((WS_PlayerData.CharacterObject)Target).Intellect.Modifier);
                        ((WS_PlayerData.CharacterObject)Target).Intellect.Base += value;
                        ((WS_PlayerData.CharacterObject)Target).Intellect.Base = (int)(((WS_PlayerData.CharacterObject)Target).Intellect.Base * ((WS_PlayerData.CharacterObject)Target).Intellect.Modifier);
                        if (value_sign > 0)
                        {
                            ((WS_PlayerData.CharacterObject)Target).Strength.PositiveBonus = (short)(((WS_PlayerData.CharacterObject)Target).Strength.PositiveBonus + value);
                            ((WS_PlayerData.CharacterObject)Target).Agility.PositiveBonus = (short)(((WS_PlayerData.CharacterObject)Target).Agility.PositiveBonus + value);
                            ((WS_PlayerData.CharacterObject)Target).Stamina.PositiveBonus = (short)(((WS_PlayerData.CharacterObject)Target).Stamina.PositiveBonus + value);
                            ((WS_PlayerData.CharacterObject)Target).Spirit.PositiveBonus = (short)(((WS_PlayerData.CharacterObject)Target).Spirit.PositiveBonus + value);
                            ((WS_PlayerData.CharacterObject)Target).Intellect.PositiveBonus = (short)(((WS_PlayerData.CharacterObject)Target).Intellect.PositiveBonus + value);
                        }
                        else
                        {
                            ((WS_PlayerData.CharacterObject)Target).Strength.NegativeBonus = (short)(((WS_PlayerData.CharacterObject)Target).Strength.NegativeBonus - value);
                            ((WS_PlayerData.CharacterObject)Target).Agility.NegativeBonus = (short)(((WS_PlayerData.CharacterObject)Target).Agility.NegativeBonus - value);
                            ((WS_PlayerData.CharacterObject)Target).Stamina.NegativeBonus = (short)(((WS_PlayerData.CharacterObject)Target).Stamina.NegativeBonus - value);
                            ((WS_PlayerData.CharacterObject)Target).Spirit.NegativeBonus = (short)(((WS_PlayerData.CharacterObject)Target).Spirit.NegativeBonus - value);
                            ((WS_PlayerData.CharacterObject)Target).Intellect.NegativeBonus = (short)(((WS_PlayerData.CharacterObject)Target).Intellect.NegativeBonus - value);
                        }

                        break;
                    }

                case 0:
                    {
                        ((WS_PlayerData.CharacterObject)Target).Strength.Base = (int)(((WS_PlayerData.CharacterObject)Target).Strength.Base / ((WS_PlayerData.CharacterObject)Target).Strength.Modifier);
                        ((WS_PlayerData.CharacterObject)Target).Strength.Base += value;
                        ((WS_PlayerData.CharacterObject)Target).Strength.Base = (int)(((WS_PlayerData.CharacterObject)Target).Strength.Base * ((WS_PlayerData.CharacterObject)Target).Strength.Modifier);
                        if (value_sign > 0)
                        {
                            ((WS_PlayerData.CharacterObject)Target).Strength.PositiveBonus = (short)(((WS_PlayerData.CharacterObject)Target).Strength.PositiveBonus + value);
                        }
                        else
                        {
                            ((WS_PlayerData.CharacterObject)Target).Strength.NegativeBonus = (short)(((WS_PlayerData.CharacterObject)Target).Strength.NegativeBonus - value);
                        }

                        break;
                    }

                case 1:
                    {
                        ((WS_PlayerData.CharacterObject)Target).Agility.Base = (int)(((WS_PlayerData.CharacterObject)Target).Agility.Base / ((WS_PlayerData.CharacterObject)Target).Agility.Modifier);
                        ((WS_PlayerData.CharacterObject)Target).Agility.Base += value;
                        ((WS_PlayerData.CharacterObject)Target).Agility.Base = (int)(((WS_PlayerData.CharacterObject)Target).Agility.Base * ((WS_PlayerData.CharacterObject)Target).Agility.Modifier);
                        if (value_sign > 0)
                        {
                            ((WS_PlayerData.CharacterObject)Target).Agility.PositiveBonus = (short)(((WS_PlayerData.CharacterObject)Target).Agility.PositiveBonus + value);
                        }
                        else
                        {
                            ((WS_PlayerData.CharacterObject)Target).Agility.NegativeBonus = (short)(((WS_PlayerData.CharacterObject)Target).Agility.NegativeBonus - value);
                        }

                        break;
                    }

                case 2:
                    {
                        ((WS_PlayerData.CharacterObject)Target).Stamina.Base = (int)(((WS_PlayerData.CharacterObject)Target).Stamina.Base / ((WS_PlayerData.CharacterObject)Target).Stamina.Modifier);
                        ((WS_PlayerData.CharacterObject)Target).Stamina.Base += value;
                        ((WS_PlayerData.CharacterObject)Target).Stamina.Base = (int)(((WS_PlayerData.CharacterObject)Target).Stamina.Base * ((WS_PlayerData.CharacterObject)Target).Stamina.Modifier);
                        if (value_sign > 0)
                        {
                            ((WS_PlayerData.CharacterObject)Target).Stamina.PositiveBonus = (short)(((WS_PlayerData.CharacterObject)Target).Stamina.PositiveBonus + value);
                        }
                        else
                        {
                            ((WS_PlayerData.CharacterObject)Target).Stamina.NegativeBonus = (short)(((WS_PlayerData.CharacterObject)Target).Stamina.NegativeBonus - value);
                        }

                        break;
                    }

                case 3:
                    {
                        ((WS_PlayerData.CharacterObject)Target).Intellect.Base = (int)(((WS_PlayerData.CharacterObject)Target).Intellect.Base / ((WS_PlayerData.CharacterObject)Target).Intellect.Modifier);
                        ((WS_PlayerData.CharacterObject)Target).Intellect.Base += value;
                        ((WS_PlayerData.CharacterObject)Target).Intellect.Base = (int)(((WS_PlayerData.CharacterObject)Target).Intellect.Base * ((WS_PlayerData.CharacterObject)Target).Intellect.Modifier);
                        if (value_sign > 0)
                        {
                            ((WS_PlayerData.CharacterObject)Target).Intellect.PositiveBonus = (short)(((WS_PlayerData.CharacterObject)Target).Intellect.PositiveBonus + value);
                        }
                        else
                        {
                            ((WS_PlayerData.CharacterObject)Target).Intellect.NegativeBonus = (short)(((WS_PlayerData.CharacterObject)Target).Intellect.NegativeBonus - value);
                        }

                        break;
                    }

                case 4:
                    {
                        ((WS_PlayerData.CharacterObject)Target).Spirit.Base = (int)(((WS_PlayerData.CharacterObject)Target).Spirit.Base / ((WS_PlayerData.CharacterObject)Target).Spirit.Modifier);
                        ((WS_PlayerData.CharacterObject)Target).Spirit.Base += value;
                        ((WS_PlayerData.CharacterObject)Target).Spirit.Base = (int)(((WS_PlayerData.CharacterObject)Target).Spirit.Base * ((WS_PlayerData.CharacterObject)Target).Spirit.Modifier);
                        if (value_sign > 0)
                        {
                            ((WS_PlayerData.CharacterObject)Target).Spirit.PositiveBonus = (short)(((WS_PlayerData.CharacterObject)Target).Spirit.PositiveBonus + value);
                        }
                        else
                        {
                            ((WS_PlayerData.CharacterObject)Target).Spirit.NegativeBonus = (short)(((WS_PlayerData.CharacterObject)Target).Spirit.NegativeBonus - value);
                        }

                        break;
                    }
            } ((WS_PlayerData.CharacterObject)Target).Life.Bonus = (((WS_PlayerData.CharacterObject)Target).Stamina.Base - 18) * 10;
            ((WS_PlayerData.CharacterObject)Target).Mana.Bonus = (((WS_PlayerData.CharacterObject)Target).Intellect.Base - 18) * 15;
            ((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag = ((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_MAX_HP | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_MAX_POWER;
            ((WS_PlayerData.CharacterObject)Target).Resistances[DamageTypes.DMG_PHYSICAL].Base += value * 2;
            ((WS_PlayerData.CharacterObject)Target).UpdateManaRegen();
            WS_PlayerData.CharacterObject argobjCharacter = (WS_PlayerData.CharacterObject)Target;
            WorldServiceLocator._WS_Combat.CalculateMinMaxDamage(ref argobjCharacter, WeaponAttackType.BASE_ATTACK);
            WS_PlayerData.CharacterObject argobjCharacter1 = (WS_PlayerData.CharacterObject)Target;
            WorldServiceLocator._WS_Combat.CalculateMinMaxDamage(ref argobjCharacter1, WeaponAttackType.OFF_ATTACK);
            WS_PlayerData.CharacterObject argobjCharacter2 = (WS_PlayerData.CharacterObject)Target;
            WorldServiceLocator._WS_Combat.CalculateMinMaxDamage(ref argobjCharacter2, WeaponAttackType.RANGED_ATTACK);
            ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_STRENGTH, ((WS_PlayerData.CharacterObject)Target).Strength.Base);
            ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_AGILITY, ((WS_PlayerData.CharacterObject)Target).Agility.Base);
            ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_STAMINA, ((WS_PlayerData.CharacterObject)Target).Stamina.Base);
            ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_SPIRIT, ((WS_PlayerData.CharacterObject)Target).Spirit.Base);
            ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_INTELLECT, ((WS_PlayerData.CharacterObject)Target).Intellect.Base);
            // CType(Target, CharacterObject).SetUpdateFlag(EUnitFields.UNIT_FIELD_POSSTAT0, CType(CType(Target, CharacterObject).Strength.PositiveBonus, Integer))
            // CType(Target, CharacterObject).SetUpdateFlag(EUnitFields.UNIT_FIELD_POSSTAT1, CType(CType(Target, CharacterObject).Agility.PositiveBonus, Integer))
            // CType(Target, CharacterObject).SetUpdateFlag(EUnitFields.UNIT_FIELD_POSSTAT2, CType(CType(Target, CharacterObject).Stamina.PositiveBonus, Integer))
            // CType(Target, CharacterObject).SetUpdateFlag(EUnitFields.UNIT_FIELD_POSSTAT3, CType(CType(Target, CharacterObject).Intellect.PositiveBonus, Integer))
            // CType(Target, CharacterObject).SetUpdateFlag(EUnitFields.UNIT_FIELD_POSSTAT4, CType(CType(Target, CharacterObject).Spirit.PositiveBonus, Integer))
            // CType(Target, CharacterObject).SetUpdateFlag(EUnitFields.UNIT_FIELD_NEGSTAT0, CType(CType(Target, CharacterObject).Strength.NegativeBonus, Integer))
            // CType(Target, CharacterObject).SetUpdateFlag(EUnitFields.UNIT_FIELD_NEGSTAT1, CType(CType(Target, CharacterObject).Agility.NegativeBonus, Integer))
            // CType(Target, CharacterObject).SetUpdateFlag(EUnitFields.UNIT_FIELD_NEGSTAT2, CType(CType(Target, CharacterObject).Stamina.NegativeBonus, Integer))
            // CType(Target, CharacterObject).SetUpdateFlag(EUnitFields.UNIT_FIELD_NEGSTAT3, CType(CType(Target, CharacterObject).Intellect.NegativeBonus, Integer))
            // CType(Target, CharacterObject).SetUpdateFlag(EUnitFields.UNIT_FIELD_NEGSTAT4, CType(CType(Target, CharacterObject).Spirit.NegativeBonus, Integer))
            ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_HEALTH, ((WS_PlayerData.CharacterObject)Target).Life.Current);
            ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXHEALTH, ((WS_PlayerData.CharacterObject)Target).Life.Maximum);
            if (WorldServiceLocator._WS_Player_Initializator.GetClassManaType(((WS_PlayerData.CharacterObject)Target).Classe) == ManaTypes.TYPE_MANA)
            {
                ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER1, ((WS_PlayerData.CharacterObject)Target).Mana.Current);
                ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXPOWER1, ((WS_PlayerData.CharacterObject)Target).Mana.Maximum);
            } ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_PHYSICAL, ((WS_PlayerData.CharacterObject)Target).Resistances[DamageTypes.DMG_PHYSICAL].Base);
            ((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate(false);
            ((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag = ((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_MAX_HP;
            ((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag = ((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_MAX_POWER;
        }

        public void SPELL_AURA_MOD_STAT_PERCENT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            if (Action == AuraAction.AURA_UPDATE)
                return;
            if (!(Target is WS_PlayerData.CharacterObject))
                return;

            // TODO: This is only supposed to add % of the base stat, not the entire one.

            float value = (float)(EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d);
            int value_sign = EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
            if (Action == AuraAction.AURA_REMOVE)
                value = -value;
            short OldStr = (short)((WS_PlayerData.CharacterObject)Target).Strength.Base;
            short OldAgi = (short)((WS_PlayerData.CharacterObject)Target).Agility.Base;
            short OldSta = (short)((WS_PlayerData.CharacterObject)Target).Stamina.Base;
            short OldSpi = (short)((WS_PlayerData.CharacterObject)Target).Spirit.Base;
            short OldInt = (short)((WS_PlayerData.CharacterObject)Target).Intellect.Base;
            switch (EffectInfo.MiscValue)
            {
                case -1:
                    {
                        ((WS_PlayerData.CharacterObject)Target).Strength.RealBase = (int)(((WS_PlayerData.CharacterObject)Target).Strength.RealBase / ((WS_PlayerData.CharacterObject)Target).Strength.BaseModifier);
                        ((WS_PlayerData.CharacterObject)Target).Strength.BaseModifier += value;
                        ((WS_PlayerData.CharacterObject)Target).Strength.RealBase = (int)(((WS_PlayerData.CharacterObject)Target).Strength.RealBase * ((WS_PlayerData.CharacterObject)Target).Strength.BaseModifier);
                        ((WS_PlayerData.CharacterObject)Target).Agility.RealBase = (int)(((WS_PlayerData.CharacterObject)Target).Agility.RealBase / ((WS_PlayerData.CharacterObject)Target).Agility.BaseModifier);
                        ((WS_PlayerData.CharacterObject)Target).Agility.BaseModifier += value;
                        ((WS_PlayerData.CharacterObject)Target).Agility.RealBase = (int)(((WS_PlayerData.CharacterObject)Target).Agility.RealBase * ((WS_PlayerData.CharacterObject)Target).Agility.BaseModifier);
                        ((WS_PlayerData.CharacterObject)Target).Stamina.RealBase = (int)(((WS_PlayerData.CharacterObject)Target).Stamina.RealBase / ((WS_PlayerData.CharacterObject)Target).Stamina.BaseModifier);
                        ((WS_PlayerData.CharacterObject)Target).Stamina.BaseModifier += value;
                        ((WS_PlayerData.CharacterObject)Target).Stamina.RealBase = (int)(((WS_PlayerData.CharacterObject)Target).Stamina.RealBase * ((WS_PlayerData.CharacterObject)Target).Stamina.BaseModifier);
                        ((WS_PlayerData.CharacterObject)Target).Spirit.RealBase = (int)(((WS_PlayerData.CharacterObject)Target).Spirit.RealBase / ((WS_PlayerData.CharacterObject)Target).Spirit.BaseModifier);
                        ((WS_PlayerData.CharacterObject)Target).Spirit.BaseModifier += value;
                        ((WS_PlayerData.CharacterObject)Target).Spirit.RealBase = (int)(((WS_PlayerData.CharacterObject)Target).Spirit.RealBase * ((WS_PlayerData.CharacterObject)Target).Spirit.BaseModifier);
                        ((WS_PlayerData.CharacterObject)Target).Intellect.RealBase = (int)(((WS_PlayerData.CharacterObject)Target).Intellect.RealBase / ((WS_PlayerData.CharacterObject)Target).Intellect.BaseModifier);
                        ((WS_PlayerData.CharacterObject)Target).Intellect.BaseModifier += value;
                        ((WS_PlayerData.CharacterObject)Target).Intellect.RealBase = (int)(((WS_PlayerData.CharacterObject)Target).Intellect.RealBase * ((WS_PlayerData.CharacterObject)Target).Intellect.BaseModifier);
                        break;
                    }

                case 0:
                    {
                        ((WS_PlayerData.CharacterObject)Target).Strength.RealBase = (int)(((WS_PlayerData.CharacterObject)Target).Strength.RealBase / ((WS_PlayerData.CharacterObject)Target).Strength.BaseModifier);
                        ((WS_PlayerData.CharacterObject)Target).Strength.BaseModifier += value;
                        ((WS_PlayerData.CharacterObject)Target).Strength.RealBase = (int)(((WS_PlayerData.CharacterObject)Target).Strength.RealBase * ((WS_PlayerData.CharacterObject)Target).Strength.BaseModifier);
                        break;
                    }

                case 1:
                    {
                        ((WS_PlayerData.CharacterObject)Target).Agility.RealBase = (int)(((WS_PlayerData.CharacterObject)Target).Agility.RealBase / ((WS_PlayerData.CharacterObject)Target).Agility.BaseModifier);
                        ((WS_PlayerData.CharacterObject)Target).Agility.BaseModifier += value;
                        ((WS_PlayerData.CharacterObject)Target).Agility.RealBase = (int)(((WS_PlayerData.CharacterObject)Target).Agility.RealBase * ((WS_PlayerData.CharacterObject)Target).Agility.BaseModifier);
                        break;
                    }

                case 2:
                    {
                        ((WS_PlayerData.CharacterObject)Target).Stamina.RealBase = (int)(((WS_PlayerData.CharacterObject)Target).Stamina.RealBase / ((WS_PlayerData.CharacterObject)Target).Stamina.BaseModifier);
                        ((WS_PlayerData.CharacterObject)Target).Stamina.BaseModifier += value;
                        ((WS_PlayerData.CharacterObject)Target).Stamina.RealBase = (int)(((WS_PlayerData.CharacterObject)Target).Stamina.RealBase * ((WS_PlayerData.CharacterObject)Target).Stamina.BaseModifier);
                        break;
                    }

                case 3:
                    {
                        ((WS_PlayerData.CharacterObject)Target).Intellect.RealBase = (int)(((WS_PlayerData.CharacterObject)Target).Intellect.RealBase / ((WS_PlayerData.CharacterObject)Target).Intellect.BaseModifier);
                        ((WS_PlayerData.CharacterObject)Target).Intellect.BaseModifier += value;
                        ((WS_PlayerData.CharacterObject)Target).Intellect.RealBase = (int)(((WS_PlayerData.CharacterObject)Target).Intellect.RealBase * ((WS_PlayerData.CharacterObject)Target).Intellect.BaseModifier);
                        break;
                    }

                case 4:
                    {
                        ((WS_PlayerData.CharacterObject)Target).Spirit.RealBase = (int)(((WS_PlayerData.CharacterObject)Target).Spirit.RealBase / ((WS_PlayerData.CharacterObject)Target).Spirit.BaseModifier);
                        ((WS_PlayerData.CharacterObject)Target).Spirit.BaseModifier += value;
                        ((WS_PlayerData.CharacterObject)Target).Spirit.RealBase = (int)(((WS_PlayerData.CharacterObject)Target).Spirit.RealBase * ((WS_PlayerData.CharacterObject)Target).Spirit.BaseModifier);
                        break;
                    }
            } ((WS_PlayerData.CharacterObject)Target).Life.Bonus += (((WS_PlayerData.CharacterObject)Target).Stamina.Base - OldSta) * 10;
            ((WS_PlayerData.CharacterObject)Target).Mana.Bonus += (((WS_PlayerData.CharacterObject)Target).Intellect.Base - OldInt) * 15;
            ((WS_PlayerData.CharacterObject)Target).Resistances[DamageTypes.DMG_PHYSICAL].Base += (((WS_PlayerData.CharacterObject)Target).Agility.Base - OldAgi) * 2;
            ((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag = ((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_MAX_HP | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_MAX_POWER;
            ((WS_PlayerData.CharacterObject)Target).UpdateManaRegen();
            WS_PlayerData.CharacterObject argobjCharacter = (WS_PlayerData.CharacterObject)Target;
            WorldServiceLocator._WS_Combat.CalculateMinMaxDamage(ref argobjCharacter, WeaponAttackType.BASE_ATTACK);
            WS_PlayerData.CharacterObject argobjCharacter1 = (WS_PlayerData.CharacterObject)Target;
            WorldServiceLocator._WS_Combat.CalculateMinMaxDamage(ref argobjCharacter1, WeaponAttackType.OFF_ATTACK);
            WS_PlayerData.CharacterObject argobjCharacter2 = (WS_PlayerData.CharacterObject)Target;
            WorldServiceLocator._WS_Combat.CalculateMinMaxDamage(ref argobjCharacter2, WeaponAttackType.RANGED_ATTACK);
            ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_STRENGTH, ((WS_PlayerData.CharacterObject)Target).Strength.Base);
            ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_AGILITY, ((WS_PlayerData.CharacterObject)Target).Agility.Base);
            ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_STAMINA, ((WS_PlayerData.CharacterObject)Target).Stamina.Base);
            ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_SPIRIT, ((WS_PlayerData.CharacterObject)Target).Spirit.Base);
            ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_INTELLECT, ((WS_PlayerData.CharacterObject)Target).Intellect.Base);
            ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_HEALTH, ((WS_PlayerData.CharacterObject)Target).Life.Current);
            ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXHEALTH, ((WS_PlayerData.CharacterObject)Target).Life.Maximum);
            if (WorldServiceLocator._WS_Player_Initializator.GetClassManaType(((WS_PlayerData.CharacterObject)Target).Classe) == ManaTypes.TYPE_MANA)
            {
                ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER1, ((WS_PlayerData.CharacterObject)Target).Mana.Current);
                ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXPOWER1, ((WS_PlayerData.CharacterObject)Target).Mana.Maximum);
            } ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_PHYSICAL, ((WS_PlayerData.CharacterObject)Target).Resistances[DamageTypes.DMG_PHYSICAL].Base);
            ((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate(false);
            ((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag = ((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_MAX_HP;
            ((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag = ((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_MAX_POWER;
        }

        public void SPELL_AURA_MOD_TOTAL_STAT_PERCENTAGE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            if (Action == AuraAction.AURA_UPDATE)
                return;
            if (!(Target is WS_PlayerData.CharacterObject))
                return;
            float value = (float)(EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d);
            int value_sign = EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
            if (Action == AuraAction.AURA_REMOVE)
                value = -value;
            short OldStr = (short)((WS_PlayerData.CharacterObject)Target).Strength.Base;
            short OldAgi = (short)((WS_PlayerData.CharacterObject)Target).Agility.Base;
            short OldSta = (short)((WS_PlayerData.CharacterObject)Target).Stamina.Base;
            short OldSpi = (short)((WS_PlayerData.CharacterObject)Target).Spirit.Base;
            short OldInt = (short)((WS_PlayerData.CharacterObject)Target).Intellect.Base;
            switch (EffectInfo.MiscValue)
            {
                case -1:
                    {
                        ((WS_PlayerData.CharacterObject)Target).Strength.Base = (int)(((WS_PlayerData.CharacterObject)Target).Strength.Base / ((WS_PlayerData.CharacterObject)Target).Strength.Modifier);
                        ((WS_PlayerData.CharacterObject)Target).Strength.Modifier += value;
                        ((WS_PlayerData.CharacterObject)Target).Strength.Base = (int)(((WS_PlayerData.CharacterObject)Target).Strength.Base * ((WS_PlayerData.CharacterObject)Target).Strength.Modifier);
                        ((WS_PlayerData.CharacterObject)Target).Agility.Base = (int)(((WS_PlayerData.CharacterObject)Target).Agility.Base / ((WS_PlayerData.CharacterObject)Target).Agility.Modifier);
                        ((WS_PlayerData.CharacterObject)Target).Agility.Modifier += value;
                        ((WS_PlayerData.CharacterObject)Target).Agility.Base = (int)(((WS_PlayerData.CharacterObject)Target).Agility.Base * ((WS_PlayerData.CharacterObject)Target).Agility.Modifier);
                        ((WS_PlayerData.CharacterObject)Target).Stamina.Base = (int)(((WS_PlayerData.CharacterObject)Target).Stamina.Base / ((WS_PlayerData.CharacterObject)Target).Stamina.Modifier);
                        ((WS_PlayerData.CharacterObject)Target).Stamina.Modifier += value;
                        ((WS_PlayerData.CharacterObject)Target).Stamina.Base = (int)(((WS_PlayerData.CharacterObject)Target).Stamina.Base * ((WS_PlayerData.CharacterObject)Target).Stamina.Modifier);
                        ((WS_PlayerData.CharacterObject)Target).Spirit.Base = (int)(((WS_PlayerData.CharacterObject)Target).Spirit.Base / ((WS_PlayerData.CharacterObject)Target).Spirit.Modifier);
                        ((WS_PlayerData.CharacterObject)Target).Spirit.Modifier += value;
                        ((WS_PlayerData.CharacterObject)Target).Spirit.Base = (int)(((WS_PlayerData.CharacterObject)Target).Spirit.Base * ((WS_PlayerData.CharacterObject)Target).Spirit.Modifier);
                        ((WS_PlayerData.CharacterObject)Target).Intellect.Base = (int)(((WS_PlayerData.CharacterObject)Target).Intellect.Base / ((WS_PlayerData.CharacterObject)Target).Intellect.Modifier);
                        ((WS_PlayerData.CharacterObject)Target).Intellect.Modifier += value;
                        ((WS_PlayerData.CharacterObject)Target).Intellect.Base = (int)(((WS_PlayerData.CharacterObject)Target).Intellect.Base * ((WS_PlayerData.CharacterObject)Target).Intellect.Modifier);
                        if (value_sign > 0)
                        {
                            ((WS_PlayerData.CharacterObject)Target).Strength.PositiveBonus = (short)(((WS_PlayerData.CharacterObject)Target).Strength.PositiveBonus + (((WS_PlayerData.CharacterObject)Target).Strength.Base - OldStr));
                            ((WS_PlayerData.CharacterObject)Target).Agility.PositiveBonus = (short)(((WS_PlayerData.CharacterObject)Target).Agility.PositiveBonus + (((WS_PlayerData.CharacterObject)Target).Agility.Base - OldAgi));
                            ((WS_PlayerData.CharacterObject)Target).Stamina.PositiveBonus = (short)(((WS_PlayerData.CharacterObject)Target).Stamina.PositiveBonus + (((WS_PlayerData.CharacterObject)Target).Stamina.Base - OldSta));
                            ((WS_PlayerData.CharacterObject)Target).Spirit.PositiveBonus = (short)(((WS_PlayerData.CharacterObject)Target).Spirit.PositiveBonus + (((WS_PlayerData.CharacterObject)Target).Spirit.Base - OldSpi));
                            ((WS_PlayerData.CharacterObject)Target).Intellect.PositiveBonus = (short)(((WS_PlayerData.CharacterObject)Target).Intellect.PositiveBonus + (((WS_PlayerData.CharacterObject)Target).Intellect.Base - OldInt));
                        }
                        else
                        {
                            ((WS_PlayerData.CharacterObject)Target).Strength.NegativeBonus = (short)(((WS_PlayerData.CharacterObject)Target).Strength.NegativeBonus - (((WS_PlayerData.CharacterObject)Target).Strength.Base - OldStr));
                            ((WS_PlayerData.CharacterObject)Target).Agility.NegativeBonus = (short)(((WS_PlayerData.CharacterObject)Target).Agility.NegativeBonus - (((WS_PlayerData.CharacterObject)Target).Agility.Base - OldAgi));
                            ((WS_PlayerData.CharacterObject)Target).Stamina.NegativeBonus = (short)(((WS_PlayerData.CharacterObject)Target).Stamina.NegativeBonus - (((WS_PlayerData.CharacterObject)Target).Stamina.Base - OldSta));
                            ((WS_PlayerData.CharacterObject)Target).Spirit.NegativeBonus = (short)(((WS_PlayerData.CharacterObject)Target).Spirit.NegativeBonus - (((WS_PlayerData.CharacterObject)Target).Spirit.Base - OldSpi));
                            ((WS_PlayerData.CharacterObject)Target).Intellect.NegativeBonus = (short)(((WS_PlayerData.CharacterObject)Target).Intellect.NegativeBonus - (((WS_PlayerData.CharacterObject)Target).Intellect.Base - OldInt));
                        }

                        break;
                    }

                case 0:
                    {
                        ((WS_PlayerData.CharacterObject)Target).Strength.Base = (int)(((WS_PlayerData.CharacterObject)Target).Strength.Base / ((WS_PlayerData.CharacterObject)Target).Strength.Modifier);
                        ((WS_PlayerData.CharacterObject)Target).Strength.Modifier += value;
                        ((WS_PlayerData.CharacterObject)Target).Strength.Base = (int)(((WS_PlayerData.CharacterObject)Target).Strength.Base * ((WS_PlayerData.CharacterObject)Target).Strength.Modifier);
                        if (value_sign > 0)
                        {
                            ((WS_PlayerData.CharacterObject)Target).Strength.PositiveBonus = (short)(((WS_PlayerData.CharacterObject)Target).Strength.PositiveBonus + (((WS_PlayerData.CharacterObject)Target).Strength.Base - OldStr));
                        }
                        else
                        {
                            ((WS_PlayerData.CharacterObject)Target).Strength.NegativeBonus = (short)(((WS_PlayerData.CharacterObject)Target).Strength.NegativeBonus - (((WS_PlayerData.CharacterObject)Target).Strength.Base - OldStr));
                        }

                        break;
                    }

                case 1:
                    {
                        ((WS_PlayerData.CharacterObject)Target).Agility.Base = (int)(((WS_PlayerData.CharacterObject)Target).Agility.Base / ((WS_PlayerData.CharacterObject)Target).Agility.Modifier);
                        ((WS_PlayerData.CharacterObject)Target).Agility.Modifier += value;
                        ((WS_PlayerData.CharacterObject)Target).Agility.Base = (int)(((WS_PlayerData.CharacterObject)Target).Agility.Base * ((WS_PlayerData.CharacterObject)Target).Agility.Modifier);
                        if (value_sign > 0)
                        {
                            ((WS_PlayerData.CharacterObject)Target).Agility.PositiveBonus = (short)(((WS_PlayerData.CharacterObject)Target).Agility.PositiveBonus + (((WS_PlayerData.CharacterObject)Target).Agility.Base - OldAgi));
                        }
                        else
                        {
                            ((WS_PlayerData.CharacterObject)Target).Agility.NegativeBonus = (short)(((WS_PlayerData.CharacterObject)Target).Agility.NegativeBonus - (((WS_PlayerData.CharacterObject)Target).Agility.Base - OldAgi));
                        }

                        break;
                    }

                case 2:
                    {
                        ((WS_PlayerData.CharacterObject)Target).Stamina.Base = (int)(((WS_PlayerData.CharacterObject)Target).Stamina.Base / ((WS_PlayerData.CharacterObject)Target).Stamina.Modifier);
                        ((WS_PlayerData.CharacterObject)Target).Stamina.Modifier += value;
                        ((WS_PlayerData.CharacterObject)Target).Stamina.Base = (int)(((WS_PlayerData.CharacterObject)Target).Stamina.Base * ((WS_PlayerData.CharacterObject)Target).Stamina.Modifier);
                        if (value_sign > 0)
                        {
                            ((WS_PlayerData.CharacterObject)Target).Stamina.PositiveBonus = (short)(((WS_PlayerData.CharacterObject)Target).Stamina.PositiveBonus + (((WS_PlayerData.CharacterObject)Target).Stamina.Base - OldSta));
                        }
                        else
                        {
                            ((WS_PlayerData.CharacterObject)Target).Stamina.NegativeBonus = (short)(((WS_PlayerData.CharacterObject)Target).Stamina.NegativeBonus - (((WS_PlayerData.CharacterObject)Target).Stamina.Base - OldSta));
                        }

                        break;
                    }

                case 3:
                    {
                        break;
                    }

                case 4:
                    {
                        ((WS_PlayerData.CharacterObject)Target).Spirit.Base = (int)(((WS_PlayerData.CharacterObject)Target).Spirit.Base / ((WS_PlayerData.CharacterObject)Target).Spirit.Modifier);
                        ((WS_PlayerData.CharacterObject)Target).Spirit.Modifier += value;
                        ((WS_PlayerData.CharacterObject)Target).Spirit.Base = (int)(((WS_PlayerData.CharacterObject)Target).Spirit.Base * ((WS_PlayerData.CharacterObject)Target).Spirit.Modifier);
                        if (value_sign > 0)
                        {
                            ((WS_PlayerData.CharacterObject)Target).Spirit.PositiveBonus = (short)(((WS_PlayerData.CharacterObject)Target).Spirit.PositiveBonus + (((WS_PlayerData.CharacterObject)Target).Spirit.Base - OldSpi));
                        }
                        else
                        {
                            ((WS_PlayerData.CharacterObject)Target).Spirit.NegativeBonus = (short)(((WS_PlayerData.CharacterObject)Target).Spirit.NegativeBonus - (((WS_PlayerData.CharacterObject)Target).Spirit.Base - OldSpi));
                        }

                        break;
                    }
            } ((WS_PlayerData.CharacterObject)Target).Life.Bonus = (((WS_PlayerData.CharacterObject)Target).Stamina.Base - 18) * 10;
            ((WS_PlayerData.CharacterObject)Target).Mana.Bonus = (((WS_PlayerData.CharacterObject)Target).Intellect.Base - 18) * 15;
            ((WS_PlayerData.CharacterObject)Target).Resistances[DamageTypes.DMG_PHYSICAL].Base += (((WS_PlayerData.CharacterObject)Target).Agility.Base - OldAgi) * 2;
            ((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag = ((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_MAX_HP | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_MAX_POWER;
            ((WS_PlayerData.CharacterObject)Target).UpdateManaRegen();
            WS_PlayerData.CharacterObject argobjCharacter = (WS_PlayerData.CharacterObject)Target;
            WorldServiceLocator._WS_Combat.CalculateMinMaxDamage(ref argobjCharacter, WeaponAttackType.BASE_ATTACK);
            WS_PlayerData.CharacterObject argobjCharacter1 = (WS_PlayerData.CharacterObject)Target;
            WorldServiceLocator._WS_Combat.CalculateMinMaxDamage(ref argobjCharacter1, WeaponAttackType.OFF_ATTACK);
            WS_PlayerData.CharacterObject argobjCharacter2 = (WS_PlayerData.CharacterObject)Target;
            WorldServiceLocator._WS_Combat.CalculateMinMaxDamage(ref argobjCharacter2, WeaponAttackType.RANGED_ATTACK);
            ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_STRENGTH, ((WS_PlayerData.CharacterObject)Target).Strength.Base);
            ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_AGILITY, ((WS_PlayerData.CharacterObject)Target).Agility.Base);
            ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_STAMINA, ((WS_PlayerData.CharacterObject)Target).Stamina.Base);
            ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_SPIRIT, ((WS_PlayerData.CharacterObject)Target).Spirit.Base);
            ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_INTELLECT, ((WS_PlayerData.CharacterObject)Target).Intellect.Base);
            // CType(Target, CharacterObject).SetUpdateFlag(EUnitFields.UNIT_FIELD_POSSTAT0, CType(CType(Target, CharacterObject).Strength.PositiveBonus, Integer))
            // CType(Target, CharacterObject).SetUpdateFlag(EUnitFields.UNIT_FIELD_POSSTAT1, CType(CType(Target, CharacterObject).Agility.PositiveBonus, Integer))
            // CType(Target, CharacterObject).SetUpdateFlag(EUnitFields.UNIT_FIELD_POSSTAT2, CType(CType(Target, CharacterObject).Stamina.PositiveBonus, Integer))
            // CType(Target, CharacterObject).SetUpdateFlag(EUnitFields.UNIT_FIELD_POSSTAT3, CType(CType(Target, CharacterObject).Intellect.PositiveBonus, Integer))
            // CType(Target, CharacterObject).SetUpdateFlag(EUnitFields.UNIT_FIELD_POSSTAT4, CType(CType(Target, CharacterObject).Spirit.PositiveBonus, Integer))
            // CType(Target, CharacterObject).SetUpdateFlag(EUnitFields.UNIT_FIELD_NEGSTAT0, CType(CType(Target, CharacterObject).Strength.NegativeBonus, Integer))
            // CType(Target, CharacterObject).SetUpdateFlag(EUnitFields.UNIT_FIELD_NEGSTAT1, CType(CType(Target, CharacterObject).Agility.NegativeBonus, Integer))
            // CType(Target, CharacterObject).SetUpdateFlag(EUnitFields.UNIT_FIELD_NEGSTAT2, CType(CType(Target, CharacterObject).Stamina.NegativeBonus, Integer))
            // CType(Target, CharacterObject).SetUpdateFlag(EUnitFields.UNIT_FIELD_NEGSTAT3, CType(CType(Target, CharacterObject).Intellect.NegativeBonus, Integer))
            // CType(Target, CharacterObject).SetUpdateFlag(EUnitFields.UNIT_FIELD_NEGSTAT4, CType(CType(Target, CharacterObject).Spirit.NegativeBonus, Integer))
            ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_HEALTH, ((WS_PlayerData.CharacterObject)Target).Life.Current);
            ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXHEALTH, ((WS_PlayerData.CharacterObject)Target).Life.Maximum);
            if (WorldServiceLocator._WS_Player_Initializator.GetClassManaType(((WS_PlayerData.CharacterObject)Target).Classe) == ManaTypes.TYPE_MANA)
            {
                ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER1, ((WS_PlayerData.CharacterObject)Target).Mana.Current);
                ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXPOWER1, ((WS_PlayerData.CharacterObject)Target).Mana.Maximum);
            } ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_PHYSICAL, ((WS_PlayerData.CharacterObject)Target).Resistances[DamageTypes.DMG_PHYSICAL].Base);
            ((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate(false);
            ((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag = ((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_MAX_HP;
            ((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag = ((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_MAX_POWER;
        }

        public void SPELL_AURA_MOD_INCREASE_HEALTH(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        Target.Life.Bonus += EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        Target.Life.Bonus -= EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                        break;
                    }
            }

            if (Target is WS_PlayerData.CharacterObject)
            {
                ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXHEALTH, Target.Life.Maximum);
                ((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate();
                ((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag = ((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_MAX_HP;
            }
            else
            {
                var packet = new Packets.UpdatePacketClass();
                var UpdateData = new Packets.UpdateClass(EUnitFields.UNIT_END);
                UpdateData.SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXHEALTH, Target.Life.Maximum);
                UpdateData.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, (WS_Creatures.CreatureObject)Target);
                Packets.PacketClass argpacket = packet;
                ((WS_Creatures.CreatureObject)Target).SendToNearPlayers(ref argpacket);
                packet.Dispose();
                UpdateData.Dispose();
            }
        }

        public void SPELL_AURA_MOD_INCREASE_HEALTH_PERCENT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        Target.Life.Modifier = (float)(Target.Life.Modifier + EffectInfo.get_GetValue(Target.Level, 0) / 100d);
                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        Target.Life.Modifier = (float)(Target.Life.Modifier - EffectInfo.get_GetValue(Target.Level, 0) / 100d);
                        break;
                    }
            }

            if (Target is WS_PlayerData.CharacterObject)
            {
                ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXHEALTH, Target.Life.Maximum);
                ((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate();
                ((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag = ((WS_PlayerData.CharacterObject)Target).GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_MAX_HP;
            }
            else
            {
                var packet = new Packets.UpdatePacketClass();
                var UpdateData = new Packets.UpdateClass(EUnitFields.UNIT_END);
                UpdateData.SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXHEALTH, Target.Life.Maximum);
                UpdateData.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, (WS_Creatures.CreatureObject)Target);
                Packets.PacketClass argpacket = packet;
                ((WS_Creatures.CreatureObject)Target).SendToNearPlayers(ref argpacket);
                packet.Dispose();
                UpdateData.Dispose();
            }
        }

        public void SPELL_AURA_MOD_INCREASE_ENERGY(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        if (EffectInfo.MiscValue == Target.ManaType)
                        {
                            if (!(Target is WS_PlayerData.CharacterObject))
                            {
                                Target.Mana.Bonus += EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                            }
                            else
                            {
                                switch (Target.ManaType)
                                {
                                    case var case2 when case2 == ManaTypes.TYPE_ENERGY:
                                        {
                                            ((WS_PlayerData.CharacterObject)Target).Energy.Bonus += EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                                            break;
                                        }

                                    case var case3 when case3 == ManaTypes.TYPE_MANA:
                                        {
                                            ((WS_PlayerData.CharacterObject)Target).Mana.Bonus += EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                                            break;
                                        }

                                    case var case4 when case4 == ManaTypes.TYPE_RAGE:
                                        {
                                            ((WS_PlayerData.CharacterObject)Target).Rage.Bonus += EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                                            break;
                                        }
                                }
                            }
                        }

                        break;
                    }

                case var case5 when case5 == AuraAction.AURA_REMOVE:
                case var case6 when case6 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        if (EffectInfo.MiscValue == Target.ManaType)
                        {
                            if (!(Target is WS_PlayerData.CharacterObject))
                            {
                                Target.Mana.Bonus -= EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                            }
                            else
                            {
                                switch (Target.ManaType)
                                {
                                    case var case7 when case7 == ManaTypes.TYPE_ENERGY:
                                        {
                                            ((WS_PlayerData.CharacterObject)Target).Energy.Bonus -= EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                                            break;
                                        }

                                    case var case8 when case8 == ManaTypes.TYPE_MANA:
                                        {
                                            ((WS_PlayerData.CharacterObject)Target).Mana.Bonus -= EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                                            break;
                                        }

                                    case var case9 when case9 == ManaTypes.TYPE_RAGE:
                                        {
                                            ((WS_PlayerData.CharacterObject)Target).Rage.Bonus -= EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                                            break;
                                        }
                                }
                            }
                        }

                        break;
                    }
            }

            if (Target is WS_PlayerData.CharacterObject)
            {
                ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXPOWER1 + ManaTypes.TYPE_ENERGY, ((WS_PlayerData.CharacterObject)Target).Energy.Maximum);
                ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXPOWER1 + ManaTypes.TYPE_MANA, ((WS_PlayerData.CharacterObject)Target).Mana.Maximum);
                ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXPOWER1 + ManaTypes.TYPE_RAGE, ((WS_PlayerData.CharacterObject)Target).Rage.Maximum);
            }
            else
            {
                var packet = new Packets.UpdatePacketClass();
                var UpdateData = new Packets.UpdateClass(EUnitFields.UNIT_END);
                UpdateData.SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXPOWER1 + Target.ManaType, Target.Mana.Maximum);
                UpdateData.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, (WS_Creatures.CreatureObject)Target);
                Packets.PacketClass argpacket = packet;
                ((WS_Creatures.CreatureObject)Target).SendToNearPlayers(ref argpacket);
                packet.Dispose();
                UpdateData.Dispose();
            }
        }

        public void SPELL_AURA_MOD_BASE_RESISTANCE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            if (!(Target is WS_PlayerData.CharacterObject))
                return;
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        for (DamageTypes i = DamageTypes.DMG_PHYSICAL, loopTo = DamageTypes.DMG_ARCANE; i <= loopTo; i++)
                        {
                            if (WorldServiceLocator._Functions.HaveFlag(EffectInfo.MiscValue, i))
                            {
                                ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base = (int)(((WS_PlayerData.CharacterObject)Target).Resistances[i].Base / ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
                                ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base += EffectInfo.get_GetValue(Target.Level, 0);
                                ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base = (int)(((WS_PlayerData.CharacterObject)Target).Resistances[i].Base * ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
                                ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + i, ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base);
                            }
                        }

                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        for (DamageTypes i = DamageTypes.DMG_PHYSICAL, loopTo1 = DamageTypes.DMG_ARCANE; i <= loopTo1; i++)
                        {
                            if (WorldServiceLocator._Functions.HaveFlag(EffectInfo.MiscValue, i))
                            {
                                ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base = (int)(((WS_PlayerData.CharacterObject)Target).Resistances[i].Base / ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
                                ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base -= EffectInfo.get_GetValue(Target.Level, 0);
                                ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base = (int)(((WS_PlayerData.CharacterObject)Target).Resistances[i].Base * ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
                                ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + i, ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base);
                            }
                        }

                        break;
                    }
            }
        }

        public void SPELL_AURA_MOD_BASE_RESISTANCE_PCT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            if (!(Target is WS_PlayerData.CharacterObject))
                return;
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        for (byte i = DamageTypes.DMG_PHYSICAL, loopTo = DamageTypes.DMG_ARCANE; i <= loopTo; i++)
                        {
                            if (WorldServiceLocator._Functions.HaveFlag((uint)EffectInfo.MiscValue, i))
                            {
                                ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base = (int)(((WS_PlayerData.CharacterObject)Target).Resistances[i].Base / ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
                                ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier = (float)(((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier + EffectInfo.get_GetValue(Target.Level, 0) / 100d);
                                ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base = (int)(((WS_PlayerData.CharacterObject)Target).Resistances[i].Base * ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
                                ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + i, ((WS_PlayerData.CharacterObject)Target).Resistances[(int)i].Base);
                            }
                        }

                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        for (byte i = DamageTypes.DMG_PHYSICAL, loopTo1 = DamageTypes.DMG_ARCANE; i <= loopTo1; i++)
                        {
                            if (WorldServiceLocator._Functions.HaveFlag((uint)EffectInfo.MiscValue, i))
                            {
                                ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base = (int)(((WS_PlayerData.CharacterObject)Target).Resistances[i].Base / ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
                                ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier = (float)(((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier - EffectInfo.get_GetValue(Target.Level, 0) / 100d);
                                ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base = (int)(((WS_PlayerData.CharacterObject)Target).Resistances[i].Base * ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
                                ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + i, ((WS_PlayerData.CharacterObject)Target).Resistances[(int)i].Base);
                            }
                        }

                        break;
                    }
            }
        }

        public void SPELL_AURA_MOD_RESISTANCE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            if (!(Target is WS_PlayerData.CharacterObject))
                return;
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        for (byte i = DamageTypes.DMG_PHYSICAL, loopTo = DamageTypes.DMG_ARCANE; i <= loopTo; i++)
                        {
                            if (WorldServiceLocator._Functions.HaveFlag((uint)EffectInfo.MiscValue, i))
                            {
                                if (EffectInfo.get_GetValue(Target.Level, 0) > 0)
                                {
                                    Target.Resistances[i].Base = (int)(Target.Resistances[i].Base / ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
                                    Target.Resistances[i].Base += EffectInfo.get_GetValue(Target.Level, 0);
                                    Target.Resistances[i].Base = (int)(Target.Resistances[i].Base * ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
                                    Target.Resistances[i].PositiveBonus = (short)(Target.Resistances[i].PositiveBonus + EffectInfo.get_GetValue(Target.Level, 0));
                                }
                                else
                                {
                                    Target.Resistances[i].Base = (int)(Target.Resistances[i].Base / ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
                                    Target.Resistances[i].Base += EffectInfo.get_GetValue(Target.Level, 0);
                                    Target.Resistances[i].Base = (int)(Target.Resistances[i].Base * ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
                                    Target.Resistances[i].NegativeBonus = (short)(Target.Resistances[i].NegativeBonus - EffectInfo.get_GetValue(Target.Level, 0));
                                } ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + i, ((WS_PlayerData.CharacterObject)Target).Resistances[(int)i].Base);
                            }
                        }

                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        for (byte i = DamageTypes.DMG_PHYSICAL, loopTo1 = DamageTypes.DMG_ARCANE; i <= loopTo1; i++)
                        {
                            if (WorldServiceLocator._Functions.HaveFlag((uint)EffectInfo.MiscValue, i))
                            {
                                if (EffectInfo.get_GetValue(Target.Level, 0) > 0)
                                {
                                    Target.Resistances[i].Base = (int)(Target.Resistances[i].Base / ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
                                    Target.Resistances[i].Base -= EffectInfo.get_GetValue(Target.Level, 0);
                                    Target.Resistances[i].Base = (int)(Target.Resistances[i].Base * ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
                                    Target.Resistances[i].PositiveBonus = (short)(Target.Resistances[i].PositiveBonus - EffectInfo.get_GetValue(Target.Level, 0));
                                }
                                else
                                {
                                    Target.Resistances[i].Base = (int)(Target.Resistances[i].Base / ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
                                    Target.Resistances[i].Base -= EffectInfo.get_GetValue(Target.Level, 0);
                                    Target.Resistances[i].Base = (int)(Target.Resistances[i].Base * ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
                                    Target.Resistances[i].NegativeBonus = (short)(Target.Resistances[i].NegativeBonus + EffectInfo.get_GetValue(Target.Level, 0));
                                } ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + i, ((WS_PlayerData.CharacterObject)Target).Resistances[(int)i].Base);
                            }
                        }

                        break;
                    }
            } ((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate(false);
        }

        public void SPELL_AURA_MOD_RESISTANCE_PCT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            if (!(Target is WS_PlayerData.CharacterObject))
                return;
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        for (byte i = DamageTypes.DMG_PHYSICAL, loopTo = DamageTypes.DMG_ARCANE; i <= loopTo; i++)
                        {
                            if (WorldServiceLocator._Functions.HaveFlag((uint)EffectInfo.MiscValue, i))
                            {
                                short OldBase = (short)((WS_PlayerData.CharacterObject)Target).Resistances[i].Base;
                                if (EffectInfo.get_GetValue(Target.Level, 0) > 0)
                                {
                                    ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base = (int)(((WS_PlayerData.CharacterObject)Target).Resistances[i].Base / ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
                                    ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier += EffectInfo.get_GetValue(Target.Level, 0);
                                    ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base = (int)(((WS_PlayerData.CharacterObject)Target).Resistances[i].Base * ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
                                    ((WS_PlayerData.CharacterObject)Target).Resistances[i].PositiveBonus = (short)(((WS_PlayerData.CharacterObject)Target).Resistances[i].PositiveBonus + (((WS_PlayerData.CharacterObject)Target).Resistances[i].Base - OldBase));
                                }
                                // CType(Target, CharacterObject).SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + i, CType(Target, CharacterObject).Resistances(i).PositiveBonus)
                                else
                                {
                                    ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base = (int)(((WS_PlayerData.CharacterObject)Target).Resistances[i].Base / ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
                                    ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier -= EffectInfo.get_GetValue(Target.Level, 0);
                                    ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base = (int)(((WS_PlayerData.CharacterObject)Target).Resistances[i].Base * ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
                                    // CType(Target, CharacterObject).SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + i, CType(Target, CharacterObject).Resistances(i).NegativeBonus)
                                    ((WS_PlayerData.CharacterObject)Target).Resistances[i].PositiveBonus = (short)(((WS_PlayerData.CharacterObject)Target).Resistances[i].PositiveBonus + (((WS_PlayerData.CharacterObject)Target).Resistances[i].Base - OldBase));
                                } ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + i, ((WS_PlayerData.CharacterObject)Target).Resistances[(int)i].Base);
                            }
                        }

                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        for (byte i = DamageTypes.DMG_PHYSICAL, loopTo1 = DamageTypes.DMG_ARCANE; i <= loopTo1; i++)
                        {
                            if (WorldServiceLocator._Functions.HaveFlag((uint)EffectInfo.MiscValue, i))
                            {
                                short OldBase = (short)((WS_PlayerData.CharacterObject)Target).Resistances[i].Base;
                                if (EffectInfo.get_GetValue(Target.Level, 0) > 0)
                                {
                                    ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base = (int)(((WS_PlayerData.CharacterObject)Target).Resistances[i].Base / ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
                                    ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier -= EffectInfo.get_GetValue(Target.Level, 0);
                                    ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base = (int)(((WS_PlayerData.CharacterObject)Target).Resistances[i].Base * ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
                                    ((WS_PlayerData.CharacterObject)Target).Resistances[i].PositiveBonus = (short)(((WS_PlayerData.CharacterObject)Target).Resistances[i].PositiveBonus - (((WS_PlayerData.CharacterObject)Target).Resistances[i].Base - OldBase));
                                }
                                // CType(Target, CharacterObject).SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + i, CType(Target, CharacterObject).Resistances(i).PositiveBonus)
                                else
                                {
                                    ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base = (int)(((WS_PlayerData.CharacterObject)Target).Resistances[i].Base / ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
                                    ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier += EffectInfo.get_GetValue(Target.Level, 0);
                                    ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base = (int)(((WS_PlayerData.CharacterObject)Target).Resistances[i].Base * ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
                                    ((WS_PlayerData.CharacterObject)Target).Resistances[i].NegativeBonus = (short)(((WS_PlayerData.CharacterObject)Target).Resistances[i].NegativeBonus - (((WS_PlayerData.CharacterObject)Target).Resistances[i].Base - OldBase));
                                    // CType(Target, CharacterObject).SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + i, CType(Target, CharacterObject).Resistances(i).NegativeBonus)
                                } ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + i, ((WS_PlayerData.CharacterObject)Target).Resistances[(int)i].Base);
                            }
                        }

                        break;
                    }
            } ((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate(false);
        }

        public void SPELL_AURA_MOD_RESISTANCE_EXCLUSIVE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            if (!(Target is WS_PlayerData.CharacterObject))
                return;
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        for (byte i = DamageTypes.DMG_PHYSICAL, loopTo = DamageTypes.DMG_ARCANE; i <= loopTo; i++)
                        {
                            if (WorldServiceLocator._Functions.HaveFlag((uint)EffectInfo.MiscValue, i))
                            {
                                if (EffectInfo.get_GetValue(Target.Level, 0) > 0)
                                {
                                    ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base = (int)(((WS_PlayerData.CharacterObject)Target).Resistances[i].Base / ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
                                    ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base += EffectInfo.get_GetValue(Target.Level, 0);
                                    ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base = (int)(((WS_PlayerData.CharacterObject)Target).Resistances[i].Base * ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
                                    ((WS_PlayerData.CharacterObject)Target).Resistances[i].PositiveBonus = (short)(((WS_PlayerData.CharacterObject)Target).Resistances[i].PositiveBonus + EffectInfo.get_GetValue(Target.Level, 0));
                                    // CType(Target, CharacterObject).SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + i, CType(Target, CharacterObject).Resistances(i).PositiveBonus)
                                    ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + i, ((WS_PlayerData.CharacterObject)Target).Resistances[(int)i].Base);
                                }
                                else
                                {
                                    ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base = (int)(((WS_PlayerData.CharacterObject)Target).Resistances[i].Base / ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
                                    ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base -= EffectInfo.get_GetValue(Target.Level, 0);
                                    ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base = (int)(((WS_PlayerData.CharacterObject)Target).Resistances[i].Base * ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
                                    ((WS_PlayerData.CharacterObject)Target).Resistances[i].NegativeBonus = (short)(((WS_PlayerData.CharacterObject)Target).Resistances[i].NegativeBonus - EffectInfo.get_GetValue(Target.Level, 0));
                                    // CType(Target, CharacterObject).SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + i, CType(Target, CharacterObject).Resistances(i).NegativeBonus)
                                    ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + i, ((WS_PlayerData.CharacterObject)Target).Resistances[(int)i].Base);
                                }
                            }
                        }

                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        for (byte i = DamageTypes.DMG_PHYSICAL, loopTo1 = DamageTypes.DMG_ARCANE; i <= loopTo1; i++)
                        {
                            if (WorldServiceLocator._Functions.HaveFlag((uint)EffectInfo.MiscValue, i))
                            {
                                if (EffectInfo.get_GetValue(Target.Level, 0) > 0)
                                {
                                    ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base = (int)(((WS_PlayerData.CharacterObject)Target).Resistances[i].Base / ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
                                    ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base -= EffectInfo.get_GetValue(Target.Level, 0);
                                    ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base = (int)(((WS_PlayerData.CharacterObject)Target).Resistances[i].Base * ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
                                    ((WS_PlayerData.CharacterObject)Target).Resistances[i].PositiveBonus = (short)(((WS_PlayerData.CharacterObject)Target).Resistances[i].PositiveBonus - EffectInfo.get_GetValue(Target.Level, 0));
                                    // CType(Target, CharacterObject).SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + i, CType(Target, CharacterObject).Resistances(i).PositiveBonus)
                                    ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + i, ((WS_PlayerData.CharacterObject)Target).Resistances[(int)i].Base);
                                }
                                else
                                {
                                    ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base = (int)(((WS_PlayerData.CharacterObject)Target).Resistances[i].Base / ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
                                    ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base += EffectInfo.get_GetValue(Target.Level, 0);
                                    ((WS_PlayerData.CharacterObject)Target).Resistances[i].Base = (int)(((WS_PlayerData.CharacterObject)Target).Resistances[i].Base * ((WS_PlayerData.CharacterObject)Target).Resistances[i].Modifier);
                                    ((WS_PlayerData.CharacterObject)Target).Resistances[i].PositiveBonus = (short)(((WS_PlayerData.CharacterObject)Target).Resistances[i].PositiveBonus + EffectInfo.get_GetValue(Target.Level, 0));
                                    // CType(Target, CharacterObject).SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + i, CType(Target, CharacterObject).Resistances(i).NegativeBonus)
                                    ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + i, ((WS_PlayerData.CharacterObject)Target).Resistances[(int)i].Base);
                                }
                            }
                        }

                        break;
                    }
            } ((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate(false);
        }

        public void SPELL_AURA_MOD_ATTACK_POWER(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        Target.AttackPowerMods += EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) * StackCount;
                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        Target.AttackPowerMods -= EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) * StackCount;
                        break;
                    }
            }

            if (Target is WS_PlayerData.CharacterObject)
            {
                ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_ATTACK_POWER, ((WS_PlayerData.CharacterObject)Target).AttackPower);
                ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_ATTACK_POWER_MODS, ((WS_PlayerData.CharacterObject)Target).AttackPowerMods);
                WS_PlayerData.CharacterObject argobjCharacter = (WS_PlayerData.CharacterObject)Target;
                WorldServiceLocator._WS_Combat.CalculateMinMaxDamage(ref argobjCharacter, WeaponAttackType.BASE_ATTACK);
                WS_PlayerData.CharacterObject argobjCharacter1 = (WS_PlayerData.CharacterObject)Target;
                WorldServiceLocator._WS_Combat.CalculateMinMaxDamage(ref argobjCharacter1, WeaponAttackType.OFF_ATTACK);
                ((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate(false);
            }
        }

        public void SPELL_AURA_MOD_RANGED_ATTACK_POWER(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        Target.AttackPowerModsRanged += EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) * StackCount;
                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        Target.AttackPowerModsRanged -= EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) * StackCount;
                        break;
                    }
            }

            if (Target is WS_PlayerData.CharacterObject)
            {
                ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_RANGED_ATTACK_POWER, ((WS_PlayerData.CharacterObject)Target).AttackPowerRanged);
                ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EUnitFields.UNIT_FIELD_RANGED_ATTACK_POWER_MODS, ((WS_PlayerData.CharacterObject)Target).AttackPowerModsRanged);
                WS_PlayerData.CharacterObject argobjCharacter = (WS_PlayerData.CharacterObject)Target;
                WorldServiceLocator._WS_Combat.CalculateMinMaxDamage(ref argobjCharacter, WeaponAttackType.RANGED_ATTACK);
                ((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate(false);
            }
        }

        public void SPELL_AURA_MOD_HEALING_DONE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            if (!(Target is WS_PlayerData.CharacterObject))
                return;
            int Value = EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        ((WS_PlayerData.CharacterObject)Target).healing.PositiveBonus += Value;
                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        ((WS_PlayerData.CharacterObject)Target).healing.PositiveBonus -= Value;
                        break;
                    }
            }

            // CType(Target, CharacterObject).SetUpdateFlag(EPlayerFields.PLAYER_FIELD_MOD_HEALING_DONE_POS, CType(Target, CharacterObject).healing.PositiveBonus)
            // CType(Target, CharacterObject).SendCharacterUpdate(False)
        }

        public void SPELL_AURA_MOD_HEALING_DONE_PCT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            if (!(Target is WS_PlayerData.CharacterObject))
                return;
            float Value = (float)(EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0) / 100d);
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        ((WS_PlayerData.CharacterObject)Target).healing.Modifier += Value;
                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        ((WS_PlayerData.CharacterObject)Target).healing.Modifier -= Value;
                        break;
                    }
            }

            // CType(Target, CharacterObject).SetUpdateFlag(EPlayerFields.PLAYER_FIELD_MOD_HEALING_DONE_POS, CType(Target, CharacterObject).healing.Value)
            // CType(Target, CharacterObject).SendCharacterUpdate(False)
        }

        public void SPELL_AURA_MOD_DAMAGE_DONE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            if (!(Target is WS_PlayerData.CharacterObject))
                return;
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        for (DamageTypes i = DamageTypes.DMG_PHYSICAL, loopTo = DamageTypes.DMG_ARCANE; i <= loopTo; i++)
                        {
                            if (WorldServiceLocator._Functions.HaveFlag(EffectInfo.MiscValue, i))
                            {
                                if (EffectInfo.get_GetValue(Target.Level, 0) > 0)
                                {
                                    ((WS_PlayerData.CharacterObject)Target).spellDamage[i].PositiveBonus += EffectInfo.get_GetValue(Target.Level, 0);
                                    ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EPlayerFields.PLAYER_FIELD_MOD_DAMAGE_DONE_POS + i, ((WS_PlayerData.CharacterObject)Target).spellDamage[i].PositiveBonus);
                                }
                                else
                                {
                                    ((WS_PlayerData.CharacterObject)Target).spellDamage[i].NegativeBonus -= EffectInfo.get_GetValue(Target.Level, 0);
                                    ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EPlayerFields.PLAYER_FIELD_MOD_DAMAGE_DONE_NEG + i, ((WS_PlayerData.CharacterObject)Target).spellDamage[i].NegativeBonus);
                                }
                            }
                        }

                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        for (DamageTypes i = DamageTypes.DMG_PHYSICAL, loopTo1 = DamageTypes.DMG_ARCANE; i <= loopTo1; i++)
                        {
                            if (WorldServiceLocator._Functions.HaveFlag(EffectInfo.MiscValue, i))
                            {
                                if (EffectInfo.get_GetValue(Target.Level, 0) > 0)
                                {
                                    ((WS_PlayerData.CharacterObject)Target).spellDamage[i].PositiveBonus -= EffectInfo.get_GetValue(Target.Level, 0);
                                    ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EPlayerFields.PLAYER_FIELD_MOD_DAMAGE_DONE_POS + i, ((WS_PlayerData.CharacterObject)Target).spellDamage[i].PositiveBonus);
                                }
                                else
                                {
                                    ((WS_PlayerData.CharacterObject)Target).spellDamage[i].NegativeBonus += EffectInfo.get_GetValue(Target.Level, 0);
                                    ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EPlayerFields.PLAYER_FIELD_MOD_DAMAGE_DONE_NEG + i, ((WS_PlayerData.CharacterObject)Target).spellDamage[i].NegativeBonus);
                                }
                            }
                        }

                        break;
                    }
            } ((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate(false);
        }

        public void SPELL_AURA_MOD_DAMAGE_DONE_PCT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            if (!(Target is WS_PlayerData.CharacterObject))
                return;
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        for (DamageTypes i = DamageTypes.DMG_PHYSICAL, loopTo = DamageTypes.DMG_ARCANE; i <= loopTo; i++)
                        {
                            if (WorldServiceLocator._Functions.HaveFlag(EffectInfo.MiscValue, i))
                            {
                                ((WS_PlayerData.CharacterObject)Target).spellDamage[i].Modifier = (float)(((WS_PlayerData.CharacterObject)Target).spellDamage[i].Modifier + EffectInfo.get_GetValue(Target.Level, 0) / 100d);
                                ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EPlayerFields.PLAYER_FIELD_MOD_DAMAGE_DONE_PCT + i, ((WS_PlayerData.CharacterObject)Target).spellDamage[i].Modifier);
                            }
                        }

                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        for (DamageTypes i = DamageTypes.DMG_PHYSICAL, loopTo1 = DamageTypes.DMG_ARCANE; i <= loopTo1; i++)
                        {
                            if (WorldServiceLocator._Functions.HaveFlag(EffectInfo.MiscValue, i))
                            {
                                ((WS_PlayerData.CharacterObject)Target).spellDamage[i].Modifier = (float)(((WS_PlayerData.CharacterObject)Target).spellDamage[i].Modifier - EffectInfo.get_GetValue(Target.Level, 0) / 100d);
                                ((WS_PlayerData.CharacterObject)Target).SetUpdateFlag(EPlayerFields.PLAYER_FIELD_MOD_DAMAGE_DONE_PCT + i, ((WS_PlayerData.CharacterObject)Target).spellDamage[i].Modifier);
                            }
                        }

                        break;
                    }
            } ((WS_PlayerData.CharacterObject)Target).SendCharacterUpdate(false);
        }

        public void SPELL_AURA_EMPATHY(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        if (Target is WS_Creatures.CreatureObject && ((WS_Creatures.CreatureObject)Target).CreatureInfo.CreatureType == UNIT_TYPE.BEAST)
                        {
                            var packet = new Packets.UpdatePacketClass();
                            var tmpUpdate = new Packets.UpdateClass(EUnitFields.UNIT_END);
                            tmpUpdate.SetUpdateFlag(EUnitFields.UNIT_DYNAMIC_FLAGS, Target.cDynamicFlags | DynamicFlags.UNIT_DYNFLAG_SPECIALINFO);
                            tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, (WS_Creatures.CreatureObject)Target);
                            ((WS_PlayerData.CharacterObject)Caster).client.Send(ref (Packets.PacketClass)packet);
                            tmpUpdate.Dispose();
                            packet.Dispose();
                        }

                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        if (Target is WS_Creatures.CreatureObject && ((WS_Creatures.CreatureObject)Target).CreatureInfo.CreatureType == UNIT_TYPE.BEAST)
                        {
                            var packet = new Packets.UpdatePacketClass();
                            var tmpUpdate = new Packets.UpdateClass(EUnitFields.UNIT_END);
                            tmpUpdate.SetUpdateFlag(EUnitFields.UNIT_DYNAMIC_FLAGS, Target.cDynamicFlags);
                            tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, (WS_Creatures.CreatureObject)Target);
                            ((WS_PlayerData.CharacterObject)Caster).client.Send(ref (Packets.PacketClass)packet);
                            tmpUpdate.Dispose();
                            packet.Dispose();
                        }

                        break;
                    }
            }
        }

        public void SPELL_AURA_MOD_SILENCE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        Target.Spell_Silenced = true;
                        if (Target is WS_Creatures.CreatureObject && ((WS_Creatures.CreatureObject)Target).aiScript is object)
                        {
                            WS_Base.BaseUnit argAttacker = (WS_Base.BaseUnit)Caster;
                            ((WS_Creatures.CreatureObject)Target).aiScript.OnGenerateHate(ref argAttacker, 1);
                        }

                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        Target.Spell_Silenced = false;
                        break;
                    }
            }
        }

        public void SPELL_AURA_MOD_PACIFY(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        Target.Spell_Pacifyed = true;
                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        Target.Spell_Pacifyed = false;
                        break;
                    }
            }
        }

        public void SPELL_AURA_MOD_LANGUAGE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            if (!(Target is WS_PlayerData.CharacterObject))
                return;
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        ((WS_PlayerData.CharacterObject)Target).Spell_Language = EffectInfo.MiscValue;
                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        ((WS_PlayerData.CharacterObject)Target).Spell_Language = -1;
                        break;
                    }
            }
        }

        public void SPELL_AURA_MOD_POSSESS(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            if (!(Target is WS_Creatures.CreatureObject) && !(Target is WS_PlayerData.CharacterObject))
                return;
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        if (Target.Level > EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0))
                            return;
                        var packet = new Packets.UpdatePacketClass();
                        var tmpUpdate = new Packets.UpdateClass(EUnitFields.UNIT_END);
                        tmpUpdate.SetUpdateFlag(EUnitFields.UNIT_FIELD_CHARMEDBY, Caster.GUID);
                        tmpUpdate.SetUpdateFlag(EUnitFields.UNIT_FIELD_FACTIONTEMPLATE, ((WS_PlayerData.CharacterObject)Caster).Faction);
                        if (Target is WS_PlayerData.CharacterObject)
                        {
                            tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, (WS_PlayerData.CharacterObject)Target);
                        }
                        else if (Target is WS_Creatures.CreatureObject)
                        {
                            tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, (WS_Creatures.CreatureObject)Target);
                        }

                        Packets.PacketClass argpacket = packet;
                        Target.SendToNearPlayers(ref argpacket);
                        packet.Dispose();
                        tmpUpdate.Dispose();
                        packet = new Packets.UpdatePacketClass();
                        tmpUpdate = new Packets.UpdateClass(EUnitFields.UNIT_END);
                        tmpUpdate.SetUpdateFlag(EUnitFields.UNIT_FIELD_CHARM, Target.GUID);
                        if (Caster is WS_PlayerData.CharacterObject)
                        {
                            tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, (WS_PlayerData.CharacterObject)Caster);
                        }
                        else if (Caster is WS_Creatures.CreatureObject)
                        {
                            tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, (WS_Creatures.CreatureObject)Caster);
                        }

                        Packets.PacketClass argpacket1 = packet;
                        Caster.SendToNearPlayers(ref argpacket1);
                        packet.Dispose();
                        tmpUpdate.Dispose();
                        if (Caster is WS_PlayerData.CharacterObject)
                        {
                            WS_PlayerData.CharacterObject argCaster = (WS_PlayerData.CharacterObject)Caster;
                            WorldServiceLocator._WS_Pets.SendPetInitialize(ref argCaster, ref Target);
                            var packet2 = new Packets.PacketClass(OPCODES.SMSG_DEATH_NOTIFY_OBSOLETE);
                            packet2.AddPackGUID(Target.GUID);
                            packet2.AddInt8(1);
                            ((WS_PlayerData.CharacterObject)Caster).client.Send(ref packet2);
                            packet2.Dispose();
                            ((WS_PlayerData.CharacterObject)Caster).cUnitFlags = ((WS_PlayerData.CharacterObject)Caster).cUnitFlags | UnitFlags.UNIT_FLAG_UNK21;
                            ((WS_PlayerData.CharacterObject)Caster).SetUpdateFlag(EPlayerFields.PLAYER_FARSIGHT, Target.GUID);
                            ((WS_PlayerData.CharacterObject)Caster).SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, ((WS_PlayerData.CharacterObject)Caster).cUnitFlags);
                            ((WS_PlayerData.CharacterObject)Caster).SendCharacterUpdate(false);
                            ((WS_PlayerData.CharacterObject)Caster).MindControl = Target;
                        }

                        if (Target is WS_Creatures.CreatureObject)
                        {
                            ((WS_Creatures.CreatureObject)Target).aiScript.Reset();
                        }
                        else if (Target is WS_PlayerData.CharacterObject)
                        {
                            var packet1 = new Packets.PacketClass(OPCODES.SMSG_DEATH_NOTIFY_OBSOLETE);
                            packet1.AddPackGUID(Target.GUID);
                            packet1.AddInt8(0);
                            ((WS_PlayerData.CharacterObject)Target).client.Send(ref packet1);
                            packet1.Dispose();
                        }

                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        var packet = new Packets.UpdatePacketClass();
                        var tmpUpdate = new Packets.UpdateClass(EUnitFields.UNIT_END);
                        tmpUpdate.SetUpdateFlag(EUnitFields.UNIT_FIELD_CHARMEDBY, 0);
                        if (Target is WS_PlayerData.CharacterObject)
                        {
                            tmpUpdate.SetUpdateFlag(EUnitFields.UNIT_FIELD_FACTIONTEMPLATE, ((WS_PlayerData.CharacterObject)Target).Faction);
                            tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, (WS_PlayerData.CharacterObject)Target);
                        }
                        else if (Target is WS_Creatures.CreatureObject)
                        {
                            tmpUpdate.SetUpdateFlag(EUnitFields.UNIT_FIELD_FACTIONTEMPLATE, ((WS_Creatures.CreatureObject)Target).Faction);
                            tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, (WS_Creatures.CreatureObject)Target);
                        }

                        Packets.PacketClass argpacket2 = packet;
                        Target.SendToNearPlayers(ref argpacket2);
                        packet.Dispose();
                        tmpUpdate.Dispose();
                        packet = new Packets.UpdatePacketClass();
                        tmpUpdate = new Packets.UpdateClass(EUnitFields.UNIT_END);
                        tmpUpdate.SetUpdateFlag(EUnitFields.UNIT_FIELD_CHARM, 0);
                        if (Caster is WS_PlayerData.CharacterObject)
                        {
                            tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, (WS_PlayerData.CharacterObject)Caster);
                        }
                        else if (Caster is WS_Creatures.CreatureObject)
                        {
                            tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, (WS_Creatures.CreatureObject)Caster);
                        }

                        Packets.PacketClass argpacket3 = packet;
                        Caster.SendToNearPlayers(ref argpacket3);
                        packet.Dispose();
                        tmpUpdate.Dispose();
                        if (Caster is WS_PlayerData.CharacterObject)
                        {
                            var packet1 = new Packets.PacketClass(OPCODES.SMSG_PET_SPELLS);
                            packet1.AddUInt64(0UL);
                            ((WS_PlayerData.CharacterObject)Caster).client.Send(ref packet1);
                            packet1.Dispose();
                            var packet2 = new Packets.PacketClass(OPCODES.SMSG_DEATH_NOTIFY_OBSOLETE);
                            packet2.AddPackGUID(Target.GUID);
                            packet2.AddInt8(0);
                            ((WS_PlayerData.CharacterObject)Caster).client.Send(ref packet2);
                            packet2.Dispose();
                            ((WS_PlayerData.CharacterObject)Caster).cUnitFlags = ((WS_PlayerData.CharacterObject)Caster).cUnitFlags & !UnitFlags.UNIT_FLAG_UNK21;
                            ((WS_PlayerData.CharacterObject)Caster).SetUpdateFlag(EPlayerFields.PLAYER_FARSIGHT, 0);
                            ((WS_PlayerData.CharacterObject)Caster).SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, ((WS_PlayerData.CharacterObject)Caster).cUnitFlags);
                            ((WS_PlayerData.CharacterObject)Caster).SendCharacterUpdate(false);
                            ((WS_PlayerData.CharacterObject)Caster).MindControl = null;
                        }

                        if (Target is WS_Creatures.CreatureObject)
                        {
                            ((WS_Creatures.CreatureObject)Target).aiScript.State = AIState.AI_ATTACKING;
                        }
                        else if (Target is WS_PlayerData.CharacterObject)
                        {
                            var packet1 = new Packets.PacketClass(OPCODES.SMSG_DEATH_NOTIFY_OBSOLETE);
                            packet1.AddPackGUID(Target.GUID);
                            packet1.AddInt8(1);
                            ((WS_PlayerData.CharacterObject)Target).client.Send(ref packet1);
                            packet1.Dispose();
                        }

                        break;
                    }
            }
        }

        public void SPELL_AURA_MOD_THREAT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {

            // NOTE: EffectInfo.MiscValue => DamageType (not used for now, till new combat sytem)
            // TODO: This does not work the correct way

            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        break;
                    }
                // Target.Spell_ThreatModifier *= EffectInfo.GetValue(CType(Caster, BaseUnit).Level)

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        break;
                    }
                    // Target.Spell_ThreatModifier /= EffectInfo.GetValue(CType(Caster, BaseUnit).Level)

            }
        }

        public void SPELL_AURA_MOD_TOTAL_THREAT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            var Value = default(int);
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        if (Target is WS_PlayerData.CharacterObject)
                        {
                            Value = EffectInfo.get_GetValue(Target.Level, 0);
                        }
                        else
                        {
                            Value = EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                        }

                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        if (Target is WS_PlayerData.CharacterObject)
                        {
                            Value = -EffectInfo.get_GetValue(Target.Level, 0);
                        }
                        else
                        {
                            Value = -EffectInfo.get_GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
                        }

                        break;
                    }
            }

            if (Target is WS_PlayerData.CharacterObject)
            {
                foreach (ulong CreatureGUID in ((WS_PlayerData.CharacterObject)Target).creaturesNear)
                {
                    if (WorldServiceLocator._WorldServer.WORLD_CREATUREs[CreatureGUID].aiScript is object && WorldServiceLocator._WorldServer.WORLD_CREATUREs[CreatureGUID].aiScript.aiHateTable.ContainsKey(Target))
                    {
                        WorldServiceLocator._WorldServer.WORLD_CREATUREs[CreatureGUID].aiScript.OnGenerateHate(ref Target, Value);
                    }
                }
            }
            else if (((WS_Creatures.CreatureObject)Target).aiScript is object && ((WS_Creatures.CreatureObject)Target).aiScript.aiHateTable.ContainsKey((WS_Base.BaseUnit)Caster))
            {
                WS_Base.BaseUnit argAttacker = (WS_Base.BaseUnit)Caster;
                ((WS_Creatures.CreatureObject)Target).aiScript.OnGenerateHate(ref argAttacker, Value);
            }
        }

        public void SPELL_AURA_MOD_TAUNT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
        {
            if (!(Target is WS_Creatures.CreatureObject))
                return;
            switch (Action)
            {
                case var @case when @case == AuraAction.AURA_UPDATE:
                    {
                        return;
                    }

                case var case1 when case1 == AuraAction.AURA_ADD:
                    {
                        WS_Base.BaseUnit argAttacker = (WS_Base.BaseUnit)Caster;
                        ((WS_Creatures.CreatureObject)Target).aiScript.OnGenerateHate(ref argAttacker, 9999999);
                        break;
                    }

                case var case2 when case2 == AuraAction.AURA_REMOVE:
                case var case3 when case3 == AuraAction.AURA_REMOVEBYDURATION:
                    {
                        WS_Base.BaseUnit argAttacker1 = (WS_Base.BaseUnit)Caster;
                        ((WS_Creatures.CreatureObject)Target).aiScript.OnGenerateHate(ref argAttacker1, -9999999);
                        break;
                    }
            }
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public const int DUEL_COUNTDOWN = 3000;              // in miliseconds
        private const float DUEL_OUTOFBOUNDS_DISTANCE = 40.0f;
        public const byte DUEL_COUNTER_START = 10;
        public const byte DUEL_COUNTER_DISABLED = 11;

        public void CheckDuelDistance(ref WS_PlayerData.CharacterObject objCharacter)
        {
            if (WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(objCharacter.DuelArbiter) == false)
                return;
            if (WorldServiceLocator._WS_Combat.GetDistance(objCharacter, WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[objCharacter.DuelArbiter]) > DUEL_OUTOFBOUNDS_DISTANCE)
            {
                if (objCharacter.DuelOutOfBounds == DUEL_COUNTER_DISABLED)
                {
                    // DONE: Notify for out of bounds of the duel flag and start counting
                    var packet = new Packets.PacketClass(OPCODES.SMSG_DUEL_OUTOFBOUNDS);
                    objCharacter.client.Send(ref packet);
                    packet.Dispose();
                    objCharacter.DuelOutOfBounds = DUEL_COUNTER_START;
                }
            }
            else if (objCharacter.DuelOutOfBounds != DUEL_COUNTER_DISABLED)
            {
                objCharacter.DuelOutOfBounds = DUEL_COUNTER_DISABLED;

                // DONE: Notify for in bounds of the duel flag
                var packet = new Packets.PacketClass(OPCODES.SMSG_DUEL_INBOUNDS);
                objCharacter.client.Send(ref packet);
                packet.Dispose();
            }
        }

        public void DuelComplete(ref WS_PlayerData.CharacterObject Winner, ref WS_PlayerData.CharacterObject Loser)
        {
            if (Winner is null)
                return;
            if (Loser is null)
                return;

            // DONE: First stop the fight
            var response = new Packets.PacketClass(OPCODES.SMSG_DUEL_COMPLETE);
            response.AddInt8(1);
            Winner.client.SendMultiplyPackets(ref response);
            Loser.client.SendMultiplyPackets(ref response);
            response.Dispose();

            // DONE: Stop timed attacks for both
            Winner.FinishSpell(CurrentSpellTypes.CURRENT_AUTOREPEAT_SPELL);
            Winner.FinishSpell(CurrentSpellTypes.CURRENT_CHANNELED_SPELL);
            Winner.AutoShotSpell = 0;
            Winner.attackState.AttackStop();
            Loser.FinishSpell(CurrentSpellTypes.CURRENT_AUTOREPEAT_SPELL);
            Loser.FinishSpell(CurrentSpellTypes.CURRENT_CHANNELED_SPELL);
            Loser.AutoShotSpell = 0;
            Loser.attackState.AttackStop();

            // DONE: Clear duel things
            if (WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(Winner.DuelArbiter))
                WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[Winner.DuelArbiter].Destroy(WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[Winner.DuelArbiter]);
            Winner.DuelOutOfBounds = DUEL_COUNTER_DISABLED;
            Winner.DuelArbiter = 0UL;
            Winner.SetUpdateFlag(EPlayerFields.PLAYER_DUEL_ARBITER, 0);
            Winner.SetUpdateFlag(EPlayerFields.PLAYER_DUEL_TEAM, 0);
            Winner.RemoveFromCombat(Loser);
            Loser.DuelOutOfBounds = DUEL_COUNTER_DISABLED;
            Loser.DuelArbiter = 0UL;
            Loser.SetUpdateFlag(EPlayerFields.PLAYER_DUEL_ARBITER, 0);
            Loser.SetUpdateFlag(EPlayerFields.PLAYER_DUEL_TEAM, 0);
            Loser.RemoveFromCombat(Winner);

            // DONE: Remove all spells by your duel partner
            for (int i = 0, loopTo = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1; i <= loopTo; i++)
            {
                if (Winner.ActiveSpells[i] is object)
                    Winner.RemoveAura(i, ref Winner.ActiveSpells[i].SpellCaster);
                if (Loser.ActiveSpells[i] is object)
                    Loser.RemoveAura(i, ref Loser.ActiveSpells[i].SpellCaster);
            }

            // DONE: Update life
            if (Loser.Life.Current == 0)
            {
                Loser.Life.Current = 1;
                Loser.SetUpdateFlag(EUnitFields.UNIT_FIELD_HEALTH, 1);
                Loser.SetUpdateFlag(EUnitFields.UNIT_NPC_EMOTESTATE, EmoteStates.ANIM_EMOTE_BEG);
            }

            Loser.SendCharacterUpdate(true);
            Winner.SendCharacterUpdate(true);

            // DONE: Notify client
            var packet = new Packets.PacketClass(OPCODES.SMSG_DUEL_WINNER);
            packet.AddInt8(0);
            packet.AddString(Winner.Name);
            packet.AddInt8(1);
            packet.AddString(Loser.Name);
            Winner.SendToNearPlayers(ref packet);
            packet.Dispose();

            // DONE: Beg emote for loser
            var SMSG_EMOTE = new Packets.PacketClass(OPCODES.SMSG_EMOTE);
            SMSG_EMOTE.AddInt32(Emotes.ONESHOT_BEG);
            SMSG_EMOTE.AddUInt64(Loser.GUID);
            Loser.SendToNearPlayers(ref SMSG_EMOTE);
            SMSG_EMOTE.Dispose();

            // DONE: Final clearing (if we clear it before we can't get names)
            WS_PlayerData.CharacterObject tmpCharacter;
            tmpCharacter = Winner;
            Loser.DuelPartner = null;
            tmpCharacter.DuelPartner = null;
        }

        public void On_CMSG_DUEL_ACCEPTED(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 13)
                return;
            packet.GetInt16();
            ulong GUID = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_DUEL_ACCEPTED [{2:X}]", client.IP, client.Port, GUID);

            // NOTE: Only invited player must have GUID set up
            if (client.Character.DuelArbiter != GUID)
                return;
            var c1 = client.Character.DuelPartner;
            var c2 = client.Character;
            c1.DuelArbiter = GUID;
            c2.DuelArbiter = GUID;

            // DONE: Do updates
            c1.SetUpdateFlag(EPlayerFields.PLAYER_DUEL_ARBITER, c1.DuelArbiter);
            // c1.SetUpdateFlag(EPlayerFields.PLAYER_DUEL_TEAM, 1)
            c2.SetUpdateFlag(EPlayerFields.PLAYER_DUEL_ARBITER, c2.DuelArbiter);
            // c2.SetUpdateFlag(EPlayerFields.PLAYER_DUEL_TEAM, 2)
            c2.SendCharacterUpdate(true);
            c1.SendCharacterUpdate(true);

            // DONE: Start the duel
            var response = new Packets.PacketClass(OPCODES.SMSG_DUEL_COUNTDOWN);
            response.AddInt32(DUEL_COUNTDOWN);
            c1.client.SendMultiplyPackets(ref response);
            c2.client.SendMultiplyPackets(ref response);
            response.Dispose();
            var StartDuel = new Thread(c2.StartDuel) { Name = "Duel timer" };
            StartDuel.Start();
        }

        public void On_CMSG_DUEL_CANCELLED(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 13)
                return;
            packet.GetInt16();
            ulong GUID = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_DUEL_CANCELLED [{2:X}]", client.IP, client.Port, GUID);

            // DONE: Clear for client
            WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[client.Character.DuelArbiter].Despawn();
            client.Character.DuelArbiter = 0UL;
            client.Character.DuelPartner.DuelArbiter = 0UL;
            var response = new Packets.PacketClass(OPCODES.SMSG_DUEL_COMPLETE);
            response.AddInt8(0);
            client.Character.client.SendMultiplyPackets(ref response);
            client.Character.DuelPartner.client.SendMultiplyPackets(ref response);
            response.Dispose();

            // DONE: Final clearing
            client.Character.DuelPartner.DuelPartner = null;
            client.Character.DuelPartner = null;
        }

        public void On_CMSG_RESURRECT_RESPONSE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 14)
                return;
            packet.GetInt16();
            ulong GUID = packet.GetUInt64();
            byte Status = packet.GetInt8();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_RESURRECT_RESPONSE [GUID={2:X} Status={3}]", client.IP, client.Port, GUID, Status);
            if (Status == 0)
            {
                // DONE: Decline the request
                client.Character.resurrectGUID = 0UL;
                client.Character.resurrectMapID = 0;
                client.Character.resurrectPositionX = 0f;
                client.Character.resurrectPositionY = 0f;
                client.Character.resurrectPositionZ = 0f;
                client.Character.resurrectHealth = 0;
                client.Character.resurrectMana = 0;
                return;
            }

            if (GUID != client.Character.resurrectGUID)
                return;

            // DONE: Resurrect
            WorldServiceLocator._WS_Handlers_Misc.CharacterResurrect(ref client.Character);
            client.Character.Life.Current = client.Character.resurrectHealth;
            if (client.Character.ManaType == ManaTypes.TYPE_MANA)
                client.Character.Mana.Current = client.Character.resurrectMana;
            client.Character.SetUpdateFlag(EUnitFields.UNIT_FIELD_HEALTH, client.Character.Life.Current);
            if (client.Character.ManaType == ManaTypes.TYPE_MANA)
                client.Character.SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER1, client.Character.Mana.Current);
            client.Character.SendCharacterUpdate();
            client.Character.Teleport(client.Character.resurrectPositionX, client.Character.resurrectPositionY, client.Character.resurrectPositionZ, client.Character.orientation, client.Character.resurrectMapID);
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public void On_CMSG_CAST_SPELL(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            // If (packet.Data.Length - 1) < 11 Then Exit Sub
            packet.GetInt16();
            int spellID = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CSMG_CAST_SPELL [spellID={2}]", client.IP, client.Port, spellID);
            if (!client.Character.HaveSpell(spellID))
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "[{0}:{1}] CHEAT: Character {2} casting unlearned spell {3}!", client.IP, client.Port, client.Character.Name, spellID);
                return;
            }

            if (SPELLs.ContainsKey(spellID) == false)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "[{0}:{1}] Character tried to cast a spell that didn't exist: {2}!", client.IP, client.Port, spellID);
                return;
            }

            uint spellCooldown = client.Character.Spells[spellID].Cooldown;
            if (spellCooldown >= 0U)
            {
                uint timeNow = WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now);
                if (timeNow >= spellCooldown)
                {
                    client.Character.Spells[spellID].Cooldown = 0U;
                    client.Character.Spells[spellID].CooldownItem = 0;
                }
                else
                {
                    return;
                } // The cooldown has not ran off yet
            }

            // TODO: In duel disable

            CurrentSpellTypes SpellType = CurrentSpellTypes.CURRENT_GENERIC_SPELL;
            if (SPELLs[spellID].IsAutoRepeat)
            {
                SpellType = CurrentSpellTypes.CURRENT_AUTOREPEAT_SPELL;
                int tmpSpellID = 0;
                if (client.Character.Items.ContainsKey(EquipmentSlots.EQUIPMENT_SLOT_RANGED))
                {
                    // Select Case client.Character.Items(EQUIPMENT_SLOT_RANGED).ItemInfo.SubClass
                    // Case ITEM_SUBCLASS.ITEM_SUBCLASS_BOW, ITEM_SUBCLASS.ITEM_SUBCLASS_GUN, ITEM_SUBCLASS.ITEM_SUBCLASS_CROSSBOW
                    // tmpSpellID = 3018
                    // Case ITEM_SUBCLASS.ITEM_SUBCLASS_THROWN
                    // tmpSpellID = 2764
                    // Case ITEM_SUBCLASS.ITEM_SUBCLASS_WAND
                    // tmpSpellID = 5019
                    // Case Else
                    // tmpSpellID = spellID
                    // End Select

                    if (client.Character.AutoShotSpell == 0)
                    {
                        try
                        {
                            client.Character.AutoShotSpell = spellID;
                            client.Character.attackState.Ranged = true;
                            client.Character.attackState.AttackStart(client.Character.GetTarget);
                        }
                        catch (Exception e)
                        {
                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error casting auto-shoot {0}.{1}", spellID, Environment.NewLine + e.ToString());
                        }
                    }
                }

                return;
            }
            else if (SPELLs[spellID].channelInterruptFlags != 0)
            {
                SpellType = CurrentSpellTypes.CURRENT_CHANNELED_SPELL;
            }
            else if (SPELLs[spellID].IsMelee)
            {
                SpellType = CurrentSpellTypes.CURRENT_MELEE_SPELL;
            }

            var Targets = new SpellTargets();
            SpellFailedReason castResult = SpellFailedReason.SPELL_FAILED_ERROR;
            try
            {
                Targets.ReadTargets(ref packet, ref (WS_Base.BaseObject)client.Character);
                castResult = SPELLs[spellID].CanCast(ref client.Character, Targets, true);
                if (client.Character.spellCasted[SpellType] is object && client.Character.spellCasted[SpellType].Finished == false)
                    castResult = SpellFailedReason.SPELL_FAILED_SPELL_IN_PROGRESS;
                if (castResult == SpellFailedReason.SPELL_NO_ERROR)
                {
                    WS_Base.BaseObject argCaster = client.Character;
                    var tmpSpell = new CastSpellParameters(ref Targets, ref argCaster, spellID);
                    client.Character.spellCasted[SpellType] = tmpSpell;
                    ThreadPool.QueueUserWorkItem(new WaitCallback(tmpSpell.Cast));
                }
                else
                {
                    SendCastResult(castResult, ref client, spellID);
                }
            }
            catch (Exception e)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error casting spell {0}.{1}", spellID, Environment.NewLine + e.ToString());
                SendCastResult(castResult, ref client, spellID);
            }
        }

        public void On_CMSG_CANCEL_CAST(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 9)
                return;
            packet.GetInt16();
            int SpellID = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CANCEL_CAST", client.IP, client.Port);

            // TODO: Other players can't see when you are interrupting your spells

            if (client.Character.spellCasted[CurrentSpellTypes.CURRENT_GENERIC_SPELL] is object && client.Character.spellCasted[CurrentSpellTypes.CURRENT_GENERIC_SPELL].SpellID == SpellID)
            {
                client.Character.FinishSpell(CurrentSpellTypes.CURRENT_GENERIC_SPELL);
            }
            else if (client.Character.spellCasted[CurrentSpellTypes.CURRENT_AUTOREPEAT_SPELL] is object && client.Character.spellCasted[CurrentSpellTypes.CURRENT_AUTOREPEAT_SPELL].SpellID == SpellID)
            {
                client.Character.FinishSpell(CurrentSpellTypes.CURRENT_AUTOREPEAT_SPELL);
            }
            else if (client.Character.spellCasted[CurrentSpellTypes.CURRENT_CHANNELED_SPELL] is object && client.Character.spellCasted[CurrentSpellTypes.CURRENT_CHANNELED_SPELL].SpellID == SpellID)
            {
                client.Character.FinishSpell(CurrentSpellTypes.CURRENT_CHANNELED_SPELL);
            }
            else if (client.Character.spellCasted[CurrentSpellTypes.CURRENT_MELEE_SPELL] is object && client.Character.spellCasted[CurrentSpellTypes.CURRENT_MELEE_SPELL].SpellID == SpellID)
            {
                client.Character.FinishSpell(CurrentSpellTypes.CURRENT_MELEE_SPELL);
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
            if (packet.Data.Length - 1 < 9)
                return;
            packet.GetInt16();
            int spellID = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CANCEL_AURA [spellID={2}]", client.IP, client.Port, spellID);
            client.Character.RemoveAuraBySpell(spellID);
        }

        public void On_CMSG_LEARN_TALENT(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            try
            {
                if (packet.Data.Length - 1 < 13)
                    return;
                packet.GetInt16();
                int TalentID = packet.GetInt32();
                int RequestedRank = packet.GetInt32();
                byte CurrentTalentPoints = client.Character.TalentPoints;
                int SpellID;
                int ReSpellID;
                int j;
                bool HasEnoughRank;
                int DependsOn;
                int SpentPoints;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_LEARN_TALENT [{2}:{3}]", client.IP, client.Port, TalentID, RequestedRank);
                if (CurrentTalentPoints == 0)
                    return;
                if (RequestedRank > 4)
                    return;

                // DONE: Now the character can't cheat, he must have the earlier rank to get the new one
                if (RequestedRank > 0)
                {
                    if (!client.Character.HaveSpell(WorldServiceLocator._WS_DBCDatabase.Talents[TalentID].RankID[RequestedRank - 1]))
                    {
                        return;
                    }
                }

                // DONE: Now the character can't cheat, he must have the other talents that is needed to get this one
                for (j = 0; j <= 2; j++)
                {
                    if (WorldServiceLocator._WS_DBCDatabase.Talents[TalentID].RequiredTalent[j] > 0)
                    {
                        HasEnoughRank = false;
                        DependsOn = WorldServiceLocator._WS_DBCDatabase.Talents[TalentID].RequiredTalent[j];
                        for (int i = WorldServiceLocator._WS_DBCDatabase.Talents[TalentID].RequiredPoints[j]; i <= 4; i++)
                        {
                            if (WorldServiceLocator._WS_DBCDatabase.Talents[DependsOn].RankID[i] != 0)
                            {
                                if (client.Character.HaveSpell(WorldServiceLocator._WS_DBCDatabase.Talents[DependsOn].RankID[i]))
                                {
                                    HasEnoughRank = true;
                                }
                            }
                        }

                        if (HasEnoughRank == false)
                            return;
                    }
                }

                // DONE: Count spent talent points
                SpentPoints = 0;
                if (WorldServiceLocator._WS_DBCDatabase.Talents[TalentID].Row > 0)
                {
                    foreach (KeyValuePair<int, WS_DBCDatabase.TalentInfo> TalentInfo in WorldServiceLocator._WS_DBCDatabase.Talents)
                    {
                        if (WorldServiceLocator._WS_DBCDatabase.Talents[TalentID].TalentTab == TalentInfo.Value.TalentTab)
                        {
                            for (int i = 0; i <= 4; i++)
                            {
                                if (TalentInfo.Value.RankID[i] != 0)
                                {
                                    if (client.Character.HaveSpell(TalentInfo.Value.RankID[i]))
                                    {
                                        SpentPoints += i + 1;
                                    }
                                }
                            }
                        }
                    }
                }

                /* TODO ERROR: Skipped IfDirectiveTrivia */
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "_WS_DBCDatabase.Talents spent: {0}", SpentPoints);
                /* TODO ERROR: Skipped EndIfDirectiveTrivia */
                if (SpentPoints < WorldServiceLocator._WS_DBCDatabase.Talents[TalentID].Row * 5)
                    return;
                SpellID = WorldServiceLocator._WS_DBCDatabase.Talents[TalentID].RankID[RequestedRank];
                if (SpellID == 0)
                    return;
                if (client.Character.HaveSpell(SpellID))
                    return;
                client.Character.LearnSpell(SpellID);

                // DONE: Cast passive talents on the character
                if (SPELLs.ContainsKey(SpellID) && SPELLs[SpellID].IsPassive)
                    client.Character.ApplySpell(SpellID);

                // DONE: Unlearning the earlier rank of the talent
                if (RequestedRank > 0)
                {
                    ReSpellID = WorldServiceLocator._WS_DBCDatabase.Talents[TalentID].RankID[RequestedRank - 1];
                    client.Character.UnLearnSpell(ReSpellID);
                    client.Character.RemoveAuraBySpell(ReSpellID);
                }

                // DONE: Remove 1 talentpoint from the character
                client.Character.TalentPoints = (byte)(client.Character.TalentPoints - 1);
                client.Character.SetUpdateFlag(EPlayerFields.PLAYER_CHARACTER_POINTS1, client.Character.TalentPoints);
                client.Character.SendCharacterUpdate(true);
                client.Character.SaveCharacter();
            }
            catch (Exception e)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error learning talen: {0}{1}", Environment.NewLine, e.ToString());
            }
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public void SendLoot(WS_PlayerData.CharacterObject Player, ulong GUID, LootType LootingType)
        {
            if (WorldServiceLocator._CommonGlobalFunctions.GuidIsGameObject(GUID))
            {
                switch (WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].ObjectInfo.Type)
                {
                    case var @case when @case == GameObjectType.GAMEOBJECT_TYPE_DOOR:
                    case var case1 when case1 == GameObjectType.GAMEOBJECT_TYPE_BUTTON:
                        {
                            return;
                        }

                    case var case2 when case2 == GameObjectType.GAMEOBJECT_TYPE_QUESTGIVER:
                        {
                            return;
                        }

                    case var case3 when case3 == GameObjectType.GAMEOBJECT_TYPE_SPELL_FOCUS:
                        {
                            return;
                        }

                    case var case4 when case4 == GameObjectType.GAMEOBJECT_TYPE_GOOBER:
                        {
                            return;
                        }

                    case var case5 when case5 == GameObjectType.GAMEOBJECT_TYPE_CHEST:
                        {
                            break;
                        }
                        // TODO: Script events
                        // Note: Don't exit sub here! We need the loot if it's a chest :P
                }
            }

            // DONE: Sending loot
            WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].LootObject(ref Player, LootingType);
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
    }
}