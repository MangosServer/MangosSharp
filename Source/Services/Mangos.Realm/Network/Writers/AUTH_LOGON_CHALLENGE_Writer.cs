using Mangos.Common.Enums.Authentication;
using Mangos.Common.Enums.Global;
using Mangos.Network.Tcp.Extensions;
using Mangos.Realm.Network.Responses;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Realm.Network.Writers
{
    public class AUTH_LOGON_CHALLENGE_Writer : IPacketWriter<AUTH_LOGON_CHALLENGE>
    {
        public async ValueTask WriteAsync(ChannelWriter<byte> writer, AUTH_LOGON_CHALLENGE packet)
        {
            await writer.WriteAsync((byte)AuthCMD.CMD_AUTH_LOGON_CHALLENGE);
            await writer.WriteAsync((byte)AccountState.LOGIN_OK);
            await writer.WriteAsync(0);
            await writer.WriteAsync(packet.PublicB);
            await writer.WriteAsync((byte)packet.G.Length);
            await writer.WriteAsync(packet.G[0]);
            await writer.WriteAsync(32);
            await writer.WriteAsync(packet.N);
            await writer.WriteAsync(packet.Salt);
            await writer.WriteAsync(packet.CrcSalt);
            await writer.WriteAsync(0);
        }
    }
}
