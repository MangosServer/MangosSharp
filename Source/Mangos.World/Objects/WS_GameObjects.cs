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
using Mangos.Common.Enums.Spell;
using Mangos.Common.Globals;
using Mangos.Common.Legacy;
using Mangos.World.Globals;
using Mangos.World.Loots;
using Mangos.World.Maps;
using Mangos.World.Network;
using Mangos.World.Player;
using Mangos.World.Quests;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Mangos.World.Objects;

public class WS_GameObjects
{
    public class GameObjectInfo : IDisposable
    {
        public int ID;

        public int Model;

        public GameObjectType Type;

        public string Name;

        public short Faction;

        public int Flags;

        public float Size;

        public uint[] Fields;

        public string ScriptName;

        private readonly bool found_;

        private bool _disposedValue;

        public GameObjectInfo(int ID_)
        {
            ID = 0;
            Model = 0;
            Type = GameObjectType.GAMEOBJECT_TYPE_DOOR;
            Name = "";
            Faction = 0;
            Flags = 0;
            Size = 1f;
            Fields = new uint[24];
            ScriptName = "";
            found_ = false;
            ID = ID_;
            WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase.Add(ID, this);
            DataTable MySQLQuery = new();
            WorldServiceLocator._WorldServer.WorldDatabase.Query($"SELECT * FROM gameobject_template WHERE entry = {ID_};", ref MySQLQuery);
            if (MySQLQuery.Rows.Count == 0)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "gameobject_template {0} not found in SQL database!", ID_);
                found_ = false;
                return;
            }
            found_ = true;
            Model = MySQLQuery.Rows[0].As<int>("displayId");
            Type = (GameObjectType)MySQLQuery.Rows[0].As<byte>("type");
            Name = MySQLQuery.Rows[0].As<string>("name");
            Faction = MySQLQuery.Rows[0].As<short>("faction");
            Flags = MySQLQuery.Rows[0].As<int>("flags");
            Size = MySQLQuery.Rows[0].As<float>("size");
            byte i = 0;
            do
            {
                Fields[i] = MySQLQuery.Rows[0].As<uint>("data" + Conversions.ToString(i));
                checked
                {
                    i = (byte)unchecked((uint)(i + 1));
                }
            }
            while (i <= 23u);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase.Remove(ID);
            }
            _disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        void IDisposable.Dispose()
        {
            //ILSpy generated this explicit interface implementation from .override directive in Dispose
            Dispose();
        }
    }

    public class GameObject : WS_Base.BaseObject, IDisposable
    {
        public int ID;

        public int Flags;

        public float Size;

        public int Faction;

        public GameObjectLootState State;

        public float[] Rotations;

        public ulong Owner;

        public WS_Loot.LootObject Loot;

        public bool Despawned;

        public int MineRemaining;

        public int AnimProgress;

        public int SpawnTime;

        public int GameEvent;

        public int CreatedBySpell;

        public int Level;

        private bool ToDespawn;

        public List<int> IncludesQuestItems;

        private Timer RespawnTimer;

        private bool _disposedValue;

        public GameObjectInfo ObjectInfo => WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase[ID];

        public string Name => ObjectInfo.Name;

        public GameObjectType Type => ObjectInfo.Type;

        public uint GetSound(int Index)
        {
            return ObjectInfo.Fields[Index];
        }

        public bool IsUsedForQuests => IncludesQuestItems.Count > 0;

        public int LockID => checked(ObjectInfo.Type switch
        {
            GameObjectType.GAMEOBJECT_TYPE_DOOR => (int)GetSound(1),
            GameObjectType.GAMEOBJECT_TYPE_BUTTON => (int)GetSound(1),
            GameObjectType.GAMEOBJECT_TYPE_QUESTGIVER => (int)GetSound(0),
            GameObjectType.GAMEOBJECT_TYPE_CHEST => (int)GetSound(0),
            GameObjectType.GAMEOBJECT_TYPE_TRAP => (int)GetSound(0),
            GameObjectType.GAMEOBJECT_TYPE_GOOBER => (int)GetSound(0),
            GameObjectType.GAMEOBJECT_TYPE_AREADAMAGE => (int)GetSound(0),
            GameObjectType.GAMEOBJECT_TYPE_CAMERA => (int)GetSound(0),
            GameObjectType.GAMEOBJECT_TYPE_FLAGSTAND => (int)GetSound(0),
            GameObjectType.GAMEOBJECT_TYPE_FISHINGHOLE => (int)GetSound(4),
            GameObjectType.GAMEOBJECT_TYPE_FLAGDROP => (int)GetSound(0),
            _ => 0,
        });

        public int LootID => checked(ObjectInfo.Type switch
        {
            GameObjectType.GAMEOBJECT_TYPE_CHEST => (int)GetSound(1),
            GameObjectType.GAMEOBJECT_TYPE_FISHINGNODE => (int)GetSound(1),
            GameObjectType.GAMEOBJECT_TYPE_FISHINGHOLE => (int)GetSound(1),
            _ => 0,
        });

        public int AutoCloseTime => checked(ObjectInfo.Type switch
        {
            GameObjectType.GAMEOBJECT_TYPE_DOOR => (int)Math.Round(GetSound(2) / 65536.0 * 1000.0),
            GameObjectType.GAMEOBJECT_TYPE_BUTTON => (int)Math.Round(GetSound(2) / 65536.0 * 1000.0),
            GameObjectType.GAMEOBJECT_TYPE_TRAP => (int)Math.Round(GetSound(6) / 65536.0 * 1000.0),
            GameObjectType.GAMEOBJECT_TYPE_GOOBER => (int)Math.Round(GetSound(3) / 65536.0 * 1000.0),
            GameObjectType.GAMEOBJECT_TYPE_TRANSPORT => (int)Math.Round(GetSound(2) / 65536.0 * 1000.0),
            GameObjectType.GAMEOBJECT_TYPE_AREADAMAGE => (int)Math.Round(GetSound(5) / 65536.0 * 1000.0),
            _ => 0,
        });

        public bool IsConsumeable
        {
            get
            {
                var type = ObjectInfo.Type;
                return type == GameObjectType.GAMEOBJECT_TYPE_CHEST && (ulong)GetSound(3) == 1;
            }
        }

        public virtual void FillAllUpdateFlags(ref Packets.UpdateClass Update, ref WS_PlayerData.CharacterObject Character)
        {
            Update.SetUpdateFlag(0, GUID);
            Update.SetUpdateFlag(2, 33);
            Update.SetUpdateFlag(3, ID);
            Update.SetUpdateFlag(4, Size);
            if (Owner != 0)
            {
                Update.SetUpdateFlag(6, Owner);
            }
            Update.SetUpdateFlag(15, positionX);
            Update.SetUpdateFlag(16, positionY);
            Update.SetUpdateFlag(17, positionZ);
            Update.SetUpdateFlag(18, orientation);
            var Rotation = 0L;
            var f_rot1 = (float)Math.Sin(orientation / 2f);
            var i_rot1 = checked((long)Math.Round(f_rot1 / Math.Atan(Math.Pow(2.0, -20.0))));
            Rotation |= (i_rot1 << 43 >> 43) & 0x1FFFFF;
            Update.SetUpdateFlag(10, Rotation);
            var DynFlags = 0;
            if (Type == GameObjectType.GAMEOBJECT_TYPE_CHEST)
            {
                var aLLQUESTS = WorldServiceLocator._WorldServer.ALLQUESTS;
                var gameobject = this;
                var UsedForQuest = aLLQUESTS.IsGameObjectUsedForQuest(ref gameobject, ref Character);
                if (UsedForQuest > 0)
                {
                    Flags |= 4;
                    if (UsedForQuest == 2)
                    {
                        DynFlags = 9;
                    }
                }
            }
            else if (Type == GameObjectType.GAMEOBJECT_TYPE_GOOBER)
            {
                DynFlags = 1;
            }
            if (DynFlags != 0)
            {
                Update.SetUpdateFlag(19, DynFlags);
            }
            Update.SetUpdateFlag(14, 0, (byte)State);
            Update.SetUpdateFlag(21, (int)Type);
            if (Level > 0)
            {
                Update.SetUpdateFlag(22, Level);
            }
            Update.SetUpdateFlag(20, Faction);
            Update.SetUpdateFlag(9, Flags);
            Update.SetUpdateFlag(8, ObjectInfo.Model);
            Update.SetUpdateFlag(10, Rotations[0]);
            Update.SetUpdateFlag(11, Rotations[1]);
            Update.SetUpdateFlag(12, Rotations[2]);
            Update.SetUpdateFlag(13, Rotations[3]);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                RemoveFromWorld();
                if (Loot != null && Type != GameObjectType.GAMEOBJECT_TYPE_FISHINGNODE)
                {
                    Loot.Dispose();
                }
                if (this is WS_Transports.TransportObject)
                {
                    WorldServiceLocator._WorldServer.WORLD_TRANSPORTs_Lock.AcquireWriterLock(-1);
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

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        void IDisposable.Dispose()
        {
            //ILSpy generated this explicit interface implementation from .override directive in Dispose
            Dispose();
        }

        public GameObject(int ID_)
        {
            ID = 0;
            Flags = 0;
            Size = 1f;
            Faction = 0;
            State = GameObjectLootState.DOOR_CLOSED;
            Rotations = new float[4];
            Loot = null;
            Despawned = false;
            MineRemaining = 0;
            AnimProgress = 0;
            SpawnTime = 0;
            GameEvent = 0;
            CreatedBySpell = 0;
            Level = 0;
            ToDespawn = false;
            IncludesQuestItems = new List<int>();
            RespawnTimer = null;
            if (!WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase.ContainsKey(ID_))
            {
                GameObjectInfo baseGameObject = new(ID_);
            }
            ID = ID_;
            GUID = WorldServiceLocator._WS_GameObjects.GetNewGUID();
            Flags = WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase[ID].Flags;
            Faction = WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase[ID].Faction;
            Size = WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase[ID].Size;
            WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.Add(GUID, this);
        }

        public GameObject(int ID_, ulong GUID_)
        {
            ID = 0;
            Flags = 0;
            Size = 1f;
            Faction = 0;
            State = GameObjectLootState.DOOR_CLOSED;
            Rotations = new float[4];
            Loot = null;
            Despawned = false;
            MineRemaining = 0;
            AnimProgress = 0;
            SpawnTime = 0;
            GameEvent = 0;
            CreatedBySpell = 0;
            Level = 0;
            ToDespawn = false;
            IncludesQuestItems = new List<int>();
            RespawnTimer = null;
            if (!WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase.ContainsKey(ID_))
            {
                GameObjectInfo baseGameObject = new(ID_);
            }
            ID = ID_;
            GUID = GUID_;
            Flags = WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase[ID].Flags;
            Faction = WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase[ID].Faction;
            Size = WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase[ID].Size;
        }

        public GameObject(int ID_, uint MapID_, float PosX, float PosY, float PosZ, float Rotation, ulong Owner_ = 0uL)
        {
            ID = 0;
            Flags = 0;
            Size = 1f;
            Faction = 0;
            State = GameObjectLootState.DOOR_CLOSED;
            Rotations = new float[4];
            Loot = null;
            Despawned = false;
            MineRemaining = 0;
            AnimProgress = 0;
            SpawnTime = 0;
            GameEvent = 0;
            CreatedBySpell = 0;
            Level = 0;
            ToDespawn = false;
            IncludesQuestItems = new List<int>();
            RespawnTimer = null;
            if (!WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase.ContainsKey(ID_))
            {
                GameObjectInfo baseGameObject = new(ID_);
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
                VisibleDistance = 99999f;
                State = GameObjectLootState.DOOR_CLOSED;
            }
            WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.Add(GUID, this);
        }

        public GameObject(ulong cGUID, DataRow Info = null)
        {
            ID = 0;
            Flags = 0;
            Size = 1f;
            Faction = 0;
            State = GameObjectLootState.DOOR_CLOSED;
            Rotations = new float[4];
            Loot = null;
            Despawned = false;
            MineRemaining = 0;
            AnimProgress = 0;
            SpawnTime = 0;
            GameEvent = 0;
            CreatedBySpell = 0;
            Level = 0;
            ToDespawn = false;
            IncludesQuestItems = new List<int>();
            RespawnTimer = null;
            if (Info == null)
            {
                DataTable MySQLQuery = new();
                WorldServiceLocator._WorldServer.WorldDatabase.Query($"SELECT * FROM gameobject LEFT OUTER JOIN game_event_gameobject ON gameobject.guid = game_event_gameobject.guid WHERE gameobject.guid = {cGUID};", ref MySQLQuery);
                if (MySQLQuery.Rows.Count <= 0)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "GameObject Spawn not found in database. [cGUID={0:X}]", cGUID);
                    return;
                }
                Info = MySQLQuery.Rows[0];
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
            State = (GameObjectLootState)Conversions.ToByte(Info["state"]);
            if (!WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase.ContainsKey(ID))
            {
                GameObjectInfo baseGameObject = new(ID);
            }
            Flags = WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase[ID].Flags;
            Faction = WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase[ID].Faction;
            Size = WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase[ID].Size;
            checked
            {
                if (Type == GameObjectType.GAMEOBJECT_TYPE_TRANSPORT)
                {
                    VisibleDistance = 99999f;
                    GUID = cGUID + WorldServiceLocator._Global_Constants.GUID_TRANSPORT;
                }
                else
                {
                    GUID = cGUID + WorldServiceLocator._Global_Constants.GUID_GAMEOBJECT;
                }
                WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.Add(GUID, this);
                if (WorldServiceLocator._WS_Loot.LootTable.ContainsKey(GUID))
                {
                    Loot = WorldServiceLocator._WS_Loot.LootTable[GUID];
                }
                CalculateMineRemaning(Force: true);
            }
        }

        public void AddToWorld()
        {
            WorldServiceLocator._WS_Maps.GetMapTile(positionX, positionY, ref CellX, ref CellY);
            if (WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY] == null)
            {
                WorldServiceLocator._WS_CharMovement.MAP_Load(CellX, CellY, MapID);
            }
            try
            {
                WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY].GameObjectsHere.Add(GUID);
            }
            catch (Exception projectError)
            {
                ProjectData.SetProjectError(projectError);
                ProjectData.ClearProjectError();
                return;
            }
            if (Type == GameObjectType.GAMEOBJECT_TYPE_CHEST && Loot == null)
            {
                GenerateLoot();
            }
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
                                if (num != 0)
                                {
                                    Packets.PacketClass packet = new(Opcodes.SMSG_UPDATE_OBJECT);
                                    packet.AddInt32(1);
                                    packet.AddInt8(0);
                                    Packets.UpdateClass tmpUpdate = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_GAMEOBJECT);
                                    Dictionary<ulong, WS_PlayerData.CharacterObject> cHARACTERs;
                                    ulong key;
                                    var Character = (cHARACTERs = WorldServiceLocator._WorldServer.CHARACTERs)[key = plGUID];
                                    FillAllUpdateFlags(ref tmpUpdate, ref Character);
                                    cHARACTERs[key] = Character;
                                    var updateClass = tmpUpdate;
                                    var updateObject = this;
                                    updateClass.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, ref updateObject);
                                    tmpUpdate.Dispose();
                                    WorldServiceLocator._WorldServer.CHARACTERs[plGUID].client.SendMultiplyPackets(ref packet);
                                    WorldServiceLocator._WorldServer.CHARACTERs[plGUID].gameObjectsNear.Add(GUID);
                                    SeenBy.Add(plGUID);
                                    packet.Dispose();
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

        public void RemoveFromWorld()
        {
            if (WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY] == null)
            {
                return;
            }
            WorldServiceLocator._WS_Maps.GetMapTile(positionX, positionY, ref CellX, ref CellY);
            WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY].GameObjectsHere.Remove(GUID);
            var array = SeenBy.ToArray();
            foreach (var plGUID in array)
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
            Packets.PacketClass packet = new(Opcodes.SMSG_UPDATE_OBJECT);
            packet.AddInt32(1);
            packet.AddInt8(0);
            Packets.UpdateClass tmpUpdate = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_GAMEOBJECT);
            tmpUpdate.SetUpdateFlag(14, 0, (byte)State);
            var updateObject = this;
            tmpUpdate.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
            tmpUpdate.Dispose();
            SendToNearPlayers(ref packet);
            packet.Dispose();
        }

        public void OpenDoor()
        {
            Flags |= 1;
            State = GameObjectLootState.DOOR_OPEN;
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "AutoCloseTime: {0}", AutoCloseTime);
            if (AutoCloseTime > 0)
            {
                ThreadPool.RegisterWaitForSingleObject(new AutoResetEvent(initialState: false), CloseDoor, null, AutoCloseTime, executeOnlyOnce: true);
            }
            Packets.PacketClass packet = new(Opcodes.SMSG_UPDATE_OBJECT);
            packet.AddInt32(1);
            packet.AddInt8(0);
            Packets.UpdateClass tmpUpdate = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_GAMEOBJECT);
            tmpUpdate.SetUpdateFlag(9, Flags);
            tmpUpdate.SetUpdateFlag(14, 0, (byte)State);
            var updateObject = this;
            tmpUpdate.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
            tmpUpdate.Dispose();
            SendToNearPlayers(ref packet);
            packet.Dispose();
        }

        public void CloseDoor(object state, bool timedOut)
        {
            Flags &= 254;
            state = GameObjectLootState.DOOR_CLOSED;
            Packets.PacketClass packet = new(Opcodes.SMSG_UPDATE_OBJECT);
            packet.AddInt32(1);
            packet.AddInt8(0);
            Packets.UpdateClass tmpUpdate = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_GAMEOBJECT);
            tmpUpdate.SetUpdateFlag(9, Flags);
            tmpUpdate.SetUpdateFlag(14, 0, Conversions.ToByte(state));
            var updateObject = this;
            tmpUpdate.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
            tmpUpdate.Dispose();
            SendToNearPlayers(ref packet);
            packet.Dispose();
        }

        public void LootObject(ref WS_PlayerData.CharacterObject Character, LootType LootingType)
        {
            State = GameObjectLootState.LOOT_LOOTED;
            switch (Type)
            {
                case GameObjectType.GAMEOBJECT_TYPE_DOOR:
                case GameObjectType.GAMEOBJECT_TYPE_BUTTON:
                    OpenDoor();
                    return;

                case GameObjectType.GAMEOBJECT_TYPE_QUESTGIVER:
                    return;
            }
            if (Loot != null)
            {
                Loot.SendLoot(ref Character.client);
                if (Character.spellCasted[1] != null)
                {
                    Character.spellCasted[1].State = SpellCastState.SPELL_STATE_FINISHED;
                }
            }
        }

        public bool GenerateLoot()
        {
            if (Loot != null)
            {
                return true;
            }
            if (LootID == 0)
            {
                return false;
            }
            Loot = new WS_Loot.LootObject(GUID, LootType.LOOTTYPE_SKINNING);
            WorldServiceLocator._WS_Loot.LootTemplates_Gameobject.GetLoot(LootID)?.Process(ref Loot, 0);
            Loot.LootOwner = 0uL;
            return true;
        }

        public void SetupFishingNode()
        {
            var RandomTime = WorldServiceLocator._WorldServer.Rnd.Next(3000, 17000);
            ThreadPool.RegisterWaitForSingleObject(new AutoResetEvent(initialState: false), SetFishHooked, null, RandomTime, executeOnlyOnce: true);
            State = GameObjectLootState.DOOR_CLOSED;
        }

        public void SetFishHooked(object state, bool timedOut)
        {
            if (!Operators.ConditionalCompareObjectNotEqual(state, GameObjectLootState.DOOR_CLOSED, TextCompare: false))
            {
                state = GameObjectLootState.DOOR_OPEN;
                Flags = 32;
                Loot = new WS_Loot.LootObject(GUID, LootType.LOOTTYPE_FISHING)
                {
                    LootOwner = Owner
                };
                var AreaFlag = WorldServiceLocator._WS_Maps.GetAreaFlag(positionX, positionY, checked((int)MapID));
                var AreaID = WorldServiceLocator._WS_Maps.AreaTable[AreaFlag].ID;
                WorldServiceLocator._WS_Loot.LootTemplates_Fishing.GetLoot(AreaID)?.Process(ref Loot, 0);
                Packets.PacketClass packet = new(Opcodes.SMSG_UPDATE_OBJECT);
                packet.AddInt32(1);
                packet.AddInt8(0);
                Packets.UpdateClass tmpUpdate = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_GAMEOBJECT);
                tmpUpdate.SetUpdateFlag(9, Flags);
                tmpUpdate.SetUpdateFlag(14, 0, Conversions.ToByte(state));
                var updateObject = this;
                tmpUpdate.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
                tmpUpdate.Dispose();
                SendToNearPlayers(ref packet);
                packet.Dispose();
                Packets.PacketClass packetAnim = new(Opcodes.SMSG_GAMEOBJECT_CUSTOM_ANIM);
                packetAnim.AddUInt64(GUID);
                packetAnim.AddInt32(0);
                SendToNearPlayers(ref packetAnim);
                packetAnim.Dispose();
                var FishEscapeTime = 2000;
                ThreadPool.RegisterWaitForSingleObject(new AutoResetEvent(initialState: false), SetFishEscaped, null, FishEscapeTime, executeOnlyOnce: true);
            }
        }

        public void SetFishEscaped(object state, bool timedOut)
        {
            if (!Operators.ConditionalCompareObjectNotEqual(state, GameObjectLootState.DOOR_OPEN, TextCompare: false))
            {
                Flags = 2;
                if (Loot != null)
                {
                    Loot.Dispose();
                    Loot = null;
                }
                if (decimal.Compare(new decimal(Owner), 0m) > 0 && WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(Owner) && WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(Owner))
                {
                    Packets.PacketClass fishEscaped = new(Opcodes.SMSG_FISH_ESCAPED);
                    WorldServiceLocator._WorldServer.CHARACTERs[Owner].client.Send(ref fishEscaped);
                    fishEscaped.Dispose();
                    WorldServiceLocator._WorldServer.CHARACTERs[Owner].FinishSpell(CurrentSpellTypes.CURRENT_CHANNELED_SPELL, OK: true);
                }
            }
        }

        public void CalculateMineRemaning(bool Force = false)
        {
            if (Type != GameObjectType.GAMEOBJECT_TYPE_CHEST || !WorldServiceLocator._WS_Loot.Locks.ContainsKey(LockID))
            {
                return;
            }
            var i = 0;
            checked
            {
                do
                {
                    if (WorldServiceLocator._WS_Loot.Locks[LockID].KeyType[i] is 2 and (3 or 2))
                    {
                        if (Force || MineRemaining == 0)
                        {
                            MineRemaining = WorldServiceLocator._WorldServer.Rnd.Next((int)GetSound(4), (int)(GetSound(5) + 1L));
                        }
                        break;
                    }
                    i++;
                }
                while (i <= 4);
            }
        }

        public void SpawnAnimation()
        {
            Packets.PacketClass packet = new(Opcodes.SMSG_GAMEOBJECT_SPAWN_ANIM);
            packet.AddUInt64(GUID);
            SendToNearPlayers(ref packet);
            packet.Dispose();
        }

        public void Respawn(object state)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Gameobject {0:X} respawning.", GUID);
            if (RespawnTimer != null)
            {
                RespawnTimer.Dispose();
                RespawnTimer = null;
                Despawned = false;
            }
            Loot = null;
            AddToWorld();
            CalculateMineRemaning(Force: true);
        }

        public void Despawn(int Delay = 0)
        {
            if (Delay == 0)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Gameobject {0:X} despawning.", GUID);
                Packets.PacketClass packet = new(Opcodes.SMSG_GAMEOBJECT_DESPAWN_ANIM);
                packet.AddUInt64(GUID);
                SendToNearPlayers(ref packet);
                packet.Dispose();
                Despawned = true;
                if (Loot != null)
                {
                    Loot.Dispose();
                }
                RemoveFromWorld();
                if (SpawnTime > 0)
                {
                    RespawnTimer = new Timer(Respawn, null, SpawnTime, -1);
                }
            }
            else
            {
                ToDespawn = true;
                RespawnTimer = new Timer(Destroy, null, Delay, -1);
            }
        }

        public void Destroy(object state)
        {
            if (RespawnTimer != null)
            {
                RespawnTimer.Dispose();
                RespawnTimer = null;
                Despawned = false;
            }
            if (CreatedBySpell > 0 && decimal.Compare(new decimal(Owner), 0m) > 0 && WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(Owner) && WorldServiceLocator._WorldServer.CHARACTERs[Owner].gameObjects.Contains(this))
            {
                WorldServiceLocator._WorldServer.CHARACTERs[Owner].gameObjects.Remove(this);
            }
            if (ToDespawn)
            {
                ToDespawn = false;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Gameobject {0:X} despawning.", GUID);
                Packets.PacketClass despawnPacket = new(Opcodes.SMSG_GAMEOBJECT_DESPAWN_ANIM);
                despawnPacket.AddUInt64(GUID);
                SendToNearPlayers(ref despawnPacket);
                despawnPacket.Dispose();
            }
            Packets.PacketClass packet = new(Opcodes.SMSG_DESTROY_OBJECT);
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
                Packets.PacketClass packet = new(Opcodes.SMSG_UPDATE_OBJECT);
                packet.AddInt32(2);
                packet.AddInt8(0);
                Packets.UpdateClass tmpUpdate = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_GAMEOBJECT);
                tmpUpdate.SetUpdateFlag(18, orientation);
                tmpUpdate.SetUpdateFlag(10, Rotations[0]);
                tmpUpdate.SetUpdateFlag(11, Rotations[1]);
                tmpUpdate.SetUpdateFlag(12, Rotations[2]);
                tmpUpdate.SetUpdateFlag(13, Rotations[3]);
                var updateObject = this;
                tmpUpdate.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
                tmpUpdate.Dispose();
                SendToNearPlayers(ref packet);
                packet.Dispose();
            }
        }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private ulong GetNewGUID()
    {
        ref var gameObjectsGUIDCounter = ref WorldServiceLocator._WorldServer.GameObjectsGUIDCounter;
        gameObjectsGUIDCounter = Convert.ToUInt64(decimal.Add(new decimal(gameObjectsGUIDCounter), 1m));
        return WorldServiceLocator._WorldServer.GameObjectsGUIDCounter;
    }

    public GameObject GetClosestGameobject(ref WS_Base.BaseUnit unit, int GameObjectEntry = 0)
    {
        var minDistance = float.MaxValue;
        GameObject targetGameobject = null;
        if (unit is WS_PlayerData.CharacterObject @object)
        {
            var array = @object.gameObjectsNear.ToArray();
            foreach (var GUID2 in array)
            {
                if (WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(GUID2) && (GameObjectEntry == 0 || WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID2].ID == GameObjectEntry))
                {
                    var tmpDistance = WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID2], unit);
                    if (tmpDistance < minDistance)
                    {
                        minDistance = tmpDistance;
                        targetGameobject = WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID2];
                    }
                }
            }
            return targetGameobject;
        }
        byte cellX = default;
        byte cellY = default;
        WorldServiceLocator._WS_Maps.GetMapTile(unit.positionX, unit.positionY, ref cellX, ref cellY);
        var x = -1;
        checked
        {
            do
            {
                var y = -1;
                do
                {
                    if (x + cellX > -1 && x + cellX < 64 && y + cellY > -1 && y + cellY < 64 && WorldServiceLocator._WS_Maps.Maps[unit.MapID].Tiles[x + cellX, y + cellY] != null)
                    {
                        var gameobjects = WorldServiceLocator._WS_Maps.Maps[unit.MapID].Tiles[x + cellX, y + cellY].GameObjectsHere.ToArray();
                        var array2 = gameobjects;
                        foreach (var GUID in array2)
                        {
                            if (WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(GUID) && (GameObjectEntry == 0 || WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID].ID == GameObjectEntry))
                            {
                                var tmpDistance = WorldServiceLocator._WS_Combat.GetDistance(WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID], unit);
                                if (tmpDistance < minDistance)
                                {
                                    minDistance = tmpDistance;
                                    targetGameobject = WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GUID];
                                }
                            }
                        }
                    }
                    y++;
                }
                while (y <= 1);
                x++;
            }
            while (x <= 1);
            return targetGameobject;
        }
    }

    public void On_CMSG_GAMEOBJECT_QUERY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        checked
        {
            if (packet.Data.Length - 1 < 17)
            {
                return;
            }
            Packets.PacketClass response = new(Opcodes.SMSG_GAMEOBJECT_QUERY_RESPONSE);
            packet.GetInt16();
            var GameObjectID = packet.GetInt32();
            var GameObjectGUID = packet.GetUInt64();
            try
            {
                if (!WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase.ContainsKey(GameObjectID))
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GAMEOBJECT_QUERY [GameObject {2} not loaded.]", client.IP, client.Port, GameObjectID);
                    response.AddUInt32((uint)(GameObjectID | int.MinValue));
                    client.Send(ref response);
                    response.Dispose();
                    return;
                }
                var GameObject = WorldServiceLocator._WorldServer.GAMEOBJECTSDatabase[GameObjectID];
                response.AddInt32(GameObject.ID);
                response.AddInt32((int)GameObject.Type);
                response.AddInt32(GameObject.Model);
                response.AddString(GameObject.Name);
                response.AddInt16(0);
                response.AddInt8(0);
                response.AddInt8(0);
                byte i = 0;
                do
                {
                    response.AddUInt32(GameObject.Fields[i]);
                    i = (byte)unchecked((uint)(i + 1));
                }
                while (i <= 23u);
                client.Send(ref response);
                response.Dispose();
            }
            catch (Exception ex)
            {
                ProjectData.SetProjectError(ex);
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Unknown Error: Unable to find GameObjectID={0} in database.", GameObjectID);
                ProjectData.ClearProjectError();
            }
        }
    }

    public void On_CMSG_GAMEOBJ_USE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        checked
        {
            if (packet.Data.Length - 1 < 13)
            {
                return;
            }
            packet.GetInt16();
            var GameObjectGUID = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GAMEOBJ_USE [GUID={2:X}]", client.IP, client.Port, GameObjectGUID);
            if (!WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(GameObjectGUID))
            {
                return;
            }
            var GO = WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GameObjectGUID];
            client.Character.RemoveAurasByInterruptFlag(2048);
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "GameObjectType: {0}", WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GameObjectGUID].Type);
            var type = GO.Type;
            switch (type)
            {
                case GameObjectType.GAMEOBJECT_TYPE_QUESTGIVER:
                    if (type == GameObjectType.GAMEOBJECT_TYPE_QUESTGIVER)
                    {
                        var qm = WorldServiceLocator._WorldServer.ALLQUESTS.GetQuestMenuGO(ref client.Character, GameObjectGUID);
                        WorldServiceLocator._WorldServer.ALLQUESTS.SendQuestMenu(ref client.Character, GameObjectGUID, "Available quests", qm);
                    }
                    break;

                case GameObjectType.GAMEOBJECT_TYPE_DOOR:
                case GameObjectType.GAMEOBJECT_TYPE_BUTTON:
                    GO.OpenDoor();
                    break;

                case GameObjectType.GAMEOBJECT_TYPE_CHAIR:
                    {
                        Packets.PacketClass StandState = new(Opcodes.CMSG_STANDSTATECHANGE);
                        try
                        {
                            StandState.AddInt8((byte)(4L + WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GameObjectGUID].GetSound(1)));
                            client.Character.Teleport(GO.positionX, GO.positionY, GO.positionZ, GO.orientation, (int)GO.MapID);
                            client.Send(ref StandState);
                        }
                        finally
                        {
                            StandState.Dispose();
                        }
                        Packets.PacketClass packetACK = new(Opcodes.SMSG_STANDSTATE_CHANGE_ACK);
                        try
                        {
                            packetACK.AddInt8((byte)(4L + GO.GetSound(1)));
                            client.Send(ref packetACK);
                        }
                        finally
                        {
                            packetACK.Dispose();
                        }
                        break;
                    }
                case GameObjectType.GAMEOBJECT_TYPE_CAMERA:
                    {
                        Packets.PacketClass cinematicPacket = new(Opcodes.SMSG_TRIGGER_CINEMATIC);
                        cinematicPacket.AddUInt32(GO.GetSound(1));
                        client.Send(ref cinematicPacket);
                        cinematicPacket.Dispose();
                        break;
                    }
                case GameObjectType.GAMEOBJECT_TYPE_RITUAL:
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Clicked a ritual.");
                    if ((GO.Owner == client.Character.GUID || client.Character.IsInGroup) && (GO.Owner == client.Character.GUID || (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(GO.Owner) && WorldServiceLocator._WorldServer.CHARACTERs[GO.Owner].IsInGroup && WorldServiceLocator._WorldServer.CHARACTERs[GO.Owner].Group == client.Character.Group)))
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Casting ritual spell.");
                        client.Character.CastOnSelf((int)GO.GetSound(1));
                    }
                    break;

                case GameObjectType.GAMEOBJECT_TYPE_SPELLCASTER:
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Clicked a spellcaster.");
                    GO.Flags = 2;
                    if (GO.GetSound(2) != 0)
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Spellcaster requires same group.");
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Owner: {0:X}  You: {1:X}", WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GameObjectGUID].Owner, client.Character.GUID);
                        if ((GO.Owner != client.Character.GUID && !client.Character.IsInGroup) || (GO.Owner != client.Character.GUID && (!WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(GO.Owner) || !WorldServiceLocator._WorldServer.CHARACTERs[GO.Owner].IsInGroup || WorldServiceLocator._WorldServer.CHARACTERs[GO.Owner].Group != client.Character.Group)))
                        {
                            break;
                        }
                    }
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Casted spellcaster spell.");
                    client.Character.CastOnSelf((int)GO.GetSound(0));
                    break;

                case GameObjectType.GAMEOBJECT_TYPE_MEETINGSTONE:
                    if (client.Character.Level < GO.GetSound(0))
                    {
                        WorldServiceLocator._WS_Spells.SendCastResult(SpellFailedReason.SPELL_FAILED_LEVEL_REQUIREMENT, ref client, 23598);
                    }
                    else if (client.Character.Level > WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[GameObjectGUID].GetSound(1))
                    {
                        WorldServiceLocator._WS_Spells.SendCastResult(SpellFailedReason.SPELL_FAILED_LEVEL_REQUIREMENT, ref client, 23598);
                    }
                    else
                    {
                        client.Character.CastOnSelf(23598);
                    }
                    break;

                case GameObjectType.GAMEOBJECT_TYPE_FISHINGNODE:
                    if (GO.Owner != client.Character.GUID)
                    {
                        break;
                    }
                    if (GO.Loot == null)
                    {
                        if (GO.State == GameObjectLootState.DOOR_CLOSED)
                        {
                            GO.State = GameObjectLootState.DOOR_OPEN;
                            Packets.PacketClass fishNotHookedPacket = new(Opcodes.SMSG_FISH_NOT_HOOKED);
                            client.Send(ref fishNotHookedPacket);
                            fishNotHookedPacket.Dispose();
                        }
                    }
                    else
                    {
                        var AreaFlag = WorldServiceLocator._WS_Maps.GetAreaFlag(GO.positionX, GO.positionY, (int)GO.MapID);
                        var AreaID = WorldServiceLocator._WS_Maps.AreaTable[AreaFlag].ID;
                        DataTable MySQLQuery = new();
                        WorldServiceLocator._WorldServer.WorldDatabase.Query($"SELECT * FROM skill_fishing_base_level WHERE entry = {AreaID};", ref MySQLQuery);
                        if (MySQLQuery.Rows.Count == 0)
                        {
                            AreaID = WorldServiceLocator._WS_Maps.AreaTable[AreaFlag].Zone;
                            MySQLQuery.Clear();
                            WorldServiceLocator._WorldServer.WorldDatabase.Query($"SELECT * FROM skill_fishing_base_level WHERE entry = {AreaID};", ref MySQLQuery);
                        }
                        var zoneSkill = 0;
                        if (MySQLQuery.Rows.Count > 0)
                        {
                            zoneSkill = MySQLQuery.Rows[0].As<int>("skill");
                        }
                        else
                        {
                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "No fishing entry in 'skill_fishing_base_level' for area [{0}] in zone [{1}]", WorldServiceLocator._WS_Maps.AreaTable[AreaFlag].ID, WorldServiceLocator._WS_Maps.AreaTable[AreaFlag].Zone);
                        }
                        int skill = client.Character.Skills[356].CurrentWithBonus;
                        var chance = skill - zoneSkill + 5;
                        var roll = WorldServiceLocator._WorldServer.Rnd.Next(1, 101);
                        if (skill > zoneSkill && roll >= chance)
                        {
                            GO.State = GameObjectLootState.DOOR_CLOSED;
                            GO.Loot.SendLoot(ref client);
                            client.Character.UpdateSkill(356, 0.01f);
                        }
                        else
                        {
                            GO.State = GameObjectLootState.DOOR_CLOSED;
                            Packets.PacketClass fishEscaped = new(Opcodes.SMSG_FISH_ESCAPED);
                            client.Send(ref fishEscaped);
                            fishEscaped.Dispose();
                        }
                    }
                    client.Character.FinishSpell(CurrentSpellTypes.CURRENT_CHANNELED_SPELL, OK: true);
                    break;
            }
        }
    }
}
