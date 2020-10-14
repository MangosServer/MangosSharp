using Mangos.Configuration;
using Mangos.Network.Tcp;
using Mangos.Realm.Async.Factories;
using Mangos.Storage.MySql;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Mangos.Realm.Async
{
    public class RealmServer
    {
        private readonly MySqlAccountStorage accountStorage;
        private readonly IConfigurationProvider<RealmServerConfiguration> configurationProvider;

        private readonly TcpServer tcpServer;

        public RealmServer(MySqlAccountStorage accountStorage,
            IConfigurationProvider<RealmServerConfiguration> configurationProvider, 
            RealmServerClientFactory realmServerClientFactory)
        {
            this.accountStorage = accountStorage;
            this.configurationProvider = configurationProvider;

            tcpServer = new TcpServer(realmServerClientFactory);
        }

        public async Task StartAsync()
        {
            var configuration = await configurationProvider.GetConfigurationAsync();
            await accountStorage.ConnectAsync(configuration.AccountConnectionString);

            var endpoint = IPEndPoint.Parse(configuration.RealmServerEndpoint);
            tcpServer.Start(endpoint, 10);

            Console.WriteLine("Started");
            Console.ReadLine();
        }
    }
}
