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

using Dapper;
using MySql.Data.MySqlClient;

namespace Mangos.MySql.Implementation.Connections;

internal sealed class AccountConnection
{
    private readonly MySqlConnection mySqlConnection;
    private readonly Dictionary<string, string> scripts;

    public AccountConnection(MySqlConnection mySqlConnection, Dictionary<string, string> scripts)
    {
        this.mySqlConnection = mySqlConnection;
        this.scripts = scripts;
    }

    public async Task<IEnumerable<T>?> QueryAsync<T>(string script)
    {
        if (!scripts.TryGetValue(script, out var command))
        {
            throw new Exception($"Unable to get sql script {script}");
        }
        return await mySqlConnection.QueryAsync<T>(command);
    }

    public async Task<T?> QueryFirstOrDefaultAsync<T>(string script, object arguments)
    {
        if (!scripts.TryGetValue(script, out var command))
        {
            throw new Exception($"Unable to get sql script {script}");
        }
        return await mySqlConnection.QueryFirstOrDefaultAsync<T>(command, arguments);
    }

    public async Task ExecuteAsync(string script, object arguments)
    {
        if (!scripts.TryGetValue(script, out var command))
        {
            throw new Exception($"Unable to get sql script {script}");
        }
        await mySqlConnection.QueryAsync(command, arguments);
    }
}
