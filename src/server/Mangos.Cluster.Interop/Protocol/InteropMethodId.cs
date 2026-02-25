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

namespace Mangos.Cluster.Interop.Protocol;

/// <summary>
/// Method IDs for binary RPC calls between cluster and world servers.
/// ICluster methods (called by World → Cluster) use range 0x0001-0x00FF.
/// IWorld methods (called by Cluster → World) use range 0x0101-0x01FF.
/// </summary>
public enum InteropMethodId : ushort
{
    // ICluster methods (World → Cluster)
    ClusterConnect = 0x0001,
    ClusterDisconnect = 0x0002,
    ClusterClientSend = 0x0003,
    ClusterClientDrop = 0x0004,
    ClusterClientTransfer = 0x0005,
    ClusterClientUpdate = 0x0006,
    ClusterClientSetChatFlag = 0x0007,
    ClusterClientGetCryptKey = 0x0008,
    ClusterBattlefieldList = 0x0009,
    ClusterBattlefieldFinish = 0x000A,
    ClusterBroadcast = 0x000B,
    ClusterBroadcastGroup = 0x000C,
    ClusterBroadcastRaid = 0x000D,
    ClusterBroadcastGuild = 0x000E,
    ClusterBroadcastGuildOfficers = 0x000F,
    ClusterGroupRequestUpdate = 0x0010,

    // IWorld methods (Cluster → World)
    WorldClientConnect = 0x0101,
    WorldClientDisconnect = 0x0102,
    WorldClientLogin = 0x0103,
    WorldClientLogout = 0x0104,
    WorldClientPacket = 0x0105,
    WorldClientCreateCharacter = 0x0106,
    WorldPing = 0x0107,
    WorldGetServerInfo = 0x0108,
    WorldInstanceCreateAsync = 0x0109,
    WorldInstanceDestroy = 0x010A,
    WorldInstanceCanCreate = 0x010B,
    WorldClientSetGroup = 0x010C,
    WorldGroupUpdate = 0x010D,
    WorldGroupUpdateLoot = 0x010E,
    WorldGroupMemberStats = 0x010F,
    WorldGuildUpdate = 0x0110,
    WorldBattlefieldCreate = 0x0111,
    WorldBattlefieldDelete = 0x0112,
    WorldBattlefieldJoin = 0x0113,
    WorldBattlefieldLeave = 0x0114,

    // Protocol-level
    Response = 0xFFFF,
}
