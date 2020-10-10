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
using System.Threading;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Player;
using Mangos.Common.Enums.Spell;
using Mangos.Common.Enums.Unit;
using Mangos.Common.Globals;
using Functions = Mangos.World.Globals.Functions;
using Mangos.World.Objects;
using Mangos.World.Player;
using Mangos.World.Weather;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Server
{
    public class WS_TimerBasedEvents
    {
        public TRegenerator Regenerator;
        public TAIManager AIManager;
        public TSpellManager SpellManager;
        public TCharacterSaver CharacterSaver;
        public TWeatherChanger WeatherChanger;

        // NOTE: Regenerates players' Mana, Life and Rage
        public class TRegenerator : IDisposable
        {
            private Timer RegenerationTimer = null;
            private bool RegenerationWorking = false;
            private readonly int operationsCount;
            private int BaseMana;
            private int BaseLife;
            private int BaseRage;
            private int BaseEnergy;
            private bool _updateFlag;
            private bool NextGroupUpdate = true;
            public const int REGENERATION_TIMER = 2;          // Timer period (sec)
            public const int REGENERATION_ENERGY = 20;        // Base energy regeneration rate
            public const int REGENERATION_RAGE = 25;          // Base rage degeneration rate (Rage = 1000 but shows only as 100 in game)

            public TRegenerator()
            {
                RegenerationTimer = new Timer(Regenerate, null, 10000, REGENERATION_TIMER * 1000);
            }

            private void Regenerate(object state)
            {
                if (RegenerationWorking)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Update: Regenerator skipping update");
                    return;
                }

                RegenerationWorking = true;
                NextGroupUpdate = !NextGroupUpdate; // Group update = every 4 sec
                try
                {
                    WorldServiceLocator._WorldServer.CHARACTERs_Lock.AcquireReaderLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                    foreach (KeyValuePair<ulong, WS_PlayerData.CharacterObject> Character in WorldServiceLocator._WorldServer.CHARACTERs)
                    {
                        // DONE: If all invalid check passed then regenerate
                        // DONE: If dead don't regenerate
                        if (!Character.Value.DEAD && Character.Value.underWaterTimer is null && Character.Value.LogoutTimer is null && Character.Value.client is object)
                        {
                            {
                                var withBlock = Character.Value;
                                BaseMana = withBlock.Mana.Current;
                                BaseRage = withBlock.Rage.Current;
                                BaseEnergy = withBlock.Energy.Current;
                                BaseLife = withBlock.Life.Current;
                                _updateFlag = false;

                                // Rage
                                // DONE: In combat do not decrease, but send updates
                                if (withBlock.ManaType == ManaTypes.TYPE_RAGE)
                                {
                                    if ((withBlock.cUnitFlags & UnitFlags.UNIT_FLAG_IN_COMBAT) == 0)
                                    {
                                        if (withBlock.Rage.Current > 0)
                                        {
                                            withBlock.Rage.Current -= REGENERATION_RAGE;
                                        }
                                    }
                                    else if (withBlock.RageRegenBonus != 0) // In Combat Regen from spells
                                    {
                                        withBlock.Rage.Increment(withBlock.RageRegenBonus);
                                    }
                                }

                                // Energy
                                if (withBlock.ManaType == ManaTypes.TYPE_ENERGY && withBlock.Energy.Current < withBlock.Energy.Maximum)
                                {
                                    withBlock.Energy.Increment(REGENERATION_ENERGY);
                                }

                                // Mana
                                if (withBlock.ManaRegen == 0)
                                    withBlock.UpdateManaRegen();
                                // DONE: Don't regenerate while casting, 5 second rule
                                // TODO: If objCharacter.ManaRegenerationWhileCastingPercent > 0 ...
                                if (withBlock.spellCastManaRegeneration == 0)
                                {
                                    if ((withBlock.ManaType == ManaTypes.TYPE_MANA || withBlock.Classe == Classes.CLASS_DRUID) && withBlock.Mana.Current < withBlock.Mana.Maximum)
                                    {
                                        withBlock.Mana.Increment(withBlock.ManaRegen * REGENERATION_TIMER);
                                    }
                                }
                                else
                                {
                                    if ((withBlock.ManaType == ManaTypes.TYPE_MANA || withBlock.Classe == Classes.CLASS_DRUID) && withBlock.Mana.Current < withBlock.Mana.Maximum)
                                    {
                                        withBlock.Mana.Increment(withBlock.ManaRegenInterrupt * REGENERATION_TIMER);
                                    }

                                    if (withBlock.spellCastManaRegeneration < REGENERATION_TIMER)
                                    {
                                        withBlock.spellCastManaRegeneration = 0;
                                    }
                                    else
                                    {
                                        withBlock.spellCastManaRegeneration = (byte)(withBlock.spellCastManaRegeneration - REGENERATION_TIMER);
                                    }
                                }

                                // Life
                                // DONE: Don't regenerate in combat
                                // TODO: If objCharacter.LifeRegenWhileFightingPercent > 0 ...
                                if (withBlock.Life.Current < withBlock.Life.Maximum && (withBlock.cUnitFlags & UnitFlags.UNIT_FLAG_IN_COMBAT) == 0)
                                {
                                    switch (withBlock.Classe)
                                    {
                                        case var @case when @case == Classes.CLASS_MAGE:
                                            {
                                                withBlock.Life.Increment(Conversions.ToInteger(withBlock.Spirit.Base * 0.1d * withBlock.LifeRegenerationModifier) + withBlock.LifeRegenBonus);
                                                break;
                                            }

                                        case var case1 when case1 == Classes.CLASS_PRIEST:
                                            {
                                                withBlock.Life.Increment(Conversions.ToInteger(withBlock.Spirit.Base * 0.1d * withBlock.LifeRegenerationModifier) + withBlock.LifeRegenBonus);
                                                break;
                                            }

                                        case var case2 when case2 == Classes.CLASS_WARLOCK:
                                            {
                                                withBlock.Life.Increment(Conversions.ToInteger(withBlock.Spirit.Base * 0.11d * withBlock.LifeRegenerationModifier) + withBlock.LifeRegenBonus);
                                                break;
                                            }

                                        case var case3 when case3 == Classes.CLASS_DRUID:
                                            {
                                                withBlock.Life.Increment(Conversions.ToInteger(withBlock.Spirit.Base * 0.11d * withBlock.LifeRegenerationModifier) + withBlock.LifeRegenBonus);
                                                break;
                                            }

                                        case var case4 when case4 == Classes.CLASS_SHAMAN:
                                            {
                                                withBlock.Life.Increment(Conversions.ToInteger(withBlock.Spirit.Base * 0.11d * withBlock.LifeRegenerationModifier) + withBlock.LifeRegenBonus);
                                                break;
                                            }

                                        case var case5 when case5 == Classes.CLASS_ROGUE:
                                            {
                                                withBlock.Life.Increment(Conversions.ToInteger(withBlock.Spirit.Base * 0.5d * withBlock.LifeRegenerationModifier) + withBlock.LifeRegenBonus);
                                                break;
                                            }

                                        case var case6 when case6 == Classes.CLASS_WARRIOR:
                                            {
                                                withBlock.Life.Increment(Conversions.ToInteger(withBlock.Spirit.Base * 0.8d * withBlock.LifeRegenerationModifier) + withBlock.LifeRegenBonus);
                                                break;
                                            }

                                        case var case7 when case7 == Classes.CLASS_HUNTER:
                                            {
                                                withBlock.Life.Increment(Conversions.ToInteger(withBlock.Spirit.Base * 0.25d * withBlock.LifeRegenerationModifier) + withBlock.LifeRegenBonus);
                                                break;
                                            }

                                        case var case8 when case8 == Classes.CLASS_PALADIN:
                                            {
                                                withBlock.Life.Increment(Conversions.ToInteger(withBlock.Spirit.Base * 0.25d * withBlock.LifeRegenerationModifier) + withBlock.LifeRegenBonus);
                                                break;
                                            }
                                    }
                                }

                                // DONE: Send updates to players near
                                if (BaseMana != withBlock.Mana.Current)
                                {
                                    _updateFlag = true;
                                    withBlock.GroupUpdateFlag |= (uint)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_CUR_POWER;
                                    withBlock.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_POWER1, withBlock.Mana.Current);
                                }

                                if (BaseRage != withBlock.Rage.Current | (withBlock.cUnitFlags & UnitFlags.UNIT_FLAG_IN_COMBAT) == UnitFlags.UNIT_FLAG_IN_COMBAT)
                                {
                                    _updateFlag = true;
                                    withBlock.GroupUpdateFlag |= (uint)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_CUR_POWER;
                                    withBlock.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_POWER2, withBlock.Rage.Current);
                                }

                                if (BaseEnergy != withBlock.Energy.Current)
                                {
                                    _updateFlag = true;
                                    withBlock.GroupUpdateFlag |= (uint)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_CUR_POWER;
                                    withBlock.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_POWER4, withBlock.Energy.Current);
                                }

                                if (BaseLife != withBlock.Life.Current)
                                {
                                    _updateFlag = true;
                                    withBlock.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_HEALTH, withBlock.Life.Current);
                                    withBlock.GroupUpdateFlag |= (uint)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_CUR_HP;
                                }

                                if (_updateFlag)
                                    withBlock.SendCharacterUpdate();

                                // DONE: Duel counter
                                if (withBlock.DuelOutOfBounds != Spells.WS_Spells.DUEL_COUNTER_DISABLED)
                                {
                                    withBlock.DuelOutOfBounds = (byte)(withBlock.DuelOutOfBounds - REGENERATION_TIMER);
                                    if (withBlock.DuelOutOfBounds == 0)
                                        WorldServiceLocator._WS_Spells.DuelComplete(ref withBlock.DuelPartner, ref withBlock.client.Character);
                                }

                                // Check combat, incase of pvp action
                                withBlock.CheckCombat();

                                // Send group update
                                if (NextGroupUpdate)
                                    withBlock.GroupUpdate();

                                // Send UPDATE_OUT_OF_RANGE
                                if (withBlock.guidsForRemoving.Count > 0)
                                    withBlock.SendOutOfRangeUpdate();
                            }
                        }
                    }

                    if (WorldServiceLocator._WorldServer.CHARACTERs_Lock.IsReaderLockHeld == true)
                        WorldServiceLocator._WorldServer.CHARACTERs_Lock.ReleaseReaderLock();
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Error at regenerate.{0}", Environment.NewLine + ex.ToString());
                }

                RegenerationWorking = false;
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
                    RegenerationTimer.Dispose();
                    RegenerationTimer = null;
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

        // NOTE: Manages spell durations and DOT spells
        public class TSpellManager : IDisposable
        {
            private Timer SpellManagerTimer = null;
            private bool SpellManagerWorking = false;
            public const int UPDATE_TIMER = 1000;        // Timer period (ms)

            public TSpellManager()
            {
                SpellManagerTimer = new Timer(Update, null, 10000, UPDATE_TIMER);
            }

            private void Update(object state)
            {
                if (SpellManagerWorking)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Update: Spell Manager skipping update");
                    return;
                }

                SpellManagerWorking = true;
                try
                {
                    WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.AcquireReaderLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                    for (long i = 0L, loopTo = WorldServiceLocator._WorldServer.WORLD_CREATUREsKeys.Count - 1; i <= loopTo; i++)
                    {
                        if (WorldServiceLocator._WorldServer.WORLD_CREATUREs[Conversions.ToULong(WorldServiceLocator._WorldServer.WORLD_CREATUREsKeys[(int)i])] is object)
                        {
                            var tmp = WorldServiceLocator._WorldServer.WORLD_CREATUREs;
                            WS_Base.BaseUnit argobjCharacter = tmp[Conversions.ToULong(WorldServiceLocator._WorldServer.WORLD_CREATUREsKeys[(int)i])];
                            UpdateSpells(ref argobjCharacter);
                            tmp[Conversions.ToULong(WorldServiceLocator._WorldServer.WORLD_CREATUREsKeys[(int)i])] = (WS_Creatures.CreatureObject)argobjCharacter;
                        }
                    }
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, ex.ToString(), default);
                }
                finally
                {
                    if (WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.IsReaderLockHeld == true)
                    {
                        WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.ReleaseReaderLock();
                    }
                }

                try
                {
                    WorldServiceLocator._WorldServer.CHARACTERs_Lock.AcquireReaderLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                    foreach (KeyValuePair<ulong, WS_PlayerData.CharacterObject> Character in WorldServiceLocator._WorldServer.CHARACTERs)
                    {
                        if (Character.Value is object)
                        {
                            WS_Base.BaseUnit argobjCharacter1 = Character.Value;
                            UpdateSpells(ref argobjCharacter1);
                            Character.Value = (WS_PlayerData.CharacterObject)argobjCharacter1;
                        }
                    }
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, ex.ToString(), default);
                }
                finally
                {
                    WorldServiceLocator._WorldServer.CHARACTERs_Lock.ReleaseLock();
                }

                var DynamicObjectsToDelete = new List<WS_DynamicObjects.DynamicObjectObject>();
                try
                {
                    WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs_Lock.AcquireReaderLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                    foreach (KeyValuePair<ulong, WS_DynamicObjects.DynamicObjectObject> Dynamic in WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs)
                    {
                        if (Dynamic.Value is object && Dynamic.Value.Update())
                        {
                            DynamicObjectsToDelete.Add(Dynamic.Value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, ex.ToString(), default);
                }
                finally
                {
                    WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs_Lock.ReleaseReaderLock();
                }

                foreach (WS_DynamicObjects.DynamicObjectObject Dynamic in DynamicObjectsToDelete)
                {
                    if (Dynamic is object)
                        Dynamic.Delete();
                }

                SpellManagerWorking = false;
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
                    SpellManagerTimer.Dispose();
                    SpellManagerTimer = null;
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
            private void UpdateSpells(ref WS_Base.BaseUnit objCharacter)
            {
                if (objCharacter is WS_Totems.TotemObject)
                {
                    ((WS_Totems.TotemObject)objCharacter).Update();
                }
                else
                {
                    for (int i = 0, loopTo = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs - 1; i <= loopTo; i++)
                    {
                        if (objCharacter.ActiveSpells[i] is object)
                        {

                            // DONE: Count aura duration
                            if (objCharacter.ActiveSpells[i].SpellDuration != WorldServiceLocator._Global_Constants.SPELL_DURATION_INFINITE)
                            {
                                objCharacter.ActiveSpells[i].SpellDuration -= UPDATE_TIMER;

                                // DONE: Cast aura (check if: there is aura; aura is periodic; time for next activation)
                                for (byte j = 0; j <= 2; j++)
                                {
                                    if (objCharacter.ActiveSpells[i] is object && objCharacter.ActiveSpells[i].Aura[j] is object && objCharacter.ActiveSpells[i].Aura_Info[j] is object && objCharacter.ActiveSpells[i].Aura_Info[j].Amplitude != 0 && (objCharacter.ActiveSpells[i].GetSpellInfo.GetDuration - objCharacter.ActiveSpells[i].SpellDuration) % objCharacter.ActiveSpells[i].Aura_Info[j].Amplitude == 0)
                                    {
                                        objCharacter.ActiveSpells[i].Aura[(int)j].Invoke(ref objCharacter, ref (WS_Base.BaseObject)objCharacter.ActiveSpells[i].SpellCaster, ref objCharacter.ActiveSpells[i].Aura_Info[(int)j], objCharacter.ActiveSpells[i].SpellID, objCharacter.ActiveSpells[i].StackCount + 1, AuraAction.AURA_UPDATE);
                                    }
                                }

                                // DONE: Remove finished aura
                                if (objCharacter.ActiveSpells[i] is object && objCharacter.ActiveSpells[i].SpellDuration <= 0 && objCharacter.ActiveSpells[i].SpellDuration != WorldServiceLocator._Global_Constants.SPELL_DURATION_INFINITE)
                                    objCharacter.RemoveAura(i, ref objCharacter.ActiveSpells[i].SpellCaster, true);
                            }

                            // DONE: Check if there are units that are out of range for the area aura
                            for (byte j = 0; j <= 2; j++)
                            {
                                if (objCharacter.ActiveSpells[i] is object && objCharacter.ActiveSpells[i].Aura_Info[j] is object)
                                {
                                    if (objCharacter.ActiveSpells[i].Aura_Info[j].ID == SpellEffects_Names.SPELL_EFFECT_APPLY_AREA_AURA)
                                    {
                                        if (ReferenceEquals(objCharacter.ActiveSpells[i].SpellCaster, objCharacter))
                                        {
                                            // DONE: Check if there are friendly targets around you that does not have your aura
                                            var Targets = new List<WS_Base.BaseUnit>();
                                            if (objCharacter is WS_PlayerData.CharacterObject)
                                            {
                                                List<WS_Base.BaseUnit> localGetPartyMembersAroundMe() { WS_PlayerData.CharacterObject argobjCharacter = (WS_PlayerData.CharacterObject)objCharacter; var ret = WorldServiceLocator._WS_Spells.GetPartyMembersAroundMe(ref argobjCharacter, objCharacter.ActiveSpells[i].Aura_Info[j].GetRadius); return ret; }

                                                Targets = localGetPartyMembersAroundMe();
                                            }
                                            else if (objCharacter is WS_Totems.TotemObject && ((WS_Totems.TotemObject)objCharacter).Caster is object && ((WS_Totems.TotemObject)objCharacter).Caster is WS_PlayerData.CharacterObject)
                                            {
                                                List<WS_Base.BaseUnit> localGetPartyMembersAtPoint() { WS_PlayerData.CharacterObject argobjCharacter = (WS_PlayerData.CharacterObject)((WS_Totems.TotemObject)objCharacter).Caster; var ret = WorldServiceLocator._WS_Spells.GetPartyMembersAtPoint(ref argobjCharacter, objCharacter.ActiveSpells[i].Aura_Info[j].GetRadius, objCharacter.positionX, objCharacter.positionY, objCharacter.positionZ); return ret; }

                                                Targets = localGetPartyMembersAtPoint();
                                            }

                                            foreach (WS_Base.BaseUnit Unit in Targets)
                                            {
                                                if (Unit.HaveAura(objCharacter.ActiveSpells[i].SpellID) == false)
                                                {
                                                    WS_Base.BaseObject argCaster = objCharacter;
                                                    WorldServiceLocator._WS_Spells.ApplyAura(ref Unit, ref argCaster, ref objCharacter.ActiveSpells[i].Aura_Info[j], objCharacter.ActiveSpells[i].SpellID);
                                                }
                                            }
                                        }
                                        // DONE: Check if your aura source is too far away, has removed the aura or you / the source left the group
                                        else if (objCharacter.ActiveSpells[i].SpellCaster is object && objCharacter.ActiveSpells[i].SpellCaster.Exist)
                                        {
                                            WS_PlayerData.CharacterObject caster = null;
                                            if (objCharacter.ActiveSpells[i].SpellCaster is WS_PlayerData.CharacterObject)
                                            {
                                                caster = (WS_PlayerData.CharacterObject)objCharacter.ActiveSpells[i].SpellCaster;
                                            }
                                            else if (objCharacter.ActiveSpells[i].SpellCaster is WS_Totems.TotemObject && ((WS_Totems.TotemObject)objCharacter.ActiveSpells[i].SpellCaster).Caster is object && ((WS_Totems.TotemObject)objCharacter.ActiveSpells[i].SpellCaster).Caster is WS_PlayerData.CharacterObject)
                                            {
                                                caster = (WS_PlayerData.CharacterObject)((WS_Totems.TotemObject)objCharacter.ActiveSpells[i].SpellCaster).Caster;
                                            }

                                            if (caster is null || caster.Group is null || caster.Group.LocalMembers.Contains(objCharacter.GUID) == false)
                                            {
                                                objCharacter.RemoveAura(i, ref objCharacter.ActiveSpells[i].SpellCaster);
                                            }
                                            else if (objCharacter.ActiveSpells[i].SpellCaster.HaveAura(objCharacter.ActiveSpells[i].SpellID) == false)
                                            {
                                                objCharacter.RemoveAura(i, ref objCharacter.ActiveSpells[i].SpellCaster);
                                            }
                                            else if (WorldServiceLocator._WS_Combat.GetDistance(objCharacter, objCharacter.ActiveSpells[i].SpellCaster) > objCharacter.ActiveSpells[i].Aura_Info[j].GetRadius)
                                            {
                                                objCharacter.RemoveAura(i, ref objCharacter.ActiveSpells[i].SpellCaster);
                                            }
                                        }
                                        else
                                        {
                                            objCharacter.RemoveAura(i, ref objCharacter.ActiveSpells[i].SpellCaster);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // NOTE: Manages ai movement
        public class TAIManager : IDisposable
        {
            public Timer AIManagerTimer = null;
            private bool AIManagerWorking = false;
            public const int UPDATE_TIMER = 1000;     // Timer period (ms)

            public TAIManager()
            {
                AIManagerTimer = new Timer(Update, null, 10000, UPDATE_TIMER);
            }

            private void Update(object state)
            {
                if (AIManagerWorking)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Update: AI Manager skipping update");
                    return;
                }

                int StartTime = WorldServiceLocator._NativeMethods.timeGetTime("");
                AIManagerWorking = true;

                // First transports
                try
                {
                    WorldServiceLocator._WorldServer.WORLD_TRANSPORTs_Lock.AcquireReaderLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                    foreach (KeyValuePair<ulong, WS_Transports.TransportObject> Transport in WorldServiceLocator._WorldServer.WORLD_TRANSPORTs)
                        Transport.Value.Update();
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error updating transports.{0}{1}", Environment.NewLine, ex.ToString());
                }
                finally
                {
                    WorldServiceLocator._WorldServer.WORLD_TRANSPORTs_Lock.ReleaseReaderLock();
                }

                // Then creatures
                try
                {
                    WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.AcquireReaderLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                    try
                    {
                        for (long i = 0L, loopTo = WorldServiceLocator._WorldServer.WORLD_CREATUREsKeys.Count - 1; i <= loopTo; i++)
                        {
                            if (WorldServiceLocator._WorldServer.WORLD_CREATUREs[Conversions.ToULong(WorldServiceLocator._WorldServer.WORLD_CREATUREsKeys[(int)i])] is object && WorldServiceLocator._WorldServer.WORLD_CREATUREs[Conversions.ToULong(WorldServiceLocator._WorldServer.WORLD_CREATUREsKeys[(int)i])].aiScript is object)
                            {
                                WorldServiceLocator._WorldServer.WORLD_CREATUREs[Conversions.ToULong(WorldServiceLocator._WorldServer.WORLD_CREATUREsKeys[(int)i])].aiScript.DoThink();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error updating AI.{0}{1}", Environment.NewLine, ex.ToString());
                    }
                    finally
                    {
                        WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.ReleaseReaderLock();
                    }
                }
                catch (ApplicationException)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Update: AI Manager timed out");
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error updating AI.{0}{1}", Environment.NewLine, ex.ToString());
                }

                AIManagerWorking = false;
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
                    AIManagerTimer.Dispose();
                    AIManagerTimer = null;
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
            // Protected Overrides Sub Finalize()
            // MyBase.Finalize()
            // End Sub
        }

        // NOTE: Manages character savings
        public class TCharacterSaver : IDisposable
        {
            public Timer CharacterSaverTimer = null;
            private bool CharacterSaverWorking = false;
            public int UPDATE_TIMER = WorldServiceLocator._WorldServer.Config.SaveTimer;     // Timer period (ms)

            public TCharacterSaver()
            {
                CharacterSaverTimer = new Timer(Update, null, 10000, UPDATE_TIMER);
            }

            private void Update(object state)
            {
                if (CharacterSaverWorking)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Update: Character Saver skipping update");
                    return;
                }

                CharacterSaverWorking = true;
                try
                {
                    WorldServiceLocator._WorldServer.CHARACTERs_Lock.AcquireReaderLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                    foreach (KeyValuePair<ulong, WS_PlayerData.CharacterObject> Character in WorldServiceLocator._WorldServer.CHARACTERs)
                        Character.Value.SaveCharacter();
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, ex.ToString(), default);
                }
                finally
                {
                    WorldServiceLocator._WorldServer.CHARACTERs_Lock.ReleaseReaderLock();
                }

                // Here we hook the instance expire checks too
                WorldServiceLocator._WS_Handlers_Instance.InstanceMapUpdate();
                CharacterSaverWorking = false;
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
                    CharacterSaverTimer.Dispose();
                    CharacterSaverTimer = null;
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

        // NOTE: Manages the weather
        public class TWeatherChanger : IDisposable
        {
            public Timer WeatherTimer = null;
            private bool WeatherWorking = false;
            public int UPDATE_TIMER = WorldServiceLocator._WorldServer.Config.WeatherTimer;     // Timer period (ms)

            public TWeatherChanger()
            {
                WeatherTimer = new Timer(Update, null, 10000, UPDATE_TIMER);
            }

            private void Update(object state)
            {
                if (WeatherWorking)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Update: Weather changer skipping update");
                    return;
                }

                WeatherWorking = true;
                foreach (KeyValuePair<int, WS_Weather.WeatherZone> Weather in WorldServiceLocator._WS_Weather.WeatherZones)
                    Weather.Value.Update();
                WeatherWorking = false;
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
                    WeatherTimer.Dispose();
                    WeatherTimer = null;
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

        // TODO: Timer for kicking not connected players (ping timeout)
        // TODO: Timer for auction items and mails
        // TODO: Timer for weather change

    }
}