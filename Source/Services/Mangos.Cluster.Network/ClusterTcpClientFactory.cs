﻿//
// Copyright (C) 2013-2021 getMaNGOS <https://getmangos.eu>
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
            Client client = new Client();

            ClientClass clientClass = new ClientClass(
                client,
                clientSocket,
                _clusterServiceLocator,
                configurationProvider);

            await clientClass.OnConnectAsync();

            ClusterTcpClient clusterTcpClient = new ClusterTcpClient(
                logger,
                client,
                clientClass);

            return clusterTcpClient;
        }
    }
}
