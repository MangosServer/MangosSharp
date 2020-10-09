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
using System.Threading;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Spell;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Server;
using Mangos.World.Spells;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Player
{
    public class WS_PlayerHelper
    {
        public class TSkill
        {
            private short _Current = 0;
            public short Bonus = 0;
            public short Base = 300;

            public TSkill(short CurrentVal, short MaximumVal = 375)
            {
                Current = CurrentVal;
                Base = MaximumVal;
            }

            public void Increment(short Incrementator = 1)
            {
                if ((short)(Current + Incrementator) < Base)
                {
                    Current += Incrementator;
                }
                else
                {
                    Current = Base;
                }
            }

            public int Maximum
            {
                get
                {
                    return Base;
                }
            }

            public int MaximumWithBonus
            {
                get
                {
                    return Base + Bonus;
                }
            }

            public short Current
            {
                get
                {
                    return _Current;
                }

                set
                {
                    if (value <= Maximum)
                        _Current = value;
                }
            }

            public short CurrentWithBonus
            {
                get
                {
                    return (short)(_Current + Bonus);
                }
            }

            public int GetSkill
            {
                get
                {
                    return _Current + (Conversions.ToInteger(Base + Bonus) << 16);
                }
            }
        }

        public class TStatBar
        {
            private int _Current = 0;
            public int Bonus = 0;
            public int Base = 0;
            public float Modifier = 1f;

            public void Increment(int Incrementator = 1)
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

            public TStatBar(int CurrentVal, int BaseVal, int BonusVal)
            {
                _Current = CurrentVal;
                Bonus = BonusVal;
                Base = BaseVal;
            }

            public int Maximum
            {
                get
                {
                    return (int)((Bonus + Base) * Modifier);
                }
            }

            public int Current
            {
                get
                {
                    return (int)(_Current * Modifier);
                }

                set
                {
                    if (value <= Maximum)
                        _Current = value;
                    else
                        _Current = Maximum;
                    if (_Current < 0)
                        _Current = 0;
                }
            }
        }

        public class TStat
        {
            public int Base = 0;
            public short PositiveBonus = 0;
            public short NegativeBonus = 0;
            public float BaseModifier = 1f;
            public float Modifier = 1f;

            public int RealBase
            {
                get
                {
                    return Base - PositiveBonus + NegativeBonus;
                }

                set
                {
                    Base = Base - PositiveBonus + NegativeBonus;
                    Base = value;
                    Base = Base + PositiveBonus - NegativeBonus;
                }
            }

            public TStat(byte BaseValue = 0, byte PosValue = 0, byte NegValue = 0)
            {
                Base = BaseValue;
                PositiveBonus = PosValue;
                PositiveBonus = NegValue;
            }
        }

        public class TDamageBonus
        {
            public int PositiveBonus = 0;
            public int NegativeBonus = 0;
            public float Modifier = 1f;

            public int Value
            {
                get
                {
                    return (int)((PositiveBonus - NegativeBonus) * Modifier);
                }
            }

            public TDamageBonus(byte PosValue = 0, byte NegValue = 0)
            {
                PositiveBonus = PosValue;
                PositiveBonus = NegValue;
            }
        }

        public class THonor
        {
            public ulong CharGUID = 0UL;
            public short HonorPoints = 0;                 // ! MAX=1000 ?
            public byte HonorRank = 0;
            public byte HonorHightestRank = 0;
            public int Standing = 0;
            public int HonorLastWeek = 0;
            public int HonorThisWeek = 0;
            public int HonorYesterday = 0;
            public int KillsLastWeek = 0;
            public int KillsThisWeek = 0;
            public int KillsYesterday = 0;
            public int KillsHonorableToday = 0;
            public int KillsDisHonorableToday = 0;
            public int KillsHonorableLifetime = 0;
            public int KillsDisHonorableLifetime = 0;

            // ??? not sure why there is two save points.
            // here and back on PlayerDataType Line: 401
            public void Save()
            {
                string tmp = "UPDATE characters_honor SET";
                tmp = tmp + " honor_points =\"" + HonorPoints + "\"";
                tmp = tmp + ", honor_rank =" + HonorRank;
                tmp = tmp + ", honor_hightestRank =" + HonorHightestRank;
                tmp = tmp + ", honor_standing =" + Standing;
                tmp = tmp + ", honor_lastWeek =" + HonorLastWeek;
                tmp = tmp + ", honor_thisWeek =" + HonorThisWeek;
                tmp = tmp + ", honor_yesterday =" + HonorYesterday;
                tmp = tmp + ", kills_lastWeek =" + KillsLastWeek;
                tmp = tmp + ", kills_thisWeek =" + KillsThisWeek;
                tmp = tmp + ", kills_yesterday =" + KillsYesterday;
                tmp = tmp + ", kills_dishonorableToday =" + KillsDisHonorableToday;
                tmp = tmp + ", kills_honorableToday =" + KillsHonorableToday;
                tmp = tmp + ", kills_dishonorableLifetime =" + KillsDisHonorableLifetime;
                tmp = tmp + ", kills_honorableLifetime =" + KillsHonorableLifetime;
                tmp += string.Format(" WHERE char_guid = \"{0}\";", CharGUID);
                WorldServiceLocator._WorldServer.CharacterDatabase.Update(tmp);
            }

            // ???
            public void Load(ulong GUID)
            {
            }

            // ???
            public void SaveAsNew(ulong GUID)
            {
            }
        }

        public class TReputation
        {
            // 1:"AtWar" clickable but not checked
            // 3:"AtWar" clickable and checked
            public int Flags = 0;
            public int Value = 0;
        }

        public class TActionButton
        {
            public byte ActionType = 0;
            public byte ActionMisc = 0;
            public int Action = 0;

            public TActionButton(int Action_, byte Type_, byte Misc_)
            {
                ActionType = Type_;
                ActionMisc = Misc_;
                Action = Action_;
            }
        }

        public class TDrowningTimer : IDisposable
        {
            private Timer DrowningTimer = null;
            public int DrowningValue = 70000;
            public byte DrowningDamage = 1;
            public ulong CharacterGUID = 0UL;

            public TDrowningTimer(ref WS_PlayerData.CharacterObject Character)
            {
                CharacterGUID = Character.GUID;
                Character.StartMirrorTimer(MirrorTimer.DROWNING, 70000);
                DrowningTimer = new Timer(Character.HandleDrowning, null, 2000, 1000);
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
                    if (Information.IsNothing(DrowningTimer) == false)
                    {
                        DrowningTimer.Dispose();
                        DrowningTimer = null;
                    }

                    if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(CharacterGUID))
                        WorldServiceLocator._WorldServer.CHARACTERs[CharacterGUID].StopMirrorTimer(1);
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

        public class TRepopTimer : IDisposable
        {
            private Timer RepopTimer = null;
            public WS_PlayerData.CharacterObject Character = null;

            public TRepopTimer(ref WS_PlayerData.CharacterObject Character)
            {
                this.Character = Character;
                RepopTimer = new Timer(Repop, null, 360000, 360000);
            }

            public void Repop(object Obj)
            {
                WorldServiceLocator._WS_Handlers_Misc.CharacterRepop(ref Character.client);
                Character.repopTimer = null;
                Dispose();
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
                    RepopTimer.Dispose();
                    RepopTimer = null;
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

        public void SendBindPointUpdate(ref WS_Network.ClientClass client, ref WS_PlayerData.CharacterObject Character)
        {
            var SMSG_BINDPOINTUPDATE = new Packets.PacketClass(OPCODES.SMSG_BINDPOINTUPDATE);
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
            var SMSG_SET_REST_START = new Packets.PacketClass(OPCODES.SMSG_SET_REST_START);
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
            var SMSG_TUTORIAL_FLAGS = new Packets.PacketClass(OPCODES.SMSG_TUTORIAL_FLAGS);
            try
            {
                // [8*Int32] or [32 Bytes] or [256 Bits Flags] Total!!!
                // SMSG_TUTORIAL_FLAGS.AddInt8(0)
                // SMSG_TUTORIAL_FLAGS.AddInt8(Character.TutorialFlags.Length)
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
            var packet = new Packets.PacketClass(OPCODES.SMSG_INITIALIZE_FACTIONS);
            try
            {
                packet.AddInt32(64);
                for (byte i = 0; i <= 63; i++)
                {
                    packet.AddInt8((byte)Character.Reputation[i].Flags);                               // Flags
                    packet.AddInt32(Character.Reputation[i].Value);                              // Standing
                }

                client.Send(ref packet);
            }
            finally
            {
                packet.Dispose();
            }
        }

        public void SendActionButtons(ref WS_Network.ClientClass client, ref WS_PlayerData.CharacterObject Character)
        {
            var packet = new Packets.PacketClass(OPCODES.SMSG_ACTION_BUTTONS);
            try
            {
                for (byte i = 0; i <= 119; i++)    // or 480 ?
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
                }

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
            ushort NumberOfFields;
            switch (Character.ZoneID)
            {
                case 0:
                case 1:
                case 4:
                case 8:
                case 10:
                case 11:
                case 12:
                case 36:
                case 38:
                case 40:
                case 41:
                case 51:
                case 267:
                case 1519:
                case 1537:
                case 2257:
                case 2918:
                    {
                        NumberOfFields = 6;
                        break;
                    }

                case 2597:
                    {
                        NumberOfFields = 81;
                        break;
                    }

                case 3277:
                    {
                        NumberOfFields = 14;
                        break;
                    }

                case 3358:
                case 3820:
                    {
                        NumberOfFields = 38;
                        break;
                    }

                case 3483:
                    {
                        NumberOfFields = 22;
                        break;
                    }

                case 3519:
                    {
                        NumberOfFields = 36;
                        break;
                    }

                case 3521:
                    {
                        NumberOfFields = 35;
                        break;
                    }

                case 3698:
                case 3702:
                case 3968:
                    {
                        NumberOfFields = 9;
                        break;
                    }

                case 3703:
                    {
                        NumberOfFields = 9;
                        break;
                    }

                default:
                    {
                        NumberOfFields = 10;
                        break;
                    }
            }

            var packet = new Packets.PacketClass(OPCODES.SMSG_INIT_WORLD_STATES);
            try
            {
                packet.AddUInt32(Character.MapID);
                packet.AddInt32(Character.ZoneID);
                packet.AddInt32(Character.AreaID);
                packet.AddUInt16(NumberOfFields);
                packet.AddUInt32(0x8D8U);
                packet.AddUInt32(0x0U);
                packet.AddUInt32(0x8D7U);
                packet.AddUInt32(0x0U);
                packet.AddUInt32(0x8D6U);
                packet.AddUInt32(0x0U);
                packet.AddUInt32(0x8D5U);
                packet.AddUInt32(0x0U);
                packet.AddUInt32(0x8D4U);
                packet.AddUInt32(0x0U);
                packet.AddUInt32(0x8D3U);
                packet.AddUInt32(0x0U);
                switch (Character.ZoneID)
                {
                    case 1:
                    case 11:
                    case 12:
                    case 38:
                    case 40:
                    case 51:
                    case 1519:
                    case 1537:
                    case 2257:
                        {
                            break;
                        }

                    case 2597: // AV
                        {
                            break;
                        }
                    // TODO
                    case 3277: // WSG
                        {
                            break;
                        }
                    // TODO
                    case 3358: // AB
                        {
                            break;
                        }
                    // TODO
                    case 3820: // Eye of the Storm
                        {
                            break;
                        }
                    // TODO
                    case 3968: // Ruins of Lordaeron Arena
                        {
                            packet.AddUInt32(0xBB8U);
                            packet.AddUInt32(0x0U);
                            packet.AddUInt32(0xBB9U);
                            packet.AddUInt32(0x0U);
                            packet.AddUInt32(0xBBAU);
                            packet.AddUInt32(0x0U);
                            break;
                        }

                    default:
                        {
                            packet.AddUInt32(0x914U);
                            packet.AddUInt32(0x0U);
                            packet.AddUInt32(0x913U);
                            packet.AddUInt32(0x0U);
                            packet.AddUInt32(0x912U);
                            packet.AddUInt32(0x0U);
                            packet.AddUInt32(0x915U);
                            packet.AddUInt32(0x0U);
                            break;
                        }
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
            var packet = new Packets.PacketClass(OPCODES.SMSG_INITIAL_SPELLS);
            try
            {
                packet.AddInt8(0);
                int countPos = packet.Data.Length;
                packet.AddInt16(0); // Updated later
                int spellCount = 0;
                var spellCooldowns = new Dictionary<int, KeyValuePair<uint, int>>();
                foreach (KeyValuePair<int, WS_Spells.CharacterSpell> Spell in Character.Spells)
                {
                    if (Spell.Value.Active == 1)
                    {
                        packet.AddUInt16((ushort)Spell.Key); // SpellID
                        packet.AddInt16(0); // Unknown
                        spellCount += 1;
                        if (Spell.Value.Cooldown > 0U)
                        {
                            spellCooldowns.Add(Spell.Key, new KeyValuePair<uint, int>(Spell.Value.Cooldown, 0));
                        }
                    }
                }

                packet.AddInt16((short)spellCount, countPos);
                spellCount = 0;
                countPos = packet.Data.Length;
                packet.AddInt16(0); // Updated later
                foreach (KeyValuePair<int, KeyValuePair<uint, int>> Cooldown in spellCooldowns)
                {
                    if (WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(Cooldown.Key) == false)
                        continue;
                    packet.AddUInt16((ushort)Cooldown.Key); // SpellID
                    int timeLeft = 0;
                    if (Cooldown.Value.Key > WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now))
                    {
                        timeLeft = (int)((Cooldown.Value.Key - WorldServiceLocator._Functions.GetTimestamp(DateAndTime.Now)) * 1000L);
                    }

                    packet.AddUInt16((ushort)Cooldown.Value.Value); // CastItemID
                    packet.AddUInt16((ushort)WorldServiceLocator._WS_Spells.SPELLs[Cooldown.Key].Category); // SpellCategory
                    if (WorldServiceLocator._WS_Spells.SPELLs[Cooldown.Key].CategoryCooldown > 0)
                    {
                        packet.AddInt32(0); // SpellCooldown
                        packet.AddInt32(timeLeft); // CategoryCooldown
                    }
                    else
                    {
                        packet.AddInt32(timeLeft); // SpellCooldown
                        packet.AddInt32(0);
                    } // CategoryCooldown

                    spellCount += 1;
                }

                packet.AddInt16((short)spellCount, countPos);
                client.Send(ref packet);
            }
            finally
            {
                packet.Dispose();
            }
        }

        public void InitializeTalentSpells(WS_PlayerData.CharacterObject objCharacter)
        {
            var t = new WS_Spells.SpellTargets();
            Objects.WS_Base.BaseUnit argobjCharacter = objCharacter;
            t.SetTarget_SELF(ref argobjCharacter);
            foreach (KeyValuePair<int, WS_Spells.CharacterSpell> Spell in objCharacter.Spells)
            {
                if (WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(Spell.Key) && WorldServiceLocator._WS_Spells.SPELLs[Spell.Key].IsPassive)
                {
                    // DONE: Add passive spell we don't have
                    // DONE: Remove passive spells we can't have anymore
                    if (objCharacter.HavePassiveAura(Spell.Key) == false && WorldServiceLocator._WS_Spells.SPELLs[Spell.Key].CanCast(ref objCharacter, t, false) == SpellFailedReason.SPELL_NO_ERROR)
                    {
                        Objects.WS_Base.BaseObject argcaster = objCharacter;
                        WorldServiceLocator._WS_Spells.SPELLs[Spell.Key].Apply(ref argcaster, t);
                    }
                    else if (objCharacter.HavePassiveAura(Spell.Key) && WorldServiceLocator._WS_Spells.SPELLs[Spell.Key].CanCast(ref objCharacter, t, false) != SpellFailedReason.SPELL_NO_ERROR)
                    {
                        objCharacter.RemoveAuraBySpell(Spell.Key);
                    }
                }
            }
        }
    }
}