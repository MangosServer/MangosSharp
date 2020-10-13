using System.Net;
using System.Threading.Tasks;

namespace Mangos.Storage
{
    public interface IAccountStorage
    {
        Task<bool> IsBannedAsync(IPAddress address);
    }
}
