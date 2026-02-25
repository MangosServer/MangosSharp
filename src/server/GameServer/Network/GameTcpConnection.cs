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

using GameServer.Responses;
using Mangos.Cluster.Globals;
using Mangos.Cluster.Network;
using Mangos.Tcp;
using System.Buffers;
using System.Buffers.Binary;
using System.Net.Sockets;

namespace GameServer.Network;

internal sealed class GameTcpConnection : ITcpConnection
{
    private const int MAX_PACKET_LENGTH = 10000;

    private readonly ClientClass legacyClientClass;
    private readonly IHandlerDispatcher[] dispatchers;

    private readonly MemoryPool<byte> memoryPool = MemoryPool<byte>.Shared;

    public GameTcpConnection(ClientClass legacyClientClass, IEnumerable<IHandlerDispatcher> dispatchers)
    {
        this.legacyClientClass = legacyClientClass;

        this.dispatchers = dispatchers.ToArray();
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
        using var memoryOwner = memoryPool.Rent(MAX_PACKET_LENGTH);
        var header = await ReadPacketHeaderAsync(socket, memoryOwner.Memory, cancellationToken);
        var body = await ReadPacketBodyAsync(socket, memoryOwner.Memory, cancellationToken);

        var opcode = (Opcodes)BinaryPrimitives.ReadUInt32LittleEndian(header.Span.Slice(2));

        var dispatcher = dispatchers.FirstOrDefault(x => x.Opcode == opcode);
        if (dispatcher != null)
        {
            await ExecuteHandlerAsync(dispatcher, body, socket, cancellationToken);
        }
        else
        {
            ExecuteLegacyHandler(memoryOwner.Memory.Slice(0, header.Length + body.Length));
        }
    }

    private async Task ExecuteHandlerAsync(IHandlerDispatcher dispatcher, Memory<byte> body, Socket socket, CancellationToken cancellationToken)
    {
        using var result = await dispatcher.ExectueAsync(new PacketReader(body));
        using var memoryOwner = memoryPool.Rent(MAX_PACKET_LENGTH);
        foreach (var response in result.GetResponseMessages())
        {
            await SendAsync(socket, memoryOwner.Memory, response, cancellationToken);
        }
    }

    private void ExecuteLegacyHandler(ReadOnlyMemory<byte> packet)
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

    public void EncodePacketHeader(Span<byte> data)
    {
        if (!legacyClientClass.Client.PacketEncryption.IsEncryptionEnabled)
        {
            return;
        }

        var key = legacyClientClass.Client.PacketEncryption.Key;
        var hash = legacyClientClass.Client.PacketEncryption.Hash;
        for (var i = 0; i < 4; i++)
        {
            data[i] = (byte)(((hash[key[3]] ^ data[i]) + key[2]) % 256);
            key[2] = data[i];
            key[3] = (byte)((key[3] + 1) % 40);
        }
    }

    private async ValueTask WaitForNextPacket(Socket socket, CancellationToken cancellationToken)
    {
        await socket.ReceiveAsync(Array.Empty<byte>(), cancellationToken);
    }

    private async ValueTask<Memory<byte>> ReadPacketHeaderAsync(Socket socket, Memory<byte> buffer, CancellationToken cancellationToken)
    {
        var header = buffer.Slice(0, 6);
        await ReadAsync(socket, header, cancellationToken);
        DecodePacketHeader(header.Span);
        return header;
    }

    private async ValueTask<Memory<byte>> ReadPacketBodyAsync(Socket socket, Memory<byte> buffer, CancellationToken cancellationToken)
    {
        var length = BinaryPrimitives.ReadUInt16BigEndian(buffer.Span) - 4;
        var body = buffer.Slice(6, length);
        await ReadAsync(socket, body, cancellationToken);
        return body;
    }

    private async ValueTask SendAsync(Socket socket, Memory<byte> buffer, IResponseMessage response, CancellationToken cancellationToken)
    {
        var packetWriter = new PacketWriter(buffer, response.Opcode);
        response.Write(packetWriter);
        var packet = packetWriter.ToPacket();
        EncodePacketHeader(packet.Span);
        await SendAsync(socket, packet, cancellationToken);
    }

    private async ValueTask ReadAsync(Socket socket, Memory<byte> buffer, CancellationToken cancellationToken)
    {
        if (buffer.Length == 0)
        {
            return;
        }

        var length = await socket.ReceiveAsync(buffer, cancellationToken);
        if (length != buffer.Length)
        {
            throw new NotImplementedException("Invalid number of bytes was readed from socket");
        }
    }

    private async ValueTask SendAsync(Socket socket, ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
    {
        var length = await socket.SendAsync(buffer, cancellationToken);
        if (length != buffer.Length)
        {
            throw new NotImplementedException("Invalid number of bytes was sended to socket");
        }
    }
}
