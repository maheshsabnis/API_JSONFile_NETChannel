using System.Threading.Channels;

namespace API_JSONFile_NETChannel.FileProcessor
{
    public class ChannellingJob
    {
        private readonly Channel<string> _channel;
        private readonly JsonFileProcessor _processor;

        public ChannellingJob(JsonFileProcessor processor, Channel<string> channel)
        {
            _channel = channel;
            _processor = processor;
        }

        /// <summary>
        /// Write File to Channel
        /// </summary>
        /// <param name="file"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task EnqueueFileAsync(string file, CancellationToken cancellationToken = default)
        {
            await _channel.Writer.WriteAsync(file);
            var res = await _channel.Writer.WaitToWriteAsync(cancellationToken);
        }

        /// <summary>
        /// Read File from Channel
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        public async Task DequeueFilesAsync(CancellationToken cancellationToken, Channel<string> channel)
        {
            /// wait for the data available to read
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
