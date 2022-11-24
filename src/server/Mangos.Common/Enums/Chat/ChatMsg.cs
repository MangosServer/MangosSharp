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

namespace Mangos.Common.Enums.Chat;

public enum ChatMsg
{
    CHAT_MSG_SAY = 0x0,
    CHAT_MSG_PARTY = 0x1,
    CHAT_MSG_RAID = 0x2,
    CHAT_MSG_GUILD = 0x3,
    CHAT_MSG_OFFICER = 0x4,
    CHAT_MSG_YELL = 0x5,
    CHAT_MSG_WHISPER = 0x6,
    CHAT_MSG_WHISPER_INFORM = 0x7,
    CHAT_MSG_EMOTE = 0x8,
    CHAT_MSG_TEXT_EMOTE = 0x9,
    CHAT_MSG_SYSTEM = 0xA,
    CHAT_MSG_MONSTER_SAY = 0xB,
    CHAT_MSG_MONSTER_YELL = 0xC,
    CHAT_MSG_MONSTER_EMOTE = 0xD,
    CHAT_MSG_CHANNEL = 0xE,
    CHAT_MSG_CHANNEL_JOIN = 0xF,
    CHAT_MSG_CHANNEL_LEAVE = 0x10,
    CHAT_MSG_CHANNEL_LIST = 0x11,
    CHAT_MSG_CHANNEL_NOTICE = 0x12,
    CHAT_MSG_CHANNEL_NOTICE_USER = 0x13,
    CHAT_MSG_AFK = 0x14,
    CHAT_MSG_DND = 0x15,
    CHAT_MSG_IGNORED = 0x16,
    CHAT_MSG_SKILL = 0x17,
    CHAT_MSG_LOOT = 0x18,
    CHAT_MSG_RAID_LEADER = 0x57,
    CHAT_MSG_RAID_WARNING = 0x58
}
