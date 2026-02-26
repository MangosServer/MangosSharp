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

using System.Collections.Generic;

namespace Mangos.World.AI.Sequencing;

/// <summary>
/// Manages multiple named action sequences for a creature.
/// Allows boss scripts to run parallel or sequential multi-step behaviors.
/// </summary>
public sealed class SequenceManager
{
    private readonly Dictionary<string, IActionSequence> _sequences = new();
    private readonly List<string> _completed = new();

    /// <summary>
    /// Starts or replaces a named sequence.
    /// </summary>
    public void Start(string name, IActionSequence sequence)
    {
        if (_sequences.TryGetValue(name, out var existing))
        {
            existing.Cancel();
        }
        _sequences[name] = sequence;
    }

    /// <summary>
    /// Updates all active sequences and removes completed ones.
    /// </summary>
    public void Update(int diffMs)
    {
        _completed.Clear();
        foreach (var kvp in _sequences)
        {
            kvp.Value.Update(diffMs);
            if (kvp.Value.IsComplete || kvp.Value.IsCancelled)
            {
                _completed.Add(kvp.Key);
            }
        }
        foreach (var name in _completed)
        {
            _sequences.Remove(name);
        }
    }

    /// <summary>
    /// Returns true if the named sequence is currently running.
    /// </summary>
    public bool IsRunning(string name)
    {
        return _sequences.TryGetValue(name, out var seq) && !seq.IsComplete && !seq.IsCancelled;
    }

    /// <summary>
    /// Cancels a named sequence.
    /// </summary>
    public void Cancel(string name)
    {
        if (_sequences.TryGetValue(name, out var seq))
        {
            seq.Cancel();
            _sequences.Remove(name);
        }
    }

    /// <summary>
    /// Cancels all running sequences.
    /// </summary>
    public void CancelAll()
    {
        foreach (var seq in _sequences.Values)
        {
            seq.Cancel();
        }
        _sequences.Clear();
    }

    /// <summary>
    /// Returns true if any sequence is currently running.
    /// </summary>
    public bool HasActiveSequences => _sequences.Count > 0;
}
