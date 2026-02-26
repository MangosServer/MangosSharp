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

using Mangos.World.AI.Movement;
using Mangos.World.Objects;

namespace Mangos.World.AI;

/// <summary>
/// Enhanced creature AI interface inspired by TrinityCore's CreatureAI.
/// Provides ~15 hooks vs. the legacy TBaseAI's 7, with structured context data.
///
/// This is the long-term replacement target for TBaseAI. New boss scripts
/// and creature behaviors should implement this interface (via CreatureAIBase
/// or ScriptedAI) rather than extending DefaultAI/BossAI.
/// </summary>
public interface ICreatureAI
{
    /// <summary>
    /// Called each AI tick with the elapsed time since last tick.
    /// Replaces the parameterless DoThink().
    /// </summary>
    void UpdateAI(int diffMs);

    /// <summary>
    /// Called when the creature first enters combat with an attacker.
    /// Inspired by TrinityCore's JustEngagedWith(Unit* who).
    /// </summary>
    void JustEngagedWith(WS_Base.BaseUnit attacker);

    /// <summary>
    /// Called when the creature takes damage. The damage value can be modified via ref.
    /// Inspired by TrinityCore's DamageTaken(Unit* attacker, uint32&amp; damage).
    /// </summary>
    void DamageTaken(WS_Base.BaseUnit attacker, ref int damage);

    /// <summary>
    /// Called when the creature kills a unit.
    /// </summary>
    void KilledUnit(WS_Base.BaseUnit victim);

    /// <summary>
    /// Called when the creature dies.
    /// Inspired by TrinityCore's JustDied(Unit* killer).
    /// </summary>
    void JustDied(WS_Base.BaseUnit killer);

    /// <summary>
    /// Called when the creature respawns.
    /// </summary>
    void JustRespawned();

    /// <summary>
    /// Called when the creature enters evade mode (returns to spawn after losing all targets).
    /// </summary>
    void EnterEvadeMode();

    /// <summary>
    /// Called when the creature is hit by a spell.
    /// Inspired by TrinityCore's SpellHit(Unit* caster, SpellInfo const* spell).
    /// </summary>
    void SpellHit(WS_Base.BaseUnit caster, int spellId);

    /// <summary>
    /// Called when a movement generator completes (e.g., reached a waypoint or point).
    /// Inspired by TrinityCore's MovementInform(uint32 type, uint32 id).
    /// </summary>
    void MovementInform(MovementGeneratorType type, int pointId);

    /// <summary>
    /// Called when the creature's health changes significantly.
    /// Provides both old and new percentage for phase transition detection.
    /// </summary>
    void OnHealthChange(int oldPercent, int newPercent);

    /// <summary>
    /// Called when a creature summoned by this creature dies.
    /// </summary>
    void SummonedCreatureDies(WS_Creatures.CreatureObject summon, WS_Base.BaseUnit killer);

    /// <summary>
    /// Called when the creature leaves combat.
    /// </summary>
    void OnLeaveCombat(bool reset);

    /// <summary>
    /// Called when threat is generated against this creature.
    /// </summary>
    void OnGenerateHate(WS_Base.BaseUnit attacker, int hateValue);
}
