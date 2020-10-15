//
//  Copyright (C) 2013-2020 getMaNGOS <https:\\getmangos.eu>
//  
//  This program is free software. You can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation. either version 2 of the License, or
//  (at your option) any later version.
//  
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY. Without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//  
//  You should have received a copy of the GNU General Public License
//  along with this program. If not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//

using System;
using System.Xml.Serialization;
using Mangos.Common.Enums.Global;

namespace Mangos.Cluster
{
    public partial class WorldCluster
    {
        [XmlRoot(ElementName = "WorldCluster")]
        public class XMLConfigFile
        {
            [XmlElement(ElementName = "WorldClusterPort")]
            private int _worldClusterPort = 8085;
            [XmlElement(ElementName = "WorldClusterAddress")]
            private string _worldClusterAddress = "127.0.0.1";
            [XmlElement(ElementName = "ServerPlayerLimit")]
            private int _serverPlayerLimit = 10;

            // Database Settings
            [XmlElement(ElementName = "AccountDatabase")]
            private string _accountDatabase = "root;mangosVB;localhost;3306;mangosVB;MySQL";
            [XmlElement(ElementName = "CharacterDatabase")]
            private string _characterDatabase = "root;mangosVB;localhost;3306;mangosVB;MySQL";
            [XmlElement(ElementName = "WorldDatabase")]
            private string _worldDatabase = "root;mangosVB;localhost;3306;mangosVB;MySQL";

            // Cluster Settings

            [XmlElement(ElementName = "ClusterListenAddress")]
            private string _clusterListenAddress = "127.0.0.1";
            [XmlElement(ElementName = "ClusterListenPort")]
            private int _clusterListenPort = 50001;

            // Stats Settings
            [XmlElement(ElementName = "StatsEnabled")]
            private bool _statsEnabled = true;
            [XmlElement(ElementName = "StatsTimer")]
            private int _statsTimer = 120000;
            [XmlElement(ElementName = "StatsLocation")]
            private string _statsLocation = "stats.xml";

            // Logging Settings
            [XmlElement(ElementName = "LogType")]
            private string _logType = "FILE";
            [XmlElement(ElementName = "LogLevel")]
            private LogType _logLevel = Common.Enums.Global.LogType.NETWORK;
            [XmlElement(ElementName = "LogConfig")]
            private string _logConfig = "";
            [XmlElement(ElementName = "PacketLogging")]
            private bool _packetLogging = false;
            [XmlElement(ElementName = "GMLogging")]
            private bool _gMLogging = false;

            public int WorldClusterPort
            {
                get
                {
                    return _worldClusterPort;
                }

                set
                {
                    _worldClusterPort = value;
                }
            }

            public string WorldClusterAddress
            {
                get
                {
                    return _worldClusterAddress;
                }

                set
                {
                    if (value is null)
                    {
                        throw new ArgumentNullException(nameof(value));
                    }

                    _worldClusterAddress = value;
                }
            }

            public int ServerPlayerLimit
            {
                get
                {
                    return _serverPlayerLimit;
                }

                set
                {
                    _serverPlayerLimit = value;
                }
            }

            public string AccountDatabase
            {
                get
                {
                    return _accountDatabase;
                }

                set
                {
                    if (value is null)
                    {
                        throw new ArgumentNullException(nameof(value));
                    }

                    _accountDatabase = value;
                }
            }

            public string CharacterDatabase
            {
                get
                {
                    return _characterDatabase;
                }

                set
                {
                    if (value is null)
                    {
                        throw new ArgumentNullException(nameof(value));
                    }

                    _characterDatabase = value;
                }
            }

            public string WorldDatabase
            {
                get
                {
                    return _worldDatabase;
                }

                set
                {
                    if (value is null)
                    {
                        throw new ArgumentNullException(nameof(value));
                    }

                    _worldDatabase = value;
                }
            }

            public string ClusterListenAddress
            {
                get
                {
                    return _clusterListenAddress;
                }

                set
                {
                    if (value is null)
                    {
                        throw new ArgumentNullException(nameof(value));
                    }

                    _clusterListenAddress = value;
                }
            }

            public int ClusterListenPort
            {
                get
                {
                    return _clusterListenPort;
                }

                set
                {
                    _clusterListenPort = value;
                }
            }

            public bool StatsEnabled
            {
                get
                {
                    return _statsEnabled;
                }

                set
                {
                    _statsEnabled = value;
                }
            }

            public int StatsTimer
            {
                get
                {
                    return _statsTimer;
                }

                set
                {
                    _statsTimer = value;
                }
            }

            public string StatsLocation
            {
                get
                {
                    return _statsLocation;
                }

                set
                {
                    if (value is null)
                    {
                        throw new ArgumentNullException(nameof(value));
                    }

                    _statsLocation = value;
                }
            }

            public string LogType
            {
                get
                {
                    return _logType;
                }

                set
                {
                    if (value is null)
                    {
                        throw new ArgumentNullException(nameof(value));
                    }

                    _logType = value;
                }
            }

            public LogType LogLevel
            {
                get
                {
                    return _logLevel;
                }

                set
                {
                    _logLevel = value;
                }
            }

            public string LogConfig
            {
                get
                {
                    return _logConfig;
                }

                set
                {
                    _logConfig = value;
                }
            }

            public bool PacketLogging
            {
                get
                {
                    return _packetLogging;
                }

                set
                {
                    _packetLogging = value;
                }
            }

            public bool GMLogging
            {
                get
                {
                    return _gMLogging;
                }

                set
                {
                    _gMLogging = value;
                }
            }
        }
    }
}