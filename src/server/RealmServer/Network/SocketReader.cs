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

using System.Buffers;
using System.Net.Sockets;
using System.Text;

namespace RealmServer.Network;

internal sealed class SocketReader
{
    private readonly Socket socket;
    private readonly CancellationToken cancellationToken;

    private readonly ArrayPool<byte> arrayPool = ArrayPool<byte>.Shared;

    public SocketReader(Socket socket, CancellationToken cancellationToken)
    {
        this.socket = socket;
        this.cancellationToken = cancellationToken;
    }

    public async ValueTask ReadVoidAsync(int length)
    {
        var buffer = arrayPool.Rent(length);
        try
        {
            await ReadAsync(buffer, length);
        }
        finally
        {
            arrayPool.Return(buffer);
        }
    }

    public async ValueTask<byte> ReadByteAsync()
    {
        var buffer = arrayPool.Rent(1);
        try
        {
            await ReadAsync(buffer, 1);
            return buffer[0];
        }
        finally
        {
            arrayPool.Return(buffer);
        }
    }

    public async ValueTask<short> ReadInt16Async()
    {
        var buffer = arrayPool.Rent(2);
        try
        {
            await ReadAsync(buffer, 2);
            return BitConverter.ToInt16(buffer);
        }
        finally
        {
            arrayPool.Return(buffer);
        }
    }

    public async ValueTask<string> ReadStringAsync(int length)
    {
        var buffer = arrayPool.Rent(length);
        try
        {
            await ReadAsync(buffer, length);
            return Encoding.UTF8.GetString(buffer, 0, length);
        }
        finally
        {
            arrayPool.Return(buffer);
        }
    }

    public async ValueTask<byte[]> ReadByteArrayAsync(int length)
    {
        var buffer = new byte[length];
        await ReadAsync(buffer, length);
        return buffer;
    }

    private async ValueTask ReadAsync(byte[] buffer, int length)
    {
        if (length == 0)
        {
            return;
        }

        int totalRead = 0;
        while (totalRead < length)
        {
            var bytesRead = await socket.ReceiveAsync(buffer.AsMemory(totalRead, length - totalRead), cancellationToken);
            if (bytesRead == 0)
            {
                throw new SocketException((int)SocketError.ConnectionReset);
            }
            totalRead += bytesRead;
        }
    }
}
