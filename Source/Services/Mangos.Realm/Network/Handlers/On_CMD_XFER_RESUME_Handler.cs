using Mangos.Network.Tcp.Extensions;
using Mangos.Realm.Models;
using Mangos.Realm.Network.Readers;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Realm.Network.Handlers
{
    public class On_CMD_XFER_RESUME_Handler : IPacketHandler
    {
        private readonly CMD_XFER_RESUME_Reader CMD_XFER_RESUME_Reader;

        public On_CMD_XFER_RESUME_Handler(CMD_XFER_RESUME_Reader CMD_XFER_RESUME_Reader)
        {
            this.CMD_XFER_RESUME_Reader = CMD_XFER_RESUME_Reader;
        }

        public async Task HandleAsync(ChannelReader<byte> reader, ChannelWriter<byte> writer, ClientModel clientModel)
        {
            await CMD_XFER_RESUME_Reader.ReadAsync(reader);
        }
    }
}
