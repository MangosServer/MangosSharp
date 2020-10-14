using global;
using Mangos.Common.Globals;
using Mangos.Loggers;
using Mangos.Network.Tcp;
using Mangos.Storage;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Mangos.Realm.Async.Factories
{
    public class RealmServerClientFactory : ITcpClientFactory
    {
        private readonly ILogger logger;
        private readonly IAccountStorage accountStorage;

        private readonly Converter _Converter;
        private readonly Global_Constants _Global_Constants;

        public RealmServerClientFactory(ILogger logger, 
            IAccountStorage accountStorage, 
            Converter converter, 
            Global_Constants global_Constants)
        {
            this.logger = logger;
            this.accountStorage = accountStorage;
            _Converter = converter;
            _Global_Constants = global_Constants;
        }

        public async Task<ITcpClient> CreateTcpClientAsync(Socket clientSocket)
        {
            return new RealmServerClient(
                logger, 
                accountStorage, 
                (IPEndPoint)clientSocket.RemoteEndPoint,
                _Converter,
                _Global_Constants);
        }
    }
}
