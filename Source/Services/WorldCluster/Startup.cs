using Mangos.Cluster.Configuration;
using Mangos.Configuration.Xml;
using Mangos.Loggers;
using System.Threading.Tasks;

namespace WorldCluster
{
    public class Startup
    {
        private readonly ILogger logger;
        private readonly XmlConfigurationProvider<ClusterConfiguration> configurationProvider;

        private readonly Mangos.Cluster.WorldCluster worldCluster;

        public Startup(
            ILogger logger,
            XmlConfigurationProvider<ClusterConfiguration> configurationProvider,
            Mangos.Cluster.WorldCluster worldCluster)
        {
            this.worldCluster = worldCluster;
            this.configurationProvider = configurationProvider;
            this.logger = logger;
        }

        public async Task StartAsync()
        {
            LoadConfiguration();
            WriteServiceInformation();

            await worldCluster.StartAsync();
        }

        private void LoadConfiguration()
        {
            configurationProvider.LoadFromFile("configs/WorldCluster.ini");
            logger.Debug("Cluster configuration has been loaded");
        }

        private void WriteServiceInformation()
        {

        }
    }
}
