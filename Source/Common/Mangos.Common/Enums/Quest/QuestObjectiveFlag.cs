using System;

namespace Mangos.Common.Enums.Quest
{
    [Flags] // These flags are custom and are only used for MangosVB
    public enum QuestObjectiveFlag
    {
        QUEST_OBJECTIVE_KILL = 1, // You have to kill creatures
        QUEST_OBJECTIVE_EXPLORE = 2, // You have to explore an area
        QUEST_OBJECTIVE_ESCORT = 4, // You have to escort someone
        QUEST_OBJECTIVE_EVENT = 8, // Something is required to happen (escort may be included in this one)
        QUEST_OBJECTIVE_CAST = 16, // You will have to cast a spell on a creature or a gameobject (spells on gameobjects are f.ex opening)
        QUEST_OBJECTIVE_ITEM = 32, // You have to recieve some items to deliver
        QUEST_OBJECTIVE_EMOTE = 64 // You do an emote to a creature
    }
}