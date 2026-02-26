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

using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Network;
using System;
using System.Collections.Generic;

namespace Mangos.World.Handlers;

public class WS_Handlers_Battleground
{
    public void On_CMSG_BATTLEMASTER_HELLO(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) < 13)
        {
            return;
        }
        packet.GetInt16();
        var GUID = packet.GetUInt64();
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_BATTLEMASTER_HELLO [{2:X}]", client.IP, client.Port, GUID);
        if (!WorldServiceLocator.WorldServer.WORLD_CREATUREs.ContainsKey(GUID) || (WorldServiceLocator.WorldServer.WORLD_CREATUREs[GUID].CreatureInfo.cNpcFlags & 0x800) == 0 || !WorldServiceLocator.WSDBCDatabase.Battlemasters.ContainsKey(WorldServiceLocator.WorldServer.WORLD_CREATUREs[GUID].ID))
        {
            return;
        }
        var BGType = WorldServiceLocator.WSDBCDatabase.Battlemasters[WorldServiceLocator.WorldServer.WORLD_CREATUREs[GUID].ID];
        if (!WorldServiceLocator.WSDBCDatabase.Battlegrounds.ContainsKey(BGType))
        {
            return;
        }
        if (WorldServiceLocator.WSDBCDatabase.Battlegrounds[BGType].MinLevel > (uint)client.Character.Level || WorldServiceLocator.WSDBCDatabase.Battlegrounds[BGType].MaxLevel < (uint)client.Character.Level)
        {
            WorldServiceLocator.Functions.SendMessageNotification(ref client, "You don't meet Battleground level requirements");
            return;
        }
        Packets.PacketClass response = new(Opcodes.SMSG_BATTLEFIELD_LIST);
        try
        {
            response.AddUInt64(client.Character.GUID);
            response.AddInt32(BGType);
            response.AddInt8(0);
            var Battlegrounds = WorldServiceLocator.WorldServer.ClsWorldServer.Cluster.BattlefieldList(BGType);
            response.AddInt32(Battlegrounds.Count);
            foreach (var Instance in Battlegrounds)
            {
                response.AddInt32(Instance);
            }
            client.Send(ref response);
        }
        finally
        {
            response.Dispose();
        }
    }

    public void On_MSG_BATTLEGROUND_PLAYER_POSITIONS(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        packet.GetUInt32();
    }

    public void On_CMSG_BATTLEMASTER_JOIN(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        try
        {
            if (checked(packet.Data.Length - 1) < 25)
            {
                return;
            }
            packet.GetInt16();
            var battlemasterGuid = packet.GetUInt64();
            var bgTypeId = packet.GetInt32();
            var instanceId = packet.GetInt32();
            var joinAsGroup = packet.GetInt8();
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_BATTLEMASTER_JOIN [GUID={2:X} BG={3} Instance={4} Group={5}]", client.IP, client.Port, battlemasterGuid, bgTypeId, instanceId, joinAsGroup);
            if (client.Character == null || client.Character.DEAD)
            {
                return;
            }
            if (!WorldServiceLocator.WSDBCDatabase.Battlegrounds.ContainsKey(bgTypeId))
            {
                return;
            }
            if (WorldServiceLocator.WSDBCDatabase.Battlegrounds[bgTypeId].MinLevel > (uint)client.Character.Level || WorldServiceLocator.WSDBCDatabase.Battlegrounds[bgTypeId].MaxLevel < (uint)client.Character.Level)
            {
                WorldServiceLocator.Functions.SendMessageNotification(ref client, "You don't meet Battleground level requirements");
                return;
            }
            Packets.PacketClass statusPacket = new(Opcodes.SMSG_BATTLEFIELD_STATUS);
            try
            {
                statusPacket.AddInt32(0);
                statusPacket.AddInt32(bgTypeId);
                statusPacket.AddInt32(0);
                statusPacket.AddInt32(0);
                statusPacket.AddInt32(1);
                client.Send(ref statusPacket);
            }
            finally
            {
                statusPacket.Dispose();
            }
        }
        catch (Exception e)
        {
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.CRITICAL, "Error at battlemaster join.{0}", Environment.NewLine + e);
        }
    }

    public void On_CMSG_BATTLEFIELD_PORT(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        try
        {
            if (checked(packet.Data.Length - 1) < 9)
            {
                return;
            }
            packet.GetInt16();
            var bgType = packet.GetInt32();
            var action = packet.GetInt8();
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_BATTLEFIELD_PORT [BG={2} Action={3}]", client.IP, client.Port, bgType, action);
            if (client.Character == null)
            {
                return;
            }
            if (action == 0)
            {
                Packets.PacketClass statusPacket = new(Opcodes.SMSG_BATTLEFIELD_STATUS);
                try
                {
                    statusPacket.AddInt32(0);
                    statusPacket.AddInt32(0);
                    statusPacket.AddInt32(0);
                    statusPacket.AddInt32(0);
                    statusPacket.AddInt32(0);
                    client.Send(ref statusPacket);
                }
                finally
                {
                    statusPacket.Dispose();
                }
            }
        }
        catch (Exception e)
        {
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.CRITICAL, "Error at battlefield port.{0}", Environment.NewLine + e);
        }
    }

    public void On_CMSG_LEAVE_BATTLEFIELD(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_LEAVE_BATTLEFIELD", client.IP, client.Port);
        if (client.Character == null)
        {
            return;
        }
        if (WorldServiceLocator.WSMaps.Maps.ContainsKey(client.Character.MapID) && WorldServiceLocator.WSMaps.Maps[client.Character.MapID].IsBattleGround)
        {
            client.Character.Teleport(client.Character.bindpoint_positionX, client.Character.bindpoint_positionY, client.Character.bindpoint_positionZ, 0f, client.Character.bindpoint_map_id);
        }
    }

    public void On_CMSG_AREA_SPIRIT_HEALER_QUERY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        try
        {
            if (checked(packet.Data.Length - 1) < 13)
            {
                return;
            }
            packet.GetInt16();
            var spiritGuid = packet.GetUInt64();
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AREA_SPIRIT_HEALER_QUERY [{2:X}]", client.IP, client.Port, spiritGuid);
            Packets.PacketClass response = new(Opcodes.SMSG_AREA_SPIRIT_HEALER_TIME);
            try
            {
                response.AddUInt64(spiritGuid);
                response.AddInt32(30000);
                client.Send(ref response);
            }
            finally
            {
                response.Dispose();
            }
        }
        catch (Exception e)
        {
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.CRITICAL, "Error at area spirit healer query.{0}", Environment.NewLine + e);
        }
    }

    public void On_CMSG_AREA_SPIRIT_HEALER_QUEUE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        try
        {
            if (checked(packet.Data.Length - 1) < 13)
            {
                return;
            }
            packet.GetInt16();
            var spiritGuid = packet.GetUInt64();
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AREA_SPIRIT_HEALER_QUEUE [{2:X}]", client.IP, client.Port, spiritGuid);
        }
        catch (Exception e)
        {
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.CRITICAL, "Error at area spirit healer queue.{0}", Environment.NewLine + e);
        }
    }
}
