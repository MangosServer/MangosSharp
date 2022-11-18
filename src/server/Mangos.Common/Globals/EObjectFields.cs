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

public enum EObjectFields
{
    OBJECT_FIELD_GUID = 0x0,                                                       // 0x000 - Size: 2 - Type: GUID - Flags: PUBLIC
    OBJECT_FIELD_TYPE = 0x2,                                                       // 0x002 - Size: 1 - Type: INT - Flags: PUBLIC
    OBJECT_FIELD_ENTRY = 0x3,                                                      // 0x003 - Size: 1 - Type: INT - Flags: PUBLIC
    OBJECT_FIELD_SCALE_X = 0x4,                                                    // 0x004 - Size: 1 - Type: FLOAT - Flags: PUBLIC
    OBJECT_FIELD_PADDING = 0x5,                                                    // 0x005 - Size: 1 - Type: INT - Flags: NONE
    OBJECT_END = 0x6
}
