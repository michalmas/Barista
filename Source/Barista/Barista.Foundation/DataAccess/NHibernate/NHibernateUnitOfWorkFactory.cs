using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;

using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;

using NHibernate;
using NHibernate.Dialect;
using Configuration = NHibernate.Cfg.Configuration;
using Environment = System.Environment;

namespace Barista.Foundation.DataAccess.NHibernate
{
    public abstract class NHibernateUnitOfWorkFactory<TUnitOfWork> : UnitOfWorkFactory<TUnitOfWork>
        where TUnitOfWork : UnitOfWork
    {
        #region Private Definitions

        private static readonly Dictionary<Type, ISessionFactory> sessionFactoryCache = new Dictionary<Type, ISessionFactory>();

        private readonly ConnectionStringSettings connectionSettings;

        #endregion

        protected NHibernateUnitOfWorkFactory(ConnectionStringSettings settings, string schemaName)
        {
            /*connectionSettings = settings;
            persistenceConfigurerFactory = new PersistenceConfigurerFactory(settings, schemaName);

            if (SessionFactory == null)
            {
                FluentConfiguration fluentConfiguration = CreateFluentConfiguration();

                fluentConfiguration.ExposeConfiguration(cfg => persistenceConfigurerFactory.ApplyFluentConfiguration(cfg));

                Configuration configuration = fluentConfiguration.BuildConfiguration();

                SessionFactory = configuration.BuildSessionFactory();
            }*/

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

            SessionFactory = sessFact;
        }

        /*protected FluentConfiguration CreateFluentConfiguration()
        {
            var fluentConfiguration = new NHibernateConfigurationBuilder().BuildFluentConfiguration(ExecutingAssembly,
                connectionSettings.ProviderName);

            fluentConfiguration.Database(DatabaseConfiguration);

            return fluentConfiguration;
        }

        protected virtual IPersistenceConfigurer DatabaseConfiguration
        {
            get { return persistenceConfigurerFactory.Create(); }
        }*/

        protected Assembly ExecutingAssembly
        {
            get { return typeof(TUnitOfWork).Assembly; }
        }

        /// <summary>
        /// Starts a new unit-of-work that tracks changes made the entities obtained through the associated repositories.
        /// </summary>
        /// <returns>
        /// Always creates a new unit-of-work, regardless of an existing one that is associated with the current thread.
        /// </returns>
        protected override TUnitOfWork CreateNew()
        {
            var dataMapper = new NHibernateDataMapper(SessionFactory.OpenSession());

            return CreateUnitOfWorkMappedTo(dataMapper);
        }

        protected abstract TUnitOfWork CreateUnitOfWorkMappedTo(INHibernateDataMapper dataMapper);

        protected ISessionFactory SessionFactory
        {
            get { return sessionFactoryCache.ContainsKey(GetType()) ? sessionFactoryCache[GetType()] : null; }
            set { sessionFactoryCache[GetType()] = value; }
        }
    }
}
