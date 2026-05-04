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

using Mangos.Cluster.Interop.Protocol;
using Mangos.Cluster.Interop.Proxies;

namespace Mangos.Cluster.Interop.Dispatchers;

/// <summary>
/// Cluster-side dispatcher: turns inbound envelopes from a world server into
/// ICluster method calls. The IWorld counterpart is a WorldInteropProxy
/// bound to the same connection - exposed here so the cluster can route
/// outbound (cluster -> world) traffic over it.
///
/// Envelope organization mirrors <see cref="InteropMethodId"/>:
///   1. Relay: PacketOut (world -> cluster).
///   2. Directives: drop/transfer/update/chat-flag/broadcast/group-update.
///   3. Control RPC: world hello/goodbye, crypt-key read, battlefield list.
/// </summary>
public sealed class ClusterInteropDispatcher
{
    private readonly ICluster _cluster;
    private readonly InteropConnection _connection;
    private readonly WorldInteropProxy _worldProxy;

    public ClusterInteropDispatcher(ICluster cluster, InteropConnection connection)
    {
        _cluster = cluster;
        _connection = connection;
        _worldProxy = new WorldInteropProxy(connection);
    }

    /// <summary>The IWorld proxy for the world server on this connection.</summary>
    public IWorld WorldProxy => _worldProxy;

    public byte[]? Dispatch(InteropMethodId methodId, byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        switch (methodId)
        {
            // ---- 1. Relay -----------------------------------------------
            case InteropMethodId.PacketOut:
                {
                    var id = br.ReadUInt32();
                    var packet = InteropSerializer.ReadByteArray(br);
                    _cluster.ClientSend(id, packet);
                    return null;
                }

            // ---- 2. Directives ------------------------------------------
            case InteropMethodId.DirectiveDropClient:
                {
                    var id = br.ReadUInt32();
                    _cluster.ClientDrop(id);
                    return null;
                }

            case InteropMethodId.DirectiveTransferClient:
                {
                    var id = br.ReadUInt32();
                    var posX = br.ReadSingle();
                    var posY = br.ReadSingle();
                    var posZ = br.ReadSingle();
                    var ori = br.ReadSingle();
                    var map = br.ReadUInt32();
                    _cluster.ClientTransfer(id, posX, posY, posZ, ori, map);
                    return null;
                }

            case InteropMethodId.DirectiveUpdateClient:
                {
                    var id = br.ReadUInt32();
                    var zone = br.ReadUInt32();
                    var level = br.ReadByte();
                    _cluster.ClientUpdate(id, zone, level);
                    return null;
                }

            case InteropMethodId.DirectiveSetClientChatFlag:
                {
                    var id = br.ReadUInt32();
                    var flag = br.ReadByte();
                    _cluster.ClientSetChatFlag(id, flag);
                    return null;
                }

            case InteropMethodId.DirectiveBroadcast:
                {
                    var packet = InteropSerializer.ReadByteArray(br);
                    _cluster.Broadcast(packet);
                    return null;
                }

            case InteropMethodId.DirectiveBroadcastGroup:
                {
                    var groupId = br.ReadInt64();
                    var packet = InteropSerializer.ReadByteArray(br);
                    _cluster.BroadcastGroup(groupId, packet);
                    return null;
                }

            case InteropMethodId.DirectiveBroadcastRaid:
                {
                    var groupId = br.ReadInt64();
                    var packet = InteropSerializer.ReadByteArray(br);
                    _cluster.BroadcastRaid(groupId, packet);
                    return null;
                }

            case InteropMethodId.DirectiveBroadcastGuild:
                {
                    var guildId = br.ReadInt64();
                    var packet = InteropSerializer.ReadByteArray(br);
                    _cluster.BroadcastGuild(guildId, packet);
                    return null;
                }

            case InteropMethodId.DirectiveBroadcastGuildOfficers:
                {
                    var guildId = br.ReadInt64();
                    var packet = InteropSerializer.ReadByteArray(br);
                    _cluster.BroadcastGuildOfficers(guildId, packet);
                    return null;
                }

            case InteropMethodId.DirectiveGroupRequestUpdate:
                {
                    var id = br.ReadUInt32();
                    _cluster.GroupRequestUpdate(id);
                    return null;
                }

            // ---- 3. Control RPC -----------------------------------------
            case InteropMethodId.ControlWorldHello:
                {
                    var uri = br.ReadString();
                    var maps = InteropSerializer.ReadUInt32List(br);
                    var result = _cluster.Connect(uri, maps, _worldProxy);
                    return new[] { result ? (byte)1 : (byte)0 };
                }

            case InteropMethodId.ControlWorldGoodbye:
                {
                    var uri = br.ReadString();
                    var maps = InteropSerializer.ReadUInt32List(br);
                    _cluster.Disconnect(uri, maps);
                    return null;
                }

            case InteropMethodId.ControlGetCryptKey:
                {
                    var id = br.ReadUInt32();
                    var key = _cluster.ClientGetCryptKey(id);
                    return InteropSerializer.WriteByteArray(key);
                }

            case InteropMethodId.ControlBattlefieldList:
                {
                    var type = br.ReadByte();
                    var list = _cluster.BattlefieldList(type);
                    return InteropSerializer.WriteInt32List(list);
                }

            case InteropMethodId.ControlBattlefieldFinish:
                {
                    var battlefieldId = br.ReadInt32();
                    _cluster.BattlefieldFinish(battlefieldId);
                    return null;
                }

            case InteropMethodId.ControlRunAdminCommand:
                {
                    var cmdBytes = InteropSerializer.ReadByteArray(br);
                    var reply = _cluster.RunAdminCommand(cmdBytes);
                    return InteropSerializer.WriteByteArray(reply);
                }

            case InteropMethodId.ControlRouteFederatedChat:
                {
                    var realm = br.ReadUInt32();
                    var env = InteropSerializer.ReadByteArray(br);
                    _cluster.RouteFederatedChat(realm, env);
                    return null;
                }

            case InteropMethodId.ControlRouteFederatedGroupInvite:
                {
                    var realm = br.ReadUInt32();
                    var env = InteropSerializer.ReadByteArray(br);
                    _cluster.RouteFederatedGroupInvite(realm, env);
                    return null;
                }

            default:
                return null;
        }
    }
}
