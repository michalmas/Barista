using System;
using System.Linq;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;

namespace Barista.Foundation.DataAccess.Migrations
{
    internal static class Runner
    {
        public class MigrationOptions : IMigrationProcessorOptions
        {
            public bool PreviewOnly { get; set; }
            public string ProviderSwitches { get; set; }
            public int Timeout { get; set; }
        }

        public static void MigrateToLatest(string connectionString, long? version)
        {
            var announcer = new NullAnnouncer();
            //var announcer = new TextWriterAnnouncer(s => Console.WriteLine(s));

            var asss = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.Contains("Barista")).ToArray();


            foreach (var assembly in asss)
            {
                var migrationContext = new RunnerContext(announcer)
                {
                    //Namespace = "Barista.Foundation.DataAccess.Migrations"
                };
                var options = new MigrationOptions { PreviewOnly = false, Timeout = 60 };
                var factory = new FluentMigrator.Runner.Processors.SqlServer.SqlServer2008ProcessorFactory();
                var processor = factory.Create(connectionString, announcer, options);
                var runner = new MigrationRunner(assembly, migrationContext, processor);
                //runner.Conventions.TypeIsMigration = type => type.BaseType == typeof(DbMigration);

                if (version.HasValue)
                {
                    var ver = runner.VersionLoader.VersionInfo.Latest();

                    if (version.Value > ver)
                    {
                        runner.MigrateUp(version.Value, true);
                    }
                    else
                    {
                        runner.MigrateDown(version.Value, true);
                    }
                }
                else
                {
                    runner.MigrateUp(true);
                }
            }
        }
    }

    public class MigrationsRunner
    {
        public static void Migrate(long? version = null)
        {
            Runner.MigrateToLatest("Data Source=.\\SQLEXPRESS;Initial Catalog=Barista;Trusted_Connection=yes;", version);
        }
    }
}
