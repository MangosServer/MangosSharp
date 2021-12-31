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

public enum QuestInvalidError
{
    // SMSG_QUESTGIVER_QUEST_INVALID
    // uint32 invalidReason

    INVALIDREASON_DONT_HAVE_REQ = 0,                     // You don't meet the requirements for that quest
    INVALIDREASON_DONT_HAVE_LEVEL = 1,                   // You are not high enough level for that quest.
    INVALIDREASON_DONT_HAVE_RACE = 6,                    // That quest is not available to your race
    INVALIDREASON_COMPLETED_QUEST = 7,                   // You have already completed this quest
    INVALIDREASON_HAVE_TIMED_QUEST = 12,                 // You can only be on one timed quest at a time
    INVALIDREASON_HAVE_QUEST = 13,                       // You are already on that quest
    INVALIDREASON_DONT_HAVE_EXP_ACCOUNT = 16,            // ??????
    INVALIDREASON_DONT_HAVE_REQ_ITEMS = 21,  // Changed for 2.1.3  'You don't have the required items with you. Check storage.
    INVALIDREASON_DONT_HAVE_REQ_MONEY = 23,              // You don't have enough money for that quest
    INVALIDREASON_REACHED_DAILY_LIMIT = 26,              // You have completed xx daily quests today
    INVALIDREASON_UNKNOW27 = 27                         // You can not complete quests once you have reached tired time ???
}
