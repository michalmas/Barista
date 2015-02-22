using System;
using System.Linq;
using Barista.Foundation.DataAccess.Migrations;
using Barista.Foundation.DataAccess.NHibernate;
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

            var stream = eventStore.OpenStream("da1", 0, int.MaxValue);
            stream.Add(new EventMessage
            {
                Body = "baietii"
            });
            stream.CommitChanges(Guid.NewGuid());

            var s = eventStore.GetFrom("da1", 0, int.MaxValue);
            var evs = s.SelectMany(x => x.Events).ToArray();
        }
    }
}
