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

namespace Mangos.Domain;

public enum GameState
{
    Starting,
    Loading,
    Running,
    ShuttingDown,
    Stopped
}

public sealed class Game
{
    private GameState _state = GameState.Stopped;
    private DateTime _startedAt;

    public GameState State => _state;
    public DateTime StartedAt => _startedAt;
    public TimeSpan Uptime => _state == GameState.Running ? DateTime.UtcNow - _startedAt : TimeSpan.Zero;

    public void TransitionTo(GameState newState)
    {
        var previousState = _state;
        Console.WriteLine($"[Game] State transition requested: {previousState} -> {newState}");

        var valid = (_state, newState) switch
        {
            (GameState.Stopped, GameState.Starting) => true,
            (GameState.Starting, GameState.Loading) => true,
            (GameState.Loading, GameState.Running) => true,
            (GameState.Running, GameState.ShuttingDown) => true,
            (GameState.ShuttingDown, GameState.Stopped) => true,
            _ => false
        };

        if (!valid)
        {
            Console.WriteLine($"[Game] ERROR: Invalid state transition from {_state} to {newState}");
            throw new InvalidOperationException($"Cannot transition from {_state} to {newState}");
        }

        if (newState == GameState.Running)
        {
            _startedAt = DateTime.UtcNow;
            Console.WriteLine($"[Game] Server start time recorded: {_startedAt:yyyy-MM-dd HH:mm:ss.fff} UTC");
        }

        _state = newState;
        Console.WriteLine($"[Game] State transitioned: {previousState} -> {newState}");

        if (newState == GameState.ShuttingDown)
        {
            Console.WriteLine($"[Game] Server shutting down after {Uptime.TotalSeconds:F1}s uptime");
        }
    }
}
