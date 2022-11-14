//
// Copyright (C) 2013-2022 getMaNGOS <https://getmangos.eu>
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

    public TcpPacketOpCodes TcpPacketOpCode => TcpPacketOpCodes.CMD_AUTH_LOGON_CHALLENGE;

    public async Task<IResponseMessage> ExectueAsync(RsLogonChallengeRequest request)
    {
        if (request.ClientBuild != WowClientBuildVersions.WOW_1_12_1)
        {
            logger.Warning($"Someone try to login with unsupported client version {request.ClientBuild}");
            return new AuthLogonProofResponse { AccountState = AccountStates.LOGIN_BADVERSION };
        }

        var accountInfo = await getAccountInfoQuery.ExectueAsync(request.AccountName);
        if (accountInfo == null)
        {
            return new AuthLogonProofResponse { AccountState = AccountStates.LOGIN_UNKNOWN_ACCOUNT };
        }
        if (await isBannedAccountQuery.ExecuteAsync(accountInfo.id))
        {
            return new AuthLogonProofResponse { AccountState = AccountStates.LOGIN_BANNED };
        }
        if (accountInfo.sha_pass_hash.Length != 40)
        {
            return new AuthLogonProofResponse { AccountState = AccountStates.LOGIN_BAD_PASS };
        }

        var hash = GetPasswordHashFromString(accountInfo.sha_pass_hash);
        clientState.AccountName = request.AccountName;
        clientState.AuthEngine.CalculateX(Encoding.UTF8.GetBytes(request.AccountName), hash);

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
