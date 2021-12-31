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

namespace Mangos.Realm.Network.Responses;

public class AUTH_REALMLIST
{
    public class Realm
    {
        public string Address { get; }
        public string Name { get; }
        public string Port { get; }
        public byte Timezone { get; }
        public byte Icon { get; }
        public byte Realmflags { get; }
        public float Population { get; }
        public int CharacterCount { get; }

        public Realm(string address, string name, string port, byte timezone, byte icon, byte realmflags, float population, int characterCount)
        {
            Address = address;
            Name = name;
            Port = port;
            Timezone = timezone;
            Icon = icon;
            Realmflags = realmflags;
            Population = population;
            CharacterCount = characterCount;
        }
    }

    public byte[] Unk { get; }

    public Realm[] Realms { get; }

    public AUTH_REALMLIST(byte[] unk, Realm[] realms)
    {
        Unk = unk;
        Realms = realms;
    }
}
