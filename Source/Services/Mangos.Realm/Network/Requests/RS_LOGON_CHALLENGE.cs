namespace Mangos.Realm.Network.Requests
{
    public class RS_LOGON_CHALLENGE
    {
        public byte[] Account { get; }

        public string AccountName { get; }
        public int Build { get; }

        public RS_LOGON_CHALLENGE(byte[] account, string accountName, int build)
        {
            Account = account;
            AccountName = accountName;
            Build = build;
        }
    }
}
