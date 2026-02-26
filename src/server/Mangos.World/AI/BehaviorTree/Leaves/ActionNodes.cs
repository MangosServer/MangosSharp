//
// Copyright (C) 2013-2025 getMaNGOS <https://www.getmangos.eu>
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

namespace Mangos.World.AI.BehaviorTree.Leaves;

/// <summary>
/// Selects the highest-threat target from the hate table.
/// </summary>
public sealed class SelectHighestThreatTarget : IBTNode
{
    public BTStatus Tick(BTContext context, int diffMs)
    {
        if (context.AI == null || context.AI.aiHateTable.Count == 0)
            return BTStatus.Failure;

        Objects.WS_Base.BaseUnit bestTarget = null;
        int maxHate = int.MinValue;
        foreach (var entry in context.AI.aiHateTable)
        {
            if (entry.Value > maxHate)
            {
                maxHate = entry.Value;
                bestTarget = entry.Key;
            }
        }

        if (bestTarget != null)
        {
            context.AI.aiTarget = bestTarget;
            return BTStatus.Success;
        }
        return BTStatus.Failure;
    }
    public void Reset() { }
}

/// <summary>
/// Casts a specified spell on the current target.
/// </summary>
public sealed class CastSpellAction : IBTNode
{
    private readonly int _spellId;

    public CastSpellAction(int spellId) => _spellId = spellId;

    public BTStatus Tick(BTContext context, int diffMs)
    {
        if (context.Creature == null || context.AI?.aiTarget == null)
            return BTStatus.Failure;

        context.Creature.CastSpell(_spellId, context.AI.aiTarget);
        return BTStatus.Success;
    }
    public void Reset() { }
}

/// <summary>
/// Executes an arbitrary action delegate.
/// </summary>
public sealed class ExecuteAction : IBTNode
{
    private readonly Func<BTContext, BTStatus> _action;

    public ExecuteAction(Func<BTContext, BTStatus> action) => _action = action;

    public BTStatus Tick(BTContext context, int diffMs) => _action(context);
    public void Reset() { }
}

/// <summary>
/// Waits for a specified duration. Returns Running until the time elapses.
/// </summary>
public sealed class WaitAction : IBTNode
{
    private readonly int _durationMs;
    private int _elapsed;

    public WaitAction(int durationMs) => _durationMs = durationMs;

    public BTStatus Tick(BTContext context, int diffMs)
    {
        _elapsed += diffMs;
        return _elapsed >= _durationMs ? BTStatus.Success : BTStatus.Running;
    }

    public void Reset() => _elapsed = 0;
}
