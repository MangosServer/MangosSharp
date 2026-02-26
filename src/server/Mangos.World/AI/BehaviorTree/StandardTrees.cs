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

using Mangos.World.AI.BehaviorTree.Composites;
using Mangos.World.AI.BehaviorTree.Leaves;

namespace Mangos.World.AI.BehaviorTree;

/// <summary>
/// Pre-built behavior trees for common creature archetypes.
/// These replace the hardcoded switch statement in DefaultAI.DoThink()
/// with composable, testable behavior trees.
///
/// Tree structure (Melee):
/// <code>
/// Selector
///   Sequence [Combat]
///     IsInCombat?
///     SelectHighestThreatTarget
///   Sequence [Idle]
///     Wait(3000)
/// </code>
/// </summary>
public static class StandardTrees
{
    /// <summary>
    /// Standard melee creature: fight highest threat when in combat, idle when not.
    /// </summary>
    public static IBTNode CreateMeleeTree()
    {
        return new SelectorNode(
            new SequenceNode(
                new IsInCombatCheck(),
                new SelectHighestThreatTarget()
            ),
            new WaitAction(3000)
        );
    }

    /// <summary>
    /// Caster creature: cast spells at range when in combat.
    /// </summary>
    public static IBTNode CreateCasterTree(int primarySpellId, int cooldownMs = 3000)
    {
        return new SelectorNode(
            new SequenceNode(
                new IsInCombatCheck(),
                new HasTargetCheck(),
                new CastSpellAction(primarySpellId)
            ),
            new WaitAction(cooldownMs)
        );
    }

    /// <summary>
    /// Critter: does nothing, just exists.
    /// </summary>
    public static IBTNode CreateCritterTree()
    {
        return new WaitAction(5000);
    }

    /// <summary>
    /// Guard: fights when in combat, otherwise stands still.
    /// </summary>
    public static IBTNode CreateGuardTree()
    {
        return new SelectorNode(
            new SequenceNode(
                new IsInCombatCheck(),
                new SelectHighestThreatTarget()
            ),
            new WaitAction(5000)
        );
    }
}
