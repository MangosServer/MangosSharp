using System;

namespace Mangos.Common.Enums.Player
{
    [Flags]
    public enum PlayerFlags : int
    {
        PLAYER_FLAGS_GROUP_LEADER = 0x1,
        PLAYER_FLAGS_AFK = 0x2,
        PLAYER_FLAGS_DND = 0x4,
        PLAYER_FLAGS_GM = 0x8,                        // GM Prefix
        PLAYER_FLAGS_DEAD = 0x10,
        PLAYER_FLAGS_RESTING = 0x20,
        PLAYER_FLAGS_UNK7 = 0x40,                    // Admin Prefix?
        PLAYER_FLAGS_FFA_PVP = 0x80,
        PLAYER_FLAGS_CONTESTED_PVP = 0x100,
        PLAYER_FLAGS_IN_PVP = 0x200,
        PLAYER_FLAGS_HIDE_HELM = 0x400,
        PLAYER_FLAGS_HIDE_CLOAK = 0x800,
        PLAYER_FLAGS_PARTIAL_PLAY_TIME = 0x1000,
        PLAYER_FLAGS_IS_OUT_OF_BOUNDS = 0x4000,      // Out of Bounds
        PLAYER_FLAGS_UNK15 = 0x8000,                 // Dev Prefix?
        PLAYER_FLAGS_SANCTUARY = 0x10000,
        PLAYER_FLAGS_NO_PLAY_TIME = 0x2000,
        PLAYER_FLAGS_PVP_TIMER = 0x40000
    }
}