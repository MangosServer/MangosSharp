//
//  Copyright (C) 2013-2020 getMaNGOS <https:\\getmangos.eu>
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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Mangos.Cluster.Globals;
using Mangos.Cluster.Handlers;
using Mangos.Common;
using Mangos.Common.Enums.Chat;
using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using Mangos.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace Mangos.Cluster.Server
{
    public partial class WC_Network
    {
        public class WorldServerClass : Hub, ICluster, IDisposable
        {
            public bool m_flagStopListen = false;
            private readonly Timer m_TimerPing;
            private readonly Timer m_TimerStats;
            private readonly Timer m_TimerCPU;
            private readonly Socket m_Socket;

            public WorldServerClass()
            {
                try
                {
                    m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    m_Socket.Bind(new IPEndPoint(IPAddress.Parse(ClusterServiceLocator._WorldCluster.GetConfig().WorldClusterAddress), ClusterServiceLocator._WorldCluster.GetConfig().WorldClusterPort));
                    m_Socket.Listen(5);
                    m_Socket.BeginAccept(AcceptConnection, null);
                    ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.SUCCESS, "Listening on {0} on port {1}", IPAddress.Parse(ClusterServiceLocator._WorldCluster.GetConfig().WorldClusterAddress), ClusterServiceLocator._WorldCluster.GetConfig().WorldClusterPort);

                    // Creating ping timer
                    m_TimerPing = new Timer(Ping, null, 0, 15000);

                    // Creating stats timer
                    if (ClusterServiceLocator._WorldCluster.GetConfig().StatsEnabled)
                    {
                        m_TimerStats = new Timer(ClusterServiceLocator._WC_Stats.GenerateStats, null, ClusterServiceLocator._WorldCluster.GetConfig().StatsTimer, ClusterServiceLocator._WorldCluster.GetConfig().StatsTimer);
                    }

                    // Creating CPU check timer
                    m_TimerCPU = new Timer(ClusterServiceLocator._WC_Stats.CheckCpu, null, 1000, 1000);
                }
                catch (Exception e)
                {
                    Console.WriteLine();
                    ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.FAILED, "Error in {1}: {0}.", e.Message, e.Source);
                }
            }

            protected void AcceptConnection(IAsyncResult ar)
            {
                if (m_flagStopListen)
                    return;
                var m_Client = new ClientClass() { Socket = m_Socket.EndAccept(ar) };
                m_Client.Socket.NoDelay = true;
                m_Socket.BeginAccept(AcceptConnection, null);
                ThreadPool.QueueUserWorkItem(new WaitCallback(m_Client.OnConnect));
            }

            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            private readonly bool _disposedValue; // To detect redundant calls

            // This code added by Visual Basic to correctly implement the disposable pattern.
            public new void Dispose()
            {
                m_flagStopListen = true;
                m_Socket.Close();
                m_TimerPing.Dispose();
                m_TimerStats.Dispose();
                m_TimerCPU.Dispose();
                GC.SuppressFinalize(this);
            }
            /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
            public Dictionary<uint, IWorld> Worlds = new Dictionary<uint, IWorld>();
            public Dictionary<uint, WorldInfo> WorldsInfo = new Dictionary<uint, WorldInfo>();

            public bool Connect(string uri, List<uint> maps)
            {
                try
                {
                    Disconnect(uri, maps);
                    var WorldServerInfo = new WorldInfo();
                    ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "Connected Map Server: {0}", uri);
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
                    ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.CRITICAL, "Unable to reverse connect. [{0}]", ex.ToString());
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
                    lock (((ICollection)ClusterServiceLocator._WorldCluster.CLIENTs).SyncRoot)
                    {
                        foreach (KeyValuePair<uint, ClientClass> objCharacter in ClusterServiceLocator._WorldCluster.CLIENTs)
                        {
                            if (objCharacter.Value.Character is object && objCharacter.Value.Character.IsInWorld && objCharacter.Value.Character.Map == map)
                            {
                                objCharacter.Value.Send(new Packets.PacketClass(Opcodes.SMSG_LOGOUT_COMPLETE));
                                new Packets.PacketClass(Opcodes.SMSG_LOGOUT_COMPLETE).Dispose();
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
                                ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "Map: {0:000} has been disconnected!", map);
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
                    foreach (KeyValuePair<uint, IWorld> w in Worlds)
                    {
                        try
                        {
                            if (!SentPingTo.ContainsKey(WorldsInfo[w.Key]))
                            {
                                MyTime = ClusterServiceLocator._NativeMethods.timeGetTime("");
                                ServerTime = w.Value.Ping(MyTime, WorldsInfo[w.Key].Latency);
                                Latency = Math.Abs(MyTime - ClusterServiceLocator._NativeMethods.timeGetTime(""));
                                WorldsInfo[w.Key].Latency = Latency;
                                SentPingTo[WorldsInfo[w.Key]] = Latency;
                                ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.NETWORK, "Map {0:000} ping: {1}ms", w.Key, Latency);

                                // Query CPU and Memory usage
                                var serverInfo = w.Value.GetServerInfo();
                                WorldsInfo[w.Key].CPUUsage = serverInfo.cpuUsage;
                                WorldsInfo[w.Key].MemoryUsage = serverInfo.memoryUsage;
                            }
                            else
                            {
                                ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.NETWORK, "Map {0:000} ping: {1}ms", w.Key, SentPingTo[WorldsInfo[w.Key]]);
                            }
                        }
                        catch (Exception)
                        {
                            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.WARNING, "Map {0:000} is currently down!", w.Key);
                            DownedServers.Add(w.Key);
                        }
                    }
                }

                // Notification message
                if (Worlds.Count == 0)
                    ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.WARNING, "No maps are currently available!");

                // Drop WorldServers
                Disconnect("NULL", DownedServers);
            }

            public void ClientSend(uint id, byte[] data)
            {
                if (ClusterServiceLocator._WorldCluster.CLIENTs.ContainsKey(id))
                    ClusterServiceLocator._WorldCluster.CLIENTs[id].Send(data);
            }

            public void ClientDrop(uint ID)
            {
                if (ClusterServiceLocator._WorldCluster.CLIENTs.ContainsKey(ID))
                {
                    try
                    {
                        ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "[{0:000000}] Client has dropped map {1:000}", ID, ClusterServiceLocator._WorldCluster.CLIENTs[ID].Character.Map);
                        ClusterServiceLocator._WorldCluster.CLIENTs[ID].Character.IsInWorld = false;
                        ClusterServiceLocator._WorldCluster.CLIENTs[ID].Character.OnLogout();
                    }
                    catch (Exception ex)
                    {
                        ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "[{0:000000}] Client has dropped an exception: {1}", ID, ex.ToString());
                    }
                }
                else
                {
                    ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "[{0:000000}] Client connection has been lost.", ID);
                }
            }

            public void ClientTransfer(uint ID, float posX, float posY, float posZ, float ori, uint map)
            {
                ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "[{0:000000}] Client has transferred from map {1:000} to map {2:000}", ID, ClusterServiceLocator._WorldCluster.CLIENTs[ID].Character.Map, map);
                var p = new Packets.PacketClass(Opcodes.SMSG_NEW_WORLD);
                p.AddUInt32(map);
                p.AddSingle(posX);
                p.AddSingle(posY);
                p.AddSingle(posZ);
                p.AddSingle(ori);
                ClusterServiceLocator._WorldCluster.CLIENTs[ID].Send(p);
                ClusterServiceLocator._WorldCluster.CLIENTs[ID].Character.Map = map;
            }

            public void ClientUpdate(uint ID, uint zone, byte level)
            {
                if (ClusterServiceLocator._WorldCluster.CLIENTs[ID].Character is null)
                    return;
                ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "[{0:000000}] Client has an updated zone {1:000}", ID, zone);
                ClusterServiceLocator._WorldCluster.CLIENTs[ID].Character.Zone = zone;
                ClusterServiceLocator._WorldCluster.CLIENTs[ID].Character.Level = level;
            }

            public void ClientSetChatFlag(uint ID, byte flag)
            {
                if (ClusterServiceLocator._WorldCluster.CLIENTs[ID].Character is null)
                    return;
                ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0:000000}] Client chat flag update [0x{1:X}]", ID, flag);
                ClusterServiceLocator._WorldCluster.CLIENTs[ID].Character.ChatFlag = (ChatFlag)flag;
            }

            public byte[] ClientGetCryptKey(uint ID)
            {
                ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0:000000}] Requested client crypt key", ID);
                return ClusterServiceLocator._WorldCluster.CLIENTs[ID].SS_Hash;
            }

            public void Broadcast(byte[] Data)
            {
                byte[] b;
                ClusterServiceLocator._WorldCluster.CHARACTERs_Lock.AcquireReaderLock(ClusterServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                foreach (KeyValuePair<ulong, WcHandlerCharacter.CharacterObject> objCharacter in ClusterServiceLocator._WorldCluster.CHARACTERs)
                {
                    if (objCharacter.Value.IsInWorld && objCharacter.Value.Client is object)
                    {
                        b = (byte[])Data.Clone();
                        objCharacter.Value.Client.Send(Data);
                    }
                }

                ClusterServiceLocator._WorldCluster.CHARACTERs_Lock.ReleaseReaderLock();
            }

            public void BroadcastGroup(long groupId, byte[] Data)
            {
                {
                    var withBlock = ClusterServiceLocator._WC_Handlers_Group.GROUPs[groupId];
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
                    var withBlock = ClusterServiceLocator._WC_Handlers_Group.GROUPs[GroupID];
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
                if (!ClusterServiceLocator._WC_Network.WorldServer.Worlds.ContainsKey(MapID))
                {
                    // We don't create new continents
                    if (ClusterServiceLocator._Functions.IsContinentMap((int)MapID))
                    {
                        ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.WARNING, "[{0:000000}] Requested Instance Map [{1}] is a continent", client.Index, MapID);
                        client.Send(new Packets.PacketClass(Opcodes.SMSG_LOGOUT_COMPLETE));
                        new Packets.PacketClass(Opcodes.SMSG_LOGOUT_COMPLETE).Dispose();
                        client.Character.IsInWorld = false;
                        return false;
                    }

                    ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "[{0:000000}] Requesting Instance Map [{1}]", client.Index, MapID);
                    IWorld ParentMap = default;
                    WorldInfo ParentMapInfo = null;

                    // Check if we got parent map
                    if (ClusterServiceLocator._WC_Network.WorldServer.Worlds.ContainsKey((uint)ClusterServiceLocator._WS_DBCDatabase.Maps[(int)MapID].ParentMap) && ClusterServiceLocator._WC_Network.WorldServer.Worlds[(uint)ClusterServiceLocator._WS_DBCDatabase.Maps[(int)MapID].ParentMap].InstanceCanCreate((int)ClusterServiceLocator._WS_DBCDatabase.Maps[(int)MapID].Type))
                    {
                        ParentMap = ClusterServiceLocator._WC_Network.WorldServer.Worlds[(uint)ClusterServiceLocator._WS_DBCDatabase.Maps[(int)MapID].ParentMap];
                        ParentMapInfo = ClusterServiceLocator._WC_Network.WorldServer.WorldsInfo[(uint)ClusterServiceLocator._WS_DBCDatabase.Maps[(int)MapID].ParentMap];
                    }
                    else if (ClusterServiceLocator._WC_Network.WorldServer.Worlds.ContainsKey(0U) && ClusterServiceLocator._WC_Network.WorldServer.Worlds[0U].InstanceCanCreate((int)ClusterServiceLocator._WS_DBCDatabase.Maps[(int)MapID].Type))
                    {
                        ParentMap = ClusterServiceLocator._WC_Network.WorldServer.Worlds[0U];
                        ParentMapInfo = ClusterServiceLocator._WC_Network.WorldServer.WorldsInfo[0U];
                    }
                    else if (ClusterServiceLocator._WC_Network.WorldServer.Worlds.ContainsKey(1U) && ClusterServiceLocator._WC_Network.WorldServer.Worlds[1U].InstanceCanCreate((int)ClusterServiceLocator._WS_DBCDatabase.Maps[(int)MapID].Type))
                    {
                        ParentMap = ClusterServiceLocator._WC_Network.WorldServer.Worlds[1U];
                        ParentMapInfo = ClusterServiceLocator._WC_Network.WorldServer.WorldsInfo[1U];
                    }

                    if (ParentMap is null)
                    {
                        ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.WARNING, "[{0:000000}] Requested Instance Map [{1}] can't be loaded", client.Index, MapID);
                        client.Send(new Packets.PacketClass(Opcodes.SMSG_LOGOUT_COMPLETE));
                        new Packets.PacketClass(Opcodes.SMSG_LOGOUT_COMPLETE).Dispose();
                        client.Character.IsInWorld = false;
                        return false;
                    }

                    ParentMap.InstanceCreateAsync(MapID).Wait();
                    ClusterServiceLocator._WC_Network.WorldServer.Worlds.Add(MapID, ParentMap);
                    ClusterServiceLocator._WC_Network.WorldServer.WorldsInfo.Add(MapID, ParentMapInfo);
                    return true;
                }
                else
                {
                    return true;
                }
            }

            public bool BattlefieldCheck(uint MapID)
            {
                // Create map
                if (!ClusterServiceLocator._WC_Network.WorldServer.Worlds.ContainsKey(MapID))
                {
                    ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "[SERVER] Requesting battlefield map [{0}]", MapID);
                    IWorld ParentMap = default;
                    WorldInfo ParentMapInfo = null;

                    // Check if we got parent map
                    if (ClusterServiceLocator._WC_Network.WorldServer.Worlds.ContainsKey((uint)ClusterServiceLocator._WS_DBCDatabase.Maps[(int)MapID].ParentMap) && ClusterServiceLocator._WC_Network.WorldServer.Worlds[(uint)ClusterServiceLocator._WS_DBCDatabase.Maps[(int)MapID].ParentMap].InstanceCanCreate((int)ClusterServiceLocator._WS_DBCDatabase.Maps[(int)MapID].Type))
                    {
                        ParentMap = ClusterServiceLocator._WC_Network.WorldServer.Worlds[(uint)ClusterServiceLocator._WS_DBCDatabase.Maps[(int)MapID].ParentMap];
                        ParentMapInfo = ClusterServiceLocator._WC_Network.WorldServer.WorldsInfo[(uint)ClusterServiceLocator._WS_DBCDatabase.Maps[(int)MapID].ParentMap];
                    }
                    else if (ClusterServiceLocator._WC_Network.WorldServer.Worlds.ContainsKey(0U) && ClusterServiceLocator._WC_Network.WorldServer.Worlds[0U].InstanceCanCreate((int)ClusterServiceLocator._WS_DBCDatabase.Maps[(int)MapID].Type))
                    {
                        ParentMap = ClusterServiceLocator._WC_Network.WorldServer.Worlds[0U];
                        ParentMapInfo = ClusterServiceLocator._WC_Network.WorldServer.WorldsInfo[0U];
                    }
                    else if (ClusterServiceLocator._WC_Network.WorldServer.Worlds.ContainsKey(1U) && ClusterServiceLocator._WC_Network.WorldServer.Worlds[1U].InstanceCanCreate((int)ClusterServiceLocator._WS_DBCDatabase.Maps[(int)MapID].Type))
                    {
                        ParentMap = ClusterServiceLocator._WC_Network.WorldServer.Worlds[1U];
                        ParentMapInfo = ClusterServiceLocator._WC_Network.WorldServer.WorldsInfo[1U];
                    }

                    if (ParentMap is null)
                    {
                        ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.WARNING, "[SERVER] Requested battlefield map [{0}] can't be loaded", MapID);
                        return false;
                    }

                    ParentMap.InstanceCreateAsync(MapID).Wait();
                    ClusterServiceLocator._WC_Network.WorldServer.Worlds.Add(MapID, ParentMap);
                    ClusterServiceLocator._WC_Network.WorldServer.WorldsInfo.Add(MapID, ParentMapInfo);
                    return true;
                }
                else
                {
                    return true;
                }
            }

            public List<int> BattlefieldList(byte MapType)
            {
                var BattlefieldMap = new List<int>();
                ClusterServiceLocator._WC_Handlers_Battleground.BATTLEFIELDs_Lock.AcquireReaderLock(ClusterServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                foreach (KeyValuePair<int, WC_Handlers_Battleground.Battlefield> BG in ClusterServiceLocator._WC_Handlers_Battleground.BATTLEFIELDs)
                {
                    if ((byte)BG.Value.MapType == MapType)
                    {
                        BattlefieldMap.Add(BG.Value.ID);
                    }
                }

                ClusterServiceLocator._WC_Handlers_Battleground.BATTLEFIELDs_Lock.ReleaseReaderLock();
                return BattlefieldMap;
            }

            public void BattlefieldFinish(int battlefieldId)
            {
                ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "[B{0:0000}] Battlefield finished", battlefieldId);
            }

            public void GroupRequestUpdate(uint ID)
            {
                if (ClusterServiceLocator._WorldCluster.CLIENTs.ContainsKey(ID) && ClusterServiceLocator._WorldCluster.CLIENTs[ID].Character is object && ClusterServiceLocator._WorldCluster.CLIENTs[ID].Character.IsInWorld && ClusterServiceLocator._WorldCluster.CLIENTs[ID].Character.IsInGroup)
                {
                    ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.NETWORK, "[G{0:00000}] Group update request", ClusterServiceLocator._WorldCluster.CLIENTs[ID].Character.Group.Id);
                    try
                    {
                        ClusterServiceLocator._WorldCluster.CLIENTs[ID].Character.GetWorld.GroupUpdate(ClusterServiceLocator._WorldCluster.CLIENTs[ID].Character.Group.Id, (byte)ClusterServiceLocator._WorldCluster.CLIENTs[ID].Character.Group.Type, ClusterServiceLocator._WorldCluster.CLIENTs[ID].Character.Group.GetLeader().Guid, ClusterServiceLocator._WorldCluster.CLIENTs[ID].Character.Group.GetMembers());
                        ClusterServiceLocator._WorldCluster.CLIENTs[ID].Character.GetWorld.GroupUpdateLoot(ClusterServiceLocator._WorldCluster.CLIENTs[ID].Character.Group.Id, (byte)ClusterServiceLocator._WorldCluster.CLIENTs[ID].Character.Group.DungeonDifficulty, (byte)ClusterServiceLocator._WorldCluster.CLIENTs[ID].Character.Group.LootMethod, (byte)ClusterServiceLocator._WorldCluster.CLIENTs[ID].Character.Group.LootThreshold, ClusterServiceLocator._WorldCluster.CLIENTs[ID].Character.Group.GetLootMaster().Guid);
                    }
                    catch
                    {
                        ClusterServiceLocator._WC_Network.WorldServer.Disconnect("NULL", new List<uint>() { ClusterServiceLocator._WorldCluster.CLIENTs[ID].Character.Map });
                    }
                }
            }

            public void GroupSendUpdate(long GroupID)
            {
                ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.NETWORK, "[G{0:00000}] Group update", GroupID);
                lock (((ICollection)Worlds).SyncRoot)
                {
                    foreach (KeyValuePair<uint, IWorld> w in Worlds)
                    {
                        try
                        {
                            w.Value.GroupUpdate(GroupID, (byte)ClusterServiceLocator._WC_Handlers_Group.GROUPs[GroupID].Type, ClusterServiceLocator._WC_Handlers_Group.GROUPs[GroupID].GetLeader().Guid, ClusterServiceLocator._WC_Handlers_Group.GROUPs[GroupID].GetMembers());
                        }
                        catch (Exception)
                        {
                            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.FAILED, "[G{0:00000}] Group update failed for [M{1:000}]", GroupID, w.Key);
                        }
                    }
                }
            }

            public void GroupSendUpdateLoot(long GroupID)
            {
                ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.NETWORK, "[G{0:00000}] Group update loot", GroupID);
                lock (((ICollection)Worlds).SyncRoot)
                {
                    foreach (KeyValuePair<uint, IWorld> w in Worlds)
                    {
                        try
                        {
                            w.Value.GroupUpdateLoot(GroupID, (byte)ClusterServiceLocator._WC_Handlers_Group.GROUPs[GroupID].DungeonDifficulty, (byte)ClusterServiceLocator._WC_Handlers_Group.GROUPs[GroupID].LootMethod, (byte)ClusterServiceLocator._WC_Handlers_Group.GROUPs[GroupID].LootThreshold, ClusterServiceLocator._WC_Handlers_Group.GROUPs[GroupID].GetLootMaster().Guid);
                        }
                        catch (Exception)
                        {
                            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.FAILED, "[G{0:00000}] Group update loot failed for [M{1:000}]", GroupID, w.Key);
                        }
                    }
                }
            }
        }
    }
}