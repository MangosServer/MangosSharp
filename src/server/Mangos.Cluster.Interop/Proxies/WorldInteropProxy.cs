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

namespace Mangos.Cluster.Interop.Proxies;

/// <summary>
/// Cluster-side stub of IWorld: each method serializes its arguments into
/// the appropriate envelope and ships it down the IPC connection to the
/// world server process.
///
/// Layout matches <see cref="InteropMethodId"/>: relay (attach/detach/login/
/// logout/PacketIn) and control RPC (ping, server info, instance lifecycle,
/// character creation, group/guild/battlefield orchestration).
/// </summary>
public sealed class WorldInteropProxy : IWorld
{
    private readonly InteropConnection _connection;

    public WorldInteropProxy(InteropConnection connection)
    {
        _connection = connection;
    }

    // ---- 1. Relay --------------------------------------------------------
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

        _connection.SendOneWayAsync(InteropMethodId.ClientAttach, ms.ToArray()).GetAwaiter().GetResult();
    }

    public void ClientDisconnect(uint id)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(id);

        _connection.SendOneWayAsync(InteropMethodId.ClientDetach, ms.ToArray()).GetAwaiter().GetResult();
    }

    public void ClientLogin(uint id, ulong guid)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(id);
        bw.Write(guid);

        _connection.SendOneWayAsync(InteropMethodId.ClientLogin, ms.ToArray()).GetAwaiter().GetResult();
    }

    public void ClientLogout(uint id)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(id);

        _connection.SendOneWayAsync(InteropMethodId.ClientLogout, ms.ToArray()).GetAwaiter().GetResult();
    }

    public void ClientPacket(uint id, byte[] data)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(id);
        bw.Write(data.Length);
        bw.Write(data);

        _connection.SendOneWayAsync(InteropMethodId.PacketIn, ms.ToArray()).GetAwaiter().GetResult();
    }

    // ---- 2. Control RPC --------------------------------------------------
    public int Ping(int timestamp, int latency)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(timestamp);
        bw.Write(latency);

        var response = _connection.SendRequestAsync(InteropMethodId.ControlPing, ms.ToArray()).GetAwaiter().GetResult();
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
        var response = _connection.SendRequestAsync(InteropMethodId.ControlGetServerInfo, Array.Empty<byte>()).GetAwaiter().GetResult();
        using var ms = new MemoryStream(response);
        using var br = new BinaryReader(ms);
        return InteropSerializer.ReadServerInfo(br);
    }

    public async Task InstanceCreateAsync(uint Map)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(Map);

        await _connection.SendRequestAsync(InteropMethodId.ControlInstanceCreate, ms.ToArray());
    }

    public void InstanceDestroy(uint Map)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(Map);

        _connection.SendOneWayAsync(InteropMethodId.ControlInstanceDestroy, ms.ToArray()).GetAwaiter().GetResult();
    }

    public bool InstanceCanCreate(int Type)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(Type);

        var response = _connection.SendRequestAsync(InteropMethodId.ControlInstanceCanCreate, ms.ToArray()).GetAwaiter().GetResult();
        return response.Length >= 1 && response[0] != 0;
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

        var response = _connection.SendRequestAsync(InteropMethodId.ControlClientCreateCharacter, ms.ToArray()).GetAwaiter().GetResult();
        if (response.Length >= 4)
        {
            using var rms = new MemoryStream(response);
            using var br = new BinaryReader(rms);
            return br.ReadInt32();
        }
        return 0;
    }

    public void ClientSetGroup(uint ID, long GroupID)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(ID);
        bw.Write(GroupID);

        _connection.SendOneWayAsync(InteropMethodId.ControlClientSetGroup, ms.ToArray()).GetAwaiter().GetResult();
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

        _connection.SendOneWayAsync(InteropMethodId.ControlGroupUpdate, ms.ToArray()).GetAwaiter().GetResult();
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

        _connection.SendOneWayAsync(InteropMethodId.ControlGroupUpdateLoot, ms.ToArray()).GetAwaiter().GetResult();
    }

    public byte[] GroupMemberStats(ulong GUID, int Flag)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(GUID);
        bw.Write(Flag);

        var response = _connection.SendRequestAsync(InteropMethodId.ControlGroupMemberStats, ms.ToArray()).GetAwaiter().GetResult();
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

        _connection.SendOneWayAsync(InteropMethodId.ControlGuildUpdate, ms.ToArray()).GetAwaiter().GetResult();
    }

    public void BattlefieldCreate(int BattlefieldID, byte BattlefieldMapType, uint Map)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(BattlefieldID);
        bw.Write(BattlefieldMapType);
        bw.Write(Map);

        _connection.SendOneWayAsync(InteropMethodId.ControlBattlefieldCreate, ms.ToArray()).GetAwaiter().GetResult();
    }

    public void BattlefieldDelete(int BattlefieldID)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(BattlefieldID);

        _connection.SendOneWayAsync(InteropMethodId.ControlBattlefieldDelete, ms.ToArray()).GetAwaiter().GetResult();
    }

    public void BattlefieldJoin(int BattlefieldID, ulong GUID)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(BattlefieldID);
        bw.Write(GUID);

        _connection.SendOneWayAsync(InteropMethodId.ControlBattlefieldJoin, ms.ToArray()).GetAwaiter().GetResult();
    }

    public void BattlefieldLeave(int BattlefieldID, ulong GUID)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(BattlefieldID);
        bw.Write(GUID);

        _connection.SendOneWayAsync(InteropMethodId.ControlBattlefieldLeave, ms.ToArray()).GetAwaiter().GetResult();
    }
}
