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
using System.Collections.Generic;
using Mangos.Common.Enums.Chat;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Misc;
using Mangos.World.Handlers;
using Mangos.World.Maps;
using Mangos.World.Objects;
using Mangos.World.Player;
using Mangos.World.Server;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.AI
{
    public partial class WS_Creatures_AI
	{
        public class DefaultAI : TBaseAI
		{
			protected WS_Creatures.CreatureObject aiCreature;

			protected int aiTimer;

			protected int nextAttack;

			protected bool ignoreLoot;

			protected bool AllowedAttack;

			protected bool AllowedMove;

			protected bool IsWaypoint;

			protected float ResetX;

			protected float ResetY;

			protected float ResetZ;

			protected float ResetO;

			protected bool ResetRun;

			protected bool ResetFinished;

			protected float LastHitX;

			protected float LastHitY;

			protected float LastHitZ;

			protected const int AI_INTERVAL_MOVE = 3000;

			protected const int AI_INTERVAL_SLEEP = 6000;

			protected const int AI_INTERVAL_DEAD = 60000;

			protected const float PIx2 = (float)Math.PI * 2f;

			public DefaultAI(ref WS_Creatures.CreatureObject Creature)
			{
				aiCreature = null;
				aiTimer = 0;
				nextAttack = 0;
				ignoreLoot = false;
				AllowedAttack = true;
				AllowedMove = true;
				IsWaypoint = false;
				ResetX = 0f;
				ResetY = 0f;
				ResetZ = 0f;
				ResetO = 0f;
				ResetRun = true;
				ResetFinished = false;
				LastHitX = 0f;
				LastHitY = 0f;
				LastHitZ = 0f;
				State = AIState.AI_WANDERING;
				aiCreature = Creature ?? throw new ArgumentNullException(nameof(Creature));
				aiTarget = null;
			}

			public override bool IsMoving()
			{
				if (checked(WorldServiceLocator._NativeMethods.timeGetTime("") - aiCreature.LastMove) < aiTimer)
				{
					return State switch
					{
						AIState.AI_MOVE_FOR_ATTACK => true, 
						AIState.AI_MOVING => true, 
						AIState.AI_WANDERING => true, 
						_ => false, 
					};
				}
				return false;
			}

			public override void Pause(int Time)
			{
				aiTimer = Time;
			}

			public override void OnEnterCombat()
			{
				if (!aiCreature.IsDead)
				{
					aiCreature.SetToRealPosition();
					ResetX = aiCreature.positionX;
					ResetY = aiCreature.positionY;
					ResetZ = aiCreature.positionZ;
					ResetO = aiCreature.orientation;
					State = AIState.AI_ATTACKING;
					DoThink();
				}
			}

			public override void OnLeaveCombat(bool Reset = true)
			{
				foreach (KeyValuePair<WS_Base.BaseUnit, int> Victim in aiHateTable)
				{
					if (Victim.Key is WS_PlayerData.CharacterObject)
					{
						((WS_PlayerData.CharacterObject)Victim.Key).RemoveFromCombat(aiCreature);
					}
				}
				if (aiCreature.DestroyAtNoCombat)
				{
					aiCreature.Destroy();
					return;
				}
				aiTarget = null;
				aiHateTable.Clear();
				aiHateTableRemove.Clear();
				aiCreature.SendTargetUpdate(0uL);
				if (Reset)
				{
					State = AIState.AI_MOVING_TO_SPAWN;
					aiCreature.Life.Current = aiCreature.Life.Maximum;
					WS_Creatures.CreatureObject creatureObject = aiCreature;
					WS_Base.BaseUnit Attacker = null;
					creatureObject.Heal(0, Attacker);
					if (ResetX == 0f && ResetY == 0f && ResetZ == 0f)
					{
						ResetX = aiCreature.SpawnX;
						ResetY = aiCreature.SpawnY;
						ResetZ = aiCreature.SpawnZ;
						ResetO = aiCreature.SpawnO;
					}
					DoMoveReset();
				}
			}

			public override void OnGenerateHate(ref WS_Base.BaseUnit Attacker, int HateValue)
			{
                if (Attacker is null)
                {
                    throw new ArgumentNullException(nameof(Attacker));
                }

                checked
				{
					if (Attacker != aiCreature && State != AIState.AI_DEAD && State != AIState.AI_RESPAWN && State != AIState.AI_MOVING_TO_SPAWN)
					{
						aiCreature.SetToRealPosition();
						LastHitX = aiCreature.positionX;
						LastHitY = aiCreature.positionY;
						LastHitZ = aiCreature.positionZ;
						if (Attacker is WS_PlayerData.CharacterObject)
						{
							((WS_PlayerData.CharacterObject)Attacker).AddToCombat(aiCreature);
						}
						if (!InCombat())
						{
                            aiHateTable.Add(Attacker, (int)Math.Round(HateValue * Attacker.Spell_ThreatModifier));
							OnEnterCombat();
						}
						else if (!aiHateTable.ContainsKey(Attacker))
						{
                            aiHateTable.Add(Attacker, (int)Math.Round(HateValue * Attacker.Spell_ThreatModifier));
						}
						else
						{
							Dictionary<WS_Base.BaseUnit, int> aiHateTable;
							WS_Base.BaseUnit key;
							(aiHateTable = base.aiHateTable)[key = Attacker] = (int)Math.Round(aiHateTable[key] + HateValue * Attacker.Spell_ThreatModifier);
						}
					}
				}
			}

			public override void Reset()
			{
				aiTimer = 0;
				OnLeaveCombat();
			}

			protected void GoBackToSpawn()
			{
				State = AIState.AI_MOVING_TO_SPAWN;
				ResetX = aiCreature.SpawnX;
				ResetY = aiCreature.SpawnY;
				ResetZ = aiCreature.SpawnZ;
				ResetO = aiCreature.SpawnO;
				ResetRun = false;
				Reset();
			}

			protected void SelectTarget()
			{
				try
				{
					int max = -1;
					WS_Base.BaseUnit tmpTarget = null;
					foreach (KeyValuePair<WS_Base.BaseUnit, int> Victim in aiHateTable)
					{
						if (Victim.Key.IsDead)
						{
							aiHateTableRemove.Add(Victim.Key);
							if (Victim.Key is WS_PlayerData.CharacterObject)
							{
								((WS_PlayerData.CharacterObject)Victim.Key).RemoveFromCombat(aiCreature);
							}
						}
						else if (Victim.Value > max)
						{
							max = Victim.Value;
							tmpTarget = Victim.Key;
						}
					}
					foreach (WS_Base.BaseUnit VictimRemove in aiHateTableRemove)
					{
						aiHateTable.Remove(VictimRemove);
					}
					if (tmpTarget != null && aiTarget != tmpTarget)
					{
						aiTarget = tmpTarget;
						aiCreature.TurnTo(aiTarget.positionX, aiTarget.positionY);
						aiCreature.SendTargetUpdate(tmpTarget.GUID);
						State = AIState.AI_ATTACKING;
					}
				}
				catch (Exception ex2)
				{
					ProjectData.SetProjectError(ex2);
					Exception ex = ex2;
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error selecting target.{0}{1}", Environment.NewLine, ex.ToString());
					Reset();
					ProjectData.ClearProjectError();
				}
				if (aiTarget == null)
				{
					Reset();
				}
			}

			protected bool CheckTarget()
			{
				if (aiTarget == null)
				{
					Reset();
					return true;
				}
				return false;
			}

			public override void DoThink()
			{
				if (aiCreature == null)
				{
					return;
				}
				checked
				{
					if (aiTimer > 1000)
					{
						aiTimer -= 1000;
						return;
					}
					aiTimer = 0;
					if (ResetFinished)
					{
						aiCreature.positionX = aiCreature.MoveX;
						aiCreature.positionY = aiCreature.MoveY;
						aiCreature.positionZ = aiCreature.MoveZ;
						aiCreature.PositionUpdated = true;
						ResetFinished = false;
						ResetRun = true;
						State = AIState.AI_WANDERING;
						aiCreature.orientation = ResetO;
					}
					if (State != AIState.AI_DEAD && State != AIState.AI_RESPAWN && aiCreature.Life.Current == 0)
					{
						State = AIState.AI_DEAD;
					}
					if (aiCreature.IsStunned)
					{
						aiTimer = 1000;
						return;
					}
					if (aiTarget == null)
					{
					}
					switch (State)
					{
					case AIState.AI_DEAD:
					{
						if (aiHateTable.Count > 0)
						{
							OnLeaveCombat(Reset: false);
							aiTimer = WorldServiceLocator._WS_Creatures.CorpseDecay[aiCreature.CreatureInfo.Elite] * 1000;
							ignoreLoot = false;
							break;
						}
						if (!ignoreLoot && WorldServiceLocator._WS_Loot.LootTable.ContainsKey(aiCreature.GUID))
						{
							aiTimer = WorldServiceLocator._WS_Creatures.CorpseDecay[aiCreature.CreatureInfo.Elite] * 1000;
							ignoreLoot = true;
							break;
						}
						State = AIState.AI_RESPAWN;
						int RespawnTime = aiCreature.SpawnTime;
						if (RespawnTime > 0)
						{
							aiTimer = RespawnTime * 1000;
							aiCreature.Despawn();
						}
						else
						{
							aiCreature.Destroy();
						}
						break;
					}
					case AIState.AI_RESPAWN:
						State = AIState.AI_WANDERING;
						aiCreature.Respawn();
						aiTimer = 10000;
						break;
					case AIState.AI_MOVE_FOR_ATTACK:
						DoMove();
						break;
					case AIState.AI_WANDERING:
						if (!AllowedMove)
						{
							State = AIState.AI_DO_NOTHING;
						}
						else if (IsWaypoint || WorldServiceLocator._WorldServer.Rnd.NextDouble() > 0.20000000298023224)
						{
							DoMove();
						}
						break;
					case AIState.AI_MOVING_TO_SPAWN:
						DoMoveReset();
						break;
					case AIState.AI_ATTACKING:
						if (!AllowedAttack)
						{
							State = AIState.AI_DO_NOTHING;
						}
						else
						{
							DoAttack();
						}
						break;
					case AIState.AI_MOVING:
						if (!AllowedMove)
						{
							State = AIState.AI_DO_NOTHING;
						}
						else
						{
							DoMove();
						}
						break;
					case AIState.AI_DO_NOTHING:
						break;
					default:
						aiCreature.SendChatMessage("Unknown AI mode!", ChatMsg.CHAT_MSG_MONSTER_SAY, LANGUAGES.LANG_GLOBAL, 0uL);
						State = AIState.AI_DO_NOTHING;
						break;
					}
				}
			}

			protected void DoAttack()
			{
				if (!AllowedAttack)
				{
					State = AIState.AI_DO_NOTHING;
					aiTimer = 3000;
					return;
				}
				if (aiCreature.Spell_Pacifyed)
				{
					aiTimer = 1000;
					return;
				}
				SelectTarget();
				if (State != AIState.AI_ATTACKING)
				{
					aiTimer = 6000;
					return;
				}
				checked
				{
					try
					{
						if (aiTarget != null && aiTarget.IsDead)
						{
							aiHateTable.Remove(aiTarget);
                            aiTarget = null;
							SelectTarget();
						}
						if (CheckTarget())
						{
							return;
						}
						float distance = WorldServiceLocator._WS_Combat.GetDistance(aiCreature, aiTarget);
						if (distance > 2f + aiCreature.CombatReach + aiTarget.BoundingRadius)
						{
							State = AIState.AI_MOVE_FOR_ATTACK;
							DoMove();
							return;
						}
						nextAttack -= 1000;
						if (nextAttack <= 0)
						{
							nextAttack = 0;
							WS_Combat wS_Combat = WorldServiceLocator._WS_Combat;
							ref WS_Creatures.CreatureObject reference = ref aiCreature;
							ref WS_Creatures.CreatureObject reference2 = ref reference;
							WS_Base.BaseObject Object = reference;
							ref WS_Base.BaseUnit aiTarget = ref base.aiTarget;
							ref WS_Base.BaseUnit reference3 = ref aiTarget;
							WS_Base.BaseObject Object2 = aiTarget;
							bool flag = wS_Combat.IsInFrontOf(ref Object, ref Object2);
							reference3 = (WS_Base.BaseUnit)Object2;
							reference2 = (WS_Creatures.CreatureObject)Object;
							if (!flag)
							{
								WS_Creatures.CreatureObject creatureObject = aiCreature;
								ref WS_Base.BaseUnit aiTarget2 = ref base.aiTarget;
								reference3 = ref aiTarget2;
								Object2 = aiTarget2;
								creatureObject.TurnTo(ref Object2);
								reference3 = (WS_Base.BaseUnit)Object2;
							}
							WS_Combat wS_Combat2 = WorldServiceLocator._WS_Combat;
							ref WS_Creatures.CreatureObject reference4 = ref aiCreature;
							reference2 = ref reference4;
							WS_Base.BaseUnit Attacker = reference4;
							WS_Combat.DamageInfo damageInfo2 = wS_Combat2.CalculateDamage(ref Attacker, ref base.aiTarget, DualWield: false, Ranged: false);
							reference2 = (WS_Creatures.CreatureObject)Attacker;
							WS_Combat.DamageInfo damageInfo = damageInfo2;
							WS_Combat wS_Combat3 = WorldServiceLocator._WS_Combat;
							ref WS_Creatures.CreatureObject reference5 = ref aiCreature;
							reference2 = ref reference5;
							Object2 = reference5;
							ref WS_Base.BaseUnit aiTarget3 = ref base.aiTarget;
							reference3 = ref aiTarget3;
							Object = aiTarget3;
							WS_Combat.DamageInfo damageInfo3 = damageInfo;
							WS_Network.ClientClass client = null;
							wS_Combat3.SendAttackerStateUpdate(ref Object2, ref Object, damageInfo3, client);
							reference3 = (WS_Base.BaseUnit)Object;
							reference2 = (WS_Creatures.CreatureObject)Object2;
							WS_Base.BaseUnit aiTarget4 = base.aiTarget;
							int getDamage = damageInfo.GetDamage;
							ref WS_Creatures.CreatureObject reference6 = ref aiCreature;
							reference2 = ref reference6;
							Attacker = reference6;
							aiTarget4.DealDamage(getDamage, Attacker);
							reference2 = (WS_Creatures.CreatureObject)Attacker;
							nextAttack = WorldServiceLocator._WorldServer.CREATURESDatabase[aiCreature.ID].BaseAttackTime;
							aiTimer = 1000;
						}
					}
					catch (Exception ex2)
					{
						ProjectData.SetProjectError(ex2);
						Exception ex = ex2;
						WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "WS_Creatures:DoAttack failed - Guid: {1} ID: {2}  {0}", ex.Message);
						Reset();
						ProjectData.ClearProjectError();
					}
				}
			}

			public override void DoMove()
			{
				if (aiTarget == null)
				{
					float distanceToSpawn = WorldServiceLocator._WS_Combat.GetDistance(aiCreature.positionX, aiCreature.SpawnX, aiCreature.positionY, aiCreature.SpawnY, aiCreature.positionZ, aiCreature.SpawnZ);
					if (!IsWaypoint && aiCreature.SpawnID > 0 && distanceToSpawn > aiCreature.MaxDistance)
					{
						GoBackToSpawn();
						return;
					}
				}
				else
				{
					float distanceToLastHit = WorldServiceLocator._WS_Combat.GetDistance(aiCreature.positionX, LastHitX, aiCreature.positionY, LastHitY, aiCreature.positionZ, LastHitZ);
					if (distanceToLastHit > aiCreature.MaxDistance)
					{
						OnLeaveCombat();
						return;
					}
				}
				if (aiCreature.IsRooted)
				{
					aiTimer = 1000;
					return;
				}
				if (aiTarget == null)
				{
					int MoveTries = 0;
					float selectedX2;
					float selectedY2;
					float selectedZ2;
					while (true)
					{
						if (MoveTries > 5)
						{
							GoBackToSpawn();
							return;
						}
						float distance2 = (float)(3.0 * aiCreature.CreatureInfo.WalkSpeed);
						float angle2 = (float)(WorldServiceLocator._WorldServer.Rnd.NextDouble() * 6.2831854820251465);
						aiCreature.SetToRealPosition();
						aiCreature.orientation = angle2;
						selectedX2 = (float)(aiCreature.positionX + Math.Cos(angle2) * distance2);
						selectedY2 = (float)(aiCreature.positionY + Math.Sin(angle2) * distance2);
						selectedZ2 = WorldServiceLocator._WS_Maps.GetZCoord(selectedX2, selectedY2, aiCreature.positionZ, aiCreature.MapID);
						MoveTries = checked(MoveTries + 1);
						if (!(Math.Abs(aiCreature.positionZ - selectedZ2) > 5f))
						{
							WS_Maps wS_Maps = WorldServiceLocator._WS_Maps;
							ref WS_Creatures.CreatureObject reference = ref aiCreature;
							WS_Base.BaseObject obj = reference;
							bool flag = wS_Maps.IsInLineOfSight(ref obj, selectedX2, selectedY2, selectedZ2 + 1f);
							reference = (WS_Creatures.CreatureObject)obj;
							if (flag)
							{
								break;
							}
						}
					}
					if (aiCreature.CanMoveTo(selectedX2, selectedY2, selectedZ2))
					{
						State = AIState.AI_WANDERING;
						aiTimer = aiCreature.MoveTo(selectedX2, selectedY2, selectedZ2);
					}
					else
					{
						aiTimer = 3000;
					}
					return;
				}
				SelectTarget();
				if (CheckTarget())
				{
					return;
				}
				aiCreature.SetToRealPosition();
				if (aiTarget is WS_Creatures.CreatureObject)
				{
					((WS_Creatures.CreatureObject)aiTarget).SetToRealPosition();
				}
				float distance = 1000f * aiCreature.CreatureInfo.RunSpeed;
				float distanceToTarget = WorldServiceLocator._WS_Combat.GetDistance(aiCreature, aiTarget);
				if (distanceToTarget < distance)
				{
					State = AIState.AI_ATTACKING;
					float destDist = 2f + aiCreature.CombatReach + aiTarget.BoundingRadius;
					if (distanceToTarget <= destDist)
					{
						DoAttack();
					}
					destDist *= 0.5f;
					float NearX = aiTarget.positionX;
					NearX = ((!(aiTarget.positionX > aiCreature.positionX)) ? (NearX + destDist) : (NearX - destDist));
					float NearY = aiTarget.positionY;
					NearY = ((!(aiTarget.positionY > aiCreature.positionY)) ? (NearY + destDist) : (NearY - destDist));
					float NearZ = WorldServiceLocator._WS_Maps.GetZCoord(NearX, NearY, aiCreature.positionZ, aiCreature.MapID);
					if ((NearZ > aiTarget.positionZ + 2f) | (NearZ < aiTarget.positionZ - 2f))
					{
						NearZ = aiTarget.positionZ;
					}
					if (aiCreature.CanMoveTo(NearX, NearY, NearZ))
					{
						aiCreature.orientation = WorldServiceLocator._WS_Combat.GetOrientation(aiCreature.positionX, NearX, aiCreature.positionY, NearY);
						aiTimer = aiCreature.MoveTo(NearX, NearY, NearZ, 0f, Running: true);
						return;
					}
					aiHateTable.Remove(aiTarget);
					if (aiTarget is WS_PlayerData.CharacterObject)
					{
						((WS_PlayerData.CharacterObject)aiTarget).RemoveFromCombat(aiCreature);
					}
					SelectTarget();
					CheckTarget();
					return;
				}
				State = AIState.AI_MOVE_FOR_ATTACK;
				float angle = WorldServiceLocator._WS_Combat.GetOrientation(aiCreature.positionX, aiTarget.positionX, aiCreature.positionY, aiTarget.positionY);
				aiCreature.orientation = angle;
				float selectedX = (float)(aiCreature.positionX + Math.Cos(angle) * distance);
				float selectedY = (float)(aiCreature.positionY + Math.Sin(angle) * distance);
				float selectedZ = WorldServiceLocator._WS_Maps.GetZCoord(selectedX, selectedY, aiCreature.positionZ, aiCreature.MapID);
				if (aiCreature.CanMoveTo(selectedX, selectedY, selectedZ))
				{
					aiTimer = aiCreature.MoveTo(selectedX, selectedY, selectedZ, 0f, Running: true);
					return;
				}
				aiHateTable.Remove(aiTarget);
				if (aiTarget is WS_PlayerData.CharacterObject)
				{
					((WS_PlayerData.CharacterObject)aiTarget).RemoveFromCombat(aiCreature);
				}
				SelectTarget();
				CheckTarget();
			}

			protected void DoMoveReset()
			{
				float distance = ((!ResetRun) ? ((float)(3.0 * aiCreature.CreatureInfo.WalkSpeed)) : ((float)(3.0 * aiCreature.CreatureInfo.RunSpeed)));
				aiCreature.SetToRealPosition(Forced: true);
				float angle = WorldServiceLocator._WS_Combat.GetOrientation(aiCreature.positionX, ResetX, aiCreature.positionY, ResetY);
				aiCreature.orientation = angle;
				float tmpDist = WorldServiceLocator._WS_Combat.GetDistance(aiCreature, ResetX, ResetY, ResetZ);
				if (tmpDist < distance)
				{
					aiTimer = aiCreature.MoveTo(ResetX, ResetY, ResetZ, ResetO, ResetRun);
					ResetFinished = true;
					return;
				}
				float selectedX = (float)(aiCreature.positionX + Math.Cos(angle) * distance);
				float selectedY = (float)(aiCreature.positionY + Math.Sin(angle) * distance);
				float selectedZ = WorldServiceLocator._WS_Maps.GetZCoord(selectedX, selectedY, aiCreature.positionZ, aiCreature.MapID);
				aiTimer = checked(aiCreature.MoveTo(selectedX, selectedY, selectedZ, 0f, ResetRun) - 50);
			}
		}
    }
}
