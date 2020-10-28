using Mangos.Network.Tcp.Extensions;
using Mangos.Realm.Models;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Realm.Network.Handlers
{
    public class On_CMD_XFER_RESUME_Handler : IPacketHandler
    {
        public async Task HandleAsync(ChannelReader<byte> reader, ChannelWriter<byte> writer, ClientModel clientModel)
        {
            await reader.ReadVoidAsync(8);
        }
    }
}
