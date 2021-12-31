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

public enum QuestPartyPushError : byte
{
    QUEST_PARTY_MSG_SHARRING_QUEST = 0,
    QUEST_PARTY_MSG_CANT_TAKE_QUEST = 1,
    QUEST_PARTY_MSG_ACCEPT_QUEST = 2,
    QUEST_PARTY_MSG_REFUSE_QUEST = 3,
    QUEST_PARTY_MSG_TO_FAR = 4,
    QUEST_PARTY_MSG_BUSY = 5,
    QUEST_PARTY_MSG_LOG_FULL = 6,
    QUEST_PARTY_MSG_HAVE_QUEST = 7,
    QUEST_PARTY_MSG_FINISH_QUEST = 8
}
