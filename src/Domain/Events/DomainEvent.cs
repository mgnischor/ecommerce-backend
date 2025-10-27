namespace ECommerce.Domain.Events;

/// <summary>
/// Base class for all domain events
/// </summary>
public abstract class DomainEvent
{
    /// <summary>
    /// Unique identifier for this event
    /// </summary>
    public Guid EventId { get; }

    /// <summary>
    /// Timestamp when the event occurred
    /// </summary>
    public DateTime OccurredAt { get; }

    protected DomainEvent()
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
    }
}
