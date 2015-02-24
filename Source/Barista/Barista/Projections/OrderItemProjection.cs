using System;

using Barista.Foundation.Domain;

namespace Barista.Projections
{
    public class OrderItemProjection : IQueryModel
    {
        public virtual OrderProjection Order { get; set; }

        [Identity]
        public virtual string Id { get; set; }

        public virtual Guid OrderId { get; set; }

        public virtual string ProductName { get; set; }

        public virtual int Quantity { get; set; }
    }
}
