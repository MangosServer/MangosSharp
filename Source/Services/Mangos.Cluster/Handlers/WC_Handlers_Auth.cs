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

using Mangos.Cluster.Configuration;
using Mangos.Cluster.Globals;
using Mangos.Cluster.Network;
using Mangos.Common.Enums.Authentication;
using Mangos.Common.Enums.Character;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Misc;
using Mangos.Common.Enums.Player;
using Mangos.Common.Globals;
using Mangos.Common.Legacy;
using Mangos.Configuration;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;

namespace Mangos.Cluster.Handlers;

public class WcHandlersAuth
{
    private readonly IConfigurationProvider<ClusterConfiguration> configurationProvider;
    private readonly ClusterServiceLocator _clusterServiceLocator;

    public WcHandlersAuth(
        ClusterServiceLocator clusterServiceLocator,
        IConfigurationProvider<ClusterConfiguration> configurationProvider)
    {
        _clusterServiceLocator = clusterServiceLocator;
        this.configurationProvider = configurationProvider;
    }

    private const int REQUIRED_BUILD_LOW = 5875; // 1.12.1
    private const int REQUIRED_BUILD_HIGH = 6141;

    public void SendLoginOk(ClientClass client)
    {
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AUTH_SESSION [{2}]", client.IP, client.Port, client.Account);
        Thread.Sleep(500);
        PacketClass response = new(Opcodes.SMSG_AUTH_RESPONSE);
        response.AddInt8((byte)LoginResponse.LOGIN_OK);
        response.AddInt32(0);
        response.AddInt8(2); // BillingPlanFlags
        response.AddUInt32(0U); // BillingTimeRested
        client.Send(response);
    }

    public void On_CMSG_AUTH_SESSION(PacketClass packet, ClientClass client)
    {
        // _WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}] [{1}:{2}] CMSG_AUTH_SESSION", Format(TimeOfDay, "hh:mm:ss"), client.IP, client.Port)

        packet.GetInt16();
        var clientVersion = packet.GetInt32();
        var clientSessionId = packet.GetInt32();
        var clientAccount = packet.GetString();
        var clientSeed = packet.GetInt32();
        var clientHash = new byte[20];
        for (var i = 0; i <= 19; i++)
        {
            clientHash[i] = packet.GetInt8();
        }

        var clientAddOnsSize = packet.GetInt32();

        // DONE: Set client.Account
        var tmp = clientAccount;

        // DONE: Kick if existing
        foreach (var tmpClientEntry in _clusterServiceLocator.WorldCluster.ClienTs)
        {
            if (tmpClientEntry.Value is not null)
            {
                if (tmpClientEntry.Value.Account == tmp)
                {
                    if (tmpClientEntry.Value.Character is not null)
                    {
                        tmpClientEntry.Value.Character.Dispose();
                        tmpClientEntry.Value.Character = null;
                    }

                    tmpClientEntry.Value.Dispose();
                }
            }
        }

        client.Account = tmp;

        // DONE: Set client.SS_Hash
        DataTable result = new();
        string query;
        query = "SELECT sessionkey, gmlevel FROM account WHERE username = '" + client.Account + "';";
        _clusterServiceLocator.WorldCluster.GetAccountDatabase().Query(query, ref result);
        if (result.Rows.Count > 0)
        {
            tmp = result.Rows[0].As<string>("sessionkey");
            client.Access = (AccessLevel)result.Rows[0]["gmlevel"];
        }
        else
        {
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.USER, "[{0}:{1}] AUTH_UNKNOWN_ACCOUNT: Account not in DB!", client.IP, client.Port);
            PacketClass responseUnkAcc = new(Opcodes.SMSG_AUTH_RESPONSE);
            responseUnkAcc.AddInt8((byte)AuthResult.WOW_FAIL_UNKNOWN_ACCOUNT);
            client.Send(responseUnkAcc);
            return;
        }

        client.Client.PacketEncryption.Hash = new byte[40];
        for (int i = 0, loopTo = Strings.Len(tmp) - 1; i <= loopTo; i += 2)
        {
            client.Client.PacketEncryption.Hash[i / 2] = (byte)Conversion.Val("&H" + Strings.Mid(tmp, i + 1, 2));
        }

        client.Client.PacketEncryption.IsEncryptionEnabled = true;

        // DONE: Disconnect clients trying to enter with an invalid build
        if (clientVersion is < REQUIRED_BUILD_LOW or > REQUIRED_BUILD_HIGH)
        {
            PacketClass invalidVersion = new(Opcodes.SMSG_AUTH_RESPONSE);
            invalidVersion.AddInt8((byte)AuthResult.WOW_FAIL_VERSION_INVALID);
            client.Send(invalidVersion);
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
        if (_clusterServiceLocator.WorldCluster.ClienTs.Count > configurationProvider.GetConfiguration().ServerPlayerLimit && client.Access <= AccessLevel.Player)
        {
            ThreadPool.QueueUserWorkItem(client.EnQueue);
        }
        else
        {
            SendLoginOk(client);
        }

        // DONE: Addons info reading
        var decompressBuffer = new byte[packet.Data.Length - packet.Offset + 1];
        Array.Copy(packet.Data, packet.Offset, decompressBuffer, 0, packet.Data.Length - packet.Offset);
        packet.Data = _clusterServiceLocator.GlobalZip.DeCompress(decompressBuffer);
        packet.Offset = 0;
        // DumpPacket(packet.Data)

        List<string> addOnsNames = new();
        List<uint> addOnsHashes = new();
        // Dim AddOnsConsoleWrite As String = String.Format("[{0}:{1}] Client addons loaded:", client.IP, client.Port)
        while (packet.Offset < clientAddOnsSize)
        {
            addOnsNames.Add(packet.GetString());
            addOnsHashes.Add(packet.GetUInt32());
            packet.GetInt32(); // Unk7
            packet.GetInt8(); // Unk6
                              // AddOnsConsoleWrite &= String.Format("{0}{1} AddOnName: [{2,-30}], AddOnHash: [{3:X}]", vbCrLf, vbTab, AddOnsNames(AddOnsNames.Count - 1), AddOnsHashes(AddOnsHashes.Count - 1))
        }
        // _WorldCluster.Log.WriteLine(LogType.DEBUG, AddOnsConsoleWrite)

        // DONE: Build mysql addons query
        // Not needed already - in 1.11 addons list is removed.

        // DONE: Send packet
        PacketClass addOnsEnable = new(Opcodes.SMSG_ADDON_INFO);
        for (int i = 0, loopTo1 = addOnsNames.Count - 1; i <= loopTo1; i++)
        {
            if (File.Exists(string.Format(@"interface\{0}.pub", addOnsNames[i])) && addOnsHashes[i] != 0x1C776D01U)
            {
                // We have hash data
                addOnsEnable.AddInt8(2);                    // AddOn Type [1-enabled, 0-banned, 2-blizzard]
                addOnsEnable.AddInt8(1);                    // Unk
                FileStream fs = new(string.Format(@"interface\{0}.pub", addOnsNames[i]), FileMode.Open, FileAccess.Read, FileShare.Read, 258, FileOptions.SequentialScan);
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

    public void On_CMSG_PING(PacketClass packet, ClientClass client)
    {
        if (packet.Data.Length - 1 < 9)
        {
            return;
        }

        packet.GetInt16();
        PacketClass response = new(Opcodes.SMSG_PONG);
        response.AddInt32(packet.GetInt32());
        client.Send(response);
        if (client.Character is not null)
        {
            client.Character.Latency = packet.GetInt32();
        }

        // _WorldCluster.Log.WriteLine(LogType.NETWORK, "[{0}:{1}] SMSG_PONG [{2}]", client.IP, client.Port, client.Character.Latency)
    }

    public void On_CMSG_UPDATE_ACCOUNT_DATA(PacketClass packet, ClientClass client)
    {
        try
        {
            if (packet.Data.Length - 1 < 13)
            {
                return;
            }

            packet.GetInt16();
            var dataId = packet.GetUInt32();
            var uncompressedSize = packet.GetUInt32();
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_UPDATE_ACCOUNT_DATA [ID={2} Size={3}]", client.IP, client.Port, dataId, uncompressedSize);
            if (dataId > 7L)
            {
                return;
            }

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

            var receivedPacketSize = packet.Data.Length - packet.Offset;
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
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.FAILED, "Error while updating account data.{0}", Constants.vbCrLf + e);
        }
    }

    public void On_CMSG_REQUEST_ACCOUNT_DATA(PacketClass packet, ClientClass client)
    {
        if (packet.Data.Length - 1 < 9)
        {
            return;
        }

        packet.GetInt16();
        var dataId = packet.GetUInt32();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_REQUEST_ACCOUNT_DATA [ID={2}]", client.IP, client.Port, dataId);
        if (dataId > 7L)
        {
            return;
        }

        // Dim AccData As New DataTable
        // _WorldCluster.AccountDatabase.Query(String.Format("SELECT account_id FROM accounts WHERE username = ""{0}"";", client.Account), AccData)
        // If AccData.Rows.Count > 0 Then
        // Dim AccID As Integer = CType(AccData.Rows(0).Item("account_id"), Integer)
        //
        // AccData.Clear()
        // _WorldCluster.AccountDatabase.Query(String.Format("SELECT `account_data{1}` FROM account_data WHERE account_id = {0}", AccID, DataID), AccData)
        // If AccData.Rows.Count > 0 Then FoundData = True
        // End If

        PacketClass response = new(Opcodes.SMSG_UPDATE_ACCOUNT_DATA);
        response.AddUInt32(dataId);

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

    public void On_CMSG_CHAR_ENUM(PacketClass packet, ClientClass client)
    {
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHAR_ENUM", client.IP, client.Port);

        // DONE: Query _WorldCluster.CHARACTERs DB
        PacketClass response = new(Opcodes.SMSG_CHAR_ENUM);
        DataTable mySqlQuery = new();
        int accountId;
        try
        {
            _clusterServiceLocator.WorldCluster.GetAccountDatabase().Query(string.Format("SELECT id FROM account WHERE username = '{0}';", client.Account), ref mySqlQuery);
            accountId = mySqlQuery.Rows[0].As<int>("id");
            mySqlQuery.Clear();
            _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(string.Format("SELECT * FROM characters WHERE account_id = '{0}' ORDER BY char_guid;", accountId), ref mySqlQuery);

            // DONE: Make The Packet
            response.AddInt8((byte)mySqlQuery.Rows.Count);
            for (int i = 0, loopTo = mySqlQuery.Rows.Count - 1; i <= loopTo; i++)
            {
                var dead = false;
                DataTable deadMySqlQuery = new();
                _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(string.Format("SELECT COUNT(*) FROM corpse WHERE player = {0};", mySqlQuery.Rows[i]["char_guid"]), ref deadMySqlQuery);
                if (deadMySqlQuery.Rows[0].As<int>(0) > 0)
                {
                    dead = true;
                }

                DataTable petQuery = new();
                _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(string.Format("SELECT modelid, level, entry FROM character_pet WHERE owner = '{0}';", mySqlQuery.Rows[i]["char_guid"]), ref petQuery);
                response.AddInt64(mySqlQuery.Rows[i].As<long>("char_guid"));
                response.AddString(mySqlQuery.Rows[i].As<string>("char_name"));
                response.AddInt8(mySqlQuery.Rows[i].As<byte>("char_race"));
                response.AddInt8(mySqlQuery.Rows[i].As<byte>("char_class"));
                response.AddInt8(mySqlQuery.Rows[i].As<byte>("char_gender"));
                response.AddInt8(mySqlQuery.Rows[i].As<byte>("char_skin"));
                response.AddInt8(mySqlQuery.Rows[i].As<byte>("char_face"));
                response.AddInt8(mySqlQuery.Rows[i].As<byte>("char_hairStyle"));
                response.AddInt8(mySqlQuery.Rows[i].As<byte>("char_hairColor"));
                response.AddInt8(mySqlQuery.Rows[i].As<byte>("char_facialHair"));
                response.AddInt8(mySqlQuery.Rows[i].As<byte>("char_level"));
                response.AddInt32(mySqlQuery.Rows[i].As<int>("char_zone_id"));
                response.AddInt32(mySqlQuery.Rows[i].As<int>("char_map_id"));
                response.AddSingle(mySqlQuery.Rows[i].As<float>("char_positionX"));
                response.AddSingle(mySqlQuery.Rows[i].As<float>("char_positionY"));
                response.AddSingle(mySqlQuery.Rows[i].As<float>("char_positionZ"));
                response.AddInt32(mySqlQuery.Rows[i].As<int>("char_guildId"));
                var playerState = (uint)CharacterFlagState.CHARACTER_FLAG_NONE;
                var forceRestrictions = mySqlQuery.Rows[i].As<uint>("force_restrictions");
                if ((forceRestrictions & (uint)ForceRestrictionFlags.RESTRICT_TRANSFER) != 0)
                {
                    playerState += (uint)CharacterFlagState.CHARACTER_FLAG_LOCKED_FOR_TRANSFER;
                }

                if ((forceRestrictions & (uint)ForceRestrictionFlags.RESTRICT_BILLING) != 0)
                {
                    playerState += (uint)CharacterFlagState.CHARACTER_FLAG_LOCKED_BY_BILLING;
                }

                if ((forceRestrictions & (uint)ForceRestrictionFlags.RESTRICT_RENAME) != 0)
                {
                    playerState += (uint)CharacterFlagState.CHARACTER_FLAG_RENAME;
                }

                if (dead)
                {
                    playerState += (uint)CharacterFlagState.CHARACTER_FLAG_GHOST;
                }

                response.AddUInt32(playerState);
                response.AddInt8(mySqlQuery.Rows[i].As<byte>("char_restState"));
                var petModel = 0;
                var petLevel = 0;
                var petFamily = 0;
                if (petQuery.Rows.Count > 0)
                {
                    petModel = petQuery.Rows[0].As<int>("modelid");
                    petLevel = petQuery.Rows[0].As<int>("level");
                    DataTable petFamilyQuery = new();
                    _clusterServiceLocator.WorldCluster.GetWorldDatabase().Query(string.Format("SELECT family FROM creature_template WHERE entry = '{0}'", petQuery.Rows[0]["entry"]), ref petFamilyQuery);
                    petFamily = petFamilyQuery.Rows[0].As<int>("family");
                }

                response.AddInt32(petModel);
                response.AddInt32(petLevel);
                response.AddInt32(petFamily);

                // DONE: Get items
                var guid = mySqlQuery.Rows[i].As<long>("char_guid");
                DataTable itemsMySqlQuery = new();
                var characterDb = _clusterServiceLocator.WorldCluster.GetCharacterDatabase().SQLDBName;
                var worldDb = _clusterServiceLocator.WorldCluster.GetWorldDatabase().SQLDBName;
                _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(string.Format("SELECT item_slot, displayid, inventorytype FROM " + characterDb + ".characters_inventory, " + worldDb + ".item_template WHERE item_bag = {0} AND item_slot <> 255 AND entry = item_id  ORDER BY item_slot;", guid), ref itemsMySqlQuery);
                var e = itemsMySqlQuery.Rows.GetEnumerator();
                e.Reset();
                e.MoveNext();
                DataRow row = (DataRow)e.Current;

                // DONE: Add model info
                for (byte slot = 0, loopTo1 = (byte)EquipmentSlots.EQUIPMENT_SLOT_END; slot <= loopTo1; slot++) // - 1
                {
                    if (row is null || row.As<int>("item_slot") != slot)
                    {
                        // No equiped item in this slot
                        response.AddInt32(0); // Item Model
                        response.AddInt8(0);  // Item Slot
                    }
                    else
                    {
                        // DONE: Do not show helmet or cloak
                        if (((forceRestrictions & (uint)ForceRestrictionFlags.RESTRICT_HIDECLOAK) != 0) && (EquipmentSlots)row.As<byte>("item_slot") == EquipmentSlots.EQUIPMENT_SLOT_BACK || ((forceRestrictions & (uint)ForceRestrictionFlags.RESTRICT_HIDEHELM) != 0) && (EquipmentSlots)row.As<byte>("item_slot") == EquipmentSlots.EQUIPMENT_SLOT_HEAD)
                        {
                            response.AddInt32(0); // Item Model
                            response.AddInt8(0);  // Item Slot
                        }
                        else
                        {
                            response.AddInt32(row.As<int>("displayid"));          // Item Model
                            response.AddInt8(row.As<byte>("inventorytype"));
                        }       // Item Slot

                        e.MoveNext();
                        row = (DataRow)e.Current;
                    }
                }
            }
        }
        catch (Exception e)
        {
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.FAILED, "[{0}:{1}] Unable to enum characters. [{2}]", client.IP, client.Port, e.Message);
            // TODO: Find what opcode officials use
            response = new PacketClass(Opcodes.SMSG_CHAR_CREATE);
            response.AddInt8((byte)CharResponse.CHAR_LIST_FAILED);
        }

        client.Send(response);
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_CHAR_ENUM", client.IP, client.Port);
    }

    public void On_CMSG_CHAR_DELETE(PacketClass packet, ClientClass client)
    {
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHAR_DELETE", client.IP, client.Port);
        PacketClass response = new(Opcodes.SMSG_CHAR_DELETE);
        packet.GetInt16();
        var guid = packet.GetUInt64();
        try
        {
            DataTable q = new();

            // Done: Fixed packet manipulation protection
            _clusterServiceLocator.WorldCluster.GetAccountDatabase().Query(string.Format("SELECT id FROM account WHERE username = \"{0}\";", client.Account), ref q);
            if (q.Rows.Count == 0)
            {
                return;
            }

            _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(string.Format("SELECT char_guid FROM characters WHERE account_id = \"{0}\" AND char_guid = \"{1}\";", q.Rows[0]["id"], guid), ref q);
            if (q.Rows.Count == 0)
            {
                response.AddInt8((byte)AuthResult.WOW_FAIL_BANNED);
                client.Send(response);
                _clusterServiceLocator.Functions.Ban_Account(client.Account, "Packet Manipulation/Character Deletion");
                client.Delete();
                return;
            }

            q.Clear();
            _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(string.Format("SELECT item_guid FROM characters_inventory WHERE item_bag = {0};", guid), ref q);
            foreach (DataRow row in q.Rows)
            {
                // DONE: Delete items
                _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("DELETE FROM characters_inventory WHERE item_guid = \"{0}\";", row.As<string>("item_guid")));
                // DONE: Delete items in bags
                _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("DELETE FROM characters_inventory WHERE item_bag = \"{0}\";", row.As<ulong>("item_guid") + _clusterServiceLocator.GlobalConstants.GUID_ITEM));
            }

            _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(string.Format("SELECT item_guid FROM characters_inventory WHERE item_owner = {0};", guid), ref q);
            q.Clear();
            _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(string.Format("SELECT mail_id FROM characters_mail WHERE mail_receiver = \"{0}\";", guid), ref q);
            foreach (DataRow row in q.Rows)
            {
                // TODO: Return mails?
                // DONE: Delete mails
                _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("DELETE FROM characters_mail WHERE mail_id = \"{0}\";", row.As<string>("mail_id")));
                // DONE: Delete mail items
                _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("DELETE FROM mail_items WHERE mail_id = \"{0}\";", row.As<string>("mail_id")));
            }

            _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("DELETE FROM characters WHERE char_guid = \"{0}\";", guid));
            _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("DELETE FROM characters_honor WHERE char_guid = \"{0}\";", guid));
            _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("DELETE FROM characters_quests WHERE char_guid = \"{0}\";", guid));
            _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("DELETE FROM character_social WHERE guid = '{0}' OR friend = '{0}';", guid));
            _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("DELETE FROM characters_spells WHERE guid = \"{0}\";", guid));
            _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("DELETE FROM petitions WHERE petition_owner = \"{0}\";", guid));
            _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("DELETE FROM auctionhouse WHERE auction_owner = \"{0}\";", guid));
            _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("DELETE FROM characters_tickets WHERE char_guid = \"{0}\";", guid));
            _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("DELETE FROM corpse WHERE guid = \"{0}\";", guid));
            q.Clear();
            _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(string.Format("SELECT guild_id FROM guilds WHERE guild_leader = \"{0}\";", guid), ref q);
            if (q.Rows.Count > 0)
            {
                _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("UPDATE characters SET char_guildid = 0, char_guildrank = 0, char_guildpnote = '', charguildoffnote = '' WHERE char_guildid = \"{0}\";", q.Rows[0]["guild_id"]));
                _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("DELETE FROM guild WHERE guild_id = \"{0}\";", q.Rows[0]["guild_id"]));
            }

            response.AddInt8((byte)CharResponse.CHAR_DELETE_SUCCESS); // Changed in 1.12.x client branch?
        }
        catch (Exception)
        {
            response.AddInt8((byte)CharResponse.CHAR_DELETE_FAILED);
        }

        client.Send(response);
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_CHAR_DELETE [{2:X}]", client.IP, client.Port, guid);
    }

    public void On_CMSG_CHAR_RENAME(PacketClass packet, ClientClass client)
    {
        packet.GetInt16();
        var guid = packet.GetInt64();
        var name = packet.GetString();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHAR_RENAME [{2}:{3}]", client.IP, client.Port, guid, name);
        var errCode = (byte)ATLoginFlags.AT_LOGIN_RENAME;

        // DONE: Check for existing name
        DataTable q = new();
        _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Query(string.Format("SELECT char_name FROM characters WHERE char_name LIKE \"{0}\";", name), ref q);
        if (q.Rows.Count > 0)
        {
            errCode = (byte)CharResponse.CHAR_CREATE_NAME_IN_USE;
        }

        // DONE: Do the rename
        if (errCode == (byte)ATLoginFlags.AT_LOGIN_RENAME)
        {
            _clusterServiceLocator.WorldCluster.GetCharacterDatabase().Update(string.Format("UPDATE characters SET char_name = \"{1}\", force_restrictions = 0 WHERE char_guid = {0};", guid, name));
        }

        // DONE: Send response
        PacketClass response = new(Opcodes.SMSG_CHAR_RENAME);
        response.AddInt8(errCode);
        client.Send(response);
        response.Dispose();
        PacketClass argpacket = null;
        On_CMSG_CHAR_ENUM(argpacket, client);
    }

    public void On_CMSG_CHAR_CREATE(PacketClass packet, ClientClass client)
    {
        packet.GetInt16();
        var name = packet.GetString();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHAR_CREATE [{2}]", client.IP, client.Port, name);
        var race = packet.GetInt8();
        var classe = packet.GetInt8();
        var gender = packet.GetInt8();
        var skin = packet.GetInt8();
        var face = packet.GetInt8();
        var hairStyle = packet.GetInt8();
        var hairColor = packet.GetInt8();
        var facialHair = packet.GetInt8();
        var outfitId = packet.GetInt8();
        var result = (int)CharResponse.CHAR_CREATE_DISABLED;

        // Try to pass the packet to one of World Servers
        try
        {
            if (_clusterServiceLocator.WcNetwork.WorldServer.Worlds.ContainsKey(0U))
            {
                result = _clusterServiceLocator.WcNetwork.WorldServer.Worlds[0U].ClientCreateCharacter(client.Account, name, race, classe, gender, skin, face, hairStyle, hairColor, facialHair, outfitId);
            }
            else if (_clusterServiceLocator.WcNetwork.WorldServer.Worlds.ContainsKey(1U))
            {
                result = _clusterServiceLocator.WcNetwork.WorldServer.Worlds[1U].ClientCreateCharacter(client.Account, name, race, classe, gender, skin, face, hairStyle, hairColor, facialHair, outfitId);
            }
        }
        catch (Exception ex)
        {
            result = (int)CharResponse.CHAR_CREATE_ERROR;
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.FAILED, "[{0}:{1}] Character creation failed!{2}{3}", client.IP, client.Port, Constants.vbCrLf, ex.ToString());
        }

        PacketClass response = new(Opcodes.SMSG_CHAR_CREATE);
        response.AddInt8((byte)result);
        client.Send(response);
    }

    public void On_CMSG_PLAYER_LOGIN(PacketClass packet, ClientClass client)
    {
        packet.GetInt16();               // int16 unknown
        var guid = packet.GetUInt64();
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_PLAYER_LOGIN [0x{2:X}]", client.IP, client.Port, guid);
        if (client.Character is null)
        {
            client.Character = new WcHandlerCharacter.CharacterObject(guid, client, _clusterServiceLocator);
        }
        else if (client.Character.Guid != guid)
        {
            client.Character.Dispose();
            client.Character = new WcHandlerCharacter.CharacterObject(guid, client, _clusterServiceLocator);
        }
        else
        {
            client.Character.ReLoad();
        }

        if (_clusterServiceLocator.WcNetwork.WorldServer.InstanceCheck(client, client.Character.Map))
        {
            client.Character.GetWorld.ClientConnect(client.Index, client.GetClientInfo());
            client.Character.IsInWorld = true;
            client.Character.GetWorld.ClientLogin(client.Index, client.Character.Guid);
            client.Character.OnLogin();
        }
        else
        {
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.FAILED, "[{0:000000}] Unable to login: WORLD SERVER DOWN", client.Index);
            client.Character.Dispose();
            client.Character = null;
            PacketClass r = new(Opcodes.SMSG_CHARACTER_LOGIN_FAILED);
            try
            {
                r.AddInt8((byte)CharResponse.CHAR_LOGIN_NO_WORLD);
                client.Send(r);
            }
            catch (Exception ex)
            {
                _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.FAILED, "[{0:000000}] Unable to login: {1}", client.Index, ex.ToString());
                client.Character.Dispose();
                client.Character = null;
                PacketClass a = new(Opcodes.SMSG_CHARACTER_LOGIN_FAILED);
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
    public void On_CMSG_PLAYER_LOGOUT(PacketClass packet, ClientClass client)
    {
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_PLAYER_LOGOUT", client.IP, client.Port);
        client.Character.OnLogout();
        client.Character.GetWorld.ClientDisconnect(client.Index); // Likely the cause of it
        client.Character.Dispose();
        client.Character = null;
    }

    public void On_MSG_MOVE_WORLDPORT_ACK(PacketClass packet, ClientClass client)
    {
        _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_MOVE_WORLDPORT_ACK", client.IP, client.Port);
        try
        {
            if (!_clusterServiceLocator.WcNetwork.WorldServer.InstanceCheck(client, client.Character.Map))
            {
                return;
            }

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
            _clusterServiceLocator.WorldCluster.Log.WriteLine(LogType.CRITICAL, "{0}", ex.ToString());
        }
    }
}
