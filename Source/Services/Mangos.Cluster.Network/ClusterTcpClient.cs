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
using Mangos.Loggers;
using Mangos.Network.Tcp;
using Mangos.Network.Tcp.Extensions;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Cluster.Network;

public class ClusterTcpClient : ITcpClient
{
    private readonly ILogger logger;
    private readonly ClientClass clientClass;

    private readonly Client client;

    public ClusterTcpClient(ILogger logger, Client client, ClientClass clientClass)
    {
        this.logger = logger;
        this.client = client;
        this.clientClass = clientClass;
    }

    public async void HandleAsync(
        ChannelReader<byte> reader,
        ChannelWriter<byte> writer,
        CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var buffer = new byte[8192];
                while (!cancellationToken.IsCancellationRequested)
                {
                    await ReadEncodedPacketHeaderToBufferAsync(reader, buffer);
                    var length = buffer[1] + (buffer[0] * 256) + 2;
                    await reader.ReadToArrayAsync(buffer, 6, length - 6);

                    PacketClass packet = new(buffer);
                    clientClass.OnPacket(packet);
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error("Packet handler error", ex);
        }
    }

    private async ValueTask ReadEncodedPacketHeaderToBufferAsync(ChannelReader<byte> reader, byte[] buffer)
    {
        await reader.ReadToArrayAsync(buffer, 0, 6);
        if (client.PacketEncryption.IsEncryptionEnabled)
        {
            DecodePacketHeader(buffer);
        }
    }

    public void DecodePacketHeader(byte[] data)
    {
        var key = client.PacketEncryption.Key;

        for (var i = 0; i < 6; i++)
        {
            var tmp = data[i];
            data[i] = (byte)(client.PacketEncryption.Hash[key[1]] ^ ((256 + data[i] - key[0]) % 256));
            key[0] = tmp;
            key[1] = (byte)((key[1] + 1) % 40);
        }
    }
}
