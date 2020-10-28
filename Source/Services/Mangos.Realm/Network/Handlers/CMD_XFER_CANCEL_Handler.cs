using Mangos.Realm.Models;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Realm.Network.Handlers
{
    public class CMD_XFER_CANCEL_Handler : IPacketHandler
    {
        public Task HandleAsync(ChannelReader<byte> reader, ChannelWriter<byte> writer, ClientModel clientModel)
        {
            // TODO: data parameter is never used
            // logger.Debug("[{0}:{1}] CMD_XFER_CANCEL", Ip, Port);
            // Socket.Close();
            return Task.CompletedTask;
        }
    }
}
