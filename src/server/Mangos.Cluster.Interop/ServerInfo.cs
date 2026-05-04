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
/// Heartbeat payload reported by a world to its supervisor. Cheap to
/// produce, summarises the world's current load so the cluster can place
/// new instances/clients on the least-loaded eligible world.
/// </summary>
public class ServerInfo
{
    /// <summary>0.0-1.0 CPU usage, sampled over the last heartbeat interval.</summary>
    public float CpuUsage
    {
        get; set;
    }

    /// <summary>Resident memory in bytes.</summary>
    public ulong MemoryUsage
    {
        get; set;
    }

    /// <summary>Total online clients on this world.</summary>
    public int PlayerCount
    {
        get; set;
    }

    /// <summary>Active instances (continents + dungeons + raids + battlegrounds).</summary>
    public int InstanceCount
    {
        get; set;
    }

    /// <summary>Battlegrounds currently hosted.</summary>
    public int BattlegroundCount
    {
        get; set;
    }

    /// <summary>Milliseconds since this world process started.</summary>
    public long UptimeMs
    {
        get; set;
    }
}
