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
using System.Threading;
using System.Threading.Tasks;

namespace Mangos.World.Events;

/// <summary>
/// Central event bus for publishing and subscribing to game events.
/// Provides decoupled communication between game systems (AI, combat, spells, guilds, etc.).
/// Inspired by the Observer pattern used in Unity and Unreal Engine event dispatchers.
/// </summary>
public interface IGameEventBus
{
    /// <summary>
    /// Publishes an event synchronously to all subscribers.
    /// </summary>
    void Publish<TEvent>(TEvent gameEvent) where TEvent : IGameEvent;

    /// <summary>
    /// Publishes an event asynchronously to all async subscribers.
    /// </summary>
    Task PublishAsync<TEvent>(TEvent gameEvent, CancellationToken cancellationToken = default) where TEvent : IGameEvent;

    /// <summary>
    /// Subscribes a synchronous handler. Returns a disposable that unsubscribes when disposed.
    /// </summary>
    IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IGameEvent;

    /// <summary>
    /// Subscribes an asynchronous handler. Returns a disposable that unsubscribes when disposed.
    /// </summary>
    IDisposable Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler) where TEvent : IGameEvent;
}
