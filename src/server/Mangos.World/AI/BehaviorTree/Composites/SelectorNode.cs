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

using System.Collections.Generic;

namespace Mangos.World.AI.BehaviorTree.Composites;

/// <summary>
/// Selector (OR) composite: tries children in order until one succeeds.
/// Returns Success on first child success, Failure if all children fail.
/// Equivalent to UE4's BTComposite_Selector.
/// </summary>
public sealed class SelectorNode : IBTNode
{
    private readonly List<IBTNode> _children;
    private int _currentChild;

    public SelectorNode(params IBTNode[] children)
    {
        _children = new List<IBTNode>(children);
    }

    public BTStatus Tick(BTContext context, int diffMs)
    {
        while (_currentChild < _children.Count)
        {
            var status = _children[_currentChild].Tick(context, diffMs);
            switch (status)
            {
                case BTStatus.Success:
                    _currentChild = 0;
                    return BTStatus.Success;
                case BTStatus.Running:
                    return BTStatus.Running;
                case BTStatus.Failure:
                    _currentChild++;
                    break;
            }
        }
        _currentChild = 0;
        return BTStatus.Failure;
    }

    public void Reset()
    {
        _currentChild = 0;
        foreach (var child in _children) child.Reset();
    }
}
