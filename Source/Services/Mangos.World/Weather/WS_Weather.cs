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

using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Network;
using System.Collections.Generic;

namespace Mangos.World.Weather;

public partial class WS_Weather
{
    public Dictionary<int, WeatherZone> WeatherZones;

    public WS_Weather()
    {
        WeatherZones = new Dictionary<int, WeatherZone>();
    }

    public void SendWeather(int ZoneID, ref WS_Network.ClientClass client)
    {
        if (WeatherZones.ContainsKey(ZoneID))
        {
            var Weather = WeatherZones[ZoneID];
            Packets.PacketClass SMSG_WEATHER = new(Opcodes.SMSG_WEATHER);
            SMSG_WEATHER.AddInt32((int)Weather.CurrentWeather);
            SMSG_WEATHER.AddSingle(Weather.Intensity);
            SMSG_WEATHER.AddInt32(Weather.GetSound());
            client.Send(ref SMSG_WEATHER);
            SMSG_WEATHER.Dispose();
        }
    }
}
