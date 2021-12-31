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

using Mangos.Realm.Network.Readers;
using Mangos.Realm.Network.Responses;
using Mangos.Realm.Network.Writers;
using Mangos.Storage.Account;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Realm.Network.Handlers;

public class RS_REALMLIST_Handler : IPacketHandler
{
    private readonly IAccountStorage accountStorage;

    private readonly RS_REALMLIST_Reader RS_REALMLIST_Reader;
    private readonly AUTH_REALMLIST_Writer AUTH_REALMLIST_Writer;

    public RS_REALMLIST_Handler(
        IAccountStorage accountStorage,
        RS_REALMLIST_Reader RS_REALMLIST_Reader,
        AUTH_REALMLIST_Writer AUTH_REALMLIST_Writer)
    {
        this.accountStorage = accountStorage;
        this.RS_REALMLIST_Reader = RS_REALMLIST_Reader;
        this.AUTH_REALMLIST_Writer = AUTH_REALMLIST_Writer;
    }

    public async Task HandleAsync(ChannelReader<byte> reader, ChannelWriter<byte> writer, Client clientModel)
    {
        var request = await RS_REALMLIST_Reader.ReadAsync(reader);

        var realmList = await accountStorage.GetRealmListAsync();

        var realms = realmList.Select(x => new AUTH_REALMLIST.Realm(
            x.address,
            x.name,
            x.port,
            x.timezone,
            x.icon,
            x.realmflags,
            x.population,
            x.numchars));

        await AUTH_REALMLIST_Writer.WriteAsync(writer, new AUTH_REALMLIST(request.Unk, realms.ToArray()));
    }
}
