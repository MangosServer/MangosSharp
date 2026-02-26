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

using System;
using System.Collections.Generic;

namespace Mangos.World.AI.Sequencing;

/// <summary>
/// Action sequence implementation that steps through queued actions with proper
/// elapsed-time tracking. Supports immediate actions, timed waits, and conditional waits.
/// </summary>
public sealed class ActionSequence : IActionSequence
{
    private readonly List<ISequenceStep> _steps;
    private int _currentStep;

    public bool IsComplete => _currentStep >= _steps.Count;
    public bool IsCancelled { get; private set; }

    internal ActionSequence(List<ISequenceStep> steps)
    {
        _steps = steps;
        _currentStep = 0;
    }

    public void Update(int diffMs)
    {
        if (IsComplete || IsCancelled) return;

        while (_currentStep < _steps.Count)
        {
            var step = _steps[_currentStep];
            if (step.Execute(diffMs))
            {
                _currentStep++;
            }
            else
            {
                break; // Step still running (e.g., waiting)
            }
        }
    }

    public void Cancel()
    {
        IsCancelled = true;
    }

    public void Reset()
    {
        _currentStep = 0;
        IsCancelled = false;
        foreach (var step in _steps)
        {
            step.Reset();
        }
    }
}

internal interface ISequenceStep
{
    /// <summary>
    /// Returns true when the step is complete and the sequence should advance.
    /// </summary>
    bool Execute(int diffMs);
    void Reset();
}

internal sealed class ImmediateActionStep : ISequenceStep
{
    private readonly Action _action;
    public ImmediateActionStep(Action action) => _action = action;
    public bool Execute(int diffMs) { _action(); return true; }
    public void Reset() { }
}

internal sealed class WaitStep : ISequenceStep
{
    private readonly int _durationMs;
    private int _elapsed;

    public WaitStep(int durationMs)
    {
        _durationMs = durationMs;
    }

    public bool Execute(int diffMs)
    {
        _elapsed += diffMs;
        return _elapsed >= _durationMs;
    }

    public void Reset() => _elapsed = 0;
}

internal sealed class WaitUntilStep : ISequenceStep
{
    private readonly Func<bool> _condition;
    public WaitUntilStep(Func<bool> condition) => _condition = condition;
    public bool Execute(int diffMs) => _condition();
    public void Reset() { }
}
