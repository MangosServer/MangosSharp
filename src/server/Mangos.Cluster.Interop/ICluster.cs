//
// Copyright (C) 2013-2025 getMaNGOS <https://www.getmangos.eu>
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

namespace Mangos.Cluster.Interop;

/// <summary>
/// Surface a world server uses to talk to its cluster.
///
/// On the wire (see <see cref="Protocol.InteropMethodId"/>) the calls are
/// grouped into three buckets. Methods are listed below in the same order:
///
///   1. Relay (PacketOut): hot-path packet flow back to a client.
///   2. Directives: fire-and-forget actions the cluster performs on
///      behalf of a world (drops, transfers, broadcasts, group fanout).
///   3. Control RPC: registration and request/response queries.
///
/// Methods keep their historical names so existing call sites stay stable.
/// </summary>
public interface ICluster
{
    // ---- 1. Relay ----------------------------------------------------------
    [Description("Relay: send a packet to a specific client.")]
    void ClientSend(uint id, byte[] data);

    // ---- 2. Directives (fire-and-forget) ----------------------------------
    [Description("Directive: drop the named client's connection.")]
    void ClientDrop(uint id);

    [Description("Directive: notify a client transfer.")]
    void ClientTransfer(uint id, float posX, float posY, float posZ, float ori, uint map);

    [Description("Directive: client zone/level update.")]
    void ClientUpdate(uint id, uint zone, byte level);

    [Description("Directive: set a client's chat flag.")]
    void ClientSetChatFlag(uint id, byte flag);

    [Description("Directive: send a packet to all online clients.")]
    void Broadcast(byte[] data);

    [Description("Directive: send a packet to all clients in a group.")]
    void BroadcastGroup(long groupId, byte[] data);

    [Description("Directive: send a packet to all clients in a raid.")]
    void BroadcastRaid(long groupId, byte[] data);

    [Description("Directive: send a packet to all online members of a guild.")]
    void BroadcastGuild(long guildId, byte[] data);

    [Description("Directive: send a packet to all online officers of a guild.")]
    void BroadcastGuildOfficers(long guildId, byte[] data);

    [Description("Directive: ask the cluster to push a fresh group payload to a client.")]
    void GroupRequestUpdate(uint id);

    // ---- 3. Control RPC (request/response) --------------------------------
    [Description("Control: register this world's claim on the given maps.")]
    bool Connect(string uri, List<uint> maps, IWorld world);

    [Description("Control: deregister this world's claim on the given maps.")]
    void Disconnect(string uri, List<uint> maps);

    [Description("Control: read the current crypt key for a client.")]
    byte[] ClientGetCryptKey(uint id);

    [Description("Control: list active battlefield ids of the given type.")]
    List<int> BattlefieldList(byte type);

    [Description("Control: a battlefield has finished.")]
    void BattlefieldFinish(int battlefieldId);
}
