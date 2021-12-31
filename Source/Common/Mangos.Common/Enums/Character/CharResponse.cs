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

namespace Mangos.Common.Enums.Character;

public enum CharResponse : byte
{
    CHAR_LIST_FAILED = 0x2C,
    CHAR_CREATE_SUCCESS = 0x2E,
    CHAR_CREATE_ERROR = 0x2F,
    CHAR_CREATE_FAILED = 0x30,
    CHAR_CREATE_NAME_IN_USE = 0x31,
    CHAR_CREATE_DISABLED = 0x32,
    CHAR_CREATE_PVP_TEAMS_VIOLATION = 0x33,
    CHAR_CREATE_SERVER_LIMIT = 0x34,
    CHAR_CREATE_ACCOUNT_LIMIT = 0x35,
    CHAR_DELETE_SUCCESS = 0x39,
    CHAR_DELETE_FAILED = 0x3A,
    CHAR_LOGIN_NO_WORLD = 0x3D,
    CHAR_LOGIN_FAILED = 0x40,
    CHAR_NAME_INVALID_CHARACTER = 0x46
}
