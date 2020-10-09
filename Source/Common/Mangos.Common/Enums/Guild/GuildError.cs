
namespace Mangos.Common.Enums.Guild
{
    public enum GuildError : byte
    {
        GUILD_PLAYER_NO_MORE_IN_GUILD = 0x0,
        GUILD_INTERNAL = 0x1,
        GUILD_ALREADY_IN_GUILD = 0x2,
        ALREADY_IN_GUILD = 0x3,
        INVITED_TO_GUILD = 0x4,
        ALREADY_INVITED_TO_GUILD = 0x5,
        GUILD_NAME_INVALID = 0x6,
        GUILD_NAME_EXISTS = 0x7,
        GUILD_LEADER_LEAVE = 0x8,
        GUILD_PERMISSIONS = 0x8,
        GUILD_PLAYER_NOT_IN_GUILD = 0x9,
        GUILD_PLAYER_NOT_IN_GUILD_S = 0xA,
        GUILD_PLAYER_NOT_FOUND = 0xB,
        GUILD_NOT_ALLIED = 0xC
    }
}