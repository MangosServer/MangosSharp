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
using Mangos.World.Player;
using Microsoft.VisualBasic.CompilerServices;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Mangos.World.AntiCheat;

[StandardModule]
public sealed class WS_Anticheat
{
    private static readonly List<SpeedHackViolation> SpeedHacks = new();

    public static void MovementEvent(ref WS_Network.ClientClass client, float RunSpeed, float posX, float positionX, float posY, float positionY, float posZ, float positionZ, int sTime, int cTime)
    {
        var character = client.Character;
        SpeedHackViolation sData;
        if (!SpeedHacks.Exists(obj => obj.Character.Equals(character.Name, System.StringComparison.Ordinal)))
        {
            sData = new SpeedHackViolation(client.Character.Name, cTime, sTime);
            SpeedHacks.Add(sData);
        }
        else
        {
            sData = SpeedHacks.Find(match: obj => obj.Character.Equals(character.Name, System.StringComparison.Ordinal));
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
