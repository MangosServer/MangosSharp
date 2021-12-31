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

using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Network.Tcp.Extensions;

public static class ChannelWriterExtensions
{
    public static async ValueTask WriteEnumerableAsync(this ChannelWriter<byte> writer, IEnumerable<byte> data)
    {
        foreach (var item in data)
        {
            await writer.WriteAsync(item);
        }
    }

    public static async ValueTask WriteFloatAsync(this ChannelWriter<byte> writer, float data)
    {
        await writer.WriteEnumerableAsync(BitConverter.GetBytes(data));
    }

    public static async ValueTask WriteZeroNCountAsync(this ChannelWriter<byte> writer, int count)
    {
        for (var i = 0; i < count; i++)
        {
            await writer.WriteAsync(0);
        }
    }

    public static async ValueTask WriteArrayAsync(this ChannelWriter<byte> writer, byte[] data, int count)
    {
        for (var i = 0; i < count; i++)
        {
            await writer.WriteAsync(data[i]);
        }
    }
}
