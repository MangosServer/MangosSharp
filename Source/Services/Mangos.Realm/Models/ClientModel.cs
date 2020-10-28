using System.Net;

namespace Mangos.Realm.Models
{
    public class ClientModel
    {
        public string AccountName { get; set; }
        public ClientAuthEngine ClientAuthEngine { get; }
        public IPEndPoint RemoteEnpoint { get; }

        public ClientModel(IPEndPoint remoteEnpoint)
        {
            RemoteEnpoint = remoteEnpoint;
            ClientAuthEngine = new ClientAuthEngine();
        }
    }
}
