using Mangos.Realm.Models;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Realm.Network.Handlers
{
    public class CMD_XFER_ACCEPT_Handler : IPacketHandler
    {
        public Task HandleAsync(ChannelReader<byte> reader, ChannelWriter<byte> writer, ClientModel clientModel)
        {
            // TODO: data parameter is never used		
            return Task.CompletedTask;
        }
    }
}
