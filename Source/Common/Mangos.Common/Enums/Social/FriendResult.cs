
namespace Mangos.Common.Enums.Social
{
    public enum FriendResult : byte
    {
        FRIEND_DB_ERROR = 0x0,
        FRIEND_LIST_FULL = 0x1,
        FRIEND_ONLINE = 0x2,
        FRIEND_OFFLINE = 0x3,
        FRIEND_NOT_FOUND = 0x4,
        FRIEND_REMOVED = 0x5,
        FRIEND_ADDED_ONLINE = 0x6,
        FRIEND_ADDED_OFFLINE = 0x7,
        FRIEND_ALREADY = 0x8,
        FRIEND_SELF = 0x9,
        FRIEND_ENEMY = 0xA,
        FRIEND_IGNORE_FULL = 0xB,
        FRIEND_IGNORE_SELF = 0xC,
        FRIEND_IGNORE_NOT_FOUND = 0xD,
        FRIEND_IGNORE_ALREADY = 0xE,
        FRIEND_IGNORE_ADDED = 0xF,
        FRIEND_IGNORE_REMOVED = 0x10
    }
}