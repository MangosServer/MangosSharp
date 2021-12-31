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

using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Objects;
using Mangos.World.Player;
using System.Collections.Generic;
using System.Threading;

namespace Mangos.World.Loots;

public partial class WS_Loot
{
    public class GroupLootInfo
    {
        public LootObject LootObject;

        public byte LootSlot;

        public LootItem Item;

        public List<WS_PlayerData.CharacterObject> Rolls;

        public Dictionary<WS_PlayerData.CharacterObject, int> Looters;

        public Timer RollTimeoutTimer;

        public GroupLootInfo()
        {
            Rolls = new List<WS_PlayerData.CharacterObject>();
            Looters = new Dictionary<WS_PlayerData.CharacterObject, int>(5);
            RollTimeoutTimer = null;
        }

        public void Check()
        {
            if (Looters.Count != Rolls.Count)
            {
                return;
            }
            byte maxRollType = 0;
            foreach (var looter2 in Looters)
            {
                if (looter2.Value == 1)
                {
                    maxRollType = 1;
                }
                if (looter2.Value == 2 && maxRollType != 1)
                {
                    maxRollType = 2;
                }
            }
            if (maxRollType == 0)
            {
                LootObject.GroupLootInfo.Remove(LootSlot);
                Packets.PacketClass response2 = new(Opcodes.SMSG_LOOT_ALL_PASSED);
                response2.AddUInt64(LootObject.GUID);
                response2.AddInt32(LootSlot);
                response2.AddInt32(Item.ItemID);
                response2.AddInt32(0);
                response2.AddInt32(0);
                Broadcast(ref response2);
                response2.Dispose();
                return;
            }
            WS_PlayerData.CharacterObject looterCharacter = null;
            checked
            {
                var maxRoll = -1;
                foreach (var looter in Looters)
                {
                    if (looter.Value == maxRollType)
                    {
                        var rollValue = (byte)WorldServiceLocator._WorldServer.Rnd.Next(0, 100);
                        if (rollValue > maxRoll)
                        {
                            maxRoll = rollValue;
                            looterCharacter = looter.Key;
                        }
                        Packets.PacketClass response = new(Opcodes.SMSG_LOOT_ROLL);
                        response.AddUInt64(LootObject.GUID);
                        response.AddInt32(LootSlot);
                        response.AddUInt64(looter.Key.GUID);
                        response.AddInt32(Item.ItemID);
                        response.AddInt32(0);
                        response.AddInt32(0);
                        response.AddInt8(rollValue);
                        response.AddInt8((byte)looter.Value);
                        Broadcast(ref response);
                        response.Dispose();
                    }
                }
                ItemObject itemObject = new(Item.ItemID, looterCharacter.GUID)
                {
                    StackCount = Item.ItemCount
                };
                var tmpItem = itemObject;
                Packets.PacketClass wonItem = new(Opcodes.SMSG_LOOT_ROLL_WON);
                wonItem.AddUInt64(LootObject.GUID);
                wonItem.AddInt32(LootSlot);
                wonItem.AddInt32(Item.ItemID);
                wonItem.AddInt32(0);
                wonItem.AddInt32(0);
                wonItem.AddUInt64(looterCharacter.GUID);
                wonItem.AddInt8((byte)maxRoll);
                wonItem.AddInt8(maxRollType);
                Broadcast(ref wonItem);
                if (looterCharacter.ItemADD(ref tmpItem))
                {
                    looterCharacter.LogLootItem(tmpItem, Item.ItemCount, Recieved: false, Created: false);
                    LootObject.GroupLootInfo.Remove(LootSlot);
                    LootObject.Items[LootSlot] = null;
                }
                else
                {
                    tmpItem.Delete();
                    LootObject.GroupLootInfo.Remove(LootSlot);
                }
            }
        }

        public void Broadcast(ref Packets.PacketClass packet)
        {
            foreach (var objCharacter in Rolls)
            {
                objCharacter.client.SendMultiplyPackets(ref packet);
            }
        }

        public void EndRoll(object state)
        {
            foreach (var objCharacter in Rolls)
            {
                if (!Looters.ContainsKey(objCharacter))
                {
                    Looters[objCharacter] = 0;
                    Packets.PacketClass response = new(Opcodes.SMSG_LOOT_ROLL);
                    response.AddUInt64(LootObject.GUID);
                    response.AddInt32(LootSlot);
                    response.AddUInt64(objCharacter.GUID);
                    response.AddInt32(Item.ItemID);
                    response.AddInt32(0);
                    response.AddInt32(0);
                    response.AddInt8(249);
                    response.AddInt8(0);
                    Broadcast(ref response);
                }
            }
            RollTimeoutTimer.Dispose();
            RollTimeoutTimer = null;
            Check();
        }
    }
}
