using System;
using Barista.Foundation.DataAccess;

namespace Barista.Foundation.EventSourcing.Persistence
{
    public class CommitRecord : IPersistable
    {
        public virtual int CheckpointNumber { get; set; }

        public virtual bool IsDispatched { get; set; }

        public virtual string StreamId { get; set; }

        public virtual string StreamIdOriginal { get; set; }

        public virtual int StreamRevision { get; set; }

        public virtual int Items { get; set; }

        public virtual Guid CommitId { get; set; }

        public virtual int CommitSequence { get; set; }

        public virtual DateTime CommitStamp { get; set; }

        public virtual byte[] Headers { get; set; }

        public virtual byte[] Payload { get; set; }
    }
}
