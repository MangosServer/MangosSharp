//
// Copyright (C) 2013-2021 getMaNGOS <https://getmangos.eu>
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

using System.Collections.Generic;
using System.Threading.Tasks;
using Mangos.Storage.Account.Results;

namespace Mangos.Storage.Account
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
