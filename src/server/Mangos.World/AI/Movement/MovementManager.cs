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

using Mangos.World.Objects;
using System.Collections.Generic;

namespace Mangos.World.AI.Movement;

/// <summary>
/// Manages a stack of movement generators for a creature. The topmost generator
/// is the active one. Supports Push/Pop/Clear semantics.
///
/// Inspired by TrinityCore's MotionMaster which uses a priority-based movement stack.
/// </summary>
public sealed class MovementManager
{
    private readonly Stack<IMovementGenerator> _generators = new();
    private readonly WS_Creatures.CreatureObject _creature;

    public MovementManager(WS_Creatures.CreatureObject creature)
    {
        _creature = creature;
    }

    /// <summary>
    /// The currently active movement generator type.
    /// </summary>
    public MovementGeneratorType CurrentType =>
        _generators.Count > 0 ? _generators.Peek().Type : MovementGeneratorType.Idle;

    /// <summary>
    /// Pushes a new movement generator onto the stack, making it active.
    /// </summary>
    public void Push(IMovementGenerator generator)
    {
        generator.Initialize(_creature);
        _generators.Push(generator);
    }

    /// <summary>
    /// Removes the topmost movement generator.
    /// </summary>
    public void Pop()
    {
        if (_generators.Count > 0)
        {
            var gen = _generators.Pop();
            gen.Finalize(_creature);
        }
    }

    /// <summary>
    /// Clears all movement generators.
    /// </summary>
    public void Clear()
    {
        while (_generators.Count > 0)
        {
            var gen = _generators.Pop();
            gen.Finalize(_creature);
        }
    }

    /// <summary>
    /// Updates the active movement generator. If it completes, pops it.
    /// </summary>
    public void Update(int diffMs)
    {
        if (_generators.Count == 0) return;

        var active = _generators.Peek();
        if (active.Update(_creature, diffMs))
        {
            Pop();
        }
    }

    /// <summary>
    /// Replaces all movement with a single new generator.
    /// </summary>
    public void MovePoint(float x, float y, float z, int pointId = 0)
    {
        Push(new PointMovementGenerator(x, y, z, pointId));
    }

    /// <summary>
    /// Starts chasing a target.
    /// </summary>
    public void MoveChase(WS_Base.BaseUnit target)
    {
        Push(new ChaseMovementGenerator(target));
    }

    /// <summary>
    /// Starts fleeing from a unit.
    /// </summary>
    public void MoveFlee(WS_Base.BaseUnit from, int durationMs = 5000)
    {
        Push(new FleeMovementGenerator(from, durationMs));
    }

    /// <summary>
    /// Starts returning to spawn point.
    /// </summary>
    public void MoveReturnHome()
    {
        Push(new ReturnHomeMovementGenerator());
    }

    /// <summary>
    /// Starts random wandering around spawn.
    /// </summary>
    public void MoveRandom(float radius = 10f)
    {
        Push(new RandomMovementGenerator(radius));
    }

    /// <summary>
    /// Returns true if a specific movement type is active.
    /// </summary>
    public bool HasMovementType(MovementGeneratorType type)
    {
        foreach (var gen in _generators)
        {
            if (gen.Type == type) return true;
        }
        return false;
    }
}
