using Mangos.Network.Tcp;
using Mangos.Realm.Models;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Realm.Network
{
    public class RealmTcpClient : ITcpClient
    {
        private readonly ClientModel clientModel;
        private readonly Router router;

        public RealmTcpClient(
            ClientModel clientModel, 
            Router router)
        {
            this.clientModel = clientModel;
            this.router = router;
        }

        public async void HandleAsync(
            ChannelReader<byte> reader, 
            ChannelWriter<byte> writer, 
            CancellationToken cancellationToken)
        {
            while(!cancellationToken.IsCancellationRequested)
            {
                var opcode = await reader.ReadAsync(cancellationToken);
                await HandleAsync(opcode, reader, writer);
            }
        }

        private async Task HandleAsync(
            byte opcode, 
            ChannelReader<byte> reader,
            ChannelWriter<byte> writer)
        {
            var packetHandler = router.GetPacketHandler(opcode);
            await packetHandler.HandleAsync(reader, writer, clientModel);
        }
    }
}
