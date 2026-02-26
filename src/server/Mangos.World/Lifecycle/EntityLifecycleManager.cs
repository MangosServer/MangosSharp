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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Mangos.World.Lifecycle;

/// <summary>
/// Tracks all active world entities and ensures proper lifecycle management.
/// Guarantees that OnRemovedFromWorld() is always called during cleanup,
/// fixing the incomplete IDisposable patterns throughout the codebase.
///
/// Inspired by Unreal Engine's world subsystem that manages actor lifecycles.
/// </summary>
public sealed class EntityLifecycleManager
{
    private readonly ConcurrentDictionary<ulong, IWorldEntity> _entities = new();
    private readonly List<ulong> _pendingRemoval = new();

    /// <summary>
    /// Registers and activates an entity.
    /// </summary>
    public void Register(ulong guid, IWorldEntity entity)
    {
        if (_entities.TryAdd(guid, entity))
        {
            try
            {
                entity.OnAddedToWorld();
            }
            catch (Exception)
            {
                _entities.TryRemove(guid, out _);
                throw;
            }
        }
    }

    /// <summary>
    /// Removes an entity, guaranteeing OnRemovedFromWorld() is called.
    /// </summary>
    public void Unregister(ulong guid)
    {
        if (_entities.TryRemove(guid, out var entity))
        {
            try
            {
                entity.OnRemovedFromWorld();
            }
            catch (Exception)
            {
                // Swallow cleanup exceptions to prevent cascading failures
            }
        }
    }

    /// <summary>
    /// Updates all active entities.
    /// </summary>
    public void UpdateAll(int diffMs)
    {
        _pendingRemoval.Clear();

        foreach (var kvp in _entities)
        {
            if (kvp.Value.LifecycleState == EntityLifecycleState.Deactivating)
            {
                _pendingRemoval.Add(kvp.Key);
                continue;
            }

            try
            {
                kvp.Value.OnUpdate(diffMs);
            }
            catch (Exception)
            {
                // Log and continue; don't let one entity crash the update loop
            }
        }

        foreach (var guid in _pendingRemoval)
        {
            Unregister(guid);
        }
    }

    /// <summary>
    /// Removes all entities, calling OnRemovedFromWorld() on each.
    /// Used during server shutdown.
    /// </summary>
    public void UnregisterAll()
    {
        foreach (var guid in _entities.Keys)
        {
            Unregister(guid);
        }
    }

    /// <summary>
    /// Gets the count of active entities.
    /// </summary>
    public int ActiveCount => _entities.Count;

    /// <summary>
    /// Returns true if an entity with the given GUID is registered and active.
    /// </summary>
    public bool IsActive(ulong guid)
    {
        return _entities.TryGetValue(guid, out var entity) &&
               entity.LifecycleState == EntityLifecycleState.Active;
    }
}
