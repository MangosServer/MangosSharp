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
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Quest;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Objects;
using Mangos.World.Player;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Quests
{
    public class WS_QuestsBase : IDisposable
    {

        // WARNING: These are used only for CharManagment
        public int ID; // = 0
        public string Title; // = ""
        public int SpecialFlags; // = 0
        public int ObjectiveFlags; // = 0
        public byte Slot; // = 0
        public byte[] ObjectivesType; // = {0, 0, 0, 0}
        public int ObjectivesDeliver;
        public int[] ObjectivesExplore;
        public int[] ObjectivesSpell;
        public int[] ObjectivesItem;
        public byte[] ObjectivesItemCount; // = {0, 0, 0, 0}
        public int[] ObjectivesObject;
        public byte[] ObjectivesCount; // = {0, 0, 0, 0}
        public bool Explored; // = True
        public byte[] Progress; // = {0, 0, 0, 0}
        public byte[] ProgressItem; // = {0, 0, 0, 0}
        public bool Complete; // = False
        public bool Failed; // = False
        public int TimeEnd; // = 0

        public WS_QuestsBase()
        {
        }

        public WS_QuestsBase(WS_QuestInfo Quest)
        {
            ID = 0;
            Title = "";
            SpecialFlags = 0;
            ObjectiveFlags = 0;
            Slot = 0;
            ObjectivesType = new[] { 0, 0, 0, 0 };
            ObjectivesItemCount = new[] { 0, 0, 0, 0 };
            ObjectivesCount = new[] { 0, 0, 0, 0 };
            ObjectivesObject = new[] { 0, 0, 0, 0 };
            ObjectivesExplore = new[] { 0, 0, 0, 0 };
            ObjectivesSpell = new[] { 0, 0, 0, 0 };
            ObjectivesItem = new[] { 0, 0, 0, 0 };
            Explored = true;
            Progress = new[] { 0, 0, 0, 0 };
            ProgressItem = new[] { 0, 0, 0, 0 };
            Complete = false;
            Failed = false;
            TimeEnd = 0;

            // Load Spell Casts
            for (byte bytLoop = 0; bytLoop <= 3; bytLoop++)
            {
                if (Quest.ObjectivesCastSpell[bytLoop] > 0)
                {
                    ObjectiveFlags = ObjectiveFlags | QuestObjectiveFlag.QUEST_OBJECTIVE_CAST;
                    ObjectivesType[bytLoop] = (byte)QuestObjectiveFlag.QUEST_OBJECTIVE_CAST;
                    ObjectivesSpell[bytLoop] = Quest.ObjectivesCastSpell[bytLoop];
                    ObjectivesObject[0] = Quest.ObjectivesKill[bytLoop];
                    ObjectivesCount[0] = (byte)Quest.ObjectivesKill_Count[bytLoop];
                }
            }

            // Load Kills
            for (byte bytLoop = 0; bytLoop <= 3; bytLoop++)
            {
                if (Quest.ObjectivesKill[bytLoop] > 0)
                {
                    for (byte bytLoop2 = 0; bytLoop2 <= 3; bytLoop2++)
                    {
                        if (ObjectivesType[bytLoop2] == 0)
                        {
                            ObjectiveFlags = ObjectiveFlags | QuestObjectiveFlag.QUEST_OBJECTIVE_KILL;
                            ObjectivesType[bytLoop2] = (byte)QuestObjectiveFlag.QUEST_OBJECTIVE_KILL;
                            ObjectivesObject[bytLoop2] = Quest.ObjectivesKill[bytLoop];
                            ObjectivesCount[bytLoop2] = (byte)Quest.ObjectivesKill_Count[bytLoop];
                            break;
                        }
                    }
                }
            }

            // Load Items
            for (byte bytLoop = 0; bytLoop <= 3; bytLoop++)
            {
                if (Quest.ObjectivesItem[bytLoop] > 0)
                {
                    ObjectiveFlags = ObjectiveFlags | QuestObjectiveFlag.QUEST_OBJECTIVE_ITEM;
                    ObjectivesType[bytLoop] = (byte)QuestObjectiveFlag.QUEST_OBJECTIVE_ITEM;
                    ObjectivesItem[bytLoop] = Quest.ObjectivesItem[bytLoop];
                    ObjectivesItemCount[bytLoop] = (byte)Quest.ObjectivesItem_Count[bytLoop];
                }
            }

            // Load Exploration loctions
            if (Quest.SpecialFlags & QuestSpecialFlag.QUEST_SPECIALFLAGS_EXPLORE)
            {
                ObjectiveFlags = ObjectiveFlags | QuestObjectiveFlag.QUEST_OBJECTIVE_EXPLORE;
                for (byte bytLoop = 0; bytLoop <= 3; bytLoop++)
                {
                    ObjectivesType[bytLoop] = (byte)QuestObjectiveFlag.QUEST_OBJECTIVE_EXPLORE;
                    ObjectivesExplore[bytLoop] = Quest.ObjectivesTrigger[bytLoop];
                }
            }
            // 'TODO: Fix this below
            if (Quest.SpecialFlags & QuestObjectiveFlag.QUEST_OBJECTIVE_EVENT)
            {
                ObjectiveFlags = ObjectiveFlags | QuestObjectiveFlag.QUEST_OBJECTIVE_EVENT;
                for (int i = 0; i <= 3; i++)
                {
                    if (ObjectivesType[i] == 0)
                    {
                        ObjectivesType[i] = (byte)QuestObjectiveFlag.QUEST_OBJECTIVE_EVENT;
                        ObjectivesCount[i] = 1;
                    }
                }
            }

            // No objective flags are set, complete it directly
            if (ObjectiveFlags == 0)
            {
                for (byte bytLoop = 0; bytLoop <= 3; bytLoop++)
                {
                    // Make sure these are zero
                    ObjectivesObject[bytLoop] = 0;
                    ObjectivesCount[bytLoop] = 0;
                    ObjectivesExplore[bytLoop] = 0;
                    ObjectivesSpell[bytLoop] = 0;
                    ObjectivesType[bytLoop] = 0;
                }

                IsCompleted();
            }

            Title = Quest.Title;
            ID = Quest.ID;
            SpecialFlags = Quest.SpecialFlags;
            ObjectivesDeliver = Quest.ObjectivesDeliver;
            // TODO: Fix a timer or something so that the quest really expires when it does
            if (Quest.TimeLimit > 0)
                TimeEnd = (int)(WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now) + Quest.TimeLimit); // The time the quest expires
        }

        /// <summary>
        /// Updates the item count.
        /// </summary>
        /// <param name="objCharacter">The Character.</param>
        public void UpdateItemCount(ref WS_PlayerData.CharacterObject objCharacter)
        {
            // DONE: Update item count at login
            for (byte i = 0; i <= 3; i++)
            {
                if (ObjectivesItem[i] != 0)
                {
                    ProgressItem[i] = (byte)objCharacter.ItemCOUNT(ObjectivesItem[i]);
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "ITEM COUNT UPDATED TO: {0}", ProgressItem[i]);
                }
            }

            // DONE: If the quest doesn't require any explore than set this as completed
            if ((ObjectiveFlags & QuestObjectiveFlag.QUEST_OBJECTIVE_EXPLORE) == 0)
                Explored = true;

            // DONE: Check if the quest is completed
            IsCompleted();
        }

        /// <summary>
        /// Initializes the specified objCharacter.
        /// </summary>
        /// <param name="objCharacter">The Character.</param>
        public void Initialize(ref WS_PlayerData.CharacterObject objCharacter)
        {
            if (ObjectivesDeliver > 0)
            {
                var tmpItem = new ItemObject(ObjectivesDeliver, objCharacter.GUID);
                if (!objCharacter.ItemADD(ref tmpItem))
                {
                    // DONE: Some error, unable to add item, quest is uncompletable
                    tmpItem.Delete();
                    var response = new Packets.PacketClass(OPCODES.SMSG_QUESTGIVER_QUEST_FAILED);
                    response.AddInt32(ID);
                    response.AddInt32((int)QuestFailedReason.FAILED_INVENTORY_FULL);
                    objCharacter.client.Send(response);
                    response.Dispose();
                    return;
                }
                else
                {
                    objCharacter.LogLootItem(tmpItem, 1, true, false);
                }
            }

            for (byte i = 0; i <= 3; i++)
            {
                if (ObjectivesItem[i] != 0)
                    ProgressItem[i] = (byte)objCharacter.ItemCOUNT(ObjectivesItem[i]);
            }

            if (ObjectiveFlags & QuestObjectiveFlag.QUEST_OBJECTIVE_EXPLORE)
                Explored = false;
            IsCompleted();
        }

        /// <summary>
        /// Determines whether this instance is completed.
        /// </summary>
        /// <returns>Boolean</returns>
        public virtual bool IsCompleted()
        {
            Complete = ObjectivesCount[0] <= Progress[0] && ObjectivesCount[1] <= Progress[1] && ObjectivesCount[2] <= Progress[2] && ObjectivesCount[3] <= Progress[3] && ObjectivesItemCount[0] <= ProgressItem[0] && ObjectivesItemCount[1] <= ProgressItem[1] && ObjectivesItemCount[2] <= ProgressItem[2] && ObjectivesItemCount[3] <= ProgressItem[3] && Explored && Failed == false;
            return Complete;
        }

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <param name="ForSave">if set to <c>true</c> [for save].</param>
        /// <returns>Integer <c>1 = Complere</c><c>2 = Failed</c></returns>
        public virtual int GetState(bool ForSave = false)
        {
            var tmpState = default(int);
            if (Complete)
                tmpState = 1;
            if (Failed)
                tmpState = 2;
            return tmpState;
        }

        /// <summary>
        /// Gets the progress.
        /// </summary>
        /// <param name="ForSave">if set to <c>true</c> [for save].</param>
        /// <returns></returns>
        public virtual int GetProgress(bool ForSave = false)
        {
            int tmpProgress = 0;
            if (ForSave)
            {
                tmpProgress += Progress[0];
                tmpProgress += Conversions.ToInteger(Progress[1]) << 6;
                tmpProgress += Conversions.ToInteger(Progress[2]) << 12;
                tmpProgress += Conversions.ToInteger(Progress[3]) << 18;
                if (Explored)
                    tmpProgress += 1 << 24;
                if (Complete)
                    tmpProgress += 1 << 25;
                if (Failed)
                    tmpProgress += 1 << 26;
            }
            else
            {
                tmpProgress += Progress[0];
                tmpProgress += Conversions.ToInteger(Progress[1]) << 6;
                tmpProgress += Conversions.ToInteger(Progress[2]) << 12;
                tmpProgress += Conversions.ToInteger(Progress[3]) << 18;
                if (Complete)
                    tmpProgress += 1 << 24;
                if (Failed)
                    tmpProgress += 1 << 25;
            }

            return tmpProgress;
        }

        /// <summary>
        /// Loads the state.
        /// </summary>
        /// <param name="state">The state.</param>
        public virtual void LoadState(int state)
        {
            Progress[0] = (byte)(state & 0x3F);
            Progress[1] = (byte)(state >> 6 & 0x3F);
            Progress[2] = (byte)(state >> 12 & 0x3F);
            Progress[3] = (byte)(state >> 18 & 0x3F);
            Explored = (state >> 24 & 0x1) == 1;
            Complete = (state >> 25 & 0x1) == 1;
            Failed = (state >> 26 & 0x1) == 1;
        }

        /// <summary>
        /// Adds the kill.
        /// </summary>
        /// <param name="objCharacter">The Character.</param>
        /// <param name="index">The index.</param>
        /// <param name="oGUID">The o unique identifier.</param>
        public void AddKill(WS_PlayerData.CharacterObject objCharacter, byte index, ulong oGUID)
        {
            Progress[index] = (byte)(Progress[index] + 1);
            IsCompleted();
            objCharacter.TalkUpdateQuest(Slot);
            WorldServiceLocator._WorldServer.ALLQUESTS.SendQuestMessageAddKill(ref objCharacter.client, ID, oGUID, ObjectivesObject[index], Progress[index], ObjectivesCount[index]);
        }

        /// <summary>
        /// Adds the cast.
        /// </summary>
        /// <param name="objCharacter">The Character.</param>
        /// <param name="index">The index.</param>
        /// <param name="oGUID">The o unique identifier.</param>
        public void AddCast(WS_PlayerData.CharacterObject objCharacter, byte index, ulong oGUID)
        {
            Progress[index] = (byte)(Progress[index] + 1);
            IsCompleted();
            objCharacter.TalkUpdateQuest(Slot);
            WorldServiceLocator._WorldServer.ALLQUESTS.SendQuestMessageAddKill(ref objCharacter.client, ID, oGUID, ObjectivesObject[index], Progress[index], ObjectivesCount[index]);
        }

        /// <summary>
        /// Adds the explore.
        /// </summary>
        /// <param name="objCharacter">The Character.</param>
        public void AddExplore(WS_PlayerData.CharacterObject objCharacter)
        {
            Explored = true;
            IsCompleted();
            objCharacter.TalkUpdateQuest(Slot);
            WorldServiceLocator._WorldServer.ALLQUESTS.SendQuestMessageComplete(ref objCharacter.client, ID);
        }

        /// <summary>
        /// Adds the emote.
        /// </summary>
        /// <param name="objCharacter">The Character.</param>
        /// <param name="index">The index.</param>
        public void AddEmote(WS_PlayerData.CharacterObject objCharacter, byte index)
        {
            Progress[index] = (byte)(Progress[index] + 1);
            IsCompleted();
            objCharacter.TalkUpdateQuest(Slot);
            WorldServiceLocator._WorldServer.ALLQUESTS.SendQuestMessageComplete(ref objCharacter.client, ID);
        }

        /// <summary>
        /// Adds the item.
        /// </summary>
        /// <param name="objCharacter">The Character.</param>
        /// <param name="index">The index.</param>
        /// <param name="Count">The count.</param>
        public void AddItem(WS_PlayerData.CharacterObject objCharacter, byte index, byte Count)
        {
            if ((byte)(ProgressItem[index] + Count) > ObjectivesItemCount[index])
                Count = (byte)(ObjectivesItemCount[index] - ProgressItem[index]);
            ProgressItem[index] += Count;
            IsCompleted();
            objCharacter.TalkUpdateQuest(Slot);

            // TODO: When item quest event is fired as it should, remove -1 here.
            int ItemCount = Count - 1;
            WorldServiceLocator._WorldServer.ALLQUESTS.SendQuestMessageAddItem(ref objCharacter.client, ObjectivesItem[index], ItemCount);
        }

        /// <summary>
        /// Removes the item.
        /// </summary>
        /// <param name="objCharacter">The Character.</param>
        /// <param name="index">The index.</param>
        /// <param name="Count">The count.</param>
        public void RemoveItem(WS_PlayerData.CharacterObject objCharacter, byte index, byte Count)
        {
            if (ProgressItem[index] - Count < 0)
                Count = ProgressItem[index];
            ProgressItem[index] -= Count;
            IsCompleted();
            objCharacter.TalkUpdateQuest(Slot);
        }

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        private bool _disposedValue; // To detect redundant calls

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                // TODO: set large fields to null.
            }

            _disposedValue = true;
        }

        // This code added by Visual Basic to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
    }
}