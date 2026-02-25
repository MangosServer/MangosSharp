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

using Mangos.Cluster.Globals;
using Mangos.Cluster.Network;
using Mangos.Tcp;
using System.Net.Sockets;

namespace WorldCluster.Network;

internal sealed class ClusterTcpConnection : ITcpConnection
{
    private readonly ClientClass legacyClientClass;

    public ClusterTcpConnection(ClientClass legacyClientClass)
    {
        this.legacyClientClass = legacyClientClass;
    }

    public async Task ExecuteAsync(Socket socket, CancellationToken cancellationToken)
    {
        legacyClientClass.Socket = socket;
        await legacyClientClass.OnConnectAsync();

        while (!cancellationToken.IsCancellationRequested)
        {
            await ExecuteLegacyMessageAsync(socket, cancellationToken);
        }
    }

    private async Task ExecuteLegacyMessageAsync(Socket socket, CancellationToken cancellationToken)
    {
        var header = new byte[6];
        await ReadAsync(socket, header, cancellationToken);

        if (legacyClientClass.Client.PacketEncryption.IsEncryptionEnabled)
        {
            DecodePacketHeader(header);
        }

        var length = header[1] + (header[0] * 256) + 2;
        var body = new byte[length - 6];
        await ReadAsync(socket, body, cancellationToken);

        var packet = new PacketClass(header.Concat(body).ToArray());
        legacyClientClass.OnPacket(packet);
    }

    private void DecodePacketHeader(Span<byte> data)
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

    private static async ValueTask ReadAsync(Socket socket, byte[] buffer, CancellationToken cancellationToken)
    {
        var offset = 0;
        while (offset < buffer.Length)
        {
            var received = await socket.ReceiveAsync(buffer.AsMemory(offset), SocketFlags.None, cancellationToken);
            if (received == 0)
            {
                throw new IOException("Connection closed by client");
            }
            offset += received;
        }
    }
}
