using Mangos.Common.Enums.Global;

namespace Mangos.Realm.Network.Responses
{
    public class AUTH_LOGON_PROOF
    {
        public AccountState AccountState { get; }
        public byte[] M2 { get; }

        public AUTH_LOGON_PROOF(AccountState accountState)
        {
            AccountState = accountState;
        }

        public AUTH_LOGON_PROOF(AccountState accountState, byte[] m2)
        {
            AccountState = accountState;
            M2 = m2;
        }
    }
}
