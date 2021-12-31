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

public enum ObjectUpdateType
{
    UPDATETYPE_VALUES = 0,

    // 1 byte  - MASK
    // 8 bytes - GUID
    // Goto Update Block
    UPDATETYPE_MOVEMENT = 1,

    // 1 byte  - MASK
    // 8 bytes - GUID
    // Goto Position Update
    UPDATETYPE_CREATE_OBJECT = 2,

    UPDATETYPE_CREATE_OBJECT_SELF = 3,

    // 1 byte  - MASK
    // 8 bytes - GUID
    // 1 byte - Object Type (*)
    // Goto Position Update
    // Goto Update Block
    UPDATETYPE_OUT_OF_RANGE_OBJECTS = 4,

    // 4 bytes - Count
    // Loop Count Times:
    // 1 byte  - MASK
    // 8 bytes - GUID
    UPDATETYPE_NEAR_OBJECTS = 5 // looks like 4 & 5 do the same thing

    // 4 bytes - Count
    // Loop Count Times:
    // 1 byte  - MASK
    // 8 bytes - GUID
}
