
namespace Mangos.Common.Enums.Guild
{
    public enum GuildEvent : byte
    {
        PROMOTION = 0,           // uint8(2), string(name), string(rankName)
        DEMOTION = 1,            // uint8(2), string(name), string(rankName)
        MOTD = 2,                // uint8(1), string(text)                                             'Guild message of the day: <text>
        JOINED = 3,              // uint8(1), string(name)                                             '<name> has joined the guild.
        LEFT = 4,                // uint8(1), string(name)                                             '<name> has left the guild.
        REMOVED = 5,             // ??
        LEADER_IS = 6,           // uint8(1), string(name                                              '<name> is the leader of your guild.
        LEADER_CHANGED = 7,      // uint8(2), string(oldLeaderName), string(newLeaderName) 
        DISBANDED = 8,           // uint8(0)                                                           'Your guild has been disbanded.
        TABARDCHANGE = 9,        // ??
        SIGNED_ON = 12,
        SIGNED_OFF = 13
    }
}