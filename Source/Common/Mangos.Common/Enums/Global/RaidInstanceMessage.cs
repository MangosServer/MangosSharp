
namespace Mangos.Common.Enums.Global
{
    public enum RaidInstanceMessage : uint
    {
        RAID_INSTANCE_WARNING_HOURS = 1U,         // WARNING! %s is scheduled to reset in %d hour(s).
        RAID_INSTANCE_WARNING_MIN = 2U,           // WARNING! %s is scheduled to reset in %d minute(s)!
        RAID_INSTANCE_WARNING_MIN_SOON = 3U,      // WARNING! %s is scheduled to reset in %d minute(s). Please exit the zone or you will be returned to your bind location!
        RAID_INSTANCE_WELCOME = 4U               // Welcome to %s. This raid instance is scheduled to reset in %s.
    }
}