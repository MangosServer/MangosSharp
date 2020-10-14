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
    public class CreatureAI_Searing_Blade_Enforcer : World.AI.WS_Creatures_AI.BossAI
    {
        private const int AI_UPDATE = 1000;
        private const int SLAM_COOLDOWN = 8000;
        private const int SLAM_SPELL = 8242;
        public int NextWaypoint = 0;
        public int NextSLAM = 0;
        public int CurrentWaypoint = 0;

        public CreatureAI_Searing_Blade_Enforcer(ref World.Objects.WS_Creatures.CreatureObject Creature) : base(ref Creature)
        {
            AllowedMove = false;
            Creature.Flying = false;
            Creature.VisibleDistance = 700f;
        }

        public override void OnThink()
        {
            NextSLAM -= AI_UPDATE;
            if (NextSLAM <= 0)
            {
                NextSLAM = SLAM_COOLDOWN;
                aiCreature.CastSpell(SLAM_SPELL, aiTarget); // Curse of Agony
            }
        }

        public void CastSLAM()
        {
            for (int i = 0; i <= 3; i++)
            {
                World.Objects.WS_Base.BaseUnit Target = aiCreature;
                if (Target is null)
                    return;
                aiCreature.CastSpell(SLAM_SPELL, aiTarget);
            }
        }
    }
}