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

namespace Mangos.Common.Enums.Quest;

public enum QuestFailedReason
{
    // SMSG_QUESTGIVER_QUEST_FAILED
    // uint32 questID
    // uint32 failedReason

    FAILED_INVENTORY_FULL = 4,       // 0x04: '%s failed: Inventory is full.'
    FAILED_DUPE_ITEM = 0x11,         // 0x11: '%s failed: Duplicate item found.'
    FAILED_INVENTORY_FULL2 = 0x31,   // 0x31: '%s failed: Inventory is full.'
    FAILED_NOREASON = 0       // 0x00: '%s failed.'
}
