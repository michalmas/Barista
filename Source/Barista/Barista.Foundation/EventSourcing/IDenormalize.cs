using Barista.Foundation.Domain.Events;

namespace Barista.Foundation.EventSourcing
{
    /// <summary>
    /// Should be implemented by a denormalizer to create or update a projection based on a domain event.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public interface IDenormalize<in TEvent> where TEvent : IDomainEvent
    {
        void Handle(TEvent @event, EventMetadata metadata);
    }
}
