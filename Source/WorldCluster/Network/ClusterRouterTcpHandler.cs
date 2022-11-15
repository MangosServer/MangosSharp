//
// Copyright (C) 2013-2022 getMaNGOS <https://getmangos.eu>
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

using Mangos.Cluster.Globals;
using Mangos.Cluster.Network;
using Mangos.Tcp;
using System.Net;
using System.Net.Sockets;

namespace WorldCluster.Network;

internal sealed class ClusterRouterTcpHandler : ITcpClientHandler
{
    private readonly ClientClass legacyClientClass;

    public ClusterRouterTcpHandler(ClientClass legacyClientClass)
    {
        this.legacyClientClass = legacyClientClass;
    }

    public async Task ExectueAsync(
        ITcpReader reader,
        ITcpWriter writer,
        IPAddress remoteAddress,
        CancellationToken cancellationToken,
        Socket socket)
    {
        legacyClientClass.Socket = socket;
        await legacyClientClass.OnConnectAsync();

        while (!cancellationToken.IsCancellationRequested)
        {
            await ExectueMessageAsync(reader, writer);
        }
    }

    public async Task ExectueMessageAsync(ITcpReader reader, ITcpWriter writer)
    {
        await ExectueLegacyMessageAsync(reader);
    }

    private async Task ExectueLegacyMessageAsync(ITcpReader reader)
    {
        var header = await reader.ReadByteArrayAsync(6);
        if (legacyClientClass.Client.PacketEncryption.IsEncryptionEnabled)
        {
            DecodePacketHeader(header);
        }
        var length = header[1] + (header[0] * 256) + 2;
        var body = await reader.ReadByteArrayAsync(length - 6);

        var packet = new PacketClass(header.Concat(body).ToArray());
        legacyClientClass.OnPacket(packet);
    }

    private void DecodePacketHeader(byte[] data)
    {
        var key = legacyClientClass.Client.PacketEncryption.Key;
        var hash = legacyClientClass.Client.PacketEncryption.Hash;

        for (var i = 0; i < 6; i++)
        {
            var tmp = data[i];
            data[i] = (byte)(hash[key[1]] ^ ((256 + data[i] - key[0]) % 256));
            key[0] = tmp;
            key[1] = (byte)((key[1] + 1) % 40);
        }
    }
}
