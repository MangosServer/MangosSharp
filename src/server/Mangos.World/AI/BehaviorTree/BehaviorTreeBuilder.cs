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

using Mangos.World.AI.BehaviorTree.Composites;
using Mangos.World.AI.BehaviorTree.Decorators;
using Mangos.World.AI.BehaviorTree.Leaves;
using System;
using System.Collections.Generic;

namespace Mangos.World.AI.BehaviorTree;

/// <summary>
/// Fluent builder for constructing behavior trees.
/// Inspired by fluid BT builders from game AI libraries.
///
/// Example:
/// <code>
/// var tree = new BehaviorTreeBuilder()
///     .Selector()
///         .Sequence()
///             .Leaf(new IsInCombatCheck())
///             .Leaf(new SelectHighestThreatTarget())
///             .Leaf(new CastSpellAction(SPELL_ID))
///         .End()
///         .Leaf(new WaitAction(3000))
///     .End()
///     .Build();
/// </code>
/// </summary>
public sealed class BehaviorTreeBuilder
{
    private readonly Stack<List<IBTNode>> _nodeStack = new();
    private readonly Stack<string> _typeStack = new(); // "selector" or "sequence"
    private IBTNode _root;

    public BehaviorTreeBuilder Selector()
    {
        _typeStack.Push("selector");
        _nodeStack.Push(new List<IBTNode>());
        return this;
    }

    public BehaviorTreeBuilder Sequence()
    {
        _typeStack.Push("sequence");
        _nodeStack.Push(new List<IBTNode>());
        return this;
    }

    public BehaviorTreeBuilder Leaf(IBTNode node)
    {
        if (_nodeStack.Count > 0)
        {
            _nodeStack.Peek().Add(node);
        }
        else
        {
            _root = node;
        }
        return this;
    }

    public BehaviorTreeBuilder Inverter()
    {
        _typeStack.Push("inverter");
        _nodeStack.Push(new List<IBTNode>());
        return this;
    }

    public BehaviorTreeBuilder Cooldown(int cooldownId, int cooldownMs)
    {
        _typeStack.Push($"cooldown:{cooldownId}:{cooldownMs}");
        _nodeStack.Push(new List<IBTNode>());
        return this;
    }

    public BehaviorTreeBuilder Condition(Func<BTContext, bool> condition)
    {
        _typeStack.Push("condition");
        _nodeStack.Push(new List<IBTNode>());
        // Store condition for use in End()
        _nodeStack.Peek().Add(new ConditionalNode(
            new Leaves.ExecuteAction(_ => BTStatus.Success), condition));
        return this;
    }

    public BehaviorTreeBuilder End()
    {
        var children = _nodeStack.Pop();
        var type = _typeStack.Pop();

        IBTNode node;
        if (type == "selector")
        {
            node = new SelectorNode(children.ToArray());
        }
        else if (type == "sequence")
        {
            node = new SequenceNode(children.ToArray());
        }
        else if (type == "inverter" && children.Count > 0)
        {
            node = new InverterNode(children[0]);
        }
        else if (type.StartsWith("cooldown:") && children.Count > 0)
        {
            var parts = type.Split(':');
            node = new CooldownNode(children[0], int.Parse(parts[1]), int.Parse(parts[2]));
        }
        else if (type == "condition" && children.Count > 0)
        {
            node = children[0];
        }
        else
        {
            node = children.Count > 0 ? children[0] : new Leaves.ExecuteAction(_ => BTStatus.Failure);
        }

        if (_nodeStack.Count > 0)
        {
            _nodeStack.Peek().Add(node);
        }
        else
        {
            _root = node;
        }

        return this;
    }

    public IBTNode Build()
    {
        return _root ?? new Leaves.ExecuteAction(_ => BTStatus.Failure);
    }
}
