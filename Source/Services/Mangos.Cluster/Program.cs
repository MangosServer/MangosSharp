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

using Autofac;
using global;
using Mangos.Cluster.DataStores;
using Mangos.Cluster.Globals;
using Mangos.Cluster.Handlers;
using Mangos.Cluster.Server;
using Mangos.Common;
using Mangos.Common.Zip;
using Mangos.Loggers;
using Mangos.Loggers.Console;

namespace Mangos.Cluster
{
    public static class Program
    {
        public static void Main()
        {
            ClusterServiceLocator._WorldCluster.Main();
        }

        public static IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();
			RegisterLoggers(builder);
			builder.RegisterType<MangosGlobalConstants>().As<MangosGlobalConstants>();
            builder.RegisterType<Common.Globals.Functions>().As<Common.Globals.Functions>();
            builder.RegisterType<Common.Functions>().As<Common.Functions>();
            builder.RegisterType<ZipService>().As<ZipService>();
            builder.RegisterType<NativeMethods>().As<NativeMethods>();
            builder.RegisterType<WorldCluster>().As<WorldCluster>();
            builder.RegisterType<WS_DBCDatabase>().As<WS_DBCDatabase>();
            builder.RegisterType<WS_DBCLoad>().As<WS_DBCLoad>();
            builder.RegisterType<Globals.Functions>().As<Globals.Functions>();
            builder.RegisterType<Packets>().As<Packets>();
            builder.RegisterType<WC_Guild>().As<WC_Guild>();
            builder.RegisterType<WC_Stats>().As<WC_Stats>();
            builder.RegisterType<WC_Network>().As<WC_Network>();
            builder.RegisterType<WC_Handlers>().As<WC_Handlers>();
            builder.RegisterType<WC_Handlers_Auth>().As<WC_Handlers_Auth>();
            builder.RegisterType<WC_Handlers_Battleground>().As<WC_Handlers_Battleground>();
            builder.RegisterType<WC_Handlers_Chat>().As<WC_Handlers_Chat>();
            builder.RegisterType<WC_Handlers_Group>().As<WC_Handlers_Group>();
            builder.RegisterType<WC_Handlers_Guild>().As<WC_Handlers_Guild>();
            builder.RegisterType<WC_Handlers_Misc>().As<WC_Handlers_Misc>();
            builder.RegisterType<WC_Handlers_Movement>().As<WC_Handlers_Movement>();
            builder.RegisterType<WC_Handlers_Social>().As<WC_Handlers_Social>();
            builder.RegisterType<WC_Handlers_Tickets>().As<WC_Handlers_Tickets>();
            builder.RegisterType<WS_Handler_Channels>().As<WS_Handler_Channels>();
            builder.RegisterType<WcHandlerCharacter>().As<WcHandlerCharacter>();
            return builder.Build();
		}

		public static void RegisterLoggers(ContainerBuilder builder)
		{
			builder.RegisterType<ConsoleLogger>().As<ILogger>();
		}
	}
}