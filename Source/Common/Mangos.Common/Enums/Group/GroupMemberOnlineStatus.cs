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

namespace Mangos.Common.Enums.Group;

[Flags]
public enum GroupMemberOnlineStatus
{
    MEMBER_STATUS_OFFLINE = 0x0,
    MEMBER_STATUS_ONLINE = 0x1,
    MEMBER_STATUS_PVP = 0x2,
    MEMBER_STATUS_DEAD = 0x4,            // dead (health=0)
    MEMBER_STATUS_GHOST = 0x8,           // ghost (health=1)
    MEMBER_STATUS_PVP_FFA = 0x10,        // pvp ffa
    MEMBER_STATUS_UNK3 = 0x20,           // unknown
    MEMBER_STATUS_AFK = 0x40,            // afk flag
    MEMBER_STATUS_DND = 0x80            // dnd flag
}
