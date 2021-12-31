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

using System.Collections.Generic;
using System.ComponentModel;

namespace Mangos.Common.Legacy;

public interface ICluster
{
    [Description("Signal realm server for new world server.")]
    bool Connect(string uri, List<uint> maps);

    [Description("Signal realm server for disconected world server.")]
    void Disconnect(string uri, List<uint> maps);

    [Description("Send data packet to client.")]
    void ClientSend(uint id, byte[] data);

    [Description("Notify client drop.")]
    void ClientDrop(uint id);

    [Description("Notify client transfer.")]
    void ClientTransfer(uint id, float posX, float posY, float posZ, float ori, uint map);

    [Description("Notify client update.")]
    void ClientUpdate(uint id, uint zone, byte level);

    [Description("Set client chat flag.")]
    void ClientSetChatFlag(uint id, byte flag);

    [Description("Get client crypt key.")]
    byte[] ClientGetCryptKey(uint id);

    List<int> BattlefieldList(byte type);

    void BattlefieldFinish(int battlefieldId);

    [Description("Send data packet to all clients online.")]
    void Broadcast(byte[] data);

    [Description("Send data packet to all clients in specified client's group.")]
    void BroadcastGroup(long groupId, byte[] data);

    [Description("Send data packet to all clients in specified client's raid.")]
    void BroadcastRaid(long groupId, byte[] data);

    [Description("Send data packet to all clients in specified client's guild.")]
    void BroadcastGuild(long guildId, byte[] data);

    [Description("Send data packet to all clients in specified client's guild officers.")]
    void BroadcastGuildOfficers(long guildId, byte[] data);

    [Description("Send update for the requested group.")]
    void GroupRequestUpdate(uint id);

    void Dispose();
}
