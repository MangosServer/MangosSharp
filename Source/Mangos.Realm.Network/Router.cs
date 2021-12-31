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

using Mangos.Common.Enums.Authentication;
using Mangos.Realm.Network.Handlers;
using System.Collections.Generic;

namespace Mangos.Realm.Network;

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
        AuthCMD authCMD = (AuthCMD)opcode;
        return handlers.ContainsKey(authCMD) ? handlers[authCMD] : null;
    }
}
