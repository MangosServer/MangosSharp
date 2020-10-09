// 
// Copyright (C) 2013-2020 getMaNGOS <https://getmangos.eu>
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

using System.Collections.Generic;
using Mangos.Cluster.Globals;
using Mangos.Cluster.Server;
using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;

namespace Mangos.Cluster.Handlers
{
    public class WC_Handlers_Misc
    {
        public void On_CMSG_QUERY_TIME(ref Packets.PacketClass packet, ref WC_Network.ClientClass client)
        {
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_QUERY_TIME", client.IP, client.Port);
            var response = new Packets.PacketClass(OPCODES.SMSG_QUERY_TIME_RESPONSE);
            response.AddInt32(ClusterServiceLocator._NativeMethods.timeGetTime("")); // GetTimestamp(Now))
            client.Send(ref response);
            response.Dispose();
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_QUERY_TIME_RESPONSE", client.IP, client.Port);
        }

        public void On_CMSG_NEXT_CINEMATIC_CAMERA(ref Packets.PacketClass packet, ref WC_Network.ClientClass client)
        {
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_NEXT_CINEMATIC_CAMERA", client.IP, client.Port);
        }

        public void On_CMSG_COMPLETE_CINEMATIC(ref Packets.PacketClass packet, ref WC_Network.ClientClass client)
        {
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_COMPLETE_CINEMATIC", client.IP, client.Port);
        }

        public void On_CMSG_PLAYED_TIME(ref Packets.PacketClass packet, ref WC_Network.ClientClass client)
        {
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_NAME_QUERY", client.IP, client.Port);
            var response = new Packets.PacketClass(OPCODES.SMSG_PLAYED_TIME);
            response.AddInt32(1);
            response.AddInt32(1);
            client.Send(ref response);
            response.Dispose();
        }

        public void On_CMSG_NAME_QUERY(ref Packets.PacketClass packet, ref WC_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 13)
                return;
            packet.GetInt16();
            ulong GUID = packet.GetUInt64();
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_NAME_QUERY [GUID={2:X}]", client.IP, client.Port, GUID);
            if (ClusterServiceLocator._CommonGlobalFunctions.GuidIsPlayer(GUID) && ClusterServiceLocator._WorldCluster.CHARACTERs.ContainsKey(GUID))
            {
                var SMSG_NAME_QUERY_RESPONSE = new Packets.PacketClass(OPCODES.SMSG_NAME_QUERY_RESPONSE);
                SMSG_NAME_QUERY_RESPONSE.AddUInt64(GUID);
                SMSG_NAME_QUERY_RESPONSE.AddString(ClusterServiceLocator._WorldCluster.CHARACTERs[GUID].Name);
                SMSG_NAME_QUERY_RESPONSE.AddInt32(ClusterServiceLocator._WorldCluster.CHARACTERs[GUID].Race);
                SMSG_NAME_QUERY_RESPONSE.AddInt32(ClusterServiceLocator._WorldCluster.CHARACTERs[GUID].Gender);
                SMSG_NAME_QUERY_RESPONSE.AddInt32(ClusterServiceLocator._WorldCluster.CHARACTERs[GUID].Classe);
                SMSG_NAME_QUERY_RESPONSE.AddInt8(0);
                client.Send(ref SMSG_NAME_QUERY_RESPONSE);
                SMSG_NAME_QUERY_RESPONSE.Dispose();
                return;
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
                    ClusterServiceLocator._WC_Network.WorldServer.Disconnect("NULL", new List<uint>() { client.Character.Map });
                }
            }
        }

        public void On_CMSG_INSPECT(ref Packets.PacketClass packet, ref WC_Network.ClientClass client)
        {
            packet.GetInt16();
            ulong GUID = packet.GetUInt64();
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_INSPECT [GUID={2:X}]", client.IP, client.Port, GUID);
        }

        public void On_CMSG_CANCEL_TRADE(ref Packets.PacketClass packet, ref WC_Network.ClientClass client)
        {
            if (client.Character is object && client.Character.IsInWorld)
            {
                try
                {
                    client.Character.GetWorld.ClientPacket(client.Index, packet.Data);
                }
                catch
                {
                    ClusterServiceLocator._WC_Network.WorldServer.Disconnect("NULL", new List<uint>() { client.Character.Map });
                }
            }
            else
            {
                ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CANCEL_TRADE", client.IP, client.Port);
            }
        }

        public void On_CMSG_LOGOUT_CANCEL(ref Packets.PacketClass packet, ref WC_Network.ClientClass client)
        {
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_LOGOUT_CANCEL", client.IP, client.Port);
        }

        // Public Sub On_CMSG_MOVE_TIME_SKIPPED(ByRef packet As PacketClass, ByRef client As ClientClass)
        // packet.GetUInt64()
        // packet.GetUInt32()
        // Dim WC_MsTime As Integer = msTime()
        // Dim ClientTimeDelay As Integer = MsTime - msTime()
        // Dim MoveTime As Integer = (msTime() - (msTime() - ClientTimeDelay)) + 50 + msTime()
        // packet.AddInt32(MoveTime, 10)
        // _WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MOVE_TIME_SKIPPED", client.IP, client.Port)
        // End Sub

    }
}