using Mangos.Network.Tcp.Extensions;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;

namespace Mangos.Network.Tcp
{
    public class TcpServer
	{
        private readonly ITcpClientFactory tcpClientFactory;
		private readonly Socket socket;
		private readonly CancellationTokenSource cancellationTokenSource;

		public TcpServer(ITcpClientFactory tcpClientFactory)
		{
			this.tcpClientFactory = tcpClientFactory;
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			cancellationTokenSource = new CancellationTokenSource();
		}

		public void Start(IPEndPoint endPoint, int backlog)
		{
			socket.Bind(endPoint);
			socket.Listen(backlog);
			StartAcceptLoop();
		}

		private async void StartAcceptLoop()
		{
			while (cancellationTokenSource.IsCancellationRequested)
			{
				var client = await socket.AcceptAsync();
				OnAcceptAsync(client);
			}
		}

		private async void OnAcceptAsync(Socket clientSocket)
		{
			var tcpClient = await tcpClientFactory.CreateTcpClientAsync();
			var recieveChannel = Channel.CreateUnbounded<byte>();
			var sendChannel = Channel.CreateUnbounded<byte>();

            RecieveAsync(clientSocket, recieveChannel.Writer);
			SendAsync(clientSocket, sendChannel.Reader);
			tcpClient.HandleAsync(recieveChannel.Reader, sendChannel.Writer, cancellationTokenSource.Token);
		}


		private async void RecieveAsync(Socket client, ChannelWriter<byte> writer)
		{
			var buffer = new byte[client.ReceiveBufferSize];
			while(!cancellationTokenSource.IsCancellationRequested)
            {
				var bytesRead = await client.ReceiveAsync(buffer, SocketFlags.None);
				await writer.WriteAsync(buffer, bytesRead);
			}
		}

		private async void SendAsync(Socket client, ChannelReader<byte> reader)
		{
			var buffer = new byte[client.SendBufferSize];
			while (!cancellationTokenSource.IsCancellationRequested)
			{
				var writeCount = await reader.ReadAllAsync().Select((x, i) => buffer[i] = x).CountAsync();
				var arraySegment = new ArraySegment<byte>(buffer, 0, writeCount);
				await client.SendAsync(arraySegment, SocketFlags.None);
			}
		}
	}
}
