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

namespace Mangos.Common.Globals;

public enum ObjectType
{
    TYPE_OBJECT = 1,
    TYPE_ITEM = 2,
    TYPE_CONTAINER = 6,
    TYPE_UNIT = 8,
    TYPE_PLAYER = 16,
    TYPE_GAMEOBJECT = 32,
    TYPE_DYNAMICOBJECT = 64,
    TYPE_CORPSE = 128,
    TYPE_AIGROUP = 256,
    TYPE_AREATRIGGER = 512
}
