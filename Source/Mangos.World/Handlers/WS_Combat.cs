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
using Mangos.Common.Enums.Item;
using Mangos.Common.Enums.Player;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Network;
using Mangos.World.Objects;
using Mangos.World.Player;
using Mangos.World.Spells;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Mangos.World.Handlers;

public class WS_Combat
{
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

        public int GetDamage => checked(Damage - Absorbed - Blocked - Resist);
    }

    public class TAttackTimer : IDisposable
    {
        private int LastAttack;

        private Timer NextAttackTimer;

        public WS_Base.BaseUnit Victim;

        public WS_PlayerData.CharacterObject Character;

        public float combatReach;

        public float minRanged;

        public bool combatDualWield;

        public bool Ranged;

        private int TimeLeftMainHand;

        private int TimeLeftOffHand;

        private bool _disposedValue;

        public AutoResetEvent combatNextAttack;

        public bool combatNextAttackSpell;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                NextAttackTimer.Dispose();
                NextAttackTimer = null;
                combatNextAttack.Dispose();
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

        public TAttackTimer(ref WS_Base.BaseObject Victim_, ref WS_PlayerData.CharacterObject Character_)
        {
            LastAttack = 0;
            NextAttackTimer = null;
            combatDualWield = false;
            Ranged = false;
            TimeLeftMainHand = -1;
            TimeLeftOffHand = -1;
            combatNextAttack = new AutoResetEvent(initialState: false);
            combatNextAttackSpell = false;
            NextAttackTimer = new Timer(DoAttack, null, 1000, -1);
            Victim = (WS_Base.BaseUnit)Victim_;
            Character = Character_;
        }

        public TAttackTimer(ref WS_PlayerData.CharacterObject Character_)
        {
            LastAttack = 0;
            NextAttackTimer = null;
            combatDualWield = false;
            Ranged = false;
            TimeLeftMainHand = -1;
            TimeLeftOffHand = -1;
            combatNextAttack = new AutoResetEvent(initialState: false);
            combatNextAttackSpell = false;
            NextAttackTimer = new Timer(DoAttack, null, -1, -1);
            Character = Character_;
            Victim = null;
        }

        public void AttackStop()
        {
            if (Character.AutoShotSpell <= 0)
            {
                NextAttackTimer.Change(-1, -1);
                Victim = null;
                Ranged = false;
            }
        }

        public void AttackStart(WS_Base.BaseUnit Victim_ = null)
        {
            if (Victim == null)
            {
                Victim = Victim_;
                combatReach = 2f + Victim.BoundingRadius + Character.CombatReach;
                minRanged = Victim.BoundingRadius + 8f;
            }
            else
            {
                if (Victim.GUID == Victim_.GUID)
                {
                    return;
                }
                WorldServiceLocator._WS_Combat.SendAttackStop(Character.GUID, Victim.GUID, ref Character.client);
                Victim = Victim_;
                combatReach = 2f + Victim.BoundingRadius + Character.CombatReach;
                minRanged = Victim.BoundingRadius + 8f;
            }
            var wS_Combat = WorldServiceLocator._WS_Combat;
            ref var character = ref Character;
            var flag = false;
            var AttackSpeed = wS_Combat.GetAttackTime(ref character, ref flag);
            checked
            {
                if (WorldServiceLocator._NativeMethods.timeGetTime("") - LastAttack >= AttackSpeed)
                {
                    DoAttack(null);
                }
                else
                {
                    NextAttackTimer.Change(WorldServiceLocator._NativeMethods.timeGetTime("") - LastAttack, -1);
                }
            }
        }

        public void DoAttack(object Status)
        {
            if (Victim == null)
            {
                Character.AutoShotSpell = 0;
                AttackStop();
                return;
            }
            LastAttack = WorldServiceLocator._NativeMethods.timeGetTime("");
            Character.RemoveAurasByInterruptFlag(4096);
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
            catch (Exception ex2)
            {
                ProjectData.SetProjectError(ex2);
                var ex = ex2;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error doing attack.{0}{1}", Environment.NewLine, ex.ToString());
                ProjectData.ClearProjectError();
            }
        }

        public void DoMeleeAttack(object Status)
        {
            if (Victim == null)
            {
                Packets.PacketClass SMSG_ATTACKSWING_CANT_ATTACK2 = new(Opcodes.SMSG_ATTACKSWING_CANT_ATTACK);
                Character.client.Send(ref SMSG_ATTACKSWING_CANT_ATTACK2);
                SMSG_ATTACKSWING_CANT_ATTACK2.Dispose();
                Character.AutoShotSpell = 0;
                AttackStop();
                return;
            }
            checked
            {
                try
                {
                    bool flag;
                    if (Character.spellCasted[1] != null && !Character.spellCasted[1].Finished)
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "AttackStop: Casting Spell");
                        var nextAttackTimer = NextAttackTimer;
                        var wS_Combat = WorldServiceLocator._WS_Combat;
                        ref var character = ref Character;
                        flag = false;
                        nextAttackTimer.Change(wS_Combat.GetAttackTime(ref character, ref flag), -1);
                        return;
                    }
                    if (Victim.IsDead)
                    {
                        Packets.PacketClass SMSG_ATTACKSWING_DEADTARGET = new(Opcodes.SMSG_ATTACKSWING_DEADTARGET);
                        Character.client.Send(ref SMSG_ATTACKSWING_DEADTARGET);
                        SMSG_ATTACKSWING_DEADTARGET.Dispose();
                        Character.AutoShotSpell = 0;
                        AttackStop();
                        return;
                    }
                    if (Character.IsDead)
                    {
                        Packets.PacketClass SMSG_ATTACKSWING_DEADTARGET2 = new(Opcodes.SMSG_ATTACKSWING_DEADTARGET);
                        Character.client.Send(ref SMSG_ATTACKSWING_DEADTARGET2);
                        SMSG_ATTACKSWING_DEADTARGET2.Dispose();
                        Character.AutoShotSpell = 0;
                        AttackStop();
                        return;
                    }
                    if (Character.StandState > 0)
                    {
                        Packets.PacketClass SMSG_ATTACKSWING_NOTSTANDING = new(Opcodes.SMSG_ATTACKSWING_NOTSTANDING);
                        Character.client.Send(ref SMSG_ATTACKSWING_NOTSTANDING);
                        SMSG_ATTACKSWING_NOTSTANDING.Dispose();
                        Character.AutoShotSpell = 0;
                        AttackStop();
                        return;
                    }
                    if (Victim is WS_Creatures.CreatureObject @object)
                    {
                        @object.SetToRealPosition();
                    }
                    var tmpPosX = Victim.positionX;
                    var tmpPosY = Victim.positionY;
                    var tmpPosZ = Victim.positionZ;
                    var tmpDist = WorldServiceLocator._WS_Combat.GetDistance(Character, tmpPosX, tmpPosY, tmpPosZ);
                    if (tmpDist > 8f + Victim.CombatReach)
                    {
                        if (Character.CanShootRanged)
                        {
                            Ranged = true;
                            DoRangedAttack(null);
                            return;
                        }
                        NextAttackTimer.Change(2000, -1);
                        Packets.PacketClass SMSG_ATTACKSWING_NOTINRANGE2 = new(Opcodes.SMSG_ATTACKSWING_NOTINRANGE);
                        Character.client.Send(ref SMSG_ATTACKSWING_NOTINRANGE2);
                        SMSG_ATTACKSWING_NOTINRANGE2.Dispose();
                        return;
                    }
                    if (tmpDist > combatReach)
                    {
                        NextAttackTimer.Change(2000, -1);
                        Packets.PacketClass SMSG_ATTACKSWING_NOTINRANGE = new(Opcodes.SMSG_ATTACKSWING_NOTINRANGE);
                        Character.client.Send(ref SMSG_ATTACKSWING_NOTINRANGE);
                        SMSG_ATTACKSWING_NOTINRANGE.Dispose();
                        return;
                    }
                    var wS_Combat2 = WorldServiceLocator._WS_Combat;
                    ref var character2 = ref Character;
                    WS_Base.BaseObject Object = character2;
                    flag = wS_Combat2.IsInFrontOf(ref Object, tmpPosX, tmpPosY);
                    character2 = (WS_PlayerData.CharacterObject)Object;
                    if (!flag)
                    {
                        NextAttackTimer.Change(2000, -1);
                        Packets.PacketClass SMSG_ATTACKSWING_BADFACING = new(Opcodes.SMSG_ATTACKSWING_BADFACING);
                        Character.client.Send(ref SMSG_ATTACKSWING_BADFACING);
                        SMSG_ATTACKSWING_BADFACING.Dispose();
                        return;
                    }
                    var HaveMainHand = Character.GetAttackTime(WeaponAttackType.BASE_ATTACK) > 0 && Character.Items.ContainsKey(15);
                    var HaveOffHand = Character.GetAttackTime(WeaponAttackType.OFF_ATTACK) > 0 && Character.Items.ContainsKey(16);
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
                    var NextAttack = WorldServiceLocator._WS_Combat.GetAttackTime(ref Character, ref combatDualWield);
                    if (HaveMainHand && HaveOffHand)
                    {
                        if (combatDualWield)
                        {
                            if (TimeLeftMainHand == -1)
                            {
                                TimeLeftMainHand = (int)Math.Round(Character.GetAttackTime(WeaponAttackType.OFF_ATTACK) / 2.0);
                            }
                            TimeLeftOffHand = Character.GetAttackTime(WeaponAttackType.OFF_ATTACK);
                        }
                        else
                        {
                            if (TimeLeftMainHand == -1)
                            {
                                TimeLeftOffHand = (int)Math.Round(Character.GetAttackTime(WeaponAttackType.BASE_ATTACK) / 2.0);
                            }
                            TimeLeftMainHand = Character.GetAttackTime(WeaponAttackType.BASE_ATTACK);
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
                        Character.CommandResponse("NO: " + Conversions.ToString(TimeLeftOffHand));
                        Character.CommandResponse("NM: " + Conversions.ToString(TimeLeftMainHand));
                    }
                    else
                    {
                        TimeLeftMainHand = -1;
                        combatDualWield = HaveOffHand;
                    }
                    NextAttackTimer.Change(NextAttack, -1);
                }
                catch (Exception ex)
                {
                    ProjectData.SetProjectError(ex);
                    var e = ex;
                    if (Character != null && Character.client != null)
                    {
                        Packets.PacketClass SMSG_ATTACKSWING_CANT_ATTACK = new(Opcodes.SMSG_ATTACKSWING_CANT_ATTACK);
                        Character.client.Send(ref SMSG_ATTACKSWING_CANT_ATTACK);
                        SMSG_ATTACKSWING_CANT_ATTACK.Dispose();
                    }
                    AttackStop();
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Error while doing melee attack.{0}", Environment.NewLine + e);
                    ProjectData.ClearProjectError();
                }
            }
        }

        public void DoRangedAttack(object Status)
        {
            if (Victim is WS_Creatures.CreatureObject @object)
            {
                @object.SetToRealPosition();
            }
            var tmpPosX = Victim.positionX;
            var tmpPosY = Victim.positionY;
            var tmpPosZ = Victim.positionZ;
            bool flag;
            if (Character.spellCasted[1] != null && !Character.spellCasted[1].Finished)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "AttackPause: Casting Spell");
                var nextAttackTimer = NextAttackTimer;
                var wS_Combat = WorldServiceLocator._WS_Combat;
                ref var character = ref Character;
                flag = false;
                nextAttackTimer.Change(wS_Combat.GetAttackTime(ref character, ref flag), -1);
                return;
            }
            if (Victim.Life.Current == 0)
            {
                Packets.PacketClass SMSG_ATTACKSWING_DEADTARGET2 = new(Opcodes.SMSG_ATTACKSWING_DEADTARGET);
                Character.client.Send(ref SMSG_ATTACKSWING_DEADTARGET2);
                SMSG_ATTACKSWING_DEADTARGET2.Dispose();
                Character.AutoShotSpell = 0;
                AttackStop();
                return;
            }
            if (Character.DEAD)
            {
                Packets.PacketClass SMSG_ATTACKSWING_DEADTARGET = new(Opcodes.SMSG_ATTACKSWING_DEADTARGET);
                Character.client.Send(ref SMSG_ATTACKSWING_DEADTARGET);
                SMSG_ATTACKSWING_DEADTARGET.Dispose();
                Character.AutoShotSpell = 0;
                AttackStop();
                return;
            }
            if (Character.StandState > 0)
            {
                Packets.PacketClass SMSG_ATTACKSWING_NOTSTANDING = new(Opcodes.SMSG_ATTACKSWING_NOTSTANDING);
                Character.client.Send(ref SMSG_ATTACKSWING_NOTSTANDING);
                SMSG_ATTACKSWING_NOTSTANDING.Dispose();
                Character.AutoShotSpell = 0;
                AttackStop();
                return;
            }
            var tmpDist = WorldServiceLocator._WS_Combat.GetDistance(Character, tmpPosX, tmpPosY, tmpPosZ);
            if (tmpDist < combatReach)
            {
                Ranged = false;
                DoMeleeAttack(null);
                return;
            }
            if (tmpDist < minRanged)
            {
                NextAttackTimer.Change(2000, -1);
                Packets.PacketClass SMSG_ATTACKSWING_NOTINRANGE = new(Opcodes.SMSG_ATTACKSWING_NOTINRANGE);
                Character.client.Send(ref SMSG_ATTACKSWING_NOTINRANGE);
                SMSG_ATTACKSWING_NOTINRANGE.Dispose();
                return;
            }
            var wS_Combat2 = WorldServiceLocator._WS_Combat;
            ref var character2 = ref Character;
            WS_Base.BaseObject Object = character2;
            flag = wS_Combat2.IsInFrontOf(ref Object, tmpPosX, tmpPosY);
            character2 = (WS_PlayerData.CharacterObject)Object;
            if (!flag)
            {
                NextAttackTimer.Change(2000, -1);
                Packets.PacketClass SMSG_ATTACKSWING_BADFACING = new(Opcodes.SMSG_ATTACKSWING_BADFACING);
                Character.client.Send(ref SMSG_ATTACKSWING_BADFACING);
                SMSG_ATTACKSWING_BADFACING.Dispose();
            }
            else
            {
                DoRangedDamage();
                var nextAttackTimer2 = NextAttackTimer;
                var wS_Combat3 = WorldServiceLocator._WS_Combat;
                ref var character3 = ref Character;
                flag = false;
                nextAttackTimer2.Change(wS_Combat3.GetAttackTime(ref character3, ref flag), -1);
            }
        }

        public void DoMeleeDamage()
        {
            var wS_Combat = WorldServiceLocator._WS_Combat;
            ref var character = ref Character;
            ref var reference = ref character;
            WS_Base.BaseUnit Attacker = character;
            var damageInfo2 = wS_Combat.CalculateDamage(ref Attacker, ref Victim, combatDualWield, Ranged: false);
            reference = (WS_PlayerData.CharacterObject)Attacker;
            var damageInfo = damageInfo2;
            var wS_Combat2 = WorldServiceLocator._WS_Combat;
            ref var character2 = ref Character;
            reference = ref character2;
            WS_Base.BaseObject Attacker2 = character2;
            ref var victim = ref Victim;
            ref var reference2 = ref victim;
            WS_Base.BaseObject baseObject = victim;
            wS_Combat2.SendAttackerStateUpdate(ref Attacker2, ref baseObject, damageInfo, Character.client);
            reference2 = (WS_Base.BaseUnit)baseObject;
            reference = (WS_PlayerData.CharacterObject)Attacker2;
            WS_Spells.SpellTargets Target = new();
            var spellTargets = Target;
            ref var character3 = ref Character;
            reference = ref character3;
            Attacker = character3;
            spellTargets.SetTarget_UNIT(ref Attacker);
            reference = (WS_PlayerData.CharacterObject)Attacker;
            checked
            {
                var b = (byte)(WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1);
                byte i = 0;
                while (i <= (uint)b)
                {
                    if (Victim.ActiveSpells[i] != null && (unchecked((uint)Victim.ActiveSpells[i].GetSpellInfo.procFlags) & (true ? 1u : 0u)) != 0)
                    {
                        byte j = 0;
                        do
                        {
                            if (Victim.ActiveSpells[i].Aura_Info[j] != null && Victim.ActiveSpells[i].Aura_Info[j].ApplyAuraIndex == 42 && WorldServiceLocator._Functions.RollChance(Victim.ActiveSpells[i].GetSpellInfo.procChance))
                            {
                                ref var victim2 = ref Victim;
                                reference2 = ref victim2;
                                baseObject = victim2;
                                WS_Spells.CastSpellParameters castSpellParameters = new(ref Target, ref baseObject, Victim.ActiveSpells[i].Aura_Info[j].TriggerSpell, Instant: true);
                                reference2 = (WS_Base.BaseUnit)baseObject;
                                var castParams = castSpellParameters;
                                castParams.Cast(null);
                            }
                            j = (byte)unchecked((uint)(j + 1));
                        }
                        while (j <= 2u);
                    }
                    i = (byte)unchecked((uint)(i + 1));
                }
                if (Character.Classe == Classes.CLASS_WARRIOR || (Character.Classe == Classes.CLASS_DRUID && (Character.ShapeshiftForm == ShapeshiftForm.FORM_BEAR || Character.ShapeshiftForm == ShapeshiftForm.FORM_DIREBEAR)))
                {
                    Character.Rage.Increment((int)(((7.5 * damageInfo.Damage / Character.GetRageConversion) + (Character.GetHitFactor((damageInfo.HitInfo & 4) == 0, (damageInfo.HitInfo & 0x200) != 0) * WorldServiceLocator._WS_Combat.GetAttackTime(ref Character, ref combatDualWield))) / 2.0));
                    Character.SetUpdateFlag(24, Character.Rage.Current);
                    Character.SendCharacterUpdate();
                }
                var victim3 = Victim;
                var getDamage = damageInfo.GetDamage;
                ref var character4 = ref Character;
                reference = ref character4;
                Attacker = character4;
                victim3.DealDamage(getDamage, Attacker);
                reference = (WS_PlayerData.CharacterObject)Attacker;
                if (Victim == null || Victim.IsDead)
                {
                    AttackStop();
                }
            }
        }

        public void DoRangedDamage()
        {
            WS_Spells.SpellTargets Targets = new();
            Targets.SetTarget_UNIT(ref Victim);
            var SpellID = (Character.AutoShotSpell <= 0) ? 75 : Character.AutoShotSpell;
            ref var character = ref Character;
            WS_Base.BaseObject Caster = character;
            WS_Spells.CastSpellParameters castSpellParameters = new(ref Targets, ref Caster, SpellID, Instant: true);
            character = (WS_PlayerData.CharacterObject)Caster;
            var tmpSpell = castSpellParameters;
            ThreadPool.QueueUserWorkItem(tmpSpell.Cast);
        }

        public void DoMeleeDamageBySpell(ref WS_PlayerData.CharacterObject Character, ref WS_Base.BaseObject Victim2, int BonusDamage, int SpellID)
        {
            var wS_Combat = WorldServiceLocator._WS_Combat;
            WS_Base.BaseUnit Attacker = Character;
            WS_Base.BaseUnit baseUnit = (WS_Base.BaseUnit)Victim2;
            var damageInfo2 = wS_Combat.CalculateDamage(ref Attacker, ref baseUnit, DualWield: false, Ranged: false, WorldServiceLocator._WS_Spells.SPELLs[SpellID]);
            Victim2 = baseUnit;
            Character = (WS_PlayerData.CharacterObject)Attacker;
            var damageInfo = damageInfo2;
            var IsCrit = false;
            checked
            {
                if (damageInfo.Damage > 0)
                {
                    damageInfo.Damage += BonusDamage;
                }
                if (damageInfo.HitInfo == 512)
                {
                    damageInfo.Damage += BonusDamage;
                    IsCrit = true;
                }
                var wS_Spells = WorldServiceLocator._WS_Spells;
                baseUnit = Character;
                Attacker = (WS_Base.BaseUnit)Victim2;
                wS_Spells.SendNonMeleeDamageLog(ref baseUnit, ref Attacker, SpellID, (int)damageInfo.DamageType, damageInfo.Damage, 0, damageInfo.Absorbed, IsCrit);
                Victim2 = Attacker;
                Character = (WS_PlayerData.CharacterObject)baseUnit;
                if (Victim2 is WS_Creatures.CreatureObject obj)
                {
                    var getDamage = damageInfo.GetDamage;
                    Attacker = Character;
                    obj.DealDamage(getDamage, Attacker);
                    Character = (WS_PlayerData.CharacterObject)Attacker;
                    if (Victim2 == Victim && ((WS_Creatures.CreatureObject)Victim).IsDead)
                    {
                        AttackStop();
                    }
                }
                else if (Victim2 is WS_PlayerData.CharacterObject obj2)
                {
                    var getDamage2 = damageInfo.GetDamage;
                    Attacker = Character;
                    obj2.DealDamage(getDamage2, Attacker);
                    Character = (WS_PlayerData.CharacterObject)Attacker;
                    if (((WS_PlayerData.CharacterObject)Victim2).Classe == Classes.CLASS_WARRIOR)
                    {
                        ((WS_PlayerData.CharacterObject)Victim2).Rage.Increment((int)((damageInfo.Damage / (double)(((WS_PlayerData.CharacterObject)Victim2).Level * 4) * 25.0) + 10.0));
                        ((WS_PlayerData.CharacterObject)Victim2).SetUpdateFlag(24, ((WS_PlayerData.CharacterObject)Victim2).Rage.Current);
                        Character.SendCharacterUpdate();
                    }
                }
                if (Character.Classe == Classes.CLASS_WARRIOR || (Character.Classe == Classes.CLASS_DRUID && (Character.ShapeshiftForm == ShapeshiftForm.FORM_BEAR || Character.ShapeshiftForm == ShapeshiftForm.FORM_DIREBEAR)))
                {
                    Character.Rage.Increment((int)((damageInfo.Damage / (double)(Character.Level * 4) * 75.0) + 10.0));
                    Character.SetUpdateFlag(24, Character.Rage.Current);
                    Character.SendCharacterUpdate();
                }
            }
        }
    }

    public void DoEmote(int AnimationID, ref WS_Base.BaseObject Unit)
    {
        Packets.PacketClass packet = new(Opcodes.SMSG_EMOTE);
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
            case WeaponAttackType.BASE_ATTACK:
                WepSlot = 15;
                break;

            case WeaponAttackType.OFF_ATTACK:
                WepSlot = 16;
                break;

            case WeaponAttackType.RANGED_ATTACK:
                WepSlot = 17;
                break;

            default:
                return 0f;
        }
        if (!objCharacter.Items.ContainsKey(WepSlot) || objCharacter.Items[WepSlot].ItemInfo.ObjectClass != ITEM_CLASS.ITEM_CLASS_WEAPON || objCharacter.Items[WepSlot].IsBroken())
        {
            return 0f;
        }
        var Dmg = 0f;
        byte i = 0;
        do
        {
            Dmg = (!MaxDmg) ? (Dmg + objCharacter.Items[WepSlot].ItemInfo.Damage[i].Minimum) : (Dmg + objCharacter.Items[WepSlot].ItemInfo.Damage[i].Maximum);
            checked
            {
                i = (byte)unchecked((uint)(i + 1));
            }
        }
        while (i <= 4u);
        return Dmg;
    }

    public float GetAPMultiplier(ref WS_Base.BaseUnit objCharacter, WeaponAttackType AttackType, bool Normalized)
    {
        if (!Normalized || objCharacter is not WS_PlayerData.CharacterObject)
        {
            return AttackType switch
            {
                WeaponAttackType.BASE_ATTACK => ((WS_Creatures.CreatureObject)objCharacter).CreatureInfo.BaseAttackTime / 1000f,
                WeaponAttackType.RANGED_ATTACK => ((WS_Creatures.CreatureObject)objCharacter).CreatureInfo.BaseRangedAttackTime / 1000f,
                _ => 0f,
            };
        }
        ItemObject Weapon;
        switch (AttackType)
        {
            case WeaponAttackType.BASE_ATTACK:
                if (!((WS_PlayerData.CharacterObject)objCharacter).Items.ContainsKey(15))
                {
                    return 2.4f;
                }
                Weapon = ((WS_PlayerData.CharacterObject)objCharacter).Items[15];
                break;

            case WeaponAttackType.OFF_ATTACK:
                if (!((WS_PlayerData.CharacterObject)objCharacter).Items.ContainsKey(16))
                {
                    return 2.4f;
                }
                Weapon = ((WS_PlayerData.CharacterObject)objCharacter).Items[16];
                break;

            case WeaponAttackType.RANGED_ATTACK:
                if (!((WS_PlayerData.CharacterObject)objCharacter).Items.ContainsKey(17))
                {
                    return 0f;
                }
                Weapon = ((WS_PlayerData.CharacterObject)objCharacter).Items[17];
                break;

            default:
                return 0f;
        }
        if (Weapon == null || Weapon.ItemInfo.ObjectClass != ITEM_CLASS.ITEM_CLASS_WEAPON)
        {
            return AttackType == WeaponAttackType.RANGED_ATTACK ? 0f : 2.4f;
        }
        switch (Weapon.ItemInfo.InventoryType)
        {
            case INVENTORY_TYPES.INVTYPE_TWOHAND_WEAPON:
                return 3.3f;

            case INVENTORY_TYPES.INVTYPE_RANGED:
            case INVENTORY_TYPES.INVTYPE_THROWN:
            case INVENTORY_TYPES.INVTYPE_RANGEDRIGHT:
                return 2.8f;

            default:
                if (Weapon.ItemInfo.SubClass == ITEM_SUBCLASS.ITEM_SUBCLASS_DAGGER)
                {
                    return 1.7f;
                }
                return 2.4f;
        }
    }

    public void CalculateMinMaxDamage(ref WS_PlayerData.CharacterObject objCharacter, WeaponAttackType AttackType)
    {
        WS_Base.BaseUnit objCharacter2 = objCharacter;
        var aPMultiplier = GetAPMultiplier(ref objCharacter2, AttackType, Normalized: true);
        objCharacter = (WS_PlayerData.CharacterObject)objCharacter2;
        var AttSpeed = aPMultiplier;
        var BasePercent = 1f;
        float BaseValue;
        float WepMin;
        float WepMax;
        checked
        {
            switch (AttackType)
            {
                default:
                    return;

                case WeaponAttackType.BASE_ATTACK:
                case WeaponAttackType.OFF_ATTACK:
                    BaseValue = objCharacter.AttackPower + objCharacter.AttackPowerMods;
                    break;

                case WeaponAttackType.RANGED_ATTACK:
                    BaseValue = objCharacter.AttackPowerRanged + objCharacter.AttackPowerModsRanged;
                    break;
            }
            BaseValue = BaseValue / 14f * AttSpeed;
            WepMin = GetWeaponDmg(ref objCharacter, AttackType, MaxDmg: false);
            WepMax = GetWeaponDmg(ref objCharacter, AttackType, MaxDmg: true);
        }
        if (AttackType == WeaponAttackType.RANGED_ATTACK)
        {
            if (objCharacter.AmmoID > 0)
            {
                var AmmoDmg = objCharacter.AmmoDPS / (1f / objCharacter.AmmoMod) * AttSpeed;
                WepMin += AmmoDmg;
                WepMax += AmmoDmg;
            }
        }
        else if (objCharacter.ShapeshiftForm is ShapeshiftForm.FORM_BEAR or ShapeshiftForm.FORM_DIREBEAR or ShapeshiftForm.FORM_CAT)
        {
            WepMin = (float)(WepMin + (objCharacter.Level * 0.85 * AttSpeed));
            WepMax = (float)(WepMax + (objCharacter.Level * 0.85 * AttSpeed));
        }
        var MinDamage = (BaseValue + WepMin) * BasePercent;
        var MaxDamage = (BaseValue + WepMax) * BasePercent;
        switch (AttackType)
        {
            case WeaponAttackType.BASE_ATTACK:
                objCharacter.Damage.Minimum = MinDamage;
                objCharacter.Damage.Maximum = MaxDamage;
                objCharacter.SetUpdateFlag(134, objCharacter.Damage.Minimum);
                objCharacter.SetUpdateFlag(135, objCharacter.Damage.Maximum);
                break;

            case WeaponAttackType.OFF_ATTACK:
                objCharacter.OffHandDamage.Minimum = MinDamage;
                objCharacter.OffHandDamage.Maximum = MaxDamage;
                objCharacter.SetUpdateFlag(136, objCharacter.OffHandDamage.Minimum);
                objCharacter.SetUpdateFlag(137, objCharacter.OffHandDamage.Maximum);
                break;

            case WeaponAttackType.RANGED_ATTACK:
                objCharacter.RangedDamage.Minimum = MinDamage;
                objCharacter.RangedDamage.Maximum = MaxDamage;
                objCharacter.SetUpdateFlag(171, objCharacter.RangedDamage.Minimum);
                objCharacter.SetUpdateFlag(172, objCharacter.RangedDamage.Maximum);
                break;
        }
    }

    public DamageInfo CalculateDamage(ref WS_Base.BaseUnit Attacker, ref WS_Base.BaseUnit Victim, bool DualWield, bool Ranged, WS_Spells.SpellInfo Ability = null, WS_Spells.SpellEffect Effect = null)
    {
        DamageInfo result = default;
        result.victimState = AttackVictimState.VICTIMSTATE_NORMAL;
        result.Blocked = 0;
        result.Absorbed = 0;
        result.Turn = 0;
        result.HitInfo = 0;
        if (DualWield)
        {
            result.HitInfo |= 4;
        }
        if (Ability != null)
        {
            result.DamageType = (DamageTypes)checked((byte)Ability.School);
        }
        else if (Attacker is WS_PlayerData.CharacterObject characterObject)
        {
            if (Ranged)
            {
                if (characterObject.Items.ContainsKey(17))
                {
                    result.DamageType = (DamageTypes)checked((byte)characterObject.Items[17].ItemInfo.Damage[0].Type);
                }
            }
            else if (DualWield)
            {
                if (characterObject.Items.ContainsKey(16))
                {
                    result.DamageType = (DamageTypes)checked((byte)characterObject.Items[16].ItemInfo.Damage[0].Type);
                }
            }
            else if (characterObject.Items.ContainsKey(15))
            {
                result.DamageType = (DamageTypes)checked((byte)characterObject.Items[15].ItemInfo.Damage[0].Type);
            }
        }
        else
        {
            result.DamageType = DamageTypes.DMG_PHYSICAL;
        }
        if (Victim is WS_Creatures.CreatureObject @object && @object.aiScript != null && @object.aiScript.State == AIState.AI_MOVING_TO_SPAWN)
        {
            result.HitInfo |= 16;
            return result;
        }
        var skillDiference = GetSkillWeapon(ref Attacker, DualWield);
        checked
        {
            skillDiference -= GetSkillDefence(ref Victim);
            if (Victim is WS_PlayerData.CharacterObject object1)
            {
                object1.UpdateSkill(95);
            }
            var chanceToMiss = GetBasePercentMiss(ref Attacker, skillDiference);
            var chanceToCrit = GetBasePercentCrit(ref Attacker, skillDiference);
            var chanceToBlock = GetBasePercentBlock(ref Victim, skillDiference);
            var chanceToParry = GetBasePercentParry(ref Victim, skillDiference);
            var chanceToDodge = GetBasePercentDodge(ref Victim, skillDiference);
            short chanceToGlancingBlow = 0;
            if (Attacker is WS_PlayerData.CharacterObject && Victim is WS_Creatures.CreatureObject && Attacker.Level > Victim.Level + 2 && skillDiference <= -15)
            {
                chanceToGlancingBlow = (short)((Victim.Level - Attacker.Level) * 10);
            }
            short chanceToCrushingBlow = 0;
            if (Attacker is WS_Creatures.CreatureObject && Victim is WS_PlayerData.CharacterObject && Ability == null && Attacker.Level > Victim.Level + 2)
            {
                chanceToCrushingBlow = (short)Math.Round((skillDiference * 2f) - 15f);
            }
            if (chanceToMiss > 60f)
            {
                chanceToMiss = 60f;
            }
            if (chanceToGlancingBlow > 40f)
            {
                chanceToGlancingBlow = 40;
            }
            if (chanceToMiss < 0f)
            {
                chanceToMiss = 0f;
            }
            if (chanceToCrit < 0f)
            {
                chanceToCrit = 0f;
            }
            if (chanceToBlock < 0f)
            {
                chanceToBlock = 0f;
            }
            if (chanceToParry < 0f)
            {
                chanceToParry = 0f;
            }
            if (chanceToDodge < 0f)
            {
                chanceToDodge = 0f;
            }
            if (chanceToGlancingBlow < 0f)
            {
                chanceToGlancingBlow = 0;
            }
            if (chanceToCrushingBlow < 0f)
            {
                chanceToCrushingBlow = 0;
            }
            if (Victim is WS_PlayerData.CharacterObject object2 && object2.StandState != 0)
            {
                chanceToCrit = 100f;
                chanceToCrushingBlow = 0;
            }
            if (Ranged)
            {
                chanceToGlancingBlow = 0;
            }
            GetDamage(ref Attacker, DualWield, ref result);
            if (Effect != null)
            {
                result.Damage += Effect.GetValue(Attacker.Level, 0);
            }
            var DamageReduction = Victim.GetDamageReduction(ref Attacker, result.DamageType, result.Damage);
            ref var damage = ref result.Damage;
            damage = (int)Math.Round(damage - (result.Damage * DamageReduction));
            var roll = (float)(WorldServiceLocator._WorldServer.Rnd.Next(0, 10000) / 100.0);
            var num = roll;
            if (num < chanceToMiss)
            {
                result.Damage = 0;
                result.HitInfo |= 16;
            }
            else if (num < chanceToMiss + chanceToDodge)
            {
                result.Damage = 0;
                result.victimState = AttackVictimState.VICTIMSTATE_DODGE;
                WS_Base.BaseObject Unit = Victim;
                DoEmote(39, ref Unit);
                Victim = (WS_Base.BaseUnit)Unit;
                Victim.AuraState |= 1;
                if (Victim is WS_PlayerData.CharacterObject object3)
                {
                    object3.SetUpdateFlag(125, Victim.AuraState);
                    object3.SendCharacterUpdate();
                }
            }
            else if (num < chanceToMiss + chanceToDodge + chanceToParry)
            {
                result.Damage = 0;
                result.victimState = AttackVictimState.VICTIMSTATE_PARRY;
                WS_Base.BaseObject Unit = Victim;
                DoEmote(39, ref Unit);
                Victim = (WS_Base.BaseUnit)Unit;
                Victim.AuraState |= 0x40;
                if (Victim is WS_PlayerData.CharacterObject object3)
                {
                    object3.SetUpdateFlag(125, Victim.AuraState);
                    object3.SendCharacterUpdate();
                }
            }
            else if (num < chanceToMiss + chanceToDodge + chanceToParry + chanceToGlancingBlow)
            {
                ref var damage2 = ref result.Damage;
                damage2 = (int)Math.Round(damage2 - Conversion.Fix(skillDiference * 0.03f * result.Damage));
                result.HitInfo |= 2;
                result.HitInfo |= 65536;
            }
            else if (num < chanceToMiss + chanceToDodge + chanceToParry + chanceToGlancingBlow + chanceToBlock)
            {
                if (Victim is WS_PlayerData.CharacterObject object3)
                {
                    result.Blocked = (int)Math.Round(object3.combatBlockValue + (object3.Strength.Base / 20.0));
                    if (object3.combatBlockValue != 0)
                    {
                        WS_Base.BaseObject Unit = Victim;
                        DoEmote(43, ref Unit);
                        Victim = (WS_Base.BaseUnit)Unit;
                    }
                    else
                    {
                        WS_Base.BaseObject Unit = Victim;
                        DoEmote(39, ref Unit);
                        Victim = (WS_Base.BaseUnit)Unit;
                    }
                    result.victimState = AttackVictimState.VICTIMSTATE_BLOCKS;
                }
                result.HitInfo |= 2;
                if (result.GetDamage <= 0)
                {
                    result.HitInfo |= 2048;
                }
            }
            else if (num < chanceToMiss + chanceToDodge + chanceToParry + chanceToGlancingBlow + chanceToBlock + chanceToCrit)
            {
                result.Damage *= 2;
                result.HitInfo |= 2;
                result.HitInfo |= 512;
                WS_Base.BaseObject Unit = Victim;
                DoEmote(34, ref Unit);
                Victim = (WS_Base.BaseUnit)Unit;
            }
            else if (num < chanceToMiss + chanceToDodge + chanceToParry + chanceToGlancingBlow + chanceToBlock + chanceToCrit + chanceToCrushingBlow)
            {
                result.Damage = (result.Damage * 3) >> 1;
                result.HitInfo |= 2;
                result.HitInfo |= 32768;
            }
            else
            {
                result.HitInfo |= 2;
            }
            if (result.GetDamage > 0 && result.DamageType != 0)
            {
                result.Resist = (int)Math.Round(Victim.GetResist(ref Attacker, result.DamageType, result.Damage));
                if (result.GetDamage <= 0)
                {
                    result.HitInfo |= 64;
                }
            }
            if (result.GetDamage > 0)
            {
                result.Absorbed = Victim.GetAbsorb(result.DamageType, result.Damage);
                if (result.GetDamage <= 0)
                {
                    result.HitInfo |= 32;
                }
            }
            return result;
        }
    }

    public float GetBasePercentDodge(ref WS_Base.BaseUnit objCharacter, int skillDiference)
    {
        if (objCharacter is WS_PlayerData.CharacterObject @object)
        {
            if (((uint)objCharacter.cUnitFlags & 0x40000u) != 0)
            {
                return 0f;
            }
            checked
            {
                if (@object.combatDodge > 0)
                {
                    var combatDodgeAgilityBonus = @object.Classe switch
                    {
                        Classes.CLASS_HUNTER => (int)(@object.Agility.Base / 26.5f),
                        Classes.CLASS_ROGUE => (int)(@object.Agility.Base / 14.5f),
                        Classes.CLASS_PALADIN or Classes.CLASS_MAGE or Classes.CLASS_WARLOCK => (int)(@object.Agility.Base / 19.5f),
                        _ => (int)(@object.Agility.Base / 20.0),
                    };
                    return @object.combatDodge + combatDodgeAgilityBonus - (skillDiference * 0.04f);
                }
            }
        }
        return 0f;
    }

    public float GetBasePercentParry(ref WS_Base.BaseUnit objCharacter, int skillDiference)
    {
        return objCharacter switch
        {
            WS_PlayerData.CharacterObject _ when ((WS_PlayerData.CharacterObject)objCharacter).combatParry > 0 => ((WS_PlayerData.CharacterObject)objCharacter).combatParry - (skillDiference * 0.04f),
            _ => 0f
        };
    }

    public float GetBasePercentBlock(ref WS_Base.BaseUnit objCharacter, int skillDiference)
    {
        return objCharacter switch
        {
            WS_PlayerData.CharacterObject _ when ((WS_PlayerData.CharacterObject)objCharacter).combatBlock > 0 => ((WS_PlayerData.CharacterObject)objCharacter).combatBlock - (skillDiference * 0.04f),
            _ => 0f
        };
    }

    public float GetBasePercentMiss(ref WS_Base.BaseUnit objCharacter, int skillDiference)
    {
        if (objCharacter is WS_PlayerData.CharacterObject characterObject)
        {
            if (characterObject.attackSheathState == SHEATHE_SLOT.SHEATHE_WEAPON)
            {
                if (characterObject.Items.ContainsKey(16) && characterObject.Items[16].ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_WEAPON)
                {
                    return skillDiference > 10 ? 24f - (skillDiference * 0.1f) : 24f - (skillDiference * 0.2f);
                }
                return skillDiference > 10 ? 5f - (skillDiference * 0.1f) : 5f - (skillDiference * 0.2f);
            }
        }
        return 5f - (skillDiference * 0.04f);
    }

    public float GetBasePercentCrit(ref WS_Base.BaseUnit objCharacter, int skillDiference)
    {
        switch (objCharacter)
        {
            case WS_PlayerData.CharacterObject _:
                {
                    var baseCrit = 0f;
                    switch (((WS_PlayerData.CharacterObject)objCharacter).Classe)
                    {
                        case Classes.CLASS_ROGUE:
                            baseCrit = (float)(0.0 + (((WS_PlayerData.CharacterObject)objCharacter).Agility.Base / 29.0));
                            break;

                        case Classes.CLASS_DRUID:
                            baseCrit = (float)(0.92000001668930054 + (((WS_PlayerData.CharacterObject)objCharacter).Agility.Base / 20.0));
                            break;

                        case Classes.CLASS_HUNTER:
                            baseCrit = (float)(0.0 + (((WS_PlayerData.CharacterObject)objCharacter).Agility.Base / 33.0));
                            break;

                        case Classes.CLASS_MAGE:
                            baseCrit = (float)(3.2000000476837158 + (((WS_PlayerData.CharacterObject)objCharacter).Agility.Base / 19.44));
                            break;

                        case Classes.CLASS_PALADIN:
                            baseCrit = (float)(0.699999988079071 + (((WS_PlayerData.CharacterObject)objCharacter).Agility.Base / 19.77));
                            break;

                        case Classes.CLASS_PRIEST:
                            baseCrit = (float)(3.0 + (((WS_PlayerData.CharacterObject)objCharacter).Agility.Base / 20.0));
                            break;

                        case Classes.CLASS_SHAMAN:
                            baseCrit = (float)(1.7000000476837158 + (((WS_PlayerData.CharacterObject)objCharacter).Agility.Base / 19.7));
                            break;

                        case Classes.CLASS_WARLOCK:
                            baseCrit = (float)(2.0 + (((WS_PlayerData.CharacterObject)objCharacter).Agility.Base / 20.0));
                            break;

                        case Classes.CLASS_WARRIOR:
                            baseCrit = (float)(0.0 + (((WS_PlayerData.CharacterObject)objCharacter).Agility.Base / 20.0));
                            break;
                    }
                    return baseCrit + ((WS_PlayerData.CharacterObject)objCharacter).combatCrit + (skillDiference * 0.2f);
                }

            default:
                return 5f + (skillDiference * 0.2f);
        }
    }

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
        return (float)Math.Sqrt(((x1 - x2) * (x1 - x2)) + ((y1 - y2) * (y1 - y2)) + ((z1 - z2) * (z1 - z2)));
    }

    public float GetDistance(float x1, float x2, float y1, float y2)
    {
        return (float)Math.Sqrt(((x1 - x2) * (x1 - x2)) + ((y1 - y2) * (y1 - y2)));
    }

    public float GetOrientation(float x1, float x2, float y1, float y2)
    {
        var angle = (float)Math.Atan2(y2 - y1, x2 - x1);
        if (angle < 0f)
        {
            angle = (float)(angle + (Math.PI * 2.0));
        }
        return angle;
    }

    public bool IsInFrontOf(ref WS_Base.BaseObject Object1, ref WS_Base.BaseObject Object2)
    {
        return IsInFrontOf(ref Object1, Object2.positionX, Object2.positionY);
    }

    public bool IsInFrontOf(ref WS_Base.BaseObject Object1, float x2, float y2)
    {
        var angle2 = GetOrientation(Object1.positionX, x2, Object1.positionY, y2);
        var lowAngle = Object1.orientation - ((float)Math.PI / 3f);
        var hiAngle = Object1.orientation + ((float)Math.PI / 3f);
        return lowAngle < 0f
            ? (angle2 >= (Math.PI * 2.0) + lowAngle && angle2 <= Math.PI * 2.0) || (angle2 >= 0f && angle2 <= hiAngle)
            : angle2 >= lowAngle && angle2 <= hiAngle;
    }

    public bool IsInBackOf(ref WS_Base.BaseObject Object1, ref WS_Base.BaseObject Object2)
    {
        return IsInBackOf(ref Object1, Object2.positionX, Object2.positionY);
    }

    public bool IsInBackOf(ref WS_Base.BaseObject Object1, float x2, float y2)
    {
        var angle2 = GetOrientation(x2, Object1.positionX, y2, Object1.positionY);
        var lowAngle = Object1.orientation - ((float)Math.PI / 3f);
        var hiAngle = Object1.orientation + ((float)Math.PI / 3f);
        return lowAngle < 0f
            ? (angle2 >= (Math.PI * 2.0) + lowAngle && angle2 <= Math.PI * 2.0) || (angle2 >= 0f && angle2 <= hiAngle)
            : angle2 >= lowAngle && angle2 <= hiAngle;
    }

    public int GetSkillWeapon(ref WS_Base.BaseUnit objCharacter, bool DualWield)
    {
        checked
        {
            if (objCharacter is WS_PlayerData.CharacterObject characterObject)
            {
                int tmpSkill = default;
                switch (characterObject.attackSheathState)
                {
                    case SHEATHE_SLOT.SHEATHE_NONE:
                        tmpSkill = 162;
                        break;

                    case SHEATHE_SLOT.SHEATHE_WEAPON:
                        if (DualWield && characterObject.Items.ContainsKey(16))
                        {
                            tmpSkill = WorldServiceLocator._WorldServer.ITEMDatabase[characterObject.Items[16].ItemEntry].GetReqSkill;
                        }
                        else if (characterObject.Items.ContainsKey(15))
                        {
                            tmpSkill = WorldServiceLocator._WorldServer.ITEMDatabase[characterObject.Items[15].ItemEntry].GetReqSkill;
                        }
                        break;

                    case SHEATHE_SLOT.SHEATHE_RANGED:
                        if (characterObject.Items.ContainsKey(17))
                        {
                            tmpSkill = WorldServiceLocator._WorldServer.ITEMDatabase[characterObject.Items[17].ItemEntry].GetReqSkill;
                        }
                        break;
                }
                if (tmpSkill == 0)
                {
                    return objCharacter.Level * 5;
                }
                characterObject.UpdateSkill(tmpSkill, 0.01f);
                return characterObject.Skills[tmpSkill].CurrentWithBonus;
            }
            return objCharacter.Level * 5;
        }
    }

    public int GetSkillDefence(ref WS_Base.BaseUnit objCharacter)
    {
        if (objCharacter is WS_PlayerData.CharacterObject @object)
        {
            @object.UpdateSkill(95, 0.01f);
            return @object.Skills[95].CurrentWithBonus;
        }
        checked
        {
            return objCharacter.Level * 5;
        }
    }

    public int GetAttackTime(ref WS_PlayerData.CharacterObject objCharacter, ref bool combatDualWield)
    {
        switch (objCharacter.attackSheathState)
        {
            case SHEATHE_SLOT.SHEATHE_NONE:
                return objCharacter.GetAttackTime(WeaponAttackType.BASE_ATTACK);

            case SHEATHE_SLOT.SHEATHE_WEAPON:
                if (combatDualWield)
                {
                    return objCharacter.GetAttackTime(WeaponAttackType.OFF_ATTACK) == 0
                        ? objCharacter.GetAttackTime(WeaponAttackType.BASE_ATTACK)
                        : objCharacter.GetAttackTime(WeaponAttackType.OFF_ATTACK);
                }
                if (objCharacter.GetAttackTime(WeaponAttackType.BASE_ATTACK) == 0)
                {
                    return objCharacter.GetAttackTime(WeaponAttackType.OFF_ATTACK);
                }
                return objCharacter.GetAttackTime(WeaponAttackType.BASE_ATTACK);

            case SHEATHE_SLOT.SHEATHE_RANGED:
                return objCharacter.GetAttackTime(WeaponAttackType.RANGED_ATTACK);

            default:
                {
                    int GetAttackTime = default;
                    return GetAttackTime;
                }
        }
    }

    public void GetDamage(ref WS_Base.BaseUnit objCharacter, bool DualWield, ref DamageInfo result)
    {
        checked
        {
            if (objCharacter is WS_PlayerData.CharacterObject characterObject)
            {
                switch (characterObject.attackSheathState)
                {
                    case SHEATHE_SLOT.SHEATHE_NONE:
                        result.HitInfo = 0;
                        result.DamageType = DamageTypes.DMG_PHYSICAL;
                        result.Damage = WorldServiceLocator._WorldServer.Rnd.Next(characterObject.BaseUnarmedDamage, characterObject.BaseUnarmedDamage + 1);
                        break;

                    case SHEATHE_SLOT.SHEATHE_WEAPON:
                        if (DualWield)
                        {
                            result.HitInfo = 6;
                            result.DamageType = DamageTypes.DMG_PHYSICAL;
                            result.Damage = WorldServiceLocator._WorldServer.Rnd.Next((int)Math.Round(characterObject.OffHandDamage.Minimum / 2f), (int)Math.Round((characterObject.OffHandDamage.Maximum / 2f) + 1f)) + characterObject.BaseUnarmedDamage;
                        }
                        else
                        {
                            result.HitInfo = 2;
                            result.DamageType = DamageTypes.DMG_PHYSICAL;
                            result.Damage = WorldServiceLocator._WorldServer.Rnd.Next((int)Math.Round(characterObject.Damage.Minimum), (int)Math.Round(characterObject.Damage.Maximum + 1f)) + characterObject.BaseUnarmedDamage;
                        }
                        break;

                    case SHEATHE_SLOT.SHEATHE_RANGED:
                        result.HitInfo = 10;
                        result.DamageType = DamageTypes.DMG_PHYSICAL;
                        result.Damage = WorldServiceLocator._WorldServer.Rnd.Next((int)Math.Round(characterObject.RangedDamage.Minimum), (int)Math.Round(characterObject.RangedDamage.Maximum + 1f)) + characterObject.BaseRangedDamage;
                        break;
                }
            }
            else
            {
                WS_Creatures.CreatureObject creatureObject = (WS_Creatures.CreatureObject)objCharacter;
                result.DamageType = DamageTypes.DMG_PHYSICAL;
                result.Damage = WorldServiceLocator._WorldServer.Rnd.Next((int)Math.Round(WorldServiceLocator._WorldServer.CREATURESDatabase[creatureObject.ID].Damage.Minimum), (int)Math.Round(WorldServiceLocator._WorldServer.CREATURESDatabase[creatureObject.ID].Damage.Maximum + 1f));
            }
        }
    }

    public void SetPlayerInCombat(ref WS_PlayerData.CharacterObject objCharacter)
    {
        objCharacter.cUnitFlags |= 0x80000;
        objCharacter.SetUpdateFlag(46, objCharacter.cUnitFlags);
        objCharacter.SendCharacterUpdate(toNear: false);
        objCharacter.RemoveAurasByInterruptFlag(32);
    }

    public void SetPlayerOutOfCombat(ref WS_PlayerData.CharacterObject objCharacter)
    {
        objCharacter.cUnitFlags &= -524289;
        objCharacter.SetUpdateFlag(46, objCharacter.cUnitFlags);
        objCharacter.SendCharacterUpdate(toNear: false);
    }

    public void On_CMSG_SET_SELECTION(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 13)
        {
            if (client.Character != null)
            {
                packet.GetInt16();
                client.Character.TargetGUID = packet.GetUInt64();
                client.Character.SetUpdateFlag(16, client.Character.TargetGUID);
                client.Character.SendCharacterUpdate();
            }
        }
    }

    public void On_CMSG_ATTACKSWING(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 13 && client.Character != null)
        {
            packet.GetInt16();
            var GUID = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_ATTACKSWING [GUID={2:X}]", client.IP, client.Port, GUID);
            if (client.Character.Spell_Pacifyed)
            {
                Packets.PacketClass SMSG_ATTACKSWING_CANT_ATTACK2 = new(Opcodes.SMSG_ATTACKSWING_CANT_ATTACK);
                client.Send(ref SMSG_ATTACKSWING_CANT_ATTACK2);
                SMSG_ATTACKSWING_CANT_ATTACK2.Dispose();
                SendAttackStop(client.Character.GUID, GUID, ref client);
            }
            else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(GUID))
            {
                client.Character.attackState.AttackStart(WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID]);
            }
            else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(GUID))
            {
                client.Character.attackState.AttackStart(WorldServiceLocator._WorldServer.CHARACTERs[GUID]);
            }
            else
            {
                Packets.PacketClass SMSG_ATTACKSWING_CANT_ATTACK = new(Opcodes.SMSG_ATTACKSWING_CANT_ATTACK);
                client.Send(ref SMSG_ATTACKSWING_CANT_ATTACK);
                SMSG_ATTACKSWING_CANT_ATTACK.Dispose();
                SendAttackStop(client.Character.GUID, GUID, ref client);
            }
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
        catch (Exception ex)
        {
            ProjectData.SetProjectError(ex);
            var e = ex;
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error stopping attack: {0}", e.ToString());
            ProjectData.ClearProjectError();
        }
    }

    public void On_CMSG_SET_AMMO(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) < 9)
        {
            return;
        }
        packet.GetInt16();
        var AmmoID = packet.GetInt32();
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SET_AMMO [{2}]", client.IP, client.Port, AmmoID);
        if (client.Character.IsDead)
        {
            WorldServiceLocator._WS_Items.SendInventoryChangeFailure(ref client.Character, InventoryChangeFailure.EQUIP_ERR_YOU_ARE_DEAD, 0uL, 0uL);
        }
        else if (AmmoID != 0)
        {
            client.Character.AmmoID = AmmoID;
            if (!WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(AmmoID))
            {
                WS_Items.ItemInfo tmpItem = new(AmmoID);
            }
            var CanUse = WorldServiceLocator._CharManagementHandler.CanUseAmmo(ref client.Character, AmmoID);
            if (CanUse != 0)
            {
                WorldServiceLocator._WS_Items.SendInventoryChangeFailure(ref client.Character, CanUse, 0uL, 0uL);
                return;
            }
            var currentDPS = 0f;
            if ((WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(AmmoID) && WorldServiceLocator._WorldServer.ITEMDatabase[AmmoID].ObjectClass == ITEM_CLASS.ITEM_CLASS_PROJECTILE) || WorldServiceLocator._CharManagementHandler.CheckAmmoCompatibility(ref client.Character, AmmoID))
            {
                currentDPS = WorldServiceLocator._WorldServer.ITEMDatabase[AmmoID].Damage[0].Minimum;
            }
            if (client.Character.AmmoDPS != currentDPS)
            {
                client.Character.AmmoDPS = currentDPS;
                CalculateMinMaxDamage(ref client.Character, WeaponAttackType.RANGED_ATTACK);
            }
            client.Character.AmmoID = AmmoID;
            client.Character.SetUpdateFlag(1223, client.Character.AmmoID);
            client.Character.SendCharacterUpdate(toNear: false);
        }
        else if (client.Character.AmmoID != 0)
        {
            client.Character.AmmoDPS = 0f;
            CalculateMinMaxDamage(ref client.Character, WeaponAttackType.RANGED_ATTACK);
            client.Character.AmmoID = 0;
            client.Character.SetUpdateFlag(1223, 0);
            client.Character.SendCharacterUpdate(toNear: false);
        }
    }

    public void On_CMSG_SETSHEATHED(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 9)
        {
            packet.GetInt16();
            SHEATHE_SLOT sheathed = (SHEATHE_SLOT)checked((byte)packet.GetInt32());
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SETSHEATHED [{2}]", client.IP, client.Port, sheathed);
            SetSheath(ref client.Character, sheathed);
        }
    }

    public void SetSheath(ref WS_PlayerData.CharacterObject objCharacter, SHEATHE_SLOT State)
    {
        objCharacter.attackSheathState = State;
        objCharacter.combatCanDualWield = false;
        objCharacter.cBytes2 = (objCharacter.cBytes2 & -256) | (int)State;
        objCharacter.SetUpdateFlag(164, objCharacter.cBytes2);
        if (objCharacter is null)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "[{0}:{1} Account:{2} CharName:{3} CharGUID:{4}] Client is Null!", objCharacter.client.IP, objCharacter.client.Port, objCharacter.client.Account, objCharacter.client.Character.UnitName, objCharacter.client.Character.GUID);
            return;
        }
        switch (State)
        {
            case SHEATHE_SLOT.SHEATHE_NONE:
                {
                    var objChar13 = objCharacter;
                    ItemObject Item = null;
                    SetVirtualItemInfo(objChar13, 0, ref Item);
                    var objChar14 = objCharacter;
                    Item = null;
                    SetVirtualItemInfo(objChar14, 1, ref Item);
                    var objChar15 = objCharacter;
                    Item = null;
                    SetVirtualItemInfo(objChar15, 2, ref Item);
                    break;
                }
            case SHEATHE_SLOT.SHEATHE_WEAPON:
                {
                    ItemObject Item;
                    if (objCharacter.Items.ContainsKey(15) && !objCharacter.Items[15].IsBroken())
                    {
                        var objChar8 = objCharacter;
                        Dictionary<byte, ItemObject> items;
                        Item = (items = objCharacter.Items)[15];
                        SetVirtualItemInfo(objChar8, 0, ref Item);
                        items[15] = Item;
                    }
                    else
                    {
                        var objChar9 = objCharacter;
                        Item = null;
                        SetVirtualItemInfo(objChar9, 0, ref Item);
                        objCharacter.attackSheathState = SHEATHE_SLOT.SHEATHE_NONE;
                    }
                    if (objCharacter.Items.ContainsKey(16) && !objCharacter.Items[16].IsBroken())
                    {
                        var objChar10 = objCharacter;
                        Dictionary<byte, ItemObject> items;
                        Item = (items = objCharacter.Items)[16];
                        SetVirtualItemInfo(objChar10, 1, ref Item);
                        items[16] = Item;
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "spellCanDualWeild = {0}", objCharacter.spellCanDualWeild);
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "objectClass = {0}", objCharacter.Items[16].ItemInfo.ObjectClass);
                        if (objCharacter.spellCanDualWeild && objCharacter.Items[16].ItemInfo.ObjectClass == ITEM_CLASS.ITEM_CLASS_WEAPON)
                        {
                            objCharacter.combatCanDualWield = true;
                        }
                    }
                    else
                    {
                        var objChar11 = objCharacter;
                        Item = null;
                        SetVirtualItemInfo(objChar11, 1, ref Item);
                    }
                    var objChar12 = objCharacter;
                    Item = null;
                    SetVirtualItemInfo(objChar12, 2, ref Item);
                    break;
                }
            case SHEATHE_SLOT.SHEATHE_RANGED:
                {
                    var objChar4 = objCharacter;
                    ItemObject Item = null;
                    SetVirtualItemInfo(objChar4, 0, ref Item);
                    var objChar5 = objCharacter;
                    Item = null;
                    SetVirtualItemInfo(objChar5, 1, ref Item);
                    if (objCharacter.Items.ContainsKey(17) && !objCharacter.Items[17].IsBroken())
                    {
                        var objChar6 = objCharacter;
                        Dictionary<byte, ItemObject> items;
                        Item = (items = objCharacter.Items)[17];
                        SetVirtualItemInfo(objChar6, 2, ref Item);
                        items[17] = Item;
                    }
                    else
                    {
                        var objChar7 = objCharacter;
                        Item = null;
                        SetVirtualItemInfo(objChar7, 2, ref Item);
                        objCharacter.attackSheathState = SHEATHE_SLOT.SHEATHE_NONE;
                    }
                    break;
                }
            default:
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Unhandled sheathe state [{0}]", State);
                    var objChar = objCharacter;
                    ItemObject Item = null;
                    SetVirtualItemInfo(objChar, 0, ref Item);
                    var objChar2 = objCharacter;
                    Item = null;
                    SetVirtualItemInfo(objChar2, 1, ref Item);
                    var objChar3 = objCharacter;
                    Item = null;
                    SetVirtualItemInfo(objChar3, 2, ref Item);
                    break;
                }
        }
        objCharacter.SendCharacterUpdate();
    }

    public void SetVirtualItemInfo(WS_PlayerData.CharacterObject objChar, byte Slot, ref ItemObject Item)
    {
        if (Slot <= 2 && Item != null)
        {
        }
    }

    public void SendAttackStop(ulong attackerGUID, ulong victimGUID, ref WS_Network.ClientClass client)
    {
        Packets.PacketClass SMSG_ATTACKSTOP = new(Opcodes.SMSG_ATTACKSTOP);
        SMSG_ATTACKSTOP.AddPackGUID(attackerGUID);
        SMSG_ATTACKSTOP.AddPackGUID(victimGUID);
        SMSG_ATTACKSTOP.AddInt32(0);
        SMSG_ATTACKSTOP.AddInt8(0);
        client.Character.SendToNearPlayers(ref SMSG_ATTACKSTOP);
        SMSG_ATTACKSTOP.Dispose();
    }

    public void SendAttackStart(ulong attackerGUID, ulong victimGUID, WS_Network.ClientClass client)
    {
        Packets.PacketClass SMSG_ATTACKSTART = new(Opcodes.SMSG_ATTACKSTART);
        SMSG_ATTACKSTART.AddUInt64(attackerGUID);
        SMSG_ATTACKSTART.AddUInt64(victimGUID);
        client.Character.SendToNearPlayers(ref SMSG_ATTACKSTART);
        SMSG_ATTACKSTART.Dispose();
    }

    public void SendAttackerStateUpdate(ref WS_Base.BaseObject Attacker, ref WS_Base.BaseObject Victim, DamageInfo damageInfo, WS_Network.ClientClass client = null)
    {
        Packets.PacketClass packet = new(Opcodes.SMSG_ATTACKERSTATEUPDATE);
        packet.AddInt32(damageInfo.HitInfo);
        packet.AddPackGUID(Attacker.GUID);
        packet.AddPackGUID(Victim.GUID);
        packet.AddInt32(damageInfo.GetDamage);
        packet.AddInt8(1);
        packet.AddUInt32((uint)damageInfo.DamageType);
        packet.AddSingle(damageInfo.GetDamage);
        packet.AddInt32(damageInfo.GetDamage);
        packet.AddInt32(damageInfo.Absorbed);
        packet.AddInt32(damageInfo.Resist);
        packet.AddInt32((int)damageInfo.victimState);
        if (damageInfo.Absorbed == 0)
        {
            packet.AddInt32(1000);
        }
        else
        {
            packet.AddInt32(-1);
        }
        packet.AddInt32(0);
        packet.AddInt32(damageInfo.Blocked);
        if (client != null)
        {
            client.Character.SendToNearPlayers(ref packet);
        }
        else
        {
            Attacker.SendToNearPlayers(ref packet);
        }
        packet.Dispose();
    }
}
