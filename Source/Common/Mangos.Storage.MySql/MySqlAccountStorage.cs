//
// Copyright (C) 2013-2020 getMaNGOS <https://getmangos.eu>
//
// This program is free software. You can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation. either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY. Without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//

using Mangos.Loggers;
using Mangos.Storage.Account.Responses;
using Mangos.Storage.Account.Results;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mangos.Storage.MySql
{
    public class MySqlAccountStorage : MySqlStorage, IAccountStorage, IAsyncDisposable
    {
        public MySqlAccountStorage(ILogger logger) : base(logger, "Account")
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

        public async Task<AccountInfo> GetAccountInfoAsync(string username)
        {
            return await QueryFirstOrDefaultAsync<AccountInfo>(new
            {
                Username = username
            });
        }

        public async Task<List<RealmListItem>> GetRealmListAsync()
        {
            return await QueryAsync<RealmListItem>(null);
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

        public async Task<int> GetNumcharsAsync(string realmId)
        {
            return await QuerySingleOrDefaultAsync<int>(new
            {
                Realmid = realmId
            });
        }
    }
}
