using System.Threading.Channels;

namespace UTB.Minute.WebApi.Services;

public class NotificationService
{
    private readonly List<Channel<string>> _clients = new();
    private readonly object _syncLock = new();

    public ChannelReader<string> Subscribe()
    {
        var channel = Channel.CreateUnbounded<string>();
        lock (_syncLock)
        {
            _clients.Add(channel);
        }
        return channel.Reader;
    }

    public void Unsubscribe(ChannelReader<string> reader)
    {
        lock (_syncLock)
        {
            var channel = _clients.FirstOrDefault(c => c.Reader == reader);
            if (channel != null)
            {
                _clients.Remove(channel);
                channel.Writer.TryComplete();
            }
        }
    }

    public async Task BroadcastAsync(string message)
    {
        List<Channel<string>> clientsToNotify;
        lock (_syncLock)
        {
            clientsToNotify = _clients.ToList();
        }

        foreach (var client in clientsToNotify)
        {
            await client.Writer.WriteAsync(message);
        }
    }
}
