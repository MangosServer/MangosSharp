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
        logger.Information($"[Auth] Logon proof received for account '{clientState.AccountName}' from {clientState.IPAddress}");
        logger.Trace($"[Auth] Calculating SRP6 U value from client's public key A (length={request.A.Length})");
        clientState.AuthEngine.CalculateU(request.A);
        logger.Trace($"[Auth] Calculating expected M1 proof value");
        clientState.AuthEngine.CalculateM1();

        if (!clientState.AuthEngine.M1.SequenceEqual(request.M1))
        {
            logger.Warning($"[Auth] Wrong password for user '{clientState.AccountName}' from {clientState.IPAddress} - M1 mismatch");
            logger.Trace($"[Auth] Expected M1 length={clientState.AuthEngine.M1.Length}, received M1 length={request.M1.Length}");
            return new AuthLogonProofResponse { AccountState = AccountStates.LOGIN_BAD_PASS };
        }
        logger.Debug($"[Auth] Password proof verified successfully for account '{clientState.AccountName}'");

        logger.Trace($"[Auth] Calculating M2 server proof for account '{clientState.AccountName}'");
        clientState.AuthEngine.CalculateM2(request.M1);

        var sshash = string.Concat(clientState.AuthEngine.SsHash.Select(x => x.ToString("X2")));
        logger.Trace($"[Auth] Session key hash computed for account '{clientState.AccountName}' (length={sshash.Length})");

        var ipAddress = clientState.IPAddress?.ToString() ?? throw new Exception($"Unable to get ip address");
        var loginDate = DateAndTime.Now.ToString("yyyy-MM-dd");
        var accountName = clientState.AccountName ?? throw new Exception($"Unable to get account name");

        logger.Debug($"[Auth] Updating account record: account='{accountName}', ip={ipAddress}, login_date={loginDate}");
        await updateAccountCommand.ExecuteAsync(sshash, ipAddress, loginDate, accountName);
        logger.Debug($"[Auth] Account record updated successfully for '{accountName}'");

        logger.Information($"[Auth] Login successful for account '{accountName}' from {ipAddress}");
        return new AuthLogonProofResponse
        {
            AccountState = AccountStates.LOGIN_OK,
            M2 = clientState.AuthEngine.M2
        };
    }
}
