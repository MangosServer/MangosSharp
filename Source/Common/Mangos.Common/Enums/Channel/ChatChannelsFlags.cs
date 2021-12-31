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
public enum ChatChannelsFlags
{
    FLAG_NONE = 0x0,
    FLAG_INITIAL = 0x1,              // General, Trade, LocalDefense, LFG
    FLAG_ZONE_DEP = 0x2,             // General, Trade, LocalDefense, GuildRecruitment
    FLAG_GLOBAL = 0x4,               // WorldDefense
    FLAG_TRADE = 0x8,                // Trade
    FLAG_CITY_ONLY = 0x10,           // Trade, GuildRecruitment
    FLAG_CITY_ONLY2 = 0x20,          // Trade, GuildRecruitment
    FLAG_DEFENSE = 0x10000,          // LocalDefense, WorldDefense
    FLAG_GUILD_REQ = 0x20000,        // GuildRecruitment
    FLAG_LFG = 0x40000              // LookingForGroup
}
