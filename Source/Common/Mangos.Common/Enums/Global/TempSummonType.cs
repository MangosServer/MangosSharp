
namespace Mangos.Common.Enums.Global
{
    public enum TempSummonType
    {
        TEMPSUMMON_TIMED_OR_DEAD_DESPAWN = 1,             // despawns after a specified time OR when the creature disappears
        TEMPSUMMON_TIMED_OR_CORPSE_DESPAWN = 2,             // despawns after a specified time OR when the creature dies
        TEMPSUMMON_TIMED_DESPAWN = 3,             // despawns after a specified time
        TEMPSUMMON_TIMED_DESPAWN_OUT_OF_COMBAT = 4,             // despawns after a specified time after the creature is out of combat
        TEMPSUMMON_CORPSE_DESPAWN = 5,             // despawns instantly after death
        TEMPSUMMON_CORPSE_TIMED_DESPAWN = 6,             // despawns after a specified time after death
        TEMPSUMMON_DEAD_DESPAWN = 7,             // despawns when the creature disappears
        TEMPSUMMON_MANUAL_DESPAWN = 8              // despawns when UnSummon() is called
    }
}