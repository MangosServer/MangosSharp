using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Realm.Network.Readers
{
    public interface IPacketReader<T>
    {
        ValueTask<T> ReadAsync(ChannelReader<byte> reader);
    }
}
