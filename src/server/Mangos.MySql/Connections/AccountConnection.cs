//
// Copyright (C) 2013-2023 getMaNGOS <https://getmangos.eu>
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
using System.Collections.Concurrent;

namespace Mangos.MySql.Connections;

internal sealed class AccountConnection
{
    private readonly MySqlConnection mySqlConnection;
    private readonly ConcurrentDictionary<object, string> scripts = new();

    public AccountConnection(MySqlConnection mySqlConnection)
    {
        this.mySqlConnection = mySqlConnection;
    }

    public async Task<IEnumerable<T>?> QueryAsync<T>(object target)
    {
        var script = scripts.GetOrAdd(target, GetSqlScriptFromResources);
        return await mySqlConnection.QueryAsync<T>(script);
    }

    public async Task<T?> QueryFirstOrDefaultAsync<T>(object target, object arguments)
    {
        var script = scripts.GetOrAdd(target, GetSqlScriptFromResources);
        return await mySqlConnection.QueryFirstOrDefaultAsync<T>(script, arguments);
    }

    public async Task ExecuteAsync(object target, object arguments)
    {
        var script = scripts.GetOrAdd(target, GetSqlScriptFromResources);
        await mySqlConnection.QueryAsync(script, arguments);
    }

    private string GetSqlScriptFromResources(object target)
    {
        var type = target.GetType();
        using var stream = type.Assembly.GetManifestResourceStream($"{type.FullName}.sql") ?? throw new FileNotFoundException($"Unable to get sql script for {type.FullName}");
        using var streamReader = new StreamReader(stream);
        return streamReader.ReadToEnd();
    }
}
