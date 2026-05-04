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
/// Cluster &lt;-&gt; cluster federation: admin RPC, cross-realm chat,
/// cross-realm group state. Disabled by default. Each peer is keyed by
/// cluster id; the actual host:port comes from the realmlist DB column
/// added in PR #4 so peer endpoints stay in one place.
/// </summary>
public sealed class FederationConfiguration
{
    /// <summary>Master switch.</summary>
    public bool Enabled { get; init; } = false;

    /// <summary>Identifier this cluster reports to peers.</summary>
    public uint LocalClusterId { get; init; } = 0;

    /// <summary>Short tag this cluster reports to peers (e.g. "WM"). Falls back to realmlist.displayTag.</summary>
    public string LocalDisplayTag { get; init; } = string.Empty;

    /// <summary>Bind address for inbound peer connections.</summary>
    public string ListenAddress { get; init; } = "0.0.0.0";

    /// <summary>Listen port (separate from the world IPC port).</summary>
    public int ListenPort { get; init; } = 50101;

    /// <summary>Per-peer shared secrets. Local copy of the symmetric key with each peer.</summary>
    public ImmutableArray<FederationPeerSecret> Peers { get; init; } = ImmutableArray<FederationPeerSecret>.Empty;

    /// <summary>How players from other realms appear in chat / unit frames on this cluster.</summary>
    public FederationMarkerMode MarkerMode { get; init; } = FederationMarkerMode.ClientPreference;
}

public sealed class FederationPeerSecret
{
    public required uint ClusterId { get; init; }
    public required string Secret { get; init; }
}

public enum FederationMarkerMode
{
    /// <summary>Markers always rendered regardless of per-account preference.</summary>
    Always = 0,
    /// <summary>Render markers only when the account's federation_show_markers flag is on.</summary>
    ClientPreference = 1,
    /// <summary>Never render markers (server-enforced off).</summary>
    Off = 2,
}
