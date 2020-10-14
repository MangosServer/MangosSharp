using System.Xml.Serialization;

namespace Mangos.Realm.Async
{
    [XmlRoot(ElementName = "RealmServer")]
    public class RealmServerConfiguration
    {
        public string RealmServerEndpoint { get; set; } = "127.0.0.1:3724";
        public string AccountConnectionString { get; set; } = "Server=localhost;Port=3306;User ID=root;Password=rootpass;Database=mangosVBaccounts;Compress=false;Connection Timeout=1;";
    }
}
