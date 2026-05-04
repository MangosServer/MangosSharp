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

namespace Mangos.MySql.GetFederationPeers;

/// <summary>
/// Row shape returned by GetFederationPeersQuery. Mirrors the federation
/// columns added to the realmlist table in PR #4 (Rel21_02_002.sql).
/// </summary>
public sealed class FederationPeerModel
{
    public required uint clusterId;
    public required string clusterAdminEndpoint;
    public required string displayTag;
    public required string markerPosition;
}
