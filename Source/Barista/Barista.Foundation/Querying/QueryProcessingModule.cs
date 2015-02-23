using System.Reflection;

using Autofac;

using Module = Autofac.Module;

namespace Barista.Foundation.Querying
{
    /// <summary>
    /// Autofac module that will scan the provided assembly for query handlers and expose
    /// the <see cref="IQueryProcessor"/> interface for executing queries.
    /// </summary>
    public class QueryProcessingModule : Module
    {
        private readonly Assembly queryhandlersAssembly;

        public QueryProcessingModule(Assembly queryhandlersAssembly)
        {
            this.queryhandlersAssembly = queryhandlersAssembly;
        }

        /// <summary>
        /// Override to add registrations to the container.
        /// </summary>
        /// <remarks>
        /// Note that the ContainerBuilder parameter is unique to this module.
        /// </remarks>
        /// <param name="builder">The builder through which components can be registered.</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterAssemblyTypes(queryhandlersAssembly)
                .AsClosedTypesOf(typeof(IQueryHandler<,>));

            builder.RegisterType<QueryProcessor>().Named<IQueryProcessor>("queryProcessor").SingleInstance();
        }
    }
}
