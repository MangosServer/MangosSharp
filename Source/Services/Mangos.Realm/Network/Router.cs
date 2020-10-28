using Mangos.Common.Enums.Authentication;
using Mangos.Realm.Network.Handlers;
using System.Collections.Generic;

namespace Mangos.Realm.Network
{
    public class Router
    {
        private readonly Dictionary<AuthCMD, IPacketHandler> handlers;

        public Router(
            RS_LOGON_CHALLENGE_Handler RS_LOGON_CHALLENGE_Handler, 
            RS_LOGON_PROOF_Handler RS_LOGON_PROOF_Handler,
            RS_REALMLIST_Handler RS_REALMLIST_Handler,
            CMD_XFER_CANCEL_Handler CMD_XFER_CANCEL_Handler,
            CMD_XFER_ACCEPT_Handler CMD_XFER_ACCEPT_Handler,
            On_CMD_XFER_RESUME_Handler On_CMD_XFER_RESUME_Handler)
        {
            handlers = new Dictionary<AuthCMD, IPacketHandler>
            {
                [AuthCMD.CMD_AUTH_LOGON_CHALLENGE] = RS_LOGON_CHALLENGE_Handler,
                [AuthCMD.CMD_AUTH_RECONNECT_CHALLENGE] = RS_LOGON_CHALLENGE_Handler,
                [AuthCMD.CMD_AUTH_LOGON_PROOF] = RS_LOGON_PROOF_Handler,
                [AuthCMD.CMD_AUTH_REALMLIST] = RS_REALMLIST_Handler,
                [AuthCMD.CMD_XFER_CANCEL] = CMD_XFER_CANCEL_Handler,
                [AuthCMD.CMD_XFER_ACCEPT] = CMD_XFER_ACCEPT_Handler,
                [AuthCMD.CMD_XFER_RESUME] = On_CMD_XFER_RESUME_Handler
            };
        }

        public IPacketHandler GetPacketHandler(byte opcode)
        {
            var authCMD = (AuthCMD)opcode;
            return handlers.ContainsKey(authCMD) ? handlers[authCMD] : null;
        }
    }
}
