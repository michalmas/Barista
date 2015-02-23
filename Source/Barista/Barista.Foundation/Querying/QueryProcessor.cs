using System.Reflection;
using System.Transactions;

using Autofac;

using Barista.Foundation.Common.Exceptions;
using Barista.Foundation.Common.Extensions;

namespace Barista.Foundation.Querying
{
    /// <summary>
    /// Represents a generic access point for executing (cross-product) queries.
    /// </summary>
    internal class QueryProcessor : IQueryProcessor
    {

        private readonly IContainer container;

        public QueryProcessor(IContainer container)
        {
            this.container = container;
        }

        /// <summary>
        /// Executes the specified query by finding an appropriate handler and passing the query to it.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="query">The query.</param>
        public TResult Execute<TResult>(IQuery<TResult> query)
        {
            using (ILifetimeScope lifetimeScope = container.BeginLifetimeScope())
            {
                var interfaceType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));

                var handler = lifetimeScope.Resolve(interfaceType);
                string methodName = StaticReflection
                    .GetMemberName<IQueryHandler<object, object>>(o => o.Handle(null));
                var handleMethod = handler.GetType().GetMethod(methodName, new[] { query.GetType() });

                using (var tx = new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.ReadCommitted
                }))
                {
                    try
                    {
                        var result = (TResult)handleMethod.Invoke(handler, new object[] { query });

                        tx.Complete();


                        return result;
                    }
                    catch (TargetInvocationException ex)
                    {
                        throw ex.Unwrap();
                    }
                }
            }

        }
    }
}
