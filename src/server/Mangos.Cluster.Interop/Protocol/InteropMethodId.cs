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
/// Wire-level method identifiers for the cluster <-> world interop link.
///
/// The protocol is organized into three buckets so the cluster can act
/// as a packet proxy rather than a 30+ method RPC server:
///
/// 1. Client relay (0x0200-0x020F): per-client lifecycle and the WoW
///    packet stream. This is the hot path - the vast majority of traffic.
///    Cluster receives, decrypts, then forwards inbound packets to the
///    world that owns that client. Outbound packets travel the same way
///    in reverse.
///
/// 2. Cluster directives (0x0210-0x021F): fire-and-forget instructions
///    from a world to the cluster ("send this to that client", "drop
///    this client", "broadcast this to a group"). The world never
///    learns about clients beyond the ids the cluster gave it.
///
/// 3. Control RPC (0x0220-0x023F): request/response control plane for
///    things that cannot be fire-and-forget: registration, instance
///    spawning, character creation, party stats. Bidirectional - both
///    sides can originate calls.
///
/// 0xFFFF is reserved for the framing layer to mark response frames.
///
/// The 0x02xx range deliberately leaves room above (0x0300+) for the
/// federation channel introduced in PR #4 (cluster <-> cluster).
/// </summary>
public enum InteropMethodId : ushort
{
    // ----- Client relay (cluster <-> world) ----------------------------
    ClientAttach = 0x0200,
    ClientDetach = 0x0201,
    ClientLogin = 0x0202,
    ClientLogout = 0x0203,
    PacketIn = 0x0204,   // cluster -> world (decrypted client packet)
    PacketOut = 0x0205,  // world -> cluster (packet to send to client)

    // ----- Cluster directives (world -> cluster) -----------------------
    DirectiveDropClient = 0x0210,
    DirectiveTransferClient = 0x0211,
    DirectiveUpdateClient = 0x0212,
    DirectiveSetClientChatFlag = 0x0213,
    DirectiveBroadcast = 0x0214,
    DirectiveBroadcastGroup = 0x0215,
    DirectiveBroadcastRaid = 0x0216,
    DirectiveBroadcastGuild = 0x0217,
    DirectiveBroadcastGuildOfficers = 0x0218,
    DirectiveGroupRequestUpdate = 0x0219,

    // ----- Control RPC (bidirectional) ---------------------------------
    // World -> cluster
    ControlWorldHello = 0x0220,
    ControlWorldGoodbye = 0x0221,
    ControlGetCryptKey = 0x0222,
    ControlBattlefieldList = 0x0223,
    ControlBattlefieldFinish = 0x0224,

    // Cluster -> world
    ControlPing = 0x0230,
    ControlGetServerInfo = 0x0231,
    ControlInstanceCreate = 0x0232,
    ControlInstanceDestroy = 0x0233,
    ControlInstanceCanCreate = 0x0234,
    ControlClientCreateCharacter = 0x0235,
    ControlClientSetGroup = 0x0236,
    ControlGroupUpdate = 0x0237,
    ControlGroupUpdateLoot = 0x0238,
    ControlGroupMemberStats = 0x0239,
    ControlGuildUpdate = 0x023A,
    ControlBattlefieldCreate = 0x023B,
    ControlBattlefieldDelete = 0x023C,
    ControlBattlefieldJoin = 0x023D,
    ControlBattlefieldLeave = 0x023E,

    // World -> cluster (admin)
    ControlRunAdminCommand = 0x0240,

    // World -> cluster (federation gateway). World hands the cluster a
    // serialized cross-realm envelope; cluster routes via FederationRouter.
    ControlRouteFederatedChat = 0x0241,
    ControlRouteFederatedGroupInvite = 0x0242,

    // World -> cluster (Phase B shard lookup). World asks before loading
    // an instance whether a federated shard claims this map for this
    // character; the cluster consults its ShardRegistry.
    ControlQueryShard = 0x0243,

    // ----- Framing -----------------------------------------------------
    Response = 0xFFFF,
}
