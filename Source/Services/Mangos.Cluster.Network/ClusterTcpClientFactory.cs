using Mangos.Cluster.Configuration;
using Mangos.Configuration;
using Mangos.Loggers;
using Mangos.Network.Tcp;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Mangos.Cluster.Network
{
    public class ClusterTcpClientFactory : ITcpClientFactory
    {
        private readonly ClusterServiceLocator _clusterServiceLocator;
        private readonly ILogger logger;
        private readonly IConfigurationProvider<ClusterConfiguration> configurationProvider;

        public ClusterTcpClientFactory(
            ClusterServiceLocator clusterServiceLocator,
            ILogger logger,
            IConfigurationProvider<ClusterConfiguration> configurationProvider)
        {
            _clusterServiceLocator = clusterServiceLocator;
            this.logger = logger;
            this.configurationProvider = configurationProvider;
        }

        public async Task<ITcpClient> CreateTcpClientAsync(Socket clientSocket)
        {
            var clientClass = new ClientClass(_clusterServiceLocator, clientSocket, configurationProvider);
            await clientClass.OnConnectAsync();
            var clusterTcpClient = new ClusterTcpClient(logger, clientClass);
            return clusterTcpClient;
        }
    }
}
