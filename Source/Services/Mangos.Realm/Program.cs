//
//  Copyright (C) 2013-2020 getMaNGOS <https:\\getmangos.eu>
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
using global;
using Mangos.Common.Globals;
using Mangos.Configuration;
using Mangos.Configuration.Store;
using Mangos.Configuration.Xml;
using Mangos.Loggers;
using Mangos.Loggers.Console;
using Mangos.Realm.Factories;

namespace Mangos.Realm
{
	public static class Program
	{
		public static void Main()
		{
			var container = CreateContainer();
			var realmServer = container.Resolve<RealmServer>();
			realmServer.Start();
		}

		public static IContainer CreateContainer()
		{
			var builder = new ContainerBuilder();
			RegisterLoggers(builder);
			RegisterConfiguration(builder);
			RegisterServices(builder);
			return builder.Build();
		}

		public static void RegisterConfiguration(ContainerBuilder builder)
		{
			builder.Register(x => new XmlFileConfigurationProvider<RealmServerConfiguration>("configs/RealmServer.ini"))
				.As<IConfigurationProvider<RealmServerConfiguration>>();
			builder.RegisterDecorator<StoredConfigurationProvider<RealmServerConfiguration>, 
				IConfigurationProvider<RealmServerConfiguration>>();
		}

		public static void RegisterLoggers(ContainerBuilder builder)
		{
			builder.RegisterType<ConsoleLogger>().As<ILogger>();
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