using System;

using Barista.Foundation.DataAccess;

namespace Barista.Foundation.Commanding
{
    public interface IHandlerRegistry<TUnitOfWork>
        where TUnitOfWork : DomainUnitOfWork
    {
        /// <summary>
        /// Adds a new handler for a certain command.
        /// </summary>
        /// <typeparam name="TCommand">The type of command to add the handler for.</typeparam>
        /// <param name="handler">A build method used to build the handler.</param>
        void AddFor<TCommand>(Func<CommandHandlerBuilder<TUnitOfWork, TCommand>, ICommandHandlerBuilder<TCommand>> handler)
            where TCommand : class, ICommand;

        /// <summary>
        /// Adds a decorator that should be added to the pipeline of all handlers.
        /// </summary>
        /// <param name="decoratorType">The type of decorator. Should be a generic type with two type parameters. Command & Aggregate</param>
        void AddDefaultDecorator(Type decoratorType);
    }
}
