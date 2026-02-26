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

using System.Collections.Generic;
using System.Linq;

namespace Mangos.World.AI;

/// <summary>
/// Centralized cooldown tracking for creature abilities and spells.
/// Replaces the manual "NextX -= AI_UPDATE; if (NextX &lt;= 0)" pattern
/// found in every boss script (Onyxia: 7 fields, Gluth: 5 fields).
///
/// Inspired by Unreal Engine's Gameplay Ability System (GAS) cooldown management
/// and TrinityCore's spell cooldown tracking.
/// </summary>
public sealed class CooldownManager
{
    private readonly struct CooldownEntry
    {
        public int RemainingMs { get; init; }
        public int DurationMs { get; init; }
        public string Category { get; init; }
    }

    private readonly Dictionary<int, CooldownEntry> _cooldowns = new();

    /// <summary>
    /// Starts a cooldown for the given ID (typically a spell ID).
    /// </summary>
    public void StartCooldown(int id, int durationMs, string category = null)
    {
        _cooldowns[id] = new CooldownEntry
        {
            RemainingMs = durationMs,
            DurationMs = durationMs,
            Category = category
        };
    }

    /// <summary>
    /// Returns true if the cooldown for the given ID has expired or was never started.
    /// </summary>
    public bool IsReady(int id)
    {
        return !_cooldowns.TryGetValue(id, out var entry) || entry.RemainingMs <= 0;
    }

    /// <summary>
    /// If the cooldown is ready, starts it and returns true. Otherwise returns false.
    /// This replaces the common "if (NextSpell &lt;= 0) { NextSpell = COOLDOWN; Cast(); }" pattern.
    /// </summary>
    public bool TryUse(int id, int durationMs, string category = null)
    {
        if (!IsReady(id)) return false;
        StartCooldown(id, durationMs, category);
        return true;
    }

    /// <summary>
    /// Gets the remaining cooldown time in milliseconds. Returns 0 if ready.
    /// </summary>
    public int GetRemaining(int id)
    {
        return _cooldowns.TryGetValue(id, out var entry) ? (entry.RemainingMs > 0 ? entry.RemainingMs : 0) : 0;
    }

    /// <summary>
    /// Called each AI tick to decrement all cooldowns by the elapsed time.
    /// </summary>
    public void Update(int elapsedMs)
    {
        var keys = _cooldowns.Keys.ToList();
        foreach (var key in keys)
        {
            var entry = _cooldowns[key];
            if (entry.RemainingMs > 0)
            {
                _cooldowns[key] = entry with { RemainingMs = entry.RemainingMs - elapsedMs };
            }
        }
    }

    /// <summary>
    /// Resets all cooldowns (e.g., on combat reset / evade).
    /// </summary>
    public void ResetAll()
    {
        _cooldowns.Clear();
    }

    /// <summary>
    /// Resets all cooldowns in a specific category.
    /// </summary>
    public void ResetCategory(string category)
    {
        var toRemove = _cooldowns
            .Where(kvp => kvp.Value.Category == category)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in toRemove)
        {
            _cooldowns.Remove(key);
        }
    }

    /// <summary>
    /// Forces a specific cooldown to be ready immediately.
    /// </summary>
    public void ResetCooldown(int id)
    {
        _cooldowns.Remove(id);
    }

    /// <summary>
    /// Returns true if any cooldown with the given category is active.
    /// </summary>
    public bool HasActiveCooldown(string category)
    {
        return _cooldowns.Values.Any(e => e.Category == category && e.RemainingMs > 0);
    }
}
