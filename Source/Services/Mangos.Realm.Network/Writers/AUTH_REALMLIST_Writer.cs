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
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Realm.Network.Writers;

public class AUTH_REALMLIST_Writer : IPacketWriter<AUTH_REALMLIST>
{
    public async ValueTask WriteAsync(ChannelWriter<byte> writer, AUTH_REALMLIST packet)
    {
        Channel<byte> t = Channel.CreateUnbounded<byte>();
        //writer = t.Writer;

        var responseBodyLength = packet.Realms.Sum(x =>
            5
            + x.Name.Length + 1
            + x.Address.Length + 1 + x.Port.Length + 1
            + 7) + 7;

        await writer.WriteAsync((byte)AuthCMD.CMD_AUTH_REALMLIST);
        await writer.WriteAsync((byte)(responseBodyLength % 256));
        await writer.WriteAsync((byte)(responseBodyLength / 256));
        await writer.WriteEnumerableAsync(packet.Unk);

        await writer.WriteAsync((byte)packet.Realms.Length);

        foreach (var realmListItem in packet.Realms)
        {
            // (uint8) Realm Icon
            // 0 -> Normal; 1 -> PvP; 6 -> RP; 8 -> RPPvP;
            await writer.WriteAsync(realmListItem.Icon);

            // (uint8) IsLocked
            // 0 -> none; 1 -> locked
            await writer.WriteAsync(0);
            // (uint8) unk
            await writer.WriteAsync(0);
            // (uint8) unk
            await writer.WriteAsync(0);
            // (uint8) Realm Color
            // 0 -> Green; 1 -> Red; 2 -> Offline;
            await writer.WriteAsync(realmListItem.Realmflags);
            // (string) Realm Name (zero terminated)
            await writer.WriteEnumerableAsync(Encoding.UTF8.GetBytes(realmListItem.Name));
            await writer.WriteAsync(0);
            // (string) Realm Address ("ip:port", zero terminated)
            await writer.WriteEnumerableAsync(Encoding.UTF8.GetBytes($"{realmListItem.Address}:{realmListItem.Port}"));
            await writer.WriteAsync(0);
            // (float) Population
            // 400F -> Full; 5F -> Medium; 1.6F -> Low; 200F -> New; 2F -> High
            // 00 00 48 43 -> Recommended
            // 00 00 C8 43 -> Full
            // 9C C4 C0 3F -> Low
            // BC 74 B3 3F -> Low
            await writer.WriteFloatAsync(realmListItem.Population);
            // (byte) Number of character at this realm for this account
            await writer.WriteAsync((byte)realmListItem.CharacterCount);
            // (byte) Timezone
            // 0x01 - Development
            // 0x02 - USA
            // 0x03 - Oceania
            // 0x04 - LatinAmerica
            // 0x05 - Tournament
            // 0x06 - Korea
            // 0x07 - Tournament
            // 0x08 - UnitedKingdom
            // 0x09 - Germany
            // 0x0A - France
            // 0x0B - Spain
            // 0x0C - Russian
            // 0x0D - Tournament
            // 0x0E - Taiwan
            // 0x0F - Tournament
            // 0x10 - China
            // 0x11 - CN1
            // 0x12 - CN2
            // 0x13 - CN3
            // 0x14 - CN4
            // 0x15 - CN5
            // 0x16 - CN6
            // 0x17 - CN7
            // 0x18 - CN8
            // 0x19 - Tournament
            // 0x1A - Test Server
            // 0x1B - Tournament
            // 0x1C - QA Server
            // 0x1D - CN9
            // 0x1E - Test Server 2
            await writer.WriteAsync(realmListItem.Timezone);
            // (byte) Unknown (may be 2 -> TestRealm, / 6 -> ?)
            await writer.WriteAsync(0);
        }

        await writer.WriteAsync(2); // 2=list of realms 0=wizard
        await writer.WriteAsync(0);
    }
}
