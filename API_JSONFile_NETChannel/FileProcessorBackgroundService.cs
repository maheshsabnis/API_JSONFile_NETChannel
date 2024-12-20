
using API_JSONFile_NETChannel.FileProcessor;
using System.Threading.Channels;

namespace API_JSONFile_NETChannel
{

    /// <summary>
    /// The Consumer
    /// </summary>
    public class FileProcessorBackgroundService : BackgroundService
    {
          private readonly IServiceProvider _serviceProvider;
          Channel<string> _channel;
        public FileProcessorBackgroundService(IServiceProvider serviceProvider, Channel<string> channel)
        {
           _serviceProvider = serviceProvider;
            _channel = channel;
        }

        /// <summary>
        /// Method to Consume data from Channel by invoking the DequeueFilesAsync() method 
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
           while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var jsonFileProcessor = scope.ServiceProvider.GetRequiredService<JsonFileProcessor>();
                        var channellingJob = new ChannellingJob(jsonFileProcessor, _channel);
                        // Conditional execution
                        if (ShouldExecute())
                        {
                            await channellingJob.DequeueFilesAsync(stoppingToken,_channel);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it accordingly
                   throw;
                }

                // Wait for a specified interval before checking again
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // Adjust the interval as needed
            }
        }

        private bool ShouldExecute()
        {
            const string uploadsFolderPath = "uploads";
            return Directory.Exists(uploadsFolderPath) && Directory.GetFiles(uploadsFolderPath).Length > 0;
        }
    }
}
