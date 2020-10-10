// 
// Copyright (C) 2013-2020 getMaNGOS <https://getmangos.eu>
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
using System.Collections.Generic;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Misc;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Player;
using Mangos.World.Server;
using Microsoft.VisualBasic;

namespace Mangos.World.Weather
{
    public class WS_Weather
    {
        public Dictionary<int, WeatherZone> WeatherZones = new Dictionary<int, WeatherZone>();

        public class WeatherSeasonChances
        {
            public int RainChance;
            public int SnowChance;
            public int StormChance;

            public WeatherSeasonChances(int RainChance, int SnowChance, int StormChance)
            {
                this.RainChance = RainChance;
                this.SnowChance = SnowChance;
                this.StormChance = StormChance;
            }
        }

        public class WeatherZone
        {
            public int ZoneID;
            public WeatherSeasonChances[] Seasons = new WeatherSeasonChances[4];
            public WeatherType CurrentWeather = WeatherType.WEATHER_FINE;
            public float Intensity = 0.0f;

            public WeatherZone(int ZoneID)
            {
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
                // Weather statistics:
                // - 30% - no change
                // - 30% - weather gets better (if not fine) or change weather type
                // - 30% - weather worsens (if not fine)
                // - 10% - radical change (if not fine)
                int u = WorldServiceLocator._WorldServer.Rnd.Next(0, 100);
                if (u < 30)
                    return default; // No change

                // remember old values
                var oldWeather = CurrentWeather;
                float oldIntensity = Intensity;

                // 78 days between January 1st and March 20nd; 365/4=91 days by season
                int TimeSince1Jan = (int)Conversion.Fix(DateAndTime.Now.Subtract(new DateTime(DateAndTime.Now.Year, 1, 1)).TotalDays);
                int Season = (TimeSince1Jan - 78 + 365) / 91 % 4;
                if (u < 60 && Intensity < 0.333333343f) // Get fine
                {
                    CurrentWeather = WeatherType.WEATHER_FINE;
                    Intensity = 0.0f;
                }

                if (u < 60 && CurrentWeather != WeatherType.WEATHER_FINE) // Get better
                {
                    Intensity -= 0.333333343f;
                    return true;
                }

                if (u < 90 && CurrentWeather != WeatherType.WEATHER_FINE) // Get worse
                {
                    Intensity += 0.333333343f;
                    return true;
                }

                if (CurrentWeather != WeatherType.WEATHER_FINE)
                {
                    // Radical change:
                    // - if light -> heavy
                    // - if medium -> change weather type
                    // - if heavy -> 50% light, 50% change weather type

                    if (Intensity < 0.333333343f)
                    {
                        Intensity = 0.9999f; // Go nuts
                        return true;
                    }
                    else
                    {
                        if (Intensity > 0.6666667f)
                        {
                            int v = WorldServiceLocator._WorldServer.Rnd.Next(0, 100);
                            if (v < 50) // Severe change, but how severe?
                            {
                                Intensity -= 0.6666667f;
                                return true;
                            }
                        }

                        CurrentWeather = WeatherType.WEATHER_FINE; // Clear up
                        Intensity = 0.0f;
                    }
                }

                // At this point, only weather that isn't doing anything remains but that have weather data
                int chance1 = Seasons[Season].RainChance;
                int chance2 = chance1 + Seasons[Season].SnowChance;
                int chance3 = chance2 + Seasons[Season].StormChance;
                int r = WorldServiceLocator._WorldServer.Rnd.Next(0, 100);
                if (r < chance1)
                {
                    CurrentWeather = WeatherType.WEATHER_RAIN;
                }
                else if (r < chance2)
                {
                    CurrentWeather = WeatherType.WEATHER_SNOW;
                }
                else if (r < chance3)
                {
                    CurrentWeather = WeatherType.WEATHER_SANDSTORM;
                }
                else
                {
                    CurrentWeather = WeatherType.WEATHER_FINE;
                }

                // New weather statistics (if not fine):
                // - 85% light
                // - 7% medium
                // - 7% heavy
                // If fine 100% sun (no fog)

                if (CurrentWeather == WeatherType.WEATHER_FINE)
                {
                    Intensity = 0.0f;
                }
                else if (u < 90)
                {
                    Intensity = (float)(WorldServiceLocator._WorldServer.Rnd.NextDouble() * 0.3333);
                }
                else
                {
                    // Severe change, but how severe?
                    r = WorldServiceLocator._WorldServer.Rnd.Next(0, 100);
                    if (r < 50)
                    {
                        Intensity = (float)(WorldServiceLocator._WorldServer.Rnd.NextDouble() * 0.3333F) + 0.3334f;
                    }
                    else
                    {
                        Intensity = (float)(WorldServiceLocator._WorldServer.Rnd.NextDouble() * 0.3333F) + 0.6667f;
                    }
                }

                // return true only in case weather changes
                return CurrentWeather != oldWeather || Intensity != oldIntensity;
            }

            public int GetSound()
            {
                switch (CurrentWeather)
                {
                    case var @case when @case == WeatherType.WEATHER_RAIN:
                        {
                            if (Intensity < 0.333333343f)
                            {
                                return (int)WeatherSounds.WEATHER_SOUND_RAINLIGHT;
                            }
                            else if (Intensity < 0.6666667f)
                            {
                                return (int)WeatherSounds.WEATHER_SOUND_RAINMEDIUM;
                            }
                            else
                            {
                                return (int)WeatherSounds.WEATHER_SOUND_RAINHEAVY;
                            }

                            break;
                        }

                    case var case1 when case1 == WeatherType.WEATHER_SNOW:
                        {
                            if (Intensity < 0.333333343f)
                            {
                                return (int)WeatherSounds.WEATHER_SOUND_SNOWLIGHT;
                            }
                            else if (Intensity < 0.6666667f)
                            {
                                return (int)WeatherSounds.WEATHER_SOUND_SNOWMEDIUM;
                            }
                            else
                            {
                                return (int)WeatherSounds.WEATHER_SOUND_SNOWHEAVY;
                            }

                            break;
                        }

                    case var case2 when case2 == WeatherType.WEATHER_SANDSTORM:
                        {
                            if (Intensity < 0.333333343f)
                            {
                                return (int)WeatherSounds.WEATHER_SOUND_SANDSTORMLIGHT;
                            }
                            else if (Intensity < 0.6666667f)
                            {
                                return (int)WeatherSounds.WEATHER_SOUND_SANDSTORMMEDIUM;
                            }
                            else
                            {
                                return (int)WeatherSounds.WEATHER_SOUND_SANDSTORMHEAVY;
                            }

                            break;
                        }

                    default:
                        {
                            return (int)WeatherSounds.WEATHER_SOUND_NOSOUND;
                        }
                }
            }

            public void SendUpdate()
            {
                var SMSG_WEATHER = new Packets.PacketClass(OPCODES.SMSG_WEATHER);
                SMSG_WEATHER.AddInt32((int)CurrentWeather);
                SMSG_WEATHER.AddSingle(Intensity);
                SMSG_WEATHER.AddInt32(GetSound());
                try
                {
                    WorldServiceLocator._WorldServer.CHARACTERs_Lock.AcquireReaderLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                    try
                    {
                        foreach (KeyValuePair<ulong, WS_PlayerData.CharacterObject> Character in WorldServiceLocator._WorldServer.CHARACTERs)
                        {
                            if (Character.Value.client is object && Character.Value.ZoneID == ZoneID)
                            {
                                Character.Value.client.SendMultiplyPackets(ref SMSG_WEATHER);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error updating Weather.{0}{1}", Environment.NewLine, ex.ToString());
                    }
                    finally
                    {
                        WorldServiceLocator._WorldServer.CHARACTERs_Lock.ReleaseReaderLock();
                    }
                }
                catch (ApplicationException)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Update: Weather Manager timed out");
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error updating Weather.{0}{1}", Environment.NewLine, ex.ToString());
                }

                SMSG_WEATHER.Dispose();
            }
        }

        public void SendWeather(int ZoneID, ref WS_Network.ClientClass client)
        {
            if (!WeatherZones.ContainsKey(ZoneID))
                return;
            var Weather = WeatherZones[ZoneID];
            var SMSG_WEATHER = new Packets.PacketClass(OPCODES.SMSG_WEATHER);
            SMSG_WEATHER.AddInt32((int)Weather.CurrentWeather);
            SMSG_WEATHER.AddSingle(Weather.Intensity);
            SMSG_WEATHER.AddInt32(Weather.GetSound());
            client.Send(SMSG_WEATHER);
            SMSG_WEATHER.Dispose();
        }
    }
}