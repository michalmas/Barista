using Barista.Projections;

using FluentNHibernate.Mapping;

namespace Barista.DataAccess.Mappings
{
    public class OrderItemProjectionClassMap : ClassMap<OrderItemProjection>
    {
        public OrderItemProjectionClassMap()
        {
            Table("OrderItemsProjections");

            Id(x => x.Id);

            Map(x => x.OrderId);
            Map(x => x.ProductName);
            Map(x => x.Quantity);

            References(x => x.Order).Column("OrderIdentity").ForeignKey("Id");
        }
    }
}
