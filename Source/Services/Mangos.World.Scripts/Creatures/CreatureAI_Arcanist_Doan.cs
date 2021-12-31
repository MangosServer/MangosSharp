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

public class CreatureAI_Arcanist_Doan : WS_Creatures_AI.BossAI
{
    // AI TODO: Fix arcane explosion. Make the AoE silence an AoE instead of random target.
    private const int AI_UPDATE = 1000;

    private const int ARCANE_BUBBLE_CD = 500000; // This should never be recasted.
    private const int DETONATION_CD = 500000; // This should never be recasted.

    // Private Const Arcane_Explosion_CD As Integer = 6000
    private const int Silence_CD = 9000;

    private const int Polymorph_CD = 15000;
    private const int SPELL_POLYMORPH = 13323;
    private const int SPELL_SILENCE = 8988;
    private const int SPELL_DETONATION = 9435;
    private const int SPELL_ARCANE_BUBBLE = 9438;
    // Private Const SPELL_ARCANE_EXPLOSION As Integer = 34517 'SPELL UNSUPPORTED, CAUSES CRASHES

    public int NextDetonation; // Again, this should never be reused.
    public int NextArcaneBubble; // Again, this should never be reused.
    public int NextPolymorph;
    public int NextSilence;
    public int NextWaypoint;
    public int NextAcid;
    public int CurrentWaypoint;
    // Public NextExplosion As Integer = 0

    public CreatureAI_Arcanist_Doan(ref WS_Creatures.CreatureObject Creature) : base(ref Creature)
    {
        AllowedMove = false;
        Creature.Flying = false;
        Creature.VisibleDistance = 700f;
    }

    public override void OnThink()
    {
        // NextExplosion -= AI_UPDATE
        NextPolymorph -= AI_UPDATE;
        NextSilence -= AI_UPDATE;
        NextArcaneBubble -= AI_UPDATE;
        NextDetonation -= AI_UPDATE;

        // If NextExplosion <= 0 Then  'Need to implement better AoE handling. This may be the reason for lack of boss casts with AoE spells.
        // NextExplosion = Arcane_Explosion_CD
        // aiCreature.CastSpell(SPELL_ARCANE_EXPLOSION, aiTarget)
        // End If

        if (NextPolymorph <= 0)
        {
            NextPolymorph = Polymorph_CD;
            aiCreature.CastSpell(SPELL_POLYMORPH, aiCreature.GetRandomTarget());
        }

        if (NextSilence <= 0)
        {
            NextSilence = Silence_CD;
            aiCreature.CastSpell(SPELL_SILENCE, aiCreature.GetRandomTarget());
        }
        // No need to handle Detonation or Bubble here.
    }

    // Public Sub CastExplosion() - This is commented out because Arcane Explosion completely crashes the core.
    // For i As Integer = 0 To 2
    // Dim Target As BaseUnit = aiCreature
    // If Target Is Nothing Then Exit Sub
    // Try
    // aiCreature.CastSpell(SPELL_ARCANE_EXPLOSION, aiTarget)
    // Catch ex As Exception
    // aiCreature.SendChatMessage("Failed to cast Arcane Explosion. Please report this to a developer!", ChatMsg.CHAT_MSG_YELL, LANGUAGES.LANG_GLOBAL)
    // End Try
    // Next
    // End Sub

    public void CastPolymorph()
    {
        for (var i = 1; i <= 2; i++)
        {
            var target = aiCreature.GetRandomTarget(); // Finally learned how random target functions work.
            if (target is null)
            {
                return;
            }

            try
            {
                aiCreature.CastSpell(SPELL_POLYMORPH, aiCreature.GetRandomTarget()); // Might not properly work.
            }
            catch (Exception)
            {
                aiCreature.SendChatMessage("I was unable to cast polymorph. Please report this to a developer!", ChatMsg.CHAT_MSG_YELL, LANGUAGES.LANG_GLOBAL);
            }
        }
    }

    public void CastSilence()
    {
        for (var i = 2; i <= 2; i++)
        {
            var target = aiCreature.GetRandomTarget();
            if (target is null)
            {
                return;
            }

            try
            {
                aiCreature.CastSpell(SPELL_SILENCE, aiCreature.GetRandomTarget());
            }
            catch (Exception)
            {
                aiCreature.SendChatMessage("I was unable to silence my target. Please report this to a developer!", ChatMsg.CHAT_MSG_YELL, LANGUAGES.LANG_GLOBAL);
            }
        }
    }

    public override void OnHealthChange(int Percent)
    {
        base.OnHealthChange(Percent);
        if (Percent <= 50)
        {
            aiCreature.MoveToInstant(148.403458f, (float)-429.035919d, 18.485929f, 0.002225f);
            aiCreature.CastSpellOnSelf(SPELL_ARCANE_BUBBLE);
        }

        if (aiCreature.HaveAura(SPELL_ARCANE_BUBBLE)) // Workaround for casting detonation.
        {
            try
            {
                aiCreature.CastSpell(SPELL_DETONATION, aiTarget);
            }
            catch (Exception)
            {
                aiCreature.SendChatMessage("I was unable to cast Arcane Bubble upon myself. Please report this to a developer!", ChatMsg.CHAT_MSG_YELL, LANGUAGES.LANG_GLOBAL);
            }

            aiCreature.SendPlaySound(5843, true); // Should play sound when he says "Burn in righteous fire!"
            aiCreature.SendChatMessage("Burn in righteous fire!", ChatMsg.CHAT_MSG_YELL, LANGUAGES.LANG_GLOBAL);
        }
    }

    public override void OnEnterCombat()
    {
        base.OnEnterCombat();
        aiCreature.SendChatMessage("You will not defile these mysteries!", ChatMsg.CHAT_MSG_YELL, LANGUAGES.LANG_GLOBAL);
        aiCreature.SendPlaySound(5842, true);
    }
}
