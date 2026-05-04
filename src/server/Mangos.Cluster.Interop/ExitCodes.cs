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

namespace Mangos.Cluster.Interop;

/// <summary>
/// Shared exit-code conventions for cluster and world processes. The
/// supervisor on the other side reads these to decide whether to respawn.
///
/// Codes are deliberately disjoint and small so they can be matched by
/// systemd Restart=on-failure rules or container orchestrators.
/// </summary>
public static class ExitCodes
{
    /// <summary>Clean shutdown requested by operator. Supervisor will not respawn.</summary>
    public const int Clean = 0;

    /// <summary>Configuration was invalid or missing. Supervisor will not respawn until config changes.</summary>
    public const int ConfigInvalid = 2;

    /// <summary>A required database is at the wrong schema version. No respawn.</summary>
    public const int DatabaseVersionMismatch = 3;

    /// <summary>Peer (cluster or world) requested a restart. Supervisor respawns immediately.</summary>
    public const int RestartRequested = 10;

    /// <summary>Peer requested a stop (hard). Supervisor will not respawn.</summary>
    public const int StopRequested = 11;

    /// <summary>Unhandled fatal exception. Supervisor respawns with backoff.</summary>
    public const int FatalCrash = 20;

    /// <summary>Cluster connection lost and grace period expired with no clients. Respawn when peer recovers.</summary>
    public const int Orphaned = 30;
}
