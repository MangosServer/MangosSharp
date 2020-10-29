using Autofac;
using Mangos.Cluster;
using Mangos.Cluster.DataStores;
using Mangos.Cluster.Globals;
using Mangos.Cluster.Handlers;
using Mangos.Cluster.Handlers.Guild;
using Mangos.Cluster.Network;
using Mangos.Cluster.Stats;
using Mangos.Common.Globals;
using Mangos.Common.Legacy;
using Mangos.Zip;

namespace WorldCluster.Modules
{
    public class ClusterModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MangosGlobalConstants>().As<MangosGlobalConstants>().SingleInstance();
            builder.RegisterType<Mangos.Common.Legacy.Globals.Functions>().As<Mangos.Common.Legacy.Globals.Functions>().SingleInstance();
            builder.RegisterType<Mangos.Common.Legacy.Functions>().As<Mangos.Common.Legacy.Functions>().SingleInstance();
            builder.RegisterType<ZipService>().As<ZipService>().SingleInstance();
            builder.RegisterType<NativeMethods>().As<NativeMethods>().SingleInstance();
            builder.RegisterType<Mangos.Cluster.WorldCluster>().As<Mangos.Cluster.WorldCluster>().SingleInstance();
            builder.RegisterType<WorldServerClass>().As<WorldServerClass>().SingleInstance();
            builder.RegisterType<WsDbcDatabase>().As<WsDbcDatabase>().SingleInstance();
            builder.RegisterType<WsDbcLoad>().As<WsDbcLoad>().SingleInstance();
            builder.RegisterType<Mangos.Cluster.Globals.Functions>().As<Mangos.Cluster.Globals.Functions>().SingleInstance();
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
