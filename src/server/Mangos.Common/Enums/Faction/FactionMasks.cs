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

namespace Mangos.Common.Enums.Faction;

public enum FactionMasks
{
    FACTION_MASK_PLAYER = 1,     // any player
    FACTION_MASK_ALLIANCE = 2,   // player or creature from alliance team
    FACTION_MASK_HORDE = 4,      // player or creature from horde team
    FACTION_MASK_MONSTER = 8    // aggressive creature from monster team
}
