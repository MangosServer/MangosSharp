namespace Mangos.Realm.Network.Requests
{
    public class RS_LOGON_PROOF
    {
        public byte[] A { get; }

        public byte[] M1 { get; }

        public RS_LOGON_PROOF(byte[] a, byte[] m1)
        {
            A = a;
            M1 = m1;
        }
    }
}
