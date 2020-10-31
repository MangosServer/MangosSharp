using System.Threading.Tasks;

namespace WorldCluster
{
    public class Startup
    {
        private readonly Mangos.Cluster.WorldCluster worldCluster;

        public Startup(Mangos.Cluster.WorldCluster worldCluster)
        {
            this.worldCluster = worldCluster;
        }

        public async Task StartAsync()
        {
            await worldCluster.StartAsync();
        }
    }
}
