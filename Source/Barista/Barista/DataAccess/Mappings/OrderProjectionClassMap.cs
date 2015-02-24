using Barista.Projections;

using FluentNHibernate.Mapping;

namespace Barista.DataAccess.Mappings
{
    public class OrderProjectionClassMap : ClassMap<OrderProjection>
    {
        public OrderProjectionClassMap()
        {
            Table("OrderProjections");

            Id(s => s.Id);

            Map(s => s.CreatedOn);
            Map(s => s.BaristaName);

            HasMany(x => x.Items).ForeignKeyConstraintName("FK_OrderItems");
        }
    }
}
