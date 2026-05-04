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
/// First frame on a federation connection. Carries the dialer's cluster id,
/// a fresh nonce, and an HMAC over (clusterId || nonce) using a shared
/// secret configured per peer. The receiver verifies the HMAC against its
/// peer table; on success it replies with a PeerHelloAck and the link is
/// authenticated for the lifetime of the connection.
///
/// We don't roll our own crypto: the HMAC uses HMAC-SHA256 from BCL.
/// Anyone with the peer secret can join; rotate the secret to evict.
/// </summary>
public sealed class PeerHello
{
    public required uint ClusterId
    {
        get; init;
    }
    public required byte[] Nonce
    {
        get; init;
    }
    public required byte[] Hmac
    {
        get; init;
    }
    public required string DisplayTag
    {
        get; init;
    }

    public byte[] Serialize()
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(ClusterId);
        bw.Write(Nonce.Length);
        bw.Write(Nonce);
        bw.Write(Hmac.Length);
        bw.Write(Hmac);
        bw.Write(DisplayTag ?? string.Empty);
        return ms.ToArray();
    }

    public static PeerHello Deserialize(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);
        var clusterId = br.ReadUInt32();
        var nonceLen = br.ReadInt32();
        var nonce = br.ReadBytes(nonceLen);
        var hmacLen = br.ReadInt32();
        var hmac = br.ReadBytes(hmacLen);
        var tag = br.ReadString();
        return new PeerHello
        {
            ClusterId = clusterId,
            Nonce = nonce,
            Hmac = hmac,
            DisplayTag = tag,
        };
    }
}

/// <summary>Acknowledgement frame for a successful peer handshake.</summary>
public sealed class PeerHelloAck
{
    public required uint ClusterId
    {
        get; init;
    }
    public required string DisplayTag
    {
        get; init;
    }

    public byte[] Serialize()
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(ClusterId);
        bw.Write(DisplayTag ?? string.Empty);
        return ms.ToArray();
    }

    public static PeerHelloAck Deserialize(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);
        return new PeerHelloAck
        {
            ClusterId = br.ReadUInt32(),
            DisplayTag = br.ReadString(),
        };
    }
}
