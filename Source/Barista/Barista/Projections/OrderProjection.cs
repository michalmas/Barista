using System;
using System.Collections.Generic;
using Barista.Foundation.Domain;

namespace Barista.Projections
{
    public class OrderProjection : IQueryModel
    {
        [Identity]
        public virtual Guid Id { get; set; }

        public virtual DateTimeOffset CreatedOn { get; set; }

        public virtual string BaristaName { get; set; }

        public virtual IList<OrderItemProjection> Items { get; protected set; }

        public OrderProjection()
        {
            Items = new List<OrderItemProjection>();
        }
    }
}
