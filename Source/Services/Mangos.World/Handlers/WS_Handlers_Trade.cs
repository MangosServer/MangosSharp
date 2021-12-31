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
using Mangos.Common.Enums.Item;
using Mangos.Common.Enums.Misc;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Network;
using Mangos.World.Objects;
using Mangos.World.Player;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;

namespace Mangos.World.Handlers;

public class WS_Handlers_Trade
{
    public class TTradeInfo : IDisposable
    {
        public int ID;

        public WS_PlayerData.CharacterObject Trader;

        public WS_PlayerData.CharacterObject Target;

        public int[] TraderSlots;

        public uint TraderGold;

        public bool TraderAccept;

        public int[] TargetSlots;

        public uint TargetGold;

        public bool TargetAccept;

        private bool _disposedValue;

        public TTradeInfo(ref WS_PlayerData.CharacterObject Trader_, ref WS_PlayerData.CharacterObject Target_)
        {
            ID = 0;
            Trader = null;
            Target = null;
            TraderSlots = new int[7]
            {
                    -1,
                    -1,
                    -1,
                    -1,
                    -1,
                    -1,
                    -1
            };
            TraderGold = 0u;
            TraderAccept = false;
            TargetSlots = new int[7]
            {
                    -1,
                    -1,
                    -1,
                    -1,
                    -1,
                    -1,
                    -1
            };
            TargetGold = 0u;
            TargetAccept = false;
            Trader = Trader_;
            Target = Target_;
            Trader.tradeInfo = this;
            Target.tradeInfo = this;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                Trader.tradeInfo = null;
                Target.tradeInfo = null;
                Trader = null;
                Target = null;
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

        public void SendTradeUpdateToTrader()
        {
            if (Trader == null)
            {
                return;
            }
            checked
            {
                Packets.PacketClass packet = new(Opcodes.SMSG_TRADE_STATUS_EXTENDED);
                try
                {
                    packet.AddInt8(1);
                    packet.AddInt32(7);
                    packet.AddInt32(7);
                    packet.AddUInt32(TargetGold);
                    packet.AddInt32(0);
                    var i = 0;
                    do
                    {
                        packet.AddInt8((byte)i);
                        switch (TargetSlots[i])
                        {
                            case > 0:
                                {
                                    var mySlot = (byte)(TargetSlots[i] & 0xFF);
                                    var myBag = (byte)(TargetSlots[i] >> 8);
                                    ItemObject myItem = null;
                                    myItem = (myBag != 0) ? Target.Items[myBag].Items[mySlot] : Target.Items[mySlot];
                                    packet.AddInt32(myItem.ItemEntry);
                                    packet.AddInt32(myItem.ItemInfo.Model);
                                    packet.AddInt32(myItem.StackCount);
                                    packet.AddInt32(0);
                                    packet.AddUInt64(myItem.GiftCreatorGUID);
                                    if (myItem.Enchantments.ContainsKey(0))
                                    {
                                        packet.AddInt32(myItem.Enchantments[0].ID);
                                    }
                                    else
                                    {
                                        packet.AddInt32(0);
                                    }
                                    packet.AddUInt64(myItem.CreatorGUID);
                                    packet.AddInt32(myItem.ChargesLeft);
                                    packet.AddInt32(0);
                                    packet.AddInt32(myItem.RandomProperties);
                                    packet.AddInt32(myItem.ItemInfo.Flags);
                                    packet.AddInt32(myItem.ItemInfo.Durability);
                                    packet.AddInt32(myItem.Durability);
                                    break;
                                }

                            default:
                                {
                                    var j = 0;
                                    do
                                    {
                                        packet.AddInt32(0);
                                        j++;
                                    }
                                    while (j <= 14);
                                    break;
                                }
                        }
                        i++;
                    }
                    while (i <= 6);
                    Trader.client.Send(ref packet);
                }
                finally
                {
                    packet.Dispose();
                }
            }
        }

        public void SendTradeUpdateToTarget()
        {
            if (Target == null)
            {
                return;
            }
            checked
            {
                Packets.PacketClass packet = new(Opcodes.SMSG_TRADE_STATUS_EXTENDED);
                try
                {
                    packet.AddInt8(1);
                    packet.AddInt32(7);
                    packet.AddInt32(7);
                    packet.AddUInt32(TraderGold);
                    packet.AddInt32(0);
                    var i = 0;
                    do
                    {
                        packet.AddInt8((byte)i);
                        if (TraderSlots[i] > 0)
                        {
                            var mySlot = (byte)(TraderSlots[i] & 0xFF);
                            var myBag = (byte)(TraderSlots[i] >> 8);
                            ItemObject myItem = null;
                            myItem = (myBag != 0) ? Trader.Items[myBag].Items[mySlot] : Trader.Items[mySlot];
                            packet.AddInt32(myItem.ItemEntry);
                            packet.AddInt32(myItem.ItemInfo.Model);
                            packet.AddInt32(myItem.StackCount);
                            packet.AddInt32(0);
                            packet.AddUInt64(myItem.GiftCreatorGUID);
                            if (myItem.Enchantments.ContainsKey(0))
                            {
                                packet.AddInt32(myItem.Enchantments[0].ID);
                            }
                            else
                            {
                                packet.AddInt32(0);
                            }
                            packet.AddUInt64(myItem.CreatorGUID);
                            packet.AddInt32(myItem.ChargesLeft);
                            packet.AddInt32(0);
                            packet.AddInt32(myItem.RandomProperties);
                            packet.AddInt32(myItem.ItemInfo.Flags);
                            packet.AddInt32(myItem.ItemInfo.Durability);
                            packet.AddInt32(myItem.Durability);
                        }
                        else
                        {
                            var j = 0;
                            do
                            {
                                packet.AddInt32(0);
                                j++;
                            }
                            while (j <= 14);
                        }
                        i++;
                    }
                    while (i <= 6);
                    Target.client.Send(ref packet);
                }
                finally
                {
                    packet.Dispose();
                }
            }
        }

        public void DoTrade(ref WS_PlayerData.CharacterObject Who)
        {
            Packets.PacketClass response = new(Opcodes.SMSG_TRADE_STATUS);
            try
            {

                response.AddInt32(4);
                if (Trader == Who)
                {
                    Target.client.SendMultiplyPackets(ref response);
                    TraderAccept = true;
                }
                else
                {
                    Trader.client.SendMultiplyPackets(ref response);
                    TargetAccept = true;
                }
            }
            finally
            {
                response.Dispose();
            }
            if (TargetAccept && TraderAccept)
            {
                DoTrade();
            }
        }

        private void DoTrade()
        {
            byte TargetReqItems = 0;
            byte TraderReqItems = 0;
            byte i = 0;
            do
            {
                checked
                {
                    if (TraderSlots[i] > 0)
                    {
                        TargetReqItems = (byte)(TargetReqItems + 1);
                    }
                    if (TargetSlots[i] > 0)
                    {
                        TraderReqItems = (byte)(TraderReqItems + 1);
                    }
                    i = (byte)unchecked((uint)(i + 1));
                }
            }
            while (i <= 5u);
            try
            {
                if (Target.ItemFREESLOTS() < TargetReqItems)
                {
                    Packets.PacketClass responseUnAccept2 = new(Opcodes.SMSG_TRADE_STATUS);
                    try
                    {
                        responseUnAccept2.AddInt32(7);
                        Target.client.SendMultiplyPackets(ref responseUnAccept2);
                        TraderAccept = false;
                        Trader.client.SendMultiplyPackets(ref responseUnAccept2);
                        TraderAccept = false;
                    }
                    finally
                    {
                        responseUnAccept2.Dispose();
                    }
                    Packets.PacketClass responseNoSlot2 = new(Opcodes.SMSG_INVENTORY_CHANGE_FAILURE);
                    try
                    {
                        responseNoSlot2.AddInt8(50);
                        responseNoSlot2.AddUInt64(0uL);
                        responseNoSlot2.AddUInt64(0uL);
                        responseNoSlot2.AddInt8(0);
                        Target.client.Send(ref responseNoSlot2);
                    }
                    finally
                    {
                        responseNoSlot2.Dispose();
                    }
                    return;
                }
                if (Trader.ItemFREESLOTS() < TraderReqItems)
                {
                    Packets.PacketClass responseUnAccept = new(Opcodes.SMSG_TRADE_STATUS);
                    try
                    {
                        responseUnAccept.AddInt32(7);
                        Target.client.SendMultiplyPackets(ref responseUnAccept);
                        TraderAccept = false;
                        Trader.client.SendMultiplyPackets(ref responseUnAccept);
                        TargetAccept = false;
                    }
                    finally
                    {
                        responseUnAccept.Dispose();
                    }
                    Packets.PacketClass responseNoSlot = new(Opcodes.SMSG_INVENTORY_CHANGE_FAILURE);
                    try
                    {
                        responseNoSlot.AddInt8(50);
                        responseNoSlot.AddUInt64(0uL);
                        responseNoSlot.AddUInt64(0uL);
                        responseNoSlot.AddInt8(0);
                        Trader.client.Send(ref responseNoSlot);
                    }
                    finally
                    {
                        responseNoSlot.Dispose();
                    }
                    return;
                }
                if ((TargetGold > 0L) || (TraderGold > 0L))
                {
                    checked
                    {
                        Trader.Copper = Trader.Copper - TraderGold + TargetGold;
                        Target.Copper = Target.Copper + TraderGold - TargetGold;
                        Trader.SetUpdateFlag(1176, Trader.Copper);
                        Target.SetUpdateFlag(1176, Target.Copper);
                    }
                }
                if (TargetReqItems > 0 || TraderReqItems > 0)
                {
                    byte j = 0;
                    do
                    {
                        checked
                        {
                            if (TraderSlots[j] > 0)
                            {
                                var mySlot2 = (byte)(TraderSlots[j] & 0xFF);
                                var myBag2 = (byte)(TraderSlots[j] >> 8);
                                ItemObject myItem2 = null;
                                myItem2 = (myBag2 != 0) ? Trader.Items[myBag2].Items[mySlot2] : Trader.Items[mySlot2];
                                if (myItem2.ItemInfo.ObjectClass != ITEM_CLASS.ITEM_CLASS_QUEST)
                                {
                                    myItem2.OwnerGUID = Target.GUID;
                                    if (Target.ItemADD(ref myItem2))
                                    {
                                        Trader.ItemREMOVE(myBag2, mySlot2, Destroy: false, Update: false);
                                    }
                                }
                            }
                            if (TargetSlots[j] > 0)
                            {
                                var mySlot = (byte)(TargetSlots[j] & 0xFF);
                                var myBag = (byte)(TargetSlots[j] >> 8);
                                ItemObject myItem = null;
                                myItem = (myBag != 0) ? Target.Items[myBag].Items[mySlot] : Target.Items[mySlot];
                                if (myItem.ItemInfo.ObjectClass != ITEM_CLASS.ITEM_CLASS_QUEST)
                                {
                                    myItem.OwnerGUID = Trader.GUID;
                                    if (Trader.ItemADD(ref myItem))
                                    {
                                        Target.ItemREMOVE(myBag, mySlot, Destroy: false, Update: false);
                                    }
                                }
                            }
                            j = (byte)unchecked((uint)(j + 1));
                        }
                    }
                    while (j <= 5u);
                }
                Trader.SendCharacterUpdate();
                Target.SendCharacterUpdate();
                Packets.PacketClass response = new(Opcodes.SMSG_TRADE_STATUS);
                try
                {
                    response.AddInt32(8);
                    Target.client.SendMultiplyPackets(ref response);
                    Trader.client.SendMultiplyPackets(ref response);
                }
                finally
                {
                    response.Dispose();
                    Dispose();
                }
            }
            catch (Exception e)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error doing trade: {0}{1}", Environment.NewLine, e.ToString());
            }
        }
    }

    public void On_CMSG_CANCEL_TRADE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        if (client == null || client.Character == null)
        {
            return;
        }
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CANCEL_TRADE", client.IP, client.Port);
        if (client.Character.tradeInfo == null)
        {
            return;
        }
        Packets.PacketClass response = new(Opcodes.SMSG_TRADE_STATUS);
        try
        {
            response.AddInt32(3);
            if (client.Character.tradeInfo.Target != null)
            {
                client.Character.tradeInfo.Target.client.SendMultiplyPackets(ref response);
            }
            if (client.Character.tradeInfo.Trader != null)
            {
                client.Character.tradeInfo.Trader.client.SendMultiplyPackets(ref response);
            }
        }
        finally
        {
            response.Dispose();
        }
        client.Character.tradeInfo.Dispose();
    }

    public void On_CMSG_SET_TRADE_GOLD(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        packet.GetInt16();
        var gold = packet.GetUInt32();
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SET_TRADE_GOLD [gold={2}]", client.IP, client.Port, gold);
        if (client.Character.tradeInfo != null)
        {
            if (client.Character.tradeInfo.Trader == client.Character)
            {
                client.Character.tradeInfo.TraderGold = gold;
                client.Character.tradeInfo.SendTradeUpdateToTarget();
            }
            else
            {
                client.Character.tradeInfo.TargetGold = gold;
                client.Character.tradeInfo.SendTradeUpdateToTrader();
            }
        }
    }

    public void On_CMSG_SET_TRADE_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        packet.GetInt16();
        var myBag = packet.GetInt8();
        if (myBag == byte.MaxValue)
        {
            myBag = 0;
        }
        var slot = packet.GetInt8();
        if (slot > 6)
        {
            return;
        }
        var mySlot = packet.GetInt8();
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SET_TRADE_ITEM [slot={2} myBag={3} mySlot={4}]", client.IP, client.Port, slot, myBag, mySlot);
        checked
        {
            if (client.Character.tradeInfo != null)
            {
                if (client.Character.tradeInfo.Trader == client.Character)
                {
                    client.Character.tradeInfo.TraderSlots[slot] = (myBag << 8) + mySlot;
                    client.Character.tradeInfo.SendTradeUpdateToTarget();
                }
                else
                {
                    client.Character.tradeInfo.TargetSlots[slot] = (myBag << 8) + mySlot;
                    client.Character.tradeInfo.SendTradeUpdateToTrader();
                }
            }
        }
    }

    public void On_CMSG_CLEAR_TRADE_ITEM(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        packet.GetInt16();
        var slot = packet.GetInt8();
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CLEAR_TRADE_ITEM [slot={2}]", client.IP, client.Port, slot);
        if (client.Character.tradeInfo.Trader == client.Character)
        {
            client.Character.tradeInfo.TraderSlots[slot] = -1;
            client.Character.tradeInfo.SendTradeUpdateToTarget();
        }
        else
        {
            client.Character.tradeInfo.TargetSlots[slot] = -1;
            client.Character.tradeInfo.SendTradeUpdateToTrader();
        }
    }

    public void On_CMSG_INITIATE_TRADE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        packet.GetInt16();
        var targetGUID = packet.GetUInt64();
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_INITIATE_TRADE [Trader={2} Target={3}]", client.IP, client.Port, client.Character.GUID, targetGUID);
        if (client.Character.DEAD)
        {
            Packets.PacketClass response6 = new(Opcodes.SMSG_TRADE_STATUS);
            try
            {
                response6.AddInt32(17);
                client.Send(ref response6);
            }
            finally
            {
                response6.Dispose();
            }
            return;
        }
        if (client.Character.LogoutTimer != null)
        {
            Packets.PacketClass response8 = new(Opcodes.SMSG_TRADE_STATUS);
            try
            {
                response8.AddInt32(19);
                client.Send(ref response8);
            }
            finally
            {
                response8.Dispose();
            }
            return;
        }
        if (((uint)client.Character.cUnitFlags & 0x40000u) != 0)
        {
            Packets.PacketClass response10 = new(Opcodes.SMSG_TRADE_STATUS);
            try
            {
                response10.AddInt32(15);
                client.Send(ref response10);
            }
            finally
            {
                response10.Dispose();
            }
            return;
        }
        if (!WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(targetGUID))
        {
            Packets.PacketClass response12 = new(Opcodes.SMSG_TRADE_STATUS);
            try
            {
                response12.AddInt32(6);
                client.Send(ref response12);
            }
            finally
            {
                response12.Dispose();
            }
            return;
        }
        if (WorldServiceLocator._WorldServer.CHARACTERs[targetGUID].DEAD)
        {
            Packets.PacketClass response13 = new(Opcodes.SMSG_TRADE_STATUS);
            try
            {
                response13.AddInt32(18);
                client.Send(ref response13);
            }
            finally
            {
                response13.Dispose();
            }
            return;
        }
        if (WorldServiceLocator._WorldServer.CHARACTERs[targetGUID].LogoutTimer != null)
        {
            Packets.PacketClass response11 = new(Opcodes.SMSG_TRADE_STATUS);
            try
            {
                response11.AddInt32(20);
                client.Send(ref response11);
            }
            finally
            {
                response11.Dispose();
            }
            return;
        }
        if (((uint)WorldServiceLocator._WorldServer.CHARACTERs[targetGUID].cUnitFlags & 0x40000u) != 0)
        {
            Packets.PacketClass response9 = new(Opcodes.SMSG_TRADE_STATUS);
            try
            {
                response9.AddInt32(15);
                client.Send(ref response9);
            }
            finally
            {
                response9.Dispose();
            }
            return;
        }
        if (client.Character.tradeInfo != null)
        {
            Packets.PacketClass response7 = new(Opcodes.SMSG_TRADE_STATUS);
            try
            {
                response7.AddInt32(0);
                client.Send(ref response7);
            }
            finally
            {
                response7.Dispose();
            }
            return;
        }
        if (WorldServiceLocator._WorldServer.CHARACTERs[targetGUID].tradeInfo != null)
        {
            Packets.PacketClass response5 = new(Opcodes.SMSG_TRADE_STATUS);
            try
            {
                response5.AddInt32(5);
                client.Send(ref response5);
            }
            finally
            {
                response5.Dispose();
            }
            return;
        }
        if (WorldServiceLocator._WorldServer.CHARACTERs[targetGUID].IsHorde != client.Character.IsHorde)
        {
            Packets.PacketClass response4 = new(Opcodes.SMSG_TRADE_STATUS);
            try
            {
                response4.AddInt32(11);
                client.Send(ref response4);
            }
            finally
            {
                response4.Dispose();
            }
            return;
        }
        if (WorldServiceLocator._WS_Combat.GetDistance(client.Character, WorldServiceLocator._WorldServer.CHARACTERs[targetGUID]) > 30f)
        {
            Packets.PacketClass response3 = new(Opcodes.SMSG_TRADE_STATUS);
            try
            {
                response3.AddInt32(10);
                client.Send(ref response3);
            }
            finally
            {
                response3.Dispose();
            }
            return;
        }
        if (client.Character.Access == AccessLevel.Trial)
        {
            Packets.PacketClass response2 = new(Opcodes.SMSG_TRADE_STATUS);
            try
            {
                response2.AddInt32(21);
                client.Send(ref response2);
            }
            finally
            {
                response2.Dispose();
            }
            return;
        }
        if (WorldServiceLocator._WorldServer.CHARACTERs[targetGUID].Access == AccessLevel.Trial)
        {
            Packets.PacketClass response = new(Opcodes.SMSG_TRADE_STATUS);
            try
            {
                response.AddInt32(21);
                client.Send(ref response);
            }
            finally
            {
                response.Dispose();
            }
            return;
        }
        ref var character = ref client.Character;
        ulong key;
        Dictionary<ulong, WS_PlayerData.CharacterObject> cHARACTERs;
        var Target_ = (cHARACTERs = WorldServiceLocator._WorldServer.CHARACTERs)[key = targetGUID];
        TTradeInfo tTradeInfo = new(ref character, ref Target_);
        cHARACTERs[key] = Target_;
        Packets.PacketClass response_ok = new(Opcodes.SMSG_TRADE_STATUS);
        try
        {
            response_ok.AddInt32(1);
            response_ok.AddUInt64(client.Character.GUID);
            client.Character.tradeInfo.Target.client.Send(ref response_ok);
        }
        finally
        {
            response_ok.Dispose();
        }
    }

    public void On_CMSG_BEGIN_TRADE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_BEGIN_TRADE", client.IP, client.Port);
        checked
        {
            client.Character.tradeInfo.ID++;
            Packets.PacketClass response = new(Opcodes.SMSG_TRADE_STATUS);
            try
            {
                response.AddInt32(2);
                response.AddInt32(client.Character.tradeInfo.ID);
                client.Character.tradeInfo.Trader.client.SendMultiplyPackets(ref response);
                client.Character.tradeInfo.Target.client.SendMultiplyPackets(ref response);
            }
            finally
            {
                response.Dispose();
            }
        }
    }

    public void On_CMSG_UNACCEPT_TRADE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_UNACCEPT_TRADE", client.IP, client.Port);
        Packets.PacketClass response = new(Opcodes.SMSG_TRADE_STATUS);
        try
        {
            response.AddInt32(7);
            if (client.Character.tradeInfo.Trader == client.Character)
            {
                client.Character.tradeInfo.Target.client.SendMultiplyPackets(ref response);
                client.Character.tradeInfo.TraderAccept = false;
            }
            else
            {
                client.Character.tradeInfo.Trader.client.SendMultiplyPackets(ref response);
                client.Character.tradeInfo.TargetAccept = false;
            }
        }
        finally
        {
            response.Dispose();
        }
    }

    public void On_CMSG_ACCEPT_TRADE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_ACCEPT_TRADE", client.IP, client.Port);
        client.Character.tradeInfo.DoTrade(ref client.Character);
    }

    public void On_CMSG_IGNORE_TRADE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_IGNORE_TRADE", client.IP, client.Port);
    }

    public void On_CMSG_BUSY_TRADE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_BUSY_TRADE", client.IP, client.Port);
    }
}
