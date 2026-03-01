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
using Mangos.Configuration;
using Mangos.Logging;
using Mangos.MySql;
using Mangos.Tcp;
using RealmServer;
using RealmServer.Network;
using RealmServer.Verification;

Console.Title = "Realm server";

var builder = new ContainerBuilder();
builder.RegisterModule<ConfigurationModule>();
builder.RegisterModule<LoggingModule>();
builder.RegisterModule<MySqlModule>();
builder.RegisterModule<TcpModule>();
builder.RegisterModule<RealmModule>();

var container = builder.Build();
var configuration = container.Resolve<MangosConfiguration>();
var logger = container.Resolve<IMangosLogger>();
var tcpServer = container.Resolve<TcpServer>();

logger.Trace(@" __  __      _  _  ___  ___  ___               ");
logger.Trace(@"|  \/  |__ _| \| |/ __|/ _ \/ __|   We Love    ");
logger.Trace(@"| |\/| / _` | .` | (_ | (_) \__ \   Vanilla Wow");
logger.Trace(@"|_|  |_\__,_|_|\_|\___|\___/|___/              ");
logger.Trace("                                                ");
logger.Trace("Website / Forum / Support: https://www.getmangos.eu/");

logger.Information("Realm server initialization starting");
logger.Debug($"Configuration loaded - Realm endpoint: {configuration.Realm.RealmServerEndpoint}");
logger.Debug($"DI container built successfully");
logger.Debug($"Runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
logger.Debug($"OS: {System.Runtime.InteropServices.RuntimeInformation.OSDescription}");
logger.Debug($"Process ID: {Environment.ProcessId}");

logger.Information("Initializing realm verifier");
var realmVerifier = container.Resolve<RealmVerifier>();
using (var scope = container.BeginLifetimeScope())
{
    var dispatchers = scope.Resolve<IEnumerable<IHandlerDispatcher>>();
    logger.Debug($"Resolved {dispatchers.Count()} handler dispatchers for realm verification");
    realmVerifier.Initialize(dispatchers);
}
logger.Debug("Starting realm verifier periodic checks");
realmVerifier.Start();
logger.Information("Realm verifier started successfully");

logger.Information($"Starting realm TCP server on {configuration.Realm.RealmServerEndpoint}");
logger.Debug("TCP server will now begin accepting authentication connections");
await tcpServer.RunAsync(configuration.Realm.RealmServerEndpoint);
