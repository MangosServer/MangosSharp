﻿//
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

using Mangos.Common.Enums.Global;
using Mangos.Loggers;
using Mangos.Realm.Models;
using Mangos.Realm.Network.Readers;
using Mangos.Realm.Network.Responses;
using Mangos.Realm.Network.Writers;
using Mangos.Storage.Account;
using Microsoft.VisualBasic;
using System;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Realm.Network.Handlers
{
    public class RS_LOGON_PROOF_Handler : IPacketHandler
    {
        private readonly ILogger logger;
        private readonly IRealmStorage realmStorage;

        private readonly AUTH_LOGON_PROOF_Writer AUTH_LOGON_PROOF_Writer;
        private readonly RS_LOGON_PROOF_Reader RS_LOGON_PROOF_Reader;

        public RS_LOGON_PROOF_Handler(
            ILogger logger,
            IRealmStorage realmStorage,
            AUTH_LOGON_PROOF_Writer AUTH_LOGON_PROOF_Writer, 
            RS_LOGON_PROOF_Reader RS_LOGON_PROOF_Reader)
        {
            this.logger = logger;
            this.realmStorage = realmStorage;
            this.AUTH_LOGON_PROOF_Writer = AUTH_LOGON_PROOF_Writer;
            this.RS_LOGON_PROOF_Reader = RS_LOGON_PROOF_Reader;
        }

        public async Task HandleAsync(ChannelReader<byte> reader, ChannelWriter<byte> writer, ClientModel clientModel)
        {
            var request = await RS_LOGON_PROOF_Reader.ReadAsync(reader);

            // Calculate U and M1
            clientModel.ClientAuthEngine.CalculateU(request.A);
            clientModel.ClientAuthEngine.CalculateM1();
            // AuthEngine.CalculateCRCHash()

            if (!clientModel.ClientAuthEngine.M1.SequenceEqual(request.M1))
            {
                // Wrong pass
                logger.Debug("Wrong password for user {0}.", clientModel.AccountName);

                await AUTH_LOGON_PROOF_Writer.WriteAsync(writer, new AUTH_LOGON_PROOF(AccountState.LOGIN_BAD_PASS));
            }
            else
            {
                clientModel.ClientAuthEngine.CalculateM2(request.M1);

                await AUTH_LOGON_PROOF_Writer.WriteAsync(writer, new AUTH_LOGON_PROOF(
                    AccountState.LOGIN_OK, 
                    clientModel.ClientAuthEngine.M2));

                // Set SSHash in DB
                var sshash = string.Concat(clientModel.ClientAuthEngine.SsHash.Select(x => x.ToString("X2")));

                await realmStorage.UpdateAccountAsync(sshash,
                    clientModel.RemoteEnpoint.Address.ToString(), 
                    Strings.Format(DateAndTime.Now, "yyyy-MM-dd"), 
                    clientModel.AccountName);

                logger.Debug("Auth success for user {0} [{1}]", clientModel.AccountName, sshash);
            }
        }
    }
}
