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

public class WcHandlersMovement
{
    private readonly ClusterServiceLocator _clusterServiceLocator;

    public WcHandlersMovement(ClusterServiceLocator clusterServiceLocator)
    {
        _clusterServiceLocator = clusterServiceLocator;
    }

    public void On_MSG_MOVE_HEARTBEAT(PacketClass packet, ClientClass client)
    {
        try
        {
            client.Character.GetWorld.ClientPacket(client.Index, packet.Data);
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_MOVE_HEARTBEAT [{2}]", client.IP, client.Port, client.Character.Map);
        }
        catch
        {
            _clusterServiceLocator.WcNetwork.WorldServer.Disconnect("NULL", new List<uint> { client.Character.Map });
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.WARNING, "[{0}:{1}] MSG_MOVE_HEARTBEAT error occured [{2}]", client.IP, client.Port, client.Character.Map);
            return;
        }

        // DONE: Save location on cluster
        client.Character.PositionX = packet.GetFloat(); // (15)
        client.Character.PositionY = packet.GetFloat();
        client.Character.PositionZ = packet.GetFloat();

        // DONE: Sync your location to other party / raid members
        if (client.Character.IsInGroup)
        {
            PacketClass statsPacket = new(Opcodes.UMSG_UPDATE_GROUP_MEMBERS) { Data = client.Character.GetWorld.GroupMemberStats(client.Character.Guid, (int)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_POSITION + (int)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_ZONE) };
            client.Character.Group.BroadcastToOutOfRange(statsPacket, client.Character);
            statsPacket.Dispose();
        }
    }

    public void On_MSG_START_BACKWARD(PacketClass packet, ClientClass client)
    {
        try
        {
            client.Character.GetWorld.ClientPacket(client.Index, packet.Data);
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_START_BACKWARD [{2}]", client.IP, client.Port, client.Character.Map);
        }
        catch
        {
            // _WC_Network.WorldServer.Disconnect("NULL", New List(Of UInteger)() From {client.Character.Map}) 'There's an error coming from here, uncomment for full runtime error log details!
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.WARNING, "[{0}:{1}] MSG_START_BACKWARD error occured [{2}]", client.IP, client.Port, client.Character.Map);
            return;
        }

        // DONE: Save location on cluster
        client.Character.PositionX = packet.GetFloat(); // (15)
        client.Character.PositionY = packet.GetFloat();
        client.Character.PositionZ = packet.GetFloat();

        // DONE: Sync your location to other party / raid members
        if (client.Character.IsInGroup)
        {
            PacketClass statsPacket = new(Opcodes.UMSG_UPDATE_GROUP_MEMBERS) { Data = client.Character.GetWorld.GroupMemberStats(client.Character.Guid, (int)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_POSITION + (int)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_ZONE) };
            client.Character.Group.BroadcastToOutOfRange(statsPacket, client.Character);
            statsPacket.Dispose();
        }
    }

    public void On_MSG_MOVE_START_FORWARD(PacketClass packet, ClientClass client)
    {
        try
        {
            client.Character.GetWorld.ClientPacket(client.Index, packet.Data);
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_MOVE_START_FOWARD [{2}]", client.IP, client.Port, client.Character.Map);
        }
        catch
        {
            _clusterServiceLocator.WcNetwork.WorldServer.Disconnect("NULL", new List<uint> { client.Character.Map });
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.WARNING, "[{0}:{1}] MSG_MOVE_START_FOWARD error occured [{2}]", client.IP, client.Port, client.Character.Map);
            return;
        }

        // DONE: Save location on cluster
        client.Character.PositionX = packet.GetFloat(); // (15)
        client.Character.PositionY = packet.GetFloat();
        client.Character.PositionZ = packet.GetFloat();

        // DONE: Sync your location to other party / raid members
        if (client.Character.IsInGroup)
        {
            PacketClass statsPacket = new(Opcodes.UMSG_UPDATE_GROUP_MEMBERS) { Data = client.Character.GetWorld.GroupMemberStats(client.Character.Guid, (int)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_POSITION + (int)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_ZONE) };
            client.Character.Group.BroadcastToOutOfRange(statsPacket, client.Character);
            statsPacket.Dispose();
        }
    }

    public void On_MSG_MOVE_START_PITCH_DOWN(PacketClass packet, ClientClass client)
    {
        try
        {
            client.Character.GetWorld.ClientPacket(client.Index, packet.Data);
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_MOVE_START_PITCH_DOWN [{2}]", client.IP, client.Port, client.Character.Map);
        }
        catch
        {
            _clusterServiceLocator.WcNetwork.WorldServer.Disconnect("NULL", new List<uint> { client.Character.Map });
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.WARNING, "[{0}:{1}] MSG_MOVE_START_PITCH_DOWN error occured [{2}]", client.IP, client.Port, client.Character.Map);
            return;
        }

        // DONE: Save location on cluster
        client.Character.PositionX = packet.GetFloat(); // (15)
        client.Character.PositionY = packet.GetFloat();
        client.Character.PositionZ = packet.GetFloat();

        // DONE: Sync your location to other party / raid members
        if (client.Character.IsInGroup)
        {
            PacketClass statsPacket = new(Opcodes.UMSG_UPDATE_GROUP_MEMBERS) { Data = client.Character.GetWorld.GroupMemberStats(client.Character.Guid, (int)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_POSITION + (int)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_ZONE) };
            client.Character.Group.BroadcastToOutOfRange(statsPacket, client.Character);
            statsPacket.Dispose();
        }
    }

    public void On_MSG_MOVE_START_PITCH_UP(PacketClass packet, ClientClass client)
    {
        try
        {
            client.Character.GetWorld.ClientPacket(client.Index, packet.Data);
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_MOVE_START_PITCH_UP [{2}]", client.IP, client.Port, client.Character.Map);
        }
        catch
        {
            _clusterServiceLocator.WcNetwork.WorldServer.Disconnect("NULL", new List<uint> { client.Character.Map });
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.WARNING, "[{0}:{1}] MSG_MOVE_START_PITCH_UP error occured [{2}]", client.IP, client.Port, client.Character.Map);
            return;
        }

        // DONE: Save location on cluster
        client.Character.PositionX = packet.GetFloat(); // (15)
        client.Character.PositionY = packet.GetFloat();
        client.Character.PositionZ = packet.GetFloat();

        // DONE: Sync your location to other party / raid members
        if (client.Character.IsInGroup)
        {
            PacketClass statsPacket = new(Opcodes.UMSG_UPDATE_GROUP_MEMBERS) { Data = client.Character.GetWorld.GroupMemberStats(client.Character.Guid, (int)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_POSITION + (int)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_ZONE) };
            client.Character.Group.BroadcastToOutOfRange(statsPacket, client.Character);
            statsPacket.Dispose();
        }
    }

    public void On_MSG_MOVE_STRAFE_LEFT(PacketClass packet, ClientClass client)
    {
        try
        {
            client.Character.GetWorld.ClientPacket(client.Index, packet.Data);
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_MOVE_STRAFE_LEFT [{2}]", client.IP, client.Port, client.Character.Map);
        }
        catch
        {
            _clusterServiceLocator.WcNetwork.WorldServer.Disconnect("NULL", new List<uint> { client.Character.Map });
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.WARNING, "[{0}:{1}] MSG_MOVE_STRAFE_LEFT error occured [{2}]", client.IP, client.Port, client.Character.Map);
            return;
        }

        // DONE: Save location on cluster
        client.Character.PositionX = packet.GetFloat(); // (15)
        client.Character.PositionY = packet.GetFloat();
        client.Character.PositionZ = packet.GetFloat();

        // DONE: Sync your location to other party / raid members
        if (client.Character.IsInGroup)
        {
            PacketClass statsPacket = new(Opcodes.UMSG_UPDATE_GROUP_MEMBERS) { Data = client.Character.GetWorld.GroupMemberStats(client.Character.Guid, (int)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_POSITION + (int)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_ZONE) };
            client.Character.Group.BroadcastToOutOfRange(statsPacket, client.Character);
            statsPacket.Dispose();
        }
    }

    public void On_MSG_MOVE_START_STRAFE_RIGHT(PacketClass packet, ClientClass client)
    {
        try
        {
            client.Character.GetWorld.ClientPacket(client.Index, packet.Data);
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_MOVE_START_STRAFE_RIGHT [{2}]", client.IP, client.Port, client.Character.Map);
        }
        catch
        {
            _clusterServiceLocator.WcNetwork.WorldServer.Disconnect("NULL", new List<uint> { client.Character.Map });
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.WARNING, "[{0}:{1}] MSG_MOVE_START_STRAFE_RIGHT error occured [{2}]", client.IP, client.Port, client.Character.Map);
            return;
        }

        // DONE: Save location on cluster
        client.Character.PositionX = packet.GetFloat(); // (15)
        client.Character.PositionY = packet.GetFloat();
        client.Character.PositionZ = packet.GetFloat();

        // DONE: Sync your location to other party / raid members
        if (client.Character.IsInGroup)
        {
            PacketClass statsPacket = new(Opcodes.UMSG_UPDATE_GROUP_MEMBERS) { Data = client.Character.GetWorld.GroupMemberStats(client.Character.Guid, (int)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_POSITION + (int)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_ZONE) };
            client.Character.Group.BroadcastToOutOfRange(statsPacket, client.Character);
            statsPacket.Dispose();
        }
    }

    public void On_MSG_MOVE_START_SWIM(PacketClass packet, ClientClass client)
    {
        try
        {
            client.Character.GetWorld.ClientPacket(client.Index, packet.Data);
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_MOVE_START_SWIM [{2}]", client.IP, client.Port, client.Character.Map);
        }
        catch
        {
            _clusterServiceLocator.WcNetwork.WorldServer.Disconnect("NULL", new List<uint> { client.Character.Map });
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.WARNING, "[{0}:{1}] MSG_MOVE_START_SWIM error occured [{2}]", client.IP, client.Port, client.Character.Map);
            return;
        }

        // DONE: Save location on cluster
        client.Character.PositionX = packet.GetFloat(); // (15)
        client.Character.PositionY = packet.GetFloat();
        client.Character.PositionZ = packet.GetFloat();

        // DONE: Sync your location to other party / raid members
        if (client.Character.IsInGroup)
        {
            PacketClass statsPacket = new(Opcodes.UMSG_UPDATE_GROUP_MEMBERS) { Data = client.Character.GetWorld.GroupMemberStats(client.Character.Guid, (int)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_POSITION + (int)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_ZONE) };
            client.Character.Group.BroadcastToOutOfRange(statsPacket, client.Character);
            statsPacket.Dispose();
        }
    }

    public void On_MSG_MOVE_START_TURN_LEFT(PacketClass packet, ClientClass client)
    {
        try
        {
            client.Character.GetWorld.ClientPacket(client.Index, packet.Data);
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_MOVE_START_TURN_LEFT [{2}]", client.IP, client.Port, client.Character.Map);
        }
        catch
        {
            _clusterServiceLocator.WcNetwork.WorldServer.Disconnect("NULL", new List<uint> { client.Character.Map });
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.WARNING, "[{0}:{1}] MSG_MOVE_START_TURN_LEFT error occured [{2}]", client.IP, client.Port, client.Character.Map);
            return;
        }

        // DONE: Save location on cluster
        client.Character.PositionX = packet.GetFloat(); // (15)
        client.Character.PositionY = packet.GetFloat();
        client.Character.PositionZ = packet.GetFloat();
        client.Character.PositionO = packet.GetFloat();

        // DONE: Sync your location to other party / raid members
        if (client.Character.IsInGroup)
        {
            PacketClass statsPacket = new(Opcodes.UMSG_UPDATE_GROUP_MEMBERS) { Data = client.Character.GetWorld.GroupMemberStats(client.Character.Guid, (int)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_POSITION + (int)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_ZONE) };
            client.Character.Group.BroadcastToOutOfRange(statsPacket, client.Character);
            statsPacket.Dispose();
        }
    }

    public void On_MSG_MOVE_START_TURN_RIGHT(PacketClass packet, ClientClass client)
    {
        try
        {
            client.Character.GetWorld.ClientPacket(client.Index, packet.Data);
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_MOVE_START_TURN_RIGHT [{2}]", client.IP, client.Port, client.Character.Map);
        }
        catch
        {
            _clusterServiceLocator.WcNetwork.WorldServer.Disconnect("NULL", new List<uint> { client.Character.Map });
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.WARNING, "[{0}:{1}] MSG_MOVE_START_TURN_RIGHT error occured [{2}]", client.IP, client.Port, client.Character.Map);
            return;
        }

        // DONE: Save location on cluster
        client.Character.PositionX = packet.GetFloat(); // (15)
        client.Character.PositionY = packet.GetFloat();
        client.Character.PositionZ = packet.GetFloat();
        client.Character.PositionO = packet.GetFloat();

        // DONE: Sync your location to other party / raid members
        if (client.Character.IsInGroup)
        {
            PacketClass statsPacket = new(Opcodes.UMSG_UPDATE_GROUP_MEMBERS) { Data = client.Character.GetWorld.GroupMemberStats(client.Character.Guid, (int)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_POSITION + (int)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_ZONE) };
            client.Character.Group.BroadcastToOutOfRange(statsPacket, client.Character);
            statsPacket.Dispose();
        }
    }

    public void On_MSG_MOVE_STOP(PacketClass packet, ClientClass client)
    {
        try
        {
            client.Character.GetWorld.ClientPacket(client.Index, packet.Data);
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_MOVE_STOP [{2}]", client.IP, client.Port, client.Character.Map);
        }
        catch
        {
            _clusterServiceLocator.WcNetwork.WorldServer.Disconnect("NULL", new List<uint> { client.Character.Map });
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.WARNING, "[{0}:{1}] MSG_MOVE_STOP error occured [{2}]", client.IP, client.Port, client.Character.Map);
            return;
        }

        // DONE: Save location on cluster
        client.Character.PositionX = packet.GetFloat(); // (15)
        client.Character.PositionY = packet.GetFloat();
        client.Character.PositionZ = packet.GetFloat();

        // DONE: Sync your location to other party / raid members
        if (client.Character.IsInGroup)
        {
            PacketClass statsPacket = new(Opcodes.UMSG_UPDATE_GROUP_MEMBERS) { Data = client.Character.GetWorld.GroupMemberStats(client.Character.Guid, (int)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_POSITION + (int)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_ZONE) };
            client.Character.Group.BroadcastToOutOfRange(statsPacket, client.Character);
            statsPacket.Dispose();
        }
    }

    public void On_MSG_MOVE_STOP_PITCH(PacketClass packet, ClientClass client)
    {
        try
        {
            client.Character.GetWorld.ClientPacket(client.Index, packet.Data);
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_MOVE_STOP_PITCH [{2}]", client.IP, client.Port, client.Character.Map);
        }
        catch
        {
            _clusterServiceLocator.WcNetwork.WorldServer.Disconnect("NULL", new List<uint> { client.Character.Map });
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.WARNING, "[{0}:{1}] MSG_MOVE_STOP_PITCH error occured [{2}]", client.IP, client.Port, client.Character.Map);
            return;
        }

        // DONE: Save location on cluster
        client.Character.PositionX = packet.GetFloat(); // (15)
        client.Character.PositionY = packet.GetFloat();
        client.Character.PositionZ = packet.GetFloat();

        // DONE: Sync your location to other party / raid members
        if (client.Character.IsInGroup)
        {
            PacketClass statsPacket = new(Opcodes.UMSG_UPDATE_GROUP_MEMBERS) { Data = client.Character.GetWorld.GroupMemberStats(client.Character.Guid, (int)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_POSITION + (int)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_ZONE) };
            client.Character.Group.BroadcastToOutOfRange(statsPacket, client.Character);
            statsPacket.Dispose();
        }
    }

    public void On_MSG_MOVE_STOP_STRAFE(PacketClass packet, ClientClass client)
    {
        try
        {
            client.Character.GetWorld.ClientPacket(client.Index, packet.Data);
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_MOVE_STOP_STRAFE [{2}]", client.IP, client.Port, client.Character.Map);
        }
        catch
        {
            _clusterServiceLocator.WcNetwork.WorldServer.Disconnect("NULL", new List<uint> { client.Character.Map });
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.WARNING, "[{0}:{1}] MSG_MOVE_STOP_STRAFE error occured [{2}]", client.IP, client.Port, client.Character.Map);
            return;
        }

        // DONE: Save location on cluster
        client.Character.PositionX = packet.GetFloat(); // (15)
        client.Character.PositionY = packet.GetFloat();
        client.Character.PositionZ = packet.GetFloat();

        // DONE: Sync your location to other party / raid members
        if (client.Character.IsInGroup)
        {
            PacketClass statsPacket = new(Opcodes.UMSG_UPDATE_GROUP_MEMBERS) { Data = client.Character.GetWorld.GroupMemberStats(client.Character.Guid, (int)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_POSITION + (int)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_ZONE) };
            client.Character.Group.BroadcastToOutOfRange(statsPacket, client.Character);
            statsPacket.Dispose();
        }
    }

    public void On_MSG_MOVE_STOP_SWIM(PacketClass packet, ClientClass client)
    {
        try
        {
            client.Character.GetWorld.ClientPacket(client.Index, packet.Data);
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_MOVE_STOP_SWIM [{2}]", client.IP, client.Port, client.Character.Map);
        }
        catch
        {
            _clusterServiceLocator.WcNetwork.WorldServer.Disconnect("NULL", new List<uint> { client.Character.Map });
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.WARNING, "[{0}:{1}] MSG_MOVE_STOP_SWIM error occured [{2}", client.IP, client.Port, client.Character.Map);
            return;
        }

        // DONE: Save location on cluster
        client.Character.PositionX = packet.GetFloat(); // (15)
        client.Character.PositionY = packet.GetFloat();
        client.Character.PositionZ = packet.GetFloat();

        // DONE: Sync your location to other party / raid members
        if (client.Character.IsInGroup)
        {
            PacketClass statsPacket = new(Opcodes.UMSG_UPDATE_GROUP_MEMBERS) { Data = client.Character.GetWorld.GroupMemberStats(client.Character.Guid, (int)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_POSITION + (int)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_ZONE) };
            client.Character.Group.BroadcastToOutOfRange(statsPacket, client.Character);
            statsPacket.Dispose();
        }
    }

    public void On_MSG_MOVE_STOP_TURN(PacketClass packet, ClientClass client)
    {
        try
        {
            client.Character.GetWorld.ClientPacket(client.Index, packet.Data);
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_MOVE_STOP_TURN [{2}", client.IP, client.Port, client.Character.Map);
        }
        catch
        {
            _clusterServiceLocator.WcNetwork.WorldServer.Disconnect("NULL", new List<uint> { client.Character.Map });
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.WARNING, "[{0}:{1}] MSG_MOVE_STOP_TURN error occured [{2}", client.IP, client.Port, client.Character.Map);
            return;
        }

        // DONE: Save location on cluster
        client.Character.PositionX = packet.GetFloat(); // (15)
        client.Character.PositionY = packet.GetFloat();
        client.Character.PositionZ = packet.GetFloat();
        client.Character.PositionO = packet.GetFloat();

        // DONE: Sync your location to other party / raid members
        if (client.Character.IsInGroup)
        {
            PacketClass statsPacket = new(Opcodes.UMSG_UPDATE_GROUP_MEMBERS) { Data = client.Character.GetWorld.GroupMemberStats(client.Character.Guid, (int)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_POSITION + (int)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_ZONE) };
            client.Character.Group.BroadcastToOutOfRange(statsPacket, client.Character);
            statsPacket.Dispose();
        }
    }

    public void On_MSG_MOVE_SET_FACING(PacketClass packet, ClientClass client)
    {
        try
        {
            client.Character.GetWorld.ClientPacket(client.Index, packet.Data);
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_MOVE_SET_FACING [{2}", client.IP, client.Port, client.Character.Map);
        }
        catch
        {
            _clusterServiceLocator.WcNetwork.WorldServer.Disconnect("NULL", new List<uint> { client.Character.Map });
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.WARNING, "[{0}:{1}] MSG_MOVE_SET_FACING error occured [{2}", client.IP, client.Port, client.Character.Map);
            return;
        }

        // DONE: Save location on cluster
        client.Character.PositionX = packet.GetFloat(); // (15)
        client.Character.PositionY = packet.GetFloat();
        client.Character.PositionZ = packet.GetFloat();
        client.Character.PositionO = packet.GetFloat();

        // DONE: Sync your location to other party / raid members
        if (client.Character.IsInGroup)
        {
            PacketClass statsPacket = new(Opcodes.UMSG_UPDATE_GROUP_MEMBERS) { Data = client.Character.GetWorld.GroupMemberStats(client.Character.Guid, (int)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_POSITION + (int)Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_ZONE) };
            client.Character.Group.BroadcastToOutOfRange(statsPacket, client.Character);
            statsPacket.Dispose();
        }
    }
}
