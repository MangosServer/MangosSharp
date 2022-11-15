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

namespace RealmServer.Requests;

internal sealed class RsLogonChallengeRequest : IRequestMessage<RsLogonChallengeRequest>
{
    public required string AccountName { get; init; }
    public required WowClientBuildVersions ClientBuild { get; init; }

    public static async ValueTask<RsLogonChallengeRequest> ReadAsync(ITcpReader reader)
    {
        await reader.ReadVoidAsync(10);
        var clientBuild = await reader.ReadInt16Async();
        await reader.ReadVoidAsync(20);
        var accountLength = await reader.ReadByteAsync();
        var accountName = await reader.ReadStringAsync(accountLength);

        return new()
        {
            AccountName = accountName,
            ClientBuild = (WowClientBuildVersions)clientBuild
        };
    }
}
