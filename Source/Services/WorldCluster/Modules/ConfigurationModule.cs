using Autofac;
using Mangos.Cluster.Configuration;
using Mangos.Configuration;
using Mangos.Configuration.Xml;

namespace WorldCluster.Modules
{
    public class ConfigurationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<XmlConfigurationProvider<ClusterConfiguration>>()
                .As<IConfigurationProvider<ClusterConfiguration>>()
                .AsSelf()
                .SingleInstance();
        }
    }
}
