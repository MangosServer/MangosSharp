using Autofac;
using Mangos.Loggers;
using Mangos.Loggers.Console;

namespace WorldCluster.Modules
{
    public class LoggerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ConsoleLogger>().As<ILogger>().SingleInstance();
        }
    }
}
