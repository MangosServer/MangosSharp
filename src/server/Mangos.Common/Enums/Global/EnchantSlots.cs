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

public enum EnchantSlots : byte
{
    ENCHANTMENT_PERM = 0,
    ENCHANTMENT_TEMP = 1,
    ENCHANTMENT_BONUS = 2,
    MAX_INSPECT = 3,
    ENCHANTMENT_PROP_SLOT_1 = 3, // used with RandomSuffix
    ENCHANTMENT_PROP_SLOT_2 = 4, // used with RandomSuffix
    ENCHANTMENT_PROP_SLOT_3 = 5, // used with RandomSuffix and RandomProperty
    ENCHANTMENT_PROP_SLOT_4 = 6, // used with RandomProperty
    ENCHANTMENT_PROP_SLOT_5 = 7, // used with RandomProperty
    MAX_ENCHANTS = 8
}
