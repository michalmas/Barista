using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using Autofac;
using Barista.Commands;
using Barista.DataAccess;
using Barista.Foundation.Commanding;
using Barista.Foundation.Common.Extensions;
using Barista.Foundation.DataAccess;
using Barista.Foundation.DataAccess.Migrations;
using Barista.Foundation.DataAccess.NHibernate;
using Barista.Foundation.Domain;
using Barista.Foundation.Domain.Events;
using Barista.Foundation.EventSourcing;
using Barista.Foundation.EventSourcing.Persistence;
using Barista.Foundation.EventSourcing.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FakeItEasy;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate.Dialect;

namespace Barista.Foundation.Specs.EventSourcing
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            MigrationsRunner.Migrate(0);
            MigrationsRunner.Migrate();

            var sessFact = Fluently
                .Configure()
                .Mappings(m =>
                {
                    foreach (var assemblyContainingMappings in AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.Contains("Barista")))
                    {
                        m.FluentMappings.AddFromAssembly(assemblyContainingMappings/*, mappingAssembliesFilterPredicate*/);
                    }
                })
                .Database(MsSqlConfiguration.MsSql2008
                .Dialect<MsSql2008Dialect>()
                .ConnectionString("Data Source=.\\SQLEXPRESS;Initial Catalog=Barista;Trusted_Connection=yes;"))
                .BuildSessionFactory();

            var persistence = new DataMapperPersistenceEngine(new NHibernateDataMapper(sessFact.OpenSession()), new JsonSerializer(), new Sha1StreamIdHasher());

            

            var containerBuilder = new ContainerBuilder();

            containerBuilder
                .RegisterAssemblyTypes(typeof(BaristaProjectionsUnitOfWork).Assembly)
                .Where(type => type.IsSubclassOfRawGeneric(typeof(IDenormalize<>)))
                .AsImplementedInterfaces().InstancePerDependency();

            var container = containerBuilder.Build();

            Func<UnitOfWorkOption, BaristaProjectionsUnitOfWork> f = null;

            var eventStore = new OptimisticEventStore(persistence, new EventStoreCommitDispatcher<BaristaProjectionsUnitOfWork>((x) => f(x), new DenormalizerProvider(container)));

            var uowFact = new BaristaUnitOfWorkFactory(new Lazy<IStoreEvents>(() => eventStore));
            var pUowFact = new BaristaProjectionsUnitOfWorkFactory();

            f = pUowFact.Create;

            containerBuilder = new ContainerBuilder();

            containerBuilder.Register(x => uowFact.Create());
            containerBuilder.Register(x => pUowFact.Create());

            containerBuilder
                .RegisterAssemblyTypes(typeof(CommandHandlerRegistrations).Assembly)
                .AsClosedTypesOf(typeof(IHandleCommand<>));

            containerBuilder
                .RegisterAssemblyTypes(typeof(CommandHandlerRegistrations).Assembly)
                .AsClosedTypesOf(typeof(ICreateAggregate<,>));

            containerBuilder.RegisterModule(new CommandingModule());

            containerBuilder.Update(container);

            using (var uow = uowFact.Create())
            {
                var commandService = new BaristaCommandService(container, uowFact.Create);

                var id = Guid.NewGuid();

                commandService.Execute(new CreateOrderCommand
                {
                    Identity = id,
                    BaristaName = "Cristian"
                });

                commandService.Execute(new AddOrderItemCommand
                {
                    OrderIdentity = id,

                    ProductName = "Beer",
                    Quantity = 100
                });
            }
        }
    }
}
