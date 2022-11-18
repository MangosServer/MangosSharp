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

public enum MovementFlags
{
    MOVEMENTFLAG_NONE = 0x0,
    MOVEMENTFLAG_FORWARD = 0x1,
    MOVEMENTFLAG_BACKWARD = 0x2,
    MOVEMENTFLAG_STRAFE_LEFT = 0x4,
    MOVEMENTFLAG_STRAFE_RIGHT = 0x8,
    MOVEMENTFLAG_LEFT = 0x10,
    MOVEMENTFLAG_RIGHT = 0x20,
    MOVEMENTFLAG_PITCH_UP = 0x40,
    MOVEMENTFLAG_PITCH_DOWN = 0x80,
    MOVEMENTFLAG_WALK = 0x100,
    MOVEMENTFLAG_JUMPING = 0x2000,
    MOVEMENTFLAG_FALLING = 0x4000,
    MOVEMENTFLAG_SWIMMING = 0x200000,
    MOVEMENTFLAG_ONTRANSPORT = 0x2000000,
    MOVEMENTFLAG_SPLINE = 0x4000000
}
