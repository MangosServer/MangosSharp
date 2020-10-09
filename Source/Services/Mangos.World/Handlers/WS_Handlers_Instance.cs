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
using System.Data;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Map;
using Mangos.World.Maps;
using Mangos.World.Player;
using Mangos.World.Server;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Handlers
{
    public class WS_Handlers_Instance
    {
        public void InstanceMapUpdate()
        {
            var q = new DataTable();
            uint t = WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now);
            WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT * FROM characters_instances WHERE expire < {0};", (object)t), q);
            foreach (DataRow r in q.Rows)
            {
                if (WorldServiceLocator._WS_Maps.Maps.ContainsKey(Conversions.ToUInteger(r["map"])))
                    InstanceMapExpire(Conversions.ToUInteger(r["map"]), Conversions.ToUInteger(r["instance"]));
            }
        }

        public uint InstanceMapCreate(uint Map)
        {
            var q = new DataTable();

            // TODO: Save instance IDs in MAP class, using current way it may happen 2 groups to be in same instance
            WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT MAX(instance) FROM characters_instances WHERE map = {0};", (object)Map), q);
            if (!ReferenceEquals(q.Rows[0][0], DBNull.Value))
            {
                return (uint)(Conversions.ToInteger(q.Rows[0][0]) + 1);
            }
            else
            {
                return 0U;
            }
        }

        public void InstanceMapSpawn(uint Map, uint Instance)
        {
            // DONE: Load map data
            for (short x = 0; x <= 63; x++)
            {
                for (short y = 0; y <= 63; y++)
                {
                    if (WorldServiceLocator._WS_Maps.Maps[Map].TileUsed[x, y] == false && System.IO.File.Exists(string.Format(@"maps\{0}{1}{2}.map", Strings.Format(Map, "000"), Strings.Format(x, "00"), Strings.Format(y, "00"))))
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Loading map [{2}: {0},{1}]...", x, y, Map);
                        WorldServiceLocator._WS_Maps.Maps[Map].TileUsed[x, y] = true;
                        WorldServiceLocator._WS_Maps.Maps[Map].Tiles[x, y] = new WS_Maps.TMapTile((byte)x, (byte)y, Map);
                    }

                    if (WorldServiceLocator._WS_Maps.Maps[Map].Tiles[x, y] is object)
                    {
                        // DONE: Spawn the instance
                        WorldServiceLocator._WS_Maps.LoadSpawns((byte)x, (byte)y, Map, Instance);
                    }
                }
            }
        }

        public void InstanceMapExpire(uint Map, uint Instance)
        {
            bool empty = true;
            try
            {
                // DONE: Check for players
                for (short x = 0; x <= 63; x++)
                {
                    for (short y = 0; y <= 63; y++)
                    {
                        if (WorldServiceLocator._WS_Maps.Maps[Map].Tiles[x, y] is object)
                        {
                            foreach (ulong GUID in WorldServiceLocator._WS_Maps.Maps[Map].Tiles[x, y].PlayersHere.ToArray())
                            {
                                if (WorldServiceLocator._WorldServer.CHARACTERs[GUID].instance == Instance)
                                {
                                    empty = false;
                                    break;
                                }
                            }
                        }

                        if (!empty)
                            break;
                    }

                    if (!empty)
                        break;
                }

                if (empty)
                {
                    // DONE: Delete the instance if there are no players
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("DELETE FROM characters_instances WHERE instance = {0} AND map = {1};", Instance, Map));
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("DELETE FROM characters_instances_group WHERE instance = {0} AND map = {1};", Instance, Map));

                    // DONE: Delete spawned things
                    for (short x = 0; x <= 63; x++)
                    {
                        for (short y = 0; y <= 63; y++)
                        {
                            if (WorldServiceLocator._WS_Maps.Maps[Map].Tiles[x, y] is object)
                            {
                                foreach (ulong GUID in WorldServiceLocator._WS_Maps.Maps[Map].Tiles[x, y].CreaturesHere.ToArray())
                                {
                                    if (WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].instance == Instance)
                                        WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].Destroy();
                                }

                                foreach (ulong GUID in WorldServiceLocator._WS_Maps.Maps[Map].Tiles[x, y].GameObjectsHere.ToArray())
                                {
                                    if (WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].instance == Instance)
                                        WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].Destroy(WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID]);
                                }

                                foreach (ulong GUID in WorldServiceLocator._WS_Maps.Maps[Map].Tiles[x, y].CorpseObjectsHere.ToArray())
                                {
                                    if (WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs[GUID].instance == Instance)
                                        WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs[GUID].Destroy();
                                }

                                foreach (ulong GUID in WorldServiceLocator._WS_Maps.Maps[Map].Tiles[x, y].DynamicObjectsHere.ToArray())
                                {
                                    if (WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs[GUID].instance == Instance)
                                        WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs[GUID].Delete();
                                }
                            }
                        }
                    }
                }
                else
                {
                    // DONE: Extend the expire time
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_instances SET expire = {2} WHERE instance = {0} AND map = {1};", Instance, Map, WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now) + WorldServiceLocator._WS_Maps.Maps[Map].ResetTime));
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("UPDATE characters_instances_group SET expire = {2} WHERE instance = {0} AND map = {1};", Instance, Map, WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now) + WorldServiceLocator._WS_Maps.Maps[Map].ResetTime));

                    // DONE: Respawn the instance if there are players
                    for (short x = 0; x <= 63; x++)
                    {
                        for (short y = 0; y <= 63; y++)
                        {
                            if (WorldServiceLocator._WS_Maps.Maps[Map].Tiles[x, y] is object)
                            {
                                foreach (ulong GUID in WorldServiceLocator._WS_Maps.Maps[Map].Tiles[x, y].CreaturesHere.ToArray())
                                {
                                    if (WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].instance == Instance)
                                        WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].Respawn();
                                }

                                foreach (ulong GUID in WorldServiceLocator._WS_Maps.Maps[Map].Tiles[x, y].GameObjectsHere.ToArray())
                                {
                                    if (WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].instance == Instance)
                                        WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].Respawn(WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID]);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error expiring map instance.{0}{1}", Environment.NewLine, ex.ToString());
            }
        }

        public void InstanceMapEnter(WS_PlayerData.CharacterObject objCharacter)
        {
            if (WorldServiceLocator._WS_Maps.Maps[objCharacter.MapID].Type == MapTypes.MAP_COMMON)
            {
                objCharacter.instance = 0U;

                /* TODO ERROR: Skipped IfDirectiveTrivia */
                objCharacter.SystemMessage(WorldServiceLocator._Functions.SetColor("You are not in instance.", 0, 0, 255));
            }
            /* TODO ERROR: Skipped EndIfDirectiveTrivia */
            else
            {
                // DONE: Instances expire check
                InstanceMapUpdate();
                var q = new DataTable();

                // DONE: Check if player is already saved to instance
                WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT * FROM characters_instances WHERE char_guid = {0} AND map = {1};", (object)objCharacter.GUID, (object)objCharacter.MapID), q);
                if (q.Rows.Count > 0)
                {
                    // Character is saved to instance
                    objCharacter.instance = Conversions.ToUInteger(q.Rows[0]["instance"]);
                    /* TODO ERROR: Skipped IfDirectiveTrivia */
                    objCharacter.SystemMessage(WorldServiceLocator._Functions.SetColor(string.Format("You are in instance #{0}, map {1}", objCharacter.instance, objCharacter.MapID), 0, 0, 255));
                    /* TODO ERROR: Skipped EndIfDirectiveTrivia */
                    SendInstanceMessage(ref objCharacter.client, objCharacter.MapID, Conversions.ToInteger(Operators.SubtractObject(q.Rows[0]["expire"], WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now))));
                    return;
                }

                // DONE: Check if group is already in instance
                if (objCharacter.IsInGroup)
                {
                    WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT * FROM characters_instances_group WHERE group_id = {0} AND map = {1};", (object)objCharacter.Group.ID, (object)objCharacter.MapID), q);
                    if (q.Rows.Count > 0)
                    {
                        // Group is saved to instance
                        objCharacter.instance = Conversions.ToUInteger(q.Rows[0]["instance"]);
                        /* TODO ERROR: Skipped IfDirectiveTrivia */
                        objCharacter.SystemMessage(WorldServiceLocator._Functions.SetColor(string.Format("You are in instance #{0}, map {1}", objCharacter.instance, objCharacter.MapID), 0, 0, 255));
                        /* TODO ERROR: Skipped EndIfDirectiveTrivia */
                        SendInstanceMessage(ref objCharacter.client, objCharacter.MapID, Conversions.ToInteger(Operators.SubtractObject(q.Rows[0]["expire"], WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now))));
                        return;
                    }
                }

                // DONE Create new instance
                int instanceNewID = (int)InstanceMapCreate(objCharacter.MapID);
                int instanceNewResetTime = (int)(WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now) + WorldServiceLocator._WS_Maps.Maps[objCharacter.MapID].ResetTime);

                // Set instance
                objCharacter.instance = (uint)instanceNewID;
                if (objCharacter.IsInGroup)
                {
                    // Set group in the same instance
                    WorldServiceLocator._WorldServer.CharacterDatabase.Update(string.Format("INSERT INTO characters_instances_group (group_id, map, instance, expire) VALUES ({0}, {1}, {2}, {3});", objCharacter.Group.ID, objCharacter.MapID, instanceNewID, instanceNewResetTime));
                }

                InstanceMapSpawn(objCharacter.MapID, (uint)instanceNewID);

                /* TODO ERROR: Skipped IfDirectiveTrivia */
                objCharacter.SystemMessage(WorldServiceLocator._Functions.SetColor(string.Format("You are in instance #{0}, map {1}", objCharacter.instance, objCharacter.MapID), 0, 0, 255));
                /* TODO ERROR: Skipped EndIfDirectiveTrivia */
                SendInstanceMessage(ref objCharacter.client, objCharacter.MapID, (int)(WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now) - instanceNewResetTime));
            }
        }

        public void InstanceUpdate(uint Map, uint Instance, uint Cleared)
        {
            // NOTE: This should be used when a boss is killed, since he and his units will no longer spawn, raid instances only
            // TODO: Save everybody to the instance at the first kill
            // TODO: Save the instance to the database
        }

        public void InstanceMapLeave(WS_PlayerData.CharacterObject objChar)
        {
            // TODO: Start teleport timer
        }

        // SMSG_INSTANCE_DIFFICULTY
        public void SendResetInstanceSuccess(ref WS_Network.ClientClass client, uint Map)
        {
            // Dim p As New PacketClass(OPCODES.SMSG_INSTANCE_RESET)
            // p.AddUInt32(Map)
            // Client.Send(p)
            // p.Dispose()
        }

        public void SendResetInstanceFailed(ref WS_Network.ClientClass client, uint Map, ResetFailedReason Reason)
        {
            // Dim p As New PacketClass(OPCODES.SMSG_INSTANCE_RESET)
            // p.AddUInt32(Reason)
            // p.AddUInt32(Map)
            // Client.Send(p)
            // p.Dispose()
        }

        public void SendResetInstanceFailedNotify(ref WS_Network.ClientClass client, uint Map)
        {
            // Dim p As New PacketClass(OPCODES.SMSG_RESET_FAILED_NOTIFY)
            // p.AddUInt32(Map)
            // Client.Send(p)
            // p.Dispose()
        }

        private void SendUpdateInstanceOwnership(ref WS_Network.ClientClass client, uint Saved)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_UPDATE_INSTANCE_OWNERSHIP", client.IP, client.Port);

            // Dim p As New PacketClass(OPCODES.SMSG_UPDATE_INSTANCE_OWNERSHIP)
            // p.AddUInt32(Saved)                  'True/False if have been saved
            // Client.Send(p)
            // p.Dispose()
        }

        private void SendUpdateLastInstance(ref WS_Network.ClientClass client, uint Map)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_UPDATE_LAST_INSTANCE", client.IP, client.Port);

            // Dim p As New PacketClass(OPCODES.SMSG_UPDATE_LAST_INSTANCE)
            // p.AddUInt32(Map)
            // Client.Send(p)
            // p.Dispose()
        }

        public void SendInstanceSaved(WS_PlayerData.CharacterObject Character)
        {
            var q = new DataTable();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT * FROM characters_instances WHERE char_guid = {0};", (object)Character.GUID), q);
            SendUpdateInstanceOwnership(ref Character.client, Conversions.ToUInteger(q.Rows.Count > 0));
            foreach (DataRow r in q.Rows)
                SendUpdateLastInstance(ref Character.client, Conversions.ToUInteger(r["map"]));
        }

        public void SendInstanceMessage(ref WS_Network.ClientClass client, uint Map, int Time)
        {
            if (Time < 0)
            {
                Time = -Time;
            }
            else if (Time > 60 && Time < 3600)
            {
            }
            else if (Time > 3600)
            {
            }
            else if (Time < 60)
            {
            }

            // Dim p As New PacketClass(OPCODES.SMSG_RAID_INSTANCE_MESSAGE)
            // p.AddUInt32(Type)
            // p.AddUInt32(Map)
            // p.AddUInt32(Time)
            // Client.Send(p)
            // p.Dispose()
        }
    }
}