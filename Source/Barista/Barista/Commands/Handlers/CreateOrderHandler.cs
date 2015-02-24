using Barista.DataAccess;
using Barista.Foundation.Commanding;
using Barista.Domain;
using System;

namespace Barista.Commands.Handlers
{
    public class CreateOrderHandler : ICreateAggregate<CreateOrderCommand, Order>
    {
        private readonly Func<BaristaUnitOfWork> _uowFactory;

        public CreateOrderHandler(Func<BaristaUnitOfWork> uowFactory)
        {
            _uowFactory = uowFactory;
        }

        public Order Create(CreateOrderCommand command)
        {
            using (var uow = _uowFactory())
            {
                var order = new Order(command.Identity, command.BaristaName);

                uow.Add(order);

                return order;
            }
        }
    }
}
