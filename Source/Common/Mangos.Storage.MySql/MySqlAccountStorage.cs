using Dapper;
using Mangos.Storage.Account.Responses;
using Mangos.Storage.Account.Results;
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
    public class MySqlAccountStorage : IAccountStorage, IAsyncDisposable
    {
        private MySqlConnection connection;
        private readonly Dictionary<string, string> sql;

        public MySqlAccountStorage()
        {
            sql = GetEmbeddedSqlResources();
        }

        public async Task ConnectAsync(string conenctionString)
        {
            connection = new MySqlConnection(conenctionString);
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

        private async Task<T> QuerySingleOrDefaultAsync<T>(
            object parameters,
            [CallerMemberName] string callerMemberName = null)
        {
            return await connection.QuerySingleOrDefaultAsync<T>(sql[callerMemberName], parameters);
        }

        private async Task<T> QueryFirstOrDefault<T>(
            object parameters,
            [CallerMemberName] string callerMemberName = null)
        {
            return await connection.QueryFirstOrDefaultAsync<T>(sql[callerMemberName], parameters);
        }

        private async Task<List<T>> QueryAsync<T>(
            object parameters,
            [CallerMemberName] string callerMemberName = null)
        {
            var data = await connection.QueryAsync<T>(sql[callerMemberName], parameters);
            return data.ToList();
        }

        private async Task UpdateAsync(
           object parameters,
           [CallerMemberName] string callerMemberName = null)
        {
            await connection.QueryAsync(sql[callerMemberName], parameters);
        }

        public async Task<bool> IsBannedAccountAsync(string id)
        {
            var count = await QuerySingleAsync<int>(new
            {
                Id = id
            });
            return count > 0;
        }

        public async Task<AccountInfo> GetAccountInfoAsync(string username)
        {
            return await QueryFirstOrDefault<AccountInfo>(new
            {
                Username = username
            });
        }

        public async Task<List<RealmListItem>> GetRealmListAsync()
        {
            return await QueryAsync<RealmListItem>(null);
        }

        public async Task UpdateAccountAsync(string sessionkey, string last_ip, string last_login, string username)
        {
            await UpdateAsync(new
            {
                Sessionkey = sessionkey,
                Last_ip = last_ip,
                Last_login = last_login,
                Username = username
            });
        }

        public async Task<int> GetNumcharsAsync(string realmId)
        {
            return await QuerySingleOrDefaultAsync<int>(new
            {
                Realmid = realmId
            });
        }
    }
}
