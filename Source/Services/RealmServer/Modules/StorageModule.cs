using Autofac;
using Mangos.Realm.Storage.MySql;
using Mangos.Storage.Account;

namespace RealmServer.Modules
{
    public class StorageModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AccountStorage>()
                .AsSelf()
                .As<IAccountStorage>()
                .SingleInstance();
        }
    }
}
