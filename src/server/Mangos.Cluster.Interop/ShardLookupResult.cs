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

namespace Mangos.Cluster.Interop;

/// <summary>
/// Result of <see cref="ICluster.QueryShard"/>: tells a world whether the
/// (mapId, characterGuid) pair belongs to a federated shard, and if so
/// whether the local cluster is the host or a foreign cluster owns it.
/// </summary>
public sealed class ShardLookupResult
{
    public required ShardLookupKind Kind { get; init; }

    /// <summary>Owning cluster id when Kind == Foreign; 0 otherwise.</summary>
    public uint OwnerClusterId { get; init; }

    /// <summary>Host:port of the foreign cluster's federation listener; empty when Kind != Foreign.</summary>
    public string OwnerEndpoint { get; init; } = string.Empty;

    /// <summary>Owner's display tag (for player-facing messages).</summary>
    public string OwnerDisplayTag { get; init; } = string.Empty;

    public static readonly ShardLookupResult NoShard = new() { Kind = ShardLookupKind.NoShard };
    public static readonly ShardLookupResult Local = new() { Kind = ShardLookupKind.Local };
}

public enum ShardLookupKind : byte
{
    /// <summary>No federated shard claims this (mapId, characterGuid). Host normally.</summary>
    NoShard = 0,

    /// <summary>This cluster owns the shard. Host normally.</summary>
    Local = 1,

    /// <summary>A foreign cluster owns the shard; this world should not host.</summary>
    Foreign = 2,
}
