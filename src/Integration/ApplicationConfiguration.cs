using System;
using System.Collections.Generic;
using System.Linq;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Hosting;
using Vertica.Utilities_v4.Extensions.EnumerableExt;

namespace Vertica.Integration
{
	public class ApplicationConfiguration : IInitializable<IWindsorContainer>, IDisposable
    {
		private readonly ExtensibilityConfiguration _extensibility;

        private readonly List<IWindsorInstaller> _customInstallers;

        private readonly DatabaseConfiguration _database;
        private readonly TasksConfiguration _tasks;
        private readonly LoggingConfiguration _logging;
        private readonly MigrationConfiguration _migration;
        private readonly HostsConfiguration _hosts;

		private Type _runtimeSettings;

		internal ApplicationConfiguration()
        {
			_extensibility = new ExtensibilityConfiguration();

            IgnoreSslErrors = true;

            _customInstallers = new List<IWindsorInstaller>();

            _database = new DatabaseConfiguration(this);
            _tasks = new TasksConfiguration(this);
            _logging = new LoggingConfiguration(this);
            _migration = new MigrationConfiguration(this);
            _hosts = new HostsConfiguration(this);

			_runtimeSettings = typeof (AppConfigRuntimeSettings);
        }

        public bool IgnoreSslErrors { get; set; }

        public ConnectionString DatabaseConnectionString { get { return _database.ConnectionString; } }

        public ApplicationConfiguration AddCustomInstaller(IWindsorInstaller installer)
        {
            return AddCustomInstallers(installer);
        }

        public ApplicationConfiguration AddCustomInstallers(params IWindsorInstaller[] installers)
        {
            if (installers != null)
                _customInstallers.AddRange(installers.SkipNulls());

            return this;
        }

        public ApplicationConfiguration RegisterDependency<T>(T singletonInstance) where T : class
        {
            if (singletonInstance == null) throw new ArgumentNullException("singletonInstance");

            return AddCustomInstaller(new InstanceInstaller<T>(singletonInstance));
        }

        public ApplicationConfiguration Tasks(Action<TasksConfiguration> tasks)
        {
            if (tasks != null)
                tasks(_tasks);

            return this;
        }

        public ApplicationConfiguration Logging(Action<LoggingConfiguration> logger)
        {
            if (logger != null)
                logger(_logging);

            return this;
        }

        public ApplicationConfiguration Database(Action<DatabaseConfiguration> database)
        {
            if (database != null)
                database(_database);

            return this;
        }

        public ApplicationConfiguration Migration(Action<MigrationConfiguration> migration)
        {
            if (migration != null)
                migration(_migration);

            return this;
        }

        public ApplicationConfiguration Hosts(Action<HostsConfiguration> hosts)
        {
            if (hosts != null)
                hosts(_hosts);

            return this;
        }

		public ApplicationConfiguration Extensibility(Action<ExtensibilityConfiguration> extensibility)
		{
			if (extensibility != null)
				extensibility(_extensibility);

			return this;
		}

		public ApplicationConfiguration RuntimeSettings<T>()
			where T : IRuntimeSettings
		{
			_runtimeSettings = typeof (T);

			return this;
		}

        public ApplicationConfiguration Change(Action<ApplicationConfiguration> change)
        {
            if (change != null)
                change(this);

            return this;
        }

		void IInitializable<IWindsorContainer>.Initialize(IWindsorContainer container)
        {
	        container.Register(Component
				.For<IRuntimeSettings>()
				.ImplementedBy(_runtimeSettings));

            container.Install(_customInstallers.ToArray());
        }

        internal IEnumerable<IInitializable<IWindsorContainer>> ContainerInitializations
        {
            get { return _extensibility.ContainerInitializations.Concat(new[] {this}); }
        }

		public void Dispose()
		{
			var exceptions = new List<Exception>();

			_extensibility.Disposers.ForEach(d => 
			{
				try
				{
					d.Dispose();
				}
				catch (Exception ex)
				{
					exceptions.Add(ex);
				}
			});

			if (exceptions.Count > 0)
				throw new AggregateException(exceptions);
		}
    }
}