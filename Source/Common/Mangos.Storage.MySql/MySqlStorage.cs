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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dapper;
using Mangos.Loggers;
using MySql.Data.MySqlClient;

namespace Mangos.Storage.MySql
{
    public abstract class MySqlStorage : IAsyncDisposable
    {
        private readonly ILogger logger;

        private MySqlConnection connection;
        private readonly Dictionary<string, string> queries;

        protected MySqlStorage(ILogger logger)
        {
            this.logger = logger;
            queries = GetEmbeddedQueries();
        }

        public async Task ConnectAsync(string conenctionString)
        {
            if (connection != null)
            {
                logger.Error("MySql connection has already opened");
                throw new Exception("MySql connection has already opened");
            }
            try
            {
                connection = new MySqlConnection(conenctionString);
                await connection.OpenAsync();
                logger.Debug("MySql connection for {0} database has been opened", connection.Database);
            }
            catch (Exception ex)
            {
                logger.Error("Unable to open MySql conenction", ex);
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (connection != null)
            {
                await connection.DisposeAsync();
            }
        }

        private Dictionary<string, string> GetEmbeddedQueries()
        {
            var type = GetType();
            var queriesCatalog = $"{type.Namespace}.Queries";
            var assembly = type.Assembly;
            var resources = assembly.GetManifestResourceNames()
                .Where(x => x.StartsWith(queriesCatalog))
                .ToDictionary(
                    x => GetEmbeddedSqlResourceName(queriesCatalog, x), 
                    x => GetEmbeddedSqlResourcebody(assembly , x));
            return resources;
        }

        private string GetEmbeddedSqlResourcebody(Assembly assembly, string resource)
        {
            using var stream = assembly.GetManifestResourceStream(resource);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private string GetSqlScript(string name)
        {
            if (queries.ContainsKey(name))
            {
                return queries[name];
            }
            throw new Exception($"Unknown sql script {name}");
        }

        private string GetEmbeddedSqlResourceName(string queriesCatalog, string resource)
        {
            return Regex.Split(resource, $"{queriesCatalog}.(.*).sql")[1];
        }

        protected async Task<T> QuerySingleAsync<T>(
            object parameters,
            [CallerMemberName] string callerMemberName = null)
        {
            try
            {
                var result = await connection.QuerySingleAsync<T>(GetSqlScript(callerMemberName), parameters);
                logger.Debug($"QuerySingleAsync for {callerMemberName} has beed successfuly executed");
                return result;
            }
            catch (Exception ex)
            {
                logger.Error($"Unable to execute {callerMemberName} script with args {0}", ex, JsonSerializer.Serialize(parameters));
                throw;
            }
        }

        protected async Task<T> QuerySingleOrDefaultAsync<T>(
            object parameters,
            [CallerMemberName] string callerMemberName = null)
        {
            try
            {
                var result = await connection.QuerySingleOrDefaultAsync<T>(GetSqlScript(callerMemberName), parameters);
                logger.Debug($"QuerySingleOrDefaultAsync for {callerMemberName} has beed successfuly executed");
                return result;
            }
            catch (Exception ex)
            {
                logger.Error($"Unable to execute {callerMemberName} script with args {0}", ex, JsonSerializer.Serialize(parameters));
                throw;
            }
        }

        protected async Task<T> QueryFirstOrDefaultAsync<T>(
            object parameters,
            [CallerMemberName] string callerMemberName = null)
        {
            try
            {
                var result = await connection.QueryFirstOrDefaultAsync<T>(GetSqlScript(callerMemberName), parameters);
                logger.Debug($"QueryFirstOrDefaultAsync for {callerMemberName} has beed successfuly executed");
                return result;
            }
            catch (Exception ex)
            {
                logger.Error($"Unable to execute {callerMemberName} script with args {0}", ex, JsonSerializer.Serialize(parameters));
                throw;
            }
        }

        protected async Task<List<T>> QueryAsync<T>(
            object parameters,
            [CallerMemberName] string callerMemberName = null)
        {
            try
            {
                var result = await connection.QueryAsync<T>(GetSqlScript(callerMemberName), parameters);
                logger.Debug($"QueryAsync for {callerMemberName} has beed successfuly executed");
                return result.ToList();
            }
            catch (Exception ex)
            {
                logger.Error($"Unable to execute {callerMemberName} script with args {0}", ex, JsonSerializer.Serialize(parameters));
                throw;
            }
        }

        protected async Task QueryAsync(
           object parameters,
           [CallerMemberName] string callerMemberName = null)
        {
            try
            {
                await connection.QueryAsync(GetSqlScript(callerMemberName), parameters);
                logger.Debug($"QueryAsync for {callerMemberName} has beed successfuly executed");
            }
            catch (Exception ex)
            {
                logger.Error($"Unable to execute {callerMemberName} script with args {0}", ex, JsonSerializer.Serialize(parameters));
                throw;
            }
        }
    }
}
