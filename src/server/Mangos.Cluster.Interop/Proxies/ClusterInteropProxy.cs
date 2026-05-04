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
/// World-side stub of ICluster: each method serializes its arguments into
/// the appropriate envelope and ships it down the IPC connection.
///
/// Layout matches <see cref="InteropMethodId"/>: relay (PacketOut),
/// directives (drop/transfer/update/chat-flag/broadcast/group), control RPC
/// (world hello/goodbye, crypt-key, battlefield list/finish).
/// </summary>
public sealed class ClusterInteropProxy : ICluster
{
    private readonly InteropConnection _connection;

    public ClusterInteropProxy(InteropConnection connection)
    {
        _connection = connection;
    }

    // ---- 1. Relay --------------------------------------------------------
    public void ClientSend(uint id, byte[] data)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(id);
        bw.Write(data.Length);
        bw.Write(data);

        _connection.SendOneWayAsync(InteropMethodId.PacketOut, ms.ToArray()).GetAwaiter().GetResult();
    }

    // ---- 2. Directives ---------------------------------------------------
    public void ClientDrop(uint id)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(id);

        _connection.SendOneWayAsync(InteropMethodId.DirectiveDropClient, ms.ToArray()).GetAwaiter().GetResult();
    }

    public void ClientTransfer(uint id, float posX, float posY, float posZ, float ori, uint map)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(id);
        bw.Write(posX);
        bw.Write(posY);
        bw.Write(posZ);
        bw.Write(ori);
        bw.Write(map);

        _connection.SendOneWayAsync(InteropMethodId.DirectiveTransferClient, ms.ToArray()).GetAwaiter().GetResult();
    }

    public void ClientUpdate(uint id, uint zone, byte level)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(id);
        bw.Write(zone);
        bw.Write(level);

        _connection.SendOneWayAsync(InteropMethodId.DirectiveUpdateClient, ms.ToArray()).GetAwaiter().GetResult();
    }

    public void ClientSetChatFlag(uint id, byte flag)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(id);
        bw.Write(flag);

        _connection.SendOneWayAsync(InteropMethodId.DirectiveSetClientChatFlag, ms.ToArray()).GetAwaiter().GetResult();
    }

    public void Broadcast(byte[] data)
    {
        _connection.SendOneWayAsync(InteropMethodId.DirectiveBroadcast, InteropSerializer.WriteByteArray(data)).GetAwaiter().GetResult();
    }

    public void BroadcastGroup(long groupId, byte[] data)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(groupId);
        bw.Write(data.Length);
        bw.Write(data);

        _connection.SendOneWayAsync(InteropMethodId.DirectiveBroadcastGroup, ms.ToArray()).GetAwaiter().GetResult();
    }

    public void BroadcastRaid(long groupId, byte[] data)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(groupId);
        bw.Write(data.Length);
        bw.Write(data);

        _connection.SendOneWayAsync(InteropMethodId.DirectiveBroadcastRaid, ms.ToArray()).GetAwaiter().GetResult();
    }

    public void BroadcastGuild(long guildId, byte[] data)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(guildId);
        bw.Write(data.Length);
        bw.Write(data);

        _connection.SendOneWayAsync(InteropMethodId.DirectiveBroadcastGuild, ms.ToArray()).GetAwaiter().GetResult();
    }

    public void BroadcastGuildOfficers(long guildId, byte[] data)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(guildId);
        bw.Write(data.Length);
        bw.Write(data);

        _connection.SendOneWayAsync(InteropMethodId.DirectiveBroadcastGuildOfficers, ms.ToArray()).GetAwaiter().GetResult();
    }

    public void GroupRequestUpdate(uint id)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(id);

        _connection.SendOneWayAsync(InteropMethodId.DirectiveGroupRequestUpdate, ms.ToArray()).GetAwaiter().GetResult();
    }

    // ---- 3. Control RPC --------------------------------------------------
    public bool Connect(string uri, List<uint> maps, IWorld world)
    {
        // The IWorld parameter is implicit on the wire: the cluster builds
        // its own IWorld stub from this connection in the dispatcher.
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(uri);
        bw.Write(maps.Count);
        foreach (var map in maps)
        {
            bw.Write(map);
        }

        var response = _connection.SendRequestAsync(InteropMethodId.ControlWorldHello, ms.ToArray()).GetAwaiter().GetResult();
        return response.Length >= 1 && response[0] != 0;
    }

    public void Disconnect(string uri, List<uint> maps)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(uri);
        bw.Write(maps.Count);
        foreach (var map in maps)
        {
            bw.Write(map);
        }

        _connection.SendOneWayAsync(InteropMethodId.ControlWorldGoodbye, ms.ToArray()).GetAwaiter().GetResult();
    }

    public byte[] ClientGetCryptKey(uint id)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(id);

        var response = _connection.SendRequestAsync(InteropMethodId.ControlGetCryptKey, ms.ToArray()).GetAwaiter().GetResult();
        using var rms = new MemoryStream(response);
        using var br = new BinaryReader(rms);
        return InteropSerializer.ReadByteArray(br);
    }

    public List<int> BattlefieldList(byte type)
    {
        var response = _connection.SendRequestAsync(InteropMethodId.ControlBattlefieldList, new[] { type }).GetAwaiter().GetResult();
        using var ms = new MemoryStream(response);
        using var br = new BinaryReader(ms);
        return InteropSerializer.ReadInt32List(br);
    }

    public void BattlefieldFinish(int battlefieldId)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(battlefieldId);

        _connection.SendOneWayAsync(InteropMethodId.ControlBattlefieldFinish, ms.ToArray()).GetAwaiter().GetResult();
    }

    public byte[] RunAdminCommand(byte[] commandBytes)
    {
        var response = _connection.SendRequestAsync(
            InteropMethodId.ControlRunAdminCommand,
            InteropSerializer.WriteByteArray(commandBytes)).GetAwaiter().GetResult();
        using var rms = new MemoryStream(response);
        using var br = new BinaryReader(rms);
        return InteropSerializer.ReadByteArray(br);
    }
}
