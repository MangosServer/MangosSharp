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

using Autofac;
using Mangos.Common.Enums.Global;
using Mangos.Logging;
using Xunit;

namespace Mangos.Tests.Logging;

public class LoggingTests
{
    [Fact]
    public void When_AllLogTypesAreUsed_Then_OutputIsGenerated()
    {
        // Create and configure the container using Autofac
        var builder = new ContainerBuilder();
        builder.RegisterModule<LoggingModule>();
        var container = builder.Build();

        // Resolve the logger from the container
        using (var scope = container.BeginLifetimeScope())
        {
            var logger = scope.Resolve<IMangosLogger>();

            // Generate randomized output for each log type
            foreach (var logType in Enum.GetValues<LogType>())
            {
                GenerateRandomMessage(logger, logType);
            }
        }

        container.Dispose();
    }

    private static void GenerateRandomMessage(IMangosLogger logger, LogType logType)
    {
        var random = new Random();
        switch (logType)
        {
            case LogType.NETWORK:
                GenerateNetworkMessage(logger, random);
                break;
            case LogType.DEBUG:
                GenerateDebugMessage(logger, random);
                break;
            case LogType.INFORMATION:
                GenerateInformationMessage(logger, random);
                break;
            case LogType.USER:
                GenerateUserMessage(logger, random);
                break;
            case LogType.SUCCESS:
                GenerateSuccessMessage(logger, random);
                break;
            case LogType.WARNING:
                GenerateWarningMessage(logger, random);
                break;
            case LogType.FAILED:
                GenerateFailedMessage(logger, random);
                break;
            case LogType.CRITICAL:
                GenerateCriticalMessage(logger, random);
                break;
            case LogType.DATABASE:
                GenerateDatabaseMessage(logger, random);
                break;
            case LogType.ALERT:
                GenerateAlertMessage(logger, random);
                break;
            case LogType.EMERG:
                GenerateEmergMessage(logger, random);
                break;
            case LogType.FUNC:
                GenerateFuncMessage(logger, random);
                break;
            case LogType.NOTICE:
                GenerateNoticeMessage(logger, random);
                break;
            case LogType.THREAD:
                GenerateThreadMessage(logger, random);
                break;
            case LogType.TRACE:
                GenerateTraceMessage(logger, random);
                break;
            default:
                break;
        }
    }

    private static void GenerateNetworkMessage(IMangosLogger logger, Random random)
    {
        var opcodes = new[] { "0x123", "0x456", "0x789", "0xABC", "0xDEF" };
        // Codacy warning suppressed: These are test-only IP addresses for generating example log messages
        #pragma warning disable S1313 // "IP addresses should not be hardcoded"
        var ips = new[] { "192.168.1.1", "10.0.0.1", "172.16.0.1", "127.0.0.1" };
        #pragma warning restore S1313
        var actions = new[] { "Received", "Sent", "Processing" };

        logger.Network($"{actions[random.Next(actions.Length)]} packet {opcodes[random.Next(opcodes.Length)]} from client {ips[random.Next(ips.Length)]}");
    }

    private static void GenerateDebugMessage(IMangosLogger logger, Random random)
    {
        var opcodes = new[] { "0x123", "0x456", "0x789", "0xABC", "0xDEF" };
        var lengths = new[] { 64, 128, 256, 512, 1024 };
        var operations = new[] { "Processing", "Parsing", "Validating", "Encoding" };

        logger.Debug($"{operations[random.Next(operations.Length)]} opcode {opcodes[random.Next(opcodes.Length)]} with data length {lengths[random.Next(lengths.Length)]}");
    }

    private static void GenerateInformationMessage(IMangosLogger logger, Random random)
    {
        var players = new[] { "TestUser", "Player123", "Gamer456", "WoWPlayer", "MangosUser" };
        var actions = new[] { "logged in", "logged out", "joined channel", "left channel", "completed quest" };
        var locations = new[] { "Stormwind", "Orgrimmar", "Ironforge", "Darnassus", "Thunder Bluff" };

        logger.Information($"Player '{players[random.Next(players.Length)]}' {actions[random.Next(actions.Length)]} from {locations[random.Next(locations.Length)]}");
    }

    private static void GenerateUserMessage(IMangosLogger logger, Random random)
    {
        var players = new[] { "TestUser", "Player123", "Gamer456", "WoWPlayer", "MangosUser" };
        var actions = new[] { "moved to", "attacked", "cast spell", "used item", "traded with" };
        var targets = new[] { "a wolf", "another player", "a quest NPC", "a vendor", "a guard" };

        logger.User($"Player '{players[random.Next(players.Length)]}' {actions[random.Next(actions.Length)]} {targets[random.Next(targets.Length)]}");
    }

    private static void GenerateSuccessMessage(IMangosLogger logger, Random random)
    {
        var operations = new[] { "Database connection", "World loading", "Character save", "Guild creation", "Auction posting" };
        var results = new[] { "established", "completed", "saved", "created", "posted" };

        logger.Success($"{operations[random.Next(operations.Length)]} {results[random.Next(results.Length)]} successfully");
    }

    private static void GenerateWarningMessage(IMangosLogger logger, Random random)
    {
        var issues = new[] { "High memory usage", "Slow query performance", "Network latency", "Disk space low", "Connection timeout" };
        var values = new[] { "85%", "2.5s", "150ms", "10GB remaining", "30s" };

        logger.Warning($"{issues[random.Next(issues.Length)]} detected: {values[random.Next(values.Length)]}");
    }

    private static void GenerateFailedMessage(IMangosLogger logger, Random random)
    {
        var operations = new[] { "Character loading", "Spell casting", "Item creation", "Quest acceptance", "Trade completion" };
        var reasons = new[] { "invalid data", "permission denied", "resource unavailable", "timeout", "validation error" };

        logger.Failed($"{operations[random.Next(operations.Length)]} failed: {reasons[random.Next(reasons.Length)]}");
    }

    private static void GenerateCriticalMessage(IMangosLogger logger, Random random)
    {
        var systems = new[] { "Database", "Network", "Memory", "File system", "Authentication" };
        var errors = new[] { "connection lost", "out of memory", "disk full", "corruption detected", "security breach" };

        logger.Critical($"{systems[random.Next(systems.Length)]} critical error: {errors[random.Next(errors.Length)]}");
    }

    private static void GenerateDatabaseMessage(IMangosLogger logger, Random random)
    {
        var operations = new[] { "SELECT", "INSERT", "UPDATE", "DELETE", "JOIN" };
        var tables = new[] { "characters", "items", "quests", "guilds", "auctions" };
        var issues = new[] { "query timeout", "deadlock detected", "constraint violation", "index corruption", "connection failed" };

        logger.Database($"{operations[random.Next(operations.Length)]} on table '{tables[random.Next(tables.Length)]}' failed: {issues[random.Next(issues.Length)]}");
    }

    private static void GenerateAlertMessage(IMangosLogger logger, Random random)
    {
        var alerts = new[] { "High CPU usage", "Memory leak detected", "Disk space critical", "Network outage", "Security threat" };
        var actions = new[] { "immediate action required", "system monitoring activated", "alert sent to administrators", "automatic recovery initiated" };

        logger.Alert($"{alerts[random.Next(alerts.Length)]}: {actions[random.Next(actions.Length)]}");
    }

    private static void GenerateEmergMessage(IMangosLogger logger, Random random)
    {
        var emergencies = new[] { "System crash imminent", "Data corruption detected", "Hardware failure", "Critical service down", "Emergency shutdown" };
        var impacts = new[] { "all services affected", "data loss possible", "immediate shutdown required", "manual intervention needed" };

        logger.Emerg($"{emergencies[random.Next(emergencies.Length)]}: {impacts[random.Next(impacts.Length)]}");
    }

    private static void GenerateFuncMessage(IMangosLogger logger, Random random)
    {
        var functions = new[] { "ProcessPacket", "ValidateInput", "CalculateDamage", "UpdateStats", "HandleEvent" };
        var steps = new[] { "entry", "processing", "validation", "execution", "exit" };

        logger.Func($"Function '{functions[random.Next(functions.Length)]}' - {steps[random.Next(steps.Length)]} step");
    }

    private static void GenerateNoticeMessage(IMangosLogger logger, Random random)
    {
        var notices = new[] { "Server restart scheduled", "Maintenance window", "Configuration updated", "New feature deployed", "Performance optimization" };
        var details = new[] { "in 30 minutes", "completed successfully", "requires restart", "monitoring active", "no impact expected" };

        logger.Notice($"{notices[random.Next(notices.Length)]}: {details[random.Next(details.Length)]}");
    }

    private static void GenerateThreadMessage(IMangosLogger logger, Random random)
    {
        var threads = new[] { "WorkerThread-1", "NetworkThread-2", "DatabaseThread-3", "MainThread", "BackgroundThread-4" };
        var states = new[] { "started", "processing", "waiting", "completed", "terminated" };

        logger.Thread($"Thread '{threads[random.Next(threads.Length)]}' {states[random.Next(states.Length)]}");
    }

    private static void GenerateTraceMessage(IMangosLogger logger, Random random)
    {
        var components = new[] { "PacketHandler", "DatabaseManager", "NetworkManager", "GameLogic", "EventSystem" };
        var traces = new[] { "method call", "variable assignment", "loop iteration", "condition check", "return value" };

        logger.Trace($"{components[random.Next(components.Length)]}: {traces[random.Next(traces.Length)]} at line {random.Next(100, 1000)}");
    }
}
