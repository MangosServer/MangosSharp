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

using System;

namespace Mangos.World.AntiCheat;

public class SpeedHackViolation
{
    public string Character;

    public int Violations;

    public int LastClientTime;

    public int LastServerTime;

    public int TotalOffset;

    public string LastMessage;

    public ViolationType LastViolation;

    public SpeedHackViolation(string Name, int cTime, int sTime)
    {
        LastViolation = ViolationType.AC_VIOLATION_NONE;
        Character = Name ?? throw new ArgumentNullException(nameof(Name));
        Violations = 0;
        LastClientTime = cTime;
        LastServerTime = sTime;
        TotalOffset = 0;
        LastMessage = "";
    }

    public float PlayerMoveDistance(float posX, float positionX, float posY, float positionY, float posZ, float positionZ)
    {
        return (float)Math.Sqrt(Math.Pow(Math.Abs(posX - positionX), 2.0) + Math.Pow(Math.Abs(posY - positionY), 2.0) + Math.Pow(Math.Abs(posZ - positionZ), 2.0));
    }

    public int GetTotalOffset(int cTime, int sTime)
    {
        return Math.Abs(checked(cTime - LastClientTime - (sTime - LastServerTime)));
    }

    public void TriggerViolation(float posX, float positionX, float posY, float positionY, float posZ, float positionZ, int sTime, int cTime, float RunSpeed)
    {
        if (posX != positionX && posY != positionY && posZ != positionZ)
        {
            TotalOffset = GetTotalOffset(cTime, sTime);
            var Distance = PlayerMoveDistance(posX, positionX, posY, positionY, posZ, positionZ);
            switch (TotalOffset)
            {
                case >= 235 and < 35000:
                    LastMessage = $"Time Hack | Offset: {TotalOffset}";
                    LastViolation = ViolationType.AC_VIOLATION_SPEEDHACK_TIME;
                    break;
                default:
                    if (Distance >= RunSpeed)
                    {
                        var Estimate = (float)(Distance * 1.54);
                        LastMessage = $"Memory Hack | Distance: {Distance} Estimated Speed: {Estimate}";
                        LastViolation = ViolationType.AC_VIOLATION_SPEEDHACK_MEM;
                    }
                    else if (Math.Abs(posZ - positionZ) >= 10f)
                    {
                        LastMessage = $"Jump/Fly Hack | Z: {posZ}";
                        LastViolation = ViolationType.AC_VIOLATION_MOVEMENT_Z;
                    }
                    else
                    {
                        LastMessage = "";
                        LastViolation = ViolationType.AC_VIOLATION_NONE;
                    }

                    break;
            }
        }
        LastClientTime = cTime;
        LastServerTime = sTime;
    }
}
