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
using Mangos.Common.Enums.Chat;
using Mangos.Common.Enums.Misc;

// AI TODO: Implement a workaround (Or fix, fixes work too!) for Armageddon.
namespace Mangos.Scripts.Creatures
{
    public class CreatureAI_Baron_Geddon : Mangos.World.AI.WS_Creatures_AI.BossAI
    {
        private const int AI_UPDATE = 1000;
        private const int Inferno_CD = 45000;
        private const int Ignite_CD = 30000;
        private const int Living_Bomb_CD = 35000;
        private const int Spell_Inferno = 19695;
        private const int Spell_Ignite = 19659; // Drains a random targets mana.
        private const int Spell_Living_Bomb = 20475;
        private const int Spell_Armageddon = 20478; // Cast at 2% to make self invincible, this spell won't work so we'll make a workaround.
        public int NextWaypoint = 0;
        public int CurrentWaypoint = 0;
        public int NextInferno = 0;
        public int NextIgnite = 0;
        public int NextLivingBomb = 0;

        public CreatureAI_Baron_Geddon(ref Mangos.World.Objects.WS_Creatures.CreatureObject Creature) : base(ref Creature)
        {
            this.AllowedMove = false;
            Creature.Flying = false;
            Creature.VisibleDistance = 700f;
        }

        public override void OnThink()
        {
            NextInferno -= AI_UPDATE;
            NextIgnite -= AI_UPDATE;
            NextLivingBomb -= AI_UPDATE;
            if (NextInferno <= 0)
            {
                NextInferno = Inferno_CD;
                this.aiCreature.CastSpell(Spell_Inferno, this.aiTarget);
            }

            if (NextIgnite <= 0)
            {
                NextIgnite = Ignite_CD;
                this.aiCreature.CastSpell(Spell_Ignite, this.aiCreature.GetRandomTarget());
            }

            if (NextLivingBomb <= 0)
            {
                NextLivingBomb = Living_Bomb_CD;
                this.aiCreature.CastSpell(Spell_Living_Bomb, this.aiCreature.GetRandomTarget());
            }
        }

        public void CastInferno()
        {
            for (int i = 0; i <= 3; i++)
            {
                var Target = this.aiTarget;
                if (Target is null)
                    return;
                this.aiCreature.CastSpell(Spell_Inferno, this.aiTarget); // This spell should be mitigated with fire resistance and nothing more.
            }
        }

        public void CastIgnite()
        {
            for (int i = 1; i <= 3; i++)
            {
                var target = this.aiCreature.GetRandomTarget();
                if (target is null)
                    return;
                this.aiCreature.CastSpell(Spell_Ignite, this.aiCreature.GetRandomTarget()); // This spell drains 400 mana per second and MUST be dispelled immediately or your healers will wipe the group.
            }
        }

        public void CastLivingBomb()
        {
            for (int i = 2; i <= 3; i++)
            {
                var target = this.aiCreature.GetRandomTarget();
                if (target is null)
                    return;
                this.aiCreature.CastSpell(Spell_Living_Bomb, this.aiCreature.GetRandomTarget()); // The traditional way of getting away of this is to run where the dead trash is from your group so they don't die, but we may need to fix AoE implementations for this.
            }
        }

        public override void OnHealthChange(int Percent)
        {
            base.OnHealthChange(Percent);
            if (Percent <= 2)
            {
                try
                {
                    this.aiCreature.CastSpellOnSelf(Spell_Armageddon); // I think during this time he's supposed to have a kamakazie of sorts.
                }
                catch (Exception ex)
                {
                    this.aiCreature.SendChatMessage("I have failed to become invincible at 2% or less HP. this is a problem.", ChatMsg.CHAT_MSG_YELL, LANGUAGES.LANG_GLOBAL);
                }
            }
        }
    }
}