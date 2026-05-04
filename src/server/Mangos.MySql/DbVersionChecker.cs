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

using System;
using System.Data;
using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using Mangos.Logging;
using MySqlConnector;

namespace Mangos.MySql;

// Checks if the database schema version matches the required core version.
// Provides both synchronous and asynchronous version checking.
public class DbVersionChecker
{
    private readonly MangosGlobalConstants _globalConstants;
    private readonly IMangosLogger _logger;

    // Represents database version information.
    private record DbVersionInfo(int Version, int Structure, int Content);

    public DbVersionChecker(IMangosLogger logger, MangosGlobalConstants mangosGlobalConstants)
    {
        _logger = logger; // Allow null for backward compatibility
        _globalConstants = mangosGlobalConstants ?? throw new ArgumentNullException(nameof(mangosGlobalConstants));
    }

    // Checks if the database version matches the required core version.
    // database: The database to check.
    // serverDb: The type of database being checked.
    // Returns: True if version is compatible or content mismatch (warnings), false if version mismatch (fatal).
    public bool CheckRequiredDbVersion(SQL database, ServerDb serverDb)
    {
        var (queryCode, result) = database.QueryResult("SELECT `version`,`structure`,`content` FROM db_version ORDER BY version DESC, structure DESC, content DESC LIMIT 0,1");

        if (queryCode != (int)SQL.ReturnState.Success)
        {
            LogDatabaseCheckFailure(database.SQLDBName, "Failed to query database version");
            return false;
        }

        var expectedVersion = GetExpectedVersion(serverDb);
        if (expectedVersion is null)
        {
            return false;
        }

        if (result.Rows.Count == 0)
        {
            LogMissingVersionTable(database.SQLDBName, expectedVersion);
            return false;
        }

        var dbVersion = ExtractVersionInfo(result);
        return ValidateVersion(database.SQLDBName, dbVersion, expectedVersion);
    }

    // Checks if the database version matches the required core version using a MySqlConnection.
    // connection: The MySqlConnection to use for the check.
    // databaseName: The name of the database being checked.
    // serverDb: The type of database being checked.
    // Returns: True if version is compatible or content mismatch (warnings), false if version mismatch (fatal).
    public bool CheckRequiredDbVersion(MySqlConnection connection, string databaseName, ServerDb serverDb)
    {
        try
        {
            using var command = new MySqlCommand("SELECT `version`,`structure`,`content` FROM db_version ORDER BY version DESC, structure DESC, content DESC LIMIT 0,1", connection);
            using var adapter = new MySqlDataAdapter(command);
            var result = new DataTable();
            adapter.Fill(result);

            var expectedVersion = GetExpectedVersion(serverDb);
            if (expectedVersion is null)
            {
                return false;
            }

            if (result.Rows.Count == 0)
            {
                LogMissingVersionTable(databaseName, expectedVersion);
                return false;
            }

            var dbVersion = ExtractVersionInfo(result);
            return ValidateVersion(databaseName, dbVersion, expectedVersion);
        }
        catch (Exception ex)
        {
            LogDatabaseCheckFailure(databaseName, $"Failed to query database version: {ex.Message}");
            return false;
        }
    }

    // Gets the expected version for the given server database type.
    private DbVersionInfo? GetExpectedVersion(ServerDb serverDb)
    {
        return serverDb switch
        {
            ServerDb.Realm => new DbVersionInfo(
                _globalConstants.RevisionDbRealmVersion,
                _globalConstants.RevisionDbRealmStructure,
                _globalConstants.RevisionDbRealmContent),

            ServerDb.Character => new DbVersionInfo(
                _globalConstants.RevisionDbCharactersVersion,
                _globalConstants.RevisionDbCharactersStructure,
                _globalConstants.RevisionDbCharactersContent),

            ServerDb.World => new DbVersionInfo(
                _globalConstants.RevisionDbMangosVersion,
                _globalConstants.RevisionDbMangosStructure,
                _globalConstants.RevisionDbMangosContent),

            _ => null
        };
    }

    // Extracts version information from a query result row.
    private static DbVersionInfo ExtractVersionInfo(DataTable result)
    {
        var row = result.Rows[0];
        return new DbVersionInfo(row.As<int>("version"), row.As<int>("structure"), row.As<int>("content"));
    }

    // Validates that the database version matches the expected version.
    private bool ValidateVersion(string dbName, DbVersionInfo actual, DbVersionInfo expected)
    {
        // Perfect match
        if (actual.Version == expected.Version && actual.Structure == expected.Structure && actual.Content == expected.Content)
        {
            _logger.Database($"Database version matched for '{dbName}'");
            return true;
        }

        // Content mismatch but compatible (warning level)
        if (actual.Version == expected.Version && actual.Structure == expected.Structure && actual.Content != expected.Content)
        {
            _logger.Warning("--------------------------------------------------------------");
            _logger.Warning("-- WARNING: CONTENT VERSION MISMATCH                        --");
            _logger.Warning("--------------------------------------------------------------");
            _logger.Warning($"Your database '{dbName}' requires updating.");
            _logger.Warning($"You have: Rev{actual.Version}.{actual.Structure}.{actual.Content}, " +
                         $"however the core expects Rev{expected.Version}.{expected.Structure}.{expected.Content}");
            _logger.Warning("The server will run, but you may be missing some database fixes");
            return true;
        }

        // Version or structure mismatch (fatal)
        _logger.Critical("--------------------------------------------------------------");
        _logger.Critical("-- CRITICAL ERROR: DATABASE VERSION MISMATCH                --");
        _logger.Critical("--------------------------------------------------------------");
        _logger.Critical($"Your database '{dbName}' requires updating.");
        _logger.Critical($"You have: Rev{actual.Version}.{actual.Structure}.{actual.Content}, " +
                         $"but the core requires Rev{expected.Version}.{expected.Structure}.{expected.Content}");
        _logger.Critical("The server is unable to run until the required updates are applied.");
        _logger.Critical("--------------------------------------------------------------");
        _logger.Critical($"Please apply all updates after Rev{expected.Version}.{expected.Structure}.{expected.Content}");
        _logger.Critical("These updates are located in the sql/updates folder.");
        _logger.Critical("--------------------------------------------------------------");
        return false;
    }

    // Logs when the database version check fails.
    private void LogDatabaseCheckFailure(string dbName, string reason)
    {
        _logger.Failed("--------------------------------------------------------------");
        _logger.Failed("-- DATABASE VERSION CHECK FAILURE                           --");
        _logger.Failed("--------------------------------------------------------------");
        _logger.Failed($"Failed to check version for database '{dbName}': {reason}");
        _logger.Failed("--------------------------------------------------------------");
    }

    // Logs when the db_version table is missing.
    private void LogMissingVersionTable(string dbName, DbVersionInfo expected)
    {
        _logger.Alert("--------------------------------------------------------------");
        _logger.Alert("-- MISSING VERSION TABLE                                    --");
        _logger.Alert("--------------------------------------------------------------");
        _logger.Alert($"The table 'db_version' is missing in database '{dbName}'");
        _logger.Alert("This database is likely not properly set up or is too old.");
        _logger.Alert($"The core requires database schema Rev{expected.Version}.{expected.Structure}.{expected.Content}");
        _logger.Alert("--------------------------------------------------------------");
    }
}
