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

using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Mangos.Common.Globals;
using Mangos.Loggers;
using Mangos.Network.Tcp;
using Mangos.Realm.Models;
using Mangos.Storage.Account;

namespace Mangos.Realm.Network
{
    public class RealmTcpClientFactory : ITcpClientFactory
    {
        private readonly ILogger logger;
        private readonly IRealmStorage realmStorage;
        private readonly Converter converter;
        private readonly MangosGlobalConstants mangosGlobalConstants;

        public RealmTcpClientFactory(ILogger logger,
            IRealmStorage realmStorage,
            Converter converter,
            MangosGlobalConstants mangosGlobalConstants)
        {
            this.logger = logger;
            this.realmStorage = realmStorage;
            this.converter = converter;
            this.mangosGlobalConstants = mangosGlobalConstants;
        }

        public async Task<ITcpClient> CreateTcpClientAsync(Socket clientSocket)
        {
            var clientModel = new ClientModel();

            var legacyServerClient =  new LegacyServerClient(
                logger,
                realmStorage,
                converter,
                mangosGlobalConstants,
                (IPEndPoint)clientSocket.RemoteEndPoint,
                clientModel);

            return new RealmTcpClient(clientModel, legacyServerClient);
        }
    }
}
