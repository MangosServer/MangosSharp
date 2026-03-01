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
using Mangos.Cluster.Interop.Dispatchers;
using Mangos.Cluster.Interop.Protocol;
using Mangos.Cluster.Interop.Proxies;
using Mangos.Configuration;
using Mangos.Logging;
using Mangos.MySql;
using Mangos.World;
using Mangos.World.Network;
using System.Net;
using System.Net.Sockets;
using WorldServer;

Console.Title = "World Server";

// Phase 1: Load configuration to determine cluster connection parameters
var preBuilder = new ContainerBuilder();
preBuilder.RegisterModule<ConfigurationModule>();
preBuilder.RegisterModule<LoggingModule>();
var preContainer = preBuilder.Build();
var configuration = preContainer.Resolve<MangosConfiguration>();
var logger = preContainer.Resolve<IMangosLogger>();

logger.Trace(@" __  __      _  _  ___  ___  ___   __   __ ___               ");
logger.Trace(@"|  \/  |__ _| \| |/ __|/ _ \/ __|  \ \ / /| _ )      We Love ");
logger.Trace(@"| |\/| / _` | .` | (_ | (_) \__ \   \ V / | _ \   Vanilla Wow");
logger.Trace(@"|_|  |_\__,_|_|\_|\___|\___/|___/    \_/  |___/              ");
logger.Trace("                                                              ");
logger.Trace(" Website / Forum / Support: https://www.getmangos.eu/          ");

logger.Information("World server initialization - Phase 1 complete");
logger.Debug($"Configuration loaded - Cluster host: {configuration.World.ClusterConnectHost}:{configuration.World.ClusterConnectPort}");
logger.Debug($"World databases: Account={configuration.World.AccountDatabase}, Character={configuration.World.CharacterDatabase}, World={configuration.World.WorldDatabase}");
logger.Debug($"Maps configured: {configuration.World.Maps.Length} maps, VMaps enabled: {configuration.World.VMapsEnabled}");
logger.Debug($"XP Rate: {configuration.World.XPRate}, Map resolution: {configuration.World.MapResolution}");
logger.Debug($"Runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
logger.Debug($"OS: {System.Runtime.InteropServices.RuntimeInformation.OSDescription}");
logger.Debug($"Process ID: {Environment.ProcessId}");

// Phase 2: Connect to cluster via IPC
logger.Information($"Connecting to cluster at {configuration.World.ClusterConnectHost}:{configuration.World.ClusterConnectPort}");

InteropConnection? interopConnection = null;
ClusterInteropProxy? clusterProxy = null;
var connectionAttempt = 0;

while (interopConnection == null)
{
    connectionAttempt++;
    logger.Debug($"Cluster IPC connection attempt #{connectionAttempt}");
    try
    {
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.NoDelay = true;
        logger.Trace($"Attempting TCP connection to {configuration.World.ClusterConnectHost}:{configuration.World.ClusterConnectPort}");
        await socket.ConnectAsync(new IPEndPoint(
            IPAddress.Parse(configuration.World.ClusterConnectHost),
            configuration.World.ClusterConnectPort));

        interopConnection = new InteropConnection(socket);
        clusterProxy = new ClusterInteropProxy(interopConnection);

        logger.Information($"Connected to cluster via IPC after {connectionAttempt} attempt(s)");
    }
    catch (Exception ex)
    {
        logger.Warning($"Unable to connect to cluster (attempt #{connectionAttempt}): {ex.GetType().Name}: {ex.Message}. Retrying in 3 seconds...");
        interopConnection = null;
        clusterProxy = null;
        await Task.Delay(3000);
    }
}

// Phase 3: Build full DI container with the IPC-backed ICluster proxy
logger.Information("Phase 3: Building full DI container with IPC-backed ICluster proxy");
var builder = new ContainerBuilder();
builder.RegisterModule<ConfigurationModule>();
builder.RegisterModule<LoggingModule>();
builder.RegisterModule<MySqlModule>();
builder.RegisterModule<LegacyWorldModule>();
if (clusterProxy != null)
{
    logger.Debug("Registering WorldServerModule with cluster proxy");
    builder.RegisterModule(new WorldServerModule(clusterProxy));
}
var container = builder.Build();
WorldServiceLocator.Container = container;
var worldServer = container.Resolve<Mangos.World.WorldServer>();
logger.Debug("Full DI container built successfully");

// Phase 4: Start the world server (loads DB, DBC, quests, etc.)
logger.Information("Phase 4: Starting legacy world server (DB, DBC, quests loading)");
using (logger.BeginTimedOperation("Legacy world server startup"))
{
    await worldServer.StartAsync();
}
logger.Information("Legacy world server started successfully");

// Phase 5: Wire up the IPC dispatcher so the cluster can call IWorld methods on us
logger.Information("Phase 5: Wiring up IPC dispatcher for cluster-to-world calls");
var wsWorldServerClass = worldServer.ClsWorldServer;
var worldDispatcher = new WorldInteropDispatcher(wsWorldServerClass);

interopConnection.OnMethodCallAsync = (methodId, data) =>
{
    logger.Trace($"[IPC] Received method call from cluster: {methodId}, data size: {data.Length} bytes");
    return worldDispatcher.DispatchAsync(methodId, data);
};
interopConnection.OnDisconnected = () =>
{
    logger.Error("Cluster IPC connection lost! Attempting reconnection...");
    logger.Critical("World server may be unable to process cluster requests until reconnected");
};

interopConnection.StartReceiving();
logger.Debug("IPC receive loop started, listening for cluster method calls");

logger.Information("World server is ready and connected to cluster");
logger.Information("All initialization phases complete - world server is fully operational");

// Keep the process alive
worldServer.WaitConsoleCommand();
