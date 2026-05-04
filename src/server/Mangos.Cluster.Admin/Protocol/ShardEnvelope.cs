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

using System.IO;

namespace Mangos.Cluster.Admin.Protocol;

/// <summary>
/// Phase B shard claim. Sent by a leader's cluster to a peer when a
/// federated group enters a shardable zone, so the peer routes its
/// member's world packets through the leader's cluster's world for the
/// duration. Receipt is best-effort; the peer may decline if it cannot
/// reach the host shard endpoint.
/// </summary>
public sealed class ShardClaimEnvelope
{
    /// <summary>Owning cluster's group id (matches federation_group.groupId).</summary>
    public required long GroupId
    {
        get; init;
    }

    /// <summary>Cluster id that owns the shard (= the host).</summary>
    public required uint OwnerClusterId
    {
        get; init;
    }

    /// <summary>WoW map id this shard covers.</summary>
    public required uint MapId
    {
        get; init;
    }

    /// <summary>Stable shard key; clients with the same key end up co-located.</summary>
    public required ulong ShardKey
    {
        get; init;
    }

    /// <summary>Where to forward foreign-member world packets (host:port of host cluster's relay).</summary>
    public required string RelayEndpoint
    {
        get; init;
    }

    public byte[] Serialize()
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(GroupId);
        bw.Write(OwnerClusterId);
        bw.Write(MapId);
        bw.Write(ShardKey);
        bw.Write(RelayEndpoint ?? string.Empty);
        return ms.ToArray();
    }

    public static ShardClaimEnvelope Deserialize(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);
        return new ShardClaimEnvelope
        {
            GroupId = br.ReadInt64(),
            OwnerClusterId = br.ReadUInt32(),
            MapId = br.ReadUInt32(),
            ShardKey = br.ReadUInt64(),
            RelayEndpoint = br.ReadString(),
        };
    }
}

/// <summary>Counterpart to ShardClaimEnvelope; releases the shard when the group disbands or the leader logs off.</summary>
public sealed class ShardReleaseEnvelope
{
    public required long GroupId
    {
        get; init;
    }
    public required ulong ShardKey
    {
        get; init;
    }

    public byte[] Serialize()
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(GroupId);
        bw.Write(ShardKey);
        return ms.ToArray();
    }

    public static ShardReleaseEnvelope Deserialize(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);
        return new ShardReleaseEnvelope
        {
            GroupId = br.ReadInt64(),
            ShardKey = br.ReadUInt64(),
        };
    }
}
