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
using System.Threading.Tasks;
using System.Xml.Serialization;
using Mangos.Cluster.Globals;
using Mangos.Cluster.Handlers;
using Mangos.Cluster.Network;
using Mangos.Common;
using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using Mangos.Common.Logging;
using Mangos.SignalR;
using Microsoft.VisualBasic;

namespace Mangos.Cluster
{
    public partial class WorldCluster
    {
        private readonly ClusterServiceLocator clusterServiceLocator;

        public WorldCluster(ClusterServiceLocator clusterServiceLocator)
        {
            this.clusterServiceLocator = clusterServiceLocator;
        }

        private const string ClusterPath = "configs/WorldCluster.ini";

        // Players' containers
        public long CLIETNIDs = 0L;
        public Dictionary<uint, ClientClass> CLIENTs = new Dictionary<uint, ClientClass>();
        public ReaderWriterLock CHARACTERs_Lock = new ReaderWriterLock();
        public Dictionary<ulong, WcHandlerCharacter.CharacterObject> CHARACTERs = new Dictionary<ulong, WcHandlerCharacter.CharacterObject>();
        // Public CHARACTER_NAMEs As New Hashtable

        // System Things...
        public BaseWriter Log = new BaseWriter();
        public Random Rnd = new Random();

        public delegate void HandlePacket(PacketClass packet, ClientClass client);

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
                SetConfig(xmlConfigFile);
                Console.Write("...");
                StreamReader ostream;
                ostream = new StreamReader(ClusterPath);
                SetConfig((XMLConfigFile)new XmlSerializer(typeof(XMLConfigFile)).Deserialize(ostream));
                ostream.Close();
                Console.WriteLine(".[done]");

                // DONE: Setting SQL Connections
                var AccountDBSettings = Strings.Split(GetConfig().AccountDatabase, ";");
                if (AccountDBSettings.Length != 6)
                {
                    Console.WriteLine("Invalid connect string for the account database!");
                }
                else
                {
                    GetAccountDatabase().SQLDBName = AccountDBSettings[4];
                    GetAccountDatabase().SQLHost = AccountDBSettings[2];
                    GetAccountDatabase().SQLPort = AccountDBSettings[3];
                    GetAccountDatabase().SQLUser = AccountDBSettings[0];
                    GetAccountDatabase().SQLPass = AccountDBSettings[1];
                    GetAccountDatabase().SQLTypeServer = (SQL.DB_Type)Enum.Parse(typeof(SQL.DB_Type), AccountDBSettings[5]);
                }

                var CharacterDBSettings = Strings.Split(GetConfig().CharacterDatabase, ";");
                if (CharacterDBSettings.Length != 6)
                {
                    Console.WriteLine("Invalid connect string for the character database!");
                }
                else
                {
                    GetCharacterDatabase().SQLDBName = CharacterDBSettings[4];
                    GetCharacterDatabase().SQLHost = CharacterDBSettings[2];
                    GetCharacterDatabase().SQLPort = CharacterDBSettings[3];
                    GetCharacterDatabase().SQLUser = CharacterDBSettings[0];
                    GetCharacterDatabase().SQLPass = CharacterDBSettings[1];
                    GetCharacterDatabase().SQLTypeServer = (SQL.DB_Type)Enum.Parse(typeof(SQL.DB_Type), CharacterDBSettings[5]);
                }

                var WorldDBSettings = Strings.Split(GetConfig().WorldDatabase, ";");
                if (WorldDBSettings.Length != 6)
                {
                    Console.WriteLine("Invalid connect string for the world database!");
                }
                else
                {
                    GetWorldDatabase().SQLDBName = WorldDBSettings[4];
                    GetWorldDatabase().SQLHost = WorldDBSettings[2];
                    GetWorldDatabase().SQLPort = WorldDBSettings[3];
                    GetWorldDatabase().SQLUser = WorldDBSettings[0];
                    GetWorldDatabase().SQLPass = WorldDBSettings[1];
                    GetWorldDatabase().SQLTypeServer = (SQL.DB_Type)Enum.Parse(typeof(SQL.DB_Type), WorldDBSettings[5]);
                }

                // DONE: Creating logger
                Log = BaseWriter.CreateLog(GetConfig().LogType, GetConfig().LogConfig);
                Log.LogLevel = GetConfig().LogLevel;

                // DONE: Cleaning up the packet log
                if (GetConfig().PacketLogging)
                {
                    File.Delete("packets.log");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private Dictionary<Opcodes, HandlePacket> packetHandlers = new Dictionary<Opcodes, HandlePacket>();

        public Dictionary<Opcodes, HandlePacket> GetPacketHandlers()
        {
            return packetHandlers;
        }

        public void SetPacketHandlers(Dictionary<Opcodes, HandlePacket> value)
        {
            packetHandlers = value;
        }

        private XMLConfigFile config;

        public XMLConfigFile GetConfig()
        {
            return config;
        }

        public void SetConfig(XMLConfigFile value)
        {
            config = value;
        }

        private SQL accountDatabase = new SQL();

        public SQL GetAccountDatabase()
        {
            return accountDatabase;
        }

        public void SetAccountDatabase(SQL value)
        {
            accountDatabase = value;
        }

        private SQL characterDatabase = new SQL();

        public SQL GetCharacterDatabase()
        {
            return characterDatabase;
        }

        public void SetCharacterDatabase(SQL value)
        {
            characterDatabase = value;
        }

        private SQL worldDatabase = new SQL();

        public SQL GetWorldDatabase()
        {
            return worldDatabase;
        }

        public void SetWorldDatabase(SQL value)
        {
            worldDatabase = value;
        }

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

        public async Task StartAsync()
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
            GetAccountDatabase().SQLMessage += AccountSQLEventHandler;
            GetCharacterDatabase().SQLMessage += CharacterSQLEventHandler;
            GetWorldDatabase().SQLMessage += WorldSQLEventHandler;
            int ReturnValues;
            ReturnValues = GetAccountDatabase().Connect();
            if (ReturnValues > (int)SQL.ReturnState.Success)   // Ok, An error occurred
            {
                Console.WriteLine("[{0}] An SQL Error has occurred", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"));
                Console.WriteLine("*************************");
                Console.WriteLine("* Press any key to exit *");
                Console.WriteLine("*************************");
                Console.ReadKey();
                Environment.Exit(0);
            }

            GetAccountDatabase().Update("SET NAMES 'utf8';");
            ReturnValues = GetCharacterDatabase().Connect();
            if (ReturnValues > (int)SQL.ReturnState.Success)   // Ok, An error occurred
            {
                Console.WriteLine("[{0}] An SQL Error has occurred", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"));
                Console.WriteLine("*************************");
                Console.WriteLine("* Press any key to exit *");
                Console.WriteLine("*************************");
                Console.ReadKey();
                Environment.Exit(0);
            }

            GetCharacterDatabase().Update("SET NAMES 'utf8';");
            ReturnValues = GetWorldDatabase().Connect();
            if (ReturnValues > (int)SQL.ReturnState.Success)   // Ok, An error occurred
            {
                Console.WriteLine("[{0}] An SQL Error has occurred", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"));
                Console.WriteLine("*************************");
                Console.WriteLine("* Press any key to exit *");
                Console.WriteLine("*************************");
                Console.ReadKey();
                Environment.Exit(0);
            }

            GetWorldDatabase().Update("SET NAMES 'utf8';");
            await clusterServiceLocator._WS_DBCLoad.InitializeInternalDatabaseAsync();
            clusterServiceLocator._WC_Handlers.IntializePacketHandlers();
            if (clusterServiceLocator._CommonGlobalFunctions.CheckRequiredDbVersion(GetAccountDatabase(), ServerDb.Realm) == false)         // Check the Database version, exit if its wrong
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

            if (clusterServiceLocator._CommonGlobalFunctions.CheckRequiredDbVersion(GetCharacterDatabase(), ServerDb.Character) == false)         // Check the Database version, exit if its wrong
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

            if (clusterServiceLocator._CommonGlobalFunctions.CheckRequiredDbVersion(GetWorldDatabase(), ServerDb.World) == false)         // Check the Database version, exit if its wrong
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

            clusterServiceLocator._WC_Network.WorldServer = new WorldServerClass(clusterServiceLocator);
            var server = new ProxyServer<WorldServerClass>(IPAddress.Parse(GetConfig().ClusterListenAddress), GetConfig().ClusterListenPort, clusterServiceLocator._WC_Network.WorldServer);
            Log.WriteLine(LogType.INFORMATION, "Interface UP at: {0}:{1}", GetConfig().ClusterListenAddress, GetConfig().ClusterListenPort);
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
            while (!clusterServiceLocator._WC_Network.WorldServer.m_flagStopListen)
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
                                        clusterServiceLocator._WC_Network.WorldServer.m_flagStopListen = true;
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