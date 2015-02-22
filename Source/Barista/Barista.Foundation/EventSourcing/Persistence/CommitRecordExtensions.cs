using System;
using System.Collections.Generic;
using System.Linq;
using Barista.Foundation.EventSourcing.Serialization;

namespace Barista.Foundation.EventSourcing.Persistence
{
    public static class CommitRecordExtensions
    {
        public static ICommit GetCommit(this CommitRecord record, ISerialize serializer)
        {
            var headers = serializer.Deserialize<Dictionary<string, object>>(record.Headers);
            var events = serializer.Deserialize<List<EventMessage>>(record.Payload);

            return new Commit(
                record.StreamIdOriginal,
                record.StreamRevision,
                record.CommitId,
                record.CommitSequence,
                ToDateTime(record.CommitStamp),
                record.CheckpointNumber.ToString(),
                headers,
                events);
        }

        public static IEnumerable<ICommit> ToCommits(this IEnumerable<CommitRecord> records, ISerialize serializer)
        {
            return records.Select(x => x.GetCommit(serializer));
        } 

        public static DateTime ToDateTime(object value)
        {
            value = value is decimal ? (long)(decimal)value : value;
            return value is long ? new DateTime((long)value) : DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc);
        }
    }
}
