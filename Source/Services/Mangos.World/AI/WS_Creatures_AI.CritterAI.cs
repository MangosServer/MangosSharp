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

using Mangos.Common.Enums.Chat;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Misc;
using Mangos.World.Objects;
using System;

namespace Mangos.World.AI;

public partial class WS_Creatures_AI
{
    public class CritterAI : TBaseAI
    {
        protected WS_Creatures.CreatureObject aiCreature;

        protected int aiTimer;

        protected int CombatTimer;

        protected bool WasAlive;

        protected const int AI_INTERVAL_MOVE = 3000;

        protected const float PIx2 = (float)Math.PI * 2f;

        public CritterAI(ref WS_Creatures.CreatureObject Creature)
        {
            aiCreature = null;
            aiTimer = 0;
            CombatTimer = 0;
            WasAlive = true;
            State = AIState.AI_WANDERING;
            aiCreature = Creature ?? throw new ArgumentNullException(nameof(Creature));
            aiTarget = null;
        }

        public override bool IsMoving => checked(WorldServiceLocator._NativeMethods.timeGetTime("") - aiCreature.LastMove) < aiTimer
            && (State switch
            {
                AIState.AI_MOVE_FOR_ATTACK => true,
                AIState.AI_MOVING => true,
                AIState.AI_WANDERING => true,
                _ => false,
            });

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
            if (Attacker is null)
            {
                throw new ArgumentNullException(nameof(Attacker));
            }

            if (CombatTimer <= 0)
            {
                CombatTimer = 6000;
                State = AIState.AI_ATTACKING;
            }
        }

        public override void Reset()
        {
            aiTimer = 0;
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
                if (State != AIState.AI_DEAD && State != AIState.AI_RESPAWN && aiCreature.Life.Current == 0)
                {
                    State = AIState.AI_DEAD;
                }
                if ((bool)(aiCreature?.IsStunned))
                {
                    aiTimer = 1000;
                    return;
                }
                switch (State)
                {
                    case AIState.AI_DEAD:
                        {
                            if (WasAlive)
                            {
                                OnLeaveCombat(Reset: false);
                                aiTimer = 30000;
                                WasAlive = false;
                                break;
                            }
                            State = AIState.AI_RESPAWN;
                            WasAlive = true;
                            var RespawnTime = (int)(aiCreature?.SpawnTime);
                            switch (RespawnTime)
                            {
                                case > 0:
                                    aiTimer = RespawnTime * 1000;
                                    aiCreature?.Despawn();
                                    break;
                                default:
                                    aiCreature?.Destroy();
                                    break;
                            }
                            break;
                        }
                    case AIState.AI_RESPAWN:
                        State = AIState.AI_WANDERING;
                        aiCreature?.Respawn();
                        aiTimer = 10000;
                        break;

                    case AIState.AI_MOVE_FOR_ATTACK:
                        State = AIState.AI_ATTACKING;
                        DoMove();
                        break;

                    case AIState.AI_WANDERING:
                        if (WorldServiceLocator._WorldServer.Rnd.NextDouble() > 0.20000000298023224)
                        {
                            DoMove();
                        }
                        break;

                    case AIState.AI_MOVING_TO_SPAWN:
                        State = AIState.AI_WANDERING;
                        DoMove();
                        break;

                    case AIState.AI_ATTACKING:
                        DoMove();
                        break;

                    case AIState.AI_MOVING:
                        State = AIState.AI_WANDERING;
                        DoMove();
                        break;

                    case AIState.AI_DO_NOTHING:
                        break;

                    default:
                        aiCreature?.SendChatMessage("Unknown AI mode!", ChatMsg.CHAT_MSG_MONSTER_SAY, LANGUAGES.LANG_GLOBAL);
                        State = AIState.AI_DO_NOTHING;
                        break;
                }
            }
        }

        public override void DoMove()
        {
            if ((bool)(aiCreature?.IsRooted))
            {
                aiTimer = 1000;
                return;
            }
            byte MoveTries = 0;
            checked
            {
                bool DoRun;
                float selectedX;
                float selectedY;
                float selectedZ;
                while (true)
                {
                    if (MoveTries > 5)
                    {
                        aiCreature?.MoveToInstant(aiCreature.SpawnX, aiCreature.SpawnY, aiCreature.SpawnZ, aiCreature.orientation);
                        return;
                    }
                    DoRun = false;
                    if (State == AIState.AI_ATTACKING)
                    {
                        CombatTimer -= 1000;
                        switch (CombatTimer)
                        {
                            case <= 0:
                                CombatTimer = 0;
                                State = AIState.AI_WANDERING;
                                break;
                            default:
                                DoRun = true;
                                break;
                        }
                    }
                    var distance = (!DoRun) ? ((float)(3.0 * aiCreature?.CreatureInfo?.WalkSpeed)) : ((float)(3.0 * aiCreature?.CreatureInfo?.RunSpeed * aiCreature?.SpeedMod));
                    var angle = (float)(WorldServiceLocator._WorldServer.Rnd.NextDouble() * 6.2831854820251465);
                    aiCreature?.SetToRealPosition();
                    aiCreature.orientation = angle;
                    selectedX = (float)(aiCreature?.positionX + (Math.Cos(angle) * distance));
                    selectedY = (float)(aiCreature?.positionY + (Math.Sin(angle) * distance));
                    selectedZ = WorldServiceLocator._WS_Maps.GetZCoord(selectedX, selectedY, aiCreature.positionZ, aiCreature.MapID);
                    MoveTries = (byte)(MoveTries + 1);
                    if (!(Math.Abs(aiCreature.positionZ - selectedZ) > 5f))
                    {
                        ref var reference = ref aiCreature;
                        WS_Base.BaseObject obj = reference;
                        reference = (WS_Creatures.CreatureObject)obj;
                        var flag = WorldServiceLocator._WS_Maps.IsInLineOfSight(ref obj, selectedX, selectedY, selectedZ + 2f);
                        if (flag)
                        {
                            break;
                        }
                    }
                }
                aiTimer = aiCreature.CanMoveTo(selectedX, selectedY, selectedZ) ? aiCreature.MoveTo(selectedX, selectedY, selectedZ, 0f, DoRun) : 3000;
            }
        }
    }
}
