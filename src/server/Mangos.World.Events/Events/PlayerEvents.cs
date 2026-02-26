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
/// Published when a player logs into the world.
/// </summary>
public sealed class PlayerLoginEvent : IGameEvent
{
    public ulong PlayerGuid { get; init; }
    public string PlayerName { get; init; }
    public uint MapId { get; init; }
}

/// <summary>
/// Published when a player logs out of the world.
/// </summary>
public sealed class PlayerLogoutEvent : IGameEvent
{
    public ulong PlayerGuid { get; init; }
    public string PlayerName { get; init; }
}

/// <summary>
/// Published when a player gains a level.
/// </summary>
public sealed class PlayerLevelUpEvent : IGameEvent
{
    public ulong PlayerGuid { get; init; }
    public int OldLevel { get; init; }
    public int NewLevel { get; init; }
}

/// <summary>
/// Published when a player dies.
/// </summary>
public sealed class PlayerDeathEvent : IGameEvent
{
    public ulong PlayerGuid { get; init; }
    public ulong KillerGuid { get; init; }
    public uint MapId { get; init; }
    public float X { get; init; }
    public float Y { get; init; }
    public float Z { get; init; }
}
