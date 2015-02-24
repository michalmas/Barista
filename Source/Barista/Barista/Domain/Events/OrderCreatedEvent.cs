using System;
using Barista.Foundation.Domain.Events;

namespace Barista.Domain.Events
{
    public class OrderCreatedEvent : Event
    {
        public Guid Identity { get; set; }

        public string BaristaName { get; set; }

        public DateTimeOffset CreatedOn { get; set; }
    }
}
