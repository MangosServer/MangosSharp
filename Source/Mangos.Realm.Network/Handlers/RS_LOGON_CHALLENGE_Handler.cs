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

using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using Mangos.Loggers;
using Mangos.Realm.Network.Readers;
using Mangos.Realm.Network.Requests;
using Mangos.Realm.Network.Responses;
using Mangos.Realm.Network.Writers;
using Mangos.Realm.Storage.Entities;
using Mangos.Storage.Account;
using System.Globalization;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Realm.Network.Handlers;

public class RS_LOGON_CHALLENGE_Handler : IPacketHandler
{
    private readonly ILogger logger;
    private readonly MangosGlobalConstants mangosGlobalConstants;
    private readonly IAccountStorage accountStorage;

    private readonly AUTH_LOGON_PROOF_Writer AUTH_LOGON_PROOF_Writer;
    private readonly RS_LOGON_CHALLENGE_Reader RS_LOGON_CHALLENGE_Reader;
    private readonly AUTH_LOGON_CHALLENGE_Writer AUTH_LOGON_CHALLENGE_Writer;

    public RS_LOGON_CHALLENGE_Handler(
        ILogger logger,
        MangosGlobalConstants mangosGlobalConstants,
        IAccountStorage accountStorage,
        AUTH_LOGON_PROOF_Writer AUTH_LOGON_PROOF_Writer,
        RS_LOGON_CHALLENGE_Reader RS_LOGON_CHALLENGE_Reader,
        AUTH_LOGON_CHALLENGE_Writer AUTH_LOGON_CHALLENGE_Writer)
    {
        this.mangosGlobalConstants = mangosGlobalConstants;
        this.accountStorage = accountStorage;
        this.logger = logger;
        this.AUTH_LOGON_PROOF_Writer = AUTH_LOGON_PROOF_Writer;
        this.RS_LOGON_CHALLENGE_Reader = RS_LOGON_CHALLENGE_Reader;
        this.AUTH_LOGON_CHALLENGE_Writer = AUTH_LOGON_CHALLENGE_Writer;
    }

    public async Task HandleAsync(ChannelReader<byte> reader, ChannelWriter<byte> writer, Client clientModel)
    {
        var request = await RS_LOGON_CHALLENGE_Reader.ReadAsync(reader);
        clientModel.AccountName = request.AccountName;

        // DONE: Check if our build can join the server
        if (request.Build == mangosGlobalConstants.Required_Build_1_12_1
            || request.Build == mangosGlobalConstants.Required_Build_1_12_2
            || request.Build == mangosGlobalConstants.Required_Build_1_12_3)
        {
            // TODO: in the far future should check if the account is expired too
            var accountInfo = await accountStorage.GetAccountInfoAsync(clientModel.AccountName);
            var accountState = await GetAccountStateAsync(accountInfo).ConfigureAwait(false);

            // DONE: Send results to client
            switch (accountState)
            {
                case AccountState.LOGIN_OK:
                    await HandleLoginOkStateAsync(request, writer, clientModel, accountInfo).ConfigureAwait(false);
                    return;

                case AccountState.LOGIN_UNKNOWN_ACCOUNT:
                    await AUTH_LOGON_PROOF_Writer.WriteAsync(writer, new AUTH_LOGON_PROOF(AccountState.LOGIN_UNKNOWN_ACCOUNT));
                    return;

                case AccountState.LOGIN_BANNED:
                    await AUTH_LOGON_PROOF_Writer.WriteAsync(writer, new AUTH_LOGON_PROOF(AccountState.LOGIN_BANNED));
                    return;

                case AccountState.LOGIN_NOTIME:
                    await AUTH_LOGON_PROOF_Writer.WriteAsync(writer, new AUTH_LOGON_PROOF(AccountState.LOGIN_NOTIME));
                    return;

                case AccountState.LOGIN_ALREADYONLINE:
                    await AUTH_LOGON_PROOF_Writer.WriteAsync(writer, new AUTH_LOGON_PROOF(AccountState.LOGIN_ALREADYONLINE));
                    return;

                case AccountState.LOGIN_FAILED:
                case AccountState.LOGIN_BAD_PASS:
                case AccountState.LOGIN_DBBUSY:
                case AccountState.LOGIN_BADVERSION:
                case AccountState.LOGIN_DOWNLOADFILE:
                case AccountState.LOGIN_SUSPENDED:
                case AccountState.LOGIN_PARENTALCONTROL:
                    break;

                default:
                    await AUTH_LOGON_PROOF_Writer.WriteAsync(writer, new AUTH_LOGON_PROOF(AccountState.LOGIN_FAILED));
                    return;
            }
        }
        else
        {
            // Send BAD_VERSION
            logger.Warning($"WRONG_VERSION {request.Build}");
            await AUTH_LOGON_PROOF_Writer.WriteAsync(writer, new AUTH_LOGON_PROOF(AccountState.LOGIN_BADVERSION));
        }
    }

    private async Task HandleLoginOkStateAsync(
        RS_LOGON_CHALLENGE request,
        ChannelWriter<byte> writer,
        Client clientModel,
        AccountInfoEntity accountInfo)
    {
        if (accountInfo.sha_pass_hash.Length != 40) // Invalid password type, should always be 40 characters
        {
            await AUTH_LOGON_PROOF_Writer.WriteAsync(writer, new AUTH_LOGON_PROOF(AccountState.LOGIN_BAD_PASS));
            return;
        }

        var hash = GetPasswordHashFromString(accountInfo.sha_pass_hash);

        clientModel.AuthEngine.CalculateX(request.Account, hash);

        AUTH_LOGON_CHALLENGE resposne = new(
            clientModel.AuthEngine.PublicB,
            clientModel.AuthEngine.g,
            clientModel.AuthEngine.N,
            clientModel.AuthEngine.Salt,
            AuthEngine.CrcSalt);

        await AUTH_LOGON_CHALLENGE_Writer.WriteAsync(writer, resposne);
    }

    private async Task<AccountState> GetAccountStateAsync(AccountInfoEntity accountInfo)
    {
        return accountInfo != null
            ? await accountStorage.IsBannedAccountAsync(accountInfo.id)
                ? AccountState.LOGIN_BANNED
                : AccountState.LOGIN_OK
            : AccountState.LOGIN_UNKNOWN_ACCOUNT;
    }

    private byte[] GetPasswordHashFromString(string sha_pass_hash)
    {
        var hash = new byte[20];
        for (var i = 0; i < 40; i += 2)
        {
            hash[i / 2] = byte.Parse(sha_pass_hash.Substring(i, 2), NumberStyles.HexNumber);
        }
        return hash;
    }
}
