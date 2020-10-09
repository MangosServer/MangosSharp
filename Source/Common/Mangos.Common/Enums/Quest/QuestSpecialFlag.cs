using System;

namespace Mangos.Common.Enums.Quest
{
    [Flags]
    public enum QuestSpecialFlag : int
    {
        QUEST_SPECIALFLAGS_NONE = 0,
        QUEST_SPECIALFLAGS_DELIVER = 1,
        QUEST_SPECIALFLAGS_EXPLORE = 2,
        QUEST_SPECIALFLAGS_SPEAKTO = 4,
        QUEST_SPECIALFLAGS_KILLORCAST = 8,
        QUEST_SPECIALFLAGS_TIMED = 16,
        // 32 is unknown
        QUEST_SPECIALFLAGS_REPUTATION = 64
    }
}