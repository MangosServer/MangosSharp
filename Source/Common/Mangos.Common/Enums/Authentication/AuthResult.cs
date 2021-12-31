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

namespace Mangos.Common.Enums.Authentication;

public enum AuthResult : byte
{
    WOW_SUCCESS = 0x0,
    WOW_FAIL_BANNED = 0x3,
    WOW_FAIL_UNKNOWN_ACCOUNT = 0x4,
    WOW_FAIL_INCORRECT_PASSWORD = 0x5,
    WOW_FAIL_ALREADY_ONLINE = 0x6,
    WOW_FAIL_NO_TIME = 0x7,
    WOW_FAIL_DB_BUSY = 0x8,
    WOW_FAIL_VERSION_INVALID = 0x9,
    WOW_FAIL_VERSION_UPDATE = 0xA,
    WOW_FAIL_INVALID_SERVER = 0xB,
    WOW_FAIL_SUSPENDED = 0xC,
    WOW_FAIL_FAIL_NOACCESS = 0xD,
    WOW_SUCCESS_SURVEY = 0xE,
    WOW_FAIL_PARENTCONTROL = 0xF,
    WOW_FAIL_LOCKED_ENFORCED = 0x10,
    WOW_FAIL_TRIAL_ENDED = 0x11,
    WOW_FAIL_ANTI_INDULGENCE = 0x13,
    WOW_FAIL_EXPIRED = 0x14,
    WOW_FAIL_NO_GAME_ACCOUNT = 0x15,
    WOW_FAIL_CHARGEBACK = 0x16,
    WOW_FAIL_GAME_ACCOUNT_LOCKED = 0x18,
    WOW_FAIL_UNLOCKABLE_LOCK = 0x19,
    WOW_FAIL_CONVERSION_REQUIRED = 0x20,
    WOW_FAIL_DISCONNECTED = 0xFF
}
