using System.Threading.Channels;

namespace Grabaciones.Services.Econtact
{
    public class SimpleRateLimiter
    {
        private readonly Channel<bool> _channel;

        public SimpleRateLimiter(int permitsPerSecond)
        {
            _channel = Channel.CreateBounded<bool>(new BoundedChannelOptions(permitsPerSecond)
            {
                FullMode = BoundedChannelFullMode.Wait
            });

            _ = Task.Run(async () =>
            {
                while (true)
                {
                    await _channel.Writer.WriteAsync(true);
                    await Task.Delay(1000 / permitsPerSecond);
                }
            });
        }

        public async Task WaitAsync()
        {
            await _channel.Reader.ReadAsync();
        }
    }
}
