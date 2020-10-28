using Mangos.Network.Tcp.Extensions;
using Mangos.Realm.Network.Requests;
using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Realm.Network.Readers
{
    public class RS_LOGON_PROOF_Reader : IPacketReader<RS_LOGON_PROOF>
    {
        public async ValueTask<RS_LOGON_PROOF> ReadAsync(ChannelReader<byte> reader)
        {
            var a = await reader.ReadArrayAsync(32);
            var m1 = await reader.ReadArrayAsync(20);
            await reader.ReadVoidAsync(22);
            return new RS_LOGON_PROOF(a, m1);
        }
    }
}
