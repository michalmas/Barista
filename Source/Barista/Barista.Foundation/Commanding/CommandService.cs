using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web;

using Autofac;

using Barista.Foundation.Common.Extensions;
using Barista.Foundation.DataAccess;

namespace Barista.Foundation.Commanding
{
    /// <summary>
    /// Handles the execution of a batch of commands as a single atomic operation.
    /// </summary>
    public abstract class CommandService : ICommandService
    {
        private readonly ILifetimeScope lifetimeScope;
        private readonly Func<DomainUnitOfWork> unitOfWorkFactory;
        //private readonly Func<IEventPublishingContext> eventPublishingContextFactory;
        private readonly string contextKey;

        protected CommandService(ILifetimeScope lifetimeScope, Func<DomainUnitOfWork> unitOfWorkFactory/*,
            Func<IEventPublishingContext> eventPublishingContextFactory*/)
        {
            this.lifetimeScope = lifetimeScope;
            this.unitOfWorkFactory = unitOfWorkFactory;
            //this.eventPublishingContextFactory = eventPublishingContextFactory;
            contextKey = GetType().FullName;
        }

        /// <summary>
        /// Executes a single command.
        /// </summary>
        public virtual void Execute(ICommand command)
        {
            Execute(command.ToEnumerable());
        }

        /// <summary>
        /// Executes a batch of commands as one atomic operation.
        /// </summary>
        public void Execute(IEnumerable<ICommand> commands)
        {
            if (CurrentBatch == null)
            {
                try
                {
                    CurrentBatch = new CommandBatch(unitOfWorkFactory, lifetimeScope);
                    CurrentBatch.Execute(commands);
                }
                finally
                {
                    CurrentBatch = null;
                }

                //eventPublishingContextFactory().PublishIfAny();
            }
            else
            {
                CurrentBatch.ExecuteNested(commands);
            }
        }

        private CommandBatch CurrentBatch
        {
            get
            {
                // See http://piers7.blogspot.com/2005/11/threadstatic-callcontext-and_02.html
                if (HttpContext.Current != null)
                {
                    return HttpContext.Current.Items[contextKey] as CommandBatch;
                }
                else
                {
                    return CallContext.GetData(contextKey) as CommandBatch;
                }
            }
            set
            {
                if (HttpContext.Current != null)
                {
                    HttpContext.Current.Items[contextKey] = value;
                }
                else
                {
                    CallContext.SetData(contextKey, value);
                }
            }
        }
    }
}
