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

namespace Mangos.Common.Enums.GameObject;

[Flags]
public enum GameObjectFlags : byte
{
    GO_FLAG_IN_USE = 0x1,                        // disables interaction while animated
    GO_FLAG_LOCKED = 0x2,                        // require key, spell, event, etc to be opened. Makes "Locked" appear in tooltip
    GO_FLAG_INTERACT_COND = 0x4,                 // cannot interact (condition to interact)
    GO_FLAG_TRANSPORT = 0x8,                     // any kind of transport? Object can transport (elevator, boat, car)
    GO_FLAG_UNK1 = 0x10,                         //
    GO_FLAG_NODESPAWN = 0x20,                    // never despawn, typically for doors, they just change state
    GO_FLAG_TRIGGERED = 0x40                    // typically, summoned objects. Triggered by spell or other events
}
