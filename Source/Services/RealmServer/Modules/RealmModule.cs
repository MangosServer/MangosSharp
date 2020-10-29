using Autofac;
using Mangos.Realm;
using Mangos.Realm.Network;
using Mangos.Realm.Network.Handlers;
using Mangos.Realm.Network.Readers;
using Mangos.Realm.Network.Writers;

namespace RealmServer.Modules
{
    public class RealmModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Router>().AsSelf().SingleInstance();
            builder.RegisterType<RS_LOGON_CHALLENGE_Handler>().AsSelf().SingleInstance();
            builder.RegisterType<RS_LOGON_PROOF_Handler>().AsSelf().SingleInstance();
            builder.RegisterType<RS_REALMLIST_Handler>().AsSelf().SingleInstance();
            builder.RegisterType<CMD_XFER_CANCEL_Handler>().AsSelf().SingleInstance();
            builder.RegisterType<CMD_XFER_ACCEPT_Handler>().AsSelf().SingleInstance();
            builder.RegisterType<On_CMD_XFER_RESUME_Handler>().AsSelf().SingleInstance();

            builder.RegisterType<RS_LOGON_PROOF_Reader>().AsSelf().SingleInstance();
            builder.RegisterType<RS_LOGON_CHALLENGE_Reader>().AsSelf().SingleInstance();
            builder.RegisterType<CMD_XFER_RESUME_Reader>().AsSelf().SingleInstance();
            builder.RegisterType<RS_REALMLIST_Reader>().AsSelf().SingleInstance();

            builder.RegisterType<AUTH_LOGON_PROOF_Writer>().AsSelf().SingleInstance();
            builder.RegisterType<AUTH_LOGON_CHALLENGE_Writer>().AsSelf().SingleInstance();
            builder.RegisterType<AUTH_REALMLIST_Writer>().AsSelf().SingleInstance();

            builder.RegisterType<Startup>().AsSelf().SingleInstance();
        }
    }
}
