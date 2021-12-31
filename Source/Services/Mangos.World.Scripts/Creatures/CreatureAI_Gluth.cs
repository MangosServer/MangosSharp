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

public class CreatureAI_Gluth : WS_Creatures_AI.BossAI
{
    // TODO: Implement proper zombie chow summons. Fix decimate. Fix him going underground. Fix mortal wound a debuff instead of dispellable buff. Fix terrifying roar.
    // Reference: https://github.com/mangoszero/scripts/blob/master/scripts/eastern_kingdoms/naxxramas/boss_gluth.cpp
    private const int AI_UPDATE = 1000;

    private const int Mortal_Wound_CD = 10000;
    private const int Decimate_CD = 110000;
    private const int Frenzy_CD = 25000;
    private const int Terrifying_Roar_CD = 15000;
    private const int Zombie_Chow_CD = 22000;
    private const int Spell_Decimate = 28374; // This may not work, not sure if the spell ID is pre-TBC or not.
    private const int Spell_Frenzy = 28131;
    private const int Spell_Mortal_Wound = 25646;
    private const int Spell_Terrifying_Roar = 29685;
    private const int NPC_Zombie_Chow = 16360;
    // private Const Spell_Summon_Zombie_Chow As Integer = 28216 - Seems to be removed from DBC..
    // Private Const Spell_Call_All_Zombie_Chow As Integer = 29681 - Seems to be removed from DBC..
    // Private Const spell_zombie_chow_search As Integer = 28235 - Seems to be removed from DBC..

    public int NextMortalWound;
    public int NextDecimate;
    public int NextFrenzy;
    public int NextRoar;
    public int NextWaypoint;
    public int CurrentWaypoint;

    public CreatureAI_Gluth(ref WS_Creatures.CreatureObject Creature) : base(ref Creature)
    {
        AllowedMove = false;
        Creature.Flying = false;
        Creature.VisibleDistance = 700f;
    }

    public override void OnThink()
    {
        NextDecimate -= AI_UPDATE;
        NextFrenzy -= AI_UPDATE;
        NextMortalWound -= AI_UPDATE;
        NextRoar -= AI_UPDATE;
        if (NextDecimate <= 0)
        {
            NextDecimate = Decimate_CD;
            aiCreature.CastSpell(Spell_Decimate, aiTarget); // Earthborer Acid
        }

        if (NextFrenzy <= 1)
        {
            NextFrenzy = Frenzy_CD;
            aiCreature.CastSpellOnSelf(Spell_Frenzy);
        }

        if (NextMortalWound <= 2)
        {
            NextMortalWound = Mortal_Wound_CD;
            aiCreature.CastSpell(Spell_Mortal_Wound, aiTarget);
        }

        if (NextRoar <= 3)
        {
            NextRoar = Terrifying_Roar_CD;
            aiCreature.CastSpell(Spell_Terrifying_Roar, aiTarget);
        }
    }

    public void CastDecimate()
    {
        for (var i = 0; i <= 3; i++)
        {
            WS_Base.BaseUnit Target = aiCreature;
            if (Target is null)
            {
                return;
            }

            try
            {
                aiCreature.CastSpell(Spell_Decimate, aiTarget);
            }
            catch (Exception)
            {
                aiCreature.SendChatMessage("I have failed to cast decimate. Whoever made this script is bad. Please report this to the developers.", ChatMsg.CHAT_MSG_YELL, LANGUAGES.LANG_GLOBAL);
            }
        }
    }

    public void CastFrenzy()
    {
        for (var i = 1; i <= 3; i++)
        {
            WS_Base.BaseUnit Target = aiCreature;
            if (Target is null)
            {
                return;
            }

            try
            {
                aiCreature.CastSpellOnSelf(Spell_Frenzy);
            }
            catch (Exception)
            {
                aiCreature.SendChatMessage("I have failed to cast Frenzy. Whoever made this script did a poor job, please report this to the developers.", ChatMsg.CHAT_MSG_YELL, LANGUAGES.LANG_GLOBAL);
            }
        }
    }

    public void CastMortalWound()
    {
        for (var i = 2; i <= 3; i++)
        {
            WS_Base.BaseUnit target = aiCreature;
            if (target is null)
            {
                return;
            }

            try
            {
                aiCreature.CastSpell(Spell_Mortal_Wound, aiTarget);
            }
            catch (Exception)
            {
                aiCreature.SendChatMessage("I have failed to cast Mortal Wound. Whoever made this script did a poor job, please report this to the developers.", ChatMsg.CHAT_MSG_YELL, LANGUAGES.LANG_GLOBAL);
            }
        }
    }

    public void CastTerrifyingRoar()
    {
        for (var i = 3; i <= 3; i++)
        {
            WS_Base.BaseUnit target = aiCreature;
            if (target is null)
            {
                return;
            }

            try
            {
                aiCreature.CastSpell(Spell_Terrifying_Roar, aiTarget);
            }
            catch (Exception)
            {
                aiCreature.SendChatMessage("I have failed to cast terrifying roar. Whoever made this script did a poor job, please report this to the developers.", ChatMsg.CHAT_MSG_YELL, LANGUAGES.LANG_GLOBAL);
            }
        }
    }

    public override void OnDeath()
    {
        base.OnDeath();
        aiCreature.SendChatMessage("I have successfully been slain. Good job!", ChatMsg.CHAT_MSG_YELL, LANGUAGES.LANG_GLOBAL);
    }

    public void SpawnZombieChow(int Count)
    {
        for (int i = 1, loopTo = Count; i <= loopTo; i++)
        {
            if (Zombie_Chow_CD <= 0)
            {
                aiCreature.SpawnCreature(16360, 3267.9f, -3172.1f, 297.42f);
            }
        }
    }

    // Gluth falls under Naxxramas without this. Not perfect but much better than before. Please note this was tested on a server with no vmaps/maps and that may be why he falls under Naxxramas.
    public override void OnLeaveCombat(bool Reset = true)
    {
        base.OnLeaveCombat(Reset);
        aiCreature.MoveTo(3304.269f, (float)-3136.414d, 296.7151f, 0.8140599f);
    }
}
