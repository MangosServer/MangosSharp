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
using Mangos.Common.Enums.Misc;
using Mangos.World.AI;
using Mangos.World.Objects;
using System;

// AI TODO: Implement a workaround (Or fix, fixes work too!) for Armageddon.
namespace Mangos.World.Scripts.Creatures;

public class CreatureAI_Baron_Geddon : WS_Creatures_AI.BossAI
{
    private const int AI_UPDATE = 1000;
    private const int Inferno_CD = 45000;
    private const int Ignite_CD = 30000;
    private const int Living_Bomb_CD = 35000;
    private const int Spell_Inferno = 19695;
    private const int Spell_Ignite = 19659; // Drains a random targets mana.
    private const int Spell_Living_Bomb = 20475;
    private const int Spell_Armageddon = 20478; // Cast at 2% to make self invincible, this spell won't work so we'll make a workaround.
    public int NextWaypoint;
    public int CurrentWaypoint;
    public int NextInferno;
    public int NextIgnite;
    public int NextLivingBomb;

    public CreatureAI_Baron_Geddon(ref WS_Creatures.CreatureObject Creature) : base(ref Creature)
    {
        AllowedMove = false;
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
            aiCreature.CastSpell(Spell_Inferno, aiTarget);
        }

        if (NextIgnite <= 0)
        {
            NextIgnite = Ignite_CD;
            aiCreature.CastSpell(Spell_Ignite, aiCreature.GetRandomTarget());
        }

        if (NextLivingBomb <= 0)
        {
            NextLivingBomb = Living_Bomb_CD;
            aiCreature.CastSpell(Spell_Living_Bomb, aiCreature.GetRandomTarget());
        }
    }

    public void CastInferno()
    {
        for (var i = 0; i <= 3; i++)
        {
            var Target = aiTarget;
            if (Target is null)
            {
                return;
            }

            aiCreature.CastSpell(Spell_Inferno, aiTarget); // This spell should be mitigated with fire resistance and nothing more.
        }
    }

    public void CastIgnite()
    {
        for (var i = 1; i <= 3; i++)
        {
            var target = aiCreature.GetRandomTarget();
            if (target is null)
            {
                return;
            }

            aiCreature.CastSpell(Spell_Ignite, aiCreature.GetRandomTarget()); // This spell drains 400 mana per second and MUST be dispelled immediately or your healers will wipe the group.
        }
    }

    public void CastLivingBomb()
    {
        for (var i = 2; i <= 3; i++)
        {
            var target = aiCreature.GetRandomTarget();
            if (target is null)
            {
                return;
            }

            aiCreature.CastSpell(Spell_Living_Bomb, aiCreature.GetRandomTarget()); // The traditional way of getting away of this is to run where the dead trash is from your group so they don't die, but we may need to fix AoE implementations for this.
        }
    }

    public override void OnHealthChange(int Percent)
    {
        base.OnHealthChange(Percent);
        if (Percent <= 2)
        {
            try
            {
                aiCreature.CastSpellOnSelf(Spell_Armageddon); // I think during this time he's supposed to have a kamakazie of sorts.
            }
            catch (Exception)
            {
                aiCreature.SendChatMessage("I have failed to become invincible at 2% or less HP. this is a problem.", ChatMsg.CHAT_MSG_YELL, LANGUAGES.LANG_GLOBAL);
            }
        }
    }
}
