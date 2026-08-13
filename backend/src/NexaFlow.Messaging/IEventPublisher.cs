namespace NexaFlow.Messaging;

/// <summary>
/// Publishes domain events for other services to consume — task-assignment to email
/// notification over RabbitMQ, audit trail to Kafka. Phase 2 work; NoOpEventPublisher is
/// the only registered implementation today. See docs/adr/002-messaging-choice.md.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class;
}
