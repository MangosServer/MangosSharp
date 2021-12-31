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

using Mangos.Common.Enums.Global;
using System.Xml.Serialization;

namespace Mangos.Cluster.Configuration;

[XmlRoot(ElementName = "WorldCluster")]
public class ClusterConfiguration
{
    public string WorldClusterEndpoint { get; set; } = "127.0.0.1:8085";
    public int ServerPlayerLimit { get; set; } = 10;

    // Database Settings
    public string AccountDatabase { get; set; } = "root;mangosVB;localhost;3306;mangosVB;MySQL";

    public string CharacterDatabase { get; set; } = "root;mangosVB;localhost;3306;mangosVB;MySQL";
    public string WorldDatabase { get; set; } = "root;mangosVB;localhost;3306;mangosVB;MySQL";

    // Cluster Settings
    public string ClusterListenAddress { get; set; } = "127.0.0.1";

    public int ClusterListenPort { get; set; } = 50001;

    // Stats Settings
    public bool StatsEnabled { get; set; } = true;

    public int StatsTimer { get; set; } = 120000;
    public string StatsLocation { get; set; } = "stats.xml";

    // Logging Settings
    public string LogType { get; set; } = "FILE";

    public LogType LogLevel { get; set; } = Common.Enums.Global.LogType.NETWORK;
    public string LogConfig { get; set; } = string.Empty;
    public bool PacketLogging { get; set; }
    public bool GmLogging { get; set; }
}
