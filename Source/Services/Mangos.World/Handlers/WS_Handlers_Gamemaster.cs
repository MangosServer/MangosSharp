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

using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Misc;
using Mangos.World.Globals;
using Mangos.World.Network;

namespace Mangos.World.Handlers;

public class WS_Handlers_Gamemaster
{
    public void On_CMSG_WORLD_TELEPORT(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_WORLD_TELEPORT", client.IP, client.Port);
        if (client.Access >= AccessLevel.GameMaster)
        {
            packet.GetInt16();
            //int Time = packet.GetInt32(); //What is the purpose of "Time" for this packet?
            var Map = packet.GetUInt32();
            var X = packet.GetFloat();
            var Y = packet.GetFloat();
            var Z = packet.GetFloat();
            var O = packet.GetFloat();
            client.Character.Teleport(X, Y, Z, O, checked((int)Map));
        }
    }
}
