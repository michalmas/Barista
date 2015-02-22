using System;

namespace Barista.Foundation.Domain.Events
{
    /// <summary>
    /// Convenient class for creating domain events that contain the version of an object.
    /// </summary>
    [Serializable]
    public abstract class Event : IDomainEvent
    {
        /// <summary>
        /// Gets or sets the version of the aggregate that this event applies to.
        /// </summary>
        public long Version { get; set; }
    }
}
