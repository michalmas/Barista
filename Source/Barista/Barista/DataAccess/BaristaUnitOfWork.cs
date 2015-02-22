using Barista.Foundation.DataAccess;

namespace Barista.DataAccess
{
    public class BaristaUnitOfWork : DomainUnitOfWork
    {
        public BaristaUnitOfWork(IDataMapper mapper)
            : base(mapper)
        {
        }
    }
}
