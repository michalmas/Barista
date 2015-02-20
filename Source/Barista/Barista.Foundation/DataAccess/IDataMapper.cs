using System;
using System.Data;

namespace Barista.Foundation.DataAccess
{
    public interface IDataMapper
    {
        /// <summary>
        /// Gets a value indicating whether this data mapper is currently maintaining a transaction.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has transaction; otherwise, <c>false</c>.
        /// </value>
        bool HasTransaction { get; }

        /// <summary>
        /// Gets a value indicating whether the data mapper still has changes to commit.
        /// </summary>
        bool HasChanges { get; }

        /// <summary>
        /// Gets a repository object for persisting and loading persistable objects.
        /// </summary>
        Repository<T> GetRepository<T>() where T : class, IPersistable;

        /// <summary>
        /// Should add the specified aggregate root to the unit of work and keep track of its changes.
        /// </summary>
        /// <remarks>
        /// If the aggregate is already part of the unit of work, then the call should be ignored.
        /// </remarks>
        void Add(IPersistable aggregateRoot);

        /// <summary>
        /// If the data mapper <see cref="HasChanges"/>, then it will persist all changes to the underlying data store.
        /// </summary>
        void SubmitChanges();

        /// <summary>
        /// Gets an instance of an entity based on its functional key and the version.
        /// </summary>
        object Get(Type aggregateRootType, object key, long version = VersionedEntity.IgnoredVersion);

        /// <summary>
        /// Check if a specific entity exists.
        /// </summary>
        bool Exists(Type aggregateRootType, object key);

        /// <summary>
        /// Gets the entity with the specified id while locking it for other sessions, and optionally verifies that it has not
        /// been changed by somebody else.
        /// </summary>
        object GetWithLock(Type aggregateRootType, object key, long version = VersionedEntity.IgnoredVersion);

        /// <summary>
        /// Enlists the current unit of work in an (existing ambient) transaction.
        /// </summary>
        void EnlistTransaction();

        /// <summary>
        /// Enlists the current unit of work in an (existing ambient) transaction with the specified isolation level.
        /// </summary>
        void EnlistTransaction(IsolationLevel isolationLevel);

        /// <summary>
        /// Rolls back any changes in the current transaction.
        /// </summary>
        void RollbackTransaction();

        /// <summary>
        /// Completes the transaction.
        /// </summary>
        void CommitTransaction();

        /// <summary>
        /// Removes an individual entity from the current unit of work.
        /// </summary>
        void Evict(IPersistable aggregate);

        /// <summary>
        /// Removes all entities from the current unit of work.
        /// </summary>
        void EvictAll();

        /// <summary>
        /// Deletes all the records for a particular entity, including any additional tables within the same schema.
        /// </summary>
        /// <remarks>
        /// It will not remove any related or join tables, unless you provide the names of those tables explicitly.
        /// </remarks>
        void DeleteAllImmediately<TEntity>(params string[] tableNames);
    }
}
