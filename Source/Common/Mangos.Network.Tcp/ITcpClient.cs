using System.Threading;
using System.Threading.Channels;

namespace Mangos.Network.Tcp
{
    public interface ITcpClient
	{
		void HandleAsync(
			ChannelReader<byte> reader, 
			ChannelWriter<byte> writer, 
			CancellationToken cancellationToken);
	}
}
