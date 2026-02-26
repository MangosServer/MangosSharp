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

namespace Mangos.World.AI.Sequencing;

/// <summary>
/// Represents a sequential series of actions that execute over multiple AI ticks.
/// Inspired by Unity's Coroutine system for expressing timed, multi-step behavior.
///
/// Primary use case: boss encounter phases that require "move, wait, cast, spawn"
/// sequences that currently can't be expressed without manual timer fields.
/// </summary>
public interface IActionSequence
{
    bool IsComplete { get; }
    bool IsCancelled { get; }
    void Update(int diffMs);
    void Cancel();
    void Reset();
}
