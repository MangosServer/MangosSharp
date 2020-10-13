using System.Threading.Tasks;

namespace Mangos.Network.Tcp
{
	public interface ITcpClientFactory
	{
		Task<ITcpClient> CreateTcpClientAsync();
	}
}
