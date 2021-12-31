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

namespace Mangos.Common.Enums.Global;

public enum AuraFlags
{
    AFLAG_NONE = 0x0,
    AFLAG_VISIBLE = 0x1,
    AFLAG_EFF_INDEX_1 = 0x2,
    AFLAG_EFF_INDEX_2 = 0x4,
    AFLAG_NOT_GUID = 0x8,
    AFLAG_CANCELLABLE = 0x10,
    AFLAG_HAS_DURATION = 0x20,
    AFLAG_UNK2 = 0x40,
    AFLAG_NEGATIVE = 0x80,
    AFLAG_POSITIVE = 0x1F,
    AFLAG_MASK = 0xFF
}
