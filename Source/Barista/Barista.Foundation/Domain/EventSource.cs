using System;
using System.Collections.Generic;
using Barista.Foundation.Common.Extensions;
using Barista.Foundation.DataAccess;
using Barista.Foundation.Domain.Events;

namespace Barista.Foundation.Domain
{
    /// <summary>
    /// Represents an aggregate root which changes are persisted as a collection of <see cref="IDomainEvent" />.
    /// </summary>
    /// <typeparam name="TState">
    /// The type of the object that will hold the state of the aggregate.
    /// </typeparam>
    public abstract class EventSource<TState> : IEventSource
        where TState : AggregateState, new()
    {
        private readonly List<Event> changes = new List<Event>();
        private long committedVersion;
        private Guid? id;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSource{TState}" /> class.
        /// </summary>
        protected EventSource()
        {
            AggregateState = new TState();
        }

        /// <summary>
        /// Occurs when a change is applied to the aggregate.
        /// </summary>
        public event EventHandler<EventAppliedArgs> EventApplied = delegate { };

        /// <summary>
        /// Gets the id.
        /// </summary>
        public Guid Id { get { return StreamId; } }

        /// <summary>
        /// Gets an identifier that identifies the underlying event stream.
        /// </summary>
        /// <remarks>
        /// The concrete implementation must have a property representing the functional key identified with
        /// <see cref="IdentityAttribute"/>. If that property is not of type <see cref="Guid"/>, it will convert it using
        /// hash algorithm.
        /// </remarks>
        public Guid StreamId
        {
            get
            {
                if (!id.HasValue)
                {
                    object key = PersistableObjectReflector.GetKey(this);
                    id = key.ToDeterministicGuid(PersistableObjectReflector.GetTypeStreamPrefix(GetType()));
                }

                return id.Value;
            }
        }

        /// <summary>
        /// Represents an object that contains the state of the aggregate and processes the events.
        /// </summary>
        protected TState AggregateState { get; private set; }

        /// <summary>
        /// Applies the specified event to the aggregate.
        /// </summary>
        public void Apply<TEvent>(TEvent @event) where TEvent : Event
        {
            changes.Add(@event);

            @event.Version = Version;

            AggregateState.Process(@event);

            //DomainEvents.Raise(@event);

            EventApplied(this, new EventAppliedArgs(@event));
        }

        /// <summary>
        /// Loads the aggregate with a collection of events in the provided order to represent the state of the 
        /// aggregate when it was last committed.
        /// </summary>
        public void Load(long committedVersion, IEnumerable<Event> events)
        {
            changes.Clear();

            foreach (var @event in events)
            {
                AggregateState.Process(@event);
            }

            this.committedVersion = committedVersion;
        }

        /// <summary>
        /// Gets the changes applied to the aggregate since it was initially loaded or the last time committed.
        /// </summary>
        public IEnumerable<Event> GetChanges()
        {
            return changes;
        }

        /// <summary>
        /// Marks the aggregate as committed.
        /// </summary>
        public void MarkAsCommitted(long committedVersion)
        {
            changes.Clear();
            this.committedVersion = committedVersion;
        }

        /// <summary>
        /// Gets the version of the aggregate at the time it was loaded from the underlying data store
        /// </summary>
        public long CommittedVersion
        {
            get { return committedVersion; }
        }

        /// <summary>
        /// Gets the version of the entire aggregate, including any uncommitted changes.
        /// </summary>
        public long Version
        {
            get { return committedVersion + changes.Count; }
        }

        /// <summary>
        /// Temporary property to prevent an aggregate to be stored in NHibernate
        /// </summary>
        public bool EventStoreOnly { get; set; }
    }
}
