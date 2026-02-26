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
/// Published when damage is dealt to any unit.
/// </summary>
public sealed class DamageDealtEvent : IGameEvent
{
    public ulong AttackerGuid { get; init; }
    public ulong VictimGuid { get; init; }
    public int Damage { get; init; }
    public int DamageType { get; init; }
    public int SpellId { get; init; }
    public bool IsCritical { get; init; }
}

/// <summary>
/// Published when healing is done to any unit.
/// </summary>
public sealed class HealingDoneEvent : IGameEvent
{
    public ulong HealerGuid { get; init; }
    public ulong TargetGuid { get; init; }
    public int Amount { get; init; }
    public int SpellId { get; init; }
    public bool IsCritical { get; init; }
}

/// <summary>
/// Published when threat is generated against a creature.
/// </summary>
public sealed class ThreatGeneratedEvent : IGameEvent
{
    public ulong CreatureGuid { get; init; }
    public ulong SourceGuid { get; init; }
    public int ThreatAmount { get; init; }
    public int SpellId { get; init; }
}
