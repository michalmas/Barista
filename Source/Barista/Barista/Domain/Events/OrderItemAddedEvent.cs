using System;
using Barista.Foundation.Domain.Events;

namespace Barista.Domain.Events
{
    public class OrderItemAddedEvent : Event
    {
        public Guid OrderIdentity { get; set; }

        public string ProductName { get; set; }

        public int Quantity { get; set; }
    }
}
