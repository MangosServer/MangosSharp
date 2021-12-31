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
using Mangos.Common.Globals;
using Mangos.World.Objects;
using Mangos.World.Player;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections;

namespace Mangos.World.Globals;

public partial class Packets
{
    public class UpdateClass : IDisposable
    {
        public BitArray UpdateMask;

        public Hashtable UpdateData;

        private bool _disposedValue;

        public UpdateClass(int max)
        {
            UpdateData = new Hashtable();
            UpdateMask = new BitArray(max, defaultValue: false);
        }

        public void SetUpdateFlag(int pos, int value)
        {
            UpdateMask.Set(pos, value: true);
            UpdateData[pos] = value;
        }

        public void SetUpdateFlag(int pos, int index, byte value)
        {
            UpdateMask.Set(pos, value: true);
            checked
            {
                UpdateData[pos] = UpdateData.ContainsKey(pos) ? Conversions.ToInteger(UpdateData[pos]) | (value << (8 * index)) : (object)(value << (8 * index));
            }
        }

        public void SetUpdateFlag(int pos, uint value)
        {
            UpdateMask.Set(pos, value: true);
            UpdateData[pos] = value;
        }

        public void SetUpdateFlag(int pos, long value)
        {
            UpdateMask.Set(pos, value: true);
            checked
            {
                UpdateMask.Set(pos + 1, value: true);
                UpdateData[pos] = (int)(value & 0xFFFFFFFFu);
                UpdateData[pos + 1] = (int)((value >> 32) & 0xFFFFFFFFu);
            }
        }

        public void SetUpdateFlag(int pos, ulong value)
        {
            UpdateMask.Set(pos, value: true);
            checked
            {
                UpdateMask.Set(pos + 1, value: true);
                UpdateData[pos] = (uint)(value & 0xFFFFFFFFu);
                UpdateData[pos + 1] = (uint)((value >> 32) & 0xFFFFFFFFu);
            }
        }

        public void SetUpdateFlag(int pos, float value)
        {
            UpdateMask.Set(pos, value: true);
            UpdateData[pos] = value;
        }

        public void AddToPacket(ref PacketClass packet, ObjectUpdateType updateType, ref WS_Creatures.CreatureObject updateObject)
        {
            checked
            {
                packet.AddInt8((byte)updateType);
                packet.AddPackGUID(updateObject.GUID);
                if (updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT)
                {
                    packet.AddInt8(3);
                }
                if (updateType is ObjectUpdateType.UPDATETYPE_CREATE_OBJECT or ObjectUpdateType.UPDATETYPE_MOVEMENT)
                {
                    packet.AddInt8(112);
                    packet.AddInt32(8388608);
                    packet.AddInt32(WorldServiceLocator._WS_Network.MsTime());
                    packet.AddSingle(updateObject.positionX);
                    packet.AddSingle(updateObject.positionY);
                    packet.AddSingle(updateObject.positionZ);
                    packet.AddSingle(updateObject.orientation);
                    packet.AddSingle(0f);
                    packet.AddSingle(WorldServiceLocator._WorldServer.CREATURESDatabase[updateObject.ID].WalkSpeed);
                    packet.AddSingle(WorldServiceLocator._WorldServer.CREATURESDatabase[updateObject.ID].RunSpeed);
                    packet.AddSingle(WorldServiceLocator._Global_Constants.UNIT_NORMAL_SWIM_BACK_SPEED);
                    packet.AddSingle(WorldServiceLocator._Global_Constants.UNIT_NORMAL_SWIM_SPEED);
                    packet.AddSingle(WorldServiceLocator._Global_Constants.UNIT_NORMAL_WALK_BACK_SPEED);
                    packet.AddSingle(WorldServiceLocator._Global_Constants.UNIT_NORMAL_TURN_RATE);
                    packet.AddUInt32(1u);
                }
                if (updateType is ObjectUpdateType.UPDATETYPE_CREATE_OBJECT or ObjectUpdateType.UPDATETYPE_VALUES)
                {
                    var updateCount = 0;
                    var num = UpdateMask.Count - 1;
                    for (var i = 0; i <= num; i++)
                    {
                        if (UpdateMask.Get(i))
                        {
                            updateCount = i;
                        }
                    }
                    packet.AddInt8((byte)(checked(updateCount + 32) / 32));
                    packet.AddBitArray(UpdateMask, checked((byte)(checked(updateCount + 32) / 32)) * 4);
                    var num2 = UpdateMask.Count - 1;
                    for (var j = 0; j <= num2; j++)
                    {
                        if (UpdateMask.Get(j))
                        {
                            if (UpdateData[j] is uint)
                            {
                                packet.AddUInt32(Conversions.ToUInteger(UpdateData[j]));
                            }
                            else if (UpdateData[j] is float)
                            {
                                packet.AddSingle(Conversions.ToSingle(UpdateData[j]));
                            }
                            else
                            {
                                packet.AddInt32(Conversions.ToInteger(UpdateData[j]));
                            }
                        }
                    }
                    UpdateMask.SetAll(value: false);
                }
                if (packet is UpdatePacketClass @class)
                {
                    @class.UpdatesCount++;
                }
            }
        }

        public void AddToPacket(ref PacketClass packet, ObjectUpdateType updateType, ref WS_PlayerData.CharacterObject updateObject)
        {
            packet.AddInt8(checked((byte)updateType));
            packet.AddPackGUID(updateObject.GUID);
            if (updateType is ObjectUpdateType.UPDATETYPE_CREATE_OBJECT or ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF)
            {
                packet.AddInt8(4);
            }
            if (updateType is ObjectUpdateType.UPDATETYPE_CREATE_OBJECT or ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF or ObjectUpdateType.UPDATETYPE_MOVEMENT)
            {
                var flags2 = updateObject.charMovementFlags & 0xFF;
                if (updateObject.OnTransport != null)
                {
                    flags2 |= 0x2000000;
                }
                packet.AddInt8(112);
                packet.AddInt32(flags2);
                packet.AddInt32(WorldServiceLocator._WS_Network.MsTime());
                packet.AddSingle(updateObject.positionX);
                packet.AddSingle(updateObject.positionY);
                packet.AddSingle(updateObject.positionZ);
                packet.AddSingle(updateObject.orientation);
                if (((uint)flags2 & 0x2000000u) != 0)
                {
                    packet.AddUInt64(updateObject.OnTransport.GUID);
                    packet.AddSingle(updateObject.transportX);
                    packet.AddSingle(updateObject.transportY);
                    packet.AddSingle(updateObject.transportZ);
                    packet.AddSingle(updateObject.orientation);
                }
                packet.AddInt32(0);
                packet.AddSingle(updateObject.WalkSpeed);
                packet.AddSingle(updateObject.RunSpeed);
                packet.AddSingle(updateObject.RunBackSpeed);
                packet.AddSingle(updateObject.SwimSpeed);
                packet.AddSingle(updateObject.SwimBackSpeed);
                packet.AddSingle(updateObject.TurnRate);
                packet.AddUInt32(WorldServiceLocator._CommonGlobalFunctions.GuidLow(updateObject.GUID));
            }
            checked
            {
                if (updateType is ObjectUpdateType.UPDATETYPE_CREATE_OBJECT or ObjectUpdateType.UPDATETYPE_VALUES or ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF)
                {
                    var updateCount = 0;
                    var num = UpdateMask.Count - 1;
                    for (var i = 0; i <= num; i++)
                    {
                        if (UpdateMask.Get(i))
                        {
                            updateCount = i;
                        }
                    }
                    packet.AddInt8((byte)(checked(updateCount + 32) / 32));
                    packet.AddBitArray(UpdateMask, checked((byte)(checked(updateCount + 32) / 32)) * 4);
                    var num2 = UpdateMask.Count - 1;
                    for (var j = 0; j <= num2; j++)
                    {
                        if (UpdateMask.Get(j))
                        {
                            if (UpdateData[j] is uint)
                            {
                                packet.AddUInt32(Conversions.ToUInteger(UpdateData[j]));
                            }
                            else if (UpdateData[j] is float)
                            {
                                packet.AddSingle(Conversions.ToSingle(UpdateData[j]));
                            }
                            else
                            {
                                packet.AddInt32(Conversions.ToInteger(UpdateData[j]));
                            }
                        }
                    }
                    UpdateMask.SetAll(value: false);
                }
                if (packet is UpdatePacketClass @class)
                {
                    @class.UpdatesCount++;
                }
            }
        }

        public void AddToPacket(ref PacketClass packet, ObjectUpdateType updateType, ref ItemObject updateObject)
        {
            checked
            {
                packet.AddInt8((byte)updateType);
                packet.AddPackGUID(updateObject.GUID);
                if (updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT)
                {
                    if (WorldServiceLocator._WorldServer.ITEMDatabase[updateObject.ItemEntry].ContainerSlots > 0)
                    {
                        packet.AddInt8(2);
                    }
                    else
                    {
                        packet.AddInt8(1);
                    }
                    packet.AddInt8(24);
                    packet.AddUInt64(updateObject.GUID);
                }
                if (updateType is ObjectUpdateType.UPDATETYPE_CREATE_OBJECT or ObjectUpdateType.UPDATETYPE_VALUES)
                {
                    var updateCount = 0;
                    var num = UpdateMask.Count - 1;
                    for (var j = 0; j <= num; j++)
                    {
                        if (UpdateMask.Get(j))
                        {
                            updateCount = j;
                        }
                    }
                    packet.AddInt8((byte)(checked(updateCount + 32) / 32));
                    packet.AddBitArray(UpdateMask, checked((byte)(checked(updateCount + 32) / 32)) * 4);
                    var num2 = UpdateMask.Count - 1;
                    for (var i = 0; i <= num2; i++)
                    {
                        if (UpdateMask.Get(i))
                        {
                            if (UpdateData[i] is uint)
                            {
                                packet.AddUInt32(Conversions.ToUInteger(UpdateData[i]));
                            }
                            else if (UpdateData[i] is float)
                            {
                                packet.AddSingle(Conversions.ToSingle(UpdateData[i]));
                            }
                            else
                            {
                                packet.AddInt32(Conversions.ToInteger(UpdateData[i]));
                            }
                        }
                    }
                    UpdateMask.SetAll(value: false);
                }
                if (packet is UpdatePacketClass @class)
                {
                    @class.UpdatesCount++;
                }
            }
        }

        public void AddToPacket(ref PacketClass packet, ObjectUpdateType updateType, ref WS_GameObjects.GameObject updateObject)
        {
            checked
            {
                packet.AddInt8((byte)updateType);
                packet.AddPackGUID(updateObject.GUID);
                switch (updateObject.Type)
                {
                    case GameObjectType.GAMEOBJECT_TYPE_TRAP:
                    case GameObjectType.GAMEOBJECT_TYPE_DUEL_ARBITER:
                    case GameObjectType.GAMEOBJECT_TYPE_FLAGSTAND:
                    case GameObjectType.GAMEOBJECT_TYPE_FLAGDROP:
                        updateType = ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF;
                        break;
                }
                if (updateType is ObjectUpdateType.UPDATETYPE_CREATE_OBJECT or ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF)
                {
                    packet.AddInt8(5);
                    if (updateObject.Type is GameObjectType.GAMEOBJECT_TYPE_TRANSPORT or GameObjectType.GAMEOBJECT_TYPE_MO_TRANSPORT)
                    {
                        packet.AddInt8(82);
                    }
                    else
                    {
                        packet.AddInt8(80);
                    }
                    if (updateObject.Type == GameObjectType.GAMEOBJECT_TYPE_MO_TRANSPORT)
                    {
                        packet.AddSingle(0f);
                        packet.AddSingle(0f);
                        packet.AddSingle(0f);
                        packet.AddSingle(updateObject.orientation);
                    }
                    else
                    {
                        packet.AddSingle(updateObject.positionX);
                        packet.AddSingle(updateObject.positionY);
                        packet.AddSingle(updateObject.positionZ);
                        packet.AddSingle(updateObject.orientation);
                    }
                    packet.AddUInt32(WorldServiceLocator._CommonGlobalFunctions.GuidHigh(updateObject.GUID));
                    if (updateObject.Type is GameObjectType.GAMEOBJECT_TYPE_TRANSPORT or GameObjectType.GAMEOBJECT_TYPE_MO_TRANSPORT)
                    {
                        packet.AddInt32(WorldServiceLocator._WS_Network.MsTime());
                    }
                }
                if (updateType is ObjectUpdateType.UPDATETYPE_CREATE_OBJECT or ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF or ObjectUpdateType.UPDATETYPE_VALUES)
                {
                    var updateCount = 0;
                    var num = UpdateMask.Count - 1;
                    for (var i = 0; i <= num; i++)
                    {
                        if (UpdateMask.Get(i))
                        {
                            updateCount = i;
                        }
                    }
                    packet.AddInt8((byte)(checked(updateCount + 32) / 32));
                    packet.AddBitArray(UpdateMask, checked((byte)(checked(updateCount + 32) / 32)) * 4);
                    var num2 = UpdateMask.Count - 1;
                    for (var j = 0; j <= num2; j++)
                    {
                        if (UpdateMask.Get(j))
                        {
                            if (UpdateData[j] is uint)
                            {
                                packet.AddUInt32(Conversions.ToUInteger(UpdateData[j]));
                            }
                            else if (UpdateData[j] is float)
                            {
                                packet.AddSingle(Conversions.ToSingle(UpdateData[j]));
                            }
                            else
                            {
                                packet.AddInt32(Conversions.ToInteger(UpdateData[j]));
                            }
                        }
                    }
                    UpdateMask.SetAll(value: false);
                }
                if (packet is UpdatePacketClass @class)
                {
                    @class.UpdatesCount++;
                }
            }
        }

        public void AddToPacket(ref PacketClass packet, ObjectUpdateType updateType, ref WS_DynamicObjects.DynamicObject updateObject)
        {
            checked
            {
                packet.AddInt8((byte)updateType);
                packet.AddPackGUID(updateObject.GUID);
                if (updateType is ObjectUpdateType.UPDATETYPE_CREATE_OBJECT or ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF)
                {
                    packet.AddInt8(6);
                    packet.AddInt8(88);
                    packet.AddSingle(updateObject.positionX);
                    packet.AddSingle(updateObject.positionY);
                    packet.AddSingle(updateObject.positionZ);
                    packet.AddSingle(updateObject.orientation);
                    packet.AddUInt64(updateObject.GUID);
                }
                if (updateType is ObjectUpdateType.UPDATETYPE_CREATE_OBJECT or ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF or ObjectUpdateType.UPDATETYPE_VALUES)
                {
                    var updateCount = 0;
                    var num = UpdateMask.Count - 1;
                    for (var i = 0; i <= num; i++)
                    {
                        if (UpdateMask.Get(i))
                        {
                            updateCount = i;
                        }
                    }
                    packet.AddInt8((byte)(checked(updateCount + 32) / 32));
                    packet.AddBitArray(UpdateMask, checked((byte)(checked(updateCount + 32) / 32)) * 4);
                    var num2 = UpdateMask.Count - 1;
                    for (var j = 0; j <= num2; j++)
                    {
                        if (UpdateMask.Get(j))
                        {
                            if (UpdateData[j] is uint)
                            {
                                packet.AddUInt32(Conversions.ToUInteger(UpdateData[j]));
                            }
                            else if (UpdateData[j] is float)
                            {
                                packet.AddSingle(Conversions.ToSingle(UpdateData[j]));
                            }
                            else
                            {
                                packet.AddInt32(Conversions.ToInteger(UpdateData[j]));
                            }
                        }
                    }
                    UpdateMask.SetAll(value: false);
                }
                if (packet is UpdatePacketClass @class)
                {
                    @class.UpdatesCount++;
                }
            }
        }

        public void AddToPacket(ref PacketClass packet, ObjectUpdateType updateType, ref WS_Corpses.CorpseObject updateObject)
        {
            checked
            {
                packet.AddInt8((byte)updateType);
                packet.AddPackGUID(updateObject.GUID);
                if (updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT)
                {
                    packet.AddInt8(7);
                    packet.AddInt8(88);
                    packet.AddSingle(updateObject.positionX);
                    packet.AddSingle(updateObject.positionY);
                    packet.AddSingle(updateObject.positionZ);
                    packet.AddSingle(updateObject.orientation);
                    packet.AddUInt64(updateObject.GUID);
                }
                if (updateType is ObjectUpdateType.UPDATETYPE_CREATE_OBJECT or ObjectUpdateType.UPDATETYPE_VALUES)
                {
                    var updateCount = 0;
                    var num = UpdateMask.Count - 1;
                    for (var j = 0; j <= num; j++)
                    {
                        if (UpdateMask.Get(j))
                        {
                            updateCount = j;
                        }
                    }
                    packet.AddInt8((byte)(checked(updateCount + 32) / 32));
                    packet.AddBitArray(UpdateMask, checked((byte)(checked(updateCount + 32) / 32)) * 4);
                    var num2 = UpdateMask.Count - 1;
                    for (var i = 0; i <= num2; i++)
                    {
                        if (UpdateMask.Get(i))
                        {
                            if (UpdateData[i] is uint)
                            {
                                packet.AddUInt32(Conversions.ToUInteger(UpdateData[i]));
                            }
                            else if (UpdateData[i] is float)
                            {
                                packet.AddSingle(Conversions.ToSingle(UpdateData[i]));
                            }
                            else
                            {
                                packet.AddInt32(Conversions.ToInteger(UpdateData[i]));
                            }
                        }
                    }
                    UpdateMask.SetAll(value: false);
                }
                if (packet is UpdatePacketClass @class)
                {
                    @class.UpdatesCount++;
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
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
}
