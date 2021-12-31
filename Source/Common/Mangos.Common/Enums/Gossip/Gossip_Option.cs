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

namespace Mangos.Common.Enums.Gossip;

public enum Gossip_Option
{
    GOSSIP_OPTION_NONE = 0,                                 // UNIT_NPC_FLAG_NONE              = 0
    GOSSIP_OPTION_GOSSIP = 1,                               // UNIT_NPC_FLAG_GOSSIP            = 1
    GOSSIP_OPTION_QUESTGIVER = 2,                           // UNIT_NPC_FLAG_QUESTGIVER        = 2
    GOSSIP_OPTION_VENDOR = 3,                               // UNIT_NPC_FLAG_VENDOR            = 4
    GOSSIP_OPTION_TAXIVENDOR = 4,                           // UNIT_NPC_FLAG_FLIGHTMASTER      = 8
    GOSSIP_OPTION_TRAINER = 5,                              // UNIT_NPC_FLAG_TRAINER           = 16
    GOSSIP_OPTION_SPIRITHEALER = 6,                         // UNIT_NPC_FLAG_SPIRITHEALER      = 32
    GOSSIP_OPTION_GUARD = 7,                                // UNIT_NPC_FLAG_GUARD		        = 64
    GOSSIP_OPTION_INNKEEPER = 8,                            // UNIT_NPC_FLAG_INNKEEPER         = 128
    GOSSIP_OPTION_BANKER = 9,                               // UNIT_NPC_FLAG_BANKER            = 256
    GOSSIP_OPTION_ARENACHARTER = 10,                        // UNIT_NPC_FLAG_ARENACHARTER      = 262144
    GOSSIP_OPTION_TABARDVENDOR = 11,                        // UNIT_NPC_FLAG_TABARDVENDOR      = 1024
    GOSSIP_OPTION_BATTLEFIELD = 12,                         // UNIT_NPC_FLAG_BATTLEFIELDPERSON = 2048
    GOSSIP_OPTION_AUCTIONEER = 13,                          // UNIT_NPC_FLAG_AUCTIONEER        = 4096
    GOSSIP_OPTION_STABLEPET = 14,                           // UNIT_NPC_FLAG_STABLE            = 8192
    GOSSIP_OPTION_ARMORER = 15,                             // UNIT_NPC_FLAG_REPAIR            = 16384
    GOSSIP_OPTION_TALENTWIPE = 16
}
