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
using System;

namespace Mangos.World.AI.Movement;

/// <summary>
/// Moves the creature toward a target unit for melee combat.
/// Extracted from DefaultAI.DoMove() chase logic.
/// </summary>
public sealed class ChaseMovementGenerator : IMovementGenerator
{
    private WS_Base.BaseUnit _target;

    public MovementGeneratorType Type => MovementGeneratorType.Chase;

    public ChaseMovementGenerator(WS_Base.BaseUnit target)
    {
        _target = target;
    }

    public void Initialize(WS_Creatures.CreatureObject creature) { }

    public bool Update(WS_Creatures.CreatureObject creature, int diffMs)
    {
        if (_target == null) return true; // Target gone, complete

        var dx = _target.positionX - creature.positionX;
        var dy = _target.positionY - creature.positionY;
        var distance = (float)Math.Sqrt(dx * dx + dy * dy);

        if (distance <= creature.CombatReach_Base + 1f)
        {
            return false; // In range, stay here
        }

        // Move toward target
        var angle = (float)Math.Atan2(dy, dx);
        creature.orientation = angle;

        return false; // Chase never self-completes; removed by AI state transitions
    }

    public void Finalize(WS_Creatures.CreatureObject creature) { }

    public void Reset(WS_Creatures.CreatureObject creature) { }
}
