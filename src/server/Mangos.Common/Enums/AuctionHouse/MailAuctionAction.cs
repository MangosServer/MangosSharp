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

namespace Mangos.Common.Enums.AuctionHouse;

// Auction Mail Format:
//
// Outbid
// Subject -> ItemID:0:0
// Body    -> ""
// Money returned
// Auction won
// Subject -> ItemID:0:1
// Body    -> FFFFFFFF:Bid:Buyout
// Item received
// Auction Successful
// Subject -> ItemID:0:2
// Body    -> FFFFFFFF:Bid:Buyout:0:0
// Money received
// Auction Canceled
// Subject -> ItemID:0:4
// Body    -> ""
// Item returned
public enum MailAuctionAction
{
    OUTBID = 0,
    AUCTION_WON = 1,
    AUCTION_SUCCESSFUL = 2,
    AUCTION_CANCELED = 3
}
