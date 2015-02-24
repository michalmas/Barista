using System;
using System.Collections.Generic;
using System.Linq;

using Barista.Foundation.DataAccess;
using Barista.Foundation.Domain;

namespace Barista.Foundation.EventSourcing
{
    /// <summary>
    /// Represents a queue of uncommited events as tracked by the <see cref="EventStoreDataMapper"/>.
    /// </summary>
    internal class UncommitedEventQueue
    {
        private List<UncommitedEvent> events = new List<UncommitedEvent>();
        public const int MaxEventsPerCommit = 255;

        /// <summary>
        /// Indicates whether the events in the queue require multiple commits to persist.
        /// </summary>
        public bool HasMultipleCommits
        {
            get { return !InvolvesSingleStream || (events.Count > MaxEventsPerCommit); }
        }

        private bool InvolvesSingleStream
        {
            get { return events.Select(e => e.EventSource).Distinct().Count() == 1; }
        }

        public bool IsEmpty
        {
            get { return (events.Count == 0); }
        }

        /// <summary>
        /// Adds a new uncommitted event to the queue.
        /// </summary>
        public void Enqueue(UncommitedEvent @event)
        {
            events.Add(@event);
        }

        /// <summary>
        /// Extracts a batch of events that can be persisted as a single commit in the event store. 
        /// </summary>
        /// <remarks>
        /// Accounts for the size constraints applied on a single commit. 
        /// </remarks>
        public UncommitedEvent[] DequeueCommit()
        {
            UncommitedEvent firstEvent = events.FirstOrDefault();
            if (firstEvent != null)
            {
                UncommitedEvent[] eventsFromSameSource = events.
                    TakeWhile(@event => @event.EventSource.StreamId == firstEvent.EventSource.StreamId).ToArray();

                eventsFromSameSource =
                    eventsFromSameSource.Take(Math.Min(eventsFromSameSource.Length, MaxEventsPerCommit)).ToArray();

                events = events.Except(eventsFromSameSource).ToList();

                return eventsFromSameSource;
            }

            return new UncommitedEvent[0];
        }

        /// <summary>
        /// Removes all uncommited events from the queue.
        /// </summary>
        public void Clear()
        {
            events.Clear();
        }

        /// <summary>
        /// Removes only the uncommited events raised by a particular event source from the queue.
        /// </summary>
        public void Clear(IPersistable eventSource)
        {
            events.RemoveAll(e => ReferenceEquals(e.EventSource, eventSource));
        }
    }
}
