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

using Autofac;
using Mangos.Logging;
using System.Net;
using System.Net.Sockets;

namespace Mangos.Tcp;

public sealed class TcpServer
{
    private readonly IMangosLogger logger;
    private readonly ILifetimeScope lifetimeScope;
    private int _activeConnections;

    public TcpServer(IMangosLogger logger, ILifetimeScope lifetimeScope)
    {
        this.logger = logger;
        this.lifetimeScope = lifetimeScope;
    }

    public async Task RunAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        socket.Bind(IPEndPoint.Parse(endpoint));
        socket.Listen(10);

        logger.Information($"TCP server started on {endpoint}");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                HandleClientConnection(await socket.AcceptAsync(cancellationToken), cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            logger.Information("TCP server shutting down");
        }
        finally
        {
            socket.Dispose();
            logger.Information("TCP server stopped");
        }
    }

    private async void HandleClientConnection(Socket socket, CancellationToken cancellationToken)
    {
        if (socket.RemoteEndPoint is not IPEndPoint endpoint)
        {
            logger.Error("Unable to get remote endpoint");
            socket.Dispose();
            return;
        }

        var connectionCount = Interlocked.Increment(ref _activeConnections);
        logger.Information($"TCP client connected: {endpoint} (active connections: {connectionCount})");
        try
        {
            using var scope = lifetimeScope.BeginLifetimeScope();
            var tcpConnection = scope.Resolve<ITcpConnection>();
            await tcpConnection.ExecuteAsync(socket, cancellationToken);
        }
        catch (SocketException exception) when (exception.SocketErrorCode == SocketError.ConnectionAborted ||
                                                  exception.SocketErrorCode == SocketError.ConnectionReset)
        {
            logger.Information($"Connection closed by client: {endpoint}");
        }
        catch (IOException)
        {
            logger.Information($"Connection lost: {endpoint}");
        }
        catch (OperationCanceledException)
        {
            logger.Debug($"Connection cancelled: {endpoint}");
        }
        catch (Exception exception)
        {
            logger.Error(exception, $"Unhandled exception from client {endpoint}");
        }
        finally
        {
            socket.Dispose();
            connectionCount = Interlocked.Decrement(ref _activeConnections);
            logger.Information($"TCP client disconnected: {endpoint} (active connections: {connectionCount})");
        }
    }
}
