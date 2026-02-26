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

namespace Mangos.Cluster.Interop.Proxies;

/// <summary>
/// IWorld proxy that serializes method calls over TCP to a world server.
/// Used by the cluster to communicate with remote world server processes.
/// </summary>
public sealed class WorldInteropProxy : IWorld
{
    private readonly InteropConnection _connection;

    public WorldInteropProxy(InteropConnection connection)
    {
        _connection = connection;
    }

    public void ClientConnect(uint id, ClientInfo client)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(id);
        bw.Write(client.Index);
        bw.Write(client.IP ?? string.Empty);
        bw.Write(client.Port);
        bw.Write(client.Account ?? string.Empty);
        bw.Write((byte)client.Access);
        bw.Write((byte)client.Expansion);

        _connection.SendOneWayAsync(InteropMethodId.WorldClientConnect, ms.ToArray()).GetAwaiter().GetResult();
    }

    public void ClientDisconnect(uint id)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(id);

        _connection.SendOneWayAsync(InteropMethodId.WorldClientDisconnect, ms.ToArray()).GetAwaiter().GetResult();
    }

    public void ClientLogin(uint id, ulong guid)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(id);
        bw.Write(guid);

        _connection.SendOneWayAsync(InteropMethodId.WorldClientLogin, ms.ToArray()).GetAwaiter().GetResult();
    }

    public void ClientLogout(uint id)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(id);

        _connection.SendOneWayAsync(InteropMethodId.WorldClientLogout, ms.ToArray()).GetAwaiter().GetResult();
    }

    public void ClientPacket(uint id, byte[] data)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(id);
        bw.Write(data.Length);
        bw.Write(data);

        _connection.SendOneWayAsync(InteropMethodId.WorldClientPacket, ms.ToArray()).GetAwaiter().GetResult();
    }

    public int ClientCreateCharacter(string account, string name, byte race, byte classe, byte gender, byte skin, byte face, byte hairStyle, byte hairColor, byte facialHair, byte outfitId)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(account);
        bw.Write(name);
        bw.Write(race);
        bw.Write(classe);
        bw.Write(gender);
        bw.Write(skin);
        bw.Write(face);
        bw.Write(hairStyle);
        bw.Write(hairColor);
        bw.Write(facialHair);
        bw.Write(outfitId);

        var response = _connection.SendRequestAsync(InteropMethodId.WorldClientCreateCharacter, ms.ToArray()).GetAwaiter().GetResult();
        if (response.Length >= 4)
        {
            using var rms = new MemoryStream(response);
            using var br = new BinaryReader(rms);
            return br.ReadInt32();
        }
        return 0;
    }

    public int Ping(int timestamp, int latency)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(timestamp);
        bw.Write(latency);

        var response = _connection.SendRequestAsync(InteropMethodId.WorldPing, ms.ToArray()).GetAwaiter().GetResult();
        if (response.Length >= 4)
        {
            using var rms = new MemoryStream(response);
            using var br = new BinaryReader(rms);
            return br.ReadInt32();
        }
        return 0;
    }

    public ServerInfo GetServerInfo()
    {
        var response = _connection.SendRequestAsync(InteropMethodId.WorldGetServerInfo, Array.Empty<byte>()).GetAwaiter().GetResult();
        using var ms = new MemoryStream(response);
        using var br = new BinaryReader(ms);
        return InteropSerializer.ReadServerInfo(br);
    }

    public async Task InstanceCreateAsync(uint Map)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(Map);

        await _connection.SendRequestAsync(InteropMethodId.WorldInstanceCreateAsync, ms.ToArray());
    }

    public void InstanceDestroy(uint Map)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(Map);

        _connection.SendOneWayAsync(InteropMethodId.WorldInstanceDestroy, ms.ToArray()).GetAwaiter().GetResult();
    }

    public bool InstanceCanCreate(int Type)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(Type);

        var response = _connection.SendRequestAsync(InteropMethodId.WorldInstanceCanCreate, ms.ToArray()).GetAwaiter().GetResult();
        return response.Length >= 1 && response[0] != 0;
    }

    public void ClientSetGroup(uint ID, long GroupID)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(ID);
        bw.Write(GroupID);

        _connection.SendOneWayAsync(InteropMethodId.WorldClientSetGroup, ms.ToArray()).GetAwaiter().GetResult();
    }

    public void GroupUpdate(long GroupID, byte GroupType, ulong GroupLeader, ulong[] Members)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(GroupID);
        bw.Write(GroupType);
        bw.Write(GroupLeader);
        bw.Write(Members.Length);
        foreach (var m in Members)
        {
            bw.Write(m);
        }

        _connection.SendOneWayAsync(InteropMethodId.WorldGroupUpdate, ms.ToArray()).GetAwaiter().GetResult();
    }

    public void GroupUpdateLoot(long GroupID, byte Difficulty, byte Method, byte Threshold, ulong Master)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(GroupID);
        bw.Write(Difficulty);
        bw.Write(Method);
        bw.Write(Threshold);
        bw.Write(Master);

        _connection.SendOneWayAsync(InteropMethodId.WorldGroupUpdateLoot, ms.ToArray()).GetAwaiter().GetResult();
    }

    public byte[] GroupMemberStats(ulong GUID, int Flag)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(GUID);
        bw.Write(Flag);

        var response = _connection.SendRequestAsync(InteropMethodId.WorldGroupMemberStats, ms.ToArray()).GetAwaiter().GetResult();
        using var rms = new MemoryStream(response);
        using var br = new BinaryReader(rms);
        return InteropSerializer.ReadByteArray(br);
    }

    public void GuildUpdate(ulong GUID, uint GuildID, byte GuildRank)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(GUID);
        bw.Write(GuildID);
        bw.Write(GuildRank);

        _connection.SendOneWayAsync(InteropMethodId.WorldGuildUpdate, ms.ToArray()).GetAwaiter().GetResult();
    }

    public void BattlefieldCreate(int BattlefieldID, byte BattlefieldMapType, uint Map)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(BattlefieldID);
        bw.Write(BattlefieldMapType);
        bw.Write(Map);

        _connection.SendOneWayAsync(InteropMethodId.WorldBattlefieldCreate, ms.ToArray()).GetAwaiter().GetResult();
    }

    public void BattlefieldDelete(int BattlefieldID)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(BattlefieldID);

        _connection.SendOneWayAsync(InteropMethodId.WorldBattlefieldDelete, ms.ToArray()).GetAwaiter().GetResult();
    }

    public void BattlefieldJoin(int BattlefieldID, ulong GUID)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(BattlefieldID);
        bw.Write(GUID);

        _connection.SendOneWayAsync(InteropMethodId.WorldBattlefieldJoin, ms.ToArray()).GetAwaiter().GetResult();
    }

    public void BattlefieldLeave(int BattlefieldID, ulong GUID)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(BattlefieldID);
        bw.Write(GUID);

        _connection.SendOneWayAsync(InteropMethodId.WorldBattlefieldLeave, ms.ToArray()).GetAwaiter().GetResult();
    }
}
