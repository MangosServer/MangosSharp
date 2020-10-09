
namespace Mangos.Common.Enums.Misc
{
    public enum BUY_ERROR : byte
    {
        // SMSG_BUY_FAILED error
        // 0: cant find item
        // 1: item already selled
        // 2: not enought money
        // 4: seller(dont Like u)
        // 5: distance too far
        // 8: cant carry more
        // 11: level(require)
        // 12: reputation(require)

        BUY_ERR_CANT_FIND_ITEM = 0,
        BUY_ERR_ITEM_ALREADY_SOLD = 1,
        BUY_ERR_NOT_ENOUGHT_MONEY = 2,
        BUY_ERR_SELLER_DONT_LIKE_YOU = 4,
        BUY_ERR_DISTANCE_TOO_FAR = 5,
        BUY_ERR_CANT_CARRY_MORE = 8,
        BUY_ERR_LEVEL_REQUIRE = 11,
        BUY_ERR_REPUTATION_REQUIRE = 12
    }
}