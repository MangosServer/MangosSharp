using Mangos.Common.Enums.Authentication;
using Mangos.Network.Tcp;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Realm.Network
{
    public class TcpClient : ITcpClient
    {
        private readonly PipelineMap pipelineMap;
        private readonly ClientContext clientContext;

        public TcpClient(PipelineMap pipelineMap, ClientContext clientContext)
        {
            this.pipelineMap = pipelineMap;
            this.clientContext = clientContext;
        }

        public async void HandleAsync(
            ChannelReader<byte> reader, 
            ChannelWriter<byte> writer, 
            CancellationToken cancellationToken)
        {
            while(!cancellationToken.IsCancellationRequested)
            {
                await ExecuteAsync(reader, writer, cancellationToken);
            }
        }

        private async Task ExecuteAsync(
            ChannelReader<byte> reader, 
            ChannelWriter<byte> writer, 
            CancellationToken cancellationToken)
        {
            var opcode = await reader.ReadAsync(cancellationToken);
            var authCMD = (AuthCMD) opcode;
            var pipeline = pipelineMap.GetPipeline(authCMD);
            await pipeline.ExecuteAsync(reader, writer, clientContext);
        }
    }
}
