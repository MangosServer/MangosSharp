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

namespace Mangos.Common.Enums.Guild;

public enum GuildRankRights
{
    GR_RIGHT_EMPTY = 0x40,
    GR_RIGHT_GCHATLISTEN = 0x41,
    GR_RIGHT_GCHATSPEAK = 0x42,
    GR_RIGHT_OFFCHATLISTEN = 0x44,
    GR_RIGHT_OFFCHATSPEAK = 0x48,
    GR_RIGHT_PROMOTE = 0xC0,
    GR_RIGHT_DEMOTE = 0x140,
    GR_RIGHT_INVITE = 0x50,
    GR_RIGHT_REMOVE = 0x60,
    GR_RIGHT_SETMOTD = 0x1040,
    GR_RIGHT_EPNOTE = 0x2040,
    GR_RIGHT_VIEWOFFNOTE = 0x4040,
    GR_RIGHT_EOFFNOTE = 0x8040,
    GR_RIGHT_ALL = 0xF1FF
}
