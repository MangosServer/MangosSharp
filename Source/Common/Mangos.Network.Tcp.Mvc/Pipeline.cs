using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Network.Tcp.Mvc
{
    public class Pipeline<TRequest, TContext, TResponse> : IPipeline<TContext>
    {
        private readonly IPacketReader<TRequest> requestReader;
        private readonly IPacketHandler<TRequest, TContext, TResponse> packetHandler;
        private readonly IPacketWriter<TResponse> responseWriter;

        public Pipeline(
            IPacketReader<TRequest> requestReader, 
            IPacketHandler<TRequest, TContext, TResponse> packetHandler, 
            IPacketWriter<TResponse> responseWriter)
        {
            this.requestReader = requestReader;
            this.packetHandler = packetHandler;
            this.responseWriter = responseWriter;
        }

        public async Task ExecuteAsync(
            ChannelReader<byte> reader, 
            ChannelWriter<byte> writer,
            TContext context)
        {
            var request = await requestReader.ReadAsync(reader);
            var response = await packetHandler.HandleAsync(request, context);
            await responseWriter.WriteAsync(response, writer);
        }
    }
}
