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
using Mangos.Cluster.Admin.Auth;
using Mangos.Cluster.Admin.Commands;
using Mangos.Cluster.Admin.Protocol;
using Mangos.Cluster.Federation;
using Mangos.Cluster.Interop;
using Mangos.Cluster.Interop.Dispatchers;
using Mangos.Cluster.Interop.Protocol;
using Mangos.Cluster.Network;
using Mangos.Cluster.Supervision;
using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using Mangos.Configuration;
using Mangos.Logging;
using Mangos.MySql;
using Mangos.MySql.Connections;
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
var supervisor = container.Resolve<WorldSupervisor>();
var adminHandler = container.Resolve<IAdminCommandHandler>();
var federationRouter = container.Resolve<FederationRouter>();

logger.Trace(@" __  __      _  _  ___  ___  ___               ");
logger.Trace(@"|  \/  |__ _| \| |/ __|/ _ \/ __|   We Love    ");
logger.Trace(@"| |\/| / _` | .` | (_ | (_) \__ \   Vanilla Wow");
logger.Trace(@"|_|  |_\__,_|_|\_|\___|\___/|___/              ");
logger.Trace("                                                ");
logger.Trace("Website / Forum / Support: https://www.getmangos.eu/");

// Check database version for account database
using (var scope = container.BeginLifetimeScope())
{
    var accountConnection = scope.Resolve<AccountConnection>();
    var globalConstants = scope.Resolve<MangosGlobalConstants>();
    var dbVersionChecker = new DbVersionChecker(logger, globalConstants);
    
    if (!dbVersionChecker.CheckRequiredDbVersion(accountConnection.MySqlConnection, "account", ServerDb.Realm))
    {
        logger.Error("Database version check failed. Exiting...");
        Environment.Exit(1);
    }
}

// Check database version for character database
using (var scope = container.BeginLifetimeScope())
{
    var characterConnection = scope.Resolve<CharacterConnection>();
    var globalConstants = scope.Resolve<MangosGlobalConstants>();
    var dbVersionChecker = new DbVersionChecker(logger, globalConstants);

    if (!dbVersionChecker.CheckRequiredDbVersion(characterConnection.MySqlConnection, "character", ServerDb.Character))
    {
        logger.Error("Database version check failed. Exiting...");
        Environment.Exit(1);
    }
}

// Check database version for world database
using (var scope = container.BeginLifetimeScope())
{
    var worldConnection = scope.Resolve<WorldConnection>();
    var globalConstants = scope.Resolve<MangosGlobalConstants>();
    var dbVersionChecker = new DbVersionChecker(logger, globalConstants);

    if (!dbVersionChecker.CheckRequiredDbVersion(worldConnection.MySqlConnection, "world", ServerDb.World))
    {
        logger.Error("Database version check failed. Exiting...");
        Environment.Exit(1);
    }
}

logger.Information("Starting legacy cluster server");
await legacyWorldCluster.StartAsync();

// Start the supervisor before any world IPC connection arrives so hello/goodbye are tracked.
await supervisor.StartAsync();

// Federation listener (cluster <-> cluster). Off by default; enable in
// Federation.* config to allow peer admin commands and (PR #6) cross-realm
// chat / groups. We hold the FederationServer alive for the lifetime of
// the process via a top-level using; ProcessExit drains it.
FederationServer? federation = null;
if (configuration.Federation is { Enabled: true } fedCfg)
{
    var secrets = fedCfg.Peers.ToDictionary(p => p.ClusterId, p => PeerAuth.SecretFromString(p.Secret));
    federation = new FederationServer(
        fedCfg.LocalClusterId,
        fedCfg.LocalDisplayTag,
        peerId => secrets.TryGetValue(peerId, out var s) ? s : null)
    {
        AdminHandler = adminHandler,
        OnLinkAccepted = link => federationRouter.BindHandlers(link),
    };
    await federation.StartAsync(fedCfg.ListenAddress, fedCfg.ListenPort);
    logger.Information($"Federation listener up on {fedCfg.ListenAddress}:{fedCfg.ListenPort} (cluster id {fedCfg.LocalClusterId})");
}
else
{
    logger.Information("Federation disabled");
}
AppDomain.CurrentDomain.ProcessExit += (_, _) => federation?.Dispose();

// Hook process exit so we drain managed worlds gracefully on Ctrl-C / SIGTERM.
AppDomain.CurrentDomain.ProcessExit += async (_, _) =>
{
    try { await supervisor.DisposeAsync(); }
    catch (Exception ex) { logger.Error($"Supervisor dispose failed: {ex.Message}"); }
};
Console.CancelKeyPress += (_, args) =>
{
    args.Cancel = true; // we handle it; don't kill abruptly
    logger.Information("Ctrl-C received; draining...");
    Environment.Exit(ExitCodes.Clean);
};

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

// Console REPL for operator commands. Runs in the background so the
// cluster's own logs aren't drowned out; same command syntax as in-game
// GM chat and the external CLI.
var repl = new ConsoleAdminRepl(adminHandler);
_ = repl.RunAsync();

logger.Information("Starting cluster TCP server for game clients");
await tcpServer.RunAsync(configuration.Cluster.ClusterServerEndpoint);
