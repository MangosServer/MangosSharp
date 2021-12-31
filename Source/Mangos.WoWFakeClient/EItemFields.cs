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

namespace Mangos.WoWFakeClient;

public enum EItemFields
{
    ITEM_FIELD_OWNER = EObjectFields.OBJECT_END + 0x0,                             // 0x006 - Size: 2 - Type: GUID - Flags: PUBLIC
    ITEM_FIELD_CONTAINED = EObjectFields.OBJECT_END + 0x2,                         // 0x008 - Size: 2 - Type: GUID - Flags: PUBLIC
    ITEM_FIELD_CREATOR = EObjectFields.OBJECT_END + 0x4,                           // 0x00A - Size: 2 - Type: GUID - Flags: PUBLIC
    ITEM_FIELD_GIFTCREATOR = EObjectFields.OBJECT_END + 0x6,                       // 0x00C - Size: 2 - Type: GUID - Flags: PUBLIC
    ITEM_FIELD_STACK_COUNT = EObjectFields.OBJECT_END + 0x8,                       // 0x00E - Size: 1 - Type: INT - Flags: OWNER_ONLY + UNK2
    ITEM_FIELD_DURATION = EObjectFields.OBJECT_END + 0x9,                          // 0x00F - Size: 1 - Type: INT - Flags: OWNER_ONLY + UNK2
    ITEM_FIELD_SPELL_CHARGES = EObjectFields.OBJECT_END + 0xA,                     // 0x010 - Size: 5 - Type: INT - Flags: OWNER_ONLY + UNK2
    ITEM_FIELD_FLAGS = EObjectFields.OBJECT_END + 0xF,                             // 0x015 - Size: 1 - Type: INT - Flags: PUBLIC
    ITEM_FIELD_ENCHANTMENT = EObjectFields.OBJECT_END + 0x10,                      // 0x016 - Size: 21 - Type: INT - Flags: PUBLIC
    ITEM_FIELD_PROPERTY_SEED = EObjectFields.OBJECT_END + 0x25,                    // 0x02B - Size: 1 - Type: INT - Flags: PUBLIC
    ITEM_FIELD_RANDOM_PROPERTIES_ID = EObjectFields.OBJECT_END + 0x26,             // 0x02C - Size: 1 - Type: INT - Flags: PUBLIC
    ITEM_FIELD_ITEM_TEXT_ID = EObjectFields.OBJECT_END + 0x27,                     // 0x02D - Size: 1 - Type: INT - Flags: OWNER_ONLY
    ITEM_FIELD_DURABILITY = EObjectFields.OBJECT_END + 0x28,                       // 0x02E - Size: 1 - Type: INT - Flags: OWNER_ONLY + UNK2
    ITEM_FIELD_MAXDURABILITY = EObjectFields.OBJECT_END + 0x29,                    // 0x02F - Size: 1 - Type: INT - Flags: OWNER_ONLY + UNK2
    ITEM_END = EObjectFields.OBJECT_END + 0x2A                                    // 0x030
}
