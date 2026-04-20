namespace SmartFinance.Application.Interfaces;

public interface IEventChannel
{
    Task PublishAsync(Guid eventId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<Guid> ReadAllAsync(CancellationToken cancellationToken = default);
}
