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
using Mangos.World.Player;

namespace Mangos.World.Maps;

public partial class WS_Maps
{
    public class TArea
    {
        public int ID;

        public int mapId;

        public byte Level;

        public int Zone;

        public int ZoneType;

        public AreaTeam Team;

        public string Name;

        public bool IsMyLand(ref WS_PlayerData.CharacterObject objCharacter)
        {
            if (Team == AreaTeam.AREATEAM_NONE)
            {
                return false;
            }
            if (!objCharacter.IsHorde)
            {
                return Team == AreaTeam.AREATEAM_ALLY;
            }
            return objCharacter.IsHorde && Team == AreaTeam.AREATEAM_HORDE;
        }

        public bool IsCity()
        {
            return ZoneType == 312;
        }

        public bool NeedFlyingMount()
        {
            return (ZoneType & 0x1000) != 0;
        }

        public bool IsSanctuary()
        {
            return (ZoneType & 0x800) != 0;
        }

        public bool IsArena()
        {
            return (ZoneType & 0x80) != 0;
        }
    }
}
