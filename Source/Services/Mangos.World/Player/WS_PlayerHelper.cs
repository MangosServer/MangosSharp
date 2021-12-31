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
using Mangos.Common.Enums.Spell;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Network;
using Mangos.World.Objects;
using Mangos.World.Spells;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Mangos.World.Player;

public class WS_PlayerHelper
{
    public class TSkill
    {
        private short _Current;

        public short Bonus;

        public short Base;

        public int Maximum => Base;

        public int MaximumWithBonus
        {
            get
            {
                checked
                {
                    return (short)unchecked(Base + Bonus);
                }
            }
        }

        public short Current
        {
            get => _Current;
            set
            {
                if (value <= Maximum)
                {
                    _Current = value;
                }
            }
        }

        public short CurrentWithBonus
        {
            get
            {
                checked
                {
                    return (short)unchecked(_Current + Bonus);
                }
            }
        }

        public int GetSkill
        {
            get
            {
                checked
                {
                    return _Current + ((short)unchecked(Base + Bonus) << 16);
                }
            }
        }

        public TSkill(short CurrentVal, short MaximumVal = 375)
        {
            _Current = 0;
            Bonus = 0;
            Base = 300;
            Current = CurrentVal;
            Base = MaximumVal;
        }

        public void Increment(short Incrementator = 1)
        {
            checked
            {
                Current = (short)unchecked(Current + Incrementator) < Base ? (short)unchecked(Current + Incrementator) : Base;
            }
        }
    }

    public class TStatBar
    {
        private int _Current;

        public int Bonus;

        public int Base;

        public float Modifier;

        public int Maximum => checked((int)Math.Round((Bonus + Base) * Modifier));

        public int Current
        {
            get => checked((int)Math.Round(_Current * Modifier));
            set
            {
                _Current = value <= Maximum ? value : Maximum;
                if (_Current < 0)
                {
                    _Current = 0;
                }
            }
        }

        public void Increment(int Incrementator = 1)
        {
            checked
            {
                if (Current + Incrementator < Bonus + Base)
                {
                    Current += Incrementator;
                }
                else
                {
                    Current = Maximum;
                }
            }
        }

        public TStatBar(int CurrentVal, int BaseVal, int BonusVal)
        {
            _Current = 0;
            Bonus = 0;
            Base = 0;
            Modifier = 1f;
            _Current = CurrentVal;
            Bonus = BonusVal;
            Base = BaseVal;
        }
    }

    public class TStat
    {
        public int Base;

        public short PositiveBonus;

        public short NegativeBonus;

        public float BaseModifier;

        public float Modifier;

        public int RealBase
        {
            get => checked(Base - PositiveBonus + NegativeBonus);
            set
            {
                checked
                {
                    Base = Base - PositiveBonus + NegativeBonus;
                    Base = value;
                    Base = Base + PositiveBonus - NegativeBonus;
                }
            }
        }

        public TStat(byte BaseValue = 0, byte PosValue = 0, byte NegValue = 0)
        {
            Base = 0;
            PositiveBonus = 0;
            NegativeBonus = 0;
            BaseModifier = 1f;
            Modifier = 1f;
            Base = BaseValue;
            PositiveBonus = PosValue;
            PositiveBonus = NegValue;
        }
    }

    public class TDamageBonus
    {
        public int PositiveBonus;

        public int NegativeBonus;

        public float Modifier;

        public int Value => checked((int)Math.Round((PositiveBonus - NegativeBonus) * Modifier));

        public TDamageBonus(byte PosValue = 0, byte NegValue = 0)
        {
            PositiveBonus = 0;
            NegativeBonus = 0;
            Modifier = 1f;
            PositiveBonus = PosValue;
            PositiveBonus = NegValue;
        }
    }

    public class THonor
    {
        public ulong CharGUID;

        public short HonorPoints;

        public byte HonorRank;

        public byte HonorHightestRank;

        public int Standing;

        public int HonorLastWeek;

        public int HonorThisWeek;

        public int HonorYesterday;

        public int KillsLastWeek;

        public int KillsThisWeek;

        public int KillsYesterday;

        public int KillsHonorableToday;

        public int KillsDisHonorableToday;

        public int KillsHonorableLifetime;

        public int KillsDisHonorableLifetime;

        public THonor()
        {
            CharGUID = 0uL;
            HonorPoints = 0;
            HonorRank = 0;
            HonorHightestRank = 0;
            Standing = 0;
            HonorLastWeek = 0;
            HonorThisWeek = 0;
            HonorYesterday = 0;
            KillsLastWeek = 0;
            KillsThisWeek = 0;
            KillsYesterday = 0;
            KillsHonorableToday = 0;
            KillsDisHonorableToday = 0;
            KillsHonorableLifetime = 0;
            KillsDisHonorableLifetime = 0;
        }

        public void Save()
        {
            var tmp = "UPDATE characters_honor SET";
            tmp = tmp + " honor_points =\"" + Conversions.ToString((int)HonorPoints) + "\"";
            tmp = tmp + ", honor_rank =" + Conversions.ToString(HonorRank);
            tmp = tmp + ", honor_hightestRank =" + Conversions.ToString(HonorHightestRank);
            tmp = tmp + ", honor_standing =" + Conversions.ToString(Standing);
            tmp = tmp + ", honor_lastWeek =" + Conversions.ToString(HonorLastWeek);
            tmp = tmp + ", honor_thisWeek =" + Conversions.ToString(HonorThisWeek);
            tmp = tmp + ", honor_yesterday =" + Conversions.ToString(HonorYesterday);
            tmp = tmp + ", kills_lastWeek =" + Conversions.ToString(KillsLastWeek);
            tmp = tmp + ", kills_thisWeek =" + Conversions.ToString(KillsThisWeek);
            tmp = tmp + ", kills_yesterday =" + Conversions.ToString(KillsYesterday);
            tmp = tmp + ", kills_dishonorableToday =" + Conversions.ToString(KillsDisHonorableToday);
            tmp = tmp + ", kills_honorableToday =" + Conversions.ToString(KillsHonorableToday);
            tmp = tmp + ", kills_dishonorableLifetime =" + Conversions.ToString(KillsDisHonorableLifetime);
            tmp = tmp + ", kills_honorableLifetime =" + Conversions.ToString(KillsHonorableLifetime);
            tmp += $" WHERE char_guid = \"{CharGUID}\";";
            WorldServiceLocator._WorldServer.CharacterDatabase.Update(tmp);
        }

        public void Load(ulong GUID)
        {
        }

        public void SaveAsNew(ulong GUID)
        {
        }
    }

    public class TReputation
    {
        public int Flags;

        public int Value;

        public TReputation()
        {
            Flags = 0;
            Value = 0;
        }
    }

    public class TActionButton
    {
        public byte ActionType;

        public byte ActionMisc;

        public int Action;

        public TActionButton(int Action_, byte Type_, byte Misc_)
        {
            ActionType = 0;
            ActionMisc = 0;
            Action = 0;
            ActionType = Type_;
            ActionMisc = Misc_;
            Action = Action_;
        }
    }

    public class TDrowningTimer : IDisposable
    {
        private Timer DrowningTimer;

        public int DrowningValue;

        public byte DrowningDamage;

        public ulong CharacterGUID;

        private bool _disposedValue;

        public TDrowningTimer(ref WS_PlayerData.CharacterObject Character)
        {
            DrowningTimer = null;
            DrowningValue = 70000;
            DrowningDamage = 1;
            CharacterGUID = 0uL;
            CharacterGUID = Character.GUID;
            Character.StartMirrorTimer(MirrorTimer.DROWNING, 70000);
            DrowningTimer = new Timer(Character.HandleDrowning, null, 2000, 1000);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (!Information.IsNothing(DrowningTimer))
                {
                    DrowningTimer.Dispose();
                    DrowningTimer = null;
                }
                if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(CharacterGUID))
                {
                    WorldServiceLocator._WorldServer.CHARACTERs[CharacterGUID].StopMirrorTimer(MirrorTimer.DROWNING);
                }
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

    public class TRepopTimer : IDisposable
    {
        private Timer RepopTimer;

        public WS_PlayerData.CharacterObject Character;

        private bool _disposedValue;

        public TRepopTimer(ref WS_PlayerData.CharacterObject Character)
        {
            RepopTimer = null;
            this.Character = null;
            this.Character = Character;
            RepopTimer = new Timer(Repop, null, 360000, 360000);
        }

        public void Repop(object Obj)
        {
            WorldServiceLocator._WS_Handlers_Misc.CharacterRepop(ref Character.client);
            Character.repopTimer = null;
            Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                RepopTimer.Dispose();
                RepopTimer = null;
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

    public void SendBindPointUpdate(ref WS_Network.ClientClass client, ref WS_PlayerData.CharacterObject Character)
    {
        Packets.PacketClass SMSG_BINDPOINTUPDATE = new(Opcodes.SMSG_BINDPOINTUPDATE);
        try
        {
            SMSG_BINDPOINTUPDATE.AddSingle(Character.bindpoint_positionX);
            SMSG_BINDPOINTUPDATE.AddSingle(Character.bindpoint_positionY);
            SMSG_BINDPOINTUPDATE.AddSingle(Character.bindpoint_positionZ);
            SMSG_BINDPOINTUPDATE.AddInt32(Character.bindpoint_map_id);
            SMSG_BINDPOINTUPDATE.AddInt32(Character.bindpoint_zone_id);
            client.Send(ref SMSG_BINDPOINTUPDATE);
        }
        finally
        {
            SMSG_BINDPOINTUPDATE.Dispose();
        }
    }

    public void Send_SMSG_SET_REST_START(ref WS_Network.ClientClass client, ref WS_PlayerData.CharacterObject Character)
    {
        Packets.PacketClass SMSG_SET_REST_START = new(Opcodes.SMSG_SET_REST_START);
        try
        {
            SMSG_SET_REST_START.AddInt32(WorldServiceLocator._WS_Network.MsTime());
            client.Send(ref SMSG_SET_REST_START);
        }
        finally
        {
            SMSG_SET_REST_START.Dispose();
        }
    }

    public void SendTutorialFlags(ref WS_Network.ClientClass client, ref WS_PlayerData.CharacterObject Character)
    {
        Packets.PacketClass SMSG_TUTORIAL_FLAGS = new(Opcodes.SMSG_TUTORIAL_FLAGS);
        try
        {
            SMSG_TUTORIAL_FLAGS.AddByteArray(Character.TutorialFlags);
            client.Send(ref SMSG_TUTORIAL_FLAGS);
        }
        finally
        {
            SMSG_TUTORIAL_FLAGS.Dispose();
        }
    }

    public void SendFactions(ref WS_Network.ClientClass client, ref WS_PlayerData.CharacterObject Character)
    {
        Packets.PacketClass packet = new(Opcodes.SMSG_INITIALIZE_FACTIONS);
        try
        {
            packet.AddInt32(64);
            byte i = 0;
            do
            {
                checked
                {
                    if (Character.Reputation != null)
                    {
                        packet.AddInt8((byte)Character.Reputation[i].Flags);
                    }
                    packet.AddInt32(Character.Reputation[i].Value);
                    i = (byte)unchecked((uint)(i + 1));
                }
            }
            while (i <= 63u);
            client.Send(ref packet);
        }
        finally
        {
            packet.Dispose();
        }
    }

    public void SendActionButtons(ref WS_Network.ClientClass client, ref WS_PlayerData.CharacterObject Character)
    {
        Packets.PacketClass packet = new(Opcodes.SMSG_ACTION_BUTTONS);
        try
        {
            byte i = 0;
            do
            {
                checked
                {
                    if (Character.ActionButtons.ContainsKey(i))
                    {
                        packet.AddUInt16((ushort)Character.ActionButtons[i].Action);
                        packet.AddInt8(Character.ActionButtons[i].ActionType);
                        packet.AddInt8(Character.ActionButtons[i].ActionMisc);
                    }
                    else
                    {
                        packet.AddInt32(0);
                    }
                    i = (byte)unchecked((uint)(i + 1));
                }
            }
            while (i <= 119u);
            client.Send(ref packet);
        }
        finally
        {
            packet.Dispose();
        }
    }

    public void SendInitWorldStates(ref WS_Network.ClientClass client, ref WS_PlayerData.CharacterObject Character)
    {
        Character.ZoneCheck();
        var NumberOfFields = Character.ZoneID switch
        {
            0 or 1 or 4 or 8 or 10 or 11 or 12 or 36 or 38 or 40 or 41 or 51 or 267 or 1519 or 1537 or 2257 or 2918 => 6,
            2597 => 81,
            3277 => 14,
            3358 or 3820 => 38,
            3483 => 22,
            3519 => 36,
            3521 => 35,
            3698 or 3702 or 3968 => 9,
            3703 => 9,
            _ => 10,
        };
        Packets.PacketClass packet = new(Opcodes.SMSG_INIT_WORLD_STATES);
        try
        {
            packet.AddUInt32(Character.MapID);
            packet.AddInt32(Character.ZoneID);
            packet.AddInt32(Character.AreaID);
            packet.AddUInt16((ushort)NumberOfFields);
            packet.AddUInt32(2264u);
            packet.AddUInt32(0u);
            packet.AddUInt32(2263u);
            packet.AddUInt32(0u);
            packet.AddUInt32(2262u);
            packet.AddUInt32(0u);
            packet.AddUInt32(2261u);
            packet.AddUInt32(0u);
            packet.AddUInt32(2260u);
            packet.AddUInt32(0u);
            packet.AddUInt32(2259u);
            packet.AddUInt32(0u);
            switch (Character.ZoneID)
            {
                case 3968:
                    packet.AddUInt32(3000u);
                    packet.AddUInt32(0u);
                    packet.AddUInt32(3001u);
                    packet.AddUInt32(0u);
                    packet.AddUInt32(3002u);
                    packet.AddUInt32(0u);
                    break;

                default:
                    packet.AddUInt32(2324u);
                    packet.AddUInt32(0u);
                    packet.AddUInt32(2323u);
                    packet.AddUInt32(0u);
                    packet.AddUInt32(2322u);
                    packet.AddUInt32(0u);
                    packet.AddUInt32(2325u);
                    packet.AddUInt32(0u);
                    break;

                case 1:
                case 11:
                case 12:
                case 38:
                case 40:
                case 51:
                case 1519:
                case 1537:
                case 2257:
                case 2597:
                case 3277:
                case 3358:
                case 3820:
                    break;
            }
            client.Send(ref packet);
        }
        finally
        {
            packet.Dispose();
        }
    }

    public void SendInitialSpells(ref WS_Network.ClientClass client, ref WS_PlayerData.CharacterObject Character)
    {
        Packets.PacketClass packet = new(Opcodes.SMSG_INITIAL_SPELLS);
        checked
        {
            try
            {
                packet.AddInt8(0);
                var countPos = packet.Data.Length;
                packet.AddInt16(0);
                var spellCount = 0;
                Dictionary<int, KeyValuePair<uint, int>> spellCooldowns = new();
                foreach (var Spell in Character.Spells)
                {
                    if (Spell.Value.Active == 1)
                    {
                        packet.AddUInt16((ushort)Spell.Key);
                        packet.AddInt16(0);
                        spellCount++;
                        if (Spell.Value.Cooldown != 0)
                        {
                            spellCooldowns.Add(Spell.Key, new KeyValuePair<uint, int>(Spell.Value.Cooldown, 0));
                        }
                    }
                }
                packet.AddInt16((short)spellCount, countPos);
                spellCount = 0;
                countPos = packet.Data.Length;
                packet.AddInt16(0);
                foreach (var Cooldown in spellCooldowns)
                {
                    if (WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(Cooldown.Key))
                    {
                        packet.AddUInt16((ushort)Cooldown.Key);
                        var timeLeft = 0;
                        if (Cooldown.Value.Key > WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now))
                        {
                            timeLeft = (int)(checked(Cooldown.Value.Key - WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now)) * 1000L);
                        }
                        packet.AddUInt16((ushort)Cooldown.Value.Value);
                        packet.AddUInt16((ushort)WorldServiceLocator._WS_Spells.SPELLs[Cooldown.Key].Category);
                        if (WorldServiceLocator._WS_Spells.SPELLs[Cooldown.Key].CategoryCooldown > 0)
                        {
                            packet.AddInt32(0);
                            packet.AddInt32(timeLeft);
                        }
                        else
                        {
                            packet.AddInt32(timeLeft);
                            packet.AddInt32(0);
                        }
                        spellCount++;
                    }
                }
                packet.AddInt16((short)spellCount, countPos);
                client.Send(ref packet);
            }
            finally
            {
                packet.Dispose();
            }
        }
    }

    public void InitializeTalentSpells(WS_PlayerData.CharacterObject objCharacter)
    {
        WS_Spells.SpellTargets t = new();
        WS_Base.BaseUnit objCharacter2 = objCharacter;
        t.SetTarget_SELF(ref objCharacter2);
        objCharacter = (WS_PlayerData.CharacterObject)objCharacter2;
        foreach (var Spell in objCharacter.Spells)
        {
            if (WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(Spell.Key) && WorldServiceLocator._WS_Spells.SPELLs[Spell.Key].IsPassive)
            {
                if (!objCharacter.HavePassiveAura(Spell.Key) && WorldServiceLocator._WS_Spells.SPELLs[Spell.Key].CanCast(ref objCharacter, t, FirstCheck: false) == SpellFailedReason.SPELL_NO_ERROR)
                {
                    var spellInfo = WorldServiceLocator._WS_Spells.SPELLs[Spell.Key];
                    WS_Base.BaseObject caster = objCharacter;
                    spellInfo.Apply(ref caster, t);
                    objCharacter = (WS_PlayerData.CharacterObject)caster;
                }
                else if (objCharacter.HavePassiveAura(Spell.Key) && WorldServiceLocator._WS_Spells.SPELLs[Spell.Key].CanCast(ref objCharacter, t, FirstCheck: false) != SpellFailedReason.SPELL_NO_ERROR)
                {
                    objCharacter.RemoveAuraBySpell(Spell.Key);
                }
            }
        }
    }
}
