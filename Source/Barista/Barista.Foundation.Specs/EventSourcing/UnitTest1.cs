using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using Barista.DataAccess;
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
    public class DaCreatedEvent : Event
    {
        public Guid Identity { get; set; }
        public string Name { get; set; }
    }

    public class DaState : AggregateState
    {
        public Guid Identity { get; private set; }

        public string Name { get; private set; }

        public void When(DaCreatedEvent @event)
        {
            Identity = @event.Identity;
            Name = @event.Name;
        }
    }

    [StreamIdPrefix("Barista")]
    public class Da : EventSource<DaState>
    {
        [Identity]
        public Guid Identity { get { return AggregateState.Identity; } }

        public string Name { get { return AggregateState.Name; } }

        public Da()
        {
            
        }

        public Da(string name)
        {
            Apply(new DaCreatedEvent
            {
                Identity = Guid.NewGuid(),
                Name = name
            });
        }
    }

    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
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

            var eventStore = new OptimisticEventStore(persistence);

            var uowFact = new BaristaUnitOfWorkFactory(new Lazy<IStoreEvents>(() => eventStore));

            var uow = uowFact.Create();

            var da = uow.Get<Da>(Guid.Parse("f1d997dd-2ad5-46bc-8f08-c4dc1d03eebf"));

            //uow.Add(da);
            //uow.SubmitChanges();
        }
    }
}
