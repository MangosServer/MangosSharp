//
// Copyright (C) 2013-2025 getMaNGOS <https://www.getmangos.eu>
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

using Mangos.Logging;
using Mangos.MySql.Connections;

namespace Mangos.MySql.GetRealmList;

internal sealed class GetRealmListQuery : IGetRealmListQuery
{
    private readonly AccountConnection accountConnection;
    private readonly IMangosLogger logger;

    public GetRealmListQuery(AccountConnection accountConnection, IMangosLogger logger)
    {
        this.accountConnection = accountConnection;
        this.logger = logger;
        logger.Trace("[DB] GetRealmListQuery instance created");
    }

    public async Task<IEnumerable<RealmListModel>?> ExectueAsync()
    {
        logger.Debug("[DB] Querying realm list from database");
        var result = await accountConnection.QueryAsync<RealmListModel>(this);
        var count = result?.Count() ?? 0;
        logger.Debug($"[DB] Realm list query returned {count} realm(s)");
        return result;
    }
}
