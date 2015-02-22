using System;
using System.Collections.Generic;
using Barista.Foundation.EventSourcing.Persistence;

namespace Barista.Foundation.EventSourcing
{
    public class OptimisticEventStore : IStoreEvents, ICommitEvents
    {
        private readonly IPersistStreams _persistence;

        public OptimisticEventStore(IPersistStreams persistence)
        {
            if (persistence == null)
            {
                throw new ArgumentNullException("persistence");
            }

            _persistence = persistence;
        }

        public virtual IEnumerable<ICommit> GetFrom(string streamId, int minRevision, int maxRevision)
        {
            return _persistence.GetFrom(streamId, minRevision, maxRevision);
        }

        public virtual ICommit Commit(CommitAttempt attempt)
        {
            ICommit commit = _persistence.Commit(attempt);

            return commit;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual IEventStream CreateStream(string streamId)
        {
            return new OptimisticEventStream(streamId, this);
        }

        public virtual IEventStream OpenStream(string streamId, int minRevision, int maxRevision)
        {
            maxRevision = maxRevision <= 0 ? int.MaxValue : maxRevision;

            return new OptimisticEventStream(streamId, this, minRevision, maxRevision);
        }

        /*public virtual IEventStream OpenStream(ISnapshot snapshot, int maxRevision)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException("snapshot");
            }

            Logger.Debug(Resources.OpeningStreamWithSnapshot, snapshot.StreamId, snapshot.StreamRevision, maxRevision);
            maxRevision = maxRevision <= 0 ? int.MaxValue : maxRevision;
            return new OptimisticEventStream(snapshot, this, maxRevision);
        }*/

        public virtual IPersistStreams Advanced
        {
            get { return _persistence; }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            _persistence.Dispose();
        }
    }
}
