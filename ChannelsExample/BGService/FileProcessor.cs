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

        public FileProcessor(Channel<FileImportRequest> channel)
        {
            _channel = channel;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("File processor started...");

            await foreach (var request in _channel.Reader.ReadAllAsync(stoppingToken))
            {
                Console.WriteLine($"Processing Request {request.RequestId}, Size: {request.FileData.Length} bytes");

                // Simulate work (e.g., saving file, parsing, etc.)
                await Task.Delay(2000, stoppingToken);
            }

            Console.WriteLine("File processor stopped.");
        }
    }
}
