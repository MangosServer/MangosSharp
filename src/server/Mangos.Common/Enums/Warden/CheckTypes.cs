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

namespace Mangos.Common.Enums.Warden;

public enum CheckTypes : byte
{
    MEM_CHECK = 0,
    PAGE_CHECK_A_B = 1,
    MPQ_CHECK = 2,
    LUA_STR_CHECK = 3,
    DRIVER_CHECK = 4,
    TIMING_CHECK = 5,
    PROC_CHECK = 6,
    MODULE_CHECK = 7
}
