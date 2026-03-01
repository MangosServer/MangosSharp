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

namespace Mangos.MySql.IsBannedAccount;

internal sealed class IsBannedAccountQuery : IIsBannedAccountQuery
{
    private readonly AccountConnection accountConnection;
    private readonly IMangosLogger logger;

    public IsBannedAccountQuery(AccountConnection accountConnection, IMangosLogger logger)
    {
        this.accountConnection = accountConnection;
        this.logger = logger;
        logger.Trace("[DB] IsBannedAccountQuery instance created");
    }

    public async Task<bool> ExecuteAsync(string id)
    {
        logger.Debug($"[DB] Checking ban status for account id={id}");
        var count = await accountConnection.QueryFirstOrDefaultAsync<int>(this, new { Id = id });
        var isBanned = count > 0;
        logger.Debug($"[DB] Account id={id} ban check result: {(isBanned ? "BANNED" : "not banned")} (count={count})");
        return isBanned;
    }
}
