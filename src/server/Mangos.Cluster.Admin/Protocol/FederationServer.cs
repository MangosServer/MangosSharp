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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Mangos.Cluster.Admin.Auth;
using Mangos.Cluster.Admin.Commands;

namespace Mangos.Cluster.Admin.Protocol;

/// <summary>
/// Listens for inbound peer cluster connections. Each accepted socket
/// receives the first PeerHello frame; the link's OnPeerHello callback
/// verifies the HMAC against the per-peer secret and either accepts
/// (returning the remote identity to send back as PeerHelloAck) or
/// rejects.
/// </summary>
public sealed class FederationServer : IDisposable
{
    private readonly Func<uint, byte[]?> _peerSecretLookup;
    private readonly uint _localClusterId;
    private readonly string _localDisplayTag;
    private readonly ConcurrentDictionary<uint, FederationLink> _peers = new();
    private CancellationTokenSource? _cts;
    private Socket? _listener;

    public FederationServer(uint localClusterId, string localDisplayTag, Func<uint, byte[]?> peerSecretLookup)
    {
        _localClusterId = localClusterId;
        _localDisplayTag = localDisplayTag;
        _peerSecretLookup = peerSecretLookup;
    }

    public IReadOnlyDictionary<uint, FederationLink> Peers => _peers;

    /// <summary>Bound by the cluster: invoked for inbound admin commands on every accepted link.</summary>
    public IAdminCommandHandler? AdminHandler { get; set; }

    /// <summary>Optional hook fired per accepted link, used to wire chat/group/presence handlers.</summary>
    public Action<FederationLink>? OnLinkAccepted { get; set; }

    public Task StartAsync(string bindAddress, int port)
    {
        _cts = new CancellationTokenSource();
        _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _listener.Bind(new IPEndPoint(IPAddress.Parse(bindAddress), port));
        _listener.Listen(64);
        _ = Task.Run(() => AcceptLoopAsync(_cts.Token));
        return Task.CompletedTask;
    }

    private async Task AcceptLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && _listener is not null)
        {
            Socket socket;
            try
            {
                socket = await _listener.AcceptAsync(ct);
            }
            catch (OperationCanceledException) { return; }
            catch { continue; }

            socket.NoDelay = true;
            var link = new FederationLink(socket)
            {
                AdminHandler = AdminHandler,
            };
            OnLinkAccepted?.Invoke(link);
            link.OnPeerHello = hello =>
            {
                var secret = _peerSecretLookup(hello.ClusterId);
                if (secret is null) return null;
                if (!PeerAuth.Verify(secret, hello.ClusterId, hello.Nonce, hello.Hmac))
                    return null;
                _peers[hello.ClusterId] = link;
                link.Disconnected += () => _peers.TryRemove(hello.ClusterId, out _);
                return (_localClusterId, _localDisplayTag);
            };
            link.StartReceiving();
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        try { _listener?.Close(); } catch { }
        foreach (var l in _peers.Values) try { l.Dispose(); } catch { }
        _peers.Clear();
    }
}
