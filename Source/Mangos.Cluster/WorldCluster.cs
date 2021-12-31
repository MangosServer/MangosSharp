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
using Mangos.Cluster.Globals;
using Mangos.Cluster.Handlers;
using Mangos.Cluster.Network;
using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using Mangos.Common.Legacy;
using Mangos.Common.Legacy.Logging;
using Mangos.Configuration;
using Mangos.SignalR;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Mangos.Cluster;

public class WorldCluster
{
    private readonly ClusterServiceLocator _clusterServiceLocator;
    private readonly IConfigurationProvider<ClusterConfiguration> configurationProvider;

    public WorldCluster(
        ClusterServiceLocator clusterServiceLocator,
        IConfigurationProvider<ClusterConfiguration> configurationProvider)
    {
        _clusterServiceLocator = clusterServiceLocator;
        this.configurationProvider = configurationProvider;
    }

    // Players' containers
    public long ClietniDs;

    public Dictionary<uint, ClientClass> ClienTs = new();
    public ReaderWriterLock CharacteRsLock = new();
    public Dictionary<ulong, WcHandlerCharacter.CharacterObject> CharacteRs = new();
    // Public CHARACTER_NAMEs As New Hashtable

    // System Things...
    public BaseWriter Log = new();

    public Random Rnd = new();

    public delegate void HandlePacket(PacketClass packet, ClientClass client);

    public void LoadConfig()
    {
        // DONE: Setting SQL Connections
        var accountDbSettings = Strings.Split(configurationProvider.GetConfiguration().AccountDatabase, ";");
        if (accountDbSettings.Length != 6)
        {
            Console.WriteLine("Invalid connect string for the account database!");
        }
        else
        {
            GetAccountDatabase().SQLDBName = accountDbSettings[4];
            GetAccountDatabase().SQLHost = accountDbSettings[2];
            GetAccountDatabase().SQLPort = accountDbSettings[3];
            GetAccountDatabase().SQLUser = accountDbSettings[0];
            GetAccountDatabase().SQLPass = accountDbSettings[1];
            GetAccountDatabase().SQLTypeServer = (SQL.DB_Type)Enum.Parse(typeof(SQL.DB_Type), accountDbSettings[5]);
        }

        var characterDbSettings = Strings.Split(configurationProvider.GetConfiguration().CharacterDatabase, ";");
        if (characterDbSettings.Length != 6)
        {
            Console.WriteLine("Invalid connect string for the character database!");
        }
        else
        {
            GetCharacterDatabase().SQLDBName = characterDbSettings[4];
            GetCharacterDatabase().SQLHost = characterDbSettings[2];
            GetCharacterDatabase().SQLPort = characterDbSettings[3];
            GetCharacterDatabase().SQLUser = characterDbSettings[0];
            GetCharacterDatabase().SQLPass = characterDbSettings[1];
            GetCharacterDatabase().SQLTypeServer = (SQL.DB_Type)Enum.Parse(typeof(SQL.DB_Type), characterDbSettings[5]);
        }

        var worldDbSettings = Strings.Split(configurationProvider.GetConfiguration().WorldDatabase, ";");
        if (worldDbSettings.Length != 6)
        {
            Console.WriteLine("Invalid connect string for the world database!");
        }
        else
        {
            GetWorldDatabase().SQLDBName = worldDbSettings[4];
            GetWorldDatabase().SQLHost = worldDbSettings[2];
            GetWorldDatabase().SQLPort = worldDbSettings[3];
            GetWorldDatabase().SQLUser = worldDbSettings[0];
            GetWorldDatabase().SQLPass = worldDbSettings[1];
            GetWorldDatabase().SQLTypeServer = (SQL.DB_Type)Enum.Parse(typeof(SQL.DB_Type), worldDbSettings[5]);
        }

        // DONE: Creating logger
        Log = BaseWriter.CreateLog(configurationProvider.GetConfiguration().LogType, configurationProvider.GetConfiguration().LogConfig);
        Log.LogLevel = configurationProvider.GetConfiguration().LogLevel;

        // DONE: Cleaning up the packet log
        if (configurationProvider.GetConfiguration().PacketLogging)
        {
            File.Delete("packets.log");
        }
    }

    private readonly Dictionary<Opcodes, HandlePacket> _packetHandlers = new();

    public Dictionary<Opcodes, HandlePacket> GetPacketHandlers()
    {
        return _packetHandlers;
    }

    private readonly SQL _accountDatabase = new();

    public SQL GetAccountDatabase()
    {
        return _accountDatabase;
    }

    private readonly SQL _characterDatabase = new();

    public SQL GetCharacterDatabase()
    {
        return _characterDatabase;
    }

    private readonly SQL _worldDatabase = new();

    public SQL GetWorldDatabase()
    {
        return _worldDatabase;
    }

    public void AccountSqlEventHandler(SQL.EMessages messageId, string outBuf)
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
        }
    }

    public void CharacterSqlEventHandler(SQL.EMessages messageId, string outBuf)
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
        }
    }

    public void WorldSqlEventHandler(SQL.EMessages messageId, string outBuf)
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
        }
    }

    public async Task StartAsync()
    {
        LoadConfig();
        GetAccountDatabase().SQLMessage += AccountSqlEventHandler;
        GetCharacterDatabase().SQLMessage += CharacterSqlEventHandler;
        GetWorldDatabase().SQLMessage += WorldSqlEventHandler;
        int returnValues;
        returnValues = GetAccountDatabase().Connect();
        if (returnValues > (int)SQL.ReturnState.Success)   // Ok, An error occurred
        {
            Console.WriteLine("[{0}] An SQL Error has occurred", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"));
            Console.WriteLine("*************************");
            Console.WriteLine("* Press any key to exit *");
            Console.WriteLine("*************************");
            Console.ReadKey();
            Environment.Exit(0);
        }

        GetAccountDatabase().Update("SET NAMES 'utf8';");
        returnValues = GetCharacterDatabase().Connect();
        if (returnValues > (int)SQL.ReturnState.Success)   // Ok, An error occurred
        {
            Console.WriteLine("[{0}] An SQL Error has occurred", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"));
            Console.WriteLine("*************************");
            Console.WriteLine("* Press any key to exit *");
            Console.WriteLine("*************************");
            Console.ReadKey();
            Environment.Exit(0);
        }

        GetCharacterDatabase().Update("SET NAMES 'utf8';");
        returnValues = GetWorldDatabase().Connect();
        if (returnValues > (int)SQL.ReturnState.Success)   // Ok, An error occurred
        {
            Console.WriteLine("[{0}] An SQL Error has occurred", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"));
            Console.WriteLine("*************************");
            Console.WriteLine("* Press any key to exit *");
            Console.WriteLine("*************************");
            Environment.Exit(0);
        }

        GetWorldDatabase().Update("SET NAMES 'utf8';");
        await _clusterServiceLocator.WsDbcLoad.InitializeInternalDatabaseAsync();
        _clusterServiceLocator.WcHandlers.IntializePacketHandlers();
        if (_clusterServiceLocator.CommonGlobalFunctions.CheckRequiredDbVersion(GetAccountDatabase(), ServerDb.Realm) == false)         // Check the Database version, exit if its wrong
        {
            if (true)
            {
                Console.WriteLine("*************************");
                Console.WriteLine("* Press any key to exit *");
                Console.WriteLine("*************************");
                Environment.Exit(0);
            }
        }

        if (_clusterServiceLocator.CommonGlobalFunctions.CheckRequiredDbVersion(GetCharacterDatabase(), ServerDb.Character) == false)         // Check the Database version, exit if its wrong
        {
            if (true)
            {
                Console.WriteLine("*************************");
                Console.WriteLine("* Press any key to exit *");
                Console.WriteLine("*************************");
                Environment.Exit(0);
            }
        }

        if (_clusterServiceLocator.CommonGlobalFunctions.CheckRequiredDbVersion(GetWorldDatabase(), ServerDb.World) == false)         // Check the Database version, exit if its wrong
        {
            if (true)
            {
                Console.WriteLine("*************************");
                Console.WriteLine("* Press any key to exit *");
                Console.WriteLine("*************************");
                Environment.Exit(0);
            }
        }

        _clusterServiceLocator.WorldServerClass.Start();

        var configuration = configurationProvider.GetConfiguration();
        ProxyServer<WorldServerClass> server = new(
            IPAddress.Parse(configuration.ClusterListenAddress),
            configuration.ClusterListenPort,
            _clusterServiceLocator.WcNetwork.WorldServer);
        Log.WriteLine(LogType.INFORMATION, "Interface UP at: {0}:{1}",
            configuration.ClusterListenAddress,
            configuration.ClusterListenPort);
        Log.WriteLine(LogType.INFORMATION, "Load Time: {0}", Strings.Format(DateAndTime.DateDiff(DateInterval.Second, DateAndTime.Now, DateAndTime.Now), "0 seconds"));
        Log.WriteLine(LogType.INFORMATION, "Used memory: {0}", Strings.Format(GC.GetTotalMemory(false), "### ### ##0 bytes"));
    }

    public void WaitConsoleCommand()
    {
        var tmp = "";
        string[] commandList;
        string[] cmds;
        var cmd = Array.Empty<string>();
        int varList;
        while (!_clusterServiceLocator.WcNetwork.WorldServer.MFlagStopListen)
        {
            try
            {
                tmp = Log.ReadLine();
                commandList = tmp.Split(";");
                var loopTo = Information.UBound(commandList);
                for (varList = Information.LBound(commandList); varList <= loopTo; varList++)
                {
                    cmds = Strings.Split(commandList[varList], " ", 2);
                    if (commandList[varList].Length > 0)
                    {
                        // <<<<<<<<<<<COMMAND STRUCTURE>>>>>>>>>>
                        switch (cmds[0].ToLower() ?? "")
                        {
                            case "shutdown":
                                {
                                    Log.WriteLine(LogType.WARNING, "Server shutting down...");
                                    _clusterServiceLocator.WcNetwork.WorldServer.MFlagStopListen = true;
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
}
