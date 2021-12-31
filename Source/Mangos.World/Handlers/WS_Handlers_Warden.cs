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
using Mangos.Common.Enums.Warden;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Network;
using Mangos.World.Player;
using Mangos.World.Warden;
using System;
using System.IO;
using System.Security.Cryptography;

namespace Mangos.World.Handlers;

public class WS_Handlers_Warden
{
    public class WardenData
    {
        public byte Failed;

        public bool Ready;

        public byte[] KeyOut;

        public byte[] KeyIn;

        public byte[] Seed;

        public byte[] K;

        public byte[] ClientSeed;

        public byte xorByte;

        public WS_Warden.WardenScan Scan;

        public WardenData()
        {
            Failed = 0;
            Ready = false;
            KeyOut = null;
            KeyIn = null;
            Seed = null;
            K = null;
            ClientSeed = null;
            xorByte = 0;
            Scan = null;
        }
    }

    public class MaievData
    {
        public int index;

        public byte[] source1;

        public byte[] source2;

        public byte[] data;

        public MaievData(byte[] seed)
        {
            index = 0;
            data = new byte[20];
            SHA1Managed sha1 = new();
            source1 = sha1.ComputeHash(seed, 0, 20);
            source2 = sha1.ComputeHash(seed, 20, 20);
            Update();
        }

        public void Update()
        {
            var buffer1 = new byte[60];
            SHA1Managed sha1 = new();
            Buffer.BlockCopy(source1, 0, buffer1, 0, 20);
            Buffer.BlockCopy(data, 0, buffer1, 20, 20);
            Buffer.BlockCopy(source2, 0, buffer1, 40, 20);
            data = sha1.ComputeHash(buffer1);
        }

        private byte GetByte()
        {
            var r = data[index];
            checked
            {
                index++;
                if (index >= 20)
                {
                    Update();
                    index = 0;
                }
                return r;
            }
        }

        public byte[] GetBytes(int count)
        {
            checked
            {
                var b = new byte[count - 1 + 1];
                var num = count - 1;
                for (var i = 0; i <= num; i++)
                {
                    b[i] = GetByte();
                }
                return b;
            }
        }
    }

    public class RC4
    {
        public static byte[] Init(byte[] @base)
        {
            var val = 0;
            var position = 0;
            var key = new byte[258];
            var j = 0;
            checked
            {
                do
                {
                    key[j] = (byte)j;
                    j++;
                }
                while (j <= 255);
                key[256] = 0;
                key[257] = 0;
                var i = 1;
                do
                {
                    val = val + key[checked((i * 4) - 4)] + @base[position % @base.Length];
                    val &= 0xFF;
                    position++;
                    var temp = key[(i * 4) - 4];
                    key[(i * 4) - 4] = key[val & 0xFF];
                    key[val & 0xFF] = temp;
                    val = val + key[checked((i * 4) - 3)] + @base[position % @base.Length];
                    val &= 0xFF;
                    position++;
                    temp = key[(i * 4) - 3];
                    key[(i * 4) - 3] = key[val & 0xFF];
                    key[val & 0xFF] = temp;
                    val = val + key[checked((i * 4) - 2)] + @base[position % @base.Length];
                    val &= 0xFF;
                    position++;
                    temp = key[(i * 4) - 2];
                    key[(i * 4) - 2] = key[val & 0xFF];
                    key[val & 0xFF] = temp;
                    val = val + key[checked((i * 4) - 1)] + @base[position % @base.Length];
                    val &= 0xFF;
                    position++;
                    temp = key[(i * 4) - 1];
                    key[(i * 4) - 1] = key[val & 0xFF];
                    key[val & 0xFF] = temp;
                    i++;
                }
                while (i <= 64);
                return key;
            }
        }

        public static void Crypt(ref byte[] data, byte[] key)
        {
            checked
            {
                var num = data.Length - 1;
                for (var i = 0; i <= num; i++)
                {
                    key[256] = (byte)((key[256] + 1) & 0xFF);
                    key[257] = (byte)((key[257] + key[key[256]]) & 0xFF);
                    var temp = key[key[257] & 0xFF];
                    key[key[257]] = key[key[256]];
                    key[key[256]] = temp;
                    unchecked
                    {
                        data[i] = (byte)(data[i] ^ key[checked(key[key[257]] + key[key[256]]) & 0xFF]);
                    }
                }
            }
        }
    }

    private readonly int OutKeyAdr;

    private readonly int InKeyAdr;

    public void On_CMSG_WARDEN_DATA(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
    {
        var b = new byte[checked(packet.Data.Length - 6 - 1 + 1)];
        Buffer.BlockCopy(packet.Data, 6, b, 0, b.Length);
        RC4.Crypt(ref b, client.Character.WardenData.KeyOut);
        Buffer.BlockCopy(b, 0, packet.Data, 6, b.Length);
        packet.GetInt16();
        MaievResponse Response = (MaievResponse)packet.GetInt8();
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_WARDEN_DATA [{2}]", client.IP, client.Port, Response);
        if (!client.Character.WardenData.Ready)
        {
            return;
        }
        switch (Response)
        {
            case MaievResponse.MAIEV_RESPONSE_FAILED_OR_MISSING:
                MaievSendTransfer(ref client.Character);
                break;

            case MaievResponse.MAIEV_RESPONSE_SUCCESS:
                MaievSendSeed(ref client.Character);
                break;

            case MaievResponse.MAIEV_RESPONSE_RESULT:
                MaievResult(ref client.Character, ref packet);
                break;

            case MaievResponse.MAIEV_RESPONSE_HASH:
                {
                    var hash = new byte[20];
                    Buffer.BlockCopy(packet.Data, packet.Offset, hash, 0, 20);
                    WorldServiceLocator._WS_Warden.Maiev.GenerateNewRC4Keys(client.Character.WardenData.K);
                    var PacketData = new byte[17]
                    {
                    5,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0
                    };
                    Buffer.BlockCopy(client.Character.WardenData.Seed, 0, PacketData, 1, 16);
                    var HandledBytes = WorldServiceLocator._WS_Warden.Maiev.HandlePacket(PacketData);
                    if (HandledBytes <= 0)
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "[WARDEN] Failed to handle 0x05 packet.");
                        break;
                    }
                    var thePacket = WorldServiceLocator._WS_Warden.Maiev.ReadPacket();
                    var ourHash = new byte[20];
                    Array.Copy(thePacket, 1, ourHash, 0, ourHash.Length);
                    WorldServiceLocator._WS_Warden.Maiev.ReadXorByte(ref client.Character);
                    WorldServiceLocator._WS_Warden.Maiev.ReadKeys(ref client.Character);
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[WARDEN] XorByte: {0}", client.Character.WardenData.xorByte);
                    var HashCorrect = true;
                    var i = 0;
                    do
                    {
                        if (hash[i] != ourHash[i])
                        {
                            HashCorrect = false;
                            break;
                        }
                        i = checked(i + 1);
                    }
                    while (i <= 19);
                    if (!HashCorrect)
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "[WARDEN] Hashes in packet 0x05 didn't match. Cheater?");
                    }
                    break;
                }
            case (MaievResponse)3:
                break;
            default:
                break;
        }
    }

    public void MaievInit(ref WS_PlayerData.CharacterObject objCharacter)
    {
        var i = WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.ClientGetCryptKey(objCharacter.client.Index);
        MaievData j = new(i);
        var seedOut = j.GetBytes(16);
        var seedIn = j.GetBytes(16);
        objCharacter.WardenData.KeyOut = RC4.Init(seedOut);
        objCharacter.WardenData.KeyIn = RC4.Init(seedIn);
        objCharacter.WardenData.Ready = true;
        objCharacter.WardenData.Scan = new WS_Warden.WardenScan(ref objCharacter);
        objCharacter.WardenData.xorByte = 0;
        objCharacter.WardenData.K = i;
        WorldServiceLocator._Functions.RAND_bytes(ref objCharacter.WardenData.Seed, 16);
        MaievSendModule(ref objCharacter);
    }

    public void MaievSendModule(ref WS_PlayerData.CharacterObject objCharacter)
    {
        if (!objCharacter.WardenData.Ready)
        {
            throw new ApplicationException("Maiev.mod not ready!");
        }
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_WARDEN_DATA [{2}]", objCharacter.client.IP, objCharacter.client.Port, WorldServiceLocator._WS_Warden.Maiev.ModuleName);
        Packets.PacketClass r = new(Opcodes.SMSG_WARDEN_DATA);
        r.AddInt8(0);
        r.AddByteArray(WorldServiceLocator._WS_Warden.Maiev.WardenModule);
        r.AddByteArray(WorldServiceLocator._WS_Warden.Maiev.ModuleKey);
        r.AddUInt32(checked((uint)WorldServiceLocator._WS_Warden.Maiev.ModuleSize));
        WorldServiceLocator._WS_Warden.SendWardenPacket(ref objCharacter, ref r);
    }

    public void MaievSendTransfer(ref WS_PlayerData.CharacterObject objCharacter)
    {
        if (!objCharacter.WardenData.Ready)
        {
            throw new ApplicationException("Maiev.mod not ready!");
        }
        FileStream file = new($"warden\\{WorldServiceLocator._WS_Warden.Maiev.ModuleName}.bin", FileMode.Open, FileAccess.Read);
        checked
        {
            int size;
            for (size = (int)file.Length; size > 500; size -= 500)
            {
                Packets.PacketClass r = new(Opcodes.SMSG_WARDEN_DATA);
                r.AddInt8(1);
                r.AddInt16(500);
                var i = 1;
                do
                {
                    r.AddInt8((byte)file.ReadByte());
                    i++;
                }
                while (i <= 500);
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_WARDEN_DATA [data]", objCharacter.client.IP, objCharacter.client.Port);
                WorldServiceLocator._WS_Warden.SendWardenPacket(ref objCharacter, ref r);
            }
            if (size > 0)
            {
                Packets.PacketClass r2 = new(Opcodes.SMSG_WARDEN_DATA);
                r2.AddInt8(1);
                r2.AddUInt16((ushort)size);
                var num = size;
                for (var j = 1; j <= num; j++)
                {
                    r2.AddInt8((byte)file.ReadByte());
                }
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_WARDEN_DATA [done]", objCharacter.client.IP, objCharacter.client.Port);
                WorldServiceLocator._WS_Warden.SendWardenPacket(ref objCharacter, ref r2);
            }
        }
    }

    public void MaievSendUnk(ref WS_PlayerData.CharacterObject objCharacter)
    {
        Packets.PacketClass unk = new(Opcodes.SMSG_WARDEN_DATA);
        try
        {
            unk.AddInt8(3);
            unk.AddByteArray(new byte[56]
            {
                    20,
                    0,
                    96,
                    208,
                    254,
                    44,
                    1,
                    0,
                    2,
                    0,
                    32,
                    26,
                    54,
                    0,
                    192,
                    227,
                    53,
                    0,
                    80,
                    241,
                    53,
                    0,
                    192,
                    245,
                    53,
                    0,
                    3,
                    8,
                    0,
                    119,
                    108,
                    147,
                    169,
                    4,
                    0,
                    0,
                    96,
                    168,
                    64,
                    0,
                    1,
                    3,
                    8,
                    0,
                    54,
                    133,
                    234,
                    240,
                    1,
                    1,
                    0,
                    144,
                    244,
                    69,
                    0,
                    1
            });
            WorldServiceLocator._WS_Warden.SendWardenPacket(ref objCharacter, ref unk);
        }
        finally
        {
            unk.Dispose();
        }
    }

    public void MaievSendCheck(ref WS_PlayerData.CharacterObject objCharacter)
    {
        if (!objCharacter.WardenData.Ready)
        {
            throw new ApplicationException("Maiev.mod not ready!");
        }
        objCharacter.WardenData.Scan.Do_TIMING_CHECK();
        var packet = objCharacter.WardenData.Scan.GetPacket();
        try
        {
            WorldServiceLocator._WS_Warden.SendWardenPacket(ref objCharacter, ref packet);
        }
        finally
        {
            packet.Dispose();
        }
    }

    public void MaievSendSeed(ref WS_PlayerData.CharacterObject objCharacter)
    {
        Packets.PacketClass r = new(Opcodes.SMSG_WARDEN_DATA);
        r.AddInt8(5);
        r.AddByteArray(objCharacter.WardenData.Seed);
        WorldServiceLocator._WS_Warden.SendWardenPacket(ref objCharacter, ref r);
    }

    public void MaievResult(ref WS_PlayerData.CharacterObject objCharacter, ref Packets.PacketClass Packet)
    {
        var bufLen = Packet.GetUInt16();
        var checkSum = Packet.GetUInt32();
        var tmpOffset = Packet.Offset;
        var data = Packet.GetByteArray();
        Packet.Offset = tmpOffset;
        if (!ControlChecksum(checkSum, data))
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "[WARDEN] Failed checkSum at result packet. Cheater?");
            objCharacter.CommandResponse("[WARDEN] Pack your bags cheater, you're going!");
            return;
        }
        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[WARDEN] Result bufLen:{0} checkSum:{1:X}", bufLen, checkSum);
        objCharacter.WardenData.Scan.HandleResponse(ref Packet);
    }

    public bool ControlChecksum(uint checkSum, byte[] data)
    {
        SHA1Managed sha1 = new();
        var hash = sha1.ComputeHash(data);
        var ints = new uint[5];
        var i = 0;
        checked
        {
            do
            {
                ints[i] = BitConverter.ToUInt32(hash, i * 4);
                i++;
            }
            while (i <= 4);
            var ourCheckSum = ints[0] ^ ints[1] ^ ints[2] ^ ints[3] ^ ints[4];
            return checkSum == ourCheckSum;
        }
    }

    private byte[] FixWardenSHA(byte[] hash)
    {
        checked
        {
            var num = hash.Length - 1;
            for (var i = 0; i <= num; i += 4)
            {
                var tmp = hash[i + 3];
                hash[i + 3] = hash[i];
                hash[i] = tmp;
                tmp = hash[i + 2];
                hash[i + 2] = hash[i + 1];
                hash[i + 1] = tmp;
            }
            return hash;
        }
    }
}
