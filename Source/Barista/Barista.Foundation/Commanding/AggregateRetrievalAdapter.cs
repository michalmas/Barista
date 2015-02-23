using System;

using Barista.Foundation.DataAccess;
using Barista.Foundation.Domain;

namespace Barista.Foundation.Commanding
{
    /// <summary>
    /// Adapter implementation that can be inserted into the command handler pipeline to adapt the <see cref="IHandleCommand{TCommand}"/> interface
    /// to the more specific <see cref="IHandleCommandOn{TCommand,TAggregateRoot}" /> interface. The aggregate will be retrieved from the unit of work
    /// based on the key value of the command. 
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    /// <typeparam name="TAggregateRoot"></typeparam>
    public class AggregateRetrievalAdapter<TCommand, TAggregateRoot> : IHandleCommand<TCommand>
        where TAggregateRoot : class, IAggregateRoot
        where TCommand : class, ICommand
    {
        private readonly Func<DomainUnitOfWork> uowFactory;
        private readonly Func<IHandleCommandOn> getWrappedHandler;
        private readonly Func<ICreateAggregate<TCommand, TAggregateRoot>> getWrappedFactory;

        /// <summary>
        /// Creates a new instance of the decorator.
        /// </summary>
        /// <param name="uowFactory">The factory method used to get a unit of work.</param>
        /// <param name="getWrappedHandler">Gets the more specific handler to which this adapter will adapt.</param>
        /// <param name="getWrappedFactory">If available, gets a factory that can be used for creating a new aggregate;</param>
        public AggregateRetrievalAdapter(Func<DomainUnitOfWork> uowFactory, Func<IHandleCommandOn> getWrappedHandler,
            Func<ICreateAggregate<TCommand, TAggregateRoot>> getWrappedFactory)
        {
            this.uowFactory = uowFactory;
            this.getWrappedHandler = getWrappedHandler;
            this.getWrappedFactory = getWrappedFactory;
        }

        public void Handle(TCommand command)
        {
            TAggregateRoot aggregate = TryGetAggregate(command) ?? TryCreateAggregate(command);

            if (aggregate != null)
            {
                var wrappedHandler = getWrappedHandler();

                wrappedHandler.Handle(command, aggregate);
            }
        }

        private TAggregateRoot TryGetAggregate(TCommand command)
        {
            TAggregateRoot aggregate = null;

            using (var uow = uowFactory())
            {
                var aggregateCommand = command as IAggregateRootCommand;
                if (aggregateCommand != null && uow.Exists<TAggregateRoot>(aggregateCommand.Identity))
                {
                    var versionedCommand = command as IVersionedCommand;
                    long version = (versionedCommand != null) ? versionedCommand.Version : VersionedEntity.IgnoredVersion;
                    aggregate = uow.Get<TAggregateRoot>(aggregateCommand.Identity, version);
                }
            }

            return aggregate;
        }

        private TAggregateRoot TryCreateAggregate(TCommand command)
        {
            TAggregateRoot aggregate = null;

            var creationalHandler = getWrappedFactory();
            if (creationalHandler != null)
            {
                aggregate = creationalHandler.Create(command);
            }

            return aggregate;
        }
    }
}
