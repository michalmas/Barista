using Barista.Foundation.DataAccess.NHibernate;

namespace Barista.DataAccess
{
    public class BaristaProjectionsUnitOfWorkFactory : NHibernateUnitOfWorkFactory<BaristaProjectionsUnitOfWork>,
        IBaristaProjectionsUnitOfWorkFactory
    {
        public BaristaProjectionsUnitOfWorkFactory()
            : base(null, null)
        {
        }

        protected override BaristaProjectionsUnitOfWork CreateUnitOfWorkMappedTo(INHibernateDataMapper dataMapper)
        {
            return new BaristaProjectionsUnitOfWork(dataMapper);
        }
    }
}
