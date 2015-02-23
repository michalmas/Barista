using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Autofac;
using Autofac.Core;

using Barista.Foundation.DataAccess;
using Barista.Foundation.Domain;

namespace Barista.Foundation.Commanding
{
    /// <summary>
    /// Fluent builder to build an aggregate based handler pipeline.
    /// </summary>
    /// <typeparam name="TUnitOfWork">Type of unit of work used by the aggregate retrieval adapter.</typeparam>
    /// <typeparam name="TCommand">Type of command to build the handler pipeline for.</typeparam>
    /// <typeparam name="TAggregateRoot">The type of the aggregate root involved in the command.</typeparam>
    public class AggregateCommandHandlerBuilder<TUnitOfWork, TCommand, TAggregateRoot> : ICommandHandlerBuilder<TCommand>
        where TUnitOfWork : DomainUnitOfWork
        where TCommand : class, ICommand
        where TAggregateRoot : class, IAggregateRoot
    {
        private ILifetimeScope lifetimeScope;
        private readonly Func<IHandleCommandOn> wrappedFactory;

        protected bool checkOwnership = true;
        protected Type policyType;
        protected Func<TCommand, TAggregateRoot, object> policyContextFactory;
        /*protected ApplyPolicyBehavior applyPolicyBehavior;*/
        protected Func<TCommand, TAggregateRoot, string> resolveKey = (c, e) => CommandNameResolver.GetName<TCommand>();

        private readonly DecoratorPipeline decoratorDecoratorPipeline = new DecoratorPipeline();

        /// <summary>
        /// Creates a new instance of the builder.
        /// </summary>
        /// <param name="lifetimeScope">The lifetimescope used to build the involved objects.</param>
        /// <param name="wrappedFactory">A factory that is used to created the handler that will be wrapped by 
        /// the result of this builder.</param>
        public AggregateCommandHandlerBuilder(ILifetimeScope lifetimeScope, Func<ILifetimeScope, IHandleCommandOn> wrappedFactory)
        {
            this.lifetimeScope = lifetimeScope;
            this.wrappedFactory = () => wrappedFactory(lifetimeScope);
        }

        /// <summary>
        /// Configures the pipeline to delegate ownership checks to the actual command handler.
        /// </summary>
        public AggregateCommandHandlerBuilder<TUnitOfWork, TCommand, TAggregateRoot> WhichIgnoresOwnership
        {
            get
            {
                checkOwnership = false;
                return this;
            }
        }

        /// <summary>
        /// Configures the handler pipeline to apply the specified policy.
        /// </summary>
        /// <typeparam name="TPolicy">The type of policy to apply.</typeparam>
        /// <typeparam name="TFactory">The factory to create a command policy context from the provided command and aggregate root.</typeparam>
        /// <param name="resolveKey">Used to resolve a custom key that is used to get the policy from the registry.</param>
        /// <param name="behavior">Indicates whether the policy should be applied before or after invoking the wrapped handler.</param>
        /// <returns>The next builder in the chain.</returns>
        /*public AggregateCommandHandlerBuilder<TUnitOfWork, TCommand, TAggregateRoot> Applies<TPolicy, TFactory>(
            Func<TCommand, TAggregateRoot, string> resolveKey = null,
            ApplyPolicyBehavior behavior = ApplyPolicyBehavior.Before)
            where TPolicy : class, IPolicy
            where TFactory : ICreateCommandPolicyContext<TCommand, TAggregateRoot>
        {
            return Applies<TPolicy>((cmd, agg) =>
            {
                var factory = lifetimeScope.Resolve<TFactory>();
                return factory.Create(cmd, agg);
            }, resolveKey, behavior);
        }*/

        /// <summary>
        /// Configures the handler pipeline to apply the specified policy.
        /// </summary>
        /// <typeparam name="TPolicy">The type of policy to apply.</typeparam>
        /// <param name="contextFactory">A factory method that is used to create the policy context that will be passed in to the policy.</param>
        /// <param name="resolveKey">Used to resolve a custom key that is used to get the policy from the registry.</param>
        /// <param name="behavior">Indicates whether the policy should be applied before or after invoking the wrapped handler.</param>
        /// <returns>The next builder in the chain.</returns>
        /*public AggregateCommandHandlerBuilder<TUnitOfWork, TCommand, TAggregateRoot> Applies<TPolicy>(
            Func<TCommand, TAggregateRoot, object> contextFactory, Func<TCommand, TAggregateRoot, string> resolveKey = null,
            ApplyPolicyBehavior behavior = ApplyPolicyBehavior.Before)
            where TPolicy : class, IPolicy
        {
            policyType = typeof(TPolicy);
            policyContextFactory = contextFactory;

            if (resolveKey != null)
            {
                this.resolveKey = resolveKey;
            }

            applyPolicyBehavior = behavior;

            return this;
        }*/

        void ICommandHandlerBuilder<TCommand>.AddDecorator(Type decoratorType)
        {
            AddDecorator(decoratorType);
        }

        public virtual AggregateCommandHandlerBuilder<TUnitOfWork, TCommand, TAggregateRoot> AddDecorator(Type decoratorType)
        {
            var genericType = AssertAndComposeDecoratorType(decoratorType);

            decoratorDecoratorPipeline.AddDecorator(
                f => () => (IHandleCommandOn)lifetimeScope.Resolve(genericType, GetDecoratorParameters(genericType, f)));

            return this;
        }

        private static Type AssertAndComposeDecoratorType(Type decoratorType)
        {
            Type[] genericArguments = decoratorType.GetGenericArguments();

            if (genericArguments.Length != 2)
            {
                throw new ArgumentException("The decorator type should have 2 generic type arguments.");
            }

            Type genericType = decoratorType.MakeGenericType(typeof(TCommand), typeof(TAggregateRoot));

            Type typedInterface = typeof(IHandleCommandOn<,>).MakeGenericType(typeof(TCommand), typeof(TAggregateRoot));

            if (!typedInterface.IsAssignableFrom(genericType))
            {
                throw new ArgumentException("The decorator should inherit from the IHandleCommandOn");
            }

            return genericType;
        }

        private Parameter[] GetDecoratorParameters(Type decoratorType, Func<IHandleCommandOn> wrappedFactory)
        {
            IList<Parameter> parameters = new List<Parameter>();

            if (RequiresLifetimeScopeSetter(decoratorType))
            {
                parameters.Add(TypedParameter.From(new Action<ILifetimeScope>(l => lifetimeScope = l)));
            }

            parameters.Add(TypedParameter.From(wrappedFactory));

            return parameters.ToArray();
        }

        private bool RequiresLifetimeScopeSetter(Type decoratorType)
        {
            var lifetimeScopeSetterType = typeof(Action<ILifetimeScope>);

            return ConstructorContainsArgumentOfType(decoratorType, lifetimeScopeSetterType);
        }

        private bool ConstructorContainsArgumentOfType(Type decoratorType, Type parameterType)
        {
            ConstructorInfo constructor = decoratorType.GetConstructors().Single();
            var parameters = constructor.GetParameters();

            return parameters.Any(parameterInfo => parameterInfo.ParameterType == parameterType);
        }

        /// <summary>
        /// Builds the handler pipeline.
        /// </summary>
        /// <returns>An instance to the handle.</returns>
        IHandleCommand<TCommand> ICommandHandlerBuilder<TCommand>.Build()
        {
            // Add the standard decorators to the top of the stack
            /*AddDeploymentOwnerCheckingDecorator();
            AddPolicyApplyingDecorator();*/

            return AdaptToHandleCommandOn(decoratorDecoratorPipeline.Wrap(wrappedFactory));
        }

        /*private void AddDeploymentOwnerCheckingDecorator()
        {
            if (checkOwnership)
            {
                AddDecorator(typeof(OwnershipCheckingDecorator<,>));
            }
        }*/

        /*private void AddPolicyApplyingDecorator()
        {
            if (policyType != null)
            {
                decoratorDecoratorPipeline.AddDecorator(x => () =>
                    lifetimeScope.Resolve<PolicyApplyingDecorator<TCommand, TAggregateRoot>>(
                        TypedParameter.From(policyType),
                        TypedParameter.From(policyContextFactory),
                        TypedParameter.From(applyPolicyBehavior),
                        TypedParameter.From(x),
                        TypedParameter.From(resolveKey)));
            }
        }*/

        private IHandleCommand<TCommand> AdaptToHandleCommandOn(Func<IHandleCommandOn> wrapped)
        {
            Func<ICreateAggregate<TCommand, TAggregateRoot>> aggregateFactory =
                () => wrappedFactory() as ICreateAggregate<TCommand, TAggregateRoot>;

            var uowFactory = lifetimeScope.Resolve<Func<TUnitOfWork>>();

            var wrappedFactoryTypedParameter = new TypedParameter(typeof(Func<IHandleCommandOn>), wrapped);
            var aggregateFactoryTypedParameter = new TypedParameter(typeof(Func<ICreateAggregate<TCommand, TAggregateRoot>>),
                aggregateFactory);

            var adapter = lifetimeScope.Resolve<AggregateRetrievalAdapter<TCommand, TAggregateRoot>>
                (TypedParameter.From<Func<DomainUnitOfWork>>(uowFactory), wrappedFactoryTypedParameter,
                    aggregateFactoryTypedParameter);

            return adapter;
        }
    }
}
