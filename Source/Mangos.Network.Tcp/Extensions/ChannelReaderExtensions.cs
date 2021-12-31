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

using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Network.Tcp.Extensions;

public static class ChannelReaderExtensions
{
    public static async ValueTask ReadToArrayAsync(this ChannelReader<byte> reader, byte[] buffer, int offset, int count)
    {
        for (var i = 0; i < count; i++)
        {
            buffer[offset + i] = await reader.ReadAsync();
        }
    }

    public static async ValueTask<byte[]> ReadArrayAsync(this ChannelReader<byte> reader, int count)
    {
        var buffer = new byte[count];
        for (var i = 0; i < count; i++)
        {
            buffer[i] = await reader.ReadAsync();
        }
        return buffer;
    }

    public static async ValueTask ReadVoidAsync(this ChannelReader<byte> reader, int count)
    {
        for (var i = 0; i < count; i++)
        {
            await reader.ReadAsync();
        }
    }
}
