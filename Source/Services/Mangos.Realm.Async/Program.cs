using Autofac;
using global;
using Mangos.Common.Globals;
using Mangos.Configuration;
using Mangos.Configuration.Store;
using Mangos.Configuration.Xml;
using Mangos.Loggers;
using Mangos.Loggers.Console;
using Mangos.Realm.Async.Factories;
using Mangos.Storage;
using Mangos.Storage.MySql;
using System.Threading.Tasks;

namespace Mangos.Realm.Async
{
    public class Program
	{
		public async static Task Main(string[] args)
		{
			var container = CreateContainer();
			var realmServer = container.Resolve<RealmServer>();
			await realmServer.StartAsync();
		}

		public static IContainer CreateContainer()
		{
			var builder = new ContainerBuilder();
			RegisterLoggers(builder);
			RegisterConfiguration(builder);
			RegisterStorages(builder);
			RegisterServices(builder);
			return builder.Build();
		}

		public static void RegisterConfiguration(ContainerBuilder builder)
		{
			builder.Register(x => new XmlFileConfigurationProvider<RealmServerConfiguration>("configs/RealmServerAsync.ini"))
				.As<IConfigurationProvider<RealmServerConfiguration>>()
				.SingleInstance();
			builder.RegisterDecorator<StoredConfigurationProvider<RealmServerConfiguration>,
				IConfigurationProvider<RealmServerConfiguration>>();
		}

		public static void RegisterLoggers(ContainerBuilder builder)
		{
			builder.RegisterType<ConsoleLogger>().As<ILogger>().SingleInstance();
		}

		public static void RegisterStorages(ContainerBuilder builder)
		{
			builder.RegisterType<MySqlAccountStorage>()
				.AsSelf()
				.As<IAccountStorage>()
				.SingleInstance();
		}

		public static void RegisterServices(ContainerBuilder builder)
		{
			builder.RegisterType<Converter>().As<Converter>().SingleInstance();
			builder.RegisterType<Global_Constants>().As<Global_Constants>().SingleInstance();

			builder.RegisterType<RealmServer>().As<RealmServer>().SingleInstance();
			builder.RegisterType<RealmServerClientFactory>().As<RealmServerClientFactory>().SingleInstance();
		}
	}
}
