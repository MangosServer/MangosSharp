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

namespace Mangos.Cluster.Admin.Protocol;

/// <summary>
/// Method ids carried in federation envelopes (cluster <-> cluster).
///
/// Range 0x0300-0x03FF on the shared interop framing. Buckets:
///
///   * 0x0300-0x030F: peer handshake + heartbeat.
///   * 0x0310-0x031F: admin RPC (server/instance/realm commands).
///   * 0x0320-0x033F: chat envelopes (PR #6).
///   * 0x0340-0x035F: group/raid envelopes (PR #6).
///   * 0x0360-0x036F: shard/co-location (Phase B).
///   * 0x0370-0x037F: presence/lookup.
///
/// 0xFFFF stays reserved for the framing layer.
/// </summary>
public enum AdminMethodId : ushort
{
    // ----- Peer handshake -----------------------------------------------
    PeerHello = 0x0300,
    PeerHelloAck = 0x0301,
    PeerHeartbeat = 0x0302,
    PeerGoodbye = 0x0303,

    // ----- Admin RPC ----------------------------------------------------
    AdminCommand = 0x0310,
    AdminCommandReply = 0x0311,

    // ----- Chat (PR #6) -------------------------------------------------
    ChatRoute = 0x0320,

    // ----- Group / raid (PR #6) -----------------------------------------
    GroupInvite = 0x0340,
    GroupInviteResponse = 0x0341,
    GroupRosterUpdate = 0x0342,
    GroupKick = 0x0343,
    GroupDisband = 0x0344,
    GroupMemberStatus = 0x0345,

    // ----- Shard / co-location (Phase B) --------------------------------
    ShardClaim = 0x0360,
    ShardRelease = 0x0361,

    // ----- Presence / lookup --------------------------------------------
    PresenceQuery = 0x0370,
    PresenceReply = 0x0371,
}
