using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Network.Tcp.Extensions
{
    public static class ChannelWriterExtensions
    {
        public static async ValueTask WriteAsync(this ChannelWriter<byte> writer, IEnumerable<byte> data)
        {
            foreach(var item in data)
            {
                await writer.WriteAsync(item);
            }
        }

        public static async ValueTask WriteAsync(this ChannelWriter<byte> writer, byte[] data, int count)
        {
            for(int i = 0; i < count; i++)
            {
                await writer.WriteAsync(data[i]);
            }
        }
    }
}
