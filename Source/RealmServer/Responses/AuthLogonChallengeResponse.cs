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

namespace RealmServer.Responses;

public sealed class AuthLogonChallengeResponse : IResponseMessage
{
    public required byte[] PublicB { get; init; }
    public required byte[] G { get; init; }
    public required byte[] N { get; init; }
    public required byte[] Salt { get; init; }
    public required byte[] CrcSalt { get; init; }

    public async ValueTask WriteAsync(ITcpWriter writer)
    {
        await writer.WriteByteAsync((byte)TcpPacketOpCodes.CMD_AUTH_LOGON_CHALLENGE);
        await writer.WriteByteAsync((byte)AccountStates.LOGIN_OK);
        await writer.WriteZeroBytesAsync(1);
        await writer.WriteByteArrayAsync(PublicB);
        await writer.WriteByteAsync((byte)G.Length);
        await writer.WriteByteAsync(G[0]);
        await writer.WriteByteAsync(32);
        await writer.WriteByteArrayAsync(N);
        await writer.WriteByteArrayAsync(Salt);
        await writer.WriteByteArrayAsync(CrcSalt);
        await writer.WriteZeroBytesAsync(1);
    }
}
