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
using System.Collections;
using System.Runtime.InteropServices;
using Mangos.Common.Enums.GameObject;
using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using Mangos.World.Objects;
using Mangos.World.Player;
using Mangos.World.Server;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Globals
{
    public class Packets
    {
        /// <summary>
        /// Dumps the packet.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="client">The client.</param>
        /// <param name="start">The start.</param>
        /// <returns></returns>
        public void DumpPacket(byte[] data, [Optional, DefaultParameterValue(null)] ref WS_Network.ClientClass client, int start = 0)
        {
            // #If DEBUG Then
            int j;
            string buffer = "";
            try
            {
                if (client is null)
                {
                    buffer += string.Format("DEBUG: Packet Dump{0}", Environment.NewLine);
                }
                else
                {
                    buffer += string.Format("[{0}:{1}] DEBUG: Packet Dump - Length={2}{3}", client.IP, client.Port, data.Length - start, Environment.NewLine);
                }

                if ((data.Length - start) % 16 == 0)
                {
                    var loopTo = data.Length - 1;
                    for (j = start; j <= loopTo; j += 16)
                    {
                        buffer += "|  " + BitConverter.ToString(data, j, 16).Replace("-", " ");
                        buffer += " |  " + System.Text.Encoding.ASCII.GetString(data, j, 16).Replace(Constants.vbTab, "?").Replace(Constants.vbBack, "?").Replace(Constants.vbCr, "?").Replace(Constants.vbFormFeed, "?").Replace(Constants.vbLf, "?") + " |" + Environment.NewLine;
                    }
                }
                else
                {
                    var loopTo1 = data.Length - 1 - 16;
                    for (j = start; j <= loopTo1; j += 16)
                    {
                        buffer += "|  " + BitConverter.ToString(data, j, 16).Replace("-", " ");
                        buffer += " |  " + System.Text.Encoding.ASCII.GetString(data, j, 16).Replace(Constants.vbTab, "?").Replace(Constants.vbBack, "?").Replace(Constants.vbCr, "?").Replace(Constants.vbFormFeed, "?").Replace(Constants.vbLf, "?") + " |" + Environment.NewLine;
                    }

                    buffer += "|  " + BitConverter.ToString(data, j, (data.Length - start) % 16).Replace("-", " ");
                    buffer += new string(' ', (16 - (data.Length - start) % 16) * 3);
                    buffer += " |  " + System.Text.Encoding.ASCII.GetString(data, j, (data.Length - start) % 16).Replace(Constants.vbTab, "?").Replace(Constants.vbBack, "?").Replace(Constants.vbCr, "?").Replace(Constants.vbFormFeed, "?").Replace(Constants.vbLf, "?");
                    buffer += new string(' ', 16 - (data.Length - start) % 16);
                    buffer += " |" + Environment.NewLine;
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, buffer, default);
            }
            // #End If
            catch (Exception e)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error dumping packet: {0}{1}", Environment.NewLine, e.ToString());
            }
        }

        public class UpdateClass : IDisposable
        {

            // Dim packet As New PacketClass(OPCODES.SMSG_UPDATE_OBJECT)
            // packet.AddInt32(OPERATIONS_COUNT)
            // packet.AddInt8(0)

            public BitArray UpdateMask;
            public Hashtable UpdateData = new Hashtable();

            public UpdateClass(int max)
            {
                UpdateMask = new BitArray(max, false);
            }

            /// <summary>
            /// Sets the update flag.
            /// </summary>
            /// <param name="pos">The pos.</param>
            /// <param name="value">The value.</param>
            /// <returns></returns>
            public void SetUpdateFlag(EObjectFields oBJECT_FIELD_TYPE, int pos, int value)
            {
                UpdateMask.Set(pos, true);
                UpdateData[pos] = value;
            }

            /// <summary>
            /// Sets the update flag.
            /// </summary>
            /// <param name="pos">The pos.</param>
            /// <param name="index">The index.</param>
            /// <param name="value">The value.</param>
            /// <returns></returns>
            public void SetUpdateFlag(int pos, int index, byte value)
            {
                UpdateMask.Set(pos, true);
                if (UpdateData.ContainsKey(pos))
                {
                    UpdateData[pos] = Conversions.ToInteger(UpdateData[pos]) | value << 8 * index;
                }
                else
                {
                    UpdateData[pos] = value << 8 * index;
                }
            }

            /// <summary>
            /// Sets the update flag.
            /// </summary>
            /// <param name="pos">The pos.</param>
            /// <param name="value">The value.</param>
            /// <returns></returns>
            public void SetUpdateFlag(EObjectFields oBJECT_FIELD_TYPE, int pos, uint value)
            {
                UpdateMask.Set(pos, true);
                UpdateData[pos] = value;
            }

            /// <summary>
            /// Sets the update flag.
            /// </summary>
            /// <param name="pos">The pos.</param>
            /// <param name="value">The value.</param>
            /// <returns></returns>
            public void SetUpdateFlag(int pos, long value)
            {
                UpdateMask.Set(pos, true);
                UpdateMask.Set(pos + 1, true);
                UpdateData[pos] = Conversions.ToInteger(value & uint.MaxValue);
                UpdateData[pos + 1] = Conversions.ToInteger(value >> 32 & uint.MaxValue);
            }

            /// <summary>
            /// Sets the update flag.
            /// </summary>
            /// <param name="pos">The pos.</param>
            /// <param name="value">The value.</param>
            /// <returns></returns>
            public void SetUpdateFlag(int pos, ulong value)
            {
                UpdateMask.Set(pos, true);
                UpdateMask.Set(pos + 1, true);
                UpdateData[pos] = Conversions.ToUInteger(value & uint.MaxValue);
                UpdateData[pos + 1] = Conversions.ToUInteger(value >> 32 & uint.MaxValue);
            }

            /// <summary>
            /// Sets the update flag.
            /// </summary>
            /// <param name="pos">The pos.</param>
            /// <param name="value">The value.</param>
            /// <returns></returns>
            public void SetUpdateFlag(int pos, float value)
            {
                UpdateMask.Set(pos, true);
                UpdateData[pos] = value;
            }

            /// <summary>
            /// Adds to packet.
            /// </summary>
            /// <param name="packet">The packet.</param>
            /// <param name="updateType">Type of the update.</param>
            /// <param name="updateObject">The update object.</param>
            /// <returns></returns>
            public void AddToPacket(ref PacketClass packet, ObjectUpdateType updateType, ref WS_Creatures.CreatureObject updateObject)
            {
                packet.AddInt8((byte)updateType);
                packet.AddPackGUID(updateObject.GUID);
                if (updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT)
                {
                    packet.AddInt8((byte)ObjectTypeID.TYPEID_UNIT);
                }

                if (updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT || updateType == ObjectUpdateType.UPDATETYPE_MOVEMENT)
                {
                    // TODO: If creature is moving when this packet is created, send it here with help of movementflags?

                    packet.AddInt8(0x70);
                    packet.AddInt32(0x800000);  // movementflags
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
                    packet.AddUInt32(1U);
                }

                if (updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT || updateType == ObjectUpdateType.UPDATETYPE_VALUES)
                {
                    int updateCount = 0;
                    for (int i = 0, loopTo = UpdateMask.Count - 1; i <= loopTo; i++)
                    {
                        if (UpdateMask.Get(i))
                            updateCount = i;
                    }

                    packet.AddInt8((byte)((updateCount + 32) / 32));
                    packet.AddBitArray(UpdateMask, Conversions.ToByte((updateCount + 32) / 32) * 4);      // OK Flags are Int32, so to byte -> *4
                    for (int i = 0, loopTo1 = UpdateMask.Count - 1; i <= loopTo1; i++)
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

                    UpdateMask.SetAll(false);
                }

                if (packet is UpdatePacketClass)
                    ((UpdatePacketClass)packet).UpdatesCount += 1;
            }

            /// <summary>
            /// Adds to packet.
            /// </summary>
            /// <param name="packet">The packet.</param>
            /// <param name="updateType">Type of the update.</param>
            /// <param name="updateObject">The update object.</param>
            /// <returns></returns>
            public void AddToPacket(ref PacketClass packet, ObjectUpdateType updateType, ref WS_PlayerData.CharacterObject updateObject)
            {
                packet.AddInt8((byte)updateType);
                packet.AddPackGUID(updateObject.GUID);
                if (updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT || updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF)
                {
                    packet.AddInt8((byte)ObjectTypeID.TYPEID_PLAYER);
                }

                if (updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT || updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF || updateType == ObjectUpdateType.UPDATETYPE_MOVEMENT)
                {
                    int flags2 = updateObject.charMovementFlags & 0xFF;
                    if (updateObject.OnTransport is object)
                    {
                        flags2 = flags2 | MovementFlags.MOVEMENTFLAG_ONTRANSPORT;
                    }

                    packet.AddInt8(0x70);        // flags
                    packet.AddInt32(flags2);     // flags 2
                    packet.AddInt32(WorldServiceLocator._WS_Network.MsTime());
                    packet.AddSingle(updateObject.positionX);
                    packet.AddSingle(updateObject.positionY);
                    packet.AddSingle(updateObject.positionZ);
                    packet.AddSingle(updateObject.orientation);
                    if (flags2 & MovementFlags.MOVEMENTFLAG_ONTRANSPORT)
                    {
                        packet.AddUInt64(updateObject.OnTransport.GUID);
                        packet.AddSingle(updateObject.transportX);
                        packet.AddSingle(updateObject.transportY);
                        packet.AddSingle(updateObject.transportZ);
                        packet.AddSingle(updateObject.orientation);
                    }

                    packet.AddInt32(0);          // Last fall time
                    packet.AddSingle(updateObject.WalkSpeed);
                    packet.AddSingle(updateObject.RunSpeed);
                    packet.AddSingle(updateObject.RunBackSpeed);
                    packet.AddSingle(updateObject.SwimSpeed);
                    packet.AddSingle(updateObject.SwimBackSpeed);
                    packet.AddSingle(updateObject.TurnRate);
                    packet.AddUInt32(WorldServiceLocator._CommonGlobalFunctions.GuidLow(updateObject.GUID));
                }

                if (updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT || updateType == ObjectUpdateType.UPDATETYPE_VALUES || updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF)
                {
                    int updateCount = 0;
                    for (int i = 0, loopTo = UpdateMask.Count - 1; i <= loopTo; i++)
                    {
                        if (UpdateMask.Get(i))
                            updateCount = i;
                    }

                    packet.AddInt8((byte)((updateCount + 32) / 32));
                    packet.AddBitArray(UpdateMask, Conversions.ToByte((updateCount + 32) / 32) * 4);      // OK Flags are Int32, so to byte -> *4
                    for (int i = 0, loopTo1 = UpdateMask.Count - 1; i <= loopTo1; i++)
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

                    UpdateMask.SetAll(false);
                }

                if (packet is UpdatePacketClass)
                    ((UpdatePacketClass)packet).UpdatesCount += 1;
            }

            /// <summary>
            /// Adds to packet.
            /// </summary>
            /// <param name="packet">The packet.</param>
            /// <param name="updateType">Type of the update.</param>
            /// <param name="updateObject">The update object.</param>
            /// <returns></returns>
            public void AddToPacket(ref PacketClass packet, ObjectUpdateType updateType, ref ItemObject updateObject)
            {
                packet.AddInt8((byte)updateType);
                packet.AddPackGUID(updateObject.GUID);
                if (updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT)
                {
                    if (WorldServiceLocator._WorldServer.ITEMDatabase[updateObject.ItemEntry].ContainerSlots > 0)
                    {
                        packet.AddInt8((byte)ObjectTypeID.TYPEID_CONTAINER);
                    }
                    else
                    {
                        packet.AddInt8((byte)ObjectTypeID.TYPEID_ITEM);
                    }

                    packet.AddInt8(0x18);
                    packet.AddUInt64(updateObject.GUID);
                }

                if (updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT | updateType == ObjectUpdateType.UPDATETYPE_VALUES)
                {
                    int updateCount = 0;
                    for (int i = 0, loopTo = UpdateMask.Count - 1; i <= loopTo; i++)
                    {
                        if (UpdateMask.Get(i))
                            updateCount = i;
                    }

                    packet.AddInt8((byte)((updateCount + 32) / 32));
                    packet.AddBitArray(UpdateMask, Conversions.ToByte((updateCount + 32) / 32) * 4);      // OK Flags are Int32, so to byte -> *4
                    for (int i = 0, loopTo1 = UpdateMask.Count - 1; i <= loopTo1; i++)
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

                    UpdateMask.SetAll(false);
                }

                if (packet is UpdatePacketClass)
                    ((UpdatePacketClass)packet).UpdatesCount += 1;
            }

            /// <summary>
            /// Adds to packet.
            /// </summary>
            /// <param name="packet">The packet.</param>
            /// <param name="updateType">Type of the update.</param>
            /// <param name="updateObject">The update object.</param>
            /// <returns></returns>
            public void AddToPacket(ref PacketClass packet, ObjectUpdateType updateType, ref WS_GameObjects.GameObjectObject updateObject)
            {
                packet.AddInt8((byte)updateType);
                packet.AddPackGUID(updateObject.GUID);
                switch (updateObject.Type)
                {
                    case var @case when @case == GameObjectType.GAMEOBJECT_TYPE_DUEL_ARBITER:
                    case var case1 when case1 == GameObjectType.GAMEOBJECT_TYPE_TRAP:
                    case var case2 when case2 == GameObjectType.GAMEOBJECT_TYPE_FLAGDROP:
                    case var case3 when case3 == GameObjectType.GAMEOBJECT_TYPE_FLAGSTAND:
                        {
                            updateType = ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF;
                            break;
                        }
                }

                if (updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT || updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF)
                {
                    packet.AddInt8((byte)ObjectTypeID.TYPEID_GAMEOBJECT);

                    // packet.AddInt8(&H58)
                    if (updateObject.Type == GameObjectType.GAMEOBJECT_TYPE_TRANSPORT || updateObject.Type == GameObjectType.GAMEOBJECT_TYPE_MO_TRANSPORT)
                    {
                        packet.AddInt8(0x52);
                    }
                    else
                    {
                        packet.AddInt8(0x50);
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

                    // packet.AddUInt64(updateObject.GUID)
                    packet.AddUInt32(WorldServiceLocator._CommonGlobalFunctions.GuidHigh(updateObject.GUID));
                    if (updateObject.Type == GameObjectType.GAMEOBJECT_TYPE_TRANSPORT || updateObject.Type == GameObjectType.GAMEOBJECT_TYPE_MO_TRANSPORT)
                    {
                        packet.AddInt32(WorldServiceLocator._WS_Network.MsTime());
                    }
                }

                if (updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT || updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF || updateType == ObjectUpdateType.UPDATETYPE_VALUES)
                {
                    int updateCount = 0;
                    for (int i = 0, loopTo = UpdateMask.Count - 1; i <= loopTo; i++)
                    {
                        if (UpdateMask.Get(i))
                            updateCount = i;
                    }

                    packet.AddInt8((byte)((updateCount + 32) / 32));
                    packet.AddBitArray(UpdateMask, Conversions.ToByte((updateCount + 32) / 32) * 4);      // OK Flags are Int32, so to byte -> *4
                    for (int i = 0, loopTo1 = UpdateMask.Count - 1; i <= loopTo1; i++)
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

                    UpdateMask.SetAll(false);
                }

                if (packet is UpdatePacketClass)
                    ((UpdatePacketClass)packet).UpdatesCount += 1;
            }

            /// <summary>
            /// Adds to packet.
            /// </summary>
            /// <param name="packet">The packet.</param>
            /// <param name="updateType">Type of the update.</param>
            /// <param name="updateObject">The update object.</param>
            /// <returns></returns>
            public void AddToPacket(ref PacketClass packet, ObjectUpdateType updateType, ref WS_DynamicObjects.DynamicObjectObject updateObject)
            {
                packet.AddInt8((byte)updateType);
                packet.AddPackGUID(updateObject.GUID);
                if (updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT || updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF)
                {
                    packet.AddInt8((byte)ObjectTypeID.TYPEID_DYNAMICOBJECT);
                    packet.AddInt8(0x58);
                    packet.AddSingle(updateObject.positionX);
                    packet.AddSingle(updateObject.positionY);
                    packet.AddSingle(updateObject.positionZ);
                    packet.AddSingle(updateObject.orientation);
                    packet.AddUInt64(updateObject.GUID);
                }

                if (updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT || updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF || updateType == ObjectUpdateType.UPDATETYPE_VALUES)
                {
                    int updateCount = 0;
                    for (int i = 0, loopTo = UpdateMask.Count - 1; i <= loopTo; i++)
                    {
                        if (UpdateMask.Get(i))
                            updateCount = i;
                    }

                    packet.AddInt8((byte)((updateCount + 32) / 32));
                    packet.AddBitArray(UpdateMask, Conversions.ToByte((updateCount + 32) / 32) * 4);      // OK Flags are Int32, so to byte -> *4
                    for (int i = 0, loopTo1 = UpdateMask.Count - 1; i <= loopTo1; i++)
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

                    UpdateMask.SetAll(false);
                }

                if (packet is UpdatePacketClass)
                    ((UpdatePacketClass)packet).UpdatesCount += 1;
            }

            /// <summary>
            /// Adds to packet.
            /// </summary>
            /// <param name="packet">The packet.</param>
            /// <param name="updateType">Type of the update.</param>
            /// <param name="updateObject">The update object.</param>
            /// <returns></returns>
            public void AddToPacket(ref PacketClass packet, ObjectUpdateType updateType, ref WS_Corpses.CorpseObject updateObject)
            {
                packet.AddInt8((byte)updateType);
                packet.AddPackGUID(updateObject.GUID);
                if (updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT)
                {
                    packet.AddInt8((byte)ObjectTypeID.TYPEID_CORPSE);
                    packet.AddInt8(0x58);
                    packet.AddSingle(updateObject.positionX);
                    packet.AddSingle(updateObject.positionY);
                    packet.AddSingle(updateObject.positionZ);
                    packet.AddSingle(updateObject.orientation);
                    packet.AddUInt64(updateObject.GUID);
                    // packet.AddInt32(1)
                }

                if (updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT | updateType == ObjectUpdateType.UPDATETYPE_VALUES)
                {
                    int updateCount = 0;
                    for (int i = 0, loopTo = UpdateMask.Count - 1; i <= loopTo; i++)
                    {
                        if (UpdateMask.Get(i))
                            updateCount = i;
                    }

                    packet.AddInt8((byte)((updateCount + 32) / 32));
                    packet.AddBitArray(UpdateMask, Conversions.ToByte((updateCount + 32) / 32) * 4);      // OK Flags are Int32, so to byte -> *4
                    for (int i = 0, loopTo1 = UpdateMask.Count - 1; i <= loopTo1; i++)
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

                    UpdateMask.SetAll(false);
                }

                if (packet is UpdatePacketClass)
                    ((UpdatePacketClass)packet).UpdatesCount += 1;
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

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public class PacketClass : IDisposable
        {
            public byte[] Data;
            public int Offset = 4;

            public int Length
            {
                get
                {
                    return Data[1] + Data[0] * 256;
                }
            }

            public OPCODES OpCode
            {
                get
                {
                    if (Information.UBound(Data) > 2)
                    {
                        return (OPCODES)(Data[2] + Data[3] * 256);
                    }
                    else
                    {
                        // If it's a dodgy packet, change it to a null packet
                        return 0;
                    }
                }
            }

            public PacketClass(OPCODES opcode)
            {
                Data = new byte[4];
                Data[0] = 0;
                Data[1] = 0;
                Data[2] = (byte)(Conversions.ToShort(opcode) % 256);
                Data[3] = (byte)(Conversions.ToShort(opcode) / 256);
            }

            public PacketClass(ref byte[] rawdata)
            {
                Data = rawdata;
                rawdata.CopyTo(Data, 0);
            }

            /// <summary>
            /// Compresses the update packet.
            /// </summary>
            /// <returns></returns>
            public void CompressUpdatePacket()
            {
                if (OpCode != OPCODES.SMSG_UPDATE_OBJECT)
                    return; // Wrong packet type
                if (Data.Length < 200)
                    return; // Too small packet
                int uncompressedSize = Data.Length;
                byte[] compressedBuffer = WorldServiceLocator._GlobalZip.Compress(Data, 4, Data.Length - 4);
                if (compressedBuffer.Length == 0)
                    return;
                Data = new byte[4];
                Data[0] = 0;
                Data[1] = 0;
                Data[2] = (byte)(Conversions.ToShort(OPCODES.SMSG_COMPRESSED_UPDATE_OBJECT) % 256);
                Data[3] = (byte)(Conversions.ToShort(OPCODES.SMSG_COMPRESSED_UPDATE_OBJECT) / 256);
                AddInt32(uncompressedSize);
                AddByteArray(compressedBuffer);
                UpdateLength(); // Update packet size
            }

            /// <summary>
            /// Adds the bit array.
            /// </summary>
            /// <param name="buffer">The buffer.</param>
            /// <param name="arraryLen">The len.</param>
            /// <returns></returns>
            public void AddBitArray(BitArray buffer, int arraryLen)
            {
                Array.Resize(ref Data, Data.Length - 1 + arraryLen + 1);
                var bufferarray = new byte[(Conversions.ToByte((buffer.Length + 8) / 8d) + 1)];
                buffer.CopyTo(bufferarray, 0);
                Array.Copy(bufferarray, 0, Data, Data.Length - arraryLen, arraryLen);
            }

            /// <summary>
            /// Adds the int8.
            /// </summary>
            /// <param name="buffer">The buffer.</param>
            /// <param name="position">The position.</param>
            /// <returns></returns>
            public void AddInt8(byte buffer, int position = 0)
            {
                if (position <= 0 || position >= Data.Length)
                {
                    position = Data.Length;
                    Array.Resize(ref Data, Data.Length + 1);
                }

                Data[position] = buffer;
            }

            /// <summary>
            /// Adds the int16.
            /// </summary>
            /// <param name="buffer">The buffer.</param>
            /// <param name="position">The position.</param>
            /// <returns></returns>
            public void AddInt16(short buffer, int position = 0)
            {
                if (position <= 0 || position >= Data.Length)
                {
                    position = Data.Length;
                    Array.Resize(ref Data, Data.Length + 1 + 1);
                }

                Data[position] = (byte)(buffer & 255);
                Data[position + 1] = (byte)(buffer >> 8 & 255);
            }

            /// <summary>
            /// Adds the int32.
            /// </summary>
            /// <param name="buffer">The buffer.</param>
            /// <param name="position">The position.</param>
            /// <returns></returns>
            public void AddInt32(int buffer, int position = 0)
            {
                if (position <= 0 || position > Data.Length - 3)
                {
                    position = Data.Length;
                    Array.Resize(ref Data, Data.Length + 3 + 1);
                }

                Data[position] = (byte)(buffer & 255);
                Data[position + 1] = (byte)(buffer >> 8 & 255);
                Data[position + 2] = (byte)(buffer >> 16 & 255);
                Data[position + 3] = (byte)(buffer >> 24 & 255);
            }

            /// <summary>
            /// Adds the int64.
            /// </summary>
            /// <param name="buffer">The buffer.</param>
            /// <param name="position">The position.</param>
            /// <returns></returns>
            public void AddInt64(long buffer, int position = 0)
            {
                if (position <= 0 || position > Data.Length - 7)
                {
                    position = Data.Length;
                    Array.Resize(ref Data, Data.Length + 7 + 1);
                }

                Data[position] = (byte)(buffer & 255L);
                Data[position + 1] = (byte)(buffer >> 8 & 255L);
                Data[position + 2] = (byte)(buffer >> 16 & 255L);
                Data[position + 3] = (byte)(buffer >> 24 & 255L);
                Data[position + 4] = (byte)(buffer >> 32 & 255L);
                Data[position + 5] = (byte)(buffer >> 40 & 255L);
                Data[position + 6] = (byte)(buffer >> 48 & 255L);
                Data[position + 7] = (byte)(buffer >> 56 & 255L);
            }

            /// <summary>
            /// Adds the string.
            /// </summary>
            /// <param name="buffer">The buffer.</param>
            /// <returns></returns>
            public void AddString(string buffer)
            {
                if (Information.IsDBNull(buffer) | string.IsNullOrEmpty(buffer))
                {
                    AddInt8(0);
                }
                else
                {
                    var bytes = System.Text.Encoding.UTF8.GetBytes(buffer.ToCharArray());
                    Array.Resize(ref Data, Data.Length + bytes.Length + 1);
                    for (int i = 0, loopTo = bytes.Length - 1; i <= loopTo; i++)
                        Data[Data.Length - 1 - bytes.Length + i] = bytes[i];
                    Data[Data.Length - 1] = 0;
                }
            }

            /// <summary>
            /// Adds the string2.
            /// </summary>
            /// <param name="buffer">The buffer.</param>
            /// <returns></returns>
            public void AddString2(string buffer)
            {
                if (Information.IsDBNull(buffer) | string.IsNullOrEmpty(buffer))
                {
                    AddInt8(0);
                }
                else
                {
                    var bytes = System.Text.Encoding.UTF8.GetBytes(buffer.ToCharArray());
                    Array.Resize(ref Data, Data.Length + bytes.Length + 1);
                    Data[Data.Length - 1 - bytes.Length] = (byte)bytes.Length;
                    for (int i = 0, loopTo = bytes.Length - 1; i <= loopTo; i++)
                        Data[Data.Length - bytes.Length + i] = bytes[i];
                }
            }

            /// <summary>
            /// Adds the single.
            /// </summary>
            /// <param name="buffer2">The buffer2.</param>
            /// <returns></returns>
            public void AddSingle(float buffer2)
            {
                var buffer1 = BitConverter.GetBytes(buffer2);
                Array.Resize(ref Data, Data.Length + buffer1.Length);
                Buffer.BlockCopy(buffer1, 0, Data, Data.Length - buffer1.Length, buffer1.Length);
            }

            /// <summary>
            /// Adds the byte array.
            /// </summary>
            /// <param name="buffer">The buffer.</param>
            /// <returns></returns>
            public void AddByteArray(byte[] buffer)
            {
                int tmp = Data.Length;
                Array.Resize(ref Data, Data.Length + buffer.Length);
                Array.Copy(buffer, 0, Data, tmp, buffer.Length);
            }

            /// <summary>
            /// Adds the pack GUID.
            /// </summary>
            /// <param name="buffer">The buffer.</param>
            /// <returns></returns>
            public void AddPackGUID(ulong buffer)
            {
                var guid = BitConverter.GetBytes(buffer);
                var flags = new BitArray(8);
                int offsetStart = Data.Length;
                int offsetNewSize = offsetStart;
                for (byte i = 0; i <= 7; i++)
                {
                    flags[i] = guid[i] != 0;
                    if (flags[i])
                        offsetNewSize += 1;
                }

                Array.Resize(ref Data, offsetNewSize + 1);
                flags.CopyTo(Data, offsetStart);
                offsetStart += 1;
                for (byte i = 0; i <= 7; i++)
                {
                    if (flags[i])
                    {
                        Data[offsetStart] = guid[i];
                        offsetStart += 1;
                    }
                }
            }

            /// <summary>
            /// Adds the U int16.
            /// </summary>
            /// <param name="buffer">The buffer.</param>
            /// <returns></returns>
            public void AddUInt16(ushort buffer)
            {
                Array.Resize(ref Data, Data.Length + 1 + 1);
                Data[Data.Length - 2] = (byte)(buffer & 255);
                Data[Data.Length - 1] = (byte)(buffer >> 8 & 255);
            }

            /// <summary>
            /// Adds the U int32.
            /// </summary>
            /// <param name="buffer">The buffer.</param>
            /// <returns></returns>
            public void AddUInt32(uint buffer)
            {
                Array.Resize(ref Data, Data.Length + 3 + 1);
                Data[Data.Length - 4] = (byte)(buffer & 255L);
                Data[Data.Length - 3] = (byte)(buffer >> 8 & 255L);
                Data[Data.Length - 2] = (byte)(buffer >> 16 & 255L);
                Data[Data.Length - 1] = (byte)(buffer >> 24 & 255L);
            }

            /// <summary>
            /// Adds the U int64.
            /// </summary>
            /// <param name="buffer">The buffer.</param>
            /// <param name="position">The position.</param>
            /// <returns></returns>
            public void AddUInt64(ulong buffer, int position = 0)
            {
                var dBuffer = BitConverter.GetBytes(buffer);
                long valueConverted = BitConverter.ToInt64(dBuffer, 0);
                AddInt64(valueConverted, position);
            }

            /// <summary>
            /// Updates the length.
            /// </summary>
            /// <returns></returns>
            public void UpdateLength()
            {
                if (Data[0] != 0 | Data[1] != 0)
                    return;
                Data[0] = (byte)((Data.Length - 2) / 256);
                Data[1] = (byte)((Data.Length - 2) % 256);
            }

            /// <summary>
            /// Gets the int8.
            /// </summary>
            /// <returns></returns>
            public byte GetInt8()
            {
                Offset += 1;
                return Data[Offset - 1];
            }

            /// <summary>
            /// Gets the int16.
            /// </summary>
            /// <returns></returns>
            public short GetInt16()
            {
                short num1 = BitConverter.ToInt16(Data, Offset);
                Offset += 2;
                return num1;
            }

            /// <summary>
            /// Gets the int32.
            /// </summary>
            /// <returns></returns>
            public int GetInt32()
            {
                int num1 = BitConverter.ToInt32(Data, Offset);
                Offset += 4;
                return num1;
            }

            /// <summary>
            /// Gets the int64.
            /// </summary>
            /// <returns></returns>
            public long GetInt64()
            {
                long num1 = BitConverter.ToInt64(Data, Offset);
                Offset += 8;
                return num1;
            }

            /// <summary>
            /// Gets the float.
            /// </summary>
            /// <returns></returns>
            public float GetFloat()
            {
                float single1 = BitConverter.ToSingle(Data, Offset);
                Offset += 4;
                return single1;
            }

            // Public Function GetFloat(ByVal Offset As Integer) As Single
            // Dim single1 As Single = BitConverter.ToSingle(Data, Offset)
            // Offset = (Offset + 4)
            // Return single1
            // End Function

            /// <summary>
            /// Gets the string.
            /// </summary>
            /// <returns></returns>
            public string GetString()
            {
                int start = Offset;
                int i = 0;
                while (Data[start + i] != 0)
                {
                    i += 1;
                    Offset += 1;
                }

                Offset += 1;
                return WorldServiceLocator._Functions.EscapeString(System.Text.Encoding.UTF8.GetString(Data, start, i));
            }

            /// <summary>
            /// Gets the string2.
            /// </summary>
            /// <returns></returns>
            public string GetString2()
            {
                int thisLength = Data[Offset];
                int start = Offset + 1;
                Offset += thisLength + 1;
                return WorldServiceLocator._Functions.EscapeString(System.Text.Encoding.UTF8.GetString(Data, start, thisLength));
            }

            /// <summary>
            /// Gets the U int16.
            /// </summary>
            /// <returns></returns>
            public ushort GetUInt16()
            {
                ushort num1 = BitConverter.ToUInt16(Data, Offset);
                Offset += 2;
                return num1;
            }

            /// <summary>
            /// Gets the U int32.
            /// </summary>
            /// <returns></returns>
            public uint GetUInt32()
            {
                uint num1 = BitConverter.ToUInt32(Data, Offset);
                Offset += 4;
                return num1;
            }

            /// <summary>
            /// Gets the U int64.
            /// </summary>
            /// <returns></returns>
            public ulong GetUInt64()
            {
                ulong num1 = BitConverter.ToUInt64(Data, Offset);
                Offset += 8;
                return num1;
            }

            /// <summary>
            /// Gets the pack GUID.
            /// </summary>
            /// <returns></returns>
            public ulong GetPackGuid()
            {
                byte flags = Data[Offset];
                var guid = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                Offset += 1;
                if ((flags & 1) == 1)
                {
                    guid[0] = Data[Offset];
                    Offset += 1;
                }

                if ((flags & 2) == 2)
                {
                    guid[1] = Data[Offset];
                    Offset += 1;
                }

                if ((flags & 4) == 4)
                {
                    guid[2] = Data[Offset];
                    Offset += 1;
                }

                if ((flags & 8) == 8)
                {
                    guid[3] = Data[Offset];
                    Offset += 1;
                }

                if ((flags & 16) == 16)
                {
                    guid[4] = Data[Offset];
                    Offset += 1;
                }

                if ((flags & 32) == 32)
                {
                    guid[5] = Data[Offset];
                    Offset += 1;
                }

                if ((flags & 64) == 64)
                {
                    guid[6] = Data[Offset];
                    Offset += 1;
                }

                if ((flags & 128) == 128)
                {
                    guid[7] = Data[Offset];
                    Offset += 1;
                }

                return BitConverter.ToUInt64(guid, 0);
            }

            // Public Function GetPackGUID(ByVal Offset As Integer) As ULong
            // Dim flags As Byte = Data(Offset)
            // Dim GUID() As Byte = {0, 0, 0, 0, 0, 0, 0, 0}
            // Offset += 1

            // If (flags And 1) = 1 Then
            // GUID(0) = Data(Offset)
            // Offset += 1
            // End If
            // If (flags And 2) = 2 Then
            // GUID(1) = Data(Offset)
            // Offset += 1
            // End If
            // If (flags And 4) = 4 Then
            // GUID(2) = Data(Offset)
            // Offset += 1
            // End If
            // If (flags And 8) = 8 Then
            // GUID(3) = Data(Offset)
            // Offset += 1
            // End If
            // If (flags And 16) = 16 Then
            // GUID(4) = Data(Offset)
            // Offset += 1
            // End If
            // If (flags And 32) = 32 Then
            // GUID(5) = Data(Offset)
            // Offset += 1
            // End If
            // If (flags And 64) = 64 Then
            // GUID(6) = Data(Offset)
            // Offset += 1
            // End If
            // If (flags And 128) = 128 Then
            // GUID(7) = Data(Offset)
            // Offset += 1
            // End If

            /// <summary>
            /// Gets the byte array.
            /// </summary>
            /// <returns></returns>
            public byte[] GetByteArray()
            {
                int lengthLoc = Data.Length - Offset;
                if (lengthLoc <= 0)
                    return new byte[] { };
                return GetByteArray(lengthLoc);
            }

            /// <summary>
            /// Gets the byte array.
            /// </summary>
            /// <param name="lengthLoc">The length loc.</param>
            /// <returns></returns>
            private byte[] GetByteArray(int lengthLoc)
            {
                if (Offset + lengthLoc > Data.Length)
                    lengthLoc = Data.Length - Offset;
                if (lengthLoc <= 0)
                    return new byte[] { };
                var tmpBytes = new byte[lengthLoc];
                Array.Copy(Data, Offset, tmpBytes, 0, tmpBytes.Length);
                Offset += tmpBytes.Length;
                return tmpBytes;
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

        public class UpdatePacketClass : PacketClass
        {

            /// <summary>
            /// Gets or sets the updates count.
            /// </summary>
            /// <value>The updates count.</value>
            public int UpdatesCount
            {
                get
                {
                    return BitConverter.ToInt32(Data, 4);
                }

                set
                {
                    Data[4] = (byte)(value & 255);
                    Data[5] = (byte)(value >> 8 & 255);
                    Data[6] = (byte)(value >> 16 & 255);
                    Data[7] = (byte)(value >> 24 & 255);
                }
            }

            public UpdatePacketClass() : base(OPCODES.SMSG_UPDATE_OBJECT)
            {
                AddInt32(0);
                AddInt8(0);
            }

            // Public Sub Compress()
            // End Sub
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia *//* TODO ERROR: Skipped RegionDirectiveTrivia */
        // Public Class PacketClassNew
        // Implements IDisposable

        // Public Offset As Integer = 4
        // Public Length As Integer = 0
        // Public ms As MemoryStream
        // Public bw As BinaryWriter
        // Public br As BinaryReader

        // Public ReadOnly Property OpCode() As OPCODES
        // Get
        // ms.Seek(2, SeekOrigin.Begin)
        // Return br.ReadUInt16.ToString
        // End Get
        // End Property
        // Public Property Data() As Byte()
        // Get
        // Return ms.ToArray
        // End Get
        // Set(ByVal Value As Byte())
        // ms.Close()
        // br.Close()
        // bw.Close()
        // ms = New MemoryStream(Value.Length)
        // bw = New BinaryWriter(ms, System.Text.Encoding.UTF8)
        // br = New BinaryReader(ms, System.Text.Encoding.UTF8)
        // Length = Value.Length - 2
        // bw.Write(Value)
        // End Set
        // End Property

        // Public Sub New(ByVal opcode As OPCODES)
        // ms = New MemoryStream(12)
        // bw = New BinaryWriter(ms, System.Text.Encoding.UTF8)
        // br = New BinaryReader(ms, System.Text.Encoding.UTF8)

        // Length = 2
        // bw.Write(CType(Length, Int16))
        // bw.Write(CType(opcode, Int16))
        // End Sub
        // Public Sub New(ByRef rawms() As Byte)
        // ms = New MemoryStream(12)
        // bw = New BinaryWriter(ms, System.Text.Encoding.UTF8)
        // br = New BinaryReader(ms, System.Text.Encoding.UTF8)

        // bw.Write(rawms)

        // ms.Seek(0, SeekOrigin.Begin)
        // Length = br.ReadInt16

        // ms.Seek(Offset, SeekOrigin.Begin)
        // End Sub

        // #Region "IDisposable Support"
        // Private _disposedValue As Boolean ' To detect redundant calls

        // ' IDisposable
        // Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        // If Not _disposedValue Then
        // ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
        // ' TODO: set large fields to null.
        // bw.Close()
        // br.Close()
        // ms.Close()
        // End If
        // _disposedValue = True
        // End Sub

        // ' This code added by Visual Basic to correctly implement the disposable pattern.
        // Public Sub Dispose() Implements IDisposable.Dispose
        // ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        // Dispose(True)
        // GC.SuppressFinalize(Me)
        // End Sub
        // #End Region

        // Public Sub AddBitArray(ByVal buffer As BitArray, ByVal Len As Integer)
        // Dim bufferarray(CType((buffer.Length + 8) / 8, Byte)) As Byte
        // buffer.CopyTo(bufferarray, 0)
        // ms.Seek(0, SeekOrigin.End)
        // bw.Write(bufferarray, 0, Len)

        // ms.Seek(0, SeekOrigin.Begin)
        // Length += Len
        // bw.Write(CType(Length, Int16))
        // End Sub
        // Public Sub AddInt8(ByVal buffer As Byte)
        // ms.Seek(0, SeekOrigin.End)
        // bw.Write(buffer)

        // ms.Seek(0, SeekOrigin.Begin)
        // Length += 1
        // bw.Write(CType(Length, Int16))
        // End Sub
        // Public Sub AddInt16(ByVal buffer As Short)
        // ms.Seek(0, SeekOrigin.End)
        // bw.Write(buffer)

        // ms.Seek(0, SeekOrigin.Begin)
        // Length += 2
        // bw.Write(CType(Length, Int16))
        // End Sub
        // Public Sub AddInt32(ByVal buffer As Integer)
        // ms.Seek(0, SeekOrigin.End)
        // bw.Write(buffer)

        // ms.Seek(0, SeekOrigin.Begin)
        // Length += 4
        // bw.Write(CType(Length, Int16))
        // End Sub
        // Public Sub AddInt64(ByVal buffer As Long)
        // ms.Seek(0, SeekOrigin.End)
        // bw.Write(buffer)

        // ms.Seek(0, SeekOrigin.Begin)
        // Length += 8
        // bw.Write(CType(Length, Int16))
        // End Sub
        // Public Sub AddString(ByVal buffer As String)
        // Dim Bytes As Byte() = System.Text.Encoding.UTF8.GetBytes(buffer.ToCharArray)

        // ms.Seek(0, SeekOrigin.End)
        // bw.Write(Bytes)
        // bw.Write(CType(0, Byte))

        // ms.Seek(0, SeekOrigin.Begin)
        // Length += Bytes.Length + 1
        // bw.Write(CType(Length, Int16))
        // End Sub
        // Public Sub AddDouble(ByVal buffer As Double)
        // ms.Seek(0, SeekOrigin.End)
        // bw.Write(buffer)

        // ms.Seek(0, SeekOrigin.Begin)
        // Length += 8
        // bw.Write(CType(Length, Int16))
        // End Sub
        // Public Sub AddSingle(ByVal buffer As Single)
        // ms.Seek(0, SeekOrigin.End)
        // bw.Write(buffer)

        // ms.Seek(0, SeekOrigin.Begin)
        // Length += 4
        // bw.Write(CType(Length, Int16))
        // End Sub
        // Public Sub AddByteArray(ByVal buffer() As Byte)
        // ms.Seek(0, SeekOrigin.End)
        // bw.Write(buffer)

        // ms.Seek(0, SeekOrigin.Begin)
        // Length += buffer.Length
        // bw.Write(CType(Length, Int16))
        // End Sub
        // Public Sub AddPackGUID(ByVal buffer As ULong)
        // Dim GUID() As Byte = BitConverter.GetBytes(buffer)
        // Dim flags As New BitArray(8)
        // Dim flagsByte(1) As Byte
        // Length += 1

        // flags(0) = (GUID(0) <> 0)
        // flags(1) = (GUID(1) <> 0)
        // flags(2) = (GUID(2) <> 0)
        // flags(3) = (GUID(3) <> 0)
        // flags(4) = (GUID(4) <> 0)
        // flags(5) = (GUID(5) <> 0)
        // flags(6) = (GUID(6) <> 0)
        // flags(7) = (GUID(7) <> 0)

        // If flags(0) Then Length += 1
        // If flags(1) Then Length += 1
        // If flags(2) Then Length += 1
        // If flags(3) Then Length += 1
        // If flags(4) Then Length += 1
        // If flags(5) Then Length += 1
        // If flags(6) Then Length += 1
        // If flags(7) Then Length += 1

        // ms.Seek(0, SeekOrigin.End)
        // flags.CopyTo(flagsByte, 0)
        // bw.Write(flagsByte(0))
        // If flags(0) Then bw.Write(GUID(0))
        // If flags(1) Then bw.Write(GUID(1))
        // If flags(2) Then bw.Write(GUID(2))
        // If flags(3) Then bw.Write(GUID(3))
        // If flags(4) Then bw.Write(GUID(4))
        // If flags(5) Then bw.Write(GUID(5))
        // If flags(6) Then bw.Write(GUID(6))
        // If flags(7) Then bw.Write(GUID(7))

        // ms.Seek(0, SeekOrigin.Begin)
        // bw.Write(CType(Length, Int16))
        // End Sub

        // Public Function GetInt8() As Byte
        // Return br.ReadByte()
        // End Function
        // 'Public Function GetInt8(ByVal Offset As Integer) As Byte
        // '    ms.Seek(Offset, SeekOrigin.Begin)
        // '    Return br.ReadByte()
        // 'End Function
        // Public Function GetInt16() As Short
        // Return br.ReadInt16
        // End Function
        // 'Public Function GetInt16(ByVal Offset As Integer) As Short
        // '    ms.Seek(Offset, SeekOrigin.Begin)
        // '    Return br.ReadInt16
        // 'End Function
        // Public Function GetInt32() As Integer
        // Return br.ReadInt32
        // End Function
        // 'Public Function GetInt32(ByVal Offset As Integer) As Integer
        // '    ms.Seek(Offset, SeekOrigin.Begin)
        // '    Return br.ReadInt32
        // 'End Function
        // Public Function GetInt64() As Long
        // Return br.ReadInt64
        // End Function
        // 'Public Function GetInt64(ByVal Offset As Integer) As Long
        // '    ms.Seek(Offset, SeekOrigin.Begin)
        // '    Return br.ReadInt64
        // 'End Function
        // Public Function GetFloat() As Single
        // Return br.ReadSingle
        // End Function
        // 'Public Function GetFloat(ByVal Offset As Integer) As Single
        // '    ms.Seek(Offset, SeekOrigin.Begin)
        // '    Return br.ReadSingle
        // 'End Function
        // Public Function GetDouble() As Double
        // Return br.ReadDouble
        // End Function
        // 'Public Function GetDouble(ByVal Offset As Integer) As Double
        // '    ms.Seek(Offset, SeekOrigin.Begin)
        // '    Return br.ReadDouble
        // 'End Function
        // Public Function GetString() As String
        // Dim tmpString As New System.Text.StringBuilder
        // Dim tmpChar As Char = br.ReadChar()
        // Dim tmpEndChar As Char = System.Text.Encoding.UTF8.GetString(New Byte() {0})

        // While tmpChar <> tmpEndChar
        // tmpString.Append(tmpChar)
        // tmpChar = br.ReadChar()
        // End While

        // Return tmpString.ToString
        // End Function
        // 'Public Function GetString(ByVal Offset As Integer) As String
        // '    ms.Seek(Offset, SeekOrigin.Begin)
        // '    Dim tmpString As New System.Text.StringBuilder
        // '    Dim tmpChar As Char = br.ReadChar()
        // '    Dim tmpEndChar As Char = System.Text.Encoding.UTF8.GetString(New Byte() {0})

        // '    While tmpChar <> tmpEndChar
        // '        tmpString.Append(tmpChar)
        // '        tmpChar = br.ReadChar()
        // '    End While

        // '    Return tmpString.ToString
        // 'End Function
        // 'Public Function GetPackGUID() As ULong
        // '    Dim flags As Byte = br.ReadByte
        // '    Dim GUID() As Byte = {0, 0, 0, 0, 0, 0, 0, 0}
        // '    Offset += 1

        // '    If (flags And 1) = 1 Then GUID(0) = br.ReadByte
        // '    If (flags And 2) = 2 Then GUID(1) = br.ReadByte
        // '    If (flags And 4) = 4 Then GUID(2) = br.ReadByte
        // '    If (flags And 8) = 8 Then GUID(3) = br.ReadByte
        // '    If (flags And 16) = 16 Then GUID(4) = br.ReadByte
        // '    If (flags And 32) = 32 Then GUID(5) = br.ReadByte
        // '    If (flags And 64) = 64 Then GUID(6) = br.ReadByte
        // '    If (flags And 128) = 128 Then GUID(7) = br.ReadByte

        // '    Return CType(BitConverter.ToUInt64(GUID, 0), ULong)
        // 'End Function
        // '    Public Function GetPackGUID(ByVal Offset As Integer) As ULong
        // '        ms.Seek(Offset, SeekOrigin.Begin)
        // '        Dim flags As Byte = br.ReadByte
        // '        Dim GUID() As Byte = {0, 0, 0, 0, 0, 0, 0, 0}
        // '        Offset += 1

        // '        If (flags And 1) = 1 Then GUID(0) = br.ReadByte
        // '        If (flags And 2) = 2 Then GUID(1) = br.ReadByte
        // '        If (flags And 4) = 4 Then GUID(2) = br.ReadByte
        // '        If (flags And 8) = 8 Then GUID(3) = br.ReadByte
        // '        If (flags And 16) = 16 Then GUID(4) = br.ReadByte
        // '        If (flags And 32) = 32 Then GUID(5) = br.ReadByte
        // '        If (flags And 64) = 64 Then GUID(6) = br.ReadByte
        // '        If (flags And 128) = 128 Then GUID(7) = br.ReadByte

        // '        Return CType(BitConverter.ToUInt64(GUID, 0), ULong)
        // '    End Function

        // 'End Class
        // 'Public Class UpdatePacketClassNew
        // '    Inherits PacketClassNew

        // '    Public Property UpdatesCount() As Integer
        // '        Get
        // '            ms.Seek(4, SeekOrigin.Begin)
        // '            Return br.ReadInt32
        // '        End Get
        // '        Set(ByVal Value As Integer)
        // '            ms.Seek(4, SeekOrigin.Begin)
        // '            bw.Write(Value)
        // '        End Set
        // '    End Property
        // Public Sub New()
        // '            MyBase.New(OPCODES.SMSG_UPDATE_OBJECT)
        // MyBase.New()

        // AddInt32(0)
        // AddInt8(0)
        // End Sub

        // 'Public Sub Compress()
        // 'End Sub
        // End Class
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
    }
}