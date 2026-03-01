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

using System.Net;
using System.Net.Sockets;

namespace Mangos.Cluster.Interop.Protocol;

/// <summary>
/// TCP server that listens for world server IPC connections.
/// Used by the cluster to accept incoming world server registrations.
/// </summary>
public sealed class InteropServer : IDisposable
{
    private readonly Socket _listener;
    private CancellationTokenSource? _cts;

    public Action<InteropConnection>? OnWorldServerConnected { get; set; }

    private int _connectionsAccepted;

    public InteropServer()
    {
        _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Console.WriteLine("[IPC Server] InteropServer created");
    }

    public async Task RunAsync(string address, int port, CancellationToken ct = default)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var endpoint = new IPEndPoint(IPAddress.Parse(address), port);
        Console.WriteLine($"[IPC Server] Binding to {endpoint}");
        _listener.Bind(endpoint);
        _listener.Listen(8);
        Console.WriteLine($"[IPC Server] Listening on {endpoint} with backlog 8");
        Console.WriteLine("[IPC Server] Waiting for world server connections...");

        while (!_cts.Token.IsCancellationRequested)
        {
            try
            {
                Console.WriteLine("[IPC Server] Waiting for next IPC connection");
                var socket = await _listener.AcceptAsync(_cts.Token);
                socket.NoDelay = true;
                _connectionsAccepted++;
                Console.WriteLine($"[IPC Server] World server connected from {socket.RemoteEndPoint} (total connections: {_connectionsAccepted})");
                var connection = new InteropConnection(socket);
                OnWorldServerConnected?.Invoke(connection);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[IPC Server] Accept loop cancelled (shutdown)");
                break;
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"[IPC Server] Socket error in accept loop: {ex.SocketErrorCode}: {ex.Message}");
                break;
            }
        }
        Console.WriteLine($"[IPC Server] Server stopped. Total connections accepted: {_connectionsAccepted}");
    }

    public void Dispose()
    {
        Console.WriteLine("[IPC Server] Disposing InteropServer");
        _cts?.Cancel();
        _listener.Dispose();
        _cts?.Dispose();
        Console.WriteLine("[IPC Server] InteropServer disposed");
    }
}
