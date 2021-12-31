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

using Mangos.Common.Enums.Global;
using Mangos.World.AI;
using Mangos.World.Objects;
using System;

// Summon implementation isn't yet supported.
// Sand trap not implemented into script, need to make a gameobject I assume.
namespace Mangos.World.Scripts.Creatures;

public class CreatureAI_Kurinnax : WS_Creatures_AI.BossAI
{
    private const int AI_UPDATE = 1000;
    private const int wound_cooldown = 8000;

    // private const summon_player_cooldown As Integer = 5000
    // Private Const summon_player_cooldown2 As Integer = 5001
    // Not sure if has correct core support or on cooldowns.
    private const int Thrash_Cooldown = 9000;

    private const int Wide_Slash_Cooldown = 10000;
    private const int Spell_Frenzy = 26527;
    private const int Spell_Mortal_Wound = 25646;

    // Private Const Spell_Summon_1 As Integer = 20477 'Unused until we figure out how this works and what it does.
    // Private Const Spell_Summon_2 As Integer = 26446 'Same as above, unused until more is figured out.
    private const int spell_Thrash = 3391;

    private const int Spell_Wide_Slash = 25814;
    public int phase;
    public int Next_Mortal_Wound;
    public int Next_Thrash;
    public int Next_Wide_Slash;
    // Public Next_Summon_1 As Integer = 0
    // Public Next_Summon_2 As Integer = 0

    public CreatureAI_Kurinnax(ref WS_Creatures.CreatureObject Creature) : base(ref Creature)
    {
        phase = 0;
        AllowedMove = false;
        Creature.Flying = false;
        Creature.VisibleDistance = 700;
    }

    public override void OnEnterCombat()
    {
        if (phase > 1)
        {
            return;
        }

        base.OnEnterCombat();
        AllowedAttack = true;
        aiCreature.Flying = false;
        // ReinitSpells()
    }

    public override void OnLeaveCombat(bool Reset = true)
    {
        base.OnLeaveCombat(Reset);
        AllowedAttack = true;
        phase = 0;
    }

    public override void OnThink()
    {
        base.OnThink();
        if (phase < 1)
        {
            return;
        }

        if (phase is 1 or 3)
        {
        }

        Next_Mortal_Wound -= AI_UPDATE;
        Next_Thrash -= AI_UPDATE;
        Next_Wide_Slash -= AI_UPDATE;
        if (Next_Mortal_Wound <= 0)
        {
            Next_Mortal_Wound = wound_cooldown;
            try
            {
                aiCreature.CastSpell(Spell_Mortal_Wound, aiTarget);
            }
            catch (Exception)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Mortal Wound failed to cast!");
            }
        }

        if (Next_Thrash <= 1)
        {
            Next_Thrash = Thrash_Cooldown;
            aiCreature.CastSpell(spell_Thrash, aiTarget);
        }

        if (Next_Wide_Slash <= 2)
        {
            Next_Wide_Slash = Wide_Slash_Cooldown;
            aiCreature.CastSpell(spell_Thrash, aiTarget);
        }
    }

    public override void OnHealthChange(int Percent)
    {
        base.OnHealthChange(Percent);
        if (phase == 1)
        {
            if (Percent <= 20)
            {
                aiCreature.CastSpellOnSelf(Spell_Frenzy);
            }
        }
    }
}
