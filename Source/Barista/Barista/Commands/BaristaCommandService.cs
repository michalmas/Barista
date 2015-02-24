using System;

using Autofac;

using Barista.Foundation.Commanding;
using Barista.Foundation.DataAccess;

namespace Barista.Commands
{
    public class BaristaCommandService : CommandService
    {
        public BaristaCommandService(ILifetimeScope lifetimeScope, Func<DomainUnitOfWork> unitOfWorkFactory) : 
            base(lifetimeScope, unitOfWorkFactory)
        {
        }
    }
}
