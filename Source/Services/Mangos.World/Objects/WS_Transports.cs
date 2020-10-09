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
using System.Data;
using System.Threading;
using Mangos.Common.Enums.GameObject;
using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Player;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Objects
{
    public class WS_Transports
    {
        private ulong GetNewGUID()
        {
            ulong GetNewGUIDRet = default;
            WorldServiceLocator._WorldServer.TransportGUIDCounter = (ulong)(WorldServiceLocator._WorldServer.TransportGUIDCounter + 1m);
            GetNewGUIDRet = WorldServiceLocator._WorldServer.TransportGUIDCounter;
            return GetNewGUIDRet;
        }

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public void LoadTransports()
        {
            try
            {
                var TransportQuery = new DataTable();
                WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT * FROM transports", TransportQuery);
                foreach (DataRow Transport in TransportQuery.Rows)
                {
                    int TransportEntry = Conversions.ToInteger(Transport["entry"]);
                    string TransportName = Conversions.ToString(Transport["name"]);
                    int TransportPeriod = Conversions.ToInteger(Transport["period"]);
                    var newTransport = new TransportObject(TransportEntry, TransportName, TransportPeriod);
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Database: {0} Transports initialized.", TransportQuery.Rows.Count);
            }
            catch (System.IO.DirectoryNotFoundException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Database : TransportQuery missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
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
                DistSinceStop = -1.0f;
                DistUntilStop = -1.0f;
                DistSinceStop = -1.0f;
                tFrom = 0.0f;
                tTo = 0.0f;
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
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        public class TransportObject : WS_GameObjects.GameObjectObject, IDisposable
        {
            public string TransportName = "";
            private readonly List<WS_Base.BaseUnit> Passengers = new List<WS_Base.BaseUnit>();
            private readonly List<TransportWP> Waypoints = new List<TransportWP>();
            private readonly int Period = 0;
            private int PathTime = 0;
            private int FirstStop = -1;
            private int LastStop = -1;
            private int CurrentWaypoint = 0;
            private int NextWaypoint = 0;
            private int NextNodeTime = 0;
            private readonly int TimeToNextEvent = 0;
            private readonly TransportStates TransportState = TransportStates.TRANSPORT_DOCKED;
            private readonly byte TransportAt = 0;

            public TransportObject(int ID_, string Name, int Period_) : base(ID_, WorldServiceLocator._WS_Transports.GetNewGUID())
            {

                // TODO: Only handle transports on the map(s) this server handles

                TransportName = Name;
                Period = Period_;
                if (!GenerateWaypoints()) // Check if we want to use this transport on this server
                {
                    return;
                }

                positionX = Waypoints[0].X;
                positionY = Waypoints[0].Y;
                positionZ = Waypoints[0].Z;
                MapID = Waypoints[0].MapID;
                orientation = 1f;
                VisibleDistance = 99999.0f; // Transports are always visible
                State = GameObjectLootState.DOOR_CLOSED;
                TransportState = TransportStates.TRANSPORT_DOCKED;
                TimeToNextEvent = 60000;
                WorldServiceLocator._WorldServer.WORLD_TRANSPORTs_Lock.AcquireWriterLock(Timeout.Infinite);
                WorldServiceLocator._WorldServer.WORLD_TRANSPORTs.Add(GUID, this);
                WorldServiceLocator._WorldServer.WORLD_TRANSPORTs_Lock.ReleaseWriterLock();

                // Update the transport so that we get it's current position
                Update();
            }

            public bool GenerateWaypoints()
            {
                int PathID = (int)get_Sound(0);
                float ShipSpeed = get_Sound(1);
                if (WorldServiceLocator._WS_DBCDatabase.TaxiPaths.ContainsKey(PathID) == false)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "An transport [{0} - {1}] is created with an invalid TaxiPath.", ID, TransportName);
                    return false;
                }

                int MapsUsed = 0;
                int MapChange = 0;
                var PathPoints = new List<TransportMove>();
                int t = 0;
                if (WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes.ContainsKey(PathID) == true)
                {
                    for (int i = 0, loopTo = WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes[PathID].Count - 2; i <= loopTo; i++)
                    {
                        if (MapChange == 0)
                        {
                            if (WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes[PathID].ContainsKey(i) == true & WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes[PathID].ContainsKey(i + 1) == true)
                            {
                                if (WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes[PathID][i].MapID == WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes[PathID][i + 1].MapID)
                                {
                                    PathPoints.Add(new TransportMove(WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes[PathID][i].x, WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes[PathID][i].y, WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes[PathID][i].z, (uint)WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes[PathID][i].MapID, WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes[PathID][i].action, WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes[PathID][i].waittime));
                                    if (WorldServiceLocator._WS_Maps.Maps.ContainsKey((uint)WorldServiceLocator._WS_DBCDatabase.TaxiPathNodes[PathID][i].MapID))
                                    {
                                        MapsUsed += 1;
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
                        return false; // No maps for this transport is used on this server
                    PathPoints[0].DistFromPrev = 0.0f;
                    if (PathPoints[0].ActionFlag == 2)
                    {
                        LastStop = 0;
                    }

                    for (int i = 1, loopTo1 = PathPoints.Count - 1; i <= loopTo1; i++)
                    {
                        if (PathPoints[i].ActionFlag == 1 || PathPoints[i].MapID != PathPoints[i - 1].MapID)
                        {
                            PathPoints[i].DistFromPrev = 0.0f;
                        }
                        else
                        {
                            PathPoints[i].DistFromPrev = WorldServiceLocator._WS_Combat.GetDistance(PathPoints[i].X, PathPoints[i - 1].X, PathPoints[i].Y, PathPoints[i - 1].Y, PathPoints[i].Z, PathPoints[i - 1].Z);
                        }

                        if (PathPoints[i].ActionFlag == 2)
                        {
                            if (FirstStop == -1)
                            {
                                FirstStop = i;
                            }

                            LastStop = i;
                        }
                    }

                    float tmpDist = 0.0f;
                    int j;
                    for (int i = 0, loopTo2 = PathPoints.Count - 1; i <= loopTo2; i++)
                    {
                        j = (i + LastStop) % PathPoints.Count;
                        if (j >= 0)
                        {
                            if (PathPoints[j].ActionFlag == 2)
                            {
                                tmpDist = 0.0f;
                            }
                            else
                            {
                                tmpDist += PathPoints[j].DistFromPrev;
                            }

                            PathPoints[j].DistSinceStop = tmpDist;
                        }
                    }

                    for (int i = PathPoints.Count - 1; i >= 0; i -= 1)
                    {
                        j = (i + FirstStop + 1) % PathPoints.Count;
                        tmpDist += PathPoints[(j + 1) % PathPoints.Count].DistFromPrev;
                        PathPoints[j].DistUntilStop = tmpDist;
                        if (PathPoints[j].ActionFlag == 2)
                            tmpDist = 0.0f;
                    }

                    for (int i = 0, loopTo3 = PathPoints.Count - 1; i <= loopTo3; i++)
                    {
                        if (PathPoints[i].DistSinceStop < 30.0f * 30.0f * 0.5f)
                        {
                            PathPoints[i].tFrom = (float)Math.Sqrt(2.0f * PathPoints[i].DistSinceStop);
                        }
                        else
                        {
                            PathPoints[i].tFrom = (PathPoints[i].DistSinceStop - 30.0f * 30.0f * 0.5f) / 30.0f + 30.0f;
                        }

                        if (PathPoints[i].DistUntilStop < 30.0f * 30.0f * 0.5f)
                        {
                            PathPoints[i].tTo = (float)Math.Sqrt(2.0f * PathPoints[i].DistUntilStop);
                        }
                        else
                        {
                            PathPoints[i].tTo = (PathPoints[i].DistUntilStop - 30.0f * 30.0f * 0.5f) / 30.0f + 30.0f;
                        }

                        PathPoints[i].tFrom *= 1000.0f;
                        PathPoints[i].tTo *= 1000.0f;
                    }

                    bool teleport = false;
                    if (PathPoints[PathPoints.Count - 1].MapID != PathPoints[0].MapID)
                        teleport = true;
                    Waypoints.Add(new TransportWP(0, 0, PathPoints[0].X, PathPoints[0].Y, PathPoints[0].Z, PathPoints[0].MapID, teleport));
                    t += PathPoints[0].Delay * 1000;
                    uint cM = PathPoints[0].MapID;
                    for (int i = 0, loopTo4 = PathPoints.Count - 2; i <= loopTo4; i++)
                    {
                        float d = 0.0f;
                        float tFrom = PathPoints[i].tFrom;
                        float tTo = PathPoints[i].tTo;
                        if (d < PathPoints[i + 1].DistFromPrev && tTo > 0f)
                        {
                            while (d < PathPoints[i + 1].DistFromPrev && tTo > 0f)
                            {
                                tFrom += 100.0f;
                                tTo -= 100.0f;
                                if (d > 0f)
                                {
                                    float newX = PathPoints[i].X + (PathPoints[i + 1].X - PathPoints[i].X) * d / PathPoints[i + 1].DistFromPrev;
                                    float newY = PathPoints[i].Y + (PathPoints[i + 1].Y - PathPoints[i].Y) * d / PathPoints[i + 1].DistFromPrev;
                                    float newZ = PathPoints[i].Z + (PathPoints[i + 1].Z - PathPoints[i].Z) * d / PathPoints[i + 1].DistFromPrev;
                                    teleport = false;
                                    if (PathPoints[i].MapID != cM)
                                    {
                                        teleport = true;
                                        cM = PathPoints[i].MapID;
                                    }

                                    if (teleport)
                                    {
                                        Waypoints.Add(new TransportWP(i, t, newX, newY, newZ, PathPoints[i].MapID, teleport));
                                    }
                                }

                                if (tFrom < tTo)
                                {
                                    if (tFrom <= 30000.0f)
                                    {
                                        d = 0.5f * (tFrom / 1000.0f) * (tFrom / 1000.0f);
                                    }
                                    else
                                    {
                                        d = 0.5f * 30.0f * 30.0f + 30.0f * ((tFrom - 30000.0f) / 1000.0f);
                                    }

                                    d -= PathPoints[i].DistSinceStop;
                                }
                                else
                                {
                                    if (tTo <= 30000.0f)
                                    {
                                        d = 0.5f * (tTo / 1000.0f) * (tTo / 1000.0f);
                                    }
                                    else
                                    {
                                        d = 0.5f * 30.0f * 30.0f + 30.0f * ((tTo - 30000.0f) / 1000.0f);
                                    }

                                    d = PathPoints[i].DistUntilStop - d;
                                }

                                t += 100;
                            }

                            t -= 100;
                        }

                        if (PathPoints[i + 1].tFrom > PathPoints[i + 1].tTo)
                        {
                            t = (int)(t + (100L - (long)Conversion.Fix(PathPoints[i + 1].tTo) % 100L));
                        }
                        else
                        {
                            t = (int)(t + (long)Conversion.Fix(PathPoints[i + 1].tTo) % 100L);
                        }

                        teleport = false;
                        if (PathPoints[i + 1].ActionFlag == 1 || PathPoints[i + 1].MapID != PathPoints[i].MapID)
                        {
                            teleport = true;
                            cM = PathPoints[i + 1].MapID;
                        }

                        Waypoints.Add(new TransportWP(i, t, PathPoints[i + 1].X, PathPoints[i + 1].Y, PathPoints[i + 1].Z, PathPoints[i + 1].MapID, teleport));
                        t += PathPoints[i + 1].Delay * 1000;
                    }

                    CurrentWaypoint = 0;
                    CurrentWaypoint = GetNextWaypoint();
                    NextWaypoint = GetNextWaypoint();
                    PathTime = t;
                    NextNodeTime = Waypoints[CurrentWaypoint].Time;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public int GetNextWaypoint()
            {
                int tmpWP = CurrentWaypoint;
                tmpWP += 1;
                if (tmpWP >= Waypoints.Count)
                    tmpWP = 0;
                return tmpWP;
            }

            public void AddPassenger(ref WS_Base.BaseUnit Unit)
            {
                if (Passengers.Contains(Unit))
                    return;
                lock (Passengers)
                    Passengers.Add(Unit);
            }

            public void RemovePassenger(ref WS_Base.BaseUnit Unit)
            {
                if (Passengers.Contains(Unit) == false)
                    return;
                lock (Passengers)
                    Passengers.Remove(Unit);
            }

            public void Update()
            {
                if (Waypoints.Count <= 1)
                    return;
                int Timer = WorldServiceLocator._WS_Network.MsTime() % Period;
                while (Math.Abs(Timer - Waypoints[CurrentWaypoint].Time) % PathTime > Math.Abs(Waypoints[NextWaypoint].Time - Waypoints[CurrentWaypoint].Time) % PathTime)
                {
                    CurrentWaypoint = GetNextWaypoint();
                    NextWaypoint = GetNextWaypoint();
                    if (Waypoints[CurrentWaypoint].MapID != MapID || Waypoints[CurrentWaypoint].Teleport)
                    {
                        // Teleport transport
                        TeleportTransport(Waypoints[CurrentWaypoint].MapID, Waypoints[CurrentWaypoint].X, Waypoints[CurrentWaypoint].Y, Waypoints[CurrentWaypoint].Z);
                    }
                    else
                    {
                        // Relocate teleport
                        positionX = Waypoints[CurrentWaypoint].X;
                        positionY = Waypoints[CurrentWaypoint].Y;
                        positionZ = Waypoints[CurrentWaypoint].Z;
                        CheckCell();
                    }

                    if (CurrentWaypoint == FirstStop || CurrentWaypoint == LastStop)
                    {
                        switch (ID)
                        {
                            case 176495:
                            case 164871:
                            case 175080:
                                {
                                    SendPlaySound(5154); // ZeppelinDocked
                                    break;
                                }

                            case 20808:
                            case 181646:
                            case 176231:
                            case 176244:
                            case 176310:
                            case 177233:
                                {
                                    SendPlaySound(5495); // BoatDockingWarning
                                    break;
                                }

                            default:
                                {
                                    SendPlaySound(5154); // ShipDocked
                                    break;
                                }
                        }
                    }

                    NextNodeTime = Waypoints[CurrentWaypoint].Time;
                    // TODO: This line aborts the infinite loop sometimes created, all 8 transports wil need to be checked
                    if (CurrentWaypoint == 0)
                        break;
                }
            }

            public void CreateEveryoneOnTransport(ref WS_PlayerData.CharacterObject Character)
            {
                // Create an update packet for you only one time, more effecient :)
                var mePacket = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                mePacket.AddInt32(1);
                mePacket.AddInt8(0);
                var meTmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
                Character.FillAllUpdateFlags(ref meTmpUpdate);
                meTmpUpdate.AddToPacket(mePacket, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, Character);
                meTmpUpdate.Dispose();
                mePacket.CompressUpdatePacket();
                var tmpArray = Passengers.ToArray();
                foreach (WS_Base.BaseUnit tmpUnit in tmpArray)
                {
                    if (tmpUnit is null)
                        continue;
                    if (tmpUnit is WS_PlayerData.CharacterObject) // If the passenger is a player
                    {
                        WS_Base.BaseObject argobjCharacter = tmpUnit;
                        if (Character.CanSee(ref argobjCharacter)) // If you can see player
                        {
                            var myPacket = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                            try
                            {
                                myPacket.AddInt32(1);
                                myPacket.AddInt8(0);
                                var myTmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
                                ((WS_PlayerData.CharacterObject)tmpUnit).FillAllUpdateFlags(ref myTmpUpdate);
                                myTmpUpdate.AddToPacket(myPacket, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, (WS_PlayerData.CharacterObject)tmpUnit);
                                myTmpUpdate.Dispose();
                                Character.client.Send(ref myPacket);
                            }
                            finally
                            {
                                myPacket.Dispose();
                            } ((WS_PlayerData.CharacterObject)tmpUnit).SeenBy.Add(Character.GUID);
                            Character.playersNear.Add(tmpUnit.GUID);
                        }

                        WS_Base.BaseObject argobjCharacter1 = Character;
                        if (((WS_PlayerData.CharacterObject)tmpUnit).CanSee(ref argobjCharacter1)) // If player can see you
                        {
                            ((WS_PlayerData.CharacterObject)tmpUnit).client.SendMultiplyPackets(ref mePacket);
                            Character.SeenBy.Add(tmpUnit.GUID);
                            ((WS_PlayerData.CharacterObject)tmpUnit).playersNear.Add(Character.GUID);
                        }
                    }
                    else if (tmpUnit is WS_Creatures.CreatureObject) // If the passenger is a creature
                    {
                        WS_Base.BaseObject argobjCharacter2 = tmpUnit;
                        if (Character.CanSee(ref argobjCharacter2)) // If you can see creature
                        {
                            var myPacket = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                            try
                            {
                                myPacket.AddInt32(1);
                                myPacket.AddInt8(0);
                                var myTmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_UNIT);
                                ((WS_Creatures.CreatureObject)tmpUnit).FillAllUpdateFlags(ref myTmpUpdate);
                                myTmpUpdate.AddToPacket(myPacket, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, (WS_Creatures.CreatureObject)tmpUnit);
                                myTmpUpdate.Dispose();
                                Character.client.Send(ref myPacket);
                            }
                            finally
                            {
                                myPacket.Dispose();
                            } ((WS_PlayerData.CharacterObject)tmpUnit).SeenBy.Add(Character.GUID);
                            Character.creaturesNear.Add(tmpUnit.GUID);
                        }
                    }
                }

                mePacket.Dispose();
            }

            public void CheckCell(bool Teleported = false)
            {
                var TileX = default(byte);
                var TileY = default(byte);
                WorldServiceLocator._WS_Maps.GetMapTile(positionX, positionY, ref TileX, ref TileY);
                if (Teleported || CellX != TileX || CellY != TileY)
                {
                    if (WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY] is object)
                    {
                        try
                        {
                            WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY].GameObjectsHere.Remove(GUID);
                        }
                        catch
                        {
                        }
                    }

                    CellX = TileX;
                    CellY = TileY;
                    if (WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY] is object)
                    {
                        try
                        {
                            WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY].GameObjectsHere.Add(GUID);
                        }
                        catch
                        {
                        }
                    }

                    NotifyEnter();
                }
            }

            public void NotifyEnter()
            {
                // DONE: Sending to players in nearby cells
                ulong[] list;
                for (short i = -1; i <= 1; i++)
                {
                    for (short j = -1; j <= 1; j++)
                    {
                        if (CellX + i >= 0 && CellX + i <= 63 && CellY + j >= 0 && CellY + j <= 63 && WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX + i, CellY + j] is object && WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX + i, CellY + j].PlayersHere.Count > 0)
                        {
                            {
                                var withBlock = WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX + i, CellY + j];
                                list = withBlock.PlayersHere.ToArray();
                                foreach (ulong plGUID in list)
                                {
                                    WS_Base.BaseObject argobjCharacter = this;
                                    if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(plGUID) && WorldServiceLocator._WorldServer.CHARACTERs[plGUID].CanSee(ref argobjCharacter))
                                    {
                                        var packet = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                                        try
                                        {
                                            packet.AddInt32(1);
                                            packet.AddInt8(0);
                                            var tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_GAMEOBJECT);
                                            try
                                            {
                                                var tmp = WorldServiceLocator._WorldServer.CHARACTERs;
                                                var argCharacter = tmp[plGUID];
                                                FillAllUpdateFlags(ref tmpUpdate, ref argCharacter);
                                                tmp[plGUID] = argCharacter;
                                                tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, this);
                                            }
                                            finally
                                            {
                                                tmpUpdate.Dispose();
                                            }

                                            WorldServiceLocator._WorldServer.CHARACTERs[plGUID].client.SendMultiplyPackets(ref packet);
                                            WorldServiceLocator._WorldServer.CHARACTERs[plGUID].gameObjectsNear.Add(GUID);
                                            SeenBy.Add(plGUID);
                                        }
                                        finally
                                        {
                                            packet.Dispose();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            public void NotifyLeave()
            {
                // DONE: Removing from players that can see the object
                foreach (ulong plGUID in SeenBy.ToArray())
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
                uint oldMap = MapID;
                var tmpArray = Passengers.ToArray();
                foreach (WS_Base.BaseUnit tmpUnit in tmpArray)
                {
                    try
                    {
                        // Remove passengers that doesn't exist anymore
                        if (tmpUnit is null)
                        {
                            lock (Passengers)
                                Passengers.Remove(tmpUnit);
                            continue;
                        }

                        if (tmpUnit.IsDead)
                        {
                            if (tmpUnit is WS_PlayerData.CharacterObject)
                            {
                                WS_PlayerData.CharacterObject argCharacter = (WS_PlayerData.CharacterObject)tmpUnit;
                                WorldServiceLocator._WS_Handlers_Misc.CharacterResurrect(ref argCharacter);
                            }
                            else if (tmpUnit is WS_Creatures.CreatureObject)
                            {
                                // TODO!
                            }
                        }

                        if (tmpUnit is WS_PlayerData.CharacterObject)
                        {
                            if (((WS_PlayerData.CharacterObject)tmpUnit).OnTransport is object && ReferenceEquals(((WS_PlayerData.CharacterObject)tmpUnit).OnTransport, this))
                            {
                                ((WS_PlayerData.CharacterObject)tmpUnit).Teleport(PosX, PosY, PosZ, ((WS_PlayerData.CharacterObject)tmpUnit).orientation, (int)NewMap);
                            }
                            else
                            {
                                // Remove players no longer on this transport
                                lock (Passengers)
                                    Passengers.Remove(tmpUnit);
                            }
                        }
                        else if (tmpUnit is WS_Creatures.CreatureObject)
                        {
                            ((WS_Creatures.CreatureObject)tmpUnit).positionX = PosX;
                            ((WS_Creatures.CreatureObject)tmpUnit).positionY = PosY;
                            ((WS_Creatures.CreatureObject)tmpUnit).positionZ = PosZ;
                            ((WS_Creatures.CreatureObject)tmpUnit).MapID = MapID;
                            // TODO: What more?
                        }
                    }
                    catch (Exception ex)
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Failed to transfer player [0x{0:X}].{1}{2}", tmpUnit.GUID, Environment.NewLine, ex.ToString());
                    }
                }

                MapID = NewMap;
                positionX = PosX;
                positionY = PosY;
                positionZ = PosZ;
                if (NewMap != oldMap)
                {
                    NotifyLeave();
                    CheckCell(true);
                }
            }

            public override void FillAllUpdateFlags(ref Packets.UpdateClass Update, ref WS_PlayerData.CharacterObject Character)
            {
                Update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_GUID, GUID);
                Update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_TYPE, ObjectType.TYPE_GAMEOBJECT + ObjectType.TYPE_OBJECT);
                Update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_ENTRY, ID);
                Update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_SCALE_X, Size);
                Update.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_POS_X, positionX);
                Update.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_POS_Y, positionY);
                Update.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_POS_Z, positionZ);
                // Update.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_FACING, orientation)

                Update.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_STATE, 0, State);
                Update.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_TYPE_ID, Type);
                Update.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_FACTION, Faction);
                Update.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_FLAGS, Flags);
                Update.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_DISPLAYID, ObjectInfo.Model);
                // Update.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_ROTATION, Rotations(0))
                // Update.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_ROTATION + 1, Rotations(1))
                // Update.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_ROTATION + 2, Rotations(2))
                // Update.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_ROTATION + 3, Rotations(3))

                // Update.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_TIMESTAMP, msTime) ' Changed in 1.12.x client branch?
            }
        }
    }
}