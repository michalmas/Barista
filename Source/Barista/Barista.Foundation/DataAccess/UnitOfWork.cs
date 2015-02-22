using System;
using System.Data;
using System.Threading;

namespace Barista.Foundation.DataAccess
{
    public abstract class UnitOfWork : IDisposable
    {
        private IDataMapper mapper;
        private readonly object syncObject = new object();
        private int referenceCount;
        private readonly long id;
        private static long nextId = 1;

        protected UnitOfWork(IDataMapper mapper)
        {
            this.mapper = mapper;

            id = Interlocked.Increment(ref nextId);
            referenceCount = 1;
        }

        public IDataMapper Mapper
        {
            get { return mapper; }
            protected set { mapper = value; }
        }

        /// <summary>
        /// Occurs when the unit of work is completed disposed.
        /// </summary>
        public EventHandler Disposing = delegate { };

        /// <summary>
        /// Gets or sets a value indicating whether this unit of work has already been disposed.
        /// </summary>
        internal bool IsDisposed { get; private set; }

        /// <summary>
        /// Persists all pending changes.
        /// </summary>
        public void SubmitChanges()
        {
            Mapper.SubmitChanges();
        }

        /// <summary>
        /// Enlists in an ambient transaction, or, if none is available, creates a new transaction.
        /// </summary>
        public void EnlistTransaction()
        {
            mapper.EnlistTransaction();
        }

        /// <summary>
        /// Enlists in an ambient transaction, or, if none is available, creates a new transaction.
        /// </summary>
        /// <param name="isolationLevel">The isolation level.</param>
        public void EnlistTransaction(IsolationLevel isolationLevel)
        {
            mapper.EnlistTransaction(isolationLevel);
        }

        /// <summary>
        /// Flags the transaction as complete, and if no ambient transaction exists, commits the changes to the data store.
        /// </summary>
        public void CommitTransaction()
        {
            mapper.CommitTransaction();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            lock (syncObject)
            {
                referenceCount--;
                if (referenceCount == 0)
                {
                    if (mapper.HasTransaction)
                    {
                        mapper.RollbackTransaction();
                    }

                    Disposing(this, EventArgs.Empty);
                    mapper.Dispose();
                    IsDisposed = true;

                    GC.SuppressFinalize(this);
                }
            }
        }

        internal void IncreaseReferenceCount()
        {
            lock (syncObject)
            {
                referenceCount++;
            }
        }
    }
}
