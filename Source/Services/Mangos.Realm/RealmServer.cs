//
//  Copyright (C) 2013-2020 getMaNGOS <https:\\getmangos.eu>
//  
//  This program is free software. You can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation. either version 2 of the License, or
//  (at your option) any later version.
//  
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY. Without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//  
//  You should have received a copy of the GNU General Public License
//  along with this program. If not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using global;
using Mangos.Common;
using Mangos.Common.Enums.Authentication;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Misc;
using Mangos.Common.Globals;
using Mangos.Common.Logging;
using Mangos.Configuration;
using Mangos.Realm.Factories;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.Realm
{
    public class RealmServer
    {
        private readonly IConfigurationProvider<RealmServerConfiguration> _configurationProvider;
        private readonly Common.Globals.Functions _CommonGlobalFunctions;
        private readonly Converter _Converter;
        private readonly Global_Constants _Global_Constants;
        private readonly RealmServerClassFactory _RealmServerClassFactory;
        private const string RealmPath = "configs/RealmServer.ini";
        public BaseWriter Log = new BaseWriter();

        public RealmServer(Common.Globals.Functions commonGlobalFunctions, Converter converter, Global_Constants globalConstants, RealmServerClassFactory realmServerClassFactory, IConfigurationProvider<RealmServerConfiguration> configurationProvider)
        {
            _CommonGlobalFunctions = commonGlobalFunctions;
            _Converter = converter;
            _Global_Constants = globalConstants;
            _RealmServerClassFactory = realmServerClassFactory;
            _configurationProvider = configurationProvider;
        }

        private void LoadConfig()
        {
            try
            {
                // Make sure RealmServer.ini exists
                if (File.Exists(RealmPath) == false)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[{0}] Cannot Continue. {1} does not exist.", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), RealmPath);
                    Console.WriteLine("Please make sure your ini files are inside config folder where the mangosvb executables are located.");
                    Console.WriteLine("Press any key to exit server: ");
                    Console.ReadKey();
                    Environment.Exit(0);
                }

                Console.Write("[{0}] Loading Configuration...", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"));
                Console.WriteLine(".[done]");

                // DONE: Setting SQL Connection
                var configuration = _configurationProvider.GetConfiguration();
                var accountDbSettings = Strings.Split(configuration.AccountDatabase, ";");
                if (accountDbSettings.Length != 6)
                {
                    Console.WriteLine("Invalid connect string for the account database!");
                }
                else
                {
                    AccountDatabase.SQLDBName = accountDbSettings[4];
                    AccountDatabase.SQLHost = accountDbSettings[2];
                    AccountDatabase.SQLPort = accountDbSettings[3];
                    AccountDatabase.SQLUser = accountDbSettings[0];
                    AccountDatabase.SQLPass = accountDbSettings[1];
                    AccountDatabase.SQLTypeServer = (SQL.DB_Type)Enum.Parse(typeof(SQL.DB_Type), accountDbSettings[5]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public RealmServerClass RealmServerClass { get; set; }
        public Dictionary<uint, DateTime> LastSocketConnection { get; private set; } = new Dictionary<uint, DateTime>();
        public SQL AccountDatabase { get; set; } = new SQL();

        private void SqlEventHandler(SQL.EMessages messageId, string outBuf)
        {
            switch (messageId)
            {
                case var @case when @case == SQL.EMessages.ID_Error:
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    }

                case var case1 when case1 == SQL.EMessages.ID_Message:
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        break;
                    }

                default:
                    {
                        break;
                    }
            }

            Console.WriteLine("[" + Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss") + "] " + outBuf);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        // Public Enum WoWLanguage As Byte
        // EnGb = 0
        // EnUs = 1
        // DeDe = 2
        // FrFr = 3
        // End Enum

        public void On_RS_LOGON_CHALLENGE(ref byte[] data, ref ClientClass client)
        {
            int iUpper = data[33] - 1;
            int packetSize = BitConverter.ToInt16(new byte[] { data[3], data[2] }, 0);
            string packetAccount;
            string packetIp;
            AccountState accState; // = AccountState.LOGIN_DBBUSY

            // Read account name from packet
            packetAccount = "";
            for (int i = 0, loopTo = iUpper; i <= loopTo; i++)
                packetAccount += Conversions.ToString((char)data[34 + i]);
            client.Account = packetAccount;

            // Read users ip from packet
            packetIp = ((int)data[29]).ToString() + "." + ((int)data[30]).ToString() + "." + ((int)data[31]).ToString() + "." + ((int)data[32]).ToString();

            // Get the client build from packet.
            int bMajor = data[8];
            int bMinor = data[9];
            int bRevision = data[10];
            int clientBuild = BitConverter.ToInt16(new byte[] { data[11], data[12] }, 0);
            string clientLanguage = Conversions.ToString((char)data[24]) + (char)data[23] + (char)data[22] + (char)data[21];
            Console.WriteLine("[{0}] [{1}:{2}] CMD_AUTH_LOGON_CHALLENGE [{3}] [{4}], WoW Version [{5}.{6}.{7}.{8}] [{9}].", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), client.Ip, client.Port, packetAccount, packetIp, bMajor.ToString(), bMinor.ToString(), bRevision.ToString(), clientBuild.ToString(), clientLanguage);

            // DONE: Check if our build can join the server
            // If ((RequiredVersion1 = 0 AndAlso RequiredVersion2 = 0 AndAlso RequiredVersion3 = 0) OrElse
            // (bMajor = RequiredVersion1 AndAlso bMinor = RequiredVersion2 AndAlso bRevision = RequiredVersion3)) AndAlso
            // clientBuild >= RequiredBuildLow AndAlso clientBuild <= RequiredBuildHigh Then
            if (bMajor == 0 & bMinor == 0 & bRevision == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[{0}] [{1}:{2}] Invalid Client", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), client.Ip, client.Port);
                Console.ForegroundColor = ConsoleColor.White;
                var dataResponse = new byte[2];
                dataResponse[0] = (byte)AuthCMD.CMD_AUTH_LOGON_PROOF;
                dataResponse[1] = (byte)AccountState.LOGIN_BADVERSION;
                client.Send(dataResponse, "RS_LOGON_CHALLENGE-FAIL-BADVERSION");
            }
            else if (clientBuild == _Global_Constants.Required_Build_1_12_1 | clientBuild == _Global_Constants.Required_Build_1_12_2 | clientBuild == _Global_Constants.Required_Build_1_12_3)
            {
                // TODO: in the far future should check if the account is expired too
                DataTable result = null;
                try
                {
                    // Get Account info
                    AccountDatabase.Query($"SELECT id, sha_pass_hash, gmlevel, expansion FROM account WHERE username = \"{packetAccount}\";", ref result);

                    // Check Account state
                    if (result.Rows.Count > 0)
                    {
                        accState = AccountDatabase.QuerySQL(
                            (string)Operators.ConcatenateObject(
                                Operators.ConcatenateObject("SELECT id FROM account_banned WHERE id = '", result.Rows[0]["id"]), "';")) 
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
                            Console.WriteLine("[{0}] [{1}:{2}] Account found [{3}]", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), client.Ip, client.Port, packetAccount);
                            var account = new byte[(data[33])];
                            Array.Copy(data, 34, account, 0, data[33]);
                            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(result.Rows[0]["sha_pass_hash"].ToString().Length, 40, false))) // Invalid password type, should always be 40 characters
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("[{0}] [{1}:{2}] Not a valid SHA1 password for account: '{3}' SHA1 Hash: '{4}'", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), client.Ip, client.Port, packetAccount, result.Rows[0]["sha_pass_hash"]);
                                Console.ForegroundColor = ConsoleColor.White;
                                var dataResponse = new byte[2];
                                dataResponse[0] = (byte)AuthCMD.CMD_AUTH_LOGON_PROOF;
                                dataResponse[1] = (byte)AccountState.LOGIN_BAD_PASS;
                                client.Send(dataResponse, "RS_LOGON_CHALLENGE-FAIL-BADPWFORMAT");
                            }
                            else // Bail out with something meaningful
                            {
                                client.Access = (AccessLevel)result.Rows[0]["gmlevel"];
                                var hash = new byte[20];
                                for (int i = 0; i <= 39; i += 2)
                                    hash[i / 2] = (byte)Conversions.ToInteger(Operators.ConcatenateObject("&H", result.Rows[0]["sha_pass_hash"].ToString().Substring(i, 2)));

                                // client.Language = clientLanguage
                                // If Not IsDBNull(result.Rows(0).Item("expansion")) Then
                                // client.Expansion = result.Rows(0).Item("expansion")
                                // Else
                                // client.Expansion = ExpansionLevel.NORMAL
                                // End If

                                try
                                {
                                    client.AuthEngine = new AuthEngineClass();
                                    client.AuthEngine.CalculateX(account, hash);
                                    var dataResponse = new byte[119];
                                    dataResponse[0] = (byte)AuthCMD.CMD_AUTH_LOGON_CHALLENGE;
                                    dataResponse[1] = (byte)AccountState.LOGIN_OK;
                                    dataResponse[2] = (byte)Conversion.Val("&H00");
                                    Array.Copy(client.AuthEngine.PublicB, 0, dataResponse, 3, 32);
                                    dataResponse[35] = (byte)client.AuthEngine.g.Length;
                                    dataResponse[36] = client.AuthEngine.g[0];
                                    dataResponse[37] = 32;
                                    Array.Copy(client.AuthEngine.N, 0, dataResponse, 38, 32);
                                    Array.Copy(client.AuthEngine.Salt, 0, dataResponse, 70, 32);
                                    Array.Copy(AuthEngineClass.CrcSalt, 0, dataResponse, 102, 16);
                                    dataResponse[118] = 0; // Added in 1.12.x client branch? Security Flags (&H0...&H4)?
                                    client.Send(dataResponse, "RS_LOGON_CHALLENGE OK");
                                }
                                catch (Exception ex)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("[{0}] [{1}:{2}] Error loading AuthEngine: {3}{4}", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), client.Ip, client.Port, Environment.NewLine, ex);
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                            }

                            return;
                        }

                    case var case1 when case1 == AccountState.LOGIN_UNKNOWN_ACCOUNT:
                        {
                            Console.WriteLine("[{0}] [{1}:{2}] Account not found [{3}]", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), client.Ip, client.Port, packetAccount);
                            var dataResponse = new byte[2];
                            dataResponse[0] = (byte)AuthCMD.CMD_AUTH_LOGON_PROOF;
                            dataResponse[1] = (byte)AccountState.LOGIN_UNKNOWN_ACCOUNT;
                            client.Send(dataResponse, "RS_LOGON_CHALLENGE-UNKNOWN_ACCOUNT");
                            return;
                        }

                    case var case2 when case2 == AccountState.LOGIN_BANNED:
                        {
                            Console.WriteLine("[{0}] [{1}:{2}] Account banned [{3}]", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), client.Ip, client.Port, packetAccount);
                            var dataResponse = new byte[2];
                            dataResponse[0] = (byte)AuthCMD.CMD_AUTH_LOGON_PROOF;
                            dataResponse[1] = (byte)AccountState.LOGIN_BANNED;
                            client.Send(dataResponse, "RS_LOGON_CHALLENGE-BANNED");
                            return;
                        }

                    case var case3 when case3 == AccountState.LOGIN_NOTIME:
                        {
                            Console.WriteLine("[{0}] [{1}:{2}] Account prepaid time used [{3}]", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), client.Ip, client.Port, packetAccount);
                            var dataResponse = new byte[2];
                            dataResponse[0] = (byte)AuthCMD.CMD_AUTH_LOGON_PROOF;
                            dataResponse[1] = (byte)AccountState.LOGIN_NOTIME;
                            client.Send(dataResponse, "RS_LOGON_CHALLENGE-NOTIME");
                            return;
                        }

                    case var case4 when case4 == AccountState.LOGIN_ALREADYONLINE:
                        {
                            Console.WriteLine("[{0}] [{1}:{2}] Account already logged in the game [{3}]", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), client.Ip, client.Port, packetAccount);
                            var dataResponse = new byte[2];
                            dataResponse[0] = (byte)AuthCMD.CMD_AUTH_LOGON_PROOF;
                            dataResponse[1] = (byte)AccountState.LOGIN_ALREADYONLINE;
                            client.Send(dataResponse, "RS_LOGON_CHALLENGE-ALREADYONLINE");
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
                            Console.WriteLine("[{0}] [{1}:{2}] Account error [{3}]", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), client.Ip, client.Port, packetAccount);
                            var dataResponse = new byte[2];
                            dataResponse[0] = (byte)AuthCMD.CMD_AUTH_LOGON_PROOF;
                            dataResponse[1] = (byte)AccountState.LOGIN_FAILED;
                            client.Send(dataResponse, "RS_LOGON_CHALLENGE-FAILED");
                            return;
                        }
                }
            }
            else if (!string.IsNullOrEmpty(FileSystem.Dir("Updates/wow-patch-" + Conversion.Val("&H" + Conversion.Hex(data[12]) + Conversion.Hex(data[11])) + "-" + (char)data[24] + (char)data[23] + (char)data[22] + (char)data[21] + ".mpq")))
            {
                // Send UPDATE_MPQ
                Console.WriteLine("[{0}] [{1}:{2}] CMD_XFER_INITIATE [" + (char)data[6] + (char)data[5] + (char)data[4] + " " + data[8] + "." + data[9] + "." + data[10] + "." + Conversion.Val("&H" + Conversion.Hex(data[12]) + Conversion.Hex(data[11])) + " " + (char)data[15] + (char)data[14] + (char)data[13] + " " + (char)data[19] + (char)data[18] + (char)data[17] + " " + (char)data[24] + (char)data[23] + (char)data[22] + (char)data[21] + "]", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), client.Ip, client.Port);
                client.UpdateFile = "Updates/wow-patch-" + Conversion.Val("&H" + Conversion.Hex(data[12]) + Conversion.Hex(data[11])) + "-" + (char)data[24] + (char)data[23] + (char)data[22] + (char)data[21] + ".mpq";
                var dataResponse = new byte[31];
                dataResponse[0] = (byte)AuthCMD.CMD_XFER_INITIATE;
                // Name Len 0x05 -> sizeof(Patch)
                int i = 1;
                _Converter.ToBytes(Conversions.ToByte(5), dataResponse, ref i);
                // Name 'Patch'
                _Converter.ToBytes("Patch", dataResponse, ref i);
                // Size 0x34 C4 0D 00 = 902,196 byte (180->181 enGB)
                _Converter.ToBytes(Conversions.ToInteger(FileSystem.FileLen(client.UpdateFile)), dataResponse, ref i);
                // Unknown 0x0 always
                _Converter.ToBytes(0, dataResponse, ref i);
                // MD5 CheckSum
                var md5 = new MD5CryptoServiceProvider();
                byte[] buffer;
                var fs = new FileStream(client.UpdateFile, FileMode.Open);
                var r = new BinaryReader(fs);
                buffer = r.ReadBytes((int)FileSystem.FileLen(client.UpdateFile));
                r.Close();
                // fs.Close()
                var result = md5.ComputeHash(buffer);
                Array.Copy(result, 0, dataResponse, 15, 16);
                client.Send(dataResponse, "RS_LOGON_CHALLENGE-CMD-XFER-INITIATE");
            }
            else
            {
                // Send BAD_VERSION
                Console.WriteLine("[{0}] [{1}:{2}] WRONG_VERSION [" + (char)data[6] + (char)data[5] + (char)data[4] + " " + data[8] + "." + data[9] + "." + data[10] + "." + Conversion.Val("&H" + Conversion.Hex(data[12]) + Conversion.Hex(data[11])) + " " + (char)data[15] + (char)data[14] + (char)data[13] + " " + (char)data[19] + (char)data[18] + (char)data[17] + " " + (char)data[24] + (char)data[23] + (char)data[22] + (char)data[21] + "]", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), client.Ip, client.Port);
                var dataResponse = new byte[2];
                dataResponse[0] = (byte)AuthCMD.CMD_AUTH_LOGON_PROOF;
                dataResponse[1] = (byte)AccountState.LOGIN_BADVERSION;
                client.Send(dataResponse, "RS_LOGON_CHALLENGE-WRONG-VERSION");
            }
        }

        public void On_RS_LOGON_PROOF(ref byte[] data, ref ClientClass client)
        {
            Console.WriteLine("[{0}] [{1}:{2}] CMD_AUTH_LOGON_PROOF", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), client.Ip, client.Port);
            var a = new byte[32];
            Array.Copy(data, 1, a, 0, 32);
            var m1 = new byte[20];
            Array.Copy(data, 33, m1, 0, 20);
            // Dim CRC_Hash(19) As Byte
            // Array.Copy(data, 53, CRC_Hash, 0, 20)
            // Dim NumberOfKeys as Byte = data(73)
            // Dim unk as Byte = data(74)

            // Calculate U and M1
            client.AuthEngine.CalculateU(a);
            client.AuthEngine.CalculateM1();
            // Client.AuthEngine.CalculateCRCHash()

            // Check M1=ClientM1
            bool passCheck = true;
            for (byte i = 0; i <= 19; i++)
            {
                if (m1[i] != client.AuthEngine.M1[i])
                {
                    passCheck = false;
                    break;
                }
            }

            if (!passCheck)
            {
                // Wrong pass
                Console.WriteLine("[{0}] [{1}:{2}] Wrong password for user {3}.", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), client.Ip, client.Port, client.Account);
                var dataResponse = new byte[2];
                dataResponse[0] = (byte)AuthCMD.CMD_AUTH_LOGON_PROOF;
                dataResponse[1] = (byte)AccountState.LOGIN_BAD_PASS;
                client.Send(dataResponse, "RS_LOGON_PROOF WRONGPASS");
            }
            else
            {
                client.AuthEngine.CalculateM2(m1);
                var dataResponse = new byte[26];
                dataResponse[0] = (byte)AuthCMD.CMD_AUTH_LOGON_PROOF;
                dataResponse[1] = (byte)AccountState.LOGIN_OK;
                Array.Copy(client.AuthEngine.M2, 0, dataResponse, 2, 20);
                dataResponse[22] = 0;
                dataResponse[23] = 0;
                dataResponse[24] = 0;
                dataResponse[25] = 0;
                client.Send(dataResponse, "RS_LOGON_PROOF OK");

                // Set SSHash in DB
                string sshash = "";

                // For i as Integer = 0 To client.AuthEngine.SS_Hash.Length - 1
                for (int i = 0; i <= 40 - 1; i++)
                    sshash = client.AuthEngine.SsHash[i] < 16 ? sshash + "0" + Conversion.Hex(client.AuthEngine.SsHash[i]) : sshash + Conversion.Hex(client.AuthEngine.SsHash[i]);
                AccountDatabase.Update($"UPDATE account SET sessionkey = '{sshash}', last_ip = '{client.Ip}', last_login = '{Strings.Format(DateAndTime.Now, "yyyy-MM-dd")}' WHERE username = '{client.Account}';");
                Console.WriteLine("[{0}] [{1}:{2}] Auth success for user {3}. [{4}]", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), client.Ip, client.Port, client.Account, sshash);
            }
        }

        public void On_RS_REALMLIST(ref byte[] data, ref ClientClass client)
        {
            Console.WriteLine("[{0}] [{1}:{2}] CMD_REALM_LIST", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), client.Ip, client.Port);
            int packetLen = 0;
            int characterCount = 0;
            DataTable result = null;
            DataTable countresult = null;

            // Retrieve the Account ID
            AccountDatabase.Query($"SELECT id FROM account WHERE username = \"{client.Account}\";", ref result);

            // Fetch RealmList Data
            AccountDatabase.Query(string.Format("SELECT * FROM realmlist;"), ref result);
            foreach (DataRow row in result.Rows)
                packetLen = packetLen + Strings.Len(row["address"]) + Strings.Len(row["name"]) + 1 + Strings.Len(Strings.Format(row["port"], "0")) + 14;
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
            dataResponse[7] = (byte)result.Rows.Count;
            dataResponse[8] = 0;
            int tmp = 8;
            foreach (DataRow host in result.Rows)
            {

                // Get Number of Characters for the Realm
                AccountDatabase.Query($"SELECT * FROM realmcharacters WHERE realmid = \"{Conversions.ToInteger(host["id"])}\" AND acctid = \"{Conversions.ToInteger(result.Rows[0]["id"])}\";", ref countresult);
                if (countresult.Rows.Count > 0)
                {
                    characterCount = Conversions.ToInteger(countresult.Rows[0]["numchars"]);
                }

                // (uint8) Realm Icon
                // 0 -> Normal; 1 -> PvP; 6 -> RP; 8 -> RPPvP;
                _Converter.ToBytes(Conversions.ToByte(host["icon"]), dataResponse, ref tmp);
                // (uint8) IsLocked
                // 0 -> none; 1 -> locked
                _Converter.ToBytes(Conversions.ToByte(0), dataResponse, ref tmp);
                // (uint8) unk
                _Converter.ToBytes(Conversions.ToByte(0), dataResponse, ref tmp);
                // (uint8) unk
                _Converter.ToBytes(Conversions.ToByte(0), dataResponse, ref tmp);
                // (uint8) Realm Color
                // 0 -> Green; 1 -> Red; 2 -> Offline;
                _Converter.ToBytes(Conversions.ToByte(host["realmflags"]), dataResponse, ref tmp);
                // (string) Realm Name (zero terminated)
                _Converter.ToBytes(Conversions.ToString(host["name"]), dataResponse, ref tmp);
                _Converter.ToBytes(Conversions.ToByte(0), dataResponse, ref tmp); // \0
                                                                              // (string) Realm Address ("ip:port", zero terminated)
                _Converter.ToBytes(Operators.ConcatenateObject(Operators.ConcatenateObject(host["address"], ":"), host["port"]).ToString(), dataResponse, ref tmp);
                _Converter.ToBytes(Conversions.ToByte(0), dataResponse, ref tmp); // \0
                                                                              // (float) Population
                                                                              // 400F -> Full; 5F -> Medium; 1.6F -> Low; 200F -> New; 2F -> High
                                                                              // 00 00 48 43 -> Recommended
                                                                              // 00 00 C8 43 -> Full
                                                                              // 9C C4 C0 3F -> Low
                                                                              // BC 74 B3 3F -> Low
                _Converter.ToBytes(Conversions.ToSingle(host["population"]), dataResponse, ref tmp);
                // (byte) Number of character at this realm for this account
                _Converter.ToBytes(Conversions.ToByte(characterCount), dataResponse, ref tmp);
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
                _Converter.ToBytes(Conversions.ToByte(host["timezone"]), dataResponse, ref tmp);
                // (byte) Unknown (may be 2 -> TestRealm, / 6 -> ?)
                _Converter.ToBytes(Conversions.ToByte(0), dataResponse, ref tmp);
            }

            dataResponse[tmp] = 2; // 2=list of realms 0=wizard
            dataResponse[tmp + 1] = 0;
            client.Send(dataResponse, "RS-REALMLIST");
        }

        public void On_CMD_XFER_CANCEL(ref byte[] data, ref ClientClass client)
        {
            // TODO: data parameter is never used
            Console.WriteLine("[{0}] [{1}:{2}] CMD_XFER_CANCEL", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), client.Ip, client.Port);
            client.Socket.Close();
        }

        public void On_CMD_XFER_ACCEPT(ref byte[] data, ref ClientClass client)
        {
            // TODO: data parameter is never used
            Console.WriteLine("[{0}] [{1}:{2}] CMD_XFER_ACCEPT", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), client.Ip, client.Port);
            int tmp; // = 1
            byte[] buffer;
            int filelen;
            filelen = (int)FileSystem.FileLen(client.UpdateFile);
            int fileOffset = 0;
            var fs = new FileStream(client.UpdateFile, FileMode.Open, FileAccess.Read);
            var r = new BinaryReader(fs);
            buffer = r.ReadBytes(filelen);
            r.Close();
            // fs.Close()

            if (filelen <= 1500)
            {
                tmp = 1;
                var dataResponse = new byte[filelen + 2 + 1];
                dataResponse[0] = (byte)AuthCMD.CMD_XFER_DATA;
                _Converter.ToBytes(Conversions.ToShort(filelen), dataResponse, ref tmp);
                Array.Copy(buffer, 0, dataResponse, 3, filelen);
                client.Send(dataResponse, "CMD-XFER-ACCEPT-3");
            }
            else
            {
                byte[] dataResponse;
                while (filelen > 1500)
                {
                    tmp = 1;
                    dataResponse = new byte[1503];
                    dataResponse[0] = (byte)AuthCMD.CMD_XFER_DATA;
                    _Converter.ToBytes(Conversions.ToShort(1500), dataResponse, ref tmp);
                    Array.Copy(buffer, fileOffset, dataResponse, 3, 1500);
                    filelen -= 1500;
                    fileOffset += 1500;
                    client.Send(dataResponse, "CMD-XFER-ACCEPT-1");
                }

                tmp = 1;
                dataResponse = new byte[filelen + 2 + 1];
                dataResponse[0] = (byte)AuthCMD.CMD_XFER_DATA;
                _Converter.ToBytes(Conversions.ToShort(filelen), dataResponse, ref tmp);
                Array.Copy(buffer, fileOffset, dataResponse, 3, filelen);
                client.Send(dataResponse, "CMD-XFER-ACCEPT-2");
            }
            // Client.Socket.Close()
        }

        public void On_CMD_XFER_RESUME(ref byte[] data, ref ClientClass client)
        {
            Console.WriteLine("[{0}] [{1}:{2}] CMD_XFER_RESUME", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), client.Ip, client.Port);
            int tmp = 1;
            byte[] buffer;
            int filelen;
            filelen = (int)FileSystem.FileLen(client.UpdateFile);
            int fileOffset;
            fileOffset = _Converter.ToInt32(data, ref tmp);
            filelen -= fileOffset;
            var fs = new FileStream(client.UpdateFile, FileMode.Open, FileAccess.Read);
            var r = new BinaryReader(fs);
            r.ReadBytes(fileOffset);
            buffer = r.ReadBytes(filelen);
            r.Close();
            // fs.Close()
            fileOffset = 0;
            if (filelen <= 1500)
            {
                tmp = 1;
                var dataResponse = new byte[filelen + 2 + 1];
                dataResponse[0] = (byte)AuthCMD.CMD_XFER_DATA;
                _Converter.ToBytes(Conversions.ToShort(filelen), dataResponse, ref tmp);
                Array.Copy(buffer, 0, dataResponse, 3, filelen);
                client.Send(dataResponse, "XFER-RESUME-XFER-DATA");
            }
            else
            {
                byte[] dataResponse;
                while (filelen > 1500)
                {
                    tmp = 1;
                    dataResponse = new byte[1503];
                    dataResponse[0] = (byte)AuthCMD.CMD_XFER_DATA;
                    _Converter.ToBytes(Conversions.ToShort(1500), dataResponse, ref tmp);
                    Array.Copy(buffer, fileOffset, dataResponse, 3, 1500);
                    filelen -= 1500;
                    fileOffset += 1500;
                    client.Send(dataResponse, "XFER-RESUME");
                }

                tmp = 1;
                dataResponse = new byte[filelen + 2 + 1];
                dataResponse[0] = (byte)AuthCMD.CMD_XFER_DATA;
                _Converter.ToBytes(Conversions.ToShort(filelen), dataResponse, ref tmp);
                Array.Copy(buffer, fileOffset, dataResponse, 3, filelen);
                client.Send(dataResponse, "XFER-RESUME-XFER-DATALARGER");
            }
            // Client.Socket.Close()
        }

        public void DumpPacket(ref byte[] data, ref ClientClass client)
        {
            string buffer = "";
            if (client is null)
            {
                buffer += string.Format("[{0}] DEBUG: Packet Dump{1}", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), Environment.NewLine);
            }
            else
            {
                buffer += string.Format("[{0}] [{1}:{2}] DEBUG: Packet Dump{3}", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"), client.Ip, client.Port, Environment.NewLine);
            }

            int j;
            if (data.Length % 16 == 0)
            {
                var loopTo = data.Length - 1;
                for (j = 0; j <= loopTo; j += 16)
                {
                    buffer += "|  " + BitConverter.ToString(data, j, 16).Replace("-", " ");
                    buffer += " |  " + Encoding.ASCII.GetString(data, j, 16).Replace(Constants.vbTab, "?").Replace(Constants.vbBack, "?").Replace(Constants.vbCr, "?").Replace(Constants.vbFormFeed, "?").Replace(Constants.vbLf, "?") + " |" + Environment.NewLine;
                }
            }
            else
            {
                var loopTo1 = data.Length - 1 - 16;
                for (j = 0; j <= loopTo1; j += 16)
                {
                    buffer += "|  " + BitConverter.ToString(data, j, 16).Replace("-", " ");
                    buffer += " |  " + Encoding.ASCII.GetString(data, j, 16).Replace(Constants.vbTab, "?").Replace(Constants.vbBack, "?").Replace(Constants.vbCr, "?").Replace(Constants.vbFormFeed, "?").Replace(Constants.vbLf, "?") + " |" + Environment.NewLine;
                }

                buffer += "|  " + BitConverter.ToString(data, j, data.Length % 16).Replace("-", " ");
                buffer += new string(' ', (16 - data.Length % 16) * 3);
                buffer += " |  " + Encoding.ASCII.GetString(data, j, data.Length % 16).Replace(Constants.vbTab, "?").Replace(Constants.vbBack, "?").Replace(Constants.vbCr, "?").Replace(Constants.vbFormFeed, "?").Replace(Constants.vbLf, "?");
                buffer += new string(' ', 16 - data.Length % 16);
                buffer += " |" + Environment.NewLine;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(buffer);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private void WorldServer_Status_Report()
        {
            var result1 = new DataTable();
            int returnValues;
            returnValues = AccountDatabase.Query(string.Format("SELECT * FROM realmlist WHERE allowedSecurityLevel < '1';"), ref result1);
            if (returnValues > (byte)SQL.ReturnState.Success) // Ok, An error occurred
            {
                Console.WriteLine("[{0}] An SQL Error has occurred", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"));
                Console.WriteLine("*************************");
                Console.WriteLine("* Press any key to exit *");
                Console.WriteLine("*************************");
                Console.ReadKey();
                Environment.Exit(0);
            }

            Console.WriteLine();
            Console.WriteLine("[{0}] Loading known game servers...", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"));
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            foreach (DataRow row in result1.Rows)
                Console.WriteLine("     [{1}] at {0}:{2} - {3}", row["address"].ToString().PadRight(6),
                    row["name"].ToString().PadRight(6), 
                    Strings.Format(row["port"]).PadRight(6), 
                    _Global_Constants.WorldServerStatus[(byte)(Conversion.Int(row["realmflags"]))].PadRight(6));
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public void Start()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            AssemblyTitleAttribute assemblyTitleAttribute = (AssemblyTitleAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0];
            Console.Title = $"{assemblyTitleAttribute.Title} v{Assembly.GetExecutingAssembly().GetName().Version}";
            Console.ForegroundColor = ConsoleColor.Yellow;
            AssemblyProductAttribute assemblyProductAttribute = (AssemblyProductAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0];
            Console.WriteLine("{0}", assemblyProductAttribute.Product);
            AssemblyCopyrightAttribute assemblyCopyrightAttribute = (AssemblyCopyrightAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0];
            Console.WriteLine(assemblyCopyrightAttribute.Copyright);
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  __  __      _  _  ___  ___  ___   __   __ ___               ");
            Console.WriteLine(@" |  \/  |__ _| \| |/ __|/ _ \/ __|  \ \ / /| _ )      We Love ");
            Console.WriteLine(@" | |\/| / _` | .` | (_ | (_) \__ \   \ V / | _ \   Vanilla Wow");
            Console.WriteLine(@" |_|  |_\__,_|_|\_|\___|\___/|___/    \_/  |___/              ");
            Console.WriteLine("                                                              ");
            Console.WriteLine(" Website / Forum / Support: https://getmangos.eu/             ");
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.ForegroundColor = ConsoleColor.White;
            var attributeType = typeof(AssemblyTitleAttribute);
            Console.Write(Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title);
            Console.WriteLine(" version {0}", Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("[{0}] Realm Server Starting...", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"));
            LoadConfig();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Log.WriteLine(LogType.INFORMATION, "Running from: {0}", AppDomain.CurrentDomain.BaseDirectory);
            Console.ForegroundColor = ConsoleColor.Gray;
            AccountDatabase.SQLMessage += SqlEventHandler;
            int ReturnValues;
            ReturnValues = AccountDatabase.Connect();
            if (ReturnValues > (int)SQL.ReturnState.Success)   // Ok, An error occurred
            {
                Console.WriteLine("[{0}] An SQL Error has occurred", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"));
                Console.WriteLine("*************************");
                Console.WriteLine("* Press any key to exit *");
                Console.WriteLine("*************************");
                Console.ReadKey();
                Environment.Exit(0);
            }

            if (_CommonGlobalFunctions.CheckRequiredDbVersion(AccountDatabase, ServerDb.Realm) == false) // Check the Database version, exit if its wrong
            {
                if (true)
                {
                    Console.WriteLine("*************************");
                    Console.WriteLine("* Press any key to exit *");
                    Console.WriteLine("*************************");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
            }

            RealmServerClass = _RealmServerClassFactory.Create(this);
            RealmServerClass.Start();
            GC.Collect();
            WorldServer_Status_Report();
        }

        public uint Ip2Int(string ip)
        {
            if (ip.Split(".").Length != 4)
                return 0U;
            try
            {
                var ipBytes = new byte[4];
                ipBytes[0] = Conversions.ToByte(ip.Split(".")[3]);
                ipBytes[1] = Conversions.ToByte(ip.Split(".")[2]);
                ipBytes[2] = Conversions.ToByte(ip.Split(".")[1]);
                ipBytes[3] = Conversions.ToByte(ip.Split(".")[0]);
                return BitConverter.ToUInt32(ipBytes, 0);
            }
            catch
            {
                return 0U;
            }
        }

        private void GenericExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            Log.WriteLine(LogType.CRITICAL, ex.ToString() + Environment.NewLine);
            Log.WriteLine(LogType.FAILED, "Unexpected error has occured. An 'RealmServer-Error-yyyy-mmm-d-h-mm.log' file has been created. Check your log folder for more information.");
            TextWriter tw;
            tw = new StreamWriter(new FileStream(string.Format("RealmServer-Error-{0}.log", Strings.Format(DateAndTime.Now, "yyyy-MMM-d-H-mm")), FileMode.Create));
            tw.Write(ex.ToString());
            tw.Close();
        }
    }
}