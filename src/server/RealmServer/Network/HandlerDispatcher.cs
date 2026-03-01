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
using RealmServer.Handlers;
using RealmServer.Requests;

namespace RealmServer.Network;

internal sealed class HandlerDispatcher<THandler, TRequest> : IHandlerDispatcher
    where THandler : IHandler<TRequest>
    where TRequest : IRequestMessage<TRequest>
{
    private readonly THandler handler;
    private readonly IMangosLogger logger;

    public HandlerDispatcher(THandler handler, IMangosLogger logger)
    {
        this.handler = handler;
        this.logger = logger;
        logger.Trace($"[RealmDispatcher] Registered handler {typeof(THandler).Name} for opcode {handler.MessageOpcode}");
    }

    public MessageOpcode Opcode => handler.MessageOpcode;

    public async Task ExectueAsync(SocketReader reader, SocketWriter writer)
    {
        logger.Trace($"[RealmDispatcher] Reading {typeof(TRequest).Name} from socket stream");
        var request = await TRequest.ReadAsync(reader);
        logger.Trace($"[RealmDispatcher] Executing {typeof(THandler).Name} for opcode {Opcode}");
        var response = await handler.ExectueAsync(request);
        logger.Trace($"[RealmDispatcher] Writing response {response.GetType().Name} to socket stream");
        await response.WriteAsync(writer);
        logger.Trace($"[RealmDispatcher] Response sent for opcode {Opcode}");
    }
}
