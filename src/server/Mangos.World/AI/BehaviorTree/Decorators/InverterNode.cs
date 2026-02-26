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
/// Inverts the result of its child node (Success → Failure, Failure → Success).
/// </summary>
public sealed class InverterNode : IBTNode
{
    private readonly IBTNode _child;

    public InverterNode(IBTNode child) => _child = child;

    public BTStatus Tick(BTContext context, int diffMs)
    {
        var status = _child.Tick(context, diffMs);
        return status switch
        {
            BTStatus.Success => BTStatus.Failure,
            BTStatus.Failure => BTStatus.Success,
            _ => status
        };
    }

    public void Reset() => _child.Reset();
}
