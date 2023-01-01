//
// Copyright (C) 2013-2023 getMaNGOS <https://getmangos.eu>
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

using Mangos.Logging;
using Mangos.MySql.UpdateAccount;
using Microsoft.VisualBasic;
using RealmServer.Domain;
using RealmServer.Network;
using RealmServer.Requests;
using RealmServer.Responses;

namespace RealmServer.Handlers;

internal sealed class RsLogonProofHandler : IHandler<RsLogonProofRequest>
{
    private readonly IMangosLogger logger;
    private readonly IUpdateAccountCommand updateAccountCommand;
    private readonly ClientState clientState;

    public RsLogonProofHandler(IMangosLogger logger, IUpdateAccountCommand updateAccountCommand, ClientState clientState)
    {
        this.logger = logger;
        this.updateAccountCommand = updateAccountCommand;
        this.clientState = clientState;
    }

    public MessageOpcode MessageOpcode => MessageOpcode.CMD_AUTH_LOGON_PROOF;

    public async Task<IResponseMessage> ExectueAsync(RsLogonProofRequest request)
    {
        clientState.AuthEngine.CalculateU(request.A);
        clientState.AuthEngine.CalculateM1();

        if (!clientState.AuthEngine.M1.SequenceEqual(request.M1))
        {
            logger.Information($"Wrong password for user {clientState.AccountName}");
            return new AuthLogonProofResponse { AccountState = AccountStates.LOGIN_BAD_PASS };
        }

        clientState.AuthEngine.CalculateM2(request.M1);

        var sshash = string.Concat(clientState.AuthEngine.SsHash.Select(x => x.ToString("X2")));

        await updateAccountCommand.ExecuteAsync(
            sshash,
            clientState.IPAddress?.ToString() ?? throw new Exception($"Unable to get ip address"),
            DateAndTime.Now.ToString("yyyy-MM-dd"),
            clientState.AccountName ?? throw new Exception($"Unable to get account name"));

        return new AuthLogonProofResponse
        {
            AccountState = AccountStates.LOGIN_OK,
            M2 = clientState.AuthEngine.M2
        };
    }
}
