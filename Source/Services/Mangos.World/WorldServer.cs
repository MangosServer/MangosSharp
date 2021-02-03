//
//  Copyright (C) 2013-2021 getMaNGOS <https://getmangos.eu>
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using Mangos.Common.Legacy;
using Mangos.Common.Legacy.Logging;
using Mangos.Configuration.Xml;
using Mangos.SignalR;
using Mangos.World.Globals;
using Mangos.World.Maps;
using Mangos.World.Network;
using Mangos.World.Objects;
using Mangos.World.Player;
using Mangos.World.Quests;
using Microsoft.VisualBasic;
//using Microsoft.VisualBasic.CompilerServices;
using System.Threading;

namespace Mangos.World
{
    public class WorldServer
    {
        public delegate void HandlePacket(ref Packets.PacketClass Packet, ref WS_Network.ClientClass client);

        private ProxyServer<WS_Network.WorldServerClass> server;

        public Dictionary<uint, WS_Network.ClientClass> CLIENTs;

        public Dictionary<ulong, WS_PlayerData.CharacterObject> CHARACTERs;

        public System.Threading.ReaderWriterLock CHARACTERs_Lock;

        public WS_Quests ALLQUESTS;

        public WS_GraveYards AllGraveYards;

        public Dictionary<int, List<int>> CreatureQuestStarters;

        public Dictionary<int, List<int>> CreatureQuestFinishers;

        public Dictionary<int, List<int>> GameobjectQuestStarters;

        public Dictionary<int, List<int>> GameobjectQuestFinishers;

        public System.Threading.ReaderWriterLock WORLD_CREATUREs_Lock;

        public Dictionary<ulong, WS_Creatures.CreatureObject> WORLD_CREATUREs;

        public ArrayList WORLD_CREATUREsKeys;

        public Dictionary<ulong, WS_GameObjects.GameObjectObject> WORLD_GAMEOBJECTs;

        public Dictionary<ulong, WS_Corpses.CorpseObject> WORLD_CORPSEOBJECTs;

        public System.Threading.ReaderWriterLock WORLD_DYNAMICOBJECTs_Lock;

        public Dictionary<ulong, WS_DynamicObjects.DynamicObjectObject> WORLD_DYNAMICOBJECTs;

        public System.Threading.ReaderWriterLock WORLD_TRANSPORTs_Lock;

        public Dictionary<ulong, WS_Transports.TransportObject> WORLD_TRANSPORTs;

        public Dictionary<ulong, ItemObject> WORLD_ITEMs;

        public Dictionary<int, WS_Items.ItemInfo> ITEMDatabase;

        public Dictionary<int, CreatureInfo> CREATURESDatabase;

        public Dictionary<int, WS_GameObjects.GameObjectInfo> GAMEOBJECTSDatabase;

        public ulong itemGuidCounter;

        public ulong CreatureGUIDCounter;

        public ulong GameObjectsGUIDCounter;

        public ulong CorpseGUIDCounter;

        public ulong DynamicObjectsGUIDCounter;

        public ulong TransportGUIDCounter;

        public BaseWriter Log;

        public Dictionary<Opcodes, HandlePacket> PacketHandlers;

        public Random Rnd;

        public ScriptedObject AreaTriggers;

        public ScriptedObject AI;

        public WS_Network.WorldServerClass ClsWorldServer;

        public const int SERVERSEED = -569166080;
        private readonly XmlConfigurationProvider<WorldServerConfiguration> xmlConfigurationProvider;
        public SQL AccountDatabase;

        public SQL CharacterDatabase;

        public SQL WorldDatabase;

        public WorldServer(XmlConfigurationProvider<WorldServerConfiguration> xmlConfigurationProvider)
        {
            CLIENTs = new Dictionary<uint, WS_Network.ClientClass>();
            CHARACTERs = new Dictionary<ulong, WS_PlayerData.CharacterObject>();
            CHARACTERs_Lock = new System.Threading.ReaderWriterLock();
            ALLQUESTS = new WS_Quests();
            AllGraveYards = new WS_GraveYards(WorldServiceLocator._DataStoreProvider);
            CreatureQuestStarters = new Dictionary<int, List<int>>();
            CreatureQuestFinishers = new Dictionary<int, List<int>>();
            GameobjectQuestStarters = new Dictionary<int, List<int>>();
            GameobjectQuestFinishers = new Dictionary<int, List<int>>();
            WORLD_CREATUREs_Lock = new System.Threading.ReaderWriterLock();
            WORLD_CREATUREs = new Dictionary<ulong, WS_Creatures.CreatureObject>();
            WORLD_CREATUREsKeys = new ArrayList();
            WORLD_GAMEOBJECTs = new Dictionary<ulong, WS_GameObjects.GameObjectObject>();
            WORLD_CORPSEOBJECTs = new Dictionary<ulong, WS_Corpses.CorpseObject>();
            WORLD_DYNAMICOBJECTs_Lock = new System.Threading.ReaderWriterLock();
            WORLD_DYNAMICOBJECTs = new Dictionary<ulong, WS_DynamicObjects.DynamicObjectObject>();
            WORLD_TRANSPORTs_Lock = new System.Threading.ReaderWriterLock();
            WORLD_TRANSPORTs = new Dictionary<ulong, WS_Transports.TransportObject>();
            WORLD_ITEMs = new Dictionary<ulong, ItemObject>();
            ITEMDatabase = new Dictionary<int, WS_Items.ItemInfo>();
            CREATURESDatabase = new Dictionary<int, CreatureInfo>();
            GAMEOBJECTSDatabase = new Dictionary<int, WS_GameObjects.GameObjectInfo>();
            itemGuidCounter = WorldServiceLocator._Global_Constants.GUID_ITEM;
            CreatureGUIDCounter = WorldServiceLocator._Global_Constants.GUID_UNIT;
            GameObjectsGUIDCounter = WorldServiceLocator._Global_Constants.GUID_GAMEOBJECT;
            CorpseGUIDCounter = WorldServiceLocator._Global_Constants.GUID_CORPSE;
            DynamicObjectsGUIDCounter = WorldServiceLocator._Global_Constants.GUID_DYNAMICOBJECT;
            TransportGUIDCounter = WorldServiceLocator._Global_Constants.GUID_MO_TRANSPORT;
            Log = new BaseWriter();
            PacketHandlers = new Dictionary<Opcodes, HandlePacket>();
            Rnd = new Random();
            AccountDatabase = new SQL();
            CharacterDatabase = new SQL();
            WorldDatabase = new SQL();
            this.xmlConfigurationProvider = xmlConfigurationProvider;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public void LoadConfig()
        {
            try
            {
                string FileName = "configs/WorldServer.ini";
                string[] args = Environment.GetCommandLineArgs();
                string[] array = args;
                foreach (var arg in from string arg in array
                                    where arg.IndexOf("config") != -1
                                    select arg)
                {
                    FileName = Strings.Trim(arg.Substring(checked(arg.IndexOf("=") + 1)));
                }

                if (!File.Exists(FileName))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[{0}] Cannot Continue. {1} does not exist.", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), FileName);
                    Console.WriteLine("Please copy the ini files into the same directory as the Server exe files.");
                    Console.WriteLine("Press any key to exit server: ");
                    Console.ReadKey();
                }
                Console.Write("[{0}] Loading Configuration from {1}...", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), FileName);
                WorldServerConfiguration configuration = WorldServiceLocator._ConfigurationProvider.GetConfiguration();
                Console.WriteLine(".[done]");
                if (!configuration.VMapsEnabled)
                {
                    configuration.LineOfSightEnabled = false;
                    configuration.HeightCalcEnabled = false;
                }
                string[] AccountDBSettings = Strings.Split(configuration.AccountDatabase, ";");
                if (AccountDBSettings.Length == 6)
                {
                    AccountDatabase.SQLDBName = AccountDBSettings[4];
                    AccountDatabase.SQLHost = AccountDBSettings[2];
                    AccountDatabase.SQLPort = AccountDBSettings[3];
                    AccountDatabase.SQLUser = AccountDBSettings[0];
                    AccountDatabase.SQLPass = AccountDBSettings[1];
                    AccountDatabase.SQLTypeServer = (SQL.DB_Type)Conversion.Int(Enum.Parse(typeof(SQL.DB_Type), AccountDBSettings[5]));
                }
                else
                {
                    Console.WriteLine("Invalid connect string for the account database!");
                }
                string[] CharacterDBSettings = Strings.Split(configuration.CharacterDatabase, ";");
                if (CharacterDBSettings.Length == 6)
                {
                    CharacterDatabase.SQLDBName = CharacterDBSettings[4];
                    CharacterDatabase.SQLHost = CharacterDBSettings[2];
                    CharacterDatabase.SQLPort = CharacterDBSettings[3];
                    CharacterDatabase.SQLUser = CharacterDBSettings[0];
                    CharacterDatabase.SQLPass = CharacterDBSettings[1];
                    CharacterDatabase.SQLTypeServer = (SQL.DB_Type)Conversion.Int(Enum.Parse(typeof(SQL.DB_Type), CharacterDBSettings[5]));
                }
                else
                {
                    Console.WriteLine("Invalid connect string for the character database!");
                }
                string[] WorldDBSettings = Strings.Split(configuration.WorldDatabase, ";");
                if (WorldDBSettings.Length == 6)
                {
                    WorldDatabase.SQLDBName = WorldDBSettings[4];
                    WorldDatabase.SQLHost = WorldDBSettings[2];
                    WorldDatabase.SQLPort = WorldDBSettings[3];
                    WorldDatabase.SQLUser = WorldDBSettings[0];
                    WorldDatabase.SQLPass = WorldDBSettings[1];
                    WorldDatabase.SQLTypeServer = (SQL.DB_Type)Conversion.Int(Enum.Parse(typeof(SQL.DB_Type), WorldDBSettings[5]));
                }
                else
                {
                    Console.WriteLine("Invalid connect string for the world database!");
                }
                WorldServiceLocator._WS_Maps.RESOLUTION_ZMAP = checked(configuration.MapResolution - 1);
                if (WorldServiceLocator._WS_Maps.RESOLUTION_ZMAP < 63)
                {
                    WorldServiceLocator._WS_Maps.RESOLUTION_ZMAP = 63;
                }
                if (WorldServiceLocator._WS_Maps.RESOLUTION_ZMAP > 255)
                {
                    WorldServiceLocator._WS_Maps.RESOLUTION_ZMAP = 255;
                }
                Log = BaseWriter.CreateLog(configuration.LogType, configuration.LogConfig);
                Log.LogLevel = configuration.LogLevel;
            }
            catch (Exception ex)
            {
                Exception e = ex;
                Console.WriteLine(e.ToString());
            }
        }

        public void AccountSQLEventHandler(SQL.EMessages MessageID, string OutBuf)
        {
            if (OutBuf is null)
            {
                throw new ArgumentNullException(nameof(OutBuf));
            }

            switch (MessageID)
            {
                case SQL.EMessages.ID_Error:
                    Log.WriteLine(LogType.FAILED, "[ACCOUNT] " + OutBuf);
                    break;
                case SQL.EMessages.ID_Message:
                    Log.WriteLine(LogType.SUCCESS, "[ACCOUNT] " + OutBuf);
                    break;
            }
        }

        public void CharacterSQLEventHandler(SQL.EMessages MessageID, string OutBuf)
        {
            if (OutBuf is null)
            {
                throw new ArgumentNullException(nameof(OutBuf));
            }

            switch (MessageID)
            {
                case SQL.EMessages.ID_Error:
                    Log.WriteLine(LogType.FAILED, "[CHARACTER] " + OutBuf);
                    break;
                case SQL.EMessages.ID_Message:
                    Log.WriteLine(LogType.SUCCESS, "[CHARACTER] " + OutBuf);
                    break;
            }
        }

        public void WorldSQLEventHandler(SQL.EMessages MessageID, string OutBuf)
        {
            if (OutBuf is null)
            {
                throw new ArgumentNullException(nameof(OutBuf));
            }

            switch (MessageID)
            {
                case SQL.EMessages.ID_Error:
                    Log.WriteLine(LogType.FAILED, "[WORLD] " + OutBuf);
                    break;
                case SQL.EMessages.ID_Message:
                    Log.WriteLine(LogType.SUCCESS, "[WORLD] " + OutBuf);
                    break;
            }
        }

        [MTAThread]
        public async Task StartAsync()
        {
            xmlConfigurationProvider.LoadFromFile("configs/WorldServer.ini");

            Console.BackgroundColor = ConsoleColor.Black;
            Console.Title = $"{((AssemblyTitleAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), inherit: false)[0]).Title} v{Assembly.GetExecutingAssembly().GetName().Version}";
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("{0}", ((AssemblyProductAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), inherit: false)[0]).Product);
            Console.WriteLine(((AssemblyCopyrightAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), inherit: false)[0]).Copyright);
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  __  __      _  _  ___  ___  ___   __   __ ___               ");
            Console.WriteLine(" |  \\/  |__ _| \\| |/ __|/ _ \\/ __|  \\ \\ / /| _ )      We Love ");
            Console.WriteLine(" | |\\/| / _` | .` | (_ | (_) \\__ \\   \\ V / | _ \\   Vanilla Wow");
            Console.WriteLine(" |_|  |_\\__,_|_|\\_|\\___|\\___/|___/    \\_/  |___/              ");
            Console.WriteLine("                                                              ");
            Console.WriteLine(" Website / Forum / Support: https://getmangos.eu/          ");
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(((AssemblyTitleAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), inherit: false)[0]).Title);
            Console.WriteLine(" version {0}", Assembly.GetExecutingAssembly().GetName().Version);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Gray;
            DateTime dateTimeStarted = DateTime.Now;
            Log.WriteLine(LogType.INFORMATION, "[{0}] World Server Starting...", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"));
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += GenericExceptionHandler;
            LoadConfig();
            Console.ForegroundColor = ConsoleColor.Gray;
            AccountDatabase.SQLMessage += AccountSQLEventHandler;
            CharacterDatabase.SQLMessage += CharacterSQLEventHandler;
            WorldDatabase.SQLMessage += WorldSQLEventHandler;
            int ReturnValues = AccountDatabase.Connect();
            if (ReturnValues > 0)
            {
                Console.WriteLine("[{0}] An SQL Error has occurred", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"));
                Console.WriteLine("*************************");
                Console.WriteLine("* Press any key to exit *");
                Console.WriteLine("*************************");
                Console.ReadKey();
            }
            AccountDatabase.Update("SET NAMES 'utf8';");
            ReturnValues = CharacterDatabase.Connect();
            if (ReturnValues > 0)
            {
                Console.WriteLine("[{0}] An SQL Error has occurred", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"));
                Console.WriteLine("*************************");
                Console.WriteLine("* Press any key to exit *");
                Console.WriteLine("*************************");
                Console.ReadKey();
            }
            CharacterDatabase.Update("SET NAMES 'utf8';");
            ReturnValues = WorldDatabase.Connect();
            if (ReturnValues > 0)
            {
                Console.WriteLine("[{0}] An SQL Error has occurred", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"));
                Console.WriteLine("*************************");
                Console.WriteLine("* Press any key to exit *");
                Console.WriteLine("*************************");
                Console.ReadKey();
            }
            WorldDatabase.Update("SET NAMES 'utf8';");
            bool areDbVersionsOk = true;
            if (!WorldServiceLocator._CommonGlobalFunctions.CheckRequiredDbVersion(AccountDatabase, ServerDb.Realm))
            {
                areDbVersionsOk = false;
            }
            if (!WorldServiceLocator._CommonGlobalFunctions.CheckRequiredDbVersion(CharacterDatabase, ServerDb.Character))
            {
                areDbVersionsOk = false;
            }
            if (!WorldServiceLocator._CommonGlobalFunctions.CheckRequiredDbVersion(WorldDatabase, ServerDb.World))
            {
                areDbVersionsOk = false;
            }
            if (!areDbVersionsOk)
            {
                Console.WriteLine("*************************");
                Console.WriteLine("* Press any key to exit *");
                Console.WriteLine("*************************");
                Console.ReadKey();
            }
            await WorldServiceLocator._WS_DBCDatabase.InitializeInternalDatabaseAsync();
            WorldServiceLocator._WS_Handlers.IntializePacketHandlers();
            ALLQUESTS.LoadAllQuests();
            await AllGraveYards.InitializeGraveyardsAsync();
            WorldServiceLocator._WS_Transports.LoadTransports();
            ClsWorldServer = new WS_Network.WorldServerClass(WorldServiceLocator._DataStoreProvider);
            WorldServerConfiguration configuration = WorldServiceLocator._ConfigurationProvider.GetConfiguration();
            server = new ProxyServer<WS_Network.WorldServerClass>(Dns.GetHostAddresses(configuration.LocalConnectHost)[0], configuration.LocalConnectPort, ClsWorldServer);
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
            Log.WriteLine(LogType.INFORMATION, " Used Memory: {0}", Strings.Format(GC.GetTotalMemory(forceFullCollection: false), "### ### ##0 bytes"));
            try
            {
                WaitConsoleCommand();
            }
            catch (Exception ex2)
            {
                Exception ex = ex2;
                WorldServiceLocator._WS_TimerBasedEvents.Regenerator.Dispose();
                AreaTriggers.Dispose();
            }
        }

        public void WaitConsoleCommand()
        {
            string tmp = "";
            string[] cmd = Array.Empty<string>();
            while (!ClsWorldServer._flagStopListen)
            {
                try
                {
                    tmp = Log.ReadLine();
                    string[] CommandList = tmp.Split(";");
                    int num = Information.LBound(CommandList);
                    int num2 = Information.UBound(CommandList);
                    for (int varList = num; varList <= num2; varList = checked(varList + 1))
                    {
                        string[] cmds = Strings.Split(CommandList[varList], " ", 2);
                        if (CommandList[varList].Length > 0)
                        {
                            switch (cmds[0].ToLower())
                            {
                                case "shutdown":
                                    Log.WriteLine(LogType.WARNING, "Server shutting down...");
                                    ClsWorldServer._flagStopListen = true;
                                    break;
                                case "info":
                                    Log.WriteLine(LogType.INFORMATION, "Used memory: {0}", Strings.Format(GC.GetTotalMemory(forceFullCollection: false), "### ### ##0 bytes"));
                                    break;
                                case "help":
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    Console.WriteLine("'WorldServer' Command list:");
                                    Console.ForegroundColor = ConsoleColor.White;
                                    Console.WriteLine("---------------------------------");
                                    Console.WriteLine("");
                                    Console.WriteLine("");
                                    Console.WriteLine("'help' - Brings up the 'WorldServer' Command list (this).");
                                    Console.WriteLine("");
                                    Console.WriteLine("'info' - Brings up a context menu showing server information (such as memory used).");
                                    Console.WriteLine("");
                                    Console.WriteLine("'shutdown' - Shuts down 'WorldServer'.");
                                    break;
                                default:
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("Error! Cannot find specified command. Please type 'help' for information on 'WorldServer' console commands.");
                                    Console.ForegroundColor = ConsoleColor.Gray;
                                    break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Exception e = ex;
                    Log.WriteLine(LogType.FAILED, "Error executing command [{0}]. {2}{1}", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), tmp, e.ToString(), Environment.NewLine);
                }
            }
        }

        private void GenericExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Exception EX = (Exception)e.ExceptionObject;
            Log.WriteLine(LogType.CRITICAL, EX + Environment.NewLine);
            Log.WriteLine(LogType.FAILED, "Unexpected error has occured. An 'WorldServer-Error-yyyy-mmm-d-h-mm.log' file has been created. Check your log folder for more information.");
            new StreamWriter(new FileStream(string.Format("WorldServer-Error-{0}.log", Strings.Format(DateAndTime.Now, "yyyy-MMM-d-H-mm")), FileMode.Create)).Write(EX.ToString());
            new StreamWriter(new FileStream(string.Format("WorldServer-Error-{0}.log", Strings.Format(DateAndTime.Now, "yyyy-MMM-d-H-mm")), FileMode.Create)).Close();
        }
    }
}
