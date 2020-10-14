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
using System.Runtime.CompilerServices;
using System.Threading;
using Mangos.Cluster.Globals;
using Mangos.Cluster.Handlers;
using Mangos.Common;
using Mangos.Common.Enums.Authentication;
using Mangos.Common.Enums.Chat;
using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using Mangos.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.Cluster.Server
{
    public class WC_Network
    {
        public WorldServerClass WorldServer;
        private readonly int LastPing = 0;

        public int MsTime()
        {
            // DONE: Calculate the clusters timeGetTime("")
            return ClusterServiceLocator._NativeMethods.timeGetTime("") - LastPing;
        }

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
                    m_Socket.Bind(new IPEndPoint(IPAddress.Parse(ClusterServiceLocator._WorldCluster.Config.WorldClusterAddress), ClusterServiceLocator._WorldCluster.Config.WorldClusterPort));
                    m_Socket.Listen(5);
                    m_Socket.BeginAccept(AcceptConnection, null);
                    ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.SUCCESS, "Listening on {0} on port {1}", IPAddress.Parse(ClusterServiceLocator._WorldCluster.Config.WorldClusterAddress), ClusterServiceLocator._WorldCluster.Config.WorldClusterPort);

                    // Creating ping timer
                    m_TimerPing = new Timer(Ping, null, 0, 15000);

                    // Creating stats timer
                    if (ClusterServiceLocator._WorldCluster.Config.StatsEnabled)
                    {
                        m_TimerStats = new Timer(ClusterServiceLocator._WC_Stats.GenerateStats, null, ClusterServiceLocator._WorldCluster.Config.StatsTimer, ClusterServiceLocator._WorldCluster.Config.StatsTimer);
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

                    ParentMap.InstanceCreate(MapID);
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

                    ParentMap.InstanceCreate(MapID);
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

        public class WorldInfo
        {
            public int Latency;
            public DateTime Started = DateAndTime.Now;
            public float CPUUsage;
            public ulong MemoryUsage;
        }

        public Dictionary<uint, DateTime> LastConnections = new Dictionary<uint, DateTime>();

        public class ClientClass : ClientInfo, IDisposable
        {
            public Socket Socket = null;
            public Queue Queue = new Queue();
            public WcHandlerCharacter.CharacterObject Character = null;
            public byte[] SS_Hash;
            public bool Encryption = false;
            protected byte[] SocketBuffer = new byte[8193];
            protected int SocketBytes;
            protected byte[] SavedBytes = Array.Empty<byte>();
            public bool DEBUG_CONNECTION = false;
            private readonly byte[] Key = new byte[] { 0, 0, 0, 0 };
            private bool HandingPackets = false;

            public ClientInfo GetClientInfo()
            {
                var ci = new ClientInfo()
                {
                    Access = Access,
                    Account = Account,
                    Index = Index,
                    IP = IP,
                    Port = Port
                };
                return ci;
            }

            public void OnConnect(object state)
            {
                if (Socket is null)
                    throw new ApplicationException("socket doesn't exist!");
                if (ClusterServiceLocator._WorldCluster.CLIENTs is null)
                    throw new ApplicationException("Clients doesn't exist!");
                IPEndPoint remoteEndPoint = (IPEndPoint)Socket.RemoteEndPoint;
                IP = remoteEndPoint.Address.ToString();
                Port = (uint)remoteEndPoint.Port;

                // DONE: Connection spam protection
                if (ClusterServiceLocator._WC_Network.LastConnections.ContainsKey(ClusterServiceLocator._WC_Network.Ip2Int(IP)))
                {
                    if (DateAndTime.Now > ClusterServiceLocator._WC_Network.LastConnections[ClusterServiceLocator._WC_Network.Ip2Int(IP)])
                    {
                        ClusterServiceLocator._WC_Network.LastConnections[ClusterServiceLocator._WC_Network.Ip2Int(IP)] = DateAndTime.Now.AddSeconds(5d);
                    }
                    else
                    {
                        Socket.Close();
                        Dispose();
                        return;
                    }
                }
                else
                {
                    ClusterServiceLocator._WC_Network.LastConnections.Add(ClusterServiceLocator._WC_Network.Ip2Int(IP), DateAndTime.Now.AddSeconds(5d));
                }

                ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "Incoming connection from [{0}:{1}]", IP, Port);
                Socket.BeginReceive(SocketBuffer, 0, SocketBuffer.Length, SocketFlags.None, OnData, null);

                // Send Auth Challenge
                var p = new Packets.PacketClass(Opcodes.SMSG_AUTH_CHALLENGE);
                p.AddInt32((int)Index);
                Send(p);
                Index = (uint)Interlocked.Increment(ref ClusterServiceLocator._WorldCluster.CLIETNIDs);
                lock (((ICollection)ClusterServiceLocator._WorldCluster.CLIENTs).SyncRoot)
                    ClusterServiceLocator._WorldCluster.CLIENTs.Add(Index, this);
                ClusterServiceLocator._WC_Stats.ConnectionsIncrement();
            }

            public void OnData(IAsyncResult ar)
            {
                if (!Socket.Connected)
                    return;
                if (ClusterServiceLocator._WC_Network.WorldServer.m_flagStopListen)
                    return;
                if (ar is null)
                    throw new ApplicationException("Value ar is empty!");
                if (SocketBuffer is null)
                    throw new ApplicationException("SocketBuffer is empty!");
                if (Socket is null)
                    throw new ApplicationException("Socket is Null!");
                if (ClusterServiceLocator._WorldCluster.CLIENTs is null)
                    throw new ApplicationException("Clients doesn't exist!");
                if (Queue is null)
                    throw new ApplicationException("Queue is Null!");
                if (SavedBytes is null)
                    throw new ApplicationException("SavedBytes is empty!");
                try
                {
                    SocketBytes = Socket.EndReceive(ar);
                    if (SocketBytes == 0)
                    {
                        Dispose(Conversions.ToBoolean(SocketBytes));
                    }
                    else
                    {
                        Interlocked.Add(ref ClusterServiceLocator._WC_Stats.DataTransferIn, SocketBytes);
                        while (SocketBytes > 0)
                        {
                            if (SavedBytes.Length == 0)
                            {
                                if (Encryption)
                                    Decode(SocketBuffer);
                            }
                            else
                            {
                                SocketBuffer = ClusterServiceLocator._Functions.Concat(SavedBytes, SocketBuffer);
                                SavedBytes = (new byte[] { });
                            }

                            // Calculate Length from packet
                            int PacketLen = SocketBuffer[1] + SocketBuffer[0] * 256 + 2;
                            if (SocketBytes < PacketLen)
                            {
                                SavedBytes = new byte[SocketBytes];
                                try
                                {
                                    Array.Copy(SocketBuffer, 0, SavedBytes, 0, SocketBytes);
                                }
                                catch (Exception)
                                {
                                    Dispose(Conversions.ToBoolean(SocketBytes));
                                    Socket.Dispose();
                                    Socket.Close();
                                    ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.CRITICAL, "[{0}:{1}] BAD PACKET {2}({3}) bytes, ", IP, Port, SocketBytes, PacketLen);
                                }

                                break;
                            }

                            // Move packet to Data
                            var data = new byte[PacketLen];
                            Array.Copy(SocketBuffer, data, PacketLen);

                            // Create packet and add it to queue
                            var p = new Packets.PacketClass(data);
                            lock (Queue.SyncRoot)
                                Queue.Enqueue(p);
                            try
                            {
                                // Delete packet from buffer
                                SocketBytes -= PacketLen;
                                Array.Copy(SocketBuffer, PacketLen, SocketBuffer, 0, SocketBytes);
                            }
                            catch (Exception)
                            {
                                ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.CRITICAL, "[{0}:{1}] Could not delete packet from buffer! {2}({3}{4}) bytes, ", IP, Port, SocketBuffer, PacketLen, SocketBytes);
                            }
                        }

                        if (SocketBuffer.Length > 0)
                        {
                            try
                            {
                                SocketError argerrorCode = (SocketError)SocketFlags.None;
                                Socket.BeginReceive(SocketBuffer, 0, SocketBuffer.Length, (SocketFlags)SocketBytes, out argerrorCode, OnData, null);
                                if (HandingPackets == false)
                                    ThreadPool.QueueUserWorkItem(OnPacket);
                            }
                            catch (Exception)
                            {
                                ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.WARNING, "Packet Disconnect from [{0}:{1}] caused an error {2}{3}", IP, Port, Information.Err().ToString(), Constants.vbCrLf);
                            }
                        }
                    }
                }
                catch (Exception Err)
                {
                    // NOTE: If it's a error here it means the connection is closed?
                    ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.WARNING, "Connection from [{0}:{1}] caused an error {2}{3}", IP, Port, Err.ToString(), Constants.vbCrLf);
                    Dispose(Conversions.ToBoolean(SocketBuffer.Length));
                    Dispose(HandingPackets);
                }
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void OnPacket(object state)
            {
                if (SocketBuffer is null)
                    throw new ApplicationException("SocketBuffer is empty!");
                if (Socket is null)
                    throw new ApplicationException("Socket is Null!");
                if (ClusterServiceLocator._WorldCluster.CLIENTs is null)
                    throw new ApplicationException("Clients doesn't exist!");
                if (Queue is null)
                    throw new ApplicationException("Queue is Null!");
                if (SavedBytes is null)
                    throw new ApplicationException("SavedBytes is empty!");
                if (ClusterServiceLocator._WorldCluster.PacketHandlers is null)
                    throw new ApplicationException("PacketHandler is empty!");
                try
                {
                }
                catch (Exception)
                {
                    HandingPackets = true;
                    ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.FAILED, "Handing Packets Failed: {0}", HandingPackets);
                }

                while (Queue.Count > 0)
                {
                    Packets.PacketClass p;
                    lock (Queue.SyncRoot)
                        p = (Packets.PacketClass)Queue.Dequeue();
                    if (ClusterServiceLocator._WorldCluster.Config.PacketLogging)
                    {
                        var argclient = this;
                        ClusterServiceLocator._Packets.LogPacket(p.Data, false, argclient);
                    }

                    if (ClusterServiceLocator._WorldCluster.PacketHandlers.ContainsKey(p.OpCode) != true)
                    {
                        if (Character is null || Character.IsInWorld == false)
                        {
                            Socket.Dispose();
                            Socket.Close();
                            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.WARNING, "[{0}:{1}] Unknown Opcode 0x{2:X} [{2}], DataLen={4}", IP, Port, p.OpCode, Constants.vbCrLf, p.Length);
                            var argclient1 = this;
                            ClusterServiceLocator._Packets.DumpPacket(p.Data, argclient1);
                        }
                        else
                        {
                            try
                            {
                                Character.GetWorld.ClientPacket(Index, p.Data);
                            }
                            catch
                            {
                                ClusterServiceLocator._WC_Network.WorldServer.Disconnect("NULL", new List<uint>() { Character.Map });
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            var argclient2 = this;
                            ClusterServiceLocator._WorldCluster.PacketHandlers[p.OpCode].Invoke(p, argclient2);
                        }
                        catch (Exception e)
                        {
                            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.FAILED, "Opcode handler {2}:{2:X} caused an error: {1}{0}", e.ToString(), Constants.vbCrLf, p.OpCode);
                        }
                    }

                    try
                    {
                    }
                    catch (Exception)
                    {
                        if (Queue.Count == 0)
                            p.Dispose();
                        ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.WARNING, "Unable to dispose of packet: {0}", p.OpCode);
                        var argclient = this;
                        ClusterServiceLocator._Packets.DumpPacket(p.Data, argclient);
                    }
                }

                HandingPackets = false;
            }

            public void Send(byte[] data)
            {
                if (!Socket.Connected)
                    return;
                try
                {
                    if (ClusterServiceLocator._WorldCluster.Config.PacketLogging)
                    {
                        var argclient = this;
                        ClusterServiceLocator._Packets.LogPacket(data, true, argclient);
                    }

                    if (Encryption)
                        Encode(data);
                    Socket.BeginSend(data, 0, data.Length, SocketFlags.None, OnSendComplete, null);
                }
                catch (Exception Err)
                {
                    // NOTE: If it's a error here it means the connection is closed?
                    ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.CRITICAL, "Connection from [{0}:{1}] caused an error {2}{3}", IP, Port, Err.ToString(), Constants.vbCrLf);
                    Delete();
                }
            }

            public void Send(Packets.PacketClass packet)
            {
                if (Information.IsNothing(packet))
                    throw new ApplicationException("Packet doesn't contain data!");
                if (Information.IsNothing(Socket) | Socket.Connected == false)
                    return;
                try
                {
                    var data = packet.Data;
                    if (ClusterServiceLocator._WorldCluster.Config.PacketLogging)
                    {
                        var argclient = this;
                        ClusterServiceLocator._Packets.LogPacket(data, true, argclient);
                    }

                    if (Encryption)
                        Encode(data);
                    Socket.BeginSend(data, 0, data.Length, SocketFlags.None, OnSendComplete, null);
                }
                catch (Exception err)
                {
                    // NOTE: If it's a error here it means the connection is closed?
                    ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.CRITICAL, "Connection from [{0}:{1}] caused an error {2}{3}", IP, Port, err.ToString(), Constants.vbCrLf);
                    Delete();
                }

                // Only attempt to dispose of the packet if it actually exists
                if (!Information.IsNothing(packet))
                    packet.Dispose();
            }

            public void SendMultiplyPackets(Packets.PacketClass packet)
            {
                if (packet is null)
                    throw new ApplicationException("Packet doesn't contain data!");
                if (!Socket.Connected)
                    return;
                try
                {
                    byte[] data = (byte[])packet.Data.Clone();
                    if (ClusterServiceLocator._WorldCluster.Config.PacketLogging)
                    {
                        var argclient = this;
                        ClusterServiceLocator._Packets.LogPacket(data, true, argclient);
                    }

                    if (Encryption)
                        Encode(data);
                    Socket.BeginSend(data, 0, data.Length, SocketFlags.None, OnSendComplete, null);
                }
                catch (Exception Err)
                {
                    // NOTE: If it's a error here it means the connection is closed?
                    ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.CRITICAL, "Connection from [{0}:{1}] caused an error {2}{3}", IP, Port, Err.ToString(), Constants.vbCrLf);
                    Delete();
                }

                // Don't forget to clean after using this function
            }

            public void OnSendComplete(IAsyncResult ar)
            {
                if (Socket is object)
                {
                    int bytesSent = Socket.EndSend(ar);
                    Interlocked.Add(ref ClusterServiceLocator._WC_Stats.DataTransferOut, bytesSent);
                }
            }

            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            private bool _disposedValue; // To detect redundant calls

            // IDisposable
            protected virtual void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                    // TODO: set large fields to null.

                    // On Error Resume Next
                    // May have to trap and use exception handler rather than the on error resume next rubbish

                    if (Socket is object)
                        Socket.Close();
                    Socket = null;
                    lock (((ICollection)ClusterServiceLocator._WorldCluster.CLIENTs).SyncRoot)
                        ClusterServiceLocator._WorldCluster.CLIENTs.Remove(Index);
                    if (Character is object)
                    {
                        if (Character.IsInWorld)
                        {
                            Character.IsInWorld = false;
                            Character.GetWorld.ClientDisconnect(Index);
                        }

                        Character.Dispose();
                    }

                    Character = null;
                    ClusterServiceLocator._WC_Stats.ConnectionsDecrement();
                }

                _disposedValue = true;
            }

            // This code added by Visual Basic to correctly implement the disposable pattern.
            public void Dispose()
            {
                // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
            public void Delete()
            {
                Socket.Close();
                Dispose();
            }

            public void Decode(byte[] data)
            {
                int tmp;
                for (int i = 0; i <= 6 - 1; i++)
                {
                    tmp = data[i];
                    data[i] = (byte)(SS_Hash[Key[1]] ^ (256 + data[i] - Key[0]) % 256);
                    Key[0] = (byte)tmp;
                    Key[1] = (byte)((Key[1] + 1) % 40);
                }
            }

            public void Encode(byte[] data)
            {
                for (int i = 0; i <= 4 - 1; i++)
                {
                    data[i] = (byte)(((SS_Hash[Key[3]] ^ data[i]) + Key[2]) % 256);
                    Key[2] = data[i];
                    Key[3] = (byte)((Key[3] + 1) % 40);
                }
            }

            public void EnQueue(object state)
            {
                while (ClusterServiceLocator._WorldCluster.CHARACTERs.Count > ClusterServiceLocator._WorldCluster.Config.ServerPlayerLimit)
                {
                    if (!Socket.Connected)
                        return;
                    new Packets.PacketClass(Opcodes.SMSG_AUTH_RESPONSE).AddInt8((byte)LoginResponse.LOGIN_WAIT_QUEUE);
                    new Packets.PacketClass(Opcodes.SMSG_AUTH_RESPONSE).AddInt32(ClusterServiceLocator._WorldCluster.CLIENTs.Count - ClusterServiceLocator._WorldCluster.CHARACTERs.Count);            // amount of players in queue
                    Send(new Packets.PacketClass(Opcodes.SMSG_AUTH_RESPONSE));
                    ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.INFORMATION, "[{1}:{2}] AUTH_WAIT_QUEUE: Server player limit reached!", IP, Port);
                    Thread.Sleep(6000);
                }

                var argclient = this;
                ClusterServiceLocator._WC_Handlers_Auth.SendLoginOk(argclient);
            }
        }

        private uint Ip2Int(string ip)
        {
            if (ip.Split(".").Length != 4)
                return 0U;
            try
            {
                var ipBytes = new byte[4];
                ipBytes[0] = Conversions.ToByte(ip.Split(".")[3]);
                ipBytes[1] = Conversions.ToByte(ip.Split(".")[2]);
                ipBytes[2] = Conversions.ToByte(ip.Split(".")[1]);
                ipBytes[3] = Conversions.ToByte(ip.Split(".")[0]);
                return BitConverter.ToUInt32(ipBytes, 0);
            }
            catch
            {
                return 0U;
            }
        }
    }
}