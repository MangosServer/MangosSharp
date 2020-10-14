//
//  Copyright (C) 2013-2020 getMaNGOS <https:\\getmangos.eu>
//  
//  This program is free software. You can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation. either version 2 of the License, or
//  (at your option) any later version.
//  
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY. Without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//  
//  You should have received a copy of the GNU General Public License
//  along with this program. If not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//

using System;
using System.Collections.Generic;
using System.Data;
using Mangos.Common.Enums.Global;
using Mangos.World.Player;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Quests
{
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
			DataTable MySQLQuery = new DataTable();
			WorldServiceLocator._WorldServer.WorldDatabase.Query($"SELECT * FROM quests WHERE entry = {QuestID};", ref MySQLQuery);
			if (MySQLQuery.Rows.Count == 0)
			{
				throw new ApplicationException("Quest " + Conversions.ToString(QuestID) + " not found in database.");
			}
			if (Operators.ConditionalCompareObjectGreater(MySQLQuery.Rows[0]["PrevQuestId"], 0, TextCompare: false))
			{
				PreQuests.Add(Conversions.ToInteger(MySQLQuery.Rows[0]["PrevQuestId"]));
			}
			NextQuest = Conversions.ToInteger(MySQLQuery.Rows[0]["NextQuestId"]);
			NextQuestInChain = Conversions.ToInteger(MySQLQuery.Rows[0]["NextQuestInChain"]);
			Level_Start = Conversions.ToByte(MySQLQuery.Rows[0]["MinLevel"]);
			Level_Normal = Conversions.ToShort(MySQLQuery.Rows[0]["QuestLevel"]);
			Method = Conversions.ToByte(MySQLQuery.Rows[0]["Method"]);
			Type = Conversions.ToInteger(MySQLQuery.Rows[0]["Type"]);
			ZoneOrSort = Conversions.ToInteger(MySQLQuery.Rows[0]["ZoneOrSort"]);
			QuestFlags = Conversions.ToInteger(MySQLQuery.Rows[0]["QuestFlags"]);
			SpecialFlags = Conversions.ToInteger(MySQLQuery.Rows[0]["SpecialFlags"]);
			int SkillOrClass = Conversions.ToInteger(MySQLQuery.Rows[0]["SkillOrClass"]);
			if (SkillOrClass < 0)
			{
				RequiredClass = checked(-SkillOrClass);
			}
			else
			{
				RequiredTradeSkill = SkillOrClass;
			}
			RequiredRace = Conversions.ToInteger(MySQLQuery.Rows[0]["RequiredRaces"]);
			RequiredTradeSkillValue = Conversions.ToInteger(MySQLQuery.Rows[0]["RequiredSkillValue"]);
			RequiredMinReputation_Faction = Conversions.ToInteger(MySQLQuery.Rows[0]["RequiredMinRepFaction"]);
			RequiredMinReputation = Conversions.ToInteger(MySQLQuery.Rows[0]["RequiredMinRepValue"]);
			RequiredMinReputation_Faction = Conversions.ToInteger(MySQLQuery.Rows[0]["RequiredMaxRepFaction"]);
			RequiredMinReputation = Conversions.ToInteger(MySQLQuery.Rows[0]["RequiredMaxRepValue"]);
			ObjectiveRepFaction = Conversions.ToInteger(MySQLQuery.Rows[0]["RepObjectiveFaction"]);
			ObjectiveRepStanding = Conversions.ToInteger(MySQLQuery.Rows[0]["RepObjectiveValue"]);
			if (!(MySQLQuery.Rows[0]["Title"] is DBNull))
			{
				Title = Conversions.ToString(MySQLQuery.Rows[0]["Title"]);
			}
			if (!(MySQLQuery.Rows[0]["Objectives"] is DBNull))
			{
				TextObjectives = Conversions.ToString(MySQLQuery.Rows[0]["Objectives"]);
			}
			if (!(MySQLQuery.Rows[0]["Details"] is DBNull))
			{
				TextDescription = Conversions.ToString(MySQLQuery.Rows[0]["Details"]);
			}
			if (!(MySQLQuery.Rows[0]["EndText"] is DBNull))
			{
				TextEnd = Conversions.ToString(MySQLQuery.Rows[0]["EndText"]);
			}
			if (!(MySQLQuery.Rows[0]["RequestItemsText"] is DBNull))
			{
				TextIncomplete = Conversions.ToString(MySQLQuery.Rows[0]["RequestItemsText"]);
			}
			if (!(MySQLQuery.Rows[0]["OfferRewardText"] is DBNull))
			{
				TextComplete = Conversions.ToString(MySQLQuery.Rows[0]["OfferRewardText"]);
			}
			RewardGold = Conversions.ToInteger(MySQLQuery.Rows[0]["RewOrReqMoney"]);
			RewMoneyMaxLevel = Conversions.ToInteger(MySQLQuery.Rows[0]["RewMoneyMaxLevel"]);
			RewardSpell = Conversions.ToInteger(MySQLQuery.Rows[0]["RewSpell"]);
			RewardSpellCast = Conversions.ToInteger(MySQLQuery.Rows[0]["RewSpellCast"]);
			RewMailTemplateId = Conversions.ToInteger(MySQLQuery.Rows[0]["RewMailTemplateId"]);
			RewMailDelaySecs = Conversions.ToInteger(MySQLQuery.Rows[0]["RewMailDelaySecs"]);
			RewardItems[0] = Conversions.ToInteger(MySQLQuery.Rows[0]["RewChoiceItemId1"]);
			RewardItems[1] = Conversions.ToInteger(MySQLQuery.Rows[0]["RewChoiceItemId2"]);
			RewardItems[2] = Conversions.ToInteger(MySQLQuery.Rows[0]["RewChoiceItemId3"]);
			RewardItems[3] = Conversions.ToInteger(MySQLQuery.Rows[0]["RewChoiceItemId4"]);
			RewardItems[4] = Conversions.ToInteger(MySQLQuery.Rows[0]["RewChoiceItemId5"]);
			RewardItems[5] = Conversions.ToInteger(MySQLQuery.Rows[0]["RewChoiceItemId6"]);
			RewardItems_Count[0] = Conversions.ToInteger(MySQLQuery.Rows[0]["RewChoiceItemCount1"]);
			RewardItems_Count[1] = Conversions.ToInteger(MySQLQuery.Rows[0]["RewChoiceItemCount2"]);
			RewardItems_Count[2] = Conversions.ToInteger(MySQLQuery.Rows[0]["RewChoiceItemCount3"]);
			RewardItems_Count[3] = Conversions.ToInteger(MySQLQuery.Rows[0]["RewChoiceItemCount4"]);
			RewardItems_Count[4] = Conversions.ToInteger(MySQLQuery.Rows[0]["RewChoiceItemCount5"]);
			RewardItems_Count[5] = Conversions.ToInteger(MySQLQuery.Rows[0]["RewChoiceItemCount6"]);
			RewardStaticItems[0] = Conversions.ToInteger(MySQLQuery.Rows[0]["RewItemId1"]);
			RewardStaticItems[1] = Conversions.ToInteger(MySQLQuery.Rows[0]["RewItemId2"]);
			RewardStaticItems[2] = Conversions.ToInteger(MySQLQuery.Rows[0]["RewItemId3"]);
			RewardStaticItems[3] = Conversions.ToInteger(MySQLQuery.Rows[0]["RewItemId4"]);
			RewardStaticItems_Count[0] = Conversions.ToInteger(MySQLQuery.Rows[0]["RewItemCount1"]);
			RewardStaticItems_Count[1] = Conversions.ToInteger(MySQLQuery.Rows[0]["RewItemCount2"]);
			RewardStaticItems_Count[2] = Conversions.ToInteger(MySQLQuery.Rows[0]["RewItemCount3"]);
			RewardStaticItems_Count[3] = Conversions.ToInteger(MySQLQuery.Rows[0]["RewItemCount4"]);
			RewardRepFaction[0] = Conversions.ToInteger(MySQLQuery.Rows[0]["RewRepFaction1"]);
			RewardRepFaction[1] = Conversions.ToInteger(MySQLQuery.Rows[0]["RewRepFaction2"]);
			RewardRepFaction[2] = Conversions.ToInteger(MySQLQuery.Rows[0]["RewRepFaction3"]);
			RewardRepFaction[3] = Conversions.ToInteger(MySQLQuery.Rows[0]["RewRepFaction4"]);
			RewardRepFaction[4] = Conversions.ToInteger(MySQLQuery.Rows[0]["RewRepFaction5"]);
			RewardRepValue[0] = Conversions.ToInteger(MySQLQuery.Rows[0]["RewRepValue1"]);
			RewardRepValue[1] = Conversions.ToInteger(MySQLQuery.Rows[0]["RewRepValue2"]);
			RewardRepValue[2] = Conversions.ToInteger(MySQLQuery.Rows[0]["RewRepValue3"]);
			RewardRepValue[3] = Conversions.ToInteger(MySQLQuery.Rows[0]["RewRepValue4"]);
			RewardRepValue[4] = Conversions.ToInteger(MySQLQuery.Rows[0]["RewRepValue5"]);
			ObjectivesCastSpell[0] = Conversions.ToInteger(MySQLQuery.Rows[0]["ReqSpellCast1"]);
			ObjectivesCastSpell[1] = Conversions.ToInteger(MySQLQuery.Rows[0]["ReqSpellCast2"]);
			ObjectivesCastSpell[2] = Conversions.ToInteger(MySQLQuery.Rows[0]["ReqSpellCast3"]);
			ObjectivesCastSpell[3] = Conversions.ToInteger(MySQLQuery.Rows[0]["ReqSpellCast4"]);
			ObjectivesKill[0] = Conversions.ToInteger(MySQLQuery.Rows[0]["ReqCreatureOrGOId1"]);
			ObjectivesKill[1] = Conversions.ToInteger(MySQLQuery.Rows[0]["ReqCreatureOrGOId2"]);
			ObjectivesKill[2] = Conversions.ToInteger(MySQLQuery.Rows[0]["ReqCreatureOrGOId3"]);
			ObjectivesKill[3] = Conversions.ToInteger(MySQLQuery.Rows[0]["ReqCreatureOrGOId4"]);
			ObjectivesKill_Count[0] = Conversions.ToInteger(MySQLQuery.Rows[0]["ReqCreatureOrGOCount1"]);
			ObjectivesKill_Count[1] = Conversions.ToInteger(MySQLQuery.Rows[0]["ReqCreatureOrGOCount2"]);
			ObjectivesKill_Count[2] = Conversions.ToInteger(MySQLQuery.Rows[0]["ReqCreatureOrGOCount3"]);
			ObjectivesKill_Count[3] = Conversions.ToInteger(MySQLQuery.Rows[0]["ReqCreatureOrGOCount4"]);
			ObjectivesItem[0] = Conversions.ToInteger(MySQLQuery.Rows[0]["ReqItemId1"]);
			ObjectivesItem[1] = Conversions.ToInteger(MySQLQuery.Rows[0]["ReqItemId2"]);
			ObjectivesItem[2] = Conversions.ToInteger(MySQLQuery.Rows[0]["ReqItemId3"]);
			ObjectivesItem[3] = Conversions.ToInteger(MySQLQuery.Rows[0]["ReqItemId4"]);
			ObjectivesItem_Count[0] = Conversions.ToInteger(MySQLQuery.Rows[0]["ReqItemCount1"]);
			ObjectivesItem_Count[1] = Conversions.ToInteger(MySQLQuery.Rows[0]["ReqItemCount2"]);
			ObjectivesItem_Count[2] = Conversions.ToInteger(MySQLQuery.Rows[0]["ReqItemCount3"]);
			ObjectivesItem_Count[3] = Conversions.ToInteger(MySQLQuery.Rows[0]["ReqItemCount4"]);
			ObjectivesDeliver = Conversions.ToInteger(MySQLQuery.Rows[0]["SrcItemId"]);
			ObjectivesDeliver_Count = Conversions.ToInteger(MySQLQuery.Rows[0]["SrcItemCount"]);
			if (!(MySQLQuery.Rows[0]["ObjectiveText1"] is DBNull))
			{
				ObjectivesText[0] = Conversions.ToString(MySQLQuery.Rows[0]["ObjectiveText1"]);
			}
			if (!(MySQLQuery.Rows[0]["ObjectiveText2"] is DBNull))
			{
				ObjectivesText[1] = Conversions.ToString(MySQLQuery.Rows[0]["ObjectiveText2"]);
			}
			if (!(MySQLQuery.Rows[0]["ObjectiveText3"] is DBNull))
			{
				ObjectivesText[2] = Conversions.ToString(MySQLQuery.Rows[0]["ObjectiveText3"]);
			}
			if (!(MySQLQuery.Rows[0]["ObjectiveText4"] is DBNull))
			{
				ObjectivesText[3] = Conversions.ToString(MySQLQuery.Rows[0]["ObjectiveText4"]);
			}
			TimeLimit = Conversions.ToInteger(MySQLQuery.Rows[0]["LimitTime"]);
			SourceSpell = Conversions.ToInteger(MySQLQuery.Rows[0]["SrcSpell"]);
			PointMapID = Conversions.ToInteger(MySQLQuery.Rows[0]["PointMapId"]);
			PointX = Conversions.ToSingle(MySQLQuery.Rows[0]["PointX"]);
			PointY = Conversions.ToSingle(MySQLQuery.Rows[0]["PointY"]);
			PointOpt = Conversions.ToInteger(MySQLQuery.Rows[0]["PointOpt"]);
			DetailsEmote[0] = Conversions.ToInteger(MySQLQuery.Rows[0]["DetailsEmote1"]);
			DetailsEmote[1] = Conversions.ToInteger(MySQLQuery.Rows[0]["DetailsEmote2"]);
			DetailsEmote[2] = Conversions.ToInteger(MySQLQuery.Rows[0]["DetailsEmote3"]);
			DetailsEmote[3] = Conversions.ToInteger(MySQLQuery.Rows[0]["DetailsEmote4"]);
			IncompleteEmote = Conversions.ToInteger(MySQLQuery.Rows[0]["IncompleteEmote"]);
			CompleteEmote = Conversions.ToInteger(MySQLQuery.Rows[0]["CompleteEmote"]);
			OfferRewardEmote[0] = Conversions.ToInteger(MySQLQuery.Rows[0]["OfferRewardEmote1"]);
			OfferRewardEmote[1] = Conversions.ToInteger(MySQLQuery.Rows[0]["OfferRewardEmote2"]);
			OfferRewardEmote[2] = Conversions.ToInteger(MySQLQuery.Rows[0]["OfferRewardEmote3"]);
			OfferRewardEmote[3] = Conversions.ToInteger(MySQLQuery.Rows[0]["OfferRewardEmote4"]);
			StartScript = Conversions.ToInteger(MySQLQuery.Rows[0]["StartScript"]);
			CompleteScript = Conversions.ToInteger(MySQLQuery.Rows[0]["CompleteScript"]);
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
					WS_QuestInfo tmpQuest2 = new WS_QuestInfo(NextQuestInChain);
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
			int unsignedNextQuest = Math.Abs(NextQuest);
			int signedQuestID = ((NextQuest < 0) ? checked(-ID) : ID);
			if (!WorldServiceLocator._WorldServer.ALLQUESTS.IsValidQuest(unsignedNextQuest))
			{
				WS_QuestInfo tmpQuest = new WS_QuestInfo(unsignedNextQuest);
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
			bool retValue = true;
			checked
			{
				if (unchecked(objCharacter.Level) + 6 < Level_Start)
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
				WS_Quests tmpQuest = new WS_Quests();
				byte reqSort = tmpQuest.ClassByQuestSort(checked(-ZoneOrSort));
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
				foreach (int QuestID in PreQuests)
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
			if (objCharacter.Level < (uint)Level_Start)
			{
				return false;
			}
			return true;
		}
	}
}
