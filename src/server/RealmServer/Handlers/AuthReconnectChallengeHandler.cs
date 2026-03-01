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
using Mangos.Realm.Network.Handlers;
using RealmServer.Network;
using RealmServer.Requests;
using RealmServer.Responses;

namespace RealmServer.Handlers;

internal sealed class AuthReconnectChallengeHandler : IHandler<RsLogonChallengeRequest>
{
    private readonly RsLogonChallengeHandler rsLogonChallengeHandler;
    private readonly IMangosLogger logger;

    public AuthReconnectChallengeHandler(RsLogonChallengeHandler rsLogonChallengeHandler, IMangosLogger logger)
    {
        this.rsLogonChallengeHandler = rsLogonChallengeHandler;
        this.logger = logger;
        logger.Trace("[AuthReconnectChallengeHandler] Handler instance created");
    }

    public MessageOpcode MessageOpcode => MessageOpcode.CMD_AUTH_RECONNECT_CHALLENGE;

    public async Task<IResponseMessage> ExectueAsync(RsLogonChallengeRequest request)
    {
        logger.Information($"[Auth] Reconnect challenge received for account '{request.AccountName}', delegating to logon challenge handler");
        logger.Trace($"[Auth] Reconnect request - client build: {request.ClientBuild}");
        var response = await rsLogonChallengeHandler.ExectueAsync(request);
        logger.Debug($"[Auth] Reconnect challenge completed for account '{request.AccountName}', response type: {response.GetType().Name}");
        return response;
    }
}
