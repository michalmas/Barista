using System.Linq;

using NHibernate;
using NHibernate.Linq;

namespace Barista.Foundation.DataAccess.NHibernate
{
    internal class NHibernateRepository<T> : Repository<T>, INHibernateRepository 
        where T : class, IPersistable
    {
        private readonly ISession session;
        private IQueryable<T> queryable;
        private ISession originalSession;

        public NHibernateRepository(ISession session)
        {
            this.session = session;
        }

        ISession INHibernateRepository.Session
        {
            get { return session; }
        }

        /// <summary>
        ///   Provides access to the repository-specific LINQ-enabled NHibernate collection.
        /// </summary>
        /// <remarks>
        ///   <see name = "IQueryable{T}" />.
        /// </remarks>
        protected override IQueryable<T> Entities
        {
            get
            {
                // In cases that a reference to a repository is maintained over multiple post-backs, the session belonging to the
                // queryable will be invalid (closed). This results in an exception when performing queries on the queryable.
                // This behavior is caused by keeping the queryable in a variable. To workaround this problem, we create a new
                // queryable when the current session is a different one that we originally created the queryable with.
                if ((queryable == null) || (originalSession != session))
                {
                    originalSession = session;
                    queryable = session.Query<T>();
                }

                return queryable;
            }
        }

        protected override void AddAggregateRoot(T entity)
        {
            session.Save(entity);
        }

        protected override void RemoveAggregateRoot(T entity)
        {
            session.Delete(entity);
        }

        /// <summary>
        /// Returns the object identified by <paramref name="identity"/> or <c>null</c> if no such object exists.
        /// </summary>
        public override T Find(object identity)
        {
            return session.Get<T>(identity);
        }
    }
}
