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

using Mangos.Tcp;
using RealmServer.Domain;
using System.Net;
using System.Net.Sockets;

namespace RealmServer.Network;

internal sealed class RealmRouterTcpClientHandler : ITcpClientHandler
{
    private readonly IHandlerDispatcher[] dispatchers;
    private readonly ClientState clientState;

    public RealmRouterTcpClientHandler(IEnumerable<IHandlerDispatcher> dispatchers, ClientState clientState)
    {
        this.dispatchers = dispatchers.ToArray();
        this.clientState = clientState;
    }

    public async Task ExectueAsync(
        ITcpReader reader,
        ITcpWriter writer,
        IPAddress remoteAddress,
        CancellationToken cancellationToken,
        Socket socket)
    {
        clientState.IPAddress = remoteAddress;
        while (!cancellationToken.IsCancellationRequested)
        {
            await ExecuteMessageAsync(reader, writer);
        }
    }

    private async Task ExecuteMessageAsync(ITcpReader reader, ITcpWriter writer)
    {
        var opcode = (TcpPacketOpCodes)await reader.ReadByteAsync();

        var dispatcher = dispatchers.FirstOrDefault(x => x.Opcode == opcode);
        if (dispatcher == null)
        {
            throw new NotImplementedException($"Unsupported opcode {opcode}");
        }
        else
        {
            await dispatcher.ExectueAsync(reader, writer);
        }
    }
}