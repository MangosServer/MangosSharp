//
// Copyright (C) 2013-2025 getMaNGOS <https://www.getmangos.eu>
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

using Mangos.Cluster.Globals;
using Mangos.Cluster.Handlers;
using Mangos.Cluster.Network;
using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using Mangos.Logging;
using Mangos.MySql;
using Mangos.Configuration;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mangos.Cluster;

public class LegacyWorldCluster
{
    private readonly MangosConfiguration mangosConfiguration;
    private readonly ClusterServiceLocator _clusterServiceLocator;

    public LegacyWorldCluster(ClusterServiceLocator clusterServiceLocator, MangosConfiguration mangosConfiguration)
    {
        _clusterServiceLocator = clusterServiceLocator;
        this.mangosConfiguration = mangosConfiguration;
    }

    // Players' containers
    public long ClietniDs;

    public Dictionary<uint, ClientClass> ClienTs = new();
    public ReaderWriterLockSlim CharacteRsLock = new();
    public Dictionary<ulong, WcHandlerCharacter.CharacterObject> CharacteRs = new();
    // Public CHARACTER_NAMEs As New Hashtable

    // System Things...
    public BaseWriter Log = new ColoredConsoleWriter();

    public Random Rnd = Random.Shared;

    public delegate void HandlePacket(PacketClass packet, ClientClass client);

    public void LoadConfig()
    {
        Log.WriteLine(LogType.DEBUG, "[LoadConfig] Entry - Loading database configuration from settings.");

        // DONE: Setting SQL Connections
        var accountDbSettings = Strings.Split(mangosConfiguration.Cluster.AccountDatabase, ";");
        if (accountDbSettings.Length != 6)
        {
            Log.WriteLine(LogType.FAILED, "[LoadConfig] Invalid connect string for the account database! Expected 6 parts, got {0}.", accountDbSettings.Length);
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
            Log.WriteLine(LogType.DEBUG, "[LoadConfig] Account database parsed - Host={0}, Port={1}, DBName={2}, User={3}, Type={4}.", accountDbSettings[2], accountDbSettings[3], accountDbSettings[4], accountDbSettings[0], accountDbSettings[5]);
        }

        var characterDbSettings = Strings.Split(mangosConfiguration.Cluster.CharacterDatabase, ";");
        if (characterDbSettings.Length != 6)
        {
            Log.WriteLine(LogType.FAILED, "[LoadConfig] Invalid connect string for the character database! Expected 6 parts, got {0}.", characterDbSettings.Length);
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
            Log.WriteLine(LogType.DEBUG, "[LoadConfig] Character database parsed - Host={0}, Port={1}, DBName={2}, User={3}, Type={4}.", characterDbSettings[2], characterDbSettings[3], characterDbSettings[4], characterDbSettings[0], characterDbSettings[5]);
        }

        var worldDbSettings = Strings.Split(mangosConfiguration.Cluster.WorldDatabase, ";");
        if (worldDbSettings.Length != 6)
        {
            Log.WriteLine(LogType.FAILED, "[LoadConfig] Invalid connect string for the world database! Expected 6 parts, got {0}.", worldDbSettings.Length);
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
            Log.WriteLine(LogType.DEBUG, "[LoadConfig] World database parsed - Host={0}, Port={1}, DBName={2}, User={3}, Type={4}.", worldDbSettings[2], worldDbSettings[3], worldDbSettings[4], worldDbSettings[0], worldDbSettings[5]);
        }

        Log.WriteLine(LogType.INFORMATION, "[LoadConfig] Database configuration loading completed.");
    }

    private readonly Dictionary<Opcodes, HandlePacket> _packetHandlers = new();

    public Dictionary<Opcodes, HandlePacket> GetPacketHandlers()
    {
        Log.WriteLine(LogType.DEBUG, "[GetPacketHandlers] Entry - Returning packet handlers dictionary with {0} registered handlers.", _packetHandlers.Count);
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
        Log.WriteLine(LogType.DEBUG, "[AccountSqlEventHandler] Entry - messageId={0}, outBuf={1}", messageId, outBuf);
        switch (messageId)
        {
            case var @case when @case == SQL.EMessages.ID_Error:
                {
                    Log.WriteLine(LogType.DEBUG, "[AccountSqlEventHandler] Branch taken: ID_Error");
                    Log.WriteLine(LogType.FAILED, "[ACCOUNT] " + outBuf);
                    break;
                }

            case var case1 when case1 == SQL.EMessages.ID_Message:
                {
                    Log.WriteLine(LogType.DEBUG, "[AccountSqlEventHandler] Branch taken: ID_Message");
                    Log.WriteLine(LogType.SUCCESS, "[ACCOUNT] " + outBuf);
                    break;
                }

            default:
                {
                    Log.WriteLine(LogType.DEBUG, "[AccountSqlEventHandler] Branch taken: unhandled messageId={0}", messageId);
                    break;
                }
        }
    }

    public void CharacterSqlEventHandler(SQL.EMessages messageId, string outBuf)
    {
        Log.WriteLine(LogType.DEBUG, "[CharacterSqlEventHandler] Entry - messageId={0}, outBuf={1}", messageId, outBuf);
        switch (messageId)
        {
            case var @case when @case == SQL.EMessages.ID_Error:
                {
                    Log.WriteLine(LogType.DEBUG, "[CharacterSqlEventHandler] Branch taken: ID_Error");
                    Log.WriteLine(LogType.FAILED, "[CHARACTER] " + outBuf);
                    break;
                }

            case var case1 when case1 == SQL.EMessages.ID_Message:
                {
                    Log.WriteLine(LogType.DEBUG, "[CharacterSqlEventHandler] Branch taken: ID_Message");
                    Log.WriteLine(LogType.SUCCESS, "[CHARACTER] " + outBuf);
                    break;
                }

            default:
                {
                    Log.WriteLine(LogType.DEBUG, "[CharacterSqlEventHandler] Branch taken: unhandled messageId={0}", messageId);
                    break;
                }
        }
    }

    public void WorldSqlEventHandler(SQL.EMessages messageId, string outBuf)
    {
        Log.WriteLine(LogType.DEBUG, "[WorldSqlEventHandler] Entry - messageId={0}, outBuf={1}", messageId, outBuf);
        switch (messageId)
        {
            case var @case when @case == SQL.EMessages.ID_Error:
                {
                    Log.WriteLine(LogType.DEBUG, "[WorldSqlEventHandler] Branch taken: ID_Error");
                    Log.WriteLine(LogType.FAILED, "[WORLD] " + outBuf);
                    break;
                }

            case var case1 when case1 == SQL.EMessages.ID_Message:
                {
                    Log.WriteLine(LogType.DEBUG, "[WorldSqlEventHandler] Branch taken: ID_Message");
                    Log.WriteLine(LogType.SUCCESS, "[WORLD] " + outBuf);
                    break;
                }

            default:
                {
                    Log.WriteLine(LogType.DEBUG, "[WorldSqlEventHandler] Branch taken: unhandled messageId={0}", messageId);
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
        if (new Mangos.MySql.DbVersionChecker(null, _clusterServiceLocator.GlobalConstants).CheckRequiredDbVersion(GetAccountDatabase(), ServerDb.Realm) == false)         // Check the Database version, exit if its wrong
        {
            if (true)
            {
                Console.WriteLine("*************************");
                Console.WriteLine("* Press any key to exit *");
                Console.WriteLine("*************************");
                Environment.Exit(0);
            }
        }

        if (new Mangos.MySql.DbVersionChecker(null, _clusterServiceLocator.GlobalConstants).CheckRequiredDbVersion(GetCharacterDatabase(), ServerDb.Character) == false)         // Check the Database version, exit if its wrong
        {
            if (true)
            {
                Console.WriteLine("*************************");
                Console.WriteLine("* Press any key to exit *");
                Console.WriteLine("*************************");
                Environment.Exit(0);
            }
        }

        if (new Mangos.MySql.DbVersionChecker(null, _clusterServiceLocator.GlobalConstants).CheckRequiredDbVersion(GetWorldDatabase(), ServerDb.World) == false)         // Check the Database version, exit if its wrong
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
        _clusterServiceLocator.ClusterVerifier.Start();

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
