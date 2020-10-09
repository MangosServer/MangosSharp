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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Mangos.Common.Enums.GameObject;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Spell;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Loots;
using Mangos.World.Player;
using Mangos.World.Server;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Objects
{
    public class WS_GameObjects
    {

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        // WARNING: Use only with _WorldServer.GAMEOBJECTSDatabase()
        public class GameObjectInfo : IDisposable
        {
            public int ID = 0;
            public int Model = 0;
            public GameObjectType Type = 0;
            public string Name = "";
            public short Faction = 0;
            public int Flags = 0;
            public float Size = 1f;
            public uint[] Fields = new uint[24];
            public string ScriptName = "";
            private readonly bool found_ = false;

            public GameObjectInfo(int ID_)
            {
                ID = ID_;
                WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase.Add(ID, this);
                var MySQLQuery = new DataTable();
                WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT * FROM gameobject_template WHERE entry = {0};", (object)ID_), MySQLQuery);
                if (MySQLQuery.Rows.Count == 0)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "gameobject_template {0} not found in SQL database!", ID_);
                    found_ = false;
                    return;
                }

                found_ = true;
                Model = Conversions.ToInteger(MySQLQuery.Rows[0]["displayId"]);
                Type = MySQLQuery.Rows[0]["type"];
                Name = Conversions.ToString(MySQLQuery.Rows[0]["name"]);
                Faction = Conversions.ToShort(MySQLQuery.Rows[0]["faction"]);
                Flags = Conversions.ToInteger(MySQLQuery.Rows[0]["flags"]);
                Size = Conversions.ToSingle(MySQLQuery.Rows[0]["size"]);
                for (byte i = 0; i <= 23; i++)
                    Fields[i] = Conversions.ToUInteger(MySQLQuery.Rows[0]["data" + i]);

                // TODO: Need to load the scriptname of script_bindings
                // ScriptName = MySQLQuery.Rows(0).Item("ScriptName")
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
                    WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase.Remove(ID);
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
        }
        // WARNING: Use only with _WorldServer.WORLD_GAMEOBJECTs()
        public class GameObjectObject : WS_Base.BaseObject, IDisposable
        {
            public int ID = 0;
            public int Flags = 0;
            public float Size = 1f;
            public int Faction = 0;
            public GameObjectLootState State = GameObjectLootState.LOOT_UNLOOTED;
            public float[] Rotations = new float[] { 0f, 0f, 0f, 0f };
            public ulong Owner;
            public WS_Loot.LootObject Loot = null;
            public bool Despawned = false;
            public int MineRemaining = 0;
            public int AnimProgress = 0;
            public int SpawnTime = 0;
            public int GameEvent = 0;
            public int CreatedBySpell = 0;
            public int Level = 0;
            private bool ToDespawn = false;
            public List<int> IncludesQuestItems = new List<int>();
            private Timer RespawnTimer = null;

            public GameObjectInfo ObjectInfo
            {
                get
                {
                    return WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase[ID];
                }
            }

            public string Name
            {
                get
                {
                    return ObjectInfo.Name;
                }
            }

            public GameObjectType Type
            {
                get
                {
                    return ObjectInfo.Type;
                }
            }

            public uint get_Sound(byte Index)
            {
                return ObjectInfo.Fields[Index];
            }

            public bool IsUsedForQuests
            {
                get
                {
                    return IncludesQuestItems.Count > 0;
                }
            }

            public int LockID
            {
                get
                {
                    switch (ObjectInfo.Type)
                    {
                        case var @case when @case == GameObjectType.GAMEOBJECT_TYPE_DOOR:
                            {
                                return (int)get_Sound(1);
                            }

                        case var case1 when case1 == GameObjectType.GAMEOBJECT_TYPE_BUTTON:
                            {
                                return (int)get_Sound(1);
                            }

                        case var case2 when case2 == GameObjectType.GAMEOBJECT_TYPE_QUESTGIVER:
                            {
                                return (int)get_Sound(0);
                            }

                        case var case3 when case3 == GameObjectType.GAMEOBJECT_TYPE_CHEST:
                            {
                                return (int)get_Sound(0);
                            }

                        case var case4 when case4 == GameObjectType.GAMEOBJECT_TYPE_TRAP:
                            {
                                return (int)get_Sound(0);
                            }

                        case var case5 when case5 == GameObjectType.GAMEOBJECT_TYPE_GOOBER:
                            {
                                return (int)get_Sound(0);
                            }

                        case var case6 when case6 == GameObjectType.GAMEOBJECT_TYPE_AREADAMAGE:
                            {
                                return (int)get_Sound(0);
                            }

                        case var case7 when case7 == GameObjectType.GAMEOBJECT_TYPE_CAMERA:
                            {
                                return (int)get_Sound(0);
                            }

                        case var case8 when case8 == GameObjectType.GAMEOBJECT_TYPE_FLAGSTAND:
                            {
                                return (int)get_Sound(0);
                            }

                        case var case9 when case9 == GameObjectType.GAMEOBJECT_TYPE_FISHINGHOLE:
                            {
                                return (int)get_Sound(4);
                            }

                        case var case10 when case10 == GameObjectType.GAMEOBJECT_TYPE_FLAGDROP:
                            {
                                return (int)get_Sound(0);
                            }

                        default:
                            {
                                return 0;
                            }
                    }
                }
            }

            public int LootID
            {
                get
                {
                    switch (ObjectInfo.Type)
                    {
                        case var @case when @case == GameObjectType.GAMEOBJECT_TYPE_CHEST:
                            {
                                return (int)get_Sound(1);
                            }

                        case var case1 when case1 == GameObjectType.GAMEOBJECT_TYPE_FISHINGNODE:
                            {
                                return (int)get_Sound(1);
                            }

                        case var case2 when case2 == GameObjectType.GAMEOBJECT_TYPE_FISHINGHOLE:
                            {
                                return (int)get_Sound(1);
                            }

                        default:
                            {
                                return 0;
                            }
                    }
                }
            }

            public int AutoCloseTime
            {
                get
                {
                    switch (ObjectInfo.Type)
                    {
                        case var @case when @case == GameObjectType.GAMEOBJECT_TYPE_DOOR:
                            {
                                return (int)(get_Sound(2) / 0x10000d * 1000d);
                            }

                        case var case1 when case1 == GameObjectType.GAMEOBJECT_TYPE_BUTTON:
                            {
                                return (int)(get_Sound(2) / 0x10000d * 1000d);
                            }

                        case var case2 when case2 == GameObjectType.GAMEOBJECT_TYPE_TRAP:
                            {
                                return (int)(get_Sound(6) / 0x10000d * 1000d);
                            }

                        case var case3 when case3 == GameObjectType.GAMEOBJECT_TYPE_GOOBER:
                            {
                                return (int)(get_Sound(3) / 0x10000d * 1000d);
                            }

                        case var case4 when case4 == GameObjectType.GAMEOBJECT_TYPE_TRANSPORT:
                            {
                                return (int)(get_Sound(2) / 0x10000d * 1000d);
                            }

                        case var case5 when case5 == GameObjectType.GAMEOBJECT_TYPE_AREADAMAGE:
                            {
                                return (int)(get_Sound(5) / 0x10000d * 1000d);
                            }

                        default:
                            {
                                return 0;
                            }
                    }
                }
            }

            public bool IsConsumeable
            {
                get
                {
                    switch (ObjectInfo.Type)
                    {
                        case var @case when @case == GameObjectType.GAMEOBJECT_TYPE_CHEST:
                            {
                                return get_Sound(3) == 1L;
                            }

                        default:
                            {
                                return false;
                            }
                    }
                }
            }

            public virtual void FillAllUpdateFlags(ref Packets.UpdateClass Update, ref WS_PlayerData.CharacterObject Character)
            {
                Update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_GUID, GUID);
                Update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_TYPE, ObjectType.TYPE_GAMEOBJECT + ObjectType.TYPE_OBJECT);
                Update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_ENTRY, ID);
                Update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_SCALE_X, Size);
                if (Owner > 0UL)
                    Update.SetUpdateFlag(EGameObjectFields.OBJECT_FIELD_CREATED_BY, Owner);
                Update.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_POS_X, positionX);
                Update.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_POS_Y, positionY);
                Update.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_POS_Z, positionZ);
                Update.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_FACING, orientation);
                long Rotation = 0L;
                float f_rot1 = (float)Math.Sin(orientation / 2f);
                long i_rot1 = (long)(f_rot1 / Math.Atan(Math.Pow(2d, -20)));
                Rotation = Rotation | i_rot1 << 43 >> 43 & 0x1FFFFFL;
                Update.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_ROTATION, Rotation);

                // If a game object has bit 4 set in the flag it needs to be activated (used for quests)
                // DynFlags = Activate a game object (Chest = 9, Goober = 1)
                int DynFlags = 0;
                if (Type == GameObjectType.GAMEOBJECT_TYPE_CHEST)
                {
                    var arggameobject = this;
                    byte UsedForQuest = WorldServiceLocator._WorldServer.ALLQUESTS.IsGameObjectUsedForQuest(ref arggameobject, ref Character);
                    if (UsedForQuest > 0)
                    {
                        Flags = Flags | 4;
                        if (UsedForQuest == 2)
                        {
                            DynFlags = 9;
                        }
                    }
                }
                else if (Type == GameObjectType.GAMEOBJECT_TYPE_GOOBER)
                {
                    // TODO: Check conditions
                    DynFlags = 1;
                }

                if (Conversions.ToBoolean(DynFlags))
                    Update.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_DYN_FLAGS, DynFlags);
                Update.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_STATE, 0, State);
                Update.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_TYPE_ID, Type);
                if (Level > 0)
                    Update.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_LEVEL, Level);
                Update.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_FACTION, Faction);
                Update.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_FLAGS, Flags);
                Update.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_DISPLAYID, ObjectInfo.Model);
                Update.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_ROTATION, Rotations[0]);
                Update.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_ROTATION + 1, Rotations[1]);
                Update.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_ROTATION + 2, Rotations[2]);
                Update.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_ROTATION + 3, Rotations[3]);
                // Update.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_TIMESTAMP, msTime) ' Changed in 1.12.x client branch?
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
                    RemoveFromWorld();
                    if (Loot is object && Type != GameObjectType.GAMEOBJECT_TYPE_FISHINGNODE)
                        Loot.Dispose();
                    if (this is WS_Transports.TransportObject)
                    {
                        WorldServiceLocator._WorldServer.WORLD_TRANSPORTs_Lock.AcquireWriterLock(Timeout.Infinite);
                        WorldServiceLocator._WorldServer.WORLD_TRANSPORTs.Remove(GUID);
                        WorldServiceLocator._WorldServer.WORLD_TRANSPORTs_Lock.ReleaseWriterLock();
                        RespawnTimer.Dispose();
                    }
                    else
                    {
                        WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.Remove(GUID);
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
            public GameObjectObject(int ID_)
            {
                // WARNING: Use only for spawning new object
                if (!WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase.ContainsKey(ID_))
                {
                    var baseGameObject = new GameObjectInfo(ID_);
                }

                ID = ID_;
                GUID = WorldServiceLocator._WS_GameObjects.GetNewGUID();
                Flags = WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase[ID].Flags;
                Faction = WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase[ID].Faction;
                Size = WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase[ID].Size;
                WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.Add(GUID, this);
            }

            public GameObjectObject(int ID_, ulong GUID_)
            {
                // WARNING: Use only for spawning new object
                if (!WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase.ContainsKey(ID_))
                {
                    var baseGameObject = new GameObjectInfo(ID_);
                }

                ID = ID_;
                GUID = GUID_;
                Flags = WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase[ID].Flags;
                Faction = WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase[ID].Faction;
                Size = WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase[ID].Size;
            }

            public GameObjectObject(int ID_, uint MapID_, float PosX, float PosY, float PosZ, float Rotation, ulong Owner_ = 0UL)
            {
                // WARNING: Use only for spawning new object
                if (!WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase.ContainsKey(ID_))
                {
                    var baseGameObject = new GameObjectInfo(ID_);
                }

                ID = ID_;
                GUID = WorldServiceLocator._WS_GameObjects.GetNewGUID();
                MapID = MapID_;
                positionX = PosX;
                positionY = PosY;
                positionZ = PosZ;
                orientation = Rotation;
                Owner = Owner_;
                Flags = WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase[ID].Flags;
                Faction = WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase[ID].Faction;
                Size = WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase[ID].Size;
                if (Type == GameObjectType.GAMEOBJECT_TYPE_TRANSPORT)
                {
                    VisibleDistance = 99999.0f;
                    State = GameObjectLootState.DOOR_CLOSED;
                }

                WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.Add(GUID, this);
            }

            public GameObjectObject(ulong cGUID, [Optional, DefaultParameterValue(null)] ref DataRow Info)
            {
                // WARNING: Use only for loading from DB
                if (Info is null)
                {
                    var MySQLQuery = new DataTable();
                    WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT * FROM gameobject LEFT OUTER JOIN game_event_gameobject ON gameobject.guid = game_event_gameobject.guid WHERE gameobject.guid = {0};", (object)cGUID), MySQLQuery);
                    if (MySQLQuery.Rows.Count > 0)
                    {
                        Info = MySQLQuery.Rows[0];
                    }
                    else
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "GameObject Spawn not found in database. [cGUID={0:X}]", cGUID);
                        return;
                    }
                }

                positionX = Conversions.ToSingle(Info["position_X"]);
                positionY = Conversions.ToSingle(Info["position_Y"]);
                positionZ = Conversions.ToSingle(Info["position_Z"]);
                orientation = Conversions.ToSingle(Info["orientation"]);
                MapID = Conversions.ToUInteger(Info["map"]);
                Rotations[0] = Conversions.ToSingle(Info["rotation0"]);
                Rotations[1] = Conversions.ToSingle(Info["rotation1"]);
                Rotations[2] = Conversions.ToSingle(Info["rotation2"]);
                Rotations[3] = Conversions.ToSingle(Info["rotation3"]);
                ID = Conversions.ToInteger(Info["id"]);
                AnimProgress = Conversions.ToInteger(Info["animprogress"]);
                SpawnTime = Conversions.ToInteger(Info["spawntimesecs"]);
                State = Info["state"];

                // If Not Info.Item("event") Is DBNull.Value Then
                // GameEvent = Info.Item("event")
                // Else
                // GameEvent = 0
                // End If

                if (!WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase.ContainsKey(ID))
                {
                    var baseGameObject = new GameObjectInfo(ID);
                }

                Flags = WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase[ID].Flags;
                Faction = WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase[ID].Faction;
                Size = WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase[ID].Size;
                if (Type == GameObjectType.GAMEOBJECT_TYPE_TRANSPORT)
                {
                    // State = GameObjectLootState.DOOR_CLOSED
                    VisibleDistance = 99999.0f;
                    GUID = cGUID + WorldServiceLocator._Global_Constants.GUID_TRANSPORT;
                }
                else
                {
                    GUID = cGUID + WorldServiceLocator._Global_Constants.GUID_GAMEOBJECT;
                }

                WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.Add(GUID, this);

                // DONE: If there's a loottable open for this gameobject already then hook it to the gameobject
                if (WorldServiceLocator._WS_Loot.LootTable.ContainsKey(GUID))
                {
                    Loot = WorldServiceLocator._WS_Loot.LootTable[GUID];
                }

                // DONE: Calculate mines remaining
                CalculateMineRemaning(true);
            }

            public void AddToWorld()
            {
                WorldServiceLocator._WS_Maps.GetMapTile(positionX, positionY, ref CellX, ref CellY);
                if (WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY] is null)
                    WorldServiceLocator._WS_CharMovement.MAP_Load(CellX, CellY, MapID);
                try
                {
                    WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY].GameObjectsHere.Add(GUID);
                }
                catch
                {
                    return;
                }

                ulong[] list;

                // DONE: Generate loot at spawn
                if (Type == GameObjectType.GAMEOBJECT_TYPE_CHEST && Loot is null)
                    GenerateLoot();

                // DONE: Sending to players in nearby cells
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
                                        packet.AddInt32(1);
                                        packet.AddInt8(0);
                                        var tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_GAMEOBJECT);
                                        var tmp = WorldServiceLocator._WorldServer.CHARACTERs;
                                        var argCharacter = tmp[plGUID];
                                        FillAllUpdateFlags(ref tmpUpdate, ref argCharacter);
                                        tmp[plGUID] = argCharacter;
                                        tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, this);
                                        tmpUpdate.Dispose();
                                        WorldServiceLocator._WorldServer.CHARACTERs[plGUID].client.SendMultiplyPackets(ref packet);
                                        WorldServiceLocator._WorldServer.CHARACTERs[plGUID].gameObjectsNear.Add(GUID);
                                        SeenBy.Add(plGUID);
                                        packet.Dispose();
                                    }
                                }
                            }
                        }
                    }
                }
            }

            public void RemoveFromWorld()
            {
                if (WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY] is null)
                    return;
                WorldServiceLocator._WS_Maps.GetMapTile(positionX, positionY, ref CellX, ref CellY);
                WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY].GameObjectsHere.Remove(GUID);

                // DONE: Removing from players that can see the object
                foreach (ulong plGUID in SeenBy.ToArray())
                {
                    if (WorldServiceLocator._WorldServer.CHARACTERs[plGUID].gameObjectsNear.Contains(GUID))
                    {
                        WorldServiceLocator._WorldServer.CHARACTERs[plGUID].guidsForRemoving_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                        WorldServiceLocator._WorldServer.CHARACTERs[plGUID].guidsForRemoving.Add(GUID);
                        WorldServiceLocator._WorldServer.CHARACTERs[plGUID].guidsForRemoving_Lock.ReleaseWriterLock();
                        WorldServiceLocator._WorldServer.CHARACTERs[plGUID].gameObjectsNear.Remove(GUID);
                    }
                }
            }

            public void SetState(GameObjectLootState State)
            {
                var packet = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                packet.AddInt32(1);
                packet.AddInt8(0);
                var tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_GAMEOBJECT);
                tmpUpdate.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_STATE, 0, State);
                tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, this);
                tmpUpdate.Dispose();
                SendToNearPlayers(ref packet);
                packet.Dispose();
            }

            public void OpenDoor()
            {
                Flags = Flags | GameObjectFlags.GO_FLAG_IN_USE;
                State = GameObjectLootState.DOOR_OPEN;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "AutoCloseTime: {0}", AutoCloseTime);
                if (AutoCloseTime > 0)
                    ThreadPool.RegisterWaitForSingleObject(new AutoResetEvent(false), new WaitOrTimerCallback(CloseDoor), null, AutoCloseTime, true);
                var packet = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                packet.AddInt32(1);
                packet.AddInt8(0);
                var tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_GAMEOBJECT);
                tmpUpdate.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_FLAGS, Flags);
                tmpUpdate.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_STATE, 0, State);
                tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, this);
                tmpUpdate.Dispose();
                SendToNearPlayers(ref packet);
                packet.Dispose();
            }

            public void CloseDoor(object state, bool timedOut)
            {
                Flags = Flags & !GameObjectFlags.GO_FLAG_IN_USE;
                state = GameObjectLootState.DOOR_CLOSED;
                var packet = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                packet.AddInt32(1);
                packet.AddInt8(0);
                var tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_GAMEOBJECT);
                tmpUpdate.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_FLAGS, Flags);
                tmpUpdate.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_STATE, 0, state);
                tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, this);
                tmpUpdate.Dispose();
                SendToNearPlayers(ref packet);
                packet.Dispose();
            }

            public void LootObject(ref WS_PlayerData.CharacterObject Character, LootType LootingType)
            {
                State = GameObjectLootState.LOOT_LOOTED;
                switch (Type)
                {
                    case var @case when @case == GameObjectType.GAMEOBJECT_TYPE_BUTTON:
                    case var case1 when case1 == GameObjectType.GAMEOBJECT_TYPE_DOOR:
                        {
                            // DONE: Open door
                            OpenDoor();
                            // TODO: Close it again after some sec
                            return;
                        }

                    case var case2 when case2 == GameObjectType.GAMEOBJECT_TYPE_QUESTGIVER:
                        {
                            // TODO: Start or end quest
                            return;
                        }
                }

                if (Loot is null)
                    return;
                Loot.SendLoot(ref Character.client);

                // DONE: So that loot isn't released instantly for gameobject looting
                if (Character.spellCasted[CurrentSpellTypes.CURRENT_GENERIC_SPELL] is object)
                {
                    Character.spellCasted[CurrentSpellTypes.CURRENT_GENERIC_SPELL].State = SpellCastState.SPELL_STATE_FINISHED;
                }
            }

            public bool GenerateLoot()
            {
                if (Loot is object)
                    return true;
                if (LootID == 0)
                    return false;

                // DONE: Loot generation
                Loot = new WS_Loot.LootObject(GUID, LootType.LOOTTYPE_SKINNING);
                var Template = WorldServiceLocator._WS_Loot.LootTemplates_Gameobject.GetLoot(LootID);
                if (Template is object)
                {
                    Template.Process(ref Loot, 0);
                }

                Loot.LootOwner = 0UL;
                return true;
            }

            public void SetupFishingNode()
            {
                int RandomTime = WorldServiceLocator._WorldServer.Rnd.Next(3000, 17000);
                ThreadPool.RegisterWaitForSingleObject(new AutoResetEvent(false), new WaitOrTimerCallback(SetFishHooked), null, RandomTime, true);
                State = GameObjectLootState.DOOR_CLOSED;
            }

            public void SetFishHooked(object state, bool timedOut)
            {
                if (!Operators.ConditionalCompareObjectEqual(state, GameObjectLootState.DOOR_CLOSED, false))
                    return;
                state = GameObjectLootState.DOOR_OPEN;
                Flags = GameObjectFlags.GO_FLAG_NODESPAWN;
                Loot = new WS_Loot.LootObject(GUID, LootType.LOOTTYPE_FISHING) { LootOwner = Owner };
                int AreaFlag = WorldServiceLocator._WS_Maps.GetAreaFlag(positionX, positionY, (int)MapID);
                int AreaID = WorldServiceLocator._WS_Maps.AreaTable[AreaFlag].ID;
                var Template = WorldServiceLocator._WS_Loot.LootTemplates_Fishing.GetLoot(AreaID);
                if (Template is object)
                {
                    Template.Process(ref Loot, 0);
                }

                var packet = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                packet.AddInt32(1);
                packet.AddInt8(0);
                var tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_GAMEOBJECT);
                tmpUpdate.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_FLAGS, Flags);
                tmpUpdate.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_STATE, 0, state);
                tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, this);
                tmpUpdate.Dispose();
                SendToNearPlayers(ref packet);
                packet.Dispose();
                var packetAnim = new Packets.PacketClass(OPCODES.SMSG_GAMEOBJECT_CUSTOM_ANIM);
                packetAnim.AddUInt64(GUID);
                packetAnim.AddInt32(0);
                SendToNearPlayers(ref packetAnim);
                packetAnim.Dispose();
                int FishEscapeTime = 2000;
                ThreadPool.RegisterWaitForSingleObject(new AutoResetEvent(false), new WaitOrTimerCallback(SetFishEscaped), null, FishEscapeTime, true);
            }

            public void SetFishEscaped(object state, bool timedOut)
            {
                if (!Operators.ConditionalCompareObjectEqual(state, GameObjectLootState.DOOR_OPEN, false))
                    return;
                Flags = GameObjectFlags.GO_FLAG_LOCKED;
                if (Loot is object)
                {
                    Loot.Dispose();
                    Loot = null;
                }

                if (Owner > 0m && WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(Owner) && WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(Owner))
                {
                    var fishEscaped = new Packets.PacketClass(OPCODES.SMSG_FISH_ESCAPED);
                    WorldServiceLocator._WorldServer.CHARACTERs[Owner].client.Send(ref fishEscaped);
                    fishEscaped.Dispose();
                    WorldServiceLocator._WorldServer.CHARACTERs[Owner].FinishSpell(CurrentSpellTypes.CURRENT_CHANNELED_SPELL, true);
                }
            }

            public void CalculateMineRemaning(bool Force = false)
            {
                if (Type != GameObjectType.GAMEOBJECT_TYPE_CHEST)
                    return;
                if (WorldServiceLocator._WS_Loot.Locks.ContainsKey(LockID) == false)
                    return;
                for (int i = 0; i <= 4; i++)
                {
                    if (WorldServiceLocator._WS_Loot.Locks[LockID].KeyType[i] == LockKeyType.LOCK_KEY_SKILL && (WorldServiceLocator._WS_Loot.Locks[LockID].KeyType[i] == LockType.LOCKTYPE_MINING || WorldServiceLocator._WS_Loot.Locks[LockID].KeyType[i] == LockType.LOCKTYPE_HERBALISM))
                    {
                        if (Force || MineRemaining == 0)
                        {
                            MineRemaining = WorldServiceLocator._WorldServer.Rnd.Next((int)get_Sound(4), (int)(get_Sound(5) + 1L));
                        }

                        return;
                    }
                }
            }

            public void SpawnAnimation()
            {
                var packet = new Packets.PacketClass(OPCODES.SMSG_GAMEOBJECT_SPAWN_ANIM);
                packet.AddUInt64(GUID);
                SendToNearPlayers(ref packet);
                packet.Dispose();
            }

            public void Respawn(object state)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Gameobject {0:X} respawning.", GUID);

                // DONE: Remove the timer
                if (RespawnTimer is object)
                {
                    RespawnTimer.Dispose();
                    RespawnTimer = null;
                    Despawned = false;
                }

                // DONE: Add to world
                Loot = null;
                AddToWorld();

                // DONE: Recalculate mines remaining
                CalculateMineRemaning(true);
            }

            public void Despawn(int Delay = 0)
            {
                if (Delay == 0)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Gameobject {0:X} despawning.", GUID);
                    var packet = new Packets.PacketClass(OPCODES.SMSG_GAMEOBJECT_DESPAWN_ANIM);
                    packet.AddUInt64(GUID);
                    SendToNearPlayers(ref packet);
                    packet.Dispose();

                    // DONE: Remove from world
                    Despawned = true;
                    if (Loot is object)
                        Loot.Dispose();
                    RemoveFromWorld();

                    // DONE: Start the respawn timer
                    if (SpawnTime > 0)
                    {
                        RespawnTimer = new Timer(new TimerCallback(Respawn), null, SpawnTime, Timeout.Infinite);
                    }
                }
                else
                {
                    ToDespawn = true;
                    RespawnTimer = new Timer(new TimerCallback(Destroy), null, Delay, Timeout.Infinite);
                }
            }

            public void Destroy(object state)
            {
                // DONE: Remove the timer
                if (RespawnTimer is object)
                {
                    RespawnTimer.Dispose();
                    RespawnTimer = null;
                    Despawned = false;
                }

                // DONE: If this gameobject were created by a player then remove the reference between them
                if (CreatedBySpell > 0 && Owner > 0m && WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(Owner))
                {
                    if (WorldServiceLocator._WorldServer.CHARACTERs[Owner].gameObjects.Contains(this))
                        WorldServiceLocator._WorldServer.CHARACTERs[Owner].gameObjects.Remove(this);
                }

                if (ToDespawn)
                {
                    ToDespawn = false;
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Gameobject {0:X} despawning.", GUID);
                    var despawnPacket = new Packets.PacketClass(OPCODES.SMSG_GAMEOBJECT_DESPAWN_ANIM);
                    despawnPacket.AddUInt64(GUID);
                    SendToNearPlayers(ref despawnPacket);
                    despawnPacket.Dispose();
                }

                var packet = new Packets.PacketClass(OPCODES.SMSG_DESTROY_OBJECT);
                packet.AddUInt64(GUID);
                SendToNearPlayers(ref packet);
                packet.Dispose();
                Dispose();
            }

            public void TurnTo(ref WS_Base.BaseObject Target)
            {
                TurnTo(Target.positionX, Target.positionY);
            }

            public void TurnTo(float x, float y)
            {
                orientation = WorldServiceLocator._WS_Combat.GetOrientation(positionX, x, positionY, y);
                Rotations[2] = (float)Math.Sin(orientation / 2f);
                Rotations[3] = (float)Math.Cos(orientation / 2f);
                if (SeenBy.Count > 0)
                {

                    // TODO: Rotation change is not visible with simple update
                    var packet = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                    packet.AddInt32(2);
                    packet.AddInt8(0);
                    var tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_GAMEOBJECT);
                    tmpUpdate.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_FACING, orientation);
                    tmpUpdate.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_ROTATION, Rotations[0]);
                    tmpUpdate.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_ROTATION + 1, Rotations[1]);
                    tmpUpdate.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_ROTATION + 2, Rotations[2]);
                    tmpUpdate.SetUpdateFlag(EGameObjectFields.GAMEOBJECT_ROTATION + 3, Rotations[3]);
                    tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, this);
                    tmpUpdate.Dispose();
                    SendToNearPlayers(ref packet);
                    packet.Dispose();
                }
            }
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia *//* TODO ERROR: Skipped RegionDirectiveTrivia */
        [MethodImpl(MethodImplOptions.Synchronized)]
        private ulong GetNewGUID()
        {
            ulong GetNewGUIDRet = default;
            WorldServiceLocator._WorldServer.GameObjectsGUIDCounter = (ulong)(WorldServiceLocator._WorldServer.GameObjectsGUIDCounter + 1m);
            GetNewGUIDRet = WorldServiceLocator._WorldServer.GameObjectsGUIDCounter;
            return GetNewGUIDRet;
        }

        public GameObjectObject GetClosestGameobject(ref WS_Base.BaseUnit unit, int GameObjectEntry = 0)
        {
            float minDistance = float.MaxValue;
            float tmpDistance;
            GameObjectObject targetGameobject = null;
            if (unit is WS_PlayerData.CharacterObject)
            {
                foreach (ulong GUID in ((WS_PlayerData.CharacterObject)unit).gameObjectsNear.ToArray())
                {
                    if (WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(GUID) && (GameObjectEntry == 0 || WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].ID == GameObjectEntry))
                    {
                        tmpDistance = WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID], unit);
                        if (tmpDistance < minDistance)
                        {
                            minDistance = tmpDistance;
                            targetGameobject = WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID];
                        }
                    }
                }

                return targetGameobject;
            }
            else
            {
                var cellX = default(byte);
                var cellY = default(byte);
                WorldServiceLocator._WS_Maps.GetMapTile(unit.positionX, unit.positionY, ref cellX, ref cellY);

                // TODO: Do we really have to look in all of those tiles?
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x + cellX > -1 && x + cellX < 64 && y + cellY > -1 && y + cellY < 64)
                        {
                            if (WorldServiceLocator._WS_Maps.Maps[unit.MapID].Tiles[x + cellX, y + cellY] is object)
                            {
                                var gameobjects = WorldServiceLocator._WS_Maps.Maps[unit.MapID].Tiles[x + cellX, y + cellY].GameObjectsHere.ToArray();
                                foreach (ulong GUID in gameobjects)
                                {
                                    if (WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(GUID) && (GameObjectEntry == 0 || WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].ID == GameObjectEntry))
                                    {
                                        tmpDistance = WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID], unit);
                                        if (tmpDistance < minDistance)
                                        {
                                            minDistance = tmpDistance;
                                            targetGameobject = WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID];
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return targetGameobject;
            }
        }

        public void On_CMSG_GAMEOBJECT_QUERY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 17)
                return;
            var response = new Packets.PacketClass(OPCODES.SMSG_GAMEOBJECT_QUERY_RESPONSE);
            packet.GetInt16();
            int GameObjectID = packet.GetInt32();
            ulong GameObjectGUID = packet.GetUInt64();
            try
            {
                GameObjectInfo GameObject;
                if (WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase.ContainsKey(GameObjectID) == false)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GAMEOBJECT_QUERY [GameObject {2} not loaded.]", client.IP, client.Port, GameObjectID);
                    response.AddUInt32((uint)(GameObjectID | 0x80000000));
                    client.Send(ref response);
                    response.Dispose();
                    return;
                }
                else
                {
                    GameObject = WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase[GameObjectID];
                }

                response.AddInt32(GameObject.ID);
                response.AddInt32(GameObject.Type);
                response.AddInt32(GameObject.Model);
                response.AddString(GameObject.Name);
                response.AddInt16(0); // Name2
                response.AddInt8(0); // Name3
                response.AddInt8(0); // Name4
                for (byte i = 0; i <= 23; i++)
                    response.AddUInt32(GameObject.Fields[i]);
                client.Send(ref response);
                response.Dispose();
            }
            // _WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_GAMEOBJECT_QUERY_RESPONSE", client.IP, client.Port)
            catch (Exception e)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Unknown Error: Unable to find GameObjectID={0} in database.", GameObjectID);
            }
        }

        public void On_CMSG_GAMEOBJ_USE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 13)
                return;
            packet.GetInt16();
            ulong GameObjectGUID = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GAMEOBJ_USE [GUID={2:X}]", client.IP, client.Port, GameObjectGUID);
            if (WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(GameObjectGUID) == false)
                return;
            var GO = WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GameObjectGUID];
            client.Character.RemoveAurasByInterruptFlag(SpellAuraInterruptFlags.AURA_INTERRUPT_FLAG_USE);
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "GameObjectType: {0}", WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GameObjectGUID].Type);
            switch (GO.Type)
            {
                case var @case when @case == GameObjectType.GAMEOBJECT_TYPE_BUTTON:
                case var case1 when case1 == GameObjectType.GAMEOBJECT_TYPE_DOOR:
                    {
                        // DONE: Doors opening
                        GO.OpenDoor();
                        break;
                    }

                case var case2 when case2 == GameObjectType.GAMEOBJECT_TYPE_CHAIR:
                    {
                        // DONE: Chair sitting again
                        var StandState = new Packets.PacketClass(OPCODES.CMSG_STANDSTATECHANGE);
                        try
                        {
                            StandState.AddInt8((byte)(4L + WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GameObjectGUID].get_Sound(1)));
                            client.Character.Teleport(GO.positionX, GO.positionY, GO.positionZ, GO.orientation, (int)GO.MapID);
                            client.Send(ref StandState);
                        }
                        finally
                        {
                            StandState.Dispose();
                        }

                        var packetACK = new Packets.PacketClass(OPCODES.SMSG_STANDSTATE_CHANGE_ACK);
                        try
                        {
                            packetACK.AddInt8((byte)(4L + GO.get_Sound(1)));
                            client.Send(ref packetACK);
                        }
                        finally
                        {
                            packetACK.Dispose();
                        }

                        break;
                    }

                case var case3 when case3 == GameObjectType.GAMEOBJECT_TYPE_QUESTGIVER:
                    {
                        var qm = WorldServiceLocator._WorldServer.ALLQUESTS.GetQuestMenuGO(ref client.Character, GameObjectGUID);
                        WorldServiceLocator._WorldServer.ALLQUESTS.SendQuestMenu(ref client.Character, GameObjectGUID, questMenu: qm);
                        break;
                    }

                case var case4 when case4 == GameObjectType.GAMEOBJECT_TYPE_CAMERA:
                    {
                        var cinematicPacket = new Packets.PacketClass(OPCODES.SMSG_TRIGGER_CINEMATIC);
                        cinematicPacket.AddUInt32(GO.get_Sound(1));
                        client.Send(ref cinematicPacket);
                        cinematicPacket.Dispose();
                        break;
                    }

                case var case5 when case5 == GameObjectType.GAMEOBJECT_TYPE_RITUAL:
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Clicked a ritual.");
                        // DONE: You can only click on rituals by group members
                        if (GO.Owner != client.Character.GUID && client.Character.IsInGroup == false)
                            return;
                        if (GO.Owner != client.Character.GUID)
                        {
                            if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(GO.Owner) == false || WorldServiceLocator._WorldServer.CHARACTERs[GO.Owner].IsInGroup == false)
                                return;
                            if (!ReferenceEquals(WorldServiceLocator._WorldServer.CHARACTERs[GO.Owner].Group, client.Character.Group))
                                return;
                        }

                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Casting ritual spell.");
                        client.Character.CastOnSelf((int)GO.get_Sound(1));
                        break;
                    }

                case var case6 when case6 == GameObjectType.GAMEOBJECT_TYPE_SPELLCASTER:
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Clicked a spellcaster.");
                        GO.Flags = 2;

                        // DONE: Check if you're in the same party
                        if (Conversions.ToBoolean(GO.get_Sound(2)))
                        {
                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Spellcaster requires same group.");
                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Owner: {0:X}  You: {1:X}", WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GameObjectGUID].Owner, client.Character.GUID);
                            if (GO.Owner != client.Character.GUID && client.Character.IsInGroup == false)
                                return;
                            if (GO.Owner != client.Character.GUID)
                            {
                                if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(GO.Owner) == false || WorldServiceLocator._WorldServer.CHARACTERs[GO.Owner].IsInGroup == false)
                                    return;
                                if (!ReferenceEquals(WorldServiceLocator._WorldServer.CHARACTERs[GO.Owner].Group, client.Character.Group))
                                    return;
                            }
                        }

                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Casted spellcaster spell.");
                        client.Character.CastOnSelf((int)GO.get_Sound(0));
                        break;
                    }

                // TODO: Remove one charge

                case var case7 when case7 == GameObjectType.GAMEOBJECT_TYPE_MEETINGSTONE:
                    {
                        if (client.Character.Level < GO.get_Sound(0)) // Too low level
                        {
                            // TODO: Send the correct packet.
                            WorldServiceLocator._WS_Spells.SendCastResult(SpellFailedReason.SPELL_FAILED_LEVEL_REQUIREMENT, ref client, 23598);
                            return;
                        }

                        if (client.Character.Level > WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GameObjectGUID].get_Sound(1)) // Too high level
                        {
                            // TODO: Send the correct packet.
                            WorldServiceLocator._WS_Spells.SendCastResult(SpellFailedReason.SPELL_FAILED_LEVEL_REQUIREMENT, ref client, 23598);
                            return;
                        }

                        client.Character.CastOnSelf(23598);
                        break;
                    }

                case var case8 when case8 == GameObjectType.GAMEOBJECT_TYPE_FISHINGNODE:
                    {
                        if (GO.Owner != client.Character.GUID)
                            return;
                        if (GO.Loot is null)
                        {
                            if (GO.State == GameObjectLootState.DOOR_CLOSED)
                            {
                                GO.State = GameObjectLootState.DOOR_OPEN;
                                var fishNotHookedPacket = new Packets.PacketClass(OPCODES.SMSG_FISH_NOT_HOOKED);
                                client.Send(ref fishNotHookedPacket);
                                fishNotHookedPacket.Dispose();
                            }
                        }
                        else
                        {
                            // DONE: Check if we where able to loot it with our skill level
                            int AreaFlag = WorldServiceLocator._WS_Maps.GetAreaFlag(GO.positionX, GO.positionY, (int)GO.MapID);
                            int AreaID = WorldServiceLocator._WS_Maps.AreaTable[AreaFlag].ID;
                            var MySQLQuery = new DataTable();
                            WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT * FROM skill_fishing_base_level WHERE entry = {0};", (object)AreaID), MySQLQuery);
                            if (MySQLQuery.Rows.Count == 0)
                            {
                                AreaID = WorldServiceLocator._WS_Maps.AreaTable[AreaFlag].Zone;
                                MySQLQuery.Clear();
                                WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT * FROM skill_fishing_base_level WHERE entry = {0};", (object)AreaID), MySQLQuery);
                            }

                            int zoneSkill = 0;
                            if (MySQLQuery.Rows.Count > 0)
                            {
                                zoneSkill = Conversions.ToInteger(MySQLQuery.Rows[0]["skill"]);
                            }
                            else
                            {
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "No fishing entry in 'skill_fishing_base_level' for area [{0}] in zone [{1}]", WorldServiceLocator._WS_Maps.AreaTable[AreaFlag].ID, WorldServiceLocator._WS_Maps.AreaTable[AreaFlag].Zone);
                            }

                            int skill = client.Character.Skills(SKILL_IDs.SKILL_FISHING).CurrentWithBonus;
                            int chance = skill - zoneSkill + 5;
                            int roll = WorldServiceLocator._WorldServer.Rnd.Next(1, 101);
                            if (skill > zoneSkill && roll >= chance)
                            {
                                GO.State = GameObjectLootState.DOOR_CLOSED;
                                GO.Loot.SendLoot(ref client);

                                // DONE: Update skill!
                                client.Character.UpdateSkill(SKILL_IDs.SKILL_FISHING, 0.01d);
                            }
                            else
                            {
                                GO.State = GameObjectLootState.DOOR_CLOSED;
                                var fishEscaped = new Packets.PacketClass(OPCODES.SMSG_FISH_ESCAPED);
                                client.Send(ref fishEscaped);
                                fishEscaped.Dispose();
                            }
                        }

                        // Stop channeling!
                        client.Character.FinishSpell(CurrentSpellTypes.CURRENT_CHANNELED_SPELL, true);
                        break;
                    }
            }
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
    }
}