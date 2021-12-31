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

using Mangos.Loggers;
using Mangos.Network.Tcp;
using System;
using System.Threading;
using System.Threading.Channels;

namespace Mangos.Realm.Network;

public class RealmTcpClient : ITcpClient
{
    private readonly ILogger logger;
    private readonly Client clientModel;
    private readonly Router router;

    public RealmTcpClient(ILogger logger, Router router, Client clientModel)
    {
        this.logger = logger;
        this.router = router;
        this.clientModel = clientModel;
    }

    public async void HandleAsync(
        ChannelReader<byte> reader,
        ChannelWriter<byte> writer,
        CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var opcode = await reader.ReadAsync(cancellationToken);
                var packetHandler = router.GetPacketHandler(opcode);
                await packetHandler.HandleAsync(reader, writer, clientModel);
            }
        }
        catch (Exception ex)
        {
            logger.Error("Packet handler error", ex);
        }
    }
}
