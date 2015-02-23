using System;

namespace Barista.Foundation.Commanding
{
    /// <summary>
    /// Represents a command that acts on the aggregate root identified by the <see cref="Identity"/>.
    /// </summary>
    public interface IAggregateRootCommand : ICommand
    {
        object Identity { get; }
    }

    /// <summary>
    /// Represents a command that acts on the aggregate root identified by the <see cref="Identity"/>.
    /// </summary>
    /// <typeparam name="TIdentity">The <see cref="Type"/> of the key (<see cref="string"/> or <see cref="Guid"/>.</typeparam>
    public class AggregateRootCommand<TIdentity> : Command, IAggregateRootCommand
    {
        protected AggregateRootCommand()
        {
        }

        protected AggregateRootCommand(TIdentity identity)
        {
            Identity = identity;
        }

        public TIdentity Identity { get; set; }

        object IAggregateRootCommand.Identity { get { return Identity; } }
    }
}
