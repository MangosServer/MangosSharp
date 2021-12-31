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

public class CreatureAI_Patchwerk : WS_Creatures_AI.BossAI
{
    private const int AI_UPDATE = 1000;
    private const int BERSERK_COOLDOWN = 420000; // Heavy enrage, cuts through raid like butter.
    private const int FRENZY_COOLDOWN = 150000; // "soft" enrage, pops at 5%, no need to be recasted.
                                                // Private Const SUMMONPLAYER_COOLDOWN As Integer = 6000 'This might be an unoffical spell! Recheck at https://github.com/mangoszero/scripts/blob/master/scripts/eastern_kingdoms/naxxramas/boss_patchwerk.cpp
                                                // Private Const HATEFUL_STRIKE_COOLDOWN As Integer = 1000 'This is by far the biggest group breaker. Won't have this working for a VERY long time.

    private const int BERSERK_SPELL = 26662;
    private const int FRENZY_SPELL = 28131;
    // Private Const SUMMONPLAYER_SPELL As Integer = 20477
    // Private Const HATEFUL_STRIKE As Integer = 28308 - See cooldown for more information.

    public int Phase;
    public int NextWaypoint;
    public int NextBerserk;
    public int NextFrenzy;

    // Public NextSummon As Integer = 0
    public int CurrentWaypoint;

    public CreatureAI_Patchwerk(ref WS_Creatures.CreatureObject Creature) : base(ref Creature)
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
        aiCreature.SendChatMessage("Patchwerk want to play!", ChatMsg.CHAT_MSG_MONSTER_YELL, LANGUAGES.LANG_UNIVERSAL);
        aiCreature.SendPlaySound(8909, true);
    }

    public override void OnLeaveCombat(bool Reset = true)
    {
        base.OnLeaveCombat(Reset);
        AllowedAttack = true;
        Phase = 0;
        aiCreature.SendChatMessage("LEAVING COMBAT!", ChatMsg.CHAT_MSG_MONSTER_YELL, LANGUAGES.LANG_UNIVERSAL);
    }

    public override void OnKill(ref WS_Base.BaseUnit Victim)
    {
        aiCreature.SendChatMessage("No more play?", ChatMsg.CHAT_MSG_MONSTER_YELL, LANGUAGES.LANG_UNIVERSAL);
        aiCreature.SendPlaySound(8912, true);
    }

    public override void OnDeath()
    {
        aiCreature.SendChatMessage("What happened to.. Patch..", ChatMsg.CHAT_MSG_MONSTER_YELL, LANGUAGES.LANG_UNIVERSAL);
        aiCreature.SendPlaySound(8911, true);
    }

    public override void OnThink()
    {
        if (Phase < 1)
        {
            return;
        }

        if (Phase == 1)
        {
            NextBerserk -= AI_UPDATE;
            NextFrenzy -= AI_UPDATE;
            // NextSummon -= AI_UPDATE

            if (NextBerserk <= 0)
            {
                NextBerserk = BERSERK_COOLDOWN;
                aiCreature.CastSpellOnSelf(BERSERK_SPELL); // Berserk
            }

            if (NextFrenzy <= 1)
            {
                NextFrenzy = FRENZY_COOLDOWN;
                aiCreature.CastSpellOnSelf(FRENZY_SPELL); // Frenzy
            }

            // If NextSummon <= 0 Then
            // NextSummon = SUMMONPLAYER_COOLDOWN
            // aiCreature.CastSpell(SUMMONPLAYER_SPELL, aiTarget) 'Summon Player
            // End If
        }

        if (NextWaypoint > 0)
        {
            NextWaypoint -= AI_UPDATE;
            if (NextWaypoint <= 0)
            {
                On_Waypoint();
            }
        }
    }

    public override void OnHealthChange(int Percent)
    {
        base.OnHealthChange(Percent);
        if (Percent <= 5)
        {
            aiCreature.CastSpellOnSelf(FRENZY_SPELL);
        }
    }

    public void CastBerserk()
    {
        for (var i = 0; i <= 1; i++)
        {
            WS_Base.BaseUnit Self = aiCreature;
            if (Self is null)
            {
                return;
            }

            try
            {
                aiCreature.CastSpellOnSelf(BERSERK_SPELL);
            }
            catch (Exception)
            {
                // Log.WriteLine(LogType.WARNING, "BERSERK FAILED TO CAST ON PATCHWERK!")
                aiCreature.SendChatMessage("BERSERK FAILED TO CAST ON PATCHWERK! Please report this to the DEV'S!", ChatMsg.CHAT_MSG_MONSTER_YELL, LANGUAGES.LANG_UNIVERSAL);
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

    public void On_Waypoint() // Waypoints will definitely need some adjustments, but these should hold for now.
    {
        switch (CurrentWaypoint)
        {
            case 0:
                {
                    NextWaypoint = aiCreature.MoveTo(3261.996582f, 3228.5979f, 294.063354f, 2.53919f, true); // Coordinates do not work, but they are accurate for when we figure out how to move Patchwerk.
                    break;
                }

            case 1:
                {
                    NextWaypoint = 10000;
                    // NextSummon = NextWaypoint
                    aiCreature.MoveTo(316.822021f, 3149.243652f, 294.063354f, 2.437088f);
                    break;
                }

            case 2:
                {
                    NextWaypoint = 23000;
                    break;
                }

            case 3:
                {
                    NextWaypoint = 10000;
                    aiCreature.MoveTo(3130.012695f, 3141.432861f, 294.063354f, 3.364644f);
                    break;
                }

            case 4:
            case 6:
            case 8:
            case 10:
            case 12:
                {
                    NextWaypoint = 23000;
                    break;
                }

            case 5:
                {
                    NextWaypoint = 10000;
                    aiCreature.MoveTo(3162.650635f, 3152.284912f, 294.063354f, 5.952532f);
                    break;
                }

            case 7:
                {
                    NextWaypoint = 10000;
                    aiCreature.MoveTo(3248.241211f, 3226.086426f, 294.063354f, 5.571616f);
                    break;
                }

            case 9:
                {
                    NextWaypoint = 10000;
                    aiCreature.MoveTo(3313.826904f, 3231.999512f, 294.063354f, 6.231347f);
                    break;
                }
        }

        CurrentWaypoint += 1;
        if (CurrentWaypoint > 11)
        {
            CurrentWaypoint = 3;
        }
    }
}
