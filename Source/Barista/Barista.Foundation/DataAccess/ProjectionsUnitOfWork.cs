using System;

using Barista.Foundation.Domain;

namespace Barista.Foundation.DataAccess
{
    /// <summary>
    /// Represents a unit of work that should be used for maintaining the projections store.
    /// </summary>
    public abstract class ProjectionsUnitOfWork : StatefulUnitOfWork
    {
        protected ProjectionsUnitOfWork(IDataMapper mapper)
            : base(mapper)
        {
        }

        public object GetWithLock(Type entityType, object key)
        {
            return Mapper.GetWithLock(entityType, key);
        }

        /// <summary>
        /// Returns a repository for adding, removing or querying persistable objects.
        /// </summary>
        public Repository<T> GetRepository<T>() where T : class, IPersistable
        {
            return Mapper.GetRepository<T>();
        }

        /// <summary>
        /// Deletes all the records for a particular entity, including any additional tables within the same schema.
        /// </summary>
        /// <remarks>
        /// It will not remove any related or join tables, unless you provide the names of those tables explicitly.
        /// </remarks>
        public void DeleteAllStatelessly<T>(params string[] tableNames) where T : class, IPersistable
        {
            Mapper.DeleteAllImmediately<T>(tableNames);
        }
    }
}
