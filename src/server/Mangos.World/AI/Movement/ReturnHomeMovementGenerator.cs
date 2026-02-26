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
/// Moves the creature back to its spawn point after evading.
/// Extracted from DefaultAI.DoMoveReset() logic.
/// </summary>
public sealed class ReturnHomeMovementGenerator : IMovementGenerator
{
    public MovementGeneratorType Type => MovementGeneratorType.ReturnHome;

    public void Initialize(WS_Creatures.CreatureObject creature) { }

    public bool Update(WS_Creatures.CreatureObject creature, int diffMs)
    {
        var dx = creature.SpawnX - creature.positionX;
        var dy = creature.SpawnY - creature.positionY;
        var distance = (float)Math.Sqrt(dx * dx + dy * dy);

        if (distance < 1f)
        {
            creature.positionX = creature.SpawnX;
            creature.positionY = creature.SpawnY;
            creature.positionZ = creature.SpawnZ;
            creature.orientation = creature.SpawnO;
            return true; // Arrived home
        }

        var angle = (float)Math.Atan2(dy, dx);
        creature.orientation = angle;

        return false;
    }

    public void Finalize(WS_Creatures.CreatureObject creature) { }
    public void Reset(WS_Creatures.CreatureObject creature) { }
}
