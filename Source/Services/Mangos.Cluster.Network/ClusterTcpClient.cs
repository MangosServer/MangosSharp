using Mangos.Cluster.Globals;
using Mangos.Loggers;
using Mangos.Network.Tcp;
using Mangos.Network.Tcp.Extensions;
using System;
using System.Threading;
using System.Threading.Channels;

namespace Mangos.Cluster.Network
{
    public class ClusterTcpClient : ITcpClient
    {
        private readonly ILogger logger;
        private readonly ClientClass clientClass;

        public ClusterTcpClient(ILogger logger, ClientClass clientClass)
        {
            this.logger = logger;
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
                        await reader.ReadToArrayAsync(buffer, 0, 6);
                        if (clientClass.Encryption)
                        {
                            clientClass.Decode(buffer);
                        }
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
    }
}
