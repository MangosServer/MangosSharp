﻿//
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

using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mangos.Storage.MySql
{
    public class MySqlStorage : IAsyncDisposable
    {
        private MySqlConnection connection;
        private Dictionary<string, string> queries;

        public async Task ConnectAsync(object queriesTarget, string conenctionString)
        {
            if (connection != null)
            {
                throw new Exception("MySql connection has already been opened");
            }

            connection = new MySqlConnection(conenctionString);
            Task conenctionTask = connection.OpenAsync();
            queries = GetEmbeddedQueries(queriesTarget);
            await conenctionTask;
        }

        public async ValueTask DisposeAsync()
        {
            if (connection != null)
            {
                await connection.DisposeAsync();
            }
        }

        private Dictionary<string, string> GetEmbeddedQueries(object executor)
        {
            Type type = executor.GetType();
            Assembly assembly = type.Assembly;
            string queriesCatalog = $"{type.Namespace}.Queries";

            Dictionary<string, string> resources = assembly.GetManifestResourceNames()
                .Where(x => x.StartsWith(queriesCatalog))
                .ToDictionary(
                    x => GetEmbeddedSqlResourceName(queriesCatalog, x),
                    x => GetEmbeddedSqlResourcebody(assembly, x));
            return resources;
        }

        private string GetEmbeddedSqlResourcebody(Assembly assembly, string resource)
        {
            using Stream stream = assembly.GetManifestResourceStream(resource);
            using StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private string GetQuery(string query)
        {
            if (queries.ContainsKey(query))
            {
                return queries[query];
            }
            throw new Exception($"Unknown sql query '{query}'");
        }

        private string GetEmbeddedSqlResourceName(string queriesCatalog, string resource)
        {
            return Regex.Split(resource, $"{queriesCatalog}.(.*).sql")[1];
        }

        public async Task<T> QuerySingleAsync<T>(
            object parameters,
            [CallerMemberName] string callerMemberName = null)
        {
            return await connection.QuerySingleAsync<T>(GetQuery(callerMemberName), parameters);
        }

        public async Task<T> QuerySingleOrDefaultAsync<T>(
            object parameters,
            [CallerMemberName] string callerMemberName = null)
        {
            return await connection.QuerySingleOrDefaultAsync<T>(GetQuery(callerMemberName), parameters);
        }

        public async Task<T> QueryFirstOrDefaultAsync<T>(
            object parameters,
            [CallerMemberName] string callerMemberName = null)
        {
            return await connection.QueryFirstOrDefaultAsync<T>(GetQuery(callerMemberName), parameters);
        }

        public async Task<List<T>> QueryListAsync<T>(
            object parameters,
            [CallerMemberName] string callerMemberName = null)
        {
            IEnumerable<T> result = await connection.QueryAsync<T>(GetQuery(callerMemberName), parameters);
            return result.ToList();
        }

        public async Task QueryAsync(
           object parameters,
           [CallerMemberName] string callerMemberName = null)
        {
            await connection.QueryAsync(GetQuery(callerMemberName), parameters);
        }
    }
}
