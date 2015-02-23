using System;

using Autofac;

using Barista.Foundation.DataAccess;
using Barista.Foundation.Domain;

namespace Barista.Foundation.Commanding
{
    /// <summary>
    /// Fluent builder to build a command handler pipeline.
    /// </summary>
    /// <typeparam name="TUnitOfWork">Type of unit of work used by the aggregate retrieval adapter.</typeparam>
    /// <typeparam name="TCommand">Type of command to build the handler pipeline for.</typeparam>
    public class CommandHandlerBuilder<TUnitOfWork, TCommand> : ICommandHandlerBuilder<TCommand>
        where TUnitOfWork : DomainUnitOfWork
        where TCommand : class, ICommand
    {
        private readonly ILifetimeScope lifetimeScope;

        private Func<IHandleCommand<TCommand>> handlerFactory;

        public CommandHandlerBuilder(ILifetimeScope lifetimeScope)
        {
            this.lifetimeScope = lifetimeScope;
        }

        /// <summary>
        /// Configures the command to be handled directly by a method on an aggregate.
        /// </summary>
        /// <typeparam name="TEntity">The type of aggregate to map to.</typeparam>
        /// <param name="action">The action that will map from the command to the aggregate method.</param>
        /// <returns>The next builder in the chain.</returns>
        public virtual AggregateCommandHandlerBuilder<TUnitOfWork, TCommand, TEntity> MapsTo<TEntity>(Action<TCommand, TEntity> action)
            where TEntity : class, IAggregateRoot
        {
            return new AggregateCommandHandlerBuilder<TUnitOfWork, TCommand, TEntity>(lifetimeScope,
                _ => new MappedCommandAdapter<TCommand, TEntity>(action));
        }

        /// <summary>
        /// Configures the command to be handled by an aggregate handler. This will cause the aggregate to be retreived and be 
        /// passed into the handler.
        /// </summary>
        /// <typeparam name="TCommandHandler">The type of the handler to invoke.</typeparam>
        /// <typeparam name="TEntity">The type of the aggregate that will be retrieved and passed into the handler.</typeparam>
        /// <returns>The next builder in the chain.</returns>
        public virtual AggregateCommandHandlerBuilder<TUnitOfWork, TCommand, TEntity> HandledBy<TCommandHandler, TEntity>()
            where TCommandHandler : IHandleCommandOn
            where TEntity : class, IAggregateRoot
        {
            return new AggregateCommandHandlerBuilder<TUnitOfWork, TCommand, TEntity>(lifetimeScope,
                l => l.Resolve<TCommandHandler>());
        }

        /// <summary>
        /// Configures the command to be handled by a custom handler. Basically skipping the whole handler pipeline.
        /// </summary>
        /// <typeparam name="TCommandHandler">The type of the handler to invoke.</typeparam>
        /// <returns>The next builder in the chain.</returns>
        public virtual CommandHandlerBuilder<TUnitOfWork, TCommand> HandledBy<TCommandHandler>()
            where TCommandHandler : IHandleCommand<TCommand>
        {
            handlerFactory = () => lifetimeScope.Resolve<TCommandHandler>();

            return this;
        }

        /// <summary>
        /// Decorators are not supported on native handlers, so ignore.
        /// </summary>
        /// <param name="decoratorType"></param>
        public void AddDecorator(Type decoratorType)
        { }

        /// <summary>
        /// Builds the handler pipeline.
        /// </summary>
        /// <returns>An instance to the handle.</returns>
        IHandleCommand<TCommand> ICommandHandlerBuilder<TCommand>.Build()
        {
            return handlerFactory();
        }
    }
}
