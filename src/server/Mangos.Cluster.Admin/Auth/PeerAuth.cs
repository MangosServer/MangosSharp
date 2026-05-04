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
using System.Security.Cryptography;
using System.Text;

namespace Mangos.Cluster.Admin.Auth;

/// <summary>
/// HMAC-SHA256 helpers for peer cluster handshakes. The secret is the
/// shared bytes between two clusters; both sides hold a per-peer copy
/// in cluster.toml/configuration. Rotate the secret to evict.
/// </summary>
public static class PeerAuth
{
    public static byte[] ComputeHmac(byte[] secret, uint clusterId, byte[] nonce)
    {
        using var hmac = new HMACSHA256(secret);
        var prefix = new byte[4];
        prefix[0] = (byte)(clusterId & 0xFF);
        prefix[1] = (byte)((clusterId >> 8) & 0xFF);
        prefix[2] = (byte)((clusterId >> 16) & 0xFF);
        prefix[3] = (byte)((clusterId >> 24) & 0xFF);
        hmac.TransformBlock(prefix, 0, prefix.Length, null, 0);
        hmac.TransformFinalBlock(nonce, 0, nonce.Length);
        return hmac.Hash ?? Array.Empty<byte>();
    }

    public static bool Verify(byte[] secret, uint clusterId, byte[] nonce, byte[] presented)
    {
        var expected = ComputeHmac(secret, clusterId, nonce);
        return CryptographicOperations.FixedTimeEquals(expected, presented);
    }

    public static byte[] FreshNonce(int sizeBytes = 16)
    {
        var n = new byte[sizeBytes];
        RandomNumberGenerator.Fill(n);
        return n;
    }

    /// <summary>Convenience: convert a UTF-8 string secret (config-friendly) into bytes.</summary>
    public static byte[] SecretFromString(string s) => Encoding.UTF8.GetBytes(s ?? string.Empty);
}
