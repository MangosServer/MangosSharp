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

using Mangos.Cluster.Configuration;
using Mangos.Cluster.Network;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Misc;
using Mangos.Common.Enums.Player;
using Mangos.Configuration;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Xml;

namespace Mangos.Cluster.Stats;

public class WcStats
{
    private readonly IConfigurationProvider<ClusterConfiguration> configurationProvider;
    private readonly ClusterServiceLocator _clusterServiceLocator;

    public WcStats(ClusterServiceLocator clusterServiceLocator,
        IConfigurationProvider<ClusterConfiguration> configurationProvider)
    {
        _clusterServiceLocator = clusterServiceLocator;
        this.configurationProvider = configurationProvider;
    }

    // http://www.15seconds.com/issue/050615.htm

    private int _connectionsHandled;
    private int _connectionsPeak;
    private int _connectionsCurrent;

    public void ConnectionsIncrement()
    {
        Interlocked.Increment(ref _connectionsHandled);
        if (Interlocked.Increment(ref _connectionsCurrent) > _connectionsPeak)
        {
            _connectionsPeak = _connectionsCurrent;
        }
    }

    public void ConnectionsDecrement()
    {
        Interlocked.Decrement(ref _connectionsCurrent);
    }

    public long DataTransferOut;
    public long DataTransferIn;
    private int _threadsWorker;
    private int _threadsComletion;
    private DateTime _lastCheck = DateAndTime.Now;
    private double _lastCpuTime;
    private TimeSpan _uptime;
    private long _latency;
    private float _usageCpu;
    private long _usageMemory;
    private int _countPlayers;
    private int _countPlayersAlliance;
    private int _countPlayersHorde;
    private int _countGMs;
    private readonly Dictionary<WorldInfo, List<string>> _w = new();

    private string FormatUptime(TimeSpan time)
    {
        return string.Format("{0}d {1}h {2}m {3}s {4}ms", time.Days, time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
    }

    public void CheckCpu(object state)
    {
        var timeSinceLastCheck = DateAndTime.Now.Subtract(_lastCheck);
        _usageCpu = (float)((Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds - _lastCpuTime) / timeSinceLastCheck.TotalMilliseconds * 100d);
        _lastCheck = DateAndTime.Now;
        _lastCpuTime = Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds;
    }

    private void PrepareStats()
    {
        _uptime = DateAndTime.Now.Subtract(Process.GetCurrentProcess().StartTime);
        ThreadPool.GetAvailableThreads(out _threadsWorker, out _threadsComletion);
        _usageMemory = (long)(Process.GetCurrentProcess().WorkingSet64 / (double)(1024 * 1024));
        _countPlayers = 0;
        _countPlayersHorde = 0;
        _countPlayersAlliance = 0;
        _countGMs = 0;
        _latency = 0L;
        _clusterServiceLocator.WorldCluster.CharacteRsLock.AcquireReaderLock(_clusterServiceLocator.GlobalConstants.DEFAULT_LOCK_TIMEOUT);
        foreach (var objCharacter in _clusterServiceLocator.WorldCluster.CharacteRs)
        {
            if (objCharacter.Value.IsInWorld)
            {
                _countPlayers += 1;
                if (objCharacter.Value.Race is Races.RACE_ORC or Races.RACE_TAUREN or Races.RACE_TROLL or Races.RACE_UNDEAD)
                {
                    _countPlayersHorde += 1;
                }
                else
                {
                    _countPlayersAlliance += 1;
                }

                if (objCharacter.Value.Access > AccessLevel.Player)
                {
                    _countGMs += 1;
                }

                _latency += objCharacter.Value.Latency;
            }
        }

        _clusterServiceLocator.WorldCluster.CharacteRsLock.ReleaseReaderLock();
        if (_countPlayers > 1)
        {
            _latency /= _countPlayers;
        }

        foreach (var objCharacter in _clusterServiceLocator.WcNetwork.WorldServer.WorldsInfo)
        {
            if (!Information.IsNothing(objCharacter.Value))
            {
                if (!_w.ContainsKey(objCharacter.Value))
                {
                    _w.Add(objCharacter.Value, new List<string>());
                }

                _w[objCharacter.Value].Add(objCharacter.Key.ToString());
            }
        }
    }

    public void GenerateStats(object state)
    {
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "Generating stats");
        PrepareStats();
        XmlWriter f = XmlWriter.Create(configurationProvider.GetConfiguration().StatsLocation);
        f.WriteStartDocument(true);
        f.WriteComment("generated at " + DateTime.Now.ToString("hh:mm:ss"));
        // <?xml-stylesheet type="text/xsl" href="stats.xsl"?>
        f.WriteProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"stats.xsl\"");
        // <server>
        f.WriteStartElement("server");

        // <cluster>
        f.WriteStartElement("cluster");
        f.WriteStartElement("platform");
        f.WriteValue(string.Format("mangosVB rev{0}", Assembly.GetExecutingAssembly().GetName().Version));
        f.WriteEndElement();
        f.WriteStartElement("uptime");
        f.WriteValue(FormatUptime(_uptime));
        f.WriteEndElement();
        f.WriteStartElement("onlineplayers");
        f.WriteValue(_countPlayers);
        f.WriteEndElement();
        f.WriteStartElement("gmcount");
        f.WriteValue(_countGMs);
        f.WriteEndElement();
        f.WriteStartElement("alliance");
        f.WriteValue(_countPlayersAlliance);
        f.WriteEndElement();
        f.WriteStartElement("horde");
        f.WriteValue(_countPlayersHorde);
        f.WriteEndElement();
        f.WriteStartElement("cpu");
        f.WriteValue(Strings.Format(_usageCpu, "0.00"));
        f.WriteEndElement();
        f.WriteStartElement("ram");
        f.WriteValue(_usageMemory);
        f.WriteEndElement();
        f.WriteStartElement("latency");
        f.WriteValue(_latency);
        f.WriteEndElement();
        f.WriteStartElement("connaccepted");
        f.WriteValue(_connectionsHandled);
        f.WriteEndElement();
        f.WriteStartElement("connpeak");
        f.WriteValue(_connectionsPeak);
        f.WriteEndElement();
        f.WriteStartElement("conncurrent");
        f.WriteValue(_connectionsCurrent);
        f.WriteEndElement();
        f.WriteStartElement("networkin");
        f.WriteValue(DataTransferIn);
        f.WriteEndElement();
        f.WriteStartElement("networkout");
        f.WriteValue(DataTransferOut);
        f.WriteEndElement();
        f.WriteStartElement("threadsw");
        f.WriteValue(_threadsWorker);
        f.WriteEndElement();
        f.WriteStartElement("threadsc");
        f.WriteValue(_threadsComletion);
        f.WriteEndElement();
        f.WriteStartElement("lastupdate");
        f.WriteValue(DateAndTime.Now.ToString());
        f.WriteEndElement();

        // </cluster>
        f.WriteEndElement();

        // <world>
        f.WriteStartElement("world");
        try
        {
            foreach (var objCharacter in _w)
            {
                f.WriteStartElement("instance");
                f.WriteStartElement("uptime");
                f.WriteValue(FormatUptime(DateAndTime.Now - objCharacter.Key.Started));
                f.WriteEndElement();
                f.WriteStartElement("players");
                f.WriteValue("-");
                f.WriteEndElement();
                f.WriteStartElement("maps");
                f.WriteValue(Strings.Join(objCharacter.Value.ToArray(), ", "));
                f.WriteEndElement();
                f.WriteStartElement("cpu");
                f.WriteValue(Strings.Format(objCharacter.Key.CpuUsage, "0.00"));
                f.WriteEndElement();
                f.WriteStartElement("ram");
                f.WriteValue((decimal)objCharacter.Key.MemoryUsage);
                f.WriteEndElement();
                f.WriteStartElement("latency");
                f.WriteValue(objCharacter.Key.Latency);
                f.WriteEndElement();
                f.WriteEndElement();
            }
        }
        catch (Exception ex)
        {
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.FAILED, "Error while generating stats file: {0}", ex.ToString());
        }
        // </world>
        f.WriteEndElement();
        _clusterServiceLocator.WorldCluster.CharacteRsLock.AcquireReaderLock(_clusterServiceLocator.GlobalConstants.DEFAULT_LOCK_TIMEOUT);
        f.WriteStartElement("users");
        foreach (var objCharacter in _clusterServiceLocator.WorldCluster.CharacteRs)
        {
            if (objCharacter.Value.IsInWorld && objCharacter.Value.Access >= AccessLevel.GameMaster)
            {
                f.WriteStartElement("gmplayer");
                f.WriteStartElement("name");
                f.WriteValue(objCharacter.Value.Name);
                f.WriteEndElement();
                f.WriteStartElement("access");
                f.WriteValue(objCharacter.Value.Access.ToString());
                f.WriteEndElement();
                f.WriteEndElement();
            }
        }

        f.WriteEndElement();
        f.WriteStartElement("sessions");
        foreach (var objCharacter in _clusterServiceLocator.WorldCluster.CharacteRs)
        {
            if (objCharacter.Value.IsInWorld)
            {
                f.WriteStartElement("player");
                f.WriteStartElement("name");
                f.WriteValue(objCharacter.Value.Name);
                f.WriteEndElement();
                f.WriteStartElement("race");
                f.WriteValue((byte)objCharacter.Value.Race);
                f.WriteEndElement();
                f.WriteStartElement("class");
                f.WriteValue((byte)objCharacter.Value.Classe);
                f.WriteEndElement();
                f.WriteStartElement("level");
                f.WriteValue(objCharacter.Value.Level.ToString());
                f.WriteEndElement();
                f.WriteStartElement("map");
                f.WriteValue(objCharacter.Value.Map.ToString());
                f.WriteEndElement();
                f.WriteStartElement("zone");
                f.WriteValue(objCharacter.Value.Zone.ToString());
                f.WriteEndElement();
                f.WriteStartElement("ontime");
                f.WriteValue(FormatUptime(DateAndTime.Now - objCharacter.Value.Time));
                f.WriteEndElement();
                f.WriteStartElement("latency");
                f.WriteValue(objCharacter.Value.Latency.ToString());
                f.WriteEndElement();
                f.WriteEndElement();
            }
        }

        f.WriteEndElement();
        _clusterServiceLocator.WorldCluster.CharacteRsLock.ReleaseReaderLock();

        // </server>
        f.WriteEndElement();
        f.WriteEndDocument();
        f.Close();
        _w.Clear();
    }
}
