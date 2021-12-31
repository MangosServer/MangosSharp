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

using Mangos.Network.Tcp.Extensions;
using Mangos.Realm.Network.Requests;
using System;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Realm.Network.Readers;

public class RS_LOGON_CHALLENGE_Reader : IPacketReader<RS_LOGON_CHALLENGE>
{
    public async ValueTask<RS_LOGON_CHALLENGE> ReadAsync(ChannelReader<byte> reader)
    {
        await reader.ReadVoidAsync(1);
        var lengthArray = await reader.ReadArrayAsync(2);
        var length = BitConverter.ToInt16(lengthArray);
        var data = await reader.ReadArrayAsync(length);

        // Read account name from packet
        var accountLength = data[29];
        var account = new byte[accountLength];
        Array.Copy(data, 30, account, 0, accountLength);
        var accountName = Encoding.UTF8.GetString(account);

        // Get the client build from packet.
        int clientBuild = BitConverter.ToInt16(new[] { data[7], data[8] }, 0);

        return new RS_LOGON_CHALLENGE(account, accountName, clientBuild);
    }
}
