using System;

using Barista.Domain.Events;
using Barista.Foundation.DataAccess;
using Barista.Foundation.Denormalization;
using Barista.Foundation.EventSourcing;
using Barista.Projections;

namespace Barista.Denormalizers
{
    public class OrderItemProjectionDenormalizer : IdentityBasedDenormalizer<OrderItemProjection, string>,
        IDenormalize<OrderItemAddedEvent>
    {
        public OrderItemProjectionDenormalizer(Func<ProjectionsUnitOfWork> uowFactory) 
            : base(uowFactory)
        {
        }

        private string GetId(OrderItemAddedEvent @event)
        {
            return String.Format("{0}.{1}", @event.OrderIdentity, @event.ProductName);
        }

        public void Handle(OrderItemAddedEvent @event, EventMetadata metadata)
        {
            OnHandle(GetId(@event), @event.Version, metadata, orderItem =>
            {
                orderItem.OrderId = @event.OrderIdentity;
                orderItem.ProductName = @event.ProductName;
                orderItem.Quantity = @event.Quantity;
            });
        }
    }
}
