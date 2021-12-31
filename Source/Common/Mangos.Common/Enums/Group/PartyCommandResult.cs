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

namespace Mangos.Common.Enums.Group;

public enum PartyCommandResult : byte
{
    INVITE_OK = 0,                   // You have invited [name] to join your group.
    INVITE_NOT_FOUND = 1,            // Cannot find [name].
    INVITE_NOT_IN_YOUR_PARTY = 2,    // [name] is not in your party.
    INVITE_NOT_IN_YOUR_INSTANCE = 3, // [name] is not in your instance.
    INVITE_PARTY_FULL = 4,           // Your party is full.
    INVITE_ALREADY_IN_GROUP = 5,     // [name] is already in group.
    INVITE_NOT_IN_PARTY = 6,         // You aren't in party.
    INVITE_NOT_LEADER = 7,           // You are not the party leader.
    INVITE_NOT_SAME_SIDE = 8,        // gms - Target is not part of your alliance.
    INVITE_IGNORED = 9,              // [name] is ignoring you.
    INVITE_RESTRICTED = 13
}
