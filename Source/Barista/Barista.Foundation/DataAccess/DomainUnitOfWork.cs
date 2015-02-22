using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Barista.Foundation.Common.Exceptions;
using Barista.Foundation.Domain;

namespace Barista.Foundation.DataAccess
{
    /// <summary>
    /// Represents a unit of work for keeping track of changes made to the domain model of a system.
    /// </summary>
    public abstract class DomainUnitOfWork : StatefulUnitOfWork
    {
        private readonly Dictionary<Type, HashSet<object>> versionAssertedEntities = new Dictionary<Type, HashSet<object>>();

        protected DomainUnitOfWork(IDataMapper mapper)
            : base(mapper)
        {
        }

        public object Get(Type entityType, object key)
        {
            return Mapper.Get(entityType, key);
        }

        public T Get<T>(object key, long version)
        {
            // To facilitate command batching, the version of an aggregate only needs to be checked once within a unit of work.
            if (HasEntityVersionPreviouslyBeenAsserted(typeof(T), key))
            {
                return (T)Mapper.Get(typeof(T), key);
            }

            MarkAsVersionAsserted(typeof(T), key);

            return (T)Mapper.Get(typeof(T), key, version);
        }

        public object GetWithLock(Type entityType, object key)
        {
            return Mapper.GetWithLock(entityType, key);
        }

        private bool HasEntityVersionPreviouslyBeenAsserted(Type entityType, object key)
        {
            var query =
                from type in versionAssertedEntities.Keys
                where type.IsAssignableFrom(entityType) || entityType.IsAssignableFrom(type)
                select versionAssertedEntities[type].Contains(key);

            return query.Any(x => x);
        }

        private void MarkAsVersionAsserted(Type entityType, object key)
        {
            if (!versionAssertedEntities.ContainsKey(entityType))
            {
                versionAssertedEntities.Add(entityType, new HashSet<object>());
            }

            versionAssertedEntities[entityType].Add(key);
        }

        /// <summary>
        /// Finds the aggregate root for the specified code. Returns <c>null</c> if the entity does not exist.
        /// </summary>
        /// <remarks>
        /// This is a combination of <see cref="Exists{T}"/> and <see cref="Get{T}(object)"/>.
        /// </remarks>
        public T Find<T>(object code) where T : class
        {
            if (Exists<T>(code))
            {
                return Get<T>(code);
            }

            return null;
        }

        /// <summary>
        /// Indicates whether an entity of the specified type <typeparamref name="T"/> exists for the
        /// specified <paramref name="key"/>.
        /// </summary>
        public bool Exists<T>(object key)
        {
            AssertKeyIsNotEmpty(typeof(T), key);

            return Mapper.Exists(typeof(T), key);
        }

        /// <summary>
        /// Gets the aggregate root for the specified code. Throws an exception if the entity does not exist.
        /// </summary>
        public T Get<T>(object key)
        {
            AssertKeyIsNotEmpty(typeof(T), key);

            return (T)Mapper.Get(typeof(T), key);
        }

        private static void AssertKeyIsNotEmpty(Type aggregateRootType, object key)
        {
            if ((key == null) || key.Equals(GetDefaultValue(key.GetType())))
            {
                throw new ApplicationErrorException(Error.AttemptToRetrieveRecordWithEmptyKey)
                {
                    { "EntityType", aggregateRootType.Name },
                };
            }
        }

        private static object GetDefaultValue(Type type)
        {
            try
            {
                if (type == typeof(string))
                {
                    return string.Empty;
                }

                ConstructorInfo defaultConstructor = type.GetConstructor(new Type[0]);

                if (defaultConstructor == null)
                {
                    return null;
                }

                return defaultConstructor.Invoke(new object[0]);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Adds the specified aggregate root to the unit of work.
        /// </summary>
        /// <remarks>
        /// If the aggregate is already part of the unit of work, then the call is ignored.
        /// </remarks>
        public void Add(IEventSource aggregate)
        {
            // To facilitate command batching, the version of an aggregate only needs to be checked once within a unit of work.
            if (HasEntityVersionPreviouslyBeenAsserted(aggregate.GetType(), PersistableObjectReflector.GetKey(aggregate)))
            {
                Mapper.Add(aggregate);
            }
            MarkAsVersionAsserted(aggregate.GetType(), PersistableObjectReflector.GetKey(aggregate));
            Mapper.Add(aggregate);
        }

        /// <summary>
        /// Removes an individual entity from the current unit of work.
        /// </summary>
        public override void Evict(IPersistable aggregate)
        {
            base.Evict(aggregate);
            versionAssertedEntities.Remove(aggregate.GetType());
        }

        /// <summary>
        /// Removes all entities from the current unit of work.
        /// </summary>
        public override void EvictAll()
        {
            base.EvictAll();
            versionAssertedEntities.Clear();
        }
    }
}
