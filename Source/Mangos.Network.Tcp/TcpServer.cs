//
// Copyright (C) 2013-2022 getMaNGOS <https://getmangos.eu>
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

using Mangos.Loggers;
using Mangos.Network.Tcp.Extensions;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;

namespace Mangos.Network.Tcp;

public class TcpServer
{
    private readonly ILogger logger;
    private readonly ITcpClientFactory tcpClientFactory;
    private readonly CancellationTokenSource cancellationTokenSource;

    private Socket socket;

    public TcpServer(ILogger logger, ITcpClientFactory tcpClientFactory)
    {
        this.logger = logger;
        this.tcpClientFactory = tcpClientFactory;

        cancellationTokenSource = new CancellationTokenSource();
    }

    public void Start(IPEndPoint endPoint, int backlog)
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(endPoint);
        socket.Listen(backlog);
        StartAcceptLoop();
    }

    private async void StartAcceptLoop()
    {
        while (!cancellationTokenSource.IsCancellationRequested)
        {
            OnAcceptAsync(await socket.AcceptAsync());
        }
    }

    private async void OnAcceptAsync(Socket clientSocket)
    {
        try
        {
            var tcpClient = await tcpClientFactory.CreateTcpClientAsync(clientSocket);
            Channel<byte> recieveChannel = Channel.CreateUnbounded<byte>();
            Channel<byte> sendChannel = Channel.CreateUnbounded<byte>();

            RecieveAsync(clientSocket, recieveChannel.Writer);
            SendAsync(clientSocket, sendChannel.Reader);
            tcpClient.HandleAsync(recieveChannel.Reader, sendChannel.Writer, cancellationTokenSource.Token);

            logger.Debug("New Tcp conenction established");
        }
        catch (Exception ex)
        {
            logger.Error("Error during accepting conenction handler", ex);
        }
    }

    private async void RecieveAsync(Socket client, ChannelWriter<byte> writer)
    {
        try
        {
            var buffer = new byte[client.ReceiveBufferSize];
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                var bytesRead = await client.ReceiveAsync(buffer, SocketFlags.None);
                if (bytesRead == 0)
                {
                    client.Dispose();
                    return;
                }
                await writer.WriteArrayAsync(buffer, bytesRead);
            }
        }
        catch (Exception ex)
        {
            logger.Error("Error during recieving data from socket", ex);
        }
    }

    private async void SendAsync(Socket client, ChannelReader<byte> reader)
    {
        try
        {
            var buffer = new byte[client.SendBufferSize];
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                await reader.WaitToReadAsync();
                int writeCount;
                for (writeCount = 0;
                    writeCount < buffer.Length && reader.TryRead(out buffer[writeCount]);
                    writeCount++)
                {
                    ;
                }

                ArraySegment<byte> arraySegment = new(buffer, 0, writeCount);
                await client.SendAsync(arraySegment, SocketFlags.None);
            }
        }
        catch (Exception ex)
        {
            logger.Error("Error during sending data to socket", ex);
        }
    }
}
