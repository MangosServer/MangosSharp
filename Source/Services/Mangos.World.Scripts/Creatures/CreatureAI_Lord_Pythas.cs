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

namespace Mangos.World.Scripts.Creatures;

public class CreatureAI_Lord_Pythas : WS_Creatures_AI.BossAI
{
    private const int AI_UPDATE = 1000;
    private const int SLUMBER_CD = 10000;
    private const int Lightning_Bolt_CD = 6000;
    private const int Thunder_Clap_CD = 9000;
    private const int Thunderclap_Spell = 8147;
    private const int Slumber_Spell = 8040;
    private const int Healing_Spell = 23381;
    private const int Spell_Serpent_Form = 8041; // Not sure how this will work.
    private const int Spell_Lightning_Bolt = 9532;
    public int NextWaypoint;
    public int NextThunderClap;
    public int NextLightningBolt;
    public int NextSlumber;
    public int CurrentWaypoint;
    public int NextHealingTouch;

    public CreatureAI_Lord_Pythas(ref WS_Creatures.CreatureObject Creature) : base(ref Creature)
    {
        AllowedMove = false;
        Creature.Flying = false;
        Creature.VisibleDistance = 700f;
    }

    public override void OnEnterCombat()
    {
        base.OnEnterCombat();
        aiCreature.SendChatMessage("The coils of death... Will crush you.", ChatMsg.CHAT_MSG_YELL, LANGUAGES.LANG_GLOBAL); // If you can do anything, then go serpent form.
    }

    public override void OnThink()
    {
        NextLightningBolt -= AI_UPDATE;
        NextHealingTouch -= AI_UPDATE;
        NextSlumber -= AI_UPDATE;
        NextThunderClap -= AI_UPDATE;
        if (NextLightningBolt <= 0)
        {
            NextLightningBolt = Lightning_Bolt_CD;
            aiCreature.CastSpell(Spell_Lightning_Bolt, aiTarget); // Lightning bolt on current target.
        }

        if (NextSlumber <= 0)
        {
            NextSlumber = SLUMBER_CD;
            aiCreature.CastSpell(Slumber_Spell, aiCreature.GetRandomTarget());
        }

        if (NextThunderClap <= 0)
        {
            NextThunderClap = Thunder_Clap_CD;
            aiCreature.CastSpell(Thunderclap_Spell, aiTarget); // TODO: Make this an AoE cast.
        }
    }

    public void CastLightning()
    {
        for (var i = 0; i <= 3; i++)
        {
            WS_Base.BaseUnit Target = aiCreature;
            if (Target is null)
            {
                return;
            }

            aiCreature.CastSpell(Spell_Lightning_Bolt, aiTarget);
        }
    }

    public void CastSlumber()
    {
        for (var i = 1; i <= 3; i++)
        {
            var target = aiCreature.GetRandomTarget();
            if (target is null)
            {
                return;
            }
        }

        aiCreature.CastSpell(Slumber_Spell, aiCreature.GetRandomTarget());
    }

    public void CastThunderClap()
    {
        for (var i = 2; i <= 3; i++)
        {
            WS_Base.BaseUnit target = aiCreature;
            if (target is null)
            {
                return;
            }
        }

        aiCreature.CastSpell(Thunder_Clap_CD, aiTarget);
    }

    public override void OnHealthChange(int Percent)
    {
        base.OnHealthChange(Percent);
        if (Percent <= 10)
        {
            try
            {
                aiCreature.CastSpellOnSelf(Healing_Spell);
            }
            catch (Exception)
            {
                aiCreature.SendChatMessage("I was unable to cast healing touch on myself. This is a problem. Please report this to the developers.", ChatMsg.CHAT_MSG_YELL, LANGUAGES.LANG_GLOBAL);
            }
        }
    }
}
