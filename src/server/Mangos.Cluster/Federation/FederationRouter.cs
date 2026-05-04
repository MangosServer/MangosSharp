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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Mangos.Cluster.Admin.Auth;
using Mangos.Cluster.Admin.Commands;
using Mangos.Cluster.Admin.Protocol;
using Mangos.Configuration;
using Mangos.Logging;
using Mangos.MySql.GetFederationPeers;

namespace Mangos.Cluster.Federation;

/// <summary>
/// Cluster-side outbound side of the federation transport: maintains
/// dial-out connections to peer clusters by realm id, multiplexes admin
/// / chat / group / presence envelopes over each, and lazily reconnects
/// after drops.
///
/// Peer endpoints come from the realmlist DB column added in PR #4
/// (clusterAdminEndpoint); the per-peer secret comes from the
/// FederationConfiguration.Peers list. Both lookups are injected so this
/// class doesn't grow MySql dependencies.
/// </summary>
public sealed class FederationRouter : IDisposable
{
    private readonly FederationConfiguration _cfg;
    private readonly IMangosLogger _logger;
    private readonly Func<uint, string?> _resolveEndpoint;
    private readonly ConcurrentDictionary<uint, FederationLink> _outbound = new();
    private readonly ConcurrentDictionary<uint, FederationPeerInfo> _peerInfo = new();
    private readonly IGetFederationPeersQuery? _peersQuery;
    private CancellationTokenSource? _refreshCts;
    private Task? _refreshLoop;

    public FederationRouter(
        FederationConfiguration cfg,
        IMangosLogger logger,
        Func<uint, string?> resolveEndpoint,
        IGetFederationPeersQuery? peersQuery = null)
    {
        _cfg = cfg;
        _logger = logger;
        _resolveEndpoint = resolveEndpoint;
        _peersQuery = peersQuery;
    }

    /// <summary>Information about a known peer cluster, populated from the realmlist DB.</summary>
    public sealed class FederationPeerInfo
    {
        public required uint ClusterId
        {
            get; init;
        }
        public required string Endpoint
        {
            get; init;
        }
        public required string DisplayTag
        {
            get; init;
        }
        public required string MarkerPosition
        {
            get; init;
        }
    }

    /// <summary>Snapshot of all peers discovered from the realmlist table.</summary>
    public IReadOnlyDictionary<uint, FederationPeerInfo> PeerInfo => _peerInfo;

    /// <summary>Active outbound links keyed by remote cluster id.</summary>
    public IReadOnlyDictionary<uint, FederationLink> Peers => _outbound;

    /// <summary>Optional callbacks on inbound envelopes; bound by gameplay code.</summary>
    public Action<ChatEnvelope>? OnChat
    {
        get; set;
    }
    public Action<GroupInviteEnvelope>? OnGroupInvite
    {
        get; set;
    }
    public Action<GroupInviteResponseEnvelope>? OnGroupInviteResponse
    {
        get; set;
    }
    public Action<GroupRosterUpdateEnvelope>? OnGroupRosterUpdate
    {
        get; set;
    }
    public Func<PresenceQueryEnvelope, PresenceReplyEnvelope>? OnPresenceQuery
    {
        get; set;
    }
    public Action<ShardClaimEnvelope>? OnShardClaim
    {
        get; set;
    }
    public Action<ShardReleaseEnvelope>? OnShardRelease
    {
        get; set;
    }

    /// <summary>Bind the outbound side's hooks onto a newly opened or accepted link.</summary>
    public void BindHandlers(FederationLink link)
    {
        link.OnChatRoute = e => OnChat?.Invoke(e);
        link.OnGroupInvite = e => OnGroupInvite?.Invoke(e);
        link.OnGroupInviteResponse = e => OnGroupInviteResponse?.Invoke(e);
        link.OnGroupRosterUpdate = e => OnGroupRosterUpdate?.Invoke(e);
        link.OnPresenceQuery = e => OnPresenceQuery?.Invoke(e) ?? new PresenceReplyEnvelope { Name = e.Name, Online = false };
        link.OnShardClaim = e => OnShardClaim?.Invoke(e);
        link.OnShardRelease = e => OnShardRelease?.Invoke(e);
    }

    /// <summary>
    /// Start the periodic peer-table refresh from the realmlist DB plus the
    /// auto-dial / heartbeat maintenance loop. Safe to call once at cluster
    /// startup. The refresh side is a no-op when no DB query is bound.
    /// </summary>
    public Task StartAsync()
    {
        _refreshCts = new CancellationTokenSource();
        if (_peersQuery is not null)
            _refreshLoop = Task.Run(() => RefreshLoopAsync(_refreshCts.Token));
        _ = Task.Run(() => MaintainLinksAsync(_refreshCts.Token));
        return Task.CompletedTask;
    }

    private async Task MaintainLinksAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            // For each known peer, ensure we have an outbound link and
            // heartbeat the live ones. Failures here drop the link so the
            // next iteration redials.
            foreach (var info in _peerInfo.Values)
            {
                if (ct.IsCancellationRequested) return;
                if (info.ClusterId == _cfg.LocalClusterId) continue;
                try
                {
                    var link = await GetOrOpenAsync(info.ClusterId);
                    if (link is null) continue;
                    await link.HeartbeatAsync().WaitAsync(TimeSpan.FromSeconds(10), ct);
                }
                catch
                {
                    if (_outbound.TryRemove(info.ClusterId, out var bad))
                    {
                        try { bad.Dispose(); } catch { }
                        _logger.Warning($"Federation: heartbeat to peer {info.ClusterId} failed; link dropped");
                    }
                }
            }
            try { await Task.Delay(TimeSpan.FromSeconds(15), ct); }
            catch (OperationCanceledException) { return; }
        }
    }

    private async Task RefreshLoopAsync(CancellationToken ct)
    {
        // First load is eager so the first GetOrOpenAsync after startup
        // sees populated peer info.
        await RefreshPeersOnceAsync();
        while (!ct.IsCancellationRequested)
        {
            try { await Task.Delay(TimeSpan.FromMinutes(1), ct); }
            catch (OperationCanceledException) { return; }
            await RefreshPeersOnceAsync();
        }
    }

    private async Task RefreshPeersOnceAsync()
    {
        try
        {
            var rows = await _peersQuery!.ExecuteAsync();
            if (rows is null) return;
            var fresh = new Dictionary<uint, FederationPeerInfo>();
            foreach (var r in rows)
            {
                if (r.clusterId == 0 || string.IsNullOrEmpty(r.clusterAdminEndpoint)) continue;
                fresh[r.clusterId] = new FederationPeerInfo
                {
                    ClusterId = r.clusterId,
                    Endpoint = r.clusterAdminEndpoint,
                    DisplayTag = r.displayTag ?? string.Empty,
                    MarkerPosition = r.markerPosition ?? "prefix",
                };
            }
            // Replace wholesale.
            foreach (var k in _peerInfo.Keys.Where(k => !fresh.ContainsKey(k)).ToList())
                _peerInfo.TryRemove(k, out _);
            foreach (var kv in fresh)
                _peerInfo[kv.Key] = kv.Value;
        }
        catch (Exception ex)
        {
            _logger.Warning($"Federation: realmlist peer refresh failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Get or open a federation link to the cluster that owns realm id N.
    /// Returns null if the peer is unreachable or no secret is configured.
    /// Endpoint is resolved (in order) from: cached realmlist row, then the
    /// caller-supplied <c>resolveEndpoint</c> lambda for tests/overrides.
    /// </summary>
    public async Task<FederationLink?> GetOrOpenAsync(uint realmId)
    {
        if (_outbound.TryGetValue(realmId, out var existing) && existing.IsAuthenticated)
            return existing;

        string? endpoint = null;
        if (_peerInfo.TryGetValue(realmId, out var info))
            endpoint = info.Endpoint;
        endpoint ??= _resolveEndpoint(realmId);
        if (string.IsNullOrEmpty(endpoint))
            return null;

        // Find the peer secret. We key peers by *peer's* clusterId, not realmId,
        // but for a single-realm-per-cluster setup these are equivalent. The
        // realmlist row carries clusterId so callers should pass that here.
        byte[]? secret = null;
        foreach (var p in _cfg.Peers)
        {
            if (p.ClusterId == realmId)
            {
                secret = PeerAuth.SecretFromString(p.Secret);
                break;
            }
        }
        if (secret is null)
        {
            _logger.Warning($"Federation: no secret configured for cluster id {realmId}; refusing dial");
            return null;
        }

        var parts = endpoint.Split(':');
        if (parts.Length != 2 || !int.TryParse(parts[1], out var port))
        {
            _logger.Warning($"Federation: bad endpoint '{endpoint}' for cluster id {realmId}");
            return null;
        }

        try
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
            await socket.ConnectAsync(new IPEndPoint(IPAddress.Parse(parts[0]), port));
            var link = new FederationLink(socket);
            BindHandlers(link);
            await link.ConnectAsAsync(_cfg.LocalClusterId, _cfg.LocalDisplayTag, secret);
            _outbound[realmId] = link;
            link.Disconnected += () => _outbound.TryRemove(realmId, out _);
            _logger.Information($"Federation: dialed peer cluster {realmId} at {endpoint}");
            return link;
        }
        catch (Exception ex)
        {
            _logger.Warning($"Federation: dial to cluster {realmId} ({endpoint}) failed: {ex.Message}");
            return null;
        }
    }

    public void Dispose()
    {
        _refreshCts?.Cancel();
        try { _refreshLoop?.Wait(TimeSpan.FromSeconds(2)); } catch { }
        _refreshCts?.Dispose();
        foreach (var l in _outbound.Values)
            try { l.Dispose(); } catch { }
        _outbound.Clear();
    }
}
