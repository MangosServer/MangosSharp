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

namespace Mangos.Common.Enums.Map;

public enum AreaFlag
{
    AREA_FLAG_SNOW = 0x1,                // snow (only Dun Morogh, Naxxramas, Razorfen Downs and Winterspring)
    AREA_FLAG_UNK1 = 0x2,                // unknown, (only Naxxramas and Razorfen Downs)
    AREA_FLAG_UNK2 = 0x4,                // Only used on development map
    AREA_FLAG_SLAVE_CAPITAL = 0x8,       // slave capital city flag?
    AREA_FLAG_UNK3 = 0x10,               // unknown
    AREA_FLAG_SLAVE_CAPITAL2 = 0x20,     // slave capital city flag?
    AREA_FLAG_UNK4 = 0x40,               // many zones have this flag
    AREA_FLAG_ARENA = 0x80,              // arena, both instanced and world arenas
    AREA_FLAG_CAPITAL = 0x100,           // main capital city flag
    AREA_FLAG_CITY = 0x200,              // only for one zone named "City" (where it located?)
    AREA_FLAG_SANCTUARY = 0x800,         // sanctuary area (PvP disabled)
    AREA_FLAG_NEED_FLY = 0x1000,         // only Netherwing Ledge, Socrethar's Seat, Tempest Keep, The Arcatraz, The Botanica, The Mechanar, Sorrow Wing Point, Dragonspine Ridge, Netherwing Mines, Dragonmaw Base Camp, Dragonmaw Skyway
    AREA_FLAG_UNUSED1 = 0x2000,          // not used now (no area/zones with this flag set in 2.4.2)
    AREA_FLAG_PVP = 0x8000,              // pvp objective area? (Death's Door also has this flag although it's no pvp object area)
    AREA_FLAG_ARENA_INSTANCE = 0x10000,  // used by instanced arenas only
    AREA_FLAG_UNUSED2 = 0x20000,         // not used now (no area/zones with this flag set in 2.4.2)
    AREA_FLAG_UNK5 = 0x40000,            // just used for Amani Pass, Hatchet Hills
    AREA_FLAG_LOWLEVEL = 0x100000       // used for some starting areas with area_level <=15
}
