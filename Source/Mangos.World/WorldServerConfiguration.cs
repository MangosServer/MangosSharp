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
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Mangos.World;

[XmlRoot(ElementName = "WorldServer")]
public class WorldServerConfiguration
{
    [XmlElement(ElementName = "AccountDatabase")]
    public string AccountDatabase;

    [XmlElement(ElementName = "CharacterDatabase")]
    public string CharacterDatabase;

    [XmlElement(ElementName = "WorldDatabase")]
    public string WorldDatabase;

    [XmlElement(ElementName = "ServerPlayerLimit")]
    public int ServerPlayerLimit;

    [XmlElement(ElementName = "CommandCharacter")]
    public string CommandCharacter;

    [XmlElement(ElementName = "XPRate")]
    public float XPRate;

    [XmlElement(ElementName = "ManaRegenerationRate")]
    public float ManaRegenerationRate;

    [XmlElement(ElementName = "HealthRegenerationRate")]
    public float HealthRegenerationRate;

    [XmlElement(ElementName = "GlobalAuction")]
    public bool GlobalAuction;

    [XmlElement(ElementName = "SaveTimer")]
    public int SaveTimer;

    [XmlElement(ElementName = "WeatherTimer")]
    public int WeatherTimer;

    [XmlElement(ElementName = "MapResolution")]
    public int MapResolution;

    [XmlArray(ElementName = "HandledMaps")]
    [XmlArrayItem(typeof(string), ElementName = "Map")]
    public List<string> Maps;

    [XmlElement(ElementName = "VMaps")]
    public bool VMapsEnabled;

    [XmlElement(ElementName = "VMapLineOfSightCalc")]
    public bool LineOfSightEnabled;

    [XmlElement(ElementName = "VMapHeightCalc")]
    public bool HeightCalcEnabled;

    [XmlElement(ElementName = "LogType")]
    public string LogType;

    [XmlElement(ElementName = "LogLevel")]
    public LogType LogLevel;

    [XmlElement(ElementName = "LogConfig")]
    public string LogConfig;

    [XmlArray(ElementName = "ScriptsCompiler")]
    [XmlArrayItem(typeof(string), ElementName = "Include")]
    public ArrayList CompilerInclude;

    [XmlElement(ElementName = "CreatePartyInstances")]
    public bool CreatePartyInstances;

    [XmlElement(ElementName = "CreateRaidInstances")]
    public bool CreateRaidInstances;

    [XmlElement(ElementName = "CreateBattlegrounds")]
    public bool CreateBattlegrounds;

    [XmlElement(ElementName = "CreateArenas")]
    public bool CreateArenas;

    [XmlElement(ElementName = "CreateOther")]
    public bool CreateOther;

    [XmlElement(ElementName = "ClusterConnectHost")]
    public string ClusterConnectHost;

    [XmlElement(ElementName = "ClusterConnectPort")]
    public int ClusterConnectPort;

    [XmlElement(ElementName = "LocalConnectHost")]
    public string LocalConnectHost;

    [XmlElement(ElementName = "LocalConnectPort")]
    public int LocalConnectPort;

    public WorldServerConfiguration()
    {
        AccountDatabase = "root;mangosVB;localhost;3306;mangosVB;MySQL";
        CharacterDatabase = "root;mangosVB;localhost;3306;mangosVB;MySQL";
        WorldDatabase = "root;mangosVB;localhost;3306;mangosVB;MySQL";
        ServerPlayerLimit = 10;
        CommandCharacter = ".";
        XPRate = 1f;
        ManaRegenerationRate = 1f;
        HealthRegenerationRate = 1f;
        GlobalAuction = false;
        SaveTimer = 120000;
        WeatherTimer = 600000;
        MapResolution = 64;
        Maps = new List<string>();
        VMapsEnabled = false;
        LineOfSightEnabled = false;
        HeightCalcEnabled = false;
        LogType = "FILE";
        LogLevel = Common.Enums.Global.LogType.NETWORK;
        LogConfig = "";
        CompilerInclude = new ArrayList();
        CreatePartyInstances = false;
        CreateRaidInstances = false;
        CreateBattlegrounds = false;
        CreateArenas = false;
        CreateOther = false;
        ClusterConnectHost = "127.0.0.1";
        ClusterConnectPort = 50001;
        LocalConnectHost = "127.0.0.1";
        LocalConnectPort = 50002;
    }
}
