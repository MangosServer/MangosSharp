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

using System.Net;
using System.Net.Sockets;
using Autofac;
using Mangos.Cluster.Interop;
using Mangos.Cluster.Interop.Dispatchers;
using Mangos.Cluster.Interop.Protocol;
using Mangos.Cluster.Interop.Proxies;
using Mangos.Configuration;
using Mangos.Logging;
using Mangos.MySql;
using Mangos.World;
using Mangos.World.Network;
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

// Phase 2: Connect to cluster via IPC. The world is autonomous - it can
// outlive a missing cluster and keep retrying. If the cluster never
// appears we exit with ExitCodes.Orphaned so the supervisor can respawn
// us once the cluster is back.
const int ClusterConnectGraceMs = 60_000;
var connectStarted = DateTime.UtcNow;
InteropConnection? interopConnection = null;
ClusterInteropProxy? clusterProxy = null;

while (interopConnection == null)
{
    try
    {
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            NoDelay = true,
        };
        await socket.ConnectAsync(new IPEndPoint(
            IPAddress.Parse(configuration.World.ClusterConnectHost),
            configuration.World.ClusterConnectPort));

        interopConnection = new InteropConnection(socket);
        clusterProxy = new ClusterInteropProxy(interopConnection);

        logger.Information("Connected to cluster via IPC");
    }
    catch (Exception ex)
    {
        if ((DateTime.UtcNow - connectStarted).TotalMilliseconds > ClusterConnectGraceMs)
        {
            logger.Error($"Cluster unreachable for >{ClusterConnectGraceMs / 1000}s; exiting Orphaned ({ExitCodes.Orphaned})");
            Environment.Exit(ExitCodes.Orphaned);
        }
        logger.Warning($"Unable to connect to cluster: {ex.Message}. Retrying in 3 seconds...");
        interopConnection = null;
        clusterProxy = null;
        await Task.Delay(3000);
    }
}

// Phase 3: Build full DI container with the IPC-backed ICluster proxy
var builder = new ContainerBuilder();
builder.RegisterModule<ConfigurationModule>();
builder.RegisterModule<LoggingModule>();
builder.RegisterModule<MySqlModule>();
builder.RegisterModule<LegacyWorldModule>();
builder.RegisterModule(new WorldServerModule(clusterProxy!));
var container = builder.Build();
WorldServiceLocator.Container = container;
var worldServer = container.Resolve<Mangos.World.WorldServer>();

// Phase 4: Start the world server (loads DB, DBC, quests, etc.)
logger.Information("Starting legacy world server");
try
{
    await worldServer.StartAsync();
}
catch (Exception ex)
{
    logger.Error($"World failed to start: {ex.Message}");
    Environment.Exit(ExitCodes.FatalCrash);
}

// Phase 5: Wire up the IPC dispatcher so the cluster can call IWorld methods on us
var wsWorldServerClass = worldServer.ClsWorldServer;
var worldDispatcher = new WorldInteropDispatcher(wsWorldServerClass);

interopConnection.OnMethodCallAsync = (methodId, data) => worldDispatcher.DispatchAsync(methodId, data);
interopConnection.OnDisconnected = () =>
{
    logger.Error("Cluster IPC connection lost; entering autonomous mode");
};

interopConnection.StartReceiving();

logger.Information("World server is ready and connected to cluster");

// Trap Ctrl-C / SIGTERM for a clean shutdown.
Console.CancelKeyPress += (_, args) =>
{
    args.Cancel = true;
    logger.Information("Shutdown requested; exiting cleanly");
    Environment.Exit(ExitCodes.Clean);
};
AppDomain.CurrentDomain.ProcessExit += (_, _) =>
{
    logger.Information("Process exit; flushing");
};

// Keep the process alive on the console command loop. WaitConsoleCommand
// returns when the operator types 'shutdown'; treat that as a clean exit.
worldServer.WaitConsoleCommand();
Environment.Exit(ExitCodes.Clean);
