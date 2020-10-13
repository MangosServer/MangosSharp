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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Xml.Serialization;
using Mangos.Cluster.Globals;
using Mangos.Cluster.Handlers;
using Mangos.Cluster.Server;
using Mangos.Common;
using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using Mangos.Common.Logging;
using Mangos.SignalR;
using Microsoft.VisualBasic;

namespace Mangos.Cluster
{
    public class WorldCluster
    {
        private const string ClusterPath = "configs/WorldCluster.ini";

        // Players' containers
        public long CLIETNIDs = 0L;
        public Dictionary<uint, WC_Network.ClientClass> CLIENTs = new Dictionary<uint, WC_Network.ClientClass>();
        public ReaderWriterLock CHARACTERs_Lock = new ReaderWriterLock();
        public Dictionary<ulong, WcHandlerCharacter.CharacterObject> CHARACTERs = new Dictionary<ulong, WcHandlerCharacter.CharacterObject>();
        // Public CHARACTER_NAMEs As New Hashtable

        // System Things...
        public BaseWriter Log = new BaseWriter();
        public Random Rnd = new Random();

        public delegate void HandlePacket(Packets.PacketClass packet, WC_Network.ClientClass client);

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

        public void LoadConfig()
        {
            try
            {
                // Make sure WorldCluster.ini exists
                if (File.Exists(ClusterPath) == false)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[{0}] Cannot Continue. {1} does not exist.", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), ClusterPath);
                    Console.WriteLine("Please make sure your ini files are inside config folder where the mangosvb executables are located.");
                    Console.WriteLine("Press any key to exit server: ");
                    Console.ReadKey();
                    Environment.Exit(0);
                }

                Console.Write("[{0}] Loading Configuration...", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"));
                var xmlConfigFile = new XMLConfigFile();
                Config = xmlConfigFile;
                Console.Write("...");
                StreamReader ostream;
                ostream = new StreamReader(ClusterPath);
                Config = (XMLConfigFile)new XmlSerializer(typeof(XMLConfigFile)).Deserialize(ostream);
                ostream.Close();
                Console.WriteLine(".[done]");

                // DONE: Setting SQL Connections
                var AccountDBSettings = Strings.Split(Config.AccountDatabase, ";");
                if (AccountDBSettings.Length != 6)
                {
                    Console.WriteLine("Invalid connect string for the account database!");
                }
                else
                {
                    AccountDatabase.SQLDBName = AccountDBSettings[4];
                    AccountDatabase.SQLHost = AccountDBSettings[2];
                    AccountDatabase.SQLPort = AccountDBSettings[3];
                    AccountDatabase.SQLUser = AccountDBSettings[0];
                    AccountDatabase.SQLPass = AccountDBSettings[1];
                    AccountDatabase.SQLTypeServer = (SQL.DB_Type)Enum.Parse(typeof(SQL.DB_Type), AccountDBSettings[5]);
                }

                var CharacterDBSettings = Strings.Split(Config.CharacterDatabase, ";");
                if (CharacterDBSettings.Length != 6)
                {
                    Console.WriteLine("Invalid connect string for the character database!");
                }
                else
                {
                    CharacterDatabase.SQLDBName = CharacterDBSettings[4];
                    CharacterDatabase.SQLHost = CharacterDBSettings[2];
                    CharacterDatabase.SQLPort = CharacterDBSettings[3];
                    CharacterDatabase.SQLUser = CharacterDBSettings[0];
                    CharacterDatabase.SQLPass = CharacterDBSettings[1];
                    CharacterDatabase.SQLTypeServer = (SQL.DB_Type)Enum.Parse(typeof(SQL.DB_Type), CharacterDBSettings[5]);
                }

                var WorldDBSettings = Strings.Split(Config.WorldDatabase, ";");
                if (WorldDBSettings.Length != 6)
                {
                    Console.WriteLine("Invalid connect string for the world database!");
                }
                else
                {
                    WorldDatabase.SQLDBName = WorldDBSettings[4];
                    WorldDatabase.SQLHost = WorldDBSettings[2];
                    WorldDatabase.SQLPort = WorldDBSettings[3];
                    WorldDatabase.SQLUser = WorldDBSettings[0];
                    WorldDatabase.SQLPass = WorldDBSettings[1];
                    WorldDatabase.SQLTypeServer = (SQL.DB_Type)Enum.Parse(typeof(SQL.DB_Type), WorldDBSettings[5]);
                }

                // DONE: Creating logger
                Log = BaseWriter.CreateLog(Config.LogType, Config.LogConfig);
                Log.LogLevel = Config.LogLevel;

                // DONE: Cleaning up the packet log
                if (Config.PacketLogging)
                {
                    File.Delete("packets.log");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public Dictionary<OPCODES, HandlePacket> PacketHandlers { get; set; } = new Dictionary<OPCODES, HandlePacket>();
        public XMLConfigFile Config { get; set; }
        public SQL AccountDatabase { get; set; } = new SQL();
        public SQL CharacterDatabase { get; set; } = new SQL();
        public SQL WorldDatabase { get; set; } = new SQL();

        public void AccountSQLEventHandler(SQL.EMessages messageId, string outBuf)
        {
            switch (messageId)
            {
                case var @case when @case == SQL.EMessages.ID_Error:
                    {
                        Log.WriteLine(LogType.FAILED, "[ACCOUNT] " + outBuf);
                        break;
                    }

                case var case1 when case1 == SQL.EMessages.ID_Message:
                    {
                        Log.WriteLine(LogType.SUCCESS, "[ACCOUNT] " + outBuf);
                        break;
                    }

                default:
                    {
                        break;
                    }
            }
        }

        public void CharacterSQLEventHandler(SQL.EMessages messageId, string outBuf)
        {
            switch (messageId)
            {
                case var @case when @case == SQL.EMessages.ID_Error:
                    {
                        Log.WriteLine(LogType.FAILED, "[CHARACTER] " + outBuf);
                        break;
                    }

                case var case1 when case1 == SQL.EMessages.ID_Message:
                    {
                        Log.WriteLine(LogType.SUCCESS, "[CHARACTER] " + outBuf);
                        break;
                    }

                default:
                    {
                        break;
                    }
            }
        }

        public void WorldSQLEventHandler(SQL.EMessages messageId, string outBuf)
        {
            switch (messageId)
            {
                case var @case when @case == SQL.EMessages.ID_Error:
                    {
                        Log.WriteLine(LogType.FAILED, "[WORLD] " + outBuf);
                        break;
                    }

                case var case1 when case1 == SQL.EMessages.ID_Message:
                    {
                        Log.WriteLine(LogType.SUCCESS, "[WORLD] " + outBuf);
                        break;
                    }

                default:
                    {
                        break;
                    }
            }
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        [MTAThread()]
        public void Main()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            AssemblyTitleAttribute assemblyTitleAttribute = (AssemblyTitleAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0];
            Console.Title = $"{assemblyTitleAttribute.Title} v{Assembly.GetExecutingAssembly().GetName().Version}";
            Console.ForegroundColor = ConsoleColor.Yellow;
            AssemblyProductAttribute assemblyProductAttribute = (AssemblyProductAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0];
            Console.WriteLine("{0}", assemblyProductAttribute.Product);
            AssemblyCopyrightAttribute assemblyCopyrightAttribute = (AssemblyCopyrightAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0];
            Console.WriteLine(assemblyCopyrightAttribute.Copyright);
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  __  __      _  _  ___  ___  ___   __   __ ___               ");
            Console.WriteLine(@" |  \/  |__ _| \| |/ __|/ _ \/ __|  \ \ / /| _ )      We Love ");
            Console.WriteLine(@" | |\/| / _` | .` | (_ | (_) \__ \   \ V / | _ \   Vanilla Wow");
            Console.WriteLine(@" |_|  |_\__,_|_|\_|\___|\___/|___/    \_/  |___/              ");
            Console.WriteLine("                                                              ");
            Console.WriteLine(" Website / Forum / Support: https://getmangos.eu/             ");
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.ForegroundColor = ConsoleColor.White;
            AssemblyTitleAttribute assemblyTitleAttribute1 = (AssemblyTitleAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0];
            Console.WriteLine(assemblyTitleAttribute1.Title);
            Console.WriteLine("version {0}", Assembly.GetExecutingAssembly().GetName().Version);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Gray;
            Log.WriteLine(LogType.INFORMATION, "[{0}] World Cluster Starting...", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"));
            AppDomain.CurrentDomain.UnhandledException += GenericExceptionHandler;
            LoadConfig();
            Console.ForegroundColor = ConsoleColor.Gray;
            AccountDatabase.SQLMessage += AccountSQLEventHandler;
            CharacterDatabase.SQLMessage += CharacterSQLEventHandler;
            WorldDatabase.SQLMessage += WorldSQLEventHandler;
            int ReturnValues;
            ReturnValues = AccountDatabase.Connect();
            if (ReturnValues > (int)SQL.ReturnState.Success)   // Ok, An error occurred
            {
                Console.WriteLine("[{0}] An SQL Error has occurred", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"));
                Console.WriteLine("*************************");
                Console.WriteLine("* Press any key to exit *");
                Console.WriteLine("*************************");
                Console.ReadKey();
                Environment.Exit(0);
            }

            AccountDatabase.Update("SET NAMES 'utf8';");
            ReturnValues = CharacterDatabase.Connect();
            if (ReturnValues > (int)SQL.ReturnState.Success)   // Ok, An error occurred
            {
                Console.WriteLine("[{0}] An SQL Error has occurred", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"));
                Console.WriteLine("*************************");
                Console.WriteLine("* Press any key to exit *");
                Console.WriteLine("*************************");
                Console.ReadKey();
                Environment.Exit(0);
            }

            CharacterDatabase.Update("SET NAMES 'utf8';");
            ReturnValues = WorldDatabase.Connect();
            if (ReturnValues > (int)SQL.ReturnState.Success)   // Ok, An error occurred
            {
                Console.WriteLine("[{0}] An SQL Error has occurred", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"));
                Console.WriteLine("*************************");
                Console.WriteLine("* Press any key to exit *");
                Console.WriteLine("*************************");
                Console.ReadKey();
                Environment.Exit(0);
            }

            WorldDatabase.Update("SET NAMES 'utf8';");
            ClusterServiceLocator._WS_DBCLoad.InitializeInternalDatabase();
            ClusterServiceLocator._WC_Handlers.IntializePacketHandlers();
            if (ClusterServiceLocator._CommonGlobalFunctions.CheckRequiredDbVersion(AccountDatabase, ServerDb.Realm) == false)         // Check the Database version, exit if its wrong
            {
                if (true)
                {
                    Console.WriteLine("*************************");
                    Console.WriteLine("* Press any key to exit *");
                    Console.WriteLine("*************************");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
            }

            if (ClusterServiceLocator._CommonGlobalFunctions.CheckRequiredDbVersion(CharacterDatabase, ServerDb.Character) == false)         // Check the Database version, exit if its wrong
            {
                if (true)
                {
                    Console.WriteLine("*************************");
                    Console.WriteLine("* Press any key to exit *");
                    Console.WriteLine("*************************");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
            }

            if (ClusterServiceLocator._CommonGlobalFunctions.CheckRequiredDbVersion(WorldDatabase, ServerDb.World) == false)         // Check the Database version, exit if its wrong
            {
                if (true)
                {
                    Console.WriteLine("*************************");
                    Console.WriteLine("* Press any key to exit *");
                    Console.WriteLine("*************************");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
            }

            ClusterServiceLocator._WC_Network.WorldServer = new WC_Network.WorldServerClass();
            var server = new ProxyServer<WC_Network.WorldServerClass>(IPAddress.Parse(Config.ClusterListenAddress), Config.ClusterListenPort, ClusterServiceLocator._WC_Network.WorldServer);
            Log.WriteLine(LogType.INFORMATION, "Interface UP at: {0}:{1}", Config.ClusterListenAddress, Config.ClusterListenPort);
            GC.Collect();
            if (Process.GetCurrentProcess().PriorityClass != ProcessPriorityClass.High)
            {
                Log.WriteLine(LogType.WARNING, "Setting Process Priority to NORMAL..[done]");
            }
            else
            {
                Log.WriteLine(LogType.WARNING, "Setting Process Priority to HIGH..[done]");
            }

            Log.WriteLine(LogType.INFORMATION, "Load Time: {0}", Strings.Format(DateAndTime.DateDiff(DateInterval.Second, DateAndTime.Now, DateAndTime.Now), "0 seconds"));
            Log.WriteLine(LogType.INFORMATION, "Used memory: {0}", Strings.Format(GC.GetTotalMemory(false), "### ### ##0 bytes"));
            WaitConsoleCommand();
        }

        public void WaitConsoleCommand()
        {
            string tmp = "";
            string[] CommandList;
            string[] cmds;
            var cmd = Array.Empty<string>();
            int varList;
            while (!ClusterServiceLocator._WC_Network.WorldServer.m_flagStopListen)
            {
                try
                {
                    tmp = Log.ReadLine();
                    CommandList = tmp.Split(";");
                    var loopTo = Information.UBound(CommandList);
                    for (varList = Information.LBound(CommandList); varList <= loopTo; varList++)
                    {
                        cmds = Strings.Split(CommandList[varList], " ", 2);
                        if (CommandList[varList].Length > 0)
                        {
                            // <<<<<<<<<<<COMMAND STRUCTURE>>>>>>>>>>
                            switch (cmds[0].ToLower() ?? "")
                            {
                                case "shutdown":
                                    {
                                        Log.WriteLine(LogType.WARNING, "Server shutting down...");
                                        ClusterServiceLocator._WC_Network.WorldServer.m_flagStopListen = true;
                                        break;
                                    }

                                case "info":
                                    {
                                        Log.WriteLine(LogType.INFORMATION, "Used memory: {0}", Strings.Format(GC.GetTotalMemory(false), "### ### ##0 bytes"));
                                        break;
                                    }

                                case "help":
                                    {
                                        Console.ForegroundColor = ConsoleColor.Blue;
                                        Console.WriteLine("'WorldCluster' Command list:");
                                        Console.ForegroundColor = ConsoleColor.White;
                                        Console.WriteLine("---------------------------------");
                                        Console.WriteLine("");
                                        Console.WriteLine("'help' - Brings up the 'WorldCluster' Command list (this).");
                                        Console.WriteLine("");
                                        Console.WriteLine("'info' - Displays used memory.");
                                        Console.WriteLine("");
                                        Console.WriteLine("'shutdown' - Shuts down WorldCluster.");
                                        break;
                                    }

                                default:
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("Error! Cannot find specified command. Please type 'help' for information on console for commands.");
                                        Console.ForegroundColor = ConsoleColor.Gray;
                                        break;
                                    }
                            }
                            // <<<<<<<<<<</END COMMAND STRUCTURE>>>>>>>>>>>>
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.WriteLine(LogType.FAILED, "Error executing command [{0}]. {2}{1}", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), tmp, e.ToString(), Constants.vbCrLf);
                }
            }
        }

        private void GenericExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            Log.WriteLine(LogType.CRITICAL, ex.ToString() + Constants.vbCrLf);
            Log.WriteLine(LogType.FAILED, "Unexpected error has occured. An 'WorldCluster-Error-yyyy-mmm-d-h-mm.log' file has been created. Check your log folder for more information.");
            TextWriter tw;
            tw = new StreamWriter(new FileStream(string.Format("WorldCluster-Error-{0}.log", Strings.Format(DateAndTime.Now, "yyyy-MMM-d-H-mm")), FileMode.Create));
            tw.Write(ex.ToString());
            tw.Close();
        }
    }
}