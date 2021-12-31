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

using Mangos.Cluster.Configuration;
using Mangos.Cluster.Globals;
using Mangos.Common.Enums.Chat;
using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using Mangos.Common.Legacy;
using Mangos.Configuration;
using Mangos.SignalR;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Mangos.Cluster.Network;

public class WorldServerClass : Hub, ICluster
{
    private readonly ClusterServiceLocator _clusterServiceLocator;
    private readonly IConfigurationProvider<ClusterConfiguration> configurationProvider;

    public bool MFlagStopListen;
    private Timer _mTimerPing;
    private Timer _mTimerStats;
    private Timer _mTimerCpu;

    public WorldServerClass(ClusterServiceLocator clusterServiceLocator,
        IConfigurationProvider<ClusterConfiguration> configurationProvider)
    {
        _clusterServiceLocator = clusterServiceLocator;
        this.configurationProvider = configurationProvider;
    }

    public void Start()
    {
        // Creating ping timer
        _mTimerPing = new Timer(Ping, null, 0, 15000);

        // Creating stats timer
        if (configurationProvider.GetConfiguration().StatsEnabled)
        {
            _mTimerStats = new Timer(
                _clusterServiceLocator.WcStats.GenerateStats,
                null,
                configurationProvider.GetConfiguration().StatsTimer,
                configurationProvider.GetConfiguration().StatsTimer);
        }

        // Creating CPU check timer
        _mTimerCpu = new Timer(_clusterServiceLocator.WcStats.CheckCpu, null, 1000, 1000);
    }

    public Dictionary<uint, IWorld> Worlds = new();
    public Dictionary<uint, WorldInfo> WorldsInfo = new();

    public bool Connect(string uri, List<uint> maps)
    {
        try
        {
            Disconnect(uri, maps);
            WorldInfo worldServerInfo = new();
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.INFORMATION, "Connected Map Server: {0}", uri);
            lock (((ICollection)Worlds).SyncRoot)
            {
                foreach (var map in maps)
                {
                    // NOTE: Password protected remoting
                    Worlds[map] = ProxyClient.Create<IWorld>(uri);
                    WorldsInfo[map] = worldServerInfo;
                }
            }
        }
        catch (Exception ex)
        {
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.CRITICAL, "Unable to reverse connect. [{0}]", ex.ToString());
            return false;
        }

        return true;
    }

    public void Disconnect(string uri, List<uint> maps)
    {
        if (maps.Count == 0)
        {
            return;
        }

        // TODO: Unload arenas or battlegrounds that is hosted on this server!
        foreach (var map in maps)
        {
            // DONE: Disconnecting clients
            lock (((ICollection)_clusterServiceLocator.WorldCluster.ClienTs).SyncRoot)
            {
                foreach (var objCharacter in _clusterServiceLocator.WorldCluster.ClienTs)
                {
                    if (objCharacter.Value.Character is not null && objCharacter.Value.Character.IsInWorld && objCharacter.Value.Character.Map == map)
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
                    _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.WARNING, "Map: {0:000} has thrown an Exception!", map);
                }
                finally
                {
                    lock (((ICollection)Worlds).SyncRoot)
                    {
                        Worlds.Remove(map);
                        WorldsInfo.Remove(map);
                        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.INFORMATION, "Map: {0:000} has been disconnected!", map);
                    }
                }
            }
        }
    }

    public void Ping(object state)
    {
        List<uint> downedServers = new();
        Dictionary<WorldInfo, int> sentPingTo = new();
        int myTime;
        int serverTime;
        int latency;

        // Ping WorldServers
        lock (((ICollection)Worlds).SyncRoot)
        {
            if (Worlds != null && WorldsInfo != null)
            {
                foreach (var w in Worlds)
                {
                    try
                    {
                        if (!sentPingTo.ContainsKey(WorldsInfo[w.Key]))
                        {
                            myTime = _clusterServiceLocator.NativeMethods.timeGetTime("");
                            serverTime = w.Value.Ping(myTime, WorldsInfo[w.Key].Latency);
                            latency = Math.Abs(myTime - _clusterServiceLocator.NativeMethods.timeGetTime(""));
                            WorldsInfo[w.Key].Latency = latency;
                            sentPingTo[WorldsInfo[w.Key]] = latency;
                            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.NETWORK, "Map {0:000} ping: {1}ms", w.Key, latency);

                            // Query CPU and Memory usage
                            var serverInfo = w.Value.GetServerInfo();
                            WorldsInfo[w.Key].CpuUsage = serverInfo.cpuUsage;
                            WorldsInfo[w.Key].MemoryUsage = serverInfo.memoryUsage;
                        }
                        else
                        {
                            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.NETWORK, "Map {0:000} ping: {1}ms", w.Key, sentPingTo[WorldsInfo[w.Key]]);
                        }
                    }
                    catch (Exception)
                    {
                        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.WARNING, "Map {0:000} is currently down!", w.Key);
                        downedServers.Add(w.Key);
                    }
                }
            }
        }

        // Notification message
        if (Worlds.Count == 0)
        {
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.WARNING, "No maps are currently available!");
        }

        // Drop WorldServers
        Disconnect("NULL", downedServers);
    }

    public void ClientSend(uint id, byte[] data)
    {
        if (_clusterServiceLocator.WorldCluster.ClienTs.ContainsKey(id))
        {
            _clusterServiceLocator.WorldCluster.ClienTs[id].Send(data);
        }
    }

    public void ClientDrop(uint id)
    {
        if (_clusterServiceLocator.WorldCluster.ClienTs.ContainsKey(id))
        {
            try
            {
                _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.INFORMATION, "[{0:000000}] Client has dropped map {1:000}", id, _clusterServiceLocator.WorldCluster.ClienTs[id].Character.Map);
                _clusterServiceLocator.WorldCluster.ClienTs[id].Character.IsInWorld = false;
                _clusterServiceLocator.WorldCluster.ClienTs[id].Character.OnLogout();
            }
            catch (Exception ex)
            {
                _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.INFORMATION, "[{0:000000}] Client has dropped an exception: {1}", id, ex.ToString());
            }
        }
        else
        {
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.INFORMATION, "[{0:000000}] Client connection has been lost.", id);
        }
    }

    public void ClientTransfer(uint id, float posX, float posY, float posZ, float ori, uint map)
    {
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.INFORMATION, "[{0:000000}] Client has transferred from map {1:000} to map {2:000}", id, _clusterServiceLocator.WorldCluster.ClienTs[id].Character.Map, map);
        PacketClass p = new(Opcodes.SMSG_NEW_WORLD);
        p.AddUInt32(map);
        p.AddSingle(posX);
        p.AddSingle(posY);
        p.AddSingle(posZ);
        p.AddSingle(ori);
        _clusterServiceLocator.WorldCluster.ClienTs[id].Send(p);
        _clusterServiceLocator.WorldCluster.ClienTs[id].Character.Map = map;
    }

    public void ClientUpdate(uint id, uint zone, byte level)
    {
        if (_clusterServiceLocator.WorldCluster.ClienTs[id].Character is null)
        {
            return;
        }

        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.INFORMATION, "[{0:000000}] Client has an updated zone {1:000}", id, zone);
        _clusterServiceLocator.WorldCluster.ClienTs[id].Character.Zone = zone;
        _clusterServiceLocator.WorldCluster.ClienTs[id].Character.Level = level;
    }

    public void ClientSetChatFlag(uint id, byte flag)
    {
        if (_clusterServiceLocator.WorldCluster.ClienTs[id].Character is null)
        {
            return;
        }

        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0:000000}] Client chat flag update [0x{1:X}]", id, flag);
        _clusterServiceLocator.WorldCluster.ClienTs[id].Character.ChatFlag = (ChatFlag)flag;
    }

    public byte[] ClientGetCryptKey(uint id)
    {
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0:000000}] Requested client crypt key", id);
        return _clusterServiceLocator.WorldCluster.ClienTs[id].Client.PacketEncryption.Hash;
    }

    public void Broadcast(byte[] data)
    {
        byte[] b;
        _clusterServiceLocator.WorldCluster.CharacteRsLock.AcquireReaderLock(_clusterServiceLocator.GlobalConstants.DEFAULT_LOCK_TIMEOUT);
        foreach (var objCharacter in _clusterServiceLocator.WorldCluster.CharacteRs)
        {
            if (objCharacter.Value.IsInWorld && objCharacter.Value.Client is not null)
            {
                b = (byte[])data.Clone();
                objCharacter.Value.Client.Send(data);
            }
        }

        _clusterServiceLocator.WorldCluster.CharacteRsLock.ReleaseReaderLock();
    }

    public void BroadcastGroup(long groupId, byte[] data)
    {
        {
            var withBlock = _clusterServiceLocator.WcHandlersGroup.GrouPs[groupId];
            for (byte i = 0, loopTo = (byte)(withBlock.Members.Length - 1); i <= loopTo; i++)
            {
                if (withBlock.Members[i] is not null)
                {
                    withBlock.Members[i].Client.Send((byte[])data.Clone());
                }
            }
        }
    }

    public void BroadcastRaid(long groupId, byte[] data)
    {
        {
            var withBlock = _clusterServiceLocator.WcHandlersGroup.GrouPs[groupId];
            for (byte i = 0, loopTo = (byte)(withBlock.Members.Length - 1); i <= loopTo; i++)
            {
                if (withBlock.Members[i] is not null && withBlock.Members[i].Client is not null)
                {
                    withBlock.Members[i].Client.Send((byte[])data.Clone());
                }
            }
        }
    }

    public void BroadcastGuild(long guildId, byte[] data)
    {
        // TODO: Not implement yet
    }

    public void BroadcastGuildOfficers(long guildId, byte[] data)
    {
        // TODO: Not implement yet
    }

    public bool InstanceCheck(ClientClass client, uint mapId)
    {
        if (!_clusterServiceLocator.WcNetwork.WorldServer.Worlds.ContainsKey(mapId))
        {
            // We don't create new continents
            if (_clusterServiceLocator.Functions.IsContinentMap((int)mapId))
            {
                _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.WARNING, "[{0:000000}] Requested Instance Map [{1}] is a continent", client.Index, mapId);
                client.Send(new PacketClass(Opcodes.SMSG_LOGOUT_COMPLETE));
                new PacketClass(Opcodes.SMSG_LOGOUT_COMPLETE).Dispose();
                client.Character.IsInWorld = false;
                return false;
            }

            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.INFORMATION, "[{0:000000}] Requesting Instance Map [{1}]", client.Index, mapId);
            IWorld parentMap = default;
            WorldInfo parentMapInfo = null;

            // Check if we got parent map
            if (_clusterServiceLocator.WcNetwork.WorldServer.Worlds.ContainsKey((uint)_clusterServiceLocator.WsDbcDatabase.Maps[(int)mapId].ParentMap) && _clusterServiceLocator.WcNetwork.WorldServer.Worlds[(uint)_clusterServiceLocator.WsDbcDatabase.Maps[(int)mapId].ParentMap].InstanceCanCreate((int)_clusterServiceLocator.WsDbcDatabase.Maps[(int)mapId].Type))
            {
                parentMap = _clusterServiceLocator.WcNetwork.WorldServer.Worlds[(uint)_clusterServiceLocator.WsDbcDatabase.Maps[(int)mapId].ParentMap];
                parentMapInfo = _clusterServiceLocator.WcNetwork.WorldServer.WorldsInfo[(uint)_clusterServiceLocator.WsDbcDatabase.Maps[(int)mapId].ParentMap];
            }
            else if (_clusterServiceLocator.WcNetwork.WorldServer.Worlds.ContainsKey(0U) && _clusterServiceLocator.WcNetwork.WorldServer.Worlds[0U].InstanceCanCreate((int)_clusterServiceLocator.WsDbcDatabase.Maps[(int)mapId].Type))
            {
                parentMap = _clusterServiceLocator.WcNetwork.WorldServer.Worlds[0U];
                parentMapInfo = _clusterServiceLocator.WcNetwork.WorldServer.WorldsInfo[0U];
            }
            else if (_clusterServiceLocator.WcNetwork.WorldServer.Worlds.ContainsKey(1U) && _clusterServiceLocator.WcNetwork.WorldServer.Worlds[1U].InstanceCanCreate((int)_clusterServiceLocator.WsDbcDatabase.Maps[(int)mapId].Type))
            {
                parentMap = _clusterServiceLocator.WcNetwork.WorldServer.Worlds[1U];
                parentMapInfo = _clusterServiceLocator.WcNetwork.WorldServer.WorldsInfo[1U];
            }

            if (parentMap is null)
            {
                _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.WARNING, "[{0:000000}] Requested Instance Map [{1}] can't be loaded", client.Index, mapId);
                client.Send(new PacketClass(Opcodes.SMSG_LOGOUT_COMPLETE));
                new PacketClass(Opcodes.SMSG_LOGOUT_COMPLETE).Dispose();
                client.Character.IsInWorld = false;
                return false;
            }

            if (parentMap.InstanceCreateAsync(mapId) != null)
            {
                parentMap.InstanceCreateAsync(mapId).Wait();
            }

            lock (((ICollection)Worlds).SyncRoot)
            {
                _clusterServiceLocator.WcNetwork.WorldServer.Worlds.Add(mapId, parentMap);
                _clusterServiceLocator.WcNetwork.WorldServer.WorldsInfo.Add(mapId, parentMapInfo);
            }
            return true;
        }

        return true;
    }

    public bool BattlefieldCheck(uint mapId)
    {
        // Create map
        if (!_clusterServiceLocator.WcNetwork.WorldServer.Worlds.ContainsKey(mapId))
        {
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.INFORMATION, "[SERVER] Requesting battlefield map [{0}]", mapId);
            IWorld parentMap = default;
            WorldInfo parentMapInfo = null;

            // Check if we got parent map
            if (_clusterServiceLocator.WcNetwork.WorldServer.Worlds.ContainsKey((uint)_clusterServiceLocator.WsDbcDatabase.Maps[(int)mapId].ParentMap) && _clusterServiceLocator.WcNetwork.WorldServer.Worlds[(uint)_clusterServiceLocator.WsDbcDatabase.Maps[(int)mapId].ParentMap].InstanceCanCreate((int)_clusterServiceLocator.WsDbcDatabase.Maps[(int)mapId].Type))
            {
                parentMap = _clusterServiceLocator.WcNetwork.WorldServer.Worlds[(uint)_clusterServiceLocator.WsDbcDatabase.Maps[(int)mapId].ParentMap];
                parentMapInfo = _clusterServiceLocator.WcNetwork.WorldServer.WorldsInfo[(uint)_clusterServiceLocator.WsDbcDatabase.Maps[(int)mapId].ParentMap];
            }
            else if (_clusterServiceLocator.WcNetwork.WorldServer.Worlds.ContainsKey(0U) && _clusterServiceLocator.WcNetwork.WorldServer.Worlds[0U].InstanceCanCreate((int)_clusterServiceLocator.WsDbcDatabase.Maps[(int)mapId].Type))
            {
                parentMap = _clusterServiceLocator.WcNetwork.WorldServer.Worlds[0U];
                parentMapInfo = _clusterServiceLocator.WcNetwork.WorldServer.WorldsInfo[0U];
            }
            else if (_clusterServiceLocator.WcNetwork.WorldServer.Worlds.ContainsKey(1U) && _clusterServiceLocator.WcNetwork.WorldServer.Worlds[1U].InstanceCanCreate((int)_clusterServiceLocator.WsDbcDatabase.Maps[(int)mapId].Type))
            {
                parentMap = _clusterServiceLocator.WcNetwork.WorldServer.Worlds[1U];
                parentMapInfo = _clusterServiceLocator.WcNetwork.WorldServer.WorldsInfo[1U];
            }

            if (parentMap is null)
            {
                _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.WARNING, "[SERVER] Requested battlefield map [{0}] can't be loaded", mapId);
                return false;
            }

            parentMap.InstanceCreateAsync(mapId).Wait();
            lock (((ICollection)Worlds).SyncRoot)
            {
                _clusterServiceLocator.WcNetwork.WorldServer.Worlds.Add(mapId, parentMap);
                _clusterServiceLocator.WcNetwork.WorldServer.WorldsInfo.Add(mapId, parentMapInfo);
            }
            return true;
        }

        return true;
    }

    public List<int> BattlefieldList(byte mapType)
    {
        List<int> battlefieldMap = new();
        _clusterServiceLocator.WcHandlersBattleground.BattlefielDsLock.AcquireReaderLock(_clusterServiceLocator.GlobalConstants.DEFAULT_LOCK_TIMEOUT);
        foreach (var bg in _clusterServiceLocator.WcHandlersBattleground.BattlefielDs)
        {
            if ((byte)bg.Value.MapType == mapType)
            {
                battlefieldMap.Add(bg.Value.Id);
            }
        }

        _clusterServiceLocator.WcHandlersBattleground.BattlefielDsLock.ReleaseReaderLock();
        return battlefieldMap;
    }

    public void BattlefieldFinish(int battlefieldId)
    {
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.INFORMATION, "[B{0:0000}] Battlefield finished", battlefieldId);
    }

    public void GroupRequestUpdate(uint id)
    {
        if (_clusterServiceLocator.WorldCluster.ClienTs.ContainsKey(id) && _clusterServiceLocator.WorldCluster.ClienTs[id].Character is not null && _clusterServiceLocator.WorldCluster.ClienTs[id].Character.IsInWorld && _clusterServiceLocator.WorldCluster.ClienTs[id].Character.IsInGroup)
        {
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.NETWORK, "[G{0:00000}] Group update request", _clusterServiceLocator.WorldCluster.ClienTs[id].Character.Group.Id);
            try
            {
                _clusterServiceLocator.WorldCluster.ClienTs[id].Character.GetWorld.GroupUpdate(_clusterServiceLocator.WorldCluster.ClienTs[id].Character.Group.Id, (byte)_clusterServiceLocator.WorldCluster.ClienTs[id].Character.Group.Type, _clusterServiceLocator.WorldCluster.ClienTs[id].Character.Group.GetLeader().Guid, _clusterServiceLocator.WorldCluster.ClienTs[id].Character.Group.GetMembers());
                _clusterServiceLocator.WorldCluster.ClienTs[id].Character.GetWorld.GroupUpdateLoot(_clusterServiceLocator.WorldCluster.ClienTs[id].Character.Group.Id, (byte)_clusterServiceLocator.WorldCluster.ClienTs[id].Character.Group.DungeonDifficulty, (byte)_clusterServiceLocator.WorldCluster.ClienTs[id].Character.Group.LootMethod, (byte)_clusterServiceLocator.WorldCluster.ClienTs[id].Character.Group.LootThreshold, _clusterServiceLocator.WorldCluster.ClienTs[id].Character.Group.GetLootMaster().Guid);
            }
            catch
            {
                _clusterServiceLocator.WcNetwork.WorldServer.Disconnect("NULL", new List<uint> { _clusterServiceLocator.WorldCluster.ClienTs[id].Character.Map });
            }
        }
    }

    public void GroupSendUpdate(long groupId)
    {
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.NETWORK, "[G{0:00000}] Group update", groupId);
        lock (((ICollection)Worlds).SyncRoot)
        {
            foreach (var w in Worlds)
            {
                try
                {
                    w.Value.GroupUpdate(groupId, (byte)_clusterServiceLocator.WcHandlersGroup.GrouPs[groupId].Type, _clusterServiceLocator.WcHandlersGroup.GrouPs[groupId].GetLeader().Guid, _clusterServiceLocator.WcHandlersGroup.GrouPs[groupId].GetMembers());
                }
                catch (Exception)
                {
                    _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.FAILED, "[G{0:00000}] Group update failed for [M{1:000}]", groupId, w.Key);
                }
            }
        }
    }

    public void GroupSendUpdateLoot(long groupId)
    {
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.NETWORK, "[G{0:00000}] Group update loot", groupId);
        lock (((ICollection)Worlds).SyncRoot)
        {
            foreach (var w in Worlds)
            {
                try
                {
                    w.Value.GroupUpdateLoot(groupId, (byte)_clusterServiceLocator.WcHandlersGroup.GrouPs[groupId].DungeonDifficulty, (byte)_clusterServiceLocator.WcHandlersGroup.GrouPs[groupId].LootMethod, (byte)_clusterServiceLocator.WcHandlersGroup.GrouPs[groupId].LootThreshold, _clusterServiceLocator.WcHandlersGroup.GrouPs[groupId].GetLootMaster().Guid);
                }
                catch (Exception)
                {
                    _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.FAILED, "[G{0:00000}] Group update loot failed for [M{1:000}]", groupId, w.Key);
                }
            }
        }
    }
}
