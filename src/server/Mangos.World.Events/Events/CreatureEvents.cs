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

namespace Mangos.World.Events.Events;

/// <summary>
/// Published when a creature is spawned into the world.
/// </summary>
public sealed class CreatureSpawnedEvent : IGameEvent
{
    public ulong CreatureGuid { get; init; }
    public int CreatureEntry { get; init; }
    public uint MapId { get; init; }
    public float X { get; init; }
    public float Y { get; init; }
    public float Z { get; init; }
}

/// <summary>
/// Published when a creature dies.
/// Inspired by TrinityCore's JustDied(Unit* killer) AI hook.
/// </summary>
public sealed class CreatureDeathEvent : IGameEvent
{
    public ulong CreatureGuid { get; init; }
    public int CreatureEntry { get; init; }
    public ulong KillerGuid { get; init; }
    public uint MapId { get; init; }
}

/// <summary>
/// Published when a creature enters combat.
/// Inspired by TrinityCore's JustEngagedWith(Unit* who) AI hook.
/// </summary>
public sealed class CreatureEnterCombatEvent : IGameEvent
{
    public ulong CreatureGuid { get; init; }
    public int CreatureEntry { get; init; }
    public ulong AttackerGuid { get; init; }
}

/// <summary>
/// Published when a creature leaves combat (evade or all enemies dead).
/// </summary>
public sealed class CreatureLeaveCombatEvent : IGameEvent
{
    public ulong CreatureGuid { get; init; }
    public int CreatureEntry { get; init; }
    public bool Evade { get; init; }
}

/// <summary>
/// Published when a creature takes damage.
/// Inspired by TrinityCore's DamageTaken(Unit* attacker, uint32& damage) hook.
/// </summary>
public sealed class CreatureDamageTakenEvent : IGameEvent
{
    public ulong CreatureGuid { get; init; }
    public int CreatureEntry { get; init; }
    public ulong AttackerGuid { get; init; }
    public int Damage { get; init; }
    public int RemainingHealth { get; init; }
}
