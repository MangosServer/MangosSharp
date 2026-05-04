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
using Mangos.Cluster.Admin.Protocol;
using Mangos.Cluster.Globals;
using Mangos.Cluster.Handlers;
using Mangos.Common.Enums.Chat;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Misc;
using Mangos.Configuration;
using Mangos.Logging;

namespace Mangos.Cluster.Federation;

/// <summary>
/// Receives <see cref="ChatEnvelope"/> from peer clusters and delivers
/// the message to the local recipient as a standard SMSG_MESSAGECHAT.
///
/// The sender's display tag is rendered into the visible name per
/// <see cref="FederationMarkerMode"/>: whispers always carry the marker
/// (so /reply works); other channels respect the receiving account's
/// federation_show_markers preference.
/// </summary>
public sealed class FederatedChatDeliverer
{
    private readonly ClusterServiceLocator _serviceLocator;
    private readonly FederationRouter _router;
    private readonly FederationConfiguration _cfg;
    private readonly IMangosLogger _logger;

    public FederatedChatDeliverer(
        ClusterServiceLocator serviceLocator,
        FederationRouter router,
        MangosConfiguration mangosConfiguration,
        IMangosLogger logger)
    {
        _serviceLocator = serviceLocator;
        _router = router;
        _cfg = mangosConfiguration.Federation ?? new FederationConfiguration();
        _logger = logger;
    }

    public void WireUp()
    {
        _router.OnChat = HandleInbound;
    }

    private void HandleInbound(ChatEnvelope env)
    {
        switch (env.Channel)
        {
            case ChatChannel.Whisper:
                DeliverWhisper(env);
                break;
            case ChatChannel.Party:
            case ChatChannel.Raid:
                DeliverGroup(env);
                break;
            case ChatChannel.Guild:
            case ChatChannel.GuildOfficer:
                // Guilds are not yet federated (intentional - cross-realm
                // guilds are a separate scope). Drop with a log.
                _logger.Information($"Federation: dropped guild chat from {env.SenderName}@{env.SenderRealmId} (guilds not federated)");
                break;
            case ChatChannel.System:
                Broadcast(env);
                break;
            case ChatChannel.NamedChannel:
                _logger.Information($"Federation: dropped named-channel chat from {env.SenderName}@{env.SenderRealmId} (named channels not federated)");
                break;
        }
    }

    private void DeliverWhisper(ChatEnvelope env)
    {
        if (string.IsNullOrEmpty(env.RecipientName)) return;
        var target = LookupByName(env.RecipientName);
        if (target?.Client is null)
        {
            _logger.Information($"Federation: whisper for unknown local recipient '{env.RecipientName}'");
            return;
        }
        DeliverTo(target, env, isWhisper: true);
    }

    private void DeliverGroup(ChatEnvelope env)
    {
        if (env.GroupId == 0) return;
        // Find local members of this group; deliver one envelope to each.
        var wc = _serviceLocator.WorldCluster;
        wc.CharacteRsLock.EnterReadLock();
        WcHandlerCharacter.CharacterObject[] members;
        try
        {
            members = wc.CharacteRs.Values
                .Where(c => c.IsInGroup && c.Group != null && c.Group.Id == env.GroupId)
                .ToArray();
        }
        finally
        {
            wc.CharacteRsLock.ExitReadLock();
        }
        foreach (var m in members)
            DeliverTo(m, env, isWhisper: false);
    }

    private void Broadcast(ChatEnvelope env)
    {
        var wc = _serviceLocator.WorldCluster;
        wc.CharacteRsLock.EnterReadLock();
        WcHandlerCharacter.CharacterObject[] all;
        try
        {
            all = wc.CharacteRs.Values.Where(c => c.Client is not null).ToArray();
        }
        finally
        {
            wc.CharacteRsLock.ExitReadLock();
        }
        foreach (var c in all)
            DeliverTo(c, env, isWhisper: false);
    }

    private void DeliverTo(WcHandlerCharacter.CharacterObject target, ChatEnvelope env, bool isWhisper)
    {
        var senderName = RealmMarkers.Decorate(
            env.SenderName,
            env.SenderRealmTag,
            _cfg.MarkerMode,
            accountWantsMarkers: true, // per-account flag is read in the world; cluster defaults to "yes"
            isWhisper: isWhisper,
            placement: ParsePlacement(env));

        // Use the synthetic guid 0 + a name-bearing channel so the WoW
        // client renders the sender as "[WM] Bob" rather than as a local
        // GUID (which we don't have for foreign players).
        var msgType = MapChannel(env.Channel, isWhisper);
        var packet = _serviceLocator.Functions.BuildChatMessage(
            senderGuid: 0,
            message: env.Body,
            msgType: msgType,
            msgLanguage: (LANGUAGES)env.Language,
            flag: 0,
            msgChannel: senderName);
        try { target.Client?.Send(packet); }
        finally { packet.Dispose(); }
    }

    private static MarkerPlacement ParsePlacement(ChatEnvelope env)
        // Sender-side placement is informational; receiver decides locally.
        // Default to prefix until per-realm marker placement is plumbed in.
        => MarkerPlacement.Prefix;

    private static ChatMsg MapChannel(ChatChannel ch, bool isWhisper) => ch switch
    {
        ChatChannel.Whisper => isWhisper ? ChatMsg.CHAT_MSG_WHISPER : ChatMsg.CHAT_MSG_SYSTEM,
        ChatChannel.Party => ChatMsg.CHAT_MSG_PARTY,
        ChatChannel.Raid => ChatMsg.CHAT_MSG_RAID,
        ChatChannel.Guild => ChatMsg.CHAT_MSG_GUILD,
        ChatChannel.GuildOfficer => ChatMsg.CHAT_MSG_OFFICER,
        ChatChannel.System => ChatMsg.CHAT_MSG_SYSTEM,
        _ => ChatMsg.CHAT_MSG_SYSTEM,
    };

    private WcHandlerCharacter.CharacterObject? LookupByName(string name)
    {
        var wc = _serviceLocator.WorldCluster;
        wc.CharacteRsLock.EnterReadLock();
        try
        {
            return wc.CharacteRs.Values.FirstOrDefault(c =>
                _serviceLocator.CommonFunctions.UppercaseFirstLetter(c.Name)
                == _serviceLocator.CommonFunctions.UppercaseFirstLetter(name));
        }
        finally
        {
            wc.CharacteRsLock.ExitReadLock();
        }
    }
}
