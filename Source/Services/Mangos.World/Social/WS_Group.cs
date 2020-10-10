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
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Group;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Player;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Social
{
    public class WS_Group
    {
        public readonly Dictionary<long, Group> Groups = new Dictionary<long, Group>();
        private ulong _lastLooter = 0UL;

        public sealed class Group : IDisposable
        {
            public readonly long ID;
            public GroupType Type = GroupType.PARTY;
            public GroupDungeonDifficulty DungeonDifficulty = GroupDungeonDifficulty.DIFFICULTY_NORMAL;
            public GroupLootMethod LootMethod = GroupLootMethod.LOOT_GROUP;
            public GroupLootThreshold LootThreshold = GroupLootThreshold.Uncommon;
            public ulong Leader;
            public List<ulong> LocalMembers;
            public WS_PlayerData.CharacterObject LocalLootMaster;

            public Group(long groupID)
            {
                ID = groupID;
                WorldServiceLocator._WS_Group.Groups.Add(ID, this);
            }

            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            private bool _disposedValue; // To detect redundant calls

            // IDisposable
            private void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                    // TODO: set large fields to null.
                    WorldServiceLocator._WS_Group.Groups.Remove(ID);
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
            /// <summary>
            /// Broadcasts the specified p.
            /// </summary>
            /// <param name="p">The p.</param>
            /// <returns></returns>
            public void Broadcast(Packets.PacketClass p)
            {
                p.UpdateLength();
                WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.BroadcastGroup(ID, p.Data);
            }

            /// <summary>
            /// Gets the next looter.
            /// </summary>
            /// <returns></returns>
            public WS_PlayerData.CharacterObject GetNextLooter()
            {
                bool nextIsLooter = false;
                bool nextLooterFound = false;
                foreach (ulong guid in LocalMembers)
                {
                    if (nextIsLooter)
                    {
                        WorldServiceLocator._WS_Group._lastLooter = guid;
                        nextLooterFound = true;
                        break;
                    }

                    if (guid == WorldServiceLocator._WS_Group._lastLooter)
                        nextIsLooter = true;
                }

                if (!nextLooterFound)
                {
                    WorldServiceLocator._WS_Group._lastLooter = LocalMembers[0];
                }

                return WorldServiceLocator._WorldServer.CHARACTERs[WorldServiceLocator._WS_Group._lastLooter];
            }

            /// <summary>
            /// Gets the members count.
            /// </summary>
            /// <returns></returns>
            public int GetMembersCount()
            {
                return LocalMembers.Count;
            }
        }

        /// <summary>
        /// Builds the party member stats.
        /// </summary>
        /// <param name="objCharacter">The objCharacter.</param>
        /// <param name="flag">The flag.</param>
        /// <returns></returns>
        public Packets.PacketClass BuildPartyMemberStats(ref WS_PlayerData.CharacterObject objCharacter, uint flag)
        {
            OPCODES opCode = OPCODES.SMSG_PARTY_MEMBER_STATS;
            if (flag == (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FULL || flag == (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FULL_PET)
            {
                opCode = OPCODES.SMSG_PARTY_MEMBER_STATS_FULL;
                if (objCharacter.ManaType != ManaTypes.TYPE_MANA)
                    flag = flag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_POWER_TYPE;
            }

            var packet = new Packets.PacketClass(opCode);
            packet.AddPackGUID(objCharacter.GUID);
            packet.AddUInt32(flag);
            if (Conversions.ToBoolean(flag & (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_STATUS))
            {
                byte memberFlags = (byte)Globals.Functions.PartyMemberStatsStatus.STATUS_ONLINE;
                if (objCharacter.isPvP)
                    memberFlags = (byte)(memberFlags | (byte)Globals.Functions.PartyMemberStatsStatus.STATUS_PVP);
                if (objCharacter.DEAD)
                    memberFlags = (byte)(memberFlags | (byte)Globals.Functions.PartyMemberStatsStatus.STATUS_DEAD);
                packet.AddInt8(memberFlags);
            }

            if (Conversions.ToBoolean(flag & (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_CUR_HP))
                packet.AddUInt16((ushort)objCharacter.Life.Current);
            if (Conversions.ToBoolean(flag & (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_MAX_HP))
                packet.AddUInt16((ushort)objCharacter.Life.Maximum);
            if (Conversions.ToBoolean(flag & (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_POWER_TYPE))
                packet.AddInt8((byte)objCharacter.ManaType);
            if (Conversions.ToBoolean(flag & (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_CUR_POWER))
            {
                if (objCharacter.ManaType == ManaTypes.TYPE_RAGE)
                {
                    packet.AddUInt16((ushort)objCharacter.Rage.Current);
                }
                else if (objCharacter.ManaType == ManaTypes.TYPE_ENERGY)
                {
                    packet.AddUInt16((ushort)objCharacter.Energy.Current);
                }
                else
                {
                    packet.AddUInt16((ushort)objCharacter.Mana.Current);
                }
            }

            if (Conversions.ToBoolean(flag & (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_MAX_POWER))
            {
                if (objCharacter.ManaType == ManaTypes.TYPE_RAGE)
                {
                    packet.AddUInt16((ushort)objCharacter.Rage.Maximum);
                }
                else if (objCharacter.ManaType == ManaTypes.TYPE_ENERGY)
                {
                    packet.AddUInt16((ushort)objCharacter.Energy.Maximum);
                }
                else
                {
                    packet.AddUInt16((ushort)objCharacter.Mana.Maximum);
                }
            }

            if (Conversions.ToBoolean(flag & (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_LEVEL))
                packet.AddUInt16(objCharacter.Level);
            if (Conversions.ToBoolean(flag & (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_ZONE))
                packet.AddUInt16((ushort)objCharacter.ZoneID);
            if (Conversions.ToBoolean(flag & (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_POSITION))
            {
                packet.AddInt16((short)Conversion.Fix(objCharacter.positionX));
                packet.AddInt16((short)Conversion.Fix(objCharacter.positionY));
            }

            if (Conversions.ToBoolean(flag & (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_AURAS))
            {
                ulong auraMask = 0UL;
                int auraPos = packet.Data.Length;
                packet.AddUInt64(0UL); // AuraMask (is set after the loop)
                for (int i = 0, loopTo = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1; i <= loopTo; i++)
                {
                    if (objCharacter.ActiveSpells[i] is object)
                    {
                        auraMask = auraMask | 1 << (int)(ulong)i;
                        packet.AddUInt16((ushort)objCharacter.ActiveSpells[i].SpellID);
                        packet.AddInt8(1); // Stack Count?
                    }
                }

                packet.AddUInt64(auraMask, auraPos); // Set the AuraMask
            }

            if (Conversions.ToBoolean(flag & (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_PET_GUID))
            {
                if (objCharacter.Pet is object)
                {
                    packet.AddUInt64(objCharacter.Pet.GUID);
                }
                else
                {
                    packet.AddInt64(0L);
                }
            }

            if (Conversions.ToBoolean(flag & (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_PET_NAME))
            {
                if (objCharacter.Pet is object)
                {
                    packet.AddString(objCharacter.Pet.PetName);
                }
                else
                {
                    packet.AddString("");
                }
            }

            if (Conversions.ToBoolean(flag & (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_PET_MODEL_ID))
            {
                if (objCharacter.Pet is object)
                {
                    packet.AddUInt16((ushort)objCharacter.Pet.Model);
                }
                else
                {
                    packet.AddInt16(0);
                }
            }

            if (Conversions.ToBoolean(flag & (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_PET_CUR_HP))
            {
                if (objCharacter.Pet is object)
                {
                    packet.AddUInt16((ushort)objCharacter.Pet.Life.Current);
                }
                else
                {
                    packet.AddInt16(0);
                }
            }

            if (Conversions.ToBoolean(flag & (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_PET_MAX_HP))
            {
                if (objCharacter.Pet is object)
                {
                    packet.AddUInt16((ushort)objCharacter.Pet.Life.Maximum);
                }
                else
                {
                    packet.AddInt16(0);
                }
            }

            if (Conversions.ToBoolean(flag & (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_PET_POWER_TYPE))
            {
                if (objCharacter.Pet is object)
                {
                    packet.AddInt8((byte)ManaTypes.TYPE_FOCUS);
                }
                else
                {
                    packet.AddInt8(0);
                }
            }

            if (Conversions.ToBoolean(flag & (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_PET_CUR_POWER))
            {
                if (objCharacter.Pet is object)
                {
                    packet.AddUInt16((ushort)objCharacter.Pet.Mana.Current);
                }
                else
                {
                    packet.AddInt16(0);
                }
            }

            if (Conversions.ToBoolean(flag & (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_PET_MAX_POWER))
            {
                if (objCharacter.Pet is object)
                {
                    packet.AddUInt16((ushort)objCharacter.Pet.Mana.Maximum);
                }
                else
                {
                    packet.AddInt16(0);
                }
            }

            if (Conversions.ToBoolean(flag & (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_PET_AURAS))
            {
                if (objCharacter.Pet is object)
                {
                    ulong auraMask = 0UL;
                    int auraPos = packet.Data.Length;
                    packet.AddUInt64(0UL); // AuraMask (is set after the loop)
                    for (int i = 0, loopTo1 = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1; i <= loopTo1; i++)
                    {
                        if (objCharacter.Pet.ActiveSpells[i] is object)
                        {
                            auraMask = auraMask | 1 << (int)(ulong)i;
                            packet.AddUInt16((ushort)objCharacter.Pet.ActiveSpells[i].SpellID);
                            packet.AddInt8(1); // Stack Count?
                        }
                    }

                    packet.AddUInt64(auraMask, auraPos); // Set the AuraMask
                }
                else
                {
                    packet.AddInt64(0L);
                }
            }

            return packet;
        }
    }
}