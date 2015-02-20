using System;
using System.Collections.Generic;
using System.Data;

using Barista.Foundation.Common.Exceptions;
using Barista.Foundation.Domain;

using NHibernate;
using NHibernate.Criterion;

namespace Barista.Foundation.DataAccess.NHibernate
{
    public class NHibernateDataMapper : IDataMapper
    {
        private ISession session;
        private Dictionary<Type, object> repositories = new Dictionary<Type, object>();
        private ITransaction transaction;

        public NHibernateDataMapper(ISession session)
        {
            this.session = session;
        }

        /// <summary>
        /// Gets a value indicating whether this data mapper is currently maintaining a transaction.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has transaction; otherwise, <c>false</c>.
        /// </value>
        public bool HasTransaction
        {
            get { return (transaction != null); }
        }

        /// <summary>
        /// Gets a value indicating whether the data mapper still has changes to commit.
        /// </summary>
        public bool HasChanges
        {
            get { return (session != null) && session.IsDirty(); }
        }

        /// <summary>
        /// Gets the underlying <see cref="IDbConnection" />
        /// </summary>
        public IDbConnection Connection
        {
            get { return session.Connection; }
        }

        /// <summary>
        /// Creates a new <see cref="IQuery" /> for the given query string
        /// </summary>
        public IQuery CreateQuery(string queryString)
        {
            return session.CreateQuery(queryString);
        }

        /// <summary>
        /// Creates a new <see cref="ISQLQuery" /> for the given SQL query string
        /// </summary>
        public ISQLQuery CreateSQLQuery(string queryString)
        {
            return session.CreateSQLQuery(queryString);
        }

        public Repository<T> GetRepository<T>() where T : class, IPersistable
        {
            object repository;
            if (!repositories.TryGetValue(typeof(T), out repository))
            {
                repository = new NHibernateRepository<T>(session);
                repositories[typeof(T)] = repository;
            }

            return (Repository<T>)repository;
        }

        /// <summary>
        /// Gets the entity with the specified id while locking it for other sessions, and optionally verifies that it has not
        /// been changed by somebody else.
        /// </summary>
        public object GetWithLock(Type aggregateRootType, object key, long version = VersionedEntity.IgnoredVersion)
        {
            string keyProperty = PersistableObjectReflector.GetKeyPropertyName(aggregateRootType);

            var entity = session.CreateCriteria(aggregateRootType)
                                .Add(Restrictions.Eq(keyProperty, key))
                                .SetLockMode(LockMode.Upgrade)
                                .UniqueResult();

            AssertRecordExists(aggregateRootType, entity, key, version);
            AssertRecordNotChangedForVersionedEntity(aggregateRootType, entity, version);

            return entity;
        }

        /// <summary>
        /// Gets the entity with the specified id and optionally verifies that it has not been changed by somebody else.
        /// </summary>
        public object Get(Type aggregateRootType, object key, long version = VersionedEntity.IgnoredVersion)
        {
            var entity = GetOrDefault(aggregateRootType, key);

            AssertRecordExists(aggregateRootType, entity, key, version);
            AssertRecordNotChangedForVersionedEntity(aggregateRootType, entity, version);

            return entity;
        }

        public bool Exists(Type aggregateRootType, object key)
        {
            var entity = GetOrDefault(aggregateRootType, key);

            return entity != null;
        }

        private object GetOrDefault(Type aggregateRootType, object key)
        {
            string keyProperty = PersistableObjectReflector.GetKeyPropertyName(aggregateRootType);

            var entity = session.CreateCriteria(aggregateRootType)
                                .Add(Restrictions.Eq(keyProperty, key))
                                .UniqueResult();
            return entity;
        }


        private static void AssertRecordExists(Type entityType, object entity, object key, long version)
        {
            if (entity == null)
            {
                AssertKeyIsNotEmpty(entityType, key);

                throw new ApplicationErrorException(Error.RecordNotFound)
                {
                    { "EntityType", entityType.Name },
                    { "Key", key },
                    { "Version", version }
                };
            }
        }

        private static void AssertKeyIsNotEmpty(Type entityType, object key)
        {
            if ((key == null) || key.Equals(GetDefaultValue(key.GetType())))
            {
                throw new ApplicationErrorException(Error.AttemptToRetrieveRecordWithEmptyKey)
                {
                    { "EntityType", entityType.Name },
                };
            }
        }

        private static object GetDefaultValue(Type type)
        {
            try
            {
                var defaultConstructor = type.GetConstructor(new Type[0]);

                if (type == typeof(string))
                {
                    return string.Empty;
                }

                if (defaultConstructor != null)
                {
                    return defaultConstructor.Invoke(new object[0]);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private static void AssertRecordNotChangedForVersionedEntity(Type entityType, object entity, long version)
        {
            if ((version != VersionedEntity.IgnoredVersion) && (version != VersionedEntity.NewVersion))
            {
                var versionedEntity = (entity as IVersionedEntity);
                if (versionedEntity == null)
                {
                    throw new InvalidOperationException(entityType.Name + " is not a versioned entity");
                }

                if (versionedEntity.Version != version)
                {
                    throw new ApplicationErrorException(Error.RecordIsChangedByAnotherUser);
                }
            }
        }

        /// <summary>
        /// Should add the specified aggregate root to the unit of work and keep track of its changes.
        /// </summary>
        /// <remarks>
        /// If the aggregate is already part of the unit of work, then the call should be ignored.
        /// </remarks>
        public void Add(IPersistable aggregate)
        {
            session.Save(aggregate);
        }

        /// <summary>
        /// Enlists the current unit of work in an (existing ambient) transaction with the specified isolation level.
        /// </summary>
        public void EnlistTransaction(IsolationLevel isolationLevel)
        {
            transaction = session.BeginTransaction(isolationLevel);
        }

        /// <summary>
        /// Enlists the current unit of work in an (existing ambient) transaction.
        /// </summary>
        public void EnlistTransaction()
        {
            transaction = session.BeginTransaction();
        }

        /// <summary>
        /// Completes the transaction.
        /// </summary>
        public void CommitTransaction()
        {
            transaction.Commit();
            transaction = null;
        }

        /// <summary>
        /// Rolls back any changes in the current transaction.
        /// </summary>
        public void RollbackTransaction()
        {
            try
            {
                transaction.Rollback();
            }
            catch
            {
                // Ignore any exception while roll-backing to make sure we can retry
            }
            transaction = null;
        }

        /// <summary>
        /// Removes an individual entity from the current unit of work.
        /// </summary>
        public void Evict(IPersistable aggregate)
        {
            session.Evict(aggregate);
        }

        /// <summary>
        /// Removes all entities from the current unit of work.
        /// </summary>
        public void EvictAll()
        {
            session.Clear();
        }

        public void DeleteAllImmediately<TEntity>(params string[] tableNames)
        {
            session.DeleteImmediately<TEntity>(tableNames);
        }

        /// <summary>
        /// If the data mapper <see cref="IDataMapper.HasChanges"/>, then it will persist all changes to the underlying data store.
        /// </summary>
        public void SubmitChanges()
        {
            session.Flush();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (session != null)
            {
                session.Dispose();

                transaction = null;
                repositories = null;
                session = null;
            }
        }
    }
}
