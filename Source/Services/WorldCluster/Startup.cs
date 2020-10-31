using Mangos.Cluster.Configuration;
using Mangos.Configuration.Xml;
using Mangos.Loggers;
using Mangos.Network.Tcp;
using System.Net;
using System.Threading.Tasks;

namespace WorldCluster
{
    public class Startup
    {
        private readonly ILogger logger;
        private readonly XmlConfigurationProvider<ClusterConfiguration> configurationProvider;
        private readonly TcpServer tcpServer;

        private readonly Mangos.Cluster.WorldCluster worldCluster;

        public Startup(
            ILogger logger,
            XmlConfigurationProvider<ClusterConfiguration> configurationProvider,
            Mangos.Cluster.WorldCluster worldCluster, 
            TcpServer tcpServer)
        {
            this.worldCluster = worldCluster;
            this.configurationProvider = configurationProvider;
            this.logger = logger;
            this.tcpServer = tcpServer;
        }

        public async Task StartAsync()
        {
            LoadConfiguration();
            WriteServiceInformation();

            await worldCluster.StartAsync();

            StartTcpServer();

            worldCluster.WaitConsoleCommand();
        }

        private void LoadConfiguration()
        {
            configurationProvider.LoadFromFile("configs/WorldCluster.ini");
            logger.Debug("Cluster configuration has been loaded");
        }

        private void StartTcpServer()
        {
            var configuration = configurationProvider.GetConfiguration();
            var endpoint = IPEndPoint.Parse(configuration.WorldClusterEndpoint);
            tcpServer.Start(endpoint, 10);
            logger.Debug("Tcp server has been started");
        }

        private void WriteServiceInformation()
        {

        }
    }
}
