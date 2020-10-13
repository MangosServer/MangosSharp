//
//  Copyright (C) 2013-2020 getMaNGOS <https:\\getmangos.eu>
//  
//  This program is free software. You can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation. either version 2 of the License, or
//  (at your option) any later version.
//  
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY. Without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//  
//  You should have received a copy of the GNU General Public License
//  along with this program. If not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//

using System;
using System.Collections;
using System.Text;
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
					if (UpdateData.ContainsKey(pos))
					{
						UpdateData[pos] = Conversions.ToInteger(UpdateData[pos]) | (value << 8 * index);
					}
					else
					{
						UpdateData[pos] = value << 8 * index;
					}
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
					if (updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT || updateType == ObjectUpdateType.UPDATETYPE_MOVEMENT)
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
					if (updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT || updateType == ObjectUpdateType.UPDATETYPE_VALUES)
					{
						int updateCount = 0;
						int num = UpdateMask.Count - 1;
						for (int i = 0; i <= num; i++)
						{
							if (UpdateMask.Get(i))
							{
								updateCount = i;
							}
						}
						packet.AddInt8((byte)unchecked(checked(updateCount + 32) / 32));
						packet.AddBitArray(UpdateMask, unchecked((int)checked((byte)unchecked(checked(updateCount + 32) / 32))) * 4);
						int num2 = UpdateMask.Count - 1;
						for (int j = 0; j <= num2; j++)
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
					if (packet is UpdatePacketClass)
					{
						((UpdatePacketClass)packet).UpdatesCount++;
					}
				}
			}

			public void AddToPacket(ref PacketClass packet, ObjectUpdateType updateType, ref WS_PlayerData.CharacterObject updateObject)
			{
				packet.AddInt8(checked((byte)updateType));
				packet.AddPackGUID(updateObject.GUID);
				if (updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT || updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF)
				{
					packet.AddInt8(4);
				}
				if (updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT || updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF || updateType == ObjectUpdateType.UPDATETYPE_MOVEMENT)
				{
					int flags2 = updateObject.charMovementFlags & 0xFF;
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
					if (updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT || updateType == ObjectUpdateType.UPDATETYPE_VALUES || updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF)
					{
						int updateCount = 0;
						int num = UpdateMask.Count - 1;
						for (int i = 0; i <= num; i++)
						{
							if (UpdateMask.Get(i))
							{
								updateCount = i;
							}
						}
						packet.AddInt8((byte)unchecked(checked(updateCount + 32) / 32));
						packet.AddBitArray(UpdateMask, unchecked((int)checked((byte)unchecked(checked(updateCount + 32) / 32))) * 4);
						int num2 = UpdateMask.Count - 1;
						for (int j = 0; j <= num2; j++)
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
					if (packet is UpdatePacketClass)
					{
						((UpdatePacketClass)packet).UpdatesCount++;
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
					if (unchecked(updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT || updateType == ObjectUpdateType.UPDATETYPE_VALUES))
					{
						int updateCount = 0;
						int num = UpdateMask.Count - 1;
						for (int j = 0; j <= num; j++)
						{
							if (UpdateMask.Get(j))
							{
								updateCount = j;
							}
						}
						packet.AddInt8((byte)unchecked(checked(updateCount + 32) / 32));
						packet.AddBitArray(UpdateMask, unchecked((int)checked((byte)unchecked(checked(updateCount + 32) / 32))) * 4);
						int num2 = UpdateMask.Count - 1;
						for (int i = 0; i <= num2; i++)
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
					if (packet is UpdatePacketClass)
					{
						((UpdatePacketClass)packet).UpdatesCount++;
					}
				}
			}

			public void AddToPacket(ref PacketClass packet, ObjectUpdateType updateType, ref WS_GameObjects.GameObjectObject updateObject)
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
					if (updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT || updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF)
					{
						packet.AddInt8(5);
						if (updateObject.Type == GameObjectType.GAMEOBJECT_TYPE_TRANSPORT || updateObject.Type == GameObjectType.GAMEOBJECT_TYPE_MO_TRANSPORT)
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
						if (updateObject.Type == GameObjectType.GAMEOBJECT_TYPE_TRANSPORT || updateObject.Type == GameObjectType.GAMEOBJECT_TYPE_MO_TRANSPORT)
						{
							packet.AddInt32(WorldServiceLocator._WS_Network.MsTime());
						}
					}
					if (updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT || updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF || updateType == ObjectUpdateType.UPDATETYPE_VALUES)
					{
						int updateCount = 0;
						int num = UpdateMask.Count - 1;
						for (int i = 0; i <= num; i++)
						{
							if (UpdateMask.Get(i))
							{
								updateCount = i;
							}
						}
						packet.AddInt8((byte)unchecked(checked(updateCount + 32) / 32));
						packet.AddBitArray(UpdateMask, unchecked((int)checked((byte)unchecked(checked(updateCount + 32) / 32))) * 4);
						int num2 = UpdateMask.Count - 1;
						for (int j = 0; j <= num2; j++)
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
					if (packet is UpdatePacketClass)
					{
						((UpdatePacketClass)packet).UpdatesCount++;
					}
				}
			}

			public void AddToPacket(ref PacketClass packet, ObjectUpdateType updateType, ref WS_DynamicObjects.DynamicObjectObject updateObject)
			{
				checked
				{
					packet.AddInt8((byte)updateType);
					packet.AddPackGUID(updateObject.GUID);
					if (updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT || updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF)
					{
						packet.AddInt8(6);
						packet.AddInt8(88);
						packet.AddSingle(updateObject.positionX);
						packet.AddSingle(updateObject.positionY);
						packet.AddSingle(updateObject.positionZ);
						packet.AddSingle(updateObject.orientation);
						packet.AddUInt64(updateObject.GUID);
					}
					if (updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT || updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF || updateType == ObjectUpdateType.UPDATETYPE_VALUES)
					{
						int updateCount = 0;
						int num = UpdateMask.Count - 1;
						for (int i = 0; i <= num; i++)
						{
							if (UpdateMask.Get(i))
							{
								updateCount = i;
							}
						}
						packet.AddInt8((byte)unchecked(checked(updateCount + 32) / 32));
						packet.AddBitArray(UpdateMask, unchecked((int)checked((byte)unchecked(checked(updateCount + 32) / 32))) * 4);
						int num2 = UpdateMask.Count - 1;
						for (int j = 0; j <= num2; j++)
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
					if (packet is UpdatePacketClass)
					{
						((UpdatePacketClass)packet).UpdatesCount++;
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
					if (unchecked(updateType == ObjectUpdateType.UPDATETYPE_CREATE_OBJECT || updateType == ObjectUpdateType.UPDATETYPE_VALUES))
					{
						int updateCount = 0;
						int num = UpdateMask.Count - 1;
						for (int j = 0; j <= num; j++)
						{
							if (UpdateMask.Get(j))
							{
								updateCount = j;
							}
						}
						packet.AddInt8((byte)unchecked(checked(updateCount + 32) / 32));
						packet.AddBitArray(UpdateMask, unchecked((int)checked((byte)unchecked(checked(updateCount + 32) / 32))) * 4);
						int num2 = UpdateMask.Count - 1;
						for (int i = 0; i <= num2; i++)
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
					if (packet is UpdatePacketClass)
					{
						((UpdatePacketClass)packet).UpdatesCount++;
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
				this.Dispose();
			}
		}

		public class PacketClass : IDisposable
		{
			public byte[] Data;

			public int Offset;

			private bool _disposedValue;

			public int Length
			{
				get
				{
					checked
					{
						return unchecked((int)Data[1]) + unchecked((int)Data[0]) * 256;
					}
				}
			}

			public OPCODES OpCode
			{
				get
				{
					if (Information.UBound(Data) > 2)
					{
						return (OPCODES)checked(unchecked((int)Data[2]) + unchecked((int)Data[3]) * 256);
					}
					return OPCODES.MSG_NULL_ACTION;
				}
			}

			public PacketClass(OPCODES opcode)
			{
				Offset = 4;
				Data = new byte[4];
				Data[0] = 0;
				Data[1] = 0;
				checked
				{
					Data[2] = (byte)unchecked(checked((short)opcode) % 256);
					Data[3] = (byte)unchecked(checked((short)opcode) / 256);
				}
			}

			public PacketClass(ref byte[] rawdata)
			{
				Offset = 4;
				Data = rawdata;
				rawdata.CopyTo(Data, 0);
			}

			public void CompressUpdatePacket()
			{
				if (OpCode == OPCODES.SMSG_UPDATE_OBJECT && Data.Length >= 200)
				{
					int uncompressedSize = Data.Length;
					byte[] compressedBuffer = WorldServiceLocator._GlobalZip.Compress(Data, 4, checked(Data.Length - 4));
					if (compressedBuffer.Length != 0)
					{
						Data = new byte[4];
						Data[0] = 0;
						Data[1] = 0;
						Data[2] = 246;
						Data[3] = 1;
						AddInt32(uncompressedSize);
						AddByteArray(compressedBuffer);
						UpdateLength();
					}
				}
			}

			public void AddBitArray(BitArray buffer, int arraryLen)
			{
				ref byte[] data = ref Data;
				checked
				{
					data = (byte[])Utils.CopyArray(data, new byte[Data.Length - 1 + arraryLen + 1]);
					byte[] bufferarray = new byte[unchecked((int)checked((byte)Math.Round((double)(buffer.Length + 8) / 8.0))) + 1];
					buffer.CopyTo(bufferarray, 0);
					Array.Copy(bufferarray, 0, Data, Data.Length - arraryLen, arraryLen);
				}
			}

			public void AddInt8(byte buffer, int position = 0)
			{
				if (position <= 0 || position >= Data.Length)
				{
					position = Data.Length;
					ref byte[] data = ref Data;
					data = (byte[])Utils.CopyArray(data, new byte[checked(Data.Length + 1)]);
				}
				Data[position] = buffer;
			}

			public void AddInt16(short buffer, int position = 0)
			{
				checked
				{
					if (position <= 0 || position >= Data.Length)
					{
						position = Data.Length;
						ref byte[] data = ref Data;
						data = (byte[])Utils.CopyArray(data, new byte[Data.Length + 1 + 1]);
					}
					Data[position] = (byte)(buffer & 0xFF);
					Data[position + 1] = (byte)(unchecked((short)(buffer >> 8)) & 0xFF);
				}
			}

			public void AddInt32(int buffer, int position = 0)
			{
				checked
				{
					if (position <= 0 || position > Data.Length - 3)
					{
						position = Data.Length;
						ref byte[] data = ref Data;
						data = (byte[])Utils.CopyArray(data, new byte[Data.Length + 3 + 1]);
					}
					Data[position] = (byte)(buffer & 0xFF);
					Data[position + 1] = (byte)((buffer >> 8) & 0xFF);
					Data[position + 2] = (byte)((buffer >> 16) & 0xFF);
					Data[position + 3] = (byte)((buffer >> 24) & 0xFF);
				}
			}

			public void AddInt64(long buffer, int position = 0)
			{
				checked
				{
					if (position <= 0 || position > Data.Length - 7)
					{
						position = Data.Length;
						ref byte[] data = ref Data;
						data = (byte[])Utils.CopyArray(data, new byte[Data.Length + 7 + 1]);
					}
					Data[position] = (byte)(buffer & 0xFF);
					Data[position + 1] = (byte)((buffer >> 8) & 0xFF);
					Data[position + 2] = (byte)((buffer >> 16) & 0xFF);
					Data[position + 3] = (byte)((buffer >> 24) & 0xFF);
					Data[position + 4] = (byte)((buffer >> 32) & 0xFF);
					Data[position + 5] = (byte)((buffer >> 40) & 0xFF);
					Data[position + 6] = (byte)((buffer >> 48) & 0xFF);
					Data[position + 7] = (byte)((buffer >> 56) & 0xFF);
				}
			}

			public void AddString(string buffer)
			{
				if (Information.IsDBNull(buffer) | (Operators.CompareString(buffer, "", TextCompare: false) == 0))
				{
					AddInt8(0);
					return;
				}
				byte[] bytes = Encoding.UTF8.GetBytes(buffer.ToCharArray());
				ref byte[] data = ref Data;
				checked
				{
					data = (byte[])Utils.CopyArray(data, new byte[Data.Length + bytes.Length + 1]);
					int num = bytes.Length - 1;
					for (int i = 0; i <= num; i++)
					{
						Data[Data.Length - 1 - bytes.Length + i] = bytes[i];
					}
					Data[Data.Length - 1] = 0;
				}
			}

			public void AddString2(string buffer)
			{
				if (Information.IsDBNull(buffer) | (Operators.CompareString(buffer, "", TextCompare: false) == 0))
				{
					AddInt8(0);
					return;
				}
				byte[] bytes = Encoding.UTF8.GetBytes(buffer.ToCharArray());
				ref byte[] data = ref Data;
				checked
				{
					data = (byte[])Utils.CopyArray(data, new byte[Data.Length + bytes.Length + 1]);
					Data[Data.Length - 1 - bytes.Length] = (byte)bytes.Length;
					int num = bytes.Length - 1;
					for (int i = 0; i <= num; i++)
					{
						Data[Data.Length - bytes.Length + i] = bytes[i];
					}
				}
			}

			public void AddSingle(float buffer2)
			{
				byte[] buffer3 = BitConverter.GetBytes(buffer2);
				ref byte[] data = ref Data;
				checked
				{
					data = (byte[])Utils.CopyArray(data, new byte[Data.Length + buffer3.Length - 1 + 1]);
					Buffer.BlockCopy(buffer3, 0, Data, Data.Length - buffer3.Length, buffer3.Length);
				}
			}

			public void AddByteArray(byte[] buffer)
			{
				int tmp = Data.Length;
				ref byte[] data = ref Data;
				data = (byte[])Utils.CopyArray(data, new byte[checked(Data.Length + buffer.Length - 1 + 1)]);
				Array.Copy(buffer, 0, Data, tmp, buffer.Length);
			}

			public void AddPackGUID(ulong buffer)
			{
				byte[] guid = BitConverter.GetBytes(buffer);
				BitArray flags = new BitArray(8);
				int offsetStart = Data.Length;
				int offsetNewSize = offsetStart;
				byte j = 0;
				checked
				{
					do
					{
						flags[j] = guid[j] != 0;
						if (flags[j])
						{
							offsetNewSize++;
						}
						j = (byte)unchecked((uint)(j + 1));
					}
					while (unchecked((uint)j) <= 7u);
					ref byte[] data = ref Data;
					data = (byte[])Utils.CopyArray(data, new byte[offsetNewSize + 1]);
					flags.CopyTo(Data, offsetStart);
					offsetStart++;
					byte i = 0;
					do
					{
						if (flags[i])
						{
							Data[offsetStart] = guid[i];
							offsetStart++;
						}
						i = (byte)unchecked((uint)(i + 1));
					}
					while (unchecked((uint)i) <= 7u);
				}
			}

			public void AddUInt16(ushort buffer)
			{
				ref byte[] data = ref Data;
				checked
				{
					data = (byte[])Utils.CopyArray(data, new byte[Data.Length + 1 + 1]);
					Data[Data.Length - 2] = (byte)(buffer & 0xFF);
					Data[Data.Length - 1] = (byte)(unchecked((ushort)((uint)buffer >> 8)) & 0xFF);
				}
			}

			public void AddUInt32(uint buffer)
			{
				ref byte[] data = ref Data;
				checked
				{
					data = (byte[])Utils.CopyArray(data, new byte[Data.Length + 3 + 1]);
					Data[Data.Length - 4] = (byte)(unchecked((long)buffer) & 0xFFL);
					Data[Data.Length - 3] = (byte)(unchecked((long)(buffer >> 8)) & 0xFFL);
					Data[Data.Length - 2] = (byte)(unchecked((long)(buffer >> 16)) & 0xFFL);
					Data[Data.Length - 1] = (byte)(unchecked((long)(buffer >> 24)) & 0xFFL);
				}
			}

			public void AddUInt64(ulong buffer, int position = 0)
			{
				byte[] dBuffer = BitConverter.GetBytes(buffer);
				long valueConverted = BitConverter.ToInt64(dBuffer, 0);
				AddInt64(valueConverted, position);
			}

			public void UpdateLength()
			{
				checked
				{
					if (!((Data[0] != 0) | (Data[1] != 0)))
					{
						Data[0] = (byte)unchecked(checked(Data.Length - 2) / 256);
						Data[1] = (byte)unchecked(checked(Data.Length - 2) % 256);
					}
				}
			}

			public byte GetInt8()
			{
				checked
				{
					Offset++;
					return Data[Offset - 1];
				}
			}

			public short GetInt16()
			{
				short num1 = BitConverter.ToInt16(Data, Offset);
				checked
				{
					Offset += 2;
					return num1;
				}
			}

			public int GetInt32()
			{
				int num1 = BitConverter.ToInt32(Data, Offset);
				checked
				{
					Offset += 4;
					return num1;
				}
			}

			public long GetInt64()
			{
				long num1 = BitConverter.ToInt64(Data, Offset);
				checked
				{
					Offset += 8;
					return num1;
				}
			}

			public float GetFloat()
			{
				float single1 = BitConverter.ToSingle(Data, Offset);
				checked
				{
					Offset += 4;
					return single1;
				}
			}

			public string GetString()
			{
				int start = Offset;
				int i = 0;
				checked
				{
					while (Data[start + i] != 0)
					{
						i++;
						Offset++;
					}
					Offset++;
					return WorldServiceLocator._Functions.EscapeString(Encoding.UTF8.GetString(Data, start, i));
				}
			}

			public string GetString2()
			{
				int thisLength = Data[Offset];
				checked
				{
					int start = Offset + 1;
					Offset += thisLength + 1;
					return WorldServiceLocator._Functions.EscapeString(Encoding.UTF8.GetString(Data, start, thisLength));
				}
			}

			public ushort GetUInt16()
			{
				ushort num1 = BitConverter.ToUInt16(Data, Offset);
				checked
				{
					Offset += 2;
					return num1;
				}
			}

			public uint GetUInt32()
			{
				uint num1 = BitConverter.ToUInt32(Data, Offset);
				checked
				{
					Offset += 4;
					return num1;
				}
			}

			public ulong GetUInt64()
			{
				ulong num1 = BitConverter.ToUInt64(Data, Offset);
				checked
				{
					Offset += 8;
					return num1;
				}
			}

			public ulong GetPackGuid()
			{
				byte flags = Data[Offset];
				byte[] guid = new byte[8];
				checked
				{
					Offset++;
					if ((flags & 1) == 1)
					{
						guid[0] = Data[Offset];
						Offset++;
					}
					if ((flags & 2) == 2)
					{
						guid[1] = Data[Offset];
						Offset++;
					}
					if ((flags & 4) == 4)
					{
						guid[2] = Data[Offset];
						Offset++;
					}
					if ((flags & 8) == 8)
					{
						guid[3] = Data[Offset];
						Offset++;
					}
					if ((flags & 0x10) == 16)
					{
						guid[4] = Data[Offset];
						Offset++;
					}
					if ((flags & 0x20) == 32)
					{
						guid[5] = Data[Offset];
						Offset++;
					}
					if ((flags & 0x40) == 64)
					{
						guid[6] = Data[Offset];
						Offset++;
					}
					if ((flags & 0x80) == 128)
					{
						guid[7] = Data[Offset];
						Offset++;
					}
					return BitConverter.ToUInt64(guid, 0);
				}
			}

			public byte[] GetByteArray()
			{
				int lengthLoc = checked(Data.Length - Offset);
				if (lengthLoc <= 0)
				{
					return new byte[0];
				}
				return GetByteArray(lengthLoc);
			}

			private byte[] GetByteArray(int lengthLoc)
			{
				checked
				{
					if (Offset + lengthLoc > Data.Length)
					{
						lengthLoc = Data.Length - Offset;
					}
					if (lengthLoc <= 0)
					{
						return new byte[0];
					}
					byte[] tmpBytes = new byte[lengthLoc - 1 + 1];
					Array.Copy(Data, Offset, tmpBytes, 0, tmpBytes.Length);
					Offset += tmpBytes.Length;
					return tmpBytes;
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
				this.Dispose();
			}
		}

		public class UpdatePacketClass : PacketClass
		{
			public int UpdatesCount
			{
				get
				{
					return BitConverter.ToInt32(Data, 4);
				}
				set
				{
					checked
					{
						Data[4] = (byte)(value & 0xFF);
						Data[5] = (byte)((value >> 8) & 0xFF);
						Data[6] = (byte)((value >> 16) & 0xFF);
						Data[7] = (byte)((value >> 24) & 0xFF);
					}
				}
			}

			public UpdatePacketClass()
				: base(OPCODES.SMSG_UPDATE_OBJECT)
			{
				AddInt32(0);
				AddInt8(0);
			}
		}

		public void DumpPacket(byte[] data, WS_Network.ClientClass client = null, int start = 0)
		{
			string buffer = "";
			checked
			{
				try
				{
					buffer = ((client != null) ? (buffer + $"[{client.IP}:{client.Port}] DEBUG: Packet Dump - Length={data.Length - start}{Environment.NewLine}") : (buffer + $"DEBUG: Packet Dump{Environment.NewLine}"));
					if (unchecked(checked(data.Length - start) % 16) == 0)
					{
						int num = data.Length - 1;
						for (int i = start; i <= num; i += 16)
						{
							buffer = buffer + "|  " + BitConverter.ToString(data, i, 16).Replace("-", " ");
							buffer = buffer + " |  " + Encoding.ASCII.GetString(data, i, 16).Replace("\t", "?").Replace("\b", "?")
								.Replace("\r", "?")
								.Replace("\f", "?")
								.Replace("\n", "?") + " |" + Environment.NewLine;
						}
					}
					else
					{
						int num2 = data.Length - 1 - 16;
						int i;
						for (i = start; i <= num2; i += 16)
						{
							buffer = buffer + "|  " + BitConverter.ToString(data, i, 16).Replace("-", " ");
							buffer = buffer + " |  " + Encoding.ASCII.GetString(data, i, 16).Replace("\t", "?").Replace("\b", "?")
								.Replace("\r", "?")
								.Replace("\f", "?")
								.Replace("\n", "?") + " |" + Environment.NewLine;
						}
						unchecked
						{
							buffer = buffer + "|  " + BitConverter.ToString(data, i, checked(data.Length - start) % 16).Replace("-", " ");
						}
						buffer += new string(' ', (16 - unchecked(checked(data.Length - start) % 16)) * 3);
						unchecked
						{
							buffer = buffer + " |  " + Encoding.ASCII.GetString(data, i, checked(data.Length - start) % 16).Replace("\t", "?").Replace("\b", "?")
								.Replace("\r", "?")
								.Replace("\f", "?")
								.Replace("\n", "?");
						}
						buffer += new string(' ', 16 - unchecked(checked(data.Length - start) % 16));
						buffer = buffer + " |" + Environment.NewLine;
					}
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, buffer, null);
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception e = ex;
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error dumping packet: {0}{1}", Environment.NewLine, e.ToString());
					ProjectData.ClearProjectError();
				}
			}
		}
	}
}
