using Autofac;
using Mangos.Configuration;
using Mangos.Configuration.Store;
using Mangos.Configuration.Xml;
using Mangos.Loggers;
using Mangos.Realm;

namespace RealmServer.Modules
{
    public class ConfigurationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(x => new XmlFileConfigurationProvider<RealmServerConfiguration>(
                    x.Resolve<ILogger>(), "configs/RealmServer.ini"))
                .As<IConfigurationProvider<RealmServerConfiguration>>()
                .SingleInstance();
            builder.RegisterDecorator<StoredConfigurationProvider<RealmServerConfiguration>, 
                IConfigurationProvider<RealmServerConfiguration>>();
        }
    }
}
