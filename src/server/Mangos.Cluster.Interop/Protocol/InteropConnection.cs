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

using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace Mangos.Cluster.Interop.Protocol;

/// <summary>
/// Bidirectional TCP connection for cluster-world IPC.
///
/// Frame format:
///   [4 bytes LE: payload length (excludes this header)]
///   [2 bytes LE: method ID (InteropMethodId)]
///   [4 bytes LE: request ID (0 = fire-and-forget, >0 = expects response)]
///   [N bytes: BinaryWriter-serialized parameters]
///
/// Response frames use method ID 0xFFFF (Response) and carry the original request ID.
/// </summary>
public sealed class InteropConnection : IDisposable
{
    private const int FrameHeaderSize = 10; // 4 (length) + 2 (method) + 4 (requestId)
    private const int MaxFrameSize = 1024 * 1024; // 1MB max frame

    private readonly Socket _socket;
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private readonly ConcurrentDictionary<uint, TaskCompletionSource<byte[]>> _pendingRequests = new();
    private uint _nextRequestId;

    private CancellationTokenSource? _cts;
    private Task? _receiveLoop;

    public Func<InteropMethodId, byte[], byte[]?>? OnMethodCall { get; set; }
    public Func<InteropMethodId, byte[], Task<byte[]?>>? OnMethodCallAsync { get; set; }
    public Action? OnDisconnected { get; set; }

    public bool IsConnected => _socket.Connected;

    private long _framesReceived;
    private long _framesSent;
    private long _bytesReceived;
    private long _bytesSent;

    public InteropConnection(Socket socket)
    {
        _socket = socket;
        Console.WriteLine($"[IPC] InteropConnection created, remote: {socket.RemoteEndPoint}, local: {socket.LocalEndPoint}");
    }

    public void StartReceiving()
    {
        Console.WriteLine("[IPC] Starting receive loop");
        _cts = new CancellationTokenSource();
        _receiveLoop = Task.Run(() => ReceiveLoopAsync(_cts.Token));
        Console.WriteLine("[IPC] Receive loop task started");
    }

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        Console.WriteLine("[IPC] Receive loop entered");
        try
        {
            while (!ct.IsCancellationRequested && _socket.Connected)
            {
                var headerBuf = new byte[FrameHeaderSize];
                await ReadExactAsync(headerBuf, ct);

                var payloadLength = BinaryPrimitives.ReadInt32LittleEndian(headerBuf.AsSpan(0, 4));
                if (payloadLength < 0 || payloadLength > MaxFrameSize)
                {
                    Console.WriteLine($"[IPC] ERROR: Invalid frame size: {payloadLength} (max: {MaxFrameSize})");
                    throw new InvalidDataException($"Invalid frame size: {payloadLength}");
                }

                var methodId = (InteropMethodId)BinaryPrimitives.ReadUInt16LittleEndian(headerBuf.AsSpan(4, 2));
                var requestId = BinaryPrimitives.ReadUInt32LittleEndian(headerBuf.AsSpan(6, 4));

                // payload = total frame minus the 6 bytes (method + requestId) already in header
                var dataLength = payloadLength - 6;
                var data = dataLength > 0 ? new byte[dataLength] : Array.Empty<byte>();
                if (dataLength > 0)
                {
                    await ReadExactAsync(data, ct);
                }

                Interlocked.Increment(ref _framesReceived);
                Interlocked.Add(ref _bytesReceived, FrameHeaderSize + dataLength);

                if (methodId == InteropMethodId.Response)
                {
                    HandleResponse(requestId, data);
                }
                else
                {
                    Console.WriteLine($"[IPC] Incoming call: method={methodId}, requestId={requestId}, dataLength={dataLength}, total frames received: {_framesReceived}");
                    _ = Task.Run(() => HandleIncomingCall(methodId, requestId, data), ct);
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("[IPC] Receive loop cancelled (normal shutdown)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[IPC] Receive loop error: {ex.GetType().Name}: {ex.Message}");
        }
        finally
        {
            Console.WriteLine($"[IPC] Receive loop ended. Stats: {_framesReceived} frames received ({_bytesReceived} bytes), {_framesSent} frames sent ({_bytesSent} bytes)");
            Console.WriteLine($"[IPC] Cancelling {_pendingRequests.Count} pending request(s)");
            foreach (var kvp in _pendingRequests)
            {
                kvp.Value.TrySetException(new IOException("Connection lost"));
            }
            _pendingRequests.Clear();
            OnDisconnected?.Invoke();
        }
    }

    private void HandleResponse(uint requestId, byte[] data)
    {
        if (_pendingRequests.TryRemove(requestId, out var tcs))
        {
            Console.WriteLine($"[IPC] Response received for requestId={requestId}, data size={data.Length} bytes, pending: {_pendingRequests.Count}");
            tcs.TrySetResult(data);
        }
        else
        {
            Console.WriteLine($"[IPC] WARNING: Received response for unknown requestId={requestId}");
        }
    }

    private async Task HandleIncomingCall(InteropMethodId methodId, uint requestId, byte[] data)
    {
        try
        {
            byte[]? result = null;

            if (OnMethodCallAsync != null)
            {
                result = await OnMethodCallAsync(methodId, data);
            }
            else if (OnMethodCall != null)
            {
                result = OnMethodCall(methodId, data);
            }
            else
            {
                Console.WriteLine($"[IPC] WARNING: No handler registered for method {methodId}");
            }

            // Send response if this was a request (non-zero request ID)
            if (requestId != 0)
            {
                var responseSize = result?.Length ?? 0;
                Console.WriteLine($"[IPC] Sending response for requestId={requestId}, method={methodId}, response size={responseSize} bytes");
                await SendFrameAsync(InteropMethodId.Response, requestId, result ?? Array.Empty<byte>());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[IPC] ERROR handling method {methodId} requestId={requestId}: {ex.GetType().Name}: {ex.Message}");
            // Send empty response on error to unblock the caller
            if (requestId != 0)
            {
                try
                {
                    await SendFrameAsync(InteropMethodId.Response, requestId, Array.Empty<byte>());
                }
                catch (Exception sendEx)
                {
                    Console.WriteLine($"[IPC] ERROR: Failed to send error response: {sendEx.GetType().Name}: {sendEx.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Send a fire-and-forget call (no response expected).
    /// </summary>
    public async Task SendOneWayAsync(InteropMethodId methodId, byte[] data)
    {
        Console.WriteLine($"[IPC] SendOneWay: method={methodId}, data size={data.Length} bytes");
        await SendFrameAsync(methodId, 0, data);
    }

    /// <summary>
    /// Send a request and wait for a response.
    /// </summary>
    public async Task<byte[]> SendRequestAsync(InteropMethodId methodId, byte[] data, int timeoutMs = 30000)
    {
        var requestId = Interlocked.Increment(ref _nextRequestId);
        var tcs = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingRequests[requestId] = tcs;

        Console.WriteLine($"[IPC] SendRequest: method={methodId}, requestId={requestId}, data size={data.Length} bytes, timeout={timeoutMs}ms, pending: {_pendingRequests.Count}");
        try
        {
            await SendFrameAsync(methodId, requestId, data);

            using var cts = new CancellationTokenSource(timeoutMs);
            await using var registration = cts.Token.Register(() =>
            {
                Console.WriteLine($"[IPC] TIMEOUT: RPC call {methodId} requestId={requestId} timed out after {timeoutMs}ms");
                tcs.TrySetException(new TimeoutException($"RPC call {methodId} timed out after {timeoutMs}ms"));
            });

            var result = await tcs.Task;
            Console.WriteLine($"[IPC] Response received for requestId={requestId}, method={methodId}, response size={result.Length} bytes");
            return result;
        }
        catch
        {
            _pendingRequests.TryRemove(requestId, out _);
            throw;
        }
    }

    private async Task SendFrameAsync(InteropMethodId methodId, uint requestId, byte[] data)
    {
        // payload = 2 (method) + 4 (requestId) + data
        var payloadLength = 6 + data.Length;
        var frame = new byte[4 + payloadLength];

        BinaryPrimitives.WriteInt32LittleEndian(frame.AsSpan(0, 4), payloadLength);
        BinaryPrimitives.WriteUInt16LittleEndian(frame.AsSpan(4, 2), (ushort)methodId);
        BinaryPrimitives.WriteUInt32LittleEndian(frame.AsSpan(6, 4), requestId);

        if (data.Length > 0)
        {
            data.CopyTo(frame.AsSpan(10));
        }

        await _writeLock.WaitAsync();
        try
        {
            var sent = 0;
            while (sent < frame.Length)
            {
                sent += await _socket.SendAsync(frame.AsMemory(sent), SocketFlags.None);
            }
            Interlocked.Increment(ref _framesSent);
            Interlocked.Add(ref _bytesSent, frame.Length);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    private async Task ReadExactAsync(byte[] buffer, CancellationToken ct)
    {
        var offset = 0;
        while (offset < buffer.Length)
        {
            var read = await _socket.ReceiveAsync(buffer.AsMemory(offset), SocketFlags.None, ct);
            if (read == 0)
            {
                Console.WriteLine("[IPC] Connection closed by remote during read");
                throw new IOException("Connection closed by remote");
            }
            offset += read;
        }
    }

    public void Dispose()
    {
        Console.WriteLine($"[IPC] Disposing InteropConnection. Stats: {_framesReceived} received, {_framesSent} sent");
        _cts?.Cancel();
        try { _socket.Shutdown(SocketShutdown.Both); } catch { }
        _socket.Dispose();
        _cts?.Dispose();
        _writeLock.Dispose();
        Console.WriteLine("[IPC] InteropConnection disposed");
    }
}
