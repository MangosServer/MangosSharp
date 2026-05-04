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

using System.Collections.Immutable;

namespace Mangos.Configuration;

/// <summary>
/// Cluster-side supervisor settings: which world processes to manage,
/// how to launch them, and how aggressively to react to outages.
/// </summary>
public sealed class SupervisorConfiguration
{
    /// <summary>Master switch. When false the cluster manages no worlds and trusts external orchestration.</summary>
    public bool Enabled { get; init; } = false;

    /// <summary>Heartbeat interval (cluster pings world). Default 5000ms.</summary>
    public int HeartbeatIntervalMs { get; init; } = 5000;

    /// <summary>Missed heartbeats before a world is declared stale (still hosting clients but unresponsive).</summary>
    public int StaleAfterMissed { get; init; } = 3;

    /// <summary>Missed heartbeats before a world is declared dead (eligible for kill+respawn).</summary>
    public int DeadAfterMissed { get; init; } = 5;

    /// <summary>Backoff (ms) added to each successive crash respawn, capped at <see cref="RespawnBackoffMaxMs"/>.</summary>
    public int RespawnBackoffStepMs { get; init; } = 2000;

    /// <summary>Maximum backoff (ms) between respawn attempts after repeated crashes.</summary>
    public int RespawnBackoffMaxMs { get; init; } = 60000;

    /// <summary>Worlds the cluster supervises.</summary>
    public ImmutableArray<SupervisedWorldEntry> Worlds { get; init; } = ImmutableArray<SupervisedWorldEntry>.Empty;
}

/// <summary>
/// One supervised world definition. The supervisor reconciles the running
/// process set against this list every tick, spawning/killing as needed.
/// </summary>
public sealed class SupervisedWorldEntry
{
    /// <summary>Stable identifier for this world (used in admin commands).</summary>
    public required string WorldId { get; init; }

    /// <summary>internal = cluster forks the process; external = something else owns the process (systemd, docker).</summary>
    public SupervisorMode Mode { get; init; } = SupervisorMode.Internal;

    /// <summary>Path to the WorldServer executable (Internal mode only).</summary>
    public string? ExecutablePath { get; init; }

    /// <summary>Extra command-line arguments (Internal mode only).</summary>
    public ImmutableArray<string> Arguments { get; init; } = ImmutableArray<string>.Empty;

    /// <summary>Working directory override (Internal mode only).</summary>
    public string? WorkingDirectory { get; init; }

    /// <summary>Maps this world is allowed to claim. Empty = any.</summary>
    public ImmutableArray<uint> AllowedMaps { get; init; } = ImmutableArray<uint>.Empty;

    /// <summary>Whether the supervisor should auto-start this world at cluster startup.</summary>
    public bool Autostart { get; init; } = true;

    /// <summary>Whether to respawn after non-clean exits (FatalCrash / Orphaned).</summary>
    public bool Autorestart { get; init; } = true;
}

public enum SupervisorMode
{
    /// <summary>Cluster owns the child process and respawns it.</summary>
    Internal = 0,

    /// <summary>External orchestrator (systemd, docker, k8s) owns the process. Cluster only tracks state.</summary>
    External = 1,
}
