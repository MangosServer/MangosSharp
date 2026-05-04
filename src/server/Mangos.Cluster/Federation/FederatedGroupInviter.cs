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

using System.Collections.Concurrent;
using System.Linq;
using Mangos.Cluster.Admin.Protocol;
using Mangos.Cluster.Globals;
using Mangos.Cluster.Handlers;
using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using Mangos.Logging;

namespace Mangos.Cluster.Federation;

/// <summary>
/// Receives <see cref="GroupInviteEnvelope"/> from peer clusters and surfaces
/// the standard SMSG_GROUP_INVITE packet to the targeted local character so
/// they see the invite popup. Replies with a GroupInviteResponseEnvelope so
/// the leader's cluster knows whether the invite was delivered.
///
/// Outbound (this cluster's player invites a Name-RealmTag) is handled in
/// the cluster-side group invite path (WC_Handlers_Group).
/// </summary>
public sealed class FederatedGroupInviter
{
    private readonly ClusterServiceLocator _serviceLocator;
    private readonly FederationRouter _router;
    private readonly IMangosLogger _logger;

    // Pending invites awaiting accept/decline from local players.
    // Keyed by recipient character guid; value carries enough context to
    // emit a GroupInviteResponseEnvelope back to the leader's cluster.
    private readonly ConcurrentDictionary<ulong, PendingInvite> _pending = new();

    public FederatedGroupInviter(
        ClusterServiceLocator serviceLocator,
        FederationRouter router,
        IMangosLogger logger)
    {
        _serviceLocator = serviceLocator;
        _router = router;
        _logger = logger;
    }

    private sealed class PendingInvite
    {
        public required long GroupId { get; init; }
        public required uint LeaderRealmId { get; init; }
        public required string TargetName { get; init; }
    }

    /// <summary>Bind onto the router so inbound peer invites pop the popup locally.</summary>
    public void WireUp()
    {
        _router.OnGroupInvite = HandleInbound;
    }

    private void HandleInbound(GroupInviteEnvelope env)
    {
        var wc = _serviceLocator.WorldCluster;
        wc.CharacteRsLock.EnterReadLock();
        WcHandlerCharacter.CharacterObject? target = null;
        try
        {
            target = wc.CharacteRs.Values.FirstOrDefault(c =>
                _serviceLocator.CommonFunctions.UppercaseFirstLetter(c.Name)
                == _serviceLocator.CommonFunctions.UppercaseFirstLetter(env.TargetName));
        }
        finally
        {
            wc.CharacteRsLock.ExitReadLock();
        }

        if (target is null || target.Client is null)
        {
            // Not online here; tell the leader's cluster.
            ReplyAsync(env, GroupInviteResponse.NotFound, 0, env.TargetName);
            return;
        }

        // Render leader's name with their realm tag prepended so the invitee
        // sees [WM] Bob in the popup. Whisper-style, always prefixed.
        var leaderRendered = string.IsNullOrEmpty(env.LeaderRealmTag)
            ? env.LeaderName
            : $"[{env.LeaderRealmTag}] {env.LeaderName}";

        try
        {
            PacketClass invite = new(Opcodes.SMSG_GROUP_INVITE);
            invite.AddInt8(1);
            invite.AddString(leaderRendered);
            target.Client.Send(invite);
            invite.Dispose();
            _logger.Information($"Federation: delivered group invite from {env.LeaderName}@{env.LeaderRealmId} to {env.TargetName}");
            // Stash so On_CMSG_GROUP_ACCEPT/DECLINE can fire the response.
            _pending[target.Guid] = new PendingInvite
            {
                GroupId = env.GroupId,
                LeaderRealmId = env.LeaderRealmId,
                TargetName = target.Name,
            };
        }
        catch
        {
            ReplyAsync(env, GroupInviteResponse.NotFound, 0, env.TargetName);
        }
    }

    /// <summary>
    /// Called from the cluster's CMSG_GROUP_ACCEPT handler when the local
    /// recipient clicks Accept. Returns true if the invite was federated
    /// (and the response has been queued); false if it was a local invite
    /// and should fall through to the standard path.
    /// </summary>
    public bool TryHandleAccept(ulong recipientGuid)
    {
        if (!_pending.TryRemove(recipientGuid, out var p)) return false;
        ReplyAsync(p.GroupId, p.LeaderRealmId, GroupInviteResponse.Accepted, recipientGuid, p.TargetName);
        return true;
    }

    /// <summary>Counterpart to TryHandleAccept for declines.</summary>
    public bool TryHandleDecline(ulong recipientGuid)
    {
        if (!_pending.TryRemove(recipientGuid, out var p)) return false;
        ReplyAsync(p.GroupId, p.LeaderRealmId, GroupInviteResponse.Declined, recipientGuid, p.TargetName);
        return true;
    }

    private void ReplyAsync(GroupInviteEnvelope env, GroupInviteResponse decision, ulong targetGuid, string targetName)
        => ReplyAsync(env.GroupId, env.LeaderRealmId, decision, targetGuid, targetName);

    private async void ReplyAsync(long groupId, uint leaderRealmId, GroupInviteResponse decision, ulong targetGuid, string targetName)
    {
        try
        {
            var link = await _router.GetOrOpenAsync(leaderRealmId);
            if (link is null) return;
            await link.SendGroupInviteResponseAsync(new GroupInviteResponseEnvelope
            {
                GroupId = groupId,
                TargetRealmId = 0,
                TargetGuid = targetGuid,
                TargetName = targetName,
                Decision = decision,
            });
        }
        catch
        {
            // Best effort.
        }
    }

}
