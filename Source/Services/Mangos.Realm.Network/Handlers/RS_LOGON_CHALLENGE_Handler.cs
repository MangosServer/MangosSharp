using Mangos.Network.Tcp.Mvc;
using Mangos.Realm.Network.Requests;
using Mangos.Realm.Network.Responses;
using System.Threading.Tasks;

namespace Mangos.Realm.Network.Handlers
{
    public class RS_LOGON_CHALLENGE_Handler : IPacketHandler<RS_LOGON_CHALLENGE, ClientContext, AUTH_LOGON_PROOF>
    {
        public async Task<AUTH_LOGON_PROOF> HandleAsync(RS_LOGON_CHALLENGE request, ClientContext context)
        {
            return new AUTH_LOGON_PROOF();
        }
    }
}
