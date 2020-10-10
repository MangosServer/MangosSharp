using global;
using Mangos.Configuration;

namespace Mangos.Realm.Factories
{
    public class RealmServerClassFactory
    {
        private readonly Global_Constants _Global_Constants;
        private readonly ClientClassFactory _ClientClassFactory;
        private readonly IConfigurationProvider<RealmServerConfiguration> _configurationProvider;

        public RealmServerClassFactory(Global_Constants globalConstants, ClientClassFactory clientClassFactory, IConfigurationProvider<RealmServerConfiguration> configurationProvider)
        {
            _Global_Constants = globalConstants;
            _ClientClassFactory = clientClassFactory;
            _configurationProvider = configurationProvider;
        }

        public RealmServerClass Create(RealmServer realmServer)
        {
            return new RealmServerClass(_Global_Constants, _ClientClassFactory, realmServer, _configurationProvider);
        }
    }
}