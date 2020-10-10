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
using System.Data;
using System.Net.Sockets;
using System.Threading;
using Mangos.Cluster.Globals;
using Mangos.Cluster.Server;
using Mangos.Common.Enums.Authentication;
using Mangos.Common.Enums.Character;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Misc;
using Mangos.Common.Enums.Player;
using Mangos.Common.Globals;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.Cluster.Handlers
{
    public class WC_Handlers_Auth
    {
        private const int REQUIRED_BUILD_LOW = 5875; // 1.12.1
        private const int REQUIRED_BUILD_HIGH = 6141;

        public void SendLoginOk(WC_Network.ClientClass client)
        {
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AUTH_SESSION [{2}]", client.IP, client.Port, client.Account);
            Thread.Sleep(500);
            var response = new Packets.PacketClass(OPCODES.SMSG_AUTH_RESPONSE);
            response.AddInt8((byte)LoginResponse.LOGIN_OK);
            response.AddInt32(0);
            response.AddInt8(2); // BillingPlanFlags
            response.AddUInt32(0U); // BillingTimeRested
            client.Send(response);
        }

        public void On_CMSG_AUTH_SESSION(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            // _WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}] [{1}:{2}] CMSG_AUTH_SESSION", Format(TimeOfDay, "hh:mm:ss"), client.IP, client.Port)

            packet.GetInt16();
            int clientVersion = packet.GetInt32();
            int clientSessionID = packet.GetInt32();
            string clientAccount = packet.GetString();
            int clientSeed = packet.GetInt32();
            var clientHash = new byte[20];
            for (int i = 0; i <= 19; i++)
                clientHash[i] = packet.GetInt8();
            int clientAddOnsSize = packet.GetInt32();

            // DONE: Set client.Account
            string tmp = clientAccount;

            // DONE: Kick if existing
            foreach (KeyValuePair<uint, WC_Network.ClientClass> tmpClientEntry in ClusterServiceLocator._WorldCluster.CLIENTs)
            {
                if (tmpClientEntry.Value is object)
                {
                    if (tmpClientEntry.Value.Account == tmp)
                    {
                        if (tmpClientEntry.Value.Character is object)
                        {
                            tmpClientEntry.Value.Character.Dispose();
                            tmpClientEntry.Value.Character = null;
                        }

                        try
                        {
                            tmpClientEntry.Value.Socket.Shutdown(SocketShutdown.Both);
                        }
                        catch
                        {
                            tmpClientEntry.Value.Socket.Close();
                        }
                    }
                }
            }

            client.Account = tmp;

            // DONE: Set client.SS_Hash
            var result = new DataTable();
            string query;
            query = "SELECT sessionkey, gmlevel FROM account WHERE username = '" + client.Account + "';";
            ClusterServiceLocator._WorldCluster.AccountDatabase.Query(query, ref result);
            if (result.Rows.Count > 0)
            {
                tmp = Conversions.ToString(result.Rows[0]["sessionkey"]);
                client.Access = (AccessLevel)result.Rows[0]["gmlevel"];
            }
            else
            {
                ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.USER, "[{0}:{1}] AUTH_UNKNOWN_ACCOUNT: Account not in DB!", client.IP, client.Port);
                var response_unk_acc = new Packets.PacketClass(OPCODES.SMSG_AUTH_RESPONSE);
                response_unk_acc.AddInt8((byte)AuthResult.WOW_FAIL_UNKNOWN_ACCOUNT);
                client.Send(response_unk_acc);
                return;
            }

            client.SS_Hash = new byte[40];
            for (int i = 0, loopTo = Strings.Len(tmp) - 1; i <= loopTo; i += 2)
                client.SS_Hash[i / 2] = (byte)Conversion.Val("&H" + Strings.Mid(tmp, i + 1, 2));
            client.Encryption = true;

            // DONE: Disconnect clients trying to enter with an invalid build
            if (clientVersion < REQUIRED_BUILD_LOW || clientVersion > REQUIRED_BUILD_HIGH)
            {
                var invalid_version = new Packets.PacketClass(OPCODES.SMSG_AUTH_RESPONSE);
                invalid_version.AddInt8((byte)AuthResult.WOW_FAIL_VERSION_INVALID);
                client.Send(invalid_version);
                return;
            }

            // TODO: Make sure the correct client connected
            // Dim temp() As Byte = System.Text.Encoding.ASCII.GetBytes(clientAccount)
            // temp = Concat(temp, BitConverter.GetBytes(0))
            // temp = Concat(temp, BitConverter.GetBytes(clientSeed))
            // temp = Concat(temp, BitConverter.GetBytes(client.Index))
            // temp = Concat(temp, client.SS_Hash)
            // Dim ShaDigest() As Byte = New System.Security.Cryptography.SHA1Managed().ComputeHash(temp)
            // _WorldCluster.Log.WriteLine(LogType.DEBUG, "Client Hash: {0}", BitConverter.ToString(clientHash).Replace("-", ""))
            // _WorldCluster.Log.WriteLine(LogType.DEBUG, "Server Hash: {0}", BitConverter.ToString(ShaDigest).Replace("-", ""))
            // For i As Integer = 0 To 19
            // If clientHash(i) <> ShaDigest(i) Then
            // Dim responseFail As New PacketClass(OPCODES.SMSG_AUTH_RESPONSE)
            // responseFail.AddInt8(AuthResponseCodes.AUTH_FAILED)
            // client.Send(responseFail)
            // Exit Sub
            // End If
            // Next

            // DONE: If server full then queue, If GM/Admin let in
            if (ClusterServiceLocator._WorldCluster.CLIENTs.Count > ClusterServiceLocator._WorldCluster.Config.ServerPlayerLimit & client.Access <= AccessLevel.Player)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(client.EnQueue));
            }
            else
            {
                SendLoginOk(client);
            }

            // DONE: Addons info reading
            var decompressBuffer = new byte[packet.Data.Length - packet.Offset + 1];
            Array.Copy(packet.Data, packet.Offset, decompressBuffer, 0, packet.Data.Length - packet.Offset);
            packet.Data = ClusterServiceLocator._GlobalZip.DeCompress(decompressBuffer);
            packet.Offset = 0;
            // DumpPacket(packet.Data)

            var AddOnsNames = new List<string>();
            var AddOnsHashes = new List<uint>();
            // Dim AddOnsConsoleWrite As String = String.Format("[{0}:{1}] Client addons loaded:", client.IP, client.Port)
            while (packet.Offset < clientAddOnsSize)
            {
                AddOnsNames.Add(packet.GetString());
                AddOnsHashes.Add(packet.GetUInt32());
                packet.GetInt32(); // Unk7
                packet.GetInt8(); // Unk6
                // AddOnsConsoleWrite &= String.Format("{0}{1} AddOnName: [{2,-30}], AddOnHash: [{3:X}]", vbCrLf, vbTab, AddOnsNames(AddOnsNames.Count - 1), AddOnsHashes(AddOnsHashes.Count - 1))
            }
            // _WorldCluster.Log.WriteLine(LogType.DEBUG, AddOnsConsoleWrite)

            // DONE: Build mysql addons query
            // Not needed already - in 1.11 addons list is removed.

            // DONE: Send packet
            var addOnsEnable = new Packets.PacketClass(OPCODES.SMSG_ADDON_INFO);
            for (int i = 0, loopTo1 = AddOnsNames.Count - 1; i <= loopTo1; i++)
            {
                if (System.IO.File.Exists(string.Format(@"interface\{0}.pub", AddOnsNames[i])) && AddOnsHashes[i] != 0x1C776D01U)
                {
                    // We have hash data
                    addOnsEnable.AddInt8(2);                    // AddOn Type [1-enabled, 0-banned, 2-blizzard]
                    addOnsEnable.AddInt8(1);                    // Unk
                    var fs = new System.IO.FileStream(string.Format(@"interface\{0}.pub", AddOnsNames[i]), System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read, 258, System.IO.FileOptions.SequentialScan);
                    var fb = new byte[257];
                    fs.Read(fb, 0, 257);

                    // NOTE: Read from file
                    addOnsEnable.AddByteArray(fb);
                    addOnsEnable.AddInt32(0);
                    addOnsEnable.AddInt8(0);
                }
                else
                {
                    // We don't have hash data or already sent to client
                    addOnsEnable.AddInt8(2);                    // AddOn Type [1-enabled, 0-banned, 2-blizzard]
                    addOnsEnable.AddInt8(1);                    // Unk
                    addOnsEnable.AddInt32(0);
                    addOnsEnable.AddInt16(0);
                }
            }

            client.Send(addOnsEnable);
            addOnsEnable.Dispose();
        }

        public void On_CMSG_PING(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 9)
                return;
            packet.GetInt16();
            var response = new Packets.PacketClass(OPCODES.SMSG_PONG);
            response.AddInt32(packet.GetInt32());
            client.Send(response);
            if (client.Character is object)
            {
                client.Character.Latency = packet.GetInt32();
            }

            // _WorldCluster.Log.WriteLine(LogType.NETWORK, "[{0}:{1}] SMSG_PONG [{2}]", client.IP, client.Port, client.Character.Latency)
        }

        public void On_CMSG_UPDATE_ACCOUNT_DATA(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            try
            {
                if (packet.Data.Length - 1 < 13)
                    return;
                packet.GetInt16();
                uint DataID = packet.GetUInt32();
                uint UncompressedSize = packet.GetUInt32();
                ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_UPDATE_ACCOUNT_DATA [ID={2} Size={3}]", client.IP, client.Port, DataID, UncompressedSize);
                if (DataID > 7L)
                    return;

                // TODO: How does Mangos Zero Handle the Account Data For the Character?
                // Dim AccData As New DataTable
                // _WorldCluster.AccountDatabase.Query(String.Format("SELECT account_id FROM accounts WHERE username = ""{0}"";", client.Account), AccData)
                // If AccData.Rows.Count = 0 Then
                // _WorldCluster.Log.WriteLine(LogType.WARNING, "[{0}:{1}] CMSG_UPDATE_ACCOUNT_DATA [Account ID not found]", client.IP, client.Port)
                // Exit Sub
                // End If

                // Dim AccID As Integer = CType(AccData.Rows(0).Item("account_id"), Integer)
                // AccData.Clear()

                // DONE: Clear the entry
                // If UncompressedSize = 0 Then
                // _WorldCluster.AccountDatabase.Update(String.Format("UPDATE `account_data` SET `account_data{0}`='' WHERE `account_id`={1}", DataID, AccID))
                // Exit Sub
                // End If

                // DONE: Can not handle more than 65534 bytes
                // If UncompressedSize >= 65534 Then
                // _WorldCluster.Log.WriteLine(LogType.WARNING, "[{0}:{1}] CMSG_UPDATE_ACCOUNT_DATA [Invalid uncompressed size]", client.IP, client.Port)
                // Exit Sub
                // End If

                int ReceivedPacketSize = packet.Data.Length - packet.Offset;
            }
            // Dim dataStr As String
            // DONE: Check if it's compressed, if so, decompress it
            // If UncompressedSize > ReceivedPacketSize Then
            // Dim compressedBuffer(ReceivedPacketSize - 1) As Byte
            // Array.Copy(packet.Data, packet.Offset, compressedBuffer, 0, compressedBuffer.Length)
            // 
            // dataStr = ToHex(DeCompress(compressedBuffer))
            // Else
            // dataStr = ToHex(packet.Data, packet.Offset)
            // End If

            // _WorldCluster.AccountDatabase.Update(String.Format("UPDATE `account_data` SET `account_data{0}`={2} WHERE `account_id`={1};", DataID, AccID, dataStr))

            catch (Exception e)
            {
                ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.FAILED, "Error while updating account data.{0}", Constants.vbCrLf + e.ToString());
            }
        }

        public void On_CMSG_REQUEST_ACCOUNT_DATA(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 9)
                return;
            packet.GetInt16();
            uint DataID = packet.GetUInt32();
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_REQUEST_ACCOUNT_DATA [ID={2}]", client.IP, client.Port, DataID);
            if (DataID > 7L)
                return;

            // Dim AccData As New DataTable
            // _WorldCluster.AccountDatabase.Query(String.Format("SELECT account_id FROM accounts WHERE username = ""{0}"";", client.Account), AccData)
            // If AccData.Rows.Count > 0 Then
            // Dim AccID As Integer = CType(AccData.Rows(0).Item("account_id"), Integer)
            // 
            // AccData.Clear()
            // _WorldCluster.AccountDatabase.Query(String.Format("SELECT `account_data{1}` FROM account_data WHERE account_id = {0}", AccID, DataID), AccData)
            // If AccData.Rows.Count > 0 Then FoundData = True
            // End If

            var response = new Packets.PacketClass(OPCODES.SMSG_UPDATE_ACCOUNT_DATA);
            response.AddUInt32(DataID);

            // If FoundData = False Then
            response.AddInt32(0); // Uncompressed buffer length
            // Else
            // Dim AccountData() As Byte = AccData.Rows(0).Item("account_data" & DataID)
            // If AccountData.Length > 0 Then
            // response.AddInt32(AccountData.Length) 'Uncompressed buffer length
            // DONE: Compress buffer if it's longer than 200 bytes
            // If AccountData.Length > 200 Then
            // Dim CompressedBuffer() As Byte = Compress(AccountData, 0, AccountData.Length)
            // response.AddByteArray(CompressedBuffer)
            // Else
            // response.AddByteArray(AccountData)
            // End If
            // Else
            // response.AddInt32(0) 'Uncompressed buffer length
            // End If
            // End If

            client.Send(response);
            response.Dispose();
        }

        public void On_CMSG_CHAR_ENUM(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHAR_ENUM", client.IP, client.Port);

            // DONE: Query _WorldCluster.CHARACTERs DB
            var response = new Packets.PacketClass(OPCODES.SMSG_CHAR_ENUM);
            var MySQLQuery = new DataTable();
            int Account_ID;
            try
            {
                ClusterServiceLocator._WorldCluster.AccountDatabase.Query(string.Format("SELECT id FROM account WHERE username = '{0}';", client.Account), ref MySQLQuery);
                Account_ID = Conversions.ToInteger(MySQLQuery.Rows[0]["id"]);
                MySQLQuery.Clear();
                ClusterServiceLocator._WorldCluster.CharacterDatabase.Query(string.Format("SELECT * FROM characters WHERE account_id = '{0}' ORDER BY char_guid;", Account_ID), ref MySQLQuery);

                // DONE: Make The Packet
                response.AddInt8((byte)MySQLQuery.Rows.Count);
                for (int i = 0, loopTo = MySQLQuery.Rows.Count - 1; i <= loopTo; i++)
                {
                    bool DEAD = false;
                    var DeadMySQLQuery = new DataTable();
                    ClusterServiceLocator._WorldCluster.CharacterDatabase.Query(string.Format("SELECT COUNT(*) FROM corpse WHERE player = {0};", MySQLQuery.Rows[i]["char_guid"]), ref DeadMySQLQuery);
                    if (Conversions.ToInteger(DeadMySQLQuery.Rows[0][0]) > 0)
                        DEAD = true;
                    var PetQuery = new DataTable();
                    ClusterServiceLocator._WorldCluster.CharacterDatabase.Query(string.Format("SELECT modelid, level, entry FROM character_pet WHERE owner = '{0}';", MySQLQuery.Rows[i]["char_guid"]), ref PetQuery);
                    response.AddInt64(Conversions.ToLong(MySQLQuery.Rows[i]["char_guid"]));
                    response.AddString(Conversions.ToString(MySQLQuery.Rows[i]["char_name"]));
                    response.AddInt8(Conversions.ToByte(MySQLQuery.Rows[i]["char_race"]));
                    response.AddInt8(Conversions.ToByte(MySQLQuery.Rows[i]["char_class"]));
                    response.AddInt8(Conversions.ToByte(MySQLQuery.Rows[i]["char_gender"]));
                    response.AddInt8(Conversions.ToByte(MySQLQuery.Rows[i]["char_skin"]));
                    response.AddInt8(Conversions.ToByte(MySQLQuery.Rows[i]["char_face"]));
                    response.AddInt8(Conversions.ToByte(MySQLQuery.Rows[i]["char_hairStyle"]));
                    response.AddInt8(Conversions.ToByte(MySQLQuery.Rows[i]["char_hairColor"]));
                    response.AddInt8(Conversions.ToByte(MySQLQuery.Rows[i]["char_facialHair"]));
                    response.AddInt8(Conversions.ToByte(MySQLQuery.Rows[i]["char_level"]));
                    response.AddInt32(Conversions.ToInteger(MySQLQuery.Rows[i]["char_zone_id"]));
                    response.AddInt32(Conversions.ToInteger(MySQLQuery.Rows[i]["char_map_id"]));
                    response.AddSingle(Conversions.ToSingle(MySQLQuery.Rows[i]["char_positionX"]));
                    response.AddSingle(Conversions.ToSingle(MySQLQuery.Rows[i]["char_positionY"]));
                    response.AddSingle(Conversions.ToSingle(MySQLQuery.Rows[i]["char_positionZ"]));
                    response.AddInt32(Conversions.ToInteger(MySQLQuery.Rows[i]["char_guildId"]));
                    uint playerState = (uint)CharacterFlagState.CHARACTER_FLAG_NONE;
                    uint ForceRestrictions = Conversions.ToUInteger(MySQLQuery.Rows[i]["force_restrictions"]);
                    if ((ForceRestrictions & (uint)ForceRestrictionFlags.RESTRICT_TRANSFER) != 0)
                    {
                        playerState += (uint)CharacterFlagState.CHARACTER_FLAG_LOCKED_FOR_TRANSFER;
                    }

                    if ((ForceRestrictions & (uint)ForceRestrictionFlags.RESTRICT_BILLING) != 0)
                    {
                        playerState += (uint)CharacterFlagState.CHARACTER_FLAG_LOCKED_BY_BILLING;
                    }

                    if ((ForceRestrictions & (uint)ForceRestrictionFlags.RESTRICT_RENAME) != 0)
                    {
                        playerState += (uint)CharacterFlagState.CHARACTER_FLAG_RENAME;
                    }

                    if (DEAD)
                    {
                        playerState += (uint)CharacterFlagState.CHARACTER_FLAG_GHOST;
                    }

                    response.AddUInt32(playerState);
                    response.AddInt8(Conversions.ToByte(MySQLQuery.Rows[i]["char_restState"]));
                    int PetModel = 0;
                    int PetLevel = 0;
                    int PetFamily = 0;
                    if (PetQuery.Rows.Count > 0)
                    {
                        PetModel = Conversions.ToInteger(PetQuery.Rows[0]["modelid"]);
                        PetLevel = Conversions.ToInteger(PetQuery.Rows[0]["level"]);
                        var PetFamilyQuery = new DataTable();
                        ClusterServiceLocator._WorldCluster.WorldDatabase.Query(string.Format("SELECT family FROM creature_template WHERE entry = '{0}'", PetQuery.Rows[0]["entry"]), ref PetFamilyQuery);
                        PetFamily = Conversions.ToInteger(PetFamilyQuery.Rows[0]["family"]);
                    }

                    response.AddInt32(PetModel);
                    response.AddInt32(PetLevel);
                    response.AddInt32(PetFamily);

                    // DONE: Get items
                    long GUID = Conversions.ToLong(MySQLQuery.Rows[i]["char_guid"]);
                    var ItemsMySQLQuery = new DataTable();
                    string characterDB = ClusterServiceLocator._WorldCluster.CharacterDatabase.SQLDBName;
                    string worldDB = ClusterServiceLocator._WorldCluster.WorldDatabase.SQLDBName;
                    ClusterServiceLocator._WorldCluster.CharacterDatabase.Query(string.Format("SELECT item_slot, displayid, inventorytype FROM " + characterDB + ".characters_inventory, " + worldDB + ".item_template WHERE item_bag = {0} AND item_slot <> 255 AND entry = item_id  ORDER BY item_slot;", GUID), ref ItemsMySQLQuery);
                    var e = ItemsMySQLQuery.Rows.GetEnumerator();
                    e.Reset();
                    e.MoveNext();
                    DataRow r = (DataRow)e.Current;

                    // DONE: Add model info
                    for (byte slot = 0, loopTo1 = (byte)EquipmentSlots.EQUIPMENT_SLOT_END; slot <= loopTo1; slot++) // - 1
                    {
                        if (r is null || Conversions.ToInteger(r["item_slot"]) != slot)
                        {
                            // No equiped item in this slot
                            response.AddInt32(0); // Item Model
                            response.AddInt8(0);  // Item Slot
                        }
                        else
                        {
                            // DONE: Do not show helmet or cloak
                            if (((ForceRestrictions & (uint)ForceRestrictionFlags.RESTRICT_HIDECLOAK) != 0) && (EquipmentSlots)Conversions.ToByte(r["item_slot"]) == EquipmentSlots.EQUIPMENT_SLOT_BACK || ((ForceRestrictions & (uint)ForceRestrictionFlags.RESTRICT_HIDEHELM) != 0) && (EquipmentSlots)Conversions.ToByte(r["item_slot"]) == EquipmentSlots.EQUIPMENT_SLOT_HEAD)
                            {
                                response.AddInt32(0); // Item Model
                                response.AddInt8(0);  // Item Slot
                            }
                            else
                            {
                                response.AddInt32(Conversions.ToInteger(r["displayid"]));          // Item Model
                                response.AddInt8(Conversions.ToByte(r["inventorytype"]));
                            }       // Item Slot

                            e.MoveNext();
                            r = (DataRow)e.Current;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.FAILED, "[{0}:{1}] Unable to enum characters. [{2}]", client.IP, client.Port, e.Message);
                // TODO: Find what opcode officials use
                response = new Packets.PacketClass(OPCODES.SMSG_CHAR_CREATE);
                response.AddInt8((byte)CharResponse.CHAR_LIST_FAILED);
            }

            client.Send(response);
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_CHAR_ENUM", client.IP, client.Port);
        }

        public void On_CMSG_CHAR_DELETE(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHAR_DELETE", client.IP, client.Port);
            var response = new Packets.PacketClass(OPCODES.SMSG_CHAR_DELETE);
            packet.GetInt16();
            ulong guid = packet.GetUInt64();
            try
            {
                var q = new DataTable();

                // Done: Fixed packet manipulation protection
                ClusterServiceLocator._WorldCluster.AccountDatabase.Query(string.Format("SELECT id FROM account WHERE username = \"{0}\";", client.Account), ref q);
                if (q.Rows.Count == 0)
                {
                    return;
                }

                ClusterServiceLocator._WorldCluster.CharacterDatabase.Query(string.Format("SELECT char_guid FROM characters WHERE account_id = \"{0}\" AND char_guid = \"{1}\";", q.Rows[0]["id"], guid), ref q);
                if (q.Rows.Count == 0)
                {
                    response.AddInt8((byte)AuthResult.WOW_FAIL_BANNED);
                    client.Send(response);
                    ClusterServiceLocator._Functions.Ban_Account(client.Account, "Packet Manipulation/Character Deletion");
                    client.Delete();
                    return;
                }

                q.Clear();
                ClusterServiceLocator._WorldCluster.CharacterDatabase.Query(string.Format("SELECT item_guid FROM characters_inventory WHERE item_bag = {0};", guid), ref q);
                foreach (DataRow row in q.Rows)
                {
                    // DONE: Delete items
                    ClusterServiceLocator._WorldCluster.CharacterDatabase.Update(string.Format("DELETE FROM characters_inventory WHERE item_guid = \"{0}\";", row["item_guid"]));
                    // DONE: Delete items in bags
                    ClusterServiceLocator._WorldCluster.CharacterDatabase.Update(string.Format("DELETE FROM characters_inventory WHERE item_bag = \"{0}\";", Conversions.ToULong(row["item_guid"]) + ClusterServiceLocator._Global_Constants.GUID_ITEM));
                }

                ClusterServiceLocator._WorldCluster.CharacterDatabase.Query(string.Format("SELECT item_guid FROM characters_inventory WHERE item_owner = {0};", guid), ref q);
                q.Clear();
                ClusterServiceLocator._WorldCluster.CharacterDatabase.Query(string.Format("SELECT mail_id FROM characters_mail WHERE mail_receiver = \"{0}\";", guid), ref q);
                foreach (DataRow row in q.Rows)
                {
                    // TODO: Return mails?
                    // DONE: Delete mails
                    ClusterServiceLocator._WorldCluster.CharacterDatabase.Update(string.Format("DELETE FROM characters_mail WHERE mail_id = \"{0}\";", row["mail_id"]));
                    // DONE: Delete mail items
                    ClusterServiceLocator._WorldCluster.CharacterDatabase.Update(string.Format("DELETE FROM mail_items WHERE mail_id = \"{0}\";", row["mail_id"]));
                }

                ClusterServiceLocator._WorldCluster.CharacterDatabase.Update(string.Format("DELETE FROM characters WHERE char_guid = \"{0}\";", guid));
                ClusterServiceLocator._WorldCluster.CharacterDatabase.Update(string.Format("DELETE FROM characters_honor WHERE char_guid = \"{0}\";", guid));
                ClusterServiceLocator._WorldCluster.CharacterDatabase.Update(string.Format("DELETE FROM characters_quests WHERE char_guid = \"{0}\";", guid));
                ClusterServiceLocator._WorldCluster.CharacterDatabase.Update(string.Format("DELETE FROM character_social WHERE guid = '{0}' OR friend = '{0}';", guid));
                ClusterServiceLocator._WorldCluster.CharacterDatabase.Update(string.Format("DELETE FROM characters_spells WHERE guid = \"{0}\";", guid));
                ClusterServiceLocator._WorldCluster.CharacterDatabase.Update(string.Format("DELETE FROM petitions WHERE petition_owner = \"{0}\";", guid));
                ClusterServiceLocator._WorldCluster.CharacterDatabase.Update(string.Format("DELETE FROM auctionhouse WHERE auction_owner = \"{0}\";", guid));
                ClusterServiceLocator._WorldCluster.CharacterDatabase.Update(string.Format("DELETE FROM characters_tickets WHERE char_guid = \"{0}\";", guid));
                ClusterServiceLocator._WorldCluster.CharacterDatabase.Update(string.Format("DELETE FROM corpse WHERE guid = \"{0}\";", guid));
                q.Clear();
                ClusterServiceLocator._WorldCluster.CharacterDatabase.Query(string.Format("SELECT guild_id FROM guilds WHERE guild_leader = \"{0}\";", guid), ref q);
                if (q.Rows.Count > 0)
                {
                    ClusterServiceLocator._WorldCluster.CharacterDatabase.Update(string.Format("UPDATE characters SET char_guildid = 0, char_guildrank = 0, char_guildpnote = '', charguildoffnote = '' WHERE char_guildid = \"{0}\";", q.Rows[0]["guild_id"]));
                    ClusterServiceLocator._WorldCluster.CharacterDatabase.Update(string.Format("DELETE FROM guild WHERE guild_id = \"{0}\";", q.Rows[0]["guild_id"]));
                }

                response.AddInt8((byte)CharResponse.CHAR_DELETE_SUCCESS); // Changed in 1.12.x client branch?
            }
            catch (Exception)
            {
                response.AddInt8((byte)CharResponse.CHAR_DELETE_FAILED);
            }

            client.Send(response);
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_CHAR_DELETE [{2:X}]", client.IP, client.Port, guid);
        }

        public void On_CMSG_CHAR_RENAME(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            packet.GetInt16();
            long GUID = packet.GetInt64();
            string Name = packet.GetString();
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHAR_RENAME [{2}:{3}]", client.IP, client.Port, GUID, Name);
            byte ErrCode = (byte)ATLoginFlags.AT_LOGIN_RENAME;

            // DONE: Check for existing name
            var q = new DataTable();
            ClusterServiceLocator._WorldCluster.CharacterDatabase.Query(string.Format("SELECT char_name FROM characters WHERE char_name LIKE \"{0}\";", Name), ref q);
            if (q.Rows.Count > 0)
            {
                ErrCode = (byte)CharResponse.CHAR_CREATE_NAME_IN_USE;
            }

            // DONE: Do the rename
            if (ErrCode == (byte)ATLoginFlags.AT_LOGIN_RENAME)
                ClusterServiceLocator._WorldCluster.CharacterDatabase.Update(string.Format("UPDATE characters SET char_name = \"{1}\", force_restrictions = 0 WHERE char_guid = {0};", GUID, Name));

            // DONE: Send response
            var response = new Packets.PacketClass(OPCODES.SMSG_CHAR_RENAME);
            response.AddInt8(ErrCode);
            client.Send(response);
            response.Dispose();
            Packets.PacketClass argpacket = null;
            On_CMSG_CHAR_ENUM(argpacket, client);
        }

        public void On_CMSG_CHAR_CREATE(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            packet.GetInt16();
            string Name = packet.GetString();
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHAR_CREATE [{2}]", client.IP, client.Port, Name);
            byte Race = packet.GetInt8();
            byte Classe = packet.GetInt8();
            byte Gender = packet.GetInt8();
            byte Skin = packet.GetInt8();
            byte Face = packet.GetInt8();
            byte HairStyle = packet.GetInt8();
            byte HairColor = packet.GetInt8();
            byte FacialHair = packet.GetInt8();
            byte OutfitId = packet.GetInt8();
            int result = (int)CharResponse.CHAR_CREATE_DISABLED;

            // Try to pass the packet to one of World Servers
            try
            {
                if (ClusterServiceLocator._WC_Network.WorldServer.Worlds.ContainsKey(0U))
                {
                    result = ClusterServiceLocator._WC_Network.WorldServer.Worlds[0U].ClientCreateCharacter(client.Account, Name, Race, Classe, Gender, Skin, Face, HairStyle, HairColor, FacialHair, OutfitId);
                }
                else if (ClusterServiceLocator._WC_Network.WorldServer.Worlds.ContainsKey(1U))
                {
                    result = ClusterServiceLocator._WC_Network.WorldServer.Worlds[1U].ClientCreateCharacter(client.Account, Name, Race, Classe, Gender, Skin, Face, HairStyle, HairColor, FacialHair, OutfitId);
                }
            }
            catch (Exception ex)
            {
                result = (int)CharResponse.CHAR_CREATE_ERROR;
                ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.FAILED, "[{0}:{1}] Character creation failed!{2}{3}", client.IP, client.Port, Constants.vbCrLf, ex.ToString());
            }

            var response = new Packets.PacketClass(OPCODES.SMSG_CHAR_CREATE);
            response.AddInt8((byte)result);
            client.Send(response);
        }

        public void On_CMSG_PLAYER_LOGIN(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            packet.GetInt16();               // int16 unknown
            ulong GUID = packet.GetUInt64();
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_PLAYER_LOGIN [0x{2:X}]", client.IP, client.Port, GUID);
            if (client.Character is null)
            {
                client.Character = new WcHandlerCharacter.CharacterObject(GUID, client);
            }
            else if (client.Character.Guid != GUID)
            {
                client.Character.Dispose();
                client.Character = new WcHandlerCharacter.CharacterObject(GUID, client);
            }
            else
            {
                client.Character.ReLoad();
            }

            if (ClusterServiceLocator._WC_Network.WorldServer.InstanceCheck(client, client.Character.Map))
            {
                client.Character.GetWorld.ClientConnect(client.Index, client.GetClientInfo());
                client.Character.IsInWorld = true;
                client.Character.GetWorld.ClientLogin(client.Index, client.Character.Guid);
                client.Character.OnLogin();
            }
            else
            {
                ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.FAILED, "[{0:000000}] Unable to login: WORLD SERVER DOWN", client.Index);
                client.Character.Dispose();
                client.Character = null;
                var r = new Packets.PacketClass(OPCODES.SMSG_CHARACTER_LOGIN_FAILED);
                try
                {
                    r.AddInt8((byte)CharResponse.CHAR_LOGIN_NO_WORLD);
                    client.Send(r);
                }
                catch (Exception ex)
                {
                    ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.FAILED, "[{0:000000}] Unable to login: {1}", client.Index, ex.ToString());
                    client.Character.Dispose();
                    client.Character = null;
                    var a = new Packets.PacketClass(OPCODES.SMSG_CHARACTER_LOGIN_FAILED);
                    try
                    {
                        a.AddInt8((byte)CharResponse.CHAR_LOGIN_FAILED);
                        client.Send(a);
                    }
                    finally
                    {
                        r.Dispose();
                    }
                }
            }
        }

        // Leak is with in this code. Needs a rewrite to correct the leak. This only effects the CPU Usage.
        // Happens when the client disconnects from the server.
        public void On_CMSG_PLAYER_LOGOUT(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_PLAYER_LOGOUT", client.IP, client.Port);
            client.Character.OnLogout();
            client.Character.GetWorld.ClientDisconnect(client.Index); // Likely the cause of it
            client.Character.Dispose();
            client.Character = null;
        }

        public void On_MSG_MOVE_WORLDPORT_ACK(Packets.PacketClass packet, WC_Network.ClientClass client)
        {
            ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_MOVE_WORLDPORT_ACK", client.IP, client.Port);
            try
            {
                if (!ClusterServiceLocator._WC_Network.WorldServer.InstanceCheck(client, client.Character.Map))
                    return;
                if (client.Character.IsInWorld)
                {
                    // Inside server transfer
                    client.Character.GetWorld.ClientLogin(client.Index, client.Character.Guid);
                }
                else
                {
                    // Inter-server transfer
                    client.Character.ReLoad();
                    client.Character.GetWorld.ClientConnect(client.Index, client.GetClientInfo());
                    client.Character.IsInWorld = true;
                    client.Character.GetWorld.ClientLogin(client.Index, client.Character.Guid);
                }
            }
            catch (Exception ex)
            {
                ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.CRITICAL, "{0}", ex.ToString());
            }
        }
    }
}