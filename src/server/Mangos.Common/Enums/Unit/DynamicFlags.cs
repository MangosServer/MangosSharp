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

namespace Mangos.Common.Enums.Unit;

public enum DynamicFlags   // Dynamic flags for units
{
    // Unit has blinking stars effect showing lootable
    UNIT_DYNFLAG_LOOTABLE = 0x1,

    // Shows marked unit as small red dot on radar
    UNIT_DYNFLAG_TRACK_UNIT = 0x2,

    // Gray mob title marks that mob is tagged by another player
    UNIT_DYNFLAG_OTHER_TAGGER = 0x4,

    // Blocks player character from moving
    UNIT_DYNFLAG_ROOTED = 0x8,

    // Shows infos like Damage and Health of the enemy
    UNIT_DYNFLAG_SPECIALINFO = 0x10,

    // Unit falls on the ground and shows like dead
    UNIT_DYNFLAG_DEAD = 0x20
}
