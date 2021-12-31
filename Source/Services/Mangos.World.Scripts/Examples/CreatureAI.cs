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

// Example AI for combat.
// TODO: Fix AoE spells on AIs and then insert it as an example into this.
namespace Mangos.World.Scripts.Examples;

public class CreatureAI : WS_Creatures_AI.BossAI
{
    private const int AI_UPDATE = 1000;
    private const int Knockdown_CD = 5000; // Cooldown for spells get listed under a private constant. The timer is followed in milliseconds.
    private const int Spell_Knockdown = 16790; // The spell is defined here. This is the equivilent of a MaNGOS C++ enumerator. This should work for NPCs and healing spells too.
    private const int Frost_Armor = 7302; // Self buff.
    public int NextWaypoint;
    public int NextKnockdown; // This will be called later, this is only needed along with the CD if you plan to have it recasted.
    public int CurrentWaypoint;

    public CreatureAI(ref WS_Creatures.CreatureObject Creature) : base(ref Creature) // The following under this are very self explanatory, on spawn the creature will not move by itself nor fly. It will be visible from far away. This can be changed.
    {
        AllowedMove = false;
        Creature.Flying = false;
        Creature.VisibleDistance = 700f;
    }

    public override void OnThink()
    {
        NextKnockdown -= AI_UPDATE; // The update is required for the AI to actually go on with anything.
        if (NextKnockdown <= 0) // This is the cooldown definition, this is not required to be used on a non-reused spell I believe.
        {
            NextKnockdown = Knockdown_CD;
            aiCreature.CastSpell(Spell_Knockdown, aiTarget); // aiTarget can be changed to select a random target by making it "aiCreature.GetRandomTarget".
        }
    }

    public void CastKnockdown() // This is where the spell is brought into actual usage.
    {
        for (var i = 0; i <= 3; i++) // I believe this number is capped by the amount of spells from 0-any number. We'll make it 0 to 3 here just to be safe. You should do the same.
        {
            WS_Base.BaseUnit Target = aiCreature;
            if (Target is null)
            {
                return; // If no player is targetted, don't cast knockdown.
            }

            aiCreature.CastSpell(Spell_Knockdown, aiTarget); // The casting of the spell. This will be casted on the selected target as defined previously.
        }
    }

    public override void OnEnterCombat() // Override sub is self explanatory. This is on the AIs entering of combat. Put anything here, for this we'll put a self buff.
    {
        base.OnEnterCombat();
        aiCreature.SendChatMessage("I have been pulled, so I send this message if I am inserted within the entercombat sub.", ChatMsg.CHAT_MSG_YELL, LANGUAGES.LANG_GLOBAL); // ChatMsg refers to the chat type, this can be changed to say, bg or anything else. Lang_Global is a global langauge that ALL can understand, this can be changed to any language. (Draenei, gnomish, etc.)
        aiCreature.CastSpellOnSelf(Frost_Armor); // Casts frost armor upon self when entering combat.
    }

    public override void OnDeath() // Same as enter combat, except casting on self during death will probably lead to a core crash.
    {
        base.OnDeath();
        aiCreature.SendChatMessage("Y u do dat. I'm dead and tell you this because my sent chat message was set under the OnDeath() sub.", ChatMsg.CHAT_MSG_YELL, LANGUAGES.LANG_GLOBAL);
    }

    public override void OnHealthChange(int Percent) // gets alot more tricky here. Just follow along and you'll understand.
    {
        base.OnHealthChange(Percent);
        if (Percent <= 50) // If health is equal to or less than 50, it'll continue with the code. Otherwise it won't.
        {
            aiCreature.SendChatMessage("You have reduced me to 50% health. I will try harder now to ensure your death. Good job on inserting this into the onhealthchange percent sub though. That's why I'm talking! =]", ChatMsg.CHAT_MSG_YELL, LANGUAGES.LANG_GLOBAL);
        }
    }
}
