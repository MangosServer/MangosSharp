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

namespace Mangos.MySql.GetAccountInfo;

internal sealed class GetAccountInfoQuery : IGetAccountInfoQuery
{
    private readonly AccountConnection accountConnection;
    private readonly IMangosLogger logger;

    public GetAccountInfoQuery(AccountConnection accountConnection, IMangosLogger logger)
    {
        this.accountConnection = accountConnection;
        this.logger = logger;
        logger.Trace("[DB] GetAccountInfoQuery instance created");
    }

    public async Task<AccountInfoModel?> ExectueAsync(string accountName)
    {
        logger.Debug($"[DB] Querying account info for '{accountName}'");
        var result = await accountConnection.QueryFirstOrDefaultAsync<AccountInfoModel>(this, new { UserName = accountName });
        if (result != null)
        {
            logger.Debug($"[DB] Account info found for '{accountName}': id={result.id}");
        }
        else
        {
            logger.Debug($"[DB] No account info found for '{accountName}'");
        }
        return result;
    }
}
