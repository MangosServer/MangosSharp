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

using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Mangos.Cluster.Verification;

public class ClusterVerifier
{
    private readonly ClusterServiceLocator _clusterServiceLocator;
    private Timer _verificationTimer;
    private readonly object _lock = new();
    private bool _isRunning;
    private readonly List<VerificationResult> _recentResults = new();
    private const int MaxRecentResults = 100;

    public ClusterVerifier(ClusterServiceLocator clusterServiceLocator)
    {
        _clusterServiceLocator = clusterServiceLocator;
    }

    public void Start(int intervalSeconds = 60)
    {
        lock (_lock)
        {
            if (_isRunning)
            {
                return;
            }

            _isRunning = true;
            _verificationTimer = new Timer(
                RunVerification,
                null,
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(intervalSeconds));

            _clusterServiceLocator.WorldCluster.Log.WriteLine(
                LogType.INFORMATION,
                "[ClusterVerifier] Started with {0}s interval",
                intervalSeconds);
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            if (!_isRunning)
            {
                return;
            }

            _isRunning = false;
            _verificationTimer?.Dispose();
            _verificationTimer = null;

            _clusterServiceLocator.WorldCluster.Log.WriteLine(
                LogType.INFORMATION,
                "[ClusterVerifier] Stopped");
        }
    }

    public IReadOnlyList<VerificationResult> GetRecentResults()
    {
        lock (_lock)
        {
            return _recentResults.ToList();
        }
    }

    private void RunVerification(object state)
    {
        try
        {
            var results = new List<VerificationResult>
            {
                VerifyClientConnections(),
                VerifyCharacterState(),
                VerifyPacketHandlers(),
                VerifyDatabaseConnections(),
                VerifyWorldServerConnections()
            };

            lock (_lock)
            {
                foreach (var result in results)
                {
                    _recentResults.Add(result);
                    LogResult(result);
                }

                while (_recentResults.Count > MaxRecentResults)
                {
                    _recentResults.RemoveAt(0);
                }
            }
        }
        catch (Exception ex)
        {
            _clusterServiceLocator.WorldCluster.Log.WriteLine(
                LogType.CRITICAL,
                "[ClusterVerifier] Verification run failed: {0}",
                ex.Message);
        }
    }

    private VerificationResult VerifyClientConnections()
    {
        var result = new VerificationResult
        {
            CheckName = "ClientConnections"
        };

        try
        {
            var clients = _clusterServiceLocator.WorldCluster.ClienTs;
            var issues = 0;

            lock (((ICollection)clients).SyncRoot)
            {
                result.Details.Add($"Total connected clients: {clients.Count}");

                foreach (var kvp in clients)
                {
                    if (kvp.Value is null)
                    {
                        issues++;
                        result.Details.Add($"Client ID {kvp.Key}: null client reference");
                        continue;
                    }

                    if (kvp.Value.Socket is null || !kvp.Value.Socket.Connected)
                    {
                        issues++;
                        result.Details.Add($"Client ID {kvp.Key}: disconnected socket still in client list");
                    }

                    if (kvp.Value.Character is not null && !kvp.Value.Character.IsInWorld)
                    {
                        result.Details.Add($"Client ID {kvp.Key}: character exists but not in world (may be loading)");
                    }
                }
            }

            result.IssuesFound = issues;
            result.Status = issues == 0 ? VerificationStatus.Passed : VerificationStatus.Warning;
            result.Message = issues == 0
                ? $"All {clients.Count} client connections are valid"
                : $"Found {issues} client connection issue(s)";
        }
        catch (Exception ex)
        {
            result.Status = VerificationStatus.Error;
            result.Message = $"Client connection check failed: {ex.Message}";
            result.IssuesFound = 1;
        }

        return result;
    }

    private VerificationResult VerifyCharacterState()
    {
        var result = new VerificationResult
        {
            CheckName = "CharacterState"
        };

        try
        {
            var issues = 0;

            _clusterServiceLocator.WorldCluster.CharacteRsLock.EnterReadLock();
            try
            {
                var characters = _clusterServiceLocator.WorldCluster.CharacteRs;
                result.Details.Add($"Total characters in memory: {characters.Count}");

                foreach (var kvp in characters)
                {
                    var character = kvp.Value;
                    if (character is null)
                    {
                        issues++;
                        result.Details.Add($"GUID {kvp.Key}: null character object");
                        continue;
                    }

                    if (character.Guid != kvp.Key)
                    {
                        issues++;
                        result.Details.Add($"GUID mismatch: dictionary key {kvp.Key} != character GUID {character.Guid}");
                    }

                    if (character.Client is null)
                    {
                        issues++;
                        result.Details.Add($"Character '{character.Name}' (GUID {character.Guid}): no client reference");
                    }

                    if (string.IsNullOrEmpty(character.Name))
                    {
                        issues++;
                        result.Details.Add($"Character GUID {character.Guid}: empty name");
                    }

                    if (character.Level < 1 || character.Level > 60)
                    {
                        issues++;
                        result.Details.Add($"Character '{character.Name}': invalid level {character.Level}");
                    }

                    if (character.IsInWorld && character.Map > 999)
                    {
                        issues++;
                        result.Details.Add($"Character '{character.Name}': suspicious map ID {character.Map}");
                    }
                }
            }
            finally
            {
                _clusterServiceLocator.WorldCluster.CharacteRsLock.ExitReadLock();
            }

            result.IssuesFound = issues;
            result.Status = issues == 0 ? VerificationStatus.Passed : VerificationStatus.Warning;
            result.Message = issues == 0
                ? "All character objects are valid"
                : $"Found {issues} character state issue(s)";
        }
        catch (Exception ex)
        {
            result.Status = VerificationStatus.Error;
            result.Message = $"Character state check failed: {ex.Message}";
            result.IssuesFound = 1;
        }

        return result;
    }

    private VerificationResult VerifyPacketHandlers()
    {
        var result = new VerificationResult
        {
            CheckName = "PacketHandlers"
        };

        try
        {
            var handlers = _clusterServiceLocator.WorldCluster.GetPacketHandlers();
            var registeredCount = handlers.Count;

            result.Details.Add($"Registered packet handlers: {registeredCount}");

            var expectedOpcodes = new[]
            {
                Opcodes.CMSG_PING,
                Opcodes.CMSG_AUTH_SESSION,
                Opcodes.CMSG_CHAR_ENUM,
                Opcodes.CMSG_CHAR_CREATE,
                Opcodes.CMSG_CHAR_DELETE,
                Opcodes.CMSG_PLAYER_LOGIN,
                Opcodes.CMSG_PLAYER_LOGOUT,
                Opcodes.CMSG_MESSAGECHAT,
                Opcodes.CMSG_JOIN_CHANNEL,
                Opcodes.CMSG_LEAVE_CHANNEL,
                Opcodes.CMSG_GROUP_INVITE,
                Opcodes.CMSG_GUILD_QUERY,
                Opcodes.CMSG_FRIEND_LIST,
                Opcodes.CMSG_WHO,
                Opcodes.CMSG_NAME_QUERY,
                Opcodes.MSG_MOVE_HEARTBEAT,
                Opcodes.MSG_MOVE_START_FORWARD,
                Opcodes.MSG_MOVE_STOP,
            };

            var issues = 0;
            foreach (var opcode in expectedOpcodes)
            {
                if (!handlers.ContainsKey(opcode))
                {
                    issues++;
                    result.Details.Add($"Missing handler for critical opcode: {opcode}");
                }
            }

            var nullHandlers = handlers.Where(h => h.Value is null).Select(h => h.Key).ToList();
            if (nullHandlers.Any())
            {
                issues += nullHandlers.Count;
                foreach (var opcode in nullHandlers)
                {
                    result.Details.Add($"Null handler delegate for opcode: {opcode}");
                }
            }

            result.IssuesFound = issues;
            result.Status = issues == 0 ? VerificationStatus.Passed :
                            issues <= 3 ? VerificationStatus.Warning : VerificationStatus.Failed;
            result.Message = issues == 0
                ? $"All {registeredCount} packet handlers are properly registered"
                : $"Found {issues} packet handler issue(s) out of {registeredCount} registered";
        }
        catch (Exception ex)
        {
            result.Status = VerificationStatus.Error;
            result.Message = $"Packet handler check failed: {ex.Message}";
            result.IssuesFound = 1;
        }

        return result;
    }

    private VerificationResult VerifyDatabaseConnections()
    {
        var result = new VerificationResult
        {
            CheckName = "DatabaseConnections"
        };

        try
        {
            var issues = 0;

            var accountDb = _clusterServiceLocator.WorldCluster.GetAccountDatabase();
            if (accountDb is null)
            {
                issues++;
                result.Details.Add("Account database: null reference");
            }
            else
            {
                result.Details.Add($"Account database: host={accountDb.SQLHost}, db={accountDb.SQLDBName}");
            }

            var characterDb = _clusterServiceLocator.WorldCluster.GetCharacterDatabase();
            if (characterDb is null)
            {
                issues++;
                result.Details.Add("Character database: null reference");
            }
            else
            {
                result.Details.Add($"Character database: host={characterDb.SQLHost}, db={characterDb.SQLDBName}");
            }

            var worldDb = _clusterServiceLocator.WorldCluster.GetWorldDatabase();
            if (worldDb is null)
            {
                issues++;
                result.Details.Add("World database: null reference");
            }
            else
            {
                result.Details.Add($"World database: host={worldDb.SQLHost}, db={worldDb.SQLDBName}");
            }

            result.IssuesFound = issues;
            result.Status = issues == 0 ? VerificationStatus.Passed : VerificationStatus.Failed;
            result.Message = issues == 0
                ? "All 3 database connections are configured"
                : $"Found {issues} database connection issue(s)";
        }
        catch (Exception ex)
        {
            result.Status = VerificationStatus.Error;
            result.Message = $"Database connection check failed: {ex.Message}";
            result.IssuesFound = 1;
        }

        return result;
    }

    private VerificationResult VerifyWorldServerConnections()
    {
        var result = new VerificationResult
        {
            CheckName = "WorldServerConnections"
        };

        try
        {
            var issues = 0;
            var worldServer = _clusterServiceLocator.WcNetwork.WorldServer;

            if (worldServer is null)
            {
                issues++;
                result.Details.Add("WorldServerClass: null reference");
                result.IssuesFound = 1;
                result.Status = VerificationStatus.Failed;
                result.Message = "WorldServerClass is not initialized";
                return result;
            }

            lock (((ICollection)worldServer.Worlds).SyncRoot)
            {
                var worldCount = worldServer.Worlds.Count;
                result.Details.Add($"Connected world servers: {worldCount}");

                if (worldCount == 0)
                {
                    result.Details.Add("No world servers connected - clients cannot enter world");
                }

                foreach (var kvp in worldServer.Worlds)
                {
                    if (kvp.Value is null)
                    {
                        issues++;
                        result.Details.Add($"Map {kvp.Key}: null world server reference");
                    }
                }
            }

            result.Details.Add($"Stop listen flag: {worldServer.MFlagStopListen}");
            if (worldServer.MFlagStopListen)
            {
                issues++;
                result.Details.Add("Server has stop listen flag set - shutting down");
            }

            result.IssuesFound = issues;
            result.Status = issues == 0 ? VerificationStatus.Passed : VerificationStatus.Warning;
            result.Message = issues == 0
                ? "World server connections are healthy"
                : $"Found {issues} world server connection issue(s)";
        }
        catch (Exception ex)
        {
            result.Status = VerificationStatus.Error;
            result.Message = $"World server connection check failed: {ex.Message}";
            result.IssuesFound = 1;
        }

        return result;
    }

    private void LogResult(VerificationResult result)
    {
        var logType = result.Status switch
        {
            VerificationStatus.Passed => LogType.DEBUG,
            VerificationStatus.Warning => LogType.WARNING,
            VerificationStatus.Failed => LogType.FAILED,
            VerificationStatus.Error => LogType.CRITICAL,
            _ => LogType.INFORMATION,
        };

        _clusterServiceLocator.WorldCluster.Log.WriteLine(
            logType,
            "[ClusterVerifier] {0}: {1} ({2})",
            result.CheckName,
            result.Message,
            result.Status);

        var detailLimit = Math.Min(result.Details.Count, 10);
        for (var i = 0; i < detailLimit; i++)
        {
            _clusterServiceLocator.WorldCluster.Log.WriteLine(
                logType,
                "[ClusterVerifier]   - {0}",
                result.Details[i]);
        }

        if (result.Details.Count > 10)
        {
            _clusterServiceLocator.WorldCluster.Log.WriteLine(
                logType,
                "[ClusterVerifier]   ... and {0} more details",
                result.Details.Count - 10);
        }
    }
}
