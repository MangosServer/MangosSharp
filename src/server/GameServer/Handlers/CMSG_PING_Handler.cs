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

using GameServer.Network;
using GameServer.Requests;
using GameServer.Responses;
using Mangos.Logging;

namespace GameServer.Handlers;

internal sealed class CMSG_PING_Handler : IHandler<CMSG_PING>
{
    private readonly IMangosLogger logger;

    public CMSG_PING_Handler(IMangosLogger logger)
    {
        this.logger = logger;
        logger.Trace("[CMSG_PING_Handler] Handler instance created");
    }

    public Task<HandlerResult> ExectueAsync(CMSG_PING request)
    {
        logger.Trace($"[CMSG_PING_Handler] Received PING with payload: {request.Payload}");
        var response = new SMSG_PONG
        {
            Payload = request.Payload
        };
        logger.Trace($"[CMSG_PING_Handler] Sending PONG response with payload: {response.Payload}");

        return HandlerResult.FromTask(response);
    }
}
