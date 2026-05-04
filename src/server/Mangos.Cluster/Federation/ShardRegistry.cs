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

using System.Collections.Concurrent;
using System.Collections.Generic;
using Mangos.Cluster.Admin.Protocol;
using Mangos.Logging;

namespace Mangos.Cluster.Federation;

/// <summary>
/// Phase B foundation: in-memory registry of active shards. Each entry
/// records "for this (mapId, shardKey) the host cluster is N at relay
/// endpoint E". When a player from a federated group enters mapId, the
/// world looks up its shardKey here and decides whether to host locally
/// or proxy to the recorded relay endpoint.
///
/// The actual world-packet proxying is intentionally not built yet -
/// this registry is the first half. Once the world's enter-zone path
/// consults <see cref="GetShard"/> we can add the proxy fan-out.
/// </summary>
public sealed class ShardRegistry
{
    private readonly IMangosLogger _logger;
    // Key is (mapId, shardKey).
    private readonly ConcurrentDictionary<(uint MapId, ulong ShardKey), ShardEntry> _shards = new();

    public ShardRegistry(IMangosLogger logger)
    {
        _logger = logger;
    }

    public sealed class ShardEntry
    {
        public required long GroupId { get; init; }
        public required uint OwnerClusterId { get; init; }
        public required uint MapId { get; init; }
        public required ulong ShardKey { get; init; }
        public required string RelayEndpoint { get; init; }
    }

    /// <summary>Look up the shard for the given (mapId, shardKey). Returns null if unknown.</summary>
    public ShardEntry? GetShard(uint mapId, ulong shardKey)
        => _shards.TryGetValue((mapId, shardKey), out var s) ? s : null;

    /// <summary>All currently-known shards, for diagnostics and admin reporting.</summary>
    public IEnumerable<ShardEntry> All() => _shards.Values;

    /// <summary>Bind onto a router so inbound shard claim/release envelopes update the registry.</summary>
    public void WireUp(FederationRouter router)
    {
        router.OnShardClaim = e => Apply(e);
        router.OnShardRelease = e => Release(e.ShardKey, e.GroupId);
    }

    private void Apply(ShardClaimEnvelope e)
    {
        var entry = new ShardEntry
        {
            GroupId = e.GroupId,
            OwnerClusterId = e.OwnerClusterId,
            MapId = e.MapId,
            ShardKey = e.ShardKey,
            RelayEndpoint = e.RelayEndpoint,
        };
        _shards[(e.MapId, e.ShardKey)] = entry;
        _logger.Information($"Shard claim: group {e.GroupId} -> map {e.MapId} key {e.ShardKey} via {e.RelayEndpoint}");
    }

    private void Release(ulong shardKey, long groupId)
    {
        // ShardReleaseEnvelope doesn't carry mapId today; sweep by key+group.
        foreach (var kv in _shards)
        {
            if (kv.Key.ShardKey == shardKey && kv.Value.GroupId == groupId)
            {
                _shards.TryRemove(kv.Key, out _);
                _logger.Information($"Shard release: group {groupId} key {shardKey}");
            }
        }
    }
}
