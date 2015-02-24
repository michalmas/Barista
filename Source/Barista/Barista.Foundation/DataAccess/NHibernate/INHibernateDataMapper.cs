using System.Data;

using NHibernate;

namespace Barista.Foundation.DataAccess.NHibernate
{
    public interface INHibernateDataMapper : IDataMapper
    {
        /// <summary>
        /// Gets the underlying <see cref="IDbConnection"/>
        /// </summary>
        IDbConnection Connection { get; }

        /// <summary>
        /// Creates a new <see cref="IQuery"/> for the given query string
        /// </summary>
        IQuery CreateQuery(string queryString);

        /// <summary>
        /// Creates a new <see cref="ISQLQuery"/> for the given SQL query string
        /// </summary>
        ISQLQuery CreateSQLQuery(string queryString);
    }
}
