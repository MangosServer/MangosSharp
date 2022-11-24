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

namespace Mangos.Common.Enums.Guild;

public enum GuildEvent : byte
{
    PROMOTION = 0,           // uint8(2), string(name), string(rankName)
    DEMOTION = 1,            // uint8(2), string(name), string(rankName)
    MOTD = 2,                // uint8(1), string(text)                                             'Guild message of the day: <text>
    JOINED = 3,              // uint8(1), string(name)                                             '<name> has joined the guild.
    LEFT = 4,                // uint8(1), string(name)                                             '<name> has left the guild.
    REMOVED = 5,             // ??
    LEADER_IS = 6,           // uint8(1), string(name                                              '<name> is the leader of your guild.
    LEADER_CHANGED = 7,      // uint8(2), string(oldLeaderName), string(newLeaderName)
    DISBANDED = 8,           // uint8(0)                                                           'Your guild has been disbanded.
    TABARDCHANGE = 9,        // ??
    SIGNED_ON = 12,
    SIGNED_OFF = 13
}
