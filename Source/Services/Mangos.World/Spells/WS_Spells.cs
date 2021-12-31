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
using Mangos.World.Network;
using Mangos.World.Objects;
using Mangos.World.Player;
using Mangos.World.Quests;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mangos.World.Spells;

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

        public int GetDuration => WorldServiceLocator._WS_Spells.SpellDuration.ContainsKey(DurationIndex)
                    ? WorldServiceLocator._WS_Spells.SpellDuration[DurationIndex]
                    : 0;

        public int GetRange => WorldServiceLocator._WS_Spells.SpellRange.ContainsKey(rangeIndex)
                    ? (int)Math.Round(WorldServiceLocator._WS_Spells.SpellRange[rangeIndex])
                    : 0;

        public string GetFocusObject => WorldServiceLocator._WS_Spells.SpellFocusObject.ContainsKey(FocusObjectIndex)
                    ? WorldServiceLocator._WS_Spells.SpellFocusObject[FocusObjectIndex]
                    : Conversions.ToString(0);

        public int GetCastTime => WorldServiceLocator._WS_Spells.SpellCastTime.ContainsKey(SpellCastTimeIndex)
                    ? WorldServiceLocator._WS_Spells.SpellCastTime[SpellCastTimeIndex]
                    : 0;

        public int GetManaCost(int level, int Mana)
        {
            return checked((int)Math.Round(manaCost + (manaCostPerlevel * level) + (Mana * (manaCostPercent / 100.0))));
        }

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
                return SpellEffects[2] != null && SpellEffects[2].ApplyAuraIndex != 0;
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
                return SpellEffects[2] != null && SpellEffects[2].IsAOE;
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
                return SpellEffects[2] != null && SpellEffects[2].ID == SpellEffects_Names.SPELL_EFFECT_DISPEL;
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

        /// <summary>
        /// Function Totem GetTargets
        /// </summary>
        /// <param name="Caster"></param>
        /// <param name="Targets"></param>
        /// <param name="Index"></param>
        /// <returns></returns>
        public Dictionary<WS_Base.BaseObject, SpellMissInfo> GetTargets(ref WS_Base.BaseObject Caster, SpellTargets Targets, byte Index)
        {
            WS_Base.BaseUnit Ref = null;
            if (Caster is WS_Totems.TotemObject @object)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "[MapID:{0} X:{1} Y:{2} Z:{3}] Totem GetTargets [Caster = TotemObject: {4}]", Caster.MapID, Caster.positionX, Caster.positionY, Caster.positionZ, Caster is WS_Totems.TotemObject);
                Ref = @object.Caster;
            }
            if (SpellEffects[Index] != null)
            {
                byte i = 0;
                List<WS_Base.BaseObject> TargetsInfected = new();
                do
                {
                    var implicitTargetA = (byte)SpellEffects[Index].implicitTargetA;
                    SpellImplicitTargets ImplicitTarget = (SpellImplicitTargets)checked(implicitTargetA);
                    if (i == 1)
                    {
                        var implicitTargetB = (byte)SpellEffects[Index].implicitTargetB;
                        ImplicitTarget = (SpellImplicitTargets)checked(implicitTargetB);
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
                                    var targetMask = (uint)Targets.targetMask;
                                    if ((unchecked(targetMask) & 0x40u) != 0)
                                    {
                                        WS_Base.BaseUnit objCharacter = (WS_Base.BaseUnit)Caster;
                                        Caster = objCharacter;
                                        var wS_Spells6 = WorldServiceLocator._WS_Spells;
                                        var enemyAroundMe = wS_Spells6.GetEnemyAtPoint(ref objCharacter, Targets.dstX, Targets.dstY, Targets.dstZ, SpellEffects[Index].GetRadius);
                                        EnemyTargets2 = enemyAroundMe;
                                    }
                                    else if (Caster is WS_DynamicObjects.DynamicObject object1)
                                    {
                                        EnemyTargets2 = WorldServiceLocator._WS_Spells.GetEnemyAtPoint(ref object1.Caster, Caster.positionX, Caster.positionY, Caster.positionZ, SpellEffects[Index].GetRadius);
                                    }
                                    else
                                    {
                                        WS_Base.BaseUnit objCharacter = (WS_Base.BaseUnit)Caster;
                                        Caster = objCharacter;
                                        var wS_Spells7 = WorldServiceLocator._WS_Spells;
                                        var enemyAroundMe = wS_Spells7.GetEnemyAtPoint(ref objCharacter, Caster.positionX, Caster.positionY, Caster.positionZ, SpellEffects[Index].GetRadius);
                                        EnemyTargets2 = enemyAroundMe;
                                    }
                                    foreach (var EnemyTarget in EnemyTargets2)
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
                                    WS_Base.BaseUnit objCharacter = (WS_Base.BaseUnit)Caster;
                                    Caster = objCharacter;
                                    var wS_Spells5 = WorldServiceLocator._WS_Spells;
                                    var enemyAroundMe = wS_Spells5.GetEnemyAroundMe(ref objCharacter, SpellEffects[Index].GetRadius, Ref);
                                    var EnemyTargets6 = enemyAroundMe;
                                    foreach (var EnemyTarget5 in EnemyTargets6)
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
                                    WS_PlayerData.CharacterObject objCharacter2 = (WS_PlayerData.CharacterObject)Caster;
                                    Caster = objCharacter2;
                                    var wS_Spells10 = WorldServiceLocator._WS_Spells;
                                    var enemyAroundMe = wS_Spells10.GetPartyMembersAroundMe(ref objCharacter2, 9999999f);
                                    var PartyTargets2 = enemyAroundMe;
                                    foreach (var PartyTarget2 in PartyTargets2)
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
                                                ref var caster = ref object2.Caster;
                                                WS_PlayerData.CharacterObject objCharacter2 = (WS_PlayerData.CharacterObject)caster;
                                                caster = objCharacter2;
                                                var wS_Spells8 = WorldServiceLocator._WS_Spells;
                                                var enemyAroundMe = wS_Spells8.GetPartyMembersAtPoint(ref objCharacter2, SpellEffects[Index].GetRadius, Caster.positionX, Caster.positionY, Caster.positionZ);
                                                PartyTargets = enemyAroundMe;
                                                break;
                                            }

                                        default:
                                            {
                                                WS_PlayerData.CharacterObject objCharacter2 = (WS_PlayerData.CharacterObject)Caster;
                                                Caster = objCharacter2;
                                                var wS_Spells9 = WorldServiceLocator._WS_Spells;
                                                var enemyAroundMe = wS_Spells9.GetPartyMembersAroundMe(ref objCharacter2, SpellEffects[Index].GetRadius);
                                                PartyTargets = enemyAroundMe;
                                                break;
                                            }
                                    }
                                    foreach (var PartyTarget in PartyTargets)
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
                                    List<WS_Base.BaseUnit> UsedTargets = new();
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
                                    var b = (byte)SpellEffects[Index].ChainTarget;
                                    byte j = 2;
                                    while (j <= (uint)b)
                                    {
                                        WS_Base.BaseUnit objCharacter = (WS_Base.BaseUnit)Caster;
                                        Caster = objCharacter;
                                        var wS_Spells2 = WorldServiceLocator._WS_Spells;
                                        var enemyAroundMe = wS_Spells2.GetEnemyAroundMe(ref TargetUnit, 10f, objCharacter);
                                        var EnemyTargets = enemyAroundMe;
                                        TargetUnit = null;
                                        foreach (var tmpUnit in EnemyTargets)
                                        {
                                            if (!UsedTargets.Contains(tmpUnit))
                                            {
                                                var TmpLife = (float)(tmpUnit.Life.Current / (double)tmpUnit.Life.Maximum);
                                                var LowHealth = 1.01f;
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
                                    WS_Base.BaseUnit objCharacter = (WS_Base.BaseUnit)Caster;
                                    Caster = objCharacter;
                                    var wS_Spells = WorldServiceLocator._WS_Spells;
                                    var enemyAroundMe = wS_Spells.GetEnemyAroundMe(ref objCharacter, SpellEffects[Index].GetRadius, Ref);
                                    var EnemyTargets3 = enemyAroundMe;
                                    foreach (var EnemyTarget2 in EnemyTargets3)
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
                                    WS_Base.BaseUnit objCharacter = (WS_Base.BaseUnit)Caster;
                                    Caster = objCharacter;
                                    var wS_Spells3 = WorldServiceLocator._WS_Spells;
                                    var enemyAroundMe = wS_Spells3.GetEnemyInBehindMe(ref objCharacter, SpellEffects[Index].GetRadius);
                                    var EnemyTargets5 = enemyAroundMe;
                                    foreach (var EnemyTarget4 in EnemyTargets5)
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
                                    WS_Base.BaseUnit objCharacter = (WS_Base.BaseUnit)Caster;
                                    Caster = objCharacter;
                                    var wS_Spells4 = WorldServiceLocator._WS_Spells;
                                    var enemyAroundMe = wS_Spells4.GetEnemyInFrontOfMe(ref objCharacter, SpellEffects[Index].GetRadius);
                                    var EnemyTargets4 = enemyAroundMe;
                                    foreach (var EnemyTarget3 in EnemyTargets4)
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

        /// <summary>
        /// Function CalculateMisses
        /// </summary>
        /// <param name="Caster"></param>
        /// <param name="Targets"></param>
        /// <param name="SpellEffect"></param>
        /// <returns></returns>
        public Dictionary<WS_Base.BaseObject, SpellMissInfo> CalculateMisses(ref WS_Base.BaseObject Caster, ref List<WS_Base.BaseObject> Targets, ref SpellEffect SpellEffect)
        {
            Dictionary<WS_Base.BaseObject, SpellMissInfo> newTargets = new();
            foreach (var Target in Targets)
            {
                if (Target != Caster && Caster is WS_Base.BaseUnit unit && Target is WS_Base.BaseUnit unit1)
                {
                    var baseUnit = unit1;
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
                    else if (Caster is not WS_PlayerData.CharacterObject object4 || !object4.GM)
                    {
                        switch (DamageType)
                        {
                            case 0:
                                newTargets.Add(Target, SpellMissInfo.SPELL_MISS_NONE);
                                break;

                            case 1:
                                {
                                    var baseUnit3 = baseUnit;
                                    var Caster2 = unit;
                                    var meleeSpellHitResult = baseUnit3.GetMagicSpellHitResult(ref Caster2, this);
                                    Caster = Caster2;
                                    newTargets.Add(Target, meleeSpellHitResult);
                                    break;
                                }
                            case 2:
                            case 3:
                                {
                                    var baseUnit2 = baseUnit;
                                    var Caster2 = unit;
                                    var meleeSpellHitResult = baseUnit2.GetMeleeSpellHitResult(ref Caster2, this);
                                    Caster = Caster2;
                                    newTargets.Add(Target, meleeSpellHitResult);
                                    break;
                                }

                            default:
                                break;
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

        /// <summary>
        /// Function GetHits
        /// </summary>
        /// <param name="Targets"></param>
        /// <returns></returns>
        public List<WS_Base.BaseObject> GetHits(ref Dictionary<WS_Base.BaseObject, SpellMissInfo> Targets)
        {
            List<WS_Base.BaseObject> targetHits = new();
            foreach (var Target in Targets)
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
            var auraSpell = true;
            var i = 0;
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

            var spellFamilyFlags = (uint)SpellFamilyFlags;
            if (SpellFamilyName == 10 && (spellFamilyFlags & 0xC0000000u) != 0 && SpellEffects[0] != null)
            {
                SpellEffects[0].ID = SpellEffects_Names.SPELL_EFFECT_HEAL;
            }
            var j = 0;
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
                        default:
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
                        default:
                            break;
                    }
                }
                j = checked(j + 1);
            }
            while (j <= 2);
            var k = 0;
            do
            {
                if (SpellEffects[k] != null)
                {
                    var applyAuraIndex = SpellEffects[k].ApplyAuraIndex;
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
                default:
                    break;
            }
        }

        /// <summary>
        /// Function CastAsync
        /// </summary>
        /// <param name="castParams"></param>
        /// <returns></returns>
        public async Task CastAsync(CastSpellParameters castParams)
        {
            try
            {
                short CastFlags = 2;
                if (IsRanged)
                {
                    CastFlags = checked((short)(CastFlags | 0x20));
                }
                Packets.PacketClass spellStart = new(Opcodes.SMSG_SPELL_START);
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
                var Targets = castParams.Targets;
                Targets.WriteTargets(ref spellStart);
                var Caster = castParams.Caster;
                if (((uint)CastFlags & 0x20u) != 0)
                {
                    WS_Base.BaseUnit Caster2 = (WS_Base.BaseUnit)Caster;
                    WriteAmmoToPacket(ref spellStart, ref Caster2);
                    Caster = Caster2;
                }
                Caster.SendToNearPlayers(ref spellStart);
                spellStart.Dispose();
                castParams.State = SpellCastState.SPELL_STATE_PREPARING;
                castParams.Stopped = false;
                if (Caster is WS_Creatures.CreatureObject @object)
                {
                    if (Targets.unitTarget != null)
                    {
                        var obj = @object;
                        WS_Base.BaseObject unitTarget = Targets.unitTarget;
                        obj.TurnTo(ref unitTarget);
                    }
                    else if (((uint)Targets.targetMask & 0x40u) != 0)
                    {
                        @object.TurnTo(Targets.dstX, Targets.dstY);
                    }
                }
                var NeedSpellLog = true;
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
                    await Task.Delay(GetCastTime);
                    while (castParams.Delayed > 0)
                    {
                        var delayTime = castParams.Delayed;
                        castParams.Delayed = 0;
                        await Task.Delay(delayTime);
                    }
                }
                if (castParams.Stopped || castParams.State != SpellCastState.SPELL_STATE_PREPARING)
                {
                    castParams.Dispose();
                    return;
                }
                castParams.State = SpellCastState.SPELL_STATE_CASTING;
                var SpellTime = 0;
                if (Speed > 0f)
                {
                    var SpellDistance = 0f;
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
                var SpellCastError = SpellFailedReason.SPELL_NO_ERROR;
                if ((!castParams.InstantCast || GetCastTime == 0) && Caster is WS_PlayerData.CharacterObject object1)
                {
                    var Character = object1;
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
                var attributes = (uint)Attributes;
                if (((attributes & 4u) != 0 || (attributes & 0x400u) != 0) && Caster is WS_PlayerData.CharacterObject object2)
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
                        while (l <= 7u);
                        if (IsRanged && characterObject.AmmoID > 0)
                        {
                            characterObject.ItemCONSUME(characterObject.AmmoID, 1);
                        }

                        var attributesEx1 = (uint)AttributesEx;
                        switch (powerType)
                        {
                            case 0:
                                {
                                    var ManaCost = 0;
                                    var attributesEx = attributesEx1;
                                    if ((unchecked(attributesEx) & 2u) != 0)
                                    {
                                        characterObject.Mana.Current = 0;
                                        ManaCost = 1;
                                    }
                                    else
                                    {
                                        ManaCost = GetManaCost(characterObject.Level, characterObject.Mana.Base);
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
                                if ((unchecked(attributesEx1) & 2u) != 0)
                                {
                                    characterObject.Rage.Current = 0;
                                }
                                else
                                {
                                    characterObject.Rage.Current -= GetManaCost(characterObject.Level, characterObject.Rage.Base);
                                }
                                characterObject.SetUpdateFlag(24, characterObject.Rage.Current);
                                characterObject.GroupUpdateFlag |= 16u;
                                characterObject.SendCharacterUpdate();
                                break;

                            case -2:
                                if ((unchecked(attributesEx1) & 2u) != 0)
                                {
                                    characterObject.Life.Current = 1;
                                }
                                else
                                {
                                    characterObject.Life.Current -= GetManaCost(characterObject.Level, characterObject.Life.Base);
                                }
                                characterObject.SetUpdateFlag(22, characterObject.Life.Current);
                                characterObject.GroupUpdateFlag |= 2u;
                                characterObject.SendCharacterUpdate();
                                break;

                            case 3:
                                if ((unchecked(attributesEx1) & 2u) != 0)
                                {
                                    characterObject.Energy.Current = 0;
                                }
                                else
                                {
                                    characterObject.Energy.Current -= GetManaCost(characterObject.Level, characterObject.Energy.Base);
                                }
                                characterObject.SetUpdateFlag(26, characterObject.Energy.Current);
                                characterObject.GroupUpdateFlag |= 16u;
                                characterObject.SendCharacterUpdate();
                                break;
                            default:
                                break;
                        }
                        characterObject.RemoveAurasByInterruptFlag(32768);
                        if (Targets.unitTarget is not null and WS_Creatures.CreatureObject object3)
                        {
                            Character = (WS_PlayerData.CharacterObject)Caster;
                            var creature = object3;
                            var aLLQUESTS = WorldServiceLocator._WorldServer.ALLQUESTS;
                            aLLQUESTS.OnQuestCastSpell(ref Character, ref creature, ID);
                        }
                        if (Targets.goTarget != null)
                        {
                            Character = (WS_PlayerData.CharacterObject)Caster;
                            WS_GameObjects.GameObject gameObject = (WS_GameObjects.GameObject)Targets.goTarget;
                            var aLLQUESTS2 = WorldServiceLocator._WorldServer.ALLQUESTS;
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
                                    creatureObject.Mana.Current -= GetManaCost(creatureObject.Level, creatureObject.Mana.Base);
                                    Packets.UpdateClass powerUpdate2 = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
                                    powerUpdate2.SetUpdateFlag(23, creatureObject.Mana.Current);
                                    Packets.UpdatePacketClass updatePacket2 = new();
                                    Packets.PacketClass packet = updatePacket2;
                                    WS_Creatures.CreatureObject creature = (WS_Creatures.CreatureObject)Caster;
                                    powerUpdate2.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref creature);
                                    updatePacket2 = (Packets.UpdatePacketClass)packet;
                                    packet = updatePacket2;
                                    var creatureObject3 = creatureObject;
                                    creatureObject3.SendToNearPlayers(ref packet);
                                    updatePacket2 = (Packets.UpdatePacketClass)packet;
                                    powerUpdate2.Dispose();
                                    break;
                                }
                            case -2:
                                {
                                    creatureObject.Life.Current -= GetManaCost(creatureObject.Level, creatureObject.Life.Base);
                                    Packets.UpdateClass powerUpdate = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
                                    powerUpdate.SetUpdateFlag(22, creatureObject.Life.Current);
                                    Packets.UpdatePacketClass updatePacket = new();
                                    Packets.PacketClass packet = updatePacket;
                                    WS_Creatures.CreatureObject creature = (WS_Creatures.CreatureObject)Caster;
                                    powerUpdate.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref creature);
                                    updatePacket = (Packets.UpdatePacketClass)packet;
                                    packet = updatePacket;
                                    var creatureObject2 = creatureObject;
                                    creatureObject2.SendToNearPlayers(ref packet);
                                    updatePacket = (Packets.UpdatePacketClass)packet;
                                    powerUpdate.Dispose();
                                    break;
                                }

                            default:
                                break;
                        }
                    }
                    castParams.State = SpellCastState.SPELL_STATE_FINISHED;
                    if (Caster is WS_PlayerData.CharacterObject object4)
                    {
                        WorldServiceLocator._WS_Spells.SendCastResult(SpellFailedReason.SPELL_NO_ERROR, ref object4.client, ID);
                    }
                    Dictionary<ulong, SpellMissInfo> tmpTargets = new();
                    byte j = 0;
                    do
                    {
                        foreach (var tmpTarget in TargetsInfected[j])
                        {
                            if (!tmpTargets.ContainsKey(tmpTarget.Key.GUID))
                            {
                                tmpTargets.Add(tmpTarget.Key.GUID, tmpTarget.Value);
                            }
                        }
                        j = (byte)unchecked((uint)(j + 1));
                    }
                    while (j <= 2u);
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
                                var slot = object5.ItemGetSLOTBAG(castParams.Item.GUID, ref bag);
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
                        await Task.Delay(SpellTime);
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
                        var iD = (int)SpellEffects[i].ID;
                        SpellCastError = WorldServiceLocator._WS_Spells.SPELL_EFFECTs[iD](ref Targets, ref Caster, ref SpellEffects[i], ID, ref TargetHits[i], ref castParams.Item);
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
                        case WS_PlayerData.CharacterObject:
                            {
                                WS_PlayerData.CharacterObject caster = (WS_PlayerData.CharacterObject)Caster;
                                WorldServiceLocator._WS_Spells.SendCastResult(SpellCastError, ref caster.client, ID);
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
                    await Task.Delay(GetDuration);
                    castParams.State = SpellCastState.SPELL_STATE_IDLE;
                }
            }
            catch (Exception e)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error when casting spell. {0}", Environment.NewLine + e);
            }
            if (castParams != null)
            {
                try
                {
                    castParams.Dispose();
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Unable to dispose of castParams. {0}:{1}", castParams, Environment.NewLine + ex);
                }
            }
        }

        /// <summary>
        /// Function Apply
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="Targets"></param>
        public void Apply(ref WS_Base.BaseObject caster, SpellTargets Targets)
        {
            List<WS_Base.BaseObject>[] TargetsInfected = new List<WS_Base.BaseObject>[3];
            var Targets2 = GetTargets(ref caster, Targets, 0);
            TargetsInfected[0] = GetHits(ref Targets2);
            Targets2 = GetTargets(ref caster, Targets, 1);
            TargetsInfected[1] = GetHits(ref Targets2);
            Targets2 = GetTargets(ref caster, Targets, 2);
            TargetsInfected[2] = GetHits(ref Targets2);
            if (SpellEffects[0] != null)
            {
                var iD = (int)SpellEffects[0].ID;
                var obj = WorldServiceLocator._WS_Spells.SPELL_EFFECTs[iD];
                ref var spellInfo = ref SpellEffects[0];
                var iD1 = ID;
                ref var infected = ref TargetsInfected[0];
                ItemObject Item = null;
                obj(ref Targets, ref caster, ref spellInfo, iD1, ref infected, ref Item);
            }
            if (SpellEffects[1] != null)
            {
                var iD = (int)SpellEffects[1].ID;
                var obj2 = WorldServiceLocator._WS_Spells.SPELL_EFFECTs[iD];
                ref var spellInfo2 = ref SpellEffects[1];
                var iD2 = ID;
                ref var infected2 = ref TargetsInfected[1];
                ItemObject Item = null;
                obj2(ref Targets, ref caster, ref spellInfo2, iD2, ref infected2, ref Item);
            }
            if (SpellEffects[2] != null)
            {
                var iD = (int)SpellEffects[2].ID;
                var obj3 = WorldServiceLocator._WS_Spells.SPELL_EFFECTs[iD];
                ref var spellInfo3 = ref SpellEffects[2];
                var iD3 = ID;
                ref var infected3 = ref TargetsInfected[2];
                ItemObject Item = null;
                obj3(ref Targets, ref caster, ref spellInfo3, iD3, ref infected3, ref Item);
            }
        }

        /// <summary>
        /// SpellFailedReason CanCast
        /// </summary>
        /// <param name="Character"></param>
        /// <param name="Targets"></param>
        /// <param name="FirstCheck"></param>
        /// <returns></returns>
        public SpellFailedReason CanCast(ref WS_PlayerData.CharacterObject Character, SpellTargets Targets, bool FirstCheck)
        {
            if (Character == null)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "WS_Spells:CanCast Null Character Object");
                return SpellFailedReason.SPELL_FAILED_ERROR;
            }
            if (Targets == null)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "WS_Spells:CanCast Null Target Object");
                return SpellFailedReason.SPELL_FAILED_ERROR;
            }
            if (Character != null)
            {
                if (Character.Spell_Silenced)
                {
                    return SpellFailedReason.SPELL_FAILED_SILENCED;
                }
            }

            if (((uint)Character.cUnitFlags & 0x100000u) != 0)
            {
                return SpellFailedReason.SPELL_FAILED_ERROR;
            }
            if (Targets.unitTarget != null && Targets.unitTarget != Character)
            {
                if (((uint)FacingCasterFlags & (true ? 1u : 0u)) != 0)
                {
                    var wS_Combat = WorldServiceLocator._WS_Combat;
                    WS_Base.BaseObject Object = Character;
                    WS_Base.BaseObject Object2 = Targets.unitTarget;
                    Character = (WS_PlayerData.CharacterObject)Object;
                    var flag = wS_Combat.IsInFrontOf(ref Object, ref Object2);
                    if (!flag)
                    {
                        return SpellFailedReason.SPELL_FAILED_NOT_INFRONT;
                    }
                }
                if (((uint)FacingCasterFlags & 2u) != 0)
                {
                    var wS_Combat2 = WorldServiceLocator._WS_Combat;
                    WS_Base.BaseObject Object2 = Character;
                    WS_Base.BaseObject Object = Targets.unitTarget;
                    Character = (WS_PlayerData.CharacterObject)Object2;
                    var flag = wS_Combat2.IsInBackOf(ref Object2, ref Object);
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
            // Integrity/Cheat Checks
            // TODO: Move these into their own MovementHackViolation file under AntiCheat
            if (Character.IsInCombat && Character.IsDead)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "[Integrity Check Failed] Player:{0} IsInCombat && IsDead at [MapID:{1} X:{2} Y:{3} Z:{4}]", Character.Name, Character.MapID, Character.positionX, Character.positionY, Character.positionZ);
                Character.Logout();
            }
            if (Character.IsInDuel && Character.IsDead)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "[Integrity Check Failed] Player:{0} IsInDuel && IsDead at [MapID:{1} X:{2} Y:{3} Z:{4}]", Character.Name, Character.MapID, Character.positionX, Character.positionY, Character.positionZ);
                Character.Logout();
            }
            if (Character.IsRooted && Character.IsDead)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "[Integrity Check Failed] Player:{0} IsRooted && IsDead at [MapID:{1} X:{2} Y:{3} Z:{4}]", Character.Name, Character.MapID, Character.positionX, Character.positionY, Character.positionZ);
                Character.Logout();
            }
            if (Character.IsInFeralForm && Character.IsDead)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "[Integrity Check Failed] Player:{0} IsInFeralForm && IsDead at [MapID:{1} X:{2} Y:{3} Z:{4}]", Character.Name, Character.MapID, Character.positionX, Character.positionY, Character.positionZ);
                Character.Logout();
            }
            if (Character.isMovingOrTurning && Character.IsStunned)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "[Integrity Check Failed] Player:{0} IsMovingOrTurning && IsStunned at [MapID:{1} X:{2} Y:{3} Z:{4}]", Character.Name, Character.MapID, Character.positionX, Character.positionY, Character.positionZ);
                Character.Logout();
            }
            if (Character.IsRooted && Character.isMoving)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "[Integrity Check Failed] Player:{0} IsRooted && IsMoving at [MapID:{1} X:{2} Y:{3} Z:{4}]", Character.Name, Character.MapID, Character.positionX, Character.positionY, Character.positionZ);
                Character.Logout();
            }
            var StanceMask = 0;
            checked
            {
                if (Character.ShapeshiftForm != 0)
                {
                    var shapeshiftForm = (int)Character.ShapeshiftForm;
                    StanceMask = 1 << shapeshiftForm - 1;
                }
                if ((StanceMask & ShapeshiftExclude) != 0)
                {
                    return SpellFailedReason.SPELL_FAILED_NOT_SHAPESHIFT;
                }
            }
            if ((StanceMask & RequredCasterStance) == 0)
            {
                var actAsShifted = false;
                if (Character.ShapeshiftForm != 0)
                {
                    var shapeshiftForm = (int)Character.ShapeshiftForm;
                    var ShapeShift = WorldServiceLocator._WS_DBCDatabase.FindShapeshiftForm(shapeshiftForm);
                    if (ShapeShift == null)
                    {
                        goto IL_028d;
                    }
                    actAsShifted = (ShapeShift.Flags1 & 1) == 0;
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

        IL_028d:
            if (((uint)Attributes & 0x20000u & (0u - ((Character.Invisibility != InvisibilityLevel.STEALTH) ? 1u : 0u))) != 0)
            {
                return SpellFailedReason.SPELL_FAILED_ONLY_STEALTHED;
            }
            if ((Character.charMovementFlags & WorldServiceLocator._Global_Constants.movementFlagsMask) != 0 && ((Character.charMovementFlags & 0x4000) == 0 || SpellEffects[0].ID != SpellEffects_Names.SPELL_EFFECT_STUCK) && (IsAutoRepeat || ((uint)auraInterruptFlags & 0x40000u) != 0))
            {
                return SpellFailedReason.SPELL_FAILED_MOVING;
            }
            var ManaCost = GetManaCost(Character.Level, Character.Mana.Base);
            if (ManaCost > 0)
            {
                var manaType = (int)Character.ManaType;
                if (powerType != manaType)
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
                var FoundItem = false;
                var i = 0;
                do
                {
                    if ((EquippedItemSubClass & (1 << i)) != 0)
                    {
                        var objectClass = (int)Character.Items[15].ItemInfo.ObjectClass;
                        switch (i)
                        {
                            case 1:
                            case 5:
                            case 6:
                            case 8:
                            case 10:
                            case 17:
                            case 20:
                                if (Character.Items.ContainsKey(15) && !Character.Items[15].IsBroken() && objectClass == EquippedItemClass)
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
                                if (Character.Items.ContainsKey(15) && !Character.Items[15].IsBroken() && objectClass == EquippedItemClass)
                                {
                                    FoundItem = true;
                                    break;
                                }

                                var objectClass1 = (int)Character.Items[16].ItemInfo.ObjectClass;
                                if (Character.Items.ContainsKey(16) && !Character.Items[16].IsBroken() && objectClass1 == EquippedItemClass)
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
                                    switch (i)
                                    {
                                        case 2 or 18 or 3:
                                            if (Character.AmmoID == 0)
                                            {
                                                return SpellFailedReason.SPELL_FAILED_NO_AMMO;
                                            }
                                            if (Character.ItemCOUNT(Character.AmmoID) == 0)
                                            {
                                                return SpellFailedReason.SPELL_FAILED_NO_AMMO;
                                            }
                                            break;
                                        case 16 when Character.ItemCOUNT(Character.Items[17].ItemEntry) != 0:
                                            return SpellFailedReason.SPELL_FAILED_NO_AMMO;
                                        default:
                                            break;
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
                                        selectedX = (float)(Character.positionX + (Math.Cos(Character.orientation) * SpellEffects[j].GetRadius));
                                        selectedY = (float)(Character.positionY + (Math.Sin(Character.orientation) * SpellEffects[j].GetRadius));
                                    }
                                    else
                                    {
                                        selectedX = (float)(Character.positionX + (Math.Cos(Character.orientation) * GetRange));
                                        selectedY = (float)(Character.positionY + (Math.Sin(Character.orientation) * GetRange));
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
                var wS_Maps = WorldServiceLocator._WS_Maps;
                WS_Base.BaseObject Object = Character;
                ref var unitTarget = ref Targets.unitTarget;
                WS_Base.BaseObject Object2 = unitTarget;
                unitTarget = (WS_Base.BaseUnit)Object2;
                Character = (WS_PlayerData.CharacterObject)Object;
                var flag = wS_Maps.IsInLineOfSight(ref Object, ref Object2);
                if (!flag)
                {
                    return SpellFailedReason.SPELL_FAILED_LINE_OF_SIGHT;
                }
            }
            else if (((uint)Targets.targetMask & 0x40u) != 0)
            {
                var wS_Maps2 = WorldServiceLocator._WS_Maps;
                WS_Base.BaseObject Object2 = Character;
                Character = (WS_PlayerData.CharacterObject)Object2;
                var flag = wS_Maps2.IsInLineOfSight(ref Object2, Targets.dstX, Targets.dstY, Targets.dstZ);
                if (!flag)
                {
                    return SpellFailedReason.SPELL_FAILED_LINE_OF_SIGHT;
                }
            }
            return SpellFailedReason.SPELL_NO_ERROR;
        }

        /// <summary>
        /// Function StartChannel
        /// </summary>
        /// <param name="Caster"></param>
        /// <param name="Duration"></param>
        /// <param name="Targets"></param>
        public void StartChannel(ref WS_Base.BaseUnit Caster, int Duration, ref SpellTargets Targets)
        {
            switch (Caster)
            {
                case WS_PlayerData.CharacterObject:
                    {
                        Packets.PacketClass packet = new(Opcodes.MSG_CHANNEL_START);
                        packet.AddInt32(ID);
                        packet.AddInt32(Duration);
                        WS_PlayerData.CharacterObject caster = (WS_PlayerData.CharacterObject)Caster;
                        caster.client.Send(ref packet);
                        packet.Dispose();
                        break;
                    }

                default:
                    if (Caster is not WS_Creatures.CreatureObject)
                    {
                        return;
                    }

                    break;
            }
            Packets.UpdatePacketClass updatePacket = new();
            Packets.UpdateClass updateBlock = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
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
                var updateObject2 = @object;
                updateBlock.AddToPacket(ref packet2, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject2);
                updatePacket = (Packets.UpdatePacketClass)packet2;
            }
            var obj = Caster;
            packet2 = updatePacket;
            obj.SendToNearPlayers(ref packet2);
            updatePacket = (Packets.UpdatePacketClass)packet2;
            updatePacket.Dispose();
        }

        /// <summary>
        /// Function WriteAmmoToPacket
        /// </summary>
        /// <param name="Packet"></param>
        /// <param name="Caster"></param>
        public void WriteAmmoToPacket(ref Packets.PacketClass Packet, ref WS_Base.BaseUnit Caster)
        {
            WS_Items.ItemInfo ItemInfo = null;
            if (Caster is WS_PlayerData.CharacterObject characterObject)
            {
                var RangedItem = characterObject.ItemGET(0, 17);
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
                    WS_Items.ItemInfo tmpInfo = new(2512);
                    WorldServiceLocator._WorldServer.ITEMDatabase.Add(2512, tmpInfo);
                }
                ItemInfo = WorldServiceLocator._WorldServer.ITEMDatabase[2512];
            }
            Packet.AddInt32(ItemInfo.Model);
            Packet.AddInt32((int)ItemInfo.InventoryType);
        }

        /// <summary>
        /// Function SendInterrupted
        /// </summary>
        /// <param name="result"></param>
        /// <param name="Caster"></param>
        public void SendInterrupted(byte result, ref WS_Base.BaseUnit Caster)
        {
            if (Caster is WS_PlayerData.CharacterObject @object)
            {
                Packets.PacketClass packet = new(Opcodes.SMSG_SPELL_FAILURE);
                packet.AddUInt64(Caster.GUID);
                packet.AddInt32(ID);
                packet.AddInt8(result);
                @object.client.Send(ref packet);
                packet.Dispose();
            }
            Packets.PacketClass packet2 = new(Opcodes.SMSG_SPELL_FAILED_OTHER);
            packet2.AddUInt64(Caster.GUID);
            packet2.AddInt32(ID);
            Caster.SendToNearPlayers(ref packet2);
            packet2.Dispose();
        }

        /// <summary>
        /// Function SendSpellGo
        /// </summary>
        /// <param name="Caster"></param>
        /// <param name="Targets"></param>
        /// <param name="InfectedTargets"></param>
        /// <param name="Item"></param>
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
                var hits = 0;
                var misses = 0;
                foreach (var InfectedTarget in InfectedTargets)
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
                foreach (var Target2 in InfectedTargets)
                {
                    if (Target2.Value == SpellMissInfo.SPELL_MISS_NONE)
                    {
                        packet.AddUInt64(Target2.Key);
                    }
                }
                packet.AddInt8((byte)misses);
            }
            foreach (var Target in InfectedTargets)
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
            Caster.SendToNearPlayers(ref packet);
            packet.Dispose();
        }

        /// <summary>
        /// SendSpellMiss Log Packet
        /// </summary>
        /// <param name="Caster"></param>
        /// <param name="Target"></param>
        /// <param name="MissInfo"></param>
        public void SendSpellMiss(ref WS_Base.BaseObject Caster, ref WS_Base.BaseUnit Target, SpellMissInfo MissInfo)
        {
            Packets.PacketClass packet = new(Opcodes.SMSG_SPELLLOGMISS);
            packet.AddInt32(ID);
            packet.AddUInt64(Caster.GUID);
            packet.AddInt8(0);
            packet.AddInt32(1);
            packet.AddUInt64(Target.GUID);
            packet.AddInt8((byte)MissInfo);
            Caster.SendToNearPlayers(ref packet);
            packet.Dispose();
        }

        /// <summary>
        /// SendSpellExecute Packet
        /// </summary>
        /// <param name="Caster"></param>
        /// <param name="Targets"></param>
        public void SendSpellLog(ref WS_Base.BaseObject Caster, ref SpellTargets Targets)
        {
            Packets.PacketClass packet = new(Opcodes.SMSG_SPELLLOGEXECUTE);
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
            var numOfSpellEffects = 1;
            packet.AddInt32(numOfSpellEffects);
            var UnitTargetGUID = 0uL;
            if (Targets.unitTarget != null)
            {
                UnitTargetGUID = Targets.unitTarget.GUID;
            }
            var ItemTargetGUID = 0uL;
            if (Targets.itemTarget != null)
            {
                ItemTargetGUID = Targets.itemTarget.GUID;
            }
            var num = checked(numOfSpellEffects - 1);
            for (var i = 0; i <= num; i = checked(i + 1))
            {
                var iD = (int)SpellEffects[i].ID;
                packet.AddInt32(iD);
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
            Caster.SendToNearPlayers(ref packet);
            packet.Dispose();
        }

        /// <summary>
        /// Function SendSpellCooldown
        /// </summary>
        /// <param name="objCharacter"></param>
        /// <param name="castItem"></param>
        public void SendSpellCooldown(ref WS_PlayerData.CharacterObject objCharacter, ItemObject castItem = null)
        {
            if (!objCharacter.Spells.ContainsKey(ID))
            {
                return;
            }
            var Recovery = SpellCooldown;
            var CatRecovery = CategoryCooldown;
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
                    var i = 0;
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
                objCharacter.Spells[ID].Cooldown = (uint)(WorldServiceLocator._Functions.GetTimestamp(DateTime.Now) + (Recovery / 1000));
                if (castItem != null)
                {
                    objCharacter.Spells[ID].CooldownItem = castItem.ItemEntry;
                }
                if (Recovery > 10000)
                {
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_spells SET cooldown={2}, cooldownitem={3} WHERE guid = {0} AND spellid = {1};", objCharacter.GUID, ID, objCharacter.Spells[ID].Cooldown, objCharacter.Spells[ID].CooldownItem));
                }
                Packets.PacketClass packet = new(Opcodes.SMSG_SPELL_COOLDOWN);
                packet.AddUInt64(objCharacter.GUID);
                if (CatRecovery > 0)
                {
                    foreach (var Spell in objCharacter.Spells)
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

        public float GetRadius => WorldServiceLocator._WS_Spells.SpellRadius.ContainsKey(RadiusIndex)
                    ? WorldServiceLocator._WS_Spells.SpellRadius[RadiusIndex]
                    : 0f;

        /// <summary>
        /// Integer GetValue
        /// </summary>
        /// <param name="Level"></param>
        /// <param name="ComboPoints"></param>
        /// <returns></returns>
        public int GetValue(int Level, int ComboPoints)
        {
            checked
            {
                try
                {
                    return valueBase + (Level * valuePerLevel) + (ComboPoints * valuePerComboPoint) + WorldServiceLocator._WorldServer.Rnd.Next(1, (int)Math.Round(valueDie + (Level * dicePerLevel)));
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "GetValue returned an Exception:{0}, valueBase:{1}", ex, valueBase);
                    var GetValue = valueBase + (Level * valuePerLevel) + (ComboPoints * valuePerComboPoint) + 1;
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
                checked
                {
                    var i = 0;
                    do
                    {
                        var implicitTargetB1 = (byte)implicitTargetB;
                        var implicitTargetA1 = (byte)implicitTargetA;
                        switch ((i != 0) ? implicitTargetB1 : implicitTargetA1)
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
                            default:
                                break;
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

        /// <summary>
        /// Function ReadTargets
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="Caster"></param>
        public void ReadTargets(ref Packets.PacketClass packet, ref WS_Base.BaseObject Caster)
        {
            targetMask = packet.GetInt16();
            if (targetMask == 0)
            {
                unitTarget = (WS_Base.BaseUnit)Caster;
                return;
            }

            var targetMask1 = (uint)targetMask;
            if ((targetMask1 & 2u) != 0)
            {
                var GUID = packet.GetPackGuid();
                if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(GUID) || WorldServiceLocator._CommonGlobalFunctions.GuidIsPet(GUID))
                {
                    unitTarget = WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID];
                }
                else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(GUID))
                {
                    unitTarget = WorldServiceLocator._WorldServer.CHARACTERs[GUID];
                }
            }
            if ((targetMask1 & 0x800u) != 0)
            {
                var GUID3 = packet.GetPackGuid();
                if (WorldServiceLocator._CommonGlobalFunctions.GuidIsGameObject(GUID3))
                {
                    goTarget = WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID3];
                }
                else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsDnyamicObject(GUID3))
                {
                    goTarget = WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs[GUID3];
                }
            }
            if ((targetMask1 & 0x10u) != 0 || (targetMask1 & 0x1000u) != 0)
            {
                var GUID4 = packet.GetPackGuid();
                if (WorldServiceLocator._CommonGlobalFunctions.GuidIsItem(GUID4))
                {
                    itemTarget = WorldServiceLocator._WorldServer.WORLD_ITEMs[GUID4];
                }
            }
            if ((targetMask1 & 0x20u) != 0)
            {
                srcX = packet.GetFloat();
                srcY = packet.GetFloat();
                srcZ = packet.GetFloat();
            }
            if ((targetMask1 & 0x40u) != 0)
            {
                dstX = packet.GetFloat();
                dstY = packet.GetFloat();
                dstZ = packet.GetFloat();
            }
            if ((targetMask1 & 0x2000u) != 0)
            {
                stringTarget = packet.GetString();
            }
            if ((targetMask1 & 0x8000u) != 0 || (targetMask1 & 0x200u) != 0)
            {
                var GUID2 = packet.GetPackGuid();
                if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCorpse(GUID2))
                {
                    corpseTarget = WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs[GUID2];
                }
            }
        }

        /// <summary>
        /// Function WriteTargets
        /// </summary>
        /// <param name="packet"></param>
        public void WriteTargets(ref Packets.PacketClass packet)
        {
            var targetMask1 = (short)targetMask;
            packet.AddInt16(checked(targetMask1));
            var targetMask2 = (uint)targetMask;
            if ((targetMask2 & 2u) != 0)
            {
                packet.AddPackGUID(unitTarget.GUID);
            }
            if ((targetMask2 & 0x800u) != 0)
            {
                packet.AddPackGUID(goTarget.GUID);
            }
            if ((targetMask2 & 0x10u) != 0 || (targetMask2 & 0x1000u) != 0)
            {
                packet.AddPackGUID(itemTarget.GUID);
            }
            if ((targetMask2 & 0x20u) != 0)
            {
                packet.AddSingle(srcX);
                packet.AddSingle(srcY);
                packet.AddSingle(srcZ);
            }
            if ((targetMask2 & 0x40u) != 0)
            {
                packet.AddSingle(dstX);
                packet.AddSingle(dstY);
                packet.AddSingle(dstZ);
            }
            if ((targetMask2 & 0x2000u) != 0)
            {
                packet.AddString(stringTarget);
            }
            if ((targetMask2 & 0x8000u) != 0 || (targetMask2 & 0x200u) != 0)
            {
                packet.AddPackGUID(corpseTarget.GUID);
            }
        }

        /// <summary>
        /// Function SetTarget_SELF
        /// </summary>
        /// <param name="objCharacter"></param>
        public void SetTarget_SELF(ref WS_Base.BaseUnit objCharacter)
        {
            unitTarget = objCharacter;
            checked
            {
                targetMask += 0;
            }
        }

        /// <summary>
        /// Function SetTarget_UNIT
        /// </summary>
        /// <param name="objCharacter"></param>
        public void SetTarget_UNIT(ref WS_Base.BaseUnit objCharacter)
        {
            unitTarget = objCharacter;
            checked
            {
                targetMask += 2;
            }
        }

        /// <summary>
        /// Function SetTarget_OBJECT
        /// </summary>
        /// <param name="o"></param>
        public void SetTarget_OBJECT(ref WS_Base.BaseObject o)
        {
            goTarget = o;
            checked
            {
                targetMask += 2048;
            }
        }

        /// <summary>
        /// Function SetTarget_ITEM
        /// </summary>
        /// <param name="i"></param>
        public void SetTarget_ITEM(ref ItemObject i)
        {
            itemTarget = i;
            checked
            {
                targetMask += 16;
            }
        }

        /// <summary>
        /// Function SetTarget_SOURCELOCATION
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
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

        /// <summary>
        /// Function SetTarget_DESTINATIONLOCATION
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
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

        /// <summary>
        /// Function SetTarget_STRING
        /// </summary>
        /// <param name="str"></param>
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

        /// <summary>
        /// Function Cast
        /// </summary>
        /// <param name="status"></param>
        public async void Cast(object status)
        {
            try
            {
                Stopped = false;
                var spellInfo = WorldServiceLocator._WS_Spells.SPELLs[SpellID];
                var castParams = this;
                await spellInfo.CastAsync(castParams);
            }
            catch (Exception ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Cast Exception {0} : Interrupted {1} : Exception {2}", SpellID, Stopped, ex);
            }
        }

        /// <summary>
        /// Function StopCast
        /// </summary>
        public void StopCast()
        {
            try
            {
                Stopped = true;
            }
            catch (Exception ex2)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "StopCast Exception {0} : Interrupted {1} : Exception {2}", SpellID, Stopped, ex2);
            }
        }

        /// <summary>
        /// Function Delay
        /// </summary>
        public void Delay()
        {
            checked
            {
                if (Caster != null && !Finished)
                {
                    WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                    var resistChance = caster.GetAuraModifier(AuraEffects_Names.SPELL_AURA_RESIST_PUSHBACK);
                    if (resistChance <= 0 || resistChance <= WorldServiceLocator._WorldServer.Rnd.Next(0, 100))
                    {
                        var delaytime = 200;
                        Delayed += delaytime;
                        Packets.PacketClass packet = new(Opcodes.SMSG_SPELL_DELAYED);
                        packet.AddPackGUID(Caster.GUID);
                        packet.AddInt32(delaytime);
                        Caster.SendToNearPlayers(ref packet);
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

    public const int SLOT_NOT_FOUND = -1;

    public const int SLOT_CREATE_NEW = -2;

    public const int SLOT_NO_SPACE = int.MaxValue;

    public const int AURAs_COUNT = 261;

    public ApplyAuraHandler[] AURAs;

    public const int DUEL_COUNTDOWN = 3000;

    public const float DUEL_OUTOFBOUNDS_DISTANCE = 40f;

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

    /// <summary>
    /// Packet SendCastResult
    /// </summary>
    /// <param name="result"></param>
    /// <param name="client"></param>
    /// <param name="id"></param>
    public void SendCastResult(SpellFailedReason result, ref WS_Network.ClientClass client, int id)
    {
        Packets.PacketClass packet = new(Opcodes.SMSG_CAST_RESULT);
        packet.AddInt32(id);
        if (result != SpellFailedReason.SPELL_NO_ERROR)
        {
            packet.AddInt8(2);
            var result1 = (byte)result;
            packet.AddInt8(result1);
        }
        else
        {
            packet.AddInt8(0);
        }
        client.Send(ref packet);
        packet.Dispose();
    }

    /// <summary>
    /// Packet SendNonMeleeDamageLog
    /// </summary>
    /// <param name="Caster"></param>
    /// <param name="Target"></param>
    /// <param name="SpellID"></param>
    /// <param name="SchoolType"></param>
    /// <param name="Damage"></param>
    /// <param name="Resist"></param>
    /// <param name="Absorbed"></param>
    /// <param name="CriticalHit"></param>
    public void SendNonMeleeDamageLog(ref WS_Base.BaseUnit Caster, ref WS_Base.BaseUnit Target, int SpellID, int SchoolType, int Damage, int Resist, int Absorbed, bool CriticalHit)
    {
        Packets.PacketClass packet = new(Opcodes.SMSG_SPELLNONMELEEDAMAGELOG);
        packet.AddPackGUID(Target.GUID);
        packet.AddPackGUID(Caster.GUID);
        packet.AddInt32(SpellID);
        packet.AddInt32(Damage);
        var schoolType = (byte)SchoolType;
        packet.AddInt8(checked(schoolType));
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
        Caster.SendToNearPlayers(ref packet);
    }

    /// <summary>
    /// Packet SendHealSpellLog
    /// </summary>
    /// <param name="Caster"></param>
    /// <param name="Target"></param>
    /// <param name="SpellID"></param>
    /// <param name="Damage"></param>
    /// <param name="CriticalHit"></param>
    public void SendHealSpellLog(ref WS_Base.BaseUnit Caster, ref WS_Base.BaseUnit Target, int SpellID, int Damage, bool CriticalHit)
    {
        Packets.PacketClass packet = new(Opcodes.SMSG_HEALSPELL_ON_PLAYER_OBSOLETE);
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

    /// <summary>
    /// Packet SendEnergizeSpellLog
    /// </summary>
    /// <param name="Caster"></param>
    /// <param name="Target"></param>
    /// <param name="SpellID"></param>
    /// <param name="Damage"></param>
    /// <param name="PowerType"></param>
    public void SendEnergizeSpellLog(ref WS_Base.BaseUnit Caster, ref WS_Base.BaseUnit Target, int SpellID, int Damage, int PowerType)
    {
    }

    /// <summary>
    /// Packet SendPeriodicAuraLog
    /// </summary>
    /// <param name="Caster"></param>
    /// <param name="Target"></param>
    /// <param name="SpellID"></param>
    /// <param name="School"></param>
    /// <param name="Damage"></param>
    /// <param name="AuraIndex"></param>
    public void SendPeriodicAuraLog(ref WS_Base.BaseUnit Caster, ref WS_Base.BaseUnit Target, int SpellID, int School, int Damage, int AuraIndex)
    {
        Packets.PacketClass packet = new(Opcodes.SMSG_PERIODICAURALOG);
        packet.AddPackGUID(Target.GUID);
        packet.AddPackGUID(Caster.GUID);
        packet.AddInt32(SpellID);
        packet.AddInt32(1);
        var auraIndex = (byte)AuraIndex;
        packet.AddInt8(checked(auraIndex));
        packet.AddInt32(Damage);
        packet.AddInt32(School);
        packet.AddInt32(0);
        Caster.SendToNearPlayers(ref packet);
        packet.Dispose();
    }

    /// <summary>
    /// Packet SendPlaySpellVisual
    /// </summary>
    /// <param name="Caster"></param>
    /// <param name="SpellId"></param>
    public void SendPlaySpellVisual(ref WS_Base.BaseUnit Caster, int SpellId)
    {
        Packets.PacketClass packet = new(Opcodes.SMSG_PLAY_SPELL_VISUAL);
        packet.AddUInt64(Caster.GUID);
        packet.AddInt32(SpellId);
        Caster.SendToNearPlayers(ref packet);
        packet.Dispose();
    }

    /// <summary>
    /// Packet SendChannelUpdate
    /// </summary>
    /// <param name="Caster"></param>
    /// <param name="Time"></param>
    public void SendChannelUpdate(ref WS_PlayerData.CharacterObject Caster, int Time)
    {
        Packets.PacketClass packet = new(Opcodes.MSG_CHANNEL_UPDATE);
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
        checked
        {
            var j = 0;
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
            SPELL_EFFECTs[121] = SPELL_EFFECT_ADDICTIONAL_DMG;
            SPELL_EFFECTs[137] = SPELL_EFFECT_ENERGIZE_PCT;
            var i = 0;
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

    /// <summary>
    /// SPELL_EFFECT_NOTHING
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_NOTHING(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        return SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// SPELL_EFFECT_BIND
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_BIND(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        for (var i = 0; i < Infected.Count; i++)
        {
            WS_Base.BaseUnit Unit = (WS_Base.BaseUnit)Infected[i];
            if (Unit is WS_PlayerData.CharacterObject @object)
            {
                @object.BindPlayer(Caster.GUID);
            }
        }
        return SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// SPELL_EFFECT_DUMMY
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_DUMMY(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        return SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// SPELL_EFFECT_INSTAKILL
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_INSTAKILL(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        for (var i = 0; i < Infected.Count; i++)
        {
            WS_Base.BaseUnit Unit = (WS_Base.BaseUnit)Infected[i];
            WS_Base.BaseUnit Attacker = (WS_Base.BaseUnit)Caster;
            Unit.Die(ref Attacker);
            Caster = Attacker;
        }
        return SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// SPELL_EFFECT_SCHOOL_DAMAGE
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_SCHOOL_DAMAGE(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        foreach (WS_Base.BaseUnit Unit in Infected)
        {
            WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
            var Damage = (Caster is not WS_DynamicObjects.DynamicObject @object) ? SpellInfo.GetValue(Level: caster.Level, ComboPoints: 0) : SpellInfo.GetValue(@object.Caster.Level, 0);
            var Current = 0;
            if (Current > 0)
            {
                Damage = checked((int)Math.Round(Damage * Math.Pow(SpellInfo.DamageMultiplier, Current)));
            }
            WS_Base.BaseUnit realCaster = null;
            switch (Caster)
            {
                case WS_Base.BaseUnit:
                    realCaster = caster;
                    break;

                case WS_DynamicObjects.DynamicObject:
                    WS_DynamicObjects.DynamicObject caster1 = (WS_DynamicObjects.DynamicObject)Caster;
                    realCaster = caster1.Caster;
                    break;
                default:
                    break;
            }
            if (realCaster != null)
            {
                var school = (byte)SPELLs[SpellID].School;
                Unit.DealSpellDamage(ref realCaster, ref SpellInfo, SpellID, Damage, (DamageTypes)checked(school), SpellType.SPELL_TYPE_NONMELEE);
            }
            Current = checked(Current + 1);
        }
        return SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// SPELL_EFFECT_ENVIONMENTAL_DAMAGE
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_ENVIRONMENTAL_DAMAGE(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        for (var i = 0; i < Infected.Count; i++)
        {
            WS_Base.BaseUnit Unit = (WS_Base.BaseUnit)Infected[i];
            WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
            var Attacker = caster;
            var Damage = SpellInfo.GetValue(Level: caster.Level, ComboPoints: 0);
            Unit.DealDamage(Damage, Attacker);
            Caster = Attacker;
            if (Unit is WS_PlayerData.CharacterObject @object)
            {
                var school = (byte)SPELLs[SpellID].School;
                @object.LogEnvironmentalDamage((DamageTypes)checked(school), Damage);
            }
        }
        return SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// SPELL_EFFECT_TRIGGER_SPELL
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_TRIGGER_SPELL(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        if (!SPELLs.ContainsKey(SpellInfo.TriggerSpell) || Target.unitTarget == null)
        {
            return SpellFailedReason.SPELL_FAILED_ERROR;
        }

        switch (SpellInfo.TriggerSpell)
        {
            case 18461:
                {
                    var unitTarget = Target.unitTarget;
                    var NotSpellID = 0;
                    unitTarget.RemoveAurasOfType(AuraEffects_Names.SPELL_AURA_MOD_ROOT, NotSpellID);
                    var unitTarget2 = Target.unitTarget;
                    NotSpellID = 0;
                    unitTarget2.RemoveAurasOfType(AuraEffects_Names.SPELL_AURA_MOD_DECREASE_SPEED, NotSpellID);
                    var unitTarget3 = Target.unitTarget;
                    NotSpellID = 0;
                    unitTarget3.RemoveAurasOfType(AuraEffects_Names.SPELL_AURA_MOD_STALKED, NotSpellID);
                    break;
                }
            case 35729:
                {
                    byte i;
                    byte b2;
                    checked
                    {
                        var b = (byte)WorldServiceLocator._Global_Constants.MAX_POSITIVE_AURA_EFFECTs;
                        b2 = (byte)(WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1);
                        i = b;
                    }

                    uint b3 = b2;
                    while (i <= b3)
                    {
                        var attributes = (uint)SPELLs[Target.unitTarget.ActiveSpells[i].SpellID].Attributes;
                        if (Target.unitTarget.ActiveSpells[i] != null && (SPELLs[Target.unitTarget.ActiveSpells[i].SpellID].School & 1) == 0 && (attributes & 0x10000u) != 0)
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

            default:
                break;
        }
        if ((SPELLs[SpellInfo.TriggerSpell].EquippedItemClass >= 0) & (Caster is WS_PlayerData.CharacterObject))
        {
            return SpellFailedReason.SPELL_NO_ERROR;
        }
        ThreadPool.QueueUserWorkItem(new CastSpellParameters(ref Target, ref Caster, SpellInfo.TriggerSpell).Cast);
        return SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// SPELL_EFFECT_TELEPORT_UNITS
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_TELEPORT_UNITS(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        for (var i = 0; i < Infected.Count; i++)
        {
            WS_Base.BaseUnit Unit = (WS_Base.BaseUnit)Infected[i];
            if (Unit is WS_PlayerData.CharacterObject @object)
            {
                using var characterObject = @object;
                if (SpellID == 8690)
                {
                    characterObject.Teleport(characterObject.bindpoint_positionX, characterObject.bindpoint_positionY, characterObject.bindpoint_positionZ, characterObject.orientation, characterObject.bindpoint_map_id);
                }
                else if (WorldServiceLocator._WS_DBCDatabase.TeleportCoords.ContainsKey(SpellID))
                {
                    var mapID = (int)WorldServiceLocator._WS_DBCDatabase.TeleportCoords[SpellID].MapID;
                    characterObject.Teleport(WorldServiceLocator._WS_DBCDatabase.TeleportCoords[SpellID].PosX, WorldServiceLocator._WS_DBCDatabase.TeleportCoords[SpellID].PosY, WorldServiceLocator._WS_DBCDatabase.TeleportCoords[SpellID].PosZ, characterObject.orientation, checked(mapID));
                }
                else
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "WARNING: Spell {0} did not have any teleport coordinates.", SpellID);
                    return SpellFailedReason.SPELL_FAILED_UNKNOWN;
                }
            }
        }
        return SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// SPELL_EFECT_MANA_DRAIN
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_MANA_DRAIN(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        checked
        {
            WS_Base.BaseUnit caster1 = (WS_Base.BaseUnit)Caster;
            for (var i = 0; i < Infected.Count; i++)
            {
                WS_Base.BaseUnit Unit = (WS_Base.BaseUnit)Infected[i];
                var caster = caster1;
                var Damage = SpellInfo.GetValue(Level: caster.Level, ComboPoints: 0);
                if (Caster is WS_PlayerData.CharacterObject @object)
                {
                    Damage += SpellInfo.valuePerLevel * @object.Level;
                }
                var TargetPower = 0;
                switch (SpellInfo.MiscValue)
                {
                    case 0:
                        if (Damage > Unit.Mana.Current)
                        {
                            Damage = Unit.Mana.Current;
                        }
                        Unit.Mana.Current -= Damage;
                        caster.Mana.Current += Damage;
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
                        caster.Mana.Current += Damage;
                        TargetPower = Unit.Mana.Current;
                        break;
                }
                switch (Unit)
                {
                    case WS_Creatures.CreatureObject:
                        {
                            Packets.UpdateClass myTmpUpdate = new(188);
                            myTmpUpdate.SetUpdateFlag(23 + SpellInfo.MiscValue, TargetPower);
                            Packets.UpdatePacketClass myPacket = new();
                            Packets.PacketClass packet = myPacket;
                            WS_Creatures.CreatureObject updateObject = (WS_Creatures.CreatureObject)Unit;
                            myTmpUpdate.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
                            myPacket = (Packets.UpdatePacketClass)packet;
                            packet = myPacket;
                            Unit.SendToNearPlayers(ref packet);
                            myPacket = (Packets.UpdatePacketClass)packet;
                            myPacket.Dispose();
                            myTmpUpdate.Dispose();
                            break;
                        }
                    case WS_PlayerData.CharacterObject object1:
                        object1.SetUpdateFlag(23 + SpellInfo.MiscValue, TargetPower);
                        object1.SendCharacterUpdate();
                        break;
                    default:
                        break;
                }
            }
            var CasterPower = 0;
            switch (SpellInfo.MiscValue)
            {
                case 0:
                    CasterPower = caster1.Mana.Current;
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
                    CasterPower = caster1.Mana.Current;
                    break;
            }
            switch (Caster)
            {
                case WS_Creatures.CreatureObject:
                    {
                        Packets.UpdateClass TmpUpdate = new(188);
                        Packets.UpdatePacketClass Packet = new();
                        TmpUpdate.SetUpdateFlag(23 + SpellInfo.MiscValue, CasterPower);
                        Packets.PacketClass packet = Packet;
                        WS_Creatures.CreatureObject updateObject = (WS_Creatures.CreatureObject)Caster;
                        TmpUpdate.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
                        Packet = (Packets.UpdatePacketClass)packet;
                        var unitTarget = Target.unitTarget;
                        packet = Packet;
                        unitTarget.SendToNearPlayers(ref packet);
                        Packet = (Packets.UpdatePacketClass)packet;
                        Packet.Dispose();
                        TmpUpdate.Dispose();
                        break;
                    }

                case WS_PlayerData.CharacterObject:
                    WS_PlayerData.CharacterObject caster2 = (WS_PlayerData.CharacterObject)Caster;
                    caster2.SetUpdateFlag(23 + SpellInfo.MiscValue, CasterPower);
                    caster2.SendCharacterUpdate();
                    break;
                default:
                    break;
            }
            return SpellFailedReason.SPELL_NO_ERROR;
        }
    }

    /// <summary>
    /// SPELL_EFFECT_HEAL
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_HEAL(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        if (Caster is WS_PlayerData.CharacterObject)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "[MapID:{0} X:{1} Y:{2} Z:{3}] SPELL_EFFECT_HEAL [Caster = CharacterObject: {4}]", Caster.MapID, Caster.positionX, Caster.positionY, Caster.positionZ, Caster is WS_PlayerData.CharacterObject);
        }
        else
        if (Caster is not WS_PlayerData.CharacterObject)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "[MapID:{0} X:{1} Y:{2} Z:{3}] SPELL_EFFECT_HEAL [Caster = CharacterObject: {4}]", Caster.MapID, Caster.positionX, Caster.positionY, Caster.positionZ, Caster is not WS_PlayerData.CharacterObject);
            return SpellFailedReason.SPELL_FAILED_ERROR;
        }
        for (var i = 0; i < Infected.Count; i++)
        {
            WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
            var Damage = SpellInfo.GetValue(Level: caster.Level, ComboPoints: 0);
            var Current = 0;
            if (Current > 0)
            {
                Damage = checked((int)Math.Round(Damage * Math.Pow(SpellInfo.DamageMultiplier, Current)));
            }
            var Caster2 = caster;
            WS_Base.BaseUnit Unit = (WS_Base.BaseUnit)Infected[i];
            var school = (byte)SPELLs[SpellID].School;
            Unit.DealSpellDamage(ref Caster2, ref SpellInfo, SpellID, Damage, (DamageTypes)checked(school), SpellType.SPELL_TYPE_HEAL);
            Caster = Caster2;
            Current = checked(Current + 1);
        }
        return SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// SPELL_EFFECT_HEAL_MAX_HEALTH
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_HEAL_MAX_HEALTH(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        for (var i = 0; i < Infected.Count; i++)
        {
            WS_Base.BaseUnit Unit = (WS_Base.BaseUnit)Infected[i];
            WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
            var Damage = caster.Life.Maximum;
            var Current = 0;
            if (Current > 0 && SpellInfo.DamageMultiplier < 1f)
            {
                Damage = checked((int)Math.Round(Damage * Math.Pow(SpellInfo.DamageMultiplier, Current)));
            }
            var Caster2 = caster;
            var school = (byte)SPELLs[SpellID].School;
            Unit.DealSpellDamage(ref Caster2, ref SpellInfo, SpellID, Damage, (DamageTypes)checked(school), SpellType.SPELL_TYPE_HEAL);
            Caster = Caster2;
            Current = checked(Current + 1);
        }
        return SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// SPELL_EFFECT_ENERGIZE
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_ENERGIZE(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        for (var i = 0; i < Infected.Count; i++)
        {
            WS_Base.BaseUnit Unit = (WS_Base.BaseUnit)Infected[i];
            WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
            var Caster2 = caster;
            var Damage = SpellInfo.GetValue(Level: caster.Level, ComboPoints: 0);
            SendEnergizeSpellLog(ref Caster2, ref Target.unitTarget, SpellID, Damage, SpellInfo.MiscValue);
            Caster = Caster2;
            Caster2 = caster;
            var miscValue = SpellInfo.MiscValue;
            Unit.Energize(Damage: Damage, Power: (ManaTypes)miscValue, Attacker: Caster2);
            Caster = Caster2;
        }
        return SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// SPELL_EFFECT_ENERGIZE_PCT
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_ENERGIZE_PCT(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        for (var i = 0; i < Infected.Count; i++)
        {
            WS_Base.BaseUnit Unit = (WS_Base.BaseUnit)Infected[i];
            WS_Base.BaseUnit Caster2;
            int miscValue;
            int damage;
            checked
            {
                var Damage = 0;
                if (SpellInfo.MiscValue == 0)
                {
                    WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                    Damage = (int)Math.Round(SpellInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0 * Unit.Mana.Maximum);
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

    /// <summary>
    /// SPELL_EFECT_OPEN_LOCK
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_OPEN_LOCK(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        if (Caster is not WS_PlayerData.CharacterObject)
        {
            return SpellFailedReason.SPELL_FAILED_ERROR;
        }
        ulong targetGUID;
        int lockID;
        if (Target.goTarget != null)
        {
            targetGUID = Target.goTarget.GUID;
            WS_GameObjects.GameObject goTarget = (WS_GameObjects.GameObject)Target.goTarget;
            lockID = goTarget.LockID;
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
        LootType LootType = LootType.LOOTTYPE_CORPSE;
        if (lockID == 0)
        {
            if (WorldServiceLocator._CommonGlobalFunctions.GuidIsGameObject(targetGUID) && WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(targetGUID))
            {
                WS_PlayerData.CharacterObject Character = (WS_PlayerData.CharacterObject)Caster;
                WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[targetGUID].LootObject(ref Character, LootType);
            }
            return SpellFailedReason.SPELL_NO_ERROR;
        }
        if (!WorldServiceLocator._WS_Loot.Locks.ContainsKey(lockID))
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "[DEBUG] Lock {0} did not exist.", lockID);
            return SpellFailedReason.SPELL_FAILED_ERROR;
        }
        byte i = 0;
        do
        {
            if (Item != null && WorldServiceLocator._WS_Loot.Locks[lockID].KeyType[i] == 1 && WorldServiceLocator._WS_Loot.Locks[lockID].Keys[i] == Item.ItemEntry)
            {
                if (WorldServiceLocator._CommonGlobalFunctions.GuidIsGameObject(targetGUID) && WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(targetGUID))
                {
                    WS_PlayerData.CharacterObject Character = (WS_PlayerData.CharacterObject)Caster;
                    WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[targetGUID].LootObject(ref Character, LootType);
                }
                return SpellFailedReason.SPELL_NO_ERROR;
            }
            checked
            {
                i = (byte)unchecked((uint)(i + 1));
            }
        }
        while (i <= 4u);
        var SkillID = 0;
        if (SPELLs[SpellID].SpellEffects[1] != null && SPELLs[SpellID].SpellEffects[1].ID == SpellEffects_Names.SPELL_EFFECT_SKILL)
        {
            SkillID = SPELLs[SpellID].SpellEffects[1].MiscValue;
        }
        else if (SPELLs[SpellID].SpellEffects[0] != null && SPELLs[SpellID].SpellEffects[0].MiscValue == 1)
        {
            SkillID = 633;
        }
        var ReqSkillValue = WorldServiceLocator._WS_Loot.Locks[lockID].RequiredMiningSkill;
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
            WS_PlayerData.CharacterObject caster = (WS_PlayerData.CharacterObject)Caster;
            if (!caster.Skills.ContainsKey(SkillID) || caster.Skills[SkillID].Current < ReqSkillValue)
            {
                return SpellFailedReason.SPELL_FAILED_LOW_CASTLEVEL;
            }
        }
        if (WorldServiceLocator._CommonGlobalFunctions.GuidIsGameObject(targetGUID) && WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(targetGUID))
        {
            WS_PlayerData.CharacterObject Character = (WS_PlayerData.CharacterObject)Caster;
            WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[targetGUID].LootObject(ref Character, LootType);
        }
        return SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// SPELL_EFFECT_PICKPOCKET
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_FAILED_BAD_TARGETS</returns>
    public SpellFailedReason SPELL_EFFECT_PICKPOCKET(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        if (Caster is not WS_PlayerData.CharacterObject)
        {
            return SpellFailedReason.SPELL_FAILED_ERROR;
        }
        checked
        {
            WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
            if (Target.unitTarget is WS_Creatures.CreatureObject @object && !caster.IsFriendlyTo(ref Target.unitTarget))
            {
                var creatureObject = @object;
                if (creatureObject.CreatureInfo.CreatureType is 7 or 6)
                {
                    if (!creatureObject.IsDead)
                    {
                        var chance = 10 + caster.Level - creatureObject.Level;
                        if (chance > WorldServiceLocator._WorldServer.Rnd.Next(0, 20))
                        {
                            if (creatureObject.CreatureInfo.PocketLootID > 0)
                            {
                                WS_Loot.LootObject Loot = new(creatureObject.GUID, LootType.LOOTTYPE_PICKPOCKETING)
                                {
                                    LootOwner = Caster.GUID
                                };
                                WorldServiceLocator._WS_Loot.LootTemplates_Pickpocketing.GetLoot(creatureObject.CreatureInfo.PocketLootID)?.Process(ref Loot, 0);
                                WS_PlayerData.CharacterObject caster1 = (WS_PlayerData.CharacterObject)Caster;
                                new WS_Loot.LootObject(creatureObject.GUID, LootType.LOOTTYPE_PICKPOCKETING)
                                {
                                    LootOwner = Caster.GUID
                                }.SendLoot(ref caster1.client);
                            }
                            else
                            {
                                WS_PlayerData.CharacterObject caster1 = (WS_PlayerData.CharacterObject)Caster;
                                WorldServiceLocator._WS_Loot.SendEmptyLoot(creatureObject.GUID, LootType.LOOTTYPE_PICKPOCKETING, ref caster1.client);
                            }
                        }
                        else
                        {
                            caster.RemoveAurasByInterruptFlag(1024);
                            if (creatureObject.aiScript != null)
                            {
                                var Attacker = caster;
                                creatureObject.aiScript.OnGenerateHate(ref Attacker, 100);
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

    /// <summary>
    /// SPELL_EFFECT_SKINNING
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_FAILED_BAD_TARGETS</returns>
    public SpellFailedReason SPELL_EFFECT_SKINNING(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        if (Caster is not WS_PlayerData.CharacterObject)
        {
            return SpellFailedReason.SPELL_FAILED_ERROR;
        }
        switch (Target.unitTarget)
        {
            case WS_Creatures.CreatureObject:
                {
                    using (WS_Creatures.CreatureObject creatureObject = (WS_Creatures.CreatureObject)Target.unitTarget)
                    {
                        if (creatureObject.IsDead && WorldServiceLocator._Functions.HaveFlags(creatureObject.cUnitFlags, 67108864))
                        {
                            creatureObject.cUnitFlags &= -67108865;
                            if (creatureObject.CreatureInfo.SkinLootID > 0)
                            {
                                WS_Loot.LootObject Loot = new(creatureObject.GUID, LootType.LOOTTYPE_SKINNING)
                                {
                                    LootOwner = Caster.GUID
                                };
                                WorldServiceLocator._WS_Loot.LootTemplates_Skinning.GetLoot(creatureObject.CreatureInfo.SkinLootID)?.Process(ref Loot, 0);
                                WS_PlayerData.CharacterObject caster = (WS_PlayerData.CharacterObject)Caster;
                                new WS_Loot.LootObject(creatureObject.GUID, LootType.LOOTTYPE_SKINNING)
                                {
                                    LootOwner = Caster.GUID
                                }.SendLoot(ref caster.client);
                            }
                            else
                            {
                                WS_PlayerData.CharacterObject caster = (WS_PlayerData.CharacterObject)Caster;
                                WorldServiceLocator._WS_Loot.SendEmptyLoot(creatureObject.GUID, LootType.LOOTTYPE_SKINNING, ref caster.client);
                            }
                            Packets.UpdateClass TmpUpdate = new(188);
                            TmpUpdate.SetUpdateFlag(46, creatureObject.cUnitFlags);
                            Packets.UpdatePacketClass Packet = new();
                            Packets.PacketClass packet = Packet;
                            WS_Creatures.CreatureObject updateObject = (WS_Creatures.CreatureObject)Target.unitTarget;
                            TmpUpdate.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
                            Packet = (Packets.UpdatePacketClass)packet;
                            packet = Packet;
                            var unitTarget = Target.unitTarget;
                            unitTarget.SendToNearPlayers(ref packet);
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

    /// <summary>
    /// SPELL_EFFECT_DISENCHANT
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>Not Implemented</returns>
    public SpellFailedReason SPELL_EFFECT_DISENCHANT(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        return Caster is not WS_PlayerData.CharacterObject ? SpellFailedReason.SPELL_FAILED_ERROR : SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// SPELL_EFFECT_PROFICIENCY
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_PROFICIENCY(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        for (var i = 0; i < Infected.Count; i++)
        {
            WS_Base.BaseUnit Unit = (WS_Base.BaseUnit)Infected[i];
            if (Unit is WS_PlayerData.CharacterObject @object)
            {
                @object.SendProficiencies();
            }
        }
        return SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// SPELL_EFECT_LEARN_SPELL
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_LEARN_SPELL(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        if (SpellInfo.TriggerSpell != 0)
        {
            for (var i = 0; i < Infected.Count; i++)
            {
                WS_Base.BaseUnit Unit = (WS_Base.BaseUnit)Infected[i];
                if (Unit is WS_PlayerData.CharacterObject @object)
                {
                    @object.LearnSpell(SpellInfo.TriggerSpell);
                }
            }
        }
        return SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// SPELL_EFFECT_SKILL_STEP
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_SKILL_STEP(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        if (SpellInfo.MiscValue != 0)
        {
            for (var i = 0; i < Infected.Count; i++)
            {
                WS_Base.BaseUnit Unit = (WS_Base.BaseUnit)Infected[i];
                if (Unit is WS_PlayerData.CharacterObject @object)
                {
                    @object.LearnSkill(SpellInfo.MiscValue, 1, checked((short)((SpellInfo.valueBase + 1) * 75)));
                    @object.SendCharacterUpdate(toNear: false);
                }
            }
        }
        return SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// SPELL_EFFECT_DISPEL
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_DISPEL(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        for (var i = 0; i < Infected.Count; i++)
        {
            WS_Base.BaseUnit Unit = (WS_Base.BaseUnit)Infected[i];
            if ((Unit.DispellImmunity & (1 << SpellInfo.MiscValue)) == 0)
            {
                WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                Unit.RemoveAurasByDispellType(SpellInfo.MiscValue, SpellInfo.GetValue(Level: caster.Level, ComboPoints: 0));
            }
        }
        return SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// SPELL_EFFECT_EVADE
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_FAILED_ERROR (combatEvade not implemented!)</returns>
    public SpellFailedReason SPELL_EFFECT_EVADE(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        /*for (int i = 0; i < Infected.Count; i++)
        {
            WS_Base.BaseUnit Unit = (WS_Base.BaseUnit)Infected[i];
            if (Unit is WS_PlayerData.CharacterObject @object)
            {
                @object.combatEvade += SpellInfo.GetValue(((WS_Base.BaseUnit)Caster).Level, 0);
            }
        }*/
        return SpellFailedReason.SPELL_FAILED_ERROR;
    }

    /// <summary>
    /// SPELL_EFFECT_DODGE
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_DODGE(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        checked
        {
            for (var i = 0; i < Infected.Count; i++)
            {
                WS_Base.BaseUnit Unit = (WS_Base.BaseUnit)Infected[i];
                if (Unit is WS_PlayerData.CharacterObject @object)
                {
                    WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                    @object.combatDodge += SpellInfo.GetValue(Level: caster.Level, ComboPoints: 0);
                }
            }
            return SpellFailedReason.SPELL_NO_ERROR;
        }
    }

    /// <summary>
    /// SPELL_EFFECT_PARRY
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_PARRY(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        checked
        {
            for (var i = 0; i < Infected.Count; i++)
            {
                WS_Base.BaseUnit Unit = (WS_Base.BaseUnit)Infected[i];
                if (Unit is WS_PlayerData.CharacterObject @object)
                {
                    WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                    @object.combatParry += SpellInfo.GetValue(Level: caster.Level, ComboPoints: 0);
                }
            }
            return SpellFailedReason.SPELL_NO_ERROR;
        }
    }

    /// <summary>
    /// SPELL_EFFECT_BLOCK
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_BLOCK(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        checked
        {
            for (var i = 0; i < Infected.Count; i++)
            {
                WS_Base.BaseUnit Unit = (WS_Base.BaseUnit)Infected[i];
                if (Unit is WS_PlayerData.CharacterObject @object)
                {
                    WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                    @object.combatBlock += SpellInfo.GetValue(Level: caster.Level, ComboPoints: 0);
                }
            }
            return SpellFailedReason.SPELL_NO_ERROR;
        }
    }

    /// <summary>
    /// SPELL_EFFECT_DUEL_WIELD
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_DUAL_WIELD(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        for (var i = 0; i < Infected.Count; i++)
        {
            WS_Base.BaseUnit Unit = (WS_Base.BaseUnit)Infected[i];
            if (Unit is WS_PlayerData.CharacterObject @object)
            {
                @object.spellCanDualWeild = true;
            }
        }
        return SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// SPELL_EFECT_WEAPON_DAMAGE
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    /// <param name="SpellID"></param>
    public SpellFailedReason SPELL_EFFECT_WEAPON_DAMAGE(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        var Ranged = false;
        var Offhand = false;
        if (SPELLs[SpellID].IsRanged)
        {
            Ranged = true;
        }
        foreach (WS_Base.BaseUnit item in Infected)
        {
            var Unit = item;
            var wS_Combat = WorldServiceLocator._WS_Combat;
            WS_Base.BaseUnit Attacker = (WS_Base.BaseUnit)Caster;
            Caster = Attacker;
            var damageInfo2 = wS_Combat.CalculateDamage(ref Attacker, ref Unit, Offhand, Ranged, SPELLs[SpellID], SpellInfo);
            var damageInfo = damageInfo2;
            var hitInfo = (uint)damageInfo.HitInfo;
            if ((hitInfo & 0x40u) != 0)
            {
                SPELLs[SpellID].SendSpellMiss(ref Caster, ref Unit, SpellMissInfo.SPELL_MISS_RESIST);
                continue;
            }
            if ((hitInfo & 0x10u) != 0)
            {
                SPELLs[SpellID].SendSpellMiss(ref Caster, ref Unit, SpellMissInfo.SPELL_MISS_MISS);
                continue;
            }
            if ((hitInfo & 0x20u) != 0)
            {
                SPELLs[SpellID].SendSpellMiss(ref Caster, ref Unit, SpellMissInfo.SPELL_MISS_ABSORB);
                continue;
            }
            if ((hitInfo & 0x800u) != 0)
            {
                SPELLs[SpellID].SendSpellMiss(ref Caster, ref Unit, SpellMissInfo.SPELL_MISS_BLOCK);
                continue;
            }
            Attacker = (WS_Base.BaseUnit)Caster;
            var damageType = (int)damageInfo.DamageType;
            SendNonMeleeDamageLog(ref Attacker, ref Unit, SpellID, damageType, damageInfo.GetDamage, damageInfo.Resist, damageInfo.Absorbed, (damageInfo.HitInfo & 0x200) != 0);
            Caster = Attacker;
            var getDamage = damageInfo.GetDamage;
            Attacker = (WS_Base.BaseUnit)Caster;
            var baseUnit = Unit;
            baseUnit.DealDamage(getDamage, Attacker);
            Caster = Attacker;
        }
        return SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// SPELL_EFFECT_WEAPON_DAMAGE_NOSCHOOL
    /// </summary>
    /// <param name="target"></param>
    /// <param name="caster"></param>
    /// <param name="spellInfo"></param>
    /// <param name="spellId"></param>
    /// <param name="infected"></param>
    /// <param name="item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
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

    /// <summary>
    /// SPELL_EFFECT_ADDICTIONAL_DMG
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_ADDICTIONAL_DMG(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        var Ranged = false;
        var Offhand = false;
        if (SPELLs[SpellID].IsRanged)
        {
            Ranged = true;
        }
        foreach (WS_Base.BaseUnit item in Infected)
        {
            var Unit = item;
            var wS_Combat = WorldServiceLocator._WS_Combat;
            WS_Base.BaseUnit Attacker = (WS_Base.BaseUnit)Caster;
            Caster = Attacker;
            var damageInfo2 = wS_Combat.CalculateDamage(ref Attacker, ref Unit, Offhand, Ranged, SPELLs[SpellID], SpellInfo);
            var damageInfo = damageInfo2;
            Attacker = (WS_Base.BaseUnit)Caster;
            var damageType = (int)damageInfo.DamageType;
            SendNonMeleeDamageLog(ref Attacker, ref Unit, SpellID, damageType, damageInfo.GetDamage, damageInfo.Resist, damageInfo.Absorbed, (damageInfo.HitInfo & 0x200) != 0);
            Caster = Attacker;
            var getDamage = damageInfo.GetDamage;
            Attacker = (WS_Base.BaseUnit)Caster;
            var baseUnit = Unit;
            baseUnit.DealDamage(getDamage, Attacker);
            Caster = Attacker;
        }
        return SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// SPELL_EFFECT_HONOR
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_HONOR(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        checked
        {
            for (var i = 0; i < Infected.Count; i++)
            {
                WS_Base.BaseUnit Unit = (WS_Base.BaseUnit)Infected[i];
                if (Unit is WS_PlayerData.CharacterObject @object)
                {
                    WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                    @object.HonorPoints += SpellInfo.GetValue(Level: caster.Level, ComboPoints: 0);
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

    /// <summary>
    /// ApplyAura
    /// </summary>
    /// <param name="auraTarget"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <returns></returns>
    public SpellFailedReason ApplyAura(ref WS_Base.BaseUnit auraTarget, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID)
    {
        checked
        {
            try
            {
                var spellCasted = -1;
                do
                {
                    var AuraStart = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1;
                    var AuraEnd = 0;
                    if (SPELLs[SpellID].IsPassive)
                    {
                        AuraStart = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs - 1;
                        AuraEnd = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE;
                    }
                    var Duration = SPELLs[SpellID].GetDuration;
                    if (SpellID == 15007)
                    {
                        Duration = auraTarget.Level switch
                        {
                            0 or 1 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 10 => 0,
                            11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 => (auraTarget.Level - 10) * 60 * 1000,
                            _ => 600000,
                        };
                    }
                    var num = AuraStart;
                    var num2 = AuraEnd;
                    for (var i = num; i >= num2; i += -1)
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
                                    WS_Pets.PetObject auraTarget2 = (WS_Pets.PetObject)auraTarget;
                                    switch (auraTarget)
                                    {
                                        case WS_PlayerData.CharacterObject:
                                            WS_PlayerData.CharacterObject auraTarget1 = (WS_PlayerData.CharacterObject)auraTarget;
                                            auraTarget1.GroupUpdateFlag |= 0x200u;
                                            break;

                                        case WS_Pets.PetObject when auraTarget2.Owner is WS_PlayerData.CharacterObject @object:
                                            @object.GroupUpdateFlag |= 0x40000u;
                                            break;
                                        default:
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
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "APPLYING AURA[0] {0}", (AuraEffects_Names)SpellInfo.ApplyAuraIndex);
                                break;
                            }
                            if (auraTarget.ActiveSpells[i].Aura[1] == null)
                            {
                                auraTarget.ActiveSpells[i].Aura[1] = AURAs[SpellInfo.ApplyAuraIndex];
                                auraTarget.ActiveSpells[i].Aura_Info[1] = SpellInfo;
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "APPLYING AURA[1] {0}", (AuraEffects_Names)SpellInfo.ApplyAuraIndex);
                                break;
                            }
                            if (auraTarget.ActiveSpells[i].Aura[2] == null)
                            {
                                auraTarget.ActiveSpells[i].Aura[2] = AURAs[SpellInfo.ApplyAuraIndex];
                                auraTarget.ActiveSpells[i].Aura_Info[2] = SpellInfo;
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "APPLYING AURA[2] {0}", (AuraEffects_Names)SpellInfo.ApplyAuraIndex);
                                break;
                            }
                            spellCasted = int.MaxValue;
                        }
                    }
                    if (spellCasted == -1)
                    {
                        var obj = auraTarget;
                        var duration = Duration;
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
            catch (Exception e)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error while applying aura for spell [Caster:{0}, SpellID:{1}, SpellInfo:{2}, AuraTarget:{3}, Exception:{4}]", Caster, SpellID, SpellInfo, auraTarget, Environment.NewLine + e);
            }
            return SpellFailedReason.SPELL_NO_ERROR;
        }
    }

    /// <summary>
    /// SPELL_EFFECT_APPLY_AURA
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_APPLY_AURA(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        if ((((uint)Target.targetMask & 2u) != 0 || Target.targetMask == 0) && Target.unitTarget == null)
        {
            return SpellFailedReason.SPELL_FAILED_BAD_IMPLICIT_TARGETS;
        }
        var result = SpellFailedReason.SPELL_NO_ERROR;
        var auraInterruptFlags = (uint)SPELLs[SpellID].auraInterruptFlags;
        if (Caster is WS_PlayerData.CharacterObject @object && (auraInterruptFlags & 0x40000u) != 0)
        {
            WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
            caster.StandState = 1;
            @object.SetUpdateFlag(138, caster.cBytes1);
            @object.SendCharacterUpdate();
            Packets.PacketClass packetACK = new(Opcodes.SMSG_STANDSTATE_CHANGE_ACK);
            packetACK.AddInt8(caster.StandState);
            @object.client.Send(ref packetACK);
            packetACK.Dispose();
        }
        if (((uint)Target.targetMask & 2u) != 0 || Target.targetMask == 0)
        {
            var count = SPELLs[SpellID].MaxTargets;
            foreach (WS_Base.BaseUnit item in Infected)
            {
                var u = item;
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
            WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
            var array = caster.dynamicObjects.ToArray();
            foreach (var dynamic in array)
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

    /// <summary>
    /// SPELL_EFFECT_APPLY_AREA_AURA
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_APPLY_AREA_AURA(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        foreach (WS_Base.BaseUnit item in Infected)
        {
            var u = item;
            ApplyAura(ref u, ref Caster, ref SpellInfo, SpellID);
        }
        return SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// SPELL_EFFECT_PERSISTENT_AREA_AURA
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_PERSISTENT_AREA_AURA(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        if ((Target.targetMask & 0x40) == 0)
        {
            return SpellFailedReason.SPELL_FAILED_BAD_IMPLICIT_TARGETS;
        }
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Amplitude: {0}", SpellInfo.Amplitude);
        WS_Base.BaseUnit Caster_ = (WS_Base.BaseUnit)Caster;
        WS_DynamicObjects.DynamicObject dynamicObject = new(ref Caster_, SpellID, Target.dstX, Target.dstY, Target.dstZ, SPELLs[SpellID].GetDuration, SpellInfo.GetRadius);
        Caster = Caster_;
        var tmpDO = dynamicObject;
        tmpDO.AddEffect(SpellInfo);
        tmpDO.Bytes = 32435950;
        ((WS_Base.BaseUnit)Caster).dynamicObjects.Add(tmpDO);
        tmpDO.Spawn();
        return SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// SPELL_EFFECT_CREATE_ITEM
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_CREATE_ITEM(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        if (Target.unitTarget is not WS_PlayerData.CharacterObject)
        {
            return SpellFailedReason.SPELL_FAILED_BAD_TARGETS;
        }
        checked
        {
            WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
            var Amount = SpellInfo.GetValue(caster.Level - SPELLs[SpellID].spellLevel, 0);
            if (Amount < 0)
            {
                return SpellFailedReason.SPELL_FAILED_ERROR;
            }
            if (!WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(SpellInfo.ItemType))
            {
                WS_Items.ItemInfo tmpInfo = new(SpellInfo.ItemType);
                WorldServiceLocator._WorldServer.ITEMDatabase.Add(SpellInfo.ItemType, tmpInfo);
            }
            if (Amount > WorldServiceLocator._WorldServer.ITEMDatabase[SpellInfo.ItemType].Stackable)
            {
                Amount = WorldServiceLocator._WorldServer.ITEMDatabase[SpellInfo.ItemType].Stackable;
            }
            var objCharacter = caster;
            var friendPlayersAroundMe = GetFriendPlayersAroundMe(ref objCharacter, SpellInfo.GetRadius);
            Caster = objCharacter;
            foreach (WS_Base.BaseUnit Unit in Infected)
            {
                if (Unit is WS_PlayerData.CharacterObject @object)
                {
                    ItemObject tmpItem = new(SpellInfo.ItemType, Unit.GUID)
                    {
                        StackCount = Amount
                    };
                    if (!@object.ItemADD(ref tmpItem))
                    {
                        tmpItem.Delete();
                    }
                    else
                    {
                        WS_PlayerData.CharacterObject unitTarget = (WS_PlayerData.CharacterObject)Target.unitTarget;
                        var stackCount = (byte)tmpItem.StackCount;
                        unitTarget.LogLootItem(tmpItem, stackCount, Recieved: false, Created: true);
                    }
                }
            }
            return SpellFailedReason.SPELL_NO_ERROR;
        }
    }

    /// <summary>
    /// SPELL_EFFECT_RESURRECT
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_RESURRECT(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        foreach (var Unit in Infected)
        {
            WS_Corpses.CorpseObject unit = (WS_Corpses.CorpseObject)Unit;
            switch (Unit)
            {
                case WS_PlayerData.CharacterObject:
                    {
                        WS_PlayerData.CharacterObject unit1 = (WS_PlayerData.CharacterObject)Unit;
                        if (decimal.Compare(new decimal(unit1.resurrectGUID), 0m) != 0)
                        {
                            if (Caster is WS_PlayerData.CharacterObject @object)
                            {
                                Packets.PacketClass RessurectFailed = new(Opcodes.SMSG_RESURRECT_FAILED);
                                @object.client.Send(ref RessurectFailed);
                                RessurectFailed.Dispose();
                            }
                            return SpellFailedReason.SPELL_NO_ERROR;
                        }
                        unit1.resurrectGUID = Caster.GUID;
                        unit1.resurrectMapID = checked((int)Caster.MapID);
                        unit1.resurrectPositionX = Caster.positionX;
                        unit1.resurrectPositionY = Caster.positionY;
                        unit1.resurrectPositionZ = Caster.positionZ;
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        unit1.resurrectHealth = checked(unit1.Life.Maximum * SpellInfo.GetValue(Level: caster.Level, ComboPoints: 0)) / 100;
                        unit1.resurrectMana = checked(unit1.Mana.Maximum * SpellInfo.MiscValue) / 100;
                        Packets.PacketClass RessurectRequest = new(Opcodes.SMSG_RESURRECT_REQUEST);
                        RessurectRequest.AddUInt64(Caster.GUID);
                        RessurectRequest.AddUInt32(1u);
                        RessurectRequest.AddUInt16(0);
                        RessurectRequest.AddUInt32(1u);
                        unit1.client.Send(ref RessurectRequest);
                        RessurectRequest.Dispose();
                        break;
                    }

                case WS_Creatures.CreatureObject:
                    {
                        WS_Creatures.CreatureObject unit2 = (WS_Creatures.CreatureObject)Unit;
                        var unit1 = unit2;
                        Target.unitTarget.Life.Current = checked(unit1.Life.Maximum * SpellInfo.valueBase) / 100;
                        Target.unitTarget.cUnitFlags &= -16385;
                        Packets.UpdateClass UpdateData = new(188);
                        UpdateData.SetUpdateFlag(22, unit1.Life.Current);
                        UpdateData.SetUpdateFlag(46, Target.unitTarget.cUnitFlags);
                        Packets.UpdatePacketClass packetForNear = new();
                        Packets.PacketClass packet = packetForNear;
                        var updateObject = unit2;
                        UpdateData.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
                        packetForNear = (Packets.UpdatePacketClass)packet;
                        packet = packetForNear;
                        unit2.SendToNearPlayers(ref packet);
                        packetForNear = (Packets.UpdatePacketClass)packet;
                        packetForNear.Dispose();
                        UpdateData.Dispose();
                        WS_Creatures.CreatureObject unitTarget = (WS_Creatures.CreatureObject)Target.unitTarget;
                        unitTarget.MoveToInstant(Caster.positionX, Caster.positionY, Caster.positionZ, Caster.orientation);
                        break;
                    }

                case WS_Corpses.CorpseObject when WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(unit.Owner):
                    {
                        var characterObject = WorldServiceLocator._WorldServer.CHARACTERs[unit.Owner];
                        characterObject.resurrectGUID = Caster.GUID;
                        characterObject.resurrectMapID = checked((int)Caster.MapID);
                        characterObject.resurrectPositionX = Caster.positionX;
                        characterObject.resurrectPositionY = Caster.positionY;
                        characterObject.resurrectPositionZ = Caster.positionZ;
                        characterObject.resurrectHealth = checked(characterObject.Life.Maximum * SpellInfo.valueBase) / 100;
                        characterObject.resurrectMana = checked(characterObject.Mana.Maximum * SpellInfo.MiscValue) / 100;
                        Packets.PacketClass RessurectRequest = new(Opcodes.SMSG_RESURRECT_REQUEST);
                        RessurectRequest.AddUInt64(Caster.GUID);
                        RessurectRequest.AddUInt32(1u);
                        RessurectRequest.AddUInt16(0);
                        RessurectRequest.AddUInt32(1u);
                        characterObject.client.Send(ref RessurectRequest);
                        RessurectRequest.Dispose();
                        break;
                    }

                default:
                    break;
            }
        }
        return SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// SPELL_EFFECT_RESURRECT_NEW
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_RESURRECT_NEW(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        foreach (var Unit in Infected)
        {
            WS_Corpses.CorpseObject unit1 = (WS_Corpses.CorpseObject)Unit;
            switch (Unit)
            {
                case WS_PlayerData.CharacterObject:
                    {
                        WS_PlayerData.CharacterObject unit = (WS_PlayerData.CharacterObject)Unit;
                        if (decimal.Compare(new decimal(unit.resurrectGUID), 0m) != 0)
                        {
                            if (Caster is WS_PlayerData.CharacterObject @object)
                            {
                                Packets.PacketClass RessurectFailed = new(Opcodes.SMSG_RESURRECT_FAILED);
                                @object.client.Send(ref RessurectFailed);
                                RessurectFailed.Dispose();
                            }
                            return SpellFailedReason.SPELL_NO_ERROR;
                        }
                        unit.resurrectGUID = Caster.GUID;
                        var mapID = (int)Caster.MapID;
                        unit.resurrectMapID = checked(mapID);
                        unit.resurrectPositionX = Caster.positionX;
                        unit.resurrectPositionY = Caster.positionY;
                        unit.resurrectPositionZ = Caster.positionZ;
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        unit.resurrectHealth = SpellInfo.GetValue(caster.Level, 0);
                        unit.resurrectMana = SpellInfo.MiscValue;
                        Packets.PacketClass RessurectRequest2 = new(Opcodes.SMSG_RESURRECT_REQUEST);
                        RessurectRequest2.AddUInt64(Caster.GUID);
                        RessurectRequest2.AddUInt32(1u);
                        RessurectRequest2.AddUInt16(0);
                        RessurectRequest2.AddUInt32(1u);
                        unit.client.Send(ref RessurectRequest2);
                        RessurectRequest2.Dispose();
                        break;
                    }

                case WS_Creatures.CreatureObject:
                    {
                        WS_Creatures.CreatureObject unit = (WS_Creatures.CreatureObject)Unit;
                        unit.Life.Current = checked(unit.Life.Maximum * SpellInfo.valueBase) / 100;
                        Packets.UpdatePacketClass packetForNear = new();
                        Packets.UpdateClass UpdateData = new(188);
                        UpdateData.SetUpdateFlag(22, unit.Life.Current);
                        Packets.PacketClass packet = packetForNear;
                        var updateObject = unit;
                        UpdateData.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
                        Packets.UpdatePacketClass packet1 = (Packets.UpdatePacketClass)packet;
                        packetForNear = packet1;
                        packet = packetForNear;
                        unit.SendToNearPlayers(ref packet);
                        packetForNear = packet1;
                        packetForNear.Dispose();
                        UpdateData.Dispose();
                        WS_Creatures.CreatureObject unitTarget = (WS_Creatures.CreatureObject)Target.unitTarget;
                        unitTarget.MoveToInstant(Caster.positionX, Caster.positionY, Caster.positionZ, Caster.orientation);
                        break;
                    }

                case WS_Corpses.CorpseObject when WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(unit1.Owner):
                    {
                        var unit = unit1;
                        WorldServiceLocator._WorldServer.CHARACTERs[unit.Owner].resurrectGUID = Caster.GUID;
                        WorldServiceLocator._WorldServer.CHARACTERs[unit.Owner].resurrectMapID = checked((int)Caster.MapID);
                        WorldServiceLocator._WorldServer.CHARACTERs[unit.Owner].resurrectPositionX = Caster.positionX;
                        WorldServiceLocator._WorldServer.CHARACTERs[unit.Owner].resurrectPositionY = Caster.positionY;
                        WorldServiceLocator._WorldServer.CHARACTERs[unit.Owner].resurrectPositionZ = Caster.positionZ;
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        WorldServiceLocator._WorldServer.CHARACTERs[unit.Owner].resurrectHealth = SpellInfo.GetValue(caster.Level, 0);
                        WorldServiceLocator._WorldServer.CHARACTERs[unit.Owner].resurrectMana = SpellInfo.MiscValue;
                        Packets.PacketClass RessurectRequest = new(Opcodes.SMSG_RESURRECT_REQUEST);
                        RessurectRequest.AddUInt64(Caster.GUID);
                        RessurectRequest.AddUInt32(1u);
                        RessurectRequest.AddUInt16(0);
                        RessurectRequest.AddUInt32(1u);
                        WorldServiceLocator._WorldServer.CHARACTERs[unit.Owner].client.Send(ref RessurectRequest);
                        RessurectRequest.Dispose();
                        break;
                    }

                default:
                    break;
            }
        }
        return SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// SPELL_EFFECT_TELEPORT_GRAVEYARD
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_TELEPORT_GRAVEYARD(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        for (var i = 0; i < Infected.Count; i++)
        {
            WS_Base.BaseUnit Unit = (WS_Base.BaseUnit)Infected[i];
            if (Unit is WS_PlayerData.CharacterObject @object)
            {
                var allGraveYards = WorldServiceLocator._WorldServer.AllGraveYards;
                var Character = @object;
                allGraveYards.GoToNearestGraveyard(ref Character, Alive: false, Teleport: true);
            }
        }
        return SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// SPELL_EFFECT_INTERRUPT_CAST
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_INTERRUPT_CAST(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        for (var i = 0; i < Infected.Count; i++)
        {
            WS_Base.BaseUnit Unit = (WS_Base.BaseUnit)Infected[i];
            switch (Unit)
            {
                case WS_PlayerData.CharacterObject:
                    WS_PlayerData.CharacterObject unit = (WS_PlayerData.CharacterObject)Unit;
                    if (unit.FinishAllSpells())
                    {
                        unit.ProhibitSpellSchool(SPELLs[SpellID].School, SPELLs[SpellID].GetDuration);
                    }
                    break;
                case WS_Creatures.CreatureObject:
                    WS_Creatures.CreatureObject unit1 = (WS_Creatures.CreatureObject)Unit;
                    unit1.StopCasting();
                    break;
                default:
                    break;
            }
        }
        return SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// SPELL_EFFECT_STEALTH
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_STEALTH(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        for (var i = 0; i < Infected.Count; i++)
        {
            WS_Base.BaseUnit Unit = (WS_Base.BaseUnit)Infected[i];
            ref var cBytes = ref Unit.cBytes1;
            checked
            {
                var value = (uint)cBytes;
                var functions = WorldServiceLocator._Functions;
                functions.SetFlag(ref value, 25, flagValue: true);
                cBytes = (int)value;
                Unit.Invisibility = InvisibilityLevel.INIVISIBILITY;
            }
            WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
            Unit.Invisibility_Value = SpellInfo.GetValue(Level: caster.Level, ComboPoints: 0);
            if (Unit is WS_PlayerData.CharacterObject @object)
            {
                var wS_CharMovement = WorldServiceLocator._WS_CharMovement;
                var Character = @object;
                wS_CharMovement.UpdateCell(ref Character);
            }
        }
        return SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// SPELL_EFFECT_DETECT
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_DETECT(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        for (var i = 0; i < Infected.Count; i++)
        {
            WS_Base.BaseUnit Unit = (WS_Base.BaseUnit)Infected[i];
            Unit.CanSeeInvisibility = InvisibilityLevel.INIVISIBILITY;
            WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
            Unit.CanSeeInvisibility_Stealth = SpellInfo.GetValue(caster.Level, 0);
            if (Unit is WS_PlayerData.CharacterObject @object)
            {
                var wS_CharMovement = WorldServiceLocator._WS_CharMovement;
                var Character = @object;
                wS_CharMovement.UpdateCell(ref Character);
            }
        }
        return SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// SPELL_EFFECT_LEAP
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_LEAP(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        var selectedX = (float)(Caster.positionX + (Math.Cos(Caster.orientation) * SpellInfo.GetRadius));
        var selectedY = (float)(Caster.positionY + (Math.Sin(Caster.orientation) * SpellInfo.GetRadius));
        var selectedZ = WorldServiceLocator._WS_Maps.GetZCoord(selectedX, selectedY, Caster.positionZ, Caster.MapID);
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
            case WS_PlayerData.CharacterObject:
                WS_PlayerData.CharacterObject caster = (WS_PlayerData.CharacterObject)Caster;
                caster.Teleport(selectedX, selectedY, selectedZ, Caster.orientation, checked((int)Caster.MapID));
                break;

            default:
                WS_Creatures.CreatureObject caster1 = (WS_Creatures.CreatureObject)Caster;
                caster1.MoveToInstant(selectedX, selectedY, selectedZ, Caster.orientation);
                break;
        }
        return SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// SPELL_EFFECT_SUMMON
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_FAILED_CANT_DO_THAT_YET (NOT IMPLEMENTED)</returns>
    public SpellFailedReason SPELL_EFFECT_SUMMON(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        return SpellFailedReason.SPELL_FAILED_CANT_DO_THAT_YET;
    }

    /// <summary>
    /// SPELL_EFFECT_SUMMON_WILD
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_EFFECT_SUMMON_WILD</returns>
    public SpellFailedReason SPELL_EFFECT_SUMMON_WILD(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        if (SPELLs[SpellID].GetDuration == 0)
        {
            float SelectedX;
            float SelectedY;
            float SelectedZ;
            var targetMask = (uint)Target.targetMask;
            if ((targetMask & 0x40u) != 0)
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

            WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
            var mapID = (int)Caster.MapID;
            WS_Creatures.CreatureObject creatureObject = new(SpellInfo.MiscValue, SelectedX, SelectedY, SelectedZ, Caster.orientation, checked(mapID), SPELLs[SpellID].GetDuration)
            {
                Level = caster.Level,
                CreatedBy = Caster.GUID,
                CreatedBySpell = SpellID
            };
            var tmpCreature = creatureObject;
            tmpCreature.AddToWorld();
            return SpellFailedReason.SPELL_NO_ERROR;
        }
        SpellFailedReason SPELL_EFFECT_SUMMON_WILD = default;
        return SPELL_EFFECT_SUMMON_WILD;
    }

    /// <summary>
    /// SPELL_EFFECT_SUMMON_TOTEM
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_EFFECT_SUMMON_TOTEM</returns>
    public SpellFailedReason SPELL_EFFECT_SUMMON_TOTEM(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        checked
        {
            byte Slot;
            float angle;
            float selectedX;
            float selectedY;
            float selectedZ;
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
                        WS_PlayerData.CharacterObject caster = (WS_PlayerData.CharacterObject)Caster;
                        var GUID = caster.TotemSlot[Slot];
                        if (decimal.Compare(new decimal(GUID), 0m) != 0 && WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(GUID))
                        {
                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[DEBUG] Destroyed old totem.");
                            WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].Destroy();
                        }
                    }
                    angle = 0f;
                    if (Slot < 4)
                    {
                        angle = (float)((Math.PI / 4.0) - (Slot * 2 * Math.PI / 4.0));
                    }
                    selectedX = (float)(Caster.positionX + (Math.Cos(Caster.orientation) * 2.0));
                    selectedY = (float)(Caster.positionY + (Math.Sin(Caster.orientation) * 2.0));
                    selectedZ = WorldServiceLocator._WS_Maps.GetZCoord(selectedX, selectedY, Caster.positionZ, Caster.MapID);
                    WS_Totems.TotemObject NewTotem = new(SpellInfo.MiscValue, selectedX, selectedY, selectedZ, angle, (int)Caster.MapID, SPELLs[SpellID].GetDuration);
                    WS_Base.BaseUnit caster1 = (WS_Base.BaseUnit)Caster;
                    NewTotem.Life.Base = SpellInfo.GetValue(caster1.Level, 0);
                    NewTotem.Life.Current = NewTotem.Life.Maximum;
                    NewTotem.Caster = caster1;
                    NewTotem.Level = caster1.Level;
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
                        CreatureInfo tmpInfo = new(SpellInfo.MiscValue);
                        WorldServiceLocator._WorldServer.CREATURESDatabase.Add(SpellInfo.MiscValue, tmpInfo);
                    }
                    NewTotem.InitSpell(WorldServiceLocator._WorldServer.CREATURESDatabase[SpellInfo.MiscValue].Spells[0]);
                    NewTotem.AddToWorld();
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[DEBUG] Totem spawned by [{0:X}].", NewTotem.GUID);
                    if (Slot < 4 && Caster is WS_PlayerData.CharacterObject object1)
                    {
                        object1.TotemSlot[Slot] = NewTotem.GUID;
                    }
                    return SpellFailedReason.SPELL_NO_ERROR;
            }
        }
    }

    /// <summary>
    /// SPELL_EFFECT_SUMMON_OBJECT
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_SUMMON_OBJECT(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        if (Caster is not WS_Base.BaseUnit)
        {
            return SpellFailedReason.SPELL_FAILED_CASTER_DEAD;
        }
        float selectedX;
        float selectedY;
        if (SpellInfo.RadiusIndex > 0)
        {
            selectedX = (float)(Caster.positionX + (Math.Cos(Caster.orientation) * SpellInfo.GetRadius));
            selectedY = (float)(Caster.positionY + (Math.Sin(Caster.orientation) * SpellInfo.GetRadius));
        }
        else
        {
            selectedX = (float)(Caster.positionX + (Math.Cos(Caster.orientation) * SPELLs[SpellID].GetRange));
            selectedY = (float)(Caster.positionY + (Math.Sin(Caster.orientation) * SPELLs[SpellID].GetRange));
        }
        var GameobjectInfo = WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase.ContainsKey(SpellInfo.MiscValue) ? WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase[SpellInfo.MiscValue] : new WS_GameObjects.GameObjectInfo(SpellInfo.MiscValue);
        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
        WS_GameObjects.GameObject gameObjectObject = new(PosZ: (GameobjectInfo.Type != GameObjectType.GAMEOBJECT_TYPE_FISHINGNODE) ? WorldServiceLocator._WS_Maps.GetZCoord(selectedX, selectedY, Caster.positionZ, Caster.MapID) : WorldServiceLocator._WS_Maps.GetWaterLevel(selectedX, selectedY, checked((int)Caster.MapID)), ID_: SpellInfo.MiscValue, MapID_: Caster.MapID, PosX: selectedX, PosY: selectedY, Rotation: Caster.orientation, Owner_: Caster.GUID)
        {
            CreatedBySpell = SpellID,
            Level = caster.Level,
            instance = Caster.instance
        };
        var tmpGO = gameObjectObject;
        caster.gameObjects.Add(tmpGO);
        if (GameobjectInfo.Type == GameObjectType.GAMEOBJECT_TYPE_FISHINGNODE)
        {
            tmpGO.SetupFishingNode();
        }
        tmpGO.AddToWorld();
        Packets.PacketClass packet = new(Opcodes.SMSG_GAMEOBJECT_SPAWN_ANIM);
        packet.AddUInt64(tmpGO.GUID);
        tmpGO.SendToNearPlayers(ref packet);
        packet.Dispose();
        if (GameobjectInfo.Type == GameObjectType.GAMEOBJECT_TYPE_FISHINGNODE)
        {
            WS_PlayerData.CharacterObject caster1 = (WS_PlayerData.CharacterObject)Caster;
            caster1.SetUpdateFlag(144, SpellID);
            caster1.SetUpdateFlag(20, tmpGO.GUID);
            caster1.SendCharacterUpdate();
        }
        if (SPELLs[SpellID].GetDuration > 0)
        {
            tmpGO.Despawn(SPELLs[SpellID].GetDuration);
        }
        return SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// SPELL_EFFECT_ENCHANT_ITEM
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
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
            Packets.PacketClass EnchantLog = new(Opcodes.SMSG_ENCHANTMENTLOG);
            EnchantLog.AddUInt64(Target.itemTarget.OwnerGUID);
            EnchantLog.AddUInt64(Caster.GUID);
            EnchantLog.AddInt32(Target.itemTarget.ItemEntry);
            EnchantLog.AddInt32(SpellInfo.MiscValue);
            EnchantLog.AddInt8(0);
            var client = WorldServiceLocator._WorldServer.CHARACTERs[Target.itemTarget.OwnerGUID].client;
            client.Send(ref EnchantLog);
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

    /// <summary>
    /// SPELL_EFFECT_ENCHANT_ITEM_TEMPORARY
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_ENCHANT_ITEM_TEMPORARY(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        if (Target.itemTarget == null)
        {
            return SpellFailedReason.SPELL_FAILED_ITEM_NOT_FOUND;
        }
        var Duration = SPELLs[SpellID].GetDuration;
        if (Duration == 0)
        {
            Duration = (SPELLs[SpellID].SpellVisual == 563) ? 600 : ((SPELLs[SpellID].SpellFamilyName == 8) ? 3600 : ((SPELLs[SpellID].SpellFamilyName == 11) ? 1800 : ((SPELLs[SpellID].SpellVisual == 215) ? 1800 : ((SPELLs[SpellID].SpellVisual != 0) ? 3600 : 1800))));
            Duration = checked(Duration * 1000);
        }
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[DEBUG] Enchant duration [{0}]", Duration);
        Target.itemTarget.AddEnchantment(SpellInfo.MiscValue, 1, Duration);
        if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(Target.itemTarget.OwnerGUID))
        {
            WorldServiceLocator._WorldServer.CHARACTERs[Target.itemTarget.OwnerGUID].SendItemUpdate(Target.itemTarget);
            Packets.PacketClass EnchantLog = new(Opcodes.SMSG_ENCHANTMENTLOG);
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

    /// <summary>
    /// SPELL_EFFECT_ENCHANT_HELD_ITEM
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_ENCHANT_HELD_ITEM(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        var Duration = SPELLs[SpellID].GetDuration;
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

    /// <summary>
    /// SPELL_EFFECT_CHARGE
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_CHARGE(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        if (Caster is WS_Creatures.CreatureObject @object)
        {
            @object.SetToRealPosition();
        }
        var NearX = Target.unitTarget.positionX;
        NearX = (!(Target.unitTarget.positionX > Caster.positionX)) ? (NearX + 1f) : (NearX - 1f);
        var NearY = Target.unitTarget.positionY;
        NearY = (!(Target.unitTarget.positionY > Caster.positionY)) ? (NearY + 1f) : (NearY - 1f);
        var NearZ = WorldServiceLocator._WS_Maps.GetZCoord(NearX, NearY, Caster.positionZ, Caster.MapID);
        if ((NearZ > Target.unitTarget.positionZ + 2f) | (NearZ < Target.unitTarget.positionZ - 2f))
        {
            NearZ = Target.unitTarget.positionZ;
        }
        var moveDist = WorldServiceLocator._WS_Combat.GetDistance(Caster, NearX, NearY, NearZ);
        var TimeToMove = checked((double)Math.Round(moveDist / SPELLs[SpellID].Speed * 1000f));
        Packets.PacketClass SMSG_MONSTER_MOVE = new(Opcodes.SMSG_MONSTER_MOVE);
        SMSG_MONSTER_MOVE.AddPackGUID(Caster.GUID);
        SMSG_MONSTER_MOVE.AddSingle(Caster.positionX);
        SMSG_MONSTER_MOVE.AddSingle(Caster.positionY);
        SMSG_MONSTER_MOVE.AddSingle(Caster.positionZ);
        SMSG_MONSTER_MOVE.AddInt32(WorldServiceLocator._NativeMethods.timeGetTime(""));
        SMSG_MONSTER_MOVE.AddInt8(0);
        SMSG_MONSTER_MOVE.AddInt32(256);
        var timeToMove = (int)TimeToMove;
        SMSG_MONSTER_MOVE.AddInt32(timeToMove);
        SMSG_MONSTER_MOVE.AddInt32(1);
        SMSG_MONSTER_MOVE.AddSingle(NearX);
        SMSG_MONSTER_MOVE.AddSingle(NearY);
        SMSG_MONSTER_MOVE.AddSingle(NearZ);
        Caster.SendToNearPlayers(ref SMSG_MONSTER_MOVE);
        SMSG_MONSTER_MOVE.Dispose();
        if (Caster is WS_PlayerData.CharacterObject object1)
        {
            WorldServiceLocator._WS_Combat.SendAttackStart(attackerGUID: Caster.GUID, victimGUID: Target.unitTarget.GUID, client: object1.client);
            object1.attackState.AttackStart(Victim_: Target.unitTarget);
        }
        return SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// SPELL_EFFECT_KNOCK_BACK
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_KNOCK_BACK(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        for (var i = 0; i < Infected.Count; i++)
        {
            WS_Base.BaseUnit Unit = (WS_Base.BaseUnit)Infected[i];
            var Direction = WorldServiceLocator._WS_Combat.GetOrientation(Caster.positionX, Unit.positionX, Caster.positionY, Unit.positionY);
            Packets.PacketClass packet = new(Opcodes.SMSG_MOVE_KNOCK_BACK);
            packet.AddPackGUID(Unit.GUID);
            packet.AddInt32(0);
            packet.AddSingle((float)Math.Cos(Direction));
            packet.AddSingle((float)Math.Sin(Direction));
            WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
            packet.AddSingle(SpellInfo.GetValue(caster.Level, 0) / 10f);
            packet.AddSingle(SpellInfo.MiscValue / -10f);
            Unit.SendToNearPlayers(ref packet);
            packet.Dispose();
            if (Unit is not WS_Creatures.CreatureObject)
            {
                return SpellFailedReason.SPELL_FAILED_CANT_DO_THAT_YET; // PlayerVsPlayer?
            }
        }
        return SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// SPELL_EFFECT_SCRIPT_EFFECT
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_SCRIPT_EFFECT(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        if (SPELLs[SpellID].SpellFamilyName == 10)
        {
            if (SPELLs[SpellID].SpellIconID is 70 or 242)
            {
                return SPELL_EFFECT_HEAL(ref Target, ref Caster, ref SpellInfo, SpellID, ref Infected, ref Item);
            }

            var spellFamilyFlags = (uint)SPELLs[SpellID].SpellFamilyFlags;
            if ((spellFamilyFlags & 0x800000u) != 0)
            {
                if (Target.unitTarget == null || Target.unitTarget.IsDead)
                {
                    return SpellFailedReason.SPELL_FAILED_TARGETS_DEAD;
                }
                checked
                {
                    var num = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1;
                    var SpellID2 = 0;
                    for (var i = 0; i <= num; i++)
                    {
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        if (caster.ActiveSpells[i] != null && caster.ActiveSpells[i].GetSpellInfo.SpellVisual == 5622 && caster.ActiveSpells[i].GetSpellInfo.SpellFamilyName == 10 && caster.ActiveSpells[i].Aura_Info[2] != null)
                        {
                            SpellID2 = caster.ActiveSpells[i].Aura_Info[2].valueBase + 1;
                            break;
                        }
                    }
                    if (SpellID2 == 0 || !SPELLs.ContainsKey(SpellID2))
                    {
                        return SpellFailedReason.SPELL_FAILED_UNKNOWN;
                    }
                    CastSpellParameters castParams = new(ref Target, ref Caster, SpellID2);
                    ThreadPool.QueueUserWorkItem(castParams.Cast);
                }
            }
        }
        return SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// SPELL_EFFECT_DUEL
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_EFFECT_DUEL</returns>
    public SpellFailedReason SPELL_EFFECT_DUEL(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        var implicitTargetA = SpellInfo.implicitTargetA;
        if (implicitTargetA == 25)
        {
            if (Target.unitTarget is not WS_PlayerData.CharacterObject)
            {
                return SpellFailedReason.SPELL_FAILED_TARGET_NOT_PLAYER;
            }
            if (Caster is WS_PlayerData.CharacterObject @object)
            {
                if (decimal.Compare(new decimal(@object.DuelArbiter), 0m) != 0)
                {
                    return SpellFailedReason.SPELL_FAILED_SPELL_IN_PROGRESS;
                }
                WS_PlayerData.CharacterObject unitTarget = (WS_PlayerData.CharacterObject)Target.unitTarget;
                if (unitTarget.IsInDuel)
                {
                    return SpellFailedReason.SPELL_FAILED_TARGET_DUELING;
                }
                if (unitTarget.inCombatWith.Count > 0)
                {
                    return SpellFailedReason.SPELL_FAILED_TARGET_IN_COMBAT;
                }
                if (Caster.Invisibility != 0)
                {
                    return SpellFailedReason.SPELL_FAILED_CANT_DUEL_WHILE_INVISIBLE;
                }
                var flagX = Caster.positionX + ((Target.unitTarget.positionX - Caster.positionX) / 2f);
                var flagY = Caster.positionY + ((Target.unitTarget.positionY - Caster.positionY) / 2f);
                var flagZ = WorldServiceLocator._WS_Maps.GetZCoord(flagX, flagY, Caster.positionZ + 3f, Caster.MapID);
                WS_GameObjects.GameObject tmpGO = new(SpellInfo.MiscValue, Caster.MapID, flagX, flagY, flagZ, 0f, Caster.GUID);
                tmpGO.AddToWorld();
                unitTarget.DuelArbiter = tmpGO.GUID;
                @object.DuelPartner = unitTarget;
                unitTarget.DuelPartner = @object;
                Packets.PacketClass packet = new(Opcodes.SMSG_DUEL_REQUESTED);
                packet.AddUInt64(tmpGO.GUID);
                packet.AddUInt64(Caster.GUID);
                unitTarget.client.SendMultiplyPackets(ref packet);
                @object.client.SendMultiplyPackets(ref packet);
                packet.Dispose();
                return SpellFailedReason.SPELL_NO_ERROR;
            }
            SpellFailedReason SPELL_EFFECT_DUEL = default;
            return SPELL_EFFECT_DUEL;
        }
        return SpellFailedReason.SPELL_FAILED_BAD_IMPLICIT_TARGETS;
    }

    /// <summary>
    /// SPELL_EFFECT_QUEST_COMPLETE
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="SpellInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="Infected"></param>
    /// <param name="Item"></param>
    /// <returns>SPELL_NO_ERROR</returns>
    public SpellFailedReason SPELL_EFFECT_QUEST_COMPLETE(ref SpellTargets Target, ref WS_Base.BaseObject Caster, ref SpellEffect SpellInfo, int SpellID, ref List<WS_Base.BaseObject> Infected, ref ItemObject Item)
    {
        for (var i = 0; i < Infected.Count; i++)
        {
            WS_Base.BaseUnit Unit = (WS_Base.BaseUnit)Infected[i];
            if (Unit is WS_PlayerData.CharacterObject @object)
            {
                var aLLQUESTS = WorldServiceLocator._WorldServer.ALLQUESTS;
                var objCharacter = @object;
                aLLQUESTS.CompleteQuest(ref objCharacter, SpellInfo.MiscValue, Caster.GUID);
            }
        }
        return SpellFailedReason.SPELL_NO_ERROR;
    }

    /// <summary>
    /// List GetEnemyAtPoint
    /// </summary>
    /// <param name="objCharacter"></param>
    /// <param name="PosX"></param>
    /// <param name="PosY"></param>
    /// <param name="PosZ"></param>
    /// <param name="Distance"></param>
    /// <returns></returns>
    public List<WS_Base.BaseUnit> GetEnemyAtPoint(ref WS_Base.BaseUnit objCharacter, float PosX, float PosY, float PosZ, float Distance)
    {
        List<WS_Base.BaseUnit> result = new();
        switch (objCharacter)
        {
            case WS_PlayerData.CharacterObject:
                {
                    WS_PlayerData.CharacterObject objCharacter1 = (WS_PlayerData.CharacterObject)objCharacter;
                    var array = objCharacter1.playersNear.ToArray();
                    foreach (var pGUID2 in array)
                    {
                        if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(pGUID2) && (objCharacter1.IsHorde != WorldServiceLocator._WorldServer.CHARACTERs[pGUID2].IsHorde || (objCharacter1.DuelPartner != null && objCharacter1.DuelPartner == WorldServiceLocator._WorldServer.CHARACTERs[pGUID2])) && !WorldServiceLocator._WorldServer.CHARACTERs[pGUID2].IsDead && WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.CHARACTERs[pGUID2], PosX, PosY, PosZ) < Distance)
                        {
                            result.Add(WorldServiceLocator._WorldServer.CHARACTERs[pGUID2]);
                        }
                    }
                    var array2 = objCharacter1.creaturesNear.ToArray();
                    foreach (var cGUID in array2)
                    {
                        if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(cGUID) && WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID] is not WS_Totems.TotemObject && !WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].IsDead && objCharacter1.GetReaction(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].Faction) <= TReaction.NEUTRAL && WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID], PosX, PosY, PosZ) < Distance)
                        {
                            result.Add(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID]);
                        }
                    }

                    break;
                }

            case WS_Creatures.CreatureObject:
                {
                    var array3 = objCharacter.SeenBy.ToArray();
                    foreach (var pGUID in array3)
                    {
                        if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(pGUID) && !WorldServiceLocator._WorldServer.CHARACTERs[pGUID].IsDead && WorldServiceLocator._WorldServer.CHARACTERs[pGUID].GetReaction(((WS_Creatures.CreatureObject)objCharacter).Faction) <= TReaction.NEUTRAL && WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.CHARACTERs[pGUID], PosX, PosY, PosZ) < Distance)
                        {
                            result.Add(WorldServiceLocator._WorldServer.CHARACTERs[pGUID]);
                        }
                    }

                    break;
                }

            default:
                break;
        }
        return result;
    }

    /// <summary>
    /// List GetEnemyAroundMe
    /// </summary>
    /// <param name="objCharacter"></param>
    /// <param name="Distance"></param>
    /// <param name="r"></param>
    /// <returns></returns>
    public List<WS_Base.BaseUnit> GetEnemyAroundMe(ref WS_Base.BaseUnit objCharacter, float Distance, WS_Base.BaseUnit r = null)
    {
        List<WS_Base.BaseUnit> result = new();
        if (r == null)
        {
            r = objCharacter;
        }
        switch (r)
        {
            case WS_PlayerData.CharacterObject:
                {
                    WS_PlayerData.CharacterObject r1 = (WS_PlayerData.CharacterObject)r;
                    var array = r1.playersNear.ToArray();
                    foreach (var pGUID2 in array)
                    {
                        if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(pGUID2) && (r1.IsHorde != WorldServiceLocator._WorldServer.CHARACTERs[pGUID2].IsHorde || (r1.DuelPartner != null && r1.DuelPartner == WorldServiceLocator._WorldServer.CHARACTERs[pGUID2])) && !WorldServiceLocator._WorldServer.CHARACTERs[pGUID2].IsDead && WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.CHARACTERs[pGUID2], objCharacter) < Distance)
                        {
                            result.Add(WorldServiceLocator._WorldServer.CHARACTERs[pGUID2]);
                        }
                    }
                    var array2 = r1.creaturesNear.ToArray();
                    foreach (var cGUID in array2)
                    {
                        if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(cGUID) && WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID] is not WS_Totems.TotemObject && !WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].IsDead && r1.GetReaction(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].Faction) <= TReaction.NEUTRAL && WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID], objCharacter) < Distance)
                        {
                            result.Add(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID]);
                        }
                    }

                    break;
                }

            case WS_Creatures.CreatureObject:
                {
                    var array3 = r.SeenBy.ToArray();
                    foreach (var pGUID in array3)
                    {
                        if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(pGUID) && !WorldServiceLocator._WorldServer.CHARACTERs[pGUID].IsDead && WorldServiceLocator._WorldServer.CHARACTERs[pGUID].GetReaction(((WS_Creatures.CreatureObject)r).Faction) <= TReaction.NEUTRAL && WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.CHARACTERs[pGUID], objCharacter) < Distance)
                        {
                            result.Add(WorldServiceLocator._WorldServer.CHARACTERs[pGUID]);
                        }
                    }

                    break;
                }

            default:
                break;
        }
        return result;
    }

    /// <summary>
    /// List GetFriendAroundMe
    /// </summary>
    /// <param name="objCharacter"></param>
    /// <param name="Distance"></param>
    /// <returns></returns>
    public List<WS_Base.BaseUnit> GetFriendAroundMe(ref WS_Base.BaseUnit objCharacter, float Distance)
    {
        List<WS_Base.BaseUnit> result = new();
        switch (objCharacter)
        {
            case WS_PlayerData.CharacterObject:
                {
                    WS_PlayerData.CharacterObject objCharacter1 = (WS_PlayerData.CharacterObject)objCharacter;
                    var array = objCharacter1.playersNear.ToArray();
                    foreach (var pGUID2 in array)
                    {
                        if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(pGUID2) && objCharacter1.IsHorde == WorldServiceLocator._WorldServer.CHARACTERs[pGUID2].IsHorde && !WorldServiceLocator._WorldServer.CHARACTERs[pGUID2].IsDead && WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.CHARACTERs[pGUID2], objCharacter) < Distance)
                        {
                            result.Add(WorldServiceLocator._WorldServer.CHARACTERs[pGUID2]);
                        }
                    }
                    var array2 = objCharacter1.creaturesNear.ToArray();
                    foreach (var cGUID in array2)
                    {
                        if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(cGUID) && WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID] is not WS_Totems.TotemObject && !WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].IsDead && objCharacter1.GetReaction(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID].Faction) > TReaction.NEUTRAL && WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID], objCharacter) < Distance)
                        {
                            result.Add(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGUID]);
                        }
                    }

                    break;
                }

            case WS_Creatures.CreatureObject:
                {
                    var array3 = objCharacter.SeenBy.ToArray();
                    foreach (var pGUID in array3)
                    {
                        if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(pGUID) && !WorldServiceLocator._WorldServer.CHARACTERs[pGUID].IsDead && WorldServiceLocator._WorldServer.CHARACTERs[pGUID].GetReaction(((WS_Creatures.CreatureObject)objCharacter).Faction) > TReaction.NEUTRAL && WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.CHARACTERs[pGUID], objCharacter) < Distance)
                        {
                            result.Add(WorldServiceLocator._WorldServer.CHARACTERs[pGUID]);
                        }
                    }

                    break;
                }

            default:
                break;
        }
        return result;
    }

    /// <summary>
    /// List GetFriendPlayersAroundMe
    /// </summary>
    /// <param name="objCharacter"></param>
    /// <param name="Distance"></param>
    /// <returns></returns>
    public List<WS_Base.BaseUnit> GetFriendPlayersAroundMe(ref WS_Base.BaseUnit objCharacter, float Distance)
    {
        List<WS_Base.BaseUnit> result = new();
        switch (objCharacter)
        {
            case WS_PlayerData.CharacterObject:
                {
                    WS_PlayerData.CharacterObject objCharacter1 = (WS_PlayerData.CharacterObject)objCharacter;
                    var array = objCharacter1.playersNear.ToArray();
                    foreach (var pGUID2 in array)
                    {
                        if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(pGUID2) && objCharacter1.IsHorde == WorldServiceLocator._WorldServer.CHARACTERs[pGUID2].IsHorde && !WorldServiceLocator._WorldServer.CHARACTERs[pGUID2].IsDead && WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.CHARACTERs[pGUID2], objCharacter) < Distance)
                        {
                            result.Add(WorldServiceLocator._WorldServer.CHARACTERs[pGUID2]);
                        }
                    }

                    break;
                }

            case WS_Creatures.CreatureObject:
                {
                    var array2 = objCharacter.SeenBy.ToArray();
                    foreach (var pGUID in array2)
                    {
                        if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(pGUID) && !WorldServiceLocator._WorldServer.CHARACTERs[pGUID].IsDead && WorldServiceLocator._WorldServer.CHARACTERs[pGUID].GetReaction(((WS_Creatures.CreatureObject)objCharacter).Faction) > TReaction.NEUTRAL && WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.CHARACTERs[pGUID], objCharacter) < Distance)
                        {
                            result.Add(WorldServiceLocator._WorldServer.CHARACTERs[pGUID]);
                        }
                    }

                    break;
                }

            default:
                break;
        }
        return result;
    }

    /// <summary>
    /// List GetPartyMembersAroundMe
    /// </summary>
    /// <param name="objCharacter"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    public List<WS_Base.BaseUnit> GetPartyMembersAroundMe(ref WS_PlayerData.CharacterObject objCharacter, float distance)
    {
        List<WS_Base.BaseUnit> list = new()
        {
            objCharacter
        };
        var result = list;
        if (!objCharacter.IsInGroup)
        {
            return result;
        }
        var array = objCharacter.Group.LocalMembers.ToArray();
        foreach (var GUID in array)
        {
            if (objCharacter.playersNear.Contains(GUID) && WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(GUID) && WorldServiceLocator._WS_Combat.GetDistance(objCharacter, WorldServiceLocator._WorldServer.CHARACTERs[GUID]) < distance)
            {
                result.Add(WorldServiceLocator._WorldServer.CHARACTERs[GUID]);
            }
        }
        return result;
    }

    /// <summary>
    /// List GetPartyMembersAtPoint
    /// </summary>
    /// <param name="objCharacter"></param>
    /// <param name="Distance"></param>
    /// <param name="PosX"></param>
    /// <param name="PosY"></param>
    /// <param name="PosZ"></param>
    /// <returns></returns>
    public List<WS_Base.BaseUnit> GetPartyMembersAtPoint(ref WS_PlayerData.CharacterObject objCharacter, float Distance, float PosX, float PosY, float PosZ)
    {
        List<WS_Base.BaseUnit> result = new();
        if (WorldServiceLocator._WS_Combat.GetDistance(objCharacter, PosX, PosY, PosZ) < Distance)
        {
            result.Add(objCharacter);
        }
        if (!objCharacter.IsInGroup)
        {
            return result;
        }
        var array = objCharacter.Group.LocalMembers.ToArray();
        foreach (var GUID in array)
        {
            if (objCharacter.playersNear.Contains(GUID) && WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(GUID) && WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.CHARACTERs[GUID], PosX, PosY, PosZ) < Distance)
            {
                result.Add(WorldServiceLocator._WorldServer.CHARACTERs[GUID]);
            }
        }
        return result;
    }

    /// <summary>
    /// List GetEnemtInFrontOfMe
    /// </summary>
    /// <param name="objCharacter"></param>
    /// <param name="Distance"></param>
    /// <returns></returns>
    public List<WS_Base.BaseUnit> GetEnemyInFrontOfMe(ref WS_Base.BaseUnit objCharacter, float Distance)
    {
        List<WS_Base.BaseUnit> result = new();
        WS_Base.BaseUnit r = null;
        var tmp = GetEnemyAroundMe(ref objCharacter, Distance, r);
        foreach (var unit in tmp)
        {
            var wS_Combat = WorldServiceLocator._WS_Combat;
            WS_Base.BaseObject Object = objCharacter;
            WS_Base.BaseObject Object2 = unit;
            var flag = wS_Combat.IsInFrontOf(ref Object, ref Object2);
            WS_Base.BaseUnit @object = (WS_Base.BaseUnit)Object;
            objCharacter = @object;
            if (flag)
            {
                result.Add(unit);
            }
        }
        return result;
    }

    /// <summary>
    /// List GetEnemyInBehindMe
    /// </summary>
    /// <param name="objCharacter"></param>
    /// <param name="Distance"></param>
    /// <returns></returns>
    public List<WS_Base.BaseUnit> GetEnemyInBehindMe(ref WS_Base.BaseUnit objCharacter, float Distance)
    {
        List<WS_Base.BaseUnit> result = new();
        WS_Base.BaseUnit r = null;
        var tmp = GetEnemyAroundMe(ref objCharacter, Distance, r);
        foreach (var unit in tmp)
        {
            var wS_Combat = WorldServiceLocator._WS_Combat;
            WS_Base.BaseObject Object = objCharacter;
            WS_Base.BaseObject Object2 = unit;
            var flag = wS_Combat.IsInBackOf(ref Object, ref Object2);
            WS_Base.BaseUnit @object = (WS_Base.BaseUnit)Object;
            objCharacter = @object;
            if (flag)
            {
                result.Add(unit);
            }
        }
        return result;
    }

    /// <summary>
    /// SPELL_AURA_NONE
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_NONE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[DEBUG] Aura None for spell. [Target:{0} Caster:{1} EffectInfo:{2} SpellID:{3} StackCount:{4} Action:{5}]", Target, Caster, EffectInfo, SpellID, StackCount, Action);
    }

    /// <summary>
    /// SPELL_AURA_DUMMY
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_DUMMY(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[DEBUG] Aura Dummy for spell. [Target:{0} Caster:{1} EffectInfo:{2} SpellID:{3} StackCount:{4} Action:{5}]", Target, Caster, EffectInfo, SpellID, StackCount, Action);
        if (Action == AuraAction.AURA_REMOVEBYDURATION && SpellID == 33763)
        {
            WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
            var Damage = EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0);
            var Caster2 = caster;
            SendHealSpellLog(ref Caster2, ref Target, SpellID, Damage, CriticalHit: false);
            Caster = Caster2;
            Caster2 = null;
            var obj = Target;
            obj.Heal(Damage, Caster2);
        }
    }

    /// <summary>
    /// SPELL_AURA_BIND_SIGHT
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
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
            default:
                break;
        }
    }

    /// <summary>
    /// SPELL_AURA_FAR_SIGHT
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
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
            default:
                break;
        }
    }

    /// <summary>
    /// SPELL_AURA_SCHOOL_IMMUNITY
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
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
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// SPELL_AURA_MECHANIC_IMMUNITY
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MECHANIC_IMMUNITY(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        checked
        {
            switch (Action)
            {
                case AuraAction.AURA_UPDATE:
                    break;

                case AuraAction.AURA_ADD:
                    Target.MechanicImmunity = (uint)(Target.MechanicImmunity & ~(1 << EffectInfo.MiscValue));
                    Target.RemoveAurasByMechanic(EffectInfo.MiscValue);
                    break;

                case AuraAction.AURA_REMOVE:
                case AuraAction.AURA_REMOVEBYDURATION:
                    Target.MechanicImmunity = (uint)(Target.MechanicImmunity & ~(1 << EffectInfo.MiscValue));
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// SPELL_AURA_DISPEL_IMMUNITY
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_DISPEL_IMMUNITY(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        checked
        {
            switch (Action)
            {
                case AuraAction.AURA_UPDATE:
                    break;

                case AuraAction.AURA_ADD:
                    Target.DispellImmunity = (uint)(Target.DispellImmunity & ~(1 << EffectInfo.MiscValue));
                    break;

                case AuraAction.AURA_REMOVE:
                case AuraAction.AURA_REMOVEBYDURATION:
                    Target.DispellImmunity = (uint)(Target.DispellImmunity & ~(1 << EffectInfo.MiscValue));
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// SPELL_AURA_TRACK_CREATURES
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
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
            default:
                break;
        }
    }

    /// <summary>
    /// SPELL_AURA_TRACK_RESOURCES
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
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
            default:
                break;
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_SCALE
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_SCALE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        switch (Action)
        {
            case AuraAction.AURA_UPDATE:
                return;

            case AuraAction.AURA_ADD:
                {
                    ref var size2 = ref Target.Size;
                    WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                    size2 = (float)(size2 * ((EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0) + 1.0));
                    break;
                }
            case AuraAction.AURA_REMOVE:
            case AuraAction.AURA_REMOVEBYDURATION:
                {
                    ref var size = ref Target.Size;
                    WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                    size = (float)(size / ((EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0) + 1.0));
                    break;
                }

            default:
                break;
        }
        if (Target is WS_PlayerData.CharacterObject @object)
        {
            @object.SetUpdateFlag(4, Target.Size);
            @object.SendCharacterUpdate();
            return;
        }
        Packets.UpdatePacketClass packet = new();
        Packets.UpdateClass tmpUpdate = new(6);
        tmpUpdate.SetUpdateFlag(4, Target.Size);
        Packets.PacketClass packet2 = packet;
        WS_Creatures.CreatureObject updateObject = (WS_Creatures.CreatureObject)Target;
        tmpUpdate.AddToPacket(ref packet2, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
        packet = (Packets.UpdatePacketClass)packet2;
        packet2 = packet;
        var obj = Target;
        obj.SendToNearPlayers(ref packet2);
        packet = (Packets.UpdatePacketClass)packet2;
        tmpUpdate.Dispose();
        packet.Dispose();
    }

    /// <summary>
    /// SPELL_AURA_MOD_SKILL
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
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
                        var characterObject2 = @object;
                        ref var bonus2 = ref characterObject2.Skills[EffectInfo.MiscValue].Bonus;
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        bonus2 = (short)(bonus2 + EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0));
                        characterObject2.SetUpdateFlag(718 + (characterObject2.SkillsPositions[EffectInfo.MiscValue] * 3) + 2, characterObject2.Skills[EffectInfo.MiscValue].Bonus);
                        characterObject2.SendCharacterUpdate();
                    }
                    break;

                case AuraAction.AURA_REMOVE:
                case AuraAction.AURA_REMOVEBYDURATION:
                    if (Target is WS_PlayerData.CharacterObject object1 && object1.Skills.ContainsKey(EffectInfo.MiscValue))
                    {
                        var characterObject = object1;
                        ref var bonus = ref characterObject.Skills[EffectInfo.MiscValue].Bonus;
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        bonus = (short)(bonus - EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0));
                        characterObject.SetUpdateFlag(718 + (characterObject.SkillsPositions[EffectInfo.MiscValue] * 3) + 2, characterObject.Skills[EffectInfo.MiscValue].Bonus);
                        characterObject.SendCharacterUpdate();
                    }
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// SPELL_AURA_PERIODIC_DUMMY
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
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
                                    WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                                    var Damage = EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) * StackCount;
                                    @object.ManaRegenBonus += Damage;
                                    @object.UpdateManaRegen();
                                    break;
                                }

                            default:
                                break;
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
                                    WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                                    var Damage2 = EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) * StackCount;
                                    WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
                                    target.ManaRegenBonus -= Damage2;
                                    target.UpdateManaRegen();
                                    break;
                                }
                        }
                    }
                    break;

                case AuraAction.AURA_UPDATE:
                    if (SpellID == 43265 || unchecked((uint)(SpellID - 49936)) <= 2u)
                    {
                        int Damage3;
                        if (Caster is WS_DynamicObjects.DynamicObject object1)
                        {
                            Damage3 = EffectInfo.GetValue(object1.Caster.Level, 0) * StackCount;
                            Target.DealDamage(Damage3, object1.Caster);
                            break;
                        }

                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        Damage3 = EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) * StackCount;
                        var obj = Target;
                        var damage = Damage3;
                        var Attacker = caster;
                        obj.DealDamage(damage, Attacker);
                    }
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// SPELL_AURA_PERIODIC_DAMAGE
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
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
                    if (Caster is WS_DynamicObjects.DynamicObject @object)
                    {
                        int Damage2;
                        checked
                        {
                            Damage2 = EffectInfo.GetValue(@object.Caster.Level, 0) * StackCount;
                        }
                        Target.DealSpellDamage(ref @object.Caster, ref EffectInfo, SpellID, Damage2, (DamageTypes)checked((byte)SPELLs[SpellID].School), SpellType.SPELL_TYPE_DOT);
                        break;
                    }
                    int Damage;
                    WS_Base.BaseUnit obj;
                    WS_Base.BaseUnit Caster2;
                    checked
                    {
                        Damage = EffectInfo.GetValue(Level: ((WS_Base.BaseUnit)Caster).Level, ComboPoints: 0) * StackCount;
                        obj = Target;
                        Caster2 = (WS_Base.BaseUnit)Caster;
                    }
                    obj.DealSpellDamage(ref Caster2, ref EffectInfo, SpellID, Damage, (DamageTypes)checked((byte)SPELLs[SpellID].School), SpellType.SPELL_TYPE_DOT);
                    Caster = Caster2;
                    break;
                }

            default:
                break;
        }
    }

    /// <summary>
    /// SPELL_AURA_PERIODIC_HEAL
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
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
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        Damage = EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) * StackCount;
                        obj = Target;
                        Caster2 = caster;
                    }
                    obj.DealSpellDamage(ref Caster2, ref EffectInfo, SpellID, Damage, (DamageTypes)checked((byte)SPELLs[SpellID].School), SpellType.SPELL_TYPE_HEALDOT);
                    Caster = Caster2;
                    break;
                }

            default:
                break;
        }
    }

    /// <summary>
    /// SPELL_AURA_PERIODIC_ENERGIZE
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
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
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        Damage = EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) * StackCount;
                        Caster2 = caster;
                    }
                    SendPeriodicAuraLog(ref Caster2, ref Target, SpellID, (int)Power, Damage, EffectInfo.ApplyAuraIndex);
                    var obj = Target;
                    Caster2 = (WS_Base.BaseUnit)Caster;
                    obj.Energize(Damage, Power, Caster2);
                    break;
                }
        }
    }

    /// <summary>
    /// SPELL_AURA_PERIODIC_LEECH
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
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
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        var Damage = EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) * StackCount;
                        var Caster2 = caster;
                        SendPeriodicAuraLog(ref Caster2, ref Target, SpellID, SPELLs[SpellID].School, Damage, EffectInfo.ApplyAuraIndex);
                        Caster = Caster2;
                        Caster2 = caster;
                        SendPeriodicAuraLog(ref Target, ref Caster2, SpellID, SPELLs[SpellID].School, Damage, EffectInfo.ApplyAuraIndex);
                        Caster = Caster2;
                        var obj = Target;
                        Caster2 = caster;
                        obj.DealDamage(Damage, Caster2);
                        Caster = Caster2;
                        caster.Heal(Damage, Target);
                        break;
                    }

                default:
                    break;
            }
        }
    }

    /// <summary>
    /// SPELL_AURA_PERIODIC_MANA_LEECH
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
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
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        var Damage = EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) * StackCount;
                        var Target2 = caster;
                        SendPeriodicAuraLog(ref Target, ref Target2, SpellID, (int)Power, Damage, EffectInfo.ApplyAuraIndex);
                        Caster = Target2;
                        var obj = Target;
                        var damage = -Damage;
                        Target2 = caster;
                        obj.Energize(damage, Power, Target2);
                        Caster = Target2;
                        caster.Energize(Damage, Power, Target);
                        break;
                    }
                }

            default:
                break;
        }
    }

    /// <summary>
    /// SPELL_AURA_PERIODIC_TRIGGER_SPELL
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
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
                    SpellTargets Targets = new();
                    Targets.SetTarget_UNIT(ref Target);
                    CastSpellParameters castParams = new(ref Targets, ref Caster, EffectInfo.TriggerSpell);
                    ThreadPool.QueueUserWorkItem(castParams.Cast);
                    if (Caster is WS_Base.BaseUnit Caster2)
                    {
                        SendPeriodicAuraLog(ref Caster2, ref Target, SpellID, SPELLs[SpellID].School, 0, EffectInfo.ApplyAuraIndex);
                        Caster = Caster2;
                    }
                    break;
                }

            default:
                break;
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
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        Damage = (int)Math.Round(Target.Life.Maximum * EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0 * StackCount);
                        obj = Target;
                        Caster2 = caster;
                    }
                    obj.DealSpellDamage(ref Caster2, ref EffectInfo, SpellID, Damage, (DamageTypes)checked((byte)SPELLs[SpellID].School), SpellType.SPELL_TYPE_DOT);
                    Caster = Caster2;
                    break;
                }

            default:
                break;
        }
    }

    /// <summary>
    /// SPELL_AURA_PERIODIC_HEAL_PERCENT
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
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
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        Damage = (int)Math.Round(Target.Life.Maximum * EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0 * StackCount);
                        obj = Target;
                        Caster2 = caster;
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
            default:
                break;
        }
    }

    /// <summary>
    /// SPELL_AURA_PERIODIC_ENERGIZE_PERCENT
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
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
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        Damage = (int)Math.Round(Target.Mana.Maximum * EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0 * StackCount);
                        Caster2 = caster;
                    }
                    SendPeriodicAuraLog(ref Caster2, ref Target, SpellID, (int)Power, Damage, EffectInfo.ApplyAuraIndex);
                    Caster = Caster2;
                    var obj = Target;
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
            default:
                break;
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_REGEN
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_REGEN(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        if (Target is not WS_PlayerData.CharacterObject)
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
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        var Damage2 = EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) * StackCount;
                        WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
                        target.LifeRegenBonus += Damage2;
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
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        var Damage = EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) * StackCount;
                        WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
                        target.LifeRegenBonus -= Damage;
                        break;
                    }
            }
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_POWER_REGEN
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_POWER_REGEN(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        if (Target is not WS_PlayerData.CharacterObject)
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
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        var Damage = EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) * StackCount;
                        if (EffectInfo.MiscValue == 0)
                        {
                            WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
                            target.ManaRegenBonus += Damage;
                            target.UpdateManaRegen();
                        }
                        else if (EffectInfo.MiscValue == 1)
                        {
                            WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
                            ref var rageRegenBonus2 = ref target.RageRegenBonus;
                            rageRegenBonus2 = (int)Math.Round(rageRegenBonus2 + (Damage / 17.0 * 10.0));
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
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        var Damage2 = EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) * StackCount;
                        if (EffectInfo.MiscValue == 0)
                        {
                            WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
                            target.ManaRegenBonus -= Damage2;
                            target.UpdateManaRegen();
                        }
                        else if (EffectInfo.MiscValue == 1)
                        {
                            WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
                            ref var rageRegenBonus = ref target.RageRegenBonus;
                            rageRegenBonus = (int)Math.Round(rageRegenBonus - (Damage2 / 17.0 * 10.0));
                        }
                        break;
                    }

                default:
                    break;
            }
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_POWER_REGEN_PERCENT
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_POWER_REGEN_PERCENT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        if (Target is not WS_PlayerData.CharacterObject)
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
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        var Damage2 = (int)Math.Round(EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) * StackCount / 100.0);
                        WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
                        target.ManaRegenerationModifier += Damage2;
                        target.UpdateManaRegen();
                    }
                    break;

                case AuraAction.AURA_REMOVE:
                case AuraAction.AURA_REMOVEBYDURATION:
                    {
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        var Damage = (int)Math.Round(EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) * StackCount / 100.0);
                        WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
                        target.ManaRegenerationModifier -= Damage;
                        target.UpdateManaRegen();
                        break;
                    }

                default:
                    break;
            }
        }
    }

    /// <summary>
    /// SPELL_AURA_TRANSFORM
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_TRANSFORM(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        WS_Creatures.CreatureObject target = (WS_Creatures.CreatureObject)Target;
        switch (Action)
        {
            case AuraAction.AURA_ADD:
                if (!WorldServiceLocator._WorldServer.CREATURESDatabase.ContainsKey(EffectInfo.MiscValue))
                {
                    CreatureInfo creature = new(EffectInfo.MiscValue);
                    WorldServiceLocator._WorldServer.CREATURESDatabase.Add(EffectInfo.MiscValue, creature);
                }
                Target.Model = WorldServiceLocator._WorldServer.CREATURESDatabase[EffectInfo.MiscValue].GetFirstModel;
                break;

            case AuraAction.AURA_REMOVE:
            case AuraAction.AURA_REMOVEBYDURATION:
                Target.Model = Target is WS_PlayerData.CharacterObject @object
                    ? WorldServiceLocator._Functions.GetRaceModel(@object.Race, (int)@object.Gender)
                    : target.CreatureInfo.GetRandomModel;
                break;

            case AuraAction.AURA_UPDATE:
                return;
            default:
                break;
        }
        if (Target is WS_PlayerData.CharacterObject object1)
        {
            object1.SetUpdateFlag(131, Target.Model);
            object1.SendCharacterUpdate();
            return;
        }
        Packets.UpdateClass tmpUpdate = new(188);
        tmpUpdate.SetUpdateFlag(131, Target.Model);
        Packets.UpdatePacketClass packet = new();
        Packets.PacketClass packet2 = packet;
        var updateObject = target;
        tmpUpdate.AddToPacket(ref packet2, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
        packet = (Packets.UpdatePacketClass)packet2;
        packet2 = packet;
        var obj = Target;
        obj.SendToNearPlayers(ref packet2);
        packet = (Packets.UpdatePacketClass)packet2;
        tmpUpdate.Dispose();
        packet.Dispose();
    }

    /// <summary>
    /// SPELL_AURA_GHOST
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
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
                        var wS_CharMovement2 = WorldServiceLocator._WS_CharMovement;
                        var Character = @object;
                        wS_CharMovement2.UpdateCell(ref Character);
                        Target = Character;
                        break;
                    }
                case AuraAction.AURA_REMOVE:
                case AuraAction.AURA_REMOVEBYDURATION:
                    {
                        Target.Invisibility = InvisibilityLevel.VISIBLE;
                        Target.CanSeeInvisibility = InvisibilityLevel.INIVISIBILITY;
                        var wS_CharMovement = WorldServiceLocator._WS_CharMovement;
                        var Character = @object;
                        wS_CharMovement.UpdateCell(ref Character);
                        Target = Character;
                        break;
                    }

                default:
                    break;
            }
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_INVISIBILITY
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_INVISIBILITY(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        checked
        {
            WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
            WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
            switch (Action)
            {
                case AuraAction.AURA_UPDATE:
                    return;

                case AuraAction.AURA_ADD:
                    target.cPlayerFieldBytes2 |= 0x4000;
                    Target.Invisibility = InvisibilityLevel.INIVISIBILITY;
                    Target.Invisibility_Value += EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0);
                    break;

                case AuraAction.AURA_REMOVE:
                case AuraAction.AURA_REMOVEBYDURATION:
                    target.cPlayerFieldBytes2 &= -16385;
                    Target.Invisibility = InvisibilityLevel.VISIBLE;
                    Target.Invisibility_Value -= EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0);
                    break;
                default:
                    break;
            }
            if (Target is WS_PlayerData.CharacterObject @object)
            {
                @object.SetUpdateFlag(1260, @object.cPlayerFieldBytes2);
                @object.SendCharacterUpdate();
                var wS_CharMovement = WorldServiceLocator._WS_CharMovement;
                var Character = @object;
                wS_CharMovement.UpdateCell(ref Character);
            }
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_STEALTH
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_STEALTH(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        checked
        {
            WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
            switch (Action)
            {
                case AuraAction.AURA_UPDATE:
                    return;

                case AuraAction.AURA_ADD:
                    Target.Invisibility = InvisibilityLevel.STEALTH;
                    Target.Invisibility_Value += EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0);
                    break;

                case AuraAction.AURA_REMOVE:
                case AuraAction.AURA_REMOVEBYDURATION:
                    Target.Invisibility = InvisibilityLevel.VISIBLE;
                    Target.Invisibility_Value -= EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0);
                    break;
                default:
                    break;
            }
            var wS_CharMovement = WorldServiceLocator._WS_CharMovement;
            WS_PlayerData.CharacterObject Character = (WS_PlayerData.CharacterObject)Target;
            wS_CharMovement.UpdateCell(ref Character);
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_STEALTH_LEVEL
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_STEALTH_LEVEL(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        checked
        {
            WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
            switch (Action)
            {
                case AuraAction.AURA_UPDATE:
                    break;

                case AuraAction.AURA_ADD:
                    Target.Invisibility_Bonus += EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0);
                    break;

                case AuraAction.AURA_REMOVE:
                case AuraAction.AURA_REMOVEBYDURATION:
                    Target.Invisibility_Bonus -= EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0);
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_INVISIBILITY_DETECTION
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_INVISIBILITY_DETECTION(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        checked
        {
            WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
            switch (Action)
            {
                case AuraAction.AURA_UPDATE:
                    return;

                case AuraAction.AURA_ADD:
                    Target.CanSeeInvisibility_Invisibility += EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0);
                    break;

                case AuraAction.AURA_REMOVE:
                case AuraAction.AURA_REMOVEBYDURATION:
                    Target.CanSeeInvisibility_Invisibility -= EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0);
                    break;
                default:
                    break;
            }
            if (Target is WS_PlayerData.CharacterObject @object)
            {
                var wS_CharMovement = WorldServiceLocator._WS_CharMovement;
                var Character = @object;
                wS_CharMovement.UpdateCell(ref Character);
            }
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_DETECT
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_DETECT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        checked
        {
            WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
            switch (Action)
            {
                case AuraAction.AURA_UPDATE:
                    return;

                case AuraAction.AURA_ADD:
                    Target.CanSeeInvisibility_Stealth += EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0);
                    break;

                case AuraAction.AURA_REMOVE:
                case AuraAction.AURA_REMOVEBYDURATION:
                    Target.CanSeeInvisibility_Stealth -= EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0);
                    break;
                default:
                    break;
            }
            if (Target is WS_PlayerData.CharacterObject @object)
            {
                var wS_CharMovement = WorldServiceLocator._WS_CharMovement;
                var Character = @object;
                wS_CharMovement.UpdateCell(ref Character);
            }
        }
    }

    /// <summary>
    /// SPELL_AURA_DETECT_STEALTH
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
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
            default:
                break;
        }
        if (Target is WS_PlayerData.CharacterObject @object)
        {
            var wS_CharMovement = WorldServiceLocator._WS_CharMovement;
            var Character = @object;
            wS_CharMovement.UpdateCell(ref Character);
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_DISARM
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_DISARM(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        if (Target is not WS_PlayerData.CharacterObject)
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
            default:
                break;
        }
    }

    /// <summary>
    /// SPELL_AURA_SCHOOL_ABSORB
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_SCHOOL_ABSORB(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        if (Caster is not WS_Base.BaseUnit)
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
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        Target.AbsorbSpellLeft.Add(SpellID, (uint)EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) + ((uint)EffectInfo.MiscValue << 23));
                    }
                    break;

                case AuraAction.AURA_REMOVE:
                case AuraAction.AURA_REMOVEBYDURATION:
                    if (Target.AbsorbSpellLeft.ContainsKey(SpellID))
                    {
                        Target.AbsorbSpellLeft.Remove(SpellID);
                    }
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_SHAPESHIFT
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_SHAPESHIFT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        switch (Action)
        {
            case AuraAction.AURA_ADD:
                {
                    Target.RemoveAurasOfType(AuraEffects_Names.SPELL_AURA_MOD_SHAPESHIFT, SpellID);
                    var obj = Target;
                    var NotSpellID = 0;
                    obj.RemoveAurasOfType(AuraEffects_Names.SPELL_AURA_MOUNTED, NotSpellID);
                    if (Target is WS_PlayerData.CharacterObject object1 && object1.Classe == Classes.CLASS_DRUID && (EffectInfo.MiscValue == 4 || EffectInfo.MiscValue == 1 || EffectInfo.MiscValue == 5 || EffectInfo.MiscValue == 8 || EffectInfo.MiscValue == 3 || EffectInfo.MiscValue == 29 || EffectInfo.MiscValue == 27 || EffectInfo.MiscValue == 31))
                    {
                        var obj2 = Target;
                        NotSpellID = 0;
                        obj2.RemoveAurasOfType(AuraEffects_Names.SPELL_AURA_MOD_ROOT, NotSpellID);
                        var obj3 = Target;
                        NotSpellID = 0;
                        obj3.RemoveAurasOfType(AuraEffects_Names.SPELL_AURA_MOD_DECREASE_SPEED, NotSpellID);
                    }
                    Target.ShapeshiftForm = (ShapeshiftForm)checked((byte)EffectInfo.MiscValue);
                    Target.ManaType = WorldServiceLocator._CommonGlobalFunctions.GetShapeshiftManaType((ShapeshiftForm)checked((byte)EffectInfo.MiscValue), Target.ManaType);
                    Target.Model = Target switch
                    {
                        WS_PlayerData.CharacterObject => WorldServiceLocator._CommonGlobalFunctions.GetShapeshiftModel((ShapeshiftForm)checked((byte)EffectInfo.MiscValue), ((WS_PlayerData.CharacterObject)Target).Race, Target.Model),
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
                    WS_Creatures.CreatureObject target = (WS_Creatures.CreatureObject)Target;
                    Target.ManaType = (ManaTypes)target.CreatureInfo.ManaType;
                    Target.Model = target.CreatureInfo.GetRandomModel;
                }
                break;

            case AuraAction.AURA_UPDATE:
                return;
            default:
                break;
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
            var wS_Combat = WorldServiceLocator._WS_Combat;
            WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
            var objCharacter = target;
            wS_Combat.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.BASE_ATTACK);
            characterObject.SetUpdateFlag(134, characterObject.Damage.Minimum);
            characterObject.SetUpdateFlag(135, characterObject.Damage.Maximum);
            characterObject.SendCharacterUpdate();
            characterObject.GroupUpdateFlag |= 8u;
            characterObject.GroupUpdateFlag |= 16u;
            characterObject.GroupUpdateFlag |= 32u;
            WorldServiceLocator._WS_PlayerHelper.InitializeTalentSpells(target);
        }
        else
        {
            Packets.UpdatePacketClass packet = new();
            Packets.UpdateClass tmpUpdate = new(188);
            tmpUpdate.SetUpdateFlag(164, Target.cBytes2);
            tmpUpdate.SetUpdateFlag(131, Target.Model);
            Packets.PacketClass packet2 = packet;
            WS_Creatures.CreatureObject updateObject = (WS_Creatures.CreatureObject)Target;
            tmpUpdate.AddToPacket(ref packet2, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
            packet = (Packets.UpdatePacketClass)packet2;
            var obj4 = Target;
            packet2 = packet;
            obj4.SendToNearPlayers(ref packet2);
            packet = (Packets.UpdatePacketClass)packet2;
            tmpUpdate.Dispose();
            packet.Dispose();
        }
        if (Target is not WS_PlayerData.CharacterObject)
        {
            return;
        }
        if (Action == AuraAction.AURA_ADD)
        {
            if (EffectInfo.MiscValue == 3)
            {
                WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
                target.ApplySpell(5419);
            }
            if (EffectInfo.MiscValue == 1)
            {
                WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
                target.ApplySpell(3025);
            }
            if (EffectInfo.MiscValue == 5)
            {
                WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
                target.ApplySpell(1178);
            }
            if (EffectInfo.MiscValue == 8)
            {
                WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
                target.ApplySpell(9635);
            }
            if (EffectInfo.MiscValue == 4)
            {
                WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
                target.ApplySpell(5421);
            }
            if (EffectInfo.MiscValue == 31)
            {
                WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
                target.ApplySpell(24905);
            }
            if (EffectInfo.MiscValue == 29)
            {
                WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
                target.ApplySpell(33948);
                target.ApplySpell(34764);
            }
            if (EffectInfo.MiscValue == 27)
            {
                WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
                target.ApplySpell(40121);
                target.ApplySpell(40122);
            }
            if (EffectInfo.MiscValue == 17)
            {
                WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
                target.ApplySpell(21156);
            }
            if (EffectInfo.MiscValue == 19)
            {
                WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
                target.ApplySpell(7381);
            }
            if (EffectInfo.MiscValue == 18)
            {
                WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
                target.ApplySpell(7376);
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

    /// <summary>
    /// SPELL_AURA_PROC_TRIGGER_SPELL
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
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
            default:
                break;
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_INCREASE_SPEED
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_INCREASE_SPEED(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        switch (Action)
        {
            case AuraAction.AURA_ADD:
                if (Target is WS_PlayerData.CharacterObject @object)
                {
                    var newSpeed = @object.RunSpeed;
                    WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                    newSpeed = (float)(newSpeed * ((EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0) + 1.0));
                    @object.ChangeSpeedForced(ChangeSpeedType.RUN, newSpeed);
                }
                else if (Target is WS_Creatures.CreatureObject object1)
                {
                    object1.SetToRealPosition();
                    ref var speedMod2 = ref object1.SpeedMod;
                    WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                    speedMod2 = (float)(speedMod2 * ((EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0) + 1.0));
                }
                break;

            case AuraAction.AURA_REMOVE:
            case AuraAction.AURA_REMOVEBYDURATION:
                if (Target is WS_PlayerData.CharacterObject object2)
                {
                    var newSpeed2 = object2.RunSpeed;
                    if (Caster != null)
                    {
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        newSpeed2 = (float)(newSpeed2 / ((EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0) + 1.0));
                    }
                    object2.ChangeSpeedForced(ChangeSpeedType.RUN, newSpeed2);
                }
                else if (Target is WS_Creatures.CreatureObject object1)
                {
                    object1.SetToRealPosition();
                    ref var speedMod = ref object1.SpeedMod;
                    WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                    speedMod = (float)(speedMod / ((EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0) + 1.0));
                }
                break;

            case AuraAction.AURA_UPDATE:
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_DECREASE_SPEED
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_DECREASE_SPEED(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        switch (Action)
        {
            case AuraAction.AURA_ADD:
                if (Target is WS_PlayerData.CharacterObject @object)
                {
                    var newSpeed = @object.RunSpeed;
                    WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                    newSpeed = (EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) >= 0) ? ((float)(newSpeed / ((EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0) + 1.0))) : ((float)(newSpeed / (Math.Abs(EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0) + 1.0)));
                    @object.ChangeSpeedForced(ChangeSpeedType.RUN, newSpeed);
                    @object.RemoveAurasByInterruptFlag(128);
                }
                else if (Target is WS_Creatures.CreatureObject object1)
                {
                    object1.SetToRealPosition();
                    WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                    switch (EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0))
                    {
                        case < 0:
                            {
                                ref var speedMod3 = ref object1.SpeedMod;
                                speedMod3 = (float)(speedMod3 / (Math.Abs(EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0) + 1.0));
                                break;
                            }

                        default:
                            {
                                ref var speedMod4 = ref object1.SpeedMod;
                                speedMod4 = (float)(speedMod4 / ((EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0) + 1.0));
                                break;
                            }
                    }
                }
                break;

            case AuraAction.AURA_REMOVE:
            case AuraAction.AURA_REMOVEBYDURATION:
                if (Target is WS_PlayerData.CharacterObject object2)
                {
                    var newSpeed2 = object2.RunSpeed;
                    WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                    newSpeed2 = (EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) >= 0) ? ((float)(newSpeed2 * ((EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0) + 1.0))) : ((float)(newSpeed2 * (Math.Abs(EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0) + 1.0)));
                    object2.ChangeSpeedForced(ChangeSpeedType.RUN, newSpeed2);
                }
                else if (Target is WS_Creatures.CreatureObject object1)
                {
                    object1.SetToRealPosition();
                    WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                    switch (EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0))
                    {
                        case < 0:
                            {
                                ref var speedMod = ref object1.SpeedMod;
                                speedMod = (float)(speedMod * (Math.Abs(EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0) + 1.0));
                                break;
                            }

                        default:
                            {
                                ref var speedMod2 = ref object1.SpeedMod;
                                speedMod2 = (float)(speedMod2 * ((EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0) + 1.0));
                                break;
                            }
                    }
                }
                break;

            case AuraAction.AURA_UPDATE:
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_INCREASE_SPEED_ALWAYS
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_INCREASE_SPEED_ALWAYS(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        if (Target is WS_PlayerData.CharacterObject @object)
        {
            switch (Action)
            {
                case AuraAction.AURA_ADD:
                    {
                        var newSpeed = @object.RunSpeed;
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        newSpeed = (float)(newSpeed * ((EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0) + 1.0));
                        @object.ChangeSpeedForced(ChangeSpeedType.RUN, newSpeed);
                        break;
                    }
                case AuraAction.AURA_REMOVE:
                case AuraAction.AURA_REMOVEBYDURATION:
                    {
                        var newSpeed2 = @object.RunSpeed;
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        newSpeed2 = (float)(newSpeed2 / ((EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0) + 1.0));
                        @object.ChangeSpeedForced(ChangeSpeedType.RUN, newSpeed2);
                        break;
                    }
                case AuraAction.AURA_UPDATE:
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_INCREASE_MOUNTED_SPEED
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_INCREASE_MOUNTED_SPEED(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        if (Target is WS_PlayerData.CharacterObject @object)
        {
            switch (Action)
            {
                case AuraAction.AURA_ADD:
                    {
                        var newSpeed = @object.RunSpeed;
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        newSpeed = (float)(newSpeed * ((EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0) + 1.0));
                        @object.ChangeSpeedForced(ChangeSpeedType.RUN, newSpeed);
                        break;
                    }
                case AuraAction.AURA_REMOVE:
                case AuraAction.AURA_REMOVEBYDURATION:
                    {
                        var newSpeed2 = @object.RunSpeed;
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        newSpeed2 = (float)(newSpeed2 / ((EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0) + 1.0));
                        @object.ChangeSpeedForced(ChangeSpeedType.RUN, newSpeed2);
                        break;
                    }
                case AuraAction.AURA_UPDATE:
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_INCREASE_MOUNTED_SPEED_ALWAYS
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_INCREASE_MOUNTED_SPEED_ALWAYS(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        if (Target is WS_PlayerData.CharacterObject @object)
        {
            switch (Action)
            {
                case AuraAction.AURA_ADD:
                    {
                        var newSpeed = @object.RunSpeed;
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        newSpeed = (float)(newSpeed * ((EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0) + 1.0));
                        @object.ChangeSpeedForced(ChangeSpeedType.RUN, newSpeed);
                        break;
                    }
                case AuraAction.AURA_REMOVE:
                case AuraAction.AURA_REMOVEBYDURATION:
                    {
                        var newSpeed2 = @object.RunSpeed;
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        newSpeed2 = (float)(newSpeed2 / ((EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0) + 1.0));
                        @object.ChangeSpeedForced(ChangeSpeedType.RUN, newSpeed2);
                        break;
                    }
                case AuraAction.AURA_UPDATE:
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_INCREASE_SWIM_SPEED
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_INCREASE_SWIM_SPEED(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        if (Target is not WS_PlayerData.CharacterObject)
        {
            return;
        }
        switch (Action)
        {
            case AuraAction.AURA_ADD:
                {
                    WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
                    var newSpeed = target.SwimSpeed;
                    WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                    newSpeed = (float)(newSpeed * ((EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0) + 1.0));
                    target.SwimSpeed = newSpeed;
                    target.ChangeSpeedForced(ChangeSpeedType.SWIM, newSpeed);
                    break;
                }
            case AuraAction.AURA_REMOVE:
            case AuraAction.AURA_REMOVEBYDURATION:
                {
                    WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
                    var newSpeed2 = target.SwimSpeed;
                    if (Caster != null)
                    {
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        newSpeed2 = (float)(newSpeed2 / ((EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0) + 1.0));
                    }
                    target.SwimSpeed = newSpeed2;
                    target.ChangeSpeedForced(ChangeSpeedType.SWIM, newSpeed2);
                    break;
                }
            case AuraAction.AURA_UPDATE:
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// SPELL_AURA_MOUNTED
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOUNTED(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        switch (Action)
        {
            case AuraAction.AURA_ADD:
                {
                    var obj = Target;
                    var NotSpellID = 0;
                    obj.RemoveAurasOfType(AuraEffects_Names.SPELL_AURA_MOD_SHAPESHIFT, NotSpellID);
                    Target.RemoveAurasOfType(AuraEffects_Names.SPELL_AURA_MOUNTED, SpellID);
                    Target.RemoveAurasByInterruptFlag(131072);
                    if (!WorldServiceLocator._WorldServer.CREATURESDatabase.ContainsKey(EffectInfo.MiscValue))
                    {
                        CreatureInfo creature = new(EffectInfo.MiscValue);
                        WorldServiceLocator._WorldServer.CREATURESDatabase.Add(EffectInfo.MiscValue, creature);
                    }
                    Target.Mount = WorldServiceLocator._WorldServer.CREATURESDatabase.ContainsKey(EffectInfo.MiscValue)
                        ? WorldServiceLocator._WorldServer.CREATURESDatabase[EffectInfo.MiscValue].GetFirstModel
                        : 0;
                    break;
                }
            case AuraAction.AURA_REMOVE:
            case AuraAction.AURA_REMOVEBYDURATION:
                Target.Mount = 0;
                Target.RemoveAurasByInterruptFlag(64);
                break;

            case AuraAction.AURA_UPDATE:
                return;
            default:
                break;
        }
        if (Target is WS_PlayerData.CharacterObject @object)
        {
            @object.SetUpdateFlag(133, Target.Mount);
            @object.SendCharacterUpdate();
            return;
        }
        Packets.UpdatePacketClass packet = new();
        Packets.UpdateClass tmpUpdate = new(188);
        tmpUpdate.SetUpdateFlag(133, Target.Mount);
        Packets.PacketClass packet2 = packet;
        WS_Creatures.CreatureObject updateObject = (WS_Creatures.CreatureObject)Target;
        tmpUpdate.AddToPacket(ref packet2, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
        packet = (Packets.UpdatePacketClass)packet2;
        var obj2 = Target;
        packet2 = packet;
        obj2.SendToNearPlayers(ref packet2);
        packet = (Packets.UpdatePacketClass)packet2;
        tmpUpdate.Dispose();
        packet.Dispose();
    }

    /// <summary>
    /// SPELL_AURA_MOD_HASTE
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_HASTE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        if (Target is WS_PlayerData.CharacterObject @object)
        {
            using var characterObject = @object;
            switch (Action)
            {
                case AuraAction.AURA_ADD:
                    {
                        ref var reference3 = ref characterObject.AttackTimeMods[0];
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        reference3 = (float)(reference3 / ((EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0) + 1.0));
                        ref var reference4 = ref characterObject.AttackTimeMods[1];
                        reference4 = (float)(reference4 / ((EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0) + 1.0));
                        break;
                    }
                case AuraAction.AURA_REMOVE:
                case AuraAction.AURA_REMOVEBYDURATION:
                    {
                        ref var reference = ref characterObject.AttackTimeMods[0];
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        reference = (float)(reference * ((EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0) + 1.0));
                        ref var reference2 = ref characterObject.AttackTimeMods[1];
                        reference2 = (float)(reference2 * ((EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0) + 1.0));
                        break;
                    }
                case AuraAction.AURA_UPDATE:
                    return;
                default:
                    break;
            }
            characterObject.SetUpdateFlag(126, characterObject.GetAttackTime(WeaponAttackType.BASE_ATTACK));
            characterObject.SetUpdateFlag(128, characterObject.GetAttackTime(WeaponAttackType.OFF_ATTACK));
            characterObject.SendCharacterUpdate(toNear: false);
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_RANGE_HASTE
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_RANGED_HASTE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        if (Target is WS_PlayerData.CharacterObject @object)
        {
            using var characterObject = @object;
            switch (Action)
            {
                case AuraAction.AURA_ADD:
                    {
                        ref var reference2 = ref characterObject.AttackTimeMods[2];
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        reference2 = (float)(reference2 / ((EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0) + 1.0));
                        break;
                    }
                case AuraAction.AURA_REMOVE:
                case AuraAction.AURA_REMOVEBYDURATION:
                    {
                        ref var reference = ref characterObject.AttackTimeMods[2];
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        reference = (float)(reference * ((EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0) + 1.0));
                        break;
                    }
                case AuraAction.AURA_UPDATE:
                    return;
                default:
                    break;
            }
            characterObject.SetUpdateFlag(128, characterObject.GetAttackTime(WeaponAttackType.RANGED_ATTACK));
            characterObject.SendCharacterUpdate(toNear: false);
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_RANGED_AMMO_HASTE
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_RANGED_AMMO_HASTE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        if (Target is WS_PlayerData.CharacterObject @object)
        {
            using var characterObject = @object;
            switch (Action)
            {
                case AuraAction.AURA_ADD:
                    {
                        ref var ammoMod2 = ref characterObject.AmmoMod;
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        ammoMod2 = (float)(ammoMod2 * ((EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0) + 1.0));
                        break;
                    }
                case AuraAction.AURA_REMOVE:
                case AuraAction.AURA_REMOVEBYDURATION:
                    {
                        ref var ammoMod = ref characterObject.AmmoMod;
                        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                        ammoMod = (float)(ammoMod / ((EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0) + 1.0));
                        break;
                    }
                case AuraAction.AURA_UPDATE:
                    return;
            }
            var wS_Combat = WorldServiceLocator._WS_Combat;
            var objCharacter = @object;
            wS_Combat.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.RANGED_ATTACK);
            characterObject.SendCharacterUpdate(toNear: false);
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_ROOT
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
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
                        var aiScript = object1.aiScript;
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
            default:
                break;
        }
        if (Target is WS_PlayerData.CharacterObject object4)
        {
            object4.SetUpdateFlag(46, Target.cUnitFlags);
            object4.SendCharacterUpdate();
            return;
        }
        Packets.UpdatePacketClass packet = new();
        Packets.UpdateClass tmpUpdate = new(188);
        tmpUpdate.SetUpdateFlag(46, Target.cUnitFlags);
        Packets.PacketClass packet2 = packet;
        WS_Creatures.CreatureObject updateObject = (WS_Creatures.CreatureObject)Target;
        tmpUpdate.AddToPacket(ref packet2, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
        packet = (Packets.UpdatePacketClass)packet2;
        var obj = Target;
        packet2 = packet;
        obj.SendToNearPlayers(ref packet2);
        packet = (Packets.UpdatePacketClass)packet2;
        tmpUpdate.Dispose();
        packet.Dispose();
    }

    /// <summary>
    /// SPELL_AURA_MOD_STUN
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
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
                        var aiScript = object1.aiScript;
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
            default:
                break;
        }
        if (Target is WS_PlayerData.CharacterObject object3)
        {
            object3.SetUpdateFlag(46, Target.cUnitFlags);
            object3.SendCharacterUpdate();
            return;
        }
        Packets.UpdatePacketClass packet = new();
        Packets.UpdateClass tmpUpdate = new(188);
        tmpUpdate.SetUpdateFlag(46, Target.cUnitFlags);
        Packets.PacketClass packet2 = packet;
        WS_Creatures.CreatureObject updateObject = (WS_Creatures.CreatureObject)Target;
        tmpUpdate.AddToPacket(ref packet2, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
        packet = (Packets.UpdatePacketClass)packet2;
        var obj = Target;
        packet2 = packet;
        obj.SendToNearPlayers(ref packet2);
        packet = (Packets.UpdatePacketClass)packet2;
        tmpUpdate.Dispose();
        packet.Dispose();
    }

    /// <summary>
    /// SPELL_AURA_MOD_FEAR
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_FEAR(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        Packets.PacketClass response = new(Opcodes.SMSG_DEATH_NOTIFY_OBSOLETE);
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
            default:
                break;
        }
        switch (Target)
        {
            case WS_PlayerData.CharacterObject:
                WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
                target.SetUpdateFlag(46, Target.cUnitFlags);
                target.SendCharacterUpdate();
                target.client.Send(ref response);
                break;

            default:
                {
                    Packets.UpdatePacketClass packet = new();
                    Packets.UpdateClass tmpUpdate = new(188);
                    tmpUpdate.SetUpdateFlag(46, Target.cUnitFlags);
                    Packets.PacketClass packet2 = packet;
                    WS_Creatures.CreatureObject updateObject = (WS_Creatures.CreatureObject)Target;
                    tmpUpdate.AddToPacket(ref packet2, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
                    var obj = Target;
                    packet2 = packet;
                    obj.SendToNearPlayers(ref packet2);
                    packet = (Packets.UpdatePacketClass)packet2;
                    tmpUpdate.Dispose();
                    packet.Dispose();
                    break;
                }
        }
        response.Dispose();
    }

    /// <summary>
    /// SPELL_AURA_SAFE_FALL
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_SAFE_FALL(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        switch (Action)
        {
            case AuraAction.AURA_UPDATE:
                break;

            case AuraAction.AURA_ADD:
                {
                    Packets.PacketClass packet = new(Opcodes.SMSG_MOVE_FEATHER_FALL);
                    packet.AddPackGUID(Target.GUID);
                    Target.SendToNearPlayers(ref packet);
                    packet.Dispose();
                    break;
                }
            case AuraAction.AURA_REMOVE:
            case AuraAction.AURA_REMOVEBYDURATION:
                {
                    Packets.PacketClass packet2 = new(Opcodes.SMSG_MOVE_NORMAL_FALL);
                    packet2.AddPackGUID(Target.GUID);
                    Target.SendToNearPlayers(ref packet2);
                    packet2.Dispose();
                    break;
                }

            default:
                break;
        }
    }

    /// <summary>
    /// SPELL_AURA_FEATHER_FALL
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_FEATHER_FALL(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        switch (Action)
        {
            case AuraAction.AURA_UPDATE:
                break;

            case AuraAction.AURA_ADD:
                {
                    Packets.PacketClass packet = new(Opcodes.SMSG_MOVE_FEATHER_FALL);
                    packet.AddPackGUID(Target.GUID);
                    Target.SendToNearPlayers(ref packet);
                    packet.Dispose();
                    break;
                }
            case AuraAction.AURA_REMOVE:
            case AuraAction.AURA_REMOVEBYDURATION:
                {
                    Packets.PacketClass packet2 = new(Opcodes.SMSG_MOVE_NORMAL_FALL);
                    packet2.AddPackGUID(Target.GUID);
                    Target.SendToNearPlayers(ref packet2);
                    packet2.Dispose();
                    break;
                }

            default:
                break;
        }
    }

    /// <summary>
    /// SPELL_AURA_WATER_WALK
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_WATER_WALK(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        switch (Action)
        {
            case AuraAction.AURA_UPDATE:
                break;

            case AuraAction.AURA_ADD:
                {
                    Packets.PacketClass packet = new(Opcodes.SMSG_MOVE_WATER_WALK);
                    packet.AddPackGUID(Target.GUID);
                    Target.SendToNearPlayers(ref packet);
                    packet.Dispose();
                    break;
                }
            case AuraAction.AURA_REMOVE:
            case AuraAction.AURA_REMOVEBYDURATION:
                {
                    Packets.PacketClass packet2 = new(Opcodes.SMSG_MOVE_LAND_WALK);
                    packet2.AddPackGUID(Target.GUID);
                    Target.SendToNearPlayers(ref packet2);
                    packet2.Dispose();
                    break;
                }

            default:
                break;
        }
    }

    /// <summary>
    /// SPELL_AURA_HOVER
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_HOVER(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        switch (Action)
        {
            case AuraAction.AURA_UPDATE:
                break;

            case AuraAction.AURA_ADD:
                {
                    Packets.PacketClass packet = new(Opcodes.SMSG_MOVE_SET_HOVER);
                    packet.AddPackGUID(Target.GUID);
                    Target.SendToNearPlayers(ref packet);
                    packet.Dispose();
                    break;
                }
            case AuraAction.AURA_REMOVE:
            case AuraAction.AURA_REMOVEBYDURATION:
                {
                    Packets.PacketClass packet2 = new(Opcodes.SMSG_MOVE_UNSET_HOVER);
                    packet2.AddPackGUID(Target.GUID);
                    Target.SendToNearPlayers(ref packet2);
                    packet2.Dispose();
                    break;
                }
        }
    }

    /// <summary>
    /// SPELL_AURA_WATER_BREATHING
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
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
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// SPELL_AURA_ADD_FLAT_MODIFIER
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_ADD_FLAT_MODIFIER(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        if (Target is not WS_PlayerData.CharacterObject || EffectInfo.MiscValue > 32)
        {
            return;
        }
        SpellModOp op = (SpellModOp)EffectInfo.MiscValue;
        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
        var value = EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0);
        var mask = EffectInfo.ItemType;
        checked
        {
            int num;
            switch (Action)
            {
                case AuraAction.AURA_ADD:
                    {
                        var tmpval = (short)EffectInfo.valueBase;
                        var shiftdata = 1u;
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
                                send_val = (ushort)((65535 + tmpval + 2) & 0xFFFF);
                                send_mark = ushort.MaxValue;
                            }
                        }
                        var eff = 0;
                        do
                        {
                            if ((mask & shiftdata) != 0)
                            {
                                Packets.PacketClass packet = new(Opcodes.SMSG_SET_FLAT_SPELL_MODIFIER);
                                packet.AddInt8((byte)eff);
                                packet.AddInt8((byte)op);
                                packet.AddUInt16(send_val);
                                packet.AddUInt16(send_mark);
                                WS_PlayerData.CharacterObject caster1 = (WS_PlayerData.CharacterObject)Caster;
                                caster1.client.Send(ref packet);
                                packet.Dispose();
                            }
                            shiftdata <<= 1;
                            eff++;
                        }
                        while (eff <= 31);
                        return;
                    }
                default:
                    num = (Action == AuraAction.AURA_REMOVEBYDURATION) ? 1 : 0;
                    break;

                case AuraAction.AURA_REMOVE:
                    num = 1;
                    break;
            }
            if (num == 0)
            {
                return;
            }
        }
    }

    /// <summary>
    /// SPELL_AURA_ADD_PCT_MODIFIER
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_ADD_PCT_MODIFIER(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        if (Target is not WS_PlayerData.CharacterObject || EffectInfo.MiscValue > 32)
        {
            return;
        }
        SpellModOp op = (SpellModOp)EffectInfo.MiscValue;
        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
        var value = EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0);
        var mask = EffectInfo.ItemType;
        checked
        {
            int num;
            switch (Action)
            {
                case AuraAction.AURA_ADD:
                    {
                        var tmpval = (short)EffectInfo.valueBase;
                        var shiftdata = 1u;
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
                                send_val = (ushort)((65535 + tmpval + 2) & 0xFFFF);
                                send_mark = ushort.MaxValue;
                            }
                        }
                        var eff = 0;
                        do
                        {
                            if ((mask & shiftdata) != 0)
                            {
                                Packets.PacketClass packet = new(Opcodes.SMSG_SET_PCT_SPELL_MODIFIER);
                                packet.AddInt8((byte)eff);
                                packet.AddInt8((byte)op);
                                packet.AddUInt16(send_val);
                                packet.AddUInt16(send_mark);
                                WS_PlayerData.CharacterObject caster1 = (WS_PlayerData.CharacterObject)Caster;
                                caster1.client.Send(ref packet);
                                packet.Dispose();
                            }
                            shiftdata <<= 1;
                            eff++;
                        }
                        while (eff <= 31);
                        return;
                    }
                default:
                    num = (Action == AuraAction.AURA_REMOVEBYDURATION) ? 1 : 0;
                    break;

                case AuraAction.AURA_REMOVE:
                    num = 1;
                    break;
            }
            if (num == 0)
            {
                return;
            }
        }
    }

    public void SPELL_AURA_MOD_STAT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        if (Action == AuraAction.AURA_UPDATE || Target is not WS_PlayerData.CharacterObject)
        {
            return;
        }

        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
        var value = EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0);
        var value_sign = value;
        checked
        {
            if (Action == AuraAction.AURA_REMOVE)
            {
                value = -value;
            }

            WS_PlayerData.CharacterObject target1 = (WS_PlayerData.CharacterObject)Target;
            switch (EffectInfo.MiscValue)
            {
                case -1:
                    {
                        var target = target1;
                        ref var base9 = ref target.Strength.Base;
                        base9 = (int)Math.Round(base9 / target.Strength.Modifier);
                        target.Strength.Base += value;
                        ref var base10 = ref target.Strength.Base;
                        base10 = (int)Math.Round(base10 * target.Strength.Modifier);
                        ref var base11 = ref target.Agility.Base;
                        base11 = (int)Math.Round(base11 / target.Agility.Modifier);
                        target.Agility.Base += value;
                        ref var base12 = ref target.Agility.Base;
                        base12 = (int)Math.Round(base12 * target.Agility.Modifier);
                        ref var base13 = ref target.Stamina.Base;
                        base13 = (int)Math.Round(base13 / target.Stamina.Modifier);
                        target.Stamina.Base += value;
                        ref var base14 = ref target.Stamina.Base;
                        base14 = (int)Math.Round(base14 * target.Stamina.Modifier);
                        ref var base15 = ref target.Spirit.Base;
                        base15 = (int)Math.Round(base15 / target.Spirit.Modifier);
                        target.Spirit.Base += value;
                        ref var base16 = ref target.Spirit.Base;
                        base16 = (int)Math.Round(base16 * target.Spirit.Modifier);
                        ref var base17 = ref target.Intellect.Base;
                        base17 = (int)Math.Round(base17 / target.Intellect.Modifier);
                        target.Intellect.Base += value;
                        ref var base18 = ref target.Intellect.Base;
                        base18 = (int)Math.Round(base18 * target.Intellect.Modifier);
                        if (value_sign > 0)
                        {
                            ref var positiveBonus5 = ref target.Strength.PositiveBonus;
                            positiveBonus5 = (short)(positiveBonus5 + value);
                            ref var positiveBonus6 = ref target.Agility.PositiveBonus;
                            positiveBonus6 = (short)(positiveBonus6 + value);
                            ref var positiveBonus7 = ref target.Stamina.PositiveBonus;
                            positiveBonus7 = (short)(positiveBonus7 + value);
                            ref var positiveBonus8 = ref target.Spirit.PositiveBonus;
                            positiveBonus8 = (short)(positiveBonus8 + value);
                            ref var positiveBonus9 = ref target.Intellect.PositiveBonus;
                            positiveBonus9 = (short)(positiveBonus9 + value);
                        }
                        else
                        {
                            ref var negativeBonus5 = ref target.Strength.NegativeBonus;
                            negativeBonus5 = (short)(negativeBonus5 - value);
                            ref var negativeBonus6 = ref target.Agility.NegativeBonus;
                            negativeBonus6 = (short)(negativeBonus6 - value);
                            ref var negativeBonus7 = ref target.Stamina.NegativeBonus;
                            negativeBonus7 = (short)(negativeBonus7 - value);
                            ref var negativeBonus8 = ref target.Spirit.NegativeBonus;
                            negativeBonus8 = (short)(negativeBonus8 - value);
                            ref var negativeBonus9 = ref target.Intellect.NegativeBonus;
                            negativeBonus9 = (short)(negativeBonus9 - value);
                        }
                        break;
                    }
                case 0:
                    {
                        var target = target1;
                        ref var base5 = ref target.Strength.Base;
                        base5 = (int)Math.Round(base5 / target.Strength.Modifier);
                        target.Strength.Base += value;
                        ref var base6 = ref target.Strength.Base;
                        base6 = (int)Math.Round(base6 * target.Strength.Modifier);
                        if (value_sign > 0)
                        {
                            ref var positiveBonus3 = ref target.Strength.PositiveBonus;
                            positiveBonus3 = (short)(positiveBonus3 + value);
                        }
                        else
                        {
                            ref var negativeBonus3 = ref target.Strength.NegativeBonus;
                            negativeBonus3 = (short)(negativeBonus3 - value);
                        }
                        break;
                    }
                case 1:
                    {
                        var target = target1;
                        ref var base19 = ref target.Agility.Base;
                        base19 = (int)Math.Round(base19 / target.Agility.Modifier);
                        target.Agility.Base += value;
                        ref var base20 = ref target.Agility.Base;
                        base20 = (int)Math.Round(base20 * target.Agility.Modifier);
                        if (value_sign > 0)
                        {
                            ref var positiveBonus10 = ref target.Agility.PositiveBonus;
                            positiveBonus10 = (short)(positiveBonus10 + value);
                        }
                        else
                        {
                            ref var negativeBonus10 = ref target.Agility.NegativeBonus;
                            negativeBonus10 = (short)(negativeBonus10 - value);
                        }
                        break;
                    }
                case 2:
                    {
                        var target = target1;
                        ref var base3 = ref target.Stamina.Base;
                        base3 = (int)Math.Round(base3 / target.Stamina.Modifier);
                        target.Stamina.Base += value;
                        ref var base4 = ref target.Stamina.Base;
                        base4 = (int)Math.Round(base4 * target.Stamina.Modifier);
                        if (value_sign > 0)
                        {
                            ref var positiveBonus2 = ref target.Stamina.PositiveBonus;
                            positiveBonus2 = (short)(positiveBonus2 + value);
                        }
                        else
                        {
                            ref var negativeBonus2 = ref target.Stamina.NegativeBonus;
                            negativeBonus2 = (short)(negativeBonus2 - value);
                        }
                        break;
                    }
                case 3:
                    {
                        var target = target1;
                        ref var base7 = ref target.Intellect.Base;
                        base7 = (int)Math.Round(base7 / target.Intellect.Modifier);
                        target.Intellect.Base += value;
                        ref var base8 = ref target.Intellect.Base;
                        base8 = (int)Math.Round(base8 * target.Intellect.Modifier);
                        if (value_sign > 0)
                        {
                            ref var positiveBonus4 = ref target.Intellect.PositiveBonus;
                            positiveBonus4 = (short)(positiveBonus4 + value);
                        }
                        else
                        {
                            ref var negativeBonus4 = ref target.Intellect.NegativeBonus;
                            negativeBonus4 = (short)(negativeBonus4 - value);
                        }
                        break;
                    }
                case 4:
                    {
                        var target = target1;
                        ref var @base = ref target.Spirit.Base;
                        @base = (int)Math.Round(@base / target.Spirit.Modifier);
                        target.Spirit.Base += value;
                        ref var base2 = ref target.Spirit.Base;
                        base2 = (int)Math.Round(base2 * target.Spirit.Modifier);
                        if (value_sign > 0)
                        {
                            ref var positiveBonus = ref target.Spirit.PositiveBonus;
                            positiveBonus = (short)(positiveBonus + value);
                        }
                        else
                        {
                            ref var negativeBonus = ref target.Spirit.NegativeBonus;
                            negativeBonus = (short)(negativeBonus - value);
                        }
                        break;
                    }

                default:
                    break;
            }
            target1.Life.Bonus = (target1.Stamina.Base - 18) * 10;
            target1.Mana.Bonus = (target1.Intellect.Base - 18) * 15;
            target1.GroupUpdateFlag = target1.GroupUpdateFlag | 4u | 0x20u;
            target1.Resistances[0].Base += value * 2;
            target1.UpdateManaRegen();
            var wS_Combat = WorldServiceLocator._WS_Combat;
            var objCharacter = target1;
            wS_Combat.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.BASE_ATTACK);
            var wS_Combat2 = WorldServiceLocator._WS_Combat;
            objCharacter = target1;
            wS_Combat2.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.OFF_ATTACK);
            var wS_Combat3 = WorldServiceLocator._WS_Combat;
            objCharacter = target1;
            wS_Combat3.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.RANGED_ATTACK);
            target1.SetUpdateFlag(150, target1.Strength.Base);
            target1.SetUpdateFlag(151, target1.Agility.Base);
            target1.SetUpdateFlag(152, target1.Stamina.Base);
            target1.SetUpdateFlag(153, target1.Spirit.Base);
            target1.SetUpdateFlag(154, target1.Intellect.Base);
            target1.SetUpdateFlag(22, target1.Life.Current);
            target1.SetUpdateFlag(28, target1.Life.Maximum);
            if (WorldServiceLocator._WS_Player_Initializator.GetClassManaType(target1.Classe) == ManaTypes.TYPE_MANA)
            {
                target1.SetUpdateFlag(23, target1.Mana.Current);
                target1.SetUpdateFlag(29, target1.Mana.Maximum);
            }
            target1.SetUpdateFlag(155, target1.Resistances[0].Base);
            target1.SendCharacterUpdate(toNear: false);
            target1.GroupUpdateFlag |= 4u;
            target1.GroupUpdateFlag |= 0x20u;
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_STAT_PERCENT
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_STAT_PERCENT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        if (Action != AuraAction.AURA_UPDATE && Target is WS_PlayerData.CharacterObject @object)
        {
            WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
            var value = (float)(EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0);
            var value_sign = EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0);
            if (Action == AuraAction.AURA_REMOVE)
            {
                value = 0f - value;
            }
            checked
            {
                var OldStr = (short)@object.Strength.Base;
                var OldAgi = (short)@object.Agility.Base;
                var OldSta = (short)@object.Stamina.Base;
                var OldSpi = (short)@object.Spirit.Base;
                var OldInt = (short)@object.Intellect.Base;
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
                var wS_Combat = WorldServiceLocator._WS_Combat;
                var objCharacter = @object;
                wS_Combat.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.BASE_ATTACK);
                var wS_Combat2 = WorldServiceLocator._WS_Combat;
                objCharacter = @object;
                wS_Combat2.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.OFF_ATTACK);
                var wS_Combat3 = WorldServiceLocator._WS_Combat;
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

    /// <summary>
    /// SPELL_AURA_MOD_TOTAL_STAT_PERCENTAGE
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_TOTAL_STAT_PERCENTAGE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        if (Action == AuraAction.AURA_UPDATE || Target is not WS_PlayerData.CharacterObject)
        {
            return;
        }

        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
        var value = (float)(EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0);
        var value_sign = EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0);
        if (Action == AuraAction.AURA_REMOVE)
        {
            value = 0f - value;
        }
        checked
        {
            WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
            var OldStr = (short)target.Strength.Base;
            var OldAgi = (short)target.Agility.Base;
            var OldSta = (short)target.Stamina.Base;
            var OldSpi = (short)target.Spirit.Base;
            var OldInt = (short)target.Intellect.Base;
            switch (EffectInfo.MiscValue)
            {
                case -1:
                    {
                        ref var base3 = ref target.Strength.Base;
                        base3 = (int)Math.Round(base3 / target.Strength.Modifier);
                        target.Strength.Modifier += value;
                        ref var base4 = ref target.Strength.Base;
                        base4 = (int)Math.Round(base4 * target.Strength.Modifier);
                        ref var base5 = ref target.Agility.Base;
                        base5 = (int)Math.Round(base5 / target.Agility.Modifier);
                        target.Agility.Modifier += value;
                        ref var base6 = ref target.Agility.Base;
                        base6 = (int)Math.Round(base6 * target.Agility.Modifier);
                        ref var base7 = ref target.Stamina.Base;
                        base7 = (int)Math.Round(base7 / target.Stamina.Modifier);
                        target.Stamina.Modifier += value;
                        ref var base8 = ref target.Stamina.Base;
                        base8 = (int)Math.Round(base8 * target.Stamina.Modifier);
                        ref var base9 = ref target.Spirit.Base;
                        base9 = (int)Math.Round(base9 / target.Spirit.Modifier);
                        target.Spirit.Modifier += value;
                        ref var base10 = ref target.Spirit.Base;
                        base10 = (int)Math.Round(base10 * target.Spirit.Modifier);
                        ref var base11 = ref target.Intellect.Base;
                        base11 = (int)Math.Round(base11 / target.Intellect.Modifier);
                        target.Intellect.Modifier += value;
                        ref var base12 = ref target.Intellect.Base;
                        base12 = (int)Math.Round(base12 * target.Intellect.Modifier);
                        if (value_sign > 0)
                        {
                            ref var positiveBonus2 = ref target.Strength.PositiveBonus;
                            positiveBonus2 = (short)(positiveBonus2 + (target.Strength.Base - OldStr));
                            ref var positiveBonus3 = ref target.Agility.PositiveBonus;
                            positiveBonus3 = (short)(positiveBonus3 + (target.Agility.Base - OldAgi));
                            ref var positiveBonus4 = ref target.Stamina.PositiveBonus;
                            positiveBonus4 = (short)(positiveBonus4 + (target.Stamina.Base - OldSta));
                            ref var positiveBonus5 = ref target.Spirit.PositiveBonus;
                            positiveBonus5 = (short)(positiveBonus5 + (target.Spirit.Base - OldSpi));
                            ref var positiveBonus6 = ref target.Intellect.PositiveBonus;
                            positiveBonus6 = (short)(positiveBonus6 + (target.Intellect.Base - OldInt));
                        }
                        else
                        {
                            ref var negativeBonus2 = ref target.Strength.NegativeBonus;
                            negativeBonus2 = (short)(negativeBonus2 - (target.Strength.Base - OldStr));
                            ref var negativeBonus3 = ref target.Agility.NegativeBonus;
                            negativeBonus3 = (short)(negativeBonus3 - (target.Agility.Base - OldAgi));
                            ref var negativeBonus4 = ref target.Stamina.NegativeBonus;
                            negativeBonus4 = (short)(negativeBonus4 - (target.Stamina.Base - OldSta));
                            ref var negativeBonus5 = ref target.Spirit.NegativeBonus;
                            negativeBonus5 = (short)(negativeBonus5 - (target.Spirit.Base - OldSpi));
                            ref var negativeBonus6 = ref target.Intellect.NegativeBonus;
                            negativeBonus6 = (short)(negativeBonus6 - (target.Intellect.Base - OldInt));
                        }
                        break;
                    }
                case 0:
                    {
                        ref var base17 = ref target.Strength.Base;
                        base17 = (int)Math.Round(base17 / target.Strength.Modifier);
                        target.Strength.Modifier += value;
                        ref var base18 = ref target.Strength.Base;
                        base18 = (int)Math.Round(base18 * target.Strength.Modifier);
                        if (value_sign > 0)
                        {
                            ref var positiveBonus9 = ref target.Strength.PositiveBonus;
                            positiveBonus9 = (short)(positiveBonus9 + (target.Strength.Base - OldStr));
                        }
                        else
                        {
                            ref var negativeBonus9 = ref target.Strength.NegativeBonus;
                            negativeBonus9 = (short)(negativeBonus9 - (target.Strength.Base - OldStr));
                        }
                        break;
                    }
                case 1:
                    {
                        ref var base13 = ref target.Agility.Base;
                        base13 = (int)Math.Round(base13 / target.Agility.Modifier);
                        target.Agility.Modifier += value;
                        ref var base14 = ref target.Agility.Base;
                        base14 = (int)Math.Round(base14 * target.Agility.Modifier);
                        if (value_sign > 0)
                        {
                            ref var positiveBonus7 = ref target.Agility.PositiveBonus;
                            positiveBonus7 = (short)(positiveBonus7 + (target.Agility.Base - OldAgi));
                        }
                        else
                        {
                            ref var negativeBonus7 = ref target.Agility.NegativeBonus;
                            negativeBonus7 = (short)(negativeBonus7 - (target.Agility.Base - OldAgi));
                        }
                        break;
                    }
                case 2:
                    {
                        ref var base15 = ref target.Stamina.Base;
                        base15 = (int)Math.Round(base15 / target.Stamina.Modifier);
                        target.Stamina.Modifier += value;
                        ref var base16 = ref target.Stamina.Base;
                        base16 = (int)Math.Round(base16 * target.Stamina.Modifier);
                        if (value_sign > 0)
                        {
                            ref var positiveBonus8 = ref target.Stamina.PositiveBonus;
                            positiveBonus8 = (short)(positiveBonus8 + (target.Stamina.Base - OldSta));
                        }
                        else
                        {
                            ref var negativeBonus8 = ref target.Stamina.NegativeBonus;
                            negativeBonus8 = (short)(negativeBonus8 - (target.Stamina.Base - OldSta));
                        }
                        break;
                    }
                case 4:
                    {
                        ref var @base = ref target.Spirit.Base;
                        @base = (int)Math.Round(@base / target.Spirit.Modifier);
                        target.Spirit.Modifier += value;
                        ref var base2 = ref target.Spirit.Base;
                        base2 = (int)Math.Round(base2 * target.Spirit.Modifier);
                        if (value_sign > 0)
                        {
                            ref var positiveBonus = ref target.Spirit.PositiveBonus;
                            positiveBonus = (short)(positiveBonus + (target.Spirit.Base - OldSpi));
                        }
                        else
                        {
                            ref var negativeBonus = ref target.Spirit.NegativeBonus;
                            negativeBonus = (short)(negativeBonus - (target.Spirit.Base - OldSpi));
                        }
                        break;
                    }
            }
            target.Life.Bonus = (target.Stamina.Base - 18) * 10;
            target.Mana.Bonus = (target.Intellect.Base - 18) * 15;
            target.Resistances[0].Base += (target.Agility.Base - OldAgi) * 2;
            target.GroupUpdateFlag = target.GroupUpdateFlag | 4u | 0x20u;
            target.UpdateManaRegen();
            var wS_Combat = WorldServiceLocator._WS_Combat;
            var objCharacter = target;
            wS_Combat.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.BASE_ATTACK);
            var wS_Combat2 = WorldServiceLocator._WS_Combat;
            objCharacter = target;
            wS_Combat2.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.OFF_ATTACK);
            var wS_Combat3 = WorldServiceLocator._WS_Combat;
            objCharacter = target;
            wS_Combat3.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.RANGED_ATTACK);
            target.SetUpdateFlag(150, target.Strength.Base);
            target.SetUpdateFlag(151, target.Agility.Base);
            target.SetUpdateFlag(152, target.Stamina.Base);
            target.SetUpdateFlag(153, target.Spirit.Base);
            target.SetUpdateFlag(154, target.Intellect.Base);
            target.SetUpdateFlag(22, target.Life.Current);
            target.SetUpdateFlag(28, target.Life.Maximum);
            if (WorldServiceLocator._WS_Player_Initializator.GetClassManaType(target.Classe) == ManaTypes.TYPE_MANA)
            {
                target.SetUpdateFlag(23, target.Mana.Current);
                target.SetUpdateFlag(29, target.Mana.Maximum);
            }
            target.SetUpdateFlag(155, target.Resistances[0].Base);
            target.SendCharacterUpdate(toNear: false);
            target.GroupUpdateFlag |= 4u;
            target.GroupUpdateFlag |= 0x20u;
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_INCREASE_HEALTH
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_INCREASE_HEALTH(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        checked
        {
            WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
            switch (Action)
            {
                case AuraAction.AURA_UPDATE:
                    return;

                case AuraAction.AURA_ADD:
                    Target.Life.Bonus += EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0);
                    break;

                case AuraAction.AURA_REMOVE:
                case AuraAction.AURA_REMOVEBYDURATION:
                    Target.Life.Bonus -= EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0);
                    break;
                default:
                    break;
            }
            if (Target is WS_PlayerData.CharacterObject @object)
            {
                @object.SetUpdateFlag(28, Target.Life.Maximum);
                @object.SendCharacterUpdate();
                @object.GroupUpdateFlag |= 4u;
                return;
            }
            Packets.UpdatePacketClass packet = new();
            Packets.UpdateClass UpdateData = new(188);
            UpdateData.SetUpdateFlag(28, Target.Life.Maximum);
            Packets.PacketClass packet2 = packet;
            WS_Creatures.CreatureObject target = (WS_Creatures.CreatureObject)Target;
            var updateObject = target;
            UpdateData.AddToPacket(ref packet2, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
            packet = (Packets.UpdatePacketClass)packet2;
            var obj = target;
            packet2 = packet;
            obj.SendToNearPlayers(ref packet2);
            packet = (Packets.UpdatePacketClass)packet2;
            packet.Dispose();
            UpdateData.Dispose();
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_INCREASE_HEALTH_PERCENT
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_INCREASE_HEALTH_PERCENT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        switch (Action)
        {
            case AuraAction.AURA_UPDATE:
                return;

            case AuraAction.AURA_ADD:
                {
                    ref var modifier2 = ref Target.Life.Modifier;
                    modifier2 = (float)(modifier2 + (EffectInfo.GetValue(Target.Level, 0) / 100.0));
                    break;
                }
            case AuraAction.AURA_REMOVE:
            case AuraAction.AURA_REMOVEBYDURATION:
                {
                    ref var modifier = ref Target.Life.Modifier;
                    modifier = (float)(modifier - (EffectInfo.GetValue(Target.Level, 0) / 100.0));
                    break;
                }

            default:
                break;
        }
        if (Target is WS_PlayerData.CharacterObject @object)
        {
            @object.SetUpdateFlag(28, Target.Life.Maximum);
            @object.SendCharacterUpdate();
            @object.GroupUpdateFlag |= 4u;
            return;
        }
        Packets.UpdatePacketClass packet = new();
        Packets.UpdateClass UpdateData = new(188);
        UpdateData.SetUpdateFlag(28, Target.Life.Maximum);
        Packets.PacketClass packet2 = packet;
        WS_Creatures.CreatureObject target = (WS_Creatures.CreatureObject)Target;
        var updateObject = target;
        UpdateData.AddToPacket(ref packet2, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
        packet = (Packets.UpdatePacketClass)packet2;
        var obj = target;
        packet2 = packet;
        obj.SendToNearPlayers(ref packet2);
        packet = (Packets.UpdatePacketClass)packet2;
        packet.Dispose();
        UpdateData.Dispose();
    }

    /// <summary>
    /// SPELL_AURA_MOD_INCREASE_ENERGY
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
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
                    WS_Base.BaseUnit caster1 = (WS_Base.BaseUnit)Caster;
                    if (Target is not WS_PlayerData.CharacterObject)
                    {
                        var caster = caster1;
                        Target.Mana.Bonus += EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0);
                        break;
                    }

                    WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
                    switch (Target.ManaType)
                    {
                        case ManaTypes.TYPE_ENERGY:
                            target.Energy.Bonus += EffectInfo.GetValue(Level: caster1.Level, ComboPoints: 0);
                            break;

                        case ManaTypes.TYPE_MANA:
                            target.Mana.Bonus += EffectInfo.GetValue(Level: caster1.Level, ComboPoints: 0);
                            break;

                        case ManaTypes.TYPE_RAGE:
                            target.Rage.Bonus += EffectInfo.GetValue(Level: caster1.Level, ComboPoints: 0);
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
                    WS_Base.BaseUnit caster1 = (WS_Base.BaseUnit)Caster;
                    if (Target is not WS_PlayerData.CharacterObject)
                    {
                        var caster = caster1;
                        Target.Mana.Bonus -= EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0);
                        break;
                    }

                    WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
                    switch (Target.ManaType)
                    {
                        case ManaTypes.TYPE_ENERGY:
                            target.Energy.Bonus -= EffectInfo.GetValue(Level: caster1.Level, ComboPoints: 0);
                            break;

                        case ManaTypes.TYPE_MANA:
                            target.Mana.Bonus -= EffectInfo.GetValue(Level: caster1.Level, ComboPoints: 0);
                            break;

                        case ManaTypes.TYPE_RAGE:
                            target.Rage.Bonus -= EffectInfo.GetValue(Level: caster1.Level, ComboPoints: 0);
                            break;
                        default:
                            break;
                    }
                    break;
                }

            default:
                break;
        }
        if (Target is WS_PlayerData.CharacterObject @object)
        {
            @object.SetUpdateFlag(32, @object.Energy.Maximum);
            @object.SetUpdateFlag(29, @object.Mana.Maximum);
            @object.SetUpdateFlag(30, @object.Rage.Maximum);
            return;
        }
        Packets.UpdatePacketClass packet = new();
        Packets.UpdateClass UpdateData = new(188);
        UpdateData.SetUpdateFlag((int)checked(29 + Target.ManaType), Target.Mana.Maximum);
        Packets.PacketClass packet2 = packet;
        WS_Creatures.CreatureObject target1 = (WS_Creatures.CreatureObject)Target;
        var updateObject = target1;
        UpdateData.AddToPacket(ref packet2, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
        packet = (Packets.UpdatePacketClass)packet2;
        var obj = target1;
        packet2 = packet;
        obj.SendToNearPlayers(ref packet2);
        packet = (Packets.UpdatePacketClass)packet2;
        packet.Dispose();
        UpdateData.Dispose();
    }

    /// <summary>
    /// SPELL_AURA_MOD_BASE_RESISTANCE
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_BASE_RESISTANCE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        if (Target is not WS_PlayerData.CharacterObject)
        {
            return;
        }
        switch (Action)
        {
            case AuraAction.AURA_UPDATE:
                break;

            case AuraAction.AURA_ADD:
                {
                    var i = DamageTypes.DMG_PHYSICAL;
                    var i1 = (int)i;
                    do
                    {
                        if (WorldServiceLocator._Functions.HaveFlag(checked((uint)EffectInfo.MiscValue), (byte)i))
                        {
                            WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
                            ref var base3 = ref target.Resistances[(uint)i].Base;
                            checked
                            {
                                base3 = (int)Math.Round(base3 / target.Resistances[(uint)i].Modifier);
                                target.Resistances[(uint)i].Base += EffectInfo.GetValue(Target.Level, 0);
                                ref var base4 = ref target.Resistances[(uint)i].Base;
                                base4 = (int)Math.Round(base4 * target.Resistances[(uint)i].Modifier);
                                target.SetUpdateFlag(155 + i1, target.Resistances[(uint)i].Base);
                            }
                        }
                        i++;
                    }
                    while (i1 <= 6);
                    break;
                }
            case AuraAction.AURA_REMOVE:
            case AuraAction.AURA_REMOVEBYDURATION:
                {
                    var j = DamageTypes.DMG_PHYSICAL;
                    var j1 = (int)j;
                    do
                    {
                        if (WorldServiceLocator._Functions.HaveFlag(checked((uint)EffectInfo.MiscValue), (byte)j))
                        {
                            WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
                            ref var @base = ref target.Resistances[(uint)j].Base;
                            checked
                            {
                                @base = (int)Math.Round(@base / target.Resistances[(uint)j].Modifier);
                                target.Resistances[(uint)j].Base -= EffectInfo.GetValue(Target.Level, 0);
                                ref var base2 = ref target.Resistances[(uint)j].Base;
                                base2 = (int)Math.Round(base2 * target.Resistances[(uint)j].Modifier);
                                target.SetUpdateFlag(155 + j1, target.Resistances[(uint)j].Base);
                            }
                        }
                        j++;
                    }
                    while (j1 <= 6);
                    break;
                }

            default:
                break;
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_BASE_RESISTANCE_PCT
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_BASE_RESISTANCE_PCT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        if (Target is not WS_PlayerData.CharacterObject)
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
                                WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
                                ref var base3 = ref target.Resistances[i].Base;
                                base3 = (int)Math.Round(base3 / target.Resistances[i].Modifier);
                                ref var modifier2 = ref target.Resistances[i].Modifier;
                                modifier2 = (float)(modifier2 + (EffectInfo.GetValue(Target.Level, 0) / 100.0));
                                ref var base4 = ref target.Resistances[i].Base;
                                base4 = (int)Math.Round(base4 * target.Resistances[i].Modifier);
                                target.SetUpdateFlag(155 + i, target.Resistances[i].Base);
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
                                WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
                                ref var @base = ref target.Resistances[j].Base;
                                @base = (int)Math.Round(@base / target.Resistances[j].Modifier);
                                ref var modifier = ref target.Resistances[j].Modifier;
                                modifier = (float)(modifier - (EffectInfo.GetValue(Target.Level, 0) / 100.0));
                                ref var base2 = ref target.Resistances[j].Base;
                                base2 = (int)Math.Round(base2 * target.Resistances[j].Modifier);
                                target.SetUpdateFlag(155 + j, target.Resistances[j].Base);
                            }
                            j = (byte)unchecked((uint)(j + 1));
                        }
                    }
                    while (j <= 6u);
                    break;
                }

            default:
                break;
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_RESISTANCE
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_RESISTANCE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        if (Target is not WS_PlayerData.CharacterObject)
        {
            return;
        }

        WS_PlayerData.CharacterObject target2 = (WS_PlayerData.CharacterObject)Target;
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
                                var target1 = target2;
                                if (EffectInfo.GetValue(Target.Level, 0) > 0)
                                {
                                    ref var base5 = ref Target.Resistances[i].Base;
                                    var target = target1;
                                    base5 = (int)Math.Round(base5 / target.Resistances[i].Modifier);
                                    Target.Resistances[i].Base += EffectInfo.GetValue(Target.Level, 0);
                                    ref var base6 = ref Target.Resistances[i].Base;
                                    base6 = (int)Math.Round(base6 * target.Resistances[i].Modifier);
                                    ref var positiveBonus2 = ref Target.Resistances[i].PositiveBonus;
                                    positiveBonus2 = (short)(positiveBonus2 + EffectInfo.GetValue(Target.Level, 0));
                                }
                                else
                                {
                                    ref var base7 = ref Target.Resistances[i].Base;
                                    var target = target1;
                                    base7 = (int)Math.Round(base7 / target.Resistances[i].Modifier);
                                    Target.Resistances[i].Base += EffectInfo.GetValue(Target.Level, 0);
                                    ref var base8 = ref Target.Resistances[i].Base;
                                    base8 = (int)Math.Round(base8 * target.Resistances[i].Modifier);
                                    ref var negativeBonus2 = ref Target.Resistances[i].NegativeBonus;
                                    negativeBonus2 = (short)(negativeBonus2 - EffectInfo.GetValue(Target.Level, 0));
                                }
                                target1.SetUpdateFlag(155 + i, target1.Resistances[i].Base);
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
                                var target1 = target2;
                                if (EffectInfo.GetValue(Target.Level, 0) > 0)
                                {
                                    ref var @base = ref Target.Resistances[j].Base;
                                    var target = target1;
                                    @base = (int)Math.Round(@base / target.Resistances[j].Modifier);
                                    Target.Resistances[j].Base -= EffectInfo.GetValue(Target.Level, 0);
                                    ref var base2 = ref Target.Resistances[j].Base;
                                    base2 = (int)Math.Round(base2 * target.Resistances[j].Modifier);
                                    ref var positiveBonus = ref Target.Resistances[j].PositiveBonus;
                                    positiveBonus = (short)(positiveBonus - EffectInfo.GetValue(Target.Level, 0));
                                }
                                else
                                {
                                    ref var base3 = ref Target.Resistances[j].Base;
                                    var target = target1;
                                    base3 = (int)Math.Round(base3 / target.Resistances[j].Modifier);
                                    Target.Resistances[j].Base -= EffectInfo.GetValue(Target.Level, 0);
                                    ref var base4 = ref Target.Resistances[j].Base;
                                    base4 = (int)Math.Round(base4 * target.Resistances[j].Modifier);
                                    ref var negativeBonus = ref Target.Resistances[j].NegativeBonus;
                                    negativeBonus = (short)(negativeBonus + EffectInfo.GetValue(Target.Level, 0));
                                }
                                target1.SetUpdateFlag(155 + j, target1.Resistances[j].Base);
                            }
                            j = (byte)unchecked((uint)(j + 1));
                        }
                    }
                    while (j <= 6u);
                    break;
                }

            default:
                break;
        }
        target2.SendCharacterUpdate(toNear: false);
    }

    /// <summary>
    /// SPELL_AURA_MOD_RESISTANCE_PCT
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_RESISTANCE_PCT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        if (Target is not WS_PlayerData.CharacterObject)
        {
            return;
        }

        WS_PlayerData.CharacterObject target1 = (WS_PlayerData.CharacterObject)Target;
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
                                var target = target1;
                                var OldBase = (short)target.Resistances[i].Base;
                                if (EffectInfo.GetValue(Target.Level, 0) > 0)
                                {
                                    ref var base5 = ref target.Resistances[i].Base;
                                    base5 = (int)Math.Round(base5 / target.Resistances[i].Modifier);
                                    target.Resistances[i].Modifier += EffectInfo.GetValue(Target.Level, 0);
                                    ref var base6 = ref target.Resistances[i].Base;
                                    base6 = (int)Math.Round(base6 * target.Resistances[i].Modifier);
                                    ref var positiveBonus2 = ref target.Resistances[i].PositiveBonus;
                                    positiveBonus2 = (short)(positiveBonus2 + (target.Resistances[i].Base - OldBase));
                                }
                                else
                                {
                                    ref var base7 = ref target.Resistances[i].Base;
                                    base7 = (int)Math.Round(base7 / target.Resistances[i].Modifier);
                                    target.Resistances[i].Modifier -= EffectInfo.GetValue(Target.Level, 0);
                                    ref var base8 = ref target.Resistances[i].Base;
                                    base8 = (int)Math.Round(base8 * target.Resistances[i].Modifier);
                                    ref var positiveBonus3 = ref target.Resistances[i].PositiveBonus;
                                    positiveBonus3 = (short)(positiveBonus3 + (target.Resistances[i].Base - OldBase));
                                }
                                target.SetUpdateFlag(155 + i, target.Resistances[i].Base);
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
                                var target = target1;
                                var OldBase2 = (short)target.Resistances[j].Base;
                                if (EffectInfo.GetValue(Target.Level, 0) > 0)
                                {
                                    ref var @base = ref target.Resistances[j].Base;
                                    @base = (int)Math.Round(@base / target.Resistances[j].Modifier);
                                    target.Resistances[j].Modifier -= EffectInfo.GetValue(Target.Level, 0);
                                    ref var base2 = ref target.Resistances[j].Base;
                                    base2 = (int)Math.Round(base2 * target.Resistances[j].Modifier);
                                    ref var positiveBonus = ref target.Resistances[j].PositiveBonus;
                                    positiveBonus = (short)(positiveBonus - (target.Resistances[j].Base - OldBase2));
                                }
                                else
                                {
                                    ref var base3 = ref target.Resistances[j].Base;
                                    base3 = (int)Math.Round(base3 / target.Resistances[j].Modifier);
                                    target.Resistances[j].Modifier += EffectInfo.GetValue(Target.Level, 0);
                                    ref var base4 = ref target.Resistances[j].Base;
                                    base4 = (int)Math.Round(base4 * target.Resistances[j].Modifier);
                                    ref var negativeBonus = ref target.Resistances[j].NegativeBonus;
                                    negativeBonus = (short)(negativeBonus - (target.Resistances[j].Base - OldBase2));
                                }
                                target.SetUpdateFlag(155 + j, target.Resistances[j].Base);
                            }
                            j = (byte)unchecked((uint)(j + 1));
                        }
                    }
                    while (j <= 6u);
                    break;
                }

            default:
                break;
        }
        target1.SendCharacterUpdate(toNear: false);
    }

    /// <summary>
    /// SPELL_AURA_MOD_RESISTANCE_EXCLUSIVE
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_RESISTANCE_EXCLUSIVE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        if (Target is not WS_PlayerData.CharacterObject)
        {
            return;
        }

        WS_PlayerData.CharacterObject target1 = (WS_PlayerData.CharacterObject)Target;
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
                                if (EffectInfo.GetValue(Target.Level, 0) > 0)
                                {
                                    var target = target1;
                                    ref var base5 = ref target.Resistances[i].Base;
                                    base5 = (int)Math.Round(base5 / target.Resistances[i].Modifier);
                                    target.Resistances[i].Base += EffectInfo.GetValue(Target.Level, 0);
                                    ref var base6 = ref target.Resistances[i].Base;
                                    base6 = (int)Math.Round(base6 * target.Resistances[i].Modifier);
                                    ref var positiveBonus3 = ref target.Resistances[i].PositiveBonus;
                                    positiveBonus3 = (short)(positiveBonus3 + EffectInfo.GetValue(Target.Level, 0));
                                    target.SetUpdateFlag(155 + i, target.Resistances[i].Base);
                                }
                                else
                                {
                                    var target = target1;
                                    ref var base7 = ref target.Resistances[i].Base;
                                    base7 = (int)Math.Round(base7 / target.Resistances[i].Modifier);
                                    target.Resistances[i].Base -= EffectInfo.GetValue(Target.Level, 0);
                                    ref var base8 = ref target.Resistances[i].Base;
                                    base8 = (int)Math.Round(base8 * target.Resistances[i].Modifier);
                                    ref var negativeBonus = ref target.Resistances[i].NegativeBonus;
                                    negativeBonus = (short)(negativeBonus - EffectInfo.GetValue(Target.Level, 0));
                                    target.SetUpdateFlag(155 + i, target.Resistances[i].Base);
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
                                if (EffectInfo.GetValue(Target.Level, 0) > 0)
                                {
                                    var target = target1;
                                    ref var @base = ref target.Resistances[j].Base;
                                    @base = (int)Math.Round(@base / target.Resistances[j].Modifier);
                                    target.Resistances[j].Base -= EffectInfo.GetValue(Target.Level, 0);
                                    ref var base2 = ref target.Resistances[j].Base;
                                    base2 = (int)Math.Round(base2 * target.Resistances[j].Modifier);
                                    ref var positiveBonus = ref target.Resistances[j].PositiveBonus;
                                    positiveBonus = (short)(positiveBonus - EffectInfo.GetValue(Target.Level, 0));
                                    target.SetUpdateFlag(155 + j, target.Resistances[j].Base);
                                }
                                else
                                {
                                    var target = target1;
                                    ref var base3 = ref target.Resistances[j].Base;
                                    base3 = (int)Math.Round(base3 / target.Resistances[j].Modifier);
                                    target.Resistances[j].Base += EffectInfo.GetValue(Target.Level, 0);
                                    ref var base4 = ref target.Resistances[j].Base;
                                    base4 = (int)Math.Round(base4 * target.Resistances[j].Modifier);
                                    ref var positiveBonus2 = ref target.Resistances[j].PositiveBonus;
                                    positiveBonus2 = (short)(positiveBonus2 + EffectInfo.GetValue(Target.Level, 0));
                                    target.SetUpdateFlag(155 + j, target.Resistances[j].Base);
                                }
                            }
                            j = (byte)unchecked((uint)(j + 1));
                        }
                    }
                    while (j <= 6u);
                    break;
                }
        }
        target1.SendCharacterUpdate(toNear: false);
    }

    /// <summary>
    /// SPELL_AURA_MOD_ATTACK_POWER
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_ATTACK_POWER(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        checked
        {
            WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
            switch (Action)
            {
                case AuraAction.AURA_UPDATE:
                    return;

                case AuraAction.AURA_ADD:
                    Target.AttackPowerMods += EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) * StackCount;
                    break;

                case AuraAction.AURA_REMOVE:
                case AuraAction.AURA_REMOVEBYDURATION:
                    Target.AttackPowerMods -= EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) * StackCount;
                    break;
                default:
                    break;
            }
            if (Target is WS_PlayerData.CharacterObject @object)
            {
                @object.SetUpdateFlag(165, @object.AttackPower);
                @object.SetUpdateFlag(166, @object.AttackPowerMods);
                var wS_Combat = WorldServiceLocator._WS_Combat;
                var objCharacter = @object;
                wS_Combat.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.BASE_ATTACK);
                var wS_Combat2 = WorldServiceLocator._WS_Combat;
                objCharacter = @object;
                wS_Combat2.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.OFF_ATTACK);
                @object.SendCharacterUpdate(toNear: false);
            }
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_RANGED_ATTACK_POWER
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_RANGED_ATTACK_POWER(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        checked
        {
            WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
            switch (Action)
            {
                case AuraAction.AURA_UPDATE:
                    return;

                case AuraAction.AURA_ADD:
                    Target.AttackPowerModsRanged += EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) * StackCount;
                    break;

                case AuraAction.AURA_REMOVE:
                case AuraAction.AURA_REMOVEBYDURATION:
                    Target.AttackPowerModsRanged -= EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) * StackCount;
                    break;
                default:
                    break;
            }
            if (Target is WS_PlayerData.CharacterObject @object)
            {
                @object.SetUpdateFlag(168, @object.AttackPowerRanged);
                @object.SetUpdateFlag(169, @object.AttackPowerModsRanged);
                var wS_Combat = WorldServiceLocator._WS_Combat;
                var objCharacter = @object;
                wS_Combat.CalculateMinMaxDamage(ref objCharacter, WeaponAttackType.RANGED_ATTACK);
                @object.SendCharacterUpdate(toNear: false);
            }
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_HEALING_DONE
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_HEALING_DONE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        if (Target is WS_PlayerData.CharacterObject @object)
        {
            WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
            var Value = EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0);
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
                    default:
                        break;
                }
            }
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_HEALING_DONE_PCT
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_HEALING_DONE_PCT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        if (Target is WS_PlayerData.CharacterObject @object)
        {
            WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
            var Value = (float)(EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) / 100.0);
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
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_DAMAGE_DONE
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_DAMAGE_DONE(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        if (Target is not WS_PlayerData.CharacterObject)
        {
            return;
        }

        WS_PlayerData.CharacterObject target1 = (WS_PlayerData.CharacterObject)Target;
        switch (Action)
        {
            case AuraAction.AURA_UPDATE:
                return;

            case AuraAction.AURA_ADD:
                {
                    var i = DamageTypes.DMG_PHYSICAL;
                    var i1 = (int)i;
                    do
                    {
                        checked
                        {
                            var i3 = (byte)i;
                            if (WorldServiceLocator._Functions.HaveFlag((uint)EffectInfo.MiscValue, i3))
                            {
                                if (EffectInfo.GetValue(Target.Level, 0) > 0)
                                {
                                    var target = target1;
                                    var i2 = (uint)i;
                                    target.spellDamage[i2].PositiveBonus += EffectInfo.GetValue(Target.Level, 0);
                                    target.SetUpdateFlag(1201 + i1, target.spellDamage[i2].PositiveBonus);
                                }
                                else
                                {
                                    var target = target1;
                                    target.spellDamage[(uint)i].NegativeBonus -= EffectInfo.GetValue(Target.Level, 0);
                                    target.SetUpdateFlag(1208 + i1, target.spellDamage[(uint)i].NegativeBonus);
                                }
                            }
                        }
                        i++;
                    }
                    while (i1 <= 6);
                    break;
                }
            case AuraAction.AURA_REMOVE:
            case AuraAction.AURA_REMOVEBYDURATION:
                {
                    var j = DamageTypes.DMG_PHYSICAL;
                    var j1 = (int)j;
                    do
                    {
                        checked
                        {
                            var j3 = (byte)j;
                            if (WorldServiceLocator._Functions.HaveFlag((uint)EffectInfo.MiscValue, j3))
                            {
                                if (EffectInfo.GetValue(Target.Level, 0) > 0)
                                {
                                    var target = target1;
                                    var j2 = (uint)j;
                                    target.spellDamage[j2].PositiveBonus -= EffectInfo.GetValue(Target.Level, 0);
                                    target.SetUpdateFlag(1201 + j1, target.spellDamage[j2].PositiveBonus);
                                }
                                else
                                {
                                    var target = target1;
                                    var j2 = (uint)j;
                                    target.spellDamage[j2].NegativeBonus += EffectInfo.GetValue(Target.Level, 0);
                                    target.SetUpdateFlag(1208 + j1, target.spellDamage[j2].NegativeBonus);
                                }
                            }
                        }
                        j++;
                    }
                    while (j1 <= 6);
                    break;
                }

            default:
                break;
        }
        target1.SendCharacterUpdate(toNear: false);
    }

    /// <summary>
    /// SPELL_AURA_MOD_DAMAGE_DONE_PCT
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_DAMAGE_DONE_PCT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        if (Target is not WS_PlayerData.CharacterObject)
        {
            return;
        }

        WS_PlayerData.CharacterObject target1 = (WS_PlayerData.CharacterObject)Target;
        switch (Action)
        {
            case AuraAction.AURA_UPDATE:
                return;

            case AuraAction.AURA_ADD:
                {
                    var i = DamageTypes.DMG_PHYSICAL;
                    var i1 = (int)i;
                    do
                    {
                        var miscValue = (uint)EffectInfo.MiscValue;
                        var i3 = (byte)i;
                        if (WorldServiceLocator._Functions.HaveFlag(checked(miscValue), i3))
                        {
                            var target = target1;
                            var i2 = (uint)i;
                            ref var modifier2 = ref target.spellDamage[i2].Modifier;
                            modifier2 = (float)(modifier2 + (EffectInfo.GetValue(Target.Level, 0) / 100.0));
                            target.SetUpdateFlag(checked(1215 + i1), target.spellDamage[i2].Modifier);
                        }
                        i++;
                    }
                    while (i1 <= 6);
                    break;
                }
            case AuraAction.AURA_REMOVE:
            case AuraAction.AURA_REMOVEBYDURATION:
                {
                    var j = DamageTypes.DMG_PHYSICAL;
                    var j1 = (int)j;
                    do
                    {
                        var miscValue = (uint)EffectInfo.MiscValue;
                        var j3 = (byte)j;
                        if (WorldServiceLocator._Functions.HaveFlag(checked(miscValue), j3))
                        {
                            var target = target1;
                            var j2 = (uint)j;
                            ref var modifier = ref target.spellDamage[j2].Modifier;
                            modifier = (float)(modifier - (EffectInfo.GetValue(Target.Level, 0) / 100.0));
                            target.SetUpdateFlag(checked(1215 + j1), target.spellDamage[j2].Modifier);
                        }
                        j++;
                    }
                    while (j1 <= 6);
                    break;
                }

            default:
                break;
        }
        target1.SendCharacterUpdate(toNear: false);
    }

    /// <summary>
    /// SPELL_AURA_EMPATHY
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_EMPATHY(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        switch (Action)
        {
            case AuraAction.AURA_UPDATE:
                break;

            case AuraAction.AURA_ADD:
                if (Target is WS_Creatures.CreatureObject @object && @object.CreatureInfo.CreatureType == 1)
                {
                    Packets.UpdatePacketClass packet = new();
                    Packets.UpdateClass tmpUpdate = new(188);
                    tmpUpdate.SetUpdateFlag(143, Target.cDynamicFlags | 0x10);
                    Packets.PacketClass packet3 = packet;
                    var updateObject = @object;
                    tmpUpdate.AddToPacket(ref packet3, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
                    packet3 = packet;
                    WS_PlayerData.CharacterObject caster = (WS_PlayerData.CharacterObject)Caster;
                    var client2 = caster.client;
                    client2.Send(ref packet3);
                    tmpUpdate.Dispose();
                    packet.Dispose();
                }
                break;

            case AuraAction.AURA_REMOVE:
            case AuraAction.AURA_REMOVEBYDURATION:
                if (Target is WS_Creatures.CreatureObject object1 && object1.CreatureInfo.CreatureType == 1)
                {
                    Packets.UpdatePacketClass packet2 = new();
                    Packets.UpdateClass tmpUpdate2 = new(188);
                    tmpUpdate2.SetUpdateFlag(143, Target.cDynamicFlags);
                    Packets.PacketClass packet3 = packet2;
                    var updateObject = object1;
                    tmpUpdate2.AddToPacket(ref packet3, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
                    packet3 = packet2;
                    WS_PlayerData.CharacterObject caster = (WS_PlayerData.CharacterObject)Caster;
                    var client = caster.client;
                    client.Send(ref packet3);
                    tmpUpdate2.Dispose();
                    packet2.Dispose();
                }
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_SILENCE
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
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
                    var aiScript = @object.aiScript;
                    WS_Base.BaseUnit Attacker = (WS_Base.BaseUnit)Caster;
                    aiScript.OnGenerateHate(ref Attacker, 1);
                }
                break;

            case AuraAction.AURA_REMOVE:
            case AuraAction.AURA_REMOVEBYDURATION:
                Target.Spell_Silenced = false;
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_PACIFY
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
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
            default:
                break;
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_LANGUAGE
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
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
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_POSSESS
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_POSSESS(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        if (Target is not WS_Creatures.CreatureObject and not WS_PlayerData.CharacterObject)
        {
            return;
        }
        switch (Action)
        {
            case AuraAction.AURA_UPDATE:
                break;

            case AuraAction.AURA_ADD:
                WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
                if (Target.Level <= EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0))
                {
                    Packets.UpdatePacketClass packet = new();
                    Packets.UpdateClass tmpUpdate = new(188);
                    tmpUpdate.SetUpdateFlag(10, Caster.GUID);
                    WS_PlayerData.CharacterObject caster1 = (WS_PlayerData.CharacterObject)Caster;
                    tmpUpdate.SetUpdateFlag(35, caster1.Faction);
                    Packets.PacketClass packet8;
                    WS_Creatures.CreatureObject target = (WS_Creatures.CreatureObject)Target;
                    switch (Target)
                    {
                        case WS_PlayerData.CharacterObject:
                            {
                                packet8 = packet;
                                WS_PlayerData.CharacterObject updateObject = (WS_PlayerData.CharacterObject)Target;
                                var updateClass5 = tmpUpdate;
                                updateClass5.AddToPacket(ref packet8, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
                                packet = (Packets.UpdatePacketClass)packet8;
                                break;
                            }

                        case WS_Creatures.CreatureObject:
                            {
                                packet8 = packet;
                                var updateObject2 = target;
                                var updateClass6 = tmpUpdate;
                                updateClass6.AddToPacket(ref packet8, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject2);
                                packet = (Packets.UpdatePacketClass)packet8;
                                break;
                            }

                        default:
                            break;
                    }
                    packet8 = packet;
                    var obj3 = Target;
                    obj3.SendToNearPlayers(ref packet8);
                    packet = (Packets.UpdatePacketClass)packet8;
                    packet.Dispose();
                    tmpUpdate.Dispose();
                    packet = new Packets.UpdatePacketClass();
                    tmpUpdate = new Packets.UpdateClass(188);
                    tmpUpdate.SetUpdateFlag(6, Target.GUID);
                    switch (Caster)
                    {
                        case WS_PlayerData.CharacterObject:
                            {
                                packet8 = packet;
                                var updateObject = caster1;
                                var updateClass7 = tmpUpdate;
                                updateClass7.AddToPacket(ref packet8, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
                                packet = (Packets.UpdatePacketClass)packet8;
                                break;
                            }

                        case WS_Creatures.CreatureObject:
                            {
                                packet8 = packet;
                                WS_Creatures.CreatureObject updateObject2 = (WS_Creatures.CreatureObject)Caster;
                                var updateClass8 = tmpUpdate;
                                updateClass8.AddToPacket(ref packet8, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject2);
                                packet = (Packets.UpdatePacketClass)packet8;
                                break;
                            }

                        default:
                            break;
                    }
                    packet8 = packet;
                    var obj4 = Caster;
                    obj4.SendToNearPlayers(ref packet8);
                    packet = (Packets.UpdatePacketClass)packet8;
                    packet.Dispose();
                    tmpUpdate.Dispose();
                    if (Caster is WS_PlayerData.CharacterObject @object)
                    {
                        var wS_Pets = WorldServiceLocator._WS_Pets;
                        var updateObject = @object;
                        wS_Pets.SendPetInitialize(ref updateObject, ref Target);
                        Caster = updateObject;
                        Packets.PacketClass packet6 = new(Opcodes.SMSG_DEATH_NOTIFY_OBSOLETE);
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
                        case WS_Creatures.CreatureObject:
                            target.aiScript.Reset();
                            break;

                        case WS_PlayerData.CharacterObject object1:
                            {
                                Packets.PacketClass packet3 = new(Opcodes.SMSG_DEATH_NOTIFY_OBSOLETE);
                                packet3.AddPackGUID(Target.GUID);
                                packet3.AddInt8(0);
                                object1.client.Send(ref packet3);
                                packet3.Dispose();
                                break;
                            }

                        default:
                            break;
                    }
                }
                break;

            case AuraAction.AURA_REMOVE:
            case AuraAction.AURA_REMOVEBYDURATION:
                {
                    Packets.UpdatePacketClass packet2 = new();
                    Packets.UpdateClass tmpUpdate2 = new(188);
                    tmpUpdate2.SetUpdateFlag(10, 0);
                    Packets.PacketClass packet8;
                    WS_Creatures.CreatureObject target1 = (WS_Creatures.CreatureObject)Target;
                    switch (Target)
                    {
                        case WS_PlayerData.CharacterObject:
                            {
                                WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
                                tmpUpdate2.SetUpdateFlag(35, target.Faction);
                                var updateClass = tmpUpdate2;
                                packet8 = packet2;
                                var updateObject = target;
                                updateClass.AddToPacket(ref packet8, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
                                packet2 = (Packets.UpdatePacketClass)packet8;
                                break;
                            }

                        case WS_Creatures.CreatureObject:
                            {
                                var target = target1;
                                tmpUpdate2.SetUpdateFlag(35, target.Faction);
                                var updateClass2 = tmpUpdate2;
                                packet8 = packet2;
                                var updateObject2 = target;
                                updateClass2.AddToPacket(ref packet8, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject2);
                                packet2 = (Packets.UpdatePacketClass)packet8;
                                break;
                            }
                    }
                    var obj = Target;
                    packet8 = packet2;
                    obj.SendToNearPlayers(ref packet8);
                    packet2 = (Packets.UpdatePacketClass)packet8;
                    packet2.Dispose();
                    tmpUpdate2.Dispose();
                    packet2 = new Packets.UpdatePacketClass();
                    tmpUpdate2 = new Packets.UpdateClass(188);
                    tmpUpdate2.SetUpdateFlag(6, 0);
                    switch (Caster)
                    {
                        case WS_PlayerData.CharacterObject:
                            {
                                var updateClass3 = tmpUpdate2;
                                packet8 = packet2;
                                WS_PlayerData.CharacterObject updateObject = (WS_PlayerData.CharacterObject)Caster;
                                updateClass3.AddToPacket(ref packet8, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
                                break;
                            }

                        case WS_Creatures.CreatureObject:
                            {
                                var updateClass4 = tmpUpdate2;
                                packet8 = packet2;
                                WS_Creatures.CreatureObject updateObject2 = (WS_Creatures.CreatureObject)Caster;
                                updateClass4.AddToPacket(ref packet8, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject2);
                                break;
                            }

                        default:
                            break;
                    }
                    var obj2 = Caster;
                    packet8 = packet2;
                    obj2.SendToNearPlayers(ref packet8);
                    packet2 = (Packets.UpdatePacketClass)packet8;
                    packet2.Dispose();
                    tmpUpdate2.Dispose();
                    if (Caster is WS_PlayerData.CharacterObject @object)
                    {
                        Packets.PacketClass packet5 = new(Opcodes.SMSG_PET_SPELLS);
                        packet5.AddUInt64(0uL);
                        @object.client.Send(ref packet5);
                        packet5.Dispose();
                        Packets.PacketClass packet7 = new(Opcodes.SMSG_DEATH_NOTIFY_OBSOLETE);
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
                        case WS_Creatures.CreatureObject:
                            target1.aiScript.State = AIState.AI_ATTACKING;
                            break;

                        case WS_PlayerData.CharacterObject object1:
                            {
                                Packets.PacketClass packet4 = new(Opcodes.SMSG_DEATH_NOTIFY_OBSOLETE);
                                packet4.AddPackGUID(Target.GUID);
                                packet4.AddInt8(1);
                                object1.client.Send(ref packet4);
                                packet4.Dispose();
                                break;
                            }

                        default:
                            break;
                    }
                    break;
                }

            default:
                break;
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_THREAT
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
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
            default:
                break;
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_TOTAL_THREAT
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
    public void SPELL_AURA_MOD_TOTAL_THREAT(ref WS_Base.BaseUnit Target, ref WS_Base.BaseObject Caster, ref SpellEffect EffectInfo, int SpellID, int StackCount, AuraAction Action)
    {
        int Value = default;
        WS_Base.BaseUnit caster = (WS_Base.BaseUnit)Caster;
        switch (Action)
        {
            case AuraAction.AURA_UPDATE:
                return;

            case AuraAction.AURA_ADD:
                Value = (Target is not WS_PlayerData.CharacterObject) ? EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0) : EffectInfo.GetValue(Target.Level, 0);
                break;

            case AuraAction.AURA_REMOVE:
            case AuraAction.AURA_REMOVEBYDURATION:
                checked
                {
                    Value = (Target is not WS_PlayerData.CharacterObject) ? (-EffectInfo.GetValue(Level: caster.Level, ComboPoints: 0)) : (-EffectInfo.GetValue(Target.Level, 0));
                    break;
                }

            default:
                break;
        }
        switch (Target)
        {
            case WS_PlayerData.CharacterObject:
                {
                    WS_PlayerData.CharacterObject target = (WS_PlayerData.CharacterObject)Target;
                    foreach (var CreatureGUID in target.creaturesNear)
                    {
                        if (WorldServiceLocator._WorldServer.WORLD_CREATUREs[CreatureGUID].aiScript != null && WorldServiceLocator._WorldServer.WORLD_CREATUREs[CreatureGUID].aiScript.aiHateTable.ContainsKey(Target))
                        {
                            WorldServiceLocator._WorldServer.WORLD_CREATUREs[CreatureGUID].aiScript.OnGenerateHate(ref Target, Value);
                        }
                    }

                    break;
                }

            default:
                WS_Creatures.CreatureObject target1 = (WS_Creatures.CreatureObject)Target;
                if (target1.aiScript != null && target1.aiScript.aiHateTable.ContainsKey(caster))
                {
                    var aiScript = target1.aiScript;
                    var Attacker = caster;
                    aiScript.OnGenerateHate(ref Attacker, Value);
                    Caster = Attacker;
                }

                break;
        }
    }

    /// <summary>
    /// SPELL_AURA_MOD_TAUNT
    /// </summary>
    /// <param name="Target"></param>
    /// <param name="Caster"></param>
    /// <param name="EffectInfo"></param>
    /// <param name="SpellID"></param>
    /// <param name="StackCount"></param>
    /// <param name="Action"></param>
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
                        var aiScript2 = @object.aiScript;
                        WS_Base.BaseUnit Attacker = (WS_Base.BaseUnit)Caster;
                        aiScript2.OnGenerateHate(ref Attacker, 9999999);
                        Caster = Attacker;
                        break;
                    }
                case AuraAction.AURA_REMOVE:
                case AuraAction.AURA_REMOVEBYDURATION:
                    {
                        var aiScript = @object.aiScript;
                        WS_Base.BaseUnit Attacker = (WS_Base.BaseUnit)Caster;
                        aiScript.OnGenerateHate(ref Attacker, -9999999);
                        Caster = Attacker;
                        break;
                    }

                default:
                    break;
            }
        }
    }

    /// <summary>
    /// Function CheckDuelDistance
    /// </summary>
    /// <param name="objCharacter"></param>
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
                Packets.PacketClass packet2 = new(Opcodes.SMSG_DUEL_OUTOFBOUNDS);
                objCharacter.client.Send(ref packet2);
                packet2.Dispose();
                objCharacter.DuelOutOfBounds = 10;
            }
        }
        else if (objCharacter.DuelOutOfBounds != 11)
        {
            objCharacter.DuelOutOfBounds = 11;
            Packets.PacketClass packet = new(Opcodes.SMSG_DUEL_INBOUNDS);
            objCharacter.client.Send(ref packet);
            packet.Dispose();
        }
    }

    /// <summary>
    /// Function DuelComplete
    /// </summary>
    /// <param name="Winner"></param>
    /// <param name="Loser"></param>
    public void DuelComplete(ref WS_PlayerData.CharacterObject Winner, ref WS_PlayerData.CharacterObject Loser)
    {
        if (Winner == null || Loser == null)
        {
            return;
        }
        Packets.PacketClass response = new(Opcodes.SMSG_DUEL_COMPLETE);
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
            var num = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1;
            for (var i = 0; i <= num; i++)
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
            Packets.PacketClass packet = new(Opcodes.SMSG_DUEL_WINNER);
            packet.AddInt8(0);
            packet.AddString(Winner.Name);
            packet.AddInt8(1);
            packet.AddString(Loser.Name);
            Winner.SendToNearPlayers(ref packet);
            packet.Dispose();
            Packets.PacketClass SMSG_EMOTE = new(Opcodes.SMSG_EMOTE);
            SMSG_EMOTE.AddInt32(20);
            SMSG_EMOTE.AddUInt64(Loser.GUID);
            Loser.SendToNearPlayers(ref SMSG_EMOTE);
            SMSG_EMOTE.Dispose();
            var tmpCharacter = Winner;
            Loser.DuelPartner = null;
            tmpCharacter.DuelPartner = null;
        }
    }

    /// <summary>
    /// Packet On_CMSG_DUEL_ACCEPTED
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="client"></param>
    public void On_CMSG_DUEL_ACCEPTED(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 13)
        {
            packet.GetInt16();
            var GUID = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_DUEL_ACCEPTED [{2:X}]", client.IP, client.Port, GUID);
            if (client.Character.DuelArbiter == GUID)
            {
                var c1 = client.Character.DuelPartner;
                var c2 = client.Character;
                c1.DuelArbiter = GUID;
                c2.DuelArbiter = GUID;
                c1.SetUpdateFlag(188, c1.DuelArbiter);
                c2.SetUpdateFlag(188, c2.DuelArbiter);
                c2.SendCharacterUpdate();
                c1.SendCharacterUpdate();
                Packets.PacketClass response = new(Opcodes.SMSG_DUEL_COUNTDOWN);
                response.AddInt32(3000);
                c1.client.SendMultiplyPackets(ref response);
                c2.client.SendMultiplyPackets(ref response);
                response.Dispose();
                Thread StartDuel = new(c2.StartDuel)
                {
                    Name = "Duel timer"
                };
                StartDuel.Start();
            }
        }
    }

    /// <summary>
    /// Packet On_CMSG_DUEL_CANCELLED
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="client"></param>
    public void On_CMSG_DUEL_CANCELLED(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 13)
        {
            packet.GetInt16();
            var GUID = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_DUEL_CANCELLED [{2:X}]", client.IP, client.Port, GUID);
            WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[client.Character.DuelArbiter].Despawn();
            client.Character.DuelArbiter = 0uL;
            client.Character.DuelPartner.DuelArbiter = 0uL;
            Packets.PacketClass response = new(Opcodes.SMSG_DUEL_COMPLETE);
            response.AddInt8(0);
            client.Character.client.SendMultiplyPackets(ref response);
            client.Character.DuelPartner.client.SendMultiplyPackets(ref response);
            response.Dispose();
            client.Character.DuelPartner.DuelPartner = null;
            client.Character.DuelPartner = null;
        }
    }

    /// <summary>
    /// Packet On_CMSG_RESURRECT_RESPONSE
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="client"></param>
    public void On_CMSG_RESURRECT_RESPONSE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) < 14)
        {
            return;
        }
        packet.GetInt16();
        var GUID = packet.GetUInt64();
        var Status = packet.GetInt8();
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

    /// <summary>
    /// Packet On_CMSG_CAST_SPELL
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="client"></param>
    public void On_CMSG_CAST_SPELL(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        packet.GetInt16();
        var spellID = packet.GetInt32();
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CSMG_CAST_SPELL [spellID={2}]", client.IP, client.Port, spellID);
        if (client.Character == null)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "[{0}:{1}] WS_Spells:On_CMSG_CAST_SPELL Null Character Object [spellID={2}]", client.IP, client.Port, spellID);
            return;
        }
        if (client.Character != null)
        {
            if (!client.Character.HaveSpell(spellID))
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "[{0}:{1}] CHEAT: Character {2} casting unlearned spell {3}!", client.IP, client.Port, client.Character.Name, spellID);
                return;
            }
        }

        if (!SPELLs.ContainsKey(spellID))
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "[{0}:{1}] Character tried to cast a spell that didn't exist: {2}!", client.IP, client.Port, spellID);
            return;
        }
        if (client.Character != null)
        {
            var spellCooldown = client.Character.Spells[spellID].Cooldown;
            if (spellCooldown >= 0)
            {
                var timeNow = WorldServiceLocator._Functions.GetTimestamp(DateTime.Now);
                if (timeNow < spellCooldown)
                {
                    return;
                }
                client.Character.Spells[spellID].Cooldown = 0u;
                client.Character.Spells[spellID].CooldownItem = 0;
            }
        }
        var SpellType = CurrentSpellTypes.CURRENT_GENERIC_SPELL;
        if (SPELLs[spellID].IsAutoRepeat)
        {
            SpellType = CurrentSpellTypes.CURRENT_AUTOREPEAT_SPELL;
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
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error casting auto-shoot {0}.{1}", spellID, Environment.NewLine + ex);
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
        SpellTargets Targets = new();
        var castResult = SpellFailedReason.SPELL_FAILED_ERROR;
        try
        {
            var spellTargets = Targets;
            WS_Base.BaseObject Caster = client.Character;
            spellTargets.ReadTargets(ref packet, ref Caster);
            castResult = SPELLs[spellID].CanCast(ref client.Character, Targets, FirstCheck: true);
            var spellType = (int)SpellType;
            if (client.Character.spellCasted[spellType] != null && !client.Character.spellCasted[spellType].Finished)
            {
                castResult = SpellFailedReason.SPELL_FAILED_SPELL_IN_PROGRESS;
            }
            if (castResult == SpellFailedReason.SPELL_NO_ERROR)
            {
                ref var character = ref client.Character;
                Caster = character;
                CastSpellParameters castSpellParameters = new(ref Targets, ref Caster, spellID);
                character = (WS_PlayerData.CharacterObject)Caster;
                var tmpSpell = castSpellParameters;
                client.Character.spellCasted[spellType] = tmpSpell;
                ThreadPool.QueueUserWorkItem(tmpSpell.Cast);
            }
            else
            {
                SendCastResult(castResult, ref client, spellID);
            }
        }
        catch (Exception e)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error casting spell {0}.{1}", spellID, Environment.NewLine + e);
            SendCastResult(castResult, ref client, spellID);
        }
    }

    /// <summary>
    /// Packet On_CMSG_CANCEL_CAST
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="client"></param>
    public void On_CMSG_CANCEL_CAST(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 9 && client.Character != null)
        {
            packet.GetInt16();
            var SpellID = packet.GetInt32();
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

    /// <summary>
    /// PACKET On_CMSG_CANCEL_AUTO_REPEAT_SPELL
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="client"></param>
    public void On_CMSG_CANCEL_AUTO_REPEAT_SPELL(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CANCEL_AUTO_REPEAT_SPELL", client.IP, client.Port);
        client.Character.FinishSpell(CurrentSpellTypes.CURRENT_AUTOREPEAT_SPELL);
    }

    /// <summary>
    /// Packet On_CMSG_CANCEL_CHANNELLING
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="client"></param>
    public void On_CMSG_CANCEL_CHANNELLING(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CANCEL_CHANNELLING", client.IP, client.Port);
        client.Character.FinishSpell(CurrentSpellTypes.CURRENT_CHANNELED_SPELL);
    }

    /// <summary>
    /// Packet On_CMSG_CANCEL_AURA
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="client"></param>
    public void On_CMSG_CANCEL_AURA(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 9)
        {
            packet.GetInt16();
            var spellID = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CANCEL_AURA [spellID={2}]", client.IP, client.Port, spellID);
            client.Character.RemoveAuraBySpell(spellID);
        }
    }

    /// <summary>
    /// Packet On_CMSG_LEARN_TALENT
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="client"></param>
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
                var TalentID = packet.GetInt32();
                var RequestedRank = packet.GetInt32();
                var CurrentTalentPoints = client.Character.TalentPoints;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_LEARN_TALENT [{2}:{3}]", client.IP, client.Port, TalentID, RequestedRank);
                if (CurrentTalentPoints == 0 || RequestedRank > 4 || (RequestedRank > 0 && !client.Character.HaveSpell(WorldServiceLocator._WS_DBCDatabase.Talents[TalentID].RankID[RequestedRank - 1])))
                {
                    return;
                }
                var k = 0;
                do
                {
                    if (WorldServiceLocator._WS_DBCDatabase.Talents[TalentID].RequiredTalent[k] > 0)
                    {
                        var HasEnoughRank = false;
                        var DependsOn = WorldServiceLocator._WS_DBCDatabase.Talents[TalentID].RequiredTalent[k];
                        var num = WorldServiceLocator._WS_DBCDatabase.Talents[TalentID].RequiredPoints[k];
                        for (var j = num; j <= 4; j++)
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
                var SpentPoints = 0;
                if (WorldServiceLocator._WS_DBCDatabase.Talents[TalentID].Row > 0)
                {
                    foreach (var TalentInfo in WorldServiceLocator._WS_DBCDatabase.Talents)
                    {
                        if (WorldServiceLocator._WS_DBCDatabase.Talents[TalentID].TalentTab != TalentInfo.Value.TalentTab)
                        {
                            continue;
                        }
                        var i = 0;
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
                var SpellID = WorldServiceLocator._WS_DBCDatabase.Talents[TalentID].RankID[RequestedRank];
                if (SpellID != 0 && !client.Character.HaveSpell(SpellID))
                {
                    client.Character.LearnSpell(SpellID);
                    if (SPELLs.ContainsKey(SpellID) && SPELLs[SpellID].IsPassive)
                    {
                        client.Character.ApplySpell(SpellID);
                    }
                    if (RequestedRank > 0)
                    {
                        var ReSpellID = WorldServiceLocator._WS_DBCDatabase.Talents[TalentID].RankID[RequestedRank - 1];
                        client.Character.UnLearnSpell(ReSpellID);
                        client.Character.RemoveAuraBySpell(ReSpellID);
                    }
                    client.Character.TalentPoints--;
                    client.Character.SetUpdateFlag(1102, client.Character.TalentPoints);
                    client.Character.SendCharacterUpdate();
                    client.Character.SaveCharacter();
                }
            }
            catch (Exception e)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error learning talent: {0}{1}", Environment.NewLine, e.ToString());
            }
        }
    }

    /// <summary>
    /// Function SendLoot
    /// </summary>
    /// <param name="Player"></param>
    /// <param name="GUID"></param>
    /// <param name="LootingType"></param>
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
                default:
                    break;
            }
        }
        WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].LootObject(ref Player, LootingType);
    }
}
