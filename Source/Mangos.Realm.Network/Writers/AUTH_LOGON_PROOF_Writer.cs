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

using Mangos.Common.Enums.Authentication;
using Mangos.Network.Tcp.Extensions;
using Mangos.Realm.Network.Responses;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Realm.Network.Writers;

public class AUTH_LOGON_PROOF_Writer : IPacketWriter<AUTH_LOGON_PROOF>
{
    public async ValueTask WriteAsync(ChannelWriter<byte> writer, AUTH_LOGON_PROOF packet)
    {
        await writer.WriteAsync((byte)AuthCMD.CMD_AUTH_LOGON_PROOF);
        await writer.WriteAsync((byte)packet.AccountState);

        if (packet.M2 != null)
        {
            await writer.WriteEnumerableAsync(packet.M2);
            await writer.WriteZeroNCountAsync(4);
        }
    }
}
