//
//  Copyright (C) 2013-2021 getMaNGOS <https://getmangos.eu>
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Spell;
using Mangos.World.Objects;
using Mangos.World.Player;
using Mangos.World.Spells;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Server
{
    public partial class WS_TimerBasedEvents
    {
        public class TSpellManager : IDisposable
        {
            private Timer SpellManagerTimer;

            private bool SpellManagerWorking;

            public const int UPDATE_TIMER = 1000;

            private bool _disposedValue;

            public TSpellManager()
            {
                SpellManagerTimer = null;
                SpellManagerWorking = false;
                SpellManagerTimer = new Timer(Update, null, 10000, 1000);
            }

            private void Update(object state)
            {
                if (SpellManagerWorking)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Update: Spell Manager skipping update");
                    return;
                }
                SpellManagerWorking = true;
                checked
                {
                    try
                    {
                        WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.AcquireReaderLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                        long num = WorldServiceLocator._WorldServer.WORLD_CREATUREsKeys.Count - 1;
                        for (long i = 0L; i <= num; i++)
                        {
                            WS_Creatures.CreatureObject creatureObject = WorldServiceLocator._WorldServer.WORLD_CREATUREs[Conversions.ToULong(WorldServiceLocator._WorldServer.WORLD_CREATUREsKeys[(int)i])];
                            if (creatureObject != null)
                            {
                                ulong key;
                                Dictionary<ulong, WS_Creatures.CreatureObject> wORLD_CREATUREs;
                                object value = WorldServiceLocator._WorldServer.WORLD_CREATUREsKeys[(int)i];
                                WS_Base.BaseUnit objCharacter = (wORLD_CREATUREs = WorldServiceLocator._WorldServer.WORLD_CREATUREs)[key = Conversions.ToULong(value)];
                                UpdateSpells(ref objCharacter);
                                wORLD_CREATUREs[key] = (WS_Creatures.CreatureObject)objCharacter;
                            }
                        }
                    }
                    catch (Exception ex4)
                    {
                        ProjectData.SetProjectError(ex4);
                        Exception ex3 = ex4;
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, ex3.ToString(), null);
                        ProjectData.ClearProjectError();
                    }
                    finally
                    {
                        if (WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.IsReaderLockHeld)
                        {
                            WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.ReleaseReaderLock();
                        }
                    }
                    try
                    {
                        WorldServiceLocator._WorldServer.CHARACTERs_Lock.AcquireReaderLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                        foreach (var Character in WorldServiceLocator._WorldServer.CHARACTERs.Where(Character => Character.Value != null))
                        {
                            WS_Base.BaseUnit objCharacter = Character.Value;
                            UpdateSpells(ref objCharacter);
                        }
                    }
                    catch (Exception ex5)
                    {
                        ProjectData.SetProjectError(ex5);
                        Exception ex2 = ex5;
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, ex2.ToString(), null);
                        ProjectData.ClearProjectError();
                    }
                    finally
                    {
                        WorldServiceLocator._WorldServer.CHARACTERs_Lock.ReleaseReaderLock();
                    }
                    List<WS_DynamicObjects.DynamicObjectObject> DynamicObjectsToDelete = new List<WS_DynamicObjects.DynamicObjectObject>();
                    try
                    {
                        WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs_Lock.AcquireReaderLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                        foreach (KeyValuePair<ulong, WS_DynamicObjects.DynamicObjectObject> Dynamic in WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs)
                        {
                            if (Dynamic.Value != null && Dynamic.Value.Update())
                            {
                                DynamicObjectsToDelete.Add(Dynamic.Value);
                            }
                        }
                    }
                    catch (Exception ex6)
                    {
                        ProjectData.SetProjectError(ex6);
                        Exception ex = ex6;
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, ex.ToString(), null);
                        ProjectData.ClearProjectError();
                    }
                    finally
                    {
                        WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs_Lock.ReleaseReaderLock();
                    }
                    foreach (WS_DynamicObjects.DynamicObjectObject item in DynamicObjectsToDelete)
                    {
                        item?.Delete();
                    }
                    SpellManagerWorking = false;
                }
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    SpellManagerTimer.Dispose();
                    SpellManagerTimer = null;
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

            private void UpdateSpells(ref WS_Base.BaseUnit objCharacter)
            {
                if (objCharacter is WS_Totems.TotemObject @object)
                {
                    @object.Update();
                    return;
                }
                checked
                {
                    for (int i = 0; i <= WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs - 1; i++)
                    {
                        if (objCharacter.ActiveSpells[i] == null)
                        {
                            continue;
                        }
                        if (objCharacter.ActiveSpells[i].SpellDuration != WorldServiceLocator._Global_Constants.SPELL_DURATION_INFINITE)
                        {
                            objCharacter.ActiveSpells[i].SpellDuration -= 1000;
                            byte j = 0;
                            do
                            {
                                if (objCharacter.ActiveSpells[i] != null && objCharacter.ActiveSpells[i].Aura[j] != null && objCharacter.ActiveSpells[i].Aura_Info[j] != null && objCharacter.ActiveSpells[i].Aura_Info[j].Amplitude != 0 && checked(objCharacter.ActiveSpells[i].GetSpellInfo.GetDuration - objCharacter.ActiveSpells[i].SpellDuration) % objCharacter.ActiveSpells[i].Aura_Info[j].Amplitude == 0)
                                {
                                    ref WS_Base.BaseUnit spellCaster = ref objCharacter.ActiveSpells[i].SpellCaster;
                                    WS_Base.BaseObject Caster = spellCaster;
                                    objCharacter.ActiveSpells[i].Aura[j](ref objCharacter, ref Caster, ref objCharacter.ActiveSpells[i].Aura_Info[j], objCharacter.ActiveSpells[i].SpellID, objCharacter.ActiveSpells[i].StackCount + 1, AuraAction.AURA_UPDATE);
                                    ref WS_Base.BaseUnit reference = ref spellCaster;
                                    reference = (WS_Base.BaseUnit)Caster;
                                }
                                j = (byte)unchecked((uint)(j + 1));
                            }
                            while (j <= 2u);
                            if (objCharacter.ActiveSpells[i] != null && objCharacter.ActiveSpells[i].SpellDuration <= 0 && objCharacter.ActiveSpells[i].SpellDuration != WorldServiceLocator._Global_Constants.SPELL_DURATION_INFINITE)
                            {
                                objCharacter.RemoveAura(i, ref objCharacter.ActiveSpells[i].SpellCaster, RemovedByDuration: true);
                            }
                        }
                        byte k = 0;
                        do
                        {
                            if (objCharacter.ActiveSpells[i] != null && objCharacter.ActiveSpells[i].Aura_Info[k] != null && objCharacter.ActiveSpells[i].Aura_Info[k].ID == SpellEffects_Names.SPELL_EFFECT_APPLY_AREA_AURA)
                            {
                                if (objCharacter.ActiveSpells[i].SpellCaster == objCharacter)
                                {
                                    List<WS_Base.BaseUnit> Targets = new List<WS_Base.BaseUnit>();
                                    switch (objCharacter)
                                    {
                                        case WS_PlayerData.CharacterObject _:
                                            {
                                                WS_Spells wS_Spells = WorldServiceLocator._WS_Spells;
                                                WS_PlayerData.CharacterObject objCharacter2 = (WS_PlayerData.CharacterObject)objCharacter;
                                                Targets = wS_Spells.GetPartyMembersAroundMe(ref objCharacter2, objCharacter.ActiveSpells[i].Aura_Info[k].GetRadius);
                                                break;
                                            }

                                        case WS_Totems.TotemObject _ when ((WS_Totems.TotemObject)objCharacter).Caster is not null and WS_PlayerData.CharacterObject:
                                            {
                                                WS_Spells wS_Spells2 = WorldServiceLocator._WS_Spells;
                                                ref WS_Base.BaseUnit caster2 = ref ((WS_Totems.TotemObject)objCharacter).Caster;
                                                ref WS_Base.BaseUnit reference = ref caster2;
                                                WS_PlayerData.CharacterObject objCharacter2 = (WS_PlayerData.CharacterObject)caster2;
                                                List<WS_Base.BaseUnit> partyMembersAtPoint = wS_Spells2.GetPartyMembersAtPoint(ref objCharacter2, objCharacter.ActiveSpells[i].Aura_Info[k].GetRadius, objCharacter.positionX, objCharacter.positionY, objCharacter.positionZ);
                                                reference = objCharacter2;
                                                Targets = partyMembersAtPoint;
                                                break;
                                            }
                                    }
                                    foreach (WS_Base.BaseUnit item in Targets)
                                    {
                                        WS_Base.BaseUnit Unit = item;
                                        if (!Unit.HaveAura(objCharacter.ActiveSpells[i].SpellID))
                                        {
                                            WS_Spells wS_Spells3 = WorldServiceLocator._WS_Spells;
                                            WS_Base.BaseObject Caster = objCharacter;
                                            wS_Spells3.ApplyAura(ref Unit, ref Caster, ref objCharacter.ActiveSpells[i].Aura_Info[k], objCharacter.ActiveSpells[i].SpellID);
                                            objCharacter = (WS_Base.BaseUnit)Caster;
                                        }
                                    }
                                }
                                else if (objCharacter.ActiveSpells[i].SpellCaster != null && objCharacter.ActiveSpells[i].SpellCaster.Exist)
                                {
                                    WS_PlayerData.CharacterObject caster = null;
                                    switch (objCharacter.ActiveSpells[i].SpellCaster)
                                    {
                                        case WS_PlayerData.CharacterObject _:
                                            caster = (WS_PlayerData.CharacterObject)objCharacter.ActiveSpells[i].SpellCaster;
                                            break;
                                        case WS_Totems.TotemObject _ when ((WS_Totems.TotemObject)objCharacter.ActiveSpells[i].SpellCaster).Caster is not null and WS_PlayerData.CharacterObject object1:
                                            caster = object1;
                                            break;
                                    }
                                    if (caster == null || caster.Group == null || !caster.Group.LocalMembers.Contains(objCharacter.GUID))
                                    {
                                        objCharacter.RemoveAura(i, ref objCharacter.ActiveSpells[i].SpellCaster);
                                    }
                                    else if (!objCharacter.ActiveSpells[i].SpellCaster.HaveAura(objCharacter.ActiveSpells[i].SpellID))
                                    {
                                        objCharacter.RemoveAura(i, ref objCharacter.ActiveSpells[i].SpellCaster);
                                    }
                                    else if (WorldServiceLocator._WS_Combat.GetDistance(objCharacter, objCharacter.ActiveSpells[i].SpellCaster) > objCharacter.ActiveSpells[i].Aura_Info[k].GetRadius)
                                    {
                                        objCharacter.RemoveAura(i, ref objCharacter.ActiveSpells[i].SpellCaster);
                                    }
                                }
                                else
                                {
                                    objCharacter.RemoveAura(i, ref objCharacter.ActiveSpells[i].SpellCaster);
                                }
                            }
                            k = (byte)unchecked((uint)(k + 1));
                        }
                        while (k <= 2u);
                    }
                }
            }
        }
    }
}
