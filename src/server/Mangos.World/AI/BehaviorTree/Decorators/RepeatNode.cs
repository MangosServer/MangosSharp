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
/// Repeats the child node a specified number of times (or infinitely if count is -1).
/// </summary>
public sealed class RepeatNode : IBTNode
{
    private readonly IBTNode _child;
    private readonly int _maxRepeats;
    private int _currentRepeat;

    public RepeatNode(IBTNode child, int maxRepeats = -1)
    {
        _child = child;
        _maxRepeats = maxRepeats;
    }

    public BTStatus Tick(BTContext context, int diffMs)
    {
        var status = _child.Tick(context, diffMs);
        if (status == BTStatus.Running) return BTStatus.Running;

        _currentRepeat++;
        if (_maxRepeats > 0 && _currentRepeat >= _maxRepeats)
        {
            return BTStatus.Success;
        }

        _child.Reset();
        return BTStatus.Running;
    }

    public void Reset()
    {
        _currentRepeat = 0;
        _child.Reset();
    }
}
