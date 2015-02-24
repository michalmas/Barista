using System;

namespace Barista.Foundation.EventSourcing
{
    /// <summary>
    /// Provides metadata information about a particular event.
    /// </summary>
    public class EventMetadata
    {
        /// <summary>
        /// Gets or sets the date and time in UTC at which the event occurred.
        /// </summary>
        public DateTime TimeStampUtc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the event has happened in the past. 
        /// </summary>
        /// <value>
        /// <c>true</c> if the denormalizer should reprocess the event as part of a query store
        /// rebuild.
        /// </value>
        public bool IsRedispatch { get; set; }
    }
}
