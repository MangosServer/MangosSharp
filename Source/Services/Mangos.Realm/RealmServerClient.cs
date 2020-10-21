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

using Mangos.Common.Enums.Authentication;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Misc;
using Mangos.Loggers;
using Mangos.Network.Tcp;
using Mangos.Network.Tcp.Extensions;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Mangos.Common.Globals;
using Mangos.Storage.Account;

namespace Mangos.Realm
{
    public class RealmServerClient : ITcpClient
    {
        private readonly ILogger logger;
        private readonly IAccountStorage accountStorage;
        private readonly Converter converter;
        private readonly MangosGlobalConstants mangosGlobalConstants;

        private readonly AuthEngineClass authEngineClass;

        private readonly Dictionary<AuthCMD, Func<ChannelReader<byte>, ChannelWriter<byte>, Task>> packetHandlers;
        private readonly IPEndPoint remoteEnpoint;


        public string Account = "";
        public string UpdateFile = "";
        public AccessLevel Access = AccessLevel.Player;

        public RealmServerClient(ILogger logger,
            IAccountStorage accountStorage,
            Converter converter,
            MangosGlobalConstants mangosGlobalConstants,
            IPEndPoint remoteEnpoint)
        {
            this.logger = logger;
            this.accountStorage = accountStorage;
            this.converter = converter;
            this.mangosGlobalConstants = mangosGlobalConstants;
            this.remoteEnpoint = remoteEnpoint;

            authEngineClass = new AuthEngineClass();
            packetHandlers = GetPacketHandlers();
        }

        public async void HandleAsync(
            ChannelReader<byte> reader,
            ChannelWriter<byte> writer,
            CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var header = await reader.ReadAsync(1).ToListAsync();
                var opcode = (AuthCMD)header[0];

                await packetHandlers[opcode](reader, writer);
            }
        }

        private Dictionary<AuthCMD, Func<ChannelReader<byte>, ChannelWriter<byte>, Task>> GetPacketHandlers()
        {
            return new Dictionary<AuthCMD, Func<ChannelReader<byte>, ChannelWriter<byte>, Task>>
            {
                [AuthCMD.CMD_AUTH_LOGON_CHALLENGE] = On_RS_LOGON_CHALLENGE,
                [AuthCMD.CMD_AUTH_RECONNECT_CHALLENGE] = On_RS_LOGON_CHALLENGE,
                [AuthCMD.CMD_AUTH_LOGON_PROOF] = On_RS_LOGON_PROOF,
                [AuthCMD.CMD_AUTH_REALMLIST] = On_RS_REALMLIST,
                [AuthCMD.CMD_XFER_ACCEPT] = On_CMD_XFER_ACCEPT,
                [AuthCMD.CMD_XFER_RESUME] = On_CMD_XFER_RESUME,
                [AuthCMD.CMD_XFER_CANCEL] = On_CMD_XFER_CANCEL,
            };
        }

        public async Task On_RS_LOGON_CHALLENGE(ChannelReader<byte> reader, ChannelWriter<byte> writer)
        {
            var header = await reader.ReadAsync(3).ToListAsync();
            var length = BitConverter.ToInt16(new[] { header[1], header[2] });
            var body = await reader.ReadAsync(length).ToListAsync();
            var data = new byte[1].Concat(header).Concat(body).ToArray();

            int iUpper = data[33] - 1;
            string packetAccount;
            string packetIp;
            AccountState accState; // = AccountState.LOGIN_DBBUSY

            // Read account name from packet
            packetAccount = "";
            for (int i = 0, loopTo = iUpper; i <= loopTo; i++)
                packetAccount += Conversions.ToString((char)data[34 + i]);
            Account = packetAccount;

            // Read users ip from packet
            packetIp = ((int)data[29]).ToString() + "." + ((int)data[30]).ToString() + "." + ((int)data[31]).ToString() + "." + ((int)data[32]).ToString();

            // Get the client build from packet.
            int bMajor = data[8];
            int bMinor = data[9];
            int bRevision = data[10];
            int clientBuild = BitConverter.ToInt16(new byte[] { data[11], data[12] }, 0);
            string clientLanguage = Conversions.ToString((char)data[24]) + (char)data[23] + (char)data[22] + (char)data[21];

            // DONE: Check if our build can join the server
            // If ((RequiredVersion1 = 0 AndAlso RequiredVersion2 = 0 AndAlso RequiredVersion3 = 0) OrElse
            // (bMajor = RequiredVersion1 AndAlso bMinor = RequiredVersion2 AndAlso bRevision = RequiredVersion3)) AndAlso
            // clientBuild >= RequiredBuildLow AndAlso clientBuild <= RequiredBuildHigh Then
            if (bMajor == 0 & bMinor == 0 & bRevision == 0)
            {
                var dataResponse = new byte[2];
                dataResponse[0] = (byte)AuthCMD.CMD_AUTH_LOGON_PROOF;
                dataResponse[1] = (byte)AccountState.LOGIN_BADVERSION;
                await writer.WriteAsync(dataResponse);
            }
            else if (clientBuild == mangosGlobalConstants.Required_Build_1_12_1 | clientBuild == mangosGlobalConstants.Required_Build_1_12_2 | clientBuild == mangosGlobalConstants.Required_Build_1_12_3)
            {
                // TODO: in the far future should check if the account is expired too
                var accountInfo = await accountStorage.GetAccountInfoAsync(packetAccount);
                try
                {
                    // Check Account state
                    if (accountInfo != null)
                    {
                        accState = await accountStorage.IsBannedAccountAsync(accountInfo.id)
                            ? AccountState.LOGIN_BANNED
                            : AccountState.LOGIN_OK;
                    }
                    else
                    {
                        accState = AccountState.LOGIN_UNKNOWN_ACCOUNT;
                    }
                }
                catch
                {
                    accState = AccountState.LOGIN_DBBUSY;
                }

                // DONE: Send results to client
                switch (accState)
                {
                    case var @case when @case == AccountState.LOGIN_OK:
                        {
                            var account = new byte[(data[33])];
                            Array.Copy(data, 34, account, 0, data[33]);
                            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(accountInfo.sha_pass_hash.Length, 40, false))) // Invalid password type, should always be 40 characters
                            {
                                var dataResponse = new byte[2];
                                dataResponse[0] = (byte)AuthCMD.CMD_AUTH_LOGON_PROOF;
                                dataResponse[1] = (byte)AccountState.LOGIN_BAD_PASS;
                                await writer.WriteAsync(dataResponse);
                            }
                            else // Bail out with something meaningful
                            {
                                Access = (AccessLevel)Enum.Parse(typeof(AccessLevel), accountInfo.gmlevel);
                                var hash = new byte[20];
                                for (int i = 0; i <= 39; i += 2)
                                    hash[i / 2] = (byte)Conversions.ToInteger(Operators.ConcatenateObject("&H", accountInfo.sha_pass_hash.Substring(i, 2)));

                                // Language = clientLanguage
                                // If Not IsDBNull(result.Rows(0).Item("expansion")) Then
                                // Expansion = result.Rows(0).Item("expansion")
                                // Else
                                // Expansion = ExpansionLevel.NORMAL
                                // End If

                                try
                                {
                                    authEngineClass.CalculateX(account, hash);
                                    var dataResponse = new byte[119];
                                    dataResponse[0] = (byte)AuthCMD.CMD_AUTH_LOGON_CHALLENGE;
                                    dataResponse[1] = (byte)AccountState.LOGIN_OK;
                                    dataResponse[2] = (byte)Conversion.Val("&H00");
                                    Array.Copy(authEngineClass.PublicB, 0, dataResponse, 3, 32);
                                    dataResponse[35] = (byte)authEngineClass.g.Length;
                                    dataResponse[36] = authEngineClass.g[0];
                                    dataResponse[37] = 32;
                                    Array.Copy(authEngineClass.N, 0, dataResponse, 38, 32);
                                    Array.Copy(authEngineClass.Salt, 0, dataResponse, 70, 32);
                                    Array.Copy(AuthEngineClass.CrcSalt, 0, dataResponse, 102, 16);
                                    dataResponse[118] = 0; // Added in 1.12.x client branch? Security Flags (&H0...&H4)?
                                    await writer.WriteAsync(dataResponse);
                                }
                                catch (Exception ex)
                                {
                                    logger.Error("Error loading AuthEngine: {0}", ex);
                                }
                            }

                            return;
                        }

                    case var case1 when case1 == AccountState.LOGIN_UNKNOWN_ACCOUNT:
                        {
                            var dataResponse = new byte[2];
                            dataResponse[0] = (byte)AuthCMD.CMD_AUTH_LOGON_PROOF;
                            dataResponse[1] = (byte)AccountState.LOGIN_UNKNOWN_ACCOUNT;
                            await writer.WriteAsync(dataResponse);
                            return;
                        }

                    case var case2 when case2 == AccountState.LOGIN_BANNED:
                        {
                            var dataResponse = new byte[2];
                            dataResponse[0] = (byte)AuthCMD.CMD_AUTH_LOGON_PROOF;
                            dataResponse[1] = (byte)AccountState.LOGIN_BANNED;
                            await writer.WriteAsync(dataResponse);
                            return;
                        }

                    case var case3 when case3 == AccountState.LOGIN_NOTIME:
                        {
                            var dataResponse = new byte[2];
                            dataResponse[0] = (byte)AuthCMD.CMD_AUTH_LOGON_PROOF;
                            dataResponse[1] = (byte)AccountState.LOGIN_NOTIME;
                            await writer.WriteAsync(dataResponse);
                            return;
                        }

                    case var case4 when case4 == AccountState.LOGIN_ALREADYONLINE:
                        {
                            var dataResponse = new byte[2];
                            dataResponse[0] = (byte)AuthCMD.CMD_AUTH_LOGON_PROOF;
                            dataResponse[1] = (byte)AccountState.LOGIN_ALREADYONLINE;
                            await writer.WriteAsync(dataResponse);
                            return;
                        }

                    case var case5 when case5 == AccountState.LOGIN_FAILED:
                        {
                            break;
                        }

                    case var case6 when case6 == AccountState.LOGIN_BAD_PASS:
                        {
                            break;
                        }

                    case var case7 when case7 == AccountState.LOGIN_DBBUSY:
                        {
                            break;
                        }

                    case var case8 when case8 == AccountState.LOGIN_BADVERSION:
                        {
                            break;
                        }

                    case var case9 when case9 == AccountState.LOGIN_DOWNLOADFILE:
                        {
                            break;
                        }

                    case var case10 when case10 == AccountState.LOGIN_SUSPENDED:
                        {
                            break;
                        }

                    case var case11 when case11 == AccountState.LOGIN_PARENTALCONTROL:
                        {
                            break;
                        }

                    default:
                        {
                            var dataResponse = new byte[2];
                            dataResponse[0] = (byte)AuthCMD.CMD_AUTH_LOGON_PROOF;
                            dataResponse[1] = (byte)AccountState.LOGIN_FAILED;
                            await writer.WriteAsync(dataResponse);
                            return;
                        }
                }
            }
            else if (!string.IsNullOrEmpty(FileSystem.Dir("Updates/wow-patch-" + Conversion.Val("&H" + Conversion.Hex(data[12]) + Conversion.Hex(data[11])) + "-" + (char)data[24] + (char)data[23] + (char)data[22] + (char)data[21] + ".mpq")))
            {
                // Send UPDATE_MPQ
                UpdateFile = "Updates/wow-patch-" + Conversion.Val("&H" + Conversion.Hex(data[12]) + Conversion.Hex(data[11])) + "-" + (char)data[24] + (char)data[23] + (char)data[22] + (char)data[21] + ".mpq";
                var dataResponse = new byte[31];
                dataResponse[0] = (byte)AuthCMD.CMD_XFER_INITIATE;
                // Name Len 0x05 -> sizeof(Patch)
                int i = 1;
                converter.ToBytes(Conversions.ToByte(5), dataResponse, ref i);
                // Name 'Patch'
                converter.ToBytes("Patch", dataResponse, ref i);
                // Size 0x34 C4 0D 00 = 902,196 byte (180->181 enGB)
                converter.ToBytes(Conversions.ToInteger(FileSystem.FileLen(UpdateFile)), dataResponse, ref i);
                // Unknown 0x0 always
                converter.ToBytes(0, dataResponse, ref i);
                // MD5 CheckSum
                var md5 = new MD5CryptoServiceProvider();
                byte[] buffer;
                var fs = new FileStream(UpdateFile, FileMode.Open);
                var r = new BinaryReader(fs);
                buffer = r.ReadBytes((int)FileSystem.FileLen(UpdateFile));
                r.Close();
                // fs.Close()
                var result = md5.ComputeHash(buffer);
                Array.Copy(result, 0, dataResponse, 15, 16);
                await writer.WriteAsync(dataResponse);
            }
            else
            {
                // Send BAD_VERSION
                logger.Warning("WRONG_VERSION [" + (char)data[6] + (char)data[5] + (char)data[4] + " " + data[8] + "." + data[9] + "." + data[10] + "." + Conversion.Val("&H" + Conversion.Hex(data[12]) + Conversion.Hex(data[11])) + " " + (char)data[15] + (char)data[14] + (char)data[13] + " " + (char)data[19] + (char)data[18] + (char)data[17] + " " + (char)data[24] + (char)data[23] + (char)data[22] + (char)data[21] + "]");
                var dataResponse = new byte[2];
                dataResponse[0] = (byte)AuthCMD.CMD_AUTH_LOGON_PROOF;
                dataResponse[1] = (byte)AccountState.LOGIN_BADVERSION;
                await writer.WriteAsync(dataResponse);
            }
        }

        public async Task On_RS_LOGON_PROOF(ChannelReader<byte> reader, ChannelWriter<byte> writer)
        {
            var body = await reader.ReadAsync(74).ToListAsync();
            var data = new byte[1].Concat(body).ToArray();

            var a = new byte[32];
            Array.Copy(data, 1, a, 0, 32);
            var m1 = new byte[20];
            Array.Copy(data, 33, m1, 0, 20);
            // Dim CRC_Hash(19) As Byte
            // Array.Copy(data, 53, CRC_Hash, 0, 20)
            // Dim NumberOfKeys as Byte = data(73)
            // Dim unk as Byte = data(74)

            // Calculate U and M1
            authEngineClass.CalculateU(a);
            authEngineClass.CalculateM1();
            // AuthEngine.CalculateCRCHash()

            // Check M1=ClientM1
            bool passCheck = true;
            for (byte i = 0; i <= 19; i++)
            {
                if (m1[i] != authEngineClass.M1[i])
                {
                    passCheck = false;
                    break;
                }
            }

            if (!passCheck)
            {
                // Wrong pass
                logger.Debug("Wrong password for user {0}.", Account);
                var dataResponse = new byte[2];
                dataResponse[0] = (byte)AuthCMD.CMD_AUTH_LOGON_PROOF;
                dataResponse[1] = (byte)AccountState.LOGIN_BAD_PASS;
                await writer.WriteAsync(dataResponse);
            }
            else
            {
                authEngineClass.CalculateM2(m1);
                var dataResponse = new byte[26];
                dataResponse[0] = (byte)AuthCMD.CMD_AUTH_LOGON_PROOF;
                dataResponse[1] = (byte)AccountState.LOGIN_OK;
                Array.Copy(authEngineClass.M2, 0, dataResponse, 2, 20);
                dataResponse[22] = 0;
                dataResponse[23] = 0;
                dataResponse[24] = 0;
                dataResponse[25] = 0;
                await writer.WriteAsync(dataResponse);

                // Set SSHash in DB
                string sshash = "";

                // For i as Integer = 0 To AuthEngine.SS_Hash.Length - 1
                for (int i = 0; i <= 40 - 1; i++)
                    sshash = authEngineClass.SsHash[i] < 16 ? sshash + "0" + Conversion.Hex(authEngineClass.SsHash[i]) : sshash + Conversion.Hex(authEngineClass.SsHash[i]);
                await accountStorage.UpdateAccountAsync(sshash, remoteEnpoint.Address.ToString(), Strings.Format(DateAndTime.Now, "yyyy-MM-dd"), Account);
                logger.Debug("Auth success for user {0} [{1}]", Account, sshash);
            }
        }

        public async Task On_RS_REALMLIST(ChannelReader<byte> reader, ChannelWriter<byte> writer)
        {
            var body = await reader.ReadAsync(4).ToListAsync();
            var data = new byte[1].Concat(body).ToArray();

            int packetLen = 0;

            // Fetch RealmList Data
            var realmList = await accountStorage.GetRealmListAsync();
            foreach (var row in realmList)
            {
                packetLen = packetLen
                    + Strings.Len(row.address)
                    + Strings.Len(row.name)
                    + 1
                    + Strings.Len(row.port)
                    + 14;
            }

            var dataResponse = new byte[packetLen + 9 + 1];

            // (byte) Opcode
            dataResponse[0] = (byte)AuthCMD.CMD_AUTH_REALMLIST;

            // (uint16) Packet Length
            dataResponse[2] = (byte)((packetLen + 7) / 256);
            dataResponse[1] = (byte)((packetLen + 7) % 256);

            // (uint32) Unk
            dataResponse[3] = data[1];
            dataResponse[4] = data[2];
            dataResponse[5] = data[3];
            dataResponse[6] = data[4];

            // (uint16) Realms Count
            dataResponse[7] = (byte)realmList.Count();
            dataResponse[8] = 0;
            int tmp = 8;
            foreach (var realmListItem in realmList)
            {
                // Get Number of Characters for the Realm
                var characterCount = await accountStorage.GetNumcharsAsync(realmListItem.id);

                // (uint8) Realm Icon
                // 0 -> Normal; 1 -> PvP; 6 -> RP; 8 -> RPPvP;
                converter.ToBytes(Conversions.ToByte(realmListItem.icon), dataResponse, ref tmp);
                // (uint8) IsLocked
                // 0 -> none; 1 -> locked
                converter.ToBytes(Conversions.ToByte(0), dataResponse, ref tmp);
                // (uint8) unk
                converter.ToBytes(Conversions.ToByte(0), dataResponse, ref tmp);
                // (uint8) unk
                converter.ToBytes(Conversions.ToByte(0), dataResponse, ref tmp);
                // (uint8) Realm Color
                // 0 -> Green; 1 -> Red; 2 -> Offline;
                converter.ToBytes(Conversions.ToByte(realmListItem.realmflags), dataResponse, ref tmp);
                // (string) Realm Name (zero terminated)
                converter.ToBytes(Conversions.ToString(realmListItem.name), dataResponse, ref tmp);
                converter.ToBytes(Conversions.ToByte(0), dataResponse, ref tmp); // \0
                                                                                 // (string) Realm Address ("ip:port", zero terminated)
                converter.ToBytes(Operators.ConcatenateObject(Operators.ConcatenateObject(realmListItem.address, ":"), realmListItem.port).ToString(), dataResponse, ref tmp);
                converter.ToBytes(Conversions.ToByte(0), dataResponse, ref tmp); // \0
                                                                                 // (float) Population
                                                                                 // 400F -> Full; 5F -> Medium; 1.6F -> Low; 200F -> New; 2F -> High
                                                                                 // 00 00 48 43 -> Recommended
                                                                                 // 00 00 C8 43 -> Full
                                                                                 // 9C C4 C0 3F -> Low
                                                                                 // BC 74 B3 3F -> Low
                converter.ToBytes(Conversions.ToSingle(realmListItem.population), dataResponse, ref tmp);
                // (byte) Number of character at this realm for this account
                converter.ToBytes(Conversions.ToByte(characterCount), dataResponse, ref tmp);
                // (byte) Timezone
                // 0x01 - Development
                // 0x02 - USA
                // 0x03 - Oceania
                // 0x04 - LatinAmerica
                // 0x05 - Tournament
                // 0x06 - Korea
                // 0x07 - Tournament
                // 0x08 - UnitedKingdom
                // 0x09 - Germany
                // 0x0A - France
                // 0x0B - Spain
                // 0x0C - Russian
                // 0x0D - Tournament
                // 0x0E - Taiwan
                // 0x0F - Tournament
                // 0x10 - China
                // 0x11 - CN1
                // 0x12 - CN2
                // 0x13 - CN3
                // 0x14 - CN4
                // 0x15 - CN5
                // 0x16 - CN6
                // 0x17 - CN7
                // 0x18 - CN8
                // 0x19 - Tournament
                // 0x1A - Test Server
                // 0x1B - Tournament
                // 0x1C - QA Server
                // 0x1D - CN9
                // 0x1E - Test Server 2
                converter.ToBytes(Conversions.ToByte(realmListItem.timezone), dataResponse, ref tmp);
                // (byte) Unknown (may be 2 -> TestRealm, / 6 -> ?)
                converter.ToBytes(Conversions.ToByte(0), dataResponse, ref tmp);
            }

            dataResponse[tmp] = 2; // 2=list of realms 0=wizard
            dataResponse[tmp + 1] = 0;
            await writer.WriteAsync(dataResponse);
        }

        public async Task On_CMD_XFER_CANCEL(ChannelReader<byte> reader, ChannelWriter<byte> writer)
        {
            // TODO: data parameter is never used
            // logger.Debug("[{0}:{1}] CMD_XFER_CANCEL", Ip, Port);
            // Socket.Close();
        }

        public async Task On_CMD_XFER_ACCEPT(ChannelReader<byte> reader, ChannelWriter<byte> writer)
        {
            // TODO: data parameter is never used			
        }

        public async Task On_CMD_XFER_RESUME(ChannelReader<byte> reader, ChannelWriter<byte> writer)
        {
            await reader.ReadAsync(8).ToListAsync();
        }
    }
}
