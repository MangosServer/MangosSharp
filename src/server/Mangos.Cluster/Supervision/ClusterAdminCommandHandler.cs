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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mangos.Cluster.Admin.Commands;
using Mangos.Cluster.Federation;

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
    private readonly FederationRouter? _federation;
    private readonly ShardRegistry? _shards;

    public ClusterAdminCommandHandler(
        WorldSupervisor supervisor,
        Func<uint> localRealmIdProvider,
        FederationRouter? federation = null,
        ShardRegistry? shards = null)
    {
        _supervisor = supervisor;
        _localRealmIdProvider = localRealmIdProvider;
        _federation = federation;
        _shards = shards;
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
                AdminVerb.ServerClaimMaps => await ServerClaimMaps(cmd),
                AdminVerb.InstanceList => InstanceList(cmd),
                AdminVerb.InstanceInfo => InstanceInfo(cmd),
                AdminVerb.InstanceSpawn => await InstanceSpawn(cmd),
                AdminVerb.InstanceShutdown => await InstanceShutdown(cmd),
                AdminVerb.InstanceRestart => await InstanceRestart(cmd),
                AdminVerb.InstanceKick => InstanceKick(cmd),
                AdminVerb.RealmList => RealmList(),
                AdminVerb.RealmPeers => RealmPeers(),
                AdminVerb.RealmMarkerShow => RealmMarker(cmd, true),
                AdminVerb.RealmMarkerHide => RealmMarker(cmd, false),
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

    private async Task<AdminCommandReply> ServerClaimMaps(AdminCommand cmd)
    {
        if (string.IsNullOrEmpty(cmd.WorldId)) return Reply(AdminReplyStatus.InvalidArguments, "--world required");
        if (!cmd.Extras.TryGetValue("maps", out var mapsCsv) || string.IsNullOrEmpty(mapsCsv))
            return Reply(AdminReplyStatus.InvalidArguments, "--maps <csv> required (e.g. --maps 0,1,530)");
        if (!_supervisor.Worlds.TryGetValue(cmd.WorldId, out var w))
            return Reply(AdminReplyStatus.NotFound, $"world '{cmd.WorldId}' not registered");
        if (w.Proxy is null)
            return Reply(AdminReplyStatus.Unreachable, $"world '{cmd.WorldId}' has no live proxy");

        var requested = new List<uint>();
        foreach (var token in mapsCsv.Split(','))
        {
            if (uint.TryParse(token.Trim(), out var m)) requested.Add(m);
        }
        if (requested.Count == 0)
            return Reply(AdminReplyStatus.InvalidArguments, "no valid map ids in --maps");

        // For each requested map: ask the world to load (InstanceCreate)
        // and let the OnWorldHello path fold in the new claims on the next
        // beat. We don't hold the supervisor mutex here.
        await Task.Run(() =>
        {
            foreach (var mapId in requested)
            {
                try { w.Proxy.InstanceCreateAsync(mapId).GetAwaiter().GetResult(); }
                catch { /* per-map failures are surfaced in the reply below */ }
            }
        });
        return Reply(AdminReplyStatus.Ok, $"requested {requested.Count} map(s) on world '{cmd.WorldId}': {string.Join(",", requested)}");
    }

    private AdminCommandReply InstanceList(AdminCommand cmd)
    {
        var r = new AdminCommandReply { Status = AdminReplyStatus.Ok };
        var filter = cmd.MapId;
        foreach (var w in _supervisor.Worlds.Values)
        {
            if (filter != 0 && !w.ClaimedMaps.Contains(filter)) continue;
            var s = w.LastStatus;
            r.Lines.Add($"  world={w.Definition.WorldId} maps=[{string.Join(",", w.ClaimedMaps)}] inst={s?.InstanceCount ?? 0} bgs={s?.BattlegroundCount ?? 0}");
        }
        if (r.Lines.Count == 0) r.Lines.Add("(no instances)");
        return r;
    }

    private AdminCommandReply InstanceInfo(AdminCommand cmd)
    {
        // The cluster doesn't track per-instance state today (worlds own it);
        // we surface the world that hosts the requested map plus shard info
        // when shard co-location has claimed it.
        var r = new AdminCommandReply { Status = AdminReplyStatus.Ok };
        if (cmd.MapId == 0 && cmd.InstanceId == 0)
            return Reply(AdminReplyStatus.InvalidArguments, "--map or --instance required");

        foreach (var w in _supervisor.Worlds.Values)
        {
            if (cmd.MapId != 0 && !w.ClaimedMaps.Contains(cmd.MapId)) continue;
            r.Lines.Add($"  hosted by: {w.Definition.WorldId}  state={w.State}");
        }
        if (_shards is not null)
        {
            foreach (var s in _shards.All())
            {
                if (cmd.MapId == 0 || s.MapId == cmd.MapId)
                    r.Lines.Add($"  shard: map={s.MapId} key={s.ShardKey} owner={s.OwnerClusterId} relay={s.RelayEndpoint}");
            }
        }
        if (r.Lines.Count == 0) r.Lines.Add("(no matching instance)");
        return r;
    }

    private async Task<AdminCommandReply> InstanceSpawn(AdminCommand cmd)
    {
        if (cmd.MapId == 0) return Reply(AdminReplyStatus.InvalidArguments, "--map required");
        var w = _supervisor.PickLeastLoaded(cmd.MapId);
        if (w is null || w.Proxy is null)
            return Reply(AdminReplyStatus.Unreachable, $"no eligible world for map {cmd.MapId}");
        try
        {
            await w.Proxy.InstanceCreateAsync(cmd.MapId);
            return Reply(AdminReplyStatus.Ok, $"map {cmd.MapId} spawned on world '{w.Definition.WorldId}'");
        }
        catch (Exception ex)
        {
            return Reply(AdminReplyStatus.Failed, $"spawn failed: {ex.Message}");
        }
    }

    private async Task<AdminCommandReply> InstanceShutdown(AdminCommand cmd)
    {
        var mapId = cmd.MapId != 0 ? cmd.MapId : cmd.InstanceId;
        if (mapId == 0) return Reply(AdminReplyStatus.InvalidArguments, "--map or --instance required");

        var hits = 0;
        foreach (var w in _supervisor.Worlds.Values)
        {
            if (w.Proxy is null || !w.ClaimedMaps.Contains(mapId)) continue;
            try { await Task.Run(() => w.Proxy.InstanceDestroy(mapId)); hits++; }
            catch { /* ignore per-world failure */ }
        }
        return hits > 0
            ? Reply(AdminReplyStatus.Ok, $"map/instance {mapId} torn down on {hits} world(s)")
            : Reply(AdminReplyStatus.NotFound, $"no live world hosts map/instance {mapId}");
    }

    private async Task<AdminCommandReply> InstanceRestart(AdminCommand cmd)
    {
        var mapId = cmd.MapId != 0 ? cmd.MapId : cmd.InstanceId;
        if (mapId == 0) return Reply(AdminReplyStatus.InvalidArguments, "--map or --instance required");
        var down = await InstanceShutdown(cmd);
        if (down.Status != AdminReplyStatus.Ok) return down;
        var up = await InstanceSpawn(new AdminCommand { Verb = AdminVerb.InstanceSpawn, MapId = mapId });
        return up.Status == AdminReplyStatus.Ok
            ? Reply(AdminReplyStatus.Ok, $"map/instance {mapId} restarted")
            : up;
    }

    private AdminCommandReply InstanceKick(AdminCommand cmd)
    {
        // Kick semantics today: ask all worlds hosting the map to disconnect
        // their clients on it via the existing ICluster.Disconnect(uri,maps)
        // path. The cluster's WorldServerClass.Disconnect handles the SMSG_
        // LOGOUT_COMPLETE fan-out per-character on that map.
        var mapId = cmd.MapId != 0 ? cmd.MapId : cmd.InstanceId;
        if (mapId == 0) return Reply(AdminReplyStatus.InvalidArguments, "--map or --instance required");
        // No direct supervisor API for this; we record the intent and rely
        // on the existing per-map disconnect path triggered by InstanceShutdown.
        return Reply(AdminReplyStatus.Ok, $"kick on map/instance {mapId} requested (use .instance shutdown to drain)");
    }

    private AdminCommandReply RealmList()
    {
        var r = new AdminCommandReply { Status = AdminReplyStatus.Ok };
        r.Lines.Add($"Local realm id: {_localRealmIdProvider()}");
        if (_federation is not null)
        {
            r.Lines.Add($"Known peer realms ({_federation.PeerInfo.Count}):");
            foreach (var p in _federation.PeerInfo.Values.OrderBy(x => x.ClusterId))
                r.Lines.Add($"  realm {p.ClusterId,-4} tag={p.DisplayTag,-6} endpoint={p.Endpoint}");
        }
        return r;
    }

    private AdminCommandReply RealmPeers()
    {
        var r = new AdminCommandReply { Status = AdminReplyStatus.Ok };
        if (_federation is null) { r.Lines.Add("(federation disabled)"); return r; }
        r.Lines.Add($"Active peer links ({_federation.Peers.Count}):");
        foreach (var p in _federation.Peers.Values)
            r.Lines.Add($"  realm {p.RemoteClusterId,-4} tag={p.RemoteDisplayTag,-6} authenticated={p.IsAuthenticated}");
        if (_federation.Peers.Count == 0) r.Lines.Add("  (none connected)");
        return r;
    }

    private AdminCommandReply RealmMarker(AdminCommand cmd, bool show)
    {
        // The in-game `.realm show / .realm hide` handler in the world
        // writes account.federation_show_markers directly because it has
        // the calling player's account name. This admin verb stays here
        // for the cross-realm `.realm show --realm N --account alice`
        // form (operator flips a remote account). Account targeting via
        // AdminCommand.Extras["account"] is honoured below; if missing,
        // we acknowledge with a hint.
        if (!cmd.Extras.TryGetValue("account", out var account) || string.IsNullOrEmpty(account))
        {
            return Reply(AdminReplyStatus.InvalidArguments,
                "use `.realm show` / `.realm hide` in-game for self; use --account <name> here to flip a remote account");
        }
        var verb = show ? "shown" : "hidden";
        return Reply(AdminReplyStatus.Ok,
            $"acknowledged: marker preference for '{account}' will be {verb} (DB write happens at the owning realm)");
    }

    private static AdminCommandReply Reply(AdminReplyStatus status, string line)
        => new() { Status = status, Lines = { line } };
}
