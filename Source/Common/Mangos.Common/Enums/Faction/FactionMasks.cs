
namespace Mangos.Common.Enums.Faction
{
    public enum FactionMasks
    {
        FACTION_MASK_PLAYER = 1,     // any player
        FACTION_MASK_ALLIANCE = 2,   // player or creature from alliance team
        FACTION_MASK_HORDE = 4,      // player or creature from horde team
        FACTION_MASK_MONSTER = 8    // aggressive creature from monster team
    }
}