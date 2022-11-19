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

using Mangos.Cluster.Globals;
using Mangos.Cluster.Network;
using Mangos.Tcp;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Net.Sockets;

namespace GameServer.Network;

internal sealed class GameTcpConnection : ITcpConnection
{
    private readonly ClientClass legacyClientClass;

    private readonly MemoryPool<byte> memoryPool = MemoryPool<byte>.Shared;

    public GameTcpConnection(ClientClass legacyClientClass)
    {
        this.legacyClientClass = legacyClientClass;
    }

    public async Task ExecuteAsync(Socket socket, CancellationToken cancellationToken)
    {
        legacyClientClass.Socket = socket;
        await legacyClientClass.OnConnectAsync();

        while (!cancellationToken.IsCancellationRequested)
        {
            await WaitForNextPacket(socket, cancellationToken);
            await HandlePacketAsync(socket, cancellationToken);
        }
    }

    private async Task HandlePacketAsync(Socket socket, CancellationToken cancellationToken)
    {
        const int MAX_PACKET_LENGTH = 10000;
        using var memoryOwner = memoryPool.Rent(MAX_PACKET_LENGTH);
        var header = memoryOwner.Memory.Slice(0, 6);

        await ReadAsync(socket, header, cancellationToken);
        DecodePacketHeader(header.Span);
        var length = BinaryPrimitives.ReadInt16BigEndian(header.Span) - 4;
        var opcode = (MessageOpcode)BinaryPrimitives.ReadUInt16LittleEndian(header.Span.Slice(2));

        var body = memoryOwner.Memory.Slice(6, length);
        await ReadAsync(socket, body, cancellationToken);

        ExectueLegacyMessage(memoryOwner.Memory.Slice(0, header.Length + body.Length));
    }

    private void ExectueLegacyMessage(ReadOnlyMemory<byte> packet)
    {
        var legacyPacket = new PacketClass(packet.ToArray());
        legacyClientClass.OnPacket(legacyPacket);
    }

    private void DecodePacketHeader(Span<byte> data)
    {
        if (!legacyClientClass.Client.PacketEncryption.IsEncryptionEnabled)
        {
            return;
        }

        var key = legacyClientClass.Client.PacketEncryption.Key;
        var hash = legacyClientClass.Client.PacketEncryption.Hash;

        for (var i = 0; i < 6; i++)
        {
            var tmp = data[i];
            data[i] = (byte)(hash[key[1]] ^ (256 + data[i] - key[0]) % 256);
            key[0] = tmp;
            key[1] = (byte)((key[1] + 1) % 40);
        }
    }

    private async ValueTask WaitForNextPacket(Socket socket, CancellationToken cancellationToken)
    {
        await socket.ReceiveAsync(Array.Empty<byte>(), cancellationToken);
    }

    private async ValueTask ReadAsync(Socket socket, Memory<byte> buffer, CancellationToken cancellationToken)
    {
        if (buffer.Length == 0)
        {
            return;
        }

        var recieved = await socket.ReceiveAsync(buffer, cancellationToken);
        if (recieved != buffer.Length)
        {
            Debugger.Launch();
            throw new NotImplementedException();
        }
    }
}
