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

public class CreatureAI_Golemagg_the_Incinerator : WS_Creatures_AI.BossAI
{
    private const int AI_UPDATE = 1000;
    private const int EARTHQUAKE_COOLDOWN = 10000;
    private const int MAGMASPLASH_COOLDOWN = 5000;
    private const int PYROBLAST_COOLDOWN = 20000;
    // Private Const SUMMONPLAYER_COOLDOWN As Integer = 20000

    private const int EARTHQUAKE_SPELL = 19798;
    private const int MAGMASPLASH_SPELL = 28131;
    private const int PYROBLAST_SPELL = 20228;
    // Private Const SUMMONPLAYER_SPELL As Integer = 20477

    public int Phase;
    public int NextWaypoint;
    public int NextEarthQuake;
    public int NextMagmaSplash;
    public int NextPyroBlast;

    // Public NextSummon As Integer = 0
    public int CurrentWaypoint;

    public CreatureAI_Golemagg_the_Incinerator(ref WS_Creatures.CreatureObject Creature) : base(ref Creature)
    {
        Phase = 0;
        AllowedMove = false;
        Creature.Flying = false;
        Creature.VisibleDistance = 700f;
    }

    public override void OnEnterCombat()
    {
        if (Phase > 1)
        {
            return;
        }

        base.OnEnterCombat();
        aiCreature.Flying = false;
        AllowedAttack = true;
        Phase = 1;
        // ReinitSpells()
    }

    public override void OnLeaveCombat(bool Reset = true)
    {
        base.OnLeaveCombat(Reset);
        AllowedAttack = true;
        Phase = 0;
    }

    public override void OnKill(ref WS_Base.BaseUnit Victim)
    {
        // This is only here for if something is needed when a target is killed, Golemagg doesn't have a yell.
    }

    public override void OnDeath()
    {
        // Does Golemagg give loot on death or cast a dummy spell?
    }

    public override void OnThink()
    {
        if (Phase < 1)
        {
            return;
        }

        if (Phase == 1)
        {
            NextEarthQuake -= AI_UPDATE;
            NextMagmaSplash -= AI_UPDATE;
            NextPyroBlast -= AI_UPDATE;
            // NextSummon -= AI_UPDATE

            if (NextEarthQuake <= 0)
            {
                NextEarthQuake = EARTHQUAKE_COOLDOWN;
                aiCreature.CastSpell(EARTHQUAKE_SPELL, aiTarget); // EarthQuake
            }

            if (NextMagmaSplash <= 1)
            {
                NextMagmaSplash = MAGMASPLASH_COOLDOWN;
                aiCreature.CastSpell(MAGMASPLASH_SPELL, aiTarget); // MagmaSplash
            }

            if (NextPyroBlast <= 2)
            {
                NextPyroBlast = PYROBLAST_COOLDOWN;
                aiCreature.CastSpell(PYROBLAST_SPELL, aiTarget); // PyroBlast
            }
            // If NextSummon <= 0 Then
            // NextSummon = SUMMONPLAYER_COOLDOWN
            // aiCreature.CastSpell(SUMMONPLAYER_SPELL, aiTarget) 'Summon Player
            // End If
        }
    }

    public void CastEarthQuake()
    {
        for (var i = 0; i <= 2; i++)
        {
            WS_Base.BaseUnit Target = aiCreature;
            if (Target is null)
            {
                return;
            }

            try
            {
                aiCreature.CastSpell(EARTHQUAKE_SPELL, aiTarget);
            }
            catch (Exception)
            {
                aiCreature.SendChatMessage("Earthquake FAILED TO CAST ON MY TARGET! Please report this to the DEV'S!", ChatMsg.CHAT_MSG_MONSTER_YELL, LANGUAGES.LANG_UNIVERSAL);
            }
        }
    }

    public void CastMagmaSplash()
    {
        for (var i = 1; i <= 2; i++)
        {
            WS_Base.BaseUnit Target = aiCreature;
            if (Target is null)
            {
                return;
            }

            try
            {
                aiCreature.CastSpell(MAGMASPLASH_SPELL, aiTarget);
            }
            catch (Exception)
            {
                aiCreature.SendChatMessage("MAGMASPLASH FAILED TO CAST ON TARGET! Please report this to the DEV'S!", ChatMsg.CHAT_MSG_MONSTER_YELL, LANGUAGES.LANG_UNIVERSAL);
            }
        }
    }

    public void CastPyroBlast()
    {
        for (var i = 2; i <= 2; i++)
        {
            WS_Base.BaseUnit Target = aiCreature;
            if (Target is null)
            {
                return;
            }

            try
            {
                aiCreature.CastSpell(PYROBLAST_SPELL, aiTarget);
            }
            catch (Exception)
            {
                aiCreature.SendChatMessage("PYROBLAST FAILED TO CAST ON TARGET! Please report this to the DEV'S!", ChatMsg.CHAT_MSG_MONSTER_YELL, LANGUAGES.LANG_UNIVERSAL);
            }
        }
    }

    // Public Sub CastSummonPlayer()
    // For i As Integer = 0 To 3
    // Dim theTarget As BaseUnit = aiCreature.GetRandomTarget
    // If theTarget Is Nothing Then Exit Sub
    // Try
    // aiCreature.CastSpell(SUMMONPLAYER_SPELL, theTarget.positionX, theTarget.positionY, theTarget.positionZ)
    // Catch Ex As Exception
    // 'Log.WriteLine(LogType.WARNING, "SUMMON FAILED TO CAST ON TARGET!")
    // aiCreature.SendChatMessage("SUMMON FAILED TO CAST ON TARGET! Please report this to the DEV'S!", ChatMsg.CHAT_MSG_MONSTER_YELL, LANGUAGES.LANG_UNIVERSAL)
    // End Try
    // Next
    // End Sub
}
