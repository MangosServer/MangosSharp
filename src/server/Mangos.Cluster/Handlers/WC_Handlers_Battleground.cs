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
using System;
using System.Collections.Generic;
using System.Threading;

namespace Mangos.Cluster.Handlers;

public class WcHandlersBattleground
{
    private readonly ClusterServiceLocator _clusterServiceLocator;

    public WcHandlersBattleground(ClusterServiceLocator clusterServiceLocator)
    {
        _clusterServiceLocator = clusterServiceLocator;
    }

    public void On_CMSG_BATTLEFIELD_PORT(PacketClass packet, ClientClass client)
    {
        packet.GetInt16();

        // Dim Unk1 As Byte = packet.GetInt8
        // Dim Unk2 As Byte = packet.GetInt8                   'unk, can be 0x0 (may be if was invited?) and 0x1
        // Dim MapType As UInteger = packet.GetInt32           'type id from dbc
        // Dim MapType As Byte = packet.GetUInt8
        uint id = packet.GetUInt16();               // ID
        var action = (byte)packet.GetUInt8();                 // enter battle 0x1, leave queue 0x0

        // _WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_BATTLEFIELD_PORT [MapType: {2}, Action: {3}, Unk1: {4}, Unk2: {5}, ID: {6}]", client.IP, client.Port, MapType, Action, Unk1, Unk2, ID)
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_BATTLEFIELD_PORT [Action: {1}, ID: {2}]", client.IP, client.Port, action, id);
        if (action == 0)
        {
            BattlefielDs[(int)id].Leave(client.Character);
        }
        else
        {
            BattlefielDs[(int)id].Join(client.Character);
        }
    }

    public void On_CMSG_LEAVE_BATTLEFIELD(PacketClass packet, ClientClass client)
    {
        packet.GetInt16();
        var unk1 = packet.GetInt8();
        var unk2 = packet.GetInt8();
        var mapType = (uint)packet.GetInt32();
        uint id = packet.GetUInt16();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_LEAVE_BATTLEFIELD [MapType: {2}, Unk1: {3}, Unk2: {4}, ID: {5}]", client.IP, client.Port, mapType, unk1, unk2, id);
        BattlefielDs[(int)id].Leave(client.Character);
    }

    public void On_CMSG_BATTLEMASTER_JOIN(PacketClass packet, ClientClass client)
    {
        if (packet.Data.Length - 1 < 16)
        {
            return;
        }

        packet.GetInt16();
        var guid = packet.GetUInt64();
        var mapType = (uint)packet.GetInt32();
        var instance = (uint)packet.GetInt32();
        var asGroup = packet.GetInt8();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_BATTLEMASTER_JOIN [MapType: {2}, Instance: {3}, Group: {4}, GUID: {5}]", client.IP, client.Port, mapType, instance, asGroup, guid);
        GetBattlefield((BattlefieldMapType)mapType, (byte)client.Character.Level).Enqueue(client.Character);
    }

    public Dictionary<int, Battlefield> BattlefielDs = new();
    public ReaderWriterLock BattlefielDsLock = new();
    private int _battlefielDsCounter;

    public class Battlefield : IDisposable
    {
        private readonly ClusterServiceLocator _clusterServiceLocator;

        private readonly List<WcHandlerCharacter.CharacterObject> _queueTeam1 = new();
        private readonly List<WcHandlerCharacter.CharacterObject> _queueTeam2 = new();
        private readonly List<WcHandlerCharacter.CharacterObject> _invitedTeam1 = new();
        private readonly List<WcHandlerCharacter.CharacterObject> _invitedTeam2 = new();
        private readonly List<WcHandlerCharacter.CharacterObject> _membersTeam1 = new();
        private readonly List<WcHandlerCharacter.CharacterObject> _membersTeam2 = new();
        public readonly int Id;
        private readonly uint _map;
        public readonly BattlefieldMapType MapType;
        public BattlefieldType Type;
        internal readonly byte LevelMin;
        internal readonly byte LevelMax;
        private readonly int _maxPlayersPerTeam = 10; // Is this right
        private readonly int _minPlayersPerTeam = 10;
        private readonly Timer _bfTimer;

        public Battlefield(BattlefieldMapType rMapType, byte rLevel, uint rMap, ClusterServiceLocator clusterServiceLocator)
        {
            _clusterServiceLocator = clusterServiceLocator;
            Id = Interlocked.Increment(ref _clusterServiceLocator.WcHandlersBattleground._battlefielDsCounter);
            LevelMin = 0;
            LevelMax = 60;
            MapType = rMapType;
            _map = rMap;
            _maxPlayersPerTeam = _clusterServiceLocator.WsDbcDatabase.Battlegrounds[(byte)rMapType].MaxPlayersPerTeam;
            _minPlayersPerTeam = _clusterServiceLocator.WsDbcDatabase.Battlegrounds[(byte)rMapType].MinPlayersPerTeam;
            _clusterServiceLocator.WcHandlersBattleground.BattlefielDsLock.AcquireWriterLock(_clusterServiceLocator.GlobalConstants.DEFAULT_LOCK_TIMEOUT);
            _clusterServiceLocator.WcHandlersBattleground.BattlefielDs.Add(Id, this);
            _clusterServiceLocator.WcHandlersBattleground.BattlefielDsLock.ReleaseWriterLock();
            _bfTimer = new Timer(Update, null, 20000, 20000);
        }

        private bool _disposedValue; // To detect redundant calls

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                // TODO: set large fields to null.
                _clusterServiceLocator.WcHandlersBattleground.BattlefielDsLock.AcquireWriterLock(_clusterServiceLocator.GlobalConstants.DEFAULT_LOCK_TIMEOUT);
                _clusterServiceLocator.WcHandlersBattleground.BattlefielDs.Remove(Id);
                _clusterServiceLocator.WcHandlersBattleground.BattlefielDsLock.ReleaseWriterLock();
                _bfTimer.Dispose();
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

        /// <summary>
        /// Updates the specified state.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        private void Update(object state)
        {
            // DONE: Adding members from queue
            if (_membersTeam1.Count + _invitedTeam1.Count < _maxPlayersPerTeam && _queueTeam1.Count > 0)
            {
                var objCharacter = _queueTeam1[0];
                _queueTeam1.RemoveAt(0);
                _invitedTeam1.Add(objCharacter);
                SendBattlegroundStatus(objCharacter, 0);
            }

            if (_membersTeam2.Count + _invitedTeam2.Count < _maxPlayersPerTeam && _queueTeam2.Count > 0)
            {
                var objCharacter = _queueTeam2[0];
                _queueTeam2.RemoveAt(0);
                _invitedTeam2.Add(objCharacter);
                SendBattlegroundStatus(objCharacter, 0);
            }

            // TODO: Checking minimum players
        }

        /// <summary>
        /// Enqueues the specified obj char.
        /// </summary>
        /// <param name="objCharacter">The obj char.</param>
        /// <returns></returns>
        public void Enqueue(WcHandlerCharacter.CharacterObject objCharacter)
        {
            if (_clusterServiceLocator.Functions.GetCharacterSide((byte)objCharacter.Race))
            {
                _queueTeam1.Add(objCharacter);
            }
            else
            {
                _queueTeam2.Add(objCharacter);
            }

            SendBattlegroundStatus(objCharacter, 0);
        }

        /// <summary>
        /// Joins the specified obj char.
        /// </summary>
        /// <param name="objCharacter">The obj char.</param>
        /// <returns></returns>
        public void Join(WcHandlerCharacter.CharacterObject objCharacter)
        {
            if (_invitedTeam1.Contains(objCharacter) || _invitedTeam2.Contains(objCharacter))
            {
                if (_invitedTeam1.Contains(objCharacter))
                {
                    _membersTeam1.Add(objCharacter);
                    _invitedTeam1.Remove(objCharacter);
                }

                if (_invitedTeam2.Contains(objCharacter))
                {
                    _membersTeam2.Add(objCharacter);
                    _invitedTeam2.Remove(objCharacter);
                }

                SendBattlegroundStatus(objCharacter, 0);
                {
                    var withBlock = _clusterServiceLocator.WsDbcDatabase.WorldSafeLocs[_clusterServiceLocator.WsDbcDatabase.Battlegrounds[(byte)MapType].AllianceStartLoc];
                    // TODO: WTF? characters_locations table? when?
                    // Dim q As New DataTable
                    // _WorldCluster.CharacterDatabase.Query(String.Format("SELECT char_guid FROM characters_locations WHERE char_guid = {0};", objCharacter.GUID), q)
                    // If q.Rows.Count = 0 Then
                    // 'Save only first BG location
                    // _WorldCluster.CharacterDatabase.Update(String.Format("INSERT INTO characters_locations(char_guid, char_positionX, char_positionY, char_positionZ, char_zone_id, char_map_id, char_orientation) VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6});", _
                    // objCharacter.GUID, Trim(Str(objCharacter.PositionX)), Trim(Str(objCharacter.PositionY)), Trim(Str(objCharacter.PositionZ)), objCharacter.Zone, objCharacter.Map, 0))
                    // End If
                    objCharacter.Transfer(withBlock.X, withBlock.Y, withBlock.Z, _clusterServiceLocator.WsDbcDatabase.Battlegrounds[(byte)MapType].AllianceStartO, (int)withBlock.Map);
                }
            }
        }

        /// <summary>
        /// Leaves the specified obj char.
        /// </summary>
        /// <param name="objCharacter">The obj char.</param>
        /// <returns></returns>
        public void Leave(WcHandlerCharacter.CharacterObject objCharacter)
        {
            if (_queueTeam1.Contains(objCharacter) || _queueTeam2.Contains(objCharacter))
            {
                _queueTeam1.Remove(objCharacter);
                _queueTeam2.Remove(objCharacter);
            }
            else if (_membersTeam1.Contains(objCharacter) || _membersTeam2.Contains(objCharacter))
            {
                _membersTeam1.Remove(objCharacter);
                _membersTeam2.Remove(objCharacter);

                // TODO: Still.. characters_locations, doesn't exist?
                // Dim q As New DataTable
                // _WorldCluster.CharacterDatabase.Query(String.Format("SELECT * FROM characters_locations WHERE char_guid = {0};", objCharacter.GUID), q)
                // If q.Rows.Count = 0 Then
                // SendMessageSystem(objCharacter.Client, "You don't have location saved!")
                // Else
                // objCharacter.Transfer(q.Rows(0).Item("char_positionX"), q.Rows(0).Item("char_positionY"), q.Rows(0).Item("char_positionZ"), q.Rows(0).Item("char_orientation"), q.Rows(0).Item("char_map_id"))
                // End If
            }

            SendBattlegroundStatus(objCharacter, 0);
        }

        /// <summary>
        /// Sends the battleground status.
        /// </summary>
        /// <param name="objCharacter">The obj char.</param>
        /// <param name="slot">The slot.</param>
        /// <returns></returns>
        private void SendBattlegroundStatus(WcHandlerCharacter.CharacterObject objCharacter, byte slot)
        {
            var status = BattlegroundStatus.STATUS_CLEAR;
            if (_queueTeam1.Contains(objCharacter) || _queueTeam2.Contains(objCharacter))
            {
                status = BattlegroundStatus.STATUS_WAIT_QUEUE;
            }
            else if (_invitedTeam1.Contains(objCharacter) || _invitedTeam2.Contains(objCharacter))
            {
                status = BattlegroundStatus.STATUS_WAIT_JOIN;
            }
            else if (_membersTeam1.Contains(objCharacter) || _membersTeam2.Contains(objCharacter))
            {
                status = BattlegroundStatus.STATUS_IN_PROGRESS;
            }

            PacketClass p = new(Opcodes.SMSG_BATTLEFIELD_STATUS);
            try
            {
                p.AddUInt32(slot);               // Slot (0, 1 or 2)

                // p.AddInt8(0)                    'ArenaType
                p.AddUInt32((uint)MapType);              // MapType
                                                         // p.AddInt8(&HD)                  'Unk1 (0xD?)
                                                         // p.AddInt8(0)                    'Unk2
                                                         // p.AddInt16()                   'Unk3 (String?)
                p.AddUInt32((uint)Id);                  // ID

                // p.AddInt32(0)                   'Unk5
                p.AddInt8(0);                    // alliance/horde for BG and skirmish/rated for Arenas
                p.AddUInt32((uint)status);
                switch (status)
                {
                    case var @case when @case == BattlegroundStatus.STATUS_WAIT_QUEUE:
                        {
                            p.AddUInt32(120000U);     // average wait time, milliseconds
                            p.AddUInt32(1U);          // time in queue, updated every minute?
                            break;
                        }

                    case var case1 when case1 == BattlegroundStatus.STATUS_WAIT_JOIN:
                        {
                            p.AddUInt32(_map);        // map id
                            p.AddUInt32(60000U);      // time to remove from queue, milliseconds
                            break;
                        }

                    case var case2 when case2 == BattlegroundStatus.STATUS_IN_PROGRESS:
                        {
                            p.AddUInt32(_map);        // map id
                            p.AddUInt32(0U);          // 0 at bg start, 120000 after bg end, time to bg auto leave, milliseconds
                            p.AddUInt32(1U);          // time from bg start, milliseconds
                            p.AddInt8(1);            // unk sometimes 0x0!
                            break;
                        }

                    case var case3 when case3 == BattlegroundStatus.STATUS_CLEAR:
                        {
                            break;
                        }
                        // Do nothing
                }

                objCharacter.Client.Send(p);
            }
            finally
            {
                p.Dispose();
            }
        }
    }

    /// <summary>
    /// Gets the battlefield.
    /// </summary>
    /// <param name="mapType">Type of the map.</param>
    /// <param name="level">The level.</param>
    /// <returns></returns>
    public Battlefield GetBattlefield(BattlefieldMapType mapType, byte level)
    {
        Battlefield battlefield = null;
        BattlefielDsLock.AcquireReaderLock(_clusterServiceLocator.GlobalConstants.DEFAULT_LOCK_TIMEOUT);
        foreach (var b in BattlefielDs)
        {
            if (b.Value.MapType == mapType && b.Value.LevelMax >= level && b.Value.LevelMin <= level)
            {
                battlefield = b.Value;
            }
        }

        BattlefielDsLock.ReleaseReaderLock();

        // DONE: Create new if not found any
        if (battlefield is null)
        {
            var map = (uint)GetBattleGrounMapIdByTypeId((BattleGroundTypeId)mapType);
            if (_clusterServiceLocator.WcNetwork.WorldServer.BattlefieldCheck(map))
            {
                battlefield = new Battlefield(mapType, level, map, _clusterServiceLocator);
            }
            else
            {
                return null;
            }
        }

        return battlefield;
    }

    // ''' <summary>
    // ''' Gets the battle ground type id by map id.
    // ''' </summary>
    // ''' <param name="mapId">The map id.</param>
    // ''' <returns></returns>
    // Function GetBattleGroundTypeIdByMapId(ByVal mapId As Integer) As Integer
    // Select Case mapId
    // Case 30
    // Return BattleGroundTypeId.BATTLEGROUND_AV
    // Case 489
    // Return BattleGroundTypeId.BATTLEGROUND_WS
    // Case 529
    // Return BattleGroundTypeId.BATTLEGROUND_AB
    // Case Else
    // Return BattleGroundTypeId.BATTLEGROUND_TYPE_NONE
    // End Select
    // End Function

    /// <summary>
    /// Gets the battle groun map id by type id.
    /// </summary>
    /// <param name="bgTypeId">The bg type id.</param>
    /// <returns></returns>
    private int GetBattleGrounMapIdByTypeId(BattleGroundTypeId bgTypeId)
    {
        switch (bgTypeId)
        {
            case var @case when @case == BattleGroundTypeId.BATTLEGROUND_AV:
                {
                    return 30;
                }

            case var case1 when case1 == BattleGroundTypeId.BATTLEGROUND_WS:
                {
                    return 489;
                }

            case var case2 when case2 == BattleGroundTypeId.BATTLEGROUND_AB:
                {
                    return 529;
                }

            default:
                {
                    return 0;
                }
        }
    }

    /// <summary>
    /// Sends the battleground group joined.
    /// </summary>
    /// <param name="objCharacter">The objCharacter.</param>
    /// <returns></returns>
    public void SendBattlegroundGroupJoined(WcHandlerCharacter.CharacterObject objCharacter)
    {
        // 0 - Your group has joined a battleground queue, but you are not eligible
        // 1 - Your group has joined the queue for AV
        // 2 - Your group has joined the queue for WS
        // 3 - Your group has joined the queue for AB

        PacketClass p = new(Opcodes.SMSG_GROUP_JOINED_BATTLEGROUND);
        try
        {
            p.AddUInt32(0xFFFFFFFEU);
            objCharacter.Client.Send(p);
        }
        finally
        {
            p.Dispose();
        }
    }

    public void On_MSG_BATTLEGROUND_PLAYER_POSITIONS(PacketClass packet, ClientClass client)
    {
        PacketClass p = new(Opcodes.MSG_BATTLEGROUND_PLAYER_POSITIONS);
        try
        {
            p.AddUInt32(0U);
            p.AddUInt32(0U); // flagCarrierCount
        }
        finally
        {
            p.Dispose();
        }
    }
}
