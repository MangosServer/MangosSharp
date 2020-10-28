using Autofac;
using Mangos.Network.Tcp;
using Mangos.Realm.Network;

namespace RealmServer.Modules
{
    public class TcpServerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<TcpServer>().AsSelf().SingleInstance();
            builder.RegisterType<RealmTcpClientFactory>().As<ITcpClientFactory>().SingleInstance();
        }
    }
}
