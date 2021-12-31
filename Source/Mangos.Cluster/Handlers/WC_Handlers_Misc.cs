//
// Copyright (C) 2013-2022 getMaNGOS <https://getmangos.eu>
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

using Mangos.Cluster.Globals;
using Mangos.Cluster.Network;
using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using System.Collections.Generic;

namespace Mangos.Cluster.Handlers;

public class WcHandlersMisc
{
    private readonly ClusterServiceLocator _clusterServiceLocator;

    public WcHandlersMisc(ClusterServiceLocator clusterServiceLocator)
    {
        _clusterServiceLocator = clusterServiceLocator;
    }

    public void On_CMSG_QUERY_TIME(PacketClass packet, ClientClass client)
    {
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_QUERY_TIME", client.IP, client.Port);
        PacketClass response = new(Opcodes.SMSG_QUERY_TIME_RESPONSE);
        response.AddInt32(_clusterServiceLocator.NativeMethods.timeGetTime("")); // GetTimestamp(Now))
        client.Send(response);
        response.Dispose();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_QUERY_TIME_RESPONSE", client.IP, client.Port);
    }

    public void On_CMSG_NEXT_CINEMATIC_CAMERA(PacketClass packet, ClientClass client)
    {
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_NEXT_CINEMATIC_CAMERA", client.IP, client.Port);
    }

    public void On_CMSG_COMPLETE_CINEMATIC(PacketClass packet, ClientClass client)
    {
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_COMPLETE_CINEMATIC", client.IP, client.Port);
    }

    public void On_CMSG_PLAYED_TIME(PacketClass packet, ClientClass client)
    {
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_NAME_QUERY", client.IP, client.Port);
        PacketClass response = new(Opcodes.SMSG_PLAYED_TIME);
        response.AddInt32(1);
        response.AddInt32(1);
        client.Send(response);
        response.Dispose();
    }

    public void On_CMSG_NAME_QUERY(PacketClass packet, ClientClass client)
    {
        if (packet.Data.Length - 1 < 13)
        {
            return;
        }

        packet.GetInt16();
        var guid = packet.GetUInt64();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_NAME_QUERY [GUID={2:X}]", client.IP, client.Port, guid);
        if (_clusterServiceLocator.CommonGlobalFunctions.GuidIsPlayer(guid) && _clusterServiceLocator.WorldCluster.CharacteRs.ContainsKey(guid))
        {
            PacketClass smsgNameQueryResponse = new(Opcodes.SMSG_NAME_QUERY_RESPONSE);
            smsgNameQueryResponse.AddUInt64(guid);
            smsgNameQueryResponse.AddString(_clusterServiceLocator.WorldCluster.CharacteRs[guid].Name);
            smsgNameQueryResponse.AddInt32((byte)_clusterServiceLocator.WorldCluster.CharacteRs[guid].Race);
            smsgNameQueryResponse.AddInt32(_clusterServiceLocator.WorldCluster.CharacteRs[guid].Gender);
            smsgNameQueryResponse.AddInt32((byte)_clusterServiceLocator.WorldCluster.CharacteRs[guid].Classe);
            smsgNameQueryResponse.AddInt8(0);
            client.Send(smsgNameQueryResponse);
            smsgNameQueryResponse.Dispose();
        }
        else
        {
            // DONE: Send it to the world server if it wasn't found in the cluster
            try
            {
                client.Character.GetWorld.ClientPacket(client.Index, packet.Data);
            }
            catch
            {
                _clusterServiceLocator.WcNetwork.WorldServer.Disconnect("NULL", new List<uint> { client.Character.Map });
            }
        }
    }

    public void On_CMSG_INSPECT(PacketClass packet, ClientClass client)
    {
        packet.GetInt16();
        var guid = packet.GetUInt64();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_INSPECT [GUID={2:X}]", client.IP, client.Port, guid);
    }

    public void On_CMSG_CANCEL_TRADE(PacketClass packet, ClientClass client)
    {
        if (client.Character is not null && client.Character.IsInWorld)
        {
            try
            {
                client.Character.GetWorld.ClientPacket(client.Index, packet.Data);
            }
            catch
            {
                _clusterServiceLocator.WcNetwork.WorldServer.Disconnect("NULL", new List<uint> { client.Character.Map });
            }
        }
        else
        {
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CANCEL_TRADE", client.IP, client.Port);
        }
    }

    public void On_CMSG_LOGOUT_CANCEL(PacketClass packet, ClientClass client)
    {
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_LOGOUT_CANCEL", client.IP, client.Port);
    }

    // Public Sub On_CMSG_MOVE_TIME_SKIPPED(Bypacket As PacketClass, Byclient As ClientClass)
    // packet.GetUInt64()
    // packet.GetUInt32()
    // Dim WC_MsTime As Integer = msTime()
    // Dim ClientTimeDelay As Integer = MsTime - msTime()
    // Dim MoveTime As Integer = (msTime() - (msTime() - ClientTimeDelay)) + 50 + msTime()
    // packet.AddInt32(MoveTime, 10)
    // _WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MOVE_TIME_SKIPPED", client.IP, client.Port)
    // End Sub
}
