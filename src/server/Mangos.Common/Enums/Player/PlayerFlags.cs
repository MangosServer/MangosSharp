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

namespace Mangos.Common.Enums.Player;

[Flags]
public enum PlayerFlags
{
    PLAYER_FLAGS_GROUP_LEADER = 0x1,
    PLAYER_FLAGS_AFK = 0x2,
    PLAYER_FLAGS_DND = 0x4,
    PLAYER_FLAGS_GM = 0x8,                        // GM Prefix
    PLAYER_FLAGS_DEAD = 0x10,
    PLAYER_FLAGS_RESTING = 0x20,
    PLAYER_FLAGS_UNK7 = 0x40,                    // Admin Prefix?
    PLAYER_FLAGS_FFA_PVP = 0x80,
    PLAYER_FLAGS_CONTESTED_PVP = 0x100,
    PLAYER_FLAGS_IN_PVP = 0x200,
    PLAYER_FLAGS_HIDE_HELM = 0x400,
    PLAYER_FLAGS_HIDE_CLOAK = 0x800,
    PLAYER_FLAGS_PARTIAL_PLAY_TIME = 0x1000,
    PLAYER_FLAGS_IS_OUT_OF_BOUNDS = 0x4000,      // Out of Bounds
    PLAYER_FLAGS_UNK15 = 0x8000,                 // Dev Prefix?
    PLAYER_FLAGS_SANCTUARY = 0x10000,
    PLAYER_FLAGS_NO_PLAY_TIME = 0x2000,
    PLAYER_FLAGS_PVP_TIMER = 0x40000
}
