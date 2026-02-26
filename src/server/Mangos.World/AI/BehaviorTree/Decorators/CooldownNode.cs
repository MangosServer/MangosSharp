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

namespace Mangos.World.AI.BehaviorTree.Decorators;

/// <summary>
/// Only allows the child to execute if the specified cooldown ID is ready.
/// Integrates with the CooldownManager.
/// </summary>
public sealed class CooldownNode : IBTNode
{
    private readonly IBTNode _child;
    private readonly int _cooldownId;
    private readonly int _cooldownMs;

    public CooldownNode(IBTNode child, int cooldownId, int cooldownMs)
    {
        _child = child;
        _cooldownId = cooldownId;
        _cooldownMs = cooldownMs;
    }

    public BTStatus Tick(BTContext context, int diffMs)
    {
        if (!context.Cooldowns.IsReady(_cooldownId))
            return BTStatus.Failure;

        var status = _child.Tick(context, diffMs);
        if (status == BTStatus.Success)
        {
            context.Cooldowns.StartCooldown(_cooldownId, _cooldownMs);
        }
        return status;
    }

    public void Reset() => _child.Reset();
}
