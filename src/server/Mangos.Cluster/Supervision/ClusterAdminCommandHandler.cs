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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mangos.Cluster.Admin.Commands;

namespace Mangos.Cluster.Supervision;

/// <summary>
/// Local executor for admin commands. Reads/writes the
/// <see cref="WorldSupervisor"/> state and produces a human-readable reply
/// for the operator. Cross-realm routing (TargetRealmId != local) is
/// done by the federation router before this handler sees the command.
/// </summary>
public sealed class ClusterAdminCommandHandler : IAdminCommandHandler
{
    private readonly WorldSupervisor _supervisor;
    private readonly Func<uint> _localRealmIdProvider;

    public ClusterAdminCommandHandler(WorldSupervisor supervisor, Func<uint> localRealmIdProvider)
    {
        _supervisor = supervisor;
        _localRealmIdProvider = localRealmIdProvider;
    }

    public async Task<AdminCommandReply> ExecuteAsync(AdminCommand cmd, CancellationToken ct = default)
    {
        try
        {
            return cmd.Verb switch
            {
                AdminVerb.ServerList => ServerList(),
                AdminVerb.ServerInfo => ServerInfo(cmd),
                AdminVerb.ServerShutdown => await ServerShutdown(cmd),
                AdminVerb.ServerRestart => await ServerRestart(cmd),
                AdminVerb.ServerStart => await ServerStart(cmd),
                AdminVerb.InstanceList => InstanceList(cmd),
                AdminVerb.RealmList => RealmList(),
                _ => Reply(AdminReplyStatus.InvalidArguments, $"unsupported verb: {cmd.Verb}"),
            };
        }
        catch (Exception ex)
        {
            return Reply(AdminReplyStatus.Failed, $"error: {ex.Message}");
        }
    }

    private AdminCommandReply ServerList()
    {
        var r = new AdminCommandReply { Status = AdminReplyStatus.Ok };
        r.Lines.Add($"Worlds ({_supervisor.Worlds.Count}):");
        foreach (var w in _supervisor.Worlds.Values.OrderBy(x => x.Definition.WorldId))
        {
            var s = w.LastStatus;
            var loadDesc = s is null
                ? "no status"
                : $"players={s.PlayerCount} inst={s.InstanceCount} cpu={s.CpuUsage:F1}% mem={s.MemoryUsage}MB";
            r.Lines.Add($"  {w.Definition.WorldId,-24} {w.State,-9} {loadDesc}");
        }
        return r;
    }

    private AdminCommandReply ServerInfo(AdminCommand cmd)
    {
        if (string.IsNullOrEmpty(cmd.WorldId) || !_supervisor.Worlds.TryGetValue(cmd.WorldId, out var w))
            return Reply(AdminReplyStatus.NotFound, $"world '{cmd.WorldId}' not registered");
        var r = new AdminCommandReply { Status = AdminReplyStatus.Ok };
        r.Lines.Add($"World        : {w.Definition.WorldId}");
        r.Lines.Add($"State        : {w.State}");
        r.Lines.Add($"Mode         : {w.Definition.Mode}");
        r.Lines.Add($"Maps claimed : {string.Join(",", w.ClaimedMaps)}");
        r.Lines.Add($"Last beat    : {w.LastHeartbeat:O}");
        r.Lines.Add($"Missed beats : {w.MissedHeartbeats}");
        if (w.LastStatus is { } s)
        {
            r.Lines.Add($"Players      : {s.PlayerCount}");
            r.Lines.Add($"Instances    : {s.InstanceCount}");
            r.Lines.Add($"BGs          : {s.BattlegroundCount}");
            r.Lines.Add($"CPU          : {s.CpuUsage:F1}%");
            r.Lines.Add($"Memory       : {s.MemoryUsage} MB");
            r.Lines.Add($"Uptime       : {TimeSpan.FromMilliseconds(s.UptimeMs)}");
        }
        return r;
    }

    private async Task<AdminCommandReply> ServerShutdown(AdminCommand cmd)
    {
        if (string.IsNullOrEmpty(cmd.WorldId)) return Reply(AdminReplyStatus.InvalidArguments, "--world required");
        await _supervisor.StopWorldAsync(cmd.WorldId);
        return Reply(AdminReplyStatus.Ok, $"world '{cmd.WorldId}' stopped");
    }

    private async Task<AdminCommandReply> ServerRestart(AdminCommand cmd)
    {
        if (string.IsNullOrEmpty(cmd.WorldId)) return Reply(AdminReplyStatus.InvalidArguments, "--world required");
        await _supervisor.RestartWorldAsync(cmd.WorldId);
        return Reply(AdminReplyStatus.Ok, $"world '{cmd.WorldId}' restart triggered");
    }

    private async Task<AdminCommandReply> ServerStart(AdminCommand cmd)
    {
        if (string.IsNullOrEmpty(cmd.WorldId)) return Reply(AdminReplyStatus.InvalidArguments, "--world required");
        await _supervisor.StartWorldAsync(cmd.WorldId);
        return Reply(AdminReplyStatus.Ok, $"world '{cmd.WorldId}' will be (re)started");
    }

    private AdminCommandReply InstanceList(AdminCommand cmd)
    {
        var r = new AdminCommandReply { Status = AdminReplyStatus.Ok };
        var filter = cmd.MapId;
        foreach (var w in _supervisor.Worlds.Values)
        {
            if (filter != 0 && !w.ClaimedMaps.Contains(filter)) continue;
            var s = w.LastStatus;
            r.Lines.Add($"  world={w.Definition.WorldId} maps=[{string.Join(",", w.ClaimedMaps)}] inst={s?.InstanceCount ?? 0}");
        }
        if (r.Lines.Count == 0) r.Lines.Add("(no instances)");
        return r;
    }

    private AdminCommandReply RealmList()
    {
        var r = new AdminCommandReply { Status = AdminReplyStatus.Ok };
        r.Lines.Add($"Local realm id: {_localRealmIdProvider()}");
        return r;
    }

    private static AdminCommandReply Reply(AdminReplyStatus status, string line)
        => new() { Status = status, Lines = { line } };
}
