//
// Copyright (C) 2013-2025 getMaNGOS <https://www.getmangos.eu>
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

using Autofac;
using Mangos.Cluster;
using Mangos.Cluster.Interop.Dispatchers;
using Mangos.Cluster.Interop.Protocol;
using Mangos.Cluster.Network;
using Mangos.Configuration;
using Mangos.Logging;
using Mangos.MySql;
using Mangos.Tcp;
using WorldCluster;

Console.Title = "World Cluster";

var builder = new ContainerBuilder();
builder.RegisterModule<LegacyClusterModule>();
builder.RegisterModule<ConfigurationModule>();
builder.RegisterModule<LoggingModule>();
builder.RegisterModule<MySqlModule>();
builder.RegisterModule<TcpModule>();
builder.RegisterModule<ClusterModule>();

var container = builder.Build();
var configuration = container.Resolve<MangosConfiguration>();
var logger = container.Resolve<IMangosLogger>();
var tcpServer = container.Resolve<TcpServer>();
var legacyWorldCluster = container.Resolve<LegacyWorldCluster>();
var worldServerClass = container.Resolve<WorldServerClass>();

logger.Trace(@" __  __      _  _  ___  ___  ___               ");
logger.Trace(@"|  \/  |__ _| \| |/ __|/ _ \/ __|   We Love    ");
logger.Trace(@"| |\/| / _` | .` | (_ | (_) \__ \   Vanilla Wow");
logger.Trace(@"|_|  |_\__,_|_|\_|\___|\___/|___/              ");
logger.Trace("                                                ");
logger.Trace("Website / Forum / Support: https://www.getmangos.eu/");

logger.Information("Starting legacy cluster server");
await legacyWorldCluster.StartAsync();

// Start IPC server for world server connections
logger.Information($"Starting cluster IPC server on {configuration.Cluster.ClusterListenAddress}:{configuration.Cluster.ClusterListenPort}");

var interopServer = new InteropServer();
interopServer.OnWorldServerConnected = connection =>
{
    logger.Information("World server connected via IPC");

    var dispatcher = new ClusterInteropDispatcher(worldServerClass, connection);

    connection.OnMethodCall = (methodId, data) => dispatcher.Dispatch(methodId, data);

    connection.OnDisconnected = () =>
    {
        logger.Warning("World server IPC connection lost");
    };

    connection.StartReceiving();
};

// Run IPC server in background
_ = Task.Run(async () =>
{
    try
    {
        await interopServer.RunAsync(
            configuration.Cluster.ClusterListenAddress,
            configuration.Cluster.ClusterListenPort);
    }
    catch (Exception ex)
    {
        logger.Error($"IPC server error: {ex.Message}");
    }
});

logger.Information("Starting cluster TCP server for game clients");
await tcpServer.RunAsync(configuration.Cluster.ClusterServerEndpoint);
