//
// Copyright (C) 2013-2025 getMaNGOS <https://www.getmangos.eu>
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
using Mangos.World.AI.Movement;
using Mangos.World.Objects;

namespace Mangos.World.AI;

/// <summary>
/// Abstract base class implementing ICreatureAI with default empty implementations
/// and convenience methods. Inspired by TrinityCore's CreatureAI base class.
///
/// Provides helper methods like DoCast(), Talk(), DoMeleeAttackIfReady() that
/// simplify boss and creature script development.
/// </summary>
public abstract class CreatureAIBase : ICreatureAI
{
    protected WS_Creatures.CreatureObject Me { get; }
    protected CooldownManager Cooldowns { get; }

    protected CreatureAIBase(WS_Creatures.CreatureObject creature)
    {
        Me = creature;
        Cooldowns = new CooldownManager();
    }

    public virtual void UpdateAI(int diffMs)
    {
        Cooldowns.Update(diffMs);
    }

    public virtual void JustEngagedWith(WS_Base.BaseUnit attacker) { }
    public virtual void DamageTaken(WS_Base.BaseUnit attacker, ref int damage) { }
    public virtual void KilledUnit(WS_Base.BaseUnit victim) { }
    public virtual void JustDied(WS_Base.BaseUnit killer) { }
    public virtual void JustRespawned() { }
    public virtual void EnterEvadeMode() { }
    public virtual void SpellHit(WS_Base.BaseUnit caster, int spellId) { }
    public virtual void MovementInform(MovementGeneratorType type, int pointId) { }
    public virtual void OnHealthChange(int oldPercent, int newPercent) { }
    public virtual void SummonedCreatureDies(WS_Creatures.CreatureObject summon, WS_Base.BaseUnit killer) { }
    public virtual void OnLeaveCombat(bool reset) { }
    public virtual void OnGenerateHate(WS_Base.BaseUnit attacker, int hateValue) { }

    /// <summary>
    /// Casts a spell on the given target.
    /// </summary>
    protected void DoCast(int spellId, WS_Base.BaseUnit target)
    {
        Me?.CastSpell(spellId, target);
    }

    /// <summary>
    /// Casts a spell on self.
    /// </summary>
    protected void DoCastSelf(int spellId)
    {
        Me?.CastSpellOnSelf(spellId);
    }

    /// <summary>
    /// Sends a chat message as the creature.
    /// </summary>
    protected void Talk(string text, ChatMsg chatType = ChatMsg.CHAT_MSG_MONSTER_YELL)
    {
        Me?.SendChatMessage(text, chatType, LANGUAGES.LANG_GLOBAL);
    }

    /// <summary>
    /// Gets the creature's current health percentage (0-100).
    /// </summary>
    protected int HealthPercent
    {
        get
        {
            if (Me == null) return 0;
            if (Me.Life.Maximum == 0) return 0;
            return checked((int)(Me.Life.Current * 100L / Me.Life.Maximum));
        }
    }
}
