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
using Mangos.World.Objects;
using System;
using System.Collections.Generic;

namespace Mangos.World.AI;

public partial class WS_Creatures_AI
{
    public class TBaseAI : IDisposable
    {
        public AIState State;

        public WS_Base.BaseUnit aiTarget;

        public Dictionary<WS_Base.BaseUnit, int> aiHateTable;

        public List<WS_Base.BaseUnit> aiHateTableRemove;

        private bool _disposedValue;

        public virtual bool InCombat => aiHateTable.Count > 0;

        public void ResetThreatTable()
        {
            List<WS_Base.BaseUnit> tmpUnits = new();
            foreach (var item in aiHateTable)
            {
                tmpUnits?.Add(item.Key);
            }
            aiHateTable?.Clear();
            foreach (var Victim in tmpUnits)
            {
                aiHateTable?.Add(Victim, 0);
            }
        }

        public virtual bool IsMoving => (uint)(State - 2) <= 4u;

        public virtual bool IsRunning => State == AIState.AI_MOVE_FOR_ATTACK;

        public virtual void Reset()
        {
            State = AIState.AI_DO_NOTHING;
        }

        public virtual void Pause(int Time)
        {
        }

        public virtual void OnEnterCombat()
        {
        }

        public virtual void OnLeaveCombat(bool Reset = true)
        {
        }

        public virtual void OnGenerateHate(ref WS_Base.BaseUnit Attacker, int HateValue)
        {
        }

        public virtual void OnKill(ref WS_Base.BaseUnit Victim)
        {
        }

        public virtual void OnHealthChange(int Percent)
        {
        }

        public virtual void OnDeath()
        {
        }

        public virtual void DoThink()
        {
        }

        public virtual void DoMove()
        {
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

        public TBaseAI()
        {
            State = AIState.AI_DO_NOTHING;
            aiTarget = null;
            aiHateTable = new Dictionary<WS_Base.BaseUnit, int>();
            aiHateTableRemove = new List<WS_Base.BaseUnit>();
        }
    }
}
