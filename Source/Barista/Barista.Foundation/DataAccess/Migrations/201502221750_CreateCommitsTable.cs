using FluentMigrator;

namespace Barista.Foundation.DataAccess.Migrations
{
    [Migration(201502221750)]
    public class CreateCommitsTableMigration : Migration
    {
        private const string CommitsTable = "Commits";

        public override void Up()
        {
            Create.Table(CommitsTable)
                .WithColumn("CheckpointNumber").AsInt64().Identity().NotNullable()
                .WithColumn("StreamId").AsFixedLengthString(40).NotNullable()
                .WithColumn("StreamIdOriginal").AsFixedLengthString(1000).NotNullable()
                .WithColumn("StreamRevision").AsInt32().NotNullable()
                .WithColumn("Items").AsInt16().NotNullable()
                .WithColumn("CommitId").AsGuid().NotNullable()
                .WithColumn("CommitSequence").AsInt32().NotNullable()
                .WithColumn("Headers").AsBinary().NotNullable()
                .WithColumn("Payload").AsBinary().NotNullable()
                .WithColumn("IsDispatched").AsBoolean().WithDefaultValue(false).NotNullable();

            Create.UniqueConstraint("PK_Commits")
                .OnTable(CommitsTable)
                .Column("CheckpointNumber");
        }

        public override void Down()
        {
            Delete.Table(CommitsTable);
        }
    }
}
