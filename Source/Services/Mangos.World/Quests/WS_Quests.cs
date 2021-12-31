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
using Mangos.Common.Enums.Quest;
using Mangos.Common.Enums.Spell;
using Mangos.Common.Globals;
using Mangos.Common.Legacy;
using Mangos.World.Globals;
using Mangos.World.Loots;
using Mangos.World.Network;
using Mangos.World.Objects;
using Mangos.World.Player;
using Mangos.World.Spells;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace Mangos.World.Quests;

public class WS_Quests
{
    private readonly Collection _quests;

    public void LoadAllQuests()
    {
        DataTable cQuests = new();
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Loading Quests...");
        WorldServiceLocator._WorldServer.WorldDatabase.Query("SELECT entry FROM quests;", ref cQuests);
        IEnumerator enumerator = default;
        try
        {
            enumerator = cQuests.Rows.GetEnumerator();
            while (enumerator.MoveNext())
            {
                DataRow row = (DataRow)enumerator.Current;
                var questID = row.As<int>("entry");
                WS_QuestInfo tmpQuest = new(questID);
                _quests.Add(tmpQuest, Conversions.ToString(questID));
            }
        }
        finally
        {
            if (enumerator is IDisposable)
            {
                (enumerator as IDisposable).Dispose();
            }
        }
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Loading Quests...Complete");
    }

    public int ReturnQuestIdByName(string searchValue)
    {
        IEnumerator enumerator = default;
        try
        {
            enumerator = _quests.GetEnumerator();
            while (enumerator.MoveNext())
            {
                WS_QuestInfo thisService = (WS_QuestInfo)enumerator.Current;
                if (Operators.CompareString(thisService.Title, searchValue, TextCompare: false) == 0)
                {
                    return thisService.ID;
                }
            }
        }
        finally
        {
            if (enumerator is IDisposable)
            {
                (enumerator as IDisposable).Dispose();
            }
        }
        return 0;
    }

    public bool DoesPreQuestExist(int questID, int preQuestID)
    {
        var ret = false;
        IEnumerator enumerator = default;
        try
        {
            enumerator = _quests.GetEnumerator();
            while (enumerator.MoveNext())
            {
                WS_QuestInfo thisService = (WS_QuestInfo)enumerator.Current;
                if (thisService.ID == questID && thisService.PreQuests.Contains(preQuestID))
                {
                    return true;
                }
            }
        }
        finally
        {
            if (enumerator is IDisposable)
            {
                (enumerator as IDisposable).Dispose();
            }
        }
        return ret;
    }

    public bool IsValidQuest(int questID)
    {
        return _quests.Contains(questID.ToString());
    }

    public string ReturnQuestNameById(int questId)
    {
        var ret = "";
        IEnumerator enumerator = default;
        try
        {
            enumerator = _quests.GetEnumerator();
            while (enumerator.MoveNext())
            {
                WS_QuestInfo thisQuest = (WS_QuestInfo)enumerator.Current;
                if (thisQuest.ID == questId)
                {
                    ret = thisQuest.Title;
                }
            }
        }
        finally
        {
            if (enumerator is IDisposable)
            {
                (enumerator as IDisposable).Dispose();
            }
        }
        return ret;
    }

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
        catch (Exception ex2)
        {
            ProjectData.SetProjectError(ex2);
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "ReturnQuestInfoById returned error on QuestId {0}", questId);
            ProjectData.ClearProjectError();
        }
        return ret;
    }

    public QuestMenu GetQuestMenu(ref WS_PlayerData.CharacterObject objCharacter, ulong guid)
    {
        QuestMenu questMenu = new();
        var creatureEntry = WorldServiceLocator._WorldServer.WORLD_CREATUREs[guid].ID;
        List<int> alreadyHave = new();
        checked
        {
            if (WorldServiceLocator._WorldServer.CreatureQuestFinishers.ContainsKey(creatureEntry))
            {
                try
                {
                    var i = 0;
                    do
                    {
                        if (objCharacter.TalkQuests[i] != null)
                        {
                            alreadyHave.Add(objCharacter.TalkQuests[i].ID);
                            if (WorldServiceLocator._WorldServer.CreatureQuestFinishers[creatureEntry].Contains(objCharacter.TalkQuests[i].ID))
                            {
                                questMenu.AddMenu(objCharacter.TalkQuests[i].Title, (short)objCharacter.TalkQuests[i].ID, 0, 3);
                            }
                        }
                        i++;
                    }
                    while (i <= 24);
                }
                catch (Exception ex4)
                {
                    ProjectData.SetProjectError(ex4);
                    var ex3 = ex4;
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "GetQuestMenu Failed: ", ex3.ToString());
                    ProjectData.ClearProjectError();
                }
            }
            if (WorldServiceLocator._WorldServer.CreatureQuestStarters.ContainsKey(creatureEntry))
            {
                try
                {
                    foreach (var questID in WorldServiceLocator._WorldServer.CreatureQuestStarters[creatureEntry])
                    {
                        if (alreadyHave.Contains(questID))
                        {
                            continue;
                        }
                        if (!WorldServiceLocator._WorldServer.ALLQUESTS.IsValidQuest(questID))
                        {
                            try
                            {
                                WS_QuestInfo tmpQuest = new(questID);
                                if (tmpQuest.CanSeeQuest(ref objCharacter) && tmpQuest.SatisfyQuestLevel(ref objCharacter))
                                {
                                    questMenu.AddMenu(tmpQuest.Title, (short)questID, tmpQuest.Level_Normal, 5);
                                }
                            }
                            catch (Exception ex5)
                            {
                                ProjectData.SetProjectError(ex5);
                                var ex2 = ex5;
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "GetQuestMenu returned error for QuestId {0}", questID);
                                ProjectData.ClearProjectError();
                            }
                        }
                        else if (WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID).CanSeeQuest(ref objCharacter) && WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID).SatisfyQuestLevel(ref objCharacter))
                        {
                            questMenu.AddMenu(WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID).Title, (short)questID, WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID).Level_Normal, 5);
                        }
                    }
                }
                catch (Exception ex6)
                {
                    ProjectData.SetProjectError(ex6);
                    var ex = ex6;
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "GetQuestMenu Failed: ", ex.ToString());
                    ProjectData.ClearProjectError();
                }
            }
            return questMenu;
        }
    }

    public QuestMenu GetQuestMenuGO(ref WS_PlayerData.CharacterObject objCharacter, ulong guid)
    {
        QuestMenu questMenu = new();
        var gOEntry = WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs[guid].ID;
        List<int> alreadyHave = new();
        checked
        {
            if (WorldServiceLocator._WorldServer.GameobjectQuestFinishers.ContainsKey(gOEntry))
            {
                try
                {
                    var i = 0;
                    do
                    {
                        if (objCharacter.TalkQuests[i] != null)
                        {
                            alreadyHave.Add(objCharacter.TalkQuests[i].ID);
                            if (WorldServiceLocator._WorldServer.GameobjectQuestFinishers[gOEntry].Contains(objCharacter.TalkQuests[i].ID))
                            {
                                questMenu.AddMenu(objCharacter.TalkQuests[i].Title, (short)objCharacter.TalkQuests[i].ID, 0, 3);
                            }
                        }
                        i++;
                    }
                    while (i <= 24);
                }
                catch (Exception ex3)
                {
                    ProjectData.SetProjectError(ex3);
                    var ex2 = ex3;
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "GetQuestMenuGO Failed: ", ex2.ToString());
                    ProjectData.ClearProjectError();
                }
            }
            if (WorldServiceLocator._WorldServer.GameobjectQuestStarters.ContainsKey(gOEntry))
            {
                try
                {
                    foreach (var questID in WorldServiceLocator._WorldServer.GameobjectQuestStarters[gOEntry])
                    {
                        if (alreadyHave.Contains(questID))
                        {
                            continue;
                        }
                        if (!WorldServiceLocator._WorldServer.ALLQUESTS.IsValidQuest(questID))
                        {
                            WS_QuestInfo tmpQuest = new(questID);
                            if (tmpQuest.CanSeeQuest(ref objCharacter) && tmpQuest.SatisfyQuestLevel(ref objCharacter))
                            {
                                questMenu.AddMenu(tmpQuest.Title, (short)questID, tmpQuest.Level_Normal, 5);
                            }
                        }
                        else if (WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID).CanSeeQuest(ref objCharacter) && WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID).SatisfyQuestLevel(ref objCharacter))
                        {
                            questMenu.AddMenu(WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID).Title, (short)questID, WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID).Level_Normal, 5);
                        }
                    }
                }
                catch (Exception ex4)
                {
                    ProjectData.SetProjectError(ex4);
                    var ex = ex4;
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "GetQuestMenuGO Failed: ", ex.ToString());
                    ProjectData.ClearProjectError();
                }
            }
            return questMenu;
        }
    }

    public void SendQuestMenu(ref WS_PlayerData.CharacterObject objCharacter, ulong guid, string title = "Available quests", QuestMenu questMenu = null)
    {
        if (questMenu == null)
        {
            questMenu = GetQuestMenu(ref objCharacter, guid);
        }
        Packets.PacketClass packet = new(Opcodes.SMSG_QUESTGIVER_QUEST_LIST);
        checked
        {
            try
            {
                packet.AddUInt64(guid);
                packet.AddString(title);
                packet.AddInt32(1);
                packet.AddInt32(1);
                packet.AddInt8((byte)questMenu.IDs.Count);
                try
                {
                    var num = questMenu.IDs.Count - 1;
                    for (var i = 0; i <= num; i++)
                    {
                        packet.AddInt32(Conversions.ToInteger(questMenu.IDs[i]));
                        packet.AddInt32(Conversions.ToInteger(questMenu.Icons[i]));
                        packet.AddInt32(Conversions.ToInteger(questMenu.Levels[i]));
                        packet.AddString(Conversions.ToString(questMenu.Names[i]));
                    }
                }
                catch (Exception ex2)
                {
                    ProjectData.SetProjectError(ex2);
                    var ex = ex2;
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "GetQuestMenu Failed: ", ex.ToString());
                    ProjectData.ClearProjectError();
                }
                objCharacter.client.Send(ref packet);
            }
            finally
            {
                packet.Dispose();
            }
        }
    }

    public void SendQuestDetails(ref WS_Network.ClientClass client, ref WS_QuestInfo quest, ulong guid, bool acceptActive)
    {
        Packets.PacketClass packet = new(Opcodes.SMSG_QUESTGIVER_QUEST_DETAILS);
        checked
        {
            try
            {
                packet.AddUInt64(guid);
                packet.AddInt32(quest.ID);
                packet.AddString(quest.Title);
                packet.AddString(quest.TextDescription);
                packet.AddString(quest.TextObjectives);
                packet.AddInt32(acceptActive ? 1 : 0);
                var questRewardsCount = 0;
                try
                {
                    var i3 = 0;
                    do
                    {
                        if (quest.RewardItems[i3] != 0)
                        {
                            questRewardsCount++;
                        }
                        i3++;
                    }
                    while (i3 <= 5);
                }
                catch (Exception ex4)
                {
                    ProjectData.SetProjectError(ex4);
                    var ex3 = ex4;
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "SendQuestDetails Failed: ", ex3.ToString());
                    ProjectData.ClearProjectError();
                }
                packet.AddInt32(questRewardsCount);
                try
                {
                    var i2 = 0;
                    do
                    {
                        if (quest.RewardItems[i2] != 0)
                        {
                            if (!WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(quest.RewardItems[i2]))
                            {
                                WS_Items.ItemInfo tmpItem3 = new(quest.RewardItems[i2]);
                                packet.AddInt32(tmpItem3.Id);
                            }
                            else
                            {
                                packet.AddInt32(quest.RewardItems[i2]);
                            }
                            packet.AddInt32(quest.RewardItems_Count[i2]);
                            packet.AddInt32(WorldServiceLocator._WorldServer.ITEMDatabase[quest.RewardItems[i2]].Model);
                        }
                        else
                        {
                            packet.AddInt32(0);
                            packet.AddInt32(0);
                            packet.AddInt32(0);
                        }
                        i2++;
                    }
                    while (i2 <= 5);
                }
                catch (Exception ex5)
                {
                    ProjectData.SetProjectError(ex5);
                    var ex2 = ex5;
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "SendQuestDetails Failed: ", ex2.ToString());
                    ProjectData.ClearProjectError();
                }
                questRewardsCount = 0;
                var n = 0;
                do
                {
                    if (quest.RewardStaticItems[n] != 0)
                    {
                        questRewardsCount++;
                    }
                    n++;
                }
                while (n <= 4);
                packet.AddInt32(questRewardsCount);
                try
                {
                    var m = 0;
                    do
                    {
                        if (quest.RewardStaticItems[m] != 0)
                        {
                            if (!WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(quest.RewardStaticItems[m]))
                            {
                                WS_Items.ItemInfo tmpItem2 = new(quest.RewardStaticItems[m]);
                                packet.AddInt32(tmpItem2.Id);
                            }
                            else
                            {
                                packet.AddInt32(quest.RewardStaticItems[m]);
                            }
                            packet.AddInt32(quest.RewardStaticItems_Count[m]);
                            packet.AddInt32(WorldServiceLocator._WorldServer.ITEMDatabase[quest.RewardStaticItems[m]].Model);
                        }
                        else
                        {
                            packet.AddInt32(0);
                            packet.AddInt32(0);
                            packet.AddInt32(0);
                        }
                        m++;
                    }
                    while (m <= 4);
                }
                catch (Exception ex6)
                {
                    ProjectData.SetProjectError(ex6);
                    var ex = ex6;
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "SendQuestDetails Failed: ", ex.ToString());
                    ProjectData.ClearProjectError();
                }
                packet.AddInt32(quest.RewardGold);
                questRewardsCount = 0;
                var upperBound = quest.ObjectivesItem.GetUpperBound(0);
                for (var l = 0; l <= upperBound; l++)
                {
                    if (quest.ObjectivesItem[l] != 0)
                    {
                        questRewardsCount++;
                    }
                }
                packet.AddInt32(questRewardsCount);
                var upperBound2 = quest.ObjectivesItem.GetUpperBound(0);
                for (var k = 0; k <= upperBound2; k++)
                {
                    if (quest.ObjectivesItem[k] != 0 && !WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(quest.ObjectivesItem[k]))
                    {
                        WS_Items.ItemInfo tmpItem = new(quest.ObjectivesItem[k]);
                        packet.AddInt32(tmpItem.Id);
                    }
                    else
                    {
                        packet.AddInt32(quest.ObjectivesItem[k]);
                    }
                    packet.AddInt32(quest.ObjectivesItem_Count[k]);
                }
                questRewardsCount = 0;
                var upperBound3 = quest.ObjectivesItem.GetUpperBound(0);
                for (var j = 0; j <= upperBound3; j++)
                {
                    if (quest.ObjectivesKill[j] != 0)
                    {
                        questRewardsCount++;
                    }
                }
                packet.AddInt32(questRewardsCount);
                var upperBound4 = quest.ObjectivesItem.GetUpperBound(0);
                for (var i = 0; i <= upperBound4; i++)
                {
                    packet.AddUInt32((uint)quest.ObjectivesKill[i]);
                    packet.AddInt32(quest.ObjectivesKill_Count[i]);
                }
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_QUESTGIVER_QUEST_DETAILS [GUID={2:X} Quest={3}]", client.IP, client.Port, guid, quest.ID);
                client.Send(ref packet);
            }
            finally
            {
                packet.Dispose();
            }
        }
    }

    public void SendQuest(ref WS_Network.ClientClass client, ref WS_QuestInfo quest) //TODO: Figure out the correct packet structure for this
    {
        Packets.PacketClass packet = new(Opcodes.SMSG_QUEST_QUERY_RESPONSE);
        checked
        {
            try
            {
                packet.AddUInt32((uint)quest.ID);
                packet.AddUInt32(quest.Level_Start);
                packet.AddUInt32((uint)quest.Level_Normal);
                //packet.AddUInt32((uint)quest.ZoneOrSort);
                packet.AddUInt32((uint)quest.Type);
                packet.AddUInt32((uint)quest.ObjectiveRepFaction);
                packet.AddUInt32((uint)quest.ObjectiveRepStanding);
                //packet.AddUInt32(0u);
                //packet.AddUInt32(0u);
                packet.AddUInt32((uint)quest.NextQuestInChain);
                packet.AddUInt32((uint)quest.RewardGold);
                packet.AddUInt32((uint)quest.RewMoneyMaxLevel);
                /*if (quest.RewardSpell > 0)
                {
                    if (WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(quest.RewardSpell))
                    {
                        if (WorldServiceLocator._WS_Spells.SPELLs[quest.RewardSpell].SpellEffects[0] != null && WorldServiceLocator._WS_Spells.SPELLs[quest.RewardSpell].SpellEffects[0].ID == SpellEffects_Names.SPELL_EFFECT_LEARN_SPELL)
                        {
                            //packet.AddUInt32((uint)WorldServiceLocator._WS_Spells.SPELLs[quest.RewardSpell].SpellEffects[0].TriggerSpell);
                        }
                        else
                        {
                            //packet.AddUInt32((uint)quest.RewardSpell);
                        }
                    }
                    else
                    {
                        packet.AddUInt32(0u);
                    }
                }
                else*/
                {
                    packet.AddUInt32(0u);
                }
                packet.AddUInt32((uint)quest.ObjectivesDeliver);
                packet.AddUInt32((uint)(quest.QuestFlags & 0xFFFF));
                try
                {
                    var l = 0;
                    do
                    {
                        packet.AddUInt32((uint)quest.RewardStaticItems[l]);
                        packet.AddUInt32((uint)quest.RewardStaticItems_Count[l]);
                        l++;
                    }
                    while (l <= 4);
                }
                catch (Exception ex4)
                {
                    ProjectData.SetProjectError(ex4);
                    var ex3 = ex4;
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "SendQuest Failed: ", ex3.ToString());
                    ProjectData.ClearProjectError();
                }
                try
                {
                    var k = 0;
                    do
                    {
                        packet.AddUInt32((uint)quest.RewardItems[k]);
                        packet.AddUInt32((uint)quest.RewardItems_Count[k]);
                        k++;
                    }
                    while (k <= 5);
                }
                catch (Exception ex5)
                {
                    ProjectData.SetProjectError(ex5);
                    var ex2 = ex5;
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "SendQuest Failed: ", ex2.ToString());
                    ProjectData.ClearProjectError();
                }
                packet.AddUInt32((uint)quest.PointMapID);
                packet.AddSingle(quest.PointX);
                packet.AddSingle(quest.PointY);
                packet.AddUInt32((uint)quest.PointOpt);
                packet.AddString(quest.Title);
                packet.AddString(quest.TextObjectives);
                packet.AddString(quest.TextDescription);
                packet.AddString(quest.TextEnd);
                var j = 0;
                do
                {
                    packet.AddUInt32((uint)quest.ObjectivesKill[j]);
                    packet.AddUInt32((uint)quest.ObjectivesKill_Count[j]);
                    packet.AddUInt32((uint)quest.ObjectivesItem[j]);
                    packet.AddUInt32((uint)quest.ObjectivesItem_Count[j]);
                    if (quest.ObjectivesItem[j] != 0)
                    {
                        WorldServiceLocator._WS_Items.SendItemInfo(ref client, quest.ObjectivesItem[j]);
                    }
                    j++;
                }
                while (j <= 3);
                var i = 0;
                do
                {
                    packet.AddString(quest.ObjectivesText[i]);
                    i++;
                }
                while (i <= 3);
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_QUEST_QUERY_RESPONSE [Quest={2}]", client.IP, client.Port, quest.ID);
                client.Send(ref packet);
            }
            catch (Exception ex6)
            {
                ProjectData.SetProjectError(ex6);
                var ex = ex6;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SendQuest Failed [Quest={2}] {3}", client.IP, client.Port, quest.ID, ex.ToString());
                ProjectData.ClearProjectError();
            }
            finally
            {
                packet.Dispose();
            }
        }
    }

    public void SendQuestMessageAddItem(ref WS_Network.ClientClass client, int itemID, int itemCount)
    {
        Packets.PacketClass packet = new(Opcodes.SMSG_QUESTUPDATE_ADD_ITEM);
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

    public void SendQuestMessageAddKill(ref WS_Network.ClientClass client, int questID, ulong killGuid, int killID, int killCurrentCount, int killCount)
    {
        Packets.PacketClass packet = new(Opcodes.SMSG_QUESTUPDATE_ADD_KILL);
        try
        {
            packet.AddInt32(questID);
            if (killID < 0)
            {
                killID = checked(-killID) | int.MinValue;
            }
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

    public void SendQuestMessageFailed(ref WS_Network.ClientClass client, int questID)
    {
        Packets.PacketClass packet = new(Opcodes.SMSG_QUESTGIVER_QUEST_FAILED);
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

    public void SendQuestMessageFailedTimer(ref WS_Network.ClientClass client, int questID)
    {
        Packets.PacketClass packet = new(Opcodes.SMSG_QUESTUPDATE_FAILEDTIMER);
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

    public void SendQuestMessageComplete(ref WS_Network.ClientClass client, int questID)
    {
        Packets.PacketClass packet = new(Opcodes.SMSG_QUESTUPDATE_COMPLETE);
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

    public void SendQuestComplete(ref WS_Network.ClientClass client, ref WS_QuestInfo quest, int xP, int gold)
    {
        Packets.PacketClass packet = new(Opcodes.SMSG_QUESTGIVER_QUEST_COMPLETE);
        checked
        {
            try
            {
                packet.AddInt32(quest.ID);
                packet.AddInt32(3);
                packet.AddInt32(xP);
                packet.AddInt32(gold);
                packet.AddInt32(quest.RewardHonor);
                var rewardsCount = 0;
                var j = 0;
                do
                {
                    if (quest.RewardStaticItems[j] > 0)
                    {
                        rewardsCount++;
                    }
                    j++;
                }
                while (j <= 4);
                packet.AddInt32(rewardsCount);
                var i = 0;
                do
                {
                    if (quest.RewardStaticItems[i] > 0)
                    {
                        packet.AddInt32(quest.RewardStaticItems[i]);
                        packet.AddInt32(quest.RewardStaticItems_Count[i]);
                    }
                    i++;
                }
                while (i <= 4);
                client.Send(ref packet);
            }
            finally
            {
                packet.Dispose();
            }
        }
    }

    public void SendQuestReward(ref WS_Network.ClientClass client, ref WS_QuestInfo quest, ulong guid, ref WS_QuestsBase objBaseQuest)
    {
        Packets.PacketClass packet = new(Opcodes.SMSG_QUESTGIVER_OFFER_REWARD);
        try
        {
            packet.AddUInt64(guid);
            packet.AddInt32(objBaseQuest.ID);
            packet.AddString(objBaseQuest.Title);
            packet.AddString(quest.TextComplete);
            packet.AddInt32(0 - (objBaseQuest.Complete ? 1 : 0));
            var emoteCount = 0;
            var n = 0;
            checked
            {
                do
                {
                    if (quest.OfferRewardEmote[n] > 0)
                    {
                        emoteCount++;
                    }
                    n++;
                }
                while (n <= 3);
                packet.AddInt32(emoteCount);
                var num = emoteCount - 1;
                for (var m = 0; m <= num; m++)
                {
                    packet.AddInt32(0);
                    packet.AddInt32(quest.OfferRewardEmote[m]);
                }
                var questRewardsCount = 0;
                var l = 0;
                do
                {
                    if (quest.RewardItems[l] != 0)
                    {
                        questRewardsCount++;
                    }
                    l++;
                }
                while (l <= 5);
                packet.AddInt32(questRewardsCount);
                var k = 0;
                do
                {
                    if (quest.RewardItems[k] != 0)
                    {
                        packet.AddInt32(quest.RewardItems[k]);
                        packet.AddInt32(quest.RewardItems_Count[k]);
                        if (!WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(quest.RewardItems[k]))
                        {
                            WS_Items.ItemInfo tmpItem2 = new(quest.RewardItems[k]);
                            packet.AddInt32(tmpItem2.Model);
                        }
                        else
                        {
                            packet.AddInt32(WorldServiceLocator._WorldServer.ITEMDatabase[quest.RewardItems[k]].Model);
                        }
                    }
                    k++;
                }
                while (k <= 5);
                questRewardsCount = 0;
                var j = 0;
                do
                {
                    if (quest.RewardStaticItems[j] != 0)
                    {
                        questRewardsCount++;
                    }
                    j++;
                }
                while (j <= 4);
                packet.AddInt32(questRewardsCount);
                var i = 0;
                do
                {
                    if (quest.RewardStaticItems[i] != 0)
                    {
                        packet.AddInt32(quest.RewardStaticItems[i]);
                        packet.AddInt32(quest.RewardStaticItems_Count[i]);
                        if (!WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(quest.RewardStaticItems[i]))
                        {
                            WS_Items.ItemInfo tmpItem = new(quest.RewardStaticItems[i]);
                        }
                        packet.AddInt32(WorldServiceLocator._WorldServer.ITEMDatabase[quest.RewardStaticItems[i]].Model);
                    }
                    i++;
                }
                while (i <= 4);
                packet.AddInt32(quest.RewardGold);
                packet.AddInt32(0);
                if (quest.RewardSpell > 0)
                {
                    if (WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(quest.RewardSpell))
                    {
                        if (WorldServiceLocator._WS_Spells.SPELLs[quest.RewardSpell].SpellEffects[0] != null && WorldServiceLocator._WS_Spells.SPELLs[quest.RewardSpell].SpellEffects[0].ID == SpellEffects_Names.SPELL_EFFECT_LEARN_SPELL)
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
        }
        finally
        {
            packet.Dispose();
        }
    }

    public void SendQuestRequireItems(ref WS_Network.ClientClass client, ref WS_QuestInfo quest, ulong guid, ref WS_QuestsBase objBaseQuest)
    {
        Packets.PacketClass packet = new(Opcodes.SMSG_QUESTGIVER_REQUEST_ITEMS);
        checked
        {
            try
            {
                packet.AddUInt64(guid);
                packet.AddInt32(objBaseQuest.ID);
                packet.AddString(objBaseQuest.Title);
                packet.AddString(quest.TextIncomplete);
                packet.AddInt32(0);
                if (objBaseQuest.Complete)
                {
                    packet.AddInt32(quest.CompleteEmote);
                }
                else
                {
                    packet.AddInt32(quest.IncompleteEmote);
                }
                packet.AddInt32(0);
                if (quest.RewardGold < 0)
                {
                    packet.AddInt32(-quest.RewardGold);
                }
                else
                {
                    packet.AddInt32(0);
                }
                byte requiredItemsCount = 0;
                var upperBound = quest.ObjectivesItem.GetUpperBound(0);
                for (var j = 0; j <= upperBound; j++)
                {
                    if (quest.ObjectivesItem[j] != 0)
                    {
                        requiredItemsCount = (byte)(requiredItemsCount + 1);
                    }
                }
                packet.AddInt32(requiredItemsCount);
                if (requiredItemsCount > 0)
                {
                    var upperBound2 = quest.ObjectivesItem.GetUpperBound(0);
                    for (var i = 0; i <= upperBound2; i++)
                    {
                        if (quest.ObjectivesItem[i] != 0)
                        {
                            if (!WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(quest.ObjectivesItem[i]))
                            {
                                WS_Items.ItemInfo tmpItem = new(quest.ObjectivesItem[i]);
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
                packet.AddInt32(4);
                packet.AddInt32(8);
                packet.AddInt32(16);
                client.Send(ref packet);
            }
            finally
            {
                packet.Dispose();
            }
        }
    }

    public void LoadQuests(ref WS_PlayerData.CharacterObject objCharacter)
    {
        DataTable cQuests = new();
        var i = 0;
        WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT quest_id, quest_status FROM characters_quests q WHERE q.char_guid = {objCharacter.GUID};", ref cQuests);
        checked
        {
            IEnumerator enumerator = default;
            try
            {
                enumerator = cQuests.Rows.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    DataRow row = (DataRow)enumerator.Current;
                    var questID = row.As<int>("quest_id");
                    var questStatus = row.As<int>("quest_status");
                    if (questStatus >= 0)
                    {
                        if (IsValidQuest(questID))
                        {
                            var tmpQuest = ReturnQuestInfoById(questID);
                            CreateQuest(ref objCharacter.TalkQuests[i], ref tmpQuest);
                            objCharacter.TalkQuests[i].LoadState(questStatus);
                            objCharacter.TalkQuests[i].Slot = (byte)i;
                            objCharacter.TalkQuests[i].UpdateItemCount(ref objCharacter);
                            i++;
                        }
                    }
                    else if (questStatus == -1)
                    {
                        objCharacter.QuestsCompleted.Add(questID);
                    }
                }
            }
            finally
            {
                if (enumerator is IDisposable)
                {
                    (enumerator as IDisposable).Dispose();
                }
            }
        }
    }

    public void CreateQuest(ref WS_QuestsBase objBaseQuest, ref WS_QuestInfo tmpQuest)
    {
        objBaseQuest = new WS_QuestsBase(tmpQuest);
    }

    public void OnQuestKill(ref WS_PlayerData.CharacterObject objCharacter, ref WS_Creatures.CreatureObject creature)
    {
        if (objCharacter == null)
        {
            return;
        }
        var i = 0;
        do
        {
            if (objCharacter.TalkQuests[i] != null && ((uint)objCharacter.TalkQuests[i].ObjectiveFlags & (true ? 1u : 0u)) != 0 && (objCharacter.TalkQuests[i].ObjectiveFlags & 0x10) == 0)
            {
                switch (objCharacter.TalkQuests[i])
                {
                    case WS_QuestsBaseScripted _:
                        ((WS_QuestsBaseScripted)objCharacter.TalkQuests[i]).OnQuestKill(ref objCharacter, ref creature);
                        break;

                    default:
                        {
                            var wS_QuestsBase = objCharacter.TalkQuests[i];
                            byte j = 0;
                            do
                            {
                                if (wS_QuestsBase.ObjectivesType[j] == 1 && wS_QuestsBase.ObjectivesObject[j] == creature.ID && wS_QuestsBase.Progress[j] < (uint)wS_QuestsBase.ObjectivesCount[j])
                                {
                                    wS_QuestsBase.AddKill(objCharacter, j, creature.GUID);
                                    return;
                                }
                                checked
                                {
                                    j = (byte)unchecked((uint)(j + 1));
                                }
                            }
                            while (j <= 3u);
                            break;
                        }
                }
            }
            i = checked(i + 1);
        }
        while (i <= 24);
    }

    public void OnQuestCastSpell(ref WS_PlayerData.CharacterObject objCharacter, ref WS_Creatures.CreatureObject creature, int spellID)
    {
        var i = 0;
        do
        {
            if (objCharacter.TalkQuests[i] != null && ((uint)objCharacter.TalkQuests[i].ObjectiveFlags & 0x10u) != 0)
            {
                switch (objCharacter.TalkQuests[i])
                {
                    case WS_QuestsBaseScripted _:
                        ((WS_QuestsBaseScripted)objCharacter.TalkQuests[i]).OnQuestCastSpell(ref objCharacter, ref creature, spellID);
                        break;

                    default:
                        {
                            var wS_QuestsBase = objCharacter.TalkQuests[i];
                            byte j = 0;
                            do
                            {
                                if (wS_QuestsBase.ObjectivesType[j] == 1 && wS_QuestsBase.ObjectivesSpell[j] == spellID && (wS_QuestsBase.ObjectivesObject[j] == 0 || wS_QuestsBase.ObjectivesObject[j] == creature.ID) && wS_QuestsBase.Progress[j] < (uint)wS_QuestsBase.ObjectivesCount[j])
                                {
                                    wS_QuestsBase.AddCast(objCharacter, j, creature.GUID);
                                    return;
                                }
                                checked
                                {
                                    j = (byte)unchecked((uint)(j + 1));
                                }
                            }
                            while (j <= 3u);
                            break;
                        }
                }
            }
            i = checked(i + 1);
        }
        while (i <= 24);
    }

    public void OnQuestCastSpell(ref WS_PlayerData.CharacterObject objCharacter, ref WS_GameObjects.GameObject gameObject, int spellID)
    {
        var i = 0;
        do
        {
            if (objCharacter.TalkQuests[i] != null && ((uint)objCharacter.TalkQuests[i].ObjectiveFlags & 0x10u) != 0)
            {
                switch (objCharacter.TalkQuests[i])
                {
                    case WS_QuestsBaseScripted _:
                        ((WS_QuestsBaseScripted)objCharacter.TalkQuests[i]).OnQuestCastSpell(ref objCharacter, ref gameObject, spellID);
                        break;

                    default:
                        {
                            var wS_QuestsBase = objCharacter.TalkQuests[i];
                            byte j = 0;
                            do
                            {
                                if (wS_QuestsBase.ObjectivesType[j] == 1 && wS_QuestsBase.ObjectivesSpell[j] == spellID && (wS_QuestsBase.ObjectivesObject[j] == 0 || wS_QuestsBase.ObjectivesObject[j] == checked(-gameObject.ID)) && wS_QuestsBase.Progress[j] < (uint)wS_QuestsBase.ObjectivesCount[j])
                                {
                                    wS_QuestsBase.AddCast(objCharacter, j, gameObject.GUID);
                                    return;
                                }
                                checked
                                {
                                    j = (byte)unchecked((uint)(j + 1));
                                }
                            }
                            while (j <= 3u);
                            break;
                        }
                }
            }
            i = checked(i + 1);
        }
        while (i <= 24);
    }

    public void OnQuestDoEmote(ref WS_PlayerData.CharacterObject objCharacter, ref WS_Creatures.CreatureObject creature, int emoteID)
    {
        var i = 0;
        do
        {
            if (objCharacter.TalkQuests[i] != null && ((uint)objCharacter.TalkQuests[i].ObjectiveFlags & 0x40u) != 0)
            {
                switch (objCharacter.TalkQuests[i])
                {
                    case WS_QuestsBaseScripted _:
                        ((WS_QuestsBaseScripted)objCharacter.TalkQuests[i]).OnQuestEmote(ref objCharacter, ref creature, emoteID);
                        break;

                    default:
                        {
                            var wS_QuestsBase = objCharacter.TalkQuests[i];
                            byte j = 0;
                            do
                            {
                                if (wS_QuestsBase.ObjectivesType[j] == 64 && wS_QuestsBase.ObjectivesSpell[j] == emoteID && (wS_QuestsBase.ObjectivesObject[j] == 0 || wS_QuestsBase.ObjectivesObject[j] == creature.ID) && wS_QuestsBase.Progress[j] < (uint)wS_QuestsBase.ObjectivesCount[j])
                                {
                                    wS_QuestsBase.AddEmote(objCharacter, j);
                                    return;
                                }
                                checked
                                {
                                    j = (byte)unchecked((uint)(j + 1));
                                }
                            }
                            while (j <= 3u);
                            break;
                        }
                }
            }
            i = checked(i + 1);
        }
        while (i <= 24);
    }

    public bool IsItemNeededForQuest(ref WS_PlayerData.CharacterObject objCharacter, ref int itemEntry)
    {
        var isRaid = objCharacter.IsInRaid;
        if (objCharacter.IsInGroup)
        {
            foreach (var guid in objCharacter.Group.LocalMembers)
            {
                var characterObject = WorldServiceLocator._WorldServer.CHARACTERs[guid];
                var j = 0;
                do
                {
                    if (characterObject.TalkQuests[j] != null && ((uint)characterObject.TalkQuests[j].ObjectiveFlags & 0x20u) != 0 && !isRaid)
                    {
                        var wS_QuestsBase = characterObject.TalkQuests[j];
                        byte l = 0;
                        do
                        {
                            if (wS_QuestsBase.ObjectivesItem[l] == itemEntry && wS_QuestsBase.ProgressItem[l] < (uint)wS_QuestsBase.ObjectivesItemCount[l])
                            {
                                return true;
                            }
                            checked
                            {
                                l = (byte)unchecked((uint)(l + 1));
                            }
                        }
                        while (l <= 3u);
                    }
                    j = checked(j + 1);
                }
                while (j <= 24);
            }
        }
        else
        {
            var i = 0;
            do
            {
                if (objCharacter.TalkQuests[i] != null && ((uint)objCharacter.TalkQuests[i].ObjectiveFlags & 0x20u) != 0)
                {
                    var wS_QuestsBase2 = objCharacter.TalkQuests[i];
                    byte k = 0;
                    do
                    {
                        if (wS_QuestsBase2.ObjectivesItem[k] == itemEntry && wS_QuestsBase2.ProgressItem[k] < (uint)wS_QuestsBase2.ObjectivesItemCount[k])
                        {
                            return true;
                        }
                        checked
                        {
                            k = (byte)unchecked((uint)(k + 1));
                        }
                    }
                    while (k <= 3u);
                }
                i = checked(i + 1);
            }
            while (i <= 24);
        }
        return false;
    }

    public byte IsGameObjectUsedForQuest(ref WS_GameObjects.GameObject gameobject, ref WS_PlayerData.CharacterObject objCharacter)
    {
        if (!gameobject.IsUsedForQuests)
        {
            return 0;
        }
        foreach (var questItemID in gameobject.IncludesQuestItems)
        {
            var i = 0;
            do
            {
                if (objCharacter.TalkQuests[i] != null && ((uint)objCharacter.TalkQuests[i].ObjectiveFlags & 0x20u) != 0)
                {
                    byte j = 0;
                    do
                    {
                        if (objCharacter.TalkQuests[i].ObjectivesType[j] == 32 && objCharacter.TalkQuests[i].ObjectivesItem[j] == questItemID && objCharacter.ItemCOUNT(questItemID) < objCharacter.TalkQuests[i].ObjectivesItemCount[j])
                        {
                            return 2;
                        }
                        checked
                        {
                            j = (byte)unchecked((uint)(j + 1));
                        }
                    }
                    while (j <= 3u);
                }
                i = checked(i + 1);
            }
            while (i <= 24);
        }
        return 1;
    }

    public void OnQuestAddQuestLoot(ref WS_PlayerData.CharacterObject objCharacter, ref WS_Creatures.CreatureObject creature, ref WS_Loot.LootObject loot)
    {
    }

    public void OnQuestAddQuestLoot(ref WS_PlayerData.CharacterObject objCharacter, ref WS_GameObjects.GameObject gameObject, ref WS_Loot.LootObject loot)
    {
    }

    public void OnQuestAddQuestLoot(ref WS_PlayerData.CharacterObject objCharacter, ref WS_PlayerData.CharacterObject character, ref WS_Loot.LootObject loot)
    {
    }

    public void OnQuestItemAdd(ref WS_PlayerData.CharacterObject objCharacter, int itemID, byte count)
    {
        if (count == 0)
        {
            count = 1;
        }
        var i = 0;
        do
        {
            if (objCharacter.TalkQuests[i] != null && ((uint)objCharacter.TalkQuests[i].ObjectiveFlags & 0x20u) != 0)
            {
                switch (objCharacter.TalkQuests[i])
                {
                    case WS_QuestsBaseScripted _:
                        ((WS_QuestsBaseScripted)objCharacter.TalkQuests[i]).OnQuestItem(ref objCharacter, itemID, count);
                        break;

                    default:
                        {
                            var wS_QuestsBase = objCharacter.TalkQuests[i];
                            var j = 0;
                            do
                            {
                                if (wS_QuestsBase.ObjectivesItem[j] == itemID && wS_QuestsBase.ProgressItem[j] < (uint)wS_QuestsBase.ObjectivesItemCount[j])
                                {
                                    wS_QuestsBase.AddItem(objCharacter, checked((byte)j), count);
                                    return;
                                }
                                j = checked(j + 1);
                            }
                            while (j <= 3);
                            break;
                        }
                }
            }
            i = checked(i + 1);
        }
        while (i <= 24);
    }

    public void OnQuestItemRemove(ref WS_PlayerData.CharacterObject objCharacter, int itemID, byte count)
    {
        if (count == 0)
        {
            count = 1;
        }
        var i = 0;
        checked
        {
            do
            {
                if (objCharacter.TalkQuests[i] != null && (unchecked((uint)objCharacter.TalkQuests[i].ObjectiveFlags) & 0x20u) != 0)
                {
                    switch (objCharacter.TalkQuests[i])
                    {
                        case WS_QuestsBaseScripted _:
                            ((WS_QuestsBaseScripted)objCharacter.TalkQuests[i]).OnQuestItem(ref objCharacter, itemID, (short)unchecked(-count));
                            break;

                        default:
                            {
                                var wS_QuestsBase = objCharacter.TalkQuests[i];
                                byte j = 0;
                                do
                                {
                                    if (wS_QuestsBase.ObjectivesItem[j] == itemID && wS_QuestsBase.ProgressItem[j] > 0)
                                    {
                                        wS_QuestsBase.RemoveItem(objCharacter, j, count);
                                        return;
                                    }
                                    j = (byte)unchecked((uint)(j + 1));
                                }
                                while (j <= 3u);
                                break;
                            }
                    }
                }
                i++;
            }
            while (i <= 24);
        }
    }

    public void OnQuestExplore(ref WS_PlayerData.CharacterObject objCharacter, int areaID)
    {
        var i = 0;
        do
        {
            if (objCharacter.TalkQuests[i] != null && ((uint)objCharacter.TalkQuests[i].ObjectiveFlags & 2u) != 0)
            {
                switch (objCharacter.TalkQuests[i])
                {
                    case WS_QuestsBaseScripted _:
                        ((WS_QuestsBaseScripted)objCharacter.TalkQuests[i]).OnQuestExplore(ref objCharacter, areaID);
                        break;

                    default:
                        {
                            var wS_QuestsBase = objCharacter.TalkQuests[i];
                            byte j = 0;
                            do
                            {
                                if (wS_QuestsBase.ObjectivesExplore[j] == areaID && !wS_QuestsBase.Explored)
                                {
                                    wS_QuestsBase.AddExplore(objCharacter);
                                    return;
                                }
                                checked
                                {
                                    j = (byte)unchecked((uint)(j + 1));
                                }
                            }
                            while (j <= 3u);
                            break;
                        }
                }
            }
            i = checked(i + 1);
        }
        while (i <= 24);
    }

    public byte ClassByQuestSort(int questSort)
    {
        return questSort switch
        {
            61 => 9,
            81 => 1,
            82 => 7,
            141 => 2,
            161 => 8,
            162 => 4,
            261 => 3,
            262 => 5,
            263 => 11,
            _ => 0,
        };
    }

    public QuestgiverStatusFlag GetQuestgiverStatus(ref WS_PlayerData.CharacterObject objCharacter, ulong cGuid)
    {
        var status = QuestgiverStatusFlag.DIALOG_STATUS_NONE;
        List<int> alreadyHave = new();
        if (WorldServiceLocator._CommonGlobalFunctions.GuidIsCreature(cGuid))
        {
            if (!WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(cGuid))
            {
                return QuestgiverStatusFlag.DIALOG_STATUS_NONE;
            }
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "QuestStatus ID: {0} NPC Name: {1}", WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].ID, WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].Name);
            var creatureQuestId = WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].ID;
            if (IsValidQuest(creatureQuestId))
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "QuestStatus ID: {0} Valid Quest: {1}", WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].ID, IsValidQuest(creatureQuestId));
                if (WorldServiceLocator._WorldServer.CreatureQuestStarters.ContainsKey(creatureQuestId))
                {
                    foreach (var questID in WorldServiceLocator._WorldServer.CreatureQuestStarters[creatureQuestId])
                    {
                        try
                        {
                            if (WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID).CanSeeQuest(ref objCharacter))
                            {
                                return QuestgiverStatusFlag.DIALOG_STATUS_AVAILABLE;
                            }
                        }
                        catch (Exception ex3)
                        {
                            ProjectData.SetProjectError(ex3);
                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "GetQuestGiverStatus Error");
                            ProjectData.ClearProjectError();
                        }
                    }
                    if (WorldServiceLocator._WorldServer.CreatureQuestFinishers.ContainsKey(creatureQuestId))
                    {
                        foreach (var questID2 in WorldServiceLocator._WorldServer.CreatureQuestFinishers[creatureQuestId])
                        {
                            try
                            {
                                if (WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID2).CanSeeQuest(ref objCharacter) && objCharacter.IsQuestInProgress(questID2))
                                {
                                    return QuestgiverStatusFlag.DIALOG_STATUS_REWARD;
                                }
                            }
                            catch (Exception ex4)
                            {
                                ProjectData.SetProjectError(ex4);
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "GetQuestGiverStatus Error");
                                ProjectData.ClearProjectError();
                            }
                        }
                    }
                }
            }
            return (QuestgiverStatusFlag)WorldServiceLocator._WorldServer.WORLD_CREATUREs[cGuid].CreatureInfo.TalkScript.OnQuestStatus(ref objCharacter, cGuid);
        }
        if (WorldServiceLocator._CommonGlobalFunctions.GuidIsGameObject(cGuid))
        {
            if (!WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(cGuid))
            {
                return QuestgiverStatusFlag.DIALOG_STATUS_NONE;
            }
            var i = 0;
            do
            {
                if (objCharacter.TalkQuests[i] != null)
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
                i = checked(i + 1);
            }
            while (i <= 24);
            return status;
        }
        return QuestgiverStatusFlag.DIALOG_STATUS_NONE;
    }

    public void On_CMSG_QUESTGIVER_STATUS_QUERY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        checked
        {
            try
            {
                if (packet.Data.Length - 1 >= 13)
                {
                    packet.GetInt16();
                    var guid = packet.GetUInt64();
                    var status = GetQuestgiverStatus(ref client.Character, guid);
                    Packets.PacketClass response = new(Opcodes.SMSG_QUESTGIVER_STATUS);
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
            }
            catch (Exception ex)
            {
                ProjectData.SetProjectError(ex);
                var e = ex;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "On_CMSG_QUESTGIVER_STATUS_QUERY - Error in questgiver status query.{0}", Environment.NewLine + e);
                ProjectData.ClearProjectError();
            }
        }
    }

    public void On_CMSG_QUESTGIVER_HELLO(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        try
        {
            if (checked(packet.Data.Length - 1) < 13)
            {
                return;
            }
            packet.GetInt16();
            var guid = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_QUESTGIVER_HELLO [GUID={2:X}]", client.IP, client.Port, guid);
            if (!WorldServiceLocator._WorldServer.WORLD_CREATUREs[guid].Evade)
            {
                WorldServiceLocator._WorldServer.WORLD_CREATUREs[guid].StopMoving();
                client.Character.RemoveAurasByInterruptFlag(1024);
                if (WorldServiceLocator._WorldServer.CREATURESDatabase[WorldServiceLocator._WorldServer.WORLD_CREATUREs[guid].ID].TalkScript == null)
                {
                    SendQuestMenu(ref client.Character, guid, "I have some tasks for you, $N.");
                }
                else
                {
                    WorldServiceLocator._WorldServer.CREATURESDatabase[WorldServiceLocator._WorldServer.WORLD_CREATUREs[guid].ID].TalkScript.OnGossipHello(ref client.Character, guid);
                }
            }
        }
        catch (Exception ex)
        {
            ProjectData.SetProjectError(ex);
            var e = ex;
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "On_CMSG_QUESTGIVER_HELLO - Error when sending quest menu.{0}", Environment.NewLine + e);
            ProjectData.ClearProjectError();
        }
    }

    public void On_CMSG_QUESTGIVER_QUERY_QUEST(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) < 17)
        {
            return;
        }
        packet.GetInt16();
        var guid = packet.GetUInt64();
        var questID = packet.GetInt32();
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_QUESTGIVER_QUERY_QUEST [GUID={2:X} QuestID={3}]", client.IP, client.Port, guid, questID);
        if (!WorldServiceLocator._WorldServer.ALLQUESTS.IsValidQuest(questID))
        {
            WS_QuestInfo tmpQuest = new(questID);
            try
            {
                client.Character.TalkCurrentQuest = tmpQuest;
                SendQuestDetails(ref client, ref client.Character.TalkCurrentQuest, guid, acceptActive: true);
            }
            catch (Exception ex3)
            {
                ProjectData.SetProjectError(ex3);
                var ex2 = ex3;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "On_CMSG_QUESTGIVER_QUERY_QUEST - Error while querying a quest.{0}{1}", Environment.NewLine, ex2.ToString());
                ProjectData.ClearProjectError();
            }
        }
        else
        {
            try
            {
                client.Character.TalkCurrentQuest = WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID);
                SendQuestDetails(ref client, ref client.Character.TalkCurrentQuest, guid, acceptActive: true);
            }
            catch (Exception ex4)
            {
                ProjectData.SetProjectError(ex4);
                var ex = ex4;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "On_CMSG_QUESTGIVER_QUERY_QUEST - Error while querying a quest.{0}{1}", Environment.NewLine, ex.ToString());
                ProjectData.ClearProjectError();
            }
        }
    }

    public void On_CMSG_QUESTGIVER_ACCEPT_QUEST(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) < 17)
        {
            return;
        }
        packet.GetInt16();
        var guid = packet.GetUInt64();
        var questID = packet.GetInt32();
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_QUESTGIVER_ACCEPT_QUEST [GUID={2:X} QuestID={3}]", client.IP, client.Port, guid, questID);
        if (!WorldServiceLocator._WorldServer.ALLQUESTS.IsValidQuest(questID))
        {
            WS_QuestInfo tmpQuest = new(questID);
            if (client.Character.TalkCurrentQuest.ID != questID)
            {
                client.Character.TalkCurrentQuest = tmpQuest;
            }
        }
        else if (client.Character.TalkCurrentQuest.ID != questID)
        {
            client.Character.TalkCurrentQuest = WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID);
        }
        if (!client.Character.TalkCanAccept(ref client.Character.TalkCurrentQuest))
        {
            return;
        }
        if (client.Character.TalkAddQuest(ref client.Character.TalkCurrentQuest))
        {
            if (WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(guid))
            {
                Packets.PacketClass response3 = new(Opcodes.MSG_QUEST_PUSH_RESULT);
                try
                {
                    response3.AddUInt64(client.Character.GUID);
                    response3.AddInt8(2);
                    response3.AddInt32(0);
                    WorldServiceLocator._WorldServer.CHARACTERs[guid].client.Send(ref response3);
                }
                finally
                {
                    response3.Dispose();
                }
            }
            else
            {
                var status = GetQuestgiverStatus(ref client.Character, guid);
                Packets.PacketClass response2 = new(Opcodes.SMSG_QUESTGIVER_STATUS);
                try
                {
                    response2.AddUInt64(guid);
                    response2.AddInt32((int)status);
                    client.Send(ref response2);
                }
                finally
                {
                    response2.Dispose();
                }
            }
        }
        else
        {
            Packets.PacketClass response = new(Opcodes.SMSG_QUESTLOG_FULL);
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

    public void On_CMSG_QUESTLOG_REMOVE_QUEST(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 6)
        {
            packet.GetInt16();
            var slot = packet.GetInt8();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_QUESTLOG_REMOVE_QUEST [Slot={2}]", client.IP, client.Port, slot);
            client.Character.TalkDeleteQuest(slot);
        }
    }

    public void On_CMSG_QUEST_QUERY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) < 9)
        {
            return;
        }
        packet.GetInt16();
        var questID = packet.GetInt32();
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_QUEST_QUERY [QuestID={2}]", client.IP, client.Port, questID);
        if (!WorldServiceLocator._WorldServer.ALLQUESTS.IsValidQuest(questID))
        {
            WS_QuestInfo tmpQuest = new(questID);
            if (client.Character.TalkCurrentQuest == null)
            {
                SendQuest(ref client, ref tmpQuest);
            }
            else if (client.Character.TalkCurrentQuest.ID == questID)
            {
                SendQuest(ref client, ref client.Character.TalkCurrentQuest);
            }
            else
            {
                SendQuest(ref client, ref tmpQuest);
            }
        }
        else if (client.Character.TalkCurrentQuest == null)
        {
            var quest = WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID);
            SendQuest(ref client, ref quest);
        }
        else if (client.Character.TalkCurrentQuest.ID == questID)
        {
            SendQuest(ref client, ref client.Character.TalkCurrentQuest);
        }
        else
        {
            var quest = WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID);
            SendQuest(ref client, ref quest);
        }
    }

    public void CompleteQuest(ref WS_PlayerData.CharacterObject objCharacter, int questID, ulong questGiverGuid)
    {
        if (!WorldServiceLocator._WorldServer.ALLQUESTS.IsValidQuest(questID))
        {
            WS_QuestInfo tmpQuest = new(questID);
            var j = 0;
            do
            {
                if (objCharacter.TalkQuests[j] != null && objCharacter.TalkQuests[j].ID == questID)
                {
                    if (objCharacter.TalkCurrentQuest == null)
                    {
                        objCharacter.TalkCurrentQuest = tmpQuest;
                    }
                    if (objCharacter.TalkCurrentQuest.ID != questID)
                    {
                        objCharacter.TalkCurrentQuest = tmpQuest;
                    }
                    if (objCharacter.TalkQuests[j].Complete)
                    {
                        if (((uint)objCharacter.TalkQuests[j].ObjectiveFlags & 0x20u) != 0)
                        {
                            SendQuestRequireItems(ref objCharacter.client, ref objCharacter.TalkCurrentQuest, questGiverGuid, ref objCharacter.TalkQuests[j]);
                        }
                        else
                        {
                            SendQuestReward(ref objCharacter.client, ref objCharacter.TalkCurrentQuest, questGiverGuid, ref objCharacter.TalkQuests[j]);
                        }
                    }
                    else
                    {
                        SendQuestRequireItems(ref objCharacter.client, ref objCharacter.TalkCurrentQuest, questGiverGuid, ref objCharacter.TalkQuests[j]);
                    }
                    break;
                }
                j = checked(j + 1);
            }
            while (j <= 24);
            return;
        }
        var i = 0;
        do
        {
            if (objCharacter.TalkQuests[i] != null && objCharacter.TalkQuests[i].ID == questID)
            {
                if (objCharacter.TalkCurrentQuest == null)
                {
                    objCharacter.TalkCurrentQuest = WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID);
                }
                if (objCharacter.TalkCurrentQuest.ID != questID)
                {
                    objCharacter.TalkCurrentQuest = WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID);
                }
                if (objCharacter.TalkQuests[i].Complete)
                {
                    if (((uint)objCharacter.TalkQuests[i].ObjectiveFlags & 0x20u) != 0)
                    {
                        SendQuestRequireItems(ref objCharacter.client, ref objCharacter.TalkCurrentQuest, questGiverGuid, ref objCharacter.TalkQuests[i]);
                    }
                    else
                    {
                        SendQuestReward(ref objCharacter.client, ref objCharacter.TalkCurrentQuest, questGiverGuid, ref objCharacter.TalkQuests[i]);
                    }
                }
                else
                {
                    SendQuestRequireItems(ref objCharacter.client, ref objCharacter.TalkCurrentQuest, questGiverGuid, ref objCharacter.TalkQuests[i]);
                }
                break;
            }
            i = checked(i + 1);
        }
        while (i <= 24);
    }

    public void On_CMSG_QUESTGIVER_COMPLETE_QUEST(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 17)
        {
            packet.GetInt16();
            var guid = packet.GetUInt64();
            var questID = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_QUESTGIVER_COMPLETE_QUEST [GUID={2:X} Quest={3}]", client.IP, client.Port, guid, questID);
            CompleteQuest(ref client.Character, questID, guid);
        }
    }

    public void On_CMSG_QUESTGIVER_REQUEST_REWARD(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        checked
        {
            if (packet.Data.Length - 1 < 17)
            {
                return;
            }
            packet.GetInt16();
            var guid = packet.GetUInt64();
            var questID = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_QUESTGIVER_REQUEST_REWARD [GUID={2:X} Quest={3}]", client.IP, client.Port, guid, questID);
            if (!WorldServiceLocator._WorldServer.ALLQUESTS.IsValidQuest(questID))
            {
                WS_QuestInfo tmpQuest = new(questID);
                var j = 0;
                do
                {
                    if (client.Character.TalkQuests[j] != null && client.Character.TalkQuests[j].ID == questID && client.Character.TalkQuests[j].Complete)
                    {
                        if (client.Character.TalkCurrentQuest.ID != questID)
                        {
                            client.Character.TalkCurrentQuest = tmpQuest;
                        }
                        SendQuestReward(ref client, ref client.Character.TalkCurrentQuest, guid, ref client.Character.TalkQuests[j]);
                        break;
                    }
                    j++;
                }
                while (j <= 24);
                return;
            }
            var i = 0;
            do
            {
                if (client.Character.TalkQuests[i] != null && client.Character.TalkQuests[i].ID == questID && client.Character.TalkQuests[i].Complete)
                {
                    if (client.Character.TalkCurrentQuest.ID != questID)
                    {
                        client.Character.TalkCurrentQuest = WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID);
                    }
                    SendQuestReward(ref client, ref client.Character.TalkCurrentQuest, guid, ref client.Character.TalkQuests[i]);
                    break;
                }
                i++;
            }
            while (i <= 24);
        }
    }

    public void On_CMSG_QUESTGIVER_CHOOSE_REWARD(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        checked
        {
            if (packet.Data.Length - 1 < 21)
            {
                return;
            }
            packet.GetInt16();
            var guid = packet.GetUInt64();
            var questID = packet.GetInt32();
            var rewardIndex = packet.GetInt32();
            if (!WorldServiceLocator._WorldServer.ALLQUESTS.IsValidQuest(questID))
            {
                try
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_QUESTGIVER_CHOOSE_REWARD [GUID={2:X} Quest={3} Reward={4}]", client.IP, client.Port, guid, questID, rewardIndex);
                    if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(guid))
                    {
                        if (client.Character.TalkCurrentQuest == null)
                        {
                            client.Character.TalkCurrentQuest = WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID);
                        }
                        if (client.Character.TalkCurrentQuest.ID != questID)
                        {
                            client.Character.TalkCurrentQuest = WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID);
                        }
                        if (client.Character.TalkCurrentQuest.RewardGold >= 0)
                        {
                            goto IL_01d2;
                        }
                        if (-client.Character.TalkCurrentQuest.RewardGold <= client.Character.Copper)
                        {
                            ref var copper = ref client.Character.Copper;
                            copper = (uint)(copper + client.Character.TalkCurrentQuest.RewardGold);
                            goto IL_01d2;
                        }
                        Packets.PacketClass errorPacket = new(Opcodes.SMSG_QUESTGIVER_QUEST_INVALID);
                        errorPacket.AddInt32(23);
                        client.Send(ref errorPacket);
                        errorPacket.Dispose();
                    }
                    goto end_IL_0055;
                IL_01d2:
                    var k = 0;
                    while (client.Character.TalkCurrentQuest.ObjectivesItem[k] != 0)
                    {
                        if (!client.Character.ItemCONSUME(client.Character.TalkCurrentQuest.ObjectivesItem[k], client.Character.TalkCurrentQuest.ObjectivesItem_Count[k]))
                        {
                            if (client.Character.TalkCurrentQuest.RewardGold < 0)
                            {
                                ref var copper2 = ref client.Character.Copper;
                                copper2 = (uint)(copper2 - client.Character.TalkCurrentQuest.RewardGold);
                            }
                            Packets.PacketClass errorPacket3 = new(Opcodes.SMSG_QUESTGIVER_QUEST_INVALID);
                            errorPacket3.AddInt32(21);
                            client.Send(ref errorPacket3);
                            errorPacket3.Dispose();
                            return;
                        }
                        k++;
                        if (k > 4)
                        {
                            break;
                        }
                    }
                    if (client.Character.TalkCurrentQuest.RewardItems[rewardIndex] == 0)
                    {
                        goto IL_034e;
                    }
                    ItemObject tmpItem2 = new(client.Character.TalkCurrentQuest.RewardItems[rewardIndex], client.Character.GUID)
                    {
                        StackCount = client.Character.TalkCurrentQuest.RewardItems_Count[rewardIndex]
                    };
                    if (!client.Character.ItemADD(ref tmpItem2))
                    {
                        tmpItem2.Delete();
                        return;
                    }
                    client.Character.LogLootItem(tmpItem2, 1, Recieved: true, Created: false);
                IL_034e:
                    if (client.Character.TalkCurrentQuest.RewardGold > 0)
                    {
                        ref var copper3 = ref client.Character.Copper;
                        copper3 = (uint)(copper3 + client.Character.TalkCurrentQuest.RewardGold);
                    }
                    client.Character.SetUpdateFlag(1176, client.Character.Copper);
                    if (client.Character.TalkCurrentQuest.RewardHonor != 0)
                    {
                        client.Character.HonorPoints += client.Character.TalkCurrentQuest.RewardHonor;
                    }
                    if (client.Character.TalkCurrentQuest.RewardSpell > 0)
                    {
                        WS_Spells.SpellTargets spellTargets2 = new();
                        var spellTargets3 = spellTargets2;
                        ref var character = ref client.Character;
                        ref var reference = ref character;
                        WS_Base.BaseUnit objCharacter = character;
                        spellTargets3.SetTarget_UNIT(ref objCharacter);
                        reference = (WS_PlayerData.CharacterObject)objCharacter;
                        Dictionary<ulong, WS_Creatures.CreatureObject> wORLD_CREATUREs;
                        ulong key;
                        WS_Base.BaseObject Caster = (wORLD_CREATUREs = WorldServiceLocator._WorldServer.WORLD_CREATUREs)[key = guid];
                        WS_Spells.CastSpellParameters castSpellParameters = new(ref spellTargets2, ref Caster, client.Character.TalkCurrentQuest.RewardSpell, Instant: true);
                        wORLD_CREATUREs[key] = (WS_Creatures.CreatureObject)Caster;
                        var castParams2 = castSpellParameters;
                        ThreadPool.QueueUserWorkItem(castParams2.Cast);
                    }
                    var l = 0;
                    do
                    {
                        if (client.Character.TalkQuests[l] != null && client.Character.TalkQuests[l].ID == client.Character.TalkCurrentQuest.ID)
                        {
                            client.Character.TalkCompleteQuest((byte)l);
                            break;
                        }
                        l++;
                    }
                    while (l <= 24);
                    var xp2 = 0;
                    var gold2 = client.Character.TalkCurrentQuest.RewardGold;
                    if (client.Character.TalkCurrentQuest.RewMoneyMaxLevel > 0)
                    {
                        var reqMoneyMaxLevel2 = client.Character.TalkCurrentQuest.RewMoneyMaxLevel;
                        int pLevel2 = client.Character.Level;
                        int qLevel2 = client.Character.TalkCurrentQuest.Level_Normal;
                        var fullxp2 = 0f;
                        if (pLevel2 <= WorldServiceLocator._WS_Player_Initializator.DEFAULT_MAX_LEVEL)
                        {
                            if (qLevel2 >= 65)
                            {
                                fullxp2 = reqMoneyMaxLevel2 / 6f;
                            }
                            else if (qLevel2 == 64)
                            {
                                fullxp2 = reqMoneyMaxLevel2 / 4.8f;
                            }
                            else if (qLevel2 == 63)
                            {
                                fullxp2 = reqMoneyMaxLevel2 / 3.6f;
                            }
                            else if (qLevel2 == 62)
                            {
                                fullxp2 = reqMoneyMaxLevel2 / 2.4f;
                            }
                            else if (qLevel2 == 61)
                            {
                                fullxp2 = reqMoneyMaxLevel2 / 1.2f;
                            }
                            else if (qLevel2 is > 0 and <= 60)
                            {
                                fullxp2 = reqMoneyMaxLevel2 / 0.6f;
                            }
                            xp2 = (pLevel2 <= qLevel2 + 5) ? ((int)fullxp2) : ((pLevel2 == qLevel2 + 6) ? ((int)(fullxp2 * 0.8f)) : ((pLevel2 == qLevel2 + 7) ? ((int)(fullxp2 * 0.6f)) : ((pLevel2 == qLevel2 + 8) ? ((int)(fullxp2 * 0.4f)) : ((pLevel2 != qLevel2 + 9) ? ((int)(fullxp2 * 0.1f)) : ((int)(fullxp2 * 0.2f))))));
                            client.Character.AddXP(xp2, 0);
                        }
                        else
                        {
                            gold2 += reqMoneyMaxLevel2;
                        }
                    }
                    if (gold2 < 0 && -gold2 >= client.Character.Copper)
                    {
                        client.Character.Copper = 0u;
                    }
                    else
                    {
                        ref var copper4 = ref client.Character.Copper;
                        copper4 = (uint)(copper4 + gold2);
                    }
                    client.Character.SetUpdateFlag(1176, client.Character.Copper);
                    client.Character.SendCharacterUpdate();
                    SendQuestComplete(ref client, ref client.Character.TalkCurrentQuest, xp2, gold2);
                    if (client.Character.TalkCurrentQuest.NextQuest != 0)
                    {
                        if (!WorldServiceLocator._WorldServer.ALLQUESTS.IsValidQuest(client.Character.TalkCurrentQuest.NextQuest))
                        {
                            WS_QuestInfo tmpQuest2 = new(client.Character.TalkCurrentQuest.NextQuest);
                            client.Character.TalkCurrentQuest = tmpQuest2;
                        }
                        else
                        {
                            client.Character.TalkCurrentQuest = WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(client.Character.TalkCurrentQuest.NextQuest);
                        }
                        SendQuestDetails(ref client, ref client.Character.TalkCurrentQuest, guid, acceptActive: true);
                    }
                end_IL_0055:
                    ;
                }
                catch (Exception ex)
                {
                    ProjectData.SetProjectError(ex);
                    var e2 = ex;
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "On_CMSG_QUESTGIVER_CHOOSE_REWARD - Error while choosing reward.{0}", Environment.NewLine + e2);
                    ProjectData.ClearProjectError();
                }
                return;
            }
            try
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_QUESTGIVER_CHOOSE_REWARD [GUID={2:X} Quest={3} Reward={4}]", client.IP, client.Port, guid, questID, rewardIndex);
                if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(guid))
                {
                    if (client.Character.TalkCurrentQuest == null)
                    {
                        client.Character.TalkCurrentQuest = WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID);
                    }
                    if (client.Character.TalkCurrentQuest.ID != questID)
                    {
                        client.Character.TalkCurrentQuest = WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID);
                    }
                    if (client.Character.TalkCurrentQuest.RewardGold >= 0)
                    {
                        goto IL_0a03;
                    }
                    if (-client.Character.TalkCurrentQuest.RewardGold <= client.Character.Copper)
                    {
                        ref var copper5 = ref client.Character.Copper;
                        copper5 = (uint)(copper5 + client.Character.TalkCurrentQuest.RewardGold);
                        goto IL_0a03;
                    }
                    Packets.PacketClass errorPacket4 = new(Opcodes.SMSG_QUESTGIVER_QUEST_INVALID);
                    errorPacket4.AddInt32(23);
                    client.Send(ref errorPacket4);
                    errorPacket4.Dispose();
                }
                goto end_IL_0886;
            IL_0a03:
                var j = 0;
                while (client.Character.TalkCurrentQuest.ObjectivesItem[j] != 0)
                {
                    if (!client.Character.ItemCONSUME(client.Character.TalkCurrentQuest.ObjectivesItem[j], client.Character.TalkCurrentQuest.ObjectivesItem_Count[j]))
                    {
                        if (client.Character.TalkCurrentQuest.RewardGold < 0)
                        {
                            ref var copper6 = ref client.Character.Copper;
                            copper6 = (uint)(copper6 - client.Character.TalkCurrentQuest.RewardGold);
                        }
                        Packets.PacketClass errorPacket2 = new(Opcodes.SMSG_QUESTGIVER_QUEST_INVALID);
                        errorPacket2.AddInt32(21);
                        client.Send(ref errorPacket2);
                        errorPacket2.Dispose();
                        return;
                    }
                    j++;
                    if (j > 4)
                    {
                        break;
                    }
                }
                if (client.Character.TalkCurrentQuest.RewardItems[rewardIndex] == 0)
                {
                    goto IL_0b7f;
                }
                ItemObject tmpItem = new(client.Character.TalkCurrentQuest.RewardItems[rewardIndex], client.Character.GUID)
                {
                    StackCount = client.Character.TalkCurrentQuest.RewardItems_Count[rewardIndex]
                };
                if (!client.Character.ItemADD(ref tmpItem))
                {
                    tmpItem.Delete();
                    return;
                }
                client.Character.LogLootItem(tmpItem, 1, Recieved: true, Created: false);
            IL_0b7f:
                if (client.Character.TalkCurrentQuest.RewardGold > 0)
                {
                    ref var copper7 = ref client.Character.Copper;
                    copper7 = (uint)(copper7 + client.Character.TalkCurrentQuest.RewardGold);
                }
                client.Character.SetUpdateFlag(1176, client.Character.Copper);
                if (client.Character.TalkCurrentQuest.RewardHonor != 0)
                {
                    client.Character.HonorPoints += client.Character.TalkCurrentQuest.RewardHonor;
                }
                if (client.Character.TalkCurrentQuest.RewardSpell > 0)
                {
                    WS_Spells.SpellTargets spellTargets = new();
                    var spellTargets4 = spellTargets;
                    ref var character2 = ref client.Character;
                    ref var reference = ref character2;
                    WS_Base.BaseUnit objCharacter = character2;
                    spellTargets4.SetTarget_UNIT(ref objCharacter);
                    reference = (WS_PlayerData.CharacterObject)objCharacter;
                    Dictionary<ulong, WS_Creatures.CreatureObject> wORLD_CREATUREs;
                    ulong key;
                    WS_Base.BaseObject Caster = (wORLD_CREATUREs = WorldServiceLocator._WorldServer.WORLD_CREATUREs)[key = guid];
                    WS_Spells.CastSpellParameters castSpellParameters = new(ref spellTargets, ref Caster, client.Character.TalkCurrentQuest.RewardSpell, Instant: true);
                    wORLD_CREATUREs[key] = (WS_Creatures.CreatureObject)Caster;
                    var castParams = castSpellParameters;
                    ThreadPool.QueueUserWorkItem(castParams.Cast);
                }
                var i = 0;
                do
                {
                    if (client.Character.TalkQuests[i] != null && client.Character.TalkQuests[i].ID == client.Character.TalkCurrentQuest.ID)
                    {
                        client.Character.TalkCompleteQuest((byte)i);
                        break;
                    }
                    i++;
                }
                while (i <= 24);
                var xp = 0;
                var gold = client.Character.TalkCurrentQuest.RewardGold;
                if (client.Character.TalkCurrentQuest.RewMoneyMaxLevel > 0)
                {
                    var reqMoneyMaxLevel = client.Character.TalkCurrentQuest.RewMoneyMaxLevel;
                    int pLevel = client.Character.Level;
                    int qLevel = client.Character.TalkCurrentQuest.Level_Normal;
                    var fullxp = 0f;
                    if (pLevel <= WorldServiceLocator._WS_Player_Initializator.DEFAULT_MAX_LEVEL)
                    {
                        if (qLevel >= 65)
                        {
                            fullxp = reqMoneyMaxLevel / 6f;
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
                        else if (qLevel is > 0 and <= 60)
                        {
                            fullxp = reqMoneyMaxLevel / 0.6f;
                        }
                        xp = (pLevel <= qLevel + 5) ? ((int)fullxp) : ((pLevel == qLevel + 6) ? ((int)(fullxp * 0.8f)) : ((pLevel == qLevel + 7) ? ((int)(fullxp * 0.6f)) : ((pLevel == qLevel + 8) ? ((int)(fullxp * 0.4f)) : ((pLevel != qLevel + 9) ? ((int)(fullxp * 0.1f)) : ((int)(fullxp * 0.2f))))));
                        client.Character.AddXP(xp, 0);
                    }
                    else
                    {
                        gold += reqMoneyMaxLevel;
                    }
                }
                if (gold < 0 && -gold >= client.Character.Copper)
                {
                    client.Character.Copper = 0u;
                }
                else
                {
                    ref var copper8 = ref client.Character.Copper;
                    copper8 = (uint)(copper8 + gold);
                }
                client.Character.SetUpdateFlag(1176, client.Character.Copper);
                client.Character.SendCharacterUpdate();
                SendQuestComplete(ref client, ref client.Character.TalkCurrentQuest, xp, gold);
                if (client.Character.TalkCurrentQuest.NextQuest != 0)
                {
                    if (!WorldServiceLocator._WorldServer.ALLQUESTS.IsValidQuest(client.Character.TalkCurrentQuest.NextQuest))
                    {
                        WS_QuestInfo tmpQuest3 = new(client.Character.TalkCurrentQuest.NextQuest);
                        client.Character.TalkCurrentQuest = tmpQuest3;
                    }
                    else
                    {
                        client.Character.TalkCurrentQuest = WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(client.Character.TalkCurrentQuest.NextQuest);
                    }
                    SendQuestDetails(ref client, ref client.Character.TalkCurrentQuest, guid, acceptActive: true);
                }
            end_IL_0886:
                ;
            }
            catch (Exception ex2)
            {
                ProjectData.SetProjectError(ex2);
                var e = ex2;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "On_CMSG_QUESTGIVER_CHOOSE_REWARD - Error while choosing reward.{0}", Environment.NewLine + e);
                ProjectData.ClearProjectError();
            }
        }
    }

    public void On_CMSG_PUSHQUESTTOPARTY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) < 9)
        {
            return;
        }
        packet.GetInt16();
        var questID = packet.GetInt32();
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_PUSHQUESTTOPARTY [{2}]", client.IP, client.Port, questID);
        if (!client.Character.IsInGroup)
        {
            return;
        }
        if (!WorldServiceLocator._WorldServer.ALLQUESTS.IsValidQuest(questID))
        {
            WS_QuestInfo tmpQuest = new(questID);
            foreach (var guid in client.Character.Group.LocalMembers)
            {
                if (guid == client.Character.GUID)
                {
                    continue;
                }
                var characterObject = WorldServiceLocator._WorldServer.CHARACTERs[guid];
                Packets.PacketClass response2 = new(Opcodes.MSG_QUEST_PUSH_RESULT);
                response2.AddUInt64(guid);
                response2.AddInt32(0);
                response2.AddInt8(0);
                client.Send(ref response2);
                response2.Dispose();
                var message2 = QuestPartyPushError.QUEST_PARTY_MSG_SHARRING_QUEST;
                if (Math.Sqrt(Math.Pow(characterObject.positionX - client.Character.positionX, 2.0) + Math.Pow(characterObject.positionY - client.Character.positionY, 2.0)) > 10.0)
                {
                    message2 = QuestPartyPushError.QUEST_PARTY_MSG_TO_FAR;
                }
                else if (characterObject.IsQuestInProgress(questID))
                {
                    message2 = QuestPartyPushError.QUEST_PARTY_MSG_HAVE_QUEST;
                }
                else if (characterObject.IsQuestCompleted(questID))
                {
                    message2 = QuestPartyPushError.QUEST_PARTY_MSG_FINISH_QUEST;
                }
                else
                {
                    if (characterObject.TalkCurrentQuest == null || characterObject.TalkCurrentQuest.ID != questID)
                    {
                        characterObject.TalkCurrentQuest = tmpQuest;
                    }
                    if (characterObject.TalkCanAccept(ref characterObject.TalkCurrentQuest))
                    {
                        SendQuestDetails(ref characterObject.client, ref characterObject.TalkCurrentQuest, client.Character.GUID, acceptActive: true);
                    }
                    else
                    {
                        message2 = QuestPartyPushError.QUEST_PARTY_MSG_CANT_TAKE_QUEST;
                    }
                }
                if (message2 != 0)
                {
                    Packets.PacketClass errorPacket2 = new(Opcodes.MSG_QUEST_PUSH_RESULT);
                    errorPacket2.AddUInt64(characterObject.GUID);
                    errorPacket2.AddInt32((int)message2);
                    errorPacket2.AddInt8(0);
                    client.Send(ref errorPacket2);
                    errorPacket2.Dispose();
                }
            }
            return;
        }
        foreach (var guid2 in client.Character.Group.LocalMembers)
        {
            if (guid2 == client.Character.GUID)
            {
                continue;
            }
            var characterObject2 = WorldServiceLocator._WorldServer.CHARACTERs[guid2];
            Packets.PacketClass response = new(Opcodes.MSG_QUEST_PUSH_RESULT);
            response.AddUInt64(guid2);
            response.AddInt32(0);
            response.AddInt8(0);
            client.Send(ref response);
            response.Dispose();
            var message = QuestPartyPushError.QUEST_PARTY_MSG_SHARRING_QUEST;
            if (Math.Sqrt(Math.Pow(characterObject2.positionX - client.Character.positionX, 2.0) + Math.Pow(characterObject2.positionY - client.Character.positionY, 2.0)) > 10.0)
            {
                message = QuestPartyPushError.QUEST_PARTY_MSG_TO_FAR;
            }
            else if (characterObject2.IsQuestInProgress(questID))
            {
                message = QuestPartyPushError.QUEST_PARTY_MSG_HAVE_QUEST;
            }
            else if (characterObject2.IsQuestCompleted(questID))
            {
                message = QuestPartyPushError.QUEST_PARTY_MSG_FINISH_QUEST;
            }
            else
            {
                if (characterObject2.TalkCurrentQuest == null || characterObject2.TalkCurrentQuest.ID != questID)
                {
                    characterObject2.TalkCurrentQuest = WorldServiceLocator._WorldServer.ALLQUESTS.ReturnQuestInfoById(questID);
                }
                if (characterObject2.TalkCanAccept(ref characterObject2.TalkCurrentQuest))
                {
                    SendQuestDetails(ref characterObject2.client, ref characterObject2.TalkCurrentQuest, client.Character.GUID, acceptActive: true);
                }
                else
                {
                    message = QuestPartyPushError.QUEST_PARTY_MSG_CANT_TAKE_QUEST;
                }
            }
            if (message != 0)
            {
                Packets.PacketClass errorPacket = new(Opcodes.MSG_QUEST_PUSH_RESULT);
                errorPacket.AddUInt64(characterObject2.GUID);
                errorPacket.AddInt32((int)message);
                errorPacket.AddInt8(0);
                client.Send(ref errorPacket);
                errorPacket.Dispose();
            }
        }
    }

    public void On_MSG_QUEST_PUSH_RESULT(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (checked(packet.Data.Length - 1) >= 14)
        {
            packet.GetInt16();
            var guid = packet.GetUInt64();
            QuestPartyPushError message = (QuestPartyPushError)packet.GetInt8();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_QUEST_PUSH_RESULT [{2:X} {3}]", client.IP, client.Port, guid, message);
            Packets.PacketClass response = new(Opcodes.MSG_QUEST_PUSH_RESULT);
            response.AddUInt64(guid);
            response.AddInt8(2);
            response.AddInt32(0);
            client.Send(ref response);
            response.Dispose();
        }
    }

    public WS_Quests()
    {
        _quests = new Collection();
    }
}
