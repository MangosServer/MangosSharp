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

using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Group;
using Mangos.Common.Legacy;
using Mangos.DataStores;
using Mangos.SignalR;
using Mangos.World.Globals;
using Mangos.World.Maps;
using Mangos.World.Player;
using Mangos.World.Social;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mangos.World.Network;

public partial class WS_Network
{
    public class WorldServerClass : Hub, IWorld, IDisposable
    {
        private readonly DataStoreProvider dataStoreProvider;

        public bool _flagStopListen;

        public string LocalURI;

        private readonly string m_RemoteURI;

        private readonly Timer m_Connection;

        private readonly Timer m_TimerCPU;

        private DateTime LastInfo;

        private double LastCPUTime;

        private float UsageCPU;

        public ICluster Cluster;

        private bool _disposedValue;

        public WorldServerClass(DataStoreProvider dataStoreProvider)
        {
            _flagStopListen = false;
            LastCPUTime = 0.0;
            UsageCPU = 0f;
            Cluster = null;
            var configuration = WorldServiceLocator._ConfigurationProvider.GetConfiguration();
            m_RemoteURI = $"http://{configuration.ClusterConnectHost}:{configuration.ClusterConnectPort}";
            LocalURI = $"http://{configuration.LocalConnectHost}:{configuration.LocalConnectPort}";
            Cluster = null;
            WorldServiceLocator._WS_Network.LastPing = WorldServiceLocator._NativeMethods.timeGetTime("");
            m_Connection = new Timer(CheckConnection, null, 10000, 10000);
            m_TimerCPU = new Timer(CheckCPU, null, 1000, 1000);
            this.dataStoreProvider = dataStoreProvider;
        }

        protected new virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                ClusterDisconnect();
                _flagStopListen = true;
                m_TimerCPU.Dispose();
                m_Connection.Dispose();
            }
            _disposedValue = true;
        }

        public new void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        void IDisposable.Dispose()
        {
            //ILSpy generated this explicit interface implementation from .override directive in Dispose
            Dispose();
        }

        public void ClusterConnect()
        {
            while (Cluster == null)
            {
                try
                {
                    Cluster = ProxyClient.Create<ICluster>(m_RemoteURI);
                    if (Cluster != null)
                    {
                        var configuration = WorldServiceLocator._ConfigurationProvider.GetConfiguration();
                        if (Cluster.Connect(LocalURI, configuration.Maps.Select(x => Conversions.ToUInteger(x)).ToList()))
                        {
                            break;
                        }
                        Cluster.Disconnect(LocalURI, configuration.Maps.Select(x => Conversions.ToUInteger(x)).ToList());
                    }
                }
                catch (Exception ex)
                {
                    var e = ex;
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
                Cluster.Disconnect(LocalURI, WorldServiceLocator._ConfigurationProvider.GetConfiguration().Maps.Select(x => Conversions.ToUInteger(x)).ToList());
            }
            catch (Exception ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Cluster Disconnected [{0}]", ex);
            }
            finally
            {
                Cluster = null;
            }
        }

        public void ClientTransfer(uint ID, float posX, float posY, float posZ, float ori, int map)
        {
            checked
            {
                if (!WorldServiceLocator._WS_Maps.Maps.ContainsKey((uint)map))
                {
                    WorldServiceLocator._WorldServer.CLIENTs[ID].Character.Dispose();
                    WorldServiceLocator._WorldServer.CLIENTs[ID].Delete();
                }
                Cluster.ClientTransfer(ID, posX, posY, posZ, ori, (uint)map);
            }
        }

        public void ClientConnect(uint id, ClientInfo client)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[{0:000000}] Client connected", id);
            if (client == null)
            {
                throw new ApplicationException("Client doesn't exist!");
            }
            ClientClass objCharacter = new(client);
            if (WorldServiceLocator._WorldServer.CLIENTs.ContainsKey(id))
            {
                WorldServiceLocator._WorldServer.CLIENTs.Remove(id);
            }
            WorldServiceLocator._WorldServer.CLIENTs.Add(id, objCharacter);
        }

        void IWorld.ClientConnect(uint id, ClientInfo client)
        {
            //ILSpy generated this explicit interface implementation from .override directive in ClientConnect
            ClientConnect(id, client);
        }

        public void ClientDisconnect(uint id)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[{0:000000}] Client disconnected", id);
            if (WorldServiceLocator._WorldServer.CLIENTs[id].Character != null)
            {
                WorldServiceLocator._WorldServer.CLIENTs[id].Character.Save();
            }
            WorldServiceLocator._WorldServer.CLIENTs[id].Delete();
            WorldServiceLocator._WorldServer.CLIENTs.Remove(id);
        }

        void IWorld.ClientDisconnect(uint id)
        {
            //ILSpy generated this explicit interface implementation from .override directive in ClientDisconnect
            ClientDisconnect(id);
        }

        public void ClientLogin(uint id, ulong guid)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[{0:000000}] Client login [0x{1:X}]", id, guid);
            try
            {
                var client = WorldServiceLocator._WorldServer.CLIENTs[id];
                WS_PlayerData.CharacterObject Character = new(ref client, guid);
                WorldServiceLocator._WorldServer.CHARACTERs_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                WorldServiceLocator._WorldServer.CHARACTERs[guid] = Character;
                WorldServiceLocator._WorldServer.CHARACTERs_Lock.ReleaseWriterLock();
                WorldServiceLocator._Functions.SendCorpseReclaimDelay(ref client, ref Character);
                WorldServiceLocator._WS_PlayerHelper.InitializeTalentSpells(Character);
                Character.Login();
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.USER, "[{0}:{1}] Player login complete [0x{2:X}]", client.IP, client.Port, guid);
            }
            catch (Exception ex)
            {
                var e = ex;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error on login: {0}", e.ToString());
            }
        }

        void IWorld.ClientLogin(uint id, ulong guid)
        {
            //ILSpy generated this explicit interface implementation from .override directive in ClientLogin
            ClientLogin(id, guid);
        }

        public void ClientLogout(uint id)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[{0:000000}] Client logout", id);
            WorldServiceLocator._WorldServer.CLIENTs[id].Character.Logout();
        }

        void IWorld.ClientLogout(uint id)
        {
            //ILSpy generated this explicit interface implementation from .override directive in ClientLogout
            ClientLogout(id);
        }

        public void ClientPacket(uint id, byte[] data)
        {
            if (data == null)
            {
                throw new ApplicationException("Packet doesn't contain data!");
            }

            try
            {
                if (WorldServiceLocator._WorldServer.CLIENTs.TryGetValue(id, out var _client))
                {
                    Packets.PacketClass p = new(ref data);
                    _client?.PushPacket(p);
                }
                else
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Client ID doesn't contain a key!: {0}", ToString());
                }
            }
            catch (Exception ex2)
            {
                var ex = ex2;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error on Client OnPacket: {0}", ex.ToString());
            }
        }

        void IWorld.ClientPacket(uint id, byte[] data)
        {
            //ILSpy generated this explicit interface implementation from .override directive in ClientPacket
            ClientPacket(id, data);
        }

        public int ClientCreateCharacter(string account, string name, byte race, byte classe, byte gender, byte skin, byte face, byte hairStyle, byte hairColor, byte facialHair, byte outfitId)
        {
            if (string.IsNullOrEmpty(account))
            {
                throw new ArgumentException($"'{nameof(account)}' cannot be null or empty", nameof(account));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or empty", nameof(name));
            }

            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Account {0} Created a character with Name {1}, Race {2}, Class {3}, Gender {4}, Skin {5}, Face {6}, HairStyle {7}, HairColor {8}, FacialHair {9}, outfitID {10}", account, name, race, classe, gender, skin, face, hairStyle, hairColor, facialHair, outfitId);
            return WorldServiceLocator._WS_Player_Creation.CreateCharacter(account, name, race, classe, gender, skin, face, hairStyle, hairColor, facialHair, outfitId);
        }

        int IWorld.ClientCreateCharacter(string account, string name, byte race, byte classe, byte gender, byte skin, byte face, byte hairStyle, byte hairColor, byte facialHair, byte outfitId)
        {
            if (string.IsNullOrEmpty(account))
            {
                throw new ArgumentException($"'{nameof(account)}' cannot be null or empty", nameof(account));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or empty", nameof(name));
            }
            //ILSpy generated this explicit interface implementation from .override directive in ClientCreateCharacter
            return ClientCreateCharacter(account, name, race, classe, gender, skin, face, hairStyle, hairColor, facialHair, outfitId);
        }

        public int Ping(int timestamp, int latency)
        {
            checked
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Cluster ping: [{0}ms]", WorldServiceLocator._NativeMethods.timeGetTime("") - timestamp);
                WorldServiceLocator._WS_Network.LastPing = WorldServiceLocator._NativeMethods.timeGetTime("");
                WorldServiceLocator._WS_Network.WC_MsTime = timestamp + latency;
                return WorldServiceLocator._NativeMethods.timeGetTime("");
            }
        }

        int IWorld.Ping(int timestamp, int latency)
        {
            //ILSpy generated this explicit interface implementation from .override directive in Ping
            return Ping(timestamp, latency);
        }

        public void CheckConnection(object State)
        {
            if ((WorldServiceLocator._NativeMethods.timeGetTime("") - WorldServiceLocator._WS_Network.LastPing) > 40000)
            {
                if (Cluster != null)
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
            var TimeSinceLastCheck = DateTime.Now.Subtract(LastInfo);
            UsageCPU = (float)((Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds - LastCPUTime) / TimeSinceLastCheck.TotalMilliseconds * 100.0);
            LastInfo = DateTime.Now;
            LastCPUTime = Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds;
        }

        public ServerInfo GetServerInfo()
        {
            ServerInfo serverInfo = new()
            {
                cpuUsage = UsageCPU,
                memoryUsage = checked((ulong)Math.Round(Process.GetCurrentProcess().WorkingSet64 / 1048576.0))
            };
            return serverInfo;
        }

        ServerInfo IWorld.GetServerInfo()
        {
            //ILSpy generated this explicit interface implementation from .override directive in GetServerInfo
            return GetServerInfo();
        }

        public async Task InstanceCreateAsync(uint MapID)
        {
            if (!WorldServiceLocator._WS_Maps.Maps.ContainsKey(MapID))
            {
                WS_Maps.TMap Map = new(checked((int)MapID), await dataStoreProvider.GetDataStoreAsync("Map.dbc"));
            }
        }

        async Task IWorld.InstanceCreateAsync(uint MapID)
        {
            //ILSpy generated this explicit interface implementation from .override directive in InstanceCreate
            await InstanceCreateAsync(MapID).ConfigureAwait(false);
        }

        public void InstanceDestroy(uint MapID)
        {
            WorldServiceLocator._WS_Maps.Maps[MapID].Dispose();
        }

        void IWorld.InstanceDestroy(uint MapID)
        {
            //ILSpy generated this explicit interface implementation from .override directive in InstanceDestroy
            InstanceDestroy(MapID);
        }

        public bool InstanceCanCreate(int Type)
        {
            var configuration = WorldServiceLocator._ConfigurationProvider.GetConfiguration();
            return Type switch
            {
                3 => configuration.CreateBattlegrounds,
                1 => configuration.CreatePartyInstances,
                2 => configuration.CreateRaidInstances,
                0 => configuration.CreateOther,
                _ => false,
            };
        }

        bool IWorld.InstanceCanCreate(int Type)
        {
            //ILSpy generated this explicit interface implementation from .override directive in InstanceCanCreate
            return InstanceCanCreate(Type);
        }

        public void ClientSetGroup(uint ID, long GroupID)
        {
            if (!WorldServiceLocator._WorldServer.CLIENTs.ContainsKey(ID))
            {
                return;
            }
            if (GroupID == -1)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[{0:000000}] Client group set [G NULL]", ID);
                WorldServiceLocator._WorldServer.CLIENTs[ID].Character.Group = null;
                WorldServiceLocator._WS_Handlers_Instance.InstanceMapLeave(WorldServiceLocator._WorldServer.CLIENTs[ID].Character);
                return;
            }
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[{0:000000}] Client group set [G{1:00000}]", ID, GroupID);
            if (!WorldServiceLocator._WS_Group.Groups.ContainsKey(GroupID))
            {
                WS_Group.Group Group = new(GroupID);
                Cluster.GroupRequestUpdate(ID);
            }
            WorldServiceLocator._WorldServer.CLIENTs[ID].Character.Group = WorldServiceLocator._WS_Group.Groups[GroupID];
            WorldServiceLocator._WS_Handlers_Instance.InstanceMapEnter(WorldServiceLocator._WorldServer.CLIENTs[ID].Character);
        }

        void IWorld.ClientSetGroup(uint ID, long GroupID)
        {
            //ILSpy generated this explicit interface implementation from .override directive in ClientSetGroup
            ClientSetGroup(ID, GroupID);
        }

        public void GroupUpdate(long GroupID, byte GroupType, ulong GroupLeader, ulong[] Members)
        {
            if (!WorldServiceLocator._WS_Group.Groups.ContainsKey(GroupID))
            {
                return;
            }
            List<ulong> list = new();
            foreach (var GUID in Members)
            {
                if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(GUID))
                {
                    list.Add(GUID);
                }
            }
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[G{0:00000}] Group update [{2}, {1} local members]", GroupID, list.Count, (GroupType)GroupType);
            if (list.Count == 0)
            {
                WorldServiceLocator._WS_Group.Groups[GroupID].Dispose();
                return;
            }
            WorldServiceLocator._WS_Group.Groups[GroupID].Type = (GroupType)GroupType;
            WorldServiceLocator._WS_Group.Groups[GroupID].Leader = GroupLeader;
            WorldServiceLocator._WS_Group.Groups[GroupID].LocalMembers = list;
        }

        void IWorld.GroupUpdate(long GroupID, byte GroupType, ulong GroupLeader, ulong[] Members)
        {
            //ILSpy generated this explicit interface implementation from .override directive in GroupUpdate
            GroupUpdate(GroupID, GroupType, GroupLeader, Members);
        }

        public void GroupUpdateLoot(long GroupID, byte Difficulty, byte Method, byte Threshold, ulong Master)
        {
            if (WorldServiceLocator._WS_Group.Groups.ContainsKey(GroupID))
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[G{0:00000}] Group update loot", GroupID);
                WorldServiceLocator._WS_Group.Groups[GroupID].DungeonDifficulty = (GroupDungeonDifficulty)Difficulty;
                WorldServiceLocator._WS_Group.Groups[GroupID].LootMethod = (GroupLootMethod)Method;
                WorldServiceLocator._WS_Group.Groups[GroupID].LootThreshold = (GroupLootThreshold)Threshold;
                WorldServiceLocator._WS_Group.Groups[GroupID].LocalLootMaster = WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(Master) ? WorldServiceLocator._WorldServer.CHARACTERs[Master] : null;
            }
        }

        void IWorld.GroupUpdateLoot(long GroupID, byte Difficulty, byte Method, byte Threshold, ulong Master)
        {
            //ILSpy generated this explicit interface implementation from .override directive in GroupUpdateLoot
            GroupUpdateLoot(GroupID, Difficulty, Method, Threshold, Master);
        }

        public byte[] GroupMemberStats(ulong GUID, int Flag)
        {
            if (Flag == 0)
            {
                Flag = 1015;
            }
            var wS_Group = WorldServiceLocator._WS_Group;
            Dictionary<ulong, WS_PlayerData.CharacterObject> cHARACTERs;
            ulong key;
            var objCharacter = (cHARACTERs = WorldServiceLocator._WorldServer.CHARACTERs)[key = GUID];
            var packetClass = wS_Group.BuildPartyMemberStats(ref objCharacter, checked((uint)Flag));
            cHARACTERs[key] = objCharacter;
            var p = packetClass;
            p.UpdateLength();
            return p.Data;
        }

        byte[] IWorld.GroupMemberStats(ulong GUID, int Flag)
        {
            //ILSpy generated this explicit interface implementation from .override directive in GroupMemberStats
            return GroupMemberStats(GUID, Flag);
        }

        public void GuildUpdate(ulong GUID, uint GuildID, byte GuildRank)
        {
            WorldServiceLocator._WorldServer.CHARACTERs[GUID].GuildID = GuildID;
            WorldServiceLocator._WorldServer.CHARACTERs[GUID].GuildRank = GuildRank;
            WorldServiceLocator._WorldServer.CHARACTERs[GUID].SetUpdateFlag(191, GuildID);
            WorldServiceLocator._WorldServer.CHARACTERs[GUID].SetUpdateFlag(192, GuildRank);
            WorldServiceLocator._WorldServer.CHARACTERs[GUID].SendCharacterUpdate();
        }

        void IWorld.GuildUpdate(ulong GUID, uint GuildID, byte GuildRank)
        {
            //ILSpy generated this explicit interface implementation from .override directive in GuildUpdate
            GuildUpdate(GUID, GuildID, GuildRank);
        }

        public void BattlefieldCreate(int BattlefieldID, byte BattlefieldMapType, uint Map)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[B{0:0000}] Battlefield created", BattlefieldID);
        }

        void IWorld.BattlefieldCreate(int BattlefieldID, byte BattlefieldMapType, uint Map)
        {
            //ILSpy generated this explicit interface implementation from .override directive in BattlefieldCreate
            BattlefieldCreate(BattlefieldID, BattlefieldMapType, Map);
        }

        public void BattlefieldDelete(int BattlefieldID)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[B{0:0000}] Battlefield deleted", BattlefieldID);
        }

        void IWorld.BattlefieldDelete(int BattlefieldID)
        {
            //ILSpy generated this explicit interface implementation from .override directive in BattlefieldDelete
            BattlefieldDelete(BattlefieldID);
        }

        public void BattlefieldJoin(int BattlefieldID, ulong GUID)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[B{0:0000}] Character [0x{1:X}] joined battlefield", BattlefieldID, GUID);
        }

        void IWorld.BattlefieldJoin(int BattlefieldID, ulong GUID)
        {
            //ILSpy generated this explicit interface implementation from .override directive in BattlefieldJoin
            BattlefieldJoin(BattlefieldID, GUID);
        }

        public void BattlefieldLeave(int BattlefieldID, ulong GUID)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.NETWORK, "[B{0:0000}] Character [0x{1:X}] left battlefield", BattlefieldID, GUID);
        }

        void IWorld.BattlefieldLeave(int BattlefieldID, ulong GUID)
        {
            //ILSpy generated this explicit interface implementation from .override directive in BattlefieldLeave
            BattlefieldLeave(BattlefieldID, GUID);
        }
    }
}
