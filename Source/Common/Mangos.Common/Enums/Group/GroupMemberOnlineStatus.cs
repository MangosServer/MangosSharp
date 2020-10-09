using System;

namespace Mangos.Common.Enums.Group
{
    [Flags]
    public enum GroupMemberOnlineStatus
    {
        MEMBER_STATUS_OFFLINE = 0x0,
        MEMBER_STATUS_ONLINE = 0x1,
        MEMBER_STATUS_PVP = 0x2,
        MEMBER_STATUS_DEAD = 0x4,            // dead (health=0)
        MEMBER_STATUS_GHOST = 0x8,           // ghost (health=1)
        MEMBER_STATUS_PVP_FFA = 0x10,        // pvp ffa
        MEMBER_STATUS_UNK3 = 0x20,           // unknown
        MEMBER_STATUS_AFK = 0x40,            // afk flag
        MEMBER_STATUS_DND = 0x80            // dnd flag
    }
}