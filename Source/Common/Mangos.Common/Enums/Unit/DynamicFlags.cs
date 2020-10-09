
namespace Mangos.Common.Enums.Unit
{
    public enum DynamicFlags   // Dynamic flags for units
    {
        // Unit has blinking stars effect showing lootable
        UNIT_DYNFLAG_LOOTABLE = 0x1,
        // Shows marked unit as small red dot on radar
        UNIT_DYNFLAG_TRACK_UNIT = 0x2,
        // Gray mob title marks that mob is tagged by another player
        UNIT_DYNFLAG_OTHER_TAGGER = 0x4,
        // Blocks player character from moving
        UNIT_DYNFLAG_ROOTED = 0x8,
        // Shows infos like Damage and Health of the enemy
        UNIT_DYNFLAG_SPECIALINFO = 0x10,
        // Unit falls on the ground and shows like dead
        UNIT_DYNFLAG_DEAD = 0x20
    }
}