using Autofac;
using Mangos.Realm;

namespace RealmServer.Modules
{
    public class RealmModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Converter>().As<Converter>().SingleInstance();

            builder.RegisterType<Startup>().AsSelf().SingleInstance();
        }
    }
}
