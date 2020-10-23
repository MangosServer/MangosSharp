using Autofac;
using Mangos.Network.Tcp;
using Mangos.Network.Tcp.Mvc;
using Mangos.Realm.Network.Handlers;
using Mangos.Realm.Network.Readers;
using Mangos.Realm.Network.Requests;
using Mangos.Realm.Network.Responses;
using Mangos.Realm.Network.Writers;

namespace Mangos.Realm.Network
{
    public class RealmNetworkModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<TcpServer>().AsSelf().SingleInstance();
            builder.RegisterType<TcpClientFactroy>().As<ITcpClientFactory>().SingleInstance();
            builder.RegisterType<PipelineMap>().AsSelf().SingleInstance();

            RegisterReaders(builder);
            RegisterWriters(builder);
            RegisterHandlers(builder);
            RegisterPipelines(builder);
        }

        private void RegisterReaders(ContainerBuilder builder)
        {
            builder.RegisterType<RS_LOGON_CHALLENGE_Reader>()
                .As<IPacketReader<RS_LOGON_CHALLENGE_Reader>>()
                .SingleInstance();
        }

        private void RegisterWriters(ContainerBuilder builder)
        {
            builder.RegisterType<AUTH_LOGON_PROOF_Writer>()
                .As<IPacketWriter<AUTH_LOGON_PROOF>>()
                .SingleInstance();
        }

        private void RegisterHandlers(ContainerBuilder builder)
        {
            builder.RegisterType<RS_LOGON_CHALLENGE_Handler>()
                .As<IPacketHandler<RS_LOGON_CHALLENGE, ClientContext, AUTH_LOGON_PROOF>>()
                .SingleInstance();
        }

        private void RegisterPipelines(ContainerBuilder builder)
        {
            builder.RegisterType<Pipeline<RS_LOGON_CHALLENGE, ClientContext, AUTH_LOGON_PROOF>>()
                .AsSelf()
                .SingleInstance();
        }
    }
}
