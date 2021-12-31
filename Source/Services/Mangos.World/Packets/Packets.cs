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

using Mangos.Common.Enums.Global;
using Mangos.World.Network;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Text;

namespace Mangos.World.Globals;

public partial class Packets
{
    public void DumpPacket(byte[] data, WS_Network.ClientClass client = null, int start = 0)
    {
        var buffer = "";
        checked
        {
            try
            {
                buffer = (client != null) ? (buffer + $"[{client.IP}:{client.Port}] DEBUG: Packet Dump - Length={data.Length - start}{Environment.NewLine}") : (buffer + $"DEBUG: Packet Dump{Environment.NewLine}");
                if (checked(data.Length - start) % 16 == 0)
                {
                    var num = data.Length - 1;
                    for (var i = start; i <= num; i += 16)
                    {
                        buffer = buffer + "|  " + BitConverter.ToString(data, i, 16).Replace("-", " ");
                        buffer = buffer + " |  " + Encoding.ASCII.GetString(data, i, 16).Replace("\t", "?").Replace("\b", "?")
                            .Replace("\r", "?")
                            .Replace("\f", "?")
                            .Replace("\n", "?") + " |" + Environment.NewLine;
                    }
                }
                else
                {
                    var num2 = data.Length - 1 - 16;
                    int i;
                    for (i = start; i <= num2; i += 16)
                    {
                        buffer = buffer + "|  " + BitConverter.ToString(data, i, 16).Replace("-", " ");
                        buffer = buffer + " |  " + Encoding.ASCII.GetString(data, i, 16).Replace("\t", "?").Replace("\b", "?")
                            .Replace("\r", "?")
                            .Replace("\f", "?")
                            .Replace("\n", "?") + " |" + Environment.NewLine;
                    }
                    unchecked
                    {
                        buffer = buffer + "|  " + BitConverter.ToString(data, i, checked(data.Length - start) % 16).Replace("-", " ");
                    }
                    buffer += new string(' ', (16 - (checked(data.Length - start) % 16)) * 3);
                    unchecked
                    {
                        buffer = buffer + " |  " + Encoding.ASCII.GetString(data, i, checked(data.Length - start) % 16).Replace("\t", "?").Replace("\b", "?")
                            .Replace("\r", "?")
                            .Replace("\f", "?")
                            .Replace("\n", "?");
                    }
                    buffer += new string(' ', 16 - (checked(data.Length - start) % 16));
                    buffer = buffer + " |" + Environment.NewLine;
                }
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, buffer, null);
            }
            catch (Exception ex)
            {
                ProjectData.SetProjectError(ex);
                var e = ex;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error dumping packet: {0}{1}", Environment.NewLine, e.ToString());
                ProjectData.ClearProjectError();
            }
        }
    }
}
