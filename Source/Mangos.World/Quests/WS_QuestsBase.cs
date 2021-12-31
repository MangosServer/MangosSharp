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
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Objects;
using Mangos.World.Player;
using Microsoft.VisualBasic;
using System;

namespace Mangos.World.Quests;

public class WS_QuestsBase : IDisposable
{
    public int ID;

    public string Title;

    public int SpecialFlags;

    public int ObjectiveFlags;

    public byte Slot;

    public byte[] ObjectivesType;

    public int ObjectivesDeliver;

    public int[] ObjectivesExplore;

    public int[] ObjectivesSpell;

    public int[] ObjectivesItem;

    public byte[] ObjectivesItemCount;

    public int[] ObjectivesObject;

    public byte[] ObjectivesCount;

    public bool Explored;

    public byte[] Progress;

    public byte[] ProgressItem;

    public bool Complete;

    public bool Failed;

    public int TimeEnd;

    private bool _disposedValue;

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
        ObjectivesType = new byte[4];
        ObjectivesItemCount = new byte[4];
        ObjectivesCount = new byte[4];
        ObjectivesObject = new int[4];
        ObjectivesExplore = new int[4];
        ObjectivesSpell = new int[4];
        ObjectivesItem = new int[4];
        Explored = true;
        Progress = new byte[4];
        ProgressItem = new byte[4];
        Complete = false;
        Failed = false;
        TimeEnd = 0;
        byte bytLoop5 = 0;
        do
        {
            checked
            {
                if (Quest.ObjectivesCastSpell[bytLoop5] > 0)
                {
                    ObjectiveFlags |= 16;
                    ObjectivesType[bytLoop5] = 16;
                    ObjectivesSpell[bytLoop5] = Quest.ObjectivesCastSpell[bytLoop5];
                    ObjectivesObject[0] = Quest.ObjectivesKill[bytLoop5];
                    ObjectivesCount[0] = (byte)Quest.ObjectivesKill_Count[bytLoop5];
                }
                bytLoop5 = (byte)unchecked((uint)(bytLoop5 + 1));
            }
        }
        while (bytLoop5 <= 3u);
        byte bytLoop4 = 0;
        do
        {
            checked
            {
                if (Quest.ObjectivesKill[bytLoop4] > 0)
                {
                    byte bytLoop6 = 0;
                    do
                    {
                        if (ObjectivesType[bytLoop6] == 0)
                        {
                            ObjectiveFlags |= 1;
                            ObjectivesType[bytLoop6] = 1;
                            ObjectivesObject[bytLoop6] = Quest.ObjectivesKill[bytLoop4];
                            ObjectivesCount[bytLoop6] = (byte)Quest.ObjectivesKill_Count[bytLoop4];
                            break;
                        }
                        bytLoop6 = (byte)unchecked((uint)(bytLoop6 + 1));
                    }
                    while (bytLoop6 <= 3u);
                }
                bytLoop4 = (byte)unchecked((uint)(bytLoop4 + 1));
            }
        }
        while (bytLoop4 <= 3u);
        byte bytLoop3 = 0;
        do
        {
            checked
            {
                if (Quest.ObjectivesItem[bytLoop3] > 0)
                {
                    ObjectiveFlags |= 32;
                    ObjectivesType[bytLoop3] = 32;
                    ObjectivesItem[bytLoop3] = Quest.ObjectivesItem[bytLoop3];
                    ObjectivesItemCount[bytLoop3] = (byte)Quest.ObjectivesItem_Count[bytLoop3];
                }
                bytLoop3 = (byte)unchecked((uint)(bytLoop3 + 1));
            }
        }
        while (bytLoop3 <= 3u);
        if (((uint)Quest.SpecialFlags & 2u) != 0)
        {
            ObjectiveFlags |= 2;
            byte bytLoop2 = 0;
            do
            {
                ObjectivesType[bytLoop2] = 2;
                ObjectivesExplore[bytLoop2] = Quest.ObjectivesTrigger[bytLoop2];
                checked
                {
                    bytLoop2 = (byte)unchecked((uint)(bytLoop2 + 1));
                }
            }
            while (bytLoop2 <= 3u);
        }
        if (((uint)Quest.SpecialFlags & 8u) != 0)
        {
            ObjectiveFlags |= 8;
            var i = 0;
            do
            {
                if (ObjectivesType[i] == 0)
                {
                    ObjectivesType[i] = 8;
                    ObjectivesCount[i] = 1;
                }
                i = checked(i + 1);
            }
            while (i <= 3);
        }
        checked
        {
            if (ObjectiveFlags == 0)
            {
                byte bytLoop = 0;
                do
                {
                    ObjectivesObject[bytLoop] = 0;
                    ObjectivesCount[bytLoop] = 0;
                    ObjectivesExplore[bytLoop] = 0;
                    ObjectivesSpell[bytLoop] = 0;
                    ObjectivesType[bytLoop] = 0;
                    bytLoop = (byte)unchecked((uint)(bytLoop + 1));
                }
                while (bytLoop <= 3u);
            }
            Title = Quest.Title;
            ID = Quest.ID;
            SpecialFlags = Quest.SpecialFlags;
            ObjectivesDeliver = Quest.ObjectivesDeliver;
            if (Quest.TimeLimit > 0)
            {
                TimeEnd = (int)(WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now) + Quest.TimeLimit);
            }
        }
    }

    public void UpdateItemCount(ref WS_PlayerData.CharacterObject objCharacter)
    {
        byte i = 0;
        do
        {
            checked
            {
                if (ObjectivesItem[i] != 0)
                {
                    ProgressItem[i] = (byte)objCharacter.ItemCOUNT(ObjectivesItem[i]);
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "ITEM COUNT UPDATED TO: {0}", ProgressItem[i]);
                }
                i = (byte)unchecked((uint)(i + 1));
            }
        }
        while (i <= 3u);
        if ((ObjectiveFlags & 2) == 0)
        {
            Explored = true;
        }
        IsCompleted();
    }

    public void Initialize(ref WS_PlayerData.CharacterObject objCharacter)
    {
        if (ObjectivesDeliver > 0)
        {
            ItemObject tmpItem = new(ObjectivesDeliver, objCharacter.GUID);
            if (!objCharacter.ItemADD(ref tmpItem))
            {
                tmpItem.Delete();
                Packets.PacketClass response = new(Opcodes.SMSG_QUESTGIVER_QUEST_FAILED);
                response.AddInt32(ID);
                response.AddInt32(4);
                objCharacter.client.Send(ref response);
                response.Dispose();
                return;
            }
            objCharacter.LogLootItem(tmpItem, 1, Recieved: true, Created: false);
        }
        byte i = 0;
        do
        {
            checked
            {
                if (ObjectivesItem[i] != 0)
                {
                    ProgressItem[i] = (byte)objCharacter.ItemCOUNT(ObjectivesItem[i]);
                }
                i = (byte)unchecked((uint)(i + 1));
            }
        }
        while (i <= 3u);
        if (((uint)ObjectiveFlags & 2u) != 0)
        {
            Explored = false;
        }
        IsCompleted();
    }

    public virtual bool IsCompleted()
    {
        Complete = ObjectivesCount[0] <= (uint)Progress[0] && ObjectivesCount[1] <= (uint)Progress[1] && ObjectivesCount[2] <= (uint)Progress[2] && ObjectivesCount[3] <= (uint)Progress[3] && ObjectivesItemCount[0] <= (uint)ProgressItem[0] && ObjectivesItemCount[1] <= (uint)ProgressItem[1] && ObjectivesItemCount[2] <= (uint)ProgressItem[2] && ObjectivesItemCount[3] <= (uint)ProgressItem[3] && Explored && !Failed;
        return Complete;
    }

    public virtual int GetState(bool ForSave = false)
    {
        int tmpState = default;
        if (Complete)
        {
            tmpState = 1;
        }
        if (Failed)
        {
            tmpState = 2;
        }
        return tmpState;
    }

    public virtual int GetProgress(bool ForSave = false)
    {
        var tmpProgress = 0;
        checked
        {
            if (ForSave)
            {
                tmpProgress += Progress[0];
                tmpProgress += Progress[1] << 6;
                tmpProgress += Progress[2] << 12;
                tmpProgress += Progress[3] << 18;
                if (Explored)
                {
                    tmpProgress += 16777216;
                }
                if (Complete)
                {
                    tmpProgress += 33554432;
                }
                if (Failed)
                {
                    tmpProgress += 67108864;
                }
            }
            else
            {
                tmpProgress += Progress[0];
                tmpProgress += Progress[1] << 6;
                tmpProgress += Progress[2] << 12;
                tmpProgress += Progress[3] << 18;
                if (Complete)
                {
                    tmpProgress += 16777216;
                }
                if (Failed)
                {
                    tmpProgress += 33554432;
                }
            }
            return tmpProgress;
        }
    }

    public virtual void LoadState(int state)
    {
        checked
        {
            Progress[0] = (byte)(state & 0x3F);
            Progress[1] = (byte)((state >> 6) & 0x3F);
            Progress[2] = (byte)((state >> 12) & 0x3F);
            Progress[3] = (byte)((state >> 18) & 0x3F);
            Explored = ((state >> 24) & 1) == 1;
            Complete = ((state >> 25) & 1) == 1;
            Failed = ((state >> 26) & 1) == 1;
        }
    }

    public void AddKill(WS_PlayerData.CharacterObject objCharacter, byte index, ulong oGUID)
    {
        checked
        {
            Progress[index]++;
            IsCompleted();
            objCharacter.TalkUpdateQuest(Slot);
            WorldServiceLocator._WorldServer.ALLQUESTS.SendQuestMessageAddKill(ref objCharacter.client, ID, oGUID, ObjectivesObject[index], Progress[index], ObjectivesCount[index]);
        }
    }

    public void AddCast(WS_PlayerData.CharacterObject objCharacter, byte index, ulong oGUID)
    {
        checked
        {
            Progress[index]++;
            IsCompleted();
            objCharacter.TalkUpdateQuest(Slot);
            WorldServiceLocator._WorldServer.ALLQUESTS.SendQuestMessageAddKill(ref objCharacter.client, ID, oGUID, ObjectivesObject[index], Progress[index], ObjectivesCount[index]);
        }
    }

    public void AddExplore(WS_PlayerData.CharacterObject objCharacter)
    {
        Explored = true;
        IsCompleted();
        objCharacter.TalkUpdateQuest(Slot);
        WorldServiceLocator._WorldServer.ALLQUESTS.SendQuestMessageComplete(ref objCharacter.client, ID);
    }

    public void AddEmote(WS_PlayerData.CharacterObject objCharacter, byte index)
    {
        checked
        {
            Progress[index]++;
            IsCompleted();
            objCharacter.TalkUpdateQuest(Slot);
            WorldServiceLocator._WorldServer.ALLQUESTS.SendQuestMessageComplete(ref objCharacter.client, ID);
        }
    }

    public void AddItem(WS_PlayerData.CharacterObject objCharacter, byte index, byte Count)
    {
        if (checked((byte)unchecked((uint)(ProgressItem[index] + Count))) > (uint)ObjectivesItemCount[index])
        {
            checked
            {
                Count = (byte)unchecked((uint)(ObjectivesItemCount[index] - ProgressItem[index]));
            }
        }
        ref var reference = ref ProgressItem[index];
        checked
        {
            reference = (byte)unchecked((uint)(reference + Count));
            IsCompleted();
            objCharacter.TalkUpdateQuest(Slot);
            var ItemCount = Count - 1;
            WorldServiceLocator._WorldServer.ALLQUESTS.SendQuestMessageAddItem(ref objCharacter.client, ObjectivesItem[index], ItemCount);
        }
    }

    public void RemoveItem(WS_PlayerData.CharacterObject objCharacter, byte index, byte Count)
    {
        checked
        {
            if (ProgressItem[index] - Count < 0)
            {
                Count = ProgressItem[index];
            }
            ref var reference = ref ProgressItem[index];
            reference = (byte)unchecked((uint)(reference - Count));
            IsCompleted();
            objCharacter.TalkUpdateQuest(Slot);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
        }
        _disposedValue = true;
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
}
