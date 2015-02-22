using System.Collections.Generic;

namespace Barista.Foundation.EventSourcing
{
    public interface ICommitEvents
    {
        /// <summary>
        /// Gets the corresponding commits from the stream indicated starting at the revision specified until the
        /// end of the stream sorted in ascending order--from oldest to newest.
        /// </summary>
        /// <param name="streamId">The stream from which the events will be read.</param>
        /// <param name="minRevision">The minimum revision of the stream to be read.</param>
        /// <param name="maxRevision">The maximum revision of the stream to be read.</param>
        /// <returns>A series of committed events from the stream specified sorted in ascending order.</returns>
        /// <exception cref="StorageException" />
        /// <exception cref="StorageUnavailableException" />
        IEnumerable<ICommit> GetFrom(string streamId, int minRevision, int maxRevision);

        /// <summary>
        /// Writes the to-be-commited events provided to the underlying persistence mechanism.
        /// </summary>
        /// <param name="attempt">The series of events and associated metadata to be commited.</param>
        /// <exception cref="ConcurrencyException" />
        /// <exception cref="StorageException" />
        /// <exception cref="StorageUnavailableException" />
        ICommit Commit(CommitAttempt attempt);
    }
}
