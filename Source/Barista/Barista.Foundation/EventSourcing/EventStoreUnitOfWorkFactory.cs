using System;
using System.Collections.Generic;
using System.Reflection;
using Barista.Foundation.DataAccess;

namespace Barista.Foundation.EventSourcing
{
    /// <summary>
    /// Unit of work factory that supports both NHibernate based entities as well those based on Event Sourcing.
    /// </summary>
    public abstract class EventStoreUnitOfWorkFactory<TUnitOfWork> : UnitOfWorkFactory<TUnitOfWork>
        where TUnitOfWork : DomainUnitOfWork
    {
        private readonly Lazy<IStoreEvents> eventStore;
        private readonly IDictionary<Type, Type> dehydrationTypePerEventTypeMap;

        protected EventStoreUnitOfWorkFactory(Lazy<IStoreEvents> eventStore,
            IDictionary<Type, Type> dehydrationTypePerEventTypeMap)
        {
            this.eventStore = eventStore;
            this.dehydrationTypePerEventTypeMap = dehydrationTypePerEventTypeMap;
        }

        /// <summary>
        /// Starts a new unit-of-work that tracks changes made the entities obtained through the associated repositories.
        /// </summary>
        /// <returns>
        /// Always creates a new unit-of-work, regardless of an existing one that is associated with the current thread.
        /// </returns>
        protected override TUnitOfWork CreateNew()
        {
            var eventStoreMapper = new EventStoreDataMapper(eventStore, dehydrationTypePerEventTypeMap);

            return CreateUnitOfWorkMappedTo(eventStoreMapper);
        }

        protected abstract TUnitOfWork CreateUnitOfWorkMappedTo(IDataMapper dataMapper);

        /// <summary>
        /// Gets a dictionary with events that require the instantiation of a specific aggregate root type
        /// when dehydrating the entity from the event store. The mapping will be derived from the aggregate root types
        /// that are decorated with the <see cref="CreationEventTypeAttribute"/>.
        /// </summary>
        /// <param name="assembly">The assembly in which to look for the aggregate root types.</param>
        protected static Dictionary<Type, Type> GetDehydrationTypeMap(Assembly assembly)
        {
           /* Type[] aggregationRootTypes = assembly.GetTypes()
              .Where(t => t.IsSubclassOfRawGeneric(typeof(EventSource<>)))
              .Where(t => t.HasAttribute<CreationEventTypeAttribute>())
              .ToArray();

            var dehydrationTypeMap = new Dictionary<Type, Type>();
            foreach (Type aggregationRootType in aggregationRootTypes)
            {
                aggregationRootType.FindAttributes<CreationEventTypeAttribute>()
                    .ForEach(attribute => dehydrationTypeMap.Add(attribute.Type, aggregationRootType));
            }

            return dehydrationTypeMap;*/
            return new Dictionary<Type, Type>();
        }
    }
}
