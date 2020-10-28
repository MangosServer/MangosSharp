using Mangos.Loggers;
using Mangos.Network.Tcp;
using Mangos.Realm.Models;
using System;
using System.Threading;
using System.Threading.Channels;

namespace Mangos.Realm.Network
{
    public class RealmTcpClient : ITcpClient
    {
        private readonly ILogger logger;
        private readonly ClientModel clientModel;
        private readonly Router router;

        public RealmTcpClient(ILogger logger, Router router, ClientModel clientModel)
        {
            this.logger = logger;
            this.router = router;
            this.clientModel = clientModel;
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
                    var opcode = await reader.ReadAsync(cancellationToken);
                    var packetHandler = router.GetPacketHandler(opcode);
                    await packetHandler.HandleAsync(reader, writer, clientModel);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Packet handler error", ex);
            }
        }
    }
}
