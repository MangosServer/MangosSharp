using System;

namespace Mangos.Common.Enums.Item
{
    [Flags]
    public enum ITEM_FLAGS : int
    {
        ITEM_FLAGS_BINDED = 0x1,
        ITEM_FLAGS_CONJURED = 0x2,
        ITEM_FLAGS_OPENABLE = 0x4,
        ITEM_FLAGS_WRAPPED = 0x8,
        ITEM_FLAGS_WRAPPER = 0x200, // used or not used wrapper
        ITEM_FLAGS_PARTY_LOOT = 0x800, // determines if item is party loot or not
        ITEM_FLAGS_CHARTER = 0x2000, // arena/guild charter
        ITEM_FLAGS_THROWABLE = 0x400000, // not used in game for check trow possibility, only for item in game tooltip
        ITEM_FLAGS_SPECIALUSE = 0x800000
    }
}