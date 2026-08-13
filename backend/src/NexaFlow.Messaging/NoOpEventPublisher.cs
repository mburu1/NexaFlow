using Microsoft.Extensions.Logging;

namespace NexaFlow.Messaging;

public class NoOpEventPublisher(ILogger<NoOpEventPublisher> logger) : IEventPublisher
{
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class
    {
        logger.LogDebug("Event {EventType} raised but no broker is wired up yet (Phase 2).", typeof(TEvent).Name);
        return Task.CompletedTask;
    }
}
