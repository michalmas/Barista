using System.Collections.Generic;
using System.Linq;
using Barista.Foundation.Domain;

namespace Barista.Foundation.DataAccess
{
    public interface IRepository<T> : IQueryable<T>
        where T : class, IPersistable
    {
        int Count { get; }

        T Find(object identity);

        void AddRange(IEnumerable<T> entities);
        
        void Add(T entity);

        void Update(T entity);

        void RemoveRange(IEnumerable<T> entities);
        
        void Remove(T entity);

        void Clear();
    }
}
