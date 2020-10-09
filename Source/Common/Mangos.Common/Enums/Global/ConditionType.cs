
namespace Mangos.Common.Enums.Global
{
    public enum ConditionType                   // value1       value2  for the Condition enumed
    {
        CONDITION_NONE = 0,                      // 0            0
        CONDITION_AURA = 1,                      // spell_id     effindex
        CONDITION_ITEM = 2,                      // item_id      count
        CONDITION_ITEM_EQUIPPED = 3,             // item_id      0
        CONDITION_ZONEID = 4,                    // zone_id      0
        CONDITION_REPUTATION_RANK = 5,           // faction_id   min_rank
        CONDITION_TEAM = 6,                      // player_team  0,      (469 - Alliance 67 - Horde)
        CONDITION_SKILL = 7,                     // skill_id     skill_value
        CONDITION_QUESTREWARDED = 8,             // quest_id     0
        CONDITION_QUESTTAKEN = 9,                // quest_id     0,      for condition true while quest active.
        CONDITION_AD_COMMISSION_AURA = 10,       // 0            0,      for condition true while one from AD ñommission aura active
        CONDITION_NO_AURA = 11,                  // spell_id     effindex
        CONDITION_ACTIVE_EVENT = 12,             // event_id
        CONDITION_INSTANCE_DATA = 13            // entry        data
    }
}