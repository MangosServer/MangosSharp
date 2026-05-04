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

using System.ComponentModel;
using System.Threading.Tasks;

namespace Mangos.Cluster.Interop;

/// <summary>
/// Surface the cluster uses to talk to a world server.
///
/// On the wire (see <see cref="Protocol.InteropMethodId"/>) the calls are
/// grouped into two buckets. Methods are listed below in the same order:
///
///   1. Relay: per-client lifecycle and inbound WoW packet stream. The
///      cluster decrypts each packet on its end and forwards as a one-way
///      envelope; the world hands the bytes to its opcode dispatcher.
///   2. Control RPC: heartbeats, instance lifecycle, character creation,
///      group/guild/battlefield orchestration. Request/response.
///
/// Methods keep their historical names so existing call sites stay stable.
/// </summary>
public interface IWorld
{
    // ---- 1. Relay ----------------------------------------------------------
    [Description("Relay: cluster has accepted a new client; world sets up its session state.")]
    void ClientConnect(uint id, ClientInfo client);

    [Description("Relay: client is gone; world tears down its session state.")]
    void ClientDisconnect(uint id);

    [Description("Relay: client picked a character.")]
    void ClientLogin(uint id, ulong guid);

    [Description("Relay: client logged out of their character.")]
    void ClientLogout(uint id);

    [Description("Relay: a decrypted WoW packet from the client.")]
    void ClientPacket(uint id, byte[] data);

    // ---- 2. Control RPC (request/response) --------------------------------
    [Description("Control: heartbeat. Returns the world's tick.")]
    int Ping(int timestamp, int latency);

    [Description("Control: collect CPU/memory/load snapshot for the supervisor.")]
    ServerInfo GetServerInfo();

    [Description("Control: load and host an instance of the given map.")]
    Task InstanceCreateAsync(uint Map);

    [Description("Control: tear down an instance of the given map.")]
    void InstanceDestroy(uint Map);

    [Description("Control: ask whether this world can host a new instance of the given type.")]
    bool InstanceCanCreate(int Type);

    [Description("Control: create a character for the named account.")]
    int ClientCreateCharacter(string account, string name, byte race, byte classe, byte gender, byte skin, byte face, byte hairStyle, byte hairColor, byte facialHair, byte outfitId);

    [Description("Control: associate a client with a group id.")]
    void ClientSetGroup(uint ID, long GroupID);

    [Description("Control: refresh a group's roster/leader on this world.")]
    void GroupUpdate(long GroupID, byte GroupType, ulong GroupLeader, ulong[] Members);

    [Description("Control: refresh a group's loot rules on this world.")]
    void GroupUpdateLoot(long GroupID, byte Difficulty, byte Method, byte Threshold, ulong Master);

    [Description("Control: read a character's groupable stats payload.")]
    byte[] GroupMemberStats(ulong GUID, int Flag);

    [Description("Control: refresh guild membership for a character.")]
    void GuildUpdate(ulong GUID, uint GuildID, byte GuildRank);

    [Description("Control: spin up battlefield bookkeeping.")]
    void BattlefieldCreate(int BattlefieldID, byte BattlefieldMapType, uint Map);

    [Description("Control: tear down a battlefield.")]
    void BattlefieldDelete(int BattlefieldID);

    [Description("Control: a character joined a battlefield.")]
    void BattlefieldJoin(int BattlefieldID, ulong GUID);

    [Description("Control: a character left a battlefield.")]
    void BattlefieldLeave(int BattlefieldID, ulong GUID);
}
