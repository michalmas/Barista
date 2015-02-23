using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using Autofac;

using Barista.Foundation.Common.Exceptions;
using Barista.Foundation.Common.Extensions;
using Barista.Foundation.DataAccess;
using Barista.Foundation.Domain;

namespace Barista.Foundation.Commanding
{
    /// <summary>
    /// Represents a batch of commands that are executed as a single atomic operation.
    /// </summary>
    internal class CommandBatch
    {
        private readonly Func<DomainUnitOfWork> unitOfWorkFactory;
        private readonly ILifetimeScope lifetimeScope;

        public CommandBatch(Func<DomainUnitOfWork> unitOfWorkFactory, ILifetimeScope lifetimeScope)
        {
            this.unitOfWorkFactory = unitOfWorkFactory;
            this.lifetimeScope = lifetimeScope;
        }

        public void Execute(IEnumerable<ICommand> commands)
        {
            DomainUnitOfWork uow = unitOfWorkFactory();

            try
            {
                uow.EnlistTransaction();

                ExecuteCommands(commands);

                uow.CommitTransaction();
            }
            finally
            {
                uow.Dispose();
            }
        }

        public void ExecuteNested(IEnumerable<ICommand> commands)
        {
            DisableConcurrencyChecks(commands, 0);
            ExecuteCommands(commands);
        }

        private void ExecuteCommands(IEnumerable<ICommand> commands)
        {
            /*using (IViolationContext violationContext = ViolationContext.CreateOrReuseCollectingViolationContext())
            {*/
                foreach (ICommand command in commands)
                {
                    var handler = GetHandler(command);

                    ExecuteHandler(command, handler);
                }

                /*violationContext.ThrowIfAny();
            }*/
        }

        private object GetHandler(ICommand command)
        {
            Type genericHandlerType = typeof(IHandleCommand<>);

            Type handlerType = genericHandlerType.MakeGenericType(command.GetType());

            if (!lifetimeScope.IsRegistered(handlerType))
            {
                throw new InvalidOperationException("Could not find a known or mapped handler for command " +
                                                    command.GetType().Name);
            }

            return lifetimeScope.Resolve(handlerType);
        }

        [DebuggerStepThrough]
        private static void ExecuteHandler(ICommand command, object handler)
        {
            string methodName = StaticReflection
                .GetMemberName<IHandleCommand<object>>(o => o.Handle(null));
            MethodInfo methodInfo = handler.GetType().GetMethod(methodName, new[] { command.GetType() });

            try
            {
                methodInfo.Invoke(handler, new[] { command });
            }
            catch (TargetInvocationException ex)
            {
                throw ex.Unwrap();
            }
        }

        private static void DisableConcurrencyChecks(IEnumerable<ICommand> commands, int itemsToSkip)
        {
            foreach (ICommand command in commands.Skip(itemsToSkip))
            {
                DisableConcurrencyCheck(command);
            }
        }

        private static void DisableConcurrencyCheck(ICommand command)
        {
            var updateCommand = command as IVersionedCommand;
            if (updateCommand != null)
            {
                updateCommand.Version = VersionedEntity.IgnoredVersion;
            }
        }
    }
}
