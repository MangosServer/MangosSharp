
using System;

namespace Mangos.Common.Enums.Social
{
    [Flags]
    public enum SocialFlag : byte
    {
        SOCIAL_FLAG_FRIEND = 0x1,
        SOCIAL_FLAG_IGNORED = 0x2
    }
}