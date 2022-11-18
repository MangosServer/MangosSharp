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

public class CreatureAI_Taragaman_the_Hungerer : WS_Creatures_AI.BossAI
{
    private const int AI_UPDATE = 1000;
    private const int NOVA_COOLDOWN = 4000;
    private const int UPPER_COOLDOWN = 12000;
    private const int NOVA_SPELL = 11970;
    private const int UPPER_SPELL = 18072;
    public int NextWaypoint;
    public int NextNOVA;
    public int NextUPPER;
    public int CurrentWaypoint;

    public CreatureAI_Taragaman_the_Hungerer(ref WS_Creatures.CreatureObject Creature) : base(ref Creature)
    {
        AllowedMove = false;
        Creature.Flying = false;
        Creature.VisibleDistance = 700f;
    }

    public override void OnThink()
    {
        NextNOVA -= AI_UPDATE;
        NextUPPER -= AI_UPDATE;
        if (NextNOVA <= 0)
        {
            NextNOVA = NOVA_COOLDOWN;
            aiCreature.CastSpell(NOVA_SPELL, aiTarget); // Fire Nova
        }

        if (NextUPPER <= 1)
        {
            NextUPPER = UPPER_COOLDOWN;
            aiCreature.CastSpell(UPPER_SPELL, aiTarget); // Uppercut
        }
    }

    public void CastNOVA()
    {
        for (var i = 0; i <= 1; i++)
        {
            WS_Base.BaseUnit Target = aiCreature;
            if (Target is null)
            {
                return;
            }

            aiCreature.CastSpell(NOVA_SPELL, aiTarget);
        }
    }

    public void CastUPPER()
    {
        for (var i = 1; i <= 1; i++)
        {
            WS_Base.BaseUnit Target = aiCreature;
            if (Target is null)
            {
                return;
            }

            aiCreature.CastSpell(UPPER_SPELL, aiTarget);
        }
    }
}
