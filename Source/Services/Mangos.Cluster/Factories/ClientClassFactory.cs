//
// Copyright (C) 2013-2020 getMaNGOS <https://getmangos.eu>
//
// This program is free software. You can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation. either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY. Without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//

using System.Net.Sockets;
using System.Threading.Tasks;
using Mangos.Cluster.Configuration;
using Mangos.Cluster.Network;
using Mangos.Configuration;
using Mangos.Network.Tcp;

namespace Mangos.Cluster.Factories
{
    public class ClientClassFactory : ITcpClientFactory
    {
        private readonly ClusterServiceLocator _clusterServiceLocator;
        private readonly IConfigurationProvider<ClusterConfiguration> configurationProvider;

        public ClientClassFactory(
            ClusterServiceLocator clusterServiceLocator, 
            IConfigurationProvider<ClusterConfiguration> configurationProvider)
        {
            _clusterServiceLocator = clusterServiceLocator;
            this.configurationProvider = configurationProvider;
        }


        public async Task<ITcpClient> CreateTcpClientAsync(Socket clientSocket)
        {
            var clientClass = new ClientClass(_clusterServiceLocator, clientSocket, configurationProvider);
            await clientClass.OnConnectAsync();
            return clientClass;
        }
    }
}
