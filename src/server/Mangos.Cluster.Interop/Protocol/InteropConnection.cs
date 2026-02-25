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

    public InteropConnection(Socket socket)
    {
        _socket = socket;
    }

    public void StartReceiving()
    {
        _cts = new CancellationTokenSource();
        _receiveLoop = Task.Run(() => ReceiveLoopAsync(_cts.Token));
    }

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested && _socket.Connected)
            {
                var headerBuf = new byte[FrameHeaderSize];
                await ReadExactAsync(headerBuf, ct);

                var payloadLength = BinaryPrimitives.ReadInt32LittleEndian(headerBuf.AsSpan(0, 4));
                if (payloadLength < 0 || payloadLength > MaxFrameSize)
                {
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

                if (methodId == InteropMethodId.Response)
                {
                    HandleResponse(requestId, data);
                }
                else
                {
                    _ = Task.Run(() => HandleIncomingCall(methodId, requestId, data), ct);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }
        catch (Exception)
        {
            // Connection lost
        }
        finally
        {
            // Cancel all pending requests
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
            tcs.TrySetResult(data);
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

            // Send response if this was a request (non-zero request ID)
            if (requestId != 0)
            {
                await SendFrameAsync(InteropMethodId.Response, requestId, result ?? Array.Empty<byte>());
            }
        }
        catch (Exception)
        {
            // Send empty response on error to unblock the caller
            if (requestId != 0)
            {
                try
                {
                    await SendFrameAsync(InteropMethodId.Response, requestId, Array.Empty<byte>());
                }
                catch
                {
                    // Connection may be dead
                }
            }
        }
    }

    /// <summary>
    /// Send a fire-and-forget call (no response expected).
    /// </summary>
    public async Task SendOneWayAsync(InteropMethodId methodId, byte[] data)
    {
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

        try
        {
            await SendFrameAsync(methodId, requestId, data);

            using var cts = new CancellationTokenSource(timeoutMs);
            await using var registration = cts.Token.Register(() =>
                tcs.TrySetException(new TimeoutException($"RPC call {methodId} timed out after {timeoutMs}ms")));

            return await tcs.Task;
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
                throw new IOException("Connection closed by remote");
            }
            offset += read;
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        try { _socket.Shutdown(SocketShutdown.Both); } catch { }
        _socket.Dispose();
        _cts?.Dispose();
        _writeLock.Dispose();
    }
}
