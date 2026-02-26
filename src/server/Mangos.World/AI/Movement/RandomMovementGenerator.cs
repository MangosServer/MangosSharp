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
/// Generates random wandering movement around a spawn point.
/// Extracted from DefaultAI.DoMove() wandering logic.
/// </summary>
public sealed class RandomMovementGenerator : IMovementGenerator
{
    private static readonly Random Rng = new();
    private float _spawnX, _spawnY, _spawnZ;
    private float _wanderRadius;
    private int _nextMoveTimer;
    private bool _initialized;

    public MovementGeneratorType Type => MovementGeneratorType.Random;

    public RandomMovementGenerator(float wanderRadius = 10f)
    {
        _wanderRadius = wanderRadius;
        _nextMoveTimer = 0;
    }

    public void Initialize(WS_Creatures.CreatureObject creature)
    {
        _spawnX = creature.SpawnX;
        _spawnY = creature.SpawnY;
        _spawnZ = creature.SpawnZ;
        _nextMoveTimer = Rng.Next(1000, 5000);
        _initialized = true;
    }

    public bool Update(WS_Creatures.CreatureObject creature, int diffMs)
    {
        if (!_initialized) Initialize(creature);

        _nextMoveTimer -= diffMs;
        if (_nextMoveTimer > 0) return false;

        // Generate random position within wander radius of spawn
        var angle = (float)(Rng.NextDouble() * Math.PI * 2);
        var distance = (float)(Rng.NextDouble() * _wanderRadius);
        var targetX = _spawnX + (float)(Math.Cos(angle) * distance);
        var targetY = _spawnY + (float)(Math.Sin(angle) * distance);

        creature.positionX = targetX;
        creature.positionY = targetY;
        creature.orientation = angle;

        _nextMoveTimer = Rng.Next(3000, 8000);
        return false; // Never completes on its own
    }

    public void Finalize(WS_Creatures.CreatureObject creature) { }

    public void Reset(WS_Creatures.CreatureObject creature)
    {
        _initialized = false;
    }
}
