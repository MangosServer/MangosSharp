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
using Mangos.Common.Legacy;
using Mangos.World.AI;
using Mangos.World.Globals;
using Mangos.World.Network;
using Mangos.World.Player;
using System.Collections;
using System.Data;

namespace Mangos.World.Objects;

public class WS_Pets
{
    public class PetObject : WS_Creatures.CreatureObject
    {
        public string PetName;

        public bool Renamed;

        public WS_Base.BaseUnit Owner;

        public bool FollowOwner;

        public byte Command;

        public byte State;

        public ArrayList Spells;

        public int XP;

        public PetObject(ulong GUID_, int CreatureID)
            : base(GUID_, CreatureID)
        {
            PetName = "";
            Renamed = false;
            Owner = null;
            FollowOwner = true;
            Command = 7;
            State = 6;
            XP = 0;
        }

        public void Spawn()
        {
            AddToWorld();
            if (Owner is WS_PlayerData.CharacterObject @object)
            {
                @object.GroupUpdateFlag |= 0x7FC00u;
            }
            var wS_Pets = WorldServiceLocator._WS_Pets;
            ref var owner = ref Owner;
            WS_PlayerData.CharacterObject Caster = (WS_PlayerData.CharacterObject)owner;
            WS_Base.BaseUnit Pet = this;
            wS_Pets.SendPetInitialize(ref Caster, ref Pet);
            owner = Caster;
        }

        public void Hide()
        {
            RemoveFromWorld();
            if (Owner is WS_PlayerData.CharacterObject @object)
            {
                @object.GroupUpdateFlag |= 0x7FC00u;
                Packets.PacketClass packet = new(Opcodes.SMSG_PET_SPELLS);
                packet.AddUInt64(0uL);
                @object.client.Send(ref packet);
                packet.Dispose();
            }
        }
    }

    public class PetAI : WS_Creatures_AI.DefaultAI
    {
        public PetAI(ref WS_Creatures.CreatureObject Creature)
            : base(ref Creature)
        {
            AllowedMove = false;
        }
    }

    public int[] LevelUpLoyalty;

    public int[] LevelStartLoyalty;

    public WS_Pets()
    {
        LevelUpLoyalty = new int[7];
        LevelStartLoyalty = new int[7];
    }

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

    public void On_CMSG_PET_NAME_QUERY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        packet.GetInt16();
        var PetNumber = packet.GetInt32();
        var PetGUID = packet.GetUInt64();
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "CMSG_PET_NAME_QUERY [Number={0} GUID={1:X}", PetNumber, PetGUID);
        SendPetNameQuery(ref client, PetGUID, PetNumber);
    }

    public void On_CMSG_REQUEST_PET_INFO(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "CMSG_REQUEST_PET_INFO");
        WorldServiceLocator._Packets.DumpPacket(packet.Data, client, 6);
    }

    public void On_CMSG_PET_ACTION(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        packet.GetInt16();
        var PetGUID = packet.GetUInt64();
        var SpellID = packet.GetUInt16();
        var SpellFlag = packet.GetUInt16();
        var TargetGUID = packet.GetUInt64();
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "CMSG_PET_ACTION [GUID={0:X} Spell={1} Flag={2:X} Target={3:X}]", PetGUID, SpellID, SpellFlag, TargetGUID);
    }

    public void On_CMSG_PET_CANCEL_AURA(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "CMSG_PET_CANCEL_AURA");
        WorldServiceLocator._Packets.DumpPacket(packet.Data, client, 6);
    }

    public void On_CMSG_PET_ABANDON(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        packet.GetInt16();
        var PetGUID = packet.GetUInt64();
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "CMSG_PET_ABANDON [GUID={0:X}]", PetGUID);
    }

    public void On_CMSG_PET_RENAME(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        packet.GetInt16();
        var PetGUID = packet.GetUInt64();
        var PetName = packet.GetString();
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "CMSG_PET_RENAME [GUID={0:X} Name={1}]", PetGUID, PetName);
    }

    public void On_CMSG_PET_SET_ACTION(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        packet.GetInt16();
        var PetGUID = packet.GetUInt64();
        var Position = packet.GetInt32();
        var SpellID = packet.GetUInt16();
        var ActionState = packet.GetInt16();
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "CMSG_PET_SET_ACTION [GUID={0:X} Pos={1} Spell={2} Action={3}]", PetGUID, Position, SpellID, ActionState);
    }

    public void On_CMSG_PET_SPELL_AUTOCAST(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "CMSG_PET_SPELL_AUTOCAST");
        WorldServiceLocator._Packets.DumpPacket(packet.Data, client, 6);
    }

    public void On_CMSG_PET_STOP_ATTACK(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "CMSG_PET_STOP_ATTACK");
        WorldServiceLocator._Packets.DumpPacket(packet.Data, client, 6);
    }

    public void On_CMSG_PET_UNLEARN(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "CMSG_PET_UNLEARN");
        WorldServiceLocator._Packets.DumpPacket(packet.Data, client, 6);
    }

    public void SendPetNameQuery(ref WS_Network.ClientClass client, ulong PetGUID, int PetNumber)
    {
        if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(PetGUID) && WorldServiceLocator._WorldServer.WORLD_CREATUREs[PetGUID] is PetObject @object)
        {
            Packets.PacketClass response = new(Opcodes.SMSG_PET_NAME_QUERY_RESPONSE);
            response.AddInt32(PetNumber);
            response.AddString(@object.PetName);
            response.AddInt32(WorldServiceLocator._NativeMethods.timeGetTime(""));
            client.Send(ref response);
            response.Dispose();
        }
    }

    public void LoadPet(ref WS_PlayerData.CharacterObject objCharacter)
    {
        if (objCharacter.Pet != null)
        {
            return;
        }
        DataTable PetQuery = new();
        WorldServiceLocator._WorldServer.CharacterDatabase.Query($"SELECT * FROM character_pet WHERE owner = '{objCharacter.GUID}';", ref PetQuery);
        if (PetQuery.Rows.Count != 0)
        {
            var row = PetQuery.Rows[0];
            objCharacter.Pet = new PetObject(checked(row.As<ulong>("id") + WorldServiceLocator._Global_Constants.GUID_PET), row.As<int>("entry"))
            {
                Owner = objCharacter,
                SummonedBy = objCharacter.GUID,
                CreatedBy = objCharacter.GUID,
                Level = row.As<byte>("level"),
                XP = row.As<int>("exp"),
                PetName = row.As<string>("name")
            };
            objCharacter.Pet.Renamed = row.As<byte>("renamed") != 0;
            objCharacter.Pet.Faction = objCharacter.Faction;
            objCharacter.Pet.positionX = objCharacter.positionX;
            objCharacter.Pet.positionY = objCharacter.positionY;
            objCharacter.Pet.positionZ = objCharacter.positionZ;
            objCharacter.Pet.MapID = objCharacter.MapID;
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Loaded pet [{0}] for character [{1}].", objCharacter.Pet.GUID, objCharacter.GUID);
        }
    }

    public void SendPetInitialize(ref WS_PlayerData.CharacterObject Caster, ref WS_Base.BaseUnit Pet)
    {
        if (Pet is WS_Creatures.CreatureObject or WS_PlayerData.CharacterObject)
        {
        }
        ushort Command = 7;
        ushort State = 6;
        byte AddList = 0;
        if (Pet is PetObject @object)
        {
            Command = @object.Command;
            State = @object.State;
        }
        Packets.PacketClass packet = new(Opcodes.SMSG_PET_SPELLS);
        packet.AddUInt64(Pet.GUID);
        packet.AddInt32(0);
        packet.AddInt32(16842752);
        packet.AddInt16(2);
        checked
        {
            packet.AddInt16((short)unchecked((ushort)(Command << 8)));
            packet.AddInt16(1);
            packet.AddInt16((short)unchecked((ushort)(Command << 8)));
            packet.AddInt16(0);
            packet.AddInt16((short)unchecked((ushort)(Command << 8)));
            var i = 0;
            do
            {
                packet.AddInt16(0);
                packet.AddInt16(0);
                i++;
            }
            while (i <= 3);
            packet.AddInt16(2);
            packet.AddInt16((short)unchecked((ushort)(State << 8)));
            packet.AddInt16(1);
            packet.AddInt16((short)unchecked((ushort)(State << 8)));
            packet.AddInt16(0);
            packet.AddInt16((short)unchecked((ushort)(State << 8)));
            packet.AddInt8(AddList);
            packet.AddInt8(1);
            packet.AddInt32(24592);
            packet.AddInt32(0);
            packet.AddInt32(0);
            packet.AddInt16(0);
            Caster.client.Send(ref packet);
            packet.Dispose();
        }
    }
}
