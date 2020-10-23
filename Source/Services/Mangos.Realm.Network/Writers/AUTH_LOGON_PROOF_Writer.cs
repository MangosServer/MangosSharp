using Mangos.Network.Tcp.Mvc;
using Mangos.Realm.Network.Responses;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Realm.Network.Writers
{
    public class AUTH_LOGON_PROOF_Writer : IPacketWriter<AUTH_LOGON_PROOF>
    {
        public async ValueTask WriteAsync(AUTH_LOGON_PROOF packet, ChannelWriter<byte> writer)
        {
        }
    }
}
