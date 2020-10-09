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

using System.Collections;
using System.Data;
using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using Mangos.World.AI;
using Mangos.World.Globals;
using Mangos.World.Player;
using Mangos.World.Server;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Objects
{
    public class WS_Pets
    {

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public int[] LevelUpLoyalty = new int[7];
        public int[] LevelStartLoyalty = new int[7];

        public void InitializeLevelUpLoyalty()
        {
            LevelUpLoyalty[0] = 0;
            LevelUpLoyalty[1] = 5500;
            LevelUpLoyalty[2] = 11500;
            LevelUpLoyalty[3] = 17000;
            LevelUpLoyalty[4] = 23500;
            LevelUpLoyalty[5] = 31000;
            LevelUpLoyalty[6] = 39500;
        }

        public void InitializeLevelStartLoyalty()
        {
            LevelStartLoyalty[0] = 0;
            LevelStartLoyalty[1] = 2000;
            LevelStartLoyalty[2] = 4500;
            LevelStartLoyalty[3] = 7000;
            LevelStartLoyalty[4] = 10000;
            LevelStartLoyalty[5] = 13500;
            LevelStartLoyalty[6] = 17500;
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia *//* TODO ERROR: Skipped RegionDirectiveTrivia */
        public class PetObject : WS_Creatures.CreatureObject
        {
            public string PetName = "";
            public bool Renamed = false;
            public WS_Base.BaseUnit Owner = null;
            public bool FollowOwner = true;
            public byte Command = 7;
            public byte State = 6;
            public ArrayList Spells;
            public int XP = 0;

            public PetObject(ulong GUID_, int CreatureID) : base(GUID_, CreatureID)
            {
            }

            public void Spawn()
            {
                AddToWorld();
                if (Owner is WS_PlayerData.CharacterObject)
                {
                    ((WS_PlayerData.CharacterObject)Owner).GroupUpdateFlag = ((WS_PlayerData.CharacterObject)Owner).GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_PET;
                }

                WS_PlayerData.CharacterObject argCaster = (WS_PlayerData.CharacterObject)Owner;
                WS_Base.BaseUnit argPet = this;
                WorldServiceLocator._WS_Pets.SendPetInitialize(ref argCaster, ref argPet);
            }

            public void Hide()
            {
                RemoveFromWorld();
                if (!(Owner is WS_PlayerData.CharacterObject))
                    return;
                ((WS_PlayerData.CharacterObject)Owner).GroupUpdateFlag = ((WS_PlayerData.CharacterObject)Owner).GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_PET;
                var packet = new Packets.PacketClass(OPCODES.SMSG_PET_SPELLS);
                packet.AddUInt64(0UL);
                ((WS_PlayerData.CharacterObject)Owner).client.Send(ref packet);
                packet.Dispose();
            }
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia *//* TODO ERROR: Skipped RegionDirectiveTrivia */
        public void On_CMSG_PET_NAME_QUERY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            packet.GetInt16();
            int PetNumber = packet.GetInt32();
            ulong PetGUID = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "CMSG_PET_NAME_QUERY [Number={0} GUID={1:X}", PetNumber, PetGUID);
            SendPetNameQuery(ref client, PetGUID, PetNumber);
        }

        public void On_CMSG_REQUEST_PET_INFO(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "CMSG_REQUEST_PET_INFO");
            WorldServiceLocator._Packets.DumpPacket(packet.Data, ref client, 6);
        }

        public void On_CMSG_PET_ACTION(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            packet.GetInt16();
            ulong PetGUID = packet.GetUInt64();
            ushort SpellID = packet.GetUInt16();
            ushort SpellFlag = packet.GetUInt16();
            ulong TargetGUID = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "CMSG_PET_ACTION [GUID={0:X} Spell={1} Flag={2:X} Target={3:X}]", PetGUID, SpellID, SpellFlag, TargetGUID);
        }

        public void On_CMSG_PET_CANCEL_AURA(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "CMSG_PET_CANCEL_AURA");
            WorldServiceLocator._Packets.DumpPacket(packet.Data, ref client, 6);
        }

        public void On_CMSG_PET_ABANDON(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            packet.GetInt16();
            ulong PetGUID = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "CMSG_PET_ABANDON [GUID={0:X}]", PetGUID);
        }

        public void On_CMSG_PET_RENAME(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            packet.GetInt16();
            ulong PetGUID = packet.GetUInt64();
            string PetName = packet.GetString();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "CMSG_PET_RENAME [GUID={0:X} Name={1}]", PetGUID, PetName);
        }

        public void On_CMSG_PET_SET_ACTION(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            packet.GetInt16();
            ulong PetGUID = packet.GetUInt64();
            int Position = packet.GetInt32();
            ushort SpellID = packet.GetUInt16();
            short ActionState = packet.GetInt16();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "CMSG_PET_SET_ACTION [GUID={0:X} Pos={1} Spell={2} Action={3}]", PetGUID, Position, SpellID, ActionState);
        }

        public void On_CMSG_PET_SPELL_AUTOCAST(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "CMSG_PET_SPELL_AUTOCAST");
            WorldServiceLocator._Packets.DumpPacket(packet.Data, ref client, 6);
        }

        public void On_CMSG_PET_STOP_ATTACK(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "CMSG_PET_STOP_ATTACK");
            WorldServiceLocator._Packets.DumpPacket(packet.Data, ref client, 6);
        }

        public void On_CMSG_PET_UNLEARN(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "CMSG_PET_UNLEARN");
            WorldServiceLocator._Packets.DumpPacket(packet.Data, ref client, 6);
        }

        public void SendPetNameQuery(ref WS_Network.ClientClass client, ulong PetGUID, int PetNumber)
        {
            if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(PetGUID) == false)
                return;
            if (!(WorldServiceLocator._WorldServer.WORLD_CREATUREs[PetGUID] is PetObject))
                return;
            var response = new Packets.PacketClass(OPCODES.SMSG_PET_NAME_QUERY_RESPONSE);
            response.AddInt32(PetNumber);
            response.AddString(((PetObject)WorldServiceLocator._WorldServer.WORLD_CREATUREs[PetGUID]).PetName); // Pet name
            response.AddInt32(WorldServiceLocator._NativeMethods.timeGetTime("")); // Pet name timestamp
            client.Send(ref response);
            response.Dispose();
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia *//* TODO ERROR: Skipped RegionDirectiveTrivia */
        // TODO: Fix the pet AI
        public class PetAI : WS_Creatures_AI.DefaultAI
        {
            public PetAI(ref WS_Creatures.CreatureObject Creature) : base(ref Creature)
            {
                AllowedMove = false;
            }
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia *//* TODO ERROR: Skipped RegionDirectiveTrivia */
        public void LoadPet(ref WS_PlayerData.CharacterObject objCharacter)
        {
            if (objCharacter.Pet is object)
                return;
            var PetQuery = new DataTable();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT * FROM character_pet WHERE owner = '{0}';", (object)objCharacter.GUID), PetQuery);
            if (PetQuery.Rows.Count == 0)
                return;
            var PetInfo = PetQuery.Rows[0];
            objCharacter.Pet = new PetObject(Conversions.ToULong(PetInfo["id"]) + WorldServiceLocator._Global_Constants.GUID_PET, PetInfo["entry"])
            {
                Owner = objCharacter,
                SummonedBy = objCharacter.GUID,
                CreatedBy = objCharacter.GUID,
                Level = Conversions.ToByte(PetInfo["level"]),
                XP = Conversions.ToInteger(PetInfo["exp"]),
                PetName = Conversions.ToString(PetInfo["name"])
            };
            if (Conversions.ToByte(PetInfo["renamed"]) == 0)
            {
                objCharacter.Pet.Renamed = false;
            }
            else
            {
                objCharacter.Pet.Renamed = true;
            }

            objCharacter.Pet.Faction = objCharacter.Faction;
            objCharacter.Pet.positionX = objCharacter.positionX;
            objCharacter.Pet.positionY = objCharacter.positionY;
            objCharacter.Pet.positionZ = objCharacter.positionZ;
            objCharacter.Pet.MapID = objCharacter.MapID;
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Loaded pet [{0}] for character [{1}].", objCharacter.Pet.GUID, objCharacter.GUID);
        }

        public void SendPetInitialize(ref WS_PlayerData.CharacterObject Caster, ref WS_Base.BaseUnit Pet)
        {
            if (Pet is WS_Creatures.CreatureObject)
            {
            }
            // TODO: Get spells
            else if (Pet is WS_PlayerData.CharacterObject)
            {
                // TODO: Get spells. How do we know which spells to take?
            }

            ushort Command = 7;
            ushort State = 6;
            byte AddList = 0;
            if (Pet is PetObject)
            {
                Command = ((PetObject)Pet).Command;
                State = ((PetObject)Pet).State;
            }

            var packet = new Packets.PacketClass(OPCODES.SMSG_PET_SPELLS);
            packet.AddUInt64(Pet.GUID);
            packet.AddInt32(0);
            packet.AddInt32(0x1010000);
            packet.AddInt16(2);
            packet.AddInt16((short)(Command << 8));
            packet.AddInt16(1);
            packet.AddInt16((short)(Command << 8));
            packet.AddInt16(0);
            packet.AddInt16((short)(Command << 8));
            for (int i = 0; i <= 3; i++)
            {
                packet.AddInt16(0); // SpellID?
                packet.AddInt16(0); // 0xC100?
            }

            packet.AddInt16(2);
            packet.AddInt16((short)(State << 8));
            packet.AddInt16(1);
            packet.AddInt16((short)(State << 8));
            packet.AddInt16(0);
            packet.AddInt16((short)(State << 8));
            packet.AddInt8(AddList);

            // Something based on AddList

            packet.AddInt8(1);
            packet.AddInt32(0x6010);
            packet.AddInt32(0);
            packet.AddInt32(0);
            packet.AddInt16(0);
            Caster.client.Send(ref packet);
            packet.Dispose();
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
    }
}