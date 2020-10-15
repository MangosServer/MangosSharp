//
//  Copyright (C) 2013-2020 getMaNGOS <https:\\getmangos.eu>
//  
//  This program is free software. You can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation. either version 2 of the License, or
//  (at your option) any later version.
//  
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY. Without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//  
//  You should have received a copy of the GNU General Public License
//  along with this program. If not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mangos.Common.Enums.Global;
using Mangos.World.Player;
using Mangos.World.Server;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.AntiCheat
{
    [StandardModule]
    public sealed class WS_Anticheat
    {
        [CompilerGenerated]
        internal sealed class _Closure_0024__2_002D0
        {
            public WS_PlayerData.CharacterObject _0024VB_0024Local_pChar;

            internal bool _Lambda_0024__0(SpeedHackViolation obj)
            {
                return obj.Character.Equals(_0024VB_0024Local_pChar.Name);
            }

            internal bool _Lambda_0024__1(SpeedHackViolation obj)
            {
                return obj.Character.Equals(_0024VB_0024Local_pChar.Name);
            }
        }

        private static readonly List<SpeedHackViolation> SpeedHacks = new List<SpeedHackViolation>();

        public static void MovementEvent(ref WS_Network.ClientClass client, float RunSpeed, float posX, float positionX, float posY, float positionY, float posZ, float positionZ, int sTime, int cTime)
        {
            WS_PlayerData.CharacterObject character = client.Character;
            SpeedHackViolation sData;
            if (!SpeedHacks.Exists((obj) => obj.Character.Equals(character.Name)))
            {
                sData = new SpeedHackViolation(client.Character.Name, cTime, sTime);
                SpeedHacks.Add(sData);
            }
            else
            {
                sData = SpeedHacks.Find((obj) => obj.Character.Equals(character.Name));
            }
            sData.TriggerViolation(posX, positionX, posY, positionY, posZ, positionZ, sTime, cTime, RunSpeed);
            checked
            {
                if (sData.LastViolation != 0)
                {
                    sData.Violations += (int)sData.LastViolation;
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "[AntiCheat] Player {0} triggered a speedhack violation. ({1}) {2}", client.Character.Name, sData.Violations, sData.LastMessage);
                    if (sData.Violations >= 10)
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.USER, "[AntiCheat] Player {0} exceeded violation value. Taking action.", client.Character.Name);
                        client.Character.Logout();
                        SpeedHacks.Remove(sData);
                    }
                    return;
                }
                if (sData.Violations > 0)
                {
                    switch (sData.LastViolation)
                    {
                        case ViolationType.AC_VIOLATION_NONE:
                        case ViolationType.AC_VIOLATION_SPEEDHACK_TIME:
                        case ViolationType.AC_VIOLATION_MOVEMENT_Z:
                            sData.Violations--;
                            break;
                        case ViolationType.AC_VIOLATION_SPEEDHACK_MEM:
                            sData.Violations -= 0;
                            break;
                        default:
                            break;
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
