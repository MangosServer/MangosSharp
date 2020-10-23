namespace Mangos.Realm
{
    public class ClientContext
    {
        public ClientAuthEngine ClientAuthEngine { get; }

        public ClientContext()
        {
            ClientAuthEngine = new ClientAuthEngine();
        }
    }
}
