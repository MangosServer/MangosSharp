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

namespace Mangos.World.AI.BehaviorTree.Leaves;

/// <summary>
/// Returns Success if the creature is in combat (has enemies in hate table).
/// </summary>
public sealed class IsInCombatCheck : IBTNode
{
    public BTStatus Tick(BTContext context, int diffMs)
    {
        return context.AI?.InCombat == true ? BTStatus.Success : BTStatus.Failure;
    }
    public void Reset() { }
}

/// <summary>
/// Returns Success if the creature has a valid target.
/// </summary>
public sealed class HasTargetCheck : IBTNode
{
    public BTStatus Tick(BTContext context, int diffMs)
    {
        return context.AI?.aiTarget != null ? BTStatus.Success : BTStatus.Failure;
    }
    public void Reset() { }
}

/// <summary>
/// Returns Success if the creature's health is below the specified percentage.
/// </summary>
public sealed class HealthBelowCheck : IBTNode
{
    private readonly int _percent;

    public HealthBelowCheck(int percent) => _percent = percent;

    public BTStatus Tick(BTContext context, int diffMs)
    {
        if (context.Creature == null) return BTStatus.Failure;
        if (context.Creature.Life.Maximum == 0) return BTStatus.Failure;

        var currentPct = checked((int)(context.Creature.Life.Current * 100L / context.Creature.Life.Maximum));
        return currentPct <= _percent ? BTStatus.Success : BTStatus.Failure;
    }
    public void Reset() { }
}

/// <summary>
/// Returns Success if a blackboard key exists and is true.
/// </summary>
public sealed class BlackboardCheck : IBTNode
{
    private readonly string _key;

    public BlackboardCheck(string key) => _key = key;

    public BTStatus Tick(BTContext context, int diffMs)
    {
        return context.Get<bool>(_key) ? BTStatus.Success : BTStatus.Failure;
    }
    public void Reset() { }
}
