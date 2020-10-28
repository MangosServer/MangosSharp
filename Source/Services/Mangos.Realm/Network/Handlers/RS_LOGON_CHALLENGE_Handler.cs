using Mangos.Common.Enums.Authentication;
using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using Mangos.Loggers;
using Mangos.Network.Tcp.Extensions;
using Mangos.Realm.Models;
using Mangos.Storage.Account;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Realm.Network.Handlers
{
    public class RS_LOGON_CHALLENGE_Handler : IPacketHandler
    {
        private readonly ILogger logger;
        private readonly MangosGlobalConstants mangosGlobalConstants;
        private readonly IRealmStorage realmStorage;

        public RS_LOGON_CHALLENGE_Handler(
            ILogger logger,
            MangosGlobalConstants mangosGlobalConstants, 
            IRealmStorage realmStorage)
        {
            this.mangosGlobalConstants = mangosGlobalConstants;
            this.realmStorage = realmStorage;
            this.logger = logger;
        }

        public async Task HandleAsync(ChannelReader<byte> reader, ChannelWriter<byte> writer, ClientModel clientModel)
        {
            var header = await reader.ReadArrayAsync(3);
            var length = BitConverter.ToInt16(new[] { header[1], header[2] });
            var body = await reader.ReadArrayAsync(length);
            var data = new byte[1].Concat(header).Concat(body).ToArray();

            int iUpper = data[33] - 1;
            string packetAccount;
            string packetIp;
            AccountState accState; // = AccountState.LOGIN_DBBUSY

            // Read account name from packet
            packetAccount = "";
            for (int i = 0, loopTo = iUpper; i <= loopTo; i++)
                packetAccount += Conversions.ToString((char)data[34 + i]);
            clientModel.AccountName = packetAccount;

            // Read users ip from packet
            packetIp = (int)data[29] + "." + (int)data[30] + "." + (int)data[31] + "." + (int)data[32];

            // Get the client build from packet.
            int bMajor = data[8];
            int bMinor = data[9];
            int bRevision = data[10];
            int clientBuild = BitConverter.ToInt16(new[] { data[11], data[12] }, 0);
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
                var accountInfo = await realmStorage.GetAccountInfoAsync(packetAccount);
                try
                {
                    // Check Account state
                    if (accountInfo != null)
                    {
                        accState = await realmStorage.IsBannedAccountAsync(accountInfo.id)
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
                    case AccountState.LOGIN_OK:
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
                                    clientModel.ClientAuthEngine.CalculateX(account, hash);
                                    var dataResponse = new byte[119];
                                    dataResponse[0] = (byte)AuthCMD.CMD_AUTH_LOGON_CHALLENGE;
                                    dataResponse[1] = (byte)AccountState.LOGIN_OK;
                                    dataResponse[2] = (byte)Conversion.Val("&H00");
                                    Array.Copy(clientModel.ClientAuthEngine.PublicB, 0, dataResponse, 3, 32);
                                    dataResponse[35] = (byte)clientModel.ClientAuthEngine.g.Length;
                                    dataResponse[36] = clientModel.ClientAuthEngine.g[0];
                                    dataResponse[37] = 32;
                                    Array.Copy(clientModel.ClientAuthEngine.N, 0, dataResponse, 38, 32);
                                    Array.Copy(clientModel.ClientAuthEngine.Salt, 0, dataResponse, 70, 32);
                                    Array.Copy(ClientAuthEngine.CrcSalt, 0, dataResponse, 102, 16);
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

                    case AccountState.LOGIN_UNKNOWN_ACCOUNT:
                        {
                            var dataResponse = new byte[2];
                            dataResponse[0] = (byte)AuthCMD.CMD_AUTH_LOGON_PROOF;
                            dataResponse[1] = (byte)AccountState.LOGIN_UNKNOWN_ACCOUNT;
                            await writer.WriteAsync(dataResponse);
                            return;
                        }

                    case AccountState.LOGIN_BANNED:
                        {
                            var dataResponse = new byte[2];
                            dataResponse[0] = (byte)AuthCMD.CMD_AUTH_LOGON_PROOF;
                            dataResponse[1] = (byte)AccountState.LOGIN_BANNED;
                            await writer.WriteAsync(dataResponse);
                            return;
                        }

                    case AccountState.LOGIN_NOTIME:
                        {
                            var dataResponse = new byte[2];
                            dataResponse[0] = (byte)AuthCMD.CMD_AUTH_LOGON_PROOF;
                            dataResponse[1] = (byte)AccountState.LOGIN_NOTIME;
                            await writer.WriteAsync(dataResponse);
                            return;
                        }

                    case AccountState.LOGIN_ALREADYONLINE:
                        {
                            var dataResponse = new byte[2];
                            dataResponse[0] = (byte)AuthCMD.CMD_AUTH_LOGON_PROOF;
                            dataResponse[1] = (byte)AccountState.LOGIN_ALREADYONLINE;
                            await writer.WriteAsync(dataResponse);
                            return;
                        }
                    case AccountState.LOGIN_FAILED:
                    case AccountState.LOGIN_BAD_PASS:
                    case AccountState.LOGIN_DBBUSY:     
                    case AccountState.LOGIN_BADVERSION:
                    case AccountState.LOGIN_DOWNLOADFILE:
                    case AccountState.LOGIN_SUSPENDED:
                    case AccountState.LOGIN_PARENTALCONTROL:
                        break;
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
    }
}
