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

namespace Mangos.Common.Enums.Unit;

[Flags]   // Flags for units
public enum UnitFlags
{
    UNIT_FLAG_NONE = 0x0,
    UNIT_FLAG_UNK1 = 0x1,
    UNIT_FLAG_NOT_ATTACKABLE = 0x2,                                              // Unit is not attackable
    UNIT_FLAG_DISABLE_MOVE = 0x4,                                                // Unit is frozen, rooted or stunned
    UNIT_FLAG_ATTACKABLE = 0x8,                                                  // Unit becomes temporarily hostile, shows in red, allows attack
    UNIT_FLAG_RENAME = 0x10,
    UNIT_FLAG_RESTING = 0x20,
    UNIT_FLAG_UNK5 = 0x40,
    UNIT_FLAG_NOT_ATTACKABLE_1 = 0x80,                                           // Unit cannot be attacked by player, shows no attack cursor
    UNIT_FLAG_UNK6 = 0x100,
    UNIT_FLAG_UNK7 = 0x200,
    UNIT_FLAG_NON_PVP_PLAYER = UNIT_FLAG_ATTACKABLE + UNIT_FLAG_NOT_ATTACKABLE_1, // Unit cannot be attacked by player, shows in blue
    UNIT_FLAG_LOOTING = 0x400,
    UNIT_FLAG_PET_IN_COMBAT = 0x800,
    UNIT_FLAG_PVP = 0x1000,
    UNIT_FLAG_SILENCED = 0x2000,
    UNIT_FLAG_DEAD = 0x4000,
    UNIT_FLAG_UNK11 = 0x8000,
    UNIT_FLAG_ROOTED = 0x10000,
    UNIT_FLAG_PACIFIED = 0x20000,
    UNIT_FLAG_STUNTED = 0x40000,
    UNIT_FLAG_IN_COMBAT = 0x80000,
    UNIT_FLAG_TAXI_FLIGHT = 0x100000,
    UNIT_FLAG_DISARMED = 0x200000,
    UNIT_FLAG_CONFUSED = 0x400000,
    UNIT_FLAG_FLEEING = 0x800000,
    UNIT_FLAG_UNK21 = 0x1000000,
    UNIT_FLAG_NOT_SELECTABLE = 0x2000000,
    UNIT_FLAG_SKINNABLE = 0x4000000,
    UNIT_FLAG_MOUNT = 0x8000000,
    UNIT_FLAG_UNK25 = 0x10000000,
    UNIT_FLAG_UNK26 = 0x20000000,
    UNIT_FLAG_SKINNABLE_AND_DEAD = UNIT_FLAG_SKINNABLE + UNIT_FLAG_DEAD,
    UNIT_FLAG_SPIRITHEALER = UNIT_FLAG_UNK21 + UNIT_FLAG_NOT_ATTACKABLE + UNIT_FLAG_DISABLE_MOVE + UNIT_FLAG_RESTING + UNIT_FLAG_UNK5,
    UNIT_FLAG_SHEATHE = 0x40000000
}
