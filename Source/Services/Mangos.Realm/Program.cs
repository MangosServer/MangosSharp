using Autofac;
using Mangos.Common.Globals;
using Mangos.Configuration;
using Mangos.Configuration.Store;
using Mangos.Configuration.Xml;
using Mangos.Realm.Factories;

namespace Mangos.Realm
{
    public static class Program
    {
        public static void Main()
        {
            var container = CreateContainer();
            var realmServer = container.Resolve<RealmServer>();
            realmServer.StartAsync().Wait();
        }

        public static IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();
            RegisterConfiguration(builder);
            RegisterServices(builder);
            return builder.Build();
        }

        public static void RegisterConfiguration(ContainerBuilder builder)
        {
            builder.Register(x => new XmlFileConfigurationProvider<RealmServerConfiguration>("configs/RealmServer.ini")).As<IConfigurationProvider<RealmServerConfiguration>>();
            builder.RegisterDecorator<StoredConfigurationProvider<RealmServerConfiguration>, IConfigurationProvider<RealmServerConfiguration>>();
        }

        public static void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<RealmServer>().As<RealmServer>();
            builder.RegisterType<RealmServerClass>().As<RealmServerClass>();
            builder.RegisterType<Converter>().As<Converter>();
            builder.RegisterType<Global_Constants>().As<Global_Constants>();
            builder.RegisterType<Functions>().As<Functions>();
            builder.RegisterType<ClientClassFactory>().As<ClientClassFactory>();
            builder.RegisterType<RealmServerClassFactory>().As<RealmServerClassFactory>();
        }
    }
}