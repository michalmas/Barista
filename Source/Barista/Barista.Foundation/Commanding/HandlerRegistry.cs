using System;
using System.Collections.Generic;

using Autofac;

using Barista.Foundation.DataAccess;

namespace Barista.Foundation.Commanding
{
    /// <summary>
    /// Acts like a brigde between implementors of the <see cref="IRegisterCommandHandlers{TUnitOfWork}" /> interface and the 
    /// commanding module.
    /// </summary>
    public class HandlerRegistry<TUnitOfWork> : IHandlerRegistry<TUnitOfWork>
        where TUnitOfWork : DomainUnitOfWork
    {
        private readonly ContainerBuilder builder;
        private readonly IList<Type> defaultDecorators = new List<Type>();

        public HandlerRegistry(ContainerBuilder builder)
        {
            this.builder = builder;
        }

        public virtual void AddDefaultDecorator(Type decoratorType)
        {
            defaultDecorators.Add(decoratorType);
        }

        public virtual void AddFor<TCommand>(
            Func<CommandHandlerBuilder<TUnitOfWork, TCommand>, ICommandHandlerBuilder<TCommand>> handler)
            where TCommand : class, ICommand
        {
            builder.Register(c =>
            {
                // Use the container here instead of the component context. This is because of the region specific configuration.
                // The region specific lifetime scopes need to be resolved dynamically. This can only be done through the container.
                var commandHandlerBuilder =
                    new CommandHandlerBuilder<TUnitOfWork, TCommand>(c.Resolve<ILifetimeScope>());

                ICommandHandlerBuilder<TCommand> myBuilder = handler(commandHandlerBuilder);

                foreach (var defaultDecorator in defaultDecorators)
                {
                    myBuilder.AddDecorator(defaultDecorator);
                }

                return myBuilder.Build();
            });
        }
    }
}
