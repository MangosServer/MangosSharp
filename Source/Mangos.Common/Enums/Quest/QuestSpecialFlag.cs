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

namespace Mangos.Common.Enums.Quest;

[Flags]
public enum QuestSpecialFlag
{
    QUEST_SPECIALFLAGS_NONE = 0,
    QUEST_SPECIALFLAGS_DELIVER = 1,
    QUEST_SPECIALFLAGS_EXPLORE = 2,
    QUEST_SPECIALFLAGS_SPEAKTO = 4,
    QUEST_SPECIALFLAGS_KILLORCAST = 8,
    QUEST_SPECIALFLAGS_TIMED = 16,

    // 32 is unknown
    QUEST_SPECIALFLAGS_REPUTATION = 64
}
