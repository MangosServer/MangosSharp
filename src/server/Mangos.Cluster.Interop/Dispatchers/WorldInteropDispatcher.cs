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
using Mangos.Common.Legacy;

namespace Mangos.Cluster.Interop.Dispatchers;

/// <summary>
/// Dispatches incoming IWorld method calls from the cluster TCP connection
/// to the real IWorld implementation on the world server side.
/// </summary>
public sealed class WorldInteropDispatcher
{
    private readonly IWorld _world;

    public WorldInteropDispatcher(IWorld world)
    {
        _world = world;
    }

    public async Task<byte[]?> DispatchAsync(InteropMethodId methodId, byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        switch (methodId)
        {
            case InteropMethodId.WorldClientConnect:
            {
                var id = br.ReadUInt32();
                var clientInfo = InteropSerializer.ReadClientInfo(br);
                _world.ClientConnect(id, clientInfo);
                return null;
            }

            case InteropMethodId.WorldClientDisconnect:
            {
                var id = br.ReadUInt32();
                _world.ClientDisconnect(id);
                return null;
            }

            case InteropMethodId.WorldClientLogin:
            {
                var id = br.ReadUInt32();
                var guid = br.ReadUInt64();
                _world.ClientLogin(id, guid);
                return null;
            }

            case InteropMethodId.WorldClientLogout:
            {
                var id = br.ReadUInt32();
                _world.ClientLogout(id);
                return null;
            }

            case InteropMethodId.WorldClientPacket:
            {
                var id = br.ReadUInt32();
                var packetData = InteropSerializer.ReadByteArray(br);
                _world.ClientPacket(id, packetData);
                return null;
            }

            case InteropMethodId.WorldClientCreateCharacter:
            {
                var account = br.ReadString();
                var name = br.ReadString();
                var race = br.ReadByte();
                var classe = br.ReadByte();
                var gender = br.ReadByte();
                var skin = br.ReadByte();
                var face = br.ReadByte();
                var hairStyle = br.ReadByte();
                var hairColor = br.ReadByte();
                var facialHair = br.ReadByte();
                var outfitId = br.ReadByte();
                var result = _world.ClientCreateCharacter(account, name, race, classe, gender, skin, face, hairStyle, hairColor, facialHair, outfitId);
                using var rms = new MemoryStream();
                using var bw = new BinaryWriter(rms);
                bw.Write(result);
                return rms.ToArray();
            }

            case InteropMethodId.WorldPing:
            {
                var timestamp = br.ReadInt32();
                var latency = br.ReadInt32();
                var result = _world.Ping(timestamp, latency);
                using var rms = new MemoryStream();
                using var bw = new BinaryWriter(rms);
                bw.Write(result);
                return rms.ToArray();
            }

            case InteropMethodId.WorldGetServerInfo:
            {
                var info = _world.GetServerInfo();
                return InteropSerializer.WriteServerInfo(info);
            }

            case InteropMethodId.WorldInstanceCreateAsync:
            {
                var mapId = br.ReadUInt32();
                await _world.InstanceCreateAsync(mapId);
                return Array.Empty<byte>();
            }

            case InteropMethodId.WorldInstanceDestroy:
            {
                var mapId = br.ReadUInt32();
                _world.InstanceDestroy(mapId);
                return null;
            }

            case InteropMethodId.WorldInstanceCanCreate:
            {
                var type = br.ReadInt32();
                var result = _world.InstanceCanCreate(type);
                return new[] { result ? (byte)1 : (byte)0 };
            }

            case InteropMethodId.WorldClientSetGroup:
            {
                var id = br.ReadUInt32();
                var groupId = br.ReadInt64();
                _world.ClientSetGroup(id, groupId);
                return null;
            }

            case InteropMethodId.WorldGroupUpdate:
            {
                var groupId = br.ReadInt64();
                var groupType = br.ReadByte();
                var groupLeader = br.ReadUInt64();
                var members = InteropSerializer.ReadUInt64Array(br);
                _world.GroupUpdate(groupId, groupType, groupLeader, members);
                return null;
            }

            case InteropMethodId.WorldGroupUpdateLoot:
            {
                var groupId = br.ReadInt64();
                var difficulty = br.ReadByte();
                var method = br.ReadByte();
                var threshold = br.ReadByte();
                var master = br.ReadUInt64();
                _world.GroupUpdateLoot(groupId, difficulty, method, threshold, master);
                return null;
            }

            case InteropMethodId.WorldGroupMemberStats:
            {
                var guid = br.ReadUInt64();
                var flag = br.ReadInt32();
                var stats = _world.GroupMemberStats(guid, flag);
                return InteropSerializer.WriteByteArray(stats);
            }

            case InteropMethodId.WorldGuildUpdate:
            {
                var guid = br.ReadUInt64();
                var guildId = br.ReadUInt32();
                var guildRank = br.ReadByte();
                _world.GuildUpdate(guid, guildId, guildRank);
                return null;
            }

            case InteropMethodId.WorldBattlefieldCreate:
            {
                var battlefieldId = br.ReadInt32();
                var battlefieldMapType = br.ReadByte();
                var map = br.ReadUInt32();
                _world.BattlefieldCreate(battlefieldId, battlefieldMapType, map);
                return null;
            }

            case InteropMethodId.WorldBattlefieldDelete:
            {
                var battlefieldId = br.ReadInt32();
                _world.BattlefieldDelete(battlefieldId);
                return null;
            }

            case InteropMethodId.WorldBattlefieldJoin:
            {
                var battlefieldId = br.ReadInt32();
                var guid = br.ReadUInt64();
                _world.BattlefieldJoin(battlefieldId, guid);
                return null;
            }

            case InteropMethodId.WorldBattlefieldLeave:
            {
                var battlefieldId = br.ReadInt32();
                var guid = br.ReadUInt64();
                _world.BattlefieldLeave(battlefieldId, guid);
                return null;
            }

            default:
                return null;
        }
    }
}
