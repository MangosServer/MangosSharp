using Autofac;
using Mangos.Cluster.Factories;
using Mangos.Network.Tcp;

namespace WorldCluster.Modules
{
    public class TcpServerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<TcpServer>().AsSelf().SingleInstance();
            builder.RegisterType<ClientClassFactory>().As<ITcpClientFactory>().SingleInstance();
        }
    }
}
