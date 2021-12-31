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

namespace Mangos.Common.Enums.Channel;

[Flags]
public enum CHANNEL_FLAG : byte
{
    // General                  0x18 = 0x10 | 0x08
    // Trade                    0x3C = 0x20 | 0x10 | 0x08 | 0x04
    // LocalDefence             0x18 = 0x10 | 0x08
    // GuildRecruitment         0x38 = 0x20 | 0x10 | 0x08
    // LookingForGroup          0x50 = 0x40 | 0x10

    CHANNEL_FLAG_NONE = 0x0,
    CHANNEL_FLAG_CUSTOM = 0x1,
    CHANNEL_FLAG_UNK1 = 0x2,
    CHANNEL_FLAG_TRADE = 0x4,
    CHANNEL_FLAG_NOT_LFG = 0x8,
    CHANNEL_FLAG_GENERAL = 0x10,
    CHANNEL_FLAG_CITY = 0x20,
    CHANNEL_FLAG_LFG = 0x40
}
