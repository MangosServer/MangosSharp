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
using Mangos.Common.Enums.Misc;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Player;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;

namespace Mangos.World.Weather;

public partial class WS_Weather
{
    public class WeatherZone
    {
        public int ZoneID;

        public WeatherSeasonChances[] Seasons;

        public WeatherType CurrentWeather;

        public float Intensity;

        public WeatherZone(int ZoneID)
        {
            Seasons = new WeatherSeasonChances[4];
            CurrentWeather = WeatherType.WEATHER_FINE;
            Intensity = 0f;
            this.ZoneID = ZoneID;
        }

        public void Update()
        {
            if (ChangeWeather())
            {
                SendUpdate();
            }
        }

        public bool ChangeWeather()
        {
            var u = WorldServiceLocator._WorldServer.Rnd.Next(0, 100);
            if (u >= 30)
            {
                var oldWeather = CurrentWeather;
                var oldIntensity = Intensity;
                var TimeSince1Jan = checked((int)DateAndTime.Now.Subtract(new DateTime(DateAndTime.Now.Year, 1, 1)).TotalDays);
                var Season = checked(TimeSince1Jan - 78 + 365) / 91 % 4;
                if (u < 60 && Intensity < 0.333333343f)
                {
                    CurrentWeather = WeatherType.WEATHER_FINE;
                    Intensity = 0f;
                }
                if (u < 60 && CurrentWeather != 0)
                {
                    Intensity -= 0.333333343f;
                    return true;
                }
                if (u < 90 && CurrentWeather != 0)
                {
                    Intensity += 0.333333343f;
                    return true;
                }
                if (CurrentWeather != 0)
                {
                    if (Intensity < 0.333333343f)
                    {
                        Intensity = 0.9999f;
                        return true;
                    }
                    if (Intensity > 2f / 3f)
                    {
                        var v = WorldServiceLocator._WorldServer.Rnd.Next(0, 100);
                        if (v < 50)
                        {
                            Intensity -= 2f / 3f;
                            return true;
                        }
                    }
                    CurrentWeather = WeatherType.WEATHER_FINE;
                    Intensity = 0f;
                }
                var chance1 = Seasons[Season].RainChance;
                checked
                {
                    var chance2 = chance1 + Seasons[Season].SnowChance;
                    var chance3 = chance2 + Seasons[Season].StormChance;
                    var r = WorldServiceLocator._WorldServer.Rnd.Next(0, 100);
                    if (r < chance1)
                    {
                        CurrentWeather = WeatherType.WEATHER_RAIN;
                    }
                    else if (r < chance2)
                    {
                        CurrentWeather = WeatherType.WEATHER_SNOW;
                    }
                    else
                    {
                        CurrentWeather = r < chance3 ? WeatherType.WEATHER_SANDSTORM : WeatherType.WEATHER_FINE;
                    }
                    if (CurrentWeather == WeatherType.WEATHER_FINE)
                    {
                        Intensity = 0f;
                    }
                    else if (u < 90)
                    {
                        Intensity = (float)(WorldServiceLocator._WorldServer.Rnd.NextDouble() * 0.33329999446868896);
                    }
                    else
                    {
                        r = WorldServiceLocator._WorldServer.Rnd.Next(0, 100);
                        Intensity = r < 50
                            ? (float)(WorldServiceLocator._WorldServer.Rnd.NextDouble() * 0.33329999446868896) + 0.3334f
                            : (float)(WorldServiceLocator._WorldServer.Rnd.NextDouble() * 0.33329999446868896) + 0.6667f;
                    }
                    return CurrentWeather != oldWeather || Intensity != oldIntensity;
                }
            }
            bool ChangeWeather = default;
            return ChangeWeather;
        }

        public int GetSound()
        {
            switch (CurrentWeather)
            {
                case WeatherType.WEATHER_RAIN:
                    if (Intensity < 0.333333343f)
                    {
                        return 8533;
                    }
                    if (Intensity < 2f / 3f)
                    {
                        return 8534;
                    }
                    return 8535;

                case WeatherType.WEATHER_SNOW:
                    if (Intensity < 0.333333343f)
                    {
                        return 8536;
                    }
                    if (Intensity < 2f / 3f)
                    {
                        return 8537;
                    }
                    return 8538;

                case WeatherType.WEATHER_SANDSTORM:
                    if (Intensity < 0.333333343f)
                    {
                        return 8556;
                    }
                    if (Intensity < 2f / 3f)
                    {
                        return 8557;
                    }
                    return 8558;

                default:
                    return 0;
            }
        }

        public void SendUpdate()
        {
            Packets.PacketClass SMSG_WEATHER = new(Opcodes.SMSG_WEATHER);
            SMSG_WEATHER.AddInt32((int)CurrentWeather);
            SMSG_WEATHER.AddSingle(Intensity);
            SMSG_WEATHER.AddInt32(GetSound());
            try
            {
                WorldServiceLocator._WorldServer.CHARACTERs_Lock.AcquireReaderLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                try
                {
                    foreach (var Character in WorldServiceLocator._WorldServer.CHARACTERs)
                    {
                        if (Character.Value.client != null && Character.Value.ZoneID == ZoneID)
                        {
                            Character.Value.client.SendMultiplyPackets(ref SMSG_WEATHER);
                        }
                    }
                }
                catch (Exception ex4)
                {
                    ProjectData.SetProjectError(ex4);
                    var ex3 = ex4;
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error updating Weather.{0}{1}", Environment.NewLine, ex3.ToString());
                    ProjectData.ClearProjectError();
                }
                finally
                {
                    WorldServiceLocator._WorldServer.CHARACTERs_Lock.ReleaseReaderLock();
                }
            }
            catch (ApplicationException ex5)
            {
                ProjectData.SetProjectError(ex5);
                var ex2 = ex5;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Update: Weather Manager timed out");
                ProjectData.ClearProjectError();
            }
            catch (Exception ex6)
            {
                ProjectData.SetProjectError(ex6);
                var ex = ex6;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error updating Weather.{0}{1}", Environment.NewLine, ex.ToString());
                ProjectData.ClearProjectError();
            }
            SMSG_WEATHER.Dispose();
        }
    }
}
