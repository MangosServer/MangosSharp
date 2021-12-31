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
using Mangos.Common.Enums.Player;
using Mangos.World.Player;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Mangos.World.Server;

public partial class WS_TimerBasedEvents
{
    public class TRegenerator : IDisposable
    {
        private Timer RegenerationTimer;

        private bool RegenerationWorking;

        private readonly int operationsCount;

        private int BaseMana;

        private int BaseLife;

        private int BaseRage;

        private int BaseEnergy;

        private bool _updateFlag;

        private bool NextGroupUpdate;

        public const int REGENERATION_TIMER = 2;

        public const int REGENERATION_ENERGY = 20;

        public const int REGENERATION_RAGE = 25;

        private bool _disposedValue;

        public TRegenerator()
        {
            RegenerationTimer = null;
            RegenerationWorking = false;
            NextGroupUpdate = true;
            RegenerationTimer = new Timer(Regenerate, null, 10000, 2000);
        }

        private void Regenerate(object state)
        {
            if (RegenerationWorking)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Update: Regenerator skipping update");
                return;
            }
            RegenerationWorking = true;
            NextGroupUpdate = !NextGroupUpdate;
            checked
            {
                try
                {
                    WorldServiceLocator._WorldServer.CHARACTERs_Lock.AcquireReaderLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                    foreach (var Character in WorldServiceLocator._WorldServer.CHARACTERs)
                    {
                        if (Character.Value.DEAD || Character.Value.underWaterTimer != null || Character.Value.LogoutTimer != null || Character.Value.client == null)
                        {
                            continue;
                        }
                        var value = Character.Value;
                        BaseMana = value.Mana.Current;
                        BaseRage = value.Rage.Current;
                        BaseEnergy = value.Energy.Current;
                        BaseLife = value.Life.Current;
                        _updateFlag = false;
                        if (value.ManaType == ManaTypes.TYPE_RAGE)
                        {
                            switch (value.cUnitFlags & 0x80000)
                            {
                                case 0:
                                    if (value.Rage.Current > 0)
                                    {
                                        value.Rage.Current -= 25;
                                    }

                                    break;

                                default:
                                    if (value.RageRegenBonus != 0)
                                    {
                                        value.Rage.Increment(value.RageRegenBonus);
                                    }

                                    break;
                            }
                        }
                        if (value.ManaType == ManaTypes.TYPE_ENERGY && value.Energy.Current < value.Energy.Maximum)
                        {
                            value.Energy.Increment(20);
                        }
                        if (value.ManaRegen == 0)
                        {
                            value.UpdateManaRegen();
                        }
                        if (value.spellCastManaRegeneration == 0)
                        {
                            if ((value.ManaType == ManaTypes.TYPE_MANA || value.Classe == Classes.CLASS_DRUID) && value.Mana.Current < value.Mana.Maximum)
                            {
                                value.Mana.Increment(value.ManaRegen * 2);
                            }
                        }
                        else
                        {
                            if ((value.ManaType == ManaTypes.TYPE_MANA || value.Classe == Classes.CLASS_DRUID) && value.Mana.Current < value.Mana.Maximum)
                            {
                                value.Mana.Increment(value.ManaRegenInterrupt * 2);
                            }
                            if (value.spellCastManaRegeneration < 2)
                            {
                                value.spellCastManaRegeneration = 0;
                            }
                            else
                            {
                                value.spellCastManaRegeneration -= 2;
                            }
                        }
                        if (value.Life.Current < value.Life.Maximum && (value.cUnitFlags & 0x80000) == 0)
                        {
                            switch (value.Classe)
                            {
                                case Classes.CLASS_MAGE:
                                    value.Life.Increment((int)Math.Round(value.Spirit.Base * 0.1 * value.LifeRegenerationModifier) + value.LifeRegenBonus);
                                    break;

                                case Classes.CLASS_PRIEST:
                                    value.Life.Increment((int)Math.Round(value.Spirit.Base * 0.1 * value.LifeRegenerationModifier) + value.LifeRegenBonus);
                                    break;

                                case Classes.CLASS_WARLOCK:
                                    value.Life.Increment((int)Math.Round(value.Spirit.Base * 0.11 * value.LifeRegenerationModifier) + value.LifeRegenBonus);
                                    break;

                                case Classes.CLASS_DRUID:
                                    value.Life.Increment((int)Math.Round(value.Spirit.Base * 0.11 * value.LifeRegenerationModifier) + value.LifeRegenBonus);
                                    break;

                                case Classes.CLASS_SHAMAN:
                                    value.Life.Increment((int)Math.Round(value.Spirit.Base * 0.11 * value.LifeRegenerationModifier) + value.LifeRegenBonus);
                                    break;

                                case Classes.CLASS_ROGUE:
                                    value.Life.Increment((int)Math.Round(value.Spirit.Base * 0.5 * value.LifeRegenerationModifier) + value.LifeRegenBonus);
                                    break;

                                case Classes.CLASS_WARRIOR:
                                    value.Life.Increment((int)Math.Round(value.Spirit.Base * 0.8 * value.LifeRegenerationModifier) + value.LifeRegenBonus);
                                    break;

                                case Classes.CLASS_HUNTER:
                                    value.Life.Increment((int)Math.Round(value.Spirit.Base * 0.25 * value.LifeRegenerationModifier) + value.LifeRegenBonus);
                                    break;

                                case Classes.CLASS_PALADIN:
                                    value.Life.Increment((int)Math.Round(value.Spirit.Base * 0.25 * value.LifeRegenerationModifier) + value.LifeRegenBonus);
                                    break;
                            }
                        }
                        if (BaseMana != value.Mana.Current)
                        {
                            _updateFlag = true;
                            value.GroupUpdateFlag |= 16u;
                            value.SetUpdateFlag(23, value.Mana.Current);
                        }
                        if ((BaseRage != value.Rage.Current) || ((value.cUnitFlags & 0x80000) == 0x80000))
                        {
                            _updateFlag = true;
                            value.GroupUpdateFlag |= 16u;
                            value.SetUpdateFlag(24, value.Rage.Current);
                        }
                        if (BaseEnergy != value.Energy.Current)
                        {
                            _updateFlag = true;
                            value.GroupUpdateFlag |= 16u;
                            value.SetUpdateFlag(26, value.Energy.Current);
                        }
                        if (BaseLife != value.Life.Current)
                        {
                            _updateFlag = true;
                            value.SetUpdateFlag(22, value.Life.Current);
                            value.GroupUpdateFlag |= 2u;
                        }
                        if (_updateFlag)
                        {
                            value.SendCharacterUpdate();
                        }
                        if (value.DuelOutOfBounds != 11)
                        {
                            value.DuelOutOfBounds -= 2;
                            if (value.DuelOutOfBounds == 0)
                            {
                                WorldServiceLocator._WS_Spells.DuelComplete(ref value.DuelPartner, ref value.client.Character);
                            }
                        }
                        value.CheckCombat();
                        if (NextGroupUpdate)
                        {
                            value.GroupUpdate();
                        }
                        if (value.guidsForRemoving.Count > 0)
                        {
                            value.SendOutOfRangeUpdate();
                        }
                        value = null;
                    }
                    if (WorldServiceLocator._WorldServer.CHARACTERs_Lock.IsReaderLockHeld)
                    {
                        WorldServiceLocator._WorldServer.CHARACTERs_Lock.ReleaseReaderLock();
                    }
                }
                catch (Exception ex2)
                {
                    ProjectData.SetProjectError(ex2);
                    var ex = ex2;
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Error at regenerate.{0}", Environment.NewLine + ex);
                    ProjectData.ClearProjectError();
                }
                RegenerationWorking = false;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                RegenerationTimer.Dispose();
                RegenerationTimer = null;
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
}
