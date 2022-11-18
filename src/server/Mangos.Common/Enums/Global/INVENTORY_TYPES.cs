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

namespace Mangos.Common.Enums.Global;

public enum INVENTORY_TYPES : byte
{
    INVTYPE_NON_EQUIP = 0x0,
    INVTYPE_HEAD = 0x1,
    INVTYPE_NECK = 0x2,
    INVTYPE_SHOULDERS = 0x3,
    INVTYPE_BODY = 0x4,           // cloth robes only
    INVTYPE_CHEST = 0x5,
    INVTYPE_WAIST = 0x6,
    INVTYPE_LEGS = 0x7,
    INVTYPE_FEET = 0x8,
    INVTYPE_WRISTS = 0x9,
    INVTYPE_HANDS = 0xA,
    INVTYPE_FINGER = 0xB,
    INVTYPE_TRINKET = 0xC,
    INVTYPE_WEAPON = 0xD,
    INVTYPE_SHIELD = 0xE,
    INVTYPE_RANGED = 0xF,
    INVTYPE_CLOAK = 0x10,
    INVTYPE_TWOHAND_WEAPON = 0x11,
    INVTYPE_BAG = 0x12,
    INVTYPE_TABARD = 0x13,
    INVTYPE_ROBE = 0x14,
    INVTYPE_WEAPONMAINHAND = 0x15,
    INVTYPE_WEAPONOFFHAND = 0x16,
    INVTYPE_HOLDABLE = 0x17,
    INVTYPE_AMMO = 0x18,
    INVTYPE_THROWN = 0x19,
    INVTYPE_RANGEDRIGHT = 0x1A,
    INVTYPE_SLOT_ITEM = 0x1B,
    INVTYPE_RELIC = 0x1C,
    NUM_INVENTORY_TYPES = 0x1D
}
