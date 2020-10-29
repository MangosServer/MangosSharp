using Mangos.Network.Tcp.Extensions;
using Mangos.Realm.Network.Requests;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Realm.Network.Readers
{
    public class RS_REALMLIST_Reader : IPacketReader<RS_REALMLIST>
    {
        public async ValueTask<RS_REALMLIST> ReadAsync(ChannelReader<byte> reader)
        {
            var unk = await reader.ReadArrayAsync(4);
            return new RS_REALMLIST(unk);
        }
    }
}
