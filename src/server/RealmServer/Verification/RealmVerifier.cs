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

using Mangos.Logging;
using RealmServer.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RealmServer.Verification;

internal sealed class RealmVerifier
{
    private readonly IMangosLogger _logger;
    private Timer? _verificationTimer;
    private readonly object _lock = new();
    private bool _isRunning;
    private readonly List<VerificationResult> _recentResults = new();
    private const int MaxRecentResults = 100;
    private HashSet<MessageOpcode> _registeredOpcodes = new();
    private int _dispatcherCount;

    public RealmVerifier(IMangosLogger logger)
    {
        _logger = logger;
    }

    public void Initialize(IEnumerable<IHandlerDispatcher> dispatchers)
    {
        var dispatcherList = dispatchers.ToList();
        _dispatcherCount = dispatcherList.Count;
        _registeredOpcodes = new HashSet<MessageOpcode>(dispatcherList.Select(d => d.Opcode));

        _logger.Information($"[RealmVerifier] Initialized with {_dispatcherCount} dispatchers covering {_registeredOpcodes.Count} opcodes");
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

            _logger.Information($"[RealmVerifier] Started with {intervalSeconds}s interval");
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

            _logger.Information("[RealmVerifier] Stopped");
        }
    }

    public IReadOnlyList<VerificationResult> GetRecentResults()
    {
        lock (_lock)
        {
            return _recentResults.ToList();
        }
    }

    private void RunVerification(object? state)
    {
        try
        {
            var results = new List<VerificationResult>
            {
                VerifyHandlerDispatchers(),
                VerifyOpcodesCoverage(),
                VerifyRuntimeHealth()
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
            _logger.Error(ex, "[RealmVerifier] Verification run failed");
        }
    }

    private VerificationResult VerifyHandlerDispatchers()
    {
        var result = new VerificationResult
        {
            CheckName = "HandlerDispatchers"
        };

        try
        {
            result.Details.Add($"Total registered dispatchers: {_dispatcherCount}");

            var issues = 0;

            if (_dispatcherCount == 0)
            {
                issues++;
                result.Details.Add("No handler dispatchers registered");
            }

            if (_registeredOpcodes.Count != _dispatcherCount)
            {
                issues++;
                result.Details.Add($"Dispatcher count ({_dispatcherCount}) differs from unique opcode count ({_registeredOpcodes.Count}) - possible duplicates");
            }

            foreach (var opcode in _registeredOpcodes)
            {
                result.Details.Add($"Handler registered: {opcode}");
            }

            result.IssuesFound = issues;
            result.Status = issues == 0 ? VerificationStatus.Passed : VerificationStatus.Warning;
            result.Message = issues == 0
                ? $"All {_dispatcherCount} handler dispatchers are valid"
                : $"Found {issues} handler dispatcher issue(s)";
        }
        catch (Exception ex)
        {
            result.Status = VerificationStatus.Error;
            result.Message = $"Handler dispatcher check failed: {ex.Message}";
            result.IssuesFound = 1;
        }

        return result;
    }

    private VerificationResult VerifyOpcodesCoverage()
    {
        var result = new VerificationResult
        {
            CheckName = "OpcodesCoverage"
        };

        try
        {
            var expectedOpcodes = new[]
            {
                MessageOpcode.CMD_AUTH_LOGON_CHALLENGE,
                MessageOpcode.CMD_AUTH_LOGON_PROOF,
                MessageOpcode.CMD_AUTH_RECONNECT_CHALLENGE,
                MessageOpcode.CMD_AUTH_REALMLIST,
            };

            var issues = 0;
            foreach (var expected in expectedOpcodes)
            {
                if (!_registeredOpcodes.Contains(expected))
                {
                    issues++;
                    result.Details.Add($"Missing handler for auth opcode: {expected}");
                }
                else
                {
                    result.Details.Add($"Auth opcode covered: {expected}");
                }
            }

            var allOpcodes = Enum.GetValues<MessageOpcode>();
            var unhandledOpcodes = allOpcodes.Where(o => !_registeredOpcodes.Contains(o)).ToList();
            if (unhandledOpcodes.Any())
            {
                result.Details.Add($"Unhandled opcodes ({unhandledOpcodes.Count}): {string.Join(", ", unhandledOpcodes)}");
            }

            result.IssuesFound = issues;
            result.Status = issues == 0 ? VerificationStatus.Passed : VerificationStatus.Failed;
            result.Message = issues == 0
                ? $"All {expectedOpcodes.Length} critical auth opcodes are handled"
                : $"Missing {issues} critical auth opcode handler(s)";
        }
        catch (Exception ex)
        {
            result.Status = VerificationStatus.Error;
            result.Message = $"Opcodes coverage check failed: {ex.Message}";
            result.IssuesFound = 1;
        }

        return result;
    }

    private VerificationResult VerifyRuntimeHealth()
    {
        var result = new VerificationResult
        {
            CheckName = "RuntimeHealth"
        };

        try
        {
            var issues = 0;

            var memoryBytes = GC.GetTotalMemory(false);
            var memoryMb = memoryBytes / (1024.0 * 1024.0);
            result.Details.Add($"Memory usage: {memoryMb:F1} MB");

            if (memoryMb > 512)
            {
                issues++;
                result.Details.Add("Memory usage exceeds 512 MB threshold");
            }

            var gen0 = GC.CollectionCount(0);
            var gen1 = GC.CollectionCount(1);
            var gen2 = GC.CollectionCount(2);
            result.Details.Add($"GC collections - Gen0: {gen0}, Gen1: {gen1}, Gen2: {gen2}");

            var threadCount = System.Diagnostics.Process.GetCurrentProcess().Threads.Count;
            result.Details.Add($"Active threads: {threadCount}");

            if (threadCount > 100)
            {
                issues++;
                result.Details.Add("Thread count exceeds 100 - possible thread leak");
            }

            result.IssuesFound = issues;
            result.Status = issues == 0 ? VerificationStatus.Passed : VerificationStatus.Warning;
            result.Message = issues == 0
                ? $"Runtime health is good ({memoryMb:F1} MB, {threadCount} threads)"
                : $"Found {issues} runtime health concern(s)";
        }
        catch (Exception ex)
        {
            result.Status = VerificationStatus.Error;
            result.Message = $"Runtime health check failed: {ex.Message}";
            result.IssuesFound = 1;
        }

        return result;
    }

    private void LogResult(VerificationResult result)
    {
        var message = $"[RealmVerifier] {result.CheckName}: {result.Message} ({result.Status})";

        switch (result.Status)
        {
            case VerificationStatus.Passed:
                _logger.Trace(message);
                break;
            case VerificationStatus.Warning:
                _logger.Warning(message);
                break;
            case VerificationStatus.Failed:
            case VerificationStatus.Error:
                _logger.Error(message);
                break;
        }

        var detailLimit = Math.Min(result.Details.Count, 10);
        for (var i = 0; i < detailLimit; i++)
        {
            var detail = $"[RealmVerifier]   - {result.Details[i]}";
            switch (result.Status)
            {
                case VerificationStatus.Passed:
                    _logger.Trace(detail);
                    break;
                case VerificationStatus.Warning:
                    _logger.Warning(detail);
                    break;
                case VerificationStatus.Failed:
                case VerificationStatus.Error:
                    _logger.Error(detail);
                    break;
            }
        }

        if (result.Details.Count > 10)
        {
            _logger.Trace($"[RealmVerifier]   ... and {result.Details.Count - 10} more details");
        }
    }
}
