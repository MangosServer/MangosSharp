using System;

namespace Mangos.Common.Enums.Chat
{
    [Flags]
    public enum ChatFlag : byte
    {
        FLAGS_NONE = 0,
        FLAGS_AFK = 1,
        FLAGS_DND = 2,
        FLAGS_GM = 3
    }
}