//
//  Copyright (C) 2013-2020 getMaNGOS <https://getmangos.eu>
//  
//  This program is free software. You can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation. either version 2 of the License, or
//  (at your option) any later version.
//  
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY. Without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//  
//  You should have received a copy of the GNU General Public License
//  along with this program. If not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Mangos.Cluster.Globals;
using Mangos.Cluster.Handlers;
using Mangos.Common.Enums.Chat;
using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using Mangos.Common.Legacy;
using Mangos.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace Mangos.Cluster.Network
{
    public class WorldServerClass : Hub, ICluster
    {
        private readonly ClusterServiceLocator clusterServiceLocator;

        public bool m_flagStopListen = false;
        private Timer m_TimerPing;
        private Timer m_TimerStats;
        private Timer m_TimerCPU;

        public WorldServerClass(ClusterServiceLocator clusterServiceLocator)
        {
            this.clusterServiceLocator = clusterServiceLocator;
        }

        public void Start()
        {
            // Creating ping timer
            m_TimerPing = new Timer(Ping, null, 0, 15000);

            // Creating stats timer
            if (clusterServiceLocator._WorldCluster.GetConfig().StatsEnabled)
            {
                m_TimerStats = new Timer(clusterServiceLocator._WC_Stats.GenerateStats, null, clusterServiceLocator._WorldCluster.GetConfig().StatsTimer, clusterServiceLocator._WorldCluster.GetConfig().StatsTimer);
            }

            // Creating CPU check timer
            m_TimerCPU = new Timer(clusterServiceLocator._WC_Stats.CheckCpu, null, 1000, 1000);
        }

        public Dictionary<uint, IWorld> Worlds = new Dictionary<uint, IWorld>();
        public Dictionary<uint, WorldInfo> WorldsInfo = new Dictionary<uint, WorldInfo>();

        public bool Connect(string uri, List<uint> maps)
        {
            try
            {
                Disconnect(uri, maps);
                var WorldServerInfo = new WorldInfo();
                clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "Connected Map Server: {0}", uri);
                lock (((ICollection)Worlds).SyncRoot)
                {
                    foreach (uint Map in maps)
                    {

                        // NOTE: Password protected remoting
                        Worlds[Map] = ProxyClient.Create<IWorld>(uri);
                        WorldsInfo[Map] = WorldServerInfo;
                    }
                }
            }
            catch (Exception ex)
            {
                clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.CRITICAL, "Unable to reverse connect. [{0}]", ex.ToString());
                return false;
            }

            return true;
        }

        public void Disconnect(string uri, List<uint> maps)
        {
            if (maps.Count == 0)
                return;

            // TODO: Unload arenas or battlegrounds that is hosted on this server!
            foreach (uint map in maps)
            {

                // DONE: Disconnecting clients
                lock (((ICollection)clusterServiceLocator._WorldCluster.CLIENTs).SyncRoot)
                {
                    foreach (KeyValuePair<uint, ClientClass> objCharacter in clusterServiceLocator._WorldCluster.CLIENTs)
                    {
                        if (objCharacter.Value.Character is object && objCharacter.Value.Character.IsInWorld && objCharacter.Value.Character.Map == map)
                        {
                            objCharacter.Value.Send(new PacketClass(Opcodes.SMSG_LOGOUT_COMPLETE));
                            new PacketClass(Opcodes.SMSG_LOGOUT_COMPLETE).Dispose();
                            objCharacter.Value.Character.Dispose();
                            objCharacter.Value.Character = null;
                        }
                    }
                }

                if (Worlds.ContainsKey(map))
                {
                    try
                    {
                        Worlds[map] = default;
                        WorldsInfo[map] = null;
                    }
                    catch
                    {
                    }
                    finally
                    {
                        lock (((ICollection)Worlds).SyncRoot)
                        {
                            Worlds.Remove(map);
                            WorldsInfo.Remove(map);
                            clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "Map: {0:000} has been disconnected!", map);
                        }
                    }
                }
            }
        }

        public void Ping(object State)
        {
            var DownedServers = new List<uint>();
            var SentPingTo = new Dictionary<WorldInfo, int>();
            int MyTime;
            int ServerTime;
            int Latency;

            // Ping WorldServers
            lock (((ICollection)Worlds).SyncRoot)
            {
                if (Worlds != null && WorldsInfo != null)
                {
                    foreach (KeyValuePair<uint, IWorld> w in Worlds)
                    {
                        try
                        {
                            if (!SentPingTo.ContainsKey(WorldsInfo[w.Key]))
                            {
                                MyTime = clusterServiceLocator._NativeMethods.timeGetTime("");
                                ServerTime = w.Value.Ping(MyTime, WorldsInfo[w.Key].Latency);
                                Latency = Math.Abs(MyTime - clusterServiceLocator._NativeMethods.timeGetTime(""));
                                WorldsInfo[w.Key].Latency = Latency;
                                SentPingTo[WorldsInfo[w.Key]] = Latency;
                                clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.NETWORK, "Map {0:000} ping: {1}ms", w.Key, Latency);

                                // Query CPU and Memory usage
                                var serverInfo = w.Value.GetServerInfo();
                                WorldsInfo[w.Key].CPUUsage = serverInfo.cpuUsage;
                                WorldsInfo[w.Key].MemoryUsage = serverInfo.memoryUsage;
                            }
                            else
                            {
                                clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.NETWORK, "Map {0:000} ping: {1}ms", w.Key, SentPingTo[WorldsInfo[w.Key]]);
                            }
                        }
                        catch (Exception)
                        {
                            clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.WARNING, "Map {0:000} is currently down!", w.Key);
                            DownedServers.Add(w.Key);
                        }
                    }
                }
            }

            // Notification message
            if (Worlds.Count == 0)
                clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.WARNING, "No maps are currently available!");

            // Drop WorldServers
            Disconnect("NULL", DownedServers);
        }

        public void ClientSend(uint id, byte[] data)
        {
            if (clusterServiceLocator._WorldCluster.CLIENTs.ContainsKey(id))
                clusterServiceLocator._WorldCluster.CLIENTs[id].Send(data);
        }

        public void ClientDrop(uint ID)
        {
            if (clusterServiceLocator._WorldCluster.CLIENTs.ContainsKey(ID))
            {
                try
                {
                    clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "[{0:000000}] Client has dropped map {1:000}", ID, clusterServiceLocator._WorldCluster.CLIENTs[ID].Character.Map);
                    clusterServiceLocator._WorldCluster.CLIENTs[ID].Character.IsInWorld = false;
                    clusterServiceLocator._WorldCluster.CLIENTs[ID].Character.OnLogout();
                }
                catch (Exception ex)
                {
                    clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "[{0:000000}] Client has dropped an exception: {1}", ID, ex.ToString());
                }
            }
            else
            {
                clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "[{0:000000}] Client connection has been lost.", ID);
            }
        }

        public void ClientTransfer(uint ID, float posX, float posY, float posZ, float ori, uint map)
        {
            clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "[{0:000000}] Client has transferred from map {1:000} to map {2:000}", ID, clusterServiceLocator._WorldCluster.CLIENTs[ID].Character.Map, map);
            var p = new PacketClass(Opcodes.SMSG_NEW_WORLD);
            p.AddUInt32(map);
            p.AddSingle(posX);
            p.AddSingle(posY);
            p.AddSingle(posZ);
            p.AddSingle(ori);
            clusterServiceLocator._WorldCluster.CLIENTs[ID].Send(p);
            clusterServiceLocator._WorldCluster.CLIENTs[ID].Character.Map = map;
        }

        public void ClientUpdate(uint ID, uint zone, byte level)
        {
            if (clusterServiceLocator._WorldCluster.CLIENTs[ID].Character is null)
                return;
            clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "[{0:000000}] Client has an updated zone {1:000}", ID, zone);
            clusterServiceLocator._WorldCluster.CLIENTs[ID].Character.Zone = zone;
            clusterServiceLocator._WorldCluster.CLIENTs[ID].Character.Level = level;
        }

        public void ClientSetChatFlag(uint ID, byte flag)
        {
            if (clusterServiceLocator._WorldCluster.CLIENTs[ID].Character is null)
                return;
            clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0:000000}] Client chat flag update [0x{1:X}]", ID, flag);
            clusterServiceLocator._WorldCluster.CLIENTs[ID].Character.ChatFlag = (ChatFlag)flag;
        }

        public byte[] ClientGetCryptKey(uint ID)
        {
            clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0:000000}] Requested client crypt key", ID);
            return clusterServiceLocator._WorldCluster.CLIENTs[ID].SS_Hash;
        }

        public void Broadcast(byte[] Data)
        {
            byte[] b;
            clusterServiceLocator._WorldCluster.CHARACTERs_Lock.AcquireReaderLock(clusterServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
            foreach (KeyValuePair<ulong, WcHandlerCharacter.CharacterObject> objCharacter in clusterServiceLocator._WorldCluster.CHARACTERs)
            {
                if (objCharacter.Value.IsInWorld && objCharacter.Value.Client is object)
                {
                    b = (byte[])Data.Clone();
                    objCharacter.Value.Client.Send(Data);
                }
            }

            clusterServiceLocator._WorldCluster.CHARACTERs_Lock.ReleaseReaderLock();
        }

        public void BroadcastGroup(long groupId, byte[] Data)
        {
            {
                var withBlock = clusterServiceLocator._WC_Handlers_Group.GROUPs[groupId];
                for (byte i = 0, loopTo = (byte)(withBlock.Members.Length - 1); i <= loopTo; i++)
                {
                    if (withBlock.Members[i] is object)
                    {
                        withBlock.Members[i].Client.Send((byte[])Data.Clone());
                    }
                }
            }
        }

        public void BroadcastRaid(long GroupID, byte[] Data)
        {
            {
                var withBlock = clusterServiceLocator._WC_Handlers_Group.GROUPs[GroupID];
                for (byte i = 0, loopTo = (byte)(withBlock.Members.Length - 1); i <= loopTo; i++)
                {
                    if (withBlock.Members[i] is object && withBlock.Members[i].Client is object)
                    {
                        withBlock.Members[i].Client.Send((byte[])Data.Clone());
                    }
                }
            }
        }

        public void BroadcastGuild(long GuildID, byte[] Data)
        {
            // TODO: Not implement yet
        }

        public void BroadcastGuildOfficers(long GuildID, byte[] Data)
        {
            // TODO: Not implement yet
        }

        public bool InstanceCheck(ClientClass client, uint MapID)
        {
            if (!clusterServiceLocator._WC_Network.WorldServer.Worlds.ContainsKey(MapID))
            {
                // We don't create new continents
                if (clusterServiceLocator._Functions.IsContinentMap((int)MapID))
                {
                    clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.WARNING, "[{0:000000}] Requested Instance Map [{1}] is a continent", client.Index, MapID);
                    client.Send(new PacketClass(Opcodes.SMSG_LOGOUT_COMPLETE));
                    new PacketClass(Opcodes.SMSG_LOGOUT_COMPLETE).Dispose();
                    client.Character.IsInWorld = false;
                    return false;
                }

                clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "[{0:000000}] Requesting Instance Map [{1}]", client.Index, MapID);
                IWorld ParentMap = default;
                WorldInfo ParentMapInfo = null;

                // Check if we got parent map
                if (clusterServiceLocator._WC_Network.WorldServer.Worlds.ContainsKey((uint)clusterServiceLocator._WS_DBCDatabase.Maps[(int)MapID].ParentMap) && clusterServiceLocator._WC_Network.WorldServer.Worlds[(uint)clusterServiceLocator._WS_DBCDatabase.Maps[(int)MapID].ParentMap].InstanceCanCreate((int)clusterServiceLocator._WS_DBCDatabase.Maps[(int)MapID].Type))
                {
                    ParentMap = clusterServiceLocator._WC_Network.WorldServer.Worlds[(uint)clusterServiceLocator._WS_DBCDatabase.Maps[(int)MapID].ParentMap];
                    ParentMapInfo = clusterServiceLocator._WC_Network.WorldServer.WorldsInfo[(uint)clusterServiceLocator._WS_DBCDatabase.Maps[(int)MapID].ParentMap];
                }
                else if (clusterServiceLocator._WC_Network.WorldServer.Worlds.ContainsKey(0U) && clusterServiceLocator._WC_Network.WorldServer.Worlds[0U].InstanceCanCreate((int)clusterServiceLocator._WS_DBCDatabase.Maps[(int)MapID].Type))
                {
                    ParentMap = clusterServiceLocator._WC_Network.WorldServer.Worlds[0U];
                    ParentMapInfo = clusterServiceLocator._WC_Network.WorldServer.WorldsInfo[0U];
                }
                else if (clusterServiceLocator._WC_Network.WorldServer.Worlds.ContainsKey(1U) && clusterServiceLocator._WC_Network.WorldServer.Worlds[1U].InstanceCanCreate((int)clusterServiceLocator._WS_DBCDatabase.Maps[(int)MapID].Type))
                {
                    ParentMap = clusterServiceLocator._WC_Network.WorldServer.Worlds[1U];
                    ParentMapInfo = clusterServiceLocator._WC_Network.WorldServer.WorldsInfo[1U];
                }

                if (ParentMap is null)
                {
                    clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.WARNING, "[{0:000000}] Requested Instance Map [{1}] can't be loaded", client.Index, MapID);
                    client.Send(new PacketClass(Opcodes.SMSG_LOGOUT_COMPLETE));
                    new PacketClass(Opcodes.SMSG_LOGOUT_COMPLETE).Dispose();
                    client.Character.IsInWorld = false;
                    return false;
                }

                if (ParentMap.InstanceCreateAsync(MapID) != null)
                {
                    ParentMap.InstanceCreateAsync(MapID).Wait();
                }

                lock (((ICollection)Worlds).SyncRoot)
                {
                    clusterServiceLocator._WC_Network.WorldServer.Worlds.Add(MapID, ParentMap);
                    clusterServiceLocator._WC_Network.WorldServer.WorldsInfo.Add(MapID, ParentMapInfo);
                }
                return true;
            }

            return true;
        }

        public bool BattlefieldCheck(uint MapID)
        {
            // Create map
            if (!clusterServiceLocator._WC_Network.WorldServer.Worlds.ContainsKey(MapID))
            {
                clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "[SERVER] Requesting battlefield map [{0}]", MapID);
                IWorld ParentMap = default;
                WorldInfo ParentMapInfo = null;

                // Check if we got parent map
                if (clusterServiceLocator._WC_Network.WorldServer.Worlds.ContainsKey((uint)clusterServiceLocator._WS_DBCDatabase.Maps[(int)MapID].ParentMap) && clusterServiceLocator._WC_Network.WorldServer.Worlds[(uint)clusterServiceLocator._WS_DBCDatabase.Maps[(int)MapID].ParentMap].InstanceCanCreate((int)clusterServiceLocator._WS_DBCDatabase.Maps[(int)MapID].Type))
                {
                    ParentMap = clusterServiceLocator._WC_Network.WorldServer.Worlds[(uint)clusterServiceLocator._WS_DBCDatabase.Maps[(int)MapID].ParentMap];
                    ParentMapInfo = clusterServiceLocator._WC_Network.WorldServer.WorldsInfo[(uint)clusterServiceLocator._WS_DBCDatabase.Maps[(int)MapID].ParentMap];
                }
                else if (clusterServiceLocator._WC_Network.WorldServer.Worlds.ContainsKey(0U) && clusterServiceLocator._WC_Network.WorldServer.Worlds[0U].InstanceCanCreate((int)clusterServiceLocator._WS_DBCDatabase.Maps[(int)MapID].Type))
                {
                    ParentMap = clusterServiceLocator._WC_Network.WorldServer.Worlds[0U];
                    ParentMapInfo = clusterServiceLocator._WC_Network.WorldServer.WorldsInfo[0U];
                }
                else if (clusterServiceLocator._WC_Network.WorldServer.Worlds.ContainsKey(1U) && clusterServiceLocator._WC_Network.WorldServer.Worlds[1U].InstanceCanCreate((int)clusterServiceLocator._WS_DBCDatabase.Maps[(int)MapID].Type))
                {
                    ParentMap = clusterServiceLocator._WC_Network.WorldServer.Worlds[1U];
                    ParentMapInfo = clusterServiceLocator._WC_Network.WorldServer.WorldsInfo[1U];
                }

                if (ParentMap is null)
                {
                    clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.WARNING, "[SERVER] Requested battlefield map [{0}] can't be loaded", MapID);
                    return false;
                }

                ParentMap.InstanceCreateAsync(MapID).Wait();
                lock (((ICollection)Worlds).SyncRoot)
                {
                    clusterServiceLocator._WC_Network.WorldServer.Worlds.Add(MapID, ParentMap);
                    clusterServiceLocator._WC_Network.WorldServer.WorldsInfo.Add(MapID, ParentMapInfo);
                }
                return true;
            }

            return true;
        }

        public List<int> BattlefieldList(byte MapType)
        {
            var BattlefieldMap = new List<int>();
            clusterServiceLocator._WC_Handlers_Battleground.BATTLEFIELDs_Lock.AcquireReaderLock(clusterServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
            foreach (KeyValuePair<int, WC_Handlers_Battleground.Battlefield> BG in clusterServiceLocator._WC_Handlers_Battleground.BATTLEFIELDs)
            {
                if ((byte)BG.Value.MapType == MapType)
                {
                    BattlefieldMap.Add(BG.Value.ID);
                }
            }

            clusterServiceLocator._WC_Handlers_Battleground.BATTLEFIELDs_Lock.ReleaseReaderLock();
            return BattlefieldMap;
        }

        public void BattlefieldFinish(int battlefieldId)
        {
            clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "[B{0:0000}] Battlefield finished", battlefieldId);
        }

        public void GroupRequestUpdate(uint ID)
        {
            if (clusterServiceLocator._WorldCluster.CLIENTs.ContainsKey(ID) && clusterServiceLocator._WorldCluster.CLIENTs[ID].Character is object && clusterServiceLocator._WorldCluster.CLIENTs[ID].Character.IsInWorld && clusterServiceLocator._WorldCluster.CLIENTs[ID].Character.IsInGroup)
            {
                clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.NETWORK, "[G{0:00000}] Group update request", clusterServiceLocator._WorldCluster.CLIENTs[ID].Character.Group.Id);
                try
                {
                    clusterServiceLocator._WorldCluster.CLIENTs[ID].Character.GetWorld.GroupUpdate(clusterServiceLocator._WorldCluster.CLIENTs[ID].Character.Group.Id, (byte)clusterServiceLocator._WorldCluster.CLIENTs[ID].Character.Group.Type, clusterServiceLocator._WorldCluster.CLIENTs[ID].Character.Group.GetLeader().Guid, clusterServiceLocator._WorldCluster.CLIENTs[ID].Character.Group.GetMembers());
                    clusterServiceLocator._WorldCluster.CLIENTs[ID].Character.GetWorld.GroupUpdateLoot(clusterServiceLocator._WorldCluster.CLIENTs[ID].Character.Group.Id, (byte)clusterServiceLocator._WorldCluster.CLIENTs[ID].Character.Group.DungeonDifficulty, (byte)clusterServiceLocator._WorldCluster.CLIENTs[ID].Character.Group.LootMethod, (byte)clusterServiceLocator._WorldCluster.CLIENTs[ID].Character.Group.LootThreshold, clusterServiceLocator._WorldCluster.CLIENTs[ID].Character.Group.GetLootMaster().Guid);
                }
                catch
                {
                    clusterServiceLocator._WC_Network.WorldServer.Disconnect("NULL", new List<uint> { clusterServiceLocator._WorldCluster.CLIENTs[ID].Character.Map });
                }
            }
        }

        public void GroupSendUpdate(long GroupID)
        {
            clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.NETWORK, "[G{0:00000}] Group update", GroupID);
            lock (((ICollection)Worlds).SyncRoot)
            {
                foreach (KeyValuePair<uint, IWorld> w in Worlds)
                {
                    try
                    {
                        w.Value.GroupUpdate(GroupID, (byte)clusterServiceLocator._WC_Handlers_Group.GROUPs[GroupID].Type, clusterServiceLocator._WC_Handlers_Group.GROUPs[GroupID].GetLeader().Guid, clusterServiceLocator._WC_Handlers_Group.GROUPs[GroupID].GetMembers());
                    }
                    catch (Exception)
                    {
                        clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.FAILED, "[G{0:00000}] Group update failed for [M{1:000}]", GroupID, w.Key);
                    }
                }
            }
        }

        public void GroupSendUpdateLoot(long GroupID)
        {
            clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.NETWORK, "[G{0:00000}] Group update loot", GroupID);
            lock (((ICollection)Worlds).SyncRoot)
            {
                foreach (KeyValuePair<uint, IWorld> w in Worlds)
                {
                    try
                    {
                        w.Value.GroupUpdateLoot(GroupID, (byte)clusterServiceLocator._WC_Handlers_Group.GROUPs[GroupID].DungeonDifficulty, (byte)clusterServiceLocator._WC_Handlers_Group.GROUPs[GroupID].LootMethod, (byte)clusterServiceLocator._WC_Handlers_Group.GROUPs[GroupID].LootThreshold, clusterServiceLocator._WC_Handlers_Group.GROUPs[GroupID].GetLootMaster().Guid);
                    }
                    catch (Exception)
                    {
                        clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.FAILED, "[G{0:00000}] Group update loot failed for [M{1:000}]", GroupID, w.Key);
                    }
                }
            }
        }
    }
}