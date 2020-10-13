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

using Mangos.Common.Enums.Chat;
using Mangos.Common.Enums.Misc;

namespace Mangos.Scripts.Creatures
{
    public class CreatureAI_Onyxia : Mangos.World.AI.WS_Creatures_AI.BossAI
    {
        private const int AI_UPDATE = 1000;
        private const int BREATH_COOLDOWN = 11000;
        private const int WING_BUFFET_COOLDOWN = 15000;
        private const int CLEAVE_COOLDOWN = 3000;
        private const int KNOCK_COOLDOWN = 22000;
        private const int ROAR_COOLDOWN = 18000;
        private const int FIREBALL_COOLDOWN = 5000;
        private const int BREATH_SPELL = 18435;
        private const int WING_BUFFET_SPELL = 18500;
        private const int CLEAVE_SPELL = 19983;
        private const int TAIL_SWEEP_SPELL = 15847;
        private const int KNOCK_SPELL = 19633;
        private const int ROAR_SPELL = 18431;
        private const int FIREBALL_SPELL = 18392;
        private const int WHELP_CREATURE = 11262;
        public int Phase = 0;
        public int NextWaypoint = 0;
        public int NextBreathe = 0;
        public int NextWingBuffet = 0;
        public int NextCleave = 0;
        public int NextFireball = 0;
        public int KnockTimer = 0;
        public int RoarTimer = 0;
        public int CurrentWaypoint = 0;

        public CreatureAI_Onyxia(ref Mangos.World.Objects.WS_Creatures.CreatureObject Creature) : base(ref Creature)
        {
            Phase = 0;
            this.AllowedMove = false;
            Creature.Flying = false;
            Creature.VisibleDistance = 700f;
        }

        public override void OnEnterCombat()
        {
            if (Phase > 1)
                return;
            base.OnEnterCombat();
            this.aiCreature.Flying = false;
            this.AllowedAttack = true;
            Phase = 1;
            ReinitSpells();
            this.aiCreature.SendChatMessage("How fortuitous, usually I must leave my lair to feed!", ChatMsg.CHAT_MSG_MONSTER_YELL, LANGUAGES.LANG_UNIVERSAL);
        }

        public override void OnLeaveCombat(bool Reset = true)
        {
            base.OnLeaveCombat(Reset);
            this.AllowedAttack = true;
            Phase = 0;
            this.aiCreature.SendChatMessage("LEAVING COMBAT!", ChatMsg.CHAT_MSG_MONSTER_YELL, LANGUAGES.LANG_UNIVERSAL);
        }

        public override void OnKill(ref Mangos.World.Objects.WS_Base.BaseUnit Victim)
        {
            // TODO: Yell
            // TODO: Send sound (Die mortal?)!
            this.aiCreature.SendChatMessage("Die mortal, $N!", ChatMsg.CHAT_MSG_MONSTER_YELL, LANGUAGES.LANG_UNIVERSAL, Victim.GUID);
        }

        public override void OnHealthChange(int Percent)
        {
            this.aiCreature.SendChatMessage("My health: " + Percent + "%! (" + Phase + ")", ChatMsg.CHAT_MSG_MONSTER_YELL, LANGUAGES.LANG_UNIVERSAL);
            if (Phase == 1)
            {
                if (Percent <= 65)
                {
                    Phase = 2;
                    Go_FlyPhase();
                }
            }
            else if (Phase == 2)
            {
                if (Percent <= 40)
                {
                    Phase = 3;
                    Go_LastPhase();
                }
            }
        }

        public override void OnDeath()
        {
            // TODO: Yell
            this.aiCreature.SendChatMessage("I shouldn't have died yet! Feed my kids plx!", ChatMsg.CHAT_MSG_MONSTER_YELL, LANGUAGES.LANG_UNIVERSAL);
        }

        public override void OnThink()
        {
            if (Phase < 1)
                return;
            if (Phase == 1 || Phase == 3)
            {
                NextBreathe -= AI_UPDATE;
                NextCleave -= AI_UPDATE;
                NextWingBuffet -= AI_UPDATE;
                if (NextBreathe <= 0)
                {
                    NextBreathe = BREATH_COOLDOWN;
                    this.aiCreature.CastSpell(BREATH_SPELL, this.aiTarget); // Flame breathe
                }

                if (NextWingBuffet <= 0)
                {
                    NextWingBuffet = WING_BUFFET_COOLDOWN;
                    this.aiCreature.CastSpell(WING_BUFFET_SPELL, this.aiTarget); // Wing buffet
                }

                if (NextCleave <= 0)
                {
                    NextCleave = CLEAVE_COOLDOWN;
                    this.aiCreature.CastSpell(CLEAVE_SPELL, this.aiTarget); // Cleave
                    // aiCreature.CastSpell(TAIL_SWEEP_SPELL, aiTarget) 'Tail Sweep
                }

                if (Phase == 3)
                {
                    KnockTimer -= AI_UPDATE;
                    RoarTimer -= AI_UPDATE;
                    if (KnockTimer <= 0)
                    {
                        KnockTimer = KNOCK_COOLDOWN;
                        this.aiCreature.CastSpell(KNOCK_SPELL, this.aiTarget); // Knock Away
                        this.aiHateTable[this.aiTarget] = (int)(this.aiHateTable[this.aiTarget] * 0.75f);
                    }

                    if (RoarTimer <= 0)
                    {
                        RoarTimer = ROAR_COOLDOWN;
                        this.aiCreature.CastSpell(ROAR_SPELL, this.aiTarget); // Bellowing Roar
                    }
                }
            }
            else if (Phase == 2)
            {
                NextFireball -= AI_UPDATE;
                if (NextFireball <= 0)
                {
                    NextFireball = FIREBALL_COOLDOWN;
                    CastFireball();
                }
            }

            if (NextWaypoint > 0)
            {
                NextWaypoint -= AI_UPDATE;
                if (NextWaypoint <= 0)
                {
                    On_Waypoint();
                }
            }
        }

        public void CastFireball()
        {
            for (int i = 0; i <= 3; i++)
            {
                var theTarget = this.aiCreature.GetRandomTarget();
                if (theTarget is null)
                    return;
                this.aiCreature.CastSpell(FIREBALL_SPELL, theTarget.positionX, theTarget.positionY, theTarget.positionZ);
            }
        }

        public void Go_FlyPhase()
        {
            // TODO: Get up into the air
            // TODO: Start sending random fireballs (only to ranged dps?)
            // TODO: Deep breathe
            // TODO: Do emote
            // TODO: Whelps
            // TODO: Movement in the air
            this.aiCreature.SendChatMessage("Phase 2!", ChatMsg.CHAT_MSG_MONSTER_YELL, LANGUAGES.LANG_UNIVERSAL);
            CurrentWaypoint = 0;
            On_Waypoint();
            // DONE: Reset hate table
            this.AllowedAttack = false;
            this.aiTarget = null;
            this.ResetThreatTable();
            this.aiCreature.SendTargetUpdate(0UL);
        }

        public void Go_LastPhase()
        {
            // TODO: Land again
            // TODO: Do emote
            this.aiCreature.SendChatMessage("Phase 3!", ChatMsg.CHAT_MSG_MONSTER_YELL, LANGUAGES.LANG_UNIVERSAL);
            NextWaypoint = 0;
            this.aiCreature.Flying = false;
            // TODO: Do these after landing
            ReinitSpells();
            // DONE: Reset hate table
            this.AllowedAttack = true;
            this.aiTarget = null;
            this.ResetThreatTable();
            this.aiCreature.SendTargetUpdate(0UL);
        }

        public void ReinitSpells()
        {
            NextBreathe = BREATH_COOLDOWN;
            NextWingBuffet = WING_BUFFET_COOLDOWN;
            NextCleave = CLEAVE_COOLDOWN;
            KnockTimer = KNOCK_COOLDOWN;
            RoarTimer = ROAR_COOLDOWN;
        }

        public void On_Waypoint()
        {
            switch (CurrentWaypoint)
            {
                case 0:
                    {
                        NextWaypoint = this.aiCreature.MoveTo(-75.945f, -219.245f, -83.375f, 0.004947f, true);
                        break;
                    }

                case 1:
                    {
                        // TODO: Set Flying
                        this.aiCreature.Flying = true;
                        NextWaypoint = 10000;
                        NextFireball = NextWaypoint;
                        this.aiCreature.MoveTo(42.621f, -217.195f, -66.056f, 3.014011f);
                        break;
                    }

                case 2:
                    {
                        NextWaypoint = 23000;
                        // TODO: How many whelps per side?
                        SpawnWhelpsLeft(15);
                        SpawnWhelpsRight(15);
                        break;
                    }

                case 3:
                    {
                        NextWaypoint = 10000;
                        this.aiCreature.MoveTo(12.27f, -254.694f, -67.997f, 2.395585f);
                        break;
                    }

                case 4:
                case 6:
                case 8:
                case 10:
                case 12:
                    {
                        NextWaypoint = 23000;
                        // TODO: How many whelps per side?
                        SpawnWhelpsLeft(10);
                        SpawnWhelpsRight(10);
                        break;
                    }

                case 5:
                    {
                        NextWaypoint = 10000;
                        this.aiCreature.MoveTo(-79.02f, -252.374f, -68.965f, 0.885179f);
                        break;
                    }

                case 7:
                    {
                        NextWaypoint = 10000;
                        this.aiCreature.MoveTo(-80.257f, -174.24f, -69.293f, 5.695741f);
                        break;
                    }

                case 9:
                    {
                        NextWaypoint = 10000;
                        this.aiCreature.MoveTo(27.875f, -178.547f, -66.041f, 3.908957f);
                        break;
                    }

                case 11:
                    {
                        NextWaypoint = 10000;
                        this.aiCreature.MoveTo(-4.868f, -217.171f, -86.71f, 3.14159f);
                        break;
                    }
            }

            CurrentWaypoint += 1;
            if (CurrentWaypoint > 12)
                CurrentWaypoint = 3;
        }

        public void SpawnWhelpsLeft(int Count)
        {
            for (int i = 1, loopTo = Count; i <= loopTo; i++)
                this.aiCreature.SpawnCreature(WHELP_CREATURE, -30.812f, -166.395f, -89.0f);
        }

        public void SpawnWhelpsRight(int Count)
        {
            for (int i = 1, loopTo = Count; i <= loopTo; i++)
                this.aiCreature.SpawnCreature(WHELP_CREATURE, -30.233f, -264.158f, 89.896f);
        }
    }
}