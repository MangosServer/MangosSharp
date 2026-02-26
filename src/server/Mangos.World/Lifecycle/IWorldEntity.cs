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

namespace Mangos.World.Lifecycle;

/// <summary>
/// Interface for game entities with structured lifecycle management.
/// Inspired by Unreal Engine's AActor lifecycle (BeginPlay/Tick/EndPlay).
///
/// Implementing this interface guarantees that OnRemovedFromWorld() will be called
/// during cleanup, fixing the 8+ incomplete IDisposable patterns in the codebase.
/// </summary>
public interface IWorldEntity
{
    EntityLifecycleState LifecycleState { get; }

    /// <summary>
    /// Called when the entity is spawned into the world. Equivalent to UE4's BeginPlay.
    /// </summary>
    void OnAddedToWorld();

    /// <summary>
    /// Called each tick while the entity is active. Equivalent to UE4's Tick.
    /// </summary>
    void OnUpdate(int diffMs);

    /// <summary>
    /// Called when the entity is removed from the world. Equivalent to UE4's EndPlay.
    /// Guaranteed to be called by the EntityLifecycleManager, fixing Dispose leaks.
    /// </summary>
    void OnRemovedFromWorld();
}
