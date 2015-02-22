using System;
using System.Collections.Generic;
using Barista.Foundation.DataAccess;
using Barista.Foundation.EventSourcing;

namespace Barista.DataAccess
{
    public class BaristaUnitOfWorkFactory : EventStoreUnitOfWorkFactory<BaristaUnitOfWork>,
        IBaristaUnitOfWorkFactory
    {
        private static readonly Dictionary<Type, Type> dehydrationTypeMap =
            GetDehydrationTypeMap(typeof(BaristaUnitOfWorkFactory).Assembly);

        public BaristaUnitOfWorkFactory(Lazy<IStoreEvents> eventStore)
            : base(eventStore, dehydrationTypeMap)
        {
        }

        protected override BaristaUnitOfWork CreateUnitOfWorkMappedTo(IDataMapper dataMapper)
        {
            return new BaristaUnitOfWork(dataMapper);
        }
    }
}
