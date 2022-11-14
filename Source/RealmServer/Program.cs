//
// Copyright (C) 2013-2022 getMaNGOS <https://getmangos.eu>
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
using Mangos.Configuration.Implementation;
using Mangos.Logging;
using Mangos.Logging.Implementation;
using Mangos.MySql.Implementation;
using Mangos.Tcp.Implementation;
using RealmServer;

var container = CreateApplicationContainer();
var logger = container.Resolve<IMangosLogger>();
logger.Trace(@" __  __      _  _  ___  ___  ___               ");
logger.Trace(@"|  \/  |__ _| \| |/ __|/ _ \/ __|   We Love    ");
logger.Trace(@"| |\/| / _` | .` | (_ | (_) \__ \   Vanilla Wow");
logger.Trace(@"|_|  |_\__,_|_|\_|\___|\___/|___/              ");
logger.Trace("                                                ");
logger.Trace("Website / Forum / Support: https://getmangos.eu/");

var configuration = container.Resolve<MangosConfiguration>();
var tcptServer = container.Resolve<TcpServer>();

logger.Information("Starting tcp server");
await tcptServer.StartAsync(configuration.RealmServerEndpoint);

IContainer CreateApplicationContainer()
{
    var builder = new ContainerBuilder();
    builder.RegisterModule<ConfigurationModule>();
    builder.RegisterModule<LoggingModule>();
    builder.RegisterModule<MySqlModule>();
    builder.RegisterModule<TcpModule>();

    builder.RegisterModule<RealmModule>();
    return builder.Build();
}
