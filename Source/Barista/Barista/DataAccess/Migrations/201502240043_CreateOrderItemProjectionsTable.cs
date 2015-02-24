using FluentMigrator;

namespace Barista.DataAccess.Migrations
{
    [Migration(201502240043)]
    public class CreateOrderItemProjectionsTableMigration : Migration
    {
        private const string OrderItemProjectionsTable = "OrderItemProjections";

        public override void Up()
        {
            Create.Table(OrderItemProjectionsTable)
                .WithColumn("Id").AsString().NotNullable()
                .WithColumn("OrderIdentity").AsGuid().NotNullable()
                .WithColumn("ProductName").AsString().NotNullable()
                .WithColumn("Quantity").AsInt32().NotNullable();

            Create.ForeignKey()
                .FromTable(OrderItemProjectionsTable).ForeignColumn("OrderIdentity")
                .ToTable("OrderProjections").PrimaryColumn("Id");

            Create.UniqueConstraint("PK_OrderItem")
                .OnTable(OrderItemProjectionsTable)
                .Column("Id");

            Create.UniqueConstraint("PK_OrderItems")
                .OnTable(OrderItemProjectionsTable)
                .Columns("OrderIdentity", "ProductName");
        }

        public override void Down()
        {
            Delete.Table(OrderItemProjectionsTable);
        }
    }
}
