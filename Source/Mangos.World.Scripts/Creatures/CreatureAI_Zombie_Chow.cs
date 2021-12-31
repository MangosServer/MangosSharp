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
using Microsoft.VisualBasic.CompilerServices;

// Basically, this AI is kitable and if the AI hits Gluth, it heals her for 5% of her HP (50,000 in this case.). Since we can't really do it that way, it has a set waypoint.
namespace Mangos.World.Scripts.Creatures;

public class CreatureAI_Zombie_Chow : WS_Creatures_AI.BossAI
{
    private const int AI_UPDATE = 1000;
    private const int Infected_Wound_CD = 15000;
    private const int NPC_Gluth = 15932;
    private const int Spell_Infected_Wound = 29306; // The target takes 100 extra physical damage. This ability stacks.
    public int NextInfectedWound;
    public int NextWaypoint;
    public int CurrentWaypoint;

    public CreatureAI_Zombie_Chow(ref WS_Creatures.CreatureObject Creature) : base(ref Creature)
    {
        AllowedMove = false;
        Creature.Flying = false;
        Creature.VisibleDistance = 700f;
    }

    public override void OnThink()
    {
        NextInfectedWound -= AI_UPDATE;
        if (NextInfectedWound <= 0) // Not really Blizzlike, but it'll make the zombies more blizzlike.
        {
            NextInfectedWound = Infected_Wound_CD;
            aiCreature.CastSpell(Spell_Infected_Wound, aiTarget);
        }
    }

    public void CastInfectedWound()
    {
        for (var i = 0; i <= 0; i++)
        {
            WS_Base.BaseUnit target = aiCreature;
            if (target is null)
            {
                return;
            }

            aiCreature.CastSpell(Spell_Infected_Wound, aiTarget);
        }
    }

    public void HealGluth(ref WS_Creatures.CreatureObject NPC_Gluth, ref WS_Creatures.CreatureObject Zombie_Chow)
    {
        coords Waypoint1 = new()
        {
            X = 3304.919922d,
            Y = 3139.149902d,
            Z = 296.890015d,
            Orientation = 1.33d
        };
        aiCreature.MoveTo((float)Waypoint1.X, (float)Waypoint1.Y, (float)Waypoint1.Z, (float)Waypoint1.Orientation);
        if (Conversions.ToBoolean(aiCreature.MoveTo((float)Waypoint1.X, (float)Waypoint1.Y, (float)Waypoint1.Z, (float)Waypoint1.Orientation, true)))
        {
            WS_Base.BaseUnit argAttacker = null;
            aiCreature.Heal(50000, Attacker: argAttacker);
        }
    }

    private struct coords
    {
        public double X;
        public double Y;
        public double Z;
        public double Orientation;
    }
}
