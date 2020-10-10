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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Mangos.Common;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Group;
using Mangos.Common.Enums.Map;
using Mangos.Common.Globals;
using Mangos.SignalR;
using Mangos.World.Globals;
using Mangos.World.Maps;
using Mangos.World.Player;
using Mangos.World.Social;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Server
{
    public class WS_Network
    {
        private int LastPing = 0;
        public int WC_MsTime = 0;

        public int MsTime()
        {
            // DONE: Calculate the clusters _NativeMethods.timeGetTime("")
            return WC_MsTime + (WorldServiceLocator._NativeMethods.timeGetTime("") - LastPing);
        }

        public class WorldServerClass : Hub, IWorld, IDisposable
        {
            public bool _flagStopListen = false;
            public string LocalURI;
            private readonly string m_RemoteURI;
            private readonly Timer m_Connection;
            private readonly Timer m_TimerCPU;
            private DateTime LastInfo;
            private double LastCPUTime = 0.0d;
            private float UsageCPU = 0.0f;
            public ICluster Cluster = default;

            public WorldServerClass()
            {
                m_RemoteURI = string.Format("http://{0}:{1}", WorldServiceLocator._WorldServer.Config.ClusterConnectHost, WorldServiceLocator._WorldServer.Config.ClusterConnectPort);
                LocalURI = string.Format("http://{0}:{1}", WorldServiceLocator._WorldServer.Config.LocalConnectHost, WorldServiceLocator._WorldServer.Config.LocalConnectPort);
                Cluster = null;

                // Creating connection timer
                WorldServiceLocator._WS_Network.LastPing = WorldServiceLocator._NativeMethods.timeGetTime("");
                m_Connection = new Timer(CheckConnection, null, 10000, 10000);

                // Creating CPU check timer
                m_TimerCPU = new Timer(CheckCPU, null, 1000, 1000);
            }

            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            private bool _disposedValue; // To detect redundant calls

            // IDisposable
            protected virtual new void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                    // TODO: set large fields to null.
                    ClusterDisconnect();
                    _flagStopListen = true;
                    m_TimerCPU.Dispose();
                    m_Connection.Dispose();
                }

                _disposedValue = true;
            }

            // This code added by Visual Basic to correctly implement the disposable pattern.
            public new void Dispose()
            {
                // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
            public void ClusterConnect()
            {
                while (Cluster is null)
                {
                    try
                    {
                        // NOTE: Password protected remoting
                        Cluster = ProxyClient.Create<ICluster>(m_RemoteURI);

                        // NOTE: Not protected remoting
                        // Cluster = RemotingServices.Connect(GetType(ICluster), m_RemoteURI)
                        if (!Information.IsNothing(Cluster))
                        {
                            if (Cluster.Connect(LocalURI, WorldServiceLocator._WorldServer.Config.Maps.Select(x => Conversions.ToUInteger(x)).ToList()))
                                break;
                            Cluster.Disconnect(LocalURI, WorldServiceLocator._WorldServer.Config.Maps.Select(x => Conversions.ToUInteger(x)).ToList());
                        }
                    }
                    catch (Exception e)
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Unable to connect to cluster. [{0}]", e.Message);
                    }

                    Cluster = null;
                    Thread.Sleep(3000);
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.SUCCESS, "Contacted cluster [{0}]", m_RemoteURI);
            }

            public void ClusterDisconnect()
            {
                try
                {
                    Cluster.Disconnect(LocalURI, WorldServiceLocator._WorldServer.Config.Maps.Select(x => Conversions.ToUInteger(x)).ToList());
                }
                catch
                {
                }
                finally
                {
                    Cluster = null;
                }
            }

            public void ClientTransfer(uint ID, float posX, float posY, float posZ, float ori, uint map)
            {
                if (!WorldServiceLocator._WS_Maps.Maps.ContainsKey(map))
                {
                    WorldServiceLocator._WorldServer.CLIENTs[ID].Character.Dispose();
                    WorldServiceLocator._WorldServer.CLIENTs[ID].Delete();
                }

                Cluster.ClientTransfer(ID, posX, posY, posZ, ori, map);
            }

            public void ClientConnect(uint id, ClientInfo client)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[{0:000000}] Client connected", id);
                if (client is null)
                    throw new ApplicationException("Client doesn't exist!");
                var objCharacter = new ClientClass(client);
                if (WorldServiceLocator._WorldServer.CLIENTs.ContainsKey(id) == true)  // Ooops, the character is already loaded, remove it
                {
                    WorldServiceLocator._WorldServer.CLIENTs.Remove(id);
                }

                WorldServiceLocator._WorldServer.CLIENTs.Add(id, objCharacter);
            }

            public void ClientDisconnect(uint id)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[{0:000000}] Client disconnected", id);
                if (WorldServiceLocator._WorldServer.CLIENTs[id].Character is object)
                {
                    WorldServiceLocator._WorldServer.CLIENTs[id].Character.Save();
                }

                WorldServiceLocator._WorldServer.CLIENTs[id].Delete();
                WorldServiceLocator._WorldServer.CLIENTs.Remove(id);
            }

            public void ClientLogin(uint id, ulong guid)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[{0:000000}] Client login [0x{1:X}]", id, guid);
                try
                {
                    var client = WorldServiceLocator._WorldServer.CLIENTs[id];
                    var Character = new WS_PlayerData.CharacterObject(ref client, guid);
                    WorldServiceLocator._WorldServer.CHARACTERs_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                    WorldServiceLocator._WorldServer.CHARACTERs[guid] = Character;
                    WorldServiceLocator._WorldServer.CHARACTERs_Lock.ReleaseWriterLock();

                    // DONE: SMSG_CORPSE_RECLAIM_DELAY
                    WorldServiceLocator._Functions.SendCorpseReclaimDelay(ref client, ref Character);

                    // DONE: Cast talents and racial passive spells
                    WorldServiceLocator._WS_PlayerHelper.InitializeTalentSpells(Character);
                    Character.Login();
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.USER, "[{0}:{1}] Player login complete [0x{2:X}]", client.IP, client.Port, guid);
                }
                catch (Exception e)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error on login: {0}", e.ToString());
                }
            }

            public void ClientLogout(uint id)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[{0:000000}] Client logout", id);
                WorldServiceLocator._WorldServer.CLIENTs[id].Character.Logout(null);
            }

            public void ClientPacket(uint id, byte[] data)
            {
                if (data is null)
                    throw new ApplicationException("Packet doesn't contain data!");
                var p = new Packets.PacketClass(ref data);
                try
                {
                    if (WorldServiceLocator._WorldServer.CLIENTs.ContainsKey(id) == false)
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Client ID doesn't contain a key!: {0}", ToString());
                    WorldServiceLocator._WorldServer.CLIENTs[id].Packets.Enqueue(p);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(WorldServiceLocator._WorldServer.CLIENTs[id].OnPacket));
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error on Client OnPacket: {0}", ex.ToString());
                }
                finally
                {
                    p.Dispose();
                    // _WorldServer.CLIENTs(id).Dispose()
                }
            }

            public int ClientCreateCharacter(string account, string name, byte race, byte classe, byte gender, byte skin, byte face, byte hairStyle, byte hairColor, byte facialHair, byte outfitId)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Account {0} Created a character with Name {1}, Race {2}, Class {3}, Gender {4}, Skin {5}, Face {6}, HairStyle {7}, HairColor {8}, FacialHair {9}, outfitID {10}", account, name, race, classe, gender, skin, face, hairStyle, hairColor, facialHair, outfitId);
                return WorldServiceLocator._WS_Player_Creation.CreateCharacter(account, name, race, classe, gender, skin, face, hairStyle, hairColor, facialHair, outfitId);
            }

            public int Ping(int timestamp, int latency)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Cluster ping: [{0}ms]", WorldServiceLocator._NativeMethods.timeGetTime("") - timestamp);
                WorldServiceLocator._WS_Network.LastPing = WorldServiceLocator._NativeMethods.timeGetTime("");
                WorldServiceLocator._WS_Network.WC_MsTime = timestamp + latency;
                return WorldServiceLocator._NativeMethods.timeGetTime("");
            }

            public void CheckConnection(object State)
            {
                if (WorldServiceLocator._NativeMethods.timeGetTime("") - WorldServiceLocator._WS_Network.LastPing > 40000)
                {
                    if (Cluster is object)
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Cluster timed out. Reconnecting");
                        ClusterDisconnect();
                    }

                    ClusterConnect();
                    WorldServiceLocator._WS_Network.LastPing = WorldServiceLocator._NativeMethods.timeGetTime("");
                }
            }

            public void CheckCPU(object State)
            {
                var TimeSinceLastCheck = DateAndTime.Now.Subtract(LastInfo);
                UsageCPU = (float)((Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds - LastCPUTime) / TimeSinceLastCheck.TotalMilliseconds * 100d);
                LastInfo = DateAndTime.Now;
                LastCPUTime = Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds;
            }

            public ServerInfo GetServerInfo()
            {
                var serverInfo = new ServerInfo()
                {
                    cpuUsage = UsageCPU,
                    memoryUsage = (ulong)(Process.GetCurrentProcess().WorkingSet64 / (double)(1024 * 1024))
                };
                return serverInfo;
            }

            public void InstanceCreate(uint MapID)
            {
                if (WorldServiceLocator._WS_Maps.Maps.ContainsKey(MapID) == false)
                {
                    var Map = new WS_Maps.TMap((int)MapID);
                    // The New does a an add to the .Containskey collection above
                }
            }

            public void InstanceDestroy(uint MapID)
            {
                WorldServiceLocator._WS_Maps.Maps[MapID].Dispose();
            }

            public bool InstanceCanCreate(int Type)
            {
                switch (Type)
                {
                    case (int)MapTypes.MAP_BATTLEGROUND:
                        {
                            return WorldServiceLocator._WorldServer.Config.CreateBattlegrounds;
                        }

                    case (int)MapTypes.MAP_INSTANCE:
                        {
                            return WorldServiceLocator._WorldServer.Config.CreatePartyInstances;
                        }

                    case (int)MapTypes.MAP_RAID:
                        {
                            return WorldServiceLocator._WorldServer.Config.CreateRaidInstances;
                        }

                    case (int)MapTypes.MAP_COMMON:
                        {
                            return WorldServiceLocator._WorldServer.Config.CreateOther;
                        }
                }

                return false;
            }

            public void ClientSetGroup(uint ID, long GroupID)
            {
                if (!WorldServiceLocator._WorldServer.CLIENTs.ContainsKey(ID))
                    return;
                if (GroupID == -1)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[{0:000000}] Client group set [G NULL]", ID);
                    WorldServiceLocator._WorldServer.CLIENTs[ID].Character.Group = null;
                    WorldServiceLocator._WS_Handlers_Instance.InstanceMapLeave(WorldServiceLocator._WorldServer.CLIENTs[ID].Character);
                }
                else
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[{0:000000}] Client group set [G{1:00000}]", ID, GroupID);
                    if (!WorldServiceLocator._WS_Group.Groups.ContainsKey(GroupID))
                    {
                        var Group = new WS_Group.Group(GroupID);
                        // The New does a an add to the .Containskey collection above
                        // Groups.Add(GroupID, Group)
                        Cluster.GroupRequestUpdate(ID);
                    }

                    WorldServiceLocator._WorldServer.CLIENTs[ID].Character.Group = WorldServiceLocator._WS_Group.Groups[GroupID];
                    WorldServiceLocator._WS_Handlers_Instance.InstanceMapEnter(WorldServiceLocator._WorldServer.CLIENTs[ID].Character);
                }
            }

            public void GroupUpdate(long GroupID, byte GroupType, ulong GroupLeader, ulong[] Members)
            {
                if (WorldServiceLocator._WS_Group.Groups.ContainsKey(GroupID))
                {
                    var list = new List<ulong>();
                    foreach (ulong GUID in Members)
                    {
                        if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(GUID))
                            list.Add(GUID);
                    }

                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[G{0:00000}] Group update [{2}, {1} local members]", GroupID, list.Count, (GroupType)GroupType);
                    if (list.Count == 0)
                    {
                        WorldServiceLocator._WS_Group.Groups[GroupID].Dispose();
                    }
                    else
                    {
                        WorldServiceLocator._WS_Group.Groups[GroupID].Type = (GroupType)GroupType;
                        WorldServiceLocator._WS_Group.Groups[GroupID].Leader = GroupLeader;
                        WorldServiceLocator._WS_Group.Groups[GroupID].LocalMembers = list;
                    }
                }
            }

            public void GroupUpdateLoot(long GroupID, byte Difficulty, byte Method, byte Threshold, ulong Master)
            {
                if (WorldServiceLocator._WS_Group.Groups.ContainsKey(GroupID))
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[G{0:00000}] Group update loot", GroupID);
                    WorldServiceLocator._WS_Group.Groups[GroupID].DungeonDifficulty = (GroupDungeonDifficulty)Difficulty;
                    WorldServiceLocator._WS_Group.Groups[GroupID].LootMethod = (GroupLootMethod)Method;
                    WorldServiceLocator._WS_Group.Groups[GroupID].LootThreshold = (GroupLootThreshold)Threshold;
                    if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(Master))
                    {
                        WorldServiceLocator._WS_Group.Groups[GroupID].LocalLootMaster = WorldServiceLocator._WorldServer.CHARACTERs[Master];
                    }
                    else
                    {
                        WorldServiceLocator._WS_Group.Groups[GroupID].LocalLootMaster = null;
                    }
                }
            }

            public byte[] GroupMemberStats(ulong GUID, int Flag)
            {
                if (Flag == 0)
                    Flag = (int)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FULL;
                var tmp = WorldServiceLocator._WorldServer.CHARACTERs;
                var argobjCharacter = tmp[GUID];
                var p = WorldServiceLocator._WS_Group.BuildPartyMemberStats(ref argobjCharacter, (uint)Flag);
                tmp[GUID] = argobjCharacter;
                p.UpdateLength();
                return p.Data;
            }

            public void GuildUpdate(ulong GUID, uint GuildID, byte GuildRank)
            {
                WorldServiceLocator._WorldServer.CHARACTERs[GUID].GuildID = GuildID;
                WorldServiceLocator._WorldServer.CHARACTERs[GUID].GuildRank = GuildRank;
                WorldServiceLocator._WorldServer.CHARACTERs[GUID].SetUpdateFlag((int)EPlayerFields.PLAYER_GUILDID, GuildID);
                WorldServiceLocator._WorldServer.CHARACTERs[GUID].SetUpdateFlag((int)EPlayerFields.PLAYER_GUILDRANK, GuildRank);
                WorldServiceLocator._WorldServer.CHARACTERs[GUID].SendCharacterUpdate();
            }

            public void BattlefieldCreate(int BattlefieldID, byte BattlefieldMapType, uint Map)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[B{0:0000}] Battlefield created", BattlefieldID);
            }

            public void BattlefieldDelete(int BattlefieldID)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[B{0:0000}] Battlefield deleted", BattlefieldID);
            }

            public void BattlefieldJoin(int BattlefieldID, ulong GUID)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[B{0:0000}] Character [0x{1:X}] joined battlefield", BattlefieldID, GUID);
            }

            public void BattlefieldLeave(int BattlefieldID, ulong GUID)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[B{0:0000}] Character [0x{1:X}] left battlefield", BattlefieldID, GUID);
            }
        }

        public class ClientClass : ClientInfo, IDisposable
        {
            public WS_PlayerData.CharacterObject Character;
            public Queue<Packets.PacketClass> Packets = new Queue<Packets.PacketClass>();
            public bool DEBUG_CONNECTION = false;

            /// <summary>
            /// Called when a packet is recieved.
            /// </summary>
            /// <param name="state">The state.</param>
            /// <returns></returns>
            public void OnPacket(object state)
            {
                while (Packets.Count >= 1)
                {
                    try // Trap a Packets.Dequeue issue when no packets are queued... possibly an error with the Packets.Count above'
                    {
                        var p = Packets.Dequeue();
                        int start = WorldServiceLocator._NativeMethods.timeGetTime("");
                        try
                        {
                            if (!Information.IsNothing(p))
                            {
                                if (WorldServiceLocator._WorldServer.PacketHandlers.ContainsKey(p.OpCode) == true)
                                {
                                    try
                                    {
                                        var argclient = this;
                                        WorldServiceLocator._WorldServer.PacketHandlers[p.OpCode].Invoke(ref p, ref argclient);
                                        if (WorldServiceLocator._NativeMethods.timeGetTime("") - start > 100)
                                        {
                                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Packet processing took too long: {0}, {1}ms", p.OpCode, WorldServiceLocator._NativeMethods.timeGetTime("") - start);
                                        }
                                    }
                                    catch (Exception e) // TargetInvocationException
                                    {
                                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Opcode handler {2}:{3} caused an error:{1}{0}", e.ToString(), Environment.NewLine, p.OpCode, p.OpCode);
                                        if (!Information.IsNothing(p))
                                        {
                                            var argclient = this;
                                            WorldServiceLocator._Packets.DumpPacket(p.Data, ref argclient);
                                        }
                                    }
                                }
                                else
                                {
                                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "[{0}:{1}] Unknown Opcode 0x{2:X} [DataLen={3} {4}]", IP, Port, Conversions.ToInteger(p.OpCode), p.Data.Length, p.OpCode);
                                    if (!Information.IsNothing(p))
                                    {
                                        var argclient1 = this;
                                        WorldServiceLocator._Packets.DumpPacket(p.Data, ref argclient1);
                                    }
                                }
                            }
                            else
                            {
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "[{0}:{1}] No Packet Information in Queue", IP, Port);
                                if (!Information.IsNothing(p))
                                {
                                    var argclient2 = this;
                                    WorldServiceLocator._Packets.DumpPacket(p.Data, ref argclient2);
                                }
                            }
                        }
                        catch (Exception err)
                        {
                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Connection from [{0}:{1}] cause error {2}{3}", IP, Port, err.ToString(), Environment.NewLine);
                            Delete();
                        }
                        finally
                        {
                            try
                            {
                            }
                            catch (Exception)
                            {
                                if (Packets.Count == 0)
                                    p.Dispose();
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Unable to dispose of packet: {0}", p.OpCode);
                                if (!Information.IsNothing(p))
                                {
                                    var argclient = this;
                                    WorldServiceLocator._Packets.DumpPacket(p.Data, ref argclient);
                                }
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Connection from [{0}:{1}] cause error {2}{3}", IP, Port, err.ToString(), Environment.NewLine);
                        Delete();
                    }
                    finally
                    {
                        try
                        {
                        }
                        catch (Exception)
                        {
                            // If Packets.Count = 0 Then p.Dispose()
                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Unable to dispose of packet");
                            // DumpPacket(p.Data, Me)
                        }
                    }
                }
            }

            public void Send(ref byte[] data)
            {
                lock (this)
                {
                    try
                    {
                        WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.ClientSend(Index, data);
                    }
                    catch (Exception Err)
                    {
                        if (DEBUG_CONNECTION)
                            return;
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Connection from [{0}:{1}] cause error {3}{2}", IP, Port, Err.ToString(), Environment.NewLine);
                        WorldServiceLocator._WorldServer.ClsWorldServer.Cluster = null;
                        Delete();
                    }
                }
            }

            public void Send(ref Packets.PacketClass packet)
            {
                if (packet is null)
                    throw new ApplicationException("Packet doesn't contain data!");
                lock (this)
                {
                    try
                    {
                        if (packet.OpCode == OPCODES.SMSG_UPDATE_OBJECT)
                            packet.CompressUpdatePacket();
                        packet.UpdateLength();

                        // Maybe attempt to reaquire cluster here
                        if (!Information.IsNothing(WorldServiceLocator._WorldServer.ClsWorldServer.Cluster))
                        {
                            WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.ClientSend(Index, packet.Data);
                        }

                        packet.Dispose();
                    }
                    catch (Exception Err)
                    {
                        if (DEBUG_CONNECTION)
                            return;
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Connection from [{0}:{1}] cause error {3}{2}", IP, Port, Err.ToString(), Environment.NewLine);
                        WorldServiceLocator._WorldServer.ClsWorldServer.Cluster = null;
                        Delete();
                    }
                }
            }

            public void SendMultiplyPackets(ref Packets.PacketClass packet)
            {
                if (packet is null)
                    throw new ApplicationException("Packet doesn't contain data!");
                lock (this)
                {
                    try
                    {
                        if (packet.OpCode == OPCODES.SMSG_UPDATE_OBJECT)
                            packet.CompressUpdatePacket();
                        packet.UpdateLength();
                        byte[] data = (byte[])packet.Data.Clone();
                        WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.ClientSend(Index, data);
                    }
                    catch (Exception Err)
                    {
                        if (DEBUG_CONNECTION)
                            return;
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Connection from [{0}:{1}] cause error {3}{2}", IP, Port, Err.ToString(), Environment.NewLine);
                        WorldServiceLocator._WorldServer.ClsWorldServer.Cluster = null;
                        Delete();
                    }
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
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "Connection from [{0}:{1}] disposed", IP, Port);
                    if (!Information.IsNothing(WorldServiceLocator._WorldServer.ClsWorldServer.Cluster))
                    {
                        WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.ClientDrop(Index);
                    }

                    WorldServiceLocator._WorldServer.CLIENTs.Remove(Index);
                    if (Character is object)
                    {
                        Character.client = null;
                        Character.Dispose();
                    }
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
                WorldServiceLocator._WorldServer.CLIENTs.Remove(Index);
                if (Character is object)
                {
                    Character.client = null;
                    Character.Dispose();
                }

                Dispose();
            }

            public void Disconnect()
            {
                Delete();
            }

            public ClientClass()
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Creating debug connection!", default);
                DEBUG_CONNECTION = true;
            }

            public ClientClass(ClientInfo ci)
            {
                Access = ci.Access;
                Account = ci.Account;
                Index = ci.Index;
                IP = ci.IP;
                Port = ci.Port;
            }
        }
    }
}