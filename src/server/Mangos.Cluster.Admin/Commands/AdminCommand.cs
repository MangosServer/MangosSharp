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
using System.IO;

namespace Mangos.Cluster.Admin.Commands;

/// <summary>
/// One admin invocation. Constructed by the in-game chat handler, the
/// cluster console REPL, the external CLI tool, or a peer cluster acting
/// on behalf of a remote operator. The dispatcher on the receiving side
/// applies the operation against local state.
///
/// TargetRealmId == 0 means "this cluster"; non-zero routes the command
/// to the peer that owns that realmId in the realmlist.
/// </summary>
public sealed class AdminCommand
{
    /// <summary>Stable verb identifier; see <see cref="AdminVerb"/>.</summary>
    public required AdminVerb Verb
    {
        get; init;
    }

    /// <summary>0 = local cluster; otherwise route to the peer that owns this realm.</summary>
    public uint TargetRealmId
    {
        get; init;
    }

    /// <summary>Optional: target a single world (e.g. .server shutdown --world W).</summary>
    public string? WorldId
    {
        get; init;
    }

    /// <summary>Optional: target a single instance (e.g. .instance restart --instance 1234).</summary>
    public uint InstanceId
    {
        get; init;
    }

    /// <summary>Optional: target a map (e.g. .instance spawn --map 530).</summary>
    public uint MapId
    {
        get; init;
    }

    /// <summary>Optional: graceful drain window before kill, in seconds.</summary>
    public int GraceSeconds
    {
        get; init;
    }

    /// <summary>Optional free-form arguments (key=value), preserved verbatim for the dispatcher.</summary>
    public Dictionary<string, string> Extras { get; init; } = new();

    public byte[] Serialize()
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write((ushort)Verb);
        bw.Write(TargetRealmId);
        bw.Write(WorldId ?? string.Empty);
        bw.Write(InstanceId);
        bw.Write(MapId);
        bw.Write(GraceSeconds);
        bw.Write(Extras.Count);
        foreach (var kv in Extras)
        {
            bw.Write(kv.Key ?? string.Empty);
            bw.Write(kv.Value ?? string.Empty);
        }
        return ms.ToArray();
    }

    public static AdminCommand Deserialize(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);
        var verb = (AdminVerb)br.ReadUInt16();
        var realm = br.ReadUInt32();
        var worldId = br.ReadString();
        var inst = br.ReadUInt32();
        var map = br.ReadUInt32();
        var grace = br.ReadInt32();
        var n = br.ReadInt32();
        var extras = new Dictionary<string, string>(n);
        for (int i = 0; i < n; i++)
        {
            var k = br.ReadString();
            var v = br.ReadString();
            extras[k] = v;
        }
        return new AdminCommand
        {
            Verb = verb,
            TargetRealmId = realm,
            WorldId = string.IsNullOrEmpty(worldId) ? null : worldId,
            InstanceId = inst,
            MapId = map,
            GraceSeconds = grace,
            Extras = extras,
        };
    }
}

/// <summary>Verb space for admin commands. See in-game ".server"/".instance"/".realm" handlers.</summary>
public enum AdminVerb : ushort
{
    Unknown = 0,

    // .server
    ServerList = 0x0001,
    ServerInfo = 0x0002,
    ServerShutdown = 0x0003,
    ServerRestart = 0x0004,
    ServerStart = 0x0005,
    ServerClaimMaps = 0x0006,

    // .instance
    InstanceList = 0x0010,
    InstanceInfo = 0x0011,
    InstanceSpawn = 0x0012,
    InstanceShutdown = 0x0013,
    InstanceRestart = 0x0014,
    InstanceKick = 0x0015,

    // .realm
    RealmList = 0x0020,
    RealmPeers = 0x0021,
    RealmMarkerShow = 0x0022,
    RealmMarkerHide = 0x0023,
}
