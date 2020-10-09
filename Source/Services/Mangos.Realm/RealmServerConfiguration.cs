using System.Xml.Serialization;

namespace Mangos.Realm
{
    [XmlRoot(ElementName = "RealmServer")]
    public class RealmServerConfiguration
    {
        public int RealmServerPort { get; set; } = 3724;
        public string RealmServerAddress { get; set; } = "127.0.0.1";
        public string AccountDatabase { get; set; } = "root;mangosVB;localhost;3306;mangosVB;MySQL";
    }
}