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

using System;
using System.Collections.Generic;
using System.Threading;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Unit;
using Mangos.Common.Globals;
using Mangos.World.DataStores;
using Mangos.World.Globals;
using Mangos.World.Objects;
using Mangos.World.Player;
using Mangos.World.Server;

namespace Mangos.World.Handlers
{
    public class WS_Handlers_Taxi
    {
        /// <summary>
        /// Sends the activate taxi reply.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="reply">The reply.</param>
        /// <returns></returns>
        private void SendActivateTaxiReply(ref WS_Network.ClientClass client, ActivateTaxiReplies reply)
        {
            var taxiFailed = new Packets.PacketClass(OPCODES.SMSG_ACTIVATETAXIREPLY);
            try
            {
                taxiFailed.AddInt32((int)reply);
                client.Send(ref taxiFailed);
            }
            finally
            {
                taxiFailed.Dispose();
            }
        }

        /// <summary>
        /// Sends the taxi status.
        /// </summary>
        /// <param name="objCharacter">The objCharacter.</param>
        /// <param name="cGuid">The objCharacter GUID.</param>
        /// <returns></returns>
        private void SendTaxiStatus(ref WS_PlayerData.CharacterObject objCharacter, ulong cGuid)
        {
            if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(cGuid) == false)
                return;
            int currentTaxi = WorldServiceLocator._WS_DBCDatabase.GetNearestTaxi(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].positionX, WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].positionY, (int)WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].MapID);
            var SMSG_TAXINODE_STATUS = new Packets.PacketClass(OPCODES.SMSG_TAXINODE_STATUS);
            try
            {
                SMSG_TAXINODE_STATUS.AddUInt64(cGuid);
                if (objCharacter.TaxiZones[currentTaxi] == false)
                    SMSG_TAXINODE_STATUS.AddInt8(0);
                else
                    SMSG_TAXINODE_STATUS.AddInt8(1);
                objCharacter.client.Send(ref SMSG_TAXINODE_STATUS);
            }
            finally
            {
                SMSG_TAXINODE_STATUS.Dispose();
            }
        }

        /// <summary>
        /// Sends the taxi menu.
        /// </summary>
        /// <param name="objCharacter">The objCharacter.</param>
        /// <param name="cGuid">The objCharacter GUID.</param>
        /// <returns></returns>
        public void SendTaxiMenu(ref WS_PlayerData.CharacterObject objCharacter, ulong cGuid)
        {
            if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(cGuid) == false)
                return;
            int currentTaxi = WorldServiceLocator._WS_DBCDatabase.GetNearestTaxi(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].positionX, WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].positionY, (int)WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].MapID);
            if (objCharacter.TaxiZones[currentTaxi] == false)
            {
                objCharacter.TaxiZones.Set(currentTaxi, true);
                var SMSG_NEW_TAXI_PATH = new Packets.PacketClass(OPCODES.SMSG_NEW_TAXI_PATH);
                try
                {
                    objCharacter.client.Send(ref SMSG_NEW_TAXI_PATH);
                }
                finally
                {
                    SMSG_NEW_TAXI_PATH.Dispose();
                }

                var SMSG_TAXINODE_STATUS = new Packets.PacketClass(OPCODES.SMSG_TAXINODE_STATUS);
                try
                {
                    SMSG_TAXINODE_STATUS.AddUInt64(cGuid);
                    SMSG_TAXINODE_STATUS.AddInt8(1);
                    objCharacter.client.Send(ref SMSG_TAXINODE_STATUS);
                }
                finally
                {
                    SMSG_TAXINODE_STATUS.Dispose();
                }

                return;
            }

            var SMSG_SHOWTAXINODES = new Packets.PacketClass(OPCODES.SMSG_SHOWTAXINODES);
            try
            {
                SMSG_SHOWTAXINODES.AddInt32(1);
                SMSG_SHOWTAXINODES.AddUInt64(cGuid);
                SMSG_SHOWTAXINODES.AddInt32(currentTaxi);
                SMSG_SHOWTAXINODES.AddBitArray(objCharacter.TaxiZones, 8 * 4);
                objCharacter.client.Send(ref SMSG_SHOWTAXINODES);
            }
            finally
            {
                SMSG_SHOWTAXINODES.Dispose();
            }
        }

        /// <summary>
        /// Called when [CMSG_TAXINODE_STATUS_QUERY] is received.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        public void On_CMSG_TAXINODE_STATUS_QUERY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 13)
                return;
            packet.GetInt16();
            ulong guid;
            guid = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_TAXINODE_STATUS_QUERY [taxiGUID={2:X}]", client.IP, client.Port, guid);
            if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(guid) == false)
                return;
            SendTaxiStatus(ref client.Character, guid);
        }

        /// <summary>
        /// Called when [CMSG_TAXIQUERYAVAILABLENODES] is received.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        public void On_CMSG_TAXIQUERYAVAILABLENODES(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 13)
                return;
            packet.GetInt16();
            ulong guid;
            guid = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_TAXIQUERYAVAILABLENODES [taxiGUID={2:X}]", client.IP, client.Port, guid);
            if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(guid) == false)
                return;
            if ((WorldServiceLocator._WorldServer.WORLD_CREATUREs[guid].CreatureInfo.cNpcFlags & NPCFlags.UNIT_NPC_FLAG_TAXIVENDOR) == 0)
                return; // NPC is not a taxi vendor
            SendTaxiMenu(ref client.Character, guid);
        }

        /// <summary>
        /// Called when [CMSG_ACTIVATETAXI] is received.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        public void On_CMSG_ACTIVATETAXI(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 21)
                return;
            packet.GetInt16();
            ulong guid;
            guid = packet.GetUInt64();
            int srcNode = packet.GetInt32();
            int dstNode = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_ACTIVATETAXI [taxiGUID={2:X} srcNode={3} dstNode={4}]", client.IP, client.Port, guid, srcNode, dstNode);
            if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(guid) == false || (WorldServiceLocator._WorldServer.WORLD_CREATUREs[guid].CreatureInfo.cNpcFlags & NPCFlags.UNIT_NPC_FLAG_TAXIVENDOR) == 0)
            {
                SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXINOVENDORNEARBY);
                return;
            }

            if (client.Character.LogoutTimer is object)
            {
                SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXIPLAYERBUSY);
                return;
            }

            if (client.Character.cUnitFlags & UnitFlags.UNIT_FLAG_DISABLE_MOVE)
            {
                SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXINOTSTANDING);
                return;
            }

            if (client.Character.ShapeshiftForm > 0 && client.Character.ShapeshiftForm != ShapeshiftForm.FORM_BERSERKERSTANCE && client.Character.ShapeshiftForm != ShapeshiftForm.FORM_BATTLESTANCE && client.Character.ShapeshiftForm != ShapeshiftForm.FORM_DEFENSIVESTANCE && client.Character.ShapeshiftForm != ShapeshiftForm.FORM_SHADOW)
            {
                SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXIPLAYERSHAPESHIFTED);
                return;
            }

            if (client.Character.Mount != 0)
            {
                SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXIPLAYERALREADYMOUNTED);
                return;
            }

            if (WorldServiceLocator._WS_DBCDatabase.TaxiNodes.ContainsKey(srcNode) == false || WorldServiceLocator._WS_DBCDatabase.TaxiNodes.ContainsKey(dstNode) == false)
            {
                SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXINOSUCHPATH);
                return;
            }

            int mount; // = 0
            if (client.Character.IsHorde)
            {
                if (WorldServiceLocator._WorldServer.CREATURESDatabase.ContainsKey(WorldServiceLocator._WS_DBCDatabase.TaxiNodes[srcNode].HordeMount) == false)
                {
                    var tmpCr = new CreatureInfo(WorldServiceLocator._WS_DBCDatabase.TaxiNodes[srcNode].HordeMount);
                    mount = tmpCr.GetFirstModel;
                }
                else
                {
                    mount = WorldServiceLocator._WorldServer.CREATURESDatabase[WorldServiceLocator._WS_DBCDatabase.TaxiNodes[srcNode].HordeMount].ModelA1;
                }
            }
            else if (WorldServiceLocator._WorldServer.CREATURESDatabase.ContainsKey(WorldServiceLocator._WS_DBCDatabase.TaxiNodes[srcNode].AllianceMount) == false)
            {
                var tmpCr = new CreatureInfo(WorldServiceLocator._WS_DBCDatabase.TaxiNodes[srcNode].AllianceMount);
                mount = tmpCr.GetFirstModel;
            }
            else
            {
                mount = WorldServiceLocator._WorldServer.CREATURESDatabase[WorldServiceLocator._WS_DBCDatabase.TaxiNodes[srcNode].AllianceMount].ModelA2;
            }

            if (mount == 0)
            {
                SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXIUNSPECIFIEDSERVERERROR);
                return;
            }

            // DONE: Reputation discount
            float discountMod;
            discountMod = client.Character.GetDiscountMod(WorldServiceLocator._WorldServer.WORLD_CREATUREs[guid].Faction);
            var totalCost = default(int);
            foreach (KeyValuePair<int, WS_DBCDatabase.TTaxiPath> taxiPath in WorldServiceLocator._WS_DBCDatabase.TaxiPaths)
            {
                if (taxiPath.Value.TFrom == srcNode && taxiPath.Value.TTo == dstNode)
                {
                    totalCost = (int)(totalCost + taxiPath.Value.Price * discountMod);
                    break;
                }
            }

            // DONE: Check if we have enough money
            if (client.Character.Copper < totalCost)
            {
                SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXINOTENOUGHMONEY);
                return;
            }

            client.Character.Copper = (uint)(client.Character.Copper - totalCost);
            client.Character.TaxiNodes.Clear();
            client.Character.TaxiNodes.Enqueue(srcNode);
            client.Character.TaxiNodes.Enqueue(dstNode);
            SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXIOK);

            // DONE: Mount up, disable move and spell casting
            TaxiTake(client.Character, mount);
            TaxiMove(client.Character, discountMod);
        }

        /// <summary>
        /// Called when [CMSG_ACTIVATETAXI_FAR] is received.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        public void On_CMSG_ACTIVATETAXI_FAR(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 21)
                return;
            packet.GetInt16();
            try
            {
                ulong guid;
                guid = packet.GetUInt64();
                int totalCost;
                totalCost = packet.GetInt32();
                int nodeCount;
                nodeCount = packet.GetInt32();
                if (nodeCount <= 0)
                    return;
                if (packet.Data.Length - 1 < 21 + 4 * nodeCount)
                    return;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_ACTIVATETAXI_FAR [taxiGUID={2:X} TotalCost={3} NodeCount={4}]", client.IP, client.Port, guid, totalCost, nodeCount);
                if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(guid) == false || (WorldServiceLocator._WorldServer.WORLD_CREATUREs[guid].CreatureInfo.cNpcFlags & NPCFlags.UNIT_NPC_FLAG_TAXIVENDOR) == 0)
                {
                    SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXINOVENDORNEARBY);
                    return;
                }

                if (client.Character.LogoutTimer is object)
                {
                    SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXIPLAYERBUSY);
                    return;
                }

                if (client.Character.cUnitFlags & UnitFlags.UNIT_FLAG_DISABLE_MOVE)
                {
                    SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXINOTSTANDING);
                    return;
                }

                if (client.Character.ShapeshiftForm > 0 && client.Character.ShapeshiftForm != ShapeshiftForm.FORM_BERSERKERSTANCE && client.Character.ShapeshiftForm != ShapeshiftForm.FORM_BATTLESTANCE && client.Character.ShapeshiftForm != ShapeshiftForm.FORM_DEFENSIVESTANCE && client.Character.ShapeshiftForm != ShapeshiftForm.FORM_SHADOW)
                {
                    SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXIPLAYERSHAPESHIFTED);
                    return;
                }

                if (client.Character.Mount != 0)
                {
                    SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXIPLAYERALREADYMOUNTED);
                    return;
                }

                if (nodeCount < 1)
                {
                    SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXINOSUCHPATH);
                    return;
                }

                if (client.Character.Copper < totalCost)
                {
                    SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXINOTENOUGHMONEY);
                    return;
                }

                // DONE: Load nodes
                var nodes = new List<int>();
                for (int i = 0, loopTo = nodeCount - 1; i <= loopTo; i++)
                    nodes.Add(packet.GetInt32());
                int srcNode = nodes[0];
                int dstNode = nodes[1];
                foreach (int node in client.Character.TaxiNodes)
                {
                    if (!WorldServiceLocator._WS_DBCDatabase.TaxiNodes.ContainsKey(node))
                    {
                        SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXINOSUCHPATH);
                        return;
                    }
                }

                int mount; // = 0
                if (client.Character.IsHorde)
                {
                    if (WorldServiceLocator._WorldServer.CREATURESDatabase.ContainsKey(WorldServiceLocator._WS_DBCDatabase.TaxiNodes[srcNode].HordeMount) == false)
                    {
                        // TODO: This was here for a reason, i'm guessing to correct the line below.but it is never used
                        var tmpCr = new CreatureInfo(WorldServiceLocator._WS_DBCDatabase.TaxiNodes[srcNode].HordeMount);
                        mount = tmpCr.GetFirstModel;
                    }
                    else
                    {
                        mount = WorldServiceLocator._WorldServer.CREATURESDatabase[WorldServiceLocator._WS_DBCDatabase.TaxiNodes[srcNode].HordeMount].GetFirstModel;
                    }
                }
                else if (WorldServiceLocator._WorldServer.CREATURESDatabase.ContainsKey(WorldServiceLocator._WS_DBCDatabase.TaxiNodes[srcNode].AllianceMount) == false)
                {
                    // TODO: This was here for a reason, i'm guessing to correct the line below.but it is never used
                    var tmpCr = new CreatureInfo(WorldServiceLocator._WS_DBCDatabase.TaxiNodes[srcNode].AllianceMount);
                    mount = tmpCr.GetFirstModel;
                }
                else
                {
                    mount = WorldServiceLocator._WorldServer.CREATURESDatabase[WorldServiceLocator._WS_DBCDatabase.TaxiNodes[srcNode].AllianceMount].GetFirstModel;
                }

                if (mount == 0)
                {
                    SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXIUNSPECIFIEDSERVERERROR);
                    return;
                }

                // DONE: Reputation discount
                float discountMod = client.Character.GetDiscountMod(WorldServiceLocator._WorldServer.WORLD_CREATUREs[guid].Faction);
                totalCost = 0;
                foreach (KeyValuePair<int, WS_DBCDatabase.TTaxiPath> taxiPath in WorldServiceLocator._WS_DBCDatabase.TaxiPaths)
                {
                    if (taxiPath.Value.TFrom == srcNode && taxiPath.Value.TTo == dstNode)
                    {
                        totalCost = (int)(totalCost + taxiPath.Value.Price * discountMod);
                        break;
                    }
                }

                // DONE: Check if we have enough money
                if (client.Character.Copper < totalCost)
                {
                    SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXINOTENOUGHMONEY);
                    return;
                }

                client.Character.Copper = (uint)(client.Character.Copper - totalCost);
                client.Character.TaxiNodes.Clear();
                foreach (int node in nodes)
                    client.Character.TaxiNodes.Enqueue(node);
                SendActivateTaxiReply(ref client, ActivateTaxiReplies.ERR_TAXIOK);

                // DONE: Mount up, disable move and spell casting
                TaxiTake(client.Character, mount);
                TaxiMove(client.Character, discountMod);
            }
            catch (Exception e)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error when taking a long taxi.{0}", Environment.NewLine + e.ToString());
            }
        }

        /// <summary>
        /// Called when [CMSG_MOVE_SPLINE_DONE] is received.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        public void On_CMSG_MOVE_SPLINE_DONE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MOVE_SPLINE_DONE", client.IP, client.Port);
        }

        /// <summary>
        /// Lands the Taxi.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <returns></returns>
        private void TaxiLand(WS_PlayerData.CharacterObject character)
        {
            character.TaxiNodes.Clear();
            character.Mount = 0;
            character.cUnitFlags = character.cUnitFlags & !UnitFlags.UNIT_FLAG_DISABLE_MOVE;
            character.cUnitFlags = character.cUnitFlags & !UnitFlags.UNIT_FLAG_TAXI_FLIGHT;
            character.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_MOUNTDISPLAYID, character.Mount);
            character.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_FLAGS, character.cUnitFlags);
            character.SendCharacterUpdate();
        }

        /// <summary>
        /// Takes the Taxi.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="mount">The mount.</param>
        /// <returns></returns>
        private void TaxiTake(WS_PlayerData.CharacterObject character, int mount)
        {
            character.Mount = mount;
            character.cUnitFlags = character.cUnitFlags | UnitFlags.UNIT_FLAG_DISABLE_MOVE;
            character.cUnitFlags = character.cUnitFlags | UnitFlags.UNIT_FLAG_TAXI_FLIGHT;
            character.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_MOUNTDISPLAYID, character.Mount);
            character.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_FLAGS, character.cUnitFlags);
            character.SetUpdateFlag((int)EPlayerFields.PLAYER_FIELD_COINAGE, character.Copper);
            character.SendCharacterUpdate();
        }

        /// <summary>
        /// Moves the Taxi.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="discountMod">The discount mod.</param>
        /// <returns></returns>
        private void TaxiMove(WS_PlayerData.CharacterObject character, float discountMod)
        {
            bool flagFirstNode;
            flagFirstNode = true;
            float lastX;
            float lastY;
            float lastZ;
            float moveDistance;
            float totalDistance;
            var waypointPaths = new List<int>();
            var waypointNodes = new Dictionary<int, WS_DBCDatabase.TTaxiPathNode>();
            try
            {
                // DONE: Generate paths
                int srcNode;
                int dstNode = character.TaxiNodes.Dequeue();
                while (character.TaxiNodes.Count > 0)
                {
                    srcNode = dstNode;
                    dstNode = character.TaxiNodes.Dequeue();
                    foreach (KeyValuePair<int, WS_DBCDatabase.TTaxiPath> taxiPath in WorldServiceLocator._WS_DBCDatabase.TaxiPaths)
                    {
                        if (taxiPath.Value.TFrom == srcNode && taxiPath.Value.TTo == dstNode)
                        {
                            waypointPaths.Add(taxiPath.Key);
                            break;
                        }
                    }
                }

                // DONE: Do move on paths
                foreach (int path in waypointPaths)
                {
                    if (flagFirstNode)
                    {
                        // DONE: Don't tax first node, it is already taxed
                        flagFirstNode = false;
                    }
                    else
                    {
                        // DONE: Remove the money for this flight
                        int price = (int)(WorldServiceLocator._WS_DBCDatabase.TaxiPaths[path].Price * discountMod);
                        if (character.Copper < price)
                            break;
                        character.Copper = (uint)(character.Copper - price);
                        character.SetUpdateFlag((int)EPlayerFields.PLAYER_FIELD_COINAGE, character.Copper);
                        character.SendCharacterUpdate(false);
                        Console.WriteLine("Paying {0}", price);
                    }

                    lastX = character.positionX;
                    lastY = character.positionY;
                    lastZ = character.positionZ;
                    totalDistance = 0f;

                    // DONE: Get the waypoints
                    waypointNodes.Clear();
                    foreach (KeyValuePair<int, WS_DBCDatabase.TTaxiPathNode> taxiPathNode in WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes[path])
                    {
                        waypointNodes.Add(taxiPathNode.Value.Seq, taxiPathNode.Value);
                        totalDistance += WorldServiceLocator._WS_Combat.GetDistance(lastX, taxiPathNode.Value.x, lastY, taxiPathNode.Value.y, lastZ, taxiPathNode.Value.z);
                        lastX = taxiPathNode.Value.x;
                        lastY = taxiPathNode.Value.y;
                        lastZ = taxiPathNode.Value.z;
                    }

                    lastX = character.positionX;
                    lastY = character.positionY;
                    lastZ = character.positionZ;

                    // Send move packet for player
                    var SMSG_MONSTER_MOVE = new Packets.PacketClass(OPCODES.SMSG_MONSTER_MOVE);
                    try
                    {
                        SMSG_MONSTER_MOVE.AddPackGUID(character.GUID);
                        SMSG_MONSTER_MOVE.AddSingle(character.positionX);
                        SMSG_MONSTER_MOVE.AddSingle(character.positionY);
                        SMSG_MONSTER_MOVE.AddSingle(character.positionZ);
                        SMSG_MONSTER_MOVE.AddInt32(WorldServiceLocator._NativeMethods.timeGetTime(""));
                        SMSG_MONSTER_MOVE.AddInt8(0);
                        SMSG_MONSTER_MOVE.AddInt32(0x300);                           // Flags [0x0 - Walk, 0x100 - Run, 0x200 - Waypoint, 0x300 - Fly]
                        SMSG_MONSTER_MOVE.AddInt32(Fix(totalDistance / WorldServiceLocator._Global_Constants.UNIT_NORMAL_TAXI_SPEED * 1000));   // Time
                        SMSG_MONSTER_MOVE.AddInt32(waypointNodes.Count);             // Points Count
                        for (int j = 0, loopTo = waypointNodes.Count - 1; j <= loopTo; j++)
                        {
                            SMSG_MONSTER_MOVE.AddSingle(waypointNodes[j].x);         // First Point X
                            SMSG_MONSTER_MOVE.AddSingle(waypointNodes[j].y);         // First Point Y
                            SMSG_MONSTER_MOVE.AddSingle(waypointNodes[j].z);         // First Point Z
                        }

                        character.client.Send(ref SMSG_MONSTER_MOVE);
                    }
                    finally
                    {
                        SMSG_MONSTER_MOVE.Dispose();
                    }

                    for (int i = 0, loopTo1 = waypointNodes.Count - 1; i <= loopTo1; i++)
                    {
                        moveDistance = WorldServiceLocator._WS_Combat.GetDistance(lastX, waypointNodes[i].x, lastY, waypointNodes[i].y, lastZ, waypointNodes[i].z);
                        lastX = waypointNodes[i].x;
                        lastY = waypointNodes[i].y;
                        lastZ = waypointNodes[i].z;

                        // Send move packet for other players
                        var p = new Packets.PacketClass(OPCODES.SMSG_MONSTER_MOVE);
                        try
                        {
                            p.AddPackGUID(character.GUID);
                            p.AddSingle(character.positionX);
                            p.AddSingle(character.positionY);
                            p.AddSingle(character.positionZ);
                            p.AddInt32(WorldServiceLocator._NativeMethods.timeGetTime(""));
                            p.AddInt8(0);
                            p.AddInt32(0x300);                           // Flags [0x0 - Walk, 0x100 - Run, 0x200 - Waypoint, 0x300 - Fly]
                            p.AddInt32(Fix(totalDistance / WorldServiceLocator._Global_Constants.UNIT_NORMAL_TAXI_SPEED * 1000));   // Time
                            p.AddInt32(waypointNodes.Count);             // Points Count
                            for (int j = i, loopTo2 = waypointNodes.Count - 1; j <= loopTo2; j++)
                            {
                                p.AddSingle(waypointNodes[j].x);         // First Point X
                                p.AddSingle(waypointNodes[j].y);         // First Point Y
                                p.AddSingle(waypointNodes[j].z);         // First Point Z
                            }

                            character.SendToNearPlayers(ref p, ToSelf: false);
                        }
                        finally
                        {
                            p.Dispose();
                        }

                        // Wait move to complete
                        Thread.Sleep(Fix(moveDistance / WorldServiceLocator._Global_Constants.UNIT_NORMAL_TAXI_SPEED * 1000));

                        // Update character postion
                        totalDistance -= moveDistance;
                        character.positionX = lastX;
                        character.positionY = lastY;
                        character.positionZ = lastZ;
                        WorldServiceLocator._WS_CharMovement.MoveCell(ref character);
                        WorldServiceLocator._WS_CharMovement.UpdateCell(ref character);
                    }
                }
            }
            catch (Exception ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error on flight: {0}", ex.ToString());
            }

            character.Save();
            TaxiLand(character);
        }
    }
}