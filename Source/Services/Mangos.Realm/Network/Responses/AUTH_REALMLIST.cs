namespace Mangos.Realm.Network.Responses
{
    public class AUTH_REALMLIST
    {
        public class Realm
        {
            public string Address { get;  }
            public string Name { get;  }
            public string Port { get; }
            public byte Timezone { get; }
            public byte Icon { get;  }
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
}
