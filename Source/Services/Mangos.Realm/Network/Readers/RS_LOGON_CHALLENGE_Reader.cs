using Mangos.Network.Tcp.Extensions;
using Mangos.Realm.Network.Requests;
using System;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Realm.Network.Readers
{
    public class RS_LOGON_CHALLENGE_Reader : IPacketReader<RS_LOGON_CHALLENGE>
    {
        public async ValueTask<RS_LOGON_CHALLENGE> ReadAsync(ChannelReader<byte> reader)
        {
            await reader.ReadVoidAsync(1);
            var lengthArray = await reader.ReadArrayAsync(2);
            var length = BitConverter.ToInt16(lengthArray);
            var data = await reader.ReadArrayAsync(length);

            // Read account name from packet
            var accountLength = data[29];
            var account = new byte[accountLength];
            Array.Copy(data, 30, account, 0, accountLength);
            var accountName = Encoding.UTF8.GetString(account);

            // Get the client build from packet.
            int clientBuild = BitConverter.ToInt16(new[] { data[7], data[8] }, 0);

            return new RS_LOGON_CHALLENGE(account, accountName, clientBuild);
        }
    }
}
