using ChannelsExample.Worker;
using System.Threading.Channels;

namespace ChannelsExample
{
    public record FileImportRequest
    {
        public string RequestId { get; init; } = default!;
        public MemoryStream FileData { get; init; } = default!;
    }

    public record FileImportResponseModel
    {
        public string RequestId { get; init; } = default!;
        public string FileName { get; init; } = default!;
        public long FileSize { get; init; }
        public string Status { get; init; } = "Scheduled for Processing";
    }

    public class FileProcessor : BackgroundService
    {
        private readonly Channel<FileImportRequest> _channel;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FileProcessor> _logger;

        public FileProcessor(
            Channel<FileImportRequest> channel,
            IServiceProvider serviceProvider,
            ILogger<FileProcessor> logger)
        {
            _channel = channel;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("File processor started...");

            await foreach (var request in _channel.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    _logger.LogInformation(
                        "Processing Request {RequestId}, Size: {FileSize} bytes",
                        request.RequestId,
                        request.FileData.Length);

                    // Create a new DI scope per request
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var worker = scope.ServiceProvider.GetRequiredService<IFileImportWorker>();
                        await worker.Import(request);
                    }

                    _logger.LogInformation("Successfully processed Request {RequestId}", request.RequestId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing Request {RequestId}", request.RequestId);
                    // Optionally push to a dead-letter queue or retry mechanism
                }
                finally
                {
                    request.FileData?.Dispose(); // Ensure stream is freed
                }
            }

            _logger.LogInformation("File processor stopped.");
        }
    }
}
