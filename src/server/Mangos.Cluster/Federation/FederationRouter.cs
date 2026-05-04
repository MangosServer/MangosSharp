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
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Mangos.Cluster.Admin.Auth;
using Mangos.Cluster.Admin.Commands;
using Mangos.Cluster.Admin.Protocol;
using Mangos.Configuration;
using Mangos.Logging;

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

    public FederationRouter(
        FederationConfiguration cfg,
        IMangosLogger logger,
        Func<uint, string?> resolveEndpoint)
    {
        _cfg = cfg;
        _logger = logger;
        _resolveEndpoint = resolveEndpoint;
    }

    /// <summary>Optional callbacks on inbound envelopes; bound by gameplay code.</summary>
    public Action<ChatEnvelope>? OnChat { get; set; }
    public Action<GroupInviteEnvelope>? OnGroupInvite { get; set; }
    public Action<GroupInviteResponseEnvelope>? OnGroupInviteResponse { get; set; }
    public Action<GroupRosterUpdateEnvelope>? OnGroupRosterUpdate { get; set; }
    public Func<PresenceQueryEnvelope, PresenceReplyEnvelope>? OnPresenceQuery { get; set; }

    /// <summary>Bind the outbound side's hooks onto a newly opened or accepted link.</summary>
    public void BindHandlers(FederationLink link)
    {
        link.OnChatRoute = e => OnChat?.Invoke(e);
        link.OnGroupInvite = e => OnGroupInvite?.Invoke(e);
        link.OnGroupInviteResponse = e => OnGroupInviteResponse?.Invoke(e);
        link.OnGroupRosterUpdate = e => OnGroupRosterUpdate?.Invoke(e);
        link.OnPresenceQuery = e => OnPresenceQuery?.Invoke(e) ?? new PresenceReplyEnvelope { Name = e.Name, Online = false };
    }

    /// <summary>
    /// Get or open a federation link to the cluster that owns realm id N.
    /// Returns null if the peer is unreachable or no secret is configured.
    /// </summary>
    public async Task<FederationLink?> GetOrOpenAsync(uint realmId)
    {
        if (_outbound.TryGetValue(realmId, out var existing) && existing.IsAuthenticated)
            return existing;

        var endpoint = _resolveEndpoint(realmId);
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
        foreach (var l in _outbound.Values)
            try { l.Dispose(); } catch { }
        _outbound.Clear();
    }
}
