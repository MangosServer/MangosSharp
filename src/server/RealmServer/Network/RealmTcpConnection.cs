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
using Mangos.Tcp;
using RealmServer.Domain;
using System.Net;
using System.Net.Sockets;

namespace RealmServer.Network;

internal sealed class RealmTcpConnection : ITcpConnection
{
    private readonly IHandlerDispatcher[] dispatchers;
    private readonly ClientState clientState;
    private readonly IMangosLogger logger;
    private long _messagesProcessed;

    public RealmTcpConnection(IEnumerable<IHandlerDispatcher> dispatchers, ClientState clientState, IMangosLogger logger)
    {
        this.dispatchers = dispatchers.ToArray();
        this.clientState = clientState;
        this.logger = logger;
        logger.Debug($"[RealmTcp] Connection instance created with {this.dispatchers.Length} dispatchers");
    }

    public async Task ExecuteAsync(Socket socket, CancellationToken cancellationToken)
    {
        if (socket.RemoteEndPoint is not IPEndPoint endpoint)
        {
            logger.Error("[RealmTcp] Unable to get remote endpoint from socket");
            throw new Exception("Unable to get remote endponit");
        }
        clientState.IPAddress = endpoint.Address;
        logger.Information($"[RealmTcp] Auth connection established from {endpoint}");
        logger.Debug($"[RealmTcp] Client IP set to {endpoint.Address} for session tracking");

        var socketReader = new SocketReader(socket, cancellationToken);
        var socketWriter = new SocketWriter(socket, cancellationToken);
        logger.Trace($"[RealmTcp] Socket reader/writer initialized for {endpoint}");

        while (!cancellationToken.IsCancellationRequested)
        {
            await ExecuteMessageAsync(socketReader, socketWriter);
        }
        logger.Debug($"[RealmTcp] Connection loop ended for {endpoint}, processed {_messagesProcessed} messages");
    }

    private async Task ExecuteMessageAsync(SocketReader reader, SocketWriter writer)
    {
        var opcode = (MessageOpcode)await reader.ReadByteAsync();
        Interlocked.Increment(ref _messagesProcessed);
        logger.Trace($"[RealmTcp] Received auth opcode: {opcode} (0x{(byte)opcode:X2}), message #{_messagesProcessed}");

        var dispatcher = dispatchers.FirstOrDefault(x => x.Opcode == opcode);
        if (dispatcher == null)
        {
            logger.Error($"[RealmTcp] No handler registered for opcode {opcode} (0x{(byte)opcode:X2})");
            throw new NotImplementedException($"Unsupported opcode {opcode}");
        }
        else
        {
            logger.Trace($"[RealmTcp] Dispatching opcode {opcode} to handler");
            await dispatcher.ExectueAsync(reader, writer);
            logger.Trace($"[RealmTcp] Handler completed for opcode {opcode}");
        }
    }
}
