using Barista.Foundation.Domain;

namespace Barista.Foundation.Commanding
{
    /// <summary>
    /// Specifies that the target command handler is responsbile for creating the instance of aggregate, 
    /// if it does not exist yet. Otherwise it behaves as a regular command handler
    /// </summary>
    public interface ICreateAggregate<TCommand, TAggregateRoot> : IHandleCommandOn
        where TCommand : class, ICommand
        where TAggregateRoot : class, IAggregateRoot
    {
        /// <summary>
        /// Creates a new instance of aggregate root  based on command's data.
        /// <remarks>This method is executed only when corresponding aggregate does not exist, 
        /// i.e. could not be retrieved based on command's identity</remarks>
        /// </summary>
        TAggregateRoot Create(TCommand command);
    }
}
