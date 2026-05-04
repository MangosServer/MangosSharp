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
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Mangos.Cluster.Admin.Auth;
using Mangos.Cluster.Admin.Commands;
using Mangos.Cluster.Interop.Protocol;

namespace Mangos.Cluster.Admin.Protocol;

/// <summary>
/// Authenticated cluster &lt;-&gt; cluster connection. Wraps an
/// <see cref="InteropConnection"/> and reuses its framing; the only
/// difference from the world IPC is that the first frame must be a
/// signed PeerHello and the method-id range is 0x0300+.
///
/// One instance per peer pair, opened by the dialer and accepted by
/// the listener.
/// </summary>
public sealed class FederationLink : IDisposable
{
    private readonly InteropConnection _connection;

    /// <summary>Remote cluster id, populated after a successful handshake.</summary>
    public uint RemoteClusterId { get; private set; }

    /// <summary>Remote display tag (e.g. "WM"), populated after a successful handshake.</summary>
    public string RemoteDisplayTag { get; private set; } = string.Empty;

    /// <summary>True iff the handshake has completed.</summary>
    public bool IsAuthenticated { get; private set; }

    /// <summary>Bound by the cluster: handles inbound admin commands.</summary>
    public IAdminCommandHandler? AdminHandler { get; set; }

    /// <summary>
    /// Bound by FederationServer on accept. Receives the incoming PeerHello,
    /// returns either the accepted (clusterId, displayTag) or null on reject.
    /// </summary>
    public Func<PeerHello, (uint, string)?>? OnPeerHello { get; set; }

    /// <summary>Inbound cross-realm chat message (PR #6).</summary>
    public Action<ChatEnvelope>? OnChatRoute { get; set; }

    /// <summary>Inbound group invite from a peer cluster (PR #6).</summary>
    public Action<GroupInviteEnvelope>? OnGroupInvite { get; set; }

    /// <summary>Inbound group invite response from a peer cluster (PR #6).</summary>
    public Action<GroupInviteResponseEnvelope>? OnGroupInviteResponse { get; set; }

    /// <summary>Inbound roster update for a federated group (PR #6).</summary>
    public Action<GroupRosterUpdateEnvelope>? OnGroupRosterUpdate { get; set; }

    /// <summary>Inbound presence query - is the named character online here? (PR #6).</summary>
    public Func<PresenceQueryEnvelope, PresenceReplyEnvelope>? OnPresenceQuery { get; set; }

    /// <summary>Fired when the underlying connection drops.</summary>
    public event Action? Disconnected;

    public FederationLink(Socket socket)
    {
        _connection = new InteropConnection(socket);
        _connection.OnMethodCallAsync = HandleAsync;
        _connection.OnDisconnected = () => Disconnected?.Invoke();
    }

    /// <summary>Dialer side: send PeerHello, await PeerHelloAck.</summary>
    public async Task ConnectAsAsync(
        uint myClusterId,
        string myDisplayTag,
        byte[] peerSecret,
        CancellationToken ct = default)
    {
        var nonce = PeerAuth.FreshNonce();
        var hello = new PeerHello
        {
            ClusterId = myClusterId,
            Nonce = nonce,
            Hmac = PeerAuth.ComputeHmac(peerSecret, myClusterId, nonce),
            DisplayTag = myDisplayTag,
        };

        _connection.StartReceiving();
        var ackBytes = await _connection.SendRequestAsync(
            (InteropMethodId)AdminMethodId.PeerHello,
            hello.Serialize(),
            timeoutMs: 10_000);
        var ack = PeerHelloAck.Deserialize(ackBytes);
        RemoteClusterId = ack.ClusterId;
        RemoteDisplayTag = ack.DisplayTag;
        IsAuthenticated = true;
    }

    /// <summary>Send an admin command and await the reply.</summary>
    public async Task<AdminCommandReply> SendAdminCommandAsync(AdminCommand cmd, int timeoutMs = 30_000)
    {
        if (!IsAuthenticated)
            throw new InvalidOperationException("FederationLink is not authenticated");
        var bytes = await _connection.SendRequestAsync(
            (InteropMethodId)AdminMethodId.AdminCommand,
            cmd.Serialize(),
            timeoutMs);
        return AdminCommandReply.Deserialize(bytes);
    }

    /// <summary>Forward a cross-realm chat envelope to this peer (fire-and-forget).</summary>
    public Task SendChatAsync(ChatEnvelope env)
        => _connection.SendOneWayAsync((InteropMethodId)AdminMethodId.ChatRoute, env.Serialize());

    /// <summary>Forward a group invite to this peer.</summary>
    public Task SendGroupInviteAsync(GroupInviteEnvelope env)
        => _connection.SendOneWayAsync((InteropMethodId)AdminMethodId.GroupInvite, env.Serialize());

    /// <summary>Forward an invite response to this peer.</summary>
    public Task SendGroupInviteResponseAsync(GroupInviteResponseEnvelope env)
        => _connection.SendOneWayAsync((InteropMethodId)AdminMethodId.GroupInviteResponse, env.Serialize());

    /// <summary>Replicate a roster update to this peer.</summary>
    public Task SendGroupRosterAsync(GroupRosterUpdateEnvelope env)
        => _connection.SendOneWayAsync((InteropMethodId)AdminMethodId.GroupRosterUpdate, env.Serialize());

    /// <summary>Ask the peer if it has the named character online.</summary>
    public async Task<PresenceReplyEnvelope> QueryPresenceAsync(PresenceQueryEnvelope env, int timeoutMs = 5000)
    {
        var bytes = await _connection.SendRequestAsync(
            (InteropMethodId)AdminMethodId.PresenceQuery,
            env.Serialize(),
            timeoutMs);
        return PresenceReplyEnvelope.Deserialize(bytes);
    }

    private async Task<byte[]?> HandleAsync(InteropMethodId methodId, byte[] data)
    {
        var amid = (AdminMethodId)methodId;
        switch (amid)
        {
            case AdminMethodId.PeerHello:
                {
                    var hello = PeerHello.Deserialize(data);
                    var verdict = OnPeerHello?.Invoke(hello);
                    if (verdict is null)
                    {
                        // Auth rejected; reply with empty ack so the dialer fails fast.
                        return Array.Empty<byte>();
                    }
                    var (remoteId, remoteTag) = verdict.Value;
                    RemoteClusterId = remoteId;
                    RemoteDisplayTag = remoteTag;
                    IsAuthenticated = true;
                    var ack = new PeerHelloAck { ClusterId = remoteId, DisplayTag = remoteTag };
                    return ack.Serialize();
                }

            case AdminMethodId.AdminCommand:
                {
                    if (!IsAuthenticated)
                        return new AdminCommandReply { Status = AdminReplyStatus.NotPermitted, Lines = { "not authenticated" } }.Serialize();
                    var cmd = AdminCommand.Deserialize(data);
                    var handler = AdminHandler;
                    if (handler is null)
                        return new AdminCommandReply { Status = AdminReplyStatus.Failed, Lines = { "no admin handler bound" } }.Serialize();
                    var reply = await handler.ExecuteAsync(cmd);
                    return reply.Serialize();
                }

            case AdminMethodId.PeerHeartbeat:
                return Array.Empty<byte>();

            case AdminMethodId.ChatRoute:
                if (IsAuthenticated)
                    OnChatRoute?.Invoke(ChatEnvelope.Deserialize(data));
                return null;

            case AdminMethodId.GroupInvite:
                if (IsAuthenticated)
                    OnGroupInvite?.Invoke(GroupInviteEnvelope.Deserialize(data));
                return null;

            case AdminMethodId.GroupInviteResponse:
                if (IsAuthenticated)
                    OnGroupInviteResponse?.Invoke(GroupInviteResponseEnvelope.Deserialize(data));
                return null;

            case AdminMethodId.GroupRosterUpdate:
                if (IsAuthenticated)
                    OnGroupRosterUpdate?.Invoke(GroupRosterUpdateEnvelope.Deserialize(data));
                return null;

            case AdminMethodId.PresenceQuery:
                {
                    if (!IsAuthenticated || OnPresenceQuery is null)
                        return new PresenceReplyEnvelope { Name = "", Online = false }.Serialize();
                    var q = PresenceQueryEnvelope.Deserialize(data);
                    var reply = OnPresenceQuery(q);
                    return reply.Serialize();
                }

            default:
                return null;
        }
    }

    /// <summary>The listener calls this after wiring its OnPeerHello handler.</summary>
    internal void StartReceiving() => _connection.StartReceiving();

    public void Dispose() => _connection.Dispose();
}
