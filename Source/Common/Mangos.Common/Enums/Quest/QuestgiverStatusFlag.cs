
namespace Mangos.Common.Enums.Quest
{
    public enum QuestgiverStatusFlag : int
    {
        DIALOG_STATUS_NONE = 0,                  // There aren't any quests available. - No Mark
        DIALOG_STATUS_UNAVAILABLE = 1,           // Quest available and your leve isn't enough. - Gray Quotation ! Mark
        DIALOG_STATUS_CHAT = 2,                  // Quest available it shows a talk baloon. - No Mark
        DIALOG_STATUS_INCOMPLETE = 3,            // Quest isn't finished yet. - Gray Question ? Mark
        DIALOG_STATUS_REWARD_REP = 4,            // Rewards rep? :P
        DIALOG_STATUS_AVAILABLE = 5,             // Quest available, and your level is enough. - Yellow Quotation ! Mark
        DIALOG_STATUS_REWARD = 6                // Quest has been finished. - Yellow dot on the minimap
    }
}