using System.Threading.Channels;


namespace ChannelsExample
{
    
    // Your record type
    public record ChannelRequest(string Message);

    // Background service that processes requests from the channel
    public class Processor : BackgroundService
    {
        private readonly Channel<ChannelRequest> _channel;

        public Processor(Channel<ChannelRequest> channel)
        {
            _channel = channel;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Processor started...");

            // Keep reading messages until cancellation is requested
            while (await _channel.Reader.WaitToReadAsync(stoppingToken))
            {
                var request = await _channel.Reader.ReadAsync(stoppingToken);

                // Simulate work
                await Task.Delay(5000, stoppingToken);

                Console.WriteLine($"Processed: {request.Message}");
            }

            Console.WriteLine("Processor stopped...");
        }
    }
}
