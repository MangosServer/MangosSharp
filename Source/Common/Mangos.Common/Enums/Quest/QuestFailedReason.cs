
namespace Mangos.Common.Enums.Quest
{
    public enum QuestFailedReason
    {
        // SMSG_QUESTGIVER_QUEST_FAILED
        // uint32 questID
        // uint32 failedReason

        FAILED_INVENTORY_FULL = 4,       // 0x04: '%s failed: Inventory is full.'
        FAILED_DUPE_ITEM = 0x11,         // 0x11: '%s failed: Duplicate item found.'
        FAILED_INVENTORY_FULL2 = 0x31,   // 0x31: '%s failed: Inventory is full.'
        FAILED_NOREASON = 0       // 0x00: '%s failed.'
    }
}