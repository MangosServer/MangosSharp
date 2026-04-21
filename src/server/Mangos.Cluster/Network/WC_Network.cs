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

using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;

namespace Mangos.Cluster.Network;

public class WcNetwork
{
    private readonly ClusterServiceLocator _clusterServiceLocator;

    // FIX: initialize baseline ping time so CS0649 warning is resolved
    private readonly int _lastPing;

    public WcNetwork(ClusterServiceLocator clusterServiceLocator)
    {
        _clusterServiceLocator = clusterServiceLocator;

        // Capture initial time reference once
        _lastPing = _clusterServiceLocator.NativeMethods.timeGetTime("");
    }

    public WorldServerClass WorldServer => _clusterServiceLocator.WorldServerClass;

    public int MsTime()
    {
        // Returns elapsed time since network object was created
        return _clusterServiceLocator.NativeMethods.timeGetTime("") - _lastPing;
    }

    public Dictionary<uint, DateTime> LastConnections = new();

    public uint Ip2Int(string ip)
    {
        var parts = ip.Split('.');
        if (parts.Length != 4)
        {
            return 0U;
        }

        try
        {
            var ipBytes = new byte[4];
            ipBytes[0] = Conversions.ToByte(parts[3]);
            ipBytes[1] = Conversions.ToByte(parts[2]);
            ipBytes[2] = Conversions.ToByte(parts[1]);
            ipBytes[3] = Conversions.ToByte(parts[0]);

            return BitConverter.ToUInt32(ipBytes, 0);
        }
        catch
        {
            return 0U;
        }
    }
}
