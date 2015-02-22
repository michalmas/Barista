using Barista.Foundation.Domain;
using System;

namespace Barista.Foundation.EventSourcing.Extensions
{
    public static class EventStoreExtensions
    {
        public static IEventStream OpenStream(this IStoreEvents storeEvents, Guid streamId,
            int minRevision = int.MinValue, int maxRevision = int.MaxValue)
        {
            return storeEvents.OpenStream(streamId.ToString(), minRevision, maxRevision);
        }

        public static IEventStream OpenOrCreateStream(this IStoreEvents store, IEventSource eventSource)
        {
            return (eventSource.CommittedVersion == 0)
                ? store.CreateStream(eventSource.StreamId)
                : store.OpenStream(eventSource.StreamId);
        }

        // <summary>
        /// Creates a new stream.
        /// </summary>
        /// <param name="storeEvents">The store events instance.</param>
        /// <param name="streamId">The value which uniquely identifies the stream to be created.</param>
        /// <returns>An empty stream.</returns>
        public static IEventStream CreateStream(this IStoreEvents storeEvents, Guid streamId)
        {
            return CreateStream(storeEvents, streamId.ToString());
        }

        /// <summary>
        /// Creates a new stream.
        /// </summary>
        /// <param name="storeEvents">The store events instance.</param>
        /// <param name="streamId">The value which uniquely identifies the stream to be created.</param>
        /// <returns>An empty stream.</returns>
        public static IEventStream CreateStream(this IStoreEvents storeEvents, string streamId)
        {
            return storeEvents.CreateStream(streamId);
        }
    }
}
