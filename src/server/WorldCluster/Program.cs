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

logger.Information("World Cluster initialization starting");
logger.Debug($"Configuration loaded - Cluster endpoint: {configuration.Cluster.ClusterServerEndpoint}");
logger.Debug($"Cluster listen address: {configuration.Cluster.ClusterListenAddress}:{configuration.Cluster.ClusterListenPort}");
logger.Debug($"Server player limit: {configuration.Cluster.ServerPlayerLimit}");
logger.Debug($"Runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
logger.Debug($"OS: {System.Runtime.InteropServices.RuntimeInformation.OSDescription}");
logger.Debug($"Process ID: {Environment.ProcessId}");

logger.Information("Starting legacy cluster server");
logger.Debug("Initializing legacy cluster server components");
using (logger.BeginTimedOperation("Legacy cluster server startup"))
{
    await legacyWorldCluster.StartAsync();
}
logger.Information("Legacy cluster server started successfully");

// Start IPC server for world server connections
logger.Information($"Starting cluster IPC server on {configuration.Cluster.ClusterListenAddress}:{configuration.Cluster.ClusterListenPort}");

var interopServer = new InteropServer();
interopServer.OnWorldServerConnected = connection =>
{
    logger.Information("World server connected via IPC");
    logger.Debug($"IPC connection established, IsConnected: {connection.IsConnected}");

    var dispatcher = new ClusterInteropDispatcher(worldServerClass, connection);
    logger.Debug("ClusterInteropDispatcher created for new world server connection");

    connection.OnMethodCall = (methodId, data) =>
    {
        logger.Trace($"[IPC] Received method call from world server: {methodId}, data size: {data.Length} bytes");
        return dispatcher.Dispatch(methodId, data);
    };

    connection.OnDisconnected = () =>
    {
        logger.Warning("World server IPC connection lost");
        logger.Warning("Cluster will not be able to forward packets to world server until reconnected");
    };

    connection.StartReceiving();
    logger.Debug("IPC receive loop started for world server connection");
};

// Run IPC server in background
logger.Debug("Starting IPC server in background task");
_ = Task.Run(async () =>
{
    try
    {
        logger.Debug($"IPC server binding to {configuration.Cluster.ClusterListenAddress}:{configuration.Cluster.ClusterListenPort}");
        await interopServer.RunAsync(
            configuration.Cluster.ClusterListenAddress,
            configuration.Cluster.ClusterListenPort);
    }
    catch (Exception ex)
    {
        logger.Error(ex, $"IPC server fatal error");
    }
});

logger.Information($"Starting cluster TCP server for game clients on {configuration.Cluster.ClusterServerEndpoint}");
logger.Debug("TCP server will now begin accepting game client connections");
await tcpServer.RunAsync(configuration.Cluster.ClusterServerEndpoint);
