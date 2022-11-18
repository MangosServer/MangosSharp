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

namespace Mangos.Common.Globals;

public enum EGameObjectFields
{
    OBJECT_FIELD_CREATED_BY = EObjectFields.OBJECT_END + 0x0,                      // 0x006 - Size: 2 - Type: GUID - Flags: PUBLIC
    GAMEOBJECT_DISPLAYID = EObjectFields.OBJECT_END + 0x2,                         // 0x008 - Size: 1 - Type: INT - Flags: PUBLIC
    GAMEOBJECT_FLAGS = EObjectFields.OBJECT_END + 0x3,                             // 0x009 - Size: 1 - Type: INT - Flags: PUBLIC
    GAMEOBJECT_ROTATION = EObjectFields.OBJECT_END + 0x4,                          // 0x00A - Size: 4 - Type: FLOAT - Flags: PUBLIC
    GAMEOBJECT_STATE = EObjectFields.OBJECT_END + 0x8,                             // 0x00E - Size: 1 - Type: INT - Flags: PUBLIC

    // GAMEOBJECT_TIMESTAMP = EObjectFields.OBJECT_END + &H9                         ' 0x00F - Size: 1 - Type: INT - Flags: PUBLIC
    GAMEOBJECT_POS_X = EObjectFields.OBJECT_END + 0x9,                             // 0x010 - Size: 1 - Type: FLOAT - Flags: PUBLIC

    GAMEOBJECT_POS_Y = EObjectFields.OBJECT_END + 0xA,                             // 0x011 - Size: 1 - Type: FLOAT - Flags: PUBLIC
    GAMEOBJECT_POS_Z = EObjectFields.OBJECT_END + 0xB,                             // 0x012 - Size: 1 - Type: FLOAT - Flags: PUBLIC
    GAMEOBJECT_FACING = EObjectFields.OBJECT_END + 0xC,                            // 0x013 - Size: 1 - Type: FLOAT - Flags: PUBLIC
    GAMEOBJECT_DYN_FLAGS = EObjectFields.OBJECT_END + 0xD,                         // 0x014 - Size: 1 - Type: INT - Flags: DYNAMIC
    GAMEOBJECT_FACTION = EObjectFields.OBJECT_END + 0xE,                           // 0x015 - Size: 1 - Type: INT - Flags: PUBLIC
    GAMEOBJECT_TYPE_ID = EObjectFields.OBJECT_END + 0xF,                          // 0x016 - Size: 1 - Type: INT - Flags: PUBLIC
    GAMEOBJECT_LEVEL = EObjectFields.OBJECT_END + 0x10,                            // 0x017 - Size: 1 - Type: INT - Flags: PUBLIC
    GAMEOBJECT_ARTKIT = EObjectFields.OBJECT_END + 0x11,
    GAMEOBJECT_ANIMPROGRESS = EObjectFields.OBJECT_END + 0x12,
    GAMEOBJECT_PADDING = EObjectFields.OBJECT_END + 0x13,
    GAMEOBJECT_END = EObjectFields.OBJECT_END + 0x14                              // 0x018
}
