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

using Mangos.Cluster.Network;
using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Mangos.Cluster.Globals;

public class Packets
{
    private readonly ClusterServiceLocator _clusterServiceLocator;

    public Packets(ClusterServiceLocator clusterServiceLocator)
    {
        _clusterServiceLocator = clusterServiceLocator;
    }

    public void DumpPacket(byte[] data, [Optional, DefaultParameterValue(null)] ClientClass client)
    {
        // #If DEBUG Then
        int j;
        var buffer = "";
        try
        {
            buffer = client is null ? buffer + string.Format("DEBUG: Packet Dump{0}", Constants.vbCrLf) : buffer + string.Format("[{0}:{1}] DEBUG: Packet Dump - Length={2}{3}", client.IP, client.Port, data.Length, Constants.vbCrLf);
            if (data.Length % 16 == 0)
            {
                var loopTo = data.Length - 1;
                for (j = 0; j <= loopTo; j += 16)
                {
                    buffer += "|  " + BitConverter.ToString(data, j, 16).Replace("-", " ");
                    buffer += " |  " + Encoding.ASCII.GetString(data, j, 16).Replace(Constants.vbTab, "?").Replace(Constants.vbBack, "?").Replace(Constants.vbCr, "?").Replace(Constants.vbFormFeed, "?").Replace(Constants.vbLf, "?") + " |" + Constants.vbCrLf;
                }
            }
            else
            {
                var loopTo1 = data.Length - 1 - 16;
                for (j = 0; j <= loopTo1; j += 16)
                {
                    buffer += "|  " + BitConverter.ToString(data, j, 16).Replace("-", " ");
                    buffer += " |  " + Encoding.ASCII.GetString(data, j, 16).Replace(Constants.vbTab, "?").Replace(Constants.vbBack, "?").Replace(Constants.vbCr, "?").Replace(Constants.vbFormFeed, "?").Replace(Constants.vbLf, "?") + " |" + Constants.vbCrLf;
                }

                buffer += "|  " + BitConverter.ToString(data, j, data.Length % 16).Replace("-", " ");
                buffer += new string(' ', (16 - (data.Length % 16)) * 3);
                buffer += " |  " + Encoding.ASCII.GetString(data, j, data.Length % 16).Replace(Constants.vbTab, "?").Replace(Constants.vbBack, "?").Replace(Constants.vbCr, "?").Replace(Constants.vbFormFeed, "?").Replace(Constants.vbLf, "?");
                buffer += new string(' ', 16 - (data.Length % 16));
                buffer += " |" + Constants.vbCrLf;
            }

            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, buffer, default);
        }
        // #End If
        catch (Exception e)
        {
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.FAILED, "Error dumping packet: {0}{1}", Constants.vbCrLf, e.ToString());
        }
    }

    public void LogPacket(byte[] data, bool server, [Optional, DefaultParameterValue(null)] ClientClass client)
    {
        int j;
        var buffer = "";
        try
        {
            Opcodes opcode = (Opcodes)BitConverter.ToInt16(data, 2);
            if (IgnorePacket(opcode))
            {
                return;
            }

            var startAt = 6;
            if (server)
            {
                startAt = 4;
            }

            var typeStr = "IN";
            if (server)
            {
                typeStr = "OUT";
            }

            if (client is null)
            {
                buffer += string.Format("{4} Packet: (0x{0:X4}) {1} PacketSize = {2}{3}", opcode, opcode, data.Length - startAt, Constants.vbCrLf, typeStr);
            }
            else
            {
                buffer += string.Format("[{0}:{1}] {6} Packet: (0x{2:X4}) {3} PacketSize = {4}{5}", client.IP, client.Port, opcode, opcode, data.Length - startAt, Constants.vbCrLf, typeStr);
            }

            buffer += "|------------------------------------------------|----------------|" + Constants.vbCrLf;
            buffer += "|00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F |0123456789ABCDEF|" + Constants.vbCrLf;
            buffer += "|------------------------------------------------|----------------|" + Constants.vbCrLf;
            var loopTo = data.Length - 1;
            for (j = startAt; j <= loopTo; j += 16)
            {
                if (j + 16 > data.Length)
                {
                    buffer += "|" + BitConverter.ToString(data, j, data.Length - j).Replace("-", " ");
                    buffer += new string(' ', (j + 16 - data.Length) * 3);
                    buffer += " |" + FormatPacketStr(Encoding.ASCII.GetString(data, j, data.Length - j));
                    buffer += new string(' ', j + 16 - data.Length);
                }
                else
                {
                    buffer += "|" + BitConverter.ToString(data, j, 16).Replace("-", " ");
                    buffer += " |" + FormatPacketStr(Encoding.ASCII.GetString(data, j, 16));
                }

                buffer += "|" + Constants.vbCrLf;
            }

            buffer += "-------------------------------------------------------------------" + Constants.vbCrLf + Constants.vbCrLf;
            File.AppendAllText("packets.log", buffer);
        }
        catch (Exception e)
        {
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.WARNING, $"Log Packet has thrown an Exception!", e);
        }
    }

    private bool IgnorePacket(Opcodes opcode)
    {
        if (string.Format("{0}", opcode).StartsWith("MSG_MOVE"))
        {
            return true;
        }

        switch (opcode)
        {
            case var @case when @case == Opcodes.SMSG_MONSTER_MOVE:
            case var case1 when case1 == Opcodes.SMSG_UPDATE_OBJECT:
                {
                    return true;
                }

            default:
                {
                    return false;
                }
        }
    }

    private string FormatPacketStr(string str)
    {
        for (int i = 0, loopTo = str.Length - 1; i <= loopTo; i++)
        {
            if (str[i] is < 'A' or > 'z')
            {
                str.ToCharArray()[i] = '.';
            }
        }

        return Conversions.ToString(str);
    }
}
