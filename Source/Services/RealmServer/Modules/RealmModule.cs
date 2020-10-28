using Autofac;
using Mangos.Realm;
using Mangos.Realm.Network;
using Mangos.Realm.Network.Handlers;

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

            builder.RegisterType<Converter>().As<Converter>().SingleInstance();

            builder.RegisterType<Startup>().AsSelf().SingleInstance();
        }
    }
}
