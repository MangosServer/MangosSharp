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

using System.Collections.Concurrent;
using System.Data;
using Dapper;
using Mangos.Logging;
using MySqlConnector;

namespace Mangos.MySql.Connections;

public sealed class CharacterConnection : IDisposable
{
    private readonly MySqlConnection mySqlConnection;
    private readonly IMangosLogger logger;
    private readonly SemaphoreSlim connectionLock = new(1, 1);
    private readonly ConcurrentDictionary<object, string> scripts = new();

    public MySqlConnection MySqlConnection => mySqlConnection;

    public CharacterConnection(MySqlConnection mySqlConnection, IMangosLogger logger)
    {
        this.mySqlConnection = mySqlConnection;
        this.logger = logger;
    }

    public async Task<IEnumerable<T>?> QueryAsync<T>(object target)
    {
        var script = scripts.GetOrAdd(target, GetSqlScriptFromResources);
        await connectionLock.WaitAsync();
        try
        {
            await EnsureConnectionOpenAsync();
            return await mySqlConnection.QueryAsync<T>(script);
        }
        catch (MySqlException ex)
        {
            logger.Error(ex, $"Database query failed for {target.GetType().Name}");
            throw;
        }
        finally
        {
            connectionLock.Release();
        }
    }

    public async Task<T?> QueryFirstOrDefaultAsync<T>(object target, object arguments)
    {
        var script = scripts.GetOrAdd(target, GetSqlScriptFromResources);
        await connectionLock.WaitAsync();
        try
        {
            await EnsureConnectionOpenAsync();
            return await mySqlConnection.QueryFirstOrDefaultAsync<T>(script, arguments);
        }
        catch (MySqlException ex)
        {
            logger.Error(ex, $"Database query failed for {target.GetType().Name} with arguments {arguments}");
            throw;
        }
        finally
        {
            connectionLock.Release();
        }
    }

    public async Task ExecuteAsync(object target, object arguments)
    {
        var script = scripts.GetOrAdd(target, GetSqlScriptFromResources);
        await connectionLock.WaitAsync();
        try
        {
            await EnsureConnectionOpenAsync();
            await mySqlConnection.ExecuteAsync(script, arguments);
        }
        catch (MySqlException ex)
        {
            logger.Error(ex, $"Database command execution failed for {target.GetType().Name} with arguments {arguments}");
            throw;
        }
        finally
        {
            connectionLock.Release();
        }
    }

    private async Task EnsureConnectionOpenAsync()
    {
        if (mySqlConnection.State != ConnectionState.Open)
        {
            logger.Debug("Opening database connection");
            await mySqlConnection.OpenAsync();
        }
    }

    private string GetSqlScriptFromResources(object target)
    {
        var resourceName = $"{target.GetType().FullName}.sql";
        var assembly = target.GetType().Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new InvalidOperationException($"SQL script resource '{resourceName}' not found for {target.GetType().FullName}");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public void Dispose()
    {
        mySqlConnection.Dispose();
        connectionLock.Dispose();
    }
}
