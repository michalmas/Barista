using FluentMigrator;

namespace Barista.DataAccess.Migrations
{
    [Migration(201502232354)]
    public class CreateOrderProjectionsTableMigration : Migration
    {
        private const string OrderProjectionsTable = "OrderProjections";

        public override void Up()
        {
            Create.Table(OrderProjectionsTable)
                .WithColumn("Id").AsGuid().NotNullable()
                .WithColumn("CreatedOn").AsDateTime().NotNullable()
                .WithColumn("BaristaName").AsString().NotNullable();

            Create.UniqueConstraint("PK_Orders")
                .OnTable(OrderProjectionsTable)
                .Column("Id");
        }

        public override void Down()
        {
            Delete.Table(OrderProjectionsTable);
        }
    }
}
