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
/// Dispatches incoming ICluster method calls from a world server TCP connection
/// to the real ICluster implementation on the cluster side.
///
/// Also handles the special Connect call: creates a WorldInteropProxy from the
/// same TCP connection and passes it as the IWorld parameter.
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
        Console.WriteLine("[ClusterDispatcher] ClusterInteropDispatcher created");
    }

    /// <summary>
    /// Gets the IWorld proxy for the remote world server on this connection.
    /// </summary>
    public IWorld WorldProxy => _worldProxy;

    public byte[]? Dispatch(InteropMethodId methodId, byte[] data)
    {
        Console.WriteLine($"[ClusterDispatcher] Dispatching method: {methodId}, data size: {data.Length} bytes");
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        switch (methodId)
        {
            case InteropMethodId.ClusterConnect:
            {
                var uri = br.ReadString();
                var maps = InteropSerializer.ReadUInt32List(br);
                Console.WriteLine($"[ClusterDispatcher] ClusterConnect: uri={uri}, maps count={maps.Count}");
                var result = _cluster.Connect(uri, maps, _worldProxy);
                Console.WriteLine($"[ClusterDispatcher] ClusterConnect result: {result}");
                return new[] { result ? (byte)1 : (byte)0 };
            }

            case InteropMethodId.ClusterDisconnect:
            {
                var uri = br.ReadString();
                var maps = InteropSerializer.ReadUInt32List(br);
                Console.WriteLine($"[ClusterDispatcher] ClusterDisconnect: uri={uri}, maps count={maps.Count}");
                _cluster.Disconnect(uri, maps);
                return null;
            }

            case InteropMethodId.ClusterClientSend:
            {
                var id = br.ReadUInt32();
                var packetData = InteropSerializer.ReadByteArray(br);
                Console.WriteLine($"[ClusterDispatcher] ClientSend: clientId={id}, packet size={packetData.Length} bytes");
                _cluster.ClientSend(id, packetData);
                return null;
            }

            case InteropMethodId.ClusterClientDrop:
            {
                var id = br.ReadUInt32();
                Console.WriteLine($"[ClusterDispatcher] ClientDrop: clientId={id}");
                _cluster.ClientDrop(id);
                return null;
            }

            case InteropMethodId.ClusterClientTransfer:
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

            case InteropMethodId.ClusterClientUpdate:
            {
                var id = br.ReadUInt32();
                var zone = br.ReadUInt32();
                var level = br.ReadByte();
                _cluster.ClientUpdate(id, zone, level);
                return null;
            }

            case InteropMethodId.ClusterClientSetChatFlag:
            {
                var id = br.ReadUInt32();
                var flag = br.ReadByte();
                _cluster.ClientSetChatFlag(id, flag);
                return null;
            }

            case InteropMethodId.ClusterClientGetCryptKey:
            {
                var id = br.ReadUInt32();
                var key = _cluster.ClientGetCryptKey(id);
                return InteropSerializer.WriteByteArray(key);
            }

            case InteropMethodId.ClusterBattlefieldList:
            {
                var type = br.ReadByte();
                var list = _cluster.BattlefieldList(type);
                return InteropSerializer.WriteInt32List(list);
            }

            case InteropMethodId.ClusterBattlefieldFinish:
            {
                var battlefieldId = br.ReadInt32();
                _cluster.BattlefieldFinish(battlefieldId);
                return null;
            }

            case InteropMethodId.ClusterBroadcast:
            {
                var packetData = InteropSerializer.ReadByteArray(br);
                _cluster.Broadcast(packetData);
                return null;
            }

            case InteropMethodId.ClusterBroadcastGroup:
            {
                var groupId = br.ReadInt64();
                var packetData = InteropSerializer.ReadByteArray(br);
                _cluster.BroadcastGroup(groupId, packetData);
                return null;
            }

            case InteropMethodId.ClusterBroadcastRaid:
            {
                var groupId = br.ReadInt64();
                var packetData = InteropSerializer.ReadByteArray(br);
                _cluster.BroadcastRaid(groupId, packetData);
                return null;
            }

            case InteropMethodId.ClusterBroadcastGuild:
            {
                var guildId = br.ReadInt64();
                var packetData = InteropSerializer.ReadByteArray(br);
                _cluster.BroadcastGuild(guildId, packetData);
                return null;
            }

            case InteropMethodId.ClusterBroadcastGuildOfficers:
            {
                var guildId = br.ReadInt64();
                var packetData = InteropSerializer.ReadByteArray(br);
                _cluster.BroadcastGuildOfficers(guildId, packetData);
                return null;
            }

            case InteropMethodId.ClusterGroupRequestUpdate:
            {
                var id = br.ReadUInt32();
                _cluster.GroupRequestUpdate(id);
                return null;
            }

            default:
                return null;
        }
    }
}
