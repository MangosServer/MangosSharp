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

public enum QuestgiverStatusFlag
{
    DIALOG_STATUS_NONE = 0,                  // There aren't any quests available. - No Mark
    DIALOG_STATUS_UNAVAILABLE = 1,           // Quest available and your leve isn't enough. - Gray Quotation ! Mark
    DIALOG_STATUS_CHAT = 2,                  // Quest available it shows a talk baloon. - No Mark
    DIALOG_STATUS_INCOMPLETE = 3,            // Quest isn't finished yet. - Gray Question ? Mark
    DIALOG_STATUS_REWARD_REP = 4,            // Rewards rep? :P
    DIALOG_STATUS_AVAILABLE = 5,             // Quest available, and your level is enough. - Yellow Quotation ! Mark
    DIALOG_STATUS_REWARD = 6                // Quest has been finished. - Yellow dot on the minimap
}
