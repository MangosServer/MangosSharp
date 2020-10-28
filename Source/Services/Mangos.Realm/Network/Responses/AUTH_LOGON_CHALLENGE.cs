namespace Mangos.Realm.Network.Responses
{
    public class AUTH_LOGON_CHALLENGE
    {
        public byte[] PublicB { get; }
        public byte[] G { get; }
        public byte[] N { get; }
        public byte[] Salt { get; }
        public byte[] CrcSalt { get; }

        public AUTH_LOGON_CHALLENGE(byte[] publicB, byte[] g, byte[] n, byte[] salt, byte[] crcSalt)
        {
            PublicB = publicB;
            G = g;
            N = n;
            Salt = salt;
            CrcSalt = crcSalt;
        }
    }
}
