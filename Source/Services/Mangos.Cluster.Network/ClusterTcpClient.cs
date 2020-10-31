using Mangos.Cluster.Globals;
using Mangos.Loggers;
using Mangos.Network.Tcp;
using Mangos.Network.Tcp.Extensions;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Cluster.Network
{
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
                        var length = buffer[1] + buffer[0] * 256 + 2;
                        await reader.ReadToArrayAsync(buffer, 6, length - 6);

                        var packet = new PacketClass(buffer);
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
                data[i] = (byte)(client.PacketEncryption.Hash[key[1]] ^ (256 + data[i] - key[0]) % 256);
                key[0] = tmp;
                key[1] = (byte)((key[1] + 1) % 40);
            }
        }
    }
}
