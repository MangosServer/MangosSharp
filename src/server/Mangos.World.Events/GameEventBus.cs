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
using System.Threading;
using System.Threading.Tasks;

namespace Mangos.World.Events;

/// <summary>
/// Thread-safe event bus implementation using copy-on-write handler lists.
/// Inspired by Unity's UnityEvent system and Unreal Engine's multicast delegates.
/// </summary>
public sealed class GameEventBus : IGameEventBus
{
    private readonly ConcurrentDictionary<Type, object> _syncHandlers = new();
    private readonly ConcurrentDictionary<Type, object> _asyncHandlers = new();
    private readonly object _lock = new();

    public void Publish<TEvent>(TEvent gameEvent) where TEvent : IGameEvent
    {
        if (gameEvent is null) return;

        if (_syncHandlers.TryGetValue(typeof(TEvent), out var handlersObj))
        {
            var handlers = (IReadOnlyList<Action<TEvent>>)handlersObj;
            foreach (var handler in handlers)
            {
                try
                {
                    handler(gameEvent);
                }
                catch (Exception)
                {
                    // Swallow handler exceptions to prevent one bad subscriber from breaking others.
                    // In a production system you'd log this.
                }
            }
        }

        if (_asyncHandlers.TryGetValue(typeof(TEvent), out var asyncHandlersObj))
        {
            var asyncHandlers = (IReadOnlyList<Func<TEvent, CancellationToken, Task>>)asyncHandlersObj;
            foreach (var handler in asyncHandlers)
            {
                try
                {
                    handler(gameEvent, CancellationToken.None).GetAwaiter().GetResult();
                }
                catch (Exception)
                {
                }
            }
        }
    }

    public async Task PublishAsync<TEvent>(TEvent gameEvent, CancellationToken cancellationToken = default) where TEvent : IGameEvent
    {
        if (gameEvent is null) return;

        if (_syncHandlers.TryGetValue(typeof(TEvent), out var handlersObj))
        {
            var handlers = (IReadOnlyList<Action<TEvent>>)handlersObj;
            foreach (var handler in handlers)
            {
                try
                {
                    handler(gameEvent);
                }
                catch (Exception)
                {
                }
            }
        }

        if (_asyncHandlers.TryGetValue(typeof(TEvent), out var asyncHandlersObj))
        {
            var asyncHandlers = (IReadOnlyList<Func<TEvent, CancellationToken, Task>>)asyncHandlersObj;
            foreach (var handler in asyncHandlers)
            {
                try
                {
                    await handler(gameEvent, cancellationToken);
                }
                catch (Exception)
                {
                }
            }
        }
    }

    public IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IGameEvent
    {
        if (handler is null) throw new ArgumentNullException(nameof(handler));

        lock (_lock)
        {
            var list = GetOrCreateSyncList<TEvent>();
            var newList = new List<Action<TEvent>>(list) { handler };
            _syncHandlers[typeof(TEvent)] = newList.AsReadOnly();
        }

        return new Subscription(() =>
        {
            lock (_lock)
            {
                if (_syncHandlers.TryGetValue(typeof(TEvent), out var handlersObj))
                {
                    var list = (IReadOnlyList<Action<TEvent>>)handlersObj;
                    var newList = new List<Action<TEvent>>(list);
                    newList.Remove(handler);
                    _syncHandlers[typeof(TEvent)] = newList.AsReadOnly();
                }
            }
        });
    }

    public IDisposable Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler) where TEvent : IGameEvent
    {
        if (handler is null) throw new ArgumentNullException(nameof(handler));

        lock (_lock)
        {
            var list = GetOrCreateAsyncList<TEvent>();
            var newList = new List<Func<TEvent, CancellationToken, Task>>(list) { handler };
            _asyncHandlers[typeof(TEvent)] = newList.AsReadOnly();
        }

        return new Subscription(() =>
        {
            lock (_lock)
            {
                if (_asyncHandlers.TryGetValue(typeof(TEvent), out var handlersObj))
                {
                    var list = (IReadOnlyList<Func<TEvent, CancellationToken, Task>>)handlersObj;
                    var newList = new List<Func<TEvent, CancellationToken, Task>>(list);
                    newList.Remove(handler);
                    _asyncHandlers[typeof(TEvent)] = newList.AsReadOnly();
                }
            }
        });
    }

    private IReadOnlyList<Action<TEvent>> GetOrCreateSyncList<TEvent>() where TEvent : IGameEvent
    {
        if (_syncHandlers.TryGetValue(typeof(TEvent), out var existing))
        {
            return (IReadOnlyList<Action<TEvent>>)existing;
        }
        return Array.Empty<Action<TEvent>>();
    }

    private IReadOnlyList<Func<TEvent, CancellationToken, Task>> GetOrCreateAsyncList<TEvent>() where TEvent : IGameEvent
    {
        if (_asyncHandlers.TryGetValue(typeof(TEvent), out var existing))
        {
            return (IReadOnlyList<Func<TEvent, CancellationToken, Task>>)existing;
        }
        return Array.Empty<Func<TEvent, CancellationToken, Task>>();
    }

    private sealed class Subscription : IDisposable
    {
        private Action _unsubscribe;

        public Subscription(Action unsubscribe)
        {
            _unsubscribe = unsubscribe;
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _unsubscribe, null)?.Invoke();
        }
    }
}
