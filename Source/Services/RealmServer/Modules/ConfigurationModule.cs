using Autofac;
using Mangos.Configuration;
using Mangos.Configuration.Xml;
using Mangos.Realm.Configuration;

namespace RealmServer.Modules
{
    public class ConfigurationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<XmlConfigurationProvider<RealmServerConfiguration>>()
                .As<IConfigurationProvider<RealmServerConfiguration>>()
                .AsSelf()
                .SingleInstance();
        }
    }
}
