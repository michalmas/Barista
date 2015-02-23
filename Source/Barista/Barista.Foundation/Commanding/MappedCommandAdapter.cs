using System;

using Barista.Foundation.Domain;

namespace Barista.Foundation.Commanding
{
    /// <summary>
    /// A configurable command handler that will map directly to a method on an aggregate root.
    /// </summary>
    /// <typeparam name="TCommand">The type of command that will be handled.</typeparam>
    /// <typeparam name="TAggregateRoot">The type of the aggregate root that will be mapped to.</typeparam>
    public class MappedCommandAdapter<TCommand, TAggregateRoot> : IHandleCommandOn<TCommand, TAggregateRoot>
        where TCommand : class, ICommand
        where TAggregateRoot : IAggregateRoot
    {
        private readonly Action<TCommand, TAggregateRoot> action;

        /// <summary>
        /// Created an instance of the handler.
        /// </summary>
        /// <param name="action">The action that will map the command to the method on the aggregate root.</param>
        public MappedCommandAdapter(Action<TCommand, TAggregateRoot> action)
        {
            this.action = action;
        }

        public void Handle(TCommand command, TAggregateRoot aggregate)
        {
            action(command, aggregate);
        }
    }
}
