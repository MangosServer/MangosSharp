
namespace Mangos.Realm.Factories
{
    public class ClientClassFactory
    {
        private readonly Global_Constants _Global_Constants;

        public ClientClassFactory(Global_Constants globalConstants)
        {
            _Global_Constants = globalConstants;
        }

        public ClientClass Create(RealmServer realmServer)
        {
            return new ClientClass(_Global_Constants, realmServer);
        }
    }
}