//
// Copyright (C) 2013-2022 getMaNGOS <https://getmangos.eu>
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

using Mangos.Configuration;
using Mangos.Realm.Configuration;
using Mangos.Realm.Storage.Entities;
using Mangos.Storage.Account;
using Mangos.Storage.MySql;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mangos.Realm.Storage.MySql;

public class AccountStorage : IAccountStorage
{
    private readonly IConfigurationProvider<RealmServerConfiguration> configurationProvider;
    private readonly MySqlStorage mySqlStorage;

    public AccountStorage(IConfigurationProvider<RealmServerConfiguration> configurationProvider)
    {
        this.configurationProvider = configurationProvider;
        mySqlStorage = new MySqlStorage();
    }

    public async Task ConnectAsync()
    {
        var conenctionString = configurationProvider.GetConfiguration().AccountConnectionString;
        await mySqlStorage.ConnectAsync(this, conenctionString);
    }

    public async Task<bool> IsBannedAccountAsync(string id)
    {
        var count = await mySqlStorage.QuerySingleAsync<int>(new
        {
            Id = id
        });
        return count > 0;
    }

    public async Task<AccountInfoEntity> GetAccountInfoAsync(string username)
    {
        return await mySqlStorage.QueryFirstOrDefaultAsync<AccountInfoEntity>(new
        {
            Username = username
        });
    }

    public async Task<List<RealmListItemEntitiy>> GetRealmListAsync()
    {
        return await mySqlStorage.QueryListAsync<RealmListItemEntitiy>(null);
    }

    public async Task UpdateAccountAsync(string sessionkey, string last_ip, string last_login, string username)
    {
        await mySqlStorage.QueryAsync(new
        {
            Sessionkey = sessionkey,
            Last_ip = last_ip,
            Last_login = last_login,
            Username = username
        });
    }
}
