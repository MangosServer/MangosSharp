//
// Copyright (C) 2013-2022 getMaNGOS <https://getmangos.eu>
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

public sealed class WorldConfiguration
{
    public required string ClusterConnectHost { get; init; }
    public required int ClusterConnectPort { get; init; }
    public required string LocalConnectHost { get; init; }
    public required int LocalConnectPort { get; init; }

    public required string AccountDatabase { get; init; }
    public required string CharacterDatabase { get; init; }
    public required string WorldDatabase { get; init; }

    public required ImmutableArray<int> Maps { get; init; }
    public required ImmutableArray<string> ScriptsCompiler { get; init; }
    public required bool VMapsEnabled { get; init; }
    public required int MapResolution { get; init; }

    public required string CommandCharacter { get; init; }
    public required bool GlobalAuction { get; init; }

    public required bool LineOfSightEnabled { get; set; }
    public required bool HeightCalcEnabled { get; set; }

    public required float ManaRegenerationRate { get; init; }
    public required float HealthRegenerationRate { get; init; }
    public required float XPRate { get; init; }

    public required string LogType { get; init; }
    public required string LogConfig { get; init; }

    public required bool CreateBattlegrounds { get; init; }
    public required bool CreatePartyInstances { get; init; }
    public required bool CreateRaidInstances { get; init; }
    public required bool CreateOther { get; init; }

    public required int SaveTimer { get; init; }
    public required int WeatherTimer { get; init; }
}
