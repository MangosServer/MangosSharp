namespace Mangos.Realm.Network.Requests
{
    public class RS_REALMLIST
    {
        public byte[] Unk { get; }

        public RS_REALMLIST(byte[] unk)
        {
            Unk = unk;
        }
    }
}
