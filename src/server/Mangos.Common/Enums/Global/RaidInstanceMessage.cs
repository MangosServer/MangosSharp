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

public enum RaidInstanceMessage : uint
{
    RAID_INSTANCE_WARNING_HOURS = 1U,         // WARNING! %s is scheduled to reset in %d hour(s).
    RAID_INSTANCE_WARNING_MIN = 2U,           // WARNING! %s is scheduled to reset in %d minute(s)!
    RAID_INSTANCE_WARNING_MIN_SOON = 3U,      // WARNING! %s is scheduled to reset in %d minute(s). Please exit the zone or you will be returned to your bind location!
    RAID_INSTANCE_WELCOME = 4U               // Welcome to %s. This raid instance is scheduled to reset in %s.
}
