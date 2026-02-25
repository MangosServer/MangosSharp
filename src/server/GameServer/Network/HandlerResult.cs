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
using System.Buffers;

namespace GameServer.Network;

internal sealed class HandlerResult : IDisposable
{
    private readonly IResponseMessage[] messages;
    private readonly int length;

    private HandlerResult(IResponseMessage[] messages, int length)
    {
        this.messages = messages;
        this.length = length;
    }

    public static HandlerResult From(IResponseMessage responseMessage)
    {
        var memoryOwner = ArrayPool<IResponseMessage>.Shared.Rent(1);
        memoryOwner[0] = responseMessage;
        return new HandlerResult(memoryOwner, 1);
    }

    public static Task<HandlerResult> FromTask(IResponseMessage responseMessage)
    {
        return Task.FromResult(From(responseMessage));
    }

    public IEnumerable<IResponseMessage> GetResponseMessages()
    {
        return messages.Take(length);
    }

    public void Dispose()
    {
        ArrayPool<IResponseMessage>.Shared.Return(messages);
    }
}
