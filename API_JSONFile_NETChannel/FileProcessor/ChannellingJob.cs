using API_JSONFile_NETChannel.Models;
using System.Threading.Channels;

namespace API_JSONFile_NETChannel.FileProcessor
{
    public class ChannellingJob
    {
        private readonly Channel<string> _channel;

        JsonFileProcessor _processor;

        private ILogger _logger;

        public ChannellingJob(EcommContext context, JsonFileProcessor processor, Channel<string> channel)
        {
             _channel = channel;
            _processor = processor;

        }

        public async Task EnqueueFileAsync(string file, CancellationToken cancellationToken=default)
        { 
            await _channel.Writer.WriteAsync(file);
            var res =  await _channel.Writer.WaitToWriteAsync(cancellationToken);
        }
        public async Task DequeueFilesAsync(CancellationToken cancellationToken, Channel<string> channel) 
        {
            var res = await _channel.Reader.WaitToReadAsync(); 
            while (res)
            {
                await foreach (var filePath in _channel.Reader.ReadAllAsync())
                {
                    await _processor.ProcessFilesAsync(filePath);
                }
            }
        }
    }
}
