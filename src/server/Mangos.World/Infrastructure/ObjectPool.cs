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

namespace Mangos.World.Infrastructure;

/// <summary>
/// Generic thread-safe object pool for reducing allocation pressure.
/// Inspired by Unity's ObjectPool&lt;T&gt; and .NET's ArrayPool.
///
/// Primary use case: pooling frequently allocated objects like PacketClass,
/// byte arrays, and timer callbacks that are created/destroyed every tick.
/// </summary>
public sealed class ObjectPool<T> where T : class
{
    private readonly ConcurrentBag<T> _pool = new();
    private readonly Func<T> _factory;
    private readonly Action<T> _reset;
    private readonly int _maxSize;
    private int _count;

    /// <summary>
    /// Creates a new object pool.
    /// </summary>
    /// <param name="factory">Factory function to create new instances when the pool is empty.</param>
    /// <param name="reset">Optional reset function called when an object is returned to the pool.</param>
    /// <param name="maxSize">Maximum number of objects to keep in the pool. Excess objects are discarded.</param>
    public ObjectPool(Func<T> factory, Action<T> reset = null, int maxSize = 256)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _reset = reset;
        _maxSize = maxSize;
    }

    /// <summary>
    /// Gets the current number of objects in the pool.
    /// </summary>
    public int Count => _count;

    /// <summary>
    /// Rents an object from the pool, or creates a new one if the pool is empty.
    /// </summary>
    public T Rent()
    {
        if (_pool.TryTake(out var item))
        {
            System.Threading.Interlocked.Decrement(ref _count);
            return item;
        }
        return _factory();
    }

    /// <summary>
    /// Returns an object to the pool. The reset function is called before storing.
    /// If the pool is at max capacity, the object is discarded.
    /// </summary>
    public void Return(T item)
    {
        if (item is null) return;

        if (_count >= _maxSize)
        {
            // Pool is full, let GC handle it
            if (item is IDisposable disposable)
            {
                disposable.Dispose();
            }
            return;
        }

        _reset?.Invoke(item);
        _pool.Add(item);
        System.Threading.Interlocked.Increment(ref _count);
    }

    /// <summary>
    /// Clears all objects from the pool, disposing them if they implement IDisposable.
    /// </summary>
    public void Clear()
    {
        while (_pool.TryTake(out var item))
        {
            if (item is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        _count = 0;
    }
}

/// <summary>
/// RAII wrapper that returns an object to the pool when disposed.
/// Usage: using var pooled = pool.RentScoped(); var obj = pooled.Value;
/// </summary>
public readonly struct PooledObject<T> : IDisposable where T : class
{
    private readonly ObjectPool<T> _pool;

    public T Value { get; }

    public PooledObject(ObjectPool<T> pool, T value)
    {
        _pool = pool;
        Value = value;
    }

    public void Dispose()
    {
        _pool?.Return(Value);
    }
}

/// <summary>
/// Extension methods for ObjectPool.
/// </summary>
public static class ObjectPoolExtensions
{
    /// <summary>
    /// Rents an object and returns a disposable wrapper that returns it to the pool.
    /// </summary>
    public static PooledObject<T> RentScoped<T>(this ObjectPool<T> pool) where T : class
    {
        return new PooledObject<T>(pool, pool.Rent());
    }
}
