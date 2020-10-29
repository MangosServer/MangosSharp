using Mangos.Loggers;
using Mangos.Realm.Storage.Entities;
using Mangos.Storage.Account;
using Mangos.Storage.MySql;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mangos.Realm.Storage.MySql
{
    public class RealmStorage : MySqlStorage, IRealmStorage
    {
        public RealmStorage(ILogger logger) : base(logger)
        {
        }

        public async Task<bool> IsBannedAccountAsync(string id)
        {
            var count = await QuerySingleAsync<int>(new
            {
                Id = id
            });
            return count > 0;
        }

        public async Task<AccountInfoEntity> GetAccountInfoAsync(string username)
        {
            return await QueryFirstOrDefaultAsync<AccountInfoEntity>(new
            {
                Username = username
            });
        }

        public async Task<List<RealmListItemEntitiy>> GetRealmListAsync()
        {
            return await QueryAsync<RealmListItemEntitiy>(null);
        }

        public async Task UpdateAccountAsync(string sessionkey, string last_ip, string last_login, string username)
        {
            await QueryAsync(new
            {
                Sessionkey = sessionkey,
                Last_ip = last_ip,
                Last_login = last_login,
                Username = username
            });
        }
    }
}
