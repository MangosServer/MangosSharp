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
/// Moves the creature away from a threat source (fear, low health flee).
/// </summary>
public sealed class FleeMovementGenerator : IMovementGenerator
{
    private readonly WS_Base.BaseUnit _fleeFrom;
    private int _fleeDurationMs;
    private int _elapsed;

    public MovementGeneratorType Type => MovementGeneratorType.Flee;

    public FleeMovementGenerator(WS_Base.BaseUnit fleeFrom, int durationMs = 5000)
    {
        _fleeFrom = fleeFrom;
        _fleeDurationMs = durationMs;
    }

    public void Initialize(WS_Creatures.CreatureObject creature) { }

    public bool Update(WS_Creatures.CreatureObject creature, int diffMs)
    {
        _elapsed += diffMs;
        if (_elapsed >= _fleeDurationMs) return true;

        if (_fleeFrom == null) return true;

        // Move away from the threat
        var dx = creature.positionX - _fleeFrom.positionX;
        var dy = creature.positionY - _fleeFrom.positionY;
        var angle = (float)Math.Atan2(dy, dx);
        creature.orientation = angle;

        return false;
    }

    public void Finalize(WS_Creatures.CreatureObject creature) { }
    public void Reset(WS_Creatures.CreatureObject creature) => _elapsed = 0;
}
