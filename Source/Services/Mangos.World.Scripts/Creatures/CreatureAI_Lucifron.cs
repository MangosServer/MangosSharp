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

public class CreatureAI_Lucifron : WS_Creatures_AI.BossAI
{
    private const int AI_UPDATE = 1000;
    private const int Impending_Doom_Cooldown = 20000;
    private const int Lucifrons_Curse_Cooldown = 20000;
    private const int Shadow_Shock_Cooldown = 6000;
    private const int Impending_Doom = 19702;
    private const int Lucifrons_Curse = 19703;
    private const int Shadow_Shock = 19460;
    public int Phase;
    public int NextImpendingDoom;
    public int NextLucifronsCurse;
    public int NextShadowShock;
    public int NextWaypoint;
    public int CurrentWaypoint;

    public CreatureAI_Lucifron(ref WS_Creatures.CreatureObject Creature) : base(ref Creature)
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
        // Does he cast a dummy spell on target death?
    }

    public override void OnDeath()
    {
        // Does he do anything on his own death?
    }

    public override void OnThink()
    {
        if (Phase < 1)
        {
            return;
        }

        if (Phase == 1)
        {
            NextImpendingDoom -= AI_UPDATE;
            NextLucifronsCurse -= AI_UPDATE;
            NextShadowShock -= AI_UPDATE;
            if (NextImpendingDoom <= 0)
            {
                NextImpendingDoom = Impending_Doom_Cooldown;
                aiCreature.CastSpell(Impending_Doom, aiTarget); // Impending DOOOOOM!
            }

            if (NextLucifronsCurse <= 0)
            {
                NextLucifronsCurse = Lucifrons_Curse_Cooldown;
                aiCreature.CastSpell(Lucifrons_Curse, aiTarget); // Lucifrons Curse.
            }

            if (NextShadowShock <= 0)
            {
                NextShadowShock = Shadow_Shock_Cooldown;
                aiCreature.CastSpell(Shadow_Shock, aiTarget); // Summon Player
            }
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

    public void Cast_Lucirons_Curse()
    {
        for (var i = 0; i <= 2; i++)
        {
            WS_Base.BaseUnit theTarget = aiCreature;
            if (theTarget is null)
            {
                return;
            }

            try
            {
                aiCreature.CastSpell(Lucifrons_Curse, aiTarget);
            }
            catch (Exception)
            {
                aiCreature.SendChatMessage("Failed to cast Lucifron's Curse. This is bad. Please report to developers.", ChatMsg.CHAT_MSG_MONSTER_YELL, LANGUAGES.LANG_UNIVERSAL);
            }
        }
    }

    public void Cast_Impending_Doom()
    {
        for (var i = 1; i <= 2; i++)
        {
            WS_Base.BaseUnit theTarget = aiCreature;
            if (theTarget is null)
            {
                return;
            }

            try
            {
                aiCreature.CastSpell(Impending_Doom, aiTarget);
            }
            catch (Exception)
            {
                aiCreature.SendChatMessage("Failed to cast IMPENDING DOOOOOM! Please report this to a developer.", ChatMsg.CHAT_MSG_MONSTER_YELL, LANGUAGES.LANG_UNIVERSAL);
            }
        }
    }

    public void Cast_Shadow_Shock()
    {
        for (var i = 2; i <= 2; i++)
        {
            var theTarget = aiCreature.GetRandomTarget();
            if (theTarget is null)
            {
                return;
            }

            try
            {
                aiCreature.CastSpell(Shadow_Shock, theTarget.positionX, theTarget.positionY, theTarget.positionZ);
            }
            catch (Exception)
            {
                aiCreature.SendChatMessage("Failed to cast Shadow Shock. Please report this to a developer.", ChatMsg.CHAT_MSG_MONSTER_YELL, LANGUAGES.LANG_UNIVERSAL);
            }
        }
    }

    public void On_Waypoint()
    {
        switch (CurrentWaypoint)
        {
            case 0:
                {
                    NextWaypoint = aiCreature.MoveTo(-0.0f, -0.0f, -0.0f, 0.0f, true); // No Waypoint Coords! Will need to back track from MaNGOS!
                    break;
                }

            case 1:
                {
                    NextWaypoint = 10000;
                    // NextSummon = NextWaypoint
                    aiCreature.MoveTo(0.0f, -0.0f, -0.0f);
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
                    aiCreature.MoveTo(0.0f, -0.0f, -0.0f);
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
                    aiCreature.MoveTo(-0.0f, -0.0f, -0.0f);
                    break;
                }

            case 7:
                {
                    NextWaypoint = 10000;
                    aiCreature.MoveTo(-0.0f, -0.0f, -0.0f);
                    break;
                }

            case 9:
                {
                    NextWaypoint = 10000;
                    aiCreature.MoveTo(0.0f, -0.0f, -0.0f);
                    break;
                }

            case 11:
                {
                    NextWaypoint = 10000;
                    aiCreature.MoveTo(-0.0f, -0.0f, -0.0f);
                    break;
                }
        }

        CurrentWaypoint += 1;
        if (CurrentWaypoint > 12)
        {
            CurrentWaypoint = 3;
        }
    }
}
