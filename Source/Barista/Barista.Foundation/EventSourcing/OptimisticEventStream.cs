using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Barista.Foundation.Common;

namespace Barista.Foundation.EventSourcing
{
    public class OptimisticEventStream : IEventStream
    {
        private readonly ICollection<EventMessage> _committed = new LinkedList<EventMessage>();
        private readonly IDictionary<string, object> _committedHeaders = new Dictionary<string, object>();
        private readonly ICollection<EventMessage> _events = new LinkedList<EventMessage>();
        private readonly ICollection<Guid> _identifiers = new HashSet<Guid>();
        private readonly ICommitEvents _persistence;
        private readonly IDictionary<string, object> _uncommittedHeaders = new Dictionary<string, object>();
        private bool _disposed;

        public OptimisticEventStream(string streamId, ICommitEvents persistence)
        {
            StreamId = streamId;
            _persistence = persistence;
        }

        public OptimisticEventStream(string streamId, ICommitEvents persistence, int minRevision, int maxRevision)
            : this(streamId, persistence)
        {
            IEnumerable<ICommit> commits = persistence.GetFrom(streamId, minRevision, maxRevision);
            PopulateStream(minRevision, maxRevision, commits);

            if (minRevision > 0 && _committed.Count == 0)
            {
                throw new KeyNotFoundException();
            }
        }

        public string StreamId { get; private set; }
        public int StreamRevision { get; private set; }
        public int CommitSequence { get; private set; }

        public ICollection<EventMessage> CommittedEvents
        {
            get { return _committed.ToImmutableList(); }
        }

        public IDictionary<string, object> CommittedHeaders
        {
            get { return _committedHeaders; }
        }

        public ICollection<EventMessage> UncommittedEvents
        {
            get { return _events.ToImmutableList(); }
        }

        public IDictionary<string, object> UncommittedHeaders
        {
            get { return _uncommittedHeaders; }
        }

        public void Add(EventMessage uncommittedEvent)
        {
            if (uncommittedEvent == null || uncommittedEvent.Body == null)
            {
                return;
            }

            _events.Add(uncommittedEvent);
        }

        public void CommitChanges(Guid commitId)
        {
            if (_identifiers.Contains(commitId))
            {
                throw new InvalidOperationException("The key already exists");
            }

            if (!HasChanges())
            {
                return;
            }

            /*try
            {*/
                PersistChanges(commitId);
            /*}
            catch (/*ConcurrencyException*//*Exception)
            {*/
               /* IEnumerable<ICommit> commits = _persistence.GetFrom(StreamId, StreamRevision + 1, int.MaxValue);
                PopulateStream(StreamRevision + 1, int.MaxValue, commits);

                throw;
            }*/
        }

        public void ClearChanges()
        {
            _events.Clear();
            _uncommittedHeaders.Clear();
        }

        private void PopulateStream(int minRevision, int maxRevision, IEnumerable<ICommit> commits)
        {
            foreach (var commit in commits ?? Enumerable.Empty<ICommit>())
            {
                _identifiers.Add(commit.CommitId);

                CommitSequence = commit.CommitSequence;
                int currentRevision = commit.StreamRevision - commit.Events.Count + 1;
                if (currentRevision > maxRevision)
                {
                    return;
                }

                CopyToCommittedHeaders(commit);
                CopyToEvents(minRevision, maxRevision, currentRevision, commit);
            }
        }

        private void CopyToCommittedHeaders(ICommit commit)
        {
            foreach (var key in commit.Headers.Keys)
            {
                _committedHeaders[key] = commit.Headers[key];
            }
        }

        private void CopyToEvents(int minRevision, int maxRevision, int currentRevision, ICommit commit)
        {
            foreach (var @event in commit.Events)
            {
                if (currentRevision > maxRevision)
                {
                    break;
                }

                if (currentRevision++ < minRevision)
                {
                    continue;
                }

                _committed.Add(@event);
                StreamRevision = currentRevision - 1;
            }
        }

        private bool HasChanges()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("This instance has been disposed");
            }

            if (_events.Count > 0)
            {
                return true;
            }

            return false;
        }

        private void PersistChanges(Guid commitId)
        {
            CommitAttempt attempt = BuildCommitAttempt(commitId);

            ICommit commit = _persistence.Commit(attempt);

            PopulateStream(StreamRevision + 1, attempt.StreamRevision, new[] { commit });
            ClearChanges();
        }

        private CommitAttempt BuildCommitAttempt(Guid commitId)
        {
            return new CommitAttempt(
                StreamId,
                StreamRevision + _events.Count,
                commitId,
                CommitSequence + 1,
                SystemTime.UtcNow,
                _uncommittedHeaders.ToDictionary(x => x.Key, x => x.Value),
                _events.ToList());
        }

        public void Dispose()
        {
            _disposed = true;
        }
    }
}
