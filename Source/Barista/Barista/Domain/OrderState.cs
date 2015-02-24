using System;
using System.Collections.Generic;

using Barista.Domain.Events;
using Barista.Foundation.Domain;

namespace Barista.Domain
{
    public class OrderState : AggregateState
    {
        public Guid Identity { get; private set; }

        public string BaristaName { get; private set; }

        public DateTimeOffset CreatedOn { get; private set; }

        private List<OrderItem> items = new List<OrderItem>();

        public IReadOnlyList<OrderItem> Items
        {
            get { return items.AsReadOnly(); }
        }

        protected void When(OrderCreatedEvent @event)
        {
            Identity = @event.Identity;
            BaristaName = @event.BaristaName;
            CreatedOn = @event.CreatedOn;
        }

        protected void When(OrderItemAddedEvent @event)
        {
            items.Add(new OrderItem(@event.ProductName, @event.Quantity));
        }
    }
}
