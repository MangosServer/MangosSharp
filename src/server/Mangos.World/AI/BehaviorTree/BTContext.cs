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

namespace Mangos.World.AI.BehaviorTree;

/// <summary>
/// Shared context passed to all behavior tree nodes during evaluation.
/// Contains the creature, its AI state, cooldowns, and a blackboard for inter-node data sharing.
/// Inspired by UE4's BTBlackboard component.
/// </summary>
public sealed class BTContext
{
    public WS_Creatures.CreatureObject Creature { get; }
    public WS_Creatures_AI.TBaseAI AI { get; }
    public CooldownManager Cooldowns { get; }

    private readonly Dictionary<string, object> _blackboard = new();

    public BTContext(WS_Creatures.CreatureObject creature, WS_Creatures_AI.TBaseAI ai, CooldownManager cooldowns = null)
    {
        Creature = creature;
        AI = ai;
        Cooldowns = cooldowns ?? new CooldownManager();
    }

    public void Set<T>(string key, T value) => _blackboard[key] = value;

    public T Get<T>(string key, T defaultValue = default)
    {
        return _blackboard.TryGetValue(key, out var val) && val is T typed ? typed : defaultValue;
    }

    public bool Has(string key) => _blackboard.ContainsKey(key);

    public void Remove(string key) => _blackboard.Remove(key);

    public void ClearBlackboard() => _blackboard.Clear();
}
