using Autofac;
using Mangos.Common.Globals;

namespace RealmServer.Modules
{
    public class CommonModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MangosGlobalConstants>().As<MangosGlobalConstants>().SingleInstance();
        }
    }
}
