using NHibernate;

namespace Barista.Foundation.DataAccess.NHibernate
{
    /// <summary>
    /// Represents a way for extension methods to optimize a query on <see cref="Repository{T}"/> 
    /// </summary>
    public interface INHibernateRepository
    {
        ISession Session { get; }
    }
}
