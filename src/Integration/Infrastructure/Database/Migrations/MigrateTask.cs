using System;
using System.Reflection;
using System.Text;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors.SqlServer;
using Vertica.Integration.Infrastructure.Database.NHibernate;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
    public class MigrateTask : Task
    {
        private readonly Lazy<ISessionFactoryProvider> _sessionFactory;
        private readonly IDisposable _loggingDisabler;

        public MigrateTask(Lazy<ISessionFactoryProvider> sessionFactory, ILogger logger)
        {
            _sessionFactory = sessionFactory;

            StringBuilder output;
            MigrationRunner runner = CreateRunner(out output);

            // Baseline has not been applied, so we'll have to disable any logging
            if (!runner.VersionLoader.VersionInfo.HasAppliedMigration(M1_Baseline.VersionNumber))
                _loggingDisabler = logger.Disable();
        }

        public override string Description
        {
            get { return "Runs migrations."; }
        }

        public override string Schedule
        {
            get { return "TBD"; }
        }

        public override void StartTask(Log log, params string[] arguments)
        {
            StringBuilder output;
            MigrationRunner runner = CreateRunner(out output);

            runner.MigrateUp(true);

            if (output.Length > 0)
                log.Message(output.ToString());
        }

        public override void End(EmptyWorkItem workItem, Log log, params string[] arguments)
        {
            if (_loggingDisabler != null)
                _loggingDisabler.Dispose();
        }

        private MigrationRunner CreateRunner(out StringBuilder output)
        {
            var sb = output = new StringBuilder();

            var announcer = new TextWriterAnnouncer(s =>
            {
                if (sb.Length == 0)
                    sb.AppendLine();

                sb.Append(s);
            });

            var factory = new SqlServer2012ProcessorFactory();

            IMigrationProcessor processor = factory.Create(
                _sessionFactory.Value.CurrentSession.Connection.ConnectionString,
                announcer,
                new MigrationOptions());

            var assembly = Assembly.GetExecutingAssembly();

            var context = new RunnerContext(announcer)
            {
                Namespace = typeof (M1_Baseline).Namespace
            };

            return new MigrationRunner(assembly, context, processor);
        }

        private class MigrationOptions : IMigrationProcessorOptions
        {
            public MigrationOptions()
            {
                PreviewOnly = false;
                ProviderSwitches = null;
                Timeout = 60;
            }

            public bool PreviewOnly { get; private set; }
            public string ProviderSwitches { get; private set; }
            public int Timeout { get; private set; }
        }
    }
}
