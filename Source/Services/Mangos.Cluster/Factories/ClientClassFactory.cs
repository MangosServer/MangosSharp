using System.Net.Sockets;
using System.Threading.Tasks;
using Mangos.Cluster.Network;
using Mangos.Network.Tcp;

namespace Mangos.Cluster.Factories
{
    public class ClientClassFactory : ITcpClientFactory
    {
        private readonly ClusterServiceLocator clusterServiceLocator;

        public ClientClassFactory(ClusterServiceLocator clusterServiceLocator)
        {
            this.clusterServiceLocator = clusterServiceLocator;
        }


        public async Task<ITcpClient> CreateTcpClientAsync(Socket clientSocket)
        {
            var clientClass = new ClientClass(clusterServiceLocator, clientSocket);
            await clientClass.OnConnectAsync();
            return clientClass;
        }
    }
}
