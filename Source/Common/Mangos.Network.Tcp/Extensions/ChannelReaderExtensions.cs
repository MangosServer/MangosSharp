using System.Collections.Generic;
using System.Threading.Channels;

namespace Mangos.Network.Tcp.Extensions
{
    public static class ChannelReaderExtensions
    {
        public static async IAsyncEnumerable<byte> ReadAsync(this ChannelReader<byte> reader, int count)
        {
            for(int i = 0; i < count; i++)
            {
                yield return await reader.ReadAsync();
            }
        }
    }
}
