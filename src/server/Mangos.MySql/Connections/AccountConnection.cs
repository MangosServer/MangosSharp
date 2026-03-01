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

using Dapper;
using Mangos.Logging;
using MySql.Data.MySqlClient;
using System.Collections.Concurrent;
using System.Data;

namespace Mangos.MySql.Connections;

internal sealed class AccountConnection : IDisposable
{
    private readonly MySqlConnection mySqlConnection;
    private readonly IMangosLogger logger;
    private readonly SemaphoreSlim connectionLock = new(1, 1);
    private readonly ConcurrentDictionary<object, string> scripts = new();

    public AccountConnection(MySqlConnection mySqlConnection, IMangosLogger logger)
    {
        this.mySqlConnection = mySqlConnection;
        this.logger = logger;
    }

    public async Task<IEnumerable<T>?> QueryAsync<T>(object target)
    {
        var queryName = target.GetType().Name;
        logger.Trace($"[DB] QueryAsync<{typeof(T).Name}> starting for {queryName}");
        var script = scripts.GetOrAdd(target, GetSqlScriptFromResources);
        logger.Trace($"[DB] Acquired SQL script for {queryName}, waiting for connection lock");
        await connectionLock.WaitAsync();
        try
        {
            await EnsureConnectionOpenAsync();
            logger.Trace($"[DB] Executing query for {queryName}");
            var result = await mySqlConnection.QueryAsync<T>(script);
            var resultList = result?.ToList();
            logger.Debug($"[DB] Query {queryName} returned {resultList?.Count ?? 0} row(s)");
            return resultList;
        }
        catch (MySqlException ex)
        {
            logger.Error(ex, $"[DB] Database query failed for {queryName} - MySQL error: {ex.Number}, SQL state: {ex.SqlState}");
            throw;
        }
        finally
        {
            connectionLock.Release();
            logger.Trace($"[DB] Connection lock released for {queryName}");
        }
    }

    public async Task<T?> QueryFirstOrDefaultAsync<T>(object target, object arguments)
    {
        var queryName = target.GetType().Name;
        logger.Trace($"[DB] QueryFirstOrDefaultAsync<{typeof(T).Name}> starting for {queryName}");
        var script = scripts.GetOrAdd(target, GetSqlScriptFromResources);
        logger.Trace($"[DB] Acquired SQL script for {queryName}, waiting for connection lock");
        await connectionLock.WaitAsync();
        try
        {
            await EnsureConnectionOpenAsync();
            logger.Trace($"[DB] Executing single-row query for {queryName}");
            var result = await mySqlConnection.QueryFirstOrDefaultAsync<T>(script, arguments);
            logger.Debug($"[DB] Query {queryName} returned {(result != null ? "a result" : "null")}");
            return result;
        }
        catch (MySqlException ex)
        {
            logger.Error(ex, $"[DB] Database query failed for {queryName} - MySQL error: {ex.Number}, SQL state: {ex.SqlState}");
            throw;
        }
        finally
        {
            connectionLock.Release();
            logger.Trace($"[DB] Connection lock released for {queryName}");
        }
    }

    public async Task ExecuteAsync(object target, object arguments)
    {
        var commandName = target.GetType().Name;
        logger.Trace($"[DB] ExecuteAsync starting for {commandName}");
        var script = scripts.GetOrAdd(target, GetSqlScriptFromResources);
        logger.Trace($"[DB] Acquired SQL script for {commandName}, waiting for connection lock");
        await connectionLock.WaitAsync();
        try
        {
            await EnsureConnectionOpenAsync();
            logger.Trace($"[DB] Executing command for {commandName}");
            await mySqlConnection.ExecuteAsync(script, arguments);
            logger.Debug($"[DB] Command {commandName} executed successfully");
        }
        catch (MySqlException ex)
        {
            logger.Error(ex, $"[DB] Database execute failed for {commandName} - MySQL error: {ex.Number}, SQL state: {ex.SqlState}");
            throw;
        }
        finally
        {
            connectionLock.Release();
            logger.Trace($"[DB] Connection lock released for {commandName}");
        }
    }

    private async Task EnsureConnectionOpenAsync()
    {
        logger.Trace($"[DB] Connection state check: {mySqlConnection.State}");
        if (mySqlConnection.State == ConnectionState.Broken || mySqlConnection.State == ConnectionState.Closed)
        {
            logger.Warning($"[DB] Database connection was {mySqlConnection.State}, reconnecting...");
            await mySqlConnection.OpenAsync();
            logger.Information($"[DB] Database reconnection successful, new state: {mySqlConnection.State}");
        }
    }

    private string GetSqlScriptFromResources(object target)
    {
        var type = target.GetType();
        var resourceName = $"{type.FullName}.sql";
        logger.Trace($"[DB] Loading SQL script from embedded resource: {resourceName}");
        using var stream = type.Assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            logger.Error($"[DB] SQL script resource not found: {resourceName}");
            throw new FileNotFoundException($"Unable to get sql script for {type.FullName}");
        }
        using var streamReader = new StreamReader(stream);
        var script = streamReader.ReadToEnd();
        logger.Debug($"[DB] Loaded SQL script for {type.Name} ({script.Length} chars)");
        return script;
    }

    public void Dispose()
    {
        logger.Debug("[DB] Disposing AccountConnection");
        connectionLock.Dispose();
        mySqlConnection.Dispose();
        logger.Trace("[DB] AccountConnection disposed");
    }
}
