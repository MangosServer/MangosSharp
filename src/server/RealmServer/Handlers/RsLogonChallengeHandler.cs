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
using Mangos.MySql.GetAccountInfo;
using Mangos.MySql.IsBannedAccount;
using RealmServer.Domain;
using RealmServer.Handlers;
using RealmServer.Network;
using RealmServer.Requests;
using RealmServer.Responses;
using System.Globalization;
using System.Text;

namespace Mangos.Realm.Network.Handlers;

internal sealed class RsLogonChallengeHandler : IHandler<RsLogonChallengeRequest>
{
    private readonly IMangosLogger logger;
    private readonly ClientState clientState;
    private readonly IGetAccountInfoQuery getAccountInfoQuery;
    private readonly IIsBannedAccountQuery isBannedAccountQuery;

    public RsLogonChallengeHandler(
        IMangosLogger logger,
        IGetAccountInfoQuery getAccountInfoQuery,
        IIsBannedAccountQuery isBannedAccountQuery,
        ClientState clientState)
    {
        this.logger = logger;
        this.getAccountInfoQuery = getAccountInfoQuery;
        this.isBannedAccountQuery = isBannedAccountQuery;
        this.clientState = clientState;
    }

    public MessageOpcode MessageOpcode => MessageOpcode.CMD_AUTH_LOGON_CHALLENGE;

    public async Task<IResponseMessage> ExectueAsync(RsLogonChallengeRequest request)
    {
        logger.Information($"[Auth] Logon challenge received for account '{request.AccountName}', client build: {request.ClientBuild}");

        if (request.ClientBuild != WowClientBuildVersions.WOW_1_12_1)
        {
            logger.Warning($"[Auth] Login rejected - unsupported client version {request.ClientBuild} for account '{request.AccountName}' (expected {WowClientBuildVersions.WOW_1_12_1})");
            return new AuthLogonProofResponse { AccountState = AccountStates.LOGIN_BADVERSION };
        }
        logger.Debug($"[Auth] Client build version {request.ClientBuild} validated for account '{request.AccountName}'");

        logger.Trace($"[Auth] Querying database for account info: '{request.AccountName}'");
        var accountInfo = await getAccountInfoQuery.ExectueAsync(request.AccountName);
        if (accountInfo == null)
        {
            logger.Warning($"[Auth] Login rejected - unknown account '{request.AccountName}'");
            return new AuthLogonProofResponse { AccountState = AccountStates.LOGIN_UNKNOWN_ACCOUNT };
        }
        logger.Debug($"[Auth] Account found: id={accountInfo.id}, account='{request.AccountName}'");

        logger.Trace($"[Auth] Checking ban status for account id={accountInfo.id}");
        if (await isBannedAccountQuery.ExecuteAsync(accountInfo.id))
        {
            logger.Warning($"[Auth] Login rejected - account '{request.AccountName}' (id={accountInfo.id}) is banned");
            return new AuthLogonProofResponse { AccountState = AccountStates.LOGIN_BANNED };
        }
        logger.Trace($"[Auth] Account '{request.AccountName}' is not banned");

        if (accountInfo.sha_pass_hash.Length != 40)
        {
            logger.Error($"[Auth] Login rejected - invalid password hash length ({accountInfo.sha_pass_hash.Length}) for account '{request.AccountName}' (expected 40)");
            return new AuthLogonProofResponse { AccountState = AccountStates.LOGIN_BAD_PASS };
        }

        logger.Trace($"[Auth] Parsing password hash for account '{request.AccountName}'");
        var hash = GetPasswordHashFromString(accountInfo.sha_pass_hash);
        clientState.AccountName = request.AccountName;
        logger.Trace($"[Auth] Calculating SRP6 challenge (CalculateX) for account '{request.AccountName}'");
        clientState.AuthEngine.CalculateX(Encoding.UTF8.GetBytes(request.AccountName), hash);

        logger.Information($"[Auth] Logon challenge successful for account '{request.AccountName}', sending challenge response");
        logger.Trace($"[Auth] SRP6 parameters: PublicB length={clientState.AuthEngine.PublicB.Length}, Salt length={clientState.AuthEngine.Salt.Length}");
        return new AuthLogonChallengeResponse
        {
            PublicB = clientState.AuthEngine.PublicB,
            G = clientState.AuthEngine.g,
            N = clientState.AuthEngine.N,
            Salt = clientState.AuthEngine.Salt,
            CrcSalt = AuthEngine.CrcSalt
        };
    }

    private byte[] GetPasswordHashFromString(string sha_pass_hash)
    {
        var hash = new byte[20];
        for (var i = 0; i < 40; i += 2)
        {
            hash[i / 2] = byte.Parse(sha_pass_hash.AsSpan(i, 2), NumberStyles.HexNumber);
        }
        return hash;
    }
}
