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
/// "Is this character online on your realm?" query, used during
/// cross-realm whispers and group invites when the sender only knows
/// the recipient's name.
/// </summary>
public sealed class PresenceQueryEnvelope
{
    public required string Name
    {
        get; init;
    }

    public byte[] Serialize()
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(Name ?? string.Empty);
        return ms.ToArray();
    }

    public static PresenceQueryEnvelope Deserialize(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);
        return new PresenceQueryEnvelope { Name = br.ReadString() };
    }
}

public sealed class PresenceReplyEnvelope
{
    public required string Name
    {
        get; init;
    }
    public required bool Online
    {
        get; init;
    }
    public ulong Guid
    {
        get; init;
    }
    public uint MapId
    {
        get; init;
    }
    public uint ZoneId
    {
        get; init;
    }

    public byte[] Serialize()
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(Name ?? string.Empty);
        bw.Write(Online);
        bw.Write(Guid);
        bw.Write(MapId);
        bw.Write(ZoneId);
        return ms.ToArray();
    }

    public static PresenceReplyEnvelope Deserialize(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);
        return new PresenceReplyEnvelope
        {
            Name = br.ReadString(),
            Online = br.ReadBoolean(),
            Guid = br.ReadUInt64(),
            MapId = br.ReadUInt32(),
            ZoneId = br.ReadUInt32(),
        };
    }
}
