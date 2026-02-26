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
/// Published when a spell cast begins.
/// </summary>
public sealed class SpellCastStartEvent : IGameEvent
{
    public ulong CasterGuid { get; init; }
    public int SpellId { get; init; }
    public ulong TargetGuid { get; init; }
    public int CastTime { get; init; }
}

/// <summary>
/// Published when a spell cast completes successfully.
/// </summary>
public sealed class SpellCastCompleteEvent : IGameEvent
{
    public ulong CasterGuid { get; init; }
    public int SpellId { get; init; }
    public ulong TargetGuid { get; init; }
}

/// <summary>
/// Published when a spell hits a target.
/// Inspired by TrinityCore's SpellHit(Unit* caster, SpellInfo const* spell) AI hook.
/// </summary>
public sealed class SpellHitEvent : IGameEvent
{
    public ulong CasterGuid { get; init; }
    public ulong TargetGuid { get; init; }
    public int SpellId { get; init; }
    public int Damage { get; init; }
    public int Healing { get; init; }
}
