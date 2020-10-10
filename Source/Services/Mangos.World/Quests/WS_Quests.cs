// 
// Copyright (C) 2013-2020 getMaNGOS <https://getmangos.eu>
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

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Player;
using Mangos.Common.Enums.Quest;
using Mangos.Common.Enums.Spell;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Loots;
using Mangos.World.Objects;
using Mangos.World.Player;
using Mangos.World.Server;
using Mangos.World.Spells;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Quests
{
    public class WS_Quests
    {

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        private readonly Collection _quests = new Collection();

        /// <summary>
        /// Loads All the quests into the collection.
        /// </summary>
        public void LoadAllQuests()
        {
            var cQuests = new DataTable();
            WS_QuestInfo tmpQuest;
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Loading Quests...");
            WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT entry FROM quests;"), cQuests);
            foreach (DataRow cRow in cQuests.Rows)
            {
                int questID = Conversions.ToInteger(cRow["entry"]);
                tmpQuest = new WS_QuestInfo(questID);
                _quests.Add(tmpQuest, questID.ToString());
            }

            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Loading Quests...Complete");
        }

        /// <summary>
        /// Returns the first Quest Id found when passed the quest name
        /// </summary>
        /// <param name="searchValue"></param>
        /// <returns>QuestId (Int)</returns>
        /// <remarks></remarks>
        public int ReturnQuestIdByName(string searchValue)
        {
            foreach (WS_QuestInfo thisService in _quests)
            {
                if ((thisService.Title ?? "") == (searchValue ?? ""))
                {
                    return thisService.ID;
                }
            }

            return 0;
        }

        /// <summary>
        /// Does the pre quest exist.
        /// </summary>
        /// <param name="questID">The quest ID.</param>
        /// <param name="preQuestID">The pre quest ID.</param>
        /// <returns></returns>
        public bool DoesPreQuestExist(int questID, int preQuestID)
        {
            bool ret = false;
            foreach (WS_QuestInfo thisService in _quests)
            {
                if (thisService.ID == questID)
                {
                    if (thisService.PreQuests.Contains(preQuestID) == true)
                    {
                        return true;
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Returns whether the Quest Id is a valid quest
        /// </summary>
        /// <param name="questID"></param>
        /// <returns>Bool</returns>
        /// <remarks></remarks>
        public bool IsValidQuest(int questID)
        {
            if (_quests.Contains(questID.ToString()))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the Quest Name found when passed the quest Id
        /// </summary>
        /// <param name="questId"></param>
        /// <returns>QuestName (String)</returns>
        /// <remarks></remarks>
        public string ReturnQuestNameById(int questId)
        {
            string ret = "";
            foreach (WS_QuestInfo thisQuest in _quests)
            {
                if (thisQuest.ID == questId)
                    ret = thisQuest.Title;
            }

            return ret;
        }

        /// <summary>
        /// Returns the QuestInfo Structure  when passed the quest Id
        /// </summary>
        /// <param name="questId"></param>
        /// <returns><c>WS_QuestInfo</c></returns>
        /// <remarks></remarks>
        public WS_QuestInfo ReturnQuestInfoById(int questId)
        {
            WS_QuestInfo ret = null;
            try
            {
                if (_quests.Contains(questId.ToString()))
                {
                    return (WS_QuestInfo)_quests[questId.ToString()];
                }
            }
            catch (Exception)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "ReturnQuestInfoById returned error on QuestId {0}", questId);
            }

            return ret;
        }

        /// Rewritten Code above this line

        /// <summary>
        /// Gets the quest menu.
        /// </summary>
        /// <param name="objCharacter">The Character.</param>
        /// <param name="guid">The unique identifier.</param>
        /// <returns></returns>
        public QuestMenu GetQuestMenu(ref WS_PlayerData.CharacterObject objCharacter, ulong guid)
        {
            var questMenu = new QuestMenu();
            int creatureEntry = WorldServiceLocator._WorldServer.WORLD_CREATUREs[guid].ID;

            // DONE: Quests for completing
            var alreadyHave = new List<int>();
            if (WorldServiceLocator._WorldServer.CreatureQuestFinishers.ContainsKey(creatureEntry))
            {
                try
                {
                    for (int i = 0, loopTo = (int)QuestInfo.QUEST_SLOTS; i <= loopTo; i++)
                    {
                        if (objCharacter.TalkQuests[i] is object)
                        {
                            alreadyHave.Add(objCharacter.TalkQuests[i].ID);
                            if (WorldServiceLocator._WorldServer.CreatureQuestFinishers[creatureEntry].Contains(objCharacter.TalkQuests[i].ID))
                            {
                                questMenu.AddMenu(objCharacter.TalkQuests[i].Title, (short)objCharacter.TalkQuests[i].ID, 0, (byte)QuestgiverStatusFlag.DIALOG_STATUS_INCOMPLETE);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "GetQuestMenu Failed: ", ex.ToString());
                }
            }


            // DONE: Quests for taking
            if (WorldServiceLocator._WorldServer.CreatureQuestStarters.ContainsKey(creatureEntry))
            {
                try
                {
                    foreach (int questID in WorldServiceLocator._WorldServer.CreatureQuestStarters[creatureEntry])
                    {
                        if (alreadyHave.Contains(questID))
                            continue;
                        if (!WorldServiceLocator._WorldServer.ALLQUESTS.IsValidQuest(questID))
                        {
                            try // Sometimes Initialising Questinfo triggers an exception
                            {
                                var tmpQuest = new WS_QuestInfo(questID);
                                if (tmpQuest.CanSeeQuest(ref objCharacter))
                                {
                                    if (tmpQuest.SatisfyQuestLevel(ref objCharacter))
                                    {
                                        questMenu.AddMenu(tmpQuest.Title, (short)questID, tmpQuest.Level_Normal, (byte)QuestgiverStatusFlag.DIALOG_STATUS_AVAILABLE);
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "GetQuestMenu returned error for QuestId {0}", questID);
                            }
                        }
                        else if (WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID).CanSeeQuest(ref objCharacter))
                        {
                            if (WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID).SatisfyQuestLevel(ref objCharacter))
                            {
                                questMenu.AddMenu(WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID).Title, (short)questID, WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID).Level_Normal, (byte)QuestgiverStatusFlag.DIALOG_STATUS_AVAILABLE);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "GetQuestMenu Failed: ", ex.ToString());
                }
            }

            return questMenu;
        }

        /// <summary>
        /// Gets the quest menu go.
        /// </summary>
        /// <param name="objCharacter">The Character.</param>
        /// <param name="guid">The unique identifier.</param>
        /// <returns></returns>
        public QuestMenu GetQuestMenuGO(ref WS_PlayerData.CharacterObject objCharacter, ulong guid)
        {
            var questMenu = new QuestMenu();
            int gOEntry = WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[guid].ID;

            // DONE: Quests for completing
            var alreadyHave = new List<int>();
            if (WorldServiceLocator._WorldServer.GameobjectQuestFinishers.ContainsKey(gOEntry))
            {
                try
                {
                    for (int i = 0, loopTo = (int)QuestInfo.QUEST_SLOTS; i <= loopTo; i++)
                    {
                        if (objCharacter.TalkQuests[i] is object)
                        {
                            alreadyHave.Add(objCharacter.TalkQuests[i].ID);
                            if (WorldServiceLocator._WorldServer.GameobjectQuestFinishers[gOEntry].Contains(objCharacter.TalkQuests[i].ID))
                            {
                                questMenu.AddMenu(objCharacter.TalkQuests[i].Title, (short)objCharacter.TalkQuests[i].ID, 0, (byte)QuestgiverStatusFlag.DIALOG_STATUS_INCOMPLETE);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "GetQuestMenuGO Failed: ", ex.ToString());
                }
            }

            // DONE: Quests for taking
            if (WorldServiceLocator._WorldServer.GameobjectQuestStarters.ContainsKey(gOEntry))
            {
                try
                {
                    foreach (int questID in WorldServiceLocator._WorldServer.GameobjectQuestStarters[gOEntry])
                    {
                        if (alreadyHave.Contains(questID))
                            continue;
                        if (!WorldServiceLocator._WorldServer.ALLQUESTS.IsValidQuest(questID))
                        {
                            var tmpQuest = new WS_QuestInfo(questID);
                            if (tmpQuest.CanSeeQuest(ref objCharacter))
                            {
                                if (tmpQuest.SatisfyQuestLevel(ref objCharacter))
                                {
                                    questMenu.AddMenu(tmpQuest.Title, (short)questID, tmpQuest.Level_Normal, (byte)QuestgiverStatusFlag.DIALOG_STATUS_AVAILABLE);
                                }
                            }
                        }
                        else if (WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID).CanSeeQuest(ref objCharacter))
                        {
                            if (WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID).SatisfyQuestLevel(ref objCharacter))
                            {
                                questMenu.AddMenu(WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID).Title, (short)questID, WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID).Level_Normal, (byte)QuestgiverStatusFlag.DIALOG_STATUS_AVAILABLE);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "GetQuestMenuGO Failed: ", ex.ToString());
                }
            }

            return questMenu;
        }

        /// <summary>
        /// Sends the quest menu.
        /// </summary>
        /// <param name="objCharacter">The Character.</param>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="title">The title.</param>
        /// <param name="questMenu">The quest menu.</param>
        public void SendQuestMenu(ref WS_PlayerData.CharacterObject objCharacter, ulong guid, string title = "Available quests", QuestMenu questMenu = null)
        {
            if (questMenu is null)
            {
                questMenu = GetQuestMenu(ref objCharacter, guid);
            }

            var packet = new Packets.PacketClass(OPCODES.SMSG_QUESTGIVER_QUEST_LIST);
            try
            {
                packet.AddUInt64(guid);
                packet.AddString(title);
                packet.AddInt32(1);              // Delay
                packet.AddInt32(1);              // Emote
                packet.AddInt8((byte)questMenu.IDs.Count); // Count
                try
                {
                    for (int i = 0, loopTo = questMenu.IDs.Count - 1; i <= loopTo; i++)
                    {
                        packet.AddInt32(Conversions.ToInteger(questMenu.IDs[i]));
                        packet.AddInt32(Conversions.ToInteger(questMenu.Icons[i]));
                        packet.AddInt32(Conversions.ToInteger(questMenu.Levels[i]));
                        packet.AddString(Conversions.ToString(questMenu.Names[i]));
                    }
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "GetQuestMenu Failed: ", ex.ToString());
                }

                objCharacter.client.Send(ref packet);
            }
            finally
            {
                packet.Dispose();
            }
        }

        /// <summary>
        /// Sends the quest details.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="quest">The quest.</param>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="acceptActive">if set to <c>true</c> [accept active].</param>
        public void SendQuestDetails(ref WS_Network.ClientClass client, ref WS_QuestInfo quest, ulong guid, bool acceptActive)
        {
            var packet = new Packets.PacketClass(OPCODES.SMSG_QUESTGIVER_QUEST_DETAILS);
            try
            {
                packet.AddUInt64(guid);

                // QuestDetails
                packet.AddInt32(quest.ID);
                packet.AddString(quest.Title);
                packet.AddString(quest.TextDescription);
                packet.AddString(quest.TextObjectives);
                packet.AddInt32(acceptActive ? 1 : 0);

                // QuestRewards (Choosable)
                int questRewardsCount = 0;
                try
                {
                    for (int i = 0, loopTo = (int)QuestInfo.QUEST_REWARD_CHOICES_COUNT; i <= loopTo; i++)
                    {
                        if (quest.RewardItems[i] != 0)
                            questRewardsCount += 1;
                    }
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "SendQuestDetails Failed: ", ex.ToString());
                }

                packet.AddInt32(questRewardsCount);
                try
                {
                    for (int i = 0, loopTo1 = (int)QuestInfo.QUEST_REWARD_CHOICES_COUNT; i <= loopTo1; i++)
                    {
                        if (quest.RewardItems[i] != 0)
                        {
                            // Add item if not loaded into server
                            if (!WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(quest.RewardItems[i]))
                            {
                                var tmpItem = new WS_Items.ItemInfo(quest.RewardItems[i]);
                                packet.AddInt32(tmpItem.Id);
                            }
                            else
                            {
                                packet.AddInt32(quest.RewardItems[i]);
                            }

                            packet.AddInt32(quest.RewardItems_Count[i]);
                            packet.AddInt32(WorldServiceLocator._WorldServer.ITEMDatabase[quest.RewardItems[i]].Model);
                        }
                        else
                        {
                            packet.AddInt32(0);
                            packet.AddInt32(0);
                            packet.AddInt32(0);
                        }
                    }
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "SendQuestDetails Failed: ", ex.ToString());
                }
                // QuestRewards (Static)
                questRewardsCount = 0;
                for (int i = 0, loopTo2 = (int)QuestInfo.QUEST_REWARDS_COUNT; i <= loopTo2; i++)
                {
                    if (quest.RewardStaticItems[i] != 0)
                        questRewardsCount += 1;
                }

                packet.AddInt32(questRewardsCount);
                try
                {
                    for (int i = 0, loopTo3 = (int)QuestInfo.QUEST_REWARDS_COUNT; i <= loopTo3; i++)
                    {
                        if (quest.RewardStaticItems[i] != 0)
                        {
                            // Add item if not loaded into server
                            if (!WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(quest.RewardStaticItems[i]))
                            {
                                var tmpItem = new WS_Items.ItemInfo(quest.RewardStaticItems[i]);
                                packet.AddInt32(tmpItem.Id);
                            }
                            else
                            {
                                packet.AddInt32(quest.RewardStaticItems[i]);
                            }

                            packet.AddInt32(quest.RewardStaticItems_Count[i]);
                            packet.AddInt32(WorldServiceLocator._WorldServer.ITEMDatabase[quest.RewardStaticItems[i]].Model);
                        }
                        else
                        {
                            packet.AddInt32(0);
                            packet.AddInt32(0);
                            packet.AddInt32(0);
                        }
                    }
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "SendQuestDetails Failed: ", ex.ToString());
                }

                packet.AddInt32(quest.RewardGold);
                questRewardsCount = 0;
                for (int i = 0, loopTo4 = quest.ObjectivesItem.GetUpperBound(0); i <= loopTo4; i++) // QuestInfo.QUEST_OBJECTIVES_COUNT
                {
                    if (quest.ObjectivesItem[i] != 0)
                        questRewardsCount += 1;
                }

                packet.AddInt32(questRewardsCount);
                for (int i = 0, loopTo5 = quest.ObjectivesItem.GetUpperBound(0); i <= loopTo5; i++) // QuestInfo.QUEST_OBJECTIVES_COUNT
                {
                    // Add item if not loaded into server
                    if (quest.ObjectivesItem[i] != 0 && WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(quest.ObjectivesItem[i]) == false)
                    {
                        var tmpItem = new WS_Items.ItemInfo(quest.ObjectivesItem[i]);
                        packet.AddInt32(tmpItem.Id);
                    }
                    else
                    {
                        packet.AddInt32(quest.ObjectivesItem[i]);
                    }

                    packet.AddInt32(quest.ObjectivesItem_Count[i]);
                }

                questRewardsCount = 0;
                for (int i = 0, loopTo6 = quest.ObjectivesItem.GetUpperBound(0); i <= loopTo6; i++) // QuestInfo.QUEST_OBJECTIVES_COUNT
                {
                    if (quest.ObjectivesKill[i] != 0)
                        questRewardsCount += 1;
                }

                packet.AddInt32(questRewardsCount);
                for (int i = 0, loopTo7 = quest.ObjectivesItem.GetUpperBound(0); i <= loopTo7; i++) // QuestInfo.QUEST_OBJECTIVES_COUNT
                {
                    packet.AddUInt32((uint)quest.ObjectivesKill[i]);
                    packet.AddInt32(quest.ObjectivesKill_Count[i]);
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_QUESTGIVER_QUEST_DETAILS [GUID={2:X} Quest={3}]", client.IP, client.Port, guid, quest.ID);

                // Finishing
                client.Send(ref packet);
            }
            finally
            {
                packet.Dispose();
            }
        }

        /// <summary>
        /// Sends the quest.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="quest">The quest.</param>
        public void SendQuest(ref WS_Network.ClientClass client, ref WS_QuestInfo quest)
        {
            var packet = new Packets.PacketClass(OPCODES.SMSG_QUEST_QUERY_RESPONSE);
            try
            {
                packet.AddUInt32((uint)quest.ID);

                // Basic Details
                packet.AddUInt32(quest.Level_Start);
                packet.AddUInt32((uint)quest.Level_Normal);
                packet.AddUInt32((uint)quest.ZoneOrSort);
                packet.AddUInt32((uint)quest.Type);
                packet.AddUInt32((uint)quest.ObjectiveRepFaction);
                packet.AddUInt32((uint)quest.ObjectiveRepStanding);
                packet.AddUInt32(0U);
                packet.AddUInt32(0U);
                packet.AddUInt32((uint)quest.NextQuestInChain);
                packet.AddUInt32((uint)quest.RewardGold); // Negative is required money
                packet.AddUInt32((uint)quest.RewMoneyMaxLevel);
                if (quest.RewardSpell > 0)
                {
                    if (WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(quest.RewardSpell))
                    {
                        if (WorldServiceLocator._WS_Spells.SPELLs[quest.RewardSpell].SpellEffects[0] is object && WorldServiceLocator._WS_Spells.SPELLs[quest.RewardSpell].SpellEffects[0].ID == SpellEffects_Names.SPELL_EFFECT_LEARN_SPELL)
                        {
                            packet.AddUInt32((uint)WorldServiceLocator._WS_Spells.SPELLs[quest.RewardSpell].SpellEffects[0].TriggerSpell);
                        }
                        else
                        {
                            packet.AddUInt32((uint)quest.RewardSpell);
                        }
                    }
                    else
                    {
                        packet.AddUInt32(0U);
                    }
                }
                else
                {
                    packet.AddUInt32(0U);
                }

                packet.AddUInt32((uint)quest.ObjectivesDeliver); // Item given at the start of a quest (srcItem)
                packet.AddUInt32((uint)(quest.QuestFlags & 0xFFFF));
                try
                {
                    for (int i = 0, loopTo = (int)QuestInfo.QUEST_REWARDS_COUNT; i <= loopTo; i++)
                    {
                        packet.AddUInt32((uint)quest.RewardStaticItems[i]);
                        packet.AddUInt32((uint)quest.RewardStaticItems_Count[i]);
                    }
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "SendQuest Failed: ", ex.ToString());
                }

                try
                {
                    for (int i = 0, loopTo1 = (int)QuestInfo.QUEST_REWARD_CHOICES_COUNT; i <= loopTo1; i++)
                    {
                        packet.AddUInt32((uint)quest.RewardItems[i]);
                        packet.AddUInt32((uint)quest.RewardItems_Count[i]);
                    }
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "SendQuest Failed: ", ex.ToString());
                }

                packet.AddUInt32((uint)quest.PointMapID);       // Point MapID
                packet.AddSingle(quest.PointX);          // Point X
                packet.AddSingle(quest.PointY);          // Point Y
                packet.AddUInt32((uint)quest.PointOpt);         // Point Opt

                // Texts
                packet.AddString(quest.Title);
                packet.AddString(quest.TextObjectives);
                packet.AddString(quest.TextDescription);
                packet.AddString(quest.TextEnd);

                // Objectives
                for (int i = 0, loopTo2 = (int)(QuestInfo.QUEST_OBJECTIVES_COUNT - 1); i <= loopTo2; i++)
                {
                    packet.AddUInt32((uint)quest.ObjectivesKill[i]);
                    packet.AddUInt32((uint)quest.ObjectivesKill_Count[i]);
                    packet.AddUInt32((uint)quest.ObjectivesItem[i]);
                    packet.AddUInt32((uint)quest.ObjectivesItem_Count[i]);

                    // HACK: Fix for not showing "Unknown Item" (sometimes client doesn't get items on time)
                    if (quest.ObjectivesItem[i] != 0)
                        WorldServiceLocator._WS_Items.SendItemInfo(ref client, quest.ObjectivesItem[i]);
                }

                for (int i = 0, loopTo3 = (int)(QuestInfo.QUEST_OBJECTIVES_COUNT - 1); i <= loopTo3; i++)
                    packet.AddString(quest.ObjectivesText[i]);
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_QUEST_QUERY_RESPONSE [Quest={2}]", client.IP, client.Port, quest.ID);

                // Finishing
                client.Send(ref packet);
            }
            catch (Exception ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SendQuest Failed [Quest={2}] {3}", client.IP, client.Port, quest.ID, ex.ToString());
            }
            finally
            {
                packet.Dispose();
            }
        }

        /// <summary>
        /// Sends the quest message add item.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="itemID">The item identifier.</param>
        /// <param name="itemCount">The item count.</param>
        public void SendQuestMessageAddItem(ref WS_Network.ClientClass client, int itemID, int itemCount)
        {
            var packet = new Packets.PacketClass(OPCODES.SMSG_QUESTUPDATE_ADD_ITEM);
            try
            {
                packet.AddInt32(itemID);
                packet.AddInt32(itemCount);
                client.Send(ref packet);
            }
            finally
            {
                packet.Dispose();
            }
        }

        /// <summary>
        /// Sends the quest message add kill.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="questID">The quest identifier.</param>
        /// <param name="killGuid">The kill unique identifier.</param>
        /// <param name="killID">The kill identifier.</param>
        /// <param name="killCurrentCount">The kill current count.</param>
        /// <param name="killCount">The kill count.</param>
        public void SendQuestMessageAddKill(ref WS_Network.ClientClass client, int questID, ulong killGuid, int killID, int killCurrentCount, int killCount)
        {
            // Message: %s slain: %d/%d
            var packet = new Packets.PacketClass(OPCODES.SMSG_QUESTUPDATE_ADD_KILL);
            try
            {
                packet.AddInt32(questID);
                if (killID < 0)
                    killID = (int)(-killID | 0x80000000); // Gameobject
                packet.AddInt32(killID);
                packet.AddInt32(killCurrentCount);
                packet.AddInt32(killCount);
                packet.AddUInt64(killGuid);
                client.Send(ref packet);
            }
            finally
            {
                packet.Dispose();
            }
        }

        /// <summary>
        /// Sends the quest message failed.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="questID">The quest identifier.</param>
        public void SendQuestMessageFailed(ref WS_Network.ClientClass client, int questID)
        {
            // Message: ?
            var packet = new Packets.PacketClass(OPCODES.SMSG_QUESTGIVER_QUEST_FAILED);
            try
            {
                packet.AddInt32(questID);
                // TODO: Need to add failed reason to packet here
                client.Send(ref packet);
            }
            finally
            {
                packet.Dispose();
            }
        }

        /// <summary>
        /// Sends the quest message failed timer.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="questID">The quest identifier.</param>
        public void SendQuestMessageFailedTimer(ref WS_Network.ClientClass client, int questID)
        {
            // Message: ?
            var packet = new Packets.PacketClass(OPCODES.SMSG_QUESTUPDATE_FAILEDTIMER);
            try
            {
                packet.AddInt32(questID);
                client.Send(ref packet);
            }
            finally
            {
                packet.Dispose();
            }
        }

        /// <summary>
        /// Sends the quest message complete.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="questID">The quest identifier.</param>
        public void SendQuestMessageComplete(ref WS_Network.ClientClass client, int questID)
        {
            // Message: Objective Complete.
            var packet = new Packets.PacketClass(OPCODES.SMSG_QUESTUPDATE_COMPLETE);
            try
            {
                packet.AddInt32(questID);
                client.Send(ref packet);
            }
            finally
            {
                packet.Dispose();
            }
        }

        /// <summary>
        /// Sends the quest complete.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="quest">The quest.</param>
        /// <param name="xP">The xp.</param>
        /// <param name="gold">The gold.</param>
        public void SendQuestComplete(ref WS_Network.ClientClass client, ref WS_QuestInfo quest, int xP, int gold)
        {
            var packet = new Packets.PacketClass(OPCODES.SMSG_QUESTGIVER_QUEST_COMPLETE);
            try
            {
                packet.AddInt32(quest.ID);
                packet.AddInt32(3);
                packet.AddInt32(xP);
                packet.AddInt32(gold);
                packet.AddInt32(quest.RewardHonor); // bonus honor...used in BG quests
                int rewardsCount = 0;
                for (int i = 0, loopTo = (int)QuestInfo.QUEST_REWARDS_COUNT; i <= loopTo; i++)
                {
                    if (quest.RewardStaticItems[i] > 0)
                        rewardsCount += 1;
                }

                packet.AddInt32(rewardsCount);
                for (int i = 0, loopTo1 = (int)QuestInfo.QUEST_REWARDS_COUNT; i <= loopTo1; i++)
                {
                    if (quest.RewardStaticItems[i] > 0)
                    {
                        packet.AddInt32(quest.RewardStaticItems[i]);
                        packet.AddInt32(quest.RewardStaticItems_Count[i]);
                    }
                }

                client.Send(ref packet);
            }
            finally
            {
                packet.Dispose();
            }
        }

        /// <summary>
        /// Sends the quest reward.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="quest">The quest.</param>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="objBaseQuest">The Base Quest.</param>
        public void SendQuestReward(ref WS_Network.ClientClass client, ref WS_QuestInfo quest, ulong guid, ref WS_QuestsBase objBaseQuest)
        {
            var packet = new Packets.PacketClass(OPCODES.SMSG_QUESTGIVER_OFFER_REWARD);
            try
            {
                packet.AddUInt64(guid);
                packet.AddInt32(objBaseQuest.ID);
                packet.AddString(objBaseQuest.Title);
                packet.AddString(quest.TextComplete);
                packet.AddInt32(Conversions.ToInteger(objBaseQuest.Complete));     // EnbleNext
                int emoteCount = 0;
                for (int i = 0; i <= 3; i++)
                {
                    if (quest.OfferRewardEmote[i] <= 0)
                        continue;
                    emoteCount += 1;
                }

                packet.AddInt32(emoteCount);
                for (int i = 0, loopTo = emoteCount - 1; i <= loopTo; i++)
                {
                    packet.AddInt32(0); // EmoteDelay
                    packet.AddInt32(quest.OfferRewardEmote[i]);
                }

                int questRewardsCount = 0;
                for (int i = 0, loopTo1 = (int)QuestInfo.QUEST_REWARD_CHOICES_COUNT; i <= loopTo1; i++)
                {
                    if (quest.RewardItems[i] != 0)
                        questRewardsCount += 1;
                }

                packet.AddInt32(questRewardsCount);
                for (int i = 0, loopTo2 = (int)QuestInfo.QUEST_REWARD_CHOICES_COUNT; i <= loopTo2; i++)
                {
                    if (quest.RewardItems[i] != 0)
                    {
                        packet.AddInt32(quest.RewardItems[i]);
                        packet.AddInt32(quest.RewardItems_Count[i]);

                        // Add item if not loaded into server
                        if (!WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(quest.RewardItems[i]))
                        {
                            var tmpItem = new WS_Items.ItemInfo(quest.RewardItems[i]);
                            packet.AddInt32(tmpItem.Model);
                        }
                        else
                        {
                            packet.AddInt32(WorldServiceLocator._WorldServer.ITEMDatabase[quest.RewardItems[i]].Model);
                        }
                    }
                }

                questRewardsCount = 0;
                for (int i = 0, loopTo3 = (int)QuestInfo.QUEST_REWARDS_COUNT; i <= loopTo3; i++)
                {
                    if (quest.RewardStaticItems[i] != 0)
                        questRewardsCount += 1;
                }

                packet.AddInt32(questRewardsCount);
                for (int i = 0, loopTo4 = (int)QuestInfo.QUEST_REWARDS_COUNT; i <= loopTo4; i++)
                {
                    if (quest.RewardStaticItems[i] != 0)
                    {
                        packet.AddInt32(quest.RewardStaticItems[i]);
                        packet.AddInt32(quest.RewardStaticItems_Count[i]);

                        // Add item if not loaded into server
                        if (!WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(quest.RewardStaticItems[i]))
                        {
                            // TODO: Another one of these useless bits of code, needs to be implemented correctly
                            var tmpItem = new WS_Items.ItemInfo(quest.RewardStaticItems[i]);
                        }

                        packet.AddInt32(WorldServiceLocator._WorldServer.ITEMDatabase[quest.RewardStaticItems[i]].Model);
                    }
                }

                packet.AddInt32(quest.RewardGold);
                packet.AddInt32(0);
                if (quest.RewardSpell > 0)
                {
                    if (WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(quest.RewardSpell))
                    {
                        if (WorldServiceLocator._WS_Spells.SPELLs[quest.RewardSpell].SpellEffects[0] is object && WorldServiceLocator._WS_Spells.SPELLs[quest.RewardSpell].SpellEffects[0].ID == SpellEffects_Names.SPELL_EFFECT_LEARN_SPELL)
                        {
                            packet.AddInt32(WorldServiceLocator._WS_Spells.SPELLs[quest.RewardSpell].SpellEffects[0].TriggerSpell);
                        }
                        else
                        {
                            packet.AddInt32(quest.RewardSpell);
                        }
                    }
                    else
                    {
                        packet.AddInt32(0);
                    }
                }
                else
                {
                    packet.AddInt32(0);
                }

                client.Send(ref packet);
            }
            finally
            {
                packet.Dispose();
            }
        }

        /// <summary>
        /// Sends the quest required items.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="quest">The quest.</param>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="objBaseQuest">The Base Quests.</param>
        public void SendQuestRequireItems(ref WS_Network.ClientClass client, ref WS_QuestInfo quest, ulong guid, ref WS_QuestsBase objBaseQuest)
        {
            var packet = new Packets.PacketClass(OPCODES.SMSG_QUESTGIVER_REQUEST_ITEMS);
            try
            {
                packet.AddUInt64(guid);
                packet.AddInt32(objBaseQuest.ID);
                packet.AddString(objBaseQuest.Title);
                packet.AddString(quest.TextIncomplete);
                packet.AddInt32(0); // Unknown
                if (objBaseQuest.Complete)
                {
                    packet.AddInt32(quest.CompleteEmote);
                }
                else
                {
                    packet.AddInt32(quest.IncompleteEmote);
                }

                packet.AddInt32(0);                      // Close Window on Cancel (1 = true / 0 = false)
                if (quest.RewardGold < 0)
                {
                    packet.AddInt32(-quest.RewardGold);   // Required Money
                }
                else
                {
                    packet.AddInt32(0);
                }

                // DONE: Count the required items
                byte requiredItemsCount = 0;
                for (int i = 0, loopTo = quest.ObjectivesItem.GetUpperBound(0); i <= loopTo; i++) // QuestInfo.QUEST_OBJECTIVES_COUNT
                {
                    if (quest.ObjectivesItem[i] != 0)
                        requiredItemsCount = (byte)(requiredItemsCount + 1);
                }

                packet.AddInt32(requiredItemsCount);

                // DONE: List items
                if (requiredItemsCount > 0)
                {
                    for (int i = 0, loopTo1 = quest.ObjectivesItem.GetUpperBound(0); i <= loopTo1; i++) // QuestInfo.QUEST_OBJECTIVES_COUNT
                    {
                        if (quest.ObjectivesItem[i] != 0)
                        {
                            if (WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(quest.ObjectivesItem[i]) == false)
                            {
                                var tmpItem = new WS_Items.ItemInfo(quest.ObjectivesItem[i]);
                                packet.AddInt32(tmpItem.Id);
                            }
                            else
                            {
                                packet.AddInt32(quest.ObjectivesItem[i]);
                            }

                            packet.AddInt32(quest.ObjectivesItem_Count[i]);
                            if (WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(quest.ObjectivesItem[i]))
                            {
                                packet.AddInt32(WorldServiceLocator._WorldServer.ITEMDatabase[quest.ObjectivesItem[i]].Model);
                            }
                            else
                            {
                                packet.AddInt32(0);
                            }
                        }
                    }
                }

                packet.AddInt32(2);
                if (objBaseQuest.Complete)
                {
                    packet.AddInt32(3);
                }
                else
                {
                    packet.AddInt32(0);
                }

                packet.AddInt32(0x4);
                packet.AddInt32(0x8);
                packet.AddInt32(0x10);
                client.Send(ref packet);
            }
            finally
            {
                packet.Dispose();
            }
        }

        /// <summary>
        /// Loads the quests.
        /// </summary>
        /// <param name="objCharacter">The Character.</param>
        public void LoadQuests(ref WS_PlayerData.CharacterObject objCharacter)
        {
            var cQuests = new DataTable();
            int i = 0;
            WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT quest_id, quest_status FROM characters_quests q WHERE q.char_guid = {0};", (object)objCharacter.GUID), cQuests);
            foreach (DataRow cRow in cQuests.Rows)
            {
                int questID = Conversions.ToInteger(cRow["quest_id"]);
                int questStatus = Conversions.ToInteger(cRow["quest_status"]);
                if (questStatus >= 0)    // Outstanding Quest
                {
                    if (IsValidQuest(questID) == true)
                    {
                        WS_QuestInfo tmpQuest;
                        tmpQuest = ReturnQuestInfoById(questID);

                        // DONE: Initialize quest info
                        CreateQuest(ref objCharacter.TalkQuests[i], ref tmpQuest);
                        objCharacter.TalkQuests[i].LoadState(questStatus);
                        objCharacter.TalkQuests[i].Slot = (byte)i;
                        objCharacter.TalkQuests[i].UpdateItemCount(ref objCharacter);
                        i += 1;
                    }
                }
                else if (questStatus == -1) // Completed
                {
                    objCharacter.QuestsCompleted.Add(questID);
                }
            }
        }

        /// <summary>
        /// Creates the quest.
        /// </summary>
        /// <param name="objBaseQuest">The Base Quest.</param>
        /// <param name="tmpQuest">The temporary quest.</param>
        public void CreateQuest(ref WS_QuestsBase objBaseQuest, ref WS_QuestInfo tmpQuest)
        {
            // Initialize Quest
            objBaseQuest = new WS_QuestsBase(tmpQuest);
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */        // DONE: Kill quest events
                                                               /// <summary>
        /// Called when [quest kill].
        /// </summary>
        /// <param name="objCharacter">The Character.</param>
        /// <param name="creature">The creature.</param>
        public void OnQuestKill(ref WS_PlayerData.CharacterObject objCharacter, ref WS_Creatures.CreatureObject creature)
        {
            // HANDLERS: Added to DealDamage sub

            // DONE: Do not count killed from guards
            if (objCharacter is null)
                return;

            // DONE: Count kills
            for (int i = 0, loopTo = (int)QuestInfo.QUEST_SLOTS; i <= loopTo; i++)
            {
                if (objCharacter.TalkQuests[i] is object && objCharacter.TalkQuests[i].ObjectiveFlags & QuestObjectiveFlag.QUEST_OBJECTIVE_KILL && (objCharacter.TalkQuests[i].ObjectiveFlags & QuestObjectiveFlag.QUEST_OBJECTIVE_CAST) == 0)
                {
                    if (objCharacter.TalkQuests[i] is WS_QuestsBaseScripted)
                    {
                        ((WS_QuestsBaseScripted)objCharacter.TalkQuests[i]).OnQuestKill(ref objCharacter, ref creature);
                    }
                    else
                    {
                        {
                            var withBlock = objCharacter.TalkQuests[i];
                            for (byte j = 0; j <= 3; j++)
                            {
                                if (withBlock.ObjectivesType[j] == QuestObjectiveFlag.QUEST_OBJECTIVE_KILL && withBlock.ObjectivesObject[j] == creature.ID)
                                {
                                    if (withBlock.Progress[j] < withBlock.ObjectivesCount[j])
                                    {
                                        withBlock.AddKill(objCharacter, j, creature.GUID);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return;  // For now next is disabled

            // DONE: Check all in objChar's party for that quest
            foreach (ulong guid in objCharacter.Group.LocalMembers)
            {
                if (guid == objCharacter.GUID)
                    continue;
                {
                    var withBlock1 = WorldServiceLocator._WorldServer.CHARACTERs[guid];
                    for (int i = 0, loopTo1 = (int)QuestInfo.QUEST_SLOTS; i <= loopTo1; i++)
                    {
                        if (withBlock1.TalkQuests[i] is object && withBlock1.TalkQuests[i].ObjectiveFlags & QuestObjectiveFlag.QUEST_OBJECTIVE_KILL && (withBlock1.TalkQuests[i].ObjectiveFlags & QuestObjectiveFlag.QUEST_OBJECTIVE_CAST) == 0)
                        {
                            {
                                var withBlock2 = withBlock1.TalkQuests[i];
                                for (byte j = 0; j <= 3; j++)
                                {
                                    if (withBlock2.ObjectivesType[j] == QuestObjectiveFlag.QUEST_OBJECTIVE_KILL && withBlock2.ObjectivesObject[j] == creature.ID)
                                    {
                                        if (withBlock2.Progress[j] < withBlock2.ObjectivesCount[j])
                                        {
                                            withBlock2.AddKill(objCharacter, j, creature.GUID);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called when [quest cast spell].
        /// </summary>
        /// <param name="objCharacter">The Character.</param>
        /// <param name="creature">The creature.</param>
        /// <param name="spellID">The spell identifier.</param>
        public void OnQuestCastSpell(ref WS_PlayerData.CharacterObject objCharacter, ref WS_Creatures.CreatureObject creature, int spellID)
        {
            // DONE: Count spell casts
            // DONE: Check if we're casting it on the correct creature
            for (int i = 0, loopTo = (int)QuestInfo.QUEST_SLOTS; i <= loopTo; i++)
            {
                if (objCharacter.TalkQuests[i] is object && objCharacter.TalkQuests[i].ObjectiveFlags & QuestObjectiveFlag.QUEST_OBJECTIVE_CAST)
                {
                    if (objCharacter.TalkQuests[i] is WS_QuestsBaseScripted)
                    {
                        ((WS_QuestsBaseScripted)objCharacter.TalkQuests[i]).OnQuestCastSpell(ref objCharacter, ref creature, spellID);
                    }
                    else
                    {
                        {
                            var withBlock = objCharacter.TalkQuests[i];
                            for (byte j = 0; j <= 3; j++)
                            {
                                if (withBlock.ObjectivesType[j] == QuestObjectiveFlag.QUEST_OBJECTIVE_KILL && withBlock.ObjectivesSpell[j] == spellID)
                                {
                                    if (withBlock.ObjectivesObject[j] == 0 || withBlock.ObjectivesObject[j] == creature.ID)
                                    {
                                        if (withBlock.Progress[j] < withBlock.ObjectivesCount[j])
                                        {
                                            withBlock.AddCast(objCharacter, j, creature.GUID);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called when [quest cast spell].
        /// </summary>
        /// <param name="objCharacter">The Character.</param>
        /// <param name="gameObject">The game object.</param>
        /// <param name="spellID">The spell identifier.</param>
        public void OnQuestCastSpell(ref WS_PlayerData.CharacterObject objCharacter, ref WS_GameObjects.GameObjectObject gameObject, int spellID)
        {
            // DONE: Count spell casts
            // DONE: Check if we're casting it on the correct gameobject
            for (int i = 0, loopTo = (int)QuestInfo.QUEST_SLOTS; i <= loopTo; i++)
            {
                if (objCharacter.TalkQuests[i] is object && objCharacter.TalkQuests[i].ObjectiveFlags & QuestObjectiveFlag.QUEST_OBJECTIVE_CAST)
                {
                    if (objCharacter.TalkQuests[i] is WS_QuestsBaseScripted)
                    {
                        ((WS_QuestsBaseScripted)objCharacter.TalkQuests[i]).OnQuestCastSpell(ref objCharacter, ref gameObject, spellID);
                    }
                    else
                    {
                        {
                            var withBlock = objCharacter.TalkQuests[i];
                            for (byte j = 0; j <= 3; j++)
                            {
                                if (withBlock.ObjectivesType[j] == QuestObjectiveFlag.QUEST_OBJECTIVE_KILL && withBlock.ObjectivesSpell[j] == spellID)
                                {
                                    // NOTE: GameObjects are negative here!
                                    if (withBlock.ObjectivesObject[j] == 0 || withBlock.ObjectivesObject[j] == -gameObject.ID)
                                    {
                                        if (withBlock.Progress[j] < withBlock.ObjectivesCount[j])
                                        {
                                            withBlock.AddCast(objCharacter, j, gameObject.GUID);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called when [quest do emote].
        /// </summary>
        /// <param name="objCharacter">The Character.</param>
        /// <param name="creature">The creature.</param>
        /// <param name="emoteID">The emote identifier.</param>
        public void OnQuestDoEmote(ref WS_PlayerData.CharacterObject objCharacter, ref WS_Creatures.CreatureObject creature, int emoteID)
        {
            byte j;

            // DONE: Count spell casts
            // DONE: Check if we're casting it on the correct gameobject
            for (int i = 0, loopTo = (int)QuestInfo.QUEST_SLOTS; i <= loopTo; i++)
            {
                if (objCharacter.TalkQuests[i] is object && objCharacter.TalkQuests[i].ObjectiveFlags & QuestObjectiveFlag.QUEST_OBJECTIVE_EMOTE)
                {
                    if (objCharacter.TalkQuests[i] is WS_QuestsBaseScripted)
                    {
                        ((WS_QuestsBaseScripted)objCharacter.TalkQuests[i]).OnQuestEmote(ref objCharacter, ref creature, emoteID);
                    }
                    else
                    {
                        {
                            var withBlock = objCharacter.TalkQuests[i];
                            for (j = 0; j <= 3; j++)
                            {
                                if (withBlock.ObjectivesType[j] == QuestObjectiveFlag.QUEST_OBJECTIVE_EMOTE && withBlock.ObjectivesSpell[j] == emoteID)
                                {
                                    // NOTE: GameObjects are negative here!
                                    if (withBlock.ObjectivesObject[j] == 0 || withBlock.ObjectivesObject[j] == creature.ID)
                                    {
                                        if (withBlock.Progress[j] < withBlock.ObjectivesCount[j])
                                        {
                                            withBlock.AddEmote(objCharacter, j);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether the item is needed for quest for the specified character.
        /// </summary>
        /// <param name="objCharacter">The Character.</param>
        /// <param name="itemEntry">The item entry.</param>
        /// <returns></returns>
        public bool IsItemNeededForQuest(ref WS_PlayerData.CharacterObject objCharacter, ref int itemEntry)
        {

            // DONE: Check if anyone in the group has the quest that requires this item
            // DONE: If the quest isn't a raid quest then you can't loot quest items
            bool isRaid;
            isRaid = objCharacter.IsInRaid;
            if (objCharacter.IsInGroup)
            {
                foreach (ulong guid in objCharacter.Group.LocalMembers)
                {
                    {
                        var withBlock = WorldServiceLocator._WorldServer.CHARACTERs[guid];
                        for (int j = 0, loopTo = (int)QuestInfo.QUEST_SLOTS; j <= loopTo; j++)
                        {
                            if (withBlock.TalkQuests[j] is object && withBlock.TalkQuests[j].ObjectiveFlags & QuestObjectiveFlag.QUEST_OBJECTIVE_ITEM && isRaid == false)
                            {
                                {
                                    var withBlock1 = withBlock.TalkQuests[j];
                                    for (byte k = 0; k <= 3; k++)
                                    {
                                        if (withBlock1.ObjectivesItem[k] == itemEntry)
                                        {
                                            if (withBlock1.ProgressItem[k] < withBlock1.ObjectivesItemCount[k])
                                                return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                for (int j = 0, loopTo1 = (int)QuestInfo.QUEST_SLOTS; j <= loopTo1; j++)
                {
                    if (objCharacter.TalkQuests[j] is object && objCharacter.TalkQuests[j].ObjectiveFlags & QuestObjectiveFlag.QUEST_OBJECTIVE_ITEM)
                    {
                        {
                            var withBlock2 = objCharacter.TalkQuests[j];
                            for (byte k = 0; k <= 3; k++)
                            {
                                if (withBlock2.ObjectivesItem[k] == itemEntry)
                                {
                                    if (withBlock2.ProgressItem[k] < withBlock2.ObjectivesItemCount[k])
                                        return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified gameobject is used for quest.
        /// </summary>
        /// <param name="gameobject">The gameobject.</param>
        /// <param name="objCharacter">The obj character.</param>
        /// <returns></returns>
        public byte IsGameObjectUsedForQuest(ref WS_GameObjects.GameObjectObject gameobject, ref WS_PlayerData.CharacterObject objCharacter)
        {
            if (!gameobject.IsUsedForQuests)
                return 0;
            foreach (int questItemID in gameobject.IncludesQuestItems)
            {
                // DONE: Check quests needing that item
                for (int i = 0, loopTo = (int)QuestInfo.QUEST_SLOTS; i <= loopTo; i++)
                {
                    if (objCharacter.TalkQuests[i] is object && objCharacter.TalkQuests[i].ObjectiveFlags & QuestObjectiveFlag.QUEST_OBJECTIVE_ITEM)
                    {
                        for (byte j = 0; j <= 3; j++)
                        {
                            if (objCharacter.TalkQuests[i].ObjectivesType[j] == QuestObjectiveFlag.QUEST_OBJECTIVE_ITEM && objCharacter.TalkQuests[i].ObjectivesItem[j] == questItemID)
                            {
                                if (objCharacter.ItemCOUNT(questItemID) < objCharacter.TalkQuests[i].ObjectivesItemCount[j])
                                    return 2;
                            }
                        }
                    }
                }
            }

            return 1;
        }

        // DONE: Quest's loot generation
        public void OnQuestAddQuestLoot(ref WS_PlayerData.CharacterObject objCharacter, ref WS_Creatures.CreatureObject creature, ref WS_Loot.LootObject loot)
        {
            // HANDLERS: Added in loot generation sub

            // TODO: Check for quest loots for adding to looted creature
        }

        public void OnQuestAddQuestLoot(ref WS_PlayerData.CharacterObject objCharacter, ref WS_GameObjects.GameObjectObject gameObject, ref WS_Loot.LootObject loot)
        {
            // HANDLERS: None
            // TODO: Check for quest loots for adding to looted gameObject
        }

        public void OnQuestAddQuestLoot(ref WS_PlayerData.CharacterObject objCharacter, ref WS_PlayerData.CharacterObject character, ref WS_Loot.LootObject loot)
        {
            // HANDLERS: None
            // TODO: Check for quest loots for adding to looted player (used only in battleground?)
        }

        // DONE: Item quest events
        public void OnQuestItemAdd(ref WS_PlayerData.CharacterObject objCharacter, int itemID, byte count)
        {
            // HANDLERS: Added to looting sub

            if (count == 0)
                count = 1;

            // DONE: Check quests needing that item
            for (int i = 0, loopTo = (int)QuestInfo.QUEST_SLOTS; i <= loopTo; i++)
            {
                if (objCharacter.TalkQuests[i] is object && objCharacter.TalkQuests[i].ObjectiveFlags & QuestObjectiveFlag.QUEST_OBJECTIVE_ITEM)
                {
                    if (objCharacter.TalkQuests[i] is WS_QuestsBaseScripted)
                    {
                        ((WS_QuestsBaseScripted)objCharacter.TalkQuests[i]).OnQuestItem(ref objCharacter, itemID, count);
                    }
                    else
                    {
                        {
                            var withBlock = objCharacter.TalkQuests[i];
                            for (int j = 0; j <= 3; j++)
                            {
                                if (withBlock.ObjectivesItem[j] == itemID)
                                {
                                    if (withBlock.ProgressItem[j] < withBlock.ObjectivesItemCount[j])
                                    {
                                        withBlock.AddItem(objCharacter, (byte)j, count);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void OnQuestItemRemove(ref WS_PlayerData.CharacterObject objCharacter, int itemID, byte count)
        {
            // HANDLERS: Added to delete sub
            if (count == 0)
                count = 1;

            // DONE: Check quests needing that item
            for (int i = 0, loopTo = (int)QuestInfo.QUEST_SLOTS; i <= loopTo; i++)
            {
                if (objCharacter.TalkQuests[i] is object && objCharacter.TalkQuests[i].ObjectiveFlags & QuestObjectiveFlag.QUEST_OBJECTIVE_ITEM)
                {
                    if (objCharacter.TalkQuests[i] is WS_QuestsBaseScripted)
                    {
                        ((WS_QuestsBaseScripted)objCharacter.TalkQuests[i]).OnQuestItem(ref objCharacter, itemID, -count);
                    }
                    else
                    {
                        {
                            var withBlock = objCharacter.TalkQuests[i];
                            for (byte j = 0; j <= 3; j++)
                            {
                                if (withBlock.ObjectivesItem[j] == itemID)
                                {
                                    if (withBlock.ProgressItem[j] > 0)
                                    {
                                        withBlock.RemoveItem(objCharacter, j, count);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // DONE: Exploration quest events
        public void OnQuestExplore(ref WS_PlayerData.CharacterObject objCharacter, int areaID)
        {
            for (int i = 0, loopTo = (int)QuestInfo.QUEST_SLOTS; i <= loopTo; i++)
            {
                if (objCharacter.TalkQuests[i] is object && objCharacter.TalkQuests[i].ObjectiveFlags & QuestObjectiveFlag.QUEST_OBJECTIVE_EXPLORE)
                {
                    if (objCharacter.TalkQuests[i] is WS_QuestsBaseScripted)
                    {
                        ((WS_QuestsBaseScripted)objCharacter.TalkQuests[i]).OnQuestExplore(ref objCharacter, areaID);
                    }
                    else
                    {
                        {
                            var withBlock = objCharacter.TalkQuests[i];
                            for (byte j = 0; j <= 3; j++)
                            {
                                if (withBlock.ObjectivesExplore[j] == areaID)
                                {
                                    if (withBlock.Explored == false)
                                    {
                                        withBlock.AddExplore(objCharacter);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        /// <summary>
        /// Classes the by quest sort.
        /// </summary>
        /// <param name="questSort">This is the Value from Col 0 of QuestSort.dbc.</param>
        /// <returns></returns>
        public byte ClassByQuestSort(int questSort)
        {
            // TODO: There are many other quest types missing from this list, but present in the DBC
            switch (questSort)
            {
                case 61:
                    {
                        return (byte)Classes.CLASS_WARLOCK;
                    }

                case 81:
                    {
                        return (byte)Classes.CLASS_WARRIOR;
                    }

                case 82:
                    {
                        return (byte)Classes.CLASS_SHAMAN;
                    }

                case 141:
                    {
                        return (byte)Classes.CLASS_PALADIN;
                    }

                case 161:
                    {
                        return (byte)Classes.CLASS_MAGE;
                    }

                case 162:
                    {
                        return (byte)Classes.CLASS_ROGUE;
                    }

                case 261:
                    {
                        return (byte)Classes.CLASS_HUNTER;
                    }

                case 262:
                    {
                        return (byte)Classes.CLASS_PRIEST;
                    }

                case 263:
                    {
                        return (byte)Classes.CLASS_DRUID;
                    }

                default:
                    {
                        return 0;
                    }
            }
        }

        public QuestgiverStatusFlag GetQuestgiverStatus(ref WS_PlayerData.CharacterObject objCharacter, ulong cGuid)
        {
            // DONE: Invoke scripted quest status
            QuestgiverStatusFlag status = QuestgiverStatusFlag.DIALOG_STATUS_NONE;
            // DONE: Do search for completed quests or in progress

            var alreadyHave = new List<int>();
            if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(cGuid) == true)    // Is the GUID a creature (or npc)
            {
                if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(cGuid) == false)
                {
                    status = QuestgiverStatusFlag.DIALOG_STATUS_NONE;
                    return status;
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "QuestStatus ID: {0} NPC Name: {1}", WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].ID, WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].Name);
                // _WorldServer.Log.WriteLine(LogType.CRITICAL, "QuestStatus ID: {0} NPC Name: {1} Has Quest: {2}", _WorldServer.WORLD_CREATUREs(cGuid).ID, _WorldServer.WORLD_CREATUREs(cGuid).Name, IsNothing(_WorldServer.WORLD_CREATUREs(cGuid).CreatureInfo.TalkScript))
                // _WorldServer.Log.WriteLine(LogType.CRITICAL, "Status = {0} {1} {2}", _WorldServer.WORLD_CREATUREs(cGUID).)
                // If IsNothing(_WorldServer.WORLD_CREATUREs(cGUID).CreatureInfo.TalkScript) = False Then    'NPC is a questgiven
                int creatureQuestId;
                creatureQuestId = WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].ID;
                if (IsValidQuest(creatureQuestId) == true)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "QuestStatus ID: {0} Valid Quest: {1}", WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].ID, IsValidQuest(creatureQuestId));
                    if (WorldServiceLocator._WorldServer.CreatureQuestStarters.ContainsKey(creatureQuestId) == true)
                    {
                        foreach (int questID in WorldServiceLocator._WorldServer.CreatureQuestStarters[creatureQuestId])
                        {
                            try
                            {
                                if (WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID).CanSeeQuest(ref objCharacter) == true)
                                {
                                    // If objCharacter.IsQuestInProgress(creatureQuestId) = False Then
                                    // Dim Prequest As Mangos.WorldServer.WS_QuestInfo = _WorldServer.ALLQUESTS.ReturnQuestInfoById(creatureQuestId)
                                    // Prequest.PreQuests.Contains()
                                    // _WorldServer.ALLQUESTS.DoesPreQuestExist(creatureQuestId,

                                    status = QuestgiverStatusFlag.DIALOG_STATUS_AVAILABLE;
                                    return status;
                                    // End If
                                }
                            }
                            catch (Exception)
                            {
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "GetQuestGiverStatus Error");
                            }
                        }

                        if (WorldServiceLocator._WorldServer.CreatureQuestFinishers.ContainsKey(creatureQuestId))
                        {
                            foreach (int questID in WorldServiceLocator._WorldServer.CreatureQuestFinishers[creatureQuestId])
                            {
                                try
                                {
                                    if (WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID).CanSeeQuest(ref objCharacter) == true)
                                    {
                                        if (objCharacter.IsQuestInProgress(questID) == true)
                                        {
                                            status = QuestgiverStatusFlag.DIALOG_STATUS_REWARD;
                                            return status;
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "GetQuestGiverStatus Error");
                                }
                            }
                        }
                    }
                }
                // If _WorldServer.WORLD_CREATUREs(cGUID).CreatureInfo.Id
                // IF cannot see quest, run line below
                status = (QuestgiverStatusFlag)WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].CreatureInfo.TalkScript.OnQuestStatus(ref objCharacter, cGuid);
                return status;
            }
            // End If

            else if (WorldServiceLocator._CommonGlobalFunctions.GuidIsGameObject(cGuid) == true)  // Or is it a worldobject
            {
                if (WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(cGuid) == false)
                {
                    status = QuestgiverStatusFlag.DIALOG_STATUS_NONE;
                    return status;
                }
            }
            else        // everything else doesn't get a marker
            {
                status = QuestgiverStatusFlag.DIALOG_STATUS_NONE;
                return status;
            }

            for (int i = 0, loopTo = (int)QuestInfo.QUEST_SLOTS; i <= loopTo; i++)
            {
                if (objCharacter.TalkQuests[i] is object)
                {
                    alreadyHave.Add(objCharacter.TalkQuests[i].ID);
                    if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(cGuid))
                    {
                        if (WorldServiceLocator._WorldServer.CreatureQuestFinishers.ContainsKey(WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].ID) && WorldServiceLocator._WorldServer.CreatureQuestFinishers[WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].ID].Contains(objCharacter.TalkQuests[i].ID))
                        {
                            if (objCharacter.TalkQuests[i].Complete)
                            {
                                status = QuestgiverStatusFlag.DIALOG_STATUS_REWARD;
                                break;
                            }

                            status = QuestgiverStatusFlag.DIALOG_STATUS_INCOMPLETE;
                        }
                    }
                    else if (WorldServiceLocator._WorldServer.GameobjectQuestFinishers.ContainsKey(WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[cGuid].ID) && WorldServiceLocator._WorldServer.GameobjectQuestFinishers[WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[cGuid].ID].Contains(objCharacter.TalkQuests[i].ID))
                    {
                        if (objCharacter.TalkQuests[i].Complete)
                        {
                            status = QuestgiverStatusFlag.DIALOG_STATUS_REWARD;
                            break;
                        }

                        status = QuestgiverStatusFlag.DIALOG_STATUS_INCOMPLETE;
                    }
                }
            }

            return status;
        }

        public void On_CMSG_QUESTGIVER_STATUS_QUERY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            try
            {
                if (packet.Data.Length - 1 < 13)
                    return;
                packet.GetInt16();
                ulong guid = packet.GetUInt64();
                var status = GetQuestgiverStatus(ref client.Character, guid);
                var response = new Packets.PacketClass(OPCODES.SMSG_QUESTGIVER_STATUS);
                try
                {
                    response.AddUInt64(guid);
                    response.AddUInt32((uint)status);
                    client.Send(ref response);
                }
                finally
                {
                    response.Dispose();
                }
            }
            catch (Exception e)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "On_CMSG_QUESTGIVER_STATUS_QUERY - Error in questgiver status query.{0}", Environment.NewLine + e.ToString());
            }
        }

        public void On_CMSG_QUESTGIVER_HELLO(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            try
            {
                if (packet.Data.Length - 1 < 13)
                    return;
                packet.GetInt16();
                ulong guid = packet.GetUInt64();
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_QUESTGIVER_HELLO [GUID={2:X}]", client.IP, client.Port, guid);
                if (WorldServiceLocator._WorldServer.WORLD_CREATUREs[guid].Evade)
                    return;
                WorldServiceLocator._WorldServer.WORLD_CREATUREs[guid].StopMoving();
                client.Character.RemoveAurasByInterruptFlag((int)SpellAuraInterruptFlags.AURA_INTERRUPT_FLAG_TALK);

                // TODO: There is something here not working all the time :/
                if (WorldServiceLocator._WorldServer.CREATURESDatabase[WorldServiceLocator._WorldServer.WORLD_CREATUREs[guid].ID].TalkScript is null)
                {
                    SendQuestMenu(ref client.Character, guid, "I have some tasks for you, $N.");
                }
                else
                {
                    WorldServiceLocator._WorldServer.CREATURESDatabase[WorldServiceLocator._WorldServer.WORLD_CREATUREs[guid].ID].TalkScript.OnGossipHello(ref client.Character, guid);
                }
            }
            catch (Exception e)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "On_CMSG_QUESTGIVER_HELLO - Error when sending quest menu.{0}", Environment.NewLine + e.ToString());
            }
        }

        public void On_CMSG_QUESTGIVER_QUERY_QUEST(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 17)
                return;
            packet.GetInt16();
            ulong guid = packet.GetUInt64();
            int questID = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_QUESTGIVER_QUERY_QUEST [GUID={2:X} QuestID={3}]", client.IP, client.Port, guid, questID);
            if (!WorldServiceLocator._WorldServer.ALLQUESTS.IsValidQuest(questID))
            {
                var tmpQuest = new WS_QuestInfo(questID);
                try
                {
                    client.Character.TalkCurrentQuest = tmpQuest;
                    SendQuestDetails(ref client, ref client.Character.TalkCurrentQuest, guid, true);
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "On_CMSG_QUESTGIVER_QUERY_QUEST - Error while querying a quest.{0}{1}", Environment.NewLine, ex.ToString());
                }
            }
            else
            {
                try
                {
                    client.Character.TalkCurrentQuest = WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID);
                    SendQuestDetails(ref client, ref client.Character.TalkCurrentQuest, guid, true);
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "On_CMSG_QUESTGIVER_QUERY_QUEST - Error while querying a quest.{0}{1}", Environment.NewLine, ex.ToString());
                }
            }
        }

        public void On_CMSG_QUESTGIVER_ACCEPT_QUEST(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 17)
                return;
            packet.GetInt16();
            ulong guid = packet.GetUInt64();
            int questID = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_QUESTGIVER_ACCEPT_QUEST [GUID={2:X} QuestID={3}]", client.IP, client.Port, guid, questID);
            if (!WorldServiceLocator._WorldServer.ALLQUESTS.IsValidQuest(questID))
            {
                var tmpQuest = new WS_QuestInfo(questID);
                // Load quest data
                if (client.Character.TalkCurrentQuest.ID != questID)
                    client.Character.TalkCurrentQuest = tmpQuest;
            }
            // Load quest data
            else if (client.Character.TalkCurrentQuest.ID != questID)
                client.Character.TalkCurrentQuest = WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID);
            if (client.Character.TalkCanAccept(ref client.Character.TalkCurrentQuest))
            {
                if (client.Character.TalkAddQuest(ref client.Character.TalkCurrentQuest))
                {
                    if (WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(guid))
                    {
                        var response = new Packets.PacketClass(OPCODES.MSG_QUEST_PUSH_RESULT);
                        try
                        {
                            response.AddUInt64(client.Character.GUID);
                            response.AddInt8((byte)QuestPartyPushError.QUEST_PARTY_MSG_ACCEPT_QUEST);
                            response.AddInt32(0);
                            WorldServiceLocator._WorldServer.CHARACTERs[guid].client.Send(ref response);
                        }
                        finally
                        {
                            response.Dispose();
                        }
                    }
                    else
                    {
                        var status = GetQuestgiverStatus(ref client.Character, guid);
                        var response = new Packets.PacketClass(OPCODES.SMSG_QUESTGIVER_STATUS);
                        try
                        {
                            response.AddUInt64(guid);
                            response.AddInt32((int)status);
                            client.Send(ref response);
                        }
                        finally
                        {
                            response.Dispose();
                        }
                    }
                }
                else
                {
                    var response = new Packets.PacketClass(OPCODES.SMSG_QUESTLOG_FULL);
                    try
                    {
                        client.Send(ref response);
                    }
                    finally
                    {
                        response.Dispose();
                    }
                }
            }
        }

        public void On_CMSG_QUESTLOG_REMOVE_QUEST(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 6)
                return;
            packet.GetInt16();
            byte slot = packet.GetInt8();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_QUESTLOG_REMOVE_QUEST [Slot={2}]", client.IP, client.Port, slot);
            client.Character.TalkDeleteQuest(slot);
        }

        public void On_CMSG_QUEST_QUERY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 9)
                return;
            packet.GetInt16();
            int questID = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_QUEST_QUERY [QuestID={2}]", client.IP, client.Port, questID);
            if (!WorldServiceLocator._WorldServer.ALLQUESTS.IsValidQuest(questID))
            {
                var tmpQuest = new WS_QuestInfo(questID);
                if (client.Character.TalkCurrentQuest is null)
                {
                    SendQuest(ref client, ref tmpQuest);
                    return;
                }

                if (client.Character.TalkCurrentQuest.ID == questID)
                {
                    SendQuest(ref client, ref client.Character.TalkCurrentQuest);
                }
                else
                {
                    SendQuest(ref client, ref tmpQuest);
                }
            }
            else
            {
                if (client.Character.TalkCurrentQuest is null)
                {
                    var argquest = WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID);
                    SendQuest(ref client, ref argquest);
                    return;
                }

                if (client.Character.TalkCurrentQuest.ID == questID)
                {
                    SendQuest(ref client, ref client.Character.TalkCurrentQuest);
                }
                else
                {
                    var argquest1 = WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID);
                    SendQuest(ref client, ref argquest1);
                }
            }
        }

        public void CompleteQuest(ref WS_PlayerData.CharacterObject objCharacter, int questID, ulong questGiverGuid)
        {
            if (!WorldServiceLocator._WorldServer.ALLQUESTS.IsValidQuest(questID))
            {
                var tmpQuest = new WS_QuestInfo(questID);
                for (int i = 0, loopTo = (int)QuestInfo.QUEST_SLOTS; i <= loopTo; i++)
                {
                    if (objCharacter.TalkQuests[i] is object)
                    {
                        if (objCharacter.TalkQuests[i].ID == questID)
                        {

                            // Load quest data
                            if (objCharacter.TalkCurrentQuest is null)
                                objCharacter.TalkCurrentQuest = tmpQuest;
                            if (objCharacter.TalkCurrentQuest.ID != questID)
                                objCharacter.TalkCurrentQuest = tmpQuest;
                            if (objCharacter.TalkQuests[i].Complete)
                            {
                                // DONE: Show completion dialog
                                if (objCharacter.TalkQuests[i].ObjectiveFlags & QuestObjectiveFlag.QUEST_OBJECTIVE_ITEM)
                                {
                                    // Request items
                                    SendQuestRequireItems(ref objCharacter.client, ref objCharacter.TalkCurrentQuest, questGiverGuid, ref objCharacter.TalkQuests[i]);
                                }
                                else
                                {
                                    SendQuestReward(ref objCharacter.client, ref objCharacter.TalkCurrentQuest, questGiverGuid, ref objCharacter.TalkQuests[i]);
                                }
                            }
                            else
                            {
                                // DONE: Just show incomplete text with disabled complete button
                                SendQuestRequireItems(ref objCharacter.client, ref objCharacter.TalkCurrentQuest, questGiverGuid, ref objCharacter.TalkQuests[i]);
                            }

                            break;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0, loopTo1 = (int)QuestInfo.QUEST_SLOTS; i <= loopTo1; i++)
                {
                    if (objCharacter.TalkQuests[i] is object)
                    {
                        if (objCharacter.TalkQuests[i].ID == questID)
                        {

                            // Load quest data
                            if (objCharacter.TalkCurrentQuest is null)
                                objCharacter.TalkCurrentQuest = WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID);
                            if (objCharacter.TalkCurrentQuest.ID != questID)
                                objCharacter.TalkCurrentQuest = WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID);
                            if (objCharacter.TalkQuests[i].Complete)
                            {
                                // DONE: Show completion dialog
                                if (objCharacter.TalkQuests[i].ObjectiveFlags & QuestObjectiveFlag.QUEST_OBJECTIVE_ITEM)
                                {
                                    // Request items
                                    SendQuestRequireItems(ref objCharacter.client, ref objCharacter.TalkCurrentQuest, questGiverGuid, ref objCharacter.TalkQuests[i]);
                                }
                                else
                                {
                                    SendQuestReward(ref objCharacter.client, ref objCharacter.TalkCurrentQuest, questGiverGuid, ref objCharacter.TalkQuests[i]);
                                }
                            }
                            else
                            {
                                // DONE: Just show incomplete text with disabled complete button
                                SendQuestRequireItems(ref objCharacter.client, ref objCharacter.TalkCurrentQuest, questGiverGuid, ref objCharacter.TalkQuests[i]);
                            }

                            break;
                        }
                    }
                }
            }
        }

        public void On_CMSG_QUESTGIVER_COMPLETE_QUEST(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 17)
                return;
            packet.GetInt16();
            ulong guid = packet.GetUInt64();
            int questID = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_QUESTGIVER_COMPLETE_QUEST [GUID={2:X} Quest={3}]", client.IP, client.Port, guid, questID);
            CompleteQuest(ref client.Character, questID, guid);
        }

        public void On_CMSG_QUESTGIVER_REQUEST_REWARD(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 17)
                return;
            packet.GetInt16();
            ulong guid = packet.GetUInt64();
            int questID = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_QUESTGIVER_REQUEST_REWARD [GUID={2:X} Quest={3}]", client.IP, client.Port, guid, questID);
            if (!WorldServiceLocator._WorldServer.ALLQUESTS.IsValidQuest(questID))
            {
                var tmpQuest = new WS_QuestInfo(questID);
                for (int i = 0, loopTo = (int)QuestInfo.QUEST_SLOTS; i <= loopTo; i++)
                {
                    if (client.Character.TalkQuests[i] is object && client.Character.TalkQuests[i].ID == questID && client.Character.TalkQuests[i].Complete)
                    {

                        // Load quest data
                        if (client.Character.TalkCurrentQuest.ID != questID)
                            client.Character.TalkCurrentQuest = tmpQuest;
                        SendQuestReward(ref client, ref client.Character.TalkCurrentQuest, guid, ref client.Character.TalkQuests[i]);
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0, loopTo1 = (int)QuestInfo.QUEST_SLOTS; i <= loopTo1; i++)
                {
                    if (client.Character.TalkQuests[i] is object && client.Character.TalkQuests[i].ID == questID && client.Character.TalkQuests[i].Complete)
                    {

                        // Load quest data
                        if (client.Character.TalkCurrentQuest.ID != questID)
                            client.Character.TalkCurrentQuest = WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID);
                        SendQuestReward(ref client, ref client.Character.TalkCurrentQuest, guid, ref client.Character.TalkQuests[i]);
                        break;
                    }
                }
            }
        }

        public void On_CMSG_QUESTGIVER_CHOOSE_REWARD(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 21)
                return;
            packet.GetInt16();
            ulong guid = packet.GetUInt64();
            int questID = packet.GetInt32();
            int rewardIndex = packet.GetInt32();
            if (!WorldServiceLocator._WorldServer.ALLQUESTS.IsValidQuest(questID))
            {
                try
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_QUESTGIVER_CHOOSE_REWARD [GUID={2:X} Quest={3} Reward={4}]", client.IP, client.Port, guid, questID, rewardIndex);
                    if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(guid) == false)
                        return;

                    // Load quest data
                    if (client.Character.TalkCurrentQuest is null)
                        client.Character.TalkCurrentQuest = WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID);
                    if (client.Character.TalkCurrentQuest.ID != questID)
                        client.Character.TalkCurrentQuest = WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID);

                    // DONE: Removing required gold
                    if (client.Character.TalkCurrentQuest.RewardGold < 0)
                    {
                        if (-client.Character.TalkCurrentQuest.RewardGold <= client.Character.Copper)
                        {
                            // NOTE: Update flag set below
                            // NOTE: Negative reward gold is required gold, that's why this should be plus
                            client.Character.Copper = (uint)(client.Character.Copper + client.Character.TalkCurrentQuest.RewardGold);
                        }
                        else
                        {
                            var errorPacket = new Packets.PacketClass(OPCODES.SMSG_QUESTGIVER_QUEST_INVALID);
                            errorPacket.AddInt32((int)QuestInvalidError.INVALIDREASON_DONT_HAVE_REQ_MONEY);
                            client.Send(ref errorPacket);
                            errorPacket.Dispose();
                            return;
                        }
                    }

                    // DONE: Removing required items
                    for (int i = 0, loopTo = (int)QuestInfo.QUEST_OBJECTIVES_COUNT; i <= loopTo; i++)
                    {
                        if (client.Character.TalkCurrentQuest.ObjectivesItem[i] != 0)
                        {
                            if (!client.Character.ItemCONSUME(client.Character.TalkCurrentQuest.ObjectivesItem[i], client.Character.TalkCurrentQuest.ObjectivesItem_Count[i]))
                            {
                                // DONE: Restore gold
                                if (client.Character.TalkCurrentQuest.RewardGold < 0)
                                {
                                    // NOTE: Negative reward gold is required gold, that's why this should be minus
                                    client.Character.Copper = (uint)(client.Character.Copper - client.Character.TalkCurrentQuest.RewardGold);
                                }
                                // TODO: Restore items (not needed?)
                                var errorPacket = new Packets.PacketClass(OPCODES.SMSG_QUESTGIVER_QUEST_INVALID);
                                errorPacket.AddInt32((int)QuestInvalidError.INVALIDREASON_DONT_HAVE_REQ_ITEMS);
                                client.Send(ref errorPacket);
                                errorPacket.Dispose();
                                return;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    // DONE: Adding reward choice
                    if (client.Character.TalkCurrentQuest.RewardItems[rewardIndex] != 0)
                    {
                        var tmpItem = new ItemObject(client.Character.TalkCurrentQuest.RewardItems[rewardIndex], client.Character.GUID) { StackCount = client.Character.TalkCurrentQuest.RewardItems_Count[rewardIndex] };
                        if (!client.Character.ItemADD(ref tmpItem))
                        {
                            tmpItem.Delete();
                            // DONE: Inventory full sent form SetItemSlot
                            return;
                        }
                        else
                        {
                            client.Character.LogLootItem(tmpItem, 1, true, false);
                        }
                    }

                    // DONE: Adding gold
                    if (client.Character.TalkCurrentQuest.RewardGold > 0)
                    {
                        client.Character.Copper = (uint)(client.Character.Copper + client.Character.TalkCurrentQuest.RewardGold);
                    }

                    client.Character.SetUpdateFlag((int)EPlayerFields.PLAYER_FIELD_COINAGE, client.Character.Copper);

                    // DONE: Add honor
                    if (client.Character.TalkCurrentQuest.RewardHonor != 0)
                    {
                        client.Character.HonorPoints += client.Character.TalkCurrentQuest.RewardHonor;
                        // Client.Character.SetUpdateFlag(EPlayerFields.PLAYER_FIELD_HONOR_CURRENCY, client.Character.HonorCurrency)
                    }

                    // DONE: Cast spell
                    if (client.Character.TalkCurrentQuest.RewardSpell > 0)
                    {
                        var spellTargets = new WS_Spells.SpellTargets();
                        WS_Base.BaseUnit argobjCharacter = client.Character;
                        spellTargets.SetTarget_UNIT(ref argobjCharacter);
                        var tmp = WorldServiceLocator._WorldServer.WORLD_CREATUREs;
                        WS_Base.BaseObject argCaster = tmp[guid];
                        var castParams = new WS_Spells.CastSpellParameters(ref spellTargets, ref argCaster, client.Character.TalkCurrentQuest.RewardSpell, true);
                        tmp[guid] = (WS_Creatures.CreatureObject)argCaster;
                        ThreadPool.QueueUserWorkItem(new WaitCallback(castParams.Cast));
                    }

                    // DONE: Remove quest
                    for (int i = 0, loopTo1 = (int)QuestInfo.QUEST_SLOTS; i <= loopTo1; i++)
                    {
                        if (client.Character.TalkQuests[i] is object)
                        {
                            if (client.Character.TalkQuests[i].ID == client.Character.TalkCurrentQuest.ID)
                            {
                                client.Character.TalkCompleteQuest((byte)i);
                                break;
                            }
                        }
                    }

                    // DONE: XP Calculations
                    int xp = 0;
                    int gold = client.Character.TalkCurrentQuest.RewardGold;
                    if (client.Character.TalkCurrentQuest.RewMoneyMaxLevel > 0)
                    {
                        int reqMoneyMaxLevel = client.Character.TalkCurrentQuest.RewMoneyMaxLevel;
                        int pLevel = client.Character.Level;
                        int qLevel = client.Character.TalkCurrentQuest.Level_Normal;
                        float fullxp = 0.0f;
                        if (pLevel <= WorldServiceLocator._WS_Player_Initializator.DEFAULT_MAX_LEVEL)
                        {
                            if (qLevel >= 65)
                            {
                                fullxp = reqMoneyMaxLevel / 6.0f;
                            }
                            else if (qLevel == 64)
                            {
                                fullxp = reqMoneyMaxLevel / 4.8f;
                            }
                            else if (qLevel == 63)
                            {
                                fullxp = reqMoneyMaxLevel / 3.6f;
                            }
                            else if (qLevel == 62)
                            {
                                fullxp = reqMoneyMaxLevel / 2.4f;
                            }
                            else if (qLevel == 61)
                            {
                                fullxp = reqMoneyMaxLevel / 1.2f;
                            }
                            else if (qLevel > 0 && qLevel <= 60)
                            {
                                fullxp = reqMoneyMaxLevel / 0.6f;
                            }

                            if (pLevel <= qLevel + 5)
                            {
                                xp = (int)Conversion.Fix(fullxp);
                            }
                            else if (pLevel == qLevel + 6)
                            {
                                xp = (int)Conversion.Fix(fullxp * 0.8f);
                            }
                            else if (pLevel == qLevel + 7)
                            {
                                xp = (int)Conversion.Fix(fullxp * 0.6f);
                            }
                            else if (pLevel == qLevel + 8)
                            {
                                xp = (int)Conversion.Fix(fullxp * 0.4f);
                            }
                            else if (pLevel == qLevel + 9)
                            {
                                xp = (int)Conversion.Fix(fullxp * 0.2f);
                            }
                            else
                            {
                                xp = (int)Conversion.Fix(fullxp * 0.1f);
                            }

                            // DONE: Adding XP
                            client.Character.AddXP(xp, 0, 0UL, true);
                        }
                        else
                        {
                            gold += reqMoneyMaxLevel;
                        }
                    }

                    if (gold < 0 && -gold >= client.Character.Copper)
                    {
                        client.Character.Copper = 0U;
                    }
                    else
                    {
                        client.Character.Copper = (uint)(client.Character.Copper + gold);
                    }

                    client.Character.SetUpdateFlag((int)EPlayerFields.PLAYER_FIELD_COINAGE, client.Character.Copper);
                    client.Character.SendCharacterUpdate();
                    SendQuestComplete(ref client, ref client.Character.TalkCurrentQuest, xp, gold);

                    // DONE: Follow-up quests (no requirements checked?)
                    if (client.Character.TalkCurrentQuest.NextQuest != 0)
                    {
                        if (!WorldServiceLocator._WorldServer.ALLQUESTS.IsValidQuest(client.Character.TalkCurrentQuest.NextQuest))
                        {
                            var tmpQuest2 = new WS_QuestInfo(client.Character.TalkCurrentQuest.NextQuest);
                            client.Character.TalkCurrentQuest = tmpQuest2;
                        }
                        else
                        {
                            client.Character.TalkCurrentQuest = WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(client.Character.TalkCurrentQuest.NextQuest);
                        }

                        SendQuestDetails(ref client, ref client.Character.TalkCurrentQuest, guid, true);
                    }
                }
                catch (Exception e)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "On_CMSG_QUESTGIVER_CHOOSE_REWARD - Error while choosing reward.{0}", Environment.NewLine + e.ToString());
                }
            }
            else
            {
                try
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_QUESTGIVER_CHOOSE_REWARD [GUID={2:X} Quest={3} Reward={4}]", client.IP, client.Port, guid, questID, rewardIndex);
                    if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(guid) == false)
                        return;

                    // Load quest data
                    if (client.Character.TalkCurrentQuest is null)
                        client.Character.TalkCurrentQuest = WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID);
                    if (client.Character.TalkCurrentQuest.ID != questID)
                        client.Character.TalkCurrentQuest = WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID);

                    // DONE: Removing required gold
                    if (client.Character.TalkCurrentQuest.RewardGold < 0)
                    {
                        if (-client.Character.TalkCurrentQuest.RewardGold <= client.Character.Copper)
                        {
                            // NOTE: Update flag set below
                            // NOTE: Negative reward gold is required gold, that's why this should be plus
                            client.Character.Copper = (uint)(client.Character.Copper + client.Character.TalkCurrentQuest.RewardGold);
                        }
                        else
                        {
                            var errorPacket = new Packets.PacketClass(OPCODES.SMSG_QUESTGIVER_QUEST_INVALID);
                            errorPacket.AddInt32((int)QuestInvalidError.INVALIDREASON_DONT_HAVE_REQ_MONEY);
                            client.Send(ref errorPacket);
                            errorPacket.Dispose();
                            return;
                        }
                    }

                    // DONE: Removing required items
                    for (int i = 0, loopTo2 = (int)QuestInfo.QUEST_OBJECTIVES_COUNT; i <= loopTo2; i++)
                    {
                        if (client.Character.TalkCurrentQuest.ObjectivesItem[i] != 0)
                        {
                            if (!client.Character.ItemCONSUME(client.Character.TalkCurrentQuest.ObjectivesItem[i], client.Character.TalkCurrentQuest.ObjectivesItem_Count[i]))
                            {
                                // DONE: Restore gold
                                if (client.Character.TalkCurrentQuest.RewardGold < 0)
                                {
                                    // NOTE: Negative reward gold is required gold, that's why this should be minus
                                    client.Character.Copper = (uint)(client.Character.Copper - client.Character.TalkCurrentQuest.RewardGold);
                                }
                                // TODO: Restore items (not needed?)
                                var errorPacket = new Packets.PacketClass(OPCODES.SMSG_QUESTGIVER_QUEST_INVALID);
                                errorPacket.AddInt32((int)QuestInvalidError.INVALIDREASON_DONT_HAVE_REQ_ITEMS);
                                client.Send(ref errorPacket);
                                errorPacket.Dispose();
                                return;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    // DONE: Adding reward choice
                    if (client.Character.TalkCurrentQuest.RewardItems[rewardIndex] != 0)
                    {
                        var tmpItem = new ItemObject(client.Character.TalkCurrentQuest.RewardItems[rewardIndex], client.Character.GUID) { StackCount = client.Character.TalkCurrentQuest.RewardItems_Count[rewardIndex] };
                        if (!client.Character.ItemADD(ref tmpItem))
                        {
                            tmpItem.Delete();
                            // DONE: Inventory full sent form SetItemSlot
                            return;
                        }
                        else
                        {
                            client.Character.LogLootItem(tmpItem, 1, true, false);
                        }
                    }

                    // DONE: Adding gold
                    if (client.Character.TalkCurrentQuest.RewardGold > 0)
                    {
                        client.Character.Copper = (uint)(client.Character.Copper + client.Character.TalkCurrentQuest.RewardGold);
                    }

                    client.Character.SetUpdateFlag((int)EPlayerFields.PLAYER_FIELD_COINAGE, client.Character.Copper);

                    // DONE: Add honor
                    if (client.Character.TalkCurrentQuest.RewardHonor != 0)
                    {
                        client.Character.HonorPoints += client.Character.TalkCurrentQuest.RewardHonor;
                        // Client.Character.SetUpdateFlag(EPlayerFields.PLAYER_FIELD_HONOR_CURRENCY, client.Character.HonorCurrency)
                    }

                    // DONE: Cast spell
                    if (client.Character.TalkCurrentQuest.RewardSpell > 0)
                    {
                        var spellTargets = new WS_Spells.SpellTargets();
                        WS_Base.BaseUnit argobjCharacter1 = client.Character;
                        spellTargets.SetTarget_UNIT(ref argobjCharacter1);
                        var tmp1 = WorldServiceLocator._WorldServer.WORLD_CREATUREs;
                        WS_Base.BaseObject argCaster1 = tmp1[guid];
                        var castParams = new WS_Spells.CastSpellParameters(ref spellTargets, ref argCaster1, client.Character.TalkCurrentQuest.RewardSpell, true);
                        tmp1[guid] = (WS_Creatures.CreatureObject)argCaster1;
                        ThreadPool.QueueUserWorkItem(new WaitCallback(castParams.Cast));
                    }

                    // DONE: Remove quest
                    for (int i = 0, loopTo3 = (int)QuestInfo.QUEST_SLOTS; i <= loopTo3; i++)
                    {
                        if (client.Character.TalkQuests[i] is object)
                        {
                            if (client.Character.TalkQuests[i].ID == client.Character.TalkCurrentQuest.ID)
                            {
                                client.Character.TalkCompleteQuest((byte)i);
                                break;
                            }
                        }
                    }

                    // DONE: XP Calculations
                    int xp = 0;
                    int gold = client.Character.TalkCurrentQuest.RewardGold;
                    if (client.Character.TalkCurrentQuest.RewMoneyMaxLevel > 0)
                    {
                        int reqMoneyMaxLevel = client.Character.TalkCurrentQuest.RewMoneyMaxLevel;
                        int pLevel = client.Character.Level;
                        int qLevel = client.Character.TalkCurrentQuest.Level_Normal;
                        float fullxp = 0.0f;
                        if (pLevel <= WorldServiceLocator._WS_Player_Initializator.DEFAULT_MAX_LEVEL)
                        {
                            if (qLevel >= 65)
                            {
                                fullxp = reqMoneyMaxLevel / 6.0f;
                            }
                            else if (qLevel == 64)
                            {
                                fullxp = reqMoneyMaxLevel / 4.8f;
                            }
                            else if (qLevel == 63)
                            {
                                fullxp = reqMoneyMaxLevel / 3.6f;
                            }
                            else if (qLevel == 62)
                            {
                                fullxp = reqMoneyMaxLevel / 2.4f;
                            }
                            else if (qLevel == 61)
                            {
                                fullxp = reqMoneyMaxLevel / 1.2f;
                            }
                            else if (qLevel > 0 && qLevel <= 60)
                            {
                                fullxp = reqMoneyMaxLevel / 0.6f;
                            }

                            if (pLevel <= qLevel + 5)
                            {
                                xp = (int)Conversion.Fix(fullxp);
                            }
                            else if (pLevel == qLevel + 6)
                            {
                                xp = (int)Conversion.Fix(fullxp * 0.8f);
                            }
                            else if (pLevel == qLevel + 7)
                            {
                                xp = (int)Conversion.Fix(fullxp * 0.6f);
                            }
                            else if (pLevel == qLevel + 8)
                            {
                                xp = (int)Conversion.Fix(fullxp * 0.4f);
                            }
                            else if (pLevel == qLevel + 9)
                            {
                                xp = (int)Conversion.Fix(fullxp * 0.2f);
                            }
                            else
                            {
                                xp = (int)Conversion.Fix(fullxp * 0.1f);
                            }

                            // DONE: Adding XP
                            client.Character.AddXP(xp, 0, 0UL, true);
                        }
                        else
                        {
                            gold += reqMoneyMaxLevel;
                        }
                    }

                    if (gold < 0 && -gold >= client.Character.Copper)
                    {
                        client.Character.Copper = 0U;
                    }
                    else
                    {
                        client.Character.Copper = (uint)(client.Character.Copper + gold);
                    }

                    client.Character.SetUpdateFlag((int)EPlayerFields.PLAYER_FIELD_COINAGE, client.Character.Copper);
                    client.Character.SendCharacterUpdate();
                    SendQuestComplete(ref client, ref client.Character.TalkCurrentQuest, xp, gold);

                    // DONE: Follow-up quests (no requirements checked?)
                    if (client.Character.TalkCurrentQuest.NextQuest != 0)
                    {
                        if (!WorldServiceLocator._WorldServer.ALLQUESTS.IsValidQuest(client.Character.TalkCurrentQuest.NextQuest))
                        {
                            var tmpQuest3 = new WS_QuestInfo(client.Character.TalkCurrentQuest.NextQuest);
                            client.Character.TalkCurrentQuest = tmpQuest3;
                        }
                        else
                        {
                            client.Character.TalkCurrentQuest = WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(client.Character.TalkCurrentQuest.NextQuest);
                        }

                        SendQuestDetails(ref client, ref client.Character.TalkCurrentQuest, guid, true);
                    }
                }
                catch (Exception e)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "On_CMSG_QUESTGIVER_CHOOSE_REWARD - Error while choosing reward.{0}", Environment.NewLine + e.ToString());
                }
            }
        }

        public void On_CMSG_PUSHQUESTTOPARTY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 9)
                return;
            packet.GetInt16();
            int questID = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_PUSHQUESTTOPARTY [{2}]", client.IP, client.Port, questID);
            if (client.Character.IsInGroup)
            {
                if (!WorldServiceLocator._WorldServer.ALLQUESTS.IsValidQuest(questID))
                {
                    var tmpQuest = new WS_QuestInfo(questID);
                    foreach (ulong guid in client.Character.Group.LocalMembers)
                    {
                        if (guid == client.Character.GUID)
                            continue;
                        {
                            var withBlock = WorldServiceLocator._WorldServer.CHARACTERs[guid];
                            var response = new Packets.PacketClass(OPCODES.MSG_QUEST_PUSH_RESULT);
                            response.AddUInt64(guid);
                            response.AddInt32((int)QuestPartyPushError.QUEST_PARTY_MSG_SHARRING_QUEST);
                            response.AddInt8(0);
                            client.Send(ref response);
                            response.Dispose();
                            QuestPartyPushError message = QuestPartyPushError.QUEST_PARTY_MSG_SHARRING_QUEST;

                            // DONE: Check distance and ...
                            if (Math.Sqrt(Math.Pow(withBlock.positionX - client.Character.positionX, 2d) + Math.Pow(withBlock.positionY - client.Character.positionY, 2d)) > QuestInfo.QUEST_SHARING_DISTANCE)
                            {
                                message = QuestPartyPushError.QUEST_PARTY_MSG_TO_FAR;
                            }
                            else if (withBlock.IsQuestInProgress(questID))
                            {
                                message = QuestPartyPushError.QUEST_PARTY_MSG_HAVE_QUEST;
                            }
                            else if (withBlock.IsQuestCompleted(questID))
                            {
                                message = QuestPartyPushError.QUEST_PARTY_MSG_FINISH_QUEST;
                            }
                            else
                            {
                                if (withBlock.TalkCurrentQuest is null || withBlock.TalkCurrentQuest.ID != questID)
                                    withBlock.TalkCurrentQuest = tmpQuest;
                                if (withBlock.TalkCanAccept(ref withBlock.TalkCurrentQuest))
                                {
                                    SendQuestDetails(ref withBlock.client, ref withBlock.TalkCurrentQuest, client.Character.GUID, true);
                                }
                                else
                                {
                                    message = QuestPartyPushError.QUEST_PARTY_MSG_CANT_TAKE_QUEST;
                                }
                            }

                            // DONE: Send error if present
                            if (message != QuestPartyPushError.QUEST_PARTY_MSG_SHARRING_QUEST)
                            {
                                var errorPacket = new Packets.PacketClass(OPCODES.MSG_QUEST_PUSH_RESULT);
                                errorPacket.AddUInt64(withBlock.GUID);
                                errorPacket.AddInt32((int)message);
                                errorPacket.AddInt8(0);
                                client.Send(ref errorPacket);
                                errorPacket.Dispose();
                            }
                        }
                    }
                }
                else
                {
                    foreach (ulong guid in client.Character.Group.LocalMembers)
                    {
                        if (guid == client.Character.GUID)
                            continue;
                        {
                            var withBlock1 = WorldServiceLocator._WorldServer.CHARACTERs[guid];
                            var response = new Packets.PacketClass(OPCODES.MSG_QUEST_PUSH_RESULT);
                            response.AddUInt64(guid);
                            response.AddInt32((int)QuestPartyPushError.QUEST_PARTY_MSG_SHARRING_QUEST);
                            response.AddInt8(0);
                            client.Send(ref response);
                            response.Dispose();
                            QuestPartyPushError message = QuestPartyPushError.QUEST_PARTY_MSG_SHARRING_QUEST;

                            // DONE: Check distance and ...
                            if (Math.Sqrt(Math.Pow(withBlock1.positionX - client.Character.positionX, 2d) + Math.Pow(withBlock1.positionY - client.Character.positionY, 2d)) > QuestInfo.QUEST_SHARING_DISTANCE)
                            {
                                message = QuestPartyPushError.QUEST_PARTY_MSG_TO_FAR;
                            }
                            else if (withBlock1.IsQuestInProgress(questID))
                            {
                                message = QuestPartyPushError.QUEST_PARTY_MSG_HAVE_QUEST;
                            }
                            else if (withBlock1.IsQuestCompleted(questID))
                            {
                                message = QuestPartyPushError.QUEST_PARTY_MSG_FINISH_QUEST;
                            }
                            else
                            {
                                if (withBlock1.TalkCurrentQuest is null || withBlock1.TalkCurrentQuest.ID != questID)
                                    withBlock1.TalkCurrentQuest = WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID);
                                if (withBlock1.TalkCanAccept(ref withBlock1.TalkCurrentQuest))
                                {
                                    SendQuestDetails(ref withBlock1.client, ref withBlock1.TalkCurrentQuest, client.Character.GUID, true);
                                }
                                else
                                {
                                    message = QuestPartyPushError.QUEST_PARTY_MSG_CANT_TAKE_QUEST;
                                }
                            }

                            // DONE: Send error if present
                            if (message != QuestPartyPushError.QUEST_PARTY_MSG_SHARRING_QUEST)
                            {
                                var errorPacket = new Packets.PacketClass(OPCODES.MSG_QUEST_PUSH_RESULT);
                                errorPacket.AddUInt64(withBlock1.GUID);
                                errorPacket.AddInt32((int)message);
                                errorPacket.AddInt8(0);
                                client.Send(ref errorPacket);
                                errorPacket.Dispose();
                            }
                        }
                    }
                }
            }
        }

        public void On_MSG_QUEST_PUSH_RESULT(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 14)
                return;
            packet.GetInt16();
            ulong guid = packet.GetUInt64();
            QuestPartyPushError message = (QuestPartyPushError)packet.GetInt8();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_QUEST_PUSH_RESULT [{2:X} {3}]", client.IP, client.Port, guid, message);
            var response = new Packets.PacketClass(OPCODES.MSG_QUEST_PUSH_RESULT);
            response.AddUInt64(guid);
            response.AddInt8((byte)QuestPartyPushError.QUEST_PARTY_MSG_ACCEPT_QUEST);
            response.AddInt32(0);
            client.Send(ref response);
            response.Dispose();
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        public WS_Quests()
        {
            // _quests(1) = New Dictionary(Of Integer, WS_QuestInfo)

        }
    }
}