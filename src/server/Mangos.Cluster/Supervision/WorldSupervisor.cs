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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mangos.Cluster.Interop;
using Mangos.Configuration;
using Mangos.Logging;

namespace Mangos.Cluster.Supervision;

/// <summary>
/// Reconciles the desired world set (from configuration) with the running
/// process set. Spawns missing worlds, polls heartbeats, and respawns
/// after non-clean exits per the world's exit code conventions.
///
/// Cross-platform: process spawning uses System.Diagnostics.Process for
/// both Windows and Linux; graceful shutdown uses TCP/control-channel
/// drain rather than POSIX signals so the same code path works everywhere.
/// External mode skips spawning - the supervisor only tracks state.
/// </summary>
public sealed class WorldSupervisor : IAsyncDisposable
{
    private readonly SupervisorConfiguration _config;
    private readonly IMangosLogger _logger;
    private readonly ConcurrentDictionary<string, SupervisedWorld> _worlds = new();
    private CancellationTokenSource? _cts;
    private Task? _reconcileLoop;

    public WorldSupervisor(SupervisorConfiguration config, IMangosLogger logger)
    {
        _config = config;
        _logger = logger;

        foreach (var entry in config.Worlds)
        {
            _worlds[entry.WorldId] = new SupervisedWorld { Definition = entry };
        }
    }

    public IReadOnlyDictionary<string, SupervisedWorld> Worlds => _worlds;

    /// <summary>Override hook for heartbeat behaviour. Default uses the stored IWorld proxy's GetServerInfo + Ping.</summary>
    public Func<string, Task<ServerInfo?>>? PingWorld
    {
        get; set;
    }

    /// <summary>Override hook for graceful shutdown. Default has no built-in path; PR #4 adds a ControlShutdown envelope.</summary>
    public Func<string, Task>? RequestGracefulShutdown
    {
        get; set;
    }

    /// <summary>Cluster calls this when a world says hello on the IPC channel.</summary>
    public void OnWorldHello(string worldId, IWorld proxy, IReadOnlyList<uint> claimedMaps)
    {
        if (!_worlds.TryGetValue(worldId, out var w))
        {
            // Unmanaged world: still track it so admin commands can see it,
            // but don't auto-restart (no Definition).
            w = new SupervisedWorld
            {
                Definition = new SupervisedWorldEntry { WorldId = worldId, Mode = SupervisorMode.External, Autostart = false, Autorestart = false }
            };
            _worlds[worldId] = w;
        }
        w.Proxy = proxy;
        w.ClaimedMaps = claimedMaps;
        w.State = WorldRunState.Running;
        w.MissedHeartbeats = 0;
        w.LastHeartbeat = DateTime.UtcNow;
        _logger.Information($"World '{worldId}' said hello with {claimedMaps.Count} maps");
    }

    /// <summary>Cluster calls this when a world says goodbye on the IPC channel (or its connection drops).</summary>
    public void OnWorldGoodbye(string worldId)
    {
        if (!_worlds.TryGetValue(worldId, out var w))
            return;
        w.Proxy = null;
        w.ClaimedMaps = Array.Empty<uint>();
        if (w.State == WorldRunState.Running || w.State == WorldRunState.Stale)
            w.State = WorldRunState.Dead;
        _logger.Warning($"World '{worldId}' said goodbye");
    }

    public Task StartAsync()
    {
        if (!_config.Enabled)
        {
            _logger.Information("Supervisor disabled; worlds will not be auto-managed");
            return Task.CompletedTask;
        }

        _cts = new CancellationTokenSource();
        _reconcileLoop = Task.Run(() => ReconcileLoopAsync(_cts.Token));
        _logger.Information($"Supervisor started; managing {_worlds.Count} worlds");
        return Task.CompletedTask;
    }

    /// <summary>Operator action: stop a world. ExplicitStop suppresses auto-respawn.</summary>
    public async Task StopWorldAsync(string worldId)
    {
        if (!_worlds.TryGetValue(worldId, out var w))
            return;
        w.ExplicitStop = true;
        await DrainAndKillAsync(w);
        w.State = WorldRunState.Stopped;
    }

    /// <summary>Operator action: restart a world. Counts as a clean restart so backoff doesn't grow.</summary>
    public async Task RestartWorldAsync(string worldId)
    {
        if (!_worlds.TryGetValue(worldId, out var w))
            return;
        await DrainAndKillAsync(w);
        w.ConsecutiveCrashRestarts = 0;
        w.ExplicitStop = false;
        w.State = WorldRunState.Idle;
    }

    /// <summary>Operator action: start a previously stopped world.</summary>
    public Task StartWorldAsync(string worldId)
    {
        if (_worlds.TryGetValue(worldId, out var w))
        {
            w.ExplicitStop = false;
            if (w.State == WorldRunState.Stopped)
                w.State = WorldRunState.Idle;
        }
        return Task.CompletedTask;
    }

    /// <summary>Updates the cached status from the latest heartbeat reply.</summary>
    public void RecordHeartbeat(string worldId, ServerInfo info)
    {
        if (!_worlds.TryGetValue(worldId, out var w))
            return;
        w.LastHeartbeat = DateTime.UtcNow;
        w.MissedHeartbeats = 0;
        w.LastStatus = info;
        if (w.State is WorldRunState.Starting or WorldRunState.Stale or WorldRunState.Dead)
            w.State = WorldRunState.Running;
    }

    /// <summary>Pick the world with spare capacity that is allowed to host the given map. Used for instance/BG placement.</summary>
    public SupervisedWorld? PickLeastLoaded(uint mapId)
    {
        SupervisedWorld? best = null;
        int bestScore = int.MaxValue;
        foreach (var w in _worlds.Values)
        {
            if (w.State != WorldRunState.Running) continue;
            if (!w.Definition.AllowedMaps.IsDefaultOrEmpty
                && !w.Definition.AllowedMaps.Contains(mapId))
                continue;
            // Heuristic: 4*players + 8*instances + cpu*100; lower is better.
            var s = w.LastStatus;
            int score = (s?.PlayerCount ?? 0) * 4
                      + (s?.InstanceCount ?? 0) * 8
                      + (int)((s?.CpuUsage ?? 0) * 100);
            if (score < bestScore)
            {
                best = w;
                bestScore = score;
            }
        }
        return best;
    }

    private async Task ReconcileLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                foreach (var w in _worlds.Values)
                {
                    await ReconcileOneAsync(w, ct);
                }
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                _logger.Error($"Supervisor reconcile failed: {ex.Message}");
            }

            try
            {
                await Task.Delay(1000, ct);
            }
            catch (OperationCanceledException) { return; }
        }
    }

    private async Task ReconcileOneAsync(SupervisedWorld w, CancellationToken ct)
    {
        // 1. If we own a process, observe its exit.
        if (w.Process is { HasExited: true } exited)
        {
            HandleProcessExit(w, exited.ExitCode);
        }

        // 2. Heartbeat anything we believe is up.
        if (w.IsAlive && (DateTime.UtcNow - w.LastHeartbeat).TotalMilliseconds >= _config.HeartbeatIntervalMs)
        {
            try
            {
                var info = await DoPingAsync(w).WaitAsync(TimeSpan.FromSeconds(5), ct);
                if (info is not null)
                {
                    RecordHeartbeat(w.Definition.WorldId, info);
                }
                else
                {
                    w.MissedHeartbeats++;
                }
            }
            catch
            {
                w.MissedHeartbeats++;
            }

            if (w.MissedHeartbeats >= _config.DeadAfterMissed)
                w.State = WorldRunState.Dead;
            else if (w.MissedHeartbeats >= _config.StaleAfterMissed)
                w.State = WorldRunState.Stale;
        }

        // 3. Respawn dead worlds (with backoff) unless operator-stopped.
        if (w.State == WorldRunState.Dead && !w.ExplicitStop && w.Definition.Autorestart)
        {
            await DrainAndKillAsync(w);
            w.State = WorldRunState.Idle;
            w.ConsecutiveCrashRestarts++;
        }

        // 4. Spawn anything Idle that should be running.
        if (w.State == WorldRunState.Idle && !w.ExplicitStop && w.Definition.Autostart)
        {
            var backoff = Math.Min(
                _config.RespawnBackoffMaxMs,
                _config.RespawnBackoffStepMs * w.ConsecutiveCrashRestarts);
            if ((DateTime.UtcNow - w.LastSpawnAttempt).TotalMilliseconds >= backoff)
            {
                Spawn(w);
            }
        }
    }

    private async Task<ServerInfo?> DoPingAsync(SupervisedWorld w)
    {
        if (PingWorld is not null)
            return await PingWorld(w.Definition.WorldId);

        if (w.Proxy is null)
            return null;

        return await Task.Run(() =>
        {
            try
            {
                var ts = (int)(DateTime.UtcNow - new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                w.Proxy.Ping(ts, 0);
                return w.Proxy.GetServerInfo();
            }
            catch
            {
                return null;
            }
        });
    }

    private void HandleProcessExit(SupervisedWorld w, int exitCode)
    {
        var name = w.Definition.WorldId;
        switch (exitCode)
        {
            case ExitCodes.Clean:
            case ExitCodes.StopRequested:
                _logger.Information($"World '{name}' exited cleanly ({exitCode})");
                w.State = WorldRunState.Stopped;
                w.ExplicitStop = true;
                w.ConsecutiveCrashRestarts = 0;
                break;
            case ExitCodes.RestartRequested:
                _logger.Information($"World '{name}' requested restart");
                w.State = WorldRunState.Idle;
                w.ConsecutiveCrashRestarts = 0;
                break;
            case ExitCodes.ConfigInvalid:
            case ExitCodes.DatabaseVersionMismatch:
                _logger.Error($"World '{name}' exited with non-recoverable code {exitCode}; not respawning");
                w.State = WorldRunState.Stopped;
                w.ExplicitStop = true;
                break;
            default:
                _logger.Warning($"World '{name}' exited with code {exitCode}; will respawn with backoff");
                w.State = WorldRunState.Idle;
                w.ConsecutiveCrashRestarts++;
                break;
        }
        w.Process?.Dispose();
        w.Process = null;
    }

    private void Spawn(SupervisedWorld w)
    {
        w.LastSpawnAttempt = DateTime.UtcNow;
        if (w.Definition.Mode == SupervisorMode.External)
        {
            // External orchestrator owns the process; we just enter Starting and wait for hello.
            _logger.Information($"World '{w.Definition.WorldId}' (external) awaiting hello");
            w.State = WorldRunState.Starting;
            return;
        }

        if (string.IsNullOrWhiteSpace(w.Definition.ExecutablePath))
        {
            _logger.Error($"World '{w.Definition.WorldId}' has no ExecutablePath; cannot spawn");
            return;
        }

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = w.Definition.ExecutablePath,
                WorkingDirectory = w.Definition.WorkingDirectory ?? Path.GetDirectoryName(w.Definition.ExecutablePath) ?? "",
                UseShellExecute = false,
                CreateNoWindow = false,
            };
            foreach (var a in w.Definition.Arguments)
                psi.ArgumentList.Add(a);
            psi.Environment["MANGOS_WORLD_ID"] = w.Definition.WorldId;

            var p = Process.Start(psi);
            if (p is null)
            {
                _logger.Error($"World '{w.Definition.WorldId}' Process.Start returned null");
                return;
            }
            w.Process = p;
            w.State = WorldRunState.Starting;
            _logger.Information($"Spawned world '{w.Definition.WorldId}' (pid {p.Id})");
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to spawn world '{w.Definition.WorldId}': {ex.Message}");
        }
    }

    private async Task DrainAndKillAsync(SupervisedWorld w)
    {
        if (RequestGracefulShutdown is not null && w.IsAlive)
        {
            try
            {
                await RequestGracefulShutdown(w.Definition.WorldId).WaitAsync(TimeSpan.FromSeconds(15));
            }
            catch
            {
                // Swallow; we'll fall through to kill.
            }
        }

        if (w.Process is { HasExited: false } p)
        {
            try
            {
                if (!p.WaitForExit(5000))
                {
                    _logger.Warning($"World '{w.Definition.WorldId}' did not exit gracefully; killing");
                    p.Kill(entireProcessTree: true);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error killing world '{w.Definition.WorldId}': {ex.Message}");
            }
            finally
            {
                p.Dispose();
                w.Process = null;
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        _cts?.Cancel();
        if (_reconcileLoop is not null)
        {
            try { await _reconcileLoop; } catch { }
        }
        foreach (var w in _worlds.Values.Where(x => x.Process is { HasExited: false }))
        {
            try { w.Process!.Kill(entireProcessTree: true); } catch { }
            w.Process?.Dispose();
        }
        _cts?.Dispose();
    }
}
