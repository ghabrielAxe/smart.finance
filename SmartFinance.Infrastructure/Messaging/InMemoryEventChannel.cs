using System.Threading.Channels;
using SmartFinance.Application.Interfaces;

namespace SmartFinance.Infrastructure.Messaging;

public sealed class InMemoryEventChannel : IEventChannel
{
    private readonly Channel<Guid> _channel;

    public InMemoryEventChannel()
    {
        var options = new BoundedChannelOptions(10_000)
        {
            SingleReader = true,
            SingleWriter = false,
        };
        _channel = Channel.CreateBounded<Guid>(options);
    }

    public async Task PublishAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(eventId, cancellationToken);
    }

    public IAsyncEnumerable<Guid> ReadAllAsync(CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
