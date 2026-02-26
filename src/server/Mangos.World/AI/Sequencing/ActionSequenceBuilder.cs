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
using Mangos.World.Objects;
using System;
using System.Collections.Generic;

namespace Mangos.World.AI.Sequencing;

/// <summary>
/// Fluent builder for constructing action sequences.
/// Inspired by Unity's coroutine yield patterns and UE4's latent action system.
///
/// Example usage (Onyxia fly phase):
/// <code>
/// var sequence = new ActionSequenceBuilder(creature)
///     .Yell("Phase 2!")
///     .Do(() => { AllowedAttack = false; ResetThreatTable(); })
///     .Wait(2000)
///     .Do(() => creature.Flying = true)
///     .Wait(10000)
///     .Do(() => SpawnWhelps(15))
///     .WaitUntil(() => whelpsDefeated)
///     .Build();
/// </code>
/// </summary>
public sealed class ActionSequenceBuilder
{
    private readonly WS_Creatures.CreatureObject _creature;
    private readonly List<ISequenceStep> _steps = new();

    public ActionSequenceBuilder(WS_Creatures.CreatureObject creature = null)
    {
        _creature = creature;
    }

    /// <summary>
    /// Executes an immediate action and advances to the next step.
    /// </summary>
    public ActionSequenceBuilder Do(Action action)
    {
        _steps.Add(new ImmediateActionStep(action));
        return this;
    }

    /// <summary>
    /// Waits for the specified duration before advancing.
    /// </summary>
    public ActionSequenceBuilder Wait(int durationMs)
    {
        _steps.Add(new WaitStep(durationMs));
        return this;
    }

    /// <summary>
    /// Waits until the condition returns true before advancing.
    /// </summary>
    public ActionSequenceBuilder WaitUntil(Func<bool> condition)
    {
        _steps.Add(new WaitUntilStep(condition));
        return this;
    }

    /// <summary>
    /// Casts a spell on a target. Requires creature to be set.
    /// </summary>
    public ActionSequenceBuilder CastSpell(int spellId, WS_Base.BaseUnit target)
    {
        _steps.Add(new ImmediateActionStep(() => _creature?.CastSpell(spellId, target)));
        return this;
    }

    /// <summary>
    /// Casts a spell on self. Requires creature to be set.
    /// </summary>
    public ActionSequenceBuilder CastSpellOnSelf(int spellId)
    {
        _steps.Add(new ImmediateActionStep(() => _creature?.CastSpellOnSelf(spellId)));
        return this;
    }

    /// <summary>
    /// Sends a yell message as the creature.
    /// </summary>
    public ActionSequenceBuilder Yell(string text)
    {
        _steps.Add(new ImmediateActionStep(() =>
            _creature?.SendChatMessage(text, ChatMsg.CHAT_MSG_MONSTER_YELL, LANGUAGES.LANG_GLOBAL)));
        return this;
    }

    /// <summary>
    /// Sends a say message as the creature.
    /// </summary>
    public ActionSequenceBuilder Say(string text)
    {
        _steps.Add(new ImmediateActionStep(() =>
            _creature?.SendChatMessage(text, ChatMsg.CHAT_MSG_MONSTER_SAY, LANGUAGES.LANG_GLOBAL)));
        return this;
    }

    /// <summary>
    /// Builds the action sequence.
    /// </summary>
    public IActionSequence Build()
    {
        return new ActionSequence(new List<ISequenceStep>(_steps));
    }
}
