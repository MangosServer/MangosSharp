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

using Mangos.Common.Enums.Misc;
using Mangos.Common.Legacy;

namespace Mangos.Cluster.Interop.Protocol;

/// <summary>
/// Binary serialization helpers for IPC method parameters and return values.
/// Uses BinaryWriter/BinaryReader for compact, fast serialization.
/// </summary>
public static class InteropSerializer
{
    public static byte[] WriteString(string value)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(value);
        return ms.ToArray();
    }

    public static byte[] WriteClientInfo(ClientInfo client)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(client.Index);
        bw.Write(client.IP ?? string.Empty);
        bw.Write(client.Port);
        bw.Write(client.Account ?? string.Empty);
        bw.Write((byte)client.Access);
        bw.Write((byte)client.Expansion);
        return ms.ToArray();
    }

    public static ClientInfo ReadClientInfo(BinaryReader br)
    {
        return new ClientInfo
        {
            Index = br.ReadUInt32(),
            IP = br.ReadString(),
            Port = br.ReadUInt32(),
            Account = br.ReadString(),
            Access = (AccessLevel)br.ReadByte(),
            Expansion = (ExpansionLevel)br.ReadByte()
        };
    }

    public static byte[] WriteServerInfo(ServerInfo info)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(info.CpuUsage);
        bw.Write(info.MemoryUsage);
        return ms.ToArray();
    }

    public static ServerInfo ReadServerInfo(BinaryReader br)
    {
        return new ServerInfo
        {
            CpuUsage = br.ReadSingle(),
            MemoryUsage = br.ReadUInt64()
        };
    }

    public static byte[] WriteUInt32List(List<uint> values)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(values.Count);
        foreach (var v in values)
        {
            bw.Write(v);
        }
        return ms.ToArray();
    }

    public static List<uint> ReadUInt32List(BinaryReader br)
    {
        var count = br.ReadInt32();
        var list = new List<uint>(count);
        for (var i = 0; i < count; i++)
        {
            list.Add(br.ReadUInt32());
        }
        return list;
    }

    public static byte[] WriteInt32List(List<int> values)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(values.Count);
        foreach (var v in values)
        {
            bw.Write(v);
        }
        return ms.ToArray();
    }

    public static List<int> ReadInt32List(BinaryReader br)
    {
        var count = br.ReadInt32();
        var list = new List<int>(count);
        for (var i = 0; i < count; i++)
        {
            list.Add(br.ReadInt32());
        }
        return list;
    }

    public static byte[] WriteUInt64Array(ulong[] values)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(values.Length);
        foreach (var v in values)
        {
            bw.Write(v);
        }
        return ms.ToArray();
    }

    public static ulong[] ReadUInt64Array(BinaryReader br)
    {
        var count = br.ReadInt32();
        var arr = new ulong[count];
        for (var i = 0; i < count; i++)
        {
            arr[i] = br.ReadUInt64();
        }
        return arr;
    }

    public static byte[] WriteByteArray(byte[] data)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(data.Length);
        bw.Write(data);
        return ms.ToArray();
    }

    public static byte[] ReadByteArray(BinaryReader br)
    {
        var length = br.ReadInt32();
        return br.ReadBytes(length);
    }
}
