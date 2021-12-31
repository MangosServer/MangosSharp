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

namespace Mangos.Common.Enums.Spell;

public enum SpellInterruptFlags
{
    SPELL_INTERRUPT_FLAG_MOVEMENT = 0x1, // why need this for instant?
    SPELL_INTERRUPT_FLAG_PUSH_BACK = 0x2, // push back
    SPELL_INTERRUPT_FLAG_INTERRUPT = 0x4, // interrupt
    SPELL_INTERRUPT_FLAG_AUTOATTACK = 0x8, // no
    SPELL_INTERRUPT_FLAG_DAMAGE = 0x10  // _complete_ interrupt on direct damage?
}
