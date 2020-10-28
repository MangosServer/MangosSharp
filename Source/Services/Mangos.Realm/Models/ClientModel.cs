namespace Mangos.Realm.Models
{
    public class ClientModel
    {
        public string AccountName { get; set; }
        public ClientAuthEngine ClientAuthEngine { get; }

        public ClientModel()
        {
            ClientAuthEngine = new ClientAuthEngine();
        }
    }
}
