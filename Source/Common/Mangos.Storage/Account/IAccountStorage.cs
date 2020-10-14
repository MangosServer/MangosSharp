using Mangos.Storage.Account.Responses;
using Mangos.Storage.Account.Results;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mangos.Storage
{
    public interface IAccountStorage
    {
        Task<bool> IsBannedAccountAsync(string id);

        Task<AccountInfo> GetAccountInfoAsync(string accountName);

        Task<List<RealmListItem>> GetRealmListAsync();

        Task UpdateAccountAsync(string sessionkey, string last_ip, string last_login, string username);

        Task<int> GetNumcharsAsync(string realmId);
    }
}
