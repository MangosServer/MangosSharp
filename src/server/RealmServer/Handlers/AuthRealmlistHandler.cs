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

using Mangos.MySql.GetRealmList;
using RealmServer.Domain;
using RealmServer.Requests;
using RealmServer.Responses;

namespace RealmServer.Handlers;

internal sealed class AuthRealmlistHandler : IHandler<AuthRealmlistRequest>
{
    private readonly IGetRealmListQuery getRealmListQuery;

    public AuthRealmlistHandler(IGetRealmListQuery getRealmListQuery)
    {
        this.getRealmListQuery = getRealmListQuery;
    }

    public TcpPacketOpCodes TcpPacketOpCode => TcpPacketOpCodes.CMD_AUTH_REALMLIST;

    public async Task<IResponseMessage> ExectueAsync(AuthRealmlistRequest request)
    {
        var realmListModels = await getRealmListQuery.ExectueAsync();
        if (realmListModels == null)
        {
            throw new Exception("Unable to get realmlist from database");
        }

        return new AuthRealmlistResponse
        {
            Unk = request.Unk,
            Realms = realmListModels.Select(Map).ToList()
        };
    }

    private AuthRealmlistResponse.Realm Map(RealmListModel realm)
    {
        return new()
        {
            Address = realm.address,
            CharacterCount = realm.numchars,
            Icon = realm.icon,
            Name = realm.name,
            Population = realm.population,
            Port = realm.port,
            Realmflags = realm.realmflags,
            Timezone = realm.timezone
        };
    }
}