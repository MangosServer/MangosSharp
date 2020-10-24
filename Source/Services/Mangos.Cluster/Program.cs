//
//  Copyright (C) 2013-2020 getMaNGOS <https://getmangos.eu>
//  
//  This program is free software. You can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation. either version 2 of the License, or
//  (at your option) any later version.
//  
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY. Without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//  
//  You should have received a copy of the GNU General Public License
//  along with this program. If not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//

using System.Threading.Tasks;
using Autofac;
using Mangos.Cluster.DataStores;
using Mangos.Cluster.Factories;
using Mangos.Cluster.Globals;
using Mangos.Cluster.Handlers;
using Mangos.Cluster.Handlers.Guild;
using Mangos.Cluster.Network;
using Mangos.Cluster.Stats;
using Mangos.Common.Globals;
using Mangos.Common.Legacy;
using Mangos.Configuration;
using Mangos.Configuration.Store;
using Mangos.Configuration.Xml;
using Mangos.DataStores;
using Mangos.Loggers;
using Mangos.Loggers.Console;
using Mangos.Network.Tcp;
using Mangos.Zip;
using Functions = Mangos.Common.Legacy.Globals.Functions;

namespace Mangos.Cluster
{
    public static class Program
    {
        public static async Task Main()
        {
            var container = CreateContainer();
            var worldCluster = container.Resolve<WorldCluster>();
            await worldCluster.StartAsync();
        }

        public static IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();
            RegisterConfiguration(builder);
            RegisterLoggers(builder);

            RegisterTcpServer(builder);
            RegisterDataStoreProvider(builder);

            RegisterServices(builder);
            return builder.Build();
        }

        public static void RegisterConfiguration(ContainerBuilder builder)
        {
            builder.Register(x => new XmlFileConfigurationProvider<ClusterConfiguration>(
                    x.Resolve<ILogger>(), "configs/WorldCluster.ini"))
                .As<IConfigurationProvider<ClusterConfiguration>>()
                .SingleInstance();
            builder.RegisterDecorator<StoredConfigurationProvider<ClusterConfiguration>,
                IConfigurationProvider<ClusterConfiguration>>();
        }

        public static void RegisterLoggers(ContainerBuilder builder)
        {
            builder.RegisterType<ConsoleLogger>().As<ILogger>().SingleInstance();
        }

        private static void RegisterTcpServer(ContainerBuilder builder)
        {
            builder.RegisterType<TcpServer>().AsSelf().SingleInstance();
            builder.RegisterType<ClientClassFactory>().As<ITcpClientFactory>().SingleInstance();
        }

        private static void RegisterDataStoreProvider(ContainerBuilder builder)
        {
            builder.RegisterType<DataStoreProvider>().As<DataStoreProvider>().SingleInstance();
        }

        private static void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<MangosGlobalConstants>().As<MangosGlobalConstants>().SingleInstance();
            builder.RegisterType<Functions>().As<Functions>().SingleInstance();
            builder.RegisterType<Common.Legacy.Functions>().As<Common.Legacy.Functions>().SingleInstance();
            builder.RegisterType<ZipService>().As<ZipService>().SingleInstance();
            builder.RegisterType<NativeMethods>().As<NativeMethods>().SingleInstance();
            builder.RegisterType<WorldCluster>().As<WorldCluster>().SingleInstance();
            builder.RegisterType<WorldServerClass>().As<WorldServerClass>().SingleInstance();
            builder.RegisterType<WsDbcDatabase>().As<WsDbcDatabase>().SingleInstance();
            builder.RegisterType<WsDbcLoad>().As<WsDbcLoad>().SingleInstance();
            builder.RegisterType<Globals.Functions>().As<Globals.Functions>().SingleInstance();
            builder.RegisterType<Packets>().As<Packets>().SingleInstance();
            builder.RegisterType<WcGuild>().As<WcGuild>().SingleInstance();
            builder.RegisterType<WcStats>().As<WcStats>().SingleInstance();
            builder.RegisterType<WcNetwork>().As<WcNetwork>().SingleInstance();
            builder.RegisterType<WcHandlers>().As<WcHandlers>().SingleInstance();
            builder.RegisterType<WcHandlersAuth>().As<WcHandlersAuth>().SingleInstance();
            builder.RegisterType<WcHandlersBattleground>().As<WcHandlersBattleground>().SingleInstance();
            builder.RegisterType<WcHandlersChat>().As<WcHandlersChat>().SingleInstance();
            builder.RegisterType<WcHandlersGroup>().As<WcHandlersGroup>().SingleInstance();
            builder.RegisterType<WcHandlersGuild>().As<WcHandlersGuild>().SingleInstance();
            builder.RegisterType<WcHandlersMisc>().As<WcHandlersMisc>().SingleInstance();
            builder.RegisterType<WcHandlersMovement>().As<WcHandlersMovement>().SingleInstance();
            builder.RegisterType<WcHandlersSocial>().As<WcHandlersSocial>().SingleInstance();
            builder.RegisterType<WcHandlersTickets>().As<WcHandlersTickets>().SingleInstance();
            builder.RegisterType<WsHandlerChannels>().As<WsHandlerChannels>().SingleInstance();
            builder.RegisterType<WcHandlerCharacter>().As<WcHandlerCharacter>().SingleInstance();

            builder.RegisterType<ClusterServiceLocator>().As<ClusterServiceLocator>()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies)
                .SingleInstance();
        }
    }
}