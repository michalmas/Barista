using System;

using Barista.DataAccess;
using Barista.Domain.Events;
using Barista.Foundation.Denormalization;
using Barista.Foundation.EventSourcing;
using Barista.Projections;

namespace Barista.Denormalizers
{
    public class OrderProjectionDenormalizer : IdentityBasedDenormalizer<OrderProjection, Guid>,
        IDenormalize<OrderCreatedEvent>
    {
        //private readonly Func<BaristaProjectionsUnitOfWork> uowFactory;

        public OrderProjectionDenormalizer(Func<BaristaProjectionsUnitOfWork> uowFactory)
            : base(uowFactory)
        {
            //this.uowFactory = uowFactory;
        }

        public void Handle(OrderCreatedEvent @event, EventMetadata metadata)
        {
            OnHandle(@event.Identity, @event.Version, metadata, projection =>
            {
                projection.Id = @event.Identity;
                projection.CreatedOn = @event.CreatedOn;
                projection.BaristaName = @event.BaristaName;
            });
        }
    }
}