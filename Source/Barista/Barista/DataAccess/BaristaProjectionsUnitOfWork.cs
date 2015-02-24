using Barista.Foundation.DataAccess;

namespace Barista.DataAccess
{
    public class BaristaProjectionsUnitOfWork : ProjectionsUnitOfWork
    {
        public BaristaProjectionsUnitOfWork(IDataMapper dataMapper)
            : base(dataMapper)
        {
        }
    }
}
