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

public class CreatureAI_Targorr_the_Dread : WS_Creatures_AI.BossAI
{
    private const int AI_UPDATE = 1000;
    private const int ThrashCD = 7000;
    private const int FrenzyCD = 90000; // This should never be reused.
    private const int Spell_Frenzy = 8599;
    private const int Spell_Thrash = 3391;
    public int NextThrash;
    public int NextWaypoint;
    public int NextAcid;
    public int CurrentWaypoint;

    public CreatureAI_Targorr_the_Dread(ref WS_Creatures.CreatureObject Creature) : base(ref Creature)
    {
        AllowedMove = false;
        Creature.Flying = false;
        Creature.VisibleDistance = 700f;
    }

    public override void OnThink()
    {
        NextThrash -= AI_UPDATE;
        if (NextThrash <= 0)
        {
            NextThrash = ThrashCD;
            aiCreature.CastSpellOnSelf(Spell_Thrash); // Should be cast on self. Correct me if wrong.
        }
    }

    public void CastThrash()
    {
        for (var i = 0; i <= 0; i++)
        {
            WS_Base.BaseUnit Target = aiCreature;
            if (Target is null)
            {
                return;
            }

            try
            {
                aiCreature.CastSpellOnSelf(Spell_Thrash);
            }
            catch (Exception)
            {
                aiCreature.SendChatMessage("AI was unable to cast Thrash on himself. Please report this to a developer.", ChatMsg.CHAT_MSG_YELL, LANGUAGES.LANG_GLOBAL);
            }
        }
    }

    public override void OnHealthChange(int Percent)
    {
        base.OnHealthChange(Percent);
        if (Percent <= 40)
        {
            try
            {
                aiCreature.CastSpellOnSelf(Spell_Frenzy);
            }
            catch (Exception)
            {
                aiCreature.SendChatMessage("AI was unable to cast Frenzy on himself. Please report this to a developer.", ChatMsg.CHAT_MSG_YELL, LANGUAGES.LANG_GLOBAL);
            }
        }
    }
}
