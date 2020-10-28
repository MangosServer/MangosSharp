using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Realm.Network.Writers
{
    public interface IPacketWriter<T>
    {
        ValueTask WriteAsync(ChannelWriter<byte> writer, T packet);
    }
}
