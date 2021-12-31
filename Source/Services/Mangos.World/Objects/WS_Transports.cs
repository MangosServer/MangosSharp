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

using Mangos.Common.Enums.GameObject;
using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using Mangos.Common.Legacy;
using Mangos.World.Globals;
using Mangos.World.Handlers;
using Mangos.World.Maps;
using Mangos.World.Player;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace Mangos.World.Objects;

public class WS_Transports
{
    public class TransportMove
    {
        public float X;

        public float Y;

        public float Z;

        public uint MapID;

        public int ActionFlag;

        public int Delay;

        public float DistSinceStop;

        public float DistUntilStop;

        public float DistFromPrev;

        public float tFrom;

        public float tTo;

        public TransportMove(float PosX, float PosY, float PosZ, uint Map, int Action, int WaitTime)
        {
            X = PosX;
            Y = PosY;
            Z = PosZ;
            MapID = Map;
            ActionFlag = Action;
            Delay = WaitTime;
            DistSinceStop = -1f;
            DistUntilStop = -1f;
            DistSinceStop = -1f;
            tFrom = 0f;
            tTo = 0f;
        }
    }

    public class TransportWP
    {
        public int ID;

        public float X;

        public float Y;

        public float Z;

        public uint MapID;

        public bool Teleport;

        public int Time;

        public TransportWP(int ID_, int Time_, float PosX, float PosY, float PosZ, uint Map, bool Teleport_)
        {
            ID = ID_;
            Time = Time_;
            X = PosX;
            Y = PosY;
            Z = PosZ;
            MapID = Map;
            Teleport = Teleport_;
        }
    }

    public class TransportObject : WS_GameObjects.GameObject
    {
        public string TransportName;

        private readonly List<WS_Base.BaseUnit> Passengers;

        private readonly List<TransportWP> Waypoints;

        private readonly int Period;

        private int PathTime;

        private int FirstStop;

        private int LastStop;

        private int CurrentWaypoint;

        private int NextWaypoint;

        private int NextNodeTime;

        private readonly int TimeToNextEvent;

        private readonly TransportStates TransportState;

        private readonly byte TransportAt;

        public TransportObject(int ID_, string Name, int Period_)
            : base(ID_, WorldServiceLocator._WS_Transports.GetNewGUID())
        {
            TransportName = "";
            Passengers = new List<WS_Base.BaseUnit>();
            Waypoints = new List<TransportWP>();
            Period = 0;
            PathTime = 0;
            FirstStop = -1;
            LastStop = -1;
            CurrentWaypoint = 0;
            NextWaypoint = 0;
            NextNodeTime = 0;
            TimeToNextEvent = 0;
            TransportState = TransportStates.TRANSPORT_DOCKED;
            TransportAt = 0;
            TransportName = Name;
            Period = Period_;
            if (GenerateWaypoints())
            {
                positionX = Waypoints[0].X;
                positionY = Waypoints[0].Y;
                positionZ = Waypoints[0].Z;
                MapID = Waypoints[0].MapID;
                orientation = 1f;
                VisibleDistance = 99999f;
                State = GameObjectLootState.DOOR_CLOSED;
                TransportState = TransportStates.TRANSPORT_DOCKED;
                TimeToNextEvent = 60000;
                WorldServiceLocator._WorldServer.WORLD_TRANSPORTs_Lock.AcquireWriterLock(-1);
                WorldServiceLocator._WorldServer.WORLD_TRANSPORTs.Add(GUID, this);
                WorldServiceLocator._WorldServer.WORLD_TRANSPORTs_Lock.ReleaseWriterLock();
                Update();
            }
        }

        public bool GenerateWaypoints()
        {
            checked
            {
                var PathID = (int)GetSound(0);
                float ShipSpeed = GetSound(1);
                if (!WorldServiceLocator._WS_DBCDatabase.TaxiPaths.ContainsKey(PathID))
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "An transport [{0} - {1}] is created with an invalid TaxiPath.", ID, TransportName);
                    return false;
                }
                var MapsUsed = 0;
                var MapChange = 0;
                List<TransportMove> PathPoints = new();
                var t = 0;
                if (WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes.ContainsKey(PathID))
                {
                    var num = WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes[PathID].Count - 2;
                    for (var i = 0; i <= num; i++)
                    {
                        if (MapChange == 0)
                        {
                            if (WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes[PathID].ContainsKey(i) & WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes[PathID].ContainsKey(i + 1))
                            {
                                if (WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes[PathID][i].MapID == WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes[PathID][i + 1].MapID)
                                {
                                    PathPoints.Add(new TransportMove(WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes[PathID][i].x, WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes[PathID][i].y, WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes[PathID][i].z, (uint)WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes[PathID][i].MapID, WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes[PathID][i].action, WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes[PathID][i].waittime));
                                    if (WorldServiceLocator._WS_Maps.Maps.ContainsKey((uint)WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes[PathID][i].MapID))
                                    {
                                        MapsUsed++;
                                    }
                                }
                            }
                            else
                            {
                                MapChange = 1;
                            }
                        }
                        else
                        {
                            MapChange = 0;
                        }
                    }
                    if (MapsUsed == 0)
                    {
                        return false;
                    }
                    PathPoints[0].DistFromPrev = 0f;
                    if (PathPoints[0].ActionFlag == 2)
                    {
                        LastStop = 0;
                    }
                    var num2 = PathPoints.Count - 1;
                    for (var j = 1; j <= num2; j++)
                    {
                        PathPoints[j].DistFromPrev = PathPoints[j].ActionFlag == 1 || PathPoints[j].MapID != PathPoints[j - 1].MapID
                            ? 0f
                            : WorldServiceLocator._WS_Combat.GetDistance(PathPoints[j].X, PathPoints[j - 1].X, PathPoints[j].Y, PathPoints[j - 1].Y, PathPoints[j].Z, PathPoints[j - 1].Z);
                        if (PathPoints[j].ActionFlag == 2)
                        {
                            if (FirstStop == -1)
                            {
                                FirstStop = j;
                            }
                            LastStop = j;
                        }
                    }
                    var tmpDist = 0f;
                    var num3 = PathPoints.Count - 1;
                    for (var n = 0; n <= num3; n++)
                    {
                        unchecked
                        {
                            var j2 = checked(n + LastStop) % PathPoints.Count;
                            if (j2 >= 0)
                            {
                                tmpDist = (PathPoints[j2].ActionFlag != 2) ? (tmpDist + PathPoints[j2].DistFromPrev) : 0f;
                                PathPoints[j2].DistSinceStop = tmpDist;
                            }
                        }
                    }
                    var num4 = PathPoints.Count - 1;
                    for (var m = num4; m >= 0; m += -1)
                    {
                        unchecked
                        {
                            var j2 = checked(m + FirstStop + 1) % PathPoints.Count;
                            tmpDist += PathPoints[checked(j2 + 1) % PathPoints.Count].DistFromPrev;
                            PathPoints[j2].DistUntilStop = tmpDist;
                            if (PathPoints[j2].ActionFlag == 2)
                            {
                                tmpDist = 0f;
                            }
                        }
                    }
                    var num5 = PathPoints.Count - 1;
                    for (var l = 0; l <= num5; l++)
                    {
                        PathPoints[l].tFrom = PathPoints[l].DistSinceStop < 450f
                            ? (float)Math.Sqrt(2f * PathPoints[l].DistSinceStop)
                            : ((PathPoints[l].DistSinceStop - 450f) / 30f) + 30f;
                        PathPoints[l].tTo = PathPoints[l].DistUntilStop < 450f
                            ? (float)Math.Sqrt(2f * PathPoints[l].DistUntilStop)
                            : ((PathPoints[l].DistUntilStop - 450f) / 30f) + 30f;
                        PathPoints[l].tFrom *= 1000f;
                        PathPoints[l].tTo *= 1000f;
                    }
                    var teleport = false;
                    if (PathPoints[^1].MapID != PathPoints[0].MapID)
                    {
                        teleport = true;
                    }
                    Waypoints.Add(new TransportWP(0, 0, PathPoints[0].X, PathPoints[0].Y, PathPoints[0].Z, PathPoints[0].MapID, teleport));
                    t += PathPoints[0].Delay * 1000;
                    var cM = PathPoints[0].MapID;
                    var num6 = PathPoints.Count - 2;
                    for (var k = 0; k <= num6; k++)
                    {
                        var d = 0f;
                        var tFrom = PathPoints[k].tFrom;
                        var tTo = PathPoints[k].tTo;
                        if (d < PathPoints[k + 1].DistFromPrev && tTo > 0f)
                        {
                            while (d < PathPoints[k + 1].DistFromPrev && tTo > 0f)
                            {
                                tFrom += 100f;
                                tTo -= 100f;
                                if (d > 0f)
                                {
                                    var newX = PathPoints[k].X + ((PathPoints[k + 1].X - PathPoints[k].X) * d / PathPoints[k + 1].DistFromPrev);
                                    var newY = PathPoints[k].Y + ((PathPoints[k + 1].Y - PathPoints[k].Y) * d / PathPoints[k + 1].DistFromPrev);
                                    var newZ = PathPoints[k].Z + ((PathPoints[k + 1].Z - PathPoints[k].Z) * d / PathPoints[k + 1].DistFromPrev);
                                    teleport = false;
                                    if (PathPoints[k].MapID != cM)
                                    {
                                        teleport = true;
                                        cM = PathPoints[k].MapID;
                                    }
                                    if (teleport)
                                    {
                                        Waypoints.Add(new TransportWP(k, t, newX, newY, newZ, PathPoints[k].MapID, teleport));
                                    }
                                }
                                if (tFrom < tTo)
                                {
                                    d = (!(tFrom <= 30000f)) ? (450f + (30f * ((tFrom - 30000f) / 1000f))) : (0.5f * (tFrom / 1000f) * (tFrom / 1000f));
                                    d -= PathPoints[k].DistSinceStop;
                                }
                                else
                                {
                                    d = (!(tTo <= 30000f)) ? (450f + (30f * ((tTo - 30000f) / 1000f))) : (0.5f * (tTo / 1000f) * (tTo / 1000f));
                                    d = PathPoints[k].DistUntilStop - d;
                                }
                                t += 100;
                            }
                            t -= 100;
                        }
                        t = (!(PathPoints[k + 1].tFrom > PathPoints[k + 1].tTo)) ? ((int)(t + (((long)PathPoints[k + 1].tTo) % 100))) : ((int)(t + (100 - (((long)PathPoints[k + 1].tTo) % 100))));
                        teleport = false;
                        if (PathPoints[k + 1].ActionFlag == 1 || PathPoints[k + 1].MapID != PathPoints[k].MapID)
                        {
                            teleport = true;
                            cM = PathPoints[k + 1].MapID;
                        }
                        Waypoints.Add(new TransportWP(k, t, PathPoints[k + 1].X, PathPoints[k + 1].Y, PathPoints[k + 1].Z, PathPoints[k + 1].MapID, teleport));
                        t += PathPoints[k + 1].Delay * 1000;
                    }
                    CurrentWaypoint = 0;
                    CurrentWaypoint = GetNextWaypoint();
                    NextWaypoint = GetNextWaypoint();
                    PathTime = t;
                    NextNodeTime = Waypoints[CurrentWaypoint].Time;
                    return true;
                }
                return false;
            }
        }

        public int GetNextWaypoint()
        {
            var tmpWP = CurrentWaypoint;
            tmpWP = checked(tmpWP + 1);
            if (tmpWP >= Waypoints.Count)
            {
                tmpWP = 0;
            }
            return tmpWP;
        }

        public void AddPassenger(ref WS_Base.BaseUnit Unit)
        {
            if (!Passengers.Contains(Unit))
            {
                lock (Passengers)
                {
                    Passengers.Add(Unit);
                }
            }
        }

        public void RemovePassenger(ref WS_Base.BaseUnit Unit)
        {
            if (Passengers.Contains(Unit))
            {
                lock (Passengers)
                {
                    Passengers.Remove(Unit);
                }
            }
        }

        public void Update()
        {
            if (Waypoints.Count <= 1)
            {
                return;
            }
            var Timer = WorldServiceLocator._WS_Network.MsTime() % Period;
            while (Math.Abs(checked(Timer - Waypoints[CurrentWaypoint].Time)) % PathTime > Math.Abs(checked(Waypoints[NextWaypoint].Time - Waypoints[CurrentWaypoint].Time)) % PathTime)
            {
                CurrentWaypoint = GetNextWaypoint();
                NextWaypoint = GetNextWaypoint();
                if (Waypoints[CurrentWaypoint].MapID != MapID || Waypoints[CurrentWaypoint].Teleport)
                {
                    TeleportTransport(Waypoints[CurrentWaypoint].MapID, Waypoints[CurrentWaypoint].X, Waypoints[CurrentWaypoint].Y, Waypoints[CurrentWaypoint].Z);
                }
                else
                {
                    positionX = Waypoints[CurrentWaypoint].X;
                    positionY = Waypoints[CurrentWaypoint].Y;
                    positionZ = Waypoints[CurrentWaypoint].Z;
                    CheckCell();
                }
                if (CurrentWaypoint == FirstStop || CurrentWaypoint == LastStop)
                {
                    switch (ID)
                    {
                        case 164871:
                        case 175080:
                        case 176495:
                            SendPlaySound(5154);
                            break;

                        case 20808:
                        case 176231:
                        case 176244:
                        case 176310:
                        case 177233:
                        case 181646:
                            SendPlaySound(5495);
                            break;

                        default:
                            SendPlaySound(5154);
                            break;
                    }
                }
                NextNodeTime = Waypoints[CurrentWaypoint].Time;
                if (CurrentWaypoint == 0)
                {
                    break;
                }
            }
        }

        public void CreateEveryoneOnTransport(ref WS_PlayerData.CharacterObject Character)
        {
            Packets.PacketClass mePacket = new(Opcodes.SMSG_UPDATE_OBJECT);
            mePacket.AddInt32(1);
            mePacket.AddInt8(0);
            Packets.UpdateClass meTmpUpdate = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
            Character.FillAllUpdateFlags(ref meTmpUpdate);
            meTmpUpdate.AddToPacket(ref mePacket, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, ref Character);
            meTmpUpdate.Dispose();
            mePacket.CompressUpdatePacket();
            var tmpArray = Passengers.ToArray();
            var array = tmpArray;
            for (var i = 0; i < array.Length; i = checked(i + 1))
            {
                var tmpUnit = array[i];
                if (tmpUnit == null)
                {
                    continue;
                }
                switch (tmpUnit)
                {
                    case WS_PlayerData.CharacterObject _:
                        {
                            var obj = Character;
                            WS_Base.BaseObject objCharacter = tmpUnit;
                            var flag = obj.CanSee(ref objCharacter);
                            tmpUnit = (WS_Base.BaseUnit)objCharacter;
                            if (flag)
                            {
                                Packets.PacketClass myPacket2 = new(Opcodes.SMSG_UPDATE_OBJECT);
                                try
                                {
                                    myPacket2.AddInt32(1);
                                    myPacket2.AddInt8(0);
                                    Packets.UpdateClass myTmpUpdate2 = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
                                    ((WS_PlayerData.CharacterObject)tmpUnit).FillAllUpdateFlags(ref myTmpUpdate2);
                                    var updateClass = myTmpUpdate2;
                                    WS_PlayerData.CharacterObject updateObject = (WS_PlayerData.CharacterObject)tmpUnit;
                                    updateClass.AddToPacket(ref myPacket2, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, ref updateObject);
                                    myTmpUpdate2.Dispose();
                                    Character.client.Send(ref myPacket2);
                                }
                                finally
                                {
                                    myPacket2.Dispose();
                                }
                                ((WS_PlayerData.CharacterObject)tmpUnit).SeenBy.Add(Character.GUID);
                                Character.playersNear.Add(tmpUnit.GUID);
                            }
                            WS_PlayerData.CharacterObject obj2 = (WS_PlayerData.CharacterObject)tmpUnit;
                            objCharacter = Character;
                            flag = obj2.CanSee(ref objCharacter);
                            Character = (WS_PlayerData.CharacterObject)objCharacter;
                            if (flag)
                            {
                                ((WS_PlayerData.CharacterObject)tmpUnit).client.SendMultiplyPackets(ref mePacket);
                                Character.SeenBy.Add(tmpUnit.GUID);
                                ((WS_PlayerData.CharacterObject)tmpUnit).playersNear.Add(Character.GUID);
                            }

                            break;
                        }

                    default:
                        {
                            if (tmpUnit is not WS_Creatures.CreatureObject)
                            {
                                continue;
                            }
                            var obj3 = Character;
                            WS_Base.BaseObject objCharacter = tmpUnit;
                            var flag = obj3.CanSee(ref objCharacter);
                            tmpUnit = (WS_Base.BaseUnit)objCharacter;
                            if (flag)
                            {
                                Packets.PacketClass myPacket = new(Opcodes.SMSG_UPDATE_OBJECT);
                                try
                                {
                                    myPacket.AddInt32(1);
                                    myPacket.AddInt8(0);
                                    Packets.UpdateClass myTmpUpdate = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_UNIT);
                                    ((WS_Creatures.CreatureObject)tmpUnit).FillAllUpdateFlags(ref myTmpUpdate);
                                    var updateClass2 = myTmpUpdate;
                                    WS_Creatures.CreatureObject updateObject2 = (WS_Creatures.CreatureObject)tmpUnit;
                                    updateClass2.AddToPacket(ref myPacket, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, ref updateObject2);
                                    myTmpUpdate.Dispose();
                                    Character.client.Send(ref myPacket);
                                }
                                finally
                                {
                                    myPacket.Dispose();
                                }
                                ((WS_PlayerData.CharacterObject)tmpUnit).SeenBy.Add(Character.GUID);
                                Character.creaturesNear.Add(tmpUnit.GUID);
                            }

                            break;
                        }
                }
            }
            mePacket.Dispose();
        }

        public void CheckCell(bool Teleported = false)
        {
            byte TileX = default;
            byte TileY = default;
            WorldServiceLocator._WS_Maps.GetMapTile(positionX, positionY, ref TileX, ref TileY);
            if (!Teleported && CellX == TileX && CellY == TileY)
            {
                return;
            }
            if (WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY] != null)
            {
                try
                {
                    WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY].GameObjectsHere.Remove(GUID);
                }
                catch (Exception projectError)
                {
                    ProjectData.SetProjectError(projectError);
                    ProjectData.ClearProjectError();
                }
            }
            CellX = TileX;
            CellY = TileY;
            if (WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY] != null)
            {
                try
                {
                    WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY].GameObjectsHere.Add(GUID);
                }
                catch (Exception projectError2)
                {
                    ProjectData.SetProjectError(projectError2);
                    ProjectData.ClearProjectError();
                }
            }
            NotifyEnter();
        }

        public void NotifyEnter()
        {
            short i = -1;
            checked
            {
                do
                {
                    short j = -1;
                    do
                    {
                        if ((short)unchecked(CellX + i) >= 0 && (short)unchecked(CellX + i) <= 63 && (short)unchecked(CellY + j) >= 0 && (short)unchecked(CellY + j) <= 63 && WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[(short)unchecked(CellX + i), (short)unchecked(CellY + j)] != null && WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[(short)unchecked(CellX + i), (short)unchecked(CellY + j)].PlayersHere.Count > 0)
                        {
                            var tMapTile = WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[(short)unchecked(CellX + i), (short)unchecked(CellY + j)];
                            var list = tMapTile.PlayersHere.ToArray();
                            var array = list;
                            foreach (var plGUID in array)
                            {
                                int num;
                                if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(plGUID))
                                {
                                    var characterObject = WorldServiceLocator._WorldServer.CHARACTERs[plGUID];
                                    WS_Base.BaseObject objCharacter = this;
                                    num = characterObject.CanSee(ref objCharacter) ? 1 : 0;
                                }
                                else
                                {
                                    num = 0;
                                }
                                if (num == 0)
                                {
                                    continue;
                                }
                                Packets.PacketClass packet = new(Opcodes.SMSG_UPDATE_OBJECT);

                                using (packet)
                                {
                                    try
                                    {
                                        packet.AddInt32(1);
                                        packet.AddInt8(0);
                                        Packets.UpdateClass tmpUpdate = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_GAMEOBJECT);
                                        try
                                        {
                                            Dictionary<ulong, WS_PlayerData.CharacterObject> cHARACTERs;
                                            ulong key;
                                            var Character = (cHARACTERs = WorldServiceLocator._WorldServer.CHARACTERs)[key = plGUID];
                                            FillAllUpdateFlags(ref tmpUpdate, ref Character);
                                            cHARACTERs[key] = Character;
                                            var updateClass = tmpUpdate;
                                            WS_GameObjects.GameObject updateObject = this;
                                            updateClass.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, ref updateObject);
                                        }
                                        finally
                                        {
                                            tmpUpdate.Dispose();
                                        }

                                        if (WorldServiceLocator._WorldServer.CHARACTERs.TryGetValue(plGUID, out var _character))
                                        {
                                            _character?.client?.SendMultiplyPackets(ref packet);
                                            _character?.gameObjectsNear?.Add(GUID);
                                            SeenBy?.Add(plGUID);
                                        }
                                        else
                                        {
                                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, $"Failed to retrieve character {plGUID}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, $"{ex.Message}{Environment.NewLine}");
                                    }
                                }
                            }
                        }
                        j = (short)unchecked(j + 1);
                    }
                    while (j <= 1);
                    i = (short)unchecked(i + 1);
                }
                while (i <= 1);
            }
        }

        public void NotifyLeave()
        {
            var array = SeenBy.ToArray();
            foreach (var plGUID in array)
            {
                if (WorldServiceLocator._WorldServer.CHARACTERs[plGUID].gameObjectsNear.Contains(GUID))
                {
                    WorldServiceLocator._WorldServer.CHARACTERs[plGUID].guidsForRemoving_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                    WorldServiceLocator._WorldServer.CHARACTERs[plGUID].guidsForRemoving.Add(GUID);
                    WorldServiceLocator._WorldServer.CHARACTERs[plGUID].guidsForRemoving_Lock.ReleaseWriterLock();
                    WorldServiceLocator._WorldServer.CHARACTERs[plGUID].gameObjectsNear.Remove(GUID);
                    SeenBy.Remove(plGUID);
                }
            }
        }

        public void TeleportTransport(uint NewMap, float PosX, float PosY, float PosZ)
        {
            var oldMap = MapID;
            var tmpArray = Passengers.ToArray();
            var array = tmpArray;
            foreach (var tmpUnit in array)
            {
                try
                {
                    if (tmpUnit == null)
                    {
                        lock (Passengers)
                        {
                            Passengers.Remove(tmpUnit);
                        }
                        continue;
                    }
                    if (tmpUnit.IsDead)
                    {
                        switch (tmpUnit)
                        {
                            case WS_PlayerData.CharacterObject _:
                                {
                                    var wS_Handlers_Misc = WorldServiceLocator._WS_Handlers_Misc;
                                    WS_PlayerData.CharacterObject Character = (WS_PlayerData.CharacterObject)tmpUnit;
                                    wS_Handlers_Misc.CharacterResurrect(ref Character);
                                    break;
                                }

                            default:
                                if (tmpUnit is not WS_Creatures.CreatureObject)
                                {
                                }

                                break;
                        }
                    }
                    switch (tmpUnit)
                    {
                        case WS_PlayerData.CharacterObject _:
                            if (((WS_PlayerData.CharacterObject)tmpUnit).OnTransport != null && ((WS_PlayerData.CharacterObject)tmpUnit).OnTransport == this)
                            {
                                ((WS_PlayerData.CharacterObject)tmpUnit).Teleport(PosX, PosY, PosZ, ((WS_PlayerData.CharacterObject)tmpUnit).orientation, checked((int)NewMap));
                                continue;
                            }
                            lock (Passengers)
                            {
                                Passengers.Remove(tmpUnit);
                            }

                            break;

                        case WS_Creatures.CreatureObject _:
                            ((WS_Creatures.CreatureObject)tmpUnit).positionX = PosX;
                            ((WS_Creatures.CreatureObject)tmpUnit).positionY = PosY;
                            ((WS_Creatures.CreatureObject)tmpUnit).positionZ = PosZ;
                            ((WS_Creatures.CreatureObject)tmpUnit).MapID = MapID;
                            break;
                    }
                }
                catch (Exception ex2)
                {
                    ProjectData.SetProjectError(ex2);
                    var ex = ex2;
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Failed to transfer player [0x{0:X}].{1}{2}", tmpUnit.GUID, Environment.NewLine, ex.ToString());
                    ProjectData.ClearProjectError();
                }
            }
            MapID = NewMap;
            positionX = PosX;
            positionY = PosY;
            positionZ = PosZ;
            if (NewMap != oldMap)
            {
                NotifyLeave();
                CheckCell(Teleported: true);
            }
        }

        public override void FillAllUpdateFlags(ref Packets.UpdateClass Update, ref WS_PlayerData.CharacterObject Character)
        {
            Update.SetUpdateFlag(0, GUID);
            Update.SetUpdateFlag(2, 33);
            Update.SetUpdateFlag(3, ID);
            Update.SetUpdateFlag(4, Size);
            Update.SetUpdateFlag(15, positionX);
            Update.SetUpdateFlag(16, positionY);
            Update.SetUpdateFlag(17, positionZ);
            Update.SetUpdateFlag(14, 0, (byte)State);
            Update.SetUpdateFlag(21, (int)Type);
            Update.SetUpdateFlag(20, Faction);
            Update.SetUpdateFlag(9, Flags);
            Update.SetUpdateFlag(8, ObjectInfo.Model);
        }
    }

    private ulong GetNewGUID()
    {
        ref var transportGUIDCounter = ref WorldServiceLocator._WorldServer.TransportGUIDCounter;
        transportGUIDCounter = Convert.ToUInt64(decimal.Add(new decimal(transportGUIDCounter), 1m));
        return WorldServiceLocator._WorldServer.TransportGUIDCounter;
    }

    public void LoadTransports()
    {
        try
        {
            DataTable TransportQuery = new();
            WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT * FROM transports", ref TransportQuery);
            IEnumerator enumerator = default;
            try
            {
                enumerator = TransportQuery.Rows.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    DataRow row = (DataRow)enumerator.Current;
                    var TransportEntry = row.As<int>("entry");
                    var TransportName = row.As<string>("name");
                    var TransportPeriod = row.As<int>("period");
                    TransportObject newTransport = new(TransportEntry, TransportName, TransportPeriod);
                }
            }
            finally
            {
                if (enumerator is IDisposable)
                {
                    (enumerator as IDisposable).Dispose();
                }
            }
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Database: {0} Transports initialized.", TransportQuery.Rows.Count);
        }
        catch (DirectoryNotFoundException ex)
        {
            ProjectData.SetProjectError(ex);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Database : TransportQuery missing.");
            Console.ForegroundColor = ConsoleColor.Gray;
            ProjectData.ClearProjectError();
        }
    }
}
