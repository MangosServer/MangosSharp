using Mangos.Network.Tcp.Extensions;
using Mangos.Realm.Network.Requests;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Realm.Network.Readers
{
    public class CMD_XFER_RESUME_Reader : IPacketReader<CMD_XFER_RESUME>
    {
        public async ValueTask<CMD_XFER_RESUME> ReadAsync(ChannelReader<byte> reader)
        {
            await reader.ReadVoidAsync(8);
            return new CMD_XFER_RESUME();
        }
    }
}
