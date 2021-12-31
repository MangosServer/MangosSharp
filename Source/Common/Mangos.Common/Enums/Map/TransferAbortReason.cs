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

public enum TransferAbortReason : short
{
    TRANSFER_ABORT_MAX_PLAYERS = 0x1,                // Transfer Aborted: instance is full
    TRANSFER_ABORT_NOT_FOUND = 0x2,                  // Transfer Aborted: instance not found
    TRANSFER_ABORT_TOO_MANY_INSTANCES = 0x3,         // You have entered too many instances recently.
    TRANSFER_ABORT_ZONE_IN_COMBAT = 0x5,             // Unable to zone in while an encounter is in progress.
    TRANSFER_ABORT_INSUF_EXPAN_LVL1 = 0x106         // You must have TBC expansion installed to access this area.
}
