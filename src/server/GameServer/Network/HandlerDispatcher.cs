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

using GameServer.Handlers;
using GameServer.Requests;
using Mangos.Logging;

namespace GameServer.Network;

internal sealed class HandlerDispatcher<TRequest, THandler> : IHandlerDispatcher
    where TRequest : IRequestMessage<TRequest>
    where THandler : IHandler<TRequest>
{
    private readonly THandler handler;
    private readonly IMangosLogger logger;

    public HandlerDispatcher(THandler handler, IMangosLogger logger)
    {
        this.handler = handler;
        this.logger = logger;
        logger.Trace($"[HandlerDispatcher] Registered dispatcher for opcode {TRequest.Opcode} -> {typeof(THandler).Name}");
    }

    public Opcodes Opcode => TRequest.Opcode;

    public Task<HandlerResult> ExectueAsync(PacketReader reader)
    {
        logger.Trace($"[HandlerDispatcher] Deserializing {typeof(TRequest).Name} from packet (remaining bytes: {reader.Remaining})");
        var request = TRequest.Read(reader);
        logger.Trace($"[HandlerDispatcher] Executing {typeof(THandler).Name} for opcode {TRequest.Opcode}");
        return handler.ExectueAsync(request);
    }
}
