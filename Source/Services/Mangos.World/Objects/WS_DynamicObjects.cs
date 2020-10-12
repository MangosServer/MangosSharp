using System;
using System.Collections.Generic;
using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Maps;
using Mangos.World.Player;
using Mangos.World.Spells;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Objects
{
	public class WS_DynamicObjects
	{
		public class DynamicObjectObject : WS_Base.BaseObject, IDisposable
		{
			public int SpellID;

			public List<WS_Spells.SpellEffect> Effects;

			public int Duration;

			public float Radius;

			public WS_Base.BaseUnit Caster;

			public int CastTime;

			public int Bytes;

			private bool _disposedValue;

			protected virtual void Dispose(bool disposing)
			{
				if (!_disposedValue)
				{
					WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs.Remove(GUID);
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

			public DynamicObjectObject(ref WS_Base.BaseUnit Caster_, int SpellID_, float PosX, float PosY, float PosZ, int Duration_, float Radius_)
			{
				SpellID = 0;
				Effects = new List<WS_Spells.SpellEffect>();
				Duration = 0;
				Radius = 0f;
				CastTime = 0;
				Bytes = 1;
				GUID = WorldServiceLocator._WS_DynamicObjects.GetNewGUID();
				WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs.Add(GUID, this);
				Caster = Caster_;
				SpellID = SpellID_;
				positionX = PosX;
				positionY = PosY;
				positionZ = PosZ;
				orientation = 0f;
				MapID = Caster.MapID;
				instance = Caster.instance;
				Duration = Duration_;
				Radius = Radius_;
				CastTime = WorldServiceLocator._NativeMethods.timeGetTime("");
			}

			public void FillAllUpdateFlags(ref Packets.UpdateClass Update)
			{
				Update.SetUpdateFlag(0, GUID);
				Update.SetUpdateFlag(2, 65);
				Update.SetUpdateFlag(4, 0.5f * Radius);
				Update.SetUpdateFlag(6, Caster.GUID);
				Update.SetUpdateFlag(8, Bytes);
				Update.SetUpdateFlag(9, SpellID);
				Update.SetUpdateFlag(10, Radius);
				Update.SetUpdateFlag(11, positionX);
				Update.SetUpdateFlag(12, positionY);
				Update.SetUpdateFlag(13, positionZ);
				Update.SetUpdateFlag(14, orientation);
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
					WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY].DynamicObjectsHere.Add(GUID);
				}
				catch (Exception projectError)
				{
					ProjectData.SetProjectError(projectError);
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "AddToWorld failed MapId: {0} Tile XY: {1} {2} GUID: {3}", MapID, CellX, CellY, GUID);
					ProjectData.ClearProjectError();
					return;
				}
				Packets.PacketClass packet = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
				packet.AddInt32(1);
				packet.AddInt8(0);
				Packets.UpdateClass tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_DYNAMICOBJECT);
				FillAllUpdateFlags(ref tmpUpdate);
				Packets.UpdateClass updateClass = tmpUpdate;
				DynamicObjectObject updateObject = this;
				updateClass.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF, ref updateObject);
				tmpUpdate.Dispose();
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
								WS_Maps.TMapTile tMapTile = WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[(short)unchecked(CellX + i), (short)unchecked(CellY + j)];
								ulong[] list = tMapTile.PlayersHere.ToArray();
								ulong[] array = list;
								foreach (ulong plGUID in array)
								{
									int num;
									if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(plGUID))
									{
										WS_PlayerData.CharacterObject characterObject = WorldServiceLocator._WorldServer.CHARACTERs[plGUID];
										WS_Base.BaseObject objCharacter = this;
										num = (characterObject.CanSee(ref objCharacter) ? 1 : 0);
									}
									else
									{
										num = 0;
									}
									if (num != 0)
									{
										WorldServiceLocator._WorldServer.CHARACTERs[plGUID].client.SendMultiplyPackets(ref packet);
										WorldServiceLocator._WorldServer.CHARACTERs[plGUID].dynamicObjectsNear.Add(GUID);
										SeenBy.Add(plGUID);
									}
								}
								tMapTile = null;
							}
							j = (short)unchecked(j + 1);
						}
						while (j <= 1);
						i = (short)unchecked(i + 1);
					}
					while (i <= 1);
					packet.Dispose();
				}
			}

			public void RemoveFromWorld()
			{
				WorldServiceLocator._WS_Maps.GetMapTile(positionX, positionY, ref CellX, ref CellY);
				WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY].DynamicObjectsHere.Remove(GUID);
				ulong[] array = SeenBy.ToArray();
				foreach (ulong plGUID in array)
				{
					if (WorldServiceLocator._WorldServer.CHARACTERs[plGUID].dynamicObjectsNear.Contains(GUID))
					{
						WorldServiceLocator._WorldServer.CHARACTERs[plGUID].guidsForRemoving_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
						WorldServiceLocator._WorldServer.CHARACTERs[plGUID].guidsForRemoving.Add(GUID);
						WorldServiceLocator._WorldServer.CHARACTERs[plGUID].guidsForRemoving_Lock.ReleaseWriterLock();
						WorldServiceLocator._WorldServer.CHARACTERs[plGUID].dynamicObjectsNear.Remove(GUID);
					}
				}
			}

			public void AddEffect(WS_Spells.SpellEffect EffectInfo)
			{
				Effects.Add(EffectInfo);
			}

			public void RemoveEffect(WS_Spells.SpellEffect EffectInfo)
			{
				Effects.Remove(EffectInfo);
			}

			public bool Update()
			{
				if (Caster == null)
				{
					return true;
				}
				bool DeleteThis = false;
				checked
				{
					if (Duration > 1000)
					{
						Duration -= 1000;
					}
					else
					{
						DeleteThis = true;
					}
				}
				foreach (WS_Spells.SpellEffect effect in Effects)
				{
					WS_Spells.SpellEffect Effect = effect;
					if (Effect.GetRadius == 0f)
					{
						if (Effect.Amplitude == 0 || checked(WorldServiceLocator._WS_Spells.SPELLs[SpellID].GetDuration - Duration) % Effect.Amplitude == 0)
						{
							WS_Spells.ApplyAuraHandler obj = WorldServiceLocator._WS_Spells.AURAs[Effect.ApplyAuraIndex];
							ref WS_Base.BaseUnit caster = ref Caster;
							WS_Base.BaseObject baseObject = this;
							obj(ref caster, ref baseObject, ref Effect, SpellID, 1, AuraAction.AURA_UPDATE);
						}
						continue;
					}
					List<WS_Base.BaseUnit> Targets = WorldServiceLocator._WS_Spells.GetEnemyAtPoint(ref Caster, positionX, positionY, positionZ, Effect.GetRadius);
					foreach (WS_Base.BaseUnit item in Targets)
					{
						WS_Base.BaseUnit Target = item;
						if (Effect.Amplitude == 0 || checked(WorldServiceLocator._WS_Spells.SPELLs[SpellID].GetDuration - Duration) % Effect.Amplitude == 0)
						{
							WS_Spells.ApplyAuraHandler obj2 = WorldServiceLocator._WS_Spells.AURAs[Effect.ApplyAuraIndex];
							WS_Base.BaseObject baseObject = this;
							obj2(ref Target, ref baseObject, ref Effect, SpellID, 1, AuraAction.AURA_UPDATE);
						}
					}
				}
				if (DeleteThis)
				{
					Caster.dynamicObjects.Remove(this);
					return true;
				}
				return false;
			}

			public void Spawn()
			{
				AddToWorld();
				Packets.PacketClass packet = new Packets.PacketClass(OPCODES.SMSG_GAMEOBJECT_SPAWN_ANIM);
				packet.AddUInt64(GUID);
				SendToNearPlayers(ref packet, 0uL);
				packet.Dispose();
			}

			public void Delete()
			{
				if (Caster != null && Caster.dynamicObjects.Contains(this))
				{
					Caster.dynamicObjects.Remove(this);
				}
				Packets.PacketClass packet = new Packets.PacketClass(OPCODES.SMSG_GAMEOBJECT_DESPAWN_ANIM);
				packet.AddUInt64(GUID);
				SendToNearPlayers(ref packet, 0uL);
				packet.Dispose();
				RemoveFromWorld();
				Dispose();
			}
		}

		private ulong GetNewGUID()
		{
			ref ulong dynamicObjectsGUIDCounter = ref WorldServiceLocator._WorldServer.DynamicObjectsGUIDCounter;
			dynamicObjectsGUIDCounter = Convert.ToUInt64(decimal.Add(new decimal(dynamicObjectsGUIDCounter), 1m));
			return WorldServiceLocator._WorldServer.DynamicObjectsGUIDCounter;
		}
	}
}
