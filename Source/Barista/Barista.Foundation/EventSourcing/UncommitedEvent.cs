using Barista.Foundation.Domain.Events;
using Barista.Foundation.Domain;

namespace Barista.Foundation.EventSourcing
{
    /// <summary>
    /// Represents an event raised by an aggregate that still needs to be commited to the event store.
    /// </summary>
    internal class UncommitedEvent
    {
        public UncommitedEvent(IEventSource eventSource, IDomainEvent @event)
        {
            EventSource = eventSource;
            Event = @event;
        }

        public IEventSource EventSource { get; set; }

        public IDomainEvent Event { get; set; }
    }
}
