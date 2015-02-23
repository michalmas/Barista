using Barista.Foundation.Domain;

namespace Barista.Foundation.Commanding
{
    /// <summary>
    /// Marker interface. Don't directly inherit from this interface.
    /// </summary>
    public interface IHandleCommandOn
    {
    }

    /// <summary>
    /// Defines the command handler that executes domain behavior on existing aggregate root
    /// </summary>
    public interface IHandleCommandOn<in TCommand, in TAggregateRoot> : IHandleCommandOn
        where TAggregateRoot : IAggregateRoot
        where TCommand : class, ICommand //IAggregateRootCommand
    {
        /// <summary>
        /// Handles the command for existing aggregate root
        /// </summary>
        void Handle(TCommand command, TAggregateRoot aggregate);
    }
}
