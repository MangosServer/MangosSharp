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

using System.Linq;
using System.Threading.Tasks;
using Mangos.Cluster.Admin.Protocol;
using Mangos.Cluster.Globals;
using Mangos.Cluster.Handlers;
using Mangos.Configuration;
using Mangos.Logging;

namespace Mangos.Cluster.Federation;

/// <summary>
/// Leader-cluster side of Phase B shard claims. When a peer cluster's
/// player accepts a federated invite (we receive a
/// <see cref="GroupInviteResponseEnvelope"/> with Decision=Accepted),
/// we emit a <see cref="ShardClaimEnvelope"/> back to that peer for the
/// leader's current map. The peer's <see cref="ShardRegistry"/> records
/// the claim so its world's enter-zone hook refuses to host that map
/// for any character in the same group, and the player is told to
/// reconnect to our realm.
/// </summary>
public sealed class FederatedShardClaimer
{
    private readonly ClusterServiceLocator _serviceLocator;
    private readonly FederationRouter _router;
    private readonly IMangosLogger _logger;
    private readonly FederationConfiguration _cfg;

    public FederatedShardClaimer(
        ClusterServiceLocator serviceLocator,
        FederationRouter router,
        MangosConfiguration mangosConfiguration,
        IMangosLogger logger)
    {
        _serviceLocator = serviceLocator;
        _router = router;
        _logger = logger;
        _cfg = mangosConfiguration.Federation ?? new FederationConfiguration();
    }

    public void WireUp()
    {
        _router.OnGroupInviteResponse = HandleResponse;
    }

    private async void HandleResponse(GroupInviteResponseEnvelope env)
    {
        if (env.Decision != GroupInviteResponse.Accepted) return;

        // Find the leader's character locally by groupId.
        var wc = _serviceLocator.WorldCluster;
        wc.CharacteRsLock.EnterReadLock();
        WcHandlerCharacter.CharacterObject? leader = null;
        try
        {
            leader = wc.CharacteRs.Values.FirstOrDefault(c =>
                c.IsInGroup && c.Group != null && c.Group.Id == env.GroupId
                && c.Group.GetLeader() == c);
        }
        finally
        {
            wc.CharacteRsLock.ExitReadLock();
        }

        if (leader is null)
        {
            _logger.Warning($"Federation: invite-accepted but local leader for group {env.GroupId} not found");
            return;
        }

        var claim = new ShardClaimEnvelope
        {
            GroupId = env.GroupId,
            OwnerClusterId = _cfg.LocalClusterId,
            MapId = leader.Map,
            ShardKey = (ulong)env.GroupId,
            // RelayEndpoint is the host cluster's federation listener; the
            // peer uses this to know "where to point the player's client at".
            RelayEndpoint = $"{_cfg.ListenAddress}:{_cfg.ListenPort}",
        };

        try
        {
            var link = await _router.GetOrOpenAsync(env.TargetRealmId);
            if (link is null)
            {
                _logger.Warning($"Federation: cannot send shard claim - peer {env.TargetRealmId} unreachable");
                return;
            }
            await link.SendShardClaimAsync(claim);
            _logger.Information($"Federation: shard claim emitted for group {env.GroupId} on map {leader.Map} -> peer {env.TargetRealmId}");
        }
        catch
        {
            // Best-effort; the peer's player will fall through to the standard
            // "no group" experience until the next attempt.
        }
    }
}
