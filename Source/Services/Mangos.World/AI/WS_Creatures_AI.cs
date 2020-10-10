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
using System.Threading;
using Mangos.Common.Enums.Chat;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Misc;
using Mangos.World.Objects;
using Mangos.World.Player;
using Mangos.World.Server;

namespace Mangos.World.AI
{
    public class WS_Creatures_AI
    {

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public class TBaseAI : IDisposable
        {
            public AIState State = AIState.AI_DO_NOTHING;
            public WS_Base.BaseUnit aiTarget = null;
            public Dictionary<WS_Base.BaseUnit, int> aiHateTable = new Dictionary<WS_Base.BaseUnit, int>();
            public List<WS_Base.BaseUnit> aiHateTableRemove = new List<WS_Base.BaseUnit>();

            public virtual bool InCombat()
            {
                return aiHateTable.Count > 0;
            }

            public void ResetThreatTable()
            {
                var tmpUnits = new List<WS_Base.BaseUnit>();
                foreach (KeyValuePair<WS_Base.BaseUnit, int> Victim in aiHateTable)
                    tmpUnits.Add(Victim.Key);
                aiHateTable.Clear();
                foreach (WS_Base.BaseUnit Victim in tmpUnits)
                    aiHateTable.Add(Victim, 0);
            }

            public virtual bool IsMoving()
            {
                switch (State)
                {
                    case var @case when @case == AIState.AI_ATTACKING:
                    case var case1 when case1 == AIState.AI_MOVE_FOR_ATTACK:
                    case var case2 when case2 == AIState.AI_MOVING:
                    case var case3 when case3 == AIState.AI_WANDERING:
                    case var case4 when case4 == AIState.AI_MOVING_TO_SPAWN:
                        {
                            return true;
                        }

                    default:
                        {
                            return false;
                        }
                }
            }

            public virtual bool IsRunning()
            {
                return State == AIState.AI_MOVE_FOR_ATTACK;
            }

            public virtual void Reset()
            {
                State = AIState.AI_DO_NOTHING;
            }

            public virtual void Pause(int Time)
            {
            }

            public virtual void OnEnterCombat()
            {
            }

            public virtual void OnLeaveCombat(bool Reset = true)
            {
            }

            public virtual void OnGenerateHate(ref WS_Base.BaseUnit Attacker, int HateValue)
            {
            }

            public virtual void OnKill(ref WS_Base.BaseUnit Victim)
            {
            }

            public virtual void OnHealthChange(int Percent)
            {
            }

            public virtual void OnDeath()
            {
            }

            public virtual void DoThink()
            {
            }

            public virtual void DoMove()
            {
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
            public TBaseAI()
            {
            }
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        // TODO: Base Escort AI for escort units

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public class DefaultAI : TBaseAI
        {
            protected WS_Creatures.CreatureObject aiCreature = null;
            protected int aiTimer = 0;
            protected int nextAttack = 0;
            protected bool ignoreLoot = false;
            protected bool AllowedAttack = true;
            protected bool AllowedMove = true;
            protected bool IsWaypoint = false;
            protected float ResetX = 0.0f;
            protected float ResetY = 0.0f;
            protected float ResetZ = 0.0f;
            protected float ResetO = 0.0f;
            protected bool ResetRun = true;
            protected bool ResetFinished = false;
            protected float LastHitX = 0.0f;
            protected float LastHitY = 0.0f;
            protected float LastHitZ = 0.0f;
            protected const int AI_INTERVAL_MOVE = 3000;
            protected const int AI_INTERVAL_SLEEP = 6000;
            protected const int AI_INTERVAL_DEAD = 60000;
            protected const double PIx2 = 2d * Math.PI;

            public DefaultAI(ref WS_Creatures.CreatureObject Creature)
            {
                State = AIState.AI_WANDERING;
                aiCreature = Creature;
                aiTarget = null;
            }

            public override bool IsMoving()
            {
                if (WorldServiceLocator._NativeMethods.timeGetTime("") - aiCreature.LastMove < aiTimer)
                {
                    switch (State)
                    {
                        case var @case when @case == AIState.AI_MOVE_FOR_ATTACK:
                            {
                                return true;
                            }

                        case var case1 when case1 == AIState.AI_MOVING:
                            {
                                return true;
                            }

                        case var case2 when case2 == AIState.AI_WANDERING:
                            {
                                return true;
                            }

                        default:
                            {
                                return false;
                            }
                    }
                }
                else
                {
                    return false;
                }
            }

            public override void Pause(int Time)
            {
                aiTimer = Time;
            }

            public override void OnEnterCombat()
            {
                if (aiCreature.IsDead)
                    return; // Prevents the creature from doing this below if it's dead already
                // DONE: Decide it's real position if it hasn't stopped
                aiCreature.SetToRealPosition();
                ResetX = aiCreature.positionX;
                ResetY = aiCreature.positionY;
                ResetZ = aiCreature.positionZ;
                ResetO = aiCreature.orientation;
                State = AIState.AI_ATTACKING;
                DoThink();

                // If MonsterSayCombat.ContainsKey(aiCreature.ID) Then
                // Dim Chance As Integer = (MonsterSayCombat(aiCreature.ID).Chance)
                // If Rnd.Next(1, 101) <= Chance Then
                // Dim TargetGUID As ULong = 0UL
                // If Not aiTarget Is Nothing Then TargetGUID = aiTarget.GUID
                // aiCreature.SendChatMessage(SelectMonsterSay(aiCreature.ID), ChatMsg.CHAT_MSG_MONSTER_SAY, LANGUAGES.LANG_UNIVERSAL, TargetGUID)
                // End If
                // End If
            }

            public override void OnLeaveCombat(bool Reset = true)
            {
                // DONE: Remove combat flag from everyone
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
                aiCreature.SendTargetUpdate(0UL);
                if (Reset)
                {
                    // DONE: Reset values and move to spawn
                    // TODO: Evade
                    // TODO: Remove all buffs & debuffs
                    State = AIState.AI_MOVING_TO_SPAWN;
                    aiCreature.Life.Current = aiCreature.Life.Maximum;
                    WS_Base.BaseUnit argAttacker = null;
                    aiCreature.Heal(0, Attacker: ref argAttacker); // So players get the health update
                    if (ResetX == 0.0f && ResetY == 0.0f && ResetZ == 0.0f)
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
                if (ReferenceEquals(Attacker, aiCreature))
                    return;
                if (State != AIState.AI_DEAD && State != AIState.AI_RESPAWN && State != AIState.AI_MOVING_TO_SPAWN)
                {
                    aiCreature.SetToRealPosition();
                    LastHitX = aiCreature.positionX;
                    LastHitY = aiCreature.positionY;
                    LastHitZ = aiCreature.positionZ;
                    if (Attacker is WS_PlayerData.CharacterObject)
                    {
                        ((WS_PlayerData.CharacterObject)Attacker).AddToCombat(aiCreature);
                    }

                    if (InCombat() == false)
                    {
                        aiHateTable.Add(Attacker, (int)(HateValue * Attacker.Spell_ThreatModifier));
                        OnEnterCombat();
                        return;
                    }

                    if (aiHateTable.ContainsKey(Attacker) == false)
                    {
                        aiHateTable.Add(Attacker, (int)(HateValue * Attacker.Spell_ThreatModifier));
                    }
                    else
                    {
                        aiHateTable[Attacker] = (int)(aiHateTable[Attacker] + HateValue * Attacker.Spell_ThreatModifier);
                    }
                }
            }

            public override void Reset()
            {
                aiTimer = 0;
                OnLeaveCombat(true);
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

                    // DONE: Select max hate
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

                    // Remove From aiHateTable
                    foreach (WS_Base.BaseUnit VictimRemove in aiHateTableRemove)
                        aiHateTable.Remove(VictimRemove);

                    // DONE: Set the target
                    if (tmpTarget is object && !ReferenceEquals(aiTarget, tmpTarget))
                    {
                        aiTarget = tmpTarget;
                        aiCreature.TurnTo(aiTarget.positionX, aiTarget.positionY);
                        aiCreature.SendTargetUpdate(tmpTarget.GUID);
                        State = AIState.AI_ATTACKING;
                    }
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error selecting target.{0}{1}", Environment.NewLine, ex.ToString());
                    Reset();
                }

                if (aiTarget is null)
                    Reset();
            }

            protected bool CheckTarget()
            {
                if (aiTarget is null)
                {
                    Reset();
                    return true;
                }

                return false;
            }

            public override void DoThink()
            {
                if (aiCreature is null)
                    return; // Fixes a crash
                if (aiTimer > WS_TimerBasedEvents.TAIManager.UPDATE_TIMER)
                {
                    aiTimer -= WS_TimerBasedEvents.TAIManager.UPDATE_TIMER;
                    return;
                }
                else
                {
                    aiTimer = 0;
                }

                // DONE: Creature has finished resetting
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

                // DONE: Fixes a bug where creatures attack you when they are dead
                if (State != AIState.AI_DEAD && State != AIState.AI_RESPAWN && aiCreature.Life.Current == 0)
                {
                    State = AIState.AI_DEAD;
                }

                // DONE: If stunned
                if (aiCreature.IsStunned)
                {
                    aiTimer = 1000;
                    return;
                }

                // TODO: Check if there are any players to aggro!
                if (aiTarget is null)
                {
                    // Here!
                }

                switch (State)
                {
                    case var @case when @case == AIState.AI_DEAD:
                        {
                            if (aiHateTable.Count > 0)
                            {
                                OnLeaveCombat(false);
                                aiTimer = WorldServiceLocator._WS_Creatures.CorpseDecay[aiCreature.CreatureInfo.Elite] * 1000;
                                ignoreLoot = false;
                            }
                            else if (ignoreLoot == false && WorldServiceLocator._WS_Loot.LootTable.ContainsKey(aiCreature.GUID))
                            {
                                // DONE: There's still loot, double up the decay time
                                aiTimer = WorldServiceLocator._WS_Creatures.CorpseDecay[aiCreature.CreatureInfo.Elite] * 1000;
                                ignoreLoot = true; // And make sure the corpse decay after this
                            }
                            else
                            {
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
                            }

                            break;
                        }

                    case var case1 when case1 == AIState.AI_RESPAWN:
                        {
                            State = AIState.AI_WANDERING;
                            aiCreature.Respawn();
                            aiTimer = 10000; // Wait 10 seconds before starting to react
                            break;
                        }

                    case var case2 when case2 == AIState.AI_MOVE_FOR_ATTACK:
                        {
                            DoMove();
                            break;
                        }

                    case var case3 when case3 == AIState.AI_WANDERING:
                        {
                            if (!AllowedMove)
                            {
                                State = AIState.AI_DO_NOTHING;
                                return;
                            }

                            if (IsWaypoint || WorldServiceLocator._WorldServer.Rnd.NextDouble() > 0.2d)
                            {
                                DoMove();
                            }

                            break;
                        }

                    case var case4 when case4 == AIState.AI_MOVING_TO_SPAWN:
                        {
                            DoMoveReset();
                            break;
                        }

                    case var case5 when case5 == AIState.AI_ATTACKING:
                        {
                            if (!AllowedAttack)
                            {
                                State = AIState.AI_DO_NOTHING;
                                return;
                            }

                            DoAttack();
                            break;
                        }

                    case var case6 when case6 == AIState.AI_MOVING:
                        {
                            if (!AllowedMove)
                            {
                                State = AIState.AI_DO_NOTHING;
                                return;
                            }

                            DoMove();
                            break;
                        }

                    case var case7 when case7 == AIState.AI_DO_NOTHING:
                        {
                            break;
                        }

                    default:
                        {
                            aiCreature.SendChatMessage("Unknown AI mode!", ChatMsg.CHAT_MSG_MONSTER_SAY, LANGUAGES.LANG_UNIVERSAL);
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
                    aiTimer = AI_INTERVAL_MOVE;
                    return;
                }

                if (aiCreature.Spell_Pacifyed)
                {
                    aiTimer = 1000;
                    return;
                }

                // DONE: Change the target to the one with most threat
                SelectTarget();
                if (State != AIState.AI_ATTACKING)
                {
                    // DONE: Seems like we lost our target
                    aiTimer = AI_INTERVAL_SLEEP;
                }
                else
                {
                    // DONE: Do real melee attacks
                    try
                    {
                        if (aiTarget is object && aiTarget.IsDead)
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

                        // DONE: Far objects handling
                        if (distance > WS_Base.BaseUnit.CombatReach_Base + aiCreature.CombatReach + aiTarget.BoundingRadius)
                        {
                            // DONE: Move closer
                            State = AIState.AI_MOVE_FOR_ATTACK;
                            DoMove();
                            return;
                        }

                        nextAttack -= WS_TimerBasedEvents.TAIManager.UPDATE_TIMER;
                        if (nextAttack > 0)
                            return;
                        nextAttack = 0;

                        // DONE: Look to aiTarget
                        WS_Base.BaseObject argObject1 = aiCreature;
                        WS_Base.BaseObject argObject2 = aiTarget;
                        if (!WorldServiceLocator._WS_Combat.IsInFrontOf(ref argObject1, ref argObject2))
                        {
                            WS_Base.BaseObject argTarget = aiTarget;
                            aiCreature.TurnTo(ref argTarget);
                        }

                        // DONE: Deal the damage
                        WS_Base.BaseUnit argAttacker = aiCreature;
                        var damageInfo = WorldServiceLocator._WS_Combat.CalculateDamage(ref argAttacker, ref aiTarget, false, false);
                        WS_Base.BaseObject argAttacker1 = aiCreature;
                        WS_Base.BaseObject argVictim = aiTarget;
                        WS_Network.ClientClass argclient = null;
                        WorldServiceLocator._WS_Combat.SendAttackerStateUpdate(ref argAttacker1, ref argVictim, damageInfo, client: ref argclient);
                        WS_Base.BaseUnit argAttacker2 = aiCreature;
                        aiTarget.DealDamage(damageInfo.GetDamage, ref argAttacker2);

                        // TODO: Do in another way, since 1001-2000 = 2 secs, and for creatures with like 1.05 sec attack time attacks ALOT slower
                        nextAttack = WorldServiceLocator._WorldServer.CREATURESDatabase[aiCreature.ID].BaseAttackTime;
                        aiTimer = 1000;
                    }
                    catch (Exception ex)
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "WS_Creatures:DoAttack failed - Guid: {1} ID: {2}  {0}", ex.Message);
                        Reset();
                    }
                }
            }

            public override void DoMove()
            {
                // DONE: Back to spawn if too far away
                if (aiTarget is null)
                {
                    float distanceToSpawn = WorldServiceLocator._WS_Combat.GetDistance(aiCreature.positionX, aiCreature.SpawnX, aiCreature.positionY, aiCreature.SpawnY, aiCreature.positionZ, aiCreature.SpawnZ);
                    if (IsWaypoint == false && aiCreature.SpawnID > 0 && distanceToSpawn > aiCreature.MaxDistance)
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
                        OnLeaveCombat(true);
                        return;
                    }
                }

                // DONE: If rooted don't move
                if (aiCreature.IsRooted)
                {
                    aiTimer = 1000;
                    return;
                }

                if (aiTarget is null)
                {
                    // DONE: Do simple random movement
                    int MoveTries = 0;
                    float selectedX;
                    float selectedY;
                    float selectedZ;
                    while (true)
                    {
                        if (MoveTries > 5) // The creature is at a very weird location right now
                        {
                            GoBackToSpawn();
                            return;
                        }

                        float distance = (float)(AI_INTERVAL_MOVE / 1000d * aiCreature.CreatureInfo.WalkSpeed);
                        float angle = (float)(WorldServiceLocator._WorldServer.Rnd.NextDouble() * PIx2);
                        aiCreature.SetToRealPosition();
                        aiCreature.orientation = angle;
                        selectedX = (float)(aiCreature.positionX + Math.Cos(angle) * distance);
                        selectedY = (float)(aiCreature.positionY + Math.Sin(angle) * distance);
                        selectedZ = WorldServiceLocator._WS_Maps.GetZCoord(selectedX, selectedY, aiCreature.positionZ, aiCreature.MapID);
                        MoveTries += 1;
                        if (Math.Abs(aiCreature.positionZ - selectedZ) > 5.0f)
                            continue; // Prevent most cases of wall climbing
                        WS_Base.BaseObject argobj = aiCreature;
                        if (WorldServiceLocator._WS_Maps.IsInLineOfSight(ref argobj, selectedX, selectedY, selectedZ + 1.0f) == false)
                            continue; // Prevent moving through walls
                        break; // Movement success
                    }

                    if (aiCreature.CanMoveTo(selectedX, selectedY, selectedZ))
                    {
                        State = AIState.AI_WANDERING;
                        aiTimer = aiCreature.MoveTo(selectedX, selectedY, selectedZ, Running: false);
                    }
                    else
                    {
                        aiTimer = AI_INTERVAL_MOVE;
                    }
                }
                else
                {
                    // DONE: Change the target to the one with most threat
                    SelectTarget();
                    if (CheckTarget())
                    {
                        return;
                    }

                    // DONE: Decide it's real position
                    aiCreature.SetToRealPosition();
                    if (aiTarget is WS_Creatures.CreatureObject)
                        ((WS_Creatures.CreatureObject)aiTarget).SetToRealPosition();

                    // DONE: Do targeted movement to attack target
                    float distance = 1000f * aiCreature.CreatureInfo.RunSpeed;
                    float distanceToTarget = WorldServiceLocator._WS_Combat.GetDistance(aiCreature, aiTarget);
                    if (distanceToTarget < distance)
                    {
                        // DONE: Move to target
                        State = AIState.AI_ATTACKING;
                        float destDist = WS_Base.BaseUnit.CombatReach_Base + aiCreature.CombatReach + aiTarget.BoundingRadius;
                        if (distanceToTarget <= destDist)
                        {
                            DoAttack();
                        }

                        destDist *= 0.5f;
                        float NearX = aiTarget.positionX;
                        if (aiTarget.positionX > aiCreature.positionX)
                            NearX -= destDist;
                        else
                            NearX += destDist;
                        float NearY = aiTarget.positionY;
                        if (aiTarget.positionY > aiCreature.positionY)
                            NearY -= destDist;
                        else
                            NearY += destDist;
                        float NearZ = WorldServiceLocator._WS_Maps.GetZCoord(NearX, NearY, aiCreature.positionZ, aiCreature.MapID);
                        if (NearZ > aiTarget.positionZ + 2f | NearZ < aiTarget.positionZ - 2f)
                            NearZ = aiTarget.positionZ;
                        if (aiCreature.CanMoveTo(NearX, NearY, NearZ))
                        {
                            aiCreature.orientation = WorldServiceLocator._WS_Combat.GetOrientation(aiCreature.positionX, NearX, aiCreature.positionY, NearY);
                            aiTimer = aiCreature.MoveTo(NearX, NearY, NearZ, Running: true);
                        }
                        else
                        {
                            // DONE: Select next target
                            aiHateTable.Remove(aiTarget);
                            if (aiTarget is WS_PlayerData.CharacterObject)
                            {
                                ((WS_PlayerData.CharacterObject)aiTarget).RemoveFromCombat(aiCreature);
                            }

                            SelectTarget();
                            CheckTarget();
                        }
                    }
                    else
                    {
                        // DONE: Move to target by vector
                        State = AIState.AI_MOVE_FOR_ATTACK;
                        float angle = WorldServiceLocator._WS_Combat.GetOrientation(aiCreature.positionX, aiTarget.positionX, aiCreature.positionY, aiTarget.positionY);
                        aiCreature.orientation = angle;
                        float selectedX = (float)(aiCreature.positionX + Math.Cos(angle) * distance);
                        float selectedY = (float)(aiCreature.positionY + Math.Sin(angle) * distance);
                        float selectedZ = WorldServiceLocator._WS_Maps.GetZCoord(selectedX, selectedY, aiCreature.positionZ, aiCreature.MapID);
                        if (aiCreature.CanMoveTo(selectedX, selectedY, selectedZ))
                        {
                            aiTimer = aiCreature.MoveTo(selectedX, selectedY, selectedZ, Running: true);
                        }
                        else
                        {
                            // DONE: Select next target
                            aiHateTable.Remove(aiTarget);
                            if (aiTarget is WS_PlayerData.CharacterObject)
                            {
                                ((WS_PlayerData.CharacterObject)aiTarget).RemoveFromCombat(aiCreature);
                            }

                            SelectTarget();
                            CheckTarget();
                        }
                    }
                }
            }

            protected void DoMoveReset()
            {
                float distance;
                if (ResetRun)
                {
                    distance = (float)(AI_INTERVAL_MOVE / 1000d * aiCreature.CreatureInfo.RunSpeed);
                }
                else
                {
                    distance = (float)(AI_INTERVAL_MOVE / 1000d * aiCreature.CreatureInfo.WalkSpeed);
                }

                aiCreature.SetToRealPosition(true);
                float angle = WorldServiceLocator._WS_Combat.GetOrientation(aiCreature.positionX, ResetX, aiCreature.positionY, ResetY);
                aiCreature.orientation = angle;
                float tmpDist = WorldServiceLocator._WS_Combat.GetDistance(aiCreature, ResetX, ResetY, ResetZ);
                if (tmpDist < distance)
                {
                    aiTimer = aiCreature.MoveTo(ResetX, ResetY, ResetZ, ResetO, ResetRun);
                    ResetFinished = true;
                }
                else
                {
                    float selectedX = (float)(aiCreature.positionX + Math.Cos(angle) * distance);
                    float selectedY = (float)(aiCreature.positionY + Math.Sin(angle) * distance);
                    float selectedZ = WorldServiceLocator._WS_Maps.GetZCoord(selectedX, selectedY, aiCreature.positionZ, aiCreature.MapID);
                    aiTimer = aiCreature.MoveTo(selectedX, selectedY, selectedZ, Running: ResetRun) - 50;
                } // Remove 50ms so that it doesn't pause
            }
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public class StandStillAI : DefaultAI
        {
            public StandStillAI(ref WS_Creatures.CreatureObject Creature) : base(ref Creature)
            {
                AllowedMove = false;
            }
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public class CritterAI : TBaseAI
        {
            protected WS_Creatures.CreatureObject aiCreature = null;
            protected int aiTimer = 0;
            protected int CombatTimer = 0;
            protected bool WasAlive = true;
            protected const int AI_INTERVAL_MOVE = 3000;
            protected const double PIx2 = 2d * Math.PI;

            public CritterAI(ref WS_Creatures.CreatureObject Creature)
            {
                State = AIState.AI_WANDERING;
                aiCreature = Creature;
                aiTarget = null;
            }

            public override bool IsMoving()
            {
                if (WorldServiceLocator._NativeMethods.timeGetTime("") - aiCreature.LastMove < aiTimer)
                {
                    switch (State)
                    {
                        case var @case when @case == AIState.AI_MOVE_FOR_ATTACK:
                            {
                                return true;
                            }

                        case var case1 when case1 == AIState.AI_MOVING:
                            {
                                return true;
                            }

                        case var case2 when case2 == AIState.AI_WANDERING:
                            {
                                return true;
                            }

                        default:
                            {
                                return false;
                            }
                    }
                }
                else
                {
                    return false;
                }
            }

            public override void Pause(int Time)
            {
                aiTimer = Time;
            }

            public override void OnEnterCombat()
            {
            }

            public override void OnLeaveCombat(bool Reset = true)
            {
            }

            public override void OnGenerateHate(ref WS_Base.BaseUnit Attacker, int HateValue)
            {
                if (CombatTimer > 0)
                    return;
                CombatTimer = 6000;
                State = AIState.AI_ATTACKING;
            }

            public override void Reset()
            {
                aiTimer = 0;
            }

            public override void DoThink()
            {
                if (aiCreature is null)
                    return; // Fixes a crash
                if (aiTimer > WS_TimerBasedEvents.TAIManager.UPDATE_TIMER)
                {
                    aiTimer -= WS_TimerBasedEvents.TAIManager.UPDATE_TIMER;
                    return;
                }
                else
                {
                    aiTimer = 0;
                }

                // DONE: Fixes a bug where creatures attack you when they are dead
                if (State != AIState.AI_DEAD && State != AIState.AI_RESPAWN && aiCreature.Life.Current == 0)
                {
                    State = AIState.AI_DEAD;
                }

                // DONE: If stunned
                if (aiCreature.IsStunned)
                {
                    aiTimer = 1000;
                    return;
                }

                switch (State)
                {
                    case var @case when @case == AIState.AI_DEAD:
                        {
                            if (WasAlive)
                            {
                                OnLeaveCombat(false);
                                aiTimer = 30000; // 30 seconds until the corpse disappear
                                WasAlive = false;
                            }
                            else
                            {
                                State = AIState.AI_RESPAWN;
                                WasAlive = true;
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
                            }

                            break;
                        }

                    case var case1 when case1 == AIState.AI_RESPAWN:
                        {
                            State = AIState.AI_WANDERING;
                            aiCreature.Respawn();
                            aiTimer = 10000; // Wait 10 seconds before starting to react
                            break;
                        }

                    case var case2 when case2 == AIState.AI_MOVE_FOR_ATTACK:
                        {
                            State = AIState.AI_ATTACKING;
                            DoMove();
                            break;
                        }

                    case var case3 when case3 == AIState.AI_WANDERING:
                        {
                            if (WorldServiceLocator._WorldServer.Rnd.NextDouble() > 0.2d)
                            {
                                DoMove();
                            }

                            break;
                        }

                    case var case4 when case4 == AIState.AI_MOVING_TO_SPAWN:
                        {
                            State = AIState.AI_WANDERING;
                            DoMove();
                            break;
                        }

                    case var case5 when case5 == AIState.AI_ATTACKING:
                        {
                            DoMove();
                            break;
                        }

                    case var case6 when case6 == AIState.AI_MOVING:
                        {
                            State = AIState.AI_WANDERING;
                            DoMove();
                            break;
                        }

                    case var case7 when case7 == AIState.AI_DO_NOTHING:
                        {
                            break;
                        }

                    default:
                        {
                            aiCreature.SendChatMessage("Unknown AI mode!", ChatMsg.CHAT_MSG_MONSTER_SAY, LANGUAGES.LANG_UNIVERSAL);
                            State = AIState.AI_DO_NOTHING;
                            break;
                        }
                }
            }

            public override void DoMove()
            {
                // DONE: If rooted don't move
                if (aiCreature.IsRooted)
                {
                    aiTimer = 1000;
                    return;
                }

                // DONE: Do simple random movement
                byte MoveTries = 0;
            TryMoveAgain:
                ;
                if (MoveTries > 5) // The creature is at a very weird location right now
                {
                    aiCreature.MoveToInstant(aiCreature.SpawnX, aiCreature.SpawnY, aiCreature.SpawnZ, aiCreature.orientation);
                    return;
                }

                // DONE: If the creature was attacked it will start fleeing randomly
                bool DoRun = false;
                if (State == AIState.AI_ATTACKING)
                {
                    CombatTimer -= WS_TimerBasedEvents.TAIManager.UPDATE_TIMER;
                    if (CombatTimer <= 0)
                    {
                        CombatTimer = 0;
                        State = AIState.AI_WANDERING;
                    }
                    else
                    {
                        DoRun = true;
                    }
                }

                float distance;
                if (DoRun)
                {
                    distance = (float)(AI_INTERVAL_MOVE / 1000d * aiCreature.CreatureInfo.RunSpeed * aiCreature.SpeedMod);
                }
                else
                {
                    distance = (float)(AI_INTERVAL_MOVE / 1000d * aiCreature.CreatureInfo.WalkSpeed);
                }

                float angle = (float)(WorldServiceLocator._WorldServer.Rnd.NextDouble() * PIx2);
                aiCreature.SetToRealPosition();
                aiCreature.orientation = angle;
                float selectedX = (float)(aiCreature.positionX + Math.Cos(angle) * distance);
                float selectedY = (float)(aiCreature.positionY + Math.Sin(angle) * distance);
                float selectedZ = WorldServiceLocator._WS_Maps.GetZCoord(selectedX, selectedY, aiCreature.positionZ, aiCreature.MapID);
                MoveTries = (byte)(MoveTries + 1);
                if (Math.Abs(aiCreature.positionZ - selectedZ) > 5.0f)
                    goto TryMoveAgain; // Prevent most cases of wall climbing
                WS_Base.BaseObject argobj = aiCreature;
                if (WorldServiceLocator._WS_Maps.IsInLineOfSight(ref argobj, selectedX, selectedY, selectedZ + 2.0f) == false)
                    goto TryMoveAgain; // Prevent moving through walls
                if (aiCreature.CanMoveTo(selectedX, selectedY, selectedZ))
                {
                    aiTimer = aiCreature.MoveTo(selectedX, selectedY, selectedZ, Running: DoRun);
                }
                else
                {
                    aiTimer = AI_INTERVAL_MOVE;
                }
            }
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public class GuardAI : DefaultAI
        {
            public GuardAI(ref WS_Creatures.CreatureObject Creature) : base(ref Creature)
            {
                AllowedMove = false;
            }

            public void OnEmote(int emote)
            {
                switch (emote)
                {
                    case 58: // Kiss
                        {
                            aiCreature.DoEmote(Emotes.ONESHOT_BOW);
                            break;
                        }

                    case 101: // Wave
                        {
                            aiCreature.DoEmote(Emotes.ONESHOT_WAVE);
                            break;
                        }

                    case 78: // Salute
                        {
                            aiCreature.DoEmote(Emotes.ONESHOT_SALUTE);
                            break;
                        }

                    case 84: // Shy
                        {
                            aiCreature.DoEmote(Emotes.ONESHOT_FLEX);
                            break;
                        }

                    case 77:
                    case 22: // Rude, Chicken
                        {
                            aiCreature.DoEmote(Emotes.ONESHOT_POINT);
                            break;
                        }
                }
            }
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public class WaypointAI : DefaultAI
        {
            public int CurrentWaypoint = -1;

            public WaypointAI(ref WS_Creatures.CreatureObject Creature) : base(ref Creature)
            {
                IsWaypoint = true;
            }

            public override void Pause(int Time)
            {
                CurrentWaypoint -= 1;
                aiTimer = Time;
            }

            public override void DoMove()
            {
                float distanceToSpawn = WorldServiceLocator._WS_Combat.GetDistance(aiCreature.positionX, aiCreature.SpawnX, aiCreature.positionY, aiCreature.SpawnY, aiCreature.positionZ, aiCreature.SpawnZ);
                if (aiTarget is null)
                {
                    if (WorldServiceLocator._WS_DBCDatabase.CreatureMovement.ContainsKey(aiCreature.WaypointID) == false)
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Creature [{0:X}] is missing waypoints.", aiCreature.GUID - WorldServiceLocator._Global_Constants.GUID_UNIT);
                        aiCreature.ResetAI();
                        return;
                    }

                    try
                    {
                        CurrentWaypoint += 1;
                        if (WorldServiceLocator._WS_DBCDatabase.CreatureMovement[aiCreature.WaypointID].ContainsKey(CurrentWaypoint) == false)
                            CurrentWaypoint = 1;
                        var MovementPoint = WorldServiceLocator._WS_DBCDatabase.CreatureMovement[aiCreature.WaypointID][CurrentWaypoint];
                        aiTimer = aiCreature.MoveTo(MovementPoint.x, MovementPoint.y, MovementPoint.z, Running: false) + MovementPoint.waittime;
                    }
                    catch (Exception)
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Creature [{0:X}] waypoints are damaged.", aiCreature.GUID - WorldServiceLocator._Global_Constants.GUID_UNIT);
                        aiCreature.ResetAI();
                        return;
                    }
                }
                else
                {
                    base.DoMove();
                }
            }
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public class GuardWaypointAI : GuardAI
        {
            public int CurrentWaypoint = -1;

            public GuardWaypointAI(ref WS_Creatures.CreatureObject Creature) : base(ref Creature)
            {
                AllowedMove = true;
                IsWaypoint = true;
            }

            public override void Pause(int Time)
            {
                CurrentWaypoint -= 1;
                aiTimer = Time;
            }

            public override void DoMove()
            {
                float distanceToSpawn = WorldServiceLocator._WS_Combat.GetDistance(aiCreature.positionX, aiCreature.SpawnX, aiCreature.positionY, aiCreature.SpawnY, aiCreature.positionZ, aiCreature.SpawnZ);
                if (aiTarget is null)
                {
                    if (WorldServiceLocator._WS_DBCDatabase.CreatureMovement.ContainsKey(aiCreature.WaypointID) == false)
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Creature [{0:X}] is missing waypoints.", aiCreature.GUID - WorldServiceLocator._Global_Constants.GUID_UNIT);
                        aiCreature.ResetAI();
                        return;
                    }

                    try
                    {
                        CurrentWaypoint += 1;
                        if (WorldServiceLocator._WS_DBCDatabase.CreatureMovement[aiCreature.WaypointID].ContainsKey(CurrentWaypoint) == false)
                            CurrentWaypoint = 1;
                        var MovementPoint = WorldServiceLocator._WS_DBCDatabase.CreatureMovement[aiCreature.WaypointID][CurrentWaypoint];
                        aiTimer = aiCreature.MoveTo(MovementPoint.x, MovementPoint.y, MovementPoint.z, Running: false) + MovementPoint.waittime;
                    }
                    catch (Exception ex)
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Creature [{0:X}] waypoints are damaged. {1}", aiCreature.GUID - WorldServiceLocator._Global_Constants.GUID_UNIT, ex.Message);
                        aiCreature.ResetAI();
                        return;
                    }
                }
                else
                {
                    base.DoMove();
                }
            }
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public class BossAI : DefaultAI
        {
            public BossAI(ref WS_Creatures.CreatureObject Creature) : base(ref Creature)
            {
            }

            public override void OnEnterCombat()
            {
                base.OnEnterCombat();

                // DONE: Set every player in the same raid into combat
                foreach (KeyValuePair<WS_Base.BaseUnit, int> Unit in aiHateTable)
                {
                    if (Unit.Key is WS_PlayerData.CharacterObject)
                    {
                        {
                            var withBlock = (WS_PlayerData.CharacterObject)Unit.Key;
                            if (withBlock.IsInGroup)
                            {
                                var localMembers = withBlock.Group.LocalMembers.ToArray();
                                foreach (ulong member in localMembers)
                                {
                                    if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(member) && WorldServiceLocator._WorldServer.CHARACTERs[member].MapID == withBlock.MapID && WorldServiceLocator._WorldServer.CHARACTERs[member].instance == withBlock.instance)
                                    {
                                        aiHateTable.Add(WorldServiceLocator._WorldServer.CHARACTERs[member], 0);
                                    }
                                }

                                break;
                            }
                        }
                    }
                }
            }

            public override void DoThink()
            {
                base.DoThink();

                // NOTE: Bosses uses a new thread because of their heavy updates sometimes
                var tmpThread = new Thread(OnThink) { Name = "Boss Thinking" };
                tmpThread.Start();
            }

            // NOTE: Bosses uses OnThink instead of DoThink
            public virtual void OnThink()
            {
            }
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
    }
}