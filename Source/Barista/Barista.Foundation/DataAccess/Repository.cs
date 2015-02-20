using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Barista.Foundation.DataAccess
{
    public abstract class Repository<T> : IRepository<T>
    {
        protected abstract IQueryable<T> Entities { get; }

        public virtual int Count
        {
            get { return Entities.Count(); }
        }

        public Type ElementType
        {
            get { return Entities.ElementType; }
        }

        public Expression Expression
        {
            get { return Entities.Expression; }
        }

        public IQueryProvider Provider
        {
            get { return Entities.Provider; }
        }

        public abstract T Find(object identity);

        protected abstract void AddAggregateRoot(T entity);

        public void AddRange(IEnumerable<T> entities)
        {
            foreach (T entity in entities)
            {
                Add(entity);
            }
        }

        public void Add(T entity)
        {
            AddAggregateRoot(entity);
        }

        protected virtual void UpdateAggregateRoot(T entity)
        {
            throw new NotSupportedException("Explicit updates are not supported by this repository.");
        }

        public void Update(T entity)
        {
            UpdateAggregateRoot(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            foreach (T entity in entities)
            {
                Remove(entity);
            }
        }

        protected abstract void RemoveAggregateRoot(T entity);

        public void Remove(T entity)
        {
            RemoveAggregateRoot(entity);
        }

        public virtual void Clear()
        {
            RemoveRange(Entities);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Entities.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Entities.GetEnumerator();
        }
    }
}