using System;
using System.Collections.Generic;
using System.Linq;

using Autofac;

using Barista.Foundation.Common.Extensions;
using Barista.Foundation.DataAccess;
using Barista.Foundation.Hosting;

namespace Barista.Foundation.Commanding
{
    /// <summary>
    /// Autofac bootstrapping module that initializes and sets up all the command handler pipelines.
    /// </summary>
    public class CommandingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(AggregateRetrievalAdapter<,>));
            //builder.RegisterGeneric(typeof(PolicyApplyingDecorator<,>));
            //builder.RegisterGeneric(typeof(OwnershipCheckingDecorator<,>));
            builder.RegisterGeneric(typeof(MappedCommandAdapter<,>));

            RegisterHandlers(builder);
        }

        private void RegisterHandlers(ContainerBuilder builder)
        {
            var commandHandlerRegisterers = GetTypeOfCommandHandlerRegisterers();

            foreach (var registrerType in commandHandlerRegisterers)
            {
                RegisterHandlers(registrerType, builder);
            }
        }

        private IEnumerable<Type> GetTypeOfCommandHandlerRegisterers()
        {
            return from assembly in new BaristaAssemblyFinder().GetAll()
                   from type in assembly.GetLoadableTypes()
                   where !type.IsAbstract
                   from interfaceType in type.GetInterfaces()
                   where interfaceType.IsGenericType
                   where interfaceType.GetGenericTypeDefinition() == typeof(IRegisterCommandHandlers<>)
                   select type;
        }

        private static void RegisterHandlers(Type registrerType, ContainerBuilder builder)
        {
            Type unitOfWorkType = GetTypeOfUnitWorkUsedByRegisterer(registrerType);

            var handlers = CreateRegistry(builder, unitOfWorkType);

            CreateAndInvokeRegistrer(registrerType, handlers);
        }

        private static Type GetTypeOfUnitWorkUsedByRegisterer(Type commandHandlerRegistrerType)
        {
            return commandHandlerRegistrerType.GetInterfaces().Single().GetGenericArguments().Single();
        }

        private static object CreateRegistry(ContainerBuilder builder, Type unitOfWorkType)
        {
            Type handlerType = typeof(HandlerRegistry<>).MakeGenericType(unitOfWorkType);

            return Activator.CreateInstance(handlerType, builder);
        }

        private static void CreateAndInvokeRegistrer(Type registrerType, object handlers)
        {
            object registerer = Activator.CreateInstance(registrerType);
            string methodName = StaticReflection
                .GetMemberName<IRegisterCommandHandlers<DomainUnitOfWork>>(o => o.Register(null));

            registrerType.GetMethod(methodName).Invoke(registerer, new[] { handlers });
        }
    }
}
