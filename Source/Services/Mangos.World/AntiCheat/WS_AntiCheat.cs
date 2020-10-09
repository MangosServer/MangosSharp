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
using Mangos.World.Server;

namespace Mangos.World
{
    public enum ViolationType
    {
        AC_VIOLATION_NONE = 0,
        AC_VIOLATION_SPEEDHACK_TIME = 1,
        AC_VIOLATION_SPEEDHACK_MEM = 3,
        AC_VIOLATION_MOVEMENT_Z = 2
    }

    public class SpeedHackViolation
    {
        // Class used to store information about a player, and their detected anticheat violations.
        public string Character; // Character name
        public int Violations; // Violation amount
        public int LastClientTime; // Last time reported by client during movement
        public int LastServerTime; // Last time reported by server during movement
        public int TotalOffset; // Difference between the offsets of the server and client times. Under normal conditions, should be <= 60-100
        public string LastMessage; // Message used in the console to show information about a triggered violation
        public ViolationType LastViolation = ViolationType.AC_VIOLATION_NONE; // Default the last violation type to none
                                                                              // Constructor
        public SpeedHackViolation(string Name, int cTime, int sTime)
        {
            Character = Name;
            Violations = 0;
            LastClientTime = cTime;
            LastServerTime = sTime;
            TotalOffset = 0;
            LastMessage = "";
        }
        // Calculate distance between new position and old position. Any value above the actual run speed value (literally) is too high, as typical movement ticks will result in value close to RunSpeed * 0.54
        public float PlayerMoveDistance(float posX, float positionX, float posY, float positionY, float posZ, float positionZ)
        {
            return (float)Math.Sqrt(Math.Pow(Math.Abs(posX - positionX), 2d) + Math.Pow(Math.Abs(posY - positionY), 2d) + Math.Pow(Math.Abs(posZ - positionZ), 2d));
        }
        // Calculate the total offset difference between client and server. If the client time offset is higher than the server by too much, it could indicate the client is ticking faster than it should be.
        public int GetTotalOffset(int cTime, int sTime)
        {
            return Math.Abs(cTime - LastClientTime - (sTime - LastServerTime));
        }
        // Run checks on the position, speed, and time of the client.
        public void TriggerViolation(float posX, float positionX, float posY, float positionY, float posZ, float positionZ, int sTime, int cTime, float RunSpeed)
        {
            // Only check for violations if the player's position has actually changed. Ignores packets only updating orientation
            if (posX != positionX && posY != positionY && posZ != positionZ)
            {
                TotalOffset = GetTotalOffset(cTime, sTime);
                float Distance = PlayerMoveDistance(posX, positionX, posY, positionY, posZ, positionZ);
                // Check for time hack. >= 235 is probably a time hack, while ABOVE 35000 is likely a client hitch.
                if (TotalOffset >= 235 && TotalOffset < 35000)
                {
                    LastMessage = string.Format("Time Hack | Offset: {0}", TotalOffset);
                    LastViolation = ViolationType.AC_VIOLATION_SPEEDHACK_TIME;
                }
                // Check for memory hack
                else if (Distance >= RunSpeed)
                {
                    float Estimate = (float)(Distance * 1.54d);
                    LastMessage = string.Format("Memory Hack | Distance: {0} Estimated Speed: {1}", Distance, Estimate);
                    LastViolation = ViolationType.AC_VIOLATION_SPEEDHACK_MEM;
                }
                // Check for Z height hack (fly/jump)
                else if (Math.Abs(posZ - positionZ) >= 10f)
                {
                    LastMessage = string.Format("Jump/Fly Hack | Z: {0}", posZ);
                    LastViolation = ViolationType.AC_VIOLATION_MOVEMENT_Z;
                }
                else
                {
                    LastMessage = "";
                    LastViolation = ViolationType.AC_VIOLATION_NONE;
                }
            }

            LastClientTime = cTime;
            LastServerTime = sTime;
        }
    }

    public static class WS_Anticheat
    {
        // List used to track user violations
        private static List<SpeedHackViolation> SpeedHacks = new List<SpeedHackViolation>();

        public static void MovementEvent(ref WS_Network.ClientClass client, float RunSpeed, float posX, float positionX, float posY, float positionY, float posZ, float positionZ, int sTime, int cTime)
        {
            SpeedHackViolation sData;
            var pChar = client.Character;
            if (!SpeedHacks.Exists(obj => obj.Character.Equals(pChar.Name)))
            {
                sData = new SpeedHackViolation(client.Character.Name, cTime, sTime);
                SpeedHacks.Add(sData);
            }
            else
            {
                sData = SpeedHacks.Find(obj => obj.Character.Equals(pChar.Name));
            }

            sData.TriggerViolation(posX, positionX, posY, positionY, posZ, positionZ, sTime, cTime, RunSpeed);
            if (sData.LastViolation != ViolationType.AC_VIOLATION_NONE)
            {
                sData.Violations += (int)sData.LastViolation;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "[AntiCheat] Player {0} triggered a speedhack violation. ({1}) {2}", client.Character.Name, sData.Violations, sData.LastMessage);
                if (sData.Violations >= 10)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.USER, "[AntiCheat] Player {0} exceeded violation value. Taking action.", client.Character.Name);
                    // Take Action
                    // client.Character.CastOnSelf(31366) ' Apply Root Anybody Forever to the cheater
                    // client.Character.SendChatMessage(client.Character, "You have been punished for cheating.", ChatMsg.CHAT_MSG_SYSTEM, LANGUAGES.LANG_GLOBAL, "Global", True)
                    client.Character.Logout(null);
                    SpeedHacks.Remove(sData);
                }
            }
            else
            {
                if (sData.Violations > 0)
                {
                    switch (sData.LastViolation)
                    {
                        case ViolationType.AC_VIOLATION_MOVEMENT_Z:
                        case ViolationType.AC_VIOLATION_SPEEDHACK_TIME:
                        case ViolationType.AC_VIOLATION_NONE:
                            {
                                sData.Violations -= 1;
                                break;
                            }

                        case ViolationType.AC_VIOLATION_SPEEDHACK_MEM:
                            {
                                sData.Violations -= 0;
                                break;
                            }
                    }
                }

                if (sData.Violations < 0)
                {
                    sData.Violations = 0;
                }
            }
        }
    }
}