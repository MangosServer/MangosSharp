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
using System.Runtime.InteropServices;
using System.Threading;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Item;
using Mangos.Common.Enums.Player;
using Mangos.Common.Enums.Spell;
using Mangos.Common.Enums.Unit;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Objects;
using Mangos.World.Player;
using Mangos.World.Server;
using Mangos.World.Spells;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Handlers
{
    public class WS_Combat
    {

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public void DoEmote(int AnimationID, ref WS_Base.BaseObject Unit)
        {
            // EMOTE_ONESHOT_WOUNDCRITICAL
            // EMOTE_ONESHOT_PARRYSHIELD
            // EMOTE_ONESHOT_PARRYUNARMED

            var packet = new Packets.PacketClass(OPCODES.SMSG_EMOTE);
            packet.AddInt32(AnimationID);
            packet.AddUInt64(Unit.GUID);
            Unit.SendToNearPlayers(ref packet);
            packet.Dispose();
        }

        public float GetWeaponDmg(ref WS_PlayerData.CharacterObject objCharacter, WeaponAttackType AttackType, bool MaxDmg)
        {
            byte WepSlot;
            switch (AttackType)
            {
                case var @case when @case == WeaponAttackType.BASE_ATTACK:
                    {
                        WepSlot = EquipmentSlots.EQUIPMENT_SLOT_MAINHAND;
                        break;
                    }

                case var case1 when case1 == WeaponAttackType.OFF_ATTACK:
                    {
                        WepSlot = EquipmentSlots.EQUIPMENT_SLOT_OFFHAND;
                        break;
                    }

                case var case2 when case2 == WeaponAttackType.RANGED_ATTACK:
                    {
                        WepSlot = EquipmentSlots.EQUIPMENT_SLOT_RANGED;
                        break;
                    }

                default:
                    {
                        return 0f;
                    }
            }

            if (objCharacter.Items.ContainsKey(WepSlot) == false || objCharacter.Items[WepSlot].ItemInfo.ObjectClass != ITEM_CLASS.ITEM_CLASS_WEAPON || objCharacter.Items[WepSlot].IsBroken())
                return 0.0f;
            float Dmg = 0f;
            for (byte i = 0; i <= 4; i++)
            {
                if (MaxDmg)
                {
                    Dmg += objCharacter.Items[WepSlot].ItemInfo.Damage[i].Maximum;
                }
                else
                {
                    Dmg += objCharacter.Items[WepSlot].ItemInfo.Damage[i].Minimum;
                }
            }

            return Dmg;
        }

        public float GetAPMultiplier(ref WS_Base.BaseUnit objCharacter, WeaponAttackType AttackType, bool Normalized)
        {
            if (Normalized == false || !(objCharacter is WS_PlayerData.CharacterObject))
            {
                switch (AttackType)
                {
                    case var @case when @case == WeaponAttackType.BASE_ATTACK:
                        {
                            return ((WS_Creatures.CreatureObject)objCharacter).CreatureInfo.BaseAttackTime / 1000.0f;
                        }

                    case var case1 when case1 == WeaponAttackType.RANGED_ATTACK:
                        {
                            return ((WS_Creatures.CreatureObject)objCharacter).CreatureInfo.BaseRangedAttackTime / 1000.0f;
                        }

                    default:
                        {
                            return 0.0f;
                        }
                }
            }

            ItemObject Weapon;
            switch (AttackType)
            {
                case var case2 when case2 == WeaponAttackType.BASE_ATTACK:
                    {
                        if (((WS_PlayerData.CharacterObject)objCharacter).Items.ContainsKey(EquipmentSlots.EQUIPMENT_SLOT_MAINHAND) == false)
                            return 2.4f; // Fist attack
                        Weapon = ((WS_PlayerData.CharacterObject)objCharacter).Items(EquipmentSlots.EQUIPMENT_SLOT_MAINHAND);
                        break;
                    }

                case var case3 when case3 == WeaponAttackType.OFF_ATTACK:
                    {
                        if (((WS_PlayerData.CharacterObject)objCharacter).Items.ContainsKey(EquipmentSlots.EQUIPMENT_SLOT_OFFHAND) == false)
                            return 2.4f; // Fist attack
                        Weapon = ((WS_PlayerData.CharacterObject)objCharacter).Items(EquipmentSlots.EQUIPMENT_SLOT_OFFHAND);
                        break;
                    }

                case var case4 when case4 == WeaponAttackType.RANGED_ATTACK:
                    {
                        if (((WS_PlayerData.CharacterObject)objCharacter).Items.ContainsKey(EquipmentSlots.EQUIPMENT_SLOT_RANGED) == false)
                            return 0.0f;
                        Weapon = ((WS_PlayerData.CharacterObject)objCharacter).Items(EquipmentSlots.EQUIPMENT_SLOT_RANGED);
                        break;
                    }

                default:
                    {
                        return 0.0f;
                    }
            }

            if (Weapon is null || Weapon.ItemInfo.ObjectClass != ITEM_CLASS.ITEM_CLASS_WEAPON)
            {
                if (AttackType == WeaponAttackType.RANGED_ATTACK)
                    return 0.0f;
                return 2.4f;
            }

            switch (Weapon.ItemInfo.InventoryType)
            {
                case var case5 when case5 == INVENTORY_TYPES.INVTYPE_TWOHAND_WEAPON:
                    {
                        return 3.3f;
                    }

                case var case6 when case6 == INVENTORY_TYPES.INVTYPE_RANGED:
                case var case7 when case7 == INVENTORY_TYPES.INVTYPE_RANGEDRIGHT:
                case var case8 when case8 == INVENTORY_TYPES.INVTYPE_THROWN:
                    {
                        return 2.8f;
                    }

                default:
                    {
                        if (Weapon.ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_DAGGER)
                            return 1.7f;
                        return 2.4f;
                    }
            }
        }

        public void CalculateMinMaxDamage(ref WS_PlayerData.CharacterObject objCharacter, WeaponAttackType AttackType)
        {
            float AttSpeed = GetAPMultiplier(ref objCharacter, AttackType, true);
            float BasePercent = 1f;
            float BaseValue;
            switch (AttackType)
            {
                case var @case when @case == WeaponAttackType.BASE_ATTACK:
                case var case1 when case1 == WeaponAttackType.OFF_ATTACK:
                    {
                        BaseValue = objCharacter.AttackPower + objCharacter.AttackPowerMods;
                        break;
                    }

                case var case2 when case2 == WeaponAttackType.RANGED_ATTACK:
                    {
                        BaseValue = objCharacter.AttackPowerRanged + objCharacter.AttackPowerModsRanged;
                        break;
                    }

                default:
                    {
                        return;
                    }
            }

            BaseValue = BaseValue / 14.0f * AttSpeed;
            float WepMin = GetWeaponDmg(ref objCharacter, AttackType, false);
            float WepMax = GetWeaponDmg(ref objCharacter, AttackType, true);
            if (AttackType == WeaponAttackType.RANGED_ATTACK) // Add ammo dps
            {
                if (objCharacter.AmmoID > 0)
                {
                    float AmmoDmg = objCharacter.AmmoDPS / (1f / objCharacter.AmmoMod) * AttSpeed;
                    WepMin += AmmoDmg;
                    WepMax += AmmoDmg;
                }
            }
            else if (objCharacter.ShapeshiftForm == ShapeshiftForm.FORM_BEAR || objCharacter.ShapeshiftForm == ShapeshiftForm.FORM_DIREBEAR || objCharacter.ShapeshiftForm == ShapeshiftForm.FORM_CAT)
            {
                WepMin = (float)(WepMin + objCharacter.Level * 0.85d * AttSpeed);
                WepMax = (float)(WepMax + objCharacter.Level * 0.85d * AttSpeed);
            }

            float MinDamage = (BaseValue + WepMin) * BasePercent;
            float MaxDamage = (BaseValue + WepMax) * BasePercent;
            switch (AttackType)
            {
                case var case3 when case3 == WeaponAttackType.BASE_ATTACK:
                    {
                        objCharacter.Damage.Minimum = MinDamage;
                        objCharacter.Damage.Maximum = MaxDamage;
                        objCharacter.SetUpdateFlag(EUnitFields.UNIT_FIELD_MINDAMAGE, objCharacter.Damage.Minimum);
                        objCharacter.SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXDAMAGE, objCharacter.Damage.Maximum);
                        break;
                    }

                case var case4 when case4 == WeaponAttackType.OFF_ATTACK:
                    {
                        objCharacter.OffHandDamage.Minimum = MinDamage;
                        objCharacter.OffHandDamage.Maximum = MaxDamage;
                        objCharacter.SetUpdateFlag(EUnitFields.UNIT_FIELD_MINOFFHANDDAMAGE, objCharacter.OffHandDamage.Minimum);
                        objCharacter.SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXOFFHANDDAMAGE, objCharacter.OffHandDamage.Maximum);
                        break;
                    }

                case var case5 when case5 == WeaponAttackType.RANGED_ATTACK:
                    {
                        objCharacter.RangedDamage.Minimum = MinDamage;
                        objCharacter.RangedDamage.Maximum = MaxDamage;
                        objCharacter.SetUpdateFlag(EUnitFields.UNIT_FIELD_MINRANGEDDAMAGE, objCharacter.RangedDamage.Minimum);
                        objCharacter.SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXRANGEDDAMAGE, objCharacter.RangedDamage.Maximum);
                        break;
                    }
            }
        }

        public DamageInfo CalculateDamage(ref WS_Base.BaseUnit Attacker, ref WS_Base.BaseUnit Victim, bool DualWield, bool Ranged, WS_Spells.SpellInfo Ability = null, WS_Spells.SpellEffect Effect = null)
        {
            var result = default(DamageInfo);

            // DONE: Initialize result
            result.victimState = AttackVictimState.VICTIMSTATE_NORMAL;
            result.Blocked = 0;
            result.Absorbed = 0;
            result.Turn = 0;
            result.HitInfo = 0;
            if (DualWield)
                result.HitInfo = result.HitInfo | AttackHitState.HITINFO_LEFTSWING;
            if (Ability is object)
            {
                result.DamageType = Ability.School;
            }
            // TODO: Get creature damage type
            else if (Attacker is WS_PlayerData.CharacterObject)
            {
                {
                    var withBlock = (WS_PlayerData.CharacterObject)Attacker;
                    if (Ranged)
                    {
                        if (withBlock.Items.ContainsKey(EquipmentSlots.EQUIPMENT_SLOT_RANGED))
                        {
                            result.DamageType = withBlock.Items(EquipmentSlots.EQUIPMENT_SLOT_RANGED).ItemInfo.Damage(0).Type;
                        }
                    }
                    else if (DualWield)
                    {
                        if (withBlock.Items.ContainsKey(EquipmentSlots.EQUIPMENT_SLOT_OFFHAND))
                        {
                            result.DamageType = withBlock.Items(EquipmentSlots.EQUIPMENT_SLOT_OFFHAND).ItemInfo.Damage(0).Type;
                        }
                    }
                    else if (withBlock.Items.ContainsKey(EquipmentSlots.EQUIPMENT_SLOT_MAINHAND))
                    {
                        result.DamageType = withBlock.Items(EquipmentSlots.EQUIPMENT_SLOT_MAINHAND).ItemInfo.Damage(0).Type;
                    }
                }
            }
            else
            {
                result.DamageType = DamageTypes.DMG_PHYSICAL;
            }

            if (Victim is WS_Creatures.CreatureObject && ((WS_Creatures.CreatureObject)Victim).aiScript is object)
            {
                if (((WS_Creatures.CreatureObject)Victim).aiScript.State == AIState.AI_MOVING_TO_SPAWN)
                {
                    result.HitInfo = result.HitInfo | AttackHitState.HIT_MISS;
                    return result;
                }
            }

            // DONE: Miss chance calculation
            // http://www.wowwiki.com/Formulas:Weapon_Skill
            int skillDiference = GetSkillWeapon(ref Attacker, DualWield);

            // http://www.wowwiki.com/Defense
            skillDiference -= GetSkillDefence(ref Victim);
            if (Victim is WS_PlayerData.CharacterObject)
                ((WS_PlayerData.CharacterObject)Victim).UpdateSkill(SKILL_IDs.SKILL_DEFENSE);

            // DONE: Final calculations
            float chanceToMiss = GetBasePercentMiss(ref Attacker, skillDiference);
            float chanceToCrit = GetBasePercentCrit(ref Attacker, skillDiference);
            float chanceToBlock = GetBasePercentBlock(ref Victim, skillDiference);
            float chanceToParry = GetBasePercentParry(ref Victim, skillDiference);
            float chanceToDodge = GetBasePercentDodge(ref Victim, skillDiference);

            // DONE: Glancing blow (only VS Creatures)
            short chanceToGlancingBlow = 0;
            if (Attacker is WS_PlayerData.CharacterObject && Victim is WS_Creatures.CreatureObject && Attacker.Level > Victim.Level + 2 && skillDiference <= -15)
                chanceToGlancingBlow = (short)((Victim.Level - Attacker.Level) * 10);

            // DONE: Crushing blow, fix real damage (only for Creatures)
            short chanceToCrushingBlow = 0;
            if (Attacker is WS_Creatures.CreatureObject && Victim is WS_PlayerData.CharacterObject && Ability is null && Attacker.Level > Victim.Level + 2)
                chanceToCrushingBlow = (short)(skillDiference * 2.0f - 15f);

            // DONE: Some limitations
            if (chanceToMiss > 60.0f)
                chanceToMiss = 60.0f;
            if (chanceToGlancingBlow > 40.0f)
                chanceToGlancingBlow = (short)40.0f;
            if (chanceToMiss < 0.0f)
                chanceToMiss = 0.0f;
            if (chanceToCrit < 0.0f)
                chanceToCrit = 0.0f;
            if (chanceToBlock < 0.0f)
                chanceToBlock = 0.0f;
            if (chanceToParry < 0.0f)
                chanceToParry = 0.0f;
            if (chanceToDodge < 0.0f)
                chanceToDodge = 0.0f;
            if (chanceToGlancingBlow < 0.0f)
                chanceToGlancingBlow = (short)0.0f;
            if (chanceToCrushingBlow < 0.0f)
                chanceToCrushingBlow = (short)0.0f;

            // DONE: Always crit against a sitting target
            if (Victim is WS_PlayerData.CharacterObject && ((WS_PlayerData.CharacterObject)Victim).StandState != 0)
            {
                chanceToCrit = 100.0f;
                chanceToCrushingBlow = (short)0.0f;
            }

            // DONE: No glancing with ranged weapon
            if (Ranged)
            {
                chanceToGlancingBlow = (short)0.0f;
            }

            // DONE: Calculating the damage
            GetDamage(ref Attacker, DualWield, ref result);
            if (Effect is object)
            {
                result.Damage += Effect.get_GetValue(Attacker.Level, 0);
            }

            // DONE: Damage reduction
            float DamageReduction = Victim.GetDamageReduction(ref Attacker, result.DamageType, result.Damage);
            result.Damage = (int)(result.Damage - result.Damage * DamageReduction);

            // TODO: More aurastates!
            // DONE: Rolling the dice
            float roll = (float)(WorldServiceLocator._WorldServer.Rnd.Next(0, 10000) / 100d);
            switch (roll)
            {
                case var @case when @case < chanceToMiss:
                    {
                        // DONE: Miss attack
                        result.Damage = 0;
                        result.HitInfo = result.HitInfo | AttackHitState.HITINFO_MISS;
                        break;
                    }

                case var case1 when case1 < chanceToMiss + chanceToDodge:
                    {
                        // DONE: Dodge attack
                        result.Damage = 0;
                        result.victimState = AttackVictimState.VICTIMSTATE_DODGE;
                        DoEmote(Emotes.ONESHOT_PARRYUNARMED, ref Victim);
                        // TODO: Remove after 5 secs?
                        Victim.AuraState = Victim.AuraState | SpellAuraStates.AURASTATE_FLAG_DODGE_BLOCK;
                        if (Victim is WS_PlayerData.CharacterObject)
                        {
                            ((WS_PlayerData.CharacterObject)Victim).SetUpdateFlag(EUnitFields.UNIT_FIELD_AURASTATE, Victim.AuraState);
                            ((WS_PlayerData.CharacterObject)Victim).SendCharacterUpdate();
                        }

                        break;
                    }

                case var case2 when case2 < chanceToMiss + chanceToDodge + chanceToParry:
                    {
                        // DONE: Parry attack
                        result.Damage = 0;
                        result.victimState = AttackVictimState.VICTIMSTATE_PARRY;
                        DoEmote(Emotes.ONESHOT_PARRYUNARMED, ref Victim);
                        // TODO: Remove after 5 secs?
                        Victim.AuraState = Victim.AuraState | SpellAuraStates.AURASTATE_FLAG_PARRY;
                        if (Victim is WS_PlayerData.CharacterObject)
                        {
                            ((WS_PlayerData.CharacterObject)Victim).SetUpdateFlag(EUnitFields.UNIT_FIELD_AURASTATE, Victim.AuraState);
                            ((WS_PlayerData.CharacterObject)Victim).SendCharacterUpdate();
                        }

                        break;
                    }

                case var case3 when case3 < chanceToMiss + chanceToDodge + chanceToParry + chanceToGlancingBlow:
                    {
                        // DONE: Glancing Blow
                        result.Damage = (int)(result.Damage - Conversion.Fix(skillDiference * 0.03f * result.Damage));
                        result.HitInfo = result.HitInfo | AttackHitState.HITINFO_HITANIMATION;
                        result.HitInfo = result.HitInfo | AttackHitState.HIT_GLANCING_BLOW;
                        break;
                    }

                case var case4 when case4 < chanceToMiss + chanceToDodge + chanceToParry + chanceToGlancingBlow + chanceToBlock:
                    {
                        // DONE: Block (http://www.wowwiki.com/Formulas:Block)
                        if (Victim is WS_PlayerData.CharacterObject)
                        {
                            result.Blocked = (int)(((WS_PlayerData.CharacterObject)Victim).combatBlockValue + ((WS_PlayerData.CharacterObject)Victim).Strength.Base / 20d);     // ... hits you for 60. (40 blocked)
                            if (((WS_PlayerData.CharacterObject)Victim).combatBlockValue != 0)
                            {
                                DoEmote(Emotes.ONESHOT_PARRYSHIELD, ref Victim);
                            }
                            else
                            {
                                DoEmote(Emotes.ONESHOT_PARRYUNARMED, ref Victim);
                            }

                            result.victimState = AttackVictimState.VICTIMSTATE_BLOCKS;
                        }

                        result.HitInfo = result.HitInfo | AttackHitState.HITINFO_HITANIMATION;
                        if (result.GetDamage <= 0)
                            result.HitInfo = result.HitInfo | AttackHitState.HITINFO_BLOCK;
                        break;
                    }

                case var case5 when case5 < chanceToMiss + chanceToDodge + chanceToParry + chanceToGlancingBlow + chanceToBlock + chanceToCrit:
                    {
                        // DONE: Critical hit attack
                        result.Damage *= 2;
                        result.HitInfo = result.HitInfo | AttackHitState.HITINFO_HITANIMATION;
                        result.HitInfo = result.HitInfo | AttackHitState.HITINFO_CRITICALHIT;
                        DoEmote(Emotes.ONESHOT_WOUNDCRITICAL, ref Victim);
                        break;
                    }

                case var case6 when case6 < chanceToMiss + chanceToDodge + chanceToParry + chanceToGlancingBlow + chanceToBlock + chanceToCrit + chanceToCrushingBlow:
                    {
                        // DONE: Crushing Blow
                        result.Damage = result.Damage * 3 >> 1;
                        result.HitInfo = result.HitInfo | AttackHitState.HITINFO_HITANIMATION;
                        result.HitInfo = result.HitInfo | AttackHitState.HIT_CRUSHING_BLOW;
                        break;
                    }

                default:
                    {
                        // DONE: Normal hit
                        result.HitInfo = result.HitInfo | AttackHitState.HITINFO_HITANIMATION;
                        break;
                    }
            }

            // DONE: Resist
            if (result.GetDamage > 0 && result.DamageType > DamageTypes.DMG_PHYSICAL)
            {
                result.Resist = (int)Victim.GetResist(ref Attacker, result.DamageType, result.Damage);
                if (result.GetDamage <= 0)
                    result.HitInfo = result.HitInfo | AttackHitState.HIT_RESIST;
            }

            // DONE: Absorb
            if (result.GetDamage > 0)
            {
                result.Absorbed = Victim.GetAbsorb(result.DamageType, result.Damage);
                if (result.GetDamage <= 0)
                    result.HitInfo = result.HitInfo | AttackHitState.HITINFO_ABSORB;
            }

            // TODO: Procs

            /* TODO ERROR: Skipped IfDirectiveTrivia */            // _WorldServer.Log.WriteLine(LogType.INFORMATION, "skillDiference = {0}", skillDiference)
                                                                   // _WorldServer.Log.WriteLine(LogType.INFORMATION, "chanceToMiss = {0}", chanceToMiss)
                                                                   // _WorldServer.Log.WriteLine(LogType.INFORMATION, "chanceToCrit = {0}", chanceToCrit)
                                                                   // _WorldServer.Log.WriteLine(LogType.INFORMATION, "chanceToParry = {0}", chanceToParry)
                                                                   // _WorldServer.Log.WriteLine(LogType.INFORMATION, "chanceToDodge = {0}", chanceToDodge)
                                                                   // _WorldServer.Log.WriteLine(LogType.INFORMATION, "chanceToBlock = {0}", chanceToBlock)
                                                                   // _WorldServer.Log.WriteLine(LogType.INFORMATION, "result.Damage = {0}", result.Damage)
                                                                   // _WorldServer.Log.WriteLine(LogType.INFORMATION, "result.Blocked = {0}", result.Blocked)
                                                                   // _WorldServer.Log.WriteLine(LogType.INFORMATION, "result.HitInfo = {0}", result.HitInfo)
                                                                   // _WorldServer.Log.WriteLine(LogType.INFORMATION, "result.victimState = {0}", result.victimState)
            /* TODO ERROR: Skipped EndIfDirectiveTrivia */
            return result;
        }

        // Combat system calculations
        public float GetBasePercentDodge(ref WS_Base.BaseUnit objCharacter, int skillDiference)
        {
            // http://www.wowwiki.com/Formulas:Dodge

            if (objCharacter is WS_PlayerData.CharacterObject)
            {
                // DONE: Stunned target cannot dodge
                if (objCharacter.cUnitFlags & UnitFlags.UNIT_FLAG_STUNTED)
                    return 0f;
                if (((WS_PlayerData.CharacterObject)objCharacter).combatDodge > 0)
                {
                    int combatDodgeAgilityBonus;
                    switch (((WS_PlayerData.CharacterObject)objCharacter).Classe)
                    {
                        case var @case when @case == Classes.CLASS_HUNTER:
                            {
                                combatDodgeAgilityBonus = (int)Conversion.Fix(((WS_PlayerData.CharacterObject)objCharacter).Agility.Base / 26.5f);
                                break;
                            }

                        case var case1 when case1 == Classes.CLASS_ROGUE:
                            {
                                combatDodgeAgilityBonus = (int)Conversion.Fix(((WS_PlayerData.CharacterObject)objCharacter).Agility.Base / 14.5f);
                                break;
                            }

                        case var case2 when case2 == Classes.CLASS_MAGE:
                        case var case3 when case3 == Classes.CLASS_PALADIN:
                        case var case4 when case4 == Classes.CLASS_WARLOCK:
                            {
                                combatDodgeAgilityBonus = (int)Conversion.Fix(((WS_PlayerData.CharacterObject)objCharacter).Agility.Base / 19.5f);
                                break;
                            }

                        default:
                            {
                                combatDodgeAgilityBonus = (int)Conversion.Fix(((WS_PlayerData.CharacterObject)objCharacter).Agility.Base / 20d);
                                break;
                            }
                    }

                    return ((WS_PlayerData.CharacterObject)objCharacter).combatDodge + combatDodgeAgilityBonus - skillDiference * 0.04f;
                }
            }

            return 0f;
        }

        public float GetBasePercentParry(ref WS_Base.BaseUnit objCharacter, int skillDiference)
        {
            // http://www.wowwiki.com/Formulas:Parry

            if (objCharacter is WS_PlayerData.CharacterObject)
            {
                // NOTE: Must have learned "Parry" spell, ID=3127
                if (((WS_PlayerData.CharacterObject)objCharacter).combatParry > 0)
                {
                    return ((WS_PlayerData.CharacterObject)objCharacter).combatParry - skillDiference * 0.04f;
                }
            }

            return 0f;
        }

        public float GetBasePercentBlock(ref WS_Base.BaseUnit objCharacter, int skillDiference)
        {
            // http://www.wowwiki.com/Formulas:Block

            if (objCharacter is WS_PlayerData.CharacterObject)
            {
                // NOTE: Must have learned "Block" spell, ID=107
                if (((WS_PlayerData.CharacterObject)objCharacter).combatBlock > 0)
                {
                    return ((WS_PlayerData.CharacterObject)objCharacter).combatBlock - skillDiference * 0.04f;
                }
            }

            return 0f;
        }

        public float GetBasePercentMiss(ref WS_Base.BaseUnit objCharacter, int skillDiference)
        {
            // http://www.wowwiki.com/Miss

            if (objCharacter is WS_PlayerData.CharacterObject)
            {
                {
                    var withBlock = (WS_PlayerData.CharacterObject)objCharacter;
                    if (withBlock.attackSheathState == SHEATHE_SLOT.SHEATHE_WEAPON)
                    {

                        // NOTE: Character is with selected hand weapons
                        if (withBlock.Items.ContainsKey(EquipmentSlots.EQUIPMENT_SLOT_OFFHAND))
                        {
                            // NOTE: Character is with equiped offhand item, checking if it is weapon
                            if (withBlock.Items(EquipmentSlots.EQUIPMENT_SLOT_OFFHAND).ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_WEAPON)
                            {
                                // DualWield Miss chance
                                if (skillDiference > 10)
                                {
                                    return 19 + 5 - skillDiference * 0.1f;
                                }
                                else
                                {
                                    return 19 + 5 - skillDiference * 0.2f;
                                }
                            }
                        }

                        if (skillDiference > 10)
                        {
                            return 5f - skillDiference * 0.1f;
                        }
                        else
                        {
                            return 5f - skillDiference * 0.2f;
                        }
                    }
                }
            }

            // Base Miss chance
            return 5f - skillDiference * 0.04f;
        }

        public float GetBasePercentCrit(ref WS_Base.BaseUnit objCharacter, int skillDiference)
        {
            // 5% base critical chance

            if (objCharacter is WS_PlayerData.CharacterObject)
            {
                float baseCrit = 0f;
                switch (((WS_PlayerData.CharacterObject)objCharacter).Classe)
                {
                    case var @case when @case == Classes.CLASS_ROGUE:
                        {
                            baseCrit = (float)(0.0Fd + ((WS_PlayerData.CharacterObject)objCharacter).Agility.Base / 29d);
                            break;
                        }

                    case var case1 when case1 == Classes.CLASS_DRUID:
                        {
                            baseCrit = (float)(0.92Fd + ((WS_PlayerData.CharacterObject)objCharacter).Agility.Base / 20d);
                            break;
                        }

                    case var case2 when case2 == Classes.CLASS_HUNTER:
                        {
                            baseCrit = (float)(0.0Fd + ((WS_PlayerData.CharacterObject)objCharacter).Agility.Base / 33d);
                            break;
                        }

                    case var case3 when case3 == Classes.CLASS_MAGE:
                        {
                            baseCrit = (float)(3.2Fd + ((WS_PlayerData.CharacterObject)objCharacter).Agility.Base / 19.44d);
                            break;
                        }

                    case var case4 when case4 == Classes.CLASS_PALADIN:
                        {
                            baseCrit = (float)(0.7Fd + ((WS_PlayerData.CharacterObject)objCharacter).Agility.Base / 19.77d);
                            break;
                        }

                    case var case5 when case5 == Classes.CLASS_PRIEST:
                        {
                            baseCrit = (float)(3.0Fd + ((WS_PlayerData.CharacterObject)objCharacter).Agility.Base / 20d);
                            break;
                        }

                    case var case6 when case6 == Classes.CLASS_SHAMAN:
                        {
                            baseCrit = (float)(1.7Fd + ((WS_PlayerData.CharacterObject)objCharacter).Agility.Base / 19.7d);
                            break;
                        }

                    case var case7 when case7 == Classes.CLASS_WARLOCK:
                        {
                            baseCrit = (float)(2.0Fd + ((WS_PlayerData.CharacterObject)objCharacter).Agility.Base / 20d);
                            break;
                        }

                    case var case8 when case8 == Classes.CLASS_WARRIOR:
                        {
                            baseCrit = (float)(0.0Fd + ((WS_PlayerData.CharacterObject)objCharacter).Agility.Base / 20d);
                            break;
                        }
                }

                return baseCrit + ((WS_PlayerData.CharacterObject)objCharacter).combatCrit + skillDiference * 0.2f;
            }
            else
            {
                return 5f + skillDiference * 0.2f;
            }
        }

        // Helper calculations
        public float GetDistance(WS_Base.BaseObject Object1, WS_Base.BaseObject Object2)
        {
            return GetDistance(Object1.positionX, Object2.positionX, Object1.positionY, Object2.positionY, Object1.positionZ, Object2.positionZ);
        }

        public float GetDistance(WS_Base.BaseObject Object1, float x2, float y2, float z2)
        {
            return GetDistance(Object1.positionX, x2, Object1.positionY, y2, Object1.positionZ, z2);
        }

        public float GetDistance(float x1, float x2, float y1, float y2, float z1, float z2)
        {
            return (float)Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2) + (z1 - z2) * (z1 - z2));
        }

        public float GetDistance(float x1, float x2, float y1, float y2)
        {
            return (float)Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }

        public float GetOrientation(float x1, float x2, float y1, float y2)
        {
            float angle = (float)Math.Atan2(y2 - y1, x2 - x1);
            if (angle < 0f)
            {
                angle = (float)(angle + 2d * Math.PI);
            }

            return angle;
        }

        public bool IsInFrontOf(ref WS_Base.BaseObject Object1, ref WS_Base.BaseObject Object2)
        {
            return IsInFrontOf(ref Object1, Object2.positionX, Object2.positionY);
        }

        public bool IsInFrontOf(ref WS_Base.BaseObject Object1, float x2, float y2)
        {
            float angle2 = GetOrientation(Object1.positionX, x2, Object1.positionY, y2);
            float lowAngle = Object1.orientation - 1.04719758f;
            float hiAngle = Object1.orientation + 1.04719758f;
            if (lowAngle < 0f)
            {
                return angle2 >= 2d * Math.PI + lowAngle & angle2 <= 2d * Math.PI | angle2 >= 0f & angle2 <= hiAngle;
            }

            return angle2 >= lowAngle & angle2 <= hiAngle;
        }

        public bool IsInBackOf(ref WS_Base.BaseObject Object1, ref WS_Base.BaseObject Object2)
        {
            return IsInBackOf(ref Object1, Object2.positionX, Object2.positionY);
        }

        public bool IsInBackOf(ref WS_Base.BaseObject Object1, float x2, float y2)
        {
            float angle2 = GetOrientation(x2, Object1.positionX, y2, Object1.positionY);
            float lowAngle = Object1.orientation - 1.04719758f;
            float hiAngle = Object1.orientation + 1.04719758f;
            if (lowAngle < 0f)
            {
                return angle2 >= 2d * Math.PI + lowAngle & angle2 <= 2d * Math.PI | angle2 >= 0f & angle2 <= hiAngle;
            }

            return angle2 >= lowAngle & angle2 <= hiAngle;
        }

        // Helper functions
        public int GetSkillWeapon(ref WS_Base.BaseUnit objCharacter, bool DualWield)
        {
            if (objCharacter is WS_PlayerData.CharacterObject)
            {
                var tmpSkill = default(int);
                {
                    var withBlock = (WS_PlayerData.CharacterObject)objCharacter;
                    switch (withBlock.attackSheathState)
                    {
                        case var @case when @case == SHEATHE_SLOT.SHEATHE_NONE:
                            {
                                tmpSkill = SKILL_IDs.SKILL_UNARMED;
                                break;
                            }

                        case var case1 when case1 == SHEATHE_SLOT.SHEATHE_WEAPON:
                            {
                                if (DualWield && withBlock.Items.ContainsKey(EquipmentSlots.EQUIPMENT_SLOT_OFFHAND))
                                {
                                    tmpSkill = WorldServiceLocator._WorldServer.ITEMDatabase(withBlock.Items(EquipmentSlots.EQUIPMENT_SLOT_OFFHAND).ItemEntry).GetReqSkill;
                                }
                                else if (withBlock.Items.ContainsKey(EquipmentSlots.EQUIPMENT_SLOT_MAINHAND))
                                {
                                    tmpSkill = WorldServiceLocator._WorldServer.ITEMDatabase(withBlock.Items(EquipmentSlots.EQUIPMENT_SLOT_MAINHAND).ItemEntry).GetReqSkill;
                                }

                                break;
                            }

                        case var case2 when case2 == SHEATHE_SLOT.SHEATHE_RANGED:
                            {
                                if (withBlock.Items.ContainsKey(EquipmentSlots.EQUIPMENT_SLOT_RANGED))
                                {
                                    tmpSkill = WorldServiceLocator._WorldServer.ITEMDatabase(withBlock.Items(EquipmentSlots.EQUIPMENT_SLOT_RANGED).ItemEntry).GetReqSkill;
                                }

                                break;
                            }
                    }

                    if (tmpSkill == 0)
                    {
                        return objCharacter.Level * 5;
                    }
                    else
                    {
                        withBlock.UpdateSkill(tmpSkill, 0.01f);
                        return withBlock.Skills[tmpSkill].CurrentWithBonus;
                    }
                }
            }

            return objCharacter.Level * 5;
        }

        public int GetSkillDefence(ref WS_Base.BaseUnit objCharacter)
        {
            if (objCharacter is WS_PlayerData.CharacterObject)
            {
                ((WS_PlayerData.CharacterObject)objCharacter).UpdateSkill(SKILL_IDs.SKILL_DEFENSE, 0.01d);
                return ((WS_PlayerData.CharacterObject)objCharacter).Skills(SKILL_IDs.SKILL_DEFENSE).CurrentWithBonus;
            }

            return objCharacter.Level * 5;
        }

        public int GetAttackTime(ref WS_PlayerData.CharacterObject objCharacter, ref bool combatDualWield)
        {
            switch (objCharacter.attackSheathState)
            {
                case var @case when @case == SHEATHE_SLOT.SHEATHE_NONE:
                    {
                        return (int)objCharacter.AttackTime(0);
                    }

                case var case1 when case1 == SHEATHE_SLOT.SHEATHE_WEAPON:
                    {
                        if (combatDualWield)
                        {
                            if ((int)objCharacter.AttackTime(1) == 0)
                                return (int)objCharacter.AttackTime(0);
                            return (int)objCharacter.AttackTime(1);
                        }
                        else
                        {
                            if ((int)objCharacter.AttackTime(0) == 0)
                                return (int)objCharacter.AttackTime(1);
                            return (int)objCharacter.AttackTime(0);
                        }

                        break;
                    }

                case var case2 when case2 == SHEATHE_SLOT.SHEATHE_RANGED:
                    {
                        return (int)objCharacter.AttackTime(2);
                    }
            }

            return default;
        }

        public void GetDamage(ref WS_Base.BaseUnit objCharacter, bool DualWield, ref DamageInfo result)
        {
            if (objCharacter is WS_PlayerData.CharacterObject)
            {
                {
                    var withBlock = (WS_PlayerData.CharacterObject)objCharacter;
                    switch (withBlock.attackSheathState)
                    {
                        case var @case when @case == SHEATHE_SLOT.SHEATHE_NONE:
                            {
                                result.HitInfo = AttackHitState.HITINFO_NORMALSWING;
                                result.DamageType = DamageTypes.DMG_PHYSICAL;
                                result.Damage = WorldServiceLocator._WorldServer.Rnd.Next(withBlock.BaseUnarmedDamage, withBlock.BaseUnarmedDamage + 1);
                                break;
                            }

                        case var case1 when case1 == SHEATHE_SLOT.SHEATHE_WEAPON:
                            {
                                if (DualWield)
                                {
                                    result.HitInfo = AttackHitState.HITINFO_HITANIMATION + AttackHitState.HITINFO_LEFTSWING;
                                    result.DamageType = DamageTypes.DMG_PHYSICAL;
                                    result.Damage = WorldServiceLocator._WorldServer.Rnd.Next((int)(withBlock.OffHandDamage.Minimum / 2f), (int)(withBlock.OffHandDamage.Maximum / 2f + 1f)) + withBlock.BaseUnarmedDamage;
                                }
                                else
                                {
                                    result.HitInfo = AttackHitState.HITINFO_HITANIMATION;
                                    result.DamageType = DamageTypes.DMG_PHYSICAL;
                                    result.Damage = WorldServiceLocator._WorldServer.Rnd.Next((int)withBlock.Damage.Minimum, (int)(withBlock.Damage.Maximum + 1f)) + withBlock.BaseUnarmedDamage;
                                }

                                break;
                            }

                        case var case2 when case2 == SHEATHE_SLOT.SHEATHE_RANGED:
                            {
                                result.HitInfo = AttackHitState.HITINFO_HITANIMATION + AttackHitState.HITINFO_RANGED;
                                result.DamageType = DamageTypes.DMG_PHYSICAL;
                                result.Damage = WorldServiceLocator._WorldServer.Rnd.Next((int)withBlock.RangedDamage.Minimum, (int)(withBlock.RangedDamage.Maximum + 1f)) + withBlock.BaseRangedDamage;
                                break;
                            }
                    }
                }
            }
            else
            {
                {
                    var withBlock1 = (WS_Creatures.CreatureObject)objCharacter;
                    result.DamageType = DamageTypes.DMG_PHYSICAL;
                    result.Damage = WorldServiceLocator._WorldServer.Rnd.Next((int)WorldServiceLocator._WorldServer.CREATURESDatabase[withBlock1.ID].Damage.Minimum, (int)(WorldServiceLocator._WorldServer.CREATURESDatabase[withBlock1.ID].Damage.Maximum + 1f)); // + (CType(_WorldServer.CREATURESDatabase(.ID), CreatureInfo).AtackPower / 14 * (CType(_WorldServer.CREATURESDatabase(.ID), CreatureInfo).BaseAttackTime / 1000))
                }
            }
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public struct DamageInfo
        {
            public int Damage;
            public DamageTypes DamageType;
            public int Blocked;
            public int Absorbed;
            public int Resist;
            public AttackVictimState victimState;
            public int HitInfo;
            public byte Turn;

            public int GetDamage
            {
                get
                {
                    return Damage - Absorbed - Blocked - Resist;
                }
            }
        }

        public class TAttackTimer : IDisposable
        {

            // Internal
            private int LastAttack = 0;
            private Timer NextAttackTimer = null;
            public WS_Base.BaseUnit Victim;
            public WS_PlayerData.CharacterObject Character;
            public float combatReach;
            public float minRanged;
            public bool combatDualWield = false;
            public bool Ranged = false;
            private int TimeLeftMainHand = -1;
            private int TimeLeftOffHand = -1;

            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            private bool _disposedValue; // To detect redundant calls

            // IDisposable
            protected virtual void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                    // TODO: set large fields to null.
                    NextAttackTimer.Dispose();
                    NextAttackTimer = null;
                    combatNextAttack.Dispose();
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
            public TAttackTimer(ref WS_Base.BaseObject Victim_, ref WS_PlayerData.CharacterObject Character_)
            {
                NextAttackTimer = new Timer(DoAttack, null, 1000, Timeout.Infinite);
                Victim = (WS_Base.BaseUnit)Victim_;
                Character = Character_;
            }

            public TAttackTimer(ref WS_PlayerData.CharacterObject Character_)
            {
                NextAttackTimer = new Timer(DoAttack, null, Timeout.Infinite, Timeout.Infinite);
                Character = Character_;
                Victim = null;
            }

            // Packets
            public void AttackStop()
            {
                if (Character.AutoShotSpell > 0)
                    return;
                NextAttackTimer.Change(Timeout.Infinite, Timeout.Infinite);
                Victim = null;
                Ranged = false;
            }

            public void AttackStart(WS_Base.BaseUnit Victim_ = null)
            {
                if (Victim is null)
                {
                    Victim = Victim_;
                    combatReach = WS_Base.BaseUnit.CombatReach_Base + Victim.BoundingRadius + Character.CombatReach;
                    minRanged = Victim.BoundingRadius + 8.0f;
                }
                else if (Victim.GUID == Victim_.GUID)
                {
                    // DONE: If it's the same target we do nothing
                    return;
                }
                else
                {
                    WorldServiceLocator._WS_Combat.SendAttackStop(Character.GUID, Victim.GUID, ref Character.client);
                    Victim = Victim_;
                    combatReach = WS_Base.BaseUnit.CombatReach_Base + Victim.BoundingRadius + Character.CombatReach;
                    minRanged = Victim.BoundingRadius + 8.0f;
                }

                bool argcombatDualWield = false;
                int AttackSpeed = WorldServiceLocator._WS_Combat.GetAttackTime(ref Character, ref argcombatDualWield);
                if (WorldServiceLocator._NativeMethods.timeGetTime("") - LastAttack >= AttackSpeed)
                {
                    DoAttack(null);
                }
                else
                {
                    NextAttackTimer.Change(WorldServiceLocator._NativeMethods.timeGetTime("") - LastAttack, Timeout.Infinite);
                }
            }

            public void DoAttack(object Status)
            {
                // DONE: Stop attacking when there's no victim
                if (Victim is null)
                {
                    Character.AutoShotSpell = 0;
                    AttackStop();
                    return;
                }

                LastAttack = WorldServiceLocator._NativeMethods.timeGetTime("");
                Character.RemoveAurasByInterruptFlag(SpellAuraInterruptFlags.AURA_INTERRUPT_FLAG_START_ATTACK);
                try
                {
                    if (Ranged)
                    {
                        DoRangedAttack(false);
                    }
                    else
                    {
                        DoMeleeAttack(false);
                    }
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error doing attack.{0}{1}", Environment.NewLine, ex.ToString());
                }
            }

            public void DoMeleeAttack(object Status)
            {
                if (Victim is null)
                {
                    var SMSG_ATTACKSWING_CANT_ATTACK = new Packets.PacketClass(OPCODES.SMSG_ATTACKSWING_CANT_ATTACK);
                    Character.client.Send(ref SMSG_ATTACKSWING_CANT_ATTACK);
                    SMSG_ATTACKSWING_CANT_ATTACK.Dispose();
                    Character.AutoShotSpell = 0;
                    AttackStop();
                    return;
                }

                try
                {
                    // DONE: If casting spell exit
                    if (Character.spellCasted[CurrentSpellTypes.CURRENT_GENERIC_SPELL] is object && Character.spellCasted[CurrentSpellTypes.CURRENT_GENERIC_SPELL].Finished == false)
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "AttackStop: Casting Spell");
                        // AttackStop()
                        bool argcombatDualWield = false;
                        NextAttackTimer.Change(WorldServiceLocator._WS_Combat.GetAttackTime(ref Character, ref argcombatDualWield), Timeout.Infinite);
                        return;
                    }

                    if (Victim.IsDead)
                    {
                        var SMSG_ATTACKSWING_DEADTARGET = new Packets.PacketClass(OPCODES.SMSG_ATTACKSWING_DEADTARGET);
                        Character.client.Send(ref SMSG_ATTACKSWING_DEADTARGET);
                        SMSG_ATTACKSWING_DEADTARGET.Dispose();
                        Character.AutoShotSpell = 0;
                        AttackStop();
                        return;
                    }

                    if (Character.IsDead)
                    {
                        var SMSG_ATTACKSWING_DEADTARGET = new Packets.PacketClass(OPCODES.SMSG_ATTACKSWING_DEADTARGET);
                        Character.client.Send(ref SMSG_ATTACKSWING_DEADTARGET);
                        SMSG_ATTACKSWING_DEADTARGET.Dispose();
                        Character.AutoShotSpell = 0;
                        AttackStop();
                        return;
                    }

                    if (Character.StandState > 0)
                    {
                        var SMSG_ATTACKSWING_NOTSTANDING = new Packets.PacketClass(OPCODES.SMSG_ATTACKSWING_NOTSTANDING);
                        Character.client.Send(ref SMSG_ATTACKSWING_NOTSTANDING);
                        SMSG_ATTACKSWING_NOTSTANDING.Dispose();
                        Character.AutoShotSpell = 0;
                        AttackStop();
                        return;
                    }

                    // DONE: Decide it's real position
                    if (Victim is WS_Creatures.CreatureObject)
                        ((WS_Creatures.CreatureObject)Victim).SetToRealPosition();
                    float tmpPosX = Victim.positionX;
                    float tmpPosY = Victim.positionY;
                    float tmpPosZ = Victim.positionZ;
                    float tmpDist = WorldServiceLocator._WS_Combat.GetDistance(Character, tmpPosX, tmpPosY, tmpPosZ);
                    if (tmpDist > 8f + Victim.CombatReach)
                    {
                        // DONE: Use ranged if you're too far away for melee
                        if (Character.CanShootRanged)
                        {
                            Ranged = true;
                            DoRangedAttack(null);
                            return;
                        }
                        else
                        {
                            NextAttackTimer.Change(2000, Timeout.Infinite);
                            var SMSG_ATTACKSWING_NOTINRANGE = new Packets.PacketClass(OPCODES.SMSG_ATTACKSWING_NOTINRANGE);
                            Character.client.Send(ref SMSG_ATTACKSWING_NOTINRANGE);
                            SMSG_ATTACKSWING_NOTINRANGE.Dispose();
                            return;
                        }
                    }
                    else if (tmpDist > combatReach)
                    {
                        NextAttackTimer.Change(2000, Timeout.Infinite);
                        var SMSG_ATTACKSWING_NOTINRANGE = new Packets.PacketClass(OPCODES.SMSG_ATTACKSWING_NOTINRANGE);
                        Character.client.Send(ref SMSG_ATTACKSWING_NOTINRANGE);
                        SMSG_ATTACKSWING_NOTINRANGE.Dispose();
                        return;
                    }

                    WS_Base.BaseObject argObject1 = Character;
                    if (!WorldServiceLocator._WS_Combat.IsInFrontOf(ref argObject1, tmpPosX, tmpPosY))
                    {
                        NextAttackTimer.Change(2000, Timeout.Infinite);
                        var SMSG_ATTACKSWING_BADFACING = new Packets.PacketClass(OPCODES.SMSG_ATTACKSWING_BADFACING);
                        Character.client.Send(ref SMSG_ATTACKSWING_BADFACING);
                        SMSG_ATTACKSWING_BADFACING.Dispose();
                        return;
                    }

                    bool HaveMainHand = (int)Character.AttackTime(0) > 0 && Character.Items.ContainsKey(EquipmentSlots.EQUIPMENT_SLOT_MAINHAND);
                    bool HaveOffHand = (int)Character.AttackTime(1) > 0 && Character.Items.ContainsKey(EquipmentSlots.EQUIPMENT_SLOT_OFFHAND);

                    // DONE: Spells that add to attack
                    if (!combatNextAttackSpell)
                    {
                        DoMeleeDamage();
                    }
                    else
                    {
                        combatNextAttack.Set();
                        combatNextAttack.Set();
                        combatNextAttackSpell = false;
                    }

                    // DONE: Calculate next attack
                    int NextAttack = WorldServiceLocator._WS_Combat.GetAttackTime(ref Character, ref combatDualWield);
                    if (HaveMainHand && HaveOffHand) // If we are dualwielding
                    {
                        // ' Character.CommandResponse("You are dualwielding!")

                        if (combatDualWield)
                        {
                            if (TimeLeftMainHand == -1)
                                TimeLeftMainHand = (int)((double)Character.AttackTime(1) / 2d);
                            TimeLeftOffHand = (int)Character.AttackTime(1);
                        }
                        else
                        {
                            if (TimeLeftMainHand == -1)
                                TimeLeftOffHand = (int)((double)Character.AttackTime(0) / 2d);
                            TimeLeftMainHand = (int)Character.AttackTime(0);
                        }

                        if (TimeLeftMainHand < TimeLeftOffHand)
                        {
                            NextAttack = TimeLeftMainHand;
                            combatDualWield = false;
                        }
                        else
                        {
                            NextAttack = TimeLeftOffHand;
                            combatDualWield = true;
                        }

                        TimeLeftMainHand -= NextAttack;
                        TimeLeftOffHand -= NextAttack;
                        Character.CommandResponse("NO: " + TimeLeftOffHand);
                        Character.CommandResponse("NM: " + TimeLeftMainHand);
                    }
                    else
                    {
                        // ' Character.CommandResponse("You're not dualwielding!")
                        TimeLeftMainHand = -1;
                        combatDualWield = HaveOffHand;
                    }

                    // DONE: Enqueue next attack
                    NextAttackTimer.Change(NextAttack, Timeout.Infinite);
                }
                catch (Exception e)
                {
                    if (Character is object && Character.client is object)
                    {
                        var SMSG_ATTACKSWING_CANT_ATTACK = new Packets.PacketClass(OPCODES.SMSG_ATTACKSWING_CANT_ATTACK);
                        Character.client.Send(ref SMSG_ATTACKSWING_CANT_ATTACK);
                        SMSG_ATTACKSWING_CANT_ATTACK.Dispose();
                    }

                    AttackStop();
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Error while doing melee attack.{0}", Environment.NewLine + e.ToString());
                }
            }

            public void DoRangedAttack(object Status)
            {
                // DONE: Decide it's real position
                if (Victim is WS_Creatures.CreatureObject)
                    ((WS_Creatures.CreatureObject)Victim).SetToRealPosition();
                float tmpPosX = Victim.positionX;
                float tmpPosY = Victim.positionY;
                float tmpPosZ = Victim.positionZ;
                if (Character.spellCasted[CurrentSpellTypes.CURRENT_GENERIC_SPELL] is object && Character.spellCasted[CurrentSpellTypes.CURRENT_GENERIC_SPELL].Finished == false)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "AttackPause: Casting Spell");
                    bool argcombatDualWield = false;
                    NextAttackTimer.Change(WorldServiceLocator._WS_Combat.GetAttackTime(ref Character, ref argcombatDualWield), Timeout.Infinite);
                    return;
                }

                if (Victim.Life.Current == 0)
                {
                    var SMSG_ATTACKSWING_DEADTARGET = new Packets.PacketClass(OPCODES.SMSG_ATTACKSWING_DEADTARGET);
                    Character.client.Send(ref SMSG_ATTACKSWING_DEADTARGET);
                    SMSG_ATTACKSWING_DEADTARGET.Dispose();
                    Character.AutoShotSpell = 0;
                    AttackStop();
                    return;
                }

                if (Character.DEAD)
                {
                    var SMSG_ATTACKSWING_DEADTARGET = new Packets.PacketClass(OPCODES.SMSG_ATTACKSWING_DEADTARGET);
                    Character.client.Send(ref SMSG_ATTACKSWING_DEADTARGET);
                    SMSG_ATTACKSWING_DEADTARGET.Dispose();
                    Character.AutoShotSpell = 0;
                    AttackStop();
                    return;
                }

                if (Character.StandState > 0)
                {
                    var SMSG_ATTACKSWING_NOTSTANDING = new Packets.PacketClass(OPCODES.SMSG_ATTACKSWING_NOTSTANDING);
                    Character.client.Send(ref SMSG_ATTACKSWING_NOTSTANDING);
                    SMSG_ATTACKSWING_NOTSTANDING.Dispose();
                    Character.AutoShotSpell = 0;
                    AttackStop();
                    return;
                }

                // DONE: Change to melee if we're too close for ranged
                float tmpDist = WorldServiceLocator._WS_Combat.GetDistance(Character, tmpPosX, tmpPosY, tmpPosZ);
                if (tmpDist < combatReach)
                {
                    Ranged = false;
                    DoMeleeAttack(null);
                    return;
                }
                else if (tmpDist < minRanged)
                {
                    NextAttackTimer.Change(2000, Timeout.Infinite);
                    var SMSG_ATTACKSWING_NOTINRANGE = new Packets.PacketClass(OPCODES.SMSG_ATTACKSWING_NOTINRANGE);
                    Character.client.Send(ref SMSG_ATTACKSWING_NOTINRANGE);
                    SMSG_ATTACKSWING_NOTINRANGE.Dispose();
                    return;
                }

                WS_Base.BaseObject argObject1 = Character;
                if (!WorldServiceLocator._WS_Combat.IsInFrontOf(ref argObject1, tmpPosX, tmpPosY))
                {
                    NextAttackTimer.Change(2000, Timeout.Infinite);
                    var SMSG_ATTACKSWING_BADFACING = new Packets.PacketClass(OPCODES.SMSG_ATTACKSWING_BADFACING);
                    Character.client.Send(ref SMSG_ATTACKSWING_BADFACING);
                    SMSG_ATTACKSWING_BADFACING.Dispose();
                    return;
                }

                DoRangedDamage();

                // DONE: Enqueue next attack
                bool argcombatDualWield1 = false;
                NextAttackTimer.Change(WorldServiceLocator._WS_Combat.GetAttackTime(ref Character, ref argcombatDualWield1), Timeout.Infinite);
            }

            public void DoMeleeDamage()
            {
                WS_Base.BaseUnit argAttacker = Character;
                var damageInfo = WorldServiceLocator._WS_Combat.CalculateDamage(ref argAttacker, ref Victim, combatDualWield, false);
                WS_Base.BaseObject argAttacker1 = Character;
                WS_Base.BaseObject argVictim = Victim;
                WorldServiceLocator._WS_Combat.SendAttackerStateUpdate(ref argAttacker1, ref argVictim, damageInfo, ref Character.client);

                // TODO: If the victim has a spelltrigger on melee attacks
                var Target = new WS_Spells.SpellTargets();
                WS_Base.BaseUnit argobjCharacter = Character;
                Target.SetTarget_UNIT(ref argobjCharacter);
                for (byte i = 0, loopTo = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1; i <= loopTo; i++)
                {
                    if (Victim.ActiveSpells[i] is object && Victim.ActiveSpells[i].GetSpellInfo.procFlags & ProcFlags.PROC_FLAG_HIT_MELEE)
                    {
                        for (byte j = 0; j <= 2; j++)
                        {
                            if (Victim.ActiveSpells[i].Aura_Info[j] is object && Victim.ActiveSpells[i].Aura_Info[j].ApplyAuraIndex == AuraEffects_Names.SPELL_AURA_PROC_TRIGGER_SPELL)
                            {
                                if (WorldServiceLocator._Functions.RollChance(Victim.ActiveSpells[i].GetSpellInfo.procChance))
                                {
                                    WS_Base.BaseObject argCaster = Victim;
                                    var castParams = new WS_Spells.CastSpellParameters(ref Target, ref argCaster, Victim.ActiveSpells[i].Aura_Info[j].TriggerSpell, true);
                                    castParams.Cast(null);
                                }
                            }
                        }
                    }
                }

                // DONE: Rage generation
                // http://www.wowwiki.com/Formulas:Rage_generation
                if (Character.Classe == Classes.CLASS_WARRIOR || Character.Classe == Classes.CLASS_DRUID && (Character.ShapeshiftForm == ShapeshiftForm.FORM_BEAR || Character.ShapeshiftForm == ShapeshiftForm.FORM_DIREBEAR))
                {
                    Character.Rage.Increment(Fix((7.5d * (double)damageInfo.Damage / (double)Character.GetRageConversion + (double)(Character.GetHitFactor((damageInfo.HitInfo & AttackHitState.HITINFO_LEFTSWING) == 0, damageInfo.HitInfo & AttackHitState.HITINFO_CRITICALHIT) * (float)WorldServiceLocator._WS_Combat.GetAttackTime(ref Character, ref combatDualWield))) / 2d));
                    Character.SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER1 + ManaTypes.TYPE_RAGE, Character.Rage.Current);
                    Character.SendCharacterUpdate(true);
                }

                WS_Base.BaseUnit argAttacker2 = Character;
                Victim.DealDamage(damageInfo.GetDamage, ref argAttacker2);
                if (Victim is null || Victim.IsDead)
                    AttackStop();
            }

            public void DoRangedDamage()
            {
                var Targets = new WS_Spells.SpellTargets();
                Targets.SetTarget_UNIT(ref Victim);
                int SpellID;
                if (Character.AutoShotSpell > 0)
                {
                    SpellID = Character.AutoShotSpell;
                }
                else
                {
                    SpellID = 75;
                }

                WS_Base.BaseObject argCaster = Character;
                var tmpSpell = new WS_Spells.CastSpellParameters(ref Targets, ref argCaster, SpellID, true);
                ThreadPool.QueueUserWorkItem(new WaitCallback(tmpSpell.Cast));
            }

            // Spells
            public void DoMeleeDamageBySpell(ref WS_PlayerData.CharacterObject Character, ref WS_Base.BaseObject Victim2, int BonusDamage, int SpellID)
            {
                WS_Base.BaseUnit argAttacker = Character;
                WS_Base.BaseUnit argVictim = (WS_Base.BaseUnit)Victim2;
                var damageInfo = WorldServiceLocator._WS_Combat.CalculateDamage(ref argAttacker, ref argVictim, false, false, WorldServiceLocator._WS_Spells.SPELLs[SpellID]);
                bool IsCrit = false;
                if (damageInfo.Damage > 0)
                    damageInfo.Damage += BonusDamage;
                if (damageInfo.HitInfo == AttackHitState.HIT_CRIT)
                {
                    damageInfo.Damage += BonusDamage;
                    IsCrit = true;
                }

                WorldServiceLocator._WS_Spells.SendNonMeleeDamageLog(ref Character, ref Victim2, SpellID, damageInfo.DamageType, damageInfo.Damage, 0, damageInfo.Absorbed, IsCrit);
                if (Victim2 is WS_Creatures.CreatureObject)
                {
                    WS_Base.BaseUnit argAttacker1 = Character;
                    ((WS_Creatures.CreatureObject)Victim2).DealDamage(damageInfo.GetDamage, ref argAttacker1);
                    if (ReferenceEquals(Victim2, Victim) && ((WS_Creatures.CreatureObject)Victim).IsDead)
                    {
                        AttackStop();
                    }
                }
                else if (Victim2 is WS_PlayerData.CharacterObject)
                {
                    WS_Base.BaseUnit argAttacker2 = Character;
                    ((WS_PlayerData.CharacterObject)Victim2).DealDamage(damageInfo.GetDamage, ref argAttacker2);
                    if (((WS_PlayerData.CharacterObject)Victim2).Classe == Classes.CLASS_WARRIOR)
                    {
                        ((WS_PlayerData.CharacterObject)Victim2).Rage.Increment((int)Conversion.Fix(damageInfo.Damage / (double)(((WS_PlayerData.CharacterObject)Victim2).Level * 4) * 25d + 10d));
                        ((WS_PlayerData.CharacterObject)Victim2).SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER1 + ManaTypes.TYPE_RAGE, ((WS_PlayerData.CharacterObject)Victim2).Rage.Current);
                        Character.SendCharacterUpdate(true);
                    }
                }

                // DONE: Rage generation
                // http://www.wowwiki.com/Formulas:Rage_generation
                if (Character.Classe == Classes.CLASS_WARRIOR || Character.Classe == Classes.CLASS_DRUID && (Character.ShapeshiftForm == ShapeshiftForm.FORM_BEAR || Character.ShapeshiftForm == ShapeshiftForm.FORM_DIREBEAR))
                {
                    Character.Rage.Increment((int)Conversion.Fix(damageInfo.Damage / (double)(Character.Level * 4) * 75d + 10d));
                    Character.SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER1 + ManaTypes.TYPE_RAGE, Character.Rage.Current);
                    Character.SendCharacterUpdate(true);
                }
            }

            public AutoResetEvent combatNextAttack = new AutoResetEvent(false);
            public bool combatNextAttackSpell = false;
        }

        public void SetPlayerInCombat(ref WS_PlayerData.CharacterObject objCharacter)
        {
            objCharacter.cUnitFlags = objCharacter.cUnitFlags | UnitFlags.UNIT_FLAG_IN_COMBAT;
            objCharacter.SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, objCharacter.cUnitFlags);
            objCharacter.SendCharacterUpdate(false);
            objCharacter.RemoveAurasByInterruptFlag(SpellAuraInterruptFlags.AURA_INTERRUPT_FLAG_ENTER_COMBAT);
        }

        public void SetPlayerOutOfCombat(ref WS_PlayerData.CharacterObject objCharacter)
        {
            objCharacter.cUnitFlags = objCharacter.cUnitFlags & !UnitFlags.UNIT_FLAG_IN_COMBAT;
            objCharacter.SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, objCharacter.cUnitFlags);
            objCharacter.SendCharacterUpdate(false);
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public void On_CMSG_SET_SELECTION(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 13)
                return;
            packet.GetInt16();
            client.Character.TargetGUID = packet.GetUInt64();
            client.Character.SetUpdateFlag(EUnitFields.UNIT_FIELD_TARGET, client.Character.TargetGUID);
            client.Character.SendCharacterUpdate();
        }

        public void On_CMSG_ATTACKSWING(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 13)
                return;
            packet.GetInt16();
            ulong GUID = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_ATTACKSWING [GUID={2:X}]", client.IP, client.Port, GUID);
            if (client.Character.Spell_Pacifyed)
            {
                var SMSG_ATTACKSWING_CANT_ATTACK = new Packets.PacketClass(OPCODES.SMSG_ATTACKSWING_CANT_ATTACK);
                client.Send(ref SMSG_ATTACKSWING_CANT_ATTACK);
                SMSG_ATTACKSWING_CANT_ATTACK.Dispose();
                SendAttackStop(client.Character.GUID, GUID, ref client);
                return;
            }

            if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(GUID))
            {
                client.Character.attackState.AttackStart(WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID]);
            }
            else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(GUID))
            {
                client.Character.attackState.AttackStart(WorldServiceLocator._WorldServer.CHARACTERs[GUID]);
            }
            else
            {
                var SMSG_ATTACKSWING_CANT_ATTACK = new Packets.PacketClass(OPCODES.SMSG_ATTACKSWING_CANT_ATTACK);
                client.Send(ref SMSG_ATTACKSWING_CANT_ATTACK);
                SMSG_ATTACKSWING_CANT_ATTACK.Dispose();
                SendAttackStop(client.Character.GUID, GUID, ref client);
            }
        }

        public void On_CMSG_ATTACKSTOP(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            try
            {
                packet.GetInt16();
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_ATTACKSTOP", client.IP, client.Port);
                SendAttackStop(client.Character.GUID, client.Character.TargetGUID, ref client);
                client.Character.attackState.AttackStop();
            }
            catch (Exception e)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error stopping attack: {0}", e.ToString());
            }
        }

        public void On_CMSG_SET_AMMO(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 9)
                return;
            packet.GetInt16();
            int AmmoID = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SET_AMMO [{2}]", client.IP, client.Port, AmmoID);
            if (client.Character.IsDead)
            {
                WorldServiceLocator._WS_Items.SendInventoryChangeFailure(ref client.Character, InventoryChangeFailure.EQUIP_ERR_YOU_ARE_DEAD, 0, 0);
                return;
            }

            if (Conversions.ToBoolean(AmmoID)) // Set Ammo
            {
                client.Character.AmmoID = AmmoID;
                if (WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(AmmoID) == false)
                {
                    // TODO: Another one of these useless bits of code, needs to be implemented correctly
                    var tmpItem = new WS_Items.ItemInfo(AmmoID);
                }

                var CanUse = WorldServiceLocator._CharManagementHandler.CanUseAmmo(ref client.Character, AmmoID);
                if (CanUse != InventoryChangeFailure.EQUIP_ERR_OK)
                {
                    WorldServiceLocator._WS_Items.SendInventoryChangeFailure(ref client.Character, CanUse, 0, 0);
                    return;
                }

                float currentDPS = 0f;
                if (WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(AmmoID) == true && WorldServiceLocator._WorldServer.ITEMDatabase[AmmoID].ObjectClass == ITEM_CLASS.ITEM_CLASS_PROJECTILE || WorldServiceLocator._CharManagementHandler.CheckAmmoCompatibility(ref client.Character, AmmoID))
                {
                    currentDPS = WorldServiceLocator._WorldServer.ITEMDatabase[AmmoID].Damage[0].Minimum;
                }

                if (client.Character.AmmoDPS != currentDPS)
                {
                    client.Character.AmmoDPS = currentDPS;
                    CalculateMinMaxDamage(ref client.Character, WeaponAttackType.RANGED_ATTACK);
                }

                client.Character.AmmoID = AmmoID;
                client.Character.SetUpdateFlag(EPlayerFields.PLAYER_AMMO_ID, client.Character.AmmoID);
                client.Character.SendCharacterUpdate(false);
            }
            else if (Conversions.ToBoolean(client.Character.AmmoID)) // Remove Ammo
            {
                client.Character.AmmoDPS = 0f;
                CalculateMinMaxDamage(ref client.Character, WeaponAttackType.RANGED_ATTACK);
                client.Character.AmmoID = 0;
                client.Character.SetUpdateFlag(EPlayerFields.PLAYER_AMMO_ID, 0);
                client.Character.SendCharacterUpdate(false);
            }
        }

        public void On_CMSG_SETSHEATHED(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 9)
                return;
            packet.GetInt16();
            SHEATHE_SLOT sheathed = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SETSHEATHED [{2}]", client.IP, client.Port, sheathed);
            SetSheath(ref client.Character, sheathed);
        }

        public void SetSheath(ref WS_PlayerData.CharacterObject objCharacter, SHEATHE_SLOT State)
        {
            objCharacter.attackSheathState = State;
            objCharacter.combatCanDualWield = false;
            objCharacter.cBytes2 = objCharacter.cBytes2 & ~0xFF | State;
            objCharacter.SetUpdateFlag(EUnitFields.UNIT_FIELD_BYTES_2, objCharacter.cBytes2);
            switch (State)
            {
                case var @case when @case == SHEATHE_SLOT.SHEATHE_NONE:
                    {
                        ItemObject argItem = null;
                        SetVirtualItemInfo(objCharacter, 0, ref argItem);
                        ItemObject argItem1 = null;
                        SetVirtualItemInfo(objCharacter, 1, ref argItem1);
                        ItemObject argItem2 = null;
                        SetVirtualItemInfo(objCharacter, 2, ref argItem2);
                        break;
                    }

                case var case1 when case1 == SHEATHE_SLOT.SHEATHE_WEAPON:
                    {
                        if (objCharacter.Items.ContainsKey(EquipmentSlots.EQUIPMENT_SLOT_MAINHAND) && !objCharacter.Items(EquipmentSlots.EQUIPMENT_SLOT_MAINHAND).IsBroken)
                        {
                            var argItem3 = objCharacter.Items(EquipmentSlots.EQUIPMENT_SLOT_MAINHAND);
                            SetVirtualItemInfo(objCharacter, 0, ref argItem3);
                            objCharacter.Items(EquipmentSlots.EQUIPMENT_SLOT_MAINHAND) = argItem3;
                        }
                        else
                        {
                            ItemObject argItem4 = null;
                            SetVirtualItemInfo(objCharacter, 0, ref argItem4);
                            objCharacter.attackSheathState = SHEATHE_SLOT.SHEATHE_NONE;
                        }

                        if (objCharacter.Items.ContainsKey(EquipmentSlots.EQUIPMENT_SLOT_OFFHAND) && !objCharacter.Items(EquipmentSlots.EQUIPMENT_SLOT_OFFHAND).IsBroken)
                        {
                            var argItem5 = objCharacter.Items(EquipmentSlots.EQUIPMENT_SLOT_OFFHAND);
                            SetVirtualItemInfo(objCharacter, 1, ref argItem5);
                            objCharacter.Items(EquipmentSlots.EQUIPMENT_SLOT_OFFHAND) = argItem5;
                            // DONE: Must be applyed SPELL_EFFECT_DUAL_WIELD and weapon in offhand
                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "spellCanDualWeild = {0}", objCharacter.spellCanDualWeild);
                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "objectClass = {0}", objCharacter.Items(EquipmentSlots.EQUIPMENT_SLOT_OFFHAND).ItemInfo.ObjectClass);
                            if (objCharacter.spellCanDualWeild && objCharacter.Items(EquipmentSlots.EQUIPMENT_SLOT_OFFHAND).ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_WEAPON)
                                objCharacter.combatCanDualWield = true;
                        }
                        else
                        {
                            ItemObject argItem6 = null;
                            SetVirtualItemInfo(objCharacter, 1, ref argItem6);
                        }

                        ItemObject argItem7 = null;
                        SetVirtualItemInfo(objCharacter, 2, ref argItem7);
                        break;
                    }

                case var case2 when case2 == SHEATHE_SLOT.SHEATHE_RANGED:
                    {
                        ItemObject argItem8 = null;
                        SetVirtualItemInfo(objCharacter, 0, ref argItem8);
                        ItemObject argItem9 = null;
                        SetVirtualItemInfo(objCharacter, 1, ref argItem9);
                        if (objCharacter.Items.ContainsKey(EquipmentSlots.EQUIPMENT_SLOT_RANGED) && !objCharacter.Items(EquipmentSlots.EQUIPMENT_SLOT_RANGED).IsBroken)
                        {
                            var argItem10 = objCharacter.Items(EquipmentSlots.EQUIPMENT_SLOT_RANGED);
                            SetVirtualItemInfo(objCharacter, 2, ref argItem10);
                            objCharacter.Items(EquipmentSlots.EQUIPMENT_SLOT_RANGED) = argItem10;
                        }
                        else
                        {
                            ItemObject argItem11 = null;
                            SetVirtualItemInfo(objCharacter, 2, ref argItem11);
                            objCharacter.attackSheathState = SHEATHE_SLOT.SHEATHE_NONE;
                        }

                        break;
                    }

                default:
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Unhandled sheathe state [{0}]", State);
                        ItemObject argItem12 = null;
                        SetVirtualItemInfo(objCharacter, 0, ref argItem12);
                        ItemObject argItem13 = null;
                        SetVirtualItemInfo(objCharacter, 1, ref argItem13);
                        ItemObject argItem14 = null;
                        SetVirtualItemInfo(objCharacter, 2, ref argItem14);
                        break;
                    }
            }

            objCharacter.SendCharacterUpdate(true);
        }

        public void SetVirtualItemInfo(WS_PlayerData.CharacterObject objChar, byte Slot, ref ItemObject Item)
        {
            if (Slot > 2)
                return;
            if (Item is null)
            {
            }
            // c.SetUpdateFlag(EUnitFields.UNIT_VIRTUAL_ITEM_INFO + Slot * 2, 0)
            // c.SetUpdateFlag(EUnitFields.UNIT_VIRTUAL_ITEM_INFO + Slot * 2 + 1, 0)
            // c.SetUpdateFlag(EUnitFields.UNIT_VIRTUAL_ITEM_SLOT_DISPLAY + Slot, 0)
            else
            {
                // c.SetUpdateFlag(EUnitFields.UNIT_VIRTUAL_ITEM_INFO + Slot * 2, CType(Item.GUID << 32UI >> 32UI, UInteger))
                // c.SetUpdateFlag(EUnitFields.UNIT_VIRTUAL_ITEM_INFO + Slot * 2 + 1, Item.ItemInfo.Sheath)
                // c.SetUpdateFlag(EUnitFields.UNIT_VIRTUAL_ITEM_SLOT_DISPLAY + Slot, Item.ItemInfo.Model)
            }
        }

        public void SendAttackStop(ulong attackerGUID, ulong victimGUID, ref WS_Network.ClientClass client)
        {
            // AttackerGUID stopped attacking victimGUID
            var SMSG_ATTACKSTOP = new Packets.PacketClass(OPCODES.SMSG_ATTACKSTOP);
            SMSG_ATTACKSTOP.AddPackGUID(attackerGUID);
            SMSG_ATTACKSTOP.AddPackGUID(victimGUID);
            SMSG_ATTACKSTOP.AddInt32(0);
            SMSG_ATTACKSTOP.AddInt8(0);
            client.Character.SendToNearPlayers(ref SMSG_ATTACKSTOP);
            SMSG_ATTACKSTOP.Dispose();
        }

        public void SendAttackStart(ulong attackerGUID, ulong victimGUID, [Optional, DefaultParameterValue(null)] ref WS_Network.ClientClass client)
        {
            var SMSG_ATTACKSTART = new Packets.PacketClass(OPCODES.SMSG_ATTACKSTART);
            SMSG_ATTACKSTART.AddUInt64(attackerGUID);
            SMSG_ATTACKSTART.AddUInt64(victimGUID);
            client.Character.SendToNearPlayers(ref SMSG_ATTACKSTART);
            SMSG_ATTACKSTART.Dispose();
        }

        public void SendAttackerStateUpdate(ref WS_Base.BaseObject Attacker, ref WS_Base.BaseObject Victim, DamageInfo damageInfo, [Optional, DefaultParameterValue(null)] ref WS_Network.ClientClass client)
        {
            var packet = new Packets.PacketClass(OPCODES.SMSG_ATTACKERSTATEUPDATE);
            packet.AddInt32(damageInfo.HitInfo);
            packet.AddPackGUID(Attacker.GUID);
            packet.AddPackGUID(Victim.GUID);
            packet.AddInt32(damageInfo.GetDamage);                               // RealDamage

            // TODO: How do we know what type of swing it is?
            packet.AddInt8(SwingTypes.SINGLEHANDEDSWING);                        // Swing type
            packet.AddUInt32(damageInfo.DamageType); // Damage type
            packet.AddSingle(damageInfo.GetDamage);                                 // Damage float
            packet.AddInt32(damageInfo.GetDamage);                                  // Damage amount
            packet.AddInt32(damageInfo.Absorbed);                            // Damage absorbed
            packet.AddInt32(damageInfo.Resist);                              // Damage resisted
            packet.AddInt32(damageInfo.victimState);                              // Victim state
            if (damageInfo.Absorbed == 0)
            {
                packet.AddInt32(0x3E8);
            }
            else
            {
                packet.AddInt32(-1);
            }

            packet.AddInt32(0);
            packet.AddInt32(damageInfo.Blocked);                                 // Damage amount blocked
            if (client is object)
            {
                client.Character.SendToNearPlayers(ref packet);
            }
            else
            {
                Attacker.SendToNearPlayers(ref packet);
            }

            packet.Dispose();
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
    }
}