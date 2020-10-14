//
// Copyright (C) 2013-2020 getMaNGOS <https:\\getmangos.eu>
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

using Mangos.Configuration;
using Mangos.Loggers;
using Mangos.Network.Tcp;
using Mangos.Storage.MySql;
using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Mangos.Realm.Async
{
    public class RealmServer
    {
        private readonly ILogger logger;
        private readonly MySqlAccountStorage accountStorage;
        private readonly IConfigurationProvider<RealmServerConfiguration> configurationProvider;

        private readonly TcpServer tcpServer;

        public RealmServer(
            ILogger logger,
            MySqlAccountStorage accountStorage,
            IConfigurationProvider<RealmServerConfiguration> configurationProvider,
            TcpServer tcpServer)
        {
            this.logger = logger;
            this.accountStorage = accountStorage;
            this.configurationProvider = configurationProvider;
            this.tcpServer = tcpServer;
        }

        public async Task StartAsync()
        {
            LogInitialInformation();

            await ConnectToDatabaseAsync();
            await StartTcpServer();

            Console.ReadLine();
        }

        private async Task ConnectToDatabaseAsync()
        {
            var configuration = await configurationProvider.GetConfigurationAsync();
            await accountStorage.ConnectAsync(configuration.AccountConnectionString);
        }

        private async Task StartTcpServer()
        {
            var configuration = await configurationProvider.GetConfigurationAsync();
            var endpoint = IPEndPoint.Parse(configuration.RealmServerEndpoint);
            tcpServer.Start(endpoint, 10);
        }

        private void LogInitialInformation()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyTitle = assembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;
            var product = assembly.GetCustomAttribute<AssemblyProductAttribute>().Product;
            var copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;

            Console.Title = $"{assemblyTitle} v{Assembly.GetExecutingAssembly().GetName().Version}";
            logger.Debug(product);
            logger.Debug(copyright);
            logger.Message("  __  __      _  _  ___  ___  ___                ");
            logger.Message(@" |  \/  |__ _| \| |/ __|/ _ \/ __|   We Love    ");
            logger.Message(@" | |\/| / _` | .` | (_ | (_) \__ \   Vanilla Wow");
            logger.Message(@" |_|  |_\__,_|_|\_|\___|\___/|___/              ");
            logger.Message("                                                 ");
            logger.Message(" Website / Forum / Support: https://getmangos.eu/");
        }
    }
}
