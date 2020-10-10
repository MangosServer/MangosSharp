// 
// Copyright (C) 2013-2020 getMaNGOS <https://getmangos.eu>
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Xml.Serialization;
using Mangos.Common;
using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using Mangos.Common.Logging;
using Mangos.SignalR;
using Mangos.World.Globals;
using Mangos.World.Maps;
using Mangos.World.Objects;
using Mangos.World.Player;
using Mangos.World.Quests;
using Mangos.World.Server;
using Microsoft.VisualBasic;

namespace Mangos.World
{
    public class WorldServer
    {

        /* TODO ERROR: Skipped RegionDirectiveTrivia */    // Players' containers
        public Dictionary<uint, WS_Network.ClientClass> CLIENTs = new Dictionary<uint, WS_Network.ClientClass>();
        public Dictionary<ulong, WS_PlayerData.CharacterObject> CHARACTERs = new Dictionary<ulong, WS_PlayerData.CharacterObject>();
        public ReaderWriterLock CHARACTERs_Lock = new ReaderWriterLock(); // ReaderWriterLock_Debug("CHARACTERS")
        public WS_Quests ALLQUESTS = new WS_Quests();
        public WS_GraveYards AllGraveYards = new WS_GraveYards();
        public Dictionary<int, List<int>> CreatureQuestStarters = new Dictionary<int, List<int>>();
        public Dictionary<int, List<int>> CreatureQuestFinishers = new Dictionary<int, List<int>>();
        public Dictionary<int, List<int>> GameobjectQuestStarters = new Dictionary<int, List<int>>();
        public Dictionary<int, List<int>> GameobjectQuestFinishers = new Dictionary<int, List<int>>();

        // Worlds containers
        public ReaderWriterLock WORLD_CREATUREs_Lock = new ReaderWriterLock(); // ReaderWriterLock_Debug("CREATURES")
        public Dictionary<ulong, WS_Creatures.CreatureObject> WORLD_CREATUREs = new Dictionary<ulong, WS_Creatures.CreatureObject>();
        public ArrayList WORLD_CREATUREsKeys = new ArrayList();
        public Dictionary<ulong, WS_GameObjects.GameObjectObject> WORLD_GAMEOBJECTs = new Dictionary<ulong, WS_GameObjects.GameObjectObject>();
        public Dictionary<ulong, WS_Corpses.CorpseObject> WORLD_CORPSEOBJECTs = new Dictionary<ulong, WS_Corpses.CorpseObject>();
        public ReaderWriterLock WORLD_DYNAMICOBJECTs_Lock = new ReaderWriterLock();
        public Dictionary<ulong, WS_DynamicObjects.DynamicObjectObject> WORLD_DYNAMICOBJECTs = new Dictionary<ulong, WS_DynamicObjects.DynamicObjectObject>();
        public ReaderWriterLock WORLD_TRANSPORTs_Lock = new ReaderWriterLock();
        public Dictionary<ulong, WS_Transports.TransportObject> WORLD_TRANSPORTs = new Dictionary<ulong, WS_Transports.TransportObject>();
        public Dictionary<ulong, ItemObject> WORLD_ITEMs = new Dictionary<ulong, ItemObject>();

        // Database's containers - READONLY
        public Dictionary<int, WS_Items.ItemInfo> ITEMDatabase = new Dictionary<int, WS_Items.ItemInfo>();
        public Dictionary<int, CreatureInfo> CREATURESDatabase = new Dictionary<int, CreatureInfo>();
        public Dictionary<int, WS_GameObjects.GameObjectInfo> GAMEOBJECTSDatabase = new Dictionary<int, WS_GameObjects.GameObjectInfo>();

        // Other
        public ulong itemGuidCounter = WorldServiceLocator._Global_Constants.GUID_ITEM;
        public ulong CreatureGUIDCounter = WorldServiceLocator._Global_Constants.GUID_UNIT;
        public ulong GameObjectsGUIDCounter = WorldServiceLocator._Global_Constants.GUID_GAMEOBJECT;
        public ulong CorpseGUIDCounter = WorldServiceLocator._Global_Constants.GUID_CORPSE;
        public ulong DynamicObjectsGUIDCounter = WorldServiceLocator._Global_Constants.GUID_DYNAMICOBJECT;
        public ulong TransportGUIDCounter = WorldServiceLocator._Global_Constants.GUID_MO_TRANSPORT;

        // System Things...
        public BaseWriter Log = new BaseWriter();
        public Dictionary<OPCODES, HandlePacket> PacketHandlers = new Dictionary<OPCODES, HandlePacket>();
        public Random Rnd = new Random();

        public delegate void HandlePacket(ref Packets.PacketClass Packet, ref WS_Network.ClientClass client);

        // Scripting Support
        public ScriptedObject AreaTriggers;
        public ScriptedObject AI;
        // Public CharacterCreation As ScriptedObject

        public WS_Network.WorldServerClass ClsWorldServer;
        private const uint V = 0xDE133700;
        public const uint SERVERSEED = V;

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        public XMLConfigFile Config;

        [XmlRoot(ElementName = "WorldServer")]
        public class XMLConfigFile
        {
            // Database Settings
            [XmlElement(ElementName = "AccountDatabase")]
            public string AccountDatabase = "root;mangosVB;localhost;3306;mangosVB;MySQL";
            [XmlElement(ElementName = "CharacterDatabase")]
            public string CharacterDatabase = "root;mangosVB;localhost;3306;mangosVB;MySQL";
            [XmlElement(ElementName = "WorldDatabase")]
            public string WorldDatabase = "root;mangosVB;localhost;3306;mangosVB;MySQL";

            // Server Settings
            [XmlElement(ElementName = "ServerPlayerLimit")]
            public int ServerPlayerLimit = 10;
            [XmlElement(ElementName = "CommandCharacter")]
            public string CommandCharacter = ".";
            [XmlElement(ElementName = "XPRate")]
            public float XPRate = 1.0f;
            [XmlElement(ElementName = "ManaRegenerationRate")]
            public float ManaRegenerationRate = 1.0f;
            [XmlElement(ElementName = "HealthRegenerationRate")]
            public float HealthRegenerationRate = 1.0f;
            [XmlElement(ElementName = "GlobalAuction")]
            public bool GlobalAuction = false;
            [XmlElement(ElementName = "SaveTimer")]
            public int SaveTimer = 120000;
            [XmlElement(ElementName = "WeatherTimer")]
            public int WeatherTimer = 600000;
            [XmlElement(ElementName = "MapResolution")]
            public int MapResolution = 64;
            [XmlArray(ElementName = "HandledMaps")]
            [XmlArrayItem(typeof(string), ElementName = "Map")]
            public List<string> Maps = new List<string>();

            // VMap Settings
            [XmlElement(ElementName = "VMaps")]
            public bool VMapsEnabled = false;
            [XmlElement(ElementName = "VMapLineOfSightCalc")]
            public bool LineOfSightEnabled = false;
            [XmlElement(ElementName = "VMapHeightCalc")]
            public bool HeightCalcEnabled = false;

            // Logging Settings
            [XmlElement(ElementName = "LogType")]
            public string LogType = "FILE";
            [XmlElement(ElementName = "LogLevel")]
            public LogType LogLevel = Common.Enums.Global.LogType.NETWORK;
            [XmlElement(ElementName = "LogConfig")]
            public string LogConfig = "";

            // Other Settings
            [XmlArray(ElementName = "ScriptsCompiler")]
            [XmlArrayItem(typeof(string), ElementName = "Include")]
            public ArrayList CompilerInclude = new ArrayList();
            [XmlElement(ElementName = "CreatePartyInstances")]
            public bool CreatePartyInstances = false;
            [XmlElement(ElementName = "CreateRaidInstances")]
            public bool CreateRaidInstances = false;
            [XmlElement(ElementName = "CreateBattlegrounds")]
            public bool CreateBattlegrounds = false;
            [XmlElement(ElementName = "CreateArenas")]
            public bool CreateArenas = false;
            [XmlElement(ElementName = "CreateOther")]
            public bool CreateOther = false;

            // Cluster Settings
            [XmlElement(ElementName = "ClusterConnectHost")]
            public string ClusterConnectHost = "127.0.0.1";
            [XmlElement(ElementName = "ClusterConnectPort")]
            public int ClusterConnectPort = 50001;
            [XmlElement(ElementName = "LocalConnectHost")]
            public string LocalConnectHost = "127.0.0.1";
            [XmlElement(ElementName = "LocalConnectPort")]
            public int LocalConnectPort = 50002;
        }

        public void LoadConfig()
        {
            try
            {
                string FileName = "configs/WorldServer.ini";

                // Get filename from console arguments
                var args = Environment.GetCommandLineArgs();
                foreach (string arg in args)
                {
                    if (arg.IndexOf("config") != -1)
                    {
                        FileName = Strings.Trim(arg.Substring(arg.IndexOf("=") + 1));
                    }
                }
                // Make sure a config file exists
                if (File.Exists(FileName) == false)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[{0}] Cannot Continue. {1} does not exist.", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), FileName);
                    Console.WriteLine("Please copy the ini files into the same directory as the Server exe files.");
                    Console.WriteLine("Press any key to exit server: ");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
                // Load config
                Console.Write("[{0}] Loading Configuration from {1}...", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), FileName);
                Config = new XMLConfigFile();
                Console.Write("...");
                var oXS = new XmlSerializer(typeof(XMLConfigFile));
                Console.Write("...");
                StreamReader oStmR;
                oStmR = new StreamReader(FileName);
                Config = (XMLConfigFile)oXS.Deserialize(oStmR);
                oStmR.Close();
                Console.WriteLine(".[done]");

                // DONE: Make sure VMap functionality is disabled with VMaps
                if (!Config.VMapsEnabled)
                {
                    Config.LineOfSightEnabled = false;
                    Config.HeightCalcEnabled = false;
                }

                // DONE: Setting SQL Connections
                var AccountDBSettings = Strings.Split(Config.AccountDatabase, ";");
                if (AccountDBSettings.Length == 6)
                {
                    AccountDatabase.SQLDBName = AccountDBSettings[4];
                    AccountDatabase.SQLHost = AccountDBSettings[2];
                    AccountDatabase.SQLPort = AccountDBSettings[3];
                    AccountDatabase.SQLUser = AccountDBSettings[0];
                    AccountDatabase.SQLPass = AccountDBSettings[1];
                    AccountDatabase.SQLTypeServer = (SQL.DB_Type)Enum.Parse(typeof(SQL.DB_Type), AccountDBSettings[5]);
                }
                else
                {
                    Console.WriteLine("Invalid connect string for the account database!");
                }

                var CharacterDBSettings = Strings.Split(Config.CharacterDatabase, ";");
                if (CharacterDBSettings.Length == 6)
                {
                    CharacterDatabase.SQLDBName = CharacterDBSettings[4];
                    CharacterDatabase.SQLHost = CharacterDBSettings[2];
                    CharacterDatabase.SQLPort = CharacterDBSettings[3];
                    CharacterDatabase.SQLUser = CharacterDBSettings[0];
                    CharacterDatabase.SQLPass = CharacterDBSettings[1];
                    CharacterDatabase.SQLTypeServer = (SQL.DB_Type)Enum.Parse(typeof(SQL.DB_Type), CharacterDBSettings[5]);
                }
                else
                {
                    Console.WriteLine("Invalid connect string for the character database!");
                }

                var WorldDBSettings = Strings.Split(Config.WorldDatabase, ";");
                if (WorldDBSettings.Length == 6)
                {
                    WorldDatabase.SQLDBName = WorldDBSettings[4];
                    WorldDatabase.SQLHost = WorldDBSettings[2];
                    WorldDatabase.SQLPort = WorldDBSettings[3];
                    WorldDatabase.SQLUser = WorldDBSettings[0];
                    WorldDatabase.SQLPass = WorldDBSettings[1];
                    WorldDatabase.SQLTypeServer = (SQL.DB_Type)Enum.Parse(typeof(SQL.DB_Type), WorldDBSettings[5]);
                }
                else
                {
                    Console.WriteLine("Invalid connect string for the world database!");
                }

                WorldServiceLocator._WS_Maps.RESOLUTION_ZMAP = Config.MapResolution - 1;
                if (WorldServiceLocator._WS_Maps.RESOLUTION_ZMAP < 63)
                    WorldServiceLocator._WS_Maps.RESOLUTION_ZMAP = 63;
                if (WorldServiceLocator._WS_Maps.RESOLUTION_ZMAP > 255)
                    WorldServiceLocator._WS_Maps.RESOLUTION_ZMAP = 255;

                // DONE: Creating logger
                Log = CreateLog(Config.LogType, Config.LogConfig);
                Log.LogLevel = Config.LogLevel;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public SQL AccountDatabase = new SQL();
        public SQL CharacterDatabase = new SQL();
        public SQL WorldDatabase = new SQL();

        public void AccountSQLEventHandler(SQL.EMessages MessageID, string OutBuf)
        {
            switch (MessageID)
            {
                case var @case when @case == SQL.EMessages.ID_Error:
                    {
                        Log.WriteLine(LogType.FAILED, "[ACCOUNT] " + OutBuf);
                        break;
                    }

                case var case1 when case1 == SQL.EMessages.ID_Message:
                    {
                        Log.WriteLine(LogType.SUCCESS, "[ACCOUNT] " + OutBuf);
                        break;
                    }
            }
        }

        public void CharacterSQLEventHandler(SQL.EMessages MessageID, string OutBuf)
        {
            switch (MessageID)
            {
                case var @case when @case == SQL.EMessages.ID_Error:
                    {
                        Log.WriteLine(LogType.FAILED, "[CHARACTER] " + OutBuf);
                        break;
                    }

                case var case1 when case1 == SQL.EMessages.ID_Message:
                    {
                        Log.WriteLine(LogType.SUCCESS, "[CHARACTER] " + OutBuf);
                        break;
                    }
            }
        }

        public void WorldSQLEventHandler(SQL.EMessages MessageID, string OutBuf)
        {
            switch (MessageID)
            {
                case var @case when @case == SQL.EMessages.ID_Error:
                    {
                        Log.WriteLine(LogType.FAILED, "[WORLD] " + OutBuf);
                        break;
                    }

                case var case1 when case1 == SQL.EMessages.ID_Message:
                    {
                        Log.WriteLine(LogType.SUCCESS, "[WORLD] " + OutBuf);
                        break;
                    }
            }
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        [MTAThread()]
        public void Main()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Title = string.Format("{0} v{1}", ((AssemblyTitleAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]).Title, Assembly.GetExecutingAssembly().GetName().Version);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("{0}", ((AssemblyProductAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0]).Product);
            Console.WriteLine(((AssemblyCopyrightAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0]).Copyright);
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  __  __      _  _  ___  ___  ___   __   __ ___               ");
            Console.WriteLine(@" |  \/  |__ _| \| |/ __|/ _ \/ __|  \ \ / /| _ )      We Love ");
            Console.WriteLine(@" | |\/| / _` | .` | (_ | (_) \__ \   \ V / | _ \   Vanilla Wow");
            Console.WriteLine(@" |_|  |_\__,_|_|\_|\___|\___/|___/    \_/  |___/              ");
            Console.WriteLine("                                                              ");
            Console.WriteLine(" Website / Forum / Support: https://getmangos.eu/          ");
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(((AssemblyTitleAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]).Title);
            Console.WriteLine(" version {0}", Assembly.GetExecutingAssembly().GetName().Version);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Gray;
            var dateTimeStarted = DateAndTime.Now;
            Log.WriteLine(LogType.INFORMATION, "[{0}] World Server Starting...", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"));
            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += GenericExceptionHandler;
            LoadConfig();
            Console.ForegroundColor = ConsoleColor.Gray;
            AccountDatabase.SQLMessage += AccountSQLEventHandler;
            CharacterDatabase.SQLMessage += CharacterSQLEventHandler;
            WorldDatabase.SQLMessage += WorldSQLEventHandler;
            int ReturnValues;
            ReturnValues = AccountDatabase.Connect();
            if (ReturnValues > SQL.ReturnState.Success)   // Ok, An error occurred
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
            if (ReturnValues > SQL.ReturnState.Success)   // Ok, An error occurred
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
            if (ReturnValues > SQL.ReturnState.Success)   // Ok, An error occurred
            {
                Console.WriteLine("[{0}] An SQL Error has occurred", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"));
                Console.WriteLine("*************************");
                Console.WriteLine("* Press any key to exit *");
                Console.WriteLine("*************************");
                Console.ReadKey();
                Environment.Exit(0);
            }

            WorldDatabase.Update("SET NAMES 'utf8';");

            // Check the Database version, exit if its wrong
            bool areDbVersionsOk = true;
            if (WorldServiceLocator._CommonGlobalFunctions.CheckRequiredDbVersion(AccountDatabase, ServerDb.Realm) == false)
                areDbVersionsOk = false;
            if (WorldServiceLocator._CommonGlobalFunctions.CheckRequiredDbVersion(CharacterDatabase, ServerDb.Character) == false)
                areDbVersionsOk = false;
            if (WorldServiceLocator._CommonGlobalFunctions.CheckRequiredDbVersion(WorldDatabase, ServerDb.World) == false)
                areDbVersionsOk = false;
            if (areDbVersionsOk == false)
            {
                Console.WriteLine("*************************");
                Console.WriteLine("* Press any key to exit *");
                Console.WriteLine("*************************");
                Console.ReadKey();
                Environment.Exit(0);
            }

            WorldServiceLocator._WS_DBCDatabase.InitializeInternalDatabase();
            WorldServiceLocator._WS_Handlers.IntializePacketHandlers();

            /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
            ALLQUESTS.LoadAllQuests();
            AllGraveYards.InitializeGraveyards();
            WorldServiceLocator._WS_Transports.LoadTransports();
            ClsWorldServer = new WS_Network.WorldServerClass();
            var server = new ProxyServer<WS_Network.WorldServerClass>(Dns.GetHostAddresses(Config.LocalConnectHost)[0], Config.LocalConnectPort, ClsWorldServer);
            ClsWorldServer.ClusterConnect();
            Log.WriteLine(LogType.INFORMATION, "Interface UP at: {0}", ClsWorldServer.LocalURI);
            GC.Collect();
            if (Process.GetCurrentProcess().PriorityClass == ProcessPriorityClass.High)
            {
                Log.WriteLine(LogType.WARNING, "Setting Process Priority to HIGH..[done]");
            }
            else
            {
                Log.WriteLine(LogType.WARNING, "Setting Process Priority to NORMAL..[done]");
            }

            Log.WriteLine(LogType.INFORMATION, " Load Time:   {0}", Strings.Format(DateAndTime.DateDiff(DateInterval.Second, dateTimeStarted, DateAndTime.Now), "0 seconds"));
            Log.WriteLine(LogType.INFORMATION, " Used Memory: {0}", Strings.Format(GC.GetTotalMemory(false), "### ### ##0 bytes"));
            WaitConsoleCommand();
            try
            {
            }
            catch (Exception)
            {
                WorldServiceLocator._WS_TimerBasedEvents.Regenerator.Dispose();
                AreaTriggers.Dispose();
            }
        }

        public void WaitConsoleCommand()
        {
            string tmp = "";
            string[] CommandList;
            string[] cmds;
            var cmd = Array.Empty<string>();
            int varList;
            while (!ClsWorldServer._flagStopListen)
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
                                        ClsWorldServer._flagStopListen = true;
                                        break;
                                    }

                                // Case "createaccount" 'ToDo: Fix create account command.
                                // AccountDatabase.InsertSQL([String].Format("INSERT INTO accounts (account, password, email, joindate, last_ip) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}')", cmd(1), cmd(2), cmd(3), Format(Now, "yyyy-MM-dd"), "0.0.0.0"))
                                // If AccountDatabase.QuerySQL("SELECT * FROM accounts WHERE account = "" & packet_account & "";") Then
                                // Console.ForegroundColor = ConsoleColor.DarkGreen
                                // Console.WriteLine("[Account: " & cmd(1) & " Password: " & cmd(2) & " Email: " & cmd(3) & "] has been created.")
                                // Console.ForegroundColor = ConsoleColor.Gray
                                // Else
                                // Console.ForegroundColor = ConsoleColor.Red
                                // Console.WriteLine("[Account: " & cmd(1) & " Password: " & cmd(2) & " Email: " & cmd(3) & "] could not be created.")
                                // Console.ForegroundColor = ConsoleColor.Gray
                                // End If

                                case "info":
                                    {
                                        Log.WriteLine(LogType.INFORMATION, "Used memory: {0}", Strings.Format(GC.GetTotalMemory(false), "### ### ##0 bytes"));
                                        break;
                                    }

                                case "help":
                                    {
                                        Console.ForegroundColor = ConsoleColor.Blue;
                                        Console.WriteLine("'WorldServer' Command list:");
                                        Console.ForegroundColor = ConsoleColor.White;
                                        Console.WriteLine("---------------------------------");
                                        Console.WriteLine("");
                                        Console.WriteLine("");
                                        Console.WriteLine("'help' - Brings up the 'WorldServer' Command list (this).");
                                        Console.WriteLine("");
                                        // Console.WriteLine("'create account <user> <password> <email>' - Creates an account with the specified username <user>, password <password>, and email <email>.")
                                        // Console.WriteLine("")
                                        Console.WriteLine("'info' - Brings up a context menu showing server information (such as memory used).");
                                        Console.WriteLine("");
                                        Console.WriteLine("'shutdown' - Shuts down 'WorldServer'.");
                                        break;
                                    }

                                default:
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("Error! Cannot find specified command. Please type 'help' for information on 'WorldServer' console commands.");
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
                    Log.WriteLine(LogType.FAILED, "Error executing command [{0}]. {2}{1}", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), tmp, e.ToString(), Environment.NewLine);
                }
            }
        }

        private void GenericExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Exception EX;
            EX = (Exception)e.ExceptionObject;
            Log.WriteLine(LogType.CRITICAL, EX.ToString() + Environment.NewLine);
            Log.WriteLine(LogType.FAILED, "Unexpected error has occured. An 'WorldServer-Error-yyyy-mmm-d-h-mm.log' file has been created. Check your log folder for more information.");
            TextWriter tw;
            tw = new StreamWriter(new FileStream(string.Format("WorldServer-Error-{0}.log", Strings.Format(DateAndTime.Now, "yyyy-MMM-d-H-mm")), FileMode.Create));
            tw.Write(EX.ToString());
            tw.Close();
        }
    }
}