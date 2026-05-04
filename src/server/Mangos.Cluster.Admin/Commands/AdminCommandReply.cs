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
/// Response to an <see cref="AdminCommand"/>. Carries a status flag and
/// a list of human-readable text lines that the requester echoes to the
/// operator's chat / console / stdout.
/// </summary>
public sealed class AdminCommandReply
{
    public required AdminReplyStatus Status
    {
        get; init;
    }
    public List<string> Lines { get; init; } = new();

    public byte[] Serialize()
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write((byte)Status);
        bw.Write(Lines.Count);
        foreach (var l in Lines)
            bw.Write(l ?? string.Empty);
        return ms.ToArray();
    }

    public static AdminCommandReply Deserialize(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);
        var status = (AdminReplyStatus)br.ReadByte();
        var n = br.ReadInt32();
        var lines = new List<string>(n);
        for (int i = 0; i < n; i++)
            lines.Add(br.ReadString());
        return new AdminCommandReply { Status = status, Lines = lines };
    }
}

public enum AdminReplyStatus : byte
{
    Ok = 0,
    NotFound = 1,
    NotPermitted = 2,
    InvalidArguments = 3,
    Unreachable = 4,
    Failed = 5,
}
