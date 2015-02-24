using System;

namespace Barista.Foundation.EventSourcing
{
    /// <summary>
    /// Indicates the ability to (re)dispatch commits, optionally in a batch of multiple commits.
    /// </summary>
    public interface ICommitDispatcher
    {
        /// <summary>
        /// Dispatches the commit specified to the messaging infrastructure.
        /// </summary>
        /// <param name="commit">The commmit to be dispatched.</param>
        void Dispatch(ICommit commit);

        /// <summary>
        /// Indicates that the EventStore is about to dispatch several related commits and gives the dispatcher to setup
        /// any optimizations.
        /// </summary>
        IDisposable BeginBatch();

        /// <summary>
        /// Redispatches the specified events within the commit.
        /// </summary>
        /// <param name="commit">The commit, used for extracting metadata.</param>
        void Redispatch(ICommit commit);
    }
}
