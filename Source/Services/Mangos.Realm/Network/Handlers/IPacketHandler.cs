using Mangos.Realm.Models;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Realm.Network.Handlers
{
    public interface IPacketHandler
    {
        Task HandleAsync(ChannelReader<byte> reader, ChannelWriter<byte> writer, ClientModel clientModel);
    }
}
