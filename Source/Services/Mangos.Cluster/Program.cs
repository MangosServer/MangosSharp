using Autofac;
using Mangos.Cluster.DataStores;
using Mangos.Cluster.Globals;
using Mangos.Cluster.Handlers;
using Mangos.Cluster.Server;
using Mangos.Common;
using Mangos.Common.Globals;

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
            builder.RegisterType<Global_Constants>().As<Global_Constants>();
            builder.RegisterType<Common.Globals.Functions>().As<Common.Globals.Functions>();
            builder.RegisterType<Common.Functions>().As<Common.Functions>();
            builder.RegisterType<GlobalZip>().As<GlobalZip>();
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
    }
}