using Autofac;
using Mangos.DataStores;

namespace WorldCluster.Modules
{
    public class DataStoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DataStoreProvider>().AsSelf().SingleInstance();
        }
    }
}
