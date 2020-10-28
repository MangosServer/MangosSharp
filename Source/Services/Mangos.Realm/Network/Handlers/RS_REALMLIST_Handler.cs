using Mangos.Common.Enums.Authentication;
using Mangos.Network.Tcp.Extensions;
using Mangos.Realm.Models;
using Mangos.Storage.Account;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Realm.Network.Handlers
{
    public class RS_REALMLIST_Handler : IPacketHandler
    {
        private readonly IRealmStorage realmStorage;
        private readonly Converter converter;

        public RS_REALMLIST_Handler(IRealmStorage realmStorage, Converter converter)
        {
            this.realmStorage = realmStorage;
            this.converter = converter;
        }

        public async Task HandleAsync(ChannelReader<byte> reader, ChannelWriter<byte> writer, ClientModel clientModel)
        {
            var body = await reader.ReadArrayAsync(4);
            var data = new byte[1].Concat(body).ToArray();

            int packetLen = 0;

            // Fetch RealmList Data
            var realmList = await realmStorage.GetRealmListAsync();
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
                var characterCount = await realmStorage.GetNumcharsAsync(realmListItem.id);

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
    }
}
