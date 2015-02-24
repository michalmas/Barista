using System;

namespace Barista.Foundation.EventSourcing
{
    /// <summary>
    /// Provides contextual information about dispatching a commit.
    /// </summary>
    public class DispatchContext
    {
        /// <summary>
        /// Gets or sets the time stamp in UTC of the commit.
        /// </summary>
        public DateTime TimeStampUtc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the commit is being redispatched.
        /// </summary>
        public bool IsRedispatch { get; set; }
    }
}
