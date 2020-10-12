using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Mangos.Common.Enums.Chat;
using Mangos.Common.Enums.Faction;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Group;
using Mangos.Common.Enums.Misc;
using Mangos.Common.Enums.Player;
using Mangos.Common.Globals;
using Mangos.World.AI;
using Mangos.World.DataStores;
using Mangos.World.Globals;
using Mangos.World.Loots;
using Mangos.World.Maps;
using Mangos.World.Player;
using Mangos.World.Quests;
using Mangos.World.Server;
using Mangos.World.Spells;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Objects
{
	public class WS_Creatures
	{
		public class CreatureObject : WS_Base.BaseUnit, IDisposable
		{
			public int ID;

			public WS_Creatures_AI.TBaseAI aiScript;

			public float SpawnX;

			public float SpawnY;

			public float SpawnZ;

			public float SpawnO;

			public short Faction;

			public float SpawnRange;

			public byte MoveType;

			public int MoveFlags;

			public byte cStandState;

			public Timer ExpireTimer;

			public int SpawnTime;

			public float SpeedMod;

			public int EquipmentID;

			public int WaypointID;

			public int GameEvent;

			public WS_Spells.CastSpellParameters SpellCasted;

			public bool DestroyAtNoCombat;

			public bool Flying;

			public int LastPercent;

			public float OldX;

			public float OldY;

			public float OldZ;

			public float MoveX;

			public float MoveY;

			public float MoveZ;

			public int LastMove;

			public int LastMove_Time;

			public bool PositionUpdated;

			private bool _disposedValue;

			public CreatureInfo CreatureInfo => WorldServiceLocator._WorldServer.CREATURESDatabase[ID];

			public string Name => CreatureInfo.Name;

			public float MaxDistance => 35f;

			public bool isAbleToWalkOnWater
			{
				get
				{
					switch (CreatureInfo.CreatureFamily)
					{
					case 3:
					case 10:
					case 11:
					case 12:
					case 20:
					case 21:
					case 27:
						return false;
					default:
						return true;
					}
				}
			}

			public bool isAbleToWalkOnGround
			{
				get
				{
					byte creatureFamily = CreatureInfo.CreatureFamily;
					if (creatureFamily == byte.MaxValue)
					{
						return false;
					}
					return true;
				}
			}

			public bool isCritter => CreatureInfo.CreatureType == 8;

			public bool isGuard => (CreatureInfo.cNpcFlags & 0x40) == 64;

			public override bool IsDead
			{
				get
				{
					if (aiScript != null)
					{
						return Life.Current == 0 || aiScript.State == AIState.AI_DEAD || aiScript.State == AIState.AI_RESPAWN;
					}
					return Life.Current == 0;
				}
			}

			public bool Evade
			{
				get
				{
					if (aiScript != null && aiScript.State == AIState.AI_MOVING_TO_SPAWN)
					{
						return true;
					}
					return false;
				}
			}

			public int NPCTextID
			{
				get
				{
					checked
					{
						if (WorldServiceLocator._WS_DBCDatabase.CreatureGossip.ContainsKey(GUID - WorldServiceLocator._Global_Constants.GUID_UNIT))
						{
							return WorldServiceLocator._WS_DBCDatabase.CreatureGossip[GUID - WorldServiceLocator._Global_Constants.GUID_UNIT];
						}
						return 16777215;
					}
				}
			}

			public override bool IsFriendlyTo(ref WS_Base.BaseUnit Unit)
			{
				if (Unit == this)
				{
					return true;
				}
				if (Unit is WS_PlayerData.CharacterObject)
				{
					WS_PlayerData.CharacterObject characterObject = (WS_PlayerData.CharacterObject)Unit;
					if (characterObject.GM)
					{
						return true;
					}
					if (characterObject.GetReputation(characterObject.Faction) < ReputationRank.Friendly)
					{
						return false;
					}
					if (characterObject.GetReaction(characterObject.Faction) < TReaction.NEUTRAL)
					{
						return false;
					}
					characterObject = null;
				}
				else if (Unit is CreatureObject)
				{
					CreatureObject creatureObject = (CreatureObject)Unit;
					creatureObject = null;
				}
				return true;
			}

			public override bool IsEnemyTo(ref WS_Base.BaseUnit Unit)
			{
				if (Unit == this)
				{
					return false;
				}
				if (Unit is WS_PlayerData.CharacterObject)
				{
					WS_PlayerData.CharacterObject characterObject = (WS_PlayerData.CharacterObject)Unit;
					if (characterObject.GM)
					{
						return false;
					}
					if (characterObject.GetReputation(characterObject.Faction) < ReputationRank.Friendly)
					{
						return true;
					}
					if (characterObject.GetReaction(characterObject.Faction) < TReaction.NEUTRAL)
					{
						return true;
					}
					characterObject = null;
				}
				else if (Unit is CreatureObject)
				{
					CreatureObject creatureObject = (CreatureObject)Unit;
					creatureObject = null;
				}
				return false;
			}

			public float AggroRange(WS_PlayerData.CharacterObject objCharacter)
			{
				checked
				{
					short LevelDiff = (short)unchecked(Level - objCharacter.Level);
					float Range = 20 + LevelDiff;
					if (Range < 5f)
					{
						Range = 5f;
					}
					if (Range > 45f)
					{
						Range = 45f;
					}
					return Range;
				}
			}

			public void SendTargetUpdate(ulong TargetGUID)
			{
				Packets.UpdatePacketClass packet = new Packets.UpdatePacketClass();
				Packets.UpdateClass tmpUpdate = new Packets.UpdateClass(188);
				tmpUpdate.SetUpdateFlag(16, TargetGUID);
				Packets.PacketClass packet2 = packet;
				CreatureObject updateObject = this;
				tmpUpdate.AddToPacket(ref packet2, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
				tmpUpdate.Dispose();
				packet2 = packet;
				SendToNearPlayers(ref packet2, 0uL);
				packet.Dispose();
			}

			public WS_Base.BaseUnit GetRandomTarget()
			{
				if (aiScript == null)
				{
					return null;
				}
				if (aiScript.aiHateTable.Count == 0)
				{
					return null;
				}
				int i = 0;
				int target = WorldServiceLocator._WorldServer.Rnd.Next(0, aiScript.aiHateTable.Count);
				foreach (KeyValuePair<WS_Base.BaseUnit, int> tmpUnit in aiScript.aiHateTable)
				{
					if (target == i)
					{
						return tmpUnit.Key;
					}
					i = checked(i + 1);
				}
				return null;
			}

			public void FillAllUpdateFlags(ref Packets.UpdateClass Update)
			{
				Update.SetUpdateFlag(0, GUID);
				Update.SetUpdateFlag(4, Size);
				Update.SetUpdateFlag(2, 9);
				Update.SetUpdateFlag(3, ID);
				if (aiScript != null && aiScript.aiTarget != null)
				{
					Update.SetUpdateFlag(16, aiScript.aiTarget.GUID);
				}
				if (decimal.Compare(new decimal(SummonedBy), 0m) > 0)
				{
					Update.SetUpdateFlag(12, SummonedBy);
				}
				if (decimal.Compare(new decimal(CreatedBy), 0m) > 0)
				{
					Update.SetUpdateFlag(14, CreatedBy);
				}
				if (CreatedBySpell > 0)
				{
					Update.SetUpdateFlag(146, CreatedBySpell);
				}
				Update.SetUpdateFlag(131, Model);
				Update.SetUpdateFlag(132, WorldServiceLocator._WorldServer.CREATURESDatabase[ID].GetFirstModel);
				if (Mount > 0)
				{
					Update.SetUpdateFlag(133, Mount);
				}
				Update.SetUpdateFlag(36, cBytes0);
				Update.SetUpdateFlag(138, cBytes1);
				Update.SetUpdateFlag(164, cBytes2);
				Update.SetUpdateFlag(148, cEmoteState);
				Update.SetUpdateFlag(22, Life.Current);
				checked
				{
					Update.SetUpdateFlag(23 + unchecked((int)WorldServiceLocator._WorldServer.CREATURESDatabase[ID].ManaType), Mana.Current);
					Update.SetUpdateFlag(28, Life.Maximum);
					Update.SetUpdateFlag(29 + unchecked((int)WorldServiceLocator._WorldServer.CREATURESDatabase[ID].ManaType), Mana.Maximum);
					Update.SetUpdateFlag(34, Level);
					Update.SetUpdateFlag(35, Faction);
					Update.SetUpdateFlag(147, WorldServiceLocator._WorldServer.CREATURESDatabase[ID].cNpcFlags);
					Update.SetUpdateFlag(46, cUnitFlags);
					Update.SetUpdateFlag(143, cDynamicFlags);
					Update.SetUpdateFlag(155, WorldServiceLocator._WorldServer.CREATURESDatabase[ID].Resistances[0]);
					Update.SetUpdateFlag(156, WorldServiceLocator._WorldServer.CREATURESDatabase[ID].Resistances[1]);
					Update.SetUpdateFlag(157, WorldServiceLocator._WorldServer.CREATURESDatabase[ID].Resistances[2]);
					Update.SetUpdateFlag(158, WorldServiceLocator._WorldServer.CREATURESDatabase[ID].Resistances[3]);
					Update.SetUpdateFlag(159, WorldServiceLocator._WorldServer.CREATURESDatabase[ID].Resistances[4]);
					Update.SetUpdateFlag(160, WorldServiceLocator._WorldServer.CREATURESDatabase[ID].Resistances[5]);
					Update.SetUpdateFlag(161, WorldServiceLocator._WorldServer.CREATURESDatabase[ID].Resistances[6]);
					if (EquipmentID > 0)
					{
						try
						{
							if (WorldServiceLocator._WS_DBCDatabase.CreatureEquip.ContainsKey(EquipmentID))
							{
								WS_DBCDatabase.CreatureEquipInfo EquipmentInfo = WorldServiceLocator._WS_DBCDatabase.CreatureEquip[EquipmentID];
								Update.SetUpdateFlag(37, EquipmentInfo.EquipModel[0]);
								Update.SetUpdateFlag(40, EquipmentInfo.EquipInfo[0]);
								Update.SetUpdateFlag(41, EquipmentInfo.EquipSlot[0]);
								Update.SetUpdateFlag(38, EquipmentInfo.EquipModel[1]);
								Update.SetUpdateFlag(42, EquipmentInfo.EquipInfo[1]);
								Update.SetUpdateFlag(43, EquipmentInfo.EquipSlot[1]);
								Update.SetUpdateFlag(39, EquipmentInfo.EquipModel[2]);
								Update.SetUpdateFlag(44, EquipmentInfo.EquipInfo[2]);
								Update.SetUpdateFlag(45, EquipmentInfo.EquipSlot[2]);
							}
						}
						catch (DataException ex2)
						{
							ProjectData.SetProjectError(ex2);
							DataException ex = ex2;
							Console.ForegroundColor = ConsoleColor.Red;
							Console.WriteLine($"FillAllUpdateFlags : Unable to equip items {EquipmentID} for Creature");
							Console.ForegroundColor = ConsoleColor.Gray;
							ProjectData.ClearProjectError();
						}
					}
					Update.SetUpdateFlag(129, BoundingRadius);
					Update.SetUpdateFlag(130, CombatReach);
					int num = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1;
					for (int k = 0; k <= num; k++)
					{
						if (ActiveSpells[k] != null)
						{
							Update.SetUpdateFlag(47 + k, ActiveSpells[k].SpellID);
						}
					}
					int num2 = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECT_FLAGs - 1;
					for (int j = 0; j <= num2; j++)
					{
						Update.SetUpdateFlag(95 + j, ActiveSpells_Flags[j]);
					}
					int num3 = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECT_LEVELSs - 1;
					for (int i = 0; i <= num3; i++)
					{
						Update.SetUpdateFlag(113 + i, ActiveSpells_Count[i]);
						Update.SetUpdateFlag(101 + i, ActiveSpells_Level[i]);
					}
				}
			}

			public void MoveToInstant(float x, float y, float z, float o)
			{
				positionX = x;
				positionY = y;
				positionZ = z;
				orientation = o;
				if (SeenBy.Count > 0)
				{
					Packets.PacketClass packet = new Packets.PacketClass(OPCODES.MSG_MOVE_HEARTBEAT);
					packet.AddPackGUID(GUID);
					packet.AddInt32(0);
					packet.AddInt32(WorldServiceLocator._NativeMethods.timeGetTime(""));
					packet.AddSingle(positionX);
					packet.AddSingle(positionY);
					packet.AddSingle(positionZ);
					packet.AddSingle(orientation);
					packet.AddInt32(0);
					SendToNearPlayers(ref packet, 0uL);
					packet.Dispose();
				}
			}

			public void SetToRealPosition(bool Forced = false)
			{
				if (aiScript != null && (Forced || aiScript.State != AIState.AI_MOVING_TO_SPAWN))
				{
					int timeDiff = checked(WorldServiceLocator._NativeMethods.timeGetTime("") - LastMove);
					if ((Forced || aiScript.IsMoving()) && LastMove > 0 && timeDiff < LastMove_Time)
					{
						float distance = ((aiScript.State != AIState.AI_MOVING && aiScript.State != AIState.AI_WANDERING) ? ((float)timeDiff / 1000f * (CreatureInfo.RunSpeed * SpeedMod)) : ((float)timeDiff / 1000f * (CreatureInfo.WalkSpeed * SpeedMod)));
						positionX = (float)((double)OldX + Math.Cos(orientation) * (double)distance);
						positionY = (float)((double)OldY + Math.Sin(orientation) * (double)distance);
						positionZ = WorldServiceLocator._WS_Maps.GetZCoord(positionX, positionY, positionZ, MapID);
					}
					else if (!PositionUpdated && timeDiff >= LastMove_Time)
					{
						PositionUpdated = true;
						positionX = MoveX;
						positionY = MoveY;
						positionZ = MoveZ;
					}
				}
			}

			public void StopMoving()
			{
				if (aiScript != null && !aiScript.InCombat())
				{
					aiScript.Pause(10000);
					SetToRealPosition(Forced: true);
					MoveToInstant(positionX, positionY, positionZ, orientation);
				}
			}

			public int MoveTo(float x, float y, float z, float o = 0f, bool Running = false)
			{
				try
				{
					if (SeenBy.Count == 0)
					{
						return 10000;
					}
				}
				catch (Exception projectError)
				{
					ProjectData.SetProjectError(projectError);
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "MoveTo:SeenBy Failed");
					ProjectData.ClearProjectError();
				}
				int TimeToMove = 1;
				Packets.PacketClass SMSG_MONSTER_MOVE = new Packets.PacketClass(OPCODES.SMSG_MONSTER_MOVE);
				checked
				{
					try
					{
						SMSG_MONSTER_MOVE.AddPackGUID(GUID);
						SMSG_MONSTER_MOVE.AddSingle(positionX);
						SMSG_MONSTER_MOVE.AddSingle(positionY);
						SMSG_MONSTER_MOVE.AddSingle(positionZ);
						SMSG_MONSTER_MOVE.AddInt32(WorldServiceLocator._WS_Network.MsTime());
						if (o == 0f)
						{
							SMSG_MONSTER_MOVE.AddInt8(0);
						}
						else
						{
							SMSG_MONSTER_MOVE.AddInt8(4);
							SMSG_MONSTER_MOVE.AddSingle(o);
						}
						float moveDist = WorldServiceLocator._WS_Combat.GetDistance(positionX, x, positionY, y, positionZ, z);
						if (Flying)
						{
							SMSG_MONSTER_MOVE.AddInt32(768);
							TimeToMove = (int)Math.Round(moveDist / (CreatureInfo.RunSpeed * SpeedMod) * 1000f + 0.5f);
						}
						else if (Running)
						{
							SMSG_MONSTER_MOVE.AddInt32(256);
							TimeToMove = (int)Math.Round(moveDist / (CreatureInfo.RunSpeed * SpeedMod) * 1000f + 0.5f);
						}
						else
						{
							SMSG_MONSTER_MOVE.AddInt32(0);
							TimeToMove = (int)Math.Round(moveDist / (CreatureInfo.WalkSpeed * SpeedMod) * 1000f + 0.5f);
						}
						orientation = WorldServiceLocator._WS_Combat.GetOrientation(positionX, x, positionY, y);
						OldX = positionX;
						OldY = positionY;
						OldZ = positionZ;
						LastMove = WorldServiceLocator._NativeMethods.timeGetTime("");
						LastMove_Time = TimeToMove;
						PositionUpdated = false;
						positionX = x;
						positionY = y;
						positionZ = z;
						MoveX = x;
						MoveY = y;
						MoveZ = z;
						SMSG_MONSTER_MOVE.AddInt32(TimeToMove);
						SMSG_MONSTER_MOVE.AddInt32(1);
						SMSG_MONSTER_MOVE.AddSingle(x);
						SMSG_MONSTER_MOVE.AddSingle(y);
						SMSG_MONSTER_MOVE.AddSingle(z);
						SendToNearPlayers(ref SMSG_MONSTER_MOVE, 0uL);
					}
					catch (Exception ex2)
					{
						ProjectData.SetProjectError(ex2);
						Exception ex = ex2;
						WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "MoveTo:Main Failed - {0}", ex.Message);
						ProjectData.ClearProjectError();
					}
					finally
					{
						SMSG_MONSTER_MOVE.Dispose();
					}
					MoveCell();
					return TimeToMove;
				}
			}

			public bool CanMoveTo(float x, float y, float z)
			{
				WS_Maps wS_Maps = WorldServiceLocator._WS_Maps;
				WS_Base.BaseObject objCharacter = this;
				if (wS_Maps.IsOutsideOfMap(ref objCharacter))
				{
					return false;
				}
				if (z < WorldServiceLocator._WS_Maps.GetWaterLevel(x, y, checked((int)MapID)))
				{
					if (!isAbleToWalkOnWater)
					{
						return false;
					}
				}
				else if (!isAbleToWalkOnGround)
				{
					return false;
				}
				return true;
			}

			public void TurnTo(ref WS_Base.BaseObject Target)
			{
				TurnTo(Target.positionX, Target.positionY);
			}

			public void TurnTo(float x, float y)
			{
				orientation = WorldServiceLocator._WS_Combat.GetOrientation(positionX, x, positionY, y);
				TurnTo(orientation);
			}

			public void TurnTo(float orientation_)
			{
				orientation = orientation_;
				if (SeenBy.Count > 0 && (aiScript == null || !aiScript.IsMoving()))
				{
					Packets.PacketClass packet = new Packets.PacketClass(OPCODES.MSG_MOVE_HEARTBEAT);
					try
					{
						packet.AddPackGUID(GUID);
						packet.AddInt32(0);
						packet.AddInt32(WorldServiceLocator._NativeMethods.timeGetTime(""));
						packet.AddSingle(positionX);
						packet.AddSingle(positionY);
						packet.AddSingle(positionZ);
						packet.AddSingle(orientation);
						packet.AddInt32(0);
						SendToNearPlayers(ref packet, 0uL);
					}
					finally
					{
						packet.Dispose();
					}
				}
			}

			public override void Die(ref WS_Base.BaseUnit Attacker)
			{
				cUnitFlags = 16384;
				Life.Current = 0;
				Mana.Current = 0;
				if (aiScript != null)
				{
					SetToRealPosition(Forced: true);
					MoveToInstant(positionX, positionY, positionZ, orientation);
					PositionUpdated = true;
					LastMove = 0;
					LastMove_Time = 0;
					aiScript.State = AIState.AI_DEAD;
					aiScript.DoThink();
				}
				if (aiScript != null)
				{
					aiScript.OnDeath();
				}
				if (Attacker != null && Attacker is CreatureObject && ((CreatureObject)Attacker).aiScript != null)
				{
					WS_Creatures_AI.TBaseAI tBaseAI = ((CreatureObject)Attacker).aiScript;
					WS_Base.BaseUnit Victim = this;
					tBaseAI.OnKill(ref Victim);
				}
				Packets.UpdatePacketClass packetForNear = new Packets.UpdatePacketClass();
				Packets.UpdateClass UpdateData = new Packets.UpdateClass(188);
				checked
				{
					int num = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1;
					for (int i = 0; i <= num; i++)
					{
						if (ActiveSpells[i] != null)
						{
							RemoveAura(i, ref ActiveSpells[i].SpellCaster, RemovedByDuration: false, SendUpdate: false);
							UpdateData.SetUpdateFlag(47 + i, 0);
						}
					}
					UpdateData.SetUpdateFlag(22, Life.Current);
				}
				UpdateData.SetUpdateFlag((int)checked(23 + base.ManaType), Mana.Current);
				UpdateData.SetUpdateFlag(46, cUnitFlags);
				Packets.PacketClass packet = packetForNear;
				CreatureObject updateObject = this;
				UpdateData.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
				packet = packetForNear;
				SendToNearPlayers(ref packet, 0uL);
				packetForNear = (Packets.UpdatePacketClass)packet;
				packetForNear.Dispose();
				UpdateData.Dispose();
				if (Attacker is WS_PlayerData.CharacterObject)
				{
					((WS_PlayerData.CharacterObject)Attacker).RemoveFromCombat(this);
					WS_PlayerData.CharacterObject Character;
					if (!isCritter && !isGuard && CreatureInfo.cNpcFlags == 0)
					{
						Character = (WS_PlayerData.CharacterObject)Attacker;
						GiveXP(ref Character);
						Character = (WS_PlayerData.CharacterObject)Attacker;
						LootCorpse(ref Character);
					}
					WS_Quests aLLQUESTS = WorldServiceLocator._WorldServer.ALLQUESTS;
					Character = (WS_PlayerData.CharacterObject)Attacker;
					updateObject = this;
					aLLQUESTS.OnQuestKill(ref Character, ref updateObject);
					Attacker = Character;
				}
			}

			public override void DealDamage(int Damage, WS_Base.BaseUnit Attacker = null)
			{
				if (Life.Current == 0)
				{
					return;
				}
				RemoveAurasByInterruptFlag(2);
				checked
				{
					Life.Current -= Damage;
					if (Attacker != null && aiScript != null)
					{
						aiScript.OnGenerateHate(ref Attacker, Damage);
					}
					if (Life.Current == 0)
					{
						Die(ref Attacker);
						return;
					}
					int tmpPercent = (int)((double)Life.Current / (double)Life.Maximum * 100.0);
					if (tmpPercent != LastPercent)
					{
						LastPercent = tmpPercent;
						if (aiScript != null)
						{
							aiScript.OnHealthChange(LastPercent);
						}
					}
				}
				if (SeenBy.Count > 0)
				{
					Packets.UpdatePacketClass packetForNear = new Packets.UpdatePacketClass();
					Packets.UpdateClass UpdateData = new Packets.UpdateClass(188);
					UpdateData.SetUpdateFlag(22, Life.Current);
					UpdateData.SetUpdateFlag((int)checked(23 + base.ManaType), Mana.Current);
					Packets.PacketClass packet = packetForNear;
					CreatureObject updateObject = this;
					UpdateData.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
					packet = packetForNear;
					SendToNearPlayers(ref packet, 0uL);
					packetForNear.Dispose();
					UpdateData.Dispose();
				}
			}

			public override void Heal(int Damage, WS_Base.BaseUnit Attacker = null)
			{
				checked
				{
					if (Life.Current != 0)
					{
						Life.Current += Damage;
						if (SeenBy.Count > 0)
						{
							Packets.UpdatePacketClass packetForNear = new Packets.UpdatePacketClass();
							Packets.UpdateClass UpdateData = new Packets.UpdateClass(188);
							UpdateData.SetUpdateFlag(22, Life.Current);
							Packets.PacketClass packet = packetForNear;
							CreatureObject updateObject = this;
							UpdateData.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
							packet = packetForNear;
							SendToNearPlayers(ref packet, 0uL);
							packetForNear.Dispose();
							UpdateData.Dispose();
						}
					}
				}
			}

			public override void Energize(int Damage, ManaTypes Power, WS_Base.BaseUnit Attacker = null)
			{
				if (ManaType == Power)
				{
					checked
					{
						Mana.Current += Damage;
					}
					if (SeenBy.Count > 0)
					{
						Packets.UpdatePacketClass packetForNear = new Packets.UpdatePacketClass();
						Packets.UpdateClass UpdateData = new Packets.UpdateClass(188);
						UpdateData.SetUpdateFlag((int)checked(23 + base.ManaType), Mana.Current);
						Packets.PacketClass packet = packetForNear;
						CreatureObject updateObject = this;
						UpdateData.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
						packet = packetForNear;
						SendToNearPlayers(ref packet, 0uL);
						packetForNear.Dispose();
						UpdateData.Dispose();
					}
				}
			}

			public void LootCorpse(ref WS_PlayerData.CharacterObject Character)
			{
				if (GenerateLoot(ref Character, LootType.LOOTTYPE_CORPSE))
				{
					cDynamicFlags = 1;
				}
				else
				{
					if (CreatureInfo.SkinLootID <= 0)
					{
						return;
					}
					cUnitFlags |= 67108864;
				}
				Packets.PacketClass packet = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
				packet.AddInt32(1);
				packet.AddInt8(0);
				Packets.UpdateClass UpdateData = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
				UpdateData.SetUpdateFlag(143, cDynamicFlags);
				UpdateData.SetUpdateFlag(46, cUnitFlags);
				CreatureObject updateObject = this;
				UpdateData.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
				UpdateData.Dispose();
				if (!WorldServiceLocator._WS_Loot.LootTable.ContainsKey(GUID) && (cUnitFlags & 0x4000000) == 67108864)
				{
					SendToNearPlayers(ref packet, 0uL);
				}
				else if (Character.IsInGroup)
				{
					WorldServiceLocator._WS_Loot.LootTable[GUID].LootOwner = 0uL;
					switch (Character.Group.LootMethod)
					{
					case GroupLootMethod.LOOT_FREE_FOR_ALL:
						foreach (ulong objCharacter in Character.Group.LocalMembers)
						{
							if (SeenBy.Contains(objCharacter))
							{
								WorldServiceLocator._WS_Loot.LootTable[GUID].LootOwner = objCharacter;
								WorldServiceLocator._WorldServer.CHARACTERs[objCharacter].client.Send(ref packet);
							}
						}
						break;
					case GroupLootMethod.LOOT_MASTER:
						if (Character.Group.LocalLootMaster == null)
						{
							WorldServiceLocator._WS_Loot.LootTable[GUID].LootOwner = Character.GUID;
							Character.client.Send(ref packet);
						}
						else
						{
							WorldServiceLocator._WS_Loot.LootTable[GUID].LootOwner = Character.Group.LocalLootMaster.GUID;
							Character.Group.LocalLootMaster.client.Send(ref packet);
						}
						break;
					case GroupLootMethod.LOOT_ROUND_ROBIN:
					case GroupLootMethod.LOOT_GROUP:
					case GroupLootMethod.LOOT_NEED_BEFORE_GREED:
					{
						WS_PlayerData.CharacterObject cLooter = Character.Group.GetNextLooter();
						while (!SeenBy.Contains(cLooter.GUID) && cLooter != Character)
						{
							cLooter = Character.Group.GetNextLooter();
						}
						WorldServiceLocator._WS_Loot.LootTable[GUID].LootOwner = cLooter.GUID;
						cLooter.client.Send(ref packet);
						break;
					}
					}
				}
				else
				{
					WorldServiceLocator._WS_Loot.LootTable[GUID].LootOwner = Character.GUID;
					Character.client.Send(ref packet);
				}
				packet.Dispose();
			}

			public bool GenerateLoot(ref WS_PlayerData.CharacterObject Character, LootType LootType)
			{
				if (CreatureInfo.LootID == 0)
				{
					return false;
				}
				WS_Loot.LootObject Loot = new WS_Loot.LootObject(GUID, LootType);
				WorldServiceLocator._WS_Loot.LootTemplates_Creature.GetLoot(CreatureInfo.LootID)?.Process(ref Loot, 0);
				checked
				{
					if (LootType == LootType.LOOTTYPE_CORPSE && CreatureInfo.CreatureType == 7)
					{
						Loot.Money = WorldServiceLocator._WorldServer.Rnd.Next((int)CreatureInfo.MinGold, (int)(unchecked((long)CreatureInfo.MaxGold) + 1L));
					}
					Loot.LootOwner = Character.GUID;
					return true;
				}
			}

			public void GiveXP(ref WS_PlayerData.CharacterObject Character)
			{
				checked
				{
					int XP = unchecked((int)Level) * 5 + 45;
					int lvlDifference = unchecked((int)Character.Level) - unchecked((int)Level);
					if (lvlDifference > 0)
					{
						XP = (int)Math.Round((double)XP * (1.0 + 0.05 * (double)(unchecked((int)Level) - unchecked((int)Character.Level))));
					}
					else if (lvlDifference < 0)
					{
						byte GrayLevel = 0;
						switch (Character.Level)
						{
						case 0:
						case 1:
						case 2:
						case 3:
						case 4:
						case 5:
							GrayLevel = 0;
							break;
						case 6:
						case 7:
						case 8:
						case 9:
						case 10:
						case 11:
						case 12:
						case 13:
						case 14:
						case 15:
						case 16:
						case 17:
						case 18:
						case 19:
						case 20:
						case 21:
						case 22:
						case 23:
						case 24:
						case 25:
						case 26:
						case 27:
						case 28:
						case 29:
						case 30:
						case 31:
						case 32:
						case 33:
						case 34:
						case 35:
						case 36:
						case 37:
						case 38:
						case 39:
							GrayLevel = (byte)Math.Round(unchecked((double)(int)Character.Level - Math.Floor((double)(int)Character.Level / 10.0)) - 5.0);
							break;
						case 40:
						case 41:
						case 42:
						case 43:
						case 44:
						case 45:
						case 46:
						case 47:
						case 48:
						case 49:
						case 50:
						case 51:
						case 52:
						case 53:
						case 54:
						case 55:
						case 56:
						case 57:
						case 58:
						case 59:
							GrayLevel = (byte)Math.Round(unchecked((double)(int)Character.Level - Math.Floor((double)(int)Character.Level / 5.0)) - 1.0);
							break;
						default:
							GrayLevel = (byte)(unchecked((int)Character.Level) - 9);
							break;
						}
						if (unchecked((uint)Level > (uint)GrayLevel))
						{
							int ZD = 0;
							switch (Character.Level)
							{
							case 0:
							case 1:
							case 2:
							case 3:
							case 4:
							case 5:
							case 6:
							case 7:
								ZD = 5;
								break;
							case 8:
							case 9:
								ZD = 6;
								break;
							case 10:
							case 11:
								ZD = 7;
								break;
							case 12:
							case 13:
							case 14:
							case 15:
								ZD = 8;
								break;
							case 16:
							case 17:
							case 18:
							case 19:
								ZD = 9;
								break;
							case 20:
							case 21:
							case 22:
							case 23:
							case 24:
							case 25:
							case 26:
							case 27:
							case 28:
							case 29:
								ZD = 11;
								break;
							case 30:
							case 31:
							case 32:
							case 33:
							case 34:
							case 35:
							case 36:
							case 37:
							case 38:
							case 39:
								ZD = 12;
								break;
							case 40:
							case 41:
							case 42:
							case 43:
							case 44:
								ZD = 13;
								break;
							case 45:
							case 46:
							case 47:
							case 48:
							case 49:
								ZD = 14;
								break;
							case 50:
							case 51:
							case 52:
							case 53:
							case 54:
								ZD = 15;
								break;
							case 55:
							case 56:
							case 57:
							case 58:
							case 59:
								ZD = 16;
								break;
							default:
								ZD = 17;
								break;
							}
							XP = (int)Math.Round((double)XP * (1.0 - (double)(unchecked((int)Character.Level) - unchecked((int)Level)) / (double)ZD));
						}
						else
						{
							XP = 0;
						}
					}
					if (WorldServiceLocator._WorldServer.CREATURESDatabase[ID].Elite > 0)
					{
						XP *= 2;
					}
					XP = (int)Math.Round((float)XP * WorldServiceLocator._ConfigurationProvider.GetConfiguration().XPRate);
					if (!Character.IsInGroup)
					{
						int RestedXP2 = 0;
						if (Character.RestBonus >= 0)
						{
							RestedXP2 = XP;
							if (RestedXP2 > Character.RestBonus)
							{
								RestedXP2 = Character.RestBonus;
							}
							Character.RestBonus -= RestedXP2;
							XP += RestedXP2;
						}
						Character.AddXP(XP, RestedXP2, GUID);
						return;
					}
					XP = (int)Math.Round((double)XP / (double)Character.Group.GetMembersCount());
					int membersCount = Character.Group.GetMembersCount();
					XP = ((membersCount <= 2) ? (XP * 1) : (membersCount switch
					{
						3 => (int)Math.Round((double)XP * 1.166), 
						4 => (int)Math.Round((double)XP * 1.3), 
						_ => (int)Math.Round((double)XP * 1.4), 
					}));
					int baseLvl = 0;
					foreach (ulong Member2 in Character.Group.LocalMembers)
					{
						WS_PlayerData.CharacterObject characterObject = WorldServiceLocator._WorldServer.CHARACTERs[Member2];
						if (!characterObject.DEAD && Math.Sqrt(Math.Pow(positionX - positionX, 2.0) + Math.Pow(positionY - positionY, 2.0)) <= (double)VisibleDistance)
						{
							baseLvl += unchecked((int)Level);
						}
						characterObject = null;
					}
					foreach (ulong Member in Character.Group.LocalMembers)
					{
						WS_PlayerData.CharacterObject characterObject2 = WorldServiceLocator._WorldServer.CHARACTERs[Member];
						if (!characterObject2.DEAD && Math.Sqrt(Math.Pow(positionX - positionX, 2.0) + Math.Pow(positionY - positionY, 2.0)) <= (double)VisibleDistance)
						{
							int tmpXP = XP;
							int RestedXP = 0;
							if (characterObject2.RestBonus >= 0)
							{
								RestedXP = tmpXP;
								if (RestedXP > characterObject2.RestBonus)
								{
									RestedXP = characterObject2.RestBonus;
								}
								characterObject2.RestBonus -= RestedXP;
								tmpXP += RestedXP;
							}
							tmpXP = (int)((double)(tmpXP * unchecked((int)Level)) / (double)baseLvl);
							characterObject2.AddXP(tmpXP, RestedXP, GUID, LogIt: false);
							characterObject2.LogXPGain(tmpXP, RestedXP, GUID, (float)((double)(Character.Group.GetMembersCount() - 1) / 10.0));
						}
						characterObject2 = null;
					}
				}
			}

			public void StopCasting()
			{
				if (SpellCasted != null && !SpellCasted.Finished)
				{
					SpellCasted.StopCast();
				}
			}

			public void ApplySpell(int SpellID)
			{
				if (WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(SpellID))
				{
					WS_Spells.SpellTargets t = new WS_Spells.SpellTargets();
					WS_Base.BaseUnit objCharacter = this;
					t.SetTarget_SELF(ref objCharacter);
					WS_Spells.SpellInfo spellInfo = WorldServiceLocator._WS_Spells.SPELLs[SpellID];
					WS_Base.BaseObject caster = this;
					spellInfo.Apply(ref caster, t);
				}
			}

			public int CastSpellOnSelf(int SpellID)
			{
				if (Spell_Silenced)
				{
					return -1;
				}
				WS_Spells.SpellTargets Targets = new WS_Spells.SpellTargets();
				WS_Spells.SpellTargets spellTargets = Targets;
				WS_Base.BaseUnit objCharacter = this;
				spellTargets.SetTarget_SELF(ref objCharacter);
				WS_Base.BaseObject Caster = this;
				WS_Spells.CastSpellParameters tmpSpell = new WS_Spells.CastSpellParameters(ref Targets, ref Caster, SpellID);
				if (WorldServiceLocator._WS_Spells.SPELLs[SpellID].GetDuration > 0)
				{
					SpellCasted = tmpSpell;
				}
				ThreadPool.QueueUserWorkItem(new WaitCallback(tmpSpell.Cast));
				return WorldServiceLocator._WS_Spells.SPELLs[SpellID].GetCastTime;
			}

			public int CastSpell(int SpellID, WS_Base.BaseUnit Target)
			{
				if (Spell_Silenced)
				{
					return -1;
				}
				if (Target == null)
				{
					return -1;
				}
				if (WorldServiceLocator._WS_Combat.GetDistance(this, Target) > (float)WorldServiceLocator._WS_Spells.SPELLs[SpellID].GetRange)
				{
					return -1;
				}
				WS_Spells.SpellTargets Targets = new WS_Spells.SpellTargets();
				Targets.SetTarget_UNIT(ref Target);
				WS_Base.BaseObject Caster = this;
				WS_Spells.CastSpellParameters tmpSpell = new WS_Spells.CastSpellParameters(ref Targets, ref Caster, SpellID);
				if (WorldServiceLocator._WS_Spells.SPELLs[SpellID].GetDuration > 0)
				{
					SpellCasted = tmpSpell;
				}
				ThreadPool.QueueUserWorkItem(new WaitCallback(tmpSpell.Cast));
				return WorldServiceLocator._WS_Spells.SPELLs[SpellID].GetCastTime;
			}

			public int CastSpell(int SpellID, float x, float y, float z)
			{
				if (Spell_Silenced)
				{
					return -1;
				}
				if (WorldServiceLocator._WS_Combat.GetDistance(this, x, y, z) > (float)WorldServiceLocator._WS_Spells.SPELLs[SpellID].GetRange)
				{
					return -1;
				}
				WS_Spells.SpellTargets Targets = new WS_Spells.SpellTargets();
				Targets.SetTarget_DESTINATIONLOCATION(x, y, z);
				WS_Base.BaseObject Caster = this;
				WS_Spells.CastSpellParameters tmpSpell = new WS_Spells.CastSpellParameters(ref Targets, ref Caster, SpellID);
				if (WorldServiceLocator._WS_Spells.SPELLs[SpellID].GetDuration > 0)
				{
					SpellCasted = tmpSpell;
				}
				ThreadPool.QueueUserWorkItem(new WaitCallback(tmpSpell.Cast));
				return WorldServiceLocator._WS_Spells.SPELLs[SpellID].GetCastTime;
			}

			public void SpawnCreature(int Entry, float PosX, float PosY, float PosZ)
			{
				CreatureObject tmpCreature = new CreatureObject(Entry, PosX, PosY, PosZ, 0f, checked((int)MapID))
				{
					instance = instance,
					DestroyAtNoCombat = true
				};
				tmpCreature.AddToWorld();
				if (tmpCreature.aiScript != null)
				{
					tmpCreature.aiScript.Dispose();
				}
				tmpCreature.aiScript = new WS_Creatures_AI.DefaultAI(ref tmpCreature);
				tmpCreature.aiScript.aiHateTable = aiScript.aiHateTable;
				tmpCreature.aiScript.OnEnterCombat();
				tmpCreature.aiScript.State = AIState.AI_ATTACKING;
				tmpCreature.aiScript.DoThink();
			}

			public void SendChatMessage(string Message, ChatMsg msgType, LANGUAGES msgLanguage, ulong SecondGUID = 0uL)
			{
				Packets.PacketClass packet = new Packets.PacketClass(OPCODES.SMSG_MESSAGECHAT);
				byte flag = 0;
				packet.AddInt8(checked((byte)msgType));
				packet.AddInt32((int)msgLanguage);
				if ((uint)(msgType - 11) <= 2u)
				{
					packet.AddUInt64(GUID);
					packet.AddInt32(checked(Encoding.UTF8.GetByteCount(Name) + 1));
					packet.AddString(Name);
					packet.AddUInt64(SecondGUID);
				}
				else
				{
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Creature.SendChatMessage() must not handle this chat type!");
				}
				packet.AddInt32(checked(Encoding.UTF8.GetByteCount(Message) + 1));
				packet.AddString(Message);
				packet.AddInt8(flag);
				SendToNearPlayers(ref packet, 0uL);
				packet.Dispose();
			}

			public void ResetAI()
			{
				aiScript.Dispose();
				CreatureObject Creature = this;
				aiScript = new WS_Creatures_AI.DefaultAI(ref Creature);
				MoveType = 1;
			}

			public void Initialize()
			{
				Level = checked((byte)WorldServiceLocator._WorldServer.Rnd.Next(WorldServiceLocator._WorldServer.CREATURESDatabase[ID].LevelMin, WorldServiceLocator._WorldServer.CREATURESDatabase[ID].LevelMax));
				Size = WorldServiceLocator._WorldServer.CREATURESDatabase[ID].Size;
				if (Size == 0f)
				{
					Size = 1f;
				}
				Model = WorldServiceLocator._WorldServer.CREATURESDatabase[ID].GetRandomModel;
				ManaType = (ManaTypes)WorldServiceLocator._WorldServer.CREATURESDatabase[ID].ManaType;
				Mana.Base = WorldServiceLocator._WorldServer.CREATURESDatabase[ID].Mana;
				Mana.Current = Mana.Maximum;
				Life.Base = WorldServiceLocator._WorldServer.CREATURESDatabase[ID].Life;
				Life.Current = Life.Maximum;
				Faction = WorldServiceLocator._WorldServer.CREATURESDatabase[ID].Faction;
				byte i = 0;
				checked
				{
					do
					{
						Resistances[i].Base = WorldServiceLocator._WorldServer.CREATURESDatabase[ID].Resistances[i];
						i = (byte)unchecked((uint)(i + 1));
					}
					while (unchecked((uint)i) <= 6u);
					if (EquipmentID == 0 && WorldServiceLocator._WorldServer.CREATURESDatabase[ID].EquipmentID > 0)
					{
						EquipmentID = WorldServiceLocator._WorldServer.CREATURESDatabase[ID].EquipmentID;
					}
					if (WorldServiceLocator._WS_DBCDatabase.CreatureModel.ContainsKey(Model))
					{
						BoundingRadius = WorldServiceLocator._WS_DBCDatabase.CreatureModel[Model].BoundingRadius;
						CombatReach = WorldServiceLocator._WS_DBCDatabase.CreatureModel[Model].CombatReach;
					}
					MechanicImmunity = WorldServiceLocator._WorldServer.CREATURESDatabase[ID].MechanicImmune;
					CanSeeInvisibility_Stealth = 5 * unchecked((int)Level);
					CanSeeInvisibility_Invisibility = 0;
					if ((WorldServiceLocator._WorldServer.CREATURESDatabase[ID].cNpcFlags & 0x20) == 32)
					{
						Invisibility = InvisibilityLevel.DEAD;
						cUnitFlags = 16777318;
					}
					cDynamicFlags = WorldServiceLocator._WorldServer.CREATURESDatabase[ID].DynFlags;
					StandState = cStandState;
					cBytes2 = 1;
					if (this is WS_Pets.PetObject)
					{
						CreatureObject Creature = this;
						aiScript = new WS_Pets.PetAI(ref Creature);
						return;
					}
					if (Operators.CompareString(WorldServiceLocator._WorldServer.CREATURESDatabase[ID].AIScriptSource, "", TextCompare: false) != 0)
					{
						aiScript = (WS_Creatures_AI.TBaseAI)WorldServiceLocator._WorldServer.AI.InvokeConstructor(WorldServiceLocator._WorldServer.CREATURESDatabase[ID].AIScriptSource, new object[1]
						{
							this
						});
					}
					else if (File.Exists("scripts\\creatures\\" + WorldServiceLocator._Functions.FixName(Name) + ".vb"))
					{
						ScriptedObject tmpScript = new ScriptedObject("scripts\\creatures\\" + WorldServiceLocator._Functions.FixName(Name) + ".vb", "", InMemory: true);
						aiScript = (WS_Creatures_AI.TBaseAI)tmpScript.InvokeConstructor("CreatureAI_" + WorldServiceLocator._Functions.FixName(Name).Replace(" ", "_"), new object[1]
						{
							this
						});
						tmpScript.Dispose();
					}
					if (aiScript != null)
					{
						return;
					}
					if (isCritter)
					{
						CreatureObject Creature = this;
						aiScript = new WS_Creatures_AI.CritterAI(ref Creature);
					}
					else if (isGuard)
					{
						if (MoveType == 2)
						{
							CreatureObject Creature = this;
							aiScript = new WS_Creatures_AI.GuardWaypointAI(ref Creature);
						}
						else
						{
							CreatureObject Creature = this;
							aiScript = new WS_Creatures_AI.GuardAI(ref Creature);
						}
					}
					else if (MoveType == 1)
					{
						CreatureObject Creature = this;
						aiScript = new WS_Creatures_AI.DefaultAI(ref Creature);
					}
					else if (MoveType == 2)
					{
						CreatureObject Creature = this;
						aiScript = new WS_Creatures_AI.WaypointAI(ref Creature);
					}
					else
					{
						CreatureObject Creature = this;
						aiScript = new WS_Creatures_AI.StandStillAI(ref Creature);
					}
				}
			}

			public CreatureObject(ulong GUID_, DataRow Info = null)
			{
				ID = 0;
				aiScript = null;
				SpawnX = 0f;
				SpawnY = 0f;
				SpawnZ = 0f;
				SpawnO = 0f;
				Faction = 0;
				SpawnRange = 0f;
				MoveType = 0;
				MoveFlags = 0;
				cStandState = 0;
				ExpireTimer = null;
				SpawnTime = 0;
				SpeedMod = 1f;
				EquipmentID = 0;
				WaypointID = 0;
				GameEvent = 0;
				SpellCasted = null;
				DestroyAtNoCombat = false;
				Flying = false;
				LastPercent = 100;
				OldX = 0f;
				OldY = 0f;
				OldZ = 0f;
				MoveX = 0f;
				MoveY = 0f;
				MoveZ = 0f;
				LastMove = 0;
				LastMove_Time = 0;
				PositionUpdated = true;
				if (Info == null)
				{
					DataTable MySQLQuery = new DataTable();
					WorldServiceLocator._WorldServer.WorldDatabase.Query($"SELECT * FROM creature LEFT OUTER JOIN game_event_creature ON creature.guid = game_event_creature.guid WHERE creature.guid = {GUID_};", ref MySQLQuery);
					if (MySQLQuery.Rows.Count <= 0)
					{
						WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Creature Spawn not found in database. [GUID={0:X}]", GUID_);
						return;
					}
					Info = MySQLQuery.Rows[0];
				}
				DataRow AddonInfo = null;
				DataTable AddonInfoQuery = new DataTable();
				WorldServiceLocator._WorldServer.WorldDatabase.Query($"SELECT * FROM spawns_creatures_addon WHERE spawn_id = {GUID_};", ref AddonInfoQuery);
				if (AddonInfoQuery.Rows.Count > 0)
				{
					AddonInfo = AddonInfoQuery.Rows[0];
				}
				positionX = Conversions.ToSingle(Info["position_X"]);
				positionY = Conversions.ToSingle(Info["position_Y"]);
				positionZ = Conversions.ToSingle(Info["position_Z"]);
				orientation = Conversions.ToSingle(Info["orientation"]);
				OldX = positionX;
				OldY = positionY;
				OldZ = positionZ;
				SpawnX = positionX;
				SpawnY = positionY;
				SpawnZ = positionZ;
				SpawnO = orientation;
				ID = Conversions.ToInteger(Info["id"]);
				MapID = Conversions.ToUInteger(Info["map"]);
				SpawnID = Conversions.ToInteger(Info["guid"]);
				Model = Conversions.ToInteger(Info["modelid"]);
				SpawnTime = Conversions.ToInteger(Info["spawntimesecs"]);
				SpawnRange = Conversions.ToSingle(Info["spawndist"]);
				MoveType = Conversions.ToByte(Info["MovementType"]);
				Life.Current = Conversions.ToInteger(Info["curhealth"]);
				Mana.Current = Conversions.ToInteger(Info["curmana"]);
				EquipmentID = Conversions.ToInteger(Info["equipment_id"]);
				if (AddonInfo != null)
				{
					Mount = Conversions.ToInteger(AddonInfo["spawn_mount"]);
					cEmoteState = Conversions.ToInteger(AddonInfo["spawn_emote"]);
					MoveFlags = Conversions.ToInteger(AddonInfo["spawn_moveflags"]);
					cBytes0 = Conversions.ToInteger(AddonInfo["spawn_bytes0"]);
					cBytes1 = Conversions.ToInteger(AddonInfo["spawn_bytes1"]);
					cBytes2 = Conversions.ToInteger(AddonInfo["spawn_bytes2"]);
					WaypointID = Conversions.ToInteger(AddonInfo["spawn_pathid"]);
				}
				if (!WorldServiceLocator._WorldServer.CREATURESDatabase.ContainsKey(ID))
				{
					CreatureInfo baseCreature = new CreatureInfo(ID);
				}
				GUID = checked(GUID_ + WorldServiceLocator._Global_Constants.GUID_UNIT);
				Initialize();
				try
				{
					WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
					WorldServiceLocator._WorldServer.WORLD_CREATUREs.Add(GUID, this);
					WorldServiceLocator._WorldServer.WORLD_CREATUREsKeys.Add(GUID);
					WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.ReleaseWriterLock();
				}
				catch (Exception ex2)
				{
					ProjectData.SetProjectError(ex2);
					Exception ex = ex2;
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "WS_Creatures:New failed - Guid: {1}  {0}", ex.Message, GUID_);
					ProjectData.ClearProjectError();
				}
			}

			public CreatureObject(ulong GUID_, int ID_)
			{
				ID = 0;
				aiScript = null;
				SpawnX = 0f;
				SpawnY = 0f;
				SpawnZ = 0f;
				SpawnO = 0f;
				Faction = 0;
				SpawnRange = 0f;
				MoveType = 0;
				MoveFlags = 0;
				cStandState = 0;
				ExpireTimer = null;
				SpawnTime = 0;
				SpeedMod = 1f;
				EquipmentID = 0;
				WaypointID = 0;
				GameEvent = 0;
				SpellCasted = null;
				DestroyAtNoCombat = false;
				Flying = false;
				LastPercent = 100;
				OldX = 0f;
				OldY = 0f;
				OldZ = 0f;
				MoveX = 0f;
				MoveY = 0f;
				MoveZ = 0f;
				LastMove = 0;
				LastMove_Time = 0;
				PositionUpdated = true;
				if (!WorldServiceLocator._WorldServer.CREATURESDatabase.ContainsKey(ID_))
				{
					CreatureInfo baseCreature = new CreatureInfo(ID_);
				}
				ID = ID_;
				GUID = GUID_;
				Initialize();
				try
				{
					WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
					WorldServiceLocator._WorldServer.WORLD_CREATUREs.Add(GUID, this);
					WorldServiceLocator._WorldServer.WORLD_CREATUREsKeys.Add(GUID);
					WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.ReleaseWriterLock();
				}
				catch (Exception ex2)
				{
					ProjectData.SetProjectError(ex2);
					Exception ex = ex2;
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "WS_Creatures:New failed - Guid: {1} ID: {2}  {0}", ex.Message, GUID_, ID_);
					ProjectData.ClearProjectError();
				}
			}

			public CreatureObject(int ID_)
			{
				ID = 0;
				aiScript = null;
				SpawnX = 0f;
				SpawnY = 0f;
				SpawnZ = 0f;
				SpawnO = 0f;
				Faction = 0;
				SpawnRange = 0f;
				MoveType = 0;
				MoveFlags = 0;
				cStandState = 0;
				ExpireTimer = null;
				SpawnTime = 0;
				SpeedMod = 1f;
				EquipmentID = 0;
				WaypointID = 0;
				GameEvent = 0;
				SpellCasted = null;
				DestroyAtNoCombat = false;
				Flying = false;
				LastPercent = 100;
				OldX = 0f;
				OldY = 0f;
				OldZ = 0f;
				MoveX = 0f;
				MoveY = 0f;
				MoveZ = 0f;
				LastMove = 0;
				LastMove_Time = 0;
				PositionUpdated = true;
				if (!WorldServiceLocator._WorldServer.CREATURESDatabase.ContainsKey(ID_))
				{
					CreatureInfo baseCreature = new CreatureInfo(ID_);
				}
				ID = ID_;
				GUID = WorldServiceLocator._WS_Creatures.GetNewGUID();
				Initialize();
				try
				{
					WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
					WorldServiceLocator._WorldServer.WORLD_CREATUREs.Add(GUID, this);
					WorldServiceLocator._WorldServer.WORLD_CREATUREsKeys.Add(GUID);
					WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.ReleaseWriterLock();
				}
				catch (Exception ex2)
				{
					ProjectData.SetProjectError(ex2);
					Exception ex = ex2;
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "WS_Creatures:New failed - Guid: {1} ID: {2}  {0}", ex.Message, ID_);
					ProjectData.ClearProjectError();
				}
			}

			public CreatureObject(int ID_, float PosX, float PosY, float PosZ, float Orientation_, int Map, int Duration = 0)
			{
				ID = 0;
				aiScript = null;
				SpawnX = 0f;
				SpawnY = 0f;
				SpawnZ = 0f;
				SpawnO = 0f;
				Faction = 0;
				SpawnRange = 0f;
				MoveType = 0;
				MoveFlags = 0;
				cStandState = 0;
				ExpireTimer = null;
				SpawnTime = 0;
				SpeedMod = 1f;
				EquipmentID = 0;
				WaypointID = 0;
				GameEvent = 0;
				SpellCasted = null;
				DestroyAtNoCombat = false;
				Flying = false;
				LastPercent = 100;
				OldX = 0f;
				OldY = 0f;
				OldZ = 0f;
				MoveX = 0f;
				MoveY = 0f;
				MoveZ = 0f;
				LastMove = 0;
				LastMove_Time = 0;
				PositionUpdated = true;
				if (!WorldServiceLocator._WorldServer.CREATURESDatabase.ContainsKey(ID_))
				{
					CreatureInfo baseCreature = new CreatureInfo(ID_);
				}
				ID = ID_;
				GUID = WorldServiceLocator._WS_Creatures.GetNewGUID();
				positionX = PosX;
				positionY = PosY;
				positionZ = PosZ;
				orientation = Orientation_;
				MapID = checked((uint)Map);
				SpawnX = PosX;
				SpawnY = PosY;
				SpawnZ = PosZ;
				SpawnO = Orientation_;
				Initialize();
				if (Duration > 0)
				{
					ExpireTimer = new Timer(new TimerCallback(Destroy), null, Duration, Duration);
				}
				try
				{
					WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
					WorldServiceLocator._WorldServer.WORLD_CREATUREs.Add(GUID, this);
					WorldServiceLocator._WorldServer.WORLD_CREATUREsKeys.Add(GUID);
					WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.ReleaseWriterLock();
				}
				catch (Exception ex2)
				{
					ProjectData.SetProjectError(ex2);
					Exception ex = ex2;
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "WS_Creatures:New failed - Guid: {1} ID: {2} Map: {3}  {0}", ex.Message, GUID, ID_, Map);
					ProjectData.ClearProjectError();
				}
			}

			protected virtual void Dispose(bool disposing)
			{
				if (!_disposedValue)
				{
					if (aiScript != null)
					{
						aiScript.Dispose();
					}
					RemoveFromWorld();
					try
					{
						WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
						WorldServiceLocator._WorldServer.WORLD_CREATUREs.Remove(GUID);
						WorldServiceLocator._WorldServer.WORLD_CREATUREsKeys.Remove(GUID);
						WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.ReleaseWriterLock();
						ExpireTimer.Dispose();
					}
					catch (Exception ex2)
					{
						ProjectData.SetProjectError(ex2);
						Exception ex = ex2;
						WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "WS_Creatures:Dispose failed -  {0}", ex.Message);
						ProjectData.ClearProjectError();
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
				this.Dispose();
			}

			public void Destroy(object state = null)
			{
				if (decimal.Compare(new decimal(SummonedBy), 0m) > 0 && WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(SummonedBy) && WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(SummonedBy) && WorldServiceLocator._WorldServer.CHARACTERs[SummonedBy].NonCombatPet != null && WorldServiceLocator._WorldServer.CHARACTERs[SummonedBy].NonCombatPet == this)
				{
					WorldServiceLocator._WorldServer.CHARACTERs[SummonedBy].NonCombatPet = null;
				}
				Packets.PacketClass packet = new Packets.PacketClass(OPCODES.SMSG_DESTROY_OBJECT);
				packet.AddUInt64(GUID);
				SendToNearPlayers(ref packet, 0uL);
				packet.Dispose();
				Dispose();
			}

			public void Despawn()
			{
				RemoveFromWorld();
				if (WorldServiceLocator._WS_Loot.LootTable.ContainsKey(GUID))
				{
					WorldServiceLocator._WS_Loot.LootTable[GUID].Dispose();
				}
				if (SpawnTime > 0)
				{
					if (aiScript != null)
					{
						aiScript.State = AIState.AI_RESPAWN;
						aiScript.Pause(checked(SpawnTime * 1000));
					}
				}
				else
				{
					Dispose();
				}
			}

			public void Respawn()
			{
				Life.Current = Life.Maximum;
				Mana.Current = Mana.Maximum;
				cUnitFlags &= -16385;
				cDynamicFlags = 0;
				positionX = SpawnX;
				positionY = SpawnY;
				positionZ = SpawnZ;
				orientation = SpawnO;
				if (aiScript != null)
				{
					aiScript.OnLeaveCombat(Reset: false);
					aiScript.State = AIState.AI_WANDERING;
				}
				if (SeenBy.Count > 0)
				{
					Packets.UpdatePacketClass packetForNear = new Packets.UpdatePacketClass();
					Packets.UpdateClass UpdateData = new Packets.UpdateClass(188);
					UpdateData.SetUpdateFlag(22, Life.Current);
					UpdateData.SetUpdateFlag((int)checked(23 + base.ManaType), Mana.Current);
					UpdateData.SetUpdateFlag(46, cUnitFlags);
					UpdateData.SetUpdateFlag(143, cDynamicFlags);
					Packets.PacketClass packet = packetForNear;
					CreatureObject updateObject = this;
					UpdateData.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
					packet = packetForNear;
					SendToNearPlayers(ref packet, 0uL);
					packetForNear = (Packets.UpdatePacketClass)packet;
					packetForNear.Dispose();
					UpdateData.Dispose();
					MoveToInstant(SpawnX, SpawnY, SpawnZ, SpawnO);
				}
				else
				{
					AddToWorld();
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
					WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY].CreaturesHere.Add(GUID);
				}
				catch (Exception ex2)
				{
					ProjectData.SetProjectError(ex2);
					Exception ex = ex2;
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "WS_Creatures:AddToWorld failed - Guid: {1} ID: {2}  {0}", ex.Message);
					ProjectData.ClearProjectError();
					return;
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
								WS_Maps.TMapTile tMapTile = WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[(short)unchecked(CellX + i), (short)unchecked(CellY + j)];
								ulong[] list = tMapTile.PlayersHere.ToArray();
								ulong[] array = list;
								foreach (ulong plGUID in array)
								{
									WS_PlayerData.CharacterObject characterObject = WorldServiceLocator._WorldServer.CHARACTERs[plGUID];
									WS_Base.BaseObject objCharacter = this;
									if (characterObject.CanSee(ref objCharacter))
									{
										Packets.PacketClass packet = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
										try
										{
											packet.AddInt32(1);
											packet.AddInt8(0);
											Packets.UpdateClass tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_UNIT);
											FillAllUpdateFlags(ref tmpUpdate);
											Packets.UpdateClass updateClass = tmpUpdate;
											CreatureObject updateObject = this;
											updateClass.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, ref updateObject);
											tmpUpdate.Dispose();
											WorldServiceLocator._WorldServer.CHARACTERs[plGUID].client.SendMultiplyPackets(ref packet);
											WorldServiceLocator._WorldServer.CHARACTERs[plGUID].creaturesNear.Add(GUID);
											SeenBy.Add(plGUID);
										}
										finally
										{
											packet.Dispose();
										}
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
				}
			}

			public void RemoveFromWorld()
			{
				WorldServiceLocator._WS_Maps.GetMapTile(positionX, positionY, ref CellX, ref CellY);
				WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY].CreaturesHere.Remove(GUID);
				ulong[] array = SeenBy.ToArray();
				foreach (ulong plGUID in array)
				{
					if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(plGUID))
					{
						WorldServiceLocator._WorldServer.CHARACTERs[plGUID].guidsForRemoving_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
						WorldServiceLocator._WorldServer.CHARACTERs[plGUID].guidsForRemoving.Add(GUID);
						WorldServiceLocator._WorldServer.CHARACTERs[plGUID].guidsForRemoving_Lock.ReleaseWriterLock();
						WorldServiceLocator._WorldServer.CHARACTERs[plGUID].creaturesNear.Remove(GUID);
					}
				}
				SeenBy.Clear();
			}

			public void MoveCell()
			{
				try
				{
					if (CellX != WorldServiceLocator._WS_Maps.GetMapTileX(positionX) || CellY != WorldServiceLocator._WS_Maps.GetMapTileY(positionY))
					{
						if (!Information.IsNothing(WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY].CreaturesHere.Remove(GUID)))
						{
							WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY].CreaturesHere.Remove(GUID);
						}
						WorldServiceLocator._WS_Maps.GetMapTile(positionX, positionY, ref CellX, ref CellY);
						if (WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY] == null)
						{
							aiScript.Reset();
						}
						else
						{
							WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY].CreaturesHere.Add(GUID);
						}
					}
				}
				catch (Exception ex2)
				{
					ProjectData.SetProjectError(ex2);
					Exception e = ex2;
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "WS_Creatures:MoveCell - Creature outside of map bounds, Resetting  {0}", e.Message);
					try
					{
						aiScript.Reset();
					}
					catch (Exception ex3)
					{
						ProjectData.SetProjectError(ex3);
						Exception ex = ex3;
						WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "WS_Creatures:MoveCell - Couldn't reset creature outside of map bounds, Disposing  {0}", ex.Message);
						aiScript.Dispose();
						ProjectData.ClearProjectError();
					}
					ProjectData.ClearProjectError();
				}
			}
		}

		public class NPCText
		{
			public byte Count;

			public int TextID;

			public float[] Probability;

			public int[] Language;

			public string[] TextLine1;

			public string[] TextLine2;

			public int[] Emote1;

			public int[] Emote2;

			public int[] Emote3;

			public int[] EmoteDelay1;

			public int[] EmoteDelay2;

			public int[] EmoteDelay3;

			public NPCText(int _TextID)
			{
				Count = 1;
				TextID = 0;
				Probability = new float[8];
				Language = new int[8];
				TextLine1 = new string[8]
				{
					"",
					"",
					"",
					"",
					"",
					"",
					"",
					""
				};
				TextLine2 = new string[8]
				{
					"",
					"",
					"",
					"",
					"",
					"",
					"",
					""
				};
				Emote1 = new int[8];
				Emote2 = new int[8];
				Emote3 = new int[8];
				EmoteDelay1 = new int[8];
				EmoteDelay2 = new int[8];
				EmoteDelay3 = new int[8];
				TextID = _TextID;
				DataTable MySQLQuery = new DataTable();
				WorldServiceLocator._WorldServer.WorldDatabase.Query($"SELECT * FROM npc_text WHERE ID = {TextID};", ref MySQLQuery);
				checked
				{
					if (MySQLQuery.Rows.Count > 0)
					{
						int i = 0;
						do
						{
							Probability[i] = Conversions.ToSingle(MySQLQuery.Rows[0][("prob" + Conversions.ToString(i)) ?? ""]);
							if (!Information.IsDBNull(RuntimeHelpers.GetObjectValue(MySQLQuery.Rows[0]["text" + Conversions.ToString(i) + "_0"])))
							{
								TextLine1[i] = Conversions.ToString(MySQLQuery.Rows[0]["text" + Conversions.ToString(i) + "_0"]);
							}
							if (!Information.IsDBNull(RuntimeHelpers.GetObjectValue(MySQLQuery.Rows[0]["text" + Conversions.ToString(i) + "_1"])))
							{
								TextLine2[i] = Conversions.ToString(MySQLQuery.Rows[0]["text" + Conversions.ToString(i) + "_1"]);
							}
							if (!Information.IsDBNull(RuntimeHelpers.GetObjectValue(MySQLQuery.Rows[0][("lang" + Conversions.ToString(i)) ?? ""])))
							{
								Language[i] = Conversions.ToInteger(MySQLQuery.Rows[0][("lang" + Conversions.ToString(i)) ?? ""]);
							}
							if (!Information.IsDBNull(RuntimeHelpers.GetObjectValue(MySQLQuery.Rows[0]["em" + Conversions.ToString(i) + "_0_delay"])))
							{
								EmoteDelay1[i] = Conversions.ToInteger(MySQLQuery.Rows[0]["em" + Conversions.ToString(i) + "_0_delay"]);
							}
							if (!Information.IsDBNull(RuntimeHelpers.GetObjectValue(MySQLQuery.Rows[0]["em" + Conversions.ToString(i) + "_0"])))
							{
								Emote1[i] = Conversions.ToInteger(MySQLQuery.Rows[0]["em" + Conversions.ToString(i) + "_0"]);
							}
							if (!Information.IsDBNull(RuntimeHelpers.GetObjectValue(MySQLQuery.Rows[0]["em" + Conversions.ToString(i) + "_1_delay"])))
							{
								EmoteDelay2[i] = Conversions.ToInteger(MySQLQuery.Rows[0]["em" + Conversions.ToString(i) + "_1_delay"]);
							}
							if (!Information.IsDBNull(RuntimeHelpers.GetObjectValue(MySQLQuery.Rows[0]["em" + Conversions.ToString(i) + "_1"])))
							{
								Emote2[i] = Conversions.ToInteger(MySQLQuery.Rows[0]["em" + Conversions.ToString(i) + "_1"]);
							}
							if (!Information.IsDBNull(RuntimeHelpers.GetObjectValue(MySQLQuery.Rows[0]["em" + Conversions.ToString(i) + "_2_delay"])))
							{
								EmoteDelay3[i] = Conversions.ToInteger(MySQLQuery.Rows[0]["em" + Conversions.ToString(i) + "_2_delay"]);
							}
							if (!Information.IsDBNull(RuntimeHelpers.GetObjectValue(MySQLQuery.Rows[0]["em" + Conversions.ToString(i) + "_2"])))
							{
								Emote3[i] = Conversions.ToInteger(MySQLQuery.Rows[0]["em" + Conversions.ToString(i) + "_2"]);
							}
							if (Operators.CompareString(TextLine1[i], "", TextCompare: false) != 0)
							{
								Count = (byte)(unchecked((int)checked((byte)i)) + 1);
							}
							i++;
						}
						while (i <= 7);
					}
					else
					{
						Probability[0] = 1f;
						TextLine1[0] = "Hey there, $N. How can I help you?";
						TextLine2[0] = TextLine1[0];
						Count = 0;
					}
					WorldServiceLocator._WS_Creatures.NPCTexts.Add(TextID, this);
				}
			}

			public NPCText(int _TextID, string TextLine)
			{
				Count = 1;
				TextID = 0;
				Probability = new float[8];
				Language = new int[8];
				TextLine1 = new string[8]
				{
					"",
					"",
					"",
					"",
					"",
					"",
					"",
					""
				};
				TextLine2 = new string[8]
				{
					"",
					"",
					"",
					"",
					"",
					"",
					"",
					""
				};
				Emote1 = new int[8];
				Emote2 = new int[8];
				Emote3 = new int[8];
				EmoteDelay1 = new int[8];
				EmoteDelay2 = new int[8];
				EmoteDelay3 = new int[8];
				TextID = _TextID;
				TextLine1[0] = TextLine;
				TextLine2[0] = TextLine;
				Count = 0;
				WorldServiceLocator._WS_Creatures.NPCTexts.Add(TextID, this);
			}
		}

		public const int SKILL_DETECTION_PER_LEVEL = 5;

		public int[] CorpseDecay;

		public Dictionary<int, NPCText> NPCTexts;

		public WS_Creatures()
		{
			CorpseDecay = new int[5]
			{
				30,
				150,
				150,
				150,
				1800
			};
			NPCTexts = new Dictionary<int, NPCText>();
		}

		public void On_CMSG_CREATURE_QUERY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
		{
			checked
			{
				if (packet.Data.Length - 1 < 17)
				{
					return;
				}
				Packets.PacketClass response = new Packets.PacketClass(OPCODES.SMSG_CREATURE_QUERY_RESPONSE);
				packet.GetInt16();
				int CreatureID = packet.GetInt32();
				ulong CreatureGUID = packet.GetUInt64();
				try
				{
					if (!WorldServiceLocator._WorldServer.CREATURESDatabase.ContainsKey(CreatureID))
					{
						WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CREATURE_QUERY [Creature {2} not loaded.]", client.IP, client.Port, CreatureID);
						response.AddUInt32((uint)(CreatureID | int.MinValue));
						client.Send(ref response);
						response.Dispose();
						return;
					}
					CreatureInfo Creature = WorldServiceLocator._WorldServer.CREATURESDatabase[CreatureID];
					response.AddInt32(Creature.Id);
					response.AddString(Creature.Name);
					response.AddInt8(0);
					response.AddInt8(0);
					response.AddInt8(0);
					response.AddString(Creature.SubName);
					response.AddInt32((int)Creature.TypeFlags);
					response.AddInt32(Creature.CreatureType);
					response.AddInt32(Creature.CreatureFamily);
					response.AddInt32(Creature.Elite);
					response.AddInt32(0);
					response.AddInt32(Creature.PetSpellDataID);
					response.AddInt32(Creature.ModelA1);
					response.AddInt32(Creature.ModelA2);
					response.AddInt32(Creature.ModelH1);
					response.AddInt32(Creature.ModelH2);
					response.AddSingle(1f);
					response.AddSingle(1f);
					response.AddInt8(Creature.Leader);
					client.Send(ref response);
					response.Dispose();
				}
				catch (Exception ex2)
				{
					ProjectData.SetProjectError(ex2);
					Exception ex = ex2;
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Unknown Error: Unable to find CreatureID={0} in database. {1}", CreatureID, ex.Message);
					ProjectData.ClearProjectError();
				}
			}
		}

		public void On_CMSG_NPC_TEXT_QUERY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
		{
			checked
			{
				if (packet.Data.Length - 1 >= 17)
				{
					packet.GetInt16();
					long TextID = packet.GetInt32();
					ulong TargetGUID = packet.GetUInt64();
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_NPC_TEXT_QUERY [TextID={2}]", client.IP, client.Port, TextID);
					client.Character.SendTalking((int)TextID);
				}
			}
		}

		public void On_CMSG_GOSSIP_HELLO(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
		{
			if (checked(packet.Data.Length - 1) < 13)
			{
				return;
			}
			packet.GetInt16();
			ulong GUID = packet.GetUInt64();
			WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GOSSIP_HELLO [GUID={2:X}]", client.IP, client.Port, GUID);
			if (!WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(GUID) || WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].CreatureInfo.cNpcFlags == 0)
			{
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "[{0}:{1}] Client tried to speak with a creature that didn't exist or couldn't interact with. [GUID={2:X}  ID={3}]", client.IP, client.Port, GUID, WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].ID);
			}
			else
			{
				if (WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].Evade)
				{
					return;
				}
				WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].StopMoving();
				client.Character.RemoveAurasByInterruptFlag(1024);
				try
				{
					if (WorldServiceLocator._WorldServer.CREATURESDatabase[WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].ID].TalkScript == null)
					{
						Packets.PacketClass test = new Packets.PacketClass(OPCODES.SMSG_NPC_WONT_TALK);
						test.AddUInt64(GUID);
						test.AddInt8(1);
						client.Send(ref test);
						test.Dispose();
						if (!NPCTexts.ContainsKey(34))
						{
							NPCText tmpText = new NPCText(34, "Hi $N, I'm not yet scripted to talk with you.");
						}
						client.Character.SendTalking(34);
						WS_PlayerData.CharacterObject character = client.Character;
						GossipMenu Menu = null;
						QuestMenu qMenu = null;
						character.SendGossip(GUID, 34, Menu, qMenu);
					}
					else
					{
						WorldServiceLocator._WorldServer.CREATURESDatabase[WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].ID].TalkScript.OnGossipHello(ref client.Character, GUID);
					}
				}
				catch (Exception ex2)
				{
					ProjectData.SetProjectError(ex2);
					Exception ex = ex2;
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error in gossip hello.{0}{1}", Environment.NewLine, ex.ToString());
					ProjectData.ClearProjectError();
				}
			}
		}

		public void On_CMSG_GOSSIP_SELECT_OPTION(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
		{
			if (checked(packet.Data.Length - 1) < 17)
			{
				return;
			}
			packet.GetInt16();
			ulong GUID = packet.GetUInt64();
			int SelOption = packet.GetInt32();
			WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GOSSIP_SELECT_OPTION [SelOption={3} GUID={2:X}]", client.IP, client.Port, GUID, SelOption);
			if (!WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(GUID) || WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].CreatureInfo.cNpcFlags == 0)
			{
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "[{0}:{1}] Client tried to speak with a creature that didn't exist or couldn't interact with. [GUID={2:X}  ID={3}]", client.IP, client.Port, GUID, WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].ID);
			}
			else
			{
				if (WorldServiceLocator._WorldServer.CREATURESDatabase[WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].ID].TalkScript == null)
				{
					throw new ApplicationException("Invoked OnGossipSelect() on creature without initialized TalkScript!");
				}
				WorldServiceLocator._WorldServer.CREATURESDatabase[WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].ID].TalkScript.OnGossipSelect(ref client.Character, GUID, SelOption);
			}
		}

		public void On_CMSG_SPIRIT_HEALER_ACTIVATE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
		{
			checked
			{
				if (packet.Data.Length - 1 < 13)
				{
					return;
				}
				packet.GetInt16();
				ulong GUID = packet.GetUInt64();
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SPIRIT_HEALER_ACTIVATE [GUID={2}]", client.IP, client.Port, GUID);
				try
				{
					byte i = 0;
					do
					{
						if (client.Character.Items.ContainsKey(i))
						{
							client.Character.Items[i].ModifyDurability(0.25f, ref client);
						}
						i = (byte)unchecked((uint)(i + 1));
					}
					while (unchecked((uint)i) <= 18u);
				}
				catch (Exception ex2)
				{
					ProjectData.SetProjectError(ex2);
					Exception ex = ex2;
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error activating spirit healer: {0}", ex.ToString());
					ProjectData.ClearProjectError();
				}
				WorldServiceLocator._WS_Handlers_Misc.CharacterResurrect(ref client.Character);
				client.Character.ApplySpell(15007);
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		private ulong GetNewGUID()
		{
			ref ulong creatureGUIDCounter = ref WorldServiceLocator._WorldServer.CreatureGUIDCounter;
			creatureGUIDCounter = Convert.ToUInt64(decimal.Add(new decimal(creatureGUIDCounter), 1m));
			return WorldServiceLocator._WorldServer.CreatureGUIDCounter;
		}
	}
}
