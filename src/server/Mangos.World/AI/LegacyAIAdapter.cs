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
/// Adapter that wraps existing TBaseAI instances as ICreatureAI for backward compatibility.
/// Allows the new event system to dispatch to legacy AI scripts without modification.
/// </summary>
public sealed class LegacyAIAdapter : ICreatureAI
{
    private readonly WS_Creatures_AI.TBaseAI _legacyAI;

    public LegacyAIAdapter(WS_Creatures_AI.TBaseAI legacyAI)
    {
        _legacyAI = legacyAI;
    }

    public void UpdateAI(int diffMs) => _legacyAI.DoThink();
    public void JustEngagedWith(WS_Base.BaseUnit attacker) => _legacyAI.OnEnterCombat();
    public void DamageTaken(WS_Base.BaseUnit attacker, ref int damage) { }
    public void KilledUnit(WS_Base.BaseUnit victim)
    {
        var victimRef = victim;
        _legacyAI.OnKill(ref victimRef);
    }

    public void JustDied(WS_Base.BaseUnit killer) => _legacyAI.OnDeath();
    public void JustRespawned() { }
    public void EnterEvadeMode() => _legacyAI.OnLeaveCombat(true);
    public void SpellHit(WS_Base.BaseUnit caster, int spellId) { }
    public void MovementInform(MovementGeneratorType type, int pointId) { }
    public void OnHealthChange(int oldPercent, int newPercent) => _legacyAI.OnHealthChange(newPercent);
    public void SummonedCreatureDies(WS_Creatures.CreatureObject summon, WS_Base.BaseUnit killer) { }
    public void OnLeaveCombat(bool reset) => _legacyAI.OnLeaveCombat(reset);
    public void OnGenerateHate(WS_Base.BaseUnit attacker, int hateValue)
    {
        var attackerRef = attacker;
        _legacyAI.OnGenerateHate(ref attackerRef, hateValue);
    }
}
