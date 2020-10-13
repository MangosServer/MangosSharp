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


namespace Mangos.Scripts.Creatures
{
    public class CreatureAI_Ragefire_Shaman : Mangos.World.AI.WS_Creatures_AI.BossAI
    {
        private const int AI_UPDATE = 1000;
        private const int HEAL_COOLDOWN = 8000;
        private const int BOLT_COOLDOWN = 3000;
        private const int HEAL_SPELL = 11986;
        private const int BOLT_SPELL = 9532;
        public int NextWaypoint = 0;
        public int NextHeal = 0;
        public int NextBolt = 0;
        public int CurrentWaypoint = 0;

        public CreatureAI_Ragefire_Shaman(ref Mangos.World.Objects.WS_Creatures.CreatureObject Creature) : base(ref Creature)
        {
            this.AllowedMove = false;
            Creature.Flying = false;
            Creature.VisibleDistance = 700f;
        }

        public override void OnThink()
        {
            NextHeal -= AI_UPDATE;
            NextBolt -= AI_UPDATE;
            if (NextHeal <= 0)
            {
                NextHeal = HEAL_COOLDOWN;
                this.aiCreature.CastSpell(HEAL_SPELL, this.aiTarget); // HEALING WAVE
            }

            if (NextBolt <= 1)
            {
                NextBolt = BOLT_COOLDOWN;
                this.aiCreature.CastSpell(BOLT_SPELL, this.aiTarget); // LIGHTNING BOLT
            }
        }

        public void CastHeal()
        {
            for (int i = 0; i <= 1; i++)
            {
                Mangos.World.Objects.WS_Base.BaseUnit Target = this.aiCreature;
                if (Target is null)
                    return;
                this.aiCreature.CastSpell(HEAL_SPELL, this.aiTarget);
            }
        }

        public void CastBolt()
        {
            for (int i = 1; i <= 1; i++)
            {
                Mangos.World.Objects.WS_Base.BaseUnit Target = this.aiCreature;
                if (Target is null)
                    return;
                this.aiCreature.CastSpell(BOLT_SPELL, this.aiTarget);
            }
        }
    }
}