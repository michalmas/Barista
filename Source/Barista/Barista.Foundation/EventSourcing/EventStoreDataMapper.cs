using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Barista.Foundation.Common.Exceptions;
using Barista.Foundation.Common.Extensions;
using Barista.Foundation.DataAccess;
using Barista.Foundation.Domain;
using Barista.Foundation.Domain.Events;
using NHibernate.Util;
using Barista.Foundation.EventSourcing.Extensions;
using IsolationLevel = System.Data.IsolationLevel;

namespace Barista.Foundation.EventSourcing
{
    /// <summary>
    /// Implementation of <see cref="IDataMapper"/> that persists an aggregate by storing its changes into EventStore.
    /// </summary>
    public class EventStoreDataMapper : IDataMapper
    {
        private const int MinRevision = 0;
        private const int NewAggregateRootVersion = 0;
        private readonly Lazy<IStoreEvents> eventStore;
        private readonly IDictionary<Type, Type> dehydrationTypePerEventTypeMap;
        private readonly UncommitedEventQueue eventQueue = new UncommitedEventQueue();
        private readonly List<IEventSource> trackedSources = new List<IEventSource>();
        //private readonly IEventStoreConnectionScope eventStoreConnectionScope;

        public EventStoreDataMapper(Lazy<IStoreEvents> eventStore,
            IDictionary<Type, Type> dehydrationTypePerEventTypeMap/*,
            IEventStoreConnectionScope eventStoreConnectionScope*/)
        {
            this.eventStore = eventStore;
            this.dehydrationTypePerEventTypeMap = dehydrationTypePerEventTypeMap;
            /*this.eventStoreConnectionScope = eventStoreConnectionScope;*/
        }

        /// <summary>
        /// Gets a value indicating whether this data mapper is currently maintaining a transaction.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has transaction; otherwise, <c>false</c>.
        /// </value>
        public bool HasTransaction
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the data mapper still has changes to commit.
        /// </summary>
        public bool HasChanges
        {
            get { return !eventQueue.IsEmpty; }
        }

        /// <summary>
        /// Temporary property to prevent using the event store from within unit tests that don't need it.
        /// </summary>
        public bool IsEnabled
        {
            get { return (eventStore != null); }
        }

        /// <summary>
        /// Gets a repository object for persisting and loading persistable objects.
        /// </summary>
        public Repository<T> GetRepository<T>() where T : class, IPersistable
        {
            throw new InvalidOperationException("Event sourced aggregates cannot loaded through repositories");
        }

        /// <summary>
        /// Should add the specified aggregate root to the unit of work and keep track of its changes.
        /// </summary>
        /// <remarks>
        /// If the aggregate is already part of the unit of work, then the call should be ignored.
        /// </remarks>
        public void Add(IPersistable aggregate)
        {
            var eventSource = (IEventSource)aggregate;
            if (trackedSources.All(s => s.StreamId != eventSource.StreamId))
            {
                AddEventsThatOccurredBeforeTracking(eventSource);

                TrackFutureEvents(eventSource);
            }
        }

        private void AddEventsThatOccurredBeforeTracking(IEventSource aggregate)
        {
            foreach (Event @event in aggregate.GetChanges())
            {
                eventQueue.Enqueue(new UncommitedEvent(aggregate, @event));
            }
        }

        /// <summary>
        /// If the data mapper <see cref="IDataMapper.HasChanges"/>, then it will persist all changes to the underlying data store.
        /// </summary>
        public void SubmitChanges()
        {
            ForceInitializationOfTheEventStore();

            Action<Action> scope = BuildTransactionScope(eventQueue.HasMultipleCommits);

            scope(() =>
            {
                /*using (eventStoreConnectionScope.Open())
                {*/
                    while (!eventQueue.IsEmpty)
                    {
                        CommitBatch(eventQueue.DequeueCommit());
                    }
                /*}*/
            });
        }

        private void ForceInitializationOfTheEventStore()
        {
            // Force lazy initialization of the event store so that any undispatched commits are immediatelly dispatched.
            IStoreEvents temp = eventStore.Value;
        }

        private Action<Action> BuildTransactionScope(bool isMultiCommit)
        {
            if (isMultiCommit)
            {
                return action =>
                {
                    /*using (var scope = new DelayedDispatchingScope())
                    {*/
                        using (var tx = CreateTransactionScope())
                        {
                            action();

                            tx.Complete();
                            /*scope.Complete();*/
                        }
                    /*}*/
                };
            }

            return action => action();
        }

        private TransactionScope CreateTransactionScope()
        {
            return new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
            });
        }

        private void CommitBatch(UncommitedEvent[] batch)
        {
            IEventSource eventSource = batch.First().EventSource;

            using (IEventStream stream = eventStore.Value.OpenOrCreateStream(eventSource))
            {
                AssertRecordIsNotChangedByAnotherUser(eventSource, eventSource.CommittedVersion, stream.StreamRevision);

                batch.ForEach(uncommitedEvent => stream.Add(CreateEvent(uncommitedEvent)));

                AddCommitHeaders(eventSource, stream);

                /*try
                {*/
                    stream.CommitChanges(Guid.NewGuid());
                    eventSource.MarkAsCommitted(stream.StreamRevision);
                /*}
                catch (ConcurrencyException)
                {
                    if (eventSource.CommittedVersion == NewAggregateRootVersion)
                    {
                        ThrowDuplicateCommit(eventSource);
                    }
                    else
                    {
                        ThrowRecordIsChanged(eventSource, stream.StreamRevision);
                    }
                }
                catch (DuplicateCommitException)
                {
                    ThrowDuplicateCommit(eventSource);
                }*/
            }
        }

        private static EventMessage CreateEvent(UncommitedEvent uncommitedEvent)
        {
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                //var user = SystemContext.User();

                return new EventMessage
                {
                    Body = uncommitedEvent.Event,
                    Headers =
                    {
                        /*{ EventHeaders.CausedByIdentity, (user != null) ? user.Identity : null },
                        { EventHeaders.CausedByUserName, (user != null) ? user.Username : null },*/
                    }
                };
            }
        }

        private void AddCommitHeaders(IEventSource eventSource, IEventStream stream)
        {
            //AddSiteAsCommitHeader(eventSource, stream);
            //AddConditionalSyncAsCommitHeader(eventSource, stream);
        }

        /*private static void AddSiteAsCommitHeader(IEventSource eventSource, IEventStream stream)
        {
            var haveSite = eventSource as IHaveSiteScope;
            if (haveSite != null)
            {
                stream.UncommittedHeaders.Add(CommitHeaders.Site, haveSite.Site.DefaultOr(s => s.Key));
            }
        }

        private void AddConditionalSyncAsCommitHeader(IEventSource eventSource, IEventStream stream)
        {
            bool canBeConditionallySynced = eventSource is ICanBeSyncedConditionally;
            if (canBeConditionallySynced)
            {
                stream.UncommittedHeaders.Add(CommitHeaders.ConditionalSync, true);
            }
        }*/

        /*private static void ThrowDuplicateCommit(IEventSource eventSource)
        {
            throw new ApplicationErrorException(Error.RecordWithDuplicateUniqueKeyExists)
            {
                { "EntityType", eventSource.GetType().Name },
                { "Key", PersistableObjectReflector.GetKey(eventSource) }
            };
        }*/

        /// <summary>
        /// Gets the entity with the specified id while locking it for other sessions, and optionally verifies that it has not
        /// been changed by somebody else.
        /// </summary>
        public object GetWithLock(Type aggregateRootType, object key, long version = VersionedEntity.IgnoredVersion)
        {
            throw new NotSupportedException("The EventStoreDataMapper doesn't support pessimistic locking by design");
        }

        /// <summary>
        /// Gets an instance of an entity based on its functional key and the version.
        /// </summary>
        public object Get(Type aggregateRootType, object key, long version = VersionedEntity.IgnoredVersion)
        {
            Guid streamId = key.ToDeterministicGuid(aggregateRootType);

            IEventSource eventSource = trackedSources.SingleOrDefault(source => (source.StreamId == streamId));
            if (eventSource == null)
            {
                eventSource = TryLoadFromEventStream(aggregateRootType, streamId);
            }

            AssertAggregateExists(eventSource, aggregateRootType, key);

            AssertRecordIsNotChangedByAnotherUser(eventSource, version, eventSource.Version);

            return eventSource;
        }

        public bool Exists(Type aggregateRootType, object key)
        {
            Guid streamId = key.ToDeterministicGuid(aggregateRootType);

            bool found = ExistsInTrackedSources(aggregateRootType, streamId);
            if (!found)
            {
                found = ExistsInEventStore(aggregateRootType, streamId);
            }

            return found;
        }

        private bool ExistsInTrackedSources(Type aggregateRootType, Guid streamId)
        {
            return trackedSources.Any(source => ((source.StreamId == streamId) && InstanceIsOfType(source, aggregateRootType)));
        }

        private bool ExistsInEventStore(Type aggregateRootType, Guid streamId)
        {
            bool exists = false;
            try
            {
                object eventSource = TryLoadFromEventStream(aggregateRootType, streamId);
                exists = ((eventSource != null) && InstanceIsOfType(eventSource, aggregateRootType));
            }
            catch (ApplicationErrorException ex)
            {
                if (ex.Error != Error.RecordNotFound)
                {
                    throw;
                }
            }

            return exists;
        }

        private static bool InstanceIsOfType(object source, Type aggregateRootType)
        {
            return aggregateRootType.IsInstanceOfType(source);
        }

        private IEventSource TryLoadFromEventStream(Type requestedAggregateRootType, Guid streamId)
        {
            using (IEventStream stream = eventStore.Value.OpenStream(streamId, MinRevision))
            {
                if (stream.StreamRevision == MinRevision)
                {
                    return null;
                }

                Type actualAggregateRootType = DetermineAggregateRootType(stream, requestedAggregateRootType);

                if (!requestedAggregateRootType.IsAssignableFrom(actualAggregateRootType))
                {
                    return null;
                }

                var eventSource = (IEventSource)Activator.CreateInstance(actualAggregateRootType, true);
                eventSource.Load(
                    stream.StreamRevision,
                    stream.CommittedEvents.Select(e => e.Body).Cast<Event>());

                TrackFutureEvents(eventSource);

                return eventSource;
            }
        }

        private Type DetermineAggregateRootType(IEventStream stream, Type defaultAggregateRootType)
        {
            Type aggregateRootType = FindAggregateRootTypeFromStream(stream);
            return aggregateRootType ?? defaultAggregateRootType;
        }

        private Type FindAggregateRootTypeFromStream(IEventStream stream)
        {
            IDomainEvent firstEvent = stream.CommittedEvents.Select(e => e.Body).Cast<Event>().First();
            Type eventType = firstEvent.GetType();

            if (dehydrationTypePerEventTypeMap.ContainsKey(eventType))
            {
                return dehydrationTypePerEventTypeMap[eventType];
            }

            return null;
        }

        private static void AssertAggregateExists(IEventSource eventSource, Type aggregateRootType, object key)
        {
            if (eventSource == null)
            {
                throw new ApplicationErrorException(Error.RecordNotFound)
                {
                    { "EntityType", aggregateRootType.Name },
                    { "Key", key },
                    { "Version", NewAggregateRootVersion }
                };
            }
        }

        private static void AssertRecordIsNotChangedByAnotherUser(IEventSource eventSource, long expectedVersion,
            long actualVersion)
        {
            if ((expectedVersion != VersionedEntity.IgnoredVersion) &&
                (actualVersion != NewAggregateRootVersion) &&
                (actualVersion != expectedVersion))
            {
                ThrowRecordIsChanged(eventSource, actualVersion);
            }
        }

        private static void ThrowRecordIsChanged(IEventSource eventSource, long actualVersion)
        {
            Type type = eventSource.GetType();

            throw new ApplicationErrorException(Error.RecordIsChangedByAnotherUser)
            {
                { "EntityType", type.Name },
                { "Key", PersistableObjectReflector.GetKey(eventSource) },
                { "Version", actualVersion }
            };
        }

        public void TrackFutureEvents(IEventSource aggregate)
        {
            if (!trackedSources.Contains(aggregate))
            {
                trackedSources.Add(aggregate);
                aggregate.EventApplied += OnEventApplied;
            }
        }

        /// <summary>
        /// Enlists the current unit of work in an (existing ambient) transaction.
        /// </summary>
        public void EnlistTransaction()
        {
            // Events are stored within their own transaction
        }

        /// <summary>
        /// Enlists the current unit of work in an (existing ambient) transaction with the specified isolation level.
        /// </summary>
        public void EnlistTransaction(IsolationLevel isolationLevel)
        {
            // Events are stored within their own transaction
        }

        /// <summary>
        /// Rolls back any changes in the current transaction.
        /// </summary>
        public void RollbackTransaction()
        {
            // Events are stored within their own transaction
        }

        /// <summary>
        /// Completes the transaction.
        /// </summary>
        public void CommitTransaction()
        {
            SubmitChanges();
        }

        public void DeleteAllImmediately<TEntity>(params string[] tableNames)
        {

        }

        /// <summary>
        /// Removes an individual entity from the current unit of work.
        /// </summary>
        public void Evict(IPersistable aggregate)
        {
            trackedSources.Remove((IEventSource)aggregate);
            eventQueue.Clear(aggregate);
        }

        /// <summary>
        /// Removes all entities from the current unit of work.
        /// </summary>
        public void EvictAll()
        {
            trackedSources.Clear();
            eventQueue.Clear();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            eventQueue.Clear();

            foreach (IEventSource trackedSource in trackedSources)
            {
                trackedSource.EventApplied -= OnEventApplied;
            }

            trackedSources.Clear();
        }

        private void OnEventApplied(object sender, EventAppliedArgs e)
        {
            eventQueue.Enqueue(new UncommitedEvent((IEventSource)sender, e.Event));
        }
    }
}
