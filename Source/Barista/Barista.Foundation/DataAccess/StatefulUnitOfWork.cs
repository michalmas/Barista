using System;
using Barista.Foundation.Domain;

namespace Barista.Foundation.DataAccess
{
    public abstract class StatefulUnitOfWork : UnitOfWork
    {
        protected StatefulUnitOfWork(IDataMapper mapper)
            : base(mapper)
        {
        }

        /// <summary>
        /// Connects an existing unit of work to a <see cref="IDataMapper"/> which lifecycle control is out of scope.
        /// </summary>
        public void ConnectToSharedMapper(IDataMapper mapper)
        {
            Mapper = mapper;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Removes an individual entity from the current unit of work.
        /// </summary>
        public virtual void Evict(IPersistable aggregate)
        {
            Mapper.Evict(aggregate);
        }

        /// <summary>
        /// Removes all entities from the current unit of work.
        /// </summary>
        public virtual void EvictAll()
        {
            Mapper.EvictAll();
        }
    }
}
