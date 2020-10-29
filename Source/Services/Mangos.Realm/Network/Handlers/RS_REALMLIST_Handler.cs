using Mangos.Realm.Models;
using Mangos.Realm.Network.Readers;
using Mangos.Realm.Network.Responses;
using Mangos.Realm.Network.Writers;
using Mangos.Storage.Account;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Realm.Network.Handlers
{
    public class RS_REALMLIST_Handler : IPacketHandler
    {
        private readonly IRealmStorage realmStorage;

        private readonly RS_REALMLIST_Reader RS_REALMLIST_Reader;
        private readonly AUTH_REALMLIST_Writer AUTH_REALMLIST_Writer;

        public RS_REALMLIST_Handler(
            IRealmStorage realmStorage,
            RS_REALMLIST_Reader RS_REALMLIST_Reader, 
            AUTH_REALMLIST_Writer AUTH_REALMLIST_Writer)
        {
            this.realmStorage = realmStorage;
            this.RS_REALMLIST_Reader = RS_REALMLIST_Reader;
            this.AUTH_REALMLIST_Writer = AUTH_REALMLIST_Writer;
        }

        public async Task HandleAsync(ChannelReader<byte> reader, ChannelWriter<byte> writer, ClientModel clientModel)
        {
            var request = await RS_REALMLIST_Reader.ReadAsync(reader);

            var realmList = await realmStorage.GetRealmListAsync();

            var realms = realmList.Select(x => new AUTH_REALMLIST.Realm(
                x.address,
                x.name,
                x.port,
                x.timezone,
                x.icon,
                x.realmflags,
                x.population,
                x.numchars));

            await AUTH_REALMLIST_Writer.WriteAsync(writer, new AUTH_REALMLIST(request.Unk, realms.ToArray()));
        }
    }
}
