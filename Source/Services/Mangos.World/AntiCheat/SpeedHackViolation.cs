using System;

namespace Mangos.World.AntiCheat
{
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
            Character = Name;
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
                float Distance = PlayerMoveDistance(posX, positionX, posY, positionY, posZ, positionZ);
                if (TotalOffset >= 235 && TotalOffset < 35000)
                {
                    LastMessage = $"Time Hack | Offset: {TotalOffset}";
                    LastViolation = ViolationType.AC_VIOLATION_SPEEDHACK_TIME;
                }
                else if (Distance >= RunSpeed)
                {
                    float Estimate = (float)(Distance * 1.54);
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
            }
            LastClientTime = cTime;
            LastServerTime = sTime;
        }
    }
}
