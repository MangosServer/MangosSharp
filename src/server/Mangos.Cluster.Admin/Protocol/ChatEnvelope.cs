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
/// Cross-realm chat envelope. Sent on the federation transport
/// (AdminMethodId.ChatRoute). The receiving cluster looks up the
/// destination by (Channel, RecipientName/RecipientGuid) and delivers
/// the message to that player; if they're online and federation
/// markers are enabled for them, the sender's display tag is rendered
/// in front of the sender name per the receiving realm's marker config.
/// </summary>
public sealed class ChatEnvelope
{
    /// <summary>Realm id the message originates from.</summary>
    public required uint SenderRealmId
    {
        get; init;
    }

    /// <summary>Display tag of the sender's realm (e.g. "WM").</summary>
    public required string SenderRealmTag
    {
        get; init;
    }

    /// <summary>Sender's character GUID (home realm).</summary>
    public required ulong SenderGuid
    {
        get; init;
    }

    /// <summary>Sender's character name.</summary>
    public required string SenderName
    {
        get; init;
    }

    /// <summary>Channel: whisper, party, raid, guild, system, etc.</summary>
    public required ChatChannel Channel
    {
        get; init;
    }

    /// <summary>Whisper: target name (host realm of recipient lives in TargetRealmId of the AdminCommand wrapper).</summary>
    public string? RecipientName
    {
        get; init;
    }

    /// <summary>Whisper: target GUID if known. 0 means "look up by name".</summary>
    public ulong RecipientGuid
    {
        get; init;
    }

    /// <summary>Group/raid id when channel is Party/Raid.</summary>
    public long GroupId
    {
        get; init;
    }

    /// <summary>Wire language id (matches WoW's LANG_* constants).</summary>
    public uint Language
    {
        get; init;
    }

    /// <summary>The message body.</summary>
    public required string Body
    {
        get; init;
    }

    public byte[] Serialize()
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(SenderRealmId);
        bw.Write(SenderRealmTag ?? string.Empty);
        bw.Write(SenderGuid);
        bw.Write(SenderName ?? string.Empty);
        bw.Write((byte)Channel);
        bw.Write(RecipientName ?? string.Empty);
        bw.Write(RecipientGuid);
        bw.Write(GroupId);
        bw.Write(Language);
        bw.Write(Body ?? string.Empty);
        return ms.ToArray();
    }

    public static ChatEnvelope Deserialize(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);
        return new ChatEnvelope
        {
            SenderRealmId = br.ReadUInt32(),
            SenderRealmTag = br.ReadString(),
            SenderGuid = br.ReadUInt64(),
            SenderName = br.ReadString(),
            Channel = (ChatChannel)br.ReadByte(),
            RecipientName = br.ReadString() is { Length: > 0 } n ? n : null,
            RecipientGuid = br.ReadUInt64(),
            GroupId = br.ReadInt64(),
            Language = br.ReadUInt32(),
            Body = br.ReadString(),
        };
    }
}

public enum ChatChannel : byte
{
    Whisper = 0,
    Party = 1,
    Raid = 2,
    Guild = 3,
    GuildOfficer = 4,
    System = 5,
    /// <summary>Public custom channels (e.g. /join World).</summary>
    NamedChannel = 6,
}
