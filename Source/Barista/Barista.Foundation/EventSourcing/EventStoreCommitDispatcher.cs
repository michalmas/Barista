using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

using Barista.Foundation.Common.Extensions;
using Barista.Foundation.DataAccess;

using IsolationLevel = System.Data.IsolationLevel;

namespace Barista.Foundation.EventSourcing
{
    /// <summary>
    /// Generic Event Sourcing dispatcher that wraps the denormalization of a commit with a particular unit of work.
    /// </summary>
    public class EventStoreCommitDispatcher<TUnitOfWork> : ICommitDispatcher
        where TUnitOfWork : ProjectionsUnitOfWork
    {
        private readonly int batchSizeForMinimalLockingAndOptimalPerformance = 30;

        private readonly Func<UnitOfWorkOption, TUnitOfWork> uowFactory;
        private readonly IDenormalizerProvider denormalizerProvider;

        public EventStoreCommitDispatcher(Func<UnitOfWorkOption, TUnitOfWork> uowFactory,
            IDenormalizerProvider denormalizerProvider)
        {
            this.uowFactory = uowFactory;
            this.denormalizerProvider = denormalizerProvider;
        }

        /// <summary>
        /// Indicates that a batch of commits will be dispatched.
        /// </summary>
        public IDisposable BeginBatch()
        {
            return uowFactory(UnitOfWorkOption.CreateNew);
        }

        public void Dispatch(ICommit commit)
        {
            Dispatch(commit.Events, new DispatchContext
            {
                TimeStampUtc = commit.CommitStamp
            });
        }

        public void Redispatch(ICommit commit)
        {
            Dispatch(commit.Events, new DispatchContext
            {
                TimeStampUtc = commit.CommitStamp,
                IsRedispatch = true
            });
        }

        private void Dispatch(IEnumerable<EventMessage> events, DispatchContext context)
        {
            DenormalizationCommand[] commands = events
                .SelectMany(@event => DenormalizationCommandBuilder.BuildsCommandsFor(@event, e => denormalizerProvider.Get(e).Where(RelatesToUnitOfWork))).ToArray();

            DenormalizationCommand current = null;

            try
            {
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    commands.Batch(batch =>
                    {
                        using (var uow = uowFactory(UnitOfWorkOption.ExistingOrNew))
                        {
                            EnlistTransaction(uow);

                            foreach (DenormalizationCommand command in batch)
                            {
                                current = command;

                                ExecuteCommand(command, context);
                            }

                            CommitTransaction(uow);
                        }
                    }, batchSizeForMinimalLockingAndOptimalPerformance);
                }
            }
            catch (Exception exception)
            {
                //throw new DenormalizationException(exception, commands, current);
                throw;
            }
        }

        protected virtual void EnlistTransaction(TUnitOfWork uow)
        {
            uow.EnlistTransaction(IsolationLevel.ReadCommitted);
        }

        protected virtual void ExecuteCommand(DenormalizationCommand command, DispatchContext context)
        {
            command.Execute(context);
        }

        protected virtual void CommitTransaction(TUnitOfWork uow)
        {
            uow.CommitTransaction();
        }

        protected virtual bool RelatesToUnitOfWork(object denormalizer)
        {
            return (denormalizer.GetType().Assembly == typeof(TUnitOfWork).Assembly);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
        }
    }
}
