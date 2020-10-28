//
// Copyright (C) 2013-2020 getMaNGOS <https://getmangos.eu>
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

using System.Threading.Tasks;
using Autofac;
using Mangos.Common.Globals;
using Mangos.Configuration;
using Mangos.Configuration.Store;
using Mangos.Configuration.Xml;
using Mangos.Loggers;
using Mangos.Loggers.Console;
using Mangos.Network.Tcp;
using Mangos.Realm;
using Mangos.Realm.Factories;
using Mangos.Realm.Storage.MySql;
using Mangos.Storage.Account;
using Mangos.Storage.MySql;

namespace RealmServer
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var container = CreateContainer();
            var realmServer = container.Resolve<RealmServerService>();
            await realmServer.StartAsync();
        }

        public static IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();
            RegisterLoggers(builder);
            RegisterConfiguration(builder);
            RegisterStorages(builder);
            RegisterTcpServer(builder);
            RegisterServices(builder);
            return builder.Build();
        }

        public static void RegisterConfiguration(ContainerBuilder builder)
        {
            builder.Register(x => new XmlFileConfigurationProvider<RealmServerConfiguration>(
                    x.Resolve<ILogger>(), "configs/RealmServer.ini"))
                .As<IConfigurationProvider<RealmServerConfiguration>>()
                .SingleInstance();
            builder.RegisterDecorator<StoredConfigurationProvider<RealmServerConfiguration>,
                IConfigurationProvider<RealmServerConfiguration>>();
        }

        public static void RegisterLoggers(ContainerBuilder builder)
        {
            builder.RegisterType<ConsoleLogger>().As<ILogger>().SingleInstance();
        }

        private static void RegisterTcpServer(ContainerBuilder builder)
        {
            builder.RegisterType<TcpServer>().AsSelf().SingleInstance();
        }

        public static void RegisterStorages(ContainerBuilder builder)
        {
            builder.RegisterType<RealmStorage>()
                .AsSelf()
                .As<IRealmStorage>()
                .SingleInstance();
        }

        public static void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<Converter>().As<Converter>().SingleInstance();
            builder.RegisterType<MangosGlobalConstants>().As<MangosGlobalConstants>().SingleInstance();

            builder.RegisterType<RealmServerService>().As<RealmServerService>().SingleInstance();
            builder.RegisterType<RealmServerClientFactory>().As<ITcpClientFactory>().SingleInstance();
        }
    }
}
