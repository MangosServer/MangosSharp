
namespace Mangos.Common.Enums.AuctionHouse
{

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
    public enum MailAuctionAction : int
    {
        OUTBID = 0,
        AUCTION_WON = 1,
        AUCTION_SUCCESSFUL = 2,
        AUCTION_CANCELED = 3
    }
}