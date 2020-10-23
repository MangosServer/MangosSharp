using Mangos.Network.Tcp.Mvc;
using Mangos.Realm.Network.Requests;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Realm.Network.Readers
{
    public class RS_LOGON_CHALLENGE_Reader : IPacketReader<RS_LOGON_CHALLENGE>
    {
        public async ValueTask<RS_LOGON_CHALLENGE> ReadAsync(ChannelReader<byte> reader)
        {
            return new RS_LOGON_CHALLENGE();
        }
    }
}
