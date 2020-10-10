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
using System.Security.Cryptography;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Warden;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Player;
using Mangos.World.Server;
using Mangos.World.Warden;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Handlers
{
    public class WS_Handlers_Warden
    {
        private readonly int OutKeyAdr;
        private readonly int InKeyAdr;

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public void On_CMSG_WARDEN_DATA(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            // START Warden Decryption
            var b = new byte[(packet.Data.Length - 6)];
            Buffer.BlockCopy(packet.Data, 6, b, 0, b.Length);
            RC4.Crypt(ref b, client.Character.WardenData.KeyOut);
            Buffer.BlockCopy(b, 0, packet.Data, 6, b.Length);
            // END

            packet.GetInt16();
            MaievResponse Response = (MaievResponse)packet.GetInt8();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_WARDEN_DATA [{2}]", client.IP, client.Port, Response);
            if (client.Character.WardenData.Ready)
            {
                switch (Response)
                {
                    case var @case when @case == MaievResponse.MAIEV_RESPONSE_FAILED_OR_MISSING:
                        {
                            MaievSendTransfer(ref client.Character);
                            break;
                        }

                    case var case1 when case1 == MaievResponse.MAIEV_RESPONSE_SUCCESS:
                        {
                            MaievSendSeed(ref client.Character);
                            break;
                        }

                    case var case2 when case2 == MaievResponse.MAIEV_RESPONSE_RESULT:
                        {
                            MaievResult(ref client.Character, ref packet);
                            break;
                        }

                    case var case3 when case3 == MaievResponse.MAIEV_RESPONSE_HASH:
                        {
                            var hash = new byte[20];
                            Buffer.BlockCopy(packet.Data, packet.Offset, hash, 0, 20);

                            // TODO: Only one character can do this at the same time

                            WorldServiceLocator._WS_Warden.Maiev.GenerateNewRC4Keys(client.Character.WardenData.K);
                            var PacketData = new byte[17];
                            PacketData[0] = (byte)MaievOpcode.MAIEV_MODULE_SEED;
                            Buffer.BlockCopy(client.Character.WardenData.Seed, 0, PacketData, 1, 16);
                            int HandledBytes = WorldServiceLocator._WS_Warden.Maiev.HandlePacket(PacketData);
                            if (HandledBytes <= 0)
                            {
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "[WARDEN] Failed to handle 0x05 packet.");
                                return;
                            }

                            var thePacket = WorldServiceLocator._WS_Warden.Maiev.ReadPacket();
                            var ourHash = new byte[20];
                            Array.Copy(thePacket, 1, ourHash, 0, ourHash.Length);
                            WorldServiceLocator._WS_Warden.Maiev.ReadXorByte(ref client.Character);
                            WorldServiceLocator._WS_Warden.Maiev.ReadKeys(ref client.Character);
                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[WARDEN] XorByte: {0}", client.Character.WardenData.xorByte);
                            bool HashCorrect = true;
                            for (int i = 0; i <= 19; i++)
                            {
                                if (hash[i] != ourHash[i])
                                {
                                    HashCorrect = false;
                                    break;
                                }
                            }

                            if (!HashCorrect)
                            {
                                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "[WARDEN] Hashes in packet 0x05 didn't match. Cheater?");
                            }
                            else
                            {
                                // MaievSendUnk(Client.Character)
                            }

                            break;
                        }
                }
            }
        }

        public void MaievInit(ref WS_PlayerData.CharacterObject objCharacter)
        {
            byte[] k = WorldServiceLocator._WorldServer.ClsWorldServer.Cluster.ClientGetCryptKey(objCharacter.client.Index);
            var m = new MaievData(k);
            var seedOut = m.GetBytes(16);
            var seedIn = m.GetBytes(16);
            objCharacter.WardenData.KeyOut = RC4.Init(seedOut);
            objCharacter.WardenData.KeyIn = RC4.Init(seedIn);
            objCharacter.WardenData.Ready = true;
            objCharacter.WardenData.Scan = new WS_Warden.WardenScan(ref objCharacter);
            objCharacter.WardenData.xorByte = 0;
            objCharacter.WardenData.K = k;
            WorldServiceLocator._Functions.RAND_bytes(ref objCharacter.WardenData.Seed, 16);

            // Sending our test module
            MaievSendModule(ref objCharacter);
        }

        public void MaievSendModule(ref WS_PlayerData.CharacterObject objCharacter)
        {
            if (!objCharacter.WardenData.Ready)
                throw new ApplicationException("Maiev.mod not ready!");
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_WARDEN_DATA [{2}]", objCharacter.client.IP, objCharacter.client.Port, WorldServiceLocator._WS_Warden.Maiev.ModuleName);
            var r = new Packets.PacketClass(OPCODES.SMSG_WARDEN_DATA);
            r.AddInt8((byte)MaievOpcode.MAIEV_MODULE_INFORMATION);     // Opcode
            r.AddByteArray(WorldServiceLocator._WS_Warden.Maiev.WardenModule);                  // MD5 checksum of the modules compressed encrypted data
            r.AddByteArray(WorldServiceLocator._WS_Warden.Maiev.ModuleKey);                     // RC4 seed for decryption of the module
            r.AddUInt32((uint)WorldServiceLocator._WS_Warden.Maiev.ModuleSize);                       // Module Compressed Length - Size of the packet
            WorldServiceLocator._WS_Warden.SendWardenPacket(ref objCharacter, ref r);
        }

        public void MaievSendTransfer(ref WS_PlayerData.CharacterObject objCharacter)
        {
            if (!objCharacter.WardenData.Ready)
                throw new ApplicationException("Maiev.mod not ready!");
            var file = new System.IO.FileStream(string.Format(@"warden\{0}.bin", WorldServiceLocator._WS_Warden.Maiev.ModuleName), System.IO.FileMode.Open, System.IO.FileAccess.Read);
            int size = (int)file.Length;
            while (size > 500)
            {
                var r = new Packets.PacketClass(OPCODES.SMSG_WARDEN_DATA);
                r.AddInt8((byte)MaievOpcode.MAIEV_MODULE_TRANSFER);                // Opcode
                r.AddInt16(500);                                             // Payload Length
                for (int i = 1; i <= 500; i++)                                 // Payload
                    r.AddInt8((byte)file.ReadByte());
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_WARDEN_DATA [data]", objCharacter.client.IP, objCharacter.client.Port);
                // DumpPacket(r.Data, objCharacter.Client, 4)

                WorldServiceLocator._WS_Warden.SendWardenPacket(ref objCharacter, ref r);
                size -= 500;
            }

            if (size > 0)
            {
                var r = new Packets.PacketClass(OPCODES.SMSG_WARDEN_DATA);
                r.AddInt8((byte)MaievOpcode.MAIEV_MODULE_TRANSFER);                // Opcode
                r.AddUInt16((ushort)size);                                           // Payload Length
                for (int i = 1, loopTo = size; i <= loopTo; i++)                                // Payload
                    r.AddInt8((byte)file.ReadByte());
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_WARDEN_DATA [done]", objCharacter.client.IP, objCharacter.client.Port);
                // DumpPacket(r.Data, objCharacter.Client, 4)

                WorldServiceLocator._WS_Warden.SendWardenPacket(ref objCharacter, ref r);
            }
        }

        public void MaievSendUnk(ref WS_PlayerData.CharacterObject objCharacter)
        {
            var unk = new Packets.PacketClass(OPCODES.SMSG_WARDEN_DATA);
            try
            {
                unk.AddInt8((byte)MaievOpcode.MAIEV_MODULE_UNK);
                unk.AddByteArray(new byte[] { 0x14, 0x0, 0x60, 0xD0, 0xFE, 0x2C, 0x1, 0x0, 0x2, 0x0, 0x20, 0x1A, 0x36, 0x0, 0xC0, 0xE3, 0x35, 0x0, 0x50, 0xF1, 0x35, 0x0, 0xC0, 0xF5, 0x35, 0x0, 0x3, 0x8, 0x0, 0x77, 0x6C, 0x93, 0xA9, 0x4, 0x0, 0x0, 0x60, 0xA8, 0x40, 0x0, 0x1, 0x3, 0x8, 0x0, 0x36, 0x85, 0xEA, 0xF0, 0x1, 0x1, 0x0, 0x90, 0xF4, 0x45, 0x0, 0x1 });
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
                throw new ApplicationException("Maiev.mod not ready!");
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
            var r = new Packets.PacketClass(OPCODES.SMSG_WARDEN_DATA);
            r.AddInt8((byte)MaievOpcode.MAIEV_MODULE_SEED);
            r.AddByteArray(objCharacter.WardenData.Seed);
            WorldServiceLocator._WS_Warden.SendWardenPacket(ref objCharacter, ref r);
        }

        public void MaievResult(ref WS_PlayerData.CharacterObject objCharacter, ref Packets.PacketClass Packet)
        {
            ushort bufLen = Packet.GetUInt16();
            uint checkSum = Packet.GetUInt32();

            // DONE: Make sure the checkSum is correct
            int tmpOffset = Packet.Offset;
            var data = Packet.GetByteArray();
            Packet.Offset = tmpOffset;
            if (ControlChecksum(checkSum, data) == false)
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
            var sha1 = new SHA1Managed();
            var hash = sha1.ComputeHash(data);
            var ints = new uint[5];
            for (int i = 0; i <= 4; i++)
                ints[i] = BitConverter.ToUInt32(hash, i * 4);
            uint ourCheckSum = ints[0] ^ ints[1] ^ ints[2] ^ ints[3] ^ ints[4];
            return checkSum == ourCheckSum;
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia *//* TODO ERROR: Skipped RegionDirectiveTrivia */
        public class WardenData
        {
            public byte Failed = 0;
            public bool Ready = false;
            public byte[] KeyOut = null;
            public byte[] KeyIn = null;
            public byte[] Seed = null;
            public byte[] K = null;
            public byte[] ClientSeed = null;
            public byte xorByte = 0;
            public WS_Warden.WardenScan Scan = null;
        }

        public class MaievData
        {
            public int index = 0;
            public byte[] source1;
            public byte[] source2;
            public byte[] data = new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            public MaievData(byte[] seed)
            {
                // Initialization
                var sha1 = new SHA1Managed();
                source1 = sha1.ComputeHash(seed, 0, 20);
                source2 = sha1.ComputeHash(seed, 20, 20);
                Update();
            }

            public void Update()
            {
                var buffer1 = new byte[60];
                var sha1 = new SHA1Managed();
                Buffer.BlockCopy(source1, 0, buffer1, 0, 20);
                Buffer.BlockCopy(data, 0, buffer1, 20, 20);
                Buffer.BlockCopy(source2, 0, buffer1, 40, 20);
                data = sha1.ComputeHash(buffer1);
            }

            private byte GetByte()
            {
                byte r = data[index];
                index += 1;
                if (index >= 0x14)
                {
                    Update();
                    index = 0;
                }

                return r;
            }

            public byte[] GetBytes(int count)
            {
                var b = new byte[count];
                for (int i = 0, loopTo = count - 1; i <= loopTo; i++)
                    b[i] = GetByte();
                return b;
            }
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia *//* TODO ERROR: Skipped RegionDirectiveTrivia */
        public class RC4
        {
            // http://www.skullsecurity.org/wiki/index.php/Crypto_and_Hashing

            public static byte[] Init(byte[] @base)
            {
                int val = 0;
                int position = 0;
                byte temp;
                var key = new byte[258];
                for (int i = 0; i <= 256 - 1; i++)
                    key[i] = (byte)i;
                key[256] = 0;
                key[257] = 0;
                for (int i = 1; i <= 64; i++)
                {
                    val = val + key[i * 4 - 4] + @base[position % @base.Length];
                    val = val & 0xFF;
                    position += 1;
                    temp = key[i * 4 - 4];
                    key[i * 4 - 4] = key[val & 0xFF];
                    key[val & 0xFF] = temp;
                    val = val + key[i * 4 - 3] + @base[position % @base.Length];
                    val = val & 0xFF;
                    position += 1;
                    temp = key[i * 4 - 3];
                    key[i * 4 - 3] = key[val & 0xFF];
                    key[val & 0xFF] = temp;
                    val = val + key[i * 4 - 2] + @base[position % @base.Length];
                    val = val & 0xFF;
                    position += 1;
                    temp = key[i * 4 - 2];
                    key[i * 4 - 2] = key[val & 0xFF];
                    key[val & 0xFF] = temp;
                    val = val + key[i * 4 - 1] + @base[position % @base.Length];
                    val = val & 0xFF;
                    position += 1;
                    temp = key[i * 4 - 1];
                    key[i * 4 - 1] = key[val & 0xFF];
                    key[val & 0xFF] = temp;
                }

                return key;
            }

            public static void Crypt(ref byte[] data, byte[] key)
            {
                byte temp;
                for (int i = 0, loopTo = data.Length - 1; i <= loopTo; i++)
                {
                    key[256] = (byte)(key[256] + 1 & 0xFF);
                    key[257] = (byte)(key[257] + Conversions.ToInteger(key[key[256]]) & 0xFF);
                    temp = key[key[257] & 0xFF];
                    key[key[257]] = key[key[256]];
                    key[key[256]] = temp;
                    data[i] = (byte)(data[i] ^ key[key[key[257]] + Conversions.ToInteger(key[key[256]]) & 0xFF]);
                }
            }
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia *//* TODO ERROR: Skipped RegionDirectiveTrivia */
        private byte[] FixWardenSHA(byte[] hash)
        {
            for (int i = 0, loopTo = hash.Length - 1; i <= loopTo; i += 4)
            {
                byte tmp = hash[i + 3];
                hash[i + 3] = hash[i];
                hash[i] = tmp;
                tmp = hash[i + 2];
                hash[i + 2] = hash[i + 1];
                hash[i + 1] = tmp;
            }

            return hash;
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
    }
}