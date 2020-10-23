using Mangos.Network.Tcp;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Mangos.Realm.Network
{
    public class TcpClientFactroy : ITcpClientFactory
    {
        private readonly PipelineMap pipelineMap;

        public TcpClientFactroy(PipelineMap pipelineMap)
        {
            this.pipelineMap = pipelineMap;
        }

        public Task<ITcpClient> CreateTcpClientAsync(Socket clientSocket)
        {
            var tpcClient = new TcpClient(pipelineMap);
            return Task.FromResult<ITcpClient>(tpcClient);
        }
    }
}
