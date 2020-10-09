using System;

namespace Mangos.Common.Enums.Channel
{
    [Flags]
    public enum CHANNEL_USER_FLAG : byte
    {
        CHANNEL_FLAG_NONE = 0x0,
        CHANNEL_FLAG_OWNER = 0x1,
        CHANNEL_FLAG_MODERATOR = 0x2,
        CHANNEL_FLAG_MUTED = 0x4,
        CHANNEL_FLAG_CUSTOM = 0x10
    }
}