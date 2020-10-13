using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Mangos.Configuration.Xml
{
    public class XmlFileConfigurationProvider<T> : IConfigurationProvider<T>
    {
        private readonly string _filePath;

        public XmlFileConfigurationProvider(string filePath)
        {
            _filePath = filePath;
        }

        public T GetConfiguration()
        {
            using (var streamReader = new StreamReader(_filePath))
            {
                var xmlSerializer = new XmlSerializer(typeof(T));
                T configuration = (T)xmlSerializer.Deserialize(streamReader);
                return configuration;
            }
        }
    }
}