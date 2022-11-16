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

using RealmServer.Domain;
using RealmServer.Network;
using System.Text;

namespace RealmServer.Responses;

internal sealed class AuthRealmlistResponse : IResponseMessage
{
    public sealed class Realm
    {
        public required string Address { get; init; }
        public required string Name { get; init; }
        public required string Port { get; init; }
        public required byte Timezone { get; init; }
        public required byte Icon { get; init; }
        public required byte Realmflags { get; init; }
        public required float Population { get; init; }
        public required int CharacterCount { get; init; }
    }

    public required byte[] Unk { get; init; }

    public required List<Realm> Realms { get; init; }

    public async ValueTask WriteAsync(SocketWriter writer)
    {
        var responseBodyLength = Realms.Sum(x =>
            5
            + x.Name.Length + 1
            + x.Address.Length + 1 + x.Port.Length + 1
            + 7) + 7;

        await writer.WriteByteAsync((byte)TcpPacketOpCodes.CMD_AUTH_REALMLIST);
        await writer.WriteByteAsync((byte)(responseBodyLength % 256));
        await writer.WriteByteAsync((byte)(responseBodyLength / 256));
        await writer.WriteByteArrayAsync(Unk);
        await writer.WriteByteAsync((byte)Realms.Count);

        foreach (var realmListItem in Realms)
        {
            // (uint8) Realm Icon
            // 0 -> Normal; 1 -> PvP; 6 -> RP; 8 -> RPPvP;
            await writer.WriteByteAsync(realmListItem.Icon);

            // (uint8) IsLocked
            // 0 -> none; 1 -> locked
            // (uint8) unk
            // (uint8) unk
            await writer.WriteZeroBytesAsync(3);
            // (uint8) Realm Color
            // 0 -> Green; 1 -> Red; 2 -> Offline;
            await writer.WriteByteAsync(realmListItem.Realmflags);
            // (string) Realm Name (zero terminated)
            await writer.WriteByteArrayAsync(Encoding.UTF8.GetBytes(realmListItem.Name));
            await writer.WriteZeroBytesAsync(1);
            // (string) Realm Address ("ip:port", zero terminated)
            await writer.WriteByteArrayAsync(Encoding.UTF8.GetBytes($"{realmListItem.Address}:{realmListItem.Port}"));
            await writer.WriteZeroBytesAsync(1);
            // (float) Population
            // 400F -> Full; 5F -> Medium; 1.6F -> Low; 200F -> New; 2F -> High
            // 00 00 48 43 -> Recommended
            // 00 00 C8 43 -> Full
            // 9C C4 C0 3F -> Low
            // BC 74 B3 3F -> Low
            await writer.WriteFloatAsync(realmListItem.Population);
            // (byte) Number of character at this realm for this account
            await writer.WriteByteAsync((byte)realmListItem.CharacterCount);
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
            await writer.WriteByteAsync(realmListItem.Timezone);
            // (byte) Unknown (may be 2 -> TestRealm, / 6 -> ?)
            await writer.WriteZeroBytesAsync(1);
        }

        await writer.WriteByteAsync(2); // 2=list of realms 0=wizard
        await writer.WriteByteAsync(0);
    }
}
