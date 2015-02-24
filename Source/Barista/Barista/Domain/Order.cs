using System;
using System.Collections.Generic;

using Barista.Domain.Events;
using Barista.Foundation.Domain;
using Barista.Foundation.EventSourcing;

namespace Barista.Domain
{
    [StreamIdPrefix("Barista.Domain.Order")]
    public class Order : EventSource<OrderState>
    {
        [Identity]
        public Guid Identity { get { return AggregateState.Identity; } }

        public string BaristaName { get { return AggregateState.BaristaName; } }

        public DateTimeOffset CreatedOn { get { return AggregateState.CreatedOn; } }

        public IReadOnlyList<OrderItem> Items { get { return AggregateState.Items; } }

        public Order()
        {
        }

        public Order(Guid identity, string baristaName)
        {
            Apply(new OrderCreatedEvent
            {
                Identity = identity,

                BaristaName = baristaName,

                CreatedOn = DateTimeOffset.UtcNow
            });
        }

        public void AddItem(string productName, int quantity)
        {
            Apply(new OrderItemAddedEvent
            {
                OrderIdentity = Identity,

                ProductName = productName,
                Quantity = quantity
            });
        }
    }
}
