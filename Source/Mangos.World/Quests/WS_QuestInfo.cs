//
// Copyright (C) 2013-2022 getMaNGOS <https://getmangos.eu>
//
// This program is free software. You can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation. either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY. Without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//

using Mangos.Common.Enums.Global;
using Mangos.Common.Legacy;
using Mangos.World.Player;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Data;

namespace Mangos.World.Quests;

public class WS_QuestInfo : IDisposable
{
    public int ID;

    public List<int> PreQuests;

    public int NextQuest;

    public int NextQuestInChain;

    public byte Method;

    public int Type;

    public int ZoneOrSort;

    public int QuestFlags;

    public int SpecialFlags;

    public byte Level_Start;

    public short Level_Normal;

    public string Title;

    public string TextObjectives;

    public string TextDescription;

    public string TextEnd;

    public string TextIncomplete;

    public string TextComplete;

    public int RequiredRace;

    public int RequiredClass;

    public int RequiredTradeSkill;

    public int RequiredTradeSkillValue;

    public int RequiredMinReputation;

    public int RequiredMinReputation_Faction;

    public int RequiredMaxReputation;

    public int RequiredMaxReputation_Faction;

    public int RewardHonor;

    public int RewardGold;

    public int RewMoneyMaxLevel;

    public int RewardSpell;

    public int RewardSpellCast;

    public int[] RewardItems;

    public int[] RewardItems_Count;

    public int[] RewardStaticItems;

    public int[] RewardStaticItems_Count;

    public int[] RewardRepFaction;

    public int[] RewardRepValue;

    public int RewMailTemplateId;

    public int RewMailDelaySecs;

    public int ObjectiveRepFaction;

    public int ObjectiveRepStanding;

    public int[] ObjectivesTrigger;

    public int[] ObjectivesCastSpell;

    public int[] ObjectivesKill;

    public int[] ObjectivesKill_Count;

    public int[] ObjectivesItem;

    public int[] ObjectivesItem_Count;

    public int ObjectivesDeliver;

    public int ObjectivesDeliver_Count;

    public string[] ObjectivesText;

    public int TimeLimit;

    public int SourceSpell;

    public int PointMapID;

    public float PointX;

    public float PointY;

    public int PointOpt;

    public int[] DetailsEmote;

    public int IncompleteEmote;

    public int CompleteEmote;

    public int[] OfferRewardEmote;

    public int StartScript;

    public int CompleteScript;

    private bool _disposedValue;

    public WS_QuestInfo(int QuestID)
    {
        RewardItems = new int[6];
        RewardItems_Count = new int[6];
        RewardStaticItems = new int[5];
        RewardStaticItems_Count = new int[5];
        RewardRepFaction = new int[5];
        RewardRepValue = new int[5];
        ObjectivesTrigger = new int[4];
        ObjectivesCastSpell = new int[4];
        ObjectivesKill = new int[4];
        ObjectivesKill_Count = new int[4];
        ObjectivesItem = new int[4];
        ObjectivesItem_Count = new int[4];
        DetailsEmote = new int[4];
        OfferRewardEmote = new int[4];
        NextQuest = 0;
        NextQuestInChain = 0;
        Method = 0;
        QuestFlags = 0;
        SpecialFlags = 0;
        Level_Start = 0;
        Level_Normal = 0;
        Title = "";
        TextObjectives = "";
        TextDescription = "";
        TextEnd = "";
        TextIncomplete = "";
        TextComplete = "";
        RewardHonor = 0;
        RewardGold = 0;
        RewMoneyMaxLevel = 0;
        RewardSpell = 0;
        RewardSpellCast = 0;
        ObjectivesText = new string[4]
        {
                "",
                "",
                "",
                ""
        };
        TimeLimit = 0;
        SourceSpell = 0;
        PointMapID = 0;
        PointX = 0f;
        PointY = 0f;
        PointOpt = 0;
        IncompleteEmote = 0;
        CompleteEmote = 0;
        StartScript = 0;
        CompleteScript = 0;
        ID = QuestID;
        PreQuests = new List<int>();
        DataTable MySQLQuery = new();
        WorldServiceLocator._WorldServer.WorldDatabase.Query($"SELECT * FROM quests WHERE entry = {QuestID};", ref MySQLQuery);
        if (MySQLQuery.Rows.Count == 0)
        {
            throw new ApplicationException("Quest " + Conversions.ToString(QuestID) + " not found in database.");
        }
        if (Operators.ConditionalCompareObjectGreater(MySQLQuery.Rows[0]["PrevQuestId"], 0, TextCompare: false))
        {
            PreQuests.Add(MySQLQuery.Rows[0].As<int>("PrevQuestId"));
        }
        NextQuest = MySQLQuery.Rows[0].As<int>("NextQuestId");
        NextQuestInChain = MySQLQuery.Rows[0].As<int>("NextQuestInChain");
        Level_Start = MySQLQuery.Rows[0].As<byte>("MinLevel");
        Level_Normal = MySQLQuery.Rows[0].As<short>("QuestLevel");
        Method = MySQLQuery.Rows[0].As<byte>("Method");
        Type = MySQLQuery.Rows[0].As<int>("Type");
        ZoneOrSort = MySQLQuery.Rows[0].As<int>("ZoneOrSort");
        QuestFlags = MySQLQuery.Rows[0].As<int>("QuestFlags");
        SpecialFlags = MySQLQuery.Rows[0].As<int>("SpecialFlags");
        var SkillOrClass = MySQLQuery.Rows[0].As<int>("SkillOrClass");
        if (SkillOrClass < 0)
        {
            RequiredClass = checked(-SkillOrClass);
        }
        else
        {
            RequiredTradeSkill = SkillOrClass;
        }
        RequiredRace = MySQLQuery.Rows[0].As<int>("RequiredRaces");
        RequiredTradeSkillValue = MySQLQuery.Rows[0].As<int>("RequiredSkillValue");
        RequiredMinReputation_Faction = MySQLQuery.Rows[0].As<int>("RequiredMinRepFaction");
        RequiredMinReputation = MySQLQuery.Rows[0].As<int>("RequiredMinRepValue");
        RequiredMinReputation_Faction = MySQLQuery.Rows[0].As<int>("RequiredMaxRepFaction");
        RequiredMinReputation = MySQLQuery.Rows[0].As<int>("RequiredMaxRepValue");
        ObjectiveRepFaction = MySQLQuery.Rows[0].As<int>("RepObjectiveFaction");
        ObjectiveRepStanding = MySQLQuery.Rows[0].As<int>("RepObjectiveValue");
        if (MySQLQuery.Rows[0]["Title"] is not DBNull)
        {
            Title = MySQLQuery.Rows[0].As<string>("Title");
        }
        if (MySQLQuery.Rows[0]["Objectives"] is not DBNull)
        {
            TextObjectives = MySQLQuery.Rows[0].As<string>("Objectives");
        }
        if (MySQLQuery.Rows[0]["Details"] is not DBNull)
        {
            TextDescription = MySQLQuery.Rows[0].As<string>("Details");
        }
        if (MySQLQuery.Rows[0]["EndText"] is not DBNull)
        {
            TextEnd = MySQLQuery.Rows[0].As<string>("EndText");
        }
        if (MySQLQuery.Rows[0]["RequestItemsText"] is not DBNull)
        {
            TextIncomplete = MySQLQuery.Rows[0].As<string>("RequestItemsText");
        }
        if (MySQLQuery.Rows[0]["OfferRewardText"] is not DBNull)
        {
            TextComplete = MySQLQuery.Rows[0].As<string>("OfferRewardText");
        }
        RewardGold = MySQLQuery.Rows[0].As<int>("RewOrReqMoney");
        RewMoneyMaxLevel = MySQLQuery.Rows[0].As<int>("RewMoneyMaxLevel");
        RewardSpell = MySQLQuery.Rows[0].As<int>("RewSpell");
        RewardSpellCast = MySQLQuery.Rows[0].As<int>("RewSpellCast");
        RewMailTemplateId = MySQLQuery.Rows[0].As<int>("RewMailTemplateId");
        RewMailDelaySecs = MySQLQuery.Rows[0].As<int>("RewMailDelaySecs");
        RewardItems[0] = MySQLQuery.Rows[0].As<int>("RewChoiceItemId1");
        RewardItems[1] = MySQLQuery.Rows[0].As<int>("RewChoiceItemId2");
        RewardItems[2] = MySQLQuery.Rows[0].As<int>("RewChoiceItemId3");
        RewardItems[3] = MySQLQuery.Rows[0].As<int>("RewChoiceItemId4");
        RewardItems[4] = MySQLQuery.Rows[0].As<int>("RewChoiceItemId5");
        RewardItems[5] = MySQLQuery.Rows[0].As<int>("RewChoiceItemId6");
        RewardItems_Count[0] = MySQLQuery.Rows[0].As<int>("RewChoiceItemCount1");
        RewardItems_Count[1] = MySQLQuery.Rows[0].As<int>("RewChoiceItemCount2");
        RewardItems_Count[2] = MySQLQuery.Rows[0].As<int>("RewChoiceItemCount3");
        RewardItems_Count[3] = MySQLQuery.Rows[0].As<int>("RewChoiceItemCount4");
        RewardItems_Count[4] = MySQLQuery.Rows[0].As<int>("RewChoiceItemCount5");
        RewardItems_Count[5] = MySQLQuery.Rows[0].As<int>("RewChoiceItemCount6");
        RewardStaticItems[0] = MySQLQuery.Rows[0].As<int>("RewItemId1");
        RewardStaticItems[1] = MySQLQuery.Rows[0].As<int>("RewItemId2");
        RewardStaticItems[2] = MySQLQuery.Rows[0].As<int>("RewItemId3");
        RewardStaticItems[3] = MySQLQuery.Rows[0].As<int>("RewItemId4");
        RewardStaticItems_Count[0] = MySQLQuery.Rows[0].As<int>("RewItemCount1");
        RewardStaticItems_Count[1] = MySQLQuery.Rows[0].As<int>("RewItemCount2");
        RewardStaticItems_Count[2] = MySQLQuery.Rows[0].As<int>("RewItemCount3");
        RewardStaticItems_Count[3] = MySQLQuery.Rows[0].As<int>("RewItemCount4");
        RewardRepFaction[0] = MySQLQuery.Rows[0].As<int>("RewRepFaction1");
        RewardRepFaction[1] = MySQLQuery.Rows[0].As<int>("RewRepFaction2");
        RewardRepFaction[2] = MySQLQuery.Rows[0].As<int>("RewRepFaction3");
        RewardRepFaction[3] = MySQLQuery.Rows[0].As<int>("RewRepFaction4");
        RewardRepFaction[4] = MySQLQuery.Rows[0].As<int>("RewRepFaction5");
        RewardRepValue[0] = MySQLQuery.Rows[0].As<int>("RewRepValue1");
        RewardRepValue[1] = MySQLQuery.Rows[0].As<int>("RewRepValue2");
        RewardRepValue[2] = MySQLQuery.Rows[0].As<int>("RewRepValue3");
        RewardRepValue[3] = MySQLQuery.Rows[0].As<int>("RewRepValue4");
        RewardRepValue[4] = MySQLQuery.Rows[0].As<int>("RewRepValue5");
        ObjectivesCastSpell[0] = MySQLQuery.Rows[0].As<int>("ReqSpellCast1");
        ObjectivesCastSpell[1] = MySQLQuery.Rows[0].As<int>("ReqSpellCast2");
        ObjectivesCastSpell[2] = MySQLQuery.Rows[0].As<int>("ReqSpellCast3");
        ObjectivesCastSpell[3] = MySQLQuery.Rows[0].As<int>("ReqSpellCast4");
        ObjectivesKill[0] = MySQLQuery.Rows[0].As<int>("ReqCreatureOrGOId1");
        ObjectivesKill[1] = MySQLQuery.Rows[0].As<int>("ReqCreatureOrGOId2");
        ObjectivesKill[2] = MySQLQuery.Rows[0].As<int>("ReqCreatureOrGOId3");
        ObjectivesKill[3] = MySQLQuery.Rows[0].As<int>("ReqCreatureOrGOId4");
        ObjectivesKill_Count[0] = MySQLQuery.Rows[0].As<int>("ReqCreatureOrGOCount1");
        ObjectivesKill_Count[1] = MySQLQuery.Rows[0].As<int>("ReqCreatureOrGOCount2");
        ObjectivesKill_Count[2] = MySQLQuery.Rows[0].As<int>("ReqCreatureOrGOCount3");
        ObjectivesKill_Count[3] = MySQLQuery.Rows[0].As<int>("ReqCreatureOrGOCount4");
        ObjectivesItem[0] = MySQLQuery.Rows[0].As<int>("ReqItemId1");
        ObjectivesItem[1] = MySQLQuery.Rows[0].As<int>("ReqItemId2");
        ObjectivesItem[2] = MySQLQuery.Rows[0].As<int>("ReqItemId3");
        ObjectivesItem[3] = MySQLQuery.Rows[0].As<int>("ReqItemId4");
        ObjectivesItem_Count[0] = MySQLQuery.Rows[0].As<int>("ReqItemCount1");
        ObjectivesItem_Count[1] = MySQLQuery.Rows[0].As<int>("ReqItemCount2");
        ObjectivesItem_Count[2] = MySQLQuery.Rows[0].As<int>("ReqItemCount3");
        ObjectivesItem_Count[3] = MySQLQuery.Rows[0].As<int>("ReqItemCount4");
        ObjectivesDeliver = MySQLQuery.Rows[0].As<int>("SrcItemId");
        ObjectivesDeliver_Count = MySQLQuery.Rows[0].As<int>("SrcItemCount");
        if (MySQLQuery.Rows[0]["ObjectiveText1"] is not DBNull)
        {
            ObjectivesText[0] = MySQLQuery.Rows[0].As<string>("ObjectiveText1");
        }
        if (MySQLQuery.Rows[0]["ObjectiveText2"] is not DBNull)
        {
            ObjectivesText[1] = MySQLQuery.Rows[0].As<string>("ObjectiveText2");
        }
        if (MySQLQuery.Rows[0]["ObjectiveText3"] is not DBNull)
        {
            ObjectivesText[2] = MySQLQuery.Rows[0].As<string>("ObjectiveText3");
        }
        if (MySQLQuery.Rows[0]["ObjectiveText4"] is not DBNull)
        {
            ObjectivesText[3] = MySQLQuery.Rows[0].As<string>("ObjectiveText4");
        }
        TimeLimit = MySQLQuery.Rows[0].As<int>("LimitTime");
        SourceSpell = MySQLQuery.Rows[0].As<int>("SrcSpell");
        PointMapID = MySQLQuery.Rows[0].As<int>("PointMapId");
        PointX = MySQLQuery.Rows[0].As<float>("PointX");
        PointY = MySQLQuery.Rows[0].As<float>("PointY");
        PointOpt = MySQLQuery.Rows[0].As<int>("PointOpt");
        DetailsEmote[0] = MySQLQuery.Rows[0].As<int>("DetailsEmote1");
        DetailsEmote[1] = MySQLQuery.Rows[0].As<int>("DetailsEmote2");
        DetailsEmote[2] = MySQLQuery.Rows[0].As<int>("DetailsEmote3");
        DetailsEmote[3] = MySQLQuery.Rows[0].As<int>("DetailsEmote4");
        IncompleteEmote = MySQLQuery.Rows[0].As<int>("IncompleteEmote");
        CompleteEmote = MySQLQuery.Rows[0].As<int>("CompleteEmote");
        OfferRewardEmote[0] = MySQLQuery.Rows[0].As<int>("OfferRewardEmote1");
        OfferRewardEmote[1] = MySQLQuery.Rows[0].As<int>("OfferRewardEmote2");
        OfferRewardEmote[2] = MySQLQuery.Rows[0].As<int>("OfferRewardEmote3");
        OfferRewardEmote[3] = MySQLQuery.Rows[0].As<int>("OfferRewardEmote4");
        StartScript = MySQLQuery.Rows[0].As<int>("StartScript");
        CompleteScript = MySQLQuery.Rows[0].As<int>("CompleteScript");
        InitQuest();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
        }
        _disposedValue = true;
        GC.Collect();
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    void IDisposable.Dispose()
    {
        //ILSpy generated this explicit interface implementation from .override directive in Dispose
        Dispose();
    }

    private void InitQuest()
    {
        if (NextQuestInChain > 0)
        {
            if (!WorldServiceLocator._WorldServer.ALLQUESTS.IsValidQuest(NextQuestInChain))
            {
                WS_QuestInfo tmpQuest2 = new(NextQuestInChain);
                if (!tmpQuest2.PreQuests.Contains(ID))
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Added prequest [{0}] to quest [{1}]", ID, NextQuestInChain);
                    tmpQuest2.PreQuests.Add(ID);
                }
            }
            else if (!WorldServiceLocator._WorldServer.ALLQUESTS.DoesPreQuestExist(NextQuestInChain, ID))
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Added prequest [{0}] to quest [{1}]", NextQuestInChain, ID);
                WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(NextQuestInChain).PreQuests.Add(ID);
            }
        }
        if (NextQuest == 0)
        {
            return;
        }
        var unsignedNextQuest = Math.Abs(NextQuest);
        var signedQuestID = (NextQuest < 0) ? checked(-ID) : ID;
        if (!WorldServiceLocator._WorldServer.ALLQUESTS.IsValidQuest(unsignedNextQuest))
        {
            WS_QuestInfo tmpQuest = new(unsignedNextQuest);
            if (!tmpQuest.PreQuests.Contains(signedQuestID))
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Added prequest [{0}] to quest [{1}]", signedQuestID, unsignedNextQuest);
                tmpQuest.PreQuests.Add(signedQuestID);
            }
        }
        else if (!WorldServiceLocator._WorldServer.ALLQUESTS.DoesPreQuestExist(unsignedNextQuest, signedQuestID))
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Added prequest [{0}] to quest [{1}]", signedQuestID, unsignedNextQuest);
            WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(unsignedNextQuest).PreQuests.Add(signedQuestID);
        }
    }

    public bool CanSeeQuest(ref WS_PlayerData.CharacterObject objCharacter)
    {
        var retValue = true;
        checked
        {
            if (objCharacter.Level + 6 < Level_Start)
            {
                retValue = false;
            }
        }
        if (RequiredClass > 0 && RequiredClass != (int)objCharacter.Classe)
        {
            retValue = false;
        }
        if (ZoneOrSort < 0)
        {
            WS_Quests tmpQuest = new();
            var reqSort = tmpQuest.ClassByQuestSort(checked(-ZoneOrSort));
            if (reqSort > 0 && reqSort != (uint)objCharacter.Classe)
            {
                retValue = false;
            }
        }
        if (RequiredRace != 0 && (RequiredRace & objCharacter.RaceMask) == 0)
        {
            retValue = false;
        }
        if (RequiredTradeSkill > 0)
        {
            if (!objCharacter.Skills.ContainsKey(RequiredTradeSkill))
            {
                retValue = false;
            }
            if (objCharacter.Skills[RequiredTradeSkill].Current < RequiredTradeSkillValue)
            {
                retValue = false;
            }
        }
        if (RequiredMinReputation_Faction > 0 && objCharacter.GetReputationValue(RequiredMinReputation_Faction) < RequiredMinReputation)
        {
            retValue = false;
        }
        if (RequiredMaxReputation_Faction > 0 && objCharacter.GetReputationValue(RequiredMaxReputation_Faction) >= RequiredMaxReputation)
        {
            retValue = false;
        }
        if (PreQuests.Count > 0)
        {
            foreach (var QuestID in PreQuests)
            {
                if (QuestID > 0)
                {
                    if (!objCharacter.QuestsCompleted.Contains(QuestID))
                    {
                        retValue = false;
                    }
                }
                else if (QuestID < 0 && objCharacter.QuestsCompleted.Contains(QuestID))
                {
                    retValue = false;
                }
            }
        }
        if (objCharacter.QuestsCompleted.Contains(ID))
        {
            retValue = false;
        }
        if (objCharacter.IsQuestInProgress(ID))
        {
            retValue = false;
        }
        return retValue;
    }

    public bool SatisfyQuestLevel(ref WS_PlayerData.CharacterObject objCharacter)
    {
        return objCharacter.Level >= (uint)Level_Start;
    }
}
