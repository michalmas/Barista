using FluentNHibernate.Mapping;

namespace Barista.Foundation.EventSourcing.Persistence
{
    public class CommitRecordClassMap : ClassMap<CommitRecord>
    {
        public CommitRecordClassMap()
        {
            Table("Commits");

            Id(x => x.CheckpointNumber).GeneratedBy.Identity();

            Map(x => x.StreamId);
            Map(x => x.StreamIdOriginal);
            Map(x => x.StreamRevision);
            Map(x => x.Items);
            Map(x => x.CommitId);
            Map(x => x.CommitSequence);
            Map(x => x.Headers);
            Map(x => x.Payload);
            Map(x => x.IsDispatched);
        }
    }
}
