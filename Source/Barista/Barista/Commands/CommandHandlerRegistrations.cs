using Barista.Commands.Handlers;
using Barista.DataAccess;
using Barista.Domain;
using Barista.Foundation.Commanding;

namespace Barista.Commands
{
    public class CommandHandlerRegistrations : IRegisterCommandHandlers<BaristaUnitOfWork>
    {
        public void Register(IHandlerRegistry<BaristaUnitOfWork> handlers)
        {
            handlers.AddFor<CreateOrderCommand>(h => h
                .HandledBy<CreateOrderHandler, Order>());

            handlers.AddFor<AddOrderItemCommand>(h => h
                .MapsTo<Order>((c, x) => x.AddItem(c.ProductName, c.Quantity)));
        }
    }
}
