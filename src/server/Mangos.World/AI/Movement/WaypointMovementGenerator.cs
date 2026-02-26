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
using System.Collections.Generic;

namespace Mangos.World.AI.Movement;

/// <summary>
/// Moves the creature along a predefined path of waypoints.
/// Extracted from WaypointAI, provides a cleaner abstraction.
/// Inspired by TrinityCore's WaypointMovementGenerator.
/// </summary>
public sealed class WaypointMovementGenerator : IMovementGenerator
{
    public struct Waypoint
    {
        public float X;
        public float Y;
        public float Z;
        public int WaitTimeMs;
    }

    private readonly List<Waypoint> _waypoints;
    private readonly bool _repeat;
    private int _currentIndex;
    private int _waitTimer;
    private bool _waiting;

    public MovementGeneratorType Type => MovementGeneratorType.Waypoint;

    public WaypointMovementGenerator(List<Waypoint> waypoints, bool repeat = true)
    {
        _waypoints = waypoints;
        _repeat = repeat;
        _currentIndex = 0;
    }

    public void Initialize(WS_Creatures.CreatureObject creature) { }

    public bool Update(WS_Creatures.CreatureObject creature, int diffMs)
    {
        if (_waypoints.Count == 0) return true;
        if (_currentIndex >= _waypoints.Count)
        {
            if (_repeat)
            {
                _currentIndex = 0;
            }
            else
            {
                return true;
            }
        }

        if (_waiting)
        {
            _waitTimer -= diffMs;
            if (_waitTimer > 0) return false;
            _waiting = false;
            _currentIndex++;
            return _currentIndex >= _waypoints.Count && !_repeat;
        }

        var wp = _waypoints[_currentIndex];
        var dx = wp.X - creature.positionX;
        var dy = wp.Y - creature.positionY;
        var distance = (float)Math.Sqrt(dx * dx + dy * dy);

        if (distance < 1f)
        {
            creature.positionX = wp.X;
            creature.positionY = wp.Y;
            creature.positionZ = wp.Z;

            if (wp.WaitTimeMs > 0)
            {
                _waiting = true;
                _waitTimer = wp.WaitTimeMs;
            }
            else
            {
                _currentIndex++;
            }
            return false;
        }

        var angle = (float)Math.Atan2(dy, dx);
        creature.orientation = angle;
        return false;
    }

    public void Finalize(WS_Creatures.CreatureObject creature) { }

    public void Reset(WS_Creatures.CreatureObject creature)
    {
        _currentIndex = 0;
        _waiting = false;
    }

    public int CurrentWaypointIndex => _currentIndex;
}
