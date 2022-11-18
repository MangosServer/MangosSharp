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

using Mangos.World.AI;
using Mangos.World.Objects;

namespace Mangos.World.Scripts.Creatures;

public class CreatureAI_Ragefire_Shaman : WS_Creatures_AI.BossAI
{
    private const int AI_UPDATE = 1000;
    private const int HEAL_COOLDOWN = 8000;
    private const int BOLT_COOLDOWN = 3000;
    private const int HEAL_SPELL = 11986;
    private const int BOLT_SPELL = 9532;
    public int NextWaypoint;
    public int NextHeal;
    public int NextBolt;
    public int CurrentWaypoint;

    public CreatureAI_Ragefire_Shaman(ref WS_Creatures.CreatureObject Creature) : base(ref Creature)
    {
        AllowedMove = false;
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
            aiCreature.CastSpell(HEAL_SPELL, aiTarget); // HEALING WAVE
        }

        if (NextBolt <= 1)
        {
            NextBolt = BOLT_COOLDOWN;
            aiCreature.CastSpell(BOLT_SPELL, aiTarget); // LIGHTNING BOLT
        }
    }

    public void CastHeal()
    {
        for (var i = 0; i <= 1; i++)
        {
            WS_Base.BaseUnit Target = aiCreature;
            if (Target is null)
            {
                return;
            }

            aiCreature.CastSpell(HEAL_SPELL, aiTarget);
        }
    }

    public void CastBolt()
    {
        for (var i = 1; i <= 1; i++)
        {
            WS_Base.BaseUnit Target = aiCreature;
            if (Target is null)
            {
                return;
            }

            aiCreature.CastSpell(BOLT_SPELL, aiTarget);
        }
    }
}
