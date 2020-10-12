using System.Threading.Tasks;

namespace Mangos.Configuration.Store
{
    public class StoredConfigurationProvider<T> : IConfigurationProvider<T>
    {
        private readonly IConfigurationProvider<T> _configurationProvider;
        private T _configuration;

        public StoredConfigurationProvider(IConfigurationProvider<T> configurationProvider)
        {
            _configurationProvider = configurationProvider;
        }

        public T GetConfiguration()
        {
            if (_configuration is null)
            {
                _configuration = _configurationProvider.GetConfiguration();
            }
            return _configuration;
        }
    }
}