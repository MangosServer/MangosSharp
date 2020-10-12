using System.Threading.Tasks;

namespace Mangos.Configuration
{
    public interface IConfigurationProvider<T>
    {
        T GetConfiguration();
    }
}