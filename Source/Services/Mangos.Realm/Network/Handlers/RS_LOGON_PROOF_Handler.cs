using Mangos.Common.Enums.Authentication;
using Mangos.Common.Enums.Global;
using Mangos.Loggers;
using Mangos.Network.Tcp.Extensions;
using Mangos.Realm.Models;
using Mangos.Storage.Account;
using Microsoft.VisualBasic;
using System;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Realm.Network.Handlers
{
    public class RS_LOGON_PROOF_Handler : IPacketHandler
    {
        private readonly ILogger logger;
        private readonly IRealmStorage realmStorage;

        public RS_LOGON_PROOF_Handler(ILogger logger, IRealmStorage realmStorage)
        {
            this.logger = logger;
            this.realmStorage = realmStorage;
        }

        public async Task HandleAsync(ChannelReader<byte> reader, ChannelWriter<byte> writer, ClientModel clientModel)
        {
            var body = await reader.ReadArrayAsync(74);
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
            clientModel.ClientAuthEngine.CalculateU(a);
            clientModel.ClientAuthEngine.CalculateM1();
            // AuthEngine.CalculateCRCHash()

            // Check M1=ClientM1
            bool passCheck = true;
            for (byte i = 0; i <= 19; i++)
            {
                if (m1[i] != clientModel.ClientAuthEngine.M1[i])
                {
                    passCheck = false;
                    break;
                }
            }

            if (!passCheck)
            {
                // Wrong pass
                logger.Debug("Wrong password for user {0}.", clientModel.AccountName);
                var dataResponse = new byte[2];
                dataResponse[0] = (byte)AuthCMD.CMD_AUTH_LOGON_PROOF;
                dataResponse[1] = (byte)AccountState.LOGIN_BAD_PASS;
                await writer.WriteAsync(dataResponse);
            }
            else
            {
                clientModel.ClientAuthEngine.CalculateM2(m1);
                var dataResponse = new byte[26];
                dataResponse[0] = (byte)AuthCMD.CMD_AUTH_LOGON_PROOF;
                dataResponse[1] = (byte)AccountState.LOGIN_OK;
                Array.Copy(clientModel.ClientAuthEngine.M2, 0, dataResponse, 2, 20);
                dataResponse[22] = 0;
                dataResponse[23] = 0;
                dataResponse[24] = 0;
                dataResponse[25] = 0;
                await writer.WriteAsync(dataResponse);

                // Set SSHash in DB
                string sshash = "";

                // For i as Integer = 0 To AuthEngine.SS_Hash.Length - 1
                for (int i = 0; i <= 40 - 1; i++)
                    sshash = clientModel.ClientAuthEngine.SsHash[i] < 16 ? sshash + "0" + Conversion.Hex(clientModel.ClientAuthEngine.SsHash[i]) : sshash + Conversion.Hex(clientModel.ClientAuthEngine.SsHash[i]);
                await realmStorage.UpdateAccountAsync(sshash, clientModel.RemoteEnpoint.Address.ToString(), Strings.Format(DateAndTime.Now, "yyyy-MM-dd"), clientModel.AccountName);
                logger.Debug("Auth success for user {0} [{1}]", clientModel.AccountName, sshash);
            }
        }
    }
}
