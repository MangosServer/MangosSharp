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

using System.Buffers;
using System.Net.Sockets;

namespace Mangos.Tcp.Implementation;

internal sealed class TcpWriter : ITcpWriter
{
    private readonly Socket socket;
    private readonly ArrayPool<byte> arrayPool;

    public TcpWriter(ArrayPool<byte> arrayPool, Socket socket)
    {
        this.arrayPool = arrayPool;
        this.socket = socket;
    }

    public async ValueTask WriteByteArrayAsync(byte[] value)
    {
        await WriteAsync(value, value.Length);
    }

    public async ValueTask WriteByteAsync(byte value)
    {
        var buffer = arrayPool.Rent(sizeof(byte));
        try
        {
            buffer[0] = value;
            await WriteAsync(buffer, sizeof(byte));
        }
        finally
        {
            arrayPool.Return(buffer);
        }
    }

    public async ValueTask WriteFloatAsync(float population)
    {
        var buffer = arrayPool.Rent(sizeof(float));
        try
        {
            BitConverter.TryWriteBytes(buffer, population);
            await WriteAsync(buffer, sizeof(float));
        }
        finally
        {
            arrayPool.Return(buffer);
        }
    }

    public async ValueTask WriteZeroBytesAsync(int count)
    {
        var buffer = arrayPool.Rent(count);
        try
        {
            Array.Fill<byte>(buffer, 0);
            await WriteAsync(buffer, count);
        }
        finally
        {
            arrayPool.Return(buffer);
        }
    }

    private async ValueTask WriteAsync(byte[] buffer, int length)
    {
        await socket.SendAsync(buffer.AsMemory(0, length));
    }
}
