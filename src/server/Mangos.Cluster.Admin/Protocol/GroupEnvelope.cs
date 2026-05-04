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

namespace Mangos.Cluster.Admin.Protocol;

/// <summary>
/// Cross-realm group invite envelope (AdminMethodId.GroupInvite).
/// The leader's cluster sends this to the recipient's cluster; the
/// recipient's cluster surfaces a standard group-invite popup to the
/// targeted character if they're online.
/// </summary>
public sealed class GroupInviteEnvelope
{
    public required long GroupId
    {
        get; init;
    }
    public required uint LeaderRealmId
    {
        get; init;
    }
    public required ulong LeaderGuid
    {
        get; init;
    }
    public required string LeaderName
    {
        get; init;
    }
    public required string LeaderRealmTag
    {
        get; init;
    }
    public required string TargetName
    {
        get; init;
    }
    public byte GroupType
    {
        get; init;
    } // 0 party, 1 raid

    public byte[] Serialize()
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(GroupId);
        bw.Write(LeaderRealmId);
        bw.Write(LeaderGuid);
        bw.Write(LeaderName ?? string.Empty);
        bw.Write(LeaderRealmTag ?? string.Empty);
        bw.Write(TargetName ?? string.Empty);
        bw.Write(GroupType);
        return ms.ToArray();
    }

    public static GroupInviteEnvelope Deserialize(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);
        return new GroupInviteEnvelope
        {
            GroupId = br.ReadInt64(),
            LeaderRealmId = br.ReadUInt32(),
            LeaderGuid = br.ReadUInt64(),
            LeaderName = br.ReadString(),
            LeaderRealmTag = br.ReadString(),
            TargetName = br.ReadString(),
            GroupType = br.ReadByte(),
        };
    }
}

/// <summary>
/// Reply to a GroupInviteEnvelope. Travels along the same federation
/// link in the opposite direction.
/// </summary>
public sealed class GroupInviteResponseEnvelope
{
    public required long GroupId
    {
        get; init;
    }
    public required uint TargetRealmId
    {
        get; init;
    }
    public required ulong TargetGuid
    {
        get; init;
    }
    public required string TargetName
    {
        get; init;
    }
    public required GroupInviteResponse Decision
    {
        get; init;
    }

    public byte[] Serialize()
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(GroupId);
        bw.Write(TargetRealmId);
        bw.Write(TargetGuid);
        bw.Write(TargetName ?? string.Empty);
        bw.Write((byte)Decision);
        return ms.ToArray();
    }

    public static GroupInviteResponseEnvelope Deserialize(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);
        return new GroupInviteResponseEnvelope
        {
            GroupId = br.ReadInt64(),
            TargetRealmId = br.ReadUInt32(),
            TargetGuid = br.ReadUInt64(),
            TargetName = br.ReadString(),
            Decision = (GroupInviteResponse)br.ReadByte(),
        };
    }
}

public enum GroupInviteResponse : byte
{
    Accepted = 0,
    Declined = 1,
    AlreadyInGroup = 2,
    NotFound = 3,
    Timeout = 4,
}

/// <summary>
/// Authoritative roster snapshot for a federated group. Sent from the
/// leader's cluster to every peer that owns at least one member, on
/// every roster change. Replicas overwrite their local copy.
/// </summary>
public sealed class GroupRosterUpdateEnvelope
{
    public required long GroupId
    {
        get; init;
    }
    public required uint LeaderRealmId
    {
        get; init;
    }
    public required ulong LeaderGuid
    {
        get; init;
    }
    public byte GroupType
    {
        get; init;
    }
    public List<GroupMemberEntry> Members { get; init; } = new();

    public byte[] Serialize()
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(GroupId);
        bw.Write(LeaderRealmId);
        bw.Write(LeaderGuid);
        bw.Write(GroupType);
        bw.Write(Members.Count);
        foreach (var m in Members)
        {
            bw.Write(m.RealmId);
            bw.Write(m.Guid);
            bw.Write(m.Name ?? string.Empty);
            bw.Write(m.Role);
        }
        return ms.ToArray();
    }

    public static GroupRosterUpdateEnvelope Deserialize(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);
        var env = new GroupRosterUpdateEnvelope
        {
            GroupId = br.ReadInt64(),
            LeaderRealmId = br.ReadUInt32(),
            LeaderGuid = br.ReadUInt64(),
            GroupType = br.ReadByte(),
        };
        var n = br.ReadInt32();
        for (int i = 0; i < n; i++)
        {
            env.Members.Add(new GroupMemberEntry
            {
                RealmId = br.ReadUInt32(),
                Guid = br.ReadUInt64(),
                Name = br.ReadString(),
                Role = br.ReadByte(),
            });
        }
        return env;
    }
}

public sealed class GroupMemberEntry
{
    public required uint RealmId
    {
        get; init;
    }
    public required ulong Guid
    {
        get; init;
    }
    public required string Name
    {
        get; init;
    }
    /// <summary>Bitfield: 1=leader, 2=assist, 4=mainTank, 8=mainAssist.</summary>
    public byte Role
    {
        get; init;
    }
}
