using Barista.Foundation.Domain;

namespace Barista.Foundation.Commanding
{
    public static class CommandHandlerExtensions
    {
        public static void Handle<TCommand, TAggregateRoot>(
            this IHandleCommandOn wrapped, TCommand command, TAggregateRoot aggregate)
            where TCommand : class, ICommand
            where TAggregateRoot : IAggregateRoot
        {
            var handler = wrapped as IHandleCommandOn<TCommand, TAggregateRoot>;
            if (handler != null)
            {
                handler.Handle(command, aggregate);
            }
        }
    }
}
