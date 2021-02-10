﻿//
// Copyright (C) 2013-2021 getMaNGOS <https://getmangos.eu>
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
using Mangos.Common.Enums.Global;
using Mangos.Network.Tcp.Extensions;
using Mangos.Realm.Network.Responses;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Realm.Network.Writers
{
    public class AUTH_LOGON_CHALLENGE_Writer : IPacketWriter<AUTH_LOGON_CHALLENGE>
    {
        public async ValueTask WriteAsync(ChannelWriter<byte> writer, AUTH_LOGON_CHALLENGE packet)
        {
            await writer.WriteAsync((byte)AuthCMD.CMD_AUTH_LOGON_CHALLENGE);
            await writer.WriteAsync((byte)AccountState.LOGIN_OK);
            await writer.WriteAsync(0);
            await writer.WriteEnumerableAsync(packet.PublicB);
            await writer.WriteAsync((byte)packet.G.Length);
            await writer.WriteAsync(packet.G[0]);
            await writer.WriteAsync(32);
            await writer.WriteEnumerableAsync(packet.N);
            await writer.WriteEnumerableAsync(packet.Salt);
            await writer.WriteEnumerableAsync(packet.CrcSalt);
            await writer.WriteAsync(0);
        }
    }
}