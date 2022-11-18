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

public enum EContainerFields
{
    CONTAINER_FIELD_NUM_SLOTS = EItemFields.ITEM_END + 0x0,                        // 0x02A - Size: 1 - Type: INT - Flags: PUBLIC
    CONTAINER_ALIGN_PAD = EItemFields.ITEM_END + 0x1,                              // 0x02B - Size: 1 - Type: BYTES - Flags: NONE
    CONTAINER_FIELD_SLOT_1 = EItemFields.ITEM_END + 0x2,                           // 0x02C - Size: 72 - Type: GUID - Flags: PUBLIC
    CONTAINER_END = EItemFields.ITEM_END + 0x3A                                   // 0x074
}
