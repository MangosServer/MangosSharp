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

using Mangos.Configuration;

namespace Mangos.Cluster.Federation;

/// <summary>
/// Centralised renderer for the cross-realm display tag. Applied at the
/// receiving cluster only - we never trust a tag that came in over the
/// wire (peers could spoof). The receiver looks up the sender realm's
/// configured tag locally and decides whether to render based on
/// FederationMarkerMode + the per-account opt-in.
/// </summary>
public static class RealmMarkers
{
    /// <summary>Decorate <paramref name="name"/> with the supplied tag per the marker mode.</summary>
    public static string Decorate(
        string name,
        string tag,
        FederationMarkerMode mode,
        bool accountWantsMarkers,
        bool isWhisper,
        MarkerPlacement placement = MarkerPlacement.Prefix)
    {
        if (string.IsNullOrEmpty(tag)) return name;

        bool render = mode switch
        {
            FederationMarkerMode.Off => isWhisper, // Whispers always carry the marker for replyability.
            FederationMarkerMode.Always => true,
            FederationMarkerMode.ClientPreference => accountWantsMarkers || isWhisper,
            _ => false,
        };
        if (!render) return name;

        return placement switch
        {
            MarkerPlacement.Prefix => $"[{tag}] {name}",
            MarkerPlacement.Suffix => $"{name} [{tag}]",
            _ => name,
        };
    }
}

public enum MarkerPlacement
{
    Prefix = 0,
    Suffix = 1,
    None = 2,
}
