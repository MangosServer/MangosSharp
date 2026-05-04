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
using System.Diagnostics;
using Mangos.Cluster.Interop;
using Mangos.Configuration;

namespace Mangos.Cluster.Supervision;

/// <summary>
/// Live state for one supervised world. Owned by <see cref="WorldSupervisor"/>;
/// not thread-safe in itself - the supervisor's reconcile loop is the only writer.
/// </summary>
public sealed class SupervisedWorld
{
    public required SupervisedWorldEntry Definition
    {
        get; init;
    }

    public WorldRunState State { get; set; } = WorldRunState.Idle;

    /// <summary>Wall-clock time of the last successful heartbeat reply.</summary>
    public DateTime LastHeartbeat { get; set; } = DateTime.MinValue;

    /// <summary>Heartbeats issued without reply since last contact.</summary>
    public int MissedHeartbeats
    {
        get; set;
    }

    /// <summary>Most recent load snapshot from the world's GetServerInfo reply.</summary>
    public ServerInfo? LastStatus
    {
        get; set;
    }

    /// <summary>OS process for the running world (Internal mode only).</summary>
    public Process? Process
    {
        get; set;
    }

    /// <summary>Live IWorld proxy for this world, set by the cluster on hello and cleared on goodbye.</summary>
    public IWorld? Proxy
    {
        get; set;
    }

    /// <summary>The maps this world claimed in its most recent hello.</summary>
    public IReadOnlyList<uint> ClaimedMaps { get; set; } = Array.Empty<uint>();

    /// <summary>True iff the operator (or a peer cluster) explicitly asked for a stop.</summary>
    public bool ExplicitStop
    {
        get; set;
    }

    /// <summary>Wall-clock time of the previous spawn attempt; used for backoff.</summary>
    public DateTime LastSpawnAttempt { get; set; } = DateTime.MinValue;

    /// <summary>Number of consecutive crash respawns since the last clean run.</summary>
    public int ConsecutiveCrashRestarts
    {
        get; set;
    }

    public bool IsAlive => State is WorldRunState.Starting or WorldRunState.Running or WorldRunState.Stale;
}

public enum WorldRunState
{
    /// <summary>Not running; waiting to be started or stopped permanently.</summary>
    Idle = 0,

    /// <summary>Spawn in progress; process started but cluster handshake not complete.</summary>
    Starting = 1,

    /// <summary>Running and replying to heartbeats.</summary>
    Running = 2,

    /// <summary>Process is up but heartbeats are overdue.</summary>
    Stale = 3,

    /// <summary>Heartbeats stopped past DeadAfterMissed; eligible for kill+respawn.</summary>
    Dead = 4,

    /// <summary>Operator-requested stop. Will not be auto-respawned.</summary>
    Stopped = 5,
}
