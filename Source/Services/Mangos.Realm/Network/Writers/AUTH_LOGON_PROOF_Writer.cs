using Mangos.Common.Enums.Authentication;
using Mangos.Network.Tcp.Extensions;
using Mangos.Realm.Network.Responses;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Realm.Network.Writers
{
    public class AUTH_LOGON_PROOF_Writer : IPacketWriter<AUTH_LOGON_PROOF>
    {
        public async ValueTask WriteAsync(ChannelWriter<byte> writer, AUTH_LOGON_PROOF packet)
        {
            await writer.WriteAsync((byte)AuthCMD.CMD_AUTH_LOGON_PROOF);
            await writer.WriteAsync((byte)packet.AccountState);

            if(packet.M2 != null)
            {
                await writer.WriteEnumerableAsync(packet.M2);
                await writer.WriteZeroNCountAsync(4);
            }
        }
    }
}
