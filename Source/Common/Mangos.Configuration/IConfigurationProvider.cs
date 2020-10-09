using System.Threading.Tasks;

namespace Mangos.Configuration
{
    public interface IConfigurationProvider<T>
    {
        Task<T> GetConfigurationAsync();
    }
}