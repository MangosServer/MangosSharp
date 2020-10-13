using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mangos.Storage.MySql
{
    public class MySqlAccountStorage : IAccountStorage, IAsyncDisposable
    {
        private readonly MySqlConnection connection;
        private readonly Dictionary<string, string> sql;

        public MySqlAccountStorage(string conenctionString)
        {
            connection = new MySqlConnection(conenctionString);
            sql = GetEmbeddedSqlResources();
        }

        public async Task StartAsync()
        {
            await connection.OpenAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await connection.DisposeAsync();
        }

        private Dictionary<string, string> GetEmbeddedSqlResources()
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceNames()
                .Where(x => x.StartsWith("Mangos.Storage.MySql.Account"))
                .ToDictionary(GetEmbeddedSqlResourceName, GetEmbeddedSqlResourcebody);
        }

        private string GetEmbeddedSqlResourcebody(string resource)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(resource);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private string GetEmbeddedSqlResourceName(string resource)
        {
            return Regex.Split(resource, "Mangos.Storage.MySql.Account.(.*).sql")[1];
        }

        private async Task<T> QuerySingleAsync<T>(
            object parameters, 
            [CallerMemberName] string callerMemberName = null)
        {
            return await connection.QuerySingleAsync<T>(sql[callerMemberName], parameters);
        }

        public async Task<bool> IsBannedAsync(IPAddress address)
        {
            var count = await QuerySingleAsync<int>(new
            {
                Address = address
            });
            return count > 0;
        }
    }
}
