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

namespace Mangos.Common.Enums.Channel;

public enum CHANNEL_NOTIFY_FLAGS
{
    CHANNEL_JOINED = 0,                      // %s joined channel.
    CHANNEL_LEFT = 1,                        // %s left channel.
    CHANNEL_YOU_JOINED = 2,                  // Joined Channel: [%s]
    CHANNEL_YOU_LEFT = 3,                    // Left Channel: [%s]
    CHANNEL_WRONG_PASS = 4,                  // Wrong password for %s.
    CHANNEL_NOT_ON = 5,                      // Not on channel %s.
    CHANNEL_NOT_MODERATOR = 6,               // Not a moderator of %s.
    CHANNEL_SET_PASSWORD = 7,                // [%s] Password changed by %s.
    CHANNEL_CHANGE_OWNER = 8,                // [%s] Owner changed to %s.
    CHANNEL_NOT_ON_FOR_NAME = 9,             // [%s] Player %s was not found.
    CHANNEL_NOT_OWNER = 0xA,                 // [%s] You are not the channel owner.
    CHANNEL_WHO_OWNER = 0xB,                 // [%s] Channel owner is %s.
    CHANNEL_MODE_CHANGE = 0xC,               //
    CHANNEL_ENABLE_ANNOUNCE = 0xD,           // [%s] Channel announcements enabled by %s.
    CHANNEL_DISABLE_ANNOUNCE = 0xE,          // [%s] Channel announcements disabled by %s.
    CHANNEL_MODERATED = 0xF,                 // [%s] Channel moderation enabled by %s.
    CHANNEL_UNMODERATED = 0x10,              // [%s] Channel moderation disabled by %s.
    CHANNEL_YOUCANTSPEAK = 0x11,             // [%s] You do not have permission to speak.
    CHANNEL_KICKED = 0x12,                   // [%s] Player %s kicked by %s.
    CHANNEL_YOU_ARE_BANNED = 0x13,           // [%s] You are banned from that channel.
    CHANNEL_BANNED = 0x14,                   // [%s] Player %s banned by %s.
    CHANNEL_UNBANNED = 0x15,                 // [%s] Player %s unbanned by %s.
    CHANNEL_NOT_BANNED = 0x16,               // [%s] Player %s is not banned.
    CHANNEL_ALREADY_ON = 0x17,               // [%s] Player %s is already on the channel.
    CHANNEL_INVITED = 0x18,                  // %s has invited you to join the channel '%s'
    CHANNEL_INVITED_WRONG_FACTION = 0x19,    // Target is in the wrong alliance for %s.
    CHANNEL_WRONG_FACTION = 0x1A,            // Wrong alliance for %s.
    CHANNEL_INVALID_NAME = 0x1B,             // Invalid channel name
    CHANNEL_NOT_MODERATED = 0x1C,            // %s is not moderated
    CHANNEL_PLAYER_INVITED = 0x1D,           // [%s] You invited %s to join the channel
    CHANNEL_PLAYER_INVITE_BANNED = 0x1E,     // [%s] %s has been banned.
    CHANNEL_THROTTLED = 0x1F,                // [%s] The number of messages that can be sent to this channel is limited, please wait to send another message.
    CHANNEL_NOT_IN_AREA = 0x20,              // [%s] You are not in the correct area for this channel.
    CHANNEL_NOT_IN_LFG = 0x21               // [%s] You must be queued in looking for group before joining this channel.
}
