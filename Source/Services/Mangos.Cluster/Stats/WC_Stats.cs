//
//  Copyright (C) 2013-2020 getMaNGOS <https://getmangos.eu>
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Xml;
using Mangos.Cluster.Handlers;
using Mangos.Cluster.Network;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Misc;
using Mangos.Common.Enums.Player;
using Microsoft.VisualBasic;

namespace Mangos.Cluster.Stats
{
    public class WC_Stats
    {
        private readonly ClusterServiceLocator clusterServiceLocator;

        public WC_Stats(ClusterServiceLocator clusterServiceLocator)
        {
            this.clusterServiceLocator = clusterServiceLocator;
        }

        // http://www.15seconds.com/issue/050615.htm

        private int ConnectionsHandled;
        private int ConnectionsPeak;
        private int ConnectionsCurrent;

        public void ConnectionsIncrement()
        {
            Interlocked.Increment(ref ConnectionsHandled);
            if (Interlocked.Increment(ref ConnectionsCurrent) > ConnectionsPeak)
            {
                ConnectionsPeak = ConnectionsCurrent;
            }
        }

        public void ConnectionsDecrement()
        {
            Interlocked.Decrement(ref ConnectionsCurrent);
        }

        public long DataTransferOut = 0L;
        public long DataTransferIn = 0L;
        private int ThreadsWorker;
        private int ThreadsComletion;
        private DateTime LastCheck = DateAndTime.Now;
        private double LastCPUTime;
        private TimeSpan Uptime;
        private long Latency;
        private float UsageCPU;
        private long UsageMemory;
        private int CountPlayers;
        private int CountPlayersAlliance;
        private int CountPlayersHorde;
        private int CountGMs;
        private readonly Dictionary<WorldInfo, List<string>> w = new Dictionary<WorldInfo, List<string>>();

        private string FormatUptime(TimeSpan time)
        {
            return string.Format("{0}d {1}h {2}m {3}s {4}ms", time.Days, time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
        }

        public void CheckCpu(object state)
        {
            var timeSinceLastCheck = DateAndTime.Now.Subtract(LastCheck);
            UsageCPU = (float)((Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds - LastCPUTime) / timeSinceLastCheck.TotalMilliseconds * 100d);
            LastCheck = DateAndTime.Now;
            LastCPUTime = Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds;
        }

        private void PrepareStats()
        {
            Uptime = DateAndTime.Now.Subtract(Process.GetCurrentProcess().StartTime);
            ThreadPool.GetAvailableThreads(out ThreadsWorker, out ThreadsComletion);
            UsageMemory = (long)(Process.GetCurrentProcess().WorkingSet64 / (double)(1024 * 1024));
            CountPlayers = 0;
            CountPlayersHorde = 0;
            CountPlayersAlliance = 0;
            CountGMs = 0;
            Latency = 0L;
            clusterServiceLocator._WorldCluster.CHARACTERs_Lock.AcquireReaderLock(clusterServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
            foreach (KeyValuePair<ulong, WcHandlerCharacter.CharacterObject> objCharacter in clusterServiceLocator._WorldCluster.CHARACTERs)
            {
                if (objCharacter.Value.IsInWorld)
                {
                    CountPlayers += 1;
                    if (objCharacter.Value.Race == Races.RACE_ORC || objCharacter.Value.Race == Races.RACE_TAUREN || objCharacter.Value.Race == Races.RACE_TROLL || objCharacter.Value.Race == Races.RACE_UNDEAD)
                    {
                        CountPlayersHorde += 1;
                    }
                    else
                    {
                        CountPlayersAlliance += 1;
                    }

                    if (objCharacter.Value.Access > AccessLevel.Player)
                        CountGMs += 1;
                    Latency += objCharacter.Value.Latency;
                }
            }

            clusterServiceLocator._WorldCluster.CHARACTERs_Lock.ReleaseReaderLock();
            if (CountPlayers > 1)
            {
                Latency /= CountPlayers;
            }

            foreach (KeyValuePair<uint, WorldInfo> objCharacter in clusterServiceLocator._WC_Network.WorldServer.WorldsInfo)
            {
                if (!Information.IsNothing(objCharacter.Value))
                {
                    if (!w.ContainsKey(objCharacter.Value))
                    {
                        w.Add(objCharacter.Value, new List<string>());
                    }

                    w[objCharacter.Value].Add(objCharacter.Key.ToString());
                }
            }
        }

        public void GenerateStats(object state)
        {
            clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "Generating stats");
            PrepareStats();
            var f = XmlWriter.Create(clusterServiceLocator._WorldCluster.GetConfig().StatsLocation);
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
            f.WriteValue(FormatUptime(Uptime));
            f.WriteEndElement();
            f.WriteStartElement("onlineplayers");
            f.WriteValue(CountPlayers);
            f.WriteEndElement();
            f.WriteStartElement("gmcount");
            f.WriteValue(CountGMs);
            f.WriteEndElement();
            f.WriteStartElement("alliance");
            f.WriteValue(CountPlayersAlliance);
            f.WriteEndElement();
            f.WriteStartElement("horde");
            f.WriteValue(CountPlayersHorde);
            f.WriteEndElement();
            f.WriteStartElement("cpu");
            f.WriteValue(Strings.Format(UsageCPU, "0.00"));
            f.WriteEndElement();
            f.WriteStartElement("ram");
            f.WriteValue(UsageMemory);
            f.WriteEndElement();
            f.WriteStartElement("latency");
            f.WriteValue(Latency);
            f.WriteEndElement();
            f.WriteStartElement("connaccepted");
            f.WriteValue(ConnectionsHandled);
            f.WriteEndElement();
            f.WriteStartElement("connpeak");
            f.WriteValue(ConnectionsPeak);
            f.WriteEndElement();
            f.WriteStartElement("conncurrent");
            f.WriteValue(ConnectionsCurrent);
            f.WriteEndElement();
            f.WriteStartElement("networkin");
            f.WriteValue(DataTransferIn);
            f.WriteEndElement();
            f.WriteStartElement("networkout");
            f.WriteValue(DataTransferOut);
            f.WriteEndElement();
            f.WriteStartElement("threadsw");
            f.WriteValue(ThreadsWorker);
            f.WriteEndElement();
            f.WriteStartElement("threadsc");
            f.WriteValue(ThreadsComletion);
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
                foreach (KeyValuePair<WorldInfo, List<string>> objCharacter in w)
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
                    f.WriteValue(Strings.Format(objCharacter.Key.CPUUsage, "0.00"));
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
                clusterServiceLocator._WorldCluster.Log.WriteLine(LogType.FAILED, "Error while generating stats file: {0}", ex.ToString());
            }
            // </world>
            f.WriteEndElement();
            clusterServiceLocator._WorldCluster.CHARACTERs_Lock.AcquireReaderLock(clusterServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
            f.WriteStartElement("users");
            foreach (KeyValuePair<ulong, WcHandlerCharacter.CharacterObject> objCharacter in clusterServiceLocator._WorldCluster.CHARACTERs)
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
            foreach (KeyValuePair<ulong, WcHandlerCharacter.CharacterObject> objCharacter in clusterServiceLocator._WorldCluster.CHARACTERs)
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
            clusterServiceLocator._WorldCluster.CHARACTERs_Lock.ReleaseReaderLock();

            // </server>
            f.WriteEndElement();
            f.WriteEndDocument();
            f.Close();
            w.Clear();
        }
    }
}